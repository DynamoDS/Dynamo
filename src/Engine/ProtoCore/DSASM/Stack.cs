using System;
using System.Collections.Generic;
using System.Linq;

using ProtoCore.Utils;
using ProtoCore.Properties;

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

            Frame[(int)AbsoluteIndex.kThisPtr] = svThisPtr;
            Frame[(int)AbsoluteIndex.kClass] = StackValue.BuildClassIndex(classIndex);
            Frame[(int)AbsoluteIndex.kFunction] = StackValue.BuildFunctionIndex(funcIndex);
            Frame[(int)AbsoluteIndex.kReturnAddress] = StackValue.BuildInt(pc);
            Frame[(int)AbsoluteIndex.kFunctionBlock] = StackValue.BuildBlockIndex(functionBlockDecl);
            Frame[(int)AbsoluteIndex.kFunctionCallerBlock] = StackValue.BuildBlockIndex(functionBlockCaller);
            Frame[(int)AbsoluteIndex.kCallerStackFrameType] = StackValue.BuildFrameType((int)callerType);
            Frame[(int)AbsoluteIndex.kStackFrameType] = StackValue.BuildFrameType((int)type);
            Frame[(int)AbsoluteIndex.kStackFrameDepth] = StackValue.BuildInt(depth);
            Frame[(int)AbsoluteIndex.kLocalVariables] = StackValue.BuildInt(0);
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
            Frame[(int)AbsoluteIndex.kFramePointer] = StackValue.BuildInt(framePointer);
            
            Validity.Assert(kStackFrameSize == Frame.Length);
        }

        public StackFrame(StackValue svThisPtr, int classIndex, int funcIndex, int pc, int functionBlockDecl, int functionBlockCaller, StackFrameType callerType, StackFrameType type, int depth, int framePointer, List<StackValue> stack, List<bool> execStates) 
        {
            Init(svThisPtr, classIndex, funcIndex, pc, functionBlockDecl, functionBlockCaller, callerType, type, depth, framePointer, stack, execStates);
        }

        public StackFrame(StackValue[] frame)
        {
            Validity.Assert(frame.Count() == kStackFrameSize);
            Frame = frame;
        }

        public StackFrame(int globalOffset)
        {
            StackValue svThisPtr = ProtoCore.DSASM.StackValue.BuildPointer(Constants.kInvalidPointer);
            int ci = Constants.kInvalidIndex;
            int fi = Constants.kInvalidIndex;
            int returnAddr = Constants.kInvalidIndex;
            int blockDecl = Constants.kInvalidIndex;
            int blockCaller = 0;

            StackFrameType callerType = DSASM.StackFrameType.kTypeLanguage;
            StackFrameType type = DSASM.StackFrameType.kTypeLanguage;
            int depth = -1;
            int framePointer = globalOffset;

            Init(svThisPtr, ci, fi, returnAddr, blockDecl, blockCaller, callerType, type, depth, framePointer, StackValue.BuildInvalidRegisters(), new List<bool>());
        }

        private StackValue GetAt(AbsoluteIndex index)
        {
            return Frame[(int)index];
        }

        private void SetAt(AbsoluteIndex index, StackValue sv)
        {
            Frame[(int)index] = sv;
        }

        public StackValue ThisPtr
        {
            get { return GetAt(AbsoluteIndex.kThisPtr); }
            set { SetAt(AbsoluteIndex.kThisPtr, value);}
        }

        public int ClassScope
        {
            get { return (int)GetAt(AbsoluteIndex.kClass).ClassIndex; }
            set { SetAt(AbsoluteIndex.kClass, StackValue.BuildClassIndex(value)); }
        }

        public int FunctionScope
        {
            get { return (int)GetAt(AbsoluteIndex.kFunction).FunctionIndex; }
            set { SetAt(AbsoluteIndex.kFunction, StackValue.BuildFunctionIndex(value)); }
        }

        public int ReturnPC
        {
            get { return (int)GetAt(AbsoluteIndex.kReturnAddress).IntegerValue; }
            set { SetAt(AbsoluteIndex.kReturnAddress, StackValue.BuildInt(value));}
        }

        public int FunctionBlock
        {
            get { return (int)GetAt(AbsoluteIndex.kFunctionBlock).BlockIndex; }
            set { SetAt(AbsoluteIndex.kFunctionBlock, StackValue.BuildBlockIndex(value)); }
        }

        public int FunctionCallerBlock
        {
            get { return (int)GetAt(AbsoluteIndex.kFunctionCallerBlock).BlockIndex; }
            set { SetAt(AbsoluteIndex.kFunctionCallerBlock, StackValue.BuildBlockIndex(value)); }
        }

        public StackFrameType CallerStackFrameType
        {
            get { return GetAt(AbsoluteIndex.kCallerStackFrameType).FrameType; }
            set { SetAt(AbsoluteIndex.kCallerStackFrameType, StackValue.BuildFrameType((int)value));}
        }

        public StackFrameType StackFrameType
        {
            get { return GetAt(AbsoluteIndex.kStackFrameType).FrameType; }
            set { SetAt(AbsoluteIndex.kStackFrameType, StackValue.BuildFrameType((int)value)); }
        }

        public int Depth
        {
            get { return (int)GetAt(AbsoluteIndex.kStackFrameDepth).IntegerValue; }
            set { SetAt(AbsoluteIndex.kStackFrameDepth, StackValue.BuildInt(value)); }
        }

        public int FramePointer
        {
            get { return (int)GetAt(AbsoluteIndex.kFramePointer).IntegerValue; }
            set { SetAt(AbsoluteIndex.kFramePointer, StackValue.BuildInt(value));}
        }

        public int ExecutionStateSize
        {
            get { return (int)GetAt(AbsoluteIndex.kExecutionStates).IntegerValue; }
            set { SetAt(AbsoluteIndex.kExecutionStates, StackValue.BuildInt(value)); }
        }

        public StackValue AX
        {
            get { return GetAt(AbsoluteIndex.kRegisterAX); }
            set { SetAt(AbsoluteIndex.kRegisterAX, value);}
        }

        public StackValue BX
        {
            get { return GetAt(AbsoluteIndex.kRegisterBX); }
            set { SetAt(AbsoluteIndex.kRegisterBX, value);}
        }

        public StackValue CX
        {
            get { return GetAt(AbsoluteIndex.kRegisterCX); }
            set { SetAt(AbsoluteIndex.kRegisterCX, value); }
        }

        public StackValue DX
        {
            get { return GetAt(AbsoluteIndex.kRegisterDX); }
            set { SetAt(AbsoluteIndex.kRegisterDX, value); }
        }

        public StackValue EX
        {
            get { return GetAt(AbsoluteIndex.kRegisterEX); }
            set { SetAt(AbsoluteIndex.kRegisterEX, value); }
        }

        public StackValue FX
        {
            get { return GetAt(AbsoluteIndex.kRegisterFX); }
            set { SetAt(AbsoluteIndex.kRegisterFX, value); }
        }

        public StackValue LX
        {
            get { return GetAt(AbsoluteIndex.kRegisterLX); }
            set { SetAt(AbsoluteIndex.kRegisterLX, value); }
        }

        public StackValue RX
        {
            get { return GetAt(AbsoluteIndex.kRegisterRX); }
            set { SetAt(AbsoluteIndex.kRegisterRX, value); }
        }

        public StackValue SX
        {
            get { return GetAt(AbsoluteIndex.kRegisterSX); }
            set { SetAt(AbsoluteIndex.kRegisterSX, value); }
        }

        public StackValue TX
        {
            get { return GetAt(AbsoluteIndex.kRegisterTX); }
            set { SetAt(AbsoluteIndex.kRegisterTX, value); }
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
