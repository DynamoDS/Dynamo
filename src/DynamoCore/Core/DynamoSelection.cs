using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Dynamo.Core;
using DynamoUtilities;

namespace Dynamo.Selection
{
    internal class DynamoSelection : NotificationObject
    {
        private static DynamoSelection _instance;
        private SmartObservableCollection<ISelectable> selection = new SmartObservableCollection<ISelectable>();

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
                    _instance.selection.CollectionChangedDuringDeferredReset -= selection_CollectionChanged;
                    _instance.selection.Clear();
                    _instance.selection = null;
                }

                _instance = null;
            }
        }

        /// <summary>
        /// Returns a collection of ISelectable elements.
        /// </summary>
        internal SmartObservableCollection<ISelectable> Selection
        {
            get { return selection; }
            set
            {
                selection = value;
                RaisePropertyChanged("Selection");
            }
        }

        public bool ClearSelectionDisabled { get; set; }

        private DynamoSelection()
        {
            Selection.CollectionChanged += selection_CollectionChanged;
            Selection.CollectionChangedDuringDeferredReset += selection_CollectionChanged;
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
            if (ClearSelectionDisabled) return;

            Instance.Selection.ToList().ForEach(x => x.Deselect());
            Instance.Selection.Clear();
        }
    }

    /// <summary>
    /// Interface, that provides selectable objects. These are objects, which user can select by mouse.
    /// E.g. nodes, connectors, groups etc.
    /// </summary>
    public interface ISelectable
    {
        /// <summary>
        /// Bool value indicates if object is selected or not.
        /// </summary>
        bool IsSelected { get; set; }

        /// <summary>
        /// Select object.
        /// </summary>
        void Select();

        /// <summary>
        /// Deselect object.
        /// </summary>
        void Deselect();
    }
}
