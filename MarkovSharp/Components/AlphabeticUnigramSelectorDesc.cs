using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkovSharp.Components
{
    public class AlphabeticUnigramSelectorDesc<T> : IUnigramSelector<T>
    {
        public T SelectUnigram(IEnumerable<T> ngrams)
        {
            return ngrams
                .OrderByDescending(a => a)
                .FirstOrDefault();
        }
    }
}
