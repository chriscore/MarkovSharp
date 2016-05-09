using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace MarkovSharp
{
    public class Markov
    {
        public Markov(int level = 2)
        {
            if (level < 1)
            {
                throw new ArgumentException("Invalid value: level must be a positive integer", nameof(level));
            }

            Model = new Dictionary<SourceWords, List<string>>();
            SourceLines = new List<string>();
            Level = level;
        }

        // Dictionary containing the model data. The key is the N number of
        // previous words and value is a list of possible outcomes, given that key
        [JsonIgnore]
        public Dictionary<SourceWords, List<string>> Model { get; set; }

        public List<string> SourceLines { get; set; }

        // The number of previous states for the model to to consider when 
        //suggesting the next state
        public int Level { get; private set; }

        public void Learn(IEnumerable<string> sentences, bool ignoreAlreadyLearnt = true)
        {
            if (ignoreAlreadyLearnt)
            {
                var newTerms = sentences.Where(s => !SourceLines.Contains(s.Trim()));

                Console.WriteLine("Learning {0} lines", newTerms.Count());
                // For every sentence which hasnt already been learnt, learn it
                foreach (var sentence in newTerms)
                {
                    Learn(sentence.Trim());
                }
            }
            else
            {
                Console.WriteLine("Learning {0} lines", sentences.Count());
                // For every sentence, learn it
                foreach (var sentence in sentences)
                {
                    Learn(sentence.Trim());
                }
            }
        }

        public void Learn(string sentence)
        {
            if (string.IsNullOrWhiteSpace(sentence))
            {
                return;
            }

            // Ignore particularly short sentences
            if (sentence.Split(' ').Length < Level)
            {
                return;
            }

            // Add it to the source lines so we can ignore it 
            // when learning in future
            if (!SourceLines.Contains(sentence))
            {
                SourceLines.Add(sentence);
            }
            
            // Split the sentence to an array of words
            var tokens = sentence
                .Split(' ')
                .ToArray();


            for (var i = 0; i < tokens.Length; i++)
            {
                var current = tokens[i];

                var previousCol = new List<string>();
                for (var j = Level; j > 0; j--)
                {
                    string previous;
                    try
                    {
                        if (i - j < 0)
                        {
                            previousCol.Add("");
                        }
                        else
                        {
                            previous = tokens[i - j];
                            previousCol.Add(previous);
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        previous = "";
                        previousCol.Add(previous);
                    }
                }

                var key = new SourceWords(previousCol.ToArray());
                AddOrCreate(key, current);
            }
            
            var lastCol = new List<string>();
            for (var j = Level; j > 0; j--)
            {
                string previous;
                try
                {
                    previous = tokens[tokens.Length - j];
                    lastCol.Add(previous);
                }
                catch (IndexOutOfRangeException)
                {
                    previous = "";
                    lastCol.Add(previous);
                }
            }

            var finalKey = new SourceWords(lastCol.ToArray());
            AddOrCreate(finalKey, null);
        }

        public void Retrain(int newLevel)
        {
            Console.WriteLine("Retraining model as level {0}", newLevel);
            Level = newLevel;

            // Empty the model so it can be rebuilt
            Model = new Dictionary<SourceWords, List<string>>();

            Learn(SourceLines, false);
        }

        private void AddOrCreate(SourceWords key, string value)
        {
            if (!Model.ContainsKey(key))
            {
                Model.Add(key, new List<string> { value });
            }
            else
            {
                Model[key].Add(value);
            }
        }

        public IEnumerable<string> Walk(int lines = 1, string seed = "")
        {
            var sentences = new List<string>();

            for (var z = 0; z < lines; z++)
            {
                sentences.Add(WalkLine(seed));
            }

            return sentences;
        }

        private string WalkLine(string seed)
        {
            var arraySeed = PadArrayLow(seed.Split(' '));

            StringBuilder sb = new StringBuilder();

            // Allocate a queue to act as the memory, which is n 
            // levels deep of previous words that were used
            var q = new Queue(arraySeed);

            // If the start of the generated text has been seeded,
            // append that before generating the rest
            if (!string.IsNullOrEmpty(seed))
            { sb.Append(seed + " "); }

            while (true)
            {
                // Choose a new word to add from the model
                var key = new SourceWords(q.Cast<string>().ToArray());
                if (Model.ContainsKey(key))
                {
                    var chosen = Model[key].OrderBy(x => Guid.NewGuid()).FirstOrDefault();

                    q.Dequeue();
                    q.Enqueue(chosen);

                    sb.Append(chosen + " ");
                }
                else
                {
                    break;
                }
            }

            return sb.ToString().Trim();
        }

        // Returns any viable options for the next word based on
        // what was provided as input, based on the trained model.
        public List<string> GetMatches(string input)
        {
            var inputArray = input.Trim().Split(' ');
            if (inputArray.Length > Level)
            {
                inputArray = inputArray.Skip(inputArray.Length - Level).ToArray();
            }
            else if (inputArray.Length < Level)
            {
                inputArray = PadArrayLow(inputArray);
            }

            var key = new SourceWords(inputArray);
            var chosen = Model[key];
            return chosen;
        }

        // Pad out an array with empty strings from bottom up
        // Used when providing a seed sentence or word for generation
        private string[] PadArrayLow(string[] input)
        {
            var splitCount = input.Length;
            if (splitCount > Level)
            {
                input = input.Skip(splitCount - Level).Take(Level).ToArray();
            }

            string[] p = new string[Level];
            int j = 0;
            for (int i = (Level - input.Length); i < (Level); i++)
            {
                p[i] = input[j];
                j++;
            }
            for (int i = Level - input.Length; i > 0; i--)
            {
                p[i - 1] = "";
            }

            return p;
        }

        // Save the model to file for use later
        public void Save(string file)
        {
            var modelJson = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(file, modelJson);
        }

        // Load a model which has been saved
        public static Markov Load(string file, int level = 1)
        {
            var model = JsonConvert.DeserializeObject<Markov>(File.ReadAllText(file));
            
            model.Retrain(level);

            Console.WriteLine("Loaded level {0} model with {1} lines of training data", model.Level, model.SourceLines.Count);
            return model;
        }
    }

    public class SourceWords
    {
        public string[] Before { get; set; }

        public SourceWords(params string[] args)
        {
            this.Before = args;
        }

        public override bool Equals(object o)
        {
            var x = o as SourceWords;

            if (x == null && this != null)
            {
                return false;
            }

            bool equals = this.Before.OrderBy(a => a).SequenceEqual(x.Before.OrderBy(a => a));
            return equals;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                foreach (var member in Before.Where(a => !string.IsNullOrWhiteSpace(a)))
                {
                    hash = hash * 23 + member.GetHashCode();
                }
                return hash;
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(Before);
        }
    }
}
