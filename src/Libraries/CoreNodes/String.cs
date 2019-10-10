using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Dynamo.Graph.Nodes;

namespace DSCore
{
    /// <summary>
    /// Methods for managing strings.
    /// </summary>
    public static class String
    {
        // It has been moved to String.FromObject UI node, which is compiled 
        // to built-in function __ToStringFromObject().
        [NodeObsolete("FromObjectObsolete", typeof(Properties.Resources))]
        public static string FromObject(object obj)
        {
            return obj.ToString();
        }

        /// <summary>
        ///     Converts a string to an integer or a double.
        /// </summary>
        /// <param name="str">String to be converted.</param>
        /// <returns name="number">Integer or double-type number.</returns>
        /// <search>2number,str2number,strtonumber,string2number,stringtonumber,int,double,cast</search>
        public static object ToNumber(string str)
        {
            int i;
            double d;

            if (Int32.TryParse(str, out i))
                return i;
            if (Double.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out d))
                return d;
            throw new ArgumentException(Properties.Resources.StringToNumberInvalidNumberMessage, "str");
        }

        /// <summary>
        ///     Concatenates multiple strings into a single string.
        /// </summary>
        /// <param name="strings">List of strings to concatenate.</param>
        /// <returns name="str">String made from list of strings.</returns>
        /// <search>concatenate,join,combine strings</search>
        public static string Concat(params string[] strings)
        {
            return string.Concat(strings);
        }

        /// <summary>
        ///     Returns the number of characters contained in the given string.
        /// </summary>
        /// <param name="str">String to find the length of.</param>
        /// <returns name="length">Number of characters in the string.</returns>
        /// <search>count,size,characters,chars,length,sizeof</search>
        public static int Length(string str)
        {
            return str.Length;
        }

