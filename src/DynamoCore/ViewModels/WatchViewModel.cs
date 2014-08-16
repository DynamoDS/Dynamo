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

        private readonly IVisualizationManager visualizationManager;
        private ObservableCollection<WatchViewModel> _children = new ObservableCollection<WatchViewModel>();
        private string _label;
        private string _link;
        private bool _showRawData;
        private string _path = "";

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

        #endregion

        public WatchViewModel(IVisualizationManager visualizationManager)
        {
            this.visualizationManager = visualizationManager;
            FindNodeForPathCommand = new DelegateCommand(FindNodeForPath, CanFindNodeForPath);
            IsNodeExpanded = true;
            _showRawData = false;
        }

        public WatchViewModel(IVisualizationManager visualizationManager, string label, string path, bool expanded = false)
        {
            this.visualizationManager = visualizationManager;
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
            visualizationManager.TagRenderPackageForPath(obj.ToString());
        }
    }
}
