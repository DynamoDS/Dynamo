//#define ENABLE_INC_DEC_FIX

using System;
using System.Collections.Generic;
using System.Diagnostics;
using ProtoCore;
using ProtoCore.AST.ImperativeAST;
using ProtoCore.Exceptions;
using ProtoCore.DSASM;
using System.Text;
using ProtoCore.Utils;
using ProtoCore.BuildData;
using ProtoImperative.Properties;

namespace ProtoImperative
{
    public class BackpatchMap
    {
        public BackpatchMap()
        {
            BreakTable = new Dictionary<int, BackpatchTable>();
            EntryTable = new Dictionary<int, int>();
        }

        public Dictionary<int, BackpatchTable> BreakTable;
        public Dictionary<int, int> EntryTable;
    }

    public class CodeGen : ProtoCore.CodeGen
    {
        private List<ImperativeNode> astNodes; 

        private ProtoCore.CompilerDefinitions.Imperative.CompilePass compilePass;

        private readonly BackpatchMap backpatchMap;

        private NodeBuilder nodeBuilder;

        public CodeGen(Core coreObj, ProtoCore.CompileTime.Context callContext, ProtoCore.DSASM.CodeBlock parentBlock = null) : base(coreObj, parentBlock)
        {
            context = callContext;
            //  dumpbytecode is optionally enabled
            //
            astNodes = new List<ImperativeNode>();

            // Create a new symboltable for this block
            // Set the new symbol table's parent
            // Set the new table as a child of the parent table

            // Comment Jun: Get the codeblock to use for this codegenerator
            if (core.Options.IsDeltaExecution)
            {
                codeBlock = GetDeltaCompileCodeBlock();
            }
            else
            {
                codeBlock = BuildNewCodeBlock();
            }

            if (null == parentBlock)
            {
                // This is a top level block
                core.CodeBlockList.Add(codeBlock);
            }
            else
            {
                // This is a nested block
                parentBlock.children.Add(codeBlock);
                codeBlock.parent = parentBlock;
            }

            blockScope = 0;

            // Bouncing to this language codeblock from a function should immediatlet se the first instruction as the entry point
            if (ProtoCore.DSASM.Constants.kGlobalScope != globalProcIndex)
            {
                isEntrySet = true;
                codeBlock.instrStream.entrypoint = 0;
            }

            backpatchMap = new BackpatchMap();

            nodeBuilder = new NodeBuilder(core);
        }

        private ProtoCore.DSASM.CodeBlock GetDeltaCompileCodeBlock()
        {
            ProtoCore.DSASM.CodeBlock cb = null;
            cb = BuildNewCodeBlock();
            Validity.Assert(null != cb);
            return cb;
        }

        /// <summary>
        /// Emits a block of code for the following cases:
        ///     if elseif else body
        ///     while body
        ///     for body
        /// </summary>
        /// <param name="codeBlock"></param>
        /// <param name="inferedType"></param>
        /// <param name="subPass"></param>
        private void EmitCodeBlock(
            List<ImperativeNode> codeBlock, 
            ref ProtoCore.Type inferedType, 
            bool isBooleanOp = false, 
            ProtoCore.AssociativeGraph.GraphNode graphNode = null)
        {
            foreach (ImperativeNode bodyNode in codeBlock)
            {
                inferedType = new ProtoCore.Type();
                inferedType.UID = (int)PrimitiveType.kTypeVar;

                if (bodyNode is LanguageBlockNode)
                {
                    BinaryExpressionNode langBlockNode = new BinaryExpressionNode();
                    langBlockNode.LeftNode = nodeBuilder.BuildIdentfier(core.GenerateTempLangageVar());
                    langBlockNode.Optr = ProtoCore.DSASM.Operator.assign;
                    langBlockNode.RightNode = bodyNode;
                    DfsTraverse(langBlockNode, ref inferedType, isBooleanOp, graphNode);
                }
                else
                {
                    DfsTraverse(bodyNode, ref inferedType, isBooleanOp, graphNode);
                }
            }
        }

        private ProtoCore.DSASM.CodeBlock BuildNewCodeBlock(ProcedureTable procTable = null)
        {
            /*
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

            return cb;
             * */

            ProtoCore.DSASM.CodeBlock cb = new ProtoCore.DSASM.CodeBlock(
                context.guid,
                ProtoCore.DSASM.CodeBlockType.kLanguage,
                ProtoCore.Language.kImperative,
                core.CodeBlockIndex,
                new ProtoCore.DSASM.SymbolTable("imperative lang block", core.RuntimeTableIndex),
                new ProtoCore.DSASM.ProcedureTable(core.RuntimeTableIndex), 
                false, 
                core);

            ++core.CodeBlockIndex;
            ++core.RuntimeTableIndex;

            return cb;
        }

        protected override void SetEntry()
        {
            if (ProtoCore.DSASM.Constants.kGlobalScope == globalProcIndex && !isEntrySet)
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
                codeBlock.instrStream.entrypoint = pc;
            }
        }

        private string GetForLoopKeyIdent()
        {
            return ProtoCore.DSASM.Constants.kForLoopKey + core.ForLoopBlockIndex.ToString();
        }

        private string GetForExprIdent()
        {
            // TODO Jun: Should this be autogenerated or kept as a constant?
            return ProtoCore.DSASM.Constants.kForLoopExpression + core.ForLoopBlockIndex.ToString();
        }

        private bool IsNoValueStatement(ImperativeNode node)
        {
            return (node is IfStmtNode) || (node is ForLoopNode) || (node is WhileStmtNode); 
        }

        private int DfsExprValue(ImperativeNode node)
        {
            if (node is IdentifierNode)
            {
                int val = 0;
                IdentifierNode t = node as IdentifierNode;
                try
                {
                    val = System.Convert.ToInt32(t.Value);
                    return val;
                }
                catch (OverflowException)
                {
                    buildStatus.LogSemanticError(Resources.ArraySizeOverflow, core.CurrentDSFileName, t.line, t.col);
                }
                catch (FormatException)
                {
                    buildStatus.LogSemanticError(Resources.ConstantExpectedInArrayDeclaration, core.CurrentDSFileName, t.line, t.col);
                } 
            }
            else if (node is BinaryExpressionNode)
            {
                BinaryExpressionNode b = node as BinaryExpressionNode;
                Validity.Assert(ProtoCore.DSASM.Operator.mul == b.Optr);
                int left = DfsExprValue(b.LeftNode);
                int right = DfsExprValue(b.RightNode);
                return left * right;
            }
            return 1;
        }

