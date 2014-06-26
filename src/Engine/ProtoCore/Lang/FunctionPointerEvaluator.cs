
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore.DSASM;
using ProtoCore.RuntimeData;
using ProtoCore.Utils;

namespace ProtoCore.Lang
{
    class FunctionPointerEvaluator
    {
        private Interpreter interpreter;
        private ProcedureNode procNode;
        private CallSite callsite;
        public string Name { get { return procNode.name; } }

        public FunctionPointerEvaluator(StackValue pointer, Interpreter dsi)
        {
            Validity.Assert(pointer.IsFunctionPointer);
            interpreter = dsi;
            Core core = dsi.runtime.Core;

            int fptr = (int)pointer.opdata;
            FunctionPointerNode fptrNode;
            int classScope = Constants.kGlobalScope;

            if (core.FunctionPointerTable.functionPointerDictionary.TryGetByFirst(fptr, out fptrNode))
            {
                int blockId = fptrNode.blockId;
                int procId = fptrNode.procId;
                classScope = fptrNode.classScope;
                procNode = dsi.runtime.GetProcedureNode(blockId, classScope, procId);
            }

            callsite = new ProtoCore.CallSite(classScope, Name, core.FunctionTable, core.Options.ExecutionMode);
        }

        public StackValue Evaluate(List<StackValue> args, StackFrame stackFrame)
        {
            // Build the stackframe
            var runtimeCore = interpreter.runtime.Core;

            int classScopeCaller = (int)stackFrame.GetAt(StackFrame.AbsoluteIndex.kClass).opdata;
            int returnAddr = (int)stackFrame.GetAt(StackFrame.AbsoluteIndex.kReturnAddress).opdata;
            int blockDecl = (int)procNode.runtimeIndex;
            int blockCaller = (int)stackFrame.GetAt(StackFrame.AbsoluteIndex.kFunctionCallerBlock).opdata;
            int framePointer = runtimeCore.Rmem.FramePointer;
            StackValue thisPtr = StackValue.BuildPointer(-1);

            // Functoion has variable input parameter. This case only happen
            // for FFI functions whose last parameter's type is (params T[]).
            // In this case, we need to convert argument list from
            //
            //    {a1, a2, ..., am, v1, v2, ..., vn}
            // 
            // to
            // 
            //    {a1, a2, ..., am, {v1, v2, ..., vn}}
            if (procNode.isVarArg)
            {
                int paramCount = procNode.argInfoList.Count;
                Validity.Assert(paramCount >= 1);

                int varParamCount = args.Count - (paramCount - 1);
                var varParams = args.GetRange(paramCount - 1, varParamCount).ToArray();
                args.RemoveRange(paramCount - 1, varParamCount);

                var packedParams = HeapUtils.StoreArray(varParams, null, interpreter.runtime.Core);
                args.Add(packedParams);
            }

            bool isCallingMemberFunciton = procNode.classScope != Constants.kInvalidIndex 
                                           && !procNode.isConstructor 
                                           && !procNode.isStatic;

            bool isValidThisPointer = true;
            if (isCallingMemberFunciton)
            {
                Validity.Assert(args.Count >= 1);
                thisPtr = args[0];
                if (thisPtr.IsArray)
                {
                    isValidThisPointer = ArrayUtils.GetFirstNonArrayStackValue(thisPtr, ref thisPtr, runtimeCore);
                }
                else
                {
                    args.RemoveAt(0);
                }
            }

            if (!isValidThisPointer || (!thisPtr.IsPointer && !thisPtr.IsArray))
            {
                runtimeCore.RuntimeStatus.LogWarning(WarningID.kDereferencingNonPointer,
                                                     WarningMessage.kDeferencingNonPointer);
                return StackValue.Null;
            }

            var callerType = (StackFrameType)stackFrame.GetAt(StackFrame.AbsoluteIndex.kStackFrameType).opdata;
            interpreter.runtime.TX = StackValue.BuildCallingConversion((int)ProtoCore.DSASM.CallingConvention.BounceType.kImplicit);

            StackValue svBlockDecl = StackValue.BuildBlockIndex(blockDecl);
            interpreter.runtime.SX = svBlockDecl;

            var repGuides = new List<List<ProtoCore.ReplicationGuide>>();

            List<StackValue> registers = new List<StackValue>();
            interpreter.runtime.SaveRegisters(registers);
            var newStackFrame = new StackFrame(thisPtr, 
                                               classScopeCaller, 
                                               1, 
                                               returnAddr, 
                                               blockDecl, 
                                               blockCaller, 
                                               callerType, 
                                               StackFrameType.kTypeFunction, 
                                               0,   // depth
                                               framePointer, 
                                               registers, 
                                               null);

            bool isInDebugMode = runtimeCore.Options.IDEDebugMode &&
                                 runtimeCore.ExecMode != InterpreterMode.kExpressionInterpreter;
            if (isInDebugMode)
            {
                runtimeCore.DebugProps.SetUpCallrForDebug(runtimeCore, 
                                                          interpreter.runtime, 
                                                          procNode, 
                                                          returnAddr - 1, 
                                                          false, 
                                                          callsite, 
                                                          args, 
                                                          repGuides, 
                                                          newStackFrame);
            }

            StackValue rx = callsite.JILDispatchViaNewInterpreter(
                                        new Runtime.Context(), 
                                        args, 
                                        repGuides, 
                                        newStackFrame, 
                                        runtimeCore);

            if (isInDebugMode)
            {
                runtimeCore.DebugProps.RestoreCallrForNoBreak(runtimeCore, procNode);
            }

            return rx;
        }

        public static string GetMethodName(StackValue pointer, Interpreter dsi)
        {
            Validity.Assert(pointer.IsFunctionPointer);
            return dsi.runtime.exe.procedureTable[0].procList[(int)pointer.opdata].name;
        }
    }
}
