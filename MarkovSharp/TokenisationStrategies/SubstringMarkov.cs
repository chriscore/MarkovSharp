using System.Collections.Generic;
using System.Linq;

namespace MarkovSharp.TokenisationStrategies
{
    public class SubstringMarkov : GenericMarkov<string, char?>
    {
        public SubstringMarkov()
            :this(2)
        { }

        public SubstringMarkov(int level)
            :base(level)
        { }

        public override IEnumerable<char?> SplitTokens(string phrase)
        {
            if (string.IsNullOrEmpty(phrase))
            {
                return new List<char?> { GetPrepadGram() };
            }
            
            return phrase.Select(c => new char?(c));
        }

        public override string RebuildPhrase(IEnumerable<char?> tokens)
        {
            var transformed = tokens
                .Where(t => t != null)
                .Select(t => t.Value).ToArray();
            
            return new string(transformed).Replace(new string(new char[] { GetPrepadGram().Value }), "");
        }

        public override char? GetTerminatorGram()
        {
            return null;
        }

        public override char? GetPrepadGram()
        {
            return '\0';
        }
    }
}
