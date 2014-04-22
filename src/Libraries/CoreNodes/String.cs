using System;
using System.Collections;
using System.Globalization;
using Autodesk.DesignScript.Runtime;

namespace DSCore
{
    /// <summary>
    /// Methods for managing strings.
    /// </summary>
    public static class String
    {
        /// <summary>
        ///     Converts an object to a string representation.
        /// </summary>
        /// <param name="obj">Object to serialize.</param>
        /// <returns name="str">String representation of the object.</returns>
        /// <search>tostring,2string,number2string,numbertostring</search>
        public static string FromObject(object obj)
        {
            return obj.ToString();
        }

        /// <summary>
        ///     Converts a string to an integer or a double.
        /// </summary>
        /// <param name="str">String to be converted.</param>
        /// <returns name="number">Integer or double-type number.</returns>
        /// <search>2number,str2number,strtonumber,string2number,stringtonumber</search>
        public static object ToNumber(string str)
        {
            int i;
            double d;

            if (Int32.TryParse(str, out i))
                return i;
            else if (Double.TryParse(str, out d))
                return d;
            else
                throw new ArgumentException("Not a valid number.", "str");
        }

        /// <summary>
        ///     Concatenates multiple strings into a single string.
        /// </summary>
        /// <param name="strings">List of strings to concatenate.</param>
        /// <returns name="str">String made from list of strings.</returns>
        /// <search>concatenate</search>
        public static string Concat(params string[] strings)
        {
            return string.Concat(strings);
        }

        /// <summary>
        ///     Returns the number of characters contained in the given string.
        /// </summary>
        /// <param name="str">String to find the length of.</param>
        /// <returns name="length">Number of characters in the string.</returns>
        /// <search>count,size,characters</search>
        public static int Length(string str)
        {
            return str.Length;
        }

