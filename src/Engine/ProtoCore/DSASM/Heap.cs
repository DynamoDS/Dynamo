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

        private int AllocSize { get; set; }
        public int VisibleSize { get; set; }
        public Dictionary<StackValue, StackValue> Dict;
        public StackValue[] Stack;
        public MetaData MetaData { get; set; }

        public int GetAllocatedSize()
        {
            return AllocSize;
        }

        public HeapElement(int size, int symbolindex)
        {
            AllocSize = VisibleSize = size;
            Dict = null; 
            Stack = new StackValue[AllocSize];

            for (int n = 0; n < AllocSize; ++n)
            {
                Stack[n] = StackValue.BuildInvalid();
            }
        }

        public HeapElement(StackValue[] arrayElements)
        {
            AllocSize = VisibleSize = arrayElements.Length;
            Stack = arrayElements;
        }

        private int GetNewSize(int size)
        {
            Validity.Assert(size > AllocSize);
            int nextSize = kInitialSize;
            if (size > kInitialSize)
            {
                // Determine the next allocation size
                nextSize = (int)(AllocSize * kReallocFactor) + AllocSize;

                // If the requested index is greater than the computed next allocation size, 
                // then the requested index is the next allocation size
                nextSize = (size >= nextSize) ? size : nextSize;
            }
            return nextSize;
        }

        //
        // TODO Jun: Optimize the reallocation routines
        //      1. Copying the temps can be optimized.
        //      2. Explore using List for the HeapStack stack. In this case we take advantage of .Net List arrays
        //
        private void ReAllocate(int size)
        {
            int newAllocatedSize = GetNewSize(size);

            // Copy current contents into a temp array
            StackValue[] tempstack = new StackValue[AllocSize];
            Stack.CopyTo(tempstack, 0);

            // Reallocate the array and copy the temp contents to it
            Stack = new StackValue[newAllocatedSize];
            tempstack.CopyTo(Stack, 0);

            for (int i = AllocSize; i < newAllocatedSize; ++i)
            {
                Stack[i] = StackValue.Null;
            }
            
            AllocSize = newAllocatedSize;
            Validity.Assert(size <= AllocSize);
        }

        private void RightShiftElements(int size)
        {
            Validity.Assert(VisibleSize + size <= AllocSize);
            if (size <= 0)
            {
                return;
            }

            for (int pos = VisibleSize - 1; pos >= 0; pos--)
            {
                int targetPos = pos + size;
                Stack[targetPos] = Stack[pos];
                Stack[pos] = StackValue.Null;
            }

            VisibleSize = VisibleSize + size;
        }

        public int ExpandByAcessingAt(int index)
        {
            int retIndex = index;

            if (index < 0)
            {
                if (index + VisibleSize < 0)
                {
                    int size = -index;
                    int shiftSize = size - (VisibleSize == 0 ? size : VisibleSize);

                    if (size > GetAllocatedSize())
                    {
                        ReAllocate(size);
                    }

                    RightShiftElements(shiftSize);
                    retIndex = 0;
                    VisibleSize = size;
                }
                else
                {
                    retIndex = index + VisibleSize;
                }
            }
            else if (index >= GetAllocatedSize())
            {
                ReAllocate(index + 1);
                VisibleSize = index + 1;
            }

            if (retIndex >= VisibleSize)
            {
                VisibleSize = retIndex + 1;
            }
            return retIndex;
        }

        public void Free()
        {
        }

        public IEnumerable<StackValue> VisibleItems
        {
            get
            {
                for (int i = 0; i < this.VisibleSize; ++i)
                {
                    yield return this.Stack[i];
                }
            }
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
                string s = runtimeCore.Heap.GetString(value);
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

        /// <summary>
        /// Allocate an array.
        /// </summary>
        /// <param name="values">Array elements whose indices are integer</param>
        /// <param name="dict">Array elements whose indices are not integer</param>
        /// <returns></returns>
        public StackValue AllocateArray(IEnumerable<StackValue> values, 
                                        Dictionary<StackValue, StackValue> dict = null)
        {
            int index = AllocateInternal(values);
            var heapElement = heapElements[index];
            heapElement.Dict = dict;
            heapElement.MetaData = new MetaData { type = (int)PrimitiveType.kTypeArray };
            return StackValue.BuildArrayPointer(index);
        }

        /// <summary>
        /// Allocate an object pointer.
        /// </summary>
        /// <param name="values">Values of object properties</param>
        /// <param name="metaData">Object type</param>
        /// <returns></returns>
        public StackValue AllocatePointer(IEnumerable<StackValue> values, 
                                          MetaData metaData)
        {
            int index = AllocateInternal(values);
            var heapElement = heapElements[index];
            heapElement.MetaData = metaData;
            return StackValue.BuildPointer(index, metaData);
        }

        /// <summary>
        /// Allocate an object pointer.
        /// </summary>
        /// <param name="values">Values of object properties</param>
        /// <returns></returns>
        public StackValue AllocatePointer(IEnumerable<StackValue> values)
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
            int index = AllocateInternal(size);
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
        private int AllocateStringInternal(string str)
        {
            int index;
            if (!stringTable.TryGetPointer(str, out index))
            {
                index = AllocateInternal(Enumerable.Empty<StackValue>());
                stringTable.AddString(index, str);
                heapElements[index].MetaData = new MetaData { type = (int)PrimitiveType.kTypeString};
            }
            return index;
        }

        /// <summary>
        /// Allocate string constant. String constant won't be garbage collected.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public StackValue AllocateFixedString(string str)
        {
            int index = AllocateStringInternal(str);
            fixedHeapElements.Add(index);
            return StackValue.BuildString(index);
        }

        /// <summary>
        /// Allocate string. 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public StackValue AllocateString(string str)
        {
            int index = AllocateStringInternal(str);
            return StackValue.BuildString(index);
        }

        /// <summary>
        /// Get string that pointer represents.
        /// </summary>
        /// <param name="pointer"></param>
        /// <returns></returns>
        public string GetString(StackValue pointer)
        {
            if (!pointer.IsString)
                return null;

            int index = (int)pointer.opdata;
            Validity.Assert(index >= 0 && index < heapElements.Count);

            string s;
            if (stringTable.TryGetString(index, out s))
                return s;
            else
                return null;
        }

        /// <summary>
        /// Get HeapElement that pointer represents.
        /// </summary>
        /// <param name="pointer"></param>
        /// <returns></returns>
        public HeapElement GetHeapElement(StackValue pointer)
        {
            int index = (int)pointer.opdata;
            var heapElement = heapElements[index];
            return heapElement;
        }

        public bool TryGetHeapElement(StackValue pointer, out HeapElement heapElement)
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

        private int AllocateInternal(int size)
        {
            HeapElement hpe = new HeapElement(size, Constants.kInvalidIndex);
            return AddHeapElement(hpe);
        }

        private int AllocateInternal(IEnumerable<StackValue> values)
        {
            int size = values.Count();
            int index = AllocateInternal(size);
            var heapElement = heapElements[index];

            int i = 0;
            foreach (var item in values)
            {
                heapElement.Stack[i] = item;
                i++;
            }
            return index;
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
                    var subElements = heapElement.VisibleItems;
                    if (heapElement.Dict != null)
                    {
                        subElements = subElements.Concat(heapElement.Dict.Keys)
                            .Concat(heapElement.Dict.Values);
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
            if (heapElement.VisibleSize > 0)
            {
                // Traverse each element in the heap
                foreach (StackValue sv in heapElement.Stack)
                {
                    // Is it a pointer
                    if (sv.IsReferenceType)
                    {
                        // Check if the current element in the heap points to the original pointer
                        if (sv.opdata == HeapID)
                        {
                            return true;
                        }
                        return IsHeapCyclic(GetHeapElement(sv),  HeapID);
                    }
                }
            }
            return false;
        }
        #endregion
    }
}
