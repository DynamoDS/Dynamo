using System;
using System.IO;
using System.Collections.Generic;
using ProtoCore;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Utils;
using ProtoCore.DSASM;
using System.Text;
using ProtoCore.AssociativeGraph;
using ProtoCore.BuildData;
using System.Linq;
using ProtoAssociative.Properties;
using ProtoCore.CompilerDefinitions;

namespace ProtoAssociative
{
    public partial class CodeGen : ProtoCore.CodeGen
    {
        public ProtoCore.CompilerDefinitions.CompilePass compilePass;

        // Jun Comment: 'setConstructorStartPC' a flag to check if the graphnode pc needs to be adjusted by -1
        // This is because constructors auto insert an allocc instruction before any cosntructo body is traversed
        private bool setConstructorStartPC;

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

        // This constructor is only called for Preloading of assemblies and 
        // precompilation of CodeBlockNode nodes in GraphUI for global language blocks - pratapa
        public CodeGen(Core coreObj) : base(coreObj)
        {
            Validity.Assert(core.IsParsingPreloadedAssembly || core.IsParsingCodeBlockNode);

            classOffset = 0;

            //  either of these should set the console to flood
            emitReplicationGuide = false;
            setConstructorStartPC = false;

            // Re-use the existing procedureTable and symbolTable to access the built-in and predefined functions
            ProcedureTable procTable = core.CodeBlockList[0].procedureTable;
            codeBlock = BuildNewCodeBlock(procTable);
            compilePass = ProtoCore.CompilerDefinitions.CompilePass.ClassName;

            // Bouncing to this language codeblock from a function should immediately set the first instruction as the entry point
            if (ProtoCore.DSASM.Constants.kGlobalScope != globalProcIndex)
            {
                isEntrySet = true;
                codeBlock.instrStream.entrypoint = 0;
            }

            unPopulatedClasses = new Dictionary<int, ClassDeclNode>();
        }

        public CodeGen(Core coreObj, ProtoCore.CompileTime.Context callContext, ProtoCore.DSASM.CodeBlock parentBlock = null) : base(coreObj, parentBlock)
        {
            context = callContext;
            classOffset = 0;

            //  either of these should set the console to flood
            emitReplicationGuide = false;
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

            compilePass = ProtoCore.CompilerDefinitions.CompilePass.ClassName;

            // Bouncing to this language codeblock from a function should immediately set the first instruction as the entry point
            if (ProtoCore.DSASM.Constants.kGlobalScope != globalProcIndex)
            {
                isEntrySet = true;
                codeBlock.instrStream.entrypoint = 0;
            }

            unPopulatedClasses = new Dictionary<int, ClassDeclNode>();
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
            dependentNode.PushSymbolReference(symbol, UpdateNodeType.Symbol);
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
                ProtoCore.DSASM.CodeBlockType.Language,
                ProtoCore.Language.Associative,
                core.CodeBlockIndex,
                new ProtoCore.DSASM.SymbolTable("associative lang block", core.RuntimeTableIndex),
                pTable,
                false,
                core);

            ++core.CodeBlockIndex;
            ++core.RuntimeTableIndex;

            return cb;
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
                || subNode.ssaSubExpressionID == node.ssaSubExpressionID
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
            if (subNode.updateNodeRefList[0].nodeList[0].nodeType != ProtoCore.AssociativeGraph.UpdateNodeType.Method)
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

                foreach (var n in dependencyList.GroupBy(x => x.guid).Select(y => y.First()))
                {
                    core.BuildStatus.LogWarning(ProtoCore.BuildData.WarningID.InvalidStaticCyclicDependency,
                        ProtoCore.Properties.Resources.kInvalidStaticCyclicDependency, core.CurrentDSFileName, graphNode: n);
                }
                firstNode.isCyclic = true;

                cyclicSymbol1 = firstNode.updateNodeRefList[0].nodeList[0].symbol;
                cyclicSymbol2 = lastNode.updateNodeRefList[0].nodeList[0].symbol;

                firstNode.cyclePoint = lastNode;

