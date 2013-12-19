using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Microsoft.Win32;

namespace DSCoreNodes
{
    /// <summary>
    /// Methods for managing strings.
    /// </summary>
    public static class String
    {
        /// <summary>
        /// Concatenates multiple strings into a single string.
        /// </summary>
        public static string Concat(params string[] strings)
        {
            return string.Concat(strings);
        }

        /// <summary>
        /// Returns the number of characters contained in the given string.
        /// </summary>
        /// <param name="s">String to take the length of.</param>
        public static int Length(string s)
        {
            return s.Length;
        }

        /// <summary>
        /// Divides a single string into a list of strings, determined by
        /// the given separater strings.
        /// </summary>
        /// <param name="s">String to split up.</param>
        /// <param name="separaters">
        /// Strings that, if present, determine the end and start of a split.
        /// </param>
        public static IList Split(string s, params string[] separaters)
        {
            return s.Split(separaters, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Concatenates multiple strings into a single string, inserting the given
        /// separator between each joined string.
        /// </summary>
        /// <param name="separator">String to be inserted between joined strings.</param>
        /// <param name="strings">Strings to be joined into a single string.</param>
        public static string Join(string separator, params string[] strings)
        {
            return string.Join(separator, strings);
        }

        /// <summary>
        /// Converts the given string to all uppercase characters.
        /// </summary>
        /// <param name="s">String to be made uppercase.</param>
        public static string ToUpper(string s)
        {
            return s.ToUpper();
        }

        /// <summary>
        /// Converts the given string to all lowercase characters.
        /// </summary>
        /// <param name="s">String to be made lowercase.</param>
        public static string ToLower(string s)
        {
            return s.ToLower();
        }

        /// <summary>
        /// Retrieves a substring from the given string. The substring starts at the given
        /// character position and has the given length.
        /// </summary>
        /// <param name="s">String to take substring of.</param>
        /// <param name="start">
        /// Starting character position of the substring in the original string.
        /// </param>
        /// <param name="length">Number of characters in the substring.</param>
        public static string Substring(string s, int start, int length)
        {
            return s.Substring(start, length);
        }

        /// <summary>
        /// Determines if the given string contains the given substring.
        /// </summary>
        /// <param name="s">String to search in.</param>
        /// <param name="search">Substring to search for.</param>
        public static bool Contains(string s, string search)
        {
            return s.Contains(search);
        }
    }
}
