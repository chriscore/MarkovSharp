using System.Collections.Generic;

namespace MarkovSharp.TokenisationStrategies
{
    public class StringMarkov : GenericMarkov<string, string>
    {
        public StringMarkov()
            : this(2) { }

        public StringMarkov(int level)
            : base(level) { }

        public override IEnumerable<string> SplitTokens(string input) => input == null 
            ? (IEnumerable<string>) new List<string> {GetPrepadGram()} 
            : input.Split(' ');

        public override string RebuildPhrase(IEnumerable<string> tokens) => string.Join(" ", tokens);

        public override string GetTerminatorGram() => null;

        public override string GetPrepadGram() => string.Empty;
    }
}
