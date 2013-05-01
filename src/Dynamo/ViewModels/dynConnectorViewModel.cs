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


        #region Properties

        double bezOffset = 20;
        
        private dynPortModel start;
        private dynPortModel end;
        private dynConnectorModel _model;
        bool isDrawing = false;
        ConnectorType connectorType;
        Brush strokeBrush;

        public double Left
        {
            get { return 0;  }
        }

        public double Top
        {
            get { return 0; }
        }


        public DelegateCommand<object> ConnectCommand { get; set; }
        public DelegateCommand RedrawCommand { get; set; }
        public DelegateCommand HighlightCommand { get; set; }
        public DelegateCommand UnHighlightCommand { get; set; }

        public dynConnectorModel ConnectorModel
        {
            get { return _model; }
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
        public Point BezStartPoint
        {
            get
            {
                if (_model == null)
                    return start.Center;
                return _model.Start.Center;
            }
        }

        
        private Point _bezPoint1;
        public Point BezPoint1
        {
            get
            {
                return _bezPoint1;
            }
            set
            {
                _bezPoint1 = value;
                RaisePropertyChanged("BezPoint1");
            }
        }

        private Point _bezPoint2;
        public Point BezPoint2
        {
            get { return _bezPoint2; }
            set
            {
                _bezPoint2 = value;
                RaisePropertyChanged("BezPoint2");
            }
        }

        private Point _bezPoint3;
        public Point BezPoint3
        {
            get { return _bezPoint3; }
            set
            {
                _bezPoint3 = value;
                RaisePropertyChanged("BezPoint3");
            }
        }

        private double _dotTop;
        public double DotTop
        {
            get { return _dotTop; }
            set
            {
                _dotTop = value;
                RaisePropertyChanged("DotTop");
            }
        }

        private double _dotLeft;
        public double DotLeft
        {
            get { return _dotLeft; }
            set
            {
                _dotLeft = value;
                RaisePropertyChanged("DotLeft");
            }
        }

        private double _endDotSize = 6;
        public double EndDotSize
        {
            get { return _endDotSize; }
            set
            {
                _endDotSize = value;
                RaisePropertyChanged("EndDotSize");
            }
        }
        
        private const double HighlightThickness = 6;

        private double _strokeThickness = 2;
        public double StrokeThickness
        {
            get { return _strokeThickness; }
            set 
            {
                _strokeThickness = value;
                RaisePropertyChanged("StrokeThickness");
            }
        }

        /*public ConnectorType ConnectorType
        {
            get { return connectorType; }
            set
            {
//MVVM : Use a binding to set the connector type
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
        private Visibility _bezVisibility = Visibility.Visible;
        public Visibility BezVisibility
        {
            get
            {
                //if (dynSettings.Controller.DynamoViewModel.Model.CurrentSpace.Connectors.Contains(_model)
                //    && dynSettings.Controller.DynamoViewModel.ConnectorType == ConnectorType.BEZIER)
                return _bezVisibility;
                //return Visibility.Hidden;
            }
            set
            {
                _bezVisibility = value;
                RaisePropertyChanged("BezVisibility");
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
                if (dynSettings.Controller.DynamoViewModel.Model.CurrentSpace.Connectors.Contains(_model)
                    && dynSettings.Controller.DynamoViewModel.ConnectorType == ConnectorType.POLYLINE)
                    return Visibility.Visible;
                return Visibility.Hidden;
            }
        }


#endregion

        //construct a view and start drawing.
        public dynConnectorViewModel(dynPortModel port)
        {
            ConnectCommand = new DelegateCommand<object>(Connect, CanConnect);
            RedrawCommand = new DelegateCommand(Redraw, CanRedraw);
            HighlightCommand = new DelegateCommand(Highlight, CanHighlight);
            UnHighlightCommand = new DelegateCommand(Unhighlight, CanUnHighlight);

            var bc = new BrushConverter();
            strokeBrush = (Brush)bc.ConvertFrom("#313131");

            start = port;

            // shouldn't be doing this kind of check in a constructor

            ////don't allow connections to start at an input port
            //if (port.PortType != PortType.INPUT)
            //{
            //    start = port;

            //    //add ourself to the collection of view models on the workspace view model
            //    dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.Connectors.Add(this);
            //    isDrawing = true;

            //}
            //else
            //{
            //    throw new InvalidPortException();
            //}

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

        void ModelConnected(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Connect(object parameters)
        {
            //make the connector model
            dynPortModel end = parameters as dynPortModel;

            _model = new dynConnectorModel(start.Owner, end.Owner, start.Index, end.Index, 0);
            _model.Connected += ModelConnected;

            _model.Start.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Start_PropertyChanged);
            _model.End.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(End_PropertyChanged);
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
            if ((parameters as dynPortModel) == null)
                return false;

            return true;
        }

        /// <summary>
        /// Recalculate the connector's points when a node is moved.
        /// </summary>
        public void Redraw()
        {
            double distance = 0.0;
            if (connectorType == Connectors.ConnectorType.BEZIER)
            {
                distance = Math.Sqrt(Math.Pow(_model.End.Center.X - BezStartPoint.X, 2) + Math.Pow(_model.End.Center.Y - BezStartPoint.Y, 2));
                bezOffset = .3 * distance;
            }
            else
            {
                distance = _model.End.Center.X - BezStartPoint.X;
                bezOffset = distance / 2;
            }

            BezPoint1 = new Point(_model.Start.Center.X + bezOffset, _model.Start.Center.Y);
            BezPoint2 = new Point(_model.End.Center.X - bezOffset, _model.End.Center.Y);
            BezPoint3 = _model.End.Center;

            DotTop = _bezPoint3.Y - _endDotSize / 2;
            DotLeft = _bezPoint3.X - _endDotSize / 2;
        }

        /// <summary>
        /// Recalculate the connector's points during a drag
        /// </summary>
        /// <param name="p2"></param>
        public void Redraw(Point p2 )
        {

            double distance = p2.X - BezStartPoint.X;
            if (connectorType == Connectors.ConnectorType.BEZIER)
            {
                bezOffset = .3 * distance;
            }
            else
            {
                bezOffset = distance / 2;
            }

            BezPoint1 = new Point(BezStartPoint.X + bezOffset, BezStartPoint.Y);
            BezPoint2 = new Point(p2.X - bezOffset, p2.Y);
            BezPoint3 = p2;

            DotTop= _bezPoint3.Y - _endDotSize / 2;
            DotLeft = _bezPoint3.X - _endDotSize / 2;

        }

        private bool CanRedraw()
        {
            return true;
        }

        private void Highlight()
        {
            StrokeThickness = HighlightThickness;
        }

        private bool CanHighlight()
        {
            return true;
        }

        private void Unhighlight()
        {
            StrokeThickness = _strokeThickness;
        }

        private bool CanUnHighlight()
        {
            return true;
        }
    }
}
