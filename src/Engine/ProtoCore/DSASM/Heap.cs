using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using ProtoCore.Utils;

namespace ProtoCore.DSASM
{
    public enum GCState
    {
        Pause,
        WaitingForRoots,
        Propagate,
        Sweep,
    }

    public enum GCMark
    {
        White,
        Gray,
        Black
    }

    public class HeapElement
    {
        private const int kInitialSize = 5;
        private const double kReallocFactor = 0.5;

        protected Heap heap;
        private int allocated;
        private StackValue[] items;

        public int VisibleSize { get; set; }
        public MetaData MetaData { get; set; }
        public GCMark Mark { get; set; }

        public HeapElement(int size, Heap heap)
        {
            allocated = VisibleSize = size;
            items = new StackValue[allocated];
            this.heap = heap;

            for (int n = 0; n < allocated; ++n)
            {
                items[n] = StackValue.BuildInvalid();
            }
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
            items.CopyTo(tempstack, 0);

            // Reallocate the array and copy the temp contents to it
            items = new StackValue[newSize];
            tempstack.CopyTo(items, 0);

            for (int i = allocated; i < newSize; ++i)
            {
                items[i] = StackValue.Null;
            }

            // We should move StackValue list to heap. That is, heap
            // manages StackValues instead of HeapElement itself.
            heap.ReportAllocation(size - allocated);

            allocated = newSize;
            Validity.Assert(size <= allocated);
        }

        private void RightShiftElements(int size)
        {
            Validity.Assert(VisibleSize + size <= allocated);
            if (size <= 0)
            {
                return;
            }

            for (int pos = VisibleSize - 1; pos >= 0; pos--)
            {
                int targetPos = pos + size;
                items[targetPos] = items[pos];
                items[pos] = StackValue.Null;
            }

            VisibleSize = VisibleSize + size;
        }

