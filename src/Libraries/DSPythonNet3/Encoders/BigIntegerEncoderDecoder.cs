using System;
using System.Numerics;
using Python.Runtime;

namespace DSPythonNet3.Encoders
{
    //internal class BigIntegerEncoderDecoder : IPyObjectEncoder, IPyObjectDecoder
    //{
    //    public bool CanDecode(PyType objectType, Type targetType)
    //    {
    //        return targetType == typeof(BigInteger);
    //    }

    //    public bool CanEncode(Type type)
    //    {
    //        return type == typeof(BigInteger);
    //    }

    //    public bool TryDecode<T>(PyObject pyObj, out T value)
    //    {
    //        if (!PyLong.IsLongType(pyObj))
    //        {
    //            value = default;
    //            return false;
    //        }

    //        using (var pyLong = PyLong.AsLong(pyObj))
    //        {
    //            value = (T)(object)pyLong.ToBigInteger();
    //            return true;
    //        }
    //    }

    //    public PyObject TryEncode(object value)
    //    {
    //        return new PyLong(value.ToString());
    //    }
    //}
}
