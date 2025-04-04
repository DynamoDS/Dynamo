using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
using Greg.Requests;
using Newtonsoft.Json.Linq;
using Dynamo.Models;
using System.Globalization;

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
        private bool _applicationLoaded = false;
        private bool _disposed = false;

        private PublishPackageViewModel publishPackageViewModel;
        private PublishPackageViewModel previousViewModel = null;
        private List<PackageItemRootViewModel> _previousPackageContents = new List<PackageItemRootViewModel>();
        private List<PackageItemRootViewModel> _previousPreviewPackageContents = new List<PackageItemRootViewModel>();

        internal Action<string> RequestAddFileOrFolder;
        internal Action<string> RequestRemoveFileOrFolder;
        internal Action<bool> RequestRetainFolderStructure;
        internal Action<string> RequestUpdatePackageDetails;
        internal Action RequestSubmit;
        internal Action RequestPublish;
        internal Action RequestReset;
        internal Action<string> RequestToggleNodeLibraryOnItem;
        internal Action<string> RequestOpenFolder;
        internal Action<string> RequestUpdateCompatibilityMatrix;
        internal Action RequestLoadMarkdownContent;
        internal Action RequestClearMarkdownContent;
        internal Action<string> RequestLogMessage;
        internal Action RequestApplicationLoaded;
        internal Action<string, string> RequestShowDialog;

        private PackageUpdateRequest previousPackageDetails;

        private bool _hasPendingUpdates = false;
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
            RequestUpdatePackageDetails = UpdatePackageDetails;
            RequestSubmit = Submit;
            RequestPublish = Publish;
            RequestReset = Reset;
            RequestToggleNodeLibraryOnItem = ToggleNodeLibraryOnItem;
            RequestOpenFolder = OpenFolder;
            RequestUpdateCompatibilityMatrix = UpdateCompatibilityMatrix;
            RequestLoadMarkdownContent = LoadMarkdownContent;
            RequestClearMarkdownContent = ClearMarkdownContent;
            RequestLogMessage = LogMessage;
            RequestApplicationLoaded = ApplicationLoaded;
            RequestShowDialog = ShowDialog;

            DataContextChanged += OnDataContextChanged;
        }

        internal void ApplicationLoaded()
        {
            _applicationLoaded = true;

            LoadingDone();
            Logging.Analytics.TrackEvent(Logging.Actions.Load, Logging.Categories.PackageManagerOperations);
        }

        private void LoadingDone()
        {
            CompatibilityMap();
            SetLocale();
            SetDefaultUserNamePreviewValue();

            if (_hasPendingUpdates)
            {
                _hasPendingUpdates = false;
                UpdateFromBackEnd();
            }
        }


        private async void SetLocale()
        {
            var userLocale = CultureInfo.CurrentCulture.Name;

            if (dynWebView?.CoreWebView2 != null)
            {
                await dynWebView.CoreWebView2.ExecuteScriptAsync(@$"window.setLocale('{userLocale}');");
            }
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
            // If we have triggered on data context chagnes with the same context, return
            if (this.DataContext as PublishPackageViewModel == previousViewModel) return;
            if (previousViewModel != null)
            {
                previousViewModel.PropertyChanged -= PublishPackageViewModel_PropertyChanged;
                previousViewModel.PublishSuccess -= PublishPackageViewModel_PublishSuccess;
                previousViewModel.UploadCancelled -= OnUploadCancelled;
            }

            // Cast and assign the new DataContext
            publishPackageViewModel = this.DataContext as PublishPackageViewModel;
            previousViewModel = publishPackageViewModel;

            // If the application hasn't been loaded, and the flag hasn't been flipped, flip it here
            if(!_applicationLoaded && !_hasPendingUpdates && (bool)publishPackageViewModel?.HasChanges)
                _hasPendingUpdates = true;

            // Subscribe to the new ViewModel
            if (publishPackageViewModel != null)
            {
                publishPackageViewModel.PropertyChanged += PublishPackageViewModel_PropertyChanged;
                publishPackageViewModel.PublishSuccess += PublishPackageViewModel_PublishSuccess;
                previousViewModel.UploadCancelled += OnUploadCancelled;

                // Only send updates if the application has been loaded
                if (_applicationLoaded) UpdateFromBackEnd();
            }
        }

        private void UpdateFromBackEnd()
        {
            if (publishPackageViewModel != null)
            {
                if (publishPackageViewModel.HasDependencies) SendPackageDependencies(publishPackageViewModel.DependencyNames);
                if (publishPackageViewModel.HasChanges) SendPackageUpdates(publishPackageViewModel);
                if (publishPackageViewModel.PackageContents?.Count > 0) UpdatePackageContents();
                if (publishPackageViewModel.PreviewPackageContents?.Count > 0) UpdatePreviewPackageContents();
                if (publishPackageViewModel.CompatibilityMatrix?.Count > 0) SendCompatibilityMatrix(publishPackageViewModel.CompatibilityMatrix);
                if (publishPackageViewModel.RetainFolderStructureOverride) UpdateRetainFolderStructureFlag(publishPackageViewModel.RetainFolderStructureOverride);
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
                            RequestRetainFolderStructure,
                            RequestUpdatePackageDetails,
                            RequestSubmit,
                            RequestPublish,
                            RequestReset,
                            RequestToggleNodeLibraryOnItem,
                            RequestOpenFolder,
                            RequestUpdateCompatibilityMatrix,
                            RequestLoadMarkdownContent,
                            RequestClearMarkdownContent,
                            RequestLogMessage,
                            RequestApplicationLoaded,
                            RequestShowDialog));

                }
                catch (Exception ex)
                {
                    LogMessage(ex.Message);
                }
            }
            catch (ObjectDisposedException ex)
            {
                LogMessage(ex.Message);
            }
        }

        internal void NavigateToPage(int number)
        {
            SendNavigateToPage(number);
        }

        internal void ResetProgress()
        {
            SendResetProgress();
        }

        #region ViewModel EventHandlers

        /// <summary>
        /// The main PublishPackageViewModel PropertyChanged reference 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">PropertyName contains the name of the changed property</param>
        private void PublishPackageViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            /* PackageContents */
            if (e.PropertyName.Equals(nameof(publishPackageViewModel.PackageContents)))
            {
                UpdatePackageContents();
            }
            /* PreviewPackageContents */
            else if (e.PropertyName.Equals(nameof(publishPackageViewModel.PreviewPackageContents)))
            {
                UpdatePreviewPackageContents();
            }
            /* PublishDirectory*/
            else if (e.PropertyName.Equals(nameof(publishPackageViewModel.PublishDirectory)))
            {
                SendPublishPathLocation(publishPackageViewModel.PublishDirectory);
            }
            /* MarkdownFilesDirectory*/
            else if (e.PropertyName.Equals(nameof(publishPackageViewModel.MarkdownFilesDirectory)))
            {
                SendMarkdownFilesDirectory(publishPackageViewModel.MarkdownFilesDirectory);
            }
            /* MarkdownFiles*/
            else if (e.PropertyName.Equals(nameof(publishPackageViewModel.MarkdownFiles)))
            {
                SendMarkdownFiles(publishPackageViewModel.MarkdownFiles);
            }
            /* ErrorString*/
            else if (e.PropertyName.Equals(nameof(publishPackageViewModel.ErrorString)))
            {
                // Calling the SendErrorString when the publish routine is running on the background thread is causign a crash 
                if (!Dispatcher.CheckAccess())
                {
                    Dispatcher.InvokeAsync(() =>
                        SendErrorString(publishPackageViewModel.ErrorString)
                    );
                }
                else
                {
                    SendErrorString(publishPackageViewModel.ErrorString);
                }
            }
        }

        private void UpdatePackageContents()
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

        private void UpdatePreviewPackageContents()
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

        /// <summary>
        /// Subscribes to PublishSuccess event and notifies the front end
        /// </summary>
        /// <param name="sender"></param>
        private void PublishPackageViewModel_PublishSuccess(PublishPackageViewModel sender)
        {
            SendPublishSuccess();
        }
        #endregion

        #region Upstream API calls

        private async void SendCompatibilityMap(List<JObject> compatibilityMapList)
        {
            if (compatibilityMapList != null)
            {
                var payload = new { jsonData = compatibilityMapList };

                string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload, Formatting.None);

                if (dynWebView?.CoreWebView2 != null)
                {
                    await dynWebView.CoreWebView2.ExecuteScriptAsync($"window.receiveCompatibilityMap({jsonPayload})");
                }
            }
        }

        private async void SendCompatibilityMatrix(ICollection<Greg.Requests.PackageCompatibility> compatibilityMatrix)
        {
            if (compatibilityMatrix != null)
            {
                var payload = new { jsonData = compatibilityMatrix };

                string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload, Formatting.None);

                if (dynWebView?.CoreWebView2 != null)
                {
                    await dynWebView.CoreWebView2.ExecuteScriptAsync($"window.receiveCompatibilityMatrix({jsonPayload})");
                }
            }
        }

        private async void SendUpdatedPackageContents(object frontendData, string type)
        {
            if (frontendData != null)
            {
                var payload = new { jsonData = frontendData, type = type };

                string jsonPayload = JsonSerializer.Serialize(payload);

                if (dynWebView?.CoreWebView2 != null)
                {
                    await dynWebView.CoreWebView2.ExecuteScriptAsync($"window.receiveUpdatedPackageContents({jsonPayload})");
                }
            }
        }

        private async void SendPublishPathLocation(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            var payload = new { folderPath = path };
            string jsonPayload = JsonSerializer.Serialize(payload);

            if (dynWebView?.CoreWebView2 != null)
            {
                await dynWebView.CoreWebView2.ExecuteScriptAsync($"window.receivePublishPathLocation({jsonPayload});");
            }
        }

        private async void SendMarkdownFiles(IEnumerable<string> markdownFiles)
        {
            if (!markdownFiles.Any()) return;

            var payload = new { markdownFiles = markdownFiles };
            string jsonPayload = JsonSerializer.Serialize(payload);

            if (dynWebView?.CoreWebView2 != null)
            {
                await dynWebView.CoreWebView2.ExecuteScriptAsync($"window.receiveMarkdownFiles({jsonPayload});");
            }
        }

        private async void SendMarkdownFilesDirectory(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            var payload = new { markdownPath = path };
            string jsonPayload = JsonSerializer.Serialize(payload);

            if (dynWebView?.CoreWebView2 != null)
            {
                await dynWebView.CoreWebView2.ExecuteScriptAsync($"window.receiveMarkdownPathLocation({jsonPayload});");
            }
        }

        private async void SendErrorString(string error)
        {
            // We only want to surface actual publish Errors to the front end,
            // and 'Ready to publish' is triggered multiple times during the publishing process
            // preventing us to report an actual Error.
            if (error.Equals(Wpf.Properties.Resources.PackageManagerNoValidationErrors))
                error = string.Empty;

            var payload = new { errorString = error };
            string jsonPayload = JsonSerializer.Serialize(payload);

            if (dynWebView?.CoreWebView2 != null)
            {
                await dynWebView.CoreWebView2.ExecuteScriptAsync($"window.receiveErrorString({jsonPayload});");
            }
        }

        private async void SendPublishSuccess()
        {
            var payload = new { publishSuccess = true };
            string jsonPayload = JsonSerializer.Serialize(payload);

            if (dynWebView?.CoreWebView2 != null)
            {
                await dynWebView.CoreWebView2.ExecuteScriptAsync($"window.receivePublishSuccess({jsonPayload});");
            }
        }

        private async void SendPackageDependencies(string names)
        {
            if (string.IsNullOrEmpty(names)) return;

            var payload = new { dependencyNames = names };
            string jsonPayload = JsonSerializer.Serialize(payload);

            if (dynWebView?.CoreWebView2 != null)
            {
                await dynWebView.CoreWebView2.ExecuteScriptAsync($"window.receiveDependencyNames({jsonPayload});");
            }
        }

        private async void SendPackageUpdates(PublishPackageViewModel vm)
        {
            if (vm == null) return;

            var packageDetails = new PackageUpdateRequest
            {
                Name = vm.Name ?? string.Empty,
                Description = vm.Description ?? string.Empty,
                Major = vm.MajorVersion,
                Minor = vm.MinorVersion,
                Patch = vm.BuildVersion,
                Keywords = vm.Keywords?.Split(',')
                    .Select(k => k.Trim())
                    .Where(k => !string.IsNullOrWhiteSpace(k))
                    .ToList() ?? new List<string>(),
                CopyrightHolder = vm.CopyrightHolder ?? string.Empty,
                CopyrightYear = vm.CopyrightYear ?? string.Empty,
                License = vm.License ?? string.Empty,
                RepositoryUrl = vm.RepositoryUrl ?? string.Empty,
                SiteUrl = vm.SiteUrl ?? string.Empty,
                Group = vm.Group ?? string.Empty,
                ReleaseNotesUrl = vm.ReleaseNotesUrl ?? string.Empty
            };

            var payload = new { payload = packageDetails };
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string jsonPayload = JsonSerializer.Serialize(payload, options);

            if (dynWebView?.CoreWebView2 != null)   
            {
                await dynWebView.CoreWebView2.ExecuteScriptAsync($"window.receiveUpdatedPackageDetails({jsonPayload});");
            }
        }

        private async void UpdateRetainFolderStructureFlag(bool flag)
        {
            var payload = new { flag = flag };
            string jsonPayload = JsonSerializer.Serialize(payload);

            if (dynWebView?.CoreWebView2 != null)
            {
                await dynWebView.CoreWebView2.ExecuteScriptAsync($"window.receiveRetainFolderStructure({jsonPayload});");
            }
        }

        private async void SendNavigateToPage(int number)
        {
            var payload = new { pageNumber = number };
            string jsonPayload = JsonSerializer.Serialize(payload);

            if (dynWebView?.CoreWebView2 != null)
            {
                await dynWebView.CoreWebView2.ExecuteScriptAsync($"window.receiveNavigateToPage({jsonPayload});");
            }
        }

        private async void SendResetProgress()
        {
            if (dynWebView?.CoreWebView2 != null)
            {
                await dynWebView.CoreWebView2.ExecuteScriptAsync($"window.receiveResetProgress();");
            }
        }

        private async void SendUploadCancel()
        {
            if (dynWebView?.CoreWebView2 != null)
            {
                await dynWebView.CoreWebView2.ExecuteScriptAsync($"window.receiveUploadCancel();");
            }
        }

        private async void SendDialogResult(string msg)
        {
            var payload = new { result = msg };
            string jsonPayload = JsonSerializer.Serialize(payload);

            if (dynWebView?.CoreWebView2 != null)
            {
                await dynWebView.CoreWebView2.ExecuteScriptAsync($"window.receiveDialogResult({jsonPayload});");
            }
        }

        private async void SetDefaultUserNamePreviewValue()
        {
            if (!string.IsNullOrEmpty(publishPackageViewModel?.DynamoViewModel?.Model?.AuthenticationManager?.Username))
            {
                var payload = new { userName = publishPackageViewModel.DynamoViewModel.Model.AuthenticationManager.Username };

                string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload, Formatting.None);

                if (dynWebView?.CoreWebView2 != null)
                {
                    await dynWebView.CoreWebView2.ExecuteScriptAsync($"window.receiveDefaultUserName({jsonPayload})");
                }
            }
        }

        #endregion

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

        internal void RemoveFileOrFolder(string filePath)
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

        internal void ToggleNodeLibraryOnItem(string filePath)
        {
            if (publishPackageViewModel == null) return;

            var itemToToggle = FindItemByFilePath(publishPackageViewModel.PackageContents.ToList(), filePath);

            if (itemToToggle != null)
            {
                itemToToggle.IsNodeLibrary = !itemToToggle.IsNodeLibrary;
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

        internal void ToggleRetainFolderStructure(bool flag)
        {
            if (publishPackageViewModel == null) return;

            publishPackageViewModel.RetainFolderStructureOverride = flag;
        }

        internal void UpdatePackageDetails(string jsonPayload)
        {
            if (string.IsNullOrWhiteSpace(jsonPayload) || publishPackageViewModel == null)
                return;

            try
            {
                var packageDetails = JsonSerializer.Deserialize<PackageUpdateRequest>(
                    jsonPayload,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                if (packageDetails == null) return;

                // Check if the new details are the same as the previous ones
                if (previousPackageDetails != null && previousPackageDetails.Equals(packageDetails))
                    return;

                // Store the new details to prevent redundant updates
                previousPackageDetails = packageDetails;

                // Assign required fields
                publishPackageViewModel.Name = packageDetails.Name;
                publishPackageViewModel.Description = packageDetails.Description;

                // Assign version numbers separately
                publishPackageViewModel.MajorVersion = packageDetails.Major;
                publishPackageViewModel.MinorVersion = packageDetails.Minor;
                publishPackageViewModel.BuildVersion = packageDetails.Patch;

                // Join keywords into a comma-separated string
                publishPackageViewModel.Keywords = packageDetails.Keywords?.Count > 0
                    ? string.Join(", ", packageDetails.Keywords)
                    : string.Empty;

                // Assign optional fields
                publishPackageViewModel.CopyrightHolder = packageDetails.CopyrightHolder ?? string.Empty;
                publishPackageViewModel.CopyrightYear = packageDetails.CopyrightYear ?? string.Empty;
                publishPackageViewModel.License = packageDetails.License ?? string.Empty;
                publishPackageViewModel.RepositoryUrl = packageDetails.RepositoryUrl ?? string.Empty;
                publishPackageViewModel.SiteUrl = packageDetails.SiteUrl ?? string.Empty;
                publishPackageViewModel.Group = packageDetails.Group ?? string.Empty;
                publishPackageViewModel.ReleaseNotesUrl = packageDetails.ReleaseNotesUrl ?? string.Empty;

                LogMessage("Package details updated successfully.");
            }
            catch (Exception ex)
            {
                LogMessage(ex);
            }
        }

        internal void UpdateCompatibilityMatrix(string jsonPayload)
        {
            if (string.IsNullOrWhiteSpace(jsonPayload) || publishPackageViewModel == null)
                return;

            try
            {
                var compatibilityMatrix = JsonSerializer.Deserialize<ICollection<PackageCompatibility>>(
                    jsonPayload,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                if (compatibilityMatrix == null) return;

                // Replace any 'x' in the max field with '*'
                foreach (var item in compatibilityMatrix)
                {
                    if (!string.IsNullOrWhiteSpace(item.max))
                    {
                        item.max = item.max.Replace("x", "*");
                    }
                }

                publishPackageViewModel.CompatibilityMatrix = compatibilityMatrix;

                LogMessage("Compatibility matrix updated successfully.");
            }
            catch (Exception ex)
            {
                LogMessage(ex);
            }
        }

        internal void LoadMarkdownContent()
        {
            if (publishPackageViewModel == null)
                return;

            if (publishPackageViewModel.SelectMarkdownDirectoryCommand?.CanExecute() == true)
            {
                publishPackageViewModel.SelectMarkdownDirectoryCommand.Execute();
            }
        }

        internal void ClearMarkdownContent()
        {
            if (publishPackageViewModel == null)
                return;

            if (publishPackageViewModel.ClearMarkdownDirectoryCommand?.CanExecute() == true)
            {
                publishPackageViewModel.ClearMarkdownDirectoryCommand.Execute();
            }
        }

        internal void CompatibilityMap()
        {
            var compatibilityMapList = PackageManagerClient.CompatibilityMapList(); // Fetch full compatibility map

            SendCompatibilityMap(compatibilityMapList);
        }

        internal void Submit()
        {
            PrePopulateDefaultValues();

            if (publishPackageViewModel == null) return;
            if (publishPackageViewModel.SubmitCommand?.CanExecute() == true)
            {
                publishPackageViewModel.SubmitCommand.Execute();
            }
        }

        internal void Publish()
        {
            PrePopulateDefaultValues();

            if (publishPackageViewModel == null) return;
            if (publishPackageViewModel.PublishLocallyCommand?.CanExecute() == true)
            {
                publishPackageViewModel.PublishLocallyCommand.Execute();
            }
        }

        internal void Reset()
        {
            if (publishPackageViewModel == null) return;
            if (publishPackageViewModel.CancelCommand?.CanExecute() == true)
            {
                publishPackageViewModel.CancelCommand.Execute();
            }
        }

        internal void OpenFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                System.Diagnostics.Process.Start("explorer.exe", folderPath);
            }
        }


        /// <summary>
        /// A request from the front-end to use a modal dialog
        /// </summary>
        /// <param name="title">The title of the dialog</param>
        /// <param name="message">The message to be displayed</param>
        /// <exception cref="NotImplementedException"></exception>
        internal void ShowDialog(string title, string message)
        {
            var ownerWindow = Window.GetWindow(this);

            MessageBoxResult response = DynamoModel.IsTestMode ? MessageBoxResult.OK :
                    MessageBoxService.Show(
                    ownerWindow,
                    message,
                    title,
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Warning);

            if (response == MessageBoxResult.OK)
            {
                SendDialogResult("Yes");
            }
            else
            {
                SendDialogResult("Cancel");
            }
        }

        /// <summary>
        ///  Notify the front-end that the upload was cancelled
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void OnUploadCancelled()
        {
           SendUploadCancel();
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

        /// <summary>
        /// Process web page opening 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Error handling and loging
        /// Overloaded method for messages
        /// </summary>
        /// <param name="message">Message to log</param>
        private void LogMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            string logMessage = $"Package Manager Wizard INFO: {message}";
            this.publishPackageViewModel?.DynamoViewModel?.Model.Logger.Log(logMessage);
        }

        /// <summary>
        /// Error handling and loging
        /// Overloaded method for exceptions
        /// </summary>
        /// <param name="ex">Exception to log</param>
        private void LogMessage(Exception ex)
        {
            if (ex == null) return;

            string logMessage = $"Package Manager Wizard ERROR: {ex.GetType()}: {ex.Message}\nStack Trace:\n{ex.StackTrace}";
            this.publishPackageViewModel?.DynamoViewModel?.Model.Logger.Log(logMessage);
        }

        /// <summary>
        /// Pre-populate default values for the package details
        /// </summary>
        private void PrePopulateDefaultValues()
        {
            if (publishPackageViewModel == null) return;
            if (string.IsNullOrEmpty(publishPackageViewModel.CopyrightYear))
            {
                publishPackageViewModel.CopyrightYear = DateTime.Now.Year.ToString();
            }
            if (string.IsNullOrEmpty(publishPackageViewModel.License))
            {
                publishPackageViewModel.License = "MIT";
            }
            if (string.IsNullOrEmpty(publishPackageViewModel.CopyrightHolder))
            {
                publishPackageViewModel.CopyrightHolder = publishPackageViewModel.DynamoViewModel.Model.AuthenticationManager.Username;
            }
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
                    this.DataContextChanged -= OnDataContextChanged;

                    if (this.publishPackageViewModel != null)
                    {
                        this.publishPackageViewModel.PropertyChanged -= PublishPackageViewModel_PropertyChanged;
                        this.publishPackageViewModel.PublishSuccess -= PublishPackageViewModel_PublishSuccess;
                        this.previousViewModel.UploadCancelled -= OnUploadCancelled;
                    }

                    if (this.dynWebView != null && this.dynWebView.CoreWebView2 != null)
                    {
                        this.dynWebView.CoreWebView2.NewWindowRequested -= CoreWebView2_NewWindowRequested;
                    }
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
        readonly Action<string> RequestUpdatePackageDetails;
        readonly Action RequestSubmit;
        readonly Action RequestPublish;
        readonly Action RequestReset;
        readonly Action<string> RequestToggleNodeLibraryOnItem;
        readonly Action<string> RequestOpenFolder;
        readonly Action<string> RequestUpdateCompatibilityMatrix;
        readonly Action RequestLoadMarkdownContent;
        readonly Action RequestClearMarkdownContent;
        readonly Action<string> RequestLogMessage;
        readonly Action RequestApplicationLoaded;
        readonly Action<string, string> RequestShowDialog;

        public ScriptWizardObject(
            Action<string> requestAddFileOrFolder,
            Action<string> requestRemoveFileOrFolder,
            Action<bool> requestRetainFolderStructure,
            Action<string> requestUpdatePackageDetails,
            Action requestSubmit,
            Action requestPublish,
            Action requestReset,
            Action<string> requestToggleNodeLibraryOnItem,
            Action<string> requestOpenFolder,
            Action<string> requestUpdateCompatibilityMatrix,
            Action requestLoadMarkdownContent,
            Action requestClearMarkdownContent,
            Action<string> requestLogMessage,
            Action requestApplicationLoaded,
            Action<string, string> requestShowDialog)
        {
            RequestAddFileOrFolder = requestAddFileOrFolder;
            RequestRemoveFileOrFolder = requestRemoveFileOrFolder;
            RequestRetainFolderStructure = requestRetainFolderStructure;
            RequestUpdatePackageDetails = requestUpdatePackageDetails;
            RequestSubmit = requestSubmit;
            RequestPublish = requestPublish;
            RequestReset = requestReset;
            RequestToggleNodeLibraryOnItem = requestToggleNodeLibraryOnItem;
            RequestOpenFolder = requestOpenFolder;
            RequestUpdateCompatibilityMatrix = requestUpdateCompatibilityMatrix;
            RequestLoadMarkdownContent = requestLoadMarkdownContent;
            RequestClearMarkdownContent = requestClearMarkdownContent;
            RequestLogMessage = requestLogMessage;
            RequestApplicationLoaded = requestApplicationLoaded;
            RequestShowDialog = requestShowDialog;
        }

        [DynamoJSInvokable]
        public void ApplicationLoaded()
        {
            RequestApplicationLoaded();
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

        [DynamoJSInvokable]
        public void UpdatePackageDetails(string jsonPayload)
        {
            RequestUpdatePackageDetails(jsonPayload);
        }

        [DynamoJSInvokable]
        public void UpdateCompatibilityMatrix(string jsonPayload)
        {
            RequestUpdateCompatibilityMatrix(jsonPayload);
        }

        [DynamoJSInvokable]
        public void LoadMarkdownContent()
        {
            RequestLoadMarkdownContent();
        }

        [DynamoJSInvokable]
        public void ClearMarkdownContent()
        {
            RequestClearMarkdownContent();
        }

        [DynamoJSInvokable]
        public void Submit()
        {
            RequestSubmit();
        }

        [DynamoJSInvokable]
        public void Publish()
        {
            RequestPublish();
        }

        [DynamoJSInvokable]
        public void Reset()
        {
            RequestReset();
        }

        [DynamoJSInvokable]
        public void ToggleNodeLibraryOnItem(string name)
        {
            RequestToggleNodeLibraryOnItem(name);
        }

        [DynamoJSInvokable]
        public void OpenFolder(string path)
        {
            RequestOpenFolder(path);
        }

        [DynamoJSInvokable]
        public void LogMessage(string message)
        {
            RequestLogMessage(message);
        }

        [DynamoJSInvokable]
        public void ShowDialog(string title, string message)
        {
            RequestShowDialog(title, message);
        }
    }

    public class PackageUpdateRequest : IEquatable<PackageUpdateRequest>
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("major")]
        public string Major { get; set; }

        [JsonPropertyName("minor")]
        public string Minor { get; set; }

        [JsonPropertyName("patch")]
        public string Patch { get; set; }

        [JsonPropertyName("keywords")]
        public List<string> Keywords { get; set; } = new();

        [JsonPropertyName("release_notes_url")]
        public string ReleaseNotesUrl { get; set; }

        [JsonPropertyName("copyright_holder")]
        public string CopyrightHolder { get; set; }

        [JsonPropertyName("copyright_year")]
        public string CopyrightYear { get; set; }

        [JsonPropertyName("license")]
        public string License { get; set; }

        [JsonPropertyName("repository_url")]
        public string RepositoryUrl { get; set; }

        [JsonPropertyName("site_url")]
        public string SiteUrl { get; set; }

        [JsonPropertyName("group")]
        public string Group { get; set; }

        public bool Equals(PackageUpdateRequest other)
        {
            if (other == null) return false;

            return Name == other.Name &&
                   Description == other.Description &&
                   Major == other.Major &&
                   Minor == other.Minor &&
                   Patch == other.Patch &&
                   (Keywords == null && other.Keywords == null || (Keywords?.SequenceEqual(other.Keywords) ?? false)) &&
                   ReleaseNotesUrl == other.ReleaseNotesUrl &&
                   CopyrightHolder == other.CopyrightHolder &&
                   CopyrightYear == other.CopyrightYear &&
                   License == other.License &&
                   RepositoryUrl == other.RepositoryUrl &&
                   SiteUrl == other.SiteUrl &&
                   Group == other.Group;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PackageUpdateRequest);
        }

        /// <summary>
        /// Overriding GetHashCode allows us to get a deep equality
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked // Allow integer overflow (safe for hash calculations)
            {
                int hash = 17;
                hash = hash * 31 + (Name?.GetHashCode() ?? 0);
                hash = hash * 31 + (Description?.GetHashCode() ?? 0);
                hash = hash * 31 + (Major?.GetHashCode() ?? 0);
                hash = hash * 31 + (Minor?.GetHashCode() ?? 0);
                hash = hash * 31 + (Patch?.GetHashCode() ?? 0);
                hash = hash * 31 + (ReleaseNotesUrl?.GetHashCode() ?? 0);
                hash = hash * 31 + (CopyrightHolder?.GetHashCode() ?? 0);
                hash = hash * 31 + (CopyrightYear?.GetHashCode() ?? 0);
                hash = hash * 31 + (License?.GetHashCode() ?? 0);
                hash = hash * 31 + (RepositoryUrl?.GetHashCode() ?? 0);
                hash = hash * 31 + (SiteUrl?.GetHashCode() ?? 0);
                hash = hash * 31 + (Group?.GetHashCode() ?? 0);

                // Handle list separately
                if (Keywords != null)
                {
                    foreach (var keyword in Keywords)
                    {
                        hash = hash * 31 + (keyword?.GetHashCode() ?? 0);
                    }
                }

                return hash;
            }
        }

    }

}
