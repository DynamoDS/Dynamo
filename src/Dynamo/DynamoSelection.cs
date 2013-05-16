using System;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.Selection
{
    public class DynamoSelection : NotificationObject
    {
        private static DynamoSelection _instance;
        private ObservableCollection<ISelectable> selection = new ObservableCollection<ISelectable>();

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

        /// <summary>
        /// Returns a collection of ISelectable elements.
        /// </summary>
        public ObservableCollection<ISelectable> Selection
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
            Selection.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(selection_CollectionChanged);
        }

        /// <summary>
        /// A callback for automatically selecting and deselecting elements 
        /// when they are added to the Selection collection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void selection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                throw new Exception("To properly clean the selection, please use RemoveAll() instead.");
            }

            // call the select method on elements added to the collection
            if (e.NewItems != null)
            {
                foreach (ISelectable n in e.NewItems)
                {
                    n.Select();
                }
            }

            if (e.OldItems != null)
            {
                // call the deselect method on elements removed from the collection
                foreach (ISelectable n in e.OldItems)
                {
                    n.Deselect();
                }
            }
        }

        public void ClearSelection()
        {
            Instance.Selection.RemoveAll();
        }
    }

    public interface ISelectable
    {
        bool IsSelected { get; set; }
        void Select();
        void Deselect();
    }

    public static class Extensions
    {
        public static void RemoveAll(this ObservableCollection<ISelectable> list)
        {
            while (list.Count > 0)
            {
                list.RemoveAt(list.Count - 1);
            }
        }
    }

}
