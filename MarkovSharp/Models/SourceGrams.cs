using System.Linq;
using Newtonsoft.Json;

namespace MarkovSharp.Models
{
    public class SourceGrams<T>
    {
        public T[] Before { get; }

        public SourceGrams(params T[] args) => Before = args;

        /// <summary>Determines whether the specified <see cref="object" />, is equal to this instance.</summary>
        /// <param name="o">The <see cref="object" /> to compare with this instance.</param>
        /// <returns><c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object o)
        {
            var x = o as SourceGrams<T>;
            if (x == null) return false;

            return Before.OrderBy(a => a)
                .SequenceEqual(x.Before.OrderBy(a => a));
        }

        /// <summary>Returns a hash code for this instance.</summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return Before.Where(a => a != null && !a.Equals(default(T)))
                    .Aggregate(17, (current, member) => current * 23 + member.GetHashCode());
            }
        }

        /// <summary>Returns a <see cref="System.String" /> that represents this instance.</summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString() => JsonConvert.SerializeObject(Before);
    }
}
