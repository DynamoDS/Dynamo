using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Navigation;

namespace Dynamo.Wpf.Utilities
{
    public class QuietObservableCollection<T> : ObservableCollection<T>
    {
        private bool IsQuiet = false;

        public QuietObservableCollection(): base() { }

        public QuietObservableCollection(IEnumerable<T> items) : base(items) { }

        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (IsQuiet) return;
            base.OnCollectionChanged(e);
        }

        internal void Shout()
        {
            OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
        }

        internal QuietScope Quietly() => new QuietScope(this);

        internal class QuietScope : IDisposable
        {
            QuietObservableCollection<T> collection;

            public QuietScope(QuietObservableCollection<T> collection)
            {
                this.collection = collection;
                collection.IsQuiet = true;
            }

            public void Dispose()
            {
                collection.IsQuiet = false;
                collection = null;
            }
        }
    }
}
