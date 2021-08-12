
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
        private Interpreter mInterpreter;
        private bool explicitCall;
        private bool isDispose;
        private int execStateSize;
        private int formalParametersCount;

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

        private void Init(ProtoCore.Runtime.Context c, List<StackValue> formalParameters, ProtoCore.DSASM.StackFrame stackFrame, RuntimeCore runtimeCore)
        {
            if (mInterpreter != null) return;

            mInterpreter = new Interpreter(runtimeCore, true);
            activation.globs = runtimeCore.DSExecutable.runtimeSymbols[runtimeCore.RunningBlock].GetGlobalSize();
            isDispose = CoreUtils.IsDisposeMethod(procedureNode.Name);
            execStateSize = procedureNode.GraphNodeList.Count;
            formalParametersCount = formalParameters.Count;
        }

        public override StackValue Execute(ProtoCore.Runtime.Context c, List<StackValue> formalParameters, ProtoCore.DSASM.StackFrame stackFrame, RuntimeCore runtimeCore)
        {
            if (mInterpreter == null)
            {
                Init(c, formalParameters, stackFrame, runtimeCore);
            }
            else
            {
                mInterpreter.ResetInterpreter(runtimeCore, true);
            }

            ProtoCore.DSASM.Executive oldDSASMExec = null;
            if (runtimeCore.CurrentExecutive != null)
            {
                oldDSASMExec = runtimeCore.CurrentExecutive.CurrentDSASMExec;
                runtimeCore.CurrentExecutive.CurrentDSASMExec = mInterpreter.runtime;
            }

            try
            {
                //
                // Comment Jun:
                // Storing execution states is relevant only if the current scope is a function,
                // as this mechanism is used to keep track of maintining execution states of recursive calls
                // This mechanism should also be ignored if the function call is non-recursive as it does not need to maintains state in that case
                stackFrame.ExecutionStateSize = execStateSize;
                for (int n = execStateSize - 1; n >= 0; --n)
                {
                    mInterpreter.Push(StackValue.BuildBoolean(procedureNode.GraphNodeList[n].isDirty));
                }

                // Push Params
                for (int i = formalParametersCount - 1; i >= 0; i--)
                {
                    mInterpreter.Push(formalParameters[i]);
                }

                // Jun: Make sure we have no empty or unaligned frame data
                Validity.Assert(DSASM.StackFrame.StackFrameSize == stackFrame.Frame.Length);

                // Update the running block to tell the execution engine which set of instruction to execute
                // TODO(Jun/Jiong): Considering store the orig block id to stack frame
                int origRunningBlock = runtimeCore.RunningBlock;
                runtimeCore.RunningBlock = stackFrame.FunctionBlock;

                StackValue svCallConvention;
                explicitCall = !c.IsReplicating && !c.IsImplicitCall && !isDispose;

                if (explicitCall)
                {
                    svCallConvention = StackValue.BuildCallingConversion((int)ProtoCore.DSASM.CallingConvention.CallType.Explicit);
                }
                else
                {
                    svCallConvention = StackValue.BuildCallingConversion((int)ProtoCore.DSASM.CallingConvention.CallType.Implicit);
                }
                mInterpreter.runtime.TX = svCallConvention;
                stackFrame.TX = svCallConvention;

                // Comment Jun: the depth is always 0 for a function call as we are reseting this for each function call
                // This is only incremented for every language block bounce
                int depth = stackFrame.Depth;
                DSASM.StackFrameType type = stackFrame.StackFrameType;
                Validity.Assert(depth == 0);
                Validity.Assert(type == DSASM.StackFrameType.Function);

                runtimeCore.RuntimeMemory.PushFrameForLocals(activation.locals);
                //StackFrame newStackFrame = new StackFrame(svThisPtr, ci, fi, returnAddr, blockDecl, blockCaller, callerType, type, depth, framePointer, svBlockDecl.BlockIndex, stackFrame.GetRegisters(), execStateSize);
                StackFrame newStackFrame = new StackFrame(stackFrame.Frame)
                {
                    ClassScope = activation.classIndex,
                    FunctionScope = activation.funcIndex
                };

                runtimeCore.RuntimeMemory.PushStackFrame(newStackFrame);

                StackValue svRet;

                if (explicitCall)
                {
                    svRet = ProtoCore.DSASM.StackValue.BuildExplicitCall(activation.pc);
                }
                else
                {
                    svRet = mInterpreter.Run(runtimeCore.RunningBlock, activation.pc, Language.NotSpecified, runtimeCore.Breakpoints);
                    runtimeCore.RunningBlock = origRunningBlock;
                }

                return svRet;
            }
            finally
            {
                if (runtimeCore.CurrentExecutive != null)
                {
                    runtimeCore.CurrentExecutive.CurrentDSASMExec = oldDSASMExec;
                }
            }
        }
    }
}
