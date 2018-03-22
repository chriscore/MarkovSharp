using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkovSharp.Components
{
    public class AlphabeticUnigramSelector<T> : IUnigramSelector<T>
    {
        public T SelectUnigram(IEnumerable<T> ngrams)
        {
            return ngrams
                .OrderBy(a => a)
                .FirstOrDefault();
        }
    }
}
