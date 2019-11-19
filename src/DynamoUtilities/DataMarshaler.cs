using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Dynamo.Utilities
{
    /// <summary>
    ///     Provides ability to register data marshalers which can then be used to marshal arbitrary data.
    /// </summary>
    public class DataMarshaler
    {
        private readonly Dictionary<Type, Converter<object, object>> marshalers =
            new Dictionary<Type, Converter<object, object>>();

        private readonly Dictionary<Type, Converter<object, object>> cache =
            new Dictionary<Type, Converter<object, object>>();

        public DataMarshaler()
        {
            RegisterMarshaler((IList e) => e.Cast<object>().Select(Marshal).ToList());
            RegisterMarshaler(
                (IDictionary dict) =>
                {
                    // Dictionary<TKey, TValue> and IronPython.Runtime.PythonDictionary both implement IDictionary
                    return dict.Keys.Cast<object>().ToDictionary(key => Marshal(key), key => Marshal(dict[key]));
                });
            RegisterMarshaler(
                (BigInteger bigInt) =>
                {
                    long int64;
                    try
                    {
                        int64 = (long)bigInt;
                    }
                    catch (OverflowException)
                    {
                        return bigInt;
                    }
                    return int64;
                });
        }

        /// <summary>
        ///     Registers a new data marshaler for a given type.
        /// </summary>
        /// <typeparam name="T">Type to marshal.</typeparam>
        /// <param name="marshaler">Converter to be used to marshaling.</param>
        public void RegisterMarshaler<T>(Converter<T, object> marshaler)
        {
            RegisterMarshaler(typeof(T), x => marshaler((T)x));
        }

        /// <summary>
        ///     Registers a new data marshaler for a given type.
        /// </summary>
        /// <param name="t">Type to marshal.</param>
        /// <param name="marshaler">Converter to be used to marshaling.</param>
        public void RegisterMarshaler(Type t, Converter<object, object> marshaler)
        {
            marshalers[t] = marshaler;
            cache.Clear();
        }

        /// <summary>
        ///     Unregisters a data marshaler for a given type.
        /// </summary>
        /// <typeparam name="T">Type of data to unregister marshaling for.</typeparam>
        public void UnregisterMarshalerOfType<T>()
        {
            UnregisterMarshalerOfType(typeof(T));
        }

        /// <summary>
        ///     Unregisters a data marshaler for a given type.
        /// </summary>
        /// <param name="t">Type of data to unregister marshaling for.</param>
        public void UnregisterMarshalerOfType(Type t)
        {
            marshalers.Remove(t);
            cache.Clear();
        }

        /// <summary>
        ///     Marshals data using the registered marshalers. If no marshaler exists, data is returned unmodified.
        /// </summary>
        /// <param name="obj">Data to marshal.</param>
        public object Marshal(object obj)
        {
            if (obj == null)
                return null;

            var targetType = obj.GetType();

            Converter<object, object> marshaler;
            if (marshalers.TryGetValue(targetType, out marshaler) || cache.TryGetValue(targetType, out marshaler))
                return marshaler(obj);

            // Only deal with marshalers that can operator on the target type
            var applicable = marshalers.Where(pair => pair.Key.IsAssignableFrom(targetType));

            if (!applicable.Any())
            {
                return obj;
            }

            // Find the marshaler that operates on the closest base type of the target type.
            var dispatchedMarshaler =
                applicable.Aggregate(
                    // always keep the more specific marshaler
                    (current, next) => current.Key.IsAssignableFrom(next.Key) ? next : current);

            // Cache the marshaler so we don't keep doing this lookup
            cache[targetType] = dispatchedMarshaler.Value;

            return dispatchedMarshaler.Value(obj);
        }
    }
}
