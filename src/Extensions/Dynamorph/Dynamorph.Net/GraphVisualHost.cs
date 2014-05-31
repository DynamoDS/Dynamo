using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Dynamorph
{
    class GraphResources
    {
        internal enum BrushIndex
        {
            NodeFillColor,
            NodeBorderColor,
            NodeTextColor,

            Lime,
            Green,
            Emerald,
            Teal,
            Cyan,
            Cobalt,
            Indigo,
            Violet,
            Pink,
            Magenta,
            Crimson,
            Red,
            Orange,
            Amber,
            Yellow,
            Brown,
            Olive,
            Steel,
            Mauve,
            Taupe,
        }

        static GraphResources()
        {
            Brushes = new List<SolidColorBrush>();
            Brushes.Add(new SolidColorBrush(Color.FromRgb(0xcb, 0xc6, 0xbe))); // NodeFillColor
            Brushes.Add(new SolidColorBrush(Color.FromRgb(0x5e, 0x5c, 0x5a))); // NodeBorderColor
            Brushes.Add(new SolidColorBrush(Color.FromRgb(0xff, 0xff, 0xff))); // NodeTextColor

            Brushes.Add(new SolidColorBrush(Color.FromRgb(164, 196, 0)));   // Lime
            Brushes.Add(new SolidColorBrush(Color.FromRgb(96, 169, 23)));   // Green
            Brushes.Add(new SolidColorBrush(Color.FromRgb(0, 138, 0)));     // Emerald
            Brushes.Add(new SolidColorBrush(Color.FromRgb(0, 171, 169)));   // Teal
            Brushes.Add(new SolidColorBrush(Color.FromRgb(27, 161, 226)));  // Cyan
            Brushes.Add(new SolidColorBrush(Color.FromRgb(0, 80, 239)));    // Cobalt
            Brushes.Add(new SolidColorBrush(Color.FromRgb(106, 0, 255)));   // Indigo
            Brushes.Add(new SolidColorBrush(Color.FromRgb(170, 0, 255)));   // Violet
            Brushes.Add(new SolidColorBrush(Color.FromRgb(244, 114, 208))); // Pink
            Brushes.Add(new SolidColorBrush(Color.FromRgb(216, 0, 115)));   // Magenta
            Brushes.Add(new SolidColorBrush(Color.FromRgb(162, 0, 37)));    // Crimson
            Brushes.Add(new SolidColorBrush(Color.FromRgb(229, 20, 0)));    // Red    
            Brushes.Add(new SolidColorBrush(Color.FromRgb(250, 104, 0)));   // Orange
            Brushes.Add(new SolidColorBrush(Color.FromRgb(240, 163, 10)));  // Amber
            Brushes.Add(new SolidColorBrush(Color.FromRgb(227, 200, 0)));   // Yellow
            Brushes.Add(new SolidColorBrush(Color.FromRgb(130, 90, 44)));   // Brown
            Brushes.Add(new SolidColorBrush(Color.FromRgb(109, 135, 100))); // Olive
            Brushes.Add(new SolidColorBrush(Color.FromRgb(100, 118, 135))); // Steel
            Brushes.Add(new SolidColorBrush(Color.FromRgb(118, 96, 138)));  // Mauve
            Brushes.Add(new SolidColorBrush(Color.FromRgb(135, 121, 78)));  // Taupe

            Brushes.ForEach(b => b.Freeze());
        }

        internal static SolidColorBrush Brush(BrushIndex index)
        {
            return Brushes[(int)index];
        }

        internal static SolidColorBrush Pen(int index)
        {
            int start = ((int)BrushIndex.Lime);
            int end = ((int)BrushIndex.Taupe);
            int count = end - start + 1;

            return Brushes[(start + (index % count))];
        }

        internal static readonly List<SolidColorBrush> Brushes;
    }

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
                var bi = GraphResources.BrushIndex.NodeBorderColor;
                var fi = GraphResources.BrushIndex.NodeFillColor;
                var bp = new Pen(GraphResources.Brush(bi), 1.0);
                var rect = node.Rect;
                rect.Offset(-0.5, -0.5);
                drawingContext.DrawRectangle(GraphResources.Brush(fi), bp, rect);
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
                var pb = GraphResources.Pen(index);
                drawingContext.DrawGeometry(null, new Pen(pb, 2.0), pathGeometry);
            }

            drawingContext.Close();
        }

        private void AddBezier(PathFigure figure, Point start, Point end)
        {
            var offset = ((end.X - start.X) * 0.5);
            figure.StartPoint = start;

            BezierSegment bezierSegment = new BezierSegment(
                new Point(start.X + offset, start.Y),
                new Point(end.X - offset, end.Y), end, true);

            figure.Segments.Add(bezierSegment);
        }

        #endregion
    }
}
