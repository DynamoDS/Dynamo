using System.Collections.Generic;

namespace ProtoCore.DSASM
{
    public enum InterpreterMode
    {
        Expression,
        Normal,
    }

    namespace CallingConvention
    {
        public enum BounceType
        {
            Explicit,  // Explicit bounce is using the same executive for the bounce target
            Implicit,  // Implicit bounce is using a new executive for the bounce target
        }

        public enum CallType
        {
            Explicit,      // Explicit call is using the same executive for the function call
            ExplicitBase,  // Explicit call to the base class
            Implicit,      // Implicit call is using a new executive for the function call
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
        StepSize,
        Number,
        ApproximateSize
    }

    //@TODO(Jun): This should be an enumeration, not a bunch of consts?

    // @TODO(Jun Comment), The actual enums are in InstructionSet.cs. 
    // These were intended for emitting assembly code (Currently to console, but perhaps also to a file)
    // We can then easily have a *.dsasm file containing only assembly code that the VM/Interpreter can execute
    public struct kw
    {
        public const string call = "call";
        public const string callr = "callr";
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
        public const string jmp = "jmp";
        public const string cjmp = "cjmp";
        public const string jdep = "jdep";
        public const string bounce = "bounce";
        public const string newarr = "newarr";
        public const string newobj = "newobj";
        public const string push = "push";
        public const string pushm = "pushm";
        public const string pushw = "pushw";
        public const string pushdep = "pushdep";
        public const string pushrepguide = "pushguide";
        public const string pushlevel = "pushlevel";
        public const string ret = "return";
        public const string retb = "retb";
        public const string retcn = "retcn";
        public const string pop = "pop";
        public const string popw = "popw";
        public const string popm = "popm";
        public const string poprepguides = "popguides";
        public const string poplevel = "poplevel";
        public const string dep = "dep";
        public const string setexpuid = "setexpuid";
        public const string pushb = "pushb";
        public const string pushvarsize = "pushvarsize";
        public const string loadelement = "loadelement";
        public const string setelement = "setelement";
        public const string setmemelement = "setmemelement";
        public const string cast = "cast";

        public const string regRX = "_rx";
        public const string regLX = "_lx";

        // TODO: Replace with ProtoCore.DSDefinitions.Keyword struct
        public const string associative = "Associative";
        public const string imperative = "Imperative";
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
        /// Returns the corresponding opcode of an operator.
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
        /// Returns the corresponding opcode of an unary operator.
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
        /// Returns the symbol representation of an operator. E.g., return "+"
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
        /// Returns the symbol representation of an unary operator. E.g., return
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
        /// Returns the string representation of an operator. E.g., return "add"
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
        /// Returns the string representation of an unary operator. E.g., return 
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
        /// Returns the internal function name for operator.
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public static string GetOpFunction(Operator op)
        {
            return Constants.kInternalNamePrefix + op.ToString();
        }

        /// <summary>
        /// Returns the internal function name for unary operator
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
            unaryOpCodeTable.Add(UnaryOperator.Neg, ProtoCore.DSASM.OpCode.NEG);
        }

        private static void initUnaryOpNameTable()
        {
            unaryOpNameTable = new Dictionary<UnaryOperator, string>();
            unaryOpNameTable.Add(UnaryOperator.None, "none");
            unaryOpNameTable.Add(UnaryOperator.Not, "not");
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
        }

        private static void initOpSymbolTable()
        {
            opSymbolTable = new Dictionary<Operator, string>();
            opSymbolTable.Add(Operator.add, "+");
            opSymbolTable.Add(Operator.sub, "-");
            opSymbolTable.Add(Operator.mul, "*");
            opSymbolTable.Add(Operator.div, "/");
            opSymbolTable.Add(Operator.mod, "%");
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
            unaryOpSymbolTable.Add(UnaryOperator.Neg, "-");
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
        public const int kDotCallArgCount = 6;
        public const int kDotArgIndexPtr = 0;
        public const int kDotArgIndexDynTableIndex = 1;
        public const int kDotArgIndexArrayIndex = 2;
        public const int kDotArgIndexDimCount = 3;
        public const int kDotArgIndexArrayArgs = 4;
        public const int kDotArgIndexArgCount = 5;

        // This is being moved to Core.Options as this needs to be overridden for the Watch test framework runner
        //public const int kDynamicCycleThreshold = 2000;
        public const int kRecursionTheshold = 1000;
 
        public const string termline = ";\n";
        public const string kInternalNamePrefix = "%";
        public const string kGetterPrefix = "get_";
        public const string kSetterPrefix = "set_";
        public const string kLHS = "%lhs";
        public const string kRHS = "%rhs";
        public const string kTempFunctionReturnVar = "%tmpRet";
        public const string kTempDefaultArg = "%tmpDefaultArg";
        public const string kTempArg = "%targ";
        public const string kTempVar = "%tvar";
        public const string kTempPropertyVar = "%t_property";
        public const string kTempLangBlock = "%tempLangBlock";
        public const string kForLoopExpression = "%forloop_expr_";
        public const string kForLoopKey = "%forloop_key_";
        public const string kForLoopCounter = "%forloop_count_";
        public const string kFunctionPointerCall = "%FunctionPointerCall";
        public const string kFunctionRangeExpression = "%generate_range";
        public const string kDotMethodName = "%dot";
        public const string kInlineConditionalMethodName = "%inlineconditional";
        public const string kGetTypeMethodName = "%get_type";
        public const string kNodeAstFailed = "%nodeAstFailed";
        public const string kWatchResultVar = "watch_result_var";
        public const string kSSATempPrefix = "%t_";
        public const string kThisPointerArgName = "%thisPtrArg";
        public const string kTempProcLeftVar = "%temp_proc_var_";
        public const string kImportData = "ImportData";
        public const string kTempVarForNonAssignment = "t6BBA4B28C5E54CF89F300D510499A00E_";
        public const char kLongestPostfix = 'L';
        public const string kDoubleUnderscores = "__";
        public const string kSingleUnderscore = "_";
        public const string kTempVarForTypedIdentifier = "%tTypedIdent";

        internal const string kInClassDecl = "InClassDecl";
        internal const string kInFunctionScope = "InFunctionScope";
        internal const string kInstance = "Instance";
    }

    public enum MemoryRegion
    {
        InvalidRegion = -1,
        MemStatic,
        MemStack,
    }
}
