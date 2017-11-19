using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace MarkovSharp.Tests
{
    public class BaseMarkovTests
    {
        /*
        public string[] ExampleData =
        {
            "First example line for training",
            "Second line of training data",
            "Third line of example data"
        };
        */
        public static IEnumerable<string> ExampleData = JsonConvert.DeserializeObject<string[]>(File.ReadAllText("testlines.json")).Take(150);

        public readonly string ModelFileName = $"{Guid.NewGuid()}.json";
    }
}
