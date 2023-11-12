using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace DynamoUtilities
{
    /// <summary>
    /// Wrapper over System.Collections.ObjectModel.ObservableCollection that fires minimal notifications.
    /// This class supports batch operations that should defer CollectionChaned notifications.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SmartObservableCollection<T> : ObservableCollection<T>
    {
        private bool suppressNotification = false;
        private bool anyChangesDuringSuppress = false;

        /// <summary>
        /// This event is fired when DeferCollectionReset is used
        /// DeferCollectionReset only fires a single CollectionChanged Reset event with no added/removed data.
        /// Use this event to get data on what was added and removed during DeferCollectionReset.
        /// </summary>
        internal event NotifyCollectionChangedEventHandler CollectionChangedDuringDeferredReset;

        public SmartObservableCollection() : base()
        {}

        public SmartObservableCollection(List<T> list)
            : base(list)
        {}

        public SmartObservableCollection(IEnumerable<T> collection) : base(collection)
        {}

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

        private void OnCollectionDeferredResetChanged(IList<T> itemsBeforeReset)
        {
            if (CollectionChangedDuringDeferredReset != null &&
                CollectionChangedDuringDeferredReset.GetInvocationList().Length > 0)
            {
                var wasAdded = Items.Except(itemsBeforeReset).Where(x => x != null).ToList() as System.Collections.IList;
                var wasRemoved = itemsBeforeReset.Except(Items).Where(x => x != null).ToList() as System.Collections.IList;

                CollectionChangedDuringDeferredReset(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, wasRemoved as System.Collections.IList));
                CollectionChangedDuringDeferredReset(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, wasAdded as System.Collections.IList));
            }
        }

        /// <summary>
        /// Suppresses all CollectionChanged notifications until the returned IDisposable is destroyed.
        /// When the returned object is destroyed, a single NotifyCollectionChangedAction.Reset event will be triggered.
        /// In this case Reset means a major change to the Collection has happened. 
        /// Make sure that any CollectionChanged handlers know to interpret the Reset event as a major change (not only that the collection was cleared).
        /// Optionally CollectionChangedDuringDeferredReset can be used to get notifications on what was added/removed.
        /// </summary>
        /// <returns>IDisposible that controls the lifetime of the deferred notifications</returns>
        internal IDisposable DeferCollectionReset()
        {
            IList<T> itemsBeforeReset = null;
            if (CollectionChangedDuringDeferredReset != null &&
                CollectionChangedDuringDeferredReset.GetInvocationList().Length > 0)
            {
                itemsBeforeReset = Items.ToList();
            }
            return DeferCollectionNotification(NotifyCollectionChangedAction.Reset, itemsBeforeReset);
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

                if (!anyChanges)
                    return;

                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));

                if (action == NotifyCollectionChangedAction.Reset)
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(action));
                    OnCollectionDeferredResetChanged(changes);
                }
                else if (changes != null && changes.Count > 0)
                {
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
    }
}