
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using ProtoCore.AST;
using ProtoCore.DSASM;
using ProtoCore.Utils;
using ProtoCore.Properties;

namespace ProtoCore
{
    public class BackpatchNode
    {
        public int bp;
        public int pc;
    }

    public class BackpatchTable
    {
        public List<BackpatchNode> backpatchList { get; protected set; }
        public BackpatchTable()
        {
            backpatchList = new List<BackpatchNode>();
        }

        public void Append(int bp, int pc)
        {
            BackpatchNode node = new BackpatchNode();
            node.bp = bp;
            node.pc = pc;
            backpatchList.Add(node);
        }

        public void Append(int bp)
        {
            BackpatchNode node = new BackpatchNode();
            node.bp = bp;
            node.pc = ProtoCore.DSASM.Constants.kInvalidIndex;
            backpatchList.Add(node);
        }
    }

    public abstract class CodeGen
    {
        protected Core core;
        protected bool emitReplicationGuide;

        //protected static Dictionary<ulong, ulong> codeToLocation = new Dictionary<ulong, ulong>();
        //public static Dictionary<ulong, ulong> codeToLocation = new Dictionary<ulong, ulong>();
    
        protected int pc;
        protected int argOffset;
        protected int classOffset;
        protected int globalClassIndex;
        protected bool dumpByteCode;
        protected bool isAssocOperator;
        protected bool isEntrySet;
        protected int targetLangBlock;
        protected int blockScope;
        protected bool enforceTypeCheck;
        public ProtoCore.DSASM.CodeBlock codeBlock { get; set; }
        public ProtoCore.CompileTime.Context context { get; set; }
        protected BuildStatus buildStatus;

        protected int globalProcIndex;
        protected ProtoCore.DSASM.ProcedureNode localProcedure;
        protected ProtoCore.AST.Node localFunctionDefNode;
        protected ProtoCore.AST.Node localCodeBlockNode;
        protected bool emitDebugInfo = true;
        protected int tryLevel;

        protected int currentBinaryExprUID = ProtoCore.DSASM.Constants.kInvalidIndex;
        protected List<ProtoCore.DSASM.ProcedureNode> functionCallStack;
        protected bool IsAssociativeArrayIndexing { get; set; }

        protected bool isEmittingImportNode = false;

        // The parser currently inteprets floating point literal values as being 
        // separated by a period character '.', in cultural settings like German,
        // this will not be the case (they use ',' as decimal separation character).
        // For this reason, whenever "Convert.ToDouble" method is used to convert 
        // a 'string' value into a 'double' value, the conversion cannot be based 
        // on the current system culture (e.g. "de-DE"), it needs to be able to 
        // parse the string in "en-US" format (because that's how the parser is 
        // made to recognize floating point numbers.
        // 
        protected CultureInfo cultureInfo = new CultureInfo("en-US");


        // Contains the list of Nodes in an identifier list
        protected Stack<List<ProtoCore.AST.AssociativeAST.AssociativeNode>> ssaPointerStack;

        // The first graphnode of the SSA'd identifier
        protected ProtoCore.AssociativeGraph.GraphNode firstSSAGraphNode = null;

        // These variables hold data when backtracking static SSA'd calls
        protected string staticClass = null;

        public CodeGen(Core coreObj, ProtoCore.DSASM.CodeBlock parentBlock = null)
        {
            Validity.Assert(coreObj != null);
            core = coreObj;
            buildStatus = core.BuildStatus;
            isEntrySet = false;

            emitReplicationGuide = false;

            dumpByteCode = core.Options.DumpByteCode;
            isAssocOperator = false;

            pc = 0;
            argOffset = 0;
            globalClassIndex = core.ClassIndex;

            context = new ProtoCore.CompileTime.Context();

            targetLangBlock = ProtoCore.DSASM.Constants.kInvalidIndex;

            enforceTypeCheck = true;

            localProcedure = core.ProcNode;
            globalProcIndex = null != localProcedure ? localProcedure.procId : ProtoCore.DSASM.Constants.kGlobalScope;

            tryLevel = 0;

            functionCallStack = new List<DSASM.ProcedureNode>();

            IsAssociativeArrayIndexing = false;

            if (core.AsmOutput == null)
            {
                if (core.Options.CompileToLib)
                {
                    string path = "";
                    if (core.Options.LibPath == null)
                    {
                        path += core.Options.RootModulePathName + "ASM";
                    }
                    else
                    {
                        path = Path.Combine(core.Options.LibPath, Path.GetFileNameWithoutExtension(core.Options.RootModulePathName) + ".dsASM");
                    }

                    core.AsmOutput = new StreamWriter(File.Open(path, FileMode.Create));
                }
                else
                {
                    core.AsmOutput = Console.Out;
                }
            }

            ssaPointerStack = new Stack<List<AST.AssociativeAST.AssociativeNode>>();
        }


        private ProcedureNode GetProcedureNode(int classIndex, int functionIndex)
        {
            if (Constants.kGlobalScope != classIndex)
            {
                return core.ClassTable.ClassNodes[classIndex].vtable.procList[functionIndex];
            }
            return codeBlock.procedureTable.procList[functionIndex];
        }

        /// <summary>
        /// Append the graphnode to the instruction stream and procedure nodes
        /// </summary>
        /// <param name="graphnode"></param>
        protected void PushGraphNode(AssociativeGraph.GraphNode graphnode)
        {
            codeBlock.instrStream.dependencyGraph.Push(graphnode);
            if (globalProcIndex != Constants.kGlobalScope)
            {
                localProcedure.GraphNodeList.Add(graphnode);
            }
        }

        /// <summary>
        /// Generates unique identifier for the callsite associated with the graphnode
        /// </summary>
        /// <param name="graphNode"></param>
        /// <param name="procNode"></param>
        protected void GenerateCallsiteIdentifierForGraphNode(AssociativeGraph.GraphNode graphNode, string procName)
        {
            // This instance count in which the function appears lexically in the current guid
            // This must be stored and incremented
            int functionCallInstance = 0;


            // Cache this function call instance count 
            // This is the count of the number of times in which this callsite appears in the program
            int callInstance = 0;
            if (!core.CallsiteGuidMap.TryGetValue(graphNode.guid, out callInstance))
            {
                // The guid doesnt exist yet
                core.CallsiteGuidMap.Add(graphNode.guid, 0);
            }
            else
            {
                // Increment the current count
                core.CallsiteGuidMap[graphNode.guid]++;
                functionCallInstance = core.CallsiteGuidMap[graphNode.guid];
            }

            // Build the unique ID for a callsite 
            string callsiteIdentifier =
                procName + 
                "_InClassDecl" + globalClassIndex + 
                "_InFunctionScope" + globalProcIndex + 
                "_Instance" + functionCallInstance.ToString() + 
                "_" + graphNode.guid.ToString();

            // TODO Jun: Address this in MAGN-3774
            // The current limitation is retrieving the cached trace data for multiple callsites in a single CBN
            // after modifying the lines of code.
            graphNode.CallsiteIdentifier = callsiteIdentifier;
        }

        protected ProtoCore.DSASM.AddressType GetOpType(ProtoCore.PrimitiveType type)
        {
            ProtoCore.DSASM.AddressType optype = ProtoCore.DSASM.AddressType.Int;
            // Data coercion for the prototype
            // The JIL executive handles int primitives
            if (ProtoCore.PrimitiveType.kTypeInt == type
                || ProtoCore.PrimitiveType.kTypeBool == type
                || ProtoCore.PrimitiveType.kTypeChar == type
                || ProtoCore.PrimitiveType.kTypeString == type)
            {
                optype = ProtoCore.DSASM.AddressType.Int;
            }
            else if (ProtoCore.PrimitiveType.kTypeDouble == type)
            {
                optype = ProtoCore.DSASM.AddressType.Double;
            }
            else if (ProtoCore.PrimitiveType.kTypeVar == type)
            {
                optype = ProtoCore.DSASM.AddressType.VarIndex;
            }
            else if (ProtoCore.PrimitiveType.kTypeReturn == type)
            {
                optype = ProtoCore.DSASM.AddressType.Register;
            }
            else
            {
                Validity.Assert(false);
            }
            return optype;
        }

        protected StackValue BuildOperand(ProtoCore.DSASM.SymbolNode symbol)
        {
            if (symbol.classScope != ProtoCore.DSASM.Constants.kInvalidIndex &&
                symbol.functionIndex == ProtoCore.DSASM.Constants.kGlobalScope)
            {
                // Member var
                return symbol.isStatic 
                        ? StackValue.BuildStaticMemVarIndex(symbol.symbolTableIndex) 
                        : StackValue.BuildMemVarIndex(symbol.symbolTableIndex);
            }
            else
            {
                return StackValue.BuildVarIndex(symbol.symbolTableIndex);
            }
        }

        protected void AllocateVar(ProtoCore.DSASM.SymbolNode symbol)
        {
            symbol.isArray = false;
            SetStackIndex(symbol);
        }

        protected void SetHeapData(ProtoCore.DSASM.SymbolNode symbol)
        {
            symbol.size = ProtoCore.DSASM.Constants.kPointerSize;
            symbol.heapIndex = core.GlobHeapOffset++;
        }

        protected void SetStackIndex(ProtoCore.DSASM.SymbolNode symbol)
        {
            if (core.Options.RunMode == ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter)
            {
                //Set the index of the symbol relative to the watching stack
                symbol.index = core.watchBaseOffset;
                core.watchBaseOffset += symbol.size;
                return;
            }

            int langblockOffset = 0;
            bool isGlobal = null == localProcedure;

            if (ProtoCore.DSASM.Constants.kGlobalScope != globalClassIndex)
            {
                if (!isGlobal)
                {
                    // Local variable in a member function
                    symbol.index = -1 - ProtoCore.DSASM.StackFrame.kStackFrameSize - langblockOffset - core.BaseOffset;
                    core.BaseOffset += symbol.size;
                }
                else
                {
                    // Member variable: static variable allocated on global
                    // stack
                    if (symbol.isStatic)
                    {
                        symbol.index = core.GlobOffset - langblockOffset;
                        core.GlobOffset += symbol.size;
                    }
                    else
                    {
                        symbol.index = classOffset - langblockOffset;
                        classOffset += symbol.size;
                    }
                }
            }
            else if (!isGlobal)
            {
                // Local variable in a global function
                symbol.index = -1 - ProtoCore.DSASM.StackFrame.kStackFrameSize - langblockOffset - core.BaseOffset;
                core.BaseOffset += symbol.size;
            }
            else
            {
                // Global variable
                symbol.index = core.GlobOffset + langblockOffset;
                core.GlobOffset += symbol.size;
            }
        }

        #region EMIT_INSTRUCTION_TO_CONSOLE
        public void EmitCompileLog(string message)
        {
            if (dumpByteCode && !isAssocOperator && !core.Options.DumpOperatorToMethodByteCode)
            {
                for (int i = 0; i < core.AsmOutputIdents; ++i)
                    core.AsmOutput.Write("\t");
                core.AsmOutput.Write(message);
            }
            
        }
        public void EmitCompileLogFunctionStart(string function)
        {
            if (dumpByteCode && !isAssocOperator && !core.Options.DumpOperatorToMethodByteCode)
            {
                core.AsmOutput.Write(function);
                core.AsmOutput.Write("{\n");
                core.AsmOutputIdents++;
            }
        }

        public void EmitCompileLogFunctionEnd()
        {
            if (dumpByteCode && !isAssocOperator && !core.Options.DumpOperatorToMethodByteCode)
            {
                core.AsmOutputIdents--;
                for (int i = 0; i < core.AsmOutputIdents; ++i)
                    core.AsmOutput.Write("\t");
                core.AsmOutput.Write("}\n\n");
            }
        }

        public void EmitInstrConsole(string instr)
        {
            if (core.Options.DumpOperatorToMethodByteCode == false)
            {
                if (dumpByteCode && !isAssocOperator)
                {
                    var str = string.Format("[{0}.{1}.{2}]{3}\n", codeBlock.language == Language.kAssociative ? "a" : "i", codeBlock.codeBlockId, pc, instr);
                    for (int i = 0; i < core.AsmOutputIdents; ++i)
                        core.AsmOutput.Write("\t");
                    core.AsmOutput.Write(str);
                }
            }
            else
            {
                if (dumpByteCode)
                {
                    var str = string.Format("[{0}.{1}.{2}]{3}\n", codeBlock.language == Language.kAssociative ? "a" : "i", codeBlock.codeBlockId, pc, instr);
                    for (int i = 0; i < core.AsmOutputIdents; ++i)
                        core.AsmOutput.Write("\t");
                    core.AsmOutput.Write(str);
                }
            }
        }
        public void EmitInstrConsole(string instr, string op1)
        {
            if (core.Options.DumpOperatorToMethodByteCode == false)
            {
                if (dumpByteCode && !isAssocOperator)
                {
                    var str = string.Format("[{0}.{1}.{2}]{3} {4}\n", codeBlock.language == Language.kAssociative ? "a" : "i", codeBlock.codeBlockId, pc, instr, op1);
                    for (int i = 0; i < core.AsmOutputIdents; ++i)
                        core.AsmOutput.Write("\t");
                    core.AsmOutput.Write(str);
                }
            }
            else
            {
                if (dumpByteCode)
                {
                    var str = string.Format("[{0}.{1}.{2}]{3} {4}\n", codeBlock.language == Language.kAssociative ? "a" : "i", codeBlock.codeBlockId, pc, instr, op1);
                    for (int i = 0; i < core.AsmOutputIdents; ++i)
                        core.AsmOutput.Write("\t");
                    core.AsmOutput.Write(str);
                }
            }
        }
        public void EmitInstrConsole(string instr, string op1, string op2)
        {
            if (core.Options.DumpOperatorToMethodByteCode == false)
            {
                if (dumpByteCode && !isAssocOperator)
                {
                    var str = string.Format("[{0}.{1}.{2}]{3} {4} {5}\n", codeBlock.language == Language.kAssociative ? "a" : "i", codeBlock.codeBlockId, pc, instr, op1, op2);
                    for (int i = 0; i < core.AsmOutputIdents; ++i)
                        core.AsmOutput.Write("\t");
                    core.AsmOutput.Write(str);
                }
            }
            else
            {
                if (dumpByteCode)
                {
                    var str = string.Format("[{0}.{1}.{2}]{3} {4} {5}\n", codeBlock.language == Language.kAssociative ? "a" : "i", codeBlock.codeBlockId, pc, instr, op1, op2);
                    for (int i = 0; i < core.AsmOutputIdents; ++i)
                        core.AsmOutput.Write("\t");
                    core.AsmOutput.Write(str);
                }
            }
        }
        public void EmitInstrConsole(string instr, string op1, string op2, string op3)
        {
            if (core.Options.DumpOperatorToMethodByteCode == false)
            {
                if (dumpByteCode && !isAssocOperator)
                {
                    var str = string.Format("[{0}.{1}.{2}]{3} {4} {5} {6}\n", codeBlock.language == Language.kAssociative ? "a" : "i", codeBlock.codeBlockId, pc, instr, op1, op2, op3);
                    for (int i = 0; i < core.AsmOutputIdents; ++i)
                        core.AsmOutput.Write("\t");
                    core.AsmOutput.Write(str);
                }
            }
            else
            {
                if (dumpByteCode)
                {
                    var str = string.Format("[{0}.{1}.{2}]{3} {4} {5} {6}\n", codeBlock.language == Language.kAssociative ? "a" : "i", codeBlock.codeBlockId, pc, instr, op1, op2, op3);
                    for (int i = 0; i < core.AsmOutputIdents; ++i)
                        core.AsmOutput.Write("\t");
                    core.AsmOutput.Write(str);
                }
            }

        }
        #endregion //   EMIT_INSTRUCTION_TO_CONSOLE

