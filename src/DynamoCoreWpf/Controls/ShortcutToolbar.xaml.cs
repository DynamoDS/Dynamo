using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels.Core;
using Microsoft.Practices.Prism.ViewModel;
using Greg.AuthProviders;
using System.Linq;
using System.Windows;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// An object which provides the data for the shortcut toolbar.
    /// </summary>
    public partial class ShortcutToolbar : UserControl
    {
        private readonly ObservableCollection<ShortcutBarItem> shortcutBarItems;
        private readonly ObservableCollection<ShortcutBarItem> shortcutBarRightSideItems;

        /// <summary>
        /// A collection of <see cref="ShortcutBarItem"/>.
        /// </summary>
        public ObservableCollection<ShortcutBarItem> ShortcutBarItems
        {
            get { return shortcutBarItems; }
        }

        /// <summary>
        /// A collection of <see cref="ShortcutBarItems"/> for the right hand side of the shortcut bar.
        /// </summary>
        public ObservableCollection<ShortcutBarItem> ShortcutBarRightSideItems
        {
            get { return shortcutBarRightSideItems; }
        }
        private readonly Core.AuthenticationManager authManager;

        /// <summary>
        /// Construct a ShortcutToolbar.
        /// </summary>
        /// <param name="dynamoViewModel"></param>
        public ShortcutToolbar(DynamoViewModel dynamoViewModel)
        {
            shortcutBarItems = new ObservableCollection<ShortcutBarItem>();
            shortcutBarRightSideItems = new ObservableCollection<ShortcutBarItem>();    

            InitializeComponent();         

            var shortcutToolbar = new ShortcutToolbarViewModel(dynamoViewModel);
            DataContext = shortcutToolbar;
            authManager = dynamoViewModel.Model.AuthenticationManager;
            if (authManager.IsLoggedIn())
            {
                authManager.LoginStateChanged += SignOutHandler;
            }
            else {
                logoutOption.Visibility = Visibility.Collapsed;
            }
        }

        private void SignOutHandler(LoginState status)
        {
            if (status == LoginState.LoggedOut)
            {
                LoginButton.ToolTip = Wpf.Properties.Resources.SignInButtonContentToolTip;
                txtSignIn.Text = Wpf.Properties.Resources.SignInButtonText;
                logoutOption.Visibility = Visibility.Collapsed;
                authManager.LoginStateChanged -= SignOutHandler;
            }
        }

        private void exportMenu_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.HeaderText.FontFamily = SharedDictionaryManager.DynamoModernDictionary["ArtifaktElementRegular"] as FontFamily;
            this.Icon.Source = new BitmapImage(new System.Uri(@"pack://application:,,,/DynamoCoreWpf;component/UI/Images/image-icon.png"));
        }

        private void exportMenu_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.HeaderText.FontFamily = SharedDictionaryManager.DynamoModernDictionary["ArtifaktElementRegular"] as FontFamily;
            this.Icon.Source = new BitmapImage(new System.Uri(@"pack://application:,,,/DynamoCoreWpf;component/UI/Images/image-icon-default.png"));
        }

        private void LoginButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (authManager.LoginState == LoginState.LoggedIn)
            {
                var button = (Button)sender;
                MenuItem mi = button.Parent as MenuItem;
                if (mi != null)
                {
                    mi.IsSubmenuOpen = !mi.IsSubmenuOpen;
                }
            }
            else if (authManager.LoginState == LoginState.LoggedOut)
            {
                authManager.ToggleLoginState(null);
                if (authManager.IsLoggedIn()) {
                    var tb = (((sender as Button).Content as StackPanel).Children.OfType<TextBlock>().FirstOrDefault() as TextBlock);
                    tb.Text = authManager.Username;
                    logoutOption.Visibility = Visibility.Visible;
                    LoginButton.ToolTip = null;
                    authManager.LoginStateChanged += SignOutHandler;
                }
            }
        }
    }

    /// <summary>
    /// An object which provides the data for an item in the shortcut toolbar.
    /// </summary>
    public partial class ShortcutBarItem : NotificationObject
    {
        protected string shortcutToolTip;

        /// <summary>
        /// A parameter sent to the ShortcutCommand.
        /// </summary>
        public string ShortcutCommandParameter { get; set; }

        /// <summary>
        /// The Command that will be executed by this shortcut item.
        /// </summary>
        public DelegateCommand ShortcutCommand { get; set; }

        /// <summary>
        /// The path to the image for the disabled state of this shortcut item.
        /// </summary>
        public string ImgDisabledSource { get; set; }

        /// <summary>
        /// The path to the image for the hover state of this shortcut item.
        /// </summary>
        public string ImgHoverSource { get; set; }

        /// <summary>
        /// The path to the image for the normal state of this shortcut item.
        /// </summary>
        public string ImgNormalSource { get; set; }

        /// <summary>
        /// The tooltip for this shortcut item.
        /// </summary>
        public virtual string ShortcutToolTip
        {
            get { return shortcutToolTip; }
            set { shortcutToolTip = value; }
        }
    }

    internal partial class ImageExportShortcutBarItem : ShortcutBarItem
    {
        private readonly DynamoViewModel vm;

        public override string ShortcutToolTip
        {
            get
            {
                return vm.BackgroundPreviewViewModel == null || !vm.BackgroundPreviewViewModel.CanNavigateBackground
                    ? Wpf.Properties.Resources.DynamoViewToolbarExportButtonTooltip
                    : Wpf.Properties.Resources.DynamoViewToolbarExport3DButtonTooltip;
            }
            set
            {
                shortcutToolTip = value;
            }
        }

        public ImageExportShortcutBarItem(DynamoViewModel viewModel)
        {
            vm = viewModel;
            vm.BackgroundPreviewViewModel.PropertyChanged += BackgroundPreviewViewModel_PropertyChanged;
        }

        private void BackgroundPreviewViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "CanNavigateBackground") return;
            RaisePropertyChanged("ShortcutToolTip");
        }
    }
}
