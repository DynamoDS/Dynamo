using System;
using System.Linq;
using Dynamo.Models;
using Dynamo.Utilities;

using Point = System.Windows.Point;

namespace Dynamo.ViewModels
{
    public enum PreviewState{Selection, ExecutionPreview, None}

    public partial class ConnectorViewModel:ViewModelBase
    {

        #region Properties

        private readonly WorkspaceViewModel workspaceViewModel;
        private PortModel _activeStartPort;
        public PortModel ActiveStartPort { get { return _activeStartPort; } internal set { _activeStartPort = value; } }

        private ConnectorModel _model;

        public ConnectorModel ConnectorModel
        {
            get { return _model; }
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

        private bool _isHitTestVisible = true;
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
                    return _activeStartPort.Center.AsWindowsType();
                else if (_model.Start != null)
                    return _model.Start.Center.AsWindowsType();
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

        /// <summary>
        /// Returns visible if the connectors is in the current space and the 
        /// model's current connector type is BEZIER
        /// </summary>
        public bool BezVisibility
        {
            get
            {
                if (workspaceViewModel.DynamoViewModel.ConnectorType == ConnectorType.BEZIER &&
                    workspaceViewModel.DynamoViewModel.IsShowingConnectors)
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
                if (workspaceViewModel.DynamoViewModel.ConnectorType == ConnectorType.POLYLINE &&
                    workspaceViewModel.DynamoViewModel.IsShowingConnectors)
                    return true;
                return false;
            }
            set
            {
                RaisePropertyChanged("PlineVisibility");
            }
        }

        public NodeViewModel Nodevm
        {
            get
            {
                return workspaceViewModel.Nodes.FirstOrDefault(x => x.NodeLogic.GUID == _model.Start.Owner.GUID);
            }
        }

        public PreviewState PreviewState
        {
            get
            {               
                if (_model == null)
                {
                    return PreviewState.None;
                }
              
                if (Nodevm.ShowExecutionPreview)
                {                  
                    return PreviewState.ExecutionPreview;
                }

                if (_model.Start.Owner.IsSelected ||
                    _model.End.Owner.IsSelected)
                {
                    return PreviewState.Selection;
                }

                return PreviewState.None;
            }
        }
#endregion

        /// <summary>
        /// Construct a view and start drawing.
        /// </summary>
        /// <param name="port"></param>
        public ConnectorViewModel(WorkspaceViewModel workspace, PortModel port)
        {
            this.workspaceViewModel = workspace;
            IsConnecting = true;
            _activeStartPort = port;

            Redraw(port.Center);
        }

        /// <summary>
        /// Construct a view and respond to property changes on the model. 
        /// </summary>
        /// <param name="model"></param>
        public ConnectorViewModel(WorkspaceViewModel workspace, ConnectorModel model)
        {
            this.workspaceViewModel = workspace;
            _model = model;

            _model.PropertyChanged += Model_PropertyChanged;
            _model.Start.Owner.PropertyChanged += StartOwner_PropertyChanged;
            _model.End.Owner.PropertyChanged += EndOwner_PropertyChanged;

            workspaceViewModel.DynamoViewModel.PropertyChanged += DynamoViewModel_PropertyChanged;
            Nodevm.PropertyChanged += nodeViewModel_PropertyChanged;
            Redraw();
        }

        private void nodeViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ShowExecutionPreview":
                    RaisePropertyChanged("PreviewState");
                    break;
            }
        }

        /// <summary>
        /// If the start owner changes position or size, redraw the connector.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void StartOwner_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsSelected":
                    RaisePropertyChanged("PreviewState");
                    break;
                case "Position":
                    RaisePropertyChanged("CurvePoint0");
                    Redraw();
                    break;
                case "Width":
                    RaisePropertyChanged("CurvePoint0");
                    Redraw();
                    break;
                case "ShowExecutionPreview":
                    RaisePropertyChanged("PreviewState");
                    break;
            }
        }

        /// <summary>
        /// If the end owner changes position or size, redraw the connector.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EndOwner_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsSelected":
                    RaisePropertyChanged("PreviewState");
                    break;
                case "Position":
                    RaisePropertyChanged("CurvePoint0");
                    Redraw();
                    break;
                case "Width":
                    RaisePropertyChanged("CurvePoint0");
                    Redraw();
                    break;
                case "ShowExecutionPreview":
                    RaisePropertyChanged("PreviewState");
                    break;
            }
        }

        void DynamoViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ConnectorType":
                    if (workspaceViewModel.DynamoViewModel.ConnectorType == ConnectorType.BEZIER)
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

        /// <summary>
        ///     Recalculate the path points using the internal model.
        /// </summary>
        public void Redraw()
        {
            //Debug.WriteLine("Redrawing...");
            if (this.ConnectorModel.End != null)
                this.Redraw(this.ConnectorModel.End.Center);
        }

        /// <summary>
        /// Recalculate the connector's points given the end point
        /// </summary>
        /// <param name="p2">The position of the end point</param>
        public void Redraw(object parameter)
        {
            var p2 = new Point();

            if (parameter is Point)
            {
                p2 = (Point) parameter;
            } else if (parameter is Point2D)
            {
                p2 = ((Point2D)parameter).AsWindowsType();
            }

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

            //if connector is dragged from an input port
            if (ActiveStartPort != null && ActiveStartPort.PortType == PortType.Input)
            {
                CurvePoint1 = new Point(CurvePoint0.X - offset, CurvePoint1.Y); ;
                CurvePoint2 = new Point(p2.X + offset, p2.Y);
            }

            _dotTop = CurvePoint3.Y - EndDotSize / 2;
            _dotLeft = CurvePoint3.X - EndDotSize / 2;

            //Update all the bindings at once.
            //http://stackoverflow.com/questions/4651466/good-way-to-refresh-databinding-on-all-properties-of-a-viewmodel-when-model-chan
            //RaisePropertyChanged(string.Empty);
        }

        private bool CanRedraw(object parameter)
        {
            return true;
        }
    }
}
