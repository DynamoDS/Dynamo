using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using Dynamo.Utilities;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Commands;
using Brush = System.Windows.Media.Brush;

namespace Dynamo.Connectors
{
    public class dynConnectorViewModel:dynViewModelBase
    {
        private const double stroke_thickness = 2;
        private const double highlight_thickness = 6;

        private Point bez_point1;
        private Point bez_point2;
        private Point bez_point3;
        private double dot_top;
        private double dot_left;
        double bezOffset = 20;
        double end_dot_size = 6;
        private dynPortModel start;
        private dynPortModel end;
        private dynConnector _connector;
        bool isDrawing = false;
        ConnectorType connectorType;
        Brush strokeBrush;

        public DelegateCommand<object> ConnectCommand { get; set; }
        public DelegateCommand RedrawCommand { get; set; }
        public DelegateCommand HighlightCommand { get; set; }
        public DelegateCommand UnHighlightCommand { get; set; }

        public dynConnector ConnectorModel
        {
            get { return _connector; }
        }

        public bool IsDrawing
        {
            get { return isDrawing; }
        }

        public Brush StrokeBrush
        {
            get { return strokeBrush; }
            set
            {
                strokeBrush = value;
                RaisePropertyChanged("StrokeBrush");
            }
        }

        /// <summary>
        /// The start point of the path pulled from the port's center
        /// </summary>
        public Point Bez_StartPoint
        {
            get { return _connector.Start.Center; }
        }

        public Point Bez_Point1
        {
            get { return bez_point1; }
        }

        public Point Bez_Point2
        {
            get { return bez_point2; }
        }

        public Point Bez_Point3
        {
            get { return bez_point3; }
        }

        public double Dot_Top
        {
            get { return dot_top; }
        }

        public double Dot_Left
        {
            get { return dot_left; }
        }

        public double EndDotSize
        {
            get { return end_dot_size; }
        }

        public double StrokeThickness
        {
            get { return strokeThickness; }
            set 
            { 
                strokeThickness = value;
                RaisePropertyChanged("StrokeThickness");
            }
        }

        /*public ConnectorType ConnectorType
        {
            get { return connectorType; }
            set
            {
#warning MVVM : Use a binding to set the connector type
                if (value == Connectors.ConnectorType.BEZIER)
                {
                    //hide the polyline
                    plineConnector.Visibility = System.Windows.Visibility.Hidden;
                    //show the bez
                    connector.Visibility = System.Windows.Visibility.Visible;
                }
                else if (value == Connectors.ConnectorType.POLYLINE)
                {
                    //show the polyline
                    plineConnector.Visibility = System.Windows.Visibility.Visible;
                    //hide the bez
                    connector.Visibility = System.Windows.Visibility.Hidden;
                }

                connectorType = value;
            }
        }*/

        /// <summary>
        /// Returns visible if the connectors is in the current space and the 
        /// model's current connector type is BEZIER
        /// </summary>
        public Visibility BezVisibility
        {
            get
            {
                if (dynSettings.Controller.DynamoViewModel.Model.CurrentSpace.Connectors.Contains(_connector)
                    && dynSettings.Controller.DynamoViewModel.ConnectorType == ConnectorType.BEZIER)
                    return Visibility.Visible;
                return Visibility.Hidden;
            }
        }

        /// <summary>
        /// Returns visible if the connectors is in the current space and the 
        /// model's current connector type is POLYLINE
        /// </summary>
        public Visibility PlineVisibility
        {
            get
            {
                if (dynSettings.Controller.DynamoViewModel.Model.CurrentSpace.Connectors.Contains(_connector)
                    && dynSettings.Controller.DynamoViewModel.ConnectorType == ConnectorType.POLYLINE)
                    return Visibility.Visible;
                return Visibility.Hidden;
            }
        }

        //construct a view and start drawing.
        public dynConnectorViewModel(dynPortModel port)
        {
            ConnectCommand = new DelegateCommand<object>(Connect, CanConnect);
            RedrawCommand = new DelegateCommand(Redraw, CanRedraw);
            HighlightCommand = new DelegateCommand(Highlight, CanHighlight);
            UnHighlightCommand = new DelegateCommand(Unhighlight, CanUnHighlight);

            BrushConverter bc = new BrushConverter();
            strokeBrush = (Brush)bc.ConvertFrom("#313131");

            //don't allow connections to start at an input port
            if (port.PortType != PortType.INPUT)
            {
                start = port;

                //add ourself to the collection of view models on the workspace view model
                dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.Connectors.Add(this);
                isDrawing = true;

                #region old non MVVM
                //get start point
                //this.workBench = workBench;
                //pStart = port;

                //pStart.Connect(this);

#warning MVVM : Create the paths in XAML and bind
                /*
                BrushConverter bc = new BrushConverter();
                strokeBrush = (Brush)bc.ConvertFrom("#313131");

                #region bezier creation
                connector = new Path();
                connector.Stroke = strokeBrush;
                connector.StrokeThickness = STROKE_THICKNESS;
                connector.Opacity = STROKE_OPACITY;

                connector.DataContext = this;
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
                workBench.Children.Add(connector);
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
                plineFigure.StartPoint = connectorPoints.StartPoint;
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

                connector.MouseEnter += delegate { if (pEnd != null) Highlight(); };
                connector.MouseLeave += delegate { Unhighlight(); };
                plineConnector.MouseEnter += delegate { if (pEnd != null) Highlight(); };
                plineConnector.MouseLeave += delegate { Unhighlight(); };

                isDrawing = true;

                //set this to not draggable
                Dynamo.Controls.DragCanvas.SetCanBeDragged(this, false);
                Dynamo.Controls.DragCanvas.SetCanBeDragged(connector, false);

                //set the z order to the front
                Canvas.SetZIndex(connector, 0);
                Canvas.SetZIndex(endDot, 1);

                ConnectorType = dynSettings.Bench.ConnectorType;*/
                #endregion
            }
            else
            {
                throw new InvalidPortException();
            }

        }
        
