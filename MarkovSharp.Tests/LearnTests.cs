using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MarkovSharp.TokenisationStrategies;
using Xunit;

namespace MarkovSharp.Tests
{
    public class LearnTests : BaseMarkovTests
    {
        [Fact]
        public void LearnEmptyStringDoesNotThrow()
        {
            var model = new StringMarkov();
            new Action(() => model.Learn("")).ShouldNotThrow();
        }

        [Fact]
        public void LearnEmptyStringDoesNotAddToSourceLinesOrModel()
        {
            var model = new StringMarkov();
            model.Learn("");

            model.SourceLines.Should().BeEmpty();
            model.SourceLines.Count.Should().Be(0);
            model.Model.Sum(x => x.Value.Count).Should().Be(0);
        }

        [Fact]
        public void LearnNullStringDoesNotThrow()
        {
            var model = new StringMarkov();
            new Action(() => model.Learn(null)).ShouldNotThrow();
        }

        [Fact]
        public void LearnNullStringDoesNotAddToSourceLinesOrModel()
        {
            var model = new StringMarkov();
            model.Learn(null);

            model.SourceLines.Should().BeEmpty();
            model.SourceLines.Should().BeEmpty();
            model.Model.Sum(x => x.Value.Count).Should().Be(0);
        }

        [Fact]
        public void LinesAreAddedToModelOnLearn()
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);

            model.SourceLines.Count.Should().Be(ExampleData.Count());
            ExampleData.ShouldBeEquivalentTo(model.SourceLines);
        }

        [Fact]
        public void CanLearnLinesWithTrainedModel()
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);

            model.SourceLines.Count.Should().Be(ExampleData.Count(s => s.Split(' ').Length > model.Level));
            model.SourceLines.ShouldBeEquivalentTo(ExampleData.Where(s => s.Split(' ').Length > model.Level));

            model.Learn("I do not like green eggs and hams");
            model.SourceLines.Count.Should().Be(ExampleData.Count() + 1);
        }

        [Fact]
        public void LearningDuplicateLinesAreIgnored()
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);
            model.Learn(ExampleData);

            var _ = ExampleData.Except(model.SourceLines).ToList();

            model.SourceLines.Count.Should().Be(ExampleData.Count());
            ExampleData.ToList().ShouldBeEquivalentTo(model.SourceLines);
            model.SourceLines.Should().OnlyHaveUniqueItems();
        }

        [Fact]
        public void CanLearnDuplicatesDoesNotAddDupeSourceLinesAddsDupeTransitions()
        {
            var model = new StringMarkov();
            model.Learn("Testing the model");
            model.Learn("Testing the model");

            model.SourceLines.ShouldBeEquivalentTo(new List<string> { "Testing the model" });
            model.SourceLines.Count.Should().Be(1);
            model.Model.Sum(x => x.Value.Count).Should().Be(8);
            model.SourceLines.Should().OnlyHaveUniqueItems();
        }

        [Fact]
        public void SentenceSmallerThanLevelIsNotAddedToSourceLines()
        {
            var model = new StringMarkov(5);
            model.Learn("A short sentence");

            model.SourceLines.Should().BeEmpty();
        }

        [Fact]
        public void SentenceSmallerThanLevelIsNotAddedToModel()
        {
            var model = new StringMarkov(5);
            model.Learn("A short sentence");

            model.Model.Count.Should().Be(0);
        }
    }
}
