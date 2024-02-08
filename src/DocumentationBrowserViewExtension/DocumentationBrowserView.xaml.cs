using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Logging;
using Dynamo.Models;
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

        /// <summary>
        /// Simple representation of the states of the initialize async method
        /// </summary>
        private enum WebView2InitState
        {
            NotStarted = 0,// Async method not called yet
            Started,// Async method called but not finished execution (usually set before any awaits)
            Done// Async method has finished execution (all awaits have finished)
        }

        private const string ABOUT_BLANK_URI = "about:blank";
        private readonly DocumentationBrowserViewModel viewModel;
        private const string VIRTUAL_FOLDER_MAPPING = "appassets";
        static readonly string HTML_IMAGE_PATH_PREFIX = @"http://";
        private WebView2InitState initState = WebView2InitState.NotStarted;
        private ScriptingObject comScriptingObject;
        private string fontStylePath = "Dynamo.Wpf.Views.GuidedTour.HtmlPages.Resources.ArtifaktElement-Regular.woff";

        internal string WebBrowserUserDataFolder { get; set; }
        internal string FallbackDirectoryName { get; set; }
        //This folder will be used to store images and dyn files previosuly located in /rootDirectory/en-US/fallback_docs so we don't need to copy all those files per each language
        internal static readonly string SharedDocsDirectoryName = "NodeHelpSharedDocs";

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
                Process.Start(new ProcessStartInfo(e.Uri) { UseShellExecute = true });
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
            Dispatcher.Invoke(InitializeAsync);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
            this.viewModel.LinkChanged -= NavigateToPage;
            if (this.documentationBrowser != null)
            {
                this.documentationBrowser.NavigationStarting -= ShouldAllowNavigation;
                this.documentationBrowser.DpiChanged -= DocumentationBrowser_DpiChanged;
                if (this.documentationBrowser.CoreWebView2 != null)
                {
                    this.documentationBrowser.CoreWebView2.WebMessageReceived -= CoreWebView2OnWebMessageReceived;
                }

                this.documentationBrowser.Dispose();
            }
        }

        async void InitializeAsync()
        {
            VirtualFolderPath = string.Empty;
            try
            {
                //if this node is from a package then we set the virtual host path to the packages docs directory.
                if (viewModel.Link != null && !string.IsNullOrEmpty(viewModel.CurrentPackageName) && viewModel.IsOwnedByPackage)
                {
                    VirtualFolderPath = Path.GetDirectoryName(HttpUtility.UrlDecode(viewModel.Link.AbsolutePath));
                }
                //if the node is not from a package, then set the virtual host path to the shared docs folder.
                else if (viewModel.Link != null && !viewModel.IsOwnedByPackage)
                {
                    VirtualFolderPath = Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, SharedDocsDirectoryName);
                }
                //unclear what would cause this.
                else
                {
                    VirtualFolderPath = FallbackDirectoryName;
                }
                //TODO - the above will not handle the case that a package's images/dyns are located in the shared folder
                //we may have to do some inspection of the package docs folder and decide to fallback in some cases, or mark the package
                //in some way.
            }
            catch (Exception ex)
            {
                VirtualFolderPath = string.Empty;
                Log(ex.Message);
            }

            // Only initialize once 
            if (initState == WebView2InitState.NotStarted)
            {
                initState = WebView2InitState.Started;
                if (!string.IsNullOrEmpty(WebBrowserUserDataFolder))
                {
                    //This indicates in which location will be created the WebView2 cache folder
                    documentationBrowser.CreationProperties = new CoreWebView2CreationProperties()
                    {
                        UserDataFolder = WebBrowserUserDataFolder
                    };
                }

                try
                {
                    //Initialize the CoreWebView2 component otherwise we can't navigate to a web page
                    await documentationBrowser.Initialize(Log);
     
                    this.documentationBrowser.CoreWebView2.WebMessageReceived += CoreWebView2OnWebMessageReceived;
                    comScriptingObject = new ScriptingObject(this.viewModel);
                    //register the interop object into the browser.
                    this.documentationBrowser.CoreWebView2.AddHostObjectToScript("bridge", comScriptingObject);

                    this.documentationBrowser.CoreWebView2.Settings.IsZoomControlEnabled = true;
                    this.documentationBrowser.CoreWebView2.Settings.AreDevToolsEnabled = true;

                    initState = WebView2InitState.Done;
                }
                catch(ObjectDisposedException ex)
                {
                    Log(ex.Message);
                }
                
            }
            //if we make it this far, for example to do re-entry to to this method, while we're still
            //initializing, don't do anything, just bail.
            if (initState == WebView2InitState.Done)
            {
                if (Directory.Exists(VirtualFolderPath))
                {
                    //Due that the Web Browser(WebView2 - Chromium) security CORS is blocking the load of resources like images then we need to create a virtual folder in which the image are located.
                    this.documentationBrowser?.CoreWebView2?.SetVirtualHostNameToFolderMapping(VIRTUAL_FOLDER_MAPPING, VirtualFolderPath, CoreWebView2HostResourceAccessKind.DenyCors);
                }
                string htmlContent = this.viewModel.GetContent();

                htmlContent = ResourceUtilities.LoadResourceAndReplaceByKey(htmlContent, "#fontStyle", fontStylePath);

                this.documentationBrowser.NavigateToString(htmlContent);
            }
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
            if (DynamoModel.IsTestMode)
            {
                System.Console.WriteLine(message);
            }
            else
            {
                viewModel?.MessageLogged?.Invoke(LogMessage.Info(message));
            }
        }
        #endregion
    }
}
