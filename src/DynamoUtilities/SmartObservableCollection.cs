using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace DynamoUtilities
{
    /// <summary>
    /// Wrapper over System.Collections.ObjectModel.ObservableCollection
    /// This class supports batch operations that should defer collectionChaned notifications.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SmartObservableCollection<T> : ObservableCollection<T>
    {
        private bool suppressNotification = false;
        private bool anyChangesDuringSuppress = false;

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (suppressNotification)
            {
                anyChangesDuringSuppress = true;
                return;
            }

            base.OnPropertyChanged(e);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (suppressNotification)
            {
                anyChangesDuringSuppress = true;
                return;
            }

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
        /// Suppresses the all CollectionChanged notifications until the returned IDisposable is destroyed.
        /// When the returned object is destroyed, a single NotifyCollectionChangedAction.Reset event will be triggered.
        /// In this case Reset means a major change to the Collection has happened. 
        /// Make sure that any CollectionChanged handlers know to interpret the Reset event as a major change (not only that the collection was cleared).
        /// </summary>
        /// <returns></returns>
        internal IDisposable DeferCollectionReset()
        {
            return DeferCollectionNotification(NotifyCollectionChangedAction.Reset);
        }

        /// <summary>
        /// Suppresses the all CollectionChanged notifications until the returned IDisposable is destroyed.
        /// When the returned object is destroyed, a single NotifyCollectionChanged event will be triggered (with the input action and changes as event arguments).
        /// </summary>
        /// <returns></returns>
        private IDisposable DeferCollectionNotification(NotifyCollectionChangedAction action, IList<T> changes = null)
        {
            if (suppressNotification)
                return null;// Already suppressed so we can return a "dummy"

            return Dynamo.Scheduler.Disposable.Create(() => {
                suppressNotification = true;
            },
            () => {
                bool anyChanges = anyChangesDuringSuppress;
                suppressNotification = false;
                anyChangesDuringSuppress = false;

                if (anyChanges && 
                    (action == NotifyCollectionChangedAction.Reset || (changes != null && changes.Count > 0)))
                {
                    this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                    this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, changes as System.Collections.IList));
                }
            });
        }

        internal new void Clear()
        {
            ClearItems();
        }

        protected override void ClearItems()
        {
            if (Items.Count > 0)
            {
                base.ClearItems();
            }
        }

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

        /// <summary>
        /// Adds the input list to the inner collection and only fires a single CollectionChanged event with the actions set as 
        /// NotifyCollectionChangedAction.Add and with the input list as the NotifyCollectionChangedEventArgs.NewItems
        /// </summary>
        /// <param name="list"></param>
        internal void AddRange(IEnumerable<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            using (DeferCollectionNotification(NotifyCollectionChangedAction.Add, list.ToList()))
            {
                foreach (T item in list)
                {
                    Add(item);
                }
            }
        }

        /// <summary>
        /// Removes the input list from the inner collection and only fires a single CollectionChanged event with the actions set as 
        /// NotifyCollectionChangedAction.Remove and with the removed items as the NotifyCollectionChangedEventArgs.OldItems
        /// </summary>
        /// <param name="list"></param>
        internal void RemoveRange(IEnumerable<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            var removedItems = new List<T>();
            using (DeferCollectionNotification(NotifyCollectionChangedAction.Remove, removedItems))
            {
                foreach (T item in list)
                {
                    if (Remove(item))
                    {
                        removedItems.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// Clears the Items and adds the input list to the inner collection.
        /// Fires two CollectionChanged events, Remove(for previous items in collection) and Add (for the input range).
        /// </summary>
        /// <param name="range"></param>
        internal void SetCollection(IEnumerable<T> range)
        {
            if (range == null)
                throw new ArgumentNullException("range");

            using (DeferCollectionNotification(NotifyCollectionChangedAction.Remove, Items.ToList()))
            {
                ClearItems();
            }

            using (DeferCollectionNotification(NotifyCollectionChangedAction.Add, range.ToList()))
            {
                foreach (T item in range)
                {
                    Add(item);
                }
            }
        }

        /// <summary>
        /// Removes the toRemove list and adds the toAdd list to the inner collection.
        /// Fires 2 CollectionChanged events, corresponding to the two input arguments.
        /// Use this method when expecting to drastically change the collection.
        /// </summary>
        /// <param name="toRemove"></param>
        /// <param name="toAdd"></param>
        internal void AddRemove(IEnumerable<T> toRemove, IEnumerable<T> toAdd)
        {
            if (toRemove == null)
                throw new ArgumentNullException("toRemove");

            if (toAdd == null)
                throw new ArgumentNullException("toAdd");

            var removedItems = new List<T>();
            using (DeferCollectionNotification(NotifyCollectionChangedAction.Remove, removedItems))
            {
                foreach (T item in toRemove)
                {
                    if (Remove(item))
                    {
                        removedItems?.Add(item);
                    }
                }
            }

            using (DeferCollectionNotification(NotifyCollectionChangedAction.Add, toAdd.ToList()))
            {
                foreach (T item in toAdd)
                {
                    Add(item);
                }
            }
        }
    }
}