        /// <summary>
        ///     Divides a single string into a list of strings, with divisions
        ///     determined by the given separator strings.
        /// </summary>
        /// <param name="str">String to split up.</param>
        /// <param name="separators">
        ///     Strings that, if present, determine the end and start of a split.
        /// </param>
        /// <returns name="strings">List of strings made from the input string.</returns>
        /// <search>divide,separator,delimiter,cut,csv,comma,</search>
        public static string[] Split(string str, params string[] separators)
        {
            separators = separators.Select(s => s == "\n" ? Environment.NewLine : s).ToArray(); // converts all \n in separater array to Environment Newline (i.e. \r\n)
            str = Regex.Replace(str, "(?<!\r)\n", Environment.NewLine); // converts all \n in String str to Environment.NewLine (i.e. '\r\n')
            
            return separators.Contains("")
                ? str.ToCharArray().Select(char.ToString).ToArray()
                : str.Split(separators, StringSplitOptions.RemoveEmptyEntries);
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
        /// <search>join,separator,build csv,concat,construct</search>
        public static string Join(string separator, params string[] strings)
        {
            return string.Join(separator, strings);
        }

        /// <summary>
        ///     Converts the given string to all uppercase characters.
        /// </summary>
        /// <param name="str">String to be made uppercase.</param>
        /// <returns name="str">Uppercase string.</returns>
        /// <search>2uppercase,to uppercase,touppercase,uppercase</search>
        public static string ToUpper(string str)
        {
            return str.ToUpper();
        }

        /// <summary>
        ///     Converts the given string to all lowercase characters.
        /// </summary>
        /// <param name="str">String to be made lowercase.</param>
        /// <returns name="str">Lowercase string.</returns>
        /// <search>2lowercase,to lowercase,tolowercase,lowercase</search>
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
        /// <returns name="str">String with converted case.</returns>
        /// <search>
        ///     2lowercase,to lowercase,tolowercase,lowercase,
        ///     2uppercase,to uppercase,touppercase,uppercase
        /// </search>
        public static string ChangeCase(string str, bool upper)
        {
            return upper ? str.ToUpper() : str.ToLower();
        }

        /// <summary>
        ///     Retrieves a substring from the given string. The substring starts at the given
        ///     character position and has the given length.
        /// </summary>
        /// <param name="str">String to take substring of.</param>
        /// <param name="startIndex">
        ///     Starting character position of the substring in the original string.
        /// </param>
        /// <param name="length">Number of characters in the substring.</param>
        /// <returns name="substring">Substring made from the original string.</returns>
		///  <search>subset,get string,part,smaller string</search>
        public static string Substring(string str, int startIndex, int length)
        {
            if (startIndex < 0)
            {
                startIndex += str.Length;
            }
            bool reverse = false;
            if (length < 0)
            {
                reverse = true;
                startIndex += length;
                length *= -1;
            }
            var result = str.Substring(startIndex, length);
            return reverse ? new string(result.Reverse().ToArray()) : result;
        }

        /// <summary>
        ///     Determines if the given string contains the given substring.
        /// </summary>
        /// <param name="str">String to search in.</param>
        /// <param name="searchFor">Substring to search for.</param>
        /// <param name="ignoreCase">Whether or not comparison takes case into account.</param>
        /// <returns name="bool">Whether the string contains the substring.</returns>
        /// <search>test,within,in,is in,part of</search>
        public static bool Contains(string str, string searchFor, bool ignoreCase = false)
        {
            return !ignoreCase ? str.Contains(searchFor) : str.ToLowerInvariant().Contains(searchFor.ToLowerInvariant());
        }

        /// <summary>
        ///     Counts the number of non-overlapping occurrences of a substring inside a given string.
        /// </summary>
        /// <param name="str">String to search in.</param>
        /// <param name="searchFor">Substring to search for.</param>
        /// <param name="ignoreCase">Whether or not comparison takes case into account.</param>
        /// <returns name="count">Number of non-overlapping occurrences of the substring in the string.</returns>
        /// <search>count,substring,count occurrences,numberof,search,find,within</search>
        public static int CountOccurrences(string str, string searchFor, bool ignoreCase = false)
        {
            if (searchFor == string.Empty)
                return str.Length + 1;

            int count = 0, start = 0;
            while (true)
            {
                start = str.IndexOf(
                    searchFor,
                    start,
                    ignoreCase
                        ? StringComparison.InvariantCultureIgnoreCase
                        : StringComparison.InvariantCulture);

                if (start < 0)
                    break;

                count++;
                start += searchFor.Length;
            }
            return count;
        }

        /// <summary>
        ///     Replaces all occurrences of text in a string with other text.
        /// </summary>
        /// <param name="str">String to replace substrings in.</param>
        /// <param name="searchFor">Text to be replaced.</param>
        /// <param name="replaceWith">Text to replace with.</param>
        /// <returns name="str">String with replacements made.</returns>
        /// <search>replace,overwrite,override,find and replace</search>
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
        /// <search>test,does end,last,str end,terminated</search>
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
        /// <search>test,beginswith,start,string start,front</search>
        public static bool StartsWith(string str, string searchFor, bool ignoreCase = false)
        {
            return str.StartsWith(searchFor, ignoreCase, CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Removes all whitespace from the start and end of the given string.
        /// </summary>
        /// <param name="str">String to trim.</param>
        /// <returns name="str">String with beginning and ending whitespaces removed.</returns>
        /// <search>trimstring,cleanstring,whitespace,blanks,spaces,string trim</search>
        public static string TrimWhitespace(string str)
        {
            return str.Trim();
        }

        /// <summary>
        ///     Removes all whitespace from the start of the given string.
        /// </summary>
        /// <param name="str">String to trim.</param>
        /// <returns name="str">String with leading white spaces removed.</returns>
        /// <search>trim string,clean string,trim leading whitespaces,string trim</search>
        public static string TrimLeadingWhitespace(string str)
        {
            return str.TrimStart();
        }

        /// <summary>
        ///     Removes all whitespace from the end of the given string.
        /// </summary>
        /// <param name="str">String to trim.</param>
        /// <returns name="str">String with white spaces at end removed.</returns>
        /// <search>trim string,clean string,trim trailing whitespaces,string trim</search>
        public static string TrimTrailingWhitespace(string str)
        {
            return str.TrimEnd();
        }

        /// <summary>
        ///     Finds the zero-based index of the first occurrence of a sub-string inside a string.
        ///     Returns -1 if no index could be found.
        /// </summary>
        /// <param name="str">A string to search in.</param>
        /// <param name="searchFor">Substring to search for.</param>
        /// <param name="ignoreCase">Whether or not comparison takes case into account.</param>
        /// <returns name="index">
        ///     Index of the first occurrence of the substring or -1 if not found.
        /// </returns>
        /// <search>index of,find substring,where,search</search>
        public static int IndexOf(string str, string searchFor, bool ignoreCase = false)
        {
            return str.IndexOf(
                searchFor,
                ignoreCase
                    ? StringComparison.InvariantCultureIgnoreCase
                    : StringComparison.InvariantCulture);
        }

        public static int[] AllIndicesOf(string str, string searchFor, bool ignoreCase = false)
        {
            var indices = new List<int>();
            if (searchFor == System.String.Empty) return indices.ToArray();

            for (int index = 0; ; index += searchFor.Length)
            {
                index = str.IndexOf(searchFor, index, ignoreCase
                    ? StringComparison.InvariantCultureIgnoreCase
                    : StringComparison.InvariantCulture);
                if (index == -1)
                    break;
                indices.Add(index);
            }
            return indices.ToArray();
        }

        /// <summary>
        ///     Finds the zero-based index of the last occurrence of a sub-string inside a string.
        ///     Returns -1 if no index could be found.
        /// </summary>
        /// <param name="str">A string to search in.</param>
        /// <param name="searchFor">Substring to search for.</param>
        /// <param name="ignoreCase">Whether comparison takes case into account.</param>
        /// <returns name="index">
        ///     Index of the last occurrence of the substring or -1 if not found.
        /// </returns>
		/// <search>last index of,find substring,where,search</search>
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
        /// <param name="newWidth">Total length of the string after padding.</param>
        /// <param name="padChars">Character to pad with, defaults to space.</param>
        /// <returns name="str">
        ///     Strings right-aligned by padding with leading whitespaces for a specified total length.
        /// </returns>
        /// <search>pad left,right align,right-align,pad,string space,whitespace</search>
        public static string PadLeft(string str, int newWidth, string padChars = " ")
        {
            return new string(padChars.Cycle().Take(newWidth - str.Length).Concat(str).ToArray());
        }

        /// <summary>
        ///     Left-aligns the characters in the given string by padding them with spaces on the right,
        ///     for a specified total length.
        /// </summary>
        /// <param name="str">String to pad.</param>
        /// <param name="newWidth">Total length of the string after padding.</param>
        /// <param name="padChars">Character to pad with, defaults to space.</param>
        /// <returns name="str">
        ///     Strings left-aligned by padding with trailing whitespaces for a specified total length.
        /// </returns>
        /// <search>pad right,left align,left-align,pad string space,whitespace</search>
        public static string PadRight(string str, int newWidth, string padChars = " ")
        {
            return new string(
                Enumerable.Concat(str, padChars.Cycle().Take(newWidth - str.Length)).ToArray());
        }

        /// <summary>
        ///     Increases the width of a string by encasing the original characters with spaces on
        ///     either side.
        /// </summary>
        /// <param name="str">String to center.</param>
        /// <param name="newWidth">Total length of the string after centering.</param>
        /// <param name="padChars">Character to center with, defaults to space.</param>
        /// <returns name="str">
        ///     Strings center-aligned by padding them with leading and trailing
        ///     whitespaces for a specified total length.
        /// </returns>
        /// <search>center align,center-align,centered,whitespace,expand string,surround</search>
        public static string Center(string str, int newWidth, string padChars = " ")
        {
            var padHalf = (newWidth - str.Length)/2;

            return
                new string(
                    padChars.Cycle()
                        .Take(padHalf)
                        .Concat(str)
                        .Concat(padChars.Cycle().Take(newWidth - str.Length - padHalf))
                        .ToArray());
        }

        #region Padding Helpers

        private static IEnumerable<T> Cycle<T>(this IEnumerable<T> enumerable)
        {
            while (true)
            {
                foreach (var item in enumerable)
                    yield return item;
            }
        }

        #endregion

        /// <summary>
        ///     Inserts a string into another string at a given index.
        /// </summary>
        /// <param name="str">String to insert into.</param>
        /// <param name="index">Index to insert at.</param>
        /// <param name="toInsert">String to be inserted.</param>
        /// <returns name="str">String with inserted substring.</returns>
        /// <search>insertstring,insert string</search>
        public static string Insert(string str, int index, string toInsert)
        {
            return str.Insert(index, toInsert);
        }

        /// <summary>
        ///     Removes characters from a string.
        /// </summary>
        /// <param name="str">String to remove characters from.</param>
        /// <param name="startIndex">Index at which to start removal.</param>
        /// <param name="count">
        ///     Amount of characters to remove, by default will remove all characters from
        ///     the given startIndex to the end of the string.
        ///     Note: if the Count is negative, the removal process goes from right to left.
        /// </param>
        /// <returns name="str">String with characters removed.</returns>
		/// <search>delete,rem,shorten</search>
        public static string Remove(string str, int startIndex, int? count = null)
        {
            if (startIndex < 0)
            {
                startIndex += str.Length;
            }

            var _count = count ?? str.Length - startIndex;

            if (_count < 0)
            {
                // The input count is negative means the removal process is required operate from right to left.
                // However, the removal process in this function always operates rightwards (left to right).
                // Therefore, a conversion for start index needs to be done in order to change
                // from leftwards removal (right to left) to rightwards removal (left to right).
                startIndex = startIndex + _count + 1; 
                _count *= -1;
            }
            
            if(_count > str.Length)
            {
                throw new ArgumentOutOfRangeException("count", Properties.Resources.StringRemoveCountOutOfRangeMessage);
            }

            if (startIndex == 0 && str.Length == 0)
            {
                return string.Empty;
            }

            if (startIndex >= str.Length || startIndex < 0) 
            {
                // startIndex of an array must be within the string length. 
                // If after the conversion of negative startIndex, startIndex is still
                // less than zero, ArgumentOutOfRangeException is occurred.

                throw new ArgumentOutOfRangeException("startIndex", Properties.Resources.StringRemoveStartIndexOutOfRangeMessage);

            }

            return str.Remove(startIndex, _count);
        }
    }
}
