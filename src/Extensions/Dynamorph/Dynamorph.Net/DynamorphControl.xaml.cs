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
        private SynthesizedGraph pendingGraph = null;
        private VisualizerHwndHost visualizer = null;
        private GraphVisualHost graphVisualHost = null;

        #region Public Operational Class Methods

        public DynamorphControl()
        {
            InitializeComponent();
            this.Loaded += OnDynamorphControlLoaded;
        }

        public ISynthesizedGraph GetSynthesizedGraph()
        {
            return new SynthesizedGraph();
        }

        public void SetSynthesizedGraph(ISynthesizedGraph graph)
        {
            var nextGraph = graph as SynthesizedGraph;
            nextGraph.BuildGraphStructure();

            if (this.graphVisualHost == null)
            {
                // No visual host yet, set it later when it gets created.
                this.pendingGraph = nextGraph;
            }
            else
            {
                // The graph visual host is ready to update.
                this.graphVisualHost.RefreshGraph(nextGraph);
            }
        }

        #endregion

        #region Internal Class Methods

        internal void DestroyVisualizer()
        {
            if (visualizer != null)
            {
                visualizer.Dispose();
                visualizer = null;
            }
        }

        #endregion

        #region Private Class Event Handlers

        private void OnDynamorphControlLoaded(object sender, RoutedEventArgs e)
        {
            this.graphVisualHost = new GraphVisualHost();
            this.GraphCanvas.Children.Add(graphVisualHost);

            if (this.pendingGraph != null)
            {
                // A graph was sent before GraphVisualHost got created, now 
                // that we have the visual host, set the graph for display.
                this.graphVisualHost.RefreshGraph(this.pendingGraph);
                this.pendingGraph = null;
            }

            if (visualizer == null)
            {
                var b = VisualizerHostElement;
                visualizer = new VisualizerHwndHost(b.ActualWidth, b.ActualHeight);
                VisualizerHostElement.Child = visualizer;
            }
        }

        #endregion
    }
}
