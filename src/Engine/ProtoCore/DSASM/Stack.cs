using System;
using System.Collections.Generic;
using System.Linq;
using ProtoCore.Utils;

namespace ProtoCore.DSASM
{
    /// <summary>
    /// Stack frame type.
    /// </summary>
    public enum StackFrameType
    {
        Function,
        LanguageBlock,
    }

    /// <summary>
    /// Stack frame.
    /// </summary>
    public class StackFrame
    {
        public const int FrameIndexThisPtr = -1;
        public const int FrameIndexClassIndex = -2;
        public const int FrameIndexFunctionIndex = -3;
        public const int FrameIndexReturnAddress = -4;
        public const int FrameIndexFunctionBlockIndex = -5;
        public const int FrameIndexCallerBlockIndex = -6;
        public const int FrameIndexCallerStackFrameType = -7;
        public const int FrameIndexStackFrameType = -8;
        public const int FrameIndexStackFrameDepth = -9;
        public const int FrameIndexLocalVariableCount = -10;
        public const int FrameIndexExecutionStates = -11;
        public const int FrameIndexAX = -12;
        public const int FrameIndexBX = -13;
        public const int FrameIndexCX = -14;
        public const int FrameIndexDX = -15;
        public const int FrameIndexEX = -16;
        public const int FrameIndexFX = -17;
        public const int FrameIndexLX = -18;
        public const int FrameIndexRX = -19;
        public const int FrameIndexSX = -20;
        public const int FrameIndexTX = -21;
        public const int FrameIndexFramePointer = -22;
        public const int StackFrameSize = 22;

        private struct AbsoluteIndex
        {
            public const int ThisPtr = -FrameIndexThisPtr - 1;
            public const int ClassIndex = -FrameIndexClassIndex - 1;
            public const int FunctionIndex = -FrameIndexFunctionIndex - 1;
            public const int ReturnAddress = -FrameIndexReturnAddress - 1;
            public const int FunctionBlockIndex = -FrameIndexFunctionBlockIndex - 1;
            public const int CallerBlockIndex = -FrameIndexCallerBlockIndex - 1;
            public const int CallerStackFrameType = -FrameIndexCallerStackFrameType - 1;
            public const int StackFrameType = -FrameIndexStackFrameType - 1;
            public const int StackFrameDepth = -FrameIndexStackFrameDepth - 1;
            public const int LocalVariableCount  = -FrameIndexLocalVariableCount - 1;
            public const int ExecutionStates = -FrameIndexExecutionStates - 1;
            public const int AX = -FrameIndexAX - 1;
            public const int BX = -FrameIndexBX - 1;
            public const int CX = -FrameIndexCX - 1;
            public const int DX = -FrameIndexDX - 1;
            public const int EX = -FrameIndexEX - 1;
            public const int FX = -FrameIndexFX - 1;
            public const int LX = -FrameIndexLX - 1;
            public const int RX = -FrameIndexRX - 1;
            public const int SX = -FrameIndexSX - 1;
            public const int TX = -FrameIndexTX - 1;
            public const int FramePointer = -FrameIndexFramePointer - 1;
        }

        public StackValue[] Frame { get; set; }
        public StackValue[] ExecutionStates { get; set; }

