using System.Collections.Generic;

namespace MarkovSharp.TokenisationStrategies
{
    public class StringMarkov : GenericMarkov<string, string>
    {
        public StringMarkov()
            : this(2)
        { }

        public StringMarkov(int level)
            : base(level)
        { }

        public override IEnumerable<string> SplitTokens(string input)
        {
            if (input == null)
            {
                return new List<string>() { GetPrepadGram() };
            }

            return input?.Split(' ');
        }

        public override string RebuildPhrase(IEnumerable<string> tokens)
        {
            return string.Join(" ", tokens);
        }

        public override string GetTerminatorGram()
        {
            return null;
        }

        public override string GetPrepadGram()
        {
            return "";
        }
    }
}
