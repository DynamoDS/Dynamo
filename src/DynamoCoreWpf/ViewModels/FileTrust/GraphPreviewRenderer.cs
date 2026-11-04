using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Dynamo.Wpf.ViewModels.FileTrust
{
    /// <summary>
    /// Draws a <see cref="GraphPreview"/> as a single frozen vector image. Everything created
    /// here is frozen, so it can be built on a background thread and shown on the UI thread.
    /// </summary>
    internal static class GraphPreviewRenderer
    {
        // Approximate Dynamo node metrics, used because node sizes are not saved in .dyn files.
        private const double NodeHeaderHeight = 46;
        private const double PortHeight = 29;
        private const double MinNodeWidth = 110;
        private const double MaxNodeWidth = 360;
        private const double CharWidth = 8;
        private const double LabelFontSize = 14;
        private const double GroupTitleFontSize = 28;

        // Above this many nodes, labels are unreadable at fit-to-window zoom and only cost time.
        private const int MaxLabeledNodes = 1500;

        private static readonly Brush NodeFill = Frozen(new SolidColorBrush(Color.FromRgb(0x53, 0x53, 0x53)));
        private static readonly Brush NodeHeaderFill = Frozen(new SolidColorBrush(Color.FromRgb(0x3C, 0x3C, 0x3C)));
        private static readonly Brush LabelBrush = Frozen(new SolidColorBrush(Colors.White));
        private static readonly Brush GroupTitleBrush = Frozen(new SolidColorBrush(Color.FromRgb(0x3C, 0x3C, 0x3C)));
        private static readonly Brush NoteBrush = Frozen(new SolidColorBrush(Color.FromRgb(0x5C, 0x5C, 0x5C)));
        private static readonly Brush DefaultGroupFill = Frozen(new SolidColorBrush(Color.FromArgb(0x80, 0xC1, 0xD6, 0x76)));
        private static readonly Pen WirePen = Frozen(new Pen(Frozen(new SolidColorBrush(Color.FromRgb(0x99, 0x99, 0x99))), 2));

        private static readonly Typeface LabelTypeface = new Typeface("Segoe UI");

        /// <summary>
        /// Renders the preview. Returns null when there is nothing to draw.
        /// </summary>
        internal static ImageSource Render(GraphPreview preview, double pixelsPerDip)
        {
            if (preview == null || preview.IsEmpty) return null;

            var nodeBounds = new Rect[preview.Nodes.Count];
            for (var i = 0; i < preview.Nodes.Count; i++)
            {
                var node = preview.Nodes[i];
                var width = Math.Max(MinNodeWidth, Math.Min(MaxNodeWidth, (node.Name?.Length ?? 0) * CharWidth + 40));
                var height = NodeHeaderHeight + Math.Max(1, Math.Max(node.InputCount, node.OutputCount)) * PortHeight;
                nodeBounds[i] = new Rect(node.X, node.Y, width, height);
            }

            var drawing = new DrawingGroup();

            // Groups first so they sit behind nodes and wires.
            foreach (var group in preview.Groups)
            {
                if (group.Width <= 0 || group.Height <= 0) continue;
                var rect = new Rect(group.Left, group.Top, group.Width, group.Height);
                drawing.Children.Add(Frozen(new GeometryDrawing(ParseGroupBrush(group.Background), null, Frozen(new RectangleGeometry(rect)))));
                AddText(drawing, group.Title, new Point(rect.Left + 10, rect.Top + 6), GroupTitleFontSize, GroupTitleBrush, rect.Width - 20, pixelsPerDip);
            }

            if (preview.Connectors.Count > 0)
            {
                var wires = new StreamGeometry();
                using (var context = wires.Open())
                {
                    foreach (var connector in preview.Connectors)
                    {
                        var start = nodeBounds[connector.StartNode];
                        var end = nodeBounds[connector.EndNode];
                        var from = new Point(start.Right, start.Top + NodeHeaderHeight + (connector.StartPortIndex + 0.5) * PortHeight);
                        var to = new Point(end.Left, end.Top + NodeHeaderHeight + (connector.EndPortIndex + 0.5) * PortHeight);
                        var handle = Math.Max(40, Math.Abs(to.X - from.X) / 2);

                        context.BeginFigure(from, false, false);
                        context.BezierTo(new Point(from.X + handle, from.Y), new Point(to.X - handle, to.Y), to, true, false);
                    }
                }
                drawing.Children.Add(Frozen(new GeometryDrawing(null, WirePen, Frozen(wires))));
            }

            if (nodeBounds.Length > 0)
            {
                var bodies = new StreamGeometry();
                var headers = new StreamGeometry();
                using (var bodyContext = bodies.Open())
                using (var headerContext = headers.Open())
                {
                    foreach (var rect in nodeBounds)
                    {
                        AddRectangle(bodyContext, rect);
                        AddRectangle(headerContext, new Rect(rect.Left, rect.Top, rect.Width, NodeHeaderHeight));
                    }
                }
                drawing.Children.Add(Frozen(new GeometryDrawing(NodeFill, null, Frozen(bodies))));
                drawing.Children.Add(Frozen(new GeometryDrawing(NodeHeaderFill, null, Frozen(headers))));

                if (nodeBounds.Length <= MaxLabeledNodes)
                {
                    for (var i = 0; i < nodeBounds.Length; i++)
                    {
                        var rect = nodeBounds[i];
                        AddText(drawing, preview.Nodes[i].Name, new Point(rect.Left + 10, rect.Top + 13), LabelFontSize, LabelBrush, rect.Width - 20, pixelsPerDip);
                    }
                }
            }

            foreach (var note in preview.Notes)
            {
                AddText(drawing, note.Text, new Point(note.X, note.Y), LabelFontSize, NoteBrush, 400, pixelsPerDip);
            }

            if (drawing.Bounds.IsEmpty) return null;

            return Frozen(new DrawingImage(Frozen(drawing)));
        }

        private static void AddRectangle(StreamGeometryContext context, Rect rect)
        {
            context.BeginFigure(rect.TopLeft, true, true);
            context.PolyLineTo(new[] { rect.TopRight, rect.BottomRight, rect.BottomLeft }, false, false);
        }

        private static void AddText(DrawingGroup drawing, string text, Point origin, double fontSize, Brush brush, double maxWidth, double pixelsPerDip)
        {
            if (string.IsNullOrWhiteSpace(text) || maxWidth <= 0) return;

            var formatted = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, LabelTypeface, fontSize, brush, pixelsPerDip)
            {
                MaxTextWidth = maxWidth,
                MaxLineCount = 1,
                Trimming = TextTrimming.CharacterEllipsis
            };
            var geometry = formatted.BuildGeometry(origin);
            if (geometry.IsEmpty()) return;
            drawing.Children.Add(Frozen(new GeometryDrawing(brush, null, Frozen(geometry))));
        }

        private static Brush ParseGroupBrush(string color)
        {
            if (string.IsNullOrEmpty(color)) return DefaultGroupFill;
            try
            {
                var parsed = (Color)ColorConverter.ConvertFromString(color);
                parsed.A = 0x80;
                return Frozen(new SolidColorBrush(parsed));
            }
            catch (Exception ex) when (ex is FormatException || ex is NotSupportedException)
            {
                // Unknown color strings fall back to the default group color.
                return DefaultGroupFill;
            }
        }

        private static T Frozen<T>(T freezable) where T : Freezable
        {
            if (freezable.CanFreeze) freezable.Freeze();
            return freezable;
        }
    }
}
