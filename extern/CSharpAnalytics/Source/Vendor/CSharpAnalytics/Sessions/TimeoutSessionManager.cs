using System;

namespace CSharpAnalytics.Sessions
{
    public class TimeoutSessionManager : SessionManager
    {
        private readonly TimeSpan timeout;

        public TimeoutSessionManager(SessionState sessionState, TimeSpan timeout)
            : base(sessionState)
        {
            this.timeout = timeout;
        }

        /// <summary>
        /// How long before a session will expire if no activity is seen.
        /// </summary>
        public TimeSpan Timeout { get { return timeout; } }

        /// <summary>
        /// Calculate the elapsed time since the last activity.
        /// </summary>
        /// <param name="nextActivityTime">Next activity start time.</param>
        /// <returns>Elapsed time since the last activity.</returns>
        private TimeSpan TimeSinceLastActivity(DateTimeOffset nextActivityTime)
        {
            return nextActivityTime - lastActivityAt;
        }

        internal override void Hit()
        {
            StartNewSessionIfTimedOut(DateTimeOffset.Now);
            base.Hit();
        }

        /// <summary>
        /// Starts are new session if the previous one has expired.
        /// </summary>
        /// <param name="activityStartedAt">When this hit activity started.</param>
        private void StartNewSessionIfTimedOut(DateTimeOffset activityStartedAt)
        {
            // Two threads could trigger activities back to back after a session ends, e.g. restarting the app
            // after some time spent suspended.  Only let one of them cause a new session to be started.
            while (TimeSinceLastActivity(activityStartedAt) > timeout)
            {
                lock (newSessionLock)
                {
                    if (TimeSinceLastActivity(activityStartedAt) > timeout)
                        StartNewSession();
                    lastActivityAt = activityStartedAt;
                }
            }
        }
    }
}
