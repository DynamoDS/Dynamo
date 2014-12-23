using System;
using System.Collections.Generic;
using System.Linq;

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
            get { return (int)GetAt(AbsoluteIndex.kClass).opdata; }
            set { SetAt(AbsoluteIndex.kClass, StackValue.BuildClassIndex(value)); }
        }

        public int FunctionScope
        {
            get { return (int)GetAt(AbsoluteIndex.kFunction).opdata; }
            set { SetAt(AbsoluteIndex.kFunction, StackValue.BuildFunctionIndex(value)); }
        }

        public int ReturnPC
        {
            get { return (int)GetAt(AbsoluteIndex.kReturnAddress).opdata; }
            set { SetAt(AbsoluteIndex.kReturnAddress, StackValue.BuildInt(value));}
        }

        public int FunctionBlock
        {
            get { return (int)GetAt(AbsoluteIndex.kFunctionBlock).opdata; }
            set { SetAt(AbsoluteIndex.kFunctionBlock, StackValue.BuildBlockIndex(value)); }
        }

        public int FunctionCallerBlock
        {
            get { return (int)GetAt(AbsoluteIndex.kFunctionCallerBlock).opdata; }
            set { SetAt(AbsoluteIndex.kFunctionCallerBlock, StackValue.BuildBlockIndex(value)); }
        }

        public StackFrameType CallerStackFrameType
        {
            get { return (StackFrameType)GetAt(AbsoluteIndex.kCallerStackFrameType).opdata; }
            set { SetAt(AbsoluteIndex.kCallerStackFrameType, StackValue.BuildInt((int)value));}
        }

        public StackFrameType StackFrameType
        {
            get { return (StackFrameType)GetAt(AbsoluteIndex.kStackFrameType).opdata; }
            set { SetAt(AbsoluteIndex.kStackFrameType, StackValue.BuildInt((int)value)); }
        }

        public int Depth
        {
            get { return (int)GetAt(AbsoluteIndex.kStackFrameDepth).opdata; }
            set { SetAt(AbsoluteIndex.kStackFrameDepth, StackValue.BuildInt(value)); }
        }

        public int FramePointer
        {
            get { return (int)GetAt(AbsoluteIndex.kFramePointer).opdata; }
            set { SetAt(AbsoluteIndex.kFramePointer, StackValue.BuildInt(value));}
        }

        public int ExecutionStateSize
        {
            get { return (int)GetAt(AbsoluteIndex.kExecutionStates).opdata; }
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
    
    public struct RTSymbol
    {
        public StackValue Sv;
        public int BlockId;
        public int[] Dimlist;
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
        public static bool CompareStackValues(StackValue sv1, StackValue sv2, Core core)
        {
            return CompareStackValues(sv1, sv2, core, core);
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
                    var value1 = sv1.RawDoubleValue;
                    var value2 = sv2.RawDoubleValue;

                    if(Double.IsInfinity(value1) && Double.IsInfinity(value2))
                        return true;
                    return MathUtils.Equals(value1, value2);
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
                    ClassNode classnode = c1.DSExecutable.classTable.ClassNodes[sv1.metaData.type];
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

        // heaper method to support negative index into stack
        public static StackValue GetValue(this HeapElement hs, int ix, Core core)
        {
            int index = ix < 0 ? ix + hs.VisibleSize : ix;
            if (index >= hs.VisibleSize || index < 0)
            {
                //throw new IndexOutOfRangeException();
                core.RuntimeStatus.LogWarning(ProtoCore.RuntimeData.WarningID.kOverIndexing, StringConstants.kArrayOverIndexed);
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
