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
using System.Diagnostics;
using System.Threading.Tasks;

namespace DelaunayWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // set up the trackball
            var trackball = new Wpf3DTools.Trackball();
            trackball.EventSource = background;
            viewport.Camera.Transform = trackball.Transform;
            light.Transform = trackball.RotateTransform;
        }

        RandomTriangulation triangulation;

        void Create(bool uniform = false)
        {
            if (triangulation != null) viewport.Children.Remove(triangulation);

            int count;
            if (!int.TryParse(numPoints.Text, out count) && count < 10) count = 100;

            triangulation = RandomTriangulation.Create(count, 10, uniform);
            viewport.Children.Add(triangulation);

            infoText.Text = string.Format("{0} tetrahedrons", triangulation.Count);
        }
        
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Create();
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            Create();
        }

        private void GenerateUniformButton_Click(object sender, RoutedEventArgs e)
        {
            Create(true);
        }

        private void ExpandButton_Click(object sender, RoutedEventArgs e)
        {
            triangulation.Expand();
        }
       
        private void ExpandRandomButton_Click(object sender, RoutedEventArgs e)
        {
            triangulation.ExpandRandom();
        }

        private void CollapseButton_Click(object sender, RoutedEventArgs e)
        {
            triangulation.Collapse();
        }

        private void PulseButton_Click(object sender, RoutedEventArgs e)
        {
            triangulation.Pulse();
        }
    }
}