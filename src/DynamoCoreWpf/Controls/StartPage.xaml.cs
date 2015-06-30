using Dynamo.Utilities;
using Dynamo.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Practices.Prism.ViewModel;
using Dynamo.Wpf.Properties;
using Dynamo.Wpf.Views.Gallery;
using Dynamo.Wpf.ViewModels.Core;
using System.Linq;
using Dynamo.Services;

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

        internal StartPageListItem(string caption)
        {
            this.Caption = caption;
        }

        internal StartPageListItem(string caption, string iconPath)
        {
            this.Caption = caption;
            this.icon = LoadBitmapImage(iconPath);
        }

        #region Public Class Properties

        public string Caption { get; private set; }
        public string SubScript { get; set; }
        public string ToolTip { get; set; }
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

        private BitmapImage LoadBitmapImage(string iconPath)
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
            sampleFiles = new ObservableCollection<SampleFileEntry>();
            backupFiles = new ObservableCollection<StartPageListItem>();


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

            references.Add(new StartPageListItem(Resources.StartPageWhatsNew, "icon-whats-new.png")
            {
                ContextData = ButtonNames.ShowGallery,
                ClickAction = StartPageListItem.Action.RegularCommand
            });

            references.Add(new StartPageListItem(Resources.StartPageAdvancedTutorials, "icon-reference.png")
            {
                ContextData = Configurations.DynamoAdvancedTutorials,
                ClickAction = StartPageListItem.Action.ExternalUrl
            });

            references.Add(new StartPageListItem(Resources.StartPageVideoTutorials, "icon-video.png")
            {
                ContextData = Configurations.DynamoVideoTutorials,
                ClickAction = StartPageListItem.Action.ExternalUrl
            });

            references.Add(new StartPageListItem(Resources.StartPageMoreSamples, "icons-more-samples.png")
            {
                ContextData = Configurations.DynamoMoreSamples,
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
                ContextData = Configurations.GitHubBugReportingLink,
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
                            // Resursive call for each subdirectory.
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
                        }
                        // Add each file under the root directory property list.
                        rootProperty.AddChildSampleFile(new SampleFileEntry(file.Name, file.FullName));
                    }
                }
            }
            catch (Exception)
            {
                // Perhaps some permission problems?
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
            foreach (var filePath in filePaths)
            {
                var extension = Path.GetExtension(filePath).ToUpper();
                var caption = Path.GetFileNameWithoutExtension(filePath);
                files.Add(new StartPageListItem(caption)
                {
                    ContextData = filePath,
                    ToolTip = filePath,
                    SubScript = extension.Substring(1), // Skipping the 'dot'
                    ClickAction = StartPageListItem.Action.FilePath
                });
            }
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

                case ButtonNames.ShowGallery:
                    dvm.ShowGalleryCommand.Execute(null);
                    break;

                default:
                    throw new ArgumentException(
                        string.Format("Invalid command: {0}", item.ContextData));
            }
        }

        private void HandleFilePath(StartPageListItem item)
        {
            var path = item.ContextData;
            if (string.IsNullOrEmpty(path) || (File.Exists(path) == false))
            {
                MessageBox.Show(string.Format(Resources.MessageFileNotFound, path));
                return;
            }

            var dvm = this.DynamoViewModel;
            if (dvm.OpenCommand.CanExecute(path))
                dvm.OpenCommand.Execute(path);
        }

        private void HandleExternalUrl(StartPageListItem item)
        {
            System.Diagnostics.Process.Start(item.ContextData);
        }

        #endregion

    }

    struct ButtonNames
    {
        public const string NewWorkspace = "NewWorkspace";
        public const string NewCustomNodeWorkspace = "NewCustomNodeWorkspace";
        public const string OpenWorkspace = "OpenWorkspace";
        public const string ShowGallery = "ShowGallery";
    }

    public partial class StartPageView : UserControl
    {
        private DynamoViewModel dynamoViewModel;

        public StartPageView()
        {
            InitializeComponent();
            if (StabilityUtils.IsLastShutdownClean)
            {
                openAll.Visibility = Visibility.Collapsed;
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

            if (startPageViewModel.IsFirstRun)
            {
                dynamoViewModel.ShowGalleryCommand.Execute(null);
            }

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

        private void OnCloseStartPageClicked(object sender, MouseButtonEventArgs e)
        {
            this.dynamoViewModel.ShowStartPage = false;
        }

        #endregion

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
            Process.Start("explorer.exe", "/select," 
                + startPageViewModel.SampleFolderPath);
        }

        private void OpenAllFilesOnCrash(object sender, MouseButtonEventArgs e)
        {
            var dvm = dynamoViewModel;
            foreach (var filePath in dvm.Model.PreferenceSettings.BackupFiles)
            {
                if (dvm.OpenCommand.CanExecute(filePath))
                    dvm.OpenCommand.Execute(filePath);
            }
        }

        private void ShowBackupFilesInFolder(object sender, MouseButtonEventArgs e)
        {
            var startPageViewModel = this.DataContext as StartPageViewModel;
            Process.Start("explorer.exe", dynamoViewModel.Model.PathManager.BackupDirectory);
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
    }

    public class SampleFileEntry
    {
        List<SampleFileEntry> childSampleFiles = null;

        public SampleFileEntry(string name, string path)
        {
            this.FileName = name;
            this.FilePath = path;
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
