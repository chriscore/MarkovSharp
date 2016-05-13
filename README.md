# MarkovSharp

[![chriscore MyGet Build Status](https://www.myget.org/BuildSource/Badge/chriscore?identifier=2e1ed033-4736-4537-9a85-1ad807bf13c3)](https://www.myget.org/)

An easy to use C# implementation of an N-state Markov model.

## Getting Started

Download and reference the latest version of MarkovSharp from NuGet here: https://www.nuget.org/packages/MarkovSharp
Alternatively, just pull and build the class library to get going.

This repo has a file containing some training data with famous quotes to use and test with.

### Usage

I have tried to make this library really easy to use. Once referenced, the following will generate and train a second order Markov model.

```
	// Some training data
	var lines = new string[]
	{
		"Frankly, my dear, I don't give a damn.",
		"Mama always said life was like a box of chocolates. You never know what you're gonna get.",
		"Many wealthy people are little more than janitors of their possessions."
	};
	
	// Create a new model
	var model = new Markov(2);
	
	// Train the model
	model.Learn(lines);
	
	// Create some permutations
	Console.WriteLine(model.Walk(1).First());
	
	// Output:
	// Frankly, my dear, I don't give a box of their possessions.  
```

## License

This project is licensed under the MIT License

