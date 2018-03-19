using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkovSharp.Models
{
    public class MarkovChain<T>
    {
        public MarkovChain()
        {
            ChainDictionary = new ConcurrentDictionary<NgramContainer<T>, List<T>>();
        }

        internal ConcurrentDictionary<NgramContainer<T>, List<T>> ChainDictionary { get; }
        private readonly object _lockObj = new object();

        /// <summary>
        /// The number of states in the chain
        /// </summary>
        public int Count => ChainDictionary.Count;

        internal bool Contains(NgramContainer<T> key)
        {
            return ChainDictionary.ContainsKey(key);
        }
        
        /// <summary>
        /// Add a TGram to the markov models store with a composite key of the previous [Level] number of TGrams
        /// </summary>
        /// <param name="key">The composite key under which to add the TGram value</param>
        /// <param name="value">The value to add to the store</param>
        internal void AddOrCreate(NgramContainer<T> key, T value)
        {
            lock (_lockObj)
            {
                if (!ChainDictionary.ContainsKey(key))
                {
                    ChainDictionary.TryAdd(key, new List<T> { value });
                }
                else
                {
                    ChainDictionary[key].Add(value);
                }
            }
        }

        internal List<T> GetValuesForKey(NgramContainer<T> key)
        {
            return ChainDictionary[key];
        }

        internal IEnumerable<StateStatistic<T>> GetStatistics()
        {
            var stats = ChainDictionary.Keys.Select(a => new StateStatistic<T>(a, ChainDictionary[a]))
                .OrderByDescending(a => a.Next.Sum(x => x.Count));

            return stats;
        }
    }
}
