using System;
using System.Collections.Generic;
using System.Linq;
using ProtoCore.DSASM;
using ProtoCore.Exceptions;
using ProtoCore.Runtime;
using ProtoCore.Properties;

namespace ProtoCore.Utils
{
    public static class ArrayUtils
    {
        private static int RECURSION_LIMIT = 1024;

        /// <summary>
        /// If an empty array is passed, the result will be null
        /// if there are instances, but they share no common supertype the result will be var
        /// </summary>
        /// <param name="array"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static ClassNode GetGreatestCommonSubclassForArray(StackValue array, RuntimeCore runtimeCore)
        {
            if (!array.IsArray)
                throw new ArgumentException("The stack value provided was not an array");

            Dictionary<ClassNode, int> typeStats = GetTypeStatisticsForArray(array, runtimeCore);


            //@PERF: This could be improved with a 
            List<List<int>> chains = new List<List<int>>();
            HashSet<int> commonTypeIDs = new HashSet<int>();

            foreach (ClassNode cn in typeStats.Keys)
            {
//<<<<<<< .mine
                List<int> chain = ClassUtils.GetClassUpcastChain(cn, runtimeCore);

                //Now add in the other conversions - as we don't have a common superclass yet
                //@TODO(Jun): Remove this hack when we have a proper casting structure
                foreach (int id in cn.coerceTypes.Keys)
                    if (!chain.Contains(id))
                        chain.Add((id));

//=======
//                List<int> chain = GetConversionChain(cn, core);
//>>>>>>> .r2886
                chains.Add(chain);

                foreach (int nodeId in chain)
                    commonTypeIDs.Add(nodeId);

 

            }

            //Remove nulls if they exist
            {
 
            if (commonTypeIDs.Contains(
                (int)PrimitiveType.kTypeNull))
                commonTypeIDs.Remove((int)PrimitiveType.kTypeNull);

                List<List<int>> nonNullChains = new List<List<int>>();

                foreach (List<int> chain in chains)
                {
                    if (chain.Contains((int)PrimitiveType.kTypeNull))
                        chain.Remove((int)PrimitiveType.kTypeNull);

                    if (chain.Count > 0)
                        nonNullChains.Add(chain);
                }

                chains = nonNullChains;
                    
            }


            //Contract the hashset so that it contains only the nodes present in all chains
            //@PERF: this is very inefficent
            {
                foreach (List<int> chain in chains)
                {
                    commonTypeIDs.IntersectWith(chain);
                    

                }
            }

            //No common subtypes
            if (commonTypeIDs.Count == 0)
                return null;

            if (commonTypeIDs.Count == 1)
                return runtimeCore.DSExecutable.classTable.ClassNodes[commonTypeIDs.First()];


            List<int> lookupChain = chains[0];

            
            //Insertion sort the IDs, we may only have a partial ordering on them.
            List<int> orderedTypes = new List<int>();

            foreach (int typeToInsert in commonTypeIDs)
            {
                bool inserted = false;

                for (int i = 0; i < orderedTypes.Count; i++)
                {
                    int orderedType = orderedTypes[i];

                    if (lookupChain.IndexOf(typeToInsert) < lookupChain.IndexOf(orderedType))
                    {
                        inserted = true;
                        orderedTypes.Insert(i, typeToInsert);
                        break;
                    }
                }

                if (!inserted)
                    orderedTypes.Add(typeToInsert);
            }

            return runtimeCore.DSExecutable.classTable.ClassNodes[orderedTypes.First()];
        }

