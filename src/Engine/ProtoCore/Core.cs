using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProtoCore.AssociativeGraph;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.Lang;
using ProtoCore.Utils;
using ProtoFFI;

namespace ProtoCore
{
    public enum ExecutionMode
    {
        Parallel,
        Serial
    }

    /// <summary>
    /// Represents a single replication guide entity that is associated with an argument to a function
    /// 
    /// Given:
    ///     a = f(i<1>, j<2L>)
    ///     
    ///     <1> and <2L> are each represented by a ReplicationGuide instance
    ///     
    /// </summary>
    public class ReplicationGuide
    {
        public ReplicationGuide(int guide, bool longest)
        {
            GuideNumber = guide;
            IsLongest = longest;
        }

        public int GuideNumber { get; private set; }
        public bool IsLongest {get; private set;}
    }

    public class AtLevel
    {
        public AtLevel(int level, bool isDominant)
        {
            Level = level;
            IsDominant = isDominant;
        }

        public int Level { get; private set; }
        public bool IsDominant { get; private set; }
    }

    public class Options
    {
        public Options()
        {
            // Execute using new graphnode dependency
            // When executing direct dependency set the following:
            //      DirectDependencyExecution = true
            //      LHSGraphNodeUpdate = false
            DirectDependencyExecution = true;

            ApplyUpdate = false;
            DumpByteCode = false;
            Verbose = false;
            DumpIL = false;

            GenerateSSA = true;
            ExecuteSSA = true;
            GCTempVarsOnDebug = true;

            DumpFunctionResolverLogic = false; 
            BuildOptErrorAsWarning = false;
            ExecutionMode = ExecutionMode.Serial;
            IDEDebugMode = false;
            WatchTestMode = false;
            IncludeDirectories = new List<string>();
            RootModulePathName = Path.GetFullPath(@".");
            staticCycleCheck = true;
            dynamicCycleCheck = true;
            EmitBreakpoints = true;
            AssociativeToImperativePropagation = true;
            SuppressFunctionResolutionWarning = true;
            IsDeltaExecution = false;
            IsDeltaCompile = false;
        }

        public bool DirectDependencyExecution { get; set; }
        public bool ApplyUpdate { get; set; }
        public bool DumpByteCode { get; set; }
        public bool DumpIL { get; private set; }
        public bool GenerateSSA { get; set; }
        public bool ExecuteSSA { get; set; }
        public bool GCTempVarsOnDebug { get; set; }
        public bool Verbose { get; set; }
        public bool SuppressBuildOutput { get; set; }
        public bool BuildOptErrorAsWarning { get; set; }
        public bool IDEDebugMode { get; set; }      //set to true if two way mapping b/w DesignScript and JIL code is needed
        public bool WatchTestMode { get; set; }     // set to true when running automation tests for expression interpreter
        public ExecutionMode ExecutionMode { get; set; }
        public string FormatToPrintFloatingPoints { get; set; }
        public bool staticCycleCheck { get; set; }
        public bool dynamicCycleCheck { get; set; }
        public bool DumpFunctionResolverLogic { get; set; }
        public bool EmitBreakpoints { get; set; }
        public bool SuppressFunctionResolutionWarning { get; set; }
        public bool AssociativeToImperativePropagation { get; set; }
        public bool IsDeltaExecution { get; set; }
        public InterpreterMode RunMode { get; set; }

        /// <summary>
        /// TODO: Aparajit: This flag is true for Delta AST compilation
        /// This will be removed once we make this the default and deprecate "deltaCompileStartPC" 
        /// which requires recompiling the entire source code for every delta execution 
        /// </summary>
        public bool IsDeltaCompile { get; set; }
        
        // This is being moved to Core.Options as this needs to be overridden for the Watch test framework runner        
        public int kDynamicCycleThreshold = 2000;

        public List<string> IncludeDirectories { get; set; }
        public string RootModulePathName { get; set; }
    }

