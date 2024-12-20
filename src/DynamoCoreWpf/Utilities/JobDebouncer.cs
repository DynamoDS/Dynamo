using System;

namespace Dynamo.Wpf.Utilities
{
    public static class JobDebouncer
    {
        public class DebounceQueueToken
        {
            public long LastExecutionId = 0;
            public SerialQueue SerialQueue = new();
        };
        /// <summary>
        /// Action <paramref name="job"/> is guaranteed to run at most once for every call, and exactly once after the last call.
        /// Execution is sequential, and optional jobs that share a <see cref="DebounceQueueToken"/> with a newer optional job will be ignored.
        /// </summary>
        /// <param name="job"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static void EnqueueOptionalJobAsync(Action job, DebounceQueueToken token)
        {
            lock (token)
            {
                token.LastExecutionId++;
                var myExecutionId = token.LastExecutionId;
                token.SerialQueue.DispatchAsync(() =>
                {
                    if (myExecutionId < token.LastExecutionId) return;
                    job();
                });
            }
        }
        public static void EnqueueMandatoryJobAsync(Action job, DebounceQueueToken token)
        {
            lock (token)
            {
                token.SerialQueue.DispatchAsync(() =>
                {
                    job();
                });
            }
        }
    }
}
