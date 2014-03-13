using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace DSNodeServices
{
    public static class TraceUtils
    {
        /// <summary>
        /// Get the data that is bound to a particular key
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
