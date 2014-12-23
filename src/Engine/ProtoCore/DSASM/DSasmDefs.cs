using System.Collections.Generic;

namespace ProtoCore.DSASM
{
    public enum InterpreterMode
    {
        kExpressionInterpreter,
        kNormal,
        kModes
    }

    namespace CallingConvention
    {
        public enum BounceType
        {
            kExplicit,  // Explicit bounce is using the same executive for the bounce target
            kImplicit,  // Implicit bounce is using a new executive for the bounce target
            kNumTypes
        }

        public enum CallType
        {
            kExplicit,      // Explicit call is using the same executive for the function call
            kExplicitBase,  // Explicit call to the base class
            kImplicit,      // Implicit call is using a new executive for the function call
            kNumTypes
        }
    }

    public enum Operator
    {
        none,
        assign,

        lt,
        gt,
        le,
        ge,
        eq,
        nq,
        add,
        sub,
        mul,
        div,
        mod,

        and,
        or,
        dot,

        bitwiseand,
        bitwiseor,
        bitwisexor,
        bitwisenegate
    }

    public enum UnaryOperator
    {
        None,
        Not,
        Negate,
        Increment,
        Decrement,
        Neg         
    }

    public enum RangeStepOperator
    {
        stepsize,
        num,
        approxsize
    }

    //@TODO(Jun): This should be an enumeration, not a bunch of consts?

    // @TODO(Jun Comment), The actual enums are in InstructionSet.cs. 
    // These were intended for emitting assembly code (Currently to console, but perhaps also to a file)
    // We can then easily have a *.dsasm file containing only assembly code that the VM/Interpreter can execute
    public struct kw
    {
        public const string mov = /*NXLT*/"mov";
        public const string call = /*NXLT*/"call";
        public const string callr = /*NXLT*/"callr";
        public const string callc = /*NXLT*/"callc";
        public const string add = /*NXLT*/"add";
        public const string sub = /*NXLT*/"sub";
        public const string mul = /*NXLT*/"mul";
        public const string div = /*NXLT*/"div";
        public const string mod = /*NXLT*/"mod";
        public const string eq = /*NXLT*/"eq";
        public const string nq = /*NXLT*/"nq";
        public const string gt = /*NXLT*/"gt";
        public const string lt = /*NXLT*/"lt";
        public const string ge = /*NXLT*/"ge";
        public const string le = /*NXLT*/"le";
        public const string jg = /*NXLT*/"jg";
        public const string jl = /*NXLT*/"jl";
        public const string jge = /*NXLT*/"jge";
        public const string jle = /*NXLT*/"jle";
        public const string jleq = /*NXLT*/"jleq";
        public const string jgeq = /*NXLT*/"jgeq";
        public const string jmp = /*NXLT*/"jmp";
        public const string cjmp = /*NXLT*/"cjmp";
        public const string jlz = /*NXLT*/"jlz";
        public const string jgz = /*NXLT*/"jgz";
        public const string jz = /*NXLT*/"jz";
        public const string jdep = /*NXLT*/"jdep";
        public const string label = /*NXLT*/"label";
        public const string bounce = /*NXLT*/"bounce";
        public const string alloca = /*NXLT*/"alloca";
        public const string allocc = /*NXLT*/"allocc";
        public const string push = /*NXLT*/"push";
        public const string pushg = /*NXLT*/"pushg";
        public const string pushm = /*NXLT*/"pushm";
        public const string pushw = /*NXLT*/"pushw";
        public const string pushindex = /*NXLT*/"pushindex";
        public const string pushdep = /*NXLT*/"pushdep";
        public const string pushlist = /*NXLT*/"pushlist";
        public const string ret = /*NXLT*/"ret";
        public const string retc = /*NXLT*/"retc";
        public const string retb = /*NXLT*/"retb";
        public const string retcn = /*NXLT*/"retcn";
        public const string pop = /*NXLT*/"pop";
        public const string popw = /*NXLT*/"popw";
        public const string popg = /*NXLT*/"popg";
        public const string popm = /*NXLT*/"popm";
        public const string poplist = /*NXLT*/"poplist";
        public const string not = /*NXLT*/"not";
        public const string negate = /*NXLT*/"negate";
        public const string dep = /*NXLT*/"dep";
        public const string depx = /*NXLT*/"depx";
        public const string setexpuid = /*NXLT*/"setexpuid";
        public const string pushb = /*NXLT*/"pushb";
        public const string popb = /*NXLT*/"popb";

