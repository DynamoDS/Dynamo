using System;
using System.Runtime.Serialization;
using System.Threading;

namespace DynamoServices
{
    /// <summary>
    /// This attribute can be applied to methods that register callsite with
    /// trace mechanism.
    /// </summary>
    public class RegisterForTraceAttribute : Attribute
    {
    }

    /// <summary>
    /// Utility class to Get/Set TraceData
    /// </summary>
    public static class TraceUtils
    {
        /// <summary>
        /// Returns the data that is bound to a particular key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static ISerializable GetTraceData(string key)
        {
            Object data = Thread.GetData(Thread.GetNamedDataSlot(key));

            //Null is ok
            if (data == null)
                return null;

            var ret = data as ISerializable;
            if (ret != null)
                return ret;
       
            //Data, that was not serializable is not
            throw new InvalidOperationException("Data in Named slot was not serializable");
        }

        /// <summary>
        /// Set the data bound to a particular key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetTraceData(string key, ISerializable value)
        {
            Thread.SetData(Thread.GetNamedDataSlot(key), value);
        }
    }
}
