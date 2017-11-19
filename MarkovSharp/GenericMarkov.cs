using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MarkovSharp.TokenisationStrategies;
using MarkovSharp.Models;
using Newtonsoft.Json;

namespace MarkovSharp
{
    /// <summary>
    /// This class contains core functionality of the generic Markov model.
    /// Shouldn't be used directly, instead, extend GenericMarkov 
    /// and implement the IMarkovModel interface - this will allow you to 
    /// define overrides for SplitTokens and RebuildPhrase, which is generally
    /// all that should be needed for implementation of a new model type.
    /// </summary>
    /// <typeparam name="TPhrase"></typeparam>
    /// <typeparam name="TGram"></typeparam>
    public abstract class GenericMarkov<TPhrase, TGram> : IMarkovStrategy<TPhrase, TGram>
    {
        /// <summary>Initializes a new instance of the <see cref="GenericMarkov{TPhrase, TGram}"/> class.</summary>
        /// <param name="level">The level.</param>
        /// <exception cref="ArgumentException">Invalid value: level must be a positive integer - level</exception>
        protected GenericMarkov(int level = 2)
        {
            if (level < 1)
            {
                throw new ArgumentException("Invalid value: level must be a positive integer", nameof(level));
            }

            Model = new ConcurrentDictionary<SourceGrams<TGram>, List<TGram>>();
            SourceLines = new List<TPhrase>();
            Level = level;
            EnsureUniqueWalk = false;
        }


        /// <summary>// Dictionary containing the model data. The key is the N number of
        /// previous words and value is a list of possible outcomes, given that key</summary>
        /// <value>The model.</value>
        [JsonIgnore]
        public ConcurrentDictionary<SourceGrams<TGram>, List<TGram>> Model { get; set; }

        public List<TPhrase> SourceLines { get; }

        /// <summary>
        /// Defines how to split the phrase to ngrams
        /// </summary>
        /// <param name="phrase"></param>
        /// <returns></returns>
        public abstract IEnumerable<TGram> SplitTokens(TPhrase phrase);

        /// <summary>
        /// Defines how to join ngrams back together to form a phrase
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public abstract TPhrase RebuildPhrase(IEnumerable<TGram> tokens);

        public abstract TGram GetTerminatorGram();

        public abstract TGram GetPrepadGram();

        /// <summary>
        /// Set to true to ensure that all lines generated are different and not same as the training data.
        /// This might not return as many lines as requested if genreation is exhausted and finds no new unique values.
        /// </summary>
        public bool EnsureUniqueWalk { get; set; }

        /// <summary>The number of previous states for the model 
        /// to consider when suggesting the next state</summary>
        /// <value>The level.</value>
        public int Level { get; private set; }
        
        public void Learn(IEnumerable<TPhrase> phrases, bool ignoreAlreadyLearnt = true)
        {
            var source = phrases.ToArray();
            if (ignoreAlreadyLearnt)
            {
                var newTerms = source.Where(s => !SourceLines.Contains(s));

                Console.WriteLine($"Learning {newTerms.Count()} lines");
                // For every sentence which hasnt already been learnt, learn it
                Parallel.ForEach(source, Learn);
            }
            else
            {
                Console.WriteLine($"Learning {source.Length} lines");
                // For every sentence, learn it
                Parallel.ForEach(source, Learn);
            }
        }

        public void Learn(TPhrase phrase)
        {
            Console.WriteLine($"Learning phrase: '{phrase}'");
            if (phrase == null || phrase.Equals(default(TPhrase)))
            {
                return;
            }

            // Ignore particularly short sentences
            if (SplitTokens(phrase).Count() < Level)
            {
                Console.WriteLine($"Phrase {phrase} too short - skipped");
                return;
            }

            // Add it to the source lines so we can ignore it 
            // when learning in future
            if (!SourceLines.Contains(phrase))
            {
                Console.WriteLine($"Adding phrase {phrase} to source lines");
                SourceLines.Add(phrase);
            }
            
            // Split the sentence to an array of words
            var tokens = SplitTokens(phrase).ToArray();

            LearnTokens(tokens);
            
            var lastCol = new List<TGram>();
            for (var j = Level; j > 0; j--)
            {
                TGram previous;
                try
                {
                    previous = tokens[tokens.Length - j];
                    Console.WriteLine($"Adding TGram ({typeof(TGram)}) {previous} to lastCol");
                    lastCol.Add(previous);
                }
                catch (IndexOutOfRangeException e)
                {
                    Console.WriteLine($"Caught an exception: {e}");
                    previous = GetPrepadGram();
                    lastCol.Add(previous);
                }
            }

            Console.WriteLine($"Reached final key for phrase {phrase}");
            var finalKey = new SourceGrams<TGram>(lastCol.ToArray());
            AddOrCreate(finalKey, GetTerminatorGram());
        }

