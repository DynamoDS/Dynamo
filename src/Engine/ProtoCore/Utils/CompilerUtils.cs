using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProtoCore.AST;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.BuildData;
using ProtoCore.DSASM;
using ProtoCore.Namespace;
using ProtoCore.Properties;
using ProtoCore.SyntaxAnalysis;

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
        public string displayName;

        #endregion

        #region Struct Public Methods

        public VariableLine(string variable, int line)
        {
            if (line < 0)
                throw new ArgumentException("Argument cannot be negative", "line");
            if (string.IsNullOrEmpty(variable))
                throw new ArgumentException("Invalid variable name", "variable");

            this.displayName = this.variable = variable;
            this.line = line;
        }

        public VariableLine(int line) // Only used for NUnit purposes (with temp names).
        {
            if (line < 0)
                throw new ArgumentException("Argument cannot be negative", "line");

            this.displayName = this.variable = string.Empty; // In NUnit temp names are ignored.
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
        private Dictionary<string, string> unboundIdentifiers;
        private List<AssociativeNode> parsedNodes;
        private List<AssociativeNode> commentNodes;
        private List<BuildData.ErrorEntry> errors;
        private List<BuildData.WarningEntry> warnings;
        
        public ParseParam(System.Guid postfixGuid, System.String code, ElementResolver elementResolver)
        {
            this.PostfixGuid = postfixGuid;
            this.OriginalCode = code;
            this.ElementResolver = elementResolver;
            this.parsedNodes = new List<AssociativeNode>();
            this.commentNodes = new List<AssociativeNode>();
            this.errors = new List<BuildData.ErrorEntry>();
            this.warnings = new List<BuildData.WarningEntry>();
        }

        public void AppendUnboundIdentifier(string displayName, string identifier)
        {
            if (this.unboundIdentifiers == null)
                this.unboundIdentifiers = new Dictionary<string, string>();

            if (!this.unboundIdentifiers.ContainsKey(displayName))
                this.unboundIdentifiers.Add(displayName, identifier);
        }

        public void AppendParsedNodes(IEnumerable<AssociativeNode> parsedNodes)
        {
            if (parsedNodes == null || !parsedNodes.Any())
                return;

            this.parsedNodes.AddRange(parsedNodes);
        }

        public void AppendErrors(IEnumerable<ProtoCore.BuildData.ErrorEntry> errors)
        {
            if (errors == null || (errors.Count() <= 0))
                return;

            this.errors.AddRange(errors);
        }

        public void AppendWarnings(IEnumerable<ProtoCore.BuildData.WarningEntry> warnings)
        {
            if (warnings == null || !warnings.Any())
                return;

            this.warnings.AddRange(warnings);
        }

        public void AppendComments(IEnumerable<AssociativeNode> commentNodes)
        {
            if (commentNodes == null || !commentNodes.Any())
                return;

            this.commentNodes.AddRange(commentNodes);
        }

        #region Public Class Properties

        public System.Guid PostfixGuid { get; private set; }
        public System.String OriginalCode { get; private set; }
        public ElementResolver ElementResolver { get; private set; }

        public IDictionary<string, string> UnboundIdentifiers
        {
            get { return unboundIdentifiers; }
        }

        public IEnumerable<AssociativeNode> ParsedNodes
        {
            get { return this.parsedNodes; }
        }

        public IEnumerable<AssociativeNode> ParsedComments
        {
            get { return this.commentNodes; }
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

    public static class CompilerUtils
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
            blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            try
            {
                //defining the global Assoc block that wraps the entire .ds source file
                ProtoCore.LanguageCodeBlock globalBlock = new ProtoCore.LanguageCodeBlock();
                globalBlock.Language = ProtoCore.Language.Associative;
                globalBlock.Code = code;

                //passing the global Assoc wrapper block to the compiler
                ProtoCore.CompileTime.Context context = new ProtoCore.CompileTime.Context();
                ProtoCore.Language id = globalBlock.Language;

                core.Compilers[id].Compile(out blockId, null, globalBlock, context, codeBlockNode: codeBlock);

                core.BuildStatus.ReportBuildResult();

            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif

                if (!(ex is ProtoCore.BuildHaltException))
                {
                    throw ex;
                }
            }

            return core.BuildStatus;
        }

        internal static string ToLiteral(string input)
        {
            using (var writer = new StringWriter())
            {
                using (var provider = CodeDomProvider.CreateProvider("CSharp"))
                {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, new CodeGeneratorOptions { IndentString = "\t" });
                    var literString = writer.ToString();
                    literString = literString.Replace(string.Format("\" +{0}\t\"", Environment.NewLine), "");
                    return literString.Substring(1, literString.Length - 2);
                }
            }
        }

        public static bool TryLoadAssemblyIntoCore(Core core, string assemblyPath)
        {
            bool parsingPreloadFlag = core.IsParsingPreloadedAssembly;
            bool parsingCbnFlag = core.IsParsingCodeBlockNode;
            core.IsParsingPreloadedAssembly = true;
            core.IsParsingCodeBlockNode = false;

            int blockId;
            string importStatement = @"import (""" + ToLiteral(assemblyPath) + @""");";

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
        /// <param name="priorNames"></param>
        /// <param name="parseParams"> container for compilation related parameters </param>
        /// <param name="elementResolver"> classname resolver </param>
        /// <returns> true if code compilation succeeds, false otherwise </returns>
        public static bool PreCompileCodeBlock(Core core, ref ParseParam parseParams, IDictionary<string, string> priorNames = null)
        {
            string postfixGuid = parseParams.PostfixGuid.ToString().Replace("-", "_");

            // Parse code to generate AST and add temporaries to non-assignment nodes
            List<AssociativeNode> astNodes;
            List<AssociativeNode> comments;
            ParseUserCode(core, parseParams.OriginalCode, postfixGuid, out astNodes, out comments);
            parseParams.AppendErrors(core.BuildStatus.Errors);
            if (parseParams.Errors.Count() > 0)
            {
                return false;
            }

            // Catch the syntax errors and errors for unsupported 
            // language constructs thrown by compile expression
            parseParams.AppendWarnings(core.BuildStatus.Warnings);
            var warnings = Check(astNodes);
            parseParams.AppendWarnings(warnings);

            parseParams.AppendParsedNodes(astNodes.Where(n => !n.skipMe));
            parseParams.AppendComments(comments);

            // Compile the code to get the resultant unboundidentifiers  
            // and any errors or warnings that were caught by the compiler and cache them in parseParams
            return CompileCodeBlockAST(core, parseParams, priorNames);
        }

        private static bool CompileCodeBlockAST(Core core, ParseParam parseParams, IDictionary<string, string> priorNames)
        {
            var unboundIdentifiers = new Dictionary<int, List<VariableLine>>();

            ProtoCore.BuildStatus buildStatus = null;
            try
            {
                int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
                

                bool parsingPreloadFlag = core.IsParsingPreloadedAssembly;
                bool parsingCbnFlag = core.IsParsingPreloadedAssembly;
                core.IsParsingPreloadedAssembly = false;
                core.IsParsingCodeBlockNode = true;

                core.ResetForPrecompilation();

                var astNodes = parseParams.ParsedNodes;

                // Lookup namespace resolution map in elementResolver to rewrite 
                // partial classnames with their fully qualified names in ASTs
                // before passing them for pre-compilation. If partial class is not found in map, 
                // update Resolution map in elementResolver with fully resolved name from compiler.
                var reWrittenNodes = ElementRewriter.RewriteElementNames(core.ClassTable,  
                    parseParams.ElementResolver, astNodes, core.BuildStatus.LogSymbolConflictWarning);

                if (priorNames != null)
                {
                    // Use migration rewriter to migrate old method names to new names based on priorNameHints from LibraryServices
                    reWrittenNodes = MigrationRewriter.MigrateMethodNames(reWrittenNodes, priorNames, core.BuildStatus.LogDeprecatedMethodWarning);
                }

                // Clone a disposable copy of AST nodes for PreCompile() as Codegen mutates AST's
                // while performing SSA transforms and we want to keep the original AST's
                var codeblock = new CodeBlockNode();
                var nodes = reWrittenNodes.OfType<AssociativeNode>().Select(NodeUtils.Clone).ToList();
                codeblock.Body.AddRange(nodes);

                buildStatus = PreCompile(string.Empty, core, codeblock, out blockId);

                core.IsParsingCodeBlockNode = parsingCbnFlag;
                core.IsParsingPreloadedAssembly = parsingPreloadFlag;

                parseParams.AppendErrors(buildStatus.Errors);
                parseParams.AppendWarnings(buildStatus.Warnings);

                if (buildStatus.ErrorCount > 0)
                {
                    return false;
                }
                IEnumerable<BuildData.WarningEntry> warnings = buildStatus.Warnings;

                // Get the unboundIdentifiers from the warnings
                GetInputLines(parseParams.ParsedNodes, warnings, unboundIdentifiers);
                foreach (KeyValuePair<int, List<VariableLine>> kvp in unboundIdentifiers)
                {
                    foreach (VariableLine vl in kvp.Value)
                        parseParams.AppendUnboundIdentifier(vl.displayName, vl.variable);
                }

                return true;
            }
            catch (Exception)
            {
                buildStatus = null;
                return false;
            }
        }

        private static void ParseUserCode(ProtoCore.Core core, string expression, string postfixGuid, out List<AssociativeNode> astNodes, out List<AssociativeNode> commentNodes)
        {
            if (expression != null)
            {
                expression = expression.Replace("\r\n", "\n");

                try
                {
                    ParseUserCodeCore(core, expression, out astNodes, out commentNodes);
                    return;
                }
                catch
                {
                    // For class declarations, import statements etc. that are currently ignored
                }
            }

            astNodes = new List<AssociativeNode>();
            commentNodes = new List<AssociativeNode>();
        }

        private static void ParseUserCodeCore(Core core, string expression, out List<AssociativeNode> astNodes, out List<AssociativeNode> commentNodes)
        {
            astNodes = new List<AssociativeNode>();

            core.ResetForPrecompilation();
            core.IsParsingCodeBlockNode = true;
            core.ParsingMode = ParseMode.AllowNonAssignment;

            ParseResult parseResult = ParserUtils.ParseWithCore(expression, core);

            commentNodes = ParserUtils.GetAstNodes(parseResult.CommentBlockNode);
            var nodes = ParserUtils.GetAstNodes(parseResult.CodeBlockNode);
            Validity.Assert(nodes != null);

            int index = 0;
            int typedIdentIndex = 0;
            foreach (var node in nodes)
            {
                var n = node as AssociativeNode;
                Validity.Assert(n != null);

                // Append the temporaries only if it is not a function def or class decl
                bool isFunctionOrClassDef = n is FunctionDefinitionNode;

                if (n is ImportNode)
                {
                    core.BuildStatus.LogSemanticError(Resources.ImportStatementNotSupported);
                }
                else if (n is ClassDeclNode)
                {
                    core.BuildStatus.LogSemanticError(Resources.ClassDeclarationNotSupported);
                }
                else if (isFunctionOrClassDef)
                {
                    // Add node as it is
                    astNodes.Add(node);
                }
                else
                {
                    // Handle temporary naming for temporary Binary exp. nodes and non-assignment nodes
                    var ben = node as BinaryExpressionNode;
                    if (ben != null && ben.Optr == Operator.assign)
                    {
                        var lNode = ben.LeftNode as IdentifierNode;
                        if (lNode != null && lNode.Value == Constants.kTempProcLeftVar)
                        {
                            string name = Constants.kTempVarForNonAssignment + index;
                            var newNode = new BinaryExpressionNode(new IdentifierNode(name), ben.RightNode);
                            astNodes.Add(newNode);
                            index++;
                        }
                        else
                        {
                            // Add node as it is
                            astNodes.Add(node);
                            index++;
                        }
                    }
                    else
                    {
                        if (node is TypedIdentifierNode)
                        {
                            // e.g. a : int = %tTypedIdent_<Index>;
                            var ident = new IdentifierNode(Constants.kTempVarForTypedIdentifier + typedIdentIndex);
                            NodeUtils.CopyNodeLocation(ident, node);
                            var typedNode = new BinaryExpressionNode(node as TypedIdentifierNode, ident, Operator.assign);
                            NodeUtils.CopyNodeLocation(typedNode, node);
                            astNodes.Add(typedNode);
                            typedIdentIndex++;
                        }
                        else
                        {
                            string name = Constants.kTempVarForNonAssignment + index;
                            var newNode = new BinaryExpressionNode(new IdentifierNode(name), n);
                            astNodes.Add(newNode);
                            index++;
                        }
                        
                    }
                }
            }
        }

        private static void GetInputLines(IEnumerable<Node> astNodes,
                                   IEnumerable<BuildData.WarningEntry> warnings,
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
                var bNode = node as BinaryExpressionNode;
                if (bNode != null)
                {
                    var variableLineList = new List<VariableLine>();

                    for (int i = 0; i < warningVLList.Count; ++i)
                    {
                        var warning = warningVLList[i];
                        if (warning.line >= bNode.line && warning.line <= bNode.endLine)
                        {
                            if (warning.variable.StartsWith(Constants.kTempVarForTypedIdentifier))
                            {
                                warning.displayName = bNode.LeftNode.Name;
                            }
                            variableLineList.Add(warning);
                        }
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
                if (warningEntry.ID == ProtoCore.BuildData.WarningID.IdUnboundIdentifier)
                {
                    var varName = warningEntry.UnboundVariableSymbolNode.name;
                    result.Add(new VariableLine()
                    {
                        variable = varName,
                        line = warningEntry.Line,
                        displayName = varName
                    });
                }
            }
            return result;
        }

        internal class VariableFinder : AssociativeAstVisitor
        {
            private string variable;
            public bool Found { get; private set; }

            public VariableFinder(string variable)
            {
                this.variable = variable;
            }

            public override bool VisitIdentifierNode(IdentifierNode node)
            {
                if (node.Name == variable)
                {
                    Found = true;
                    return true;
                }

                if (node.ArrayDimensions == null)
                {
                    return false;
                }

                return node.ArrayDimensions.Accept(this);
            }

            public override bool VisitIdentifierListNode(IdentifierListNode node)
            {
                if (node == null)
                    return false;

                if (node.LeftNode is IdentifierNode || node.LeftNode is IdentifierListNode)
                {
                    if (node.RightNode is FunctionCallNode || node.RightNode is IdentifierNode)
                    {
                        node.LeftNode.Accept(this);

                        if (!(node.RightNode is IdentifierNode))
                        {
                            node.RightNode.Accept(this);
                        }

                        return true;
                    }
                }

                return base.VisitIdentifierListNode(node);
            }
        }

        /// <summary>
        /// Check does some sanity check, e.g., if a variable is re-defined.
        /// </summary>
        /// <param name="asts"></param>
        private static List<WarningEntry> Check(IEnumerable<AssociativeNode> asts)
        {
            var warnings = new List<WarningEntry>();

            HashSet<string> scope = new HashSet<string>();
            foreach (var node in asts)
            {
                BinaryExpressionNode bnode = node as BinaryExpressionNode;
                if (bnode == null || bnode.Optr != Operator.assign)
                {
                    var fNode = node as FunctionDefinitionNode;
                    if (fNode != null)
                    {
                        var fbody = NodeUtils.Clone(fNode.FunctionBody) as CodeBlockNode;
                        var body = fbody.Body;

                        warnings.AddRange(Check(body));

                        fNode.FunctionBody.Body.Clear();
                        fNode.FunctionBody.Body.AddRange(body.Where(n => !n.skipMe));
                    }
                    
                    continue;
                }

                IdentifierNode ident = bnode.LeftNode as IdentifierNode;
                if (ident == null)
                {
                    continue;
                }

                var variable = ident.Value;

                if (!scope.Contains(variable))
                {
                    scope.Add(variable);

                    VariableFinder finder = new VariableFinder(variable);

                    var langNode = bnode.RightNode as LanguageBlockNode;
                    if (langNode != null)
                    {
                        var cbn = langNode.CodeBlockNode as CodeBlockNode;
                        if (cbn != null)
                        {
                            var copy = NodeUtils.Clone(cbn) as CodeBlockNode;

                            warnings.AddRange(Check(copy.Body));

                            cbn.Body.Clear();
                            cbn.Body.AddRange(copy.Body.Where(n => !n.skipMe));
                        }
                        continue;
                    }
                    bnode.RightNode.Accept(finder);

                    if (finder.Found) 
                    {
                        warnings.Add(new WarningEntry
                        {
                            Message = String.Format(Resources.VariableRecursiveReference, variable),
                        });
                        node.skipMe = true;
                    }
                }
                else if (ident.ArrayDimensions == null)
                {
                    warnings.Add(new WarningEntry
                    {
                        Message = String.Format(Resources.VariableRedifinitionError, variable),
                    });
                    node.skipMe = true;
                }
            }

            return warnings;
        }
    }
}
