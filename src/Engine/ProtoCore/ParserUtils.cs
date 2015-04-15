using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ProtoCore.DSASM;
using ProtoCore.Lang.Replication;
using ProtoCore.Utils;

namespace ProtoCore.Utils
{
    /// <summary>
    /// These are string manipulation utility functions that focus on lexing and parsing heuristics
    /// </summary>
    public static class ParserUtils
    {
        private static string FindIdentifiers(string statement, int equalIndex)
        {

            // If there is a '{' before the first '=' in the statement,
            // split the string at '{' and extract the portion after it
            string identifier = null;
            int braceIndex = statement.IndexOf('{');

            if (braceIndex != -1)
            {
                if (braceIndex < equalIndex)
                {
                    statement = statement.Split('{')[1];
                }
            }

            identifier = statement.IndexOf('=') == -1 ? null : statement.Split('=')[0];

            return identifier;
        }

        public static List<string> GetLHSatAssignment(string line, int equalIndex)
        {
            var spaceNormalizationRule = new Regex(@"([^\S]+)");

            // the line could have multiple program statements separated by ';'
            var programStatements = line.Split(';');

            List<string> identifiers = new List<string>();
            foreach (var statement in programStatements)
            {
                if (statement.Equals(string.Empty))
                    continue;

                string identifier = null;
                identifier = FindIdentifiers(statement, equalIndex);

                if (identifier == null)
                    continue;
                else
                {
                    identifier = identifier.Trim();

                    if (identifier.StartsWith(@"//"))
                        continue;
                    else if (identifier.Equals("return"))
                        continue;

                    identifier = spaceNormalizationRule.Replace(identifier, string.Empty);

                    // Check if identifier contains ':' and extract the lhs of the ':'
                    identifier = identifier.Split(':')[0];

                    identifiers.Add(identifier);
                }
            }
            return identifiers;
        }

