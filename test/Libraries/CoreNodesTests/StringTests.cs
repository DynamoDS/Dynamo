//using System;
using DSCore;
using NUnit.Framework;

namespace DSCoreNodesTests
{
    [TestFixture]
    internal static class StringTests
    {
        [Test]
        [Category("UnitTests")]
        public static void ToNumber()
        {
            Assert.AreEqual(10, String.ToNumber("10"));
            Assert.AreEqual(10.0, String.ToNumber("10.0"));
        }

        [Test]
        [Category("UnitTests")]
        public static void Concat()
        {
            Assert.AreEqual("", String.Concat());
            Assert.AreEqual("abcdef", String.Concat("a", "b", "c", "d", "e", "f"));
        }
        
        [Test]
        [Category("UnitTests")]
        public static void Length()
        {
            Assert.AreEqual(0, String.Length(""));
            Assert.AreEqual(6, String.Length("abcdef"));
        }

        [Test]
        [Category("UnitTests")]
        public static void Split()
        {
            Assert.AreEqual(new[] { "a", "b", "c", "d", "e", "f" }, String.Split("abcdef", "", "a"));
            Assert.AreEqual(new[] { "ab", "d", "f"}, String.Split("abcdef", "c", "e"));
        }

        [Test]
        [Category("UnitTests")]
        public static void Join()
        {
            //Joining nothing yeilds nothing
            Assert.AreEqual("", String.Join("something"));

            //Joining one thing yeilds that same thing
            Assert.AreEqual("else", String.Join("something", "else"));

            Assert.AreEqual("abcdef", String.Join("", new[] { "a", "b", "c", "d", "e", "f" }));
            Assert.AreEqual("a b c d e f", String.Join(" ", new[] { "a", "b", "c", "d", "e", "f" }));
        }

        [Test]
        [Category("UnitTests")]
        public static void ToUpper()
        {
            Assert.AreEqual("", String.ToUpper(""));
            Assert.AreEqual("ABCDEF", String.ToUpper("abcdef"));
            Assert.AreEqual("ABCDEF", String.ToUpper("ABCDEF"));
            Assert.AreEqual("ABCDEF", String.ToUpper("aBCdEf"));
            Assert.AreEqual("1A2B3C4D5E6F!", String.ToUpper("1a2B3C4d5E6f!"));
        }

        [Test]
        [Category("UnitTests")]
        public static void ToLower()
        {
            Assert.AreEqual("", String.ToLower(""));
            Assert.AreEqual("abcdef", String.ToLower("abcdef"));
            Assert.AreEqual("abcdef", String.ToLower("ABCDEF"));
            Assert.AreEqual("abcdef", String.ToLower("aBCdEf"));
            Assert.AreEqual("1a2b3c4d5e6f!", String.ToLower("1a2B3C4d5E6f!"));
        }

        [Test]
        [Category("UnitTests")]
        public static void ChangeCase()
        {
            Assert.AreEqual("", String.ChangeCase("", false));
            Assert.AreEqual("abcdef", String.ChangeCase("abcdef", false));
            Assert.AreEqual("abcdef", String.ChangeCase("ABCDEF", false));
            Assert.AreEqual("abcdef", String.ChangeCase("aBCdEf", false));
            Assert.AreEqual("1a2b3c4d5e6f!", String.ChangeCase("1a2B3C4d5E6f!", false));

            Assert.AreEqual("", String.ChangeCase("", true));
            Assert.AreEqual("ABCDEF", String.ChangeCase("abcdef", true));
            Assert.AreEqual("ABCDEF", String.ChangeCase("ABCDEF", true));
            Assert.AreEqual("ABCDEF", String.ChangeCase("aBCdEf", true));
            Assert.AreEqual("1A2B3C4D5E6F!", String.ChangeCase("1a2B3C4d5E6f!", true));
        }