    public struct InlineConditional
    {
        public bool isInlineConditional;
        public int endPc;
        public int startPc;
        public int instructionStream;
        public List<Instruction> ActiveBreakPoints;
    }

   
    public enum ParseMode
    {
        Normal,
        AllowNonAssignment,
        None
    }

    public class Core
    {
        public Dictionary<Guid, int> CallsiteGuidMap { get; set; }
        public IDictionary<string, object> Configurations { get; set; }
        public List<System.Type> DllTypesToLoad { get; private set; }

        public void AddDLLExtensionAppType(System.Type type)
        {
            Validity.Assert(DllTypesToLoad != null);
            DllTypesToLoad.Add(type);
        }

        /// <summary>
        /// Properties in under COMPILER_GENERATED_TO_RUNTIME_DATA, are generated at compile time, and passed to RuntimeData/Exe
        /// Only Core can initialize these
        /// </summary>
#region COMPILER_GENERATED_TO_RUNTIME_DATA

        public FunctionTable FunctionTable { get; private set; }

#endregion

        // This flag is set true when we call GraphUtilities.PreloadAssembly to load libraries in Graph UI
        public bool IsParsingPreloadedAssembly { get; set; }
        
        // THe ImportModuleHandler owned by the temporary core used in Graph UI precompilation
        // needed to detect if the same assembly is not being imported more than once
        public ImportModuleHandler ImportHandler { get; set; }
        
        // This is set to true when the temporary core is used for precompilation of CBN's in GraphUI
        public bool IsParsingCodeBlockNode { get; set; }

        // This is the AST node list of default imported libraries needed for Graph Compiler
        public CodeBlockNode ImportNodes { get; set; }

        public enum ErrorType
        {
            OK,
            Error,
            Warning
        }

        public struct ErrorEntry
        {
            public ErrorType Type;
            public string FileName;
            public string Message;
            public BuildData.WarningID BuildId;
            public Runtime.WarningID RuntimeId;
            public int Line;
            public int Col;
        }

        public Dictionary<ulong, ulong> CodeToLocation = new Dictionary<ulong, ulong>();
        public Dictionary<ulong, ErrorEntry> LocationErrorMap = new Dictionary<ulong, ErrorEntry>();


        public Dictionary<Language, Compiler> Compilers { get; private set; }

        public int GlobOffset { get; set; }
        public int GlobHeapOffset { get; set; }
        public int BaseOffset { get; set; }
        public int GraphNodeUID { get; set; }

        public Heap Heap { get; private set; }
        //public RuntimeMemory Rmem { get; set; }

        public int ClassIndex { get; set; }     // Holds the current class scope
        public int CodeBlockIndex { get; set; }
        public int RuntimeTableIndex { get; set; }


        public List<CodeBlock> CodeBlockList { get; set; }
        // The Complete Code Block list contains all the code blocks
        // unlike the codeblocklist which only stores the outer most code blocks
        public List<CodeBlock> CompleteCodeBlockList { get; set; }

        /// <summary>
        /// ForLoopBlockIndex tracks the current number of new for loop blocks created at compile time for every new compile phase
        /// It is reset for delta compilation
        /// </summary>
        public int ForLoopBlockIndex { get; set; }

        public Executable DSExecutable { get; set; }

        public Options Options { get; private set; }
        public BuildStatus BuildStatus { get; private set; }

        public TypeSystem TypeSystem { get; set; }
        public InternalAttributes internalAttributes { get; set; }

        // The global class table and function tables
        public ClassTable ClassTable { get; set; }
        public ProcedureTable ProcTable { get; set; }
        public ProcedureNode ProcNode { get; set; }

        // The function pointer table
        public FunctionPointerTable FunctionPointerTable { get; set; }

        //The dynamic string table and function table
        public DynamicVariableTable DynamicVariableTable { get; set; }
        public DynamicFunctionTable DynamicFunctionTable { get; set; }


        //Manages injected context data.
        internal ContextDataManager ContextDataManager { get; set; }

        public ParseMode ParsingMode { get; set; }
        public bool ParseDeprecatedListSyntax = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void AddContextData(Dictionary<string, Object> data)
        {
            if (data == null)
                return;

            ContextDataManager.GetInstance(this).AddData(data);
        }

