using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkovSharp.Models
{
    public class StateStatistic<TGram>
    {
        internal StateStatistic(SourceGrams<TGram> state, IEnumerable<TGram> valuesAtState)
        {
            State = state.Before;
            var groupedValues = valuesAtState.GroupBy(x => x);
            Next = groupedValues.Select(a => new NgramStatistic<TGram>
            {
                Value = a.Key,
                Count = a.Count(),
                Probability = Math.Round(((double)a.Count() / (double)groupedValues.Sum(x => x.Count())) * 100, 2)
            }).OrderByDescending(x => x.Probability);
        }
        public TGram[] State { get; set; }
        public IEnumerable<NgramStatistic<TGram>> Next { get; set; }
    }
}
