using System;
using System.Linq;
using System.Threading;

using Dynamo.Models;

namespace Dynamo.Core
{
    public class PulseMaker
    {
        #region Class Data Members and Properties

        private readonly Timer internalTimer;
        private readonly object stateMutex;
        private bool evaluationInProgress = false;
        private bool evaluationRequestPending = false;
        private HomeWorkspaceModel workspace;
        internal int TimerPeriod { get; private set; }

        #endregion

        #region Public Class Operational Methods

        /// <summary>
        /// An internal constructor to ensure PulseMaker object can only
        /// be instantiated from within DynamoModel (i.e. DynamoCore.dll).
        /// </summary>
        /// <param name="dynamoModel">The owning DynamoModel object.</param>
        /// 
        internal PulseMaker(HomeWorkspaceModel workspace)
        {
            workspace.EvaluationCompleted += OnRunExpressionCompleted;
            this.workspace = workspace;
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
                BeginRunExpression();
            }
        }

        private void OnRunExpressionCompleted(object sender,
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
                BeginRunExpression();
            }
        }

        private void BeginRunExpression()
        {
            // Here we know for a fact that the evaluation will begin at one 
            // point in the near future. Mark it as in progress because from 
            // this point till the evaluation takes place, the timer should 
            // not cause another evaluation to be scheduled.
            // 
            evaluationInProgress = true;

            DynamoModel.OnRequestDispatcherBeginInvoke(() =>
            {
                // Dirty selective nodes so they get included for evaluation.
                var nodesToUpdate = workspace.Nodes.Where(n => n.EnablePeriodicUpdate);
                foreach (var nodeToUpdate in nodesToUpdate)
                {
                    nodeToUpdate.MarkNodeAsModified(true);
                }

                workspace.OnNodesModified();
            });
        }

        #endregion
    }
}
