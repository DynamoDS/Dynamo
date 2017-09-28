#region
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using Autodesk.DesignScript.Runtime;
using DSCore.Properties;

#endregion

namespace DSCore
{
    #region public methods
    /// <summary>
    ///     Methods for creating and manipulating Lists.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public static class Dictionary
    {
        /// <summary>
        ///     Produces a Dictionary with the supplied keys and values. The number of entries is 
        ///     the shorter of keys or values.
        /// </summary>
        /// <param name="keys">The keys of the Dictionary</param>
        /// <param name="values">The values of the Dictionary</param>
        /// <returns name="dictionary">The result Dictionary</returns>
        /// <search>map,{},table</search>
        [IsVisibleInDynamoLibrary(true)]
        public static IDictionary ByKeysValues(IList keys, IList values)
        {
            return keys.Cast<object>().Zip(values.Cast<object>(), (a, b) =>
            {
                if (a is double)
                {
                    a = (int) Math.Floor((double) a);
                }

                if (a is string || a is int || a is long)
                {
                    return new KeyValuePair<object, object>(a, b);
                }

                throw new Exception("All keys must be strings or numbers");
            }).ToDictionary(a => a.Key, a => a.Value);
        }

        /// <summary>
        ///     Produces the components of a Dictionary. The reverse of Dictionary.ByKeysValues.
        /// </summary>
        /// <param name="dictionary">The input dictionary</param>
        /// <returns name="keys">The keys of the dictionary</returns>
        /// <returns name="values">The values of the dictionary</returns>
        [MultiReturn(new[] { "keys", "values" })]
        [IsVisibleInDynamoLibrary(true)]
        public static IDictionary Components(IDictionary dictionary)
        {
            return new Dictionary<string, object>
            {
                { "keys", dictionary.Keys },
                { "values", dictionary.Values }
            };
        }

        /// <summary>
        ///     Produces the keys in a Dictionary.
        /// </summary>
        /// <param name="dictionary">The input dictionary</param>
        /// <returns name="keys">The keys of the dictionary</returns>
        [IsVisibleInDynamoLibrary(true)]
        public static ICollection Keys(IDictionary dictionary)
        {
            return dictionary.Keys;
        }

        /// <summary>
        ///     Produces the values in a Dictionary.
        /// </summary>
        /// <param name="dictionary">The input dictionary</param>
        /// <returns name="keys">The values of the dictionary</returns>
        [IsVisibleInDynamoLibrary(true)]
        public static ICollection Values(IDictionary dictionary)
        {
            return dictionary.Values;
        }
    }
    #endregion
}