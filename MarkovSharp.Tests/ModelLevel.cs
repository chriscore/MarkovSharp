using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkovSharp.Tests;
using NUnit.Framework;

namespace MarkovSharp
{
    [TestFixture]
    public class ModelLevel : BaseMarkovTests
    {
        [Test]
        public void ParameterlessConstructorUsesLevel2()
        {
            var model = new Markov();
            Assert.AreEqual(2, model.Level);
        }

        [Test]
        public void LevelCanBeSetUsingConstructor()
        {
            var model = new Markov(4);
            Assert.AreEqual(4, model.Level);
        }

        [Test]
        public void LevelCorrectWhenModelIsLoadedUsingDefault()
        {
            var model = new Markov(3);

            model.Learn(ExampleData);
            model.Save(ModelFileName);

            var model2 = Markov.Load(ModelFileName);
            Assert.AreEqual(1, model2.Level);
        }

        [Test]
        public void LevelCorrectWhenModelIsLoadedUsingValue()
        {
            var model = new Markov(3);

            model.Learn(ExampleData);
            model.Save(ModelFileName);

            var model2 = Markov.Load(ModelFileName, 2);
            Assert.AreEqual(2, model2.Level);
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(int.MinValue)]
        public void LevelInvalidValueThrowsArgumentException(int level)
        {
            Assert.Throws<ArgumentException>(() => new Markov(level));
        }

        [TestCase(1)]
        [TestCase(3)]
        public void RetrainingSetsCorrectLevel(int retrainDepth)
        {
            var model = new Markov();
            model.Learn(ExampleData);
            Assert.AreEqual(2, model.Level);

            model.Retrain(retrainDepth);
            Assert.AreEqual(retrainDepth, model.Level);
        }
    }
}
