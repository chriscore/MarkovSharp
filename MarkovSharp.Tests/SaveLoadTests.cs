using System;
using System.Linq;
using FluentAssertions;
using MarkovSharp.TokenisationStrategies;
using Newtonsoft.Json;
using Xunit;

namespace MarkovSharp.Tests
{
    public class SerializeTests : BaseMarkovTests
    {
        [Fact]
        public void CanSerializeEmptyModel()
        {
            var model = new StringMarkov();
            var serialized = model.Serialize();
            
            var forLoading = new StringMarkov().Deserialize<StringMarkov>(serialized, model.Level);

            forLoading.Level.Should().Be(model.Level);
            forLoading.SourceLines.Should().BeEquivalentTo(model.SourceLines);
        }

        [Fact]
        public void CanSerializeTrainedModel()
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);
            var serialized = model.Serialize();

            var forLoading = new StringMarkov().Deserialize<StringMarkov>(serialized, model.Level);

            forLoading.Level.Should().Be(model.Level);
            forLoading.SourceLines.Should().BeEquivalentTo(model.SourceLines);
        }

        [Fact]
        public void SerializedModelDoesntContainModelDictionary()
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);
            var serialized = model.Serialize();

            var loaded = JsonConvert.DeserializeObject<dynamic>(serialized);
            ((object)loaded.Model).Should().BeNull();
        }

        [Fact]
        public void SerializedModelContainsLevel()
        {
            var model = new StringMarkov();
            model.Learn(ExampleData);
            var serialized = model.Serialize();

            var loaded = JsonConvert.DeserializeObject<dynamic>(serialized);
            ((string)loaded.Level.ToString()).Should().Be("2");
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        public void CanDeserializeWithGivenLevel(int newLevel)
        {
            var model = new StringMarkov(1);
            model.Learn(ExampleData);
            var serialized = model.Serialize();

            var forLoading = new StringMarkov().Deserialize<StringMarkov>(serialized, newLevel);

            forLoading.Level.Should().Be(newLevel);
            forLoading.SourceLines.Should().Equal(model.SourceLines);

            forLoading.Model.Max(a => a.Key.Before.Length).Should().Be(newLevel);
        }

        [Fact]
        public void CanWalkLoadedModel()
        {
            var model = new StringMarkov(1);
            model.Learn(ExampleData);
            var serialized = model.Serialize();

            var newModel = new StringMarkov().Deserialize<StringMarkov>(serialized);
            
            var lines = newModel.Walk().ToList();

            Console.WriteLine(string.Join("\r\n", lines));
            lines.Should().HaveCount(1);
            lines.First().Should().NotBeEmpty();
        }
    }
}