        // TODO Jun: these are temporary instruction 
        public const string pushvarsize = "pushvarsize";

        public const string regAX = /*NXLT*/"_ax";
        public const string regBX = /*NXLT*/"_bx";
        public const string regCX = /*NXLT*/"_cx";
        public const string regDX = /*NXLT*/"_dx";
        public const string regEX = /*NXLT*/"_ex";
        public const string regFX = /*NXLT*/"_fx";
        public const string regRX = /*NXLT*/"_rx";
        public const string regSX = /*NXLT*/"_sx";
        public const string regLX = /*NXLT*/"_lx";
        public const string regTX = /*NXLT*/"_tx";

        // TODO: Replace with ProtoCore.DSDefinitions.Keyword struct
        public const string cast = "cast";
        public const string throwexception = "throw";

        // TODO: Replace with ProtoCore.DSDefinitions.Keyword struct
        public const string associative = "Associative";
        public const string imperative = "Imperative";
        public const string options = "Options";
    }

    // TODO: Replace with ProtoCore.DSDefinitions.Keyword struct
    public struct Literal
    {
        public const string True = "true";
        public const string False = "false";
        public const string Null = "null";
    }

    /// <summary>
    /// Translate an operator to other representations.
    /// </summary>
    public class Op
    {
        /// <summary>
        /// Return the corresponding opcode of an operator.
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public static ProtoCore.DSASM.OpCode GetOpCode(Operator op)
        {
            if (null == opCodeTable)
            {
                initOpCodeTable();
            }
            return opCodeTable[op];
        }

        /// <summary>
        /// Return the corresponding opcode of an unary operator.
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public static ProtoCore.DSASM.OpCode GetUnaryOpCode(UnaryOperator op)
        {
            if (null == unaryOpCodeTable)
            {
                initUnaryOpCodeTable();
            }
            return unaryOpCodeTable[op];
        }

        /// <summary>
        /// Return the symbol representation of an operator. E.g., return "+"
        /// for Operator.add.
        /// </summary>
        /// <param name="op">Operator</param>
        /// <returns></returns>
        public static string GetOpSymbol(Operator op)
        {
            if (null == opSymbolTable)
            {
                initOpSymbolTable();
            }
            return opSymbolTable[op];
        }

        /// <summary>
        /// Return the symbol representation of an unary operator. E.g., return
        /// "-" for UnaryOperator.neg
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public static string GetUnaryOpSymbol(UnaryOperator op)
        {
            if (null == unaryOpSymbolTable)
            {
                initUnaryOpSymbolTable();
            }
            return unaryOpSymbolTable[op];
        }

        /// <summary>
        /// Return the string representation of an operator. E.g., return "add"
        /// for Operator.add.
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public static string GetOpName(Operator op)
        {
            if (null == opNameTable)
            {
                initOpNameTable();
            }
            return opNameTable[op];
        }

        /// <summary>
        /// Return the string representation of an unary operator. E.g., return 
        /// "not" for UnaryOperator.not.
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public static string GetUnaryOpName(UnaryOperator op)
        {
            if (null == unaryOpNameTable)
            {
                initUnaryOpNameTable();
            }
            return unaryOpNameTable[op];
        }

        /// <summary>
        /// Return the internal function name for operator.
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public static string GetOpFunction(Operator op)
        {
            return Constants.kInternalNamePrefix + op.ToString();
        }

