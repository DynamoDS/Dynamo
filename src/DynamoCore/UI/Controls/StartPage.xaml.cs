using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : UserControl
    {
        public StartPage()
        {
            InitializeComponent();
            this.Loaded += OnStartPageLoaded;
        }

        void OnStartPageLoaded(object sender, RoutedEventArgs e)
        {
            this.filesListBox.Items.Add("New");
            this.filesListBox.Items.Add("Open");

            this.recentListBox.Items.Add("ImportUsingSat");
            this.recentListBox.Items.Add("door_movable-copy");
            this.recentListBox.Items.Add("door_movable");
            this.recentListBox.Items.Add("test file");
            this.recentListBox.Items.Add("doormovable");

            this.samplesListBox.Items.Add("Abstract cubes");
            this.samplesListBox.Items.Add("Attractor circles");
            this.samplesListBox.Items.Add("Parametric bridge");
            this.samplesListBox.Items.Add("Rotated bricks");
            this.samplesListBox.Items.Add("Shading devices");

            this.askListBox.Items.Add("Discussion forum");
            this.askListBox.Items.Add("email team@dynamobim.org");
            this.askListBox.Items.Add("Visit www.dynamobim.org");

            this.referenceListBox.Items.Add("Dynamo reference");
            this.referenceListBox.Items.Add("Video tutorials");

            this.codeListBox.Items.Add("Github repository");
            this.codeListBox.Items.Add("Send issues");
        }
    }
}
