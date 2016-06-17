using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MarkovSharp.Tests
{
    public class BaseMarkovTests
    {
        private readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [SetUp]
        public void Setup()
        {
            Logger.Info("Running Setup..");
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
