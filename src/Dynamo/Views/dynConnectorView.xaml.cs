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
using Dynamo.Connectors;
using Dynamo.Utilities;

namespace Dynamo.Nodes.Views
{
    /// <summary>
    /// Interaction logic for dynConnectorView.xaml
    /// </summary>
    public partial class dynConnectorView : UserControl
    {
        
        //const double STROKE_OPACITY = .6;
        //const double DEFAULT_BEZ_OFFSET = 20;
        
        /*PathFigure connectorPoints;
        BezierSegment connectorCurve;
        PathFigure plineFigure;
        PolyLineSegment pline;*/
        
        //Ellipse endDot;

        public dynConnectorView()
        {
            InitializeComponent();

//MVVM : Create the paths in XAML and bind to model view
            /*
            #region bezier creation
            connector = new Path();
            connector.Stroke = strokeBrush;
            connector.StrokeThickness = STROKE_THICKNESS;
            connector.Opacity = STROKE_OPACITY;

            /*connector.DataContext = this;
            Binding strokeBinding = new Binding("StrokeBrush");
            connector.SetBinding(Path.StrokeProperty, strokeBinding);

            DoubleCollection dashArray = new DoubleCollection();
            dashArray.Add(5); dashArray.Add(2);
            connector.StrokeDashArray = dashArray;

            PathGeometry connectorGeometry = new PathGeometry();
            connectorPoints = new PathFigure();
            connectorCurve = new BezierSegment();

            connectorPoints.StartPoint = new Point(pStart.Center.X, pStart.Center.Y);
            connectorCurve.Point1 = connectorPoints.StartPoint;
            connectorCurve.Point2 = connectorPoints.StartPoint;
            connectorCurve.Point3 = connectorPoints.StartPoint;

            connectorPoints.Segments.Add(connectorCurve);
            connectorGeometry.Figures.Add(connectorPoints);
            connector.Data = connectorGeometry;
            dynSettings.Workbench.Children.Add(connector);
            #endregion

            #region polyline creation
            plineConnector = new Path();
            plineConnector.Stroke = strokeBrush;
            plineConnector.StrokeThickness = STROKE_THICKNESS;
            plineConnector.Opacity = STROKE_OPACITY;
            plineConnector.StrokeDashArray = dashArray;

            PathGeometry plineGeometry = new PathGeometry();
            //http://msdn.microsoft.com/en-us/library/system.windows.media.polylinesegment(v=vs.85).aspx
            plineFigure = new PathFigure();
            plineFigure.StartPoint = new Point(pStart.Center.X, pStart.Center.Y);
            Point[] polyLinePointArray = new Point[] { connectorPoints.StartPoint, 
                connectorPoints.StartPoint,
                connectorPoints.StartPoint};
            pline = new PolyLineSegment(polyLinePointArray, true);
            pline.Points = new PointCollection(polyLinePointArray);
            plineFigure.Segments.Add(pline);
            plineGeometry.Figures.Add(plineFigure);
            plineConnector.Data = plineGeometry;
            dynSettings.Workbench.Children.Add(plineConnector);
            #endregion

            endDot = new Ellipse();
            endDot.Height = 6;
            endDot.Width = 6;
            endDot.Fill = Brushes.Black;
            endDot.StrokeThickness = 2;
            endDot.Stroke = Brushes.Black;
            endDot.IsHitTestVisible = false;
            endDot.MouseDown += new System.Windows.Input.MouseButtonEventHandler(endDot_MouseDown);
            Canvas.SetTop(endDot, connectorCurve.Point3.Y - END_DOT_SIZE / 2);
            Canvas.SetLeft(endDot, connectorCurve.Point3.X - END_DOT_SIZE / 2);
            dynSettings.Workbench.Children.Add(endDot);
            endDot.Opacity = STROKE_OPACITY;
            endDot.IsHitTestVisible = false;
            endDot.MouseDown += new System.Windows.Input.MouseButtonEventHandler(endDot_MouseDown);

            this.Visible = visible;

            connector.MouseEnter += delegate { if (pEnd != null) Highlight(); };
            connector.MouseLeave += delegate { Unhighlight(); };
            plineConnector.MouseEnter += delegate { if (pEnd != null) Highlight(); };
            plineConnector.MouseLeave += delegate { Unhighlight(); };

            //set this to not draggable
            Dynamo.Controls.DragCanvas.SetCanBeDragged(this, false);
            Dynamo.Controls.DragCanvas.SetCanBeDragged(connector, false);

            //set the z order to the front
            //Canvas.SetZIndex(this, 300);
            Canvas.SetZIndex(this, 1);*/

            Dynamo.Controls.DragCanvas.SetCanBeDragged(this, false);
        }

        void endDot_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Highlight(object sender, MouseEventArgs e)
        {
            (DataContext as dynConnectorViewModel).HighlightCommand.Execute();
        }

        private void Unhighlight(object sender, MouseEventArgs e)
        {
            (DataContext as dynConnectorViewModel).UnHighlightCommand.Execute();
        }

        
    }
}
