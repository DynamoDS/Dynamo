using System;
using ProtoCore.Properties;
using ProtoCore.Runtime;

namespace ProtoCore.DSASM
{
    public class DSObject : HeapElement
    {
        public DSObject(int size, Heap heap)
            : base(size, heap)
        {
            MetaData = new MetaData { type = (int)PrimitiveType.Pointer };
        }

        public DSObject(StackValue[] values, Heap heap)
            : base(values, heap)
        {
            MetaData = new MetaData { type = (int)PrimitiveType.Pointer };
        }

        public StackValue GetValueFromIndex(int index, RuntimeCore runtimeCore)
        {
            if (index >= Count || index < 0)
            {
                if (runtimeCore != null)
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.OverIndexing, Resources.kArrayOverIndexed);
                return StackValue.Null;
            }

            return GetValueAt(index);
        }

        public StackValue SetValueAtIndex(int index, StackValue value, RuntimeCore runtimeCore)
        {
            if (index >= Count || index < 0)
            {
                if (runtimeCore != null)
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.OverIndexing, Resources.kArrayOverIndexed);
                return StackValue.Null;
            }

            StackValue oldValue = GetValueAt(index);
            SetValueAt(index, value);
            return oldValue;
        }

        /// <summary>
        /// Expand the memory by specified size so that the object can contain
        /// extra information.
        ///
        /// Exception ProtoCore.Exceptions.RunOutOfMemoryException
        /// </summary>
        public void ExpandBySize(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentException("size is less than 0");
            }
            ExpandByAcessingAt(Count + size - 1);
        }
    }
}
