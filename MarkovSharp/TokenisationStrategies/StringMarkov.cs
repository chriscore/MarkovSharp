using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkovSharp.TokenisationStrategies
{
    public class StringMarkov : GenericMarkov<string, string>
    {
        public StringMarkov(int level = 2)
            : base(level)
        { }

        public override IEnumerable<string> SplitTokens(string input)
        {
            if (input == null)
            {
                return new List<string> { GetPrepadUnigram() };
            }

            input = input.Trim();
            return input.Split(' ');
        }

        public override string RebuildPhrase(IEnumerable<string> tokens)
        {
            return string.Join(" ", tokens);
        }

        public override string GetTerminatorUnigram()
        {
            return null;
        }

        public override string GetPrepadUnigram()
        {
            return "";
        }
    }
}
