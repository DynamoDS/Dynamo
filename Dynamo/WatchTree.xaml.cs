using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Dynamo.Elements
{
    //http://blogs.msdn.com/b/chkoenig/archive/2008/05/24/hierarchical-databinding-in-wpf.aspx

    /// <summary>
    /// Interaction logic for WatchTree.xaml
    /// </summary>
    public partial class WatchTree : UserControl
    {
        public WatchTree()
        {
            InitializeComponent();
        }
    }

    public class WatchNode : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify(string propName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        WatchTreeBranch _children = new WatchTreeBranch();
        string _label;

        public WatchTreeBranch Children
        {
            get { return _children; }
            set { 
                _children = value;
                Notify("Children");
            }
        }
        public string NodeLabel
        {
            get { return _label; }
            set
            {
                _label = value;
                Notify("NodeLabel");
            }
        }

        public WatchNode()
        {
        }
        public WatchNode(string label)
        {
            _label = label;
        }
    }

    public class WatchTreeBranch:ObservableCollection<WatchNode>
    {
        
    }

}
