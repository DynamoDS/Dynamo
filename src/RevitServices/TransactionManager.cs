using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;

namespace RevitServices
{
    /// <summary>
    /// Wraps Revit Transaction methods and provides events for transaction initialization
    /// and completion.
    /// </summary>
    public class TransactionManager
    {
        private Transaction _trans;
        private readonly WarningHandler _handler;

        public TransactionManager()
        {
            _handler = new WarningHandler(this);
        }

        /// <summary>
        /// Event for handling failure messages from Revit.
        /// </summary>
        public event FailureDelegate FailuresRaised;

        /// <summary>
        /// Called when the managed Transaction is started.
        /// </summary>
        public event Action TransactionStarted;

        /// <summary>
        /// Called when the managed Transaction is committed.
        /// </summary>
        public event Action TransactionCommitted;

        /// <summary>
        /// Called when the managed Transaction is cancelled.
        /// </summary>
        public event Action TransactionCancelled;

        #region Event Raising Utility Methods
        private void RaiseTransactionStarted()
        {
            if (TransactionStarted != null)
                TransactionStarted();
        }

        private void RaiseTransactionCommitted()
        {
            if (TransactionCommitted != null)
                TransactionCommitted();
        }

        private void RaiseTransactionCancelled()
        {
            if (TransactionCancelled != null)
                TransactionCancelled();
        }

        private void ProcessFailures(FailuresAccessor failures)
        {
            if (FailuresRaised == null)
                return;

            FailuresRaised(failures);
        }
        #endregion

        /// <summary>
        /// Starts a RevitAPI Transaction that will be managed by this TransactionManager instance.
        /// </summary>
        /// <param name="document">Document (DB) to start a Transaction for.</param>
        public void StartTransaction(Document document)
        {
            if (_trans == null || _trans.GetStatus() != TransactionStatus.Started)
            {
                _trans = new Transaction(document, "Dynamo Script");
                _trans.Start();

                FailureHandlingOptions failOpt = _trans.GetFailureHandlingOptions();
                failOpt.SetFailuresPreprocessor(_handler);
                _trans.SetFailureHandlingOptions(failOpt);

                RaiseTransactionStarted();
            }
        }

        /// <summary>
        /// Commits the managed Transaction to the Revit DB.
        /// </summary>
        public void CommitTransaction()
        {
            if (_trans != null)
            {
                if (_trans.GetStatus() == TransactionStatus.Started)
                {
                    _trans.Commit();
                    RaiseTransactionCommitted();
                }
                //_trans = null;
            }
        }

        /// <summary>
        /// Cancels the managed Transaction, rolling-back any non-committed changes.
        /// </summary>
        public void CancelTransaction()
        {
            if (_trans != null)
            {
                if (_trans.GetStatus() == TransactionStatus.Started)
                {
                    _trans.RollBack();
                    RaiseTransactionCancelled();
                }
                //_trans = null;
            }
        }

        /// <summary>
        /// Is there a currently active Transaction?
        /// </summary>
        public bool TransactionActive
        {
            get
            {
                return _trans != null 
                    && _trans.GetStatus() == TransactionStatus.Started;
            }
        }

        /// <summary>
        /// Status of the managed Transaction.
        /// </summary>
        public TransactionStatus TransactionStatus
        {
            get
            {
                if (_trans == null)
                    return TransactionStatus.Uninitialized;
                return _trans.GetStatus();
            }
        }

        #region Failures Preprocessor
        private class WarningHandler : IFailuresPreprocessor
        {
            private readonly TransactionManager _tm;

            internal WarningHandler(TransactionManager transactionManager)
            {
                _tm = transactionManager;
            }

            public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
            {
                _tm.ProcessFailures(failuresAccessor);

                return FailureProcessingResult.Continue;
            }
        }
        #endregion
    }

    /// <summary>
    /// Callback signature for processing failure messages from Revit.
    /// </summary>
    /// <param name="failures">
    /// FailuresAccessor from Revit, containing all emmitted failures.
    /// </param>
    public delegate void FailureDelegate(FailuresAccessor failures);
}
