using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dynamo.Configuration;
using Dynamo.Logging;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Properties;
using Newtonsoft.Json;
using NotificationObject = Dynamo.Core.NotificationObject;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// This class represents the unified data class that is bound to all the
    /// list boxes on the StartPageView. The bound data item can be handled in
    /// different ways depending on their ClickAction and ContextData properties.
    /// See "Action" enumeration below for more details of each item sub-type.
    /// </summary>
    ///
    public class StartPageListItem : NotificationObject
    {
        private ImageSource icon = null;

        public enum Action
        {
            /// <summary>
            /// Indicates a regular command should be invoked if the list view
            /// item corresponding to this StartPageListItem is clicked. The
            /// meaning of ContextData will be interpreted in StartPageViewModel
            /// and corresponding action taken as a result.
            /// </summary>
            RegularCommand,

            /// <summary>
            /// Indicates that the StartPageListItem carries a file path. When
            /// clicked, StartPageViewModel issues a file open command to open
            /// the file path indicated by ContextData property.
            /// </summary>
            FilePath,

            /// <summary>
            /// Indicates that the StartPageListItem points to an external URL.
            /// When the list view item corresponding to this StartPageListItem
            /// is clicked, StartPageViewModel brings up the default browser and
            /// navigate to the URL indicated by ContextData property.
            /// </summary>
            ExternalUrl
        }

        protected internal StartPageListItem(string caption)
        {
            this.Caption = caption;
        }

        protected internal StartPageListItem(string caption, string iconPath)
        {
            this.Caption = caption;
            this.icon = LoadBitmapImage(iconPath);
        }

        #region Public Class Properties

        public string Caption { get; private set; }
        public string SubScript { get; set; }
        public string ToolTip { get; set; }
        public string DateModified { get; set; }
        public string Description { get; internal set; }
        public string Thumbnail { get; set; }
        public string Author { get; internal set; }
        public string ContextData { get; set; }
        public Action ClickAction { get; set; }

        public ImageSource Icon
        {
            get { return this.icon; }
        }

        // Extended (derived) class properties.
        public Visibility IconVisibility
        {
            get { return ((icon == null) ? Visibility.Collapsed : Visibility.Visible); }
        }

        #endregion

        #region Private Class Helper Methods

        protected BitmapImage LoadBitmapImage(string iconPath)
        {
            var format = @"pack://application:,,,/DynamoCoreWpf;component/UI/Images/StartPage/{0}";
            iconPath = string.Format(format, iconPath);
            return new BitmapImage(new Uri(iconPath, UriKind.Absolute));
        }

        #endregion
    }

    public class StartPageViewModel : ViewModelBase
    {
        // Static lists that gets created only during creation.
        List<StartPageListItem> fileOperations = new List<StartPageListItem>();
        List<StartPageListItem> communityLinks = new List<StartPageListItem>();
        List<StartPageListItem> references = new List<StartPageListItem>();
        List<StartPageListItem> contributeLinks = new List<StartPageListItem>();
        string sampleFolderPath = null;
        string sampleDatasetsPath = null;

        // Dynamic lists that update views on the fly.
        ObservableCollection<SampleFileEntry> sampleFiles = null;
        ObservableCollection<StartPageListItem> recentFiles = null;
        ObservableCollection<StartPageListItem> backupFiles = null;
        internal readonly DynamoViewModel DynamoViewModel;
        private readonly bool isFirstRun;

        internal StartPageViewModel(DynamoViewModel dynamoViewModel, bool isFirstRun)
        {
            this.DynamoViewModel = dynamoViewModel;
            this.isFirstRun = isFirstRun;

            this.recentFiles = new ObservableCollection<StartPageListItem>();
            this.sampleFiles = new ObservableCollection<SampleFileEntry>();
            this.backupFiles = new ObservableCollection<StartPageListItem>();


            #region File Operations

            fileOperations.Add(new StartPageListItem(Resources.StartPageNewFile, "icon-new.png")
            {
                ContextData = ButtonNames.NewWorkspace,
                ClickAction = StartPageListItem.Action.RegularCommand
            });

            fileOperations.Add(new StartPageListItem(Resources.StartPageNewCustomNode, "icon-customnode.png")
            {
                ContextData = ButtonNames.NewCustomNodeWorkspace,
                ClickAction = StartPageListItem.Action.RegularCommand
            });

            fileOperations.Add(new StartPageListItem(Resources.StartPageOpenFile, "icon-open.png")
            {
                ContextData = ButtonNames.OpenWorkspace,
                ClickAction = StartPageListItem.Action.RegularCommand
            });

            #endregion

            #region Community Links

            communityLinks.Add(new StartPageListItem(Resources.StartPageDiscussionForum, "icon-discussion.png")
            {
                ContextData = Configurations.DynamoBimForum,
                ClickAction = StartPageListItem.Action.ExternalUrl
            });

            communityLinks.Add(new StartPageListItem(
                string.Format(Resources.StartPageVisitWebsite, dynamoViewModel.BrandingResourceProvider.ProductName),
                "icon-dynamobim.png")
            {
                ContextData = Configurations.DynamoSiteLink,
                ClickAction = StartPageListItem.Action.ExternalUrl
            });

            #endregion

            #region Reference List

            references.Add(new StartPageListItem(Resources.StartPageDynamoPrimer, "icon-reference.png")
            {
                ContextData = Configurations.DynamoPrimer,
                ClickAction = StartPageListItem.Action.ExternalUrl
            });

            references.Add(new StartPageListItem(Resources.StartPageVideoTutorials, "icon-video.png")
            {
                ContextData = Configurations.DynamoVideoTutorials,
                ClickAction = StartPageListItem.Action.ExternalUrl
            });

            references.Add(new StartPageListItem(Resources.StartPageDynamoDictionary, "icon-dictionary.png")
            {
                ContextData = Configurations.DynamoDictionary,
                ClickAction = StartPageListItem.Action.ExternalUrl
            });

            #endregion

            #region Contribution Links

            contributeLinks.Add(new StartPageListItem(Resources.StartPageGithubRepository, "icon-github.png")
            {
                ContextData = Configurations.GitHubDynamoLink,
                ClickAction = StartPageListItem.Action.ExternalUrl
            });

            contributeLinks.Add(new StartPageListItem(Resources.StartPageSendIssues, "icon-issues.png")
            {
                ContextData = Configurations.GitHubBugReportingLink + "?template=issue.yml",
                ClickAction = StartPageListItem.Action.ExternalUrl
            });

            #endregion

            var dvm = this.DynamoViewModel;
            RefreshRecentFileList(dvm.RecentFiles);
            RefreshBackupFileList(dvm.Model.PreferenceSettings.BackupFiles);
            dvm.RecentFiles.CollectionChanged += OnRecentFilesChanged;
        }

        internal void WalkDirectoryTree(System.IO.DirectoryInfo root, SampleFileEntry rootProperty)
        {
            try
            {
                // First try to get all the sub-directories before the files themselves.
                System.IO.DirectoryInfo[] directories = root.GetDirectories();
                if (null != directories && (directories.Length > 0))
                {
                    foreach (System.IO.DirectoryInfo directory in directories)
                    {
                        //Make sure the folder's name is not "backup"
                        if (!directory.Name.Equals(Configurations.BackupFolderName))
                        {
                            // Recursive call for each subdirectory.
                            SampleFileEntry sampleFileEntry =
                                new SampleFileEntry(directory.Name, directory.FullName);
                            WalkDirectoryTree(directory, sampleFileEntry);
                            rootProperty.AddChildSampleFile(sampleFileEntry);
                        }
                    }
                }

                // Secondly, process all the files directly under this folder
                System.IO.FileInfo[] dynamoFiles = null;
                dynamoFiles = root.GetFiles("*.dyn", System.IO.SearchOption.TopDirectoryOnly);

                if (null != dynamoFiles && (dynamoFiles.Length > 0))
                {
                    foreach (System.IO.FileInfo file in dynamoFiles)
                    {
                        if (sampleFolderPath == null)
                        {
                            sampleFolderPath = Path.GetDirectoryName(file.FullName);
                            SetSampleDatasetsPath();
                        }

                        // Add each file under the root directory property list.
                        var properties = GetFileProperties(file.FullName);

                        rootProperty.AddChildSampleFile(new SampleFileEntry(
                            file.Name,
                            file.FullName,
                            properties.thumbnail,
                            properties.author,
                            properties.description,
                            properties.date));
                    }
                }
            }
            catch (Exception ex)
            {
                // Perhaps some permission problems?
                DynamoViewModel.Model.Logger.Log("Error loading sample file: " + ex.StackTrace);
            }
        }

        /// <summary>
        /// Sets the sampleDatasetsPath based on the value of sampleFolderPath
        /// </summary>
        private void SetSampleDatasetsPath()
        {
            try
            {
                var directoryInfo = new DirectoryInfo(sampleFolderPath);

                // Traverse the directory tree upwards to locate the "samples" folder
                while (directoryInfo != null && directoryInfo.Name != "samples")
                {
                    directoryInfo = directoryInfo.Parent;
                }

                if (directoryInfo != null && directoryInfo.Name == "samples")
                {
                    var datasetsPath = Path.Combine(directoryInfo.FullName, "Data");

                    if (Directory.Exists(datasetsPath))
                    {
                        sampleDatasetsPath = datasetsPath;
                    }
                    else
                    {
                        DynamoViewModel.Model.Logger.Log("Error, Dataset folder not found.");
                    }
                }
            }
            catch (Exception ex)
            {
                DynamoViewModel.Model.Logger.Log("Error loading Dataset folder: " + ex.Message);
            }
        }

        internal void HandleListItemClicked(StartPageListItem clicked)
        {
            if (clicked != null)
            {
                switch (clicked.ClickAction)
                {
                    case StartPageListItem.Action.RegularCommand:
                        HandleRegularCommand(clicked);
                        break;

                    case StartPageListItem.Action.FilePath:
                        HandleFilePath(clicked);
                        break;

                    case StartPageListItem.Action.ExternalUrl:
                        HandleExternalUrl(clicked);
                        break;
                }
            }
        }

        public bool IsFirstRun { get { return isFirstRun; } }

        public string SampleFolderPath
        {
            get { return this.sampleFolderPath; }
        }

        public string SampleDatasetsPath
        {
            get { return this.sampleDatasetsPath; }
        }

        #region Public Class Properties (Static Lists)

        public IEnumerable<StartPageListItem> FileOperations
        {
            get { return this.fileOperations; }
        }

        public IEnumerable<StartPageListItem> CommunityLinks
        {
            get { return this.communityLinks; }
        }

        public IEnumerable<StartPageListItem> References
        {
            get { return this.references; }
        }

        public IEnumerable<StartPageListItem> ContributeLinks
        {
            get { return this.contributeLinks; }
        }

        #endregion

        #region Public Class Properties (Dynamic Lists)


        public ObservableCollection<StartPageListItem> RecentFiles
        {
            get { return this.recentFiles; }
        }

        public ObservableCollection<StartPageListItem> BackupFiles
        {
            get { return this.backupFiles; }
        }

        #endregion

        public ObservableCollection<SampleFileEntry> SampleFiles
        {
            get { return this.sampleFiles; }
        }

        public string BackupTitle
        {
            get
            {
                if (StabilityUtils.IsLastShutdownClean
                    || DynamoViewModel.Model.PreferenceSettings.BackupFiles.Count == 0)
                {
                    return Dynamo.Wpf.Properties.Resources.StartPageBackupNoCrash;
                }
                else
                {
                    return Dynamo.Wpf.Properties.Resources.StartPageBackupOnCrash;
                }
            }
        }

        #region Private Class Event Handlers

        private void OnRecentFilesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshRecentFileList(sender as IEnumerable<string>);
        }

        #endregion

        #region Private Class Helper Methods

        private void RefreshRecentFileList(IEnumerable<string> filePaths)
        {
            RefreshFileList(recentFiles, filePaths);
        }

        private void RefreshBackupFileList(IEnumerable<string> filePaths)
        {
            RefreshFileList(backupFiles, filePaths);
        }

        private void RefreshFileList(ObservableCollection<StartPageListItem> files,
            IEnumerable<string> filePaths)
        {
            files.Clear();
            foreach (var filePath in filePaths.Where(x => x != null))
            {
                try
                {
                    // Skip files which were moved or deleted (consistent with Revit behavior)
                    if (!DynamoUtilities.PathHelper.IsValidPath(filePath)) continue;

                    var extension = Path.GetExtension(filePath).ToUpper();
                    // If not extension specified and code reach here, this means this is still a valid file
                    // only without file type. Otherwise, simply take extension substring skipping the 'dot'.
                    var subScript = extension.StartsWith(".") ? extension.Substring(1) : "";
                    var caption = Path.GetFileNameWithoutExtension(filePath);

                    // deserializes the file only once
                    var properties = GetFileProperties(filePath);

                    files.Add(new StartPageListItem(caption)
                    {
                        ContextData = filePath,
                        ToolTip = filePath,
                        SubScript = subScript,
                        Description = properties.description,
                        Thumbnail = properties.thumbnail,
                        Author = properties.author,
                        DateModified = properties.date,
                        ClickAction = StartPageListItem.Action.FilePath,

                    }); 
                }
                catch (ArgumentException ex)
                {
                    DynamoViewModel.Model.Logger.Log("File path is not valid: " + ex.StackTrace);
                }
                catch (Exception ex)
                {
                    DynamoViewModel.Model.Logger.Log("Error loading the file: " + ex.StackTrace);
                }
            }
        }

        private Dictionary<string, object> DeserializeJsonFile(string filePath)
        {
            if (DynamoUtilities.PathHelper.isValidJson(filePath, out string jsonString, out Exception ex))
            {
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            }
            else
            {
                if(ex is JsonReaderException)
                {
                    DynamoViewModel.Model.Logger.Log("File is not a valid json format.");
                }
                else
                {
                    DynamoViewModel.Model.Logger.Log("File is not valid: " + ex.StackTrace);
                }
                return null;
            }
        }

        private const string BASE64PREFIX = "data:image/png;base64,";

        private string GetGraphThumbnail(Dictionary<string, object> jsonObject)
        {
            jsonObject.TryGetValue("Thumbnail", out object thumbnail);

            if (string.IsNullOrEmpty(thumbnail as string)) return string.Empty;

            var base64 = String.Format("{0}{1}", BASE64PREFIX, thumbnail as string);

            return base64;
        }

        private string GetGraphDescription(Dictionary<string, object> jsonObject)
        {
            jsonObject.TryGetValue("Description", out object description);

            return description as string;
        }

        private string GetGraphAuthor(Dictionary<string, object> jsonObject)
        {
            jsonObject.TryGetValue("Author", out object author);

            return author as string;
        }

        private void HandleRegularCommand(StartPageListItem item)
        {
            var dvm = this.DynamoViewModel;

            switch (item.ContextData)
            {
                case ButtonNames.NewWorkspace:
                    dvm.NewHomeWorkspaceCommand.Execute(null);
                    break;

                case ButtonNames.OpenWorkspace:
                    dvm.ShowOpenDialogAndOpenResultCommand.Execute(null);
                    break;

                case ButtonNames.NewCustomNodeWorkspace:
                    dvm.ShowNewFunctionDialogCommand.Execute(null);
                    break;

                default:
                    throw new ArgumentException(
                        string.Format("Invalid command: {0}", item.ContextData));
            }
        }

        private void HandleFilePath(StartPageListItem item)
        {
            var path = item.ContextData;
            this.DynamoViewModel.OpenCommand.Execute(path);
        }

        private void HandleExternalUrl(StartPageListItem item)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo(item.ContextData) { UseShellExecute = true });
        }

        /// <summary>
        /// Attempts to deserialize a dynamo graph file and extract metadata from it
        /// </summary>
        /// <param name="filePath">The file path to the dynamo file</param>
        /// <returns></returns>
        internal (string description, string thumbnail, string author, string date) GetFileProperties(string filePath)
        {
            if (!filePath.ToLower().EndsWith(".dyn") && !filePath.ToLower().EndsWith(".dyf")) return (null, null, null, null);

            try
            {
                var jsonObject = DeserializeJsonFile(filePath);
                var description = jsonObject != null ? GetGraphDescription(jsonObject) : string.Empty;
                var thumbnail = jsonObject != null ? GetGraphThumbnail(jsonObject) : string.Empty;
                var author = jsonObject != null ? GetGraphAuthor(jsonObject) : Resources.DynamoXmlFileFormat;
                var date = DynamoUtilities.PathHelper.GetDateModified(filePath);

                return (description, thumbnail, author, date);
            }
            catch (Exception ex)
            {
                DynamoViewModel.Model.Logger.Log("Error deserializing dynamo graph file: " + ex.StackTrace);
                return (null, null, null, null);
            }
        }

        #endregion

    }

    struct ButtonNames
    {
        public const string NewWorkspace = "NewWorkspace";
        public const string NewCustomNodeWorkspace = "NewCustomNodeWorkspace";
        public const string OpenWorkspace = "OpenWorkspace";
    }

    public partial class StartPageView : UserControl
    {
        private DynamoViewModel dynamoViewModel;

        /// <summary>
        /// Constructor
        /// </summary>
        public StartPageView()
        {
            InitializeComponent();
            if (StabilityUtils.IsLastShutdownClean)
            {
                backupFilesList.Visibility = Visibility.Collapsed;
            }

            this.Loaded += OnStartPageLoaded;
        }

        #region Private Class Event Handlers

        private void OnStartPageLoaded(object sender, RoutedEventArgs e)
        {
            var startPageViewModel = this.DataContext as StartPageViewModel;
            this.dynamoViewModel = startPageViewModel.DynamoViewModel;

            this.filesListBox.ItemsSource = startPageViewModel.FileOperations;
            this.askListBox.ItemsSource = startPageViewModel.CommunityLinks;
            this.referenceListBox.ItemsSource = startPageViewModel.References;
            this.codeListBox.ItemsSource = startPageViewModel.ContributeLinks;
            this.recentListBox.ItemsSource = startPageViewModel.RecentFiles;
            this.sampleFileTreeView.ItemsSource = startPageViewModel.SampleFiles;
            this.backupFilesList.ItemsSource = startPageViewModel.BackupFiles;

            var id = Wpf.Interfaces.ResourceNames.StartPage.Image;
            StartPageLogo.Source = dynamoViewModel.BrandingResourceProvider.GetImageSource(id);
        }

        private void OnItemSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e == null || (e.AddedItems == null) || (e.AddedItems.Count <= 0))
                return;

            var selected = e.AddedItems[0] as StartPageListItem;
            var startPageViewModel = this.DataContext as StartPageViewModel;
            startPageViewModel.HandleListItemClicked(selected);

            // Clear list box selection so that the same item, when
            // clicked, still triggers "selection changed" notification.
            var listBox = sender as ListBox;
            listBox.SelectedIndex = -1;
        }

        private void OnSampleFileSelected(object sender, RoutedEventArgs e)
        {
            var dp = e.OriginalSource as DependencyObject;
            var treeViewItem = WpfUtilities.FindUpVisualTree<TreeViewItem>(dp) as TreeViewItem;
            if (sampleFileTreeView.SelectedItem != null)
                treeViewItem.IsExpanded = !treeViewItem.IsExpanded;

            var filePath = (sampleFileTreeView.SelectedItem as SampleFileEntry).FilePath;

            if (string.IsNullOrEmpty(filePath))
                return;

            if (!Path.GetExtension(filePath).Equals(".dyn"))
                return;

            var dvm = this.dynamoViewModel;
            if (dvm.OpenCommand.CanExecute(filePath))
                dvm.OpenCommand.Execute(filePath);
        }

        private void ShowSamplesInFolder(object sender, MouseButtonEventArgs e)
        {
            var startPageViewModel = this.DataContext as StartPageViewModel;
            Process.Start(new ProcessStartInfo("explorer.exe", "/select,"
                + startPageViewModel.SampleFolderPath)
                { UseShellExecute = true });
        }

        private void ShowBackupFilesInFolder(object sender, MouseButtonEventArgs e)
        {
            var startPageViewModel = this.DataContext as StartPageViewModel;
            Process.Start(new ProcessStartInfo("explorer.exe", dynamoViewModel.Model.PathManager.BackupDirectory) { UseShellExecute = true });
        }

        private void StartPage_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                var homespace = dynamoViewModel.HomeSpace;
                if (homespace.HasUnsavedChanges &&
                    !dynamoViewModel.AskUserToSaveWorkspaceOrCancel(homespace))
                {
                    return;
                }

                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null && (files.Length > 0))
                {
                    if (dynamoViewModel.OpenCommand.CanExecute(files[0]))
                    {
                        dynamoViewModel.OpenCommand.Execute(files[0]);
                        e.Handled = true;
                    }
                }

            }
        }
        #endregion
    }

    public class SampleFileEntry : StartPageListItem
    {
        List<SampleFileEntry> childSampleFiles = null;

        public SampleFileEntry(string name, string path)
            : base(name)
        {
            this.FileName = name;
            this.FilePath = path;
        }

        public SampleFileEntry(string name, string path, string thumbnail, string author, string description, string dateModified)
            : base(name)
        {
            this.FileName = name;
            this.FilePath = path;
            this.Thumbnail = thumbnail;
            this.Author = author;
            this.Description = description;
            this.DateModified = dateModified;
        }

        public void AddChildSampleFile(SampleFileEntry childSampleFile)
        {
            if (null == childSampleFiles)
                childSampleFiles = new List<SampleFileEntry>();

            childSampleFiles.Add(childSampleFile);
        }

        public string FileName { get; private set; }
        public string FilePath { get; private set; }
        public IEnumerable<SampleFileEntry> Children { get { return childSampleFiles; } }
    }

}
