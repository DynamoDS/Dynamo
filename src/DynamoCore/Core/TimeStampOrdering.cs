using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Interfaces;

namespace Dynamo.Core
{
    /// <summary>
    /// Time marker for handling total-ordering requirements within Dynamo
    /// </summary>
    public struct TimeStamp
    {

        /// <summary>
        /// Time stamp 
        /// </summary>
        internal readonly long Id;
        
        /// <summary>
        /// User friendly time stamp for debugging use only
        /// </summary>
        public readonly DateTime ApproxTime;

        internal TimeStamp(long newId)
        {
            this.ApproxTime = DateTime.Now;
            this.Id = newId;
        }

        public override string ToString()
        {
            return String.Format("Time: {0} ID: {1}", ApproxTime, Id);
        }
        

        /// <summary>
        /// The public usage of time stamps should be restricted to these
        /// methods used to ensure an ordering on timestamps
        /// </summary>
        /// <param name="o1"></param>
        /// <param name="o2"></param>
        /// <returns></returns>
        public static bool operator >(TimeStamp o1, TimeStamp o2)
        {
            return o1.Id > o2.Id;
        }

        public static bool operator <(TimeStamp o1, TimeStamp o2)
        {
            return o1.Id < o2.Id;
        }

        /// <summary>
        /// Get the first time stamp in the usage
        /// </summary>
        internal static TimeStamp First
        {
            get
            {
                return new TimeStamp(0);
            }
        }

        /// <summary>
        /// Get the next time stamp from this one, this doesn't register it with the
        /// time stamp manager and should in general only be called by the TSManager
        /// It's use is not thread safe
        /// </summary>
        /// <returns></returns>
        internal TimeStamp Next()
        {
            //Throw an exception if we're off the end of number space
            checked
            {
                return new TimeStamp(Id + 1);
            }




        }



    }

    /// <summary>
    /// Class for handling global ordering of events
    /// </summary>
    class TSOManager : ICleanup
    {
        private static TSOManager instance;
        private readonly static Object singletonProtectionMutex = new object();
        private readonly Object tsoMutex = new Object();

        private TimeStamp currentTime;

        private TSOManager()
        {
            currentTime = TimeStamp.First;
        }

        public static TSOManager GetInstance()
        {
            lock (singletonProtectionMutex)
            {
                if (instance == null)
                    instance = new TSOManager();

                return instance;
            }
        }

        /// <summary>
        /// Peek at the current time stamp
        /// </summary>
        public TimeStamp CurrentTime { get
        {
            lock (tsoMutex)
            {
                return currentTime;
            }
        }}

        /// <summary>
        /// Get the current time stamp and increment the
        /// time representation. Use this method if you wish
        /// to do something with the current timestamp
        /// </summary>
        /// <returns></returns>
        public TimeStamp AllocateTimeStamp()
        {
            lock (tsoMutex)
            {
                TimeStamp ret = currentTime;
                currentTime = currentTime.Next();
                return ret;
            }
            
        }

        public void Cleanup()
        {
            lock (singletonProtectionMutex)
            {
                instance = null;
            }
        }
    }
}
