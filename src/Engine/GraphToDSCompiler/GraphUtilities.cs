using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using ProtoCore.DSASM.Mirror;
using System.Text.RegularExpressions;
using ProtoCore.Utils;
using System.IO;
using ProtoCore.DSASM;
using ProtoCore.AST.AssociativeAST;

namespace GraphToDSCompiler
{
    public struct kw
    {
        public const string tempPrefix = "temp";
    }

    public struct Constants
    {
        public const uint TempMask = 0x80000000;
        public const uint UIDStart = 10000;
        public const char ReplicationGuideDelimiter = '¡';

        public const string kwTempNull = "temp_NULL";
    }

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

        internal void AppendTemporaryVariable(string variable)
        {
            if (this.temporaries == null)
                this.temporaries = new List<string>();

            this.temporaries.Add(variable);
        }

        internal void AppendUnboundIdentifier(string identifier)
        {
            if (this.unboundIdentifiers == null)
                this.unboundIdentifiers = new List<string>();

            if (!this.unboundIdentifiers.Contains(identifier))
                this.unboundIdentifiers.Add(identifier);
        }

        internal void AppendParsedNodes(IEnumerable<ProtoCore.AST.Node> parsedNodes)
        {
            if (this.parsedNodes == null)
                this.parsedNodes = new List<ProtoCore.AST.Node>();

            this.parsedNodes.AddRange(parsedNodes);
        }

        internal void AppendErrors(IEnumerable<ProtoCore.BuildData.ErrorEntry> errors)
        {
            if (errors == null || (errors.Count() <= 0))
                return;

            if (this.errors == null)
                this.errors = new List<ProtoCore.BuildData.ErrorEntry>();

            this.errors.AddRange(errors);
        }

        internal void AppendWarnings(IEnumerable<ProtoCore.BuildData.WarningEntry> warnings)
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

    public class GraphUtilities
    {
        // TODO Jun: is it better to have GraphUtils as a singleton rather than checking for core? 
        // We only need core to be instantiated once
        private static ProtoCore.Core core = null;
        private static string rootModulePath = string.Empty;

        public static ProtoCore.Core GetCore() { return core; }

        private static int numBuiltInMethods = 0;

        private static List<ProcedureNode> builtInMethods = null;

        private static Dictionary<string, string> importedAssemblies = new Dictionary<string,string>();

        public static IEnumerable<ClassNode> GetImportedClasses()
        {
            foreach (ClassNode classNode in GraphUtilities.GetCore().ClassTable.ClassNodes)
            {
                if (classNode.IsImportedClass && !string.IsNullOrEmpty(classNode.ExternLib))
                {
                    yield return classNode;
                }
            }
        }

        public static ProtoCore.BuildStatus BuildStatus { get { return core.BuildStatus; } }

        private static uint runningUID = Constants.UIDStart;
        public static uint GenerateUID() { return runningUID++; }

        private static bool resetCore = true;

