using System.Collections.Generic;
using System.Linq;
using MarkovSharp.TokenisationStrategies;
using NUnit.Framework;

namespace MarkovSharp.Tests
{
    [TestFixture]
    public class LearnTests : BaseMarkovTests
    {
        [Test]
        public void LearnEmptyStringDoesNotThrow()
        {
            var model = new StringMarkov();
            Assert.DoesNotThrow(() => model.Learn(""));
        }

        [Test]
        public void LearnEmptyStringDoesNotAddToSourceLinesOrModel()
        {
            var model = new StringMarkov();
            model.Learn("");

            CollectionAssert.AreEquivalent(new List<string>(), model.SourceLines);
            Assert.AreEqual(0, model.SourceLines.Count);
            Assert.AreEqual(0, model.Model.Sum(x => x.Value.Count));
        }

        [Test]
        public void LearnNullStringDoesNotThrow()
        {
            var model = new StringMarkov();
            Assert.DoesNotThrow(() => model.Learn(null));
        }

        [Test]
        public void LearnNullStringDoesNotAddToSourceLinesOrModel()
        {
            var model = new StringMarkov();
            model.Learn(null);

            CollectionAssert.AreEquivalent(new List<string>(), model.SourceLines);
            Assert.AreEqual(0, model.SourceLines.Count);
            Assert.AreEqual(0, model.Model.Sum(x => x.Value.Count));
        }

        [Test]
        public void LinesAreAddedToModelOnLearn()
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);

            Assert.AreEqual(ExampleData.Count(), model.SourceLines.Count);
            CollectionAssert.AreEquivalent(ExampleData, model.SourceLines);
        }

        [Test]
        public void CanLearnLinesWithTrainedModel()
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);
            
            Assert.AreEqual(ExampleData.Count(s => s.Split(' ').Length > model.Level), model.SourceLines.Count);
            CollectionAssert.AreEquivalent(ExampleData.Where(s => s.Split(' ').Length > model.Level), model.SourceLines);

            model.Learn("I do not like green eggs and hams");
            Assert.AreEqual(ExampleData.Count() + 1, model.SourceLines.Count);
        }

        [Test]
        public void LearningDuplicateLinesAreIgnored()
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);
            model.Learn(ExampleData);

            var e1 = ExampleData.Except(model.SourceLines).ToList();

            Assert.AreEqual(ExampleData.Count(), model.SourceLines.Count);
            Assert.That(ExampleData.ToList(), Is.EquivalentTo(model.SourceLines));
            Assert.That(model.SourceLines, Is.Unique);
        }

        [Test]
        public void CanLearnDuplicatesDoesNotAddDupeSourceLinesAddsDupeTransitions()
        {
            var model = new StringMarkov();
            model.Learn("Testing the model");
            model.Learn("Testing the model");

            CollectionAssert.AreEquivalent(new List<string> { "Testing the model" }, model.SourceLines);
            Assert.AreEqual(1, model.SourceLines.Count);
            Assert.AreEqual(8, model.Model.Sum(x => x.Value.Count));
            Assert.That(model.SourceLines, Is.Unique);
        }

        [Test]
        public void SentenceSmallerThanLevelIsNotAddedToSourceLines()
        {
            var model = new StringMarkov(5);
            model.Learn("A short sentence");

            CollectionAssert.AreEquivalent(new List<string>(), model.SourceLines);
        }

        [Test]
        public void SentenceSmallerThanLevelIsNotAddedToModel()
        {
            var model = new StringMarkov(5);
            model.Learn("A short sentence");
            
            Assert.AreEqual(0, model.Model.Count, string.Join(", ", model.Model.Select(a => string.Join(" ", a))));
        }
    }
}
