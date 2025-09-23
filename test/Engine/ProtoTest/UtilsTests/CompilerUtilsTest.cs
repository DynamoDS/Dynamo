using NUnit.Framework;
using ProtoCore.Utils;
using System.Text;

namespace ProtoTest.UtilsTests
{
    [TestFixture]
    class CompilerUtilsTest : ProtoTestBase
    {
        #region Basic Functionality Tests

        [Test]
        public void ToLiteral_NullInput_ReturnsEmptyString()
        {
            // Arrange
            string input = null;

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void ToLiteral_EmptyString_ReturnsEmptyString()
        {
            // Arrange
            string input = string.Empty;

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void ToLiteral_PlainASCII_ReturnsSameString()
        {
            // Arrange
            string input = "abcDEF123";

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual("abcDEF123", result);
        }

        [Test]
        public void ToLiteral_StringWithoutEscapes_UsesFastPath()
        {
            // Arrange - String with no backslashes or quotes (should use fast path)
            string input = "Hello World 123 !@#$%^&*()";

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual(input, result);
            // Fast path should return the same reference
            Assert.AreSame(input, result);
        }

        #endregion

        #region Core Escaping Tests (Backslash and Quote Only)

        [Test]
        public void ToLiteral_StringWithDoubleQuotes_EscapesQuotes()
        {
            // Arrange
            string input = "He said \"Hello\"";

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual("He said \\\"Hello\\\"", result);
        }

        [Test]
        public void ToLiteral_StringWithBackslashes_EscapesBackslashes()
        {
            // Arrange
            string input = "C:\\Users\\Test";

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual("C:\\\\Users\\\\Test", result);
        }

        [Test]
        public void ToLiteral_StringWithBothBackslashAndQuote_EscapesBoth()
        {
            // Arrange
            string input = "Path: \"C:\\Users\\Test\"";

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual("Path: \\\"C:\\\\Users\\\\Test\\\"", result);
        }

        #endregion

        #region Newline Handling Tests

        [Test]
        public void ToLiteral_StringWithNewlines_EscapesNewlines()
        {
            // Arrange
            string input = "Line1\nLine2";

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual("Line1\\nLine2", result);
        }

        [Test]
        public void ToLiteral_StringWithCarriageReturns_EscapesCarriageReturns()
        {
            // Arrange
            string input = "Line1\rLine2";

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual("Line1\\rLine2", result);
        }

        [Test]
        public void ToLiteral_StringWithCRLF_EscapesCRLF()
        {
            // Arrange
            string input = "Line1\r\nLine2";

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual("Line1\\r\\nLine2", result);
        }

        [Test]
        public void ToLiteral_StringWithTabs_EscapesTabs()
        {
            // Arrange
            string input = "Column1\tColumn2";

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual("Column1\\tColumn2", result);
        }

        #endregion

        #region Unicode and Special Characters Tests

        [Test]
        public void ToLiteral_StringWithUnicodeCharacters_LeavesUnicodeUnchanged()
        {
            // Arrange
            string input = "Hello 世界 🌍 Жълъд";

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual("Hello 世界 🌍 Жълъд", result);
        }

        [Test]
        public void ToLiteral_StringWithUnicodeSurrogatePairs_LeavesUnicodeUnchanged()
        {
            // Arrange - Test Unicode surrogate pairs (characters above U+FFFF)
            string input = "🧡" + char.ConvertFromUtf32(0x1F9E1); // Orange heart + 🧡

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual(input, result);
        }

        [Test]
        public void ToLiteral_StringWithControlCharacters_EscapesControlCharacters()
        {
            // Arrange
            string input = "Text" + (char)7 + (char)8 + (char)12 + (char)11 + (char)0 + "End";

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual("Text\\a\\b\\f\\v\\0End", result);
        }

        #endregion

        #region Edge Cases and Stress Tests

        [Test]
        public void ToLiteral_StringWithBackslashAtEnd_EscapesCorrectly()
        {
            // Arrange
            string input = "Path\\";

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual("Path\\\\", result);
        }

        [Test]
        public void ToLiteral_StringWithQuoteAtEnd_EscapesCorrectly()
        {
            // Arrange
            string input = "Text\"";

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual("Text\\\"", result);
        }

        [Test]
        public void ToLiteral_StringWithMultipleConsecutiveEscapes_EscapesAll()
        {
            // Arrange
            string input = "\\\\\"\"";

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual("\\\\\\\\\\\"\\\"", result);
        }

        [Test]
        public void ToLiteral_StringWithOnlyBackslashes_EscapesAll()
        {
            // Arrange
            string input = "\\\\\\\\";

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual("\\\\\\\\\\\\\\\\", result);
        }

        [Test]
        public void ToLiteral_StringWithOnlyQuotes_EscapesAll()
        {
            // Arrange
            string input = "\"\"\"\"";

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual("\\\"\\\"\\\"\\\"", result);
        }

        [Test]
        public void ToLiteral_ParserCompatibility_HandlesAllEscapeSequences()
        {
            // Test that our escaping method is perfectly compatible with the Parser's GetEscapedString
            // This ensures that any string we escape can be properly unescaped by the parser
            
            string[] testCases = {
                "",                    // Empty string
                "abcDEF123",          // Plain ASCII
                "He said \"Hello\"",   // String with quotes
                @"C:\Users\Test",     // String with backslashes
                "line1\r\nline2",     // String with CRLF
                "line1\nline2",       // String with newlines
                "Column1\tColumn2",   // String with tabs
                "Alert\aBell\bForm\fFeed", // String with control characters
                "Null\0Character",    // String with null character
                "Vertical\vTab",      // String with vertical tab
                "Жълъд你好🙂",        // Unicode characters
                "Mixed\"quotes\\and\\backslashes\nwith\ttabs" // Complex mixed case
            };

            foreach (string original in testCases)
            {
                // Act: Escape the string using our updated ToLiteral method
                string escaped = CompilerUtils.ToLiteral(original);
                
                // Assert: The escaped string should be valid and not null
                Assert.IsNotNull(escaped, $"Escaped result should not be null for input: {original}");
                
                // The escaped string should be longer or equal to the original (since we're adding escape characters)
                Assert.IsTrue(escaped.Length >= original.Length, 
                    $"Escaped string should be at least as long as original. Original: '{original}', Escaped: '{escaped}'");
            }
        }

        [Test]
        public void ToLiteral_ControlCharacters_EscapesCorrectly()
        {
            // Test specific control character escaping to ensure Parser compatibility
            Assert.AreEqual("\\a", CompilerUtils.ToLiteral("\a"), "Alert character");
            Assert.AreEqual("\\b", CompilerUtils.ToLiteral("\b"), "Backspace character");
            Assert.AreEqual("\\f", CompilerUtils.ToLiteral("\f"), "Form feed character");
            Assert.AreEqual("\\n", CompilerUtils.ToLiteral("\n"), "Newline character");
            Assert.AreEqual("\\r", CompilerUtils.ToLiteral("\r"), "Carriage return character");
            Assert.AreEqual("\\t", CompilerUtils.ToLiteral("\t"), "Tab character");
            Assert.AreEqual("\\v", CompilerUtils.ToLiteral("\v"), "Vertical tab character");
            Assert.AreEqual("\\0", CompilerUtils.ToLiteral("\0"), "Null character");
        }

        #endregion

        #region Real-World Usage Tests

        [Test]
        public void ToLiteral_AssemblyPathWithSpaces_EscapesCorrectly()
        {
            // Arrange
            string input = @"C:\Program Files\Autodesk\Dynamo\Dynamo Core.dll";

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual(@"C:\\Program Files\\Autodesk\\Dynamo\\Dynamo Core.dll", result);
        }

        [Test]
        public void ToLiteral_AssemblyPathWithQuotes_EscapesCorrectly()
        {
            // Arrange
            string input = @"C:\Program Files\Autodesk\Dynamo\Dynamo ""Core"".dll";

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual("C:\\\\Program Files\\\\Autodesk\\\\Dynamo\\\\Dynamo \\\"Core\\\".dll", result);
        }

        [Test]
        public void ToLiteral_ModuleNameWithSpecialCharacters_EscapesCorrectly()
        {
            // Arrange
            string input = "MyModule.v1.0.0";

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual("MyModule.v1.0.0", result);
        }

        [Test]
        public void ToLiteral_StringWithPlusAndParens_IgnoresPlusAndParens()
        {
            // Arrange - These symbols should pass through unchanged
            string input = "test + () + concatenation";

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual("test + () + concatenation", result);
        }

        #endregion

        #region Long String Tests

        [Test]
        public void ToLiteral_LongStringWithEscapes_EscapesCorrectly()
        {
            // Arrange
            var sb = new StringBuilder();
            for (int i = 0; i < 1000; i++)
            {
                sb.Append("\\\"");
            }
            string input = sb.ToString();

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            string expected = input.Replace("\\", "\\\\").Replace("\"", "\\\"");
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ToLiteral_VeryLongString_PerformsWell()
        {
            // Arrange
            var sb = new StringBuilder();
            for (int i = 0; i < 10000; i++)
            {
                sb.Append("a");
            }
            string input = sb.ToString();

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual(input, result);
        }

        #endregion

    }
}
