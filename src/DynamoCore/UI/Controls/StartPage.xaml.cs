using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace Dynamo.UI.Controls
{
    public class StartPageListItem
    {
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

        // Extended (derived) class properties.
        public Visibility IconVisibility
        {
            get { return ((this.Icon == null) ? Visibility.Collapsed : Visibility.Visible); }
        }
    }

    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : UserControl
    {
        private ObservableCollection<StartPageListItem> fileList = null;
        private ObservableCollection<StartPageListItem> recentList = null;
        private ObservableCollection<StartPageListItem> sampleList = null;
        private ObservableCollection<StartPageListItem> askList = null;
        private ObservableCollection<StartPageListItem> referenceList = null;
        private ObservableCollection<StartPageListItem> codeList = null;

        public StartPage()
        {
            InitializeComponent();
            this.Loaded += OnStartPageLoaded;
        }

        void OnStartPageLoaded(object sender, RoutedEventArgs e)
        {
            fileList = new ObservableCollection<StartPageListItem>();
            recentList = new ObservableCollection<StartPageListItem>();
            sampleList = new ObservableCollection<StartPageListItem>();
            askList = new ObservableCollection<StartPageListItem>();
            referenceList = new ObservableCollection<StartPageListItem>();
            codeList = new ObservableCollection<StartPageListItem>();

            var fileListItems = new StartPageListItem[]
            {
                new StartPageListItem("New", "icon-new.png"),
                new StartPageListItem("Open", "icon-open.png")
            };

            foreach (var item in fileListItems)
                this.fileList.Add(item);

            var recentListItems = new StartPageListItem[]
            {
                new StartPageListItem("ImportUsingSat"),
                new StartPageListItem("door_movable-copy"),
                new StartPageListItem("door_movable"),
                new StartPageListItem("test file"),
                new StartPageListItem("doormovable")
            };

            foreach (var item in recentListItems)
                this.recentList.Add(item);

            var sampleListItems = new StartPageListItem[]
            {
                new StartPageListItem("Abstract cubes"),
                new StartPageListItem("Attractor circles"),
                new StartPageListItem("Parametric bridge"),
                new StartPageListItem("Rotated bricks"),
                new StartPageListItem("Shading devices")
            };

            foreach (var item in sampleListItems)
                this.sampleList.Add(item);

            var askListItems = new StartPageListItem[]
            {
                new StartPageListItem("Discussion forum", "icon-discussion.png"),
                new StartPageListItem("email team@dynamobim.org", "icon-email.png"),
                new StartPageListItem("Visit www.dynamobim.org", "icon-dynamobim.png")
            };

            foreach (var item in askListItems)
                this.askList.Add(item);

            var referenceListItems = new StartPageListItem[]
            {
                new StartPageListItem("Dynamo reference", "icon-reference.png"),
                new StartPageListItem("Video tutorials", "icon-video.png")
            };

            foreach (var item in referenceListItems)
                this.referenceList.Add(item);

            var codeListItems = new StartPageListItem[]
            {
                new StartPageListItem("Github repository", "icon-github.png"),
                new StartPageListItem("Send issues", "icon-issues.png")
            };

            foreach (var item in codeListItems)
                this.codeList.Add(item);

            this.filesListBox.ItemsSource = fileList;
            this.recentListBox.ItemsSource = recentList;
            this.samplesListBox.ItemsSource = sampleList;
            this.askListBox.ItemsSource = askList;
            this.referenceListBox.ItemsSource = referenceList;
            this.codeListBox.ItemsSource = codeList;
        }
    }
}
