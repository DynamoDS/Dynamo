using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore.Properties;
using ProtoCore.Runtime;
using ProtoCore.Utils;
using ProtoCore.Exceptions;

namespace ProtoCore.DSASM
{
    public class DSDictionary : HeapElement
    {
        // Kept around basically for convenience. Strictly speaking, the keys/values are owned by the DS heap. TODO(boyerp) verify
        private Dictionary<StackValue, StackValue> Contents;

        internal static StackValue ByKeysValues(DSArray keys, DSArray values, RuntimeCore runtimeCore)
        {
            var keysArr = keys.Values.ToArray();
            var valuesArr = values.Values.ToArray();

            // TODO(pboyer) verify the type of the keys, skipping non-string/numeric key/value pairs
            if (keysArr.Length != valuesArr.Length)
            {
                runtimeCore.RuntimeStatus.LogWarning(WarningID.Default, "Number of keys does not match the number of values"); // TODO(pboyer) move into resource
                return StackValue.Null;
            }

            return runtimeCore.Heap.AllocateDictionary(keysArr, valuesArr);
        }

        /// <summary>
        /// Create a DSDictionary. This should only be called by Heap itself.
        /// </summary>
        internal DSDictionary(IEnumerable<StackValue> keys, IEnumerable<StackValue> values, Heap heap)
            : base(keys.Concat(values).ToArray(), heap)
        {
            Contents = keys
                .Zip(values, (a, b) => new KeyValuePair<StackValue, StackValue>(a, b))
                .ToDictionary(p => p.Key, p => p.Value);
            MetaData = new MetaData { type = (int)PrimitiveType.Dictionary };
        }

        public IEnumerable<StackValue> Keys
        {
            get
            {
                return Contents.Keys;
            }
        }

        public override IEnumerable<StackValue> Values
        {
            get
            {
                return Contents.Values;
            }
        }

        public override int MemorySize
        {
            get
            {
                return base.MemorySize + Contents.Keys.Count + Contents.Values.Count;
            }
        }
        
        /// <summary>
        /// Returns true if array contain key or not.
        /// </summary>
        public bool ContainsKey(StackValue key)
        {
            if (key.IsNumeric)
            {
                int index = (int)key.ToInteger().IntegerValue;
                if (index < 0)
                {
                    index = index + Count;
                }
                return (index >= 0 && index < Count);
            }
            else
            {
                return Contents != null && Contents.ContainsKey(key);
            }
        }

