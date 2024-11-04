using ProtoCore.DSASM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProtoCore.Utils
{
    public static class StringUtils
    {
        //TODO consider removing this option. It makes sharing harder.
        internal const string DynamoPreferencesNumberFormat = nameof(DynamoPreferencesNumberFormat);
        internal const string LEGACYFORMATTING = nameof(LEGACYFORMATTING);

        public static int CompareString(StackValue s1, StackValue s2, RuntimeCore runtimeCore)
        {
            if (!s1.IsString || !s2.IsString)
                return Constants.kInvalidIndex;

            if (s1.Equals(s2))
                return 0;

            string str1 = runtimeCore.RuntimeMemory.Heap.ToHeapObject<DSString>(s1).Value;
            string str2 = runtimeCore.RuntimeMemory.Heap.ToHeapObject<DSString>(s2).Value;
            return string.Compare(str1, str2);
        }

        /// <summary>
        /// Wraps ToString(format) in a try catch. Will return empty string if format is not valid for the target type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="Format"></param>
        /// <returns>Will return empty string if format is not valid for the target type.</returns>
        internal static string SafeToStringWithFormat<T>(this T target, string Format)
            where T : IFormattable
        {
            try
            {
                return target.ToString(Format, null);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string GetStringValue(StackValue sv, RuntimeCore runtimeCore)
        {
            ProtoCore.DSASM.Mirror.ExecutionMirror mirror = new DSASM.Mirror.ExecutionMirror(new ProtoCore.DSASM.Executive(runtimeCore), runtimeCore);
            return mirror.GetStringValue(sv, runtimeCore.RuntimeMemory.Heap, 0, true);
        }

        //used by legacy ToString methods without format specifier.
        public static StackValue ConvertToString(StackValue sv, RuntimeCore runtimeCore, ProtoCore.Runtime.RuntimeMemory rmem)
        {
            //maintain old behavior of existing string conversion nodes by passing null for formatSpecifier.
            return ConvertToStringInternal(sv, runtimeCore, null);
        }
        //used by new ToString methods with format specifier.
        internal static StackValue ConvertToString(IEnumerable<StackValue> args, RuntimeCore runtimeCore, ProtoCore.Runtime.RuntimeMemory rmem)
        {
            //TODO dislike this as a default...
            var formatSpecifier = DynamoPreferencesNumberFormat;
            var sv = args.ElementAt(0);
            if (args.Count() > 1)
            {   //TODO performance concern?
                formatSpecifier = GetStringValue(args.ElementAt(1),runtimeCore);
            }
            return ConvertToStringInternal(sv, runtimeCore, formatSpecifier);
        }

        private static StackValue ConvertToStringInternal(StackValue sv, RuntimeCore runtimeCore, string formatSpecifier)
        {
            StackValue returnSV;
            //TODO: Change Execution mirror class to have static methods, so that an instance does not have to be created
            ProtoCore.DSASM.Mirror.ExecutionMirror mirror =
                new DSASM.Mirror.ExecutionMirror(new ProtoCore.DSASM.Executive(runtimeCore), runtimeCore);
            if (formatSpecifier == null)
            {
               returnSV = ProtoCore.DSASM.StackValue.BuildString(
               mirror.GetStringValue(sv, runtimeCore.RuntimeMemory.Heap, 0, true), runtimeCore.RuntimeMemory.Heap);
            }
            else
            {
                returnSV = ProtoCore.DSASM.StackValue.BuildString(
                    mirror.GetStringValueUsingFormat(sv, formatSpecifier, runtimeCore.RuntimeMemory.Heap, 0, true), runtimeCore.RuntimeMemory.Heap);
            }
            return returnSV;
        }

      

        public static StackValue ConcatString(StackValue op1, StackValue op2, RuntimeCore runtimeCore)
        {
            var v1 = runtimeCore.RuntimeMemory.Heap.ToHeapObject<DSString>(op1).Value;
            var v2 = runtimeCore.RuntimeMemory.Heap.ToHeapObject<DSString>(op2).Value;
            return StackValue.BuildString(v1 + v2, runtimeCore.RuntimeMemory.Heap);
        }
        public static string ReplaceLineOfText(string text, int lineNumber, string newLine)
        {
            var textInLines = BreakTextIntoLines(text);
            textInLines[lineNumber] = newLine;
            return String.Join("\n", textInLines);
        }

        public static bool IsStringSpacesWithTabs(string text)
        {
            return String.IsNullOrWhiteSpace(text) || text.All(c => c == ' ' || c == '\t');
        }

        /// <summary>
        /// Following suggestions from stackoverflow,
        /// A reliable method for breaking text into lines
        /// using Regex is used.
        /// https://stackoverflow.com/questions/1508203/best-way-to-split-string-into-lines
        /// </summary>
        /// <param name="text"> text to break into lines</param>
        /// <returns> text lines </returns>
        public static string[] BreakTextIntoLines(string text)
        {
            return Regex.Split(text, "\r\n|\r|\n");
        }

        /// <summary>
        /// Replace all tabs with spaces given the text and tab spacing size
        /// </summary>
        /// <param name="text"></param>
        /// <param name="tabSpacingSize"></param>
        /// <returns></returns>
        public static string TabToSpaceConversion(string text, int tabSpacingSize)
        {
            try
            {
                return text.Replace("\t", new String(' ', tabSpacingSize));
            }
            catch (Exception)
            {
                return text;
            }
        }

        /// <summary>
        /// Replace all spaces with tabs given the text and tab spacing size
        /// </summary>
        /// <param name="text"></param>
        /// <param name="tabSpacingSize"></param>
        /// <returns></returns>
        public static string SpaceToTabConversion(string text, int tabSpacingSize)
        {
            try
            {
                return text.Replace(new string(' ', tabSpacingSize), "\t");
            }
            catch (Exception)
            {
                return text;
            }
        }
    }
}
