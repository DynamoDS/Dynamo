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
        private ObservableCollection<StartPageListItem> fileList = null;
        private ObservableCollection<StartPageListItem> recentList = null;
        private ObservableCollection<StartPageListItem> sampleList = null;
        private ObservableCollection<StartPageListItem> askList = null;
        private ObservableCollection<StartPageListItem> referenceList = null;
        private ObservableCollection<StartPageListItem> codeList = null;

        public StartPage()
        {
            InitializeComponent();
        }

        public StartPage(DynamoViewModel dynamoViewModel)
        {
            InitializeComponent();

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
            fileList = new ObservableCollection<StartPageListItem>();
            recentList = new ObservableCollection<StartPageListItem>();
            sampleList = new ObservableCollection<StartPageListItem>();
            askList = new ObservableCollection<StartPageListItem>();
            referenceList = new ObservableCollection<StartPageListItem>();
            codeList = new ObservableCollection<StartPageListItem>();

            var fileListItems = new StartPageListItem[]
            {
                new StartPageListItem("New", "icon-new.png")
                {
                    ContextData = CommandNames.NewWorkspace
                },
                new StartPageListItem("Open", "icon-open.png")
                {
                    ContextData = CommandNames.OpenWorkspace
                }
            };

            foreach (var item in fileListItems)
            {
                item.ClickAction = StartPageListItem.Action.RegularCommand;
                this.fileList.Add(item);
            }

            var sampleListItems = new StartPageListItem[]
            {
                new StartPageListItem("Abstract cubes"),
                new StartPageListItem("Attractor circles"),
                new StartPageListItem("Parametric bridge"),
                new StartPageListItem("Rotated bricks"),
                new StartPageListItem("Shading devices")
            };

            foreach (var item in sampleListItems)
            {
                item.ClickAction = StartPageListItem.Action.FilePath;
                this.sampleList.Add(item);
            }

            var askListItems = new StartPageListItem[]
            {
                new StartPageListItem("Discussion forum", "icon-discussion.png")
                {
                    ContextData = Configurations.DynamoBimForum
                },
                new StartPageListItem("email team@dynamobim.org", "icon-email.png")
                {
                    ContextData = Configurations.DynamoTeamEmail
                },
                new StartPageListItem("Visit www.dynamobim.org", "icon-dynamobim.png")
                {
                    ContextData = Configurations.DynamoSiteLink
                }
            };

            foreach (var item in askListItems)
            {
                item.ClickAction = StartPageListItem.Action.ExternalUrl;
                this.askList.Add(item);
            }

            var referenceListItems = new StartPageListItem[]
            {
                new StartPageListItem("PDF Tutorials", "icon-reference.png")
                {
                    ContextData = Configurations.DynamoPdfTutorials
                },
                new StartPageListItem("Video tutorials", "icon-video.png")
                {
                    ContextData = Configurations.DynamoVideoTutorials
                }
            };

            foreach (var item in referenceListItems)
            {
                item.ClickAction = StartPageListItem.Action.ExternalUrl;
                this.referenceList.Add(item);
            }

            var codeListItems = new StartPageListItem[]
            {
                new StartPageListItem("Github repository", "icon-github.png")
                {
                    ContextData = Configurations.GitHubDynamoLink
                },
                new StartPageListItem("Send issues", "icon-issues.png")
                {
                    ContextData = Configurations.GitHubBugReportingLink
                }
            };

            foreach (var item in codeListItems)
            {
                item.ClickAction = StartPageListItem.Action.ExternalUrl;
                this.codeList.Add(item);
            }

            RefreshRecentFileList(dynamoViewModel.RecentFiles);

            this.filesListBox.ItemsSource = fileList;
            this.recentListBox.ItemsSource = recentList;
            this.samplesListBox.ItemsSource = sampleList;
            this.askListBox.ItemsSource = askList;
            this.referenceListBox.ItemsSource = referenceList;
            this.codeListBox.ItemsSource = codeList;
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
