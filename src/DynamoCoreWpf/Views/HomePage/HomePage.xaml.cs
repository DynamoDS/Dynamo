using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Dynamo.UI.Controls;
using Dynamo.Utilities;
using Dynamo.Wpf.UI.GuidedTour;
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
        private static readonly string jsEmbeddedFile = "Dynamo.Wpf.Packages.HomePage.bundle.js";
        //private static readonly string jsEmbeddedFile = "Dynamo.Wpf.Packages.HomePage.build.bundle.js";
        private static readonly string fontStylePath = "Dynamo.Wpf.Views.GuidedTour.HtmlPages.Resources.ArtifaktElement-Regular.woff";
        private static readonly string virtualFolderName = "embeddedFonts";
        private static readonly string virtualFolderPath = Path.Combine(Path.GetTempPath(), virtualFolderName);

        private string fontFilePath;

        private StartPageViewModel startPage;

        /// <summary>
        /// The WebView2 Browser instance used to display splash screen
        /// </summary>
        internal WebView2 webView;

        internal Action<string> RequestOpenFile;
        internal Action<string> RequestShowGuidedTour;
        internal Action RequestNewWorkspace;
        internal Action RequestOpenWorkspace;
        internal Action RequestNewCustomNodeWorkspace;
        internal Action RequestApplicationLoaded;
        internal Action RequestShowSampleFilesInFolder;
        internal Action RequestShowBackupFilesInFolder;

        private List<GuidedTourItem> guidedTourItems;

        public HomePage()
        {   
            InitializeComponent();
            InitializeGuideTourItems();

            webView = new WebView2();

            webView.Margin = new System.Windows.Thickness(0);  // Set margin to zero
            webView.ZoomFactor = 1.0;  // Set zoom factor (optional)
                
            HostGrid.Children.Add(webView);

            // Bind event handlers
            RequestOpenFile = OpenFile;
            RequestShowGuidedTour = StartGuidedTour;
            RequestNewWorkspace = NewWorkspace;
            RequestOpenWorkspace = OpenWorkspace;
            RequestNewCustomNodeWorkspace = NewCustomNodeWorkspace;
            RequestShowSampleFilesInFolder = ShowSampleFilesInFolder;
            RequestShowBackupFilesInFolder = ShowBackupFilesInFolder;
            RequestApplicationLoaded = ApplicationLoaded;

            DataContextChanged += OnDataContextChanged; 
        }


        private void InitializeGuideTourItems()
        {
            guidedTourItems = new List<GuidedTourItem>
            {
                new GuidedTourItem(Wpf.Properties.Resources.GetStartedGuide, "Description of the guide", GuidedTourType.UserInterface.ToString()),
                new GuidedTourItem(Wpf.Properties.Resources.OnboardingGuide, "Description of the guide", GuidedTourType.GetStarted.ToString()),
                new GuidedTourItem(Wpf.Properties.Resources.PackagesGuide, "Description of the guide", GuidedTourType.Packages.ToString())
            };
        }


        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            startPage = this.DataContext as StartPageViewModel;
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

            // Exposing commands to the React front-end
            webView.CoreWebView2.AddHostObjectToScript("scriptObject",
               new ScriptHomeObject(RequestOpenFile,
               RequestNewWorkspace,
               RequestOpenWorkspace,
               RequestNewCustomNodeWorkspace,
               RequestApplicationLoaded,
               RequestShowGuidedTour,
               RequestShowSampleFilesInFolder,
               RequestShowBackupFilesInFolder));
        }

        internal async void LoadingDone()   
        {
            if (startPage == null) { return; }

            var recentFiles = startPage.RecentFiles;
            if (recentFiles == null || !recentFiles.Any()) { return; }

            LoadGraphs(recentFiles);
            SendSamplesData();
            SendGuidesData();

            var testMessage = "I am being tested";
            var userLocale = "en";

            if (webView?.CoreWebView2 != null)
            {
                await webView.CoreWebView2.ExecuteScriptAsync(@$"window.setLoadingDone('{testMessage}')");
                await webView.CoreWebView2.ExecuteScriptAsync(@$"window.setLocale('{userLocale}');");
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

        /// <summary>
        /// Sends samples data to react app
        /// </summary>
        private async void SendSamplesData()
        {
            if (!this.startPage.SampleFiles.Any()) return;

            string jsonData = JsonSerializer.Serialize(this.startPage.SampleFiles);

            if (webView?.CoreWebView2 != null)
            {
                await webView.CoreWebView2.ExecuteScriptAsync(@$"window.receiveSamplesDataFromDotNet({jsonData})");
            }
        }


        /// <summary>
        /// Sends guided tour data to react app
        /// </summary>
        private async void SendGuidesData()
        {
            if (!this.guidedTourItems.Any()) return;

            string jsonData = JsonSerializer.Serialize(this.guidedTourItems);

            if (webView?.CoreWebView2 != null)
            {
                await webView.CoreWebView2.ExecuteScriptAsync(@$"window.receiveInteractiveGuidesDataFromDotNet({jsonData})");
            }
        }

        #region Interactive Guides Commands
        internal void ShowGuidedTour(string typeString)
        {
            if (!Enum.TryParse(typeString, true, out GuidedTourType type))
            {
                return;
            }

            switch (type)
            {
                case GuidedTourType.UserInterface:
                    // This is the LandingPage, so we need to open a blank Workspace before running the Guided Tours
                    NewWorkspace();
                    ShowUserInterfaceGuidedTour();  
                    break;
                case GuidedTourType.GetStarted:
                    ShowGettingStartedGuidedTour();
                    break;
                case GuidedTourType.Packages:
                    // This is the LandingPage, so we need to open a blank Workspace before running the Guided Tours
                    NewWorkspace();
                    ShowPackagesGuidedTour();
                    break;
            }
        }

        private void ShowUserInterfaceGuidedTour()
        {
            //We pass the root UIElement to the GuidesManager so we can found other child UIElements
            try
            {
                this.startPage.DynamoViewModel.MainGuideManager.LaunchTour(GuidesManager.GetStartedGuideName);
            }
            catch (Exception)
            {
                return;
                //sidebarGrid.Visibility = Visibility.Visible;
            }
        }

        private void ShowGettingStartedGuidedTour()
        {
            try
            {
                if (this.startPage.DynamoViewModel.ClearHomeWorkspaceInternal())
                {
                    this.startPage.DynamoViewModel.OpenOnboardingGuideFile();
                    this.startPage.DynamoViewModel.MainGuideManager.LaunchTour(GuidesManager.OnboardingGuideName);
                }
            }
            catch (Exception ex)
            {
                this.startPage.DynamoViewModel.Model.Logger.Log(ex.Message);
                this.startPage.DynamoViewModel.Model.Logger.Log(ex.StackTrace);
            }
        }

        private void ShowPackagesGuidedTour()
        {
            try
            {
                this.startPage.DynamoViewModel.MainGuideManager.LaunchTour(GuidesManager.PackagesGuideName);
            }
            catch (Exception)
            {
                return;
                //sidebarGrid.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region Relay Commands
        internal void OpenFile(string path)
        {
            if (String.IsNullOrEmpty(path)) return;
            if(this.startPage.DynamoViewModel.OpenCommand.CanExecute(path))
                this.startPage.DynamoViewModel.OpenCommand.Execute(path);
        }

        internal void StartGuidedTour(string path)
        {
            if (String.IsNullOrEmpty(path)) return;
            ShowGuidedTour(path);
        }

        internal void NewWorkspace()
        {
            this.startPage.DynamoViewModel.NewHomeWorkspaceCommand.Execute(null);
        }

        internal void OpenWorkspace()
        {
            this.startPage.DynamoViewModel.ShowOpenDialogAndOpenResultCommand.Execute(null);
        }

        internal void NewCustomNodeWorkspace()
        {
            this.startPage.DynamoViewModel.ShowNewFunctionDialogCommand.Execute(null);
        }

        internal void ShowSampleFilesInFolder()
        {
            if (this.startPage == null) return;
            Process.Start(new ProcessStartInfo("explorer.exe", "/select,"
                + this.startPage.SampleFolderPath)
            { UseShellExecute = true });
        }

        internal void ShowBackupFilesInFolder()
        {
            if (this.startPage == null) return;
            Process.Start(new ProcessStartInfo("explorer.exe", this.startPage.DynamoViewModel.Model.PathManager.BackupDirectory)
            { UseShellExecute = true });
        }

        internal void ApplicationLoaded()
        {
            LoadingDone();
        }

        #endregion

        #region Dispose
        public void Dispose()
        {
            DataContextChanged -= OnDataContextChanged;

            if (File.Exists(fontFilePath))
            {
                File.Delete(fontFilePath);
            }
        }
        #endregion
    }


    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class ScriptHomeObject
    {
        readonly Action<string> RequestOpenFile;
        readonly Action RequestNewWorkspace;
        readonly Action RequestOpenWorkspace;
        readonly Action RequestNewCustomNodeWorkspace;
        readonly Action RequestApplicationLoaded;
        readonly Action<string> RequestShowGuidedTour;
        readonly Action RequestShowSampleFilesInFolder;
        readonly Action RequestShowBackupFilesInFolder;

        public ScriptHomeObject(Action<string> requestOpenFile,
            Action requestNewWorkspace,
            Action requestOpenWorkspace,
            Action requestNewCustomNodeWorkspace,
            Action requestApplicationLoaded,
            Action<string> requestShowGuidedTour,
            Action requestShowSampleFilesInFolder,
            Action requestShowBackupFilesInFolder)
        {
            RequestOpenFile = requestOpenFile;
            RequestNewWorkspace = requestNewWorkspace;
            RequestOpenWorkspace = requestOpenWorkspace;
            RequestNewCustomNodeWorkspace = requestNewCustomNodeWorkspace;
            RequestApplicationLoaded = requestApplicationLoaded;
            RequestShowGuidedTour = requestShowGuidedTour;
            RequestShowSampleFilesInFolder = requestShowSampleFilesInFolder;
            RequestShowBackupFilesInFolder = requestShowBackupFilesInFolder;

        }

        public void OpenFile(string path)
        {
            RequestOpenFile(path);
        }
        public void StartGuidedTour(string path)
        {
            RequestShowGuidedTour(path);
        }

        public void NewWorkspace()
        {
            RequestNewWorkspace();
        }

        public void OpenWorkspace()
        {
            RequestOpenWorkspace();
        }

        public void NewCustomNodeWorkspace()
        {
            RequestNewCustomNodeWorkspace();
        }

        public void ShowSampleFilesInFolder()
        {
            RequestShowSampleFilesInFolder();
        }

        public void ShowBackupFilesInFolder()
        {
            RequestShowBackupFilesInFolder();
        }

        public void ApplicationLoaded()
        {
            RequestApplicationLoaded();
        }

    }

    public enum GuidedTourType
    {
        UserInterface,
        GetStarted,
        Packages
    }

    public class GuidedTourItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }

        public GuidedTourItem(string name, string description, string type)
        {
            Name = name;
            Description = description;
            Type = type;
        }
    }
}
