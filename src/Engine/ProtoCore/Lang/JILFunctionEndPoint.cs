
using System.Collections.Generic;
using ProtoCore.DSASM;
using ProtoCore.Lang.Replication;
using ProtoCore.Utils;

namespace ProtoCore.Lang
{
    public class JILActivationRecord
    {
        public int pc;
        public int locals;
        public int globs;
        public int retAddr;
        public int funcIndex;
        public int classIndex;
    }

    public class JILFunctionEndPoint : FunctionEndPoint
    {
        private readonly JILActivationRecord activation;
        public JILFunctionEndPoint()
        {
            activation = new JILActivationRecord();
        }

        public JILFunctionEndPoint(JILActivationRecord activation)
        {
            this.activation = activation;
        }

        public override bool DoesPredicateMatch(ProtoCore.Runtime.Context c, List<StackValue> formalParameters, List<ReplicationInstruction> replicationInstructions)
        {
            //@TODO: FIXME
            return true;
        }

        public override StackValue Execute(ProtoCore.Runtime.Context c, List<StackValue> formalParameters, ProtoCore.DSASM.StackFrame stackFrame, Core core)
        {
            ProtoCore.DSASM.Interpreter interpreter = new ProtoCore.DSASM.Interpreter(core, true);
            ProtoCore.DSASM.Executive oldDSASMExec = null;
            if (core.CurrentExecutive != null)
            {
                oldDSASMExec = core.CurrentExecutive.CurrentDSASMExec;
                core.CurrentExecutive.CurrentDSASMExec = interpreter.runtime;
            }

            // Assert for the block type
            activation.globs = core.DSExecutable.runtimeSymbols[core.RunningBlock].GetGlobalSize();

            // Push Execution states
            int execStateSize = 0;
            if (null != stackFrame.ExecutionStates)
            {
                execStateSize = stackFrame.ExecutionStates.Length;

                // ExecutionStates are in lexical order
                // Push them in reverse order (similar to args) so they can be retrieved in sequence
                // Retrieveing the executing states occur on function return
                for (int n = execStateSize - 1; n >= 0 ; --n)
                {
                    StackValue svState = stackFrame.ExecutionStates[n];
                    Validity.Assert(svState.optype == DSASM.AddressType.Boolean);
                    interpreter.Push(svState);
                }
            }

            // Push Params
            formalParameters.Reverse();
            for (int i = 0; i < formalParameters.Count; i++)
            {
                interpreter.Push(formalParameters[i]);
            }

            StackValue svThisPtr = stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kThisPtr);
            StackValue svBlockDecl = stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kFunctionBlock);

            // Jun: Make sure we have no empty or unaligned frame data
            Validity.Assert(DSASM.StackFrame.kStackFrameSize == stackFrame.Frame.Length);

            // Setup the stack frame data
            //int thisPtr = (int)stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kThisPtr).opdata;
            int ci = activation.classIndex;
            int fi = activation.funcIndex;
            int returnAddr = (int)stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kReturnAddress).opdata;
            int blockDecl = (int)svBlockDecl.opdata;
            int blockCaller = (int)stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kFunctionCallerBlock).opdata;
            int framePointer = core.Rmem.FramePointer; 
            int locals = activation.locals;
            

            // Update the running block to tell the execution engine which set of instruction to execute
            // TODO(Jun/Jiong): Considering store the orig block id to stack frame
            int origRunningBlock = core.RunningBlock;
            core.RunningBlock = (int)svBlockDecl.opdata;

            // Set SX register 
            interpreter.runtime.SX = svBlockDecl;

            DSASM.StackFrameType callerType = (DSASM.StackFrameType)stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kCallerStackFrameType).opdata;

            List<StackValue> registers = new List<DSASM.StackValue>();

            StackValue svCallConvention;
            bool isDispose = procedureNode.name.Equals(ProtoCore.DSDefinitions.Keyword.Dispose);

            bool explicitCall = !c.IsReplicating && !c.IsImplicitCall && !isDispose;
            if (explicitCall)
            {
                svCallConvention = StackValue.BuildCallingConversion((int)ProtoCore.DSASM.CallingConvention.CallType.kExplicit);
            }
            else
            {
                svCallConvention = StackValue.BuildCallingConversion((int)ProtoCore.DSASM.CallingConvention.CallType.kImplicit);                
            }

            stackFrame.SetAt(DSASM.StackFrame.AbsoluteIndex.kRegisterTX, svCallConvention);
            interpreter.runtime.TX = svCallConvention;

            // Set SX register 
            stackFrame.SetAt(DSASM.StackFrame.AbsoluteIndex.kRegisterSX, svBlockDecl);
            interpreter.runtime.SX = svBlockDecl;

            // TODO Jun:
            // The stackframe carries the current set of registers
            // Determine if this can be done even for the non explicit call implementation
            registers.AddRange(stackFrame.GetRegisters());


            // Comment Jun: the depth is always 0 for a function call as we are reseting this for each function call
            // This is only incremented for every language block bounce
            int depth = (int)stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kStackFrameDepth).opdata;
            DSASM.StackFrameType type = (DSASM.StackFrameType)stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kStackFrameType).opdata;
            Validity.Assert(depth == 0);
            Validity.Assert(type == DSASM.StackFrameType.kTypeFunction);

            core.Rmem.PushStackFrame(svThisPtr, ci, fi, returnAddr, blockDecl, blockCaller, callerType, type, depth, framePointer, registers, locals, execStateSize);


            StackValue svRet;

            if (explicitCall)
            {
                svRet = ProtoCore.DSASM.StackValue.BuildExplicitCall(activation.pc);
            }
            else
            {
                if (core.ExecMode != DSASM.InterpreterMode.kExpressionInterpreter && core.Options.IDEDebugMode)
                {
                    svRet = interpreter.Run(core.Breakpoints, core.RunningBlock, activation.pc, Language.kInvalid);
                }
                else
                {
                    svRet = interpreter.Run(core.RunningBlock, activation.pc, Language.kInvalid);
                }
                core.RunningBlock = origRunningBlock;
            }

            if (core.CurrentExecutive != null)
            {
                core.CurrentExecutive.CurrentDSASMExec = oldDSASMExec;
            }
            return svRet; //DSASM.Mirror.ExecutionMirror.Unpack(svRet, core.heap, core);
        }
    }
}
