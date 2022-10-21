using ProtoCore.DSASM;
using ProtoCore.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ProtoCore;
using ProtoFFI;
using Type = ProtoCore.Type;

namespace EmitMSIL
{
    public class BuiltIn
    {
        /// <summary>
        /// A Dictionary wrapper for MSIL engine outputs, any dynamic CLR stackvalues are unmarshaled before they are returned.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        [Obsolete("Internal, do not use")]
        public class MSILOutputMap<K, V> : IDictionary<K, V> where V : class
        {
            private Dictionary<K, V> backingDict = new Dictionary<K, V>();
            private MSILRuntimeCore runtimeCore;
            private FFIObjectMarshaler marshaler;

            public MSILOutputMap(MSILRuntimeCore runtimeCore)
            {
                this.runtimeCore = runtimeCore;
                marshaler = ProtoFFI.CLRDLLModule.GetMarshaler(runtimeCore);

            }

            public V this[K key]
            {
                get => Unmarshal(key);
                set => backingDict[key] = value;
            }
            /// <summary>
            /// A utility method for unmarshaling from replication wrappers to
            /// plain c# objects.
            /// </summary>
            /// <param name="clrwrapped"></param>
            /// <returns></returns>
            public object Unmarshal(CLRStackValue clrwrapped)
            {
                return marshaler.UnMarshal(clrwrapped, clrwrapped.CLRFEPReturnType, runtimeCore);
            }

            private V Unmarshal(K key)
            {
                //TODO consider scanning for nested wrappers we missed.
                if (backingDict[key] is CLRStackValue clrwrapped)
                {
                    return marshaler.UnMarshal(clrwrapped, clrwrapped.CLRFEPReturnType, runtimeCore) as V;
                }

                return backingDict[key];
            }

            public ICollection<K> Keys => backingDict.Keys;

            public ICollection<V> Values => backingDict.Keys.Select(x => Unmarshal(x)).ToArray();


            public int Count => backingDict.Count;

            public bool IsReadOnly => (backingDict as ICollection<KeyValuePair<K, V>>).IsReadOnly;

            public void Add(K key, V value)
            {
                backingDict.Add(key, value);
            }

            public void Add(KeyValuePair<K, V> item)
            {
                (backingDict as ICollection<KeyValuePair<K, V>>).Add(item);
            }

            public void Clear()
            {
                backingDict.Clear();
            }

            public bool Contains(KeyValuePair<K, V> item)
            {
                return (backingDict as ICollection<KeyValuePair<K, V>>).Contains(item);
            }

            public bool ContainsKey(K key)
            {
                return backingDict.ContainsKey(key);
            }

            public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
            {
                (backingDict as ICollection<KeyValuePair<K, V>>).CopyTo(array, arrayIndex);
            }

            public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
            {
                return backingDict.GetEnumerator();
            }

            public bool Remove(K key)
            {
                return backingDict.Remove(key);
            }

            public bool Remove(KeyValuePair<K, V> item)
            {
                return (backingDict as ICollection<KeyValuePair<K, V>>).Remove(item);
            }

            public bool TryGetValue(K key, out V value)
            {
                if (backingDict.TryGetValue(key, out _))
                {
                    value = Unmarshal(key);
                    return true;
                }

                value = null;
                return false;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return backingDict.GetEnumerator();
            }
        }
        private static object ToBoolean(object a)
        {
            switch (a)
            {
                case bool ab:
                    return ab;

                case long al:
                    return al != 0;

                case int ai:
                    return ai != 0;

                case null:
                    return false;

                case double ad:
                    return !Double.IsNaN(ad) && !ad.Equals(0.0);

                case string str:
                    return !string.IsNullOrEmpty(str);

                case char ac:
                    return ac != 0;
            }

            if (a is Pointer)
            {
                return true;
            }

            return null;
        }

