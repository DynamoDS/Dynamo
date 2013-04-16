//Copyright 2013 Ian Keough

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;

using Dynamo.Nodes;
using Dynamo.Controls;
using Dynamo.Utilities;

namespace Dynamo.Connectors
{
    public enum ConnectorType { BEZIER, POLYLINE };

    public delegate void ConnectorConnectedHandler(object sender, EventArgs e);

    public class dynConnector : UIElement, INotifyPropertyChanged
    {
        public event ConnectorConnectedHandler Connected;

        protected virtual void OnConnected(EventArgs e)
        {
            if (Connected != null)
                Connected(this, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        const int STROKE_THICKNESS = 2;
        const double STROKE_OPACITY = .6;
        const double DEFAULT_BEZ_OFFSET = 20;
        const int END_DOT_SIZE = 6;
        double bezOffset = 20;

        dynPort pStart;
        dynPort pEnd;

        PathFigure connectorPoints;
        BezierSegment connectorCurve;
        PathFigure plineFigure;
        PolyLineSegment pline;
        ConnectorType connectorType;

        Ellipse endDot;
        
        Path connector;
        Path plineConnector;
        Brush strokeBrush;

        bool isDrawing = false;

        public bool IsDrawing
        {
            get { return isDrawing; }
        }
        
        public dynPort Start
        {
            get { return pStart; }
            set { pStart = value; }
        }
        
        public dynPort End
        {
            get { return pEnd; }
            set
            {
                pEnd = value;
            }
        }
        
        public ConnectorType ConnectorType
        {
            get { return connectorType; }
            set
            {
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
        }

        public Brush StrokeBrush
        {
            get { return strokeBrush; }
            set 
            { 
                strokeBrush = value;
                NotifyPropertyChanged("StrokeBrush");
            }
        }

        #region constructors
        public dynConnector(dynPort port, Canvas workBench, Point mousePt)
        {
            //don't allow connections to start at an input port
            if (port.PortType != PortType.INPUT)
            {
                //get start point
                //this.workBench = workBench;
                pStart = port;

                pStart.Connect(this);

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

                ConnectorType = dynSettings.Bench.ConnectorType;

            }
            else
            {
                throw new InvalidPortException();
            }

        }

        public dynConnector(dynNodeUI start, dynNodeUI end, int startIndex, int endIndex, int portType, bool visible)
        {
            //don't try to create a connector with a bad start,
            //end, or if we're trying to connector the same
            //port to itself.
            if (start == null || end == null || start == end)
            {
                throw new Exception("Attempting to create connector with invalid start or end nodes.");
            }

            pStart = start.OutPorts[startIndex];

            dynPort endPort = null;

            if (portType == 0)
                endPort = end.InPorts[endIndex];

            //connect the two ports
            //get start point

            pStart.Connect(this);

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
            endDot.MouseDown +=new System.Windows.Input.MouseButtonEventHandler(endDot_MouseDown);
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

            isDrawing = true;

            //set this to not draggable
            Dynamo.Controls.DragCanvas.SetCanBeDragged(this, false);
            Dynamo.Controls.DragCanvas.SetCanBeDragged(connector, false);

            //set the z order to the front
            //Canvas.SetZIndex(this, 300);
            Canvas.SetZIndex(this, 1);

            this.Connect(endPort);

            ConnectorType = dynSettings.Bench.ConnectorType;
        }

        public dynConnector(dynNodeUI start, dynNodeUI end, int startIndex, int endIndex, int portType)
            : this(start, end, startIndex, endIndex, portType, true)
        { }
        #endregion
        
        void endDot_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void Highlight()
        {
            if (connector != null)
            {
                connector.StrokeThickness = STROKE_THICKNESS * 3;
                plineConnector.StrokeThickness = STROKE_THICKNESS * 3;
            }
        }

        public void Unhighlight()
        {
            if (connector != null)
            {
                connector.StrokeThickness = STROKE_THICKNESS;
                plineConnector.StrokeThickness = STROKE_THICKNESS;
            }
        }

        public bool Connect(dynPort p)
        {
            //test if the port that you are connecting too is not the start port or the end port
            //of the current connector
            if (p.Equals(pStart) || p.Equals(pEnd))
            {
                return false;
            }

            //if the selected connector is also an output connector, return false
            //output ports can't be connected to eachother
            if (p.PortType == PortType.OUTPUT)
            {
                return false;
            }

            //test if the port that you are connecting to is an input and 
            //already has other connectors
            if (p.PortType == PortType.INPUT && p.Connectors.Count > 0)
            {
                return false;
            }

            //turn the line solid
            connector.StrokeDashArray.Clear();
            plineConnector.StrokeDashArray.Clear();
            pEnd = p;

            if (pEnd != null)
            {
                //set the start and end values to equal so this 
                //starts evaulating immediately
                //pEnd.Owner.InPortData[p.Index].Object = pStart.Owner.OutPortData.Object;
                p.Connect(this);
                pEnd.Update();
            }

            return true;
        }

        public bool Visible
        {
            get
            {
                return connector.Opacity > 0;
            }
            set
            {
                if (value)
                {
                    connector.Opacity = STROKE_OPACITY;
                    plineConnector.Opacity = STROKE_OPACITY;
                    endDot.Opacity = STROKE_OPACITY;
                }
                else
                {
                    connector.Opacity = 0;
                    plineConnector.Opacity = 0;
                    endDot.Opacity = 0;
                }
            }
        }

        public void Disconnect(dynPort p)
        {
            if (p.Equals(pStart))
            {
                pStart = null;
            }

            if (p.Equals(pEnd))
            {
                pEnd = null;
            }

            p.Disconnect(this);

            //turn the connector back to dashed
            connector.StrokeDashArray.Add(5);
            connector.StrokeDashArray.Add(2);

            plineConnector.StrokeDashArray.Add(5);
            plineConnector.StrokeDashArray.Add(2);

        }

        public void Kill()
        {
            if (pStart != null && pStart.Connectors.Contains(this))
            {
                pStart.Disconnect(this);
                //pStart.Connectors.Remove(this);
                //do not remove the owner's output element
            }
            if (pEnd != null && pEnd.Connectors.Contains(this))
            {
                pEnd.Disconnect(this);
                //remove the reference to the
                //dynElement attached to port A

                //if (pEnd.Index < pEnd.Owner.InPortData.Count)
                //{
                //   pEnd.Owner.InPortData[pEnd.Index].Object = null;
                //}
            }

            pStart = null;
            pEnd = null;

            dynSettings.Workbench.Children.Remove(connector);
            dynSettings.Workbench.Children.Remove(plineConnector);
            dynSettings.Workbench.Children.Remove(endDot);

            isDrawing = false;

            dynSettings.Bench.RemoveConnector(this);
        }

        public void Redraw(Point p2)
        {
            if (isDrawing)
            {
                if (pStart != null)
                {
                    connectorPoints.StartPoint = pStart.Center;
                    plineFigure.StartPoint = pStart.Center;

                    //calculate the bezier offset based on the distance
                    //between ports. if the distance is less than 2 * 100,
                    //make the offset 1/3 of the distance
                    double distance = 0.0;
                    if (connectorType == Connectors.ConnectorType.BEZIER)
                    {
                        distance = Math.Sqrt(Math.Pow(p2.X - pStart.Center.X, 2) + Math.Pow(p2.Y - pStart.Center.Y, 2));
                        bezOffset = .3 * distance;
                    }
                    else
                    {
                        distance = p2.X - pStart.Center.X;
                        bezOffset = distance / 2;
                    }


                    connectorCurve.Point1 = new Point(pStart.Center.X + bezOffset, pStart.Center.Y);
                    connectorCurve.Point2 = new Point(p2.X - bezOffset, p2.Y);
                    connectorCurve.Point3 = p2;

                    pline.Points[0] = new Point(pStart.Center.X + bezOffset, pStart.Center.Y);
                    pline.Points[1] = new Point(p2.X - bezOffset, p2.Y);
                    pline.Points[2] = p2;

                    Canvas.SetTop(endDot, connectorCurve.Point3.Y - END_DOT_SIZE / 2);
                    Canvas.SetLeft(endDot, connectorCurve.Point3.X - END_DOT_SIZE / 2);
                }

            }
        }

        public void Redraw()
        {
            if (pStart == null && pEnd == null)
                return;

            double distance = 0.0;
            if (connectorType == Connectors.ConnectorType.BEZIER)
            {
                distance = Math.Sqrt(Math.Pow(pEnd.Center.X - pStart.Center.X, 2) + Math.Pow(pEnd.Center.Y - pStart.Center.Y, 2));
                bezOffset = .3 * distance;
            }
            else
            {
                distance = pEnd.Center.X - pStart.Center.X;
                bezOffset = distance / 2;
            }


            //don't redraw with null end points;
            if (pStart != null)
            {
                connectorPoints.StartPoint = pStart.Center;
                plineFigure.StartPoint = pStart.Center;

                connectorCurve.Point1 = new Point(pStart.Center.X + bezOffset, pStart.Center.Y);
                pline.Points[0] = new Point(pStart.Center.X + bezOffset, pStart.Center.Y);
            }
            if (pEnd != null)
            {

                if (pEnd.PortType == PortType.INPUT)
                {
                    connectorCurve.Point2 = new Point(pEnd.Center.X - bezOffset, pEnd.Center.Y);
                    pline.Points[1] = new Point(pEnd.Center.X - bezOffset, pEnd.Center.Y);
                }
                connectorCurve.Point3 = pEnd.Center;
                pline.Points[2] = pEnd.Center;

                Canvas.SetTop(endDot, connectorCurve.Point3.Y - END_DOT_SIZE / 2);
                Canvas.SetLeft(endDot, connectorCurve.Point3.X - END_DOT_SIZE / 2);
            }
        }

    }

    public class InvalidPortException : ApplicationException
    {
        private string message;
        public override string Message
        {
            get { return message; }
        }

        public InvalidPortException()
        {
            message = "Connection port is not valid.";
        }
    }
}
