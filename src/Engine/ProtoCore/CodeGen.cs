using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ProtoCore.AST;
using ProtoCore.DSASM;
using ProtoCore.Properties;
using ProtoCore.Utils;

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
            globalProcIndex = null != localProcedure ? localProcedure.ID : ProtoCore.DSASM.Constants.kGlobalScope;

            functionCallStack = new List<DSASM.ProcedureNode>();

            IsAssociativeArrayIndexing = false;

            if (core.AsmOutput == null)
            {
                core.AsmOutput = Console.Out;
            }

            ssaPointerStack = new Stack<List<AST.AssociativeAST.AssociativeNode>>();
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
                Constants.kSingleUnderscore + Constants.kInClassDecl + globalClassIndex + 
                Constants.kSingleUnderscore + Constants.kInFunctionScope + globalProcIndex +
                Constants.kSingleUnderscore + Constants.kInstance + functionCallInstance +
                Constants.kSingleUnderscore + graphNode.guid;

            // TODO Jun: Address this in MAGN-3774
            // The current limitation is retrieving the cached trace data for multiple callsites in a single CBN
            // after modifying the lines of code.
            graphNode.CallsiteIdentifier = callsiteIdentifier;
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
            SetStackIndex(symbol);
        }

        protected void SetStackIndex(ProtoCore.DSASM.SymbolNode symbol)
        {
            if (core.Options.RunMode == ProtoCore.DSASM.InterpreterMode.Expression)
            {
                //Set the index of the symbol relative to the watching stack
                symbol.index = core.watchBaseOffset;
                core.watchBaseOffset += 1; 
                return;
            }

            int langblockOffset = 0;
            bool isGlobal = null == localProcedure;

            if (ProtoCore.DSASM.Constants.kGlobalScope != globalClassIndex)
            {
                if (!isGlobal)
                {
                    // Local variable in a member function
                    symbol.index = -1 - ProtoCore.DSASM.StackFrame.StackFrameSize - langblockOffset - core.BaseOffset;
                    core.BaseOffset += 1;
                }
                else
                {
                    // Member variable: static variable allocated on global
                    // stack
                    if (symbol.isStatic)
                    {
                        symbol.index = core.GlobOffset - langblockOffset;
                        core.GlobOffset += 1;
                    }
                    else
                    {
                        symbol.index = classOffset - langblockOffset;
                        classOffset += 1;
                    }
                }
            }
            else if (!isGlobal)
            {
                // Local variable in a global function
                symbol.index = -1 - ProtoCore.DSASM.StackFrame.StackFrameSize - langblockOffset - core.BaseOffset;
                core.BaseOffset += 1;
            }
            else
            {
                // Global variable
                symbol.index = core.GlobOffset + langblockOffset;
                core.GlobOffset += 1;
            }
        }

        #region EMIT_INSTRUCTION_TO_CONSOLE
        public void EmitCompileLog(string message)
        {
            if (dumpByteCode && !isAssocOperator)
            {
                for (int i = 0; i < core.AsmOutputIdents; ++i)
                    core.AsmOutput.Write("\t");
                core.AsmOutput.Write(message);
            }
            
        }
        public void EmitCompileLogFunctionStart(string function)
        {
            if (dumpByteCode && !isAssocOperator)
            {
                core.AsmOutput.Write(function);
                core.AsmOutput.Write("{\n");
                core.AsmOutputIdents++;
            }
        }

        public void EmitCompileLogFunctionEnd()
        {
            if (dumpByteCode && !isAssocOperator)
            {
                core.AsmOutputIdents--;
                for (int i = 0; i < core.AsmOutputIdents; ++i)
                    core.AsmOutput.Write("\t");
                core.AsmOutput.Write("}\n\n");
            }
        }

        public void EmitInstrConsole(string instr)
        {
            if (dumpByteCode && !isAssocOperator)
            {
                var str = string.Format("[{0}.{1}.{2}]{3}\n", codeBlock.language == Language.Associative ? "a" : "i", codeBlock.codeBlockId, pc, instr);
                for (int i = 0; i < core.AsmOutputIdents; ++i)
                    core.AsmOutput.Write("\t");
                core.AsmOutput.Write(str);
            }
        }
        public void EmitInstrConsole(string instr, string op1)
        {
            if (dumpByteCode && !isAssocOperator)
            {
                var str = string.Format("[{0}.{1}.{2}]{3} {4}\n", codeBlock.language == Language.Associative ? "a" : "i", codeBlock.codeBlockId, pc, instr, op1);
                for (int i = 0; i < core.AsmOutputIdents; ++i)
                    core.AsmOutput.Write("\t");
                core.AsmOutput.Write(str);
            }
        }
        public void EmitInstrConsole(string instr, string op1, string op2)
        {
            if (dumpByteCode && !isAssocOperator)
            {
                var str = string.Format("[{0}.{1}.{2}]{3} {4} {5}\n", codeBlock.language == Language.Associative ? "a" : "i", codeBlock.codeBlockId, pc, instr, op1, op2);
                for (int i = 0; i < core.AsmOutputIdents; ++i)
                    core.AsmOutput.Write("\t");
                core.AsmOutput.Write(str);
            }
        }
        public void EmitInstrConsole(string instr, string op1, string op2, string op3)
        {
            if (dumpByteCode && !isAssocOperator)
            {
                var str = string.Format("[{0}.{1}.{2}]{3} {4} {5} {6}\n", codeBlock.language == Language.Associative ? "a" : "i", codeBlock.codeBlockId, pc, instr, op1, op2, op3);
                for (int i = 0; i < core.AsmOutputIdents; ++i)
                    core.AsmOutput.Write("\t");
                core.AsmOutput.Write(str);
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
            ProtoCore.CompilerDefinitions.SubCompilePass subPass, 
            out bool isCollapsed, 
            ProtoCore.AST.Node setterArgument = null
            );

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
                            symbolnode = core.ClassTable.ClassNodes[lefttype.UID].Symbols.symbolList[symindex];
                        }
                    }

                    lefttype = symbolnode.datatype;

                    ProtoCore.AssociativeGraph.UpdateNode updateNode = new AssociativeGraph.UpdateNode();
                    updateNode.symbol = symbolnode;
                    updateNode.nodeType = ProtoCore.AssociativeGraph.UpdateNodeType.Symbol;
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
                        classSymbol.memregion = DSASM.MemoryRegion.MemStatic;
                        classSymbol.name = identnode.Value;
                        classSymbol.classScope = ci;

                        ProtoCore.AssociativeGraph.UpdateNode updateNode = new AssociativeGraph.UpdateNode();
                        updateNode.symbol = classSymbol;
                        updateNode.nodeType = ProtoCore.AssociativeGraph.UpdateNodeType.Symbol;
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
                            updateNode.nodeType = AssociativeGraph.UpdateNodeType.Symbol;
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
                                symbolnode = core.ClassTable.ClassNodes[lefttype.UID].Symbols.symbolList[symindex];
                            }
                        }

                        lefttype = symbolnode.datatype;

                        ProtoCore.AssociativeGraph.UpdateNode updateNode = new AssociativeGraph.UpdateNode();
                        updateNode.symbol = symbolnode;
                        updateNode.nodeType = AssociativeGraph.UpdateNodeType.Symbol;
                        nodeRef.PushUpdateNode(updateNode);
                    }
                }
                else
                {
                    AssociativeGraph.UpdateNode updateNode = new AssociativeGraph.UpdateNode();
                    ProcedureNode procNodeDummy = new ProcedureNode();
                    procNodeDummy.Name = functionName;
                    updateNode.procNode = procNodeDummy;
                    updateNode.nodeType = AssociativeGraph.UpdateNodeType.Method;
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
                            symbolnode = core.ClassTable.ClassNodes[lefttype.UID].Symbols.symbolList[symindex];
                        }
                    }

                    // Since the variable was found, all succeeding nodes in the ident list are class members
                    // Class members have a function scope of kGlobalScope as they are only local to the class, not with any member function
                    functionindex = ProtoCore.DSASM.Constants.kGlobalScope;

                    lefttype = symbolnode.datatype;

                    ProtoCore.AssociativeGraph.UpdateNode updateNode = new AssociativeGraph.UpdateNode();
                    updateNode.symbol = symbolnode;
                    updateNode.nodeType = ProtoCore.AssociativeGraph.UpdateNodeType.Symbol;
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
                        classSymbol.memregion = DSASM.MemoryRegion.MemStatic;
                        classSymbol.name = identnode.Value;
                        classSymbol.classScope = ci;

                        ProtoCore.AssociativeGraph.UpdateNode updateNode = new AssociativeGraph.UpdateNode();
                        updateNode.symbol = classSymbol;
                        updateNode.nodeType = ProtoCore.AssociativeGraph.UpdateNodeType.Symbol;
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
                            updateNode.nodeType = AssociativeGraph.UpdateNodeType.Symbol;
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
                                symbolnode = core.ClassTable.ClassNodes[lefttype.UID].Symbols.symbolList[symindex];
                            }
                        }

                        lefttype = symbolnode.datatype;

                        ProtoCore.AssociativeGraph.UpdateNode updateNode = new AssociativeGraph.UpdateNode();
                        updateNode.symbol = symbolnode;
                        updateNode.nodeType = AssociativeGraph.UpdateNodeType.Symbol;
                        nodeRef.PushUpdateNode(updateNode);
                    }
                }
                else
                {
                    ProtoCore.AssociativeGraph.UpdateNode updateNode = new AssociativeGraph.UpdateNode();
                    ProtoCore.DSASM.ProcedureNode procNodeDummy = new DSASM.ProcedureNode();
                    procNodeDummy.Name = functionName;
                    updateNode.procNode = procNodeDummy;
                    updateNode.nodeType = AssociativeGraph.UpdateNodeType.Method;
                    nodeRef.PushUpdateNode(updateNode);
                }
            }
        }

        protected bool DfsEmitIdentList(
            Node pNode, 
            Node parentNode, 
            int contextClassScope, 
            ref Type lefttype,
            ref int depth, 
            ref Type finalType, 
            bool isLeftidentList, 
            AssociativeGraph.GraphNode graphNode = null,
            CompilerDefinitions.SubCompilePass subPass = CompilerDefinitions.SubCompilePass.None,
            Node binaryExpNode = null)
        {
            dynamic node = pNode;
            if (node is AST.ImperativeAST.IdentifierListNode || node is AST.AssociativeAST.IdentifierListNode)
            {
                dynamic bnode = node;
                if (Operator.dot != bnode.Optr)
                {
                    string message = "The left hand side of an operation can only contain an indirection operator '.' (48D67B9B)";
                    buildStatus.LogSemanticError(message, core.CurrentDSFileName, bnode.line, bnode.col);
                    throw new BuildHaltException(message);
                }

                var identListNode = bnode.LeftNode as AST.ImperativeAST.IdentifierListNode;
                int ci = Constants.kInvalidIndex;
                bool isImperativeFunc = true;

                if (identListNode != null)
                {
                    // Check if identListNode is a valid class 
                    // A valid class exists for the following cases of identListNode:
                    // A.B.ClassName
                    // A valid class is not found for the following, in which case, continue recursing into LeftNode:
                    // A.B.ClassName.foo() where identListNode.RightNode is the function foo()
                    // A.B.ClassName.Property
                    isImperativeFunc = identListNode.RightNode is AST.ImperativeAST.FunctionCallNode;
                    if(!isImperativeFunc)
                    {
                        var className = CoreUtils.GetIdentifierStringUntilFirstParenthesis(identListNode);
                        ci = core.ClassTable.IndexOf(className);
                    }
                    if (ci != Constants.kInvalidIndex && !isImperativeFunc)
                    {
                        finalType.UID = lefttype.UID = ci;
                    }
                }
                if (ci == Constants.kInvalidIndex || isImperativeFunc)
                {
                    DfsEmitIdentList(bnode.LeftNode, bnode, contextClassScope, ref lefttype, ref depth, ref finalType, isLeftidentList, graphNode, subPass);

                    if (lefttype.rank > 0)
                    {
                        lefttype.UID = finalType.UID = (int)PrimitiveType.Null;
                        EmitPushNull();
                        return false;
                    }
                }
                node = bnode.RightNode;
            }

            if (node is AST.ImperativeAST.GroupExpressionNode)
            {
                var array = node.ArrayDimensions;
                node = node.Expression;
                node.ArrayDimensions = array;
            }
            else if (node is AST.AssociativeAST.GroupExpressionNode)
            {
                var array = node.ArrayDimensions;
                var replicationGuides = node.ReplicationGuides;

                node = node.Expression;
                node.ArrayDimensions = array;
                node.ReplicationGuides = replicationGuides;
            }

            if (node is AST.ImperativeAST.IdentifierNode || node is AST.AssociativeAST.IdentifierNode)
            {
                dynamic identnode = node;

                int ci = core.ClassTable.IndexOf(identnode.Value);
                if (Constants.kInvalidIndex != ci)
                {
                    finalType.UID = lefttype.UID = ci;
                }
                else if (identnode.Value == DSDefinitions.Keyword.This)
                {
                    finalType.UID = lefttype.UID = contextClassScope;
                    EmitThisPointerNode();
                    depth++;
                    return true;
                }
                else
                {
                    SymbolNode symbolnode = null;
                    bool isAllocated = false;
                    bool isAccessible = false;
                    if (lefttype.UID != -1)
                    {
                        isAllocated = VerifyAllocation(identnode.Value, lefttype.UID, globalProcIndex, out symbolnode, out isAccessible);
                    }
                    else
                    {
                        isAllocated = VerifyAllocation(identnode.Value, contextClassScope, globalProcIndex, out symbolnode, out isAccessible);
                    }

                    if (null == symbolnode)    //unbound identifier
                    {
                        if (isAllocated && !isAccessible)
                        {
                            string message = String.Format(Resources.kPropertyIsInaccessible, identnode.Value);
                            buildStatus.LogWarning(ProtoCore.BuildData.WarningID.AccessViolation, message, core.CurrentDSFileName, identnode.line, identnode.col, graphNode);
                            lefttype.UID = finalType.UID = (int)PrimitiveType.Null;
                            EmitPushNull();
                            return false;
                        }
                        else
                        {
                            string message = String.Format(Resources.kUnboundIdentifierMsg, identnode.Value);
                            var unboundSymbol = new SymbolNode
                            {
                                name = identnode.Value
                            };
                            buildStatus.LogUnboundVariableWarning(unboundSymbol, message, core.CurrentDSFileName, identnode.line, identnode.col, graphNode);
                        }

                        if (depth == 0)
                        {
                            lefttype.UID = finalType.UID = (int)PrimitiveType.Null;
                            EmitPushNull();
                            depth = 1;
                            return false;
                        }
                        else
                        {
                            var dynamicVariableNode = new DSASM.DyanmicVariableNode(identnode.Value, globalProcIndex, globalClassIndex);
                            core.DynamicVariableTable.variableTable.Add(dynamicVariableNode);
                            int dim = 0;
                            if (null != identnode.ArrayDimensions)
                            {
                                dim = DfsEmitArrayIndexHeap(identnode.ArrayDimensions, graphNode);
                            }


                            EmitInstrConsole(kw.pushm, identnode.Value + "[dynamic]");
                            StackValue dynamicOp = StackValue.BuildDynamic(core.DynamicVariableTable.variableTable.Count - 1);
                            EmitPushm(dynamicOp, symbolnode == null ? globalClassIndex : symbolnode.classScope, DSASM.Constants.kInvalidIndex);

                            if (dim > 0)
                            {
                                EmitPushDimensions(dim);
                                EmitLoadElement(null, Constants.kInvalidIndex);
                            }

                            lefttype.UID = finalType.UID = (int)PrimitiveType.Var;
                            depth++;
                            return true;
                        }
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

                    if (lefttype.rank >= 0)
                    {
                        lefttype.rank -= dimensions;
                        if (lefttype.rank < 0)
                        {
                            lefttype.rank = 0;
                        }
                    }

                    StackValue op = StackValue.Null;
                    if (0 == depth || (symbolnode != null && symbolnode.isStatic))
                    {
                        if (Constants.kGlobalScope == symbolnode.functionIndex
                            && Constants.kInvalidIndex != symbolnode.classScope)
                        {
                            // member var
                            if (symbolnode.isStatic)
                                op = StackValue.BuildStaticMemVarIndex(symbolnode.symbolTableIndex);
                            else
                                op = StackValue.BuildMemVarIndex(symbolnode.symbolTableIndex);
                        }
                        else
                        {
                            op = StackValue.BuildVarIndex(symbolnode.symbolTableIndex);
                        }
                    }

                    if (isLeftidentList || depth == 0)
                    {
                        EmitInstrConsole(kw.pushm, identnode.Value);
                        EmitPushm(op, symbolnode == null ? globalClassIndex : symbolnode.classScope, runtimeIndex);

                        if (null != identnode.ArrayDimensions)
                        {
                            dimensions = DfsEmitArrayIndexHeap(identnode.ArrayDimensions, graphNode);
                        }

                        if (dimensions > 0)
                        {
                            EmitPushDimensions(dimensions);
                            EmitLoadElement(symbolnode, runtimeIndex);
                        }
                    }
                    else
                    {
                        // change to dynamic call to facilitate update mechanism
                        var dynamicVariableNode = new DSASM.DyanmicVariableNode(identnode.Name, globalProcIndex, globalClassIndex);
                        core.DynamicVariableTable.variableTable.Add(dynamicVariableNode);
                        StackValue dynamicOp = StackValue.BuildDynamic(core.DynamicVariableTable.variableTable.Count - 1);
                        EmitInstrConsole(ProtoCore.DSASM.kw.pushm, identnode.Value + "[dynamic]");
                        EmitPushm(dynamicOp, symbolnode == null ? globalClassIndex : symbolnode.classScope, runtimeIndex);

                        if (null != identnode.ArrayDimensions)
                        {
                            dimensions = DfsEmitArrayIndexHeap(identnode.ArrayDimensions, graphNode);
                        }

                        if (dimensions > 0)
                        {
                            EmitPushDimensions(dimensions);
                            EmitLoadElement(null, Constants.kInvalidIndex);
                        }
                    }
                    depth = depth + 1;
                    finalType = lefttype;
                }
                return true;
            }
            else if (node is AST.ImperativeAST.FunctionCallNode || node is AST.AssociativeAST.FunctionCallNode)
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
                var procnode = TraverseFunctionCall(node, pNode, lefttype.UID, depth, ref finalType, graphNode, subPass, binaryExpNode);

                // Restore the graphNode dependent state
                if (null != graphNode)
                {
                    graphNode.allowDependents = allowDependents;
                }

                // This is the first non-auto generated procedure found in the identifier list
                if (null != procnode)
                {
                    if (!procnode.IsConstructor)
                    {
                        functionCallStack.Add(procnode);
                    }
                }

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


        protected int DfsEmitArrayIndexHeap(Node node, AssociativeGraph.GraphNode graphNode = null, ProtoCore.AST.Node parentNode = null, ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None)
        {
            int indexCnt = 0;
            Validity.Assert(node is ProtoCore.AST.AssociativeAST.ArrayNode || node is ProtoCore.AST.ImperativeAST.ArrayNode);

            IsAssociativeArrayIndexing = true;

            dynamic arrayNode = node;
            while (arrayNode is ProtoCore.AST.AssociativeAST.ArrayNode || arrayNode is ProtoCore.AST.ImperativeAST.ArrayNode)
            {
                ++indexCnt;
                dynamic array = arrayNode;
                ProtoCore.Type lastType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 0);
                DfsTraverse(array.Expr, ref lastType, false, graphNode, subPass, parentNode);
                arrayNode = array.Type;
            }

            IsAssociativeArrayIndexing = false;

            return indexCnt;
        }

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
                symbolTable = core.ClassTable.ClassNodes[classScope].Symbols;
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
            if (core.Options.RunMode == DSASM.InterpreterMode.Expression)
            {
                int tempBlockId = context.CurrentBlockId;
                currentCodeBlock = ProtoCore.Utils.CoreUtils.GetCodeBlock(core.CodeBlockList, tempBlockId);
            }

            if (classScope != Constants.kGlobalScope)
            {
                if (currentCodeBlock.blockType != CodeBlockType.Function)
                {
                    // step 1: go through nested language block chain till the top one
                    while (symbolIndex == Constants.kInvalidIndex && currentCodeBlock.parent != null)
                    {
                        symbolIndex = currentCodeBlock.symbolTable.IndexOf(name, Constants.kGlobalScope, Constants.kGlobalScope);
                        if (symbolIndex == Constants.kInvalidIndex)
                        {
                            currentCodeBlock = currentCodeBlock.parent;
                            if (currentCodeBlock.parent == null)
                            {
                                // currentCodeBlock is top language block. Break here.
                                break;
                            }
                        }
                        else
                        {
                            symbol = currentCodeBlock.symbolTable.symbolList[symbolIndex];
                            isAccessible = true;
                            return true;
                        }
                    }
                }

                if ((int)PrimitiveType.Void == classScope)
                {
                    return false;
                }

                if (core.Options.RunMode == ProtoCore.DSASM.InterpreterMode.Expression)
                {
                    //Search local variables in the class member function first
                    if (functionScope != Constants.kGlobalScope)
                    {
                        symbol = core.GetFirstVisibleSymbol(name, classScope, functionScope, currentCodeBlock);
                        isAccessible = symbol != null;
                        return isAccessible;
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
                        symbol = thisClass.Symbols.symbolList[symbolIndex];
                    }

                    isAccessible = true;
                }

                return hasThisSymbol;
            }

            if (functionScope != Constants.kGlobalScope)
            {
                symbol = core.GetFirstVisibleSymbol(name, Constants.kGlobalScope, functionScope, currentCodeBlock);
                isAccessible = symbol != null;
                return symbol != null;
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
                        return false;
                    }
                    isAccessible = true;
                    return true;
                }
                searchBlock = searchBlock.parent;
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

            return (classnode.Symbols.symbolList[symbolIndex].functionIndex == ProtoCore.DSASM.Constants.kGlobalScope);
        }

        protected void Backpatch(int bp, int pc)
        {
            if (core.IsParsingCodeBlockNode)
                return;

            if (ProtoCore.DSASM.OpCode.JMP == codeBlock.instrStream.instrList[bp].opCode
                && codeBlock.instrStream.instrList[bp].op1.IsLabelIndex)
            {
                Validity.Assert(ProtoCore.DSASM.Constants.kInvalidIndex == codeBlock.instrStream.instrList[bp].op1.LabelIndex);
                codeBlock.instrStream.instrList[bp].op1 = StackValue.BuildLabelIndex(pc);
            }
            else if (ProtoCore.DSASM.OpCode.CJMP == codeBlock.instrStream.instrList[bp].opCode
                && codeBlock.instrStream.instrList[bp].op1.IsLabelIndex)
            {
                Validity.Assert(ProtoCore.DSASM.Constants.kInvalidIndex == codeBlock.instrStream.instrList[bp].op1.LabelIndex);
                codeBlock.instrStream.instrList[bp].op1 = StackValue.BuildLabelIndex(pc);
            }
        }

        protected void Backpatch(List<BackpatchNode> table, int pc)
        {
            foreach (BackpatchNode node in table)
            {
                Backpatch(node.bp, pc);
            }
        }

        public abstract ProtoCore.DSASM.ProcedureNode TraverseFunctionCall(Node node, Node parentNode, int lefttype, int depth, ref ProtoCore.Type inferedType, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None, ProtoCore.AST.Node bnode = null);

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
            instr.opCode = ProtoCore.DSASM.OpCode.NEWARR;

            instr.op1 = StackValue.BuildInt(size);
            instr.op2 = StackValue.BuildArrayPointer(0);

            ++pc;
            AppendInstruction(instr);
        }

        protected void EmitPushReplicationGuide(int repGuide, bool isLongest)
        {
            SetEntry();

            Instruction instr = new Instruction();
            instr.opCode = OpCode.PUSHREPGUIDE;
            instr.op1 = StackValue.BuildReplicationGuide(repGuide);
            instr.op2 = StackValue.BuildBoolean(isLongest);

            ++pc;
            AppendInstruction(instr);
        }

        protected void EmitPushLevel(int level, bool isDominant)
        {
            SetEntry();

            Instruction instr = new Instruction();
            instr.opCode = OpCode.PUSHLEVEL;
            instr.op1 = StackValue.BuildInt(level);
            instr.op2 = StackValue.BuildBoolean(isDominant);

            ++pc;
            AppendInstruction(instr);
        }

        protected void EmitPopReplicationGuides(int replicationGuide)
        {
            SetEntry();
            
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.POPREPGUIDES;
            instr.op1 = StackValue.BuildReplicationGuide(replicationGuide);

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

        protected void EmitSetElement(SymbolNode symbol,
           int blockId,
           int line = Constants.kInvalidIndex,
           int col = Constants.kInvalidIndex,
           int eline = Constants.kInvalidIndex,
           int ecol = Constants.kInvalidIndex)
        {
            Validity.Assert(symbol != null);
            if (symbol == null)
            {
                return;
            }

            Instruction instr = new Instruction();
            instr.opCode = OpCode.SETELEMENT;
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

        protected void EmitPushBlockID(int blockID)
        {
            EmitInstrConsole(ProtoCore.DSASM.kw.pushb, blockID.ToString());
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.PUSHBLOCK;
            instr.op1 = StackValue.BuildBlockIndex(blockID);

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

        private void AppendInstruction(Instruction instr, int line = Constants.kInvalidIndex, int col = Constants.kInvalidIndex)
        {
            if (DSASM.InterpreterMode.Expression == core.Options.RunMode)
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

        protected void EmitLoadElement(SymbolNode symbol, int blockId)
        {
            EmitInstrConsole(kw.loadelement);

            SetEntry();
            Instruction instr = new Instruction();
            instr.opCode = OpCode.LOADELEMENT;

            if (symbol != null)
            {
                instr.op1 = BuildOperand(symbol);
                instr.op2 = StackValue.BuildClassIndex(symbol.classScope);
                instr.op3 = StackValue.BuildBlockIndex(blockId);
            }

            ++pc;
            AppendInstruction(instr);
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

        protected void EmitSetMemElement(StackValue op, int blockId, int line = ProtoCore.DSASM.Constants.kInvalidIndex, int col = ProtoCore.DSASM.Constants.kInvalidIndex,
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

        protected abstract void EmitRetb(int line = ProtoCore.DSASM.Constants.kInvalidIndex, int col = ProtoCore.DSASM.Constants.kInvalidIndex,
             int endline = ProtoCore.DSASM.Constants.kInvalidIndex, int endcol = ProtoCore.DSASM.Constants.kInvalidIndex);
			
		protected abstract void EmitRetcn(int blockId = Constants.kInvalidIndex, int line = ProtoCore.DSASM.Constants.kInvalidIndex, int col = ProtoCore.DSASM.Constants.kInvalidIndex,
             int endline = ProtoCore.DSASM.Constants.kInvalidIndex, int endcol = ProtoCore.DSASM.Constants.kInvalidIndex);
        
        protected abstract void EmitReturn(int line = ProtoCore.DSASM.Constants.kInvalidIndex, int col = ProtoCore.DSASM.Constants.kInvalidIndex,
             int endline = ProtoCore.DSASM.Constants.kInvalidIndex, int endcol = ProtoCore.DSASM.Constants.kInvalidIndex);

        protected void EmitReturnToRegister(int line = ProtoCore.DSASM.Constants.kInvalidIndex, int col = ProtoCore.DSASM.Constants.kInvalidIndex,
            int endline = ProtoCore.DSASM.Constants.kInvalidIndex, int endcol = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
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

        protected void EmitIntNode(Node node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None)
        {
            if (subPass == ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier) 
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

            if (!enforceTypeCheck || core.TypeSystem.IsHigherRank((int)PrimitiveType.Integer, inferedType.UID))
            {
                inferedType.UID = (int)PrimitiveType.Integer;
            }

            inferedType.UID = isBooleanOp ? (int)PrimitiveType.Bool : inferedType.UID;
            EmitOpWithEmptyAtLevelAndGuides(emitReplicationGuide, StackValue.BuildInt(value), value.ToString(), node);

            if (IsAssociativeArrayIndexing)
            {
                if (null != graphNode)
                {
                    // Get the last dependent which is the current identifier being indexed into
                    SymbolNode literalSymbol = new SymbolNode();
                    literalSymbol.name = value.ToString();

                    AssociativeGraph.UpdateNode intNode = new AssociativeGraph.UpdateNode();
                    intNode.symbol = literalSymbol;
                    intNode.nodeType = AssociativeGraph.UpdateNodeType.Literal;

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

        protected void EmitCharNode(Node node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None)
        {
            if (subPass == ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier)
            {
                return;
            }

            dynamic cNode = node;
            if (!enforceTypeCheck || core.TypeSystem.IsHigherRank((int)PrimitiveType.Char, inferedType.UID))
            {
                inferedType.UID = (int)PrimitiveType.Char;
            }
            inferedType.UID = isBooleanOp ? (int)PrimitiveType.Bool : inferedType.UID;

            String value = (String)cNode.Value;
            if (value.Length > 1)
            {
                buildStatus.LogSyntaxError(Resources.TooManyCharacters, null, node.line, node.col);
            }
  
            String strValue = "'" + value + "'";
            StackValue op = StackValue.BuildChar(value[0]);

            EmitOpWithEmptyAtLevelAndGuides(emitReplicationGuide, StackValue.BuildChar(value[0]), strValue, node);
        }
       
        protected void EmitStringNode(
            Node node, 
            ref Type inferedType, 
            AssociativeGraph.GraphNode graphNode = null,
            ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None)
        {
            if (subPass == ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier)
            {
                return;
            }

            dynamic sNode = node;
            if (!enforceTypeCheck || core.TypeSystem.IsHigherRank((int)PrimitiveType.String, inferedType.UID))
            {
                inferedType.UID = (int)PrimitiveType.String;
            }

            string value = (string)sNode.Value;
            StackValue svString = core.Heap.AllocateFixedString(value);
            EmitOpWithEmptyAtLevelAndGuides(emitReplicationGuide, svString, "\"" + value + "\"", node);

            if (IsAssociativeArrayIndexing && graphNode != null && graphNode.isIndexingLHS)
            {
                SymbolNode literalSymbol = new SymbolNode();
                literalSymbol.name = value;

                var dimNode = new AssociativeGraph.UpdateNode();
                dimNode.symbol = literalSymbol;
                dimNode.nodeType = AssociativeGraph.UpdateNodeType.Literal;

                graphNode.dimensionNodeList.Add(dimNode);
            }
        }
        
        protected void EmitDoubleNode(Node node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None)
        {
            if (subPass == ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier)
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

            if (!enforceTypeCheck || core.TypeSystem.IsHigherRank((int)PrimitiveType.Double, inferedType.UID))
            {
                inferedType.UID = (int)PrimitiveType.Double;
            }
            inferedType.UID = isBooleanOp ? (int)PrimitiveType.Bool : inferedType.UID;
            EmitOpWithEmptyAtLevelAndGuides(emitReplicationGuide, StackValue.BuildDouble(value), value.ToString(), node);

            if (IsAssociativeArrayIndexing)
            {
                if (null != graphNode)
                {
                    // Get the last dependent which is the current identifier being indexed into
                    SymbolNode literalSymbol = new SymbolNode();
                    literalSymbol.name = value.ToString();

                    AssociativeGraph.UpdateNode intNode = new AssociativeGraph.UpdateNode();
                    intNode.symbol = literalSymbol;
                    intNode.nodeType = AssociativeGraph.UpdateNodeType.Literal;

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

        protected void EmitBooleanNode(Node node, ref ProtoCore.Type inferedType, ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None)
        {
            if (subPass == ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier)
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
            if (enforceTypeCheck || core.TypeSystem.IsHigherRank((int)PrimitiveType.Bool, inferedType.UID))
            {
                inferedType.UID = (int)PrimitiveType.Bool;
            }

            EmitOpWithEmptyAtLevelAndGuides(emitReplicationGuide, StackValue.BuildBoolean(value), value.ToString(), node);
        }

        protected void EmitNullNode(Node node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None)
        {
            if (subPass == ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier)
            {
                return;
            }

            inferedType.UID = isBooleanOp ? (int)PrimitiveType.Bool : inferedType.UID;
            EmitOpWithEmptyAtLevelAndGuides(emitReplicationGuide, StackValue.Null, Literal.Null, node);
        }

        protected void EmitOpWithEmptyAtLevelAndGuides(bool emit, StackValue op, string value, AST.Node node)
        {
            if (emit)
            {
                EmitAtLevel(null);
                EmitReplicationGuides(new List<AST.AssociativeAST.AssociativeNode>());
            }
            EmitInstrConsole(ProtoCore.DSASM.kw.push, value);
            EmitPush(op, node.line, node.col);
        }

        protected void EmitAtLevel(AST.AssociativeAST.AtLevelNode atLevel)
        {
            if (atLevel == null)
            {
                EmitInstrConsole(kw.pushlevel, "@0");
                EmitPushLevel(0, false);
            }
            else
            {
                EmitInstrConsole(kw.pushlevel, atLevel.ToString());
                EmitPushLevel((int)atLevel.Level, atLevel.IsDominant);
            }
        }

        protected void EmitReplicationGuides(List<AST.AssociativeAST.AssociativeNode> replicationGuidesList)
        {
            int replicationGuides = 0;
            if (null != replicationGuidesList && replicationGuidesList.Count > 0)
            {
                replicationGuides = replicationGuidesList.Count;
                for (int n = 0; n < replicationGuides; ++n)
                {
                    var repGuideNode = replicationGuidesList[n] as AST.AssociativeAST.ReplicationGuideNode;
                    var nodeGuide = repGuideNode.RepGuide as AST.AssociativeAST.IdentifierNode;

                    EmitInstrConsole(kw.pushrepguide, nodeGuide.Value + (repGuideNode.IsLongest ? "L" : ""));
                    EmitPushReplicationGuide(Convert.ToInt32(nodeGuide.Value), repGuideNode.IsLongest);
                }
            }

            EmitInstrConsole(kw.poprepguides, replicationGuides.ToString());
            EmitPopReplicationGuides(replicationGuides);
        }


        protected void EmitExprListNode(Node node, ref ProtoCore.Type inferedType, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None, ProtoCore.AST.Node parentNode = null)
        {
            dynamic exprlist = node;
            int rank = 0;

            if (subPass != ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier)
            {
                //get the rank
                dynamic ltNode = exprlist;

                bool isExprListNode = (ltNode is ProtoCore.AST.ImperativeAST.ExprListNode || ltNode is ProtoCore.AST.AssociativeAST.ExprListNode);
                bool isStringNode = (ltNode is ProtoCore.AST.ImperativeAST.StringNode || ltNode is ProtoCore.AST.AssociativeAST.StringNode);
                while ((isExprListNode && ltNode.Exprs.Count > 0) || isStringNode)
                {
                    rank++;
                    if (isStringNode)
                        break;

                    ltNode = ltNode.Exprs[0];
                    isExprListNode = (ltNode is ProtoCore.AST.ImperativeAST.ExprListNode || ltNode is ProtoCore.AST.AssociativeAST.ExprListNode);
                    isStringNode = (ltNode is ProtoCore.AST.ImperativeAST.StringNode || ltNode is ProtoCore.AST.AssociativeAST.StringNode);
                }
            }

            int commonType = (int)PrimitiveType.Void;
            foreach (Node listNode in exprlist.Exprs)
            {
                bool emitReplicationGuideFlag = emitReplicationGuide;
                emitReplicationGuide = false;

                DfsTraverse(listNode, ref inferedType, false, graphNode, subPass, parentNode);
                if ((int)PrimitiveType.Void== commonType)
                {
                    commonType = inferedType.UID;
                }
                else 
                {
                    if (inferedType.UID != commonType)
                    {
                        commonType = (int)PrimitiveType.Var;
                    }
                }

                emitReplicationGuide = emitReplicationGuideFlag;
            }

            inferedType.UID = commonType;
            inferedType.rank = rank;

            if (subPass == ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier)
            {
                return;
            }

            EmitInstrConsole(ProtoCore.DSASM.kw.newarr, exprlist.Exprs.Count.ToString());
            EmitPopArray(exprlist.Exprs.Count);

            if (exprlist.ArrayDimensions != null)
            {
                int dimensions = DfsEmitArrayIndexHeap(exprlist.ArrayDimensions, graphNode);
                EmitPushDimensions(dimensions);
                EmitLoadElement(null, Constants.kInvalidIndex);
            }

            var exprNode = node as AST.AssociativeAST.ExprListNode;
            if (exprNode != null && emitReplicationGuide)
            {
                EmitAtLevel(exprNode.AtLevel);
                EmitReplicationGuides(exprNode.ReplicationGuides);
            }
        }

        protected void EmitReturnStatement(Node node, Type inferedType)
        {
            // Check the returned type against the declared return type
            if (null != localProcedure && 
                localProcedure.IsConstructor &&
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
                    }
                }
            }
        }

        protected void EmitIdentifierListNode(Node node, 
            ref Type inferedType, 
            bool isBooleanOp = false,
            AssociativeGraph.GraphNode graphNode = null,
            CompilerDefinitions.SubCompilePass subPass = CompilerDefinitions.SubCompilePass.None,
            Node bnode = null)
        {
            if (subPass == CompilerDefinitions.SubCompilePass.UnboundIdentifier)
            {
                //to process all unbounded parameters if any
                dynamic iNode = node;
                while (iNode is AST.AssociativeAST.IdentifierListNode || iNode is AST.ImperativeAST.IdentifierListNode)
                {
                    dynamic rightNode = iNode.RightNode;
                    if (rightNode is AST.AssociativeAST.FunctionCallNode || rightNode is AST.ImperativeAST.FunctionCallNode)
                    {
                        foreach (dynamic paramNode in rightNode.FormalArguments)
                        {
                            Type paramType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 0);
                            DfsTraverse(paramNode, ref paramType, false, graphNode, CompilerDefinitions.SubCompilePass.UnboundIdentifier);
                        }
                    }
                    iNode = iNode.LeftNode;
                }
                return;
            }

            int depth = 0;

            Type leftType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.InvalidType, 0);

            BuildSSADependency(node, graphNode);
            if (core.Options.GenerateSSA)
            {
                BuildRealDependencyForIdentList(graphNode);

                if (node is AST.AssociativeAST.IdentifierListNode)
                {
                    if ((node as AST.AssociativeAST.IdentifierListNode).IsLastSSAIdentListFactor)
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
                bool isIdentReference = DfsEmitIdentList(node, null, globalClassIndex, ref leftType, ref depth, ref inferedType, false, graphNode, subPass, bnode);
                inferedType.UID = isBooleanOp ? (int)PrimitiveType.Bool : inferedType.UID;
            }
        }

        protected void EmitDefaultArgNode(ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None)
        {
            if (subPass == ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier)
            {
                return;
            }
            EmitInstrConsole(ProtoCore.DSASM.kw.push, "defaultArg");
            EmitPush(StackValue.BuildDefaultArgument());
        }

        protected void EmitCast(int UID = (int)PrimitiveType.Var, int rank = 0)
        {
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
            EmitInstrConsole(kw.cast, UID + srank + "[type]");

            SetEntry();
            Instruction instr = new Instruction();
            instr.opCode = OpCode.CAST;
            instr.op1 = StackValue.BuildStaticType(UID, rank);

            ++pc;
            AppendInstruction(instr);
        }

        protected void EmitPushDimensions(int dimensions)
        {
            if (dimensions <= 0)
            {
                return;
            }

            EmitInstrConsole(ProtoCore.DSASM.kw.push, dimensions + "[dim]");
            StackValue opdim = StackValue.BuildArrayDimension(dimensions);
            EmitPush(opdim);
        }

        protected void EmitDynamicNode(ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None)
        {
            if (subPass == ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier)
            {
                return;
            }

            StackValue op = StackValue.BuildDynamic(0);
            EmitInstrConsole(ProtoCore.DSASM.kw.push, "dynamic");
            EmitPush(op);
        }

        protected void EmitThisPointerNode(ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None)
        {
            if (subPass == ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier)
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

            string name = procNode.Name;
            if (Constants.kGlobalScope != procNode.ClassID)
            {
                int classIndex = procNode.ClassID;
                string className = core.ClassTable.ClassNodes[classIndex].Name;
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
            attribute.ClassNode = core.ClassTable.ClassNodes[cix];
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
                foreach (ProtoCore.AST.Node subnode in arraynode.Exprs)
                {
                    if (!IsConstantExpression(subnode))
                        return false;
                }
                return true;
            }
            else if (node is ProtoCore.AST.ImperativeAST.ExprListNode)
            {
                ProtoCore.AST.ImperativeAST.ExprListNode arraynode = node as ProtoCore.AST.ImperativeAST.ExprListNode;
                foreach (ProtoCore.AST.Node subnode in arraynode.Exprs)
                {
                    if (!IsConstantExpression(subnode))
                        return false;
                }
                return true;
            }
            else if (node is ProtoCore.AST.AssociativeAST.RangeExprNode)
            {
                ProtoCore.AST.AssociativeAST.RangeExprNode rangenode = node as ProtoCore.AST.AssociativeAST.RangeExprNode;
                return IsConstantExpression(rangenode.From) && IsConstantExpression(rangenode.To) && (rangenode.Step == null || IsConstantExpression(rangenode.Step));
            }
            else if (node is ProtoCore.AST.ImperativeAST.RangeExprNode)
            {
                ProtoCore.AST.ImperativeAST.RangeExprNode rangenode = node as ProtoCore.AST.ImperativeAST.RangeExprNode;
                return IsConstantExpression(rangenode.From) && IsConstantExpression(rangenode.To) && (rangenode.Step == null || IsConstantExpression(rangenode.Step));
            }

            return false;
        }
            
        protected bool InsideFunction()
        {
            CodeBlock cb = codeBlock;
            while (cb != null)
            {
                if (cb.blockType == ProtoCore.DSASM.CodeBlockType.Function)
                    return true;
                else if (cb.blockType == ProtoCore.DSASM.CodeBlockType.Language)
                    return false;

                cb = cb.parent;
            }
            return false;
        }

        // used to manully emit "return = null" instruction if a function or language block does not have a return statement
        // there is update code involved in associativen code gen, so it is not implemented here
        protected abstract void EmitReturnNull();

        protected abstract void DfsTraverse(Node node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, 
            ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None, ProtoCore.AST.Node parentNode = null);
        
        protected static int staticPc;
        static int blk = 0;
        public static void setBlkId(int b){ blk = b; }
       
        public void updatePcDictionary(int line, int col)
        {
            blk = codeBlock.codeBlockId;
            if ((line > 0) && (col > 0))
            {
                ulong mergedKey = (((ulong)blk) << 32 | ((uint)pc));
                ulong location = (((ulong)line) << 32 | ((uint)col));

                if (core.CodeToLocation.ContainsKey(mergedKey))
                {
                    core.CodeToLocation.Remove(mergedKey);
                }
                
                core.CodeToLocation.Add(mergedKey, location);
            }
        }

        protected bool IsInLanguageBlockDefinedInFunction()
        {
            return localProcedure != null && localProcedure.RuntimeIndex != codeBlock.codeBlockId && codeBlock.blockType != CodeBlockType.Function;
        }
    }
}
