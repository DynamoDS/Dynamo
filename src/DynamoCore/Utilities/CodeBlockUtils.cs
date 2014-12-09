using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.UI;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
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

                inputPorts.Add(new PortData(portName, name)
                {
                    Height = Configurations.CodeBlockPortHeightInPixels
                });
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
        /// 1. Leading and trailing whitespaces are removed from the original 
        ///    string. Characters that qualify as "whitespaces" are: '\n', '\t'
        ///    and ' '.
        /// 
        /// 2. Multiple statements on a single line will be broken down further 
        ///    into multiple statements. For example, "a = 1; b = 2;" will be 
        ///    broken down into two lines: "a = 1;\nb = 2;" (line break denoted 
        ///    by the new \n character).
        /// 
        /// 3. Leading whitespaces will be removed ony for the first line. This 
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
            // code does not end with a comment or string (in which case a 
            // trailing semi-colon is not required).
            // 
            if (!string.IsNullOrEmpty(inputCode) && 
                !CodeCompletionParser.IsInsideCommentOrString(inputCode, inputCode.Length))
            {
                if (inputCode.EndsWith(";") == false)
                    inputCode = inputCode + ";";
            }

            return inputCode;
        }

        /// <summary>
        /// Returns a list of defined variables, along with the line number on which 
        /// they are defined last. A variable can be defined multiple times in a single 
        /// code block node, but the output port is only shown on the last definition.
        /// </summary>
        /// <returns>Returns a map between defined variables and the line index on 
        /// which they are defined last.</returns>
        public static IOrderedEnumerable<KeyValuePair<string, int>> GetDefinitionLineIndexMap(IEnumerable<Statement> codeStatements)
        {
            // Get all defined variables and their locations
            var definedVars = codeStatements.Select(s => new KeyValuePair<Variable, int>(s.FirstDefinedVariable, s.StartLine))
                                            .Where(pair => pair.Key != null)
                                            .Select(pair => new KeyValuePair<string, int>(pair.Key.Name, pair.Value))
                                            .OrderBy(pair => pair.Key)
                                            .GroupBy(pair => pair.Key);

            // Calc each variable's last location of definition
            var locationMap = new Dictionary<string, int>();
            foreach (var defs in definedVars)
            {
                var name = defs.FirstOrDefault().Key;
                var loc = defs.Select(p => p.Value).Max<int>();
                locationMap[name] = loc;
            }

            return locationMap.OrderBy(p => p.Value);
        }

        public static HighlightingRule CreateDigitRule()
        {
            var digitRule = new HighlightingRule();

            Color color = (Color)ColorConverter.ConvertFromString("#2585E5");
            digitRule.Color = new HighlightingColor()
            {
                Foreground = new CustomizedBrush(color)
            };

            // These Regex's must match with the grammars in the DS ATG for digits
            // Refer to the 'number' and 'float' tokens in Start.atg
            //*******************************************************************************
            // number = digit {digit} .
            // float = digit {digit} '.' digit {digit} [('E' | 'e') ['+'|'-'] digit {digit}].
            //*******************************************************************************

            string digit = @"(-?\b\d+)";
            string floatingPoint = @"(\.[0-9]+)";
            string numberWithOptionalDecimal = digit + floatingPoint + "?";

            string exponent = @"([eE][+-]?[0-9]+)";
            string numberWithExponent = digit + floatingPoint + exponent;

            digitRule.Regex = new Regex(numberWithExponent + "|" + numberWithOptionalDecimal);

            return digitRule;
        }
    }

    /// <summary>
    /// This class exposes utility methods to:
    /// 1. extract variable types from variable declarations such as 'Point' from a : Point;
    /// 2. extract the string to autocomplete on from a block of code
    /// </summary>
    internal class CodeCompletionParser
    {
        #region private members

        private readonly string text;

        // This should match with production for identifier in language parser
        // See Start.atg file: ident = (letter | '_' | '@'){letter | digit | '_' | '@'}.
        private static string variableNamePattern = @"[a-zA-Z_@]([a-zA-Z_@0-9]*)";

        private static string spacesOrNonePattern = @"(\s*)";
        private static string colonPattern = ":";

        // This pattern matches with identifier lists such as Autodesk.DesignScript.Geometry.Point
        private static string identifierListPattern = string.Format("{0}([.]({0})+)*", variableNamePattern);

        // Maintains a stack of symbols in a nested expression being typed
        // where the symbols are nested based on brackets, braces or parentheses
        // An expression like: x[y[z.foo(). will be stacked up as follows:
        // z.foo().
        // y[
        // x[
        // The stack is then popped upon closing the brackets, and the popped value is transferred to the top:
        // y[z.foo()].
        // x[
        // So at any given time the top of the stack will contain the string to be completed
        private Stack<string> expressionStack = new Stack<string>();

        /// <summary>
        /// Expression to autocomplete on is on top of 'expressionStack'
        /// </summary>
        private string strPrefix = String.Empty;

        /// <summary>
        /// Function call being currently typed
        /// </summary>
        private string functionName = String.Empty;

        private int argCount = 0;

        /// <summary>
        /// Identifier or Class name on which function being typed currently is invoked
        /// </summary>
        private string functionPrefix = String.Empty;

        private string type = string.Empty;

        // Context of string being typed for completion
        private bool isInSingleComment = false;
        private bool isInString = false;
        private bool isInChar = false;
        private bool isInMultiLineComment = false;

        #endregion

        private CodeCompletionParser(string text)
        {
            this.text = text;
        }

        #region public members

        /// <summary>
        /// Parses given block of code and declared variable,
        /// returns the type of the variable: e.g. in:
        /// "a : Point;" returns 'Point'
        /// </summary>
        /// <param name="code"> block of code being parsed </param>
        /// <param name="variableName">input declared variable: 'a' in example </param>
        /// <returns> returns Point in example </returns>
        public static string GetVariableType(string code, string variableName)
        {
            var symbolTable = FindVariableTypes(code);

            string type = null;
            symbolTable.TryGetValue(variableName, out type);

            return type;
        }

        /// <summary>
        /// Given the code that's currently being typed in a CBN,
        /// this function extracts the expression that needs to be code-completed
        /// e.g. given "abc.X[{xyz.b.foo((abc" it returns "abc"
        /// which is the "thing" that needs to be queried for completions
        /// </summary>
        /// <param name="code"></param>
        public static string GetStringToComplete(string code)
        {
            var codeParser = new CodeCompletionParser(code);
            // TODO: Discard complete code statements terminated by ';'
            // and extract only the current line being typed
            for (int i = 0; i < code.Length; ++i)
            {
                codeParser.ParseStringToComplete(code[i]);
            }
            return codeParser.strPrefix;
        }

        /// <summary>
        /// Given a block of code that's currently being typed 
        /// this returns the method name and the type name on which it is invoked
        /// e.g. "Point.ByCoordinates" returns 'ByCoordinates' as the functionName and 'Point' as functionPrefix
        /// "abc.X[{xyz.b.foo" returns 'foo' as the functionName and 'xyz.b' as the "functionPrefix" on which it is invoked
        /// </summary>
        /// <param name="code"> input code block </param>
        /// <param name="functionName"> output function name </param>
        /// <param name="functionPrefix"> output type or variable on which fn is invoked </param>
        public static void GetFunctionToComplete(string code, out string functionName, out string functionPrefix)
        {
            var codeParser = new CodeCompletionParser(code);
            // TODO: Discard complete code statements terminated by ';'
            // and extract only the current line being typed
            for (int i = 0; i < code.Length; ++i)
            {
                codeParser.ParseStringToComplete(code[i]);
            }
            functionName = codeParser.functionName;
            functionPrefix = codeParser.functionPrefix;
        }

        /// <summary>
        /// Parse text to determine if string being typed at caretPos is in 
        /// the context of a comment or string or character
        /// </summary>
        /// <param name="text"> input block of code </param>
        /// <param name="caretPos"> caret position in text at which to determine context </param>
        /// <returns> True if any of above context is true </returns>
        public static bool IsInsideCommentOrString(string text, int caretPos)
        {
            var lexer = new CodeCompletionParser(text);
            lexer.ParseContext(caretPos);
            return
                lexer.isInSingleComment ||
                    lexer.isInString ||
                    lexer.isInChar ||
                    lexer.isInMultiLineComment;
        }

        #endregion

        #region private methods

        private static Dictionary<string, string> FindVariableTypes(string code)
        {
            // Contains pairs of variable names and their types
            Dictionary<string, string> variableTypes = new Dictionary<string, string>();

            // This pattern is used to create a Regex to match expressions such as "a : Point" and add the Pair of ("a", "Point") to the dictionary
            string pattern = variableNamePattern + spacesOrNonePattern + colonPattern + spacesOrNonePattern + identifierListPattern;

            var varDeclarations = Regex.Matches(code, pattern);
            for (int i = 0; i < varDeclarations.Count; i++)
            {
                var match = varDeclarations[i].Value;
                match = Regex.Replace(match, @"\s", "");
                var groups = match.Split(':');

                // Overwrite variable type for a redefinition
                if (variableTypes.ContainsKey(groups[0]))
                {
                    variableTypes[groups[0]] = groups[1];
                }
                else
                    variableTypes.Add(groups[0], groups[1]);
            }

            return variableTypes;
        }

        private string ParseStringToComplete(char currentChar)
        {
            string prefix = string.Empty;
            switch (currentChar)
            {
                case ']':
                    strPrefix = PopFromExpressionStack(']');
                    break;
                case '[':
                    if (!string.IsNullOrEmpty(strPrefix))
                    {
                        strPrefix += '[';
                        expressionStack.Push(strPrefix);
                        strPrefix = string.Empty;
                    }
                    break;
                case '}':
                    strPrefix = PopFromExpressionStack('}');
                    break;
                case '{':
                    {
                        strPrefix = string.Empty;
                        strPrefix += '{';
                        expressionStack.Push(strPrefix);
                        strPrefix = string.Empty;
                    }
                    break;
                case ')':
                    argCount = 0;
                    strPrefix = PopFromExpressionStack(')');
                    break;
                case '(':
                    argCount = 1;
                    if (!string.IsNullOrEmpty(strPrefix))
                    {
                        // function call
                        // Auto-complete function signature  
                        // Class/Type and function name must be known at this point
                        functionName = GetMemberIdentifier();
                        if (string.Equals(strPrefix, functionName))
                            functionPrefix = string.Empty;
                        else
                            functionPrefix = strPrefix.Substring(0, strPrefix.Length - functionName.Length - 1);

                        expressionStack.Push(strPrefix + @"(");
                    }
                    else
                    {
                        // simple expression
                        expressionStack.Push(@"(");
                    }
                    strPrefix = string.Empty;
                    break;
                case '.':
                    strPrefix += '.';
                    break;
                case ' ':
                    break;
                case '=':
                    strPrefix = string.Empty;
                    expressionStack.Clear();
                    break;
                case ';':
                    strPrefix = string.Empty;
                    expressionStack.Clear();
                    break;
                default:
                    if (char.IsLetterOrDigit(currentChar))
                    {
                        strPrefix += currentChar;

                        if (strPrefix.IndexOf('.') != -1)
                        {
                            // If type exists, extract string after previous '.'                            
                            string identToComplete = GetMemberIdentifier();
                            // Auto-completion happens over type, search for identToComplete in type's auto-complete list
                        }

                    }
                    else
                    {
                        strPrefix = PopFromExpressionStack(currentChar);
                        expressionStack.Push(strPrefix);

                        strPrefix = string.Empty;
                    }
                    break;
            }
            return strPrefix;
        }

        private void ParseContext(int caretPos)
        {
            for (int i = 0; i < caretPos; i++)
            {
                char ch = text[i];
                char lookAhead = i + 1 < text.Length ? text[i + 1] : '\0';
                switch (ch)
                {
                    case '/':
                        if (isInString || isInChar || isInSingleComment || isInMultiLineComment)
                            break;
                        if (lookAhead == '/')
                        {
                            i++;
                            isInSingleComment = true;
                        }
                        if (lookAhead == '*')
                        {
                            isInMultiLineComment = true;
                            i++;
                        }
                        break;
                    case '*':
                        if (isInString || isInChar || isInSingleComment)
                            break;
                        if (lookAhead == '/')
                        {
                            i++;
                            isInMultiLineComment = false;
                        }
                        break;
                    case '\n':
                    case '\r':
                        isInSingleComment = false;
                        isInString = false;
                        isInChar = false;
                        break;
                    case '\\':
                        if (isInString || isInChar)
                            i++;
                        break;
                    case '"':
                        if (isInSingleComment || isInMultiLineComment || isInChar)
                            break;
                        isInString = !isInString;
                        break;
                    case '\'':
                        if (isInSingleComment || isInMultiLineComment || isInString)
                            break;
                        isInChar = !isInChar;
                        break;
                    default:
                        break;
                }
            }
        }

        private string GetMemberIdentifier()
        {
            return strPrefix.Split('.').Last();
        }

        private string PopFromExpressionStack(char currentChar)
        {
            string prefix = string.Empty;
            if (expressionStack.Count > 0)
            {
                prefix = expressionStack.Pop();
            }
            return prefix + strPrefix + currentChar;
        }
        #endregion

    }

    // Refer to link: 
    // http://stackoverflow.com/questions/11806764/adding-syntax-highlighting-rules-to-avalonedit-programmatically
    internal sealed class CustomizedBrush : HighlightingBrush
    {
        private readonly SolidColorBrush brush;
        public CustomizedBrush(Color color)
        {
            brush = CreateFrozenBrush(color);
        }

        public override Brush GetBrush(ITextRunConstructionContext context)
        {
            return brush;
        }

        public override string ToString()
        {
            return brush.ToString();
        }

        private static SolidColorBrush CreateFrozenBrush(Color color)
        {
            SolidColorBrush brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }
    }

}