                return false;
            }
        }

        private SymbolNode Allocate(
            int classIndex,  // In which class table this variable will be allocated to ?
            int classScope,  // Variable's class scope. For example, it is a variable in base class
            int funcIndex,   // In which function this variable is defined? 
            string ident, 
            ProtoCore.Type datatype, 
            bool isStatic = false,
            ProtoCore.CompilerDefinitions.AccessModifier access = ProtoCore.CompilerDefinitions.AccessModifier.Public,
            int line = -1,
            int col = -1)
        {
            bool allocateForBaseVar = classScope < classIndex;
            bool isProperty = classIndex != Constants.kInvalidIndex && funcIndex == Constants.kInvalidIndex;
            if (!allocateForBaseVar && !isProperty && core.ClassTable.IndexOf(ident) != ProtoCore.DSASM.Constants.kInvalidIndex)
                buildStatus.LogSemanticError(String.Format(Resources.ClassNameAsVariableError, ident), null, line, col);

            ProtoCore.DSASM.SymbolNode symbolnode = new ProtoCore.DSASM.SymbolNode();
            symbolnode.name = ident;
            symbolnode.isTemp = ident.StartsWith("%");
            symbolnode.isSSATemp = CoreUtils.IsSSATemp(ident);
            symbolnode.functionIndex = funcIndex;
            symbolnode.absoluteFunctionIndex = funcIndex;
            symbolnode.datatype = datatype;
            symbolnode.isArgument = false;
            symbolnode.memregion = MemoryRegion.MemStack;
            symbolnode.classScope = classScope;
            symbolnode.absoluteClassScope = classScope;
            symbolnode.runtimeTableIndex = codeBlock.symbolTable.RuntimeIndex;
            symbolnode.isStatic = isStatic;
            symbolnode.access = access;
            symbolnode.codeBlockId = codeBlock.codeBlockId;
            if (isEmittingImportNode)
                symbolnode.ExternLib = core.CurrentDSFileName;

            int symbolindex = ProtoCore.DSASM.Constants.kInvalidIndex;

            if (IsInLanguageBlockDefinedInFunction())
            {
                symbolnode.classScope = Constants.kGlobalScope;
                symbolnode.functionIndex = Constants.kGlobalScope;
            }

            if (ProtoCore.DSASM.Constants.kInvalidIndex != classIndex && !IsInLanguageBlockDefinedInFunction())
            {
                symbolindex = core.ClassTable.ClassNodes[classIndex].Symbols.IndexOf(ident);
                if (symbolindex != ProtoCore.DSASM.Constants.kInvalidIndex)
                {
                    ProtoCore.DSASM.SymbolNode node = core.ClassTable.ClassNodes[classIndex].Symbols.symbolList[symbolindex];
                    if (node.functionIndex == ProtoCore.DSASM.Constants.kGlobalScope &&
                        funcIndex == ProtoCore.DSASM.Constants.kGlobalScope)
                        return null;
                }

                symbolindex = core.ClassTable.ClassNodes[classIndex].Symbols.Append(symbolnode);
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
                    staticSymbolnode.isSSATemp = CoreUtils.IsSSATemp(ident);
                    staticSymbolnode.functionIndex = funcIndex;
                    staticSymbolnode.datatype = datatype;
                    staticSymbolnode.isArgument = false;
                    staticSymbolnode.memregion = MemoryRegion.MemStack;
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
            AssociativeNode nodeArray = null,
            ProtoCore.DSASM.MemoryRegion region = ProtoCore.DSASM.MemoryRegion.MemStack)
        {
            ProtoCore.DSASM.SymbolNode node = new ProtoCore.DSASM.SymbolNode(
                ident,
                ProtoCore.DSASM.Constants.kInvalidIndex,
                funcIndex,
                datatype,
                true,
                codeBlock.symbolTable.RuntimeIndex,
                region,
                globalClassIndex);

            node.name = ident;
            node.isTemp = ident.StartsWith("%");
            node.functionIndex = funcIndex;
            node.absoluteFunctionIndex = funcIndex;
            node.datatype = datatype;
            node.isArgument = true;
            node.memregion = ProtoCore.DSASM.MemoryRegion.MemStack;
            node.classScope = globalClassIndex;
            node.absoluteClassScope = globalClassIndex;
            node.codeBlockId = codeBlock.codeBlockId;
            if (this.isEmittingImportNode)
                node.ExternLib = core.CurrentDSFileName;

            // Comment Jun: The local count will be adjusted and all dependent symbol offsets after the function body has been traversed
            int locOffset = 0;

            // This will be offseted by the local count after locals have been allocated
            node.index = -1 - ProtoCore.DSASM.StackFrame.StackFrameSize - (locOffset + argOffset);
            ++argOffset;

            int symbolindex = ProtoCore.DSASM.Constants.kInvalidIndex;
            if (ProtoCore.DSASM.Constants.kInvalidIndex != globalClassIndex)
            {
                symbolindex = core.ClassTable.ClassNodes[globalClassIndex].Symbols.Append(node);
            }
            else
            {
                symbolindex = codeBlock.symbolTable.Append(node);
            }
            return symbolindex;
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
            ProtoCore.CompilerDefinitions.SubCompilePass subPass,
            bool isProcedureOwned
            )
        {
            bool hasReturnStatement = false;
            foreach (AssociativeNode bnode in astList)
            {
                inferedType.UID = (int)PrimitiveType.Var;
                inferedType.rank = 0;

                if (bnode is LanguageBlockNode)
                {
                    // Build a binaryn node with a temporary lhs for every stand-alone language block
                    var iNode =AstFactory.BuildIdentifier(core.GenerateTempLangageVar());
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
            instr.opCode = ProtoCore.DSASM.OpCode.NEWOBJ;
            instr.op1 = StackValue.BuildClassIndex(type);

            ++pc;
            codeBlock.instrStream.instrList.Add(instr);

            // TODO: Figure out why using AppendInstruction fails for adding these instructions to ExpressionInterpreter
            //AppendInstruction(instr);
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

        protected void EmitDependency(int exprUID, bool isSSAAssign)
        {
            SetEntry();

            EmitInstrConsole(ProtoCore.DSASM.kw.dep, exprUID.ToString() + "[ExprUID]", isSSAAssign.ToString() + "[SSA]");

            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.DEP;

            instr.op1 = StackValue.BuildInt(exprUID);
            instr.op2 = StackValue.BuildBoolean(isSSAAssign);

            ++pc;
            codeBlock.instrStream.instrList.Add(instr);

            if (!core.Options.IsDeltaExecution)
            {
                EmitJumpDependency();
            }
        }

        public ProtoCore.DSASM.ProcedureNode GetProcedureFromInstance(int classScope, ProtoCore.AST.AssociativeAST.FunctionCallNode funcCall)
        {
            string procName = funcCall.Function.Name;
            Validity.Assert(ProtoCore.DSASM.Constants.kInvalidIndex != classScope);
            List<ProtoCore.Type> arglist = new List<ProtoCore.Type>();

            foreach (AssociativeNode paramNode in funcCall.FormalArguments)
            {
                ProtoCore.Type paramType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 0);
                DfsTraverse(paramNode, ref paramType, false, null, ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier);
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
                                      ProtoCore.AST.Node funcCall, 
                                      bool isGetter = false, 
                                      BinaryExpressionNode parentExpression = null)
        {
            int blockId = procNode.RuntimeIndex;

            //push value-not-provided default argument
            for (int i = arglist.Count; i < procNode.ArgumentInfos.Count; i++)
            {
                EmitDefaultArgNode();
            }

            // Emit depth
            EmitInstrConsole(kw.push, depth + "[depth]");
            EmitPush(StackValue.BuildInt(depth));

            // The function call
            EmitInstrConsole(ProtoCore.DSASM.kw.callr, procNode.Name);

            if (isGetter)
            {
                EmitCall(procNode.ID, blockId, type, 
                         Constants.kInvalidIndex, Constants.kInvalidIndex,
                         Constants.kInvalidIndex, Constants.kInvalidIndex, 
                         procNode.PC);
            }
            // Break at function call inside dynamic lang block created for a 'true' or 'false' expression inside an inline conditional
            else if (core.DebuggerProperties.breakOptions.HasFlag(DebugProperties.BreakpointOptions.EmitInlineConditionalBreakpoint))
            {
                ProtoCore.CodeModel.CodePoint startInclusive = core.DebuggerProperties.highlightRange.StartInclusive;
                ProtoCore.CodeModel.CodePoint endExclusive = core.DebuggerProperties.highlightRange.EndExclusive;

                EmitCall(procNode.ID, blockId, type, startInclusive.LineNo, startInclusive.CharNo, endExclusive.LineNo, endExclusive.CharNo, procNode.PC);
            }
            else if (parentExpression != null)
            {
                EmitCall(procNode.ID, blockId, type, parentExpression.line, parentExpression.col, parentExpression.endLine, parentExpression.endCol, procNode.PC);
            }
            else
            {
                EmitCall(procNode.ID, blockId, type, funcCall.line, funcCall.col, funcCall.endLine, funcCall.endCol, procNode.PC);
            }

            // The function return value
            EmitInstrConsole(ProtoCore.DSASM.kw.push, ProtoCore.DSASM.kw.regRX);
            StackValue opReturn = StackValue.BuildRegister(Registers.RX);
            EmitPush(opReturn);
        }
        
        
        private void TraverseDotCallArguments(FunctionDotCallNode dotCall,
                                              ProcedureNode procCallNode,
                                              List<ProtoCore.Type> arglist,
                                              string procName,
                                              int classIndex,
                                              string className,
                                              bool isStaticCall,
                                              bool isConstructor,
                                              GraphNode graphNode,
                                              ProtoCore.CompilerDefinitions.SubCompilePass subPass,
                                              BinaryExpressionNode bnode)
        {
            // Update graph dependencies
            if (subPass != ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier && graphNode != null)
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
            for (int n = 0; n < dotCall.Arguments.Count; ++n)
            {
                if (isStaticCall || isConstructor)
                {
                    if (n != Constants.kDotArgIndexArrayArgs)
                    {
                        continue;
                    }
                }

                AssociativeNode paramNode = dotCall.Arguments[n];
                ProtoCore.Type paramType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 0);

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
                        defaultArgNumber = procCallNode.ArgumentInfos.Count - dotCall.FunctionCall.FormalArguments.Count;
                    }

                    // Enable graphnode dependencies if its a setter method
                    bool allowDependentState = null != graphNode ? graphNode.allowDependents : false;
                    if (CoreUtils.IsSetter(procName))
                    {
                        // If the arguments are not temporaries
                        ExprListNode exprList = paramNode as ExprListNode;
                        Validity.Assert(1 == exprList.Exprs.Count);

                        string varname = string.Empty;
                        if (exprList.Exprs[0] is IdentifierNode)
                        {
                            varname = (exprList.Exprs[0] as IdentifierNode).Name;

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

                        if (subPass != ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier)
                        {
                            for (int i = 0; i < defaultArgNumber; i++)
                            {
                                exprList.Exprs.Add(new DefaultArgNode());
                            }

                        }

                        if (!isStaticCall && !isConstructor)
                        {
                            DfsTraverse(paramNode, ref paramType, false, graphNode, subPass);
                            funtionArgCount = exprList.Exprs.Count;
                        }
                        else
                        {
                            foreach (AssociativeNode exprListNode in exprList.Exprs)
                            {
                                bool repGuideState = emitReplicationGuide;
                                if (subPass == ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier)
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
                            if (ProtoCore.CompilerDefinitions.SubCompilePass.None == subPass)
                            {
                                exprList.Exprs.Insert(0, new DynamicNode());
                            }
                        }

                        if (exprList.Exprs.Count > 0)
                        {
                            foreach (AssociativeNode exprListNode in exprList.Exprs)
                            {
                                bool repGuideState = emitReplicationGuide;
                                if (subPass == ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier)
                                {
                                    emitReplicationGuide = false;
                                }

                                DfsTraverse(exprListNode, ref paramType, false, graphNode, subPass, bnode);
                                emitReplicationGuide = repGuideState;

                                arglist.Add(paramType);
                            }

                            if (subPass != ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier &&
                                !isStaticCall &&
                                !isConstructor)
                            {
                                EmitInstrConsole(ProtoCore.DSASM.kw.newarr, exprList.Exprs.Count.ToString());
                                EmitPopArray(exprList.Exprs.Count);

                                if (exprList.ArrayDimensions != null)
                                {
                                    int dimensions = DfsEmitArrayIndexHeap(exprList.ArrayDimensions, graphNode);
                                    EmitPushDimensions(dimensions);
                                    EmitLoadElement(null, Constants.kInvalidIndex);
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

                        funtionArgCount = exprList.Exprs.Count;
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
                                SubCompilePass subPass,
                                BinaryExpressionNode bnode,
                                ref ProtoCore.Type inferedType)
        {
            ProcedureNode procCallNode = null;

            var dotCallType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 0);

            bool isConstructor = false;
            bool isStaticCall = false;
            bool isUnresolvedDot = false;

            int classIndex = Constants.kInvalidIndex;
            string className = string.Empty;

            var dotCall = new FunctionDotCallNode(node as FunctionDotCallNode);
            // procName is get_X for dot-call node "a.X" and is X for "a.X()"
            var procName = dotCall.FunctionCall.Function.Name;

            var firstArgument = dotCall.Arguments[0];
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
                        // this means that the lhs is a variable (class instance)
                        toResolveMethodOnClass = false;
                        classIndex = Constants.kInvalidIndex;
                        className = string.Empty;
                    }
                }

                if (toResolveMethodOnClass)
                {
                    dotCall.Arguments[0] = new IntNode(classIndex);
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
                    //    y = Bar.bar;   // static property or function pointer
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

                    //    y = Bar.bar;   // static property getter OR function pointer
                    //    Bar.bar_2 = z; // static property setter
                    string property;
                    if (CoreUtils.TryGetPropertyName(procName, out property))
                    {
                        procCallNode = classNode.GetFirstStaticFunctionBy(procName);
                        isStaticCall = procCallNode != null;

                        // function pointer to non-static property, e.g. p = Point.X;
                        int argCount = dotCall.FunctionCall.FormalArguments.Count;
                        if (procCallNode != null && argCount == 0
                            && CoreUtils.IsNonStaticPropertyLookupOnClass(procCallNode, className))
                        {
                            if (subPass != SubCompilePass.None)
                            {
                                return null;
                            }
                            EmitFunctionPointer(procCallNode);

                            return null;
                        }

                        if (procCallNode == null)
                        {
                            if (subPass != SubCompilePass.None)
                            {
                                return null;
                            }

                            // Try static function first
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

                                buildStatus.LogWarning(WarningID.CallingNonStaticMethodOnClass,
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
                            // This checks if there is a static property like Point.X(arg) 
                            // and if so renames it to Point.get_X(arg) so that it can be 
                            // found as a static getter in the class declaration.
                            if (argCount == 1)
                            {
                                procName = Constants.kGetterPrefix + procName;
                                procCallNode = classNode.GetFirstStaticFunctionBy(procName);
                                isStaticCall = procCallNode != null;
                            }
                            if (procCallNode == null && subPass == SubCompilePass.None)
                            {
                                string message = String.Format(ProtoCore.Properties.Resources.kStaticMethodNotFound,
                                    className,
                                    procName);

                                buildStatus.LogWarning(WarningID.FunctionNotFound,
                                    message,
                                    core.CurrentDSFileName,
                                    dotCall.line,
                                    dotCall.col,
                                    graphNode);

                                EmitNullNode(new NullNode(), ref inferedType);

                                return null;
                            }
                        }
                    }
                }
                else if (hasAllocated && symbolnode.datatype.UID != (int)PrimitiveType.Var)
                {
                    inferedType.UID = symbolnode.datatype.UID;
                    if (Constants.kInvalidIndex != inferedType.UID)
                    {
                        procCallNode = GetProcedureFromInstance(symbolnode.datatype.UID, dotCall.FunctionCall);
                    }

                    if (null != procCallNode)
                    {
                        if (procCallNode.IsConstructor)
                        {
                            if (subPass == ProtoCore.CompilerDefinitions.SubCompilePass.None)
                            {
                                string message = String.Format(ProtoCore.Properties.Resources.KCallingConstructorOnInstance,
                                                               procName);
                                buildStatus.LogWarning(WarningID.CallingConstructorOnInstance,
                                                       message,
                                                       core.CurrentDSFileName,
                                                       -1,
                                                       -1, 
                                                       graphNode);
                                EmitNullNode(new NullNode(), ref inferedType);
                            }
                            return null;
                        }

                        isAccessible = procCallNode.AccessModifier == ProtoCore.CompilerDefinitions.AccessModifier.Public
                             || (procCallNode.AccessModifier == ProtoCore.CompilerDefinitions.AccessModifier.Private && procCallNode.ClassID == globalClassIndex);

                        if (!isAccessible)
                        {
                            if (subPass == ProtoCore.CompilerDefinitions.SubCompilePass.None)
                            {
                                string message = String.Format(ProtoCore.Properties.Resources.kMethodIsInaccessible, procName);
                                buildStatus.LogWarning(ProtoCore.BuildData.WarningID.AccessViolation, message, core.CurrentDSFileName, dotCall.line, dotCall.col, graphNode);
                            }
                        }

                        var dynamicRhsIndex = (int)(dotCall.Arguments[1] as IntNode).Value;
                        var dynFunc = core.DynamicFunctionTable.GetFunctionAtIndex(dynamicRhsIndex);
                        dynFunc.ClassIndex = procCallNode.ClassID;
                    }
                }
                else
                {
                    isUnresolvedDot = true;
                }
            }

            // Its an accceptable method at this point
            List<ProtoCore.Type> arglist = new List<ProtoCore.Type>();
            TraverseDotCallArguments(dotCall,
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

            if (subPass == ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier)
            {
                return null;
            }

            if (!isConstructor && !isStaticCall)
            {
                Validity.Assert(dotCall.Arguments[ProtoCore.DSASM.Constants.kDotArgIndexArrayArgs] is ExprListNode);
                ExprListNode functionArgs = dotCall.Arguments[ProtoCore.DSASM.Constants.kDotArgIndexArrayArgs] as ExprListNode;
                functionArgs.Exprs.Insert(0, dotCall.Arguments[ProtoCore.DSASM.Constants.kDotArgIndexPtr]);
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
                && (int)PrimitiveType.InvalidType != inferedType.UID
                && (int)PrimitiveType.Void != inferedType.UID
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
                procCallNode = CoreUtils.GetFunctionBySignature(procName, arglist, codeBlock);
                if (null != procCallNode)
                {
                    type = Constants.kGlobalScope;
                    if (core.TypeSystem.IsHigherRank(procCallNode.ReturnType.UID, inferedType.UID))
                    {
                        inferedType = procCallNode.ReturnType;
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
                        inferedType = procCallNode.ReturnType;
                        type = realType;

                        if (!isAccessible)
                        {
                            string message = String.Format(ProtoCore.Properties.Resources.kMethodIsInaccessible, procName);
                            buildStatus.LogWarning(WarningID.AccessViolation, message, core.CurrentDSFileName, dotCall.line, dotCall.col, graphNode);

                            inferedType.UID = (int)PrimitiveType.Null;
                            EmitPushNull();
                            return procCallNode;
                        }
                    }
                }
            }

            if (isUnresolvedDot || procCallNode == null)
            {
                if (dotCallType.UID != (int)PrimitiveType.Var)
                {
                    inferedType.UID = dotCallType.UID;
                }

                var procNode = CoreUtils.GetFunctionByName(Constants.kDotMethodName, codeBlock);
                if (CoreUtils.IsGetter(procName))
                {
                    EmitFunctionCall(depth, type, arglist, procNode, dotCall, true);
                }
                else
                {
                    EmitFunctionCall(depth, type, arglist, procNode, dotCall, false, bnode);
                }
                return procNode;
            }
            else
            {
                if (procCallNode.IsConstructor &&
                    (globalClassIndex != Constants.kInvalidIndex) &&
                    (globalProcIndex != Constants.kInvalidIndex) &&
                    (globalClassIndex == inferedType.UID))
                {
                    ProcedureNode contextProcNode = core.ClassTable.ClassNodes[globalClassIndex].ProcTable.Procedures[globalProcIndex];
                    if (contextProcNode.IsConstructor &&
                        string.Equals(contextProcNode.Name, procCallNode.Name) &&
                        contextProcNode.RuntimeIndex == procCallNode.RuntimeIndex)
                    {
                        string message = String.Format(ProtoCore.Properties.Resources.kCallingConstructorInConstructor, procName);
                        buildStatus.LogWarning(WarningID.CallingConstructorInConstructor, message, core.CurrentDSFileName, node.line, node.col, graphNode);
                        inferedType.UID = (int)PrimitiveType.Null;
                        EmitPushNull();
                        return procCallNode;
                    }
                }

                inferedType = procCallNode.ReturnType;

                // Get the dot call procedure
                if (isConstructor || isStaticCall)
                {
                    bool isGetter = CoreUtils.IsGetter(procName);
                    EmitFunctionCall(depth, procCallNode.ClassID, arglist, procCallNode, dotCall, isGetter, bnode);
                }
                else
                {
                    var procNode = CoreUtils.GetFunctionByName(Constants.kDotMethodName, codeBlock);
                    if (CoreUtils.IsSetter(procName))
                    {
                        EmitFunctionCall(depth, type, arglist, procNode, dotCall);
                    }
                    // Do not emit breakpoint at getters only - pratapa
                    else if (CoreUtils.IsGetter(procName))
                    {
                        EmitFunctionCall(depth, type, arglist, procNode, dotCall, true);
                    }
                    else
                    {
                        EmitFunctionCall(depth, type, arglist, procNode, dotCall, false, bnode);
                    }

                    if (dotCallType.UID != (int)PrimitiveType.Var)
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
            SubCompilePass subPass = SubCompilePass.None,                 
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
                    GenerateCallsiteIdentifierForGraphNode(graphNode, procNode.Name);
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
                if (constructor != null && constructor.IsConstructor)
                {
                    var rhsFNode = node as FunctionCallNode;
                    var classNode = AstFactory.BuildIdentifier(procName);
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
                        GenerateCallsiteIdentifierForGraphNode(graphNode, procNode.Name);
                    }
                    return procNode;
                }
            }

            foreach (AssociativeNode paramNode in funcCall.FormalArguments)
            {
                var paramType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 0);

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
           
            if (subPass == ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier)
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
            if ((int)PrimitiveType.InvalidType != inferedType.UID
                && (int)PrimitiveType.Void != inferedType.UID
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
                        buildStatus.LogWarning(WarningID.AccessViolation, message, core.CurrentDSFileName, funcCall.line, funcCall.col, graphNode);
                        inferedType.UID = (int)PrimitiveType.Null;

                        EmitPushNull();
                        if (graphNode != null && procNode != null)
                        {
                            GenerateCallsiteIdentifierForGraphNode(graphNode, procNode.Name);
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
                procNode = CoreUtils.GetFunctionBySignature(procName, arglist, codeBlock);
                if (null != procNode)
                {
                    type = ProtoCore.DSASM.Constants.kGlobalScope;
                    if (core.TypeSystem.IsHigherRank(procNode.ReturnType.UID, inferedType.UID))
                    {
                        inferedType = procNode.ReturnType;
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
                        inferedType = procNode.ReturnType;
                        type = realType;

                        if (!isAccessible)
                        {
                            string message = String.Format(ProtoCore.Properties.Resources.kMethodIsInaccessible, procName);
                            buildStatus.LogWarning(WarningID.AccessViolation, message, core.CurrentDSFileName, funcCall.line, funcCall.col, graphNode);

                            inferedType.UID = (int)PrimitiveType.Null;
                            EmitPushNull();
                            if (graphNode != null && procNode != null)
                            {
                                GenerateCallsiteIdentifierForGraphNode(graphNode, procNode.Name);
                            }
                            return procNode;
                        }
                    }
                }
            }

            if (null != procNode)
            {
                if (procNode.IsConstructor && 
                    (globalClassIndex != Constants.kInvalidIndex) && 
                    (globalProcIndex != Constants.kInvalidIndex) && 
                    (globalClassIndex == inferedType.UID))
                {
                    ProcedureNode contextProcNode = core.ClassTable.ClassNodes[globalClassIndex].ProcTable.Procedures[globalProcIndex];
                    if (contextProcNode.IsConstructor &&
                        string.Equals(contextProcNode.Name, procNode.Name) &&
                        contextProcNode.RuntimeIndex == procNode.RuntimeIndex)
                    {
                        string message = String.Format(ProtoCore.Properties.Resources.kCallingConstructorInConstructor, procName);
                        buildStatus.LogWarning(WarningID.CallingConstructorInConstructor, message, core.CurrentDSFileName, node.line, node.col, graphNode);
                        inferedType.UID = (int)PrimitiveType.Null;
                        EmitPushNull();
                        if (graphNode != null && procNode != null)
                        {
                            GenerateCallsiteIdentifierForGraphNode(graphNode, procNode.Name);
                        }
                        return procNode;
                    }
                }

                inferedType = procNode.ReturnType;

                if (procNode.ID != Constants.kInvalidIndex)
                {
                    foreach (AssociativeNode paramNode in funcCall.FormalArguments)
                    {
                        // Get the lhs symbol list
                        ProtoCore.Type ltype = new ProtoCore.Type();
                        ltype.UID = globalClassIndex;
                        UpdateNodeRef argNodeRef = new UpdateNodeRef();
                        DFSGetSymbolList(paramNode, ref ltype, argNodeRef);
                    }

                    // The function is at block 0 if its a constructor, member 
                    // or at the globals scope.  Its at block 1 if its inside a 
                    // language block. Its limited to block 1 as of R1 since we 
                    // dont support nested function declarations yet
                    int blockId = procNode.RuntimeIndex;

                    //push value-not-provided default argument
                    for (int i = arglist.Count; i < procNode.ArgumentInfos.Count; i++)
                    {
                        EmitDefaultArgNode();
                    }
                    
                    // Emit depth
                    EmitInstrConsole(kw.push, depth + "[depth]");
                    EmitPush(StackValue.BuildInt(depth));

                    // The function call
                    EmitInstrConsole(kw.callr, procNode.Name);

                    // Do not emit breakpoints at built-in methods like _add/_sub etc. - pratapa
                    if (procNode.IsAssocOperator || procNode.Name.Equals(Constants.kInlineConditionalMethodName))
                    {
                        EmitCall(procNode.ID, 
                                 blockId,
                                 type, 
                                 Constants.kInvalidIndex, 
                                 Constants.kInvalidIndex, 
                                 Constants.kInvalidIndex, 
                                 Constants.kInvalidIndex, 
                                 procNode.PC);
                    }
                    // Break at function call inside dynamic lang block created for a 'true' or 'false' expression inside an inline conditional
                    else if (core.DebuggerProperties.breakOptions.HasFlag(DebugProperties.BreakpointOptions.EmitInlineConditionalBreakpoint))
                    {
                        var codeRange = core.DebuggerProperties.highlightRange;

                        var startInclusive = codeRange.StartInclusive;
                        var endExclusive = codeRange.EndExclusive;

                        EmitCall(procNode.ID, 
                                 blockId,
                                 type, 
                                 startInclusive.LineNo, 
                                 startInclusive.CharNo, 
                                 endExclusive.LineNo, 
                                 endExclusive.CharNo, 
                                 procNode.PC);
                    }
                    // Use startCol and endCol of binary expression node containing function call except if it's a setter
                    else if (bnode != null && !procNode.Name.StartsWith(Constants.kSetterPrefix))
                    {
                        EmitCall(procNode.ID, 
                                 blockId,
                                 type, 
                                 bnode.line, 
                                 bnode.col, 
                                 bnode.endLine, 
                                 bnode.endCol, 
                                 procNode.PC);
                    }
                    else
                    {
                        EmitCall(procNode.ID, 
                                 blockId,
                                 type, 
                                 funcCall.line, 
                                 funcCall.col, 
                                 funcCall.endLine, 
                                 funcCall.endCol, 
                                 procNode.PC);
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
                        buildStatus.LogWarning(ProtoCore.BuildData.WarningID.PropertyNotFound, message, core.CurrentDSFileName, funcCall.line, funcCall.col, graphNode);
                    }
                    else
                    {
                        string message = String.Format(ProtoCore.Properties.Resources.kMethodNotFound, procName);
                        buildStatus.LogWarning(ProtoCore.BuildData.WarningID.FunctionNotFound, message, core.CurrentDSFileName, funcCall.line, funcCall.col, graphNode);
                    }

                    inferedType.UID = (int)PrimitiveType.Null;
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
                        var iNode =AstFactory.BuildIdentifier(funcCall.Function.Name);
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
                    inferedType.UID = (int)PrimitiveType.Var;
                }
            }

            if (graphNode != null && procNode != null)
            {
                GenerateCallsiteIdentifierForGraphNode(graphNode, procNode.Name);
            }
            
            return procNode;
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
                var guides = ((ArrayNameNode) node).ReplicationGuides;
                if (guides == null || guides.Count == 0)
                {
                    if (node is IdentifierListNode)
                    {
                        var identListNode = node as IdentifierListNode;
                        return GetReplicationGuides(identListNode.RightNode);
                    }
                }
                return guides;
            }
            
            if (node is FunctionDotCallNode)
            {
                var dotCallNode = node as FunctionDotCallNode;
                return GetReplicationGuides(dotCallNode.FunctionCall.Function);
            }

            return null;
        }

        private AtLevelNode GetAtLevel(AssociativeNode node)
        {
            var n1 = node as ArrayNameNode;
            if (n1 != null)
            {
                return n1.AtLevel;
            }

            var n2 = node as IdentifierListNode;
            if (n2 != null)
            {
                return GetAtLevel(n2.RightNode);
            }

            var n3 = node as FunctionDotCallNode;
            if (n3 != null)
            {
                return GetAtLevel(n3.FunctionCall.Function);
            }

            return null;
        }

        private void RemoveAtLevel(AssociativeNode node)
        {
            var n1 = node as ArrayNameNode;
            if (n1 != null)
            {
                n1.AtLevel = null;
                return;
            }

            var n2 = node as IdentifierListNode;
            if (n2 != null)
            {
                RemoveAtLevel(n2.RightNode);
                return;
            }

            var n3 = node as FunctionDotCallNode;
            if (n3 != null)
            {
                RemoveAtLevel(n3.FunctionCall.Function);
                return;
            }
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

        private AssociativeNode DFSEmitSplitAssign_AST(AssociativeNode node, ref List<AssociativeNode> astList)
        {
            Validity.Assert(null != astList && null != node);

            if (node.Kind == AstKind.BinaryExpression)
            {
                BinaryExpressionNode bnode = node as BinaryExpressionNode;
                if (ProtoCore.DSASM.Operator.assign == bnode.Optr)
                {
                    AssociativeNode lastNode = DFSEmitSplitAssign_AST(bnode.RightNode, ref astList);
                    var newBNode = AstFactory.BuildBinaryExpression(bnode.LeftNode, lastNode, Operator.assign);
                    newBNode.OriginalAstID = bnode.OriginalAstID;

                    astList.Add(newBNode);
                    return bnode.LeftNode;
                }
            }
            return node;
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

        private int EmitExpressionInterpreter(ProtoCore.AST.Node codeBlockNode)
        {
            core.watchStartPC = this.pc;
            compilePass = ProtoCore.CompilerDefinitions.CompilePass.GlobalScope;
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
                    localProcedure = core.ClassTable.ClassNodes[globalClassIndex].ProcTable.Procedures[globalProcIndex];
                else
                {
                    // TODO: to investigate whethter to use the table under executable or in core.FuncTable - Randy, Jun
                    localProcedure = core.DSExecutable.procedureTable[functionBlock].Procedures[globalProcIndex];
                }
            }

            foreach (AssociativeNode node in codeblock.Body)
            {
                inferedType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 0);

                DfsTraverse(node, ref inferedType, false, null, ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier);
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
                    Allocate(Constants.kInvalidIndex, Constants.kInvalidIndex, Constants.kGlobalScope, globalName, type);
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
            if (core.Options.RunMode == ProtoCore.DSASM.InterpreterMode.Expression)
            {
                return EmitExpressionInterpreter(codeBlockNode);
            }

            this.localCodeBlockNode = codeBlockNode;
            ProtoCore.AST.AssociativeAST.CodeBlockNode codeblock = codeBlockNode as ProtoCore.AST.AssociativeAST.CodeBlockNode;
            bool isTopBlock = null == codeBlock.parent;
            if (!isTopBlock)
            {
                // If this is an inner block where there can be no classes, we can start at parsing at the global function state
                compilePass = ProtoCore.CompilerDefinitions.CompilePass.GlobalScope;
            }

            codeblock.Body = ApplyTransform(codeblock.Body);
            
            bool hasReturnStatement = false;
            ProtoCore.Type inferedType = new ProtoCore.Type();
            bool ssaTransformed = false;
            while (ProtoCore.CompilerDefinitions.CompilePass.Done != compilePass)
            {
                // Emit SSA only after generating the class definitions
                if (core.Options.GenerateSSA)
                {
                    if (compilePass > ProtoCore.CompilerDefinitions.CompilePass.ClassName && !ssaTransformed)
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

                inferedType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 0);
                hasReturnStatement = EmitCodeBlock(codeblock.Body, ref inferedType, ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier, false);
                if (compilePass == ProtoCore.CompilerDefinitions.CompilePass.GlobalScope && !hasReturnStatement)
                {
                    EmitReturnNull(); 
                }

                compilePass++;
            }

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

        public int AllocateMemberVariable(int classIndex, int classScope, string name, int rank = 0, ProtoCore.CompilerDefinitions.AccessModifier access = ProtoCore.CompilerDefinitions.AccessModifier.Public, bool isStatic = false)
        {
            // TODO Jun: Create a class table for holding the primitive and custom data types
            int datasize = ProtoCore.DSASM.Constants.kPointerSize;
            ProtoCore.Type ptrType = new ProtoCore.Type();
            if (rank == 0)
                ptrType.UID = (int)PrimitiveType.Pointer;
            else
                ptrType.UID = (int)PrimitiveType.Array;
            ptrType.rank = rank;
            ProtoCore.DSASM.SymbolNode symnode = Allocate(classIndex, classScope, ProtoCore.DSASM.Constants.kGlobalScope, name, ptrType, isStatic, access);
            if (null == symnode)
            {
                buildStatus.LogSemanticError(String.Format(Resources.MemberVariableAlreadyDefined, name, core.ClassTable.ClassNodes[classIndex].Name));
                return ProtoCore.DSASM.Constants.kInvalidIndex;
            }


            return symnode.symbolTableIndex;
        }

        private void EmitIdentifierNode(AssociativeNode node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None, BinaryExpressionNode parentNode = null)
        {
            IdentifierNode t = node as IdentifierNode;
            if (t.Name.Equals(ProtoCore.DSDefinitions.Keyword.This))
            {
                if (subPass != ProtoCore.CompilerDefinitions.SubCompilePass.None)
                {
                    return;
                }

                if (localProcedure != null)
                {
                    if (localProcedure.IsStatic)
                    {
                        string message = ProtoCore.Properties.Resources.kUsingThisInStaticFunction;
                        core.BuildStatus.LogWarning(ProtoCore.BuildData.WarningID.InvalidThis, message, core.CurrentDSFileName, t.line, t.col, graphNode);
                        EmitPushNull();
                        return;
                    }
                    else if (localProcedure.ClassID == Constants.kGlobalScope)
                    {
                        string message = ProtoCore.Properties.Resources.kInvalidThis;
                        core.BuildStatus.LogWarning(ProtoCore.BuildData.WarningID.InvalidThis, message, core.CurrentDSFileName, t.line, t.col, graphNode);
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
                    core.BuildStatus.LogWarning(ProtoCore.BuildData.WarningID.InvalidThis, message, core.CurrentDSFileName, t.line, t.col, graphNode);
                    EmitPushNull();
                    return;
                }
            }

            int dimensions = 0;

            ProtoCore.DSASM.SymbolNode symbolnode = null;
            int runtimeIndex = codeBlock.symbolTable.RuntimeIndex;

            ProtoCore.Type type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 0);

            bool isAccessible = false;
            bool isAllocated = VerifyAllocation(t.Name, globalClassIndex, globalProcIndex, out symbolnode, out isAccessible);
            if (!isAllocated && null == t.ArrayDimensions)
            {
                //check if it is a function instance
                ProtoCore.DSASM.ProcedureNode procNode = null;
                procNode = CoreUtils.GetFunctionByName(t.Name, codeBlock);
                if (null != procNode)
                {
                    if (ProtoCore.DSASM.Constants.kInvalidIndex != procNode.ID)
                    {
                        // A global function
                        inferedType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.FunctionPointer, 0);
                        if (ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier != subPass)
                        {
                            int fptr = core.FunctionPointerTable.functionPointerDictionary.Count;
                            var fptrNode = new FunctionPointerNode(procNode);
                            core.FunctionPointerTable.functionPointerDictionary.TryAdd(fptr, fptrNode);
                            core.FunctionPointerTable.functionPointerDictionary.TryGetBySecond(fptrNode, out fptr);

                            EmitInstrConsole(ProtoCore.DSASM.kw.push, t.Name);
                            StackValue opFunctionPointer = StackValue.BuildFunctionPointer(fptr);
                            EmitPush(opFunctionPointer, runtimeIndex, t.line, t.col);
                        }
                        return;
                    }
         
                }
            }            

            if (ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier == subPass)
            {
                if (symbolnode == null)
                {
                    // The variable is unbound
                    ProtoCore.DSASM.SymbolNode unboundVariable = null;
                    if (ProtoCore.DSASM.InterpreterMode.Expression != core.Options.RunMode)
                    {
                        inferedType.UID = (int)ProtoCore.PrimitiveType.Null;

                        // Jun Comment: Specification 
                        //      If resolution fails at this point a com.Design-Script.Imperative.Core.UnboundIdentifier 
                        //      warning is emitted during pre-execute phase, and at the ID is bound to null. (R1 - Feb)

                        // Set the first symbol that triggers the cycle to null
                        ProtoCore.AssociativeGraph.GraphNode nullAssignGraphNode = new ProtoCore.AssociativeGraph.GraphNode();
                        nullAssignGraphNode.updateBlock.startpc = pc;

                        EmitPushNull();

                        // Push the identifier local block  
                        ProtoCore.Type varType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 0);

                        // TODO Jun: Refactor Allocate() to just return the symbol node itself
                        unboundVariable = Allocate(globalClassIndex, globalClassIndex, globalProcIndex, t.Value, varType, line: t.line, col: t.col); 
                        Validity.Assert(unboundVariable != null);

                        int symbolindex = unboundVariable.symbolTableIndex;
                        if (ProtoCore.DSASM.Constants.kInvalidIndex != globalClassIndex)
                        {
                            symbolnode = core.ClassTable.ClassNodes[globalClassIndex].Symbols.symbolList[symbolindex];
                        }
                        else
                        {
                            symbolnode = codeBlock.symbolTable.symbolList[symbolindex];
                        }

                        EmitInstrConsole(kw.pop, t.Value);
                        EmitPopForSymbol(unboundVariable, runtimeIndex);

                        nullAssignGraphNode.PushSymbolReference(symbolnode);
                        nullAssignGraphNode.procIndex = globalProcIndex;
                        nullAssignGraphNode.classIndex = globalClassIndex;
                        nullAssignGraphNode.updateBlock.endpc = pc - 1;

                        PushGraphNode(nullAssignGraphNode);
                        EmitDependency(ProtoCore.DSASM.Constants.kInvalidIndex, false);
                    }

                    if (isAllocated)
                    {
                        string message = String.Format(ProtoCore.Properties.Resources.kPropertyIsInaccessible, t.Value);
                        if (localProcedure != null && localProcedure.IsStatic)
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
                            WarningID.AccessViolation,
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
                if (core.Options.RunMode == ProtoCore.DSASM.InterpreterMode.Expression &&
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
                    updateNode.nodeType = UpdateNodeType.Symbol;

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
                    if (!string.Equals(localProcedure.Name, getterName))
                    {
                        var thisNode =AstFactory.BuildIdentifier(ProtoCore.DSDefinitions.Keyword.This);
                        var identListNode = AstFactory.BuildIdentList(thisNode, t);
                        EmitIdentifierListNode(identListNode, ref inferedType, false, graphNode, ProtoCore.CompilerDefinitions.SubCompilePass.None);

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

                if (ProtoCore.DSASM.InterpreterMode.Expression == core.Options.RunMode)
                {
                    EmitInstrConsole(ProtoCore.DSASM.kw.pushw, t.Value);
                    EmitPushForSymbolW(symbolnode, runtimeIndex, t.line, t.col);
                }
                else
                {
                    EmitInstrConsole(kw.push, t.Value);
                    EmitPushForSymbol(symbolnode, runtimeIndex, t);
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


                if (ProtoCore.DSASM.InterpreterMode.Expression != core.Options.RunMode)
                {
                    if (dimensions > 0)
                    {
                        EmitPushDimensions(dimensions);
                        EmitLoadElement(symbolnode, runtimeIndex);
                    }

                    if (emitReplicationGuide)
                    {
                        EmitAtLevel(t.AtLevel);
                        EmitReplicationGuides(t.ReplicationGuides);
                    }
                }

                if (core.TypeSystem.IsHigherRank(type.UID, inferedType.UID))
                {
                    inferedType = type;
                }
                // We need to get inferedType for boolean variable so that we can perform type check
                inferedType.UID = (isBooleanOp || (type.UID == (int)PrimitiveType.Bool)) ? (int)PrimitiveType.Bool : inferedType.UID;
            }

        }

        private void EmitRangeExprNode(AssociativeNode node, 
                                       ref ProtoCore.Type inferedType, 
                                       GraphNode graphNode = null,
                                       ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None)
        {
            RangeExprNode range = node as RangeExprNode;

            // Do some static checking...
            var fromNode = range.From;
            var toNode = range.To;
            var stepNode = range.Step;
            var stepOp = range.StepOperator;
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
                    case ProtoCore.DSASM.RangeStepOperator.StepSize:

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

                    case ProtoCore.DSASM.RangeStepOperator.Number:

                       if (hasAmountOperator)
                       {
                           isStepValid = false;
                           warningMsg = ProtoCore.Properties.Resources.kRangeExpressionWithStepSizeZero;
                       }
                       else
                       {
                           if (stepNode != null && stepNode is DoubleNode &&
                               subPass == ProtoCore.CompilerDefinitions.SubCompilePass.None)
                           {
                               buildStatus.LogWarning(WarningID.InvalidRangeExpression,
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

                    case ProtoCore.DSASM.RangeStepOperator.ApproximateSize:
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

            if (!isStepValid && subPass == ProtoCore.CompilerDefinitions.SubCompilePass.None)
            {
                buildStatus.LogWarning(WarningID.InvalidRangeExpression,
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
                case RangeStepOperator.StepSize:
                    op = new IntNode(0);
                    break;
                case RangeStepOperator.Number:
                    op = new IntNode(1);
                    break;
                case RangeStepOperator.ApproximateSize:
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

            if (subPass != ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier)
            {
                if (range.ArrayDimensions != null)
                {
                    int dimensions = DfsEmitArrayIndexHeap(range.ArrayDimensions, graphNode);
                    EmitPushDimensions(dimensions);
                    EmitLoadElement(null, Constants.kInvalidIndex);
                }

                if (emitReplicationGuide)
                {
                    EmitAtLevel(range.AtLevel);
                    EmitReplicationGuides(range.ReplicationGuides);
                }
            }
        }

        private void EmitLanguageBlockNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.AssociativeGraph.GraphNode graphNode, ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None)
        {
            if (IsParsingGlobal() || IsParsingGlobalFunctionBody() || IsParsingMemberFunctionBody() )
            {
                if (subPass == ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier)
                {
                    return;
                }

                LanguageBlockNode langblock = node as LanguageBlockNode;

                //Validity.Assert(ProtoCore.Language.kInvalid != langblock.codeblock.language);
                if (ProtoCore.Language.NotSpecified == langblock.codeblock.Language)
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
                if (ProtoCore.Language.Associative == langblock.codeblock.Language && !isTopBlock)
                {
                    // TODO Jun: Move the associative and all common string into some table
                    buildStatus.LogSyntaxError(Resources.InvalidNestedAssociativeBlock, core.CurrentDSFileName, langblock.line, langblock.col);
                }


                // Set the current class scope so the next language can refer to it
                core.ClassIndex = globalClassIndex;

                if (globalProcIndex != ProtoCore.DSASM.Constants.kInvalidIndex && core.ProcNode == null)
                {
                    if (globalClassIndex != ProtoCore.DSASM.Constants.kInvalidIndex)
                        core.ProcNode = core.ClassTable.ClassNodes[globalClassIndex].ProcTable.Procedures[globalProcIndex];
                    else
                        core.ProcNode = codeBlock.procedureTable.Procedures[globalProcIndex];
                }

                ProtoCore.AssociativeGraph.GraphNode propagateGraphNode = null;
                if (core.Options.AssociativeToImperativePropagation && Language.Imperative == langblock.codeblock.Language)
                {
                    propagateGraphNode = graphNode;
                }

                core.Compilers[langblock.codeblock.Language].Compile(out blockId, codeBlock, langblock.codeblock, nextContext, codeBlock.EventSink, langblock.CodeBlockNode, propagateGraphNode);
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

                EmitInstrConsole(ProtoCore.DSASM.kw.bounce + " " + blockId + ", " + entry.ToString());
                EmitBounceIntrinsic(blockId, entry);


                // The callee language block will have stored its result into the RX register. 
                EmitInstrConsole(ProtoCore.DSASM.kw.push, ProtoCore.DSASM.kw.regRX);
                StackValue opRes = StackValue.BuildRegister(Registers.RX);
                EmitPush(opRes);
            }
        }

        private void EmitDynamicLanguageBlockNode(AssociativeNode node, AssociativeNode singleBody, ref ProtoCore.Type inferedType, ref int blockId, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None)
        {
            if (IsParsingGlobal() || IsParsingGlobalFunctionBody() || IsParsingMemberFunctionBody())
            {
                if (subPass == ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier)
                {
                    return;
                }

                LanguageBlockNode langblock = node as LanguageBlockNode;

                //Validity.Assert(ProtoCore.Language.kInvalid != langblock.codeblock.language);
                if (ProtoCore.Language.NotSpecified == langblock.codeblock.Language)
                    throw new BuildHaltException("Invalid language block type (B1C57E37)");

                ProtoCore.CompileTime.Context context = new ProtoCore.CompileTime.Context();

                // Set the current class scope so the next language can refer to it
                core.ClassIndex = globalClassIndex;

                if (globalProcIndex != ProtoCore.DSASM.Constants.kInvalidIndex && core.ProcNode == null)
                {
                    if (globalClassIndex != ProtoCore.DSASM.Constants.kInvalidIndex)
                        core.ProcNode = core.ClassTable.ClassNodes[globalClassIndex].ProcTable.Procedures[globalProcIndex];
                    else
                        core.ProcNode = codeBlock.procedureTable.Procedures[globalProcIndex];
                }

                core.Compilers[langblock.codeblock.Language].Compile(
                    out blockId, codeBlock, langblock.codeblock, context, codeBlock.EventSink, langblock.CodeBlockNode, null);
                
            }
        }

        private void EmitGetterForProperty(ProtoCore.AST.AssociativeAST.ClassDeclNode cnode, ProtoCore.DSASM.SymbolNode prop)
        {
            FunctionDefinitionNode getter = new FunctionDefinitionNode
            {
                Name = ProtoCore.DSASM.Constants.kGetterPrefix + prop.name,
                Signature = new ArgumentSignatureNode(),
                ReturnType = prop.datatype,
                FunctionBody = new CodeBlockNode(),
                IsExternLib = false,
                ExternLibName = null,
                Access = prop.access,
                IsStatic = prop.isStatic,
                IsAutoGenerated = true
            };

            var ret = AstFactory.BuildIdentifier(ProtoCore.DSDefinitions.Keyword.Return); 
            var ident =AstFactory.BuildIdentifier(prop.name);
            var retStatement = AstFactory.BuildBinaryExpression(ret, ident, Operator.assign); 
            getter.FunctionBody.Body.Add(retStatement);
            cnode.Procedures.Add(getter);
        }

        private FunctionDefinitionNode EmitSetterFunction(ProtoCore.DSASM.SymbolNode prop, ProtoCore.Type argType)
        {
            var argument = new ProtoCore.AST.AssociativeAST.VarDeclNode()
            {
                Access = ProtoCore.CompilerDefinitions.AccessModifier.Public,
                NameNode =AstFactory.BuildIdentifier(Constants.kTempArg),
                ArgumentType = argType
            };
            var argumentSingature = new ArgumentSignatureNode();
            argumentSingature.AddArgument(argument);

            FunctionDefinitionNode setter = new FunctionDefinitionNode
            {
                Name = ProtoCore.DSASM.Constants.kSetterPrefix + prop.name,
                Signature = argumentSingature,
                ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Null, 0),
                FunctionBody = new CodeBlockNode(),
                IsExternLib = false,
                ExternLibName = null,
                Access = prop.access,
                IsStatic = prop.isStatic,
                IsAutoGenerated = true
            };

            // property = %tmpArg
            var propIdent = new TypedIdentifierNode();
            propIdent.Name = propIdent.Value = prop.name;
            propIdent.datatype = prop.datatype;

            var tmpArg =AstFactory.BuildIdentifier(Constants.kTempArg);
            var assignment = AstFactory.BuildBinaryExpression(propIdent, tmpArg, Operator.assign);
            setter.FunctionBody.Body.Add(assignment);

            // return = null;
            var returnNull = AstFactory.BuildReturnStatement(new NullNode()); 
            setter.FunctionBody.Body.Add(returnNull);

            return setter;
        }

        private void EmitSetterForProperty(ProtoCore.AST.AssociativeAST.ClassDeclNode cnode, ProtoCore.DSASM.SymbolNode prop)
        {
            ProtoCore.Type varArrayType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank);
            {
                var setterForVarArray = EmitSetterFunction(prop, varArrayType);
                cnode.Procedures.Add(setterForVarArray);
            }
        }

        private void EmitMemberVariables(ClassDeclNode classDecl, GraphNode graphNode = null)
        {
            // Class member variable pass
            // Populating each class entry symbols with their respective member variables

            int thisClassIndex = core.ClassTable.GetClassId(classDecl.ClassName);
            globalClassIndex = thisClassIndex;

            if (!unPopulatedClasses.ContainsKey(thisClassIndex))
            {
                return;
            }
            ProtoCore.DSASM.ClassNode thisClass = core.ClassTable.ClassNodes[thisClassIndex];

            // Handle member vars from base class
            if (!string.IsNullOrEmpty(classDecl.BaseClass))
            {
                int baseClassIndex = core.ClassTable.GetClassId(classDecl.BaseClass);
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
                    foreach (ProtoCore.DSASM.SymbolNode symnode in baseClass.Symbols.symbolList.Values)
                    {
                        // It is a member variables
                        if (ProtoCore.DSASM.Constants.kGlobalScope == symnode.functionIndex)
                        {
                            Validity.Assert(!symnode.isArgument);
                            int symbolIndex = AllocateMemberVariable(thisClassIndex, symnode.isStatic ? symnode.classScope : baseClassIndex, symnode.name, symnode.datatype.rank, symnode.access, symnode.isStatic);

                            if (symbolIndex != ProtoCore.DSASM.Constants.kInvalidIndex)
                                thisClass.Size += ProtoCore.DSASM.Constants.kPointerSize;
                        }
                    }
                }
                else
                {
                    Validity.Assert(false, "n-pass compile error, fixme Jun....");
                }
            }

            foreach (VarDeclNode vardecl in classDecl.Variables)
            {
                IdentifierNode varIdent = vardecl.NameNode as IdentifierNode;
                int symbolIndex = AllocateMemberVariable(thisClassIndex, thisClassIndex, varIdent.Value, vardecl.ArgumentType.rank, vardecl.Access, vardecl.IsStatic);
                if (symbolIndex != Constants.kInvalidIndex && !classDecl.IsExternLib)
                {
                    ProtoCore.DSASM.SymbolNode prop =
                        vardecl.IsStatic
                        ? core.CodeBlockList[0].symbolTable.symbolList[symbolIndex]
                        : core.ClassTable.ClassNodes[thisClassIndex].Symbols.symbolList[symbolIndex];
                    string typeName = vardecl.ArgumentType.Name;
                    if (String.IsNullOrEmpty(typeName))
                    {
                        prop.datatype = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank);
                    }
                    else
                    {
                        int type = core.TypeSystem.GetType(typeName);
                        if (type == (int)PrimitiveType.InvalidType)
                        {
                            string message = String.Format(ProtoCore.Properties.Resources.kTypeUndefined, typeName);
                            core.BuildStatus.LogWarning(ProtoCore.BuildData.WarningID.TypeUndefined, message, core.CurrentDSFileName, vardecl.line, vardecl.col, graphNode);
                            prop.datatype = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 0);
                        }
                        else
                        {
                            int rank = vardecl.ArgumentType.rank;
                            prop.datatype = core.TypeSystem.BuildTypeObject(type, rank);
                        }
                    }

                    EmitGetterForProperty(classDecl, prop);
                    EmitSetterForProperty(classDecl, prop);
                }
            }
            classOffset = 0;

            unPopulatedClasses.Remove(thisClassIndex);
        }

        private void EmitClassDeclNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None,
            GraphNode graphNode = null)
        {
            ClassDeclNode classDecl = node as ClassDeclNode;

            // Handling n-pass on class declaration
            if (ProtoCore.CompilerDefinitions.CompilePass.ClassName == compilePass)
            {
                // Class name pass
                // Populating the class tables with the class names
                if (null != codeBlock.parent)
                {
                    buildStatus.LogSemanticError(Resources.ClassCannotBeDefinedInsideLanguageBlock, core.CurrentDSFileName, classDecl.line, classDecl.col);
                }


                ProtoCore.DSASM.ClassNode thisClass = new ProtoCore.DSASM.ClassNode();
                thisClass.Name = classDecl.ClassName;
                thisClass.Size = classDecl.Variables.Count;
                thisClass.IsImportedClass = classDecl.IsImportedClass;
                thisClass.TypeSystem = core.TypeSystem;
                thisClass.ClassAttributes = classDecl.ClassAttributes;
                thisClass.IsStatic = classDecl.IsStatic;
                thisClass.IsInterface = classDecl.IsInterface;

                thisClass.ExternLib = classDecl.ExternLibName ?? Path.GetFileName(core.CurrentDSFileName);

                globalClassIndex = core.ClassTable.Append(thisClass);
                if (ProtoCore.DSASM.Constants.kInvalidIndex == globalClassIndex)
                {
                    string message = string.Format("Class redefinition '{0}' (BE1C3285).\n", classDecl.ClassName);
                    buildStatus.LogSemanticError(message, core.CurrentDSFileName, classDecl.line, classDecl.col);
                }

                unPopulatedClasses.Add(globalClassIndex, classDecl);

                //Always allow us to convert a class to a bool
                thisClass.CoerceTypes.Add((int)PrimitiveType.Bool, (int)ProtoCore.DSASM.ProcedureDistance.CoerceScore);
            }
            else if (ProtoCore.CompilerDefinitions.CompilePass.ClassBaseClass == compilePass)
            { 
                // Base class pass
                // Populating each class entry with their immediate baseclass
                globalClassIndex = core.ClassTable.GetClassId(classDecl.ClassName);

                ProtoCore.DSASM.ClassNode thisClass = core.ClassTable.ClassNodes[globalClassIndex];

                // Verify and store the list of classes it inherits from 
                if (!string.IsNullOrEmpty(classDecl.BaseClass))
                {
                    int baseClass = core.ClassTable.GetClassId(classDecl.BaseClass);
                    if (baseClass == globalClassIndex)
                    {
                        string message = string.Format("Class '{0}' cannot derive from itself (DED0A61F).\n", classDecl.ClassName);
                        buildStatus.LogSemanticError(message, core.CurrentDSFileName, classDecl.line, classDecl.col);
                    }

                    if (ProtoCore.DSASM.Constants.kInvalidIndex != baseClass)
                    {
                        var baseClassNode = core.ClassTable.ClassNodes[baseClass];

                        // Cannot derive DS class from non-static FFI class
                        if (baseClassNode.IsImportedClass && !classDecl.IsExternLib)
                        {
                            if (!baseClassNode.IsStatic)
                            {
                                string message =
                                    string.Format("Cannot derive DS class from non-static FFI class {0} (DA87AC4D).\n",
                                        core.ClassTable.ClassNodes[baseClass].Name);

                                buildStatus.LogSemanticError(message, core.CurrentDSFileName, classDecl.line,
                                    classDecl.col);
                            }
                            else
                            {
                                // All DS classes declared in imported DS files that inherit from static FFI base classes are imported
                                classDecl.IsStatic = baseClassNode.IsStatic;
                                thisClass.IsStatic = baseClassNode.IsStatic;
                                thisClass.IsImportedClass = true;
                            }
                        }

                        thisClass.Base = baseClass;
                        thisClass.CoerceTypes.Add(baseClass, (int)ProtoCore.DSASM.ProcedureDistance.CoerceBaseClass);
                    }
                    else
                    {
                        string message = string.Format("Unknown base class '{0}' (9E44FFB3).\n", classDecl.BaseClass);
                        buildStatus.LogSemanticError(message, core.CurrentDSFileName, classDecl.line, classDecl.col);
                    }
                }
            }
            else if (ProtoCore.CompilerDefinitions.CompilePass.ClassHierarchy == compilePass)
            {
                // Class hierarchy pass
                // Populating each class entry with all sub classes in the hierarchy
                globalClassIndex = core.ClassTable.GetClassId(classDecl.ClassName);

                ProtoCore.DSASM.ClassNode thisClass = core.ClassTable.ClassNodes[globalClassIndex];

                // Verify and store the list of classes it inherits from 
                if (!string.IsNullOrEmpty(classDecl.BaseClass))
                {
                    int baseClass = core.ClassTable.GetClassId(classDecl.BaseClass);

                    // Iterate through all the base classes until the the root class
                    // For every base class, add the coercion score
                    ProtoCore.DSASM.ClassNode tmpCNode = core.ClassTable.ClassNodes[baseClass];
                    if (tmpCNode.Base != Constants.kInvalidIndex)
                    {
                        baseClass = tmpCNode.Base;
                        while (ProtoCore.DSASM.Constants.kInvalidIndex != baseClass)
                        {
                            thisClass.CoerceTypes.Add(baseClass, (int)ProtoCore.DSASM.ProcedureDistance.CoerceBaseClass);
                            tmpCNode = core.ClassTable.ClassNodes[baseClass];

                            baseClass = ProtoCore.DSASM.Constants.kInvalidIndex;
                            if (tmpCNode.Base != Constants.kInvalidIndex)
                            {
                                baseClass = tmpCNode.Base;
                            }
                        }
                    }
                }

                // Verify and store the list of interfaces it inherits from
                thisClass.Interfaces = classDecl.Interfaces.Select(iname => core.ClassTable.GetClassId(iname)).ToList();
            }
            else if (ProtoCore.CompilerDefinitions.CompilePass.ClassMemVar == compilePass)
            {
                EmitMemberVariables(classDecl, graphNode);
            }
            else if (ProtoCore.CompilerDefinitions.CompilePass.ClassMemFuncSig == compilePass)
            {
                // Class member variable pass
                // Populating each class entry vtables with their respective
                // member variables signatures
                globalClassIndex = core.ClassTable.GetClassId(classDecl.ClassName);

                List<AssociativeNode> thisPtrOverloadList = new List<AssociativeNode>();
                foreach (AssociativeNode funcdecl in classDecl.Procedures)
                {
                    // Do not allow static DS class to have a constructor defined 
                    if (!classDecl.IsExternLib && classDecl.IsStatic && funcdecl is ConstructorDefinitionNode)
                    {
                        string message = string.Format("Static DS class {0} cannot have a constructor.\n", classDecl.ClassName);

                        buildStatus.LogSemanticError(message, core.CurrentDSFileName, funcdecl.line, funcdecl.col);
                    }

                    DfsTraverse(funcdecl, ref inferedType);

                    var funcDef = funcdecl as FunctionDefinitionNode;
                    if (funcDef == null || funcDef.IsStatic || funcDef.Name == ProtoCore.DSDefinitions.Keyword.Dispose)
                        continue;

                    bool isGetterSetter = CoreUtils.IsGetterSetter(funcDef.Name);

                    var thisPtrArgName = Constants.kThisPointerArgName;
                    if (!isGetterSetter)
                    {
                        var classsShortName = classDecl.ClassName.Split('.').Last();
                        var typeDepName = classsShortName.ToLower();
                        if (typeDepName != classsShortName && funcDef.Signature.Arguments.All(a => a.NameNode.Name != typeDepName))
                            thisPtrArgName = typeDepName;
                    }

                    // This is a function, create its parameterized this pointer overload
                    var procNode = new FunctionDefinitionNode(funcDef);
                    var thisPtrArg = new VarDeclNode()
                    {
                        Access = ProtoCore.CompilerDefinitions.AccessModifier.Public,
                        NameNode = AstFactory.BuildIdentifier(thisPtrArgName),
                        ArgumentType = new ProtoCore.Type { Name = classDecl.ClassName, UID = globalClassIndex, rank = 0 }
                    };
                    procNode.Signature.Arguments.Insert(0, thisPtrArg);
                    procNode.IsAutoGeneratedThisProc = true;
                    procNode.IsStatic = true;

                    if (CoreUtils.IsGetterSetter(funcDef.Name))
                    {
                        for (int n = 0; n < procNode.FunctionBody.Body.Count; ++n)
                        {
                            if (procNode.FunctionBody.Body[n] is BinaryExpressionNode)
                            {
                                var assocNode = procNode.FunctionBody.Body[n] as BinaryExpressionNode;
                                AssociativeNode outLNode = null;
                                AssociativeNode outRNode = null;

                                // This is the first traversal of this statement and is therefore the assignment op
                                Validity.Assert(Operator.assign == assocNode.Optr);
                                TraverseAndAppendThisPtrArg(assocNode.LeftNode, ref outLNode);
                                TraverseAndAppendThisPtrArg(assocNode.RightNode, ref outRNode);

                                assocNode.LeftNode = outLNode;
                                assocNode.RightNode = outRNode;
                            }
                        }
                    }
                    else
                    {
                        // Generate a static function for member function f():
                        //     static def f(a: A)
                        //     { 
                        //        return = a.f()
                        //     }
                        var args = procNode.Signature.Arguments.Select(a => a.NameNode).ToList();
                        var fcall = AstFactory.BuildFunctionCall(procNode.Name, args) as FunctionCallNode;

                        // Build the dotcall node
                        var lhs = procNode.Signature.Arguments[0].NameNode;
                        var right = CoreUtils.GenerateCallDotNode(lhs, fcall, core);
                        var body = AstFactory.BuildReturnStatement(right);
                        procNode.FunctionBody.Body = new List<AssociativeNode> { body };
                    }

                    thisPtrOverloadList.Add(procNode);
                }

                foreach (var overloadFunc in thisPtrOverloadList)
                {
                    // Emit the newly defined overloads
                    DfsTraverse(overloadFunc, ref inferedType);
                }

                classDecl.Procedures.AddRange(thisPtrOverloadList);

                // Prevent creation of default constructor for static DS class
                if (!classDecl.IsExternLib && !classDecl.IsStatic)
                {
                    ProtoCore.DSASM.ProcedureTable vtable = core.ClassTable.ClassNodes[globalClassIndex].ProcTable;
                    if (vtable.GetFunctionBySignature(classDecl.ClassName, new List<ProtoCore.Type>()) == null)
                    {
                        ConstructorDefinitionNode defaultConstructor = new ConstructorDefinitionNode();
                        defaultConstructor.Name = classDecl.ClassName;
                        defaultConstructor.LocalVariableCount = 0;
                        defaultConstructor.Signature = new ArgumentSignatureNode();
                        defaultConstructor.ReturnType = new ProtoCore.Type { Name = "var", UID = 0 };
                        defaultConstructor.FunctionBody = new CodeBlockNode();
                        defaultConstructor.BaseConstructor = null;
                        defaultConstructor.Access = ProtoCore.CompilerDefinitions.AccessModifier.Public;
                        defaultConstructor.IsExternLib = false;
                        defaultConstructor.ExternLibName = null;
                        DfsTraverse(defaultConstructor, ref inferedType);
                        classDecl.Procedures.Add(defaultConstructor);
                    }
                }
            }
            else if (ProtoCore.CompilerDefinitions.CompilePass.GlobalScope == compilePass)
            {
                // before populate the attributes, we must know the attribute class constructor signatures
                // in order to check the parameter 
                // populate the attributes for the class and class member variable 
                globalClassIndex = core.ClassTable.GetClassId(classDecl.ClassName);
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
                    foreach (ProtoCore.DSASM.SymbolNode sn in thisClass.Symbols.symbolList.Values)
                    {
                        // only populate the attributes for member variabls
                        if (sn.functionIndex != ProtoCore.DSASM.Constants.kInvalidIndex)
                            continue;
                        if (sn.classScope != globalClassIndex)
                        {
                            if (currentClassScope != sn.classScope)
                            {
                                currentClassScope = sn.classScope;
                            }
                        }
                        else
                        {
                            if (currentClassScope != globalClassIndex)
                            {
                                currentClassScope = globalClassIndex;
                            }
                        }
                    }
                }
            }
            else if (ProtoCore.CompilerDefinitions.CompilePass.ClassMemFuncBody == compilePass)
            {
                // Class member variable pass
                // Populating the function body of each member function defined in the class vtables

                globalClassIndex = core.ClassTable.GetClassId(classDecl.ClassName);

                // Initialize the global function table for this class
                // 'classIndexAtCallsite' is the class index as it is stored at the callsite function tables
                int classIndexAtCallsite = globalClassIndex + 1;
                core.FunctionTable.InitGlobalFunctionEntry(classIndexAtCallsite);

                foreach (AssociativeNode funcdecl in classDecl.Procedures)
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
                    int baseClassIndex = core.ClassTable.ClassNodes[thisClassIndex].Base;
                    baseConstructorName = core.ClassTable.ClassNodes[baseClassIndex].Name; 
                }
                else
                {
                    baseConstructorName = baseConstructor.Function.Name;
                }

                foreach (AssociativeNode paramNode in baseConstructor.FormalArguments)
                {
                    ProtoCore.Type paramType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 0);
                    emitReplicationGuide = true;
                    enforceTypeCheck = !(paramNode is BinaryExpressionNode);
                    DfsTraverse(paramNode, ref paramType, false, null, ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier);
                    DfsTraverse(paramNode, ref paramType, false, null, ProtoCore.CompilerDefinitions.SubCompilePass.None);
                    emitReplicationGuide = false;
                    enforceTypeCheck = true;
                    argTypeList.Add(paramType);
                }

                int bidx = core.ClassTable.ClassNodes[globalClassIndex].Base;
                if (bidx != Constants.kInvalidIndex )
                {
                    int cidx = core.ClassTable.ClassNodes[bidx].ProcTable.IndexOf(baseConstructorName, argTypeList);
                    if ((cidx != ProtoCore.DSASM.Constants.kInvalidIndex) &&
                        core.ClassTable.ClassNodes[bidx].ProcTable.Procedures[cidx].IsConstructor)
                    {
                        ctorIndex = cidx;
                        baseIndex = bidx;
                    }
                }
            }
            else
            {
                // call base class's default constructor
                // TODO keyu: to support multiple inheritance
                int bidx = core.ClassTable.ClassNodes[globalClassIndex].Base;
                if (bidx != Constants.kInvalidIndex)
                {
                    baseConstructorName = core.ClassTable.ClassNodes[bidx].Name;
                    int cidx = core.ClassTable.ClassNodes[bidx].ProcTable.IndexOf(baseConstructorName, argTypeList);
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

        private void EmitConstructorDefinitionNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None, GraphNode gNode = null)
        {
            ConstructorDefinitionNode funcDef = node as ConstructorDefinitionNode;
            ProtoCore.DSASM.CodeBlockType originalBlockType = codeBlock.blockType;
            codeBlock.blockType = ProtoCore.DSASM.CodeBlockType.Function;

            if (IsParsingMemberFunctionSig())
            {
                Validity.Assert(null == localProcedure);
                localProcedure = new ProtoCore.DSASM.ProcedureNode();

                localProcedure.Name = funcDef.Name;
                localProcedure.PC = ProtoCore.DSASM.Constants.kInvalidIndex;
                localProcedure.LocalCount = 0;// Defer till all locals are allocated
                ProtoCore.Type returnType = localProcedure.ReturnType; 
                if (globalClassIndex != -1)
                    returnType.Name = core.ClassTable.ClassNodes[globalClassIndex].Name;
                returnType.UID = globalClassIndex;
                returnType.rank = 0;
                localProcedure.ReturnType = returnType;
                localProcedure.IsConstructor = true;
                localProcedure.RuntimeIndex = 0;
                localProcedure.IsExternal = funcDef.IsExternLib;
                Validity.Assert(ProtoCore.DSASM.Constants.kInvalidIndex != globalClassIndex, "A constructor node must be associated with class");
                localProcedure.LocalCount = 0;
                localProcedure.ClassID = globalClassIndex;
                localProcedure.AccessModifier = funcDef.Access;

                localProcedure.MethodAttribute = funcDef.MethodAttributes;

                int peekFunctionindex = core.ClassTable.ClassNodes[globalClassIndex].ProcTable.Procedures.Count;

                // Append arg symbols
                List<KeyValuePair<string, ProtoCore.Type>> argsToBeAllocated = new List<KeyValuePair<string, ProtoCore.Type>>();
                if (null != funcDef.Signature)
                {
                    foreach (VarDeclNode argNode in funcDef.Signature.Arguments)
                    {
                        var argInfo = BuildArgumentInfoFromVarDeclNode(argNode); 
                        localProcedure.ArgumentInfos.Add(argInfo);

                        var argType = BuildArgumentTypeFromVarDeclNode(argNode, gNode);
                        localProcedure.ArgumentTypes.Add(argType);

                        argsToBeAllocated.Add(new KeyValuePair<string, ProtoCore.Type>(argInfo.Name, argType));
                    }

                    localProcedure.IsVarArg = funcDef.Signature.IsVarArg;
                }

                int findex = core.ClassTable.ClassNodes[globalClassIndex].ProcTable.Append(localProcedure);

                // Comment Jun: Catch this assert given the condition as this type of mismatch should never occur
                if (ProtoCore.DSASM.Constants.kInvalidIndex != findex)
                {
                    Validity.Assert(peekFunctionindex == localProcedure.ID);
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
                    string message = String.Format(ProtoCore.Properties.Resources.kMethodAlreadyDefined, localProcedure.Name);
                    buildStatus.LogWarning(WarningID.FunctionAlreadyDefined, message, core.CurrentDSFileName, funcDef.line, funcDef.col, gNode);
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

                var procNode = core.ClassTable.ClassNodes[globalClassIndex].ProcTable.GetFunctionBySignature(funcDef.Name, argList);

                globalProcIndex = procNode == null ? Constants.kInvalidIndex : procNode.ID;

                Validity.Assert(null == localProcedure);
                localProcedure = core.ClassTable.ClassNodes[globalClassIndex].ProcTable.Procedures[globalProcIndex];

                Validity.Assert(null != localProcedure);
                localProcedure.PC = pc;

                EmitInstrConsole(ProtoCore.DSASM.kw.newobj, localProcedure.Name);
                EmitAllocc(globalClassIndex);
                setConstructorStartPC = true;

                EmitCallingForBaseConstructor(globalClassIndex, funcDef.BaseConstructor);

                ProtoCore.FunctionEndPoint fep = null;
                if (!funcDef.IsExternLib)
                {
                    // Traverse default assignment for the class
                    emitDebugInfo = false;

                    //Traverse default argument for the constructor
                    foreach (ProtoCore.DSASM.ArgumentInfo argNode in localProcedure.ArgumentInfos)
                    {
                        if (!argNode.IsDefault)
                        {
                            continue;
                        }
                        BinaryExpressionNode bNode = argNode.DefaultExpression as BinaryExpressionNode;
                        // build a temporay node for statement : temp = defaultarg;
                        var iNodeTemp =AstFactory.BuildIdentifier(Constants.kTempDefaultArg);
                        BinaryExpressionNode bNodeTemp = new BinaryExpressionNode();
                        bNodeTemp.LeftNode = iNodeTemp;
                        bNodeTemp.Optr = ProtoCore.DSASM.Operator.assign;
                        bNodeTemp.RightNode = bNode.LeftNode;
                        EmitBinaryExpressionNode(bNodeTemp, ref inferedType);
                        //duild an inline conditional node for statement: defaultarg = (temp == DefaultArgNode) ? defaultValue : temp;
                        InlineConditionalNode icNode = new InlineConditionalNode();
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
                    localProcedure.LocalCount = core.BaseOffset;
                    core.ClassTable.ClassNodes[globalClassIndex].ProcTable.Procedures[globalProcIndex].LocalCount = core.BaseOffset;

                    // Update the param stack indices of this function
                    foreach (ProtoCore.DSASM.SymbolNode symnode in core.ClassTable.ClassNodes[globalClassIndex].Symbols.symbolList.Values)
                    {
                        if (symnode.functionIndex == globalProcIndex && symnode.isArgument)
                        {
                            symnode.index -= localProcedure.LocalCount;
                        }
                    }

                    // JIL FEP
                    ProtoCore.Lang.JILActivationRecord record = new ProtoCore.Lang.JILActivationRecord();
                    record.pc = localProcedure.PC;
                    record.locals = localProcedure.LocalCount;
                    record.classIndex = globalClassIndex;
                    record.funcIndex = globalProcIndex;

                    // Construct the fep arguments
                    fep = new ProtoCore.Lang.JILFunctionEndPoint(record);
                }
                else
                {
                    ProtoCore.Lang.JILActivationRecord jRecord = new ProtoCore.Lang.JILActivationRecord();
                    jRecord.pc = localProcedure.PC;
                    jRecord.locals = localProcedure.LocalCount;
                    jRecord.classIndex = globalClassIndex;
                    jRecord.funcIndex = localProcedure.ID;

                    ProtoCore.Lang.FFIActivationRecord record = new ProtoCore.Lang.FFIActivationRecord();
                    record.JILRecord = jRecord;
                    record.FunctionName = funcDef.Name;
                    record.ModuleName = funcDef.ExternLibName;
                    record.ModuleType = "dll";
                    record.IsDNI = false;
                    record.ReturnType = funcDef.ReturnType;
                    record.ParameterTypes = localProcedure.ArgumentTypes;
                    fep = new ProtoCore.Lang.FFIFunctionEndPoint(record);
                }

                // Construct the fep arguments
                fep.FormalParams = new ProtoCore.Type[localProcedure.ArgumentTypes.Count];
                fep.procedureNode = localProcedure;
                localProcedure.ArgumentTypes.CopyTo(fep.FormalParams, 0);

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
                EmitInstrConsole(ProtoCore.DSASM.kw.ret);

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
                EmitReturn(closeCurlyBracketLine, closeCurlyBracketColumn - 1,
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

        private void EmitFunctionDefinitionNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None, GraphNode graphNode = null)
        {
            bool parseGlobalFunctionBody = null == localProcedure && ProtoCore.CompilerDefinitions.CompilePass.GlobalFuncBody == compilePass;
            bool parseMemberFunctionBody = ProtoCore.DSASM.Constants.kGlobalScope != globalClassIndex && ProtoCore.CompilerDefinitions.CompilePass.ClassMemFuncBody == compilePass;

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
            codeBlock.blockType = ProtoCore.DSASM.CodeBlockType.Function;
            if (IsParsingGlobalFunctionSig() || IsParsingMemberFunctionSig())
            {
                Validity.Assert(null == localProcedure);
                localProcedure = new ProtoCore.DSASM.ProcedureNode();

                localProcedure.Name = funcDef.Name;
                localProcedure.PC = ProtoCore.DSASM.Constants.kInvalidIndex;
                localProcedure.LocalCount = 0; // Defer till all locals are allocated
                var uid = core.TypeSystem.GetType(funcDef.ReturnType.Name);
                var rank = funcDef.ReturnType.rank;
                var returnType = core.TypeSystem.BuildTypeObject(uid, rank); 
                if (returnType.UID == (int)PrimitiveType.InvalidType)
                {
                    string message = String.Format(ProtoCore.Properties.Resources.kReturnTypeUndefined, funcDef.ReturnType.Name, funcDef.Name);
                    buildStatus.LogWarning(WarningID.TypeUndefined, message, core.CurrentDSFileName, funcDef.line, funcDef.col, graphNode);
                    returnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, rank);
                }
                localProcedure.ReturnType = returnType;
                localProcedure.IsConstructor = false;
                localProcedure.IsStatic = funcDef.IsStatic;
                localProcedure.RuntimeIndex = codeBlock.codeBlockId;
                localProcedure.AccessModifier = funcDef.Access;
                localProcedure.IsExternal = funcDef.IsExternLib;
                localProcedure.IsAutoGenerated = funcDef.IsAutoGenerated;
                localProcedure.ClassID = globalClassIndex;
                localProcedure.IsAssocOperator = funcDef.IsAssocOperator;
                localProcedure.IsAutoGeneratedThisProc = funcDef.IsAutoGeneratedThisProc;

                localProcedure.MethodAttribute = funcDef.MethodAttributes;

                int peekFunctionindex = ProtoCore.DSASM.Constants.kInvalidIndex;

                if (ProtoCore.DSASM.Constants.kInvalidIndex == globalClassIndex)
                {
                    peekFunctionindex = codeBlock.procedureTable.Procedures.Count;
                }
                else
                {
                    peekFunctionindex = core.ClassTable.ClassNodes[globalClassIndex].ProcTable.Procedures.Count;
                }

                // Append arg symbols
                List<KeyValuePair<string, ProtoCore.Type>> argsToBeAllocated = new List<KeyValuePair<string, ProtoCore.Type>>();
                string functionDescription = localProcedure.Name;
                if (null != funcDef.Signature)
                {
                    foreach (VarDeclNode argNode in funcDef.Signature.Arguments)
                    {
                        var argInfo = BuildArgumentInfoFromVarDeclNode(argNode);
                        localProcedure.ArgumentInfos.Add(argInfo);

                        var argType = BuildArgumentTypeFromVarDeclNode(argNode, graphNode);
                        localProcedure.ArgumentTypes.Add(argType);

                        argsToBeAllocated.Add(new KeyValuePair<string, ProtoCore.Type>(argInfo.Name, argType));

                        functionDescription += argNode.ArgumentType.ToString();
                    }
                    localProcedure.HashID = functionDescription.GetHashCode();
                    localProcedure.IsVarArg = funcDef.Signature.IsVarArg;
                }

                if (ProtoCore.DSASM.Constants.kInvalidIndex == globalClassIndex)
                {
                    globalProcIndex = codeBlock.procedureTable.Append(localProcedure);
                }
                else
                {
                    globalProcIndex = core.ClassTable.ClassNodes[globalClassIndex].ProcTable.Append(localProcedure);
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
                    string message = String.Format(ProtoCore.Properties.Resources.kMethodAlreadyDefined, localProcedure.Name);
                    buildStatus.LogWarning(ProtoCore.BuildData.WarningID.FunctionAlreadyDefined, message, core.CurrentDSFileName, funcDef.line, funcDef.col, graphNode);
                    funcDef.skipMe = true;
                }
            }
            else if (parseGlobalFunctionBody || parseMemberFunctionBody)
            {
                if (CoreUtils.IsDisposeMethod(node.Name))
                {
                    core.Options.EmitBreakpoints = false;
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
                    var procNode = codeBlock.procedureTable.GetFunctionBySignature(funcDef.Name, argList);
                    globalProcIndex = procNode == null ? Constants.kInvalidIndex : procNode.ID;
                    localProcedure = codeBlock.procedureTable.Procedures[globalProcIndex];
                }
                else
                {
                    var procNode = core.ClassTable.ClassNodes[globalClassIndex].ProcTable.GetFunctionBySignature(funcDef.Name, argList);
                    globalProcIndex = procNode == null ? Constants.kInvalidIndex : procNode.ID;
                    localProcedure = core.ClassTable.ClassNodes[globalClassIndex].ProcTable.Procedures[globalProcIndex];
                }

                Validity.Assert(null != localProcedure);

                // code gen the attribute 
                // Its only on the parse body pass where the real pc is determined. Update this procedures' pc
                localProcedure.PC = pc;

                // Copy the active function to the core so nested language blocks can refer to it
                core.ProcNode = localProcedure;

                ProtoCore.FunctionEndPoint fep = null;
                if (!funcDef.IsExternLib)
                {
                    //Traverse default argument
                    emitDebugInfo = false;
                    foreach (ProtoCore.DSASM.ArgumentInfo argNode in localProcedure.ArgumentInfos)
                    {
                        if (!argNode.IsDefault)
                        {
                            continue;
                        }
                        BinaryExpressionNode bNode = argNode.DefaultExpression as BinaryExpressionNode;
                        // build a temporay node for statement : temp = defaultarg;
                        var iNodeTemp = AstFactory.BuildIdentifier(core.GenerateTempVar());
                        var bNodeTemp = AstFactory.BuildAssignment(iNodeTemp, bNode.LeftNode);
                        bNodeTemp.IsProcedureOwned = true;
                        EmitBinaryExpressionNode(bNodeTemp, ref inferedType, false, null, ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier);
                        //duild an inline conditional node for statement: defaultarg = (temp == DefaultArgNode) ? defaultValue : temp;
                        InlineConditionalNode icNode = new InlineConditionalNode();
                        BinaryExpressionNode cExprNode = new BinaryExpressionNode();
                        cExprNode.Optr = ProtoCore.DSASM.Operator.eq;
                        cExprNode.LeftNode = iNodeTemp;
                        cExprNode.RightNode = new DefaultArgNode();
                        icNode.ConditionExpression = cExprNode;
                        icNode.TrueExpression = bNode.RightNode;
                        icNode.FalseExpression = iNodeTemp;
                        bNodeTemp.LeftNode = bNode.LeftNode;
                        bNodeTemp.RightNode = icNode;
                        EmitBinaryExpressionNode(bNodeTemp, ref inferedType, false, null, ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier);
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
                    localProcedure.LocalCount = core.BaseOffset;

                    if (ProtoCore.DSASM.Constants.kInvalidIndex == globalClassIndex)
                    {
                        localProcedure.LocalCount = core.BaseOffset;

                        // Update the param stack indices of this function
                        foreach (ProtoCore.DSASM.SymbolNode symnode in codeBlock.symbolTable.symbolList.Values)
                        {
                            if (symnode.functionIndex == localProcedure.ID && symnode.isArgument)
                            {
                                symnode.index -= localProcedure.LocalCount;
                            }
                        }
                    }
                    else
                    {
                        core.ClassTable.ClassNodes[globalClassIndex].ProcTable.Procedures[localProcedure.ID].LocalCount = core.BaseOffset;

                        // Update the param stack indices of this function
                        foreach (ProtoCore.DSASM.SymbolNode symnode in core.ClassTable.ClassNodes[globalClassIndex].Symbols.symbolList.Values)
                        {
                            if (symnode.functionIndex == localProcedure.ID && symnode.isArgument)
                            {
                                symnode.index -= localProcedure.LocalCount;
                            }
                        }
                    }

                    ProtoCore.Lang.JILActivationRecord record = new ProtoCore.Lang.JILActivationRecord();
                    record.pc = localProcedure.PC;
                    record.locals = localProcedure.LocalCount;
                    record.classIndex = globalClassIndex;
                    record.funcIndex = localProcedure.ID;
                    fep = new ProtoCore.Lang.JILFunctionEndPoint(record);
                }
                else if (funcDef.BuiltInMethodId != ProtoCore.Lang.BuiltInMethods.MethodID.InvalidMethodID)
                {
                    fep = new ProtoCore.Lang.BuiltInFunctionEndPoint(funcDef.BuiltInMethodId);
                }
                else
                {
                    ProtoCore.Lang.JILActivationRecord jRecord = new ProtoCore.Lang.JILActivationRecord();
                    jRecord.pc = localProcedure.PC;
                    jRecord.locals = localProcedure.LocalCount;
                    jRecord.classIndex = globalClassIndex;
                    jRecord.funcIndex = localProcedure.ID;

                    ProtoCore.Lang.FFIActivationRecord record = new ProtoCore.Lang.FFIActivationRecord();
                    record.JILRecord = jRecord;
                    record.FunctionName = funcDef.Name;
                    record.ModuleName = funcDef.ExternLibName;
                    record.ModuleType = "dll";
                    record.ReturnType = funcDef.ReturnType;
                    record.ParameterTypes = localProcedure.ArgumentTypes;
                    fep = new ProtoCore.Lang.FFIFunctionEndPoint(record);
                }

                // Construct the fep arguments
                fep.FormalParams = new ProtoCore.Type[localProcedure.ArgumentTypes.Count];
                fep.BlockScope = codeBlock.codeBlockId;
                fep.ClassOwnerIndex = localProcedure.ClassID;
                fep.procedureNode = localProcedure;
                localProcedure.ArgumentTypes.CopyTo(fep.FormalParams, 0);

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
                        if (cnode.Base != Constants.kInvalidIndex)
                        {
                            ci = cnode.Base;

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
                        string message = String.Format(ProtoCore.Properties.Resources.kFunctionNotReturnAtAllCodePaths, localProcedure.Name);
                        buildStatus.LogWarning(ProtoCore.BuildData.WarningID.MissingReturnStatement, message, core.CurrentDSFileName, funcDef.line, funcDef.col, graphNode);
                    }
                    EmitReturnNull();
                }

                if (CoreUtils.IsDisposeMethod(node.Name))
                {
                    core.Options.EmitBreakpoints = true;
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
            ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None, ProtoCore.AST.AssociativeAST.BinaryExpressionNode parentNode = null)
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


            if (subPass == ProtoCore.CompilerDefinitions.SubCompilePass.None)
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

            if (null != procNode && !procNode.IsConstructor)
            {
                functionCallStack.Add(procNode);
                if (null != graphNode)
                {
                    graphNode.firstProc = procNode;

                    // Memoize the graphnode that contains a user-defined function call in the global scope
                    bool inGlobalScope = ProtoCore.DSASM.Constants.kGlobalScope == globalProcIndex && ProtoCore.DSASM.Constants.kInvalidIndex == globalClassIndex;
                    if (!procNode.IsExternal && inGlobalScope)
                    {
                        core.GraphNodeCallList.Add(graphNode);
                    }
                }
            }
            IsAssociativeArrayIndexing = arrayIndexing;

            inferedType.UID = isBooleanOp ? (int)PrimitiveType.Bool : inferedType.UID;

            if (subPass == ProtoCore.CompilerDefinitions.SubCompilePass.None)
            {
                if (fnode != null && fnode.ArrayDimensions != null)
                {
                    emitReplicationGuideFlag = emitReplicationGuide;
                    emitReplicationGuide = false;
                    int dimensions = DfsEmitArrayIndexHeap(fnode.ArrayDimensions, graphNode);
                    EmitPushDimensions(dimensions);
                    EmitLoadElement(null, Constants.kInvalidIndex);
                    fnode.ArrayDimensions = null;
                    emitReplicationGuide = emitReplicationGuideFlag;
                }

                List<AssociativeNode> replicationGuides = null;
                AtLevelNode atLevel = null;
                bool isRangeExpression = false;

                if (fnode != null)
                {
                    atLevel = fnode.AtLevel;
                    replicationGuides = fnode.ReplicationGuides;
                    isRangeExpression = fnode.Function.Name.Equals(Constants.kFunctionRangeExpression);
                }
                else if (node is FunctionDotCallNode)
                {
                    FunctionCallNode funcNode = (node as FunctionDotCallNode).FunctionCall;
                    var function = funcNode.Function as IdentifierNode;
                    replicationGuides = function.ReplicationGuides;
                    atLevel = function.AtLevel;
                }

                // YuKe: The replication guide for range expression will be 
                // generated in EmitRangeExprNode(). 
                // 
                // TODO: Revisit this piece of code to see how to handle array
                // index.
                if (!isRangeExpression && emitReplicationGuide)
                {
                    EmitAtLevel(atLevel);
                    EmitReplicationGuides(replicationGuides);
                }
            }

            if (null != graphNode)
            {
                graphNode.allowDependents = dependentState;
            }
        }

        private void EmitDynamicBlockNode(int block, ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None)
        {
            if (subPass == ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier)
            {
                return;
            }
            EmitInstrConsole(ProtoCore.DSASM.kw.push, "dynamicBlock");
            EmitPush(StackValue.BuildInt(block));

            if (emitReplicationGuide)
            {
                EmitInstrConsole(ProtoCore.DSASM.kw.poprepguides, "0");
                EmitPopReplicationGuides(0);
            }
        }

        private void EmitInlineConditionalNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None, ProtoCore.AST.AssociativeAST.BinaryExpressionNode parentNode = null)
        {
            if (subPass == ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier)
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
                BinaryExpressionNode bExprTrue = AstFactory.BuildReturnStatement(inlineConditionalNode.TrueExpression);

                LanguageBlockNode langblockT = new LanguageBlockNode();
                int trueBlockId = Constants.kInvalidIndex;
                langblockT.codeblock.Language = ProtoCore.Language.Associative;
                core.AssocNode = bExprTrue;
                core.InlineConditionalBodyGraphNodes.Push(new List<GraphNode>());
                EmitDynamicLanguageBlockNode(langblockT, bExprTrue, ref inferedType, ref trueBlockId, graphNode,
                    ProtoCore.CompilerDefinitions.SubCompilePass.None);
                List<GraphNode> trueBodyNodes = core.InlineConditionalBodyGraphNodes.Pop();

                // Append dependent nodes of the inline conditional 
                foreach (GraphNode gnode in trueBodyNodes)
                foreach (GraphNode dNode in gnode.dependentList)
                    graphNode.PushDependent(dNode);

                core.AssocNode = null;
                DynamicBlockNode dynBlockT = new DynamicBlockNode(trueBlockId);

                // False condition language block
                BinaryExpressionNode bExprFalse =
                    AstFactory.BuildReturnStatement(inlineConditionalNode.FalseExpression);

                LanguageBlockNode langblockF = new LanguageBlockNode();
                int falseBlockId = Constants.kInvalidIndex;
                langblockF.codeblock.Language = ProtoCore.Language.Associative;
                core.AssocNode = bExprFalse;
                core.InlineConditionalBodyGraphNodes.Push(new List<GraphNode>());
                EmitDynamicLanguageBlockNode(langblockF, bExprFalse, ref inferedType, ref falseBlockId, graphNode,
                    ProtoCore.CompilerDefinitions.SubCompilePass.None);
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
            EmitFunctionCallNode(inlineCall, ref inferedType, false, graphNode,
                ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier);
            EmitFunctionCallNode(inlineCall, ref inferedType, false, graphNode,
                ProtoCore.CompilerDefinitions.SubCompilePass.None, parentNode);

            // Need to restore those settings.
            if (graphNode != null)
            {
                graphNode.isInlineConditional = isInlineConditionalFlag;
                graphNode.updateBlock.startpc = startPC;
                graphNode.isReturn = isReturn;
            }
        }

        private void EmitUnaryExpressionNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None)
        {
            UnaryExpressionNode u = node as UnaryExpressionNode;
            DfsTraverse(u.Expression, ref inferedType, false, graphNode, subPass);

            if (subPass != ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier)
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
                VerifyAllocation(iNode.Name, globalClassIndex, globalProcIndex, out firstSymbol, out isAccessible);
            }
            else
            {
                Validity.Assert(false, "Invalid operation, this method is used to retrive the first symbol in an identifier list. It only accepts either an identlist or an ident");
            }
        }

        private void ResolveFunctionGroups()
        {
            // foreach class in classtable
            foreach (ProtoCore.DSASM.ClassNode cnode in core.ClassTable.ClassNodes)
            {
                if (cnode.Base != Constants.kInvalidIndex)
                {
                    // Get the current class functiongroup
                    int ci = cnode.ID;
                    Dictionary<string, FunctionGroup> groupList = new Dictionary<string, FunctionGroup>();
                    if (!core.FunctionTable.GlobalFuncTable.TryGetValue(ci + 1, out groupList))
                    {
                        continue;
                    }

                    // If it has a baseclass, get its function group 'basegroup'
                    int baseCI = cnode.Base;
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
                string name = dotCall.Arguments[0].Name;

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
                if (!firstProc.IsAutoGenerated)
                {
                    graphNode.firstProc = firstProc;
                }
            }

            return leftNodeRef;
        }

        private void EmitLHSIdentifierListForBinaryExpr(AssociativeNode bnode, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None, bool isTempExpression = false)
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

                ProtoCore.DSASM.SymbolNode firstSymbol = leftNodeRef.nodeList[0].symbol;
                if (null != firstSymbol)
                {
                    EmitDependency(binaryExpr.ExpressionUID, false);
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

        private bool EmitLHSThisDotProperyForBinaryExpr(AssociativeNode bnode, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None, bool isTempExpression = false)
        {
            BinaryExpressionNode binaryExpr = bnode as BinaryExpressionNode;
            if (binaryExpr == null || !(binaryExpr.LeftNode is IdentifierNode))
            {
                return false;
            }

            if (globalClassIndex != Constants.kGlobalScope && globalProcIndex != Constants.kGlobalScope)
            {
                ClassNode thisClass = core.ClassTable.ClassNodes[globalClassIndex];
                ProcedureNode procNode = thisClass.ProcTable.Procedures[globalProcIndex];

                IdentifierNode identNode = (binaryExpr.LeftNode as IdentifierNode);
                string identName = identNode.Value;

                // Local variables are not appended with 'this'
                if (!identNode.IsLocal)
                {
                    if (!procNode.Name.Equals(ProtoCore.DSASM.Constants.kSetterPrefix + identName))
                    {
                        SymbolNode symbolnode;
                        bool isAccessible = false;
                        VerifyAllocation(identName, globalClassIndex, globalProcIndex, out symbolnode, out isAccessible);

                        if (symbolnode != null &&
                            symbolnode.classScope != Constants.kGlobalScope &&
                            symbolnode.functionIndex == Constants.kGlobalScope)
                        {
                            var thisNode =AstFactory.BuildIdentifier(ProtoCore.DSDefinitions.Keyword.This);
                            var thisIdentListNode = AstFactory.BuildIdentList(thisNode, binaryExpr.LeftNode);
                            var newAssignment = AstFactory.BuildAssignment(thisIdentListNode, binaryExpr.RightNode);
                            NodeUtils.CopyNodeLocation(newAssignment, bnode);

                            if (ProtoCore.DSASM.Constants.kInvalidIndex != binaryExpr.ExpressionUID)
                            {
                                (newAssignment as BinaryExpressionNode).ExpressionUID = binaryExpr.ExpressionUID;
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

                    string className = dotcall.Arguments[0].Name;
                    string fullyQualifiedClassName = string.Empty;
                    bool isClassName = core.ClassTable.TryGetFullyQualifiedName(className, out fullyQualifiedClassName);

                    if (ProtoCore.Utils.CoreUtils.IsGetterSetter(dotcall.FunctionCall.Function.Name))
                    {
                        //string className = dotcall.DotCall.FormalArguments[0].Name;
                        if (isClassName)
                        {
                            ssaPointerStack.Peek().Add(dotcall.Arguments[0]);
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

        private void EmitBinaryExpressionNode(AssociativeNode node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None, bool isTempExpression = false)
        {
            BinaryExpressionNode bnode = null;

            if (!IsParsingGlobal() && !IsParsingGlobalFunctionBody() && !IsParsingMemberFunctionBody())
                return;

            bool isBooleanOperation = false;
            bnode = node as BinaryExpressionNode;
            ProtoCore.Type leftType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 0);
            ProtoCore.Type rightType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 0);

            DebugProperties.BreakpointOptions oldOptions = core.DebuggerProperties.breakOptions;
            
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
                    graphNode.exprUID = bnode.ExpressionUID;
                    graphNode.ssaSubExpressionID = bnode.SSASubExpressionID;
                    graphNode.ssaExpressionUID = bnode.SSAExpressionUID;
                    graphNode.IsModifier = bnode.IsModifier;
                    graphNode.guid = bnode.guid;
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
                        if (context.exprExecutionFlags.ContainsKey(bnode.ExpressionUID))
                        {
                            graphNode.isDirty = context.exprExecutionFlags[bnode.ExpressionUID];
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
                            if (localProcedure.IsStatic)
                            {
                                errorMessage = ProtoCore.Properties.Resources.kUsingThisInStaticFunction;
                            }
                            else if (localProcedure.ClassID == Constants.kGlobalScope)
                            {
                                errorMessage = ProtoCore.Properties.Resources.kInvalidThis;
                            }
                            else
                            {
                                errorMessage = ProtoCore.Properties.Resources.kAssingToThis;
                            }
                        }
                        core.BuildStatus.LogWarning(WarningID.InvalidThis, errorMessage, core.CurrentDSFileName, bnode.line, bnode.col, graphNode);

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

                if (inferedType.UID == (int)PrimitiveType.FunctionPointer && subPass != ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier && emitDebugInfo)
                {
                    buildStatus.LogSemanticError(Resources.FunctionPointerNotAllowedAtBinaryExpression, core.CurrentDSFileName, bnode.LeftNode.line, bnode.LeftNode.col);
                }

                leftType.UID = inferedType.UID;
                leftType.rank = inferedType.rank;
            }

            int startpc = ProtoCore.DSASM.Constants.kInvalidIndex;
            if ((ProtoCore.DSASM.Operator.assign == bnode.Optr) && (bnode.RightNode is LanguageBlockNode))
            {
                inferedType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 0);
            }

            if (null != localProcedure && localProcedure.IsConstructor && setConstructorStartPC)
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
                    bnode.RightNode =AstFactory.BuildIdentifier(t.Value);
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

            rightType.UID = inferedType.UID;
            rightType.rank = inferedType.rank;

            BinaryExpressionNode rightNode = bnode.RightNode as BinaryExpressionNode;
            if ((rightNode != null) && (ProtoCore.DSASM.Operator.assign == rightNode.Optr))
            {
                DfsTraverse(rightNode.LeftNode, ref inferedType, false, graphNode);
            }

            if (bnode.Optr != ProtoCore.DSASM.Operator.assign)
            {
                if (subPass == ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier)
                {
                    return;
                }

                if (inferedType.UID == (int)PrimitiveType.FunctionPointer && emitDebugInfo)
                {
                    buildStatus.LogSemanticError(Resources.FunctionPointerNotAllowedAtBinaryExpression, core.CurrentDSFileName, bnode.RightNode.line, bnode.RightNode.col);
                }
                EmitBinaryOperation(leftType, rightType, bnode.Optr);
                isBooleanOp = false;

                return;
            }

            Validity.Assert(null != graphNode);
            if (!isTempExpression)
            {
                graphNode.updateBlock.startpc = pc;
            }

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
            DfsTraverse(bnode.RightNode, ref inferedType, isBooleanOperation, graphNode, ProtoCore.CompilerDefinitions.SubCompilePass.None, bnode);
            subPass = ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier;

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
                    if (globalClassIndex != ProtoCore.DSASM.Constants.kGlobalScope)
                    {
                        bool isAccessibleFp;
                        int realType;
                        var procNode = core.ClassTable.ClassNodes[globalClassIndex].GetMemberFunction(t.Name, null, globalClassIndex, out isAccessibleFp, out realType);
                        if (procNode != null && procNode.ID != Constants.kInvalidIndex && emitDebugInfo)
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
                        buildStatus.LogWarning(WarningID.AccessViolation, message, core.CurrentDSFileName, t.line, t.col, graphNode);
                    }

                    int dimensions = 0;
                    if (null != t.ArrayDimensions)   
                    {
                        graphNode.isIndexingLHS = true;
                        dimensions = DfsEmitArrayIndexHeap(t.ArrayDimensions, graphNode, bnode);
                    }


                    // Comment Jun: Attempt to get the modified argument arrays in the current method
                    // Comment Jun: As of R1 - arrays are copy constructed and cannot propagate update unless explicitly returned
                    ProtoCore.Type castType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank);
                    var tident = bnode.LeftNode as TypedIdentifierNode;
                    if (tident != null)
                    {
                        int castUID = core.ClassTable.IndexOf(tident.datatype.Name);
                        if ((int)PrimitiveType.InvalidType == castUID)
                        {
                            castType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.InvalidType, 0);
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
                                symbolnode = Allocate(globalClassIndex, globalClassIndex, globalProcIndex, t.Name, inferedType, line: bnode.line, col: bnode.col); 
                                // Add the symbols during watching process to the watch symbol list.
                                if (core.Options.RunMode == ProtoCore.DSASM.InterpreterMode.Expression)
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
                                EmitCast(castType.UID, castType.rank);
                            }

                            symbol = symbolnode.symbolTableIndex;
                            if (t.Name == ProtoCore.DSASM.Constants.kTempArg)
                            {
                                if (dimensions == 0)
                                {
                                    EmitInstrConsole(kw.pop, t.Value);
                                    EmitPopForSymbol(symbolnode, runtimeIndex);
                                }
                                else
                                {
                                    EmitPushDimensions(dimensions);
                                    EmitInstrConsole(kw.setelement, t.Value);
                                    EmitSetElement(symbolnode, runtimeIndex);
                                }
                            }
                            else
                            {
                                if (core.Options.RunMode != ProtoCore.DSASM.InterpreterMode.Expression)
                                {
                                    if (dimensions == 0)
                                    {
                                        EmitInstrConsole(kw.pop, t.Value);
                                        EmitPopForSymbol(symbolnode, runtimeIndex, node.line, node.col, node.endLine, node.endCol);
                                    }
                                    else
                                    {
                                        EmitPushDimensions(dimensions);
                                        EmitInstrConsole(kw.setelement, t.Value);
                                        EmitSetElement(symbolnode, runtimeIndex);
                                    }
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
                                EmitCast(castType.UID, castType.rank);
                            }

                            StackValue operand = symbolnode.isStatic ? StackValue.BuildStaticMemVarIndex(symbol) : StackValue.BuildMemVarIndex(symbol);
                            if (dimensions == 0)
                            {
                                EmitInstrConsole(kw.popm, t.Name);
                                EmitPopm(operand, runtimeIndex, node.line, node.col, node.endLine, node.endCol);
                            }
                            else
                            {
                                EmitPushDimensions(dimensions);
                                EmitInstrConsole(kw.setelement, t.Name);
                                EmitSetMemElement(operand, runtimeIndex, node.line, node.col, node.endLine, node.endCol);
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
                            EmitDependency(bnode.ExpressionUID, bnode.isSSAAssignment);
                            functionCallStack.Clear();
                        }
                    }
                    else
                    {
                        if (!isAllocated)
                        {
                            symbolnode = Allocate(globalClassIndex, globalClassIndex, globalProcIndex, t.Name,
                                inferedType, line: bnode.line, col: bnode.col);

                            if (core.Options.RunMode == ProtoCore.DSASM.InterpreterMode.Expression)
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
                            EmitCast(castType.UID, castType.rank);
                        }

                        if (core.Options.RunMode != ProtoCore.DSASM.InterpreterMode.Expression)
                        {
                            if (dimensions == 0)
                            {
                                EmitInstrConsole(kw.pop, symbolnode.name);
                                EmitPopForSymbol(symbolnode, runtimeIndex, node.line, node.col, node.endLine, node.endCol);
                            }
                            else
                            {
                                EmitPushDimensions(dimensions);
                                EmitInstrConsole(kw.setelement, t.Value);
                                EmitSetElement(symbolnode, runtimeIndex);
                            }
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
                            EmitDependency(bnode.ExpressionUID, bnode.isSSAAssignment);
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
                            EmitInstrConsole(ProtoCore.DSASM.kw.pop, cyclicSymbol1.name);
                            EmitPopForSymbol(cyclicSymbol1, cyclicSymbol1.runtimeTableIndex, node.line, node.col, node.endLine, node.endCol);

                            nullAssignGraphNode1.PushSymbolReference(cyclicSymbol1);
                            nullAssignGraphNode1.procIndex = globalProcIndex;
                            nullAssignGraphNode1.classIndex = globalClassIndex;
                            nullAssignGraphNode1.updateBlock.endpc = pc - 1;

                            PushGraphNode(nullAssignGraphNode1);
                            EmitDependency(ProtoCore.DSASM.Constants.kInvalidIndex, false);


                            //
                            // Set the second symbol that triggers the cycle to null
                            ProtoCore.AssociativeGraph.GraphNode nullAssignGraphNode2 = new ProtoCore.AssociativeGraph.GraphNode();
                            nullAssignGraphNode2.updateBlock.startpc = pc;

                            EmitPushNull();
                            EmitInstrConsole(ProtoCore.DSASM.kw.pop, cyclicSymbol2.name);
                            EmitPopForSymbol(cyclicSymbol2, cyclicSymbol2.runtimeTableIndex, node.line, node.col, node.endLine, node.endCol);

                            nullAssignGraphNode2.PushSymbolReference(cyclicSymbol2);
                            nullAssignGraphNode2.procIndex = globalProcIndex;
                            nullAssignGraphNode2.classIndex = globalClassIndex;
                            nullAssignGraphNode2.updateBlock.endpc = pc - 1;

                            PushGraphNode(nullAssignGraphNode2);
                            EmitDependency(ProtoCore.DSASM.Constants.kInvalidIndex, false);
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
        }

        private void EmitImportNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None)
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

                if (ProtoCore.CompilerDefinitions.CompilePass.GlobalFuncBody == compilePass)
                {
                    core.LoadedDLLs.Add(importNode.ModulePathFileName);
                    firstImportInDeltaExecution = true;
                }
            }

            if (codeBlockNode != null)
            {
                // Only build SSA for the first time
                // Transform after class name compile pass
                if (ProtoCore.CompilerDefinitions.CompilePass.ClassName > compilePass)
                {
                    codeBlockNode.Body = ApplyTransform(codeBlockNode.Body);
                    codeBlockNode.Body = BuildSSA(codeBlockNode.Body, context);
                }

                foreach (AssociativeNode assocNode in codeBlockNode.Body)
                {
                    inferedType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 0); 

                    if (assocNode is LanguageBlockNode)
                    {
                        // Build a binaryn node with a temporary lhs for every stand-alone language block
                        var iNode = AstFactory.BuildIdentifier(core.GenerateTempLangageVar());
                        var langBlockNode = AstFactory.BuildAssignment(iNode, assocNode);

                        DfsTraverse(langBlockNode, ref inferedType, false, null, ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier);
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
            identList.RightNode = AstFactory.BuildFunctionCall(getterName, new List<AssociativeNode>());

            // %t = a.get_x();
            //IdentifierNode result = nodeBuilder.BuildTempVariable() as IdentifierNode;
            IdentifierNode result = AstFactory.BuildIdentifier(core.GenerateTempPropertyVar());
            var assignment = AstFactory.BuildAssignment(result, identList);
            EmitBinaryExpressionNode(assignment, ref inferedType, false, graphNode, ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier, true);

            result.ArrayDimensions = thisNode.ArrayDimensions;
            return result;
        }

        override protected void EmitGetterSetterForIdentList(
            ProtoCore.AST.Node node, 
            ref ProtoCore.Type inferedType, 
            ProtoCore.AssociativeGraph.GraphNode graphNode, 
            ProtoCore.CompilerDefinitions.SubCompilePass subPass, 
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
                    var thisIdent = AstFactory.BuildIdentifier(ProtoCore.DSDefinitions.Keyword.This);
                    var thisIdentList = AstFactory.BuildIdentList(thisIdent, leftMostIdent);
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
                    var tmpVar = AstFactory.BuildIdentifier(core.GenerateTempVar());
                    var assignment = AstFactory.BuildAssignment(tmpVar, setterArgument as AssociativeNode);
                    EmitBinaryExpressionNode(assignment, ref inferedType, false, graphNode, ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier, true);

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

                        var tmpVar = AstFactory.BuildIdentifier(Constants.kTempArg);
                        AssociativeNode tmpAssignmentNode = null;

                        bool isSetterFunctionCall = setterArgument is InlineConditionalNode ||
                            setterArgument is FunctionCallNode;

                        if (isSetterFunctionCall)
                        {
                            var tmpRetVar = AstFactory.BuildIdentifier(core.GenerateTempVar());
                            var tmpGetInlineRet = AstFactory.BuildAssignment(tmpRetVar, setterArgument as AssociativeNode);
                            NodeUtils.CopyNodeLocation(tmpGetInlineRet, inode);
                            EmitBinaryExpressionNode(tmpGetInlineRet, ref inferedType, false, graphNode, ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier, true);
                            inode.RightNode = AstFactory.BuildFunctionCall(rnodeName, new List<AssociativeNode> { tmpRetVar });

                            // This is more for property assignments (including those in 
                            // constructor body). Since temporary variables and generated 
                            // function nodes do not have source locations set on them, 
                            // the location can be set to the same as IdentifierListNode 
                            // (which covers the entire assignment statement).
                            // 
                            NodeUtils.CopyNodeLocation(inode.RightNode, inode);
                            tmpAssignmentNode = AstFactory.BuildAssignment(tmpVar, inode);
                            NodeUtils.CopyNodeLocation(tmpAssignmentNode, inode);
                        }
                        else
                        {
                            AssociativeNode fcall = AstFactory.BuildFunctionCall(rnodeName, new List<AssociativeNode> { setterArgument as AssociativeNode });

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
                                tmpAssignmentNode = AstFactory.BuildAssignment(tmpVar, fcall);
                            }
                            else
                            {
                                inode.RightNode = ProtoCore.Utils.CoreUtils.GenerateCallDotNode(inode.LeftNode, fcall as FunctionCallNode, core);
                                tmpAssignmentNode = AstFactory.BuildAssignment(tmpVar, inode.RightNode);
                            }
                        }

                        NodeUtils.CopyNodeLocation(tmpAssignmentNode, inode);

                        bool allowDependents = graphNode.allowDependents;
                        if (isSetterFunctionCall)
                        {
                            graphNode.allowDependents = false;
                        }
                        EmitBinaryExpressionNode(tmpAssignmentNode, ref inferedType, false, graphNode, ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier, true);
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
                        var assignment = AstFactory.BuildAssignment(tmpVar, setterArgument as AssociativeNode);
                        EmitBinaryExpressionNode(assignment, ref inferedType, false, graphNode, ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier, true);
                        (tmpVar as IdentifierNode).ArrayDimensions = null;

                        if (graphNode.dependentList.Count > 1)
                        {
                            graphNode.dependentList[0].updateNodeRefList[0].nodeList[0].dimensionNodeList
                                = graphNode.dependentList[1].updateNodeRefList[0].nodeList[0].dimensionNodeList;
                        }

                        graphNode.allowDependents = false;

                        // %t3 = %t1.%set_y(%t2[i]);
                        string setterName = ProtoCore.DSASM.Constants.kSetterPrefix + rnode.Name;
                        inode.RightNode = AstFactory.BuildFunctionCall(setterName, new List<AssociativeNode> { tmpVar });
                        var tmpSetterVar = AstFactory.BuildIdentifier(core.GenerateTempVar());
                        assignment = AstFactory.BuildAssignment(tmpSetterVar, inode);
                        
                        NodeUtils.SetNodeLocation(assignment, inode, inode);
                        EmitBinaryExpressionNode(assignment, ref inferedType, false, graphNode, ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier, true);

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
                    EmitIdentifierNode(retnode, ref inferedType, false, graphNode, ProtoCore.CompilerDefinitions.SubCompilePass.None);
                    isCollapsed = true;
                }
            }
        }

        private void EmitGroupExpressionNode(AssociativeNode node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None)
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
                if (subPass != ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier)
                {
                    int dimensions = DfsEmitArrayIndexHeap(group.ArrayDimensions, graphNode);
                    EmitPushDimensions(dimensions);
                    EmitLoadElement(null, Constants.kInvalidIndex);
                }
            }

            if (subPass != ProtoCore.CompilerDefinitions.SubCompilePass.UnboundIdentifier && emitReplicationGuide)
            {
                EmitAtLevel(group.AtLevel);
                EmitReplicationGuides(group.ReplicationGuides);
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
            if (uid == (int)PrimitiveType.InvalidType && !core.IsTempVar(argNode.NameNode.Name))
            {
                string message = String.Format(ProtoCore.Properties.Resources.kArgumentTypeUndefined, argNode.ArgumentType.Name, argNode.NameNode.Name);
                buildStatus.LogWarning(WarningID.TypeUndefined, message, core.CurrentDSFileName, argNode.line, argNode.col, graphNode);
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
            return !InsideFunction() &&
                   compilePass == ProtoCore.CompilerDefinitions.CompilePass.GlobalScope;
        }

        private bool IsParsingGlobalFunctionBody()
        {
            return InsideFunction() &&
                   compilePass == ProtoCore.CompilerDefinitions.CompilePass.GlobalFuncBody;
        }

        private bool IsParsingMemberFunctionBody()
        {
            return Constants.kGlobalScope != globalClassIndex && 
                   compilePass == ProtoCore.CompilerDefinitions.CompilePass.ClassMemFuncBody;
        }

        private bool IsParsingGlobalFunctionSig()
        { 
            return (null == localProcedure) && (ProtoCore.CompilerDefinitions.CompilePass.GlobalFuncSig == compilePass);
        }

        private bool IsParsingMemberFunctionSig()
        {
            return (ProtoCore.DSASM.Constants.kGlobalScope != globalClassIndex) && (ProtoCore.CompilerDefinitions.CompilePass.ClassMemFuncSig == compilePass);
        }

        protected override void DfsTraverse(ProtoCore.AST.Node pNode, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.CompilerDefinitions.SubCompilePass subPass = ProtoCore.CompilerDefinitions.SubCompilePass.None, ProtoCore.AST.Node parentNode = null)
        {
            AssociativeNode node = pNode as AssociativeNode;
            if (null == node || (node.skipMe))
                return;

            switch (node.Kind)
            {
                case AstKind.Identifier:
                case AstKind.TypedIdentifier:
                    EmitIdentifierNode(node, ref inferedType, isBooleanOp, graphNode, subPass, parentNode as BinaryExpressionNode);
                    break;
                case AstKind.Integer:
                    EmitIntNode(node, ref inferedType, isBooleanOp, graphNode, subPass);
                    break;
                case AstKind.Double:
                    EmitDoubleNode(node, ref inferedType, isBooleanOp, graphNode, subPass);
                    break;
                case AstKind.Boolean:
                    EmitBooleanNode(node, ref inferedType, subPass);
                    break;
                case AstKind.Char:
                    EmitCharNode(node, ref inferedType, isBooleanOp, subPass);
                    break;
                case AstKind.String:
                    EmitStringNode(node, ref inferedType, graphNode, subPass);
                    break;
                case AstKind.DefaultArgument:
                    EmitDefaultArgNode(subPass);
                    break;
                case AstKind.Null:
                    EmitNullNode(node, ref inferedType, isBooleanOp, subPass);
                    break;
                case AstKind.RangeExpression:
                    EmitRangeExprNode(node, ref inferedType, graphNode, subPass);
                    break;
                case AstKind.LanguageBlock:
                    EmitLanguageBlockNode(node, ref inferedType, graphNode, subPass);
                    break;
                case AstKind.ClassDeclaration:
                    EmitClassDeclNode(node, ref inferedType, subPass, graphNode);
                    break;
                case AstKind.Constructor:
                    EmitConstructorDefinitionNode(node, ref inferedType, subPass, graphNode);
                    break;
                case AstKind.FunctionDefintion:
                    EmitFunctionDefinitionNode(node, ref inferedType, subPass, graphNode);
                    break;
                case AstKind.FunctionCall:
                    EmitFunctionCallNode(node, ref inferedType, isBooleanOp, graphNode, subPass, parentNode as BinaryExpressionNode);
                    break;
                case AstKind.FunctionDotCall:
                    EmitFunctionCallNode(node, ref inferedType, isBooleanOp, graphNode, subPass, parentNode as BinaryExpressionNode);
                    break;
                case AstKind.ExpressionList:
                    EmitExprListNode(node, ref inferedType, graphNode, subPass, parentNode);
                    break;
                case AstKind.IdentifierList:
                    EmitIdentifierListNode(node, ref inferedType, isBooleanOp, graphNode, subPass, parentNode);
                    break;
                case AstKind.InlineConditional:
                    EmitInlineConditionalNode(node, ref inferedType, graphNode, subPass, parentNode as BinaryExpressionNode);
                    break;
                case AstKind.UnaryExpression:
                    EmitUnaryExpressionNode(node, ref inferedType, graphNode, subPass);
                    break;
                case AstKind.BinaryExpression:
                    EmitBinaryExpressionNode(node, ref inferedType, isBooleanOp, graphNode, subPass);
                    break;
                case AstKind.Import:
                    EmitImportNode(node, ref inferedType, subPass);
                    break;
                case AstKind.DynamicBlock:
                    {
                        int block = (node as DynamicBlockNode).block;
                        EmitDynamicBlockNode(block, subPass);
                        break;
                    }
                case AstKind.ThisPointer:
                    EmitThisPointerNode(subPass);
                    break;
                case AstKind.Dynamic:
                    EmitDynamicNode(subPass);
                    break;
                case AstKind.GroupExpression:
                    EmitGroupExpressionNode(node, ref inferedType, isBooleanOp, graphNode, subPass);
                    break;
            }
            int blockId = codeBlock.codeBlockId; 
        }
    }
}