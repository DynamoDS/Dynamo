using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Dynamo.Controls;
using Dynamo.Views;
using Dynamo.Utilities;
using Dynamo.UI;
using Dynamo.Graph.Connectors;
using Dynamo.ViewModels;

namespace Dynamo.Diagnostics
{
    class DiagnosticsAdorner : Adorner
    {
        private readonly ContentPresenter contentPresenter;
        private readonly AdornerLayer adornerLayer;
        private static DataTemplate diagnosticsSessionTemplate;
        private IEnumerable<ConnectorModel> contextData;

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
            adorner.contextData = session.EvaluatedNodes.SelectMany(n => n.Node.AllConnectors).Distinct();
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
        }

        public void Detach()
        {
            adornerLayer.Remove(this);
        }
    }
}
