using System;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace Dynamo.Utilities
{
    internal class ObservableHashSet<T> : INotifyCollectionChanged
    {
        private readonly HashSet<T> set = new HashSet<T>();

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Add(T item)
        {
            set.Add(item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
        }

        public void RemoveWhere(Predicate<T> match)
        {
            set.RemoveWhere(match);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove));
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
