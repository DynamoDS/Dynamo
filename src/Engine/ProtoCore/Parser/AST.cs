namespace ProtoCore.AST
{
    using System.Collections.Generic;

    public abstract class Node
    {
        private static int sID;
        public int line { get; set; }
        public int col { get; set; }
        public int endLine { get; set; }
        public int endCol { get; set; }
        public int charPos { get; set; }
        public int endCharPos { get; set; }
        public string Name { get; set; }
        public int ID { get; private set; }
        public bool skipMe { get; set; }

        internal static readonly string BuiltinGetValueAtIndexTypeName = typeof(DesignScript.Builtin.Get).FullName;
        internal static readonly string BuiltinValueAtIndexMethodName = nameof(DesignScript.Builtin.Get.ValueAtIndex);

        internal static readonly string BuiltinValueAtIndexInForLoopMethodName =
            nameof(DesignScript.Builtin.Get.ValueAtIndexInForLoop);

        public Node()
        {
            ID = ++sID;
            line = ProtoCore.DSASM.Constants.kInvalidIndex;
            col = ProtoCore.DSASM.Constants.kInvalidIndex;
            endLine = ProtoCore.DSASM.Constants.kInvalidIndex;
            endCol = ProtoCore.DSASM.Constants.kInvalidIndex;
            charPos = ProtoCore.DSASM.Constants.kInvalidIndex;
            endCharPos = ProtoCore.DSASM.Constants.kInvalidIndex;
            skipMe = false;
            Name = string.Empty;
        }

        public Node(Node rhs)
        {
            ID = rhs.ID;
            line = rhs.line;
            col = rhs.col;
            endLine = rhs.endLine;
            endCol = rhs.endCol;
            charPos = rhs.charPos;
            endCharPos = rhs.charPos;
            Name = rhs.Name;
            skipMe = rhs.skipMe;
        }

        /// <summary>
        /// An explicit mechanism to manually set the ID of an AST node
        /// </summary>
        /// <param name="id"></param>
        public void InheritID(int id)
        {
            ID = id;
        }

        public abstract IEnumerable<Node> Children();
    }
}
