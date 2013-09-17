using System;
using System.Drawing;
using Dynamo.Models;
using Dynamo.Utilities;
using Point = System.Windows.Point;

namespace Dynamo.ViewModels
{
    public partial class ConnectorViewModel:ViewModelBase
    {

        #region Properties

        private PortModel _activeStartPort;
        public PortModel ActiveStartPort { get { return _activeStartPort; } internal set { _activeStartPort = value; } }

        private ConnectorModel _model;

        public ConnectorModel ConnectorModel
        {
            get { return _model; }
        }

        Brush _strokeBrush;
        public Brush StrokeBrush
        {
            get { return _strokeBrush; }
            set
            {
                _strokeBrush = value;
                RaisePropertyChanged("StrokeBrush");
            }
        }

        private bool _isConnecting = false;
        public bool IsConnecting
        {
            get { return _isConnecting; }
            set
            {
                _isConnecting = value;
                RaisePropertyChanged("IsConnecting");
            }
        }

        private bool _isHitTestVisible = false;
        public bool IsHitTestVisible
        {
            get { return _isHitTestVisible;  } 
            set { 
                _isHitTestVisible = value;
                RaisePropertyChanged("IsHitTestVisible");
            }
        }

        public double Left
        {
            get { return 0; }
        }

        public double Top
        {
            get { return 0; }
        }

        public double ZIndex
        {
            get { return 1; }
        }

        /// <summary>
        ///     The start point of the path pulled from the port's center
        /// </summary>
        public Point CurvePoint0
        {
            get
            {
                if (_model == null)
                    return _activeStartPort.Center;
                else if (_model.Start != null)
                    return _model.Start.Center;
                else
                    return new Point();
            }
        }

        
        private Point _curvePoint1;
        public Point CurvePoint1
        {
            get
            {
                return _curvePoint1;
            }
            set
            {
                _curvePoint1 = value;
                RaisePropertyChanged("CurvePoint1");
            }
        }

        private Point _curvePoint2;
        public Point CurvePoint2
        {
            get { return _curvePoint2; }
            set
            {
                _curvePoint2 = value;
                RaisePropertyChanged("CurvePoint2");
            }
        }

        private Point _curvePoint3;
        public Point CurvePoint3
        {
            get { return _curvePoint3; }
            set
            {
                _curvePoint3 = value;
                RaisePropertyChanged("CurvePoint3");
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

        public bool IsStartSelected
        {
            get
            {
                if (ConnectorModel!=null && ConnectorModel.Start.Owner != null)
                    return ConnectorModel.Start.Owner.IsSelected;
                else return false;
            }
        }

        public bool IsEndSelected
        {
            get
            {
                if(ConnectorModel!=null && ConnectorModel.End.Owner != null)
                    return ConnectorModel.End.Owner.IsSelected;
                return false;
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

        private double _strokeThickness = 3;
        public double StrokeThickness
        {
            get { return _strokeThickness; }
            set 
            {
                _strokeThickness = value;
                RaisePropertyChanged("StrokeThickness");
            }
        }

        /// <summary>
        /// Returns visible if the connectors is in the current space and the 
        /// model's current connector type is BEZIER
        /// </summary>
        public bool BezVisibility
        {
            get
            {
                if (dynSettings.Controller.ConnectorType == ConnectorType.BEZIER &&
                    dynSettings.Controller.IsShowingConnectors)
                    return true;
                return false;
            }
            set
            {
                RaisePropertyChanged("BezVisibility");
            }
        }

        /// <summary>
        /// Returns visible if the connectors is in the current space and the 
        /// model's current connector type is POLYLINE
        /// </summary>
        public bool PlineVisibility
        {
            get
            {
                if (dynSettings.Controller.ConnectorType == ConnectorType.POLYLINE && 
                    dynSettings.Controller.IsShowingConnectors)
                    return true;
                return false;
            }
            set
            {
                RaisePropertyChanged("PlineVisibility");
            }
        }

#endregion

        //construct a view and start drawing.
        public ConnectorViewModel(PortModel port)
        {
            //var bc = new BrushConverter();
            //StrokeBrush = (Brush)bc.ConvertFrom("#777");
            const string colour = "#777";
            Color c = Color.FromArgb(
            Convert.ToByte(colour.Substring(1,1),16),
            Convert.ToByte(colour.Substring(2,1),16),
            Convert.ToByte(colour.Substring(3,1),16));
            StrokeBrush = new SolidBrush(c);

            IsConnecting = true;
            _activeStartPort = port;

            // makes sure that all of the positions on the curve path are
            // set
            this.Redraw(port.Center);

        }

        public ConnectorViewModel(ConnectorModel model)
        {
            //var bc = new BrushConverter();
            //StrokeBrush = (Brush)bc.ConvertFrom("#777");
            const string colour = "#777";
            Color c = Color.FromArgb(
            Convert.ToByte(colour.Substring(1, 1), 16),
            Convert.ToByte(colour.Substring(2, 1), 16),
            Convert.ToByte(colour.Substring(3, 1), 16));
            StrokeBrush = new SolidBrush(c);

            _model = model;
            
            _model.Start.PropertyChanged += Start_PropertyChanged;
            _model.End.PropertyChanged += End_PropertyChanged;
            _model.PropertyChanged += Model_PropertyChanged;
            _model.Start.Owner.PropertyChanged += StartOwner_PropertyChanged;
            _model.End.Owner.PropertyChanged += EndOwner_PropertyChanged;

            dynSettings.Controller.DynamoViewModel.PropertyChanged += DynamoViewModel_PropertyChanged;

            //make sure we have valid curve points
            Redraw();
        }

        void StartOwner_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsSelected":
                    RaisePropertyChanged("IsStartSelected");
                    break;
            }
        }

        void EndOwner_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsSelected":
                    RaisePropertyChanged("IsEndSelected");
                    break;
            }
        }

