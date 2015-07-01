using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;

using ProtoCore.Utils;


namespace ProtoCore.DSASM
{
    public class HeapElement
    {
        private const int kInitialSize = 5;
        private const double kReallocFactor = 0.5;

        protected Heap heap;
        private int allocated;

        private StackValue[] values;
        public virtual IEnumerable<StackValue> Values
        {
            get 
            { 
                return values.Take(Count);
            }
        }

        public int Count { get; protected set; }
        public MetaData MetaData { get; set; }

        /// <summary>
        /// Create HeapElement
        /// </summary>
        /// <param name="size"></param>
        /// <param name="heap"></param>
        public HeapElement(int size, Heap heap)
        {
            allocated = Count = size;
            this.heap = heap;
            values = Enumerable.Repeat(StackValue.BuildInvalid(), allocated).ToArray();
        }

        /// <summary>
        /// Create HeapElement based on the existing values
        /// </summary>
        /// <param name="values"></param>
        /// <param name="heap"></param>
        public HeapElement(StackValue[] values, Heap heap)
        {
            allocated = Count = values.Count();
            this.values = new StackValue[allocated];
            this.heap = heap;
            Array.Copy(values, this.values, allocated);
        }

        //
        // TODO Jun: Optimize the reallocation routines
        //      1. Copying the temps can be optimized.
        //      2. Explore using List for the HeapStack stack. In this case we take advantage of .Net List arrays
        //
        private void ReAllocate(int size)
        {
            int newSize = kInitialSize;
            if (size > kInitialSize)
            {
                // Determine the next allocation size
                newSize = (int)(allocated * kReallocFactor) + allocated;

                // If the requested index is greater than the computed next allocation size, 
                // then the requested index is the next allocation size
                newSize = (size >= newSize) ? size : newSize;
            }

            // Copy current contents into a temp array
            StackValue[] tempstack = new StackValue[allocated];
            values.CopyTo(tempstack, 0);

            // Reallocate the array and copy the temp contents to it
            values = new StackValue[newSize];
            tempstack.CopyTo(values, 0);

            for (int i = allocated; i < newSize; ++i)
            {
                values[i] = StackValue.Null;
            }

            allocated = newSize;
            Validity.Assert(size <= allocated);
        }

        private void RightShiftElements(int size)
        {
            Validity.Assert(Count + size <= allocated);
            if (size <= 0)
            {
                return;
            }

            for (int pos = Count - 1; pos >= 0; pos--)
            {
                int targetPos = pos + size;
                values[targetPos] = values[pos];
                values[pos] = StackValue.Null;
            }

            Count = Count + size;
        }

        protected int ExpandByAcessingAt(int index)
        {
            int retIndex = index;

            if (index < 0)
            {
                if (index + Count < 0)
                {
                    int size = -index;
                    int shiftSize = size - (Count == 0 ? size : Count);

                    if (size > allocated)
                    {
                        ReAllocate(size);
                    }

                    RightShiftElements(shiftSize);
                    retIndex = 0;
                    Count = size;
                }
                else
                {
                    retIndex = index + Count;
                }
            }
            else if (index >= allocated)
            {
                ReAllocate(index + 1);
                Count = index + 1;
            }

            if (retIndex >= Count)
            {
                Count = retIndex + 1;
            }
            return retIndex;
        }

        protected StackValue GetValueAt(int index)
        {
            return values[index];
        }

        protected void SetValueAt(int index, StackValue value)
        {
            values[index] = value;
        }
    }

    public class StackValueComparer : IEqualityComparer<StackValue>
    {
        private RuntimeCore runtimeCore;

        public StackValueComparer(RuntimeCore runtimeCore)
        {
            this.runtimeCore = runtimeCore;
        }

        public bool Equals(StackValue x, StackValue y)
        {
            return StackUtils.CompareStackValues(x, y, runtimeCore, runtimeCore);
        }

        public int GetHashCode(StackValue value)
        {
            if (value.IsString)
            {
                string s = runtimeCore.Heap.ToHeapObject<DSString>(value).Value;
                return s.GetHashCode();
            }
            else
            {
                unchecked
                {
                    int hash = 0;
                    hash = (hash * 397) ^ value.opdata.GetHashCode();
                    hash = (hash * 397) ^ value.metaData.type.GetHashCode();
                    return hash;
                }
            }
        }
    }

