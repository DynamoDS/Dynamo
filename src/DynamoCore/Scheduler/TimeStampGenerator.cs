using System.Diagnostics;
using System.Threading;

namespace Dynamo.Scheduler
{
    /// <summary>
    /// This class is used to set creation time of async task in Dynamo Scheduler.
    /// </summary>
    public struct TimeStamp
    {
        #region Private Class Data Members

        /// <summary>
        /// The unique identifier of the time-stamp. This identifier is 
        /// generated from a centralized TimeStampGenerator class that ensures 
        /// the identifier is unique for each generated TimeStamp object.
        /// </summary>
        private readonly long identifier;

        /// <summary>
        /// The high resolution tick count obtained from Stopwatch.GetTimestamp.
        /// This tick count is different from 'identifier' in the way that this 
        /// represents real elapsed 
        /// </summary>
        private readonly long tickCount;

        #endregion

        #region Public Struct Methods/Properties

        internal TimeStamp(long identifier)
        {
            this.tickCount = Stopwatch.GetTimestamp();
            this.identifier = identifier;
        }

        internal long Identifier
        {
            get { return identifier; }
        }

        internal long TickCount
        {
            get { return tickCount; }
        }

        #endregion

        #region Public Overloaded Operators

        public override bool Equals(object other)
        {
            if (!(other is TimeStamp))
                return false;

            var t = ((TimeStamp)other);
            return (identifier == t.identifier && (tickCount == t.tickCount));
        }

        public override int GetHashCode()
        {
            return identifier.GetHashCode() ^ tickCount.GetHashCode();
        }

        /// <summary>
        /// The public usage of time stamps should be restricted to these
        /// methods which are used to ensure an ordering on timestamps
        /// </summary>
        /// <param name="timeStamp0">The first time stamp in comparison.</param>
        /// <param name="timeStamp1">The second time stamp in comparison.</param>
        /// <returns>Return true if the first time stamp was created later than 
        /// the second time stamp, or false otherwise.</returns>
        /// 
        public static bool operator >(TimeStamp timeStamp0, TimeStamp timeStamp1)
        {
            return timeStamp0.identifier > timeStamp1.identifier;
        }

        /// <summary>
        /// The public usage of time stamps should be restricted to these
        /// methods which are used to ensure an ordering on timestamps
        /// </summary>
        /// <param name="timeStamp0">The first time stamp in comparison.</param>
        /// <param name="timeStamp1">The second time stamp in comparison.</param>
        /// <returns>Return true if the first time stamp was created earlier than 
        /// the second time stamp, or false otherwise.</returns>
        /// 
        public static bool operator <(TimeStamp timeStamp0, TimeStamp timeStamp1)
        {
            return timeStamp0.identifier < timeStamp1.identifier;
        }

        #endregion
    }

    sealed class TimeStampGenerator
    {
        private long timeStampValue = 1023;

        internal TimeStamp Next
        {
            get
            {
                return new TimeStamp(Interlocked.Increment(ref timeStampValue));
            }
        }
    }
}
