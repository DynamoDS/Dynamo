using System;
using System.Collections.Generic;
using System.Linq;
using ProtoCore.Exceptions;
using ProtoCore.Properties;
using ProtoCore.Runtime;
using ProtoCore.Utils;

namespace ProtoCore.DSASM
{
    public class DSArray : HeapElement
    {
        /// <summary>
        /// Create an array with pre-allocated size.
        /// </summary>
        public DSArray(int size, Heap heap)
            : base(size, heap)
        {
            MetaData = new MetaData { type = (int)PrimitiveType.Array };
        }

        /// <summary>
        /// Create an array and populate with input values
        /// </summary>
        public DSArray(StackValue[] values, Heap heap)
            : base(values, heap)
        {
            MetaData = new MetaData { type = (int)PrimitiveType.Array };
        }

        public IEnumerable<StackValue> Keys
        {
            get
            {
                return Enumerable.Range(0, Count).Select(i => StackValue.BuildInt(i));
            }
        }

        /// <summary>
        /// Enqueue all reference-typed element.
        /// Note: it is only used by heap manager to do garbage collection.
        /// </summary>
        /// <returns></returns>
        public void CollectElementsForGC(Queue<StackValue> gcQueue)
        {
            foreach (var item in Values)
            {
                if (item.IsReferenceType)
                {
                    gcQueue.Enqueue(item);
                }
            }
        }

        /// <summary>
        /// Returns true if array contain key or not.
        /// </summary>
        public bool ContainsKey(StackValue key)
        {
            if (key.IsNumeric)
            {
                var index = (int)key.ToInteger().IntegerValue;
                if (index < 0)
                {
                    index = index + Count;
                }
                return (index >= 0 && index < Count);
            }

            return false;
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

            return false;
        }

        public IDictionary<StackValue,StackValue> ToDictionary()
        {
            return Enumerable.Range(0, Count)
                .Select(i => new KeyValuePair<StackValue, StackValue>(StackValue.BuildInt(i), GetValueAt(i)))
                .ToDictionary(p => p.Key, p => p.Value);
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
            var elements = new StackValue[Count];
            for (int i = 0; i < Count; i++)
            {
                var coercedValue = TypeSystem.Coerce(GetValueAt(i), type, runtimeCore);
                elements[i] = coercedValue;
            }

            try
            {
                var svArray = heap.AllocateArray(elements);
                var array = heap.ToHeapObject<DSArray>(svArray);
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
                index = ExpandByAcessingAt(index);
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

            var oldValue = GetValueAt(index);
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
                return SetValueForIndex((int)index.ToInteger().IntegerValue, value, runtimeCore);
            }

            // If the index is non-numeric, return null and emit warning
            runtimeCore.RuntimeStatus.LogWarning(WarningID.InvalidArrayIndexType, Resources.InvalidArrayIndexType);
            return StackValue.Null;
        }

        /// <summary>
        /// array[index1][index2][...][indexN] = value, and
        /// indices = {index1, index2, ..., indexN}
        ///
        /// Note this function doesn't support the replication of array indexing.
        /// </summary>
        public StackValue SetValueForIndices(StackValue[] indices, StackValue value, RuntimeCore runtimeCore)
        {
            var array = this;
            for (int i = 0; i < indices.Length - 1; ++i)
            {
                var index = indices[i];
                StackValue svSubArray;

                if (index.IsNumeric)
                {
                    index = index.ToInteger();
                    try
                    {
                        int absIndex = array.ExpandByAcessingAt((int)index.IntegerValue);
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

                array = heap.ToHeapObject<DSArray>(svSubArray);
            }

            return array.SetValueForIndex(indices[indices.Length - 1], value, runtimeCore);
        }

        /// <summary>
        /// array[index1][index2][...][indexN] = value, and
        /// indices = {index1, index2, ..., indexN}
        /// </summary>
        public StackValue SetValueForIndices(List<StackValue> indices, StackValue value, RuntimeCore runtimeCore)
        {
            var zippedIndices = ArrayUtils.GetZippedIndices(indices, runtimeCore);
            if (zippedIndices == null || zippedIndices.Length == 0)
            {
                return StackValue.Null;
            }

            var t = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var);

            if (zippedIndices.Length == 1)
            {
                var coercedData = TypeSystem.Coerce(value, t, runtimeCore);
                return SetValueForIndices(zippedIndices[0], coercedData, runtimeCore);
            }

            if (value.IsArray)
            {
                // Replication happens on both side.
                var dataElements = heap.ToHeapObject<DSArray>(value);
                var length = Math.Min(zippedIndices.Length, dataElements.Count);

                var oldValues = new StackValue[length];
                for (int i = 0; i < length; ++i)
                {
                    var coercedData = TypeSystem.Coerce(dataElements.GetValueAt(i), t, runtimeCore);
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
                var coercedData = TypeSystem.Coerce(value, t, runtimeCore);
                var oldValues = new StackValue[zippedIndices.Length];
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

            return StackValue.Null;
        }

        /// <summary>
        /// = array[index1][index2][...][indexN], and
        /// indices = {index1, index2, ..., indexN}
        /// 
        /// Note this function doesn't support the replication of array indexing.
        /// </summary>
        public StackValue GetValueFromIndices(StackValue[] indices, RuntimeCore runtimeCore)
        {
            DSArray array = this;

            for (int i = 0; i < indices.Length - 1; ++i)
            {
                var index = indices[i];
                var svArray = StackValue.Null;

                if (index.IsNumeric)
                {
                    index = index.ToInteger();
                    svArray = array.GetValueFromIndex((int)index.IntegerValue, runtimeCore);
                }

                if (!svArray.IsArray)
                {
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.OverIndexing, Resources.kArrayOverIndexed);
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
        public StackValue GetValueFromIndices(List<StackValue> indices, RuntimeCore runtimeCore)
        {

            var zippedIndices = ArrayUtils.GetZippedIndices(indices, runtimeCore);
            if (zippedIndices == null || zippedIndices.Length == 0)
            {
                return StackValue.Null;
            }

            var values = new StackValue[zippedIndices.Length];
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

        public static bool CompareFromDifferentCore(DSArray array1, DSArray array2, RuntimeCore rtCore1, RuntimeCore rtCore2, Context context = null)
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

            return true;
        }
    }
}