        /// <summary>
        /// Return the internal function name for unary operator
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public static string GetUnaryOpFunction(UnaryOperator op)
        {
            return Constants.kInternalNamePrefix + op.ToString();
        }

        private static Dictionary<Operator, ProtoCore.DSASM.OpCode> opCodeTable;
        private static Dictionary<Operator, string> opNameTable;
        private static Dictionary<Operator, string> opSymbolTable;

        private static Dictionary<UnaryOperator, ProtoCore.DSASM.OpCode> unaryOpCodeTable;
        private static Dictionary<UnaryOperator, string> unaryOpNameTable;
        private static Dictionary<UnaryOperator, string> unaryOpSymbolTable;

        private static void initUnaryOpCodeTable()
        {
            unaryOpCodeTable = new Dictionary<UnaryOperator, OpCode>();
            unaryOpCodeTable.Add(UnaryOperator.None, ProtoCore.DSASM.OpCode.NONE);
            unaryOpCodeTable.Add(UnaryOperator.Not, ProtoCore.DSASM.OpCode.NOT);
            unaryOpCodeTable.Add(UnaryOperator.Negate, ProtoCore.DSASM.OpCode.NEGATE);
            unaryOpCodeTable.Add(UnaryOperator.Neg, ProtoCore.DSASM.OpCode.NEG);
        }

        private static void initUnaryOpNameTable()
        {
            unaryOpNameTable = new Dictionary<UnaryOperator, string>();
            unaryOpNameTable.Add(UnaryOperator.None, "none");
            unaryOpNameTable.Add(UnaryOperator.Not, "not");
            unaryOpNameTable.Add(UnaryOperator.Negate, "negate");
            unaryOpNameTable.Add(UnaryOperator.Neg, "neg");
        }
 
        private static void initOpNameTable()
        {
            opNameTable = new Dictionary<Operator, string>();

            opNameTable.Add(Operator.none, "none");
            opNameTable.Add(Operator.assign, "assign");
            opNameTable.Add(Operator.and, "and");
            opNameTable.Add(Operator.or, "or");
            opNameTable.Add(Operator.dot, "dot");
            opNameTable.Add(Operator.bitwiseand, "bitand");
            opNameTable.Add(Operator.bitwiseor, "biteor");
            opNameTable.Add(Operator.bitwisexor, "bitxor");

            opNameTable.Add(Operator.lt, ProtoCore.DSASM.kw.lt);
            opNameTable.Add(Operator.gt, ProtoCore.DSASM.kw.gt);
            opNameTable.Add(Operator.le, ProtoCore.DSASM.kw.le);
            opNameTable.Add(Operator.ge, ProtoCore.DSASM.kw.ge);
            opNameTable.Add(Operator.eq, ProtoCore.DSASM.kw.eq);
            opNameTable.Add(Operator.nq, ProtoCore.DSASM.kw.nq);
            opNameTable.Add(Operator.add, ProtoCore.DSASM.kw.add);
            opNameTable.Add(Operator.sub, ProtoCore.DSASM.kw.sub);
            opNameTable.Add(Operator.mul, ProtoCore.DSASM.kw.mul);
            opNameTable.Add(Operator.div, ProtoCore.DSASM.kw.div);
            opNameTable.Add(Operator.mod, ProtoCore.DSASM.kw.mod);
        }

