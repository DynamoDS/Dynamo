using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore.Properties;
using ProtoCore.Runtime;

namespace ProtoCore.DSASM
{
    public class DSArray : HeapElement
    {
        private Dictionary<StackValue, StackValue> Dict;
        public DSArray(int size)
            : base(size)
        {
            Dict = new Dictionary<StackValue, StackValue>();
            MetaData = new MetaData { type = (int)PrimitiveType.kTypeArray };
        }

        public IEnumerable<StackValue> Values
        {
            get
            {
                return VisibleItems.Concat(Dict.Values);
            }
        }

        /// <summary>
        /// Check if an array contain key
        /// </summary>
        /// <param name="array"></param>
        /// <param name="key"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public bool ContainsKey(StackValue key)
        {
            if (key.IsNumeric)
            {
                long index = key.ToInteger().opdata;
                if (index < 0)
                {
                    index = index + VisibleSize;
                }
                return (index >= 0 && index < VisibleSize);
            }
            else
            {
                return Dict != null && Dict.ContainsKey(key);
            }
        }

        public bool RemoveKey(StackValue key)
        {
            if (key.IsNumeric)
            {
                long index = key.ToInteger().opdata;
                if (index < 0)
                {
                    index = index + VisibleSize;
                }

                if (index >= 0 && index < VisibleSize)
                {
                    SetItemAt((int)index, StackValue.Null);

                    if (index == VisibleSize - 1)
                    {
                        VisibleSize -= 1;
                    }
                    return true;
                }
            }
            else
            {
                if (Dict != null && Dict.ContainsKey(key))
                {
                    Dict.Remove(key);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Try to get value for key from nested dictionaries. This function is
        /// used in the case that indexing into dictionaries that returned from
        /// a replicated function whose return type is dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public bool TryGetValueFromNestedDictionaries(StackValue key, out StackValue value, RuntimeCore runtimeCore)
        {
            if (Dict != null && Dict.TryGetValue(key, out value))
            {
                return true;
            }

            var values = new List<StackValue>();
            bool hasValue = false;
            foreach (var element in VisibleItems)
            {
                if (!(element.IsArray))
                    continue;

                StackValue valueInElement;
                DSArray subArray = heap.ToHeapObject<DSArray>(element);
                if (subArray.TryGetValueFromNestedDictionaries(key, out valueInElement, runtimeCore))
                {
                    hasValue = true;
                    values.Add(valueInElement);
                }
            }

            if (hasValue)
            {
                value = heap.AllocateArray(values);
                return true;
            }
            else
            {
                value = StackValue.Null;
                return false;
            }
        }

        /// <summary>
        /// Get all keys from an array
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public StackValue[] GetKeys(RuntimeCore runtimeCore)
        {
            var keys = Enumerable.Range(0, VisibleSize).Select(i => StackValue.BuildInt(i))
                                 .Concat(Dict.Keys)
                                 .ToArray();
            return keys;
        }

        /// <summary>
        /// Get a list of key-value pairs for an array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="runtimeCore"></param>
        /// <returns></returns>
        public IDictionary<StackValue, StackValue> ToDictionary()
        {
            var dict = Enumerable.Range(0, VisibleSize)
                                 .Select(i => new KeyValuePair<StackValue, StackValue>(StackValue.BuildInt(i), GetItemAt(i)))
                                 .Concat(Dict ?? Enumerable.Empty<KeyValuePair<StackValue, StackValue>>())
                                 .ToDictionary(p => p.Key, p => p.Value);
            return dict;
        }

        /// <summary>
        /// Simply copy an array.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public StackValue CopyArray(RuntimeCore runtimeCore)
        {
            Type anyType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank);
            return CopyArray(anyType, runtimeCore);
        }

        /// <summary>
        /// Copy an array and coerce its elements/values to target type
        /// </summary>
        /// <param name="array"></param>
        /// <param name="type"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public StackValue CopyArray(Type type, RuntimeCore runtimeCore)
        {
            StackValue[] elements = new StackValue[VisibleSize];
            for (int i = 0; i < VisibleSize; i++)
            {
                StackValue coercedValue = TypeSystem.Coerce(GetItemAt(i), type, runtimeCore);
                elements[i] = coercedValue;
            }

            Dictionary<StackValue, StackValue> dict = null;
            if (Dict != null)
            {
                dict = new Dictionary<StackValue, StackValue>(new StackValueComparer(runtimeCore));
                foreach (var pair in Dict)
                {
                    StackValue key = pair.Key;
                    StackValue value = pair.Value;
                    StackValue coercedValue = TypeSystem.Coerce(value, type, runtimeCore);

                    dict[key] = coercedValue;
                }
            }

            var svArray = heap.AllocateArray(elements);
            var array = heap.ToHeapObject<DSArray>(svArray);
            array.Dict = dict;
            return svArray;
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
        private StackValue[][] GetZippedIndices(List<StackValue> indices, RuntimeCore runtimeCore)
        {
            List<StackValue[]> allFlattenValues = new List<StackValue[]>();

            int zipLength = System.Int32.MaxValue;
            foreach (var index in indices)
            {
                int length = 1;
                if (index.IsArray)
                {
                    StackValue[] flattenValues = GetFlattenValue(index, runtimeCore);
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
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public StackValue SetValueForIndex(int index, StackValue value, RuntimeCore runtimeCore)
        {
            index = ExpandByAcessingAt(index);
            StackValue oldValue = this.SetValue(index, value);
            return oldValue;
        }

        /// <summary>
        /// array[index] = value. Here index can be any type. 
        /// 
        /// Note this function doesn't support the replication of array indexing.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public StackValue SetValueForIndex(StackValue index, StackValue value, RuntimeCore runtimeCore)
        {
            if (index.IsNumeric)
            {
                index = index.ToInteger();
                return SetValueForIndex((int)index.opdata, value, runtimeCore);
            }
            else
            {
                StackValue oldValue;
                if (Dict.TryGetValue(index, out oldValue))
                {
                    oldValue = StackValue.Null;
                }
                Dict[index] = value;

                return oldValue;
            }
        }

        /// <summary>
        /// array[index1][index2][...][indexN] = value, and
        /// indices = {index1, index2, ..., indexN}
        ///
        /// Note this function doesn't support the replication of array indexing.
        /// </summary>
        /// <param name="indices"></param>
        /// <param name="value"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public StackValue SetValueForIndices(StackValue[] indices, StackValue value, RuntimeCore runtimeCore)
        {
            DSArray array = this;
            for (int i = 0; i < indices.Length - 1; ++i)
            {
                StackValue index = indices[i];
                StackValue svSubArray;

                if (index.IsNumeric)
                {
                    index = index.ToInteger();
                    int absIndex = array.ExpandByAcessingAt((int)index.opdata);
                    svSubArray  = array.GetItemAt(absIndex);
                }
                else
                {
                    svSubArray = array.GetValueFromIndex(index, runtimeCore);
                }

                // auto-promotion
                if (!svSubArray.IsArray)
                {
                    svSubArray = heap.AllocateArray(new StackValue[] { svSubArray });
                    heap.ToHeapObject<DSArray>(svSubArray).SetValueForIndex(index, svSubArray, runtimeCore);
                }

                array = heap.ToHeapObject<DSArray>(svSubArray);
            }

            return array.SetValueForIndex(indices[indices.Length - 1], value, runtimeCore);
        }

        /// <summary>
        /// array[index1][index2][...][indexN] = value, and
        /// indices = {index1, index2, ..., indexN}
        /// </summary>
        /// <param name="indices"></param>
        /// <param name="value"></param>
        /// <param name="t"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public StackValue SetValueForIndices(List<StackValue> indices, StackValue value, Type t, RuntimeCore runtimeCore)
        {
            StackValue[][] zippedIndices = GetZippedIndices(indices, runtimeCore);
            if (zippedIndices == null || zippedIndices.Length == 0)
            {
                return StackValue.Null;
            }

            if (zippedIndices.Length == 1)
            {
                StackValue coercedData = TypeSystem.Coerce(value, t, runtimeCore);
                return SetValueForIndices(zippedIndices[0], coercedData, runtimeCore);
            }

            if (t.rank > 0)
            {
                t.rank = t.rank - 1;
            }

            if (value.IsArray)
            {
                // Replication happens on both side.
                DSArray dataElements = heap.ToHeapObject<DSArray>(value);
                int length = Math.Min(zippedIndices.Length, dataElements.VisibleSize);

                StackValue[] oldValues = new StackValue[length];
                for (int i = 0; i < length; ++i)
                {
                    StackValue coercedData = TypeSystem.Coerce(dataElements.GetItemAt(i), t, runtimeCore);
                    oldValues[i] = SetValueForIndices(zippedIndices[i], coercedData, runtimeCore);
                }

                // The returned old values shouldn't have any key-value pairs
                return heap.AllocateArray(oldValues);
            }
            else
            {
                // Replication is only on the LHS, so collect all old values 
                // and return them in an array. 
                StackValue coercedData = TypeSystem.Coerce(value, t, runtimeCore);

                StackValue[] oldValues = new StackValue[zippedIndices.Length];
                for (int i = 0; i < zippedIndices.Length; ++i)
                {
                    oldValues[i] = SetValueForIndices(zippedIndices[i], coercedData, runtimeCore);
                }

                // The returned old values shouldn't have any key-value pairs
                return heap.AllocateArray(oldValues);
            }
        }

        /// <summary>
        /// = array[index]
        /// </summary>
        /// <param name="array"></param>
        /// <param name="index"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public StackValue GetValueFromIndex(int index, RuntimeCore runtimeCore)
        {
            return StackUtils.GetValue(this, index, runtimeCore);
        }

        /// <summary>
        /// = array[index].
        /// 
        /// Note this function doesn't support the replication of array indexing.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public StackValue GetValueFromIndex(StackValue index, RuntimeCore runtimeCore)
        {
            if (index.IsNumeric)
            {
                index = index.ToInteger();
                return GetValueFromIndex((int)index.opdata, runtimeCore);
            }
            else if (index.IsArrayKey)
            {
                int fullIndex = (int)index.opdata;

                if (VisibleSize > fullIndex)
                {
                    return GetValueFromIndex(fullIndex, runtimeCore);
                }
                else
                {
                    fullIndex = fullIndex - VisibleSize;
                    if (Dict != null && Dict.Count > fullIndex)
                    {
                        int count = 0;
                        foreach (var key in Dict.Keys)
                        {
                            if (count == fullIndex)
                            {
                                return Dict[key];
                            }
                            count = count + 1;
                        }
                    }
                }

                return StackValue.Null;
            }
            else
            {
                StackValue value = StackValue.Null;
                if (Dict.TryGetValue(index, out value))
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
        public StackValue GetValueFromIndices(StackValue[] indices, RuntimeCore runtimeCore)
        {
            StackValue svArray = StackValue.Null;

            for (int i = 0; i < indices.Length - 1; ++i)
            {
                StackValue index = indices[i];
                if (index.IsNumeric)
                {
                    index = index.ToInteger();
                    svArray = GetValueFromIndex((int)index.opdata, runtimeCore);
                }
                else
                {
                    if (!svArray.IsArray)
                    {
                        runtimeCore.RuntimeStatus.LogWarning(WarningID.kOverIndexing, Resources.kArrayOverIndexed);
                        return StackValue.Null;
                    }
                    svArray = heap.ToHeapObject<DSArray>(svArray).GetValueFromIndex(index, runtimeCore);
                }

                if (!svArray.IsArray)
                {
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.kOverIndexing, Resources.kArrayOverIndexed);
                    return StackValue.Null;
                }
            }

            DSArray innerArray = heap.ToHeapObject<DSArray>(svArray);
            return innerArray.GetValueFromIndex(indices[indices.Length - 1], runtimeCore);
        }

        /// <summary>
        /// = array[index1][index2][...][indexN], and
        /// indices = {index1, index2, ..., indexN}
        /// </summary>
        /// <param name="indices"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public StackValue GetValueFromIndices(List<StackValue> indices, RuntimeCore runtimeCore)
        {
            StackValue[][] zippedIndices = GetZippedIndices(indices, runtimeCore);
            if (zippedIndices == null || zippedIndices.Length == 0)
            {
                return StackValue.Null;
            }

            StackValue[] values = new StackValue[zippedIndices.Length];
            for (int i = 0; i < zippedIndices.Length; ++i)
            {
                values[i] = GetValueFromIndices(zippedIndices[i], runtimeCore);
            }

            if (zippedIndices.Length > 1)
            {
                return runtimeCore.RuntimeMemory.Heap.AllocateArray(values);
            }
            else
            {
                return values[0];
            }
        }

        private StackValue[] GetFlattenValue(StackValue array, RuntimeCore runtimeCore)
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
                foreach (var value in heap.ToHeapObject<DSArray>(array).Values)
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

            return flattenValues.ToArray();
        }

        public static bool CompareFromDifferentCore(DSArray array1, DSArray array2, RuntimeCore rtCore1, RuntimeCore rtCore2, Context context = null)
        {
            if (array1.VisibleSize != array2.VisibleSize)
            {
                return false;
            }

            var dict1 = array1.ToDictionary();
            for (int i = 0; i < array1.VisibleSize; i++)
            {
                if (!StackUtils.CompareStackValues(array1.GetItemAt(i), array2.GetItemAt(i), rtCore1, rtCore2, context))
                {
                    return false;
                }
            }

            foreach (var key in array1.Dict.Keys)
            {
                StackValue value1 = array1.Dict[key];
                StackValue value2 = StackValue.Null;
                if (!array2.Dict.TryGetValue(key, out value2))
                {
                    return false;
                }

                if (!StackUtils.CompareStackValues(value1, value2, rtCore1, rtCore2))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public class DSObject : HeapElement
    {
        public DSObject(int size)
            : base(size)
        {
            MetaData = new MetaData { type = (int)PrimitiveType.kTypePointer };
        }

        public StackValue GetValueFromIndex(int index, RuntimeCore runtimeCore)
        {
            return StackUtils.GetValue(this, index, runtimeCore);
        }

        public StackValue SetValueAtIndex(int index, StackValue value, RuntimeCore runtimeCore)
        {
            index = ExpandByAcessingAt(index);
            StackValue oldValue = this.SetValue(index, value);
            return oldValue;
        }
    }

    public class DSString : HeapElement
    {
        private StackValue pointer = StackValue.Null;

        public DSString(int size)
            : base(size)
        {
            MetaData = new MetaData { type = (int)PrimitiveType.kTypeString };
        }

        public void SetPointer(StackValue pointer)
        {
            this.pointer = pointer;
        }

        public string Value
        {
            get
            {
                return heap.GetString(pointer);
            }
        }

        public StackValue this[int index]
        {
            get
            {
                string str = heap.GetString(pointer);
                if (str == null)
                    return StackValue.Null;

                if (index < 0)
                {
                    index = index + str.Length;
                }

                if (index >= str.Length || index < 0)
                {
                    throw new ArgumentOutOfRangeException("index", Resources.kArrayOverIndexed);
                }

                return heap.AllocateString(str.Substring(index, 1));
            }
        }

        public override bool Equals(object obj)
        {
            DSString otherString = obj as DSString;
            if (otherString == null)
                return false;

            if (object.ReferenceEquals(heap, otherString.heap) && pointer.Equals(otherString.pointer))
                return true;

            return Value.Equals(otherString.Value);
        }
    }
}
