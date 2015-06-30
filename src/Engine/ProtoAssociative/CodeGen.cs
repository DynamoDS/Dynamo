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
using System.Linq;
using ProtoAssociative.Properties;

namespace ProtoAssociative
{

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

        public ProtoCore.CompilerDefinitions.Associative.CompilePass compilePass;

        // Jun Comment: 'setConstructorStartPC' a flag to check if the graphnode pc needs to be adjusted by -1
        // This is because constructors auto insert an allocc instruction before any cosntructo body is traversed
        private bool setConstructorStartPC;

        private NodeBuilder nodeBuilder;

        private Dictionary<int, ClassDeclNode> unPopulatedClasses;

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
            setConstructorStartPC = false;

            // Re-use the existing procedureTable and symbolTable to access the built-in and predefined functions
            ProcedureTable procTable = core.CodeBlockList[0].procedureTable;
            codeBlock = BuildNewCodeBlock(procTable);

            
            // Remove global symbols from existing symbol table for subsequent run in Graph UI            
            //SymbolTable sTable = core.CodeBlockList[0].symbolTable;
            //sTable.RemoveGlobalSymbols();
            //codeBlock = core.CodeBlockList[0];

            compilePass = ProtoCore.CompilerDefinitions.Associative.CompilePass.kClassName;

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

