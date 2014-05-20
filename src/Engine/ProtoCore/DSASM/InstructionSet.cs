using System;
using System.Collections.Generic;
using Operand = ProtoCore.DSASM.StackValue;

namespace ProtoCore.DSASM
{
    public enum Registers
    {
        AX = 0,
        BX = 1,
        CX = 2,
        DX = 3,
        EX = 4,
        FX = 5,
        LX = 6,
        RX = 7,
        SX = 8,
        TX = 9
    }

    public enum AddressType
    {
        Invalid,
        Register,
        VarIndex,
        FunctionIndex,
        MemVarIndex,
        StaticMemVarIndex,
        ClassIndex,
        Int,
        Double,
        Boolean,
        Char,
        String,
        LabelIndex,
        BlockIndex,
        Pointer,
        ArrayPointer,
        FunctionPointer,
        Null,
        DefaultArg,
        ArrayDim,
        ReplicationGuide,
        Dynamic,
        StaticType,
        CallingConvention,
        FrameType,
        ThisPtr,
        ExplicitCall,
        ArrayKey
    }

    public enum OpCode
    {
        NONE,
        MOV,
        ADD,
        SUB,
        MUL,
        DIV,
        MOD,
        PUSH,
        PUSHG,
        PUSHM,
        PUSHW,
        PUSHDEP,
        PUSHINDEX,
        POP,
        POPW,
        POPG,
        CALL,
        CALLR,
        RETURN,
        END,
        JMP,
        CJMP,
        JMP_EQ,
        JMP_NEQ,
        JMP_GT,
        JMP_LT,
        JMP_GTEQ,
        JMP_LTEQ,
        JLZ,
        JGZ,
        JZ,
        AND,
        OR,
        NOT,
        EQ,
        NQ,
        GT,
        LT,
        GE,
        LE,
        BOUNCE,
        ALLOC,
        ALLOCA,
        ALLOCC,
        ALLOCM,
        POPM,
        CALLC,
        POPLIST,
        PUSHLIST,
        RETC,
        RETB,
        RETCN,
        BITAND,
        BITOR,
        BITXOR,
        NEGATE,
        NEG,
        CAST,
        DEP,
        DEPX,
        PUSHB,
        POPB,

        THROW,
        // TODO Jun: This is temporary until the lib system is implemented. 
        PUSH_ARRAYKEY,
        SETEXPUID
    }

    public struct MetaData
    {
        public int type;
    }

    [System.Diagnostics.DebuggerDisplay("{optype}, opdata = {opdata}, metaData = {metaData.type}")]
    public struct StackValue
    {
        public Int64 opdata;
        public AddressType optype;
        public MetaData metaData;

        public StackValue ShallowClone()
        {
            StackValue newSv = new StackValue();
            newSv.optype = optype;
            newSv.opdata = opdata;
            newSv.metaData = new MetaData { type = metaData.type };

            return newSv;
        }

        public override bool Equals(object other)
        {
            if (!(other is StackValue))
                return false;

            StackValue rhs = (StackValue)other;
            if (this.optype != rhs.optype || this.metaData.type != rhs.metaData.type)
                return false;

            if (this.IsDouble())
                return this.RawDoubleValue.Equals(rhs.RawDoubleValue);
            else
                return this.opdata == rhs.opdata;
        }

        public override string ToString()
        {
            if (IsDouble())
            {
                return RawDoubleValue.ToString();
            }
            else if (IsInteger())
            {
                return RawIntValue.ToString();
            }
            else
                return String.Format("{0}, opdata = {1}, metaData = {2}", optype.ToString(), opdata.ToString(),
                                     metaData.type.ToString());

        }

        /// <summary>
        /// Get integer value without checking its type or do type conversion,
        /// so the StackValue shoule be boolean typed.
        /// </summary>
        public Int64 RawIntValue
        {
            get
            {
                return opdata;
            }
        }

