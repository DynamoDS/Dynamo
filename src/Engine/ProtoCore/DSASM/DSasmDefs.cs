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

        ltd,
        gtd,
        led,
        ged,
        eqd,
        nqd,
        addd,
        subd,
        muld,
        divd,

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
        public const string mov = "mov";
        public const string call = "call";
        public const string callr = "callr";
        public const string callc = "callc";
        public const string add = "add";
        public const string sub = "sub";
        public const string mul = "mul";
        public const string div = "div";
        public const string mod = "mod";
        public const string eq = "eq";
        public const string nq = "nq";
        public const string gt = "gt";
        public const string lt = "lt";
        public const string ge = "ge";
        public const string le = "le";
        public const string addd = "addd";
        public const string subd = "subd";
        public const string muld = "muld";
        public const string divd = "divd";
        public const string eqd = "eqd";
        public const string nqd = "nqd";
        public const string gtd = "gtd";
        public const string ltd = "ltd";
        public const string ged = "ged";
        public const string led = "led";
        public const string jeq = "jeq";
        public const string jnq = "jnq";
        public const string jg = "jg";
        public const string jl = "jl";
        public const string jge = "jge";
        public const string jle = "jle";
        public const string jleq = "jleq";
        public const string jgeq = "jgeq";
        public const string jmp = "jmp";
        public const string cjmp = "cjmp";
        public const string jlz = "jlz";
        public const string jgz = "jgz";
        public const string jz = "jz";
        public const string label = "label";
        public const string bounce = "bounce";
        public const string alloca = "alloca";
        public const string allocc = "allocc";
        public const string push = "push";
        public const string pushg = "pushg";
        public const string pushm = "pushm";
        public const string pushw = "pushw";
        public const string pushindex = "pushindex";
        public const string pushdep = "pushdep";
        public const string pushlist = "pushlist";
        public const string ret = "ret";
        public const string retc = "retc";
        public const string retb = "retb";
        public const string retcn = "retcn";
        public const string pop = "pop";
        public const string popw = "popw";
        public const string popg = "popg";
        public const string popm = "popm";
        public const string poplist = "poplist";
        public const string not = "not";
        public const string negate = "negate";
        public const string dep = "dep";
        public const string depx = "depx";
        public const string setexpuid = "setexpuid";
        public const string pushb = "pushb";
        public const string popb = "popb";

        // TODO Jun: these are temporary instruction 
        public const string pushvarsize = "pushvarsize";

        public const string regAX = "_ax";
        public const string regBX = "_bx";
        public const string regCX = "_cx";
        public const string regDX = "_dx";
        public const string regEX = "_ex";
        public const string regFX = "_fx";
        public const string regRX = "_rx";
        public const string regSX = "_sx";
        public const string regLX = "_lx";
        public const string regTX = "_tx";

        public const string cast = "cast";
        public const string throwexception = "throw";


        public const string associative = "Associative";
        public const string imperative = "Imperative";
        public const string options = "Options";
    }

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
        /// Return the corresponding operator for handling floating point number.
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public static Operator GetFloatingOp(Operator op)
        {
            if (null == floatingOpTable)
            {
                initFloatingOpTable();
            }
            return floatingOpTable[op];
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
        private static Dictionary<Operator, Operator> floatingOpTable;

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

            opNameTable.Add(Operator.ltd, ProtoCore.DSASM.kw.ltd);
            opNameTable.Add(Operator.gtd, ProtoCore.DSASM.kw.gtd);
            opNameTable.Add(Operator.led, ProtoCore.DSASM.kw.led);
            opNameTable.Add(Operator.ged, ProtoCore.DSASM.kw.ged);
            opNameTable.Add(Operator.eqd, ProtoCore.DSASM.kw.eqd);
            opNameTable.Add(Operator.nqd, ProtoCore.DSASM.kw.nqd);
            opNameTable.Add(Operator.addd, ProtoCore.DSASM.kw.addd);
            opNameTable.Add(Operator.subd, ProtoCore.DSASM.kw.subd);
            opNameTable.Add(Operator.muld, ProtoCore.DSASM.kw.muld);
            opNameTable.Add(Operator.divd, ProtoCore.DSASM.kw.divd);
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

            opCodeTable.Add(Operator.ltd, ProtoCore.DSASM.OpCode.LTD);
            opCodeTable.Add(Operator.gtd, ProtoCore.DSASM.OpCode.GTD);
            opCodeTable.Add(Operator.led, ProtoCore.DSASM.OpCode.LED);
            opCodeTable.Add(Operator.ged, ProtoCore.DSASM.OpCode.GED);
            opCodeTable.Add(Operator.eqd, ProtoCore.DSASM.OpCode.EQD);
            opCodeTable.Add(Operator.nqd, ProtoCore.DSASM.OpCode.NQD);
            opCodeTable.Add(Operator.addd, ProtoCore.DSASM.OpCode.ADDD);
            opCodeTable.Add(Operator.subd, ProtoCore.DSASM.OpCode.SUBD);
            opCodeTable.Add(Operator.muld, ProtoCore.DSASM.OpCode.MULD);
            opCodeTable.Add(Operator.divd, ProtoCore.DSASM.OpCode.DIVD);
        }

        private static void initFloatingOpTable()
        {
            floatingOpTable = new Dictionary<Operator, Operator>();
            floatingOpTable.Add(Operator.lt, Operator.ltd);
            floatingOpTable.Add(Operator.gt, Operator.gtd);
            floatingOpTable.Add(Operator.le, Operator.led);
            floatingOpTable.Add(Operator.ge, Operator.ged);
            floatingOpTable.Add(Operator.eq, Operator.eqd);
            floatingOpTable.Add(Operator.nq, Operator.nqd);
            floatingOpTable.Add(Operator.add, Operator.addd);
            floatingOpTable.Add(Operator.sub, Operator.subd);
            floatingOpTable.Add(Operator.mul, Operator.muld);
            floatingOpTable.Add(Operator.div, Operator.divd);
        }

        private static void initOpSymbolTable()
        {
            opSymbolTable = new Dictionary<Operator, string>();
            opSymbolTable.Add(Operator.add, "+");
            opSymbolTable.Add(Operator.sub, "-");
            opSymbolTable.Add(Operator.mul, "*");
            opSymbolTable.Add(Operator.div, "/");
            opSymbolTable.Add(Operator.mod, "%");
            opSymbolTable.Add(Operator.bitwiseand, "&");
            opSymbolTable.Add(Operator.bitwiseor, "|");
            opSymbolTable.Add(Operator.bitwisexor, "^");
            opSymbolTable.Add(Operator.eq, "==");
            opSymbolTable.Add(Operator.nq, "!=");
            opSymbolTable.Add(Operator.ge, ">=");
            opSymbolTable.Add(Operator.gt, ">");
            opSymbolTable.Add(Operator.le, "<=");
            opSymbolTable.Add(Operator.lt, "<");
            opSymbolTable.Add(Operator.and, "&&");
            opSymbolTable.Add(Operator.or, "||");
            opSymbolTable.Add(Operator.assign, "=");
        }

        private static void initUnaryOpSymbolTable()
        {
            unaryOpSymbolTable = new Dictionary<UnaryOperator, string>();
            unaryOpSymbolTable.Add(UnaryOperator.Decrement, "--");
            unaryOpSymbolTable.Add(UnaryOperator.Increment, "++");
            unaryOpSymbolTable.Add(UnaryOperator.Neg, "-");
            unaryOpSymbolTable.Add(UnaryOperator.Negate, "~");
            unaryOpSymbolTable.Add(UnaryOperator.Not, "!");
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
 
        public const string termline = ";\n";
        public const string kInternalNamePrefix = "%";
        public const string kStaticPropertiesInitializer = "%init_static_properties";
        public const string kGetterPrefix = "%get_";
        public const string kSetterPrefix = "%set_";
        public const string kLHS = "%lhs";
        public const string kRHS = "%rhs";
        public const string kTempFunctionReturnVar = "%tmpRet";
        public const string kTempDefaultArg = "%tmpDefaultArg";
        public const string kTempArg = "%targ";
        public const string kTempVar = "%tvar";
        public const string kTempPropertyVar = "%tvar_property";
        public const string kTempExceptionVar = "%texp";
        public const string kTempLangBlock = "%tempLangBlock";
        public const string kForLoopExpression = "%forloop_expr_";
        public const string kForLoopKey = "%forloop_key_";
        public const string kStartOfAutogenForloopIteration = "%autogen_forloop_iteration_";
        public const string kStartOfAutogenForloopCount = "%autogen_forloop_count_";
        public const string kFunctionPointerCall = "%FunctionPointerCall";
        public const string kFunctionRangeExpression = "%generate_range";
        public const string kDotMethodName = "%dot";
        public const string kDotArgMethodName = "%dotarg";
        public const string kDotDynamicResolve = "%dotDynamicResolve";
        public const string kInlineConditionalMethodName = "%inlineconditional";
        public const string kInlineCondition = "%InlineCondition";
        public const string kGetTypeMethodName = "%get_type";
        public const string kWatchResultVar = "watch_result_var";
        public const string kSSATempPrefix = "%tSSA_";
        public const string kGlobalInstanceNamePrefix = "%globalInstanceFunction_";
        public const string kGlobalInstanceFunctionPrefix = "%proc";
        public const string kThisPointerArgName = "%thisPtrArg";
        public const string kMangledFunctionPlaceholderName = "%Placeholder";
        public const string kTempModifierStateNamePrefix = "%tmp_modifierState_";
        public const string kTempProcLeftVar = "%temp_proc_var_";
        public const string kImportData = "ImportData";
        public const char kLongestPostfix = 'L';
        public const string kDoubleUnderscores = "__";
    }

    public enum MemoryRegion
    {
        kInvalidRegion = -1,
        kMemStatic,
        kMemStack,
        kMemHeap,
        kMemRegionTypes
    }

    public enum AccessSpecifier
    {
       kPublic,
       kProtected,
       kPrivate
    }

    public enum AssociativeCompilePass
    {
        kClassName,
        kClassBaseClass,
        kClassHierarchy,
        kClassMemVar,
        
        kClassMemFuncSig,
        kGlobalFuncSig,

        kGlobalScope,

        kClassMemFuncBody,
        kGlobalFuncBody,
        kDone
    }

    public enum AssociativeSubCompilePass
    {
        kNone,
        kUnboundIdentifier,
        kGlobalInstanceFunctionBody,
        kAll
    }

    public enum ImperativeCompilePass
    {
        kGlobalFuncSig,
        kGlobalScope,
        kGlobalFuncBody,
        kDone
    }

}
