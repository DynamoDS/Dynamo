using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dynamo.Controls;
using Dynamo.UI.Commands;
using Dynamo.Updates;
using Dynamo.ViewModels;
using Dynamo.Wpf.Views.Notifications;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// An object which provides the data for the shortcut toolbar.
    /// </summary>
    public partial class ShortcutToolbar : UserControl
    {
        private readonly ObservableCollection<ShortcutBarItem> shortcutBarItems;
        private readonly ObservableCollection<ShortcutBarItem> shortcutBarRightSideItems;

        private NotificationUI notificationUIPopup;

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

        /// <summary>
        /// Construct a ShortcutToolbar.
        /// </summary>
        /// <param name="updateManager"></param>
        public ShortcutToolbar(DynamoView dynamoView, IUpdateManager updateManager)
        {
            shortcutBarItems = new ObservableCollection<ShortcutBarItem>();
            shortcutBarRightSideItems = new ObservableCollection<ShortcutBarItem>();

            notificationUIPopup = new NotificationUI(null);
            notificationUIPopup.IsOpen = false;
            notificationUIPopup.PlacementTarget = notificationsButton;
            notificationUIPopup.Placement = PlacementMode.Bottom;
            notificationUIPopup.HorizontalOffset = -300;
            notificationUIPopup.VerticalOffset= 30;

            notificationUIPopup.UpdatePopupLocation();

            InitializeComponent();

            UpdateControl.DataContext = updateManager;

            dynamoView.SizeChanged += DynamoView_SizeChanged;
            dynamoView.LocationChanged += DynamoView_LocationChanged;
        }

        private void DynamoView_LocationChanged(object sender, EventArgs e)
        {
            notificationUIPopup.PlacementTarget = notificationsButton;
            notificationUIPopup.Placement = PlacementMode.Bottom;
            notificationUIPopup.UpdatePopupLocation();
        }

        private void DynamoView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            notificationUIPopup.PlacementTarget = notificationsButton;
            notificationUIPopup.Placement = PlacementMode.Bottom;
            notificationUIPopup.UpdatePopupLocation();
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            notificationUIPopup.IsOpen = !notificationUIPopup.IsOpen;
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

    internal partial class NotificationsShortcutBarItem : ShortcutBarItem
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

        public NotificationsShortcutBarItem(DynamoViewModel viewModel)
        {

        }
    }
}