        /// <summary>
        /// Get double value without checking its type or do type conversion. 
        /// The StackValue should be double typed. 
        /// </summary>
        public double RawDoubleValue
        {
            get
            {
                return BitConverter.Int64BitsToDouble(opdata);
            }
        }

        /// <summary>
        /// Get boolean value without checking its type or do type conversion,
        /// so the StackValue shoule be boolean typed.
        /// </summary>
        public bool RawBooleanValue
        {
            get
            {
                return opdata != 0;
            }
        }

        #region Some constant values
        public static StackValue Null = BuildNull();
        public static StackValue True = BuildBoolean(true);
        public static StackValue False = BuildBoolean(false);
        #endregion

        #region Type checkers
        public bool IsInvalid()
        {
            return optype == AddressType.Invalid;
        }

        public bool IsRegister()
        {
            return optype == AddressType.Register;
        }

        public bool IsVariableIndex()
        {
            return optype == AddressType.VarIndex;
        }

        public bool IsFunctionIndex()
        {
            return optype == AddressType.FunctionIndex;
        }

        public bool IsMemberVariableIndex()
        {
            return optype == AddressType.MemVarIndex;
        }

        public bool IsStaticVariableIndex()
        {
            return optype == AddressType.StaticMemVarIndex;
        }

        public bool IsClassIndex()
        {
            return optype == AddressType.ClassIndex;
        }

        public bool IsInteger()
        {
            return optype == AddressType.Int;
        }

        public bool IsDouble()
        {
            return optype == AddressType.Double;
        }

        public bool IsNumeric()
        {
            return optype == AddressType.Int || optype == AddressType.Double;
        }

        public bool IsBoolean()
        {
            return optype == AddressType.Boolean;
        }

        public bool IsChar()
        {
            return optype == AddressType.Char;
        }

        public bool IsString()
        {
            return optype == AddressType.String;
        }

        public bool IsLabelIndex()
        {
            return optype == AddressType.LabelIndex;
        }

        public bool IsBlockIndex()
        {
            return optype == AddressType.BlockIndex;
        }

        public bool IsObject()
        {
            return optype == AddressType.Pointer;
        }

        public bool IsArray()
        {
            return optype == AddressType.ArrayPointer;
        }

        public bool IsFunctionPointer()
        {
            return optype == AddressType.FunctionPointer;
        }

        public bool IsNull()
        {
            return optype == AddressType.Null;
        }

        public bool IsDefaultArgument()
        {
            return optype == AddressType.DefaultArg;
        }

        public bool IsArrayDimension()
        {
            return optype == AddressType.ArrayDim;
        }

        public bool IsReplicationGuide()
        {
            return optype == AddressType.ReplicationGuide;
        }

        public bool IsDynamic()
        {
            return optype == AddressType.Dynamic;
        }

        public bool IsStaticType()
        {
            return optype == AddressType.StaticType;
        }

        public bool IsCallingConvention()
        {
            return optype == AddressType.CallingConvention;
        }

        public bool IsFrameType()
        {
            return optype == AddressType.FrameType;
        }

        public bool IsThisPtr()
        {
            return optype == AddressType.ThisPtr;
        }

        public bool IsExplicitCall()
        {
            return optype == AddressType.ExplicitCall;
        }

        public bool IsArrayKey()
        {
            return optype == AddressType.ArrayKey;
        }

        public bool IsReferenceType()
        {
            return opdata != Constants.kInvalidIndex && (IsArray() || IsObject() || IsString());
        }
        #endregion

        #region Builders
        public static StackValue BuildInvalid()
        {
            StackValue value = new StackValue();
            value.optype = AddressType.Invalid;
            value.opdata = Constants.kInvalidIndex;
            return value;
        }

        public static StackValue BuildInt(Int64 data)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.Int;
            value.opdata = data;

            MetaData mdata = new MetaData();
            mdata.type = (int)PrimitiveType.kTypeInt;
            value.metaData = mdata;
            return value;
        }

        public static StackValue BuildDouble(double data)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.Double;
            value.opdata = BitConverter.DoubleToInt64Bits(data);

