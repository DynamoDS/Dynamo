using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DynamoUtilities
{
    /// <summary>
    /// Wrapper over System.Collections.ObjectModel.ObservableCollection
    /// This class supports batch operations that should defer collectionChaned notifications.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SmartObservableCollection<T> : ObservableCollection<T>
    {
        protected bool _suppressNotification = false;

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (_suppressNotification)
                return;

            base.OnPropertyChanged(e);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_suppressNotification)
                return;

            base.OnCollectionChanged(e);
        }

        public SmartObservableCollection() : base()
        {}

        
        public SmartObservableCollection(List<T> list)
            : base(list)
        {}

        public SmartObservableCollection(IEnumerable<T> collection) : base(collection)
        {}

        /// <summary>
        /// Adds an item only if the sequence does not have it yet
        /// </summary>
        /// <param name="item">Item to add</param>
        internal void AddUnique(T item)
        {
            if (!Contains(item))
            {
                Add(item);
            }
        }

        internal void Reset(IEnumerable<T> range)
        {
            if (range == null)
                throw new ArgumentNullException("range");

            using(DeferCollectionReset())
            {
                ClearItems();
                foreach (T item in range)
                {
                    Add(item);
                }
            }
        }

        /// <summary>
        /// Suppresses the all CollectionChanged notifications until the returned IDisposable is destroyed.
        /// When the returned object is destroyed, a single NotifyCollectionChangedAction.Reset will be triggered.
        /// </summary>
        /// <returns></returns>
        internal IDisposable DeferCollectionReset()
        {
            if (_suppressNotification)
                return null;// Already suppressed so we can return a "dummy"

            return Dynamo.Scheduler.Disposable.Create(() => {
                _suppressNotification = true;
            },
            () => {
                _suppressNotification = false;
                this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            });
        }

        internal void AddRange(IEnumerable<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            using (DeferCollectionReset())
            {
                foreach (T item in list)
                {
                    Add(item);
                }
            }
        }

        internal void RemoveRange(IEnumerable<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            using (DeferCollectionReset())
            {
                foreach (T item in list)
                {
                    Remove(item);
                }
            }
        }

        internal void Reset(IEnumerable<T> toRemove, IEnumerable<T> toAdd)
        {
            if (toRemove == null)
                throw new ArgumentNullException("toRemove");

            if (toAdd == null)
                throw new ArgumentNullException("toAdd");

            using (DeferCollectionReset())
            {
                foreach (T item in toRemove)
                {
                    Remove(item);
                }

                foreach (T item in toAdd)
                {
                    Add(item);
                }
            }
        }
    }
}