    /// <summary>
    /// String table to store all DS strings. 
    /// </summary>
    internal class StringTable
    {
        private Dictionary<string, int> stringToPointerTable;
        private Dictionary<int, string> pointerToStringTable;

        internal StringTable()
        {
            stringToPointerTable = new Dictionary<string, int>();
            pointerToStringTable = new Dictionary<int, string>();
        }

        /// <summary>
        /// Add string to the string table. 
        /// </summary>
        /// <param name="pointer">The index of HeapElement that represents the string</param>
        /// <param name="s"></param>
        internal void AddString(int pointer, string s)
        {
            stringToPointerTable[s] = pointer;
            pointerToStringTable[pointer] = s;
        }

        /// <summary>
        /// Get string from the string table.
        /// </summary>
        /// <param name="pointer">The index of HeapElement that represents the string</param>
        /// <param name="s"></param>
        /// <returns></returns>
        internal bool TryGetString(int pointer, out string s)
        {
            return pointerToStringTable.TryGetValue(pointer, out s);
        }

        /// <summary>
        /// Get the index of HeapElement that represents the string
        /// </summary>
        /// <param name="s"></param>
        /// <param name="pointer"></param>
        /// <returns></returns>
        internal bool TryGetPointer(string s, out int pointer)
        {
            return stringToPointerTable.TryGetValue(s, out pointer);
        }

        internal bool TryRemoveString(int pointer)
        {
            string stringToBeRemoved = null;
            if (!pointerToStringTable.TryGetValue(pointer, out stringToBeRemoved))
                return false;

            pointerToStringTable.Remove(pointer);
            stringToPointerTable.Remove(stringToBeRemoved);
            return true;
        }
    }

    public class Heap
    {
        private readonly List<int> freeList = new List<int>();
        private readonly List<HeapElement> heapElements = new List<HeapElement>();
        private HashSet<int> fixedHeapElements = new HashSet<int>(); 
        private StringTable stringTable = new StringTable();
        private bool isGarbageCollecting = false;

        public Heap()
        {
        }

        public T ToHeapObject<T>(StackValue heapObject) where T : HeapElement
        {
            HeapElement heapElement;
            if (!TryGetHeapElement(heapObject, out heapElement))
                return null;

            if (!(heapElement is T))
                throw new InvalidCastException();

            return heapElement as T; 
        }

        /// <summary>
        /// Allocate an array.
        /// </summary>
        /// <param name="values">Array elements whose indices are integer</param>
        /// <param name="dict">Array elements whose indices are not integer</param>
        /// <returns></returns>
        public StackValue AllocateArray(StackValue[] values)
        {
            int index = AllocateInternal(values, PrimitiveType.kTypeArray);
            var heapElement = heapElements[index];
            heapElement.MetaData = new MetaData { type = (int)PrimitiveType.kTypeArray };
            return StackValue.BuildArrayPointer(index);
        }

        /// <summary>
        /// Allocate an object pointer.
        /// </summary>
        /// <param name="values">Values of object properties</param>
        /// <param name="metaData">Object type</param>
        /// <returns></returns>
        public StackValue AllocatePointer(StackValue[] values, 
                                          MetaData metaData)
        {
            int index = AllocateInternal(values, PrimitiveType.kTypePointer);
            var heapElement = heapElements[index];
            heapElement.MetaData = metaData;
            return StackValue.BuildPointer(index, metaData);
        }

        /// <summary>
        /// Allocate an object pointer.
        /// </summary>
        /// <param name="values">Values of object properties</param>
        /// <returns></returns>
        public StackValue AllocatePointer(StackValue[] values)
        {
            return AllocatePointer(
                    values, 
                    new MetaData { type = (int)PrimitiveType.kTypePointer });
        }

        /// <summary>
        /// Allocate an object pointer.
        /// </summary>
        /// <param name="size">The size of object properties.</param>
        /// <param name="metadata">Object type</param>
        /// <returns></returns>
        public StackValue AllocatePointer(int size, MetaData metadata)
        {    
            int index = AllocateInternal(size, PrimitiveType.kTypePointer);
            var hpe = heapElements[index];
            hpe.MetaData = metadata;
            return StackValue.BuildPointer(index, metadata);
        }

