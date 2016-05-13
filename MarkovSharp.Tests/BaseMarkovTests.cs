using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MarkovSharp.Tests
{
    public class BaseMarkovTests
    {
        [SetUp]
        public void Setup()
        {
            Console.WriteLine("Running Setup..");
            if (File.Exists("ExampleModel.json"))
            {
                File.Delete("ExampleModel.json");
            }
        }

        /*
        public string[] ExampleData =
        {
            "First example line for training",
            "Second line of training data",
            "Third line of example data"
        };
        */
        public static IEnumerable<string> ExampleData = JsonConvert.DeserializeObject<string[]>(File.ReadAllText("testlines.json")).Take(150);

        public static string ModelFileName => "ExampleModel.json";
    }
}
