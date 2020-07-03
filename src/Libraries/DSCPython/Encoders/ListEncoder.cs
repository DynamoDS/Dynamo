using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Utilities;
using Python.Runtime;

namespace DSCPython.Encoders
{
    internal class ListEncoder : IPyObjectEncoder, IPyObjectDecoder
    {
        private static readonly Type[] supportedTypes = new Type[]
        {
            typeof(IList), typeof(ArrayList),
            typeof(IList<>), typeof(List<>)
        };

        public bool CanDecode(PyObject objectType, Type targetType)
        {
            return supportedTypes.IndexOf(targetType) >= 0;
        }

        public bool CanEncode(Type type)
        {
            return false;
        }

        public bool TryDecode<T>(PyObject pyObj, out T value)
        {
            if (!PySequence.IsSequenceType(pyObj))
            {
                value = default;
                return false;
            }

            using (var pyList = PyList.AsList(pyObj))
            {
                if (typeof(T).IsGenericType)
                {
                    value = (T)pyList.ToList<T>();
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
            throw new NotImplementedException();
        }
    }
}
