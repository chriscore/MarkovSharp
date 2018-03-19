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

            Model = new ConcurrentDictionary<SourceGrams<TUnigram>, List<TUnigram>>();
            UnigramSelector = new WeightedRandomUnigramSelector<TUnigram>();
            SourcePhrases = new List<TPhrase>();
            Level = level;
            EnsureUniqueWalk = false;
        }

        // Dictionary containing the model data. The key is the N number of
        // previous words and value is a list of possible outcomes, given that key
        [JsonIgnore]
        public ConcurrentDictionary<SourceGrams<TUnigram>, List<TUnigram>> Model { get; set; }
        
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
            var finalKey = new SourceGrams<TUnigram>(lastCol.ToArray());
            AddOrCreate(finalKey, GetTerminatorUnigram());
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
                var key = new SourceGrams<TUnigram>(previousCol.ToArray());

                // add the current token to the markov model at the composite key
                AddOrCreate(key, current);
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
            Model = new ConcurrentDictionary<SourceGrams<TUnigram>, List<TUnigram>>();

            Learn(SourcePhrases, false);
        }

        private object lockObj = new object();

        /// <summary>
        /// Add a TGram to the markov models store with a composite key of the previous [Level] number of TGrams
        /// </summary>
        /// <param name="key">The composite key under which to add the TGram value</param>
        /// <param name="value">The value to add to the store</param>
        private void AddOrCreate(SourceGrams<TUnigram> key, TUnigram value)
        {
            lock (lockObj)
            {
                if (!Model.ContainsKey(key))
                {
                    Model.TryAdd(key, new List<TUnigram> {value});
                }
                else
                {
                    Model[key].Add(value);
                }
            }
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

            Logger.Info($"Walking to return {lines} phrases from {Model.Count} states");
            if (lines < 1)
            {
                throw new ArgumentException("Invalid argument - line count for walk must be a positive integer", nameof(lines));
            }

            var sentences = new List<TPhrase>();

            //for (var z = 0; z < lines; z++)k
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
            var arraySeed = PadArrayLow(SplitTokens(seed)?.ToArray());
            List<TUnigram> built = new List<TUnigram>();

            // Allocate a queue to act as the memory, which is n 
            // levels deep of previous words that were used
            var q = new Queue(arraySeed);

            // If the start of the generated text has been seeded,
            // append that before generating the rest
            if (!seed.Equals(GetPrepadUnigram()))
            {
                built.AddRange(SplitTokens(seed));
            }

            while (built.Count < 1500)
            {
                // Choose a new word to add from the model
                //Logger.Info($"In Walkline loop: builtcount = {built.Count}");
                var key = new SourceGrams<TUnigram>(q.Cast<TUnigram>().ToArray());
                if (Model.ContainsKey(key))
                {
                    var chosen = UnigramSelector.SelectUnigram(Model[key]);

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

            var key = new SourceGrams<TUnigram>(inputArray);
            var chosen = Model[key];
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
            int j = 0;
            for (int i = (Level - input.Length); i < (Level); i++)
            {
                p[i] = input[j];
                j++;
            }
            for (int i = Level - input.Length; i > 0; i--)
            {
                p[i - 1] = GetPrepadUnigram();
            }

            return p;
        }

        public IEnumerable<StateStatistic<TUnigram>> GetStatistics()
        {
            var stats = Model.Keys.Select(a => new StateStatistic<TUnigram>(a, Model[a]))
                .OrderByDescending(a => a.Next.Sum(x => x.Count));

            return stats;
        }
    }
}