        private void DfsEmitArraySize(ImperativeNode node)
        {
            // s = size * ( i * j * k..n )
            if (node is ArrayNode)
            {
                ArrayNode array = node as ArrayNode;

                ProtoCore.Type type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kInvalidType, 0);
                DfsTraverse(array.Expr, ref type);

                if (array.Type is ArrayNode)
                {
                    DfsEmitArraySize(array.Type);

                    string op = Op.GetOpName(ProtoCore.DSASM.Operator.add);
                    EmitInstrConsole(op);
                    EmitBinary(Op.GetOpCode(ProtoCore.DSASM.Operator.add));
                }
            }
            else
            {
                Validity.Assert(false, "ast error ? check ast construction");
            }
        }

        // this method is used in conjuction with array indexing
        private void DfsEmitArrayIndex(ImperativeNode node, int symbolindex, int index = 0)
        {
            // s = b + ((i * i.w) + (j * j.w) + (n * n.w))

            if (node is ArrayNode)
            {
                ArrayNode array = node as ArrayNode;

                ProtoCore.Type type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0);

                DfsTraverse(array.Expr, ref type);

                // Max size of the current dimension
                int w = codeBlock.symbolTable.symbolList[symbolindex].arraySizeList[index];

                // TODO Jun: Performance improvement
                // Avoid having to generate instructions for the current index if 'w' is 0

                EmitInstrConsole(ProtoCore.DSASM.kw.push, w.ToString());
                StackValue opWidth = StackValue.BuildInt(w);
                EmitPush(opWidth);

                string op = null;
                op = Op.GetOpName(ProtoCore.DSASM.Operator.mul);
                EmitInstrConsole(op);
                EmitBinary(Op.GetOpCode(ProtoCore.DSASM.Operator.mul));

                if (array.Type is ArrayNode)
                {
                    DfsEmitArrayIndex(array.Type, symbolindex, index + 1);

                    op = Op.GetOpName(ProtoCore.DSASM.Operator.add);
                    EmitInstrConsole(op);
                    EmitBinary(Op.GetOpCode(ProtoCore.DSASM.Operator.add));
                }
            }
            else
            {
                Validity.Assert(false, "ast error ? check ast construction");
            }
        }

        // this method is used in conjuction with array var declarations
        private void DfsBuildIndex(ImperativeNode node, List<int> indexlist) 
        {
            if (node is ArrayNode)
            {
                ArrayNode array = node as ArrayNode;
                int exprval = 0;
                if (null != array.Expr) {
                    exprval = DfsExprValue(array.Expr);
                }
                indexlist.Add(exprval);
                DfsBuildIndex(array.Type, indexlist);
            }
        }

        private void AllocateArray(ProtoCore.DSASM.SymbolNode symbol, ImperativeNode nodeArray)
        {
            symbol.isArray = true;

            //===================================================
            // TODO Jun: 
            //  Determine which is optimal-
            //  1. Storing the array flag in the symbol, or...
            //  2. Storing the array flag as an instruction operand
            //===================================================

            // TODO Jun: allocate to the stack is the array has empty expressions
            ArrayNode array = nodeArray as ArrayNode;
            bool heapAlloc = null != array.Expr;
            symbol.memregion = heapAlloc ? ProtoCore.DSASM.MemoryRegion.kMemHeap : ProtoCore.DSASM.MemoryRegion.kMemStack;

            if (ProtoCore.DSASM.MemoryRegion.kMemStack == symbol.memregion)
            {
                symbol.arraySizeList = new List<int>();
                List<int> indexlist = new List<int>();

                // TODO Jun: Optimize this
                DfsBuildIndex(nodeArray, indexlist);
                foreach (int indexVal in indexlist) 
                {
                    if (0 != indexVal) 
                    {
                        symbol.size *= indexVal;
                    }
                }

                // Rebuild the array that is needed to compute the size at each index
                indexlist.RemoveAt(0);
                indexlist.Add(symbol.datasize);

                for (int n = 0; n < indexlist.Count; ++n)
                {
                    symbol.arraySizeList.Add(1);
                    for (int i = n; i < indexlist.Count; ++i)
                    {
                        symbol.arraySizeList[n] *= indexlist[i];
                    }
                }
            }
            else if (ProtoCore.DSASM.MemoryRegion.kMemHeap == symbol.memregion)
            {
                int indexCnt = DfsEmitArrayIndexHeap(nodeArray);

                EmitInstrConsole(ProtoCore.DSASM.kw.push, indexCnt.ToString());
                StackValue opSize = StackValue.BuildInt(indexCnt);
                EmitPush(opSize);
                SetHeapData(symbol);
            }
            else
            {
                Validity.Assert(false, "Invalid memory region");
            }
            SetStackIndex(symbol);
        }

        private ProtoCore.DSASM.SymbolNode Allocate( 
            string ident, 
            int funcIndex, 
            ProtoCore.Type datatype, 
            int size = 1, 
            int datasize = ProtoCore.DSASM.Constants.kPrimitiveSize, 
            ImperativeNode nodeArray = null,
            ProtoCore.DSASM.MemoryRegion region = ProtoCore.DSASM.MemoryRegion.kMemStack)
        {
            if (core.ClassTable.IndexOf(ident) != ProtoCore.DSASM.Constants.kInvalidIndex)
                buildStatus.LogSemanticError(String.Format(Resources.ClassNameAsVariableError,ident));

            ProtoCore.DSASM.SymbolNode symbolnode = new ProtoCore.DSASM.SymbolNode(
                ident, 
                ProtoCore.DSASM.Constants.kInvalidIndex, 
                ProtoCore.DSASM.Constants.kInvalidIndex, 
                funcIndex, 
                datatype,
                TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                size, 
                datasize,
                false,
                codeBlock.symbolTable.RuntimeIndex,
                region,
                false,
                null,
                globalClassIndex,
                ProtoCore.CompilerDefinitions.AccessModifier.kPublic,
                false,
                codeBlock.codeBlockId);

            if (this.isEmittingImportNode)
                symbolnode.ExternLib = core.CurrentDSFileName;


            Validity.Assert(ProtoCore.DSASM.Constants.kInvalidIndex == symbolnode.symbolTableIndex);

            if (null == nodeArray)
            {
                AllocateVar(symbolnode);
            }
            else
            {
                AllocateArray(symbolnode, nodeArray);
            }

            // This is to handle that a variable is defined in a language
            // block which defined in a function, so the variable's scope
            // is that language block instead of function
            if (IsInLanguageBlockDefinedInFunction())
            {
                symbolnode.classScope = Constants.kGlobalScope;
                symbolnode.functionIndex = Constants.kGlobalScope;
            }

            int symbolindex = ProtoCore.DSASM.Constants.kInvalidIndex;
            if (ProtoCore.DSASM.Constants.kInvalidIndex != globalClassIndex && !IsInLanguageBlockDefinedInFunction())
            {
                symbolindex = core.ClassTable.ClassNodes[globalClassIndex].symbols.Append(symbolnode);
            }
            else
            {
                symbolindex = codeBlock.symbolTable.Append(symbolnode);                
            }

            symbolnode.symbolTableIndex = symbolindex;
            return symbolnode;
        }

        private int AllocateArg(
            string ident, 
            int funcIndex, 
            ProtoCore.Type datatype, 
            int size = 1,
            int datasize = ProtoCore.DSASM.Constants.kPrimitiveSize,
            ImperativeNode nodeArray = null,
            ProtoCore.DSASM.MemoryRegion region = ProtoCore.DSASM.MemoryRegion.kMemStack)
        {
            ProtoCore.DSASM.SymbolNode symbolnode = new ProtoCore.DSASM.SymbolNode(
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
                region);
            symbolnode.codeBlockId = codeBlock.codeBlockId;
            if (this.isEmittingImportNode)
                symbolnode.ExternLib = core.CurrentDSFileName;

            int symbolindex = ProtoCore.DSASM.Constants.kInvalidIndex;
            if (ProtoCore.DSASM.Constants.kInvalidIndex != codeBlock.symbolTable.IndexOf(symbolnode))
            {
                buildStatus.LogSemanticError(String.Format(Resources.IdentifierRedefinition,ident));
            }
            else
            {
                int locOffset = localProcedure.localCount;
                locOffset = localProcedure.localCount;
                symbolnode.index = -1 - ProtoCore.DSASM.StackFrame.kStackFrameSize - (locOffset + argOffset);
                ++argOffset;

                symbolindex = codeBlock.symbolTable.Append(symbolnode);
            }
            return symbolindex;
        }

        protected override void EmitReturn(int line = ProtoCore.DSASM.Constants.kInvalidIndex, int col = ProtoCore.DSASM.Constants.kInvalidIndex,
            int endline = ProtoCore.DSASM.Constants.kInvalidIndex, int endcol = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.RETURN;

            AuditReturnLocationFromFunction(ref line, ref col, ref endline, ref endcol);

            ++pc;
            instr.debug = GetDebugObject(line, col, endline, endcol, pc);
            codeBlock.instrStream.instrList.Add(instr);

            // TODO: Figure out why using AppendInstruction fails for adding these instructions to ExpressionInterpreter
            //AppendInstruction(instr, line, col);
            updatePcDictionary(line, col);
        }

        protected override void EmitRetb(int line = ProtoCore.DSASM.Constants.kInvalidIndex, int col = ProtoCore.DSASM.Constants.kInvalidIndex,
            int endline = ProtoCore.DSASM.Constants.kInvalidIndex, int endcol = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.RETB;

            AuditReturnLocationFromCodeBlock(ref line, ref col, ref endline, ref endcol);

            ++pc;
            instr.debug = GetDebugObject(line, col, endline, endcol, pc);
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
            AuditReturnLocationFromCodeBlock(ref line, ref col, ref endline, ref endcol);

            ++pc;
            instr.debug = GetDebugObject(line, col, endline, endcol, pc);
            codeBlock.instrStream.instrList.Add(instr);

            // TODO: Figure out why using AppendInstruction fails for adding these instructions to ExpressionInterpreter
            //AppendInstruction(instr, line, col);
            updatePcDictionary(line, col);
        }

        private void EmitPushArrayKey(int symbol, int block, int classIndex)
        {
            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.PUSH_ARRAYKEY;
            instr.op1 = StackValue.BuildVarIndex(symbol);
            instr.op2 = StackValue.BuildBlockIndex(block);
            instr.op3 = StackValue.BuildClassIndex(classIndex);

            ++pc;
            codeBlock.instrStream.instrList.Add(instr);

            // TODO: Figure out why using AppendInstruction fails for adding these instructions to ExpressionInterpreter
            //AppendInstruction(instr);
        }

        public override ProtoCore.DSASM.ProcedureNode TraverseFunctionCall(ProtoCore.AST.Node node, ProtoCore.AST.Node parentNode, int lefttype, int depth, ref ProtoCore.Type inferedType, 
            ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, 
            ProtoCore.AST.Node bnode = null)
        {
            if (!IsParsingGlobal() && !IsParsingGlobalFunctionBody())
            {
                return null;
            }

            //Validity.Assert(null == graphNode);

            FunctionCallNode funcCall = node as FunctionCallNode;
            string procName = funcCall.Function.Name;
            List<ProtoCore.Type> arglist = new List<ProtoCore.Type>();
            foreach (ImperativeNode paramNode in funcCall.FormalArguments)
            {
                ProtoCore.Type paramType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0);

                // If it's a binary node then continue type check, otherwise disable type check and just take the type of paramNode itself
                // f(1+2.0) -> type check enabled - param is typed as double
                // f(2) -> type check disabled - param is typed as int
                enforceTypeCheck = !(paramNode is BinaryExpressionNode);

                DfsTraverse(paramNode, ref paramType, false, graphNode, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, bnode);
                enforceTypeCheck = true;

                arglist.Add(paramType);
            }

            ProtoCore.DSASM.ProcedureNode procNode = null;
            int type = ProtoCore.DSASM.Constants.kInvalidIndex;
            bool isConstructor = false;
            bool isStatic = false;
            bool hasLogError = false;

            int refClassIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
            if (parentNode != null && parentNode is ProtoCore.AST.ImperativeAST.IdentifierListNode)
            {
                ProtoCore.AST.Node leftnode = (parentNode as ProtoCore.AST.ImperativeAST.IdentifierListNode).LeftNode;
                if (leftnode != null && leftnode is ProtoCore.AST.ImperativeAST.IdentifierNode)
                {
                    refClassIndex = core.ClassTable.IndexOf(leftnode.Name);
                }
            }

            // If lefttype is a valid class then check if calling a constructor
            if ((int)ProtoCore.PrimitiveType.kInvalidType != inferedType.UID && (int)ProtoCore.PrimitiveType.kTypeVoid != inferedType.UID)
            {
                bool isAccessible;
                int realType;

                if (procName != ProtoCore.DSASM.Constants.kFunctionPointerCall)
                {
                    bool isStaticOrConstructor = refClassIndex != ProtoCore.DSASM.Constants.kInvalidIndex;
                    procNode = core.ClassTable.ClassNodes[inferedType.UID].GetMemberFunction(procName, arglist, globalClassIndex, out isAccessible, out realType, isStaticOrConstructor);

                    if (procNode != null)
                    {
                        Validity.Assert(realType != ProtoCore.DSASM.Constants.kInvalidIndex);
                        isConstructor = procNode.isConstructor;
                        isStatic = procNode.isStatic;
                        type = lefttype = realType;

                        if (!isAccessible)
                        {
                            type = lefttype = realType;
                            string message = String.Format(ProtoCore.Properties.Resources.kMethodIsInaccessible, procName);
                            buildStatus.LogWarning(WarningID.kAccessViolation, message, core.CurrentDSFileName, funcCall.line, funcCall.col, graphNode);
                            hasLogError = true;
                        }
                    }
                    // To support unamed constructor, x = A();
                    else if (refClassIndex != Constants.kInvalidIndex)
                    {
                        string message = String.Format(ProtoCore.Properties.Resources.kCallingNonStaticMethod, core.ClassTable.ClassNodes[refClassIndex].name, procName);
                        buildStatus.LogWarning(WarningID.kCallingNonStaticMethodOnClass, message, core.CurrentDSFileName, funcCall.line, funcCall.col, graphNode);
                        inferedType.UID = (int)PrimitiveType.kTypeNull;
                        EmitPushNull();
                        return null;
                    }
                    else
                    {
                        int classIndex = core.ClassTable.IndexOf(procName);
                        int dummy;

                        if (classIndex != Constants.kInvalidIndex)
                        {
                            procNode = core.ClassTable.ClassNodes[classIndex].GetMemberFunction(procName, arglist, globalClassIndex, out isAccessible, out dummy, true);
                            if (procNode != null && procNode.isConstructor)
                            {
                                type = classIndex;
                            }
                            else
                            {
                                procNode = null;
                            }
                        }
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
                            string message = String.Format(ProtoCore.Properties.Resources.kMethodIsInaccessible, procName);
                            buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kAccessViolation, message, core.CurrentDSFileName, funcCall.line, funcCall.col, graphNode);

                            inferedType.UID = (int)PrimitiveType.kTypeNull;
                            EmitPushNull();
                            return null;
                        }
                    }
                }
            }

            if (null != procNode)
            {
                inferedType = procNode.returntype;

                if (ProtoCore.DSASM.Constants.kInvalidIndex != procNode.procId)
                {
                    // The function is at block 0 if its a constructor, member or at the globals scope.
                    // Its at block 1 if its inside a language block. 
                    // Its limited to block 1 as of R1 since we dont support nested function declarations yet
                    int blockId = procNode.runtimeIndex;

                    //push value-not-provided default argument
                    for (int i = arglist.Count; i < procNode.argInfoList.Count; i++)
                    {
                        EmitDefaultArgNode();
                    }

                    // Push the function declaration block  
                    // Jun TODO: Implementeation of indexing into a function call:
                    //  x = f()[0][1]
                    int dimensions = 0;
                    EmitPushVarData(dimensions);

                    // Emit depth
                    EmitInstrConsole(kw.push, depth + "[depth]");
                    EmitPush(StackValue.BuildInt(depth));

                    // The function call
                    EmitInstrConsole(ProtoCore.DSASM.kw.callr, procNode.name);

                    DebugProperties.BreakpointOptions oldOptions = core.DebuggerProperties.breakOptions;
                    if(procNode.name.StartsWith(Constants.kSetterPrefix))
                    {
                        EmitCall(procNode.procId, blockId, type, parentNode.line, parentNode.col, parentNode.endLine, parentNode.endCol);
                    }
                    else if (bnode != null)
                    {
                        EmitCall(procNode.procId, blockId, type, bnode.line, bnode.col, bnode.endLine, bnode.endCol);
                    }
                    else if (!procNode.name.Equals(Constants.kFunctionRangeExpression) ||
                        oldOptions.HasFlag(DebugProperties.BreakpointOptions.EmitCallrForTempBreakpoint))
                    {
                        EmitCall(procNode.procId, blockId, type, node.line, node.col, node.endLine, node.endCol);
                    }
                    else
                    {
                        EmitCall(procNode.procId, blockId, type);
                    }
                    EmitInstrConsole(ProtoCore.DSASM.kw.push, ProtoCore.DSASM.kw.regRX);
                    StackValue opReturn = StackValue.BuildRegister(Registers.RX);
                    EmitPush(opReturn);
                }
            }
            else
            {
                if (depth <= 0 && procName != ProtoCore.DSASM.Constants.kFunctionPointerCall)
                {
                    if (!hasLogError)
                    {
                        if (!core.Options.SuppressFunctionResolutionWarning || parentNode == null)
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
                        }
                        inferedType.UID = (int)PrimitiveType.kTypeNull;
                        EmitPushNull();
                    }
                }
                else
                {
                    DynamicFunction dynFunc = null;
                    if (procName == Constants.kFunctionPointerCall && depth == 0)
                    {
                        if (!core.DynamicFunctionTable.TryGetFunction(procName, 
                                                                      arglist.Count, 
                                                                      lefttype, 
                                                                      out dynFunc))
                        {
                            dynFunc = core.DynamicFunctionTable.AddNewFunction(procName, arglist.Count, lefttype);
                        }
                        var iNode = nodeBuilder.BuildIdentfier(funcCall.Function.Name);
                        EmitIdentifierNode(iNode, ref inferedType);
                    }
                    else
                    {
                        if (!core.DynamicFunctionTable.TryGetFunction(procName, 
                                                                      arglist.Count, 
                                                                      lefttype, 
                                                                      out dynFunc))
                        {
                            dynFunc = core.DynamicFunctionTable.AddNewFunction(procName, arglist.Count, lefttype);
                        }
                    }

                    // Emit depth
                    EmitInstrConsole(kw.push, depth + "[depth]");
                    EmitPush(StackValue.BuildInt(depth));

                    // The function call
                    EmitInstrConsole(ProtoCore.DSASM.kw.callr, funcCall.Function.Name + "[dynamic]");
                    EmitDynamicCall(dynFunc.Index, globalClassIndex, funcCall.line, funcCall.col, funcCall.endLine, funcCall.endCol);

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

        private bool AuditReturnLocationFromCodeBlock(ref int line, ref int col, ref int endLine, ref int endCol)
        {
            if (null != localCodeBlockNode)
            {
                if (localCodeBlockNode is CodeBlockNode ||
                    localCodeBlockNode is IfStmtPositionNode ||
                    localCodeBlockNode is WhileStmtNode)
                {
                    ImperativeNode codeBlockNode = localCodeBlockNode as ImperativeNode;
                    if (null == codeBlockNode)
                        return false;

                    col = codeBlockNode.endCol - 1;
                    endCol = codeBlockNode.endCol;
                    line = endLine = codeBlockNode.endLine;
                    return true;
                }
            }

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

            return false;
        }

        private bool AuditReturnLocationFromFunction(ref int line, ref int col, ref int endLine, ref int endCol)
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
                if (localCodeBlockNode is CodeBlockNode ||
                    localCodeBlockNode is IfStmtPositionNode ||
                    localCodeBlockNode is WhileStmtNode)
                {
                    ImperativeNode codeBlockNode = localCodeBlockNode as ImperativeNode;
                    if (null == codeBlockNode)
                        return false;

                    col = codeBlockNode.endCol - 1;
                    endCol = codeBlockNode.endCol;
                    line = endLine = codeBlockNode.endLine;
                    return true;
                }
            }

            return false;
        }

        private int EmitExpressionInterpreter(ProtoCore.AST.Node codeBlockNode)
        {
            core.watchStartPC = this.pc;
            compilePass = ProtoCore.CompilerDefinitions.Imperative.CompilePass.kGlobalScope;
            ProtoCore.AST.ImperativeAST.CodeBlockNode codeblock = codeBlockNode as ProtoCore.AST.ImperativeAST.CodeBlockNode;

            ProtoCore.Type inferedType = new ProtoCore.Type();
            foreach (ImperativeNode node in codeblock.Body)
            {
                inferedType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0);

                DfsTraverse(node, ref inferedType);

                BinaryExpressionNode binaryNode = node as BinaryExpressionNode;
            }
            core.InferedType = inferedType;

            this.pc = core.watchStartPC;

            return codeBlock.codeBlockId;
        }


        public override int Emit(ProtoCore.AST.Node codeBlockNode, ProtoCore.AssociativeGraph.GraphNode graphNode = null)
        {
            core.watchStartPC = this.pc;
            if (core.Options.RunMode == ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter)
            {
                return EmitExpressionInterpreter(codeBlockNode);
            }

            this.localCodeBlockNode = codeBlockNode;
            ProtoCore.AST.ImperativeAST.CodeBlockNode codeblock = codeBlockNode as ProtoCore.AST.ImperativeAST.CodeBlockNode;
            bool isTopBlock = null == codeBlock.parent;
            if (!isTopBlock)
            {
                // If this is an inner block where there can be no classes, we can start at parsing at the global function state
                compilePass = ProtoCore.CompilerDefinitions.Imperative.CompilePass.kGlobalFuncSig;
            }

            bool hasReturnStatement = false;
            ProtoCore.Type type = new ProtoCore.Type();
            while (ProtoCore.CompilerDefinitions.Imperative.CompilePass.kDone != compilePass)
            {
                foreach (ImperativeNode node in codeblock.Body)
                {
                    type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0);

                    if (node is LanguageBlockNode)
                    {
                        // Build a binary node with a temporary lhs for every stand-alone language block
                        var iNode = nodeBuilder.BuildIdentfier(core.GenerateTempLangageVar());
                        var langBlockNode = nodeBuilder.BuildBinaryExpression(iNode, node);
                        DfsTraverse(langBlockNode, ref type, false, graphNode);
                    }
                    else
                    {
                        DfsTraverse(node, ref type, false, graphNode);
                    }                    

                    if (ProtoCore.Utils.NodeUtils.IsReturnExpressionNode(node))
                        hasReturnStatement = true;
                }
                if (compilePass == ProtoCore.CompilerDefinitions.Imperative.CompilePass.kGlobalScope && !hasReturnStatement)
                {
                    EmitReturnNull();
                }

                compilePass++;
            }

            core.InferedType = type;

            if (core.AsmOutput != Console.Out)
            {
                core.AsmOutput.Flush();
            }

            this.localCodeBlockNode = null;
            return codeBlock.codeBlockId;
        }

        private void EmitIdentifierNode(ImperativeNode node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null)
        {
            IdentifierNode t = node as IdentifierNode;
            if (t.Name.Equals(ProtoCore.DSDefinitions.Keyword.This))
            {
                if (localProcedure != null)
                {
                    if (localProcedure.isStatic)
                    {
                        string message = ProtoCore.Properties.Resources.kUsingThisInStaticFunction;
                        core.BuildStatus.LogWarning(WarningID.kInvalidThis, message, core.CurrentDSFileName, t.line, t.col, graphNode);
                        EmitPushNull();
                        return;
                    }
                    else if (localProcedure.classScope == Constants.kGlobalScope)
                    {
                        string message = ProtoCore.Properties.Resources.kInvalidThis;
                        core.BuildStatus.LogWarning(WarningID.kInvalidThis, message, core.CurrentDSFileName, t.line, t.col, graphNode);
                        EmitPushNull();
                        return;
                    }
                    else
                    {
                        EmitThisPointerNode();
                        return;
                    }
                }
                else
                {
                    string message = ProtoCore.Properties.Resources.kInvalidThis;
                    core.BuildStatus.LogWarning(WarningID.kInvalidThis, message, core.CurrentDSFileName, t.line, t.col, graphNode);
                    EmitPushNull();
                    return;
                }
            }

            int dimensions = 0;

            int runtimeIndex = codeBlock.symbolTable.RuntimeIndex;

            ProtoCore.Type type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0);

            ProtoCore.DSASM.SymbolNode symbolnode = null;
            //bool isAllocated = VerifyAllocation(t.Value, out blockId, out localAllocBlock, out symindex, ref type);
            //bool allocatedLocally = isAllocated && core.runtimeTableIndex == localAllocBlock;
            //bool allocatedExternally = isAllocated && core.runtimeTableIndex > localAllocBlock;
            //bool isVisible = isAllocated && core.runtimeTableIndex >= localAllocBlock;
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
                        inferedType.UID = (int)PrimitiveType.kTypeFunctionPointer;

                        int fptr = core.FunctionPointerTable.functionPointerDictionary.Count;
                        var fptrNode = new ProtoCore.DSASM.FunctionPointerNode(procNode);
                        core.FunctionPointerTable.functionPointerDictionary.TryAdd(fptr, fptrNode);
                        core.FunctionPointerTable.functionPointerDictionary.TryGetBySecond(fptrNode, out fptr);

                        EmitPushVarData(0);

                        EmitInstrConsole(ProtoCore.DSASM.kw.push, t.Name);
                        StackValue opFunctionPointer = StackValue.BuildFunctionPointer(fptr);
                        EmitPush(opFunctionPointer, runtimeIndex, t.line, t.col);
                        return;
                    }
                }
            }

            bool isAllocated = VerifyAllocation(t.Value, globalClassIndex, globalProcIndex, out symbolnode, out isAccessible);
            if (!isAllocated || !isAccessible)
            {
                if (isAllocated)
                {
                    if (!isAccessible)
                    {
                        string message = String.Format(ProtoCore.Properties.Resources.kPropertyIsInaccessible, t.Value);
                        buildStatus.LogWarning(WarningID.kAccessViolation, message, core.CurrentDSFileName, t.line, t.col, graphNode);
                    }
                }
                else
                {
                    string message = String.Format(ProtoCore.Properties.Resources.kUnboundIdentifierMsg, t.Value);
                    buildStatus.LogWarning(WarningID.kIdUnboundIdentifier, message, core.CurrentDSFileName, t.line, t.col, graphNode);
                }

                inferedType.UID = (int)ProtoCore.PrimitiveType.kTypeNull;

                // Jun Comment: Specification excerpt
                //      If resolution fails at this point a com.Design-Script.Imperative.Core.UnboundIdentifier 
                //      warning is emitted during pre-execute phase, and at the ID is bound to null. (R1 - Feb)

                EmitPushNull();

                EmitPushVarData(dimensions);

                ProtoCore.Type varType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0);
                symbolnode = Allocate(t.Value, globalProcIndex, varType);

                EmitInstrConsole(ProtoCore.DSASM.kw.pop, t.Value);
                EmitPopForSymbol(symbolnode, runtimeIndex);
            }
            else
            {
                type = symbolnode.datatype;
                runtimeIndex = symbolnode.runtimeTableIndex;

                if (core.Options.AssociativeToImperativePropagation)
                {
                    // Comment Jun: If this symbol belongs to an outer block, then append it to this language blocks dependent
                    if (symbolnode.codeBlockId != codeBlock.codeBlockId)
                    {
                        // A parent codeblock owns this symbol
                        if (null != graphNode)
                        {
                            ProtoCore.AssociativeGraph.GraphNode dependentNode = new ProtoCore.AssociativeGraph.GraphNode();
                            dependentNode.PushSymbolReference(symbolnode);
                            graphNode.PushDependent(dependentNode);
                        }
                    }
                }
            }


            if (null != t.ArrayDimensions)
            {
                dimensions = DfsEmitArrayIndexHeap(t.ArrayDimensions);
            }

            //fix type's rank    
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

            EmitInstrConsole(ProtoCore.DSASM.kw.push, t.Value);
            EmitPushForSymbol(symbolnode, runtimeIndex, t);

            if (core.TypeSystem.IsHigherRank(type.UID, inferedType.UID))
            {
                inferedType = type;
            }
            // We need to get inferedType for boolean variable so that we can perform type check
            inferedType.UID = (isBooleanOp || (type.UID == (int)PrimitiveType.kTypeBool)) ? (int)PrimitiveType.kTypeBool : type.UID;
        }
