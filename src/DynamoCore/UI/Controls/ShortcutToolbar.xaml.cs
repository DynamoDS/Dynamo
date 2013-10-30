using Dynamo.UI.Commands;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Interaction logic for ShortcutToolbar.xaml
    /// </summary>
    public partial class ShortcutToolbar : UserControl
    {
        private ObservableCollection<ShortcutBarItem> shortcutBarItems;
        public ObservableCollection<ShortcutBarItem> ShortcutBarItems
        {
            get { return shortcutBarItems; }
        }
        private ObservableCollection<ShortcutBarItem> shortcutBarRightSideItems;
        public ObservableCollection<ShortcutBarItem> ShortcutBarRightSideItems
        {
            get { return shortcutBarRightSideItems; }
        }

        public ShortcutToolbar()
        {
            shortcutBarItems = new ObservableCollection<ShortcutBarItem>();
            shortcutBarRightSideItems = new ObservableCollection<ShortcutBarItem>();    

            InitializeComponent();
        }
    }

    public partial class ShortcutBarItem
    {
        private string shortcutToolTip;
        private string imgNormalSource;
        private string imgHoverSource;
        private string imgDisabledSource;
        private DelegateCommand shortcutCommand;
        private string shortcutCommandParameter;

        public string ShortcutCommandParameter
        {
            get { return shortcutCommandParameter; }
            set { shortcutCommandParameter = value; }
        }

        public DelegateCommand ShortcutCommand
        {
            get { return shortcutCommand; }
            set { shortcutCommand = value; }
        }

        public string ImgDisabledSource
        {
            get { return imgDisabledSource; }
            set { imgDisabledSource = value; }
        }

        public string ImgHoverSource
        {
            get { return imgHoverSource; }
            set { imgHoverSource = value; }
        }

        public string ImgNormalSource
        {
            get { return imgNormalSource; }
            set { imgNormalSource = value; }
        }

        public string ShortcutToolTip
        {
            get { return shortcutToolTip; }
            set { shortcutToolTip = value; }
        }

        public bool IsEnabled
        {
            get
            {
                if (this.shortcutCommand != null)
                    return this.shortcutCommand.CanExecute(null);

                return false;
            }
        }
    }
}
