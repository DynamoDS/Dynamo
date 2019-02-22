using System;
using System.IO;
using System.Collections.Generic;
using ProtoCore.DSASM;
using ProtoCore.Lang.Replication;
using ProtoCore.Utils;
using System.Linq;
using Autodesk.DesignScript.Interfaces;
using ProtoFFI;
using ProtoCore.Runtime;
using ProtoCore.Properties;
using ProtoCore.Exceptions;

namespace ProtoCore.Lang
{
    public class BuiltInFunctionEndPoint : FunctionEndPoint
    {
        private readonly ProtoCore.Lang.BuiltInMethods.MethodID buildInMethodId;
        public BuiltInFunctionEndPoint(ProtoCore.Lang.BuiltInMethods.MethodID id)
        {
            buildInMethodId = id;
        }

        public override bool DoesPredicateMatch(ProtoCore.Runtime.Context c, List<StackValue> formalParameters, List<ReplicationInstruction> replicationInstructions)
        {
            return true;
        }


        public override StackValue Execute(ProtoCore.Runtime.Context c, List<StackValue> formalParameters, ProtoCore.DSASM.StackFrame stackFrame, RuntimeCore runtimeCore)
        {
            RuntimeMemory rmem = runtimeCore.RuntimeMemory;
            ProtoCore.DSASM.Interpreter interpreter = new DSASM.Interpreter(runtimeCore);
            StackValue ret;

            switch (buildInMethodId)
            {
                case ProtoCore.Lang.BuiltInMethods.MethodID.Count:
                    {
                        if (!formalParameters[0].IsArray)
                            ret = ProtoCore.DSASM.StackValue.BuildInt(1);
                        else
                            ret = ProtoCore.DSASM.StackValue.BuildInt(ArrayUtilsForBuiltIns.Count(formalParameters[0], interpreter));
                        break;
                    }
                case ProtoCore.Lang.BuiltInMethods.MethodID.Rank:
                    {
                        ret = ProtoCore.DSASM.StackValue.BuildInt(ArrayUtilsForBuiltIns.Rank(formalParameters[0], interpreter));
                        break;
                    }
                case ProtoCore.Lang.BuiltInMethods.MethodID.Concat:
                    ret = ArrayUtilsForBuiltIns.Concat(formalParameters[0], formalParameters[1], interpreter);
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.RangeExpression:
                    try
                    {
                        ret = RangeExpressionUtils.RangeExpression(formalParameters[0],
                                                                    formalParameters[1],
                                                                    formalParameters[2],
                                                                    formalParameters[3],
                                                                    formalParameters[4],
                                                                    formalParameters[5],
                                                                    runtimeCore);
                    }
                    catch (OutOfMemoryException)
                    {
                        runtimeCore.RuntimeStatus.LogWarning(WarningID.RangeExpressionOutOfMemory, Resources.RangeExpressionOutOfMemory);
                        ret = StackValue.Null;
                    }
                    break;
                case BuiltInMethods.MethodID.Sleep:
                    {
                        StackValue stackValue = formalParameters[0];
                        if (stackValue.IsInteger)
                            System.Threading.Thread.Sleep((int)stackValue.IntegerValue);
                        else
                        {
                            runtimeCore.RuntimeStatus.LogWarning(
                                Runtime.WarningID.InvalidArguments,
                                Resources.kInvalidArguments);
                        }

                        ret = DSASM.StackValue.Null;
                        break;
                    }
                case ProtoCore.Lang.BuiltInMethods.MethodID.RemoveIfNot:
                    ret = ArrayUtilsForBuiltIns.RemoveIfNot(formalParameters[0], formalParameters[1], interpreter);
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.Equals:
                    ret = ArrayUtilsForBuiltIns.Equals(formalParameters[0], formalParameters[1], interpreter, c);
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.Transpose:
                    ret = ArrayUtilsForBuiltIns.Transpose(formalParameters[0], interpreter);
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.InlineConditional:
                    {
                        StackValue svCondition = formalParameters[0];
                        if (!svCondition.IsBoolean)
                        {
                            // Comment Jun: Perhaps we can allow coercion?
                            Type booleanType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Bool, 0);
                            svCondition = TypeSystem.Coerce(svCondition, booleanType, runtimeCore);
                            if (svCondition.IsNull)
                            {
                                svCondition = StackValue.False;
                            }
                        }

                        StackValue svTrue = formalParameters[1];
                        StackValue svFalse = formalParameters[2];

                        // If run in delta execution environment, we don't 
                        // create language blocks for true and false branch, 
                        // so directly return the value.
                        if (runtimeCore.Options.GenerateSSA)
                            return svCondition.BooleanValue ? svTrue : svFalse;

                        Validity.Assert(svTrue.IsInteger);
                        Validity.Assert(svFalse.IsInteger);
                        int blockId = svCondition.BooleanValue ? (int)svTrue.IntegerValue : (int)svFalse.IntegerValue;
                        int oldRunningBlockId = runtimeCore.RunningBlock;
                        runtimeCore.RunningBlock = blockId;

                        int returnAddr = stackFrame.ReturnPC;

                        int ci = ProtoCore.DSASM.Constants.kInvalidIndex;
                        int fi = ProtoCore.DSASM.Constants.kInvalidIndex;
                        if (interpreter.runtime.rmem.Stack.Count >= ProtoCore.DSASM.StackFrame.StackFrameSize)
                        {
                            ci = stackFrame.ClassScope;
                            fi = stackFrame.FunctionScope;
                        }

                        // The class scope does not change for inline conditional calls
                        StackValue svThisPtr = stackFrame.ThisPtr;


                        int blockDecl = 0;
                        int blockCaller = oldRunningBlockId;
                        StackFrameType type = StackFrameType.LanguageBlock;
                        int depth = (int)interpreter.runtime.rmem.GetAtRelative(StackFrame.FrameIndexStackFrameDepth).IntegerValue;
                        int framePointer = rmem.FramePointer;

                        // Comment Jun: Calling convention data is stored on the TX register
                        StackValue svCallconvention = StackValue.BuildCallingConversion((int)ProtoCore.DSASM.CallingConvention.BounceType.Implicit);
                        interpreter.runtime.TX = svCallconvention;

                        List<StackValue> registers = interpreter.runtime.GetRegisters();

                        // Comment Jun: the caller type is the current type in the stackframe
                        StackFrameType callerType = stackFrame.StackFrameType;

                        
                        blockCaller = runtimeCore.DebugProps.CurrentBlockId;
                        StackFrame bounceStackFrame = new StackFrame(svThisPtr, ci, fi, returnAddr, blockDecl, blockCaller, callerType, type, depth, framePointer, 0, registers, 0);

                        ret = interpreter.runtime.Bounce(blockId, 0, bounceStackFrame, 0, false, runtimeCore.CurrentExecutive.CurrentDSASMExec, runtimeCore.Breakpoints);

                        runtimeCore.RunningBlock = oldRunningBlockId;
                        break;
                    }

                case ProtoCore.Lang.BuiltInMethods.MethodID.Dot:
                    ret = DotMethod(formalParameters[0], stackFrame, interpreter.runtime, c);
                    break;

                case BuiltInMethods.MethodID.GetType:
                    AddressType objType = formalParameters[0].optype;
                    int typeUID = (int)PrimitiveType.InvalidType;

                    switch (objType)
                    {
                        case AddressType.Invalid:
                            typeUID = (int)PrimitiveType.InvalidType;
                            break;
                        case AddressType.VarIndex:
                        case AddressType.FunctionIndex:
                        case AddressType.MemVarIndex:
                        case AddressType.StaticMemVarIndex:
                        case AddressType.ClassIndex:
                        case AddressType.LabelIndex:
                        case AddressType.BlockIndex:
                        case AddressType.ArrayDim:
                        case AddressType.ReplicationGuide:
                        case AddressType.Int:
                            typeUID = (int)PrimitiveType.Integer;
                            break;
                        case AddressType.Double:
                            typeUID = (int)PrimitiveType.Double;
                            break;
                        case AddressType.Boolean:
                            typeUID = (int)PrimitiveType.Bool;
                            break;
                        case AddressType.Char:
                            typeUID = (int)PrimitiveType.Char;
                            break;
                        case AddressType.String:
                            typeUID = (int)PrimitiveType.String;
                            break;
                        case AddressType.Pointer:
                            typeUID = (int)PrimitiveType.Pointer;
                            break;
                        case AddressType.ArrayPointer:
                            typeUID = (int)PrimitiveType.Array;
                            break;
                        case AddressType.FunctionPointer:
                            typeUID = (int)PrimitiveType.FunctionPointer;
                            break;
                        case AddressType.Null:
                            typeUID = (int)PrimitiveType.Null;
                            break;
                        default:
                            typeUID = formalParameters[0].metaData.type;
                            break;
                    }
                    return StackValue.BuildInt(typeUID);
                case BuiltInMethods.MethodID.ImportData:
                    ret = ContextDataBuiltIns.ImportData(formalParameters[0], formalParameters[1], runtimeCore, interpreter, c);
                    break;
                case BuiltInMethods.MethodID.Break:
                    {
                        DebuggerBuiltIns.Break(interpreter, stackFrame);
                        ret = StackValue.Null;
                        break;
                    }
                case BuiltInMethods.MethodID.Evaluate:
                    ret = ArrayUtilsForBuiltIns.Evaluate(
                        formalParameters[0], 
                        formalParameters[1], 
                        formalParameters[2],
                        interpreter, 
                        stackFrame);
                    break;
                case BuiltInMethods.MethodID.NodeAstFailed:
                    var nodeFullName = formalParameters[0];
                    var fullName = StringUtils.GetStringValue(nodeFullName, runtimeCore);
                    ret = StackValue.Null;
                    break;
                case BuiltInMethods.MethodID.GC:
                    var gcRoots = interpreter.runtime.RuntimeCore.CurrentExecutive.CurrentDSASMExec.CollectGCRoots();
                    rmem.Heap.FullGC(gcRoots, interpreter.runtime);
                    ret = StackValue.Null;
                    break;
                default:
                    throw new ProtoCore.Exceptions.CompilerInternalException("Unknown built-in method. {AAFAE85A-2AEB-4E8C-90D1-BCC83F27C852}");
            }

