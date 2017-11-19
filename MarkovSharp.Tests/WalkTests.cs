using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MarkovSharp.TokenisationStrategies;
using Xunit;

namespace MarkovSharp.Tests
{
    public class WalkTests : BaseMarkovTests
    {
        [Fact]
        public void BasicWalkOnTrainedModelGeneratesCorrectNumberOfLines()
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);
            var result = model.Walk().ToList();

            result.Should().HaveCount(1);
            Console.WriteLine(result.First());
        }

        [Fact]
        public void WalkOnUntrainedModelIsEmpty()
        {
            var model = new StringMarkov();
            var result = model.Walk();

            result.ShouldBeEquivalentTo(new List<string> { string.Empty });
        }
        
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]

        public void WalkOnTrainedModelGeneratesCorrectNumberOfLines(int lineCount)
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);
            var result = model.Walk(lineCount);

            result.Should().HaveCount(lineCount);
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(0)]
        public void MustCallWalkWithPositiveInteger(int lineCount)
        {
            var model = new StringMarkov();
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                model.Walk(lineCount).ToList();
            });
            ex.Message.Should().Be("Invalid argument - line count for walk must be a positive integer\r\nParameter name: lines");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public void WalkCreatesNewContent(int walkCount)
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);
            model.EnsureUniqueWalk = true;

            var results = model.Walk(walkCount).ToList();

            ExampleData.Should().NotBeSubsetOf(results);
            foreach (var result in results)
            {
                result.Should().NotBeEmpty();
                ExampleData.Should().NotContain(result);
            }
        }

        [Fact]
        public void CanWalkWithUniqueOutputUsingSeed()
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);
            model.EnsureUniqueWalk = true;

            var results = model.Walk(1000, "This is a line").ToList();
            foreach (var result in results)
            {
                result.Should().StartWith("This is a line");
            }

            results.Distinct().Should().HaveCount(results.Count);
        }

        [Fact]
        public void CanWalkWithUniqueOutput()
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);
            model.EnsureUniqueWalk = true;

            var results = model.Walk(1000).ToList();
            results.Distinct().Should().HaveCount(results.Count);
        }

        [Fact]
        public void CanWalkUsingSeed()
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);

            var results = model.Walk(100, "This is a line").ToList();

            results.Should().HaveCount(100);
            foreach (var result in results)
            {
                result.Should().StartWith("This is a line");
            }
        }
    }
}