        // if CompileToLib is true, this is used to output the asm instruction to the dsASM file
        // if CompilerToLib is false, this will be set to Console.Out
        public TextWriter AsmOutput;
        public int AsmOutputIdents;

        public string CurrentDSFileName { get; set; }
        // this field is used to store the inferedtype information  when the code gen cross one langeage to another 
        // otherwize the inferedtype information will be lost
        public Type InferedType;


        /// <summary>
        /// Debugger properties generated at compile time.
        /// This is copied to the RuntimeCore after compilation
        /// </summary>
        public DebugProperties DebuggerProperties;


        public bool builtInsLoaded { get; set; }
        public List<string> LoadedDLLs = new List<string>();
        public int deltaCompileStartPC { get; set; }


        // A list of graphnodes that contain a function call
        public List<GraphNode> GraphNodeCallList { get; set; }

        // A stack of graphnodes that are generated on the body of an inline conditional
        public Stack<List<GraphNode>> InlineConditionalBodyGraphNodes { get; set; }

        public int newEntryPoint { get; private set; }

        public void SetNewEntryPoint(int pc)
        {
            newEntryPoint = pc;
        }

        /// <summary>
        /// Sets the function to an inactive state where it can no longer be used by the front-end and backend
        /// </summary>
        /// <param name="functionDef"></param>
        public void SetFunctionInactive(FunctionDefinitionNode functionDef)
        {
            // DS language only supports function definition on the global and first language block scope 
            // TODO Jun: Determine if it is still required to combine function tables in the codeblocks and callsite

            // Update the functiond definition in the codeblocks
            int hash = CoreUtils.GetFunctionHash(functionDef);

            ProcedureNode procNode = null;

            foreach (CodeBlock block in CodeBlockList)
            {
                // Update the current function definition in the current block
                procNode = block.procedureTable.Procedures.FirstOrDefault(p => p.HashID == hash);
                int index = procNode == null ? Constants.kInvalidIndex: procNode.ID; 
                if (Constants.kInvalidIndex == index)
                    continue;

                procNode.IsActive = false;

                // Remove staled graph nodes
                var graph = block.instrStream.dependencyGraph;
                graph.GraphList.RemoveAll(g => g.classIndex == ClassIndex && 
                                               g.procIndex == index);
                graph.RemoveNodesFromScope(Constants.kGlobalScope, index);

                // Make a copy of all symbols defined in this function
                var localSymbols = block.symbolTable.symbolList.Values
                                        .Where(n => 
                                                n.classScope == Constants.kGlobalScope 
                                             && n.functionIndex == index)
                                        .ToList();

                foreach (var symbol in localSymbols)
                {
                    block.symbolTable.UndefineSymbol(symbol);
                }

                break;
            }

            if (null != procNode)
            {
                // Remove codeblock defined in procNode from CodeBlockList and CompleteCodeBlockList
                foreach (int cbID in procNode.ChildCodeBlocks)
                {
                    CompleteCodeBlockList.RemoveAll(x => x.codeBlockId == cbID);

                    foreach (CodeBlock cb in CodeBlockList)
                    {
                        cb.children.RemoveAll(x => x.codeBlockId == cbID);
                    }
                }
            }


            // Update the function definition in global function tables
            foreach (KeyValuePair<int, Dictionary<string, FunctionGroup>> functionGroupList in DSExecutable.FunctionTable.GlobalFuncTable)
            {
                foreach (KeyValuePair<string, FunctionGroup> functionGroup in functionGroupList.Value)
                {
                    functionGroup.Value.FunctionEndPoints.RemoveAll(func => func.procedureNode.HashID == hash);
                }
            }
        }

