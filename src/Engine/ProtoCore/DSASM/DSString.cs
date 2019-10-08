using ProtoCore.Properties;
using ProtoCore.Runtime;

namespace ProtoCore.DSASM
{
    public class DSString : HeapElement
    {
        public StackValue Pointer = StackValue.Null;

        public DSString(int size, Heap heap)
            : base(size, heap)
        {
            MetaData = new MetaData { type = (int)PrimitiveType.String };
        }

        public DSString(StackValue[] values, Heap heap)
            : base(values, heap)
        {
            MetaData = new MetaData { type = (int)PrimitiveType.String };
        }

        public void SetPointer(StackValue pointer)
        {
            this.Pointer = pointer;
        }

        public string Value
        {
            get
            {
                return heap.GetString(this);
            }
        }

        public override int MemorySize
        {
            get
            {
                return 0;
            }
        }

        public StackValue GetValueAtIndex(int index, RuntimeCore runtimeCore)
        {
            string str = heap.GetString(this);
            if (str == null)
                return StackValue.Null;

            if (index < 0)
            {
                index = index + str.Length;
            }

            if (index >= str.Length || index < 0)
            {
                if (runtimeCore != null)
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.OverIndexing, Resources.kStringOverIndexed);
                return StackValue.Null;
            }

            return heap.AllocateString(str.Substring(index, 1));
        }

        public StackValue GetValueAtIndex(StackValue index, RuntimeCore runtimeCore)
        {
            int pos = Constants.kInvalidIndex;
            if (index.IsNumeric)
            {
                pos = (int)index.ToInteger().IntegerValue;
                return GetValueAtIndex(pos, runtimeCore);
            }
            else if (index.IsArrayKey)
            {
                StackValue array;
                index.TryGetArrayKey(out array, out pos);
                return GetValueAtIndex(pos, runtimeCore);
            }
            else
            {
                if (runtimeCore != null)
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.InvalidIndexing, Resources.kInvalidArguments);
                return StackValue.Null;
            }
        }

        public override bool Equals(object obj)
        {
            DSString otherString = obj as DSString;
            if (otherString == null)
                return false;

            if (object.ReferenceEquals(heap, otherString.heap) && Pointer.Equals(otherString.Pointer))
                return true;

            return Value.Equals(otherString.Value);
        }
    }
}
