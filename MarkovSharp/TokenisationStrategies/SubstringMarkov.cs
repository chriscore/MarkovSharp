using System.Collections.Generic;
using System.Linq;

namespace MarkovSharp.TokenisationStrategies
{
    public class SubstringMarkov : GenericMarkov<string, char?>
    {
        public SubstringMarkov()
            : this(2) { }

        public SubstringMarkov(int level)
            : base(level) { }

        public override IEnumerable<char?> SplitTokens(string phrase) => string.IsNullOrEmpty(phrase) 
            ? new List<char?> { GetPrepadGram() } 
            : phrase.Select(c => new char?(c));

        public override string RebuildPhrase(IEnumerable<char?> tokens)
        {
            var transformed = tokens
                .Where(t => t != null)
                .Select(t => t.Value).ToArray();

            var prepadGram = GetPrepadGram();
            return prepadGram != null 
                ? new string(transformed).Replace(new string(new[] { prepadGram.Value }), string.Empty) 
                : null;
        }

        public override char? GetTerminatorGram() => null;

        public override char? GetPrepadGram() => '\0';
    }
}
