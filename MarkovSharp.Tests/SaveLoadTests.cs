using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MarkovSharp.Tests
{
    [TestFixture]
    public class SaveLoadTests : BaseMarkovTests
    {
        [Test]
        public void CanSaveEmptyModel()
        {
            var model = new Markov();
            model.Save(ModelFileName);

            var savedModel = Markov.Load(ModelFileName, model.Level);

            Assert.AreEqual(model.Level, savedModel.Level);
            Assert.AreEqual(model.SourceLines, savedModel.SourceLines);
        }

        [Test]
        public void CanSaveTrainedModel()
        {
            var model = new Markov();
            model.Learn(ExampleData);
            model.Save(ModelFileName);

            var savedModel = Markov.Load(ModelFileName, model.Level);

            Assert.AreEqual(model.Level, savedModel.Level);
            Assert.AreEqual(model.SourceLines, savedModel.SourceLines);
        }

        [Test]
        public void SavedFileDoesntContainModelDictionary()
        {
            var model = new Markov();
            model.Learn(ExampleData);
            model.Save(ModelFileName);

            string fileContents = File.ReadAllText(ModelFileName);
            dynamic loaded = JsonConvert.DeserializeObject<dynamic>(fileContents);
            Assert.IsNull(loaded.Model);
        }

        [Test]
        public void SavedFileContainsLevel()
        {
            var model = new Markov();
            model.Learn(ExampleData);
            model.Save(ModelFileName);

            string fileContents = File.ReadAllText(ModelFileName);
            dynamic loaded = JsonConvert.DeserializeObject<dynamic>(fileContents);
            Assert.IsNotNull(loaded.Level);
            Assert.AreEqual(2, (int) loaded.Level);
        }

        [TestCase(2)]
        [TestCase(3)]
        public void CanLoadWithGivenLevel(int newLevel)
        {
            var model = new Markov(1);
            model.Learn(ExampleData);
            model.Save(ModelFileName);

            var savedModel = Markov.Load(ModelFileName, newLevel);

            Assert.AreEqual(newLevel, savedModel.Level);
            Assert.AreEqual(model.SourceLines, savedModel.SourceLines);
            
            Assert.AreEqual(newLevel, savedModel.Model.Max(a => a.Key.Before.Length));
        }
    }
}
