

using System;
using System.Diagnostics;
using System.Collections.Generic;
using Deuterium.AST;
using FusionCore;

namespace Deuterium
{
    public class BackpatchNode
    {
        public int bp;
        public int pc;
    }

    public class BackpatchTable
    {
        public List<BackpatchNode> backpatchList { get; private set; }
        public BackpatchTable()
        {
            backpatchList = new List<BackpatchNode>();
        }

        public void append(int bp, int pc)
        {
            BackpatchNode node = new BackpatchNode();
            node.bp = bp;
            node.pc = pc;
            backpatchList.Add(node);
        }

        public void append(int bp)
        {
            BackpatchNode node = new BackpatchNode();
            node.bp = bp;
            node.pc = (int)FusionCore.DSASM.Constants.kInvalidIndex;
            backpatchList.Add(node);
        }
    }

    public class CodeGen
    {
        private int locals = 0;
        private int argOffset = 0;
        private int functionindex = (int)FusionCore.DSASM.Constants.kGlobalScope;
        private bool isEntrySet = false;

        private List<Node> astNodes;
        public FusionCore.DSASM.SymbolTable symbols { get; set; }
        public FusionCore.DSASM.FunctionTable functions { get; set; }
        public FusionCore.DSASM.Executable executable { get; set; }

        private int pc = 0;
        private int globOffset = 0;
        private int baseOffset = 0;
        private bool dumpByteCode = false;

        public BackpatchTable backpatchTable{ get; set; }

        public CodeGen()
        {
            locals = 0;
            globOffset = 0;
            baseOffset = 0;
            argOffset = 0;
            symbols = symbols = new FusionCore.DSASM.SymbolTable();

            astNodes = new List<Node>();
            functions = new FusionCore.DSASM.FunctionTable();
            executable = new FusionCore.DSASM.Executable();
            backpatchTable = new BackpatchTable();
        }

        private void setEntry()
        {
            if (functionindex == (int)FusionCore.DSASM.Constants.kGlobalScope && !isEntrySet)
            {
                isEntrySet = true;
                executable.entrypoint = pc;
            }
        }

        private void Allocate(string ident, int funcIndex, FusionCore.PrimitiveType datatype, int datasize = (int)FusionCore.DSASM.Constants.kPrimitiveSize)
        {
            FusionCore.DSASM.SymbolNode node = new FusionCore.DSASM.SymbolNode();
            node.name = ident;
            node.size = datasize;
            node.functionIndex = funcIndex;
            node.datatype = datatype;
            node.isArgument = false;

            // TODO Jun: Shouldnt the offset increment be done upon successfully appending the symbol?
            if ((int)FusionCore.DSASM.Constants.kGlobalScope == funcIndex)
            {
                node.index = globOffset;
                globOffset += node.size;
            }
            else
            {
                node.index = -2 - baseOffset;
                baseOffset += node.size;
            }
            symbols.Update(node);
        }

        private void AllocateArg(string ident, int funcIndex, FusionCore.PrimitiveType datatype, int datasize = (int)FusionCore.DSASM.Constants.kPrimitiveSize)
        {
            FusionCore.DSASM.SymbolNode node = new FusionCore.DSASM.SymbolNode();
            node.name = ident;
            node.size = datasize;
            node.functionIndex = funcIndex;
            node.datatype = datatype;
            node.isArgument = true;

            int locOffset = functions.functionList[funcIndex].localCount;

            // This is standard
            argOffset++;
            node.index = -2 - (locOffset + argOffset);

            // This is through FEP
            //node.index = argOffset++;

            symbols.Update(node);
        }

        FusionCore.DSASM.AddressType getOpType(FusionCore.PrimitiveType type)
        {
            FusionCore.DSASM.AddressType optype = FusionCore.DSASM.AddressType.Int;
            // Data coercion for the prototype
            // The JIL executive handles int primitives
            if (FusionCore.PrimitiveType.kTypeInt == type
                || FusionCore.PrimitiveType.kTypeDouble == type
                || FusionCore.PrimitiveType.kTypeBool == type
                || FusionCore.PrimitiveType.kTypeChar == type
                || FusionCore.PrimitiveType.kTypeString == type)
            {
                optype = FusionCore.DSASM.AddressType.Int;
            }
            else if (FusionCore.PrimitiveType.kTypeVar == type)
            {
                optype = FusionCore.DSASM.AddressType.VarIndex;
            }
            else if (FusionCore.PrimitiveType.kTypeReturn == type)
            {
                optype = FusionCore.DSASM.AddressType.Register;
            }
            else
            {
                Debug.Assert(false);
            }
            return optype;
        }

