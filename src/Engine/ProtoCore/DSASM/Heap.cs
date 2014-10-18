
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

        public bool Active { get; set; }
        public int Symbol { get; set; }
        private int AllocSize { get; set; }
        public int VisibleSize { get; set; }
        public int Refcount { get; set; }
        public Dictionary<StackValue, StackValue> Dict;
        public StackValue[] Stack;
        public MetaData MetaData { get; set; }

        public int GetAllocatedSize()
        {
            return AllocSize;
        }

        public HeapElement(int size, int symbolindex)
        {
            Active = true;
            Symbol = symbolindex;
            AllocSize = VisibleSize = size;
            Refcount = 0;
            Dict = null; 
            Stack = new StackValue[AllocSize];

            for (int n = 0; n < AllocSize; ++n)
            {
                Stack[n] = StackValue.BuildInvalid();
            }
        }

        public HeapElement(StackValue[] arrayElements)
        {
            Active = true;
            Symbol = ProtoCore.DSASM.Constants.kInvalidIndex;
            AllocSize = VisibleSize = arrayElements.Length;
            Refcount = 0;
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
        private Core core;

        public StackValueComparer(Core core)
        {
            this.core = core;
        }

        public bool Equals(StackValue x, StackValue y)
        {
            return StackUtils.CompareStackValues(x, y, core, core);
        }

        public int GetHashCode(StackValue value)
        {
            if (value.IsString)
            {
                HeapElement he = ArrayUtils.GetHeapElement(value, core);
                int length = he.VisibleSize;

                unchecked
                {
                    int hash = 0;
                    int step = (length >> 5) + 1;
                    for (int i = he.VisibleSize; i >= step; i -= step)
                    {
                        hash = (hash * 397) ^ he.Stack[i - 1].opdata.GetHashCode();
                    }
                    return hash;
                }
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

    public class Heap
    {
        public enum GCStrategies
        {
            kReferenceCounting,
            kMarkAndSweep
        }

        private readonly List<int> freeList = new List<int>();
        private readonly List<HeapElement> heapElements = new List<HeapElement>();
        private bool isGarbageCollecting = false;

        public Heap()
        {
        }

        public GCStrategies GCStrategy
        {
            get
            {
#if GC_MARK_AND_SWEEP
                return Heap.GCStrategies.kMarkAndSweep;
#else
                return Heap.GCStrategies.kReferenceCounting;
#endif
            }
        }
        public StackValue AllocateString(string str)
        {
            var chs = str.Select(c => StackValue.BuildChar(c)).ToArray();
            int index = AllocateInternal(chs);
            var heapElement = heapElements[index];
            heapElement.MetaData = new MetaData { type = (int)PrimitiveType.kTypeString};
            return StackValue.BuildString(index);
        }

        public StackValue AllocateArray(IEnumerable<StackValue> values, 
                                        Dictionary<StackValue, StackValue> dict = null)
        {
            int index = AllocateInternal(values);
            var heapElement = heapElements[index];
            heapElement.Dict = dict;
            heapElement.MetaData = new MetaData { type = (int)PrimitiveType.kTypeArray };
            return StackValue.BuildArrayPointer(index);
        }

        public StackValue AllocatePointer(IEnumerable<StackValue> values, 
                                          MetaData metaData)
        {
            int index = AllocateInternal(values);
            var heapElement = heapElements[index];
            heapElement.MetaData = metaData;
            return StackValue.BuildPointer(index, metaData);
        }

        public StackValue AllocatePointer(IEnumerable<StackValue> values)
        {
            return AllocatePointer(
                    values, 
                    new MetaData { type = (int)PrimitiveType.kTypePointer });
        }

        public StackValue AllocatePointer(int size, MetaData metadata)
        {    
            int index = AllocateInternal(size);
            var hpe = heapElements[index];
            hpe.MetaData = metadata;
            return StackValue.BuildPointer(index, metadata);
        }

        public StackValue AllocatePointer(int size)
        {
            return AllocatePointer(
                    size, 
                    new MetaData { type = (int)PrimitiveType.kTypePointer });
        }

        public HeapElement GetHeapElement(StackValue pointer)
        {
            int index = (int)pointer.opdata;
            var heapElement = heapElements[index];

            if (!heapElement.Active)
            {
#if HEAP_VERIFICATION
                throw new Exception("Memory corrupted: Access dead memory (E4A2FC59-52DF-4F3B-8CD3-6C9E08F93AC5).");
#endif
            }

            return heapElement;
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
                exe.rmem.Push(StackValue.BuildBlockIndex(pn.runtimeIndex));
                exe.rmem.Push(StackValue.BuildArrayDimension(0));
                exe.rmem.Push(StackValue.BuildStaticType((int)PrimitiveType.kTypeVar));
                
                ++exe.Core.FunctionCallDepth;

                // TODO: Need to move IsExplicitCall to DebugProps and come up with a more elegant solution for this
                // fix for IDE-963 - pratapa
                bool explicitCall = exe.IsExplicitCall;
                bool tempFlag = explicitCall;
                exe.Callr(pn.procId, classIndex, 1, ref explicitCall);

                exe.IsExplicitCall = tempFlag;

                --exe.Core.FunctionCallDepth;
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
                    if (metaData.type >= (int)PrimitiveType.kMaxPrimitives)
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
        [Conditional("GC_REFERENCE_COUNTING")]
        public void IncRefCount(StackValue sv)
        {
            if (!sv.IsReferenceType)
            {
                return;
            }

            int ptr = (int)sv.opdata;

            this.heapElements[ptr].Refcount++;
            if (this.heapElements[ptr].Refcount > 0)
            {
                this.heapElements[ptr].Active = true;
            }
        }

        [Conditional("GC_REFERENCE_COUNTING")]
        public void DecRefCount(StackValue sv)
        {
            if (!sv.IsReferenceType)
            {
                return;
            }

            int ptr = (int)sv.opdata;
            if (this.heapElements[ptr].Refcount > 0)
            {
                this.heapElements[ptr].Refcount--;
            }
            else
            {
#if HEAP_VERIFICATION
                throw new Exception("Memory corrupted: Decrease reference count to negative (E4A2FC59-52DF-4F3B-8CD3-6C9E08F93AC5).");
#endif
            }
        }
    
        [Conditional("GC_REFERENCE_COUNTING")]
        public void GCRelease(StackValue[] ptrList, Executive exe)
        {
            for (int n = 0; n < ptrList.Length; ++n)
            {
                StackValue svPtr = ptrList[n];
                if (!svPtr.IsPointer && !svPtr.IsArray)
                {
                    continue;
                }

                int ptr = (int)svPtr.opdata;
                if (ptr < 0 || ptr >= heapElements.Count)
                {
#if HEAP_VERIFICATION
                    throw new Exception("Memory corrupted: Release invalid pointer (7364B8C2-FF34-4C67-8DFE-5DFA678BF50D).");
#else
                    continue;
#endif
                }
                HeapElement hs = heapElements[ptr];

                if (!hs.Active)
                {
#if HEAP_VERIFICATION
                    throw new Exception("Memory corrupted: Release dead memory (7F70A6A1-FE99-476E-BE8B-CA7615EE1A3B).");
#else
                    continue;
#endif
                }
                
                // The reference count could be 0 if this heap object
                // is a temporary heap object that hasn't been assigned
                // to any variable yet, for example, Type.Coerce() may 
                // allocate a new array and when this one is type converted
                // again, it will be released. 
                if (hs.Refcount > 0)
                {
                    hs.Refcount--;
                }

                // TODO Jun: If its a pointer to a primitive then dont decrease its refcount, just free it
                if (hs.Refcount == 0)
                {
                    // if it is of class type, first call its destructor before clean its members
                    if(svPtr.IsPointer)
                        GCDisposeObject(svPtr, exe);

                    if (svPtr.IsArray && hs.Dict != null)
                    {
                        foreach (var item in hs.Dict)
                        {
                            GCRelease(new StackValue[] {item.Key}, exe);
                            GCRelease(new StackValue[] {item.Value}, exe);
                        }
                    }

                    hs.Dict = null;
                    hs.Active = false;

                    GCRelease(hs.Stack, exe);
#if !HEAP_VERIFICATION
                    freeList.Add(ptr);
#endif
                }
            }
        }

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
            if (heapElement.Active && heapElement.VisibleSize > 0)
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

        /// <summary>
        /// Verify the heap integrity by performing tests on the current state of the heap
        /// Throws an exception if the heap is corrupted
        /// </summary>
        /// <param name="core"></param>
        public void Verify()
        {
            // Check the integrity of the heap memory layout
            if (IsHeapCyclic())
            {
                throw new ProtoCore.Exceptions.HeapCorruptionException("Heap contains cyclic pointers.");
            }
        }
        #endregion
    }
}
