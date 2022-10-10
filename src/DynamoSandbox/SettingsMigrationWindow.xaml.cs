using Microsoft.Web.WebView2.Core;
using System;
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

        internal Action RequestLaunchDynamo;
        internal Action<string> RequestImportSettings;

        public SettingsMigrationWindow()
        {
            InitializeComponent();
        }

        protected override async void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            string htmlString = string.Empty;

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
                htmlString = htmlString.Replace("mainJs", jsString);
            }

            webView.NavigateToString(htmlString);
            webView.CoreWebView2.AddHostObjectToScript("scriptObject",
               new ScriptObject(RequestLaunchDynamo, RequestImportSettings));

        }

        internal async void SetBarProperties(string version, string loadingDescription, float barSize, int loadingTime)
        {
            await webView.CoreWebView2.ExecuteScriptAsync($"window.setBarProperties('{version}','{loadingDescription}', '{barSize}%', 'Loading time: {loadingTime}ms')");
        }

        internal async void SetLoadingDone()
        {
            await webView.CoreWebView2.ExecuteScriptAsync($"window.setLoadingDone()");
        }

        private async void SetImportStatus(ImportStatus importStatus, string importSettingsTitle, string errorDescription)
        {
            await webView.CoreWebView2.ExecuteScriptAsync("window.setImportStatus({" +
                $"importStatus: {(int)importStatus}" +
                $"importSettingsTitle: {importSettingsTitle}" +
                $"errorDescription: {errorDescription}" + "'})");
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
        Action RequestLaunchDynamo;
        Action<string> RequestImportSettings;

        public ScriptObject(Action requestLaunchDynamo, Action<string> requestImportSettings)
        {
            RequestLaunchDynamo = requestLaunchDynamo;
            RequestImportSettings = requestImportSettings;
        }

        public void LaunchDynamo(bool showScreenAgain)
        {
            RequestLaunchDynamo();
        }

        public void ImportSettings(string file)
        {
            RequestImportSettings(file);
        }
    }
}
