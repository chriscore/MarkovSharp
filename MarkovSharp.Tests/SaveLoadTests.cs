using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using MarkovSharp.TokenisationStrategies;
using Newtonsoft.Json;
using Xunit;

namespace MarkovSharp.Tests
{
    public class SaveLoadTests : BaseMarkovTests, IDisposable
    {
        [Fact]
        public void CanSaveEmptyModel()
        {
            var model = new StringMarkov();
            model.Save(ModelFileName);
            
            var forLoading = new StringMarkov().Load<StringMarkov>(ModelFileName, model.Level);

            forLoading.Level.Should().Be(model.Level);
            forLoading.SourceLines.Should().BeEquivalentTo(model.SourceLines);
        }

        [Fact]
        public void CanSaveTrainedModel()
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);
            model.Save(ModelFileName);

            var forLoading = new StringMarkov().Load<StringMarkov>(ModelFileName, model.Level);

            forLoading.Level.Should().Be(model.Level);
            forLoading.SourceLines.Should().BeEquivalentTo(model.SourceLines);
        }

        [Fact]
        public void SavedFileDoesntContainModelDictionary()
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);
            model.Save(ModelFileName);

            var fileContents = File.ReadAllText(ModelFileName);
            var loaded = JsonConvert.DeserializeObject<dynamic>(fileContents);
            ((object)loaded.Model).Should().BeNull();
        }

        [Fact]
        public void SavedFileContainsLevel()
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);
            model.Save(ModelFileName);

            var fileContents = File.ReadAllText(ModelFileName);
            var loaded = JsonConvert.DeserializeObject<dynamic>(fileContents);
            ((string)loaded.Level.ToString()).Should().Be("2");
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        public void CanLoadWithGivenLevel(int newLevel)
        {
            var model = new StringMarkov(1);
            model.Learn(ExampleData);
            model.Save(ModelFileName);

            var forLoading = new StringMarkov().Load<StringMarkov>(ModelFileName, newLevel);

            forLoading.Level.Should().Be(newLevel);
            forLoading.SourceLines.Should().Equal(model.SourceLines);

            forLoading.Model.Max(a => a.Key.Before.Length).Should().Be(newLevel);
        }

        [Fact]
        public void CanWalkLoadedModel()
        {
            var model = new StringMarkov(1);
            model.Learn(ExampleData);
            model.Save(ModelFileName);

            var newModel = new StringMarkov().Load<StringMarkov>(ModelFileName);
            
            var lines = newModel.Walk();

            Console.WriteLine(string.Join("\r\n", lines));
            lines.Should().HaveCount(1);
            lines.First().Should().NotBeEmpty();
        }

        public void Dispose()
        {
            File.Delete(ModelFileName);
        }
    }
}
