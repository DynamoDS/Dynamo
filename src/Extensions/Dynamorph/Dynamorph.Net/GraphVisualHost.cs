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
                PathGeometry geom;
                PathFigure figure;
                BezierSegment s = new BezierSegment();

                drawingContext.DrawRectangle(Brushes.Black, null, node.Rect);
            }

            drawingContext.Close(); // Done drawing, commit changes.
        }

        private void RefreshGraphEdges()
        {
            if (graph == null || (graph.Edges.Any() == false))
                return; // There is no graph or it has no edges.

            var startPoints = new PointCollection();
            var endPoints = new PointCollection();

            var nodes = this.graph.Nodes;
            foreach (var edge in this.graph.Edges)
            {
                var startNode = nodes.First(n => n.Identifier == edge.StartNodeId);
                var endNode = nodes.First(n => n.Identifier == edge.EndNodeId);
                startPoints.Add(startNode.OutputPoint);
                endPoints.Add(endNode.InputPoint);
            }

            var drawingContext = edgeVisuals.RenderOpen();
            for (int index = 0; index < startPoints.Count; ++index)
            {
                PathGeometry pathGeometry = new PathGeometry();
                PathFigure pathFigure = new PathFigure()
                {
                    StartPoint = startPoints[index]
                };

                AddBezier(pathFigure, startPoints[index], endPoints[index]);
                pathGeometry.Figures.Add(pathFigure);
                drawingContext.DrawGeometry(null, new Pen(Brushes.Red, 1.0), pathGeometry);
            }

            drawingContext.Close();
        }

        private void AddBezier(PathFigure figure, Point start, Point end)
        {
            var offset = ((end.X - start.X) * 0.25);
            figure.StartPoint = start;

            BezierSegment bezierSegment = new BezierSegment(
                new Point(start.X + offset, start.Y),
                new Point(end.X - offset, end.Y), end, true);

            figure.Segments.Add(bezierSegment);
        }

        #endregion
    }
}
