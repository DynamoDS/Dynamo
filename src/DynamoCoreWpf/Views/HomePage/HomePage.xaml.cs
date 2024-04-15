using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Autodesk.DesignScript.Runtime;
using Dynamo.Models;
using Dynamo.UI.Controls;
using Dynamo.Utilities;
using Dynamo.Wpf.UI.GuidedTour;
using Dynamo.Wpf.Utilities;
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
        private static readonly string htmlEmbeddedFile = "Dynamo.Wpf.Packages.DynamoHome.build.index.html";
        private static readonly string jsEmbeddedFile = "Dynamo.Wpf.Packages.DynamoHome.build.index.bundle.js";
        private static readonly string fontStylePath = "Dynamo.Wpf.Views.GuidedTour.HtmlPages.Resources.ArtifaktElement-Regular.woff";
        private static readonly string virtualFolderName = "embeddedFonts";
        private static readonly string fontUrl = $"http://{virtualFolderName}/ArtifaktElement-Regular.woff";
        private static readonly string virtualFolderPath = Path.Combine(Path.GetTempPath(), virtualFolderName);

        private string fontFilePath;

        private StartPageViewModel startPage;

        /// <summary>
        /// The WebView2 Browser instance used to display splash screen
        /// </summary>
        internal DynamoWebView2 dynWebView;

        internal Action<string> RequestOpenFile;
        internal Action<string> RequestShowGuidedTour;
        internal Action RequestNewWorkspace;
        internal Action RequestOpenWorkspace;
        internal Action RequestNewCustomNodeWorkspace;
        internal Action RequestApplicationLoaded;
        internal Action RequestShowSampleFilesInFolder;
        internal Action RequestShowBackupFilesInFolder;
        internal Action RequestShowTemplate;

        internal List<GuidedTourItem> GuidedTourItems;

        /// <summary>
        /// A helper tool to let us test flows without relying on side-effects
        /// </summary>
        internal static Action<string> TestHook { get; set; }

        public HomePage()
        {   
            InitializeComponent();
            InitializeGuideTourItems();

            dynWebView = new DynamoWebView2();

            dynWebView.Margin = new System.Windows.Thickness(0);  // Set margin to zero
            dynWebView.ZoomFactor = 1.0;  // Set zoom factor (optional)
                
            HostGrid.Children.Add(dynWebView);

            // Bind event handlers
            RequestOpenFile = OpenFile;
            RequestShowGuidedTour = StartGuidedTour;
            RequestNewWorkspace = NewWorkspace;
            RequestOpenWorkspace = OpenWorkspace;
            RequestNewCustomNodeWorkspace = NewCustomNodeWorkspace;
            RequestShowSampleFilesInFolder = ShowSampleFilesInFolder;
            RequestShowBackupFilesInFolder = ShowBackupFilesInFolder;
            RequestShowTemplate = ShowTemplate;
            RequestApplicationLoaded = ApplicationLoaded;

            DataContextChanged += OnDataContextChanged;

        }


        private void InitializeGuideTourItems()
        {
            GuidedTourItems = new List<GuidedTourItem>
            {
                new GuidedTourItem(Wpf.Properties.Resources.GetStartedGuide.TrimStart('_'),
                    Wpf.Properties.Resources.GetStartedGuideDescription, GuidedTourType.UserInterface.ToString()),
                new GuidedTourItem(Wpf.Properties.Resources.OnboardingGuide.TrimStart('_'),
                    Wpf.Properties.Resources.OnboardingGuideDescription, GuidedTourType.GetStarted.ToString()),
                new GuidedTourItem(Wpf.Properties.Resources.PackagesGuide.TrimStart('_'),
                    Wpf.Properties.Resources.PackagesGuideDescription, GuidedTourType.Packages.ToString())
            };
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            startPage = this.DataContext as StartPageViewModel;
                
            if (startPage != null)
            {
                startPage.DynamoViewModel.PropertyChanged += DynamoViewModel_PropertyChanged;
            }
        }

        private void DynamoViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(dynWebView?.CoreWebView2 != null && e.PropertyName.Equals(nameof(startPage.DynamoViewModel.ShowStartPage)))
            {
                dynWebView.CoreWebView2.ExecuteScriptAsync(@$"window.setShowStartPageChanged('{startPage.DynamoViewModel.ShowStartPage}')");
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

            dynWebView.CreationProperties = new CoreWebView2CreationProperties
            {
                UserDataFolder = webBrowserUserDataFolder.FullName
            };

            //ContentRendered ensures that the webview2 component is visible.
            try
            {
                await dynWebView.Initialize();

                // Set WebView2 settings
                this.dynWebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                this.dynWebView.CoreWebView2.Settings.IsZoomControlEnabled = false;
                this.dynWebView.CoreWebView2.Settings.AreDevToolsEnabled = true;
                this.dynWebView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;

                // Load the embeded resources
                var assembly = Assembly.GetExecutingAssembly();

                htmlString = PathHelper.LoadEmbeddedResourceAsString(htmlEmbeddedFile, assembly);
                jsonString = PathHelper.LoadEmbeddedResourceAsString(jsEmbeddedFile, assembly);

                // Embed the font
                PathHelper.ExtractAndSaveEmbeddedFont(fontStylePath, virtualFolderPath, "ArtifaktElement-Regular.woff", assembly);

                // Set up virtual host name to folder mapping
                dynWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(virtualFolderName, virtualFolderPath, CoreWebView2HostResourceAccessKind.Allow);

                htmlString = htmlString.Replace("mainJs", jsonString);
                htmlString = htmlString.Replace("#fontStyle", fontUrl);

                try
                {
                    dynWebView.NavigateToString(htmlString);
                }
                catch (Exception ex)
                {
                    this.startPage.DynamoViewModel.Model.Logger.Log(ex.Message);
                }

                // Exposing commands to the React front-end
                dynWebView.CoreWebView2.AddHostObjectToScript("scriptObject",
                   new ScriptHomeObject(RequestOpenFile,
                                        RequestNewWorkspace,
                                        RequestOpenWorkspace,
                                        RequestNewCustomNodeWorkspace,
                                        RequestApplicationLoaded,
                                        RequestShowGuidedTour,
                                        RequestShowSampleFilesInFolder,
                                        RequestShowBackupFilesInFolder,
                                        RequestShowTemplate));
            }
            catch (ObjectDisposedException ex)
            {
                this.startPage.DynamoViewModel.Model.Logger.Log(ex.Message);
            }
        }

        private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;
            ProcessUri(e.Uri);
        }

        internal bool ProcessUri(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri)) return false;

            Uri fileUri;
            try
            {
                fileUri = new Uri(uri);
            }
            catch (UriFormatException)
            {
                return false;
            }

            if (fileUri.IsFile)
            {
                var filePath = fileUri.LocalPath;
                if (filePath.EndsWith(".dyn") || filePath.EndsWith(".dyf"))
                {
                    OpenFile(filePath);
                    return true;
                }
            }
            else
            {
                Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
            }

            return false;
        }

        internal async void LoadingDone()   
        {
            SendGuidesData();

            if (startPage == null) { return; }

            SendSamplesData();

            var recentFiles = startPage.RecentFiles;
            if (recentFiles == null || !recentFiles.Any()) { return; }

            LoadGraphs(recentFiles);
                
            var userLocale = CultureInfo.CurrentCulture.Name;
                
            if (dynWebView?.CoreWebView2 != null)
            {
                await dynWebView.CoreWebView2.ExecuteScriptAsync(@$"window.setLocale('{userLocale}');");
            }
        }

        #region FrontEnd Initialization Calls
        /// <summary>
        /// Sends graph data to react app
        /// </summary>
        /// <param name="data"></param>
        private async void LoadGraphs(ObservableCollection<StartPageListItem> data)
        {
            string jsonData = JsonSerializer.Serialize(data);

            if (dynWebView?.CoreWebView2 != null)
            {
                await dynWebView.CoreWebView2.ExecuteScriptAsync(@$"window.receiveGraphDataFromDotNet({jsonData})");
            }
        }

        /// <summary>
        /// Sends samples data to react app
        /// </summary>
        private async void SendSamplesData()
        {
            if (!this.startPage.SampleFiles.Any()) return;

            string jsonData = JsonSerializer.Serialize(this.startPage.SampleFiles);

            if (dynWebView?.CoreWebView2 != null)
            {
                await dynWebView.CoreWebView2.ExecuteScriptAsync(@$"window.receiveSamplesDataFromDotNet({jsonData})");
            }
        }


        /// <summary>
        /// Sends guided tour data to react app
        /// </summary>
        private async void SendGuidesData()
        {
            if (!this.GuidedTourItems.Any()) return;

            string jsonData = JsonSerializer.Serialize(this.GuidedTourItems);

            if (dynWebView?.CoreWebView2 != null)
            {
                await dynWebView.CoreWebView2.ExecuteScriptAsync(@$"window.receiveInteractiveGuidesDataFromDotNet({jsonData})");
            }
        }
        #endregion

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
            }
        }

        #endregion

        #region Relay Commands
        internal void OpenFile(string path)
        {
            if (String.IsNullOrEmpty(path)) return;
            if (DynamoModel.IsTestMode)
            {
                TestHook?.Invoke(path);
                return;
            }

            if (this.startPage.DynamoViewModel.OpenCommand.CanExecute(path))
                this.startPage.DynamoViewModel.OpenCommand.Execute(path);
        }

        internal void StartGuidedTour(string path)
        {
            if (String.IsNullOrEmpty(path)) return;
            if (DynamoModel.IsTestMode)
            {
                TestHook?.Invoke(path);
                return;
            }

            ShowGuidedTour(path);
        }

        internal void NewWorkspace()
        {
            this.startPage?.DynamoViewModel?.NewHomeWorkspaceCommand.Execute(null);
        }

        internal void OpenWorkspace()
        {
            if (DynamoModel.IsTestMode)
            {
                TestHook?.Invoke(string.Empty);
                return;
            }

            this.startPage?.DynamoViewModel?.ShowOpenDialogAndOpenResultCommand.Execute(null);
        }

        internal void NewCustomNodeWorkspace()
        {
            if (DynamoModel.IsTestMode)
            {
                TestHook?.Invoke(string.Empty);
                return;
            }

            this.startPage?.DynamoViewModel?.ShowNewFunctionDialogCommand.Execute(null);
        }

        internal void ShowSampleFilesInFolder()
        {
            if (this.startPage == null) return;
            if (DynamoModel.IsTestMode)
            {
                TestHook?.Invoke(string.Empty);
                return;
            }

            Process.Start(new ProcessStartInfo("explorer.exe", "/select,"
                + this.startPage.SampleFolderPath)
            { UseShellExecute = true });
        }

        internal void ShowBackupFilesInFolder()
        {
            if (this.startPage == null) return;
            if (DynamoModel.IsTestMode)
            {
                TestHook?.Invoke(string.Empty);
                return;
            }

            Process.Start(new ProcessStartInfo("explorer.exe", this.startPage.DynamoViewModel.Model.PathManager.BackupDirectory)
            { UseShellExecute = true });
        }

        internal void ShowTemplate()
        {
            if (DynamoModel.IsTestMode)
            {
                TestHook?.Invoke(string.Empty);
                return;
            }

            // Equivalent to CommandParameter="Template"
            this.startPage?.DynamoViewModel?.ShowOpenTemplateDialogCommand.Execute("Template"); 
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
            if(startPage != null) startPage.DynamoViewModel.PropertyChanged -= DynamoViewModel_PropertyChanged;

            this.dynWebView.CoreWebView2.NewWindowRequested -= CoreWebView2_NewWindowRequested;

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
        readonly Action RequestShowTemplate;

        public ScriptHomeObject(Action<string> requestOpenFile,
            Action requestNewWorkspace,
            Action requestOpenWorkspace,
            Action requestNewCustomNodeWorkspace,
            Action requestApplicationLoaded,
            Action<string> requestShowGuidedTour,
            Action requestShowSampleFilesInFolder,
            Action requestShowBackupFilesInFolder,
            Action requestShowTemplate)
        {
            RequestOpenFile = requestOpenFile;
            RequestNewWorkspace = requestNewWorkspace;
            RequestOpenWorkspace = requestOpenWorkspace;
            RequestNewCustomNodeWorkspace = requestNewCustomNodeWorkspace;
            RequestApplicationLoaded = requestApplicationLoaded;
            RequestShowGuidedTour = requestShowGuidedTour;
            RequestShowSampleFilesInFolder = requestShowSampleFilesInFolder;
            RequestShowBackupFilesInFolder = requestShowBackupFilesInFolder;
            RequestShowTemplate = requestShowTemplate;

        }
        [DynamoJSInvokable]
        public void OpenFile(string path)
        {
            RequestOpenFile(path);
        }
        [DynamoJSInvokable]
        public void StartGuidedTour(string path)
        {
            RequestShowGuidedTour(path);
        }
        [DynamoJSInvokable]
        public void NewWorkspace()
        {
            RequestNewWorkspace();
        }
        [DynamoJSInvokable]
        public void OpenWorkspace()
        {
            RequestOpenWorkspace();
        }
        [DynamoJSInvokable]
        public void NewCustomNodeWorkspace()
        {
            RequestNewCustomNodeWorkspace();
        }
        [DynamoJSInvokable]
        public void ShowSampleFilesInFolder()
        {
            RequestShowSampleFilesInFolder();
        }
        [DynamoJSInvokable]
        public void ShowBackupFilesInFolder()
        {
            RequestShowBackupFilesInFolder();
        }

        [DynamoJSInvokable]
        public void ShowTempalte()
        {
            RequestShowTemplate();
        }
        [DynamoJSInvokable]
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
