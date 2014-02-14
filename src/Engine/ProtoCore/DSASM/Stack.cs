using System;
using System.Collections.Generic;
using ProtoCore.Utils;

namespace ProtoCore.DSASM
{

    public enum StackFrameType
    {
        kTypeFunction = 0,
        kTypeLanguage,
        kTypeInvalid
    }

    public class StackFrame
    {
        public const int kFrameIndexThisPtr                 = -1;
        public const int kFrameIndexClass                   = -2;
        public const int kFrameIndexFunction                = -3;
        public const int kFrameIndexReturnAddress           = -4;
        public const int kFrameIndexFunctionBlock           = -5;
        public const int kFrameIndexFunctionCallerBlock     = -6;
        public const int kFrameIndexCallerStackFrameType    = -7;
        public const int kFrameIndexStackFrameType          = -8;
        public const int kFrameIndexStackFrameDepth         = -9;
        public const int kFrameIndexLocalVariables          = -10;
        public const int kFrameIndexExecutionStates         = -11;

        public const int kLastFrameIndex = kFrameIndexExecutionStates;

        // Relative idnex of each register
        public const int kFrameIndexRegisterAX = kLastFrameIndex - 1;
        public const int kFrameIndexRegisterBX = kLastFrameIndex - 2;
        public const int kFrameIndexRegisterCX = kLastFrameIndex - 3;
        public const int kFrameIndexRegisterDX = kLastFrameIndex - 4;
        public const int kFrameIndexRegisterEX = kLastFrameIndex - 5;
        public const int kFrameIndexRegisterFX = kLastFrameIndex - 6;
        public const int kFrameIndexRegisterLX = kLastFrameIndex - 7;
        public const int kFrameIndexRegisterRX = kLastFrameIndex - 8;
        public const int kFrameIndexRegisterSX = kLastFrameIndex - 9;
        public const int kFrameIndexRegisterTX = kLastFrameIndex - 10;

        // Relative index of the frame pointer
        public const int kFrameIndexFramePointer = -(kPointersSize + kRegistrySize);


        public enum AbsoluteIndex
        {
            kThisPtr = 0,
            kClass,
            kFunction,
            kReturnAddress,
            kFunctionBlock,
            kFunctionCallerBlock,
            kCallerStackFrameType,
            kStackFrameType,
            kStackFrameDepth,
            kLocalVariables,
            kExecutionStates,
            kRegisterAX,
            kRegisterBX,
            kRegisterCX,
            kRegisterDX,
            kRegisterEX,
            kRegisterFX,
            kRegisterLX,
            kRegisterRX,
            kRegisterSX,
            kRegisterTX,
            kFramePointer,
            kSize
        }


        public const int kRegistrySize = 10;
        public const int kPointersSize = 12;
        public const int kStackFrameSize = kPointersSize + kRegistrySize;

        public StackValue[] Frame { get; set; }
        public StackValue[] ExecutionStates { get; set; }

        private void Init(StackValue svThisPtr, int classIndex, int funcIndex, int pc, int functionBlockDecl, int functionBlockCaller, StackFrameType callerType, StackFrameType type, int depth,
           int framePointer, List<StackValue> stack, List<bool> execStates)
        {
            Validity.Assert((int)StackFrame.AbsoluteIndex.kSize == kStackFrameSize);

            Frame = new StackValue[kStackFrameSize];

            Frame[(int)AbsoluteIndex.kFramePointer] = StackValue.BuildInt(framePointer);
            Frame[(int)AbsoluteIndex.kStackFrameType] = StackValue.BuildFrameType((int)type);
            Frame[(int)AbsoluteIndex.kCallerStackFrameType] = StackValue.BuildFrameType((int)callerType);
            Frame[(int)AbsoluteIndex.kStackFrameDepth] = StackValue.BuildInt(depth);
            Frame[(int)AbsoluteIndex.kFunctionCallerBlock] = StackValue.BuildBlockIndex(functionBlockCaller);
            Frame[(int)AbsoluteIndex.kFunctionBlock] = StackValue.BuildBlockIndex(functionBlockDecl);
            Frame[(int)AbsoluteIndex.kReturnAddress] = StackValue.BuildInt(pc);
            Frame[(int)AbsoluteIndex.kFunction] = StackValue.BuildInt(funcIndex);
            Frame[(int)AbsoluteIndex.kClass] = StackValue.BuildInt(classIndex);
            Frame[(int)AbsoluteIndex.kThisPtr] = svThisPtr;

            Frame[(int)AbsoluteIndex.kRegisterAX] = stack[0];
            Frame[(int)AbsoluteIndex.kRegisterBX] = stack[1];
            Frame[(int)AbsoluteIndex.kRegisterCX] = stack[2];
            Frame[(int)AbsoluteIndex.kRegisterDX] = stack[3];
            Frame[(int)AbsoluteIndex.kRegisterEX] = stack[4];
            Frame[(int)AbsoluteIndex.kRegisterFX] = stack[5];
            Frame[(int)AbsoluteIndex.kRegisterLX] = stack[6];
            Frame[(int)AbsoluteIndex.kRegisterRX] = stack[7];
            Frame[(int)AbsoluteIndex.kRegisterSX] = stack[8];
            Frame[(int)AbsoluteIndex.kRegisterTX] = stack[9];

            int execStateSize = 0;
            if (null != execStates)
            {
                execStateSize = execStates.Count;
                ExecutionStates = new StackValue[execStateSize];
                for (int n = 0; n < execStateSize; ++n)
                {
                    ExecutionStates[n] = StackValue.BuildBoolean(execStates[n]);
                }
            }

            Frame[(int)AbsoluteIndex.kExecutionStates] = StackValue.BuildInt(execStateSize);
            Frame[(int)AbsoluteIndex.kLocalVariables] = StackValue.BuildInt(0);
            
            Validity.Assert(kStackFrameSize == Frame.Length);
        }

