using System.Collections.Generic;

namespace ProtoCore.DSASM
{
    public class AttributeEntry
    {
        public ClassNode ClassNode { get; set; }
        public List<ProtoCore.AST.Node> Arguments { get; set; }

        public bool IsInternalClassAttribute()
        {
            return ClassNode.Name.StartsWith(InternalAttributes.kInternalClassName);
        }
    }
}