            return ret;
        }

        private StackValue DotMethod(StackValue lhs, StackFrame stackFrame, DSASM.Executive runtime, Context context)
        {
            var runtimeCore = runtime.RuntimeCore;
            var rmem = runtime.rmem;
            var runtimeData = runtimeCore.RuntimeData;

            bool isValidThisPointer = true;
            StackValue thisObject = lhs;
            if (thisObject.IsArray)
            {
                isValidThisPointer = ArrayUtils.GetFirstNonArrayStackValue(lhs, ref thisObject, runtimeCore);
            }

            bool isInvalidDotCall = !isValidThisPointer || (!thisObject.IsPointer && !thisObject.IsArray);
            int stackPtr = rmem.Stack.Count - 1;

            // TODO Jun: Consider having a DynamicFunction AddressType
            StackValue dynamicTableIndex = rmem.Stack[stackPtr - 4];
            Validity.Assert(dynamicTableIndex.IsInteger);

            StackValue dimensions = rmem.Stack[stackPtr - 3];
            Validity.Assert(dimensions.IsArray);

            StackValue dimensionCount = rmem.Stack[stackPtr - 2];
            Validity.Assert(dimensionCount.IsInteger);

            StackValue functionArguments = rmem.Stack[stackPtr - 1];
            Validity.Assert(functionArguments.IsArray);

            StackValue argumentCount = rmem.Stack[stackPtr];
            Validity.Assert(argumentCount.IsInteger);
            int functionArgs = (int)argumentCount.IntegerValue;

            // Build the function arguments
            var argArray = rmem.Heap.ToHeapObject<DSArray>(functionArguments);
            var arguments = argArray.Values.ToList();

            bool removeFirstArgument = false;
            if (arguments.Count > 0)
            {
                bool isReplicatingCall = arguments[0].IsDynamic && lhs.IsArray;
                if (isReplicatingCall)
                {
                    arguments[0] = lhs;
                    context.IsReplicating = true;
                }
                else if (!arguments[0].IsDefaultArgument)
                {
                    context.IsReplicating = false;
                    arguments.RemoveAt(0);
                    removeFirstArgument = true;
                }
            }

            // Find the first visible method in the class and its heirarchy
            // The callsite will handle the overload
            var dynamicFunction = runtimeCore.DSExecutable.DynamicFuncTable.GetFunctionAtIndex((int)dynamicTableIndex.IntegerValue);
            string functionName = dynamicFunction.Name;

            var replicationGuides = new List<List<ProtoCore.ReplicationGuide>>();
            var atLevels = new List<AtLevel>();
            if (!CoreUtils.IsGetterSetter(functionName))
            {
                replicationGuides = runtime.GetCachedReplicationGuides(functionArgs);
                atLevels = runtime.GetCachedAtLevels(functionArgs);

                if (removeFirstArgument)
                {
                    replicationGuides.RemoveAt(0);
                    atLevels.RemoveAt(0);
                }
            }

            if (isInvalidDotCall)
            {
                if (ArrayUtils.IsEmpty(lhs, runtimeCore))
                {
                    return lhs;
                }
                else
                {
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.DereferencingNonPointer, Resources.kDeferencingNonPointer);
                    return StackValue.Null;
                }
            }