        private static void initOpCodeTable()
        {
            opCodeTable = new Dictionary<Operator, ProtoCore.DSASM.OpCode>();

            opCodeTable.Add(Operator.none, ProtoCore.DSASM.OpCode.NONE);
            opCodeTable.Add(Operator.lt, ProtoCore.DSASM.OpCode.LT);
            opCodeTable.Add(Operator.gt, ProtoCore.DSASM.OpCode.GT);
            opCodeTable.Add(Operator.le, ProtoCore.DSASM.OpCode.LE);
            opCodeTable.Add(Operator.ge, ProtoCore.DSASM.OpCode.GE);
            opCodeTable.Add(Operator.eq, ProtoCore.DSASM.OpCode.EQ);
            opCodeTable.Add(Operator.nq, ProtoCore.DSASM.OpCode.NQ);
            opCodeTable.Add(Operator.add, ProtoCore.DSASM.OpCode.ADD);
            opCodeTable.Add(Operator.sub, ProtoCore.DSASM.OpCode.SUB);
            opCodeTable.Add(Operator.mul, ProtoCore.DSASM.OpCode.MUL);
            opCodeTable.Add(Operator.div, ProtoCore.DSASM.OpCode.DIV);
            opCodeTable.Add(Operator.mod, ProtoCore.DSASM.OpCode.MOD);
            opCodeTable.Add(Operator.and, ProtoCore.DSASM.OpCode.AND);
            opCodeTable.Add(Operator.or, ProtoCore.DSASM.OpCode.OR);
            opCodeTable.Add(Operator.bitwiseand, ProtoCore.DSASM.OpCode.BITAND);
            opCodeTable.Add(Operator.bitwiseor, ProtoCore.DSASM.OpCode.BITOR);
            opCodeTable.Add(Operator.bitwisexor, ProtoCore.DSASM.OpCode.BITXOR);
        }

        private static void initOpSymbolTable()
        {
            opSymbolTable = new Dictionary<Operator, string>();
            opSymbolTable.Add(Operator.add, /*NXLT*/"+");
            opSymbolTable.Add(Operator.sub, /*NXLT*/"-");
            opSymbolTable.Add(Operator.mul, /*NXLT*/"*");
            opSymbolTable.Add(Operator.div, /*NXLT*/"/");
            opSymbolTable.Add(Operator.mod, /*NXLT*/"%");
            opSymbolTable.Add(Operator.bitwiseand, /*NXLT*/"&");
            opSymbolTable.Add(Operator.bitwiseor, /*NXLT*/"|");
            opSymbolTable.Add(Operator.bitwisexor, /*NXLT*/"^");
            opSymbolTable.Add(Operator.eq, /*NXLT*/"==");
            opSymbolTable.Add(Operator.nq, /*NXLT*/"!=");
            opSymbolTable.Add(Operator.ge, /*NXLT*/">=");
            opSymbolTable.Add(Operator.gt, /*NXLT*/">");
            opSymbolTable.Add(Operator.le, /*NXLT*/"<=");
            opSymbolTable.Add(Operator.lt, /*NXLT*/"<");
            opSymbolTable.Add(Operator.and, /*NXLT*/"&&");
            opSymbolTable.Add(Operator.or, /*NXLT*/"||");
            opSymbolTable.Add(Operator.assign, /*NXLT*/"=");
        }

        private static void initUnaryOpSymbolTable()
        {
            unaryOpSymbolTable = new Dictionary<UnaryOperator, string>();
            unaryOpSymbolTable.Add(UnaryOperator.Decrement, /*NXLT*/"--");
            unaryOpSymbolTable.Add(UnaryOperator.Increment, /*NXLT*/"++");
            unaryOpSymbolTable.Add(UnaryOperator.Neg, /*NXLT*/"-");
            unaryOpSymbolTable.Add(UnaryOperator.Negate, /*NXLT*/"~");
            unaryOpSymbolTable.Add(UnaryOperator.Not, /*NXLT*/"!");
        }
    }

    public struct Constants
    {
        public const int kInvalidIndex = -1;
        public const int kInvalidPC = -1;
        public const int kArbitraryRank = -1;
        public const int kPrimitiveSize = 1;
        public const int kGlobalScope = -1;
        public const int kPointerSize = 1;
        public const int kInvalidPointer = -1;
        public const int kPartialFrameData = 4;
        public const int kDefaultClassRank = 99;
        public const int nDimensionArrayRank = -1;
        public const int kDotArgCount = 2;
        public const int kDotCallArgCount = 6;
        public const int kDotArgIndexPtr = 0;
        public const int kDotArgIndexDynTableIndex = 1;
        public const int kDotArgIndexArrayIndex = 2;
        public const int kDotArgIndexDimCount = 3;
        public const int kDotArgIndexArrayArgs = 4;
        public const int kDotArgIndexArgCount = 5;
        public const int kThisFunctionAdditionalArgs = 1;