        FusionCore.DSASM.OpCode getOpCode(Operator optr)
        {
            FusionCore.DSASM.OpCode opcode = FusionCore.DSASM.OpCode.ADD;
            // TODO jun: perhaps a table?
            if (Operator.add == optr)
            {
                opcode = FusionCore.DSASM.OpCode.ADD;
            }
            else if (Operator.sub == optr)
            {
                opcode = FusionCore.DSASM.OpCode.SUB;
            }
            else if (Operator.mul == optr)
            {
                opcode = FusionCore.DSASM.OpCode.MULT;
            }
            else if (Operator.div == optr)
            {
                opcode = FusionCore.DSASM.OpCode.DIV;
            }
            else if (Operator.eq == optr)
            {
                opcode = FusionCore.DSASM.OpCode.EQ;
            }
            else if (Operator.nq == optr)
            {
                opcode = FusionCore.DSASM.OpCode.NQ;
            }
            else if (Operator.ge == optr)
            {
                opcode = FusionCore.DSASM.OpCode.GE;
            }
            else if (Operator.gt == optr)
            {
                opcode = FusionCore.DSASM.OpCode.GT;
            }
            else if (Operator.le == optr)
            {
                opcode = FusionCore.DSASM.OpCode.LE;
            }
            else if (Operator.lt == optr)
            {
                opcode = FusionCore.DSASM.OpCode.LT;
            }
            else
            {
                Debug.Assert(false);
            }
            return opcode;
        }

        FusionCore.DSASM.Operand buildOperand(TerminalNode node)
        {
            FusionCore.DSASM.Operand op = new FusionCore.DSASM.Operand();
            op.optype = getOpType((FusionCore.PrimitiveType)node.type);
            if (FusionCore.DSASM.AddressType.VarIndex == op.optype)
            {
                //FusionCore.DSASM.SymbolNode
                int size = symbols.symbolList.Count;
                for (int n = 0; n < size; ++n)
                {
                    // TODO jun: Hash the string
                    if (functionindex == symbols.symbolList[n].functionIndex && symbols.symbolList[n].name == node.Value)
                    {
                        op.opdata = n; // symbols.symbolList[n].index;
                        break;
                    }
                }
            }
            else if (FusionCore.DSASM.AddressType.Int == op.optype)
            {
                op.opdata = System.Convert.ToInt32(node.Value);
            }
            else if (FusionCore.DSASM.AddressType.Register == op.optype)
            {
                op.opdata = (int)FusionCore.DSASM.Registers.RX;
            }
            else
            {
                Debug.Assert(false);
            }
            return op;
        }

        public void emit(string s)
        {
            if (dumpByteCode)
            {
                System.Console.Write("[" + pc + "]" + s);
            }
        }

        private void emitPush(FusionCore.DSASM.Operand op)
        {
            setEntry();
            FusionCore.DSASM.Instruction instr = new FusionCore.DSASM.Instruction();
            instr.opCode = FusionCore.DSASM.OpCode.PUSH;
            instr.op1 = op;

            pc++;
            executable.instructionList.Add(instr);
        }

        private void emitPop(FusionCore.DSASM.Operand op)
        {
            FusionCore.DSASM.Instruction instr = new FusionCore.DSASM.Instruction();
            instr.opCode = FusionCore.DSASM.OpCode.POP;
            instr.op1 = op;

            // For debugging, assert here but these should raise runtime errors in the VM
            Debug.Assert(FusionCore.DSASM.AddressType.VarIndex == op.optype || FusionCore.DSASM.AddressType.Register == op.optype);

            pc++;
            executable.instructionList.Add(instr);
        }

        private void emitReturn()
        {
            FusionCore.DSASM.Instruction instr = new FusionCore.DSASM.Instruction();
            instr.opCode = FusionCore.DSASM.OpCode.RETURN;

            pc++;
            executable.instructionList.Add(instr);
        }

        private void emitBinary(FusionCore.DSASM.OpCode opcode, FusionCore.DSASM.Operand op1, FusionCore.DSASM.Operand op2)
        {
            setEntry();
            FusionCore.DSASM.Instruction instr = new FusionCore.DSASM.Instruction();
            instr.opCode = opcode;
            instr.op1 = op1;
            instr.op2 = op2;

            // For debugging, assert here but these should raise runtime errors in the VM
            Debug.Assert(FusionCore.DSASM.AddressType.VarIndex == op1.optype || FusionCore.DSASM.AddressType.Register == op1.optype);

            pc++;
            executable.instructionList.Add(instr);
        }

