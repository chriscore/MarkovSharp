using System.Collections.Generic;

namespace MarkovSharp.TokenisationStrategies
{
    public class StringMarkov : GenericMarkov<string, string>
    {
        /// <summary>Initializes a new instance of the <see cref="StringMarkov"/> class.</summary>
        public StringMarkov()
            : this(2) { }

        /// <summary>Initializes a new instance of the <see cref="StringMarkov"/> class.</summary>
        /// <param name="level">The level.</param>
        public StringMarkov(int level)
            : base(level) { }

        /// <summary>Defines how to split the phrase to ngrams</summary>
        /// <param name="input">The phrase to split</param>
        public override IEnumerable<string> SplitTokens(string input) => input == null 
            ? (IEnumerable<string>) new List<string> {GetPrepadGram()} 
            : input.Split(' ');

        /// <summary>Defines how to join ngrams back together to form a phrase</summary>
        /// <param name="tokens"></param>
        public override string RebuildPhrase(IEnumerable<string> tokens) => string.Join(" ", tokens);

        public override string GetTerminatorGram() => null;

        public override string GetPrepadGram() => string.Empty;
    }
}
