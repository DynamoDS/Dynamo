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

        public bool IsNodeExpanded { get; set; }

        public WatchNode()
        {
            IsNodeExpanded = true;
        }

        public WatchNode(string label)
        {
            _label = label;
            IsNodeExpanded = true;
        }

        public WatchNode( string label, bool isListMember, int count )
        {
            _label = isListMember ? "[" + count + "] " + label : label;
            IsNodeExpanded = true;
        }
    }

    public class WatchTreeBranch : ObservableCollection<WatchNode>
    {

    }
}