        [Test]
        [Category("UnitTests")]
        public static void Substring()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => String.Substring("", 1, 0));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => String.Substring("", 0, 1));

            Assert.AreEqual("", String.Substring("abcdef", 2, 0));
            Assert.AreEqual("bcd", String.Substring("abcdef", 1, 3));
            Assert.AreEqual("dcb", String.Substring("abcdef", 4, -3));
            Assert.AreEqual("ef", String.Substring("abcdef", -2, 2));
            Assert.AreEqual("dc", String.Substring("abcdef", -2, -2));
        }

        [Test]
        [Category("UnitTests")]
        public static void Contains()
        {
            Assert.IsTrue(String.Contains("", ""));
            Assert.IsFalse(String.Contains("", "a"));
            Assert.IsTrue(String.Contains("abcdef", "a"));
            Assert.IsFalse(String.Contains("Abcdef", "a"));
            Assert.IsFalse(String.Contains("abcdef", "A"));
            Assert.IsTrue(String.Contains("abcdef", "A", true));
            Assert.IsTrue(String.Contains("abcdef", "abc"));
            Assert.IsFalse(String.Contains("Abcdef", "abc"));
            Assert.IsFalse(String.Contains("abcdef", "Abc"));
            Assert.IsTrue(String.Contains("abcdef", "AbC", true));
        }

        [Test]
        [Category("UnitTests")]
        public static void CountOccurences()
        {
            Assert.AreEqual(1, String.CountOccurrences("", ""));
            Assert.AreEqual(7, String.CountOccurrences("abcdef", ""));

            Assert.AreEqual(0, String.CountOccurrences("", "a"));
            Assert.AreEqual(4, String.CountOccurrences("alabama", "a"));
            Assert.AreEqual(2, String.CountOccurrences("mississippi", "iss"));

            Assert.AreEqual(0, String.CountOccurrences("abcdef", "A"));
            Assert.AreEqual(1, String.CountOccurrences("abcdef", "A", true));
            Assert.AreEqual(0, String.CountOccurrences("missisSippi", "Iss"));
            Assert.AreEqual(2, String.CountOccurrences("missisSippi", "Iss", true));
        }

        [Test]
        [Category("UnitTests")]
        public static void Replace()
        {
            Assert.AreEqual("", String.Replace("", "a", "b"));
            Assert.AreEqual("", String.Replace("aaaa", "a", ""));
            Assert.AreEqual("b", String.Replace("aabaa", "a", ""));
            Assert.AreEqual("fatman", String.Replace("strongman", "strong", "fat"));
            Assert.AreEqual("mithithippi", String.Replace("mississippi", "ss", "th"));
        }

        [Test]
        [Category("UnitTests")]
        public static void EndsWith()
        {
            Assert.IsTrue(String.EndsWith("", ""));
            Assert.IsTrue(String.EndsWith("a", ""));

            Assert.IsTrue(String.EndsWith("abcdef", "f"));
            Assert.IsTrue(String.EndsWith("abcdef", "ef"));
            Assert.IsTrue(String.EndsWith("abcdef", "def"));
            Assert.IsTrue(String.EndsWith("abcdef", "cdef"));
            Assert.IsTrue(String.EndsWith("abcdef", "bcdef"));
            Assert.IsTrue(String.EndsWith("abcdef", "abcdef"));

            Assert.IsFalse(String.EndsWith("abcdeF", "dEf"));
            Assert.IsTrue(String.EndsWith("abcdeF", "dEf", true));
        }

        [Test]
        [Category("UnitTests")]
        public static void StartsWith()
        {
            Assert.IsTrue(String.StartsWith("", ""));
            Assert.IsTrue(String.StartsWith("a", ""));

            Assert.IsTrue(String.StartsWith("abcdef", "a"));
            Assert.IsTrue(String.StartsWith("abcdef", "ab"));
            Assert.IsTrue(String.StartsWith("abcdef", "abc"));
            Assert.IsTrue(String.StartsWith("abcdef", "abcd"));
            Assert.IsTrue(String.StartsWith("abcdef", "abcde"));
            Assert.IsTrue(String.StartsWith("abcdef", "abcdef"));

            Assert.IsFalse(String.StartsWith("abCdef", "aBc"));
            Assert.IsTrue(String.StartsWith("abCdef", "aBc", true));
        }

        [Test]
        [Category("UnitTests")]
        public static void TrimWhitespace()
        {
            Assert.AreEqual("", String.TrimWhitespace(""));
            Assert.AreEqual("", String.TrimWhitespace(" "));
            Assert.AreEqual("", String.TrimWhitespace("\t"));
            Assert.AreEqual("", String.TrimWhitespace("\n"));
            Assert.AreEqual("", String.TrimWhitespace("\r"));
            Assert.AreEqual("some words", String.TrimWhitespace(" some words "));
            Assert.AreEqual("some words", String.TrimWhitespace("\tsome words\t"));
            Assert.AreEqual("some words", String.TrimWhitespace("\nsome words\n"));
            Assert.AreEqual("some words", String.TrimWhitespace("\rsome words\r"));
            Assert.AreEqual("some words", String.TrimWhitespace("\r \n\tsome words\r\n \t "));
        }

        [Test]
        [Category("UnitTests")]
        public static void TrimLeadingWhitespace()
        {
            Assert.AreEqual("", String.TrimLeadingWhitespace(""));
            Assert.AreEqual("", String.TrimLeadingWhitespace(" "));
            Assert.AreEqual("", String.TrimLeadingWhitespace("\t"));
            Assert.AreEqual("", String.TrimLeadingWhitespace("\n"));
            Assert.AreEqual("", String.TrimLeadingWhitespace("\r"));
            Assert.AreEqual("some words ", String.TrimLeadingWhitespace(" some words "));
            Assert.AreEqual("some words\t", String.TrimLeadingWhitespace("\tsome words\t"));
            Assert.AreEqual("some words\n", String.TrimLeadingWhitespace("\nsome words\n"));
            Assert.AreEqual("some words\r", String.TrimLeadingWhitespace("\rsome words\r"));
            Assert.AreEqual("some words\r\n \t ", String.TrimLeadingWhitespace("\r \n\tsome words\r\n \t "));
        }

        [Test]
        [Category("UnitTests")]
        public static void TrimTrailingWhitespace()
        {
            Assert.AreEqual("", String.TrimTrailingWhitespace(""));
            Assert.AreEqual("", String.TrimTrailingWhitespace(" "));
            Assert.AreEqual("", String.TrimTrailingWhitespace("\t"));
            Assert.AreEqual("", String.TrimTrailingWhitespace("\n"));
            Assert.AreEqual("", String.TrimTrailingWhitespace("\r"));
            Assert.AreEqual(" some words", String.TrimTrailingWhitespace(" some words "));
            Assert.AreEqual("\tsome words", String.TrimTrailingWhitespace("\tsome words\t"));
            Assert.AreEqual("\nsome words", String.TrimTrailingWhitespace("\nsome words\n"));
            Assert.AreEqual("\rsome words", String.TrimTrailingWhitespace("\rsome words\r"));
            Assert.AreEqual("\r \n\tsome words", String.TrimTrailingWhitespace("\r \n\tsome words\r\n \t "));
        }

        [Test]
        [Category("UnitTests")]
        public static void IndexOf()
        {
            Assert.AreEqual(0, String.IndexOf("", ""));
            Assert.AreEqual(0, String.IndexOf("a", ""));
            Assert.AreEqual(0, String.IndexOf("abcdef", "a"));
            Assert.AreEqual(0, String.IndexOf("abcdef", "ab"));
            Assert.AreEqual(0, String.IndexOf("abcdef", "abc"));
            Assert.AreEqual(0, String.IndexOf("abcdef", "abcd"));
            Assert.AreEqual(0, String.IndexOf("abcdef", "abcde"));
            Assert.AreEqual(0, String.IndexOf("abcdef", "abcdef"));
            Assert.AreEqual(5, String.IndexOf("abcdef", "f"));
            Assert.AreEqual(4, String.IndexOf("abcdef", "ef"));
            Assert.AreEqual(3, String.IndexOf("abcdef", "def"));
            Assert.AreEqual(2, String.IndexOf("abcdef", "cdef"));
            Assert.AreEqual(1, String.IndexOf("abcdef", "bcdef"));
            Assert.AreEqual(-1, String.IndexOf("abcdef", "g"));
            Assert.AreEqual(-1, String.IndexOf("abcdef", "F"));
            Assert.AreEqual(5, String.IndexOf("abcdef", "F", true));
            Assert.AreEqual(1, String.IndexOf("mississippi", "i"));
            Assert.AreEqual(1, String.IndexOf("mississippi", "is"));
        }

        [Test]
        [Category("UnitTests")]
        public static void AllIndicesOf()
        {
            Assert.AreEqual(new int[] {}, String.AllIndicesOf("", ""));
            Assert.AreEqual(new int[] { }, String.AllIndicesOf("a", ""));
            Assert.AreEqual(new int[] { }, String.AllIndicesOf("abcdef", "g"));
            Assert.AreEqual(new int[] { }, String.AllIndicesOf("abcdef", "F"));
            Assert.AreEqual(new[] {5}, String.AllIndicesOf("abcdef", "F", true));
            Assert.AreEqual(new[] {1, 4, 7, 10}, String.AllIndicesOf("mississippi", "i"));
            Assert.AreEqual(new[] {1, 4}, String.AllIndicesOf("mississippi", "is"));
        }

        [Test]
        [Category("UnitTests")]
        public static void LastIndexOf()
        {
            Assert.AreEqual(0, String.LastIndexOf("", ""));
            Assert.AreEqual(0, String.LastIndexOf("a", ""));
            Assert.AreEqual(0, String.LastIndexOf("abcdef", "a"));
            Assert.AreEqual(0, String.LastIndexOf("abcdef", "ab"));
            Assert.AreEqual(0, String.LastIndexOf("abcdef", "abc"));
            Assert.AreEqual(0, String.LastIndexOf("abcdef", "abcd"));
            Assert.AreEqual(0, String.LastIndexOf("abcdef", "abcde"));
            Assert.AreEqual(0, String.LastIndexOf("abcdef", "abcdef"));
            Assert.AreEqual(5, String.LastIndexOf("abcdef", "f"));
            Assert.AreEqual(4, String.LastIndexOf("abcdef", "ef"));
            Assert.AreEqual(3, String.LastIndexOf("abcdef", "def"));
            Assert.AreEqual(2, String.LastIndexOf("abcdef", "cdef"));
            Assert.AreEqual(1, String.LastIndexOf("abcdef", "bcdef"));
            Assert.AreEqual(-1, String.LastIndexOf("abcdef", "g"));
            Assert.AreEqual(-1, String.LastIndexOf("abcdef", "F"));
            Assert.AreEqual(5, String.LastIndexOf("abcdef", "F", true));
            Assert.AreEqual(10, String.LastIndexOf("mississippi", "i"));
            Assert.AreEqual(4, String.LastIndexOf("mississippi", "is"));
        }

        [Test]
        [Category("UnitTests")]
        public static void PadLeft()
        {
            Assert.AreEqual("", String.PadLeft("", 0));
            Assert.AreEqual("     ", String.PadLeft("", 5));
            Assert.AreEqual("a", String.PadLeft("a", 0));
            Assert.AreEqual("a", String.PadLeft("a", -1));
            Assert.AreEqual("    a", String.PadLeft("a", 5));
            Assert.AreEqual("----a", String.PadLeft("a", 5, "-"));
            Assert.AreEqual("-=-=-a", String.PadLeft("a", 6, "-="));
        }

        [Test]
        [Category("UnitTests")]
        public static void PadRight()
        {
            Assert.AreEqual("", String.PadRight("", 0));
            Assert.AreEqual("     ", String.PadRight("", 5));
            Assert.AreEqual("a", String.PadRight("a", 0));
            Assert.AreEqual("a", String.PadRight("a", -1));
            Assert.AreEqual("a    ", String.PadRight("a", 5));
            Assert.AreEqual("a----", String.PadRight("a", 5, "-"));
            Assert.AreEqual("a-=-=-", String.PadRight("a", 6, "-="));
        }

        [Test]
        [Category("UnitTests")]
        public static void Center()
        {
            Assert.AreEqual("", String.Center("", 0));
            Assert.AreEqual("     ", String.Center("", 5));
            Assert.AreEqual("a", String.Center("a", 0));
            Assert.AreEqual("a", String.Center("a", -1));
            Assert.AreEqual("  a  ", String.Center("a", 5));
            Assert.AreEqual("--a--", String.Center("a", 5, "-"));
            Assert.AreEqual("-=a-=-", String.Center("a", 6, "-="));
        }

        [Test]
        [Category("UnitTests")]
        public static void Insert()
        {
            Assert.AreEqual("", String.Insert("", 0, ""));
            Assert.AreEqual("abc", String.Insert("abc", 0, ""));
            Assert.AreEqual("abc", String.Insert("abc", 1, ""));
            Assert.AreEqual("abc", String.Insert("abc", 2, ""));
            Assert.AreEqual("abc", String.Insert("abc", 3, ""));
            Assert.AreEqual("1", String.Insert("", 0, "1"));
            Assert.AreEqual("abc1def", String.Insert("abcdef", 3, "1"));
            Assert.AreEqual("abcXYZdef", String.Insert("abcdef", 3, "XYZ"));
        }

        [Test]
        [Category("UnitTests")]
        public static void Remove()
        {
            Assert.AreEqual("", String.Remove("", 0));
            Assert.AreEqual("", String.Remove("abcdef", 0));
            Assert.AreEqual("a", String.Remove("abcdef", 1));
            Assert.AreEqual("a", String.Remove("abcdef", 1));
            Assert.AreEqual("aef", String.Remove("abcdef", 1, 3));
            Assert.AreEqual("aef", String.Remove("abcdef", -5, 3));
            Assert.AreEqual("abf", String.Remove("abcdef", -2, -3));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => String.Remove("abcdef", 0, 7)); // count out of range of available character in string
            Assert.Throws<System.ArgumentOutOfRangeException>(() => String.Remove("abcdef", 8, 1)); // startIndex out of range of available character in string
            // Case of startIndex magnitude is out of range and the computation of startIndex + string.Length in the String.Remove function is negative.
            Assert.Throws<System.ArgumentOutOfRangeException>(() => String.Remove("abcdef", -7, 1)); 
        }
    }
}