        protected abstract void SetEntry();

        public abstract int Emit(ProtoCore.AST.Node codeblocknode, ProtoCore.AssociativeGraph.GraphNode graphNode = null);

        protected string GetConstructBlockName(string construct)
        {
            string desc = "blockname";
            return blockScope.ToString() + "_" + construct + "_" + desc;
        }

        protected ProtoCore.DSASM.DebugInfo GetDebugObject(int line, int col, int eline, int ecol, int nextStep_a, int nextStep_b = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            ProtoCore.DSASM.DebugInfo debug = null;

            if (core.Options.EmitBreakpoints)
            {
                if ( (core.Options.IDEDebugMode || core.Options.WatchTestMode || core.Options.IsDeltaExecution)
                    && ProtoCore.DSASM.Constants.kInvalidIndex != line
                    && ProtoCore.DSASM.Constants.kInvalidIndex != col)
                {
                    debug = new ProtoCore.DSASM.DebugInfo(line, col, eline, ecol, core.CurrentDSFileName);
                    debug.nextStep.Add(nextStep_a);

                    if (ProtoCore.DSASM.Constants.kInvalidIndex != nextStep_b)
                        debug.nextStep.Add(nextStep_b);
                }
            }

            return debug;
        }

        abstract protected void EmitGetterSetterForIdentList(
            ProtoCore.AST.Node node, 
            ref ProtoCore.Type inferedType, 
            ProtoCore.AssociativeGraph.GraphNode graphNode,
            ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass, 
            out bool isCollapsed, 
            ProtoCore.AST.Node setterArgument = null
            );

        /// <summary>
        /// Assigns the modified pointers in a function argument to the graphNode
        /// This enables associative update when a pointer that is an argument is modified within a function
        /// </summary>
        /// <param name="nodeRef"></param>
        /// <param name="graphNode"></param>
        protected void AutoGenerateUpdateArgumentReference(
            ProtoCore.AssociativeGraph.UpdateNodeRef nodeRef, ProtoCore.AssociativeGraph.GraphNode graphNode)
        {

            ProtoCore.DSASM.SymbolNode firstSymbol = null;

            // This is only valid within a function as we are dealing with function args
            if(localProcedure == null);
            {
                return;
            }
            
            // Check if there are at least 2 symbols in the list
            if (nodeRef.nodeList.Count < 2)
            {
                return;
            }

            firstSymbol = nodeRef.nodeList[0].symbol;

            bool isValidNodeRef = null != firstSymbol && nodeRef.nodeList[0].nodeType != ProtoCore.AssociativeGraph.UpdateNodeType.kMethod;
            if (!isValidNodeRef)
            {
                return;
            }
            
            // Now check if the first element of the identifier list is an argument
            foreach (ProtoCore.DSASM.ArgumentInfo argInfo in localProcedure.argInfoList)
            {
                if (argInfo.Name == firstSymbol.name)
                {
                    nodeRef.nodeList.RemoveAt(0);

                    List<ProtoCore.AssociativeGraph.UpdateNodeRef> refList = null;
                    bool found = localProcedure.updatedArgumentProperties.TryGetValue(argInfo.Name, out refList);
                    if (found)
                    {
                        refList.Add(nodeRef);
                    }
                    else
                    {
                        refList = new List<ProtoCore.AssociativeGraph.UpdateNodeRef>();
                        refList.Add(nodeRef);
                        localProcedure.updatedArgumentProperties.Add(argInfo.Name, refList);
                    }
                }
            }
        }

        //  
        //  proc dfsgetsymbollist(node, lefttype, nodeRef)
        //      if node is identlist
        //          dfsemitpushlist(node.left, lefttype, nodeRef)
        //      else if node is identifier
        //          def symbol = veryifyallocation(node.left, lefttype)
        //          if symbol is allocated
        //              lefttype = symbol.type
        //              def updateNode
        //              updateNode.symbol = symbol 
        //              updateNode.isMethod = false
        //              nodeRef.push(updateNode)
        //          end
        //      else if node is function call
        //          def procNode = procTable.GetProc(node)
        //          if procNode is allocated
        //              lefttype = procNode.returntype
        //              def updateNode
        //              updateNode.procNode = procNode
        //              updateNode.isMethod = true
        //              updateNodeRef.push(updateNode)
        //          end
        //      end
        //  end
        //

        public void DFSGetSymbolList(Node pNode, ref ProtoCore.Type lefttype, ProtoCore.AssociativeGraph.UpdateNodeRef nodeRef)
        {
            dynamic node = pNode;
            if (node is ProtoCore.AST.ImperativeAST.IdentifierListNode || node is ProtoCore.AST.AssociativeAST.IdentifierListNode)
            {
                dynamic bnode = node;
                DFSGetSymbolList(bnode.LeftNode, ref lefttype, nodeRef);
                node = bnode.RightNode;
            }
            
            if (node is ProtoCore.AST.ImperativeAST.IdentifierNode || node is ProtoCore.AST.AssociativeAST.IdentifierNode)
            {
                dynamic identnode = node;
                ProtoCore.DSASM.SymbolNode symbolnode = null;

                bool isAccessible = false;
                bool isAllocated = VerifyAllocation(identnode.Value, lefttype.UID, globalProcIndex, out symbolnode, out isAccessible);
                if (isAllocated)
                {
                    if (null == symbolnode)
                    {
                        // It is inaccessible from here due to access modifier.
                        // Just attempt to retrieve the symbol
                        int symindex = core.ClassTable.ClassNodes[lefttype.UID].GetFirstVisibleSymbolNoAccessCheck(identnode.Value);
                        if (ProtoCore.DSASM.Constants.kInvalidIndex != symindex)
                        {
                            symbolnode = core.ClassTable.ClassNodes[lefttype.UID].symbols.symbolList[symindex];
                        }
                    }

                    lefttype = symbolnode.datatype;

                    ProtoCore.AssociativeGraph.UpdateNode updateNode = new AssociativeGraph.UpdateNode();
                    updateNode.symbol = symbolnode;
                    updateNode.nodeType = ProtoCore.AssociativeGraph.UpdateNodeType.kSymbol;
                    nodeRef.PushUpdateNode(updateNode);
                }
                else
                {
                    // Is it a class?
                    int ci = core.ClassTable.IndexOf(identnode.Value);
                    if (ProtoCore.DSASM.Constants.kInvalidIndex != ci)
                    {
                        lefttype.UID = ci;

                        // Comment Jun:
                        // Create a symbol node that contains information about the class type that contains static properties
                        ProtoCore.DSASM.SymbolNode classSymbol = new DSASM.SymbolNode();
                        classSymbol.memregion = DSASM.MemoryRegion.kMemStatic;
                        classSymbol.name = identnode.Value;
                        classSymbol.classScope = ci;

                        ProtoCore.AssociativeGraph.UpdateNode updateNode = new AssociativeGraph.UpdateNode();
                        updateNode.symbol = classSymbol;
                        updateNode.nodeType = ProtoCore.AssociativeGraph.UpdateNodeType.kSymbol;
                        nodeRef.PushUpdateNode(updateNode);

                    }
                    else
                    {
                        // In this case, the lhs type is undefined
                        // Just attempt to create a symbol node
                        string ident = identnode.Value;
                        if (0 != ident.CompareTo(ProtoCore.DSDefinitions.Keyword.This))
                        {
                            symbolnode = new SymbolNode();
                            symbolnode.name = identnode.Value;

                            ProtoCore.AssociativeGraph.UpdateNode updateNode = new AssociativeGraph.UpdateNode();
                            updateNode.symbol = symbolnode;
                            updateNode.nodeType = AssociativeGraph.UpdateNodeType.kSymbol;
                            nodeRef.PushUpdateNode(updateNode);
                        }
                    }
                }
            }
            else if (node is ProtoCore.AST.ImperativeAST.FunctionCallNode || node is ProtoCore.AST.AssociativeAST.FunctionCallNode)
            {
                string functionName = node.Function.Value;
                if (ProtoCore.Utils.CoreUtils.IsGetterSetter(functionName))
                {
                    string property;
                    if (CoreUtils.TryGetPropertyName(functionName, out property))
                    {
                        functionName = property;
                    }
                    ProtoCore.DSASM.SymbolNode symbolnode = null;


                    bool isAccessible = false;
                    bool isAllocated = VerifyAllocation(functionName, lefttype.UID, globalProcIndex, out symbolnode, out isAccessible);
                    if (isAllocated)
                    {
                        if (null == symbolnode)
                        {
                            // It is inaccessible from here due to access modifier.
                            // Just attempt to retrieve the symbol
                            int symindex = core.ClassTable.ClassNodes[lefttype.UID].GetFirstVisibleSymbolNoAccessCheck(functionName);
                            if (ProtoCore.DSASM.Constants.kInvalidIndex != symindex)
                            {
                                symbolnode = core.ClassTable.ClassNodes[lefttype.UID].symbols.symbolList[symindex];
                            }
                        }

                        lefttype = symbolnode.datatype;

                        ProtoCore.AssociativeGraph.UpdateNode updateNode = new AssociativeGraph.UpdateNode();
                        updateNode.symbol = symbolnode;
                        updateNode.nodeType = AssociativeGraph.UpdateNodeType.kSymbol;
                        nodeRef.PushUpdateNode(updateNode);
                    }
                }
                else
                {
                    AssociativeGraph.UpdateNode updateNode = new AssociativeGraph.UpdateNode();
                    ProcedureNode procNodeDummy = new ProcedureNode();
                    procNodeDummy.name = functionName;
                    updateNode.procNode = procNodeDummy;
                    updateNode.nodeType = AssociativeGraph.UpdateNodeType.kMethod;
                    nodeRef.PushUpdateNode(updateNode);
                }
            }
        }

        // Deperecate this function after further regression testing and just use DFSGetSymbolList
        public void DFSGetSymbolList_Simple(Node pNode, ref ProtoCore.Type lefttype, ref int functionindex, ProtoCore.AssociativeGraph.UpdateNodeRef nodeRef)
        {
            dynamic node = pNode;
            if (node is ProtoCore.AST.ImperativeAST.IdentifierListNode || node is ProtoCore.AST.AssociativeAST.IdentifierListNode)
            {
                dynamic bnode = node;
                DFSGetSymbolList_Simple(bnode.LeftNode, ref lefttype, ref functionindex, nodeRef);
                node = bnode.RightNode;
            }

            if (node is ProtoCore.AST.ImperativeAST.IdentifierNode || node is ProtoCore.AST.AssociativeAST.IdentifierNode)
            {
                dynamic identnode = node;
                ProtoCore.DSASM.SymbolNode symbolnode = null;

                bool isAccessible = false;

                bool isAllocated = VerifyAllocation(identnode.Value, lefttype.UID, functionindex, out symbolnode, out isAccessible);
                if (isAllocated)
                {
                    if (null == symbolnode)
                    {
                        // It is inaccessible from here due to access modifier.
                        // Just attempt to retrieve the symbol
                        int symindex = core.ClassTable.ClassNodes[lefttype.UID].GetFirstVisibleSymbolNoAccessCheck(identnode.Value);
                        if (ProtoCore.DSASM.Constants.kInvalidIndex != symindex)
                        {
                            symbolnode = core.ClassTable.ClassNodes[lefttype.UID].symbols.symbolList[symindex];
                        }
                    }

                    // Since the variable was found, all succeeding nodes in the ident list are class members
                    // Class members have a function scope of kGlobalScope as they are only local to the class, not with any member function
                    functionindex = ProtoCore.DSASM.Constants.kGlobalScope;

                    lefttype = symbolnode.datatype;

                    ProtoCore.AssociativeGraph.UpdateNode updateNode = new AssociativeGraph.UpdateNode();
                    updateNode.symbol = symbolnode;
                    updateNode.nodeType = ProtoCore.AssociativeGraph.UpdateNodeType.kSymbol;
                    nodeRef.PushUpdateNode(updateNode);
                }
                else
                {
                    // Is it a class?
                    int ci = core.ClassTable.IndexOf(identnode.Value);
                    if (ProtoCore.DSASM.Constants.kInvalidIndex != ci)
                    {
                        lefttype.UID = ci;

                        // Comment Jun:
                        // Create a symbol node that contains information about the class type that contains static properties
                        ProtoCore.DSASM.SymbolNode classSymbol = new DSASM.SymbolNode();
                        classSymbol.memregion = DSASM.MemoryRegion.kMemStatic;
                        classSymbol.name = identnode.Value;
                        classSymbol.classScope = ci;

                        ProtoCore.AssociativeGraph.UpdateNode updateNode = new AssociativeGraph.UpdateNode();
                        updateNode.symbol = classSymbol;
                        updateNode.nodeType = ProtoCore.AssociativeGraph.UpdateNodeType.kSymbol;
                        nodeRef.PushUpdateNode(updateNode);

                    }
                    else
                    {
                        // In this case, the lhs type is undefined
                        // Just attempt to create a symbol node
                        string ident = identnode.Value;
                        if (0 != ident.CompareTo(ProtoCore.DSDefinitions.Keyword.This))
                        {
                            symbolnode = new SymbolNode();
                            symbolnode.name = identnode.Value;

                            ProtoCore.AssociativeGraph.UpdateNode updateNode = new AssociativeGraph.UpdateNode();
                            updateNode.symbol = symbolnode;
                            updateNode.nodeType = AssociativeGraph.UpdateNodeType.kSymbol;
                            nodeRef.PushUpdateNode(updateNode);
                        }
                    }
                }
            }
            else if (node is ProtoCore.AST.ImperativeAST.FunctionCallNode || node is ProtoCore.AST.AssociativeAST.FunctionCallNode)
            {
                string functionName = node.Function.Value;
                if (ProtoCore.Utils.CoreUtils.IsGetterSetter(functionName))
                {
                    string property;
                    if (CoreUtils.TryGetPropertyName(functionName, out property))
                    {
                        functionName = property;
                    }
                    ProtoCore.DSASM.SymbolNode symbolnode = null;


                    bool isAccessible = false;
                    bool isAllocated = VerifyAllocation(functionName, lefttype.UID, globalProcIndex, out symbolnode, out isAccessible);
                    if (isAllocated)
                    {
                        if (null == symbolnode)
                        {
                            // It is inaccessible from here due to access modifier.
                            // Just attempt to retrieve the symbol
                            int symindex = core.ClassTable.ClassNodes[lefttype.UID].GetFirstVisibleSymbolNoAccessCheck(functionName);
                            if (ProtoCore.DSASM.Constants.kInvalidIndex != symindex)
                            {
                                symbolnode = core.ClassTable.ClassNodes[lefttype.UID].symbols.symbolList[symindex];
                            }
                        }

                        lefttype = symbolnode.datatype;

                        ProtoCore.AssociativeGraph.UpdateNode updateNode = new AssociativeGraph.UpdateNode();
                        updateNode.symbol = symbolnode;
                        updateNode.nodeType = AssociativeGraph.UpdateNodeType.kSymbol;
                        nodeRef.PushUpdateNode(updateNode);
                    }
                }
                else
                {
                    ProtoCore.AssociativeGraph.UpdateNode updateNode = new AssociativeGraph.UpdateNode();
                    ProtoCore.DSASM.ProcedureNode procNodeDummy = new DSASM.ProcedureNode();
                    procNodeDummy.name = functionName;
                    updateNode.procNode = procNodeDummy;
                    updateNode.nodeType = AssociativeGraph.UpdateNodeType.kMethod;
                    nodeRef.PushUpdateNode(updateNode);
                }
            }
        }

