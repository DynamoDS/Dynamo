using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
