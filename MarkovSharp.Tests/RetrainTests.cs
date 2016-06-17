using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MarkovSharp.TokenisationStrategies;
using NUnit.Framework;

namespace MarkovSharp.Tests
{
    [TestFixture]
    public class RetrainTests : BaseMarkovTests
    {
        [TestCase(1)]
        [TestCase(3)]
        public void RetrainingSetsCorrectDictionaryKeyLength(int retrainDepth)
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);

            model.Retrain(retrainDepth);
            Assert.AreEqual(retrainDepth, model.Model.Max(a => a.Key.Before.Length));
        }

        [TestCase(1)]
        [TestCase(3)]
        public void SourceLinesAreSameAfterRetrained(int retrainDepth)
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);
            var oldLines = new List<string>(model.SourceLines);

            model.Retrain(retrainDepth);
            CollectionAssert.AreEquivalent(oldLines, model.SourceLines);
        }

        [TestCase(1, false)]
        [TestCase(2, true)]
        [TestCase(3, false)]
        public void RetrainedModelIsNotSameIfLevelIsDifferent(int retrainDepth, bool expectSameModel)
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);
            var dict = new ConcurrentDictionary<SourceGrams<string>, List<string>>(model.Model); // this will break for non string type models during testing until fixed

            model.Retrain(retrainDepth);

            if (expectSameModel)
            {
                //CollectionAssert.AreEquivalent(dict, model.Model);
                Assert.AreEqual(dict.Sum(a => a.Key.Before.Count()), model.Model.Sum(a => a.Key.Before.Count()));
            }
            else
            {
                //CollectionAssert.AreNotEquivalent(dict, model.Model);
                Assert.AreNotEqual(dict.Sum(a => a.Key.Before.Count()), model.Model.Sum(a => a.Key.Before.Count()));
            }
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(int.MinValue)]
        public void RetrainValueMustBePositiveInteger(int retrainDepth)
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);

            var ex = Assert.Throws<ArgumentException>(() => model.Retrain(retrainDepth));
            Assert.AreEqual("Invalid argument - retrain level must be a positive integer\r\nParameter name: newLevel", ex.Message);
        }
    }
}
