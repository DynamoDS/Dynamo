using System;
using System.Collections.ObjectModel;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.ViewModels
{
    public class WatchNode : NotificationObject
    {
        public event Action Clicked;

        internal void Click()
        {
            if (Clicked != null)
                Clicked();
        }

        WatchTreeBranch _children = new WatchTreeBranch();
        string _label;
        string _link;
        private bool _showRawData;

        public WatchTreeBranch Children
        {
            get { return _children; }
            set
            {
                _children = value;
                RaisePropertyChanged("Children");
            }
        }
        public string NodeLabel
        {
            get { return _label; }
            set
            {
                _label = value;
                RaisePropertyChanged("NodeLabel");
            }
        }
        public string Link
        {
            get { return _link; }
            set
            {
                _link = value;
                RaisePropertyChanged("Link");
            }
        }

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

        public WatchNode()
        {
            IsNodeExpanded = true;
            _showRawData = true;
        }

        public WatchNode(string label)
        {
            _label = label;
            IsNodeExpanded = true;
        }

        public WatchNode(string label, string tag)
        {
            _label = string.Format("[{0}] {1}", tag, label);
        }
    }

    public class WatchTreeBranch : ObservableCollection<WatchNode>
    {

    }
}
