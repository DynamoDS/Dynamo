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

        /// <summary>
        /// Locks for read. Use this API when you need more control of the lock lifetime
        /// </summary>
        internal void LockForRead() => rwlock.EnterReadLock();
        /// <summary>
        /// Locks for write. Use this API when you need more control of the lock lifetime
        /// </summary>
        internal void LockForWrite() => rwlock.EnterWriteLock();
        /// <summary>
        /// Locks for upgradeable read. Use this API when you need more control of the lock lifetime
        /// </summary>
        internal void LockForUpgradeableRead() => rwlock.EnterUpgradeableReadLock();

        /// <summary>
        /// Unlocks a read lock. Use this API if you called LockForRead beforehand.
        /// </summary>
        internal void UnlockForRead() => rwlock.ExitReadLock();
        /// <summary>
        /// Unlocks a write lock. Use this API if you called LockForWrite beforehand.
        /// </summary>
        internal void UnlockForWrite() => rwlock.ExitWriteLock();
        /// <summary>
        /// Unlocks an upgradeable read lock. Use this API if you called LockForUpgradeableRead beforehand.
        /// </summary>
        internal void UnlockForUpgradeableRead() => rwlock.ExitUpgradeableReadLock();


        public void Dispose()
        {
            try
            {
                if (rwlock.IsReadLockHeld)
                    rwlock.ExitReadLock();
                if (rwlock.IsWriteLockHeld)
                    rwlock.ExitWriteLock();
                if (rwlock.IsUpgradeableReadLockHeld)
                    rwlock.ExitUpgradeableReadLock();
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
        /// <returns>New disposable object</returns>
        internal IDisposable CreateReadLock() => Disposable.Create(() => { rwlock.EnterReadLock(); }, () => { rwlock.ExitReadLock(); });

        /// <summary>
        /// Constructs a disposable object that locks for read and allows lock for write in the same scope. Releases the lock when Dispose is called.
        /// </summary>
        /// <returns>New disposable object</returns>
        internal IDisposable CreateUpgradeableReadLock() => Disposable.Create(() => { rwlock.EnterUpgradeableReadLock(); }, () => { rwlock.ExitUpgradeableReadLock(); });
    }
}