        /// <summary>
        /// For a class node using single inheritence, get the chain of inheritences
        /// </summary>
        /// <param name="cn"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static List<int> GetConversionChain(ClassNode cn, RuntimeCore runtimeCore)
        {
            List<int> ret = new List<int>();
            /*
            //@TODO: Replace this with an ID
            ret.Add(core.classTable.list.IndexOf(cn));

            ClassNode target = cn;
            while (target.baseList.Count > 0)
            {
                Validity.Assert(target.baseList.Count == 1, "Multiple Inheritence not yet supported, {F5DDC58D-F721-4319-854A-622175AC43F8}");
                ret.Add(cn.baseList[0]);

                target = core.classTable.list[cn.baseList[0]];
            }
            */

            List<int> coercableTypes = new List<int>();

            foreach (int typeID in cn.coerceTypes.Keys)
            {
                bool inserted = false;

                for (int i = 0; i < coercableTypes.Count; i++)
                {
                    if (cn.coerceTypes[typeID] < cn.coerceTypes[coercableTypes[i]])
                    {
                        inserted = true;
                        coercableTypes.Insert(typeID, i);
                        break;
                    }
                }
                if (!inserted)
                    coercableTypes.Add(typeID);
            }
            coercableTypes.Add(runtimeCore.DSExecutable.classTable.ClassNodes.IndexOf(cn));



            ret.AddRange(coercableTypes);
            return ret;

        }

        public static Dictionary<int, StackValue> GetTypeExamplesForLayer(StackValue array, RuntimeCore runtimeCore)
        {
            if (!array.IsArray)
            {
                Dictionary<int, StackValue> ret = new Dictionary<int, StackValue>();
                ret.Add(array.metaData.type, array);
                return ret;
            }

            Dictionary<int, StackValue> usageFreq = new Dictionary<int, StackValue>();

            //This is the element on the heap that manages the data structure
            HeapElement heapElement = GetHeapElement(array, runtimeCore);
            foreach (var sv in heapElement.VisibleItems)
            {
                if (!usageFreq.ContainsKey(sv.metaData.type))
                    usageFreq.Add(sv.metaData.type, sv);
            }

            return usageFreq;
        }



        /// <summary>
        /// Generate type statistics for given layer of an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static Dictionary<ClassNode, int> GetTypeStatisticsForLayer(StackValue array, RuntimeCore runtimeCore)
        {
            if (!array.IsArray)
            {
                Dictionary<ClassNode, int> ret = new Dictionary<ClassNode, int>();
                ret.Add(runtimeCore.DSExecutable.classTable.ClassNodes[array.metaData.type], 1);
                return ret;
            }

            Dictionary<ClassNode, int> usageFreq = new Dictionary<ClassNode,int>();

            //This is the element on the heap that manages the data structure
            HeapElement heapElement = GetHeapElement(array, runtimeCore);
            foreach (var sv in heapElement.VisibleItems)
            {
                ClassNode cn = runtimeCore.DSExecutable.classTable.ClassNodes[sv.metaData.type];
                if (!usageFreq.ContainsKey(cn))
                    usageFreq.Add(cn, 0);

                usageFreq[cn] = usageFreq[cn] + 1;
            }

            return usageFreq;
        }

        /// <summary>
        /// Generate type statistics for the whole array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static Dictionary<ClassNode, int> GetTypeStatisticsForArray(StackValue array, RuntimeCore runtimeCore)
        {
            if (!array.IsArray)
            {
                Dictionary<ClassNode, int> ret = new Dictionary<ClassNode, int>();
                ret.Add(runtimeCore.DSExecutable.classTable.ClassNodes[array.metaData.type], 1);
                return ret;
            }

            Dictionary<ClassNode, int> usageFreq = new Dictionary<ClassNode, int>();

            //This is the element on the heap that manages the data structure
            HeapElement heapElement = GetHeapElement(array, runtimeCore);
            foreach (var sv in heapElement.VisibleItems)
            {
                if (sv.IsArray)
                {
                    //Recurse
                    Dictionary<ClassNode, int> subLayer = GetTypeStatisticsForArray(sv, runtimeCore);
                    foreach (ClassNode cn in subLayer.Keys)
                    {
                        if (!usageFreq.ContainsKey(cn))
                            usageFreq.Add(cn, 0);

                        usageFreq[cn] = usageFreq[cn] + subLayer[cn];

                    }
                }
                else
                {

                    ClassNode cn = runtimeCore.DSExecutable.classTable.ClassNodes[sv.metaData.type];
                    if (!usageFreq.ContainsKey(cn))
                        usageFreq.Add(cn, 0);

                    usageFreq[cn] = usageFreq[cn] + 1;
                }
            }

            return usageFreq;
        }

