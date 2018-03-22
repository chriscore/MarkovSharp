using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkovSharp.Models
{
    public class NgramChainMatch<T>
    {
        internal NgramChainMatch(T ngram, bool matches)
        {
            Ngram = ngram;
            MatchesChain = matches;
        }

        public T Ngram { get; }
        public bool MatchesChain { get; }
    }
}
