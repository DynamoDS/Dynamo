using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        private string _path = "";

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

        public string ViewPath
        {
            get
            {
                var splits = _path.Split(':');
                if (splits.Count() == 1)
                    return string.Empty;
                return splits.Any() ? string.Format("[{0}]", splits.Last()) : string.Empty;
                //return _path;
            }
        }
        
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                RaisePropertyChanged("Path");
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

        public WatchItem(string label, string path)
        {
            _path = path;
            _label = label;
            IsNodeExpanded = true;
        }

        public WatchItem(string label, string path, bool expanded)
        {
            _path = path;
            _label = label;
            IsNodeExpanded = expanded;
        }
    }
}
