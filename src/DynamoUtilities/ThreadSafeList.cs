using System;
using System.Collections;
using System.Collections.Generic;

namespace DynamoUtilities
{
    /// <summary>
    /// Thread safe List<T>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ThreadSafeList<T> : IList<T>
    {
        /// <summary>
        /// Thread safe Enumerator used by the ThreadSafeList class
        /// </summary>
        internal class TSEnumerator : IEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;
            private readonly IDisposable readLock;

            internal TSEnumerator(IEnumerator<T> inner, DynamoLock _lock)
            {
                this._inner = inner;
                this.readLock = _lock.CreateReadLock();
            }

            T IEnumerator<T>.Current => _inner.Current;

            object IEnumerator.Current => _inner.Current;

            public void Dispose()
            {
                _inner.Dispose();
                readLock.Dispose();
            }

            public bool MoveNext()
            {
                return _inner.MoveNext();
            }

            public void Reset()
            {
                _inner.Reset();
            }
        }

        private readonly IList<T> innerList;
        private readonly DynamoLock _lock = new DynamoLock(allowRecursiveLock: true);

        internal ThreadSafeList()
        {
            innerList = new List<T>();
        }

        internal ThreadSafeList(IList<T> items)
        {
            innerList = items != null ? new List<T>(items) : new List<T>();
        }

        internal ThreadSafeList(IEnumerable<T> items)
        {
            innerList = items != null ? new List<T>(items) : new List<T>();
        }

        public T this[int index]
        {
            get
            {
                _lock.LockForRead();
                T item = innerList[index];
                _lock.UnlockForRead();
                return item;
            }

            set
            {
                _lock.LockForWrite();
                innerList[index] = value;
                _lock.UnlockForWrite();
            }
        }

        bool ICollection<T>.IsReadOnly => innerList.IsReadOnly;


        public int Count
        {
            get
            {
                _lock.LockForRead();
                int count = innerList.Count;
                _lock.UnlockForRead();
                return count;
            }

        }

        public void Add(T item)
        {
            _lock.LockForWrite();
            innerList.Add(item);
            _lock.UnlockForWrite();
        }

        public void Clear()
        {
            _lock.LockForWrite();
            innerList.Clear();
            _lock.UnlockForWrite();
        }

        public bool Contains(T item)
        {
            _lock.LockForRead();
            bool contains = innerList.Contains(item);
            _lock.UnlockForRead();
            return contains;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _lock.LockForRead();
            innerList.CopyTo(array, arrayIndex);
            _lock.UnlockForRead();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new TSEnumerator(innerList.GetEnumerator(), _lock);
        }

        public int IndexOf(T item)
        {
            _lock.LockForRead();
            int index = innerList.IndexOf(item);
            _lock.UnlockForRead();
            return index;
        }

        public void Insert(int index, T item)
        {
            _lock.LockForWrite();
            innerList.Insert(index, item);
            _lock.UnlockForWrite();
        }

        public bool Remove(T item)
        {
            _lock.LockForWrite();
            bool removed = innerList.Remove(item);
            _lock.UnlockForWrite();
            return removed;
        }

        public void RemoveAt(int index)
        {
            _lock.LockForWrite();
            innerList.RemoveAt(index);
            _lock.UnlockForWrite();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new TSEnumerator(innerList.GetEnumerator(), _lock);
        }
    }
}
