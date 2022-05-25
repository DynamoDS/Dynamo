using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace DynamoUtilities
{
    /// <summary>
    /// Wrapper over System.Collections.ObjectModel.ObservableCollection
    /// This class supports batch operations that should defer collectionChaned notifications.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObservableCollectionV2<T> : ObservableCollection<T>
    {
        protected bool _suppressNotification = false;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_suppressNotification)
                base.OnCollectionChanged(e);
        }

        /// <summary>
        /// Defers the all CollectionChanged notifications to when the returned IDisposable is destroyed.
        /// </summary>
        /// <returns></returns>
        internal IDisposable DeferCollectionReset()
        {
            return Dynamo.Scheduler.Disposable.Create(() => {
                _suppressNotification = true;
            },
            () => {
                _suppressNotification = false;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            });
        }

        internal void AddRange(IEnumerable<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            _suppressNotification = true;

            foreach (T item in list)
            {
                Add(item);
            }
            _suppressNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        internal void RemoveRange(IEnumerable<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            _suppressNotification = true;

            foreach (T item in list)
            {
                Remove(item);
            }
            _suppressNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        internal void Reset(IEnumerable<T> toRemove, IEnumerable<T> toAdd)
        {
            if (toRemove == null)
                throw new ArgumentNullException("toRemove");

            if (toAdd == null)
                throw new ArgumentNullException("toAdd");

            _suppressNotification = true;

            foreach (T item in toRemove)
            {
                Remove(item);
            }

            foreach (T item in toAdd)
            {
                Add(item);
            }

            _suppressNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
