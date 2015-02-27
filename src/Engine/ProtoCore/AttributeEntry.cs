using System.Collections.Generic;

namespace ProtoCore.DSASM
{
    public class AttributeEntry
    {
        public int ClassIndex { get; set; }
        public List<ProtoCore.AST.Node> Arguments { get; set; }
    }
}
