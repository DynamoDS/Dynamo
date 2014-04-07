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
    }
}