        public CodeGen(Core coreObj, ProtoCore.CompileTime.Context callContext, ProtoCore.DSASM.CodeBlock parentBlock = null) : base(coreObj, parentBlock)
        {
            context = callContext;
            classOffset = 0;

            //  either of these should set the console to flood
            //
            ignoreRankCheck = false;
            emitReplicationGuide = false;
            astNodes = new List<AssociativeNode>();
            setConstructorStartPC = false;


            // Comment Jun: Get the codeblock to use for this codegenerator
            if (core.Options.IsDeltaExecution)
            {
                codeBlock = GetDeltaCompileCodeBlock(parentBlock);
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

            compilePass = ProtoCore.CompilerDefinitions.Associative.CompilePass.kClassName;

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

        /// <summary>
        /// Pushes the symbol as a dependent to graphNode if codegeneration semantic conditions are met
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="graphNode"></param>
        private ProtoCore.AssociativeGraph.GraphNode PushSymbolAsDependent(SymbolNode symbol, ProtoCore.AssociativeGraph.GraphNode graphNode)
        {
            // Check for symbols that need to be pushed as dependents
            // Temporary properties and default args are autogenerated and are not part of the assocaitve behavior
            // For temp properties, refer to: EmitGettersForRHSIdentList
            // For default arg temp vars, refer to usage of: Constants.kTempDefaultArg
            if (CoreUtils.IsPropertyTemp(symbol.name) || CoreUtils.IsDefaultArgTemp(symbol.name))
            {
                return null;
            }

            ProtoCore.AssociativeGraph.GraphNode dependentNode = new ProtoCore.AssociativeGraph.GraphNode();
            dependentNode.PushSymbolReference(symbol, UpdateNodeType.kSymbol);
            graphNode.PushDependent(dependentNode);
            return dependentNode;
        }

        private ProtoCore.DSASM.CodeBlock GetDeltaCompileCodeBlock(ProtoCore.DSASM.CodeBlock parentBlock)
        {
            ProtoCore.DSASM.CodeBlock cb = null;

            // Build a new codeblock for the first run or nested runs
            if (core.CodeBlockList.Count <= 0  || core.CodeBlockIndex > 1)
            {
                cb = BuildNewCodeBlock();
                if (null == parentBlock)
                {
                    core.CodeBlockList.Add(cb);
                }
            }
            else
            {
                cb = core.CodeBlockList[0];
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
                context.guid,
                ProtoCore.DSASM.CodeBlockType.kLanguage,
                ProtoCore.Language.kAssociative,
                core.CodeBlockIndex,
                new ProtoCore.DSASM.SymbolTable("associative lang block", core.RuntimeTableIndex),
                pTable,
                false,
                core);

            ++core.CodeBlockIndex;
            ++core.RuntimeTableIndex;

            return cb;
        }

        /// <summary>
        /// Call this function if there is no entry point for the current compilation session
        /// This occurs if the compilation body has only either class and function definitions
        /// </summary>
        private void SetNoEntryPoint()
        {
            if (globalProcIndex == ProtoCore.DSASM.Constants.kGlobalScope  
                && globalClassIndex == ProtoCore.DSASM.Constants.kGlobalScope 
                && !isEntrySet)
            {
                isEntrySet = true;
                codeBlock.instrStream.entrypoint = ProtoCore.DSASM.Constants.kInvalidPC;
            }
        }

        protected override void SetEntry()
        {
            if (ProtoCore.DSASM.Constants.kGlobalScope == globalProcIndex && globalClassIndex == ProtoCore.DSASM.Constants.kGlobalScope && !isEntrySet)
            {
                isEntrySet = true;
                if (ProtoCore.DSASM.Constants.kInvalidIndex != core.newEntryPoint && core.newEntryPoint < pc)
                {
                    codeBlock.instrStream.entrypoint = core.newEntryPoint;
                    core.SetNewEntryPoint(ProtoCore.DSASM.Constants.kInvalidIndex);
                }
                else
                {
                    codeBlock.instrStream.entrypoint = pc;
                }
            }
        }

        private bool IsDependentSubNode(GraphNode node, GraphNode subNode)
        {
            if (subNode.UID == node.UID
                || subNode.exprUID == node.exprUID
                || subNode.ssaExprID == node.ssaExprID
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
                    if (null != subNode.lastGraphNode)
                    {
                        return subNode.lastGraphNode;
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

                bool equalIdentList = ProtoCore.AssociativeEngine.Utils.AreLHSEqualIdentList(node, subNode);
                if (equalIdentList)
                {
                    continue;
                }

                // No update for auto generated temps
                bool isTempVarUpdate = ProtoCore.AssociativeEngine.Utils.IsTempVarLHS(node);
                if (isTempVarUpdate)
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
                string message = String.Format(ProtoCore.Properties.Resources.kInvalidStaticCyclicDependency, symbol1, symbol2);

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
            ProtoCore.CompilerDefinitions.AccessModifier access = ProtoCore.CompilerDefinitions.AccessModifier.kPublic,
            ProtoCore.DSASM.MemoryRegion region = ProtoCore.DSASM.MemoryRegion.kMemStack,
            int line = -1,
            int col = -1,
            GraphNode graphNode = null
            )
        {
            bool allocateForBaseVar = classScope < classIndex;
            bool isProperty = classIndex != Constants.kInvalidIndex && funcIndex == Constants.kInvalidIndex;
            if (!allocateForBaseVar && !isProperty && core.ClassTable.IndexOf(ident) != ProtoCore.DSASM.Constants.kInvalidIndex)
                buildStatus.LogSemanticError(String.Format(Resources.ClassNameAsVariableError, ident), null, line, col, graphNode);

            ProtoCore.DSASM.SymbolNode symbolnode = new ProtoCore.DSASM.SymbolNode();
            symbolnode.name = ident;
            symbolnode.isTemp = ident.StartsWith("%");
            symbolnode.size = datasize;
            symbolnode.functionIndex = funcIndex;
            symbolnode.absoluteFunctionIndex = funcIndex;
            symbolnode.datatype = datatype;
            symbolnode.staticType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank);
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
                    staticSymbolnode.staticType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank);
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
        
        private bool IsCompilingCodeBody()
        {
            return compilePass == ProtoCore.CompilerDefinitions.Associative.CompilePass.kClassMemFuncBody
                || compilePass == ProtoCore.CompilerDefinitions.Associative.CompilePass.kGlobalFuncBody;
        }


        /// <summary>
        /// Emits a block of code for the following cases:
        ///     Language block body
        ///     Function body
        ///     Constructor body
        /// </summary>
        /// <param name="astList"></param>
        /// <param name="inferedType"></param>
        /// <param name="subPass"></param>
        private bool EmitCodeBlock(
            List<AssociativeNode> astList, 
            ref ProtoCore.Type inferedType, 
            ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass,
            bool isProcedureOwned
            )
        {
            bool hasReturnStatement = false;
            foreach (AssociativeNode bnode in astList)
            {
                inferedType.UID = (int)PrimitiveType.kTypeVar;
                inferedType.rank = 0;

                if (bnode is LanguageBlockNode)
                {
                    // Build a binaryn node with a temporary lhs for every stand-alone language block
                    var iNode = nodeBuilder.BuildIdentfier(core.GenerateTempLangageVar());
                    BinaryExpressionNode langBlockNode = new BinaryExpressionNode();
                    langBlockNode.LeftNode = iNode;
                    langBlockNode.Optr = ProtoCore.DSASM.Operator.assign;
                    langBlockNode.RightNode = bnode;
                    langBlockNode.IsProcedureOwned = isProcedureOwned;
                    DfsTraverse(langBlockNode, ref inferedType, false, null, subPass);
                }
                else
                {
                    bnode.IsProcedureOwned = isProcedureOwned;
                    DfsTraverse(bnode, ref inferedType, false, null, subPass);
                }

                if (NodeUtils.IsReturnExpressionNode(bnode))
                {
                    hasReturnStatement = true;
                }
            }

            return hasReturnStatement;
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


        protected void EmitJumpDependency()
        {
            EmitInstrConsole(ProtoCore.DSASM.kw.jdep);

            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.JDEP;

            ++pc;
            codeBlock.instrStream.instrList.Add(instr);
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

            if (!core.Options.IsDeltaExecution)
            {
                EmitJumpDependency();
            }
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

        public ProtoCore.DSASM.ProcedureNode GetProcedureFromInstance(int classScope, ProtoCore.AST.AssociativeAST.FunctionCallNode funcCall)
        {
            string procName = funcCall.Function.Name;
            Validity.Assert(ProtoCore.DSASM.Constants.kInvalidIndex != classScope);
            List<ProtoCore.Type> arglist = new List<ProtoCore.Type>();

            foreach (AssociativeNode paramNode in funcCall.FormalArguments)
            {
                ProtoCore.Type paramType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0);
                DfsTraverse(paramNode, ref paramType, false, null, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier);
                emitReplicationGuide = false;
                enforceTypeCheck = true;

                arglist.Add(paramType);
            }

            bool isAccessible;
            int realType;
            ProtoCore.DSASM.ProcedureNode procNode = core.ClassTable.ClassNodes[classScope].GetMemberFunction(procName, arglist, globalClassIndex, out isAccessible, out realType, false);
            return procNode;
        }

        private void EmitFunctionCall(int depth, 
                                      int type,
                                      List<ProtoCore.Type> arglist, 
                                      ProcedureNode procNode, 
                                      FunctionCallNode funcCall, 
                                      bool isGetter = false, 
                                      BinaryExpressionNode parentExpression = null)
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
            EmitPushVarData(dimensions);

            // Emit depth
            EmitInstrConsole(kw.push, depth + "[depth]");
            EmitPush(StackValue.BuildInt(depth));

            // The function call
            EmitInstrConsole(ProtoCore.DSASM.kw.callr, procNode.name);

            if (isGetter)
            {
                EmitCall(procNode.procId, blockId, type, 
                         Constants.kInvalidIndex, Constants.kInvalidIndex,
                         Constants.kInvalidIndex, Constants.kInvalidIndex, 
                         procNode.pc);
            }
            // Break at function call inside dynamic lang block created for a 'true' or 'false' expression inside an inline conditional
            else if (core.DebuggerProperties.breakOptions.HasFlag(DebugProperties.BreakpointOptions.EmitInlineConditionalBreakpoint))
            {
                Validity.Assert(core.DebuggerProperties.highlightRange != null);

                ProtoCore.CodeModel.CodePoint startInclusive = core.DebuggerProperties.highlightRange.StartInclusive;
                ProtoCore.CodeModel.CodePoint endExclusive = core.DebuggerProperties.highlightRange.EndExclusive;

                EmitCall(procNode.procId, blockId, type, startInclusive.LineNo, startInclusive.CharNo, endExclusive.LineNo, endExclusive.CharNo, procNode.pc);
            }
            else if (parentExpression != null)
            {
                EmitCall(procNode.procId, blockId, type, parentExpression.line, parentExpression.col, parentExpression.endLine, parentExpression.endCol, procNode.pc);
            }
            else
            {
                EmitCall(procNode.procId, blockId, type, funcCall.line, funcCall.col, funcCall.endLine, funcCall.endCol, procNode.pc);
            }

            // The function return value
            EmitInstrConsole(ProtoCore.DSASM.kw.push, ProtoCore.DSASM.kw.regRX);
            StackValue opReturn = StackValue.BuildRegister(Registers.RX);
            EmitPush(opReturn);
        }
        
        
        private void TraverseDotCallArguments(FunctionCallNode funcCall, 
                                              FunctionDotCallNode dotCall,
                                              ProcedureNode procCallNode,
                                              List<ProtoCore.Type> arglist,
                                              string procName,
                                              int classIndex,
                                              string className,
                                              bool isStaticCall,
                                              bool isConstructor,
                                              GraphNode graphNode,
                                              ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass,
                                              BinaryExpressionNode bnode)
        {
            // Update graph dependencies
            if (subPass != ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier && graphNode != null)
            {
                if (isStaticCall)
                {
                    Validity.Assert(classIndex != Constants.kInvalidIndex);
                    Validity.Assert(!string.IsNullOrEmpty(className));

                    SymbolNode classSymbol = new SymbolNode();
                    classSymbol.name = className;
                    classSymbol.classScope = classIndex;
                    PushSymbolAsDependent(classSymbol, graphNode);
                }
            }

            int funtionArgCount = 0;
            for (int n = 0; n < funcCall.FormalArguments.Count; ++n)
            {
                if (isStaticCall || isConstructor)
                {
                    if (n != Constants.kDotArgIndexArrayArgs)
                    {
                        continue;
                    }
                }

                AssociativeNode paramNode = funcCall.FormalArguments[n];
                ProtoCore.Type paramType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0);

                emitReplicationGuide = false;

                // If it's a binary node then continue type check, otherwise 
                // disable type check and just take the type of paramNode itself
                enforceTypeCheck = !(paramNode is BinaryExpressionNode);

                if (ProtoCore.DSASM.Constants.kDotArgIndexPtr == n)
                {
                    // Traversing the first arg (the LHS pointer/Static instanct/Constructor
                    
                    // Replication guides only allowed on method, e.g.,
                    // 
                    //    x = p<1>.f({1,2}<2>); 
                    //
                    // But not on getter, e.g.,
                    // 
                    //    c = a<1>.x<2>; 
                    if (!CoreUtils.IsGetterSetter(procName) && !isConstructor)
                    {
                        emitReplicationGuide = true;
                    }

                    DfsTraverse(paramNode, ref paramType, false, graphNode, subPass, bnode);
                }
                else if (ProtoCore.DSASM.Constants.kDotArgIndexArrayArgs == n)
                {
                    // Traversing the actual arguments passed into the function 
                    // (not the dot function)
                    int defaultArgNumber = 0;

                    // If its null this is the second call in a chained dot
                    if (null != procCallNode)
                    {
                        defaultArgNumber = procCallNode.argInfoList.Count - dotCall.FunctionCall.FormalArguments.Count;
                    }

                    // Enable graphnode dependencies if its a setter method
                    bool allowDependentState = null != graphNode ? graphNode.allowDependents : false;
                    if (CoreUtils.IsSetter(procName))
                    {
                        // If the arguments are not temporaries
                        ExprListNode exprList = paramNode as ExprListNode;
                        Validity.Assert(1 == exprList.list.Count);

                        string varname = string.Empty;
                        if (exprList.list[0] is IdentifierNode)
                        {
                            varname = (exprList.list[0] as IdentifierNode).Name;

                            if (!CoreUtils.IsAutoGeneratedVar(varname))
                            {
                                graphNode.allowDependents = true;
                            }
                            else if (CoreUtils.IsSSATemp(varname) && core.Options.GenerateSSA)
                            {
                                graphNode.allowDependents = true;
                            }
                        }
                        else
                        {
                            graphNode.allowDependents = true;
                        }

                    }

                    emitReplicationGuide = true;

                    if (defaultArgNumber > 0)
                    {
                        ExprListNode exprList = paramNode as ExprListNode;

                        if (subPass != ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier)
                        {
                            for (int i = 0; i < defaultArgNumber; i++)
                            {
                                exprList.list.Add(new DefaultArgNode());
                            }

                        }

                        if (!isStaticCall && !isConstructor)
                        {
                            DfsTraverse(paramNode, ref paramType, false, graphNode, subPass);
                            funtionArgCount = exprList.list.Count;
                        }
                        else
                        {
                            foreach (AssociativeNode exprListNode in exprList.list)
                            {
                                bool repGuideState = emitReplicationGuide;
                                if (subPass != ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier)
                                {
                                    if (exprListNode is ExprListNode || exprListNode is GroupExpressionNode)
                                    {
                                        if (core.Options.TempReplicationGuideEmptyFlag)
                                        {
                                            // Emit the replication guide for the exprlist
                                            var repGuideList = GetReplicationGuides(exprListNode);
                                            if (repGuideList != null)
                                            {
                                                EmitReplicationGuides(repGuideList, true);
                                                EmitInstrConsole(ProtoCore.DSASM.kw.popg);
                                                EmitPopGuide();
                                            }

                                            emitReplicationGuide = false;
                                        }
                                    }
                                }
                                else
                                {
                                    emitReplicationGuide = false;
                                }

                                DfsTraverse(exprListNode, ref paramType, false, graphNode, subPass, bnode);
                                emitReplicationGuide = repGuideState;

                                arglist.Add(paramType);
                            }
                        }
                    }
                    else
                    {
                        ExprListNode exprList = paramNode as ExprListNode;

                        // Comment Jun: This is a getter/setter or a an auto-generated thisarg function...
                        // ...add the dynamic sv that will be resolved as a pointer at runtime
                        if (!isStaticCall && !isConstructor)
                        {
                            // TODO Jun: pls get rid of subPass checking outside the core travesal
                            if (ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone == subPass)
                            {
                                exprList.list.Insert(0, new DynamicNode());
                            }
                        }

                        if (exprList.list.Count > 0)
                        {
                            foreach (AssociativeNode exprListNode in exprList.list)
                            {
                                bool repGuideState = emitReplicationGuide;
                                if (subPass != ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier)
                                {
                                    if (exprListNode is ExprListNode || exprListNode is GroupExpressionNode)
                                    {
                                        if (core.Options.TempReplicationGuideEmptyFlag)
                                        {
                                            // Emit the replication guide for the exprlist
                                            var repGuideList = GetReplicationGuides(exprListNode);
                                            if (repGuideList != null)
                                            {
                                                EmitReplicationGuides(repGuideList, true);
                                                EmitInstrConsole(ProtoCore.DSASM.kw.popg);
                                                EmitPopGuide();
                                            }

                                            emitReplicationGuide = false;
                                        }
                                    }
                                }
                                else
                                {
                                    emitReplicationGuide = false;
                                }

                                DfsTraverse(exprListNode, ref paramType, false, graphNode, subPass, bnode);
                                emitReplicationGuide = repGuideState;

                                arglist.Add(paramType);
                            }

                            if (subPass != ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier &&
                                !isStaticCall &&
                                !isConstructor)
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
                            if (!isStaticCall && !isConstructor)
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
                    IntNode argNumNode = new IntNode(funtionArgCount);
                    DfsTraverse(argNumNode, ref paramType, false, graphNode, subPass);
                }
                else
                {
                    DfsTraverse(paramNode, ref paramType, false, graphNode, subPass);
                }

                emitReplicationGuide = false;
                enforceTypeCheck = true;

                if (!isStaticCall || !isConstructor)
                {
                    arglist.Add(paramType);
                }
            } 
        }

        public ProcedureNode TraverseDotFunctionCall(
                                ProtoCore.AST.Node node, 
                                ProtoCore.AST.Node parentNode, 
                                int lefttype, 
                                int depth, 
                                GraphNode graphNode, 
                                ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass,
                                BinaryExpressionNode bnode,
                                ref ProtoCore.Type inferedType)
        {
            ProcedureNode procCallNode = null;

            var dotCallType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0); ;

            bool isConstructor = false;
            bool isStaticCall = false;
            bool isUnresolvedDot = false;

            int classIndex = Constants.kInvalidIndex;
            string className = string.Empty;

            var dotCall = new FunctionDotCallNode(node as FunctionDotCallNode);
            var funcCall = dotCall.DotCall;
            var procName = dotCall.FunctionCall.Function.Name;

            var firstArgument = dotCall.DotCall.FormalArguments[0];
            if (firstArgument is FunctionDotCallNode)
            {
                isUnresolvedDot = true;
            }
            else if (firstArgument is IdentifierNode)
            {
                // Check if the lhs identifer is a class name
                className = (firstArgument as IdentifierNode).Name;
                classIndex = core.ClassTable.IndexOf(className);

                // Check if the lhs is an variable
                SymbolNode symbolnode;
                bool isAccessible;
                bool hasAllocated = VerifyAllocation(className,
                                                     globalClassIndex,
                                                     globalProcIndex,
                                                     out symbolnode,
                                                     out isAccessible);

                bool toResolveMethodOnClass = classIndex != Constants.kInvalidIndex;

                // If the lhs is an variable which happens to have a same name
                // as some class, then check the right hand side is a valid 
                // constructor or static function call. 
                if (toResolveMethodOnClass && hasAllocated && isAccessible)
                {
                    var classes = core.ClassTable.ClassNodes;
                    var classNode = classes[classIndex];
                    int argCount = dotCall.FunctionCall.FormalArguments.Count;
                    var procNode = classNode.GetFirstConstructorBy(procName, argCount);
                    if (procNode == null)
                    {
                        procNode = classNode.GetFirstStaticFunctionBy(procName, argCount);
                    }

                    if (procNode == null)
                    {
                        toResolveMethodOnClass = false;
                        classIndex = Constants.kInvalidIndex;
                        className = string.Empty;
                    }
                }

                if (toResolveMethodOnClass)
                {
                    dotCall.DotCall.FormalArguments[0] = new IntNode(classIndex);
                    inferedType.UID = dotCallType.UID = classIndex;

                    // Now the left hand side of dot call is a valid class name.
                    // There are three cases for the right hand side: calling a 
                    // function, getting a static property or getting a function 
                    // pointer. I.e.,
                    // 
                    //    x = Foo.foo();
                    //
                    // Or
                    //    
                    //    y = Bar.bar;   // static property or funciton pointer
                    //    Bar.bar_2 = z; // static property
                    // 
                    // For the latters, they are converted to getter/setter. 
                    // I.e., 
                    //
                    //    y = Bar.%get_bar();
                    //    %ret = Bar.%set_bar_2(z);
                    //
                    // We need to check each case. 

                    var classes = core.ClassTable.ClassNodes;
                    var classNode = classes[classIndex];

                    var property = String.Empty;
                    if (CoreUtils.TryGetPropertyName(procName, out property))
                    {
                        if (procCallNode == null)
                        {
                            procCallNode = classNode.GetFirstStaticFunctionBy(procName);
                            isStaticCall = procCallNode != null;
                        }

                        if (procCallNode == null)
                        {
                            if (subPass != ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone)
                            {
                                return null;
                            }

                            // Try static function firstly
                            procCallNode = classNode.GetFirstStaticFunctionBy(property);
                            if (procCallNode == null)
                            {
                                procCallNode = classNode.GetFirstMemberFunctionBy(property);
                            }

                            if (procCallNode == null)
                            {
                                procCallNode = classNode.GetFirstMemberFunctionBy(procName);
                            }

                            if (procCallNode != null)
                            {
                                EmitFunctionPointer(procCallNode);
                            }
                            else
                            {
                                string message = String.Format(ProtoCore.Properties.Resources.kCallingNonStaticProperty,
                                                               className,
                                                               property);

                                buildStatus.LogWarning(WarningID.kCallingNonStaticMethodOnClass,
                                                       message,
                                                       core.CurrentDSFileName,
                                                       dotCall.line,
                                                       dotCall.col, 
                                                       graphNode);

                                EmitNullNode(new NullNode(), ref inferedType);
                            }
                            
                            return null;
                        }
                    }
                    else
                    {
                        int argCount = dotCall.FunctionCall.FormalArguments.Count;
                        procCallNode = classNode.GetFirstConstructorBy(procName, argCount);
                        isConstructor = procCallNode != null;

                        if (procCallNode == null)
                        {
                            procCallNode = classNode.GetFirstStaticFunctionBy(procName, argCount);
                            isStaticCall = procCallNode != null;
                        }

                        if (!isStaticCall && !isConstructor)
                        {
                            if (subPass == ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone)
                            {
                                string message = String.Format(ProtoCore.Properties.Resources.kStaticMethodNotFound,
                                                               className,
                                                               procName);

                                buildStatus.LogWarning(WarningID.kFunctionNotFound,
                                                       message,
                                                       core.CurrentDSFileName,
                                                       dotCall.line,
                                                       dotCall.col, 
                                                       graphNode);

                                EmitNullNode(new NullNode(), ref inferedType);
                            }

                            return null;
                        }
                    }
                }
                else if (hasAllocated && symbolnode.datatype.UID != (int)PrimitiveType.kTypeVar)
                {
                    inferedType.UID = symbolnode.datatype.UID;
                    if (Constants.kInvalidIndex != inferedType.UID)
                    {
                        procCallNode = GetProcedureFromInstance(symbolnode.datatype.UID, dotCall.FunctionCall);
                    }

                    if (null != procCallNode)
                    {
                        if (procCallNode.isConstructor)
                        {
                            if (subPass == ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone)
                            {
                                string message = String.Format(ProtoCore.Properties.Resources.KCallingConstructorOnInstance,
                                                               procName);
                                buildStatus.LogWarning(WarningID.kCallingConstructorOnInstance,
                                                       message,
                                                       core.CurrentDSFileName,
                                                       funcCall.line,
                                                       funcCall.col, 
                                                       graphNode);
                                EmitNullNode(new NullNode(), ref inferedType);
                            }
                            return null;
                        }

                        isAccessible = procCallNode.access == ProtoCore.CompilerDefinitions.AccessModifier.kPublic
                             || (procCallNode.access == ProtoCore.CompilerDefinitions.AccessModifier.kPrivate && procCallNode.classScope == globalClassIndex);

                        if (!isAccessible)
                        {
                            if (subPass == ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone)
                            {
                                string message = String.Format(ProtoCore.Properties.Resources.kMethodIsInaccessible, procName);
                                buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kAccessViolation, message, core.CurrentDSFileName, funcCall.line, funcCall.col, graphNode);
                            }
                        }

                        var dynamicRhsIndex = (int)(dotCall.DotCall.FormalArguments[1] as IntNode).Value;
                        var dynFunc = core.DynamicFunctionTable.GetFunctionAtIndex(dynamicRhsIndex);
                        dynFunc.ClassIndex = procCallNode.classScope;
                    }
                }
                else
                {
                    isUnresolvedDot = true;
                }
            }

            // Its an accceptable method at this point
            List<ProtoCore.Type> arglist = new List<ProtoCore.Type>();
            TraverseDotCallArguments(funcCall, 
                                     dotCall,
                                     procCallNode, 
                                     arglist,
                                     procName, 
                                     classIndex, 
                                     className, 
                                     isStaticCall, 
                                     isConstructor,
                                     graphNode, 
                                     subPass, 
                                     bnode);

            if (subPass == ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier)
            {
                return null;
            }

            if (!isConstructor && !isStaticCall)
            {
                Validity.Assert(dotCall.DotCall.FormalArguments[ProtoCore.DSASM.Constants.kDotArgIndexArrayArgs] is ExprListNode);
                ExprListNode functionArgs = dotCall.DotCall.FormalArguments[ProtoCore.DSASM.Constants.kDotArgIndexArrayArgs] as ExprListNode;
                functionArgs.list.Insert(0, dotCall.DotCall.FormalArguments[ProtoCore.DSASM.Constants.kDotArgIndexPtr]);
            }
           
            // From here on, handle the actual procedure call 
            int type = ProtoCore.DSASM.Constants.kInvalidIndex;

            int refClassIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
            if (parentNode != null && parentNode is IdentifierListNode)
            {
                var leftnode = (parentNode as IdentifierListNode).LeftNode;
                if (leftnode != null && leftnode is IdentifierNode)
                {
                    refClassIndex = core.ClassTable.IndexOf(leftnode.Name);
                }
            }

            if (firstArgument is FunctionCallNode ||
                firstArgument is FunctionDotCallNode ||
                firstArgument is ExprListNode)
            {
                inferedType.UID = arglist[0].UID;
            }

            // If lefttype is a valid class then check if calling a constructor
            if (procCallNode == null 
                && (int)PrimitiveType.kInvalidType != inferedType.UID
                && (int)PrimitiveType.kTypeVoid != inferedType.UID
                && procName != Constants.kFunctionPointerCall)
            {
                procCallNode = core.ClassTable.ClassNodes[inferedType.UID].GetFirstMemberFunctionBy(procName);
            }

            // Try function pointer firstly
            if ((procCallNode == null) && (procName != Constants.kFunctionPointerCall))
            {
                bool isAccessibleFp;
                ProtoCore.DSASM.SymbolNode symbolnode = null;
                bool isAllocated = VerifyAllocation(procName, globalClassIndex, globalProcIndex, out symbolnode, out isAccessibleFp);
                if (isAllocated) // not checking the type against function pointer, as the type could be var
                {
                    procName = Constants.kFunctionPointerCall;
                    // The graph node always depends on this function pointer
                    if (null != graphNode)
                    {
                        PushSymbolAsDependent(symbolnode, graphNode);
                    }
                }
            }

            // Always try global function firstly. Because we dont have syntax
            // support for calling global function (say, ::foo()), if we try
            // member function firstly, there is no way to call a global function
            // For member function, we can use this.foo() to distinguish it from 
            // global function. 
            if ((procCallNode == null) && (procName != Constants.kFunctionPointerCall))
            {
                procCallNode = CoreUtils.GetFirstVisibleProcedure(procName, arglist, codeBlock);
                if (null != procCallNode)
                {
                    type = Constants.kGlobalScope;
                    if (core.TypeSystem.IsHigherRank(procCallNode.returntype.UID, inferedType.UID))
                    {
                        inferedType = procCallNode.returntype;
                    }
                }
            }

            // Try member functions in global class scope
            if ((procCallNode == null) && (procName != Constants.kFunctionPointerCall) && (parentNode == null))
            {
                if (globalClassIndex != Constants.kInvalidIndex)
                {
                    int realType;
                    bool isAccessible;
                    bool isStaticOrConstructor = refClassIndex != Constants.kInvalidIndex;
                    ProcedureNode memProcNode = core.ClassTable.ClassNodes[globalClassIndex].GetMemberFunction(procName, arglist, globalClassIndex, out isAccessible, out realType, isStaticOrConstructor);

                    if (memProcNode != null)
                    {
                        Validity.Assert(realType != Constants.kInvalidIndex);
                        procCallNode = memProcNode;
                        inferedType = procCallNode.returntype;
                        type = realType;

                        if (!isAccessible)
                        {
                            string message = String.Format(ProtoCore.Properties.Resources.kMethodIsInaccessible, procName);
                            buildStatus.LogWarning(WarningID.kAccessViolation, message, core.CurrentDSFileName, funcCall.line, funcCall.col, graphNode);

                            inferedType.UID = (int)PrimitiveType.kTypeNull;
                            EmitPushNull();
                            return procCallNode;
                        }
                    }
                }
            }

            if (isUnresolvedDot || procCallNode == null)
            {
                if (dotCallType.UID != (int)PrimitiveType.kTypeVar)
                {
                    inferedType.UID = dotCallType.UID;
                }

                var procNode = CoreUtils.GetFirstVisibleProcedure(Constants.kDotMethodName, null, codeBlock);
                if (CoreUtils.IsGetter(procName))
                {
                    EmitFunctionCall(depth, type, arglist, procNode, funcCall, true);
                }
                else
                {
                    EmitFunctionCall(depth, type, arglist, procNode, funcCall, false, bnode);
                }
                return procNode;
            }
            else
            {
                if (procCallNode.isConstructor &&
                    (globalClassIndex != Constants.kInvalidIndex) &&
                    (globalProcIndex != Constants.kInvalidIndex) &&
                    (globalClassIndex == inferedType.UID))
                {
                    ProcedureNode contextProcNode = core.ClassTable.ClassNodes[globalClassIndex].vtable.procList[globalProcIndex];
                    if (contextProcNode.isConstructor &&
                        string.Equals(contextProcNode.name, procCallNode.name) &&
                        contextProcNode.runtimeIndex == procCallNode.runtimeIndex)
                    {
                        string message = String.Format(ProtoCore.Properties.Resources.kCallingConstructorInConstructor, procName);
                        buildStatus.LogWarning(WarningID.kCallingConstructorInConstructor, message, core.CurrentDSFileName, node.line, node.col, graphNode);
                        inferedType.UID = (int)PrimitiveType.kTypeNull;
                        EmitPushNull();
                        return procCallNode;
                    }
                }

                inferedType = procCallNode.returntype;

                // Get the dot call procedure
                if (isConstructor || isStaticCall)
                {
                    bool isGetter = CoreUtils.IsGetter(procName);
                    EmitFunctionCall(depth, procCallNode.classScope, arglist, procCallNode, funcCall, isGetter, bnode);
                }
                else
                {
                    var procNode = CoreUtils.GetFirstVisibleProcedure(Constants.kDotMethodName, null, codeBlock);
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
                }

                if (isConstructor)
                {
                    foreach (AssociativeNode paramNode in dotCall.FunctionCall.FormalArguments)
                    {
                        // Get the lhs symbol list
                        ProtoCore.Type ltype = new ProtoCore.Type();
                        ltype.UID = globalClassIndex;
                        UpdateNodeRef argNodeRef = new UpdateNodeRef();
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
        }


        public override ProcedureNode TraverseFunctionCall(
            ProtoCore.AST.Node node, 
            ProtoCore.AST.Node parentNode, 
            int lefttype, 
            int depth, 
            ref ProtoCore.Type inferedType,             
            GraphNode graphNode = null,
            ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone,                 
            ProtoCore.AST.Node bnode = null)
        {
            ProcedureNode procNode = null;
            if (node is FunctionDotCallNode)
            {
                procNode = TraverseDotFunctionCall(node, 
                                               parentNode, 
                                               lefttype, 
                                               depth, 
                                               graphNode, 
                                               subPass, 
                                               bnode as BinaryExpressionNode,
                                               ref inferedType);

                if (graphNode != null && procNode != null)
                {
                    GenerateCallsiteIdentifierForGraphNode(graphNode, procNode.name);
                }
                return procNode;
            }

            var arglist = new List<ProtoCore.Type>();
            var funcCall = node as FunctionCallNode;
            var procName = funcCall.Function.Name;
            int classIndex = core.ClassTable.IndexOf(procName);

            // To support unamed constructor
            if (classIndex != Constants.kInvalidIndex)
            {
                bool isAccessible;
                int dummy;

                ProcedureNode constructor = core.ClassTable.ClassNodes[classIndex].GetMemberFunction(procName, arglist, globalClassIndex, out isAccessible, out dummy, true);
                if (constructor != null && constructor.isConstructor)
                {
                    var rhsFNode = node as FunctionCallNode;
                    var classNode = nodeBuilder.BuildIdentfier(procName);
                    var dotCallNode = CoreUtils.GenerateCallDotNode(classNode, rhsFNode, core);

                    procNode = TraverseDotFunctionCall(dotCallNode, 
                                                   parentNode, 
                                                   lefttype, 
                                                   depth, 
                                                   graphNode, 
                                                   subPass, 
                                                   bnode as BinaryExpressionNode,
                                                   ref inferedType);
                    if (graphNode != null && procNode != null)
                    {
                        GenerateCallsiteIdentifierForGraphNode(graphNode, procNode.name);
                    }
                    return procNode;
                }
            }

            foreach (AssociativeNode paramNode in funcCall.FormalArguments)
            {
                var paramType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0);

                // The range expression function does not need replication guides
                emitReplicationGuide = !procName.Equals(Constants.kFunctionRangeExpression)  
                                    && !CoreUtils.IsGetterSetter(procName);

                // If it's a binary node then continue type check, otherwise 
                // disable type check and just take the type of paramNode itself
                // f(1+2.0) -> type check enabled - param is typed as double
                // f(2) -> type check disabled - param is typed as int
                enforceTypeCheck = !(paramNode is BinaryExpressionNode);

                DfsTraverse(paramNode, ref paramType, false, graphNode, subPass, bnode);

                emitReplicationGuide = false;
                enforceTypeCheck = true;

                arglist.Add(paramType);
            }
           
            if (subPass == ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier)
            {
                return null;   
            }
                    
            int refClassIndex = Constants.kInvalidIndex;
            if (parentNode != null && parentNode is IdentifierListNode)
            {
                var leftnode = (parentNode as IdentifierListNode).LeftNode;
                if (leftnode != null && leftnode is IdentifierNode)
                {
                    refClassIndex = core.ClassTable.IndexOf(leftnode.Name);
                }
            }

            int type = Constants.kInvalidIndex;

            // Check for the actual method, not the dot method
            // If lefttype is a valid class then check if calling a constructor
            if ((int)PrimitiveType.kInvalidType != inferedType.UID
                && (int)PrimitiveType.kTypeVoid != inferedType.UID
                && procName != Constants.kFunctionPointerCall)
            {

                bool isAccessible;
                int realType;

                bool isStaticOrConstructor = refClassIndex != Constants.kInvalidIndex;
                procNode = core.ClassTable.ClassNodes[inferedType.UID].GetMemberFunction(procName, arglist, globalClassIndex, out isAccessible, out realType, isStaticOrConstructor);

                if (procNode != null)
                {
                    Validity.Assert(realType != Constants.kInvalidIndex);
                    type = lefttype = realType;

                    if (!isAccessible)
                    {
                        type = lefttype = realType;
                        procNode = null;
                        string message = String.Format(ProtoCore.Properties.Resources.kMethodIsInaccessible, procName);
                        buildStatus.LogWarning(WarningID.kAccessViolation, message, core.CurrentDSFileName, funcCall.line, funcCall.col, graphNode);
                        inferedType.UID = (int)PrimitiveType.kTypeNull;

                        EmitPushNull();
                        if (graphNode != null && procNode != null)
                        {
                            GenerateCallsiteIdentifierForGraphNode(graphNode, procNode.name);
                        }
                        return procNode;
                    }

                }
            }

            // Try function pointer firstly
            if ((procNode == null) && (procName != ProtoCore.DSASM.Constants.kFunctionPointerCall))
            {
                bool isAccessibleFp;
                SymbolNode symbolnode = null;
                bool isAllocated = VerifyAllocation(procName, globalClassIndex, globalProcIndex, out symbolnode, out isAccessibleFp);

                if (isAllocated) 
                {
                    // The graph node always depends on this function pointer
                    if (null != graphNode)
                    {
                        PushSymbolAsDependent(symbolnode, graphNode);
                        GenerateCallsiteIdentifierForGraphNode(graphNode, procName);
                    }

                    // not checking the type against function pointer, as the 
                    // type could be var
                    procName = Constants.kFunctionPointerCall;
                }
            }

            // Always try global function firstly. Because we dont have syntax
            // support for calling global function (say, ::foo()), if we try
            // member function firstly, there is no way to call a global function
            // For member function, we can use this.foo() to distinguish it from 
            // global function. 
            if ((procNode == null) && (procName != Constants.kFunctionPointerCall))
            {
                procNode = CoreUtils.GetFirstVisibleProcedure(procName, arglist, codeBlock);
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
            if ((procNode == null) && (procName != Constants.kFunctionPointerCall) && (parentNode == null))
            {
                if (globalClassIndex != Constants.kInvalidIndex)
                {
                    int realType;
                    bool isAccessible;
                    bool isStaticOrConstructor = refClassIndex != Constants.kInvalidIndex;
                    ProtoCore.DSASM.ProcedureNode memProcNode = core.ClassTable.ClassNodes[globalClassIndex].GetMemberFunction(procName, arglist, globalClassIndex, out isAccessible, out realType, isStaticOrConstructor);

                    if (memProcNode != null)
                    {
                        Validity.Assert(realType != Constants.kInvalidIndex);
                        procNode = memProcNode;
                        inferedType = procNode.returntype;
                        type = realType;

                        if (!isAccessible)
                        {
                            string message = String.Format(ProtoCore.Properties.Resources.kMethodIsInaccessible, procName);
                            buildStatus.LogWarning(WarningID.kAccessViolation, message, core.CurrentDSFileName, funcCall.line, funcCall.col, graphNode);

                            inferedType.UID = (int)PrimitiveType.kTypeNull;
                            EmitPushNull();
                            if (graphNode != null && procNode != null)
                            {
                                GenerateCallsiteIdentifierForGraphNode(graphNode, procNode.name);
                            }
                            return procNode;
                        }
                    }
                }
            }

            if (null != procNode)
            {
                if (procNode.isConstructor && 
                    (globalClassIndex != Constants.kInvalidIndex) && 
                    (globalProcIndex != Constants.kInvalidIndex) && 
                    (globalClassIndex == inferedType.UID))
                {
                    ProcedureNode contextProcNode = core.ClassTable.ClassNodes[globalClassIndex].vtable.procList[globalProcIndex];
                    if (contextProcNode.isConstructor &&
                        string.Equals(contextProcNode.name, procNode.name) &&
                        contextProcNode.runtimeIndex == procNode.runtimeIndex)
                    {
                        string message = String.Format(ProtoCore.Properties.Resources.kCallingConstructorInConstructor, procName);
                        buildStatus.LogWarning(WarningID.kCallingConstructorInConstructor, message, core.CurrentDSFileName, node.line, node.col, graphNode);
                        inferedType.UID = (int)PrimitiveType.kTypeNull;
                        EmitPushNull();
                        if (graphNode != null && procNode != null)
                        {
                            GenerateCallsiteIdentifierForGraphNode(graphNode, procNode.name);
                        }
                        return procNode;
                    }
                }

                inferedType = procNode.returntype;

                if (procNode.procId != Constants.kInvalidIndex)
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
                        UpdateNodeRef argNodeRef = new UpdateNodeRef();
                        DFSGetSymbolList(paramNode, ref ltype, argNodeRef);

                        if (null != graphNode)
                        {
                            if (argNodeRef.nodeList.Count > 0)
                            {
                                graphNode.updatedArguments.Add(argNodeRef);
                            }
                        }
                    }

                    // The function is at block 0 if its a constructor, member 
                    // or at the globals scope.  Its at block 1 if its inside a 
                    // language block. Its limited to block 1 as of R1 since we 
                    // dont support nested function declarations yet
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
                    EmitPushVarData(dimensions);

                    // Emit depth
                    EmitInstrConsole(kw.push, depth + "[depth]");
                    EmitPush(StackValue.BuildInt(depth));

                    // The function call
                    EmitInstrConsole(kw.callr, procNode.name);

                    // Do not emit breakpoints at built-in methods like _add/_sub etc. - pratapa
                    if (procNode.isAssocOperator || procNode.name.Equals(Constants.kInlineConditionalMethodName))
                    {
                        EmitCall(procNode.procId, 
                                 blockId,
                                 type, 
                                 Constants.kInvalidIndex, 
                                 Constants.kInvalidIndex, 
                                 Constants.kInvalidIndex, 
                                 Constants.kInvalidIndex, 
                                 procNode.pc);
                    }
                    // Break at function call inside dynamic lang block created for a 'true' or 'false' expression inside an inline conditional
                    else if (core.DebuggerProperties.breakOptions.HasFlag(DebugProperties.BreakpointOptions.EmitInlineConditionalBreakpoint))
                    {
                        var codeRange = core.DebuggerProperties.highlightRange;
                        Validity.Assert(codeRange != null);

                        var startInclusive = codeRange.StartInclusive;
                        var endExclusive = codeRange.EndExclusive;

                        EmitCall(procNode.procId, 
                                 blockId,
                                 type, 
                                 startInclusive.LineNo, 
                                 startInclusive.CharNo, 
                                 endExclusive.LineNo, 
                                 endExclusive.CharNo, 
                                 procNode.pc);
                    }
                    // Use startCol and endCol of binary expression node containing function call except if it's a setter
                    else if (bnode != null && !procNode.name.StartsWith(Constants.kSetterPrefix))
                    {
                        EmitCall(procNode.procId, 
                                 blockId,
                                 type, 
                                 bnode.line, 
                                 bnode.col, 
                                 bnode.endLine, 
                                 bnode.endCol, 
                                 procNode.pc);
                    }
                    else
                    {
                        EmitCall(procNode.procId, 
                                 blockId,
                                 type, 
                                 funcCall.line, 
                                 funcCall.col, 
                                 funcCall.endLine, 
                                 funcCall.endCol, 
                                 procNode.pc);
                    }

                    // The function return value
                    EmitInstrConsole(kw.push, kw.regRX);
                    StackValue opReturn = StackValue.BuildRegister(Registers.RX);
                    EmitPush(opReturn);
                }
            }
            else
            {
                if (depth <= 0 && procName != ProtoCore.DSASM.Constants.kFunctionPointerCall)
                {
                    string property;
                    if (CoreUtils.TryGetPropertyName(procName, out property))
                    {
                        string message = String.Format(ProtoCore.Properties.Resources.kPropertyNotFound, property);
                        buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kPropertyNotFound, message, core.CurrentDSFileName, funcCall.line, funcCall.col, graphNode);
                    }
                    else
                    {
                        string message = String.Format(ProtoCore.Properties.Resources.kMethodNotFound, procName);
                        buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kFunctionNotFound, message, core.CurrentDSFileName, funcCall.line, funcCall.col, graphNode);
                    }

                    inferedType.UID = (int)PrimitiveType.kTypeNull;
                    EmitPushNull();
                }
                else
                {
                    DynamicFunction dynFunc = null;
                    if (procName == Constants.kFunctionPointerCall && depth == 0)
                    {
                        if (!core.DynamicFunctionTable.TryGetFunction(procName, arglist.Count, lefttype, out dynFunc))
                        {
                            dynFunc = core.DynamicFunctionTable.AddNewFunction(procName, arglist.Count, lefttype);
                        }
                        var iNode = nodeBuilder.BuildIdentfier(funcCall.Function.Name);
                        EmitIdentifierNode(iNode, ref inferedType);
                    }
                    else
                    {
                        if (!core.DynamicFunctionTable.TryGetFunction(procName, arglist.Count, lefttype, out dynFunc))
                        {
                            dynFunc = core.DynamicFunctionTable.AddNewFunction(procName, arglist.Count, lefttype);
                        }
                    }

                    // Emit depth
                    EmitInstrConsole(kw.push, depth + "[depth]");
                    EmitPush(StackValue.BuildInt(depth));

                    // The function call
                    EmitInstrConsole(kw.callr, funcCall.Function.Name + "[dynamic]");
                    EmitDynamicCall(dynFunc.Index, 
                                    globalClassIndex, 
                                    funcCall.line, 
                                    funcCall.col, 
                                    funcCall.endLine, 
                                    funcCall.endCol);

                    // The function return value
                    EmitInstrConsole(kw.push, kw.regRX);
                    StackValue opReturn = StackValue.BuildRegister(Registers.RX);
                    EmitPush(opReturn);

                    //assign inferedType to var
                    inferedType.UID = (int)PrimitiveType.kTypeVar;
                }
            }

            if (graphNode != null && procNode != null)
            {
                GenerateCallsiteIdentifierForGraphNode(graphNode, procNode.name);
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
                    bnode.IsFirstIdentListNode = true;

                    // Left node
                    var identNode = nodeBuilder.BuildIdentfier(ProtoCore.Utils.CoreUtils.BuildSSATemp(core));
                    (identNode as IdentifierNode).ReplicationGuides = GetReplicationGuides(ident);
                    bnode.LeftNode = identNode;

                    // Right node
                    bnode.RightNode = ident;

                    astlist.Add(bnode);
                    ssaStack.Push(bnode);
                }
                else
                {
                    EmitSSAArrayIndex(ident, ssaStack, ref astlist, true);
                    (astlist[0] as BinaryExpressionNode).IsFirstIdentListNode = true;
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
                    bnode.IsFirstIdentListNode = true;

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
                    (astlist[0] as BinaryExpressionNode).IsFirstIdentListNode = true;
                }

            }
            else if (node is IdentifierListNode)
            {
                IdentifierListNode identList = node as IdentifierListNode;

                // Build the rhs identifier list containing the temp pointer
                IdentifierListNode rhsIdentList = new IdentifierListNode();
                rhsIdentList.Optr = Operator.dot;

                bool isLeftNodeExprList = false;

                // Check if identlist matches any namesapce
                bool resolvedCall = false;
                string[] classNames = ProtoCore.Utils.CoreUtils.GetResolvedClassName(core.ClassTable, identList);
                if (classNames.Length == 1)
                {
                    //
                    // The identlist is a class name and should not be SSA'd
                    // such as:
                    //  p = Obj.Create()
                    //
                    var leftNode = nodeBuilder.BuildIdentfier(classNames[0]);
                    rhsIdentList.LeftNode = leftNode;
                    ProtoCore.Utils.CoreUtils.CopyDebugData(leftNode, node);
                    resolvedCall = true;
                }
                else if (classNames.Length > 0)
                {
                    // There is a resolution conflict
                    // identList resolved to multiple classes 

                    // TODO Jun: Move this warning handler to after the SSA transform
                    // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5221
                    buildStatus.LogSymbolConflictWarning(identList.LeftNode.ToString(), classNames);
                    rhsIdentList = identList;
                    resolvedCall = true;
                }
                else
                {
                    // identList unresolved
                    // Continue to transform this identlist into SSA
                    isLeftNodeExprList = identList.LeftNode is ExprListNode;
                    
                    // Recursively traversse the left of the ident list
                    SSAIdentList(identList.LeftNode, ref ssaStack, ref astlist);

                    AssociativeNode lhsNode = ssaStack.Pop();
                    if (lhsNode is BinaryExpressionNode)
                    {
                        rhsIdentList.LeftNode = (lhsNode as BinaryExpressionNode).LeftNode;
                    }
                    else
                    {
                        rhsIdentList.LeftNode = lhsNode;
                    }
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
                        var replicationGuides = GetReplicationGuides(arg);
                        if (replicationGuides == null)
                        {
                            replicationGuides = new List<AssociativeNode> { };
                        }
                        else
                        {
                            RemoveReplicationGuides(arg);
                        }

                        DFSEmitSSA_AST(arg, ssaStack, ref astlistArgs);

                        var argNode = ssaStack.Pop();
                        var argBinaryExpr = argNode as BinaryExpressionNode;
                        if (argBinaryExpr != null)
                        {
                            var newArgNode = NodeUtils.Clone(argBinaryExpr.LeftNode);
                            (newArgNode as IdentifierNode).ReplicationGuides = replicationGuides;
                            fcNode.FormalArguments[idx] = newArgNode;
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
                    if (resolvedCall || isLeftNodeExprList )
                    {
                        bnode.IsFirstIdentListNode = true;
                    }
                    astlist.Add(bnode);
                    ssaStack.Push(bnode);
                }
                else
                {
                    List<AssociativeNode> ssaAstList = new List<AssociativeNode>();
                    EmitSSAArrayIndex(rhsIdentList, ssaStack, ref ssaAstList, true);
                    if (resolvedCall || isLeftNodeExprList)
                    {
                        (ssaAstList[0] as BinaryExpressionNode).IsFirstIdentListNode = true;
                    }
                    astlist.AddRange(ssaAstList);
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
        /// Emits the SSA form of each dimension in the ArrayNode data structure
        /// The dimensionNode is updated by the function with the SSA'd dimensions
        /// 
        /// Given:
        ///     dimensionNode -> a[b][c] 
        /// Outputs:
        ///     astlist -> t0 = b
        ///             -> t1 = c
        ///     dimensionNode -> a[t0][t1] 
        ///     
        /// </summary>
        /// <param name="dimensionNode"></param>
        /// <param name="astlist"></param>
        private void EmitSSAforArrayDimension(ref ArrayNode dimensionNode, ref List<AssociativeNode> astlist)
        {
            AssociativeNode indexNode = dimensionNode.Expr;
            List<AssociativeNode> ssaIndexList = new List<AssociativeNode>();

            // Traverse first dimension
            Stack<AssociativeNode> localStack = new Stack<AssociativeNode>();
            DFSEmitSSA_AST(indexNode, localStack, ref astlist);

            AssociativeNode tempIndexNode = localStack.Last();
            if (tempIndexNode is BinaryExpressionNode)
            {
                dimensionNode.Expr = (tempIndexNode as BinaryExpressionNode).LeftNode;

                // Traverse next dimension
                indexNode = dimensionNode.Type;
                while (indexNode is ArrayNode)
                {
                    ArrayNode arrayNode = indexNode as ArrayNode;
                    localStack = new Stack<AssociativeNode>();
                    DFSEmitSSA_AST(arrayNode.Expr, localStack, ref astlist);
                    tempIndexNode = localStack.Last();
                    if (tempIndexNode is BinaryExpressionNode)
                    {
                        arrayNode.Expr = (tempIndexNode as BinaryExpressionNode).LeftNode;
                    }
                    indexNode = arrayNode.Type;
                }
            }
        }

        /// <summary>
        /// Apply SSA to array indices, while preserving the original dimension.
        ///    
        /// Given:
        ///     a[b][c][d] = 1  
        /// Emit:
        ///     t0 = b
        ///     t1 = c
        ///     t2 = d
        ///     a[t0][t1][t3] = 1
        /// 
        /// The function will perform the following
        ///     1. Get the array index list from the node 
        ///     2. For each index:  Apply SSA transform 
        ///     3.                  Replace the original index with the SSA temp. 
        ///                         This replaces the original dimensions in the indexed array  
        ///     
        ///     
        /// </summary>
        /// <param name="node"></param>
        /// <param name="ssaStack"></param>
        /// <param name="astlist"></param>
        /// <param name="isSSAPointerAssignment"></param>
        private void EmitSSAArrayIndexRetainDimension(
            ref AssociativeNode node, 
            ref List<AssociativeNode> astlist, 
            bool isSSAPointerAssignment = false)
        {
            IdentifierNode identNode = null;
            ArrayNode arrayDimensions = null;
            if (node is IdentifierNode)
            {
                // Retrieve arrayindex data structure (Arraynode) for an identifier
                // a[b][c] -> [b][c]

                identNode = node as IdentifierNode;
                arrayDimensions = identNode.ArrayDimensions;
            }
            else if (node is FunctionCallNode)
            {
                // Retrieve arrayindex data structure (Arraynode) for an identifier
                // a()[b][c] -> [b][c]

                FunctionCallNode fcall = node as FunctionCallNode;
                Validity.Assert(fcall.Function is IdentifierNode);
                identNode = fcall.Function as IdentifierNode;

                // Get the array dimensions of this node
                arrayDimensions = identNode.ArrayDimensions;
            }
            else if (node is IdentifierListNode)
            {
                // Retrieve arrayindex data structure (Arraynode) for an identifier
                // x.y[b][c] -> [b][c]

                // Apply array indexing to the right node of the ident list
                //      a = p.x[i] -> apply it to x
                IdentifierListNode identList = node as IdentifierListNode;
                AssociativeNode rhsNode = identList.RightNode;

                if (rhsNode is IdentifierNode)
                {
                    identNode = rhsNode as IdentifierNode;

                    // Get the array dimensions of this node
                    arrayDimensions = identNode.ArrayDimensions;
                }
                else if (rhsNode is FunctionCallNode)
                {
                    FunctionCallNode fcall = rhsNode as FunctionCallNode;
                    identNode = fcall.Function as IdentifierNode;

                    // Get the array dimensions of this node
                    arrayDimensions = identNode.ArrayDimensions;
                }
                else
                {
                    Validity.Assert(false, "This token is not indexable");
                }
            }
            else if (node is GroupExpressionNode)
            {
                // Retrieve arrayindex data structure (Arraynode) for an identifier
                // (a + b)[b][c] -> [b][c]

                GroupExpressionNode groupExpr = node as GroupExpressionNode;
                arrayDimensions = groupExpr.ArrayDimensions;
            }
            else
            {
                Validity.Assert(false);
            }
            
            EmitSSAforArrayDimension(ref arrayDimensions, ref astlist);
        }

        /// <summary>
        /// This helper function extracts the replication guide data from the 
        /// AST node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private List<AssociativeNode> GetReplicationGuides(AssociativeNode node)
        {
            if (node is ArrayNameNode)
            {
                var nodeWithReplication = node as ArrayNameNode;
                return nodeWithReplication.ReplicationGuides;
            }
            else if (node is IdentifierListNode)
            {
                var identListNode = node as IdentifierListNode;
                return GetReplicationGuides(identListNode.RightNode);
            }
            else if (node is FunctionDotCallNode)
            {
                var dotCallNode = node as FunctionDotCallNode;
                return GetReplicationGuides(dotCallNode.FunctionCall.Function);
            }

            return null;
        }

        // Remove replication guides
        private void RemoveReplicationGuides(AssociativeNode node)
        {
            if (node is ArrayNameNode)
            {
                var nodeWithReplication = node as ArrayNameNode;
                nodeWithReplication.ReplicationGuides = new List<AssociativeNode>();
            }
            else if (node is IdentifierListNode)
            {
                var identListNode = node as IdentifierListNode;
                RemoveReplicationGuides(identListNode.RightNode);
            }
            else if (node is FunctionDotCallNode)
            {
                var dotCallNode = node as FunctionDotCallNode;
                RemoveReplicationGuides(dotCallNode.FunctionCall.Function);
            }
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

        /// <summary>
        /// Handle ssa transforms for LHS identifiers
        /// </summary>
        /// <param name="node"></param>
        /// <param name="ssaStack"></param>
        /// <param name="astlist"></param>
        private void EmitSSALHS(ref AssociativeNode node, Stack<AssociativeNode> ssaStack, ref List<AssociativeNode> astlist)
        {
            if (node is IdentifierNode && null != (node as IdentifierNode).ArrayDimensions)
            {
                // Handle LHS ssa for indexed identifiers
                EmitSSAArrayIndexRetainDimension(ref node, ref astlist);
            }

            // Handle other LHS cases here
            else if (node is IdentifierListNode)
            {
                // TODO Handle LHS ssa for identifier lists

                // For LHS idenlist, resolve them to the fully qualified name
                IdentifierListNode identList = node as IdentifierListNode;
                string[] classNames = ProtoCore.Utils.CoreUtils.GetResolvedClassName(core.ClassTable, identList);
                if (classNames.Length == 1)
                {
                    identList.LeftNode.Name = classNames[0];
                    (identList.LeftNode as IdentifierNode).Value = classNames[0];
                }
            }
        }

        private void DFSEmitSSA_AST(AssociativeNode node, Stack<AssociativeNode> ssaStack, ref List<AssociativeNode> astlist)
        {
            Validity.Assert(null != astlist && null != ssaStack);
            if (node is BinaryExpressionNode)
            {
                BinaryExpressionNode astBNode = node as BinaryExpressionNode;
                AssociativeNode leftNode = null;
                AssociativeNode rightNode = null;
                bool isSSAAssignment = false;
                if (ProtoCore.DSASM.Operator.assign == astBNode.Optr)
                {
                    leftNode = astBNode.LeftNode;
                    DFSEmitSSA_AST(astBNode.RightNode, ssaStack, ref astlist);
                    AssociativeNode assocNode = ssaStack.Pop();

                    if (assocNode is BinaryExpressionNode)
                    {
                        rightNode = (assocNode as BinaryExpressionNode).LeftNode;
                    }
                    else
                    {
                        rightNode = assocNode;
                    }
                    isSSAAssignment = false;

                    // Handle SSA if the lhs is not an identifier
                    // A non-identifier LHS is any LHS that is not just an identifier name.
                    //  i.e. a[0], a.b, f(), f(x)
                    if (!ProtoCore.ASTCompilerUtils.IsSingleIdentifier(leftNode))
                    {
                        EmitSSALHS(ref leftNode, ssaStack, ref astlist);
                    }
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


                    // Left node
                    var identNode = nodeBuilder.BuildIdentfier(ProtoCore.Utils.CoreUtils.BuildSSATemp(core));
                    leftNode = identNode;

                    // Right node
                    rightNode = tnode;

                    isSSAAssignment = true;
                }

                BinaryExpressionNode bnode = new BinaryExpressionNode(leftNode, rightNode, ProtoCore.DSASM.Operator.assign);
                bnode.isSSAAssignment = isSSAAssignment;
                bnode.IsInputExpression = astBNode.IsInputExpression;

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

                if (core.Options.GenerateSSA)
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
                            (argBinaryExpr.LeftNode as IdentifierNode).ReplicationGuides = GetReplicationGuides(arg);
                            
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
                if (null != fcNode)
                {
                    (identNode as IdentifierNode).ReplicationGuides = GetReplicationGuides(fcNode);
                }

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
                        identList.IsLastSSAIdentListFactor = true;
                        break;
                    }
                }
                
                // Assign the first ssa assignment flag
                BinaryExpressionNode firstBNode = astlist[0] as BinaryExpressionNode;
                Validity.Assert(null != firstBNode);
                Validity.Assert(firstBNode.Optr == Operator.assign);
                firstBNode.isSSAFirstAssignment = true;


                //
                // Get the first pointer
                // The first pointer is the lhs of the next dotcall
                //
                // Given:     
                //      a = x.y.z
                // SSA'd to:     
                //      t0 = x      -> 'x' is the lhs of the first dotcall (x.y)
                //      t1 = t0.y
                //      t2 = t1.z
                //      a = t2

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

                //=========================================================
                //
                // 1. Backtrack and convert all identlist nodes to dot calls
                //    This can potentially be optimized by performing the dotcall transform in SSAIdentList
                //
                // 2. Associate the first pointer with each SSA temp
                //
                //=========================================================
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
                            dotCall.isLastSSAIdentListFactor = identList.IsLastSSAIdentListFactor;
                            bnode.RightNode = dotCall;
                            ProtoCore.Utils.CoreUtils.CopyDebugData(bnode, lhsIdent);

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
                            dotCall.isLastSSAIdentListFactor = identList.IsLastSSAIdentListFactor;
                            bnode.RightNode = dotCall;

                            ProtoCore.Utils.CoreUtils.CopyDebugData(bnode, lhsIdent);

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

                DFSEmitSSA_AST(ilnode.TrueExpression, ssaStack, ref inlineExpressionASTList);
                cexpr = ssaStack.Pop();
                ilnode.TrueExpression = cexpr is BinaryExpressionNode ? (cexpr as BinaryExpressionNode).LeftNode : cexpr;
                astlist.AddRange(inlineExpressionASTList);
                inlineExpressionASTList.Clear();

                DFSEmitSSA_AST(ilnode.FalseExpression, ssaStack, ref inlineExpressionASTList);
                cexpr = ssaStack.Pop();
                ilnode.FalseExpression = cexpr is BinaryExpressionNode ? (cexpr as BinaryExpressionNode).LeftNode : cexpr;
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
                if (core.Options.GenerateSSA)
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

            // We allow a null to be generated an SSA variable
            // TODO Jun: Generalize this into genrating SSA temps for all literals
            else if (node is NullNode)
            {
                if (core.Options.GenerateSSA)
                {
                    string ssaTempName = string.Empty;
                    
                    BinaryExpressionNode bnode = new BinaryExpressionNode();
                    bnode.Optr = ProtoCore.DSASM.Operator.assign;

                    // Left node
                    ssaTempName = ProtoCore.Utils.CoreUtils.BuildSSATemp(core);
                    var identNode = nodeBuilder.BuildIdentfier(ssaTempName);
                    bnode.LeftNode = identNode;

                    // Right node
                    bnode.RightNode = node;

                    bnode.isSSAAssignment = true;

                    astlist.Add(bnode);
                    ssaStack.Push(bnode);
                }
                else
                {
                    BinaryExpressionNode bnode = new BinaryExpressionNode();
                    bnode.Optr = ProtoCore.DSASM.Operator.assign;

                    // Left node
                    var identNode = nodeBuilder.BuildIdentfier(ProtoCore.Utils.CoreUtils.BuildSSATemp(core));
                    bnode.LeftNode = identNode;

                    // Right node
                    bnode.RightNode = node;


                    bnode.isSSAAssignment = true;

                    astlist.Add(bnode);
                    ssaStack.Push(bnode);
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

                int ssaExprID = 0;
                foreach (AssociativeNode node in astList)
                {
                    List<AssociativeNode> newASTList = new List<AssociativeNode>();
                    if (node is BinaryExpressionNode)
                    {
                        BinaryExpressionNode bnode = (node as BinaryExpressionNode);
                        int generatedUID = ProtoCore.DSASM.Constants.kInvalidIndex;

                        if (context.applySSATransform && core.Options.GenerateSSA)
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
                                ssaNode.ssaExprID = ssaExprID;
                                ssaNode.ssaExpressionUID = core.SSAExpressionUID;
                                ssaNode.guid = bnode.guid;
                                ssaNode.OriginalAstID = bnode.OriginalAstID;
                                ssaNode.IsModifier = node.IsModifier;
                                NodeUtils.SetNodeLocation(ssaNode, node, node);
                            }

                            // Assigne the exprID of the original node 
                            // (This is the node prior to ssa transformation)
                            bnode.exprUID = ssaID;
                            bnode.ssaExprID = ssaExprID;
                            bnode.ssaExpressionUID = core.SSAExpressionUID;
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

                                    cNode.FunctionBody.Body = ApplyTransform(cNode.FunctionBody.Body);
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

                        if (core.Options.GenerateSSA)
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
                    ssaExprID++;
                    core.SSAExpressionUID++;
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

        /// <summary>
        /// Converts lhs ident lists to a function call
        /// a.x = 10 
        ///     -> t = a.%set_x(10)
        ///     
        /// a.x.y = b + c 
        ///     -> a.x.%set_y(b + c)
        ///     
        /// a.x[0] = 10
        ///     ->  tval = a.%get_x()
        ///         tval[0] = 10         
        ///         tmp = a.%set_x(tval)
        /// </summary>
        /// <param name="astList"></param>
        /// <returns></returns>
        private List<AssociativeNode> TransformLHSIdentList(List<AssociativeNode> astList)
        {
            List<AssociativeNode> newAstList = new List<AssociativeNode>();
            foreach (AssociativeNode node in astList)
            {
                BinaryExpressionNode bNode = node as BinaryExpressionNode;
                if (bNode == null)
                {
                    newAstList.Add(node);
                }
                else
                {
                    bool isLHSIdentList = bNode.LeftNode is IdentifierListNode;
                    if (!isLHSIdentList)
                    {
                        newAstList.Add(node);
                    }
                    else
                    {
                        IdentifierNode lhsTemp = new IdentifierNode(Constants.kTempVar);

                        IdentifierListNode identList = bNode.LeftNode as IdentifierListNode;
                        Validity.Assert(identList != null);

                        AssociativeNode argument = bNode.RightNode;

                        IdentifierNode identFunctionCall = identList.RightNode as IdentifierNode;
                        string setterName = ProtoCore.DSASM.Constants.kSetterPrefix + identList.RightNode.Name;
                        bool isArrayIndexed = identFunctionCall.ArrayDimensions != null;
                        if (isArrayIndexed)
                        {
                            // a.x[0] = 10
                            //      tval = a.%get_x()
                            string getterName = ProtoCore.DSASM.Constants.kGetterPrefix + identList.RightNode.Name;
                            ProtoCore.AST.AssociativeAST.FunctionCallNode fcall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
                            fcall.Function = new IdentifierNode(identList.RightNode.Name);
                            fcall.Function.Name = getterName;

                            IdentifierListNode identList1 = new IdentifierListNode();
                            identList1.LeftNode = identList.LeftNode;
                            identList1.RightNode = fcall;
                            BinaryExpressionNode bnodeGet = new BinaryExpressionNode(
                                lhsTemp,
                                identList1, 
                                Operator.assign
                                );
                            newAstList.Add(bnodeGet);

                            //      tval[0] = 10     
                            IdentifierNode lhsTempIndexed = new IdentifierNode(Constants.kTempVar);
                            lhsTempIndexed.ArrayDimensions = identFunctionCall.ArrayDimensions;
                            BinaryExpressionNode bnodeAssign = new BinaryExpressionNode(
                                lhsTempIndexed,
                                argument,
                                Operator.assign
                                );
                            newAstList.Add(bnodeAssign);

                            //      tmp = a.%set_x(tval)
                            ProtoCore.AST.AssociativeAST.FunctionCallNode fcallSet = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
                            fcallSet.Function = identFunctionCall;
                            fcallSet.Function.Name = setterName;
                            List<AssociativeNode> args = new List<AssociativeNode>();
                            IdentifierNode lhsTempAssignBack = new IdentifierNode(Constants.kTempVar);
                            args.Add(lhsTempAssignBack);
                            fcallSet.FormalArguments = args;

                            IdentifierListNode identList2 = new IdentifierListNode();
                            identList2.LeftNode = identList.LeftNode;
                            identList2.RightNode = fcallSet;

                            IdentifierNode lhsTempAssign = new IdentifierNode(Constants.kTempPropertyVar);

                            BinaryExpressionNode bnodeSet = new BinaryExpressionNode(
                                lhsTempAssign,
                                identList2,
                                Operator.assign
                                );
                            newAstList.Add(bnodeSet);
                        }
                        else
                        {
                            List<AssociativeNode> args = new List<AssociativeNode>();
                            args.Add(argument);

                            ProtoCore.AST.AssociativeAST.FunctionCallNode fcall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
                            fcall.Function = identFunctionCall;
                            fcall.Function.Name = setterName;
                            fcall.FormalArguments = args;

                            identList.RightNode = fcall;
                            BinaryExpressionNode convertedAssignNode = new BinaryExpressionNode(lhsTemp, identList, Operator.assign);

                            NodeUtils.CopyNodeLocation(convertedAssignNode, bNode);
                            newAstList.Add(convertedAssignNode);
                        }
                    }
                }
            }
            return newAstList;
        }
      
        private List<AssociativeNode> SplitMulitpleAssignment(List<AssociativeNode> astList)
        {
            bool validAst = astList != null && astList.Count > 0;
            if (!validAst)
            {
                return astList;
            }

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

        private List<AssociativeNode> ApplyTransform(List<AssociativeNode> astList)
        {
            bool validAst = astList != null && astList.Count > 0;
            if (validAst)
            {
                astList = SplitMulitpleAssignment(astList);
                astList = TransformLHSIdentList(astList);
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

        public List<AssociativeNode> EmitSSA(List<AssociativeNode> astList)
        {
            Validity.Assert(null != astList);
            astList = ApplyTransform(astList);
            return BuildSSA(astList, new ProtoCore.CompileTime.Context());
        }

        private int EmitExpressionInterpreter(ProtoCore.AST.Node codeBlockNode)
        {
            core.watchStartPC = this.pc;
            compilePass = ProtoCore.CompilerDefinitions.Associative.CompilePass.kGlobalScope;
            ProtoCore.AST.AssociativeAST.CodeBlockNode codeblock = codeBlockNode as ProtoCore.AST.AssociativeAST.CodeBlockNode;

            ProtoCore.Type inferedType = new ProtoCore.Type();

            globalClassIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
            globalProcIndex = ProtoCore.DSASM.Constants.kGlobalScope;

            // check if currently inside a function when the break was called
            if (context.DebugProps.DebugStackFrameContains(DebugProperties.StackFrameFlagOptions.FepRun))
            {
                // Save the current scope for the expression interpreter
                globalClassIndex = context.WatchClassScope = context.MemoryState.CurrentStackFrame.ClassScope;
                globalProcIndex = core.watchFunctionScope = context.MemoryState.CurrentStackFrame.FunctionScope;
                int functionBlock = context.MemoryState.CurrentStackFrame.FunctionBlock;

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
                inferedType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0);

                DfsTraverse(node, ref inferedType, false, null, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier);

                BinaryExpressionNode binaryNode = node as BinaryExpressionNode;
            }

            core.InferedType = inferedType;

            this.pc = core.watchStartPC;

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
            if (compilePass == ProtoCore.CompilerDefinitions.Associative.CompilePass.kGlobalFuncBody)
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

            core.watchStartPC = this.pc;
            if (core.Options.RunMode == ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter)
            {
                return EmitExpressionInterpreter(codeBlockNode);
            }

            this.localCodeBlockNode = codeBlockNode;
            ProtoCore.AST.AssociativeAST.CodeBlockNode codeblock = codeBlockNode as ProtoCore.AST.AssociativeAST.CodeBlockNode;
            bool isTopBlock = null == codeBlock.parent;
            if (!isTopBlock)
            {
                // If this is an inner block where there can be no classes, we can start at parsing at the global function state
                compilePass = ProtoCore.CompilerDefinitions.Associative.CompilePass.kGlobalFuncSig;
            }

            codeblock.Body = ApplyTransform(codeblock.Body);
            
            bool hasReturnStatement = false;
            ProtoCore.Type inferedType = new ProtoCore.Type();
            bool ssaTransformed = false;
            while (ProtoCore.CompilerDefinitions.Associative.CompilePass.kDone != compilePass)
            {
                // Emit SSA only after generating the class definitions
                if (core.Options.GenerateSSA)
                {
                    if (compilePass > ProtoCore.CompilerDefinitions.Associative.CompilePass.kClassName && !ssaTransformed)
                    {
                        if (!core.IsParsingCodeBlockNode && !core.Options.IsDeltaExecution)
                        {
                            //Audit class table for multiple symbol definition and emit warning.
                            this.core.ClassTable.AuditMultipleDefinition(this.core.BuildStatus, graphNode);
                        }
                        codeblock.Body = BuildSSA(codeblock.Body, context);              
                        ssaTransformed = true;
                        if (core.Options.DumpIL)
                        {
                            CodeGenDS codegenDS = new CodeGenDS(codeblock.Body);
                            EmitCompileLog(codegenDS.GenerateCode());
                        }
                    }
                }

                inferedType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0);
                hasReturnStatement = EmitCodeBlock(codeblock.Body, ref inferedType, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier, false);
                if (compilePass == ProtoCore.CompilerDefinitions.Associative.CompilePass.kGlobalScope && !hasReturnStatement)
                {
                    EmitReturnNull(); 
                }

                compilePass++;

                // We have compiled all classes
                if (compilePass == ProtoCore.CompilerDefinitions.Associative.CompilePass.kGlobalScope && isTopBlock)
                {
                    EmitFunctionCallToInitStaticProperty(codeblock.Body);
                }

            }

            ResolveFinalNodeRefs();
            ResolveSSADependencies();
            
            ProtoCore.AssociativeEngine.Utils.BuildGraphNodeDependencies(
                codeBlock.instrStream.dependencyGraph.GetGraphNodesAtScope(Constants.kInvalidIndex, Constants.kGlobalScope));
        
            
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

            // Reset the callsite guids in preparation for the next compilation
            core.CallsiteGuidMap = new Dictionary<Guid, int>();

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

        public int AllocateMemberVariable(int classIndex, int classScope, string name, int rank = 0, ProtoCore.CompilerDefinitions.AccessModifier access = ProtoCore.CompilerDefinitions.AccessModifier.kPublic, bool isStatic = false)
        {
            // TODO Jun: Create a class table for holding the primitive and custom data types
            int datasize = ProtoCore.DSASM.Constants.kPointerSize;
            ProtoCore.Type ptrType = new ProtoCore.Type();
            if (rank == 0)
                ptrType.UID = (int)PrimitiveType.kTypePointer;
            else
                ptrType.UID = (int)PrimitiveType.kTypeArray;
            ptrType.rank = rank;
            ProtoCore.DSASM.SymbolNode symnode = Allocate(classIndex, classScope, ProtoCore.DSASM.Constants.kGlobalScope, name, ptrType, datasize, isStatic, access);
            if (null == symnode)
            {
                buildStatus.LogSemanticError(String.Format(Resources.MemberVariableAlreadyDefined, name, core.ClassTable.ClassNodes[classIndex].name));
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

        private void EmitIdentifierNode(AssociativeNode node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, BinaryExpressionNode parentNode = null)
        {
            IdentifierNode t = node as IdentifierNode;
            if (t.Name.Equals(ProtoCore.DSDefinitions.Keyword.This))
            {
                if (subPass != ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone)
                {
                    return;
                }

                if (localProcedure != null)
                {
                    if (localProcedure.isStatic)
                    {
                        string message = ProtoCore.Properties.Resources.kUsingThisInStaticFunction;
                        core.BuildStatus.LogWarning(ProtoCore.BuildData.WarningID.kInvalidThis, message, core.CurrentDSFileName, t.line, t.col, graphNode);
                        EmitPushNull();
                        return;
                    }
                    else if (localProcedure.classScope == Constants.kGlobalScope)
                    {
                        string message = ProtoCore.Properties.Resources.kInvalidThis;
                        core.BuildStatus.LogWarning(ProtoCore.BuildData.WarningID.kInvalidThis, message, core.CurrentDSFileName, t.line, t.col, graphNode);
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
                    string message = ProtoCore.Properties.Resources.kInvalidThis;
                    core.BuildStatus.LogWarning(ProtoCore.BuildData.WarningID.kInvalidThis, message, core.CurrentDSFileName, t.line, t.col, graphNode);
                    EmitPushNull();
                    return;
                }
            }

            int dimensions = 0;

            ProtoCore.DSASM.SymbolNode symbolnode = null;
            int runtimeIndex = codeBlock.symbolTable.RuntimeIndex;

            ProtoCore.Type type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0);

            bool isAccessible = false;

            if (null == t.ArrayDimensions)
            {
                //check if it is a function instance
                ProtoCore.DSASM.ProcedureNode procNode = null;
                procNode = CoreUtils.GetFirstVisibleProcedure(t.Name, null, codeBlock);
                if (null != procNode)
                {
                    if (ProtoCore.DSASM.Constants.kInvalidIndex != procNode.procId)
                    {
                        // A global function
                        inferedType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeFunctionPointer, 0);
                        if (ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier != subPass)
                        {
                            int fptr = core.FunctionPointerTable.functionPointerDictionary.Count;
                            var fptrNode = new FunctionPointerNode(procNode);
                            core.FunctionPointerTable.functionPointerDictionary.TryAdd(fptr, fptrNode);
                            core.FunctionPointerTable.functionPointerDictionary.TryGetBySecond(fptrNode, out fptr);

                            EmitPushVarData(0);

                            EmitInstrConsole(ProtoCore.DSASM.kw.push, t.Name);
                            StackValue opFunctionPointer = StackValue.BuildFunctionPointer(fptr);
                            EmitPush(opFunctionPointer, runtimeIndex, t.line, t.col);
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

            if (ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier == subPass)
            {
                if (symbolnode == null)
                {
                    // The variable is unbound
                    ProtoCore.DSASM.SymbolNode unboundVariable = null;
                    if (ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter != core.Options.RunMode)
                    {
                        inferedType.UID = (int)ProtoCore.PrimitiveType.kTypeNull;

                        // Jun Comment: Specification 
                        //      If resolution fails at this point a com.Design-Script.Imperative.Core.UnboundIdentifier 
                        //      warning is emitted during pre-execute phase, and at the ID is bound to null. (R1 - Feb)

                        int startpc = pc;

                        // Set the first symbol that triggers the cycle to null
                        ProtoCore.AssociativeGraph.GraphNode nullAssignGraphNode = new ProtoCore.AssociativeGraph.GraphNode();
                        nullAssignGraphNode.updateBlock.startpc = pc;


                        EmitPushNull();

                        // Push the identifier local block  
                        dimensions = 0;
                        EmitPushVarData(dimensions);
                        ProtoCore.Type varType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0);

                        // TODO Jun: Refactor Allocate() to just return the symbol node itself
                        unboundVariable = Allocate(globalClassIndex, globalClassIndex, globalProcIndex, t.Value, varType, ProtoCore.DSASM.Constants.kPrimitiveSize,
                            false, ProtoCore.CompilerDefinitions.AccessModifier.kPublic, ProtoCore.DSASM.MemoryRegion.kMemStack, t.line, t.col, graphNode);
                        Validity.Assert(unboundVariable != null);

                        int symbolindex = unboundVariable.symbolTableIndex;
                        if (ProtoCore.DSASM.Constants.kInvalidIndex != globalClassIndex)
                        {
                            symbolnode = core.ClassTable.ClassNodes[globalClassIndex].symbols.symbolList[symbolindex];
                        }
                        else
                        {
                            symbolnode = codeBlock.symbolTable.symbolList[symbolindex];
                        }

                        EmitInstrConsole(ProtoCore.DSASM.kw.pop, t.Value);
                        EmitPopForSymbol(unboundVariable, runtimeIndex);


                        nullAssignGraphNode.PushSymbolReference(symbolnode);
                        nullAssignGraphNode.procIndex = globalProcIndex;
                        nullAssignGraphNode.classIndex = globalClassIndex;
                        nullAssignGraphNode.updateBlock.endpc = pc - 1;

                        PushGraphNode(nullAssignGraphNode);
                        EmitDependency(ProtoCore.DSASM.Constants.kInvalidIndex, ProtoCore.DSASM.Constants.kInvalidIndex, false);


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

                    if (isAllocated)
                    {
                        string message = String.Format(ProtoCore.Properties.Resources.kPropertyIsInaccessible, t.Value);
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
                                message = String.Format(ProtoCore.Properties.Resources.kUsingNonStaticMemberInStaticContext, t.Value);
                            }
                        }
                        buildStatus.LogWarning(
                            WarningID.kAccessViolation,
                            message,
                            core.CurrentDSFileName,
                            t.line,
                            t.col,
                            graphNode);
                    }
                    else
                    {
                        string message = String.Format(ProtoCore.Properties.Resources.kUnboundIdentifierMsg, t.Value);
                        buildStatus.LogUnboundVariableWarning(unboundVariable, message, core.CurrentDSFileName, t.line, t.col, graphNode);
                    }
                }

                if (null != t.ArrayDimensions)
                {
                    dimensions = DfsEmitArrayIndexHeap(t.ArrayDimensions, graphNode, parentNode, subPass);
                }
            }
            else
            {
                if (core.Options.RunMode == ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter &&
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
                    UpdateNode updateNode = new UpdateNode();
                    updateNode.symbol = symbolnode;
                    updateNode.nodeType = UpdateNodeType.kSymbol;

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

                            if (core.Options.GenerateSSA)
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
                        EmitIdentifierListNode(identListNode, ref inferedType, false, graphNode, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone);

                        if (null != graphNode)
                        {
                            PushSymbolAsDependent(symbolnode, graphNode);
                        }

                        return;
                    }
                }

                type = symbolnode.datatype;
                runtimeIndex = symbolnode.runtimeTableIndex;

                // The graph node always depends on this identifier
                GraphNode dependendNode = null;
                if (null != graphNode)
                {
                    dependendNode = PushSymbolAsDependent(symbolnode, graphNode);
                }


                // Flags if the variable is allocated in the current scope
                bool isAllocatedWithinCurrentBlock = symbolnode.codeBlockId == codeBlock.codeBlockId;
                bool isValidDependent = dependendNode != null && !CoreUtils.IsSSATemp(symbolnode.name);
                if (!isAllocatedWithinCurrentBlock && isValidDependent)
                {
                    /*
                    a = 1;
                    i = [Associative]
                    {
                        // In an inner language block:
                        //  Add all rhs to the parent graphnode dependency list if the rhs was not allocated at the current scope
                        //  Only those not allocated are added to the parent.
                        //  This is because external nodes should not be able to update the local block if the affected variable is local to the block
                        //  Variable 'x' is local and the outer block statement x = 2 (where x is allocated globally), should not update this inner block
                        x = 10;
                        b = x + 1;
                        return = a + b + 10; 
                    }
                    x = 2;
                    a = 2;
                    */
                    context.DependentVariablesInScope.Add(dependendNode);
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

                EmitPushVarData(dimensions);

                if (ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter == core.Options.RunMode)
                {
                    EmitInstrConsole(ProtoCore.DSASM.kw.pushw, t.Value);
                    EmitPushForSymbolW(symbolnode, runtimeIndex, t.line, t.col);
                }
                else
                {
                    EmitInstrConsole(ProtoCore.DSASM.kw.push, t.Value);
                    EmitPushForSymbol(symbolnode, runtimeIndex, t);

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
            bool parseGlobal = null == localProcedure && ProtoCore.CompilerDefinitions.Associative.CompilePass.kAll == compilePass;
            bool parseGlobalFunction = null != localProcedure && ProtoCore.CompilerDefinitions.Associative.CompilePass.kGlobalFuncBody == compilePass;
            bool parseMemberFunction = ProtoCore.DSASM.Constants.kGlobalScope != classIndex && ProtoCore.CompilerDefinitions.Associative.CompilePass.kClassMemFuncBody == compilePass;

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
        private void EmitRangeExprNode(AssociativeNode node, 
                                       ref ProtoCore.Type inferedType, 
                                       GraphNode graphNode = null,
                                       ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone)
        {
            RangeExprNode range = node as RangeExprNode;

            // Do some static checking...
            var fromNode = range.FromNode;
            var toNode = range.ToNode;
            var stepNode = range.StepNode;
            var stepOp = range.stepoperator;
            var hasAmountOperator = range.HasRangeAmountOperator;

            bool isStepValid = true;
            string warningMsg = string.Empty;

            if ((fromNode is IntNode || fromNode is DoubleNode) &&
                (toNode is IntNode || toNode is DoubleNode) &&
                (stepNode == null || stepNode is IntNode || stepNode is DoubleNode))
            {
                double current = (fromNode is IntNode) ?
                    (fromNode as IntNode).Value : (fromNode as DoubleNode).Value;
                double end = (toNode is IntNode) ?
                    (toNode as IntNode).Value : (toNode as DoubleNode).Value;

                double step = 1;
                if (stepNode != null)
                {
                    step = (stepNode is IntNode) ?
                        (stepNode as IntNode).Value : (stepNode as DoubleNode).Value;
                }

                switch (stepOp)
                {
                    case ProtoCore.DSASM.RangeStepOperator.stepsize:

                        if (!hasAmountOperator)
                        {
                            if (stepNode == null && end < current)
                            {
                                step = -1;
                            }

                            if (step == 0)
                            {
                                isStepValid = false;
                                warningMsg = ProtoCore.Properties.Resources.kRangeExpressionWithStepSizeZero;
                            }
                            else if ((end > current && step < 0) || (end < current && step > 0))
                            {
                                isStepValid = false;
                                warningMsg = ProtoCore.Properties.Resources.kRangeExpressionWithInvalidStepSize;
                            }
                        }

                       break;

                    case ProtoCore.DSASM.RangeStepOperator.num:

                       if (hasAmountOperator)
                       {
                           isStepValid = false;
                           warningMsg = ProtoCore.Properties.Resources.kRangeExpressionWithStepSizeZero;
                       }
                       else
                       {
                           if (stepNode != null && stepNode is DoubleNode &&
                               subPass == ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone)
                           {
                               buildStatus.LogWarning(WarningID.kInvalidRangeExpression,
                                                      ProtoCore.Properties.Resources.kRangeExpressionWithNonIntegerStepNumber,
                                                      core.CurrentDSFileName,
                                                      stepNode.line,
                                                      stepNode.col, 
                                                      graphNode);
                           }
                           else if (step <= 0)
                           {
                               isStepValid = false;
                               warningMsg = ProtoCore.Properties.Resources.kRangeExpressionWithNegativeStepNumber;
                           }
                       }

                       break;

                    case ProtoCore.DSASM.RangeStepOperator.approxsize:
                        if (hasAmountOperator)
                        {
                            isStepValid = false;
                            warningMsg = ProtoCore.Properties.Resources.kRangeExpressionConflictOperator;
                        }
                        else if (step == 0)
                        {
                            isStepValid = false;
                            warningMsg = ProtoCore.Properties.Resources.kRangeExpressionWithStepSizeZero;
                        }
                        break;

                    default:
                        break;
                }
            }

            if (!isStepValid && subPass == ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone)
            {
                buildStatus.LogWarning(WarningID.kInvalidRangeExpression,
                                       warningMsg,
                                       core.CurrentDSFileName,
                                       stepNode.line,
                                       stepNode.col, 
                                       graphNode);

                EmitNullNode(new NullNode(), ref inferedType);
                return;
            }

            // Replace with build-in RangeExpression() function. - Yu Ke
            bool emitReplicationgGuideState = emitReplicationGuide;
            emitReplicationGuide = false;

            IntNode op = null;
            switch (stepOp)
            {
                case RangeStepOperator.stepsize:
                    op = new IntNode(0);
                    break;
                case RangeStepOperator.num:
                    op = new IntNode(1);
                    break;
                case RangeStepOperator.approxsize:
                    op = new IntNode(2);
                    break;
                default:
                    op = new IntNode(-1);
                    break;
            }
            var arguments = new List<AssociativeNode> 
            {
                fromNode, 
                toNode, 
                stepNode ?? new NullNode(), 
                op, 
                AstFactory.BuildBooleanNode(stepNode != null),
                AstFactory.BuildBooleanNode(hasAmountOperator),
            };
            var rangeExprFunc = AstFactory.BuildFunctionCall(Constants.kFunctionRangeExpression, 
                                                             arguments);

            EmitFunctionCallNode(rangeExprFunc, ref inferedType, false, graphNode, subPass);

            emitReplicationGuide = emitReplicationgGuideState;

            if (subPass != ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier)
            {
                if (range.ArrayDimensions != null)
                {
                    int dim = DfsEmitArrayIndexHeap(range.ArrayDimensions, graphNode);
                    EmitInstrConsole(kw.pushindex, dim + "[dim]");
                    EmitPushArrayIndex(dim);
                }

                if (core.Options.TempReplicationGuideEmptyFlag && emitReplicationGuide)
                {
                    int guide = EmitReplicationGuides(range.ReplicationGuides);
                    EmitInstrConsole(kw.pushindex, guide + "[guide]");
                    EmitPushReplicationGuide(guide);
                }
            }
        }

        private void EmitLanguageBlockNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.AssociativeGraph.GraphNode graphNode, ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone)
        {
            if (IsParsingGlobal() || IsParsingGlobalFunctionBody() || IsParsingMemberFunctionBody() )
            {
                if (subPass == ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier)
                {
                    return;
                }

                LanguageBlockNode langblock = node as LanguageBlockNode;

                //Validity.Assert(ProtoCore.Language.kInvalid != langblock.codeblock.language);
                if (ProtoCore.Language.kInvalid == langblock.codeblock.language)
                    throw new BuildHaltException("Invalid language block type (D1B95A65)");

                ProtoCore.CompileTime.Context nextContext = new ProtoCore.CompileTime.Context();

                // Save the guid of the current scope (which is stored in the current graphnodes) to the nested language block.
                // This will be passed on to the nested language block that will be compiled
                nextContext.guid = graphNode.guid;

                int entry = 0;
                int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;

                // Top block signifies the auto inserted global block
                bool isTopBlock = null == codeBlock.parent;

                // The warning is enforced only if this is not the top block
                if (ProtoCore.Language.kAssociative == langblock.codeblock.language && !isTopBlock)
                {
                    // TODO Jun: Move the associative and all common string into some table
                    buildStatus.LogSyntaxError(Resources.InvalidNestedAssociativeBlock, core.CurrentDSFileName, langblock.line, langblock.col);
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

                core.Compilers[langblock.codeblock.language].Compile(out blockId, codeBlock, langblock.codeblock, nextContext, codeBlock.EventSink, langblock.CodeBlockNode, propagateGraphNode);
                graphNode.isLanguageBlock = true;
                graphNode.languageBlockId = blockId;
                foreach (GraphNode dNode in nextContext.DependentVariablesInScope)
                {
                    graphNode.PushDependent(dNode);
                }

                setBlkId(blockId);
                inferedType = core.InferedType;
                //Validity.Assert(codeBlock.children[codeBlock.children.Count - 1].blockType == ProtoCore.DSASM.CodeBlockType.kLanguage);
                codeBlock.children[codeBlock.children.Count - 1].Attributes = PopulateAttributes(langblock.Attributes);

                int startpc = pc;

                EmitInstrConsole(ProtoCore.DSASM.kw.bounce + " " + blockId + ", " + entry.ToString());
                EmitBounceIntrinsic(blockId, entry);


                // The callee language block will have stored its result into the RX register. 
                EmitInstrConsole(ProtoCore.DSASM.kw.push, ProtoCore.DSASM.kw.regRX);
                StackValue opRes = StackValue.BuildRegister(Registers.RX);
                EmitPush(opRes);
            }
        }

        private void EmitDynamicLanguageBlockNode(AssociativeNode node, AssociativeNode singleBody, ref ProtoCore.Type inferedType, ref int blockId, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone)
        {
            if (IsParsingGlobal() || IsParsingGlobalFunctionBody() || IsParsingMemberFunctionBody())
            {
                if (subPass == ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier)
                {
                    return;
                }

                LanguageBlockNode langblock = node as LanguageBlockNode;

                //Validity.Assert(ProtoCore.Language.kInvalid != langblock.codeblock.language);
                if (ProtoCore.Language.kInvalid == langblock.codeblock.language)
                    throw new BuildHaltException("Invalid language block type (B1C57E37)");

                ProtoCore.CompileTime.Context context = new ProtoCore.CompileTime.Context();

                // Set the current class scope so the next language can refer to it
                core.ClassIndex = globalClassIndex;

                if (globalProcIndex != ProtoCore.DSASM.Constants.kInvalidIndex && core.ProcNode == null)
                {
                    if (globalClassIndex != ProtoCore.DSASM.Constants.kInvalidIndex)
                        core.ProcNode = core.ClassTable.ClassNodes[globalClassIndex].vtable.procList[globalProcIndex];
                    else
                        core.ProcNode = codeBlock.procedureTable.procList[globalProcIndex];
                }

                core.Compilers[langblock.codeblock.language].Compile(
                    out blockId, codeBlock, langblock.codeblock, context, codeBlock.EventSink, langblock.CodeBlockNode, null);
                
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
                access = ProtoCore.CompilerDefinitions.AccessModifier.kPublic,
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
                ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeNull, 0),
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
            /*
            ProtoCore.Type varType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0);
            if (!prop.datatype.Equals(varType))
            {
                var setterForVar = EmitSetterFunction(prop, varType);
                cnode.funclist.Add(setterForVar);
            }
            */

            ProtoCore.Type varArrayType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank);
            // if (!prop.datatype.Equals(varArrayType))
            {
                var setterForVarArray = EmitSetterFunction(prop, varArrayType);
                cnode.funclist.Add(setterForVarArray);
            }
        }

        private void EmitMemberVariables(ClassDeclNode classDecl, GraphNode graphNode = null)
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
                            EmitMemberVariables(unPopulatedClasses[baseClassIndex], graphNode);
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
                            case "double": bNode.RightNode = new DoubleNode(0); break;
                            case "int": bNode.RightNode = new IntNode(0); break;
                            case "bool": bNode.RightNode = new BooleanNode(false); break;
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
                    ProtoCore.Type propType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0);
                    string typeName = vardecl.ArgumentType.Name;
                    if (String.IsNullOrEmpty(typeName))
                    {
                        prop.datatype = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank);
                    }
                    else
                    {
                        int type = core.TypeSystem.GetType(typeName);
                        if (type == (int)PrimitiveType.kInvalidType)
                        {
                            string message = String.Format(ProtoCore.Properties.Resources.kTypeUndefined, typeName);
                            core.BuildStatus.LogWarning(ProtoCore.BuildData.WarningID.kTypeUndefined, message, core.CurrentDSFileName, vardecl.line, vardecl.col, graphNode);
                            prop.datatype = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0);
                        }
                        else
                        {
                            int rank = vardecl.ArgumentType.rank;
                            prop.datatype = core.TypeSystem.BuildTypeObject(type, rank);
                            if (type != (int)PrimitiveType.kTypeVar || prop.datatype.IsIndexable)
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
                    access = ProtoCore.CompilerDefinitions.AccessModifier.kPublic,
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

        private void EmitClassDeclNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone,
            GraphNode graphNode = null)
        {
            ClassDeclNode classDecl = node as ClassDeclNode;

            // Handling n-pass on class declaration
            if (ProtoCore.CompilerDefinitions.Associative.CompilePass.kClassName == compilePass)
            {
                // Class name pass
                // Populating the class tables with the class names
                if (null != codeBlock.parent)
                {
                    buildStatus.LogSemanticError(Resources.ClassCannotBeDefinedInsideLanguageBlock, core.CurrentDSFileName, classDecl.line, classDecl.col);
                }


                ProtoCore.DSASM.ClassNode thisClass = new ProtoCore.DSASM.ClassNode();
                thisClass.name = classDecl.className;
                thisClass.size = classDecl.varlist.Count;
                thisClass.IsImportedClass = classDecl.IsImportedClass;
                thisClass.typeSystem = core.TypeSystem;
                thisClass.ClassAttributes = classDecl.ClassAttributes;
                
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
            else if (ProtoCore.CompilerDefinitions.Associative.CompilePass.kClassBaseClass == compilePass)
            { 
                // Base class pass
                // Populating each class entry with their immediate baseclass
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
            else if (ProtoCore.CompilerDefinitions.Associative.CompilePass.kClassHierarchy == compilePass)
            {
                // Class hierarchy pass
                // Populating each class entry with all sub classes in the hierarchy
                globalClassIndex = core.ClassTable.GetClassId(classDecl.className);

                ProtoCore.DSASM.ClassNode thisClass = core.ClassTable.ClassNodes[globalClassIndex];

                // Verify and store the list of classes it inherits from 
                if (null != classDecl.superClass)
                {
                    for (int n = 0; n < classDecl.superClass.Count; ++n)
                    {
                        int baseClass = core.ClassTable.GetClassId(classDecl.superClass[n]);

                        // baseClass is already resovled in the previous pass
                        Validity.Assert(ProtoCore.DSASM.Constants.kInvalidIndex != baseClass);

                            
                        // Iterate through all the base classes until the the root class
                        // For every base class, add the coercion score
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
                }
            }
            else if (ProtoCore.CompilerDefinitions.Associative.CompilePass.kClassMemVar == compilePass)
            {
                EmitMemberVariables(classDecl, graphNode);
            }
            else if (ProtoCore.CompilerDefinitions.Associative.CompilePass.kClassMemFuncSig == compilePass)
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
                    if (vtable.IndexOfExact(classDecl.className, new List<ProtoCore.Type>(), false) == ProtoCore.DSASM.Constants.kInvalidIndex)
                    {
                        ConstructorDefinitionNode defaultConstructor = new ConstructorDefinitionNode();
                        defaultConstructor.Name = classDecl.className;
                        defaultConstructor.localVars = 0;
                        defaultConstructor.Signature = new ArgumentSignatureNode();
                        defaultConstructor.Pattern = null;
                        defaultConstructor.ReturnType = new ProtoCore.Type { Name = "var", UID = 0 };
                        defaultConstructor.FunctionBody = new CodeBlockNode();
                        defaultConstructor.baseConstr = null;
                        defaultConstructor.access = ProtoCore.CompilerDefinitions.AccessModifier.kPublic;
                        defaultConstructor.IsExternLib = false;
                        defaultConstructor.ExternLibName = null;
                        DfsTraverse(defaultConstructor, ref inferedType);
                        classDecl.funclist.Add(defaultConstructor);
                    }
                }
            }
            else if (ProtoCore.CompilerDefinitions.Associative.CompilePass.kGlobalScope == compilePass)
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
            else if (ProtoCore.CompilerDefinitions.Associative.CompilePass.kClassMemFuncBody == compilePass)
            {
                // Class member variable pass
                // Populating the function body of each member function defined in the class vtables

                globalClassIndex = core.ClassTable.GetClassId(classDecl.className);

                // Initialize the global function table for this class
                // 'classIndexAtCallsite' is the class index as it is stored at the callsite function tables
                int classIndexAtCallsite = globalClassIndex + 1;
                core.FunctionTable.InitGlobalFunctionEntry(classIndexAtCallsite);

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
                    ProtoCore.Type paramType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0);
                    emitReplicationGuide = true;
                    enforceTypeCheck = !(paramNode is BinaryExpressionNode);
                    DfsTraverse(paramNode, ref paramType, false, null, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier);
                    DfsTraverse(paramNode, ref paramType, false, null, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone);
                    emitReplicationGuide = false;
                    enforceTypeCheck = true;
                    argTypeList.Add(paramType);
                }

                List<int> myBases = core.ClassTable.ClassNodes[globalClassIndex].baseList;
                foreach (int bidx in myBases)
                {
                    int cidx = core.ClassTable.ClassNodes[bidx].vtable.IndexOf(baseConstructorName, argTypeList);
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
                    int cidx = core.ClassTable.ClassNodes[bidx].vtable.IndexOf(baseConstructorName, argTypeList);
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

        private void EmitConstructorDefinitionNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, GraphNode gNode = null)
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
                localProcedure.returntype.rank = Constants.kArbitraryRank;
                localProcedure.isConstructor = true;
                localProcedure.runtimeIndex = 0;
                localProcedure.isExternal = funcDef.IsExternLib;
                Validity.Assert(ProtoCore.DSASM.Constants.kInvalidIndex != globalClassIndex, "A constructor node must be associated with class");
                localProcedure.localCount = 0;
                localProcedure.classScope = globalClassIndex;

                localProcedure.MethodAttribute = funcDef.MethodAttributes;

                int peekFunctionindex = core.ClassTable.ClassNodes[globalClassIndex].vtable.procList.Count;

                // Append arg symbols
                List<KeyValuePair<string, ProtoCore.Type>> argsToBeAllocated = new List<KeyValuePair<string, ProtoCore.Type>>();
                if (null != funcDef.Signature)
                {
                    foreach (VarDeclNode argNode in funcDef.Signature.Arguments)
                    {
                        var argInfo = BuildArgumentInfoFromVarDeclNode(argNode); 
                        localProcedure.argInfoList.Add(argInfo);

                        var argType = BuildArgumentTypeFromVarDeclNode(argNode, gNode);
                        localProcedure.argTypeList.Add(argType);

                        argsToBeAllocated.Add(new KeyValuePair<string, ProtoCore.Type>(argInfo.Name, argType));
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
                    string message = String.Format(ProtoCore.Properties.Resources.kMethodAlreadyDefined, localProcedure.name);
                    buildStatus.LogWarning(WarningID.kFunctionAlreadyDefined, message, core.CurrentDSFileName, funcDef.line, funcDef.col, gNode);
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
                        ProtoCore.Type argType = BuildArgumentTypeFromVarDeclNode(argNode, gNode);
                        argList.Add(argType);
                    }
                }

                globalProcIndex = core.ClassTable.ClassNodes[globalClassIndex].vtable.IndexOfExact(funcDef.Name, argList, false);

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

                    List<AssociativeNode> defaultArgList = core.ClassTable.ClassNodes[globalClassIndex].defaultArgExprList;
                    defaultArgList = BuildSSA(defaultArgList, context);
                    foreach (BinaryExpressionNode bNode in defaultArgList)
                    {
                        ProtoCore.AssociativeGraph.GraphNode graphNode = new ProtoCore.AssociativeGraph.GraphNode();
                        graphNode.exprUID = bNode.exprUID;
                        graphNode.modBlkUID = bNode.modBlkUID;
                        graphNode.ssaExpressionUID = bNode.ssaExpressionUID;
                        graphNode.procIndex = globalProcIndex;
                        graphNode.classIndex = globalClassIndex;
                        graphNode.languageBlockId = codeBlock.codeBlockId;
                        graphNode.isAutoGenerated = true;
                        bNode.IsProcedureOwned = graphNode.ProcedureOwned = true;

                        EmitBinaryExpressionNode(bNode, ref inferedType, false, graphNode, subPass);
                    }

                    //Traverse default argument for the constructor
                    foreach (ProtoCore.DSASM.ArgumentInfo argNode in localProcedure.argInfoList)
                    {
                        if (!argNode.IsDefault)
                        {
                            continue;
                        }
                        BinaryExpressionNode bNode = argNode.DefaultExpression as BinaryExpressionNode;
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

                    EmitCodeBlock(funcDef.FunctionBody.Body, ref inferedType, subPass, true);

                    // Build dependency within the function
                    ProtoCore.AssociativeEngine.Utils.BuildGraphNodeDependencies(
                        codeBlock.instrStream.dependencyGraph.GetGraphNodesAtScope(globalClassIndex, globalProcIndex));

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

                // 'classIndexAtCallsite' is the class index as it is stored at the callsite function tables
                int classIndexAtCallsite = globalClassIndex + 1;
                core.FunctionTable.InitGlobalFunctionEntry(classIndexAtCallsite);

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

                PushGraphNode(retNode);
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

        private ArgumentInfo BuildArgumentInfoFromVarDeclNode(VarDeclNode argNode)
        {
            var argumentName = String.Empty;
            ProtoCore.AST.Node defaultExpression = null;
            if (argNode.NameNode is IdentifierNode)
            {
                argumentName = (argNode.NameNode as IdentifierNode).Value;
            }
            else if (argNode.NameNode is BinaryExpressionNode)
            {
                var bNode = argNode.NameNode as BinaryExpressionNode;
                defaultExpression = bNode;
                argumentName = (bNode.LeftNode as IdentifierNode).Value;
            }
            else
            {
                Validity.Assert(false, "Check generated AST");
            }

            var argInfo = new ProtoCore.DSASM.ArgumentInfo 
            { 
                Name = argumentName, 
                DefaultExpression = defaultExpression,
                Attributes = argNode.ExternalAttributes
            };
            return argInfo;
        }

        private void EmitFunctionDefinitionNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, GraphNode graphNode = null)
        {
            bool parseGlobalFunctionBody = null == localProcedure && ProtoCore.CompilerDefinitions.Associative.CompilePass.kGlobalFuncBody == compilePass;
            bool parseMemberFunctionBody = ProtoCore.DSASM.Constants.kGlobalScope != globalClassIndex && ProtoCore.CompilerDefinitions.Associative.CompilePass.kClassMemFuncBody == compilePass;

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
                var uid = core.TypeSystem.GetType(funcDef.ReturnType.Name);
                var rank = funcDef.ReturnType.rank;
                localProcedure.returntype = core.TypeSystem.BuildTypeObject(uid, rank); 
                if (localProcedure.returntype.UID == (int)PrimitiveType.kInvalidType)
                {
                    string message = String.Format(ProtoCore.Properties.Resources.kReturnTypeUndefined, funcDef.ReturnType.Name, funcDef.Name);
                    buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kTypeUndefined, message, core.CurrentDSFileName, funcDef.line, funcDef.col, graphNode);
                    localProcedure.returntype.UID = (int)PrimitiveType.kTypeVar;
                }
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
                    foreach (VarDeclNode argNode in funcDef.Signature.Arguments)
                    {
                        var argInfo = BuildArgumentInfoFromVarDeclNode(argNode);
                        localProcedure.argInfoList.Add(argInfo);

                        var argType = BuildArgumentTypeFromVarDeclNode(argNode, graphNode);
                        localProcedure.argTypeList.Add(argType);

                        argsToBeAllocated.Add(new KeyValuePair<string, ProtoCore.Type>(argInfo.Name, argType));

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
                }
                else
                {
                    string message = String.Format(ProtoCore.Properties.Resources.kMethodAlreadyDefined, localProcedure.name);
                    buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kFunctionAlreadyDefined, message, core.CurrentDSFileName, funcDef.line, funcDef.col, graphNode);
                    funcDef.skipMe = true;
                }
            }
            else if (parseGlobalFunctionBody || parseMemberFunctionBody)
            {
                if (core.Options.DisableDisposeFunctionDebug)
                {
                    if (CoreUtils.IsDisposeMethod(node.Name))
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
                        ProtoCore.Type argType = BuildArgumentTypeFromVarDeclNode(argNode, graphNode);
                        argList.Add(argType);
                    }
                }

                // Get the exisitng procedure that was added on the previous pass
                if (ProtoCore.DSASM.Constants.kInvalidIndex == globalClassIndex)
                {
                    globalProcIndex = codeBlock.procedureTable.IndexOfExact(funcDef.Name, argList, false);
                    localProcedure = codeBlock.procedureTable.procList[globalProcIndex];
                }
                else
                {
                    globalProcIndex = core.ClassTable.ClassNodes[globalClassIndex].vtable.IndexOfExact(funcDef.Name, argList, funcDef.IsAutoGeneratedThisProc);
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
                        if (!argNode.IsDefault)
                        {
                            continue;
                        }
                        BinaryExpressionNode bNode = argNode.DefaultExpression as BinaryExpressionNode;
                        // build a temporay node for statement : temp = defaultarg;
                        var iNodeTemp = nodeBuilder.BuildTempVariable();
                        var bNodeTemp = nodeBuilder.BuildBinaryExpression(iNodeTemp, bNode.LeftNode) as BinaryExpressionNode;
                        bNodeTemp.IsProcedureOwned = true;
                        EmitBinaryExpressionNode(bNodeTemp, ref inferedType, false, null, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier);
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
                        EmitBinaryExpressionNode(bNodeTemp, ref inferedType, false, null, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier);
                    }
                    emitDebugInfo = true;

                    EmitCompileLogFunctionStart(GetFunctionSignatureString(funcDef.Name, funcDef.ReturnType, funcDef.Signature));

                    ProtoCore.Type itype = new ProtoCore.Type();

                    hasReturnStatement = EmitCodeBlock(funcDef.FunctionBody.Body, ref itype, subPass, true);

                    // Build dependency within the function
                    ProtoCore.AssociativeEngine.Utils.BuildGraphNodeDependencies(
                        codeBlock.instrStream.dependencyGraph.GetGraphNodesAtScope(globalClassIndex, globalProcIndex));


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

                // 'classIndexAtCallsite' is the class index as it is stored at the callsite function tables
                int classIndexAtCallsite = globalClassIndex + 1;
                core.FunctionTable.InitGlobalFunctionEntry(classIndexAtCallsite);

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
                        string message = String.Format(ProtoCore.Properties.Resources.kFunctionNotReturnAtAllCodePaths, localProcedure.name);
                        buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kMissingReturnStatement, message, core.CurrentDSFileName, funcDef.line, funcDef.col, graphNode);
                    }
                    EmitReturnNull();
                }

                if (core.Options.DisableDisposeFunctionDebug)
                {
                    if (CoreUtils.IsDisposeMethod(node.Name))
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
            ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, ProtoCore.AST.AssociativeAST.BinaryExpressionNode parentNode = null)
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


            if (subPass == ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone)
            {
                // Handle static calls to reflect the original call
                BuildRealDependencyForIdentList(graphNode);
                FunctionDotCallNode fcall = node as FunctionDotCallNode;
                if (fcall != null)
                {
                    if (fcall.isLastSSAIdentListFactor)
                    {
                        Validity.Assert(null != ssaPointerStack);
                        ssaPointerStack.Pop();
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

                    // Memoize the graphnode that contains a user-defined function call in the global scope
                    bool inGlobalScope = ProtoCore.DSASM.Constants.kGlobalScope == globalProcIndex && ProtoCore.DSASM.Constants.kInvalidIndex == globalClassIndex;
                    if (!procNode.isExternal && inGlobalScope)
                    {
                        core.GraphNodeCallList.Add(graphNode);
                    }
                }
            }
            IsAssociativeArrayIndexing = arrayIndexing;

            inferedType.UID = isBooleanOp ? (int)PrimitiveType.kTypeBool : inferedType.UID;

            if (subPass == ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone)
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

        private void EmitModifierStackNode(AssociativeNode node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone)
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
                            ProcedureNode procCallNode = core.ClassTable.ClassNodes[ci].GetFirstMemberFunctionBy(rnode.Function.Name, rnode.FormalArguments.Count);

                            // Only if procedure node is non-null do we know its a member function and prefix it with an instance pointer
                            if (procCallNode != null)
                            {
                                ////////////////////////////////      
                                BinaryExpressionNode previousElementNode = null;
                                if (core.Options.GenerateSSA)
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

                DebugProperties.BreakpointOptions oldOptions = core.DebuggerProperties.breakOptions;

                if (emitBreakpointForPop)
                {
                    DebugProperties.BreakpointOptions newOptions = oldOptions;
                    newOptions |= DebugProperties.BreakpointOptions.EmitPopForTempBreakpoint;
                    core.DebuggerProperties.breakOptions = newOptions;
                }

                DfsTraverse(modifierNode, ref inferedType, isBooleanOp, graphNode, subPass);
                core.DebuggerProperties.breakOptions = oldOptions;
            }
            //core.Options.EmitBreakpoints = true;
        }

        private void EmitIfStatementNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.AssociativeGraph.GraphNode graphNode = null)
        {
            int bp = (int)ProtoCore.DSASM.Constants.kInvalidIndex;
            int L1 = (int)ProtoCore.DSASM.Constants.kInvalidIndex;

            // If-expr
            IfStatementNode ifnode = node as IfStatementNode;
            DfsTraverse(ifnode.ifExprNode, ref inferedType, false, graphNode);
            L1 = ProtoCore.DSASM.Constants.kInvalidIndex;
            bp = pc;
            EmitCJmp(L1);


            // Create a new codeblock for this block
            // Set the current codeblock as the parent of the new codeblock
            // Set the new codeblock as a new child of the current codeblock
            // Set the new codeblock as the current codeblock
            ProtoCore.DSASM.CodeBlock localCodeBlock = new ProtoCore.DSASM.CodeBlock(
                context.guid,
                ProtoCore.DSASM.CodeBlockType.kConstruct,
                Language.kInvalid,
                core.CodeBlockIndex++,
                new ProtoCore.DSASM.SymbolTable(GetConstructBlockName("if"), core.RuntimeTableIndex++),
                null,
                false,
                core);


            localCodeBlock.instrStream = codeBlock.instrStream;
            localCodeBlock.parent = codeBlock;
            codeBlock.children.Add(localCodeBlock);
            codeBlock = localCodeBlock;

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
                    context.guid,
                    ProtoCore.DSASM.CodeBlockType.kConstruct,
                    Language.kInvalid,
                    core.CodeBlockIndex++,
                    new ProtoCore.DSASM.SymbolTable(GetConstructBlockName("else"), core.RuntimeTableIndex++),
                    null,
                    false,
                    core);

                localCodeBlock.instrStream = codeBlock.instrStream;
                localCodeBlock.parent = codeBlock;
                codeBlock.children.Add(localCodeBlock);
                codeBlock = localCodeBlock;
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

        private void EmitDynamicBlockNode(int block, ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone)
        {
            if (subPass == ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier)
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

        private void EmitInlineConditionalNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, ProtoCore.AST.AssociativeAST.BinaryExpressionNode parentNode = null)
        {
            if (subPass == ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier)
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

                DebugProperties.BreakpointOptions oldOptions = core.DebuggerProperties.breakOptions;
                DebugProperties.BreakpointOptions newOptions = oldOptions;
                newOptions |= DebugProperties.BreakpointOptions.EmitInlineConditionalBreakpoint;
                core.DebuggerProperties.breakOptions = newOptions;

                core.DebuggerProperties.highlightRange = new ProtoCore.CodeModel.CodeRange
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

                // As SSA conversion is enabled, we have got the values of
                // true and false branch, so it isn't necessary to create 
                // language blocks.
                if (core.Options.GenerateSSA)
                {
                    inlineCall.FormalArguments.Add(inlineConditionalNode.ConditionExpression);
                    inlineCall.FormalArguments.Add(inlineConditionalNode.TrueExpression);
                    inlineCall.FormalArguments.Add(inlineConditionalNode.FalseExpression);
                }
                else
                {
                    // True condition language block
                    BinaryExpressionNode bExprTrue = new BinaryExpressionNode();
                    bExprTrue.LeftNode = nodeBuilder.BuildReturn();
                    bExprTrue.Optr = Operator.assign;
                    bExprTrue.RightNode = inlineConditionalNode.TrueExpression;

                    LanguageBlockNode langblockT = new LanguageBlockNode();
                    int trueBlockId = Constants.kInvalidIndex;
                    langblockT.codeblock.language = ProtoCore.Language.kAssociative;
                    langblockT.codeblock.fingerprint = "";
                    langblockT.codeblock.version = "";
                    core.AssocNode = bExprTrue;
                    core.InlineConditionalBodyGraphNodes.Push(new List<GraphNode>());
                    EmitDynamicLanguageBlockNode(langblockT, bExprTrue, ref inferedType, ref trueBlockId, graphNode, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone);
                    List<GraphNode> trueBodyNodes = core.InlineConditionalBodyGraphNodes.Pop();

                    // Append dependent nodes of the inline conditional 
                    foreach (GraphNode gnode in trueBodyNodes)
                        foreach (GraphNode dNode in gnode.dependentList)
                            graphNode.PushDependent(dNode);

                    core.AssocNode = null;
                    DynamicBlockNode dynBlockT = new DynamicBlockNode(trueBlockId);

                    // False condition language block
                    BinaryExpressionNode bExprFalse = new BinaryExpressionNode();
                    bExprFalse.LeftNode = nodeBuilder.BuildReturn();
                    bExprFalse.Optr = Operator.assign;
                    bExprFalse.RightNode = inlineConditionalNode.FalseExpression;

                    LanguageBlockNode langblockF = new LanguageBlockNode();
                    int falseBlockId = Constants.kInvalidIndex;
                    langblockF.codeblock.language = ProtoCore.Language.kAssociative;
                    langblockF.codeblock.fingerprint = "";
                    langblockF.codeblock.version = "";
                    core.AssocNode = bExprFalse;
                    core.InlineConditionalBodyGraphNodes.Push(new List<GraphNode>());
                    EmitDynamicLanguageBlockNode(langblockF, bExprFalse, ref inferedType, ref falseBlockId, graphNode, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone);
                    List<GraphNode> falseBodyNodes = core.InlineConditionalBodyGraphNodes.Pop();

                    // Append dependent nodes of the inline conditional 
                    foreach (GraphNode gnode in falseBodyNodes)
                        foreach (GraphNode dNode in gnode.dependentList)
                            graphNode.PushDependent(dNode);

                    core.AssocNode = null;
                    DynamicBlockNode dynBlockF = new DynamicBlockNode(falseBlockId);

                    inlineCall.FormalArguments.Add(inlineConditionalNode.ConditionExpression);
                    inlineCall.FormalArguments.Add(dynBlockT);
                    inlineCall.FormalArguments.Add(dynBlockF);
                }

                core.DebuggerProperties.breakOptions = oldOptions;
                core.DebuggerProperties.highlightRange = new ProtoCore.CodeModel.CodeRange
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

                // Save the pc and store it after the call
                EmitFunctionCallNode(inlineCall, ref inferedType, false, graphNode, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier);
                EmitFunctionCallNode(inlineCall, ref inferedType, false, graphNode, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, parentNode);

                // Need to restore those settings.
                if (graphNode != null)
                {
                    graphNode.isInlineConditional = isInlineConditionalFlag;
                    graphNode.updateBlock.startpc = startPC;
                    graphNode.isReturn = isReturn;
                }
            }
        }

        private void EmitUnaryExpressionNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone)
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
                    binRight.RightNode = new IntNode(1);
                    binRight.Optr = (ProtoCore.DSASM.UnaryOperator.Increment == u.Operator) ? ProtoCore.DSASM.Operator.add : ProtoCore.DSASM.Operator.sub;
                    bin.LeftNode = u.Expression; bin.RightNode = binRight; bin.Optr = ProtoCore.DSASM.Operator.assign;
                    EmitBinaryExpressionNode(bin, ref inferedType, false, graphNode, subPass);
                }
                else
                    throw new BuildHaltException("Invalid use of prefix operation (DCDDEEF1).");
            }

            DfsTraverse(u.Expression, ref inferedType, false, graphNode, subPass);

            if (!isPrefixOperation && subPass != ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier)
            {
                string op = Op.GetUnaryOpName(u.Operator);
                EmitInstrConsole(op);
                EmitUnary(Op.GetUnaryOpCode(u.Operator));
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
                access = ProtoCore.CompilerDefinitions.AccessModifier.kPublic,
                NameNode = ident,
                ArgumentType = new ProtoCore.Type { Name = className, UID = procOverload.classIndex, rank = 0 }
            };

            procNode.Signature.Arguments.Insert(0, thisPtrArg);
        }
        
        /// <summary>
        /// Associate the SSA'd graphnodes to the final assignment graphnode
        /// 
        /// Given:
        ///     a = b + c   ->  t0 = a
        ///                     t1 = b
        ///                     t2 = t0 + t1
        ///                     a = t2
        ///                     
        /// Store the SSA graphnodes:
        ///     t0, t1, t2
        ///     
        /// in the final graphnode 
        ///     a = t2;
        ///     
        /// </summary>
        private void ResolveSSADependencies()
        {
            foreach (ProtoCore.AssociativeGraph.GraphNode graphNode in codeBlock.instrStream.dependencyGraph.GraphList)
            {
                if (graphNode != null && graphNode.IsSSANode())
                {
                    if (graphNode.lastGraphNode != null)
                    {
                        SymbolNode dependentSymbol = graphNode.updateNodeRefList[0].nodeList[0].symbol;
                        if (!graphNode.lastGraphNode.symbolListWithinExpression.Contains(dependentSymbol))
                        {
                            graphNode.lastGraphNode.symbolListWithinExpression.Add(dependentSymbol);
                        }
                    }
                }
            }
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

                if (firstProc.classScope == Constants.kGlobalScope)
                {
                    graphNode.updateNodeRefList.AddRange(firstProc.updatedGlobals);
                }
                else
                {
                    // For each property modified
                    foreach (ProtoCore.AssociativeGraph.UpdateNodeRef updateRef in firstProc.updatedProperties)
                    {
                        int index = graphNode.firstProcRefIndex;

                        // Is it a global function
                        if (ProtoCore.DSASM.Constants.kInvalidIndex != index)
                        {
                            if (core.Options.GenerateSSA)
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

                                        if (graphNode.lastGraphNode != null)
                                        {
                                            graphNode.lastGraphNode.updateNodeRefList.Add(autogenRef);
                                        }
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
                            if (core.Options.GenerateSSA)
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

        private ProtoCore.AssociativeGraph.UpdateNodeRef __To__Deprecate__AutoGenerateUpdateArgumentReference(AssociativeNode node, ProtoCore.AssociativeGraph.GraphNode graphNode)
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
                    graphNode.firstProc = firstProc;
                }
            }
            ProtoCore.DSASM.SymbolNode firstSymbol = null;

            // See if the leftmost symbol(updateNodeRef) of the lhs expression is a property of the current class. 
            // If it is, then push the lhs updateNodeRef to the list of modified properties in the procedure node
            if (null != localProcedure && leftNodeRef.nodeList.Count > 0)
            {
                firstSymbol = leftNodeRef.nodeList[0].symbol;

                // Check if this symbol being modified in this function is allocated in the current scope.
                // If it is, then it means this symbol is not a member and is local to this function
                ProtoCore.DSASM.SymbolNode symbolnode = null;
                bool isAccessible = false;
                bool isLocalVariable = isLocalVariable = VerifyAllocationInScope(firstSymbol.name, globalClassIndex, globalProcIndex, out symbolnode, out isAccessible);
                if (!isLocalVariable)
                {
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
            }
            return leftNodeRef;
        }

        private void EmitLHSIdentifierListForBinaryExpr(AssociativeNode bnode, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, bool isTempExpression = false)
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
            ProtoCore.AssociativeGraph.UpdateNodeRef leftNodeArgRef = __To__Deprecate__AutoGenerateUpdateArgumentReference(binaryExpr.LeftNode, graphNode);

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
                if (!isThisPtr && !leftNodeRef.Equals(leftNodeArgRef))
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

                if (core.Options.GenerateSSA)
                {
                    if (!graphNode.IsSSANode() && !ProtoCore.AssociativeEngine.Utils.IsTempVarLHS(graphNode))
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
                                graphNode.IsLastNodeInSSA = true;
                                codeBlock.instrStream.dependencyGraph.GraphList[n].lastGraphNode = graphNode;
                            }
                        }
                    }
                }


                // Assign the end pc to this graph node's update block
                // Dependency graph construction is complete for this expression
                graphNode.updateBlock.endpc = pc - 1;
                PushGraphNode(graphNode);
                functionCallStack.Clear();
            }
        }

