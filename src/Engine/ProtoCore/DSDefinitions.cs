namespace ProtoCore.DSDefinitions
{
    public struct Keyword
    {
        public const string Native = "native";
        public const string Class = "class";
        public const string Constructor = "constructor";
        public const string Def = "def";
        public const string External = "external";
        public const string Extend = "extends";
        public const string Heap = "__heap";
        public const string If = "if";
        public const string Elseif = "elseif";
        public const string Else = "else";
        public const string While = "while";
        public const string For = "for";
        public const string In = "in";
        public const string Import = "import";
        public const string From = "from";
        public const string Break = "break";
        public const string Continue = "continue";
        public const string This = "this";
        public const string Return = "return";
        public const string Int = "int";
        public const string Double = "double";
        public const string String = "string";
        public const string Bool = "bool";
        public const string Var = "var";
        public const string Char = "char";
        public const string Void = "void";
        public const string Array = "__array";
        public const string Null = "null";
        public const string FunctionPointer = "function";
        public const string True = "true";
        public const string False = "false";
        public const string Public = "public";
        public const string Protected = "protected";
        public const string Private = "private";
        public const string Static = "static";
        public const string Dispose = "_Dispose";
        public const string Invalid = "__invalid";
        public static string[] KeywordList = {Native, Class, Constructor, Def, External, Extend, Heap,
                                        If, Elseif, Else, While, For, Import, From, Break,
                                        Continue, Return, Int, Double, String, Bool, Var,
                                        Char, Void, Null, FunctionPointer, True, 
                                        False, Public, Protected, Private, Static};
    }
}