        /// <summary>
        /// Allocate an object pointer.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public StackValue AllocatePointer(int size)
        {
            return AllocatePointer(
                    size, 
                    new MetaData { type = (int)PrimitiveType.kTypePointer });
        }

        /// <summary>
        /// Allocate a string, the string will be put in string table.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private StackValue AllocateStringInternal(string str, bool isConstant)
        {
            int index;
            if (!stringTable.TryGetPointer(str, out index))
            {
                index = AllocateInternal(new StackValue[] {}, PrimitiveType.kTypeString);
                stringTable.AddString(index, str);
            }

            if (isConstant)
                fixedHeapElements.Add(index);

            var svString = StackValue.BuildString(index);
            DSString dsstring = ToHeapObject<DSString>(svString);
            dsstring.SetPointer(svString);
            return svString;
        }

        /// <summary>
        /// Allocate string constant. String constant won't be garbage collected.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public StackValue AllocateFixedString(string str)
        {
            return AllocateStringInternal(str, true);
        }

        /// <summary>
        /// Allocate string. 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public StackValue AllocateString(string str)
        {
            return AllocateStringInternal(str, false);
        }

        /// <summary>
        /// Get string that pointer represents.
        /// </summary>
        /// <param name="pointer"></param>
        /// <returns></returns>
        public string GetString(DSString dsString)
        {
            int index = (int)dsString.Pointer.opdata;
            Validity.Assert(index >= 0 && index < heapElements.Count);

            string s;
            if (stringTable.TryGetString(index, out s))
                return s;
            else
                return null;
        }

        private bool TryGetHeapElement(StackValue pointer, out HeapElement heapElement)
        {
            heapElement = null;
            int index = (int)pointer.opdata;

            if (index >= 0 && index < heapElements.Count)
            {
                heapElement = heapElements[index];
            }
            return heapElement != null;
        }

        public void Free()
        {
            heapElements.Clear();
            freeList.Clear();
        }

        private int AllocateInternal(int size, PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.kTypeArray:
                    var dsArray = new DSArray(size, this);
                    return AddHeapElement(dsArray);

                case PrimitiveType.kTypePointer:
                    var dsObject = new DSObject(size, this);
                    return AddHeapElement(dsObject);

                case PrimitiveType.kTypeString:
                    var dsString = new DSString(size, this);
                    return AddHeapElement(dsString);

                default:
                    throw new ArgumentException("type");
            }
        }

