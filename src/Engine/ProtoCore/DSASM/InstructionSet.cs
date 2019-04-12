using System;
using System.Collections.Generic;
using System.Linq;
using ProtoCore.Exceptions;
using ProtoCore.Utils;
using Operand = ProtoCore.DSASM.StackValue;

namespace ProtoCore.DSASM
{
    public enum Registers
    {
        RX,
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

        // operators
        ADD,
        SUB,
        MUL,
        DIV,
        MOD,
        NEG,
        AND,
        OR,
        NOT,
        EQ,
        NQ,
        GT,
        LT,
        GE,
        LE,

        // memory allocation
        NEWARR,         // Allocate array
        NEWOBJ,         // Allocate object

        PUSH,
        PUSHBLOCK,      // Push construction block id in imperative code
        PUSHM,
        PUSHW,
        PUSHDEP,        // Push symbols in left-hand-side identifier list in impertiave langauge block
        PUSHREPGUIDE,   // Push replicaion guide to the stack
        PUSHLEVEL,      // Push at-level to the stak
        POP,
        POPW,
        POPM,
        POPREPGUIDES,   // Pop replication guides from stack and save to the core

        LOADELEMENT,    // Load array element
        SETELEMENT,     // Set element
        SETMEMElEMENT,  // Set element for member variable

        CAST,

        CALL,
        CALLR,
        RETURN,         // Return from function call
        RETB,           // Return from code block (pushed by BOUNCE)
        RETCN,          // Return from local block

        BOUNCE,         // Bounce to different language block
        JMP,
        CJMP,
        JDEP,
        DEP,
        
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
        private long opdata;
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
                    return MathUtils.Equals(this.DoubleValue, rhs.DoubleValue);

