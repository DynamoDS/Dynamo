using System;
using System.Collections.Generic;
using System.Linq;
using ProtoCore.DSASM;
using ProtoCore.Exceptions;
using ProtoCore.RuntimeData;

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
        public static ClassNode GetGreatestCommonSubclassForArray(StackValue array, Core core)
        {
            if (!array.IsArray)
                throw new ArgumentException("The stack value provided was not an array");

            Dictionary<ClassNode, int> typeStats = GetTypeStatisticsForArray(array, core);


            //@PERF: This could be improved with a 
            List<List<int>> chains = new List<List<int>>();
            HashSet<int> commonTypeIDs = new HashSet<int>();

            foreach (ClassNode cn in typeStats.Keys)
            {
//<<<<<<< .mine
                List<int> chain = ClassUtils.GetClassUpcastChain(cn, core);

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
                return core.ClassTable.ClassNodes[commonTypeIDs.First()];


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

            return core.ClassTable.ClassNodes[orderedTypes.First()];
        }

        /// <summary>
        /// For a class node using single inheritence, get the chain of inheritences
        /// </summary>
        /// <param name="cn"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static List<int> GetConversionChain(ClassNode cn, Core core)
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
            coercableTypes.Add(core.ClassTable.ClassNodes.IndexOf(cn));



            ret.AddRange(coercableTypes);
            return ret;

        }

        public static Dictionary<int, StackValue> GetTypeExamplesForLayer(StackValue array, Core core)
        {
            if (!array.IsArray)
            {
                Dictionary<int, StackValue> ret = new Dictionary<int, StackValue>();
                ret.Add(array.metaData.type, array);
                return ret;
            }

            Dictionary<int, StackValue> usageFreq = new Dictionary<int, StackValue>();

            //This is the element on the heap that manages the data structure
            HeapElement heapElement = GetHeapElement(array, core);
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
        public static Dictionary<ClassNode, int> GetTypeStatisticsForLayer(StackValue array, Core core)
        {
            if (!array.IsArray)
            {
                Dictionary<ClassNode, int> ret = new Dictionary<ClassNode, int>();
                ret.Add(core.ClassTable.ClassNodes[array.metaData.type], 1);
                return ret;
            }

            Dictionary<ClassNode, int> usageFreq = new Dictionary<ClassNode,int>();

            //This is the element on the heap that manages the data structure
            HeapElement heapElement = GetHeapElement(array, core);
            foreach (var sv in heapElement.VisibleItems)
            {
                ClassNode cn = core.ClassTable.ClassNodes[sv.metaData.type];
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
        public static Dictionary<ClassNode, int> GetTypeStatisticsForArray(StackValue array, Core core)
        {
            if (!array.IsArray)
            {
                Dictionary<ClassNode, int> ret = new Dictionary<ClassNode, int>();
                ret.Add(core.ClassTable.ClassNodes[array.metaData.type], 1);
                return ret;
            }

            Dictionary<ClassNode, int> usageFreq = new Dictionary<ClassNode, int>();

            //This is the element on the heap that manages the data structure
            HeapElement heapElement = GetHeapElement(array, core);
            foreach (var sv in heapElement.VisibleItems)
            {
                if (sv.IsArray)
                {
                    //Recurse
                    Dictionary<ClassNode, int> subLayer = GetTypeStatisticsForArray(sv, core);
                    foreach (ClassNode cn in subLayer.Keys)
                    {
                        if (!usageFreq.ContainsKey(cn))
                            usageFreq.Add(cn, 0);

                        usageFreq[cn] = usageFreq[cn] + subLayer[cn];

                    }
                }
                else
                {

                    ClassNode cn = core.ClassTable.ClassNodes[sv.metaData.type];
                    if (!usageFreq.ContainsKey(cn))
                        usageFreq.Add(cn, 0);

                    usageFreq[cn] = usageFreq[cn] + 1;
                }
            }

            return usageFreq;
        }

        private static int GetMaxRankForArray(StackValue array, Core core, int tracer)
        {
            if (tracer > RECURSION_LIMIT)
                throw new CompilerInternalException("Internal Recursion limit exceeded in Rank Check - Possible heap corruption {3317D4F6-4758-4C19-9680-75B68DA0436D}");

            if (!array.IsArray)
                return 0;
            //throw new ArgumentException("The stack value provided was not an array");

            int ret = 1;

            int largestSub = 0;

            //This is the element on the heap that manages the data structure
            HeapElement heapElement = GetHeapElement(array, core);
            foreach (var sv in heapElement.VisibleItems)
            {
                if (sv.IsArray)
                {
                    int subArrayRank = GetMaxRankForArray(sv, core, tracer + 1);

                    largestSub = Math.Max(subArrayRank, largestSub);
                }
            }

            var dict = heapElement.Dict;
            if (dict != null)
            {
                foreach (var sv in dict.Values)
                {
                    if (sv.IsArray)
                    {
                        int subArrayRank = GetMaxRankForArray(sv, core, tracer + 1);
                        largestSub = Math.Max(subArrayRank, largestSub);
                    }
                }
            }

            return largestSub + ret;
        }

        public static int GetMaxRankForArray(StackValue array, Core core)
        {
            return GetMaxRankForArray(array, core, 0);

        }

        /// <summary>
        /// Whether sv is double or arrays contains double value.
        /// </summary>
        /// <param name="sv"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static bool ContainsDoubleElement(StackValue sv, Core core)
        {
            if (!sv.IsArray)
                return core.TypeSystem.GetType(sv) == (int)PrimitiveType.kTypeDouble;

            return ArrayUtils.GetValues(sv, core).Any(
                        v => (v.IsArray && ContainsDoubleElement(v, core)) ||
                             (core.TypeSystem.GetType(v) == (int)PrimitiveType.kTypeDouble));
        }

        /// <summary>
        /// If the passed in value is not an array or an empty array or an array which contains only empty arrays, return false.
        /// Otherwise, return true;
        /// </summary>
        /// <param name="sv"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static bool ContainsNonArrayElement(StackValue sv, Core core)
        {
            if (!sv.IsArray)
                return true;

            var values = ArrayUtils.GetValues(sv, core);
            return values.Any(v => ContainsNonArrayElement(v, core)); 
        }

        /// <summary>
        /// Pull the heap element out of a heap object
        /// </summary>
        /// <param name="heapObject"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static HeapElement GetHeapElement(StackValue heapObject, Core core)
        {
            if (!heapObject.IsArray && !heapObject.IsString && !heapObject.IsPointer)
            {
                return null;
            }

            return core.Heap.GetHeapElement(heapObject);
        }

        public static bool IsUniform(StackValue sv, Core core)
        {
            if (!sv.IsArray)
                return false;

            if (Utils.ArrayUtils.GetTypeStatisticsForArray(sv, core).Count != 1)
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
        public static bool GetFirstNonArrayStackValue(StackValue svArray, ref StackValue sv, Core core)
        {
            if (!svArray.IsArray)
            {
                return false;
            }

            HeapElement he = core.Heap.GetHeapElement(svArray);
            if (null == he.Stack || he.Stack.Length == 0)
            {
                return false;
            }

            while (he.Stack[0].IsArray)
            {
                he = core.Heap.GetHeapElement(he.Stack[0]);

                // Handle the case where the array is valid but empty
                if (he.Stack.Length == 0)
                {
                    return false;
                }
            }

            sv = he.Stack[0].ShallowClone();
            return true;
        }

        /// <summary>
        /// Return the element size of an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static int GetElementSize(StackValue array, Core core)
        {
            Validity.Assert(array.IsArray || array.IsString);
            if (!array.IsArray && !array.IsString)
            {
                return Constants.kInvalidIndex;
            }

            return GetHeapElement(array, core).VisibleSize;
        }

        /// <summary>
        /// For an array we supporting zipped replicaiton for array indexing as 
        /// well. I.e., for the following expression:
        /// 
        ///     a[1..3][2..4] = x;
        /// 
        /// It will be expanded to 
        /// 
        ///     a[1][2] = x;
        ///     a[2][3] = x;
        ///     a[3][4] = x;
        ///     
        /// So here we need to calculate zipped indices. The length of returned 
        /// indices is decided by the shortest length of index that used in 
        /// array indexing. E.g.,
        /// 
        /// For array indexing
        /// 
        ///     [{1, 2, 3}][{"x", "y"}][{6, 7, 8}], i.e., 
        ///     
        ///     1 -> "x" -> 6
        ///     2 -> "y" -> 7
        ///     3 ->     -> 8
        /// 
        /// The shortest length of index is 2 ({"x", "y"}), so function will 
        /// returns:
        /// 
        ///     {{1, "x", 6}, {2, "y", 7}}
        ///     
        /// </summary>
        /// <param name="indices"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        private static StackValue[][] GetZippedIndices(List<StackValue> indices, Core core)
        {
            List<StackValue[]> allFlattenValues = new List<StackValue[]>();

            int zipLength = System.Int32.MaxValue;
            foreach (var index in indices)
            {
                int length = 1;
                if (index.IsArray)
                {
                    StackValue[] flattenValues = GetFlattenValue(index, core);
                    allFlattenValues.Add(flattenValues);
                    length = flattenValues.Count();
                }
                else
                {
                    allFlattenValues.Add(null);
                }

                if (zipLength > length)
                {
                    zipLength = length;
                }
            }

            if (zipLength == 0)
            {
                return null;
            }
            else
            {
                int dims = indices.Count;
                StackValue[][] zippedIndices = new StackValue[zipLength][];
                for (int i = 0; i < zipLength; ++i)
                {
                    zippedIndices[i] = new StackValue[dims];
                }

                for (int i = 0; i < dims; ++i)
                {
                    StackValue index = indices[i];
                    StackValue[] values = null;
                    if (index.IsArray)
                    {
                        values = allFlattenValues[i];
                    }

                    if (1 == zipLength)
                    {
                        if (index.IsArray)
                        {
                            zippedIndices[0][i] = values[0];
                        }
                        else
                        {
                            zippedIndices[0][i] = index;
                        }
                    }
                    else
                    {
                        for (int j = 0; j < zipLength; ++j)
                        {
                            zippedIndices[j][i] = values[j];
                        }
                    }
                }

                return zippedIndices;
            }
        }

        /// <summary>
        /// array[index] = value. The array will be expanded if necessary.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static StackValue SetValueForIndex(StackValue array, int index, StackValue value, Core core)
        {
            Validity.Assert(array.IsArray || array.IsString);
            if (array.IsString && !value.IsChar)
            {
                core.RuntimeStatus.LogWarning(RuntimeData.WarningID.kTypeMismatch, StringConstants.kAssignNonCharacterToString);
                return StackValue.Null;
            }

            HeapElement arrayHeap = GetHeapElement(array, core);
            index = arrayHeap.ExpandByAcessingAt(index);
            StackValue oldValue = arrayHeap.SetValue(index, value);
            return oldValue;
        }

        /// <summary>
        /// array[index] = value. Here index can be any type. 
        /// 
        /// Note this function doesn't support the replication of array indexing.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static StackValue SetValueForIndex(StackValue array, StackValue index, StackValue value, Core core)
        {
            Validity.Assert(array.IsArray || array.IsString);

            if (index.IsNumeric)
            {
                index = index.ToInteger();
                return SetValueForIndex(array, (int)index.opdata, value, core);
            }
            else
            {
                HeapElement he = GetHeapElement(array, core);
                if (he.Dict == null)
                {
                    he.Dict = new Dictionary<StackValue, StackValue>(new StackValueComparer(core));
                }

                StackValue oldValue;
                GCUtils.GCRetain(value, core);
                if (he.Dict.TryGetValue(index, out oldValue))
                {
                    GCUtils.GCRelease(oldValue, core);
                }
                else
                {
                    oldValue = StackValue.Null;
                    GCUtils.GCRetain(index, core);
                }
                he.Dict[index] = value;

                return oldValue;
            }
        }

        /// <summary>
        /// array[index1][index2][...][indexN] = value, and
        /// indices = {index1, index2, ..., indexN}
        ///
        /// Note this function doesn't support the replication of array indexing.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="indices"></param>
        /// <param name="value"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static StackValue SetValueForIndices(StackValue array, StackValue[] indices, StackValue value, Core core)
        {
            Validity.Assert(array.IsArray || array.IsString);

            for (int i = 0; i < indices.Length - 1; ++i)
            {
                StackValue index = indices[i];
                HeapElement he = GetHeapElement(array, core);

                StackValue subArray;

                if (index.IsNumeric)
                {
                    index = index.ToInteger();
                    int absIndex = he.ExpandByAcessingAt((int)index.opdata);
                    subArray = he.Stack[absIndex];
                }
                else
                {
                    subArray = GetValueFromIndex(array, index, core);
                }

                // auto-promotion
                if (!subArray.IsArray)
                {
                    subArray = core.Heap.AllocateArray(new StackValue[] { subArray }, null);
                    GCUtils.GCRetain(subArray, core);
                    SetValueForIndex(array, index, subArray, core);
                }

                array = subArray;
            }
            
            return SetValueForIndex(array, indices[indices.Length - 1], value, core);
        }

        /// <summary>
        /// array[index1][index2][...][indexN] = value, and
        /// indices = {index1, index2, ..., indexN}
        /// </summary>
        /// <param name="array"></param>
        /// <param name="indices"></param>
        /// <param name="value"></param>
        /// <param name="t"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static StackValue SetValueForIndices(StackValue array, List<StackValue> indices, StackValue value, Type t, Core core)
        {
            StackValue[][] zippedIndices = ArrayUtils.GetZippedIndices(indices, core);
            if (zippedIndices == null || zippedIndices.Length == 0)
            {
                return StackValue.Null;
            }

            if (zippedIndices.Length == 1)
            {
                StackValue coercedData = TypeSystem.Coerce(value, t, core);
                GCUtils.GCRetain(coercedData, core);
                return ArrayUtils.SetValueForIndices(array, zippedIndices[0], coercedData, core);
            }

            if (t.rank > 0)
            {
                t.rank = t.rank - 1;
            }

            if (value.IsArray)
            {
                // Replication happens on both side.
                HeapElement dataHeapElement = GetHeapElement(value, core);
                int length = Math.Min(zippedIndices.Length, dataHeapElement.VisibleSize);

                StackValue[] oldValues = new StackValue[length];
                for (int i = 0; i < length; ++i)
                {
                    StackValue coercedData = TypeSystem.Coerce(dataHeapElement.Stack[i], t, core);
                    GCUtils.GCRetain(coercedData, core);
                    oldValues[i] = SetValueForIndices(array, zippedIndices[i], coercedData, core);
                }

                // The returned old values shouldn't have any key-value pairs
                return core.Heap.AllocateArray(oldValues, null);
            }
            else
            {
                // Replication is only on the LHS, so collect all old values 
                // and return them in an array. 
                StackValue coercedData = TypeSystem.Coerce(value, t, core);
                GCUtils.GCRetain(coercedData, core);

                StackValue[] oldValues = new StackValue[zippedIndices.Length];
                for (int i = 0; i < zippedIndices.Length; ++i)
                {
                    oldValues[i] = SetValueForIndices(array, zippedIndices[i], coercedData, core);
                }

                // The returned old values shouldn't have any key-value pairs
                return core.Heap.AllocateArray(oldValues, null);
            }
        }

        /// <summary>
        /// = array[index]
        /// </summary>
        /// <param name="array"></param>
        /// <param name="index"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static StackValue GetValueFromIndex(StackValue array, int index, Core core)
        {
            Validity.Assert(array.IsArray || array.IsString);
            if (!array.IsArray && !array.IsString)
            {
                return StackValue.Null;
            }

            HeapElement he = GetHeapElement(array, core);
            return StackUtils.GetValue(he, index, core);
        }

        /// <summary>
        /// = array[index].
        /// 
        /// Note this function doesn't support the replication of array indexing.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="index"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static StackValue GetValueFromIndex(StackValue array, StackValue index, Core core)
        {
            Validity.Assert(array.IsArray || array.IsString);
            if (!array.IsArray && !array.IsString)
            {
                return StackValue.Null;
            }

            if (index.IsNumeric)
            {
                index = index.ToInteger();
                return GetValueFromIndex(array, (int)index.opdata, core);
            }
            else if (index.IsArrayKey)
            {
                int fullIndex = (int)index.opdata;
                HeapElement he = GetHeapElement(array, core);

                if (he.VisibleSize > fullIndex)
                {
                    return GetValueFromIndex(array, fullIndex, core);
                }
                else
                {
                    fullIndex = fullIndex - he.VisibleSize;
                    if (he.Dict != null && he.Dict.Count > fullIndex)
                    {
                        int count = 0;
                        foreach (var key in he.Dict.Keys)
                        {
                            if (count == fullIndex)
                            {
                                return he.Dict[key];
                            }
                            count = count + 1;
                        }
                    }
                }

                return StackValue.Null;
            }
            else
            {
                HeapElement he = GetHeapElement(array, core);
                StackValue value = StackValue.Null;

                if (he.Dict != null && he.Dict.TryGetValue(index, out value))
                {
                    return value;
                }
                else
                {
                    return StackValue.Null;
                }
            }
        }

        /// <summary>
        /// = array[index1][index2][...][indexN], and
        /// indices = {index1, index2, ..., indexN}
        /// 
        /// Note this function doesn't support the replication of array indexing.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="indices"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static StackValue GetValueFromIndices(StackValue array, StackValue[] indices, Core core)
        {
            Validity.Assert(array.IsArray || array.IsString);
            for (int i = 0; i < indices.Length - 1; ++i)
            {
                StackValue index = indices[i];
                if (index.IsNumeric)
                {
                    index = index.ToInteger();
                    array = GetValueFromIndex(array, (int)index.opdata, core);
                }
                else
                {
                    if (!array.IsArray)
                    {
                        core.RuntimeStatus.LogWarning(WarningID.kOverIndexing, StringConstants.kArrayOverIndexed);
                        return StackValue.Null;
                    }
                    array = GetValueFromIndex(array, index, core);
                }

                if (!array.IsArray && !array.IsString)
                {
                    core.RuntimeStatus.LogWarning(WarningID.kOverIndexing, StringConstants.kArrayOverIndexed);
                    return StackValue.Null;
                }
            }

            return GetValueFromIndex(array, indices[indices.Length - 1], core);
        }

        /// <summary>
        /// = array[index1][index2][...][indexN], and
        /// indices = {index1, index2, ..., indexN}
        /// </summary>
        /// <param name="array"></param>
        /// <param name="indices"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static StackValue GetValueFromIndices(StackValue array, List<StackValue> indices, Core core)
        {
            if (indices.Count == 0)
            {
                return array;
            }
            else if (!array.IsArray && !array.IsString)
            {
                core.RuntimeStatus.LogWarning(WarningID.kOverIndexing, StringConstants.kArrayOverIndexed);
                return StackValue.Null;
            }

            StackValue[][] zippedIndices = ArrayUtils.GetZippedIndices(indices, core);
            if (zippedIndices == null || zippedIndices.Length == 0)
            {
                return StackValue.Null;
            }

            StackValue[] values = new StackValue[zippedIndices.Length];
            for (int i = 0; i < zippedIndices.Length; ++i)
            {
                values[i] = GetValueFromIndices(array, zippedIndices[i], core);
            }

            if (zippedIndices.Length > 1)
            {
                for (int i = 0; i < values.Length; ++i)
                {
                    GCUtils.GCRetain(values[i], core);
                }

                return core.Heap.AllocateArray(values, null);
            }
            else
            {
                return values[0];
            }
        }

        /// <summary>
        /// Simply copy an array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static StackValue CopyArray(StackValue array, Core core)
        {
            Type anyType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank);
            return CopyArray(array, anyType, core);
        }

        /// <summary>
        /// Copy an array and coerce its elements/values to target type
        /// </summary>
        /// <param name="array"></param>
        /// <param name="type"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static StackValue CopyArray(StackValue array, Type type, Core core)
        {
            Validity.Assert(array.IsArray);
            if (!array.IsArray)
            {
                return StackValue.Null;
            }

            HeapElement he = GetHeapElement(array, core);
            Validity.Assert(he != null);

            int elementSize = GetElementSize(array, core);
            StackValue[] elements = new StackValue[elementSize];
            for (int i = 0; i < elementSize; i++)
            {
                StackValue coercedValue = TypeSystem.Coerce(he.Stack[i], type, core);
                GCUtils.GCRetain(coercedValue, core);
                elements[i] = coercedValue;
            }

            Dictionary<StackValue, StackValue> dict = null;
            if (he.Dict != null)
            {
                dict = new Dictionary<StackValue, StackValue>(new StackValueComparer(core));
                foreach (var pair in he.Dict)
                {
                    StackValue key = pair.Key;
                    StackValue value = pair.Value;
                    StackValue coercedValue = TypeSystem.Coerce(value, type, core);

                    GCUtils.GCRetain(key, core);
                    GCUtils.GCRetain(coercedValue, core);

                    dict[key] = coercedValue;
                }
            }

            return core.Heap.AllocateArray(elements, dict);
        }

        /// <summary>
        /// Get all values that stored in the array, including in dictionary
        /// </summary>
        /// <param name="array"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static IEnumerable<StackValue> GetValues(StackValue array, Core core)
        {
            if (!array.IsArray && !array.IsString)
            {
                return Enumerable.Empty<StackValue>();
            }

            HeapElement he = GetHeapElement(array, core);
            return (he.Dict == null) ? he.VisibleItems : he.VisibleItems.Concat(he.Dict.Values);
        }

        private static StackValue[] GetFlattenValue(StackValue array, Core core)
        {
            Queue<StackValue> workingSet = new Queue<StackValue>();
            List<StackValue> flattenValues = new List<StackValue>();

            if (!array.IsArray)
            {
                return null;
            }

            workingSet.Enqueue(array);
            while (workingSet.Count > 0)
            {
                array = workingSet.Dequeue();
                HeapElement he = GetHeapElement(array, core);
                foreach (var value in he.VisibleItems)
                {
                    if (value.IsArray)
                    {
                        workingSet.Enqueue(value);
                    }
                    else
                    {
                        flattenValues.Add(value);
                    }
                }

                if (he.Dict != null)
                {
                    foreach (var value in he.Dict.Values)
                    {
                        if (value.IsArray)
                        {
                            workingSet.Enqueue(value);
                        }
                        else
                        {
                            flattenValues.Add(value);
                        }
                    }
                }
            }

            return flattenValues.ToArray();
        }

        /// <summary>
        /// Get all keys from an array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static StackValue[] GetKeys(StackValue array, Core core)
        {
            Validity.Assert(array.IsArray);
            if (!array.IsArray)
            {
                return null;
            }

            HeapElement he = GetHeapElement(array, core);
            var keys = Enumerable.Range(0, he.VisibleSize).Select(i => StackValue.BuildInt(i)).ToList(); 
            if (he.Dict != null)
            {
                keys.AddRange(he.Dict.Keys);
            }

            return keys.ToArray();
        }

        /// <summary>
        /// Check if an array contain key
        /// </summary>
        /// <param name="array"></param>
        /// <param name="key"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static bool ContainsKey(StackValue array, StackValue key, Core core)
        {
            Validity.Assert(array.IsArray);
            if (!array.IsArray)
            {
                return false;
            }

            HeapElement he = GetHeapElement(array, core);
            if (key.IsNumeric)
            {
                long index = key.ToInteger().opdata;
                if (index < 0)
                {
                    index = index + he.VisibleSize;
                }
                return (index >= 0 && index < he.VisibleSize);
            }
            else
            {
                return he.Dict != null && he.Dict.ContainsKey(key);
            }
        }

        public static bool RemoveKey(StackValue array, StackValue key, Core core)
        {
            Validity.Assert(array.IsArray);
            if (!array.IsArray)
            {
                return false;
            }

            HeapElement he = GetHeapElement(array, core);

            if (key.IsNumeric)
            {
                long index = key.ToInteger().opdata;
                if (index < 0)
                {
                    index = index + he.VisibleSize;
                }

                if (index >= 0 && index < he.VisibleSize)
                {
                    he.Stack[index] = StackValue.Null;

                    if (index == he.VisibleSize - 1)
                    {
                        he.VisibleSize -= 1;
                    }
                    return true;
                }
            }
            else
            {
                if (he.Dict != null && he.Dict.ContainsKey(key))
                {
                    StackValue value = he.Dict[key];
                    GCUtils.GCRelease(key, core);
                    GCUtils.GCRelease(value, core);
                    he.Dict.Remove(key);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get an array's next key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static StackValue GetNextKey(StackValue key, Core core)
        {
            StackValue array;
            int index;

            if (!key.TryGetArrayKey(out array, out index))
            {
                return StackValue.Null;
            }

            HeapElement he = GetHeapElement(array, core);
            if ((he.VisibleSize  > index + 1) ||
                (he.Dict != null && he.Dict.Count + he.VisibleSize > index + 1))
            {
                return StackValue.BuildArrayKey(array, index + 1);
            }

            return StackValue.Null;
        }

        /// <summary>
        /// Try to get value for key from nested dictionaries. This function is
        /// used in the case that indexing into dictionaries that returned from
        /// a replicated function whose return type is dictionary.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static bool TryGetValueFromNestedDictionaries(StackValue array, StackValue key, out StackValue value, Core core)
        {
            if (!array.IsArray)
            {
                value = StackValue.Null;
                return false;
            }

            HeapElement he = GetHeapElement(array, core);
            if (he.Dict != null && he.Dict.TryGetValue(key, out value))
            {
                return true;
            }

            var values = new List<StackValue>();
            bool hasValue = false;
            foreach (var element in he.VisibleItems)
            {
                StackValue valueInElement;
                if (TryGetValueFromNestedDictionaries(element, key, out valueInElement, core))
                {
                    hasValue = true;
                    GCUtils.GCRetain(valueInElement, core);
                    values.Add(valueInElement);
                }
            }

            if (hasValue)
            {
                value = core.Heap.AllocateArray(values, null);
                return true;
            }
            else
            {
                value = StackValue.Null;
                return false;
            }
        }
    }
}
