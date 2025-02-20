using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Utilities;
using Dynamo.Wpf.Utilities;
using DynamoUtilities;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Autodesk.DesignScript.Runtime;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dynamo.PackageManager;
using Dynamo.PackageManager.UI;

namespace Dynamo.UI.Views
{
    /// <summary>
    /// Interaction logic for PackageManagerWizard.xaml
    /// </summary>
    public partial class PackageManagerWizard : UserControl
    {
        #region Properties
        private static readonly string fontStylePath = "Dynamo.Wpf.Views.GuidedTour.HtmlPages.Resources.ArtifaktElement-Regular.woff";
        private static readonly string virtualFolderName = "embeddedFonts";
        private static readonly string fontUrl = $"http://{virtualFolderName}/ArtifaktElement-Regular.woff";
        private static readonly string virtualFolderPath = Path.Combine(Path.GetTempPath(), virtualFolderName);
        private static readonly string htmlRelativeFilePath = $"PackageManager\\index.html";

        private bool _onlyLoadOnce = false;
        private bool _disposed = false;

        private PublishPackageViewModel publishPackageViewModel;

        internal Action<string> RequestAddFileOrFolder;
        internal Action<string> RequestRemoveFileOrFolder;
        
        #endregion

        /// <summary>
        /// The WebView2 Browser instance used to display splash screen
        /// </summary>
        internal DynamoWebView2 dynWebView;

        public PackageManagerWizard()
        {
            InitializeComponent();

            dynWebView = new DynamoWebView2
            {
                Margin = new System.Windows.Thickness(0),  
                ZoomFactor = 1.0 
            };

            HostGrid.Children.Add(dynWebView);

            // Bind event handlers
            RequestAddFileOrFolder = AddFileOrFolder;
            RequestRemoveFileOrFolder = RemoveFileOrFolder;

            DataContextChanged += OnDataContextChanged;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_onlyLoadOnce) return;
            _onlyLoadOnce = true;

            // Ensure WebView2 is initialized if it hasn't been preloaded
            await PreloadWebView2Async();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            publishPackageViewModel = this.DataContext as PublishPackageViewModel;
            if (publishPackageViewModel != null)
            {
                publishPackageViewModel.PropertyChanged += PublishPackageViewModel_PropertyChanged;
            }
        }

