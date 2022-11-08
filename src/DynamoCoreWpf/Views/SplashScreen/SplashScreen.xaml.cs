using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using Dynamo.Applications;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels.Watch3D;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace Dynamo.UI.Views
{
    public partial class SplashScreen : Window
    {
        // These are hardcoded string and should only change when npm package structure changed or image path changed
        private static readonly string htmlEmbeddedFile = "Dynamo.Wpf.node_modules._dynamods.splash_screen.build.index.html";
        private static readonly string jsEmbeddedFile = "Dynamo.Wpf.node_modules._dynamods.splash_screen.build.index.bundle.js";
        private static readonly string backgroundImage = "Dynamo.Wpf.Views.SplashScreen.WebApp.splashScreenBackground.png";
        private static readonly string imageFileExtension = "png";

        private Stopwatch loadingTimer;

        private long totalLoadingTime;

        private readonly DirectoryInfo webBrowserUserDataFolder;

        internal Action<bool> RequestLaunchDynamo;
        internal Action<string> RequestImportSettings;
        internal Func<bool> RequestSignIn; 
        internal Func<bool> RequestSignOut;
        internal WebView2 webView;
        private DynamoView dynamoView;
        private AuthenticationManager authManager;
        public DynamoViewModel viewModel = null;
        private readonly string ASMPath;
        private readonly string CERPath;
        private readonly string commandFilePath;
        private readonly HostAnalyticsInfo hostAnalyticsInfo;

        /// <summary>
        /// Constructor
        /// </summary>
        public SplashScreen(HostAnalyticsInfo info = new HostAnalyticsInfo(), string asmPath ="", string cerPath ="", string cmdFilePath ="")
        {
            ASMPath = asmPath;
            CERPath = cerPath;
            commandFilePath = cmdFilePath;
            hostAnalyticsInfo = info;
            InitializeComponent();
            loadingTimer = new Stopwatch();
            loadingTimer.Start();

            //When executing Dynamo as Sandbox or inside any host like Revit, FormIt, Civil3D the WebView2 cache folder will be located in the AppData folder
            var userDataDir = new DirectoryInfo(GetUserDirectory());
            webBrowserUserDataFolder = userDataDir.Exists ? userDataDir : null;

            webView = new WebView2();
            AddChild(webView);
            webView.NavigationCompleted += WebView_NavigationCompleted;
            DynamoModel.RequestUpdateLoadBarStatus += DynamoModel_RequestUpdateLoadBarStatus;
            RequestLaunchDynamo = LaunchDynamo;
            RequestImportSettings = ImportSettings;
            RequestSignIn = SignIn;
            RequestSignOut = SignOut;
        }

        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            SetLabels();
            LoadDynamoView();
            if (webView != null)
            {
                webView.NavigationCompleted -= WebView_NavigationCompleted;
            }
        }

        private void LoadDynamoView()
        {
            DynamoModel model;
            model = StartupUtils.MakeModel(false, ASMPath ?? string.Empty, hostAnalyticsInfo);

            model.CERLocation = CERPath;

            viewModel = DynamoViewModel.Start(
                   new DynamoViewModel.StartConfiguration()
                   {
                       CommandFilePath = commandFilePath,
                       DynamoModel = model,
                       Watch3DViewModel =
                           HelixWatch3DViewModel.TryCreateHelixWatch3DViewModel(
                               null,
                               new Watch3DViewModelStartupParams(model),
                               model.Logger),
                       ShowLogin = true
                   });

            DynamoModel.OnRequestUpdateLoadBarStatus(new SplashScreenLoadEventArgs(Dynamo.Wpf.Properties.Resources.SplashScreenLaunchingDynamo, 70));
            dynamoView = new DynamoView(viewModel);
            authManager = model.AuthenticationManager;

            // If user is launching Dynamo for the first time or chose to always show splash screen, display it. Otherwise, display Dynamo view directly.
            if (viewModel.PreferenceSettings.IsFirstRun || viewModel.PreferenceSettings.EnableStaticSplashScreen)
            {
                SetSignInStatus(authManager.IsLoggedIn());
                SetLoadingDone();
            }
            else
            {
                LaunchDynamo(true);
            }
        }

        /// <summary>
        /// Import setting file from chosen path
        /// </summary>
        /// <param name="fileContent"></param>
        private void ImportSettings(string fileContent)
        {
            bool isImported = viewModel.PreferencesViewModel.importSettingsContent(fileContent);
            if (isImported)
            {
                SetImportStatus(ImportStatus.success, Dynamo.Wpf.Properties.Resources.SplashScreenSettingsImported, string.Empty);
            }
            else
            {
                SetImportStatus(ImportStatus.error, Dynamo.Wpf.Properties.Resources.SplashScreenFailedImportSettings, Dynamo.Wpf.Properties.Resources.SplashScreenImportSettingsFailDescription);
            }
            Analytics.TrackEvent(Actions.ImportSettings, Categories.SplashScreenOperations, isImported.ToString());
        }

        /// <summary>
        /// Returns true if the user was successfully logged in, else false.
        /// </summary>
        /// <param name="status">If set to false, it will only return the login status without performing the login function</param>
        private bool SignIn()
        {
            authManager.Login();
            bool ret = authManager.IsLoggedIn();
            Analytics.TrackEvent(Actions.SignIn, Categories.SplashScreenOperations, ret.ToString());
            return ret;
        }

        //Returns true if the user was successfully logged out, else false.
        private bool SignOut()
        {
            authManager.Logout();
            bool ret = !authManager.IsLoggedIn();
            Analytics.TrackEvent(Actions.SignOut, Categories.SplashScreenOperations, ret.ToString());
            return ret;
        }

        /// <summary>
        /// Handler to launch Dynamo View
        /// </summary>
        /// <param name="isCheckboxChecked"></param>
        private void LaunchDynamo(bool isCheckboxChecked)
        {
            viewModel.PreferenceSettings.EnableStaticSplashScreen = !isCheckboxChecked;
            Close();
            Application.Current.MainWindow = dynamoView;
            dynamoView.Show();
            dynamoView.Activate();
        }

        private void DynamoModel_RequestUpdateLoadBarStatus(SplashScreenLoadEventArgs args)
        {
            SetBarProperties(Dynamo.Utilities.AssemblyHelper.GetDynamoVersion().ToString(),
                    args.LoadDescription, args.BarSize);
        }

        private string GetUserDirectory()
        {
            var version = AssemblyHelper.GetDynamoVersion();

            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(Path.Combine(folder, "Dynamo", "Dynamo Core"),
                            String.Format("{0}.{1}", version.Major, version.Minor));
        }

        protected override async void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            string htmlString = string.Empty;
            string jsonString = string.Empty;

            await webView.EnsureCoreWebView2Async();
            // Context menu disabled
            webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
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

            using (Stream stream = assembly.GetManifestResourceStream(backgroundImage))
            {
                var resourceBase64 = Utilities.ResourceUtilities.ConvertToBase64(stream);
                jsonString = jsonString.Replace("#base64BackgroundImage", $"data:image/{imageFileExtension};base64,{resourceBase64}");
            }

            htmlString = htmlString.Replace("mainJs", jsonString);

            webView.CreationProperties = new CoreWebView2CreationProperties
            {
                UserDataFolder = webBrowserUserDataFolder.FullName
            };

            webView.NavigateToString(htmlString);
            webView.CoreWebView2.AddHostObjectToScript("scriptObject",
               new ScriptObject(RequestLaunchDynamo, RequestImportSettings, RequestSignIn, RequestSignOut, CloseWindow));
        }

        internal async void SetBarProperties(string version, string loadingDescription, float barSize)
        {
            var elapsedTime = loadingTimer.ElapsedMilliseconds;
            totalLoadingTime += elapsedTime;
            loadingTimer = Stopwatch.StartNew();
            await webView.CoreWebView2.ExecuteScriptAsync($"window.setBarProperties('{version}','{loadingDescription}', '{barSize}%', '{Wpf.Properties.Resources.SplashScreenLoadingTimeLabel}: {elapsedTime}ms')");
        }

        internal async void SetLoadingDone()
        {
            loadingTimer.Stop();
            loadingTimer = null;
            await webView.CoreWebView2.ExecuteScriptAsync($"window.setLoadingDone()");
            await webView.CoreWebView2.ExecuteScriptAsync($"window.setTotalLoadingTime('{Wpf.Properties.Resources.SplashScreenTotalLoadingTimeLabel} {totalLoadingTime}ms')");
            Analytics.TrackStartupTime("DynamoSandbox", TimeSpan.FromMilliseconds(totalLoadingTime));
        }

        internal async void SetImportStatus(ImportStatus importStatus, string importSettingsTitle, string errorDescription)
        {
            await webView.CoreWebView2.ExecuteScriptAsync("window.setImportStatus({" +
                $"status: {(int)importStatus}," +
                $"importSettingsTitle: '{importSettingsTitle}'," +
                $"errorDescription: '{errorDescription}'" + "})");
        }

        /// <summary>
        /// Set the login status on splash screen.
        /// </summary>
        internal async void SetSignInStatus(bool status)
        {
            await webView.CoreWebView2.ExecuteScriptAsync("window.setSignInStatus({" +
                $"signInTitle: '" + (status ? Wpf.Properties.Resources.SplashScreenSignOut : Wpf.Properties.Resources.SplashScreenSignIn).ToString() + "'," +
                $"signInStatus: '" + status + "'})");
        }

        /// <summary>
        /// Setup the values for all lables on splash screen using resources
        /// </summary>
        internal async void SetLabels()
        {
            await webView.CoreWebView2.ExecuteScriptAsync("window.setLabels({" +
               $"welcomeToDynamoTitle: '{Wpf.Properties.Resources.SplashScreenWelcomeToDynamo}'," +
               $"launchTitle: '{Wpf.Properties.Resources.SplashScreenLaunchTitle}'," +
               $"importSettingsTitle: '{Wpf.Properties.Resources.SplashScreenImportSettings}'," +
               $"showScreenAgainLabel: '{Wpf.Properties.Resources.SplashScreenShowScreenAgainLabel}'" + "})");
        }

        /// <summary>
        /// If the user wants to close the window, we shutdown the application and don't launch Dynamo
        /// </summary>
        private void CloseWindow()
        {
           Application.Current.Shutdown();
           Analytics.TrackEvent(Actions.Close, Categories.SplashScreenOperations);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            webView.Dispose();
            webView = null;

            GC.SuppressFinalize(this);
        }
    }

    enum ImportStatus
    {
        none = 1,
        error,
        success
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class ScriptObject
    {
        readonly Action<bool> RequestLaunchDynamo;
        readonly Action<string> RequestImportSettings;
        readonly Func<bool> RequestSignIn;
        readonly Func<bool> RequestSignOut;
        readonly Action RequestCloseWindow;

        public ScriptObject(Action<bool> requestLaunchDynamo, Action<string> requestImportSettings, Func< bool> requestSignIn, Func<bool> requestSignOut, Action requestCloseWindow)
        {
            RequestLaunchDynamo = requestLaunchDynamo;
            RequestImportSettings = requestImportSettings;
            RequestSignIn = requestSignIn;
            RequestSignOut = requestSignOut;
            RequestCloseWindow = requestCloseWindow;
        }

        public void LaunchDynamo(bool showScreenAgain)
        {
            RequestLaunchDynamo(showScreenAgain);
            Analytics.TrackEvent(Actions.Start, Categories.SplashScreenOperations);
        }

        public void ImportSettings(string file)
        {
            RequestImportSettings(file);
        }
        public bool SignIn()
        {
            return RequestSignIn();
        }
        public bool SignOut()
        {
            return RequestSignOut();
        }
        public void CloseWindow()
        {
            RequestCloseWindow();
        }
    }
}
