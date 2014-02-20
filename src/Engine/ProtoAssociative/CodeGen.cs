//#define ENABLE_INC_DEC_FIX
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using ProtoCore;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Exceptions;
using ProtoCore.Utils;
using ProtoCore.DSASM;
using System.Text;
using ProtoCore.AssociativeGraph;
using ProtoCore.BuildData;

namespace ProtoAssociative
{
    public class OpStack
    {
        public OpStack()
        {
            opstack = new List<ProtoCore.DSASM.AddressType>();
            throw new NotImplementedException();
        }

        public void push(ProtoCore.DSASM.AddressType op)
        {
            opstack.Add(op);
            throw new NotImplementedException();
        }

        public ProtoCore.DSASM.AddressType pop()
        {
            throw new NotImplementedException();
            /*
            int last = opstack.Count - 1;
            ProtoCore.DSASM.AddressType opval = opstack[last];
            opstack.RemoveAt(last);
            return opval;
            */
        }

        private readonly List<ProtoCore.DSASM.AddressType> opstack;
    }

    public class GlobalInstanceProc
    {
        public int classIndex { get; set; }
        public ProtoCore.AST.AssociativeAST.FunctionDefinitionNode procNode { get; set; }

        public GlobalInstanceProc()
        {
            classIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
            procNode = null;
        }
    }

    public class ThisPointerProcOverload
    {
        public int classIndex { get; set; }
        public ProtoCore.AST.AssociativeAST.FunctionDefinitionNode procNode { get; set; }

        public ThisPointerProcOverload()
        {
            classIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
            procNode = null;
        }
    }

    public class CodeGen : ProtoCore.CodeGen
    {
        private readonly bool ignoreRankCheck;

        private readonly List<AssociativeNode> astNodes;
        private List<GlobalInstanceProc> globalInstanceProcList;

        public ProtoCore.DSASM.AssociativeCompilePass compilePass;

        // Jun Comment: 'setConstructorStartPC' a flag to check if the graphnode pc needs to be adjusted by -1
        // This is because constructors auto insert an allocc instruction before any cosntructo body is traversed
        private bool setConstructorStartPC;

        private NodeBuilder nodeBuilder;

        private Dictionary<int, ClassDeclNode> unPopulatedClasses;


        // This variable is used to keep track of the expression ID being traversed by the code generator
        private int currentExpressionID = ProtoCore.DSASM.Constants.kInvalidIndex;

        //
        // This dictionary maps the first pointer to all the SSA temps generated for the identifier list
        // Given:
        //      a = p.x.y
        //
        //      t0 = p
        //      t1 = t0.x
        //      t2 = t1.y
        //      a = t2
        //
        //      The following SSA temps will map to the first pointer 'p'
        //
        //      <t0, p>
        //      <t1, p>
        //      <t2, p>
        //
        Dictionary<string, string> ssaTempToFirstPointerMap = new Dictionary<string, string>();

        private Stack<SymbolNode> expressionSSATempSymbolList = null;
        
        // This constructor is only called for Preloading of assemblies and 
        // precompilation of CodeBlockNode nodes in GraphUI for global language blocks - pratapa
        public CodeGen(Core coreObj) : base(coreObj)
        {
            Validity.Assert(core.IsParsingPreloadedAssembly || core.IsParsingCodeBlockNode);

            classOffset = 0;

            //  either of these should set the console to flood
            //
            ignoreRankCheck = false;
            emitReplicationGuide = false;

            astNodes = new List<AssociativeNode>();
            globalInstanceProcList = new List<GlobalInstanceProc>();
            setConstructorStartPC = false;

            // Re-use the existing procedureTable and symbolTable to access the built-in and predefined functions
            ProcedureTable procTable = core.CodeBlockList[0].procedureTable;
            codeBlock = BuildNewCodeBlock(procTable);
            
            // Remove global symbols from existing symbol table for subsequent run in Graph UI            
            //SymbolTable sTable = core.CodeBlockList[0].symbolTable;
            //sTable.RemoveGlobalSymbols();
            //codeBlock = core.CodeBlockList[0];

            compilePass = ProtoCore.DSASM.AssociativeCompilePass.kClassName;

            // Bouncing to this language codeblock from a function should immediately set the first instruction as the entry point
            if (ProtoCore.DSASM.Constants.kGlobalScope != globalProcIndex)
            {
                isEntrySet = true;
                codeBlock.instrStream.entrypoint = 0;
            }

            nodeBuilder = new NodeBuilder(core);
            unPopulatedClasses = new Dictionary<int, ClassDeclNode>();
            expressionSSATempSymbolList = new Stack<SymbolNode>();
        }

        public CodeGen(Core coreObj, ProtoCore.DSASM.CodeBlock parentBlock = null) : base(coreObj, parentBlock)
        {
            classOffset = 0;

            //  either of these should set the console to flood
            //
            ignoreRankCheck = false;
            emitReplicationGuide = false;

            astNodes = new List<AssociativeNode>();
            globalInstanceProcList = new List<GlobalInstanceProc>();
            setConstructorStartPC = false;


            // Comment Jun: Get the codeblock to use for this codegenerator
            if (core.Options.IsDeltaExecution)
            {
                codeBlock = GetDeltaCompileCodeBlock();
                if (core.Options.IsDeltaCompile)
                {
                    pc = codeBlock.instrStream.instrList.Count;
                }
                else
                {
                    pc = core.deltaCompileStartPC;
                }
            }
            else
            {
                codeBlock = BuildNewCodeBlock();
            }


            if (null == parentBlock)
            {
                if (!core.Options.IsDeltaExecution)
                {
                    // This is a top level block
                    core.CodeBlockList.Add(codeBlock);
                }
            }
            else
            {
                // TODO Jun: Handle nested codeblock here when we support scoping in the graph

                // This is a nested block

                // parentBlock == codeBlock happens when the core is in 
                // delta exectuion and at the same time we create a dynamic 
                // code block (e.g., inline condition)
                if  (parentBlock == codeBlock)
                {
                    codeBlock = BuildNewCodeBlock();
                    pc = 0;
                }
                parentBlock.children.Add(codeBlock);
                codeBlock.parent = parentBlock;
            }

            core.CompleteCodeBlockList.Add(codeBlock);
            compilePass = ProtoCore.DSASM.AssociativeCompilePass.kClassName;

            // Bouncing to this language codeblock from a function should immediately set the first instruction as the entry point
            if (ProtoCore.DSASM.Constants.kGlobalScope != globalProcIndex)
            {
                isEntrySet = true;
                codeBlock.instrStream.entrypoint = 0;
            }

            nodeBuilder = new NodeBuilder(core);
            unPopulatedClasses = new Dictionary<int, ClassDeclNode>();

            // For sub code block, say in inline condition, do we need context?
            /*
            if (core.assocCodegen != null)
            {
                context = core.assocCodegen.context;
            }
            */
            expressionSSATempSymbolList = new Stack<SymbolNode>();
        }

        private ProtoCore.DSASM.CodeBlock GetDeltaCompileCodeBlock()
        {
            ProtoCore.DSASM.CodeBlock cb = null;
            if (core.CodeBlockList.Count <= 0)
            {
                cb = BuildNewCodeBlock();
                core.CodeBlockList.Add(cb);
            }
            else
            {
                cb = core.CodeBlockList[0];
                core.DeltaCodeBlockIndex++;
            }
            Validity.Assert(null != cb);
            return cb;
        }

        private ProtoCore.DSASM.CodeBlock BuildNewCodeBlock(ProcedureTable procTable = null)
        {
            ProcedureTable pTable = procTable == null ? new ProtoCore.DSASM.ProcedureTable(core.RuntimeTableIndex) : procTable;

            // Create a new symboltable for this block
            // Set the new symbol table's parent
            // Set the new table as a child of the parent table
            ProtoCore.DSASM.CodeBlock cb = new ProtoCore.DSASM.CodeBlock(
                    ProtoCore.DSASM.CodeBlockType.kLanguage,
                    ProtoCore.Language.kAssociative,
                    core.CodeBlockIndex,
                    new ProtoCore.DSASM.SymbolTable("associative lang block", core.RuntimeTableIndex),
                    pTable,
                    false,
                    core);

            ++core.CodeBlockIndex;
            ++core.RuntimeTableIndex;

            ++core.DeltaCodeBlockIndex;

            return cb;
        }

        protected override void SetEntry()
        {
            if (ProtoCore.DSASM.Constants.kGlobalScope == globalProcIndex && globalClassIndex == ProtoCore.DSASM.Constants.kGlobalScope && !isEntrySet)
            {
                isEntrySet = true;
                codeBlock.instrStream.entrypoint = pc;
            }
        }

        private ProtoCore.AssociativeGraph.GraphNode GetNextGraphNode()
        {
            foreach (ProtoCore.AssociativeGraph.GraphNode node in codeBlock.instrStream.dependencyGraph.GraphList)
            {
                if (!node.isVisited)
                {
                    return node;
                }
            }
            return null;
        }
        
        private bool IsDependentSubNode(GraphNode node, GraphNode subNode)
        {
            if (subNode.UID == node.UID
                || subNode.exprUID == node.exprUID
                || (subNode.modBlkUID == node.modBlkUID && node.modBlkUID != ProtoCore.DSASM.Constants.kInvalidIndex)
                || subNode.procIndex != node.procIndex
                || subNode.classIndex != node.classIndex
                || subNode.isReturn)
            {
                return false;
            }

            ProtoCore.AssociativeGraph.GraphNode matchingNode = null;
            if (!subNode.DependsOn(node.updateNodeRefList[0], ref matchingNode))
            {
                return false;
            }

            return true;
        }

        private GraphNode GetFinanlStatementOfSSA(GraphNode subNode, ref int index)
        {
            if (subNode.updateNodeRefList.Count <= 0)
            {
                return subNode;
            }

            Validity.Assert(null != subNode.updateNodeRefList[0].nodeList);
            Validity.Assert(subNode.updateNodeRefList[0].nodeList.Count > 0);
            if (subNode.updateNodeRefList[0].nodeList[0].nodeType != ProtoCore.AssociativeGraph.UpdateNodeType.kMethod)
            {
                bool isSSAStatement = ProtoCore.Utils.CoreUtils.IsSSATemp(subNode.updateNodeRefList[0].nodeList[0].symbol.name);
                if (isSSAStatement)
                {
                    while (ProtoCore.DSASM.Constants.kInvalidIndex != subNode.exprUID)
                    {
                        subNode = codeBlock.instrStream.dependencyGraph.GraphList[++index];
                        if (subNode.updateNodeRefList.Count > 0 &&
                            !ProtoCore.Utils.CoreUtils.IsSSATemp(subNode.updateNodeRefList[0].nodeList[0].symbol.name))
                        {
                            break;
                        }
                    }
                }
            }
            return subNode;
        }

        // @keyu: Tarjan's strongly connected components algorithm is used here... 
        // http://en.wikipedia.org/wiki/Tarjan's_strongly_connected_components_algorithm
        // 
        // Returns the first strongly connected component
        private List<GraphNode> StrongConnectComponent(GraphNode node, ref int index, Dictionary<GraphNode, int> lowlinkMap, Dictionary<GraphNode, int> indexMap, Stack<GraphNode> S)
        {
            indexMap[node] = index;
            lowlinkMap[node] = index;
            index++;
            S.Push(node);

            for (int n = 0; n < codeBlock.instrStream.dependencyGraph.GraphList.Count; ++n)
            {
                ProtoCore.AssociativeGraph.GraphNode subNode = codeBlock.instrStream.dependencyGraph.GraphList[n];

                if (!subNode.isActive)
                {
                    continue;
                }

                if (!IsDependentSubNode(node, subNode))
                {
                    continue;
                }

                subNode = GetFinanlStatementOfSSA(subNode, ref n);

                if (indexMap[subNode] == Constants.kInvalidIndex)
                {
                    StrongConnectComponent(subNode, ref index, lowlinkMap, indexMap, S);
                    lowlinkMap[node] = Math.Min(lowlinkMap[node], lowlinkMap[subNode]);
                }
                else if (S.Contains(subNode))
                {
                    lowlinkMap[node] = Math.Min(lowlinkMap[node], indexMap[subNode]);
                }
            }

            if (lowlinkMap[node] == indexMap[node] && S.Count > 1)
            {
                List<GraphNode> dependencyList = new List<GraphNode>();
                while (true)
                {
                    GraphNode w = S.Pop();
                    dependencyList.Add(w);
                    if (w.UID == node.UID)
                    {
                        break;
                    }
                }

                return dependencyList;
            }

            return null;
        }

        private bool CyclicDependencyTest(ProtoCore.AssociativeGraph.GraphNode node, ref SymbolNode cyclicSymbol1, ref SymbolNode cyclicSymbol2)
        {
            if (null == node || node.updateNodeRefList.Count == 0)
            {
                return true;
            }

            var indexMap = new Dictionary<GraphNode, int>();
            var lowlinkMap = new Dictionary<GraphNode, int>();
            var S = new Stack<GraphNode>();
            int index = 0;

            for (int n = 0; n < codeBlock.instrStream.dependencyGraph.GraphList.Count; ++n)
            {
                ProtoCore.AssociativeGraph.GraphNode subNode = codeBlock.instrStream.dependencyGraph.GraphList[n];
                indexMap[subNode] = Constants.kInvalidIndex;
            }

            var dependencyList = StrongConnectComponent(node, ref index, lowlinkMap, indexMap, S);
            if (dependencyList == null)
            {
                return true;
            }
            else
            {
                GraphNode firstNode = dependencyList[0];
                GraphNode lastNode = node;

                string symbol1 = firstNode.updateNodeRefList[0].nodeList[0].symbol.name;
                string symbol2 = lastNode.updateNodeRefList[0].nodeList[0].symbol.name;
                string message = String.Format(ProtoCore.BuildData.WarningMessage.kInvalidStaticCyclicDependency, symbol1, symbol2);

                core.BuildStatus.LogWarning(ProtoCore.BuildData.WarningID.kInvalidStaticCyclicDependency, message, core.CurrentDSFileName);
                firstNode.isCyclic = true;

                cyclicSymbol1 = firstNode.updateNodeRefList[0].nodeList[0].symbol;
                cyclicSymbol2 = lastNode.updateNodeRefList[0].nodeList[0].symbol;

                firstNode.cyclePoint = lastNode;

                return false;
            }
        }

        private void UpdateType (string ident, int funcIndex, ProtoCore.Type dataType)
        {
            int symbolindex = ProtoCore.DSASM.Constants.kInvalidIndex;
            ProtoCore.DSASM.SymbolNode node = null;

            if (ProtoCore.DSASM.Constants.kInvalidIndex != globalClassIndex)
            {
                symbolindex = core.ClassTable.ClassNodes[globalClassIndex].symbols.IndexOf(ident, globalClassIndex, funcIndex);
                if (ProtoCore.DSASM.Constants.kInvalidIndex != symbolindex)
                {
                    node = core.ClassTable.ClassNodes[globalClassIndex].symbols.symbolList[symbolindex];
                }
            }
            else
            {
                ProtoCore.DSASM.CodeBlock searchBlock = codeBlock;

                symbolindex = searchBlock.symbolTable.IndexOf(ident, globalClassIndex, funcIndex);
                while (ProtoCore.DSASM.Constants.kInvalidIndex == symbolindex)
                {
                    ProtoCore.DSASM.CodeBlock parentBlock = searchBlock.parent;
                    if (null == parentBlock)
                        return;

                    searchBlock = parentBlock;
                    symbolindex = searchBlock.symbolTable.IndexOf(ident, globalClassIndex, funcIndex);
                }
                node = searchBlock.symbolTable.symbolList[symbolindex];
            }
            if (node != null)
                node.datatype = dataType;
        }

        private ProtoCore.DSASM.SymbolNode Allocate(
            int classIndex,  // In which class table this variable will be allocated to ?
            int classScope,  // Variable's class scope. For example, it is a variable in base class
            int funcIndex,   // In which function this variable is defined? 
            string ident, 
            ProtoCore.Type datatype, 
            int datasize = ProtoCore.DSASM.Constants.kPrimitiveSize, 
            bool isStatic = false,
            ProtoCore.DSASM.AccessSpecifier access = ProtoCore.DSASM.AccessSpecifier.kPublic,
            ProtoCore.DSASM.MemoryRegion region = ProtoCore.DSASM.MemoryRegion.kMemStack,
            int line = -1,
            int col = -1,
            GraphNode graphNode = null
            )
        {
            bool allocateForBaseVar = classScope < classIndex;
            bool isProperty = classIndex != Constants.kInvalidIndex && funcIndex == Constants.kInvalidIndex;
            if (!allocateForBaseVar && !isProperty && core.ClassTable.IndexOf(ident) != ProtoCore.DSASM.Constants.kInvalidIndex)
                buildStatus.LogSemanticError(ident + " is a class name, can't be used as a variable.", null, line, col, graphNode);

            ProtoCore.DSASM.SymbolNode symbolnode = new ProtoCore.DSASM.SymbolNode();
            symbolnode.name = ident;
            symbolnode.isTemp = ident.StartsWith("%");
            symbolnode.size = datasize;
            symbolnode.functionIndex = funcIndex;
            symbolnode.absoluteFunctionIndex = funcIndex;
            symbolnode.datatype = datatype;
            symbolnode.staticType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, false);
            symbolnode.isArgument = false;
            symbolnode.memregion = region;
            symbolnode.classScope = classScope;
            symbolnode.absoluteClassScope = classScope;
            symbolnode.runtimeTableIndex = codeBlock.symbolTable.RuntimeIndex;
            symbolnode.isStatic = isStatic;
            symbolnode.access = access;
            symbolnode.codeBlockId = codeBlock.codeBlockId;
            if (this.isEmittingImportNode)
                symbolnode.ExternLib = core.CurrentDSFileName;

            int symbolindex = ProtoCore.DSASM.Constants.kInvalidIndex;

            if (IsInLanguageBlockDefinedInFunction())
            {
                symbolnode.classScope = Constants.kGlobalScope;
                symbolnode.functionIndex = Constants.kGlobalScope;
            }

            if (ProtoCore.DSASM.Constants.kInvalidIndex != classIndex && !IsInLanguageBlockDefinedInFunction())
            {
                // NOTE: the following comment and code is OBSOLETE - member
                // variable is not supported now
                // 
                // Yu Ke: it is possible that class table contains same-named 
                // symbols if a class inherits some member variables from base 
                // class, so we need to check name + class index + function 
                // index.
                // 
                //if (core.classTable.list[classIndex].symbols.IndexOf(ident, classIndex, funcIndex) != (int)ProtoCore.DSASM.Constants.kInvalidIndex)
                //    return null;

                symbolindex = core.ClassTable.ClassNodes[classIndex].symbols.IndexOf(ident);
                if (symbolindex != ProtoCore.DSASM.Constants.kInvalidIndex)
                {
                    ProtoCore.DSASM.SymbolNode node = core.ClassTable.ClassNodes[classIndex].symbols.symbolList[symbolindex];
                    if (node.functionIndex == ProtoCore.DSASM.Constants.kGlobalScope &&
                        funcIndex == ProtoCore.DSASM.Constants.kGlobalScope)
                        return null;
                }

                symbolindex = core.ClassTable.ClassNodes[classIndex].symbols.Append(symbolnode);
                if (symbolindex == ProtoCore.DSASM.Constants.kInvalidIndex)
                {
                    return null;
                }

                if (isStatic)
                {
                    Validity.Assert(funcIndex == ProtoCore.DSASM.Constants.kGlobalScope);
                    ProtoCore.DSASM.SymbolNode staticSymbolnode = new ProtoCore.DSASM.SymbolNode();
                    staticSymbolnode.name = ident;
                    staticSymbolnode.isTemp = ident.StartsWith("%");
                    staticSymbolnode.size = datasize;
                    staticSymbolnode.functionIndex = funcIndex;
                    staticSymbolnode.datatype = datatype;
                    staticSymbolnode.staticType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, false);
                    staticSymbolnode.isArgument = false;
                    staticSymbolnode.memregion = region;
                    staticSymbolnode.classScope = classScope;
                    staticSymbolnode.runtimeTableIndex = codeBlock.symbolTable.RuntimeIndex;
                    staticSymbolnode.isStatic = isStatic;
                    staticSymbolnode.access = access;
                    staticSymbolnode.codeBlockId = codeBlock.codeBlockId;
                    if (this.isEmittingImportNode)
                        staticSymbolnode.ExternLib = core.CurrentDSFileName;

                    // If inherits a static property from base class, that propery
                    // symbol should have been added to code block's symbol table,
                    // so we just update symbolTableIndex 
                    int staticSymbolindex = codeBlock.symbolTable.IndexOf(ident, classScope);
                    if (staticSymbolindex == ProtoCore.DSASM.Constants.kInvalidIndex)
                    {
                        AllocateVar(staticSymbolnode);
                        staticSymbolindex = codeBlock.symbolTable.Append(staticSymbolnode);
                        if (staticSymbolindex == ProtoCore.DSASM.Constants.kInvalidIndex)
                        {
                            return null;
                        }
                        staticSymbolnode.symbolTableIndex = staticSymbolindex;
                    }
                    symbolnode.symbolTableIndex = staticSymbolindex;
                }
                else
                {
                    AllocateVar(symbolnode);
                }
            }
            else
            {               
                // Do not import global symbols from external libraries
                //if(this.isEmittingImportNode && core.IsParsingPreloadedAssembly)
                //{
                //    bool importGlobalSymbolFromLib = !string.IsNullOrEmpty(symbolnode.ExternLib) &&
                //        symbolnode.functionIndex == -1 && symbolnode.classScope == -1;

                //    if (importGlobalSymbolFromLib)
                //    {
                //        return symbolnode;
                //    }
                //}

                AllocateVar(symbolnode);

                symbolindex = codeBlock.symbolTable.Append(symbolnode);
                if (symbolindex == ProtoCore.DSASM.Constants.kInvalidIndex)
                {
                    return null;
                }
                symbolnode.symbolTableIndex = symbolindex;
                
            }

            // TODO Jun: Set the symbol table index of the first local variable of 'funcIndex'
            if (null != localProcedure && null == localProcedure.firstLocal && !IsInLanguageBlockDefinedInFunction())
            {
                localProcedure.firstLocal = symbolnode.index;
            }


            if (ProtoCore.DSASM.Constants.kInvalidIndex == symbolindex)
            {
                return null;
            }
            return symbolnode;
        }

        private int AllocateArg(
            string ident,
            int funcIndex,
            ProtoCore.Type datatype,
            int size = 1,
            int datasize = ProtoCore.DSASM.Constants.kPrimitiveSize,
            AssociativeNode nodeArray = null,
            ProtoCore.DSASM.MemoryRegion region = ProtoCore.DSASM.MemoryRegion.kMemStack)
        {
            ProtoCore.DSASM.SymbolNode node = new ProtoCore.DSASM.SymbolNode(
                ident,
                ProtoCore.DSASM.Constants.kInvalidIndex,
                ProtoCore.DSASM.Constants.kInvalidIndex,
                funcIndex,
                datatype,
                datatype,
                size,
                datasize,
                true,
                codeBlock.symbolTable.RuntimeIndex,
                region,
                false,
                null,
                globalClassIndex);

            node.name = ident;
            node.isTemp = ident.StartsWith("%");
            node.size = datasize;
            node.functionIndex = funcIndex;
            node.absoluteFunctionIndex = funcIndex;
            node.datatype = datatype;
            node.isArgument = true;
            node.memregion = ProtoCore.DSASM.MemoryRegion.kMemStack;
            node.classScope = globalClassIndex;
            node.absoluteClassScope = globalClassIndex;
            node.codeBlockId = codeBlock.codeBlockId;
            if (this.isEmittingImportNode)
                node.ExternLib = core.CurrentDSFileName;

            // Comment Jun: The local count will be adjusted and all dependent symbol offsets after the function body has been traversed
            int locOffset = 0;

            // This will be offseted by the local count after locals have been allocated
            node.index = -1 - ProtoCore.DSASM.StackFrame.kStackFrameSize - (locOffset + argOffset);
            ++argOffset;

            int symbolindex = ProtoCore.DSASM.Constants.kInvalidIndex;
            if (ProtoCore.DSASM.Constants.kInvalidIndex != globalClassIndex)
            {
                symbolindex = core.ClassTable.ClassNodes[globalClassIndex].symbols.Append(node);
            }
            else
            {
                symbolindex = codeBlock.symbolTable.Append(node);
            }
            return symbolindex;
        }

        private void EmitAllocc(int type)
        {
            SetEntry();

            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.ALLOCC;
            instr.op1 = StackValue.BuildClassIndex(type);

            ++pc;
            codeBlock.instrStream.instrList.Add(instr);

            // TODO: Figure out why using AppendInstruction fails for adding these instructions to ExpressionInterpreter
            //AppendInstruction(instr);
        }

        private void EmitRetc(int line = ProtoCore.DSASM.Constants.kInvalidIndex, int col = ProtoCore.DSASM.Constants.kInvalidIndex,
            int eline = ProtoCore.DSASM.Constants.kInvalidIndex, int ecol = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.RETC;

            ++pc;
            instr.debug = GetDebugObject(line, col, eline, ecol, ProtoCore.DSASM.Constants.kInvalidIndex);
            codeBlock.instrStream.instrList.Add(instr);

            // TODO: Figure out why using AppendInstruction fails for adding these instructions to ExpressionInterpreter
            //AppendInstruction(instr, line, col);
            updatePcDictionary(line, col);
        }
		
        protected override void EmitRetb( int line = ProtoCore.DSASM.Constants.kInvalidIndex, int col = ProtoCore.DSASM.Constants.kInvalidIndex,
            int endline = ProtoCore.DSASM.Constants.kInvalidIndex, int endcol = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.RETB;

            AuditReturnLocation(ref line, ref col, ref endline, ref endcol);

            ++pc;
            instr.debug = GetDebugObject(line, col, endline, endcol, ProtoCore.DSASM.Constants.kInvalidIndex);
            
            codeBlock.instrStream.instrList.Add(instr);

            // TODO: Figure out why using AppendInstruction fails for adding these instructions to ExpressionInterpreter
            //AppendInstruction(instr, line, col);
            updatePcDictionary(line, col);
        }

		protected override void EmitRetcn(int blockId = Constants.kInvalidIndex, int line = ProtoCore.DSASM.Constants.kInvalidIndex, int col = ProtoCore.DSASM.Constants.kInvalidIndex,
            int endline = ProtoCore.DSASM.Constants.kInvalidIndex, int endcol = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.RETCN;
            instr.op1 = StackValue.BuildBlockIndex(blockId);

            AuditReturnLocation(ref line, ref col, ref endline, ref endcol);

            ++pc;
            instr.debug = GetDebugObject(line, col, endline, endcol, ProtoCore.DSASM.Constants.kInvalidIndex);
            
            codeBlock.instrStream.instrList.Add(instr);

            // TODO: Figure out why using AppendInstruction fails for adding these instructions to ExpressionInterpreter
            //AppendInstruction(instr, line, col);
            updatePcDictionary(line, col);
        }
		
        protected override void EmitReturn(int line = ProtoCore.DSASM.Constants.kInvalidIndex, int col = ProtoCore.DSASM.Constants.kInvalidIndex,
            int endline = ProtoCore.DSASM.Constants.kInvalidIndex, int endcol = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.RETURN;

            AuditReturnLocation(ref line, ref col, ref endline, ref endcol);

            ++pc;
            instr.debug = GetDebugObject(line, col, endline, endcol, ProtoCore.DSASM.Constants.kInvalidIndex);
            codeBlock.instrStream.instrList.Add(instr);

            // TODO: Figure out why using AppendInstruction fails for adding these instructions to ExpressionInterpreter
            //AppendInstruction(instr, line, col);
            updatePcDictionary(line, col);
        }

        //protected override void EmitDependency(int exprUID, bool isSSAAssign)
        protected void EmitDependency(int exprUID, int modBlkUID, bool isSSAAssign)
        {
            SetEntry();

            EmitInstrConsole(ProtoCore.DSASM.kw.dep, exprUID.ToString() + "[ExprUID]", isSSAAssign.ToString() + "[SSA]");

            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.DEP;

            instr.op1 = StackValue.BuildInt(exprUID);
            instr.op2 = StackValue.BuildInt(isSSAAssign ? 1 : 0);
            instr.op3 = StackValue.BuildInt(modBlkUID);

            ++pc;
            codeBlock.instrStream.instrList.Add(instr);

            // TODO: Figure out why using AppendInstruction fails for adding these instructions to ExpressionInterpreter
            //AppendInstruction(instr);
        }

        private void InferDFSTraverse(AssociativeNode node, ref ProtoCore.Type inferedType)
        {
            if (node is IdentifierNode)
            {
                IdentifierNode t = node as IdentifierNode;
                if (core.TypeSystem.IsHigherRank(t.datatype.UID, inferedType.UID))
                {
                    ProtoCore.Type type = new ProtoCore.Type();
                    type.UID = t.datatype.UID;
                    type.IsIndexable = false;
                    inferedType = type;
                }
            }
            else if (node is FunctionCallNode)
            {
                FunctionCallNode f = node as FunctionCallNode;
            }
            else if (node is BinaryExpressionNode)
            {
                BinaryExpressionNode b = node as BinaryExpressionNode;
                InferDFSTraverse(b.LeftNode, ref inferedType);
                InferDFSTraverse(b.RightNode, ref inferedType);
            }
        }

        public ProtoCore.DSASM.ProcedureNode GetProcedureFromInstance(int classScope, ProtoCore.AST.AssociativeAST.FunctionCallNode funcCall, ProtoCore.AssociativeGraph.GraphNode graphNode = null)
        {
            string procName = funcCall.Function.Name;
            Validity.Assert(ProtoCore.DSASM.Constants.kInvalidIndex != classScope);
            List<ProtoCore.Type> arglist = new List<ProtoCore.Type>();

            foreach (AssociativeNode paramNode in funcCall.FormalArguments)
            {
                ProtoCore.Type paramType = new ProtoCore.Type();
                paramType.UID = (int)ProtoCore.PrimitiveType.kTypeVoid;
                paramType.IsIndexable = false;


                DfsTraverse(paramNode, ref paramType, false, null, ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier);
                emitReplicationGuide = false;
                enforceTypeCheck = true;

                arglist.Add(paramType);
            }

            bool isAccessible;
            int realType;
            ProtoCore.DSASM.ProcedureNode procNode = core.ClassTable.ClassNodes[classScope].GetMemberFunction(procName, arglist, globalClassIndex, out isAccessible, out realType, false);
            return procNode;
        }

        private void EmitFunctionCall(int depth, int type, List<ProtoCore.Type> arglist, ProtoCore.DSASM.ProcedureNode procNode, ProtoCore.AST.AssociativeAST.FunctionCallNode funcCall, bool getter = false, ProtoCore.AST.AssociativeAST.BinaryExpressionNode bnode = null)
        {
            int blockId = procNode.runtimeIndex;

            //push value-not-provided default argument
            for (int i = arglist.Count; i < procNode.argInfoList.Count; i++)
            {
                EmitDefaultArgNode();
            }

            // Push the function declaration block and indexed array
            // Jun TODO: Implementeation of indexing into a function call:
            //  x = f()[0][1]
            int dimensions = 0;
            EmitPushVarData(blockId, dimensions);


            // The function call
            EmitInstrConsole(ProtoCore.DSASM.kw.callr, procNode.name);
            if(getter)
                EmitCall(procNode.procId, type, depth, ProtoCore.DSASM.Constants.kInvalidIndex, ProtoCore.DSASM.Constants.kInvalidIndex, 
                    ProtoCore.DSASM.Constants.kInvalidIndex, ProtoCore.DSASM.Constants.kInvalidIndex, procNode.pc);
            // Break at function call inside dynamic lang block created for a 'true' or 'false' expression inside an inline conditional
            else if (core.DebugProps.breakOptions.HasFlag(DebugProperties.BreakpointOptions.EmitInlineConditionalBreakpoint))
            {
                Validity.Assert(core.DebugProps.highlightRange != null);

                ProtoCore.CodeModel.CodePoint startInclusive = core.DebugProps.highlightRange.StartInclusive;
                ProtoCore.CodeModel.CodePoint endExclusive = core.DebugProps.highlightRange.EndExclusive;

                EmitCall(procNode.procId, type, depth, startInclusive.LineNo, startInclusive.CharNo, endExclusive.LineNo, endExclusive.CharNo, procNode.pc);
            }
            else if(bnode != null)
                EmitCall(procNode.procId, type, depth, bnode.line, bnode.col, bnode.endLine, bnode.endCol, procNode.pc);
            else
                EmitCall(procNode.procId, type, depth, funcCall.line, funcCall.col, funcCall.endLine, funcCall.endCol, procNode.pc);

            // The function return value
            EmitInstrConsole(ProtoCore.DSASM.kw.push, ProtoCore.DSASM.kw.regRX);
            StackValue opReturn = StackValue.BuildRegister(Registers.RX);
            EmitPush(opReturn);
        }

        private List<ProtoCore.AST.AssociativeAST.AssociativeNode> GetReplicationGuides(ProtoCore.AST.Node node)
        {
            if (node is ProtoCore.AST.AssociativeAST.IntNode
                || node is ProtoCore.AST.AssociativeAST.DoubleNode
                || node is ProtoCore.AST.AssociativeAST.BooleanNode
                || node is ProtoCore.AST.AssociativeAST.CharNode
                || node is ProtoCore.AST.AssociativeAST.StringNode
                || node is ProtoCore.AST.AssociativeAST.NullNode
                )
            {
                return null;
            }
            else if (node is ProtoCore.AST.AssociativeAST.ExprListNode)
            {
                return (node as ProtoCore.AST.AssociativeAST.ExprListNode).ReplicationGuides;
            }
            else if (node is ProtoCore.AST.AssociativeAST.GroupExpressionNode)
            {
                return (node as ProtoCore.AST.AssociativeAST.GroupExpressionNode).ReplicationGuides;
            }

            return null;
        }


        public ProtoCore.DSASM.ProcedureNode TraverseDotFunctionCall(ProtoCore.AST.Node node, ProtoCore.AST.Node parentNode, int lefttype, int depth, ref ProtoCore.Type inferedType, 
            ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone,
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode bnode = null)
        {
            FunctionCallNode funcCall = null;
            ProtoCore.DSASM.ProcedureNode procCallNode = null;
            ProtoCore.DSASM.ProcedureNode procDotCallNode = null;
            string procName = null;
            List<ProtoCore.Type> arglist = new List<ProtoCore.Type>();
            ProtoCore.Type dotCallType = new ProtoCore.Type();
            dotCallType.UID = (int)PrimitiveType.kTypeVar;
            dotCallType.IsIndexable = false;

            bool isConstructor = false;
            bool isStaticCall = false;
            bool isStaticCallAllowed = false;
            bool isUnresolvedDot = false;
            bool isUnresolvedMethod = false;

            int classIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
            string className = string.Empty;

            ProtoCore.AST.AssociativeAST.FunctionDotCallNode dotCall = node as ProtoCore.AST.AssociativeAST.FunctionDotCallNode;
            funcCall = dotCall.DotCall;
            procName = dotCall.FunctionCall.Function.Name;

            List<AssociativeNode> replicationGuide = (dotCall.FunctionCall.Function as IdentifierNode).ReplicationGuides;

            var dotCallFirstArgument = dotCall.DotCall.FormalArguments[0];
            if (dotCallFirstArgument is FunctionDotCallNode)
            {
                isUnresolvedDot = true;
            }
            else if (dotCallFirstArgument is IdentifierNode || dotCallFirstArgument is ThisPointerNode)
            {
                // Check if the lhs identifer is a class name
                string lhsName = "";
                int ci = Constants.kInvalidIndex;

                if (dotCallFirstArgument is IdentifierNode)
                {
                    lhsName = (dotCallFirstArgument as IdentifierNode).Name;
                    ci = core.ClassTable.IndexOf(lhsName);
                    classIndex = ci;
                    className = lhsName;

                    // As a class name can be used as property name, we need to
                    // check if this identifier is a property or a class name.
                    //
                    if (ci != Constants.kInvalidIndex && globalClassIndex != Constants.kInvalidIndex)
                    {
                        ProtoCore.DSASM.SymbolNode symbolnode;
                        bool isAccessbile = false;
                        bool hasAllocated = VerifyAllocation(lhsName, globalClassIndex, globalProcIndex, out symbolnode, out isAccessbile);

                        // Well, found a property whose name is class name. Now
                        // we need to check if the RHS function call is 
                        // constructor or not.  
                        if (hasAllocated && isAccessbile && symbolnode.functionIndex == ProtoCore.DSASM.Constants.kInvalidIndex)
                        {
                            var procnode = GetProcedureFromInstance(ci, dotCall.FunctionCall);
                            if (procnode != null && !procnode.isConstructor)
                            {
                                ci = Constants.kInvalidIndex;
                                lhsName = "";
                            }
                        }
                    }
                }

                var classes = core.ClassTable.ClassNodes;

                if (ci != ProtoCore.DSASM.Constants.kInvalidIndex)
                {
                    // It is a class name
                    dotCall.DotCall.FormalArguments[0] = new IntNode { value = ci.ToString() };
                    dotCallFirstArgument = dotCall.DotCall.FormalArguments[0];

                    inferedType.UID = dotCallType.UID = ci;

                    string rhsName = dotCall.FunctionCall.Function.Name;
                    procCallNode = GetProcedureFromInstance(ci, dotCall.FunctionCall, graphNode);
                    if (null != procCallNode)
                    {
                        isConstructor = procCallNode.isConstructor;

                        // It's a static call if its not a constructor
                        isStaticCall = !procCallNode.isConstructor;

                        // If this is a static call and the first method found was not static
                        // Look further
                        if (isStaticCall && !procCallNode.isStatic)
                        {
                            ProcedureNode staticProcCallNode = classes[ci].GetFirstStaticMemberFunction(procName);
                            if (null != staticProcCallNode)
                            {
                                procCallNode = staticProcCallNode;
                            }
                        }

                        isStaticCallAllowed = procCallNode.isStatic && isStaticCall;
                    }
                    else
                    {
                        var procNode = classes[ci].GetFirstStaticMemberFunction(procName);
                        string functionName = dotCall.FunctionCall.Function.Name;
                        string property;

                        if (null != procNode)
                        {
                            string message = String.Format(ProtoCore.BuildData.WarningMessage.kMethodHasInvalidArguments, functionName);
                            buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kCallingNonStaticMethodOnClass, message, core.CurrentDSFileName, dotCall.line, dotCall.col);
                        }
                        else if (CoreUtils.TryGetPropertyName(functionName, out property))
                        {
                            procNode = classes[ci].GetFirstMemberFunction(property);

                            if (procNode != null)
                            {
                                if (subPass == AssociativeSubCompilePass.kNone)
                                {
                                    EmitFunctionPointer(procNode);
                                    return null;
                                }
                            }
                            else
                            {
                                string message = String.Format(WarningMessage.kCallingNonStaticProperty, 
                                                               lhsName, property);

                                buildStatus.LogWarning(WarningID.kCallingNonStaticMethodOnClass, 
                                                       message, 
                                                       core.CurrentDSFileName, 
                                                       dotCall.line, 
                                                       dotCall.col);
                            }
                        }
                        else
                        {
                            string message = String.Format(WarningMessage.kCallingNonStaticMethod, lhsName, functionName);
                            buildStatus.LogWarning(WarningID.kCallingNonStaticMethodOnClass, message, core.CurrentDSFileName, dotCall.line, dotCall.col);
                        }
                    }
                }


                if (dotCall.DotCall.FormalArguments.Count == ProtoCore.DSASM.Constants.kDotCallArgCount)
                {
                    if (dotCallFirstArgument is IdentifierNode)
                    {
                        ProtoCore.DSASM.SymbolNode symbolnode = null;
                        bool isAccessible = false;
                        bool isAllocated = VerifyAllocation((dotCallFirstArgument as IdentifierNode).Name, globalClassIndex, globalProcIndex, out symbolnode, out isAccessible);
                        if (isAllocated && symbolnode.datatype.UID != (int)PrimitiveType.kTypeVar)
                        {
                            inferedType.UID = symbolnode.datatype.UID;

                            if (ProtoCore.DSASM.Constants.kInvalidIndex != inferedType.UID)
                            {
                                procCallNode = GetProcedureFromInstance(symbolnode.datatype.UID, dotCall.FunctionCall);
                            }

                            if (null != procCallNode)
                            {
                                if (procCallNode.isConstructor)
                                {
                                    if (subPass != ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
                                    {
                                        // A constructor cannot be called from an instance
                                        string message = String.Format(ProtoCore.BuildData.WarningMessage.KCallingConstructorOnInstance, procName);
                                        buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kCallingConstructorOnInstance, message, core.CurrentDSFileName, funcCall.line, funcCall.col);
                                    }

                                    isUnresolvedDot = true;
                                    isUnresolvedMethod = true;
                                }
                                else
                                {
                                   isAccessible =
                                       procCallNode.access == ProtoCore.DSASM.AccessSpecifier.kPublic
                                        || (procCallNode.access == ProtoCore.DSASM.AccessSpecifier.kPrivate && procCallNode.classScope == globalClassIndex);

                                    if (!isAccessible)
                                    {
                                        if (subPass != ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
                                        {
                                            string message = String.Format(ProtoCore.BuildData.WarningMessage.kMethodIsInaccessible, procName);
                                            buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kAccessViolation, message, core.CurrentDSFileName, funcCall.line, funcCall.col);
                                        }
                                    }

                                    if (null != procCallNode)
                                    {
                                        int dynamicRhsIndex = int.Parse((dotCall.DotCall.FormalArguments[1] as IntNode).value);
                                        core.DynamicFunctionTable.functionTable[dynamicRhsIndex].classIndex = procCallNode.classScope;
                                        core.DynamicFunctionTable.functionTable[dynamicRhsIndex].procedureIndex = procCallNode.procId;
                                        core.DynamicFunctionTable.functionTable[dynamicRhsIndex].pc = procCallNode.pc;
                                    }
                                }
                            }
                        }
                        else
                        {
                            isUnresolvedDot = true;
                        }
                    }
                    else if (dotCallFirstArgument is ThisPointerNode)
                    {
                        if (globalClassIndex != Constants.kInvalidIndex) 
                        {
                            procCallNode = GetProcedureFromInstance(globalClassIndex, dotCall.FunctionCall);
                            if (null != procCallNode && procCallNode.isConstructor)
                            {
                                dotCall.DotCall.FormalArguments[0] = new IntNode { value = globalClassIndex.ToString() };
                                dotCallFirstArgument = dotCall.DotCall.FormalArguments[0];
                                inferedType.UID = dotCallType.UID = ci;
                            }
                        }
                    }
                }
            }
            else if (funcCall.FormalArguments[0] is IntNode)
            {
                inferedType.UID = dotCallType.UID = int.Parse((funcCall.FormalArguments[0] as IntNode).value);
                classIndex = inferedType.UID;
                procCallNode = GetProcedureFromInstance(dotCallType.UID, dotCall.FunctionCall, graphNode);

                if (null != procCallNode)
                {
                    // It's a static call if its not a constructor
                    isConstructor = procCallNode.isConstructor;
                    isStaticCall = !procCallNode.isConstructor;

                    // If this is a static call and the first method found was not static
                    // Look further
                    if (isStaticCall && !procCallNode.isStatic)
                    {
                        ProtoCore.DSASM.ProcedureNode staticProcCallNode = core.ClassTable.ClassNodes[inferedType.UID].GetFirstStaticMemberFunction(procName);
                        if (null != staticProcCallNode)
                        {
                            procCallNode = staticProcCallNode;
                        }
                    }

                    isStaticCallAllowed = procCallNode.isStatic && isStaticCall;
                    className = core.ClassTable.ClassNodes[dotCallType.UID].name;

                    if (isStaticCall && !isStaticCallAllowed)
                    {
                        if (subPass != ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
                        {
                            string property;
                            className = core.ClassTable.ClassNodes[dotCallType.UID].name;
                            ProtoCore.DSASM.ProcedureNode staticProcCallNode = core.ClassTable.ClassNodes[inferedType.UID].GetFirstStaticMemberFunction(procName);

                            if (null != staticProcCallNode)
                            {
                                string message = String.Format(ProtoCore.BuildData.WarningMessage.kMethodHasInvalidArguments, procName);
                                buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kCallingNonStaticMethodOnClass, message, core.CurrentDSFileName, dotCall.line, dotCall.col);
                            }
                            else if (CoreUtils.TryGetPropertyName(procName, out property))
                            {
                                string message = String.Format(ProtoCore.BuildData.WarningMessage.kCallingNonStaticProperty, property, className);
                                buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kCallingNonStaticMethodOnClass, message, core.CurrentDSFileName, dotCall.line, dotCall.col);
                            }
                            else
                            {
                                string message = String.Format(ProtoCore.BuildData.WarningMessage.kCallingNonStaticMethod, procName, className);
                                buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kCallingNonStaticMethodOnClass, message, core.CurrentDSFileName, dotCall.line, dotCall.col);
                            }
                        }
                        isUnresolvedMethod = true;
                    }
                    else
                    {
                        inferedType = procCallNode.returntype;
                    }
                }
            }

            // Its an accceptable method at this point
            if (!isUnresolvedMethod)
            {
                int funtionArgCount = 0;

                //foreach (AssociativeNode paramNode in funcCall.FormalArguments)
                for (int n = 0; n < funcCall.FormalArguments.Count; ++n)    
                {
                    AssociativeNode paramNode = funcCall.FormalArguments[n];
                    ProtoCore.Type paramType = new ProtoCore.Type();
                    paramType.UID = (int)ProtoCore.PrimitiveType.kTypeVoid;
                    paramType.IsIndexable = false;

                    emitReplicationGuide = false;

                    // If it's a binary node then continue type check, otherwise disable type check and just take the type of paramNode itself
                    // f(1+2.0) -> type check enabled - param is typed as double
                    // f(2) -> type check disabled - param is typed as int
                    enforceTypeCheck = !(paramNode is BinaryExpressionNode);


                    // TODO Jun: Cleansify me
                    // What im doing is just taking the second parameter of the dot op (The method call)
                    // ...and adding it to the graph node dependencies
                    if (ProtoCore.DSASM.Constants.kDotArgIndexDynTableIndex == n)
                    {
                        if (subPass != ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
                        {
                            if (!isConstructor)
                            {
                                if (null != procCallNode)
                                {
                                    if (graphNode.dependentList.Count > 0)
                                    {
                                        ProtoCore.AssociativeGraph.UpdateNodeRef nodeRef = new ProtoCore.AssociativeGraph.UpdateNodeRef();
                                        ProtoCore.AssociativeGraph.UpdateNode updateNode = new ProtoCore.AssociativeGraph.UpdateNode();
                                        ProtoCore.DSASM.ProcedureNode procNodeDummy = new ProtoCore.DSASM.ProcedureNode();
                                        if (procCallNode.isAutoGenerated)
                                        {
                                            ProtoCore.DSASM.SymbolNode sym = new ProtoCore.DSASM.SymbolNode();
                                            sym.name = procName.Remove(0, ProtoCore.DSASM.Constants.kSetterPrefix.Length);
                                            updateNode.nodeType = ProtoCore.AssociativeGraph.UpdateNodeType.kSymbol;
                                            updateNode.symbol = sym;
                                        }
                                        else
                                        {
                                            procNodeDummy.name = procName;
                                            updateNode.nodeType = ProtoCore.AssociativeGraph.UpdateNodeType.kMethod;
                                            updateNode.procNode = procNodeDummy;
                                        }
                                        graphNode.dependentList[0].updateNodeRefList[0].nodeList.Add(updateNode);
                                    }
                                }
                                else
                                {
                                    // comment Jun:
                                    // This is dotarg whos first argument is also a dotarg
                                    // dotarg(dorarg...)...)
                                    if (graphNode.dependentList.Count > 0)
                                    {
                                        if (ProtoCore.Utils.CoreUtils.IsGetterSetter(procName))
                                        {
                                            ProtoCore.AssociativeGraph.UpdateNode updateNode = new ProtoCore.AssociativeGraph.UpdateNode();
                                            ProtoCore.DSASM.SymbolNode sym = new ProtoCore.DSASM.SymbolNode();
                                            sym.name = procName.Remove(0, ProtoCore.DSASM.Constants.kSetterPrefix.Length);
                                            updateNode.nodeType = ProtoCore.AssociativeGraph.UpdateNodeType.kSymbol;
                                            updateNode.symbol = sym;

                                            graphNode.dependentList[0].updateNodeRefList[0].nodeList.Add(updateNode);
                                        }
                                    }
                                }
                            }
                        }
                    }


                    // Traversing the first arg (the LHS pointer/Static instanct/Constructor
                    if (ProtoCore.DSASM.Constants.kDotArgIndexPtr == n)
                    {
                        // Comment Jun: 
                        //      Allow guides only on 'this' pointers for non getter/setter methods
                        //      No guides for 'this' pointers in constructors calls (There is no this pointer yet)
                        //      
                        /*
                            class C
                            {
                                def f(a : int)
                                {
                                    return = 10;
                                }
                            }
                            p = {C.C(), C.C()};
                            x = p<1>.f({1,2}<2>); // guides allowed on the pointer 'p'
                      
                         
                         
                         
                         
                            class A
                            {
                                x : var[];
                                constructor A()
                                {
                                    x = {1,2};
                                }
                            }
                            a = A.A();
                            b = A.A();
                            c = a<1>.x<2>; // guides not allowed on getter
                         
                         */
                        if (!ProtoCore.Utils.CoreUtils.IsGetterSetter(procName) && !isConstructor)
                        {
                            emitReplicationGuide = true;
                        }

                        DfsTraverse(paramNode, ref paramType, false, graphNode, subPass, bnode);
                        if (subPass != ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
                        {
                            if (isStaticCall && isStaticCallAllowed)
                            {
                                Validity.Assert(ProtoCore.DSASM.Constants.kInvalidIndex != classIndex);
                                Validity.Assert(string.Empty != className);
                                SymbolNode classSymbol = new SymbolNode();
                                classSymbol.name = className;
                                classSymbol.classScope = classIndex;

                                ProtoCore.AssociativeGraph.GraphNode dependentNode = new ProtoCore.AssociativeGraph.GraphNode();
                                dependentNode.PushSymbolReference(classSymbol, ProtoCore.AssociativeGraph.UpdateNodeType.kSymbol);
                                graphNode.PushDependent(dependentNode);
                            }
                        }
                    }


                    // Traversing the actual arguments passed into the function (not the dot function)
                    else if (ProtoCore.DSASM.Constants.kDotArgIndexArrayArgs == n)
                    {
                        int defaultAdded = 0;


                        // If its null this is the second call in a chained dot
                        if (null != procCallNode)
                        {
                            // Check how many args were passed in.... against what is expected
                            defaultAdded = procCallNode.argInfoList.Count - dotCall.FunctionCall.FormalArguments.Count;
                        }


                        // Enable graphnode dependencies if its a setter method
                        bool allowDependentState = null != graphNode ? graphNode.allowDependents : false;
                        if (ProtoCore.Utils.CoreUtils.IsSetter(procName))
                        {
                            // If the arguments are not temporaries
                            ProtoCore.AST.AssociativeAST.ExprListNode exprList = paramNode as ExprListNode;
                            Validity.Assert(1 == exprList.list.Count);

                            string varname = string.Empty;
                            if (exprList.list[0] is IdentifierNode)
                            {
                                varname = (exprList.list[0] as IdentifierNode).Name;

                                // TODO Jun: deprecate SSA flag and do full SSA
                                if (core.Options.FullSSA)
                                {
                                    // Only allow the acutal function variables and SSA temp vars
                                    // TODO Jun: determine what temp could be passed in that is autodegenerated and non-SSA
                                    if (!ProtoCore.Utils.CoreUtils.IsAutoGeneratedVar(varname)
                                        || ProtoCore.Utils.CoreUtils.IsSSATemp(varname))
                                    {
                                        graphNode.allowDependents = true;
                                    }
                                }
                                else
                                {
                                    if (!ProtoCore.Utils.CoreUtils.IsAutoGeneratedVar(varname))
                                    {
                                        graphNode.allowDependents = true;
                                    }
                                }
                            }
                            else
                            {
                                graphNode.allowDependents = true;
                            }

                        }

                        emitReplicationGuide = true; 

                        if (defaultAdded > 0)
                        {
                            ProtoCore.AST.AssociativeAST.ExprListNode exprList = paramNode as ExprListNode;

                            if (subPass != AssociativeSubCompilePass.kUnboundIdentifier)
                            {
                                for (int i = 0; i < defaultAdded; i++)
                                {
                                    exprList.list.Add(new DefaultArgNode());
                                }

                            }
                            DfsTraverse(paramNode, ref paramType, false, graphNode, subPass);
                            funtionArgCount = exprList.list.Count;
                        }
                        else
                        {
                            Validity.Assert(paramNode is ProtoCore.AST.AssociativeAST.ExprListNode);
                            ProtoCore.AST.AssociativeAST.ExprListNode exprList = paramNode as ProtoCore.AST.AssociativeAST.ExprListNode;


                            // Comment Jun: This is a getter/setter or a an auto-generated thisarg function...
                            // ...add the dynamic sv that will be resolved as a pointer at runtime
                            if (!isStaticCall && !isConstructor)
                            {
                                //if (null != procCallNode && ProtoCore.Utils.CoreUtils.IsGetterSetter(procCallNode.name) && AssociativeSubCompilePass.kNone == subPass)
                                // TODO Jun: pls get rid of subPass checking outside the core travesal
                                if (ProtoCore.DSASM.AssociativeSubCompilePass.kNone == subPass)
                                {
                                    exprList.list.Insert(0, new DynamicNode());
                                }
                            }

                            if (exprList.list.Count > 0)
                            {
                                foreach (ProtoCore.AST.AssociativeAST.AssociativeNode exprListNode in exprList.list)
                                {
                                    bool repGuideState = emitReplicationGuide;
                                    if (subPass != ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
                                    {
                                        if (exprListNode is ProtoCore.AST.AssociativeAST.ExprListNode
                                            || exprListNode is ProtoCore.AST.AssociativeAST.GroupExpressionNode)
                                        {
                                            if (core.Options.TempReplicationGuideEmptyFlag)
                                            {
                                                // Emit the replication guide for the exprlist
                                                List<ProtoCore.AST.AssociativeAST.AssociativeNode> repGuideList = GetReplicationGuides(exprListNode);
                                                EmitReplicationGuides(repGuideList, true);

                                                emitReplicationGuide = false;

                                                // Pop off the guide if the current element was an array
                                                if (null != repGuideList)
                                                {
                                                    EmitInstrConsole(ProtoCore.DSASM.kw.popg);
                                                    EmitPopGuide();
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        emitReplicationGuide = false;
                                    }

                                    DfsTraverse(exprListNode, ref paramType, false, graphNode, subPass, bnode);
                                    emitReplicationGuide = repGuideState;
                                }

                                if (subPass != ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
                                {

                                    EmitInstrConsole(ProtoCore.DSASM.kw.alloca, exprList.list.Count.ToString());
                                    EmitPopArray(exprList.list.Count);

                                    if (exprList.ArrayDimensions != null)
                                    {
                                        int dimensions = DfsEmitArrayIndexHeap(exprList.ArrayDimensions, graphNode);
                                        EmitInstrConsole(ProtoCore.DSASM.kw.pushindex, dimensions.ToString() + "[dim]");
                                        EmitPushArrayIndex(dimensions);
                                    }
                                }
                            }
                            else
                            {
                                if (exprList != null)
                                {
                                    bool emitReplicationGuideState = emitReplicationGuide;
                                    emitReplicationGuide = false;
                                    DfsTraverse(paramNode, ref paramType, false, graphNode, subPass);
                                    emitReplicationGuide = emitReplicationGuideState;
                                }
                                else
                                {
                                    DfsTraverse(paramNode, ref paramType, false, graphNode, subPass);
                                }
                            }

                            funtionArgCount = exprList.list.Count;
                        }

                        emitReplicationGuide = false;

                        // Restore the state only if it is a setter method
                        if (ProtoCore.Utils.CoreUtils.IsSetter(procName))
                        {
                            graphNode.allowDependents = allowDependentState;
                        }
                    }
                    else if (ProtoCore.DSASM.Constants.kDotArgIndexArgCount == n)
                    {
                        ProtoCore.AST.AssociativeAST.IntNode argNumNode = new ProtoCore.AST.AssociativeAST.IntNode() { value = funtionArgCount.ToString() };
                        DfsTraverse(argNumNode, ref paramType, false, graphNode, subPass); 
                    }
                    else
                    {
                        DfsTraverse(paramNode, ref paramType, false, graphNode, subPass);
                    }

                    emitReplicationGuide = false;
                    enforceTypeCheck = true;

                    arglist.Add(paramType);
                }
            }


            if (subPass == ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
            {
                return null;
            }


            // Comment Jun: Append the lhs pointer as an argument to the overloaded function
            if (!isConstructor && !isStaticCall)
            {
                Validity.Assert(dotCall.DotCall.FormalArguments[ProtoCore.DSASM.Constants.kDotArgIndexArrayArgs] is ExprListNode);
                ExprListNode functionArgs = dotCall.DotCall.FormalArguments[ProtoCore.DSASM.Constants.kDotArgIndexArrayArgs] as ExprListNode;
                functionArgs.list.Insert(0, dotCall.DotCall.FormalArguments[ProtoCore.DSASM.Constants.kDotArgIndexPtr]);
            }

            if (isUnresolvedMethod)
            {
                EmitNullNode(new NullNode(), ref inferedType);
                return null;
            }

            procDotCallNode = core.GetFirstVisibleProcedure(ProtoCore.DSASM.Constants.kDotArgMethodName, arglist, codeBlock);
           

            // From here on, handle the actual procedure call 
            int type = ProtoCore.DSASM.Constants.kInvalidIndex;

            int refClassIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
            if (parentNode != null && parentNode is ProtoCore.AST.AssociativeAST.IdentifierListNode)
            {
                ProtoCore.AST.Node leftnode = (parentNode as ProtoCore.AST.AssociativeAST.IdentifierListNode).LeftNode;
                if (leftnode != null && leftnode is ProtoCore.AST.AssociativeAST.IdentifierNode)
                {
                    refClassIndex = core.ClassTable.IndexOf(leftnode.Name);
                }
            }

            if (dotCallFirstArgument is FunctionCallNode ||
                dotCallFirstArgument is FunctionDotCallNode ||
                dotCallFirstArgument is ExprListNode)
            {
                inferedType.UID = arglist[0].UID;
            }

            // If lefttype is a valid class then check if calling a constructor
            if ((int)ProtoCore.PrimitiveType.kInvalidType != inferedType.UID
                && (int)ProtoCore.PrimitiveType.kTypeVoid != inferedType.UID
                && procName != ProtoCore.DSASM.Constants.kFunctionPointerCall)
            {
                bool isStaticOrConstructor = refClassIndex != ProtoCore.DSASM.Constants.kInvalidIndex;
                //procCallNode = core.classTable.list[inferedType.UID].GetMemberFunction(procName, arglist, globalClassIndex, out isAccessible, out realType, isStaticOrConstructor);
                procCallNode = core.ClassTable.ClassNodes[inferedType.UID].GetFirstMemberFunction(procName);
            }

            // Try function pointer firstly
            if ((procCallNode == null) && (procName != ProtoCore.DSASM.Constants.kFunctionPointerCall))
            {
                bool isAccessibleFp;
                ProtoCore.DSASM.SymbolNode symbolnode = null;
                bool isAllocated = VerifyAllocation(procName, globalClassIndex, globalProcIndex, out symbolnode, out isAccessibleFp);
                if (isAllocated) // not checking the type against function pointer, as the type could be var
                {
                    procName = ProtoCore.DSASM.Constants.kFunctionPointerCall;
                    // The graph node always depends on this function pointer
                    if (null != graphNode)
                    {
                        ProtoCore.AssociativeGraph.GraphNode dependentNode = new ProtoCore.AssociativeGraph.GraphNode();
                        dependentNode.PushSymbolReference(symbolnode);
                        graphNode.PushDependent(dependentNode);
                    }
                }
            }

            // Always try global function firstly. Because we dont have syntax
            // support for calling global function (say, ::foo()), if we try
            // member function firstly, there is no way to call a global function
            // For member function, we can use this.foo() to distinguish it from 
            // global function. 
            if ((procCallNode == null) && (procName != ProtoCore.DSASM.Constants.kFunctionPointerCall))
            {
                procCallNode = core.GetFirstVisibleProcedure(procName, arglist, codeBlock);
                if (null != procCallNode)
                {
                    type = ProtoCore.DSASM.Constants.kGlobalScope;
                    if (core.TypeSystem.IsHigherRank(procCallNode.returntype.UID, inferedType.UID))
                    {
                        inferedType = procCallNode.returntype;
                    }
                }
            }

            // Try member functions in global class scope
            if ((procCallNode == null) && (procName != ProtoCore.DSASM.Constants.kFunctionPointerCall) && (parentNode == null))
            {
                if (globalClassIndex != ProtoCore.DSASM.Constants.kInvalidIndex)
                {
                    int realType;
                    bool isAccessible;
                    bool isStaticOrConstructor = refClassIndex != ProtoCore.DSASM.Constants.kInvalidIndex;
                    ProtoCore.DSASM.ProcedureNode memProcNode = core.ClassTable.ClassNodes[globalClassIndex].GetMemberFunction(procName, arglist, globalClassIndex, out isAccessible, out realType, isStaticOrConstructor);

                    if (memProcNode != null)
                    {
                        Validity.Assert(realType != ProtoCore.DSASM.Constants.kInvalidIndex);
                        procCallNode = memProcNode;
                        inferedType = procCallNode.returntype;
                        type = realType;

                        if (!isAccessible)
                        {
                            string message = String.Format(ProtoCore.BuildData.WarningMessage.kMethodIsInaccessible, procName);
                            buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kAccessViolation, message, core.CurrentDSFileName, funcCall.line, funcCall.col);

                            inferedType.UID = (int)PrimitiveType.kTypeNull;
                            EmitPushNull();
                            return procCallNode;
                        }
                    }
                }
            }

            if (isUnresolvedDot)
            {
                // Get the dot call procedure
                ProtoCore.DSASM.ProcedureNode procNode = procDotCallNode;
                if (!isConstructor && !isStaticCall)
                {
                    procNode = core.GetFirstVisibleProcedure(ProtoCore.DSASM.Constants.kDotMethodName, null, codeBlock);
                }

                if(CoreUtils.IsGetter(procName))
                {
                    EmitFunctionCall(depth, type, arglist, procNode, funcCall, true);
                }
                else
                    EmitFunctionCall(depth, type, arglist, procNode, funcCall, false, bnode);

                if (dotCallType.UID != (int)PrimitiveType.kTypeVar)
                {
                    inferedType.UID = dotCallType.UID;
                }
                
                return procCallNode;
            }

            if (null != procCallNode)
            {
                if (procCallNode.isConstructor &&
                        (globalClassIndex != ProtoCore.DSASM.Constants.kInvalidIndex) &&
                        (globalProcIndex != ProtoCore.DSASM.Constants.kInvalidIndex) &&
                        (globalClassIndex == inferedType.UID))
                {
                    ProtoCore.DSASM.ProcedureNode contextProcNode = core.ClassTable.ClassNodes[globalClassIndex].vtable.procList[globalProcIndex];
                    if (contextProcNode.isConstructor &&
                        string.Equals(contextProcNode.name, procCallNode.name) &&
                        contextProcNode.runtimeIndex == procCallNode.runtimeIndex)
                    {
                        string message = String.Format(ProtoCore.BuildData.WarningMessage.kCallingConstructorInConstructor, procName);
                        buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kCallingConstructorInConstructor, message, core.CurrentDSFileName, node.line, node.col);
                        inferedType.UID = (int)PrimitiveType.kTypeNull;
                        EmitPushNull();
                        return procCallNode;
                    }
                }

                inferedType = procCallNode.returntype;

                //if call is replication call
                if (procCallNode.isThisCallReplication)
                {
                    inferedType.IsIndexable = true;
                    inferedType.rank++;
                }


                // Get the dot call procedure
                ProtoCore.DSASM.ProcedureNode procNode = procDotCallNode;
                if (!isConstructor && !isStaticCall)
                {
                    procNode = core.GetFirstVisibleProcedure(ProtoCore.DSASM.Constants.kDotMethodName, null, codeBlock);
                }

                if (CoreUtils.IsSetter(procName))
                {
                    EmitFunctionCall(depth, type, arglist, procNode, funcCall);
                }
                // Do not emit breakpoint at getters only - pratapa
                else if (CoreUtils.IsGetter(procName))
                {
                    EmitFunctionCall(depth, type, arglist, procNode, funcCall, true);
                }
                else
                {
                    EmitFunctionCall(depth, type, arglist, procNode, funcCall, false, bnode);
                }

                if (dotCallType.UID != (int)PrimitiveType.kTypeVar)
                {
                    inferedType.UID = dotCallType.UID;
                }

                if (isConstructor)
                {
                    foreach (AssociativeNode paramNode in dotCall.FunctionCall.FormalArguments)
                    {
                        // Get the lhs symbol list
                        ProtoCore.Type ltype = new ProtoCore.Type();
                        ltype.UID = globalClassIndex;
                        ProtoCore.AssociativeGraph.UpdateNodeRef argNodeRef = new ProtoCore.AssociativeGraph.UpdateNodeRef();
                        DFSGetSymbolList(paramNode, ref ltype, argNodeRef);

                        if (null != graphNode)
                        {
                            if (argNodeRef.nodeList.Count > 0)
                            {
                                graphNode.updatedArguments.Add(argNodeRef);
                            }
                        }
                    }


                    graphNode.firstProc = procCallNode;
                }
                
                return procCallNode;
            }
            else
            {
                // Function does not exist at this point but we try to reolve at runtime
                if (depth <= 0 && procName != ProtoCore.DSASM.Constants.kFunctionPointerCall)
                {
                    if (inferedType.UID != (int)PrimitiveType.kTypeVar)
                    {
                        if (!core.Options.SuppressFunctionResolutionWarning)
                        {
                            string property;
                            if (CoreUtils.TryGetPropertyName(procName, out property))
                            {
                                string message = String.Format(ProtoCore.BuildData.WarningMessage.kPropertyNotFound, property);
                                buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kPropertyNotFound, message, core.CurrentDSFileName, funcCall.line, funcCall.col);
                            }
                            else
                            {
                                string message = String.Format(ProtoCore.BuildData.WarningMessage.kMethodNotFound, procName);
                                buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kFunctionNotFound, message, core.CurrentDSFileName, funcCall.line, funcCall.col);
                            }
                        }
                        inferedType.UID = (int)PrimitiveType.kTypeNull;
                    }
                   

                    // Get the dot call procedure
                    ProtoCore.DSASM.ProcedureNode procNode = procDotCallNode;
                    if (!isConstructor && !isStaticCall)
                    {
                        procNode = core.GetFirstVisibleProcedure(ProtoCore.DSASM.Constants.kDotMethodName, null, codeBlock);
                    }

                    if (CoreUtils.IsGetter(procName))
                    {
                        EmitFunctionCall(depth, type, arglist, procNode, funcCall, true);
                    }
                    else
                        EmitFunctionCall(depth, type, arglist, procNode, funcCall, false, bnode);

                    if (dotCallType.UID != (int)PrimitiveType.kTypeVar)
                    {
                        inferedType.UID = dotCallType.UID;
                    }
                }
                else
                {
                    if (procName == ProtoCore.DSASM.Constants.kFunctionPointerCall && depth == 0)
                    {
                        ProtoCore.DSASM.DynamicFunctionNode dynamicFunctionNode = new ProtoCore.DSASM.DynamicFunctionNode(procName, arglist, lefttype);
                        core.DynamicFunctionTable.functionTable.Add(dynamicFunctionNode);

                        var iNode = nodeBuilder.BuildIdentfier(funcCall.Function.Name);
                        EmitIdentifierNode(iNode, ref inferedType);
                    }
                    else
                    {
                        ProtoCore.DSASM.DynamicFunctionNode dynamicFunctionNode = new ProtoCore.DSASM.DynamicFunctionNode(funcCall.Function.Name, arglist, lefttype);
                        core.DynamicFunctionTable.functionTable.Add(dynamicFunctionNode);
                    }
                    // The function call
                    EmitInstrConsole(ProtoCore.DSASM.kw.callr, funcCall.Function.Name + "[dynamic]");
                    EmitDynamicCall(core.DynamicFunctionTable.functionTable.Count - 1, globalClassIndex, depth, funcCall.line, funcCall.col, funcCall.endLine, funcCall.endCol);

                    // The function return value
                    EmitInstrConsole(ProtoCore.DSASM.kw.push, ProtoCore.DSASM.kw.regRX);
                    StackValue opReturn = StackValue.BuildRegister(Registers.RX);
                    EmitPush(opReturn);

                    if (core.Options.TempReplicationGuideEmptyFlag && emitReplicationGuide)
                    {
                        int guides = EmitReplicationGuides(replicationGuide);
                        EmitInstrConsole(ProtoCore.DSASM.kw.pushindex, guides + "[guide]");
                        EmitPushReplicationGuide(guides);
                    }

                    //assign inferedType to var
                    inferedType.UID = (int)PrimitiveType.kTypeVar;
                }
            }
            return procDotCallNode;
        }


        public override ProtoCore.DSASM.ProcedureNode TraverseFunctionCall(ProtoCore.AST.Node node, ProtoCore.AST.Node parentNode, int lefttype, int depth, ref ProtoCore.Type inferedType,             
            ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone,                 
            ProtoCore.AST.Node bnode = null)
        {
            FunctionCallNode funcCall = null;
            string procName = null;
            List<ProtoCore.Type> arglist = new List<ProtoCore.Type>();
            ProtoCore.Type dotCallType = new ProtoCore.Type();
            dotCallType.UID = (int)PrimitiveType.kTypeVar;
            dotCallType.IsIndexable = false;

            ProtoCore.AssociativeGraph.UpdateNode updateNode = new ProtoCore.AssociativeGraph.UpdateNode();

            if (node is ProtoCore.AST.AssociativeAST.FunctionDotCallNode)
            {
                return TraverseDotFunctionCall(node, parentNode, lefttype, depth, ref inferedType, graphNode, subPass, bnode as BinaryExpressionNode);
            }
            else 
            {
                funcCall = node as FunctionCallNode;
                procName = funcCall.Function.Name;

                int classIndex = core.ClassTable.IndexOf(procName);
                bool isAccessible;
                int dummy;

                // To support unamed constructor
                if (classIndex != Constants.kInvalidIndex)
                {
                    ProcedureNode constructor = core.ClassTable.ClassNodes[classIndex].GetMemberFunction(procName, arglist, globalClassIndex, out isAccessible, out dummy, true);
                    if (constructor != null && constructor.isConstructor)
                    {
                        FunctionCallNode rhsFNode = node as ProtoCore.AST.AssociativeAST.FunctionCallNode;
                        AssociativeNode classNode = nodeBuilder.BuildIdentfier(procName);
                        FunctionDotCallNode dotCallNode = ProtoCore.Utils.CoreUtils.GenerateCallDotNode(classNode, rhsFNode, core);
                        return TraverseDotFunctionCall(dotCallNode, parentNode, lefttype, depth, ref inferedType, graphNode, subPass, bnode as BinaryExpressionNode);
                    }
                }
            }

            foreach (AssociativeNode paramNode in funcCall.FormalArguments)
            {
                ProtoCore.Type paramType = new ProtoCore.Type();
                paramType.UID = (int)ProtoCore.PrimitiveType.kTypeVoid; 
                paramType.IsIndexable = false;

                // The range expression function does not need replication guides
                emitReplicationGuide = !procName.Equals(ProtoCore.DSASM.Constants.kFunctionRangeExpression);

                // If it's a binary node then continue type check, otherwise disable type check and just take the type of paramNode itself
                // f(1+2.0) -> type check enabled - param is typed as double
                // f(2) -> type check disabled - param is typed as int
                enforceTypeCheck = !(paramNode is BinaryExpressionNode);

                DfsTraverse(paramNode, ref paramType, false, graphNode, subPass, bnode);

                emitReplicationGuide = false;
                enforceTypeCheck = true;

                arglist.Add(paramType);
            }
           
            if (subPass == ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
            {
                return null;   
            }
                    

            ProtoCore.DSASM.ProcedureNode procNode = null;
            int type = ProtoCore.DSASM.Constants.kInvalidIndex;
            
            int refClassIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
            if (parentNode != null && parentNode is ProtoCore.AST.AssociativeAST.IdentifierListNode)
            {
                ProtoCore.AST.Node leftnode = (parentNode as ProtoCore.AST.AssociativeAST.IdentifierListNode).LeftNode;
                if (leftnode != null && leftnode is ProtoCore.AST.AssociativeAST.IdentifierNode)
                {
                    refClassIndex = core.ClassTable.IndexOf(leftnode.Name);
                }
            }

            // Check for the actual method, not the dot method
            // If lefttype is a valid class then check if calling a constructor
            if ((int)ProtoCore.PrimitiveType.kInvalidType != inferedType.UID
                && (int)ProtoCore.PrimitiveType.kTypeVoid != inferedType.UID
                && procName != ProtoCore.DSASM.Constants.kFunctionPointerCall)
            {

                bool isAccessible;
                int realType;

                bool isStaticOrConstructor = refClassIndex != ProtoCore.DSASM.Constants.kInvalidIndex;
                procNode = core.ClassTable.ClassNodes[inferedType.UID].GetMemberFunction(procName, arglist, globalClassIndex, out isAccessible, out realType, isStaticOrConstructor);

                if (procNode != null)
                {
                    Validity.Assert(realType != ProtoCore.DSASM.Constants.kInvalidIndex);
                    type = lefttype = realType;

                    if (!isAccessible)
                    {
                        type = lefttype = realType;
                        procNode = null;
                        string message = String.Format(ProtoCore.BuildData.WarningMessage.kMethodIsInaccessible, procName);
                        buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kAccessViolation, message, core.CurrentDSFileName, funcCall.line, funcCall.col);
                        inferedType.UID = (int)PrimitiveType.kTypeNull;

                        EmitPushNull();
                        return procNode;
                    }

                }
            }

            // Try function pointer firstly
            if ((procNode == null) && (procName != ProtoCore.DSASM.Constants.kFunctionPointerCall))
            {
                bool isAccessibleFp;
                ProtoCore.DSASM.SymbolNode symbolnode = null;
                bool isAllocated = VerifyAllocation(procName, globalClassIndex, globalProcIndex, out symbolnode, out isAccessibleFp);
                if (isAllocated) // not checking the type against function pointer, as the type could be var
                {
                    procName = ProtoCore.DSASM.Constants.kFunctionPointerCall;
                    // The graph node always depends on this function pointer
                    if (null != graphNode)
                    {
                        ProtoCore.AssociativeGraph.GraphNode dependentNode = new ProtoCore.AssociativeGraph.GraphNode();
                        dependentNode.PushSymbolReference(symbolnode);
                        graphNode.PushDependent(dependentNode);
                    }
                }
            }

            // Always try global function firstly. Because we dont have syntax
            // support for calling global function (say, ::foo()), if we try
            // member function firstly, there is no way to call a global function
            // For member function, we can use this.foo() to distinguish it from 
            // global function. 
            if ((procNode == null) && (procName != ProtoCore.DSASM.Constants.kFunctionPointerCall))
            {
                procNode = core.GetFirstVisibleProcedure(procName, arglist, codeBlock);
                if (null != procNode)
                {
                    type = ProtoCore.DSASM.Constants.kGlobalScope;
                    if (core.TypeSystem.IsHigherRank(procNode.returntype.UID, inferedType.UID))
                    {
                        inferedType = procNode.returntype;
                    }
                }
            }

            // Try member functions in global class scope
            if ((procNode == null) && (procName != ProtoCore.DSASM.Constants.kFunctionPointerCall) && (parentNode == null))
            {
                if (globalClassIndex != ProtoCore.DSASM.Constants.kInvalidIndex)
                {
                    int realType;
                    bool isAccessible;
                    bool isStaticOrConstructor = refClassIndex != ProtoCore.DSASM.Constants.kInvalidIndex;
                    ProtoCore.DSASM.ProcedureNode memProcNode = core.ClassTable.ClassNodes[globalClassIndex].GetMemberFunction(procName, arglist, globalClassIndex, out isAccessible, out realType, isStaticOrConstructor);

                    if (memProcNode != null)
                    {
                        Validity.Assert(realType != ProtoCore.DSASM.Constants.kInvalidIndex);
                        procNode = memProcNode;
                        inferedType = procNode.returntype;
                        type = realType;

                        if (!isAccessible)
                        {
                            string message = String.Format(ProtoCore.BuildData.WarningMessage.kMethodIsInaccessible, procName);
                            buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kAccessViolation, message, core.CurrentDSFileName, funcCall.line, funcCall.col);

                            inferedType.UID = (int)PrimitiveType.kTypeNull;
                            EmitPushNull();
                            return procNode;
                        }
                    }
                }
            }

            if (null != procNode)
            {
                if (procNode.isConstructor &&
                        (globalClassIndex != ProtoCore.DSASM.Constants.kInvalidIndex) &&
                        (globalProcIndex != ProtoCore.DSASM.Constants.kInvalidIndex) &&
                        (globalClassIndex == inferedType.UID))
                {
                    ProtoCore.DSASM.ProcedureNode contextProcNode = core.ClassTable.ClassNodes[globalClassIndex].vtable.procList[globalProcIndex];
                    if (contextProcNode.isConstructor &&
                        string.Equals(contextProcNode.name, procNode.name) &&
                        contextProcNode.runtimeIndex == procNode.runtimeIndex)
                    {
                        string message = String.Format(ProtoCore.BuildData.WarningMessage.kCallingConstructorInConstructor, procName);
                        buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kCallingConstructorInConstructor, message, core.CurrentDSFileName, node.line, node.col );
                        inferedType.UID = (int)PrimitiveType.kTypeNull;
                        EmitPushNull();
                        return procNode;
                    }
                }

                inferedType = procNode.returntype;

                //if call is replication call
                if (procNode.isThisCallReplication)
                {
                    inferedType.IsIndexable = true;
                    inferedType.rank++;
                }

                if (ProtoCore.DSASM.Constants.kInvalidIndex != procNode.procId)
                {






                    //
                    // ==============Establishing graphnode links in modified arguments=============
                    //
                    //  proc TraverseCall(node, graphnode)
                    //      ; Get the first procedure, this will only be the first visible procedure 
                    //      ; Overloads will be handled at runtime
                    //      def fnode = getProcedure(node)
                    //      
                    //      ; For every argument in the function call,
                    //      ; attach the modified property list and append it to the graphnode update list
                    //      foreach arg in node.args
                    //          if fnode.updatedArgProps is not null
                    //              def noderef = arg.ident (or identlist)
                    //              noderef.append(fnode.updatedArgProps)
                    //              graphnode.pushUpdateRef(noderef)
                    //          end
                    //      end
                    //  end
                    //
                    // =============================================================================
                    //

                    foreach (AssociativeNode paramNode in funcCall.FormalArguments)
                    {
                        // Get the lhs symbol list
                        ProtoCore.Type ltype = new ProtoCore.Type();
                        ltype.UID = globalClassIndex;
                        ProtoCore.AssociativeGraph.UpdateNodeRef argNodeRef = new ProtoCore.AssociativeGraph.UpdateNodeRef();
                        DFSGetSymbolList(paramNode, ref ltype, argNodeRef);

                        if (null != graphNode)
                        {
                            if (argNodeRef.nodeList.Count > 0)
                            {
                                graphNode.updatedArguments.Add(argNodeRef);
                            }
                        }
                    }


                    // The function is at block 0 if its a constructor, member or at the globals scope.
                    // Its at block 1 if its inside a language block. 
                    // Its limited to block 1 as of R1 since we dont support nested function declarations yet
                    int blockId = procNode.runtimeIndex;

                    //push value-not-provided default argument
                    for (int i = arglist.Count; i < procNode.argInfoList.Count; i++)
                    {
                        EmitDefaultArgNode();
                    }
                    
                    // Push the function declaration block and indexed array
                    // Jun TODO: Implementeation of indexing into a function call:
                    //  x = f()[0][1]
                    int dimensions = 0;
                    EmitPushVarData(blockId, dimensions);


                    // The function call
                    EmitInstrConsole(ProtoCore.DSASM.kw.callr, procNode.name);

                    // Do not emit breakpoints at built-in methods like _add/_sub etc. - pratapa
                    if (procNode.isAssocOperator || procNode.name.Equals(ProtoCore.DSASM.Constants.kInlineConditionalMethodName))
                    {
                        EmitCall(procNode.procId, type, depth, ProtoCore.DSASM.Constants.kInvalidIndex, ProtoCore.DSASM.Constants.kInvalidIndex,
                                    ProtoCore.DSASM.Constants.kInvalidIndex, ProtoCore.DSASM.Constants.kInvalidIndex, procNode.pc);
                    }
                    // Break at function call inside dynamic lang block created for a 'true' or 'false' expression inside an inline conditional
                    else if (core.DebugProps.breakOptions.HasFlag(DebugProperties.BreakpointOptions.EmitInlineConditionalBreakpoint))
                    {
                        Validity.Assert(core.DebugProps.highlightRange != null);

                        ProtoCore.CodeModel.CodePoint startInclusive = core.DebugProps.highlightRange.StartInclusive;
                        ProtoCore.CodeModel.CodePoint endExclusive = core.DebugProps.highlightRange.EndExclusive;

                        EmitCall(procNode.procId, type, depth, startInclusive.LineNo, startInclusive.CharNo, endExclusive.LineNo, endExclusive.CharNo, procNode.pc);
                    }
                    // Use startCol and endCol of binary expression node containing function call except if it's a setter
                    else if (bnode != null && !procNode.name.StartsWith(Constants.kSetterPrefix))
                    {
                        EmitCall(procNode.procId, type, depth, bnode.line, bnode.col, bnode.endLine, bnode.endCol, procNode.pc);
                    }
                    else
                    {
                        EmitCall(procNode.procId, type, depth, funcCall.line, funcCall.col, funcCall.endLine, funcCall.endCol, procNode.pc);
                    }

                    // The function return value
                    EmitInstrConsole(ProtoCore.DSASM.kw.push, ProtoCore.DSASM.kw.regRX);
                    StackValue opReturn = StackValue.BuildRegister(Registers.RX);
                    EmitPush(opReturn);

                    if (dotCallType.UID != (int)PrimitiveType.kTypeVar)
                    {
                        inferedType.UID = dotCallType.UID;
                    }
                }
            }
            else
            {
                if (depth <= 0 && procName != ProtoCore.DSASM.Constants.kFunctionPointerCall)
                {
                    string property;
                    if (CoreUtils.TryGetPropertyName(procName, out property))
                    {
                        string message = String.Format(ProtoCore.BuildData.WarningMessage.kPropertyNotFound, property);
                        buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kPropertyNotFound, message, core.CurrentDSFileName, funcCall.line, funcCall.col);
                    }
                    else
                    {
                        string message = String.Format(ProtoCore.BuildData.WarningMessage.kMethodNotFound, procName);
                        buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kFunctionNotFound, message, core.CurrentDSFileName, funcCall.line, funcCall.col);
                    }

                    inferedType.UID = (int)PrimitiveType.kTypeNull;
                    EmitPushNull();
                }
                else
                {
                    if (procName == ProtoCore.DSASM.Constants.kFunctionPointerCall && depth == 0)
                    {
                        ProtoCore.DSASM.DynamicFunctionNode dynamicFunctionNode = new ProtoCore.DSASM.DynamicFunctionNode(procName, arglist, lefttype);
                        core.DynamicFunctionTable.functionTable.Add(dynamicFunctionNode);
                        var iNode = nodeBuilder.BuildIdentfier(funcCall.Function.Name);
                        EmitIdentifierNode(iNode, ref inferedType);
                    }
                    else
                    {
                        ProtoCore.DSASM.DynamicFunctionNode dynamicFunctionNode = new ProtoCore.DSASM.DynamicFunctionNode(funcCall.Function.Name, arglist, lefttype);
                        core.DynamicFunctionTable.functionTable.Add(dynamicFunctionNode);
                    }
                    // The function call
                    EmitInstrConsole(ProtoCore.DSASM.kw.callr, funcCall.Function.Name + "[dynamic]");
                    EmitDynamicCall(core.DynamicFunctionTable.functionTable.Count - 1, globalClassIndex, depth, funcCall.line, funcCall.col, funcCall.endLine, funcCall.endCol);

                    // The function return value
                    EmitInstrConsole(ProtoCore.DSASM.kw.push, ProtoCore.DSASM.kw.regRX);
                    StackValue opReturn = StackValue.BuildRegister(Registers.RX);
                    EmitPush(opReturn);

                    //assign inferedType to var
                    inferedType.UID = (int)PrimitiveType.kTypeVar;
                }
            }
            return procNode;
        }

        /*
        proc dfs_ssa_identlist(node)
            if node is not null
                dfs_ssa_identlist(node.left)
            end
            if node is functioncall
                foreach arg in functioncallArgs
	                def ssastack[]
	                def astlist[]
                    DFSEmit_SSA_AST(ref arg, ssastack, astlist)
                end
            end
        end
        */

        private BinaryExpressionNode BuildSSAIdentListAssignmentNode(IdentifierListNode identList)
        {
            // Build the final binary expression 
            BinaryExpressionNode bnode = new BinaryExpressionNode();
            bnode.Optr = ProtoCore.DSASM.Operator.assign;

            // Left node
            var identNode = nodeBuilder.BuildIdentfier(ProtoCore.Utils.CoreUtils.BuildSSATemp(core));
            bnode.LeftNode = identNode;

            //Right node
            bnode.RightNode = identList;
            bnode.isSSAAssignment = true;

            return bnode;
        }

        private void DfsSSAIeentList(AssociativeNode node, ref Stack<AssociativeNode> ssaStack, ref List<AssociativeNode> astlist)
        {
            if (node is IdentifierListNode)
            {
                IdentifierListNode listNode = node as IdentifierListNode;

                bool isSingleDot = !(listNode.LeftNode is IdentifierListNode) && !(listNode.RightNode is IdentifierListNode);
                if (isSingleDot)
                {
                    BinaryExpressionNode bnode = BuildSSAIdentListAssignmentNode(listNode);
                    astlist.Add(bnode);
                    ssaStack.Push(bnode);
                }
                else
                {
                    DfsSSAIeentList(listNode.LeftNode, ref ssaStack, ref astlist);

                    IdentifierListNode newListNode = node as IdentifierListNode;
                    newListNode.Optr = Operator.dot;

                    AssociativeNode leftnode = ssaStack.Pop();
                    Validity.Assert(leftnode is BinaryExpressionNode);

                    newListNode.LeftNode = (leftnode as BinaryExpressionNode).LeftNode;
                    newListNode.RightNode = listNode.RightNode;

                    BinaryExpressionNode bnode = BuildSSAIdentListAssignmentNode(newListNode);
                    astlist.Add(bnode);
                    ssaStack.Push(bnode);
                    
                }
            }
            else if (node is FunctionCallNode)
            {
                FunctionCallNode fcNode = node as FunctionCallNode;
                for (int idx = 0; idx < fcNode.FormalArguments.Count; idx++)
                {
                    AssociativeNode arg = fcNode.FormalArguments[idx];

                    Stack<AssociativeNode> ssaStack1 = new Stack<AssociativeNode>();
                    DFSEmitSSA_AST(arg, ssaStack1, ref astlist);
                    AssociativeNode argNode = ssaStack.Pop();
                    fcNode.FormalArguments[idx] =  argNode is BinaryExpressionNode ? (argNode as BinaryExpressionNode).LeftNode : argNode;
                }
            }
        }

        //
        //  proc SSAIdentList(node, ssastack, ast)
        //  {
        //      if node is ident
        //          t = SSATemp()
        //          tmpIdent = new Ident(t)
        //          binexpr = new BinaryExpr(tmpIdent, node)
        //          ast.push(binexpr)
        //          ssastack.push(tmpIdent)
        //      else if node is identlist
        //          SSAIdentList(node.left, ssastack, ast)
        //          rhs = new identlist(new Ident(ssastack.pop), node.right)
        //          t = SSATemp()
        //          tmpIdent = new Ident(t) 
        //          binexpr = new BinaryExpr(tmpIdent, rhs) 
        //          ast.push(binexpr)
        //          ssastack.push(tmpIdent) 
        //      end
        //  }
        //

        private void SSAIdentList(AssociativeNode node, ref Stack<AssociativeNode> ssaStack, ref List<AssociativeNode> astlist)
        {
            if (node is IdentifierNode)
            {
                IdentifierNode ident = node as IdentifierNode;
                if (null == ident.ArrayDimensions)
                {
                    // Build the temp pointer
                    BinaryExpressionNode bnode = new BinaryExpressionNode();
                    bnode.Optr = ProtoCore.DSASM.Operator.assign;
                    bnode.isSSAAssignment = true;
                    bnode.isSSAPointerAssignment = true;

                    // Left node
                    var identNode = nodeBuilder.BuildIdentfier(ProtoCore.Utils.CoreUtils.BuildSSATemp(core));
                    (identNode as IdentifierNode).ReplicationGuides = GetReplicationGuidesFromASTNode(ident);
                    bnode.LeftNode = identNode;

                    // Right node
                    bnode.RightNode = ident;

                    astlist.Add(bnode);
                    ssaStack.Push(bnode);
                }
                else
                {
                    EmitSSAArrayIndex(ident, ssaStack, ref astlist, true);
                }

            }
            else if (node is ExprListNode)
            {
                //ExprListNode exprList = node as ExprListNode;
                DFSEmitSSA_AST(node, ssaStack, ref astlist);
            }
            else if (node is FunctionCallNode)
            {
                FunctionCallNode fcall = node as FunctionCallNode;
                if (null == fcall.ArrayDimensions)
                {
                    // Build the temp pointer
                    BinaryExpressionNode bnode = new BinaryExpressionNode();
                    bnode.Optr = ProtoCore.DSASM.Operator.assign;
                    bnode.isSSAAssignment = true;
                    bnode.isSSAPointerAssignment = true;

                    // Left node
                    var identNode = nodeBuilder.BuildIdentfier(ProtoCore.Utils.CoreUtils.BuildSSATemp(core));
                    (identNode as IdentifierNode).ReplicationGuides = fcall.ReplicationGuides;
                    bnode.LeftNode = identNode;

                    // Right node
                    bnode.RightNode = fcall;


                    astlist.Add(bnode);
                    ssaStack.Push(bnode);
                }
                else
                {
                    EmitSSAArrayIndex(fcall, ssaStack, ref astlist, true);
                }

            }
            else if (node is IdentifierListNode)
            {
                IdentifierListNode identList = node as IdentifierListNode;

                //Check if the LeftNode for given IdentifierList represents a class.
                string[] classNames = this.core.ClassTable.GetAllMatchingClasses(identList.LeftNode.ToString());
                if (classNames.Length > 1)
                {
                    string message = string.Format(WarningMessage.kMultipleSymbolFound, identList.LeftNode.ToString(), classNames[0]);
                    for(int i = 1; i < classNames.Length; ++i)
                        message += ", " + classNames[i];
                    this.core.BuildStatus.LogWarning(WarningID.kMultipleSymbolFound, message);
                }

                if(classNames.Length == 1)
                {
                    var leftNode = nodeBuilder.BuildIdentfier(classNames[0]);
                    SSAIdentList(leftNode, ref ssaStack, ref astlist);
                }
                else
                {
                    // Recursively traversse the left of the ident list
                    SSAIdentList(identList.LeftNode, ref ssaStack, ref astlist);
                }

                // Build the rhs identifier list containing the temp pointer
                IdentifierListNode rhsIdentList = new IdentifierListNode();
                rhsIdentList.Optr = Operator.dot;

                AssociativeNode lhsNode = ssaStack.Pop();
                if (lhsNode is BinaryExpressionNode)
                {
                    rhsIdentList.LeftNode = (lhsNode as BinaryExpressionNode).LeftNode;
                }
                else
                {
                    rhsIdentList.LeftNode = lhsNode;
                }

                ArrayNode arrayDimension = null;

                AssociativeNode rnode = null;
                if (identList.RightNode is IdentifierNode)
                {
                    IdentifierNode identNode = identList.RightNode as IdentifierNode;
                    arrayDimension = identNode.ArrayDimensions;
                    rnode = identNode;
                }
                else if (identList.RightNode is FunctionCallNode)
                {
                    FunctionCallNode fcNode = new FunctionCallNode(identList.RightNode as FunctionCallNode);
                    arrayDimension = fcNode.ArrayDimensions;

                    List<AssociativeNode> astlistArgs = new List<AssociativeNode>();
                    for (int idx = 0; idx < fcNode.FormalArguments.Count; idx++)
                    {
                        AssociativeNode arg = fcNode.FormalArguments[idx];

                        DFSEmitSSA_AST(arg, ssaStack, ref astlistArgs);

                        AssociativeNode argNode = ssaStack.Pop();
                        if (argNode is BinaryExpressionNode)
                        {
                            BinaryExpressionNode argBinaryExpr = argNode as BinaryExpressionNode;
                            (argBinaryExpr.LeftNode as IdentifierNode).ReplicationGuides = GetReplicationGuidesFromASTNode(arg);

                            fcNode.FormalArguments[idx] = argBinaryExpr.LeftNode;
                        }
                        else
                        {
                            fcNode.FormalArguments[idx] = argNode;
                        }
                        astlist.AddRange(astlistArgs);
                        astlistArgs.Clear();
                    }

                    astlist.AddRange(astlistArgs);
                    rnode = fcNode;
                }
                else
                {
                    Validity.Assert(false);
                }

                Validity.Assert(null != rnode);
                rhsIdentList.RightNode = rnode;

                if (null == arrayDimension)
                {
                    // New SSA expr for the current dot call
                    string ssatemp = ProtoCore.Utils.CoreUtils.BuildSSATemp(core);
                    var tmpIdent = nodeBuilder.BuildIdentfier(ssatemp);
                    BinaryExpressionNode bnode = new BinaryExpressionNode(tmpIdent, rhsIdentList, Operator.assign);
                    bnode.isSSAPointerAssignment = true;
                    astlist.Add(bnode);
                    //ssaStack.Push(tmpIdent);
                    ssaStack.Push(bnode);
                }
                else
                {
                    EmitSSAArrayIndex(rhsIdentList, ssaStack, ref astlist, true);
                }
            }
        }

        /// <summary>
        /// This function applies SSA tranform to an array indexed identifier or identifier list
        /// 
        /// The transform applies as such:
        ///     i = 0
        ///     j = 1
        ///     x = a[i][j]
        ///     
        ///     t0 = a
        ///     t1 = i
        ///     t2 = t0[t1]
        ///     t3 = j
        ///     t4 = t2[t3]
        ///     x = t4
        /// 
        /// </summary>
        /// <param name="idendNode"></param>
        /// <param name="ssaStack"></param>
        /// <param name="astlist"></param>
        private void EmitSSAArrayIndex(AssociativeNode node, Stack<AssociativeNode> ssaStack, ref List<AssociativeNode> astlist, bool isSSAPointerAssignment = false)
        {
            //
            // Build the first SSA binary assignment of the identifier without its indexing
            //  x = a[i][j] -> t0 = a
            //
            BinaryExpressionNode bnode = new BinaryExpressionNode();
            bnode.Optr = ProtoCore.DSASM.Operator.assign;

            // Left node
            string ssaTempName = ProtoCore.Utils.CoreUtils.BuildSSATemp(core);
            var tmpName = nodeBuilder.BuildIdentfier(ssaTempName);
            bnode.LeftNode = tmpName;
            bnode.isSSAAssignment = true;
            bnode.isSSAPointerAssignment = isSSAPointerAssignment;
            bnode.isSSAFirstAssignment = true;

            IdentifierNode identNode = null;
            ArrayNode arrayDimensions = null;
            string firstVarName = string.Empty;
            if (node is IdentifierNode)
            {
                identNode = node as IdentifierNode;

                // Right node - Array indexing will be applied to this new identifier
                firstVarName = identNode.Name;
                bnode.RightNode = nodeBuilder.BuildIdentfier(firstVarName);
                ProtoCore.Utils.CoreUtils.CopyDebugData(bnode.RightNode, node);

                // Get the array dimensions of this node
                arrayDimensions = identNode.ArrayDimensions;


                // Associate the first pointer with each SSA temp
                // It is acceptable to store firstVar even if it is not a pointer as ResolveFinalNodeRefs() will just ignore this entry 
                // if this ssa temp is not treated as a pointer inside the function
                if (!ssaTempToFirstPointerMap.ContainsKey(ssaTempName))
                {
                    ssaTempToFirstPointerMap.Add(ssaTempName, firstVarName);
                }
            }
            else if (node is FunctionCallNode)
            {
                FunctionCallNode fcall = node as FunctionCallNode;
                Validity.Assert(fcall.Function is IdentifierNode);
                identNode = fcall.Function as IdentifierNode;

                // Assign the function array and guide properties to the new ident node
                identNode.ArrayDimensions = fcall.ArrayDimensions;
                identNode.ReplicationGuides = fcall.ReplicationGuides;

                // Right node - Remove the function array indexing.
                // The array indexing to this function will be applied downstream 
                fcall.ArrayDimensions = null;
                bnode.RightNode = fcall;
                //ProtoCore.Utils.CoreUtils.CopyDebugData(bnode.RightNode, node);

                // Get the array dimensions of this node
                arrayDimensions = identNode.ArrayDimensions;
            }
            else if (node is IdentifierListNode)
            {
                // Apply array indexing to the right node of the ident list
                //      a = p.x[i] -> apply it to x
                IdentifierListNode identList = node as IdentifierListNode;
                AssociativeNode rhsNode = identList.RightNode;

                if (rhsNode is IdentifierNode)
                {
                    identNode = rhsNode as IdentifierNode;

                    // Replace the indexed identifier with a new ident with the same name
                    //      i.e. replace x[i] with x
                    AssociativeNode nonIndexedIdent = nodeBuilder.BuildIdentfier(identNode.Name);
                    identList.RightNode = nonIndexedIdent;

                    // Get the array dimensions of this node
                    arrayDimensions = identNode.ArrayDimensions;
                }
                else if (rhsNode is FunctionCallNode)
                {
                    FunctionCallNode fcall = rhsNode as FunctionCallNode;
                    identNode = fcall.Function as IdentifierNode;

                    AssociativeNode newCall = nodeBuilder.BuildFunctionCall(identNode.Name, fcall.FormalArguments);

                    // Assign the function array and guide properties to the new ident node
                    identNode.ArrayDimensions = fcall.ArrayDimensions;
                    identNode.ReplicationGuides = fcall.ReplicationGuides;

                    identList.RightNode = newCall;

                    // Get the array dimensions of this node
                    arrayDimensions = identNode.ArrayDimensions;
                }
                else
                {
                    Validity.Assert(false, "This token is not indexable");
                }

                // Right node
                bnode.RightNode = identList;
                //ProtoCore.Utils.CoreUtils.CopyDebugData(bnode.RightNode, node);

                bnode.isSSAPointerAssignment = true;
            }
            else if (node is GroupExpressionNode)
            {
                GroupExpressionNode groupExpr = node as GroupExpressionNode;
                DFSEmitSSA_AST(groupExpr.Expression, ssaStack, ref astlist);

                arrayDimensions = groupExpr.ArrayDimensions;
            }
            else
            {
                Validity.Assert(false);
            }

            // Push the first SSA stmt
            astlist.Add(bnode);
            ssaStack.Push(bnode);

            // Traverse the array index
            //      t1 = i
            List<AssociativeNode> arrayDimASTList = new List<AssociativeNode>();
            DFSEmitSSA_AST(arrayDimensions.Expr, ssaStack, ref arrayDimASTList);
            astlist.AddRange(arrayDimASTList);
            arrayDimASTList.Clear();

            //
            // Build the indexing statement
            //      t2 = t0[t1]
            BinaryExpressionNode indexedStmt = new BinaryExpressionNode();
            indexedStmt.Optr = ProtoCore.DSASM.Operator.assign;


            // Build the left node of the indexing statement
#region SSA_INDEX_STMT_LEFT
            ssaTempName = ProtoCore.Utils.CoreUtils.BuildSSATemp(core);
            AssociativeNode tmpIdent = nodeBuilder.BuildIdentfier(ssaTempName);
            Validity.Assert(null != tmpIdent);
            indexedStmt.LeftNode = tmpIdent;
            //ProtoCore.Utils.CoreUtils.CopyDebugData(bnode, node);



            // Associate the first pointer with each SSA temp
            // It is acceptable to store firstVar even if it is not a pointer as ResolveFinalNodeRefs() will just ignore this entry 
            // if this ssa temp is not treated as a pointer inside the function
            if (!string.IsNullOrEmpty(firstVarName))
            {
                if (!ssaTempToFirstPointerMap.ContainsKey(ssaTempName))
                {
                    ssaTempToFirstPointerMap.Add(ssaTempName, firstVarName);
                }
            }

#endregion

            // Build the right node of the indexing statement
#region SSA_INDEX_STMT_RIGHT
            
            ArrayNode arrayNode = null;

            // pop off the dimension
            AssociativeNode dimensionNode = ssaStack.Pop();
            if (dimensionNode is BinaryExpressionNode)
            {
                arrayNode = new ArrayNode((dimensionNode as BinaryExpressionNode).LeftNode, null);
            }
            else
            {
                arrayNode = new ArrayNode(dimensionNode, null);
            }

            // pop off the prev SSA variable
            AssociativeNode prevSSAStmt = ssaStack.Pop();

            // Assign the curent dimension to the prev SSA variable
            Validity.Assert(prevSSAStmt is BinaryExpressionNode);
            AssociativeNode rhsIdent = nodeBuilder.BuildIdentfier((prevSSAStmt as BinaryExpressionNode).LeftNode.Name);
            (rhsIdent as IdentifierNode).ArrayDimensions = arrayNode;

            // Right node of the indexing statement
            indexedStmt.RightNode = rhsIdent;

            astlist.Add(indexedStmt);
            ssaStack.Push(indexedStmt);


            // Traverse the next dimension
            //      [j]
            if (null != arrayDimensions.Type)
            {
                DFSEmitSSA_AST(arrayDimensions.Type, ssaStack, ref arrayDimASTList);
                astlist.AddRange(arrayDimASTList);
                arrayDimASTList.Clear();
            }
#endregion
        }

        /// <summary>
        /// This helper function extracts the replication guide data from the AST node
        /// Merge this function with GetReplicationGuides
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private List<AssociativeNode> GetReplicationGuidesFromASTNode(AssociativeNode node)
        {
            List<AssociativeNode> replicationGuides = null;
            if (node is IdentifierNode)
            {
                replicationGuides = (node as IdentifierNode).ReplicationGuides;
            }
            else if (node is IdentifierListNode)
            {
                IdentifierListNode identList = node as IdentifierListNode;

                // Get the replication guide and append it to the last identifier
                if (identList.RightNode is IdentifierNode)
                {
                    replicationGuides = (identList.RightNode as IdentifierNode).ReplicationGuides;
                }
                else if (identList.RightNode is FunctionCallNode)
                {
                    replicationGuides = (identList.RightNode as FunctionCallNode).ReplicationGuides;
                }
                else
                {
                    Validity.Assert(false);
                }
            }
            else if (node is FunctionDotCallNode)
            {
                FunctionDotCallNode dotCall = node as FunctionDotCallNode;

                // Get the replication guide from the dotcall
                IdentifierNode functionCallIdent = dotCall.FunctionCall.Function as IdentifierNode;
                Validity.Assert(null != functionCallIdent);
                replicationGuides = functionCallIdent.ReplicationGuides;
            }
            else if (node is FunctionCallNode)
            {
                FunctionCallNode functionCall = node as FunctionCallNode;

                // Get the replication guide from the dotcall
                replicationGuides = functionCall.ReplicationGuides;
            }
            else if (node is RangeExprNode)
            {
                RangeExprNode rangeExpr = node as RangeExprNode;

                // Get the replication guide from the dotcall
                replicationGuides = rangeExpr.ReplicationGuides;
            }
            else if (node is InlineConditionalNode)
            {
                // TODO Jun: Parser should support replication guides on an entire inline conditional
                InlineConditionalNode inlineCondition = node as InlineConditionalNode;
                replicationGuides = null;
            }
            else if (node is ExprListNode)
            {
                ExprListNode exprlistNode = node as ExprListNode;
                replicationGuides = exprlistNode.ReplicationGuides;
            }
            else if (node is GroupExpressionNode)
            {
                GroupExpressionNode groupExprNode = node as GroupExpressionNode;
                replicationGuides = groupExprNode.ReplicationGuides;
            }
            else
            {
                // A parser error has occured if a replication guide gets attached to any AST besides"
                // Ident, identlist, functioncall and functiondotcall
                Validity.Assert(false, "This AST node should not have a replication guide.");
            }
            return replicationGuides;
        }


        /*
        proc DFSEmit_SSA_AST(node, ssastack[], astlist[])
            if node is binary expression
                def bnode 
                if node.optr is assign op 
                    bnode.optr = node.optr 
                    bnode.left = node.left
                    DFSEmit_SSA_AST(node.right, ssastack, astlist) 
                    bnode.right = ssastack.pop()
                else 
                    def tnode
                    tnode.optr = node.optr
        
                    DFSEmit_SSA_AST(node.left, ssastack, astlist) 
                    DFSEmit_SSA_AST(node.right, ssastack, astlist) 
          
                    def lastnode = ssastack.pop() 
                    def prevnode = ssastack.pop()
         
                    if prevnode is binary
                        tnode.left = prevnode.left
                    else
                        tnode.left = prevnode
                    end

                    if lastnode is binary
                        tnode.right = lastnode.left
                    else
                        tnode.right = lastnode
                    end

                    bnode.optr = ?=? 
                    bnode.left = GetSSATemp()
                    bnode.right = tnode
                end
                astlist.append(bnode)
                ssastack.push(bnode)
            else if node is identifier
                def bnode
                bnode.optr = ?=?
                bnode.left = GetSSATemp()
                bnode.right = node
                astlist.append(bnode) 
                ssastack.push(bnode)
            else
                ssastack.push(node)
            end
        end     
        */   
        private void DFSEmitSSA_AST(AssociativeNode node, Stack<AssociativeNode> ssaStack, ref List<AssociativeNode> astlist)
        {
            Validity.Assert(null != astlist && null != ssaStack);
            if (node is BinaryExpressionNode)
            {
                BinaryExpressionNode astBNode = node as BinaryExpressionNode;
                BinaryExpressionNode bnode = new BinaryExpressionNode();
                if (ProtoCore.DSASM.Operator.assign == astBNode.Optr)
                {
                    bnode.Optr = ProtoCore.DSASM.Operator.assign;
                    bnode.LeftNode = astBNode.LeftNode;
                    DFSEmitSSA_AST(astBNode.RightNode, ssaStack, ref astlist);
                    AssociativeNode assocNode = ssaStack.Pop();

                    if (assocNode is BinaryExpressionNode)
                    {
                        bnode.RightNode = (assocNode as BinaryExpressionNode).LeftNode;
                    }
                    else
                    {
                        bnode.RightNode = assocNode;
                    }

                    bnode.isSSAAssignment = false;
                }
                else
                { 
                    BinaryExpressionNode tnode = new BinaryExpressionNode();
                    tnode.Optr = astBNode.Optr;

                    DFSEmitSSA_AST(astBNode.LeftNode, ssaStack, ref astlist);
                    DFSEmitSSA_AST(astBNode.RightNode, ssaStack, ref astlist);

                    AssociativeNode lastnode = ssaStack.Pop();
                    AssociativeNode prevnode = ssaStack.Pop();
                    tnode.LeftNode = prevnode is BinaryExpressionNode ? (prevnode as BinaryExpressionNode).LeftNode : prevnode;
                    tnode.RightNode = lastnode is BinaryExpressionNode ? (lastnode as BinaryExpressionNode).LeftNode : lastnode;

                    bnode.Optr = ProtoCore.DSASM.Operator.assign; 

                    // Left node
                    var identNode = nodeBuilder.BuildIdentfier(ProtoCore.Utils.CoreUtils.BuildSSATemp(core));
                    bnode.LeftNode = identNode;

                    // Right node
                    bnode.RightNode = tnode;

                    bnode.isSSAAssignment = true;
                }
                astlist.Add(bnode);
                ssaStack.Push(bnode);
            }
            else if (node is ArrayNode)
            {
                ArrayNode arrayNode = node as ArrayNode;
                DFSEmitSSA_AST(arrayNode.Expr, ssaStack, ref astlist);

                BinaryExpressionNode bnode = new BinaryExpressionNode();
                bnode.Optr = ProtoCore.DSASM.Operator.assign;

                // Left node
                AssociativeNode tmpIdent = nodeBuilder.BuildIdentfier(ProtoCore.Utils.CoreUtils.BuildSSATemp(core));
                Validity.Assert(null != tmpIdent);
                bnode.LeftNode = tmpIdent;

                // pop off the dimension
                AssociativeNode dimensionNode = ssaStack.Pop();
                ArrayNode currentDimensionNode = null;
                if (dimensionNode is BinaryExpressionNode)
                {
                    currentDimensionNode = new ArrayNode((dimensionNode as BinaryExpressionNode).LeftNode, null);
                }
                else
                {
                    currentDimensionNode = new ArrayNode(dimensionNode, null);
                }

                // Pop the prev SSA node where the current dimension will apply
                AssociativeNode nodePrev = ssaStack.Pop();
                if (nodePrev is BinaryExpressionNode)
                {
                    AssociativeNode rhsIdent = nodeBuilder.BuildIdentfier((nodePrev as BinaryExpressionNode).LeftNode.Name);
                    (rhsIdent as IdentifierNode).ArrayDimensions = currentDimensionNode;

                    bnode.RightNode = rhsIdent;

                    astlist.Add(bnode);
                    ssaStack.Push(bnode);

                    if (null != arrayNode.Type)
                    {
                        DFSEmitSSA_AST(arrayNode.Type, ssaStack, ref astlist);
                    }
                }
                else if (nodePrev is IdentifierListNode)
                {
                    IdentifierNode iNode = (nodePrev as IdentifierListNode).RightNode as IdentifierNode;
                    Validity.Assert(null != iNode);
                    iNode.ArrayDimensions = currentDimensionNode;


                    if (null != arrayNode.Type)
                    {
                        DFSEmitSSA_AST(arrayNode.Type, ssaStack, ref astlist);
                    }
                }
                else
                {
                    Validity.Assert(false);
                }

                //bnode.RightNode = rhsIdent;

                //astlist.Add(bnode);
                //ssaStack.Push(bnode);
            }
            else if (node is IdentifierNode)
            {
                IdentifierNode ident = node as IdentifierNode;

                if (core.Options.FullSSA)
                {
                    string ssaTempName = string.Empty;
                    if (null == ident.ArrayDimensions)
                    {
                        BinaryExpressionNode bnode = new BinaryExpressionNode();
                        bnode.Optr = ProtoCore.DSASM.Operator.assign;

                        // Left node
                        ssaTempName = ProtoCore.Utils.CoreUtils.BuildSSATemp(core);
                        var identNode = nodeBuilder.BuildIdentfier(ssaTempName);
                        bnode.LeftNode = identNode;

                        // Right node
                        bnode.RightNode = ident;

                        bnode.isSSAAssignment = true;

                        astlist.Add(bnode);
                        ssaStack.Push(bnode);

                        // Associate the first pointer with each SSA temp
                        // It is acceptable to store firstVar even if it is not a pointer as ResolveFinalNodeRefs() will just ignore this entry 
                        // if this ssa temp is not treated as a pointer inside the function
                        string firstVar = ident.Name;
                        ssaTempToFirstPointerMap.Add(ssaTempName, firstVar);
                    }
                    else
                    {
                        EmitSSAArrayIndex(ident, ssaStack, ref astlist);
                    }
                }
                else
                {
                    BinaryExpressionNode bnode = new BinaryExpressionNode();
                    bnode.Optr = ProtoCore.DSASM.Operator.assign;

                    // Left node
                    var identNode = nodeBuilder.BuildIdentfier(ProtoCore.Utils.CoreUtils.BuildSSATemp(core));
                    bnode.LeftNode = identNode;

                    // Right node
                    bnode.RightNode = ident;


                    bnode.isSSAAssignment = true;

                    astlist.Add(bnode);
                    ssaStack.Push(bnode);
                }
            }
            else if (node is FunctionCallNode || node is FunctionDotCallNode)
            {
                FunctionCallNode fcNode = null;
                if (node is FunctionCallNode)
                {
                    fcNode = new FunctionCallNode(node as FunctionCallNode);

                    List<AssociativeNode> astlistArgs = new List<AssociativeNode>();

                    for (int idx = 0; idx < fcNode.FormalArguments.Count; idx++)
                    {
                        AssociativeNode arg = fcNode.FormalArguments[idx];
                        DFSEmitSSA_AST(arg, ssaStack, ref astlistArgs);
                        AssociativeNode argNode = ssaStack.Pop();

                        if (argNode is BinaryExpressionNode)
                        {
                            BinaryExpressionNode argBinaryExpr = argNode as BinaryExpressionNode;
                            //(argBinaryExpr.LeftNode as IdentifierNode).ReplicationGuides = GetReplicationGuidesFromASTNode(argBinaryExpr.RightNode);
                            (argBinaryExpr.LeftNode as IdentifierNode).ReplicationGuides = GetReplicationGuidesFromASTNode(arg);
                            
                            fcNode.FormalArguments[idx] = argBinaryExpr.LeftNode;
                        }
                        else
                        {
                            fcNode.FormalArguments[idx] = argNode;
                        }
                        astlist.AddRange(astlistArgs);
                        astlistArgs.Clear();
                    }
                }

                BinaryExpressionNode bnode = new BinaryExpressionNode();
                bnode.Optr = ProtoCore.DSASM.Operator.assign;

                // Left node
                var identNode = nodeBuilder.BuildIdentfier(ProtoCore.Utils.CoreUtils.BuildSSATemp(core));
                bnode.LeftNode = identNode;

                // Store the replication guide from the function call to the temp
                (identNode as IdentifierNode).ReplicationGuides = GetReplicationGuidesFromASTNode(fcNode);

                //Right node
                bnode.RightNode = fcNode;
                bnode.isSSAAssignment = true;


                astlist.Add(bnode);
                ssaStack.Push(bnode);
            }
            else if (node is IdentifierListNode)
            {
                SSAIdentList(node, ref ssaStack, ref astlist);
                AssociativeNode lastNode = null;
                Validity.Assert(astlist.Count > 0);


                // Get the last identifierlist node and set its flag
                // This is required to reset the pointerlist for every identifierlist traversed
                // i.e. 
                //      a = p.x
                //      reset the flag after traversing p.x
                //
                //      b = p.x + g.y    
                //      reset the flag after traversing p.x and p.y
                //
                int lastIndex = astlist.Count - 1;
                for (int n = lastIndex; n >= 0 ; --n)
                {
                    lastNode = astlist[n];
                    Validity.Assert(lastNode is BinaryExpressionNode);
                    
                    BinaryExpressionNode bnode = lastNode as BinaryExpressionNode;
                    Validity.Assert(bnode.Optr == Operator.assign);
                    if (bnode.RightNode is IdentifierListNode)
                    {
                        IdentifierListNode identList = bnode.RightNode as IdentifierListNode;
                        identList.isLastSSAIdentListFactor = true;
                        break;
                    }
                }
                
                // Assign the first ssa assignment flag
                BinaryExpressionNode firstBNode = astlist[0] as BinaryExpressionNode;
                Validity.Assert(null != firstBNode);
                Validity.Assert(firstBNode.Optr == Operator.assign);
                firstBNode.isSSAFirstAssignment = true;

                //
                // The first pointer is the lhs of the next dotcall
                //      a = x.y.z
                //      t0 = x      -> 'x' is the lhs of the first dotcall (x.y)
                //      t1 = t0.y
                //      t2 = t1.z
                //      a = t2
                //

                IdentifierNode lhsIdent = null;
                if (firstBNode.RightNode is IdentifierNode)
                {
                    // In this case the rhs of the ident list is an ident
                    // Get the ident name
                    lhsIdent = (firstBNode.RightNode as IdentifierNode);
                }
                else if(firstBNode.RightNode is FunctionCallNode)
                {
                    // In this case the rhs of the ident list is a function
                    // Get the function name
                    lhsIdent = (firstBNode.RightNode as FunctionCallNode).Function as IdentifierNode;
                }   
                else
                {

                    lhsIdent = null;
                }

                IdentifierNode firstPointer = lhsIdent;

                // Get the first pointer name
                //Validity.Assert(firstBNode.RightNode is IdentifierNode);
                //string firstPtrName = (firstBNode.RightNode as IdentifierNode).Name;

                //=========================================================
                //
                // 1. Backtrack and convert all identlist nodes to dot calls
                //    This can potentially be optimized by performing the dotcall transform in SSAIdentList
                //
                // 2. Associate the first pointer with each SSA temp
                //
                //=========================================================
                
                //for (int n = lastIndex; n >= 0; --n)
                AssociativeNode prevNode = null;
                for (int n = 0; n <= lastIndex; n++)
                {
                    lastNode = astlist[n];

                    BinaryExpressionNode bnode = lastNode as BinaryExpressionNode;
                    Validity.Assert(bnode.Optr == Operator.assign);

                    // Get the ssa temp name
                    Validity.Assert(bnode.LeftNode is IdentifierNode);
                    string ssaTempName = (bnode.LeftNode as IdentifierNode).Name;
                    Validity.Assert(CoreUtils.IsSSATemp(ssaTempName));

                    if (bnode.RightNode is IdentifierListNode)
                    {
                        IdentifierListNode identList = bnode.RightNode as IdentifierListNode;
                        if (identList.RightNode is IdentifierNode)
                        {
                            IdentifierNode identNode = identList.RightNode as IdentifierNode;

                            ProtoCore.AST.AssociativeAST.FunctionCallNode rcall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
                            rcall.Function = new IdentifierNode(identNode);
                            rcall.Function.Name = ProtoCore.DSASM.Constants.kGetterPrefix + rcall.Function.Name;

                            ProtoCore.Utils.CoreUtils.CopyDebugData(identList.LeftNode, lhsIdent);
                            FunctionDotCallNode dotCall = ProtoCore.Utils.CoreUtils.GenerateCallDotNode(identList.LeftNode, rcall, core);
                            dotCall.isLastSSAIdentListFactor = identList.isLastSSAIdentListFactor;
                            bnode.RightNode = dotCall;
                            ProtoCore.Utils.CoreUtils.CopyDebugData(bnode, lhsIdent);

                            //
                            // Set the real lhs (first pointer) of this dot call
                            // Do this only if the lhs of the ident list was an identifier
                            //      A.b -> prev was 'A'. It is an identifier
                            //      {A}.b -> prev was '{A}'. It is not an identifier
                            //      A().b -> prev was 'A()'. It is not an identifier
                            bool wasPreviousNodeAnIdentifier = prevNode is IdentifierNode;
                            if (wasPreviousNodeAnIdentifier)
                            {
                                dotCall.staticLHSIdent = firstPointer;
                            }
                            firstPointer = null;

                            // Update the LHS of the next dotcall
                            //      a = x.y.z
                            //      t0 = x      
                            //      t1 = t0.y   'y' is the lhs of the next dotcall (y.z)
                            //      t2 = t1.z
                            //      a = t2
                            //
                            Validity.Assert(rcall.Function is IdentifierNode);
                            lhsIdent = rcall.Function as IdentifierNode;
                        }
                        else if (identList.RightNode is FunctionCallNode)
                        {
                            FunctionCallNode fCallNode = identList.RightNode as FunctionCallNode;


                            ProtoCore.Utils.CoreUtils.CopyDebugData(identList.LeftNode, lhsIdent);
                            FunctionDotCallNode dotCall = ProtoCore.Utils.CoreUtils.GenerateCallDotNode(identList.LeftNode, fCallNode, core);
                            dotCall.isLastSSAIdentListFactor = identList.isLastSSAIdentListFactor;
                            bnode.RightNode = dotCall;

                            ProtoCore.Utils.CoreUtils.CopyDebugData(bnode, lhsIdent);

                            // Set the real lhs (first pointer) of this dot call
                            dotCall.staticLHSIdent = firstPointer;
                            firstPointer = null;


                            // Update the LHS of the next dotcall
                            //      a = x.y.z
                            //      t0 = x      
                            //      t1 = t0.y   'y' is the lhs of the next dotcall (y.z)
                            //      t2 = t1.z
                            //      a = t2
                            //
                            Validity.Assert(fCallNode.Function is IdentifierNode);
                            lhsIdent = fCallNode.Function as IdentifierNode;
                        }
                        else
                        {
                            Validity.Assert(false);
                        }
                    }
                    prevNode = bnode.RightNode;
                }


                ///////////////////////////
            }
            else if (node is ExprListNode)
            {
                ExprListNode exprList = node as ExprListNode;
                for (int n = 0; n < exprList.list.Count; n++)
                {
                    List<AssociativeNode> currentElementASTList = new List<AssociativeNode>();
                    DFSEmitSSA_AST(exprList.list[n], ssaStack, ref currentElementASTList);

                    astlist.AddRange(currentElementASTList);
                    currentElementASTList.Clear();
                    AssociativeNode argNode = ssaStack.Pop();
                    exprList.list[n] = argNode is BinaryExpressionNode ? (argNode as BinaryExpressionNode).LeftNode : argNode;
                }

                BinaryExpressionNode bnode = new BinaryExpressionNode();
                bnode.Optr = ProtoCore.DSASM.Operator.assign;

                // Left node
                var identNode = nodeBuilder.BuildIdentfier(ProtoCore.Utils.CoreUtils.BuildSSATemp(core));
                bnode.LeftNode = identNode;

                //Right node
                bnode.RightNode = exprList;
                bnode.isSSAAssignment = true;

                astlist.Add(bnode);
                ssaStack.Push(bnode);
            }
            else if (node is InlineConditionalNode)
            {
                InlineConditionalNode ilnode = node as InlineConditionalNode;

                List<AssociativeNode> inlineExpressionASTList = new List<AssociativeNode>();

                // Emit the boolean condition
                DFSEmitSSA_AST(ilnode.ConditionExpression, ssaStack, ref inlineExpressionASTList);
                AssociativeNode cexpr = ssaStack.Pop();
                ilnode.ConditionExpression = cexpr is BinaryExpressionNode ? (cexpr as BinaryExpressionNode).LeftNode : cexpr;
                astlist.AddRange(inlineExpressionASTList);
                inlineExpressionASTList.Clear();


                // Emit the True exprssion
                DFSEmitSSA_AST(ilnode.TrueExpression, ssaStack, ref inlineExpressionASTList);
                AssociativeNode trueExpr = ssaStack.Pop();
                ilnode.TrueExpression = trueExpr is BinaryExpressionNode ? (trueExpr as BinaryExpressionNode).LeftNode : trueExpr;
                astlist.AddRange(inlineExpressionASTList);
                inlineExpressionASTList.Clear();


                // Emit the True exprssion
                DFSEmitSSA_AST(ilnode.FalseExpression, ssaStack, ref inlineExpressionASTList);
                AssociativeNode falseExpr = ssaStack.Pop();
                ilnode.FalseExpression = falseExpr is BinaryExpressionNode ? (falseExpr as BinaryExpressionNode).LeftNode : falseExpr;
                astlist.AddRange(inlineExpressionASTList);
                inlineExpressionASTList.Clear();

                BinaryExpressionNode bnode = new BinaryExpressionNode();
                bnode.Optr = ProtoCore.DSASM.Operator.assign;

                // Left node
                var identNode = nodeBuilder.BuildIdentfier(ProtoCore.Utils.CoreUtils.BuildSSATemp(core));
                bnode.LeftNode = identNode;

                //Right node
                bnode.RightNode = ilnode;
                bnode.isSSAAssignment = true;

                astlist.Add(bnode);
                ssaStack.Push(bnode);
            }
            else if (node is RangeExprNode)
            {
                RangeExprNode rangeNode = node as RangeExprNode;

                DFSEmitSSA_AST(rangeNode.FromNode, ssaStack, ref astlist);
                AssociativeNode fromExpr = ssaStack.Pop();
                rangeNode.FromNode = fromExpr is BinaryExpressionNode ? (fromExpr as BinaryExpressionNode).LeftNode : fromExpr;

                DFSEmitSSA_AST(rangeNode.ToNode, ssaStack, ref astlist);
                AssociativeNode toExpr = ssaStack.Pop();
                rangeNode.ToNode = toExpr is BinaryExpressionNode ? (toExpr as BinaryExpressionNode).LeftNode : toExpr;

                if (rangeNode.StepNode != null)
                {
                    DFSEmitSSA_AST(rangeNode.StepNode, ssaStack, ref astlist);
                    AssociativeNode stepExpr = ssaStack.Pop();
                    rangeNode.StepNode = stepExpr is BinaryExpressionNode ? (stepExpr as BinaryExpressionNode).LeftNode : stepExpr;
                }

                BinaryExpressionNode bnode = new BinaryExpressionNode();
                bnode.Optr = ProtoCore.DSASM.Operator.assign;

                // Left node
                var identNode = nodeBuilder.BuildIdentfier(ProtoCore.Utils.CoreUtils.BuildSSATemp(core));
                bnode.LeftNode = identNode;

                //Right node
                bnode.RightNode = rangeNode;
                bnode.isSSAAssignment = true;

                astlist.Add(bnode);
                ssaStack.Push(bnode);
            }
            else if (node is GroupExpressionNode)
            {
                if (core.Options.FullSSA)
                {
                    GroupExpressionNode groupExpr = node as GroupExpressionNode;
                    if (null == groupExpr.ArrayDimensions)
                    {
                        DFSEmitSSA_AST(groupExpr.Expression, ssaStack, ref astlist);

                        BinaryExpressionNode bnode = new BinaryExpressionNode();
                        bnode.Optr = ProtoCore.DSASM.Operator.assign;

                        
                        // Left node
                        var identNode = nodeBuilder.BuildIdentfier(ProtoCore.Utils.CoreUtils.BuildSSATemp(core));
                        bnode.LeftNode = identNode;

                        // Right node
                        AssociativeNode groupExprBinaryStmt = ssaStack.Pop();
                        Validity.Assert(groupExprBinaryStmt is BinaryExpressionNode);
                        bnode.RightNode = (groupExprBinaryStmt as BinaryExpressionNode).LeftNode;

                        bnode.isSSAAssignment = true;

                        astlist.Add(bnode);
                        ssaStack.Push(bnode);
                    }
                    else
                    {
                        EmitSSAArrayIndex(groupExpr, ssaStack, ref astlist);
                    }
                }
                else
                {
                    // We never supported SSA on grouped expressions
                    // Keep it that way if SSA flag is off
                    ssaStack.Push(node);
                }
            }
            else
            {
                ssaStack.Push(node);
            }
        }


        /*
        proc SetExecutionFlagForNode(BinaryExpressionNode bnode, int exprUID)
            // Mapping the execution flaglist from string to exprssionID
            // Get the lhs of the node in the execFlagList ? if it exists
            kvp = context.execFlagList.GetKey(node.lhs)
            if kvp does not exist
                exprIdFlagList.push(exprUID, kvp.value)
            else
                // Update the existing entry
                exprIdFlagList[exprUID] = kvp.value
 	        end
        end

        */

        private void SetExecutionFlagForNode(BinaryExpressionNode bnode, int expressionUID)
        { 
            if (core.Options.IsDeltaExecution)
            {
                Validity.Assert(null != bnode);
                Validity.Assert(ProtoCore.DSASM.Constants.kInvalidIndex != expressionUID);
                bool executionflag = true;

                // Get the lhs of the node in the execFlagList ? if it exists
                if (context != null && 
                    context.execFlagList != null && 
                    context.execFlagList.TryGetValue(bnode.LeftNode.Name, out executionflag))
                {
                    if (context.exprExecutionFlags.ContainsKey(expressionUID))
                    {
                        context.exprExecutionFlags[expressionUID] = executionflag;
                    }
                    else
                    {
                        context.exprExecutionFlags.Add(expressionUID, executionflag);
                    }
                }
            }
        }




        /*
        def BuildSSA(newASTList[])
            foreach node in astlist
                def SSAList[]
                if node is binary expression and node.isModifier is true
                    DFSEmitSSA(node, SSAList) 
                    newASTList.insertlist(SSAList)
                else 
                    newASTList.append(node)
                end
            end
            return newASTList
        end
        */



        private List<AssociativeNode> BuildSSA(List<AssociativeNode> astList, ProtoCore.CompileTime.Context context)
        {
            if (null != astList && astList.Count > 0)
            {
                Dictionary<string, int> ssaUIDList = new Dictionary<string, int>();

                List<AssociativeNode> newAstList = new List<AssociativeNode>();

                foreach (AssociativeNode node in astList)
                {
                    List<AssociativeNode> newASTList = new List<AssociativeNode>();
                    if (node is BinaryExpressionNode)
                    {
                        BinaryExpressionNode bnode = (node as BinaryExpressionNode);
                        int generatedUID = ProtoCore.DSASM.Constants.kInvalidIndex;

                        if (core.Options.FullSSA)
                        {
                            node.IsModifier = true;
                        }

                        if (context.applySSATransform && node.IsModifier)
                        {
                            int ssaID = ProtoCore.DSASM.Constants.kInvalidIndex;
                            string name = ProtoCore.Utils.CoreUtils.GenerateIdentListNameString(bnode.LeftNode);
                            if (ssaUIDList.ContainsKey(name))
                            {
                                ssaUIDList.TryGetValue(name, out ssaID);
                                generatedUID = ssaID;
                            }
                            else
                            {
                                if (core.Options.GenerateExprID)
                                {
                                    ssaID = core.ExpressionUID++;
                                }
                                else
                                {
                                    ssaID = (node as BinaryExpressionNode).exprUID;
                                }
                                ssaUIDList.Add(name, ssaID);
                                generatedUID = ssaID;
                            }

                            Stack<AssociativeNode> ssaStack = new Stack<AssociativeNode>();
                            DFSEmitSSA_AST(node, ssaStack, ref newASTList);

                            // Set the unique expression id for this range of SSA nodes
                            foreach (AssociativeNode aNode in newASTList)
                            {
                                Validity.Assert(aNode is BinaryExpressionNode);

                                // Set the exprID of the SSA's node
                                BinaryExpressionNode ssaNode = aNode as BinaryExpressionNode;
                                ssaNode.exprUID = ssaID;
                                NodeUtils.SetNodeLocation(ssaNode, node, node);
                            }

                            // Assigne the exprID of the original node 
                            // (This is the node prior to ssa transformation)
                            bnode.exprUID = ssaID;
                            newAstList.AddRange(newASTList);
                        }
                        else
                        {
                            if (core.Options.GenerateExprID)
                            {
                                bnode.exprUID = generatedUID = core.ExpressionUID++;
                            }
                            newAstList.Add(node);
                        }

                        // TODO Jun: How can delta execution functionality be seamlessly integrated in the codegens?
                        SetExecutionFlagForNode(node as BinaryExpressionNode, generatedUID);
                    }
                    else if (node is ClassDeclNode)
                    {
                        ClassDeclNode classNode = node as ClassDeclNode;
                        foreach (AssociativeNode procNode in classNode.funclist)
                        {
                            if (procNode is FunctionDefinitionNode)
                            {
                                FunctionDefinitionNode fNode = procNode as FunctionDefinitionNode;
                                if (!fNode.IsExternLib)
                                {
                                    fNode.FunctionBody.Body = BuildSSA(fNode.FunctionBody.Body, context);
                                }
                            }
                            else if (procNode is ConstructorDefinitionNode)
                            {
                                ConstructorDefinitionNode cNode = procNode as ConstructorDefinitionNode;
                                if (!cNode.IsExternLib)
                                {
                                    cNode.FunctionBody.Body = BuildSSA(cNode.FunctionBody.Body, context);
                                }
                            }
                        }
                        newAstList.Add(classNode);
                    }
                    else if (node is FunctionDefinitionNode)
                    {
                        FunctionDefinitionNode fNode = node as FunctionDefinitionNode;
                        if (!fNode.IsExternLib)
                        {
                            fNode.FunctionBody.Body = BuildSSA(fNode.FunctionBody.Body, context);
                        }
                        newAstList.Add(node);
                    }
                    else if (node is ConstructorDefinitionNode)
                    {
                        ConstructorDefinitionNode fNode = node as ConstructorDefinitionNode;
                        if (!fNode.IsExternLib)
                        {
                            fNode.FunctionBody.Body = BuildSSA(fNode.FunctionBody.Body, context);
                        }
                        newAstList.Add(node);
                    }                    
                    else if (node is ModifierStackNode)
                    {
                        ModifierStackNode modStack = node as ModifierStackNode;

                        if (core.Options.FullSSA)
                        {
                            List<AssociativeNode> modStackNewElements = new List<AssociativeNode>();


                            bool ssaTranformAllModStackStmts = true;

                            if (ssaTranformAllModStackStmts)
                            {
                                foreach (AssociativeNode modstackNode in modStack.ElementNodes)
                                {
                                    if (modstackNode is BinaryExpressionNode)
                                    {
                                        Stack<AssociativeNode> ssaStack = new Stack<AssociativeNode>();
                                        List<AssociativeNode> elementList = new List<AssociativeNode>();

                                        DFSEmitSSA_AST(modstackNode, ssaStack, ref elementList);

                                        // Set the unique expression id for this range of SSA nodes
                                        foreach (AssociativeNode aNode in elementList)
                                        {
                                            Validity.Assert(aNode is BinaryExpressionNode);
                                            BinaryExpressionNode bnode = aNode as BinaryExpressionNode;
                                            bnode.exprUID = core.ExpressionUID;
                                            bnode.modBlkUID = core.ModifierBlockUID;
                                            ProtoCore.Utils.CoreUtils.CopyDebugData(bnode, modstackNode);
                                        }

                                        modStackNewElements.AddRange(elementList);

                                    }

                                    core.ExpressionUID++;
                                }

                                modStack.ElementNodes.Clear();
                                modStack.ElementNodes.AddRange(modStackNewElements);

                                newAstList.Add(node);
                                core.ModifierBlockUID++;
                            }
                            else
                            {
                                // Transform all identlists into dot calls
                                foreach (AssociativeNode modstackNode in modStack.ElementNodes)
                                {
                                    BinaryExpressionNode bnode = modstackNode as BinaryExpressionNode;
                                    if (bnode != null)
                                    {
                                        if (core.Options.GenerateExprID)
                                        {
                                            bnode.exprUID = core.ExpressionUID;
                                        }
                                        bnode.modBlkUID = core.ModifierBlockUID;
                                    }

                                    core.ExpressionUID++;
                                }

                                newAstList.Add(node);
                                core.ModifierBlockUID++;
                            }
                        }
                        else
                        {
                            foreach (AssociativeNode modstackNode in modStack.ElementNodes)
                            {
                                BinaryExpressionNode bnode = modstackNode as BinaryExpressionNode;
                                if (bnode != null)
                                {
                                    if (core.Options.GenerateExprID)
                                    {
                                        bnode.exprUID = core.ExpressionUID;
                                    }
                                    bnode.modBlkUID = core.ModifierBlockUID;
                                    //newAstList.Add(bnode);
                                }
                                
                                core.ExpressionUID++;
                            }
                            
                            newAstList.Add(node);
                            core.ModifierBlockUID++;
                        }
                    }
                    else
                    {
                        newAstList.Add(node);
                    }
                }
                return newAstList;
            }
            return astList;
        }

        private AssociativeNode DFSEmitSplitAssign_AST(AssociativeNode node, ref List<AssociativeNode> astList)
        {
            Validity.Assert(null != astList && null != node);

            if (node is BinaryExpressionNode)
            {
                BinaryExpressionNode bnode = node as BinaryExpressionNode;
                if (ProtoCore.DSASM.Operator.assign == bnode.Optr)
                {
                    AssociativeNode lastNode = DFSEmitSplitAssign_AST(bnode.RightNode, ref astList);
                    var newBNode = nodeBuilder.BuildBinaryExpression(bnode.LeftNode, lastNode);
                    
                    astList.Add(newBNode);
                    return bnode.LeftNode;
                }
            }
            else if (node is ModifierStackNode)
            {
                ModifierStackNode mnode = node as ModifierStackNode;
                
                Validity.Assert(mnode.ElementNodes.Count > 0);
                BinaryExpressionNode lastNode = mnode.ElementNodes[mnode.ElementNodes.Count - 1] as BinaryExpressionNode;

                astList.Add(mnode);
                return lastNode.LeftNode;
            }
            return node;
        }

        private List<AssociativeNode> SplitMulitpleAssignment(List<AssociativeNode> astList)
        {
            if (null != astList && astList.Count > 0)
            {
                List<AssociativeNode> newAstList = new List<AssociativeNode>();

                foreach (AssociativeNode node in astList)
                {
                    List<AssociativeNode> newASTList = new List<AssociativeNode>();
                    if (node is BinaryExpressionNode)
                    {
                        if ((node as BinaryExpressionNode).isMultipleAssign)
                        {
                            Stack<AssociativeNode> ssaStack = new Stack<AssociativeNode>();
                            DFSEmitSplitAssign_AST(node, ref newASTList);
                            foreach (AssociativeNode anode in newASTList)
                            {
                                if (node is BinaryExpressionNode)
                                {
                                    NodeUtils.SetNodeLocation(anode, (node as BinaryExpressionNode).LeftNode, (node as BinaryExpressionNode).RightNode);
                                }
                            }
                            newAstList.AddRange(newASTList);
                        }
                        else
                        {
                            newAstList.Add(node);
                        }
                    }
                    else if (node is ClassDeclNode)
                    {
                        ClassDeclNode classNode = node as ClassDeclNode;
                        foreach (AssociativeNode procNode in classNode.funclist)
                        {
                            if (procNode is FunctionDefinitionNode)
                            {
                                FunctionDefinitionNode fNode = procNode as FunctionDefinitionNode;
                                if (!fNode.IsExternLib)
                                {
                                    fNode.FunctionBody.Body = SplitMulitpleAssignment(fNode.FunctionBody.Body);
                                }
                            }
                            else if (procNode is ConstructorDefinitionNode)
                            {
                                ConstructorDefinitionNode cNode = procNode as ConstructorDefinitionNode;
                                if (!cNode.IsExternLib)
                                {
                                    cNode.FunctionBody.Body = SplitMulitpleAssignment(cNode.FunctionBody.Body);
                                }
                            }
                        }
                        newAstList.Add(classNode);
                    }
                    else if (node is FunctionDefinitionNode)
                    {
                        FunctionDefinitionNode fNode = node as FunctionDefinitionNode;
                        if (!fNode.IsExternLib)
                        {
                            fNode.FunctionBody.Body = SplitMulitpleAssignment(fNode.FunctionBody.Body);
                        }
                        newAstList.Add(node);
                    }
                    else if (node is ConstructorDefinitionNode)
                    {
                        ConstructorDefinitionNode fNode = node as ConstructorDefinitionNode;
                        if (!fNode.IsExternLib)
                        {
                            fNode.FunctionBody.Body = SplitMulitpleAssignment(fNode.FunctionBody.Body);
                        }
                        newAstList.Add(node);
                    }
                    else
                    {
                        newAstList.Add(node);
                    }
                }
                return newAstList;
            }
            return astList;
        }

        private bool AuditReturnLocation(ref int line, ref int col, ref int endLine, ref int endCol)
        {
            if (null != localFunctionDefNode)
            {
                FunctionDefinitionNode funcDefNode = localFunctionDefNode as FunctionDefinitionNode;
                if (null == funcDefNode || (null == funcDefNode.FunctionBody))
                    return false;

                col = funcDefNode.FunctionBody.endCol - 1;
                endCol = funcDefNode.FunctionBody.endCol;
                line = endLine = funcDefNode.FunctionBody.endLine;
                return true;
            }

            if (null != localCodeBlockNode)
            {
                if (null == codeBlock.parent) // Top-most language block...
                    return false; // ... do nothing and leave it as (-1, -1).

                CodeBlockNode codeBlockNode = localCodeBlockNode as CodeBlockNode;
                if (null == codeBlockNode)
                    return false;

                col = codeBlockNode.endCol - 1;
                endCol = codeBlockNode.endCol;
                line = endLine = codeBlockNode.endLine;
                return true;
            }

            return false;
        }

        public void Emit(ProtoCore.AST.AssociativeAST.DependencyTracker tracker)
        {
            // TODO Jun: Only HydrogenRunner uses this. Consider removing if HydrogenRunner becomes redundant
            throw new NotSupportedException();
        }

        private int EmitExpressionInterpreter(ProtoCore.AST.Node codeBlockNode)
        {
            core.startPC = this.pc;
            compilePass = AssociativeCompilePass.kGlobalScope;
            ProtoCore.AST.AssociativeAST.CodeBlockNode codeblock = codeBlockNode as ProtoCore.AST.AssociativeAST.CodeBlockNode;

            ProtoCore.Type inferedType = new ProtoCore.Type();

            globalClassIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
            globalProcIndex = ProtoCore.DSASM.Constants.kGlobalScope;

            // check if currently inside a function when the break was called
            if (core.DebugProps.DebugStackFrameContains(DebugProperties.StackFrameFlagOptions.FepRun))
            {
                // Save the current scope for the expression interpreter
                globalClassIndex = core.watchClassScope = (int)core.Rmem.GetAtRelative(core.Rmem.GetStackIndex(ProtoCore.DSASM.StackFrame.kFrameIndexClass)).opdata;
                globalProcIndex = core.watchFunctionScope = (int)core.Rmem.GetAtRelative(core.Rmem.GetStackIndex(ProtoCore.DSASM.StackFrame.kFrameIndexFunction)).opdata;
                int functionBlock = (int)core.Rmem.GetAtRelative(core.Rmem.GetStackIndex(ProtoCore.DSASM.StackFrame.kFrameIndexFunctionBlock)).opdata;

                if (globalClassIndex != -1)
                    localProcedure = core.ClassTable.ClassNodes[globalClassIndex].vtable.procList[globalProcIndex];
                else
                {
                    // TODO: to investigate whethter to use the table under executable or in core.FuncTable - Randy, Jun
                    localProcedure = core.DSExecutable.procedureTable[functionBlock].procList[globalProcIndex];
                }
            }

            foreach (AssociativeNode node in codeblock.Body)
            {
                inferedType = new ProtoCore.Type();
                inferedType.UID = (int)ProtoCore.PrimitiveType.kTypeVar;
                inferedType.IsIndexable = false;

                DfsTraverse(node, ref inferedType, false, null, ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier);

                BinaryExpressionNode binaryNode = node as BinaryExpressionNode;
            }

            core.InferedType = inferedType;

            this.pc = core.startPC;

            return codeBlock.codeBlockId;
        }

        private void AllocateContextGlobals()
        {
            if (null != context && null != context.GlobalVarList && context.GlobalVarList.Count > 0)
            {
                ProtoCore.Type type = new ProtoCore.Type();
                foreach (string globalName in context.GlobalVarList.Keys)
                {
                    Allocate(
                        ProtoCore.DSASM.Constants.kInvalidIndex,
                        ProtoCore.DSASM.Constants.kInvalidIndex,
                        ProtoCore.DSASM.Constants.kGlobalScope,
                        globalName,
                        type);
                }
            }
        }

        /// <summary>
        /// This function sets the current pc after codegening the a node thats either an external function or external class
        /// We want the pc at this point as it will be used to determine which instructions are to be removed from the instruction list
        /// on delta execution. We remove all instructions starting from this pc and regenerate them
        /// </summary>
        /// <param name="node"></param>
        private void SetDeltaCompilePC(ProtoCore.AST.Node node)
        {
            // Perform analysis only on global function body compile pass
            // This means that all import statments and its classes and functions have already been codegen'd
            if (compilePass == AssociativeCompilePass.kGlobalFuncBody)
            {
                if (core.Options.IsDeltaExecution && !core.Options.IsDeltaCompile)
                {
                    if (node is ProtoCore.AST.AssociativeAST.FunctionDefinitionNode)
                    {
                        ProtoCore.AST.AssociativeAST.FunctionDefinitionNode fNode = node as ProtoCore.AST.AssociativeAST.FunctionDefinitionNode;
                        Validity.Assert(null != fNode);
                        if (fNode.IsExternLib || fNode.IsBuiltIn)
                        {
                            core.deltaCompileStartPC = pc;
                        }
                    }
                    else if (node is ProtoCore.AST.AssociativeAST.ClassDeclNode)
                    {
                        if ((node as ProtoCore.AST.AssociativeAST.ClassDeclNode).IsImportedClass)
                        {
                            core.deltaCompileStartPC = pc;
                        }
                    }
                }
            }
        }

        public override int Emit(ProtoCore.AST.Node codeBlockNode, ProtoCore.AssociativeGraph.GraphNode graphNode = null)
        {
            if (core.Options.IsDeltaExecution)
            {
                if (context != null && context.symbolTable != null)
                {
                    Validity.Assert(context.symbolTable.symbolList != null && context.symbolTable.symbolList.Count > 0);
                    codeBlock.symbolTable = context.symbolTable;
                }
            }

            AllocateContextGlobals();

            core.startPC = this.pc;
            if (core.ExecMode == ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter)
            {
                return EmitExpressionInterpreter(codeBlockNode);
            }

            this.localCodeBlockNode = codeBlockNode;
            ProtoCore.AST.AssociativeAST.CodeBlockNode codeblock = codeBlockNode as ProtoCore.AST.AssociativeAST.CodeBlockNode;
            bool isTopBlock = null == codeBlock.parent;
            if (!isTopBlock)
            {
                // If this is an inner block where there can be no classes, we can start at parsing at the global function state
                compilePass = ProtoCore.DSASM.AssociativeCompilePass.kGlobalFuncSig;
            }

            codeblock.Body = SplitMulitpleAssignment(codeblock.Body);
            
            bool hasReturnStatement = false;
            ProtoCore.Type inferedType = new ProtoCore.Type();
            bool ssaTransformed = false;
            while (ProtoCore.DSASM.AssociativeCompilePass.kDone != compilePass)
            {
                // Emit SSA only after generating the class definitions
                if (compilePass > AssociativeCompilePass.kClassName && !ssaTransformed)
                {
                    //Audit class table for multiple symbol definition and emit warning.
                    this.core.ClassTable.AuditMultipleDefinition(this.core.BuildStatus);
                    codeblock.Body = BuildSSA(codeblock.Body, context);
                    ssaTransformed = true;
                    if (core.Options.DumpIL)
                    {
                        CodeGenDS codegenDS = new CodeGenDS(codeblock.Body);
                        EmitCompileLog(codegenDS.GenerateCode());
                    }
                }

                foreach (AssociativeNode node in codeblock.Body)
                {
                    inferedType = new ProtoCore.Type();
                    inferedType.UID = (int)ProtoCore.PrimitiveType.kTypeVar;
                    inferedType.IsIndexable = false;

                    //
                    // TODO Jun:    Handle stand alone language blocks
                    //              Integrate the subPass into a proper pass
                    //              
                    //              **Need to take care of EmitImportNode, in which I used the same code to handle imported language block nodes - Randy
                    //
                    if (node is LanguageBlockNode)
                    {
                        // Build a binaryn node with a temporary lhs for every stand-alone language block
                        var iNode = nodeBuilder.BuildIdentfier(core.GenerateTempLangageVar());
                        var langBlockNode = nodeBuilder.BuildBinaryExpression(iNode, node);

                        DfsTraverse(langBlockNode, ref inferedType, false, graphNode, ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier);
                    }
                    else
                    {
                        DfsTraverse(node, ref inferedType, false, graphNode, ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier);
                        SetDeltaCompilePC(node);
                    }

                    if (NodeUtils.IsReturnExpressionNode(node))
                        hasReturnStatement = true;
                }

                if (compilePass == ProtoCore.DSASM.AssociativeCompilePass.kGlobalScope && !hasReturnStatement)
                {
                    EmitReturnNull();
                }

                compilePass++;

                // We have compiled all classes
                if (compilePass == ProtoCore.DSASM.AssociativeCompilePass.kGlobalScope && isTopBlock)
                {
                    EmitFunctionCallToInitStaticProperty(codeblock.Body);
                }

            }

            ResolveFinalNodeRefs();

            if (codeBlock.parent == null)  // top-most langauge block
            {
                ResolveFunctionGroups();
            }

            core.InferedType = inferedType;

            if (core.AsmOutput != Console.Out)
            {
                core.AsmOutput.Flush();
            }

            this.localCodeBlockNode = codeBlockNode;

            return codeBlock.codeBlockId;
        }

        private void EmitFunctionCallToInitStaticProperty(List<AssociativeNode> codeblock)
        {
            List<AssociativeNode> functionCalls = new List<AssociativeNode>();

            for (int i = 0; i < core.ClassTable.ClassNodes.Count; ++i)
            {
                var classNode = core.ClassTable.ClassNodes[i];
                if (classNode.vtable != null &&
                    classNode.vtable.procList.Exists(procNode => procNode.name == ProtoCore.DSASM.Constants.kStaticPropertiesInitializer && procNode.isStatic))
                {
                    // classname.%_init_static_properties();
                    var thisClass = nodeBuilder.BuildIdentfier(classNode.name);
                    var initializer = nodeBuilder.BuildFunctionCall(Constants.kStaticPropertiesInitializer, new List<AssociativeNode>());
                    var staticCall = nodeBuilder.BuildIdentList(thisClass, initializer);

                    // %tmpRet = classname.%_init_static_properties(); 
                    var ret = nodeBuilder.BuildIdentfier(Constants.kTempFunctionReturnVar);
                    var functionCall = nodeBuilder.BuildBinaryExpression(ret, staticCall);

                    functionCalls.Add(functionCall);
                }
            }

            codeblock.InsertRange(0, functionCalls);
        }

        public int AllocateMemberVariable(int classIndex, int classScope, string name, int rank = 0, ProtoCore.DSASM.AccessSpecifier access = ProtoCore.DSASM.AccessSpecifier.kPublic, bool isStatic = false)
        {
            // TODO Jun: Create a class table for holding the primitive and custom data types
            int datasize = ProtoCore.DSASM.Constants.kPointerSize;
            ProtoCore.Type ptrType = new ProtoCore.Type();
            if (rank == 0)
                ptrType.UID = (int)PrimitiveType.kTypePointer;
            else
                ptrType.UID = (int)PrimitiveType.kTypeArray;
            ptrType.rank = rank;
            ptrType.IsIndexable = rank == 0 ? false : true;
            ProtoCore.DSASM.SymbolNode symnode = Allocate(classIndex, classScope, ProtoCore.DSASM.Constants.kGlobalScope, name, ptrType, datasize, isStatic, access);
            if (null == symnode)
            {
                buildStatus.LogSemanticError("Member variable '" + name + "' is already defined in class " + core.ClassTable.ClassNodes[classIndex].name);
                return ProtoCore.DSASM.Constants.kInvalidIndex;
            }


            return symnode.symbolTableIndex;
        }


        private bool VerifyAllocationIncludingChildBlock(string name, int classScope, int functionScope, out ProtoCore.DSASM.SymbolNode symbol, out bool isAccessible)
        {
            bool isAllocated = VerifyAllocation(name, classScope, functionScope, out symbol, out isAccessible);
            if (!isAllocated)
            {
                int symbolIndex = Constants.kInvalidIndex;

                if (codeBlock.children.Count > 0)
                {
                    CodeBlock searchBlock = codeBlock.children[0];
                    while (symbolIndex == Constants.kInvalidIndex && searchBlock != null)
                    {
                        symbolIndex = searchBlock.symbolTable.IndexOf(name, Constants.kGlobalScope, Constants.kGlobalScope);
                        if (symbolIndex != Constants.kInvalidIndex)
                        {
                            symbol = searchBlock.symbolTable.symbolList[symbolIndex];
                            isAccessible = true;
                            return true;
                        }

                        //
                        // TODO Jun: Parallel construct blocks?
                        //
                        //      if (0)
                        //      {
                        //          x = 10;
                        //      }
                        //
                        //      if (1)
                        //      {
                        //          x = 20;
                        //      }
                        //

                        if (searchBlock.children.Count > 0)
                        {
                            searchBlock = searchBlock.children[0];
                        }
                        else
                        {
                            searchBlock = null;
                        }
                    }
                }
            }
            return isAllocated;
            //int symbolIndex = Constants.kInvalidIndex;
            //symbol = null;
            //isAccessible = false;

            //if (classScope != Constants.kGlobalScope)
            //{
            //    if ((int)ProtoCore.PrimitiveType.kTypeVoid == classScope)
            //    {
            //        return false;
            //    }
            //    ClassNode thisClass = core.ClassTable.list[classScope];

            //    bool hasThisSymbol;
            //    AddressType addressType;
            //    symbolIndex = thisClass.GetSymbolIndex(name, classScope, functionScope, out hasThisSymbol, out addressType);

            //    if (Constants.kInvalidIndex != symbolIndex)
            //    {
            //        // It is static member, then get node from code block
            //        if (AddressType.StaticMemVarIndex == addressType)
            //        {
            //            symbol = core.CodeBlockList[0].symbolTable.symbolList[symbolIndex];
            //        }
            //        else
            //        {
            //            symbol = thisClass.symbols.symbolList[symbolIndex];
            //        }

            //        isAccessible = true;
            //    }

            //    if (hasThisSymbol)
            //    {
            //        return true;
            //    }
            //    else
            //    {
            //        symbolIndex = codeBlock.symbolTable.IndexOf(name, Constants.kGlobalScope, Constants.kGlobalScope);
            //        if (symbolIndex != Constants.kInvalidIndex)
            //        {
            //            symbol = codeBlock.symbolTable.symbolList[symbolIndex];
            //            isAccessible = true;
            //            return true;
            //        }
            //    }
            //}
            //else
            //{
            //    if (functionScope != Constants.kGlobalScope)
            //    {
            //        symbol = core.GetSymbolInFunction(name, Constants.kGlobalScope, functionScope, codeBlock);
            //        if (symbol != null)
            //        {
            //            isAccessible = true;
            //            return true;
            //        }
            //    }

            //    CodeBlock searchBlock = codeBlock;
            //    while (symbolIndex == Constants.kInvalidIndex && searchBlock != null)
            //    {
            //        symbolIndex = searchBlock.symbolTable.IndexOf(name, Constants.kGlobalScope, Constants.kGlobalScope);
            //        if (symbolIndex != Constants.kInvalidIndex)
            //        {
            //            symbol = searchBlock.symbolTable.symbolList[symbolIndex];
            //            isAccessible = true;
            //            return true;
            //        }
            //        searchBlock = searchBlock.parent;
            //    }
            //}
        }

        private bool EmitReplicationGuideForIdentifier(IdentifierNode t)
        {
            bool isReplicationGuideEmitted = false;
            if (emitReplicationGuide)
            {
                int replicationGuides = 0;
                if (null != t.ReplicationGuides)
                {
                    isReplicationGuideEmitted = true;

                    replicationGuides = t.ReplicationGuides.Count;
                    for (int n = 0; n < replicationGuides; ++n)
                    {
                        Validity.Assert(t.ReplicationGuides[n] is IdentifierNode);
                        IdentifierNode nodeGuide = t.ReplicationGuides[n] as IdentifierNode;

                        EmitInstrConsole(ProtoCore.DSASM.kw.push, nodeGuide.Value);
                        StackValue opguide = StackValue.BuildInt(System.Convert.ToInt64(nodeGuide.Value));
                        EmitPush(opguide);
                    }
                }
            }

            return isReplicationGuideEmitted;
        }

        /*
         proc EmitIdentNode(identnode, graphnode)
	        if ssa
		        // Check an if this identifier is array indexed
		        // The array index is a secondary property and is not the original array property of the AST. this is required because this array index is meant only to resolve graphnode dependency with arrays
		        if node.arrayindex.secondary is valid
			        dimension = traverse(node.arrayindex.secondary)

			        // Create a new dependent with the array indexing
			        dependent = new GraphNode(identnode.name, dimension)
			        graphnode.pushdependent(dependent)
		        end
	        end
        end

         */

        private void EmitIdentifierNode(AssociativeNode node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone, BinaryExpressionNode parentNode = null)
        {
            IdentifierNode t = node as IdentifierNode;
            if (t.Name.Equals(ProtoCore.DSDefinitions.Keyword.This))
            {
                if (subPass != AssociativeSubCompilePass.kNone)
                {
                    return;
                }

                if (localProcedure != null)
                {
                    if (localProcedure.isStatic)
                    {
                        string message = ProtoCore.BuildData.WarningMessage.kUsingThisInStaticFunction;
                        core.BuildStatus.LogWarning(ProtoCore.BuildData.WarningID.kInvalidThis, message, core.CurrentDSFileName, t.line, t.col);
                        EmitPushNull();
                        return;
                    }
                    else if (localProcedure.classScope == Constants.kGlobalScope)
                    {
                        string message = ProtoCore.BuildData.WarningMessage.kInvalidThis;
                        core.BuildStatus.LogWarning(ProtoCore.BuildData.WarningID.kInvalidThis, message, core.CurrentDSFileName, t.line, t.col);
                        EmitPushNull();
                        return;
                    }
                    else
                    {
                        EmitThisPointerNode(subPass);
                        return;
                    }
                }
                else
                {
                    string message = ProtoCore.BuildData.WarningMessage.kInvalidThis;
                    core.BuildStatus.LogWarning(ProtoCore.BuildData.WarningID.kInvalidThis, message, core.CurrentDSFileName, t.line, t.col);
                    EmitPushNull();
                    return;
                }
            }

            int dimensions = 0;

            ProtoCore.DSASM.SymbolNode symbolnode = null;
            int runtimeIndex = codeBlock.symbolTable.RuntimeIndex;

            ProtoCore.Type type = new ProtoCore.Type();
            type.UID = (int)ProtoCore.PrimitiveType.kTypeVoid;
            type.IsIndexable = false;

            bool isAccessible = false;

            if (null == t.ArrayDimensions)
            {
                //check if it is a function instance
                ProtoCore.DSASM.ProcedureNode procNode = null;
                procNode = core.GetFirstVisibleProcedure(t.Name, null, codeBlock);
                if (null != procNode)
                {
                    if (ProtoCore.DSASM.Constants.kInvalidIndex != procNode.procId)
                    {
                        // A global function
                        inferedType.IsIndexable = false;
                        inferedType.UID = (int)PrimitiveType.kTypeFunctionPointer;
                        if (ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier != subPass)
                        {
                            int fptr = core.FunctionPointerTable.functionPointerDictionary.Count;
                            var fptrNode = new FunctionPointerNode(procNode);
                            core.FunctionPointerTable.functionPointerDictionary.TryAdd(fptr, fptrNode);
                            core.FunctionPointerTable.functionPointerDictionary.TryGetBySecond(fptrNode, out fptr);

                            EmitPushVarData(runtimeIndex, 0);

                            EmitInstrConsole(ProtoCore.DSASM.kw.push, t.Name);
                            StackValue opFunctionPointer = StackValue.BuildFunctionPointer(fptr);
                            EmitPush(opFunctionPointer, t.line, t.col);
                        }
                        return;
                    }
                }
            }
         
            bool isAllocated = VerifyAllocation(t.Name, globalClassIndex, globalProcIndex, out symbolnode, out isAccessible);
            

            // If its executing in interpreter mode - attempt to find and anubond identifer in a child block
            // Remove this, because if we are watching cases like:
            //c1 = [Imperative]
            //{
            //    a = 1;
            //    b = 2;
            //}
            //
            //c2 = [Associative]
            //{
            //    a = 3;
            //    b = 4;
            //}
            //After c2 is executed, the watch value for a, b will be 1, 2.
            //if (ProtoCore.DSASM.ExecutionMode.kExpressionInterpreter == core.ExecMode)
            //{
            //    if (!isAllocated)
            //    {
            //        isAllocated = VerifyAllocationIncludingChildBlock(t.Name, globalClassIndex, globalProcIndex, out symbolnode, out isAccessible);
            //    }
            //}   

            if (AssociativeSubCompilePass.kUnboundIdentifier == subPass)
            {
                if (symbolnode == null)
                {
                    if (isAllocated)
                    {
                        string message = String.Format(WarningMessage.kPropertyIsInaccessible, t.Value);
                        if (localProcedure != null && localProcedure.isStatic)
                        {
                            SymbolNode tempSymbolNode;

                            VerifyAllocation(
                                t.Name,
                                globalClassIndex,
                                Constants.kGlobalScope,
                                out tempSymbolNode,
                                out isAccessible);

                            if (tempSymbolNode != null && !tempSymbolNode.isStatic && isAccessible)
                            {
                                message = String.Format(WarningMessage.kUsingNonStaticMemberInStaticContext, t.Value);
                            }
                        }
                        buildStatus.LogWarning(
                            ProtoCore.BuildData.WarningID.kAccessViolation, 
                            message, 
                            core.CurrentDSFileName, 
                            t.line, 
                            t.col);
                    }
                    else
                    {
                        string message = String.Format(WarningMessage.kUnboundIdentifierMsg, t.Value);
                        buildStatus.LogWarning(WarningID.kIdUnboundIdentifier, message, core.CurrentDSFileName, t.line, t.col);
                    }

                    if (ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter != core.ExecMode)
                    {
                        inferedType.UID = (int)ProtoCore.PrimitiveType.kTypeNull;

                        // Jun Comment: Specification 
                        //      If resolution fails at this point a com.Design-Script.Imperative.Core.UnboundIdentifier 
                        //      warning is emitted during pre-execute phase, and at the ID is bound to null. (R1 - Feb)

                        int startpc = pc;
                        EmitPushNull();

                        // Push the identifier local block  
                        dimensions = 0;
                        EmitPushVarData(runtimeIndex, dimensions);
                        ProtoCore.Type varType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, false, 0);

                        // TODO Jun: Refactor Allocate() to just return the symbol node itself
                        ProtoCore.DSASM.SymbolNode symnode = Allocate(globalClassIndex, globalClassIndex, globalProcIndex, t.Value, varType, ProtoCore.DSASM.Constants.kPrimitiveSize,
                            false, ProtoCore.DSASM.AccessSpecifier.kPublic, ProtoCore.DSASM.MemoryRegion.kMemStack, t.line, t.col, graphNode);
                        Validity.Assert(symnode != null);

                        int symbolindex = symnode.symbolTableIndex;
                        if (ProtoCore.DSASM.Constants.kInvalidIndex != globalClassIndex)
                        {
                            symbolnode = core.ClassTable.ClassNodes[globalClassIndex].symbols.symbolList[symbolindex];
                        }
                        else
                        {
                            symbolnode = codeBlock.symbolTable.symbolList[symbolindex];
                        }

                        EmitInstrConsole(ProtoCore.DSASM.kw.pop, t.Value);
                        EmitPopForSymbol(symnode);

                        // Comment it out. It doesn't work for the following 
                        // case:
                        //
                        //     x = foo.X;
                        //     x = bar.X;
                        //
                        // where bar hasn't defined yet, so a null assign
                        // graph is generated for this case:
                        //
                        //    bar = null; 
                        //    x = %dot(.., {bar}, ...);
                        // 
                        // unfortunately the expression UID of this null graph
                        // node is 0, which is wrong. Some update routines have
                        // the assumption that the exprUID of graph node is
                        // incremental. 
                        // 
                        // We may generate SSA for all expressions to fix this
                        // issue. -Yu Ke
                        /*
                        ProtoCore.AssociativeGraph.GraphNode nullAssignGraphNode = new ProtoCore.AssociativeGraph.GraphNode();
                        nullAssignGraphNode.PushSymbolReference(symbolnode);
                        nullAssignGraphNode.procIndex = globalProcIndex;
                        nullAssignGraphNode.classIndex = globalClassIndex;
                        nullAssignGraphNode.updateBlock.startpc = startpc;
                        nullAssignGraphNode.updateBlock.endpc = pc - 1;

                        codeBlock.instrStream.dependencyGraph.Push(nullAssignGraphNode);
                        */
                    }
                }

                if (null != t.ArrayDimensions)
                {
                    dimensions = DfsEmitArrayIndexHeap(t.ArrayDimensions, graphNode, parentNode, subPass);
                }
            }
            else
            {
                if (core.ExecMode == ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter &&
                    !isAllocated)
                {
                    // It happens when the debugger try to watch a variable 
                    // which has been out of scope (as watch is done through
                    // execute an expression "t = v;" where v is the variable
                    // to be watched.
                    EmitPushNull();
                    return;
                }

                Validity.Assert(isAllocated);

                if (graphNode != null 
                    && IsAssociativeArrayIndexing 
                    && !CoreUtils.IsAutoGeneratedVar(symbolnode.name))
                {
                    ProtoCore.AssociativeGraph.UpdateNode updateNode = new ProtoCore.AssociativeGraph.UpdateNode();
                    updateNode.symbol = symbolnode;
                    updateNode.nodeType = ProtoCore.AssociativeGraph.UpdateNodeType.kSymbol;

                    if (graphNode.isIndexingLHS)
                    {
                        graphNode.dimensionNodeList.Add(updateNode);
                    }
                    else
                    {
                        int curDepIndex = graphNode.dependentList.Count - 1;
                        if (curDepIndex >= 0)
                        {
                            var curDep = graphNode.dependentList[curDepIndex].updateNodeRefList[0].nodeList[0];
                            curDep.dimensionNodeList.Add(updateNode);

                            if (core.Options.FullSSA)
                            {
                                if (null != firstSSAGraphNode)
                                {
                                    curDepIndex = firstSSAGraphNode.dependentList.Count - 1;
                                    if (curDepIndex >= 0)
                                    {
                                        ProtoCore.AssociativeGraph.UpdateNode firstSSAUpdateNode = firstSSAGraphNode.dependentList[curDepIndex].updateNodeRefList[0].nodeList[0];
                                        firstSSAUpdateNode.dimensionNodeList.Add(updateNode);
                                    }
                                }
                            }
                        }
                    }
                }

                // If it is a property, replaced it with getter: %get_prop()
                if (symbolnode.classScope != ProtoCore.DSASM.Constants.kInvalidIndex &&
                    symbolnode.functionIndex == ProtoCore.DSASM.Constants.kGlobalScope &&
                    localProcedure != null)
                {
                    string getterName = ProtoCore.DSASM.Constants.kGetterPrefix + t.Name;
                    if (!string.Equals(localProcedure.name, getterName))
                    {
                        var thisNode = nodeBuilder.BuildIdentfier(ProtoCore.DSDefinitions.Keyword.This);
                        var identListNode = nodeBuilder.BuildIdentList(thisNode, t);
                        EmitIdentifierListNode(identListNode, ref inferedType, false, graphNode, ProtoCore.DSASM.AssociativeSubCompilePass.kNone);

                        if (null != graphNode)
                        {
                            ProtoCore.AssociativeGraph.GraphNode dependentNode = new ProtoCore.AssociativeGraph.GraphNode();
                            dependentNode.PushSymbolReference(symbolnode);
                            graphNode.PushDependent(dependentNode);
                        }

                        return;
                    }
                }

                type = symbolnode.datatype;
                runtimeIndex = symbolnode.runtimeTableIndex;

                // The graph node always depends on this identifier
                if (null != graphNode)
                {
                    ProtoCore.AssociativeGraph.GraphNode dependentNode = new ProtoCore.AssociativeGraph.GraphNode();
                    dependentNode.PushSymbolReference(symbolnode);
                    if (!ProtoCore.Utils.CoreUtils.IsPropertyTemp(symbolnode.name))
                    {
                        graphNode.PushDependent(dependentNode);
                    }
                }

                bool emitReplicationGuideFlag = emitReplicationGuide;
                emitReplicationGuide = false;
                if (null != t.ArrayDimensions)
                {
                    dimensions = DfsEmitArrayIndexHeap(t.ArrayDimensions, graphNode, parentNode, subPass);
                }
                emitReplicationGuide = emitReplicationGuideFlag;

                //fix type's rank   
                if (type.rank >= 0)
                {
                    type.rank -= dimensions;
                    if (type.rank < 0)
                    {
                        //throw new Exception("Exceed maximum rank!");
                        type.rank = 0;
                    }
                }

                //check whether the value is an array
                if (type.rank == 0)
                {
                    type.IsIndexable = false;
                }

                EmitPushVarData(runtimeIndex, dimensions);


                if (ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter == core.ExecMode)
                {
                    EmitInstrConsole(ProtoCore.DSASM.kw.pushw, t.Value);
                    EmitPushForSymbolW(symbolnode, t.line, t.col);
                }
                else
                {
                    EmitInstrConsole(ProtoCore.DSASM.kw.push, t.Value);
                    EmitPushForSymbol(symbolnode, t);

                    if (core.Options.TempReplicationGuideEmptyFlag && emitReplicationGuide)
                    {
                        int guides = EmitReplicationGuides(t.ReplicationGuides);
                        EmitInstrConsole(ProtoCore.DSASM.kw.pushindex, guides + "[guide]");
                        EmitPushReplicationGuide(guides);
                    }
                }

                if (ignoreRankCheck || core.TypeSystem.IsHigherRank(type.UID, inferedType.UID))
                {
                    inferedType = type;
                }
                // We need to get inferedType for boolean variable so that we can perform type check
                inferedType.UID = (isBooleanOp || (type.UID == (int)PrimitiveType.kTypeBool)) ? (int)PrimitiveType.kTypeBool : inferedType.UID;
            }

        }
#if ENABLE_INC_DEC_FIX
        private void EmitPostFixNode(AssociativeNode node, ref ProtoCore.Type inferedType)
        {
            bool parseGlobal = null == localProcedure && ProtoCore.DSASM.AssociativeCompilePass.kAll == compilePass;
            bool parseGlobalFunction = null != localProcedure && ProtoCore.DSASM.AssociativeCompilePass.kGlobalFuncBody == compilePass;
            bool parseMemberFunction = ProtoCore.DSASM.Constants.kGlobalScope != classIndex && ProtoCore.DSASM.AssociativeCompilePass.kClassMemFuncBody == compilePass;

            if (parseGlobal || parseGlobalFunction || parseMemberFunction)
            {
                PostFixNode pfNode = node as PostFixNode;

                //convert post fix operation to a binary operation
                BinaryExpressionNode binRight = new BinaryExpressionNode();
                BinaryExpressionNode bin = new BinaryExpressionNode();

                binRight.LeftNode = pfNode.Identifier;
                binRight.RightNode = new IntNode() { value = "1" };
                binRight.Optr = (ProtoCore.DSASM.UnaryOperator.Increment == pfNode.Operator) ? ProtoCore.DSASM.Operator.add : ProtoCore.DSASM.Operator.sub;
                bin.LeftNode = pfNode.Identifier;
                bin.RightNode = binRight;
                bin.Optr = ProtoCore.DSASM.Operator.assign;
                EmitBinaryExpressionNode(bin, ref inferedType);
            }
        }
#endif
        private void EmitRangeExprNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            RangeExprNode range = node as RangeExprNode;

            // Do some static checking...
            if ((range.FromNode is IntNode || range.FromNode is DoubleNode) &&
                (range.ToNode is IntNode || range.ToNode is DoubleNode) &&
                (range.StepNode == null || (range.StepNode != null && (range.StepNode is IntNode || range.StepNode is DoubleNode))))
            {
                decimal current = (range.FromNode is IntNode) ? Int64.Parse((range.FromNode as IntNode).value) : Decimal.Parse((range.FromNode as DoubleNode).value);
                decimal end = (range.ToNode is IntNode) ? Int64.Parse((range.ToNode as IntNode).value) : Decimal.Parse((range.ToNode as DoubleNode).value);
                ProtoCore.DSASM.RangeStepOperator stepoperator = range.stepoperator;

                decimal step = 1;
                if (range.StepNode != null)
                {
                    step = (range.StepNode is IntNode) ? Int64.Parse((range.StepNode as IntNode).value) : Decimal.Parse((range.StepNode as DoubleNode).value);
                }

                if (stepoperator == ProtoCore.DSASM.RangeStepOperator.stepsize)
                {
                    if (range.StepNode == null && end < current)
                    {
                        step = -1;
                    }

                    if (step == 0)
                    {
                        buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kInvalidRangeExpression, ProtoCore.BuildData.WarningMessage.kRangeExpressionWithStepSizeZero, core.CurrentDSFileName, range.StepNode.line, range.StepNode.col);
                        EmitNullNode(new NullNode(), ref inferedType);
                        return;
                    }
                    if ((end > current && step < 0) || (end < current && step > 0))
                    {
                        buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kInvalidRangeExpression, ProtoCore.BuildData.WarningMessage.kRangeExpressionWithInvalidStepSize, core.CurrentDSFileName, range.StepNode.line, range.StepNode.col);
                        EmitNullNode(new NullNode(), ref inferedType);
                        return;
                    }
                }
                else if (stepoperator == ProtoCore.DSASM.RangeStepOperator.num)
                {
                    if (range.StepNode != null && !(range.StepNode is IntNode))
                    {
                        buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kInvalidRangeExpression, ProtoCore.BuildData.WarningMessage.kRangeExpressionWithNonIntegerStepNumber, core.CurrentDSFileName, range.StepNode.line, range.StepNode.col);
                        EmitNullNode(new NullNode(), ref inferedType);
                        return;
                    }

                    if (step <= 0)
                    {
                        buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kInvalidRangeExpression, ProtoCore.BuildData.WarningMessage.kRangeExpressionWithNegativeStepNumber, core.CurrentDSFileName, range.StepNode.line, range.StepNode.col);
                        EmitNullNode(new NullNode(), ref inferedType);
                        return;
                    }
                }
                else if (stepoperator == ProtoCore.DSASM.RangeStepOperator.approxsize)
                {
                    if (step == 0)
                    {
                        buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kInvalidRangeExpression, ProtoCore.BuildData.WarningMessage.kRangeExpressionWithStepSizeZero, core.CurrentDSFileName, range.StepNode.line, range.StepNode.col);
                        EmitNullNode(new NullNode(), ref inferedType);
                        return;
                    }
                }
            }

            // Replace with build-in RangeExpression() function. - Yu Ke
            bool emitReplicationgGuideState = emitReplicationGuide;
            emitReplicationGuide = false;


            BooleanNode hasStep = new BooleanNode { value = range.StepNode == null ? "false" : "true" };

            IntNode op = new IntNode();
            switch (range.stepoperator)
            {
                case ProtoCore.DSASM.RangeStepOperator.stepsize:
                    op.value = "0";
                    break;
                case ProtoCore.DSASM.RangeStepOperator.num: 
                    op.value = "1";
                    break;
                case ProtoCore.DSASM.RangeStepOperator.approxsize:
                    op.value = "2";
                    break;
                default:
                    op.value = "-1";
                    break;
            }

            var rangeExprFunc = nodeBuilder.BuildFunctionCall(Constants.kFunctionRangeExpression,
                new List<AssociativeNode> { range.FromNode, range.ToNode, range.StepNode == null ? new NullNode() : range.StepNode, op, hasStep });

            EmitFunctionCallNode(rangeExprFunc, ref inferedType, false, graphNode, subPass);

            emitReplicationGuide = emitReplicationgGuideState;

            if (subPass != ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
            {
                if (range.ArrayDimensions != null)
                {
                    int dimensions = DfsEmitArrayIndexHeap(range.ArrayDimensions, graphNode);
                    EmitInstrConsole(ProtoCore.DSASM.kw.pushindex, dimensions.ToString() + "[dim]");
                    EmitPushArrayIndex(dimensions);
                }

                if (core.Options.TempReplicationGuideEmptyFlag && emitReplicationGuide)
                {
                    int replicationGuideNumber = EmitReplicationGuides(range.ReplicationGuides);
                    EmitInstrConsole(ProtoCore.DSASM.kw.pushindex, replicationGuideNumber + "[guide]");
                    EmitPushReplicationGuide(replicationGuideNumber);
                }
            }
        }

        private void EmitForLoopNode(AssociativeNode node)
        {
            throw new NotImplementedException();
        }

        private void EmitLanguageBlockNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.AssociativeGraph.GraphNode graphNode, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            if (IsParsingGlobal() || IsParsingGlobalFunctionBody() || IsParsingMemberFunctionBody() )
            {
                if (subPass == ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
                {
                    return;
                }

                LanguageBlockNode langblock = node as LanguageBlockNode;

                //Validity.Assert(ProtoCore.Language.kInvalid != langblock.codeblock.language);
                if (ProtoCore.Language.kInvalid == langblock.codeblock.language)
                    throw new BuildHaltException("Invalid language block type (D1B95A65)");

                ProtoCore.CompileTime.Context context = new ProtoCore.CompileTime.Context();

                int entry = 0;
                int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;

                // Top block signifies the auto inserted global block
                bool isTopBlock = null == codeBlock.parent;

                // The warning is enforced only if this is not the top block
                if (ProtoCore.Language.kAssociative == langblock.codeblock.language && !isTopBlock)
                {
                    // TODO Jun: Move the associative and all common string into some table
                    buildStatus.LogSyntaxError("An associative language block is declared within an associative language block.", core.CurrentDSFileName, langblock.line, langblock.col);
                }


                // Set the current class scope so the next language can refer to it
                core.ClassIndex = globalClassIndex;

                if (globalProcIndex != ProtoCore.DSASM.Constants.kInvalidIndex && core.ProcNode == null)
                {
                    if (globalClassIndex != ProtoCore.DSASM.Constants.kInvalidIndex)
                        core.ProcNode = core.ClassTable.ClassNodes[globalClassIndex].vtable.procList[globalProcIndex];
                    else
                        core.ProcNode = codeBlock.procedureTable.procList[globalProcIndex];
                }

                ProtoCore.AssociativeGraph.GraphNode propagateGraphNode = null;
                if (core.Options.AssociativeToImperativePropagation && Language.kImperative == langblock.codeblock.language)
                {
                    propagateGraphNode = graphNode;
                }

                core.Executives[langblock.codeblock.language].Compile(out blockId, codeBlock, langblock.codeblock, context, codeBlock.EventSink, langblock.CodeBlockNode, propagateGraphNode);
                graphNode.isLanguageBlock = true;
                graphNode.languageBlockId = blockId;

                setBlkId(blockId);
                inferedType = core.InferedType;
                //Validity.Assert(codeBlock.children[codeBlock.children.Count - 1].blockType == ProtoCore.DSASM.CodeBlockType.kLanguage);
                codeBlock.children[codeBlock.children.Count - 1].Attributes = PopulateAttributes(langblock.Attributes);

#if ENABLE_EXCEPTION_HANDLING
                core.ExceptionHandlingManager.Register(blockId, globalProcIndex, globalClassIndex);
#endif

                int startpc = pc;

                EmitInstrConsole(ProtoCore.DSASM.kw.bounce + " " + blockId + ", " + entry.ToString());
                EmitBounceIntrinsic(blockId, entry);


                // The callee language block will have stored its result into the RX register. 
                EmitInstrConsole(ProtoCore.DSASM.kw.push, ProtoCore.DSASM.kw.regRX);
                StackValue opRes = StackValue.BuildRegister(Registers.RX);
                EmitPush(opRes);
            }
        }

        private void EmitDynamicLanguageBlockNode(AssociativeNode node, AssociativeNode singleBody, ref ProtoCore.Type inferedType, ref int blockId, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            if (IsParsingGlobal() || IsParsingGlobalFunctionBody() || IsParsingMemberFunctionBody())
            {
                if (subPass == ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
                {
                    return;
                }

                LanguageBlockNode langblock = node as LanguageBlockNode;

                //Validity.Assert(ProtoCore.Language.kInvalid != langblock.codeblock.language);
                if (ProtoCore.Language.kInvalid == langblock.codeblock.language)
                    throw new BuildHaltException("Invalid language block type (B1C57E37)");

                ProtoCore.CompileTime.Context context = new ProtoCore.CompileTime.Context();
                context.applySSATransform = false;

                // Set the current class scope so the next language can refer to it
                core.ClassIndex = globalClassIndex;

                if (globalProcIndex != ProtoCore.DSASM.Constants.kInvalidIndex && core.ProcNode == null)
                {
                    if (globalClassIndex != ProtoCore.DSASM.Constants.kInvalidIndex)
                        core.ProcNode = core.ClassTable.ClassNodes[globalClassIndex].vtable.procList[globalProcIndex];
                    else
                        core.ProcNode = codeBlock.procedureTable.procList[globalProcIndex];
                }

                core.Executives[langblock.codeblock.language].Compile(out blockId, codeBlock, langblock.codeblock, context, codeBlock.EventSink, langblock.CodeBlockNode, graphNode);
                
            }
        }

        private void EmitGetterForProperty(ProtoCore.AST.AssociativeAST.ClassDeclNode cnode, ProtoCore.DSASM.SymbolNode prop)
        {
            FunctionDefinitionNode getter = new FunctionDefinitionNode
            {
                Name = ProtoCore.DSASM.Constants.kGetterPrefix + prop.name,
                Signature = new ArgumentSignatureNode(),
                Pattern = null,
                ReturnType = prop.datatype,
                FunctionBody = new CodeBlockNode(),
                IsExternLib = false,
                IsDNI = false,
                ExternLibName = null,
                access = prop.access,
                IsStatic = prop.isStatic,
                IsAutoGenerated = true
            };

            var ret = nodeBuilder.BuildReturn();
            var ident = nodeBuilder.BuildIdentfier(prop.name);
            var retStatement = nodeBuilder.BuildBinaryExpression(ret, ident); 
            getter.FunctionBody.Body.Add(retStatement);
            cnode.funclist.Add(getter);
        }

        private FunctionDefinitionNode EmitSetterFunction(ProtoCore.DSASM.SymbolNode prop, ProtoCore.Type argType)
        {
            var argument = new ProtoCore.AST.AssociativeAST.VarDeclNode()
            {
                memregion = ProtoCore.DSASM.MemoryRegion.kMemStack,
                access = ProtoCore.DSASM.AccessSpecifier.kPublic,
                NameNode = nodeBuilder.BuildIdentfier(Constants.kTempArg),
                ArgumentType = argType
            };
            var argumentSingature = new ArgumentSignatureNode();
            argumentSingature.AddArgument(argument);

            FunctionDefinitionNode setter = new FunctionDefinitionNode
            {
                Name = ProtoCore.DSASM.Constants.kSetterPrefix + prop.name,
                Signature = argumentSingature,
                Pattern = null,
                ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeNull, false, 0),
                FunctionBody = new CodeBlockNode(),
                IsExternLib = false,
                IsDNI = false,
                ExternLibName = null,
                access = prop.access,
                IsStatic = prop.isStatic,
                IsAutoGenerated = true
            };

            // property = %tmpArg
            var propIdent = new TypedIdentifierNode();
            propIdent.Name = propIdent.Value = prop.name;
            propIdent.datatype = prop.datatype;

            var tmpArg = nodeBuilder.BuildIdentfier(Constants.kTempArg);
            var assignment = nodeBuilder.BuildBinaryExpression(propIdent, tmpArg);
            setter.FunctionBody.Body.Add(assignment);

            // return = null;
            var returnNull = nodeBuilder.BuildBinaryExpression(nodeBuilder.BuildReturn(), new NullNode());
            setter.FunctionBody.Body.Add(returnNull);

            return setter;
        }

        private void EmitSetterForProperty(ProtoCore.AST.AssociativeAST.ClassDeclNode cnode, ProtoCore.DSASM.SymbolNode prop)
        {
            ProtoCore.Type varAnyRank = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, false, Constants.kUndefinedRank);
            if (!prop.datatype.Equals(varAnyRank))
            {
                var setter = EmitSetterFunction(prop, prop.datatype);
                cnode.funclist.Add(setter);
            }

            ProtoCore.Type varType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, false, 0);
            if (!prop.datatype.Equals(varType))
            {
                var setterForVar = EmitSetterFunction(prop, varType);
                cnode.funclist.Add(setterForVar);
            }

            ProtoCore.Type varArrayType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, true, Constants.kArbitraryRank);
            if (!prop.datatype.Equals(varArrayType))
            {
                var setterForVarArray = EmitSetterFunction(prop, varArrayType);
                cnode.funclist.Add(setterForVarArray);
            }
        }

        private void EmitMemberVariables(ClassDeclNode classDecl)
        {
            // Class member variable pass
            // Populating each class entry symbols with their respective member variables

            int thisClassIndex = core.ClassTable.GetClassId(classDecl.className);
            globalClassIndex = thisClassIndex;

            if (!unPopulatedClasses.ContainsKey(thisClassIndex))
            {
                return;
            }
            ProtoCore.DSASM.ClassNode thisClass = core.ClassTable.ClassNodes[thisClassIndex];

            // Handle member vars from base class
            if (null != classDecl.superClass)
            {
                for (int n = 0; n < classDecl.superClass.Count; ++n)
                {
                    int baseClassIndex = core.ClassTable.GetClassId(classDecl.superClass[n]);
                    if (ProtoCore.DSASM.Constants.kInvalidIndex != baseClassIndex)
                    {
                        // To handle the case that a base class is defined after
                        // this class.
                        if (unPopulatedClasses.ContainsKey(baseClassIndex))
                        {
                            EmitMemberVariables(unPopulatedClasses[baseClassIndex]);
                            globalClassIndex = thisClassIndex;
                        }

                        ClassNode baseClass = core.ClassTable.ClassNodes[baseClassIndex];
                        // Append the members variables of every class that this class inherits from
                        foreach (ProtoCore.DSASM.SymbolNode symnode in baseClass.symbols.symbolList.Values)
                        {
                            // It is a member variables
                            if (ProtoCore.DSASM.Constants.kGlobalScope == symnode.functionIndex)
                            {
                                Validity.Assert(!symnode.isArgument);
                                int symbolIndex = AllocateMemberVariable(thisClassIndex, symnode.isStatic ? symnode.classScope : baseClassIndex, symnode.name, symnode.datatype.rank, symnode.access, symnode.isStatic);

                                if (symbolIndex != ProtoCore.DSASM.Constants.kInvalidIndex)
                                    thisClass.size += ProtoCore.DSASM.Constants.kPointerSize;
                            }
                        }
                    }
                    else
                    {
                        Validity.Assert(false, "n-pass compile error, fixme Jun....");
                    }
                }
            }

            // This list will store all static properties initialization
            // expression (say x = 1).
            List<BinaryExpressionNode> staticPropertyInitList = new List<BinaryExpressionNode>();

            foreach (VarDeclNode vardecl in classDecl.varlist)
            {
                IdentifierNode varIdent = null;
                if (vardecl.NameNode is IdentifierNode)
                {   
                    varIdent = vardecl.NameNode as IdentifierNode;

                    BinaryExpressionNode bNode = new BinaryExpressionNode();

                    var thisNode = nodeBuilder.BuildIdentfier(ProtoCore.DSDefinitions.Keyword.This);
                    var propNode = nodeBuilder.BuildIdentfier(varIdent.Value);
                    bNode.LeftNode = nodeBuilder.BuildIdentList(thisNode, propNode);

                    NodeUtils.CopyNodeLocation(bNode, vardecl);
                    bNode.Optr = ProtoCore.DSASM.Operator.assign;

                    bool skipInitialization = false;

                    // Initialize it to default value by manually add the right hand side node
                    if (vardecl.ArgumentType.rank == 0)
                    {
                        switch (vardecl.ArgumentType.Name)
                        {
                            case "double": bNode.RightNode = new DoubleNode { value = "0" }; break;
                            case "int": bNode.RightNode = new IntNode { value = "0" }; break;
                            case "bool": bNode.RightNode = new BooleanNode { value = "false" }; break;
                            default: skipInitialization = true; break;
                        }
                    }
                    else if (vardecl.ArgumentType.rank > 0)
                    {
                        if (!vardecl.ArgumentType.Name.Equals("var"))
                            bNode.RightNode = new ExprListNode();
                        else
                            skipInitialization = true;
                    }
                    else if(vardecl.ArgumentType.rank.Equals(ProtoCore.DSASM.Constants.kArbitraryRank))
                    {
                        if (!vardecl.ArgumentType.Name.Equals("var"))
                            bNode.RightNode = new NullNode();
                        else
                            skipInitialization = true;
                    }

                    if (!skipInitialization)
                    {
                        if (vardecl.IsStatic)
                            staticPropertyInitList.Add(bNode);
                        else
                            thisClass.defaultArgExprList.Add(bNode);
                    }
                }
                else if (vardecl.NameNode is BinaryExpressionNode)
                {
                    BinaryExpressionNode bNode = vardecl.NameNode as BinaryExpressionNode;
                    varIdent = bNode.LeftNode as IdentifierNode;

                    bNode.endCol = vardecl.endCol;
                    bNode.endLine = vardecl.endLine;

                    if (vardecl.IsStatic)
                        staticPropertyInitList.Add(bNode);
                    else
                        thisClass.defaultArgExprList.Add(bNode);
                }
                else
                {
                    Validity.Assert(false, "Check generated AST");
                }

                // It is possible that fail to allocate variable. In that 
                // case we should remove initializing expression from 
                // cnode's defaultArgExprList
                int symbolIndex = AllocateMemberVariable(thisClassIndex, thisClassIndex, varIdent.Value, vardecl.ArgumentType.rank, vardecl.access, vardecl.IsStatic);
                if (symbolIndex == ProtoCore.DSASM.Constants.kInvalidIndex)
                {
                    Validity.Assert(thisClass.defaultArgExprList.Count > 0);
                    thisClass.defaultArgExprList.RemoveAt(thisClass.defaultArgExprList.Count - 1);
                }
                // Only generate getter/setter for non-ffi class
                else if (!classDecl.IsExternLib)
                {
                    ProtoCore.DSASM.SymbolNode prop =
                        vardecl.IsStatic
                        ? core.CodeBlockList[0].symbolTable.symbolList[symbolIndex]
                        : core.ClassTable.ClassNodes[thisClassIndex].symbols.symbolList[symbolIndex];
                    ProtoCore.Type propType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, false);
                    string typeName = vardecl.ArgumentType.Name;
                    if (String.IsNullOrEmpty(typeName))
                    {
                        prop.datatype = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, false);
                    }
                    else
                    {
                        int type = core.TypeSystem.GetType(typeName);
                        if (type == (int)PrimitiveType.kInvalidType)
                        {
                            string message = String.Format(ProtoCore.BuildData.WarningMessage.kTypeUndefined, typeName);
                            core.BuildStatus.LogWarning(ProtoCore.BuildData.WarningID.kTypeUndefined, message, core.CurrentDSFileName, vardecl.line, vardecl.col);
                            prop.datatype = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, false);
                        }
                        else
                        {
                            bool isArray = vardecl.ArgumentType.IsIndexable;
                            int rank = vardecl.ArgumentType.rank;
                            prop.datatype = core.TypeSystem.BuildTypeObject(type, isArray, rank);
                            if (type != (int)PrimitiveType.kTypeVar || isArray)
                            {
                                prop.staticType = prop.datatype;
                            }
                        }
                    }

                    EmitGetterForProperty(classDecl, prop);
                    EmitSetterForProperty(classDecl, prop);
                }
            }
            classOffset = 0;

            // Now we are going to create a static function __init_static_properties()
            // which will initialize all static properties. We will emit a 
            // call to this function after all classes have been compiled.
            if (staticPropertyInitList.Count > 0 && !classDecl.IsExternLib)
            {
                FunctionDefinitionNode initFunc = new FunctionDefinitionNode
                {
                    Name = ProtoCore.DSASM.Constants.kStaticPropertiesInitializer,
                    Signature = new ArgumentSignatureNode(),
                    Pattern = null,
                    ReturnType = new ProtoCore.Type { Name = core.TypeSystem.GetType((int)PrimitiveType.kTypeNull), UID = (int)PrimitiveType.kTypeNull },
                    FunctionBody = new CodeBlockNode(),
                    IsExternLib = false,
                    IsDNI = false,
                    ExternLibName = null,
                    access = ProtoCore.DSASM.AccessSpecifier.kPublic,
                    IsStatic = true
                };
                classDecl.funclist.Add(initFunc);

                staticPropertyInitList.ForEach(bNode => initFunc.FunctionBody.Body.Add(bNode));
                initFunc.FunctionBody.Body.Add(new BinaryExpressionNode
                {
                    LeftNode = nodeBuilder.BuildReturn(),
                    Optr = ProtoCore.DSASM.Operator.assign,
                    RightNode = new NullNode()
                });
            }

            unPopulatedClasses.Remove(thisClassIndex);
        }

        private void EmitClassDeclNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            ClassDeclNode classDecl = node as ClassDeclNode;

            // Handling n-pass on class declaration
            if (ProtoCore.DSASM.AssociativeCompilePass.kClassName == compilePass)
            {
                // Class name pass
                // Populating the class tables with the class names
                if (null != codeBlock.parent)
                {
                    buildStatus.LogSemanticError("A class cannot be defined inside a language block.\n", core.CurrentDSFileName, classDecl.line, classDecl.col);
                }


                ProtoCore.DSASM.ClassNode thisClass = new ProtoCore.DSASM.ClassNode();
                thisClass.name = classDecl.className;
                thisClass.size = classDecl.varlist.Count;
                thisClass.IsImportedClass = classDecl.IsImportedClass;
                thisClass.typeSystem = core.TypeSystem;
                
                if (classDecl.ExternLibName != null)
                    thisClass.ExternLib = classDecl.ExternLibName;
                else
                    thisClass.ExternLib = Path.GetFileName(core.CurrentDSFileName);

                globalClassIndex = core.ClassTable.Append(thisClass);
                if (ProtoCore.DSASM.Constants.kInvalidIndex == globalClassIndex)
                {
                    string message = string.Format("Class redefinition '{0}' (BE1C3285).\n", classDecl.className);
                    buildStatus.LogSemanticError(message, core.CurrentDSFileName, classDecl.line, classDecl.col);
                    throw new BuildHaltException(message);
                }

                unPopulatedClasses.Add(globalClassIndex, classDecl);

                //Always allow us to convert a class to a bool
                thisClass.coerceTypes.Add((int)PrimitiveType.kTypeBool, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            }
            else if (ProtoCore.DSASM.AssociativeCompilePass.kClassHeirarchy == compilePass)
            {
                // Class heirarchy pass
                // Populating each class entry with their respective base classes
                globalClassIndex = core.ClassTable.GetClassId(classDecl.className);

                ProtoCore.DSASM.ClassNode thisClass = core.ClassTable.ClassNodes[globalClassIndex];

                // Verify and store the list of classes it inherits from 
                if (null != classDecl.superClass)
                {
                    for (int n = 0; n < classDecl.superClass.Count; ++n)
                    {
                        int baseClass = core.ClassTable.GetClassId(classDecl.superClass[n]);
                        if (baseClass == globalClassIndex)
                        {
                            string message = string.Format("Class '{0}' cannot derive from itself (DED0A61F).\n", classDecl.className);
                            buildStatus.LogSemanticError(message, core.CurrentDSFileName, classDecl.line, classDecl.col);
                            throw new BuildHaltException(message);
                        }

                        if (ProtoCore.DSASM.Constants.kInvalidIndex != baseClass)
                        {
                            if (core.ClassTable.ClassNodes[baseClass].IsImportedClass && !thisClass.IsImportedClass)
                            {
                                string message = string.Format("Cannot derive from FFI class {0} (DA87AC4D).\n",
                                    core.ClassTable.ClassNodes[baseClass].name);

                                buildStatus.LogSemanticError(message, core.CurrentDSFileName, classDecl.line, classDecl.col);
                                throw new BuildHaltException(message);
                            }

                            thisClass.baseList.Add(baseClass);
                            thisClass.coerceTypes.Add(baseClass, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceBaseClass);

                            
                            // Iterate through all the base classes until the the root class
                            // For every base class, add the coercion score

                            // TODO Jun: -Integrate this with multiple inheritace when supported
                            //           -Cleansify
                            ProtoCore.DSASM.ClassNode tmpCNode = core.ClassTable.ClassNodes[baseClass];
                            if (tmpCNode.baseList.Count > 0)
                            {
                                baseClass = tmpCNode.baseList[0];
                                while (ProtoCore.DSASM.Constants.kInvalidIndex != baseClass)
                                {
                                    thisClass.coerceTypes.Add(baseClass, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceBaseClass);
                                    tmpCNode = core.ClassTable.ClassNodes[baseClass];

                                    baseClass = ProtoCore.DSASM.Constants.kInvalidIndex;
                                    if (tmpCNode.baseList.Count > 0)
                                    {
                                        baseClass = tmpCNode.baseList[0];
                                    }
                                }
                            }
                        }
                        else
                        {
                            string message = string.Format("Unknown base class '{0}' (9E44FFB3).\n", classDecl.superClass[n]);
                            buildStatus.LogSemanticError(message, core.CurrentDSFileName, classDecl.line, classDecl.col);
                            throw new BuildHaltException(message);
                        }
                    }
                }
            }
            else if (ProtoCore.DSASM.AssociativeCompilePass.kClassMemVar == compilePass)
            {
                EmitMemberVariables(classDecl);
            }
            else if (ProtoCore.DSASM.AssociativeCompilePass.kClassMemFuncSig == compilePass)
            {
                // Class member variable pass
                // Populating each class entry vtables with their respective member variables signatures

                globalClassIndex = core.ClassTable.GetClassId(classDecl.className);
                List<AssociativeNode> thisPtrOverloadList = new List<AssociativeNode>();
                foreach (AssociativeNode funcdecl in classDecl.funclist)
                {
                    DfsTraverse(funcdecl, ref inferedType);

                    // If this is a function, create its parameterized this pointer overload
                    if (funcdecl is ProtoCore.AST.AssociativeAST.FunctionDefinitionNode)
                    {
                        string procName = (funcdecl as ProtoCore.AST.AssociativeAST.FunctionDefinitionNode).Name;

                        bool isFunctionExcluded = procName.Equals(ProtoCore.DSASM.Constants.kStaticPropertiesInitializer);

                        if (!isFunctionExcluded)
                        {
                            ThisPointerProcOverload thisProc = new ThisPointerProcOverload();
                            thisProc.classIndex = globalClassIndex;

                            thisProc.procNode = new ProtoCore.AST.AssociativeAST.FunctionDefinitionNode(funcdecl as ProtoCore.AST.AssociativeAST.FunctionDefinitionNode);
                            thisProc.procNode.IsAutoGeneratedThisProc = true;

                            InsertThisPointerArg(thisProc);
                            //InsertThisPointerAtBody(thisProc);

                            if (ProtoCore.Utils.CoreUtils.IsGetterSetter(procName))
                            {
                                InsertThisPointerAtBody(thisProc);
                            }
                            else
                            {
                                // This is a normal function
                                // the body thsould be the actaul function called through the this pointer argument
                                //
                                // def f() { return = 1 }
                                // def f(%this : A) { return = %this.f()}
                                //
                                BuildThisFunctionBody(thisProc);
                            }

                            // Emit the newly defined overloads
                            DfsTraverse(thisProc.procNode, ref inferedType);

                            thisPtrOverloadList.Add(thisProc.procNode);
                        }
                    }
                }
                classDecl.funclist.AddRange(thisPtrOverloadList);

                if (!classDecl.IsExternLib)
                {
                    ProtoCore.DSASM.ProcedureTable vtable = core.ClassTable.ClassNodes[globalClassIndex].vtable;
                    if (vtable.IndexOfExact(classDecl.className, new List<ProtoCore.Type>()) == ProtoCore.DSASM.Constants.kInvalidIndex)
                    {
                        ConstructorDefinitionNode defaultConstructor = new ConstructorDefinitionNode();
                        defaultConstructor.Name = classDecl.className;
                        defaultConstructor.localVars = 0;
                        defaultConstructor.Signature = new ArgumentSignatureNode();
                        defaultConstructor.Pattern = null;
                        defaultConstructor.ReturnType = new ProtoCore.Type { Name = "var", UID = 0 };
                        defaultConstructor.FunctionBody = new CodeBlockNode();
                        defaultConstructor.baseConstr = null;
                        defaultConstructor.access = ProtoCore.DSASM.AccessSpecifier.kPublic;
                        defaultConstructor.IsExternLib = false;
                        defaultConstructor.ExternLibName = null;
                        DfsTraverse(defaultConstructor, ref inferedType);
                        classDecl.funclist.Add(defaultConstructor);
                    }
                }
            }
            else if (ProtoCore.DSASM.AssociativeCompilePass.kGlobalScope == compilePass)
            {
                // before populate the attributes, we must know the attribute class constructor signatures
                // in order to check the parameter 
                // populate the attributes for the class and class member variable 
                globalClassIndex = core.ClassTable.GetClassId(classDecl.className);
                if (globalClassIndex != ProtoCore.DSASM.Constants.kInvalidIndex)
                {
                    ProtoCore.DSASM.ClassNode thisClass = core.ClassTable.ClassNodes[globalClassIndex];
                    // class
                    if (classDecl.Attributes != null)
                    {
                        thisClass.Attributes = PopulateAttributes(classDecl.Attributes);
                    }
                    // member variable
                    int ix = -1;
                    int currentClassScope = -1;
                    foreach (ProtoCore.DSASM.SymbolNode sn in thisClass.symbols.symbolList.Values)
                    {
                        // only populate the attributes for member variabls
                        if (sn.functionIndex != ProtoCore.DSASM.Constants.kInvalidIndex)
                            continue;
                        if (sn.classScope != globalClassIndex)
                        {
                            if (currentClassScope != sn.classScope)
                            {
                                currentClassScope = sn.classScope;
                                ix = 0;
                            }
                            // copy attribute information from base class
                            sn.Attributes = core.ClassTable.ClassNodes[currentClassScope].symbols.symbolList[ix++].Attributes;
                        }
                        else
                        {
                            if (currentClassScope != globalClassIndex)
                            {
                                currentClassScope = globalClassIndex;
                                ix = 0;
                            }
                            ProtoCore.AST.AssociativeAST.VarDeclNode varnode = classDecl.varlist[ix++] as ProtoCore.AST.AssociativeAST.VarDeclNode;
                            sn.Attributes = PopulateAttributes((varnode).Attributes);
                        }
                    }
                }
            }
            else if (ProtoCore.DSASM.AssociativeCompilePass.kClassMemFuncBody == compilePass)
            {
                // Class member variable pass
                // Populating the function body of each member function defined in the class vtables

                globalClassIndex = core.ClassTable.GetClassId(classDecl.className);

                foreach (AssociativeNode funcdecl in classDecl.funclist)
                {
                    // reset the inferedtype between functions
                    inferedType = new ProtoCore.Type();
                    DfsTraverse(funcdecl, ref inferedType, false, null, subPass);
                }
            }

            // Reset the class index
            core.ClassIndex = globalClassIndex = ProtoCore.DSASM.Constants.kGlobalScope;
        }

        private void EmitCallingForBaseConstructor(int thisClassIndex, ProtoCore.AST.AssociativeAST.FunctionCallNode baseConstructor)
        {
            List<ProtoCore.Type> argTypeList = new List<ProtoCore.Type>();
            int ctorIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
            int baseIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
            string baseConstructorName = null;

            if (baseConstructor != null)
            {
                if (baseConstructor.Function == null)
                {
                    int baseClassIndex = core.ClassTable.ClassNodes[thisClassIndex].baseList[0];
                    baseConstructorName = core.ClassTable.ClassNodes[baseClassIndex].name; 
                }
                else
                {
                    baseConstructorName = baseConstructor.Function.Name;
                }

                foreach (AssociativeNode paramNode in baseConstructor.FormalArguments)
                {
                    ProtoCore.Type paramType = new ProtoCore.Type();
                    paramType.UID = (int)ProtoCore.PrimitiveType.kTypeVoid;
                    paramType.IsIndexable = false;

                    emitReplicationGuide = true;
                    enforceTypeCheck = !(paramNode is BinaryExpressionNode);
                    DfsTraverse(paramNode, ref paramType, false, null, AssociativeSubCompilePass.kUnboundIdentifier);
                    DfsTraverse(paramNode, ref paramType, false, null, AssociativeSubCompilePass.kNone);
                    emitReplicationGuide = false;
                    enforceTypeCheck = true;
                    argTypeList.Add(paramType);
                }

                List<int> myBases = core.ClassTable.ClassNodes[globalClassIndex].baseList;
                foreach (int bidx in myBases)
                {
                    int cidx = core.ClassTable.ClassNodes[bidx].vtable.IndexOf(baseConstructorName, argTypeList, core.ClassTable);
                    if ((cidx != ProtoCore.DSASM.Constants.kInvalidIndex) &&
                        core.ClassTable.ClassNodes[bidx].vtable.procList[cidx].isConstructor)
                    {
                        ctorIndex = cidx;
                        baseIndex = bidx;
                        break;
                    }
                }
            }
            else
            {
                // call base class's default constructor
                // TODO keyu: to support multiple inheritance
                List<int> myBases = core.ClassTable.ClassNodes[globalClassIndex].baseList;
                foreach (int bidx in myBases)
                {
                    baseConstructorName = core.ClassTable.ClassNodes[bidx].name;
                    int cidx = core.ClassTable.ClassNodes[bidx].vtable.IndexOf(baseConstructorName, argTypeList, core.ClassTable);
                    // If the base class is a FFI class, it may not contain a 
                    // default constructor, so only assert for design script 
                    // class for which we always generate a default constructor.
                    if (!core.ClassTable.ClassNodes[bidx].IsImportedClass)
                    { 
                        Validity.Assert(cidx != ProtoCore.DSASM.Constants.kInvalidIndex);
                    }
                    ctorIndex = cidx;
                    baseIndex = bidx;
                }
            }

            if (ctorIndex != ProtoCore.DSASM.Constants.kInvalidIndex)
            {
                EmitInstrConsole(ProtoCore.DSASM.kw.push, codeBlock.codeBlockId + "[block]");
                StackValue opblock = StackValue.BuildBlockIndex(codeBlock.codeBlockId);
                EmitPush(opblock);

                EmitInstrConsole(ProtoCore.DSASM.kw.push, 0 + "[dim]");
                StackValue opdim = StackValue.BuildArrayDimension(0);
                EmitPush(opdim);

                EmitInstrConsole(ProtoCore.DSASM.kw.call, baseConstructorName);
                EmitCallBaseCtor(ctorIndex, baseIndex, 1);
            }
        }

        private void EmitConstructorDefinitionNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            ConstructorDefinitionNode funcDef = node as ConstructorDefinitionNode;
            ProtoCore.DSASM.CodeBlockType originalBlockType = codeBlock.blockType;
            codeBlock.blockType = ProtoCore.DSASM.CodeBlockType.kFunction;

            if (IsParsingMemberFunctionSig())
            {
                Validity.Assert(null == localProcedure);
                localProcedure = new ProtoCore.DSASM.ProcedureNode();

                localProcedure.name = funcDef.Name;
                localProcedure.pc = ProtoCore.DSASM.Constants.kInvalidIndex;
                localProcedure.localCount = 0;// Defer till all locals are allocated
                localProcedure.returntype.UID = globalClassIndex;
                localProcedure.returntype.IsIndexable = false;
                localProcedure.isConstructor = true;
                localProcedure.runtimeIndex = 0;
                localProcedure.isExternal = funcDef.IsExternLib;
                Validity.Assert(ProtoCore.DSASM.Constants.kInvalidIndex != globalClassIndex, "A constructor node must be associated with class");
                localProcedure.localCount = 0;
                localProcedure.classScope = globalClassIndex;

                int peekFunctionindex = core.ClassTable.ClassNodes[globalClassIndex].vtable.procList.Count;

                // Append arg symbols
                List<KeyValuePair<string, ProtoCore.Type>> argsToBeAllocated = new List<KeyValuePair<string, ProtoCore.Type>>();
                if (null != funcDef.Signature)
                {
                    int argNumber = 0;
                    foreach (VarDeclNode argNode in funcDef.Signature.Arguments)
                    {
                        ++argNumber;

                        IdentifierNode paramNode = null;
                        bool aIsDefault = false;
                        ProtoCore.AST.Node aDefaultExpression = null;
                        if (argNode.NameNode is IdentifierNode)
                        {
                            paramNode = argNode.NameNode as IdentifierNode;
                        }
                        else if (argNode.NameNode is BinaryExpressionNode)
                        {
                            BinaryExpressionNode bNode = argNode.NameNode as BinaryExpressionNode;
                            paramNode = bNode.LeftNode as IdentifierNode;
                            aIsDefault = true;
                            aDefaultExpression = bNode;
                            //buildStatus.LogSemanticError("Default parameters are not supported");
                            //throw new BuildHaltException();
                        }
                        else
                        {
                            Validity.Assert(false, "Check generated AST");
                        }

                        ProtoCore.Type argType = BuildArgumentTypeFromVarDeclNode(argNode);
                        argsToBeAllocated.Add(new KeyValuePair<string, ProtoCore.Type>(paramNode.Value, argType));
                        localProcedure.argTypeList.Add(argType);
                        ProtoCore.DSASM.ArgumentInfo argInfo = new ProtoCore.DSASM.ArgumentInfo { Name = paramNode.Value, isDefault = aIsDefault, defaultExpression = aDefaultExpression };
                        localProcedure.argInfoList.Add(argInfo);
                    }

                    localProcedure.isVarArg = funcDef.Signature.IsVarArg;
                }

                int findex = core.ClassTable.ClassNodes[globalClassIndex].vtable.Append(localProcedure);

                // Comment Jun: Catch this assert given the condition as this type of mismatch should never occur
                if (ProtoCore.DSASM.Constants.kInvalidIndex != findex)
                {
                    Validity.Assert(peekFunctionindex == localProcedure.procId);
                    argsToBeAllocated.ForEach(arg =>
                    {
                        int symbolIndex = AllocateArg(arg.Key, findex, arg.Value);
                        if (ProtoCore.DSASM.Constants.kInvalidIndex == symbolIndex)
                        {
                            throw new BuildHaltException("44B557F1");
                        }
                    });
                }
                else
                {
                    string message = String.Format(ProtoCore.BuildData.WarningMessage.kMethodAlreadyDefined, localProcedure.name);
                    buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kFunctionAlreadyDefined, message, core.CurrentDSFileName, funcDef.line, funcDef.col);
                    funcDef.skipMe = true;
                }
            }
            else if (IsParsingMemberFunctionBody())
            {
                EmitCompileLogFunctionStart(GetFunctionSignatureString(funcDef.Name, funcDef.ReturnType, funcDef.Signature, true));
                // Build arglist for comparison
                List<ProtoCore.Type> argList = new List<ProtoCore.Type>();
                if (null != funcDef.Signature)
                {
                    foreach (VarDeclNode argNode in funcDef.Signature.Arguments)
                    {
                        ProtoCore.Type argType = BuildArgumentTypeFromVarDeclNode(argNode);
                        argList.Add(argType);
                    }
                }

                globalProcIndex = core.ClassTable.ClassNodes[globalClassIndex].vtable.IndexOfExact(funcDef.Name, argList);

                Validity.Assert(null == localProcedure);
                localProcedure = core.ClassTable.ClassNodes[globalClassIndex].vtable.procList[globalProcIndex];

                Validity.Assert(null != localProcedure);
                localProcedure.Attributes = PopulateAttributes(funcDef.Attributes);
                // Its only on the parse body pass where the real pc is determined. Update this procedures' pc
                //Validity.Assert(ProtoCore.DSASM.Constants.kInvalidIndex == localProcedure.pc);
                localProcedure.pc = pc;

                EmitInstrConsole(ProtoCore.DSASM.kw.allocc, localProcedure.name);
                EmitAllocc(globalClassIndex);
                setConstructorStartPC = true;

                EmitCallingForBaseConstructor(globalClassIndex, funcDef.baseConstr);

                ProtoCore.FunctionEndPoint fep = null;
                if (!funcDef.IsExternLib)
                {
                    // Traverse default assignment for the class
                    emitDebugInfo = false;
                    foreach (BinaryExpressionNode bNode in core.ClassTable.ClassNodes[globalClassIndex].defaultArgExprList)
                    {

                        ProtoCore.AssociativeGraph.GraphNode graphNode = new ProtoCore.AssociativeGraph.GraphNode();
                        graphNode.isParent = true;
                        graphNode.exprUID = bNode.exprUID;
                        graphNode.modBlkUID = bNode.modBlkUID;
                        graphNode.procIndex = globalProcIndex;
                        graphNode.classIndex = globalClassIndex;
                        graphNode.isAutoGenerated = true;

                        EmitBinaryExpressionNode(bNode, ref inferedType, false, graphNode, subPass);
                    }

                    //Traverse default argument for the constructor
                    foreach (ProtoCore.DSASM.ArgumentInfo argNode in localProcedure.argInfoList)
                    {
                        if (!argNode.isDefault)
                        {
                            continue;
                        }
                        BinaryExpressionNode bNode = argNode.defaultExpression as BinaryExpressionNode;
                        // build a temporay node for statement : temp = defaultarg;
                        var iNodeTemp = nodeBuilder.BuildIdentfier(Constants.kTempDefaultArg);
                        BinaryExpressionNode bNodeTemp = new BinaryExpressionNode();
                        bNodeTemp.LeftNode = iNodeTemp;
                        bNodeTemp.Optr = ProtoCore.DSASM.Operator.assign;
                        bNodeTemp.RightNode = bNode.LeftNode;
                        EmitBinaryExpressionNode(bNodeTemp, ref inferedType);
                        //duild an inline conditional node for statement: defaultarg = (temp == DefaultArgNode) ? defaultValue : temp;
                        InlineConditionalNode icNode = new InlineConditionalNode();
                        icNode.IsAutoGenerated = true;
                        BinaryExpressionNode cExprNode = new BinaryExpressionNode();
                        cExprNode.Optr = ProtoCore.DSASM.Operator.eq;
                        cExprNode.LeftNode = iNodeTemp;
                        cExprNode.RightNode = new DefaultArgNode();
                        icNode.ConditionExpression = cExprNode;
                        icNode.TrueExpression = bNode.RightNode;
                        icNode.FalseExpression = iNodeTemp;
                        bNodeTemp.LeftNode = bNode.LeftNode;
                        bNodeTemp.RightNode = icNode;
                        EmitBinaryExpressionNode(bNodeTemp, ref inferedType);
                    }
                    emitDebugInfo = true;

                    // Traverse definition
                    foreach (AssociativeNode bnode in funcDef.FunctionBody.Body)
                    {
                        inferedType.UID = (int)PrimitiveType.kTypeVoid;
                        inferedType.rank = 0;

                        if (bnode is LanguageBlockNode)
                        {
                            // Build a binaryn node with a temporary lhs for every stand-alone language block
                            var iNode = nodeBuilder.BuildIdentfier(core.GenerateTempLangageVar());
                            BinaryExpressionNode langBlockNode = new BinaryExpressionNode();
                            langBlockNode.LeftNode = iNode;
                            langBlockNode.Optr = ProtoCore.DSASM.Operator.assign;
                            langBlockNode.RightNode = bnode;                            
                            DfsTraverse(langBlockNode, ref inferedType, false, null, subPass);
                        }
                        else
                        {
                            DfsTraverse(bnode, ref inferedType, false, null, subPass);
                        }
                    }

                    // All locals have been stack allocated, update the local count of this function
                    localProcedure.localCount = core.BaseOffset;
                    core.ClassTable.ClassNodes[globalClassIndex].vtable.procList[globalProcIndex].localCount = core.BaseOffset;

                    // Update the param stack indices of this function
                    foreach (ProtoCore.DSASM.SymbolNode symnode in core.ClassTable.ClassNodes[globalClassIndex].symbols.symbolList.Values)
                    {
                        if (symnode.functionIndex == globalProcIndex && symnode.isArgument)
                        {
                            symnode.index -= localProcedure.localCount;
                        }
                    }

                    // JIL FEP
                    ProtoCore.Lang.JILActivationRecord record = new ProtoCore.Lang.JILActivationRecord();
                    record.pc = localProcedure.pc;
                    record.locals = localProcedure.localCount;
                    record.classIndex = globalClassIndex;
                    record.funcIndex = globalProcIndex;

                    // Construct the fep arguments
                    fep = new ProtoCore.Lang.JILFunctionEndPoint(record);
                }
                else
                {
                    ProtoCore.Lang.JILActivationRecord jRecord = new ProtoCore.Lang.JILActivationRecord();
                    jRecord.pc = localProcedure.pc;
                    jRecord.locals = localProcedure.localCount;
                    jRecord.classIndex = globalClassIndex;
                    jRecord.funcIndex = localProcedure.procId;

                    ProtoCore.Lang.FFIActivationRecord record = new ProtoCore.Lang.FFIActivationRecord();
                    record.JILRecord = jRecord;
                    record.FunctionName = funcDef.Name;
                    record.ModuleName = funcDef.ExternLibName;
                    record.ModuleType = "dll";
                    record.IsDNI = false;
                    record.ReturnType = funcDef.ReturnType;
                    record.ParameterTypes = localProcedure.argTypeList;
                    fep = new ProtoCore.Lang.FFIFunctionEndPoint(record);
                }

                // Construct the fep arguments
                fep.FormalParams = new ProtoCore.Type[localProcedure.argTypeList.Count];
                fep.procedureNode = localProcedure;
                localProcedure.argTypeList.CopyTo(fep.FormalParams, 0);
                
                // TODO Jun: 'classIndexAtCallsite' is the class index as it is stored at the callsite function tables
                // Determine whether this still needs to be aligned to the actual 'classIndex' variable
                // The factors that will affect this is whether the 2 function tables (compiler and callsite) need to be merged
                int classIndexAtCallsite = globalClassIndex + 1;
                if (!core.FunctionTable.GlobalFuncTable.ContainsKey(classIndexAtCallsite))
                {
                    Dictionary<string, FunctionGroup> funcList = new Dictionary<string, FunctionGroup>();
                    core.FunctionTable.GlobalFuncTable.Add(classIndexAtCallsite, funcList);
                }

                Dictionary<string, FunctionGroup> fgroup = core.FunctionTable.GlobalFuncTable[classIndexAtCallsite];
                if (!fgroup.ContainsKey(funcDef.Name))
                {
                    // Create a new function group in this class
                    ProtoCore.FunctionGroup funcGroup = new ProtoCore.FunctionGroup();
                    funcGroup.FunctionEndPoints.Add(fep);

                    // Add this group to the class function tables
                    core.FunctionTable.GlobalFuncTable[classIndexAtCallsite].Add(funcDef.Name, funcGroup);
                }
                else
                {
                    // Add this fep into the exisitng function group
                    core.FunctionTable.GlobalFuncTable[classIndexAtCallsite][funcDef.Name].FunctionEndPoints.Add(fep);
                }

                int startpc = pc;

                // Constructors auto return
                EmitInstrConsole(ProtoCore.DSASM.kw.retc);

                // Stepping out of a constructor body will have the execution cursor 
                // placed right at the closing curly bracket of the constructor definition.
                // 
                int closeCurlyBracketLine = 0, closeCurlyBracketColumn = -1;
                if (null != funcDef.FunctionBody)
                {
                    closeCurlyBracketLine = funcDef.FunctionBody.endLine;
                    closeCurlyBracketColumn = funcDef.FunctionBody.endCol;
                }

                // The execution cursor covers exactly one character -- the closing 
                // curly bracket. Note that we decrement the start-column by one here 
                // because end-column of "FunctionBody" here is *after* the closing 
                // curly bracket, so we want one before that.
                // 
                EmitRetc(closeCurlyBracketLine, closeCurlyBracketColumn - 1,
                    closeCurlyBracketLine, closeCurlyBracketColumn);

                // Build and append a graphnode for this return statememt
                ProtoCore.DSASM.SymbolNode returnNode = new ProtoCore.DSASM.SymbolNode();
                returnNode.name = ProtoCore.DSDefinitions.Keyword.Return;

                ProtoCore.AssociativeGraph.GraphNode retNode = new ProtoCore.AssociativeGraph.GraphNode();
                //retNode.symbol = returnNode;
                retNode.PushSymbolReference(returnNode);
                retNode.procIndex = globalProcIndex;
                retNode.classIndex = globalClassIndex;
                retNode.updateBlock.startpc = startpc;
                retNode.updateBlock.endpc = pc - 1;

                codeBlock.instrStream.dependencyGraph.Push(retNode);
                EmitCompileLogFunctionEnd();
            }

            // Constructors have no return statemetns, reset variables here
            core.ProcNode = localProcedure = null;
            globalProcIndex = ProtoCore.DSASM.Constants.kGlobalScope;
            core.BaseOffset = 0;
            argOffset = 0;
            classOffset = 0;
            codeBlock.blockType = originalBlockType;
        }


        private void EmitFunctionDefinitionNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            bool parseGlobalFunctionBody = null == localProcedure && ProtoCore.DSASM.AssociativeCompilePass.kGlobalFuncBody == compilePass;
            bool parseMemberFunctionBody = ProtoCore.DSASM.Constants.kGlobalScope != globalClassIndex && ProtoCore.DSASM.AssociativeCompilePass.kClassMemFuncBody == compilePass;

            FunctionDefinitionNode funcDef = node as FunctionDefinitionNode;
            localFunctionDefNode = funcDef;

            if (funcDef.IsAssocOperator)
            {
                isAssocOperator = true;
            }
            else
            {
                isAssocOperator = false;
            }

            bool hasReturnStatement = false;
            ProtoCore.DSASM.CodeBlockType origCodeBlockType = codeBlock.blockType;
            codeBlock.blockType = ProtoCore.DSASM.CodeBlockType.kFunction;
            if (IsParsingGlobalFunctionSig() || IsParsingMemberFunctionSig())
            {
                Validity.Assert(null == localProcedure);
                localProcedure = new ProtoCore.DSASM.ProcedureNode();

                localProcedure.name = funcDef.Name;
                localProcedure.pc = ProtoCore.DSASM.Constants.kInvalidIndex;
                localProcedure.localCount = 0; // Defer till all locals are allocated
                localProcedure.returntype.UID = core.TypeSystem.GetType(funcDef.ReturnType.Name);
                if (localProcedure.returntype.UID == (int)PrimitiveType.kInvalidType)
                {
                    string message = String.Format(ProtoCore.BuildData.WarningMessage.kReturnTypeUndefined, funcDef.ReturnType.Name, funcDef.Name);
                    buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kTypeUndefined, message, core.CurrentDSFileName, funcDef.line, funcDef.col);
                    localProcedure.returntype.UID = (int)PrimitiveType.kTypeVar;
                }
                localProcedure.returntype.IsIndexable = funcDef.ReturnType.IsIndexable;
                localProcedure.returntype.rank = funcDef.ReturnType.rank;
                localProcedure.isConstructor = false;
                localProcedure.isStatic = funcDef.IsStatic;
                localProcedure.runtimeIndex = codeBlock.codeBlockId;
                localProcedure.access = funcDef.access;
                localProcedure.isExternal = funcDef.IsExternLib;
                localProcedure.isAutoGenerated = funcDef.IsAutoGenerated;
                localProcedure.classScope = globalClassIndex;
                localProcedure.isAssocOperator = funcDef.IsAssocOperator;
                localProcedure.isAutoGeneratedThisProc = funcDef.IsAutoGeneratedThisProc;

                localProcedure.MethodAttribute = funcDef.MethodAttributes;

                int peekFunctionindex = ProtoCore.DSASM.Constants.kInvalidIndex;

                if (ProtoCore.DSASM.Constants.kInvalidIndex == globalClassIndex)
                {
                    peekFunctionindex = codeBlock.procedureTable.procList.Count;
                }
                else
                {
                    peekFunctionindex = core.ClassTable.ClassNodes[globalClassIndex].vtable.procList.Count;
                }

                // Append arg symbols
                List<KeyValuePair<string, ProtoCore.Type>> argsToBeAllocated = new List<KeyValuePair<string, ProtoCore.Type>>();
                string functionDescription = localProcedure.name;
                if (null != funcDef.Signature)
                {
                    int argNumber = 0;
                    foreach (VarDeclNode argNode in funcDef.Signature.Arguments)
                    {
                        ++argNumber;

                        IdentifierNode paramNode = null;
                        bool aIsDefault = false;
                        ProtoCore.AST.Node aDefaultExpression = null;
                        if (argNode.NameNode is IdentifierNode)
                        {
                            paramNode = argNode.NameNode as IdentifierNode;
                        }
                        else if (argNode.NameNode is BinaryExpressionNode)
                        {
                            BinaryExpressionNode bNode = argNode.NameNode as BinaryExpressionNode;
                            paramNode = bNode.LeftNode as IdentifierNode;
                            aIsDefault = true;
                            aDefaultExpression = bNode;
                            //buildStatus.LogSemanticError("Defualt parameters are not supported");
                            //throw new BuildHaltException();
                        }
                        else
                        {
                            Validity.Assert(false, "Check generated AST");
                        }

                        ProtoCore.Type argType = BuildArgumentTypeFromVarDeclNode(argNode);
                        // We dont directly allocate arguments now
                        argsToBeAllocated.Add(new KeyValuePair<string, ProtoCore.Type>(paramNode.Value, argType));
                        
                        localProcedure.argTypeList.Add(argType);
                        ProtoCore.DSASM.ArgumentInfo argInfo = new ProtoCore.DSASM.ArgumentInfo { Name = paramNode.Value, isDefault = aIsDefault, defaultExpression = aDefaultExpression };
                        localProcedure.argInfoList.Add(argInfo);

                        functionDescription += argNode.ArgumentType.ToString();
                    }
                    localProcedure.HashID = functionDescription.GetHashCode();
                    localProcedure.isVarArg = funcDef.Signature.IsVarArg;
                }

                if (ProtoCore.DSASM.Constants.kInvalidIndex == globalClassIndex)
                {
                    globalProcIndex = codeBlock.procedureTable.Append(localProcedure);
                }
                else
                {
                    globalProcIndex = core.ClassTable.ClassNodes[globalClassIndex].vtable.Append(localProcedure);
                }

                // Comment Jun: Catch this assert given the condition as this type of mismatch should never occur
                if (ProtoCore.DSASM.Constants.kInvalidIndex != globalProcIndex)
                {
                    argsToBeAllocated.ForEach(arg =>
                    {
                        int symbolIndex = AllocateArg(arg.Key, globalProcIndex, arg.Value);
                        if (ProtoCore.DSASM.Constants.kInvalidIndex == symbolIndex)
                        {
                            throw new BuildHaltException("B2CB2093");
                        }
                    });

                    // TODO Jun: Remove this once agree that alltest cases assume the default assoc block is block 0
                    // NOTE: Only affects mirror, not actual execution
                    if (null == codeBlock.parent && pc <= 0)
                    {
                        // The first node in the top level block is a function
                        core.DSExecutable.isSingleAssocBlock = false;
                    }

#if ENABLE_EXCEPTION_HANDLING
                    core.ExceptionHandlingManager.Register(codeBlock.codeBlockId, globalProcIndex, globalClassIndex);
#endif
                }
                else
                {
                    string message = String.Format(ProtoCore.BuildData.WarningMessage.kMethodAlreadyDefined, localProcedure.name);
                    buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kFunctionAlreadyDefined, message, core.CurrentDSFileName, funcDef.line, funcDef.col);
                    funcDef.skipMe = true;
                }
            }
            else if (parseGlobalFunctionBody || parseMemberFunctionBody)
            {
                if (core.Options.DisableDisposeFunctionDebug)
                {
                    if (node.Name.Equals(ProtoCore.DSDefinitions.Keyword.Dispose))
                    {
                        core.Options.EmitBreakpoints = false;
                    }
                }

                // Build arglist for comparison
                List<ProtoCore.Type> argList = new List<ProtoCore.Type>();
                if (null != funcDef.Signature)
                {
                    foreach (VarDeclNode argNode in funcDef.Signature.Arguments)
                    {
                        ProtoCore.Type argType = BuildArgumentTypeFromVarDeclNode(argNode);
                        argList.Add(argType);
                    }
                }

                // Get the exisitng procedure that was added on the previous pass
                if (ProtoCore.DSASM.Constants.kInvalidIndex == globalClassIndex)
                {
                    globalProcIndex = codeBlock.procedureTable.IndexOfExact(funcDef.Name, argList);
                    localProcedure = codeBlock.procedureTable.procList[globalProcIndex];
                }
                else
                {
                    globalProcIndex = core.ClassTable.ClassNodes[globalClassIndex].vtable.IndexOfExact(funcDef.Name, argList);
                    localProcedure = core.ClassTable.ClassNodes[globalClassIndex].vtable.procList[globalProcIndex];
                }

                Validity.Assert(null != localProcedure);

                // code gen the attribute 
                localProcedure.Attributes = PopulateAttributes(funcDef.Attributes);
                // Its only on the parse body pass where the real pc is determined. Update this procedures' pc
                //Validity.Assert(ProtoCore.DSASM.Constants.kInvalidIndex == localProcedure.pc);
                localProcedure.pc = pc;

                // Copy the active function to the core so nested language blocks can refer to it
                core.ProcNode = localProcedure;

                ProtoCore.FunctionEndPoint fep = null;
                if (!funcDef.IsExternLib)
                {
                    //Traverse default argument
                    emitDebugInfo = false;
                    foreach (ProtoCore.DSASM.ArgumentInfo argNode in localProcedure.argInfoList)
                    {
                        if (!argNode.isDefault)
                        {
                            continue;
                        }
                        BinaryExpressionNode bNode = argNode.defaultExpression as BinaryExpressionNode;
                        // build a temporay node for statement : temp = defaultarg;
                        var iNodeTemp = nodeBuilder.BuildTempVariable();
                        var bNodeTemp = nodeBuilder.BuildBinaryExpression(iNodeTemp, bNode.LeftNode) as BinaryExpressionNode;
                        EmitBinaryExpressionNode(bNodeTemp, ref inferedType, false, null, AssociativeSubCompilePass.kUnboundIdentifier);
                        //duild an inline conditional node for statement: defaultarg = (temp == DefaultArgNode) ? defaultValue : temp;
                        InlineConditionalNode icNode = new InlineConditionalNode();
                        icNode.IsAutoGenerated = true;
                        BinaryExpressionNode cExprNode = new BinaryExpressionNode();
                        cExprNode.Optr = ProtoCore.DSASM.Operator.eq;
                        cExprNode.LeftNode = iNodeTemp;
                        cExprNode.RightNode = new DefaultArgNode();
                        icNode.ConditionExpression = cExprNode;
                        icNode.TrueExpression = bNode.RightNode;
                        icNode.FalseExpression = iNodeTemp;
                        bNodeTemp.LeftNode = bNode.LeftNode;
                        bNodeTemp.RightNode = icNode;
                        EmitBinaryExpressionNode(bNodeTemp, ref inferedType, false, null, AssociativeSubCompilePass.kUnboundIdentifier);
                    }
                    emitDebugInfo = true;

                    EmitCompileLogFunctionStart(GetFunctionSignatureString(funcDef.Name, funcDef.ReturnType, funcDef.Signature));

                    // Traverse definition
                    foreach (AssociativeNode bnode in funcDef.FunctionBody.Body)
                    {

                        //
                        // TODO Jun:    Handle stand alone language blocks
                        //              Integrate the subPass into a proper pass
                        //

                        ProtoCore.Type itype = new ProtoCore.Type();
                        itype.UID = (int)PrimitiveType.kTypeVar;

                        if (bnode is LanguageBlockNode)
                        {
                            // Build a binaryn node with a temporary lhs for every stand-alone language block
                            BinaryExpressionNode langBlockNode = new BinaryExpressionNode();
                            langBlockNode.LeftNode = nodeBuilder.BuildIdentfier(core.GenerateTempLangageVar());
                            langBlockNode.Optr = ProtoCore.DSASM.Operator.assign;
                            langBlockNode.RightNode = bnode;

                            //DfsTraverse(bnode, ref itype, false, null, ProtoCore.DSASM.AssociativeSubCompilePass.kNone);
                            DfsTraverse(langBlockNode, ref itype, false, null, subPass);
                        }
                        else
                        {
                            DfsTraverse(bnode, ref itype, false, null, subPass);
                        }

                        if (NodeUtils.IsReturnExpressionNode(bnode))
                            hasReturnStatement = true;
                    }
                    EmitCompileLogFunctionEnd();

                    // All locals have been stack allocated, update the local count of this function
                    localProcedure.localCount = core.BaseOffset;

                    if (ProtoCore.DSASM.Constants.kInvalidIndex == globalClassIndex)
                    {
                        localProcedure.localCount = core.BaseOffset;

                        // Update the param stack indices of this function
                        foreach (ProtoCore.DSASM.SymbolNode symnode in codeBlock.symbolTable.symbolList.Values)
                        {
                            if (symnode.functionIndex == localProcedure.procId && symnode.isArgument)
                            {
                                symnode.index -= localProcedure.localCount;
                            }
                        }
                    }
                    else
                    {
                        core.ClassTable.ClassNodes[globalClassIndex].vtable.procList[localProcedure.procId].localCount = core.BaseOffset;

                        // Update the param stack indices of this function
                        foreach (ProtoCore.DSASM.SymbolNode symnode in core.ClassTable.ClassNodes[globalClassIndex].symbols.symbolList.Values)
                        {
                            if (symnode.functionIndex == localProcedure.procId && symnode.isArgument)
                            {
                                symnode.index -= localProcedure.localCount;
                            }
                        }
                    }

                    ProtoCore.Lang.JILActivationRecord record = new ProtoCore.Lang.JILActivationRecord();
                    record.pc = localProcedure.pc;
                    record.locals = localProcedure.localCount;
                    record.classIndex = globalClassIndex;
                    record.funcIndex = localProcedure.procId;
                    fep = new ProtoCore.Lang.JILFunctionEndPoint(record);
                }
                else if (funcDef.BuiltInMethodId != ProtoCore.Lang.BuiltInMethods.MethodID.kInvalidMethodID)
                {
                    fep = new ProtoCore.Lang.BuiltInFunctionEndPoint(funcDef.BuiltInMethodId);
                }
                else
                {
                    ProtoCore.Lang.JILActivationRecord jRecord = new ProtoCore.Lang.JILActivationRecord();
                    jRecord.pc = localProcedure.pc;
                    jRecord.locals = localProcedure.localCount;
                    jRecord.classIndex = globalClassIndex;
                    jRecord.funcIndex = localProcedure.procId;

                    // TODO Jun/Luke: Wrap this into Core.Options and extend if needed
                  /*  bool isCSFFI = false;
                    if (isCSFFI)
                    {
                        ProtoCore.Lang.CSFFIActivationRecord record = new ProtoCore.Lang.CSFFIActivationRecord();
                        record.JILRecord = jRecord;
                        record.FunctionName = funcDef.Name;
                        record.ModuleName = funcDef.ExternLibName;
                        record.ModuleType = "dll";
                        record.IsDNI = funcDef.IsDNI;
                        record.ReturnType = funcDef.ReturnType;
                        record.ParameterTypes = localProcedure.argTypeList;
                        fep = new ProtoCore.Lang.CSFFIFunctionEndPoint(record);
                    }
                    else
                    {*/
                        ProtoCore.Lang.FFIActivationRecord record = new ProtoCore.Lang.FFIActivationRecord();
                        record.JILRecord = jRecord;
                        record.FunctionName = funcDef.Name;
                        record.ModuleName = funcDef.ExternLibName;
                        record.ModuleType = "dll";
                        record.IsDNI = funcDef.IsDNI;
                        record.ReturnType = funcDef.ReturnType;
                        record.ParameterTypes = localProcedure.argTypeList;
                        fep = new ProtoCore.Lang.FFIFunctionEndPoint(record);
                    //}
                }


                // Construct the fep arguments
                fep.FormalParams = new ProtoCore.Type[localProcedure.argTypeList.Count];
                fep.BlockScope = codeBlock.codeBlockId;
                fep.ClassOwnerIndex = localProcedure.classScope;
                fep.procedureNode = localProcedure;
                localProcedure.argTypeList.CopyTo(fep.FormalParams, 0);

                // TODO Jun: 'classIndexAtCallsite' is the class index as it is stored at the callsite function tables
                // Determine whether this still needs to be aligned to the actual 'classIndex' variable
                // The factors that will affect this is whether the 2 function tables (compiler and callsite) need to be merged
                int classIndexAtCallsite = globalClassIndex + 1;
                if (!core.FunctionTable.GlobalFuncTable.ContainsKey(classIndexAtCallsite))
                {
                    // Create a new table for the class as this class does not exist in the tables yet
                    Dictionary<string, FunctionGroup> funcList = new Dictionary<string, FunctionGroup>();
                    core.FunctionTable.GlobalFuncTable.Add(classIndexAtCallsite, funcList);
                }

                // Get the function group of the current class and see if the current function exists
                Dictionary<string, FunctionGroup> fgroup = core.FunctionTable.GlobalFuncTable[classIndexAtCallsite];
                if (!fgroup.ContainsKey(funcDef.Name))
                {
                    // If any functions in the base class have the same name, append them here
                    int ci = classIndexAtCallsite - 1;
                    if (ProtoCore.DSASM.Constants.kInvalidIndex != ci)
                    {
                        ProtoCore.DSASM.ClassNode cnode = core.ClassTable.ClassNodes[ci];
                        if (cnode.baseList.Count > 0)
                        {
                            Validity.Assert(1 == cnode.baseList.Count, "We don't support multiple inheritance yet");

                            ci = cnode.baseList[0];

                            Dictionary<string, FunctionGroup> tgroup = new Dictionary<string, FunctionGroup>();
                            int callsiteCI = ci + 1;
                            bool bSucceed = core.FunctionTable.GlobalFuncTable.TryGetValue(callsiteCI, out tgroup);
                            if (bSucceed)
                            {
                                if (tgroup.ContainsKey(funcDef.Name))
                                {
                                    // Get that base group - the group of function from the baseclass
                                    FunctionGroup basegroup = new FunctionGroup();
                                    bSucceed = tgroup.TryGetValue(funcDef.Name, out basegroup);
                                    if (bSucceed)
                                    {
                                        // Copy all non-private feps from the basegroup into this the new group
                                        FunctionGroup newGroup = new FunctionGroup();
                                        newGroup.CopyVisible(basegroup.FunctionEndPoints);

                                        // Append the new fep 
                                        newGroup.FunctionEndPoints.Add(fep);

                                        // Copy the new group to this class table
                                        core.FunctionTable.GlobalFuncTable[classIndexAtCallsite].Add(funcDef.Name, newGroup);
                                    }
                                }
                                else
                                {
                                    // Create a new function group in this class
                                    ProtoCore.FunctionGroup funcGroup = new ProtoCore.FunctionGroup();
                                    funcGroup.FunctionEndPoints.Add(fep);

                                    // Add this group to the class function tables
                                    core.FunctionTable.GlobalFuncTable[classIndexAtCallsite].Add(funcDef.Name, funcGroup);
                                }
                            }
                            else
                            {
                                // Create a new function group in this class
                                ProtoCore.FunctionGroup funcGroup = new ProtoCore.FunctionGroup();
                                funcGroup.FunctionEndPoints.Add(fep);

                                // Add this group to the class function tables
                                core.FunctionTable.GlobalFuncTable[classIndexAtCallsite].Add(funcDef.Name, funcGroup);
                            }
                            cnode = core.ClassTable.ClassNodes[ci];
                        }
                        else
                        {
                            // Create a new function group in this class
                            ProtoCore.FunctionGroup funcGroup = new ProtoCore.FunctionGroup();
                            funcGroup.FunctionEndPoints.Add(fep);

                            // Add this group to the class function tables
                            core.FunctionTable.GlobalFuncTable[classIndexAtCallsite].Add(funcDef.Name, funcGroup);
                        }
                    }
                    else
                    {
                        // Create a new function group in this class
                        ProtoCore.FunctionGroup funcGroup = new ProtoCore.FunctionGroup();
                        funcGroup.FunctionEndPoints.Add(fep);

                        // Add this group to the class function tables
                        core.FunctionTable.GlobalFuncTable[classIndexAtCallsite].Add(funcDef.Name, funcGroup);
                    }
                }
                else
                {
                    // Add this fep into the exisitng function group
                    core.FunctionTable.GlobalFuncTable[classIndexAtCallsite][funcDef.Name].FunctionEndPoints.Add(fep);
                }

                if (!hasReturnStatement && !funcDef.IsExternLib)
                {
                    if (!core.Options.SuppressFunctionResolutionWarning)
                    {
                        string message = String.Format(ProtoCore.BuildData.WarningMessage.kFunctionNotReturnAtAllCodePaths, localProcedure.name);
                        buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kMissingReturnStatement, message, core.CurrentDSFileName, funcDef.line, funcDef.col);
                    }
                    EmitReturnNull();
                }

                if (core.Options.DisableDisposeFunctionDebug)
                {
                    if (node.Name.Equals(ProtoCore.DSDefinitions.Keyword.Dispose))
                    {
                        core.Options.EmitBreakpoints = true;
                    }
                }
            }

            core.ProcNode = localProcedure = null;
            codeBlock.blockType = origCodeBlockType;
            globalProcIndex = ProtoCore.DSASM.Constants.kGlobalScope;
            localFunctionDefNode = null;
            core.BaseOffset = 0;
            argOffset = 0;
            isAssocOperator = false;
        }

        private void EmitFunctionCallNode(AssociativeNode node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, 
            ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone, ProtoCore.AST.AssociativeAST.BinaryExpressionNode parentNode = null)
        {
            FunctionCallNode fnode = node as FunctionCallNode;
           
            bool dependentState = false;
            if (null != graphNode)
            {
                dependentState = graphNode.allowDependents;
                if (fnode != null && !ProtoCore.Utils.CoreUtils.IsSetter(fnode.Function.Name))
                {
                    graphNode.allowDependents = true;
                }
            }

            bool emitReplicationGuideFlag = emitReplicationGuide;
            bool lhsIndexing = false;
            if (graphNode != null)
            {
                lhsIndexing = graphNode.isIndexingLHS;
                graphNode.isIndexingLHS = false;
            }
            bool arrayIndexing = IsAssociativeArrayIndexing;
            IsAssociativeArrayIndexing = false;


            // Handle static calls to reflect the original call
            if (core.Options.FullSSA)
            {
                BuildRealDependencyForIdentList(graphNode);

                if (node is FunctionDotCallNode)
                {
                    if ((node as FunctionDotCallNode).isLastSSAIdentListFactor)
                    {
                        Validity.Assert(null != ssaPointerList);
                        ssaPointerList.Clear();
                    }
                }

                //if (resolveStatic)
                {
                    if (node is FunctionDotCallNode)
                    {
                        FunctionDotCallNode dotcall = node as FunctionDotCallNode;
                        Validity.Assert(null != dotcall.DotCall);
                        if (null != dotcall.staticLHSIdent)
                        {
                            string identName = dotcall.staticLHSIdent.Name;
                            string fullClassName;
                            bool isClassName = core.ClassTable.TryGetFullyQualifiedName(identName, out fullClassName);
                            if (isClassName)
                            {
                                ProtoCore.DSASM.SymbolNode symbolnode = null;
                                bool isAccessible = false;
                                bool isLHSAllocatedVariable = VerifyAllocation(identName, globalClassIndex, globalProcIndex, out symbolnode, out isAccessible);

                                bool isRHSConstructor = false;
                                int classIndex = core.ClassTable.IndexOf(identName);
                                if (classIndex != ProtoCore.DSASM.Constants.kInvalidIndex)
                                {

                                    string functionName = dotcall.FunctionCall.Function.Name;
                                    ProcedureNode callNode = core.ClassTable.ClassNodes[classIndex].GetFirstMemberFunction(functionName);
                                    if (null != callNode)
                                    {
                                        isRHSConstructor = callNode.isConstructor;
                                    }
                                }

                                bool isFunctionCallOnAllocatedClassName = isLHSAllocatedVariable && !isRHSConstructor;
                                if (!isFunctionCallOnAllocatedClassName || isRHSConstructor)
                                {
                                    ssaPointerList.Clear();

                                    dotcall.DotCall.FormalArguments[0] = dotcall.staticLHSIdent;

                                    staticClass = null;
                                    resolveStatic = false;

                                    ssaPointerList.Clear();
                                }
                            }
                        }
                    }
                }
            }

            ProtoCore.DSASM.ProcedureNode procNode = TraverseFunctionCall(node, null, ProtoCore.DSASM.Constants.kInvalidIndex, 0, ref inferedType, graphNode, subPass, parentNode);
            emitReplicationGuide = emitReplicationGuideFlag;
            if (graphNode != null)
            {
                graphNode.isIndexingLHS = lhsIndexing;
            }

            if (null != procNode && !procNode.isConstructor)
            {
                functionCallStack.Add(procNode);
                if (null != graphNode)
                {
                    graphNode.firstProc = procNode;
                    graphNode.firstProcRefIndex = graphNode.dependentList.Count - 1;

                    // Memoize the graphnode that contains a user-defined function call
                    if (!procNode.isExternal)
                    {
                        core.GraphNodeCallList.Add(graphNode);
                    }
                }
            }
            IsAssociativeArrayIndexing = arrayIndexing;

            inferedType.UID = isBooleanOp ? (int)PrimitiveType.kTypeBool : inferedType.UID;

            if (subPass == AssociativeSubCompilePass.kNone)
            {
                if (fnode != null && fnode.ArrayDimensions != null)
                {
                    emitReplicationGuideFlag = emitReplicationGuide;
                    emitReplicationGuide = false;
                    int dimensions = DfsEmitArrayIndexHeap(fnode.ArrayDimensions, graphNode);
                    EmitInstrConsole(ProtoCore.DSASM.kw.pushindex, dimensions.ToString() + "[dim]");
                    EmitPushArrayIndex(dimensions);
                    fnode.ArrayDimensions = null;
                    emitReplicationGuide = emitReplicationGuideFlag;
                }

                List<ProtoCore.AST.AssociativeAST.AssociativeNode> replicationGuides = null;
                bool isRangeExpression = false;

                if (fnode != null)
                {
                    replicationGuides = fnode.ReplicationGuides;
                    isRangeExpression = fnode.Function.Name.Equals(Constants.kFunctionRangeExpression);
                }
                else if (node is FunctionDotCallNode)
                {
                    FunctionCallNode funcNode = (node as FunctionDotCallNode).FunctionCall;
                    replicationGuides = (funcNode.Function as IdentifierNode).ReplicationGuides;
                }

                // YuKe: The replication guide for range expression will be 
                // generated in EmitRangeExprNode(). 
                // 
                // TODO: Revisit this piece of code to see how to handle array
                // index.
                if (!isRangeExpression && core.Options.TempReplicationGuideEmptyFlag && emitReplicationGuide)
                {
                    int guides = EmitReplicationGuides(replicationGuides);
                    EmitInstrConsole(ProtoCore.DSASM.kw.pushindex, guides + "[guide]");
                    EmitPushReplicationGuide(guides);
                }
            }

            if (null != graphNode)
            {
                graphNode.allowDependents = dependentState;
            }
        }

        private void EmitModifierStackNode(AssociativeNode node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            if (!IsParsingGlobal() && !IsParsingGlobalFunctionBody() && !IsParsingMemberFunctionBody())
            {
                return;
            }

            AssociativeNode prevElement = null;

            //core.Options.EmitBreakpoints = false;
            ModifierStackNode m = node as ModifierStackNode;

            int ci = Constants.kInvalidIndex;
            for(int i = 0; i < m.ElementNodes.Count; ++i)
            {
                bool emitBreakpointForPop = false;
                AssociativeNode modifierNode = m.ElementNodes[i];
                
                // Convert Function call nodes into function dot call nodes 
                if (modifierNode is BinaryExpressionNode)
                {
                    BinaryExpressionNode bnode = modifierNode as BinaryExpressionNode;
                    if (bnode.LeftNode.Name.StartsWith(ProtoCore.DSASM.Constants.kTempModifierStateNamePrefix))
                    {
                        emitBreakpointForPop = true;
                    }

                    if (!ProtoCore.Utils.CoreUtils.IsSSATemp(bnode.LeftNode.Name))
                    {
                        prevElement = bnode;
                    }

                    // Get class index from function call node if it is constructor call
                    if (bnode.RightNode is FunctionDotCallNode)
                    {
                        FunctionDotCallNode fnode = bnode.RightNode as FunctionDotCallNode;
                        IdentifierNode ident = fnode.DotCall.FormalArguments[0] as IdentifierNode;
                        string name = "";
                        if (ident != null)
                        {
                            name = ident.Value;
                        }
                        if (core.Options.FullSSA)
                        {
                            // For SSA'd ident lists, the lhs (class name) is stored in fnode.staticLHSIdent
                            if (null != fnode.staticLHSIdent)
                            {
                                name = fnode.staticLHSIdent.Name;
                            }
                        }

                        ci = core.ClassTable.IndexOf(name);
                        NodeUtils.SetNodeStartLocation(bnode, fnode.DotCall);
                    }
                    // Check if the right node of the modifierNode is a FunctionCall node
                    else if (bnode.RightNode is FunctionCallNode && ci != Constants.kInvalidIndex)
                    {
                        FunctionCallNode rnode = bnode.RightNode as FunctionCallNode;

                        if (i >= 1)
                        {
                            // Use class index to search for function call node and return procedure node
                            ProcedureNode procCallNode = core.ClassTable.ClassNodes[ci].GetFirstMemberFunction(rnode.Function.Name, rnode.FormalArguments.Count);

                            // Only if procedure node is non-null do we know its a member function and prefix it with an instance pointer
                            if (procCallNode != null)
                            {
                                ////////////////////////////////      
                                BinaryExpressionNode previousElementNode = null;
                                if (core.Options.FullSSA)
                                {
                                    Validity.Assert(null != prevElement && prevElement is BinaryExpressionNode);
                                    previousElementNode = prevElement as BinaryExpressionNode; 
                                }
                                else
                                {
                                    previousElementNode = m.ElementNodes[i - 1] as BinaryExpressionNode;
                                }
                                Validity.Assert(null != previousElementNode);
                                AssociativeNode lhs = previousElementNode.LeftNode;
                                bnode.RightNode = ProtoCore.Utils.CoreUtils.GenerateCallDotNode(lhs, rnode, core);

                                FunctionDotCallNode fdcNode = bnode.RightNode as FunctionDotCallNode;
                                if (fdcNode != null)
                                {
                                    NodeUtils.SetNodeStartLocation(fdcNode.DotCall, rnode);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // We should never get here. Fix it!
                    Validity.Assert(null != "Unknown node type!");
                }

                DebugProperties.BreakpointOptions oldOptions = core.DebugProps.breakOptions;

                if (emitBreakpointForPop)
                {
                    DebugProperties.BreakpointOptions newOptions = oldOptions;
                    newOptions |= DebugProperties.BreakpointOptions.EmitPopForTempBreakpoint;
                    core.DebugProps.breakOptions = newOptions;
                }

                DfsTraverse(modifierNode, ref inferedType, isBooleanOp, graphNode, subPass);
                core.DebugProps.breakOptions = oldOptions;
            }
            //core.Options.EmitBreakpoints = true;
        }

        private void EmitIfStatementNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.AssociativeGraph.GraphNode graphNode = null)
        {
            int bp = (int)ProtoCore.DSASM.Constants.kInvalidIndex;
            int L1 = (int)ProtoCore.DSASM.Constants.kInvalidIndex;
            int L2 = (int)ProtoCore.DSASM.Constants.kInvalidIndex;

            // If-expr
            IfStatementNode ifnode = node as IfStatementNode;
            DfsTraverse(ifnode.ifExprNode, ref inferedType, false, graphNode);

            EmitInstrConsole(ProtoCore.DSASM.kw.pop, ProtoCore.DSASM.kw.regCX);
            StackValue opCX = StackValue.BuildRegister(Registers.CX);
            EmitPop(opCX, Constants.kGlobalScope);

            L1 = pc + 1;
            L2 = ProtoCore.DSASM.Constants.kInvalidIndex;
            bp = pc;
            EmitCJmp(L1, L2);


            // Create a new codeblock for this block
            // Set the current codeblock as the parent of the new codeblock
            // Set the new codeblock as a new child of the current codeblock
            // Set the new codeblock as the current codeblock
            ProtoCore.DSASM.CodeBlock localCodeBlock = new ProtoCore.DSASM.CodeBlock(
                ProtoCore.DSASM.CodeBlockType.kConstruct,
                Language.kInvalid,
                core.CodeBlockIndex++,
                new ProtoCore.DSASM.SymbolTable(GetConstructBlockName("if"), core.RuntimeTableIndex++),
                null);


            localCodeBlock.instrStream = codeBlock.instrStream;
            localCodeBlock.parent = codeBlock;
            codeBlock.children.Add(localCodeBlock);
            codeBlock = localCodeBlock;
            core.CompleteCodeBlockList.Add(localCodeBlock);
            // If-body
            foreach (AssociativeNode ifBody in ifnode.IfBody)
            {
                inferedType = new ProtoCore.Type();
                inferedType.UID = (int)PrimitiveType.kTypeVar;
                DfsTraverse(ifBody, ref inferedType, false, graphNode);
            }

            // Restore - Set the local codeblock parent to be the current codeblock
            codeBlock = localCodeBlock.parent;


            L1 = ProtoCore.DSASM.Constants.kInvalidIndex;

            BackpatchTable backpatchTable = new BackpatchTable();
            backpatchTable.Append(pc, L1);
            EmitJmp(L1);

            // Backpatch the L2 destination of the if block
            Backpatch(bp, pc);


            /*		
            else if(E)	->	traverse E
                            L1 = pc + 1
                            L2 = null 
                            bp = pc
                            emit(jmp, _cx, L1, L2) 
            {				
                S		->	traverse S
                            L1 = null
                            bpTable.append(pc)
                            emit(jmp,labelEnd) 
                            backpatch(bp,pc)
            }
             * */

            // Elseif-expr


            /*
            else 			
            {				
                S		->	traverse S
                            L1 = null
                            bpTable.append(pc)
                            emit(jmp,labelEnd) 
                            backpatch(bp,pc)
            }		
             * */
            // Else-body     

            Validity.Assert(null != ifnode.ElseBody);
            if (0 != ifnode.ElseBody.Count)
            {
                // Create a new symboltable for this block
                // Set the current table as the parent of the new table
                // Set the new table as a new child of the current table
                // Set the new table as the current table
                // Create a new codeblock for this block
                // Set the current codeblock as the parent of the new codeblock
                // Set the new codeblock as a new child of the current codeblock
                // Set the new codeblock as the current codeblock
                localCodeBlock = new ProtoCore.DSASM.CodeBlock(
                    ProtoCore.DSASM.CodeBlockType.kConstruct,
                    Language.kInvalid,
                    core.CodeBlockIndex++,
                    new ProtoCore.DSASM.SymbolTable(GetConstructBlockName("else"), core.RuntimeTableIndex++),
                    null);

                localCodeBlock.instrStream = codeBlock.instrStream;
                localCodeBlock.parent = codeBlock;
                codeBlock.children.Add(localCodeBlock);
                codeBlock = localCodeBlock;
                core.CompleteCodeBlockList.Add(localCodeBlock);
                foreach (AssociativeNode elseBody in ifnode.ElseBody)
                {
                    inferedType = new ProtoCore.Type();
                    inferedType.UID = (int)PrimitiveType.kTypeVar;
                    DfsTraverse(elseBody, ref inferedType, false, graphNode);
                }

                // Restore - Set the local codeblock parent to be the current codeblock
                codeBlock = localCodeBlock.parent;

                L1 = ProtoCore.DSASM.Constants.kInvalidIndex;
                backpatchTable.Append(pc, L1);
                EmitJmp(L1);
            }

            /*
             * 
                      ->	backpatch(bpTable, pc) 
             */
            // ifstmt-exit
            // Backpatch all the previous unconditional jumps
            Backpatch(backpatchTable.backpatchList, pc);
        }

        private void EmitDynamicBlockNode(int block, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            if (subPass == ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
            {
                return;
            }
            EmitInstrConsole(ProtoCore.DSASM.kw.push, "dynamicBlock");
            EmitPush(StackValue.BuildInt(block));

            if (core.Options.TempReplicationGuideEmptyFlag && emitReplicationGuide)
            {
                EmitInstrConsole(ProtoCore.DSASM.kw.pushindex, 0 + "[guide]");
                EmitPushReplicationGuide(0);
            }
        }

        private void EmitInlineConditionalNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone, ProtoCore.AST.AssociativeAST.BinaryExpressionNode parentNode = null)
        {
            if (subPass == AssociativeSubCompilePass.kUnboundIdentifier)
            {
                return;
            }

            bool isInlineConditionalFlag = false;
            int startPC = pc;
            bool isReturn = false;
            if (graphNode != null)
            {
                isInlineConditionalFlag = graphNode.isInlineConditional;
                graphNode.isInlineConditional = true;
                startPC = graphNode.updateBlock.startpc;
                isReturn = graphNode.isReturn;
            }

            InlineConditionalNode inlineConditionalNode = node as InlineConditionalNode;
            // TODO: Jun, this 'if' condition needs to be removed as it was the old implementation - pratapa
            if (inlineConditionalNode.IsAutoGenerated)
            {
                // Normal inline conditional

                IfStatementNode ifNode = new IfStatementNode();
                ifNode.ifExprNode = inlineConditionalNode.ConditionExpression;
                List<AssociativeNode> ifBody = new List<AssociativeNode>();
                List<AssociativeNode> elseBody = new List<AssociativeNode>();
                ifBody.Add(inlineConditionalNode.TrueExpression);
                elseBody.Add(inlineConditionalNode.FalseExpression);
                ifNode.IfBody = ifBody;
                ifNode.ElseBody = elseBody;

                EmitIfStatementNode(ifNode, ref inferedType, graphNode);
            }
            else
            {
                // CPS inline conditional
                
                FunctionCallNode inlineCall = new FunctionCallNode();
                IdentifierNode identNode = new IdentifierNode();
                identNode.Name = ProtoCore.DSASM.Constants.kInlineConditionalMethodName;
                inlineCall.Function = identNode;

                DebugProperties.BreakpointOptions oldOptions = core.DebugProps.breakOptions;
                DebugProperties.BreakpointOptions newOptions = oldOptions;
                newOptions |= DebugProperties.BreakpointOptions.EmitInlineConditionalBreakpoint;
                core.DebugProps.breakOptions = newOptions;

                core.DebugProps.highlightRange = new ProtoCore.CodeModel.CodeRange
                {
                    StartInclusive = new ProtoCore.CodeModel.CodePoint
                    {
                        LineNo = parentNode.line,
                        CharNo = parentNode.col
                    },

                    EndExclusive = new ProtoCore.CodeModel.CodePoint
                    {
                        LineNo = parentNode.endLine,
                        CharNo = parentNode.endCol
                    }
                };

                // True condition language block
                BinaryExpressionNode bExprTrue = new BinaryExpressionNode();
                bExprTrue.LeftNode = nodeBuilder.BuildReturn();
                bExprTrue.Optr = Operator.assign;
                bExprTrue.RightNode = inlineConditionalNode.TrueExpression;

                LanguageBlockNode langblockT = new LanguageBlockNode();
                int trueBlockId = ProtoCore.DSASM.Constants.kInvalidIndex;
                langblockT.codeblock.language = ProtoCore.Language.kAssociative;
                langblockT.codeblock.fingerprint = "";
                langblockT.codeblock.version = "";
                core.AssocNode = bExprTrue;
                EmitDynamicLanguageBlockNode(langblockT, bExprTrue, ref inferedType, ref trueBlockId, graphNode, AssociativeSubCompilePass.kNone);
                core.AssocNode = null;
                ProtoCore.AST.AssociativeAST.DynamicBlockNode dynBlockT = new ProtoCore.AST.AssociativeAST.DynamicBlockNode(trueBlockId);


                // False condition language block
                BinaryExpressionNode bExprFalse = new BinaryExpressionNode();
                bExprFalse.LeftNode = nodeBuilder.BuildReturn();
                bExprFalse.Optr = Operator.assign;
                bExprFalse.RightNode = inlineConditionalNode.FalseExpression;

                LanguageBlockNode langblockF = new LanguageBlockNode();
                int falseBlockId = ProtoCore.DSASM.Constants.kInvalidIndex;
                langblockF.codeblock.language = ProtoCore.Language.kAssociative;
                langblockF.codeblock.fingerprint = "";
                langblockF.codeblock.version = "";
                core.AssocNode = bExprFalse;
                EmitDynamicLanguageBlockNode(langblockF, bExprFalse, ref inferedType, ref falseBlockId, graphNode, AssociativeSubCompilePass.kNone);
                core.AssocNode = null;
                ProtoCore.AST.AssociativeAST.DynamicBlockNode dynBlockF = new ProtoCore.AST.AssociativeAST.DynamicBlockNode(falseBlockId);

                core.DebugProps.breakOptions = oldOptions;
                core.DebugProps.highlightRange = new ProtoCore.CodeModel.CodeRange
                {
                    StartInclusive = new ProtoCore.CodeModel.CodePoint
                    {
                        LineNo = Constants.kInvalidIndex,
                        CharNo = Constants.kInvalidIndex
                    },

                    EndExclusive = new ProtoCore.CodeModel.CodePoint
                    {
                        LineNo = Constants.kInvalidIndex,
                        CharNo = Constants.kInvalidIndex
                    }
                };

                inlineCall.FormalArguments.Add(inlineConditionalNode.ConditionExpression);
                inlineCall.FormalArguments.Add(dynBlockT);
                inlineCall.FormalArguments.Add(dynBlockF);

                // Save the pc and store it after the call
                EmitFunctionCallNode(inlineCall, ref inferedType, false, graphNode, AssociativeSubCompilePass.kUnboundIdentifier);
                EmitFunctionCallNode(inlineCall, ref inferedType, false, graphNode, AssociativeSubCompilePass.kNone, parentNode);

                // Need to restore those settings.
                if (graphNode != null)
                {
                    graphNode.isInlineConditional = isInlineConditionalFlag;
                    graphNode.updateBlock.startpc = startPC;
                    graphNode.isReturn = isReturn;
                }
            }
        }

        private void EmitUnaryExpressionNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            UnaryExpressionNode u = node as UnaryExpressionNode;
            bool isPrefixOperation = ProtoCore.DSASM.UnaryOperator.Increment == u.Operator || ProtoCore.DSASM.UnaryOperator.Decrement == u.Operator;

            //(Ayush) In case of prefix, apply prefix operation first
            if (isPrefixOperation)
            {
                if (u.Expression is IdentifierListNode || u.Expression is IdentifierNode)
                {
                    BinaryExpressionNode binRight = new BinaryExpressionNode();
                    BinaryExpressionNode bin = new BinaryExpressionNode();
                    binRight.LeftNode = u.Expression;
                    binRight.RightNode = new IntNode { value = "1" };
                    binRight.Optr = (ProtoCore.DSASM.UnaryOperator.Increment == u.Operator) ? ProtoCore.DSASM.Operator.add : ProtoCore.DSASM.Operator.sub;
                    bin.LeftNode = u.Expression; bin.RightNode = binRight; bin.Optr = ProtoCore.DSASM.Operator.assign;
                    EmitBinaryExpressionNode(bin, ref inferedType, false, graphNode, subPass);
                }
                else
                    throw new BuildHaltException("Invalid use of prefix operation (DCDDEEF1).");
            }

            DfsTraverse(u.Expression, ref inferedType, false, graphNode, subPass);

            if (!isPrefixOperation && subPass != ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
            {
                EmitInstrConsole(ProtoCore.DSASM.kw.pop, ProtoCore.DSASM.kw.regAX);
                StackValue opAX = StackValue.BuildRegister(Registers.AX);
                EmitPop(opAX, Constants.kGlobalScope);

                string op = Op.GetUnaryOpName(u.Operator);
                EmitInstrConsole(op, ProtoCore.DSASM.kw.regAX);
                EmitUnary(Op.GetUnaryOpCode(u.Operator), opAX);

                EmitInstrConsole(ProtoCore.DSASM.kw.push, ProtoCore.DSASM.kw.regAX);
                StackValue opRes = StackValue.BuildRegister(Registers.AX);
                EmitPush(opRes);
            }
        }

        private void GetFirstSymbolFromIdentList(ProtoCore.AST.Node pNode, ref ProtoCore.DSASM.SymbolNode firstSymbol)
        {
            if (pNode is ProtoCore.AST.AssociativeAST.IdentifierListNode)
            {
                ProtoCore.AST.AssociativeAST.IdentifierListNode listNode = pNode as ProtoCore.AST.AssociativeAST.IdentifierListNode;
                GetFirstSymbolFromIdentList(listNode.LeftNode, ref firstSymbol);
            }
            else if (pNode is ProtoCore.AST.AssociativeAST.IdentifierNode)
            {
                firstSymbol = null;
                ProtoCore.AST.AssociativeAST.IdentifierNode iNode = pNode as ProtoCore.AST.AssociativeAST.IdentifierNode;
                bool isAccessible = false;
                bool isAllocated = VerifyAllocation(iNode.Name, globalClassIndex, globalProcIndex, out firstSymbol, out isAccessible);
            }
            else
            {
                Validity.Assert(false, "Invalid operation, this method is used to retrive the first symbol in an identifier list. It only accepts either an identlist or an ident");
            }
        }

        private void ResolveFunctionGroups()
        {
            //
            // For every class in the classtable
            //      If it has a baseclass, get its list of function group 'basegrouplist'
            //          For every basegroup in basegrouplist
            //              If this class has this function group, append the visible feps from the basegoup
            //                  If this class doesnt have basegroup, create a new group and append the visible feps from the basegoup
            //              End
            //          End
            //      End
            // End                        
            //


            // foreach class in classtable
            foreach (ProtoCore.DSASM.ClassNode cnode in core.ClassTable.ClassNodes)
            {
                if (cnode.baseList.Count > 0)
                {
                    // Get the current class functiongroup
                    int ci = cnode.classId;
                    Dictionary<string, FunctionGroup> groupList = new Dictionary<string, FunctionGroup>();
                    if (!core.FunctionTable.GlobalFuncTable.TryGetValue(ci + 1, out groupList))
                    {
                        continue;
                    }

                    // If it has a baseclass, get its function group 'basegroup'
                    int baseCI = cnode.baseList[0];
                    Dictionary<string, FunctionGroup> baseGroupList = new Dictionary<string, FunctionGroup>();
                    bool groupListExists = core.FunctionTable.GlobalFuncTable.TryGetValue(baseCI + 1, out baseGroupList);
                    if (groupListExists)
                    {
                        // If it has a baseclass, get its list of function group 'basegrouplist'
                        // for every group, check if this already exisits in the current class
                        foreach (KeyValuePair<string, FunctionGroup> baseGroup in baseGroupList)
                        {
                           if (groupList.ContainsKey(baseGroup.Key))
                           {
                               // If this class has this function group, append the visible feps from the basegoup
                               FunctionGroup currentGroup = new FunctionGroup();
                               if (groupList.TryGetValue(baseGroup.Key, out currentGroup))
                               {
                                   // currentGroup is this class's current function group given the current name
                                   currentGroup.CopyVisible(baseGroup.Value.FunctionEndPoints);
                               }
                           }
                           else
                           {
                               // If this class doesnt have basegroup, create a new group and append the visible feps from the basegoup
                               FunctionGroup newGroup = new FunctionGroup();
                               newGroup.CopyVisible(baseGroup.Value.FunctionEndPoints);
                               if (newGroup.FunctionEndPoints.Count > 0)
                               {
                                   groupList.Add(baseGroup.Key, newGroup);
                               }
                           }
                        }
                    }
                }
            }
        }
        
        private void TraverseAndAppendThisPtrArg(AssociativeNode node, ref AssociativeNode newIdentList)
        {
            if (node is BinaryExpressionNode)
            {
                TraverseAndAppendThisPtrArg((node as BinaryExpressionNode).LeftNode, ref newIdentList);
                TraverseAndAppendThisPtrArg((node as BinaryExpressionNode).RightNode, ref newIdentList);
            }
            else if (node is IdentifierListNode)
            {
                // Travere until the left most identifier
                IdentifierListNode binaryNode = node as IdentifierListNode;
                while ((binaryNode as IdentifierListNode).LeftNode is IdentifierListNode)
                {
                    binaryNode = (binaryNode as IdentifierListNode).LeftNode as IdentifierListNode;
                }
                
                IdentifierNode identNode = null;
                if ((binaryNode as IdentifierListNode).LeftNode is IdentifierNode)
                {
                    identNode = (binaryNode as IdentifierListNode).LeftNode as IdentifierNode;
                }

                Validity.Assert(identNode is IdentifierNode);

                string leftMostSymbolName = identNode.Name;

                // Is it a this pointer?
                if (leftMostSymbolName.Equals(ProtoCore.DSDefinitions.Keyword.This))
                {
                    // Then just replace it
                    identNode.Name = identNode.Value = ProtoCore.DSASM.Constants.kThisPointerArgName;

                    newIdentList = node;
                    return;
                }



                // Is the left most symbol a member?
                SymbolNode symbolnode;
                bool isAccessible = false;
                bool isAllocated = VerifyAllocation(leftMostSymbolName, globalClassIndex, ProtoCore.DSASM.Constants.kGlobalScope, out symbolnode, out isAccessible);
                if (!isAllocated)
                {
                    newIdentList = node;
                    return;
                }

                IdentifierListNode identList = new IdentifierListNode();

                IdentifierNode lhsThisArg = new IdentifierNode();
                lhsThisArg.Name = ProtoCore.DSASM.Constants.kThisPointerArgName;
                lhsThisArg.Value = ProtoCore.DSASM.Constants.kThisPointerArgName;

                identList.LeftNode = lhsThisArg;
                identList.Optr = Operator.dot;
                identList.RightNode = identNode;

                // Update the last binary dot node with the new dot node
                binaryNode.LeftNode = identList;

                newIdentList = binaryNode;
            }

            else if (node is FunctionCallNode)
            {
                FunctionCallNode fCall = node as FunctionCallNode;

                for(int n = 0; n < fCall.FormalArguments.Count; ++n)
                {
                    AssociativeNode argNode = fCall.FormalArguments[n];
                    TraverseAndAppendThisPtrArg(argNode, ref argNode);
                    fCall.FormalArguments[n] = argNode;
                }
                newIdentList = fCall;
            }
            else if (node is FunctionDotCallNode)
            {
                FunctionDotCallNode dotCall = node as FunctionDotCallNode;
                string name = dotCall.DotCall.FormalArguments[0].Name;

                // TODO Jun: If its a constructor, leave it as it is. 
                // After the first version of global instance functioni s implemented, 2nd phase would be to remove dotarg methods completely
                bool isConstructorCall = false;
                if (null != name)
                {
                    isConstructorCall = ProtoCore.DSASM.Constants.kInvalidIndex != core.ClassTable.IndexOf(name);
                }

                FunctionCallNode fCall = (node as FunctionDotCallNode).FunctionCall;


                if (isConstructorCall)
                {
                    newIdentList = node;
                }
                else
                {
                    for (int n = 0; n < fCall.FormalArguments.Count; ++n)
                    {
                        AssociativeNode argNode = fCall.FormalArguments[n];
                        TraverseAndAppendThisPtrArg(argNode, ref argNode);
                        fCall.FormalArguments[n] = argNode;
                    }
                    newIdentList = node;
                }
            }
            else if (node is IdentifierNode)
            {
                string identName = (node as IdentifierNode).Name;
                if (identName.Equals(ProtoCore.DSDefinitions.Keyword.Return))
                {
                    newIdentList = node;
                    return;
                }

                if (ProtoCore.DSASM.Constants.kInvalidIndex == globalClassIndex)
                {
                    newIdentList = node;
                    return;
                }

                // Temp are not member vars
                if (ProtoCore.Utils.CoreUtils.IsAutoGeneratedVar(identName))
                {
                    newIdentList = node;
                    return;
                }

                // Is it a member
                SymbolNode symbolnode;
                bool isAccessible = false;

                bool isAllocated = VerifyAllocation(identName, globalClassIndex, ProtoCore.DSASM.Constants.kGlobalScope, out symbolnode, out isAccessible);
                if (!isAllocated)
                {
                    newIdentList = node;
                    return;
                }

                IdentifierListNode identList = new IdentifierListNode();

                IdentifierNode lhsThisArg = new IdentifierNode();
                lhsThisArg.Name = ProtoCore.DSASM.Constants.kThisPointerArgName;
                lhsThisArg.Value = ProtoCore.DSASM.Constants.kThisPointerArgName;

                identList.LeftNode = lhsThisArg;
                identList.Optr = Operator.dot;
                identList.RightNode = node;

                newIdentList = identList;
            }
            else
            {
                newIdentList = node;
            }
        }

        // TODO Jun: move this function in a utils file
        private void TraverseAndAppendThisPtrArg(ProtoCore.AST.ImperativeAST.ImperativeNode node, ref ProtoCore.AST.ImperativeAST.ImperativeNode newIdentList)
        {
            if (node is ProtoCore.AST.ImperativeAST.BinaryExpressionNode)
            {
                TraverseAndAppendThisPtrArg((node as ProtoCore.AST.ImperativeAST.BinaryExpressionNode).LeftNode, ref newIdentList);
                TraverseAndAppendThisPtrArg((node as ProtoCore.AST.ImperativeAST.BinaryExpressionNode).RightNode, ref newIdentList);
            }
            else if (node is ProtoCore.AST.ImperativeAST.IdentifierListNode)
            {
                // Travere until the left most identifier
                ProtoCore.AST.ImperativeAST.IdentifierListNode binaryNode = node as ProtoCore.AST.ImperativeAST.IdentifierListNode;
                while ((binaryNode as ProtoCore.AST.ImperativeAST.IdentifierListNode).LeftNode is ProtoCore.AST.ImperativeAST.IdentifierListNode)
                {
                    binaryNode = (binaryNode as ProtoCore.AST.ImperativeAST.IdentifierListNode).LeftNode as ProtoCore.AST.ImperativeAST.IdentifierListNode;
                }

                ProtoCore.AST.ImperativeAST.IdentifierNode identNode = null;
                if ((binaryNode as ProtoCore.AST.ImperativeAST.IdentifierListNode).LeftNode is ProtoCore.AST.ImperativeAST.IdentifierNode)
                {
                    identNode = (binaryNode as ProtoCore.AST.ImperativeAST.IdentifierListNode).LeftNode as ProtoCore.AST.ImperativeAST.IdentifierNode;
                }

                Validity.Assert(identNode is ProtoCore.AST.ImperativeAST.IdentifierNode);

                string leftMostSymbolName = identNode.Name;

                // Is it a this pointer?
                if (leftMostSymbolName.Equals(ProtoCore.DSDefinitions.Keyword.This))
                {
                    // Then just replace it
                    identNode.Name = identNode.Value = ProtoCore.DSASM.Constants.kThisPointerArgName;

                    newIdentList = node;
                    return;
                }



                // Is the left most symbol a member?
                SymbolNode symbolnode;
                bool isAccessible = false;
                bool isAllocated = VerifyAllocation(leftMostSymbolName, globalClassIndex, ProtoCore.DSASM.Constants.kGlobalScope, out symbolnode, out isAccessible);
                if (!isAllocated)
                {
                    newIdentList = node;
                    return;
                }

                ProtoCore.AST.ImperativeAST.IdentifierListNode identList = new ProtoCore.AST.ImperativeAST.IdentifierListNode();

                ProtoCore.AST.ImperativeAST.IdentifierNode lhsThisArg = new ProtoCore.AST.ImperativeAST.IdentifierNode();
                lhsThisArg.Name = ProtoCore.DSASM.Constants.kThisPointerArgName;
                lhsThisArg.Value = ProtoCore.DSASM.Constants.kThisPointerArgName;

                identList.LeftNode = lhsThisArg;
                identList.Optr = Operator.dot;
                identList.RightNode = identNode;

                // Update the last binary dot node with the new dot node
                binaryNode.LeftNode = identList;

                newIdentList = binaryNode;
            }

            else if (node is ProtoCore.AST.ImperativeAST.FunctionCallNode)
            {
                ProtoCore.AST.ImperativeAST.FunctionCallNode fCall = node as ProtoCore.AST.ImperativeAST.FunctionCallNode;

                for (int n = 0; n < fCall.FormalArguments.Count; ++n)
                {
                    ProtoCore.AST.ImperativeAST.ImperativeNode argNode = fCall.FormalArguments[n];
                    TraverseAndAppendThisPtrArg(argNode, ref argNode);
                    fCall.FormalArguments[n] = argNode;
                }
                newIdentList = fCall;
            }
                /*
            else if (node is ProtoCore.AST.ImperativeAST.FunctionDotCallNode)
            {
                ProtoCore.AST.ImperativeAST.FunctionDotCallNode dotCall = node as ProtoCore.AST.ImperativeAST.FunctionDotCallNode;
                string name = dotCall.DotCall.FormalArguments[0].Name;

                // TODO Jun: If its a constructor, leave it as it is. 
                // After the first version of global instance functioni s implemented, 2nd phase would be to remove dotarg methods completely
                bool isConstructorCall = false;
                if (null != name)
                {
                    isConstructorCall = ProtoCore.DSASM.Constants.kInvalidIndex != core.ClassTable.IndexOf(name);
                }

                ProtoCore.AST.ImperativeAST.FunctionCallNode fCall = (node as ProtoCore.AST.ImperativeAST.FunctionDotCallNode).FunctionCall;


                if (isConstructorCall)
                {
                    newIdentList = node;
                }
                else
                {
                    for (int n = 0; n < fCall.FormalArguments.Count; ++n)
                    {
                        ProtoCore.AST.ImperativeAST.ImperativeNode argNode = fCall.FormalArguments[n];
                        TraverseAndAppendThisPtrArg(argNode, ref argNode);
                        fCall.FormalArguments[n] = argNode;
                    }
                    newIdentList = fCall;
                }
            }
            */
            else if (node is ProtoCore.AST.ImperativeAST.IdentifierNode)
            {
                string identName = (node as ProtoCore.AST.ImperativeAST.IdentifierNode).Name;
                if (identName.Equals(ProtoCore.DSDefinitions.Keyword.Return))
                {
                    newIdentList = node;
                    return;
                }

                if (ProtoCore.DSASM.Constants.kInvalidIndex == globalClassIndex)
                {
                    newIdentList = node;
                    return;
                }

                // Temp are not member vars
                if (ProtoCore.Utils.CoreUtils.IsAutoGeneratedVar(identName))
                {
                    newIdentList = node;
                    return;
                }

                // Is it a member
                SymbolNode symbolnode;
                bool isAccessible = false;

                bool isAllocated = VerifyAllocation(identName, globalClassIndex, ProtoCore.DSASM.Constants.kGlobalScope, out symbolnode, out isAccessible);
                if (!isAllocated)
                {
                    newIdentList = node;
                    return;
                }

                ProtoCore.AST.ImperativeAST.IdentifierListNode identList = new ProtoCore.AST.ImperativeAST.IdentifierListNode();

                ProtoCore.AST.ImperativeAST.IdentifierNode lhsThisArg = new ProtoCore.AST.ImperativeAST.IdentifierNode();
                lhsThisArg.Name = ProtoCore.DSASM.Constants.kThisPointerArgName;
                lhsThisArg.Value = ProtoCore.DSASM.Constants.kThisPointerArgName;

                identList.LeftNode = lhsThisArg;
                identList.Optr = Operator.dot;
                identList.RightNode = node;

                newIdentList = identList;
            }
            else
            {
                newIdentList = node;
            }
        }

        
        private void BuildThisFunctionBody(ThisPointerProcOverload procOverload)
        {
            BinaryExpressionNode thisFunctionBody = new BinaryExpressionNode();
            IdentifierNode leftNode = new IdentifierNode();
            leftNode.Name = leftNode.Value = ProtoCore.DSDefinitions.Keyword.Return;
            thisFunctionBody.LeftNode = leftNode;

            thisFunctionBody.Optr = Operator.assign;


            // Build the function call and pass it the arguments including the this pointer
            FunctionCallNode fcall = new FunctionCallNode();
            IdentifierNode identNode = new IdentifierNode();
            identNode.Name = procOverload.procNode.Name;
            fcall.Function = identNode;

            // Set the arguments passed into the function excluding the 'this' argument
            List<AssociativeNode> args = new List<AssociativeNode>();
            for (int n = 1; n < procOverload.procNode.Signature.Arguments.Count; ++n)
            {
                VarDeclNode varDecl = procOverload.procNode.Signature.Arguments[n];
                args.Add(varDecl.NameNode);
            }
            fcall.FormalArguments = args;


            // Build the dotcall node
            procOverload.procNode.FunctionBody.Body = new List<AssociativeNode>();
            procOverload.procNode.FunctionBody.Body.Add(thisFunctionBody);

            thisFunctionBody.RightNode = CoreUtils.GenerateCallDotNode(procOverload.procNode.Signature.Arguments[0].NameNode, fcall, core);
        }
        

        private void InsertThisPointerAtBody(ThisPointerProcOverload procOverload)
        {
            for (int n = 0; n < procOverload.procNode.FunctionBody.Body.Count; ++n)
            {
                if (procOverload.procNode.FunctionBody.Body[n] is BinaryExpressionNode)
                {
                    BinaryExpressionNode assocNode = procOverload.procNode.FunctionBody.Body[n] as BinaryExpressionNode;
                    AssociativeNode outLNode = null;
                    AssociativeNode outRNode = null;

                    // This is the first traversal of this statement and is therefore the assignment op
                    Validity.Assert(Operator.assign == assocNode.Optr);
                    TraverseAndAppendThisPtrArg(assocNode.LeftNode, ref outLNode);
                    TraverseAndAppendThisPtrArg(assocNode.RightNode, ref outRNode);

                    assocNode.LeftNode = outLNode;
                    assocNode.RightNode = outRNode;
                }
                else if (procOverload.procNode.FunctionBody.Body[n] is LanguageBlockNode)
                {
                    LanguageBlockNode langBlockNode = procOverload.procNode.FunctionBody.Body[n] as LanguageBlockNode;
                    if (langBlockNode.CodeBlockNode is ProtoCore.AST.ImperativeAST.CodeBlockNode)
                    {
                        ProtoCore.AST.ImperativeAST.CodeBlockNode iCodeBlockNode = langBlockNode.CodeBlockNode as ProtoCore.AST.ImperativeAST.CodeBlockNode;
                        for (int i = 0; i < iCodeBlockNode.Body.Count; ++i)
                        {
                            if (iCodeBlockNode.Body[i] is ProtoCore.AST.ImperativeAST.BinaryExpressionNode)
                            {
                                ProtoCore.AST.ImperativeAST.BinaryExpressionNode iNode = iCodeBlockNode.Body[i] as ProtoCore.AST.ImperativeAST.BinaryExpressionNode;
                                ProtoCore.AST.ImperativeAST.ImperativeNode outLNode = null;
                                ProtoCore.AST.ImperativeAST.ImperativeNode outRNode = null;

                                // This is the first traversal of this statement and is therefore the assignment op
                                Validity.Assert(Operator.assign == iNode.Optr);
                                TraverseAndAppendThisPtrArg(iNode.LeftNode, ref outLNode);
                                TraverseAndAppendThisPtrArg(iNode.RightNode, ref outRNode);

                                iNode.LeftNode = outLNode;
                                iNode.RightNode = outRNode;
                            }
                        }
                    }
                    else
                    {
                        Validity.Assert(false, "We dont have other langauges besides associative and imperative");
                    }
                }
            }
        }

        /* 
            1 Copy user-defined function definitions (excluding constructors) into a temp

            2 Modify its signature to include an additional this pointer as the first argument. 
              The 'this' argument should take the type of the current class being traversed and be the first argument in the function.
              The function name must be name mangled in order to stay unique

            3 Append this temp to the current vtable
        */

        private void InsertThisPointerArg(ThisPointerProcOverload procOverload)
        {
            // Modify its signature to include an additional this pointer as the first argument. 
            // The 'this' argument should take the type of the current class being traversed and be the first argument in the function.
            // The function name must be name mangled in order to stay unique

            ProtoCore.AST.AssociativeAST.FunctionDefinitionNode procNode = procOverload.procNode;

            string className = core.ClassTable.ClassNodes[procOverload.classIndex].name;
            string thisPtrArgName = ProtoCore.DSASM.Constants.kThisPointerArgName;

            ProtoCore.AST.AssociativeAST.IdentifierNode ident = new ProtoCore.AST.AssociativeAST.IdentifierNode();
            ident.Name = ident.Value = thisPtrArgName;

            VarDeclNode thisPtrArg = new ProtoCore.AST.AssociativeAST.VarDeclNode()
            {
                memregion = ProtoCore.DSASM.MemoryRegion.kMemStack,
                access = ProtoCore.DSASM.AccessSpecifier.kPublic,
                NameNode = ident,
                ArgumentType = new ProtoCore.Type { Name = className, UID = procOverload.classIndex, IsIndexable = false, rank = 0 }
            };

            procNode.Signature.Arguments.Insert(0, thisPtrArg);


            ProtoCore.Type type = new ProtoCore.Type();
        }

        
        //
        //  proc ResolveFinalNodeRefs()
        //      foreach graphnode in graphnodeList
        //          def firstproc = graphnode.firstproc
        //          // Auto-generate the updateNodeRefs for this graphnode given the 
        //          // list stored in the first procedure found in the assignment expression
        //          foreach noderef in firstProc.updatedProperties	
        //              def n = graphnode.firstProcRefIndex
	    //              def autogenRef = updateNodeRef[n]
        //              autogenRef.append(noderef)
	    //              graphnode.pushUpdateRef(autogenRef)
        //          end
        //      end
        //  end
        //
        private void ResolveFinalNodeRefs()
        {
            foreach (ProtoCore.AssociativeGraph.GraphNode graphNode in codeBlock.instrStream.dependencyGraph.GraphList)
            {
                ProtoCore.DSASM.ProcedureNode firstProc = graphNode.firstProc;
                if (null == firstProc || firstProc.isAutoGenerated)
                {
                    continue;
                }

                // TODO: The following implementation is wrong.
                // Suppose for function call: x = foo().bar(); which converted
                // to x = %dot(foo(), bar(), ...); the following checking skips
                // it because %dot() is an internal function. -Yu Ke

                // Do this only for non auto-generated function calls 
                //if any local var is depend on global var
                if (core.Options.localDependsOnGlobalSet)
                {
                    if (!firstProc.name.ToCharArray()[0].Equals('_') && !firstProc.name.ToCharArray()[0].Equals('%'))
                    {
                        //for each node
                        foreach (ProtoCore.AssociativeGraph.GraphNode gNode in codeBlock.instrStream.dependencyGraph.GraphList)
                        {
                            if (gNode.updateNodeRefList != null && gNode.updateNodeRefList.Count != 0)
                            {
                                if (gNode.procIndex == firstProc.procId && !gNode.updateNodeRefList[0].nodeList[0].symbol.name.ToCharArray()[0].Equals('%'))
                                {
                                    foreach (ProtoCore.AssociativeGraph.GraphNode dNode in gNode.dependentList)
                                    {
                                        if (dNode.procIndex == ProtoCore.DSASM.Constants.kGlobalScope)
                                        {
                                            if (!dNode.updateNodeRefList[0].nodeList[0].symbol.name.ToCharArray()[0].Equals('%'))
                                            {
                                                graphNode.PushDependent(dNode);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }


                // For each property modified
                foreach (ProtoCore.AssociativeGraph.UpdateNodeRef updateRef in firstProc.updatedProperties)
                {
                    int index = graphNode.firstProcRefIndex;


                    // Is it a global function
                    if (ProtoCore.DSASM.Constants.kGlobalScope == firstProc.classScope)
                    {
                        graphNode.updateNodeRefList.AddRange(firstProc.updatedGlobals);
                    }
                    else if (ProtoCore.DSASM.Constants.kInvalidIndex != index && ProtoCore.DSASM.Constants.kGlobalScope != firstProc.classScope)
                    {
                        if (core.Options.FullSSA)
                        {
                            foreach (ProtoCore.AssociativeGraph.GraphNode dependent in graphNode.dependentList)
                            {
                                // Do this only if first proc is a member function...
                                ProtoCore.AssociativeGraph.UpdateNodeRef autogenRef = new ProtoCore.AssociativeGraph.UpdateNodeRef(dependent.updateNodeRefList[0]);
                                autogenRef = autogenRef.GetUntilFirstProc();

                                // ... and the first symbol is an instance of a user-defined type
                                int last = autogenRef.nodeList.Count - 1;
                                Validity.Assert(autogenRef.nodeList[last].nodeType != ProtoCore.AssociativeGraph.UpdateNodeType.kMethod && null != autogenRef.nodeList[last].symbol);
                                if (autogenRef.nodeList[last].symbol.datatype.UID >= (int)PrimitiveType.kMaxPrimitives)
                                {
                                    autogenRef.PushUpdateNodeRef(updateRef);
                                    graphNode.updateNodeRefList.Add(autogenRef);
                                }
                            }
                        }
                        else
                        {
                            // Do this only if first proc is a member function...
                            ProtoCore.AssociativeGraph.UpdateNodeRef autogenRef = new ProtoCore.AssociativeGraph.UpdateNodeRef(graphNode.dependentList[0].updateNodeRefList[0]);
                            autogenRef = autogenRef.GetUntilFirstProc();

                            // ... and the first symbol is an instance of a user-defined type
                            int last = autogenRef.nodeList.Count - 1;
                            Validity.Assert(autogenRef.nodeList[last].nodeType != ProtoCore.AssociativeGraph.UpdateNodeType.kMethod && null != autogenRef.nodeList[last].symbol);
                            if (autogenRef.nodeList[last].symbol.datatype.UID >= (int)PrimitiveType.kMaxPrimitives)
                            {
                                autogenRef.PushUpdateNodeRef(updateRef);
                                graphNode.updateNodeRefList.Add(autogenRef);
                            }
                        }
                    }
                }

                if (graphNode.updatedArguments.Count > 0)
                {
                    // For each argument modified
                    int n = 0;

                    // Create the current modified argument
                    foreach (KeyValuePair<string, List<ProtoCore.AssociativeGraph.UpdateNodeRef>> argNameModifiedStatementsPair in firstProc.updatedArgumentProperties)
                    {
                        // For every single arguments' modified statements
                        foreach (ProtoCore.AssociativeGraph.UpdateNodeRef nodeRef in argNameModifiedStatementsPair.Value)
                        {
                            if (core.Options.FullSSA)
                            {
                                //
                                // We just trigger update from whichever statement is dependent on the first pointer associatied with this SSA stmt
                                // Given:
                                //      p = C.C();
                                //      a = p.x;
                                //      i = f(p);
                                //
                                //      %t0 = C.C()
                                //      p = %t0
                                //      %t1 = p
                                //      %t2 = %t1.x
                                //      a = %t2
                                //      %t3 = p
                                //      %t4 = f(%t3)    -> Assume that function 'f' modifies the property 'x' of its argument
                                //                      -> The graph node of this stmt has 2 updatenoderefs 
                                //                      ->  there are %t4 and p ('p' because it is the first pointer of %t3
                                //      i = %t4
                                //
                                
                                // Get the modified property name 
                                string argname = graphNode.updatedArguments[n].nodeList[0].symbol.name;
                                if (ProtoCore.Utils.CoreUtils.IsSSATemp(argname) && ssaTempToFirstPointerMap.ContainsKey(argname))
                                {
                                    // The property is an SSA temp, Get the SSA first pointer associated with this temp
                                    argname = ssaTempToFirstPointerMap[argname];
                                }

                                bool isAccessible = false;
                                SymbolNode symbol = null;
                                bool isAllocated = VerifyAllocation(argname, globalClassIndex, globalProcIndex, out symbol, out isAccessible);
                                if (isAllocated)
                                {
                                    ProtoCore.AssociativeGraph.UpdateNode updateNode = new UpdateNode();
                                    updateNode.symbol = symbol;
                                    updateNode.nodeType = ProtoCore.AssociativeGraph.UpdateNodeType.kSymbol;

                                    ProtoCore.AssociativeGraph.UpdateNodeRef argNodeRef = new ProtoCore.AssociativeGraph.UpdateNodeRef();
                                    argNodeRef.PushUpdateNode(updateNode);
                                    graphNode.updateNodeRefList.Add(argNodeRef);
                                }
                                
                            }
                            else
                            {
                                ProtoCore.AssociativeGraph.UpdateNodeRef argNodeRef = new ProtoCore.AssociativeGraph.UpdateNodeRef();
                                argNodeRef.PushUpdateNodeRef(graphNode.updatedArguments[n]);
                                argNodeRef.PushUpdateNodeRef(nodeRef);
                                graphNode.updateNodeRefList.Add(argNodeRef);
                            }
                        }
                        ++n;
                    }
                }
            }
        }

        
        // 
        //  proc TraverseFunctionDef(node)
        //      ...
        //      def argList
        //      foreach arg in node.argdefinition
        //          ; Store not just the argument types, but also the argument identifier
        //          def argtype = buildtype(arg)
        //          argtype.name = arg.identname
        //          argList.push(argtype)
        //      end
        //      ...
        //  end
        //  

        //
        //  proc AutoGenerateUpdateArgumentReference(node)
        //      def proplist = dfsGetSymbolList(node)
        //      if lhs[0] is an argument
        //          proplist = getExceptFirst(lhs)
        //      end
        //      fnode.updatedArgProps.push(proplist)
        //  end
        //
        private ProtoCore.AssociativeGraph.UpdateNodeRef AutoGenerateUpdateArgumentReference(AssociativeNode node, ProtoCore.AssociativeGraph.GraphNode graphNode)
        {
            // Get the lhs symbol list
            ProtoCore.Type type = new ProtoCore.Type();
            type.UID = globalClassIndex;
            ProtoCore.AssociativeGraph.UpdateNodeRef leftNodeRef = new ProtoCore.AssociativeGraph.UpdateNodeRef();
            DFSGetSymbolList(node, ref type, leftNodeRef);

            ProtoCore.DSASM.SymbolNode firstSymbol = null;


            // Check if we are inside a procedure
            if (null != localProcedure)
            {
                // Check if there are at least 2 symbols in the list
                if (leftNodeRef.nodeList.Count >= 2)
                {
                    firstSymbol = leftNodeRef.nodeList[0].symbol;
                    if (null != firstSymbol && leftNodeRef.nodeList[0].nodeType != ProtoCore.AssociativeGraph.UpdateNodeType.kMethod)
                    {
                        // Now check if the first element of the identifier list is an argument
                        foreach (ProtoCore.DSASM.ArgumentInfo argInfo in localProcedure.argInfoList)
                        {
                            if (argInfo.Name == firstSymbol.name)
                            {
                                leftNodeRef.nodeList.RemoveAt(0);

                                List<ProtoCore.AssociativeGraph.UpdateNodeRef> refList = null;
                                bool found = localProcedure.updatedArgumentProperties.TryGetValue(argInfo.Name, out refList);
                                if (found)
                                {
                                    refList.Add(leftNodeRef);
                                }
                                else
                                {
                                    refList = new List<ProtoCore.AssociativeGraph.UpdateNodeRef>();
                                    refList.Add(leftNodeRef);
                                    localProcedure.updatedArgumentProperties.Add(argInfo.Name, refList);
                                }
                            }
                        }
                    }
                }
            }
            return leftNodeRef;
        }

        private ProtoCore.AssociativeGraph.UpdateNodeRef AutoGenerateUpdateArgumentArrayReference(AssociativeNode node, ProtoCore.AssociativeGraph.GraphNode graphNode)
        {
            // Get the lhs symbol list
            ProtoCore.Type type = new ProtoCore.Type();
            type.UID = globalClassIndex;
            ProtoCore.AssociativeGraph.UpdateNodeRef leftNodeRef = new ProtoCore.AssociativeGraph.UpdateNodeRef();
            DFSGetSymbolList(node, ref type, leftNodeRef);

            ProtoCore.DSASM.SymbolNode firstSymbol = null;


            // Check if we are inside a procedure
            if (null != localProcedure)
            {
                if (1 == leftNodeRef.nodeList.Count)
                {
                    firstSymbol = leftNodeRef.nodeList[0].symbol;

                    // Check if it is an array modification
                    if (graphNode.dimensionNodeList.Count > 0)
                    {
                        // There is only one, see if its an array modification
                        // Now check if the first element of the identifier list is an argument
                        foreach (ProtoCore.DSASM.ArgumentInfo argInfo in localProcedure.argInfoList)
                        {
                            // See if this modified variable is an argument
                            if (argInfo.Name == firstSymbol.name)
                            {
                                List<ProtoCore.AssociativeGraph.UpdateNode> dimensionList = null;
                                bool found = localProcedure.updatedArgumentArrays.TryGetValue(argInfo.Name, out dimensionList);
                                if (found)
                                {
                                    // Overwrite it
                                    dimensionList = graphNode.dimensionNodeList;
                                }
                                else
                                {
                                    // Create a new modified array entry
                                    localProcedure.updatedArgumentArrays.Add(argInfo.Name, graphNode.dimensionNodeList);
                                }
                            }
                        }
                    }
                }
            }
            return leftNodeRef;
        }

        private ProtoCore.AssociativeGraph.UpdateNodeRef GetUpdatedNodeRef(AssociativeNode node)
        {
            if (null == localProcedure)
            {
                return null;
            }

            // Get the lhs symbol list
            ProtoCore.Type type = new ProtoCore.Type();
            type.UID = globalClassIndex;
            ProtoCore.AssociativeGraph.UpdateNodeRef leftNodeRef = new ProtoCore.AssociativeGraph.UpdateNodeRef();
            DFSGetSymbolList(node, ref type, leftNodeRef);

     
            return leftNodeRef;
        }

        private ProtoCore.AssociativeGraph.UpdateNodeRef AutoGenerateUpdateReference(AssociativeNode node, ProtoCore.AssociativeGraph.GraphNode graphNode)
        {
            // Get the lhs symbol list
            ProtoCore.Type type = new ProtoCore.Type();
            type.UID = globalClassIndex;
            ProtoCore.AssociativeGraph.UpdateNodeRef leftNodeRef = new ProtoCore.AssociativeGraph.UpdateNodeRef();
            DFSGetSymbolList(node, ref type, leftNodeRef);


            // Auto-generate the updateNodeRefs for this graphnode given the list 
            // stored in the first procedure found in the assignment expression
            if (functionCallStack.Count > 0)
            {
                ProtoCore.DSASM.ProcedureNode firstProc = functionCallStack[0];
                if (!firstProc.isAutoGenerated)
                {
                    foreach (ProtoCore.AssociativeGraph.UpdateNodeRef updateRef in firstProc.updatedProperties)
                    {
                        ProtoCore.AssociativeGraph.UpdateNodeRef autogenRef = leftNodeRef;
                        autogenRef.PushUpdateNodeRef(updateRef);
                        graphNode.updateNodeRefList.Add(autogenRef);
                    }
                    graphNode.firstProc = firstProc;
                }
            }
            ProtoCore.DSASM.SymbolNode firstSymbol = null;

            // See if the leftmost symbol(updateNodeRef) of the lhs expression is a property of the current class. 
            // If it is, then push the lhs updateNodeRef to the list of modified properties in the procedure node
            if (null != localProcedure && leftNodeRef.nodeList.Count > 0)
            {
                firstSymbol = leftNodeRef.nodeList[0].symbol;
                if (null != firstSymbol && leftNodeRef.nodeList[0].nodeType != ProtoCore.AssociativeGraph.UpdateNodeType.kMethod)
                {
                    if (firstSymbol.functionIndex == ProtoCore.DSASM.Constants.kGlobalScope)
                    {
                        // Does the symbol belong on the same class or class heirarchy as the function calling it
                        if (firstSymbol.classScope == localProcedure.classScope)
                        {
                            localProcedure.updatedProperties.Push(leftNodeRef);
                        }
                        else
                        {
                            if (localProcedure.classScope > 0)
                            {
                                if (core.ClassTable.ClassNodes[localProcedure.classScope].IsMyBase(firstSymbol.classScope))
                                {
                                    localProcedure.updatedProperties.Push(leftNodeRef);
                                }
                            }
                        }
                    }
                }
            }
            return leftNodeRef;
        }

        private void EmitLHSIdentifierListForBinaryExpr(AssociativeNode bnode, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone, bool isTempExpression = false)
        {
            BinaryExpressionNode binaryExpr = bnode as BinaryExpressionNode;
            if (binaryExpr == null || !(binaryExpr.LeftNode is IdentifierListNode))
            {
                return;
            }

            if (ProtoCore.DSASM.Constants.kInvalidIndex == graphNode.updateBlock.startpc)
            {
                graphNode.updateBlock.startpc = pc;
            }

            // This is a setter, so disable dependents
            graphNode.allowDependents = false;
            IdentifierListNode theLeftNode = binaryExpr.LeftNode as IdentifierListNode;
            bool isThisPtr = null != theLeftNode.LeftNode.Name && theLeftNode.LeftNode.Name.Equals(ProtoCore.DSDefinitions.Keyword.This);
            if (isThisPtr)
            {
                graphNode.allowDependents = true;
            }

            ProtoCore.AssociativeGraph.UpdateNodeRef leftNodeRef = AutoGenerateUpdateReference(binaryExpr.LeftNode, graphNode);
            ProtoCore.AssociativeGraph.UpdateNodeRef leftNodeArgRef = AutoGenerateUpdateArgumentReference(binaryExpr.LeftNode, graphNode);

            ProtoCore.AST.Node lnode = binaryExpr.LeftNode;
            NodeUtils.CopyNodeLocation(lnode, binaryExpr);

            ProtoCore.AST.Node rnode = binaryExpr.RightNode;
            bool isCollapsed = false;
            EmitGetterSetterForIdentList(lnode, ref inferedType, graphNode, subPass, out isCollapsed, rnode);

            graphNode.allowDependents = true;

            // Dependency
            if (!isTempExpression)
            {
                // Dependency graph top level symbol 
                graphNode.updateNodeRefList.Add(leftNodeRef);
                graphNode.updateNodeRefList[0].nodeList[0].dimensionNodeList = graphNode.dimensionNodeList;

                // @keyu: foo.id = 42; will generate same leftNodeRef and leftNodeArgRef
                if (!isThisPtr && !leftNodeRef.IsEqual(leftNodeArgRef))
                {
                    graphNode.updateNodeRefList.Add(leftNodeArgRef);
                }

                //
                // If the lhs of the expression is an identifier list, it could have been modified. 
                // It must then be a dependent of its own graphnode
                //
                //      class C
                //      {
                //          x : int;
                //          constructor C(i:int)
                //          {
                //              x = i;
                //          }
                //      }
                //
                //      i = 10;
                //      a = C.C(i);
                //      a.x = 15; -> re-execute this line ... as 'a' was redefined and its members now changed
                //      val = a.x;
                //      i = 7;
                //

                //
                // If inside a member function, and the lhs is a property, make sure it is not just:
                //      x = y (this.x = y)
                //

                if (core.Options.LHSGraphNodeUpdate)
                {
                    if (!isThisPtr || graphNode.updateNodeRefList[0].nodeList.Count > 1)
                    {
                        ProtoCore.AssociativeGraph.GraphNode dependentNode = new ProtoCore.AssociativeGraph.GraphNode();
                        dependentNode.isLHSNode = true;
                        dependentNode.updateNodeRefList.Add(graphNode.updateNodeRefList[0]);
                        graphNode.dependentList.Add(dependentNode);
                    }
                }


                // Move the pointer to FX
                StackValue opFX = StackValue.BuildRegister(Registers.FX);

                ProtoCore.DSASM.SymbolNode firstSymbol = leftNodeRef.nodeList[0].symbol;
                if (null != firstSymbol)
                {
                    EmitDependency(binaryExpr.exprUID, binaryExpr.modBlkUID, false);
                }

                if (core.Options.FullSSA)
                {
                    if (!graphNode.IsSSANode())
                    {
                        // This is the last expression in the SSA'd expression
                        // Backtrack and assign the this last final assignment graphnode to its associated SSA graphnodes
                        for (int n = codeBlock.instrStream.dependencyGraph.GraphList.Count - 1; n >= 0; --n)
                        {
                            GraphNode currentNode = codeBlock.instrStream.dependencyGraph.GraphList[n];
                            bool isWithinSameScope = currentNode.classIndex == graphNode.classIndex
                                && currentNode.procIndex == graphNode.procIndex;
                            bool isWithinSameExpressionID = currentNode.exprUID == graphNode.exprUID;
                            if (isWithinSameScope && isWithinSameExpressionID)
                            {
                                codeBlock.instrStream.dependencyGraph.GraphList[n].lastGraphNode = graphNode;
                            }
                        }
                    }
                }


                // Assign the end pc to this graph node's update block
                // Dependency graph construction is complete for this expression
                graphNode.updateBlock.endpc = pc - 1;
                codeBlock.instrStream.dependencyGraph.Push(graphNode);
                functionCallStack.Clear();
            }
        }

        private bool EmitLHSThisDotProperyForBinaryExpr(AssociativeNode bnode, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone, bool isTempExpression = false)
        {
            BinaryExpressionNode binaryExpr = bnode as BinaryExpressionNode;
            if (binaryExpr == null || !(binaryExpr.LeftNode is IdentifierNode))
            {
                return false;
            }

            if (globalClassIndex != Constants.kGlobalScope && globalProcIndex != Constants.kGlobalScope)
            {
                ClassNode thisClass = core.ClassTable.ClassNodes[globalClassIndex];
                ProcedureNode procNode = thisClass.vtable.procList[globalProcIndex];

                string identName = (binaryExpr.LeftNode as IdentifierNode).Value;
                if (!procNode.name.Equals(ProtoCore.DSASM.Constants.kSetterPrefix + identName))
                {
                    SymbolNode symbolnode;
                    bool isAccessible = false;
                    bool isAllocated = VerifyAllocation(identName, globalClassIndex, globalProcIndex, out symbolnode, out isAccessible);

                    if (symbolnode != null &&
                        symbolnode.classScope != Constants.kGlobalScope &&
                        symbolnode.functionIndex == Constants.kGlobalScope)
                    {
                        var thisNode = nodeBuilder.BuildIdentfier(ProtoCore.DSDefinitions.Keyword.This);
                        var thisIdentListNode = nodeBuilder.BuildIdentList(thisNode, binaryExpr.LeftNode);
                        var newAssignment = nodeBuilder.BuildBinaryExpression(thisIdentListNode, binaryExpr.RightNode);
                        NodeUtils.CopyNodeLocation(newAssignment, bnode);

                        if (ProtoCore.DSASM.Constants.kInvalidIndex != binaryExpr.exprUID)
                        {
                            (newAssignment as BinaryExpressionNode).exprUID = binaryExpr.exprUID;
                        }
                        EmitBinaryExpressionNode(newAssignment, ref inferedType, isBooleanOp, graphNode, subPass, isTempExpression);
                        return true;
                    }
                }
            }

            return false;
        }


        private void EmitBinaryExpressionNode(AssociativeNode node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone, bool isTempExpression = false)
        {
            BinaryExpressionNode bnode = null;

            if (!IsParsingGlobal() && !IsParsingGlobalFunctionBody() && !IsParsingMemberFunctionBody())
                return;

            bool isBooleanOperation = false;
            bnode = node as BinaryExpressionNode;
            ProtoCore.Type leftType = new ProtoCore.Type();
            leftType.UID = (int)ProtoCore.PrimitiveType.kTypeVar;
            leftType.IsIndexable = false;

            ProtoCore.Type rightType = new ProtoCore.Type();
            rightType.UID = (int)ProtoCore.PrimitiveType.kTypeVar;
            rightType.IsIndexable = false;

            DebugProperties.BreakpointOptions oldOptions = core.DebugProps.breakOptions;
            
            /*
            proc emitbinaryexpression(node)
                if node is assignment
                    if graphnode is not valid
                        graphnode = BuildNewGraphNode()
                    end
                    dfstraverse(node.right, graphnode)
 
                    def lefttype = invalid
                    def updateNodeRef = null 
                    dfsgetsymbollist(node.left, lefttype, updateNodeRef)
	
                    // Get the first procedure call in the rhs
                    // This stack is populated on traversing the entire RHS
                    def firstProc = functionCallStack.first()
 
                    graphnode.pushUpdateRef(updateNodeRef)

                    // Auto-generate the updateNodeRefs for this graphnode given 
                    the list stored in the first procedure found in the assignment expression
                    foreach noderef in firstProc.updatedProperties
                        def autogenRef = updateNodeRef
                        autogenRef.append(noderef)
                        graphnode.pushUpdateRef(autogenRef)
                    end


                    // See if the leftmost symbol(updateNodeRef) of the lhs expression is a property of the current class. 
                    // If it is, then push the lhs updateNodeRef to the list of modified properties in the procedure node
                    def symbol = classtable[ci].verifyalloc(updateNodeRef[0])
                    if symbol is valid
                        def localproc = getlocalproc(ci,fi)
                        localproc.push(updateNodeRef)
                    end		
                    functionCallStack.Clear();
                end
            end
            */

            /*
                Building the graphnode dependencies from the SSA transformed identifier list is illustrated in the following functions:

                ssaPtrList = new List

                proc EmitBinaryExpression(bnode, graphnode)
	                if bnode is assignment
                        graphnode = new graphnode

                        if bnode is an SSA pointer expression
	                        if bnode.rhs is an identifier
		                        // Push the start pointer
		                        ssaPtrList.push(node.rhs)
	                        else if bnode.rhs is an identifierlist
		                        // Push the rhs of the dot operator
		                        ssaPtrList.push(node.rhs.rhs)
	                        else
		                        Assert unhandled
	                        end
                        end

		                emit(bnode.rhs)
                        emit(bnode.lhs)

                        if (bnode is an SSA pointer expression 
                            and bnode is the last expression in the SSA factor/term
	                        ssaPtrList.Clear()
                        end
	                end
                end

            */


            // If this is an assignment statement, setup the top level graph node
            bool isGraphInScope = false;
            if (ProtoCore.DSASM.Operator.assign == bnode.Optr)
            {
                if (null == graphNode)
                {
                    isGraphInScope = true;
                    EmitCompileLog("==============Start Node==============\n");
                    graphNode = new ProtoCore.AssociativeGraph.GraphNode();
                    graphNode.isParent = true;
                    graphNode.exprUID = bnode.exprUID;
                    graphNode.modBlkUID = bnode.modBlkUID;
                    graphNode.procIndex = globalProcIndex;
                    graphNode.classIndex = globalClassIndex;
                    graphNode.languageBlockId = codeBlock.codeBlockId;

                    if (core.Options.FullSSA)
                    {
                        if (bnode.isSSAFirstAssignment)
                        {
                            firstSSAGraphNode = graphNode;
                        }

                        // All associative code is SSA'd and we want to keep track of the original identifier nodes of an identifier list:
                        //      i.e. x.y.z
                        // These identifiers will be used to populate the real graph nodes dependencies
                        if (bnode.isSSAPointerAssignment)
                        {
                            Validity.Assert(null != ssaPointerList);

                            if (bnode.RightNode is IdentifierNode)
                            {
                                ssaPointerList.Add(bnode.RightNode);
                            }
                            else if (bnode.RightNode is IdentifierListNode)
                            {
                                ssaPointerList.Add((bnode.RightNode as IdentifierListNode).RightNode);
                            }
                            else if (bnode.RightNode is FunctionDotCallNode)
                            {
                                FunctionDotCallNode dotcall = bnode.RightNode as FunctionDotCallNode;
                                Validity.Assert(dotcall.FunctionCall.Function is IdentifierNode);

                                if (ProtoCore.Utils.CoreUtils.IsGetterSetter(dotcall.FunctionCall.Function.Name))
                                {
                                    // This function is an internal getter or setter, store the identifier node
                                    ssaPointerList.Add(dotcall.FunctionCall.Function);
                                }
                                else
                                {
                                    // This function is a member function, store the functioncall node
                                    ssaPointerList.Add(dotcall.FunctionCall);
                                }
                            }
                            else if (bnode.RightNode is FunctionCallNode)
                            {
                                FunctionCallNode fcall = bnode.RightNode as FunctionCallNode;
                                Validity.Assert(fcall.Function is IdentifierNode);
                                ssaPointerList.Add(fcall.Function);
                            }
                            else
                            {
                                Validity.Assert(false);
                            }
                        }

                        /*
                           The following functions on codegen will perform the static call backtracking:

                           string staticClass = null
                           bool resolveStatic = false

                           proc EmitBinaryExpr(node)
                               if node.right is identifier
                                   if node.right is a class
                                       staticClass = node.right.name
                                       resolveStatic = true
                                   end
                               end	
                           end

                           proc EmitIdentifierList(node, graphnode)
                               if resolveStatic
                                   node.left = new IdentifierNode(staticClass)	
                               end	
                           end
                        */

                        if (bnode.RightNode is IdentifierNode)
                        {
                            // This is the first ssa statement of the transformed identifier list call
                            // The rhs is either a pointer or a classname
                            string identName = (bnode.RightNode as IdentifierNode).Name;
                            string fullClassName;
                            bool isClassName = core.ClassTable.TryGetFullyQualifiedName(identName, out fullClassName);
                            if (isClassName)
                            {
                                ProtoCore.DSASM.SymbolNode symbolnode = null;
                                bool isAccessible = false;
                                bool isAllocatedVariable = VerifyAllocation(identName, globalClassIndex, globalProcIndex, out symbolnode, out isAccessible);

                                // If the identifier is non-allocated then it is a constructor call
                                if (!isAllocatedVariable)
                                {
                                    ssaPointerList.Clear();
                                    staticClass = identName;
                                    resolveStatic = true;
                                    return;
                                }
                            }
                        }

                        //if (bnode.RightNode is FunctionDotCallNode)
                        //{
                        //    string identName = (bnode.RightNode as FunctionDotCallNode).FunctionCall.Function.Name;
                        //    if (core.ClassTable.DoesExist(identName))
                        //    {
                        //        ssaPointerList.Clear();
                        //        staticClass = identName;
                        //        resolveStatic = true;
                        //    }
                        //}
                    }
                    

                    //
                    // Comment Jun: 
                    //      If the expression ID of the assignment node in the context execDirtyFlag list is false,
                    //      it means that it was already executed. This needs to be marked as not dirty
                    if (core.Options.IsDeltaExecution)
                    {
                        if (context.exprExecutionFlags.ContainsKey(bnode.exprUID))
                        {
                            graphNode.isDirty = context.exprExecutionFlags[bnode.exprUID];
                        }
                    }
                }
                if (bnode.LeftNode is IdentifierListNode)
                {
                    EmitLHSIdentifierListForBinaryExpr(bnode, ref inferedType, isBooleanOp, graphNode, subPass);
                    if (isGraphInScope)
                    {
                        EmitCompileLog("==============End Node==============\n");
                    }
                    return;
                }
                else if (bnode.LeftNode is IdentifierNode)
                {
                    if (bnode.LeftNode.Name.Equals(ProtoCore.DSDefinitions.Keyword.This))
                    {
                        string errorMessage = ProtoCore.BuildData.WarningMessage.kInvalidThis;
                        if (localProcedure != null)
                        {
                            if (localProcedure.isStatic)
                            {
                                errorMessage = ProtoCore.BuildData.WarningMessage.kUsingThisInStaticFunction;
                            }
                            else if (localProcedure.classScope == Constants.kGlobalScope)
                            {
                                errorMessage = ProtoCore.BuildData.WarningMessage.kInvalidThis;
                            }
                            else
                            {
                                errorMessage = ProtoCore.BuildData.WarningMessage.kAssingToThis;
                            }
                        }
                        core.BuildStatus.LogWarning(ProtoCore.BuildData.WarningID.kInvalidThis, errorMessage, core.CurrentDSFileName, bnode.line, bnode.col);

                        if (isGraphInScope)
                        {
                            EmitCompileLog("==============End Node==============\n");
                        }

                        return;
                    }

                    if (EmitLHSThisDotProperyForBinaryExpr(bnode, ref inferedType, isBooleanOp, graphNode, subPass))
                    {
                        if (isGraphInScope)
                        {
                            EmitCompileLog("==============End Node==============\n");
                        }
                        return;
                    }
                }
            }
            else //(ProtoCore.DSASM.Operator.assign != b.Optr)
            {
                // Traversing the left node if this binary expression is not an assignment
                //
                isBooleanOperation = ProtoCore.DSASM.Operator.lt == bnode.Optr
                     || ProtoCore.DSASM.Operator.gt == bnode.Optr
                     || ProtoCore.DSASM.Operator.le == bnode.Optr
                     || ProtoCore.DSASM.Operator.ge == bnode.Optr
                     || ProtoCore.DSASM.Operator.eq == bnode.Optr
                     || ProtoCore.DSASM.Operator.nq == bnode.Optr
                     || ProtoCore.DSASM.Operator.and == bnode.Optr
                     || ProtoCore.DSASM.Operator.or == bnode.Optr;

                DfsTraverse(bnode.LeftNode, ref inferedType, isBooleanOperation, graphNode, subPass);

                if (inferedType.UID == (int)PrimitiveType.kTypeFunctionPointer && subPass != ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier && emitDebugInfo)
                {
                    buildStatus.LogSemanticError("Function pointer is not allowed at binary expression other than assignment!", core.CurrentDSFileName, bnode.LeftNode.line, bnode.LeftNode.col);
                }

                leftType.UID = inferedType.UID;
                leftType.IsIndexable = inferedType.IsIndexable;
            }

            int startpc = ProtoCore.DSASM.Constants.kInvalidIndex;
            // (Ayush) in case of PostFixNode, only traverse the identifier now. Post fix operation will be applied later.
#if ENABLE_INC_DEC_FIX
                if (bnode.RightNode is PostFixNode)
                {
                    DfsTraverse((bnode.RightNode as PostFixNode).Identifier, ref inferedType, isBooleanOperation, graphNode);
                }
                else
                {
#endif
            if ((ProtoCore.DSASM.Operator.assign == bnode.Optr) && (bnode.RightNode is LanguageBlockNode))
            {
                inferedType.UID = (int)ProtoCore.PrimitiveType.kTypeVar;
                inferedType.IsIndexable = false;
            }

            if (null != localProcedure && localProcedure.isConstructor && setConstructorStartPC)
            {
                startpc -= 1;
                setConstructorStartPC = false;
            }

            if (bnode.RightNode == null && bnode.Optr == Operator.assign && bnode.LeftNode is IdentifierNode)
            {
                DebugProperties.BreakpointOptions newOptions = oldOptions;
                newOptions |= DebugProperties.BreakpointOptions.SuppressNullVarDeclarationBreakpoint;
                core.DebugProps.breakOptions = newOptions;

                IdentifierNode t = bnode.LeftNode as IdentifierNode;
                ProtoCore.DSASM.SymbolNode symbolnode = null;
                bool isAccessible = false;
                bool hasAllocated = VerifyAllocation(t.Value, globalClassIndex, globalProcIndex, out symbolnode, out isAccessible);
                if (hasAllocated)
                {
                    bool allowDependent = graphNode.allowDependents;
                    graphNode.allowDependents = false;
                    bnode.RightNode = nodeBuilder.BuildIdentfier(t.Value);
                    graphNode.allowDependents = false;
                }
                else
                {
                    bnode.RightNode = new NullNode();
                }
            }

            // Keep track of current pc, because when travese right node it
            // may generate null assignment ( x = null; if x hasn't been defined
            // yet - Yu Ke
            startpc = pc;

            DfsTraverse(bnode.RightNode, ref inferedType, isBooleanOperation, graphNode, subPass);
#if ENABLE_INC_DEC_FIX
                }
#endif

            rightType.UID = inferedType.UID;
            rightType.IsIndexable = inferedType.IsIndexable;

            BinaryExpressionNode rightNode = bnode.RightNode as BinaryExpressionNode;
            if ((rightNode != null) && (ProtoCore.DSASM.Operator.assign == rightNode.Optr))
            {
                DfsTraverse(rightNode.LeftNode, ref inferedType, false, graphNode);
            }

            if (bnode.Optr != ProtoCore.DSASM.Operator.assign)
            {
                if (subPass == ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
                {
                    return;
                }

                if (inferedType.UID == (int)PrimitiveType.kTypeFunctionPointer && emitDebugInfo)
                {
                    buildStatus.LogSemanticError("Function pointer is not allowed at binary expression other than assignment!", core.CurrentDSFileName, bnode.RightNode.line, bnode.RightNode.col);
                }
                EmitBinaryOperation(leftType, rightType, bnode.Optr);
                isBooleanOp = false;

                //if post fix, now traverse the post fix
#if ENABLE_INC_DEC_FIX
                if (bnode.RightNode is PostFixNode)
                    EmitPostFixNode(bnode.RightNode, ref inferedType);
#endif
                return;
            }

            Validity.Assert(null != graphNode);
            if (!isTempExpression)
            {
                /*if (core.Options.IsDeltaExecution)
                    graphNode.updateBlock.startpc = startpc;
                else*/
                    graphNode.updateBlock.startpc = pc;
            }

            currentBinaryExprUID = bnode.exprUID;

            // These have been integrated into "EmitGetterSetterForIdentList" so 
            // that stepping through class properties can be supported. Setting 
            // these values here will cause issues with statements like this to 
            // be highlighted in its entirety (all the way up to closing bracket 
            // without highlighting the semi-colon).
            // 
            //      x = foo(a, b);
            // 
            // bnode.RightNode.line = bnode.line;
            // bnode.RightNode.col = bnode.col;
            // bnode.RightNode.endLine = bnode.endLine;
            // bnode.RightNode.endCol = bnode.endCol;

            // Traverse the entire RHS expression
            DfsTraverse(bnode.RightNode, ref inferedType, isBooleanOperation, graphNode, ProtoCore.DSASM.AssociativeSubCompilePass.kNone, bnode);
            subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier;

            if (bnode.LeftNode is IdentifierNode)
            {
                // TODO Jun: Cleansify this block where the lhs is being handled.
                // For one, make the return as a return node
                IdentifierNode t = bnode.LeftNode as IdentifierNode;
                ProtoCore.DSASM.SymbolNode symbolnode = null;


                ProtoCore.AssociativeGraph.UpdateNodeRef leftNodeGlobalRef = null; 

                string s = t.Value;
                if (s == ProtoCore.DSDefinitions.Keyword.Return)
                {
                    Validity.Assert(null == symbolnode);
                    symbolnode = new ProtoCore.DSASM.SymbolNode();
                    symbolnode.name = s;
                    symbolnode.isTemp = s.StartsWith("%");
                    symbolnode.functionIndex = globalProcIndex;
                    symbolnode.classScope = globalClassIndex;

                    EmitReturnStatement(node, inferedType);


                    // Comment Jun: The inline conditional holds a graphnode and traversing its body will set isReturn = true
                    // Resolve that here as an inline conditional is obviosuly not a return graphnode
                    if (!graphNode.isInlineConditional)
                    {
                        graphNode.isReturn = true;
                    }
                }
                else
                {
                    leftNodeGlobalRef = GetUpdatedNodeRef(bnode.LeftNode);

                    // right node is statement which wont return any value, so push null to stack
                    if ((bnode.RightNode is IfStatementNode) || (bnode.RightNode is ForLoopNode))
                    {
                        EmitPushNull();
                    }
                    {
                        // check whether the variable name is a function name
                        bool isAccessibleFp;
                        int realType;
                        ProtoCore.DSASM.ProcedureNode procNode = null;
                        if (globalClassIndex != ProtoCore.DSASM.Constants.kGlobalScope)
                        {
                            procNode = core.ClassTable.ClassNodes[globalClassIndex].GetMemberFunction(t.Name, null, globalClassIndex, out isAccessibleFp, out realType);
                        }
                        if (procNode == null)
                        {
                            procNode = core.GetFirstVisibleProcedure(t.Name, null, codeBlock);
                        }
                        if (procNode != null)
                        {
                            if (ProtoCore.DSASM.Constants.kInvalidIndex != procNode.procId && emitDebugInfo)
                            {
                                buildStatus.LogSemanticError("\"" + t.Name + "\"" + " is a function and not allowed as a variable name", core.CurrentDSFileName, t.line, t.col);
                            }
                        }
                    }

                    //int type = (int)ProtoCore.PrimitiveType.kTypeVoid;
                    bool isAccessible = false;
                    bool isAllocated = VerifyAllocation(t.Name, globalClassIndex, globalProcIndex, out symbolnode, out isAccessible);
                    int runtimeIndex = (!isAllocated || !isAccessible) ? codeBlock.symbolTable.RuntimeIndex : symbolnode.runtimeTableIndex;

                    if (isAllocated && !isAccessible)
                    {
                        string message = String.Format(ProtoCore.BuildData.WarningMessage.kPropertyIsInaccessible, t.Name);
                        buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kAccessViolation, message, core.CurrentDSFileName, t.line, t.col);
                    }

                    int dimensions = 0;
                    if (null != t.ArrayDimensions)   
                    {
                        graphNode.isIndexingLHS = true;
                        dimensions = DfsEmitArrayIndexHeap(t.ArrayDimensions, graphNode, bnode);
                    }


                    // Comment Jun: Attempt to get the modified argument arrays in the current method
                    // Comment Jun: As of R1 - arrays are copy constructed and cannot propagate update unless explicitly returned
                    //ProtoCore.AssociativeGraph.UpdateNodeRef leftNodeArgArray = AutoGenerateUpdateArgumentArrayReference(bnode.LeftNode, graphNode);


                    ProtoCore.Type castType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, false);
                    var tident = bnode.LeftNode as TypedIdentifierNode;
                    if (tident != null)
                    {
                        int castUID = tident.datatype.UID;
                        if ((int)PrimitiveType.kInvalidType == castUID)
                        {
                            castUID = core.ClassTable.IndexOf(tident.datatype.Name);
                        }

                        if ((int)PrimitiveType.kInvalidType == castUID)
                        {
                            castType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kInvalidType, false);
                            castType.Name = tident.datatype.Name;
                            castType.rank = tident.datatype.rank;
                            castType.IsIndexable = (castType.rank != 0);
                        }
                        else
                        {
                            castType = core.TypeSystem.BuildTypeObject(castUID, tident.datatype.IsIndexable, tident.datatype.rank);
                        }
                    }

                    if (ProtoCore.DSASM.Constants.kInvalidIndex != globalClassIndex)
                    {
                        // In a class
                        if (ProtoCore.DSASM.Constants.kInvalidIndex == globalProcIndex)
                        {
                            string message = "A binary assignment inside a class must be inside a function (AB5E3EC1)";
                            buildStatus.LogSemanticError(message, core.CurrentDSFileName, bnode.line, bnode.col);
                            throw new BuildHaltException(message);
                        }

                        // TODO Jun: refactor this by having symbol table functions for retrieval of node index
                        int symbol = ProtoCore.DSASM.Constants.kInvalidIndex;
                        bool isMemVar = false;
                        if (symbolnode != null)
                        {
                            if (symbolnode.classScope != ProtoCore.DSASM.Constants.kInvalidIndex 
                                && symbolnode.functionIndex == ProtoCore.DSASM.Constants.kGlobalScope)
                            {
                                isMemVar = true;
                            }
                            symbol = symbolnode.symbolTableIndex;
                        }

                        if (!isMemVar)
                        {
                            // This is local variable
                            // TODO Jun: If this local var exists globally, should it allocate a local copy?
                            if (!isAllocated || !isAccessible)
                            {
                                symbolnode = Allocate(globalClassIndex, globalClassIndex, globalProcIndex, t.Name, inferedType, ProtoCore.DSASM.Constants.kPrimitiveSize,
                                    false, ProtoCore.DSASM.AccessSpecifier.kPublic, ProtoCore.DSASM.MemoryRegion.kMemStack, bnode.line, bnode.col);

                                // Add the symbols during watching process to the watch symbol list.
                                if (core.ExecMode == ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter)
                                {
                                    core.watchSymbolList.Add(symbolnode);
                                }

                                Validity.Assert(symbolnode != null);
                            }
                            else
                            {
                                symbolnode.datatype = inferedType;
                            }

                            if (bnode.LeftNode is TypedIdentifierNode)
                            {
                                symbolnode.SetStaticType(castType);
                            }
                            castType = symbolnode.staticType;
                            EmitPushVarData(runtimeIndex, dimensions, castType.UID, castType.rank);

                            symbol = symbolnode.symbolTableIndex;
                            if (t.Name == ProtoCore.DSASM.Constants.kTempArg)
                            {
                                EmitInstrConsole(ProtoCore.DSASM.kw.pop, t.Name);
                                EmitPopForSymbol(symbolnode);
                            }
                            else
                            {
                                if (core.ExecMode != ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter)
                                {
                                    EmitInstrConsole(ProtoCore.DSASM.kw.pop, t.Name);
                                    EmitPopForSymbol(symbolnode, node.line, node.col, node.endLine, node.endCol);
                                }
                                else
                                {
                                    EmitInstrConsole(ProtoCore.DSASM.kw.popw, t.Name);
                                    EmitPopForSymbolW(symbolnode, node.line, node.col, node.endLine, node.endCol);
                                }
                            }
                        }
                        else
                        {
                            if (bnode.LeftNode is TypedIdentifierNode)
                            {
                                symbolnode.SetStaticType(castType);
                            }
                            castType = symbolnode.staticType;
                            EmitPushVarData(runtimeIndex, dimensions, castType.UID, castType.rank);

                            EmitInstrConsole(ProtoCore.DSASM.kw.popm, t.Name);

                            if (symbolnode.isStatic)
                            {
                                var op = StackValue.BuildStaticMemVarIndex(symbol);
                                EmitPopm(op, node.line, node.col, node.endLine, node.endCol);
                            }
                            else
                            {
                                var op = StackValue.BuildMemVarIndex(symbol);
                                EmitPopm(op, node.line, node.col, node.endLine, node.endCol);
                            }
                        }

                        //if (t.Name[0] != '%')
                        {
                            AutoGenerateUpdateReference(bnode.LeftNode, graphNode);
                        }

                        // Dependency
                        if (!isTempExpression)
                        {
                            // Dependency graph top level symbol 
                            graphNode.PushSymbolReference(symbolnode);
                            EmitDependency(bnode.exprUID, bnode.modBlkUID, bnode.isSSAAssignment);
                            functionCallStack.Clear();
                        }
                    }
                    else
                    {
                        if (!isAllocated)
                        {
                            symbolnode = Allocate(globalClassIndex, globalClassIndex, globalProcIndex, t.Name, inferedType, ProtoCore.DSASM.Constants.kPrimitiveSize,
                                    false, ProtoCore.DSASM.AccessSpecifier.kPublic, ProtoCore.DSASM.MemoryRegion.kMemStack, bnode.line, bnode.col);

                            if (core.ExecMode == ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter)
                            {
                                core.watchSymbolList.Add(symbolnode);
                            }

                            if (dimensions > 0)
                            {
                                symbolnode.datatype.rank = dimensions;
                            }
                        }
                        else if (dimensions == 0)
                        {
                            symbolnode.datatype = inferedType;
                        }

                        //
                        // Jun Comment: 
                        //      Update system uses the following registers:  
                        //      _ex stores prev value of ident 't'  - VM assigned
                        //      _fx stores new value                - VM assigned
                        //

                        if (bnode.LeftNode is TypedIdentifierNode)
                        {
                            symbolnode.SetStaticType(castType);
                        }
                        castType = symbolnode.staticType;
                        EmitPushVarData(runtimeIndex, dimensions, castType.UID, castType.rank);

                        if (core.ExecMode != ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter)
                        {
                            EmitInstrConsole(ProtoCore.DSASM.kw.pop, symbolnode.name);
                            EmitPopForSymbol(symbolnode, node.line, node.col, node.endLine, node.endCol);
                        }
                        else
                        {
                            EmitInstrConsole(ProtoCore.DSASM.kw.popw, symbolnode.name);
                            EmitPopForSymbolW(symbolnode, node.line, node.col, node.endLine, node.endCol);                            
                        }

                        AutoGenerateUpdateReference(bnode.LeftNode, graphNode);

                        // Dependency
                        if (!isTempExpression)
                        {
                            // Dependency graph top level symbol 
                            graphNode.PushSymbolReference(symbolnode);
                            EmitDependency(bnode.exprUID, bnode.modBlkUID, bnode.isSSAAssignment);
                            functionCallStack.Clear();
                        }
                    }
                }

                // Dependency graph top level symbol 
                //graphNode.symbol = symbolnode;

                // Assign the end pc to this graph node's update block
                // Dependency graph construction is complete for this expression
                if (!isTempExpression)
                {
                    if (null != leftNodeGlobalRef)
                    {
                        if (null != localProcedure)
                        {
                            // Track for updated globals only in user defined functions
                            if (!localProcedure.isAssocOperator && !localProcedure.isAutoGenerated)
                            {
                                localProcedure.updatedGlobals.Push(leftNodeGlobalRef);
                            }
                        }
                    }

                    if (core.Options.FullSSA)
                    {
                        if (!graphNode.IsSSANode())
                        {
                            // This is the last expression in the SSA'd expression
                            // Backtrack and assign the this last final assignment graphnode to its associated SSA graphnodes
                            for (int n = codeBlock.instrStream.dependencyGraph.GraphList.Count - 1; n >= 0; --n)
                            {
                                GraphNode currentNode = codeBlock.instrStream.dependencyGraph.GraphList[n];
                                bool isWithinSameScope = currentNode.classIndex == graphNode.classIndex
                                    && currentNode.procIndex == graphNode.procIndex;
                                bool isWithinSameExpressionID = currentNode.exprUID == graphNode.exprUID;
                                if (isWithinSameScope && isWithinSameExpressionID)
                                {
                                    if (null == codeBlock.instrStream.dependencyGraph.GraphList[n].lastGraphNode)
                                    {
                                        codeBlock.instrStream.dependencyGraph.GraphList[n].lastGraphNode = graphNode;
                                    }
                                }
                            }
                        }
                    }

                    graphNode.ResolveLHSArrayIndex();
                    graphNode.updateBlock.endpc = pc - 1;
                    codeBlock.instrStream.dependencyGraph.Push(graphNode);

                    SymbolNode cyclicSymbol1 = null;
                    SymbolNode cyclicSymbol2 = null;
                    if (core.Options.staticCycleCheck)
                    {
                        //UpdateGraphNodeDependency(graphNode);
                        if (!CyclicDependencyTest(graphNode, ref cyclicSymbol1, ref cyclicSymbol2))
                        {
                            Validity.Assert(null != cyclicSymbol1);
                            Validity.Assert(null != cyclicSymbol2);

                            //
                            // Set the first symbol that triggers the cycle to null
                            ProtoCore.AssociativeGraph.GraphNode nullAssignGraphNode1 = new ProtoCore.AssociativeGraph.GraphNode();
                            nullAssignGraphNode1.updateBlock.startpc = pc;

                            EmitPushNull();
                            EmitPushVarData(cyclicSymbol1.runtimeTableIndex, 0);
                            EmitInstrConsole(ProtoCore.DSASM.kw.pop, cyclicSymbol1.name);
                            EmitPopForSymbol(cyclicSymbol1, node.line, node.col, node.endLine, node.endCol);

                            nullAssignGraphNode1.PushSymbolReference(cyclicSymbol1);
                            nullAssignGraphNode1.procIndex = globalProcIndex;
                            nullAssignGraphNode1.classIndex = globalClassIndex;
                            nullAssignGraphNode1.updateBlock.endpc = pc - 1;

                            codeBlock.instrStream.dependencyGraph.Push(nullAssignGraphNode1);
                            EmitDependency(ProtoCore.DSASM.Constants.kInvalidIndex, ProtoCore.DSASM.Constants.kInvalidIndex, false);


                            //
                            // Set the second symbol that triggers the cycle to null
                            ProtoCore.AssociativeGraph.GraphNode nullAssignGraphNode2 = new ProtoCore.AssociativeGraph.GraphNode();
                            nullAssignGraphNode2.updateBlock.startpc = pc;

                            EmitPushNull();
                            EmitPushVarData(cyclicSymbol2.runtimeTableIndex, 0);
                            EmitInstrConsole(ProtoCore.DSASM.kw.pop, cyclicSymbol2.name);
                            EmitPopForSymbol(cyclicSymbol2, node.line, node.col, node.endLine, node.endCol);

                            nullAssignGraphNode2.PushSymbolReference(cyclicSymbol2);
                            nullAssignGraphNode2.procIndex = globalProcIndex;
                            nullAssignGraphNode2.classIndex = globalClassIndex;
                            nullAssignGraphNode2.updateBlock.endpc = pc - 1;

                            codeBlock.instrStream.dependencyGraph.Push(nullAssignGraphNode2);
                            EmitDependency(ProtoCore.DSASM.Constants.kInvalidIndex, ProtoCore.DSASM.Constants.kInvalidIndex, false);
                        }
                    }
                    if (isGraphInScope)
                    {
                        EmitCompileLog("==============End Node==============\n");
                    }
                }

                // Jun Comment: If it just so happens that the inline conditional is in the return statement
                if (graphNode.isInlineConditional)
                {
                    graphNode.isReturn = false;
                    if (0 == graphNode.updateNodeRefList.Count)
                    {
                        graphNode.isReturn = true;
                    }
                }
            }
            else
            {
                string message = "Illegal assignment (90787393)";
                buildStatus.LogSemanticError(message, core.CurrentDSFileName, bnode.line, bnode.col);
                throw new BuildHaltException(message);
            }
            core.DebugProps.breakOptions = oldOptions;

            //if post fix, now traverse the post fix

#if ENABLE_INC_DEC_FIX
                if (bnode.RightNode is PostFixNode)
                    EmitPostFixNode(bnode.RightNode, ref inferedType);
#endif
        }

        private void EmitImportNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            ImportNode importNode = node as ImportNode;
            CodeBlockNode codeBlockNode = importNode.CodeNode;
            string origSourceLocation = core.CurrentDSFileName;

            core.CurrentDSFileName = importNode.ModulePathFileName;
            this.isEmittingImportNode = true;

            // TODO Jun: Merge all the DeltaCompile routines in codegen into one place
            bool firstImportInDeltaExecution = false;
            int startPC = pc;

            if (core.Options.IsDeltaExecution)
            {
                //ModuleName can be full path as well 
                if (core.LoadedDLLs.Contains(importNode.ModulePathFileName))
                {
                    return;
                }

                if (ProtoCore.DSASM.AssociativeCompilePass.kGlobalFuncBody == compilePass)
                {
                    core.LoadedDLLs.Add(importNode.ModulePathFileName);
                    firstImportInDeltaExecution = true;
                }
            }

            if (codeBlockNode != null)
            {
                // Only build SSA for the first time
                // Transform after class name compile pass
                if (ProtoCore.DSASM.AssociativeCompilePass.kClassName > compilePass)
                {
                    codeBlockNode.Body = SplitMulitpleAssignment(codeBlockNode.Body);
                    codeBlockNode.Body = BuildSSA(codeBlockNode.Body, context);
                }

                foreach (AssociativeNode assocNode in codeBlockNode.Body)
                {
                    inferedType = new ProtoCore.Type();
                    inferedType.UID = (int)ProtoCore.PrimitiveType.kTypeVar;
                    inferedType.IsIndexable = false;

                    if (assocNode is LanguageBlockNode)
                    {
                        // Build a binaryn node with a temporary lhs for every stand-alone language block
                        var iNode = nodeBuilder.BuildIdentfier(core.GenerateTempLangageVar());
                        var langBlockNode = nodeBuilder.BuildBinaryExpression(iNode, assocNode);

                        DfsTraverse(langBlockNode, ref inferedType, false, null, ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier);
                    }
                    else
                    {
                        DfsTraverse(assocNode, ref inferedType, false, null, subPass);
                    }
                }
            }

            core.CurrentDSFileName = origSourceLocation;
            this.isEmittingImportNode = false;

            // If in delta execution (a.k.a. LiveRunner) we import an external 
            // library and at the same time this library generates some 
            // instructions, we need to keep those instructions to avoid they
            // are overwritten in the next run.
            if (firstImportInDeltaExecution && pc > startPC)
            {
                core.deltaCompileStartPC = pc;
            }
        }

        private void EmitDotFunctionBodyNode(AssociativeNode node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            emitDebugInfo = false;
            DotFunctionBodyNode dnode = node as DotFunctionBodyNode;
            //left hand side
            int depth = 0;
            ProtoCore.Type leftType = new ProtoCore.Type();
            leftType.UID = (int)PrimitiveType.kInvalidType;
            leftType.IsIndexable = false;
            bool isMethodCallPresent = false;
            bool isFirstIdent = true;
            ProtoCore.DSASM.SymbolNode firstSymbol = null;
            if (subPass != ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
            {
                DfsEmitIdentList(dnode.leftNode, null, globalClassIndex, ref leftType, ref depth, ref inferedType, false, ref isFirstIdent, ref isMethodCallPresent, ref firstSymbol, graphNode, subPass);
            }
            //right hand side
            if (dnode.rightNodeArgNum == null)
            {
                //push dimension expression
                EmitIdentifierNode(dnode.rightNodeDimExprList, ref inferedType, isBooleanOp, graphNode, subPass);
                EmitIdentifierNode(dnode.rightNodeDim, ref inferedType, isBooleanOp, graphNode, subPass);
                EmitIdentifierNode(dnode.rightNode, ref inferedType, isBooleanOp, graphNode, subPass);
                if (subPass == ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
                {
                    return;
                }
                EmitInstrConsole(ProtoCore.DSASM.kw.pushlist, "2", globalClassIndex.ToString());
                EmitPushList(2, globalClassIndex, blockScope, true);
            }
            else
            {
                EmitIdentifierNode(dnode.rightNodeArgList, ref inferedType, isBooleanOp, graphNode, subPass);
                if (subPass == ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
                {
                    return;
                }
                //EmitIdentifierNode(dnode.rightNodeArgNum, ref inferedType, isBooleanOp, graphNode, subPass);
                EmitIdentifierNode(dnode.rightNode, ref inferedType, isBooleanOp, graphNode, subPass);
                EmitInstrConsole(ProtoCore.DSASM.kw.callr, dnode.rightNode.Name + "[dynamic]");
                EmitDynamicCall(ProtoCore.DSASM.Constants.kInvalidIndex, globalClassIndex, depth);

                // The function return value
                EmitInstrConsole(ProtoCore.DSASM.kw.push, ProtoCore.DSASM.kw.regRX);
                StackValue opReturn = StackValue.BuildRegister(Registers.RX); 
                EmitPush(opReturn);
            }
            //assign inferedType to var
            inferedType.UID = (int)PrimitiveType.kTypeVar;
            emitDebugInfo = true;
        }

        protected void EmitExceptionHandlingNode(AssociativeNode node, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
#if ENABLE_EXCEPTION_HANDLING
            if (!IsParsingGlobal() && !IsParsingGlobalFunctionBody() && !IsParsingMemberFunctionBody())
            {
                return;
            }
            
            tryLevel++;

            ExceptionHandlingNode exceptionNode = node as ExceptionHandlingNode;
            if (exceptionNode == null)
                return;

            ExceptionHandler exceptionHandler = new ExceptionHandler();
            exceptionHandler.TryLevel = tryLevel;

            ExceptionRegistration registration = core.ExceptionHandlingManager.Register(codeBlock.codeBlockId, globalProcIndex, globalClassIndex);
            registration.Add(exceptionHandler);

            exceptionHandler.StartPc = pc;
            TryBlockNode tryNode = exceptionNode.tryBlock;
            Validity.Assert(tryNode != null);
            foreach (var subnode in tryNode.body)
	        {
		        ProtoCore.Type inferedType = new ProtoCore.Type();
                inferedType.UID = (int) ProtoCore.PrimitiveType.kTypeVar;
                inferedType.IsIndexable = false;
                DfsTraverse(subnode, ref inferedType, false, graphNode, subPass);
	        }
            exceptionHandler.EndPc = pc;

            // Jmp to code after catch block
            BackpatchTable backpatchTable = new BackpatchTable();
            backpatchTable.Append(pc);
            EmitJmp(ProtoCore.DSASM.Constants.kInvalidIndex);

            foreach (var catchBlock in exceptionNode.catchBlocks)
            {
                CatchHandler catchHandler = new CatchHandler();
                exceptionHandler.AddCatchHandler(catchHandler);

                CatchFilterNode filterNode = catchBlock.catchFilter;
                Validity.Assert(filterNode != null);
                catchHandler.FilterTypeUID = core.TypeSystem.GetType(filterNode.type.Name);
                if (catchHandler.FilterTypeUID == (int)PrimitiveType.kInvalidType)
                {
                    string message = String.Format(ProtoCore.BuildData.WarningMessage.kExceptionTypeUndefined, filterNode.type.Name);
                    buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kTypeUndefined, message, core.CurrentDSFileName, filterNode.line, filterNode.col);
                    catchHandler.FilterTypeUID = (int)PrimitiveType.kTypeVar;
                }

                ProtoCore.Type inferedType = new ProtoCore.Type();
                inferedType.UID = (int) ProtoCore.PrimitiveType.kTypeVar;
                inferedType.IsIndexable = false;
                DfsTraverse(filterNode.var, ref inferedType, false, graphNode, subPass);
                
                catchHandler.Entry = pc;
                foreach (var subnode in catchBlock.body)
                {
                    inferedType.UID = (int) ProtoCore.PrimitiveType.kTypeVar;
                    inferedType.IsIndexable = false;
                    DfsTraverse(subnode, ref inferedType, false, graphNode, subPass);
                }

                // Jmp to code after catch block
                backpatchTable.Append(pc);
                EmitJmp(ProtoCore.DSASM.Constants.kInvalidIndex);
            }

            Backpatch(backpatchTable.backpatchList, pc);

            tryLevel--;
#endif
        }

        protected void EmitThrowNode(AssociativeNode node, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
#if ENABLE_EXCEPTION_HANDLING
            if (!IsParsingGlobal() && !IsParsingGlobalFunctionBody() && !IsParsingMemberFunctionBody())
                return;

            ThrowNode throwNode = node as ThrowNode;
            if (throwNode == null)
            {
                return;
            }

            ProtoCore.Type inferedType = new ProtoCore.Type();
            inferedType.UID = (int)ProtoCore.PrimitiveType.kTypeVar;
            inferedType.IsIndexable = false;
            DfsTraverse(throwNode.expression, ref inferedType, false, graphNode);

            EmitInstrConsole(ProtoCore.DSASM.kw.pop, ProtoCore.DSASM.kw.regLX);
            StackValue opLx = StackValue.BuildRegister(Registers.LX);
            EmitPop(opLx, Constants.kGlobalScope);

            EmitInstrConsole(ProtoCore.DSASM.kw.throwexception);
            EmitThrow();
#endif
        }

        // what we are going to do here is go through the identifier list node,
        // and for each identifier node we convert it to a getter. Say:
        // 
        //     x = a.b[i].c.d[j]
        //
        // will be converted to:
        //
        //     %t1 = a.get_b();
        //     %t2 = %t1[i].get_c();
        //     %t3 = %t2.get_d();
        //     return %t3;
        // 
        private IdentifierNode EmitGettersForRHSIdentList(ProtoCore.AST.Node node, ref ProtoCore.Type inferedType, ProtoCore.AssociativeGraph.GraphNode graphNode)
        {
            IdentifierListNode identList = node as IdentifierListNode;
            if (identList == null)
            {
                return null;
            }

            if (identList.LeftNode is IdentifierListNode)
            {
                ProtoCore.AST.Node leftIdentList = identList.LeftNode;
                IdentifierNode retNode = EmitGettersForRHSIdentList(leftIdentList, ref inferedType, graphNode);
                if (retNode != null)
                {
                    identList.LeftNode = retNode;
                }
            }

            IdentifierNode thisNode = identList.RightNode as IdentifierNode;
            if (thisNode == null)
            {
                return null;
            }

            // a.x; => a.get_x(); 
            string getterName = ProtoCore.DSASM.Constants.kGetterPrefix + thisNode.Name;
            identList.RightNode = nodeBuilder.BuildFunctionCall(getterName, new List<AssociativeNode>());

            // %t = a.get_x();
            //IdentifierNode result = nodeBuilder.BuildTempVariable() as IdentifierNode;
            IdentifierNode result = nodeBuilder.BuildTempPropertyVariable() as IdentifierNode;
            var assignment = nodeBuilder.BuildBinaryExpression(result, identList);
            EmitBinaryExpressionNode(assignment, ref inferedType, false, graphNode, ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier, true);

            result.ArrayDimensions = thisNode.ArrayDimensions;
            return result;
        }

        override protected void EmitGetterSetterForIdentList(
            ProtoCore.AST.Node node, 
            ref ProtoCore.Type inferedType, 
            ProtoCore.AssociativeGraph.GraphNode graphNode, 
            ProtoCore.DSASM.AssociativeSubCompilePass subPass, 
            out bool isCollapsed, 
            ProtoCore.AST.Node setterArgument = null)
        {
            isCollapsed = false;

            IdentifierListNode inode = node as IdentifierListNode;
            if (inode == null)
            {
                return;
            }

            // If the left-most property is not "this", insert a "this" node 
            // so a.b.c will be converted to this.a.b.c. Otherwise we have to 
            // specially deal with the left-most property.
            //
            // Imperative language block doesn't need this preprocessing.
            IdentifierListNode leftMostIdentList = inode;
            while (leftMostIdentList.LeftNode is IdentifierListNode)
            {
                leftMostIdentList = leftMostIdentList.LeftNode as IdentifierListNode;
            }
            if (leftMostIdentList.LeftNode is IdentifierNode)
            {
                IdentifierNode leftMostIdent = leftMostIdentList.LeftNode as IdentifierNode;
                if (!string.Equals(ProtoCore.DSDefinitions.Keyword.This, leftMostIdent.Name) &&
                    IsProperty(leftMostIdent.Name))
                {
                    var thisIdent = nodeBuilder.BuildIdentfier(ProtoCore.DSDefinitions.Keyword.This);
                    var thisIdentList = nodeBuilder.BuildIdentList(thisIdent, leftMostIdent);
                    leftMostIdentList.LeftNode = thisIdentList;
                }
            }

            // If this identifier list appears on the left hand side of an 
            // assignment statement, we need to emit setter for the last propery
            // on this identifier list. Two cases: the last (right-most) property
            // is an array identifier or a normal one. 
            if (setterArgument != null)
            {
                if (setterArgument is LanguageBlockNode)
                {
                    var tmpVar = nodeBuilder.BuildTempVariable();
                    var assignment = nodeBuilder.BuildBinaryExpression(tmpVar, setterArgument as AssociativeNode);
                    EmitBinaryExpressionNode(assignment, ref inferedType, false, graphNode, ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier, true);

                    setterArgument = tmpVar;
                }

                if (inode.LeftNode is IdentifierListNode)
                {
                    inode.LeftNode = EmitGettersForRHSIdentList(inode.LeftNode, ref inferedType, graphNode);
                }

                if (inode.RightNode is IdentifierNode)
                {
                    IdentifierNode rnode = inode.RightNode as IdentifierNode;
                    if (rnode.ArrayDimensions == null)
                    {
                        // %t1.x = v; => %t2 = %t1.set_x(v); 
                        String rnodeName = ProtoCore.DSASM.Constants.kSetterPrefix + rnode.Name;

                        var tmpVar = nodeBuilder.BuildIdentfier(Constants.kTempArg);
                        AssociativeNode tmpAssignmentNode = null;

                        bool isSetterFunctionCall = setterArgument is InlineConditionalNode ||
                            setterArgument is FunctionCallNode;

                        if (isSetterFunctionCall)
                        {
                            var tmpRetVar = nodeBuilder.BuildTempVariable();
                            var tmpGetInlineRet = nodeBuilder.BuildBinaryExpression(tmpRetVar, setterArgument as AssociativeNode);
                            NodeUtils.CopyNodeLocation(tmpGetInlineRet, inode);
                            EmitBinaryExpressionNode(tmpGetInlineRet, ref inferedType, false, graphNode, ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier, true);
                            inode.RightNode = nodeBuilder.BuildFunctionCall(rnodeName, new List<AssociativeNode> { tmpRetVar });

                            // This is more for property assignments (including those in 
                            // constructor body). Since temporary variables and generated 
                            // function nodes do not have source locations set on them, 
                            // the location can be set to the same as IdentifierListNode 
                            // (which covers the entire assignment statement).
                            // 
                            NodeUtils.CopyNodeLocation(inode.RightNode, inode);
                            tmpAssignmentNode = nodeBuilder.BuildBinaryExpression(tmpVar, inode);
                            NodeUtils.CopyNodeLocation(tmpAssignmentNode, inode);
                        }
                        else
                        {
                            AssociativeNode fcall = nodeBuilder.BuildFunctionCall(rnodeName, new List<AssociativeNode> { setterArgument as AssociativeNode });

                            // This change is to enable class property step-through. We can only step 
                            // through them if the runtime generated "fcall" has source information 
                            // with it. IdentifierListNode now has the information, so it should pass 
                            // it along to the generated FunctionCallNode.
                            // 
                            NodeUtils.CopyNodeLocation(fcall, inode);

                            IdentifierNode leftMostIdent = leftMostIdentList.LeftNode as IdentifierNode;
                            if (string.Equals(ProtoCore.DSDefinitions.Keyword.This, leftMostIdent.Name))
                            {
                                // inode.RightNode = fcall;
                                tmpAssignmentNode = nodeBuilder.BuildBinaryExpression(tmpVar, fcall);
                            }
                            else
                            {
                                inode.RightNode = ProtoCore.Utils.CoreUtils.GenerateCallDotNode(inode.LeftNode, fcall as FunctionCallNode, core);
                                tmpAssignmentNode = nodeBuilder.BuildBinaryExpression(tmpVar, inode.RightNode);
                            }
                        }

                        NodeUtils.CopyNodeLocation(tmpAssignmentNode, inode);

                        bool allowDependents = graphNode.allowDependents;
                        if (isSetterFunctionCall)
                        {
                            graphNode.allowDependents = false;
                        }
                        EmitBinaryExpressionNode(tmpAssignmentNode, ref inferedType, false, graphNode, ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier, true);
                        graphNode.allowDependents = allowDependents;
                    }
                    else
                    {
                        // %t1.x[i] = v;
                        //
                        // will be converted to
                        // 
                        // %t2 = %t1.get_x();
                        // %t2[i] = v;
                        // %t3 = %t1.set_x(%t2);
                        //

                        // %t2 = %t1.%get_x();
                        bool allowDependents = graphNode.allowDependents;
                        graphNode.allowDependents = false;
                        var tmpVar = EmitGettersForRHSIdentList(inode, ref inferedType, graphNode); 
                        graphNode.allowDependents = allowDependents;

                        // %t2[i] = v;
                        var assignment = nodeBuilder.BuildBinaryExpression(tmpVar, setterArgument as AssociativeNode);
                        EmitBinaryExpressionNode(assignment, ref inferedType, false, graphNode, ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier, true);
                        (tmpVar as IdentifierNode).ArrayDimensions = null;

                        if (graphNode.dependentList.Count > 1)
                        {
                            graphNode.dependentList[0].updateNodeRefList[0].nodeList[0].dimensionNodeList
                                = graphNode.dependentList[1].updateNodeRefList[0].nodeList[0].dimensionNodeList;
                        }



                        graphNode.allowDependents = false;

                        // %t3 = %t1.%set_y(%t2[i]);
                        string setterName = ProtoCore.DSASM.Constants.kSetterPrefix + rnode.Name;
                        inode.RightNode = nodeBuilder.BuildFunctionCall(setterName, new List<AssociativeNode> { tmpVar });
                        var tmpSetterVar = nodeBuilder.BuildTempVariable();
                        assignment = nodeBuilder.BuildBinaryExpression(tmpSetterVar, inode);
                        
                        NodeUtils.SetNodeLocation(assignment, inode, inode);
                        EmitBinaryExpressionNode(assignment, ref inferedType, false, graphNode, ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier, true);

                        graphNode.allowDependents = allowDependents;
                    }
                }
                else
                {
                    Validity.Assert(false, "an unassignable object used as left-value.");
                }
            }
            else
            {
                IdentifierNode retnode = EmitGettersForRHSIdentList(node, ref inferedType, graphNode);
                if (retnode != null)
                {
                    EmitIdentifierNode(retnode, ref inferedType, false, graphNode, ProtoCore.DSASM.AssociativeSubCompilePass.kNone);
                    isCollapsed = true;
                }
            }
        }

        private void EmitGroupExpressionNode(AssociativeNode node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            GroupExpressionNode group = node as GroupExpressionNode;
            if (group == null)
            {
                return;
            }

            bool emitReplicationGuideFlag = emitReplicationGuide;
            emitReplicationGuide = false;
            DfsTraverse(group.Expression, ref inferedType, isBooleanOp, graphNode, subPass);
            emitReplicationGuide = emitReplicationGuideFlag;

            if (group.ArrayDimensions != null)
            {
                if (subPass != AssociativeSubCompilePass.kUnboundIdentifier)
                {
                    int dimensions = DfsEmitArrayIndexHeap(group.ArrayDimensions, graphNode);
                    EmitPushArrayIndex(dimensions);
                }
            }

            if (subPass != ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
            {
                if (core.Options.TempReplicationGuideEmptyFlag && emitReplicationGuide)
                {
                    int guides = EmitReplicationGuides(group.ReplicationGuides);
                    EmitInstrConsole(ProtoCore.DSASM.kw.pushindex, guides + "[guide]");
                    EmitPushReplicationGuide(guides);
                }
            }
        }

        protected override void EmitReturnNull()
        {
            int startpc = pc;

            EmitPushNull();
            EmitReturnToRegister();

            // Build and append a graphnode for this return statememt
            ProtoCore.DSASM.SymbolNode returnNode = new ProtoCore.DSASM.SymbolNode();
            returnNode.name = ProtoCore.DSDefinitions.Keyword.Return;

            ProtoCore.AssociativeGraph.GraphNode retNode = new ProtoCore.AssociativeGraph.GraphNode();
            //retNode.symbol = returnNode;
            retNode.PushSymbolReference(returnNode);
            retNode.procIndex = globalProcIndex;
            retNode.classIndex = globalClassIndex;
            retNode.updateBlock.startpc = startpc;
            retNode.updateBlock.endpc = pc - 1;
            retNode.isReturn = true;

            codeBlock.instrStream.dependencyGraph.Push(retNode);
        }

        private ProtoCore.Type BuildArgumentTypeFromVarDeclNode(VarDeclNode argNode)
        {
            Validity.Assert(argNode != null);
            if (argNode == null)
            {
                return new ProtoCore.Type();
            }

            int uid = core.TypeSystem.GetType(argNode.ArgumentType.Name);
            if (uid == (int)PrimitiveType.kInvalidType && !core.IsTempVar(argNode.NameNode.Name))
            {
                string message = String.Format(ProtoCore.BuildData.WarningMessage.kArgumentTypeUndefined, argNode.ArgumentType.Name, argNode.NameNode.Name);
                buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kTypeUndefined, message, core.CurrentDSFileName, argNode.line, argNode.col);
            }

            bool isArray = argNode.ArgumentType.IsIndexable;
            int rank = argNode.ArgumentType.rank;

            return core.TypeSystem.BuildTypeObject(uid, isArray, rank);
        }

        public String GetFunctionSignatureString(string functionName, ProtoCore.Type returnType, ArgumentSignatureNode signature, bool isConstructor = false)
        {
            StringBuilder functionSig = new StringBuilder(isConstructor ? "\nconstructor " : "\ndef ");
            functionSig.Append(functionName);
            functionSig.Append(":");
            functionSig.Append(String.IsNullOrEmpty(returnType.Name) ? core.TypeSystem.GetType(returnType.UID) : returnType.Name);
            if (returnType.rank < 0)
            {
                functionSig.Append("[]..[]");
            }
            else
            {
                for (int k = 0; k < returnType.rank; ++k)
                {
                    functionSig.Append("[]");
                }
            }
            functionSig.Append("(");

            for (int i = 0; i < signature.Arguments.Count; ++i)
            {
                var arg = signature.Arguments[i];
                functionSig.Append(arg.NameNode.Name);
                functionSig.Append(":");

                if (arg.ArgumentType.UID < 0)
                {
                    functionSig.Append("invalid");
                }
                else if (arg.ArgumentType.UID == 0 && !String.IsNullOrEmpty(arg.ArgumentType.Name))
                {
                    functionSig.Append(arg.ArgumentType.Name);
                }
                else
                {
                    functionSig.Append(core.TypeSystem.GetType(arg.ArgumentType.UID));
                }

                if (arg.ArgumentType.rank < 0)
                {
                    functionSig.Append("[]..[]");
                }
                else
                {
                    for (int k = 0; k < arg.ArgumentType.rank; ++k)
                    {
                        functionSig.Append("[]");
                    }
                }

                if (i < signature.Arguments.Count - 1)
                {
                    functionSig.Append(", ");
                }
            }
            functionSig.Append(")\n");
            return functionSig.ToString();
        }

        private bool IsParsingGlobal()
        {
            return (!InsideFunction()) && (ProtoCore.DSASM.AssociativeCompilePass.kGlobalScope == compilePass);
        }

        private bool IsParsingGlobalFunctionBody()
        {
            return (InsideFunction()) && (ProtoCore.DSASM.AssociativeCompilePass.kGlobalFuncBody == compilePass);
        }

        private bool IsParsingMemberFunctionBody()
        {
            return (ProtoCore.DSASM.Constants.kGlobalScope != globalClassIndex) && (ProtoCore.DSASM.AssociativeCompilePass.kClassMemFuncBody == compilePass);
        }

        private bool IsParsingGlobalFunctionSig()
        {
            return (null == localProcedure) && (ProtoCore.DSASM.AssociativeCompilePass.kGlobalFuncSig == compilePass);
        }

        private bool IsParsingMemberFunctionSig()
        {
            return (ProtoCore.DSASM.Constants.kGlobalScope != globalClassIndex) && (ProtoCore.DSASM.AssociativeCompilePass.kClassMemFuncSig == compilePass);
        }

        protected override void DfsTraverse(ProtoCore.AST.Node pNode, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone, ProtoCore.AST.Node parentNode = null)
        {
            AssociativeNode node = pNode as AssociativeNode;
            if (null == node || (node.skipMe))
                return;

            if (node is IdentifierNode)
            {
                EmitIdentifierNode(node, ref inferedType, isBooleanOp, graphNode, subPass, parentNode as BinaryExpressionNode);
            }
            else if (node is IntNode)
            {
                EmitIntNode(node, ref inferedType, isBooleanOp, graphNode, subPass);
            }
            else if (node is DoubleNode)
            {
                EmitDoubleNode(node, ref inferedType, isBooleanOp, graphNode, subPass);
            }
            else if (node is BooleanNode)
            {
                EmitBooleanNode(node, ref inferedType, subPass);
            }
            else if (node is CharNode)
            {
                EmitCharNode(node, ref inferedType, isBooleanOp, subPass);
            }
            else if (node is StringNode)
            {
                EmitStringNode(node, ref inferedType, subPass);
            }
            else if (node is DefaultArgNode)
            {
                EmitDefaultArgNode(subPass);
            }
            else if (node is NullNode)
            {
                EmitNullNode(node, ref inferedType, isBooleanOp, subPass);
            }
#if ENABLE_INC_DEC_FIX
            else if (node is PostFixNode)
            {
                EmitPostFixNode(node, ref inferedType);
            }
#endif
            else if (node is ReturnNode)
            {
                EmitReturnNode(node);
            }
            else if (node is RangeExprNode)
            {
                EmitRangeExprNode(node, ref inferedType, graphNode, subPass);
            }
            else if (node is ForLoopNode)
            {
                EmitForLoopNode(node);
            }
            else if (node is LanguageBlockNode)
            {
                EmitLanguageBlockNode(node, ref inferedType, graphNode, subPass);
            }
            else if (node is ClassDeclNode)
            {
                EmitClassDeclNode(node, ref inferedType, subPass);
            }
            else if (node is ConstructorDefinitionNode)
            {
                EmitConstructorDefinitionNode(node, ref inferedType, subPass);
            }
            else if (node is FunctionDefinitionNode)
            {
                EmitFunctionDefinitionNode(node, ref inferedType, subPass);
            }
            else if (node is FunctionCallNode)
            {
                EmitFunctionCallNode(node, ref inferedType, isBooleanOp, graphNode, subPass, parentNode as BinaryExpressionNode);
            }
            else if (node is FunctionDotCallNode)
            {
                EmitFunctionCallNode(node, ref inferedType, isBooleanOp, graphNode, subPass, parentNode as BinaryExpressionNode);
            }
            else if (node is ModifierStackNode)
            {
                EmitModifierStackNode(node, ref inferedType, isBooleanOp, graphNode, subPass);
            }
            else if (node is ExprListNode)
            {
                EmitExprListNode(node, ref inferedType, graphNode, subPass, parentNode);
            }
            else if (node is IdentifierListNode)
            {
                EmitIdentifierListNode(node, ref inferedType, isBooleanOp, graphNode, subPass, parentNode);
            }
            else if (node is IfStatementNode)
            {
                EmitIfStatementNode(node, ref inferedType);
            }
            else if (node is InlineConditionalNode)
            {
                EmitInlineConditionalNode(node, ref inferedType, graphNode, subPass, parentNode as BinaryExpressionNode);
            }
            else if (node is UnaryExpressionNode)
            {
                EmitUnaryExpressionNode(node, ref inferedType, graphNode, subPass);
            }
            else if (node is BinaryExpressionNode)
            {
                EmitBinaryExpressionNode(node, ref inferedType, isBooleanOp, graphNode, subPass);
            }
            else if (node is ImportNode)
            {
                EmitImportNode(node, ref inferedType, subPass);
            }
            else if (node is DefaultArgNode)
            {
                EmitDefaultArgNode(subPass);
            }
            else if (node is DynamicBlockNode)
            {
                int block = (node as DynamicBlockNode).block;
                EmitDynamicBlockNode(block,subPass);
            }
            else if (node is DotFunctionBodyNode)
            {
                EmitDotFunctionBodyNode(node, ref inferedType, isBooleanOp, graphNode, subPass);
            }
            else if (node is ThisPointerNode)
            {
                EmitThisPointerNode(subPass);
            }
            else if (node is DynamicNode)
            {
                EmitDynamicNode(subPass);
            }
            else if (node is ExceptionHandlingNode)
            {
                EmitExceptionHandlingNode(node, graphNode, subPass);
            }
            else if (node is ThrowNode)
            {
                EmitThrowNode(node, graphNode, subPass);
            }
            else if (node is GroupExpressionNode)
            {
                EmitGroupExpressionNode(node, ref inferedType, isBooleanOp, graphNode, subPass);
            }
            int blockId = codeBlock.codeBlockId; 

            //updatePcDictionary(node, blockId);
            //updatePcDictionary(node.line, node.col);
        }

    }

    public class NodeBuilder
    {
        private ProtoCore.Core core { get; set; }

        public NodeBuilder(ProtoCore.Core protocore)
        {
            core = protocore;
        }

        public AssociativeNode BuildIdentfier(string name, PrimitiveType type = PrimitiveType.kTypeVar)
        {
            var ident = AstFactory.BuildIdentifier(name);
            ident.datatype = TypeSystem.BuildPrimitiveTypeObject(type, false);
            return ident;
        }

        public AssociativeNode BuildTempVariable()
        {
            return BuildIdentfier(core.GenerateTempVar(), PrimitiveType.kTypeVar);
        }

        public AssociativeNode BuildTempPropertyVariable()
        {
            return BuildIdentfier(core.GenerateTempPropertyVar(), PrimitiveType.kTypeVar);
        }

        public AssociativeNode BuildReturn()
        {
            return BuildIdentfier(ProtoCore.DSDefinitions.Keyword.Return, PrimitiveType.kTypeReturn);
        }

        public AssociativeNode BuildIdentList(AssociativeNode leftNode, AssociativeNode rightNode)
        {
            var identList = new IdentifierListNode();
            identList.LeftNode = leftNode;
            identList.RightNode = rightNode;
            identList.Optr = Operator.dot;
            return identList;
        }

        public AssociativeNode BuildBinaryExpression(AssociativeNode leftNode, AssociativeNode rightNode, ProtoCore.DSASM.Operator op = Operator.assign)
        {
            var binaryExpr = AstFactory.BuildBinaryExpression(leftNode, rightNode, op);
            if (core.Options.GenerateExprID)
            {
                binaryExpr.exprUID = core.ExpressionUID;
            }
            ++core.ExpressionUID;

            return binaryExpr;
        }

        public AssociativeNode BuildFunctionCall(string functionName, List<AssociativeNode> arguments)
        {
            return AstFactory.BuildFunctionCall(functionName, arguments);
        }
    }
}

