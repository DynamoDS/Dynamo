using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.Nodes
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

    public sealed class NullToVisibiltyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? System.Windows.Visibility.Hidden : System.Windows.Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
