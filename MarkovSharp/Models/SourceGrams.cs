using System.Linq;
using System.Web.Script.Serialization;

namespace MarkovSharp.Models
{
    public class SourceGrams<T>
    {
        public T[] Before { get; set; }

        public SourceGrams(params T[] args)
        {
            Before = args;
        }

        public override bool Equals(object o)
        {
            var x = o as SourceGrams<T>;

            if (x == null && this != null)
            {
                return false;
            }

            var equals = Before.OrderBy(a => a).ToArray().SequenceEqual(x.Before.OrderBy(a => a).ToArray());
            return equals;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                var defaultVal = default(T);
                foreach (var member in Before.Where(a => a != null && !a.Equals(defaultVal)))
                {
                    hash = hash * 23 + member.GetHashCode();
                }
                return hash;
            }
        }

        public override string ToString()
        {
            return new JavaScriptSerializer().Serialize(Before);
        }
    }
}
