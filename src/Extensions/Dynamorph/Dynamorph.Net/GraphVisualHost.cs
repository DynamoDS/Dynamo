using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Dynamorph
{
    class GraphVisualHost : FrameworkElement
    {
        private SynthesizedGraph graph = null;
        private VisualCollection childVisuals = null;
        private DrawingVisual nodeVisuals = new DrawingVisual();
        private DrawingVisual edgeVisuals = new DrawingVisual();

        #region Public Operational Class Methods

        internal GraphVisualHost()
        {
            childVisuals = new VisualCollection(this);
            childVisuals.Add(edgeVisuals);
            childVisuals.Add(nodeVisuals);
            this.Loaded += OnGraphVisualHostLoaded;
        }

        internal void RefreshGraph(SynthesizedGraph graph)
        {
            this.graph = graph;

            if (this.IsLoaded != false)
            {
                RefreshGraphNodes();
                RefreshGraphEdges();
            }
        }

        #endregion

        #region FrameworkElement Overridable Methods

        protected override int VisualChildrenCount
        {
            get { return childVisuals.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return childVisuals[index];
        }

        #endregion

        #region Private Class Event Handlers

        private void OnGraphVisualHostLoaded(object sender, RoutedEventArgs e)
        {
            RefreshGraphNodes();
            RefreshGraphEdges();
        }

        #endregion

        #region Private Class Helper Methods

        private void RefreshGraphNodes()
        {
            if (graph == null || (graph.Nodes.Any() == false))
                return; // There is no graph or it's an empty graph.

            var drawingContext = nodeVisuals.RenderOpen();

            foreach (var node in graph.Nodes)
            {
                drawingContext.DrawRectangle(Brushes.Black, null, node.Rect);
            }

            drawingContext.Close(); // Done drawing, commit changes.
        }

        private void RefreshGraphEdges()
        {
        }

        #endregion
    }
}
