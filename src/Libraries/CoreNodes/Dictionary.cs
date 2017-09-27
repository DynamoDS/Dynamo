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
        ///     Makes a dictionary using the supplied list of keys and values. The number of entries will be equal to 
        ///     the shorter of keys or values.
        /// </summary>
        /// <returns name="keys">The keys of the dictionary</returns>
        /// <returns name="values">The values of the dictionary</returns>
        /// <search>dictionary, {}</search>
        [IsVisibleInDynamoLibrary(true)]
        public static IDictionary ByKeysValues(IList keys, IList values)
        {
            return keys.Cast<object>().Zip(values.Cast<object>(), (a, b) =>
            {
                if (a is double)
                {
                    a = (int) Math.Floor((double) a);
                }

                if (a is string || a is int || a is String)
                {
                    return new KeyValuePair<object, object>(a, b);
                }

                throw new Exception("All keys must be strings or numbers");
            }).ToDictionary(a => a.Key, a => a.Value);
        }
    }
    #endregion
}