        /// <summary>
        ///     Divides a single string into a list of strings, with divisions
        ///     determined by the given separater strings.
        /// </summary>
        /// <param name="str">String to split up.</param>
        /// <param name="separaters">
        ///     Strings that, if present, determine the end and start of a split.
        /// </param>
        /// <returns name="strings">List of strings made from the input string.</returns>
        /// <search>divide,separaters</search>
        public static IList Split(string str, params string[] separaters)
        {
            return str.Split(separaters, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        ///     Concatenates multiple strings into a single string, inserting the given
        ///     separator between each joined string.
        /// </summary>
        /// <param name="separator">String to be inserted between joined strings.</param>
        /// <param name="strings">Strings to be joined into a single string.</param>
        /// <returns name="str">
        ///     A string made from the list of strings including the separator character.
        /// </returns>
        /// <search>join,separator</search>
        public static string Join(string separator, params string[] strings)
        {
            return string.Join(separator, strings);
        }

        /// <summary>
        /// Converts the given string to all uppercase characters.
        /// </summary>
        /// <param name="str">String to be made uppercase.</param>
        [IsVisibleInDynamoLibrary(false)]
        public static string ToUpper(string str)
        {
            return str.ToUpper();
        }

        /// <summary>
        ///     Converts the given string to all lowercase characters.
        /// </summary>
        /// <param name="str">String to be made lowercase.</param>
        [IsVisibleInDynamoLibrary(false)]
        public static string ToLower(string str)
        {
            return str.ToLower();
        }

        /// <summary>
        ///     Converts the given string to all uppercase characters or all
        ///     lowercase characters based on a boolean parameter.
        /// </summary>
        /// <param name="str">String to be made uppercase or lowercase.</param>
        /// <param name="upper">
        ///     True to convert to uppercase, false to convert to lowercase.
        /// </param>
        [IsVisibleInDynamoLibrary(false)]
        public static string StringCase(string str, bool upper)
        {
            return upper ? str.ToUpper() : str.ToLower();
        }

        /// <summary>
        ///     Retrieves a substring from the given string. The substring starts at the given
        ///     character position and has the given length.
        /// </summary>
        /// <param name="str">String to take substring of.</param>
        /// <param name="start">
        ///     Starting character position of the substring in the original string.
        /// </param>
        /// <param name="length">Number of characters in the substring.</param>
        /// <returns name="substring">Substring made from the original string.</returns>
        public static string Substring(string str, int start, int length)
        {
            return str.Substring(start, length);
        }

        /// <summary>
        ///     Determines if the given string contains the given substring.
        /// </summary>
        /// <param name="str">String to search in.</param>
        /// <param name="searchFor">Substring to search for.</param>
        /// <param name="ignoreCase">Whether or not comparison takes case into account.</param>
        /// <returns name="bool">Whether the string contains the substring.</returns>
        /// <search>test</search>
        public static bool Contains(string str, string searchFor, bool ignoreCase = false)
        {
            return !ignoreCase ? str.Contains(searchFor) : str.ToLowerInvariant().Contains(searchFor.ToLowerInvariant());
        }

        /// <summary>
        ///     Counts the number of non-overlapping occurrences of a substring inside a given string.
        /// </summary>
        /// <param name="str">String to search in.</param>
        /// <param name="searchFor">Substring to search for.</param>
        /// <param name="ignoreCase">Whether or not compaison takes case into account.</param>
        [IsVisibleInDynamoLibrary(false)]
        public static int CountOccurrences(string str, string searchFor, bool ignoreCase = false)
        {
            int count = 0, start = 0;
            while (start >= 0)
            {
                start = str.IndexOf(
                    searchFor,
                    start,
                    ignoreCase
                        ? StringComparison.InvariantCultureIgnoreCase
                        : StringComparison.InvariantCulture);
                count++;
            }
            return count;
        }

        /// <summary>
        ///     Replaces all occurrances of text in a string with other text.
        /// </summary>
        /// <param name="str">String to replace substrings in.</param>
        /// <param name="searchFor">Text to be replaced.</param>
        /// <param name="replaceWith">Text to replace with.</param>
        /// <returns name="str">String with replacements made.</returns>
        /// <search>replace</search>
        public static string Replace(string str, string searchFor, string replaceWith)
        {
            return str.Replace(searchFor, replaceWith);
        }

        /// <summary>
        ///     Determines if the given string ends with the given substring.
        /// </summary>
        /// <param name="str">String to search the end of.</param>
        /// <param name="searchFor">Substring to search the end for.</param>
        /// <param name="ignoreCase">Whether or not comparison takes case into account.</param>
        /// <returns name="bool">Whether the string ends with the substring.</returns>
        /// <search>test</search>
        public static bool EndsWith(string str, string searchFor, bool ignoreCase = false)
        {
            return str.EndsWith(searchFor, ignoreCase, CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Determines if the given string starts with the given substring.
        /// </summary>
        /// <param name="str">String to search the start of.</param>
        /// <param name="searchFor">Substring to search the start for.</param>
        /// <param name="ignoreCase">Whether or not comparison takes case into account.</param>
        /// <returns name="bool">Whether the string starts with the substring.</returns>
        /// <search>test,beginswith</search>
        public static bool StartsWith(string str, string searchFor, bool ignoreCase = false)
        {
            return str.StartsWith(searchFor, ignoreCase, CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Removes all whitespace from the start and end of the given string.
        /// </summary>
        /// <param name="str">String to trim.</param>
        /// <returns name="str">String with beginning and ending whitespaces removed.</returns>
        /// <search>trimstring,cleanstring,whitespace,blanks,spaces</search>
        public static string TrimWhitespace(string str)
        {
            return str.Trim();
        }

        /// <summary>
        ///     Removes all whitespace from the start of the given string.
        /// </summary>
        /// <param name="str">String to trim.</param>
        [IsVisibleInDynamoLibrary(false)]
        public static string TrimLeadingWhitespace(string str)
        {
            return str.TrimStart();
        }

        /// <summary>
        ///     Removes all whitespace from the end of the given string.
        /// </summary>
        /// <param name="str">String to trim.</param>
        [IsVisibleInDynamoLibrary(false)]
        public static string TrimTrailingWhitespace(string str)
        {
            return str.TrimEnd();
        }

        /// <summary>
        ///     Finds the zero-based index of the first occurance of a sub-string inside a string.
        ///     Returns -1 if no index could be found.
        /// </summary>
        /// <param name="str">A string to search in.</param>
        /// <param name="searchFor">Substring to search for.</param>
        /// <param name="ignoreCase">Whether or not comparison takes case into account.</param>
        /// <returns name="index">
        ///     Index of the first occurence of the substring or -1 if not found.
        /// </returns>
        /// <search>index,contains</search>
        public static int IndexOf(string str, string searchFor, bool ignoreCase = false)
        {
            return str.IndexOf(
                searchFor,
                ignoreCase
                    ? StringComparison.InvariantCultureIgnoreCase
                    : StringComparison.InvariantCulture);
        }

        /// <summary>
        ///     Finds the zero-based index of the last occurance of a sub-string inside a string.
        ///     Returns -1 if no index could be found.
        /// </summary>
        /// <param name="str">A string to search in.</param>
        /// <param name="searchFor">Substring to search for.</param>
        /// <param name="ignoreCase">Whether comparison takes case into account.</param>
        [IsVisibleInDynamoLibrary(false)]
        public static int LastIndexOf(string str, string searchFor, bool ignoreCase = false)
        {
            return str.LastIndexOf(
                searchFor,
                ignoreCase
                    ? StringComparison.InvariantCultureIgnoreCase
                    : StringComparison.InvariantCulture);
        }

        /// <summary>
        ///     Right-aligns the characters in the given string by padding them with spaces on the left,
        ///     for a specified total length.
        /// </summary>
        /// <param name="str">String to pad.</param>
        /// <param name="totalWidth">Total length of the string after padding.</param>
        /// <param name="padChar">Character to pad with, defaults to space.</param>
        [IsVisibleInDynamoLibrary(false)]
        public static string PadLeft(string str, int totalWidth, string padChar = " ")
        {
            if (padChar.Length != 1)
                throw new ArgumentException("padChar string must contain a single character.", "padChar");

            return str.PadLeft(totalWidth);
        }

        /// <summary>
        ///     Left-aligns the characters in the given string by padding them with spaces on the right,
        ///     for a specified total length.
        /// </summary>
        /// <param name="str">String to pad.</param>
        /// <param name="newWidth">Total length of the string after padding.</param>
        /// <param name="padChar">Character to pad with, defaults to space.</param>
        [IsVisibleInDynamoLibrary(false)]
        public static string PadRight(string str, int newWidth, string padChar = " ")
        {
            if (padChar.Length != 1)
                throw new ArgumentException("padChar string must contain a single character.", "padChar");

            return str.PadRight(newWidth, padChar[0]);
        }

        /// <summary>
        ///     Increases the width of a string by encasing the original characters with spaces on
        ///     either side.
        /// </summary>
        /// <param name="str">String to center.</param>
        /// <param name="newWidth">Total length of the string after centering.</param>
        /// <param name="padChar">Character to center with, defaults to space.</param>
        [IsVisibleInDynamoLibrary(false)]
        public static string Center(string str, int newWidth, string padChar = " ")
        {
            if (padChar.Length != 1)
                throw new ArgumentException("padChar string must contain a single character.", "padChar");

            var padHalf = (newWidth - str.Length)/ 2 + str.Length;

            return str.PadLeft(padHalf, padChar[0]).PadRight(newWidth - padHalf, padChar[0]);
        }

        /// <summary>
        ///     Inserts a string into another string at a given index.
        /// </summary>
        /// <param name="str">String to insert into.</param>
        /// <param name="index">Index to insert at.</param>
        /// <param name="toInsert">String to be inserted.</param>
        /// <returns name="str">String with inserted substring.</returns>
        /// <search>insertstring</search>
        public static string Insert(string str, int index, string toInsert)
        {
            return str.Insert(index, toInsert);
        }

        /// <summary>
        ///     Removes characters from a string.
        /// </summary>
        /// <param name="str">String to remove characters from.</param>
        /// <param name="startIndex">Index to start removal.</param>
        /// <param name="count">
        ///     Amount of characters to remove, by default will remove all characters from
        ///     the given startIndex to the end of the string.
        /// </param>
        [IsVisibleInDynamoLibrary(false)]
        public static string Remove(string str, int startIndex, int? count = null)
        {
            return str.Remove(startIndex, count ?? (str.Length - startIndex));
        }
    }
}
