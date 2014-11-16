using System;
using System.Threading;

using Dynamo.Core.Threading;
using Dynamo.Models;

namespace Dynamo.Core
{
    class PulseMaker
    {
        private readonly Timer internalTimer;
        private readonly DynamoModel dynamoModel;

        // PulseMaker internal state management objects.
        private readonly object stateMutex;
        private bool evaluationInProgress = false;
        private bool evaluationRequestPending = false;

        internal PulseMaker(DynamoModel dynamoModel)
        {
            this.dynamoModel = dynamoModel;

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
                evaluationRequestPending = false;
                internalTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

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
                BeginRunExpression();
            }
        }

        private void OnRunExpressionCompleted(AsyncTask task)
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
                BeginRunExpression();
            }
        }

        private void BeginRunExpression()
        {
            evaluationInProgress = true;
            dynamoModel.OnRequestDispatcherBeginInvoke(() =>
            {
                dynamoModel.RunExpression(OnRunExpressionCompleted);
            });            
        }

        #endregion
    }
}
