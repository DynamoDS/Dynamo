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

        /// <summary>
        /// Replaces 'i' in a file size string
        /// </summary>
        /// <param name="size">The file size string</param>
        /// <returns></returns>
        public static string SimplifyFileSizeUnit(string size)
        {
            if (string.IsNullOrEmpty(size))
            {
                return size;
            }

            return size.Replace("i", "");
        }
    }
}
