using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkovSharp.TokenisationStrategies
{
    public class SubstringMarkov : GenericMarkov<string, char?>
    {
        public SubstringMarkov(int level = 2)
            :base(level)
        { }

        public override IEnumerable<char?> SplitTokens(string phrase)
        {
            if (string.IsNullOrEmpty(phrase))
            {
                return new List<char?> { GetPrepadUnigram() };
            }
            
            return phrase.Select(c => new char?(c));
        }

        public override string RebuildPhrase(IEnumerable<char?> tokens)
        {
            var transformed = tokens
                .Where(t => t != null)
                .Select(t => t.Value).ToArray();
            
            return new string(transformed).Replace(new string(new char[] { GetPrepadUnigram().Value }), "");
        }

        public override char? GetTerminatorUnigram()
        {
            return null;
        }

        public override char? GetPrepadUnigram()
        {
            return '\0';
        }
    }
}