        private List<PackageItemRootViewModel> _previousPackageContents = new List<PackageItemRootViewModel>();
        private void PublishPackageViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(publishPackageViewModel.PackageContents)))
            {
                var updatedContents = publishPackageViewModel.PackageContents.ToList();

                // Compare the previous state with the new state
                var changesDetected = !ArePackageContentsEqual(_previousPackageContents, updatedContents);

                if (changesDetected)
                {
                    _previousPackageContents = updatedContents; // Update the snapshot

                    // Convert to front-end format and send update
                    var frontendData = ConvertToJsonFormat(updatedContents);
                    SendUpdatedPackageContents(frontendData);
                }
            }
        }

        private async void SendUpdatedPackageContents(object frontendData)
        {
            if (frontendData != null)
            {
                // Serialize to JSON
                string jsonData = JsonSerializer.Serialize(frontendData);

                // Send to the front-end if WebView2 is available
                if (dynWebView?.CoreWebView2 != null)
                {
                    await dynWebView.CoreWebView2.ExecuteScriptAsync(@$"window.receiveUpdatedPackageContents({jsonData})");
                }
            }
        }

        internal async Task PreloadWebView2Async()
        {
            if (_onlyLoadOnce) return;
            _onlyLoadOnce = true;

            // When executing Dynamo as Sandbox or inside any host like Revit, FormIt, Civil3D the WebView2 cache folder will be located in the AppData folder
            var userDataDir = new DirectoryInfo(GetUserDirectory());
            PathHelper.CreateFolderIfNotExist(userDataDir.ToString());

            var webBrowserUserDataFolder = userDataDir.Exists ? userDataDir : null;

            dynWebView.CreationProperties = new CoreWebView2CreationProperties
            {
                UserDataFolder = webBrowserUserDataFolder.FullName
            };

            // Pre-generate the html temp file
            string tempFilePath = GetFileFromRelativePath(htmlRelativeFilePath);

            try
            {
                // Initialize the webview asynchroneously 
                await dynWebView?.Initialize();

                // Set WebView2 settings    
                this.dynWebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                this.dynWebView.CoreWebView2.Settings.IsZoomControlEnabled = false;
                this.dynWebView.CoreWebView2.Settings.AreDevToolsEnabled = true;
                this.dynWebView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;

                // Embed the font
                var assembly = Assembly.GetExecutingAssembly();
                PathHelper.ExtractAndSaveEmbeddedFont(fontStylePath, virtualFolderPath, "ArtifaktElement-Regular.woff", assembly);

                // Set up virtual host name to folder mapping
                dynWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(virtualFolderName, virtualFolderPath, CoreWebView2HostResourceAccessKind.Allow);

                try
                {
                    // Navigate to the temp html file
                    dynWebView.Source = new Uri(tempFilePath);

                    // Exposing commands to the React front-end
                    dynWebView.CoreWebView2.AddHostObjectToScript("scriptObject",
                        new ScriptWizardObject(
                            RequestAddFileOrFolder,
                            RequestRemoveFileOrFolder));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    //this.startPage.DynamoViewModel.Model.Logger.Log(ex.Message);
                }
            }
            catch (ObjectDisposedException ex)  
            {
                Console.WriteLine(ex.Message);
                //this.startPage.DynamoViewModel.Model.Logger.Log(ex.Message);
            }
        }

        #region Relay Commands
        internal void AddFileOrFolder(string fileOrFolder)
        {
            if (string.IsNullOrEmpty(fileOrFolder) || publishPackageViewModel == null)
                return;

            if (fileOrFolder.Equals("File", StringComparison.OrdinalIgnoreCase))
            {
                if (publishPackageViewModel.ShowAddFileDialogAndAddCommand?.CanExecute() == true)
                {
                    publishPackageViewModel.ShowAddFileDialogAndAddCommand.Execute();
                }
            }
            else if (fileOrFolder.Equals("Folder", StringComparison.OrdinalIgnoreCase))
            {
                if (publishPackageViewModel.SelectDirectoryAndAddFilesRecursivelyCommand?.CanExecute() == true)
                {
                    publishPackageViewModel.SelectDirectoryAndAddFilesRecursivelyCommand.Execute();
                }
            }
        }

        public void RemoveFileOrFolder(string name)
        {
            var itemToRemove = publishPackageViewModel.PackageContents
                .FirstOrDefault(item => item.DisplayName == name);

            if (itemToRemove != null)
            {
                publishPackageViewModel.PackageContents.Remove(itemToRemove);
            }
        }

        #endregion

        #region Utility
        /// <summary>
        ///  Generates the absolute runtime filePath of the html file
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private string GetFileFromRelativePath(string relativePath)
        {
            var rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new ArgumentNullException(nameof(Path.GetDirectoryName));
            var toolPath = Path.Combine(rootPath, relativePath);

            return toolPath;
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

            Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
            Logging.Analytics.TrackEvent(Logging.Actions.Open, Logging.Categories.DynamoHomeOperations, "Hyper Link: " + uri);

            return false;
        }

        /// <summary>
        /// This is used before DynamoModel initialization specifically to get user data dir
        /// </summary>
        /// <returns></returns>
        private static string GetUserDirectory()
        {
            var version = AssemblyHelper.GetDynamoVersion();

            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(Path.Combine(folder, "Dynamo", "Dynamo Core"),
                            String.Format("{0}.{1}", version.Major, version.Minor));
        }

        /// <summary>
        /// Compare package contents using hashsets for quick comparison
        /// </summary>
        /// <param name="oldContents"></param>
        /// <param name="newContents"></param>
        /// <returns></returns>
        private bool ArePackageContentsEqual(List<PackageItemRootViewModel> oldContents, List<PackageItemRootViewModel> newContents)
        {
            if (oldContents.Count != newContents.Count) return false;

            var oldSet = new HashSet<string>(oldContents.Select(item => item.FilePath));
            var newSet = new HashSet<string>(newContents.Select(item => item.FilePath));

            return oldSet.SetEquals(newSet);
        }

        /// <summary>
        /// Coverts a list of PackageItemRootViewModel to simple json structure
        /// </summary>
        /// <param name="packageContents"></param>
        /// <returns></returns>
        private object ConvertToJsonFormat(List<PackageItemRootViewModel> packageContents)
        {
            return new
            {
                name = "root",
                children = packageContents.Select(ConvertToNode).ToList()
            };
        }

        /// <summary>
        /// Recursive function to convert to simple json structrue
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private object ConvertToNode(PackageItemRootViewModel node)
        {
            return new
            {
                name = node.DisplayName,
                children = node.ChildItems?.Select(ConvertToNode).ToList() ?? new List<object>()
            };
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Public implementation of Dispose pattern callable by consumers (as per Microsoft documentation)
        /// https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern
        /// </summary>
        /// <param name="disposing">To be called by the finalizer if necessary</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Unsubscribe from events
                    //DataContextChanged -= OnDataContextChanged;
                    //if (startPage != null)
                    //{
                    //    if (startPage.DynamoViewModel != null)
                    //    {
                    //        startPage.DynamoViewModel.PropertyChanged -= DynamoViewModel_PropertyChanged;
                    //        if (startPage.DynamoViewModel.RecentFiles != null)
                    //        {
                    //            startPage.DynamoViewModel.RecentFiles.CollectionChanged -= RecentFiles_CollectionChanged;
                    //        }
                    //    }
                    //}

                    if (this.dynWebView != null && this.dynWebView.CoreWebView2 != null)
                    {
                        this.dynWebView.CoreWebView2.NewWindowRequested -= CoreWebView2_NewWindowRequested;
                    }

                    // Delete font file if it exists
                    //if (File.Exists(fontFilePath))
                    //{
                    //    File.Delete(fontFilePath);
                    //}
                }

                _disposed = true;
            }
        }
        #endregion
    }


    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class ScriptWizardObject
    {
        readonly Action<string> RequestAddFileOrFolder;
        readonly Action<string> RequestRemoveFileOrFolder;

        public ScriptWizardObject(
            Action<string> requestAddFileOrFolder,
            Action<string> requestRemoveFileOrFolder)
        {
            RequestAddFileOrFolder = requestAddFileOrFolder;
            RequestRemoveFileOrFolder = requestRemoveFileOrFolder;
            RequestRemoveFileOrFolder = requestRemoveFileOrFolder;
        }

        [DynamoJSInvokable]
        public void AddFileOrFolder(string fileOrFolder)
        {
            RequestAddFileOrFolder(fileOrFolder);
        }


        [DynamoJSInvokable]
        public void RemoveFileOrFolder(string name)
        {
            RequestRemoveFileOrFolder(name);
        }
    }
}
