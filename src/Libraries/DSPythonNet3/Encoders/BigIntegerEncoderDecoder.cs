using System;
using System.Numerics;
using Python.Runtime;

namespace DSPythonNet3.Encoders
{
    internal class BigIntegerEncoderDecoder : IPyObjectEncoder, IPyObjectDecoder
    {
        public bool CanDecode(PyType objectType, Type targetType)
        {
            return targetType == typeof(BigInteger);
        }

        public bool CanEncode(Type type)
        {
            return type == typeof(BigInteger);
        }

        public bool TryDecode<T>(PyObject pyObj, out T value)
        {
            using (Py.GIL())
            using (var builtins = Py.Import("builtins"))
            {
                var pyIntType = builtins.GetAttr("int");
                if (!pyObj.IsInstance(pyIntType))
                {
                    value = default;
                    return false;
                }

                string s = pyObj.ToString();
                BigInteger bi = BigInteger.Parse(s);

                value = (T)(object)bi;
                return true;
            }
        }

        public PyObject TryEncode(object value)
        {
            using (Py.GIL())
            using (var builtins = Py.Import("builtins"))
            {
                string s = ((BigInteger)value).ToString();
                using (var pyStr = new PyString(s))
                {
                    return builtins.InvokeMethod("int", pyStr);
                }
            }
        }
    }
}