        protected int ExpandByAcessingAt(int index)
        {
            int retIndex = index;

            if (index < 0)
            {
                if (index + VisibleSize < 0)
                {
                    int size = -index;
                    int shiftSize = size - (VisibleSize == 0 ? size : VisibleSize);

                    if (size > allocated)
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
            else if (index >= allocated)
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

        public IEnumerable<StackValue> VisibleItems
        {
            get
            {
                for (int i = 0; i < this.VisibleSize; ++i)
                {
                    yield return this.items[i];
                }
            }
        }

        protected StackValue GetItemAt(int index)
        {
            return items[index];
        }

        public void SetItemAt(int index, StackValue value)
        {
            items[index] = value;
        }

        public virtual int MemorySize
        {
            get
            {
                return VisibleSize;
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
        private readonly StringTable stringTable = new StringTable();

        // Totaly allocated StackValues
        private int totalAllocated = 0;
        private int totalTraversed = 0;

        private LinkedList<StackValue> grayList;
        private HashSet<int> sweepSet;
        private List<StackValue> roots;
        private Executive executive;

        public bool IsGCRunning { get; private set; }
        public GCState gcState = GCState.Pause;

        public Heap()
        {
            IsGCRunning = false;
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
        public StackValue AllocateArray(IEnumerable<StackValue> values)
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
        public StackValue AllocatePointer(IEnumerable<StackValue> values, 
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
                index = AllocateInternal(Enumerable.Empty<StackValue>(), PrimitiveType.kTypeString);
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
            HeapElement hpe = null;

            switch (type)
            {
                case PrimitiveType.kTypeArray:
                    hpe = new DSArray(size, this);
                    break;

                case PrimitiveType.kTypePointer:
                    hpe = new DSObject(size, this);
                    break;

                case PrimitiveType.kTypeString:
                    hpe = new DSString(size, this);
                    break;

                default:
                    throw new ArgumentException("type");
            }
            
            hpe.Mark = GCMark.White;
            totalAllocated += size;
            return AddHeapElement(hpe);
        }

        private int AllocateInternal(IEnumerable<StackValue> values, PrimitiveType type)
        {
            int size = values.Count();
            int index = AllocateInternal(size, type);
            var heapElement = heapElements[index];

            int i = 0;
            foreach (var item in values)
            {
                heapElement.SetItemAt(i, item);
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

        /// <summary>
        /// Mark all items in the array.
        /// </summary>
        /// <param name="hp"></param>
        /// <returns></returns>
        private int TraverseArray(DSArray array)
        {
            var dict = array.ToDictionary();
            int size = array.MemorySize;

            foreach (var pair in array.ToDictionary())
            {
                var key = pair.Key;
                if (key.IsReferenceType)
                    size += PropagateMark(key);

                var value = pair.Value;
                if (value.IsReferenceType)
                    size += PropagateMark(value);
            }

            return size;
        }

        private int TraverseObject(DSObject obj)
        {
            int size = obj.MemorySize;

            foreach (var item in obj.VisibleItems)
            {
                if (item.IsReferenceType)
                    size += PropagateMark(item);
            }

            return size;
        }

        /// <summary>
        /// Recursively mark all objects referenced by the object and change the
        /// color of this object to black.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private int PropagateMark(StackValue value)
        {
            Validity.Assert(value.IsReferenceType);

            int rawPtr = (int)value.RawIntValue;
            var hp = heapElements[rawPtr];
            if (hp.Mark == GCMark.Black)
                return 0;

            hp.Mark = GCMark.Black;

            int size = 0;
            if (value.IsArray)
            {
                size = TraverseArray(ToHeapObject<DSArray>(value));
            }
            else if (value.IsPointer)
            {
                size = TraverseObject(ToHeapObject<DSObject>(value));
            }

            return size;
        }

        /// <summary>
        /// Put all roots in gray list and be ready for gc.
        /// </summary>
        private void StartCollection()
        {
            sweepSet = new HashSet<int>(Enumerable.Range(0, heapElements.Count));
            sweepSet.ExceptWith(freeList);

            grayList = new LinkedList<StackValue>();
            foreach (var heapPointer in roots)
            {
                var ptr = (int)heapPointer.RawIntValue;
                heapElements[ptr].Mark = GCMark.Gray;
                grayList.AddLast(heapPointer);
            }

            totalTraversed = 0;
        }

        /// <summary>
        /// Move gc a step forward.
        /// </summary>
        private void SingleStep()
        { 
            switch (gcState)
            {
                case GCState.Pause:
                    gcState = GCState.WaitingForRoots;
                    break;

                case GCState.WaitingForRoots:
                    break;

                case GCState.Propagate:
                    if (grayList.Any())
                    {
                        totalTraversed += PropagateMark(grayList.First());
                        grayList.RemoveFirst();
                    }
                    else
                    {
                        gcState = GCState.Sweep;
                    }
                    break;

                case GCState.Sweep:
                    Sweep();
                    MarkAllWhite();
                    gcState = GCState.Pause;
                    IsGCRunning = false;
                    break;
            }
        }

        private void Sweep()
        {
            foreach (var ptr in sweepSet)
            {
                var hp = heapElements[ptr];
                if (hp.Mark != GCMark.White)
                    continue;

                if (hp is DSString)
                {
                    stringTable.TryRemoveString(ptr);
                }
                else if (hp is DSObject)
                {
                    var objPointer = StackValue.BuildPointer(ptr, hp.MetaData);
                    GCDisposeObject(objPointer, executive);
                }

                totalAllocated -= hp.MemorySize;
                heapElements[ptr] = null;
                freeList.Add(ptr);
            }
        }

        private void MarkAllWhite()
        {
            foreach (var hp in heapElements)
            {
                if (hp != null)
                    hp.Mark = GCMark.White;
            }
        }

        public void WriteBarrierForward(HeapElement hp, StackValue value)
        {
            if (hp.Mark == GCMark.Black && value.IsReferenceType)
            {
                HeapElement valueHp;
                if (TryGetHeapElement(value, out valueHp))
                {
                    totalTraversed += PropagateMark(value);
                }
            }
        }

        public bool IsWaitingForRoots()
        {
            return gcState == GCState.WaitingForRoots;
        }

        /// <summary>
        /// Notify the heap that gc roots are ready so that gc could move
        /// forward. The executive is passed for dispoing objects.
        /// </summary>
        /// <param name="gcroots"></param>
        /// <param name="exe"></param>
        /// <returns></returns>
        public bool SetRoots(IEnumerable<StackValue> gcroots, Executive exe)
        {
            if (gcroots == null)
                throw new ArgumentNullException("gcroots");

            if (exe == null)
                throw new ArgumentNullException("exe");

            if (!IsWaitingForRoots())
                return false;

            roots = new List<StackValue>(gcroots.Where(r => r.IsReferenceType));
            if (!roots.Any())
                return false;

            executive = exe;
            StartCollection();
            gcState = GCState.Propagate;

            return true;
        }

        /// <summary>
        /// GC
        /// </summary>
        public void GC()
        {
            SingleStep();
        }

        /// <summary>
        /// Do a full GC cycle
        /// </summary>
        /// <param name="gcroots"></param>
        /// <param name="exe"></param>
        public void FullGC(IEnumerable<StackValue> gcroots, Executive exe)
        {
            if (gcroots == null)
                throw new ArgumentNullException("gcroots");

            if (exe == null)
                throw new ArgumentNullException("exe");

            while (gcState != GCState.WaitingForRoots)
            {
                SingleStep();
            } 
            SetRoots(gcroots, exe);

            while (gcState != GCState.Pause)
            {
                SingleStep();
            }
        }

        public void ReportAllocation(int newSize)
        {
            totalAllocated += newSize;
        }

        public void GCMarkAndSweep(List<StackValue> rootPointers, Executive exe)
        {
            if (IsGCRunning)
                return;

            try
            {
                IsGCRunning = true;

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
                        subElements = heapElement.VisibleItems;
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
                IsGCRunning = false;
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
                foreach (StackValue sv in heapElement.VisibleItems)
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
