using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dynamorph
{
    public partial class DynamorphControl : UserControl
    {
        private Dynamorph.VisualizerHwndHost visualizer = null;

        public DynamorphControl()
        {
            InitializeComponent();
            this.Loaded += OnDynamorphControlLoaded;
        }

        internal void DestroyVisualizer()
        {
            if (visualizer != null)
            {
                visualizer.Dispose();
                visualizer = null;
            }
        }

        private void OnDynamorphControlLoaded(object sender, RoutedEventArgs e)
        {
            if (visualizer == null)
            {
                var b = VisualizerHostElement;
                visualizer = new VisualizerHwndHost(b.ActualWidth, b.ActualHeight);
                VisualizerHostElement.Child = visualizer;
            }
        }
    }
}
