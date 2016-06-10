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
    public class MarkovTests : BaseMarkovTests
    {
        [Test]
        public void ParameterlessConstructorUsesLevel2()
        {
            var model = new StringMarkov();
            Assert.AreEqual(2, model.Level);
        }

        [Test]
        public void LevelCanBeSetUsingConstructor()
        {
            var model = new StringMarkov(4);
            Assert.AreEqual(4, model.Level);
        }
    }
}