        private void emitCall(int funcIndex)
        {
            setEntry();
            FusionCore.DSASM.Instruction instr = new FusionCore.DSASM.Instruction();
            instr.opCode = FusionCore.DSASM.OpCode.CALL;

            FusionCore.DSASM.Operand op = new FusionCore.DSASM.Operand();
            op.optype = FusionCore.DSASM.AddressType.FunctionIndex;
            op.opdata = funcIndex;
            instr.op1 = op;

            pc++;
            executable.instructionList.Add(instr);
        }

        private void emitJmp(int L1)
        {
            emit(FusionCore.DSASM.kw.jmp + " L1(" + L1 + ")" + FusionCore.DSASM.Constants.termline);

            FusionCore.DSASM.Instruction instr = new FusionCore.DSASM.Instruction();
            instr.opCode = FusionCore.DSASM.OpCode.JMP;

            FusionCore.DSASM.Operand op1 = new FusionCore.DSASM.Operand();
            op1.optype = FusionCore.DSASM.AddressType.LabelIndex;
            op1.opdata = L1;
            instr.op1 = op1;

            pc++;
            executable.instructionList.Add(instr);
        }
        
        private void emitCJmp(int L1, int L2)
        {
            emit(FusionCore.DSASM.kw.cjmp + " " + FusionCore.DSASM.kw.regCX + " L1(" + L1 + ") L2(" + L2 + ")" + FusionCore.DSASM.Constants.termline);

            FusionCore.DSASM.Instruction instr = new FusionCore.DSASM.Instruction();
            instr.opCode = FusionCore.DSASM.OpCode.CJMP;
            
            FusionCore.DSASM.Operand op1 = new FusionCore.DSASM.Operand();
            op1.optype = FusionCore.DSASM.AddressType.Register;
            op1.opdata = (int)FusionCore.DSASM.Registers.CX;
            instr.op1 = op1;
            
            FusionCore.DSASM.Operand op2 = new FusionCore.DSASM.Operand();
            op2.optype = FusionCore.DSASM.AddressType.LabelIndex;
            op2.opdata = L1;
            instr.op2 = op2;

            FusionCore.DSASM.Operand op3 = new FusionCore.DSASM.Operand();
            op3.optype = FusionCore.DSASM.AddressType.LabelIndex;
            op3.opdata = L2;
            instr.op3 = op3;

            pc++;
            executable.instructionList.Add(instr);
        }

        private void backpatch(int bp, int pc)
        {
	        if(FusionCore.DSASM.OpCode.JMP == executable.instructionList[bp].opCode
                && FusionCore.DSASM.AddressType.LabelIndex == executable.instructionList[bp].op1.optype)
            {
		        executable.instructionList[bp].op1.opdata = pc;
            }
            else if (FusionCore.DSASM.OpCode.CJMP == executable.instructionList[bp].opCode
                && FusionCore.DSASM.AddressType.LabelIndex == executable.instructionList[bp].op3.optype)
            {
                executable.instructionList[bp].op3.opdata = pc;
	        }
        }

        private void backpatch(List<BackpatchNode> table, int pc)
        {
	        foreach( BackpatchNode node in table )
            {
                backpatch(node.bp, pc);
	        }
        }

        private string getOperator(Operator op)
        {
            // TODO jun: perhaps a table?
            string sOp = "";
            if (Operator.add == op)
            {
                sOp = "add";
            }
            else if (Operator.sub == op)
            {
                sOp = "sub";
            }
            else if (Operator.mul == op)
            {
                sOp = "mul";
            }
            else if (Operator.div == op)
            {
                sOp = "div";
            }
            else if (Operator.eq == op)
            {
                sOp = "eq";
            }
            else if (Operator.nq == op)
            {
                sOp = "nq";
            }
            else if (Operator.ge == op)
            {
                sOp = "ge";
            }
            else if (Operator.gt == op)
            {
                sOp = "gt";
            }
            else if (Operator.le == op)
            {
                sOp = "le";
            }
            else if (Operator.lt == op)
            {
                sOp = "lt";
            }
            return sOp;
        }

        private bool isAssignmentNode( Node node )
        {
            if( node is BinaryExpressionNode ) {
                BinaryExpressionNode b = node as BinaryExpressionNode;
                if( Operator.assign == b.Operator ) {
                    return false; 
                }
            }
            return false;
        }

        private void emitAssignLHS(Node node)
        {

        }

        public void emit(CodeBlockNode codeblock)
        {
            astNodes = codeblock.Body;
            foreach(Node node in astNodes)
            {
                dfsTraverse(node);
            }

            executable.functionTable = functions;

            executable.globsize = symbols.globs;
            executable.debugsymbols = symbols;
        }

