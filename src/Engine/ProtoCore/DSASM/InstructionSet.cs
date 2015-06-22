using System;
using System.Collections.Generic;
using ProtoCore.Utils;
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

    public enum AddressType: int 
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
        JMP,
        CJMP,
        JDEP,
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
        ALLOCA,
        ALLOCC,
        POPM,
      
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
        
        PUSHB,

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
        public double opdata_d;
        public AddressType optype;
        public MetaData metaData;

        /// <summary>
        /// Although StackValue is a struct and assignment creates a copy
        /// of StackValue on stack, ShallowClone() has an explicit meaning
        /// to do copy.
        /// </summary>
        /// <returns></returns>
        public StackValue ShallowClone()
        {
            StackValue newSv = new StackValue();
            newSv.optype = optype;
            newSv.opdata = opdata;
            newSv.opdata_d = opdata_d;
            newSv.metaData = new MetaData { type = metaData.type };
            return newSv;
        }

        #region Override functions
        public override bool Equals(object other)
        {
            if (!(other is StackValue))
                return false;

            StackValue rhs = (StackValue)other;
            if (this.optype != rhs.optype || this.metaData.type != rhs.metaData.type)
                return false;

            switch (optype)
            {
                case AddressType.Double:
                    return MathUtils.Equals(this.RawDoubleValue, rhs.RawDoubleValue);

                case AddressType.Boolean:
                    return this.RawBooleanValue == rhs.RawBooleanValue;

                default:
                    return opdata == rhs.opdata;
            }
        }

        public override int GetHashCode()
        {
            return opdata.GetHashCode() ^ optype.GetHashCode() ^ metaData.GetHashCode();
        }
        #endregion

        #region Get raw values
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
                return opdata_d;
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
        #endregion
        
        #region Constant values
        public static StackValue Null = BuildNull();
        public static StackValue True = BuildBoolean(true);
        public static StackValue False = BuildBoolean(false);
        #endregion

        #region Type checkers
        public bool IsInvalid
        {
            get { return optype == AddressType.Invalid; }
        }

        public bool IsRegister
        {
            get { return optype == AddressType.Register; }
        }

        public bool IsVariableIndex
        {
            get { return optype == AddressType.VarIndex; }
        }

        public bool IsFunctionIndex
        {
            get { return optype == AddressType.FunctionIndex; }
        }

        public bool IsMemberVariableIndex
        {
            get { return optype == AddressType.MemVarIndex; }
        }

        public bool IsStaticVariableIndex
        {
            get { return optype == AddressType.StaticMemVarIndex; }
        }

        public bool IsClassIndex
        {
            get { return optype == AddressType.ClassIndex; }
        }

        public bool IsInteger
        {
            get { return optype == AddressType.Int; }
        }

        public bool IsDouble
        {
            get { return optype == AddressType.Double; }
        }

        public bool IsNumeric
        {
            get { return optype == AddressType.Int || optype ==
            AddressType.Double; }
        }

        public bool IsBoolean
        {
            get { return optype == AddressType.Boolean; }
        }

        public bool IsChar
        {
            get { return optype == AddressType.Char; }
        }

        public bool IsString
        {
            get { return optype == AddressType.String; }
        }

        public bool IsLabelIndex
        {
            get { return optype == AddressType.LabelIndex; }
        }

        public bool IsBlockIndex
        {
            get { return optype == AddressType.BlockIndex; }
        }

        public bool IsPointer
        {
            get { return optype == AddressType.Pointer; }
        }

        public bool IsArray
        {
            get { return optype == AddressType.ArrayPointer; }
        }

        public bool IsFunctionPointer
        {
            get { return optype == AddressType.FunctionPointer; }
        }

        public bool IsNull
        {
            get { return optype == AddressType.Null; }
        }

        public bool IsDefaultArgument
        {
            get { return optype == AddressType.DefaultArg; }
        }

        public bool IsArrayDimension
        {
            get { return optype == AddressType.ArrayDim; }
        }

        public bool IsReplicationGuide
        {
            get { return optype == AddressType.ReplicationGuide; }
        }

        public bool IsDynamic
        {
            get { return optype == AddressType.Dynamic; }
        }

        public bool IsStaticType
        {
            get { return optype == AddressType.StaticType; }
        }

        public bool IsCallingConvention
        {
            get { return optype == AddressType.CallingConvention; }
        }

        public bool IsFrameType
        {
            get { return optype == AddressType.FrameType; }
        }

        public bool IsThisPtr
        {
            get { return optype == AddressType.ThisPtr; }
        }

        public bool IsExplicitCall
        {
            get { return optype == AddressType.ExplicitCall; }
        }

        public bool IsArrayKey
        {
            get { return optype == AddressType.ArrayKey; }
        }

        public bool IsReferenceType
        {
            get { return opdata != Constants.kInvalidIndex && (IsArray || IsPointer || IsString); }
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
            value.opdata_d = data;

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

        public static StackValue BuildArrayKey(int arrayPtr, int index)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.ArrayKey;
            value.opdata = index;
            value.opdata_d = arrayPtr;

            return value;
        }

        public static StackValue BuildArrayKey(StackValue array, int index)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.ArrayKey;
            value.opdata = index;
            value.metaData = array.metaData;

            Validity.Assert(array.IsArray || array.IsString);
            value.opdata_d = (int)array.opdata;

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
            return heap.AllocateString(str);
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

        /// <summary>
        /// Try to get the host array and key value from StackValue. The address
        /// type of StackValue should be AddressType.ArrayKey. 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool TryGetArrayKey(out StackValue array, out int index)
        {
            array = StackValue.Null;
            index = Constants.kInvalidIndex;

            if (!this.IsArrayKey || opdata == Constants.kInvalidIndex)
            {
                return false;
            }

            if (this.metaData.type == (int)PrimitiveType.kTypeString)
                array = StackValue.BuildString((long)RawDoubleValue);
            else
                array = StackValue.BuildArrayPointer((long)RawDoubleValue);

            index = (int)this.opdata;

            return true;
        }

        #region Converters
        /// <summary>
        /// Convert StackValue to boolean typed StackValue. Returns 
        /// StackValue.Null if not able to do conversion.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public StackValue ToBoolean(RuntimeCore runtimeCore)
        {
            switch (optype)
            {
                case AddressType.Boolean:
                    return this;

                case AddressType.Int:
                    return BuildBoolean(opdata != 0);

                case AddressType.Null:
                    return StackValue.Null; 

                case AddressType.Double:
                    bool b = !Double.IsNaN(RawDoubleValue) && !RawDoubleValue.Equals(0.0);
                    return BuildBoolean(b);

                case AddressType.Pointer:
                    return StackValue.BuildBoolean(true);

                case AddressType.String:
                    string str = runtimeCore.RuntimeMemory.Heap.GetString(this);
                    return string.IsNullOrEmpty(str) ? StackValue.False : StackValue.True;

                case AddressType.Char:
                    char c = EncodingUtils.ConvertInt64ToCharacter(opdata);
                    return (c == 0) ? StackValue.False : StackValue.True;

                default:
                    return StackValue.Null;
            }
        }

        /// <summary>
        /// Convert numeric typed StackValue to double typed StackValue. For
        /// other types, returns StackValue.Null.
        /// </summary>
        /// <returns></returns>
        public StackValue ToDouble()
        {
            switch (optype)
            {
                case AddressType.Int:
                    return BuildDouble(RawIntValue);

                case AddressType.Double:
                    return this;

                default:
                    return StackValue.Null;
            }
        }

        /// <summary>
        /// Convert numeric typed StackValue to integer typed StackValue. For
        /// other types, returns StackValue.Null.
        /// </summary>
        /// <returns></returns>
        public StackValue ToInteger()
        {
            switch (optype)
            {
                case AddressType.Int:
                    return this;

                case AddressType.Double:
                    double value = RawDoubleValue;
                    return BuildInt((Int64)Math.Round(value, 0, MidpointRounding.AwayFromZero));

                default:
                    return StackValue.Null;
            }
        }
        #endregion
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
