using System;
using System.Threading.Tasks;

namespace Dynamo.Wpf.Utilities
{
    public static class JobDebouncer
    {
        public class DebounceQueueToken
        {
            public bool IsDirty = false;
            public readonly object JobLock = new();
        };
        /// <summary>
        /// Action <paramref name="job"/> is guaranteed to run at most once for every call, and exactly once after the last call.
        /// Execution is sequential, and jobs that share a <see cref="DebounceQueueToken"/> with a newer job will be ignored.
        /// </summary>
        /// <param name="job"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Task EnqueueJobAsync(Action job, DebounceQueueToken token)
        {
            token.IsDirty = true;
            return Task.Run(() =>
            {
                lock (token.JobLock)
                {
                    while (token.IsDirty)
                    {
                        token.IsDirty = false;
                        job();
                    }
                }
            });
        }
    }
}