            int thisObjectType = thisObject.metaData.type;
            ClassNode classNode = runtime.exe.classTable.ClassNodes[thisObjectType];
            ProcedureNode procNode = classNode.ProcTable.GetFunctionsByName(functionName).FirstOrDefault();

            // Trace hierarchy chain to find out the procedure node.
            if (procNode == null)
            {
                var currentClassNode = classNode;
                if (currentClassNode.Base != Constants.kInvalidIndex)
                {
                    int baseCI = currentClassNode.Base;
                    currentClassNode = runtime.exe.classTable.ClassNodes[baseCI];
                    procNode = currentClassNode.ProcTable.GetFunctionsByName(functionName).FirstOrDefault();
                }
            }

            int procIndex = Constants.kInvalidIndex;
            // If the function still isn't found, then it may be a function 
            // pointer. 
            if (procNode == null)
            {
                int memvarIndex = classNode.GetFirstVisibleSymbolNoAccessCheck(dynamicFunction.Name);

                if (Constants.kInvalidIndex != memvarIndex)
                {
                    var obj = rmem.Heap.ToHeapObject<DSObject>(thisObject);
                    StackValue svMemberPtr = obj.GetValueFromIndex(memvarIndex, runtimeCore);
                    if (svMemberPtr.IsPointer)
                    {
                        StackValue svFunctionPtr = rmem.Heap.ToHeapObject<DSObject>(svMemberPtr).GetValueFromIndex(0, runtimeCore);
                        if (svFunctionPtr.IsFunctionPointer)
                        {
                            // It is a function pointer
                            // Functions pointed to are all in the global scope
                            thisObjectType = ProtoCore.DSASM.Constants.kGlobalScope;
                            procIndex = svFunctionPtr.FunctionPointer;
                            procNode = runtime.exe.procedureTable[0].Procedures[procIndex];
                            functionName = procNode.Name;
                        }
                    }
                }
            }

            // Build the stackframe
            var newStackFrame = new StackFrame(thisObject, 
                                               stackFrame.ClassScope, 
                                               procNode == null ? procIndex : procNode.ID, 
                                               stackFrame.ReturnPC, 
                                               stackFrame.FunctionBlock, 
                                               stackFrame.FunctionCallerBlock, 
                                               stackFrame.StackFrameType,
                                               StackFrameType.Function, 
                                               0,
                                               rmem.FramePointer, 
                                               0,
                                               stackFrame.GetRegisters(), 
                                               0);

            ProtoCore.CallSite callsite = runtimeData.GetCallSite( thisObjectType, functionName, runtime.exe, runtimeCore);
            Validity.Assert(null != callsite);

            // TODO: Disabling support for stepping into replicated function calls temporarily - pratapa
            if (runtimeCore.Options.IDEDebugMode &&
                runtimeCore.Options.RunMode != InterpreterMode.Expression &&
                procNode != null)
            {
                runtimeCore.DebugProps.SetUpCallrForDebug(
                                                   runtimeCore,
                                                   runtimeCore.CurrentExecutive.CurrentDSASMExec,
                                                   procNode,
                                                   stackFrame.ReturnPC - 1,
                                                   false, callsite,
                                                   arguments,
                                                   replicationGuides,
                                                   newStackFrame,
                                                   null,
                                                   false,
                                                   true,
                                                   thisObject);
            }

            var argumentAtLevels = AtLevelHandler.GetArgumentAtLevelStructure(arguments, atLevels, runtimeCore);
            argumentAtLevels.Arguments.ForEach(x => runtimeCore.AddCallSiteGCRoot(callsite.CallSiteID, x));
            StackValue ret = callsite.JILDispatchViaNewInterpreter(context, argumentAtLevels.Arguments, replicationGuides, argumentAtLevels.DominantStructure, newStackFrame, runtimeCore);
            runtimeCore.RemoveCallSiteGCRoot(callsite.CallSiteID);

            // Restore debug properties after returning from a CALL/CALLR
            if (runtimeCore.Options.IDEDebugMode &&
                runtimeCore.Options.RunMode != InterpreterMode.Expression &&
                procNode != null)
            {
                runtimeCore.DebugProps.RestoreCallrForNoBreak(runtimeCore, procNode);
            }