                case AddressType.Boolean:
                    return this.BooleanValue == rhs.BooleanValue;

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
        /// Returns raw data without checking its type or do type conversion.
        /// Use with caution.
        /// </summary>
        public long RawData
        {
            get
            {
                return opdata;
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
            get { return optype == AddressType.Int || optype == AddressType.Double; }
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

        private void Check(bool cond, string errorMessage)
        {
            if (!cond)
                throw new RuntimeException(errorMessage);
        }

        #region Type-dependent value extractor
        public int VariableIndex
        {
            get
            {
                Check(IsVariableIndex, "The type of StackValue is not VariableIndex");
                return (int)opdata;
            }
        }

        public int FunctionIndex
        {
            get
            {
                Check(IsFunctionIndex, "The type of StackValue is not FunctionIndex");
                return (int)opdata;
            }
        }

        public int MemberVariableIndex
        {
            get
            {
                Check(IsMemberVariableIndex, "The type of StackValue is not MemberVariableIndex");
                return (int)opdata;
            }
        }

        public int StaticVariableIndex
        {
            get
            {
                Check(IsStaticVariableIndex, "The type of StackValue is not StaticVariableIndex");
                return (int)opdata;
            }
        }

        public int SymbolIndex
        {
            get
            {
                Check(IsVariableIndex || IsStaticVariableIndex || IsMemberVariableIndex, "It is not a symbol type");
                return (int)opdata;
            }
        }

        public int ClassIndex
        {
            get
            {
                Check(IsClassIndex, "The Type of StackValue is not ClassIndex");
                return (int)opdata;
            }
        }

        public long IntegerValue
        {
            get
            {
                Check(IsInteger, "The Type of StackValue is not Integer");
                return opdata;
            }
        }

        public long CharValue
        {
            get
            {
                Check(IsChar, "The Type of StackValue is not Char");
                return opdata;
            }
        }

        public double DoubleValue
        {
            get
            {
                Check(IsDouble, "The Type of StackValue is not Double");
                return BitConverter.Int64BitsToDouble(opdata);
            }
        }

        public bool BooleanValue
        {
            get
            {
                Check(IsBoolean, "The Type of StackValue is not Boolean");
                return opdata != 0;
            }
        }

        public int StringPointer
        {
            get
            {
                Check(IsString, "The Type of StackValue is not String");
                return (int)opdata;
            }
        }

        public int LabelIndex
        {
            get
            {
                Check(IsLabelIndex, "The Type of StackValue is not LabelIndex");
                return (int)opdata;
            }
        }

        public int BlockIndex
        {
            get
            {
                Check(IsBlockIndex, "The Type of StackValue is not BlockIndex");
                return (int)opdata;
            }
        }

        public int Pointer
        {
            get
            {
                Check(IsPointer, "The Type of StackValue is not Pointer");
                return (int)opdata;
            }
        }

        public int ArrayPointer
        {
            get
            {
                Check(IsArray, "The Type of StackValue is not ArrayPointer");
                return (int)opdata;
            }
        }

        public int FunctionPointer
        {
            get
            {
                Check(IsFunctionPointer, "The Type of StackValue is not FunctionPointer");
                return (int)opdata;
            }
        }

        public int ArrayDimension
        {
            get
            {
                Check(IsArrayDimension, "The Type of StackValue is not ArrayDimension");
                return (int)opdata;
            }
        }

        public int ReplicationGuide
        {
            get
            {
                Check(IsReplicationGuide, "The Type of StackValue is not ReplicationGuide");
                return (int)opdata;
            }
        }

        public int Rank 
        {
            get
            {
                Check(IsStaticType, "The Type of StackValue is not StaticType");
                return (int)opdata;
            }
        }

        public CallingConvention.CallType CallType
        {
            get
            {
                Check(IsCallingConvention, "The Type of StackValue is not CallingConvention");
                return (CallingConvention.CallType)opdata;
            }
        }

        public CallingConvention.BounceType BounceType 
        {
            get
            {
                Check(IsCallingConvention, "The Type of StackValue is not CallingConvention");
                return (CallingConvention.BounceType)opdata;
            }
        }

        public int ExplicitCallEntry
        {
            get
            {
                Check(IsExplicitCall, "The Type of StackValue is not ExplicitCall");
                return (int)opdata;
            }
        }

        public StackFrameType FrameType
        {
            get
            {
                Check(IsFrameType, "The Type of StackValue is not FrameType");
                return (StackFrameType)opdata;
            }
        }

        public int DynamicIndex
        {
            get
            {
                Check(IsDynamic, "The Type of StackValue is not Dynamic");
                return (int)opdata;
            }
        }

        public int ThisPtr
        {
            get
            {
                Check(IsThisPtr, "The Type of StackValue is not ThisPtr");
                return (int)opdata;
            }
        }

        public int ArrayKeyIndex
        {
            get
            {
                Check(IsArrayKey, "The Type of StackValue is not ArrayKey");
                return (int)opdata;
            }
        }

        public Registers Register
        {
            get
            {
                Check(IsRegister, "The Type of StackValue is not ArrayKey");
                return (Registers)(int)opdata;
            }
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
            mdata.type = (int)PrimitiveType.Integer;
            value.metaData = mdata;
            return value;
        }

        public static StackValue BuildDouble(double data)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.Double;
            value.opdata = BitConverter.DoubleToInt64Bits(data);

            MetaData mdata = new MetaData();
            mdata.type = (int)PrimitiveType.Double;
            value.metaData = mdata;
            return value;
        }

        public static StackValue BuildChar(char ch)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.Char;
            value.opdata = Convert.ToInt64(ch);

            MetaData mdata = new MetaData();
            mdata.type = (int)PrimitiveType.Char;
            value.metaData = mdata;
            return value;
        }

        public static StackValue BuildNull()
        {
            StackValue value = new StackValue();
            value.optype = AddressType.Null;
            value.opdata = 0;
            MetaData mdata = new MetaData();
            mdata.type = (int)PrimitiveType.Null;
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
            mdata.type = (int)PrimitiveType.Array;
            value.metaData = mdata;
            return value;
        }

        public static StackValue BuildArrayKey(int arrayPtr, int index)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.ArrayKey;