        private void Init(
            StackValue thisPtr, 
            int classIndex, 
            int functionIndex, 
            int returnAddress, 
            int functionBlockIndex,
            int callerBlockIndex,
            StackFrameType callerStackFrameType,
            StackFrameType stackFrameType,
            int depth,
            int framePointer,
            List<StackValue> registers,
            List<bool> execStates)
        {
            Frame = new StackValue[StackFrameSize];

            Frame[AbsoluteIndex.ThisPtr] = thisPtr;
            Frame[AbsoluteIndex.ClassIndex] = StackValue.BuildClassIndex(classIndex);
            Frame[AbsoluteIndex.FunctionIndex] = StackValue.BuildFunctionIndex(functionIndex);
            Frame[AbsoluteIndex.ReturnAddress] = StackValue.BuildInt(returnAddress);
            Frame[AbsoluteIndex.FunctionBlockIndex] = StackValue.BuildBlockIndex(functionBlockIndex);
            Frame[AbsoluteIndex.CallerBlockIndex] = StackValue.BuildBlockIndex(callerBlockIndex);
            Frame[AbsoluteIndex.CallerStackFrameType] = StackValue.BuildFrameType((int)callerStackFrameType);
            Frame[AbsoluteIndex.StackFrameType] = StackValue.BuildFrameType((int)stackFrameType);
            Frame[AbsoluteIndex.StackFrameDepth] = StackValue.BuildInt(depth);
            Frame[AbsoluteIndex.LocalVariableCount] = StackValue.BuildInt(0);
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
            Frame[AbsoluteIndex.ExecutionStates] = StackValue.BuildInt(execStateSize);
            Frame[AbsoluteIndex.AX] = registers[0];
            Frame[AbsoluteIndex.BX] = registers[1];
            Frame[AbsoluteIndex.CX] = registers[2];
            Frame[AbsoluteIndex.DX] = registers[3];
            Frame[AbsoluteIndex.EX] = registers[4];
            Frame[AbsoluteIndex.FX] = registers[5];
            Frame[AbsoluteIndex.LX] = registers[6];
            Frame[AbsoluteIndex.RX] = registers[7];
            Frame[AbsoluteIndex.SX] = registers[8];
            Frame[AbsoluteIndex.TX] = registers[9];
            Frame[AbsoluteIndex.FramePointer] = StackValue.BuildInt(framePointer);
        }

        public StackFrame(
            StackValue thisPtr, 
            int classIndex,
            int functionIndex,
            int returnAddress,
            int functionBlockIndex,
            int callerBlockIndex,
            StackFrameType callerStackFrameType,
            StackFrameType stackFrameType,
            int depth,
            int framePointer,
            List<StackValue> registers,
            List<bool> execStates) 
        {
            Init(thisPtr, classIndex, functionIndex, returnAddress, functionBlockIndex, callerBlockIndex, callerStackFrameType, stackFrameType, depth, framePointer, registers, execStates);
        }

        public StackFrame(StackValue[] frame)
        {
            Validity.Assert(frame.Count() == StackFrameSize);
            Frame = frame;
        }

        public StackFrame(int globalOffset)
        {
            Init(StackValue.BuildPointer(Constants.kInvalidPointer), 
                Constants.kInvalidIndex,
                Constants.kInvalidIndex, 
                Constants.kInvalidIndex,
                Constants.kInvalidIndex,
                0, 
                StackFrameType.LanguageBlock,
                StackFrameType.LanguageBlock, 
                Constants.kInvalidIndex, 
                globalOffset,
                StackValue.BuildInvalidRegisters(), 
                new List<bool>());
        }

        private StackValue GetAt(int index)
        {
            return Frame[index];
        }

        private void SetAt(int index, StackValue sv)
        {
            Frame[index] = sv;
        }

        public StackValue ThisPtr
        {
            get { return GetAt(AbsoluteIndex.ThisPtr); }
            set { SetAt(AbsoluteIndex.ThisPtr, value);}
        }

        public int ClassScope
        {
            get { return (int)GetAt(AbsoluteIndex.ClassIndex).ClassIndex; }
            set { SetAt(AbsoluteIndex.ClassIndex, StackValue.BuildClassIndex(value)); }
        }

        public int FunctionScope
        {
            get { return (int)GetAt(AbsoluteIndex.FunctionIndex).FunctionIndex; }
            set { SetAt(AbsoluteIndex.FunctionIndex, StackValue.BuildFunctionIndex(value)); }
        }

        public int ReturnPC
        {
            get { return (int)GetAt(AbsoluteIndex.ReturnAddress).IntegerValue; }
            set { SetAt(AbsoluteIndex.ReturnAddress, StackValue.BuildInt(value));}
        }

        public int FunctionBlock
        {
            get { return (int)GetAt(AbsoluteIndex.FunctionBlockIndex).BlockIndex; }
            set { SetAt(AbsoluteIndex.FunctionBlockIndex, StackValue.BuildBlockIndex(value)); }
        }

        public int FunctionCallerBlock
        {
            get { return (int)GetAt(AbsoluteIndex.CallerBlockIndex).BlockIndex; }
            set { SetAt(AbsoluteIndex.CallerBlockIndex, StackValue.BuildBlockIndex(value)); }
        }