#if ENABLE_INC_DEC_FIX
        private void EmitPostFixNode(ImperativeNode node, ref ProtoCore.Type inferedType)
        {
            bool parseGlobal = null == localProcedure && ProtoCore.CompilerDefinitions.Imperative.CompilePass.kAll == compilePass;
            bool parseGlobalFunction = null != localProcedure && ProtoCore.CompilerDefinitions.Imperative.CompilePass.kGlobalFuncBody == compilePass;

            if (parseGlobal || parseGlobalFunction)
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
        private void EmitLanguageBlockNode(ImperativeNode node, ref ProtoCore.Type inferedType, ProtoCore.AssociativeGraph.GraphNode propogateUpdateGraphNode = null)
        {
            if (IsParsingGlobal() || IsParsingGlobalFunctionBody())
            {
                LanguageBlockNode langblock = node as LanguageBlockNode;
                //(Fuqiang, Ayush) : Throwing an assert stops NUnit. Negative tests expect to catch a 
                // CompilerException, so we throw that instead.
                //Validity.Assert(ProtoCore.Language.kInvalid != langblock.codeblock.language);

                if (ProtoCore.Language.kInvalid == langblock.codeblock.language)
                {
                    throw new ProtoCore.Exceptions.CompileErrorsOccured("Invalid language block");
                }

                ProtoCore.CompileTime.Context context = new ProtoCore.CompileTime.Context();
                // Save the guid of the current scope (which is stored in the current graphnodes) to the nested language block.
                // This will be passed on to the nested language block that will be compiled
                if (propogateUpdateGraphNode != null)
                {
                    context.guid = propogateUpdateGraphNode.guid;
                }

                int entry = 0;
                int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
                if (ProtoCore.Language.kImperative == langblock.codeblock.language)
                {
                    // TODO Jun: Move the associative and all common string into some table
                    buildStatus.LogSyntaxError(Resources.InvalidNestedImperativeBlock, core.CurrentDSFileName, langblock.line, langblock.col);
                }

                if (globalProcIndex != ProtoCore.DSASM.Constants.kInvalidIndex && core.ProcNode == null)
                    core.ProcNode = codeBlock.procedureTable.procList[globalProcIndex];

                core.Compilers[langblock.codeblock.language].Compile(out blockId, codeBlock, langblock.codeblock, context, codeBlock.EventSink, langblock.CodeBlockNode);

                if (propogateUpdateGraphNode != null)
                {
                    propogateUpdateGraphNode.languageBlockId = blockId;
                    CodeBlock childBlock = core.CompleteCodeBlockList[blockId];
                    foreach (var subGraphNode in childBlock.instrStream.dependencyGraph.GraphList)
                    {
                        foreach (var depentNode in subGraphNode.dependentList)
                        {
                            if (depentNode.updateNodeRefList != null 
                                && depentNode.updateNodeRefList.Count > 0 
                                && depentNode.updateNodeRefList[0].nodeList != null
                                && depentNode.updateNodeRefList[0].nodeList.Count > 0)
                            {
                                SymbolNode dependentSymbol = depentNode.updateNodeRefList[0].nodeList[0].symbol;
                                int symbolBlockId = dependentSymbol.codeBlockId;
                                if (symbolBlockId != Constants.kInvalidIndex)
                                {
                                    CodeBlock symbolBlock = core.CompleteCodeBlockList[symbolBlockId];
                                    if (!symbolBlock.IsMyAncestorBlock(codeBlock.codeBlockId))
                                    {
                                        propogateUpdateGraphNode.PushDependent(depentNode);
                                    }
                                }
                            }
                        }
                    }
                }

                setBlkId(blockId);
                inferedType = core.InferedType;
                //Validity.Assert(codeBlock.children[codeBlock.children.Count - 1].blockType == ProtoCore.DSASM.CodeBlockType.kLanguage);
                codeBlock.children[codeBlock.children.Count - 1].Attributes = PopulateAttributes(langblock.Attributes);

                EmitInstrConsole("bounce " + blockId + ", " + entry.ToString());
                EmitBounceIntrinsic(blockId, entry);

                // The callee language block will have stored its result into the RX register. 
                EmitInstrConsole(ProtoCore.DSASM.kw.push, ProtoCore.DSASM.kw.regRX);
                StackValue opRes = StackValue.BuildRegister(Registers.RX);
                EmitPush(opRes);
            }
        }

        private void EmitClassDeclNode(ImperativeNode node)
        {
            throw new NotImplementedException();
        }

        private void EmitConstructorDefinitionNode(ImperativeNode node)
        {
            throw new NotImplementedException();
        }

        private void EmitFunctionDefinitionNode(ImperativeNode node, ref ProtoCore.Type inferedType)
        {
            bool parseGlobalFunctionSig = null == localProcedure && ProtoCore.CompilerDefinitions.Imperative.CompilePass.kGlobalFuncSig == compilePass;
            bool parseGlobalFunctionBody = null == localProcedure && ProtoCore.CompilerDefinitions.Imperative.CompilePass.kGlobalFuncBody == compilePass;

            FunctionDefinitionNode funcDef = node as FunctionDefinitionNode;
            localFunctionDefNode = funcDef;

            ProtoCore.DSASM.CodeBlockType originalBlockType = codeBlock.blockType;
            codeBlock.blockType = ProtoCore.DSASM.CodeBlockType.kFunction;
            if (parseGlobalFunctionSig)
            {
                Validity.Assert(null == localProcedure);


                // TODO jun: Add semantics for checking overloads (different parameter types)
                localProcedure = new ProtoCore.DSASM.ProcedureNode();
                localProcedure.name = funcDef.Name;
                localProcedure.pc = pc;
                localProcedure.localCount = funcDef.localVars;
                localProcedure.returntype.UID = core.TypeSystem.GetType(funcDef.ReturnType.Name);
                if (localProcedure.returntype.UID == (int)PrimitiveType.kInvalidType)
                {
                    string message = String.Format(ProtoCore.Properties.Resources.kReturnTypeUndefined, funcDef.ReturnType.Name, funcDef.Name);
                    buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kTypeUndefined, message, null, funcDef.line, funcDef.col, firstSSAGraphNode);
                    localProcedure.returntype.UID = (int)PrimitiveType.kTypeVar;
                }
                localProcedure.returntype.rank = funcDef.ReturnType.rank;
                localProcedure.runtimeIndex = codeBlock.codeBlockId;
                globalProcIndex = codeBlock.procedureTable.Append(localProcedure);
                core.ProcNode = localProcedure;


                // Append arg symbols
                if (null != funcDef.Signature)
                {
                    foreach (VarDeclNode argNode in funcDef.Signature.Arguments)
                    {
                        IdentifierNode paramNode = null;
                        ProtoCore.AST.Node aDefaultExpression = null;
                        if (argNode.NameNode is IdentifierNode)
                        {
                            paramNode = argNode.NameNode as IdentifierNode;
                        }
                        else if (argNode.NameNode is BinaryExpressionNode)
                        {
                            BinaryExpressionNode bNode = argNode.NameNode as BinaryExpressionNode;
                            paramNode = bNode.LeftNode as IdentifierNode;
                            aDefaultExpression = bNode;
                        }
                        else
                        {
                            Validity.Assert(false, "Check generated AST");
                        }

                        ProtoCore.Type argType = BuildArgumentTypeFromVarDeclNode(argNode, firstSSAGraphNode);
                        int symbolIndex = AllocateArg(paramNode.Value, localProcedure.procId, argType);
                        if (ProtoCore.DSASM.Constants.kInvalidIndex == symbolIndex)
                        {
                            throw new BuildHaltException("26384684");
                        }

                        localProcedure.argTypeList.Add(argType);
                        ProtoCore.DSASM.ArgumentInfo argInfo = new ProtoCore.DSASM.ArgumentInfo { DefaultExpression = aDefaultExpression };
                        localProcedure.argInfoList.Add(argInfo);
                    }
                }         
            }
            else if (parseGlobalFunctionBody)
            {
                EmitCompileLogFunctionStart(GetFunctionSignatureString(funcDef.Name, funcDef.ReturnType, funcDef.Signature));

                // Build arglist for comparison
                List<ProtoCore.Type> argList = new List<ProtoCore.Type>();
                if (null != funcDef.Signature)
                {
                    foreach (VarDeclNode argNode in funcDef.Signature.Arguments)
                    {
                        ProtoCore.Type argType = BuildArgumentTypeFromVarDeclNode(argNode, firstSSAGraphNode);
                        argList.Add(argType);
                    }
                }

                // Get the exisitng procedure that was added on the previous pass
                globalProcIndex = codeBlock.procedureTable.IndexOfExact(funcDef.Name, argList, false);
                localProcedure = codeBlock.procedureTable.procList[globalProcIndex];


                Validity.Assert(null != localProcedure);
                localProcedure.Attributes = PopulateAttributes(funcDef.Attributes);
                // Its only on the parse body pass where the real pc is determined. Update this procedures' pc
                //Validity.Assert(ProtoCore.DSASM.Constants.kInvalidIndex == localProcedure.pc);
                localProcedure.pc = pc;

                // Copy the active function to the core so nested language blocks can refer to it
                core.ProcNode = localProcedure;

                // Arguments have been allocated, update the baseOffset
                localProcedure.localCount = core.BaseOffset;


                ProtoCore.FunctionEndPoint fep = null;
                                
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
                    var iNodeTemp = nodeBuilder.BuildIdentfier(Constants.kTempDefaultArg);
                    BinaryExpressionNode bNodeTemp = nodeBuilder.BuildBinaryExpression(iNodeTemp, bNode.LeftNode) as BinaryExpressionNode;
                    EmitBinaryExpressionNode(bNodeTemp, ref inferedType);

                    //duild an inline conditional node for statement: defaultarg = (temp == DefaultArgNode) ? defaultValue : temp;
                    InlineConditionalNode icNode = new InlineConditionalNode();
                    icNode.ConditionExpression = nodeBuilder.BuildBinaryExpression(iNodeTemp, new DefaultArgNode(), Operator.eq);
                    icNode.TrueExpression = bNode.RightNode;
                    icNode.FalseExpression = iNodeTemp;
                    bNodeTemp.LeftNode = bNode.LeftNode;
                    bNodeTemp.RightNode = icNode;
                    EmitBinaryExpressionNode(bNodeTemp, ref inferedType);
                }
                emitDebugInfo = true;

                // Traverse definition
                bool hasReturnStatement = false;
                foreach (ImperativeNode bnode in funcDef.FunctionBody.Body)
                {
                    DfsTraverse(bnode, ref inferedType);
                    if (ProtoCore.Utils.NodeUtils.IsReturnExpressionNode(bnode))
                    {
                        hasReturnStatement = true;
                    }

                    if (bnode is FunctionCallNode)
                    {
                        EmitSetExpressionUID(core.ExpressionUID++);
                    }
                }

                // All locals have been stack allocated, update the local count of this function
                localProcedure.localCount = core.BaseOffset;

                // Update the param stack indices of this function
                foreach (ProtoCore.DSASM.SymbolNode symnode in codeBlock.symbolTable.symbolList.Values)
                {
                    if (symnode.functionIndex == localProcedure.procId && symnode.isArgument)
                    {
                        symnode.index -= localProcedure.localCount;
                    }
                }

                ProtoCore.Lang.JILActivationRecord record = new ProtoCore.Lang.JILActivationRecord();
                record.pc = localProcedure.pc;
                record.locals = localProcedure.localCount;
                record.classIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
                record.funcIndex = localProcedure.procId;
                fep = new ProtoCore.Lang.JILFunctionEndPoint(record);



                // Construct the fep arguments
                fep.FormalParams = new ProtoCore.Type[localProcedure.argTypeList.Count];
                fep.BlockScope = codeBlock.codeBlockId;
                fep.procedureNode = localProcedure;
                localProcedure.argTypeList.CopyTo(fep.FormalParams, 0);

                // TODO Jun: 'classIndexAtCallsite' is the class index as it is stored at the callsite function tables
                // Determine whether this still needs to be aligned to the actual 'classIndex' variable
                // The factors that will affect this is whether the 2 function tables (compiler and callsite) need to be merged
                int classIndexAtCallsite = ProtoCore.DSASM.Constants.kInvalidIndex + 1;
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

                if (!hasReturnStatement)
                {
                    if (!core.Options.SuppressFunctionResolutionWarning)
                    {
                        string message = String.Format(ProtoCore.Properties.Resources.kFunctionNotReturnAtAllCodePaths, localProcedure.name);
                        core.BuildStatus.LogWarning(ProtoCore.BuildData.WarningID.kMissingReturnStatement, message, core.CurrentDSFileName, funcDef.line, funcDef.col, firstSSAGraphNode);
                    }

                    EmitReturnNull();
                }

                EmitCompileLogFunctionEnd();
                //Fuqiang: return is already done in traversing the function body
                //// function return
                //EmitInstrConsole(ProtoCore.DSASM.kw.ret);
                //EmitReturn();
            }

            core.ProcNode = localProcedure = null;
            globalProcIndex = ProtoCore.DSASM.Constants.kGlobalScope;
            argOffset = 0;
            core.BaseOffset = 0;
            codeBlock.blockType = originalBlockType;
            localFunctionDefNode = null;
        }

        private void EmitFunctionCallNode(ImperativeNode node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.AST.ImperativeAST.BinaryExpressionNode bnode = null)
        {
            FunctionCallNode fnode = node as FunctionCallNode;

            ProtoCore.DSASM.ProcedureNode procNode = TraverseFunctionCall(node, null, ProtoCore.DSASM.Constants.kInvalidIndex, 0, ref inferedType, graphNode, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, bnode);
            if (fnode != null && fnode.ArrayDimensions != null)
            {
                int dimensions = DfsEmitArrayIndexHeap(fnode.ArrayDimensions);
                EmitInstrConsole(ProtoCore.DSASM.kw.pushindex, dimensions.ToString() + "[dim]");
                EmitPushArrayIndex(dimensions);
                fnode.ArrayDimensions = null;
            }

            if(bnode == null)
                EmitSetExpressionUID(core.ExpressionUID++);
        }

        private void EmitIfStmtNode(ImperativeNode node, ref ProtoCore.Type inferedType, ProtoCore.AST.ImperativeAST.BinaryExpressionNode parentNode = null, bool isForInlineCondition = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null)
        {
            if (core.IsParsingCodeBlockNode || core.IsParsingPreloadedAssembly)
            {
                return;
            }

            if (IsParsingGlobal() || IsParsingGlobalFunctionBody())
            {
                /*
                                def backpatch(bp, pc)
                                    instr = instrstream[bp]
                                    if instr.opcode is jmp
                                        instr.op1 = pc
                                    elseif instr.opcode is cjmp
                                        instr.op2 = pc
                                    end
                                end

                                def backpatch(table, pc)
                                    foreach node in table
                                        backpatch(node.pc, pc)
                                    end
                                end

                */
                /*
                 if(E)		->	traverse E	
                                bpTable = new instance
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


                int bp = (int)ProtoCore.DSASM.Constants.kInvalidIndex;
                int L1 = (int)ProtoCore.DSASM.Constants.kInvalidIndex;

                // If-expr
                IfStmtNode ifnode = node as IfStmtNode;
                DfsTraverse(ifnode.IfExprNode, ref inferedType, false, graphNode, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, parentNode);

                L1 = ProtoCore.DSASM.Constants.kInvalidIndex;
                bp = pc;
                EmitCJmp(L1, ifnode.IfExprNode.line, ifnode.IfExprNode.col, ifnode.IfExprNode.endLine, ifnode.IfExprNode.endCol);

                if (!isForInlineCondition)
                {
                    EmitSetExpressionUID(core.ExpressionUID++);
                }

                // Create a new codeblock for this block
                // Set the current codeblock as the parent of the new codeblock
                // Set the new codeblock as a new child of the current codeblock
                // Set the new codeblock as the current codeblock
                ProtoCore.DSASM.CodeBlock localCodeBlock = null;

            
                localCodeBlock = new ProtoCore.DSASM.CodeBlock(
                    context.guid,
                    ProtoCore.DSASM.CodeBlockType.kConstruct,
                    Language.kInvalid,
                    core.CodeBlockIndex,
                    new ProtoCore.DSASM.SymbolTable(GetConstructBlockName("if"), core.RuntimeTableIndex++),
                    null,
                    false,
                    core);

                core.CodeBlockIndex++;

                localCodeBlock.instrStream = codeBlock.instrStream;
                localCodeBlock.parent = codeBlock;
                codeBlock.children.Add(localCodeBlock);

                codeBlock = localCodeBlock;
                EmitPushBlockID(localCodeBlock.codeBlockId);

                // If-body
                foreach (ImperativeNode ifBody in ifnode.IfBody)
                {
                    inferedType = new ProtoCore.Type();
                    inferedType.UID = (int)PrimitiveType.kTypeVar;
                    DfsTraverse(ifBody, ref inferedType, false, graphNode, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, parentNode);
                }

                if (!isForInlineCondition)
                {
                    ProtoCore.AST.Node oldBlockNode = localCodeBlockNode;
                    localCodeBlockNode = ifnode.IfBodyPosition;
                    EmitInstrConsole(ProtoCore.DSASM.kw.retcn);
                    EmitRetcn(localCodeBlock.codeBlockId);
                    localCodeBlockNode = oldBlockNode;
                }

                // Restore - Set the local codeblock parent to be the current codeblock
                codeBlock = localCodeBlock.parent;


                L1 = ProtoCore.DSASM.Constants.kInvalidIndex;

                BackpatchTable backpatchTable = new BackpatchTable();
                if (ifnode.ElseIfList.Count > 0 || ifnode.ElseBody.Count > 0)
                {
                    backpatchTable.Append(pc, L1);
                    EmitJmp(L1);
                }

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
                foreach (ElseIfBlock elseifNode in ifnode.ElseIfList)
                {
                    DfsTraverse(elseifNode.Expr, ref inferedType, false, graphNode);

                    L1 = ProtoCore.DSASM.Constants.kInvalidIndex;
                    bp = pc;
                    EmitCJmp(L1, elseifNode.Expr.line, elseifNode.Expr.col, elseifNode.Expr.endLine, elseifNode.Expr.endCol);

                    EmitSetExpressionUID(core.ExpressionUID++);

                    // Elseif-body   
                    if (null != elseifNode.Body)
                    {
                        // Create a new codeblock for this block
                        // Set the current codeblock as the parent of the new codeblock
                        // Set the new codeblock as a new child of the current codeblock
                        // Set the new codeblock as the current codeblock
                        localCodeBlock = new ProtoCore.DSASM.CodeBlock(
                            context.guid,
                            ProtoCore.DSASM.CodeBlockType.kConstruct,
                            Language.kInvalid,
                            core.CodeBlockIndex++,
                            new ProtoCore.DSASM.SymbolTable(GetConstructBlockName("elseif"), core.RuntimeTableIndex++),
                            null,
                            false,
                            core);

                        core.CodeBlockIndex++;

                        localCodeBlock.instrStream = codeBlock.instrStream;
                        localCodeBlock.parent = codeBlock;
                        codeBlock.children.Add(localCodeBlock);
                        codeBlock = localCodeBlock;
                        EmitPushBlockID(localCodeBlock.codeBlockId);
                        foreach (ImperativeNode elseifBody in elseifNode.Body)
                        {
                            inferedType = new ProtoCore.Type();
                            inferedType.UID = (int)PrimitiveType.kTypeVar;
                            DfsTraverse(elseifBody, ref inferedType, false, graphNode);
                        }

                        if (!isForInlineCondition)
                        {
                            ProtoCore.AST.Node oldBlockNode = localCodeBlockNode;
                            localCodeBlockNode = elseifNode.ElseIfBodyPosition;
                            EmitInstrConsole(ProtoCore.DSASM.kw.retcn);
                            EmitRetcn(localCodeBlock.codeBlockId);
                            localCodeBlockNode = oldBlockNode;
                        }

                        // Restore - Set the local codeblock parent to be the current codeblock
                        codeBlock = localCodeBlock.parent;
                    }

                    L1 = ProtoCore.DSASM.Constants.kInvalidIndex;
                    backpatchTable.Append(pc, L1);
                    EmitJmp(L1);

                    // Backpatch the L2 destination of the elseif block
                    Backpatch(bp, pc);
                }

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

                    core.CodeBlockIndex++;

                    localCodeBlock.instrStream = codeBlock.instrStream;
                    localCodeBlock.parent = codeBlock;
                    codeBlock.children.Add(localCodeBlock);
                    codeBlock = localCodeBlock;
                    EmitPushBlockID(localCodeBlock.codeBlockId);
                    foreach (ImperativeNode elseBody in ifnode.ElseBody)
                    {
                        inferedType = new ProtoCore.Type();
                        inferedType.UID = (int)PrimitiveType.kTypeVar;
                        DfsTraverse(elseBody, ref inferedType, false, graphNode, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, parentNode);
                    }

                    if (!isForInlineCondition)
                    {
                        ProtoCore.AST.Node oldBlockNode = localCodeBlockNode;
                        localCodeBlockNode = ifnode.ElseBodyPosition;
                        EmitInstrConsole(ProtoCore.DSASM.kw.retcn);
                        EmitRetcn(localCodeBlock.codeBlockId);
                        localCodeBlockNode = oldBlockNode;
                    }

                    // Restore - Set the local codeblock parent to be the current codeblock
                    codeBlock = localCodeBlock.parent;
                }

                /*
                 * 
                          ->	backpatch(bpTable, pc) 
                 */
                // ifstmt-exit
                // Backpatch all the previous unconditional jumps
                Backpatch(backpatchTable.backpatchList, pc);
            }
        }

        private void EmitWhileStmtNode(ImperativeNode node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null)
        {
            if (core.IsParsingCodeBlockNode || core.IsParsingPreloadedAssembly)
            {
                return;
            }

            if (IsParsingGlobal() || IsParsingGlobalFunctionBody())
            {
                /*
                   
                while(E)	->	entry = pc
                                traverse E	
                                emit(pop,cx)
                                L1 = pc + 1
                                L2 = null 
                                bp = pc
                                emit(jmp, _cx, L1, L2) 

                 * */

                int bp = (int)ProtoCore.DSASM.Constants.kInvalidIndex;
                int L1 = (int)ProtoCore.DSASM.Constants.kInvalidIndex;
                int entry = (int)ProtoCore.DSASM.Constants.kInvalidIndex;

                entry = pc;

                WhileStmtNode whileNode = node as WhileStmtNode;
                DfsTraverse(whileNode.Expr, ref inferedType);

                L1 = ProtoCore.DSASM.Constants.kInvalidIndex;
                bp = pc;
                EmitCJmp(L1, whileNode.Expr.line, whileNode.Expr.col, whileNode.Expr.endLine, whileNode.Expr.endCol);

                EmitSetExpressionUID(core.ExpressionUID++);

                /*
                {
                    S		->	traverse S	
                                bptable.append(pc)
                                emit(jmp, entry) 
                }	
                            ->  backpatch(bp, pc)
                */
                if (null != whileNode.Body)
                {
                    // Create a new symboltable for this block
                    // Set the current table as the parent of the new table
                    // Set the new table as a new child of the current table
                    // Set the new table as the current table
                    // Create a new codeblock for this block
                    // Set the current codeblock as the parent of the new codeblock
                    // Set the new codeblock as a new child of the current codeblock
                    // Set the new codeblock as the current codeblock
                    ProtoCore.DSASM.CodeBlock localCodeBlock = new ProtoCore.DSASM.CodeBlock(
                        context.guid,
                        ProtoCore.DSASM.CodeBlockType.kConstruct,
                        Language.kInvalid,
                        core.CodeBlockIndex++,
                        new ProtoCore.DSASM.SymbolTable(GetConstructBlockName("while"), core.RuntimeTableIndex++),
                        null,
                        true,
                        core);

                    core.CodeBlockIndex++;

                    localCodeBlock.instrStream = codeBlock.instrStream;
                    localCodeBlock.parent = codeBlock;
                    codeBlock.children.Add(localCodeBlock);
                    codeBlock = localCodeBlock;
                    backpatchMap.EntryTable[localCodeBlock.codeBlockId] = entry;
                    backpatchMap.BreakTable[localCodeBlock.codeBlockId] = new BackpatchTable();
                    
                    EmitPushBlockID(localCodeBlock.codeBlockId);
                    EmitCodeBlock(whileNode.Body, ref inferedType, isBooleanOp, graphNode);

                    ProtoCore.AST.Node oldBlockNode = localCodeBlockNode;
                    localCodeBlockNode = node;
                    EmitInstrConsole(ProtoCore.DSASM.kw.retcn);
                    EmitRetcn(localCodeBlock.codeBlockId);
                    localCodeBlockNode = oldBlockNode;


                    // Restore - Set the local codeblock parent to be the current codeblock
                    codeBlock = localCodeBlock.parent;

                    EmitJmp(entry);
                    Backpatch(backpatchMap.BreakTable[localCodeBlock.codeBlockId].backpatchList, pc);
                }
                Backpatch(bp, pc);
            }
        }

        private void EmitVarDeclNode(ImperativeNode node, ref ProtoCore.Type inferedType, ProtoCore.AssociativeGraph.GraphNode graphNode = null)
        {
            VarDeclNode varNode = node as VarDeclNode;

            ProtoCore.Type type = BuildArgumentTypeFromVarDeclNode(varNode, graphNode);
            type.rank = 0;

            // TODO Jun: Create a class table for holding the primitive and custom data types
            const int primitivesize = 1;
            int datasize = primitivesize;

            int symindex = ProtoCore.DSASM.Constants.kInvalidIndex;
            IdentifierNode tVar = null;
            if (varNode.NameNode is IdentifierNode)
            {
                // Allocate with no initializer
                tVar = varNode.NameNode as IdentifierNode;
                ProtoCore.DSASM.SymbolNode symnode = Allocate(tVar.Value, globalProcIndex, type, datasize, datasize, tVar.ArrayDimensions, varNode.memregion);
                symindex = symnode.symbolTableIndex;
            }
            else if (varNode.NameNode is BinaryExpressionNode)
            {
                BinaryExpressionNode bNode = varNode.NameNode as BinaryExpressionNode;
                tVar = bNode.LeftNode as IdentifierNode;

                Validity.Assert(null != tVar, "Check generated AST");
                Validity.Assert(null != bNode.RightNode, "Check generated AST");

                ProtoCore.DSASM.SymbolNode symnode = null;

                // Is it an array
                if (null != tVar.ArrayDimensions)
                {
                    // Allocate an array with initializer
                    if (bNode.RightNode is ExprListNode)
                    {
                        ExprListNode exprlist = bNode.RightNode as ExprListNode;
                        int size = datasize * exprlist.list.Count;

                        symnode = Allocate(tVar.Value, globalProcIndex, type, size, datasize, tVar.ArrayDimensions, varNode.memregion);
                        symindex = symnode.symbolTableIndex;

                        for (int n = 0; n < exprlist.list.Count; ++n)
                        {
                            DfsTraverse(exprlist.list[n], ref inferedType);

                            ArrayNode array = new ArrayNode();
                            array.Expr = nodeBuilder.BuildIdentfier(n.ToString(), PrimitiveType.kTypeInt);
                            array.Type = null;

                            DfsEmitArrayIndex(array, symindex);

                            EmitInstrConsole(ProtoCore.DSASM.kw.pop, ProtoCore.DSASM.kw.regDX);
                            StackValue opRes = StackValue.BuildRegister(Registers.DX);
                            EmitPop(opRes, Constants.kGlobalScope);

                            EmitInstrConsole(ProtoCore.DSASM.kw.pop, tVar.Value);
                            EmitPopForSymbol(symnode, symnode.runtimeTableIndex);
                        }
                    }
                    else
                    {
                        buildStatus.LogSemanticError(Resources.InvalidArrayInitializer, core.CurrentDSFileName, bNode.RightNode.line, bNode.RightNode.col);
                    }
                }
                else
                {
                    // Allocate a single variable with initializer

                    symnode = Allocate(tVar.Value, globalProcIndex, type, datasize, datasize, tVar.ArrayDimensions, varNode.memregion);
                    symindex = symnode.symbolTableIndex;
                    DfsTraverse(bNode.RightNode, ref inferedType);

                    EmitInstrConsole(ProtoCore.DSASM.kw.pop, tVar.Value);
                    EmitPopForSymbol(symnode, symnode.runtimeTableIndex);
                }
            }
            else
            {
                Validity.Assert(false, "Check generated AST");
            }

            if (ProtoCore.DSASM.Constants.kInvalidIndex == symindex)
            {
                throw new BuildHaltException("0CB5BD17");
            }
        }

        private void EmitBinaryExpressionNode(ImperativeNode node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null,
            ProtoCore.AST.ImperativeAST.BinaryExpressionNode parentNode = null)
        {
            if (!IsParsingGlobal() && !IsParsingGlobalFunctionBody())
                return;

            bool isBooleanOperation = false;
            BinaryExpressionNode b = node as BinaryExpressionNode;

            ProtoCore.Type leftType = new ProtoCore.Type();
            leftType.UID = (int)ProtoCore.PrimitiveType.kTypeVar;

            ProtoCore.Type rightType = new ProtoCore.Type();
            rightType.UID = (int)ProtoCore.PrimitiveType.kTypeVar;

            if (ProtoCore.DSASM.Operator.assign != b.Optr)
            {
                isBooleanOperation = ProtoCore.DSASM.Operator.lt == b.Optr
                    || ProtoCore.DSASM.Operator.gt == b.Optr
                    || ProtoCore.DSASM.Operator.le == b.Optr
                    || ProtoCore.DSASM.Operator.ge == b.Optr
                    || ProtoCore.DSASM.Operator.eq == b.Optr
                    || ProtoCore.DSASM.Operator.nq == b.Optr
                    || ProtoCore.DSASM.Operator.and == b.Optr
                    || ProtoCore.DSASM.Operator.or == b.Optr;

                DfsTraverse(b.LeftNode, ref inferedType, isBooleanOperation, graphNode, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, parentNode);

                if (inferedType.UID == (int)PrimitiveType.kTypeFunctionPointer && emitDebugInfo)
                {
                    buildStatus.LogSemanticError(Resources.FunctionPointerNotAllowedAtBinaryExpression, core.CurrentDSFileName, b.LeftNode.line, b.LeftNode.col);
                }

                leftType.UID = inferedType.UID;
                leftType.rank = inferedType.rank;
            }
            else
            {
                if (b.LeftNode is IdentifierListNode)
                {
                    ProtoCore.AST.Node lnode = b.LeftNode;
                    bool isCollapsed;

                    if (parentNode != null)
                    {
                        NodeUtils.SetNodeLocation(lnode, parentNode, parentNode);
                    }
                    else
                    {
                        NodeUtils.SetNodeLocation(lnode, b, b);
                    }
                    EmitGetterSetterForIdentList(lnode, ref inferedType, null, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, out isCollapsed, b.RightNode);


                    // Get the lhs symbol list
                    ProtoCore.Type type = new ProtoCore.Type();
                    type.UID = globalClassIndex;
                    ProtoCore.AssociativeGraph.UpdateNodeRef leftNodeRef = new ProtoCore.AssociativeGraph.UpdateNodeRef();
                    DFSGetSymbolList(lnode, ref type, leftNodeRef);



                    // Get the first identifier symbol runtime index as it is required for the pushdep
                    List<ProtoCore.DSASM.SymbolNode> symbolList = new List<ProtoCore.DSASM.SymbolNode>();
                    symbolList.Add(leftNodeRef.nodeList[0].symbol);
                    int runtimeIndex = leftNodeRef.nodeList[0].symbol.runtimeTableIndex;

                    // Append the rest of the symbols in the identifierlist
                    for (int n = 1; n < leftNodeRef.nodeList.Count; ++n)
                    {
                        if (leftNodeRef.nodeList[n].symbol != null)
                            symbolList.Add(leftNodeRef.nodeList[n].symbol);
                    }

                    EmitPushDepData(symbolList);
                    EmitPushDep(runtimeIndex, symbolList.Count, globalClassIndex);

                    return;
                }
            }

            // (Ayush) in case of PostFixNode, only traverse the identifier now. Post fix operation will be applied later.
#if ENABLE_INC_DEC_FIX
                if (b.RightNode is PostFixNode)
                    DfsTraverse((b.RightNode as PostFixNode).Identifier, ref inferedType, isBooleanOperation);
                else
                {
#endif
            if ((ProtoCore.DSASM.Operator.assign == b.Optr) && (b.RightNode is LanguageBlockNode))
            {
                inferedType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0);
            }

            if (b.RightNode == null && b.Optr == Operator.assign && b.LeftNode is IdentifierNode)
            {
                IdentifierNode t = b.LeftNode as IdentifierNode;
                ProtoCore.DSASM.SymbolNode symbolnode = null;
                bool isAccessible = false;
                bool hasAllocated = VerifyAllocation(t.Value, globalClassIndex, globalProcIndex, out symbolnode, out isAccessible);
                if (hasAllocated)
                {
                    b.RightNode = nodeBuilder.BuildIdentfier(t.Value);
                }
                else
                {
                    b.RightNode = new NullNode();
                }
            }

            if (parentNode != null)
            {
                DfsTraverse(b.RightNode, ref inferedType, isBooleanOperation, graphNode, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, parentNode);
            }
            else
            {
                DfsTraverse(b.RightNode, ref inferedType, isBooleanOperation, graphNode, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, b);
            }

#if ENABLE_INC_DEC_FIX
                }
