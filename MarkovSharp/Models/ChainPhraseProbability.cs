using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkovSharp.Models
{
    public class ChainPhraseProbability<T>
    {
        public ChainPhraseProbability(List<NgramChainMatch<T>> raw)
        {
            Raw = raw;
        }

        /// <summary>
        /// The probability for this phrase to match the chain, a double between 0 and 1
        /// </summary>
        public double Probability => (double)Raw.Count(a => a.MatchesChain) / Raw.Count;

        /// <summary>
        /// The raw ngrams in the phrase, and whether they each match or not
        /// </summary>
        public List<NgramChainMatch<T>> Raw { get; }
    }
}
