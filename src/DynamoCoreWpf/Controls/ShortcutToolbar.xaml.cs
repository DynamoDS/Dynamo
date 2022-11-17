using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dynamo.Models;
using Dynamo.ViewModels;
using Greg.AuthProviders;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Practices.Prism.Commands;

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
        private readonly DynamoModel _viewModel;
        public ICommand SignOutCommand { get; private set; }

        /// <summary>
        /// Construct a ShortcutToolbar.
        /// </summary>
        /// <param name="updateManager"></param>
        public ShortcutToolbar(DynamoModel dm)
        {
            shortcutBarItems = new ObservableCollection<ShortcutBarItem>();
            shortcutBarRightSideItems = new ObservableCollection<ShortcutBarItem>();    

            InitializeComponent();

            DataContext = dm;
            this._viewModel = dm;
            SignOutCommand = new DelegateCommand(SignOut, CanSignOut);
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
            if (_viewModel.AuthenticationManager.LoginState == LoginState.LoggedIn)
            {
                var button = (Button)sender;
                button.ContextMenu.DataContext = button.DataContext;
                button.ContextMenu.IsOpen = true;
            }
            else if (_viewModel.AuthenticationManager.LoginState == LoginState.LoggedOut)
            {
                _viewModel.AuthenticationManager.ToggleLoginState();
                if (_viewModel.AuthenticationManager.IsLoggedIn()) {
                    var tb = (((sender as Button).Content as StackPanel).Children.OfType<TextBlock>().FirstOrDefault() as TextBlock);
                    tb.Text = _viewModel.AuthenticationManager.Username;
                }
            }
        }

        /// <summary>
        /// Toggle current login state
        /// </summary>
        internal void SignOut()
        {
            _viewModel.AuthenticationManager.ToggleLoginStateCommand.Execute(null);
            if (!_viewModel.AuthenticationManager.IsLoggedIn())
            {
                txtSignIn.Text = Wpf.Properties.Resources.SignInButtonText;
            }
        }

        /// <summary>
        /// Check if able to toggle login state
        /// </summary>
        internal bool CanSignOut()
        {
            return _viewModel.AuthenticationManager.CanToggleLoginState();
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
        public Commands.DelegateCommand ShortcutCommand { get; set; }

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
