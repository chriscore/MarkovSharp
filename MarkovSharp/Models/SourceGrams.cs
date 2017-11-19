using System.Linq;
using Newtonsoft.Json;

namespace MarkovSharp.Models
{
    public class SourceGrams<T>
    {
        public T[] Before { get; }

        public SourceGrams(params T[] args) => Before = args;

        public override bool Equals(object o)
        {
            var x = o as SourceGrams<T>;
            if (x == null) return false;

            return Before.OrderBy(a => a)
                .SequenceEqual(x.Before.OrderBy(a => a));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Before.Where(a => a != null && !a.Equals(default(T)))
                    .Aggregate(17, (current, member) => current * 23 + member.GetHashCode());
            }
        }

        public override string ToString() => JsonConvert.SerializeObject(Before);
    }
}