        private void dfsTraverse(Node node)
        {
            if (node is TerminalNode)
            {
                TerminalNode t = node as TerminalNode;
                emit("push " + t.Value + ";\n");

                FusionCore.DSASM.Operand op = buildOperand(t);
                emitPush(op);
            }
            else if (node is FunctionDefinitionNode)
            {
                FunctionDefinitionNode funcDef = node as FunctionDefinitionNode;

                // TODO jun: Add semantics for checking overloads (different parameter types)
                FusionCore.DSASM.FunctionNode fnode = new FusionCore.DSASM.FunctionNode();
                fnode.name = funcDef.Name;
                fnode.paramCount = null == funcDef.Signature ? 0 : funcDef.Signature.Arguments.Count;
                fnode.pc = pc;
                fnode.localCount = funcDef.localVars;
                fnode.returntype = FusionCore.TypeSystem.getType(funcDef.ReturnType.Name);
                functionindex = functions.Append(fnode);

                // Append arg symbols
                if (fnode.paramCount > 0)
                {
                    foreach (VarDeclNode argNode in funcDef.Signature.Arguments)
                    {
                        AllocateArg(argNode.NameNode.Value, functionindex, FusionCore.TypeSystem.getType(argNode.ArgumentType.Name));
                    }
                }

                // Traverse definition
                foreach (Node bnode in funcDef.FunctionBody.Body)
                {
                    dfsTraverse(bnode);
                }

                // Append to the function group functiontable
                FusionCore.FunctionGroup funcGroup = new FusionCore.FunctionGroup();

                // ArgList
                if (fnode.paramCount > 0)
                {
                    List<FusionCore.Type> parameterTypes = new List<FusionCore.Type>();
                    foreach (VarDeclNode argNode in funcDef.Signature.Arguments)
                    {
                        parameterTypes.Add(FusionCore.TypeSystem.buildTypeObject(FusionCore.TypeSystem.getType(argNode.ArgumentType.Name), false));
                    }
                }

                // function return
                emit("ret" + FusionCore.DSASM.Constants.termline);
                emitReturn();
                functionindex = (int)FusionCore.DSASM.Constants.kGlobalScope;
                argOffset = 0;
                baseOffset = 0;
            }
            else if (node is FunctionCallNode)
            {
                FunctionCallNode funcCall = node as FunctionCallNode;
                FusionCore.DSASM.FunctionNode fnode = new FusionCore.DSASM.FunctionNode();
                fnode.name = funcCall.Function.Name;
                fnode.paramCount = funcCall.FormalArguments.Count;

                // Traverse the function args
                foreach (Node paramNode in funcCall.FormalArguments)
                {
                    dfsTraverse(paramNode);
                }

                int fIndex = functions.getIndex(fnode);
                if ((int)FusionCore.DSASM.Constants.kInvalidIndex != fIndex)
                {
                    emit("call " + fnode.name + FusionCore.DSASM.Constants.termline);
                    emitCall(fIndex);

                    if (FusionCore.PrimitiveType.kTypeVoid != functions.functionList[fIndex].returntype)
                    {
                        emit("push " + FusionCore.DSASM.kw.regRX + FusionCore.DSASM.Constants.termline);
                        FusionCore.DSASM.Operand opRes;
                        opRes.optype = FusionCore.DSASM.AddressType.Register;
                        opRes.opdata = (int)FusionCore.DSASM.Registers.RX;
                        emitPush(opRes);
                    }
                }
                else
                {
                    System.Console.WriteLine("Method '" + fnode.name + "' not found\n");
                }
            }
            else if (node is IfStmtNode)
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

                // TODO jun: Try to break up this emitter without while retaining theoretical meaning
                int bp = (int)FusionCore.DSASM.Constants.kInvalidIndex; 
                int L1 = (int)FusionCore.DSASM.Constants.kInvalidIndex;
                int L2 = (int)FusionCore.DSASM.Constants.kInvalidIndex;
                FusionCore.DSASM.Operand opCX;


                // If-expr
                IfStmtNode ifnode = node as IfStmtNode;
                dfsTraverse(ifnode.IfExprNode);

                emit("pop " + FusionCore.DSASM.kw.regCX + FusionCore.DSASM.Constants.termline);
                opCX.optype = FusionCore.DSASM.AddressType.Register;
                opCX.opdata = (int)FusionCore.DSASM.Registers.CX;
                emitPop(opCX);

                L1 = pc + 1;
                L2 = (int)FusionCore.DSASM.Constants.kInvalidIndex;
                bp = pc;
                emitCJmp(L1, L2);

                // If-body
                foreach (Node ifBody in ifnode.IfBody)
                {
                    dfsTraverse(ifBody);
                }

                L1 = (int)FusionCore.DSASM.Constants.kInvalidIndex;
                backpatchTable.append(pc, L1);
                emitJmp(L1);
                backpatch(bp, pc);


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
                    dfsTraverse(elseifNode.Expr);

                    emit("pop " + FusionCore.DSASM.kw.regCX + FusionCore.DSASM.Constants.termline);
                    opCX.optype = FusionCore.DSASM.AddressType.Register;
                    opCX.opdata = (int)FusionCore.DSASM.Registers.CX;
                    emitPop(opCX);

                    L1 = pc + 1;
                    L2 = (int)FusionCore.DSASM.Constants.kInvalidIndex;
                    bp = pc;
                    emitCJmp(L1, L2);

                    // Elseif-body   
                    if (null != elseifNode.Body)
                    {
                        foreach (Node elseifBody in elseifNode.Body)
                        {
                            dfsTraverse(elseifBody);
                        }
                    }

                    L1 = (int)FusionCore.DSASM.Constants.kInvalidIndex;
                    backpatchTable.append(pc, L1);
                    emitJmp(L1);
                    backpatch(bp, pc);
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
                if (null != ifnode.ElseBody)
                {
                    foreach (Node elseBody in ifnode.ElseBody)
                    {
                        dfsTraverse(elseBody);
                    }

                    L1 = (int)FusionCore.DSASM.Constants.kInvalidIndex;
                    backpatchTable.append(pc, L1);
                    emitJmp(L1);
                    //backpatch(bp, pc);
                }

                /*
                 * 
			              ->	backpatch(bpTable, pc) 
                 */
                // ifstmt-exit
                backpatch(backpatchTable.backpatchList, pc);


            }
            else if (node is VarDeclNode)
            {
                VarDeclNode varNode = node as VarDeclNode;
                Allocate(varNode.NameNode.Value, functionindex, FusionCore.TypeSystem.getType(varNode.ArgumentType.Name));
            }
            else if (node is BinaryExpressionNode)
            {
                BinaryExpressionNode b = node as BinaryExpressionNode;
                if (Operator.assign != b.Operator)
                {
                    dfsTraverse(b.LeftNode);
                }
                dfsTraverse(b.RightNode);

                if (Operator.assign == b.Operator)
                {
                    if (b.LeftNode is TerminalNode)
                    {
                        TerminalNode t = b.LeftNode as TerminalNode;

                        bool isReturn = false;
                        string s = t.Value;
                        if (s == "return")
                        {
                            s = "_rx";
                            //isReturn = true;
                        }

                        // TODO jun: the emit string are only for console logging, 
                        // wrap them together with the actual emit function and flag them out as needed
                        emit("pop " + s + FusionCore.DSASM.Constants.termline);

                        FusionCore.DSASM.Operand op = buildOperand(t);
                        emitPop(op);

                        //if (isReturn)
                        //{
                        //    emit("ret" + FusionCore.DSASM.Constants.termline);
                        //    emitReturn();
                        //    functionindex = (int)FusionCore.DSASM.Constants.kGlobalScope;
                        //}
                    }
                }
                else
                {
                    emit("pop " + FusionCore.DSASM.kw.regBX + FusionCore.DSASM.Constants.termline);
                    FusionCore.DSASM.Operand opBX;
                    opBX.optype = FusionCore.DSASM.AddressType.Register;
                    opBX.opdata = (int)FusionCore.DSASM.Registers.BX;
                    emitPop(opBX);


                    emit("pop " + FusionCore.DSASM.kw.regAX + FusionCore.DSASM.Constants.termline);
                    FusionCore.DSASM.Operand opAX;
                    opAX.optype = FusionCore.DSASM.AddressType.Register;
                    opAX.opdata = (int)FusionCore.DSASM.Registers.AX;
                    emitPop(opAX);


                    string op = getOperator(b.Operator);
                    emit(op + " " + FusionCore.DSASM.kw.regAX + ", " + FusionCore.DSASM.kw.regBX + FusionCore.DSASM.Constants.termline);
                    emitBinary(getOpCode(b.Operator), opAX, opBX);


                    emit("push " + FusionCore.DSASM.kw.regAX + FusionCore.DSASM.Constants.termline);
                    FusionCore.DSASM.Operand opRes;
                    opRes.optype = FusionCore.DSASM.AddressType.Register;
                    opRes.opdata = (int)FusionCore.DSASM.Registers.AX;
                    emitPush(opRes);
                }
            }
        }
    }
}