        public StackFrame(StackValue svThisPtr, int classIndex, int funcIndex, int pc, int functionBlockDecl, int functionBlockCaller, StackFrameType callerType, StackFrameType type, int depth, int framePointer, List<StackValue> stack, List<bool> execStates) 
        {
            Init(svThisPtr, classIndex, funcIndex, pc, functionBlockDecl, functionBlockCaller, callerType, type, depth, framePointer, stack, execStates);
        }

        public StackFrame(int globalOffset)
        {
            StackValue svThisPtr = ProtoCore.DSASM.StackValue.BuildPointer(ProtoCore.DSASM.Constants.kInvalidPointer);
            int ci = ProtoCore.DSASM.Constants.kInvalidIndex;
            int fi = ProtoCore.DSASM.Constants.kInvalidIndex;
            int returnAddr = ProtoCore.DSASM.Constants.kInvalidIndex;
            int blockDecl = ProtoCore.DSASM.Constants.kInvalidIndex;
            int blockCaller = 0;

            StackFrameType callerType = StackFrameType.kTypeLanguage;
            StackFrameType type = StackFrameType.kTypeLanguage;
            int depth = -1;
            int framePointer = globalOffset;

            Init(svThisPtr, ci, fi, returnAddr, blockDecl, blockCaller, callerType, type, depth, framePointer, StackValue.BuildInvalidRegisters(), new List<bool>());
        }

        public StackValue GetAt(AbsoluteIndex index)
        {
            Validity.Assert(null != Frame);
            return Frame[(int)index];
        }

        public List<StackValue> GetRegisters()
        {
            List<StackValue> registers = new List<StackValue>();

            registers.Add(Frame[(int)AbsoluteIndex.kRegisterAX]);
            registers.Add(Frame[(int)AbsoluteIndex.kRegisterBX]);
            registers.Add(Frame[(int)AbsoluteIndex.kRegisterCX]);
            registers.Add(Frame[(int)AbsoluteIndex.kRegisterDX]);
            registers.Add(Frame[(int)AbsoluteIndex.kRegisterEX]);
            registers.Add(Frame[(int)AbsoluteIndex.kRegisterFX]);
            registers.Add(Frame[(int)AbsoluteIndex.kRegisterLX]);
            registers.Add(Frame[(int)AbsoluteIndex.kRegisterRX]);
            registers.Add(Frame[(int)AbsoluteIndex.kRegisterSX]);
            registers.Add(Frame[(int)AbsoluteIndex.kRegisterTX]);

            return registers;
        }

        public void SetAt(AbsoluteIndex index, StackValue sv)
        {
            Validity.Assert(null != Frame);
            Frame[(int)index] = sv;
        }
    }
    
    public struct RTSymbol
    {
        public StackValue Sv;
        public int BlockId;
        public int[] Dimlist;
    }
    
    public static class StackUtils
    {
        public static bool IsTrue(StackValue operand)
        {
            return operand.optype == AddressType.Boolean &&
                   operand.opdata != 0;
        }

        public static bool IsArray(StackValue operand)
        {
            return operand.optype == AddressType.ArrayPointer;
        }

        public static bool IsNull(StackValue operand)
        {
            return operand.optype == AddressType.Null;
        }

        public static bool IsString(StackValue operand)
        {
            return operand.optype == AddressType.String;
        }

        public static bool IsNumeric(StackValue operand)
        {
            return operand.optype == AddressType.Int ||
                   operand.optype == AddressType.Double;
        }

