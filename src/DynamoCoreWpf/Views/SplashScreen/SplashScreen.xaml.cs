using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using DynamoUtilities;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace Dynamo.UI.Views
{
    public partial class SplashScreen : Window
    {
        // These are hardcoded string and should only change when npm package structure changed or image path changed
        private static readonly string htmlEmbeddedFile = "Dynamo.Wpf.Packages.SplashScreen.build.index.html";
        private static readonly string jsEmbeddedFile = "Dynamo.Wpf.Packages.SplashScreen.build.index.bundle.js";
        private static readonly string backgroundImage = "Dynamo.Wpf.Views.SplashScreen.WebApp.splashScreenBackground.png";
        private static readonly string imageFileExtension = "png";

        // Timer used for Splash Screen loading
        internal Stopwatch loadingTimer;

        /// <summary>
        /// Total loading time for the Dynamo loading tasks in milliseconds
        /// </summary>
        public long totalLoadingTime;

        /// <summary>
        /// Request to launch Dynamo main window.
        /// </summary>
        public Action<bool> RequestLaunchDynamo;

        internal Action<string> RequestImportSettings;
        internal Func<bool> RequestSignIn;
        internal Func<bool> RequestSignOut;

        /// <summary>
        /// Dynamo auth manager reference
        /// </summary>
        internal AuthenticationManager authManager;

        /// <summary>
        /// Dynamo View Model reference
        /// </summary>
        internal DynamoViewModel viewModel;

        private DynamoView dynamoView;
        /// <summary>
        /// Dynamo View reference
        /// </summary>
        public DynamoView DynamoView
        {
            get
            {
                return dynamoView;
            }
            set
            {
                dynamoView = value;
                viewModel = value.DataContext as DynamoViewModel;
                // When view model is closed, we need to close the splash screen if it is displayed.
                viewModel.RequestClose += SplashScreenRequestClose;
                authManager = viewModel.Model.AuthenticationManager;
            }
        }

        /// <summary>
        /// The WebView2 Browser instance used to display splash screen
        /// </summary>
        internal WebView2 webView;

        /// <summary>
        /// This delegate is used in StaticSplashScreenReady events
        /// </summary>
        internal delegate void StaticSplashScreenReadyHandler();

        /// <summary>
        /// This delegate is used in DynamicSplashScreenReady events
        /// </summary>
        public delegate void DynamicSplashScreenReadyHandler();

        /// <summary>
        /// Event to throw for Splash Screen to show Dynamo static screen
        /// </summary>
        internal event StaticSplashScreenReadyHandler StaticSplashScreenReady;

        /// <summary>
        /// Event to throw for Splash Screen to update Dynamo launching tasks
        /// </summary>
        public event DynamicSplashScreenReadyHandler DynamicSplashScreenReady;

        /// <summary>
        /// Request to trigger DynamicSplashScreenReady event
        /// </summary>
        public void OnRequestDynamicSplashScreen()
        {
            DynamicSplashScreenReady?.Invoke();
        }

        /// <summary>
        /// Request to trigger StaticSplashScreenReady event
        /// </summary>
        public void OnRequestStaticSplashScreen()
        {
            StaticSplashScreenReady?.Invoke();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SplashScreen()
        {
            InitializeComponent();

            loadingTimer = new Stopwatch();
            loadingTimer.Start();

            webView = new WebView2();
            ShadowGrid.Children.Add(webView);
            // Bind event handlers
            webView.NavigationCompleted += WebView_NavigationCompleted;
            DynamoModel.RequestUpdateLoadBarStatus += DynamoModel_RequestUpdateLoadBarStatus;
            DynamoModel.LanguageDetected += DynamoModel_LanguageDetected;
            StaticSplashScreenReady += OnStaticScreenReady;
            RequestLaunchDynamo = LaunchDynamo;
            RequestImportSettings = ImportSettings;
            RequestSignIn = SignIn;
            RequestSignOut = SignOut;
        }

        private void DynamoModel_LanguageDetected()
        {
            SetLabels();
        }

        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {            
            if (webView != null)
            {
                webView.NavigationCompleted -= WebView_NavigationCompleted;
                webView.Focus();
                System.Windows.Forms.SendKeys.SendWait("{TAB}");
            }
            OnRequestDynamicSplashScreen();           
        }
        /// <summary>
        /// Request to close SplashScreen.
        /// </summary>
        private void SplashScreenRequestClose(object sender, EventArgs e)
        {
            CloseWindow();
            viewModel.RequestClose -= SplashScreenRequestClose;
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
                SetImportStatus(ImportStatus.success);
            }
            else
            {
                SetImportStatus(ImportStatus.error);
            }
            Analytics.TrackEvent(Actions.Import, Categories.SplashScreenOperations, isImported.ToString());
        }

        /// <summary>
        /// Returns true if the user was successfully logged in, else false.
        /// </summary>
        private bool SignIn()
        {
            authManager.Login();
            bool ret = authManager.IsLoggedIn();
            Analytics.TrackEvent(Actions.SignIn, Categories.SplashScreenOperations, ret.ToString());
            return ret;
        }

        /// <summary>
        /// Returns true if the user was successfully logged out, else false.
        /// </summary>
        /// <returns></returns>
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
            StaticSplashScreenReady -= OnStaticScreenReady;
            Close();
            dynamoView.Show();
            dynamoView.Activate();
        }

        /// <summary>
        /// Once main window is initialized, Dynamic Splash screen should finish loading
        /// </summary>
        private void OnStaticScreenReady()
        {
            // Stop the timer in any case
            loadingTimer.Stop();

            //When a xml preferences settings file is located at C:\ProgramData\Dynamo will be read and deserialized so the settings can be set correctly.
            LoadPreferencesFileAtStartup();

            // If user is launching Dynamo for the first time or chose to always show splash screen, display it. Otherwise, display Dynamo view directly.
            if (viewModel.PreferenceSettings.IsFirstRun || viewModel.PreferenceSettings.EnableStaticSplashScreen)
            {
                SetSignInStatus(authManager.IsLoggedInInitial());
                SetLoadingDone();
            }
            else
            {
                RequestLaunchDynamo.Invoke(true);
            }
        }

        private void DynamoModel_RequestUpdateLoadBarStatus(SplashScreenLoadEventArgs args)
        {
            SetBarProperties(Dynamo.Utilities.AssemblyHelper.GetDynamoVersion().ToString(),
                    args.LoadDescription, args.BarSize);
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

        protected override async void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

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
            await webView.EnsureCoreWebView2Async();
            // Context menu disabled
            webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            // Zoom control disabled
            webView.CoreWebView2.Settings.IsZoomControlEnabled = false;

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

            jsonString = jsonString.Replace("Welcome to Dynamo!", "");
            htmlString = htmlString.Replace("mainJs", jsonString);

            webView.NavigateToString(htmlString);
            webView.CoreWebView2.AddHostObjectToScript("scriptObject",
               new ScriptObject(RequestLaunchDynamo, RequestImportSettings, RequestSignIn, RequestSignOut, CloseWindow));
        }

        internal async void SetBarProperties(string version, string loadingDescription, float barSize)
        {
            var elapsedTime = loadingTimer.ElapsedMilliseconds;
            totalLoadingTime += elapsedTime;
            loadingTimer = Stopwatch.StartNew();
            if (webView?.CoreWebView2 != null)
            {
                await webView.CoreWebView2.ExecuteScriptAsync($"window.setBarProperties(\"{version}\",\"{loadingDescription}\", \"{barSize}%\", \"{Wpf.Properties.Resources.SplashScreenLoadingTimeLabel}: {elapsedTime}ms\")");
            }
        }

        internal async void SetLoadingDone()
        {
            if (webView?.CoreWebView2 != null)
            {
                await webView.CoreWebView2.ExecuteScriptAsync($"window.setLoadingDone()");
                await webView.CoreWebView2.ExecuteScriptAsync($"window.setTotalLoadingTime(\"{Wpf.Properties.Resources.SplashScreenTotalLoadingTimeLabel} {totalLoadingTime}ms\")");
            }
        }

        /// <summary>
        /// Set the import status on splash screen.
        /// </summary>
        /// <param name="importStatus"></param>
        internal async void SetImportStatus(ImportStatus importStatus)
        {
            string importSettingsTitle = Dynamo.Wpf.Properties.Resources.SplashScreenImportSettings;
            string errorDescription = string.Empty;

            switch (importStatus)
            {
                case ImportStatus.none:
                    errorDescription = Dynamo.Wpf.Properties.Resources.ImportPreferencesInfo;
                    break;
                case ImportStatus.error:
                    errorDescription = Dynamo.Wpf.Properties.Resources.SplashScreenImportSettingsFailDescription;
                    break;
                default:
                    errorDescription = Dynamo.Wpf.Properties.Resources.ImportPreferencesInfo;
                    break;
            }

            // Update UI
            if (webView.CoreWebView2 != null)
            {
                await webView.CoreWebView2.ExecuteScriptAsync("window.setImportStatus({" +
                $"status: {(int)importStatus}," +
                $"importSettingsTitle: \"{importSettingsTitle}\"," +
                $"errorDescription: \"{errorDescription}\"" + "})");
            }
        }

        /// <summary>
        /// Set the login status on splash screen.
        /// </summary>
        internal async void SetSignInStatus(bool status)
        {
            if (webView?.CoreWebView2 != null)
            {
                await webView.CoreWebView2.ExecuteScriptAsync("window.setSignInStatus({" +               
                $"signInStatus: \"" + status + "\"})");
            }
        }

        /// <summary>
        /// Setup the values for all labels on splash screen using resources
        /// </summary>
        internal async void SetLabels()
        {
            if (webView.CoreWebView2 != null)
            {
                await webView.CoreWebView2.ExecuteScriptAsync("window.setLabels({" +
                   $"welcomeToDynamoTitle: \"{Wpf.Properties.Resources.SplashScreenWelcomeToDynamo}\"," +
                   $"launchTitle: \"{Wpf.Properties.Resources.SplashScreenLaunchTitle}\"," +
                   $"importSettingsTitle: \"{Wpf.Properties.Resources.ImportSettingsDialogTitle}\"," +
                   $"showScreenAgainLabel: \"{Wpf.Properties.Resources.SplashScreenShowScreenAgainLabel}\"," +
                   $"signInTitle: \"{Wpf.Properties.Resources.SplashScreenSignIn}\"," +
                   $"signOutTitle: \"{Wpf.Properties.Resources.SplashScreenSignOut}\"," +
                   $"signingInTitle: \"{Wpf.Properties.Resources.SplashScreenSigningIn}\"," +
                   $"importSettingsTooltipDescription: \"{Wpf.Properties.Resources.ImportPreferencesInfo}\"" + "})");
            }
        }

        /// <summary>
        /// At Dynamo startup process load the preferences settings file located in C:\ProgramData\Dynamo
        /// </summary>
        internal void LoadPreferencesFileAtStartup()
        {
            if (viewModel.PreferenceSettings.IsFirstRun == true)
            {
                //Move the current location two levels up
                var programDataDir = Directory.GetParent(Directory.GetParent(viewModel.Model.PathManager.CommonDataDirectory).ToString()).ToString();
                var listOfXmlFiles = Directory.GetFiles(programDataDir, "*.xml");
                string PreferencesSettingFilePath = string.Empty;

                //Find the first xml file name from the list that can be Deserialized to PreferenceSettings
                foreach (var xmlFile in listOfXmlFiles)
                {
                    if (IsValidPreferencesFile(xmlFile))
                    {
                        PreferencesSettingFilePath = xmlFile;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(PreferencesSettingFilePath) && File.Exists(PreferencesSettingFilePath))
                {
                    var content = File.ReadAllText(PreferencesSettingFilePath);
                    ImportSettings(content);
                }
            }
        }

        /// <summary>
        /// Try to Deserialize to PreferenceSettings the file content passed as parameter
        /// </summary>
        /// <param name="filePath">Full path to the xml file to be deserialized</param>
        /// <returns>true if the file content was deserialized successfully otherwise returns false</returns>
        private static bool IsValidPreferencesFile(string filePath)
        {
            string content = string.Empty;

            if (File.Exists(filePath))
            {
                content = File.ReadAllText(filePath);
            }

            if (string.IsNullOrEmpty(content))
                return false;

            try
            {
                PreferenceSettings settings = null;
                var serializer = new XmlSerializer(typeof(PreferenceSettings));
                using (TextReader reader = new StringReader(content))
                {
                    settings = serializer.Deserialize(reader) as PreferenceSettings;
                }
                return settings != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// If the user wants to close the window, we shutdown the application and don't launch Dynamo
        /// </summary>
        private void CloseWindow()
        {
            if (Application.Current != null)
            {
                Application.Current.Shutdown();
                Analytics.TrackEvent(Actions.Close, Categories.SplashScreenOperations);
            }
            // If Dynamo is launched from an integrator host, user should be able to close the splash screen window.
            // Additionally, we will have to shutdown the ViewModel which will close all the services and dispose the events.
            // RequestUpdateLoadBarStatus event needs to be unsubscribed when the splash screen window is closed, to avoid populating the info on splash screen.
            else if (this is SplashScreen)
            {
                this.Close();
                viewModel.Model.ShutDown(false);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            DynamoModel.RequestUpdateLoadBarStatus -= DynamoModel_RequestUpdateLoadBarStatus;
            DynamoModel.LanguageDetected -= DynamoModel_LanguageDetected;
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
