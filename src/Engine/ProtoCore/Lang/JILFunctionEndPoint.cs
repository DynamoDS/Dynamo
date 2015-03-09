
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

        public override StackValue Execute(ProtoCore.Runtime.Context c, List<StackValue> formalParameters, ProtoCore.DSASM.StackFrame stackFrame, RuntimeCore runtimeCore)
        {
            ProtoCore.DSASM.Interpreter interpreter = new ProtoCore.DSASM.Interpreter(runtimeCore, true);
            ProtoCore.DSASM.Executive oldDSASMExec = null;
            if (runtimeCore.CurrentExecutive != null)
            {
                oldDSASMExec = runtimeCore.CurrentExecutive.CurrentDSASMExec;
                runtimeCore.CurrentExecutive.CurrentDSASMExec = interpreter.runtime;
            }

            // Assert for the block type
            activation.globs = runtimeCore.DSExecutable.runtimeSymbols[runtimeCore.RunningBlock].GetGlobalSize();

            //
            // Comment Jun:
            // Storing execution states is relevant only if the current scope is a function,
            // as this mechanism is used to keep track of maintining execution states of recursive calls
            // This mechanism should also be ignored if the function call is non-recursive as it does not need to maintains state in that case
            int execStateSize = procedureNode.GraphNodeList.Count;
            stackFrame.ExecutionStateSize = execStateSize;
            for (int n = execStateSize - 1; n >= 0; --n)
            {
                AssociativeGraph.GraphNode gnode = procedureNode.GraphNodeList[n];
                interpreter.Push(StackValue.BuildBoolean(gnode.isDirty));
            }

            // Push Params
            formalParameters.Reverse();
            for (int i = 0; i < formalParameters.Count; i++)
            {
                interpreter.Push(formalParameters[i]);
            }

            StackValue svThisPtr = stackFrame.ThisPtr;
            StackValue svBlockDecl = StackValue.BuildBlockIndex(stackFrame.FunctionBlock);

            // Jun: Make sure we have no empty or unaligned frame data
            Validity.Assert(DSASM.StackFrame.kStackFrameSize == stackFrame.Frame.Length);

            // Setup the stack frame data
            //int thisPtr = (int)stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kThisPtr).opdata;
            int ci = activation.classIndex;
            int fi = activation.funcIndex;
            int returnAddr = stackFrame.ReturnPC;
            int blockDecl = stackFrame.FunctionBlock;
            int blockCaller = stackFrame.FunctionCallerBlock;
            int framePointer = runtimeCore.RuntimeMemory.FramePointer; 
            int locals = activation.locals;
            

            // Update the running block to tell the execution engine which set of instruction to execute
            // TODO(Jun/Jiong): Considering store the orig block id to stack frame
            int origRunningBlock = runtimeCore.RunningBlock;
            runtimeCore.RunningBlock = (int)svBlockDecl.opdata;

            // Set SX register 
            interpreter.runtime.SX = svBlockDecl;

            StackFrameType callerType = stackFrame.CallerStackFrameType;

            List<StackValue> registers = new List<DSASM.StackValue>();

            StackValue svCallConvention;
            bool isDispose = CoreUtils.IsDisposeMethod(procedureNode.name);

            bool explicitCall = !c.IsReplicating && !c.IsImplicitCall && !isDispose;
            if (explicitCall)
            {
                svCallConvention = StackValue.BuildCallingConversion((int)ProtoCore.DSASM.CallingConvention.CallType.kExplicit);
            }
            else
            {
                svCallConvention = StackValue.BuildCallingConversion((int)ProtoCore.DSASM.CallingConvention.CallType.kImplicit);                
            }

            stackFrame.TX = svCallConvention;
            interpreter.runtime.TX = svCallConvention;

            // Set SX register 
            stackFrame.SX = svBlockDecl;
            interpreter.runtime.SX = svBlockDecl;

            // TODO Jun:
            // The stackframe carries the current set of registers
            // Determine if this can be done even for the non explicit call implementation
            registers.AddRange(stackFrame.GetRegisters());


            // Comment Jun: the depth is always 0 for a function call as we are reseting this for each function call
            // This is only incremented for every language block bounce
            int depth = stackFrame.Depth;
            DSASM.StackFrameType type = stackFrame.StackFrameType;
            Validity.Assert(depth == 0);
            Validity.Assert(type == DSASM.StackFrameType.kTypeFunction);

            runtimeCore.RuntimeMemory.PushStackFrame(svThisPtr, ci, fi, returnAddr, blockDecl, blockCaller, callerType, type, depth, framePointer, registers, locals, execStateSize);


            StackValue svRet;

            if (explicitCall)
            {
                svRet = ProtoCore.DSASM.StackValue.BuildExplicitCall(activation.pc);
            }
            else
            {
                svRet = interpreter.Run(runtimeCore.RunningBlock, activation.pc, Language.kInvalid, runtimeCore.Breakpoints);
                runtimeCore.RunningBlock = origRunningBlock;
            }

            if (runtimeCore.CurrentExecutive != null)
            {
                runtimeCore.CurrentExecutive.CurrentDSASMExec = oldDSASMExec;
            }
            return svRet; //DSASM.Mirror.ExecutionMirror.Unpack(svRet, core.heap, core);
        }
    }
}