        private static int GetMaxRankForArray(StackValue array, RuntimeCore runtimeCore, int tracer)
        {
            if (tracer > RECURSION_LIMIT)
                throw new CompilerInternalException("Internal Recursion limit exceeded in Rank Check - Possible heap corruption {3317D4F6-4758-4C19-9680-75B68DA0436D}");

            if (!array.IsArray)
                return 0;
            //throw new ArgumentException("The stack value provided was not an array");

            int ret = 1;

            int largestSub = 0;

            //This is the element on the heap that manages the data structure
            foreach (var sv in runtimeCore.Heap.ToHeapObject<DSArray>(array).Values)
            {
                if (sv.IsArray)
                {
                    int subArrayRank = GetMaxRankForArray(sv, runtimeCore, tracer + 1);
                    largestSub = Math.Max(subArrayRank, largestSub);
                }
            }

            return largestSub + ret;
        }

        public static int GetMaxRankForArray(StackValue array, RuntimeCore runtimeCore)
        {
            return GetMaxRankForArray(array, runtimeCore, 0);

        }

        /// <summary>
        /// Whether sv is double or arrays contains double value.
        /// </summary>
        /// <param name="sv"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static bool ContainsDoubleElement(StackValue sv, RuntimeCore runtimeCore)
        {
            Executable exe = runtimeCore.DSExecutable;
            if (!sv.IsArray)
                return exe.TypeSystem.GetType(sv) == (int)PrimitiveType.kTypeDouble;

            DSArray array = runtimeCore.Heap.ToHeapObject<DSArray>(sv);
            return array.Values.Any(
                        v => (v.IsArray && ContainsDoubleElement(v, runtimeCore)) ||
                             (exe.TypeSystem.GetType(v) == (int)PrimitiveType.kTypeDouble));
        }

        /// <summary>
        /// If the passed in value is not an array or an empty array or an array which contains only empty arrays, return false.
        /// Otherwise, return true;
        /// </summary>
        /// <param name="sv"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static bool ContainsNonArrayElement(StackValue sv, RuntimeCore runtimeCore)
        {
            if (!sv.IsArray)
                return true;

            var array = runtimeCore.Heap.ToHeapObject<DSArray>(sv);
            return array.Values.Any(v => ContainsNonArrayElement(v, runtimeCore)); 
        }

        /// <summary>
        /// Pull the heap element out of a heap object
        /// </summary>
        /// <param name="heapObject"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static HeapElement GetHeapElement(StackValue heapObject, RuntimeCore runtimeCore)
        {
            if (!heapObject.IsArray && !heapObject.IsPointer)
            {
                return null;
            }

            return runtimeCore.RuntimeMemory.Heap.GetHeapElement(heapObject);
        }

        public static bool IsUniform(StackValue sv, RuntimeCore runtimeCore)
        {
            if (!sv.IsArray)
                return false;

            if (Utils.ArrayUtils.GetTypeStatisticsForArray(sv, runtimeCore).Count != 1)
                return false;

            return true;
        }
    
        /// <summary>
        /// Retrieve the first non-array element in an array 
        /// </summary>
        /// <param name="svArray"></param>
        /// <param name="sv"></param>
        /// <param name="core"></param>
        /// <returns> true if the element was found </returns>
        public static bool GetFirstNonArrayStackValue(StackValue svArray, ref StackValue sv, RuntimeCore runtimeCore)
        {
            RuntimeMemory rmem = runtimeCore.RuntimeMemory;
            if (!svArray.IsArray)
            {
                return false;
            }

            HeapElement he = rmem.Heap.GetHeapElement(svArray);
            if (!he.VisibleItems.Any())
            {
                return false;
            }

            while (he.GetItemAt(0).IsArray)
            {
                he = rmem.Heap.GetHeapElement(he.GetItemAt(0));

                // Handle the case where the array is valid but empty
                if (!he.VisibleItems.Any())
                {
                    return false;
                }
            }

            sv = he.GetItemAt(0).ShallowClone();
            return true;
        }
   }
}
