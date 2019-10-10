using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Autodesk.DesignScript.Runtime;

namespace DesignScript
{
    namespace Builtin
    {
        public class Dictionary
        {
            private readonly ImmutableDictionary<string, object> D;

            private Dictionary(ImmutableDictionary<string, object> dict)
            {
                this.D = dict;
            }

            /// <summary>
            ///     Produces a Dictionary with the supplied keys and values. The number of entries is 
            ///     the shorter of keys or values.
            /// </summary>
            /// <param name="keys">The string keys of the Dictionary</param>
            /// <param name="values">The values of the Dictionary</param>
            /// <returns name="dictionary">The result Dictionary</returns>
            /// <search>map,{},table</search>
            public static Dictionary ByKeysValues(IList<string> keys,
                [KeepReference] [ArbitraryDimensionArrayImport] IList<object> values)
            {
                var pairs = keys.Zip(values, (a, b) => new KeyValuePair<string, object>(a, b));

                return new Dictionary(ImmutableDictionary.Create<string, object>().AddRange(pairs));
            }

            /// <summary>
            ///     Produces the components of a Dictionary. The reverse of Dictionary.ByKeysValues.
            /// </summary>
            /// <returns name="keys">The keys of the Dictionary</returns>
            /// <returns name="values">The values of the Dictionary</returns>
            [MultiReturn(new[] {"keys", "values"})]
            public IDictionary<string, object> Components()
            {
                return new Dictionary<string, object>
                {
                    {"keys", D.Keys},
                    {"values", D.Values}
                };
            }

            /// <summary>
            ///     Produces the keys in a Dictionary.
            /// </summary>
            /// <returns name="keys">The keys of the Dictionary</returns>
            public IEnumerable<string> Keys => D.Keys;

            /// <summary>
            ///     Produces the values in a Dictionary.
            /// </summary>
            /// <returns name="values">The values of the Dictionary</returns>
            // [AllowRankReduction]
            // TODO: Consider adding this attribute in 3.0 (DYN-1697)
            public IEnumerable<object> Values => D.Values;

            /// <summary>
            ///     The number of key value pairs in a Dictionary.
            /// </summary>
            public int Count => D.Count;

            /// <summary>
            ///     Produce a new Dictionary with a list of keys set to the new values, possibly overwriting existing key-value pairs. 
            ///     These two lists are expected to be of the same length. If not, the shorter of the two bounds the number of insertions.
            /// </summary>
            /// <param name="keys">The keys in the Dictionary to set. If the same key already exists, the value at that key will be modified.</param>
            /// <param name="values">The corresponding values to insert.</param>
            /// <returns name="dictionary">A new Dictionary with the entries inserted.</returns>
            /// <search>insert,add</search>
            public Dictionary SetValueAtKeys(IList<string> keys,
                [KeepReference] [ArbitraryDimensionArrayImport] IList<object> values)
            {
                var pairs = keys.Zip(values, (a, b) => new KeyValuePair<string, object>(a, b));

                return new Dictionary(D.SetItems(pairs));
            }
            
            /// <summary>
            ///     Produce a new Dictionary with the given keys removed.
            /// </summary>
            /// <param name="keys">The key in the Dictionary to remove</param>
            /// <returns name="dictionary">A new Dictionary with the key removed</returns>
            /// <search>drop,delete</search>
            public Dictionary RemoveKeys(IList<string> keys)
            {
                return new Dictionary(D.RemoveRange(keys));
            }

            /// <summary>
            ///     Obtain the value at a specified key
            /// </summary>
            /// <param name="key">The key in the Dictionary to obtain.</param>
            /// <returns name="value">The value at the specified key or null if it is not set.</returns>
            /// <search>lookup,valueatkey,find</search>
            public object ValueAtKey(string key)
            {
                return D[key];
            }
        }
    }
}
