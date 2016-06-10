using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using MarkovSharp.TokenisationStrategies;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MarkovSharp.Tests
{
    [TestFixture]
    public class SaveLoadTests : BaseMarkovTests
    {
        private readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void CanSaveEmptyModel()
        {
            var model = new StringMarkov();
            model.Save(ModelFileName);
            
            var forLoading = new StringMarkov();
            var newmodel = forLoading.Load(ModelFileName, model.Level);

            Assert.AreEqual(model.Level, forLoading.Level);
            Assert.AreEqual(model.SourceLines, forLoading.SourceLines);
        }

        [Test]
        public void CanSaveTrainedModel()
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);
            model.Save(ModelFileName);

            var forLoading = new StringMarkov();
            var newmodel = forLoading.Load(ModelFileName, model.Level);

            Assert.AreEqual(model.Level, forLoading.Level);
            Assert.AreEqual(model.SourceLines, forLoading.SourceLines);
        }

        [Test]
        public void SavedFileDoesntContainModelDictionary()
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);
            model.Save(ModelFileName);

            string fileContents = File.ReadAllText(ModelFileName);
            dynamic loaded = JsonConvert.DeserializeObject<dynamic>(fileContents);
            Assert.IsNull(loaded.Model);
        }

        [Test]
        public void SavedFileContainsLevel()
        {
            var model = new StringMarkov();
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
            var model = new StringMarkov(1);
            model.Learn(ExampleData);
            model.Save(ModelFileName);

            var forLoading = new StringMarkov();
            var newmodel = forLoading.Load(ModelFileName, newLevel);

            Assert.AreEqual(newLevel, forLoading.Level);
            Assert.AreEqual(model.SourceLines, forLoading.SourceLines);
            
            Assert.AreEqual(newLevel, forLoading.Model.Max(a => a.Key.Before.Length));
        }

        [Test]
        public void CanWalkLoadedModel()
        {
            var model = new StringMarkov(1);
            model.Learn(ExampleData);
            model.Save(ModelFileName);

            var forLoading = new StringMarkov();
            var newmodel = forLoading.Load(ModelFileName);
            
            var lines = newmodel.Walk();

            Logger.Info(string.Join("\r\n", lines));
            Assert.AreEqual(1, lines.Count());
            Assert.That(lines.First(), Is.Not.Empty);
        }
    }
}
