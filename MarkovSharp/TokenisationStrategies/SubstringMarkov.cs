using System.Collections.Generic;
using System.Linq;

namespace MarkovSharp.TokenisationStrategies
{
    public class SubstringMarkov : GenericMarkov<string, char?>
    {
        /// <summary>Initializes a new instance of the <see cref="SubstringMarkov"/> class.</summary>
        public SubstringMarkov()
            : this(2) { }

        /// <summary>Initializes a new instance of the <see cref="SubstringMarkov"/> class.</summary>
        /// <param name="level">The level.</param>
        public SubstringMarkov(int level)
            : base(level) { }

        /// <summary>Defines how to split the phrase to ngrams</summary>
        public override IEnumerable<char?> SplitTokens(string phrase) => string.IsNullOrEmpty(phrase) 
            ? new List<char?> { GetPrepadGram() } 
            : phrase.ToCharArray().Select<char, char?>(c => new char?(c));

        /// <summary>Defines how to join ngrams back together to form a phrase</summary>
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
