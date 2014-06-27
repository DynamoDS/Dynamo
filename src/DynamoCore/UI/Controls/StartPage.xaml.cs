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

    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : UserControl
    {
        private DynamoViewModel dynamoViewModel = null;
        private ObservableCollection<StartPageListItem> recentList = null;

        public StartPage()
        {
            InitializeComponent();
        }

        public StartPage(DynamoViewModel dynamoViewModel)
        {
            InitializeComponent();
            this.recentList = new ObservableCollection<StartPageListItem>();

            this.Loaded += OnStartPageLoaded;
            this.dynamoViewModel = dynamoViewModel;
            this.dynamoViewModel.RecentFiles.CollectionChanged += OnRecentFilesChanged;
        }

        #region Private Class Event Handlers

        private void OnRecentFilesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshRecentFileList(sender as IEnumerable<string>);
        }

        private void OnStartPageLoaded(object sender, RoutedEventArgs e)
        {
            var fileList = new List<StartPageListItem>();
            {
                fileList.Add(new StartPageListItem("New", "icon-new.png")
                {
                    ContextData = CommandNames.NewWorkspace,
                    ClickAction = StartPageListItem.Action.RegularCommand
                });

                fileList.Add(new StartPageListItem("Open", "icon-open.png")
                {
                    ContextData = CommandNames.OpenWorkspace,
                    ClickAction = StartPageListItem.Action.RegularCommand
                });

                filesListBox.ItemsSource = fileList;
            }

            var sampleList = new List<StartPageListItem>();
            {
                sampleList.Add(new StartPageListItem("Abstract cubes"));
                sampleList.Add(new StartPageListItem("Attractor circles"));
                sampleList.Add(new StartPageListItem("Parametric bridge"));
                sampleList.Add(new StartPageListItem("Rotated bricks"));
                sampleList.Add(new StartPageListItem("Shading devices"));

                sampleList.ForEach((x) =>
                {
                    x.ClickAction = StartPageListItem.Action.FilePath;
                });

                this.samplesListBox.ItemsSource = sampleList;
            }

            var askList = new List<StartPageListItem>();
            {
                askList.Add(new StartPageListItem("Discussion forum", "icon-discussion.png")
                {
                    ContextData = Configurations.DynamoBimForum,
                    ClickAction = StartPageListItem.Action.ExternalUrl
                });

                askList.Add(new StartPageListItem("email team@dynamobim.org", "icon-email.png")
                {
                    ContextData = Configurations.DynamoTeamEmail,
                    ClickAction = StartPageListItem.Action.ExternalUrl
                });

                askList.Add(new StartPageListItem("Visit www.dynamobim.org", "icon-dynamobim.png")
                {
                    ContextData = Configurations.DynamoSiteLink,
                    ClickAction = StartPageListItem.Action.ExternalUrl
                });

                this.askListBox.ItemsSource = askList;
            }

            var referenceList = new List<StartPageListItem>();
            {
                referenceList.Add(new StartPageListItem("PDF Tutorials", "icon-reference.png")
                {
                    ContextData = Configurations.DynamoPdfTutorials
                });

                referenceList.Add(new StartPageListItem("Video tutorials", "icon-video.png")
                {
                    ContextData = Configurations.DynamoVideoTutorials
                });

                referenceList.ForEach((x) =>
                {
                    x.ClickAction = StartPageListItem.Action.ExternalUrl;
                });

                this.referenceListBox.ItemsSource = referenceList;
            }

            var codeList = new List<StartPageListItem>();
            {
                codeList.Add(new StartPageListItem("Github repository", "icon-github.png")
                {
                    ContextData = Configurations.GitHubDynamoLink
                });

                codeList.Add(new StartPageListItem("Send issues", "icon-issues.png")
                {
                    ContextData = Configurations.GitHubBugReportingLink
                });

                codeList.ForEach((x) =>
                {
                    x.ClickAction = StartPageListItem.Action.ExternalUrl;
                });

                this.codeListBox.ItemsSource = codeList;
            }

            RefreshRecentFileList(dynamoViewModel.RecentFiles);
            this.recentListBox.ItemsSource = recentList;
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
                var caption = Path.GetFileNameWithoutExtension(recentFile);
                recentList.Add(new StartPageListItem(caption)
                {
                    ContextData = recentFile,
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