            MetaData mdata = new MetaData();
            mdata.type = (int)PrimitiveType.kTypeDouble;
            value.metaData = mdata;
            return value;
        }

        public static StackValue BuildChar(char ch)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.Char;
            value.opdata = ProtoCore.Utils.EncodingUtils.ConvertCharacterToInt64(ch);

            MetaData mdata = new MetaData();
            mdata.type = (int)PrimitiveType.kTypeChar;
            value.metaData = mdata;
            return value;
        }

        public static StackValue BuildNull()
        {
            StackValue value = new StackValue();
            value.optype = AddressType.Null;
            value.opdata = 0;
            MetaData mdata = new MetaData();
            mdata.type = (int)PrimitiveType.kTypeNull;
            value.metaData = mdata;
            return value;
        }

        public static StackValue BuildPointer(Int64 data)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.Pointer;
            value.opdata = data;
            return value;
        }

        public static StackValue BuildPointer(Int64 data, MetaData mdata)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.Pointer;
            value.opdata = data;
            value.metaData = mdata;
            return value;
        }

        public static StackValue BuildArrayPointer(Int64 data)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.ArrayPointer;
            value.opdata = data;

            MetaData mdata;
            mdata.type = (int)PrimitiveType.kTypeArray;
            value.metaData = mdata;
            return value;
        }

        public static StackValue BuildArrayKey(int rawArrayPtr, int index)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.ArrayKey;

            if (index == Constants.kInvalidIndex || rawArrayPtr == Constants.kInvalidIndex)
            {
                value.opdata = Constants.kInvalidIndex;
            }
            else
            {
                ulong key = (ulong)rawArrayPtr;
                key = (key << 32) | (uint)index;
                value.opdata = (long)key;
            }
            return value;
        }

        public static StackValue BuildThisPtr(int thisptr)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.ThisPtr;
            value.opdata = thisptr;
            return value;
        }

        public static StackValue BuildDynamic(int dynamic)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.Dynamic;
            value.opdata = dynamic;
            return value;
        }

        public static StackValue BuildArrayDimension(int dimension)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.ArrayDim;
            value.opdata = dimension;
            return value;
        }

        public static StackValue BuildBlockIndex(int blockIndex)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.BlockIndex;
            value.opdata = blockIndex;
            return value;
        }

        public static StackValue BuildLabelIndex(int labelIndex)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.LabelIndex;
            value.opdata = labelIndex;
            return value;
        }

        public static StackValue BuildClassIndex(int classIndex)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.ClassIndex;
            value.opdata = classIndex;
            return value;
        }

        public static StackValue BuildFunctionIndex(int functionIndex)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.FunctionIndex;
            value.opdata = functionIndex;
            return value;
        }

        public static StackValue BuildVarIndex(int varIndex)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.VarIndex;
            value.opdata = varIndex;
            return value;
        }

        public static StackValue BuildStaticMemVarIndex(int varIndex)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.StaticMemVarIndex;
            value.opdata = varIndex;
            return value;
        }

        public static StackValue BuildMemVarIndex(int varIndex)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.MemVarIndex;
            value.opdata = varIndex;
            return value;
        }

        public static StackValue BuildFunctionPointer(int fptr)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.FunctionPointer;
            value.opdata = fptr;
            return value;
        }

        public static StackValue BuildReplicationGuide(int guide)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.ReplicationGuide;
            value.opdata = guide;
            return value;
        }

        public static StackValue BuildString(Int64 data)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.String;
            value.opdata = data;

            MetaData mdata;
            mdata.type = (int)PrimitiveType.kTypeString;
            value.metaData = mdata;
            return value;
        }

        public static StackValue BuildString(string str, Heap heap)
        {
            var svchars = new List<StackValue>();

            foreach (char ch in str)
            {
                svchars.Add(ProtoCore.DSASM.StackValue.BuildChar(ch));
            }

            lock (heap.cslock)
            {
                int size = str.Length;

                int ptr = heap.Allocate(size);

                for (int i = 0; i < size; ++i)
                {
                    heap.Heaplist[ptr].Stack[i] = BuildChar(str[i]);
                }

                return StackValue.BuildString(ptr);
            }
        }

        public static StackValue BuildBoolean(Boolean b)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.Boolean;
            value.opdata = b ? 1 : 0;

            MetaData mdata;
            mdata.type = (int)PrimitiveType.kTypeBool;
            value.metaData = mdata;

            return value;
        }

        public static StackValue BuildDefaultArgument()
        {
            StackValue value = new StackValue();
            value.optype = AddressType.DefaultArg;
            value.opdata = 0;

            MetaData mdata;
            mdata.type = (int)PrimitiveType.kTypeVar;
            value.metaData = mdata;

            return value;
        }

        public static StackValue BuildDynamicBlock(int block)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.BlockIndex;
            value.opdata = value.opdata = block;

            MetaData mdata;
            mdata.type = (int)PrimitiveType.kTypeVar;
            value.metaData = mdata;

            return value;
        }

        public static StackValue BuildRegister(Registers reg)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.Register;
            value.opdata = (int)reg;
            return value;
        }

        public static StackValue BuildStaticType(int UID, int rank = 0)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.StaticType;
            value.metaData = new MetaData { type = UID };
            value.opdata = rank;
            return value;
        }

        public static StackValue BuildFrameType(int type)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.FrameType;
            value.opdata = type;
            return value;
        }

        public static StackValue BuildExplicitCall(int pc)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.ExplicitCall;
            value.opdata = pc;
            return value;
        }

        public static StackValue BuildCallingConversion(int data)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.CallingConvention;
            value.opdata = data;
            return value;
        }

        public static StackValue BulildInvalid()
        {
            StackValue value = new StackValue();
            value.optype = AddressType.Invalid;
            value.opdata = -1;

            MetaData mdata = new MetaData();
            mdata.type = (int)PrimitiveType.kInvalidType;
            value.metaData = mdata;

            return value;
        }

        public static List<StackValue> BuildInvalidRegisters()
        {
            List<StackValue> registers = new List<StackValue>();

            for (int i = 0; i < 10; ++i)
            {
                registers.Add(BulildInvalid());
            }
            return registers;
        }
        #endregion

        public bool TryGetArrayKey(out int rawArrayPointer, out int key)
        {
            rawArrayPointer = Constants.kInvalidIndex;
            key = Constants.kInvalidIndex;

            if (!this.IsArrayKey())
                return false;

            long value = opdata;
            if (value == Constants.kInvalidIndex)
                return false;

            rawArrayPointer = (int)((ulong)value >> 32);
            key = (int)((ulong)value << 32 >> 32);
            return true;
        }
    }

    public class Instruction
    {
        public OpCode opCode;
        public Operand op1;     // Comment Jun: Aliasing StackValue to Operand only here ... to make it clear that an instruction has operands
        public Operand op2;
        public Operand op3;
        public DebugInfo debug;
    }

    public class DebugInfo
    {
        public ProtoCore.CodeModel.CodeRange Location { get; set; }
        public List<int> nextStep;

        public DebugInfo(int line, int col, int eline, int ecol, string file = null)
        {
            InitializeDebugInfo(line, col, eline, ecol, file);
        }

        private void InitializeDebugInfo(int line, int col, int eline, int ecol, string file)
        {
            nextStep = new List<int>();

            CodeModel.CodeFile sourceLocation = new CodeModel.CodeFile { FilePath = file };
            ProtoCore.CodeModel.CodeRange range = new CodeModel.CodeRange
            {
                StartInclusive = new CodeModel.CodePoint
                {
                    LineNo = line,
                    CharNo = col,
                    SourceLocation = sourceLocation
                },

                EndExclusive = new CodeModel.CodePoint
                {
                    LineNo = eline,
                    CharNo = ecol,
                    SourceLocation = sourceLocation
                }
            };

            Location = range;
        }
    }
}
