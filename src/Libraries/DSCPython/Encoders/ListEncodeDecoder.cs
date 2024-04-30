using System;
using System.Collections;
using System.Collections.Generic;
using Dynamo.Utilities;
using Python.Runtime;

namespace DSCPython.Encoders
{
    internal class ListEncoderDecoder : IPyObjectEncoder, IPyObjectDecoder
    {
        private static readonly Type[] decodableTypes = new Type[]
        {
            typeof(IList), typeof(ArrayList),
            typeof(IList<>), typeof(List<>),
            typeof(IEnumerable), typeof(IEnumerable<>)
        };

        public bool CanEncode(Type type)
        {
            return typeof(IList).IsAssignableFrom(type);
        }

        public bool TryDecode<T>(PyObject pyObj, out T value)
        {
            if (!pyObj.IsIterable())
            {
                value = default;
                return false;
            }

            using (var pyList = PyList.AsList(pyObj))
            {
                if (typeof(T).IsGenericType)
                {
                    value = pyList.ToList<T>();
                }
                else
                {
                    value = (T)pyList.ToList();
                }
                return true;
            }
        }

        public PyObject TryEncode(object value)
        {
            // This is a no-op to prevent Python.NET from encoding generic lists
            // https://github.com/pythonnet/pythonnet/pull/963#issuecomment-642938541
            return PyObject.FromManagedObject(value);
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
