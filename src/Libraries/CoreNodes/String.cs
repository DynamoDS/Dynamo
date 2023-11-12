using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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
        /// <param name="string">String to be converted</param>
        /// <returns name="number">Integer or double-type number</returns>
        /// <search>2number,str2number,strtonumber,string2number,stringtonumber,int,double,cast</search>
        public static object ToNumber(string @string)
        {
            int i;
            double d;

            if (Int32.TryParse(@string, out i))
                return i;
            if (Double.TryParse(@string, NumberStyles.Number, CultureInfo.InvariantCulture, out d))
                return d;
            throw new ArgumentException(Properties.Resources.StringToNumberInvalidNumberMessage, "str");
        }

        /// <summary>
        /// Get all of the number strings from the target string as a string
        /// </summary>
        /// <param name="string">Target string to be get</param>
        /// <returns name="str">Number In string</returns>
        /// <search>getnumber,tonumber,strtonumber,numberinstring,string2number,stringtonumber,int,double,cast</search> 
        public static string GetNumber(string @string)
        {
            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(@string)) return sb.ToString();
            for (int i = 0; i < @string.Length; i++)
            {
                if (char.IsDigit(@string[i]))
                    sb.Append(@string[i]);
            }

            return sb.ToString();
        }
        /// <summary>
        ///     Concatenates multiple strings into a single string.
        /// </summary>
        /// <param name="strings">List of strings to concatenate.</param>
        /// <returns name="string">String made from list of strings.</returns>
        /// <search>concatenate,join,combine strings</search>
        public static string Concat(params string[] strings)
        {
            return string.Concat(strings);
        }

        /// <summary>
        ///     Returns the number of characters contained in the given string.
        /// </summary>
        /// <param name="string">String to find the length of</param>
        /// <returns name="int">Number of characters in the string</returns>
        /// <search>count,size,characters,chars,length,sizeof</search>
        public static int Length(string @string)
        {
            return @string.Length;
        }

        /// <summary>
        ///     Divides a single string into a list of strings, with divisions
        ///     determined by the given separator strings.
        /// </summary>
        /// <param name="string">String to split up</param>
        /// <param name="separators">
        ///     Strings that, if present, determine the end and start of a split.
        /// </param>
        /// <returns name="strings">List of strings made from the input string</returns>
        /// <search>divide,separator,delimiter,cut,csv,comma,</search>
        public static string[] Split(string @string, params string[] separators)
        {
            separators = separators.Select(s => s == "\n" ? Environment.NewLine : s).ToArray(); // converts all \n in separater array to Environment Newline (i.e. \r\n)
            @string = Regex.Replace(@string, "(?<!\r)\n", Environment.NewLine); // converts all \n in String str to Environment.NewLine (i.e. '\r\n')

            return separators.Contains("")
                ? @string.ToCharArray().Select(char.ToString).ToArray()
                : @string.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        ///     Concatenates multiple strings into a single string, inserting the given
        ///     separator between each joined string.
        /// </summary>
        /// <param name="separator">String to be inserted between joined strings.</param>
        /// <param name="strings">Strings to be joined into a single string.</param>
        /// <returns name="string">
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
        /// <param name="string">String to be made uppercase</param>
        /// <returns name="string">Uppercase string</returns>
        /// <search>2uppercase,to uppercase,touppercase,uppercase</search>
        public static string ToUpper(string @string)
        {
            return @string.ToUpper();
        }

        /// <summary>
        ///     Converts the given string to all lowercase characters.
        /// </summary>
        /// <param name="string">String to be made lowercase</param>
        /// <returns name="string">Lowercase string</returns>
        /// <search>2lowercase,to lowercase,tolowercase,lowercase</search>
        public static string ToLower(string @string)
        {
            return @string.ToLower();
        }

        /// <summary>
        ///     Converts the given string to title case.
        /// </summary>
        /// <param name="str">String to be made title case</param>
        /// <returns name="str">Title case string</returns>
        /// <search>2titlecase,to titlecase,to title case,totitlecase,titlecase</search>
        public static string ToTitle(string str)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str);
        }

        /// <summary>
        ///     Converts the given string to all uppercase characters or all
        ///     lowercase characters based on a boolean parameter.
        /// </summary>
        /// <param name="string">String to be made uppercase or lowercase.</param>
        /// <param name="upper">
        ///     True to convert to uppercase, false to convert to lowercase.
        /// </param>
        /// <returns name="string">String with converted case.</returns>
        /// <search>
        ///     2lowercase,to lowercase,tolowercase,lowercase,
        ///     2uppercase,to uppercase,touppercase,uppercase
        /// </search>
        public static string ChangeCase(string @string, bool upper)
        {
            return upper ? @string.ToUpper() : @string.ToLower();
        }

        /// <summary>
        ///     Retrieves a substring from the given string. The substring starts at the given
        ///     character position and has the given length.
        /// </summary>
        /// <param name="string">String to take substring of</param>
        /// <param name="startIndex">
        ///     Starting character position of the substring in the original string
        /// </param>
        /// <param name="length">Number of characters in the substring</param>
        /// <returns name="string">Substring made from the original string</returns>
		///  <search>subset,get string,part,smaller string</search>
        public static string Substring(string @string, int startIndex, int length)
        {
            if (startIndex < 0)
            {
                startIndex += @string.Length;
            }
            bool reverse = false;
            if (length < 0)
            {
                reverse = true;
                startIndex += length;
                length *= -1;
            }
            var result = @string.Substring(startIndex, length);
            return reverse ? new string(result.Reverse().ToArray()) : result;
        }

        /// <summary>
        ///     Determines if the given string contains the given substring.
        /// </summary>
        /// <param name="string">String to search in</param>
        /// <param name="searchFor">Substring to search for</param>
        /// <param name="ignoreCase">Whether or not comparison takes case into account</param>
        /// <returns name="bool">Whether the string contains the substring</returns>
        /// <search>test,within,in,is in,part of</search>
        public static bool Contains(string @string, string searchFor, bool ignoreCase = false)
        {
            return !ignoreCase ? @string.Contains(searchFor) : @string.ToLowerInvariant().Contains(searchFor.ToLowerInvariant());
        }

        /// <summary>
        ///     Counts the number of non-overlapping occurrences of a substring inside a given string.
        /// </summary>
        /// <param name="string">String to search in</param>
        /// <param name="searchFor">Substring to search for</param>
        /// <param name="ignoreCase">Whether or not comparison takes case into account</param>
        /// <returns name="int">Number of non-overlapping occurrences of the substring in the string</returns>
        /// <search>count,substring,count occurrences,numberof,search,find,within</search>
        public static int CountOccurrences(string @string, string searchFor, bool ignoreCase = false)
        {
            if (searchFor == string.Empty)
                return @string.Length + 1;

            int count = 0, start = 0;
            while (true)
            {
                start = @string.IndexOf(
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
        /// <param name="string">String to replace substrings in.</param>
        /// <param name="searchFor">Text to be replaced.</param>
        /// <param name="replaceWith">Text to replace with.</param>
        /// <returns name="string">String with replacements made.</returns>
        /// <search>replace,overwrite,override,find and replace</search>
        public static string Replace(string @string, string searchFor, string replaceWith)
        {
            return @string.Replace(searchFor, replaceWith);
        }

        /// <summary>
        ///     Determines if the given string ends with the given substring.
        /// </summary>
        /// <param name="string">String to search the end of</param>
        /// <param name="searchFor">Substring to search the end for</param>
        /// <param name="ignoreCase">True to ignore case in comparison, false to take case into account</param>
        /// <returns name="bool">True if string starts with substring, false if it doesn’t</returns>
        /// <search>test,does end,last,str end,terminated</search>
        public static bool EndsWith(string @string, string searchFor, bool ignoreCase = false)
        {
            return @string.EndsWith(searchFor, ignoreCase, CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Determines if the given string starts with the given substring.
        /// </summary>
        /// <param name="string">String to search the start of</param>
        /// <param name="searchFor">Substring to search the start for.</param>
        /// <param name="ignoreCase">True to ignore case in comparison, false to take case into account</param>
        /// <returns name="bool">True if string starts with substring, false if it doesn’t</returns>
        /// <search>test,beginswith,start,string start,front</search>
        public static bool StartsWith(string @string, string searchFor, bool ignoreCase = false)
        {
            return @string.StartsWith(searchFor, ignoreCase, CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Removes all whitespace from the start and end of the given string.
        /// </summary>
        /// <param name="string">String to trim.</param>
        /// <returns name="string">String with beginning and ending whitespaces removed.</returns>
        /// <search>trimstring,cleanstring,whitespace,blanks,spaces,string trim</search>
        public static string TrimWhitespace(string @string)
        {
            return @string.Trim();
        }

        /// <summary>
        ///     Removes all whitespace from the start of the given string.
        /// </summary>
        /// <param name="string">String to trim.</param>
        /// <returns name="string">String with leading white spaces removed.</returns>
        /// <search>trim string,clean string,trim leading whitespaces,string trim</search>
        public static string TrimLeadingWhitespace(string @string)
        {
            return @string.TrimStart();
        }

        /// <summary>
        ///     Removes all whitespace from the end of the given string.
        /// </summary>
        /// <param name="string">String to trim.</param>
        /// <returns name="string">String with white spaces at end removed.</returns>
        /// <search>trim string,clean string,trim trailing whitespaces,string trim</search>
        public static string TrimTrailingWhitespace(string @string)
        {
            return @string.TrimEnd();
        }

        /// <summary>
        ///     Finds the zero-based index of the first occurrence of a sub-string inside a string.
        ///     Returns -1 if no index could be found.
        /// </summary>
        /// <param name="string">String to search in</param>
        /// <param name="searchFor">Substring to search for</param>
        /// <param name="ignoreCase">Whether or not comparison takes case into account</param>
        /// <returns name="int">
        ///     Index of the first occurrence of the substring or -1 if not found
        /// </returns>
        /// <search>index of,find substring,where,search</search>
        public static int IndexOf(string @string, string searchFor, bool ignoreCase = false)
        {
            return @string.IndexOf(
                searchFor,
                ignoreCase
                    ? StringComparison.InvariantCultureIgnoreCase
                    : StringComparison.InvariantCulture);
        }
        /// <summary>
        /// Finds list of indices where sub-string appears inside a string.
        /// </summary>
        /// <param name="string">A string to search in.</param>
        /// <param name="searchFor">Substring to search for.</param>
        /// <param name="ignoreCase">Whether or not comparison takes case into account.</param>
        /// <returns name="indexList">List of indices where substring is found (type: int[]) </returns>
        /// <search>all indices of,find substring,where,search</search>
        public static int[] AllIndicesOf(string @string, string searchFor, bool ignoreCase = false)
        {
            var indices = new List<int>();
            if (searchFor == System.String.Empty) return indices.ToArray();

            for (int index = 0; ; index += searchFor.Length)
            {
                index = @string.IndexOf(searchFor, index, ignoreCase
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
        /// <param name="string">String to search in.</param>
        /// <param name="searchFor">Substring to search for.</param>
        /// <param name="ignoreCase">Whether comparison takes case into account.</param>
        /// <returns name="int">
        ///     Index of the last occurrence of the substring or -1 if not found.
        /// </returns>
		/// <search>last index of,find substring,where,search</search>
        public static int LastIndexOf(string @string, string searchFor, bool ignoreCase = false)
        {
            return @string.LastIndexOf(
                searchFor,
                ignoreCase
                    ? StringComparison.InvariantCultureIgnoreCase
                    : StringComparison.InvariantCulture);
        }

        /// <summary>
        ///     Right-aligns the characters in the given string by padding them with spaces on the left,
        ///     for a specified total length.
        /// </summary>
        /// <param name="string">String to pad</param>
        /// <param name="newLength">Total length of the string after padding</param>
        /// <param name="padChars">Character to pad with, defaults to space</param>
        /// <returns name="string">
        ///     Strings right-aligned by padding with leading whitespaces for a specified total length.
        /// </returns>
        /// <search>pad left,right align,right-align,pad,string space,whitespace</search>
        public static string PadLeft(string @string, int newLength, string padChars = " ")
        {
            return new string(padChars.Cycle().Take(newLength - @string.Length).Concat(@string).ToArray());
        }

        /// <summary>
        ///     Left-aligns the characters in the given string by padding them with spaces on the right,
        ///     for a specified total length.
        /// </summary>
        /// <param name="string">String to pad</param>
        /// <param name="newLength">Total length of the string after padding</param>
        /// <param name="padChars">Character to pad with, defaults to space</param>
        /// <returns name="string">
        ///     Strings left-aligned by padding with trailing whitespaces for a specified total length
        /// </returns>
        /// <search>pad right,left align,left-align,pad string space,whitespace</search>
        public static string PadRight(string @string, int newLength, string padChars = " ")
        {
            return new string(
                Enumerable.Concat(@string, padChars.Cycle().Take(newLength - @string.Length)).ToArray());
        }

        /// <summary>
        ///     Increases the length of a string by encasing the original characters with spaces on either side.
        /// </summary>
        /// <param name="string">String to center</param>
        /// <param name="newLength">Total length of the string after centering</param>
        /// <param name="padChars">Character to center with, defaults to space</param>
        /// <returns name="string">
        ///     Strings center-aligned by padding them with leading and trailing
        ///     whitespaces for a specified total length.
        /// </returns>
        /// <search>center align,center-align,centered,whitespace,expand string,surround</search>
        public static string Center(string @string, int newLength, string padChars = " ")
        {
            var padHalf = (newLength - @string.Length) / 2;

            return
                new string(
                    padChars.Cycle()
                        .Take(padHalf)
                        .Concat(@string)
                        .Concat(padChars.Cycle().Take(newLength - @string.Length - padHalf))
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
        /// <param name="string">String to insert into.</param>
        /// <param name="index">Index to insert at.</param>
        /// <param name="toInsert">String to be inserted.</param>
        /// <returns name="string">String with inserted substring.</returns>
        /// <search>insertstring,insert string</search>
        public static string Insert(string @string, int index, string toInsert)
        {
            return @string.Insert(index, toInsert);
        }

        /// <summary>
        ///     Removes characters from a string.
        /// </summary>
        /// <param name="string">String to remove characters from.</param>
        /// <param name="startIndex">Index at which to start removal.</param>
        /// <param name="count">
        ///     Amount of characters to remove,
        ///     Note: if the Count is negative, the removal process goes from right to left.
        /// </param>
        /// <returns name="string">String with characters removed.</returns>
		/// <search>delete,rem,shorten</search>
        public static string Remove(string @string, int startIndex, int? count = null)
        {
            if (startIndex < 0)
            {
                startIndex += @string.Length;
            }

            var _count = count ?? @string.Length - startIndex;

            if (_count < 0)
            {
                // The input count is negative means the removal process is required operate from right to left.
                // However, the removal process in this function always operates rightwards (left to right).
                // Therefore, a conversion for start index needs to be done in order to change
                // from leftwards removal (right to left) to rightwards removal (left to right).
                startIndex = startIndex + _count + 1;
                _count *= -1;
            }

            if (_count > @string.Length)
            {
                throw new ArgumentOutOfRangeException("count", Properties.Resources.StringRemoveCountOutOfRangeMessage);
            }

            if (startIndex == 0 && @string.Length == 0)
            {
                return string.Empty;
            }

            if (startIndex >= @string.Length || startIndex < 0)
            {
                // startIndex of an array must be within the string length. 
                // If after the conversion of negative startIndex, startIndex is still
                // less than zero, ArgumentOutOfRangeException is occurred.

                throw new ArgumentOutOfRangeException("startIndex", Properties.Resources.StringRemoveStartIndexOutOfRangeMessage);

            }

            return @string.Remove(startIndex, _count);
        }
    }
}
