using System.Collections.Generic;
using ProtoCore.DSASM;
using ProtoCore.Exceptions;
using ProtoCore.Properties;
using ProtoCore.Runtime;
using ProtoCore.Utils;

namespace ProtoCore.Lang
{
    class FunctionPointerEvaluator
    {
        private Interpreter interpreter;
        private ProcedureNode procNode;
        private CallSite callsite;
        public string Name { get { return procNode.Name; } }

        public FunctionPointerEvaluator(StackValue pointer, Interpreter dsi)
        {
            Validity.Assert(pointer.IsFunctionPointer);
            interpreter = dsi;
            RuntimeCore runtimeCore = dsi.runtime.RuntimeCore;

            int fptr = pointer.FunctionPointer;
            FunctionPointerNode fptrNode;
            int classScope = Constants.kGlobalScope;

            if (runtimeCore.DSExecutable.FuncPointerTable.functionPointerDictionary.TryGetByFirst(fptr, out fptrNode))
            {
                int blockId = fptrNode.blockId;
                int procId = fptrNode.procId;
                classScope = fptrNode.classScope;
                procNode = dsi.runtime.GetProcedureNode(blockId, classScope, procId);
            }

            callsite = new ProtoCore.CallSite(classScope, Name, interpreter.runtime.exe.FunctionTable, runtimeCore.Options.ExecutionMode);
        }

        public StackValue Evaluate(List<StackValue> args, StackFrame stackFrame)
        {
            // Build the stackframe
            var runtimeCore = interpreter.runtime.RuntimeCore;

            int classScopeCaller = stackFrame.ClassScope;
            int returnAddr = stackFrame.ReturnPC;
            int blockDecl = procNode.RuntimeIndex;
            int blockCaller = stackFrame.FunctionCallerBlock;
            int framePointer = runtimeCore.RuntimeMemory.FramePointer;
            StackValue thisPtr = StackValue.BuildPointer(Constants.kInvalidIndex);

            // Functoion has variable input parameter. This case only happen
            // for FFI functions whose last parameter's type is (params T[]).
            // In this case, we need to convert argument list from
            //
            //    {a1, a2, ..., am, v1, v2, ..., vn}
            // 
            // to
            // 
            //    {a1, a2, ..., am, {v1, v2, ..., vn}}
            if (procNode.IsVarArg)
            {
                int paramCount = procNode.ArgumentInfos.Count;
                Validity.Assert(paramCount >= 1);

                int varParamCount = args.Count - (paramCount - 1);
                var varParams = args.GetRange(paramCount - 1, varParamCount).ToArray();
                args.RemoveRange(paramCount - 1, varParamCount);

                try
                {
                    var packedParams = interpreter.runtime.rmem.Heap.AllocateArray(varParams);
                    args.Add(packedParams);
                }
                catch (RunOutOfMemoryException)
                {
                    interpreter.runtime.RuntimeCore.RuntimeStatus.LogWarning(WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                    return StackValue.Null;
                }
            }

            bool isCallingMemberFunciton = procNode.ClassID != Constants.kInvalidIndex 
                                           && !procNode.IsConstructor 
                                           && !procNode.IsStatic;

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
                runtimeCore.RuntimeStatus.LogWarning(WarningID.DereferencingNonPointer,
                                              Resources.kDeferencingNonPointer);
                return StackValue.Null;
            }

            var callerType = stackFrame.StackFrameType;
            interpreter.runtime.TX = StackValue.BuildCallingConversion((int)ProtoCore.DSASM.CallingConvention.BounceType.Implicit);

            StackValue svBlockDecl = StackValue.BuildBlockIndex(blockDecl);
            // interpreter.runtime.SX = svBlockDecl;

            List<StackValue> registers = interpreter.runtime.GetRegisters();
            var newStackFrame = new StackFrame(thisPtr, 
                                               classScopeCaller, 
                                               1, 
                                               returnAddr, 
                                               blockDecl, 
                                               blockCaller, 
                                               callerType, 
                                               StackFrameType.Function, 
                                               0,   // depth
                                               framePointer, 
                                               svBlockDecl.BlockIndex,
                                               registers, 
                                               0);

            bool isInDebugMode = runtimeCore.Options.IDEDebugMode &&
                                 runtimeCore.Options.RunMode != InterpreterMode.Expression;
            if (isInDebugMode)
            {
                runtimeCore.DebugProps.SetUpCallrForDebug(
                                                          runtimeCore,
                                                          interpreter.runtime, 
                                                          procNode, 
                                                          returnAddr - 1, 
                                                          false, 
                                                          callsite, 
                                                          args,
                                                          new List<List<ProtoCore.ReplicationGuide>>(), 
                                                          newStackFrame);
            }

            StackValue rx = callsite.JILDispatchViaNewInterpreter(
                                        new Runtime.Context(), 
                                        args,
                                        new List<List<ProtoCore.ReplicationGuide>>(), 
                                        null,
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
            return dsi.runtime.exe.procedureTable[0].Procedures[pointer.FunctionPointer].Name;
        }
    }
}
