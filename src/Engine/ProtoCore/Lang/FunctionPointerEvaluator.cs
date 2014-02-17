
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore.DSASM;
using ProtoCore.Utils;

namespace ProtoCore.Lang
{
    class FunctionPointerEvaluator
    {
        private Interpreter mRunTime;
        private ProcedureNode mProcNode;
        private CallSite mCallSite;
        public string Name { get { return mProcNode.name; } }

        public FunctionPointerEvaluator(StackValue pointer, Interpreter dsi)
        {
            Validity.Assert(pointer.optype == AddressType.FunctionPointer);
            mRunTime = dsi;
            Core core = dsi.runtime.Core;

            int fptr = (int)pointer.opdata;
            FunctionPointerNode fptrNode;
            int classScope = Constants.kGlobalScope;

            if (core.FunctionPointerTable.functionPointerDictionary.TryGetByFirst(fptr, out fptrNode))
            {
                int blockId = fptrNode.blockId;
                int procId = fptrNode.procId;
                classScope = fptrNode.classScope;
                mProcNode = dsi.runtime.GetProcedureNode(blockId, classScope, procId);
            }

            mCallSite = new ProtoCore.CallSite(classScope, Name, core.FunctionTable, core.Options.ExecutionMode);
        }

        public StackValue Evaluate(List<StackValue> args, StackFrame stackFrame)
        {
            //
            // Build the stackframe
            int classScopeCaller = (int)stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kClass).opdata;
            int returnAddr = (int)stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kReturnAddress).opdata;
            int blockDecl = (int)mProcNode.runtimeIndex;
            int blockCaller = (int)stackFrame.GetAt(DSASM.StackFrame.AbsoluteIndex.kFunctionCallerBlock).opdata;
            int framePointer = mRunTime.runtime.Core.Rmem.FramePointer;
            StackValue thisPtr = StackValue.BuildPointer(-1);


            // Comment Jun: the caller type is the current type in the stackframe
            StackFrameType callerType = (StackFrameType)stackFrame.GetAt(StackFrame.AbsoluteIndex.kStackFrameType).opdata;

            StackFrameType type = StackFrameType.kTypeFunction;
            int depth = 0;
            List<StackValue> registers = new List<StackValue>();

            // Comment Jun: Calling convention data is stored on the TX register
            StackValue svCallconvention = StackValue.BuildCallingConversion((int)ProtoCore.DSASM.CallingConvention.BounceType.kImplicit);
            mRunTime.runtime.TX = svCallconvention;

            StackValue svBlockDecl = StackValue.BuildBlockIndex(blockDecl);
            mRunTime.runtime.SX = svBlockDecl;

            mRunTime.runtime.SaveRegisters(registers);
            ProtoCore.DSASM.StackFrame newStackFrame = new StackFrame(thisPtr, classScopeCaller, 1, returnAddr, blockDecl, blockCaller, callerType, type, depth, framePointer, registers, null);

            List<List<int>> replicationGuides = new List<List<int>>();
            if (mRunTime.runtime.Core.Options.IDEDebugMode && mRunTime.runtime.Core.ExecMode != ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter)
            {
                mRunTime.runtime.Core.DebugProps.SetUpCallrForDebug(mRunTime.runtime.Core, mRunTime.runtime, mProcNode, returnAddr - 1,
                    false, mCallSite, args, replicationGuides, newStackFrame);
            }


            StackValue rx = mCallSite.JILDispatchViaNewInterpreter(new Runtime.Context(), args, replicationGuides, newStackFrame, mRunTime.runtime.Core);

            if (mRunTime.runtime.Core.Options.IDEDebugMode && mRunTime.runtime.Core.ExecMode != ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter)
            {
                mRunTime.runtime.Core.DebugProps.RestoreCallrForNoBreak(mRunTime.runtime.Core, mProcNode);
            }

            return rx;
        }

        public static string GetMethodName(StackValue pointer, Interpreter dsi)
        {
            Validity.Assert(pointer.optype == AddressType.FunctionPointer);
            return dsi.runtime.exe.procedureTable[0].procList[(int)pointer.opdata].name;
        }
    }
}