        public static bool IsValidPointer(StackValue operand)
        {
            return operand.optype == AddressType.Pointer &&
                   operand.opdata != ProtoCore.DSASM.Constants.kInvalidIndex;
        }

        public static StackValue AsBoolean(this StackValue operand, Core core)
        {
            switch (operand.optype)
            {
                case AddressType.Boolean:
                case AddressType.Int:
                    return StackValue.BuildBoolean(operand.opdata != 0);
                case AddressType.Null:
                    return StackValue.Null; //BuildBoolean(false);
                case AddressType.Double:
                    bool b = !(Double.IsNaN(operand.opdata_d) || operand.opdata_d.Equals(0.0));
                    return StackValue.BuildBoolean(b);
                case AddressType.Pointer:
                    return StackValue.BuildBoolean(true);
                case AddressType.String:
                    if (ArrayUtils.GetElementSize(operand, core) == 0)
                    {
                        return StackValue.BuildBoolean(false);
                    }
                    return StackValue.BuildBoolean(true);

                case AddressType.Char:
                    if (EncodingUtils.ConvertInt64ToCharacter(operand.opdata)==0)
                    {
                        return StackValue.BuildBoolean(false);
                    }
                    return StackValue.BuildBoolean(true); 
                default:
                    return StackValue.Null;
            }
        }

        public static StackValue AsDouble(this StackValue operand)
        {
            switch (operand.optype)
            {
                case AddressType.Int:
                    return StackValue.BuildDouble(operand.opdata);
                case AddressType.Double:
                    return StackValue.BuildDouble(operand.opdata_d);
                default:
                    return StackValue.Null;
            }
        }

        public static StackValue AsInt(this StackValue operand)
        {
            switch (operand.optype)
            {
                case AddressType.Int:
                    return operand;
                case AddressType.Double:
                    return StackValue.BuildInt((Int64)Math.Round(operand.opdata_d, 0, MidpointRounding.AwayFromZero));
                default:
                    return StackValue.Null;
            }
        }

        public static bool IsReferenceType(this StackValue operand)
        {
            return (operand.optype == AddressType.ArrayPointer ||
                    operand.optype == AddressType.Pointer ||
                    operand.optype == AddressType.String) && ProtoCore.DSASM.Constants.kInvalidIndex != operand.opdata;
        }

        //this method compares the values of the stack variables passed
        public static bool CompareStackValues(StackValue sv1, StackValue sv2, Core c1, Core c2, ProtoCore.Runtime.Context context = null)
        {
            if (sv1.optype != sv2.optype)
                return false;
            switch (sv1.optype)
            {
                case AddressType.Int:
                case AddressType.Char:
                    return sv1.opdata == sv2.opdata;
                case AddressType.Double:
                    if(Double.IsInfinity(sv1.opdata_d) && Double.IsInfinity(sv2.opdata_d))
                        return true;
                    return MathUtils.Equals(sv1.opdata_d, sv2.opdata_d);
                case AddressType.Boolean:
                    return (sv1.opdata > 0 && sv2.opdata > 0) || (sv1.opdata == 0 && sv2.opdata == 0);
                case AddressType.ArrayPointer:
                case AddressType.String:
                    if (Object.ReferenceEquals(c1,c2) && sv1.opdata == sv2.opdata) //if both cores are same and the stack values point to the same heap element, then the stack values are equal
                        return true;
                    return CompareStackValuesFromHeap(sv1, sv2, c1, c2, context);
                case AddressType.Pointer:
                    if (sv1.metaData.type != sv2.metaData.type) //if the type of class is different, then stack values can never be equal
                        return false;
                    if (Object.ReferenceEquals(c1, c2) && sv1.opdata == sv2.opdata) //if both cores are same and the stack values point to the same heap element, then the stack values are equal
                        return true;
                    ClassNode classnode = c1.DSExecutable.classTable.ClassNodes[(int)sv1.metaData.type];
                    if (classnode.IsImportedClass)
                    {
                        var helper = ProtoFFI.DLLFFIHandler.GetModuleHelper(ProtoFFI.FFILanguage.CSharp);
                        var marshaller1 = helper.GetMarshaller(c1);
                        var marshaller2 = helper.GetMarshaller(c2);
                        try
                        {
                            //the interpreter is passed as null as it is not expected to be sued while unmarshalling in this scenario
                            object dsObject1 = marshaller1.UnMarshal(sv1, context, null, typeof(Object));
                            object dsObject2 = marshaller2.UnMarshal(sv2, context, null, typeof(Object));
                            //cores are different in debugger nunit testing only. Most of the imported objects don't have implementation of Object.Equals. It
                            //also does not make sense to compare these objects deeply, as only dummy objects are created in nunit testing, and these dummy objects
                            //might have random values. So we just check whether the object tpye is the same for this testing.
                            if (!object.ReferenceEquals(c1, c2))
                                return Object.ReferenceEquals(dsObject1.GetType(), dsObject2.GetType());

                            return Object.Equals(dsObject1, dsObject2);
                        }
                        catch (System.Exception)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return CompareStackValuesFromHeap(sv1, sv2, c1, c2, context);
                    }
                default :
                    return sv1.opdata == sv2.opdata;
            }
        }

