using Dynamo.UI.Commands;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using Microsoft.Practices.Prism.ViewModel;

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

    public partial class ShortcutBarItem : NotificationObject
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
    }
}
