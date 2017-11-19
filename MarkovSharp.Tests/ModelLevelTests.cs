using System;
using FluentAssertions;
using MarkovSharp.TokenisationStrategies;
using Xunit;

namespace MarkovSharp.Tests
{
    public class ModelLevelTests : BaseMarkovTests
    {
        [Fact]
        public void LevelCorrectWhenModelIsLoadedUsingDefault()
        {
            var model = new StringMarkov(3);

            model.Learn(ExampleData);
            model.Save(ModelFileName);

            var loaded = model.Load<StringMarkov>(ModelFileName);
            loaded.Level.Should().Be(1);
        }

        [Fact]
        public void LevelCorrectWhenModelIsLoadedUsingValue()
        {
            var model = new StringMarkov(3);

            model.Learn(ExampleData);
            model.Save(ModelFileName);

            var loaded = model.Load<StringMarkov>(ModelFileName, 2);
            loaded.Level.Should().Be(2);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void LevelInvalidValueThrowsArgumentException(int level)
        {
            Assert.Throws<ArgumentException>(() => new StringMarkov(level));
        }
        
        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public void RetrainingSetsCorrectLevel(int retrainDepth)
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);
            model.Level.Should().Be(2);

            model.Retrain(retrainDepth);
            model.Level.Should().Be(retrainDepth);
        }
    }
}