        protected bool DfsEmitIdentList(
            Node pNode, 
            Node parentNode, 
            int contextClassScope, 
            ref ProtoCore.Type lefttype,
            ref int depth, 
            ref ProtoCore.Type finalType, 
            bool isLeftidentList, 
            ref bool isFirstIdent, 
            ref bool isMethodCallPresent,
            ref ProtoCore.DSASM.SymbolNode firstSymbol,
            ProtoCore.AssociativeGraph.GraphNode graphNode = null,
            ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone,
            ProtoCore.AST.Node binaryExpNode = null)
        {
            bool isRefFromIdentifier = false;

            dynamic node = pNode;
            if (node is ProtoCore.AST.ImperativeAST.IdentifierListNode || node is ProtoCore.AST.AssociativeAST.IdentifierListNode)
            {
                dynamic bnode = node;
                if (ProtoCore.DSASM.Operator.dot != bnode.Optr)
                {
                    string message = "The left hand side of an operation can only contain an indirection operator '.' (48D67B9B)";
                    buildStatus.LogSemanticError(message, core.CurrentDSFileName, bnode.line, bnode.col);
                    throw new BuildHaltException(message);
                }

                isRefFromIdentifier = DfsEmitIdentList(bnode.LeftNode, bnode, contextClassScope, ref lefttype, ref depth, ref finalType, isLeftidentList, ref isFirstIdent, ref isMethodCallPresent, ref firstSymbol, graphNode, subPass);

                if (lefttype.rank > 0)
                {
                    lefttype.UID = finalType.UID = (int)PrimitiveType.kTypeNull;
                    EmitPushNull();
                    return false;
                }
                node = bnode.RightNode;
            }

            if (node is ProtoCore.AST.ImperativeAST.GroupExpressionNode)
            {
                ProtoCore.AST.ImperativeAST.ArrayNode array = node.ArrayDimensions;
                node = node.Expression;
                node.ArrayDimensions = array;
            }
            else if (node is ProtoCore.AST.AssociativeAST.GroupExpressionNode)
            {
                ProtoCore.AST.AssociativeAST.ArrayNode array = node.ArrayDimensions;
                List<ProtoCore.AST.AssociativeAST.AssociativeNode> replicationGuides = node.ReplicationGuides;

                node = node.Expression;
                node.ArrayDimensions = array;
                node.ReplicationGuides = replicationGuides;
            }

            if (node is ProtoCore.AST.ImperativeAST.IdentifierNode || node is ProtoCore.AST.AssociativeAST.IdentifierNode)
            {
                dynamic identnode = node;

                int ci = core.ClassTable.IndexOf(identnode.Value);
                if (ProtoCore.DSASM.Constants.kInvalidIndex != ci)
                {
                    finalType.UID = lefttype.UID = ci;
                }
                else if (identnode.Value == ProtoCore.DSDefinitions.Keyword.This)
                {
                    finalType.UID = lefttype.UID = contextClassScope;
                    EmitInstrConsole(ProtoCore.DSASM.kw.push, 0 + "[dim]");
                    StackValue opdim = StackValue.BuildArrayDimension(0);
                    EmitPush(opdim);
                    EmitThisPointerNode();
                    depth++;
                    return true;
                }
                else
                {
                    ProtoCore.DSASM.SymbolNode symbolnode = null;
                    bool isAllocated = false;
                    bool isAccessible = false;
                    if (lefttype.UID != -1)
                    {
                        isAllocated = VerifyAllocation(identnode.Value, lefttype.UID, globalProcIndex, out symbolnode, out isAccessible);
                    }
                    else
                    {
                        isAllocated = VerifyAllocation(identnode.Value, contextClassScope, globalProcIndex, out symbolnode, out isAccessible);
                        Validity.Assert(null == firstSymbol);
                        firstSymbol = symbolnode;
                    }

                    bool callOnClass = false;
                    string leftClassName = "";
                    int leftci = Constants.kInvalidIndex;

                    if (pNode is ProtoCore.AST.ImperativeAST.IdentifierListNode ||
                        pNode is ProtoCore.AST.AssociativeAST.IdentifierListNode)
                    {
                        dynamic leftnode = ((dynamic)pNode).LeftNode;
                        if (leftnode != null && 
                            (leftnode is ProtoCore.AST.ImperativeAST.IdentifierNode ||
                            leftnode is ProtoCore.AST.AssociativeAST.IdentifierNode))
                        {
                            leftClassName = leftnode.Name;
                            leftci = core.ClassTable.IndexOf(leftClassName);
                            if (leftci != ProtoCore.DSASM.Constants.kInvalidIndex)
                            {
                                callOnClass = true;

                                EmitInstrConsole(ProtoCore.DSASM.kw.push, 0 + "[dim]");
                                StackValue dynamicOpdim = StackValue.BuildArrayDimension(0);
                                EmitPush(dynamicOpdim);

                                EmitInstrConsole(ProtoCore.DSASM.kw.pushm, leftClassName);
                                StackValue classOp = StackValue.BuildClassIndex(leftci);
                                EmitPushm(classOp, globalClassIndex, codeBlock.codeBlockId);

                                depth = depth + 1;
                            }
                        }
                    }

                    if (null == symbolnode)    //unbound identifier
                    {
                        if (isAllocated && !isAccessible)
                        {
                            string message = String.Format(Resources.kPropertyIsInaccessible, identnode.Value);
                            buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kAccessViolation, message, core.CurrentDSFileName, identnode.line, identnode.col, graphNode);
                            lefttype.UID = finalType.UID = (int)PrimitiveType.kTypeNull;
                            EmitPushNull();
                            return false;
                        }
                        else
                        {
                            string message = String.Format(Resources.kUnboundIdentifierMsg, identnode.Value);
                            buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kIdUnboundIdentifier, message, core.CurrentDSFileName, identnode.line, identnode.col, graphNode);
                        }

                        if (depth == 0)
                        {
                            lefttype.UID = finalType.UID = (int)PrimitiveType.kTypeNull;
                            EmitPushNull();
                            depth = 1;
                            return false;
                        }
                        else
                        {
                            DSASM.DyanmicVariableNode dynamicVariableNode = new DSASM.DyanmicVariableNode(identnode.Value, globalProcIndex, globalClassIndex);
                            core.DynamicVariableTable.variableTable.Add(dynamicVariableNode);
                            int dim = 0;
                            if (null != identnode.ArrayDimensions)
                            {
                                dim = DfsEmitArrayIndexHeap(identnode.ArrayDimensions, graphNode);
                            }
                            EmitInstrConsole(ProtoCore.DSASM.kw.push, dim + "[dim]");
                            StackValue dynamicOpdim = StackValue.BuildArrayDimension(dim);
                            EmitPush(dynamicOpdim);

                            EmitInstrConsole(ProtoCore.DSASM.kw.pushm, identnode.Value + "[dynamic]");
                            StackValue dynamicOp = StackValue.BuildDynamic(core.DynamicVariableTable.variableTable.Count - 1);
                            EmitPushm(dynamicOp, symbolnode == null ? globalClassIndex : symbolnode.classScope, DSASM.Constants.kInvalidIndex);

                            lefttype.UID = finalType.UID = (int)PrimitiveType.kTypeVar;
                            depth++;
                            return true;
                        }
                    }
                    else
                    {
                        if (callOnClass && !symbolnode.isStatic)
                        {
                            string procName = identnode.Name;
                            string property;
                            ProtoCore.DSASM.ProcedureNode staticProcCallNode = core.ClassTable.ClassNodes[leftci].GetFirstStaticFunctionBy(procName);

                            if (null != staticProcCallNode)
                            {
                                string message = String.Format(Resources.kMethodHasInvalidArguments, procName);
                                buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kCallingNonStaticMethodOnClass, message, core.CurrentDSFileName, identnode.line, identnode.col, graphNode);
                            }
                            else if (CoreUtils.TryGetPropertyName(procName, out property))
                            {
                                string message = String.Format(Resources.kPropertyIsInaccessible, property);
                                buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kCallingNonStaticMethodOnClass, message, core.CurrentDSFileName, identnode.line, identnode.col, graphNode);
                            }
                            else
                            {
                                string message = String.Format(Resources.kMethodIsInaccessible, procName);
                                buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kCallingNonStaticMethodOnClass, message, core.CurrentDSFileName, identnode.line, identnode.col, graphNode);
                            }

                            lefttype.UID = finalType.UID = (int)PrimitiveType.kTypeNull;
                            EmitPushNull();
                            return false;
                        }
                    }

                    //
                    // The graph node depends on the first identifier in this identifier list
                    // Where:
                    //      p = f(1);			        
                    //      px = p.x; // px dependent on p
                    //      p = f(2);
                    //
                    if (isFirstIdent && null != graphNode)
                    {
                        isFirstIdent = false;
                        //ProtoCore.AssociativeGraph.GraphNode dependentNode = new ProtoCore.AssociativeGraph.GraphNode();
                        //dependentNode.symbol = symbolnode;
                        //dependentNode.symbolList.Add(symbolnode);
                        //graphNode.PushDependent(dependentNode);
                    }
                    
                    /* Dont try to figure out the type at compile time if it is
                     * an array, it is just not reliable because each element in
                     * an array can have different types
                     */
                    if (!symbolnode.datatype.IsIndexable || symbolnode.datatype.rank < 0)
                        lefttype = symbolnode.datatype;

                    int dimensions = 0;

                    // Get the symbols' table index
                    int runtimeIndex = symbolnode.runtimeTableIndex;

                    ProtoCore.DSASM.AddressType operandType = ProtoCore.DSASM.AddressType.Pointer;

                    if (null != identnode.ArrayDimensions)
                    {
                        dimensions = DfsEmitArrayIndexHeap(identnode.ArrayDimensions, graphNode);
                        operandType = ProtoCore.DSASM.AddressType.ArrayPointer;
                    }

                    if (lefttype.rank >= 0)
                    {
                        lefttype.rank -= dimensions;
                        if (lefttype.rank < 0)
                        {
                            lefttype.rank = 0;
                        }
                    }

                    if (0 == depth || (symbolnode != null && symbolnode.isStatic))
                    {
                        if (ProtoCore.DSASM.Constants.kGlobalScope == symbolnode.functionIndex
                            && ProtoCore.DSASM.Constants.kInvalidIndex != symbolnode.classScope)
                        {
                            // member var
                            operandType = symbolnode.isStatic ? ProtoCore.DSASM.AddressType.StaticMemVarIndex : ProtoCore.DSASM.AddressType.MemVarIndex;
                        }
                        else
                        {
                            operandType = ProtoCore.DSASM.AddressType.VarIndex;
                        }
                    }

                    StackValue op = new StackValue();
                    op.optype = operandType;
                    op.opdata = symbolnode.symbolTableIndex;

                    // TODO Jun: Performance. 
                    // Is it faster to have a 'push' specific to arrays to prevent pushing dimension for push instruction?
                    EmitInstrConsole(ProtoCore.DSASM.kw.push, dimensions + "[dim]");
                    StackValue opdim = StackValue.BuildArrayDimension(dimensions);
                    EmitPush(opdim);

                    
                    if (isLeftidentList || depth == 0)
                    {
                        EmitInstrConsole(ProtoCore.DSASM.kw.pushm, identnode.Value);
                        EmitPushm(op, symbolnode == null ? globalClassIndex : symbolnode.classScope, runtimeIndex);
                    }
                    else
                    {
                        // change to dynamic call to facilitate update mechanism
                        DSASM.DyanmicVariableNode dynamicVariableNode = new DSASM.DyanmicVariableNode(identnode.Name, globalProcIndex, globalClassIndex);
                        core.DynamicVariableTable.variableTable.Add(dynamicVariableNode);
                        StackValue dynamicOp = StackValue.BuildDynamic(core.DynamicVariableTable.variableTable.Count - 1);
                        EmitInstrConsole(ProtoCore.DSASM.kw.pushm, identnode.Value + "[dynamic]");
                        EmitPushm(dynamicOp, symbolnode == null ? globalClassIndex : symbolnode.classScope, runtimeIndex);
                    }
                    depth = depth + 1;
                    finalType = lefttype;
                }
                return true;
            }
            else if (node is ProtoCore.AST.ImperativeAST.FunctionCallNode || node is ProtoCore.AST.AssociativeAST.FunctionCallNode)
            {
                // A function call must always track dependents
                bool allowDependents = true;
                if (null != graphNode)
                {
                    allowDependents = graphNode.allowDependents;
                    graphNode.allowDependents = true;
                }

                if (binaryExpNode != null)
                {
                    ProtoCore.Utils.NodeUtils.SetNodeLocation(node, binaryExpNode, binaryExpNode);
                }
                ProtoCore.DSASM.ProcedureNode procnode = TraverseFunctionCall(node, pNode, lefttype.UID, depth, ref finalType, graphNode, subPass, binaryExpNode);

                // Restore the graphNode dependent state
                if (null != graphNode)
                {
                    graphNode.allowDependents = allowDependents;
                }

                // This is the first non-auto generated procedure found in the identifier list
                if (null != procnode)
                {
                    if (!procnode.isConstructor && !procnode.name.Equals(ProtoCore.DSASM.Constants.kStaticPropertiesInitializer))
                    {
                        functionCallStack.Add(procnode);
                        if (null != graphNode)
                        {
                            graphNode.firstProcRefIndex = graphNode.dependentList.Count - 1;
                        }
                    }
                    isMethodCallPresent = !isMethodCallPresent && !procnode.isAutoGenerated && !procnode.isConstructor;
                }

                //finalType.UID = isBooleanOp ? (int)PrimitiveType.kTypeBool : finalType.UID;
                lefttype = finalType;
                depth = 1;
            }
            else
            {
                string message = "The left side of operator '.' must be an identifier. (B9AEA3A6)";
                buildStatus.LogSemanticError(message, core.CurrentDSFileName, node.line, node.col);
                throw new BuildHaltException(message);
            }
            return false;
        }


        protected int DfsEmitArrayIndexHeap(Node node, AssociativeGraph.GraphNode graphNode = null, ProtoCore.AST.Node parentNode = null, ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone)
        {
            int indexCnt = 0;
            Validity.Assert(node is ProtoCore.AST.AssociativeAST.ArrayNode || node is ProtoCore.AST.ImperativeAST.ArrayNode);

            IsAssociativeArrayIndexing = true;

            dynamic arrayNode = node;
            while (arrayNode is ProtoCore.AST.AssociativeAST.ArrayNode || arrayNode is ProtoCore.AST.ImperativeAST.ArrayNode)
            {
                ++indexCnt;
                dynamic array = arrayNode;
                ProtoCore.Type lastType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0);
                DfsTraverse(array.Expr, ref lastType, false, graphNode, subPass, parentNode);
                arrayNode = array.Type;
            }

            IsAssociativeArrayIndexing = false;

            return indexCnt;
        }

