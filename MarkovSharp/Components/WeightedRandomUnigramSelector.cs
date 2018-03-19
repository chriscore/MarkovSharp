using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkovSharp.Components
{
    public class WeightedRandomUnigramSelector<T> : IUnigramSelector<T>
    {
        public T SelectUnigram(IEnumerable<T> ngrams)
        {
            return ngrams.OrderBy(a => Guid.NewGuid()).FirstOrDefault();
        }
    }
}
