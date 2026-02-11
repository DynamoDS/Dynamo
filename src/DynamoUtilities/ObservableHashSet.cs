using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;

namespace Dynamo.Utilities
{
    internal class ObservableHashSet<T> : INotifyCollectionChanged
    {
        private readonly HashSet<T> set = new HashSet<T>();

        /// <summary>
        /// Event raised when items are added or removed from the hash set. 
        /// Currently, it is possible for listeners to determine if items are added, 
        /// however, removed items cannot be accessed from the NotifyCollectionChangedEventArgs argument.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Add(T item)
        {
            set.Add(item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public void RemoveWhere(Predicate<T> match)
        {
            set.RemoveWhere(match);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Any(Func<T, bool> match)
        {
            return set.Any(match);
        }

        public void AddRange(IEnumerable<T> range)
        {
            foreach (var item in range)
            {
                set.Add(item);
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, range));
        }

        public int Count
        {
            get { return set.Count; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return set.GetEnumerator();
        }
    }
}
