using System;
using System.Collections.Generic;
using System.Linq;

namespace Analysis
{
    public class ResultsDictionary
    {
        /// <summary>
        /// Create a Results Dictionary from lists of keys and values.
        /// </summary>
        /// <param name="keys">A list of keys. The list must be equal in length to the list of values.</param>
        /// <param name="values">A list of list of analysis values. The list must be equal in length to the list of keys.</param>
        /// <returns>A dictionary.</returns>
        public static IDictionary<string, List<object>> ByKeysAndValues(
            IList<string> keys, IList<List<object>> values)
        {
            if (keys.Count() != values.Count())
            {
                throw new ArgumentException("The number of keys must match the number of values.");
            }

            var dict = new Dictionary<string, List<object>>();
            for (var i = 0; i < keys.Count(); i++)
            {
                dict.Add(keys[i], values[i]);
            }

            return dict;
        }
    }
}
