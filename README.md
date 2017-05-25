# MarkovSharp

[![chriscore MyGet Build Status](https://www.myget.org/BuildSource/Badge/chriscore?identifier=2e1ed033-4736-4537-9a85-1ad807bf13c3)](https://www.myget.org/)

An easy to use C# implementation of an N-state Markov model.
MarkovSharp exposes the notion of a model strategy, which allows you to use pre-defined model strategies, or create your own.

## Interactive Predictive Text Example
Go here to try out an example ASP.NET site which uses a MarkovSharp based backend API to easily provide predictive text functionality when given some trained text: http://markovsharp.azurewebsites.net/
The site is included in the MarkovSharp repo.

## Getting Started

Download and reference the latest version of MarkovSharp from NuGet here: https://www.nuget.org/packages/MarkovSharp
Alternatively, just pull and build the class library to get going.

This repo has a file containing some training data with famous quotes to use and test with.

### Using the StringMarkov Strategy
```
	// Some training data
	var lines = new string[]
	{
		"Frankly, my dear, I don't give a damn.",
		"Mama always said life was like a box of chocolates. You never know what you're gonna get.",
		"Many wealthy people are little more than janitors of their possessions."
	};

	// Create a new model
	var model = new StringMarkov(1);

	// Train the model
	model.Learn(lines);

	// Create some permutations
	Console.WriteLine(model.Walk().First());

	// Output:
	// Frankly, my dear, I don't give a box of their possessions.
```

### Using the SanfordMidiMarkov Strategy
```
	var midiFile = "C:/Users/currentUser/Desktop/mySong.mid";

	Sequence seq = new Sequence(midiFile);
	Sequence seqNew = new Sequence(seq.Division);

	// Get a random track to learn from
	// Take more to produce output on multiple tracks.
	var ints = Enumerable.Range(0, seq.Count).OrderBy(a => Guid.NewGuid()).Take(1);
	foreach(int i in ints)
	{
		Track t = seq[i];
		SanfordMidiMarkov model = new SanfordMidiMarkov(2);
		model.EnsureUniqueWalk = true;

		// Learn the track
		model.Learn(t);

		// Walk the model
		var result = model.Walk().FirstOrDefault();

		// Add the result to the new sequence
		seqNew.Add(result);
	}

	// Write a new midi file
	seqNew.Save("C:/Users/chriscore/Desktop/myNewSong.mid");
```

### Strategies (Extensibility)
Markov model strategies are defined by implementing IMarkovStrategy, and allow extensibility to process any type of input data.
MarkovSharp contains a base implementation of IMarkovStrategy called GenericMarkov, a generic implementation of a Markov model engine.
Although GenericMarkov cannot be used directly, classes which inherit and extend from it are used to define model implementations.
Out of the box, MarkovSharp has implementations for the written word and music (MIDI) data types.

When presented with a set of training data, a Markov model needs to understand a few things:
* How to split 'phrases' into a list of individual 'tokens' (e.g a string to list of words)
* What object is to be used to define empty tokens (e.g if we have an n=4 model but are only indexing the first token of a phrase, we need to pad 4 empty tokens before it)
* What object is to be used to define the end of a phrase
* How to join tokens back up into a phrases

These are easy to define for most types, and allow MarkovSharp to be a fairly flexible library, capable of processing generic data types.
The next section shows an example IMarkovStrategy implementation that extends from GenericMarkov, for an implementation to process the written word (StringMarkov).
If you have a data type that needs processing differently, a similar approach to below will allow this.

### StringMarkov Strategy
```
	// This model will use a phrase type of string, and also token type of string.
	public class StringMarkov : GenericMarkov<string, string>, IMarkovStrategy<string, string>
    {
        public StringMarkov(int level = 2)
            : base(level)
        { }

		// Define how to split a phrase to collection of tokens
        public override IEnumerable<string> SplitTokens(string input)
        {
            if (input == null)
            {
                return new List<string>() { GetPrepadGram() };
            }

            return input?.Split(' ');
        }

		// Define how to join the generated tokens back to a phrase
        public override string RebuildPhrase(IEnumerable<string> tokens)
        {
            return string.Join(" ", tokens);
        }

		// Define the value to signify the end of a phrase in the model
        public override string GetTerminatorGram()
        {
            return null;
        }

		// Define a default padding value to use when no value is available
        public override string GetPrepadGram()
        {
            return "";
        }
    }
```

## License

This project is licensed under the MIT License