        void End_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Center")
                Redraw();
        }

        void Start_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Center")
                Redraw();
        }

        void _connector_Connected(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Connect(object parameters)
        {
            //make the connector model
            dynPortModel end = parameters as dynPortModel;

            _connector = new dynConnector(start.Owner, end.Owner, start.Index, end.Index, 0);
            _connector.Connected += _connector_Connected;

            _connector.Start.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Start_PropertyChanged);
            _connector.End.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(End_PropertyChanged);
            dynSettings.Controller.DynamoViewModel.Model.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Model_PropertyChanged);
        }

        void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentSpace":
                    RaisePropertyChanged("BezVisibility");
                    RaisePropertyChanged("PlineVisibility");
                    break;
            }
        }

        private bool CanConnect(object parameters)
        {
            if ((parameters as dynPort) == null)
                return false;

            return true;
        }

        //public void Redraw(Point p2)
        /// <summary>
        /// Recalculate the connector's points when a node is moved.
        /// </summary>
        public void Redraw()
        {
#warning MVVM : Remove condition on isDrawing
            //if (isDrawing)
            //{
                if (_connector.Start != null)
                {
                    /*connectorPoints.StartPoint = pStart.Center;
                    plineFigure.StartPoint = pStart.Center;*/

                    //calculate the bezier offset based on the distance
                    //between ports. if the distance is less than 2 * 100,
                    //make the offset 1/3 of the distance
                    /*double distance = 0.0;
                    if (connectorType == Connectors.ConnectorType.BEZIER)
                    {
                        distance = Math.Sqrt(Math.Pow(p2.X - pStart.Center.X, 2) + Math.Pow(p2.Y - pStart.Center.Y, 2));
                        bezOffset = .3 * distance;
                    }
                    else
                    {
                        distance = p2.X - pStart.Center.X;
                        bezOffset = distance / 2;
                    }*/

                    /*connectorCurve.Point1 = new Point(pStart.Center.X + bezOffset, pStart.Center.Y);
                    connectorCurve.Point2 = new Point(p2.X - bezOffset, p2.Y);
                    connectorCurve.Point3 = p2;

                    pline.Points[0] = new Point(pStart.Center.X + bezOffset, pStart.Center.Y);
                    pline.Points[1] = new Point(p2.X - bezOffset, p2.Y);
                    pline.Points[2] = p2;

                    Canvas.SetTop(endDot, connectorCurve.Point3.Y - END_DOT_SIZE / 2);
                    Canvas.SetLeft(endDot, connectorCurve.Point3.X - END_DOT_SIZE / 2);*/


                    double distance = 0.0;
                    if (connectorType == Connectors.ConnectorType.BEZIER)
                    {
                        distance = Math.Sqrt(Math.Pow(_connector.End.Center.X - _connector.Start.Center.X, 2) + Math.Pow(_connector.End.Center.Y - _connector.Start.Center.Y, 2));
                        bezOffset = .3 * distance;
                    }
                    else
                    {
                        distance = _connector.End.Center.X - _connector.Start.Center.X;
                        bezOffset = distance / 2;
                    }

                    bez_point1 = new Point(_connector.Start.Center.X + bezOffset, _connector.Start.Center.Y);
                    bez_point2 = new Point(_connector.End.Center.X - bezOffset, _connector.End.Center.Y);
                    bez_point3 = _connector.End.Center;

                    dot_top = bez_point3.Y - end_dot_size / 2;
                    dot_left = bez_point3.X - end_dot_size/2;
                }

            //}
        }

        /// <summary>
        /// Recalculate the connector's points during a drag
        /// </summary>
        /// <param name="p2"></param>
        public void Redraw(Point p2 )
        {
            if (_connector.Start != null)
            {
                double distance = 0.0;
                if (connectorType == Connectors.ConnectorType.BEZIER)
                {
                    distance = Math.Sqrt(Math.Pow(p2.X - _connector.Start.Center.X, 2) + Math.Pow(p2.Y - _connector.Start.Center.Y, 2));
                    bezOffset = .3 * distance;
                }
                else
                {
                    distance = p2.X - _connector.Start.Center.X;
                    bezOffset = distance / 2;
                }

                bez_point1 = new Point(_connector.Start.Center.X + bezOffset, _connector.Start.Center.Y);
                bez_point2 = new Point(p2.X - bezOffset, _connector.End.Center.Y);
                bez_point3 = p2;

                dot_top = bez_point3.Y - end_dot_size / 2;
                dot_left = bez_point3.X - end_dot_size / 2;
            }
        }

        private bool CanRedraw()
        {
            return true;
        }

        private void Highlight()
        {
            StrokeThickness = highlight_thickness;
        }

        private bool CanHighlight()
        {
            return true;
        }

        private void Unhighlight()
        {
            StrokeThickness = stroke_thickness;
        }

        private bool CanUnHighlight()
        {
            return true;
        }
    }
}
