using FluentAssertions;
using MarkovSharp.TokenisationStrategies;
using Xunit;

namespace MarkovSharp.Tests
{
    public class MarkovTests : BaseMarkovTests
    {
        [Fact]
        public void ParameterlessConstructorUsesLevel2()
        {
            var model = new StringMarkov();
            model.Level.Should().Be(2);
        }

        [Fact]
        public void LevelCanBeSetUsingConstructor()
        {
            var model = new StringMarkov(4);
            model.Level.Should().Be(4);
        }
    }
}
