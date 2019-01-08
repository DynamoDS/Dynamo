using System;
using System.Threading;

using Dynamo.Models;

namespace Dynamo.Core
{
    internal class PulseMaker
    {
        #region Class Data Members and Properties

        private readonly Timer internalTimer;
        private readonly object stateMutex;
        private bool evaluationInProgress;
        private bool evaluationRequestPending;
        internal int TimerPeriod { get; private set; }

        public event Action RunStarted;
        protected virtual void OnRunStarted()
        {
            var handler = RunStarted;
            if (handler != null) handler();
        }

        #endregion

        #region Public Class Operational Methods

        /// <summary>
        /// An internal constructor to ensure PulseMaker object can only
        /// be instantiated from within DynamoModel (i.e. DynamoCore.dll).
        /// </summary>
        /// 
        internal PulseMaker()
        {
            stateMutex = new object();
            internalTimer = new Timer(OnTimerTicked);
        }

        /// <summary>
        /// Call this method to start the PulseMaker.
        /// </summary>
        /// <param name="milliseconds">The interval between two pulses in 
        /// milliseconds.</param>
        /// 
        internal void Start(int milliseconds)
        {
            if (milliseconds <= 0)
                throw new ArgumentOutOfRangeException("milliseconds");

            TimerPeriod = milliseconds;
            internalTimer.Change(0, milliseconds);
        }

        /// <summary>
        /// Call this method to stop the PulseMaker. Note that any existing 
        /// scheduled evaluation will still proceed, but this call guarantees 
        /// that no further evaluation will take place after that.
        /// </summary>
        /// 
        internal void Stop()
        {
            lock (stateMutex)
            {
                TimerPeriod = 0;
                evaluationRequestPending = false;
                evaluationInProgress = false;
                internalTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        #endregion

        #region Time Critical Methods

        private void OnTimerTicked(object state)
        {
            lock (stateMutex)
            {
                // If there is an on-going evaluation, then mark as request 
                // pending. When the evaluation is completed, PulseMaker will
                // immediately schedules another evaluation if there is any 
                // pending evaluation request.
                // 
                if (evaluationInProgress)
                {
                    evaluationRequestPending = true;
                    return;
                }

                // When it gets here, an evaluation is guaranteed.
                BeginRun();
            }
        }

        internal void OnRefreshCompleted(object sender,
            EvaluationCompletedEventArgs evaluationCompletedEventArgs)
        {
            lock (stateMutex)
            {
                // Mark evaluation as being done.
                evaluationInProgress = false;

                // If there is no pending request to perform another round of 
                // evaluation (i.e. the timer has not ticked while evaluation 
                // was taking place), then bail.
                // 
                if (!evaluationRequestPending)
                    return;

                // Further evaluation was requested.
                evaluationRequestPending = false;
                BeginRun();
            }
        }

        private void BeginRun()
        {
            // Here we know for a fact that the evaluation will begin at one 
            // point in the near future. Mark it as in progress because from 
            // this point till the evaluation takes place, the timer should 
            // not cause another evaluation to be scheduled.
            // 
            evaluationInProgress = true;

            OnRunStarted();
        }

        #endregion
    }
}
