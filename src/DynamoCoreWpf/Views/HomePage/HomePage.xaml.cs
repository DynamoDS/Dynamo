using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Dynamo.UI.Controls;
using Dynamo.Utilities;
using DynamoUtilities;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;


namespace Dynamo.UI.Views
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : UserControl, IDisposable
    {
        // These are hardcoded string and should only change when npm package structure changed or image path changed
        private static readonly string htmlEmbeddedFile = "Dynamo.Wpf.Packages.HomePage.build.index.html";
        private static readonly string jsEmbeddedFile = "Dynamo.Wpf.Packages.HomePage.build.bundle.js";
        private static readonly string fontStylePath = "Dynamo.Wpf.Views.GuidedTour.HtmlPages.Resources.ArtifaktElement-Regular.woff";
        private static readonly string virtualFolderName = "embeddedFonts";
        private static readonly string virtualFolderPath = Path.Combine(Path.GetTempPath(), virtualFolderName);

        private string fontFilePath;

        private StartPageViewModel startPage;

        /// <summary>
        /// The WebView2 Browser instance used to display splash screen
        /// </summary>
        internal WebView2 webView;

        public HomePage()
        {   
            InitializeComponent();

            //loadingTimer = new Stopwatch();
            //loadingTimer.Start();

            webView = new WebView2();

            webView.Margin = new System.Windows.Thickness(0);  // Set margin to zero
            webView.ZoomFactor = 1.0;  // Set zoom factor (optional)
                
            HostGrid.Children.Add(webView);
            // Bind event handlers
            //webView.NavigationCompleted += WebView_NavigationCompleted;
            //DynamoModel.RequestUpdateLoadBarStatus += DynamoModel_RequestUpdateLoadBarStatus;
            //DynamoModel.LanguageDetected += DynamoModel_LanguageDetected;
            //StaticSplashScreenReady += OnStaticScreenReady;
            //RequestLaunchDynamo = LaunchDynamo;
            //RequestImportSettings = ImportSettings;
            //RequestSignIn = SignIn;
            //RequestSignOut = SignOut;
            //this.enableSignInButton = enableSignInButton;
            DataContextChanged += OnDataContextChanged; 
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            startPage = this.DataContext as StartPageViewModel;
        }

        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (webView != null)
            {
                webView.NavigationCompleted -= WebView_NavigationCompleted;
                webView.Focus();
                System.Windows.Forms.SendKeys.SendWait("{TAB}");
            }
        }

        /// <summary>
        /// This is used before DynamoModel initialization specifically to get user data dir
        /// </summary>
        /// <returns></returns>
        private string GetUserDirectory()
        {
            var version = AssemblyHelper.GetDynamoVersion();

            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(Path.Combine(folder, "Dynamo", "Dynamo Core"),
                            String.Format("{0}.{1}", version.Major, version.Minor));
        }

        private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            string htmlString = string.Empty;   
            string jsonString = string.Empty;

            // When executing Dynamo as Sandbox or inside any host like Revit, FormIt, Civil3D the WebView2 cache folder will be located in the AppData folder
            var userDataDir = new DirectoryInfo(GetUserDirectory());
            PathHelper.CreateFolderIfNotExist(userDataDir.ToString());
            var webBrowserUserDataFolder = userDataDir.Exists ? userDataDir : null;

            webView.CreationProperties = new CoreWebView2CreationProperties
            {
                UserDataFolder = webBrowserUserDataFolder.FullName
            };

            //ContentRendered ensures that the webview2 component is visible.
            await webView.EnsureCoreWebView2Async();
            // Context menu disabled
            this.webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            // Zoom control disabled
            this.webView.CoreWebView2.Settings.IsZoomControlEnabled = false;
            this.webView.CoreWebView2.Settings.AreDevToolsEnabled = true;

            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(htmlEmbeddedFile))
            using (StreamReader reader = new StreamReader(stream))
            {
                htmlString = reader.ReadToEnd();
            }

            using (Stream stream = assembly.GetManifestResourceStream(jsEmbeddedFile))
            using (StreamReader reader = new StreamReader(stream))
            {
                var jsString = reader.ReadToEnd();
                jsonString = jsString;
            }

            // Copy the embedded resource to a temporary folder
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fontStylePath))
            {
                if (stream != null)
                {
                    var fontData = new byte[stream.Length];
                    stream.Read(fontData, 0, fontData.Length);

                    // Create the temporary folder if it doesn't exist
                    Directory.CreateDirectory(virtualFolderPath);

                    // Write the font file to the temporary folder
                    fontFilePath = Path.Combine(virtualFolderPath, "ArtifaktElement-Regular.woff");
                    File.WriteAllBytes(fontFilePath, fontData);
                }
            }

            // Set up virtual host name to folder mapping
            webView.CoreWebView2.SetVirtualHostNameToFolderMapping(virtualFolderName, virtualFolderPath, CoreWebView2HostResourceAccessKind.DenyCors);

            htmlString = htmlString.Replace("mainJs", jsonString);

            try
            {
                webView.NavigateToString(htmlString);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            LoadingDone();
            //webView.CoreWebView2.AddHostObjectToScript("scriptObject",
            //   new ScriptObject(RequestLaunchDynamo, RequestImportSettings, RequestSignIn, RequestSignOut, CloseWindow));
        }

        internal async void LoadingDone()
        {
            if (startPage == null) { return; }

            var recentFiles = startPage.RecentFiles;
            if (recentFiles == null || !recentFiles.Any()) { return; }

            LoadGraphs(recentFiles);

            var testMessage = "I am being tested";

            if (webView?.CoreWebView2 != null)
            {
                await webView.CoreWebView2.ExecuteScriptAsync(@$"window.setLoadingDone('{testMessage}')");
            }
        }

        /// <summary>
        /// Sends graph data to react app
        /// </summary>
        /// <param name="data"></param>
        private async void LoadGraphs(ObservableCollection<StartPageListItem> data)
        {
            string jsonData = JsonSerializer.Serialize(data);

            if (webView?.CoreWebView2 != null)
            {
                await webView.CoreWebView2.ExecuteScriptAsync(@$"window.receiveGraphDataFromDotNet({jsonData})");
            }
        }

        public void Dispose()
        {
            DataContextChanged -= OnDataContextChanged;

            if (File.Exists(fontFilePath))
            {
                File.Delete(fontFilePath);
            }
        }
    }
}
