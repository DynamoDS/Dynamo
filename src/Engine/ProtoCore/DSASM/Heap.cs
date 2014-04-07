using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                Stack[n].optype = AddressType.Invalid;
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
        public void ReAllocate(int size)
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

        public void RightShiftElements(int size)
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
    
        public HeapElement Clone()
        {
            HeapElement second = new HeapElement(AllocSize, Symbol);
            second.Active = Active;
            second.Symbol = Symbol;
            second.AllocSize = AllocSize;
            second.VisibleSize = VisibleSize;
            second.Refcount = Refcount;

            second.Stack = new StackValue[Stack.Length];
            for (int i = 0; i < Stack.Length; i++)
                second.Stack[i] = Stack[i].ShallowClone();

            return second;
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
            if (AddressType.String == value.optype)
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
        public int TotalSize { get; set; }
        public List<HeapElement> Heaplist { get; set; }
        public Object cslock { get; private set; }

        private List<int> freeList;

        public Heap()
        {
            TotalSize = 0;
            Heaplist = new List<HeapElement>();
            cslock = new Object();
            freeList = new List<int>();
        }

        public int Allocate(StackValue[] elements)
        {
            TotalSize += elements.Length;
            HeapElement hpe = new HeapElement(elements);
            return AddHeapElement(hpe);
        }

        public int Allocate(int size, int symbol = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            TotalSize += size;
            HeapElement hpe = new HeapElement(size, symbol);
            return AddHeapElement(hpe);
        }

        private int AddHeapElement(HeapElement hpe)
        {
            int index = FindFree();
            if (ProtoCore.DSASM.Constants.kInvalidIndex == index)
            {
                Heaplist.Add(hpe);
                index = Heaplist.Count - 1;
            }
            else
            {
                Heaplist[index].Active = true;
                Heaplist[index] = hpe;
            }
            return index;
        }

        public Heap Clone()
        {
            Heap ret = new Heap();
            ret.TotalSize = TotalSize;
            ret.freeList = freeList;
            foreach (HeapElement he in Heaplist)
                ret.Heaplist.Add(he.Clone());

            return ret;
        }

        public void Append(int lastPtr, Heap rhs)
        {
            TotalSize += rhs.TotalSize;
        
            rhs.ReallocPointers(lastPtr);

            Heaplist.InsertRange(lastPtr, rhs.Heaplist);
        }

        private void ReallocPointers(int offset)
        {
            for (int i = 0; i < Heaplist.Count; ++i)
            {
                for (int j = 0; j < Heaplist[i].GetAllocatedSize(); ++j)
                {
                    if (ProtoCore.DSASM.AddressType.Pointer == Heaplist[i].Stack[j].optype)
                    {
                        Heaplist[i].Stack[j].opdata += offset;
                    }
                }
            }
        }

        private int FindFree()
        {
            int freeItemCount = freeList.Count;
            if (freeItemCount > 0)
            {
                int index = freeList[freeItemCount - 1];
                freeList.RemoveAt(freeItemCount - 1);
                return index;
            }
            return ProtoCore.DSASM.Constants.kInvalidIndex;

        }

        public void Free()
        {
            TotalSize = 0;
            Heaplist.Clear();
            freeList = new List<int>();
        }

        private void GCDisposeObject(ref StackValue svPtr, Executive exe)
        {
            int classIndex = (int)svPtr.metaData.type;
            ClassNode cn = exe.exe.classTable.ClassNodes[classIndex];
            ProcedureNode pn = null;

            while (pn == null)
            {
                pn = cn.GetDisposeMethod();
                if (pn == null && cn.baseList != null && cn.baseList.Count != 0) // search the base class
                {
                    // assume multiple inheritance is not allowed
                    // it will only has a single base class 
                    classIndex = cn.baseList[0];
                    cn = exe.exe.classTable.ClassNodes[cn.baseList[0]];
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
                exe.rmem.Push(StackValue.BuildStaticType((int)ProtoCore.PrimitiveType.kTypeVar));
                
                ++exe.Core.FunctionCallDepth;

                // TODO: Need to move isExplicitCall to DebugProps and come up with a more elegant solution for this
                // fix for IDE-963 - pratapa
                bool explicitCall = exe.isExplicitCall;
                bool tempFlag = explicitCall;
                exe.Callr(pn.procId, classIndex, 1, ref explicitCall);

                exe.isExplicitCall = tempFlag;

                --exe.Core.FunctionCallDepth;
            }
        }

        public void Sweep(int first, int last)
        {
            for (int i = 0; i < Heaplist.Count; ++i)  
            {
                for (int symbol = first; symbol < last; ++symbol) 
                {
                    // Any stack allocated symbols are left out since they are not in the heaplist
                    if (symbol == Heaplist[i].Symbol)
                    {
                        Heaplist[i].Active = false;
                    }
                }
            }
        }


        public void GCMarkSweep()
        {
            throw new NotImplementedException("{3CDF5599-97DB-4EC2-9E25-EC11DBA7280E}");
        }

        public bool IsTemporaryPointer(StackValue sv)
        {
            if (!StackUtils.IsReferenceType(sv))
            {
                return false;
            }

            int ptr = (int)sv.opdata;
            HeapElement he = this.Heaplist[ptr];
            return he.Active && he.Refcount == 0; 
        }

        public void IncRefCount(StackValue sv)
        {
            if (!StackUtils.IsReferenceType(sv))
            {
                return;
            }

            int ptr = (int)sv.opdata;

            this.Heaplist[ptr].Refcount++;
            if (this.Heaplist[ptr].Refcount > 0)
            {
                this.Heaplist[ptr].Active = true;
            }
        }

        public void DecRefCount(StackValue sv)
        {
            if (!StackUtils.IsReferenceType(sv))
            {
                return;
            }

            int ptr = (int)sv.opdata;
            if (this.Heaplist[ptr].Refcount > 0)
            {
                this.Heaplist[ptr].Refcount--;
            }
            else
            {
            }
        }

        public void GCRelease(StackValue[] ptrList, Executive exe)
        {
            for (int n = 0; n < ptrList.Length; ++n)
            {
                StackValue svPtr = ptrList[n];
                if (svPtr.optype != AddressType.Pointer && svPtr.optype != AddressType.ArrayPointer)
                {
                    continue;
                }

                int ptr = (int)svPtr.opdata;
                if (ptr < 0 || ptr >= Heaplist.Count)
                {
                    continue;
                }
                HeapElement hs = Heaplist[ptr];

                if (!hs.Active)
                {
                    continue;
                }

                if (hs.Refcount > 0)
                {
                    hs.Refcount--;
                }

                // TODO Jun: If its a pointer to a primitive then dont decrease its refcount, just free it
                if (hs.Refcount == 0)
                {
                    // if it is of class type, first call its destructor before clean its members
                    if(svPtr.optype == AddressType.Pointer)
                        GCDisposeObject(ref svPtr, exe);

                    if (svPtr.optype == AddressType.ArrayPointer && hs.Dict != null)
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
                    freeList.Add(ptr);
                }
            }
        }
    }
}
