using System;
using Autodesk.Revit.DB;

namespace RevitServices.Transactions
{
    /// <summary>
    ///     Transaction helper for nodes
    /// </summary>
    public class TransactionManager
    {
        private static TransactionManager manager;
        
        /// <summary>
        ///     Setup a manager with a default strategy
        /// </summary>
        public static void SetupManager()
        {
            manager = new TransactionManager();
        }
        
        /// <summary>
        ///     Setup a manager with a specified strategy
        /// </summary>
        public static void SetupManager(ITransactionStrategy strategy)
        {
            manager = new TransactionManager(strategy);
        }

        /// <summary>
        ///     Get the current transaction manager
        /// </summary>
        /// <value></value>
        public static TransactionManager Instance
        {
            get
            {
                if (manager == null)
                {
                    throw new InvalidOperationException(
                        "TransactionManager must be setup before use. Call SetupManager with a choice of strategy");
                }

                return manager;
            }
        }

        /// <summary>
        ///     Transaction strategy utilized by this manager.
        /// </summary>
        public ITransactionStrategy Strategy { get; set; }

        /// <summary>
        ///     TransactionWrapper managed by this manager.
        /// </summary>
        public TransactionWrapper TransactionWrapper { get; private set; }

        private TransactionHandle handle;

        /// <summary>
        ///     Construct a new manager with a default transaction strategy
        /// </summary>
        private TransactionManager() : this(new DebugTransactionStrategy()) { }

        /// <summary>
        ///     Construct a new manager with a given strategy
        /// </summary>
        /// <param name="strategy"></param>
        private TransactionManager(ITransactionStrategy strategy)
        {
            if (strategy == null)
            {
                throw new ArgumentNullException(
                    "strategy",
                    "Strategy must not be null, to use a default call the overload with no arguments");
            }
            Strategy = strategy;
            TransactionWrapper = new TransactionWrapper();
        }

        /// <summary>
        /// Ensure that there is a valid transaction active
        /// </summary>
        public void EnsureInTransaction(Document document)
        {
            //Hand off the behaviour to the strategy
            handle = Strategy.EnsureInTransaction(TransactionWrapper, document);
        }
        
        /// <summary>
        ///     Notify that the transaction system that the operations
        ///     requiring a transaction are complete
        /// </summary>
        public void TransactionTaskDone()
        {
            //Hand off the behaviour to the strategy
            Strategy.TransactionTaskDone(handle);
        }
        
        /// <summary>
        ///     Require that the current transaction is recycled
        /// </summary>
        public void ForceCloseTransaction()
        {
            //Hand off the behaviour to the strategy
            Strategy.ForceCloseTransaction(handle);
        }
    }


    public interface ITransactionStrategy
    {
        /// <summary>
        ///     Ensure that there is a valid transaction active
        /// </summary>
        TransactionHandle EnsureInTransaction(TransactionWrapper wrapper, Document document);

        /// <summary>
        ///     Notify that the transaction system that the operations
        ///     requiring a transaction are complete
        /// </summary>
        /// <param name="handle"></param>
        void TransactionTaskDone(TransactionHandle handle);

        /// <summary>
        ///     Require that the current transaction is recycled
        /// </summary>
        /// <param name="handle"></param>
        void ForceCloseTransaction(TransactionHandle handle);
    }


    /// <summary>
    ///     Basic transaction handling strategy that opens
    ///     a new transaction for every operation
    /// </summary>
    public class DebugTransactionStrategy : ITransactionStrategy
    {
        public TransactionHandle EnsureInTransaction(TransactionWrapper wrapper, Document document)
        {
            return !wrapper.TransactionActive ? wrapper.StartTransaction(document) : wrapper.Handle;
        }

        public void TransactionTaskDone(TransactionHandle handle)
        {
            ForceCloseTransaction(handle);
        }
        
        public void ForceCloseTransaction(TransactionHandle handle)
        {
            if (handle != null && handle.Status == TransactionStatus.Started)
                handle.CommitTransaction();
        }
    }


    /// <summary>
    ///     Transaction handling strategy that uses the same
    ///     transaction for 
    /// </summary>
    public class AutomaticTransactionStrategy : ITransactionStrategy
    {
        public TransactionHandle EnsureInTransaction(TransactionWrapper wrapper, Document document)
        {
            return !wrapper.TransactionActive ? wrapper.StartTransaction(document) : wrapper.Handle;
        }