            if (index == Constants.kInvalidIndex || arrayPtr == Constants.kInvalidIndex)
            {
                value.opdata = Constants.kInvalidIndex;
            }
            else
            {
                // Array key information is encoded in 64bits opdata. 
                //
                // High 32 bits: array pointer
                // Low 32 bits : array key
                
                // TODO: find out a cleaner way to represent array key instead 
                // of using this kind of hacking.
                ulong key = (ulong)arrayPtr;
                key = (key << 32) | (uint)index;
                value.opdata = (long)key;
            }

            return value;
        }

        public static StackValue BuildArrayKey(StackValue array, int index)
        {
            Validity.Assert(array.IsArray || array.IsString);
            int ptr = array.IsArray ? array.ArrayPointer : array.StringPointer;
            var arrayKey = BuildArrayKey(ptr, index);
            arrayKey.metaData = array.metaData;
            return arrayKey;
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
            mdata.type = (int)PrimitiveType.String;
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
            mdata.type = (int)PrimitiveType.Bool;
            value.metaData = mdata;

            return value;
        }

        public static StackValue BuildDefaultArgument()
        {
            StackValue value = new StackValue();
            value.optype = AddressType.DefaultArg;
            value.opdata = 0;

            MetaData mdata;
            mdata.type = (int)PrimitiveType.Var;
            value.metaData = mdata;

            return value;
        }

        public static StackValue BuildDynamicBlock(int block)
        {
            StackValue value = new StackValue();
            value.optype = AddressType.BlockIndex;
            value.opdata = block;

            MetaData mdata;
            mdata.type = (int)PrimitiveType.Var;
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
            mdata.type = (int)PrimitiveType.InvalidType;
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
        /// <param name="key"></param>
        /// <returns></returns>
        public bool TryGetArrayKey(out StackValue array, out int key)
        {
            array = StackValue.Null;
            key = Constants.kInvalidIndex;

            if (!IsArrayKey || opdata == Constants.kInvalidIndex)
            {
                return false;
            }

            // Array key information is encoded in 64 bits opdata. 
            // High 32 bits: array pointer
            // Low 32 bits : array key, that is the index into the array.
            //
            // TODO: find out a cleaner way to represent array key instead of
            // using this kind of hacking.
            var rawArrayPointer = ((ulong)opdata >> 32);
            if (this.metaData.type == (int)PrimitiveType.String)
            {
                array = BuildString((long)rawArrayPointer);
            }
            else
            {
                array = BuildArrayPointer((long)rawArrayPointer);
            }
            key = (int)(((ulong)opdata << 32) >> 32);
            return true;
        }

        /// <summary>
        /// Returns an array's next key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public StackValue GetNextKey(RuntimeCore runtimeCore)
        {
            StackValue svArray;
            int index;

            if (!TryGetArrayKey(out svArray, out index))
            {
                return StackValue.Null;
            }

            int nextIndex = Constants.kInvalidIndex;

            if (svArray.IsArray)
            {
                DSArray array = runtimeCore.Heap.ToHeapObject<DSArray>(svArray);
                if (array.Values.Count() > index + 1)
                    nextIndex = index + 1;
            }
            else if (svArray.IsString)
            {
                DSString str = runtimeCore.Heap.ToHeapObject<DSString>(svArray);
                if (str.Value.Length > index + 1)
                    nextIndex = index + 1;

            }

            return nextIndex == Constants.kInvalidIndex ? StackValue.Null : StackValue.BuildArrayKey(svArray, nextIndex);
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
                    bool b = !Double.IsNaN(DoubleValue) && !DoubleValue.Equals(0.0);
                    return BuildBoolean(b);

                case AddressType.Pointer:
                    return StackValue.BuildBoolean(true);

                case AddressType.String:
                    string str = runtimeCore.RuntimeMemory.Heap.ToHeapObject<DSString>(this).Value;
                    return string.IsNullOrEmpty(str) ? StackValue.False : StackValue.True;

                case AddressType.Char:
                    char c = Convert.ToChar(opdata);
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
                    return BuildDouble(opdata);

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
                    double value = DoubleValue;
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
