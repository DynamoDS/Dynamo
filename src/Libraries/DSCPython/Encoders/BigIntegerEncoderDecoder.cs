using System;
using System.Numerics;
using Python.Runtime;

namespace DSCPython.Encoders
{
    internal class BigIntegerEncoderDecoder : IPyObjectEncoder, IPyObjectDecoder
    {
        public bool CanEncode(Type type)
        {
            return type == typeof(BigInteger);
        }

        public bool TryDecode<T>(PyObject pyObj, out T value)
        {
            if (!PyInt.IsIntType(pyObj))
            {
                value = default;
                return false;
            }

            using (var pyLong = PyInt.AsInt(pyObj))
            {
                value = (T)(object)pyLong.ToBigInteger();
                return true;
            }
        }

        public PyObject TryEncode(object value)
        {
            return new PyInt(value.ToString());
        }

        bool IPyObjectDecoder.CanDecode(PyType objectType, Type targetType)
        {
            return targetType == typeof(BigInteger);
        }
    }
}