        public StackFrameType CallerStackFrameType
        {
            get { return GetAt(AbsoluteIndex.CallerStackFrameType).FrameType; }
            set { SetAt(AbsoluteIndex.CallerStackFrameType, StackValue.BuildFrameType((int)value));}
        }

        public StackFrameType StackFrameType
        {
            get { return GetAt(AbsoluteIndex.StackFrameType).FrameType; }
            set { SetAt(AbsoluteIndex.StackFrameType, StackValue.BuildFrameType((int)value)); }
        }

        public int Depth
        {
            get { return (int)GetAt(AbsoluteIndex.StackFrameDepth).IntegerValue; }
            set { SetAt(AbsoluteIndex.StackFrameDepth, StackValue.BuildInt(value)); }
        }

        public int FramePointer
        {
            get { return (int)GetAt(AbsoluteIndex.FramePointer).IntegerValue; }
            set { SetAt(AbsoluteIndex.FramePointer, StackValue.BuildInt(value));}
        }

        public int ExecutionStateSize
        {
            get { return (int)GetAt(AbsoluteIndex.ExecutionStates).IntegerValue; }
            set { SetAt(AbsoluteIndex.ExecutionStates, StackValue.BuildInt(value)); }
        }

        public StackValue AX
        {
            get { return GetAt(AbsoluteIndex.AX); }
            set { SetAt(AbsoluteIndex.AX, value);}
        }

        public StackValue BX
        {
            get { return GetAt(AbsoluteIndex.BX); }
            set { SetAt(AbsoluteIndex.BX, value);}
        }

        public StackValue CX
        {
            get { return GetAt(AbsoluteIndex.CX); }
            set { SetAt(AbsoluteIndex.CX, value); }
        }

        public StackValue DX
        {
            get { return GetAt(AbsoluteIndex.DX); }
            set { SetAt(AbsoluteIndex.DX, value); }
        }

        public StackValue EX
        {
            get { return GetAt(AbsoluteIndex.EX); }
            set { SetAt(AbsoluteIndex.EX, value); }
        }

        public StackValue FX
        {
            get { return GetAt(AbsoluteIndex.FX); }
            set { SetAt(AbsoluteIndex.FX, value); }
        }

        public StackValue LX
        {
            get { return GetAt(AbsoluteIndex.LX); }
            set { SetAt(AbsoluteIndex.LX, value); }
        }

        public StackValue RX
        {
            get { return GetAt(AbsoluteIndex.RX); }
            set { SetAt(AbsoluteIndex.RX, value); }
        }

        public StackValue SX
        {
            get { return GetAt(AbsoluteIndex.SX); }
            set { SetAt(AbsoluteIndex.SX, value); }
        }

        public StackValue TX
        {
            get { return GetAt(AbsoluteIndex.TX); }
            set { SetAt(AbsoluteIndex.TX, value); }
        }

        public List<StackValue> GetRegisters()
        {
            List<StackValue> registers = new List<StackValue>();

            registers.Add(Frame[(int)AbsoluteIndex.AX]);
            registers.Add(Frame[(int)AbsoluteIndex.BX]);
            registers.Add(Frame[(int)AbsoluteIndex.CX]);
            registers.Add(Frame[(int)AbsoluteIndex.DX]);
            registers.Add(Frame[(int)AbsoluteIndex.EX]);
            registers.Add(Frame[(int)AbsoluteIndex.FX]);
            registers.Add(Frame[(int)AbsoluteIndex.LX]);
            registers.Add(Frame[(int)AbsoluteIndex.RX]);
            registers.Add(Frame[(int)AbsoluteIndex.SX]);
            registers.Add(Frame[(int)AbsoluteIndex.TX]);

            return registers;
        }
    }
    
    public static class StackUtils
    {
        /// <summary>
        /// Deep comparison for two StackValue. 
        /// </summary>
        /// <param name="sv1"></param>
        /// <param name="sv2"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static bool CompareStackValues(StackValue sv1, StackValue sv2, RuntimeCore runtimeCore)
        {
            return CompareStackValues(sv1, sv2, runtimeCore, runtimeCore);
        }

