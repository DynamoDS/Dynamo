using System;
using System.IO;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using ProtoCore.Exceptions;
using ProtoCore.Utils;

namespace ProtoTest.UtilsTests
{
    /// <summary>
    /// Covers the diagnosability contract of <see cref="Validity"/>.
    ///
    /// DYN-10740 surfaced as the "Unhandled exception in Dynamo engine" dialog with a completely
    /// blank message. The dialog renders Exception.Message followed by Exception.StackTrace, and the
    /// only exception ProtoCore constructed with an empty message was the one thrown by the
    /// single-argument Validity.Assert(bool) overload, so nothing at all identified the broken
    /// invariant. These tests pin the assert helper to producing a message that names the failing
    /// condition and its source location.
    /// </summary>
    [TestFixture]
    public class ValidityTests
    {
        /// <summary>
        /// Returns the line number of its own call site. Used so the expected line number is never
        /// hard coded and stays correct when this file is edited.
        /// </summary>
        private static int CurrentLine([CallerLineNumber] int lineNumber = 0)
        {
            return lineNumber;
        }

        [Test]
        [Category("UnitTests")]
        public void WhenBareAssertFailsThenExceptionMessageIsNotEmpty()
        {
            var exception = Assert.Throws<CompilerInternalException>(() => Validity.Assert(false));

            Assert.IsNotNull(exception.Message);
            Assert.IsNotEmpty(exception.Message, "A blank message is the DYN-10740 defect.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(exception.Message));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenBareAssertFailsThenMessageNamesConditionMemberAndLine()
        {
            var conditionThatDoesNotHold = false;

            var expectedLine = CurrentLine() + 1;
            var exception = Assert.Throws<CompilerInternalException>(() => Validity.Assert(conditionThatDoesNotHold));

            var message = exception.Message;

            // The literal source text of the condition, captured by [CallerArgumentExpression].
            StringAssert.Contains(nameof(conditionThatDoesNotHold), message);

            // The calling member, captured by [CallerMemberName]. Inside a lambda this resolves to
            // the enclosing member, which is this test method.
            StringAssert.Contains(nameof(WhenBareAssertFailsThenMessageNamesConditionMemberAndLine), message);

            // The call site line number, captured by [CallerLineNumber].
            StringAssert.Contains(expectedLine.ToString(), message);

            // The source file, captured by [CallerFilePath] and reduced to its file name.
            StringAssert.Contains("ValidityTests.cs", message);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenBareAssertFailsThenMessageCarriesNoAbsoluteSourcePath()
        {
            var exception = Assert.Throws<CompilerInternalException>(() => Validity.Assert(false));

            // [CallerFilePath] is the absolute path on the machine that compiled the caller. That
            // path must not reach a user-facing crash dialog, so only the file name is reported.
            var thisFilePath = CurrentFilePath();
            var directory = Path.GetDirectoryName(thisFilePath);

            Assert.IsNotEmpty(directory);
            Assert.IsFalse(exception.Message.Contains(directory),
                "The assertion message must not leak the build machine's source directory.");
        }

        [Test]
        [Category("UnitTests")]
        public void WhenBareAssertHoldsThenNothingIsThrown()
        {
            Assert.DoesNotThrow(() => Validity.Assert(true));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenAssertWithMessageFailsThenThatMessageIsUsed()
        {
            const string message = "DYN-10740 explicit assertion message.";

            var exception = Assert.Throws<CompilerInternalException>(() => Validity.Assert(false, message));

            Assert.AreEqual(message, exception.Message);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenAssertWithEmptyMessageFailsThenExceptionMessageIsStillNotEmpty()
        {
            var exception = Assert.Throws<CompilerInternalException>(() => Validity.Assert(false, string.Empty));

            Assert.IsNotEmpty(exception.Message);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenAssertWithMessageHoldsThenNothingIsThrown()
        {
            Assert.DoesNotThrow(() => Validity.Assert(true, "unused"));
        }

        /// <summary>
        /// Returns the absolute path of this source file as the compiler saw it.
        /// </summary>
        private static string CurrentFilePath([CallerFilePath] string filePath = null)
        {
            return filePath;
        }
    }
}