        /// <summary>
        /// Reset the VM state for delta execution.
        /// </summary>
        public void ResetForDeltaExecution()
        {
            Options.ApplyUpdate = false;

            Options.RunMode = InterpreterMode.Normal;

            // The main codeblock never goes out of scope
            // Resetting CodeBlockIndex means getting the number of main codeblocks that dont go out of scope.
            // As of the current requirements, there is only 1 main scope, the rest are nested within.
            CodeBlockIndex = CodeBlockList.Count;
            RuntimeTableIndex = CodeBlockIndex;

            ForLoopBlockIndex = Constants.kInvalidIndex;

            // Jun this is where the temp solutions starts for implementing language blocks in delta execution
            for (int n = 1; n < CodeBlockList.Count; ++n)
            {
                CodeBlockList[n].instrStream.instrList.Clear();
            }

            // Remove inactive graphnodes in the list
            GraphNodeCallList.RemoveAll(g => !g.isActive);
            ExprInterpreterExe = null;
        }

        public void ResetForPrecompilation()
        {
            GraphNodeUID = 0;
            CodeBlockIndex = 0;
            RuntimeTableIndex = 0;
            
            //Initialize the dynamic string table and dynamic function table
            DynamicVariableTable = new DynamicVariableTable();
            DynamicFunctionTable = new DynamicFunctionTable();

            BuildStatus = new BuildStatus(this);
            
            ExpressionUID = 0;
            ForLoopBlockIndex = Constants.kInvalidIndex;
        }

        private void ResetAll(Options options)
        {
            Heap = new Heap();
            //Rmem = new RuntimeMemory(Heap);
            Configurations = new Dictionary<string, object>();
            DllTypesToLoad = new List<System.Type>();

            Options = options;
            
            Compilers = new Dictionary<Language, Compiler>();
            ClassIndex = Constants.kInvalidIndex;

            FunctionTable = new FunctionTable(); 


            watchFunctionScope = Constants.kInvalidIndex;
            watchSymbolList = new List<SymbolNode>();
            watchBaseOffset = 0;


            GlobOffset = 0;
            GlobHeapOffset = 0;
            BaseOffset = 0;
            GraphNodeUID = 0;
            CodeBlockIndex = 0;
            RuntimeTableIndex = 0;
            CodeBlockList = new List<CodeBlock>();
            CompleteCodeBlockList = new List<CodeBlock>();
            CallsiteGuidMap = new Dictionary<Guid, int>();

            AssocNode = null;

            // TODO Jun/Luke type system refactoring
            // Initialize the globalClass table and type system
            ClassTable = new ClassTable();
            TypeSystem = new TypeSystem();
            TypeSystem.SetClassTable(ClassTable);
            ProcNode = null;
            ProcTable = new ProcedureTable(Constants.kGlobalScope);

            // Initialize internal attributes
            internalAttributes = new InternalAttributes(ClassTable);

            //Initialize the function pointer table
            FunctionPointerTable = new FunctionPointerTable();

            //Initialize the dynamic string table and dynamic function table
            DynamicVariableTable = new DynamicVariableTable();
            DynamicFunctionTable = new DynamicFunctionTable();

            watchStartPC = Constants.kInvalidIndex;

            deltaCompileStartPC = Constants.kInvalidIndex;

            BuildStatus = new BuildStatus(this, null, Options.BuildOptErrorAsWarning);

            SSAExpressionUID = 0;
            SSASubscript = 0;
            SSASubscript_GUID = Guid.NewGuid();
            SSAExprUID = 0;
            ExpressionUID = 0;

            ExprInterpreterExe = null;
            Options.RunMode = InterpreterMode.Normal;

            assocCodegen = null;

            // Default execution log is Console.Out.
            ExecutionLog = Console.Out;

            DebuggerProperties = new DebugProperties();


            ParsingMode = ParseMode.Normal;
            
            IsParsingPreloadedAssembly = false;
            IsParsingCodeBlockNode = false;
            ImportHandler = null;

            deltaCompileStartPC = 0;
            builtInsLoaded = false;


            ForLoopBlockIndex = Constants.kInvalidIndex;

            GraphNodeCallList = new List<GraphNode>();
            InlineConditionalBodyGraphNodes = new Stack<List<GraphNode>>();

            newEntryPoint = Constants.kInvalidIndex;
        }

