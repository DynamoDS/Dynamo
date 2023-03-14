using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
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
            /// <param name="keys">Keys of dictionary</param>
            /// <param name="values">Values of dictionary</param>
            /// <returns name="dictionary">Dictionary from keys and values</returns>
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
            /// <returns name="keys">Keys of the dictionary</returns>
            /// <returns name="values">Values of the dictionary</returns>
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
            /// <returns name="keys">Keys of the Dictionary</returns>
            public IEnumerable<string> Keys => D.Keys;

            /// <summary>
            ///     Produces the values in a Dictionary.
            /// </summary>
            /// <returns name="values">Values of the dictionary</returns>
            public IEnumerable<object> Values
            {
                [return: ArbitraryDimensionArrayImport]
                get { return D.Values; }
            }

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
            /// <returns name="dictionary">New dictionary with the entries inserted</returns>
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
            /// <returns name="dictionary">New dictionary with keys removed</returns>
            /// <search>drop,delete</search>
            public Dictionary RemoveKeys(IList<string> keys)
            {
                return new Dictionary(D.RemoveRange(keys));
            }

            /// <summary>
            ///     Obtain the value at a specified key
            /// </summary>
            /// <param name="key">The key in the Dictionary to obtain value for</param>
            /// <returns name="value">Value at the specified key or null if it is not set</returns>
            /// <search>lookup,valueatkey,find</search>
            public object ValueAtKey(string key)
            {
                return D[key];
            }

            /// <summary>
            /// Returns a friendly string representation of the dictionary.
            /// </summary>
            /// <returns>String representation of the dictionary.</returns>
            public override string ToString()
            {
                var result = new StringBuilder();
                result.Append("{");
                foreach (var key in D.Keys)
                {
                    result.Append($"{key}:{D[key]},");
                }

                if (D.Any())
                {
                    result.Length -= 1;
                }

                result.Append("}");
                return result.ToString();
            }

        }
    }
}