        /// <summary>
        /// Retrieves the lhs identifer of a string
        /// </summary>
        /// <returns></returns>
        public static string GetLHSString(string code)
        {
            if (code != null && code != string.Empty && code.Length > 0)
            {
                for (int n = 0; n < code.Length; ++n)
                {
                    if (code[n] == ' ' || code[n] == '=')
                    {
                        string lhs = code.Substring(0, n);

                        // We must also handle literal "\n" appearing in the identifier
                        lhs = lhs.Replace("\n", "");
                        return lhs;
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        ///  Splits the lines of code given deimiters
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static List<string> GetStatementsString(string input)
        {
            List<string> expr = new List<string>();
            int index = 0;
            int oldIndex = 0;
            do
            {
                index = input.IndexOf(";", index);
                if (index != -1)
                {
                    string sub;
                    if (index < input.Length - 2)
                    {
                        if (input[index + 1].Equals('\r') && input[index + 2].Equals('\n'))
                        {
                            index += 2;
                        }
                    }
                    sub = input.Substring(oldIndex, index - oldIndex + 1);
                    expr.Add(sub);
                    index++;
                    oldIndex = index;
                }
            } while (index != -1);
            return expr;
        }

        public static string ExtractStatementFromCode(string code, ProtoCore.AST.Node node)
        {
            int line = node.line;
            int col = node.col;

            string stmt = "";

            if (node is ProtoCore.AST.AssociativeAST.IdentifierNode || node is ProtoCore.AST.AssociativeAST.IdentifierListNode
                || node is ProtoCore.AST.AssociativeAST.FunctionCallNode || node is ProtoCore.AST.AssociativeAST.RangeExprNode
                || node is ProtoCore.AST.AssociativeAST.StringNode || node is ProtoCore.AST.AssociativeAST.IntNode
                || node is ProtoCore.AST.AssociativeAST.DoubleNode || node is ProtoCore.AST.AssociativeAST.BooleanNode
                || node is ProtoCore.AST.AssociativeAST.ExprListNode)
            {
                string[] lines = code.Split('\n');
                int lastLine = lines.Length;
                int lastCol = lines[lastLine - 1].Length;

                string stmts = ExtractStatementHelper(code, line, col, lastLine, lastCol + 1);
                string[] induvidualStmts = stmts.Split(';');
                stmt = induvidualStmts[0] + ";";
                if (induvidualStmts.Length > 1)
                {
                    foreach (char ch in induvidualStmts[1])
                        if (ch == ' ' || ch == '\n' || ch == '\t')
                            stmt += ch.ToString();
                        else
                            break;
                }
                return stmt;
            }

            stmt = ExtractStatementHelper(code, line, col, node.endLine, node.endCol);
            return stmt;
        }

        private static string ExtractStatementHelper(string code, int line, int col, int endLine, int endCol)
        {
            int linePos = 1;
            int charPos = 1;
            int startIndex = 0;
            int endIndex = 0;
            int prevLineLen = 0;

            while (charPos <= code.Length + 1)
            {
                if (linePos == line)
                {
                    if (charPos == prevLineLen + col)
                    {
                        startIndex = charPos;
                    }
                }
                if (linePos == endLine)
                {
                    if (charPos == prevLineLen + endCol)
                    {
                        if (charPos != code.Length + 1)
                            endIndex = charPos + 1;
                        else
                            endIndex = charPos;
                        while (charPos < code.Length && code[charPos] == '\n')
                        {
                            charPos++;
                            endIndex++;
                        }
                        break;
                    }

                }
                if (code[charPos - 1] == '\n')
                {
                    prevLineLen = charPos;
                    ++linePos;
                }

                ++charPos;

            }

            return code.Substring(startIndex - 1, (endIndex - startIndex));
        }

        public static List<ProtoCore.AST.Node> GetAstNodes(ProtoCore.AST.Node codeBlockNode)
        {
            List<ProtoCore.AST.Node> nodes = new List<ProtoCore.AST.Node>();
            ProtoCore.AST.AssociativeAST.CodeBlockNode cbn = codeBlockNode as ProtoCore.AST.AssociativeAST.CodeBlockNode;
            if (cbn != null)
            {
                foreach (var n in cbn.Body)
                {
                    nodes.Add(n);
                }
            }
            return nodes;
        }


        public static string ExtractCommentStatementFromCode(string code, ProtoCore.AST.AssociativeAST.CommentNode cnode)
        {
            string[] lines = code.Split('\n');
            int lastLine = lines.Length;
            int lastCol = lines[lastLine - 1].Length;
            string stmnt = ExtractStatementHelper(code, cnode.line, cnode.col, lastLine, lastCol);
            
            int commentLength = cnode.Value.Length;
            string comment = "";
            if (stmnt.Length > commentLength)
            {
                comment = stmnt.Remove(commentLength);
                for (int i = commentLength; i < stmnt.Length; i++)
                {
                    char ch = stmnt[i];
                    if (char.IsWhiteSpace(ch))
                        comment += ch.ToString();
                    else
                        break;
                }
            }
            else
                comment = stmnt;
            return comment;
        }

        /// <summary>
        /// Parses designscript code and returns a ProtoAST CodeBlockNode
        /// </summary>
        /// <param name="code"> Source code to parse </param>
        public static ProtoCore.AST.Node Parse(string code)
        {
            Validity.Assert(code != null);

            ProtoCore.Core core = new ProtoCore.Core(new ProtoCore.Options());
            core.Options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            core.ParsingMode = ProtoCore.ParseMode.AllowNonAssignment;
            core.IsParsingCodeBlockNode = true;
            core.IsParsingPreloadedAssembly = false;

            return ParseWithCore(code, core);
        }

        /// <summary>
        /// Return a parser for the DS code.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="core"></param>
        /// <param name="hasBuiltInLoaded"></param>
        /// <returns></returns>
        public static DesignScriptParser.Parser CreateParser(string code, ProtoCore.Core core)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(code);
            byte[] utf8Buffer = new byte[buffer.Length + 3];

            // Add UTF-8 BOM - Coco/R requires UTF-8 stream should contain BOM
            utf8Buffer[0] = (byte)0xEF;
            utf8Buffer[1] = (byte)0xBB;
            utf8Buffer[2] = (byte)0xBF;
            Array.Copy(buffer, 0, utf8Buffer, 3, buffer.Length);

            System.IO.MemoryStream memstream = new System.IO.MemoryStream(utf8Buffer);
            DesignScriptParser.Scanner scanner = new DesignScriptParser.Scanner(memstream);
            DesignScriptParser.Parser parser = new DesignScriptParser.Parser(scanner, core, core.builtInsLoaded);
            return parser;
        }

        /// <summary>
        /// Parses desginscript code with specified core and returns a 
        /// ProtoAST CodeBlockNode
        /// </summary>
        /// <param name="code"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static ProtoCore.AST.Node ParseWithCore(string code, ProtoCore.Core core)
        {
            var p = CreateParser(code, core);
            p.Parse();

            return p.root;
        }

        /// <summary>
        /// Parse simple RHS expression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static ProtoCore.AST.AssociativeAST.AssociativeNode ParseRHSExpression(string expression, ProtoCore.Core core)
        {
            if (string.IsNullOrEmpty(expression))
                throw new ArgumentException("expression");

            if (core == null)
                throw new ArgumentException("core");

            var currentParsingMode = core.ParsingMode;
            var currentParsingFlag = core.IsParsingCodeBlockNode;

            core.ParsingMode = ProtoCore.ParseMode.AllowNonAssignment;
            core.IsParsingCodeBlockNode = true;

            ProtoCore.AST.Node astNode = null;
            try
            {
                expression = expression.Trim();
                if (!expression.EndsWith(";"))
                    expression += ";";

                expression = "__dummy = " + expression;
                astNode = ParserUtils.ParseWithCore(expression, core);
            }
            catch (ProtoCore.BuildHaltException ex)
            {
            }

            core.ParsingMode = currentParsingMode;
            core.IsParsingCodeBlockNode = currentParsingFlag;

            if (astNode == null)
                return null;

            var cbn = astNode as ProtoCore.AST.AssociativeAST.CodeBlockNode;
            if (cbn != null && cbn.Body.Any())
            {
                var expr = cbn.Body[0] as ProtoCore.AST.AssociativeAST.BinaryExpressionNode;
                if (expr != null)
                    return expr.RightNode;
            }

            return null;
        }
    }
}
