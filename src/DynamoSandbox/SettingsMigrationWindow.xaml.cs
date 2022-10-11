using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

namespace Dynamo.DynamoSandbox
{
    public partial class SettingsMigrationWindow : Window
    {
        private static readonly string htmlEmbeddedFile = "Dynamo.DynamoSandbox.WebApp.index.html";
        private static readonly string jsEmbeddedFile = "Dynamo.DynamoSandbox.WebApp.index.bundle.js";
        private static readonly string backgroundImage = "Dynamo.DynamoSandbox.WebApp.splashScreenBackground.png";
        private static readonly string imageFileExtension = "png";

        private Stopwatch loadingTimer;

        internal Action<bool> RequestLaunchDynamo;
        internal Action<string> RequestImportSettings;

        public SettingsMigrationWindow()
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
               new ScriptObject(RequestLaunchDynamo, RequestImportSettings));

        }

        internal async void SetBarProperties(string version, string loadingDescription, float barSize)
        {
            var elapsedTime = loadingTimer.ElapsedMilliseconds;
            loadingTimer = Stopwatch.StartNew();
            await webView.CoreWebView2.ExecuteScriptAsync($"window.setBarProperties('{version}','{loadingDescription}', '{barSize}%', 'Loading time: {elapsedTime}ms')");
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
        Action<bool> RequestLaunchDynamo;
        Action<string> RequestImportSettings;

        public ScriptObject(Action<bool> requestLaunchDynamo, Action<string> requestImportSettings)
        {
            RequestLaunchDynamo = requestLaunchDynamo;
            RequestImportSettings = requestImportSettings;
        }

        public void LaunchDynamo(bool showScreenAgain)
        {
            RequestLaunchDynamo(showScreenAgain);
        }

        public void ImportSettings(string file)
        {
            RequestImportSettings(file);
        }
    }
}
