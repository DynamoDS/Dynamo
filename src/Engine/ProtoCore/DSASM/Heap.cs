using System;
using System.Collections.Generic;
using System.Linq;
using ProtoCore.Exceptions;
using ProtoCore.Utils;

namespace ProtoCore.DSASM
{
    public class HeapElement
    {
        private const int INITIAL_SIZE = 5;
        private const double REALLOC_FACTOR = 0.5;

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
        public Heap.GCMark Mark { get; set; }

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
            int newSize = INITIAL_SIZE;
            if (size > INITIAL_SIZE)
            {
                // Determine the next allocation size
                newSize = (int)(allocated * REALLOC_FACTOR) + allocated;

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

            // We should move StackValue list to heap. That is, heap
            // manages StackValues instead of HeapElement itself.
            heap.ReportAllocation(size - allocated);

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
                        try
                        {
                            ReAllocate(size);
                        }
                        catch (OutOfMemoryException)
                        {
                            throw new RunOutOfMemoryException();
                        }
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
                try
                {
                    ReAllocate(index + 1);
                }
                catch (OutOfMemoryException)
                {
                    throw new RunOutOfMemoryException();
                }
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
            heap.WriteBarrierForward(this, value);
            values[index] = value;
        }

        public virtual int MemorySize
        {
            get { return allocated; }
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
                    hash = (hash * 397) ^ value.RawData.GetHashCode();
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
        /// Returns string from the string table.
        /// </summary>
        /// <param name="pointer">The index of HeapElement that represents the string</param>
        /// <param name="s"></param>
        /// <returns></returns>
        internal bool TryGetString(int pointer, out string s)
        {
            return pointerToStringTable.TryGetValue(pointer, out s);
        }

        /// <summary>
        /// Returns the index of HeapElement that represents the string
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
        private enum GCState
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

        private readonly List<int> freeList = new List<int>();
        private readonly List<HeapElement> heapElements = new List<HeapElement>();
        private HashSet<int> fixedHeapElements = new HashSet<int>(); 
        private readonly StringTable stringTable = new StringTable();

        private const int GC_THRESHOLD = 1024*1024;
        // Totaly allocated StackValues
        private int totalAllocated = 0;
        private int totalTraversed = 0;
        private int gcDebt = GC_THRESHOLD;

        private LinkedList<StackValue> grayList;
        private HashSet<int> sweepSet;
        private List<StackValue> roots;
        private Executive executive;
        private bool isDisposing = false;

        public bool IsGCRunning { get; private set; }
        private GCState gcState = GCState.Pause;

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
        ///
        /// Exceptions: ProtoCore.Exceptions.RunOutOfMemoryException
        /// </summary>
        /// <param name="values">Array elements whose indices are integer</param>
        /// <param name="dict">Array elements whose indices are not integer</param>
        /// <returns></returns>
        public StackValue AllocateArray(StackValue[] values)
        {
            try
            {
                int index = AllocateInternal(values, PrimitiveType.Array);
                var heapElement = heapElements[index];
                heapElement.MetaData = new MetaData { type = (int)PrimitiveType.Array };
                return StackValue.BuildArrayPointer(index);
            }
            catch (OutOfMemoryException)
            {
                throw new RunOutOfMemoryException();
            }
        }

        /// <summary>
        /// Allocate an object pointer.
        /// </summary>
        /// <param name="values">Values of object properties</param>
        /// <param name="metaData">Object type</param>
        /// <returns></returns>
        private StackValue AllocatePointer(StackValue[] values, MetaData metaData)
        {
            int index = AllocateInternal(values, PrimitiveType.Pointer);
            var heapElement = heapElements[index];
            heapElement.MetaData = metaData;
            return StackValue.BuildPointer(index, metaData);
        }

        /// <summary>
        /// Allocate an object pointer.
        ///
        /// Exceptions: ProtoCore.Exceptions.RunOutOfMemoryException
        /// </summary>
        /// <param name="values">Values of object properties</param>
        /// <returns></returns>
        public StackValue AllocatePointer(StackValue[] values)
        {
            try
            {
                return AllocatePointer(values, new MetaData { type = (int)PrimitiveType.Pointer });
            }
            catch (OutOfMemoryException)
            {
                throw new RunOutOfMemoryException();
            }
        }

        /// <summary>
        /// Allocate an object pointer.
        ///
        /// Exceptions: ProtoCore.Exceptions.RunOutOfMemoryException
        /// </summary>
        /// <param name="size">The size of object properties.</param>
        /// <param name="metadata">Object type</param>
        /// <returns></returns>
        public StackValue AllocatePointer(int size, MetaData metadata)
        {
            try
            {
                int index = AllocateInternal(size, PrimitiveType.Pointer);
                var hpe = heapElements[index];
                hpe.MetaData = metadata;
                return StackValue.BuildPointer(index, metadata);
            }
            catch (OutOfMemoryException)
            {
                throw new RunOutOfMemoryException();
            }
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
                index = AllocateInternal(new StackValue[] {}, PrimitiveType.String);
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
            try
            {
                return AllocateStringInternal(str, true);
            }
            catch (OutOfMemoryException)
            {
                throw new RunOutOfMemoryException();
            }
        }

        /// <summary>
        /// Allocate string. 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public StackValue AllocateString(string str)
        {
            try
            {
                return AllocateStringInternal(str, false);
            }
            catch (OutOfMemoryException)
            {
                throw new RunOutOfMemoryException();
            }
        }

        /// <summary>
        /// Returns string that pointer represents.
        /// </summary>
        /// <param name="pointer"></param>
        /// <returns></returns>
        public string GetString(DSString dsString)
        {
            int index = dsString.Pointer.StringPointer;
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
            int index = Constants.kInvalidIndex;
            if (pointer.IsPointer)
            {
                index = pointer.Pointer;
            }
            else if (pointer.IsArray)
            {
                index = pointer.ArrayPointer;
            }
            else if (pointer.IsString)
            {
                index = pointer.StringPointer;
            }


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

        private int AddHeapElement(HeapElement hpe)
        {
            hpe.Mark = GCMark.White;
            ReportAllocation(hpe.MemorySize);

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

        private int AllocateInternal(int size, PrimitiveType type)
        {
            HeapElement hpe = null;

            switch (type)
            {
                case PrimitiveType.Array:
                    hpe = new DSArray(size, this);
                    break;

                case PrimitiveType.Pointer:
                    hpe = new DSObject(size, this);
                    break;

                case PrimitiveType.String:
                    hpe = new DSString(size, this);
                    break;

                default:
                    throw new ArgumentException("type");
            }

            return AddHeapElement(hpe);
        }

        private int AllocateInternal(StackValue[] values, PrimitiveType type)
        {
            HeapElement hpe = null;

            switch (type)
            {
                case PrimitiveType.Array:
                    hpe = new DSArray(values, this);
                    break;

                case PrimitiveType.Pointer:
                    hpe = new DSObject(values, this);
                    break;

                case PrimitiveType.String:
                    hpe = new DSString(values, this);
                    break;

                default:
                    throw new ArgumentException("type");
            }

            return AddHeapElement(hpe);
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
                if (cn.Base != Constants.kInvalidIndex)
                {
                    classIndex = cn.Base;
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
                exe.rmem.Push(StackValue.BuildPointer(svPtr.Pointer, svPtr.metaData));
                exe.rmem.Push(StackValue.BuildInt(1));
                
                ++exe.RuntimeCore.FunctionCallDepth;

                // TODO: Need to move IsExplicitCall to DebugProps and come up with a more elegant solution for this
                // fix for IDE-963 - pratapa
                bool explicitCall = exe.IsExplicitCall;
                bool tempFlag = explicitCall;
                exe.Callr(pn.RuntimeIndex, pn.ID, classIndex, ref explicitCall);

                exe.IsExplicitCall = tempFlag;

                --exe.RuntimeCore.FunctionCallDepth;
            }
        }

        /// <summary>
        /// Recursively mark all objects referenced by the object and change the
        /// color of this object to black.
        /// </summary>
        /// <param name="value">StackValue</param>
        /// <returns></returns>
        private int RecursiveMark(StackValue root)
        {
            Queue<StackValue> ptrs = new Queue<StackValue>();
            ptrs.Enqueue(root);
            int releaseSize = 0;

            while (ptrs.Any())
            {
                StackValue value = ptrs.Dequeue();
                int rawPtr = (int)value.RawData;
                var hp = heapElements[rawPtr];
                if (hp.Mark == GCMark.Black)
                    continue;

                hp.Mark = GCMark.Black;
                if (value.IsArray)
                {
                    var array = ToHeapObject<DSArray>(value);
                    releaseSize += array.MemorySize;
                    array.CollectElementsForGC(ptrs);
                }
                else if (value.IsPointer)
                {
                    var obj = ToHeapObject<DSObject>(value);
                    releaseSize += obj.MemorySize;

                    foreach (var item in obj.Values)
                    {
                        if (item.IsReferenceType)
                            ptrs.Enqueue(item);
                    }
                }
            }

            return releaseSize;
        }

        /// <summary>
        /// Put all roots in gray list and be ready for gc.
        /// </summary>
        private void StartCollection()
        {
            sweepSet = new HashSet<int>(Enumerable.Range(0, heapElements.Count));
            sweepSet.ExceptWith(freeList);
            sweepSet.ExceptWith(fixedHeapElements);

            grayList = new LinkedList<StackValue>();
            foreach (var heapPointer in roots)
            {
                var ptr = (int)heapPointer.RawData;
                heapElements[ptr].Mark = GCMark.Gray;
                grayList.AddLast(heapPointer);
            }

            totalTraversed = 0;
        }

        /// <summary>
        /// Move gc a step forward.
        /// </summary>
        private void SingleStep(bool forceGC)
        {
            switch (gcState)
            {
                case GCState.Pause:
                    if (gcDebt <= 0 || forceGC)
                        gcState = GCState.WaitingForRoots;
                    break;

                case GCState.WaitingForRoots:
                    break;

                case GCState.Propagate:
                    if (grayList.Any())
                    {
                        totalTraversed += RecursiveMark(grayList.First());
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

        /// <summary>
        /// Sweep all heap elements that are marked as white.
        /// </summary>
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
                    isDisposing = true;
                    GCDisposeObject(objPointer, executive);
                    isDisposing = false;
                }

                totalAllocated -= hp.MemorySize;
                heapElements[ptr] = null;
                freeList.Add(ptr);
            }

            gcDebt = totalAllocated > GC_THRESHOLD ? totalAllocated : GC_THRESHOLD;
        }

        /// <summary>
        /// Mark all heap elements as white.
        /// </summary>
        private void MarkAllWhite()
        {
            foreach (var hp in heapElements)
            {
                if (hp != null)
                    hp.Mark = GCMark.White;
            }
        }

        /// <summary>
        /// If the heap object is modified, mark the new value that it references to.
        /// </summary>
        /// <param name="hp">Heap object that is to be modified</param>
        /// <param name="value">The value that will be put in the heap object</param>
        public void WriteBarrierForward(HeapElement hp, StackValue value)
        {
            if (hp.Mark == GCMark.Black && value.IsReferenceType)
            {
                HeapElement valueHp;
                if (TryGetHeapElement(value, out valueHp))
                {
                    totalTraversed += RecursiveMark(value);
                }
            }
        }

        /// <summary>
        /// Returns true if the heap is waiting for GC root objects.
        /// </summary>
        public bool IsWaitingForRoots
        {
            get
            { 
                return gcState == GCState.WaitingForRoots;
            }
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

            if (!IsWaitingForRoots)
                return false;

            var validPointers = gcroots.Where(r => r.IsReferenceType && 
                                                   r.RawData < heapElements.Count() && 
                                                   r.RawData >= 0 && 
                                                   heapElements[(int)r.RawData] != null);
            roots = new List<StackValue>(validPointers);
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
            // GC disabled when the object is being disposed.
            if (isDisposing)
                return;

            SingleStep(false);
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
                SingleStep(true);

            SetRoots(gcroots, exe);
            while (gcState != GCState.Pause)
                SingleStep(true);
        }

        public void ReportAllocation(int newSize)
        {
            gcDebt -= newSize;
            totalAllocated += newSize;
        }
    }
}
