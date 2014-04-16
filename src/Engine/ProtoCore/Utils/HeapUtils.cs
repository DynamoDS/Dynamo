using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore.DSASM;

namespace ProtoCore.Utils
{
    public static class HeapUtils
    {

        /// <summary>
        /// Take an array of already allocated StackValues and push them into 
        /// the heap and returning the stack value that represents the array
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static StackValue StoreArray(StackValue[] elements, Dictionary<StackValue, StackValue> dict, Core core)
        {
            Heap heap = core.Heap;

            lock (heap.cslock)
            {
                int ptr = heap.Allocate(elements);
                StackValue overallSv = StackValue.BuildArrayPointer(ptr);
                heap.Heaplist[ptr].Dict = dict;
                return overallSv;
            }
        }

        /// <summary>
        /// Checks if the heap contains at least 1 pointer element that points to itself
        /// </summary>
        /// <param name="core"></param>
        /// <returns> Returns true if the heap contains no cycles</returns>
        public static bool VerifyHeapNonCyclic(Core core)
        {
            for (int n = 0; n < core.Heap.Heaplist.Count; ++n)
            {
                HeapElement heapElem = core.Heap.Heaplist[n];
                if (IsHeapCyclic(heapElem, core, n))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Checks if the heap element is cyclic. 
        /// Traverses the pointer element and determines it points to itself
        /// </summary>
        /// <param name="heapElement"></param>
        /// <param name="core"></param>
        /// <returns> Returns true if the array contains a cycle </returns>
        private static bool IsHeapCyclic(HeapElement heapElement, Core core, int HeapID)
        {
            if (heapElement.Active && heapElement.VisibleSize > 0)
            {
                // Traverse each element in the heap
                foreach (StackValue sv in heapElement.Stack)
                {
                    // Is it a pointer
                    if (sv.IsReferenceType())
                    {
                        // Check if the current element in the heap points to the original pointer
                        if (sv.opdata == HeapID)
                        {
                            return true;
                        }
                        return IsHeapCyclic(core.Heap.Heaplist[(int)sv.opdata], core, HeapID);
                    }
                }
            }
            return false;
        }
    }
}
