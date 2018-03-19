using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkovSharp.Models
{
    public class NgramStatistic<TNgram>
    {
        public TNgram Value { get; set; }
        public double Count { get; set; }
        public double Probability { get; set; }
    }
}
