using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.ViewModels;
using Dynamo.Wpf.Utilities;
using DynamoUtilities;
using Views.PackageManager.Pages;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for PackageManagerPublishControl.xaml
    /// </summary>
    public partial class PackageManagerPublishControl : UserControl
    {
        private int currentPage = 0;

        public Window Owner { get; set; }
        public PublishPackageViewModel PublishPackageViewModel { get; set; }
        private Dictionary<int, Page> PublishPages { get; set; }
        private Dictionary<int, DockPanel> NavButtonStacks { get; set; }

        public PackageManagerPublishControl()
        {
            InitializeComponent();

            this.Loaded += InitializeContext;
            this.DataContextChanged += PackageManagerPublishControl_DataContextChanged;
        }

        private void PackageManagerPublishControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(this.DataContext is PublishPackageViewModel)) return;

            ResetDataContext();
            SetDataContext();
        }

        private void InitializeContext(object sender, RoutedEventArgs e)
        {
            SetDataContext();

            this.Loaded -= InitializeContext;
        }

        private void ResetDataContext()
        {
            if (PublishPackageViewModel != null)
            {
                PublishPackageViewModel.PublishSuccess -= PackageViewModelOnPublishSuccess;
                PublishPackageViewModel.RequestShowFolderBrowserDialog -= OnRequestShowFolderBrowserDialog;
            }

            PublishPackageViewModel = null; 
        }

        private void SetDataContext()
        {
            // Set the owner of this user control
            this.Owner = Window.GetWindow(this);

            PublishPackageViewModel = this.DataContext as PublishPackageViewModel;
            PublishPackageViewModel.Owner = this.Owner;

            if (PublishPackageViewModel != null)
            {
                PublishPackageViewModel.PublishSuccess += PackageViewModelOnPublishSuccess;
                PublishPackageViewModel.RequestShowFolderBrowserDialog += OnRequestShowFolderBrowserDialog;
            }

            Logging.Analytics.TrackScreenView("PackageManager");

            InitializePages();
            HandlePageChanged(0);   // Navigate to the starting page

            this.Loaded -= InitializeContext;
        }

        private void OnStepChanged(object sender, int stepNumber)
        {
            ChangePage(stepNumber - 1);   // Account for step numbers starting from 1 in the wizard control
        }

        private void ChangePage(int page)
        {
            HandlePageChanged(page);
        }

        public void Dispose()
        {
            Dynamo.Logging.Analytics.TrackEvent(
                Actions.Close,
                Categories.PackageManagerOperations);

            if(PublishPackageViewModel != null )    
            {
                PublishPackageViewModel.PublishSuccess -= PackageViewModelOnPublishSuccess;
                PublishPackageViewModel.RequestShowFolderBrowserDialog -= OnRequestShowFolderBrowserDialog;

            }

            this.Loaded -= InitializeContext;

            DisposePages();

            PublishPackageViewModel = null;
            PublishPages = null;
            NavButtonStacks = null;

            this.DataContextChanged -= PackageManagerPublishControl_DataContextChanged;
        }

        private void InitializePages()
        {
            if ( PublishPages != null ) { PublishPages.Clear(); }

            PublishPages = new Dictionary<int, Page>();

            PublishPages[0] = new PublishPackagePublishPage();
            PublishPages[1] = new PublishPackageCompatibilityPage();
            PublishPages[2] = new PublishPackageSelectPage();
            PublishPages[3] = new PublishPackagePreviewPage();
            PublishPages[4] = new PublishPackageConfirmPage();
            PublishPages[5] = new PublishPackageFinishPage();

            foreach (var pageEntry in PublishPages) { pageEntry.Value.DataContext = PublishPackageViewModel; }


            NavButtonStacks = new Dictionary<int, DockPanel>();

            NavButtonStacks[0] = this.PublishPageButtonStack;
            NavButtonStacks[1] = this.CompatibilityPageButtonStack;
            NavButtonStacks[2] = this.SelectPageButtonStack;
            NavButtonStacks[3] = this.PreviewPageButtonStack;
            NavButtonStacks[4] = this.ConfirmPageButtonStack;
            NavButtonStacks[5] = this.FinishPageButtonStack;
        }

        private void PackageViewModelOnPublishSuccess(PublishPackageViewModel sender)
        {
            if (PublishPages == null) return;

            HandlePageChanged(5);

            //statusLabel.Visibility = Visibility.Collapsed;

            // Trigger load events manually for the Finish Page to get the count of published files
            if (PublishPages[currentPage] is PublishPackageFinishPage)
                (PublishPages[currentPage] as PublishPackageFinishPage).LoadEvents();

        }

        private void OnRequestShowFolderBrowserDialog(object sender, PackagePathEventArgs e)
        {
            e.Cancel = true;

            // Handle for the case, initialPath does not exist.
            var errorCannotCreateFolder = PathHelper.CreateFolderIfNotExist(e.Path);
            if (errorCannotCreateFolder == null)
            {
                var dialog = new DynamoFolderBrowserDialog
                {
                    SelectedPath = e.Path
                };

                dialog.Owner = Owner;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    e.Cancel = false;
                    e.Path = dialog.SelectedPath;
                }
            }
            else if (!String.IsNullOrEmpty(e.Path))
            {
                e.Cancel = false;
            }

        }

        /// <summary>
        /// Allows for the dragging of this custom-styled window. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PublishPackageView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Drag functionality when the TitleBar is clicked with the left button and dragged to another place
            if (e.ChangedButton == MouseButton.Left)
            {
                this.Owner.DragMove();
                Dynamo.Logging.Analytics.TrackEvent(
                    Actions.Move,
                    Categories.PackageManagerOperations);
            }
        }

        private void OnMoreInfoClicked(object sender, MouseButtonEventArgs e)
        {
            PublishPackageViewModel.DynamoViewModel.OpenDocumentationLinkCommand.Execute(new OpenDocumentationLinkEventArgs(new Uri(Wpf.Properties.Resources.PublishPackageMoreInfoFile, UriKind.Relative)));
        }


        internal void BreadcrumbButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                switch (button.Tag)
                {                    
                    case "Back":
                        HandlePageChanged(currentPage - 1);
                        break;
                    case "Next":
                        HandlePageChanged(currentPage + 1);
                        break;
                    case "Finish":
                    case "Done":
                        //statusLabel.Visibility = Visibility.Visible;
                        HandlePageChanged(0);
                        break;
                }
            }
        }

        private void ToggleButtonRowVisibility(int page)
        {
            // Reset all buttons visibility
            foreach (var stackPanel in NavButtonStacks.Values)
            {
                stackPanel.Visibility = Visibility.Collapsed;
            }

            // Set appropriate buttonStack visible
            if(NavButtonStacks.TryGetValue(page, out var buttonStack))
            {
                buttonStack.Visibility = Visibility.Visible;
            }
        }

        private void DisposePages()
        {
            if (PublishPages == null || !PublishPages.Any()) return;

            foreach(var page in PublishPages.Values)
            {
                if (page is PublishPackagePublishPage)
                    (page as PublishPackagePublishPage).Dispose();
                if (page is PublishPackageSelectPage)
                    (page as PublishPackageSelectPage).Dispose();
                if (page is PublishPackagePreviewPage)
                    (page as PublishPackagePreviewPage).Dispose();
                if (page is PublishPackageFinishPage)
                    (page as PublishPackageFinishPage).Dispose();
            }
        }

        private void mainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            Page navigatedPage = e.Content as Page;

            if (navigatedPage == null || PublishPages == null || !PublishPages.Any()) return;

            PublishPages.Values.ToList().ForEach(page => { page.IsEnabled = false; });

            if (navigatedPage != null)
            {
                if (navigatedPage is PublishPackagePublishPage)
                    (navigatedPage as PublishPackagePublishPage).LoadEvents();
                if (navigatedPage is PublishPackageSelectPage)
                    (navigatedPage as PublishPackageSelectPage).LoadEvents();
                if (navigatedPage is PublishPackagePreviewPage)
                    (navigatedPage as PublishPackagePreviewPage).LoadEvents();
                if (navigatedPage is PublishPackageFinishPage)
                    (navigatedPage as PublishPackageFinishPage).IsEnabled = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (!PublishPackageViewModel.AnyUserChanges()) return;

            MessageBoxResult response = DynamoModel.IsTestMode ? MessageBoxResult.OK :
               MessageBoxService.Show(
                   Owner,
                   Wpf.Properties.Resources.ResetChangesWarningPopupMessage,
                   Wpf.Properties.Resources.DiscardChangesWarningPopupCaption,
                   MessageBoxButton.OKCancel,
                   MessageBoxImage.Warning);

            if (response == MessageBoxResult.OK)
            {
                PublishPackageViewModel.CancelCommand.Execute();
            }
        }

        /// <summary>
        /// Navigates back to the starting page 
        /// </summary>
        internal void ResetPageOrder()
        {
            HandlePageChanged(0);
        }

        private void HandlePageChanged(int page)
        {
            currentPage = page;
            this.mainFrame.NavigationService.Navigate(PublishPages[currentPage]);
            ToggleButtonRowVisibility(page);

            if (wizardControl.CurrentStep != currentPage + 1)
                wizardControl.CurrentStep = (currentPage + 1);
        }
    }
}
