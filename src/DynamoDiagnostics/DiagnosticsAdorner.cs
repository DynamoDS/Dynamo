using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Utilities;
using Dynamo.Views;

namespace Dynamo.Diagnostics
{
    class DiagnosticsAdorner : Adorner
    {
        private readonly ContentPresenter contentPresenter;
        private readonly AdornerLayer adornerLayer;
        private static DataTemplate diagnosticsSessionTemplate;
        private IEnumerable<NodeData> contextData;

        private static DataTemplate GetAdorenerTemplate(UserControl control)
        {
            if (diagnosticsSessionTemplate != null)
                return diagnosticsSessionTemplate;

            var resources = new ResourceDictionary() { Source = new Uri(@"pack://application:,,,/DynamoDiagnostics;component/DataTemplates.xaml") };
            diagnosticsSessionTemplate = (DataTemplate)resources["DiagnosticsSessionTemplate"];
            //var nodeTemplate = (DataTemplate)resources["NodeViewDataTemplate"];
            //diagnosticsSessionTemplate.Resources.Add("NodeViewDataTemplate", nodeTemplate);
            
            return diagnosticsSessionTemplate;
        }

        private DiagnosticsAdorner(UIElement adornedElement, AdornerLayer layer, object content, DataTemplate template)
            : base(adornedElement)
        {
            adornerLayer = layer;
            contentPresenter = new ContentPresenter
            {
                Content = content,
                ContentTemplate = template,
            };

            adornerLayer.Add(this);
        }

        public static DiagnosticsAdorner CreateAdorner(Window dynamoView, DiagnosticsSession session)
        {
            var workspaceView = dynamoView.ChildOfType<WorkspaceView>() as UserControl;
            if (workspaceView == null) return null;
            var adornedElement = workspaceView.FindName("WorkspaceElements");
            if (adornedElement == null) return null;
            var layer = AdornerLayer.GetAdornerLayer(adornedElement as Visual);
            var adorner = new DiagnosticsAdorner(adornedElement as UIElement, layer, session.EvaluatedNodes/*.Select(x=>x.Node)*/, GetAdorenerTemplate(workspaceView));
            workspaceView.LayoutUpdated += adorner.OnWorkspaceViewLayoutUpdated;
            adorner.contextData = session.EvaluatedNodes;//.SelectMany(n => n.Node.OutPorts).Distinct();
            return adorner;
        }

        private void OnWorkspaceViewLayoutUpdated(object sender, EventArgs e)
        {
            this.UpdateLayout();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            contentPresenter.Measure(constraint);
            return contentPresenter.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            contentPresenter.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return contentPresenter;
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            //foreach (var node in contextData)
            //{
            //    DrawOutputConnector(node, drawingContext);
            //}
        }

        private void DrawOutputConnector(NodeData node, DrawingContext drawingContext)
        {
            int i = 0;
            foreach (var port in node.Node.OutPorts)
            {
                foreach (var c in port.Connectors)
                {
                    DrawConnector(c, node.OutputPortsDataSize.ElementAt(i), drawingContext);
                }
                i++;
            }
        }

        private void DrawConnector(ConnectorModel c, int dataSize, DrawingContext drawingContext)
        {
            var p0 = new Point(c.Start.CenterX, c.Start.CenterY);
            var p3 = new Point(c.End.CenterX, c.End.CenterY);

            double distance = Math.Sqrt(Math.Pow(p3.X - p0.X, 2) + Math.Pow(p3.Y - p0.Y, 2));
            var offset = .45 * distance;
            
            var p1 = new Point(p0.X + offset, p0.Y);
            var p2 = new Point(p3.X - offset, p3.Y);

            var pf = new PathFigure() { StartPoint = p0 };
            pf.Segments.Add(new BezierSegment(p1, p2, p3, true));
            var geometry = new PathGeometry(new[] { pf });
            drawingContext.DrawGeometry(Brushes.LightPink, new Pen(Brushes.DeepPink, 5), geometry);
        }

        public void Detach()
        {
            adornerLayer.Remove(this);
        }
    }
}