        public void TransactionTaskDone(TransactionHandle handle)
        {
            //Do nothing in automatic, continue using the same transaction.
        }

        public void ForceCloseTransaction(TransactionHandle handle)
        {
            if (handle != null && handle.Status == TransactionStatus.Started)
                handle.CommitTransaction();
        }
    }


    /// <summary>
    ///     Wraps Revit Transaction methods and provides events for transaction initialization
    ///     and completion.
    /// </summary>
    public class TransactionWrapper
    {
        internal Transaction Transaction { get; set; }
        private readonly WarningHandler handler;
        internal readonly TransactionHandle Handle;

        internal TransactionWrapper()
        {
            handler = new WarningHandler(this);
            Handle = new TransactionHandle(this);
        }
        
        /// <summary>
        ///     Event for handling failure messages from Revit.
        /// </summary>
        public event FailureDelegate FailuresRaised;

        /// <summary>
        ///     Called when the managed Transaction is started.
        /// </summary>
        public event Action TransactionStarted;

        /// <summary>
        ///     Called when the managed Transaction is committed.
        /// </summary>
        public event Action TransactionCommitted;

        /// <summary>
        ///     Called when the managed Transaction is cancelled.
        /// </summary>
        public event Action TransactionCancelled;

        #region Event Raising Utility Methods
        private void RaiseTransactionStarted()
        {
            if (TransactionStarted != null)
                TransactionStarted();
        }

        internal void RaiseTransactionCommitted()
        {
            if (TransactionCommitted != null)
                TransactionCommitted();
        }

        internal void RaiseTransactionCancelled()
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
        ///     Starts a RevitAPI Transaction that will be managed by this TransactionManager instance.
        /// </summary>
        /// <param name="document">Document (DB) to start a Transaction for.</param>
        public TransactionHandle StartTransaction(Document document)
        {
            if (Transaction == null || Transaction.GetStatus() != TransactionStatus.Started)
            {
                Transaction = new Transaction(document, "Dynamo Script");
                Transaction.Start();

                FailureHandlingOptions failOpt = Transaction.GetFailureHandlingOptions();
                failOpt.SetFailuresPreprocessor(handler);
                Transaction.SetFailureHandlingOptions(failOpt);

                RaiseTransactionStarted();
            }
            return Handle;
        }

        /// <summary>
        ///     Is there a currently active Transaction?
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
            private readonly TransactionWrapper tm;

            internal WarningHandler(TransactionWrapper transactionManager)
            {
                tm = transactionManager;
            }

            public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
            {
                tm.ProcessFailures(failuresAccessor);

                return FailureProcessingResult.Continue;
            }
        }

        #endregion
    }

    /// <summary>
    ///     Handles Transactions created via a TransactionWrapper
    /// </summary>
    public class TransactionHandle
    {
        private readonly TransactionWrapper manager;

        internal TransactionHandle(TransactionWrapper t)
        {
            manager = t;
        }

        /// <summary>
        ///     Commits the managed Transaction to the Revit DB.
        /// </summary>
        public TransactionStatus CommitTransaction()
        {
            if (manager != null)
            {
                if (manager.Transaction.GetStatus() == TransactionStatus.Started)
                {
                    var result = manager.Transaction.Commit();
                    manager.RaiseTransactionCommitted();

                    return result;
                }
            }
            throw new InvalidOperationException("Cannot commit a transaction that isn't active.");
        }

        /// <summary>
        ///     Cancels the managed Transaction, rolling-back any non-committed changes.
        /// </summary>
        public TransactionStatus CancelTransaction()
        {
            if (manager != null)
            {
                if (manager.Transaction.GetStatus() == TransactionStatus.Started)
                {
                    var result = manager.Transaction.RollBack();
                    manager.RaiseTransactionCancelled();

                    return result;
                }
            }
            throw new InvalidOperationException("Cannot cancel a transaction that isn't active.");
        }

        /// <summary>
        ///     Status of the managed Transaction.
        /// </summary>
        public TransactionStatus Status
        {
            get
            {
                return manager.Transaction == null
                    ? TransactionStatus.Uninitialized
                    : manager.Transaction.GetStatus();
            }
        }
    }

    /// <summary>
    ///     Callback signature for processing failure messages from Revit.
    /// </summary>
    /// <param name="failures">
    ///     FailuresAccessor from Revit, containing all emmitted failures.
    /// </param>
    public delegate void FailureDelegate(FailuresAccessor failures);
}
