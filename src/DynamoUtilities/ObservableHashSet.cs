using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;

namespace Dynamo.Utilities
{
    internal class ObservableHashSet<T> : INotifyCollectionChanged
    {
        private readonly HashSet<T> set = new HashSet<T>();

        // Guards all access to the underlying set. Infos entries are mutated on the
        // scheduler thread (HomeWorkspaceModel.OnUpdateGraphCompleted) while being
        // enumerated on the UI thread (NodeViewModel.UpdateBubbleContent), so the raw
        // HashSet must not be touched from two threads at once. See DYN-9493.
        private readonly object syncRoot = new object();

        /// <summary>
        /// Event raised when items are added or removed from the hash set.
        /// Currently, it is possible for listeners to determine if items are added,
        /// however, removed items cannot be accessed from the NotifyCollectionChangedEventArgs argument.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Add(T item)
        {
            lock (syncRoot)
            {
                set.Add(item);
            }
            // Raise the event outside the lock: handlers (e.g. UpdateBubbleContent)
            // marshal synchronously onto the UI thread, which may itself enumerate this
            // collection and take the lock. Firing under the lock risks a deadlock.
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public void RemoveWhere(Predicate<T> match)
        {
            lock (syncRoot)
            {
                set.RemoveWhere(match);
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Any(Func<T, bool> match)
        {
            lock (syncRoot)
            {
                return set.Any(match);
            }
        }

        public void AddRange(IEnumerable<T> range)
        {
            // Materialize once before taking the lock so a lazy/side-effecting enumerable
            // is not evaluated while syncRoot is held, and so the event args carry the
            // actual added items rather than the enumerable itself. Typed as List<T>
            // (not IList<T>) so the NotifyCollectionChangedEventArgs(action, IList)
            // overload is selected; IList<T> does not derive from the non-generic IList
            // and would bind to the (action, object) overload instead. See DYN-9493.
            var items = range as List<T> ?? range.ToList();
            lock (syncRoot)
            {
                foreach (var item in items)
                {
                    set.Add(item);
                }
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items));
        }

        public int Count
        {
            get { lock (syncRoot) { return set.Count; } }
        }

        public IEnumerator<T> GetEnumerator()
        {
            // Return a snapshot taken under the lock so callers can enumerate safely
            // even if the set is mutated on another thread mid-enumeration.
            lock (syncRoot)
            {
                return set.ToList().GetEnumerator();
            }
        }
    }
}
