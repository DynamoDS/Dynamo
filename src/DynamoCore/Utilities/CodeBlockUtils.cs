﻿using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Dynamo.Utilities
{
    public class CodeBlockUtils
    {
        /// <summary>
        /// Call this method to turn all "\r\n" and "\r" 
        /// occurrences in the given string into "\n".
        /// </summary>
        /// <param name="text">The text to be normalized</param>
        /// <returns>Returns the normalized string.</returns>
        /// 
        public static string NormalizeLineBreaks(string text)
        {
            text = text.Replace("\r\n", "\n");
            return text.Replace("\r", "\n");
        }

        /// <summary>
        /// Call this method to generate a list of PortData from given set of 
        /// unbound identifiers. This method ensures that the generated ports 
        /// are only having names that do not exceed a preconfigured length.
        /// </summary>
        /// <param name="unboundIdents">A list of unbound identifiers for which 
        /// input port data is to be generated. This list can be empty but it 
        /// cannot be null.</param>
        /// <returns>Returns a list of input port data generated based on the 
        /// input unbound identifier list.</returns>
        /// 
        public static IEnumerable<PortData> GenerateInputPortData(
            IEnumerable<string> unboundIdents)
        {
            if (unboundIdents == null)
                throw new ArgumentNullException("unboundIdents");

            int maxLength = Configurations.CBNMaxPortNameLength;
            List<PortData> inputPorts = new List<PortData>();

            foreach (string name in unboundIdents)
            {
                string portName = name;
                if (portName.Length > maxLength)
                    portName = portName.Remove(maxLength - 3) + "...";

                inputPorts.Add(new PortData(portName, name));
            }

            return inputPorts;
        }

        /// <summary>
        /// Call this method to get a list of lists of variables defined in 
        /// the given set of Statement objects. This method is typically used 
        /// in conjunction with DoesStatementRequireOutputPort method.
        /// </summary>
        /// <param name="statements">A list of Statement objects whose defined 
        /// variables are to be retrieved. This list can be empty but it cannot 
        /// be null.</param>
        /// <param name="onlyTopLevel">Set this parameter to false to retrieve 
        /// all variables defined in nested Statement objects.</param>
        /// <returns>Returns a list of lists of variables defined by the given 
        /// set of Statement objects.</returns>
        /// 
        public static IEnumerable<IEnumerable<string>> GetStatementVariables(
            IEnumerable<Statement> statements, bool onlyTopLevel)
        {
            if (statements == null)
                throw new ArgumentNullException("statements");

            var definedVariables = new List<List<string>>();
            foreach (var statement in statements)
            {
                definedVariables.Add(Statement.GetDefinedVariableNames(
                    statement, onlyTopLevel));
            }

            return definedVariables;
        }

        /// <summary>
        /// Checks wheter an outport is required for a Statement with the given 
        /// index. An outport is not required if there are no defined variables 
        /// or if any of the defined variables have been declared again later on
        /// in the same code block.
        /// </summary>
        /// <param name="statementVariables">A list of lists, each of which 
        /// contains variables defined by a Statement at the index. This list 
        /// can be obtained from calling GetStatementVariables method.</param>
        /// <param name="index">The index of the Statement for which this call 
        /// is made.</param>
        /// <returns>Returns true if an output port is required, or false 
        /// otherwise.</returns>
        /// 
        public static bool DoesStatementRequireOutputPort(
            IEnumerable<IEnumerable<string>> statementVariables, int index)
        {
            if (statementVariables == null)
                throw new ArgumentNullException("statementVariables");

            int statementCount = statementVariables.Count();
            if (statementCount <= 0)
                return false;

            if (index < 0 || (index >= statementCount))
                throw new IndexOutOfRangeException("index");

            if (!statementVariables.ElementAt(index).Any())
                return false;
           
            var currentVariables = statementVariables.ElementAt(index);
            for (int stmt = index + 1; stmt < statementCount; stmt++)
            {
                var variables = statementVariables.ElementAt(stmt);
                foreach (var cv in currentVariables)
                {
                    if (variables.Contains(cv))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Call this method to map logical lines in the given text input to their
        /// corresponding visual line index. Due to wrapping behavior, a long line
        /// may be wrapped into more than one line due to width constraint. For an
        /// example:
        /// 
        ///     0 This is a longer line that will be wrapped around to next line
        ///     1 This is a shorter line
        /// 
        /// The wrapped results will be as follow:
        /// 
        ///     0 This is a longer line that will 
        ///       be wrapped around to next line
        ///     1 This is a shorter line
        /// 
        /// The resulting array will be:
        /// 
        ///     result = { 0, 2 }
        /// 
        /// It means that the first logical line (with index 0) will be mapped to 
        /// line 0 visually; the second logical line (with index 1) will be mapped 
        /// to line 2 visually.
        /// 
        /// </summary>
        /// <param name="text">The input text for the mapping.</param>
        /// <returns>Returns a list of visual line indices. For an example, if the 
        /// result is { 0, 6, 27 }, then the first logical line (index 0) is mapped 
        /// to visual line with index 0; second logical line (index 1) is mapped to 
        /// visual line with index 6; third logical line (index 2) is mapped to 
        /// visual line with index 27.</returns>
        /// 
        public static IEnumerable<int> MapLogicalToVisualLineIndices(string text)
        {
            var logicalToVisualLines = new List<int>();
            if (string.IsNullOrEmpty(text))
                return logicalToVisualLines;

            text = NormalizeLineBreaks(text);
            var lines = text.Split(new char[] { '\n' }, StringSplitOptions.None);

            // We could have hard-coded "pack" instead of "UriSchemePack" here, 
            // but in NUnit scenario there is no "Application" created. When there 
            // is no Application instance, the Uri format "pack://" will fail Uri 
            // object creation. Adding a reference to "UriSchemePack" resolves 
            // this issue to avoid a "UriFormatException".
            // 
            string pack = System.IO.Packaging.PackUriHelper.UriSchemePack;
            var uri = new Uri(pack + "://application:,,,/DynamoCore;component/");
            var textFontFamily = new FontFamily(uri, ResourceNames.FontResourceUri);

            var typeface = new Typeface(textFontFamily, FontStyles.Normal,
                FontWeights.Normal, FontStretches.Normal);

            int totalVisualLinesSoFar = 0;
            foreach (var line in lines)
            {
                FormattedText ft = new FormattedText(
                    line, CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, typeface,
                    Configurations.CBNFontSize, Brushes.Black)
                {
                    MaxTextWidth = Configurations.CBNMaxTextBoxWidth,
                    Trimming = TextTrimming.None
                };

                logicalToVisualLines.Add(totalVisualLinesSoFar);

                // Empty lines (i.e. those with just a "\n" character) will result 
                // in "ft.Extent" to be 0.0, but the line still occupies one line
                // visually. This is why we need to make sure "lineCount" cannot be 
                // zero.
                // 
                var lineCount = Math.Floor(ft.Extent / Configurations.CBNFontSize);
                totalVisualLinesSoFar += (lineCount < 1.0 ? 1 : ((int)lineCount));
            }

            return logicalToVisualLines;
        }

        /// <summary>
        /// Call this method to format user codes in the following ways:
        /// 
        /// 1. Heading and trailing whitespaces are removed from the original 
        ///    string. Characters that quality as "whitespaces" are: '\n', '\t'
        ///    and ' '.
        /// 
        /// 2. Multiple statements on a single line will be broken down further 
        ///    into multiple statements. For example, "a = 1; b = 2;" will be 
        ///    broken down into two lines: "a = 1;\nb = 2;" (line break denoted 
        ///    by the new \n character).
        /// 
        /// 3. Heading whitespaces will be removed ony for the first line. This 
        ///    is to preserve the indentation for lines other than the first.
        /// 
        /// 4. If the resulting codes do not end with a closing curly bracket '}',
        ///    then a semi-colon is appended to the code. This ensures codes like 
        ///    "a" will result in codes becoming "a;"
        /// 
        /// </summary>
        /// <param name="inputCode">Original code content as typed in by the user.
        /// </param>
        /// <returns>Returns the formatted code with the above process.</returns>
        /// 
        public static string FormatUserText(string inputCode)
        {
            if (inputCode == null)
                return string.Empty;

            // Trailing and preceeding whitespaces removal.
            var charsToTrim = new char[] { '\n', '\t', ' ' };
            inputCode = NormalizeLineBreaks(inputCode);
            inputCode = inputCode.Trim(charsToTrim);

            List<string> statements = new List<string>();
            var splitOption = StringSplitOptions.RemoveEmptyEntries;

            // Break the input string into lines based on the \n characters that
            // are already in the string. Note that after breaking the string, 
            // each line can still contain multiple statements (e.g. "a = 1; b;"
            // that does not contain a \n between the two statements).
            // 
            var lines = inputCode.Split('\n');
            foreach (var line in lines)
            {
                if (line.IndexOf(';') == -1)
                {
                    // The line does not have any semi-colon originally. We know 
                    // this is a line by itself, but may or may not represent a 
                    // statement. But since this line (potentially an empty one)
                    // exists in the original user string, it needs to go into 
                    // the resulting statement list.
                    // 
                    var trimmed = line.TrimEnd(charsToTrim);
                    statements.Add(trimmed + "\n");
                }
                else
                {
                    // This line potentially contains more than one statements 
                    // (e.g. "a = 1; b = 2;"), or it might even be a single 
                    // statement (e.g. "a = 1;  " with trailing spaces). After 
                    // breaking each line up into statements, it is important 
                    // that only non-empty lines go into the resulting statement 
                    // list, and not the empty ones (for the case of "a = 1; ").
                    // 
                    var parts = line.Split(new char[] { ';' }, splitOption);
                    foreach (var part in parts)
                    {
                        var trimmed = part.TrimEnd(charsToTrim);
                        if (!string.IsNullOrEmpty(trimmed))
                            statements.Add(trimmed + ";\n");
                    }
                }
            }

            // Now join all the statements together into one single code, and 
            // remove the final trailing white spaces (including the last \n).
            inputCode = statements.Aggregate("", (curr, stmt) => curr + stmt);
            inputCode = inputCode.TrimEnd(charsToTrim);

            // If after all the processing we do not end up with an empty code,
            // then we may need a semi-colon at the end. This is provided if the 
            // code does not end with a closing curly bracket (in which case a 
            // trailing semi-colon is not required).
            // 
            if (!string.IsNullOrEmpty(inputCode) && (!inputCode.EndsWith("}")))
            {
                if (inputCode.EndsWith(";") == false)
                    inputCode = inputCode + ";";
            }

            return inputCode;
        }
    }
}