#endif

            rightType.UID = inferedType.UID;
            rightType.rank = inferedType.rank;

            BinaryExpressionNode rightNode = b.RightNode as BinaryExpressionNode;
            if ((rightNode != null) && (ProtoCore.DSASM.Operator.assign == rightNode.Optr))
                DfsTraverse(rightNode.LeftNode, ref inferedType);

            if (b.Optr != ProtoCore.DSASM.Operator.assign)
            {
                if (inferedType.UID == (int)PrimitiveType.kTypeFunctionPointer && emitDebugInfo)
                {
                    buildStatus.LogSemanticError(Resources.FunctionPointerNotAllowedAtBinaryExpression, core.CurrentDSFileName, b.RightNode.line, b.RightNode.col);
                }
                EmitBinaryOperation(leftType, rightType, b.Optr);
                isBooleanOp = false;

                //if post fix, now traverse the post fix
#if ENABLE_INC_DEC_FIX
                if (b.RightNode is PostFixNode)
                    EmitPostFixNode(b.RightNode, ref inferedType);
#endif
                return;
            }

            if (b.LeftNode is IdentifierNode)
            {
                IdentifierNode t = b.LeftNode as IdentifierNode;
                ProtoCore.DSASM.SymbolNode symbolnode = null;

                string s = t.Value;
                bool isReturn = (s == ProtoCore.DSDefinitions.Keyword.Return);
                if (isReturn)
                {
                    EmitReturnStatement(node, inferedType);
                }
                else
                {
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
                            procNode = CoreUtils.GetFirstVisibleProcedure(t.Name, null, codeBlock);
                        }
                        if (procNode != null)
                        {
                            if (ProtoCore.DSASM.Constants.kInvalidIndex != procNode.procId && emitDebugInfo)
                            {
                                buildStatus.LogSemanticError(String.Format(Resources.FunctionAsVaribleError,t.Name), core.CurrentDSFileName, t.line, t.col);
                            }
                        }
                    }

                    bool isAccessible = false;
                    bool isAllocated = false;
                    bool isLocalDeclaration = t.IsLocal;
                    // if it's forloop, verify allocation with its original arrayname
                    if (t.ArrayName != null && !t.ArrayName.Equals(""))
                    {
                        isAllocated = VerifyAllocation(t.Value, t.ArrayName, globalClassIndex, globalProcIndex, out symbolnode, out isAccessible);
                    }
                    else
                    {
                        if (isLocalDeclaration)
                        {
                            isAllocated = VerifyAllocationInScope(t.Value, globalClassIndex, globalProcIndex, out symbolnode, out isAccessible);
                        }
                        else
                        {
                            isAllocated = VerifyAllocation(t.Value, globalClassIndex, globalProcIndex, out symbolnode, out isAccessible);
                        }
                    }
                   
                    int runtimeIndex = (!isAllocated) ? codeBlock.symbolTable.RuntimeIndex : symbolnode.runtimeTableIndex;

                    // Comment Jun: Add modifeid properties into the updatedProperties list of the current function
                    // This propagates upated of mproperties taht were modified in an imperative block
                    if (null != localProcedure && ProtoCore.DSASM.Constants.kGlobalScope != localProcedure.classScope)
                    {
                        if (isAllocated)
                        {
                            Validity.Assert(null != symbolnode);

                            // Get the lhs symbol list
                            ProtoCore.Type type = new ProtoCore.Type();
                            type.UID = globalClassIndex;
                            ProtoCore.AssociativeGraph.UpdateNodeRef leftNodeRef = new ProtoCore.AssociativeGraph.UpdateNodeRef();
                            DFSGetSymbolList(b.LeftNode, ref type, leftNodeRef);

                            localProcedure.updatedProperties.Push(leftNodeRef);
                        }
                    }

                    // TODO Jun: Update mechanism work in progress - a flag to manually enable update 
                    bool enableUpdate = false;
                    if (enableUpdate)
                    {
                        bool isExternal = false; // isAllocated && currentLangBlock != codeBlockId;
                        //bool isAssociative = ProtoCore.Language.kAssociative == core.exeList[currentLangBlock].language; 
                        bool isAssociative = false;
                        if (isExternal && isAssociative)
                        {
                            // Check if this is a modifier variable
                            bool isVariableAModifierStack = false;
                            if (isVariableAModifierStack)
                            {
                                // Check if modifying a named modifier state
                                bool isNameModifierState = false;
                                if (isNameModifierState)
                                {
                                    //bool isStateIntermediate = false;

                                }
                                else
                                {

                                }
                                //targetLangBlock = blockId;
                            }
                        }
                    }

                    int dimensions = 0;
                    if (null != t.ArrayDimensions)
                    {
                        dimensions = DfsEmitArrayIndexHeap(t.ArrayDimensions);
                    }

                    ProtoCore.Type castType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0);
                    var tident = b.LeftNode as TypedIdentifierNode;
                    if (tident != null)
                    {
                        int castUID = tident.datatype.UID;
                        if ((int)PrimitiveType.kInvalidType == castUID)
                        {
                            castUID = core.ClassTable.IndexOf(tident.datatype.Name);
                        }

                        if ((int)PrimitiveType.kInvalidType == castUID)
                        {
                            string message = String.Format(ProtoCore.Properties.Resources.kTypeUndefined, tident.datatype.Name);
                            buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kTypeUndefined, message, core.CurrentDSFileName, b.line, b.col, graphNode);
                            castType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kInvalidType, 0);
                            castType.Name = tident.datatype.Name;
                            castType.rank = tident.datatype.rank;
                        }
                        else
                        {
                            castType = core.TypeSystem.BuildTypeObject(castUID, tident.datatype.rank);
                        }
                    }

                    if (globalClassIndex != ProtoCore.DSASM.Constants.kInvalidIndex)
                    {
                        int symbol = ProtoCore.DSASM.Constants.kInvalidIndex;

                        for (int n = 0; n < core.ClassTable.ClassNodes[globalClassIndex].symbols.symbolList.Count; ++n)
                        {
                            //Fuqiang: Not a member variable if it is a local variable inside a function with the same name
                            bool localVarInMemFunc = false;
                            if (localProcedure != null)
                            {
                                if (symbolnode == null)
                                {
                                    if (!isAllocated) // if isAllocated, inaccessible member variable 
                                    {
                                        localVarInMemFunc = true;
                                    }
                                }
                                else if (symbolnode.functionIndex != ProtoCore.DSASM.Constants.kGlobalScope && !localProcedure.isConstructor)
                                {
                                    localVarInMemFunc = true;
                                }
                            }
                            bool isMemberVar = ProtoCore.DSASM.Constants.kGlobalScope == core.ClassTable.ClassNodes[globalClassIndex].symbols.symbolList[n].functionIndex
                                && core.ClassTable.ClassNodes[globalClassIndex].symbols.symbolList[n].name == t.Name
                                && !localVarInMemFunc;
                            if (isMemberVar)
                            {
                                if (t.ArrayDimensions == null)
                                    core.ClassTable.ClassNodes[globalClassIndex].symbols.symbolList[n].datatype = inferedType;
                                else if (dimensions == inferedType.rank)
                                    core.ClassTable.ClassNodes[globalClassIndex].symbols.symbolList[n].datatype.UID = inferedType.UID;
                                symbol = symbolnode.symbolTableIndex;
                                break;
                            }
                        }

                        if (symbol == ProtoCore.DSASM.Constants.kInvalidIndex)
                        {
                            if (!isAllocated)
                            {
                                symbolnode = Allocate(t.Name, globalProcIndex, inferedType);
                            }

                            symbol = symbolnode.symbolTableIndex;

                            if (b.LeftNode is TypedIdentifierNode)
                            {
                                symbolnode.SetStaticType(castType);
                            }
                            castType = symbolnode.staticType;
                            EmitPushVarData(dimensions, castType.UID, castType.rank);

                            EmitInstrConsole(ProtoCore.DSASM.kw.pop, s);
                            StackValue operand = StackValue.BuildVarIndex(symbol);
                            EmitPop(operand, symbolnode.classScope, runtimeIndex, node.line, node.col, node.endLine, node.endCol);
                        }
                        else
                        {
                            if (b.LeftNode is TypedIdentifierNode)
                            {
                                symbolnode.SetStaticType(castType);
                            }
                            castType = symbolnode.staticType;
                            EmitPushVarData(dimensions, castType.UID, castType.rank);

                            EmitInstrConsole(ProtoCore.DSASM.kw.popm, t.Name);

                            StackValue operand = symbolnode.isStatic
                                                 ? StackValue.BuildStaticMemVarIndex(symbol)
                                                 : StackValue.BuildMemVarIndex(symbol);

                            EmitPopm(operand, runtimeIndex, node.line, node.col, node.endLine, node.endCol);
                        }
                    }
                    else
                    {
                        if (!isAllocated)
                        {
                            symbolnode = Allocate(t.Value, globalProcIndex, inferedType);
                            if (dimensions > 0)
                            {
                                symbolnode.datatype.rank = dimensions;
                            }
                        }
                        else if (dimensions == 0)
                        {
                            if (core.TypeSystem.IsHigherRank(inferedType.UID, symbolnode.datatype.UID))
                            {
                                symbolnode.datatype = inferedType;
                            }
                        }

                        if (b.LeftNode is TypedIdentifierNode)
                        {
                            symbolnode.SetStaticType(castType);
                        }
                        castType = symbolnode.staticType;
                        EmitPushVarData(dimensions, castType.UID, castType.rank);
                        EmitInstrConsole(ProtoCore.DSASM.kw.pop, t.Value);
                        if (parentNode != null)
                        {
                            EmitPopForSymbol(symbolnode, runtimeIndex, parentNode.line, parentNode.col, parentNode.endLine, parentNode.endCol);
                        }
                        else
                        {
                            EmitPopForSymbol(symbolnode, runtimeIndex, node.line, node.col, node.endLine, node.endCol);
                        }
                        

                        // Check if the symbol was not here, only then it becomes a valid propagation symbol 
                        // TODO Jun: check if the symbol was allocated from an associative block
                        if (!ProtoCore.Utils.CoreUtils.IsAutoGeneratedVar(symbolnode.name))
                        {
                            if (codeBlock.symbolTable.RuntimeIndex != symbolnode.runtimeTableIndex)
                            {
                                List<ProtoCore.DSASM.SymbolNode> symbolList = new List<ProtoCore.DSASM.SymbolNode>();
                                symbolList.Add(symbolnode);

                                EmitPushDepData(symbolList);
                                EmitPushDep(runtimeIndex, symbolList.Count, globalClassIndex);
                            }
                        }
                    }
                }
            }
            else if (b.LeftNode is IdentifierListNode)
            {
                // keyu: the left hand side of an assignment statement won't be an
                // identifier list anymore after replacing all properties (not 
                // including the left-most property) with getters/setter.
                // 
                // If this case really happens, we need to look into that.
                Validity.Assert(false, "The left hand of an assignment statement never will be an identifier list node");

                /*
                int depth = 0;

                ProtoCore.Type lastType = new ProtoCore.Type();
                lastType.UID = (int)PrimitiveType.kInvalidType;
                lastType.IsIndexable = false;

                bool isFirstIdent = false;
                bool isIdentReference = DfsEmitIdentList(b.LeftNode, b, globalClassIndex, ref lastType, ref depth, ref inferedType, true, ref isFirstIdent);
                inferedType.UID = isBooleanOp ? (int)PrimitiveType.kTypeBool : inferedType.UID;

                if (!isIdentReference)
                {
                    buildStatus.LogSemanticError("The left hand side of an operation cannot be a function call", core.CurrentDSFileName, b.LeftNode.line, b.LeftNode.col);
                    throw new BuildHaltException();
                }

                EmitInstrConsole(ProtoCore.DSASM.kw.poplist, depth.ToString(), globalClassIndex.ToString());

                // TODO Jun: Get blockid
                int blockId = 0;
                EmitPopList(depth, globalClassIndex, blockId, node.line, node.col, node.endLine, node.endCol);
                */
            }
            else
            {
                string message = "Illegal assignment (38A37EA5)";
                buildStatus.LogSemanticError(message, core.CurrentDSFileName, b.line, b.col);
                throw new BuildHaltException(message);
            }

            if ((node as BinaryExpressionNode).Optr == Operator.assign)
                EmitSetExpressionUID(core.ExpressionUID++);

            //if post fix, now traverse the post fix
