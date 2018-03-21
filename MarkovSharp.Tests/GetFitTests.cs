using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkovSharp.TokenisationStrategies;
using NUnit.Framework;

namespace MarkovSharp.Tests
{
    [TestFixture]
    public class GetFitTests : BaseMarkovTests
    {
        [Test]
        public void GetFit_DirectMatch()
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);

            var testValue = "prepared for the great ordeal of meeting me";
            var result = model.GetFit(testValue);
        }

        [Test]
        public void GetFit_DirectNonMatch()
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);

            var testValue = "prepared NON for NON the NON great NON ordeal NON of NON meeting NON me";
            var result = model.GetFit(testValue);
        }
    }
}