        /// <summary>
        /// Iterate over a list of TGrams and store each of them in the model at a composite key genreated from its prior [Level] number of TGrams
        /// </summary>
        /// <param name="tokens"></param>
        private void LearnTokens(IReadOnlyList<TGram> tokens)
        {
            for (var i = 0; i < tokens.Count; i++)
            {
                var current = tokens[i];
                var previousCol = new List<TGram>();
                
                // From the current token's index, get hold of the previous [Level] number of tokens that came before it
                for (var j = Level; j > 0; j--)
                {
                    TGram previous;
                    try
                    {
                        // this case addresses when we are at a token index less then the value of [Level], 
                        // and we effectively would be looking at tokens before the beginning phrase
                        if (i - j < 0)
                        {
                            previousCol.Add(GetPrepadGram());
                        }
                        else 
                        {
                            previous = tokens[i - j];
                            previousCol.Add(previous);
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        previous = GetPrepadGram();
                        previousCol.Add(previous);
                    }
                }

                // create the composite key based on previous tokens
                var key = new SourceGrams<TGram>(previousCol.ToArray());

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

            Console.WriteLine($"Retraining model as level {newLevel}");
            Level = newLevel;

            // Empty the model so it can be rebuilt
            Model = new ConcurrentDictionary<SourceGrams<TGram>, List<TGram>>();

            Learn(SourceLines, false);
        }

        private readonly object _lockObj = new object();

        /// <summary>
        /// Add a TGram to the markov models store with a composite key of the previous [Level] number of TGrams
        /// </summary>
        /// <param name="key">The composite key under which to add the TGram value</param>
        /// <param name="value">The value to add to the store</param>
        private void AddOrCreate(SourceGrams<TGram> key, TGram value)
        {
            lock (_lockObj)
            {
                if (!Model.ContainsKey(key))
                {
                    Model.TryAdd(key, new List<TGram> {value});
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
                seed = RebuildPhrase(new List<TGram> {GetPrepadGram()});
            }

            Console.WriteLine($"Walking to return {lines} phrases from {Model.Count} states");
            if (lines < 1)
            {
                throw new ArgumentException("Invalid argument - line count for walk must be a positive integer", nameof(lines));
            }

            var sentences = new List<TPhrase>();

            //for (var z = 0; z < lines; z++)k
            var genCount = 0;
            var created = 0;
            while (created < lines)
            {
                if (genCount == lines*10)
                {
                    Console.WriteLine($"Breaking out of walk early - {genCount} generations did not produce {lines} distinct lines ({sentences.Count} were created)");
                    break;
                }
                var result = WalkLine(seed);
                if ((!EnsureUniqueWalk || !SourceLines.Contains(result)) && (!EnsureUniqueWalk || !sentences.Contains(result)))
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
            var built = new List<TGram>();

            // Allocate a queue to act as the memory, which is n 
            // levels deep of previous words that were used
            var q = new Queue<TGram>(arraySeed);

            // If the start of the generated text has been seeded,
            // append that before generating the rest
            if (!seed.Equals(GetPrepadGram()))
            {
                built.AddRange(SplitTokens(seed));
            }

            while (built.Count < 1500)
            {
                // Choose a new word to add from the model
                //Console.WriteLine($"In Walkline loop: builtcount = {built.Count}");
                var key = new SourceGrams<TGram>(q.Cast<TGram>().ToArray());
                if (Model.ContainsKey(key))
                {
                    var chosen = Model[key].OrderBy(x => Guid.NewGuid()).FirstOrDefault();

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

        /// <summary>Returns any viable options for the next word based on
        /// what was provided as input, based on the trained model.</summary>
        /// <param name="input">The input.</param>
        public List<TGram> GetMatches(TPhrase input)
        {
            var inputArray = SplitTokens(input).ToArray();
            if (inputArray.Length > Level)
            {
                inputArray = inputArray.Skip(inputArray.Length - Level).ToArray();
            }
            else if (inputArray.Length < Level)
            {
                inputArray = PadArrayLow(inputArray);
            }

            var key = new SourceGrams<TGram>(inputArray);
            var chosen = Model[key];
            return chosen;
        }

        // Pad out an array with empty strings from bottom up
        // Used when providing a seed sentence or word for generation
        private TGram[] PadArrayLow(TGram[] input)
        {
            if (input == null)
            {
                input = new List<TGram>().ToArray();
            }

            var splitCount = input.Length;
            if (splitCount > Level)
            {
                input = input.Skip(splitCount - Level).Take(Level).ToArray();
            }

            var p = new TGram[Level];
            var j = 0;
            for (var i = Level - input.Length; i < Level; i++)
            {
                p[i] = input[j];
                j++;
            }
            for (var i = Level - input.Length; i > 0; i--)
            {
                p[i - 1] = GetPrepadGram();
            }

            return p;
        }

        /// <summary>
        /// Save the model to file for use later
        /// </summary>
        /// <param name="file">The path to a file to store the model in</param>
        public void Save(string file)
        {
            Console.WriteLine($"Saving model with {Model.Count} model values");
            var modelJson = JsonConvert.SerializeObject(this);
            File.WriteAllText(file, modelJson);
            Console.WriteLine("Model saved successfully");
        }

        /// <summary>
        /// Load a model which has been saved
        /// </summary>
        /// <typeparam name="T">The type of markov model to load the data as</typeparam>
        /// <param name="file">The path to a file containing saved model data</param>
        /// <param name="level">The level to apply to the loaded model (model will be trained on load)</param>
        /// <returns></returns>
        public T Load<T>(string file, int level = 1) where T : IMarkovStrategy<TPhrase, TGram>
        {
            Console.WriteLine($"Loading model from {file}");
            var model = JsonConvert.DeserializeObject<T>(File.ReadAllText(file));

            Console.WriteLine("Model data loaded successfully");
            Console.WriteLine("Assigning new model parameters");

            model.Retrain(level);

            return model;
        }
    }
}
