# MarkovSharp

An easy to use C# implementation of an N-state Markov model.

## Getting Started

Pull this repo and run the contained markov.linq file using LINQPad.
The script should run and generate text using the embedded training data.
There is also a file containing a few thousand famous quotes to use as additional training data.

### Usage

I have tried to make this library easy to use, the following will generate and train a Markov model with 2 levels of state.

```
	// Some training data
	var lines = new string[]
	{
		"Frankly, my dear, I don't give a damn.",
		"Mama always said life was like a box of chocolates. You never know what you're gonna get.",
		"Many wealthy people are little more than janitors of their possessions."
	};
	
	// Create a new model
	var model = new Markov(1);
	
	// Train the model
	model.Learn(lines);
	
	// Create some permutations
	model.Walk(1).Dump();
	
	// Output:
	// Frankly, my dear, I don't give a box of their possessions.  
```

## License

This project is licensed under the MIT License