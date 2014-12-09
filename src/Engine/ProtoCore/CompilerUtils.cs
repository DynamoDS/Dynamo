using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoCore.Utils
{
    /* Variable Line Struct
     * Used to present the pair value of Line number and Variable Name.
     * 
     * @variable    : string, name of the variable refered to
     * @line        : int, line number of the variable in the code block
     */
    public struct VariableLine
    {
        #region Struct Data Member

        public int line;
        public string variable;

        #endregion

        #region Struct Public Methods

        public VariableLine(string variable, int line)
        {
            if (line < 0)
                throw new ArgumentException("Argument cannot be negative", "line");
            if (string.IsNullOrEmpty(variable))
                throw new ArgumentException("Invalid variable name", "variable");

            this.variable = variable;
            this.line = line;
        }

        public VariableLine(int line) // Only used for NUnit purposes (with temp names).
        {
            if (line < 0)
                throw new ArgumentException("Argument cannot be negative", "line");

            this.variable = string.Empty; // In NUnit temp names are ignored.
            this.line = line;
        }

        public static bool operator ==(VariableLine variableLine1, VariableLine variableLine2)
        {
            return variableLine1.Equals(variableLine2);
        }

        public static bool operator !=(VariableLine variableLine1, VariableLine variableLine2)
        {
            return !variableLine1.Equals(variableLine2);
        }

        /*
         * Check if two objects have same Variable Name and Line Number.
         * @return true if they have same Variable Name and Line Number or both are null
         *         false if otherwise
         */
        public override bool Equals(object obj)
        {
            if (obj is VariableLine)
            {
                return this.Equals((VariableLine)obj);
            }
            return false;
        }

        /*
         * Get Hashcode from combining Hashcode of two attribute @variable and @line
         */
        public override int GetHashCode()
        {
            return variable.GetHashCode() ^ line;
        }

        #endregion

        #region Struct Private Methods

        private bool Equals(VariableLine variableLine)
        {
            return (this.variable == variableLine.variable) && (this.line == variableLine.line);
        }

        #endregion
    }

    public class ParseParam
    {
        private List<string> temporaries = null;
        private List<string> unboundIdentifiers = null;
        private List<ProtoCore.AST.Node> parsedNodes = null;
        private List<ProtoCore.BuildData.ErrorEntry> errors = null;
        private List<ProtoCore.BuildData.WarningEntry> warnings = null;

        public ParseParam(System.Guid postfixGuid, System.String code)
        {
            this.PostfixGuid = postfixGuid;
            this.OriginalCode = code;
        }

        public void AppendTemporaryVariable(string variable)
        {
            if (this.temporaries == null)
                this.temporaries = new List<string>();

            this.temporaries.Add(variable);
        }

        public void AppendUnboundIdentifier(string identifier)
        {
            if (this.unboundIdentifiers == null)
                this.unboundIdentifiers = new List<string>();

            if (!this.unboundIdentifiers.Contains(identifier))
                this.unboundIdentifiers.Add(identifier);
        }

        public void AppendParsedNodes(IEnumerable<ProtoCore.AST.Node> parsedNodes)
        {
            if (this.parsedNodes == null)
                this.parsedNodes = new List<ProtoCore.AST.Node>();

            this.parsedNodes.AddRange(parsedNodes);
        }

        public void AppendErrors(IEnumerable<ProtoCore.BuildData.ErrorEntry> errors)
        {
            if (errors == null || (errors.Count() <= 0))
                return;

            if (this.errors == null)
                this.errors = new List<ProtoCore.BuildData.ErrorEntry>();

            this.errors.AddRange(errors);
        }

        public void AppendWarnings(IEnumerable<ProtoCore.BuildData.WarningEntry> warnings)
        {
            if (warnings == null || (warnings.Count() <= 0))
                return;

            if (this.warnings == null)
                this.warnings = new List<ProtoCore.BuildData.WarningEntry>();

            this.warnings.AddRange(warnings);
        }

        #region Public Class Properties

        public System.Guid PostfixGuid { get; private set; }
        public System.String OriginalCode { get; private set; }
        public System.String ProcessedCode { get; internal set; }

        public IEnumerable<System.String> Temporaries
        {
            get { return this.temporaries; }
        }

        public IEnumerable<System.String> UnboundIdentifiers
        {
            get { return unboundIdentifiers; }
        }

        public IEnumerable<ProtoCore.AST.Node> ParsedNodes
        {
            get { return this.parsedNodes; }
        }

        public IEnumerable<ProtoCore.BuildData.ErrorEntry> Errors
        {
            get { return this.errors; }
        }

        public IEnumerable<ProtoCore.BuildData.WarningEntry> Warnings
        {
            get { return this.warnings; }
        }

        #endregion
    }

    public class CompilerUtils
    {
        /// <summary>
        /// Does the first pass of compilation and returns a list of wanrnings in compilation
        /// </summary>
        /// <param name="code"></param>
        /// <param name="core"></param>
        /// <param name="blockId"></param>
        /// <returns></returns>
        public static ProtoCore.BuildStatus PreCompile(string code, Core core, CodeBlockNode codeBlock, out int blockId)
        {
            core.ExecMode = ProtoCore.DSASM.InterpreterMode.kNormal;

            blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            try
            {
                //defining the global Assoc block that wraps the entire .ds source file
                ProtoCore.LanguageCodeBlock globalBlock = new ProtoCore.LanguageCodeBlock();
                globalBlock.language = ProtoCore.Language.kAssociative;
                globalBlock.body = code;
                //the wrapper block can be given a unique id to identify it as the global scope
                globalBlock.id = ProtoCore.LanguageCodeBlock.OUTERMOST_BLOCK_ID;


                //passing the global Assoc wrapper block to the compiler
                ProtoCore.CompileTime.Context context = new ProtoCore.CompileTime.Context();
                ProtoCore.Language id = globalBlock.language;

                core.Executives[id].Compile(out blockId, null, globalBlock, context, codeBlockNode: codeBlock);

                core.BuildStatus.ReportBuildResult();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                if (!(ex is ProtoCore.BuildHaltException))
                {
                    throw ex;
                }
            }

            return core.BuildStatus;
        }

        public static bool TryLoadAssemblyIntoCore(Core core, string assemblyPath)
        {
            bool parsingPreloadFlag = core.IsParsingPreloadedAssembly;
            bool parsingCbnFlag = core.IsParsingCodeBlockNode;
            core.IsParsingPreloadedAssembly = true;
            core.IsParsingCodeBlockNode = false;

            int blockId;
            string importStatement = @"import (""" + assemblyPath + @""");";

            core.ResetForPrecompilation();
            var status = PreCompile(importStatement, core, null, out blockId);

            core.IsParsingPreloadedAssembly = parsingPreloadFlag;
            core.IsParsingCodeBlockNode = parsingCbnFlag;

            return status.ErrorCount == 0;
        }

        /// <summary>
        /// Pre-compiles DS code in code block node, 
        /// checks for syntax, converts non-assignments to assignments,
        /// stores list of AST nodes, errors and warnings
        /// Evaluates and stores list of unbound identifiers
        /// </summary>
        /// <param name="parseParams"></param>
        /// <returns></returns>
        public static bool PreCompileCodeBlock(Core core, ref ParseParam parseParams)
        {
            string postfixGuid = parseParams.PostfixGuid.ToString().Replace("-", "_");

            // Parse code to generate AST and add temporaries to non-assignment nodes
            IEnumerable<ProtoCore.AST.Node> astNodes = ParseUserCode(core, parseParams.OriginalCode, postfixGuid);

            // Catch the syntax errors and errors for unsupported 
            // language constructs thrown by compile expression
            if (core.BuildStatus.ErrorCount > 0)
            {
                parseParams.AppendErrors(core.BuildStatus.Errors);
                parseParams.AppendWarnings(core.BuildStatus.Warnings);
                return false;
            }
            parseParams.AppendParsedNodes(astNodes);

            // Compile the code to get the resultant unboundidentifiers  
            // and any errors or warnings that were caught by the compiler and cache them in parseParams
            return CompileCodeBlockAST(core, parseParams);
        }

        private static bool CompileCodeBlockAST(Core core, ParseParam parseParams)
        {
            Dictionary<int, List<VariableLine>> unboundIdentifiers = new Dictionary<int, List<VariableLine>>();
            IEnumerable<BuildData.WarningEntry> warnings = null;

            ProtoCore.BuildStatus buildStatus = null;
            try
            {
                int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
                CodeBlockNode codeblock = new CodeBlockNode();
                List<AssociativeNode> nodes = new List<AssociativeNode>();
                foreach (var i in parseParams.ParsedNodes)
                {
                    AssociativeNode assocNode = i as AssociativeNode;

                    if (assocNode != null)
                        nodes.Add(NodeUtils.Clone(assocNode));
                }
                codeblock.Body.AddRange(nodes);

                bool parsingPreloadFlag = core.IsParsingPreloadedAssembly;
                bool parsingCbnFlag = core.IsParsingPreloadedAssembly;
                core.IsParsingPreloadedAssembly = false;
                core.IsParsingCodeBlockNode = true;

                core.ResetForPrecompilation();
                buildStatus = PreCompile(string.Empty, core, codeblock, out blockId);

                core.IsParsingCodeBlockNode = parsingCbnFlag;
                core.IsParsingPreloadedAssembly = parsingPreloadFlag;

                parseParams.AppendErrors(buildStatus.Errors);
                parseParams.AppendWarnings(buildStatus.Warnings);

                if (buildStatus.ErrorCount > 0)
                {
                    return false;
                }
                warnings = buildStatus.Warnings;

                // Get the unboundIdentifiers from the warnings
                GetInputLines(parseParams.ParsedNodes, warnings, unboundIdentifiers);
                foreach (KeyValuePair<int, List<VariableLine>> kvp in unboundIdentifiers)
                {
                    foreach (VariableLine vl in kvp.Value)
                        parseParams.AppendUnboundIdentifier(vl.variable);
                }

                return true;
            }
            catch (Exception)
            {
                buildStatus = null;
                return false;
            }
        }

        private static IEnumerable<ProtoCore.AST.Node> ParseUserCode(ProtoCore.Core core, string expression, string postfixGuid)
        {
            IEnumerable<ProtoCore.AST.Node> astNodes = new List<ProtoCore.AST.Node>();

            if (expression == null)
                return astNodes;

            expression = expression.Replace("\r\n", "\n");

            try
            {
                return ParseUserCodeCore(core, expression);
            }
            catch
            {
                // For modifier blocks, language blocks, etc. that are currently ignored
                return astNodes;
            }
        }

        private static IEnumerable<AST.Node> ParseUserCodeCore(Core core, string expression)
        {
            List<ProtoCore.AST.Node> astNodes = new List<ProtoCore.AST.Node>();

            core.ResetForPrecompilation();
            core.IsParsingCodeBlockNode = true;
            core.ParsingMode = ParseMode.AllowNonAssignment;

            ProtoCore.AST.Node codeBlockNode = ProtoCore.Utils.ParserUtils.ParseWithCore(expression, core);
            List<ProtoCore.AST.Node> nodes = ParserUtils.GetAstNodes(codeBlockNode);
            Validity.Assert(nodes != null);

            int index = 0;
            foreach (var node in nodes)
            {
                ProtoCore.AST.AssociativeAST.AssociativeNode n = node as ProtoCore.AST.AssociativeAST.AssociativeNode;
                ProtoCore.Utils.Validity.Assert(n != null);

                // Append the temporaries only if it is not a function def or class decl
                bool isFunctionOrClassDef = n is FunctionDefinitionNode || n is ClassDeclNode;

                // Handle non Binary expression nodes separately
                if (n is ProtoCore.AST.AssociativeAST.ModifierStackNode)
                {
                    core.BuildStatus.LogSemanticError(StringConstants.modifierBlockNotSupported);
                }
                else if (n is ProtoCore.AST.AssociativeAST.ImportNode)
                {
                    core.BuildStatus.LogSemanticError(StringConstants.importStatementNotSupported);
                }
                else if (isFunctionOrClassDef)
                {
                    // Add node as it is
                    astNodes.Add(node);
                }
                else
                {
                    // Handle temporary naming for temporary Binary exp. nodes and non-assignment nodes
                    BinaryExpressionNode ben = node as BinaryExpressionNode;
                    if (ben != null && ben.Optr == ProtoCore.DSASM.Operator.assign)
                    {
                        ModifierStackNode mNode = ben.RightNode as ModifierStackNode;
                        if (mNode != null)
                        {
                            core.BuildStatus.LogSemanticError(StringConstants.modifierBlockNotSupported);
                        }
                        IdentifierNode lNode = ben.LeftNode as IdentifierNode;
                        if (lNode != null && lNode.Value == ProtoCore.DSASM.Constants.kTempProcLeftVar)
                        {
                            string name = Constants.kTempVarForNonAssignment + index.ToString();
                            BinaryExpressionNode newNode = new BinaryExpressionNode(new IdentifierNode(name), ben.RightNode);
                            astNodes.Add(newNode);
                            index++;
                        }
                        else
                        {
                            // Add node as it is
                            astNodes.Add(node);
                        }
                    }
                    else
                    {
                        // These nodes are non-assignment nodes
                        string name = Constants.kTempVarForNonAssignment + index.ToString();
                        BinaryExpressionNode newNode = new BinaryExpressionNode(new IdentifierNode(name), n);
                        astNodes.Add(newNode);
                        index++;
                    }
                }
            }
            return astNodes;
        }

        private static bool IsNotAssigned(string code)
        {
            code = code.Trim(';', ' ');
            if (string.IsNullOrEmpty(code))
                return true;
            bool hasLHS = code.Contains("=");
            return hasLHS;
        }

        private static void GetInputLines(IEnumerable<ProtoCore.AST.Node> astNodes,
                                   IEnumerable<ProtoCore.BuildData.WarningEntry> warnings,
                                   Dictionary<int, List<VariableLine>> inputLines)
        {
            List<VariableLine> warningVLList = GetVarLineListFromWarning(warnings);

            if (warningVLList.Count == 0)
                return;

            int stmtNumber = 1;
            foreach (var node in astNodes)
            {
                // Only binary expression need warnings. 
                // Function definition nodes do not have input and output ports
                if (node is ProtoCore.AST.AssociativeAST.BinaryExpressionNode)
                {
                    List<VariableLine> variableLineList = new List<VariableLine>();
                    foreach (var warning in warningVLList)
                    {
                        if (warning.line >= node.line && warning.line <= node.endLine)
                            variableLineList.Add(warning);
                    }

                    if (variableLineList.Count > 0)
                    {
                        inputLines.Add(stmtNumber, variableLineList);
                    }
                    stmtNumber++;
                }
            }
        }

        private static List<VariableLine> GetVarLineListFromWarning(IEnumerable<ProtoCore.BuildData.WarningEntry> warnings)
        {
            List<VariableLine> result = new List<VariableLine>();
            foreach (ProtoCore.BuildData.WarningEntry warningEntry in warnings)
            {
                if (warningEntry.ID == ProtoCore.BuildData.WarningID.kIdUnboundIdentifier)
                {
                    result.Add(new VariableLine()
                    {
                        variable = warningEntry.Message.Split(' ')[1].Replace("'", ""),
                        line = warningEntry.Line
                    });
                }
            }
            return result;
        }
    }
}
