using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using MarkovSharp.Components;
using MarkovSharp.TokenisationStrategies;
using Newtonsoft.Json;
using MarkovSharp.Models;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]

namespace MarkovSharp
{
    /// <summary>
    /// This class contains core functionality of the generic Markov model.
    /// Shouldn't be used directly, instead, extend GenericMarkov 
    /// and implement the IMarkovModel interface - this will allow you to 
    /// define overrides for SplitTokens and RebuildPhrase, which is generally
    /// all that should be needed for implementation of a new model type.
    /// </summary>
    /// <typeparam name="TPhrase">The type representing the entire phrase</typeparam>
    /// <typeparam name="TUnigram">The type representing a unigram, or state</typeparam>
    public abstract class GenericMarkov<TPhrase, TUnigram> : IMarkovStrategy<TPhrase, TUnigram>
    {
        private readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected GenericMarkov(int level = 2)
        {
            if (level < 1)
            {
                throw new ArgumentException("Invalid value: level must be a positive integer", nameof(level));
            }

            Chain = new MarkovChain<TUnigram>();
            UnigramSelector = new WeightedRandomUnigramSelector<TUnigram>();
            SourcePhrases = new List<TPhrase>();
            Level = level;
            EnsureUniqueWalk = false;
        }

        public Type UnigramType => typeof(TUnigram);
        public Type PhraseType => typeof(TPhrase);
        
        // Chain containing the model data. The key is the N number of
        // previous words and value is a list of possible outcomes, given that key
        public MarkovChain<TUnigram> Chain { get; set; }

        /// <summary>
        /// Used for defining a strategy to select the next value when calling Walk()
        /// Set this to something else for different behaviours
        /// </summary>
        public IUnigramSelector<TUnigram> UnigramSelector { get; set; }

        public List<TPhrase> SourcePhrases { get; set; }

        /// <summary>
        /// Defines how to split the phrase to ngrams
        /// </summary>
        /// <param name="phrase"></param>
        /// <returns></returns>
        public abstract IEnumerable<TUnigram> SplitTokens(TPhrase phrase);

        /// <summary>
        /// Defines how to join ngrams back together to form a phrase
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public abstract TPhrase RebuildPhrase(IEnumerable<TUnigram> tokens);

        public abstract TUnigram GetTerminatorUnigram();

        public abstract TUnigram GetPrepadUnigram();

        /// <summary>
        /// Set to true to ensure that all lines generated are different and not same as the training data.
        /// This might not return as many lines as requested if genreation is exhausted and finds no new unique values.
        /// </summary>
        public bool EnsureUniqueWalk { get; set; }

        // The number of previous states for the model to to consider when 
        //suggesting the next state
        public int Level { get; private set; }
        
        public void Learn(IEnumerable<TPhrase> phrases, bool ignoreAlreadyLearnt = true)
        {
            if (ignoreAlreadyLearnt)
            {
                var newTerms = phrases.Where(s => !SourcePhrases.Contains(s));

                Logger.Info($"Learning {newTerms.Count()} lines");
                // For every sentence which hasnt already been learnt, learn it
                Parallel.ForEach(phrases, Learn);
            }
            else
            {
                Logger.Info($"Learning {phrases.Count()} lines");
                // For every sentence, learn it
                Parallel.ForEach(phrases, Learn);
            }
        }

        public void Learn(TPhrase phrase)
        {
            Logger.Info($"Learning phrase: '{phrase}'");
            if (phrase == null || phrase.Equals(default(TPhrase)))
            {
                return;
            }

            // Ignore particularly short phrases
            if (SplitTokens(phrase).Count() < Level)
            {
                Logger.Info($"Phrase {phrase} too short - skipped");
                return;
            }

            // Add it to the source lines so we can ignore it 
            // when learning in future
            if (!SourcePhrases.Contains(phrase))
            {
                Logger.Debug($"Adding phrase {phrase} to source lines");
                SourcePhrases.Add(phrase);
            }
            
            // Split the sentence to an array of words
            var tokens = SplitTokens(phrase).ToArray();

            LearnTokens(tokens);
            
            var lastCol = new List<TUnigram>();
            for (var j = Level; j > 0; j--)
            {
                TUnigram previous;
                try
                {
                    previous = tokens[tokens.Length - j];
                    Logger.Debug($"Adding TGram ({typeof(TUnigram)}) {previous} to lastCol");
                    lastCol.Add(previous);
                }
                catch (IndexOutOfRangeException e)
                {
                    Logger.Warn($"Caught an exception: {e}");
                    previous = GetPrepadUnigram();
                    lastCol.Add(previous);
                }
            }

            Logger.Debug($"Reached final key for phrase {phrase}");
            var finalKey = new NgramContainer<TUnigram>(lastCol.ToArray());
            Chain.AddOrCreate(finalKey, GetTerminatorUnigram());
        }

