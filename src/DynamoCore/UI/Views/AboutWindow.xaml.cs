using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Dynamo.Interfaces;
using Dynamo.UpdateManager;
using Dynamo.Utilities;
using Dynamo.ViewModels;

namespace Dynamo.UI.Views
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        bool ignoreClose;
        ILogger logger;

        public AboutWindow(ILogger logger, DynamoViewModel model)
        {
            InitializeComponent();
            this.logger = logger;
            InstallNewUpdate = false;
            PreviewKeyDown += new KeyEventHandler(HandleEsc);
            DataContext = model;
        }

        public bool InstallNewUpdate { get; private set; }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //Request a check for update version info
            DisplayVersionInformation(null);
            var um = dynSettings.Controller.UpdateManager;
            um.UpdateDownloaded += OnUpdatePackageDownloaded;
            dynSettings.Controller.UpdateManager.CheckForProductUpdate(new UpdateRequest(new Uri(Configurations.UpdateDownloadLocation), DynamoLogger.Instance, um.UpdateDataAvailable));
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

        private void OnUpdatePackageDownloaded(object sender, UpdateDownloadedEventArgs e)
        {
            DisplayVersionInformation(e);
        }

        private void OnUpdateInfoMouseUp(object sender, MouseButtonEventArgs e)
        {
            logger.Log("AboutWindow-OnUpdateInfoMouseUp", "AboutWindow-OnUpdateInfoMouseUp");
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

    //http://www.rhyous.com/2011/08/01/loading-a-richtextbox-from-an-rtf-file-using-binding-or-a-richtextfile-control/
    internal class RichTextFile : RichTextBox
    {
        public RichTextFile()
        {
            AddHandler(Hyperlink.RequestNavigateEvent, new RoutedEventHandler(HandleHyperlinkClick));
        }

        private void HandleHyperlinkClick(object inSender, RoutedEventArgs inArgs)
        {
            if (OpenLinksInBrowser)
            {
                Hyperlink link = inArgs.Source as Hyperlink;
                if (link != null)
                {
                    Process.Start(link.NavigateUri.ToString());
                    inArgs.Handled = true;
                }
            }
        }

        #region Properties
        public bool OpenLinksInBrowser { get; set; }

        public String File
        {
            get { return (String)GetValue(FileProperty); }
            set { SetValue(FileProperty, value); }
        }

        public static DependencyProperty FileProperty =
            DependencyProperty.Register("File", typeof(String), typeof(RichTextFile),
            new PropertyMetadata(OnFileChanged));

        private static void OnFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RichTextFile rtf = d as RichTextFile;
            if (rtf == null)
                return;

            ReadFile(rtf.File, rtf.Document);
        }
        #endregion

        #region Functions
        private static void ReadFile(string inFilename, FlowDocument inFlowDocument)
        {
            if (System.IO.File.Exists(inFilename))
            {
                TextRange range = new TextRange(inFlowDocument.ContentStart, inFlowDocument.ContentEnd);
                FileStream fStream = new FileStream(inFilename, FileMode.Open, FileAccess.Read, FileShare.Read);

                range.Load(fStream, DataFormats.Rtf);
                fStream.Close();
            }
        }
        #endregion
    }
}