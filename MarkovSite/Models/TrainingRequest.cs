using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MarkovSite.Models
{
    public class TrainingRequest
    {
        public int? ModelLevel { get; set; }
        public string TrainingData { get; set; }
    }
}