        // This is being moved to Core.Options as this needs to be overridden for the Watch test framework runner
        //public const int kDynamicCycleThreshold = 2000;
        public const int kRecursionTheshold = 1000;
        //public const int kRepetationTheshold = 1000;
        public const int kExressionInterpreterStackSize = 1;

        public const string termline = /*NXLT*/";\n";
        public const string kInternalNamePrefix = /*NXLT*/"%";
        public const string kStaticPropertiesInitializer = /*NXLT*/"%init_static_properties";
        public const string kGetterPrefix = /*NXLT*/"%get_";
        public const string kSetterPrefix = /*NXLT*/"%set_";
        public const string kLHS = /*NXLT*/"%lhs";
        public const string kRHS = /*NXLT*/"%rhs";
        public const string kTempFunctionReturnVar = /*NXLT*/"%tmpRet";
        public const string kTempDefaultArg = /*NXLT*/"%tmpDefaultArg";
        public const string kTempArg = /*NXLT*/"%targ";
        public const string kTempVar = /*NXLT*/"%tvar";
        public const string kTempPropertyVar = /*NXLT*/"%tvar_property";
        public const string kTempExceptionVar = /*NXLT*/"%texp";
        public const string kTempLangBlock = /*NXLT*/"%tempLangBlock";
        public const string kForLoopExpression = /*NXLT*/"%forloop_expr_";
        public const string kForLoopKey = /*NXLT*/"%forloop_key_";
        public const string kStartOfAutogenForloopIteration = /*NXLT*/"%autogen_forloop_iteration_";
        public const string kStartOfAutogenForloopCount = /*NXLT*/"%autogen_forloop_count_";
        public const string kFunctionPointerCall = /*NXLT*/"%FunctionPointerCall";
        public const string kFunctionRangeExpression = /*NXLT*/"%generate_range";
        public const string kDotMethodName = /*NXLT*/"%dot";
        public const string kDotArgMethodName = /*NXLT*/"%dotarg";
        public const string kInlineConditionalMethodName = /*NXLT*/"%inlineconditional";
        public const string kInlineCondition = /*NXLT*/"%InlineCondition";
        public const string kGetTypeMethodName = /*NXLT*/"%get_type";
        public const string kNodeAstFailed = /*NXLT*/"%nodeAstFailed";
        public const string kWatchResultVar = /*NXLT*/"watch_result_var";
        public const string kSSATempPrefix = /*NXLT*/"%tSSA_";
        public const string kGlobalInstanceNamePrefix = /*NXLT*/"%globalInstanceFunction_";
        public const string kGlobalInstanceFunctionPrefix = /*NXLT*/"%proc";
        public const string kThisPointerArgName = /*NXLT*/"%thisPtrArg";
        public const string kMangledFunctionPlaceholderName = /*NXLT*/"%Placeholder";
        public const string kTempModifierStateNamePrefix = /*NXLT*/"%tmp_modifierState_";
        public const string kTempProcConstant = /*NXLT*/"temp_proc_var_";
        public const string kTempProcLeftVar = /*NXLT*/"%" + kTempProcConstant;
        public const string kImportData = /*NXLT*/"ImportData";
        public const string kTempVarForNonAssignment = /*NXLT*/"temp_";
        public const char kLongestPostfix = /*NXLT*/'L';
        public const string kDoubleUnderscores = /*NXLT*/"__";
        public const string kSingleUnderscore = /*NXLT*/"_";
    }

    public enum MemoryRegion
    {
        kInvalidRegion = -1,
        kMemStatic,
        kMemStack,
        kMemHeap,
        kMemRegionTypes
    }
}