        /// <summary>
        /// Handles possible overflows from a checked operation. If it works, its result is
        /// returned, otherwise the result of the unchecked operation is returned and a warning
        /// is logged.
        /// </summary>
        /// <param name="checkedOperation">Checked operation to be attempted first</param>
        /// <param name="uncheckedOperation">Unchecked operation to be perfomed when the checked operation overflowed</param>
        /// <returns>The result of the first succesful operation</returns>
        private static T HandleOverflow<T>(Func<T> checkedOperation, Func<T> uncheckedOperation)
        {
            try
            {
                return checkedOperation();
            }
            catch (OverflowException)
            {
//                runtimeCore.RuntimeStatus.LogWarning(WarningID.IntegerOverflow, string.Format($"{Resources.IntegerOverflow}href=IntegerOverflow.html"));
                return uncheckedOperation();
            }
        }


        public static object add(object a, object b)
        {
            if (a is double ad && b is double bd)
            {
                return ad + bd;
            }

            if (a is int ai && b is int bi)
            {
                return HandleOverflow(() => checked(ai + bi), () => ai + bi);
            }

            if ((a is long || a is int) && (b is long || b is int))
            {
                var al = Convert.ToInt64(a);
                var bl = Convert.ToInt64(b);
                return HandleOverflow(() => checked(al + bl), () => al + bl);
            }

            if ((a is long || a is double || a is int) && (b is long || b is double || b is int))
            {
                return Convert.ToDouble(a) + Convert.ToDouble(b);
            }

            if (a is string || b is string)
            {
                return a.ToString() + b.ToString();
            }

            return null;
        }

        public static object sub(object a, object b)
        {
            if (a is int ai && b is int bi)
            {
                return HandleOverflow(() => checked(ai - bi), () => ai - bi);
            }

            if ((a is long || a is int) && (b is long || b is int))
            {
                var al = Convert.ToInt64(a);
                var bl = Convert.ToInt64(b);
                return HandleOverflow(() => checked(al - bl), () => al - bl);
            }

            if ((a is long || a is double || a is int) && (b is long || b is double || b is int))
            {
                return Convert.ToDouble(a) - Convert.ToDouble(b);
            }

            return null;
        }

        public static object mul(object a, object b)
        {
            if (a is int ai && b is int bi)
            {
                return HandleOverflow(() => checked(ai * bi), () => ai * bi);
            }

            if ((a is long || a is int) && (b is long || b is int))
            {
                var al = Convert.ToInt64(a);
                var bl = Convert.ToInt64(b);
                return HandleOverflow(() => checked(al * bl), () => al * bl);
            }

            if ((a is long || a is double || a is int) && (b is long || b is double || b is int))
            {
                return Convert.ToDouble(a) * Convert.ToDouble(b);
            }

            return null;
        }

        public static object div(object a, object b)
        {
            //division is always carried out as a double
            if ((a is long || a is double || a is int) && (b is long || b is double || b is int))
            {
                return Convert.ToDouble(a) / Convert.ToDouble(b);
            }

            return null;
        }

        public static object mod(object a, object b)
        {
            if ((a is long || a is double || a is int) && (b is long || b is double || b is int))
            {
                if (a is int ri &&  b is int bi)
                {
                    var intMod = ri % bi;

                    // In order to follow the conventions of Java, Python, Scala and Google's calculator,
                    // the returned modulo will follow the sign of the divisor (not the dividend). 
                    if (intMod < 0 && bi > 0 || intMod > 0 && bi < 0)
                    {
                        intMod = intMod + bi;
                    }

                    return intMod;
                }

                if ((a is long || a is int) && (b is long || b is int))
                {
                    var bl = Convert.ToInt64(b);
                    var intMod = Convert.ToInt64(a) % bl;

                    if (intMod < 0 && bl > 0 || intMod > 0 && bl < 0)
                    {
                        intMod = intMod + bl;
                    }

                    return intMod;
                }

                var bd = Convert.ToDouble(b);
                var doubleMod = Convert.ToDouble(a) % bd;

                if (doubleMod < 0 && bd > 0 || doubleMod > 0 && bd < 0)
                {
                    doubleMod = doubleMod + bd;
                }

                return doubleMod;
            }

            return null;
        }

