using Dynamo.Scheduler;
using System;
using System.Threading;

namespace DynamoUtilities
{
    // A convenience class that manages the locking/unlocking logic for a ReaderWriterLockSlim object 
    internal class DynamoLock : IDisposable
    {
        private readonly ReaderWriterLockSlim rwlock;

        internal DynamoLock()
        {
            rwlock = new ReaderWriterLockSlim();
        }

        public void Dispose()
        {
            try
            {
                if (rwlock.IsReadLockHeld)
                    rwlock.ExitReadLock();
                if (rwlock.IsWriteLockHeld)
                    rwlock.ExitWriteLock();
            }
            finally 
            {
                rwlock.Dispose();
            }
        }

        /// <summary>
        /// Constructs a disposable object that locks for write and releases the lock when Dispose is called.
        /// </summary>
        /// <returns>New disposable object</returns>
        internal IDisposable CreateWriteLock() => Disposable.Create(() => { rwlock.EnterWriteLock(); }, () => { rwlock.ExitWriteLock(); });

        /// <summary>
        /// Constructs a disposable object that locks for read and releases the lock when Dispose is called.
        /// </summary>
        /// <returns>New disposable  object</returns>
        internal IDisposable CreateReadLock() => Disposable.Create(() => { rwlock.EnterReadLock(); }, () => { rwlock.ExitReadLock(); });
    }
}
