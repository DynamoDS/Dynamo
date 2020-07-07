using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Utilities;
using Python.Runtime;
using DSDictionary = DesignScript.Builtin.Dictionary;

namespace DSCPython.Encoders
{
    internal class DictionaryDecoder : IPyObjectDecoder
    {
        private static readonly Type[] decodableTypes = new Type[]
        {
            typeof(IDictionary<,>), typeof(Dictionary<,>),
            typeof(IDictionary), typeof(Hashtable),
            typeof(DSDictionary)
        };

        public bool CanDecode(PyObject objectType, Type targetType)
        {
            if (targetType.IsGenericType)
            {
                targetType = targetType.GetGenericTypeDefinition();
            }
            return decodableTypes.IndexOf(targetType) >= 0;
        }

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
                    var dictionary = pyDict.ToDictionary();
                    if (typeof(T) == typeof(DSDictionary))
                    {
                        value = (T)(object)DSDictionary.ByKeysValues(
                            new List<string>(dictionary.Keys.Cast<string>()),
                            new List<object>(dictionary.Values.Cast<object>()));
                    }
                    else
                    {
                        value = (T)dictionary;
                    }
                }
                return true;
            }
        }
    }
}
