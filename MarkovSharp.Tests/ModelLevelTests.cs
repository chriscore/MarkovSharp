using System;
using MarkovSharp.Tests;
using MarkovSharp.TokenisationStrategies;
using NUnit.Framework;

namespace MarkovSharp
{
    [TestFixture]
    public class ModelLevelTests : BaseMarkovTests
    {
        [Test]
        public void LevelCorrectWhenModelIsLoadedUsingDefault()
        {
            var model = new StringMarkov(3);

            model.Learn(ExampleData);
            model.Save(ModelFileName);

            var loaded = model.Load<StringMarkov>(ModelFileName);
            Assert.AreEqual(1, loaded.Level);
        }

        [Test]
        public void LevelCorrectWhenModelIsLoadedUsingValue()
        {
            var model = new StringMarkov(3);

            model.Learn(ExampleData);
            model.Save(ModelFileName);

            var loaded = model.Load<StringMarkov>(ModelFileName, 2);
            Assert.AreEqual(2, loaded.Level);
        }

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
