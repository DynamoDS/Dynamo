using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Dynamo.Core;

namespace Dynamo.Selection
{
    public class DynamoSelection : NotificationObject
    {
        private static DynamoSelection _instance;
        private SmartCollection<ISelectable> selection = new SmartCollection<ISelectable>();

        public static DynamoSelection Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DynamoSelection();
                }

                return _instance;
            }
        }

        public static void DestroyInstance()
        {
            if (_instance != null)
            {
                if (_instance.selection != null)
                {
                    _instance.selection.CollectionChanged -= selection_CollectionChanged;
                    _instance.selection.Clear();
                    _instance.selection = null;
                }

                _instance = null;
            }
        }

        /// <summary>
        /// Returns a collection of ISelectable elements.
        /// </summary>
        public SmartCollection<ISelectable> Selection
        {
            get { return selection; }
            set
            {
                selection = value;
                RaisePropertyChanged("Selection");
            }
        }

        private DynamoSelection()
        {
            Selection.CollectionChanged += selection_CollectionChanged;
        }

        /// <summary>
        /// A callback for automatically selecting and deselecting elements 
        /// when they are added to the Selection collection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void selection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //call the select method on elements added to the collection
            if (e.NewItems != null)
            {
                foreach (ISelectable n in e.NewItems)
                {
                    n.Select();
                }
            }

            if (e.OldItems != null)
            {
                foreach (ISelectable n in e.OldItems)
                {
                    n.Deselect();
                }
            }

            //Debug.WriteLine(string.Format("{0} elements in selection.", Selection.Count));
        }

        /// <summary>
        /// Clears the selection, deslecting everything that is selected
        /// </summary>
        public void ClearSelection()
        {
            Instance.Selection.ToList().ForEach(x=>x.Deselect());
            Instance.Selection.Reset(new List<ISelectable>());
        }
    }

    public interface ISelectable
    {
        bool IsSelected { get; set; }
        void Select();
        void Deselect();
    }

    /// <summary>
    /// A resetable observable collection
    /// See: http://stackoverflow.com/questions/13302933/how-to-avoid-firing-observablecollection-collectionchanged-multiple-times-when-r
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SmartCollection<T> : ObservableCollection<T>
    {
        public SmartCollection()
            : base()
        {
        }

        public SmartCollection(IEnumerable<T> collection)
            : base(collection)
        {
        }

        public SmartCollection(List<T> list)
            : base(list)
        {
        }

        public void AddRange(IEnumerable<T> range)
        {
            foreach (var item in range)
            {
                Items.Add(item);
            }

            this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void Reset(IEnumerable<T> range)
        {
            this.Items.Clear();

            AddRange(range);
        }
    }

}
