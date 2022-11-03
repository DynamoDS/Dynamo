using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.Web.WebView2.Core;


namespace Dynamo.DynamoSandbox
{
    public partial class SplashScreen : Window
    {
        // These are hardcoded string and should only change when npm package structure changed or image path changed
        private static readonly string htmlEmbeddedFile = "Dynamo.DynamoSandbox.node_modules._dynamods.splash_screen.build.index.html";
        private static readonly string jsEmbeddedFile = "Dynamo.DynamoSandbox.node_modules._dynamods.splash_screen.build.index.bundle.js";
        private static readonly string backgroundImage = "Dynamo.DynamoSandbox.WebApp.splashScreenBackground.png";
        private static readonly string imageFileExtension = "png";

        private Stopwatch loadingTimer;

        internal Action<bool> RequestLaunchDynamo;
        internal Action<string> RequestImportSettings;
        internal Func<bool> RequestSignIn; 
        internal Func<bool> RequestSignOut;

        public SplashScreen()
        {
            InitializeComponent();

            loadingTimer = new Stopwatch();
            loadingTimer.Start();
        }

        protected override async void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            string htmlString = string.Empty;
            string jsonString = string.Empty;

            var webView2Environment = await CoreWebView2Environment.CreateAsync();
            await webView.EnsureCoreWebView2Async(webView2Environment);
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

            webView.NavigateToString(htmlString);
            webView.CoreWebView2.AddHostObjectToScript("scriptObject",
               new ScriptObject(RequestLaunchDynamo, RequestImportSettings, RequestSignIn, RequestSignOut));
        }

        internal async void SetBarProperties(string version, string loadingDescription, float barSize)
        {
            var elapsedTime = loadingTimer.ElapsedMilliseconds;
            loadingTimer = Stopwatch.StartNew();
            await webView.CoreWebView2.ExecuteScriptAsync($"window.setBarProperties('{version}','{loadingDescription}', '{barSize}%', '{Properties.Resources.SplashScreenLoadingTimeLabel}: {elapsedTime}ms')");
        }

        internal async void SetLoadingDone()
        {
            loadingTimer.Stop();
            loadingTimer = null;
            await webView.CoreWebView2.ExecuteScriptAsync($"window.setLoadingDone()");
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
                $"signInTitle: '" + (status ? Properties.Resources.SplashScreenSignOut : Properties.Resources.SplashScreenSignIn).ToString() + "'," +
                $"signInStatus: '" + status + "'})");
        }

        /// <summary>
        /// Setup the values for all lables on splash screen using resources
        /// </summary>
        internal async void SetLabels()
        {
            await webView.CoreWebView2.ExecuteScriptAsync("window.setLabels({" +
               $"welcomeToDynamoTitle: '{Properties.Resources.SplashScreenWelcomeToDynamo}'," +
               $"launchTitle: '{Properties.Resources.SplashScreenLaunchTitle}'," +
               $"importSettingsTitle: '{Properties.Resources.SplashScreenImportSettings}'," +
               $"showScreenAgainLabel: '{Properties.Resources.SplashScreenShowScreenAgainLabel}'" + "})");
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

        public ScriptObject(Action<bool> requestLaunchDynamo, Action<string> requestImportSettings, Func< bool> requestSignIn, Func<bool> requestSignOut)
        {
            RequestLaunchDynamo = requestLaunchDynamo;
            RequestImportSettings = requestImportSettings;
            RequestSignIn = requestSignIn;
            RequestSignOut = requestSignOut;
        }

        public void LaunchDynamo(bool showScreenAgain)
        {
            RequestLaunchDynamo(showScreenAgain);
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
    }
}
