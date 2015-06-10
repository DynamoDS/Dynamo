using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoCore.DSASM
{
    public class DSArray : HeapElement
    {
        private Dictionary<StackValue, StackValue> Dict;
        public DSArray(int size)
            : base(size)
        {
            Dict = new Dictionary<StackValue, StackValue>();
            MetaData = new MetaData { type = (int)PrimitiveType.kTypeArray };
        }
    }

    public class DSObject : HeapElement
    {
        public DSObject(int size)
            : base(size)
        {
            MetaData = new MetaData { type = (int)PrimitiveType.kTypePointer };
        }
    }

    public class DSString : HeapElement
    {
        public DSString(int size)
            : base(size)
        {
            MetaData = new MetaData { type = (int)PrimitiveType.kTypeString };
        }
    }
}