        private static void BuildCore(bool isCodeBlockNode = false, bool isPreloadedAssembly = false)
        {
            // Reuse the core for every succeeding run
            // TODO Jun: Check with UI - what instances need a new core and what needs reuse
            if (core == null || resetCore)
            {
                resetCore = false;
                ProtoCore.Options options = new ProtoCore.Options();
                options.RootModulePathName = rootModulePath;
                core = new ProtoCore.Core(options);
                core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
                core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));

            }
            else
            {
                //core.ResetDeltaExecution();
                core.ResetForPrecompilation();

            }
            core.IsParsingPreloadedAssembly = isPreloadedAssembly;
            core.IsParsingCodeBlockNode = isCodeBlockNode;
            core.ParsingMode = ProtoCore.ParseMode.AllowNonAssignment;
        }

        public static void Reset()
        {
            resetCore = true;
            if (core != null)
            {
                core.Cleanup();
            }
            core = null;

            numBuiltInMethods = 0;
            builtInMethods = null;
            importedAssemblies.Clear();
            rootModulePath = string.Empty;
        }

        private static bool LocateAssembly(string assembly, out string assemblyPath)
        {
            ProtoCore.Options options = null;
            if (null == core)
                options = new ProtoCore.Options() { RootModulePathName = GraphUtilities.rootModulePath };
            else
                options = core.Options;

            assemblyPath = FileUtils.GetDSFullPathName(assembly, options);
            string dirName = Path.GetDirectoryName(assemblyPath);
            if(!string.IsNullOrEmpty(dirName) && !core.Options.IncludeDirectories.Contains(dirName))
                core.Options.IncludeDirectories.Add(dirName);

            return File.Exists(assemblyPath);
        }

        /// <summary>
        /// TODO: Deprecate
        /// This is for Graph UI to preload assemblies as and when they are loaded from Library viewer
        /// to pre-populate class tables with imported classes and methods
        /// </summary>
        /// <param name="assemblies"></param>
        public static void PreloadAssembly(List<string> assemblies)
        {
            string expression = string.Empty;
            int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;

            BuildCore(false, true);

            foreach (string assembly in assemblies)
            {
                string assemblyPath = string.Empty;
                if (!LocateAssembly(assembly, out assemblyPath))
                {
                    BuildStatus.LogWarning(ProtoCore.BuildData.WarningID.kFileNotFound, string.Format(ProtoCore.BuildData.WarningMessage.kFileNotFound, assembly));
                    continue;
                }
                
                if(importedAssemblies.ContainsValue(assemblyPath))
                    continue;

                string key = MD5Hash(assembly);
                importedAssemblies[MD5Hash(assembly)] = assemblyPath;

                string importStatement = "import (" + '"'.ToString() + assemblyPath + '"'.ToString() + ")";
                expression = expression + importStatement + ';'.ToString();

                ProtoCore.BuildStatus status = null;
                if (!string.IsNullOrEmpty(expression))
                {
                    status = PreCompile(expression, core, out blockId);
                }

                if (status != null && status.ErrorCount > 0)
                {
                    importedAssemblies.Remove(key);
                }

            }

        }


        /// <summary>
        /// This method iterates over each imported assembly in the importedAssemblies
        /// dictionary and performs the given action with the hash and path of the 
        /// imported assembly.
        /// </summary>
        /// <param name="action">Action to perform with first argument as hash and the 
        /// second as path</param>
        public static void ForEachImportNodes(Action<string, string> action)
        {
            foreach (var item in importedAssemblies)
            {
                action(item.Key, item.Value);
            }
        }

        public static bool TryGetImportLibraryPath(string library, out string path, out string hashKey)
        {
            hashKey = GraphUtilities.MD5Hash(library);
            if (!importedAssemblies.TryGetValue(hashKey, out path))
            {
                if (!LocateAssembly(library, out path))
                    return false;
                importedAssemblies[hashKey] = path;
            }

            return true;
        }

        /// <summary>
        /// Given an assembly or DS file, it precompiles it and returns a list of the newly imported classes
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IList<ClassNode> GetClassesForAssembly(string assembly)
        {
            int currentCI = core.ClassTable.ClassNodes.Count;

            List<string> assemblies = new List<string>();
            assemblies.Add(assembly);
            PreloadAssembly(assemblies);

            int postCI = core.ClassTable.ClassNodes.Count;

            IList<ClassNode> classNodes = new List<ClassNode>();
            for (int i = currentCI; i < postCI; ++i)
            {
                classNodes.Add(core.ClassTable.ClassNodes[i]);
            }

            return classNodes;
        }

        public static List<ProcedureNode> GetBuiltInMethods()
        {
            Validity.Assert(core != null);

            Validity.Assert(core.CodeBlockList.Count > 0);

            if (builtInMethods == null)
            {
                List<ProcedureNode> procNodes = core.CodeBlockList[0].procedureTable.procList;
                numBuiltInMethods = procNodes.Count;
                List<ProcedureNode> builtIns = new List<ProcedureNode>();

                foreach (ProcedureNode procNode in procNodes)
                {
                    if (!procNode.name.StartsWith(ProtoCore.DSASM.Constants.kInternalNamePrefix) && !procNode.name.Equals("Break"))
                        builtIns.Add(procNode);
                }
                builtInMethods = builtIns;

            }
            Validity.Assert(builtInMethods.Count > 0);

            return builtInMethods;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static List<ProcedureNode> GetGlobalMethods()
        {
            List<ProcedureNode> methods = new List<ProcedureNode>();
            Validity.Assert(core != null);

            Validity.Assert(core.CodeBlockList.Count > 0);

            List<ProcedureNode> procNodes = core.CodeBlockList[0].procedureTable.procList;

            int numNewMethods = procNodes.Count - numBuiltInMethods;
            Validity.Assert(numNewMethods >= 0);

            for (int i = numBuiltInMethods; i < procNodes.Count; ++i)
            {
                methods.Add(procNodes[i]);
            }

            numBuiltInMethods = procNodes.Count;
            return methods;
        }

        private static IEnumerable<ProtoCore.AST.Node> ParseUserCodeCore(string expression, string postfixGuid, ref bool parseSuccess)
        {
            List<ProtoCore.AST.Node> astNodes = new List<ProtoCore.AST.Node>();

            ProtoCore.AST.AssociativeAST.CodeBlockNode commentNode = null;
            ProtoCore.AST.Node codeBlockNode = Parse(expression, out commentNode);
            parseSuccess = true;
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
                    core.BuildStatus.LogSemanticError("Modifier Blocks are not supported currently.");
                }
                else if (n is ProtoCore.AST.AssociativeAST.ImportNode)
                {
                    core.BuildStatus.LogSemanticError("Import statements are not supported in CodeBlock Nodes.");
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
                            core.BuildStatus.LogSemanticError("Modifier Blocks are not supported currently.");
                        }
                        IdentifierNode lNode = ben.LeftNode as IdentifierNode;
                        if (lNode != null && lNode.Value == ProtoCore.DSASM.Constants.kTempProcLeftVar)
                        {
                            string name = string.Format("temp_{0}_{1}", index++, postfixGuid);
                            BinaryExpressionNode newNode = new BinaryExpressionNode(new IdentifierNode(name), ben.RightNode);
                            astNodes.Add(newNode);
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
                        string name = string.Format("temp_{0}_{1}", index++, postfixGuid);
                        BinaryExpressionNode newNode = new BinaryExpressionNode(new IdentifierNode(name), n);
                        astNodes.Add(newNode);
                    }
                }
            }            
            return astNodes;
        }

        private static IEnumerable<ProtoCore.AST.Node> ParseUserCode(string expression, string postfixGuid)
        {
            IEnumerable<ProtoCore.AST.Node> astNodes = new List<ProtoCore.AST.Node>();

            if (expression == null)
                return astNodes;

            expression = expression.Replace("\r\n", "\n");
            
            bool parseSuccess = false;
            try
            {
                return ParseUserCodeCore(expression, postfixGuid, ref parseSuccess);
            }
            catch
            {
                // For modifier blocks, language blocks, etc. that are currently ignored
                if (parseSuccess)
                    return astNodes;

                // Reset core above as we don't wish to propagate these errors - pratapa
                core.ResetForPrecompilation();

                // Use manual parsing for invalid functional associative statement errors like for "a+b;"
                return ParseNonAssignments(expression, postfixGuid);
            }
        }

        private static IEnumerable<ProtoCore.AST.Node> ParseNonAssignments(string expression, string postfixGuid)
        {
            List<string> compiled = new List<string>();

            string[] expr = GetStatementsString(expression);
            foreach (string s in expr)
                compiled.Add(s);

            for (int i = 0; i < compiled.Count(); i++)
            {
                if (compiled[i].StartsWith("\n"))
                {
                    string newlines = string.Empty;
                    int lastPosButOne = 0;
                    string original = compiled[i];
                    for (int j = 0; j < original.Length; j++)
                    {
                        if (!original[j].Equals('\n')) 
                        { 
                            lastPosButOne = j; 
                            break; 
                        } 
                        else 
                            newlines += original[j];
                    }
                    string newStatement = original.Substring(lastPosButOne);

                    if (!IsNotAssigned(newStatement))
                    {
                        string name = string.Format("temp_{0}_{1}", i, postfixGuid);
                        newStatement = name + " = " + newStatement;
                    }
                    compiled[i] = newlines + newStatement;
                }
                else
                {
                    if (!IsNotAssigned(compiled[i]))
                    {
                        string name = string.Format("temp_{0}_{1}", i, postfixGuid);
                        compiled[i] = name + " = " + compiled[i];
                    }
                }
            }
            StringBuilder newCode = new StringBuilder();
            compiled.ForEach(x => newCode.Append(x));
            CodeBlockNode commentNode = null;
           
            try
            {
                ProtoCore.AST.Node codeBlockNode = Parse(newCode.ToString(), out commentNode);
                return ParserUtils.GetAstNodes(codeBlockNode);
            }
            catch (Exception)
            {
                return new List<ProtoCore.AST.Node>();
            }
        }

        private static bool IsNotAssigned(string code)
        {
            code = code.Trim(';', ' ');
            if (string.IsNullOrEmpty(code)) 
                return true;
            bool hasLHS = code.Contains("=");
            return hasLHS;
        }

        /*Given a block of code that has only usual statements and Modifier Stacks*/
        private static string[] GetStatementsString(string input)
        {
            var expr = new List<string>();
            
            expr.AddRange(GetBinaryStatementsList(input));
            return expr.ToArray();
        }
        /*attempt*/
        /*Given a block of code that has only usual binary statements*/
        private static List<string> GetBinaryStatementsList(string input)
        {
            var expr = new List<string>();
            int index = 0;
            int oldIndex = 0;
            do
            {
                index = input.IndexOf(";", oldIndex);
                if (index != -1)
                {
                    string sub;
                    if (index < input.Length - 1)
                    {
                        if (input[index + 1].Equals('\n'))
                            index += 1;
                    }
                    sub = input.Substring(oldIndex, index - oldIndex + 1);
                    expr.Add(sub);
                    //index++;
                    oldIndex = index + 1;
                }
            } while (index != -1);
            return expr;
        }


        /// <summary>
        /// Checks if the string in code block node is a literal or an identifier or has multiple lines of code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static SnapshotNodeType AnalyzeString(string code)
        {
            SnapshotNodeType type = SnapshotNodeType.None;
            if (!code.EndsWith(";")) code += ";";
            List<ProtoCore.AST.Node> n = new List<ProtoCore.AST.Node>();
            try
            {
                BuildCore(true);
                System.IO.MemoryStream memstream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(code));
                ProtoCore.DesignScriptParser.Scanner s = new ProtoCore.DesignScriptParser.Scanner(memstream);
                ProtoCore.DesignScriptParser.Parser p = new ProtoCore.DesignScriptParser.Parser(s, core);

                p.Parse();
                ProtoCore.AST.AssociativeAST.CodeBlockNode codeBlockNode = p.root as ProtoCore.AST.AssociativeAST.CodeBlockNode;
                n = p.GetParsedASTList(codeBlockNode);
            }
            catch (Exception)
            {
                //if syntax return SnapshotNodeType as None
                return SnapshotNodeType.CodeBlock;
            }

            if (n.Count > 1 || n.Count == 0)
                type = SnapshotNodeType.CodeBlock;
            else if (n[0] is ProtoCore.AST.AssociativeAST.BinaryExpressionNode)
                type = SnapshotNodeType.CodeBlock;
            else if (n[0] is ProtoCore.AST.AssociativeAST.IdentifierNode)
                type = SnapshotNodeType.Identifier;
            else if (n[0] is ProtoCore.AST.AssociativeAST.IntNode || n[0] is ProtoCore.AST.AssociativeAST.DoubleNode || n[0] is ProtoCore.AST.AssociativeAST.StringNode || n[0] is ProtoCore.AST.AssociativeAST.FunctionCallNode || n[0] is ProtoCore.AST.AssociativeAST.RangeExprNode)
                type = SnapshotNodeType.Literal;
            else type = SnapshotNodeType.CodeBlock;
            return type;
        }

        /// <summary>
        /// Does the first pass of compilation and returns a list of wanrnings in compilation
        /// </summary>
        /// <param name="code"></param>
        /// <param name="core"></param>
        /// <param name="blockId"></param>
        /// <returns></returns>
        private static ProtoCore.BuildStatus PreCompile(string code, ProtoCore.Core core, out int blockId, CodeBlockNode codeBlock = null)
        {
            bool buildSucceeded = false;

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

                buildSucceeded = core.BuildStatus.BuildSucceeded;
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

        private static void GetInputLines(IEnumerable<ProtoCore.AST.Node> astNodes, IEnumerable<ProtoCore.BuildData.WarningEntry> warnings,
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

        /// <summary>
        /// This is called to Parse individual assignment statements in Codeblock nodes in GraphUI
        /// and return the resulting ProtoAST node
        /// </summary>
        /// <param name="statement"></param>
        public static ProtoCore.AST.Node Parse(string statement, out ProtoCore.AST.AssociativeAST.CodeBlockNode commentNode)
        {
            commentNode = null;
            BuildCore(true);
            return ProtoCore.Utils.ParserUtils.Parse(core, statement, out commentNode);
        }

        /// <summary>
        /// Pre-compiles DS code in code block node, 
        /// checks for syntax, converts non-assignments to assignments,
        /// stores list of AST nodes, errors and warnings
        /// Evaluates and stores list of unbound identifiers
        /// </summary>
        /// <param name="parseParams"></param>
        /// <returns></returns>
        public static bool PreCompileCodeBlock(ParseParam parseParams)
        {
            string postfixGuid = parseParams.PostfixGuid.ToString().Replace("-", "_");

            // Parse code to generate AST and add temporaries to non-assignment nodes
            IEnumerable<ProtoCore.AST.Node> astNodes = ParseUserCode(parseParams.OriginalCode, postfixGuid);
            
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
            return CompileCodeBlockAST(parseParams);
        }

        public static List<ProtoCore.AST.Node> ParseCodeBlock(string code)
        {
            Validity.Assert(code != null);

            if (string.IsNullOrEmpty(code))
                return null;

            // TODO: Change the logic to ignore Import statements in this case using parser - pratapa
            // Check if this will work with modifier blocks as well
            /*string[] stmts = code.Split(';');
            string source = "";
            for (int i=0; i < stmts.Length; ++i)
            {
                if (!stmts[i].Contains("import"))
                    source += stmts[i] + ";";
            }*/


            ProtoCore.Options options = new ProtoCore.Options();
            ProtoCore.Core core = new ProtoCore.Core(options);
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));

            System.IO.MemoryStream memstream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(code));
            ProtoCore.DesignScriptParser.Scanner s = new ProtoCore.DesignScriptParser.Scanner(memstream);
            ProtoCore.DesignScriptParser.Parser p = new ProtoCore.DesignScriptParser.Parser(s, core);

            p.Parse();
            ProtoCore.AST.AssociativeAST.CodeBlockNode cbn = p.root as ProtoCore.AST.AssociativeAST.CodeBlockNode;

            Validity.Assert(cbn != null);
            return p.GetParsedASTList(cbn);
        }

        private static bool CompileCodeBlockAST(ParseParam parseParams)
        {
            Dictionary<int, List<VariableLine>> unboundIdentifiers = new Dictionary<int, List<VariableLine>>();
            IEnumerable<ProtoCore.BuildData.WarningEntry> warnings = null;

            ProtoCore.BuildStatus buildStatus = null;
            try
            {
                BuildCore(true);
                int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
                CodeBlockNode codeblock = new ProtoCore.AST.AssociativeAST.CodeBlockNode();
                List<AssociativeNode> nodes = new List<AssociativeNode>();
                foreach (var i in parseParams.ParsedNodes)
                {
                    AssociativeNode assocNode = i as AssociativeNode;

                    if (assocNode != null)
                        nodes.Add(NodeUtils.Clone(assocNode));  
                }
                codeblock.Body.AddRange(nodes);

                buildStatus = PreCompile(string.Empty, core, out blockId, codeblock);

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
      
        /// <summary>
        /// Temporary implementation for importing external libraries
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string MD5Hash(string text)
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            //compute hash from the bytes of text
            md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(text));

            //get hash result after compute it
            byte[] result = md5.Hash;

            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                //change it into 2 hexadecimal digits
                //for each byte
                strBuilder.Append(result[i].ToString("x2"));
            }
            return strBuilder.ToString();
        }

        /// <summary>
        /// This converts a list of ProtoASTs into ds using CodeGenDS
        /// </summary>
        /// <param name="astList"></param>
        /// <returns></returns>
        public static string ASTListToCode(List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList)
        {
            string code = string.Empty;
            if (null != astList && astList.Count > 0)
            {
                ProtoCore.CodeGenDS codeGen = new ProtoCore.CodeGenDS(astList);
                code = codeGen.GenerateCode();
            }
            return code;
        }
    }
}