        /// <summary>
        /// Remove a key from array. Return true if key exsits.
        /// </summary>
        public bool RemoveKey(StackValue key)
        {
            if (key.IsNumeric)
            {
                int index = (int)key.ToInteger().IntegerValue;
                if (index < 0)
                {
                    index = index + Count;
                }

                if (index >= 0 && index < Count)
                {
                    SetValueAt(index, StackValue.Null);

                    if (index == Count - 1)
                    {
                        Count -= 1;
                    }
                    return true;
                }
            }
            else
            {
                if (Contents != null && Contents.ContainsKey(key))
                {
                    Contents.Remove(key);
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

        public bool TryGetValueFromNestedDictionaries(StackValue key, out StackValue value, RuntimeCore runtimeCore)
        {
            if (Contents != null && Contents.TryGetValue(key, out value))
            {
                return true;
            }

            var values = new List<StackValue>();
            bool hasValue = false;
            foreach (var element in Values)
            {
                if (!(element.IsArray))
                    continue;

                StackValue valueInElement;
                var subArray = heap.ToHeapObject<DSDictionary>(element);
                if (subArray.TryGetValueFromNestedDictionaries(key, out valueInElement, runtimeCore))
                {
                    hasValue = true;
                    values.Add(valueInElement);
                }
            }

            if (hasValue)
            {
                try
                {
                    value = heap.AllocateArray(values.ToArray());
                    return true;
                }
                catch (RunOutOfMemoryException)
                {
                    value = StackValue.Null;
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                    return false;
                }
            }
            else
            {
                value = StackValue.Null;
                return false;
            }
        }

        /// <summary>
        /// Returns a list of key-value pairs for an array.
        /// </summary>
        public IDictionary<StackValue, StackValue> ToDictionary()
        {
            var dict = Enumerable.Range(0, Count)
                                 .Select(i => new KeyValuePair<StackValue, StackValue>(StackValue.BuildInt(i), GetValueAt(i)))
                                 .Concat(Contents ?? Enumerable.Empty<KeyValuePair<StackValue, StackValue>>())
                                 .ToDictionary(p => p.Key, p => p.Value);
            return dict;
        }


        public StackValue CopyArray(RuntimeCore runtimeCore)
        {
            var anyType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank);
            return CopyArray(anyType, runtimeCore);
        }

        /// <summary>
        /// Copy an array and coerce its elements/values to target type
        /// </summary>

        public StackValue CopyArray(Type type, RuntimeCore runtimeCore)
        {
            StackValue[] elements = new StackValue[Count];
            for (int i = 0; i < Count; i++)
            {
                StackValue coercedValue = TypeSystem.Coerce(GetValueAt(i), type, runtimeCore);
                elements[i] = coercedValue;
            }

            Dictionary<StackValue, StackValue> dict = null;
            if (Contents != null)
            {
                dict = new Dictionary<StackValue, StackValue>(new StackValueComparer(runtimeCore));
                foreach (var pair in Contents)
                {
                    StackValue key = pair.Key;
                    StackValue value = pair.Value;
                    StackValue coercedValue = TypeSystem.Coerce(value, type, runtimeCore);

                    dict[key] = coercedValue;
                }
            }

            try
            {
                var svArray = heap.AllocateArray(elements);
                var array = heap.ToHeapObject<DSDictionary>(svArray);
                array.Contents = dict;
                return svArray;
            }
            catch (RunOutOfMemoryException)
            {
                runtimeCore.RuntimeStatus.LogWarning(WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                return StackValue.Null;
            }
        }

        /// <summary>
        /// array[index] = value. The array will be expanded if necessary.
        /// </summary>
        public StackValue SetValueForIndex(int index, StackValue value, RuntimeCore runtimeCore)
        {
            try
            {
                index = ExpandByAccessingAt(index);
                if (index < 0)
                    index += Count;
            }
            catch (RunOutOfMemoryException)
            {
                runtimeCore.RuntimeStatus.LogWarning(WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                return StackValue.Null;
            }

            if (index >= Count || index < 0)
            {
                if (runtimeCore != null)
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.OverIndexing, Resources.kArrayOverIndexed);
                return StackValue.Null;
            }

            StackValue oldValue = GetValueAt(index);
            SetValueAt(index, value);
            return oldValue;
        }

        /// <summary>
        /// array[index] = value. Here index can be any type. 
        /// 
        /// Note this function doesn't support the replication of array indexing.
        /// </summary>
        public StackValue SetValueForIndex(StackValue index, StackValue value, RuntimeCore runtimeCore)
        {
            if (index.IsNumeric)
            {
                index = index.ToInteger();
                return SetValueForIndex((int)index.IntegerValue, value, runtimeCore);
            }
            else
            {
                StackValue oldValue;
                if (Contents.TryGetValue(index, out oldValue))
                {
                    oldValue = StackValue.Null;
                }
                Contents[index] = value;

                return oldValue;
            }
        }

        /// <summary>
        /// array[index1][index2][...][indexN] = value, and
        /// indices = {index1, index2, ..., indexN}
        ///
        /// Note this function doesn't support the replication of array indexing.
        /// </summary>
        public StackValue SetValueForIndices(StackValue[] indices, StackValue value, RuntimeCore runtimeCore)
        {
            DSDictionary array = this;
            for (int i = 0; i < indices.Length - 1; ++i)
            {
                var index = indices[i];
                StackValue svSubArray;

                if (index.IsNumeric)
                {
                    index = index.ToInteger();
                    try
                    {
                        int absIndex = array.ExpandByAccessingAt((int)index.IntegerValue);
                        svSubArray = array.GetValueAt(absIndex);
                    }
                    catch (RunOutOfMemoryException)
                    {
                        runtimeCore.RuntimeStatus.LogWarning(WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                        return StackValue.Null;
                    }
                }
                else
                {
                    svSubArray = array.GetValueFromIndex(index, runtimeCore);
                }

                // auto-promotion
                if (!svSubArray.IsArray)
                {
                    try
                    {
                        svSubArray = heap.AllocateArray(new StackValue[] { svSubArray });
                    }
                    catch (RunOutOfMemoryException)
                    {
                        svSubArray = StackValue.Null;
                        runtimeCore.RuntimeStatus.LogWarning(WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                    }
                    array.SetValueForIndex(index, svSubArray, runtimeCore);
                }

                array = heap.ToHeapObject<DSDictionary>(svSubArray);
            }

            return array.SetValueForIndex(indices[indices.Length - 1], value, runtimeCore);
        }

        /// <summary>
        /// array[index1][index2][...][indexN] = value, and
        /// indices = {index1, index2, ..., indexN}
        /// </summary>
        public StackValue SetValueForIndices(List<StackValue> indices, StackValue value, RuntimeCore runtimeCore)
        {
            // TODO(pboyer) this looks broken

            StackValue[][] zippedIndices = ArrayUtils.GetZippedIndices(indices, runtimeCore);
            if (zippedIndices == null || zippedIndices.Length == 0)
            {
                return StackValue.Null;
            }

            var t = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var);

            if (zippedIndices.Length == 1)
            {
                StackValue coercedData = TypeSystem.Coerce(value, t, runtimeCore);
                return SetValueForIndices(zippedIndices[0], coercedData, runtimeCore);
            }

            if (value.IsArray)
            {
                // Replication happens on both side.
                var dataElements = heap.ToHeapObject<DSDictionary>(value);
                int length = Math.Min(zippedIndices.Length, dataElements.Count);

                var oldValues = new StackValue[length];
                for (int i = 0; i < length; ++i)
                {
                    StackValue coercedData = TypeSystem.Coerce(dataElements.GetValueAt(i), t, runtimeCore);
                    oldValues[i] = SetValueForIndices(zippedIndices[i], coercedData, runtimeCore);
                }

                // The returned old values shouldn't have any key-value pairs
                try
                {
                    return heap.AllocateArray(oldValues);
                }
                catch (RunOutOfMemoryException)
                {
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                    return StackValue.Null;
                }
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
                try
                {
                    return heap.AllocateArray(oldValues);
                }
                catch (RunOutOfMemoryException)
                {
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                    return StackValue.Null;
                }
            }
        }

        /// <summary>
        /// = array[index]
        /// </summary>
        public StackValue GetValueFromIndex(int index, RuntimeCore runtimeCore)
        {
            if (index < 0)
                index += Count;

            if (index >= Count || index < 0)
            {
                if (runtimeCore != null)
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.OverIndexing, Resources.kArrayOverIndexed);
                return StackValue.Null;
            }

            return GetValueAt(index);
        }

        /// <summary>
        /// = array[index].
        /// 
        /// Note this function doesn't support the replication of array indexing.
        /// </summary>

        public StackValue GetValueFromIndex(StackValue index, RuntimeCore runtimeCore)
        {
            if (index.IsNumeric)
            {
                index = index.ToInteger();
                return GetValueFromIndex((int)index.IntegerValue, runtimeCore);
            }
            else if (index.IsArrayKey)
            {
                int fullIndex = Constants.kInvalidIndex;
                StackValue array;
                index.TryGetArrayKey(out array, out fullIndex);

                if (Count > fullIndex)
                {
                    return GetValueFromIndex(fullIndex, runtimeCore);
                }
                else
                {
                    fullIndex = fullIndex - Count;
                    if (Contents != null && Contents.Count > fullIndex)
                    {
                        int count = 0;
                        foreach (var key in Contents.Keys)
                        {
                            if (count == fullIndex)
                            {
                                return Contents[key];
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
                if (Contents.TryGetValue(index, out value))
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
        public StackValue GetValueFromIndices(StackValue[] indices, RuntimeCore runtimeCore)
        {
            DSDictionary array = this;

            for (int i = 0; i < indices.Length - 1; ++i)
            {
                StackValue index = indices[i];
                StackValue svArray = StackValue.Null;

                if (index.IsNumeric)
                {
                    index = index.ToInteger();
                    svArray = array.GetValueFromIndex((int)index.IntegerValue, runtimeCore);
                }
                else
                {
                    svArray = array.GetValueFromIndex(index, runtimeCore);
                }

                if (!svArray.IsArray)
                {
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.OverIndexing, Resources.kArrayOverIndexed);
                    return StackValue.Null;
                }

                array = heap.ToHeapObject<DSDictionary>(svArray);
            }

            return array.GetValueFromIndex(indices[indices.Length - 1], runtimeCore);
        }

        /// <summary>
        /// = array[index1][index2][...][indexN], and
        /// indices = {index1, index2, ..., indexN}
        /// </summary>
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
                try
                {
                    return runtimeCore.RuntimeMemory.Heap.AllocateArray(values);
                }
                catch (RunOutOfMemoryException)
                {
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                    return StackValue.Null;
                }
            }
            else
            {
                return values[0];
            }
        }

        public static bool CompareFromDifferentCore(DSDictionary array1, DSDictionary array2, RuntimeCore rtCore1, RuntimeCore rtCore2, Context context = null)
        {
            if (array1.Count != array2.Count)
            {
                return false;
            }

            for (int i = 0; i < array1.Count; i++)
            {
                if (!StackUtils.CompareStackValues(array1.GetValueAt(i), array2.GetValueAt(i), rtCore1, rtCore2, context))
                {
                    return false;
                }
            }

            foreach (var key in array1.Contents.Keys)
            {
                StackValue value1 = array1.Contents[key];
                StackValue value2 = StackValue.Null;
                if (!array2.Contents.TryGetValue(key, out value2))
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
}