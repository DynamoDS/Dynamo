using System;

namespace DynamoUtilities.ExceptionHelpers
{

    /// <summary>
    /// Exception class thats useful for sending aggregated exceptions and stacktraces to analytics.
    /// </summary>
    [Obsolete("For internal use only")]
    public class DynamoWrappedException : Exception
    {
        private string newStackTrace;
        private Exception wrappedException;

        public DynamoWrappedException(Exception e, string stackTrace) : base(e.Message)
        {
            this.newStackTrace = stackTrace;
        }


        public override string StackTrace
        {
            get
            {
                return newStackTrace;
            }
        }
        //maybe a horrible idea.
        public new Type GetType()
        {
            return wrappedException.GetType();
        }
    }
}
