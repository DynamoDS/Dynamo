using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore.Properties;
using ProtoCore.Runtime;
using ProtoCore.Utils;

namespace ProtoCore.DSASM
{
    public class DSArray : HeapElement
    {
        private Dictionary<StackValue, StackValue> Dict;
        public DSArray(int size, Heap heap)
            : base(size, heap)
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
        public StackValue[] GetKeys()
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
        /// array[index] = value. The array will be expanded if necessary.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public StackValue SetValueForIndex(int index, StackValue value, RuntimeCore runtimeCore)
        {
            index = ExpandByAcessingAt(index);
            if (index < 0)
                index += VisibleSize;

            if (index >= VisibleSize || index < 0)
            {
                if (runtimeCore != null)
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.kOverIndexing, Resources.kArrayOverIndexed);
                return StackValue.Null;
            }

            StackValue oldValue = GetItemAt(index);
            SetItemAt(index, value);
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
                    array.SetValueForIndex(index, svSubArray, runtimeCore);
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
            StackValue[][] zippedIndices = ArrayUtils.GetZippedIndices(indices, runtimeCore);
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
            if (index < 0)
                index += VisibleSize;

            if (index >= VisibleSize || index < 0)
            {
                if (runtimeCore != null)
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.kOverIndexing, Resources.kArrayOverIndexed);
                return StackValue.Null;
            }

            return GetItemAt(index);
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
            DSArray array = this;

            for (int i = 0; i < indices.Length - 1; ++i)
            {
                StackValue index = indices[i];
                StackValue svArray = StackValue.Null;

                if (index.IsNumeric)
                {
                    index = index.ToInteger();
                    svArray = array.GetValueFromIndex((int)index.opdata, runtimeCore);
                }
                else
                {
                    svArray = array.GetValueFromIndex(index, runtimeCore);
                }

                if (!svArray.IsArray)
                {
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.kOverIndexing, Resources.kArrayOverIndexed);
                    return StackValue.Null;
                }

                array = heap.ToHeapObject<DSArray>(svArray);
            }

            return array.GetValueFromIndex(indices[indices.Length - 1], runtimeCore);
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
            StackValue[][] zippedIndices = ArrayUtils.GetZippedIndices(indices, runtimeCore);
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
        public DSObject(int size, Heap heap)
            : base(size, heap)
        {
            MetaData = new MetaData { type = (int)PrimitiveType.kTypePointer };
        }

        public StackValue GetValueFromIndex(int index, RuntimeCore runtimeCore)
        {
            if (index >= VisibleSize || index < 0)
            {
                if (runtimeCore != null)
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.kOverIndexing, Resources.kArrayOverIndexed);
                return StackValue.Null;
            }

            return GetItemAt(index);
        }

        public StackValue SetValueAtIndex(int index, StackValue value, RuntimeCore runtimeCore)
        {
            if (index >= VisibleSize || index < 0)
            {
                if (runtimeCore != null)
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.kOverIndexing, Resources.kArrayOverIndexed);
                return StackValue.Null;
            }

            StackValue oldValue = GetItemAt(index);
            SetItemAt(index, value);
            return oldValue;
        }
    }

    public class DSString : HeapElement
    {
        public StackValue Pointer = StackValue.Null;

        public DSString(int size, Heap heap)
            : base(size, heap)
        {
            MetaData = new MetaData { type = (int)PrimitiveType.kTypeString };
        }

        public void SetPointer(StackValue pointer)
        {
            this.Pointer = pointer;
        }

        public string Value
        {
            get
            {
                return heap.GetString(this);
            }
        }

        public StackValue GetValueAtIndex(int index, RuntimeCore runtimeCore)
        {
            string str = heap.GetString(this);
            if (str == null)
                return StackValue.Null;

            if (index < 0)
            {
                index = index + str.Length;
            }

            if (index >= str.Length || index < 0)
            {
                if (runtimeCore != null)
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.kOverIndexing, Resources.kStringOverIndexed);
                return StackValue.Null;
            }

            return heap.AllocateString(str.Substring(index, 1));
        }

        public StackValue GetValueAtIndex(StackValue index, RuntimeCore runtimeCore)
        {
            int pos = Constants.kInvalidIndex;
            if (index.IsNumeric)
            {
                pos = (int)index.ToInteger().opdata;
                return GetValueAtIndex(index, runtimeCore);
            }
            else if (index.IsArrayKey)
            {
                pos = (int)index.opdata;
                return GetValueAtIndex(index, runtimeCore);
            }
            else
            {
                if (runtimeCore != null)
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.kInvalidIndexing, Resources.kInvalidArguments);
                return StackValue.Null;
            }
        }

        public override bool Equals(object obj)
        {
            DSString otherString = obj as DSString;
            if (otherString == null)
                return false;

            if (object.ReferenceEquals(heap, otherString.heap) && Pointer.Equals(otherString.Pointer))
                return true;

            return Value.Equals(otherString.Value);
        }
    }
}