        public static object Neg(object a)
        {
            switch (a)
            {
                case int ai:
                    return HandleOverflow(() => checked(-ai), () => -ai);
                case long al:
                    return HandleOverflow(() => checked(-al), () => -al);
                case double ad:
                    return -ad;
            }
            return null;
        }

        public static object and(object a, object b)
        {
            var lhs = ToBoolean(a);
            var rhs = ToBoolean(b);

            if(lhs == null || rhs == null)
            {
                return null;
            }

            return (bool)lhs && (bool)rhs;
        }

        public static object or(object a, object b)
        {
            var lhs = ToBoolean(a);
            var rhs = ToBoolean(b);

            if (lhs == null || rhs == null)
            {
                return null;
            }

            return (bool)lhs || (bool)rhs;
        }

        public static object Not(object a)
        {
            var data = ToBoolean(a);
            if (data == null)
            {
                return null;
            }

            return !(bool)data;
        }

        public static object eq(object a, object b)
        {
            if (a is bool || b is bool)
            {
                var ab = ToBoolean(a);
                var bb = ToBoolean(b);

                if (ab == null || bb == null)
                {
                    return null;
                }

                return (bool)ab == (bool)bb;
            }

            if (a is double || b is double || a is long || b is long || a is int || b is int)
            {
                if (a is double || b is double)
                {
                    return MathUtils.Equals(Convert.ToDouble(a), Convert.ToDouble(b));
                }

                return Convert.ToInt64(a) == Convert.ToInt64(b);
            }

            if (a is string astr && b is string bstr)
            {
                return String.Compare(astr, bstr, StringComparison.InvariantCulture) == 0;
            }

            return a.Equals(b);
        }

        public static object nq(object a, object b)
        {
            if (a is bool || b is bool)
            {
                var ab = ToBoolean(a);
                var bb = ToBoolean(b);

                if (ab == null || bb == null)
                {
                    return null;
                }
 
                return (bool)ab != (bool)bb;
            }

            if (a is double || b is double || a is long || b is long || a is int || b is int)
            {
                if (a is double || b is double)
                {
                    return !MathUtils.Equals(Convert.ToDouble(a), Convert.ToDouble(b));
                }

                return Convert.ToInt64(a) != Convert.ToInt64(b);
            }

            if (a is string astr && b is string bstr)
            {
                return String.Compare(astr, bstr, StringComparison.InvariantCulture) != 0;
            }

            return !a.Equals(b);
        }

        public static object gt(object a, object b)
        {
            if (a is double || b is double || a is long || b is long || a is int || b is int)
            {
                return Convert.ToDouble(a) > Convert.ToDouble(b);
            }

            return null;
        }

        public static object lt(object a, object b)
        {
            if (a is double || b is double || a is long || b is long || a is int || b is int)
            {
                return Convert.ToDouble(a) < Convert.ToDouble(b);
            }

            return null;
        }

        public static object ge(object a, object b)
        {
            if (a is double || b is double || a is long || b is long || a is int || b is int)
            {
                if (a is double || b is double)
                {
                    return Convert.ToDouble(a) >= Convert.ToDouble(b);
                }
                return Convert.ToInt64(a) >= Convert.ToInt64(b);
            }

            return null;
        }

        public static object le(object a, object b)
        {
            if (a is double || b is double || a is long || b is long || a is int || b is int)
            {
                if (a is double || b is double)
                {
                    return Convert.ToDouble(a) <= Convert.ToDouble(b);
                }
                return Convert.ToInt64(a) <= Convert.ToInt64(b);
            }

            return null;
        }

        public static MethodInfo GetInternalMethod(string name)
        {
            string methodName = "";
            if (CoreUtils.TryGetOperator(name, out Operator op))
            {
                methodName = op.ToString();
            }
            else if (CoreUtils.TryGetUnaryOperator(name, out UnaryOperator un))
            {
                methodName = un.ToString();
            }

            if (methodName.Length > 0)
            {
                return typeof(BuiltIn)?.GetMethod(methodName);
            }

            throw new MissingMethodException("No matching operator method found");
        }
    }
}
