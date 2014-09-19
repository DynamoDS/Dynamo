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
using ProtoCore;

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

    public class GraphUtilities
    {
        // TODO Jun: is it better to have GraphUtils as a singleton rather than checking for core? 
        // We only need core to be instantiated once
        private static ProtoCore.Core core = null;
        public static ProtoCore.Core GetCore() { return core; }

        private static Dictionary<string, string> importedAssemblies = new Dictionary<string,string>();
        
        public static ProtoCore.BuildStatus BuildStatus { get { return core.BuildStatus; } }

        private static uint runningUID = Constants.UIDStart;
        public static uint GenerateUID() { return runningUID++; }

        private static void SetParsingFlagsForCore(ProtoCore.Core core, bool isCodeBlockNode = false, bool isPreloadedAssembly = false)
        {
            // Reuse the core for every succeeding run
            // TODO Jun: Check with UI - what instances need a new core and what needs reuse
            core.ResetForPrecompilation();
            core.IsParsingPreloadedAssembly = isPreloadedAssembly;
            core.IsParsingCodeBlockNode = isCodeBlockNode;
            core.ParsingMode = ProtoCore.ParseMode.AllowNonAssignment;
        }

        public static void Reset()
        {
            if (core != null)
            {
                core.Cleanup();
            }

            importedAssemblies.Clear();

            ProtoCore.Options options = new ProtoCore.Options();
            options.RootModulePathName = string.Empty;
            core = new ProtoCore.Core(options);
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
        }

        private static bool LocateAssembly(string assembly, out string assemblyPath)
        {
            ProtoCore.Options options = null;
            if (null == core)
                options = new ProtoCore.Options();
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

            SetParsingFlagsForCore(core, false, true); 

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
                    status = CompilerUtils.PreCompile(expression, core, null, out blockId);
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
                SetParsingFlagsForCore(core, true, false);
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
        /// This is called to Parse individual assignment statements in Codeblock nodes in GraphUI
        /// and return the resulting ProtoAST node
        /// </summary>
        /// <param name="statement"></param>
        public static ProtoCore.AST.Node Parse(string statement)
        {
            CodeBlockNode commentNode = null;
            SetParsingFlagsForCore(core, true, false); 
            return ProtoCore.Utils.ParserUtils.Parse(core, statement, out commentNode);
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
    }
}
