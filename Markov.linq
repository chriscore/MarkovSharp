<Query Kind="Program">
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>Newtonsoft.Json.Serialization</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.IO.Compression</Namespace>
</Query>

void Main()
{
	var lines = new string[]
	{
		"I'll be back.",
		"You talking to me?",
		"May the Force be with you.",
		"Frankly, my dear, I don't give a damn.",
		"The only difference between me and a madman is that I'm not mad.",
		"A mathematician is a device for turning coffee into theorems.",
		"He is one of those people who would be enormously improved by death.",
		"Mama always said life was like a box of chocolates. You never know what you're gonna get.",
		"Many wealthy people are little more than janitors of their possessions."
	};
	
	// Create a new model
	var model = new Markov(1);
	
	// Or load a model which has been saved
	// var model = Markov.Load("C:/MarkovModel.json");
	
	// Train the model with some data
	model.Learn(lines);
	
	// Create some permutations
	model.Walk(10).Dump();
	
	// Save a model for use later
	//model.Save("C:/MarkovModel.json");
}

public class Markov
{
	public Markov(int level = 2)
	{
		if (level < 1)
		{
			throw new ArgumentException("Invalid value: level must be a positive integer", "level");
		}
		
		Model = new Dictionary<SourceWords, List<string>>();
		SourceLines = new List<string>();
		Level = level;
	}
	
	// Dictionary containing the model data. The key is the N number of
	// previous words and value is a list of possible outcomes, given that key
	private Dictionary<SourceWords, List<string>> Model { get; set; }
	public List<string> SourceLines {get;set;}
	
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
		// Add it to the source lines so we can ignore it 
		// when learning in future
		if (!SourceLines.Contains(sentence))
		{
			SourceLines.Add(sentence);
		}
		
		// Ignore particularly short sentences
		if (sentence.Split(' ').Length < Level)
		{
			return;
		}
		
		// Split the sentence to an array of words
		var tokens = sentence
			.Split(' ')
			.ToArray();
		
		
		for (int i=0; i<tokens.Length; i++)
		{
			var current = tokens[i];

			List<string> previousCol = new List<string>();
			for (int j=Level; j > 0; j--)
			{
				string previous;
				try
				{
					previous = tokens[i - j];
					previousCol.Add(previous);
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

		var lastEntry = tokens[tokens.Length - 1];
		List<string> lastCol = new List<string>();
		for (int j=Level; j>0; j--)
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
		List<string> sentences = new List<string>();
		
		for (int z = 0; z < lines; z++)
		{
			sentences.Add(WalkLine(seed));
		}
		
		return sentences;
	}

	private string WalkLine(string seed)
	{
		var arraySeed = PadArrayLow(seed.Split(' '));
		
		StringBuilder sb = new StringBuilder();
		
		// Allocate a queue to act as the memory, which is n levels deep
		// of previous words that were used
		System.Collections.Queue q = new System.Collections.Queue(arraySeed);
		
		// If the start of the generated text has been seeded,
		// append that before generating the rest
		if (!string.IsNullOrEmpty(seed))
		{ sb.Append(seed + " "); }
		
		while (true)
		{
			// Choose a new word to add from the model
			List<string> keyParams = new List<string>();
			foreach (string s in q)
			{
				keyParams.Add(s);
			}

			var key = new SourceWords(keyParams.ToArray());
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
		
		return sb.ToString();
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
		var settings = new JsonSerializerSettings() { ContractResolver = new DictionaryAsArrayResolver() };
		var modelJson = JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented, settings);
		File.WriteAllText(file, modelJson);
	}

	// Load a model which has been saved
	public static Markov Load(string file)
	{
		var settings = new JsonSerializerSettings() { ContractResolver = new DictionaryAsArrayResolver() };
		var model = JsonConvert.DeserializeObject<Markov>(File.ReadAllText(file), settings);
		
		Console.WriteLine("Loaded level {0} model with {1} lines of training data", model.Level, model.SourceLines.Count);
		return model;
	}
}

// A wrapper for string[] to override equality comparisons
// so that this can be used as the dictionary key in the model
public class SourceWords
{
	public string[] Before { get; set;}

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
		try
		{
			bool equals = this.Before.OrderBy(a => a).SequenceEqual(x.Before.OrderBy(a => a));
			return equals;
		}
		catch (Exception e)
		{
			this.Dump();
			x.Dump();
			e.ToString().Dump();
			
			throw e;
		}
	}

	public override int GetHashCode()
	{
		unchecked // Overflow is fine, just wrap
		{
			int hash = 17;
			foreach (var member in this.Before.Where(a => !string.IsNullOrWhiteSpace(a)))
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

// Used when saving and loading a model as JSON
class DictionaryAsArrayResolver : DefaultContractResolver
{
	protected override JsonContract CreateContract(Type objectType)
	{
		if (objectType.GetInterfaces().Any(i => i == typeof(IDictionary) ||
			(i.IsGenericType &&
			 i.GetGenericTypeDefinition() == typeof(IDictionary<,>))))
		{
			return base.CreateArrayContract(objectType);
		}

		return base.CreateContract(objectType);
	}

	protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
	{
		var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
						.Select(p => base.CreateProperty(p, memberSerialization))
					.Union(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
							   .Select(f => base.CreateProperty(f, memberSerialization)))
					.ToList();
		props.ForEach(p => { p.Writable = true; p.Readable = true; });
		return props;
	}
}