using System.IO;
using System.Windows;
using System.Windows.Input;
using Dynamo.UpdateManager;
using Dynamo.ViewModels;

namespace Dynamo.UI.Views
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        string eulaFilePath = string.Empty;
        bool ignoreClose = false;
        DynamoLogger logger = null;

        public AboutWindow(DynamoLogger logger, AboutWindowViewModel model)
        {
            InitializeComponent();
            this.logger = logger;
            this.InstallNewUpdate = false;
            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);
            this.DataContext = model;
        }

        public bool InstallNewUpdate { get; private set; }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //Request a check for update version info
            DisplayVersionInformation(null);
            UpdateManager.UpdateManager.Instance.UpdateDownloaded += new UpdateDownloadedEventHandler(OnUpdatePackageDownloaded);
            
            UpdateManager.UpdateManager.Instance.CheckForProductUpdate();

            string executingAssemblyPathName = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string rootModuleDirectory = System.IO.Path.GetDirectoryName(executingAssemblyPathName);
            eulaFilePath = System.IO.Path.Combine(rootModuleDirectory, "DesignScriptLauncher.exe");
            if (!File.Exists(eulaFilePath))
                ViewLicenseTextBlock.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        private void OnWindowClick(object sender, MouseButtonEventArgs e)
        {
            if (ignoreClose)
                ignoreClose = false;
            else
                Close();
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.ignoreClose = true;
        }

        private void OnClickLink(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/DynamoDS/Dynamo");
        }

        private void OnViewLicense(object sender, RoutedEventArgs e)
        {
            if (File.Exists(eulaFilePath))
                System.Diagnostics.Process.Start(eulaFilePath, "EULA");
        }

        private void OnUpdatePackageDownloaded(object sender, UpdateDownloadedEventArgs e)
        {
            DisplayVersionInformation(e);
        }

        private void OnUpdateInfoMouseUp(object sender, MouseButtonEventArgs e)
        {
            logger.LogInfo("AboutWindow-OnUpdateInfoMouseUp", "AboutWindow-OnUpdateInfoMouseUp");
            this.InstallNewUpdate = true;
            this.Close();
        }

        private void DisplayVersionInformation(UpdateDownloadedEventArgs e)
        {
            if ((null != e) && e.UpdateAvailable)
            {
                this.UpdateInfo.Cursor = Cursors.Hand;
                this.UpdateInfo.MouseUp += new MouseButtonEventHandler(OnUpdateInfoMouseUp);
            }
            else
            {
                this.UpdateInfo.Cursor = Cursors.Arrow;
                this.UpdateInfo.MouseUp -= new MouseButtonEventHandler(OnUpdateInfoMouseUp);
            }
        }
    }
}