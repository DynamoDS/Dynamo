using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public Window Owner { get; set; }
        public PublishPackageViewModel PublishPackageViewModel { get; set; }
        private Dictionary<int, Page> PublishPages { get; set; }
        private Dictionary<int, DockPanel> NavButtonStacks { get; set; }
        public ObservableCollection<string> Breadcrumbs { get; } = new ObservableCollection<string>();

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

            this.mainFrame.NavigationService.Navigate(PublishPages[0]);
            this.Loaded -= InitializeContext;
        }

        public void Dispose()
        {
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

            Breadcrumbs?.Clear();
            
            this.DataContextChanged -= PackageManagerPublishControl_DataContextChanged;
        }

        private void InitializePages()
        {
            if ( PublishPages != null ) { PublishPages.Clear(); }
            PublishPages = new Dictionary<int, Page>();

            PublishPages[0] = new PublishPackagePublishPage();
            PublishPages[1] = new PublishPackageSelectPage();
            PublishPages[2] = new PublishPackagePreviewPage();
            PublishPages[3] = new PublishPackageFinishPage();

            foreach(var pageEntry in PublishPages) { pageEntry.Value.DataContext = PublishPackageViewModel; }

            Breadcrumbs.Clear();
            Breadcrumbs.Add((string)PublishPages[0].Tag); // Initial breadcrumb

            NavButtonStacks = new Dictionary<int, DockPanel>();

            NavButtonStacks[0] = this.PublishPageButtonStack;
            NavButtonStacks[1] = this.SelectPageButtonStack;
            NavButtonStacks[2] = this.PreviewPageButtonStack;
            NavButtonStacks[3] = this.FinishPageButtonStack;
        }

        private void PackageViewModelOnPublishSuccess(PublishPackageViewModel sender)
        {
            if (PublishPages == null) return;

            statusLabel.Visibility = Visibility.Collapsed;

            currentPage = 3;

            // Trigger load events manually for the Finish Page to get the count of published files
            if (PublishPages[currentPage] is PublishPackageFinishPage)
                (PublishPages[currentPage] as PublishPackageFinishPage).LoadEvents();

            this.mainFrame.NavigationService.Navigate(PublishPages[currentPage]);
            this.breadcrumbsNavigation.Visibility = Visibility.Collapsed;

            ToggleButtonRowVisibility(currentPage);
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
            }
        }

        private void OnMoreInfoClicked(object sender, MouseButtonEventArgs e)
        {
            PublishPackageViewModel.DynamoViewModel.OpenDocumentationLinkCommand.Execute(new OpenDocumentationLinkEventArgs(new Uri(Wpf.Properties.Resources.PublishPackageMoreInfoFile, UriKind.Relative)));
        }

        private int currentPage = 0;

        internal void BreadcrumbButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                switch (button.Tag)
                {
                    // PublishPackagePublishPage
                    case var value when value == (string)PublishPages[0].Tag:
                        currentPage = 0;
                        int stepIndex = Breadcrumbs.IndexOf((string)PublishPages[0].Tag);
                        for (int i = Breadcrumbs.Count - 1; i > stepIndex; i--)
                        {
                            Breadcrumbs.RemoveAt(i);
                        }
                        this.mainFrame.NavigationService.Navigate(PublishPages[currentPage]);
                        this.breadcrumbsNavigation.Visibility = Visibility.Collapsed;
                        ToggleButtonRowVisibility(0);
                        break;
                    // PublishPackageSelectPage
                    case var value when value == (string)PublishPages[1].Tag:
                        currentPage = 1;
                        stepIndex = Breadcrumbs.IndexOf((string)PublishPages[currentPage].Tag);
                        for (int i = Breadcrumbs.Count - 1; i > stepIndex; i--)
                        {
                            Breadcrumbs.RemoveAt(i);
                        }
                        this.mainFrame.NavigationService.Navigate(PublishPages[currentPage]);
                        this.breadcrumbsNavigation.Visibility = Visibility.Visible;
                        ToggleButtonRowVisibility(1);
                        break;
                    // PublishPackagePreviewPage
                    case var value when value == (string)PublishPages[2].Tag:
                        if (!Breadcrumbs.Contains((string)PublishPages[2].Tag)) Breadcrumbs.Add((string)PublishPages[2].Tag);
                        currentPage = 2;
                        stepIndex = Breadcrumbs.IndexOf((string)PublishPages[currentPage].Tag);
                        for (int i = Breadcrumbs.Count - 1; i > stepIndex; i--)
                        {
                            Breadcrumbs.RemoveAt(i);
                        }
                        this.mainFrame.NavigationService.Navigate(PublishPages[currentPage]);
                        this.breadcrumbsNavigation.Visibility = Visibility.Visible;
                        ToggleButtonRowVisibility(2);
                        break;
                    case "Back":
                        --currentPage;
                        stepIndex = Breadcrumbs.IndexOf((string)PublishPages[currentPage].Tag);
                        for (int i = Breadcrumbs.Count - 1; i > stepIndex; i--) 
                        {
                            Breadcrumbs.RemoveAt(i);
                        }
                        this.mainFrame.NavigationService.Navigate(PublishPages[currentPage]);
                        this.breadcrumbsNavigation.Visibility = currentPage == 0 ? Visibility.Collapsed : Visibility.Visible;
                        ToggleButtonRowVisibility(currentPage);
                        break;
                    case "Next":
                        ++currentPage;
                        if (!Breadcrumbs.Contains((string)PublishPages[currentPage].Tag)) Breadcrumbs.Add((string)PublishPages[currentPage].Tag);
                        stepIndex = Breadcrumbs.IndexOf((string)PublishPages[currentPage].Tag);
                        if(stepIndex > 0)
                        {
                            for (int i = Breadcrumbs.Count - 1; i > stepIndex; i--)
                            {
                                Breadcrumbs.RemoveAt(i);
                            }
                        }
                        this.mainFrame.NavigationService.Navigate(PublishPages[currentPage]);
                        this.breadcrumbsNavigation.Visibility = Visibility.Visible;
                        ToggleButtonRowVisibility(currentPage);
                        break;
                    case "Finish":
                    case "Done":
                        statusLabel.Visibility = Visibility.Visible;
                        currentPage = 0;
                        stepIndex = Breadcrumbs.IndexOf((string)PublishPages[currentPage].Tag);
                        for (int i = Breadcrumbs.Count - 1; i > stepIndex; i--)
                        {
                            Breadcrumbs.RemoveAt(i);
                        }
                        this.mainFrame.NavigationService.Navigate(PublishPages[currentPage]);
                        this.breadcrumbsNavigation.Visibility = currentPage == 0 ? Visibility.Collapsed : Visibility.Visible;
                        ToggleButtonRowVisibility(currentPage);
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
            if(NavButtonStacks.TryGetValue(page, out var buttonstack))
            {
                buttonstack.Visibility = Visibility.Visible;
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
            currentPage = 0;
            int stepIndex = Breadcrumbs.IndexOf((string)PublishPages[0].Tag);
            for (int i = Breadcrumbs.Count - 1; i > stepIndex; i--)
            {
                Breadcrumbs.RemoveAt(i);
            }
            this.mainFrame.NavigationService.Navigate(PublishPages[currentPage]);
            this.breadcrumbsNavigation.Visibility = Visibility.Collapsed;
            ToggleButtonRowVisibility(0);
        }
    }
}
