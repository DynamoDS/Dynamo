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
            int endLine = node.endLine;
            int endCol = node.endCol;

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
    }
}