#if USE_PREVIOUS_FUNCCALL_TRAVERSAL 
        protected bool VerifyAllocation(string name, int classScope, out ProtoCore.DSASM.SymbolNode node)
        {
            // Identifier scope resolution
            //  1. Current block
            //  2. Outer language blocks
            //  3. Class scope (TODO Jun: Implement checking the class scope)
            //  4. Global scope (Comment Jun: Is there really a global scope? Conceptually, the outer most block is considered global)
            //node = core.GetFirstVisibleSymbol(name, classScope, functionindex, codeBlock);
            node = core.GetFirstVisibleSymbol(name, classScope, procIndex, codeBlock);
            if (null != node)
            {
                return true;
            }
            return false;
        }
#else

        /// <summary>
        /// Verifies the allocation of a variable in the given symbol table
        /// </summary>
        /// <param name="name"></param>
        /// <param name="classScope"></param>
        /// <param name="functionScope"></param>
        /// <param name="symbolTable"></param>
        /// <param name="symbol"></param>
        /// <param name="isAccessible"></param>
        /// <returns></returns>
        protected bool VerifyAllocationInScope(string name, int classScope, int functionScope, out ProtoCore.DSASM.SymbolNode symbol, out bool isAccessible)
        {
            SymbolTable symbolTable = null;
            if (classScope == ProtoCore.DSASM.Constants.kInvalidIndex)
            {
                symbolTable = codeBlock.symbolTable;
            }
            else
            {
                symbolTable = core.ClassTable.ClassNodes[classScope].symbols;
            }

            symbol = null;
            isAccessible = false;
            //int symbolIndex = symbolTable.IndexOf(name, Constants.kGlobalScope, Constants.kGlobalScope);
            int symbolIndex = symbolTable.IndexOf(name, classScope, functionScope);
            if (symbolIndex != Constants.kInvalidIndex)
            {
                symbol = symbolTable.symbolList[symbolIndex];
                isAccessible = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Verify the allocation of a variable in the current scope and parent scopes
        /// </summary>
        /// <param name="name"></param>
        /// <param name="classScope"></param>
        /// <param name="functionScope"></param>
        /// <param name="symbol"></param>
        /// <param name="isAccessible"></param>
        /// <returns></returns>
        protected bool VerifyAllocation(string name, int classScope, int functionScope, out ProtoCore.DSASM.SymbolNode symbol, out bool isAccessible)
        {
            int symbolIndex = Constants.kInvalidIndex;
            symbol = null;
            isAccessible = false;
            CodeBlock currentCodeBlock = codeBlock;
            if (core.Options.RunMode == DSASM.InterpreterMode.kExpressionInterpreter)
            {
                int tempBlockId = context.CurrentBlockId;
                currentCodeBlock = ProtoCore.Utils.CoreUtils.GetCodeBlock(core.CodeBlockList, tempBlockId);
            }

            if (classScope != Constants.kGlobalScope)
            {
                if (IsInLanguageBlockDefinedInFunction())
                {
                    symbolIndex = currentCodeBlock.symbolTable.IndexOf(name, Constants.kGlobalScope, Constants.kGlobalScope);
                    if (symbolIndex != Constants.kInvalidIndex)
                    {
                        symbol = currentCodeBlock.symbolTable.symbolList[symbolIndex];
                        isAccessible = true;
                        return true;
                    }
                }

                if ((int)ProtoCore.PrimitiveType.kTypeVoid == classScope)
                {
                    return false;
                }

                if (core.Options.RunMode == ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter)
                {
                    //Search local variables in the class member function first
                    if (functionScope != Constants.kGlobalScope)
                    {
                        // Aparajit: This function is found to not work well in the expression interpreter as it doesn't return the
                        // correct symbol if the same symbol exists in different contexts such as inside a function defined in a lang block,
                        // inside the lang block itself and in a function in the global scope etc.
                        // TODO: We can later consider replacing GetSymbolInFunction with GetFirstVisibleSymbol consistently in all occurrences 
                        
                        //symbol = core.GetSymbolInFunction(name, classScope, functionScope, currentCodeBlock);
                        symbol = core.GetFirstVisibleSymbol(name, classScope, functionScope, currentCodeBlock);
                        if (symbol != null)
                        {
                            isAccessible = true;
                            return true;
                        }
                    }
                }

                ClassNode thisClass = core.ClassTable.ClassNodes[classScope];

                bool hasThisSymbol;
                AddressType addressType;
                symbolIndex = ClassUtils.GetSymbolIndex(thisClass, name, classScope, functionScope, currentCodeBlock.codeBlockId, core.CompleteCodeBlockList, out hasThisSymbol, out addressType);
                if (Constants.kInvalidIndex != symbolIndex)
                {
                    // It is static member, then get node from code block
                    if (AddressType.StaticMemVarIndex == addressType)
                    {
                        symbol = core.CodeBlockList[0].symbolTable.symbolList[symbolIndex];
                    }
                    else
                    {
                        symbol = thisClass.symbols.symbolList[symbolIndex];
                    }

                    isAccessible = true;
                }

                if (hasThisSymbol)
                {
                    return true;
                }
            }
            else
            {
                if (functionScope != Constants.kGlobalScope)
                {
                    // Aparajit: This function is found to not work well in the expression interpreter as it doesn't return the
                    // correct symbol if the same symbol exists in different contexts such as inside a function defined in a lang block,
                    // inside the lang block itself and in a function in the global scope etc.
                    // TODO: We can later consider replacing GetSymbolInFunction with GetFirstVisibleSymbol consistently in all occurrences 
                    
                    //symbol = core.GetSymbolInFunction(name, Constants.kGlobalScope, functionScope, currentCodeBlock);
                    symbol = core.GetFirstVisibleSymbol(name, Constants.kGlobalScope, functionScope, currentCodeBlock);
                    if (symbol != null)
                    {
                        isAccessible = true;
                        return true;
                    }
                }
            }

            CodeBlock searchBlock = currentCodeBlock;
            while (symbolIndex == Constants.kInvalidIndex && searchBlock != null)
            {
                symbolIndex = searchBlock.symbolTable.IndexOf(name, Constants.kGlobalScope, Constants.kGlobalScope);
                if (symbolIndex != Constants.kInvalidIndex)
                {
                    symbol = searchBlock.symbolTable.symbolList[symbolIndex];

                    bool ignoreImportedSymbols = !string.IsNullOrEmpty(symbol.ExternLib) && core.IsParsingCodeBlockNode;
                    if (ignoreImportedSymbols)
                    {
                        searchBlock = searchBlock.parent;
                        continue;
                    }
                    isAccessible = true;
                    return true;
                }
                searchBlock = searchBlock.parent;
            }

            return false;
        }

        protected bool VerifyAllocation(string name,string arrayName, int classScope, int functionScope, out ProtoCore.DSASM.SymbolNode symbol, out bool isAccessible)
        {
            int symbolIndex = Constants.kInvalidIndex;
            symbol = null;
            isAccessible = false;

            if (classScope != Constants.kGlobalScope)
            {
                if ((int)ProtoCore.PrimitiveType.kTypeVoid == classScope)
                {
                    return false;
                }
                ClassNode thisClass = core.ClassTable.ClassNodes[classScope];

                bool hasThisSymbol;
                AddressType addressType;
                symbolIndex = ClassUtils.GetSymbolIndex(thisClass, name, classScope, functionScope, codeBlock.codeBlockId, core.CompleteCodeBlockList, out hasThisSymbol, out addressType);

                if (Constants.kInvalidIndex != symbolIndex)
                {
                    // It is static member, then get node from code block
                    if (AddressType.StaticMemVarIndex == addressType)
                    {
                        symbol = core.CodeBlockList[0].symbolTable.symbolList[symbolIndex];
                    }
                    else
                    {
                        symbol = thisClass.symbols.symbolList[symbolIndex];
                    }

                    isAccessible = true;
                }

                if (hasThisSymbol)
                {
                    if (symbol != null)
                    {
                        symbol.forArrayName = arrayName;
                    }
                    return true;
                }
                else
                {
                    symbolIndex = codeBlock.symbolTable.IndexOf(name, Constants.kGlobalScope, Constants.kGlobalScope);
                    if (symbolIndex != Constants.kInvalidIndex)
                    {
                        symbol = codeBlock.symbolTable.symbolList[symbolIndex];
                        isAccessible = true;
                        if (symbol != null)
                        {
                            symbol.forArrayName = arrayName;
                        }
                        return true;
                    }
                }
            }
            else
            {
                if (functionScope != Constants.kGlobalScope)
                {
                    symbol = core.GetSymbolInFunction(name, Constants.kGlobalScope, functionScope, codeBlock);
                    if (symbol != null)
                    {
                        isAccessible = true;
                         symbol.forArrayName = arrayName;
                        return true;
                    }
                }

                CodeBlock searchBlock = codeBlock;
                while (symbolIndex == Constants.kInvalidIndex && searchBlock != null)
                {
                    symbolIndex = searchBlock.symbolTable.IndexOf(name, Constants.kGlobalScope, Constants.kGlobalScope);
                    if (symbolIndex != Constants.kInvalidIndex)
                    {
                        symbol = searchBlock.symbolTable.symbolList[symbolIndex];

                        bool ignoreImportedSymbols = !string.IsNullOrEmpty(symbol.ExternLib) && core.IsParsingCodeBlockNode;
                        if (ignoreImportedSymbols)
                        {
                            searchBlock = searchBlock.parent;
                            continue;
                        }

                        isAccessible = true;
                        if (symbol != null)
                        {
                            symbol.forArrayName = arrayName;
                        }
                        return true;
                    }
                    searchBlock = searchBlock.parent;
                }

                //Fix IDE-448
                //Search current running block as well.
                searchBlock = ProtoCore.Utils.CoreUtils.GetCodeBlock(core.CodeBlockList, 0);
                symbolIndex = searchBlock.symbolTable.IndexOf(name, Constants.kGlobalScope, Constants.kGlobalScope);
                if (symbolIndex != Constants.kInvalidIndex)
                {
                    symbol = searchBlock.symbolTable.symbolList[symbolIndex];

                    if (symbol != null)
                    {
                        symbol.forArrayName = arrayName;
                    }

                    bool ignoreImportedSymbols = !string.IsNullOrEmpty(symbol.ExternLib) && core.IsParsingCodeBlockNode;
                    if (ignoreImportedSymbols)
                    {
                        return false;
                    }

                    isAccessible = true;                    
                    return true;
                }
            }
            if (symbol != null)
            {
                symbol.forArrayName = arrayName;
            }
            return false;
        }

        protected bool IsProperty(string name)
        {
            if (globalClassIndex == ProtoCore.DSASM.Constants.kInvalidIndex)
            {
                return false;
            }

            bool hasThisSymbol;
            ProtoCore.DSASM.AddressType addressType;
            ProtoCore.DSASM.ClassNode classnode = core.ClassTable.ClassNodes[globalClassIndex];

            int symbolIndex = ClassUtils.GetSymbolIndex(classnode, name, globalClassIndex, globalProcIndex, 0, core.CompleteCodeBlockList, out hasThisSymbol, out addressType);
            if (symbolIndex == ProtoCore.DSASM.Constants.kInvalidIndex)
            {
                return false;
            }

            return (classnode.symbols.symbolList[symbolIndex].functionIndex == ProtoCore.DSASM.Constants.kGlobalScope);
        }
#endif

        protected void Backpatch(int bp, int pc)
        {
            if (ProtoCore.DSASM.OpCode.JMP == codeBlock.instrStream.instrList[bp].opCode
                && codeBlock.instrStream.instrList[bp].op1.IsLabelIndex)
            {
                Validity.Assert(ProtoCore.DSASM.Constants.kInvalidIndex == codeBlock.instrStream.instrList[bp].op1.opdata);
                codeBlock.instrStream.instrList[bp].op1.opdata = pc;
            }
            else if (ProtoCore.DSASM.OpCode.CJMP == codeBlock.instrStream.instrList[bp].opCode
                && codeBlock.instrStream.instrList[bp].op1.IsLabelIndex)
            {
                Validity.Assert(ProtoCore.DSASM.Constants.kInvalidIndex == codeBlock.instrStream.instrList[bp].op1.opdata);
                codeBlock.instrStream.instrList[bp].op1.opdata = pc;
            }
        }

        protected void Backpatch(List<BackpatchNode> table, int pc)
        {
            foreach (BackpatchNode node in table)
            {
                Backpatch(node.bp, pc);
            }
        }

        public abstract ProtoCore.DSASM.ProcedureNode TraverseFunctionCall(Node node, Node parentNode, int lefttype, int depth, ref ProtoCore.Type inferedType, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, ProtoCore.AST.Node bnode = null);

        protected void EmitBounceIntrinsic(int blockId, int entry)
        {
            SetEntry();
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.BOUNCE;
            instr.op1 = StackValue.BuildBlockIndex(blockId);
            instr.op2 = StackValue.BuildInt(entry);

            ++pc;
            AppendInstruction(instr);
        }


        protected void EmitCall(int fi, 
            int blockId,
            int type, 
            int line = ProtoCore.DSASM.Constants.kInvalidIndex, 
            int col = ProtoCore.DSASM.Constants.kInvalidIndex, 
            int endLine = ProtoCore.DSASM.Constants.kInvalidIndex, 
            int endCol = ProtoCore.DSASM.Constants.kInvalidIndex,
            int entrypoint = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            SetEntry();
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.CALLR;
            instr.op1 = StackValue.BuildFunctionIndex(fi); 
            instr.op2 = StackValue.BuildClassIndex(type);
            instr.op3 = StackValue.BuildBlockIndex(blockId);

            ++pc;
            instr.debug = GetDebugObject(line, col, endLine, endCol, entrypoint);
            AppendInstruction(instr);
        }

        protected void EmitCallBaseCtor(int fi, int ci, int offset)
        {
            SetEntry();
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.CALL;
            instr.op1 = StackValue.BuildFunctionIndex(fi); 
            instr.op2 = StackValue.BuildClassIndex(ci);
            instr.op3 = StackValue.BuildInt(offset);

            ++pc;
            AppendInstruction(instr);
        }

        protected void EmitCallSetter(int fi, int ci, 
            int line = ProtoCore.DSASM.Constants.kInvalidIndex, 
            int col = ProtoCore.DSASM.Constants.kInvalidIndex,
            int endLine = ProtoCore.DSASM.Constants.kInvalidIndex, 
            int endCol = ProtoCore.DSASM.Constants.kInvalidIndex,
            int entrypoint = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            SetEntry();
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.CALL;
            instr.op1 = StackValue.BuildFunctionIndex(fi);
            instr.op2 = StackValue.BuildClassIndex(ci);
            instr.op3 = StackValue.BuildInt(0); 

            ++pc;
            instr.debug = GetDebugObject(line, col, endLine, endCol, entrypoint);
            AppendInstruction(instr, line, col);
        }

        protected void EmitDynamicCall(int functionIndex, int type, 
            int line = ProtoCore.DSASM.Constants.kInvalidIndex, int col = ProtoCore.DSASM.Constants.kInvalidIndex, 
            int endline = ProtoCore.DSASM.Constants.kInvalidIndex, int endcol = ProtoCore.DSASM.Constants.kInvalidIndex, 
            int entrypoint = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            SetEntry();
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.CALLR;
            instr.op1 = StackValue.BuildDynamic(functionIndex);
            instr.op2 = StackValue.BuildClassIndex(type);

            ++pc;
            instr.debug = GetDebugObject(line, col, endline, endcol, entrypoint);
            AppendInstruction(instr, line, col);
        }

        protected void EmitJmp(int L1, int line = ProtoCore.DSASM.Constants.kInvalidIndex, int col = ProtoCore.DSASM.Constants.kInvalidIndex,
            int eline = ProtoCore.DSASM.Constants.kInvalidIndex, int ecol = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            EmitInstrConsole(ProtoCore.DSASM.kw.jmp, " L1(" + L1 + ")");

            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.JMP;
            instr.op1 = StackValue.BuildLabelIndex(L1);

            ++pc;
            instr.debug = GetDebugObject(line, col, eline, ecol, L1);
            AppendInstruction(instr, line, col);
        }

        protected void EmitCJmp(int label, int line = ProtoCore.DSASM.Constants.kInvalidIndex, int col = ProtoCore.DSASM.Constants.kInvalidIndex, 
            int eline = ProtoCore.DSASM.Constants.kInvalidIndex, int ecol = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            EmitInstrConsole(ProtoCore.DSASM.kw.cjmp, " L(" + label + ")");

            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.CJMP;
            instr.op1 = StackValue.BuildLabelIndex(label);

            ++pc;
            if (core.DebuggerProperties.breakOptions.HasFlag(DebugProperties.BreakpointOptions.EmitInlineConditionalBreakpoint))
            {
                instr.debug = null;
            }
            else
                instr.debug = GetDebugObject(line, col, eline, ecol, label);

            AppendInstruction(instr, line, col);
        }

        protected void EmitPopArray(int size)
        {
            SetEntry();
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.ALLOCA;

            instr.op1 = StackValue.BuildInt(size);
            instr.op2 = StackValue.BuildArrayPointer(0);

            ++pc;
            AppendInstruction(instr);
        }


        protected void EmitPopGuide()
        {
            SetEntry();
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.POPG;

            ++pc;
            AppendInstruction(instr);
        }

        protected void EmitPushArrayIndex(int dimCount)
        {
            SetEntry();
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.PUSHINDEX;
            instr.op1 = StackValue.BuildArrayDimension(dimCount);

            ++pc;
            AppendInstruction(instr);
        }

        protected void EmitPushReplicationGuide(int replicationGuide)
        {
            SetEntry();
            
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.PUSHINDEX;
            instr.op1 = StackValue.BuildReplicationGuide(replicationGuide);

            ++pc;
            AppendInstruction(instr);
        }

        protected void EmitPopString(int size)
        {
            SetEntry();
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.ALLOCA;

            instr.op1 = StackValue.BuildInt(size);
            instr.op2 = StackValue.BuildString(0);
            // Push default replication guide.  
            if (core.Options.TempReplicationGuideEmptyFlag && emitReplicationGuide)
            {
                instr.op3 = StackValue.BuildReplicationGuide(0);
            }

            ++pc;
            AppendInstruction(instr);
        }

        protected void EmitPop(StackValue op, 
            int classIndex, 
            int blockId = Constants.kInvalidIndex, 
            int line = ProtoCore.DSASM.Constants.kInvalidIndex, int col = ProtoCore.DSASM.Constants.kInvalidIndex,
            int eline = ProtoCore.DSASM.Constants.kInvalidIndex, int ecol = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.POP;
            instr.op1 = op;
            instr.op2 = StackValue.BuildClassIndex(classIndex);
            instr.op3 = StackValue.BuildBlockIndex(blockId);

            // For debugging, assert here but these should raise runtime errors in the VM
            Validity.Assert(op.IsVariableIndex || op.IsMemberVariableIndex || op.IsRegister);

            ++pc;
            instr.debug = GetDebugObject(line, col, eline, ecol, pc);
            AppendInstruction(instr, line, col);
        }

        protected void EmitPopForSymbol(SymbolNode symbol,
            int blockId,
            int line = ProtoCore.DSASM.Constants.kInvalidIndex, 
            int col = ProtoCore.DSASM.Constants.kInvalidIndex,
            int eline = ProtoCore.DSASM.Constants.kInvalidIndex, 
            int ecol = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            Validity.Assert(symbol != null);
            if (symbol == null)
            {
                return;
            }

            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.POP;
            instr.op1 = BuildOperand(symbol);
            instr.op2 = StackValue.BuildClassIndex(symbol.classScope);
            instr.op3 = StackValue.BuildBlockIndex(blockId);

            ++pc;

            bool outputBreakpoint = false;
            DebugProperties.BreakpointOptions options = core.DebuggerProperties.breakOptions;
            if (options.HasFlag(DebugProperties.BreakpointOptions.EmitPopForTempBreakpoint))
                outputBreakpoint = true;

            // Do not emit breakpoints for null or var type declarations
            if (!core.DebuggerProperties.breakOptions.HasFlag(DebugProperties.BreakpointOptions.SuppressNullVarDeclarationBreakpoint))
            {
                // Don't need no pop for temp (unless caller demands it).
                if (outputBreakpoint || !symbol.name.StartsWith("%"))
                    instr.debug = GetDebugObject(line, col, eline, ecol, pc);
            }
            AppendInstruction(instr, line, col);
        }

        protected void EmitPopForSymbolW(SymbolNode symbol,
            int blockId,
            int line = ProtoCore.DSASM.Constants.kInvalidIndex,
            int col = ProtoCore.DSASM.Constants.kInvalidIndex,
            int eline = ProtoCore.DSASM.Constants.kInvalidIndex,
            int ecol = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            Validity.Assert(symbol != null);
            if (symbol == null)
            {
                return;
            }

            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.POPW;
            instr.op1 = BuildOperand(symbol);
            instr.op2 = StackValue.BuildClassIndex(symbol.classScope);
            instr.op3 = StackValue.BuildBlockIndex(blockId);

            ++pc;

            AppendInstruction(instr);
        }

        // TODO Jun: Merge EmitPopList with the associative version and implement in a codegen utils file
        protected void EmitPopList(int depth, int startscope, int block, 
            int line = ProtoCore.DSASM.Constants.kInvalidIndex, int col = ProtoCore.DSASM.Constants.kInvalidIndex,
            int endline = ProtoCore.DSASM.Constants.kInvalidIndex, int endcol = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.POPLIST;
            instr.op1 = StackValue.BuildInt(depth);
            instr.op2 = StackValue.BuildInt(startscope);
            instr.op3 = StackValue.BuildBlockIndex(block);

            ++pc;
            instr.debug = GetDebugObject(line, col, endline, endcol, pc);
            AppendInstruction(instr, line, col);
        }

        protected void EmitPushBlockID(int blockID)
        {
            EmitInstrConsole(ProtoCore.DSASM.kw.pushb, blockID.ToString());
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.PUSHB;
            instr.op1 = StackValue.BuildInt(blockID);

            ++pc;
            AppendInstruction(instr);
        }

        protected void EmitPushList(int depth, int startscope, int block, bool fromDotCall = false)
        {
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.PUSHLIST;
            instr.op1 = fromDotCall ? StackValue.BuildDynamic(depth) : StackValue.BuildInt(depth);
            instr.op2 = StackValue.BuildClassIndex(startscope);
            instr.op3 = StackValue.BuildBlockIndex(block);

            ++pc;
            AppendInstruction(instr);
        }

        protected void EmitPush(StackValue op, 
            int blockId = 0, 
            int line = ProtoCore.DSASM.Constants.kInvalidIndex, 
            int col = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            SetEntry();
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.PUSH;
            instr.op1 = op;
            instr.op2 = StackValue.BuildClassIndex(globalClassIndex);
            instr.op3 = StackValue.BuildBlockIndex(blockId);

            ++pc;
            AppendInstruction(instr, line, col);
        }

        protected void EmitPushG(StackValue op, int rank = 0, int line = ProtoCore.DSASM.Constants.kInvalidIndex, int col = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            SetEntry();
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.PUSHG;
            instr.op1 = op;
            instr.op2 = StackValue.BuildClassIndex(globalClassIndex);
            instr.op3 = StackValue.BuildArrayDimension(rank);

            ++pc;
            AppendInstruction(instr, line, col);
        }


        protected void EmitPushType(int UID, int rank)
        {
            SetEntry();
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.PUSH;
            instr.op1 = StackValue.BuildStaticType(UID, rank);
            instr.op2 = StackValue.BuildClassIndex(globalClassIndex);

            ++pc;
            AppendInstruction(instr);
        }

        private void AppendInstruction(Instruction instr, int line = Constants.kInvalidIndex, int col = Constants.kInvalidIndex)
        {
            if (DSASM.InterpreterMode.kExpressionInterpreter == core.Options.RunMode)
            {
                core.ExprInterpreterExe.iStreamCanvas.instrList.Add(instr);
            }
            else if(!core.IsParsingCodeBlockNode && !core.IsParsingPreloadedAssembly)
            {
                codeBlock.instrStream.instrList.Add(instr);

                if(line > 0 && col > 0)
                    updatePcDictionary(line, col);
            }
        }

        protected void EmitPushForSymbol(SymbolNode symbol, int blockId, ProtoCore.AST.Node identNode)
        {
            SetEntry();
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.PUSH;
            instr.op1 = BuildOperand(symbol);
            instr.op2 = StackValue.BuildClassIndex(symbol.classScope);
            instr.op3 = StackValue.BuildBlockIndex(blockId);

            ++pc;

            DebugProperties.BreakpointOptions options = core.DebuggerProperties.breakOptions;
            if (options.HasFlag(DebugProperties.BreakpointOptions.EmitIdentifierBreakpoint))
            {
                instr.debug = GetDebugObject(identNode.line, identNode.col,
                    identNode.endLine, identNode.endCol, pc);
            }

            AppendInstruction(instr, identNode.line, identNode.col);
        }

        protected void EmitPushForSymbolW(SymbolNode symbol, int blockId, int line = ProtoCore.DSASM.Constants.kInvalidIndex, int col = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            SetEntry();
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.PUSHW;
            instr.op1 = BuildOperand(symbol);
            instr.op2 = StackValue.BuildClassIndex(symbol.classScope);
            instr.op3 = StackValue.BuildBlockIndex(blockId);

            ++pc;
            //instr.debug = GetDebugObject(line, col, pc);
            AppendInstruction(instr);
        }


        protected void EmitPushForSymbolGuide(SymbolNode symbol, int line = ProtoCore.DSASM.Constants.kInvalidIndex, int col = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            SetEntry();
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.PUSHG;
            instr.op1 = BuildOperand(symbol);
            instr.op2 = StackValue.BuildClassIndex(symbol.classScope);

            ++pc;
            //instr.debug = GetDebugObject(line, col, pc);
            AppendInstruction(instr);
        }
        
        protected void EmitPushm(StackValue op, int classIndex, int blockId, int line = ProtoCore.DSASM.Constants.kInvalidIndex, int col = ProtoCore.DSASM.Constants.kInvalidIndex,
            int eline = ProtoCore.DSASM.Constants.kInvalidIndex, int ecol = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            SetEntry();
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.PUSHM;
            instr.op1 = op;
            instr.op2 = StackValue.BuildClassIndex(classIndex);
            instr.op3 = StackValue.BuildBlockIndex(blockId);

            ++pc;
            instr.debug = GetDebugObject(line, col, eline, ecol, pc);
            AppendInstruction(instr, line, col);
        }

        protected void EmitPopm(StackValue op, int blockId, int line = ProtoCore.DSASM.Constants.kInvalidIndex, int col = ProtoCore.DSASM.Constants.kInvalidIndex,
            int endline = ProtoCore.DSASM.Constants.kInvalidIndex, int endcol = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            SetEntry();
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.POPM;
            instr.op1 = op;
            instr.op2 = StackValue.BuildBlockIndex(blockId);

            ++pc;
            if (emitDebugInfo)
            {
                instr.debug = GetDebugObject(line, col, endline, endcol, pc);
            }
            AppendInstruction(instr, line, col);
        }

        protected void EmitPushNull()
        {
            EmitInstrConsole(ProtoCore.DSASM.kw.push, ProtoCore.DSASM.Literal.Null);
            EmitPush(StackValue.Null);
        }

        protected void EmitMov(StackValue opDest, StackValue opSrc)
        {
            SetEntry();
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.MOV;
            instr.op1 = opDest;
            instr.op2 = StackValue.BuildClassIndex(globalClassIndex);

            ++pc;
            AppendInstruction(instr);
        }

        protected abstract void EmitRetb(int line = ProtoCore.DSASM.Constants.kInvalidIndex, int col = ProtoCore.DSASM.Constants.kInvalidIndex,
             int endline = ProtoCore.DSASM.Constants.kInvalidIndex, int endcol = ProtoCore.DSASM.Constants.kInvalidIndex);
			
		protected abstract void EmitRetcn(int blockId = Constants.kInvalidIndex, int line = ProtoCore.DSASM.Constants.kInvalidIndex, int col = ProtoCore.DSASM.Constants.kInvalidIndex,
             int endline = ProtoCore.DSASM.Constants.kInvalidIndex, int endcol = ProtoCore.DSASM.Constants.kInvalidIndex);
        
        protected abstract void EmitReturn(int line = ProtoCore.DSASM.Constants.kInvalidIndex, int col = ProtoCore.DSASM.Constants.kInvalidIndex,
             int endline = ProtoCore.DSASM.Constants.kInvalidIndex, int endcol = ProtoCore.DSASM.Constants.kInvalidIndex);

        protected void EmitReturnToRegister(int line = ProtoCore.DSASM.Constants.kInvalidIndex, int col = ProtoCore.DSASM.Constants.kInvalidIndex,
            int endline = ProtoCore.DSASM.Constants.kInvalidIndex, int endcol = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            // Pop to the rx register
            StackValue op = StackValue.BuildRegister(Registers.RX);
            EmitInstrConsole(ProtoCore.DSASM.kw.pop, ProtoCore.DSASM.kw.regRX);
            EmitPop(op, Constants.kGlobalScope, Constants.kInvalidIndex, line, col, endline, endcol);

            // Emit the ret instruction only if this is a function we are returning from
            if (core.IsFunctionCodeBlock(codeBlock))
            {
                // Emit the return isntruction to terminate the function
                EmitInstrConsole(ProtoCore.DSASM.kw.ret);
                EmitReturn(line, col, endline, endcol);
            }
            else
            {
                EmitInstrConsole(ProtoCore.DSASM.kw.retb);
                EmitRetb( line, col, endline, endcol);
            }
        }

        protected void EmitBinary(
            ProtoCore.DSASM.OpCode opcode, 
            int line = ProtoCore.DSASM.Constants.kInvalidIndex, 
            int col = ProtoCore.DSASM.Constants.kInvalidIndex,
            int eline = ProtoCore.DSASM.Constants.kInvalidIndex, 
            int ecol = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            SetEntry();
            Instruction instr = new Instruction();
            instr.opCode = opcode;

            ++pc;
            instr.debug = GetDebugObject(line, col, eline, ecol, pc);
            AppendInstruction(instr, line, col);
        }

        protected void EmitUnary(ProtoCore.DSASM.OpCode opcode, int line = ProtoCore.DSASM.Constants.kInvalidIndex, int col = ProtoCore.DSASM.Constants.kInvalidIndex,
            int eline = ProtoCore.DSASM.Constants.kInvalidIndex, int ecol = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            SetEntry();
            Instruction instr = new Instruction();
            instr.opCode = opcode;

            ++pc;
            instr.debug = GetDebugObject(line, col, eline, ecol, pc);
            AppendInstruction(instr, line, col);
        }

        protected void EmitIntNode(Node node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone)
        {
            if (subPass == ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier) 
            {
                return;
            }

            Int64 value;
            if (node is AST.ImperativeAST.IntNode)
            {
                value = (node as AST.ImperativeAST.IntNode).Value;
            }
            else if (node is AST.AssociativeAST.IntNode)
            {
                value = (node as AST.AssociativeAST.IntNode).Value;
            }
            else
            {
                throw new InvalidDataException("The input node is not a IntNode");
            }

            if (!enforceTypeCheck || core.TypeSystem.IsHigherRank((int)PrimitiveType.kTypeInt, inferedType.UID))
            {
                inferedType.UID = (int)PrimitiveType.kTypeInt;
            }
            
            inferedType.UID = isBooleanOp ? (int)PrimitiveType.kTypeBool : inferedType.UID;


            if (core.Options.TempReplicationGuideEmptyFlag)
            {
                if (emitReplicationGuide)
                {
                    int replicationGuides = 0;
                    
                    // Push the number of guides
                    EmitInstrConsole(ProtoCore.DSASM.kw.push, replicationGuides + "[guide]");
                    StackValue opNumGuides = StackValue.BuildReplicationGuide(replicationGuides);
                    EmitPush(opNumGuides);
                }
            }


            StackValue op = StackValue.BuildInt(value);
            if (core.Options.TempReplicationGuideEmptyFlag && emitReplicationGuide)
            {
                EmitInstrConsole(ProtoCore.DSASM.kw.pushg, value.ToString());
                EmitPushG(op, node.line, node.col);
            }
            else
            {
                EmitInstrConsole(ProtoCore.DSASM.kw.push, value.ToString());
                EmitPush(op, node.line, node.col);
            }

            if (IsAssociativeArrayIndexing)
            {
                if (null != graphNode)
                {
                    // Get the last dependent which is the current identifier being indexed into
                    SymbolNode literalSymbol = new SymbolNode();
                    literalSymbol.name = value.ToString();

                    AssociativeGraph.UpdateNode intNode = new AssociativeGraph.UpdateNode();
                    intNode.symbol = literalSymbol;
                    intNode.nodeType = AssociativeGraph.UpdateNodeType.kLiteral;

                    if (graphNode.isIndexingLHS)
                    {
                        graphNode.dimensionNodeList.Add(intNode);
                    }
                    else
                    {
                        int lastDependentIndex = graphNode.dependentList.Count - 1;
                        if (lastDependentIndex >= 0)
                        {
                            ProtoCore.AssociativeGraph.UpdateNode currentDependentNode = graphNode.dependentList[lastDependentIndex].updateNodeRefList[0].nodeList[0];
                            currentDependentNode.dimensionNodeList.Add(intNode);

                            if (core.Options.GenerateSSA)
                            {
                                if (null != firstSSAGraphNode)
                                {
                                    lastDependentIndex = firstSSAGraphNode.dependentList.Count - 1;
                                    if (lastDependentIndex >= 0)
                                    {
                                        ProtoCore.AssociativeGraph.UpdateNode firstSSAUpdateNode = firstSSAGraphNode.dependentList[lastDependentIndex].updateNodeRefList[0].nodeList[0];
                                        firstSSAUpdateNode.dimensionNodeList.Add(intNode);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        protected void EmitCharNode(Node node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone)
        {
            if (subPass == ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier)
            {
                return;
            }

            dynamic cNode = node;
            if (!enforceTypeCheck || core.TypeSystem.IsHigherRank((int)PrimitiveType.kTypeChar, inferedType.UID))
            {
                inferedType.UID = (int)PrimitiveType.kTypeChar;
            }
            inferedType.UID = isBooleanOp ? (int)PrimitiveType.kTypeBool : inferedType.UID;

            Byte[] utf8bytes = ProtoCore.Utils.EncodingUtils.UTF8StringToUTF8Bytes((String)cNode.value);
            String value = Encoding.UTF8.GetString(utf8bytes);
            if (value.Length > 1)
            {
                buildStatus.LogSyntaxError(Resources.TooManyCharacters, null, node.line, node.col);
            }
  
            String strValue = "'" + value + "'";
            StackValue op = ProtoCore.DSASM.StackValue.BuildChar(value[0]);

            if (core.Options.TempReplicationGuideEmptyFlag && emitReplicationGuide)
            {
                int replicationGuides = 0;
                EmitInstrConsole(ProtoCore.DSASM.kw.push, replicationGuides + "[guide]");
                StackValue opNumGuides = StackValue.BuildReplicationGuide(replicationGuides);
                EmitPush(opNumGuides);

                EmitInstrConsole(ProtoCore.DSASM.kw.pushg, strValue);
                EmitPushG(op, node.line, node.col);
            }
            else
            {
                EmitInstrConsole(ProtoCore.DSASM.kw.push, strValue);
                EmitPush(op, cNode.line, cNode.col);
            }
        }
       
        protected void EmitStringNode(
            Node node, 
            ref Type inferedType, 
            AssociativeGraph.GraphNode graphNode = null,
            ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone)
        {
            if (subPass == ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier)
            {
                return;
            }

            dynamic sNode = node;
            if (!enforceTypeCheck || core.TypeSystem.IsHigherRank((int)PrimitiveType.kTypeString, inferedType.UID))
            {
                inferedType.UID = (int)PrimitiveType.kTypeString;
            }

            if (core.Options.TempReplicationGuideEmptyFlag && emitReplicationGuide)
            {
                EmitInstrConsole(ProtoCore.DSASM.kw.push, 0 + "[guide]");
                StackValue opNumGuides = StackValue.BuildReplicationGuide(0);
                EmitPush(opNumGuides);
            }

            string value = (string)sNode.value;
            StackValue svString = core.Heap.AllocateFixedString(value);
            if (core.Options.TempReplicationGuideEmptyFlag && emitReplicationGuide)
            {
                EmitInstrConsole(kw.pushg, "\"" + value + "\"");
                EmitPushG(svString, node.line, node.col);
            }
            else
            {
                EmitInstrConsole(kw.push, "\"" + value + "\"");
                EmitPush(svString, node.line, node.col);
            }

            if (IsAssociativeArrayIndexing && graphNode != null && graphNode.isIndexingLHS)
            {
                SymbolNode literalSymbol = new SymbolNode();
                literalSymbol.name = value;

                var dimNode = new AssociativeGraph.UpdateNode();
                dimNode.symbol = literalSymbol;
                dimNode.nodeType = AssociativeGraph.UpdateNodeType.kLiteral;

                graphNode.dimensionNodeList.Add(dimNode);
            }
        }
        
        protected void EmitDoubleNode(Node node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone)
        {
            if (subPass == ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier)
            {
                return;
            }

            double value;
            if (node is AST.ImperativeAST.DoubleNode)
            {
                value = (node as AST.ImperativeAST.DoubleNode).Value;
            }
            else if (node is AST.AssociativeAST.DoubleNode)
            {
                value = (node as AST.AssociativeAST.DoubleNode).Value;
            }
            else
            {
                throw new InvalidDataException("The input node is not DoubleNode");
            }

            if (!enforceTypeCheck || core.TypeSystem.IsHigherRank((int)PrimitiveType.kTypeDouble, inferedType.UID))
            {
                inferedType.UID = (int)PrimitiveType.kTypeDouble;
            }
            inferedType.UID = isBooleanOp ? (int)PrimitiveType.kTypeBool : inferedType.UID;

            if (core.Options.TempReplicationGuideEmptyFlag)
            {
                if (emitReplicationGuide)
                {
                    int replicationGuides = 0;

                    // Push the number of guides
                    EmitInstrConsole(ProtoCore.DSASM.kw.push, replicationGuides + "[guide]");
                    StackValue opNumGuides = StackValue.BuildReplicationGuide(replicationGuides);
                    EmitPush(opNumGuides);
                }
            }

            StackValue op = StackValue.BuildDouble(value);
            if (core.Options.TempReplicationGuideEmptyFlag && emitReplicationGuide)
            {
                EmitInstrConsole(ProtoCore.DSASM.kw.pushg, value.ToString());
                EmitPushG(op, node.line, node.col);
            }
            else
            {
                EmitInstrConsole(ProtoCore.DSASM.kw.push, value.ToString());
                EmitPush(op, node.line, node.col);
            }

            if (IsAssociativeArrayIndexing)
            {
                if (null != graphNode)
                {
                    // Get the last dependent which is the current identifier being indexed into
                    SymbolNode literalSymbol = new SymbolNode();
                    literalSymbol.name = value.ToString();

                    AssociativeGraph.UpdateNode intNode = new AssociativeGraph.UpdateNode();
                    intNode.symbol = literalSymbol;
                    intNode.nodeType = AssociativeGraph.UpdateNodeType.kLiteral;

                    if (graphNode.isIndexingLHS)
                    {
                        graphNode.dimensionNodeList.Add(intNode);
                    }
                    else
                    {
                        int lastDependentIndex = graphNode.dependentList.Count - 1;
                        ProtoCore.AssociativeGraph.UpdateNode currentDependentNode = graphNode.dependentList[lastDependentIndex].updateNodeRefList[0].nodeList[0];
                        currentDependentNode.dimensionNodeList.Add(intNode);

                        if (core.Options.GenerateSSA)
                        {
                            if (null != firstSSAGraphNode)
                            {
                                lastDependentIndex = firstSSAGraphNode.dependentList.Count - 1;
                                ProtoCore.AssociativeGraph.UpdateNode firstSSAUpdateNode = firstSSAGraphNode.dependentList[lastDependentIndex].updateNodeRefList[0].nodeList[0];
                                firstSSAUpdateNode.dimensionNodeList.Add(intNode);
                            }
                        }
                    }
                }
            }
        }

        protected void EmitBooleanNode(Node node, ref ProtoCore.Type inferedType, ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone)
        {
            if (subPass == ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier)
            {
                return;
            }

            bool value;
            if (node is AST.ImperativeAST.BooleanNode)
            {
                value = (node as AST.ImperativeAST.BooleanNode).Value;
            }
            else if (node is AST.AssociativeAST.BooleanNode)
            {
                value = (node as AST.AssociativeAST.BooleanNode).Value;
            }
            else
            {
                throw new InvalidDataException("The input node is not a BooleanNode");
            }

            // We need to get inferedType for boolean variable so that we can perform type check
            if (enforceTypeCheck || core.TypeSystem.IsHigherRank((int)PrimitiveType.kTypeBool, inferedType.UID))
            {
                inferedType.UID = (int)PrimitiveType.kTypeBool;
            }

            if (core.Options.TempReplicationGuideEmptyFlag)
            {
                if (emitReplicationGuide)
                {
                    int replicationGuides = 0;

                    // Push the number of guides
                    EmitInstrConsole(ProtoCore.DSASM.kw.push, replicationGuides + "[guide]");
                    StackValue opNumGuides = StackValue.BuildReplicationGuide(replicationGuides);
                    EmitPush(opNumGuides);
                }
            }

            StackValue op = StackValue.BuildBoolean(value);

            if (core.Options.TempReplicationGuideEmptyFlag && emitReplicationGuide)
            {
                EmitInstrConsole(ProtoCore.DSASM.kw.pushg, value.ToString());
                EmitPushG(op, node.line, node.col);
            }
            else
            {
                EmitInstrConsole(ProtoCore.DSASM.kw.push, value.ToString());
                EmitPush(op, node.line, node.col);
            }
        }

        protected void EmitNullNode(Node node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone)
        {
            if (subPass == ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier)
            {
                return;
            }

            dynamic nullNode = node;
            inferedType.UID = (int)PrimitiveType.kTypeNull;

            inferedType.UID = isBooleanOp ? (int)PrimitiveType.kTypeBool : inferedType.UID;

            if (core.Options.TempReplicationGuideEmptyFlag)
            {
                if (emitReplicationGuide)
                {
                    int replicationGuides = 0;

                    // Push the number of guides
                    EmitInstrConsole(ProtoCore.DSASM.kw.push, replicationGuides + "[guide]");
                    StackValue opNumGuides = StackValue.BuildReplicationGuide(replicationGuides);
                    EmitPush(opNumGuides);
                }
            }


            StackValue op = StackValue.Null;

            if (core.Options.TempReplicationGuideEmptyFlag && emitReplicationGuide)
            {
                EmitInstrConsole(ProtoCore.DSASM.kw.pushg, ProtoCore.DSASM.Literal.Null);
                EmitPushG(op, nullNode.line, nullNode.col);
            }
            else
            {
                EmitInstrConsole(ProtoCore.DSASM.kw.push, ProtoCore.DSASM.Literal.Null);
                EmitPush(op, nullNode.line, nullNode.col);
            }
        }

        protected int EmitReplicationGuides(List<ProtoCore.AST.AssociativeAST.AssociativeNode> replicationGuidesList, bool emitNumber = false)
        {
            int replicationGuides = 0;
            if (null != replicationGuidesList && replicationGuidesList.Count > 0)
            {
                replicationGuides = replicationGuidesList.Count;
                for (int n = 0; n < replicationGuides; ++n)
                {
                    Validity.Assert(replicationGuidesList[n] is ProtoCore.AST.AssociativeAST.ReplicationGuideNode);
                    ProtoCore.AST.AssociativeAST.ReplicationGuideNode repGuideNode = replicationGuidesList[n] as ProtoCore.AST.AssociativeAST.ReplicationGuideNode;

                    Validity.Assert(repGuideNode.RepGuide is ProtoCore.AST.AssociativeAST.IdentifierNode);
                    ProtoCore.AST.AssociativeAST.IdentifierNode nodeGuide = repGuideNode.RepGuide as ProtoCore.AST.AssociativeAST.IdentifierNode;

                    // Emit the repguide
                    EmitInstrConsole(ProtoCore.DSASM.kw.push, nodeGuide.Value);
                    StackValue opguide = StackValue.BuildInt(System.Convert.ToInt64(nodeGuide.Value));
                    EmitPush(opguide);

                    // Emit the rep guide property
                    EmitInstrConsole(ProtoCore.DSASM.kw.push, repGuideNode.IsLongest.ToString());
                    StackValue opGuideProperty = StackValue.BuildBoolean(repGuideNode.IsLongest);
                    EmitPush(opGuideProperty);
                }

                if (emitNumber)
                {
                    EmitInstrConsole(ProtoCore.DSASM.kw.push, replicationGuides + "[guide]");
                    StackValue opNumGuides = StackValue.BuildReplicationGuide(replicationGuides);
                    EmitPush(opNumGuides);
                }
            }
            
            return replicationGuides; 
        }


        protected void EmitExprListNode(Node node, ref ProtoCore.Type inferedType, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, ProtoCore.AST.Node parentNode = null)
        {
            dynamic exprlist = node;
            int rank = 0;

            if (subPass != ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier)
            {
                //get the rank
                dynamic ltNode = exprlist;

                bool isExprListNode = (ltNode is ProtoCore.AST.ImperativeAST.ExprListNode || ltNode is ProtoCore.AST.AssociativeAST.ExprListNode);
                bool isStringNode = (ltNode is ProtoCore.AST.ImperativeAST.StringNode || ltNode is ProtoCore.AST.AssociativeAST.StringNode);
                while ((isExprListNode && ltNode.list.Count > 0) || isStringNode)
                {
                    rank++;
                    if (isStringNode)
                        break;

                    ltNode = ltNode.list[0];
                    isExprListNode = (ltNode is ProtoCore.AST.ImperativeAST.ExprListNode || ltNode is ProtoCore.AST.AssociativeAST.ExprListNode);
                    isStringNode = (ltNode is ProtoCore.AST.ImperativeAST.StringNode || ltNode is ProtoCore.AST.AssociativeAST.StringNode);
                }
            }

            int commonType = (int)PrimitiveType.kTypeVoid;
            foreach (Node listNode in exprlist.list)
            {
                bool emitReplicationGuideFlag = emitReplicationGuide;
                emitReplicationGuide = false;

                DfsTraverse(listNode, ref inferedType, false, graphNode, subPass, parentNode);
                if ((int)PrimitiveType.kTypeVoid== commonType)
                {
                    commonType = inferedType.UID;
                }
                else 
                {
                    if (inferedType.UID != commonType)
                    {
                        commonType = (int)PrimitiveType.kTypeVar;
                    }
                }

                emitReplicationGuide = emitReplicationGuideFlag;
            }

            inferedType.UID = commonType;
            inferedType.rank = rank;

            if (subPass == ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier)
            {
                return;
            }

            EmitInstrConsole(ProtoCore.DSASM.kw.alloca, exprlist.list.Count.ToString());
            EmitPopArray(exprlist.list.Count);

            if (exprlist.ArrayDimensions != null)
            {
                int dimensions = DfsEmitArrayIndexHeap(exprlist.ArrayDimensions, graphNode);
                EmitInstrConsole(ProtoCore.DSASM.kw.pushindex, dimensions.ToString() + "[dim]");
                EmitPushArrayIndex(dimensions);
            }

            if (core.Options.TempReplicationGuideEmptyFlag && emitReplicationGuide)
            {
                if (node is ProtoCore.AST.AssociativeAST.ExprListNode)
                {
                    var exprNode = node as ProtoCore.AST.AssociativeAST.ExprListNode;
                    int guides = EmitReplicationGuides(exprNode.ReplicationGuides);
                    EmitInstrConsole(ProtoCore.DSASM.kw.pushindex, guides + "[guide]");
                    EmitPushReplicationGuide(guides);
                }
            }
        }

        protected void EmitReturnStatement(Node node, Type inferedType)
        {
            // Check the returned type against the declared return type
            if (null != localProcedure && 
                localProcedure.isConstructor &&
                core.IsFunctionCodeBlock(codeBlock))
            {
                buildStatus.LogSemanticError(Resources.ReturnStatementIsNotAllowedInConstructor, 
                                             core.CurrentDSFileName, 
                                             node.line, 
                                             node.col);
            }

            EmitReturnToRegister(node.line, node.col, node.endLine, node.endCol);
        }

        protected void EmitBinaryOperation(Type leftType, Type rightType, ProtoCore.DSASM.Operator optr)
        {
            string op = Op.GetOpName(optr);
            EmitInstrConsole(op);
            EmitBinary(Op.GetOpCode(optr));
        }


        /*
            proc EmitIdentifierListNode(identListNode, graphnode)
	            // Build the dependency given the SSA form
	            BuildSSADependency(identListNode, graphnode) 

                // Build the dependency based on the non-SSA code
	            BuildRealDependencyForIdentList(identListNode, graphnode)
            end

            proc BuildSSADependency(identListNode, graphnode)
	            // This is the current implementation
            end


            proc BuildRealDependencyForIdentList(identListNode, graphnode)
	            dependent = new graphnode
	            dependent.Push(ssaPtrList.GetAll())
                dependent.Push(identListNode.rhs)
                graphnode.PushDependent(dependent)
            end
        */


        private void BuildSSADependency(Node node, AssociativeGraph.GraphNode graphNode)
        {
            // Jun Comment: set the graphNode dependent as this identifier list
            ProtoCore.Type type = new ProtoCore.Type();
            type.UID = globalClassIndex;
            ProtoCore.AssociativeGraph.UpdateNodeRef nodeRef = new AssociativeGraph.UpdateNodeRef();
            DFSGetSymbolList(node, ref type, nodeRef);

            if (null != graphNode && nodeRef.nodeList.Count > 0)
            {
                ProtoCore.AssociativeGraph.GraphNode dependentNode = new ProtoCore.AssociativeGraph.GraphNode();
                dependentNode.updateNodeRefList.Add(nodeRef);
                graphNode.PushDependent(dependentNode);
            }
        }


        private ProtoCore.AST.AssociativeAST.IdentifierListNode BuildIdentifierList(List<ProtoCore.AST.AssociativeAST.AssociativeNode> astIdentList)
        {
            // TODO Jun: Replace this condition or handle this case prior to this call
            if (astIdentList.Count < 2)
            {
                return null;
            }

            AST.AssociativeAST.IdentifierListNode identList = null;

            // Build the first ident list
            identList = new AST.AssociativeAST.IdentifierListNode();
            identList.LeftNode = astIdentList[0];
            identList.RightNode = astIdentList[1];

            // Build the rest
            for (int n = 2; n < astIdentList.Count; ++n)
            {
                // Build a new identllist for the prev identlist
                AST.AssociativeAST.IdentifierListNode subIdentList = new AST.AssociativeAST.IdentifierListNode(identList);
                subIdentList.Optr = Operator.dot;

                // Build a new ident and assign it the prev identlist and the next identifier
                identList = new AST.AssociativeAST.IdentifierListNode();
                identList.LeftNode = subIdentList;
                identList.RightNode = astIdentList[n];
            }

            return identList;
        }

        protected void BuildRealDependencyForIdentList(AssociativeGraph.GraphNode graphNode)
        {
            if (ssaPointerStack.Count == 0)
            {
                return;
            }

            // Push all dependent pointers
            ProtoCore.AST.AssociativeAST.IdentifierListNode identList = BuildIdentifierList(ssaPointerStack.Peek());

            // Comment Jun: perhaps this can be an assert?
            if (null != identList)
            {
                ProtoCore.Type type = new ProtoCore.Type();
                type.UID = globalClassIndex;
                ProtoCore.AssociativeGraph.UpdateNodeRef nodeRef = new AssociativeGraph.UpdateNodeRef();
                int functionIndex = globalProcIndex;
                DFSGetSymbolList_Simple(identList, ref type, ref functionIndex, nodeRef);

                if (null != graphNode && nodeRef.nodeList.Count > 0)
                {
                    ProtoCore.AssociativeGraph.GraphNode dependentNode = new ProtoCore.AssociativeGraph.GraphNode();
                    dependentNode.updateNodeRefList.Add(nodeRef);
                    graphNode.PushDependent(dependentNode);

                    // If the pointerList is a setter, then it should also be in the lhs of a graphNode
                    //  Given:
                    //      a.x = 1 
                    //  Which was converted to: 
                    //      tvar = a.set_x(1)
                    //  Set a.x as lhs of the graphnode. 
                    //  This means that statement that depends on a.x can re-execute, such as:
                    //      p = a.x;
                    //
                    List<ProtoCore.AST.AssociativeAST.AssociativeNode> topList = ssaPointerStack.Peek();
                    string propertyName = topList[topList.Count - 1].Name;
                    bool isSetter = propertyName.StartsWith(Constants.kSetterPrefix);
                    if (isSetter)
                    {
                        graphNode.updateNodeRefList.Add(nodeRef);
                        graphNode.IsLHSIdentList = true;

                        AutoGenerateUpdateArgumentReference(nodeRef, graphNode);
                    }
                }
            }
        }

        protected void EmitIdentifierListNode(Node node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, 
            ProtoCore.AssociativeGraph.GraphNode graphNode = null, 
            ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, 
            ProtoCore.AST.Node bnode = null)
        {
            if (subPass == ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier)
            {
                //to process all unbounded parameters if any
                dynamic iNode = node;
                while (iNode is ProtoCore.AST.AssociativeAST.IdentifierListNode || iNode is ProtoCore.AST.ImperativeAST.IdentifierListNode)
                {
                    dynamic rightNode = iNode.RightNode;
                    if (rightNode is ProtoCore.AST.AssociativeAST.FunctionCallNode || rightNode is ProtoCore.AST.ImperativeAST.FunctionCallNode)
                    {
                        foreach (dynamic paramNode in rightNode.FormalArguments)
                        {
                            ProtoCore.Type paramType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0);
                            DfsTraverse(paramNode, ref paramType, false, graphNode, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier);
                        }
                    }
                    iNode = iNode.LeftNode;
                }
                return;
            }

            int depth = 0;

            ProtoCore.Type leftType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kInvalidType, 0);
            bool isFirstIdent = true;

            BuildSSADependency(node, graphNode);
            if (core.Options.GenerateSSA)
            {
                BuildRealDependencyForIdentList(graphNode);

                if (node is ProtoCore.AST.AssociativeAST.IdentifierListNode)
                {
                    if ((node as ProtoCore.AST.AssociativeAST.IdentifierListNode).IsLastSSAIdentListFactor)
                    {
                        Validity.Assert(null != ssaPointerStack);
                        ssaPointerStack.Pop();
                    }
                }
            }

            bool isCollapsed;
            EmitGetterSetterForIdentList(node, ref inferedType, graphNode, subPass, out isCollapsed);
            if (!isCollapsed)
            {
                bool isMethodCallPresent = false;
                ProtoCore.DSASM.SymbolNode firstSymbol = null;
                bool isIdentReference = DfsEmitIdentList(node, null, globalClassIndex, ref leftType, ref depth, ref inferedType, false, ref isFirstIdent, ref isMethodCallPresent, ref firstSymbol, graphNode, subPass, bnode);
                inferedType.UID = isBooleanOp ? (int)PrimitiveType.kTypeBool : inferedType.UID;


                if (isIdentReference && depth > 1)
                {
                    EmitInstrConsole(ProtoCore.DSASM.kw.pushlist, depth.ToString(), globalClassIndex.ToString());

                    // TODO Jun: Get blockid
                    int blockId = 0;
                    EmitPushList(depth, globalClassIndex, blockId);
                }

                
#if PROPAGATE_PROPERTY_MODIFY_VIA_METHOD_UPDATE
                if (isMethodCallPresent)
                {
                    // Comment Jun: If the first symbol is null, it is a constructor. If you see it isnt, pls tell me
                    if (null != firstSymbol)
                    {
                        StackValue op = StackValue.Null;

                        if (firstSymbol.classScope != ProtoCore.DSASM.Constants.kInvalidIndex &&
                            firstSymbol.functionIndex == ProtoCore.DSASM.Constants.kGlobalScope)
                        {
                            // Member var
                            op = firstSymbol.isStatic 
                                 ? StackValue.BuildStaticMemVarIndex(firstSymbol.symbolTableIndex)
                                 : StackValue.BuildMemVarIndex(firstSymbol.symbolTableIndex);
                        }
                        else
                        {
                            op = StackValue.BuildVarIndex(firstSymbol.symbolTableIndex);
                        }

                        EmitPushDependencyData(currentBinaryExprUID, false);
                        EmitInstrConsole(ProtoCore.DSASM.kw.dep, firstSymbol.name);
                        EmitDependency(firstSymbol.runtimeTableIndex, op, 0);
                    }
                }
#endif
            }
        }

        protected void EmitDefaultArgNode(ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone)
        {
            if (subPass == ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier)
            {
                return;
            }
            EmitInstrConsole(ProtoCore.DSASM.kw.push, "defaultArg");
            EmitPush(StackValue.BuildDefaultArgument());
        }

        protected void EmitPushVarData(int dimensions, int UID = (int)ProtoCore.PrimitiveType.kTypeVar, int rank = 0)
        {
            // TODO Jun: Consider adding the block and dimension information in the instruction instead of storing them on the stack

            // TODO Jun: Performance 
            // Is it faster to have a 'pop' specific to arrays to prevent popping dimension for pop to instruction?
            EmitInstrConsole(ProtoCore.DSASM.kw.push, dimensions + "[dim]");
            StackValue opdim = StackValue.BuildArrayDimension(dimensions);
            EmitPush(opdim);


            // Push the identifier block information 
            string srank = "";
            if (rank == Constants.nDimensionArrayRank)
            {
                srank = "[]..[]";
            }
            else
            {
                for (int i = 0; i < rank; ++i)
                {
                    srank += "[]";
                }
            }
            EmitInstrConsole(ProtoCore.DSASM.kw.push, UID + srank + "[type]");
            EmitPushType(UID, rank);
        }

      

        protected void EmitDynamicNode(ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone)
        {
            if (subPass == ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier)
            {
                return;
            }

            StackValue op = StackValue.BuildDynamic(0);
            EmitInstrConsole(ProtoCore.DSASM.kw.push, "dynamic");
            EmitPush(op);
        }

        protected void EmitThisPointerNode(ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone)
        {
            if (subPass == ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier)
            {
                return;
            }
            StackValue op = StackValue.BuildThisPtr(0);
            EmitInstrConsole(ProtoCore.DSASM.kw.push, "thisPtr");
            EmitPush(op);
        }

        protected void EmitFunctionPointer(ProcedureNode procNode)
        {
            var fptrDict = core.FunctionPointerTable.functionPointerDictionary;
            int fptr = fptrDict.Count;

            var fptrNode = new FunctionPointerNode(procNode);
            fptrDict.TryAdd(fptr, fptrNode);
            fptrDict.TryGetBySecond(fptrNode, out fptr);
            EmitPushVarData(0);

            string name = procNode.name;
            if (Constants.kGlobalScope != procNode.classScope)
            {
                int classIndex = procNode.classScope;
                string className = core.ClassTable.ClassNodes[classIndex].name;
                name = String.Format(@"{0}.{1}", className, name);
            }

            EmitInstrConsole(kw.push, name);
            var op = StackValue.BuildFunctionPointer(fptr);
            EmitPush(op, fptrNode.blockId);
        }

        protected List<ProtoCore.DSASM.AttributeEntry> PopulateAttributes(dynamic attributenodes)
        {
            List<ProtoCore.DSASM.AttributeEntry> attributes = new List<DSASM.AttributeEntry>();
            if (attributenodes == null)
                return attributes;
            Validity.Assert(attributenodes is List<ProtoCore.AST.AssociativeAST.AssociativeNode> || attributenodes is List<ProtoCore.AST.ImperativeAST.ImperativeNode>);
            foreach (dynamic anode in attributenodes)
            {
                ProtoCore.DSASM.AttributeEntry entry = PopulateAttribute(anode);
                if (entry != null)
                    attributes.Add(entry);
            }
            return attributes;
        }

        protected ProtoCore.DSASM.AttributeEntry PopulateAttribute(dynamic anode)
        {
            Validity.Assert(anode is ProtoCore.AST.AssociativeAST.FunctionCallNode || anode is ProtoCore.AST.ImperativeAST.FunctionCallNode);
            int cix = core.ClassTable.IndexOf(string.Format("{0}Attribute", anode.Function.Name));
            if (cix == ProtoCore.DSASM.Constants.kInvalidIndex)
            {
                buildStatus.LogSemanticError(string.Format(Resources.UnknownAttribute, anode.Function.Name), core.CurrentDSFileName, anode.line, anode.col);
            }
            ProtoCore.DSASM.AttributeEntry attribute = new ProtoCore.DSASM.AttributeEntry();
            attribute.ClassIndex = cix;
            attribute.Arguments = new List<Node>();
            foreach (dynamic attr in anode.FormalArguments)
            {
                if (!IsConstantExpression(attr))
                {
                    buildStatus.LogSemanticError(Resources.AttributeArgMustBeConstant, core.CurrentDSFileName, anode.line, anode.col);
                    return null;
                }
                attribute.Arguments.Add(attr as ProtoCore.AST.Node);
            }

            // TODO(Jiong): Do a check on the number of arguments 
            bool hasMatchedConstructor = false;
            foreach (ProtoCore.DSASM.ProcedureNode pn in core.ClassTable.ClassNodes[cix].vtable.procList)
            {
                if (pn.isConstructor && pn.argInfoList.Count == attribute.Arguments.Count)
                {
                    hasMatchedConstructor = true;
                    break;
                }
            }
            if (!hasMatchedConstructor)
            {
                buildStatus.LogSemanticError(string.Format(Resources.NoConstructorForAttribute, anode.Function.Name, attribute.Arguments.Count), core.CurrentDSFileName, anode.line, anode.col);
                return null;
            }
            
            return attribute;
        }

        protected bool IsConstantExpression(ProtoCore.AST.Node node)
        {
            if (node is ProtoCore.AST.AssociativeAST.IntNode || 
                node is ProtoCore.AST.ImperativeAST.IntNode ||
                node is ProtoCore.AST.AssociativeAST.DoubleNode || 
                node is ProtoCore.AST.ImperativeAST.DoubleNode ||
                node is ProtoCore.AST.AssociativeAST.BooleanNode || 
                node is ProtoCore.AST.ImperativeAST.BooleanNode ||
                node is ProtoCore.AST.AssociativeAST.StringNode || 
                node is ProtoCore.AST.ImperativeAST.StringNode ||
                node is ProtoCore.AST.AssociativeAST.NullNode || 
                node is ProtoCore.AST.ImperativeAST.NullNode)
                return true;
            else if (node is ProtoCore.AST.AssociativeAST.BinaryExpressionNode)
            {
                ProtoCore.AST.AssociativeAST.BinaryExpressionNode bnode = node as ProtoCore.AST.AssociativeAST.BinaryExpressionNode;
                return IsConstantExpression(bnode.LeftNode) && IsConstantExpression(bnode.RightNode);
            }
            else if  (node is ProtoCore.AST.ImperativeAST.BinaryExpressionNode)
            {
                ProtoCore.AST.ImperativeAST.BinaryExpressionNode bnode = node as ProtoCore.AST.ImperativeAST.BinaryExpressionNode;
                return IsConstantExpression(bnode.RightNode) && IsConstantExpression(bnode.LeftNode);
            }
            else if (node is ProtoCore.AST.ImperativeAST.UnaryExpressionNode)
            {
                ProtoCore.AST.ImperativeAST.UnaryExpressionNode unode = node as ProtoCore.AST.ImperativeAST.UnaryExpressionNode;
                return IsConstantExpression(unode.Expression);
            }
            else if (node is ProtoCore.AST.AssociativeAST.UnaryExpressionNode)
            {
                ProtoCore.AST.AssociativeAST.UnaryExpressionNode unode = node as ProtoCore.AST.AssociativeAST.UnaryExpressionNode;
                return IsConstantExpression(unode.Expression);
            }
            else if (node is ProtoCore.AST.AssociativeAST.ExprListNode)
            {
                ProtoCore.AST.AssociativeAST.ExprListNode arraynode = node as ProtoCore.AST.AssociativeAST.ExprListNode;
                foreach (ProtoCore.AST.Node subnode in arraynode.list)
                {
                    if (!IsConstantExpression(subnode))
                        return false;
                }
                return true;
            }
            else if (node is ProtoCore.AST.ImperativeAST.ExprListNode)
            {
                ProtoCore.AST.ImperativeAST.ExprListNode arraynode = node as ProtoCore.AST.ImperativeAST.ExprListNode;
                foreach (ProtoCore.AST.Node subnode in arraynode.list)
                {
                    if (!IsConstantExpression(subnode))
                        return false;
                }
                return true;
            }
            else if (node is ProtoCore.AST.AssociativeAST.RangeExprNode)
            {
                ProtoCore.AST.AssociativeAST.RangeExprNode rangenode = node as ProtoCore.AST.AssociativeAST.RangeExprNode;
                return IsConstantExpression(rangenode.FromNode) && IsConstantExpression(rangenode.ToNode) && (rangenode.StepNode == null || IsConstantExpression(rangenode.StepNode));
            }
            else if (node is ProtoCore.AST.ImperativeAST.RangeExprNode)
            {
                ProtoCore.AST.ImperativeAST.RangeExprNode rangenode = node as ProtoCore.AST.ImperativeAST.RangeExprNode;
                return IsConstantExpression(rangenode.FromNode) && IsConstantExpression(rangenode.ToNode) && (rangenode.StepNode == null || IsConstantExpression(rangenode.StepNode));
            }

            return false;
        }
            
        protected bool InsideFunction()
        {
            ProtoCore.DSASM.CodeBlock cb = codeBlock;
            while (cb != null)
            {
                if (cb.blockType == ProtoCore.DSASM.CodeBlockType.kFunction)
                    return true;
                else if (cb.blockType == ProtoCore.DSASM.CodeBlockType.kLanguage)
                    return false;

                cb = cb.parent;
            }
            return false;
        }

        // used to manully emit "return = null" instruction if a function or language block does not have a return statement
        // there is update code involved in associativen code gen, so it is not implemented here
        protected abstract void EmitReturnNull();

        protected abstract void DfsTraverse(Node node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, 
            ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, ProtoCore.AST.Node parentNode = null);
        
        protected static int staticPc;
        static int blk = 0;
        public static void setBlkId(int b){ blk = b; }
        public static int getBlkId() { return blk; }

       
        //public void updatePcDictionary(ProtoCore.AST.Node node, int blk)
        public void updatePcDictionary(int line, int col)
        {
            //If the node is null, skip this update
            //if (node == null) 
              //  return;

            blk = codeBlock.codeBlockId;
            //if ((node.line > 0) && (node.col > 0))
            if ((line > 0) && (col > 0))
            {
                ulong mergedKey = (((ulong)blk) << 32 | ((uint)pc));
                //ulong location = (((ulong)node.line) << 32 | ((uint)node.col));
                ulong location = (((ulong)line) << 32 | ((uint)col));

                //ProtoCore.Utils.Validity.Assert(!codeToLocation.ContainsKey(mergedKey));
                if (core.codeToLocation.ContainsKey(mergedKey))
                {
                    core.codeToLocation.Remove(mergedKey);
                }
                
                core.codeToLocation.Add(mergedKey, location);
            }
        }

        protected bool IsInLanguageBlockDefinedInFunction()
        {
            return (localProcedure != null && localProcedure.runtimeIndex != codeBlock.codeBlockId);
        }
    }
}
