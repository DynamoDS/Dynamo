using System;
using System.Linq;
using Autodesk.Revit.DB;
using RevitServices.Threading;

namespace RevitServices.Transactions
{
    /// <summary>
    ///     Transaction helper for nodes
    /// </summary>
    public class TransactionManager
    {
        public static event Action<string> OnLog;

        internal static void Log(string obj)
        {
            var handler = OnLog;
            if (handler != null) 
                handler(obj);
        }

        private static TransactionManager manager;
        
        /// <summary>
        ///     Setup a manager with a default strategy
        /// </summary>
        public static void SetupManager()
        {
            Log("Setting up Transaction Manager with Default Strategy (Debug)");
            manager = new TransactionManager();
        }
        
        /// <summary>
        ///     Setup a manager with a specified strategy
        /// </summary>
        public static void SetupManager(ITransactionStrategy strategy)
        {
            Log("Setting up Transaction Manager with Strategy: " + strategy.GetType());
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

        private ITransactionStrategy strat;
        /// <summary>
        ///     Transaction strategy utilized by this manager.
        /// </summary>
        public ITransactionStrategy Strategy
        {
            get { return strat; }
            set
            {
                strat = value;
                Log("Transaction Manager Strategy set to: " + strat.GetType());
            }
        }

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
            DoAssertInIdleThread = true;
        }

        /// <summary>
        /// Ensure that there is a valid transaction active
        /// </summary>
        public void EnsureInTransaction(Document document)
        {
            AssertInIdleThread();

            //Hand off the behaviour to the strategy
            handle = Strategy.EnsureInTransaction(TransactionWrapper, document);
        }
        
        /// <summary>
        ///     Notify that the transaction system that the operations
        ///     requiring a transaction are complete
        /// </summary>
        public void TransactionTaskDone()
        {
            AssertInIdleThread();

            //Hand off the behaviour to the strategy
            Strategy.TransactionTaskDone(handle);
        }
        
        /// <summary>
        ///     Require that the current transaction is recycled
        /// </summary>
        public void ForceCloseTransaction()
        {
             AssertInIdleThread();
            
            //Hand off the behaviour to the strategy
            Strategy.ForceCloseTransaction(handle);
        }

        /// <summary>
        ///     Ensures that the current execution context is an IdleThread
        /// </summary>
        public void AssertInIdleThread()
        {
#if !ENABLE_DYNAMO_SCHEDULER
            // SCHEDULER: The stub for IdlePromise has been moved out into DynamoRevit,
            // which means we do not have a way to check here to see if the call's been 
            // made from within an idle thread. This needs a little tweaking if we are 
            // not ready to "assume" that callers of this method always get invoked in 
            // an idle thread.
            // 
            if (DoAssertInIdleThread)
                if (!IdlePromise.InIdleThread)
                    throw new Exception("Cannot start a transaction outside of the Revit idle thread.");
#endif
        }

        /// <summary>
        /// Determines whether the TransactionManager checks to be in an IdleThread.
        /// </summary>
        public bool DoAssertInIdleThread { get; set; }
    }
    
    /// <summary>
    ///     Contains logic for managing transactions in a dynamo graph evaluation.
    /// </summary>
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
            TransactionManager.Log("EnsureInTransaction - DEBUG STRAT: Starting new Transaction");
            return !wrapper.TransactionActive ? wrapper.StartTransaction(document) : wrapper.Handle;
        }

        public void TransactionTaskDone(TransactionHandle handle)
        {
            TransactionManager.Log("TransactionTaskDone - DEBUG STRAT: Ending Transaction");
            EndTransaction(handle);
        }
        
        public void ForceCloseTransaction(TransactionHandle handle)
        {
            TransactionManager.Log("ForceCloseTransaction - DEBUG STRAT: Ending Transaction");
            EndTransaction(handle);
        }

        private static void EndTransaction(TransactionHandle handle)
        {
            if (handle != null && handle.Status == TransactionStatus.Started)
                handle.CommitTransaction();
        }
    }


    /// <summary>
    ///     Transaction handling strategy that uses the same
    ///     transaction for all operations.  Checks to make sure the
    ///     current transaction is using an IdleThread for execution.
    /// </summary>
    public class AutomaticTransactionStrategy : ITransactionStrategy
    {
        public TransactionHandle EnsureInTransaction(TransactionWrapper wrapper, Document document)
        {
            TransactionManager.Log("EnsureInTransaction - AUTO STRAT: Starting new Transaction");
            return !wrapper.TransactionActive ? wrapper.StartTransaction(document) : wrapper.Handle;
        }

        public void TransactionTaskDone(TransactionHandle handle)
        {
            TransactionManager.Log("TransactionTaskDone - AUTO STRAT: Preserving Transaction");
            //Do nothing in automatic, continue using the same transaction.
        }

        public void ForceCloseTransaction(TransactionHandle handle)
        {
            TransactionManager.Log("ForceCloseTransaction - AUTO STRAT: Ending Transaction");
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
        /// <param name="document">Document (DB) to start a Transaction for. The parameter can not be null.</param>
        public TransactionHandle StartTransaction(Document document)
        {
            if (Transaction == null || Transaction.GetStatus() != TransactionStatus.Started)
            {
                TransactionManager.Log("Starting Transaction.");
                
                // Dispose the old transaction so that it won't impact the new transaction
                if (null != Transaction && Transaction.IsValidObject)
                    Transaction.Dispose();

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
                return Transaction != null && Transaction.IsValidObject
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
                    TransactionManager.Log("Committing Transaction.");
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
                    TransactionManager.Log("Cancelling Transaction.");
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