        //this method compares the heap for the stack variables and determines if the values of the heap are same
        private static bool CompareStackValuesFromHeap(StackValue sv1, StackValue sv2, Core c1, Core c2, ProtoCore.Runtime.Context context)
        {
            HeapElement heap1 = ArrayUtils.GetHeapElement(sv1, c1); 
            HeapElement heap2 = ArrayUtils.GetHeapElement(sv2, c2);

            if (heap1.Stack.Length != heap2.Stack.Length)
            {
                return false;
            }

            for (int i = 0; i < heap1.Stack.Length; i++)
            {
                if (!CompareStackValues(heap1.Stack[i], heap2.Stack[i], c1, c2, context))
                {
                    return false;
                }
            }

            if (heap1.Dict != null && heap2.Dict != null)
            {
                if (heap1.Dict == heap2.Dict)
                {
                    return true;
                }

                foreach (var key in heap1.Dict.Keys)
                {
                    StackValue value1 = heap1.Dict[key];
                    StackValue value2 = StackValue.Null;
                    if (!heap2.Dict.TryGetValue(key, out value2))
                    {
                        return false;
                    }

                    if (!CompareStackValues(value1, value2, c1, c2))
                    {
                        return false;
                    }
                }

                return true;
            }
            else if (heap1.Dict == null && heap2.Dict == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool Equals(this StackValue lhs, StackValue rhs)
        {
            if (lhs.optype != rhs.optype)
            {
                return false;
            }

            switch (lhs.optype)
            {
                case AddressType.Int:
                case AddressType.Char:
                    return lhs.opdata == rhs.opdata;

                case AddressType.Double:
                    return MathUtils.Equals(lhs.opdata_d, rhs.opdata_d);

                case AddressType.Boolean:
                    return (lhs.opdata > 0 && rhs.opdata > 0) || (lhs.opdata == 0 && rhs.opdata ==0);
                case AddressType.Pointer:
                    return lhs.opdata == rhs.opdata && lhs.metaData.type == rhs.metaData.type;
                default:
                    return lhs.opdata == rhs.opdata;
            }
        }

        // heaper method to support negative index into stack
        public static StackValue GetValue(this HeapElement hs, int ix, Core core)
        {
            int index = ix < 0 ? ix + hs.VisibleSize : ix;
            if (index >= hs.VisibleSize || index < 0)
            {
                //throw new IndexOutOfRangeException();
                core.RuntimeStatus.LogWarning(ProtoCore.RuntimeData.WarningID.kOverIndexing, RuntimeData.WarningMessage.kArrayOverIndexed);
                return StackValue.Null;
            }

            return hs.Stack[index];
        }

        // helper method to support negative index into stack
        public static StackValue SetValue(this HeapElement hs, int ix, StackValue sv)
        {
            if (ix >= hs.GetAllocatedSize())
            {
                throw new System.IndexOutOfRangeException();
            }

            int index = ix < 0 ? ix + hs.VisibleSize : ix;
            StackValue svOld = hs.Stack[index];
            hs.Stack[index] = sv;

            return svOld;
        }
    };

    public class StackAlignToFramePointerRestorer
    {
        private List<StackValue> mStack = new List<StackValue>();
        private int mRestorePoint = 0;
        private ProtoCore.Runtime.RuntimeMemory mRmem = null;

        public void Align(ProtoCore.Runtime.RuntimeMemory rMem)
        {
            if (null == rMem)
                return;

            mRestorePoint = 0;
            mStack.Clear();

            mRestorePoint = rMem.Stack.Count;

            //Record the stack elements which is above the frame pointer
            int nDiff = mRestorePoint - rMem.FramePointer;
            for (int i = 0; i < nDiff; i++)
            {
                mStack.Add(rMem.Pop());
            }

            mRestorePoint = rMem.Stack.Count;
            mRmem = rMem;

            return;
        }

        public bool Restore()
        {
            if (null == mRmem)
                return false;

            //Pop the addtional stack elements added
            int newStackCount = mRmem.Stack.Count;
            if (newStackCount > mRestorePoint)
                mRmem.Pop(newStackCount - mRestorePoint);

            //Restore the old stack elements that is above the frame pointer
            int count = mStack.Count;
            for (int i = 0; i < count; i++)
            {
                mRmem.Push(mStack[count - 1 - i]);
            }

            return true;
        }
    }
}