        void ModelConnected(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void DynamoViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ConnectorType":
                    if (dynSettings.Controller.ConnectorType == ConnectorType.BEZIER)
                    {
                        BezVisibility = true;
                        PlineVisibility = false;
                    }
                    else
                    {
                        BezVisibility = false;
                        PlineVisibility = true;
                    }
                    Redraw();
                    break;
                case "IsShowingConnectors":
                    RaisePropertyChanged("BezVisibility");
                    RaisePropertyChanged("PlineVisibility");
                break;
            }
        }

        void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentWorkspace":
                    RaisePropertyChanged("BezVisibility");
                    RaisePropertyChanged("PlineVisibility");
                    break;
            }
        }

        void End_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Center":
                    Redraw();
                    break;
            }
        }

        void Start_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Center":
                    Redraw();
                    RaisePropertyChanged("CurvePoint0");
                    break;
            }
        }

        /// <summary>
        ///     Recalculate the path points using the internal model.
        /// </summary>
        public void Redraw()
        {
            if (this.ConnectorModel.End != null)
                this.Redraw(this.ConnectorModel.End.Center);
        }

        /// <summary>
        ///     Recalculate the connector's points given the end point
        /// </summary>
        /// <param name="p2">The position of the end point</param>
        public void Redraw(object parameter)
        {
            Point p2 = (Point)parameter;

            //Debug.WriteLine("Redrawing...");

            CurvePoint3 = p2;

            var offset = 0.0;
            double distance = 0;
            if ( this.BezVisibility == true)
            {
                distance = Math.Sqrt(Math.Pow(CurvePoint3.X - CurvePoint0.X, 2) + Math.Pow(CurvePoint3.Y - CurvePoint0.Y, 2));
                offset = .45 * distance;
            }
            else
            {
                distance = CurvePoint3.X - CurvePoint0.X;
                offset = distance / 2;
            }

            CurvePoint1 = new Point(CurvePoint0.X + offset, CurvePoint0.Y);
            CurvePoint2 = new Point(p2.X - offset, p2.Y);

            DotTop = CurvePoint3.Y - EndDotSize / 2;
            DotLeft = CurvePoint3.X - EndDotSize / 2;

        }

        private bool CanRedraw(object parameter)
        {
            return true;
        }

        private void Highlight(object parameter)
        {
            StrokeThickness = HighlightThickness;
        }

        private bool CanHighlight(object parameter)
        {
            return true;
        }

        private void Unhighlight(object parameter)
        {
            StrokeThickness = _strokeThickness;
        }

        private bool CanUnHighlight(object parameter)
        {
            return true;
        }
    }
}
