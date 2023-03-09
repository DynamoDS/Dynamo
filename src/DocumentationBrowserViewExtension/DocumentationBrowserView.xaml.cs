using System;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Logging;
using Dynamo.Utilities;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace Dynamo.DocumentationBrowser
{
    /// <summary>
    /// Interaction logic for DocumentationBrowserView.xaml
    /// </summary>
    public partial class DocumentationBrowserView : UserControl, IDisposable
    {
        private const string ABOUT_BLANK_URI = "about:blank";
        private readonly DocumentationBrowserViewModel viewModel;
        private const string VIRTUAL_FOLDER_MAPPING = "appassets";
        static readonly string HTML_IMAGE_PATH_PREFIX = @"http://";
        private bool hasBeenInitialized;
        private ScriptingObject comScriptingObject;
        private string fontStylePath = "Dynamo.Wpf.Views.GuidedTour.HtmlPages.Resources.ArtifaktElement-Regular.woff";

        internal string WebBrowserUserDataFolder { get; set; }
        internal string FallbackDirectoryName { get; set; }

        //Path in which the virtual folder for loading images will be created
        internal string VirtualFolderPath { get; set; }

        /// <summary>
        /// Construct a new DocumentationBrowserView given an appropriate viewmodel.
        /// </summary>
        /// <param name="viewModel">The ViewModel to use as source of events and content.</param>
        public DocumentationBrowserView(DocumentationBrowserViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            this.viewModel = viewModel;

            // subscribe to the link changed event on the view model
            // so we know when to navigate to a new documentation page/document
            viewModel.LinkChanged += NavigateToPage;
            // handle browser component events & disable certain features that are not needed
            this.documentationBrowser.AllowDrop = false;
            this.documentationBrowser.NavigationStarting += ShouldAllowNavigation;
            this.documentationBrowser.DpiChanged += DocumentationBrowser_DpiChanged;
        }

        private void DocumentationBrowser_DpiChanged(object sender, DpiChangedEventArgs args)
        {
            try
            {
                // it's possible we're trying to invoke this before the adaptDPI function is
                // injected into the script scope, wrap this in a try catch.
                documentationBrowser.ExecuteScriptAsync("adaptDPI()");
            }
            catch (Exception e)
            {
                viewModel.MessageLogged?.Invoke(LogMessage.Info($"failed to set DPI,{e.Message}"));
            }
        }
        /// <summary>
        /// Redirect the user to the browser if they press a link in the documentation browser
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShouldAllowNavigation(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            // if is not an URL then we should return otherwise it will crash when trying to open the URL in the default Web Browser
            if(!e.Uri.StartsWith(HTML_IMAGE_PATH_PREFIX.Substring(0,4)))
            {
                return;
            }

            // we never set the uri if navigating to a local document, so safe to navigate
            if (e.Uri == null)
                return;

            // we want to cancel navigation when a clicked link would navigate 
            // away from the page the ViewModel wants to display
            var isAboutBlankLink = e.Uri.ToString().Equals(ABOUT_BLANK_URI);
            var isRemoteLinkFromLocalDocument = !e.Uri.Equals(this.viewModel.Link);

            if (isAboutBlankLink || isRemoteLinkFromLocalDocument)
            {
                // in either of these two cases, cancel the navigation 
                // and redirect it to a new process that starts the default OS browser
                e.Cancel = true;
                Process.Start(new ProcessStartInfo(e.Uri));
            }
        }

        /// <summary>
        /// Instruct the embedded web browser to navigate to a given link.
        /// If link is remote resource it is loaded from there.
        /// If link is local resource, it is loaded from ViewModel content.
        /// </summary>
        /// <param name="link"></param>
        public void NavigateToPage(Uri link)
        {
            InitializeAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
            this.viewModel.LinkChanged -= NavigateToPage;
            this.documentationBrowser.NavigationStarting -= ShouldAllowNavigation;
            // Note to test writers
            // Disposing the document browser will cause future tests
            // that uses the Browser component to crash
            if (!Models.DynamoModel.IsTestMode)
            {
                this.documentationBrowser.Dispose();
            }
            this.documentationBrowser.DpiChanged -= DocumentationBrowser_DpiChanged;
            try
            {
                if (this.documentationBrowser.CoreWebView2 != null)
                    this.documentationBrowser.CoreWebView2.WebMessageReceived -= CoreWebView2OnWebMessageReceived;

            }
            catch (Exception)
            {
                return;
            }
        }

        async void InitializeAsync()
        {
            VirtualFolderPath = string.Empty;
            try
            {
                if (viewModel.Link != null && !string.IsNullOrEmpty(viewModel.CurrentPackageName))
                {
                    VirtualFolderPath = Path.GetDirectoryName(HttpUtility.UrlDecode(viewModel.Link.AbsolutePath));
                }
                else
                    VirtualFolderPath = FallbackDirectoryName;
            }
            catch (Exception ex)
            {
                VirtualFolderPath = string.Empty;
                Log(ex.Message);
            }

            // Only initialize once 
            if (!hasBeenInitialized)
            {
                if (!string.IsNullOrEmpty(WebBrowserUserDataFolder))
                {
                    //This indicates in which location will be created the WebView2 cache folder
                    documentationBrowser.CreationProperties = new CoreWebView2CreationProperties()
                    {
                        UserDataFolder = WebBrowserUserDataFolder
                    };
                }
                //Initialize the CoreWebView2 component otherwise we can't navigate to a web page
                await documentationBrowser.EnsureCoreWebView2Async();
                
           
                this.documentationBrowser.CoreWebView2.WebMessageReceived += CoreWebView2OnWebMessageReceived;
                comScriptingObject = new ScriptingObject(this.viewModel);
                //register the interop object into the browser.
                this.documentationBrowser.CoreWebView2.AddHostObjectToScript("bridge", comScriptingObject);

                this.documentationBrowser.CoreWebView2.Settings.IsZoomControlEnabled = true;
                this.documentationBrowser.CoreWebView2.Settings.AreDevToolsEnabled = true;

                hasBeenInitialized = true;
            }

            if(Directory.Exists(VirtualFolderPath))
                //Due that the Web Browser(WebView2 - Chromium) security CORS is blocking the load of resources like images then we need to create a virtual folder in which the image are located.
                this.documentationBrowser.CoreWebView2.SetVirtualHostNameToFolderMapping(VIRTUAL_FOLDER_MAPPING, VirtualFolderPath, CoreWebView2HostResourceAccessKind.DenyCors);

            string htmlContent = this.viewModel.GetContent();

            htmlContent = ResourceUtilities.LoadResourceAndReplaceByKey(htmlContent, "#fontStyle", fontStylePath);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                this.documentationBrowser.NavigateToString(htmlContent);
            }));
        }

        private void CoreWebView2OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var message = e.TryGetWebMessageAsString();
            comScriptingObject.Notify(message);
        }
        
        /// <summary>
        /// Dispose function for DocumentationBrowser
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region ILogSource Implementation
        private void Log(string message)
        {
            viewModel.MessageLogged?.Invoke(LogMessage.Info(message));
        }
        #endregion
    }
}
