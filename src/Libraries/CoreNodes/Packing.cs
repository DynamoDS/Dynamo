using Autodesk.DesignScript.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSCore
{
    [IsVisibleInDynamoLibrary(false)]
    public class PackFunctions
    {
        /// <summary>
        /// Packs data to a dictionary while manually handling lacing in a "Longest" strategy.
        /// 
        /// </summary>
        /// <param name="keys">Ordered list of keys to use in the output dictionary</param>
        /// <param name="isCollection">Ordered list of whether a value is a collection or not, which helps differenciating Lacing from a normal array value.</param>
        /// <param name="data">Ordered ArrayList or List of ArrayList defining the data to be packed.</param>
        /// <returns>List of Dictionary matching the ordered keys and data lists.</returns>
        public static object PackOutputAsDictionary(List<string> keys, List<bool> isCollection, [ArbitraryDimensionArrayImport] object data)
        {
            if (keys == null || keys.Count == 0 || isCollection == null || isCollection.Count == 0 || data == null)
            {
                return null;
            }

            var result = new List<Dictionary<string, object>>();

            var inputs = data as ArrayList;

            var outputCount = GetOuputCountFrom(isCollection, inputs);

            for (int outputIndex = 0; outputIndex < outputCount; ++outputIndex)
            {
                result.Add(CreateOuputDictionary(keys, isCollection, inputs, outputIndex));
            }

            return result;
        }

        private static Dictionary<string, object> CreateOuputDictionary(List<string> keys, List<bool> isCollection, ArrayList inputs, int outputIndex)
        {
            var dictionary = new Dictionary<string, object>();

            for (int inputIndex = 0; inputIndex < inputs.Count; ++inputIndex)
            {
                object value = inputs[inputIndex];

                if (value is ArrayList)
                {
                    var array = value as ArrayList;
                    if (!isCollection[inputIndex] || array[0] is ArrayList)
                    {
                        int lastIndex = array.Count - 1;
                        value = lastIndex < outputIndex ? array[lastIndex] : array[outputIndex];
                    }
                }

                dictionary[keys[inputIndex]] = value;
            }

            return dictionary;
        }

        /// <summary>
        /// Returns the number of dictionaries that should be output by the node.
        /// LacingStrategy "Longest" is used, which means the biggest list is the count of dictionaries that should be output.
        /// </summary>
        /// <param name="isCollection"></param>
        /// <param name="inputs"></param>
        /// <returns></returns>
        private static int GetOuputCountFrom(List<bool> isCollection, ArrayList inputs)
        {
            var subArrays = inputs.Cast<object>()
                .Where(i => i is ArrayList)
                .Cast<ArrayList>();

            if (!subArrays.Any()) return 1;

            return subArrays.Max(subArray =>
            {
                var index = inputs.IndexOf(subArray);
                if (!isCollection[index] || (subArray[0] is ArrayList))
                {
                    return subArray.Count;
                }

                return 1;
            });
        }
    }

    [IsVisibleInDynamoLibrary(false)]
    public class UnPackFunctions
    {
        /// <summary>
        /// Returns the value of dictionary at a given key.
        /// </summary>
        /// <param name="dictionary">Dictionary exposing all available values</param>
        /// <param name="key">The key of the value to retrieve</param>
        /// <returns>The value retrieved from the dictionary at the given key</returns>
        public static object UnPackOutputByKey(DesignScript.Builtin.Dictionary dictionary, string key)
        {
            if (dictionary == null) return null;

            return dictionary.ValueAtKey(key);
        }
    }
}
