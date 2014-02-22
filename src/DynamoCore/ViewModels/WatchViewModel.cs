using System;
using System.Collections.ObjectModel;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.ViewModels
{
    public class WatchItem : NotificationObject
    {
        public event Action Clicked;

        internal void Click()
        {
            if (Clicked != null)
                Clicked();
        }

        ObservableCollection<WatchItem> _children = new ObservableCollection<WatchItem>();
        string _label;
        string _link;
        private bool _showRawData;

        /// <summary>
        /// A collection of child WatchItems.
        /// </summary>
        public ObservableCollection<WatchItem> Children
        {
            get { return _children; }
            set
            {
                _children = value;
                RaisePropertyChanged("Children");
            }
        }
        
        /// <summary>
        /// The string lable visibile in the watch.
        /// </summary>
        public string NodeLabel
        {
            get { return _label; }
            set
            {
                _label = value;
                RaisePropertyChanged("NodeLabel");
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public string Link
        {
            get { return _link; }
            set
            {
                _link = value;
                RaisePropertyChanged("Link");
            }
        }

        /// <summary>
        /// A flag used to determine whether the item
        /// should be process to draw 'raw' data or data
        /// treated in some context. An example is the drawing
        /// of watch items with or without units.
        /// </summary>
        public bool ShowRawData
        {
            get { return _showRawData; }
            set
            {
                _showRawData = value;
                RaisePropertyChanged("ShowRawData");
            }
        }

        public bool IsNodeExpanded { get; set; }

        public WatchItem()
        {
            IsNodeExpanded = true;
            _showRawData = true;
        }

        public WatchItem(string label)
        {
            _label = label;
            IsNodeExpanded = true;
        }

        public WatchItem(string label, string tag)
        {
            _label = string.Format("[{0}] {1}", tag, label);
        }
    }

    //public class WatchTreeBranch : ObservableCollection<WatchItem>
    //{

    //}
}