        private bool EmitLHSThisDotProperyForBinaryExpr(AssociativeNode bnode, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, bool isTempExpression = false)
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

                IdentifierNode identNode = (binaryExpr.LeftNode as IdentifierNode);
                string identName = identNode.Value;

                // Local variables are not appended with 'this'
                if (!identNode.IsLocal)
                {
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
            }

            return false;
        }

        private void HandlePointerList(BinaryExpressionNode bnode)
        {
            // All associative code is SSA'd and we want to keep track of the original identifier nodes of an identifier list:
            //      i.e. x.y.z
            // These identifiers will be used to populate the real graph nodes dependencies
            if (bnode.isSSAPointerAssignment)
            {
                Validity.Assert(null != ssaPointerStack);

                if (bnode.IsFirstIdentListNode)
                {
                    ssaPointerStack.Push(new List<AssociativeNode>());
                }

                if (bnode.RightNode is IdentifierNode)
                {
                    ssaPointerStack.Peek().Add(bnode.RightNode);
                }
                else if (bnode.RightNode is IdentifierListNode)
                {
                    ssaPointerStack.Peek().Add((bnode.RightNode as IdentifierListNode).RightNode);
                }
                else if (bnode.RightNode is FunctionDotCallNode)
                {
                    FunctionDotCallNode dotcall = bnode.RightNode as FunctionDotCallNode;
                    Validity.Assert(dotcall.FunctionCall.Function is IdentifierNode);

                    string className = dotcall.DotCall.FormalArguments[0].Name;
                    string fullyQualifiedClassName = string.Empty;
                    bool isClassName = core.ClassTable.TryGetFullyQualifiedName(className, out fullyQualifiedClassName);

                    if (ProtoCore.Utils.CoreUtils.IsGetterSetter(dotcall.FunctionCall.Function.Name))
                    {
                        //string className = dotcall.DotCall.FormalArguments[0].Name;
                        if (isClassName)
                        {
                            ssaPointerStack.Peek().Add(dotcall.DotCall.FormalArguments[0]);
                        }

                        // This function is an internal getter or setter, store the identifier node
                        ssaPointerStack.Peek().Add(dotcall.FunctionCall.Function);
                    }
                    else
                    {
                        bool isConstructorCall = isClassName ? true : false;
                        if (!isConstructorCall)
                        {
                            // This function is a member function, store the functioncall node
                            ssaPointerStack.Peek().Add(dotcall.FunctionCall);
                        }
                    }
                }
                else if (bnode.RightNode is FunctionCallNode)
                {
                    FunctionCallNode fcall = bnode.RightNode as FunctionCallNode;
                    Validity.Assert(fcall.Function is IdentifierNode);
                    ssaPointerStack.Peek().Add(fcall.Function);
                }
                else
                {
                    Validity.Assert(false);
                }
            }
        }

        private void EmitBinaryExpressionNode(AssociativeNode node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, bool isTempExpression = false)
        {
            BinaryExpressionNode bnode = null;

            if (!IsParsingGlobal() && !IsParsingGlobalFunctionBody() && !IsParsingMemberFunctionBody())
                return;

            bool isBooleanOperation = false;
            bnode = node as BinaryExpressionNode;
            ProtoCore.Type leftType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0);
            ProtoCore.Type rightType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0);

            DebugProperties.BreakpointOptions oldOptions = core.DebuggerProperties.breakOptions;
            
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
                    graphNode.AstID = bnode.ID;
                    graphNode.OriginalAstID = bnode.OriginalAstID; 
                    graphNode.exprUID = bnode.exprUID;
                    graphNode.ssaExprID = bnode.ssaExprID;
                    graphNode.ssaExpressionUID = bnode.ssaExpressionUID;
                    graphNode.IsModifier = bnode.IsModifier;
                    graphNode.guid = bnode.guid;
                    graphNode.modBlkUID = bnode.modBlkUID;
                    graphNode.procIndex = globalProcIndex;
                    graphNode.classIndex = globalClassIndex;
                    graphNode.languageBlockId = codeBlock.codeBlockId;
                    graphNode.ProcedureOwned = bnode.IsProcedureOwned;


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

                if (bnode.isSSAFirstAssignment)
                {
                    firstSSAGraphNode = graphNode;
                }
                HandlePointerList(bnode);
                        

                if (bnode.LeftNode is IdentifierListNode)
                {
                    // If the lhs is an identifierlist then emit the entire expression here
                    // This also handles the dependencies of expressions where the lhs is a member variable (this.x = y)
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
                        string errorMessage = ProtoCore.Properties.Resources.kInvalidThis;
                        if (localProcedure != null)
                        {
                            if (localProcedure.isStatic)
                            {
                                errorMessage = ProtoCore.Properties.Resources.kUsingThisInStaticFunction;
                            }
                            else if (localProcedure.classScope == Constants.kGlobalScope)
                            {
                                errorMessage = ProtoCore.Properties.Resources.kInvalidThis;
                            }
                            else
                            {
                                errorMessage = ProtoCore.Properties.Resources.kAssingToThis;
                            }
                        }
                        core.BuildStatus.LogWarning(WarningID.kInvalidThis, errorMessage, core.CurrentDSFileName, bnode.line, bnode.col, graphNode);

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

                if (inferedType.UID == (int)PrimitiveType.kTypeFunctionPointer && subPass != ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier && emitDebugInfo)
                {
                    buildStatus.LogSemanticError(Resources.FunctionPointerNotAllowedAtBinaryExpression, core.CurrentDSFileName, bnode.LeftNode.line, bnode.LeftNode.col);
                }

                leftType.UID = inferedType.UID;
                leftType.rank = inferedType.rank;
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
                var inferredType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0);
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
                core.DebuggerProperties.breakOptions = newOptions;

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

            DfsTraverse(bnode.RightNode, ref inferedType, isBooleanOperation, graphNode, subPass, node);
