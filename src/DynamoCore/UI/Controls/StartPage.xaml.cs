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

        public ImageSource Icon { get; private set; }
        public string Caption { get; private set; }
        public string SubScript { get; private set; }
        public string ToolTip { get; private set; }
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

            this.fileList.Add(new StartPageListItem("New"));
            this.fileList.Add(new StartPageListItem("Open"));

            this.recentList.Add(new StartPageListItem("ImportUsingSat"));
            this.recentList.Add(new StartPageListItem("door_movable-copy"));
            this.recentList.Add(new StartPageListItem("door_movable"));
            this.recentList.Add(new StartPageListItem("test file"));
            this.recentList.Add(new StartPageListItem("doormovable"));

            this.sampleList.Add(new StartPageListItem("Abstract cubes"));
            this.sampleList.Add(new StartPageListItem("Attractor circles"));
            this.sampleList.Add(new StartPageListItem("Parametric bridge"));
            this.sampleList.Add(new StartPageListItem("Rotated bricks"));
            this.sampleList.Add(new StartPageListItem("Shading devices"));

            this.askList.Add(new StartPageListItem("Discussion forum"));
            this.askList.Add(new StartPageListItem("email team@dynamobim.org"));
            this.askList.Add(new StartPageListItem("Visit www.dynamobim.org"));

            this.referenceList.Add(new StartPageListItem("Dynamo reference"));
            this.referenceList.Add(new StartPageListItem("Video tutorials"));

            this.codeList.Add(new StartPageListItem("Github repository"));
            this.codeList.Add(new StartPageListItem("Send issues"));

            this.filesListBox.ItemsSource = fileList;
            this.recentListBox.ItemsSource = recentList;
            this.samplesListBox.ItemsSource = sampleList;
            this.askListBox.ItemsSource = askList;
            this.referenceListBox.ItemsSource = referenceList;
            this.codeListBox.ItemsSource = codeList;
        }
    }
}
