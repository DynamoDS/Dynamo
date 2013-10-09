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
        private Transaction Transaction { get; set; }
        private readonly WarningHandler _handler;
        private readonly TransactionHandle _handle;

        public TransactionManager()
        {
            _handler = new WarningHandler(this);
            _handle = new TransactionHandle(this);
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
        public TransactionHandle StartTransaction(Document document)
        {
            if (Transaction == null || Transaction.GetStatus() != TransactionStatus.Started)
            {
                Transaction = new Transaction(document, "Dynamo Script");
                Transaction.Start();

                FailureHandlingOptions failOpt = Transaction.GetFailureHandlingOptions();
                failOpt.SetFailuresPreprocessor(_handler);
                Transaction.SetFailureHandlingOptions(failOpt);

                RaiseTransactionStarted();
            }
            return _handle;
        }

        /// <summary>
        /// Is there a currently active Transaction?
        /// </summary>
        public bool TransactionActive
        {
            get
            {
                return Transaction != null 
                    && Transaction.GetStatus() == TransactionStatus.Started;
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

        public class TransactionHandle
        {
            private readonly TransactionManager _manager;

            internal TransactionHandle(TransactionManager t)
            {
                _manager = t;
            }

            /// <summary>
            /// Commits the managed Transaction to the Revit DB.
            /// </summary>
            public TransactionStatus CommitTransaction()
            {
                if (_manager != null)
                {
                    if (_manager.Transaction.GetStatus() == TransactionStatus.Started)
                    {
                        var result = _manager.Transaction.Commit();
                        _manager.RaiseTransactionCommitted();

                        return result;
                    }
                }
                throw new InvalidOperationException("Cannot commit a transaction that isn't active.");
            }

            /// <summary>
            /// Cancels the managed Transaction, rolling-back any non-committed changes.
            /// </summary>
            public TransactionStatus CancelTransaction()
            {
                if (_manager != null)
                {
                    if (_manager.Transaction.GetStatus() == TransactionStatus.Started)
                    {
                        var result = _manager.Transaction.RollBack();
                        _manager.RaiseTransactionCancelled();

                        return result;
                    }
                }
                throw new InvalidOperationException("Cannot cancel a transaction that isn't active.");
            }

            /// <summary>
            /// Status of the managed Transaction.
            /// </summary>
            public TransactionStatus Status
            {
                get
                {
                    if (_manager.Transaction == null)
                        return TransactionStatus.Uninitialized;
                    return _manager.Transaction.GetStatus();
                }
            }
        }
    }

    /// <summary>
    /// Callback signature for processing failure messages from Revit.
    /// </summary>
    /// <param name="failures">
    /// FailuresAccessor from Revit, containing all emmitted failures.
    /// </param>
    public delegate void FailureDelegate(FailuresAccessor failures);
}