#if ENABLE_INC_DEC_FIX
                }
#endif

            rightType.UID = inferedType.UID;
            rightType.rank = inferedType.rank;

            BinaryExpressionNode rightNode = bnode.RightNode as BinaryExpressionNode;
            if ((rightNode != null) && (ProtoCore.DSASM.Operator.assign == rightNode.Optr))
            {
                DfsTraverse(rightNode.LeftNode, ref inferedType, false, graphNode);
            }

            if (bnode.Optr != ProtoCore.DSASM.Operator.assign)
            {
                if (subPass == ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier)
                {
                    return;
                }

                if (inferedType.UID == (int)PrimitiveType.kTypeFunctionPointer && emitDebugInfo)
                {
                    buildStatus.LogSemanticError(Resources.FunctionPointerNotAllowedAtBinaryExpression, core.CurrentDSFileName, bnode.RightNode.line, bnode.RightNode.col);
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
            DfsTraverse(bnode.RightNode, ref inferedType, isBooleanOperation, graphNode, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, bnode);
            subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier;

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
                        procNode = CoreUtils.GetFirstVisibleProcedure(t.Name, null, codeBlock);
                    }
                    if (procNode != null)
                    {
                        if (ProtoCore.DSASM.Constants.kInvalidIndex != procNode.procId && emitDebugInfo)
                        {
                            buildStatus.LogSemanticError(String.Format(Resources.FunctionAsVariableError, t.Name), core.CurrentDSFileName, t.line, t.col);
                        }
                    }

                    //int type = (int)ProtoCore.PrimitiveType.kTypeVoid;
                    bool isLocalDeclaration = t.IsLocal;
                    bool isAccessible = false;
                    bool isAllocated = false;

                    if (isLocalDeclaration)
                    {
                        isAllocated = VerifyAllocationInScope(t.Name, globalClassIndex, globalProcIndex, out symbolnode, out isAccessible);
                    }
                    else
                    {
                        isAllocated = VerifyAllocation(t.Name, globalClassIndex, globalProcIndex, out symbolnode, out isAccessible);
                    }

                    int runtimeIndex = (!isAllocated || !isAccessible) ? codeBlock.symbolTable.RuntimeIndex : symbolnode.runtimeTableIndex;
                    if (isAllocated && !isAccessible)
                    {
                        string message = String.Format(ProtoCore.Properties.Resources.kPropertyIsInaccessible, t.Name);
                        buildStatus.LogWarning(WarningID.kAccessViolation, message, core.CurrentDSFileName, t.line, t.col, graphNode);
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


                    ProtoCore.Type castType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank);
                    var tident = bnode.LeftNode as TypedIdentifierNode;
                    if (tident != null)
                    {
                        int castUID = core.ClassTable.IndexOf(tident.datatype.Name);
                        if ((int)PrimitiveType.kInvalidType == castUID)
                        {
                            castType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kInvalidType, 0);
                            castType.Name = tident.datatype.Name;
                            castType.rank = tident.datatype.rank;
                        }
                        else
                        {
                            castType = core.TypeSystem.BuildTypeObject(castUID, tident.datatype.rank);
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
                                    false, ProtoCore.CompilerDefinitions.AccessModifier.kPublic, ProtoCore.DSASM.MemoryRegion.kMemStack, bnode.line, bnode.col);

                                // Add the symbols during watching process to the watch symbol list.
                                if (core.Options.RunMode == ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter)
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
                            EmitPushVarData(dimensions, castType.UID, castType.rank);

                            symbol = symbolnode.symbolTableIndex;
                            if (t.Name == ProtoCore.DSASM.Constants.kTempArg)
                            {
                                EmitInstrConsole(ProtoCore.DSASM.kw.pop, t.Name);
                                EmitPopForSymbol(symbolnode, runtimeIndex);
                            }
                            else
                            {
                                if (core.Options.RunMode != ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter)
                                {
                                    EmitInstrConsole(ProtoCore.DSASM.kw.pop, t.Name);
                                    EmitPopForSymbol(symbolnode, runtimeIndex, node.line, node.col, node.endLine, node.endCol);
                                }
                                else
                                {
                                    EmitInstrConsole(ProtoCore.DSASM.kw.popw, t.Name);
                                    EmitPopForSymbolW(symbolnode, runtimeIndex, node.line, node.col, node.endLine, node.endCol);
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
                            EmitPushVarData(dimensions, castType.UID, castType.rank);

                            EmitInstrConsole(ProtoCore.DSASM.kw.popm, t.Name);

                            if (symbolnode.isStatic)
                            {
                                var op = StackValue.BuildStaticMemVarIndex(symbol);
                                EmitPopm(op, runtimeIndex, node.line, node.col, node.endLine, node.endCol);
                            }
                            else
                            {
                                var op = StackValue.BuildMemVarIndex(symbol);
                                EmitPopm(op, runtimeIndex, node.line, node.col, node.endLine, node.endCol);
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
                                    false, ProtoCore.CompilerDefinitions.AccessModifier.kPublic, ProtoCore.DSASM.MemoryRegion.kMemStack, bnode.line, bnode.col);

                            if (core.Options.RunMode == ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter)
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


                        if (bnode.IsInputExpression)
                        {
                            StackValue regLX = StackValue.BuildRegister(Registers.LX);
                            EmitInstrConsole(ProtoCore.DSASM.kw.pop, ProtoCore.DSASM.kw.regLX);
                            EmitPop(regLX, globalClassIndex);

                            graphNode.updateBlock.updateRegisterStartPC = pc;

                            EmitInstrConsole(ProtoCore.DSASM.kw.push, ProtoCore.DSASM.kw.regLX);
                            EmitPush(regLX);
                        }

                        EmitPushVarData(dimensions, castType.UID, castType.rank);

                        if (core.Options.RunMode != ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter)
                        {
                            EmitInstrConsole(ProtoCore.DSASM.kw.pop, symbolnode.name);
                            EmitPopForSymbol(symbolnode, runtimeIndex, node.line, node.col, node.endLine, node.endCol);
                        }
                        else
                        {
                            EmitInstrConsole(ProtoCore.DSASM.kw.popw, symbolnode.name);
                            EmitPopForSymbolW(symbolnode, runtimeIndex, node.line, node.col, node.endLine, node.endCol);                            
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

                    {
                        if (!graphNode.IsSSANode() && !ProtoCore.AssociativeEngine.Utils.IsTempVarLHS(graphNode))
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
                                    graphNode.IsLastNodeInSSA = true;
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

                    string identName = t.Name;
                    if (ProtoCore.Utils.CoreUtils.IsSSATemp(identName))
                    {
                        // Extract the SSA subscript from the ID
                        // TODO Jun: Store the subscript before embedding it into the ID so we dont need to parse and extract it here
                        int first = identName.IndexOf('_');
                        int last = identName.LastIndexOf('_');
                        string subscript = identName.Substring(first + 1, last - first - 1);
                        graphNode.SSASubscript = Convert.ToInt32(subscript);
                    }

                    PushGraphNode(graphNode);
                    if (core.InlineConditionalBodyGraphNodes.Count > 0)
                    {
                        core.InlineConditionalBodyGraphNodes.Last().Add(graphNode);
                    }

                    SymbolNode cyclicSymbol1 = null;
                    SymbolNode cyclicSymbol2 = null;
                    if (core.Options.staticCycleCheck)
                    {
                        if (!CyclicDependencyTest(graphNode, ref cyclicSymbol1, ref cyclicSymbol2))
                        {
                            Validity.Assert(null != cyclicSymbol1);
                            Validity.Assert(null != cyclicSymbol2);

                            //
                            // Set the first symbol that triggers the cycle to null
                            ProtoCore.AssociativeGraph.GraphNode nullAssignGraphNode1 = new ProtoCore.AssociativeGraph.GraphNode();
                            nullAssignGraphNode1.updateBlock.startpc = pc;

                            EmitPushNull();
                            EmitPushVarData(0);
                            EmitInstrConsole(ProtoCore.DSASM.kw.pop, cyclicSymbol1.name);
                            EmitPopForSymbol(cyclicSymbol1, cyclicSymbol1.runtimeTableIndex, node.line, node.col, node.endLine, node.endCol);

                            nullAssignGraphNode1.PushSymbolReference(cyclicSymbol1);
                            nullAssignGraphNode1.procIndex = globalProcIndex;
                            nullAssignGraphNode1.classIndex = globalClassIndex;
                            nullAssignGraphNode1.updateBlock.endpc = pc - 1;

                            PushGraphNode(nullAssignGraphNode1);
                            EmitDependency(ProtoCore.DSASM.Constants.kInvalidIndex, ProtoCore.DSASM.Constants.kInvalidIndex, false);


                            //
                            // Set the second symbol that triggers the cycle to null
                            ProtoCore.AssociativeGraph.GraphNode nullAssignGraphNode2 = new ProtoCore.AssociativeGraph.GraphNode();
                            nullAssignGraphNode2.updateBlock.startpc = pc;

                            EmitPushNull();
                            EmitPushVarData(0);
                            EmitInstrConsole(ProtoCore.DSASM.kw.pop, cyclicSymbol2.name);
                            EmitPopForSymbol(cyclicSymbol2, cyclicSymbol2.runtimeTableIndex, node.line, node.col, node.endLine, node.endCol);

                            nullAssignGraphNode2.PushSymbolReference(cyclicSymbol2);
                            nullAssignGraphNode2.procIndex = globalProcIndex;
                            nullAssignGraphNode2.classIndex = globalClassIndex;
                            nullAssignGraphNode2.updateBlock.endpc = pc - 1;

                            PushGraphNode(nullAssignGraphNode2);
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
            core.DebuggerProperties.breakOptions = oldOptions;

            //if post fix, now traverse the post fix

#if ENABLE_INC_DEC_FIX
                if (bnode.RightNode is PostFixNode)
                    EmitPostFixNode(bnode.RightNode, ref inferedType);
#endif
        }

        private void EmitImportNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone)
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

                if (ProtoCore.CompilerDefinitions.Associative.CompilePass.kGlobalFuncBody == compilePass)
                {
                    core.LoadedDLLs.Add(importNode.ModulePathFileName);
                    firstImportInDeltaExecution = true;
                }
            }

            if (codeBlockNode != null)
            {
                // Only build SSA for the first time
                // Transform after class name compile pass
                if (ProtoCore.CompilerDefinitions.Associative.CompilePass.kClassName > compilePass)
                {
                    codeBlockNode.Body = ApplyTransform(codeBlockNode.Body);
                    codeBlockNode.Body = BuildSSA(codeBlockNode.Body, context);
                }

                foreach (AssociativeNode assocNode in codeBlockNode.Body)
                {
                    inferedType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0); 

                    if (assocNode is LanguageBlockNode)
                    {
                        // Build a binaryn node with a temporary lhs for every stand-alone language block
                        var iNode = nodeBuilder.BuildIdentfier(core.GenerateTempLangageVar());
                        var langBlockNode = nodeBuilder.BuildBinaryExpression(iNode, assocNode);

                        DfsTraverse(langBlockNode, ref inferedType, false, null, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier);
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
            EmitBinaryExpressionNode(assignment, ref inferedType, false, graphNode, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier, true);

            result.ArrayDimensions = thisNode.ArrayDimensions;
            return result;
        }

        override protected void EmitGetterSetterForIdentList(
            ProtoCore.AST.Node node, 
            ref ProtoCore.Type inferedType, 
            ProtoCore.AssociativeGraph.GraphNode graphNode, 
            ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass, 
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
                    EmitBinaryExpressionNode(assignment, ref inferedType, false, graphNode, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier, true);

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
                            EmitBinaryExpressionNode(tmpGetInlineRet, ref inferedType, false, graphNode, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier, true);
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
                        EmitBinaryExpressionNode(tmpAssignmentNode, ref inferedType, false, graphNode, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier, true);
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
                        EmitBinaryExpressionNode(assignment, ref inferedType, false, graphNode, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier, true);
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
                        EmitBinaryExpressionNode(assignment, ref inferedType, false, graphNode, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier, true);

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
                    EmitIdentifierNode(retnode, ref inferedType, false, graphNode, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone);
                    isCollapsed = true;
                }
            }
        }

        private void EmitGroupExpressionNode(AssociativeNode node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone)
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
                if (subPass != ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier)
                {
                    int dimensions = DfsEmitArrayIndexHeap(group.ArrayDimensions, graphNode);
                    EmitPushArrayIndex(dimensions);
                }
            }

            if (subPass != ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kUnboundIdentifier)
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

            PushGraphNode(retNode);
        }

        private ProtoCore.Type BuildArgumentTypeFromVarDeclNode(VarDeclNode argNode, GraphNode graphNode = null)
        {
            Validity.Assert(argNode != null);
            if (argNode == null)
            {
                return new ProtoCore.Type();
            }

            int uid = core.TypeSystem.GetType(argNode.ArgumentType.Name);
            if (uid == (int)PrimitiveType.kInvalidType && !core.IsTempVar(argNode.NameNode.Name))
            {
                string message = String.Format(ProtoCore.Properties.Resources.kArgumentTypeUndefined, argNode.ArgumentType.Name, argNode.NameNode.Name);
                buildStatus.LogWarning(WarningID.kTypeUndefined, message, core.CurrentDSFileName, argNode.line, argNode.col, graphNode);
            }

            int rank = argNode.ArgumentType.rank;

            return core.TypeSystem.BuildTypeObject(uid, rank);
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
            return (!InsideFunction()) && (ProtoCore.CompilerDefinitions.Associative.CompilePass.kGlobalScope == compilePass);
        }

        private bool IsParsingGlobalFunctionBody()
        {
            return (InsideFunction()) && (ProtoCore.CompilerDefinitions.Associative.CompilePass.kGlobalFuncBody == compilePass);
        }

        private bool IsParsingMemberFunctionBody()
        {
            return (ProtoCore.DSASM.Constants.kGlobalScope != globalClassIndex) && (ProtoCore.CompilerDefinitions.Associative.CompilePass.kClassMemFuncBody == compilePass);
        }

        private bool IsParsingGlobalFunctionSig()
        {
            return (null == localProcedure) && (ProtoCore.CompilerDefinitions.Associative.CompilePass.kGlobalFuncSig == compilePass);
        }

        private bool IsParsingMemberFunctionSig()
        {
            return (ProtoCore.DSASM.Constants.kGlobalScope != globalClassIndex) && (ProtoCore.CompilerDefinitions.Associative.CompilePass.kClassMemFuncSig == compilePass);
        }

        protected override void DfsTraverse(ProtoCore.AST.Node pNode, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, ProtoCore.AST.Node parentNode = null)
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
                EmitStringNode(node, ref inferedType, graphNode, subPass);
            }
            else if (node is DefaultArgNode)
            {
                EmitDefaultArgNode(subPass);
            }
            else if (node is NullNode)
            {
                EmitNullNode(node, ref inferedType, isBooleanOp, subPass);
            }
            else if (node is RangeExprNode)
            {
                EmitRangeExprNode(node, ref inferedType, graphNode, subPass);
            }
            else if (node is LanguageBlockNode)
            {
                EmitLanguageBlockNode(node, ref inferedType, graphNode, subPass);
            }
            else if (node is ClassDeclNode)
            {
                EmitClassDeclNode(node, ref inferedType, subPass, graphNode);
            }
            else if (node is ConstructorDefinitionNode)
            {
                EmitConstructorDefinitionNode(node, ref inferedType, subPass, graphNode);                
            }
            else if (node is FunctionDefinitionNode)
            {
                EmitFunctionDefinitionNode(node, ref inferedType, subPass, graphNode);            
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
            else if (node is ThisPointerNode)
            {
                EmitThisPointerNode(subPass);
            }
            else if (node is DynamicNode)
            {
                EmitDynamicNode(subPass);
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
            ident.datatype = TypeSystem.BuildPrimitiveTypeObject(type, 0);
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

