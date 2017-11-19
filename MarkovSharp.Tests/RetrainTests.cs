using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MarkovSharp.Models;
using MarkovSharp.TokenisationStrategies;
using Xunit;

namespace MarkovSharp.Tests
{
    public class RetrainTests : BaseMarkovTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public void RetrainingSetsCorrectDictionaryKeyLength(int retrainDepth)
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);

            model.Retrain(retrainDepth);
            model.Model.Max(a => a.Key.Before.Length).Should().Be(retrainDepth);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public void SourceLinesAreSameAfterRetrained(int retrainDepth)
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);
            var oldLines = new List<string>(model.SourceLines);

            model.Retrain(retrainDepth);
            model.SourceLines.ShouldBeEquivalentTo(oldLines);
        }

        [Theory]
        [InlineData(1, false)]
        [InlineData(2, true)]
        [InlineData(3, false)]
        public void RetrainedModelIsNotSameIfLevelIsDifferent(int retrainDepth, bool expectSameModel)
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);
            var dict = new ConcurrentDictionary<SourceGrams<string>, List<string>>(model.Model); // this will break for non string type models during testing until fixed

            model.Retrain(retrainDepth);

            if (expectSameModel)
            {
                //CollectionAssert.AreEquivalent(dict, model.Model);
                model.Model.Sum(a => a.Key.Before.Length).Should().Be(dict.Sum(a => a.Key.Before.Length));
            }
            else
            {
                //CollectionAssert.AreNotEquivalent(dict, model.Model);
                model.Model.Sum(a => a.Key.Before.Length).Should().NotBe(dict.Sum(a => a.Key.Before.Length));
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void RetrainValueMustBePositiveInteger(int retrainDepth)
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);

            var ex = Assert.Throws<ArgumentException>(() => model.Retrain(retrainDepth));
            ex.Message.Should().Be("Invalid argument - retrain level must be a positive integer\r\nParameter name: newLevel");
        }
    }
}
