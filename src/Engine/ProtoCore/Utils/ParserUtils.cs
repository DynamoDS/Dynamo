using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProtoCore.Utils
{
    /// <summary>
    /// Parse result.
    /// </summary>
    public class ParseResult
    {
        /// <summary>
        /// All code related AST nodes will be saved in a CodeBlockNode.
        /// </summary>
        public AST.AssociativeAST.CodeBlockNode CodeBlockNode
        {
            get; set;
        }

        /// <summary>
        /// All comment related AST nodes will be saved in a CodeBlockNode.
        /// </summary>
        public AST.AssociativeAST.CodeBlockNode CommentBlockNode
        {
            get; set;
        }
    }

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

        public static List<AST.AssociativeAST.AssociativeNode> GetAstNodes(AST.AssociativeAST.CodeBlockNode codeBlockNode)
        {
            var nodes = new List<AST.AssociativeAST.AssociativeNode>();
            if (codeBlockNode != null)
            {
                nodes.AddRange(codeBlockNode.Body);
            }
            return nodes;
        }

        /// <summary>
        /// Parses designscript code and returns a ProtoAST CodeBlockNode
        /// </summary>
        /// <param name="code"> Source code to parse </param>
        public static AST.AssociativeAST.CodeBlockNode Parse(string code)
        {
            Validity.Assert(code != null);

            Core core = new Core(new Options());
            core.Options.ExecutionMode = ExecutionMode.Serial;
            core.ParsingMode = ParseMode.AllowNonAssignment;
            core.IsParsingCodeBlockNode = true;
            core.IsParsingPreloadedAssembly = false;

            var parseResult = ParseWithCore(code, core);
            return parseResult.CodeBlockNode;
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
        public static ParseResult ParseWithCore(string code, ProtoCore.Core core)
        {
            var p = CreateParser(code, core);
            p.Parse();

            ParseResult result = new ParseResult();
            result.CodeBlockNode = p.root as AST.AssociativeAST.CodeBlockNode;
            result.CommentBlockNode = p.commentNode as AST.AssociativeAST.CodeBlockNode;
            return result;
        }

        /// <summary>
        /// Parse simple RHS expression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static AST.AssociativeAST.AssociativeNode ParseRHSExpression(string expression, Core core)
        {
            if (string.IsNullOrEmpty(expression))
                throw new ArgumentException("expression");

            if (core == null)
                throw new ArgumentException("core");

            var currentParsingMode = core.ParsingMode;
            var currentParsingFlag = core.IsParsingCodeBlockNode;

            core.ParsingMode = ParseMode.AllowNonAssignment;
            core.IsParsingCodeBlockNode = true;

            AST.AssociativeAST.CodeBlockNode cbn = null;
            try
            {
                expression = expression.Trim();
                if (!expression.EndsWith(";"))
                    expression += ";";

                expression = "__dummy = " + expression;
                ParseResult parseResult = ParseWithCore(expression, core);
                cbn = parseResult.CodeBlockNode;
            }
            catch (BuildHaltException)
            {
            }

            core.ParsingMode = currentParsingMode;
            core.IsParsingCodeBlockNode = currentParsingFlag;

            if (cbn == null || !cbn.Body.Any())
                return null;

            var expr = cbn.Body[0] as AST.AssociativeAST.BinaryExpressionNode;
            return expr == null ? null : expr.RightNode;
        }
    }
}
