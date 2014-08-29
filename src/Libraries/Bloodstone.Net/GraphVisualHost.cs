using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Dynamo.Bloodstone
{
    class GraphResources
    {
        internal enum BrushIndex
        {
            NodeBorderColor,
            NodeTextColor,
            EdgeOutlineColor,

            SliderEdge,
            SliderFill,
            SliderDivider,

            NodeColorStart,
            NodeColorCount = 16,
        }

        static GraphResources()
        {
            Brushes = new List<SolidColorBrush>();
            Brushes.Add(new SolidColorBrush(Color.FromRgb(0x5e, 0x5c, 0x5a))); // NodeBorderColor
            Brushes.Add(new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00))); // NodeTextColor
            Brushes.Add(new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33))); // EdgeOutlineColor

            Brushes.Add(new SolidColorBrush(Color.FromRgb(62, 62, 66)));       // SliderEdge
            Brushes.Add(new SolidColorBrush(Color.FromRgb(104, 104, 104)));    // SliderFill
            Brushes.Add(new SolidColorBrush(Color.FromRgb(30, 30, 30)));       // SliderDivider

            // Generate a series of colors. First we start off with even indices,
            // and then followed by odd indices. That will result in neighbouring
            // colors skipping "2 x unitAngle", creating better contrast. The 
            // reason direct opposite color is not used: if there are three nodes 
            // or more on the same column, then the third node will have a color 
            // that is too close to the first node. With the following color 
            // generation, the third node will end up having a color that is quite
            // far away from the first node, giving a better contrast.
            // 
            int count = ((int)BrushIndex.NodeColorCount);
            double unitAngle = 360.0 / count;

            for (int index = 0; index < count; index += 2) // Even indices
            {
                double degree = index * unitAngle;
                var color = Utilities.HsvToRgb(degree, 0.5, 1.0);
                Brushes.Add(new SolidColorBrush(color));
            }

            for (int index = 1; index < count; index += 2) // Odd indices
            {
                double degree = index * unitAngle;
                var color = Utilities.HsvToRgb(degree, 0.5, 1.0);
                Brushes.Add(new SolidColorBrush(color));
            }

            Brushes.ForEach(b => b.Freeze());
        }

        internal static SolidColorBrush Brush(BrushIndex index)
        {
            return Brushes[(int)index];
        }

        internal static SolidColorBrush NodeColor(int index)
        {
            int start = ((int)BrushIndex.NodeColorStart);
            int count = ((int)BrushIndex.NodeColorCount);
            return Brushes[(start + (index % count))];
        }

        internal static readonly List<SolidColorBrush> Brushes;
    }

    class SliderEventArgs : EventArgs
    {
        internal SliderEventArgs(double value)
        {
            this.Value = value;
        }

        internal double Value { get; private set; }
    }

    delegate void SliderChangedEventHandler(object sender, SliderEventArgs e);

    class SliderVisualHost : FrameworkElement
    {
        private double maxThumbOffset = 0;
        private double mouseThumbOffset = 0;
        private BitmapImage sliderThumbImage = null;
        private TranslateTransform horzTransform = null;
        private TranslateTransform thumbTransform = null;
        private VisualCollection childVisuals = null;
        private DrawingVisual sliderThumbLayer = new DrawingVisual();
        private DrawingVisual sliderBackground = new DrawingVisual();
        private ScrollViewer canvasScrollViewer = null;

        internal event SliderChangedEventHandler Changed = null;

        #region Public Operational Class Methods

        internal SliderVisualHost(ScrollViewer canvasScrollViewer)
        {
            this.childVisuals = new VisualCollection(this);
            this.childVisuals.Add(sliderBackground);
            this.childVisuals.Add(sliderThumbLayer);
            this.canvasScrollViewer = canvasScrollViewer;

            this.horzTransform = new TranslateTransform();
            this.RenderTransform = horzTransform;
            this.thumbTransform = new TranslateTransform();
            this.sliderThumbLayer.Transform = thumbTransform;

            this.Loaded += OnSliderVisualHostLoaded;
            this.canvasScrollViewer.ScrollChanged += OnCanvasScrollChanged;
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

        private Canvas ParentCanvas
        {
            get
            {
                Canvas parentCanvas = this.Parent as Canvas;
                if (parentCanvas == null)
                {
                    var message = "SliderVisualHost expects a Canvas as parent";
                    throw new InvalidOperationException(message);
                }

                return parentCanvas;
            }
        }

        #endregion

        #region Private Class Event Handlers

        private void OnSliderVisualHostLoaded(object sender, RoutedEventArgs e)
        {
            var pack = System.IO.Packaging.PackUriHelper.UriSchemePack;
            var path = pack + "://application:,,,/Bloodstone.Net;component/";
            var uri = new Uri(path + "Resources/Images/slider-thumb.png");
            sliderThumbImage = new BitmapImage(uri);
        }

        private void OnCanvasScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentWidthChange > 0.0)
            {
                // The thumb cannot move into the region of slider edges, 
                // and there are two edges to a slider, so we remove the 
                // edge width of total extent width, resulting in an area 
                // in which slider thumb can freely move.
                // 
                var edgeWidth = (Config.HorzGap + (0.5 * Config.NodeWidth));
                maxThumbOffset = (e.ExtentWidth - (2.0 * edgeWidth));
                UpdateAndNotify(this.thumbTransform.X);

                RenderSliderBackground(e);
                RenderSliderThumbSeparator(e);
            }
            else if (e.ExtentHeightChange > 0.0)
                RenderSliderThumbSeparator(e);

            // Update the horizontal offset of background.
            this.horzTransform.X = -e.HorizontalOffset;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (this.IsMouseCaptured)
            {
                var point = e.GetPosition(this);
                UpdateAndNotify(point.X - this.mouseThumbOffset);
            }
            else
            {
                var mouseOnThumb = IsMouseOnSliderThumb(e);
                this.Cursor = (mouseOnThumb ? Cursors.SizeWE : Cursors.Arrow);
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (IsMouseOnSliderThumb(e))
                {
                    var point = e.GetPosition(this);
                    this.mouseThumbOffset = point.X - thumbTransform.X;
                    this.CaptureMouse();
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (this.IsMouseCaptured)
                this.ReleaseMouseCapture();

            base.OnMouseUp(e);
        }

        #endregion

        #region Private Class Helper Methods

        private void UpdateAndNotify(double offset)
        {
            thumbTransform.X = offset;

            // Limit thumb movement to within range.
            if (this.thumbTransform.X < 0.0)
                this.thumbTransform.X = 0.0;
            else if (this.thumbTransform.X > maxThumbOffset)
                this.thumbTransform.X = maxThumbOffset;

            if (this.Changed != null)
            {
                double unitWidth = ((2.0 * Config.HorzGap) + Config.NodeWidth);
                Changed(this, new SliderEventArgs(thumbTransform.X / unitWidth));
            }
        }

        private bool IsMouseOnSliderThumb(MouseEventArgs e)
        {
            var result = VisualTreeHelper.HitTest(this, e.GetPosition(this));
            return (result != null && (result.VisualHit == sliderThumbLayer));
        }

        private void RenderSliderBackground(ScrollChangedEventArgs e)
        {
            var drawingContext = this.sliderBackground.RenderOpen();
            var fillBrush = GraphResources.Brush(GraphResources.BrushIndex.SliderFill);
            var edgeBrush = GraphResources.Brush(GraphResources.BrushIndex.SliderEdge);

            var height = ParentCanvas.ActualHeight - 1.0;
            var edgeWidth = (Config.HorzGap + (0.5 * Config.NodeWidth));
            var sliderWidth = e.ExtentWidth - edgeWidth - edgeWidth;

            // Fill the entire slider region with slider fill brush.
            Rect sliderRegion = new Rect(0.5 + edgeWidth, 0.5, sliderWidth, height);
            drawingContext.DrawRectangle(fillBrush, null, sliderRegion);
            drawingContext.Close();
        }

        private void RenderSliderThumbSeparator(ScrollChangedEventArgs e)
        {
            var drawingContext = this.sliderThumbLayer.RenderOpen();

            var thumbWidth = sliderThumbImage.PixelWidth;
            var thumbHeight = sliderThumbImage.PixelHeight;

            var height = ParentCanvas.ActualHeight + 1;
            var edgeWidth = (Config.HorzGap + (0.5 * Config.NodeWidth));

            var thumbRegion = new Rect((edgeWidth - (thumbWidth * 0.5)),
                ((height - thumbHeight) * 0.5), thumbWidth, thumbHeight);

            drawingContext.DrawImage(sliderThumbImage, thumbRegion);
            drawingContext.Close();
        }

        #endregion
    }

    class GraphVisualHost : FrameworkElement
    {
        private VisualCollection childVisuals = null;
        private DrawingVisual nodeVisuals = new DrawingVisual();
        private DrawingVisual edgeVisuals = new DrawingVisual();

        #region Public Operational Class Methods

        internal GraphVisualHost()
        {
            childVisuals = new VisualCollection(this);
            childVisuals.Add(edgeVisuals);
            childVisuals.Add(nodeVisuals);
        }

        internal void RefreshGraph(SynthesizedGraph graph)
        {
            if (this.IsLoaded != false)
            {
                RefreshGraphNodes(graph);
                RefreshGraphEdges(graph);
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

        private Canvas ParentCanvas
        {
            get
            {
                Canvas parentCanvas = this.Parent as Canvas;
                if (parentCanvas == null)
                {
                    var message = "GraphVisualHost expects a Canvas as parent";
                    throw new InvalidOperationException(message);
                }

                return parentCanvas;
            }
        }

        #endregion

        #region Private Class Event Handlers

        #endregion

        #region Private Class Helper Methods

        private void RefreshGraphNodes(SynthesizedGraph graph)
        {
            var parentCanvas = this.ParentCanvas;
            parentCanvas.Width = parentCanvas.Height = 0;
            if (graph == null || (graph.Nodes.Any() == false))
                return; // There is no graph or it's an empty graph.

            var drawingContext = nodeVisuals.RenderOpen();
            var textBrush = GraphResources.Brush(GraphResources.BrushIndex.NodeTextColor);

            // Font for use on node text.
            string fontResourceUri = "./Resources/Fonts/#Open Sans";
            string pack = System.IO.Packaging.PackUriHelper.UriSchemePack;
            var uri = new Uri(pack + "://application:,,,/Bloodstone.Net;component/");
            var textFontFamily = new FontFamily(uri, fontResourceUri);

            var typeface = new Typeface(textFontFamily, FontStyles.Normal,
                FontWeights.Normal, FontStretches.Normal);

            Rect boundingBox = graph.Nodes.ElementAt(0).Rect;
            foreach (var node in graph.Nodes)
            {
                var rect = node.Rect;
                boundingBox.Union(rect);

                var bi = GraphResources.BrushIndex.NodeBorderColor;
                var bp = new Pen(GraphResources.Brush(bi), 1.0);
                rect.Offset(-0.5, -0.5);
                var fi = GraphResources.NodeColor(node.NodeIndex);
                drawingContext.DrawRectangle(fi, bp, rect);

                FormattedText ft = new FormattedText(node.Name, CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight, typeface, 10.0, textBrush)
                    {
                        Trimming = TextTrimming.CharacterEllipsis,
                        TextAlignment = TextAlignment.Center
                    };

                rect = DeflateRectForText(ft, rect);
                drawingContext.DrawText(ft, rect.Location);
            }

            drawingContext.Close(); // Done drawing, commit changes.
            boundingBox.Inflate(Config.HorzGap, Config.VertGap);
            parentCanvas.Width = boundingBox.Width;
            parentCanvas.Height = boundingBox.Height;
        }

        private Rect DeflateRectForText(FormattedText formattedText, Rect rect)
        {
            double margin = 4.0;

            var gap = ((rect.Height - formattedText.Height) * 0.5);
            rect.Height = formattedText.Height;
            rect.Width = (rect.Width - (2.0 * margin));
            rect.Offset(margin, gap);

            formattedText.MaxTextWidth = rect.Width;
            formattedText.MaxTextHeight = rect.Height;
            return rect;
        }

        private void RefreshGraphEdges(SynthesizedGraph graph)
        {
            if (graph == null || (graph.Edges.Any() == false))
                return; // There is no graph or it has no edges.

            var nodes = graph.Nodes;
            var edgeData = new List<Tuple<int, Point, Point>>();

            foreach (var edge in graph.Edges)
            {
                var startNode = nodes.First(n => n.Identifier == edge.StartNodeId);
                var endNode = nodes.First(n => n.Identifier == edge.EndNodeId);

                edgeData.Add(new Tuple<int, Point, Point>(startNode.NodeIndex,
                    startNode.GetOutputPoint(edge.StartIndex),
                    endNode.GetInputPoint(edge.EndIndex)));
            }

            // Edge outline in darker color (before drawing the fill).
            var eb = GraphResources.Brush(GraphResources.BrushIndex.EdgeOutlineColor);

            var drawingContext = edgeVisuals.RenderOpen();
            foreach (var data in edgeData)
            {
                PathGeometry pathGeometry = new PathGeometry();
                PathFigure pathFigure = new PathFigure()
                {
                    StartPoint = data.Item2
                };

                AddBezier(pathFigure, data.Item2, data.Item3);
                pathGeometry.Figures.Add(pathFigure);
                var pb = GraphResources.NodeColor(data.Item1);
                drawingContext.DrawGeometry(null, new Pen(eb, 3.5), pathGeometry);
                drawingContext.DrawGeometry(null, new Pen(pb, 1.5), pathGeometry);

                var jointPen = new Pen(eb, 1.0);
                drawingContext.DrawEllipse(pb, jointPen, data.Item2, 4.0, 4.0);
                drawingContext.DrawEllipse(pb, jointPen, data.Item3, 4.0, 4.0);
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
