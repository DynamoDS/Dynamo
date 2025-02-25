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
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.Runtime.InteropServices.JavaScript;

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
        private List<PackageItemRootViewModel> _previousPackageContents = new List<PackageItemRootViewModel>();
        private List<PackageItemRootViewModel> _previousPreviewPackageContents = new List<PackageItemRootViewModel>();

        internal Action<string> RequestAddFileOrFolder;
        internal Action<string> RequestRemoveFileOrFolder;
        internal Action<bool> RequestRetainFolderStructure;

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
            RequestRetainFolderStructure = ToggleRetainFolderStructure;

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

        private void PublishPackageViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(publishPackageViewModel.PackageContents)))
            {
                var updatedContents = publishPackageViewModel.PackageContents
                    .Where(item => item.DependencyType != DependencyType.CustomNode) // Filter out CustomNode
                    .ToList();

                var changesDetected = !ArePackageContentsEqual(_previousPackageContents, updatedContents);
                if (changesDetected)
                {
                    _previousPackageContents = updatedContents; // Update the snapshot
                    var frontendData = ConvertToJsonFormat(updatedContents);
                    SendUpdatedPackageContents(frontendData, "package"); // Specify "package"
                }
            }
            else if (e.PropertyName.Equals(nameof(publishPackageViewModel.PreviewPackageContents)))
            {
                var updatedPreviewContents = publishPackageViewModel.PreviewPackageContents
                    .Where(item => item.DependencyType != DependencyType.CustomNode) // Filter out CustomNode
                    .ToList();

                var previewChangesDetected = !ArePackageContentsEqual(_previousPreviewPackageContents, updatedPreviewContents);
                if (previewChangesDetected)
                {
                    _previousPreviewPackageContents = updatedPreviewContents;
                    var frontendPreviewData = ConvertToJsonFormat(updatedPreviewContents);
                    SendUpdatedPackageContents(frontendPreviewData, "preview"); // Specify "preview"
                }
            }
        }

        private async void SendUpdatedPackageContents(object frontendData, string type)
        {
            if (frontendData != null)
            {
                // Create an object with both jsonData and type
                var payload = new { jsonData = frontendData, type = type };

                // Serialize the payload
                string jsonPayload = JsonSerializer.Serialize(payload);

                // Send to the front-end if WebView2 is available
                if (dynWebView?.CoreWebView2 != null)
                {
                    await dynWebView.CoreWebView2.ExecuteScriptAsync($"window.receiveUpdatedPackageContents({jsonPayload})");
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
                            RequestRemoveFileOrFolder,
                            RequestRetainFolderStructure));
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

        public void RemoveFileOrFolder(string filePath)
        {
            if (publishPackageViewModel == null) return;

            var itemToRemove = FindItemByFilePath(publishPackageViewModel.PackageContents.ToList(), filePath);

            if (itemToRemove != null)
            {
                if (publishPackageViewModel.RemoveItemCommand?.CanExecute(itemToRemove) == true)
                {
                    publishPackageViewModel.RemoveItemCommand.Execute(itemToRemove);
                }
            }
        }

        private PackageItemRootViewModel FindItemByFilePath(List<PackageItemRootViewModel> items, string filePath)
        {
            foreach (var item in items)
            {
                switch (item.DependencyType)
                {
                    case DependencyType.Folder when item.DirectoryName == filePath:
                    case DependencyType.File when item.FilePath == filePath:
                    case DependencyType.CustomNodePreview when item.FilePath == filePath:
                    case DependencyType.Assembly when item.FilePath == filePath:
                        return item;
                }

                var foundItem = FindItemByFilePath(item.ChildItems.ToList(), filePath);
                if (foundItem != null)
                {
                    return foundItem;
                }
            }

            return null;
        }

        public void ToggleRetainFolderStructure(bool flag)
        {
            if (publishPackageViewModel == null) return;

            publishPackageViewModel.RetainFolderStructureOverride = flag;
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
        internal static bool ArePackageContentsEqual(List<PackageItemRootViewModel> oldContents, List<PackageItemRootViewModel> newContents)
        {
            if (oldContents.Count != newContents.Count)
                return false;

            // Sort by FilePath to ensure order-independent comparison
            var oldSorted = oldContents.OrderBy(item => item.FilePath).ToList();
            var newSorted = newContents.OrderBy(item => item.FilePath).ToList();

            for (int i = 0; i < oldSorted.Count; i++)
            {
                var oldItem = oldSorted[i];
                var newItem = newSorted[i];

                // Compare file paths directly
                if (oldItem.FilePath != newItem.FilePath)
                    return false;

                // Recursively compare child items
                if (!ArePackageContentsEqual(oldItem.ChildItems.ToList(), newItem.ChildItems.ToList()))
                    return false;
            }

            return true;
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
                children = packageContents.Select(ConvertToNode).ToList(),
                filePath = "",
                isNodeLibrary = false,
                isSelected = false,
                isFolder = true,
            };
        }

        /// <summary>
        /// Recursive function to convert to simple json structrue
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private object ConvertToNode(PackageItemRootViewModel node)
        {
            bool isFolder = node.DependencyType == DependencyType.Folder;

            return new
            {
                name = node.DisplayName,
                children = node.ChildItems?.Select(ConvertToNode).ToList() ?? new List<object>(),
                filePath = isFolder ? node.DirectoryName : node.FilePath,
                isNodeLibrary = node.IsNodeLibrary,
                isSelected = node.IsSelected,
                isFolder = isFolder
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

                    this.DataContextChanged -= OnDataContextChanged;

                    if (this.publishPackageViewModel != null)
                    {
                        this.publishPackageViewModel.PropertyChanged += PublishPackageViewModel_PropertyChanged;
                    }

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
        readonly Action<bool> RequestRetainFolderStructure;

        public ScriptWizardObject(
            Action<string> requestAddFileOrFolder,
            Action<string> requestRemoveFileOrFolder,
            Action<bool> requestRetainFolderStructure)
        {
            RequestAddFileOrFolder = requestAddFileOrFolder;
            RequestRemoveFileOrFolder = requestRemoveFileOrFolder;
            RequestRetainFolderStructure = requestRetainFolderStructure;
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


        [DynamoJSInvokable]
        public void ToggleRetainFolderStructure(bool flag)
        {
            RequestRetainFolderStructure(flag);
        }
    }
}