        // The unique subscript for SSA temporaries
        // TODO Jun: Organize these variables in core into proper enums/classes/struct
        public int SSASubscript { get; set; }
        public Guid SSASubscript_GUID { get; set; }
        public int SSAExprUID { get; set; }
        public int SSAExpressionUID { get; set; }

        /// <summary> 
        /// ExpressionUID is used as the unique id to identify an expression
        /// It is incremented by 1 after mapping its current value to an expression
        /// </summary>
        public int ExpressionUID { get; set; }

        private int tempVarId = 0;
        private int tempLanguageId = 0;


        // TODO Jun: Cleansify me - i dont need to be here
        public AssociativeNode AssocNode { get; set; }
        public int watchStartPC { get; set; }


        //
        // TODO Jun: This is the expression interpreters executable. 
        //           It must be moved to its own core, whre each core is an instance of a compiler+interpreter
        //
        public Executable ExprInterpreterExe { get; set; }
        public int watchFunctionScope { get; set; }
        public int watchBaseOffset { get; set; }
        public List<SymbolNode> watchSymbolList { get; set; }

        public CodeGen assocCodegen { get; set; }

        public TextWriter ExecutionLog { get; set; }

        public Core(Options options)
        {
            ResetAll(options);
        }

        public SymbolNode GetFirstVisibleSymbol(string name, int classscope, int function, CodeBlock codeblock)
        {
            Validity.Assert(null != codeblock);
            int symbolIndex = Constants.kInvalidIndex;

            // For variables defined nested language block, their function index
            // is always Constants.kGlobalScope 
            CodeBlock searchBlock = codeblock;
            while (searchBlock != null)
            {
                // For imported node, it is possbile that the block is not the
                // topmost block.
                // 
                // For expression interpreter, as the code has been compiled, the
                // outmost block wouldn't be function block (CodeBlockType.Function).
                // CodeBlockType.Function is a temporary block type which is set when
                // the compile is generating code for function defintion node and will
                // be set back to Associative.
                var isSearchBoundry = searchBlock.blockType == CodeBlockType.Function ||
                    (Options.RunMode == InterpreterMode.Expression && searchBlock.parent == null);

                if (isSearchBoundry)
                {
                    break;
                }

                symbolIndex = searchBlock.symbolTable.IndexOf(name, classscope, Constants.kGlobalScope);
                if (symbolIndex == Constants.kInvalidIndex)
                {
                    searchBlock = searchBlock.parent;
                }
                else
                {
                    return searchBlock.symbolTable.symbolList[symbolIndex];
                }
            }

            // Search variable might be defined in function. 
            // If we are not in class defintion, then just stop here, otherwise
            // we should search global block's symbol table.
            if (searchBlock != null && 
                (searchBlock.blockType == CodeBlockType.Function || (Options.RunMode == InterpreterMode.Expression && searchBlock.parent == null)) && 
                classscope == Constants.kGlobalScope)
            {
                symbolIndex = searchBlock.symbolTable.IndexOf(name, classscope, function);
            }

            return symbolIndex == Constants.kInvalidIndex ? null : searchBlock.symbolTable.symbolList[symbolIndex];
        }

        public bool IsFunctionCodeBlock(CodeBlock cblock)
        {
            // Determine if the immediate block is a function block
            // Construct blocks are ignored
            Validity.Assert(null != cblock);
            while (null != cblock)
            {
                if (CodeBlockType.Function == cblock.blockType)
                {
                    return true;
                }
                else if (CodeBlockType.Language == cblock.blockType)
                {
                    return false;
                }
                cblock = cblock.parent;
            }
            return false;
        }