        private int AllocateInternal(StackValue[] values, PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.kTypeArray:
                    var dsArray = new DSArray(values, this);
                    return AddHeapElement(dsArray);

                case PrimitiveType.kTypePointer:
                    var dsObject = new DSObject(values, this);
                    return AddHeapElement(dsObject);

                case PrimitiveType.kTypeString:
                    var dsString = new DSString(values, this);
                    return AddHeapElement(dsString);

                default:
                    throw new ArgumentException("type");
            }
        }

        private int AddHeapElement(HeapElement hpe)
        {
            int index;
            if (TryFindFreeIndex(out index))
            {
                heapElements[index] = hpe;
            }
            else
            {
                heapElements.Add(hpe);
                index = heapElements.Count - 1;
            }
 
            return index;
        }

        private bool TryFindFreeIndex(out int index)
        {
            int freeItemCount = freeList.Count;
            if (freeItemCount > 0)
            {
                index = freeList[freeItemCount - 1];
                freeList.RemoveAt(freeItemCount - 1);
                return true;
            }
            else
            {
                index = Constants.kInvalidIndex;
                return false;
            }
        }

        private void GCDisposeObject(StackValue svPtr, Executive exe)
        {
            int classIndex = svPtr.metaData.type;
            ClassNode cn = exe.exe.classTable.ClassNodes[classIndex];

            ProcedureNode pn = cn.GetDisposeMethod();
            while (pn == null)
            {
                if (cn.baseList != null && cn.baseList.Count != 0) 
                {
                    classIndex = cn.baseList[0];
                    cn = exe.exe.classTable.ClassNodes[classIndex];
                    pn = cn.GetDisposeMethod();
                }
                else
                {
                    break;
                }
            }

            if (pn != null)
            {
                // TODO Jun/Jiong: Use build pointer utilities 
                exe.rmem.Push(StackValue.BuildArrayDimension(0));
                exe.rmem.Push(StackValue.BuildPointer(svPtr.opdata, svPtr.metaData));
                exe.rmem.Push(StackValue.BuildArrayDimension(0));
                exe.rmem.Push(StackValue.BuildStaticType((int)PrimitiveType.kTypeVar));
                exe.rmem.Push(StackValue.BuildInt(1));
                
                ++exe.RuntimeCore.FunctionCallDepth;

                // TODO: Need to move IsExplicitCall to DebugProps and come up with a more elegant solution for this
                // fix for IDE-963 - pratapa
                bool explicitCall = exe.IsExplicitCall;
                bool tempFlag = explicitCall;
                exe.Callr(pn.runtimeIndex, pn.procId, classIndex, ref explicitCall);

                exe.IsExplicitCall = tempFlag;

                --exe.RuntimeCore.FunctionCallDepth;
            }
        }

        public void GCMarkAndSweep(List<StackValue> rootPointers, Executive exe)
        {
            if (isGarbageCollecting)
                return;

            try
            {
                isGarbageCollecting = true;

                // Mark
                var count = heapElements.Count;
                var markBits = new BitArray(count);
                foreach (var index in fixedHeapElements)
                {
                    markBits.Set(index, true); 
                }

                var workingStack = new Stack<StackValue>(rootPointers);
                while (workingStack.Any())
                {
                    var pointer = workingStack.Pop();
                    var ptr = (int)pointer.RawIntValue;
                    if (!pointer.IsReferenceType || markBits.Get(ptr))
                    {
                        continue;
                    }

                    markBits.Set(ptr, true);

                    var heapElement = heapElements[ptr];
                    IEnumerable<StackValue> subElements = Enumerable.Empty<StackValue>();

                    if (pointer.IsArray)
                    {
                        var array = ToHeapObject<DSArray>(pointer);
                        var dict = array.ToDictionary();
                        subElements = subElements.Concat(dict.Keys).Concat(dict.Values);
                    }
                    else
                    {
                        subElements = heapElement.Values;
                    }

                    foreach (var subElement in subElements)
                    {
                        if (subElement.IsReferenceType &&
                            !markBits.Get((int)subElement.RawIntValue))
                        {
                            workingStack.Push(subElement);
                        }
                    }
                }

                // Sweep
                for (int i = 0; i < count; ++i)
                {
                    if (markBits.Get(i) || heapElements[i] == null)
                    {
                        continue;
                    }

                    var metaData = heapElements[i].MetaData;
                    if (metaData.type == (int)PrimitiveType.kTypeString)
                    {
                        stringTable.TryRemoveString(i);
                    }
                    else if (metaData.type >= (int)PrimitiveType.kMaxPrimitives)
                    {
                        var objPointer = StackValue.BuildPointer(i, metaData);
                        GCDisposeObject(objPointer, exe);
                    }

                    heapElements[i] = null;

#if !HEAP_VERIFICATION
                    freeList.Add(i);
#endif
                }
            }
            finally
            {
                isGarbageCollecting = false;
            }
        }


        #region Reference counting APIs
        /// <summary>
        /// Checks if the heap contains at least 1 pointer element that points to itself
        /// This function is used as a diagnostic tool for detecting heap cycles and should never return true
        /// </summary>
        /// <returns> Returns true if the heap contains at least one cycle</returns>
        public bool IsHeapCyclic()
        {
            for (int n = 0; n < heapElements.Count; ++n)
            {
                HeapElement heapElem = heapElements[n];
                if (IsHeapCyclic(heapElem, n))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if the heap element is cyclic. 
        /// Traverses the pointer element and determines it points to itself
        /// </summary>
        /// <param name="heapElement"></param>
        /// <param name="core"></param>
        /// <returns> Returns true if the array contains a cycle </returns>
        private bool IsHeapCyclic(HeapElement heapElement, int HeapID)
        {
            if (heapElement.Count > 0)
            {
                // Traverse each element in the heap
                foreach (StackValue sv in heapElement.Values)
                {
                    // Is it a pointer
                    if (sv.IsReferenceType)
                    {
                        // Check if the current element in the heap points to the original pointer
                        if (sv.opdata == HeapID)
                        {
                            return true;
                        }

                        HeapElement hpe;
                        TryGetHeapElement(sv, out hpe);
                        return IsHeapCyclic(hpe, HeapID);
                    }
                }
            }
            return false;
        }
        #endregion
    }
}
