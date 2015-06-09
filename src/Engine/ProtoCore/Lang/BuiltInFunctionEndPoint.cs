
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using ProtoCore.DSASM;
using ProtoCore.Lang.Replication;
using ProtoCore.Utils;
using System.Linq;
using Autodesk.DesignScript.Interfaces;
using ProtoFFI;
using System.Collections;
using ProtoCore.Runtime;
using ProtoCore.Properties;

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
                case ProtoCore.Lang.BuiltInMethods.MethodID.kCount:
                    {
                        if (!formalParameters[0].IsArray)
                            ret = ProtoCore.DSASM.StackValue.BuildInt(1);
                        else if (!ArrayUtils.ContainsNonArrayElement(formalParameters[0], runtimeCore))
                            ret = ProtoCore.DSASM.StackValue.BuildInt(0);
                        else
                            ret = ProtoCore.DSASM.StackValue.BuildInt(ArrayUtilsForBuiltIns.Count(formalParameters[0], interpreter));
                        break;
                    }
                case ProtoCore.Lang.BuiltInMethods.MethodID.kRank:
                    {
                        ret = ProtoCore.DSASM.StackValue.BuildInt(ArrayUtilsForBuiltIns.Rank(formalParameters[0], interpreter));
                        break;
                    }
                case ProtoCore.Lang.BuiltInMethods.MethodID.kFlatten:
                    ret = ArrayUtilsForBuiltIns.Flatten(formalParameters[0], interpreter);
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kConcat:
                    ret = ArrayUtilsForBuiltIns.Concat(formalParameters[0], formalParameters[1], interpreter);
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kDifference:
                    ret = ArrayUtilsForBuiltIns.Difference(formalParameters[0], formalParameters[1], interpreter, c);
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kUnion:
                    ret = ArrayUtilsForBuiltIns.Union(formalParameters[0], formalParameters[1], interpreter, c);
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kIntersection:
                    ret = ArrayUtilsForBuiltIns.Intersection(formalParameters[0], formalParameters[1], interpreter, c);
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kSomeNulls:
                    ret = ProtoCore.DSASM.StackValue.BuildBoolean(ArrayUtilsForBuiltIns.SomeNulls(formalParameters[0], interpreter));
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kCountTrue:
                    {
                        if (!formalParameters[0].IsArray)
                        {
                            if (formalParameters[0].IsBoolean)
                            {
                                ret = StackValue.BuildInt(formalParameters[0].RawBooleanValue ? 1 : 0);
                            }
                            else
                            {
                                ret = StackValue.BuildInt(0);
                            }
                        }
                        else
                        {
                            ret = ProtoCore.DSASM.StackValue.BuildInt(ArrayUtilsForBuiltIns.CountTrue(formalParameters[0], interpreter));
                        }
                        break;
                    }
                case ProtoCore.Lang.BuiltInMethods.MethodID.kCountFalse:
                    {
                        if (!formalParameters[0].IsArray)
                        {
                            if (formalParameters[0].IsBoolean)
                            {
                                ret = ProtoCore.DSASM.StackValue.BuildInt(formalParameters[0].RawBooleanValue ? 0 : 1);
                            }
                            else
                            {
                                ret = ProtoCore.DSASM.StackValue.BuildInt(0);
                            }
                        }
                        else
                        {
                            ret = ProtoCore.DSASM.StackValue.BuildInt(ArrayUtilsForBuiltIns.CountFalse(formalParameters[0], interpreter));
                        }
                        break;
                    }
                case ProtoCore.Lang.BuiltInMethods.MethodID.kRangeExpression:
                    ret = RangeExpressionUntils.RangeExpression(formalParameters[0],
                                                                formalParameters[1],
                                                                formalParameters[2],
                                                                formalParameters[3],
                                                                formalParameters[4],
                                                                formalParameters[5],
                                                                runtimeCore);
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kAllFalse:
                    {
                        if (!formalParameters[0].IsArray)
                            ret = ProtoCore.DSASM.StackValue.Null;
                        else
                            ret = ProtoCore.DSASM.StackValue.BuildBoolean(ArrayUtilsForBuiltIns.AllFalse(formalParameters[0], interpreter));
                        break;
                    }
                case ProtoCore.Lang.BuiltInMethods.MethodID.kAllTrue:
                    {
                        if (!formalParameters[0].IsArray)
                            ret = ProtoCore.DSASM.StackValue.Null;
                        else
                            ret = ProtoCore.DSASM.StackValue.BuildBoolean(ArrayUtilsForBuiltIns.AllTrue(formalParameters[0], interpreter));
                        break;
                    }
                case ProtoCore.Lang.BuiltInMethods.MethodID.kIsHomogeneous:
                    //throw new NotImplementedException("LC urgent fix");
                    ret = ProtoCore.DSASM.StackValue.BuildBoolean(ArrayUtilsForBuiltIns.IsHomogeneous(formalParameters[0], interpreter));
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kSum:
                    {
                        ret = ArrayUtilsForBuiltIns.Sum(formalParameters[0], interpreter);
                        break;
                    }
                case ProtoCore.Lang.BuiltInMethods.MethodID.kAverage:
                    {
                        ret = ArrayUtilsForBuiltIns.Average(formalParameters[0], interpreter);
                        break;
                    }
                case ProtoCore.Lang.BuiltInMethods.MethodID.kSomeTrue:
                    {
                        if (!formalParameters[0].IsArray)
                            ret = ProtoCore.DSASM.StackValue.Null;
                        else
                            ret = ProtoCore.DSASM.StackValue.BuildBoolean(ArrayUtilsForBuiltIns.SomeTrue(formalParameters[0], interpreter));
                        break;
                    }
                case BuiltInMethods.MethodID.kSleep:
                    {
                        StackValue stackValue = formalParameters[0];
                        if (stackValue.IsInteger)
                            System.Threading.Thread.Sleep((int)stackValue.opdata);
                        else
                        {
                            runtimeCore.RuntimeStatus.LogWarning(
                                Runtime.WarningID.kInvalidArguments,
                                Resources.kInvalidArguments);
                        }

                        ret = DSASM.StackValue.Null;
                        break;
                    }
                case ProtoCore.Lang.BuiltInMethods.MethodID.kSomeFalse:
                    {
                        if (!formalParameters[0].IsArray)
                            ret = ProtoCore.DSASM.StackValue.Null;
                        else
                            ret = ProtoCore.DSASM.StackValue.BuildBoolean(ArrayUtilsForBuiltIns.SomeFalse(formalParameters[0], interpreter));
                        break;
                    }
                case ProtoCore.Lang.BuiltInMethods.MethodID.kRemove:
                    ret = ArrayUtilsForBuiltIns.Remove(formalParameters[0], formalParameters[1], interpreter);
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kRemoveDuplicates:
                    ret = ArrayUtilsForBuiltIns.RemoveDuplicates(formalParameters[0], interpreter, c);
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kRemoveNulls:
                    ret = ArrayUtilsForBuiltIns.RemoveNulls(formalParameters[0], interpreter);
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kRemoveIfNot:
                    ret = ArrayUtilsForBuiltIns.RemoveIfNot(formalParameters[0], formalParameters[1], interpreter);
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kReverse:
                    ret = ArrayUtilsForBuiltIns.Reverse(formalParameters[0], interpreter);
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kEquals:
                    ret = ArrayUtilsForBuiltIns.Equals(formalParameters[0], formalParameters[1], interpreter, c);
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kContains:
                    {
                        if (formalParameters[1].IsArray)
                            ret = ProtoCore.DSASM.StackValue.BuildBoolean(ArrayUtilsForBuiltIns.ContainsArray(formalParameters[0], formalParameters[1], interpreter));
                        else
                            ret = ProtoCore.DSASM.StackValue.BuildBoolean(ArrayUtilsForBuiltIns.Contains(formalParameters[0], formalParameters[1], interpreter));
                        break;
                    }
                case ProtoCore.Lang.BuiltInMethods.MethodID.kIndexOf:
                    {
                        if (formalParameters[0].IsArray)
                            ret = ProtoCore.DSASM.StackValue.BuildInt(ArrayUtilsForBuiltIns.ArrayIndexOfArray(formalParameters[0], formalParameters[1], interpreter));
                        else
                            ret = ProtoCore.DSASM.StackValue.BuildInt(ArrayUtilsForBuiltIns.IndexOf(formalParameters[0], formalParameters[1], interpreter));
                        break;
                    }
                case ProtoCore.Lang.BuiltInMethods.MethodID.kSort:
                    ret = ArrayUtilsForBuiltIns.Sort(formalParameters[0], interpreter);
                    break;
                case BuiltInMethods.MethodID.kSortPointer:
                    ret = ArrayUtilsForBuiltIns.SortPointers(formalParameters[0], formalParameters[1], interpreter, stackFrame);
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kSortWithMode:
                    ret = ArrayUtilsForBuiltIns.SortWithMode(formalParameters[0], formalParameters[1], interpreter);
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kSortIndexByValue:
                    ret = ArrayUtilsForBuiltIns.SortIndexByValue(formalParameters[0], interpreter);
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kSortIndexByValueWithMode:
                    ret = ArrayUtilsForBuiltIns.SortIndexByValueWithMode(formalParameters[0], formalParameters[1], interpreter);
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kReorder:
                    ret = ArrayUtilsForBuiltIns.Reorder(formalParameters[0], formalParameters[1], interpreter);
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kInsert:
                    {
                        if (formalParameters[1].IsArray)
                            ret = ArrayUtilsForBuiltIns.InsertArray(formalParameters[0], formalParameters[1], formalParameters[2], interpreter);
                        else
                            ret = ArrayUtilsForBuiltIns.Insert(formalParameters[0], formalParameters[1], formalParameters[2], interpreter);
                        break;
                    }
                case ProtoCore.Lang.BuiltInMethods.MethodID.kMap:
                    ret = ProtoCore.DSASM.StackValue.BuildDouble(MapBuiltIns.Map(formalParameters[0], formalParameters[1], formalParameters[2], interpreter));
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kMapTo:
                    ret = ProtoCore.DSASM.StackValue.BuildDouble(MapBuiltIns.MapTo(formalParameters[0], formalParameters[1], formalParameters[2], formalParameters[3], formalParameters[4], interpreter));
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kIsUniformDepth:
                    ret = ProtoCore.DSASM.StackValue.BuildBoolean(ArrayUtilsForBuiltIns.IsUniformDepth(formalParameters[0], interpreter));
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kIsRectangular:
                    if (formalParameters[0].IsArray)
                        ret = ProtoCore.DSASM.StackValue.BuildBoolean(ArrayUtilsForBuiltIns.IsRectangular(formalParameters[0], interpreter));
                    else
                        ret = ProtoCore.DSASM.StackValue.Null;
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kNormalizeDepthWithRank:
                    ret = ArrayUtilsForBuiltIns.NormalizeDepthWithRank(formalParameters[0], formalParameters[1], interpreter);
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kNormalizeDepth:
                    ret = ArrayUtilsForBuiltIns.NormalizeDepth(formalParameters[0], interpreter);
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kTranspose:
                    ret = ArrayUtilsForBuiltIns.Transpose(formalParameters[0], interpreter);
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kLoadCSV:
                    ret = FileIOBuiltIns.LoadCSV(formalParameters[0], interpreter);
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kLoadCSVWithMode:
                    ret = FileIOBuiltIns.LoadCSVWithMode(formalParameters[0], formalParameters[1], interpreter);
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kPrint:
                    ret = FileIOBuiltIns.Print(formalParameters[0], interpreter);
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kPrintIndexable:
                    ret = FileIOBuiltIns.Print(formalParameters[0], interpreter);
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kGetElapsedTime:
                    ret = ProtoCore.DSASM.StackValue.BuildInt(ProgramUtilsBuiltIns.GetElapsedTime(interpreter));
                    break;
                case ProtoCore.Lang.BuiltInMethods.MethodID.kInlineConditional:
                    {
                        StackValue svCondition = formalParameters[0];
                        if (!svCondition.IsBoolean)
                        {
                            // Comment Jun: Perhaps we can allow coercion?
                            Type booleanType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0);
                            svCondition = TypeSystem.Coerce(svCondition, booleanType, runtimeCore);
                        }

                        StackValue svTrue = formalParameters[1];
                        StackValue svFalse = formalParameters[2];

                        // If run in delta execution environment, we don't 
                        // create language blocks for true and false branch, 
                        // so directly return the value.
                        if (runtimeCore.Options.GenerateSSA)
                            return svCondition.RawBooleanValue ? svTrue : svFalse;

                        Validity.Assert(svTrue.IsInteger);
                        Validity.Assert(svFalse.IsInteger);
                        int blockId = (1 == (int)svCondition.opdata) ? (int)svTrue.opdata : (int)svFalse.opdata;

                        int oldRunningBlockId = runtimeCore.RunningBlock;
                        runtimeCore.RunningBlock = blockId;

                        int returnAddr = stackFrame.ReturnPC;

                        int ci = ProtoCore.DSASM.Constants.kInvalidIndex;
                        int fi = ProtoCore.DSASM.Constants.kInvalidIndex;
                        if (interpreter.runtime.rmem.Stack.Count >= ProtoCore.DSASM.StackFrame.kStackFrameSize)
                        {
                            ci = stackFrame.ClassScope;
                            fi = stackFrame.FunctionScope;
                        }

                        // The class scope does not change for inline conditional calls
                        StackValue svThisPtr = stackFrame.ThisPtr;


                        int blockDecl = 0;
                        int blockCaller = oldRunningBlockId;
                        StackFrameType type = StackFrameType.kTypeLanguage;
                        int depth = (int)interpreter.runtime.rmem.GetAtRelative(StackFrame.kFrameIndexStackFrameDepth).opdata;
                        int framePointer = rmem.FramePointer;
                        List<StackValue> registers = new List<StackValue>();

                        // Comment Jun: Calling convention data is stored on the TX register
                        StackValue svCallconvention = StackValue.BuildCallingConversion((int)ProtoCore.DSASM.CallingConvention.BounceType.kImplicit);
                        interpreter.runtime.TX = svCallconvention;

                        interpreter.runtime.SaveRegisters(registers);

                        // Comment Jun: the caller type is the current type in the stackframe
                        StackFrameType callerType = stackFrame.StackFrameType;

                        
                        blockCaller = runtimeCore.DebugProps.CurrentBlockId;
                        StackFrame bounceStackFrame = new StackFrame(svThisPtr, ci, fi, returnAddr, blockDecl, blockCaller, callerType, type, depth, framePointer, registers, null);

                        ret = interpreter.runtime.Bounce(blockId, 0, bounceStackFrame, 0, false, runtimeCore.CurrentExecutive.CurrentDSASMExec, runtimeCore.Breakpoints);

                        runtimeCore.RunningBlock = oldRunningBlockId;
                        break;
                    }

                case ProtoCore.Lang.BuiltInMethods.MethodID.kDot:
                    ret = DotMethod(formalParameters[0], stackFrame, interpreter.runtime, c);
                    break;

                case BuiltInMethods.MethodID.kGetType:
                    AddressType objType = formalParameters[0].optype;
                    int typeUID = (int)PrimitiveType.kInvalidType;

                    switch (objType)
                    {
                        case AddressType.Invalid:
                            typeUID = (int)PrimitiveType.kInvalidType;
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
                            typeUID = (int)PrimitiveType.kTypeInt;
                            break;
                        case AddressType.Double:
                            typeUID = (int)PrimitiveType.kTypeDouble;
                            break;
                        case AddressType.Boolean:
                            typeUID = (int)PrimitiveType.kTypeBool;
                            break;
                        case AddressType.Char:
                            typeUID = (int)PrimitiveType.kTypeChar;
                            break;
                        case AddressType.String:
                            typeUID = (int)PrimitiveType.kTypeString;
                            break;
                        case AddressType.Pointer:
                            typeUID = (int)PrimitiveType.kTypePointer;
                            break;
                        case AddressType.ArrayPointer:
                            typeUID = (int)PrimitiveType.kTypeArray;
                            break;
                        case AddressType.FunctionPointer:
                            typeUID = (int)PrimitiveType.kTypeFunctionPointer;
                            break;
                        case AddressType.Null:
                            typeUID = (int)PrimitiveType.kTypeNull;
                            break;
                        default:
                            typeUID = formalParameters[0].metaData.type;
                            break;
                    }

                    return StackValue.BuildInt(typeUID);
                case BuiltInMethods.MethodID.kToString:
                case BuiltInMethods.MethodID.kToStringFromObject:
                case BuiltInMethods.MethodID.kToStringFromArray:
                    ret = StringUtils.ConvertToString(formalParameters[0], runtimeCore, rmem);
                    break;
                case BuiltInMethods.MethodID.kImportData:
                    ret = ContextDataBuiltIns.ImportData(formalParameters[0], formalParameters[1], runtimeCore, interpreter, c);
                    break;
                case BuiltInMethods.MethodID.kBreak:
                    {
                        DebuggerBuiltIns.Break(interpreter, stackFrame);
                        ret = StackValue.Null;
                        break;
                    }

                case BuiltInMethods.MethodID.kGetKeys:
                    {
                        StackValue array = formalParameters[0];
                        if (!array.IsArray)
                        {
                            runtimeCore.RuntimeStatus.LogWarning(WarningID.kOverIndexing, Resources.kArrayOverIndexed);
                            ret = StackValue.Null;
                        }
                        else
                        {
                            var result = ArrayUtils.GetKeys(array, runtimeCore);
                            ret = rmem.Heap.AllocateArray(result, null);
                        }
                        break;
                    }
                case BuiltInMethods.MethodID.kGetValues:
                    {
                        StackValue array = formalParameters[0];
                        if (!array.IsArray)
                        {
                            runtimeCore.RuntimeStatus.LogWarning(WarningID.kOverIndexing, Resources.kArrayOverIndexed);
                            ret = StackValue.Null;
                        }
                        else
                        {
                            var result = ArrayUtils.GetValues(array, runtimeCore);
                            ret = rmem.Heap.AllocateArray(result, null);
                        }
                        break;
                    }
                case BuiltInMethods.MethodID.kContainsKey:
                    {
                        StackValue array = formalParameters[0];
                        StackValue key = formalParameters[1];
                        bool result = ArrayUtils.ContainsKey(array, key, runtimeCore);
                        ret = StackValue.BuildBoolean(result);
                        break;
                    }
                case BuiltInMethods.MethodID.kRemoveKey:
                    {
                        StackValue array = formalParameters[0];
                        StackValue key = formalParameters[1];
                        bool result = ArrayUtils.RemoveKey(array, key, runtimeCore);
                        ret = StackValue.BuildBoolean(result);
                        break;
                    }
                case BuiltInMethods.MethodID.kEvaluate:
                    ret = ArrayUtilsForBuiltIns.Evaluate(
                        formalParameters[0], 
                        formalParameters[1], 
                        formalParameters[2],
                        interpreter, 
                        stackFrame);
                    break;
                case BuiltInMethods.MethodID.kTryGetValueFromNestedDictionaries:
                    StackValue value;
                    if (ArrayUtils.TryGetValueFromNestedDictionaries(formalParameters[0], formalParameters[1], out value, runtimeCore))
                    {
                        ret = value;
                    }
                    else
                    {
                        ret = StackValue.Null;
                    }
                    break;
                case BuiltInMethods.MethodID.kNodeAstFailed:
                    var nodeFullName = formalParameters[0];
                    var fullName = StringUtils.GetStringValue(nodeFullName, runtimeCore);
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

            if (!isValidThisPointer || (!thisObject.IsPointer && !thisObject.IsArray))
            {
                runtimeCore.RuntimeStatus.LogWarning(WarningID.kDereferencingNonPointer, Resources.kDeferencingNonPointer);
                return StackValue.Null;
            }

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
            int functionArgs = (int)argumentCount.opdata;

            // Build the function arguments
            HeapElement heapElem = rmem.Heap.GetHeapElement(functionArguments);
            var arguments = heapElem.VisibleItems.ToList();

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
            var dynamicFunction = runtimeCore.DSExecutable.DynamicFuncTable.GetFunctionAtIndex((int)dynamicTableIndex.opdata);
            string functionName = dynamicFunction.Name;

            var replicationGuides = new List<List<ProtoCore.ReplicationGuide>>();
            if (!CoreUtils.IsGetterSetter(functionName))
            {
                replicationGuides = runtime.GetCachedReplicationGuides(functionArgs);
                if (removeFirstArgument)
                {
                    replicationGuides.RemoveAt(0);
                }
            }

            int thisObjectType = thisObject.metaData.type;
            ClassNode classNode = runtime.exe.classTable.ClassNodes[thisObjectType];
            int procIndex = classNode.vtable.IndexOfFirst(functionName);
            ProcedureNode procNode = null;

            // Trace hierarchy chain to find out the procedure node.
            if (Constants.kInvalidIndex == procIndex)
            {
                var currentClassNode = classNode;
                while (currentClassNode.baseList.Count > 0)
                {
                    int baseCI = currentClassNode.baseList[0];
                    currentClassNode = runtime.exe.classTable.ClassNodes[baseCI];
                    procIndex = currentClassNode.vtable.IndexOfFirst(functionName);
                    if (Constants.kInvalidIndex != procIndex)
                    {
                        procNode = currentClassNode.vtable.procList[procIndex];
                        break;
                    }
                }
            }
            else
            {
                procNode = classNode.vtable.procList[procIndex];
            }

            // If the function still isn't found, then it may be a function 
            // pointer. 
            if (procNode == null)
            {
                int memvarIndex = classNode.GetFirstVisibleSymbolNoAccessCheck(dynamicFunction.Name);

                if (Constants.kInvalidIndex != memvarIndex)
                {
                    StackValue svMemberPtr = rmem.Heap.GetHeapElement(thisObject).Stack[memvarIndex];
                    if (svMemberPtr.IsPointer)
                    {
                        StackValue svFunctionPtr = rmem.Heap.GetHeapElement(svMemberPtr).Stack[0];
                        if (svFunctionPtr.IsFunctionPointer)
                        {
                            // It is a function pointer
                            // Functions pointed to are all in the global scope
                            thisObjectType = ProtoCore.DSASM.Constants.kGlobalScope;
                            procIndex = (int)svFunctionPtr.opdata;
                            procNode = runtime.exe.procedureTable[0].procList[procIndex];
                            functionName = procNode.name;
                        }
                    }
                }
            }

            // Build the stackframe
            var newStackFrame = new StackFrame(thisObject, 
                                               stackFrame.ClassScope, 
                                               procIndex, 
                                               stackFrame.ReturnPC, 
                                               stackFrame.FunctionBlock, 
                                               stackFrame.FunctionCallerBlock, 
                                               stackFrame.StackFrameType,
                                               StackFrameType.kTypeFunction, 
                                               0,
                                               rmem.FramePointer, 
                                               stackFrame.GetRegisters(), 
                                               null);

            ProtoCore.CallSite callsite = runtimeData.GetCallSite(
                runtime.exe.ExecutingGraphnode, 
                thisObjectType, 
                functionName, 
                runtime.exe,
                runtimeCore.RunningBlock,
                runtimeCore.Options, 
                runtimeCore.RuntimeStatus);
            Validity.Assert(null != callsite);

            // TODO: Disabling support for stepping into replicated function calls temporarily - pratapa
            if (runtimeCore.Options.IDEDebugMode &&
                runtimeCore.Options.RunMode != InterpreterMode.kExpressionInterpreter &&
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

            StackValue ret = callsite.JILDispatchViaNewInterpreter(context, arguments, replicationGuides, newStackFrame, runtimeCore);

            // Restore debug properties after returning from a CALL/CALLR
            if (runtimeCore.Options.IDEDebugMode &&
                runtimeCore.Options.RunMode != InterpreterMode.kExpressionInterpreter &&
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

            CLRObjectMarshler marshaler = CLRObjectMarshler.GetInstance(runtimeCore);

            Dictionary<string, Object> parameters = new Dictionary<string, object>();
            if (!svConnectionParameters.IsArray)
            {
                Object connectionParameters = marshaler.UnMarshal(svConnectionParameters, c, interpreter, typeof(Object));
                parameters.Add("data", connectionParameters);
            }
            else
            {
                StackValue[] svArray = ArrayUtils.GetValues(svConnectionParameters, interpreter.runtime.RuntimeCore).ToArray();
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
            ProtoCore.Type type = PrimitiveMarshler.CreateType(ProtoCore.PrimitiveType.kTypeVar);
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

    internal class ProgramUtilsBuiltIns
    {
        //return the number of milliseconds past from executing the program 
        internal static int GetElapsedTime(ProtoCore.DSASM.Interpreter runtime)
        {
            TimeSpan ts = runtime.runtime.RuntimeCore.GetCurrentTime();
            int ms = ts.Milliseconds;
            return ms;
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
                    blockId = (int)instructions[pc].op1.opdata;
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

            result = runtime.runtime.rmem.Heap.GetString(st);
            result.Replace("\\\\", "\\");
            return result;
        }

        //LoadCSV(filename) & LoadCSV(filename, transpose)
        internal static StackValue LoadCSV(StackValue fn, ProtoCore.DSASM.Interpreter runtime)
        {
            return LoadCSVWithMode(fn, StackValue.BuildBoolean(false), runtime);
        }
        internal static StackValue LoadCSVWithMode(StackValue fn, StackValue trans, ProtoCore.DSASM.Interpreter runtime)
        {
            RuntimeCore runtimeCore = runtime.runtime.RuntimeCore;
            if (!trans.IsBoolean)
            {
                // Type mismatch.
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.kInvalidArguments, Resources.kInvalidArguments);
                return DSASM.StackValue.Null;
            }

            string filename = ConvertToString(fn, runtime);
            string path = FileUtils.GetDSFullPathName(filename, runtime.runtime.RuntimeCore.Options);
            // File not existing.
            if(null==path || !File.Exists(path))
            {
                string message = String.Format(Resources.kFileNotFound, path);
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.kFileNotExist, message);
                return DSASM.StackValue.Null;
            }
            // Open the file to read from.
            List<Object[]> CSVdatalist = new List<Object[]>();
            int colNum = 0;
            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using (var sr = new StreamReader(fileStream))
            {

                while (!sr.EndOfStream)
                {
                    String[] lineStr = sr.ReadLine().Split(',');
                    int count = 0;
                    Object[] line = new Object[lineStr.Length];
                    foreach (string elementStr in lineStr)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(elementStr) || string.IsNullOrWhiteSpace(elementStr))
                                line[count] = null;
                            else if (elementStr.Contains("."))
                                line[count] = Double.Parse(elementStr);
                            else line[count] = Int32.Parse(elementStr);
                        }
                        catch(Exception)
                        {
                            line[count] = elementStr;
                        }
                        count++;
                    }
                    colNum = Math.Max(colNum, line.Length);
                    CSVdatalist.Add(line);
                }
            }
            //convert to 2-D array
            int rowNum = CSVdatalist.Count;
            StackValue[] rows = new StackValue[rowNum];

            for (int i = 0; i < rowNum; i++)
            {
                StackValue[] values = new StackValue[colNum];

                for (int k = 0; k < colNum; k++)
                {
                    if (CSVdatalist[i].Length <= k)
                    {
                        values[k] = StackValue.Null;
                    }
                    else if (null == CSVdatalist[i][k])
                    {
                        values[k] = StackValue.Null;
                    }
                    else if (CSVdatalist[i][k] is Double)
                    {
                        values[k] = StackValue.BuildDouble((double)CSVdatalist[i][k]);
                    }
                    else if (CSVdatalist[i][k] is Int32)
                    {
                        values[k] = StackValue.BuildInt((int)CSVdatalist[i][k]);
                    }
                    else
                    {
                        values[k] = StackValue.BuildString((string)CSVdatalist[i][k], runtime.runtime.rmem.Heap);
                    }
                }

                rows[i] = runtime.runtime.rmem.Heap.AllocateArray(values);
            }
            StackValue result = runtime.runtime.rmem.Heap.AllocateArray(rows);
            //Judge whether the array needed to be transposed(when Boolean:trans is false) or not(when Boolean:trans is true)
            if (trans.opdata == 1)
            {
                return result; 
            }
            else
            {
                return ArrayUtilsForBuiltIns.Transpose(result, runtime);
            }
        }
        //Print(msg) & Print(msg, mode)
        internal static StackValue Print(StackValue msg, ProtoCore.DSASM.Interpreter runtime)
        {
            //TODO: Change Execution mirror class to have static methods, so that an instance does not have to be created
            ProtoCore.DSASM.Mirror.ExecutionMirror mirror = new DSASM.Mirror.ExecutionMirror(runtime.runtime, runtime.runtime.RuntimeCore);
            string result = mirror.GetStringValue(msg, runtime.runtime.rmem.Heap, 0, true);
            //For Console output
            Console.WriteLine(result);
            
            ////For IDE output
            //ProtoCore.Core core = runtime.runtime.Core;
            //OutputMessage t_output = new OutputMessage(result);
            //core.BuildStatus.MessageHandler.Write(t_output);
            ProtoCore.RuntimeCore runtimeCore = runtime.runtime.RuntimeCore;
            OutputMessage t_output = new OutputMessage(result);
            runtimeCore.RuntimeStatus.MessageHandler.Write(t_output);

            return DSASM.StackValue.Null;
        }
    }

         
        
    internal class MapBuiltIns
    {
        internal static double Map(StackValue sv0, StackValue sv1, StackValue sv2, ProtoCore.DSASM.Interpreter runtime)
        {
            if (!(((sv0.IsDouble) || (sv0.IsInteger)) &&
                ((sv1.IsDouble) || (sv1.IsInteger)) &&                
                ((sv2.IsDouble) || (sv2.IsInteger))))
                return ProtoCore.DSASM.Constants.kInvalidIndex;
            double rangeMin = sv0.ToDouble().RawDoubleValue;
            double rangeMax = sv1.ToDouble().RawDoubleValue;
            double inputValue = sv2.ToDouble().RawDoubleValue;
            double result =  (inputValue - rangeMin) / (rangeMax - rangeMin);
            if (result < 0) return 0.0; //Exceed the range (less than rangeMin)
            if (result > 1) return 1.0; //Exceed the range (less than rangeMax)
            return result;
        }
        internal static double MapTo(StackValue sv0, StackValue sv1, StackValue sv2,
            StackValue sv3, StackValue sv4, ProtoCore.DSASM.Interpreter runtime)
        {
            if (!(((sv0.IsDouble) || (sv0.IsInteger)) &&
                ((sv1.IsDouble) || (sv1.IsInteger)) &&
                ((sv2.IsDouble) || (sv2.IsInteger)) &&
                ((sv3.IsDouble) || (sv3.IsInteger)) &&
                ((sv4.IsDouble) || (sv4.IsInteger))))
                return ProtoCore.DSASM.Constants.kInvalidIndex;
            double rangeMin = sv0.ToDouble().RawDoubleValue;
            double rangeMax = sv1.ToDouble().RawDoubleValue;
            double inputValue = sv2.ToDouble().RawDoubleValue;
            double targetRangeMin = sv3.ToDouble().RawDoubleValue;
            double targetRangeMax = sv4.ToDouble().RawDoubleValue;
            double result = targetRangeMin + (inputValue - rangeMin) * (targetRangeMax - targetRangeMin) / (rangeMax - rangeMin);
            if (result < targetRangeMin){ return targetRangeMin; }     //clamp to targetRangeMin
            if (result > targetRangeMax){ return targetRangeMax; }     //clamp to targetRangeMax
            return result;
        }
    }
    internal class RangeExpressionUntils
    {
        internal static StackValue[] GenerateRangeByStepSize(decimal start, decimal end, decimal stepsize, bool isIntRange)
        {
            if ((stepsize == 0) || (end > start && stepsize < 0) || (end < start && stepsize > 0))
            {
                return null;
            }
            else
            {
                int stepnum = (int)Math.Truncate((end - start) / stepsize) + 1;
                StackValue[] range = new StackValue[stepnum];

                decimal cur = start;
                for (int i = 0; i < stepnum; ++i)
                {
                    range[i] = isIntRange ? StackValue.BuildInt((int)cur) : StackValue.BuildDouble((double)cur);
                    cur += stepsize;
                }
                return range;
            }
        }

        // For to include start and end. 
        internal static StackValue[] GenerateRangeByStepNumber(decimal start, decimal end, decimal stepnum, bool isIntRange)
        {
            decimal stepsize = (stepnum == 1) ? 0 : (end - start) / (stepnum - 1);
            isIntRange = isIntRange && (Math.Truncate(stepsize) == stepsize);

            StackValue[] range = new StackValue[(int)stepnum > 1 ? (int)stepnum : 1];
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

        internal static StackValue[] GenerateRangeByAmount(decimal start, long amount, decimal stepsize, bool isIntRange)
        {
            if (amount == 0)
            {
                return new StackValue[] { };
            }
            else
            {
                isIntRange = isIntRange && (Math.Truncate(stepsize) == stepsize);
                StackValue[] range = new StackValue[amount];
                for (int i = 0; i < amount; ++i)
                {
                    range[i] = isIntRange ? StackValue.BuildInt((int)start) : StackValue.BuildDouble((double)start);
                    start += stepsize;
                }
                return range;
            }
        }

        internal static StackValue RangeExpression(StackValue svStart,
                                                   StackValue svEnd,
                                                   StackValue svStep,
                                                   StackValue svOp,
                                                   StackValue svHasStep,
                                                   StackValue svHasAmountOp,
                                                   ProtoCore.RuntimeCore runtimeCore)
        {

            if (!svStart.IsNumeric || !svEnd.IsNumeric)
            {
                runtimeCore.RuntimeStatus.LogWarning(
                    WarningID.kInvalidArguments, 
                    Resources.kInvalidArgumentsInRangeExpression);
                return StackValue.Null;
            }

            bool hasStep = svHasStep.IsBoolean && svHasStep.RawBooleanValue;
            bool hasAmountOp = svHasAmountOp.IsBoolean && svHasAmountOp.RawBooleanValue;

            if (hasAmountOp)
            {
                if (!svEnd.IsNumeric)
                {
                    runtimeCore.RuntimeStatus.LogWarning(
                        WarningID.kInvalidArguments, 
                        Resources.kInvalidAmountInRangeExpression);
                    return StackValue.Null;
                }
                else if (!hasStep)
                {
                    runtimeCore.RuntimeStatus.LogWarning(
                        WarningID.kInvalidArguments, 
                        Resources.kNoStepSizeInAmountRangeExpression);
                    return StackValue.Null;
                }
            }

            if (svStep.IsNull && hasStep)
            {
                runtimeCore.RuntimeStatus.LogWarning(
                    WarningID.kInvalidArguments, 
                    Resources.kInvalidArgumentsInRangeExpression);
                return StackValue.Null;
            }
            else if (!svStep.IsNull && !svStep.IsNumeric)
            {
                runtimeCore.RuntimeStatus.LogWarning(
                    WarningID.kInvalidArguments, 
                    Resources.kInvalidArgumentsInRangeExpression);
                return StackValue.Null;
            }

            double startValue = svStart.ToDouble().RawDoubleValue;
            if (double.IsInfinity(startValue) || double.IsNaN(startValue))
            {
                runtimeCore.RuntimeStatus.LogWarning(
                    WarningID.kInvalidArguments, 
                    Resources.kInvalidArgumentsInRangeExpression);
                return StackValue.Null;
            }
            decimal start = new decimal(startValue);

            double endValue = svEnd.ToDouble().RawDoubleValue;
            if (double.IsInfinity(endValue) || double.IsNaN(endValue))
            {
                runtimeCore.RuntimeStatus.LogWarning(
                    WarningID.kInvalidArguments, 
                    Resources.kInvalidArgumentsInRangeExpression);
                return StackValue.Null;
            }
            decimal end = new decimal(endValue);

            if (svStep.IsDouble)
            {
                double stepValue = svStep.RawDoubleValue;
                if (double.IsInfinity(stepValue) || double.IsNaN(stepValue))
                {
                    runtimeCore.RuntimeStatus.LogWarning(
                        WarningID.kInvalidArguments, 
                        Resources.kInvalidArgumentsInRangeExpression);
                    return StackValue.Null;
                }
            }
 
            bool isIntRange = svStart.IsInteger && svEnd.IsInteger;

            StackValue[] range = null;
            if (hasAmountOp)
            {
                long amount = svEnd.ToInteger().opdata;
                if (amount < 0)
                {
                    runtimeCore.RuntimeStatus.LogWarning(
                       WarningID.kInvalidArguments, 
                       Resources.kInvalidAmountInRangeExpression);
                   return StackValue.Null;
                }
                decimal stepsize = new decimal(svStep.ToDouble().RawDoubleValue);
                range = GenerateRangeByAmount(start, amount, stepsize, isIntRange);
            }
            else
            {
                switch (svOp.opdata)
                {
                    case (int)ProtoCore.DSASM.RangeStepOperator.stepsize:
                        {
                            decimal stepsize = (start > end) ? -1 : 1;
                            if (hasStep)
                            {
                                stepsize = new decimal(svStep.IsDouble ? svStep.RawDoubleValue : svStep.RawIntValue);
                                isIntRange = isIntRange && (svStep.IsInteger);
                            }

                            range = GenerateRangeByStepSize(start, end, stepsize, isIntRange);
                            break;
                        }
                    case (int)ProtoCore.DSASM.RangeStepOperator.num:
                        {
                            decimal stepnum = new decimal(Math.Round(svStep.IsDouble ? svStep.RawDoubleValue : svStep.RawIntValue));
                            if (stepnum > 0)
                            {
                                range = GenerateRangeByStepNumber(start, end, stepnum, isIntRange);
                            }
                            break;
                        }
                    case (int)ProtoCore.DSASM.RangeStepOperator.approxsize:
                        {
                            decimal astepsize = new decimal(svStep.IsDouble ? svStep.RawDoubleValue : svStep.RawIntValue);
                            if (astepsize != 0)
                            {
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
                                range = GenerateRangeByStepNumber(start, end, stepnum, isIntRange);
                            }
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
            return range == null ? StackValue.Null : runtimeCore.RuntimeMemory.Heap.AllocateArray(range, null);
        }
    }
    internal class ArrayUtilsForBuiltIns
    {
        internal static int Count(StackValue sv, ProtoCore.DSASM.Interpreter runtime)
        {
            if (!sv.IsArray)
                return ProtoCore.DSASM.Constants.kInvalidIndex;

            return ArrayUtils.GetElementSize(sv, runtime.runtime.RuntimeCore);
        }

        internal static int Rank(StackValue sv, ProtoCore.DSASM.Interpreter runtime)
        {
            if (!sv.IsArray)
                return 0;

            var values = ArrayUtils.GetValues(sv, runtime.runtime.RuntimeCore);
            if (values.Any())
            {
                return values.Select(x => x.IsArray ? Rank(x, runtime) : 0).Max() + 1;
            }
            else
            {

                return 1;
            }
        }

        internal static StackValue Flatten(StackValue sv, ProtoCore.DSASM.Interpreter runtime)
        {
            if (!sv.IsArray)
                return DSASM.StackValue.Null;

            List<StackValue> newElements = new List<DSASM.StackValue>();
            GetFlattenedArrayElements(sv, runtime, ref newElements);
            return runtime.runtime.rmem.Heap.AllocateArray(newElements, null);
        }

        internal static StackValue Concat(StackValue sv1, StackValue sv2, ProtoCore.DSASM.Interpreter runtime)
        {
            if (!sv1.IsArray || !sv2.IsArray)
                return DSASM.StackValue.Null;

            var svArray1 = ArrayUtils.GetValues(sv1, runtime.runtime.RuntimeCore);
            var svArray2 = ArrayUtils.GetValues(sv2, runtime.runtime.RuntimeCore);
            var values = svArray1.Concat(svArray2).ToList();
            return runtime.runtime.rmem.Heap.AllocateArray(values);
        }

        private static void GetFlattenedArrayElements(StackValue sv, ProtoCore.DSASM.Interpreter runtime, ref List<StackValue> list)
        {
            if (!sv.IsArray)
            {
                list.Add(sv);
                return;
            }

            var heapElement = ArrayUtils.GetHeapElement(sv, runtime.runtime.RuntimeCore);
            foreach (var item in heapElement.VisibleItems)
            {
                GetFlattenedArrayElements(item, runtime, ref list); 
            }
        }
        //Difference
        internal static StackValue Difference(StackValue sv1, StackValue sv2, ProtoCore.DSASM.Interpreter runtime, ProtoCore.Runtime.Context context)
        {
            if((Rank(sv1, runtime)!=1)||(Rank(sv2, runtime)!=1)){
                Console.WriteLine("Warning: Both arguments were expected to be one-dimensional array type!");
                return DSASM.StackValue.Null;
            }
            if ((!sv1.IsArray) || (!sv1.IsArray))
                return DSASM.StackValue.Null;
            sv1 = RemoveDuplicates(sv1, runtime, context);
            sv2 = RemoveDuplicates(sv2, runtime, context);
            var svArray1 = ArrayUtils.GetValues(sv1, runtime.runtime.RuntimeCore);
            var svArray2 = ArrayUtils.GetValues(sv2, runtime.runtime.RuntimeCore);
            List<StackValue> svList = new List<StackValue>();
            foreach (var item1 in svArray1)
            {
                if (svArray2.All(item2 => !StackUtils.CompareStackValues(item1, item2, runtime.runtime.RuntimeCore)))
                {
                    svList.Add(item1);
                }
            }

            if (svList.Count >= 0)
            {
                var heap = runtime.runtime.rmem.Heap;
                return heap.AllocateArray(svList);
            }
            //That means an empty array
            else return DSASM.StackValue.Null;
        }
        //Union
        internal static StackValue Union(StackValue sv1, StackValue sv2, ProtoCore.DSASM.Interpreter runtime, ProtoCore.Runtime.Context context)
        {
            RuntimeCore runtimeCore = runtime.runtime.RuntimeCore;
            if ((Rank(sv1, runtime) != 1) || (Rank(sv2, runtime) != 1))
            {
                //LC urgent patch
                runtimeCore.RuntimeStatus.LogWarning(
                    ProtoCore.Runtime.WarningID.kTypeMismatch, Resources.OneDArrayExpected);
                return DSASM.StackValue.Null;
            }
            return RemoveDuplicates(Concat(sv1, sv2, runtime), runtime, context);
        }
        //Intersection
        internal static StackValue Intersection(StackValue sv1, StackValue sv2, ProtoCore.DSASM.Interpreter runtime, ProtoCore.Runtime.Context context)
        {
            if ((Rank(sv1, runtime) != 1) || (Rank(sv2, runtime) != 1))
            {
                Console.WriteLine("Warning: Both arguments were expected to be one-dimensional array type!");
                return DSASM.StackValue.Null;
            }
            if ((!sv1.IsArray) || (!sv1.IsArray))
                return DSASM.StackValue.Null;
            sv1 = RemoveDuplicates(sv1, runtime, context);
            sv2 = RemoveDuplicates(sv2, runtime, context);
            var svArray1 = ArrayUtils.GetValues(sv1, runtime.runtime.RuntimeCore);
            var svArray2 = ArrayUtils.GetValues(sv2, runtime.runtime.RuntimeCore);
            List<StackValue> svList = new List<StackValue>();
            foreach (var item1 in svArray1)
            {
                if (svArray2.Any(item2 => StackUtils.CompareStackValues(item1, item2, runtime.runtime.RuntimeCore)))
                {
                    svList.Add(item1);
                }
            }
            if (svList.Count >= 0)
            {
                var heap = runtime.runtime.rmem.Heap;
                return heap.AllocateArray(svList);
            }
            //That means an empty array
            else return DSASM.StackValue.Null;
        }
       
        //CountFalse
        internal static int CountFalse(StackValue sv, ProtoCore.DSASM.Interpreter runtime)
        {
            var values = ArrayUtils.GetValues(sv, runtime.runtime.RuntimeCore);
            int countFalse = 0;
            foreach (var element in values)
            {
                if (element.IsBoolean && (element.opdata == 0))
                {
                    countFalse++;
                }
                else if (element.IsArray)
                {
                    countFalse += CountFalse(element, runtime);
                }
            }
            return countFalse;
        }
        //CountTrue
        internal static int CountTrue(StackValue sv, ProtoCore.DSASM.Interpreter runtime)
        {
            int countTrue = 0;

            var svArray = ArrayUtils.GetValues(sv, runtime.runtime.RuntimeCore);
            foreach (var item in svArray)
            {
                if (item.IsArray)
                    countTrue += CountTrue(item, runtime);
                else if (item.IsBoolean && item.opdata != 0)
                    ++countTrue;
            }

            return countTrue;
        }
        internal static bool Exists(StackValue sv, ProtoCore.DSASM.Interpreter runtime, Predicate<StackValue> pred)
        {
            RuntimeCore runtimeCore = runtime.runtime.RuntimeCore;
            if (!sv.IsArray)
            {
                // Type mismatch.
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.kInvalidArguments, Resources.kInvalidArguments);
                return false;
            }

            var svArray = ArrayUtils.GetValues(sv, runtime.runtime.RuntimeCore);
            foreach (var element in svArray)
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
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.kInvalidArguments, Resources.kInvalidArguments);
                return true;
            }

            var svArray = ArrayUtils.GetValues(sv, runtime.runtime.RuntimeCore);
            foreach (var element in svArray)
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
        //SomeBulls, SomeFalse, SomeTrue
        internal static bool SomeNulls(StackValue sv, ProtoCore.DSASM.Interpreter runtime)
        {
            return Exists(sv, runtime, element => element.IsNull);
        }
        internal static bool SomeTrue(StackValue sv, ProtoCore.DSASM.Interpreter runtime)
        {
            return Exists(sv, runtime, element => (element.IsBoolean && element.opdata != 0));
        }
        internal static bool SomeFalse(StackValue sv, ProtoCore.DSASM.Interpreter runtime)
        {
            return Exists(sv, runtime, element => (element.IsBoolean && element.opdata == 0));
        }
        //AllTrue
        internal static bool AllFalse(StackValue sv, ProtoCore.DSASM.Interpreter runtime)
        {
            return ForAll(sv, runtime, element => (element.IsBoolean && element.opdata == 0));
        }
        internal static bool AllTrue(StackValue sv, ProtoCore.DSASM.Interpreter runtime)
        {
            return ForAll(sv, runtime, element => (element.IsBoolean && element.opdata != 0));
        }
        //isHomogeneous
        internal static bool IsHomogeneous(StackValue sv, ProtoCore.DSASM.Interpreter runtime)
        {
            RuntimeCore runtimeCore = runtime.runtime.RuntimeCore;
            if (!sv.IsArray)
            {
                // Type mismatch.
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.kInvalidArguments, Resources.kInvalidArguments);
                return true;// ProtoCore.DSASM.Constants.kInvalidIndex;
            }
            var svArray = ArrayUtils.GetValues(sv, runtime.runtime.RuntimeCore);
            if (!svArray.Any())
            {
                return true;
            }
            AddressType type = svArray.FirstOrDefault().optype;
            foreach (var element in svArray)
            {
                if (element.IsArray)
                {
                    if (!IsHomogeneous(element, runtime))
                        return false;
                }
                if (!(type.Equals(element.optype)))
                {
                    return false;
                }
            }
            return true;
        }
        //Sum
        internal static StackValue Sum(StackValue sv, ProtoCore.DSASM.Interpreter runtime)
        {
            if (!sv.IsArray)
            {
                return ProtoCore.DSASM.StackValue.Null;
            }

            if (!ArrayUtils.ContainsNonArrayElement(sv, runtime.runtime.RuntimeCore))
                return ProtoCore.DSASM.StackValue.Null;

            StackValue svnew = ArrayUtilsForBuiltIns.Flatten(sv, runtime);

            bool bContainsValidElement = false;
            bool anyDouble = ArrayUtils.ContainsDoubleElement(svnew, runtime.runtime.RuntimeCore);
            double sum = 0;
            AddressType type = anyDouble ? AddressType.Double : AddressType.Int;
            var svArray = ArrayUtils.GetValues(svnew, runtime.runtime.RuntimeCore);
            foreach (var element in svArray)
            {
                if (element.optype != type)
                    continue;

                bContainsValidElement = true;

                if (type == AddressType.Double)
                    sum += element.ToDouble().RawDoubleValue;
                else
                    sum += element.ToInteger().RawIntValue;
            }

            if (!bContainsValidElement)
                return ProtoCore.DSASM.StackValue.Null;

            if (type == AddressType.Double)
                return ProtoCore.DSASM.StackValue.BuildDouble(sum);
            else
                return ProtoCore.DSASM.StackValue.BuildInt((int)(sum));
        }
        internal static int CountNumber(StackValue sv, ProtoCore.DSASM.Interpreter runtime)
        {
            if (!sv.IsArray)
                return ProtoCore.DSASM.Constants.kInvalidIndex;

            var values = ArrayUtils.GetValues(sv, runtime.runtime.RuntimeCore);
            return values.Count(v => v.IsInteger || v.IsDouble);
        }
        //Average
        internal static StackValue Average(StackValue sv, ProtoCore.DSASM.Interpreter runtime)
        {
            if (!sv.IsArray)
            {
                return ProtoCore.DSASM.StackValue.Null;
            }

            var svArray = ArrayUtils.GetValues(sv, runtime.runtime.RuntimeCore);
            if (!svArray.Any())
                return ProtoCore.DSASM.StackValue.Null;

            StackValue newsv = Flatten(sv, runtime);
            int length = CountNumber(newsv, runtime);
            if (length == 0) 
                return ProtoCore.DSASM.StackValue.Null;
            StackValue resSv = Sum(newsv, runtime);
            if (resSv.IsDouble)
                return ProtoCore.DSASM.StackValue.BuildDouble(resSv.RawDoubleValue / length);
            else if (resSv.IsInteger)
                return ProtoCore.DSASM.StackValue.BuildDouble((double)(resSv.opdata) / length);
            else
                return ProtoCore.DSASM.StackValue.Null;
        }
        //Remove
        internal static StackValue Remove(StackValue sv1, StackValue sv2, ProtoCore.DSASM.Interpreter runtime)
        {
            RuntimeCore runtimeCore = runtime.runtime.RuntimeCore;
            if (!sv1.IsArray || !sv2.IsInteger)
            {
                // Type mismatch.
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.kInvalidArguments, Resources.kInvalidArguments);
                return DSASM.StackValue.Null;
            }

            var svArray = ArrayUtils.GetValues(sv1, runtime.runtime.RuntimeCore).ToArray();
            int length = svArray.Length;
            int indexToBeRemoved = (int)sv2.opdata;
            if (indexToBeRemoved < 0)
            {
                indexToBeRemoved += length;
            }

            if ((indexToBeRemoved < 0) || (indexToBeRemoved >= length))
            {
                // Type mismatch.
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.kIndexOutOfRange, Resources.kIndexOutOfRange);
                return DSASM.StackValue.Null;
            }
            List<StackValue> svList = new List<StackValue>();
            for (int indexCount = 0; indexCount < length; ++indexCount)
            {
                if (indexCount != indexToBeRemoved)
                    svList.Add(svArray[indexCount]);
            }
            return runtime.runtime.rmem.Heap.AllocateArray(svList);
        }
        //RemoveDuplicate
        internal static StackValue RemoveDuplicates(StackValue sv, ProtoCore.DSASM.Interpreter runtime, ProtoCore.Runtime.Context context)
        {
            if (!sv.IsArray)
            {
                return sv;
            }
            var svArray = ArrayUtils.GetValues(sv, runtime.runtime.RuntimeCore).ToArray();
            List<StackValue> svList = new List<StackValue>();
            int length = svArray.Length;
            for (int outIx = length-1; outIx >= 0; --outIx)
            {
                Boolean duplicate = false;
                StackValue  outOp = svArray[outIx];
                for (int inIx = 0; inIx < outIx; ++inIx)
                {
                    StackValue inOp = svArray[inIx];
                    if (StackUtils.CompareStackValues(outOp, inOp, runtime.runtime.RuntimeCore, runtime.runtime.RuntimeCore, context))
                    {
                        duplicate = true;
                        break;
                    }
                }

                if (!duplicate)
                {
                    svList.Insert(0, outOp);
                }
            }
            return runtime.runtime.rmem.Heap.AllocateArray(svList);
        }

        internal static StackValue Equals(StackValue sv1, StackValue sv2, Interpreter runtime, ProtoCore.Runtime.Context context)
        {
            return StackValue.BuildBoolean(StackUtils.CompareStackValues(sv1, sv2, runtime.runtime.RuntimeCore, runtime.runtime.RuntimeCore, context));
        }
       
        //RemoveNulls
        internal static StackValue RemoveNulls(StackValue sv, ProtoCore.DSASM.Interpreter runtime)
        {
            RuntimeCore runtimeCore = runtime.runtime.RuntimeCore;
            if (!sv.IsArray)
            {
                // Type mismatch.
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.kInvalidArguments, Resources.kInvalidArguments);
                return DSASM.StackValue.Null;
            }

            List<StackValue> svList = new List<StackValue>();
            int index = 0;

            var svArray = ArrayUtils.GetValues(sv, runtime.runtime.RuntimeCore);
            foreach (StackValue op in svArray)
            {
                if (!op.IsArray)
                {
                    if (!op.IsNull)
                    {
                        index++;
                        svList.Add(op);
                    }
                }
                else //op is an Array
                {
                    StackValue newArray = RemoveNulls(op, runtime);
                    svList.Add(newArray);
                }
            }
            if (svList.Count >= 0)
            {
                return runtime.runtime.rmem.Heap.AllocateArray(svList);
            }
            //That means an empty array
            return DSASM.StackValue.Null;
        }

        //RemoveIfNot
        internal static StackValue RemoveIfNot(StackValue sv1, StackValue sv2, ProtoCore.DSASM.Interpreter runtime)
        {
            RuntimeCore runtimeCore = runtime.runtime.RuntimeCore;
            if (!sv1.IsArray)
            {
                // Type mismatch.
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.kInvalidArguments, Resources.kInvalidArguments);
                return DSASM.StackValue.Null;
            }
            string typeString = FileIOBuiltIns.ConvertToString(sv2, runtime);
            List<StackValue> svList = new List<StackValue>();
            int index = 0;

            if (typeString == "array")
                typeString = ProtoCore.DSDefinitions.Keyword.Array;
            int type = runtimeCore.DSExecutable.TypeSystem.GetType(typeString);

            var svArray = ArrayUtils.GetValues(sv1, runtime.runtime.RuntimeCore);
            foreach (StackValue op in svArray)
            {
                if (op.metaData.type == type)
                {
                    index++;
                    svList.Add(op);
                }
            }
            if (svList.Count >= 0)
            {
                return runtime.runtime.rmem.Heap.AllocateArray(svList);
            }
            //That means an empty array
            return DSASM.StackValue.Null;
        }
        //Reverse
        internal static StackValue Reverse(StackValue sv, ProtoCore.DSASM.Interpreter runtime)
        {
            RuntimeCore runtimeCore = runtime.runtime.RuntimeCore;
            if (!sv.IsArray)
            {
                // Type mismatch.
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.kInvalidArguments, Resources.kInvalidArguments);
                return DSASM.StackValue.Null;
            }

            var reverseArray = ArrayUtils.GetValues(sv, runtime.runtime.RuntimeCore).Reverse().ToList();
            return runtime.runtime.rmem.Heap.AllocateArray(reverseArray);
        }
        //Contains & ArrayContainsArray ::: sv1 contains sv2
        internal static bool Contains(StackValue sv1, StackValue sv2, ProtoCore.DSASM.Interpreter runtime)
        {
            RuntimeCore runtimeCore = runtime.runtime.RuntimeCore;
            if (!sv1.IsArray)
            {
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.kInvalidArguments, Resources.kInvalidArguments);
                return false;
            }

            if (StackUtils.CompareStackValues(sv1, sv2, runtime.runtime.RuntimeCore))
            {
                return true;
            }

            bool contains = false;
            var svArray = ArrayUtils.GetValues(sv1, runtime.runtime.RuntimeCore);
            foreach (StackValue op in svArray)
            {
                if (!op.IsArray)
                {
                    if (op.Equals(sv2))
                        return true;
                }
                else
                {
                    contains = Contains(op, sv2, runtime);
                }

                if (contains) 
                {
                    return true;
                }

            }
            return contains;
        }
        internal static bool ContainsArray(StackValue sv1, StackValue sv2, ProtoCore.DSASM.Interpreter runtime)
        {
            RuntimeCore runtimeCore = runtime.runtime.RuntimeCore;
            if (!sv1.IsArray)
            {
                // Type mismatch.
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.kInvalidArguments, Resources.kInvalidArguments);
                return false;
            }
            bool contains = false;
            if (StackUtils.CompareStackValues(sv1, sv2, runtime.runtime.RuntimeCore)) 
                return true;

            var he = runtime.runtime.rmem.Heap.GetHeapElement(sv1);
            foreach (var op in he.VisibleItems)
            {
                if (!sv2.IsArray)
                {
                    if (!op.IsArray)
                    {
                        if (op.Equals(sv2))
                            return true;
                    }
                    else
                    {
                        contains = ContainsArray(op, sv2, runtime);
                    }
                    if (contains) return contains;
                }
                else
                {
                    if (op.IsArray)
                    {
                        contains = StackUtils.CompareStackValues(op, sv2, runtime.runtime.RuntimeCore);
                        if (!contains)
                        {
                            contains = ContainsArray(op, sv2, runtime);
                        }
                        if (contains) return contains;
                    }
                }
            }
            return contains;
        }
        //IndexOf & IOndexOfArray::: sv2 is index of sv1
        internal static int IndexOf(StackValue sv1, StackValue sv2, ProtoCore.DSASM.Interpreter runtime)
        {
            RuntimeCore runtimeCore = runtime.runtime.RuntimeCore;
            int notExist = -1;
            if (!sv1.IsArray)
            {
                // Type mismatch.
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.kInvalidArguments, Resources.kInvalidArguments);
                return notExist;
            }
            var svArray = ArrayUtils.GetValues(sv1, runtime.runtime.RuntimeCore);
            int sv1Length = svArray.Count();
            if ((sv1Length == 1) && StackUtils.CompareStackValues(sv1, sv2, runtime.runtime.RuntimeCore)) return 0;
            int index = 0; //index for sv1
            foreach (StackValue op in svArray)
            {
                if (sv2.Equals(op)) return index;
                index++;
            }
            return notExist;
        }
        internal static int ArrayIndexOfArray(StackValue sv1, StackValue sv2, ProtoCore.DSASM.Interpreter runtime)
        {
            RuntimeCore runtimeCore = runtime.runtime.RuntimeCore;
            int notExist = -1;
            if (!sv1.IsArray)
            {
                // Type mismatch.
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.kInvalidArguments, Resources.kInvalidArguments);
                return notExist;
            }
            var svArray = ArrayUtils.GetValues(sv1, runtime.runtime.RuntimeCore);
            int sv1Length = svArray.Count();
            if ((sv1Length == 1) && StackUtils.CompareStackValues(sv1, sv2, runtime.runtime.RuntimeCore)) return 0;
            int index = 0; //index for sv2
            foreach (StackValue op in svArray)
            {
                if (StackUtils.CompareStackValues(sv2, op, runtime.runtime.RuntimeCore)) 
                    return index;
                index++;
            }
            return notExist;
        }
        //Sort & SortWithMode
        internal static StackValue Sort(StackValue sv, ProtoCore.DSASM.Interpreter runtime)
        {
            //throw new NotImplementedException("LC urgent fix");
            return SortWithMode(sv, DSASM.StackValue.BuildBoolean(true), runtime);
        }
    
        internal static StackValue SortWithMode(StackValue sv, StackValue mode, ProtoCore.DSASM.Interpreter runtime)
        {
            RuntimeCore runtimeCore = runtime.runtime.RuntimeCore;
            if (!sv.IsArray || !mode.IsBoolean)
            {
                // Type mismatch.
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.kInvalidArguments, Resources.kInvalidArguments);
                return DSASM.StackValue.Null;
            }
                
            bool ascending = mode.opdata != 0;
            var svList = ArrayUtils.GetValues(sv, runtime.runtime.RuntimeCore).ToList();
            svList.Sort(new StackValueComparerForDouble(ascending));

            return runtime.runtime.rmem.Heap.AllocateArray(svList);
        }

        //SortIndexByValue & SortIndexByValueWithMode
        internal static StackValue SortIndexByValue(StackValue sv, ProtoCore.DSASM.Interpreter runtime)
        {
            //throw new NotImplementedException("LC Urgent");
            return SortIndexByValueWithMode(sv, DSASM.StackValue.BuildBoolean(true), runtime);
        }
        internal static StackValue SortIndexByValueWithMode(StackValue sv, StackValue mode, ProtoCore.DSASM.Interpreter runtime)
        {
            RuntimeCore runtimeCore = runtime.runtime.RuntimeCore;
            if (!sv.IsArray || !mode.IsBoolean)
            {
                // Type mismatch.
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.kInvalidArguments, Resources.kInvalidArguments);
                return DSASM.StackValue.Null;
            }
            bool ascending = mode.opdata != 0;
            var svArray = ArrayUtils.GetValues(sv, runtime.runtime.RuntimeCore);
            //That means an empty array
            if (!svArray.Any())
                return DSASM.StackValue.Null;
            List<KeyValuePair<StackValue, int>> svList = new List<KeyValuePair<StackValue, int>>();

            int index = 0;
            foreach (var element in svArray)
            {
                svList.Add(new KeyValuePair<StackValue, int>(element, index));
                index++;
            }

            StackValueComparerForDouble comparer = new StackValueComparerForDouble(ascending);
            svList.Sort((KeyValuePair<StackValue, int> x, KeyValuePair<StackValue, int> y) => comparer.Compare(x.Key, y.Key));

            StackValue[] sortedIndices = new StackValue[svList.Count];
            for (int n = 0; n < svList.Count; ++n)
            {
                StackValue tsv = DSASM.StackValue.BuildInt(svList[n].Value);
                sortedIndices[n] = tsv;
            }
            
            return runtime.runtime.rmem.Heap.AllocateArray(sortedIndices);
        }
        //Reorder
        internal static StackValue Reorder(StackValue sv1, StackValue sv2, ProtoCore.DSASM.Interpreter runtime)
        {
            RuntimeCore runtimeCore = runtime.runtime.RuntimeCore;
            if ((!sv1.IsArray) || (!sv2.IsArray))
            {
                // Type mismatch.
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.kInvalidArguments, Resources.kInvalidArguments);
                return DSASM.StackValue.Null;
            }
            if ((Rank(sv1, runtime) != 1) || (Rank(sv2, runtime) != 1))
            {
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.kInvalidArguments, Resources.kInvalidArguments);
                return DSASM.StackValue.Null;
            }
            int length1 = runtime.runtime.rmem.Heap.GetHeapElement(sv1).Stack.Length;
            int length2 = runtime.runtime.rmem.Heap.GetHeapElement(sv2).Stack.Length;
            if (length2 == 0) return DSASM.StackValue.Null;
            if (length1 < length2)
            {
                for (int n = length1; n < length2; n++)
                {
                    sv1 = Insert(sv1, DSASM.StackValue.Null, DSASM.StackValue.BuildInt(n), runtime);
                }
            }
            StackValue[] svArray = ArrayUtils.GetValues(sv1, runtime.runtime.RuntimeCore).ToArray();
            var svIdxArray = ArrayUtils.GetValues(sv2, runtime.runtime.RuntimeCore);
            List<StackValue> svList = new List<StackValue>();
            foreach (StackValue idx in svIdxArray)
            {
                if (!idx.IsInteger)
                {
                    return DSASM.StackValue.Null;
                    //Type Error: Argument(1) must be filled with integers!
                } 
                if (idx.opdata>=length1)
                {
                    return DSASM.StackValue.Null;
                    //Type Error: Out of array index bound!
                }
                svList.Add(svArray[idx.opdata]);
            }
            if (svList.Count >= 0)
            {
                return runtime.runtime.rmem.Heap.AllocateArray(svList);
            }
            //That means an empty array
            return DSASM.StackValue.Null;
        }
        //Insert
        internal static StackValue InsertArray(StackValue sv1, StackValue sv2, StackValue idx, ProtoCore.DSASM.Interpreter runtime)
        {
            RuntimeCore runtimeCore = runtime.runtime.RuntimeCore;
            if (!sv1.IsArray || !sv2.IsArray|| !idx.IsInteger)
            {
                // Type mismatch.
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.kInvalidArguments, Resources.kInvalidArguments);
                return DSASM.StackValue.Null;
            }

            return InsertCore(sv1, sv2, idx, runtime);
        }
        internal static StackValue Insert(StackValue sv1, StackValue sv2, StackValue idx, ProtoCore.DSASM.Interpreter runtime)
        {
            RuntimeCore runtimeCore = runtime.runtime.RuntimeCore;
            if (!sv1.IsArray || !idx.IsInteger)
            {
                // Type mismatch.
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.kInvalidArguments, Resources.kInvalidArguments);
                return DSASM.StackValue.Null;
            }

            return InsertCore(sv1, sv2, idx, runtime);
        }
        private static StackValue InsertCore(StackValue array, StackValue value, StackValue idx, ProtoCore.DSASM.Interpreter runtime)
        {
            int idxToBeInsert = (int)idx.opdata;

            List<StackValue> svList = new List<StackValue>();
            var elements = ArrayUtils.GetValues(array, runtime.runtime.RuntimeCore);
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

            return runtime.runtime.rmem.Heap.AllocateArray(svList);
        }
        //IsUniformDepth
        internal static bool IsUniformDepth(StackValue sv, ProtoCore.DSASM.Interpreter runtime)
        {
            RuntimeCore runtimeCore = runtime.runtime.RuntimeCore;
            if (!sv.IsArray)
            {
                // Type mismatch.
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.kInvalidArguments, Resources.kInvalidArguments);
                return true;
            }
            int overallRank = Rank(sv,runtime);
            if (overallRank == 1)
            {
                return true;
            }
            var svArray = ArrayUtils.GetValues(sv, runtime.runtime.RuntimeCore);
            foreach (StackValue element in svArray)
            {
                if (Rank(element, runtime) != (overallRank - 1))
                {
                    return false;
                }
            }
            return true;
        }
        //IsRectangular
        internal static bool IsRectangular(StackValue sv, ProtoCore.DSASM.Interpreter runtime)
        {
            if (2 != Rank(sv, runtime))
                return false;

            int count = -1;
            bool bCountInitialized = false;
            var svArray = ArrayUtils.GetValues(sv, runtime.runtime.RuntimeCore);
            foreach (var item in svArray)
            {
                if (1 != Rank(item, runtime))
                    return false;

                if (!bCountInitialized)
                {
                    count = Count(item, runtime);
                    bCountInitialized = true;
                }
                else if (count != Count(item, runtime))
                {
                    return false;
                }
            }

            return true;
        }
        //NomalizaeDepth & NomalizeDepthWithRank
        internal static StackValue NormalizeDepthWithRank(StackValue sv, StackValue r, ProtoCore.DSASM.Interpreter runtime)
        {
            RuntimeCore runtimeCore = runtime.runtime.RuntimeCore;
            if (!sv.IsArray)
            {
                // Type mismatch.
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.kInvalidArguments, Resources.kInvalidArguments);
                return DSASM.StackValue.Null;
            }
            int overallDepth = Rank(sv, runtime);
            int expectedDepth = (int)r.opdata;
            if (expectedDepth <= 0) return DSASM.StackValue.Null;
            if (expectedDepth == 1) return Flatten(sv, runtime);
            sv = Traverse(sv, expectedDepth, overallDepth, 0, runtime);
            return sv;
        }
        internal static StackValue NormalizeDepth(StackValue sv, ProtoCore.DSASM.Interpreter runtime)
        {
            RuntimeCore runtimeCore = runtime.runtime.RuntimeCore;
            if (!sv.IsArray)
            {
                // Type mismatch.
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.kInvalidArguments, Resources.kInvalidArguments);
                return DSASM.StackValue.Null;
            }
            int overallDepth = Rank(sv, runtime);
            sv = Traverse(sv, overallDepth, overallDepth, 0, runtime);
            return sv;
        }
        internal static StackValue Traverse(StackValue sv, int expectedRank, int overallRank, int UpRankOffset, ProtoCore.DSASM.Interpreter runtime)
        {
            var svArray = ArrayUtils.GetValues(sv, runtime.runtime.RuntimeCore);
            List<StackValue> svList = new List<StackValue>();
            foreach (StackValue element in svArray)
            {
                StackValue item;
                if (element.IsArray)
                {

                    if (UpRankOffset < (expectedRank - 2))
                    {
                        UpRankOffset++;
                        item = Traverse(element, expectedRank, overallRank, UpRankOffset, runtime);
                        UpRankOffset--;
                    }
                    else
                    {
                        item = Flatten(element, runtime);
                    } 
                    
                }
                else
                {
                    int braceNum = (expectedRank - UpRankOffset - 1);
                    item = upRank(element, braceNum, runtime);
                    //(expectedRank - count - 1) is got from ((expectedRank-overallRank)+(overallRank - count - 1))
                }

                svList.Add(item);
            }
            //Convert list to Operand
            if (svList.Count >= 0)
            {
                return runtime.runtime.rmem.Heap.AllocateArray(svList, null);
            }
            //That means an empty array
            return DSASM.StackValue.Null;
        }
        internal static StackValue upRank(StackValue sv, int countBraces, ProtoCore.DSASM.Interpreter runtime)
        {
            for (; countBraces > 0; countBraces--)
            {
                sv = runtime.runtime.rmem.Heap.AllocateArray(new StackValue[] { sv }, null);
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
            var svarr = ArrayUtils.GetValues(sv, runtime.runtime.RuntimeCore);
            int numOfCols = 0;
            int numOfRows = svarr.Count();
            foreach(StackValue element in svarr)
                if (element.IsArray)
                {
                    is2DArray = true;
                    numOfCols = Math.Max(heap.GetHeapElement(element).VisibleSize, numOfCols);
                }
            if (is2DArray == false)
                return sv;
            //By now the numCols and numRows are confirmed
            StackValue[,] original = new StackValue[numOfRows, numOfCols];
            for (int c1 = 0; c1 < numOfRows; c1++)
            {
                int c2 = 1;
                StackValue rowArray = heap.GetHeapElement(sv).Stack[c1];
                if (!rowArray.IsArray)
                    original[c1, 0] = rowArray;
                else
                {
                    var heapElement = heap.GetHeapElement(rowArray);
                    var items = heapElement.VisibleItems.ToList();
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
            List<StackValue> svList1 = new List<StackValue>(transposed.GetLength(0));
            for(int count1 = 0; count1 < transposed.GetLength(0); count1++)
            {
                //build an new item
                List<StackValue> svList2 = new List<StackValue>(transposed.GetLength(1));
                for (int count2 = 0; count2 < transposed.GetLength(1); count2++)
                {
                    StackValue element = transposed[count1, count2]; 
                    svList2.Add(element);
                }

                StackValue finalCol = heap.AllocateArray(svList2);
                svList1.Add(finalCol);
            }

            return heap.AllocateArray(svList1);
        }

        internal static StackValue SortPointers(StackValue svFunction, StackValue svArray, Interpreter runtime, StackFrame stackFrame)
        {
            RuntimeCore runtimeCore = runtime.runtime.RuntimeCore;
            var evaluator = new FunctionPointerEvaluator(svFunction, runtime);
            var svList = ArrayUtils.GetValues(svArray, runtime.runtime.RuntimeCore).ToList();
            Comparison<StackValue> comparer = (StackValue x, StackValue y) => 
            {
                List<StackValue> args = new List<StackValue>();
                args.Add(x);
                args.Add(y);
                StackValue ret;
                ret = evaluator.Evaluate(args, stackFrame);
                Validity.Assert(ret.IsNumeric);
                if (ret.IsDouble)
                    return (int)ret.RawDoubleValue;
                else
                    return (int)ret.RawIntValue;
            };

            try
            {
                svList.Sort(comparer);
            }
            catch (System.Exception e)
            {
                if (e.InnerException is Exceptions.CompilerInternalException)
                    runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.kAurgumentIsNotExpected, Resources.FailedToResolveSortingFunction);
                else if(e.InnerException != null)
                    runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.kAurgumentIsNotExpected, e.InnerException.Message);
                else
                    runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.kAurgumentIsNotExpected, e.Message);

                return StackValue.Null;
            }

            var heap = runtime.runtime.rmem.Heap;
            return heap.AllocateArray(svList);
        }

        internal static StackValue Evaluate(StackValue function, StackValue parameters, StackValue unpackParams, Interpreter runtime, StackFrame stackFrame)
        {
            var evaluator = new FunctionPointerEvaluator(function, runtime);

            StackValue ret;
            if (unpackParams.IsBoolean && unpackParams.RawBooleanValue)
            {
                var args = ArrayUtils.GetValues(parameters, runtime.runtime.RuntimeCore);
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

            var v1 = sv1.IsDouble ? sv1.RawDoubleValue : sv1.RawIntValue;
            var v2 = sv2.IsDouble ? sv2.RawDoubleValue : sv2.RawIntValue;

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

            var value1 = sv1.IsDouble ? sv1.RawDoubleValue : sv1.RawIntValue;
            var value2 = sv2.IsDouble ? sv2.RawDoubleValue : sv2.RawIntValue;

            double x = mbAscending ? value1 : value2;
            double y = mbAscending ? value2 : value1; 

            if (x > y)
                return 1;
            else
                return -1;
        }
    }
}
