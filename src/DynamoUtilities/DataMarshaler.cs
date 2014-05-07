using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynamo.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public class DataMarshaler
    {
        private readonly Dictionary<Type, Converter<object, object>> marshalers =
            new Dictionary<Type, Converter<object, object>>(); 

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="marshaler"></param>
        public void RegisterMarshaler<T>(Converter<T, object> marshaler)
        {
            var dynamicWrapped = new Converter<object, object>(x => marshaler((T)x));
            marshalers[typeof(T)] = dynamicWrapped;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void UnregisterMarshalerOfType<T>()
        {
            UnregisterMarshalerOfType(typeof(T));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        public void UnregisterMarshalerOfType(Type t)
        {
            marshalers.Remove(t);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public object Marshal(object obj)
        {
            if (obj == null) return null;

            if (obj is IDictionary<object, object>)
                return (obj as IDictionary<object, object>).Keys.Select(Marshal);

            if (obj is IEnumerable<object>)
                return (obj as IEnumerable<object>).Select(Marshal);

            Converter<object, object> marshaler;
            return marshalers.TryGetValue(obj.GetType(), out marshaler) ? marshaler(obj) : obj;
        }
    }
}
