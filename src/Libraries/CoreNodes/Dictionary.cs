using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Autodesk.DesignScript.Runtime;

namespace DSCore
{
    // Doesn't implement IDictionary so suppresses FFI import
    public class Dictionary
    {
        [SupressImportIntoVM]
        private readonly ImmutableDictionary<object, object> D;

        // You can only use the StaticConstructor
        private Dictionary(ImmutableDictionary<object, object> dict)
        {
            this.D = dict;
        }

        #region private methods

        [SupressImportIntoVM]
        private static void AssertIsKeyType(object k)
        {
            if (k is string || k is int || k is long) return;
            throw new Exception("Dictionary keys must be strings or numbers");
        }

        [SupressImportIntoVM]
        private static object CoerceKey(object k)
        {
            if (k is double)
            {
                return (int)Math.Floor((double)k);
            }

            return k;
        }

        #endregion

        #region public methods

        /// <summary>
        ///     Produces a Dictionary with the supplied keys and values. The number of entries is 
        ///     the shorter of keys or values.
        /// </summary>
        /// <param name="keys">The keys of the Dictionary</param>
        /// <param name="values">The values of the Dictionary</param>
        /// <returns name="dictionary">The result Dictionary</returns>
        /// <search>map,{},table</search>
        [IsVisibleInDynamoLibrary(true)]
        public static Dictionary ByKeysValues(IList<object> keys, IList<object> values)
        {
            var pairs = keys.Cast<object>().Zip(values.Cast<object>(), (a, b) =>
            {
                return new KeyValuePair<object, object>(a, b);
            });

            return new Dictionary(ImmutableDictionary.Create<object, object>().AddRange(pairs));
        }

        /// <summary>
        ///     Produces the components of a Dictionary. The reverse of Dictionary.ByKeysValues.
        /// </summary>
        /// <returns name="keys">The keys of the dictionary</returns>
        /// <returns name="values">The values of the dictionary</returns>
        [MultiReturn(new[] { "keys", "values" })]
        [IsVisibleInDynamoLibrary(true)]
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
        [IsVisibleInDynamoLibrary(true)]
        public IEnumerable<object> Keys
        {
            get { return D.Keys; }
        }

        /// <summary>
        ///     Produces the values in a Dictionary.
        /// </summary>
        /// <returns name="values">The values of the dictionary</returns>
        [IsVisibleInDynamoLibrary(true)]
        public IEnumerable<object> Values
        {
            get { return D.Values; }
        }

        /// <summary>
        ///     Produce a new Dictionary with a new entry set to the input value
        /// </summary>
        /// <param name="key">The key in the dictionary to set. If the same key already exists, the value at that key will be modified.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns name="dictionary">A new Dictionary with the entry inserted.</returns>
        [IsVisibleInDynamoLibrary(true)]
        public Dictionary SetValueAtKey(object key, [ArbitraryDimensionArrayImport] object value)
        {
            key = CoerceKey(key);
            AssertIsKeyType(key);

            return new Dictionary(D.SetItem(key, value));
        }

        /// <summary>
        ///     Produce a new Dictionary with a new entry set to the input value
        /// </summary>
        /// <param name="key">The keys in the dictionary to set</param>
        /// <returns name="dictionary">A new dictionary with </returns>
        [IsVisibleInDynamoLibrary(true)]
        public Dictionary RemoveValueAtKey(object key)
        {
            key = CoerceKey(key);
            AssertIsKeyType(key);

            return new Dictionary(D.Remove(key));
        }
        #endregion
    }
}
