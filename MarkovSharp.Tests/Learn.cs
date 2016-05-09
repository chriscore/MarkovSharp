using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MarkovSharp.Tests
{
    [TestFixture]
    public class Learn : BaseMarkovTests
    {
        [Test]
        public void LinesAreAddedToModelOnLearn()
        {
            var model = new Markov();
            model.Learn(ExampleData);

            CollectionAssert.AreEquivalent(ExampleData, model.SourceLines);
        }

        [Test]
        public void CanLearnLinesWithTrainedModel()
        {
            var model = new Markov();
            model.Learn(ExampleData);

            CollectionAssert.AreEquivalent(ExampleData, model.SourceLines);

            model.Learn("I do not like green eggs and ham");
            Assert.AreEqual(ExampleData.Count() + 1, model.SourceLines.Count);
        }

        [Test]
        public void LearningDuplicateLinesAreIgnored()
        {
            var model = new Markov();
            model.Learn(ExampleData);
            model.Learn(ExampleData);

            CollectionAssert.AreEquivalent(ExampleData, model.SourceLines);
            Assert.AreEqual(ExampleData.Count(), model.SourceLines.Count);
        }

        [Test]
        public void CanLearnDuplicatesDoesNotAddDupeSourceLinesAddsDupeTransitions()
        {
            var model = new Markov();
            model.Learn("Testing the model");
            model.Learn("Testing the model");

            CollectionAssert.AreEquivalent(new List<string> { "Testing the model" }, model.SourceLines);
            Assert.AreEqual(1, model.SourceLines.Count);
            Assert.AreEqual(8, model.Model.Sum(x => x.Value.Count));
        }

        [Test]
        public void LearnEmptyString()
        {
            var model = new Markov();
            model.Learn("");

            CollectionAssert.AreEquivalent(new List<string> (), model.SourceLines);
            Assert.AreEqual(0, model.SourceLines.Count);
            Assert.AreEqual(0, model.Model.Sum(x => x.Value.Count));
        }

        [Test]
        public void SentenceSmallerThanLevelIsNotAddedToSourceLines()
        {
            var model = new Markov(5);
            model.Learn("A short sentence");

            CollectionAssert.AreEquivalent(new List<string>(), model.SourceLines);
        }

        [Test]
        public void SentenceSmallerThanLevelIsNotAddedToModel()
        {
            var model = new Markov(5);
            model.Learn("A short sentence");
            
            Assert.AreEqual(0, model.Model.Count);
        }
    }
}
