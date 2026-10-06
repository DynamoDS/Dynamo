using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;

namespace ProtoCore.Utils
{
    public class Validity
    {
        private const string UnspecifiedAssertionFailure =
            "Assertion failed (no diagnostic message was supplied).";

        /// <summary>
        /// Throws a compiler internal exception when <paramref name="cond"/> is false.
        /// </summary>
        /// <remarks>
        /// The optional parameters are filled in by the compiler at the call site and must not be
        /// passed explicitly. Because they are resolved at compile time they survive JIT inlining,
        /// so the resulting message still identifies the failing assertion even when the throwing
        /// frame is missing from the stack trace. Only the source file name is reported, never the
        /// full path from the build machine, because this message can reach a user-facing dialog.
        /// </remarks>
        /// <param name="cond">Condition that is expected to hold.</param>
        /// <param name="conditionExpression">
        /// Supplied by the compiler: the literal source text of <paramref name="cond"/>.
        /// </param>
        /// <param name="memberName">Supplied by the compiler: the name of the calling member.</param>
        /// <param name="filePath">Supplied by the compiler: the path of the calling source file.</param>
        /// <param name="lineNumber">Supplied by the compiler: the line number of the call site.</param>
        public static void Assert(bool cond,
            [CallerArgumentExpression(nameof(cond))] string conditionExpression = null,
            [CallerMemberName] string memberName = null,
            [CallerFilePath] string filePath = null,
            [CallerLineNumber] int lineNumber = 0)
        {
            if (!cond)
                throw new Exceptions.CompilerInternalException(
                    DescribeAssertionFailure(conditionExpression, memberName, filePath, lineNumber));
        }

        /// <summary>
        /// Throws a compiler internal exception carrying <paramref name="message"/> when
        /// <paramref name="cond"/> is false.
        /// </summary>
        /// <param name="cond">Condition that is expected to hold.</param>
        /// <param name="message">Message describing the invariant that was violated.</param>
        public static void Assert(bool cond, string message)
        {
            if (!cond)
            {
                throw new Exceptions.CompilerInternalException(EnsureDiagnosableMessage(message));
            }
        }

        // Will throw a compiler exception if the boolean "cond" is true
        // The exception will containt a formatted string (i.e string.Format(format, items))
        internal static void Assert(bool cond, string format, params object[] items)
        {
            if (!cond)
            {
                throw new Exceptions.CompilerInternalException(
                    EnsureDiagnosableMessage(string.Format(format, items)));
            }
        }

        /// <summary>
        /// Builds a self-describing assertion failure message from compiler-supplied caller info.
        /// </summary>
        private static string DescribeAssertionFailure(
            string conditionExpression, string memberName, string filePath, int lineNumber)
        {
            var condition = string.IsNullOrEmpty(conditionExpression)
                ? "<unknown condition>" : conditionExpression;
            var member = string.IsNullOrEmpty(memberName) ? "<unknown member>" : memberName;
            // Deliberately the file name only: filePath is an absolute path on the machine that
            // compiled ProtoCore and must not be shown to users.
            var fileName = string.IsNullOrEmpty(filePath) ? "<unknown file>" : Path.GetFileName(filePath);

            return string.Format(CultureInfo.InvariantCulture,
                "Assertion failed: '{0}' in {1} at {2}:{3}", condition, member, fileName, lineNumber);
        }

        /// <summary>
        /// Guarantees a non-empty exception message. An empty message renders as a blank crash
        /// dialog (DYN-10740), which makes the underlying failure impossible to diagnose.
        /// </summary>
        private static string EnsureDiagnosableMessage(string message)
        {
            return string.IsNullOrWhiteSpace(message) ? UnspecifiedAssertionFailure : message;
        }
    }
}
