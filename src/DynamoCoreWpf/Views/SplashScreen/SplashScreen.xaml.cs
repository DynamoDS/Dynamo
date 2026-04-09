using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Xml.Serialization;
using Autodesk.DesignScript.Runtime;
using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.UI.Prompts;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Utilities;
using DynamoUtilities;
using Greg.AuthProviders;
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

        /// <summary>
        /// True if the reason the splash screen was closed was because the user explicitly closed it,
        /// as opposed to the splash screen closing because dynamo was launched.
        /// This is useful for knowing if Dynamo is already started or not.
        /// </summary>
        public bool CloseWasExplicit { get; private set; }

        // Indicates if the SplashScren close button was hit.
        // Used to ensure that OnClosing is called only once.
        private bool IsClosing = false;

        internal enum CloseMode { ByStartingDynamo, ByCloseButton, ByOther };

        internal CloseMode currentCloseMode = CloseMode.ByOther;

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

        // Fields for tracking imported settings and enabling reset functionality
        private bool settingsWereImported = false;
        private string originalSettingsContent = null;
        private string importedSettingsFilePath = null;

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
                if(dynamoView == null)
                {
                    return;
                }
                viewModel = value.DataContext as DynamoViewModel;
                authManager = viewModel.Model.AuthenticationManager;
            }
        }

        /// <summary>
        /// The WebView2 Browser instance used to display splash screen
        /// </summary>
        internal DynamoWebView2 webView;

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
            if (viewModel == null)
            {
                Dispatcher.Invoke(() =>
                {
                    DynamoMessageBox.Show(
                        owner: this,
                        messageBoxText: Wpf.Properties.Resources.DynamoSplashScreenFailed,
                        caption: Wpf.Properties.Resources.DynamoSplashScreen,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                        );
                    this.Close();
                });
            }
            StaticSplashScreenReady?.Invoke();
        }

        /// <summary>
        /// Stores the value that indicates if the SignIn Button will be enabled(default) or not
        /// </summary>
        bool enableSignInButton;

        /// <summary>
        /// Splash Screen Constructor. 
        /// <paramref name="enableSignInButton"/> Indicates if the SignIn Button will be enabled(default) or not.
        /// </summary>
        public SplashScreen(bool enableSignInButton = true)
        {
            InitializeComponent();

            loadingTimer = new Stopwatch();
            loadingTimer.Start();

            webView = new DynamoWebView2();
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
            this.enableSignInButton = enableSignInButton;
            currentCloseMode = CloseMode.ByOther;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // If we have multiple OnClosing events (ex Clicking the close button multiple times)
            // we need to only process the first one. THe rest should be canceled so that we can avoid timing issues with the order of windows messages
            // Ex  WM_CLOSE => webview2.Visibility.Set => waits for windows message =>  WM_DESTROY =>
            // webview2.Dispose => webview2.Visible.Set receives windows message => crash because object got disposed. 
            if (!IsClosing)
            {
                //Means that the SplashScreen was closed by other way for example by using the Windows Task Bar
                if(currentCloseMode == CloseMode.ByOther)
                {
                    CloseWasExplicit = true;
                }
                // First call to OnClosing
                IsClosing = true;
            }
            else
            {
                // Cancel the Close action for all subsequent calls
                e.Cancel = true;
            }
            base.OnClosing(e);
        }

        private void DynamoModel_LanguageDetected()
        {
            SetLabels();
        }

        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            try
            {
                // Exceptions thrown in WebView_NavigationCompleted seem to be silenced somewhere in the webview2 callstack.
                // If we catch an exceptions here, we log it and close the spash screen.
                //
                if (webView != null)
                {
                    webView.NavigationCompleted -= WebView_NavigationCompleted;
                    webView.Focus();
                    System.Windows.Forms.SendKeys.SendWait("{TAB}");
                }
                OnRequestDynamicSplashScreen();
            }
            catch (Exception ex)
            {
                if (DynamoModel.IsTestMode)
                {
                    // Rethrow exception during testing.
                    throw;
                }

                if (!DynamoModel.IsCrashing && !IsClosing)
                {
                    CrashReportTool.ShowCrashWindow(viewModel, new CrashErrorReportArgs(ex));
                    Close();// Close the SpashScreen
                }
            }
        }

        /// <summary>
        /// Import setting file from chosen path
        /// </summary>
        /// <param name="fileContent"></param>
        private void ImportSettings(string fileContent)
        {
            try
            {
                // Get the settings file path
                var pathManager = viewModel?.Model?.PathManager ?? PathManager.Instance;
                importedSettingsFilePath = pathManager.PreferenceFilePath;

                // Backup original settings before importing (only if not already backed up)
                if (!settingsWereImported && System.IO.File.Exists(importedSettingsFilePath))
                {
                    originalSettingsContent = System.IO.File.ReadAllText(importedSettingsFilePath);
                }

                // Validate the imported settings
                bool isImported = viewModel.PreferencesViewModel.importSettingsContent(fileContent);

                if (isImported)
                {
                    // Mark that settings were imported
                    settingsWereImported = true;

                    // Show success status on splash screen
                    SetImportStatus(ImportStatus.success);

                    // Show the restart message and reset button
                    ShowRestartMessage();

                    Analytics.TrackEvent(Actions.Import, Categories.SplashScreenOperations, "true");
                }
                else
                {
                    SetImportStatus(ImportStatus.error);
                    Analytics.TrackEvent(Actions.Import, Categories.SplashScreenOperations, "false");
                }
            }
            catch (Exception ex)
            {
                viewModel?.Model?.Logger?.Log($"Error importing settings: {ex.Message}");
                SetImportStatus(ImportStatus.error);
                Analytics.TrackEvent(Actions.Import, Categories.SplashScreenOperations, "false");
            }
        }

        /// <summary>
        /// Returns true if the user was successfully logged in, else false.
        /// </summary>
        private bool SignIn()
        {
            if (!viewModel.IsIDSDKInitialized(true, this)) return false;
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
            CloseWasExplicit = false;
            if (viewModel != null)
            {
                viewModel.PreferenceSettings.EnableStaticSplashScreen = !isCheckboxChecked;
            }
            currentCloseMode = CloseMode.ByStartingDynamo;
            Close();
            dynamoView?.Show();
            dynamoView?.Activate();
        }

        private void OnLoginStateChanged(LoginState state)
        {
            HandleSignInStatusChange(authManager.IsLoggedIn());
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
                authManager.LoginStateChanged += OnLoginStateChanged;
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
            return WebView2Utilities.GetTempDirectory();
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
                UserDataFolder = DynamoModel.IsTestMode ? TestUtilities.UserDataFolderDuringTests(nameof(SplashScreen)) : webBrowserUserDataFolder.FullName
            };

            //ContentRendered ensures that the webview2 component is visible.
            try
            {
                await webView.Initialize();

                // Apply standard WebView2 settings for splash screen
                webView.ConfigureSettings();

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
                   new ScriptObject(RequestLaunchDynamo, RequestImportSettings, RequestSignIn, RequestSignOut, ResetImportedSettings));
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine(ex.Message);
            }
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
            try
            {
                if (webView?.CoreWebView2 != null)
                {
                    await webView.CoreWebView2.ExecuteScriptAsync($"window.setLoadingDone()");
                    await webView.CoreWebView2.ExecuteScriptAsync($"window.setTotalLoadingTime(\"{Wpf.Properties.Resources.SplashScreenTotalLoadingTimeLabel} {totalLoadingTime}ms\")");
                    SetSignInEnable(enableSignInButton);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
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
        /// Show the restart required message and reset button after settings import
        /// </summary>
        internal async void ShowRestartMessage()
        {
            try
            {
                if (webView?.CoreWebView2 != null)
                {
                    await webView.CoreWebView2.ExecuteScriptAsync("window.showRestartMessage()");
                }
            }
            catch (Exception ex)
            {
                viewModel?.Model?.Logger?.Log($"Error showing restart message: {ex.Message}");
            }
        }

        /// <summary>
        /// Hide the restart required message and reset button
        /// </summary>
        internal async void HideRestartMessage()
        {
            try
            {
                if (webView?.CoreWebView2 != null)
                {
                    await webView.CoreWebView2.ExecuteScriptAsync("window.hideRestartMessage()");
                }
            }
            catch (Exception ex)
            {
                viewModel?.Model?.Logger?.Log($"Error hiding restart message: {ex.Message}");
            }
        }

        /// <summary>
        /// Resets the imported settings to the original settings that were backed up
        /// </summary>
        private void ResetImportedSettings()
        {
            try
            {
                if (!string.IsNullOrEmpty(originalSettingsContent) &&
                    !string.IsNullOrEmpty(importedSettingsFilePath))
                {
                    // Restore original settings to disk
                    System.IO.File.WriteAllText(importedSettingsFilePath, originalSettingsContent);

                    // Reload the original settings into the current session
                    var restoredPreferences = PreferenceSettings.LoadContent(originalSettingsContent);
                    if (restoredPreferences.IsCreatedFromValidFile)
                    {
                        viewModel.PreferencesViewModel.importSettingsContent(originalSettingsContent);
                    }

                    // Clear the imported flag and backup
                    settingsWereImported = false;
                    originalSettingsContent = null;

                    // Hide the restart message
                    HideRestartMessage();

                    // Show reset success message briefly
                    SetImportStatus(ImportStatus.none);

                    viewModel?.Model?.Logger?.Log(Wpf.Properties.Resources.SplashScreenSettingsResetSuccess);
                }
            }
            catch (Exception ex)
            {
                viewModel?.Model?.Logger?.Log($"Error resetting settings: {ex.Message}");
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
        /// Handle the login status changes on splash screen.
        /// </summary>
        internal async void HandleSignInStatusChange(bool status)
        {
            if (webView?.CoreWebView2 != null)
            {
                await webView.CoreWebView2.ExecuteScriptAsync(@$"window.handleSignInStateChange({{""status"": ""{status}""}})");
            }
        }

        /// <summary>
        /// Enable or disable the SignIn button on splash screen.
        /// </summary>
        /// <param name="enabled"></param>
        internal async void SetSignInEnable(bool enabled)
        {
            if (webView?.CoreWebView2 != null)
            {
                await webView.CoreWebView2.ExecuteScriptAsync(@$"window.setEnableSignInButton({{""enable"": ""{enabled}""}})");
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
                   $"signInTooltip: \"{Wpf.Properties.Resources.SignInButtonContentToolTip}\"," +
                   $"signOutTitle: \"{Wpf.Properties.Resources.SplashScreenSignOut}\"," +
                   $"signOutTooltip: \"{Wpf.Properties.Resources.SignOutConfirmationDialogText}\"," +
                   $"signingInTitle: \"{Wpf.Properties.Resources.SplashScreenSigningIn}\"," +
                   $"importSettingsTooltipDescription: \"{Wpf.Properties.Resources.ImportPreferencesInfo}\"," +
                   $"restartMessage: \"{Wpf.Properties.Resources.SplashScreenSettingsImportedRestartMessage}\"," +
                   $"resetTooltip: \"{Wpf.Properties.Resources.SplashScreenResetSettingsTooltip}\"" + "})");
            }
        }

        /// <summary>
        /// At Dynamo startup process load the preferences settings file located in C:\ProgramData\Dynamo
        /// </summary>
        internal void LoadPreferencesFileAtStartup()
        {
            if (viewModel.PreferenceSettings.IsFirstRun == true)
            {
                var programDataDir = string.Empty;

                try
                {
                    //This code will be executed only when Dynamo is running inside any Host application like Revit, FormIt, Civil3D
                    if (viewModel.Model.PathManager.HostApplicationDirectory != null && !string.IsNullOrEmpty(viewModel.Model.HostVersion))
                    {
                        //Move the current location two levels up for finding the DynamoSettings.xml file (just for Hosts)
                        var firstParent = Directory.GetParent(viewModel.Model.PathManager.CommonDataDirectory);
                        if (firstParent != null)
                        {
                            var secondParent = Directory.GetParent(firstParent.ToString());
                            if (secondParent != null)
                            {
                                programDataDir = secondParent.ToString();
                            }
                            else
                            {
                                // Fallback: use firstParent or handle as appropriate
                                programDataDir = firstParent.ToString();
                            }
                        }
                        else
                        {
                            // Fallback: use CommonDataDirectory or handle as appropriate
                            programDataDir = viewModel.Model.PathManager.CommonDataDirectory;
                        }
                    }
                    //This code will be executed when Dynamo is running as a standalone application
                    else
                    {
                        programDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), Configurations.DynamoAsString);
                    }

                    if (Directory.Exists(programDataDir))
                    {
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
                catch (Exception ex)
                {
                    viewModel.Model.Logger.Log("Error loading preferences from programDataDir: " + ex.Message);
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

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            DynamoModel.RequestUpdateLoadBarStatus -= DynamoModel_RequestUpdateLoadBarStatus;
            DynamoModel.LanguageDetected -= DynamoModel_LanguageDetected;
            StaticSplashScreenReady -= OnStaticScreenReady;
            if (authManager is not null)
            {
                authManager.LoginStateChanged -= OnLoginStateChanged;
            }

            if (webView != null)
            {
                webView.Dispose();
                webView = null;
            }

            GC.SuppressFinalize(this);
        }
    }

    enum ImportStatus
    {
        none = 1,
        error,
        success
    }

    /// <summary>
    /// This class is used to expose the methods that can be called from the webview2 component, SplashScreen.
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class ScriptObject
    {
        readonly Action<bool> RequestLaunchDynamo;
        readonly Action<string> RequestImportSettings;
        readonly Func<bool> RequestSignIn;
        readonly Func<bool> RequestSignOut;
        readonly Action RequestResetSettings;

        [Obsolete("This constructor is obsolete. Use the constructor with the additional parameters instead.")]
        public ScriptObject(Action<bool> requestLaunchDynamo, Action<string> requestImportSettings, Func<bool> requestSignIn, Func<bool> requestSignOut)
        {
            RequestLaunchDynamo = requestLaunchDynamo;
            RequestImportSettings = requestImportSettings;
            RequestSignIn = requestSignIn;
            RequestSignOut = requestSignOut;
        }

        /// <summary>
        /// Constructor for ScriptObject
        /// </summary>
        public ScriptObject(Action<bool> requestLaunchDynamo, Action<string> requestImportSettings, Func< bool> requestSignIn, Func<bool> requestSignOut, Action requestResetSettings)
        {
            RequestLaunchDynamo = requestLaunchDynamo;
            RequestImportSettings = requestImportSettings;
            RequestSignIn = requestSignIn;
            RequestSignOut = requestSignOut;
            RequestResetSettings = requestResetSettings;
        }

        [DynamoJSInvokable]
        public void LaunchDynamo(bool showScreenAgain)
        {
            RequestLaunchDynamo(showScreenAgain);
            Analytics.TrackEvent(Actions.Start, Categories.SplashScreenOperations);
        }

        [DynamoJSInvokable]
        public void ImportSettings(string file)
        {
            RequestImportSettings(file);
        }

        [DynamoJSInvokable]
        public bool SignIn()
        {
            return RequestSignIn();
        }

        [DynamoJSInvokable]
        public bool SignOut()
        {
            return RequestSignOut();
        }

        [DynamoJSInvokable]
        public void ResetSettings()
        {
            RequestResetSettings();
        }
    }
}
