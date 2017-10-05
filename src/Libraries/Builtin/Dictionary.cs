using Autodesk.DesignScript.Runtime;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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
            public static Dictionary ByKeysValues(IList<string> keys, [KeepReference] [ArbitraryDimensionArrayImport] IList<object> values)
            {
                var pairs = keys.Cast<string>().Zip(values.Cast<object>(), (a, b) =>
                {
                    return new KeyValuePair<string, object>(a, b);
                });

                return new Dictionary(ImmutableDictionary.Create<string, object>().AddRange(pairs));
            }

            /// <summary>
            ///     Produces the components of a Dictionary. The reverse of Dictionary.ByKeysValues.
            /// </summary>
            /// <returns name="keys">The keys of the dictionary</returns>
            /// <returns name="values">The values of the dictionary</returns>
            [MultiReturn(new[] { "keys", "values" })]
            public IDictionary<string, object> Components()
            {
                return new Dictionary<string, object>
            {
                { "keys", D.Keys },
                { "values", D.Values }
            };
            }

            /// <summary>
            ///     Produces the keys in a Dictionary.
            /// </summary>
            /// <returns name="keys">The keys of the dictionary</returns>
            public IEnumerable<string> Keys
            {
                get { return D.Keys; }
            }

            /// <summary>
            ///     Produces the values in a Dictionary.
            /// </summary>
            /// <returns name="values">The values of the dictionary</returns>
            [AllowRankReduction]
            public IEnumerable<object> Values
            {
                get { return D.Values; }
            }

            /// <summary>
            ///     Produce a new Dictionary with the provided key set to a given value, possibly overwriting an existing key-value pair.
            /// </summary>
            /// <param name="key">The key in the dictionary to set. If the same key already exists, the value at that key will be modified.</param>
            /// <param name="value">The value to insert.</param>
            /// <returns name="dictionary">A new Dictionary with the entry inserted.</returns>
            public Dictionary SetValueAtKey(string key, [KeepReference] [ArbitraryDimensionArrayImport] object value)
            {
                return new Dictionary(D.SetItem(key, value));
            }

            /// <summary>
            ///     Produce a new Dictionary with a list of keys set to the new values, possibly overwriting existing key-value pairs. 
            ///     These two lists are expected to be of the same length. If not, the shorter of the two bounds the number of insertions.
            /// </summary>
            /// <param name="key">The keys in the dictionary to set. If the same key already exists, the value at that key will be modified.</param>
            /// <param name="value">The corresponding values to insert.</param>
            /// <returns name="dictionary">A new Dictionary with the entries inserted.</returns>
            public Dictionary SetValueAtKeys(IList<string> keys, [KeepReference] [ArbitraryDimensionArrayImport] IList<object> values)
            {
                var pairs = keys.Cast<string>().Zip(values.Cast<object>(), (a, b) =>
                {
                    return new KeyValuePair<string, object>(a, b);
                });

                return new Dictionary(D.SetItems(pairs));
            }

            /// <summary>
            ///     Produce a new Dictionary with the given key removed.
            /// </summary>
            /// <param name="key">The key in the Dictionary to remove.</param>
            /// <returns name="dictionary">A new Dictionary with the key removed.</returns>
            public Dictionary RemoveValueAtKey(string key)
            {
                return new Dictionary(D.Remove(key));
            }

            /// <summary>
            ///     Produce a new Dictionary with the given keys removed.
            /// </summary>
            /// <param name="key">The key in the Dictionary to remove</param>
            /// <returns name="dictionary">A new Dictionary with the key removed</returns>
            public Dictionary RemoveValueAtKeys(IList<string> keys)
            {
                return new Dictionary(D.RemoveRange(keys));
            }

            /// <summary>
            ///     Obtain the value at a specified key
            /// </summary>
            /// <param name="key">The key in the Dictionary to obtain.</param>
            /// <returns name="value">The value at the specified key or null if it is not set.</returns>
            public object ValueAtKey(string key)
            {
                return D[key];
            }
        }
    }
}
