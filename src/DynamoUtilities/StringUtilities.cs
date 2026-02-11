using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamoUtilities
{
    /// <summary>
    /// String utilities
    /// </summary>
    public class StringUtilities
    {
        /// <summary>
        /// Return the string with the first letter capitalized
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static string CapitalizeFirstLetter(string word)
        {
            if (string.IsNullOrEmpty(word))
                return word;

            return char.ToUpper(word[0]) + word.Substring(1);
        }
    }
}