            return ret;
        }
    }

    internal class ContextDataBuiltIns
    {
        internal static StackValue ImportData(StackValue svAppName, StackValue svConnectionParameters, RuntimeCore runtimeCore, Interpreter interpreter, ProtoCore.Runtime.Context c)
        {
            string appname = StringUtils.GetStringValue(svAppName, runtimeCore);

            IContextDataProvider provider = runtimeCore.DSExecutable.ContextDataMngr.GetDataProvider(appname);
            ProtoCore.Utils.Validity.Assert(null != provider, string.Format("Couldn't locate data provider for {0}", appname));

            CLRObjectMarshaler marshaler = CLRObjectMarshaler.GetInstance(runtimeCore);

            Dictionary<string, Object> parameters = new Dictionary<string, object>();
            if (!svConnectionParameters.IsArray)
            {
                Object connectionParameters = marshaler.UnMarshal(svConnectionParameters, c, interpreter, typeof(Object));
                parameters.Add("data", connectionParameters);
            }
            else
            {
                var heap = interpreter.runtime.RuntimeCore.Heap;
                StackValue[] svArray = heap.ToHeapObject<DSArray>(svConnectionParameters).Values.ToArray();
                ProtoCore.Utils.Validity.Assert(svArray.Length % 2 == 0, string.Format("Connection parameters for ImportData should be array of Parameter Name followed by value"));
                int nParameters = svArray.Length / 2;
                for (int i = 0; i < nParameters; ++i)
                {
                    string paramName = StringUtils.GetStringValue(svArray[2 * i], runtimeCore);
                    Object paramData = marshaler.UnMarshal(svArray[2 * i + 1], c, interpreter, typeof(Object));
                    parameters.Add(paramName, paramData);
                }
            }

            IContextData[] data = provider.ImportData(parameters);
            if (null == data)
                return StackValue.Null;

            List<Object> objects = new List<Object>();
            foreach (var item in data)
            {
                objects.Add(item.Data);
            }
            ProtoCore.Type type = PrimitiveMarshaler.CreateType(ProtoCore.PrimitiveType.Var);
            Object obj = objects;
            if (objects.Count == 1)
                obj = objects[0];
            else
            {
                type.rank = Constants.kArbitraryRank;
            }

            StackValue ret = marshaler.Marshal(obj, c, interpreter, type);
            return ret;
        }
    }

    internal class DebuggerBuiltIns
    {
        // set a breakpoint at the next breakable instruction
        internal static void Break(Interpreter interpreter, StackFrame stackFrame)
        {
            RuntimeCore runtimeCore = interpreter.runtime.RuntimeCore;
            if (!runtimeCore.Options.IDEDebugMode)
                return;

            if (runtimeCore.DebugProps.DebugStackFrameContains(DebugProperties.StackFrameFlagOptions.IsReplicating))
                return;

            // Search for next breakable instruction in list of instructions and add to RegisteredBreakPoints

            int pc = stackFrame.ReturnPC;
            int blockId = stackFrame.FunctionCallerBlock;
            List<Instruction> instructions = runtimeCore.DSExecutable.instrStreamList[blockId].instrList;

            // Search instructions from DebugEntryPC onwards for the next breakpoint and add it to current list of breakpoints
            // if there is a bounce, then jump to new lang block and continue searching
            while (pc < instructions.Count)
            {
                if (instructions[pc].debug != null)
                {
                    if(!runtimeCore.Breakpoints.Contains(instructions[pc]))
                        runtimeCore.Breakpoints.Add(instructions[pc]);
                    break;
                }
                else if (instructions[pc].opCode == OpCode.BOUNCE)
                {
                    blockId = instructions[pc].op1.BlockIndex;
                    instructions = runtimeCore.DSExecutable.instrStreamList[blockId].instrList;
                    pc = 0;
                    continue;
                }
                pc++;
            }

        }
    }
    internal class FileIOBuiltIns
    {
        //This Function is to Restore a String Type StackValue to a String
        internal static string ConvertToString(StackValue st, ProtoCore.DSASM.Interpreter runtime)
        {
            string result = "";
            if (!st.IsString) 
                return result;

            result = runtime.runtime.rmem.Heap.ToHeapObject<DSString>(st).Value;
            result.Replace("\\\\", "\\");
            return result;
        }
        
    }
    
    internal class RangeExpressionUtils
    {
        // For to include start and end. 
        internal static StackValue[] GenerateRangeByStepNumber(decimal start, decimal end, int stepnum, bool isIntRange)
        {
            decimal stepsize = (stepnum == 1) ? 0 : (end - start) / (stepnum - 1);
            isIntRange = isIntRange && (Math.Truncate(stepsize) == stepsize);

            StackValue[] range = new StackValue[stepnum > 1 ? stepnum : 1];
            range[0] = isIntRange ? StackValue.BuildInt((int)start) : StackValue.BuildDouble((double)start);

            decimal cur = start;
            for (int i = 1; i < stepnum - 1; ++i)
            {
                cur += stepsize;
                range[i] = isIntRange ? StackValue.BuildInt((int)cur) : StackValue.BuildDouble((double)cur);
            }

            if (stepnum > 1)
            {
                range[(int)stepnum - 1] = isIntRange ? StackValue.BuildInt((int)end) : StackValue.BuildDouble((double)end);
            }

            return range;
        }

        internal static StackValue RangeExpression(
            StackValue svStart,
            StackValue svEnd,
            StackValue svStep,
            StackValue svOp,
            StackValue svHasStep,
            StackValue svHasAmountOp,
            RuntimeCore runtimeCore)
        {
            bool hasStep = svHasStep.IsBoolean && svHasStep.BooleanValue;
            bool hasAmountOp = svHasAmountOp.IsBoolean && svHasAmountOp.BooleanValue;

            // If start parameter is not the same as end parameter, show warning.
            // If start parameter is not number/string and there is no amount op, show warning.
            if (!((svStart.IsNumeric && svEnd.IsNumeric) || (svStart.IsString && svEnd.IsString)) && (!hasAmountOp))
            {
                runtimeCore.RuntimeStatus.LogWarning(WarningID.InvalidArguments, Resources.kInvalidArgumentsInRangeExpression);
                return StackValue.Null;
            }

            if (hasAmountOp)
            {
                if (!svEnd.IsNumeric)
                {
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.InvalidArguments, Resources.kInvalidAmountInRangeExpression);
                    return StackValue.Null;
                }
                if (!hasStep)
                {
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.InvalidArguments, Resources.kNoStepSizeInAmountRangeExpression);
                    return StackValue.Null;
                }
            }

            if ((svStep.IsNull && hasStep) || (!svStep.IsNull && !svStep.IsNumeric))
            {
                runtimeCore.RuntimeStatus.LogWarning(WarningID.InvalidArguments, Resources.kInvalidArgumentsInRangeExpression);
                return StackValue.Null;
            }

            StackValue[] range = null;

            if (svStart.IsNumeric)
            {
                range = GenerateNumericRange(svStart, svEnd, svStep, svOp, hasStep, hasAmountOp, runtimeCore);
            }
            else
            {
                if (svStart.IsString && !hasAmountOp)
                {
                    range = GenerateAlphabetRange(svStart, svEnd, svStep, runtimeCore);
                }
                else if (svStart.IsString && hasAmountOp)
                {
                    range = GenerateAlphabetSequence(svStart, svEnd, svStep, svOp, runtimeCore);
                }
            }

            try
            {
                return range == null ? StackValue.Null : runtimeCore.RuntimeMemory.Heap.AllocateArray(range);
            }
            catch (RunOutOfMemoryException)
            {
                runtimeCore.RuntimeStatus.LogWarning(WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                return StackValue.Null;
            }
        }

        private static StackValue[] GenerateNumericRange(
            StackValue svStart,
            StackValue svEnd,
            StackValue svStep,
            StackValue svOp,
            bool hasStep,
            bool hasAmountOp,
            RuntimeCore runtimeCore)
        {
            double startValue = svStart.ToDouble().DoubleValue;
            double endValue = svEnd.ToDouble().DoubleValue;

            if (double.IsInfinity(startValue) || double.IsNaN(startValue) ||
                double.IsInfinity(endValue) || double.IsNaN(endValue) ||
                svStep.IsDouble && (double.IsInfinity(svStep.DoubleValue) || double.IsNaN(svStep.DoubleValue)))
            {
                runtimeCore.RuntimeStatus.LogWarning(WarningID.InvalidArguments, Resources.kInvalidArgumentsInRangeExpression);
                return null;
            }

            bool isIntRange = svStart.IsInteger && svEnd.IsInteger;

            if (hasAmountOp)
            {
                long amount = svEnd.ToInteger().IntegerValue;
                if (amount < 0)
                {
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.InvalidArguments, Resources.kInvalidAmountInRangeExpression);
                    return null;
                }

                if (amount == 0)
                {
                    return new StackValue[] { };
                }
                else
                {
                    double stepsize = svStep.ToDouble().DoubleValue;
                    isIntRange = isIntRange && (Math.Truncate(stepsize) == stepsize);
                    StackValue[] range = new StackValue[amount];
                    for (int i = 0; i < amount; ++i)
                    {
                        range[i] = isIntRange ? StackValue.BuildInt((int)startValue) : StackValue.BuildDouble(startValue);
                        startValue += stepsize;
                    }
                    return range;
                }
            }
            else
            {
                decimal start = new decimal(startValue);
                decimal end = new decimal(endValue);

                switch (svOp.IntegerValue)
                {
                    case (int)RangeStepOperator.StepSize:
                        {
                            decimal stepsize = (start > end) ? -1 : 1;
                            if (hasStep)
                            {
                                stepsize = new decimal(svStep.IsDouble ? svStep.DoubleValue: svStep.IntegerValue);
                                isIntRange = isIntRange && (svStep.IsInteger);
                            }

                            if ((stepsize == 0) || (end > start && stepsize < 0) || (end < start && stepsize > 0))
                            {
                                return null;
                            }

                            decimal stepnum = Math.Truncate((end - start) / stepsize) + 1;
                            if (stepnum > int.MaxValue)
                            {
                                runtimeCore.RuntimeStatus.LogWarning(WarningID.RangeExpressionOutOfMemory, Resources.RangeExpressionOutOfMemory);
                                return null;
                            }
                            StackValue[] range = new StackValue[(int)stepnum];

                            decimal cur = start;
                            for (int i = 0; i < (int)stepnum; ++i)
                            {
                                range[i] = isIntRange ? StackValue.BuildInt((int)cur) : StackValue.BuildDouble((double)cur);
                                cur += stepsize;
                            }
                            return range;
                        }
                    case (int)RangeStepOperator.Number:
                        {
                            decimal stepnum = new decimal(Math.Round(svStep.IsDouble ? svStep.DoubleValue: svStep.IntegerValue));
                            if (stepnum <= 0)
                            {
                                return null;
                            }
                            else if (stepnum > int.MaxValue)
                            {
                                runtimeCore.RuntimeStatus.LogWarning(WarningID.RangeExpressionOutOfMemory, Resources.RangeExpressionOutOfMemory);
                                return null;
                            }

                            return GenerateRangeByStepNumber(start, end, (int)stepnum, isIntRange);
                        }
                    case (int)RangeStepOperator.ApproximateSize:
                        {
                            decimal astepsize = new decimal(svStep.IsDouble ? svStep.DoubleValue : svStep.IntegerValue);
                            if (astepsize == 0)
                            {
                                return null;
                            }

                            decimal dist = end - start;
                            decimal stepnum = 1;
                            if (dist != 0)
                            {
                                decimal cstepnum = Math.Ceiling(dist / astepsize);
                                decimal fstepnum = Math.Floor(dist / astepsize);

                                if (cstepnum == 0 || fstepnum == 0)
                                {
                                    stepnum = 2;
                                }
                                else
                                {
                                    decimal capprox = Math.Abs(dist / cstepnum - astepsize);
                                    decimal fapprox = Math.Abs(dist / fstepnum - astepsize);
                                    stepnum = capprox < fapprox ? cstepnum + 1 : fstepnum + 1;
                                }
                            }

                            if (stepnum > int.MaxValue)
                            {
                                runtimeCore.RuntimeStatus.LogWarning(WarningID.RangeExpressionOutOfMemory, Resources.RangeExpressionOutOfMemory);
                                return null;
                            }

                            return GenerateRangeByStepNumber(start, end, (int)stepnum, isIntRange);
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            return null;
        }


        private static StackValue[] GenerateAlphabetRange(StackValue svStart, StackValue svEnd, StackValue svStep, RuntimeCore runtimeCore)
        {
            if (!svStart.IsString || !svEnd.IsString)
            {
                runtimeCore.RuntimeStatus.LogWarning(
                    WarningID.InvalidArguments,
                    Resources.kInvalidArgumentsInRangeExpression);
                return null;
            }

            var startValue = runtimeCore.Heap.ToHeapObject<DSString>(svStart).Value;
            var endValue = runtimeCore.Heap.ToHeapObject<DSString>(svEnd).Value;

            // Start and end values can be just alphabet letters. So their length can't be more than 1.
            if (startValue.Length != 1 || endValue.Length != 1)
            {
                runtimeCore.RuntimeStatus.LogWarning(
                    WarningID.InvalidArguments,
                    Resources.kInvalidArgumentsInRangeExpression);
                return null;
            }

            var startLetter = startValue.ToCharArray().First();
            var endLetter = endValue.ToCharArray().First();
            int step = svStep.IsNull ? 1 : Convert.ToInt32(svStep.RawData);

            // Alphabet sequence can be made just from letters (that are not unicode).
            if (!Char.IsLetter(startLetter) || !Char.IsLetter(endLetter) || step <= 0 ||
                startLetter > Byte.MaxValue || endLetter > Byte.MaxValue)
            {
                runtimeCore.RuntimeStatus.LogWarning(
                    WarningID.InvalidArguments,
                    Resources.kInvalidArgumentsInRangeExpression);
                return null;
            }

            StackValue[] letters;
            int stepOffset = (startLetter < endLetter) ? 1 : -1;
            int stepnum = (int)Math.Abs(Math.Truncate((endLetter - startLetter) / (double)step)) + 1;

            letters = Enumerable.Range(1, stepnum)
                // Generate arithmetic progression.
                 .Select(x => startLetter + (x - 1) * step * stepOffset)
                // Take just letters.
                 .Where(x => Char.IsLetter((char)x))
                // Create stack values.
                 .Select(x => StackValue.BuildString(Char.ToString((char)x), runtimeCore.Heap))
                 .ToArray();

            return letters;
        }

        private static StackValue[] GenerateAlphabetSequence(StackValue svStart, StackValue svEnd, StackValue svStep, StackValue svOp, RuntimeCore runtimeCore)
        {
            if (!svStart.IsString)
            {
                runtimeCore.RuntimeStatus.LogWarning(
                    WarningID.InvalidArguments,
                    Resources.kInvalidArgumentsInRangeExpression);
                return null;
            }
            if (!svEnd.IsInteger)
            {
                runtimeCore.RuntimeStatus.LogWarning(
                   WarningID.InvalidArguments,
                   Resources.kInvalidAmountInRangeExpression);
                return null;
            }
            if (!svStep.IsInteger)
            {
                runtimeCore.RuntimeStatus.LogWarning(
                   WarningID.InvalidArguments,
                   Resources.kRangeExpressionWithNonIntegerStepNumber);
                return null;
            }

            var startValue = runtimeCore.Heap.ToHeapObject<DSString>(svStart).Value;
            var amount = svEnd.IntegerValue;
            var step = svStep.IntegerValue;

            // Start value can be just alphabet letter. So its length can't be more than 1.
            // End value must be int. (we checked it before)
            if (startValue.Length != 1)
            {
                runtimeCore.RuntimeStatus.LogWarning(
                    WarningID.InvalidArguments,
                    Resources.kInvalidStringArgumentInRangeExpression);
                return null;
            }

            if (amount < 0)
            {
                runtimeCore.RuntimeStatus.LogWarning(
                   WarningID.InvalidArguments,
                   Resources.kInvalidAmountInRangeExpression);
                return null;
            }

            var startLetter = startValue.ToCharArray().First();

            // Alphabet sequence can be made just from letters,
            // that are not unicode, i.e. their code is less than 255.
            if (!Char.IsLetter(startLetter) || startLetter > Byte.MaxValue)
            {
                runtimeCore.RuntimeStatus.LogWarning(
                    WarningID.InvalidArguments,
                    Resources.kInvalidUnicodeArgumentInRangeExpression);
                return null;
            }

            List<StackValue> letters = new List<StackValue>();
            for (int i = 0; i < amount; i++)
            {
                if (Char.IsLetter(startLetter))
                {
                    letters.Add(StackValue.BuildString(Char.ToString(startLetter), runtimeCore.Heap));
                }

                startLetter = (char)(startLetter + step);
            }

            return letters.ToArray();
        }
    }
    internal class ArrayUtilsForBuiltIns
    {
        internal static int Count(StackValue sv, ProtoCore.DSASM.Interpreter runtime)
        {
            if (!sv.IsArray)
                return ProtoCore.DSASM.Constants.kInvalidIndex;

            DSArray array = runtime.runtime.rmem.Heap.ToHeapObject<DSArray>(sv);
            return array.Count;
        }

        internal static int Rank(StackValue sv, ProtoCore.DSASM.Interpreter runtime)
        {
            if (!sv.IsArray)
                return 0;

            var values = runtime.runtime.RuntimeCore.Heap.ToHeapObject<DSArray>(sv).Values;
            if (values.Any())
            {
                return values.Select(x => x.IsArray ? Rank(x, runtime) : 0).Max() + 1;
            }
            else
            {

                return 1;
            }
        }

        internal static StackValue Concat(StackValue sv1, StackValue sv2, ProtoCore.DSASM.Interpreter runtime)
        {
            if (!sv1.IsArray || !sv2.IsArray)
                return DSASM.StackValue.Null;

            var heap = runtime.runtime.RuntimeCore.Heap;
            var array1 = heap.ToHeapObject<DSArray>(sv1);
            var array2 = heap.ToHeapObject<DSArray>(sv2);
            var values = array1.Values.Concat(array2.Values).ToArray();
            try
            {
                return runtime.runtime.rmem.Heap.AllocateArray(values);
            }
            catch (RunOutOfMemoryException)
            {
                runtime.runtime.RuntimeCore.RuntimeStatus.LogWarning(WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                return StackValue.Null;
            }
        }

        private static void GetFlattenedArrayElements(StackValue sv, ProtoCore.DSASM.Interpreter runtime, ref List<StackValue> list)
        {
            if (!sv.IsArray)
            {
                list.Add(sv);
                return;
            }

            var array = runtime.runtime.rmem.Heap.ToHeapObject<DSArray>(sv);
            foreach (var item in array.Values)
            {
                GetFlattenedArrayElements(item, runtime, ref list); 
            }
        }

        internal static bool Exists(StackValue sv, ProtoCore.DSASM.Interpreter runtime, Predicate<StackValue> pred)
        {
            RuntimeCore runtimeCore = runtime.runtime.RuntimeCore;
            if (!sv.IsArray)
            {
                // Type mismatch.
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.InvalidArguments, Resources.kInvalidArguments);
                return false;
            }

            var array = runtimeCore.Heap.ToHeapObject<DSArray>(sv);
            foreach (var element in array.Values)
            {
                if (element.IsArray)
                {
                    if (Exists(element, runtime, pred))
                    {
                        return true;
                    }
                }
                else if (pred(element))
                {
                    return true;
                }
            }
            return false;
        }
        internal static bool ForAll(StackValue sv, ProtoCore.DSASM.Interpreter runtime, Predicate<StackValue> pred)
        {
            RuntimeCore runtimeCore = runtime.runtime.RuntimeCore;
            if (!sv.IsArray)
            {
                // Type mismatch.
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.InvalidArguments, Resources.kInvalidArguments);
                return true;
            }

            var array = runtimeCore.Heap.ToHeapObject<DSArray>(sv);
            foreach (var element in array.Values)
            {
                if (element.IsArray)
                {
                    if (!ForAll(element, runtime, pred))
                    {
                        return false;
                    }
                }
                else if (!pred(element))
                {
                    return false;
                }
            }
            return true;
        }
        
        internal static int CountNumber(StackValue sv, ProtoCore.DSASM.Interpreter runtime)
        {
            if (!sv.IsArray)
                return ProtoCore.DSASM.Constants.kInvalidIndex;

            var array = runtime.runtime.RuntimeCore.Heap.ToHeapObject<DSArray>(sv);
            return array.Values.Count(v => v.IsInteger || v.IsDouble);
        }


        internal static StackValue Equals(StackValue sv1, StackValue sv2, Interpreter runtime, ProtoCore.Runtime.Context context)
        {
            return StackValue.BuildBoolean(StackUtils.CompareStackValues(sv1, sv2, runtime.runtime.RuntimeCore, runtime.runtime.RuntimeCore, context));
        }

        //RemoveIfNot
        internal static StackValue RemoveIfNot(StackValue sv1, StackValue sv2, ProtoCore.DSASM.Interpreter runtime)
        {
            RuntimeCore runtimeCore = runtime.runtime.RuntimeCore;
            if (!sv1.IsArray)
            {
                // Type mismatch.
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.InvalidArguments, Resources.kInvalidArguments);
                return DSASM.StackValue.Null;
            }
            string typeString = FileIOBuiltIns.ConvertToString(sv2, runtime);
            List<StackValue> svList = new List<StackValue>();
            int index = 0;

            if (typeString == "array")
                typeString = ProtoCore.DSDefinitions.Keyword.Array;
            int type = runtimeCore.DSExecutable.TypeSystem.GetType(typeString);

            var array = runtimeCore.Heap.ToHeapObject<DSArray>(sv1);
            foreach (StackValue op in array.Values)
            {
                if (op.metaData.type == type)
                {
                    index++;
                    svList.Add(op);
                }
            }
            if (svList.Count >= 0)
            {
                try
                {
                    return runtime.runtime.rmem.Heap.AllocateArray(svList.ToArray());
                }
                catch (RunOutOfMemoryException)
                {
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                    return StackValue.Null;
                }
            }
            //That means an empty array
            return DSASM.StackValue.Null;
        }


        private static StackValue InsertCore(StackValue svArray, StackValue value, StackValue idx, ProtoCore.DSASM.Interpreter runtime)
        {
            int idxToBeInsert = (int)idx.IntegerValue;

            List<StackValue> svList = new List<StackValue>();
            var array = runtime.runtime.RuntimeCore.Heap.ToHeapObject<DSArray>(svArray);
            var elements = array.Values;
            int length = elements.Count();

            if (idxToBeInsert < 0)
            {
                idxToBeInsert += length;
            }

            if (idxToBeInsert < 0)
            {
                svList.Add(value);
                svList.AddRange(Enumerable.Repeat(StackValue.Null, -idxToBeInsert - 1));
                svList.AddRange(elements);
            }
            else if (idxToBeInsert >= length)
            {
                svList.AddRange(elements);
                svList.AddRange(Enumerable.Repeat(StackValue.Null, idxToBeInsert - length));
                svList.Add(value);
            }
            else
            {
                svList.AddRange(elements.ToList().GetRange(0, idxToBeInsert));
                svList.Add(value);
                svList.AddRange(elements.ToList().GetRange(idxToBeInsert, length - idxToBeInsert));
            }

            try
            {
                return runtime.runtime.rmem.Heap.AllocateArray(svList.ToArray());
            }
            catch (RunOutOfMemoryException)
            {
                runtime.runtime.RuntimeCore.RuntimeStatus.LogWarning(WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                return StackValue.Null;
            }
        }

        internal static StackValue upRank(StackValue sv, int countBraces, ProtoCore.DSASM.Interpreter runtime)
        {
            for (; countBraces > 0; countBraces--)
            {
                try
                {
                    sv = runtime.runtime.rmem.Heap.AllocateArray(new StackValue[] { sv });
                }
                catch (RunOutOfMemoryException)
                {
                    runtime.runtime.RuntimeCore.RuntimeStatus.LogWarning(WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                    return StackValue.Null;
                }
            }
            return sv;
        }
        /*
        internal static StackValue addBrace(StackValue sv, ProtoCore.DSASM.Interpreter runtime)
        {
            int newArray = runtime.runtime.rmem.Heap.Allocate(1);
            runtime.runtime.rmem.Heap.GetHeapElement(newArray).Stack[0] = sv;
            return DSASM.StackValue.BuildArrayPointer(newArray);
        }
        */
        //Transpose
        internal static StackValue Transpose(StackValue sv, ProtoCore.DSASM.Interpreter runtime){
            Heap heap = runtime.runtime.rmem.Heap;
            if (!sv.IsArray)
            {
                return sv;
            }
            bool is2DArray = false;
            var array = heap.ToHeapObject<DSArray>(sv);
            var svarr = array.Values;
            int numOfCols = 0;
            int numOfRows = svarr.Count();
            foreach (StackValue element in svarr)
            {
                if (element.IsArray)
                {
                    var elementArray = heap.ToHeapObject<DSArray>(element);
                    is2DArray = true;
                    numOfCols = Math.Max(elementArray.Count, numOfCols);
                }
            }
            if (is2DArray == false)
                return sv;
            //By now the numCols and numRows are confirmed
            StackValue[,] original = new StackValue[numOfRows, numOfCols];
            for (int c1 = 0; c1 < numOfRows; c1++)
            {
                int c2 = 1;
                StackValue rowArray = array.GetValueFromIndex(c1, runtime.runtime.RuntimeCore);
                if (!rowArray.IsArray)
                    original[c1, 0] = rowArray;
                else
                {
                    var row = heap.ToHeapObject<DSArray>(rowArray);
                    var items = row.Values.ToList();
                    for (c2 = 0; c2 < items.Count(); c2++)
                    {
                        original[c1, c2] = items[c2];
                    }
                }
                while(c2 < numOfCols)
                {
                    original[c1,c2] = DSASM.StackValue.Null;
                    c2++;
                }
            }
            StackValue[,] transposed = new StackValue[numOfCols,numOfRows];
            for (int c1 = 0; c1 < numOfCols; c1++)
            {
                for (int c2 = 0; c2 < numOfRows; c2++)
                {
                    transposed[c1, c2] = original[c2, c1];
                }
            }
            StackValue[] svList1 = new StackValue[transposed.GetLength(0)];
            for(int count1 = 0; count1 < transposed.GetLength(0); count1++)
            {
                //build an new item
                StackValue[] svList2 = new StackValue[transposed.GetLength(1)];
                for (int count2 = 0; count2 < transposed.GetLength(1); count2++)
                {
                    StackValue element = transposed[count1, count2]; 
                    svList2[count2] = element;
                }

                try
                {
                    StackValue finalCol = heap.AllocateArray(svList2);
                    svList1[count1] = finalCol;
                }
                catch (RunOutOfMemoryException)
                {
                    runtime.runtime.RuntimeCore.RuntimeStatus.LogWarning(WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                    svList1[count1] = StackValue.Null;
                }
            }

            try
            {
                return heap.AllocateArray(svList1);
            }
            catch (RunOutOfMemoryException)
            {
                runtime.runtime.RuntimeCore.RuntimeStatus.LogWarning(WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                return StackValue.Null;
            }
        }

        internal static StackValue Evaluate(StackValue function, StackValue parameters, StackValue unpackParams, Interpreter runtime, StackFrame stackFrame)
        {
            if (!function.IsFunctionPointer)
            {
                var runtimeCore = runtime.runtime.RuntimeCore;
                runtimeCore.RuntimeStatus.LogWarning(WarningID.InvalidType, Resources.InvalidFunction);
                return StackValue.Null;
            }

            var evaluator = new FunctionPointerEvaluator(function, runtime);

            StackValue ret;
            if (unpackParams.IsBoolean && unpackParams.BooleanValue)
            {
                DSArray argArray = runtime.runtime.RuntimeCore.Heap.ToHeapObject<DSArray>(parameters);
                var args = argArray.Values;
                ret = evaluator.Evaluate(args.ToList(), stackFrame);
            }
            else
            {
                ret = evaluator.Evaluate(new List<StackValue> { parameters}, stackFrame);
            }

            return ret;
        }
    }

    class StackValueComparerForDouble : IComparer<StackValue>
    {
        private bool mbAscending = true;
        public StackValueComparerForDouble(bool ascending)
        {
            mbAscending = ascending;
        }

        bool Equals(StackValue sv1, StackValue sv2)
        {
            bool sv1null = !sv1.IsNumeric;
            bool sv2null = !sv2.IsNumeric; 
            if ( sv1null && sv2null)
                return true;
            if (sv1null || sv2null)
                return false;

            var v1 = sv1.IsDouble ? sv1.DoubleValue: sv1.IntegerValue;
            var v2 = sv2.IsDouble ? sv2.DoubleValue: sv2.IntegerValue;

            return MathUtils.Equals(v1, v2);
        }

        public int Compare(StackValue sv1, StackValue sv2)
        {
            if (Equals(sv1, sv2))
                return 0;

            if (!sv1.IsNumeric)
                return mbAscending ? int.MinValue : int.MaxValue;

            if (!sv2.IsNumeric)
                return mbAscending ? int.MaxValue : int.MinValue;

            var value1 = sv1.IsDouble ? sv1.DoubleValue: sv1.IntegerValue;
            var value2 = sv2.IsDouble ? sv2.DoubleValue: sv2.IntegerValue;

            double x = mbAscending ? value1 : value2;
            double y = mbAscending ? value2 : value1; 

            if (x > y)
                return 1;
            else
                return -1;
        }
    }
}
