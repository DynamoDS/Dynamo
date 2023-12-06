using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace Dynamo.PackageManager.UI
{
    public class PackageNameLengthValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is string name && name.TrimEnd().Length > 2)
            {
                // Validation succeeded
                return ValidationResult.ValidResult;
            }

            // Validation failed
            return new ValidationResult(false, Wpf.Properties.Resources.NameNeedMoreCharacters);
        }
    }


    /// <summary>
    /// Interaction logic for PublishPackagePublishPage.xaml
    /// </summary>
    public partial class PublishPackagePublishPage : Page
    {
        private PublishPackageViewModel PublishPackageViewModel;

        public PublishPackagePublishPage()
        {
            InitializeComponent();

            this.DataContextChanged += PublishPackagePublishPage_DataContextChanged;
            this.Tag = "Publish a Package";
        }

        private void PublishPackagePublishPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            PublishPackageViewModel = this.DataContext as PublishPackageViewModel;
        }

        public void LoadEvents()
        {
            this.IsEnabled = true;

            if (previewBrowserControl != null)
            {
                var treeView = previewBrowserControl.customTreeView;

                previewBrowserControl.RefreshCustomTreeView();
                previewBrowserControl.customTreeView_SelectedItemChanged(treeView, null);
            }
        }

        private void HostEntry_CheckStateChanged(object sender, RoutedEventArgs e)
        {
            PublishPackageViewModel.SelectedHosts.Clear();
            PublishPackageViewModel.SelectedHostsString = string.Empty;
            foreach (var host in PublishPackageViewModel.KnownHosts)
            {
                if (host.IsSelected)
                {
                    PublishPackageViewModel.SelectedHosts.Add(host.HostName);
                    PublishPackageViewModel.SelectedHostsString += host.HostName + ", ";
                }
            }
            // Format string since it will be displayed
            PublishPackageViewModel.SelectedHostsString = PublishPackageViewModel.SelectedHostsString.Trim().TrimEnd(',');
        }

        /// <summary>
        /// Navigates to a predefined URL in the user's default browser.
        /// Currently used to make the MIT license text a clickable link.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        public void Dispose()
        {
            this.PublishPackageViewModel = null;
            this.DataContextChanged -= PublishPackagePublishPage_DataContextChanged;
            this.previewBrowserControl?.Dispose();
        }

        private void NavigationButton_Click(object sender, RoutedEventArgs e)
        {
            var pmPublishControl = GetUserControlFromPage(this) as PackageManagerPublishControl;
            if (pmPublishControl != null)
            {
                pmPublishControl.BreadcrumbButton_Click(sender, e);
            }
        }

        public static UserControl GetUserControlFromPage(Page page)
        {
            if (page == null)
            {
                return null;
            }

            // Get the parent of the Page (a Frame or NavigationWindow)
            DependencyObject parent = VisualTreeHelper.GetParent(page);

            while (parent != null)
            {
                if (parent is UserControl control)
                {
                    return control;
                }

                // Check the parent's parent
                parent = VisualTreeHelper.GetParent(parent);
            }

            return null; // Page is not hosted in a Window
        }

        private void previewBrowserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (previewBrowserControl != null)
            {
                previewBrowserControl.RefreshCustomTreeView();
            }
        }

        private void textBoxInput_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            // Prevents text starting with a space
            if (e.Key == System.Windows.Input.Key.Space && string.IsNullOrWhiteSpace(textBox.Text))
            {
                e.Handled = true; 
            }
        }
    }
}
