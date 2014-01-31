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
        ADDD,
        SUBD,
        MULD,
        DIVD,
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
        EQD,
        NQD,
        GTD,
        LTD,
        GED,
        LED,
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
        public Double opdata_d;
        public AddressType optype;
        public MetaData metaData;

        public StackValue ShallowClone()
        {
            StackValue newSv = new StackValue();
            newSv.optype = optype;
            newSv.opdata_d = opdata_d;
            newSv.opdata = opdata;
            newSv.metaData = new MetaData { type = metaData.type };

            return newSv;
        }

        public override string ToString()
        {
            if (optype == AddressType.Double)
                return opdata_d.ToString();

            if (optype == AddressType.Int)
                return opdata.ToString();

            else
                return String.Format("{0}, opdata = {1}, metaData = {2}", optype.ToString(), opdata.ToString(),
                                     metaData.type.ToString());

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
