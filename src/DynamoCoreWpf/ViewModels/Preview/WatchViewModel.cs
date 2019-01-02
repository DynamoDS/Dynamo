using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dynamo.UI.Commands;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.ViewModels
{
    public class WatchViewModel : NotificationObject
    {
        #region Events

        public event Action Clicked;

        internal void Click()
        {
            if (Clicked != null)
                Clicked();
        }

        #endregion

        #region Properties/Fields
        public const string EMPTY_LIST = "Empty List";
        public const string LIST = "List";
        public const string EMPTY_DICTIONARY = "Empty Dictionary";
        public const string DICTIONARY = "Dictionary";

        private ObservableCollection<WatchViewModel> _children = new ObservableCollection<WatchViewModel>();
        private string _label;
        private string _link;
        private bool _showRawData;
        private string _path = "";
        private bool _isOneRowContent;
        private readonly Action<string> tagGeometry;
        private bool isCollection;

        // Instance variable for the number of items in the list 
        private int numberOfItems;

        // Instance variable for the max depth of items in the list
        private int maxListLevel;

        // Instance variable for the list of levels 
        private IEnumerable<int> levels;

        public DelegateCommand FindNodeForPathCommand { get; set; }

        /// <summary>
        /// A collection of child WatchItems.
        /// </summary>
        public ObservableCollection<WatchViewModel> Children
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
        /// Returns the last index of the Path, 
        /// surrounded with square brackets.
        /// </summary>
        public string ViewPath
        {
            get
            {
                var splits = _path.Split(':');
                if (splits.Count() == 1)
                    return string.Empty;
                return splits.Any() ? string.Format(NodeLabel == LIST ? "{0}" : " {0} ", splits.Last()) : string.Empty;
            }
        }

        /// <summary>
        /// A path describing the location of the data.
        /// Path takes the form var_xxxx...:0:1:2, where
        /// var_xxx is the AST identifier for the node, followed
        /// by : delimited indices representing the array index
        /// of the data.
        /// </summary>
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

        /// <summary>
        /// If Content is 1 string, e.g. "Empty", "null", "Function", margin should be more to the left side.
        /// For this purpose used this value. When it's true, margin in DataTrigger is set to -15,5,5,5; otherwise
        /// it's set to 5,5,5,5
        /// </summary>
        public bool IsOneRowContent
        {
            get { return _isOneRowContent; }
            set
            {
                _isOneRowContent = value;
                RaisePropertyChanged("IsOneRowContent");
            }
        }

        /// <summary>
        /// Number of items in the overall list if node output is a list
        /// </summary>
        public int NumberOfItems
        {
            get { return numberOfItems; }
            set
            {
                numberOfItems = value;
                RaisePropertyChanged("NumberOfItems");
            }
        }


        /// <summary>
        /// Indicates if the items are lists
        /// </summary>
        public bool IsCollection
        {
            get { return isCollection; }
            set
            {
                isCollection = value;
                RaisePropertyChanged("IsCollection");
            }
        }

        /// <summary>
        /// Returns a list of listlevel items
        /// </summary>
        public IEnumerable<int> Levels
        {
            get { return levels; }
            set
            {
                levels = value;
                RaisePropertyChanged("Levels");
            }
        }

        /// <summary>
        /// Indicates if the item is the top level item
        /// </summary>
        public bool IsTopLevel { get; set; }

        #endregion

        public WatchViewModel(Action<string> tagGeometry) : this(null, null, tagGeometry, true) { }

        public WatchViewModel(string label, string path, Action<string> tagGeometry, bool expanded = false)
        {
            FindNodeForPathCommand = new DelegateCommand(FindNodeForPath, CanFindNodeForPath);
            _path = path;
            _label = label;
            IsNodeExpanded = expanded;
            this.tagGeometry = tagGeometry;
            numberOfItems = 0;
            maxListLevel = 0;
            isCollection = label == WatchViewModel.LIST || label == WatchViewModel.DICTIONARY;
        }

        private bool CanFindNodeForPath(object obj)
        {
            return !string.IsNullOrEmpty(obj.ToString());
        }

        private void FindNodeForPath(object obj)
        {
            if (tagGeometry != null)
            {
                tagGeometry(obj.ToString());
            }
            //visualizationManager.TagRenderPackageForPath(obj.ToString());
        }

        /// <summary>
        /// Method to account for the total number of items and the depth of a list in a list (in the WatchTree)
        /// </summary>
        /// 
        public void CountNumberOfItems()
        {
            var listLevelAndItemCount = GetMaximumDepthAndItemNumber(this);
            maxListLevel = listLevelAndItemCount.Item1;
            NumberOfItems = listLevelAndItemCount.Item2;
            IsCollection = maxListLevel > 1;
        }

        private Tuple<int, int> GetMaximumDepthAndItemNumber(WatchViewModel wvm)
        {
            if (wvm.Children.Count == 0)
            {
                if (wvm.NodeLabel == WatchViewModel.EMPTY_LIST)
                    return new Tuple<int, int>(0, 0);
                else
                    return new Tuple<int, int>(1, 1);
            }

            // If its a top level WatchViewModel, call function on child
            if (wvm.Path == null) 
            {
                return GetMaximumDepthAndItemNumber(wvm.Children[0]);
            }

            // if it's a list, recurse
            if (wvm.NodeLabel == LIST)
            {
                var depthAndNumbers = wvm.Children.Select(GetMaximumDepthAndItemNumber);
                var maxDepth = depthAndNumbers.Select(t => t.Item1).DefaultIfEmpty(1).Max() + 1;
                var itemNumber = depthAndNumbers.Select(t => t.Item2).Sum();
                return new Tuple<int, int>(maxDepth, itemNumber);
            }

            return new Tuple<int, int>(1,1);
        }

        /// <summary>
        /// Set the list levels of each list 
        /// </summary>
        public void CountLevels()
        {
            Levels = maxListLevel > 0 ? Enumerable.Range(1, maxListLevel).Reverse().Select(x => x).ToList() : Enumerable.Empty<int>();
        }
    }
}
