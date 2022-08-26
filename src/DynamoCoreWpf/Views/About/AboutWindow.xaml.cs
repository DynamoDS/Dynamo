using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Dynamo.ViewModels;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace Dynamo.UI.Views
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        bool ignoreClose;
        private WebView2 htmlLicenseView;
        static readonly string HTML_IMAGE_PATH_PREFIX = @"http://";
        private const string ABOUT_BLANK_URI = "about:blank";

        public AboutWindow(DynamoViewModel dynamoViewModel)
        {
            InitializeComponent();
            InstallNewUpdate = false;
            PreviewKeyDown += new KeyEventHandler(HandleEsc);
            DataContext = dynamoViewModel;

            TitleTextBlock.Text = string.Format(Dynamo.Wpf.Properties.Resources.AboutWindowTitle, dynamoViewModel.BrandingResourceProvider.ProductName);
            Title = string.Format(Dynamo.Wpf.Properties.Resources.AboutWindowTitle, dynamoViewModel.BrandingResourceProvider.ProductName);
            DynamoWebsiteButton.Content = string.Format(Dynamo.Wpf.Properties.Resources.AboutWindowDynamoWebsiteButton, dynamoViewModel.BrandingResourceProvider.ProductName);

            this.Loaded += AboutWindow_Loaded;

        }

        private void AboutWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadHTMLAboutBoxContent();
        }

        public bool InstallNewUpdate { get; private set; }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        private void LoadHTMLAboutBoxContent()
        {
            string executingAssemblyPathName = Assembly.GetExecutingAssembly().Location;
            string rootModuleDirectory = Path.GetDirectoryName(executingAssemblyPathName);
            var htmlLicensePath = Path.Combine(rootModuleDirectory, "third_party_licenses_html");
            var files = Directory.GetFiles(htmlLicensePath, "*.html");
            var head = @"<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""utf-8"">
<title>title</title>
</head>
<style>
/* total width */
body::-webkit-scrollbar {
    background-color: #fff;
    width: 16px;
}

/* background of the scrollbar except button or resizer */
body::-webkit-scrollbar-track {
    background-color: #fff;
}

/* scrollbar itself */
body::-webkit-scrollbar-thumb {
    background-color: #babac0;
    border-radius: 16px;
    border: 4px solid #fff;
}

/* set button(top and bottom of the scrollbar) */
body::-webkit-scrollbar-button {
    display:none;
}
</style>
<body style=""padding-left:15px; background-color: #F0F0F0; font-family: 'Artifakt Element', 'Open Sans';font-size: small; "">
";
            var footer = @"
</body>
</html>";

            var finalHtml = head;

             foreach (var file in files)
             {
                 var licContent = File.ReadAllText(file);
                //TODO range check.
                var licName = new FileInfo(file).Name.Split('_')[0];
                var licWrapped = $@"<details> <summary>{licName}</summary> {licContent} </details>";
                finalHtml += licWrapped;
            }
            finalHtml += footer;
            htmlLicenseView = ((browserparent as Border)?.Child as WebView2);
            if(htmlLicenseView != null)
            {
                InitializeAsync(htmlLicenseView, finalHtml);
            }
        }

        async void InitializeAsync(WebView2 browser, string htmlContent)
        {
            //Initialize the CoreWebView2 component otherwise we can't navigate to a web page
            await browser.EnsureCoreWebView2Async();

            // Context menu disabled
            browser.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            browser.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            browser.CoreWebView2.NavigationStarting += ShouldAllowNavigation;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                browser.NavigateToString(htmlContent);
            }));
        }

        private void CoreWebView2_NewWindowRequested(object sender, global::Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs e)
        {
            try
            {
                Process.Start(e.Uri);
            }
            catch(Exception ex)
            {
                if (DataContext is DynamoViewModel dvm)
                {
                    dvm.Model.Logger.Log(ex);
                }
            }
            e.Handled = true;
        }

        //TODO refactor extensions to share this.
        /// <summary>
        /// Redirect the user to the browser if they press a link in the browser.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShouldAllowNavigation(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            // if is not an URL then we should return otherwise it will crash when trying to open the URL in the default Web Browser
            if (!e.Uri.StartsWith(HTML_IMAGE_PATH_PREFIX.Substring(0, 4)))
            {
                return;
            }

            // we never set the uri if navigating to a local document, so safe to navigate
            if (e.Uri == null)
                return;

            // we want to cancel navigation when a clicked link would navigate 
            // away from the page the ViewModel wants to display
            var isAboutBlankLink = e.Uri.ToString().Equals(ABOUT_BLANK_URI);
            var isRemoteLinkFromLocalDocument = e.Uri.ToLower().StartsWith("@https://") || e.Uri.ToLower().StartsWith(HTML_IMAGE_PATH_PREFIX);

            if (isAboutBlankLink || isRemoteLinkFromLocalDocument)
            {
                // in either of these two cases, cancel the navigation 
                // and redirect it to a new process that starts the default OS browser
                e.Cancel = true;
                Process.Start(new ProcessStartInfo(e.Uri));
            }
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.ignoreClose = true;
        }

        private void OnClickLink(object sender, RoutedEventArgs e)
        {
            Process.Start("http://dynamobim.org/");
        }
        
        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Name.Equals("MaximizeButton"))
            {
                this.WindowState = WindowState.Maximized;
                ToggleButtons(true);
            }
            else
            {
                this.WindowState = WindowState.Normal;
                ToggleButtons(false);
            }
        }

        /// <summary>
        /// Toggles between the Maximize and Normalize buttons on the window
        /// </summary>
        /// <param name="toggle"></param>
        private void ToggleButtons(bool toggle)
        {
            
            if (toggle)
            {
                this.MaximizeButton.Visibility = Visibility.Collapsed;
                this.NormalizeButton.Visibility = Visibility.Visible;
            }
            else
            {
                this.MaximizeButton.Visibility = Visibility.Visible;
                this.NormalizeButton.Visibility = Visibility.Collapsed;
            }
            
        }

        /// <summary>
        /// Lets the user drag this window around with their left mouse button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;
            DragMove();
        }

        // ESC Button pressed triggers Window close        
        private void OnCloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            htmlLicenseView.CoreWebView2.NewWindowRequested -= CoreWebView2_NewWindowRequested;
            htmlLicenseView.CoreWebView2.NavigationStarting -= ShouldAllowNavigation;
            htmlLicenseView.Dispose();
            this.Close();
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
                // Set text Foreground color on the actual range
                range.ApplyPropertyValue(TextElement.ForegroundProperty, (SolidColorBrush)new BrushConverter().ConvertFrom("#3C3C3C"));    
                range.ApplyPropertyValue(TextElement.FontSizeProperty, 12.0);

                fStream.Close();
            }
        }
        #endregion
    }
}