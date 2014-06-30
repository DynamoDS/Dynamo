using Dynamo.Utilities;
using Dynamo.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Dynamo.UI.Controls
{
    public class StartPageViewModel : ViewModelBase
    {
        List<StartPageListItem> fileOperations = new List<StartPageListItem>();
        List<StartPageListItem> sampleFiles = new List<StartPageListItem>();
        List<StartPageListItem> communityLinks = new List<StartPageListItem>();
        List<StartPageListItem> references = new List<StartPageListItem>();
        List<StartPageListItem> contributeLinks = new List<StartPageListItem>();

        internal StartPageViewModel()
        {
            #region File Operations

            fileOperations.Add(new StartPageListItem("New", "icon-new.png")
            {
                ContextData = CommandNames.NewWorkspace,
                ClickAction = StartPageListItem.Action.RegularCommand
            });

            fileOperations.Add(new StartPageListItem("Open", "icon-open.png")
            {
                ContextData = CommandNames.OpenWorkspace,
                ClickAction = StartPageListItem.Action.RegularCommand
            });

            #endregion

            #region Community Links

            communityLinks.Add(new StartPageListItem("Discussion forum", "icon-discussion.png")
            {
                ContextData = Configurations.DynamoBimForum,
                ClickAction = StartPageListItem.Action.ExternalUrl
            });

            communityLinks.Add(new StartPageListItem("Visit www.dynamobim.org", "icon-dynamobim.png")
            {
                ContextData = Configurations.DynamoSiteLink,
                ClickAction = StartPageListItem.Action.ExternalUrl
            });

            #endregion

            #region Reference List

            references.Add(new StartPageListItem("PDF Tutorials", "icon-reference.png")
            {
                ContextData = Configurations.DynamoPdfTutorials,
                ClickAction = StartPageListItem.Action.ExternalUrl
            });

            references.Add(new StartPageListItem("Video tutorials", "icon-video.png")
            {
                ContextData = Configurations.DynamoVideoTutorials,
                ClickAction = StartPageListItem.Action.ExternalUrl
            });

            #endregion

            #region Contribution Links

            contributeLinks.Add(new StartPageListItem("Github repository", "icon-github.png")
            {
                ContextData = Configurations.GitHubDynamoLink,
                ClickAction = StartPageListItem.Action.ExternalUrl
            });

            contributeLinks.Add(new StartPageListItem("Send issues", "icon-issues.png")
            {
                ContextData = Configurations.GitHubBugReportingLink,
                ClickAction = StartPageListItem.Action.ExternalUrl
            });

            #endregion

            EnumerateSampleFiles();
        }

        #region Public Class Properties

        public IEnumerable<StartPageListItem> FileOperations
        {
            get { return this.fileOperations; }
        }

        public IEnumerable<StartPageListItem> SampleFiles
        {
            get { return this.sampleFiles; }
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

        #region Private Class Helper Methods

        private void EnumerateSampleFiles()
        {
            sampleFiles.Add(new StartPageListItem("Hey")
            {
                ClickAction = StartPageListItem.Action.FilePath
            });
        }

        #endregion
    }

    public class StartPageListItem
    {
        public enum Action
        {
            RegularCommand,
            FilePath,
            ExternalUrl
        }

        internal StartPageListItem(string caption)
        {
            this.Caption = caption;
        }

        internal StartPageListItem(string caption, string imageFileName)
        {
            this.Caption = caption;

            var format = @"pack://application:,,,/DynamoCore;component/UI/Images/StartPage/{0}";
            var imageUri = new Uri(string.Format(format, imageFileName));
            this.Icon = new BitmapImage(imageUri);
        }

        public ImageSource Icon { get; private set; }
        public string Caption { get; private set; }
        public string SubScript { get; set; }
        public string ToolTip { get; set; }
        public string ContextData { get; set; }
        public Action ClickAction { get; set; }

        // Extended (derived) class properties.
        public Visibility IconVisibility
        {
            get { return ((this.Icon == null) ? Visibility.Collapsed : Visibility.Visible); }
        }
    }

    struct CommandNames
    {
        public const string NewWorkspace = "NewWorkspace";
        public const string OpenWorkspace = "OpenWorkspace";
    }

    public partial class StartPage : UserControl
    {
        private DynamoViewModel dynamoViewModel = null;
        private ObservableCollection<StartPageListItem> recentList = null;

        public StartPage()
        {
            InitializeComponent();
            this.recentList = new ObservableCollection<StartPageListItem>();

            this.Loaded += OnStartPageLoaded;
            this.dynamoViewModel = dynSettings.Controller.DynamoViewModel;
            this.dynamoViewModel.RecentFiles.CollectionChanged += OnRecentFilesChanged;
        }

        internal void PopulateSampleFileList(IEnumerable<string> filePaths)
        {
            if (filePaths == null || (filePaths.Count() <= 0))
                return;

            var sampleList = new List<StartPageListItem>();
            foreach (var filePath in filePaths)
            {
                var path = Path.GetFileNameWithoutExtension(filePath);
                sampleList.Add(new StartPageListItem(path)
                {
                    ContextData = filePath,
                    ToolTip = filePath,
                    ClickAction = StartPageListItem.Action.FilePath
                });
            }

            this.samplesListBox.ItemsSource = sampleList;
        }

        #region Private Class Event Handlers

        private void OnRecentFilesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshRecentFileList(sender as IEnumerable<string>);
        }

        private void OnStartPageLoaded(object sender, RoutedEventArgs e)
        {
            RefreshRecentFileList(dynamoViewModel.RecentFiles);
            this.recentListBox.ItemsSource = recentList;

            var startPageViewModel = this.DataContext as StartPageViewModel;
            this.filesListBox.ItemsSource = startPageViewModel.FileOperations;
            this.samplesListBox.ItemsSource = startPageViewModel.SampleFiles;
            this.askListBox.ItemsSource = startPageViewModel.CommunityLinks;
            this.referenceListBox.ItemsSource = startPageViewModel.References;
            this.codeListBox.ItemsSource = startPageViewModel.ContributeLinks;
        }

        private void OnItemSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e == null || (e.AddedItems.Count <= 0))
                return;

            var selected = e.AddedItems[0] as StartPageListItem;
            if (selected == null)
                return;

            switch (selected.ClickAction)
            {
                case StartPageListItem.Action.RegularCommand:
                    HandleRegularCommand(selected);
                    break;

                case StartPageListItem.Action.FilePath:
                    HandleFilePath(selected);
                    break;

                case StartPageListItem.Action.ExternalUrl:
                    HandleExternalUrl(selected);
                    break;
            }

            var listBox = sender as ListBox;
            listBox.SelectedIndex = -1;
        }

        #endregion

        #region Private Class Helper Methods

        private void RefreshRecentFileList(IEnumerable<string> recentFiles)
        {
            recentList.Clear();
            foreach (var recentFile in recentFiles)
            {
                var extension = Path.GetExtension(recentFile).ToUpper();
                var caption = Path.GetFileNameWithoutExtension(recentFile);
                recentList.Add(new StartPageListItem(caption)
                {
                    ContextData = recentFile,
                    ToolTip = recentFile,
                    SubScript = extension.Substring(1), // Skipping the 'dot'
                    ClickAction = StartPageListItem.Action.FilePath
                });
            }
        }

        private void HandleRegularCommand(StartPageListItem item)
        {
            switch (item.ContextData)
            {
                case CommandNames.NewWorkspace:
                    dynamoViewModel.NewHomeWorkspaceCommand.Execute(null);
                    break;

                case CommandNames.OpenWorkspace:
                    dynamoViewModel.ShowOpenDialogAndOpenResultCommand.Execute(null);
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
                MessageBox.Show(string.Format("File not found: {0}", path));
                return;
            }

            if (dynamoViewModel.OpenCommand.CanExecute(path))
                dynamoViewModel.OpenCommand.Execute(path);
        }

        private void HandleExternalUrl(StartPageListItem item)
        {
            System.Diagnostics.Process.Start(item.ContextData);
        }

        #endregion
    }
}