        private void BfsBuildSequenceTable(CodeBlock codeBlock, SymbolTable[] runtimeSymbols)
        {
            if (CodeBlockType.Language == codeBlock.blockType
                || CodeBlockType.Function == codeBlock.blockType
                || CodeBlockType.Construct == codeBlock.blockType)
            {
                Validity.Assert(codeBlock.symbolTable.RuntimeIndex < RuntimeTableIndex);
                runtimeSymbols[codeBlock.symbolTable.RuntimeIndex] = codeBlock.symbolTable;
            }

            foreach (CodeBlock child in codeBlock.children)
            {
                BfsBuildSequenceTable(child, runtimeSymbols);
            }
        }

        private void BfsBuildProcedureTable(CodeBlock codeBlock, ProcedureTable[] procTable)
        {
            if (CodeBlockType.Language == codeBlock.blockType || CodeBlockType.Function == codeBlock.blockType)
            {
                Validity.Assert(codeBlock.procedureTable.RuntimeIndex < RuntimeTableIndex);
                procTable[codeBlock.procedureTable.RuntimeIndex] = codeBlock.procedureTable;
            }

            foreach (CodeBlock child in codeBlock.children)
            {
                BfsBuildProcedureTable(child, procTable);
            }
        }

        private void BfsBuildInstructionStreams(CodeBlock codeBlock, InstructionStream[] istreamList)
        {
            if (null != codeBlock)
            {
                if (CodeBlockType.Language == codeBlock.blockType || CodeBlockType.Function == codeBlock.blockType)
                {
                    Validity.Assert(codeBlock.codeBlockId < RuntimeTableIndex);
                    istreamList[codeBlock.codeBlockId] = codeBlock.instrStream;
                }

                foreach (CodeBlock child in codeBlock.children)
                {
                    BfsBuildInstructionStreams(child, istreamList);
                }
            }
        }


        public void GenerateExprExe()
        {
            // TODO Jun: Determine if we really need another executable for the expression interpreter
            Validity.Assert(null == ExprInterpreterExe);
            ExprInterpreterExe = new Executable();
            ExprInterpreterExe.FunctionTable = FunctionTable;
            ExprInterpreterExe.DynamicVarTable = DynamicVariableTable;
            ExprInterpreterExe.FuncPointerTable = FunctionPointerTable;
            ExprInterpreterExe.DynamicFuncTable = DynamicFunctionTable;
            ExprInterpreterExe.ContextDataMngr = ContextDataManager;
            ExprInterpreterExe.Configurations = Configurations;
            ExprInterpreterExe.CodeToLocation = CodeToLocation;
            ExprInterpreterExe.CurrentDSFileName = CurrentDSFileName;
           
            // Copy all tables
            ExprInterpreterExe.classTable = DSExecutable.classTable;
            ExprInterpreterExe.procedureTable = DSExecutable.procedureTable;
            ExprInterpreterExe.runtimeSymbols = DSExecutable.runtimeSymbols;
          

            ExprInterpreterExe.TypeSystem = TypeSystem;
            
            // Copy all instruction streams
            // TODO Jun: What method to copy all? Use that
            ExprInterpreterExe.instrStreamList = new InstructionStream[DSExecutable.instrStreamList.Length];
            for (int i = 0; i < DSExecutable.instrStreamList.Length; ++i)
            {
                if (null != DSExecutable.instrStreamList[i])
                {
                    ExprInterpreterExe.instrStreamList[i] = new InstructionStream(DSExecutable.instrStreamList[i].language, this);
                    //ExprInterpreterExe.instrStreamList[i] = new InstructionStream(DSExecutable.instrStreamList[i].language, DSExecutable.instrStreamList[i].dependencyGraph, this);
                    for (int j = 0; j < DSExecutable.instrStreamList[i].instrList.Count; ++j)
                    {
                        ExprInterpreterExe.instrStreamList[i].instrList.Add(DSExecutable.instrStreamList[i].instrList[j]);
                    }
                }
            }
        }


        public void GenerateExprExeInstructions(int blockScope)
        {
            // Append the expression instruction at the end of the current block
            for (int n = 0; n < ExprInterpreterExe.iStreamCanvas.instrList.Count; ++n)
            {
                ExprInterpreterExe.instrStreamList[blockScope].instrList.Add(ExprInterpreterExe.iStreamCanvas.instrList[n]);
            }
        }


