using System;

namespace Dynamo.Wpf.Utilities
{
    /// <summary>
    /// Thread-safe utility class to enqueue jobs into a serial queue of tasks executed in a background thread.
    /// No debouncing delay by executing jobs as soon as possible.
    /// At any moment, any optional job has to wait for at most one pending optional job to finish.
    /// </summary>
    internal static class JobDebouncer
    {
        internal class DebounceQueueToken
        {
            public long LastOptionalExecutionId = 0;
            public SerialQueue SerialQueue = new();
        };
        /// <summary>
        /// Action <paramref name="job"/> is guaranteed to run at most once for every call, and exactly once after the last call.
        /// Execution is sequential, and optional jobs that share a <see cref="DebounceQueueToken"/> with a newer optional job will be ignored.
        /// </summary>
        /// <param name="job"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        internal static void EnqueueOptionalJobAsync(Action job, DebounceQueueToken token)
        {
            lock (token)
            {
                token.LastOptionalExecutionId++;
                var myExecutionId = token.LastOptionalExecutionId;
                token.SerialQueue.DispatchAsync(() =>
                {
                    if (myExecutionId < token.LastOptionalExecutionId) return;
                    job();
                });
            }
        }
        internal static void EnqueueMandatoryJobAsync(Action job, DebounceQueueToken token)
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
