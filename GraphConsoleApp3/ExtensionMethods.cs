using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppRolesTesting
{
    public static class ExtensionMethods
    {
        public static string ToDebugString<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            return "{" + string.Join(",", dictionary.Select(kv => kv.Key + "=" + kv.Value).ToArray()) + "}";
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> source, params T[] tail)
        {
            return source.Concat(tail);
        }
    }
}