        public void GenerateExecutable()
        {
            Validity.Assert(CodeBlockList.Count >= 0);
            DSExecutable = new Executable();
            // Create the code block list data
            DSExecutable.CodeBlocks = new List<CodeBlock>();
            DSExecutable.CodeBlocks.AddRange(CodeBlockList);
            DSExecutable.CompleteCodeBlocks = new List<CodeBlock>();
            DSExecutable.CompleteCodeBlocks.AddRange(CompleteCodeBlockList);


            // Retrieve the class table directly since it is a global table
            DSExecutable.classTable = ClassTable;

            // The TypeSystem is a record of all primitive and compiler generated types
            DSExecutable.TypeSystem = TypeSystem;

            RuntimeTableIndex = GetRuntimeTableSize();


            // Build the runtime symbols
            DSExecutable.runtimeSymbols = new SymbolTable[RuntimeTableIndex];
            for (int n = 0; n < CodeBlockList.Count; ++n)
            {
                BfsBuildSequenceTable(CodeBlockList[n], DSExecutable.runtimeSymbols);
            }

            // Build the runtime procedure table
            DSExecutable.procedureTable = new ProcedureTable[RuntimeTableIndex];
            for (int n = 0; n < CodeBlockList.Count; ++n)
            {
                BfsBuildProcedureTable(CodeBlockList[n], DSExecutable.procedureTable);
            }

            // Build the executable instruction streams
            DSExecutable.instrStreamList = new InstructionStream[RuntimeTableIndex];
            for (int n = 0; n < CodeBlockList.Count; ++n)
            {
                BfsBuildInstructionStreams(CodeBlockList[n], DSExecutable.instrStreamList);
            }

            GenerateExprExe();
            DSExecutable.FunctionTable = FunctionTable;
            DSExecutable.DynamicVarTable = DynamicVariableTable;
            DSExecutable.DynamicFuncTable = DynamicFunctionTable;
            DSExecutable.FuncPointerTable = FunctionPointerTable;
            DSExecutable.ContextDataMngr = ContextDataManager;
            DSExecutable.Configurations = Configurations;
            DSExecutable.CodeToLocation = CodeToLocation;
            DSExecutable.CurrentDSFileName = CurrentDSFileName;           
        }

        /// <summary>
        /// Gets the size to be used for runtime tables of symbols, procedures and instruction streams.
        /// Note: since blocks are stored consecutively but may have gaps due to procedures being deleted,
        /// this is based on largest id rather than amount of blocks.
        /// </summary>
        internal int GetRuntimeTableSize()
        {
            // Due to the way this list is constructed, the largest id is the one of the last block.
            var lastBlock = CompleteCodeBlockList.LastOrDefault();
            // If there are no code blocks yet, then the required size for tables is 0.
            // This happens when the first code block is being created and its id is being generated.
            if (lastBlock == null)
            {
                return 0;
            }

            // If the last block has children, then its last child has the largest id.
            if (lastBlock.children.Count > 0)
            {
                lastBlock = lastBlock.children.Last();
            }
            return lastBlock.codeBlockId + 1;
        }

        public string GenerateTempVar()
        {
            tempVarId++;
            return Constants.kTempVar + tempVarId.ToString();
        }

        public string GenerateTempPropertyVar()
        {
            tempVarId++;
            return Constants.kTempPropertyVar + tempVarId.ToString();
        }

        public string GenerateTempLangageVar()
        {
            tempLanguageId++;
            return Constants.kTempLangBlock + tempLanguageId.ToString();
        }

        public bool IsTempVar(String varName)
        {
            if (String.IsNullOrEmpty(varName))
            {
                return false;
            }
            return varName[0] == '%';
        }

        public GraphNode ExecutingGraphnode { get; set; }


        public void ResetSSASubscript(Guid guid, int subscript)
        {
            SSASubscript_GUID = guid;
            SSASubscript = subscript;
        }

        public void Cleanup()
        {
            CLRModuleType.ClearTypes();
        }
    }
}
