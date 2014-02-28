using System;
using Autodesk.Revit.DB;

namespace RevitServices.Transactions
{
    /// <summary>
    /// Transaction helper for nodes
    /// </summary>
    public class TransactionManager : ITransactionStrategy
    {
        private static TransactionManager manager;
        private static Object mutex = new Object();
        internal ITransactionStrategy strategy;               

        /// <summary>
        /// Setup a manager with a default strategy
        /// </summary>
        public static void SetupManager()
        {
            manager = new TransactionManager();
        }

        /// <summary>
        /// Setup a manager with a specified strategy
        /// </summary>
        public static void SetupManager(ITransactionStrategy strategy)
        {
            manager = new TransactionManager(strategy);
        }


        /// <summary>
        /// Get the current transaction manager
        /// </summary>
        /// <returns></returns>
        public static TransactionManager GetInstance()
        {
            if (manager == null)
            {
                throw new InvalidOperationException("TransactionManager must be setup before use. Call SetupManager with a choice of strategy");
            }


            return manager;
        }


        /// <summary>
        /// Construct a new manager with a default transaction strategy
        /// </summary>
        private TransactionManager()
        {
            this.strategy = new DebugTransactionStrategy();
        }


        /// <summary>
        /// Construct a new manager with a given strategy
        /// </summary>
        /// <param name="strategy"></param>
        private TransactionManager(ITransactionStrategy strategy)
        {
            if (strategy == null)
                throw new ArgumentNullException("strategy", 
                    "Strategy must not be null, to use a default call the overload with no arguments");
            this.strategy = strategy;
        }




        /// <summary>
        /// Ensure that there is a valid transaction active
        /// </summary>
        public void EnsureInTransaction(Document document)
        {
            //Hand off the behaviour to the strategy
            strategy.EnsureInTransaction(document);
        }

        /// <summary>
        /// Notify that the transaction system that the operations
        /// requiring a transaction are complete
        /// </summary>
        public void TransactionTaskDone()
        {
            //Hand off the behaviour to the strategy
            strategy.TransactionTaskDone();
        }


        /// <summary>
        /// Require that the current transaction is recycled
        /// </summary>
        public void ForceCloseTransaction()
        {
            //Hand off the behaviour to the strategy
            strategy.ForceCloseTransaction();
            
        }


    }


    public interface ITransactionStrategy
    {
        /// <summary>
        /// Ensure that there is a valid transaction active
        /// </summary>
        void EnsureInTransaction(Document document);

        /// <summary>
        /// Notify that the transaction system that the operations
        /// requiring a transaction are complete
        /// </summary>
        void TransactionTaskDone();

        /// <summary>
        /// Require that the current transaction is recycled
        /// </summary>
        void ForceCloseTransaction();
    }


    /// <summary>
    /// Basic handling transaction handling strategy that opens
    /// a new transaction for every operation
    /// </summary>
    public class DebugTransactionStrategy : ITransactionStrategy
    {
        private readonly TransactionWrapper wrapper = new TransactionWrapper();
        private TransactionWrapper.TransactionHandle transaction = null;


        public void EnsureInTransaction(Document document)
        {
            if (transaction == null)
                transaction = wrapper.StartTransaction(document);
        }

        public void TransactionTaskDone()
        {
            if (transaction != null)
            {
                transaction.CommitTransaction();
                transaction = null;
            }
        }

        public void ForceCloseTransaction()
        {
            if (transaction != null)
            {
                transaction.CommitTransaction();
                transaction = null;
            }
        }
    }




    /// <summary>
    /// Wraps Revit Transaction methods and provides events for transaction initialization
    /// and completion.
    /// </summary>
    public class TransactionWrapper
    {
        private Transaction Transaction { get; set; }
        private readonly WarningHandler _handler;
        private readonly TransactionHandle _handle;

        public TransactionWrapper()
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
            private readonly TransactionWrapper _tm;

            internal WarningHandler(TransactionWrapper transactionManager)
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
            private readonly TransactionWrapper _manager;

            internal TransactionHandle(TransactionWrapper t)
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