        /// <summary>
        /// Iterate over a list of TGrams and store each of them in the model at a composite key genreated from its prior [Level] number of TGrams
        /// </summary>
        /// <param name="tokens"></param>
        private void LearnTokens(IReadOnlyList<TUnigram> tokens)
        {
            for (var i = 0; i < tokens.Count; i++)
            {
                var current = tokens[i];
                var previousCol = new List<TUnigram>();
                
                // From the current token's index, get hold of the previous [Level] number of tokens that came before it
                for (var j = Level; j > 0; j--)
                {
                    TUnigram previous;
                    try
                    {
                        // this case addresses when we are at a token index less then the value of [Level], 
                        // and we effectively would be looking at tokens before the beginning phrase
                        if (i - j < 0)
                        {
                            previousCol.Add(GetPrepadUnigram());
                        }
                        else 
                        {
                            previous = tokens[i - j];
                            previousCol.Add(previous);
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        previous = GetPrepadUnigram();
                        previousCol.Add(previous);
                    }
                }

                // create the composite key based on previous tokens
                var key = new NgramContainer<TUnigram>(previousCol.ToArray());

                // add the current token to the markov model at the composite key
                Chain.AddOrCreate(key, current);
            }
        }

        /// <summary>
        /// Retrain an existing trained model instance to a different 'level'
        /// </summary>
        /// <param name="newLevel"></param>
        public void Retrain(int newLevel)
        {
            if (newLevel < 1)
            {
                throw new ArgumentException("Invalid argument - retrain level must be a positive integer", nameof(newLevel));
            }

            Logger.Info($"Retraining model as level {newLevel}");
            Level = newLevel;

            // Empty the model so it can be rebuilt
            Chain = new MarkovChain<TUnigram>();

            Learn(SourcePhrases, false);
        }

        

        /// <summary>
        /// Generate a collection of phrase output data based on the current model
        /// </summary>
        /// <param name="lines">The number of phrases to emit</param>
        /// <param name="seed">Optionally provide the start of the phrase to generate from</param>
        /// <returns></returns>
        public IEnumerable<TPhrase> Walk(int lines = 1, TPhrase seed = default(TPhrase))
        {
            if (seed == null)
            {
                seed = RebuildPhrase(new List<TUnigram>() {GetPrepadUnigram()});
            }

            Logger.Info($"Walking to return {lines} phrases from {Chain.Count} states");
            if (lines < 1)
            {
                throw new ArgumentException("Invalid argument - line count for walk must be a positive integer", nameof(lines));
            }

            var sentences = new List<TPhrase>();
            
            int genCount = 0;
            int created = 0;
            while (created < lines)
            {
                if (genCount == lines*10)
                {
                    Logger.Info($"Breaking out of walk early - {genCount} generations did not produce {lines} distinct lines ({sentences.Count} were created)");
                    break;
                }
                var result = WalkLine(seed);
                if ((!EnsureUniqueWalk || !SourcePhrases.Contains(result)) && (!EnsureUniqueWalk || !sentences.Contains(result)))
                {
                    sentences.Add(result);
                    created++;
                    yield return result;
                }
                genCount++;
            }
        }

        /// <summary>
        /// Generate a single phrase of output data based on the current model
        /// </summary>
        /// <param name="seed">Optionally provide the start of the phrase to generate from</param>
        /// <returns></returns>
        private TPhrase WalkLine(TPhrase seed)
        {
            var paddedSeed = PadArrayLow(SplitTokens(seed)?.ToArray());
            var built = new List<TUnigram>();

            // Allocate a queue to act as the memory, which is n 
            // levels deep of previous words that were used
            var q = new Queue(paddedSeed);

            // If the start of the generated text has been seeded,
            // append that before generating the rest
            if (!seed.Equals(GetPrepadUnigram()))
            {
                built.AddRange(SplitTokens(seed));
            }

            while (built.Count < 1500)
            {
                // Choose a new token to add from the model
                var key = new NgramContainer<TUnigram>(q.Cast<TUnigram>().ToArray());
                if (Chain.Contains(key))
                {
                    TUnigram chosen;

                    if (built.Count == 0)
                    {
                        chosen = new UnweightedRandomUnigramSelector<TUnigram>().SelectUnigram(Chain.GetValuesForKey(key));
                    }
                    else
                    {
                        chosen = UnigramSelector.SelectUnigram(Chain.GetValuesForKey(key));
                    }
                    
                    q.Dequeue();
                    q.Enqueue(chosen);
                    built.Add(chosen);
                }
                else
                {
                    break;
                }
            }

            return RebuildPhrase(built);
        }

        // Returns any viable options for the next word based on
        // what was provided as input, based on the trained model.
        public List<TUnigram> GetMatches(TPhrase input)
        {
            var inputArray = SplitTokens(input).ToArray();
            if (inputArray.Count() > Level)
            {
                inputArray = inputArray.Skip(inputArray.Length - Level).ToArray();
            }
            else if (inputArray.Count() < Level)
            {
                inputArray = PadArrayLow(inputArray);
            }

            var key = new NgramContainer<TUnigram>(inputArray);
            var chosen = new List<TUnigram>();

            try
            {
                chosen = Chain.GetValuesForKey(key);
            }
            catch (KeyNotFoundException e)
            {
                throw new KeyNotFoundException("No sequence could be found that matched the provided input", e);
            }

            return chosen;
        }

        // Pad out an array with empty strings from bottom up
        // Used when providing a seed sentence or word for generation
        private TUnigram[] PadArrayLow(TUnigram[] input)
        {
            if (input == null)
            {
                input = new List<TUnigram>().ToArray();
            }

            var splitCount = input.Length;
            if (splitCount > Level)
            {
                input = input.Skip(splitCount - Level).Take(Level).ToArray();
            }

            var p = new TUnigram[Level];
            var j = 0;
            for (var i = (Level - input.Length); i < (Level); i++)
            {
                p[i] = input[j];
                j++;
            }
            for (var i = Level - input.Length; i > 0; i--)
            {
                p[i - 1] = GetPrepadUnigram();
            }

            return p;
        }

        public IEnumerable<StateStatistic<TUnigram>> GetStatistics()
        {
            return Chain.GetStatistics();
        }

        public virtual ChainPhraseProbability<TPhrase> GetFit(TPhrase testData)
        {
            var testUnigrams = SplitTokens(testData).ToArray();

            var results = new Dictionary<TPhrase, bool>();

            for (int i = 0; i < testUnigrams.Count(); i++)
            {
                var testCase = new List<TUnigram>();

                for (int j = 0; j < Level; j++)
                {
                    var index = i + j;
                    if (index >= testUnigrams.Count())
                    {
                        break;
                    }
                    
                    testCase.Add(testUnigrams[i + j]);
                }

                if (testCase.Count() == Level)
                {
                    var testPhrase = RebuildPhrase(testCase);

                    try
                    {
                        var testResults = GetMatches(testPhrase);
                        if (testResults.Any())
                        {
                            results[testPhrase] = true;
                        }
                        else
                        {
                            results[testPhrase] = false;
                        }
                    }
                    catch (KeyNotFoundException e)
                    {
                        results[testPhrase] = false;
                    }
                }
            }

            var matchCount = results.Count(a => a.Value);
            var probability = Math.Round((double)matchCount / results.Count * 100, 2);

            return new ChainPhraseProbability<TPhrase>
            {
                Probability = probability,
                Raw = results
            };
        }
    }
}
