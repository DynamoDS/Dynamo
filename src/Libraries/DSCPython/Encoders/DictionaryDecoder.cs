using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Utilities;
using Python.Runtime;

namespace DSCPython.Encoders
{
    internal class DictionaryDecoder : IPyObjectDecoder
    {
        private static readonly Type[] decodableTypes = new Type[]
        {
            typeof(IDictionary<,>), typeof(Dictionary<,>),
            typeof(IDictionary), typeof(Hashtable)
        };

        public bool TryDecode<T>(PyObject pyObj, out T value)
        {
            if (!PyDict.IsDictType(pyObj))
            {
                value = default;
                return false;
            }
            using (var pyDict = new PyDict(pyObj))
            {
                if (typeof(T).IsGenericType)
                {
                    value = pyDict.ToDictionary<T>();
                }
                else
                {
                    value = (T)pyDict.ToDictionary();
                }
                return true;
            }
        }

        bool IPyObjectDecoder.CanDecode(PyType objectType, Type targetType)
        {
            if (targetType.IsGenericType)
            {
                targetType = targetType.GetGenericTypeDefinition();
            }
            return decodableTypes.IndexOf(targetType) >= 0;
        }
    }
}
