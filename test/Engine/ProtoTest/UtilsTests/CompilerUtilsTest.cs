using System;
using System.Text;
using NUnit.Framework;
using ProtoCore.Utils;

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
        public void ToLiteral_StringWithNewlines_PreservesNewlines()
        {
            // Arrange
            string input = "Line1\nLine2";

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual("Line1\nLine2", result);
        }

        [Test]
        public void ToLiteral_StringWithCarriageReturns_PreservesCarriageReturns()
        {
            // Arrange
            string input = "Line1\rLine2";

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual("Line1\rLine2", result);
        }

        [Test]
        public void ToLiteral_StringWithCRLF_PreservesCRLF()
        {
            // Arrange
            string input = "Line1\r\nLine2";

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual("Line1\r\nLine2", result);
        }

        [Test]
        public void ToLiteral_StringWithTabs_PreservesTabs()
        {
            // Arrange
            string input = "Column1\tColumn2";

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual("Column1\tColumn2", result);
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
        public void ToLiteral_StringWithControlCharacters_PreservesControlCharacters()
        {
            // Arrange
            string input = "Text" + (char)7 + (char)8 + (char)12 + (char)11 + (char)0 + "End";

            // Act
            string result = CompilerUtils.ToLiteral(input);

            // Assert
            Assert.AreEqual("Text" + (char)7 + (char)8 + (char)12 + (char)11 + (char)0 + "End", result);
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

        #region Round-Trip Intent Tests

        [Test]
        public void ToLiteral_AfterEscape_DesignScriptParserShouldSeeExactOriginalBytes()
        {
            // This test documents the round-trip intent
            // The escaped string should, when parsed by DesignScript, produce the exact original bytes
            
            // Arrange
            string[] testCases = {
                "",
                "abcDEF123",
                "He said \"hi\"",
                @"C:\bin\app",
                "line1\r\nline2",
                "line1\nline2",
                "Жълъд你好🙂",
                "test + () + concatenation"
            };

            foreach (string original in testCases)
            {
                // Act
                string escaped = CompilerUtils.ToLiteral(original);

                // Assert - The escaped string should be valid DesignScript
                // and when parsed should produce the original string
                Assert.IsNotNull(escaped, $"Escaped result should not be null for input: {original}");
                
                // Basic validation: escaped string should not contain unescaped quotes or backslashes
                // Check that quotes are properly escaped (should be \" not ")
                if (original.Contains("\""))
                {
                    Assert.IsTrue(escaped.Contains("\\\""), $"Escaped string should contain \\\" for quotes: {escaped}");
                    // Check that there are no unescaped quotes (quotes not preceded by odd number of backslashes)
                    for (int i = 0; i < escaped.Length; i++)
                    {
                        if (escaped[i] == '"')
                        {
                            // Count consecutive backslashes immediately before the quote
                            int backslashCount = 0;
                            int j = i - 1;
                            while (j >= 0 && escaped[j] == '\\')
                            {
                                backslashCount++;
                                j--;
                            }
                            // If the number of backslashes is even, the quote is unescaped
                            if (backslashCount % 2 == 0)
                            {
                                Assert.Fail($"Escaped string contains unescaped quote at position {i}: {escaped}");
                            }
                        }
                    }
                }
                
                // Check that backslashes are properly escaped (should be \\ not \)
                if (original.Contains("\\"))
                {
                    Assert.IsTrue(escaped.Contains("\\\\"), $"Escaped string should contain \\\\ for backslashes: {escaped}");
                    // Note: We can't easily check for unescaped backslashes since \\ is valid
                }
            }
        }

        #endregion
    }
}
