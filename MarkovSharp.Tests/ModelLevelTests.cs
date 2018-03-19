using System;
using MarkovSharp.Tests;
using MarkovSharp.TokenisationStrategies;
using NUnit.Framework;

namespace MarkovSharp
{
    [TestFixture]
    public class ModelLevelTests : BaseMarkovTests
    {
        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(int.MinValue)]
        public void LevelInvalidValueThrowsArgumentException(int level)
        {
            Assert.Throws<ArgumentException>(() => new StringMarkov(level));
        }

        [TestCase(1)]
        [TestCase(3)]
        public void RetrainingSetsCorrectLevel(int retrainDepth)
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);
            Assert.AreEqual(2, model.Level);

            model.Retrain(retrainDepth);
            Assert.AreEqual(retrainDepth, model.Level);
        }
    }
}
