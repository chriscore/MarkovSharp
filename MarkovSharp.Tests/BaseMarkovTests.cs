using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace MarkovSharp.Tests
{
    public class BaseMarkovTests
    {
        protected static readonly IEnumerable<string> ExampleData = JsonConvert.DeserializeObject<string[]>(File.ReadAllText("testlines.json")).Take(150);
    }
}
