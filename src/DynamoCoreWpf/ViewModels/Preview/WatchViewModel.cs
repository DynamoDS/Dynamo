using System;
using System.Collections.ObjectModel;
using System.Linq;
using Dynamo.Interfaces;
using Dynamo.UI.Commands;
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

        private ObservableCollection<WatchViewModel> _children = new ObservableCollection<WatchViewModel>();
        private string _label;
        private string _link;
        private bool _showRawData;
        private string _path = "";
        private bool _isOneRowContent;

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
        /// If Content is 1 string, e.g. "Empty", "null", "_SingleFunctionObject", margin should be more to the left side.
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

        #endregion

        public WatchViewModel()
        {
            FindNodeForPathCommand = new DelegateCommand(FindNodeForPath, CanFindNodeForPath);
            IsNodeExpanded = true;
            _showRawData = false;
        }

        public WatchViewModel(string label, string path, bool expanded = false)
        {
            FindNodeForPathCommand = new DelegateCommand(FindNodeForPath, CanFindNodeForPath);
            _path = path;
            _label = label;
            IsNodeExpanded = expanded;
        }

        private bool CanFindNodeForPath(object obj)
        {
            return !string.IsNullOrEmpty(obj.ToString());
        }

        private void FindNodeForPath(object obj)
        {
            //visualizationManager.TagRenderPackageForPath(obj.ToString());
        }
    }
}
