using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

using Dynamo.ViewModels;

namespace Dynamo.UI.Views
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        bool ignoreClose;

        public AboutWindow(DynamoViewModel dynamoViewModel)
        {
            InitializeComponent();
            InstallNewUpdate = false;
            PreviewKeyDown += new KeyEventHandler(HandleEsc);
            DataContext = dynamoViewModel;

#if ENABLE_DYNAMO_SCHEDULER
            // SCHEDULER: Temporary way to tell that scheduler is enabled.
            VersionNumber.Foreground = new SolidColorBrush(Colors.Yellow);
#endif
        }

        public bool InstallNewUpdate { get; private set; }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.ignoreClose = true;
        }

        private void OnClickLink(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/DynamoDS/Dynamo");
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
                FileStream fStream = new FileStream(inFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                range.Load(fStream, DataFormats.Rtf);
                fStream.Close();
            }
        }
        #endregion
    }
}