        //this method compares the values of the stack variables passed
        public static bool CompareStackValues(StackValue sv1, StackValue sv2, RuntimeCore rtCore1, RuntimeCore rtCore2, ProtoCore.Runtime.Context context = null)
        {
            if (sv1.optype != sv2.optype)
                return false;

            switch (sv1.optype)
            {
                case AddressType.Invalid:
                    return true;
                case AddressType.Int:
                    return sv1.IntegerValue == sv2.IntegerValue;
                case AddressType.Char:
                    return sv1.CharValue == sv2.CharValue;
                case AddressType.Double:
                    var value1 = sv1.DoubleValue;
                    var value2 = sv2.DoubleValue;

                    if(Double.IsInfinity(value1) && Double.IsInfinity(value2))
                        return true;
                    return MathUtils.Equals(value1, value2);
                case AddressType.Boolean:
                    return sv1.BooleanValue == sv2.BooleanValue;
                case AddressType.ArrayPointer:
                    if (Object.ReferenceEquals(rtCore1, rtCore2) && sv1.ArrayPointer == sv2.ArrayPointer) //if both cores are same and the stack values point to the same heap element, then the stack values are equal
                        return true;

                    DSArray array1 = rtCore1.Heap.ToHeapObject<DSArray>(sv1);
                    DSArray array2 = rtCore2.Heap.ToHeapObject<DSArray>(sv2);
                    return DSArray.CompareFromDifferentCore(array1, array2, rtCore1, rtCore2, context);

                case AddressType.String:
                    if (Object.ReferenceEquals(rtCore1, rtCore2) && sv1.StringPointer == sv2.StringPointer) //if both cores are same and the stack values point to the same heap element, then the stack values are equal
                        return true;
                    DSString s1 = rtCore1.Heap.ToHeapObject<DSString>(sv1);
                    DSString s2 = rtCore1.Heap.ToHeapObject<DSString>(sv2);
                    return s1.Equals(s2);
                case AddressType.Pointer:
                    if (sv1.metaData.type != sv2.metaData.type) //if the type of class is different, then stack values can never be equal
                        return false;
                    if (Object.ReferenceEquals(rtCore1, rtCore2) && sv1.Pointer == sv2.Pointer) //if both cores are same and the stack values point to the same heap element, then the stack values are equal
                        return true;
                    ClassNode classnode = rtCore1.DSExecutable.classTable.ClassNodes[sv1.metaData.type];
                    if (classnode.IsImportedClass)
                    {
                        var helper = ProtoFFI.DLLFFIHandler.GetModuleHelper(ProtoFFI.FFILanguage.CSharp);
                        var marshaller1 = helper.GetMarshaller(rtCore1);
                        var marshaller2 = helper.GetMarshaller(rtCore2);
                        try
                        {
                            //the interpreter is passed as null as it is not expected to be sued while unmarshalling in this scenario
                            object dsObject1 = marshaller1.UnMarshal(sv1, context, null, typeof(Object));
                            object dsObject2 = marshaller2.UnMarshal(sv2, context, null, typeof(Object));
                            //cores are different in debugger nunit testing only. Most of the imported objects don't have implementation of Object.Equals. It
                            //also does not make sense to compare these objects deeply, as only dummy objects are created in nunit testing, and these dummy objects
                            //might have random values. So we just check whether the object tpye is the same for this testing.
                            if (!object.ReferenceEquals(rtCore1, rtCore2))
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
                        return ComparePointerFromHeap(sv1, sv2, rtCore1, rtCore2, context);
                    }
                default :
                    return sv1.Equals(sv2);
            }
        }

        //this method compares the heap for the stack variables and determines if the values of the heap are same
        private static bool ComparePointerFromHeap(StackValue sv1, StackValue sv2, RuntimeCore rtCore1, RuntimeCore rtCore2, ProtoCore.Runtime.Context context)
        {
            var obj1 = rtCore1.Heap.ToHeapObject<DSObject>(sv1);
            var obj2 = rtCore2.Heap.ToHeapObject<DSObject>(sv2);

            if (obj1.Count != obj2.Count)
            {
                return false;
            }

            for (int i = 0; i < obj1.Count; i++)
            {
                if (!CompareStackValues(obj1.GetValueFromIndex(i, rtCore1), 
                                        obj2.GetValueFromIndex(i, rtCore2), 
                                        rtCore1, rtCore2, context))
                {
                    return false;
                }
            }

            return true;
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
                mRmem.PopFrame(newStackCount - mRestorePoint);

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
