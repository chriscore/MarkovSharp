using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            var model2 = model.Load(ModelFileName);
            var loaded = model2 as StringMarkov;
            Assert.AreEqual(1, loaded.Level);
        }

        [Test]
        public void LevelCorrectWhenModelIsLoadedUsingValue()
        {
            var model = new StringMarkov(3);

            model.Learn(ExampleData);
            model.Save(ModelFileName);

            var model2 = model.Load(ModelFileName, 2);
            var loaded = model2 as StringMarkov;
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