#if ENABLE_INC_DEC_FIX
                if (b.RightNode is PostFixNode)
                    EmitPostFixNode(b.RightNode, ref inferedType);
#endif
        }

        private void EmitUnaryExpressionNode(ImperativeNode node, ref ProtoCore.Type inferedType, ProtoCore.AST.ImperativeAST.BinaryExpressionNode parentNode)
        {
            if (IsParsingGlobal() || IsParsingGlobalFunctionBody())
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
                        EmitBinaryExpressionNode(bin, ref inferedType);
                    }
                    else
                        throw new BuildHaltException("Invalid use of prefix operation (15BB9C10).");
                }

                DfsTraverse(u.Expression, ref inferedType, false, null, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, parentNode);

                if (!isPrefixOperation)
                {
                    string op = Op.GetUnaryOpName(u.Operator);
                    EmitInstrConsole(op);
                    EmitUnary(Op.GetUnaryOpCode(u.Operator));
                }
            }
        }

        private void EmitForLoopNode(ImperativeNode node, ref ProtoCore.Type inferredType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null)
        {
            if (core.IsParsingCodeBlockNode || core.IsParsingPreloadedAssembly)
            {
                return;
            }

            if (IsParsingGlobal() || IsParsingGlobalFunctionBody())
            {
                /*
                x = 0;
                a = {10,20,30,40}
                for(val in a)
                {
                    x = x + val;
                }

                Compiles down to:

                x = 0;
                a = {10,20,30,40};
                val = null;
                %forloop_key = a.key;
                %forloop_expr = a;

                while( %forloop_key != null)
                {
                    val = %forloop_expr[%forloop_key];
                    %forloop_key = %forloop_key + 1;
                    x = x + val;
                }
                */
                DebugProperties.BreakpointOptions oldOptions = core.DebuggerProperties.breakOptions;
                DebugProperties.BreakpointOptions newOptions = oldOptions;
                newOptions |= DebugProperties.BreakpointOptions.EmitCallrForTempBreakpoint;
                core.DebuggerProperties.breakOptions = newOptions;

                // TODO Jun: This compilation unit has many opportunities for optimization 
                //      1. Compiling to while need not be necessary if 'expr' has exactly one element
                //      2. For-loop can have its own semantics without the need to convert to a while node

                ForLoopNode forNode = node as ForLoopNode;
                ++core.ForLoopBlockIndex;   //new forloop beginning. increment loop counter 

                ProtoCore.Type type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVoid, 0);

                // val = null; 
                IdentifierNode loopvar = nodeBuilder.BuildIdentfier(forNode.loopVar.Name) as IdentifierNode;
                {
                    loopvar.ArrayName = forNode.expression.Name;
                    ProtoCore.Utils.NodeUtils.CopyNodeLocation(loopvar, forNode.loopVar);
                    BinaryExpressionNode loopvarInit = new BinaryExpressionNode();
                    loopvarInit.Optr = ProtoCore.DSASM.Operator.assign;
                    loopvarInit.LeftNode = loopvar;
                    loopvarInit.RightNode = new NullNode();

                    ProtoCore.Utils.NodeUtils.CopyNodeLocation(loopvarInit, forNode);
                    loopvarInit.endLine = loopvarInit.line;
                    loopvarInit.endCol = loopvarInit.col + 3;
                    EmitBinaryExpressionNode(loopvarInit, ref type, isBooleanOp, graphNode);
                }

                // %key = null;
                string keyIdent = GetForLoopKeyIdent();
                Allocate(keyIdent, globalProcIndex, TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVoid, 0));
                var key = nodeBuilder.BuildIdentfier(keyIdent);

                // %array = complicated expr in for...in loop, so that we could
                // index into it. 
                string identName = GetForExprIdent();
                var arrayExpr = nodeBuilder.BuildIdentfier(identName);
                NodeUtils.CopyNodeLocation(arrayExpr, forNode.expression);
                BinaryExpressionNode arrayexprAssignment = new BinaryExpressionNode();
                arrayexprAssignment.Optr = ProtoCore.DSASM.Operator.assign;
                arrayexprAssignment.LeftNode = arrayExpr;
                arrayexprAssignment.RightNode = forNode.expression;
                NodeUtils.UpdateBinaryExpressionLocation(arrayexprAssignment);

                switch (forNode.expression.GetType().ToString())
                {
                    case "ProtoCore.AST.ImperativeAST.IdentifierNode":
                    case "ProtoCore.AST.ImperativeAST.ExprListNode":
                        newOptions |= DebugProperties.BreakpointOptions.EmitPopForTempBreakpoint;
                        core.DebuggerProperties.breakOptions = newOptions;
                        break;
                }

                type.UID = (int)ProtoCore.PrimitiveType.kTypeVoid;
                EmitBinaryExpressionNode(arrayexprAssignment, ref type, isBooleanOp, graphNode);
                core.DebuggerProperties.breakOptions = oldOptions; // Restore breakpoint behaviors.

                // Get the size of expr and assign it to the autogen iteration var
                int symbolIndex = Constants.kInvalidIndex;
                SymbolNode symbol = null;
                if (ProtoCore.DSASM.Constants.kInvalidIndex != globalClassIndex && !IsInLanguageBlockDefinedInFunction())
                {
                    symbolIndex = core.ClassTable.ClassNodes[globalClassIndex].symbols.IndexOf(identName);
                    if (symbolIndex != Constants.kInvalidIndex)
                    {
                        symbol = core.ClassTable.ClassNodes[globalClassIndex].symbols.symbolList[symbolIndex];
                    }
                }
                else
                {
                    symbolIndex = codeBlock.symbolTable.IndexOf(identName);
                    if (symbolIndex != Constants.kInvalidIndex)
                    {
                        symbol = codeBlock.symbolTable.symbolList[symbolIndex];
                    }
                }
                EmitInstrConsole(ProtoCore.DSASM.kw.pushvarsize, identName);
                EmitPushArrayKey(symbolIndex, codeBlock.symbolTable.RuntimeIndex, (symbol == null) ? globalClassIndex : symbol.classScope);

                // Push the identifier local block information 
                // Push the array dimensions
                int dimensions = 0;
                EmitPushVarData(dimensions);

                if (ProtoCore.DSASM.Constants.kInvalidIndex != globalClassIndex && !IsInLanguageBlockDefinedInFunction())
                {
                    symbolIndex = core.ClassTable.ClassNodes[globalClassIndex].symbols.IndexOf(keyIdent);
                    if (symbolIndex != Constants.kInvalidIndex)
                    {
                        symbol = core.ClassTable.ClassNodes[globalClassIndex].symbols.symbolList[symbolIndex];
                    }
                }
                else
                {
                    symbolIndex = codeBlock.symbolTable.IndexOf(keyIdent);
                    if (symbolIndex != Constants.kInvalidIndex)
                    {
                        symbol = codeBlock.symbolTable.symbolList[symbolIndex];
                    }
                }
                StackValue opDest = StackValue.BuildVarIndex(symbolIndex);
                EmitInstrConsole(ProtoCore.DSASM.kw.pop, keyIdent);
                EmitPop(opDest, (symbol == null) ? globalClassIndex : symbol.classScope, symbol.runtimeTableIndex);

                // key == null ?
                BinaryExpressionNode condition = new BinaryExpressionNode();
                {
                    condition.Optr = ProtoCore.DSASM.Operator.nq;
                    condition.LeftNode = key;
                    condition.RightNode = new NullNode();
                    condition.line = forNode.KwInLine;
                    condition.col = forNode.KwInCol;
                    condition.endLine = forNode.KwInLine;
                    condition.endCol = forNode.KwInCol + 2; // 2 character for keyword "in".
                }

                // val = array[key];
                BinaryExpressionNode arrayIndexing = new BinaryExpressionNode();
                {
                    arrayIndexing.Optr = ProtoCore.DSASM.Operator.assign;
                    arrayIndexing.LeftNode = loopvar;

                    // Array index into the expr ident
                    ArrayNode arrayIndex = new ArrayNode();
                    arrayIndex.Expr = key;
                    arrayIndex.Type = null;
                    (arrayExpr as IdentifierNode).ArrayDimensions = arrayIndex;
                    arrayIndexing.RightNode = arrayExpr;

                    arrayIndexing.line = loopvar.line;
                    arrayIndexing.col = loopvar.col;
                    arrayIndexing.endLine = loopvar.endLine;
                    arrayIndexing.endCol = loopvar.endCol;
                }

                // key = key + 1;
                BinaryExpressionNode nextKey = new BinaryExpressionNode();
                {
                    nextKey.LeftNode = key;
                    nextKey.Optr = Operator.assign;
                    nextKey.RightNode = nodeBuilder.BuildBinaryExpression(key,
                                                    new IntNode(1),
                                                    Operator.add);
                }

                // Append the array indexing and key increment expressions into 
                // the for-loop body
                forNode.body.Insert(0, arrayIndexing);
                forNode.body.Insert(1, nextKey);

                // Construct and populate the equivalent while node
                WhileStmtNode whileStatement = new WhileStmtNode();
                whileStatement.Expr = condition;
                whileStatement.Body = forNode.body;
                whileStatement.endLine = node.endLine;
                whileStatement.endCol = node.endCol;

                type.UID = (int)ProtoCore.PrimitiveType.kTypeVoid;
                EmitWhileStmtNode(whileStatement, ref type, isBooleanOp, graphNode);
                //}

                // Comment Jun: The for loop counter must be unique and does not need to reset
                //forloopCounter--;   //for loop ended. decrement counter 
            }
        }

        private void EmitInlineConditionalNode(ImperativeNode node, ref ProtoCore.Type inferedType, ProtoCore.AST.ImperativeAST.BinaryExpressionNode parentNode = null)
        {
            InlineConditionalNode inlineConNode = node as InlineConditionalNode;
            IfStmtNode ifNode = new IfStmtNode();
            ifNode.IfExprNode = inlineConNode.ConditionExpression;
            List<ImperativeNode> trueBody = new List<ImperativeNode>();
            trueBody.Add(inlineConNode.TrueExpression);
            List<ImperativeNode> falseBody = new List<ImperativeNode>();
            falseBody.Add(inlineConNode.FalseExpression);
            ifNode.IfBody = trueBody;
            ifNode.ElseBody = falseBody;

            DebugProperties.BreakpointOptions oldOptions = core.DebuggerProperties.breakOptions;
            DebugProperties.BreakpointOptions newOptions = oldOptions;
            newOptions |= DebugProperties.BreakpointOptions.EmitInlineConditionalBreakpoint;
            core.DebuggerProperties.breakOptions = newOptions;

            EmitIfStmtNode(ifNode, ref inferedType, parentNode, true);

            core.DebuggerProperties.breakOptions = oldOptions;
        }

        private void EmitRangeExprNode(ImperativeNode node, ref ProtoCore.Type inferedType, ProtoCore.AssociativeGraph.GraphNode graphNode = null)
        {
            RangeExprNode range = node as RangeExprNode;

            // Do some static checking...probably it is not necessary. 
            // Need to move these checkings to built-in function.
            if ((range.FromNode is IntNode || range.FromNode is DoubleNode) &&
                (range.ToNode is IntNode || range.ToNode is DoubleNode) &&
                (range.StepNode == null || (range.StepNode != null && (range.StepNode is IntNode || range.StepNode is DoubleNode))))
            {
                double current = (range.FromNode is IntNode) ? (range.FromNode as IntNode).Value : (range.FromNode as DoubleNode).Value;
                double end = (range.ToNode is IntNode) ? (range.ToNode as IntNode).Value : (range.ToNode as DoubleNode).Value;
                ProtoCore.DSASM.RangeStepOperator stepoperator = range.stepoperator;

                double step = 1;
                if (range.StepNode != null)
                {
                    step = (range.StepNode is IntNode) ? (range.StepNode as IntNode).Value : (range.StepNode as DoubleNode).Value;
                }

                bool hasAmountOp = range.HasRangeAmountOperator;
                string warningMsg = String.Empty;

                if (stepoperator == ProtoCore.DSASM.RangeStepOperator.stepsize)
                {
                    if (!hasAmountOp)
                    {
                        if (range.StepNode == null && end < current)
                        {
                            step = -1;
                        }

                        if (step == 0)
                        {
                            warningMsg = ProtoCore.Properties.Resources.kRangeExpressionWithStepSizeZero;
                        }
                        else if ((end > current && step < 0) || (end < current && step > 0))
                        {
                            warningMsg = ProtoCore.Properties.Resources.kRangeExpressionWithInvalidStepSize;
                        }
                    }
                }
                else if (stepoperator == ProtoCore.DSASM.RangeStepOperator.num)
                {
                    if (hasAmountOp)
                    {
                        warningMsg = ProtoCore.Properties.Resources.kRangeExpressionConflictOperator;
                    }
                    else if (range.StepNode != null && !(range.StepNode is IntNode))
                    {
                        warningMsg = ProtoCore.Properties.Resources.kRangeExpressionWithNonIntegerStepNumber;
                    }
                    else if (step <= 0)
                    {
                        warningMsg = ProtoCore.Properties.Resources.kRangeExpressionWithNegativeStepNumber;
                    }
                }
                else if (stepoperator == ProtoCore.DSASM.RangeStepOperator.approxsize)
                {
                    if (hasAmountOp)
                    {
                        warningMsg = ProtoCore.Properties.Resources.kRangeExpressionConflictOperator;
                    }
                    else if (step == 0)
                    {
                        warningMsg = ProtoCore.Properties.Resources.kRangeExpressionWithStepSizeZero;
                    }
                }

                if (!string.IsNullOrEmpty(warningMsg))
                {
                    buildStatus.LogWarning(WarningID.kInvalidRangeExpression,
                                           warningMsg,
                                           core.CurrentDSFileName,
                                           range.StepNode.line,
                                           range.StepNode.col,
                                           graphNode);
                    EmitNullNode(new NullNode(), ref inferedType);
                    return;
                }
            }

            IntNode op = null;
            switch (range.stepoperator)
            {
                case ProtoCore.DSASM.RangeStepOperator.stepsize:
                    op = new IntNode(0);
                    break;
                case ProtoCore.DSASM.RangeStepOperator.num:
                    op = new IntNode(1);
                    break;
                case ProtoCore.DSASM.RangeStepOperator.approxsize:
                    op = new IntNode(2);
                    break;
                default:
                    op = new IntNode(-1);
                    break;
            }

            var rangeExprFunc = nodeBuilder.BuildFunctionCall(
                Constants.kFunctionRangeExpression,
                new List<ImperativeNode> 
                { 
                    range.FromNode, 
                    range.ToNode, 
                    range.StepNode ?? new NullNode(),
                    op, 
                    new BooleanNode(range.StepNode != null),
                    new BooleanNode(range.HasRangeAmountOperator) 
                });

            NodeUtils.CopyNodeLocation(rangeExprFunc, range);
            EmitFunctionCallNode(rangeExprFunc, ref inferedType, false, graphNode);

            if (range.ArrayDimensions != null)
            {
                int dimensions = DfsEmitArrayIndexHeap(range.ArrayDimensions);
                EmitInstrConsole(ProtoCore.DSASM.kw.pushindex, dimensions.ToString() + "[dim]");
                EmitPushArrayIndex(dimensions);
            }
        }

        private void EmitBreakNode(ProtoCore.AST.Node node)
        {
            ProtoCore.DSASM.CodeBlock breakableCodeBlock = codeBlock;
            while ((breakableCodeBlock != null) && (!breakableCodeBlock.isBreakable))
                breakableCodeBlock = breakableCodeBlock.parent;

            if (breakableCodeBlock != null)
            {
                int L1 = ProtoCore.DSASM.Constants.kInvalidIndex;
                BackpatchTable breakTable = backpatchMap.BreakTable[breakableCodeBlock.codeBlockId];
                if (breakTable != null)
                {
                    breakTable.Append(pc, L1);
                    EmitJmp(L1);
                }
            }
            else
            {
                if (localProcedure != null)
                {
                    core.BuildStatus.LogWarning(ProtoCore.BuildData.WarningID.kFunctionAbnormalExit, ProtoCore.Properties.Resources.kInvalidBreakForFunction , core.CurrentDSFileName, node.line, node.col);
                    EmitPushNull();
                    EmitReturnToRegister();
                }
            }
        }

        private void EmitContinueNode(ProtoCore.AST.Node node)
        {
            ProtoCore.DSASM.CodeBlock breakableCodeBlock = codeBlock;
            while ((breakableCodeBlock != null) && (!breakableCodeBlock.isBreakable))
                breakableCodeBlock = breakableCodeBlock.parent;

            if (breakableCodeBlock != null)
            {
                int entry = backpatchMap.EntryTable[breakableCodeBlock.codeBlockId];
                EmitJmp(entry);
            }
            else
            {
                if (localProcedure != null)
                {
                    core.BuildStatus.LogWarning(ProtoCore.BuildData.WarningID.kFunctionAbnormalExit, ProtoCore.Properties.Resources.kInvalidContinueForFunction, core.CurrentDSFileName, node.line, node.col);
                    EmitPushNull();
                    EmitReturnToRegister();
                }
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

            IdentifierNode result = null;

            if (identList.RightNode is IdentifierNode)
            {
                IdentifierNode thisNode = identList.RightNode as IdentifierNode;

                // a.x; => a.get_x(); 
                string getterName = ProtoCore.DSASM.Constants.kGetterPrefix + thisNode.Name;
                identList.RightNode = nodeBuilder.BuildFunctionCall(getterName, new List<ImperativeNode>()); ;

                // %t = a.get_x();
                result = nodeBuilder.BuildTempVariable() as IdentifierNode;
                var assignment = nodeBuilder.BuildBinaryExpression(result, identList);
                EmitBinaryExpressionNode(assignment, ref inferedType, false);
                result.ArrayDimensions = thisNode.ArrayDimensions;
            }
            else if (identList.RightNode is FunctionCallNode)
            {
                FunctionCallNode funcNode = identList.RightNode as FunctionCallNode;
                if (funcNode.ArrayDimensions != null)
                {
                    var arrayDimension = funcNode.ArrayDimensions;
                    funcNode.ArrayDimensions = null;

                    result = nodeBuilder.BuildTempVariable() as IdentifierNode;
                    var assignment = nodeBuilder.BuildBinaryExpression(result, identList);
                    EmitBinaryExpressionNode(assignment, ref inferedType, false);

                    result.ArrayDimensions = arrayDimension;
                }
            }

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
                    var assignment = nodeBuilder.BuildBinaryExpression(tmpVar, setterArgument as ImperativeNode);
                    EmitBinaryExpressionNode(assignment, ref inferedType, false);

                    setterArgument = tmpVar;
                }

                if (inode.LeftNode is IdentifierListNode)
                {
                    inode.LeftNode = EmitGettersForRHSIdentList(inode.LeftNode, ref inferedType, graphNode); ;
                }

                if (inode.RightNode is IdentifierNode)
                {
                    IdentifierNode rnode = inode.RightNode as IdentifierNode;
                    if (rnode.ArrayDimensions == null)
                    {
                        // %t1.x = v; => %t2 = %t1.set_x(v); 
                        String rnodeName = ProtoCore.DSASM.Constants.kSetterPrefix + rnode.Name;

                        if (setterArgument is InlineConditionalNode ||
                            setterArgument is FunctionCallNode)
                        {
                            var tmpRetVar = nodeBuilder.BuildTempVariable();
                            var tmpGetInlineRet = nodeBuilder.BuildBinaryExpression(tmpRetVar, setterArgument as ImperativeNode);
                            NodeUtils.SetNodeLocation(tmpGetInlineRet, inode, inode);
                            EmitBinaryExpressionNode(tmpGetInlineRet, ref inferedType, false);
                            inode.RightNode = nodeBuilder.BuildFunctionCall(rnodeName, new List<ImperativeNode> { tmpRetVar });
                        }
                        else
                        {
                            inode.RightNode = nodeBuilder.BuildFunctionCall(rnodeName, new List<ImperativeNode> { setterArgument as ImperativeNode });
                        }

                        var tmpVar = nodeBuilder.BuildIdentfier(Constants.kTempArg);
                        var tmpAssignmentNode = nodeBuilder.BuildBinaryExpression(tmpVar, inode);
                        EmitBinaryExpressionNode(tmpAssignmentNode, ref inferedType, false);
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
                        var tmpVar = EmitGettersForRHSIdentList(inode, ref inferedType, graphNode);

                        // %t2[i] = v;
                        var assignment = nodeBuilder.BuildBinaryExpression(tmpVar, setterArgument as ImperativeNode);
                        EmitBinaryExpressionNode(assignment, ref inferedType, false);
                        (tmpVar as IdentifierNode).ArrayDimensions = null;

                        // %t3 = %t1.%set_y(%t2[i]);
                        string setterName = ProtoCore.DSASM.Constants.kSetterPrefix + rnode.Name;
                        inode.RightNode = nodeBuilder.BuildFunctionCall(setterName, new List<ImperativeNode> { tmpVar });
                        var tmpSetterVar = nodeBuilder.BuildTempVariable();
                        assignment = nodeBuilder.BuildBinaryExpression(tmpSetterVar, inode);
                        EmitBinaryExpressionNode(assignment, ref inferedType, false);
                    }
                }
                else
                {
                    core.BuildStatus.LogSyntaxError(Resources.OnlyIdentifierOrIdentifierListCanBeOnLeftSide, core.CurrentDSFileName, inode.RightNode.line, inode.RightNode.col);
                }
            }
            else
            {
                IdentifierNode retnode = EmitGettersForRHSIdentList(node, ref inferedType, graphNode);
                if (retnode != null)
                {
                    EmitIdentifierNode(retnode, ref inferedType, false);
                    isCollapsed = true;
                }
            }

        }     

        //protected override void EmitDependency(int exprUID, bool isSSAAssign)
        protected void EmitDependency(int exprUID, bool isSSAAssign)
        {
            throw new NotImplementedException();
        }

        private void EmitPushDepData(List<ProtoCore.DSASM.SymbolNode> symbolList)
        {
            foreach (ProtoCore.DSASM.SymbolNode symbol in symbolList)
            {
                EmitInstrConsole(ProtoCore.DSASM.kw.push, symbol.name);
                StackValue op = StackValue.BuildInt(symbol.symbolTableIndex);
                EmitPush(op);
            }
        }

        private void EmitPushDep(int block, int depth, int classScope)
        {
            EmitInstrConsole(ProtoCore.DSASM.kw.pushdep, block.ToString() + "[block]", depth.ToString() + "[depth]", classScope.ToString() + "[classScope]");

            Instruction instr = new Instruction();
            instr.opCode = ProtoCore.DSASM.OpCode.PUSHDEP;
            instr.op1 = StackValue.BuildBlockIndex(block);
            instr.op2 = StackValue.BuildInt(depth);
            instr.op3 = StackValue.BuildClassIndex(classScope);

            ++pc;
            
            codeBlock.instrStream.instrList.Add(instr);

            // TODO: Figure out why using AppendInstruction fails for adding these instructions to ExpressionInterpreter
            //AppendInstruction(instr);
        }

        private void EmitSetExpressionUID(int exprId)
        {
            EmitInstrConsole(ProtoCore.DSASM.kw.setexpuid, exprId.ToString() + "[exprId]");

            Instruction instr = new Instruction();
            instr.opCode = OpCode.SETEXPUID;
            instr.op1 = StackValue.BuildInt(exprId);

            ++pc;
            
            codeBlock.instrStream.instrList.Add(instr);

            // TODO: Figure out why using AppendInstruction fails for adding these instructions to ExpressionInterpreter
            //AppendInstruction(instr);
        }

        protected override void EmitReturnNull()
        {
            EmitPushNull();
            EmitReturnToRegister();
        }

        protected void EmitGropuExpressionNode(ImperativeNode node, ref ProtoCore.Type inferedType)
        {
            GroupExpressionNode group = node as GroupExpressionNode;
            if (group == null)
            {
                return;
            }

            var tmpVar = nodeBuilder.BuildTempVariable();
            var binaryExpr = nodeBuilder.BuildBinaryExpression(tmpVar, group.Expression);
            EmitBinaryExpressionNode(binaryExpr, ref inferedType, false);

            if (group.ArrayDimensions != null)
            {
                (tmpVar as IdentifierNode).ArrayDimensions = group.ArrayDimensions;
            }
            EmitIdentifierNode(tmpVar, ref inferedType, false);
        }

        public String GetFunctionSignatureString(string functionName, ProtoCore.Type returnType, ArgumentSignatureNode signature, bool isConstructor = false)
        {
            StringBuilder functionSig = new StringBuilder(isConstructor ? "\nconstructor " : "\ndef ");
            functionSig.Append(functionName);
            functionSig.Append(":");
            functionSig.Append(core.TypeSystem.GetType(returnType.UID));
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

        private ProtoCore.Type BuildArgumentTypeFromVarDeclNode(VarDeclNode argNode, ProtoCore.AssociativeGraph.GraphNode graphNode = null)
        {
            ProtoCore.Utils.Validity.Assert(argNode != null);
            if (argNode == null)
            {
                return new ProtoCore.Type();
            }

            int uid = core.TypeSystem.GetType(argNode.ArgumentType.Name);
            if (uid == (int)PrimitiveType.kInvalidType && !core.IsTempVar(argNode.NameNode.Name))
            {
                string message = String.Format(ProtoCore.Properties.Resources.kArgumentTypeUndefined, argNode.ArgumentType.Name, argNode.NameNode.Name);
                buildStatus.LogWarning(WarningID.kTypeUndefined, message, null, argNode.line, argNode.col, graphNode);
            }

            int rank = argNode.ArgumentType.rank;
            return core.TypeSystem.BuildTypeObject(uid, rank);
        }

        private bool IsParsingGlobal()
        {
            return (!InsideFunction()) && (ProtoCore.CompilerDefinitions.Imperative.CompilePass.kGlobalScope == compilePass);
        }

        private bool IsParsingGlobalFunctionBody()
        {
            return (InsideFunction()) && (ProtoCore.CompilerDefinitions.Imperative.CompilePass.kGlobalFuncBody == compilePass);
        }

        protected void EmitIdentifierListNode(ProtoCore.AST.ImperativeAST.ImperativeNode node, ref ProtoCore.Type inferedType, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.AST.Node parentNode = null)
        {
            if (parentNode == null && !IsParsingGlobal() && !IsParsingGlobalFunctionBody())
                return;

            EmitIdentifierListNode(node, ref inferedType, false, graphNode, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, parentNode);

            if(parentNode == null)
                EmitSetExpressionUID(core.ExpressionUID++);
        }

        protected override void DfsTraverse(ProtoCore.AST.Node pNode, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, 
            ProtoCore.CompilerDefinitions.Associative.SubCompilePass subPass = ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, ProtoCore.AST.Node parentNode = null)
        {
            ImperativeNode node = pNode as ImperativeNode;
            if (null == node)
                return;

            if (node is IdentifierNode)
            {
                EmitIdentifierNode(node, ref inferedType, isBooleanOp, graphNode);
            }
            else if (node is IntNode)
            {
                EmitIntNode(node, ref inferedType, isBooleanOp);
            }
            else if (node is DoubleNode)
            {
                EmitDoubleNode(node, ref inferedType, isBooleanOp);
            }
            else if (node is BooleanNode)
            {
                EmitBooleanNode(node, ref inferedType);
            }
            else if (node is CharNode)
            {
                EmitCharNode(node, ref inferedType);
            }
            else if (node is StringNode)
            {
                EmitStringNode(node, ref inferedType);
            }
            else if (node is NullNode)
            {
                EmitNullNode(node, ref inferedType, isBooleanOp);
            }
#if ENABLE_INC_DEC_FIX
            else if (node is PostFixNode)
            {
                EmitPostFixNode(node, ref inferedType);
            }
#endif
            else if (node is LanguageBlockNode)
            {
                EmitLanguageBlockNode(node, ref inferedType, graphNode);
            }
            else if (node is ConstructorDefinitionNode)
            {
                EmitConstructorDefinitionNode(node);
            }
            else if (node is FunctionDefinitionNode)
            {
                EmitFunctionDefinitionNode(node, ref inferedType);
            }
            else if (node is FunctionCallNode)
            {
                EmitFunctionCallNode(node, ref inferedType, isBooleanOp, graphNode, parentNode as BinaryExpressionNode);
            }
            else if (node is IfStmtNode)
            {
                EmitIfStmtNode(node, ref inferedType, parentNode as BinaryExpressionNode, isBooleanOp, graphNode);
            }
            else if (node is WhileStmtNode)
            {
                EmitWhileStmtNode(node, ref inferedType, isBooleanOp, graphNode);
            }
            else if (node is VarDeclNode)
            {
                EmitVarDeclNode(node, ref inferedType, graphNode);
            }
            else if (node is ExprListNode)
            {
                EmitExprListNode(node, ref inferedType, null, ProtoCore.CompilerDefinitions.Associative.SubCompilePass.kNone, parentNode);
            }
            else if (node is IdentifierListNode)
            {
                EmitIdentifierListNode(node, ref inferedType, graphNode, parentNode as BinaryExpressionNode);
            }
            else if (node is BinaryExpressionNode)
            {
                EmitBinaryExpressionNode(node, ref inferedType, isBooleanOp, graphNode, parentNode as BinaryExpressionNode);
            }
            else if (node is UnaryExpressionNode)
            {
                EmitUnaryExpressionNode(node, ref inferedType, parentNode as BinaryExpressionNode);
            }
            else if (node is ForLoopNode)
            {
                EmitForLoopNode(node, ref inferedType, isBooleanOp, graphNode);
            }
            else if (node is InlineConditionalNode)
            {
                EmitInlineConditionalNode(node, ref inferedType, parentNode as BinaryExpressionNode);
            }
            else if (node is RangeExprNode)
            {
                EmitRangeExprNode(node, ref inferedType, graphNode);
            }
            else if (node is BreakNode)
            {
                EmitBreakNode(node);
            }
            else if (node is ContinueNode)
            {
                EmitContinueNode(node);
            }
            else if (node is DefaultArgNode)
            {
                EmitDefaultArgNode();
            }
            else if (node is GroupExpressionNode)
            {
                EmitGropuExpressionNode(node, ref inferedType);
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

        public ImperativeNode BuildIdentfier(string name, PrimitiveType type = PrimitiveType.kTypeVar)
        {
            var ident = new IdentifierNode();
            ident.Name = ident.Value = name;
            ident.datatype = TypeSystem.BuildPrimitiveTypeObject(type, 0);

            return ident;
        }

        public ImperativeNode BuildTempVariable()
        {
            return BuildIdentfier(core.GenerateTempVar(), PrimitiveType.kTypeVar);
        }

        public ImperativeNode BuildReturn()
        {
            return BuildIdentfier(ProtoCore.DSDefinitions.Keyword.Return, PrimitiveType.kTypeReturn);
        }

        public ImperativeNode BuildIdentList(ImperativeNode leftNode, ImperativeNode rightNode)
        {
            var identList = new IdentifierListNode();
            identList.LeftNode = leftNode;
            identList.RightNode = rightNode;
            identList.Optr = ProtoCore.DSASM.Operator.dot;
            return identList;
        }

        public ImperativeNode BuildBinaryExpression(ImperativeNode leftNode, ImperativeNode rightNode, Operator op = Operator.assign)
        {
            var binaryExpr = new BinaryExpressionNode();
            binaryExpr.LeftNode = leftNode;
            binaryExpr.Optr = op;
            binaryExpr.RightNode = rightNode;
            return binaryExpr;
        }

        public ImperativeNode BuildFunctionCall(string functionName, List<ImperativeNode> arguments)
        {
            var func = new FunctionCallNode();
            func.Function = BuildIdentfier(functionName);
            func.FormalArguments = arguments;

            return func;
        }
    }
}

