using System;
using System.Collections.ObjectModel;
using System.Linq;
using Dynamo.Interfaces;
using Dynamo.UI.Commands;
using Microsoft.Practices.Prism.ViewModel;
using ProtoCore.Mirror;
using System.Collections.Generic;

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

        private ObservableCollection<WatchViewModel> _children = new ObservableCollection<WatchViewModel>();
        private string _label;
        private string _link;
        private bool _showRawData;
        private string _path = "";
        private bool _isOneRowContent;
        private readonly Action<string> tagGeometry;
        private bool _isCollection;

        // Instance variable for the number of items in the list 
        private int numberOfItems;

        // Instance variable for the max depth of items in the list
        private int maxListLevel;

        //Instance variable for the current list depth of items in the list
        private int listDepth;

        //Instance variable for list of list level items
        private List<ListLevel> _listlevelsList;

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
                return splits.Any() ? string.Format("[{0}]", splits.Last()) : string.Empty;
                //return _path;
            }
        }

        /// <summary>
        /// A path describing the location of the data.
        /// Path takes the form var_xxxx...:0:1:2, where
        /// var_xxx is the AST identifier for the node, followed
        /// by : delimited indices represnting the array index
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
        public int NumberOfItemsWT
        { 
            get { return numberOfItems; }
            set
            {
                numberOfItems = value;
                IsCollection = true;
                RaisePropertyChanged("NumberOfItemsWT");
            }
        }


        /// <summary>
        /// Indicates if the items are lists
        /// </summary>
        public bool IsCollection
        {
            get { return _isCollection; }
            set {
                _isCollection = value;
                RaisePropertyChanged("IsCollection");
            }
        }

        /// <summary>
        /// Returns a list of listlevel items
        /// </summary>
        public List<ListLevel> listLevelList
        {
            get { return _listlevelsList;  }
            set
            {
                _listlevelsList = value;
                RaisePropertyChanged("listLevelList");
            }
        }



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
        public void countNumberOfItemsWVM()
        {
            NumberOfItemsWT = 0; // reset the number of items in list
            maxListLevel = 0; // reset the max list depth
            listDepth = 0; 
            numberOfItemsCountHelperWVM(this);
            IsCollection = true;
            RaisePropertyChanged("NumberOfItemsWT");
            RaisePropertyChanged("IsCollection");
            RaisePropertyChanged("maxDepthOfList");
        }

        /// <summary>
        /// Helper method to count the total number of items and the max list depth in a collection recursively
        /// </summary>
        private void numberOfItemsCountHelperWVM(WatchViewModel wvm)
        {
                if (wvm.Children.Count > 0)
                {
                    if (wvm.Path != null)
                        listDepth++; // as the list is traversed deeper on each level, the current list depth will increase

                    foreach (var item in wvm.Children)
                    {
                        numberOfItemsCountHelperWVM(item);
                    }

                    if (listDepth > 0)
                        listDepth--; // as the list depth exits each level, the current list depth will decrease
                }

                if (listDepth > maxListLevel)
                     maxListLevel = listDepth; 

                if (wvm.NodeLabel != null && wvm.NodeLabel != "List")
                    numberOfItems++;
        }

        /// <summary>
        /// Count the list levels of each list 
        /// </summary>
        public void countListLevel()
        {
            _listlevelsList = new List<ListLevel>();

            if (maxListLevel > 0)
            {
                for (int i = this.maxListLevel; i >= 1; i--)
                {
                    _listlevelsList.Add(new ListLevel() { listLevel = i });
                }
                this.listLevelList = _listlevelsList;
            }
        }
    }

    public class ListLevel
    {
        public int listLevel { get; set; }
    }

}
