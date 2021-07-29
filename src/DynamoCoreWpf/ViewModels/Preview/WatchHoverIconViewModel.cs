using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Dynamo.Core;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.ViewModels;
using DSCore;
using CoreNodeModels;
using Dynamo.UI.Commands;

namespace Dynamo.ViewModels
{
    /// <summary>
    /// ViewModel of the 'watch icon', which gets displayed when
    /// a connector is hovered over.
    /// </summary>
    public class WatchHoverIconViewModel: NotificationObject
    {
        private ConnectorViewModel ViewModel { get; set; }
        private DynamoModel DynamoModel { get; set; }
        private Dispatcher Dispatcher { get; set; }
        /// <summary>
        /// The size of the Watch Icon (x & y dimensions).
        /// </summary>
        public double MarkerSize { get; set; } = 30;

        /// <summary>
        /// Midpoint of the connector bezier curve.
        /// </summary>
        public Point MidPoint { get; set; }

        private bool isHalftone;
        /// <summary>
        /// Property used to tell xaml which image to use for the watch icon,
        /// the normal or the greyed-out. This depends on whether the wire
        /// is visible or hidden.
        /// </summary>
        public bool IsHalftone 
        {
            get
            {
                return isHalftone;
            }
            set
            {
                isHalftone = value;
                RaisePropertyChanged(nameof(IsHalftone));
            }
        }

        #region Commands
        
        /// <summary>
        /// Command which places a watch node in the center of the connector.
        /// </summary>
        public DelegateCommand PlaceWatchNodeCommand { get; set; }

        private void PlaceWatchNodeCommandExecute(object param)
        {
            var pinLocations = ViewModel.CollectPinLocations();
            ViewModel.DiscardAllConnectorPinModels();
            PlaceWatchNode(pinLocations);
        }

        private void InitCommands()
        {
            PlaceWatchNodeCommand = new DelegateCommand(PlaceWatchNodeCommandExecute, x=> true);
        }
        #endregion

        public WatchHoverIconViewModel(ConnectorViewModel connectorViewModel, DynamoModel dynamoModel)
        {
            ViewModel = connectorViewModel;
            DynamoModel = dynamoModel;
            InitCommands();

            Dispatcher = Dispatcher.CurrentDispatcher;

            if (ViewModel.ConnectorPinViewCollection.Count == 0 && ViewModel.BezierControlPoints is null)
            {
                MidPoint = ConnectorBezierMidpoint(
                    new Point[]
                    {
                        ViewModel.CurvePoint0,
                        ViewModel.CurvePoint1,
                        ViewModel.CurvePoint2,
                        ViewModel.CurvePoint3
                    });
            }
            else
            {
                MidPoint = MultiBezierMidpoint();
            }

            connectorViewModel.PropertyChanged += OnConnectorViewModelPropertyChanged;
            IsHalftone = false;
        }

        private void OnConnectorViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!(e.PropertyName.Contains("CurvePoint") |
                  e.PropertyName == nameof(ConnectorViewModel.ConnectorPinViewCollection)))
                return;

            if (ViewModel.ConnectorPinViewCollection.Count > 0 && ViewModel.BezierControlPoints != null)
            {
                MidPoint = MultiBezierMidpoint();
            }
            else
            {
                MidPoint = ConnectorBezierMidpoint(
                    new Point[]
                    {
                            ViewModel.CurvePoint0,
                            ViewModel.CurvePoint1,
                            ViewModel.CurvePoint2,
                            ViewModel.CurvePoint3
                    });
            }

            RaisePropertyChanged(nameof(MidPoint));

        }

        private Point ConnectorBezierMidpoint(Point[] points)
        {
            // formula to get bezier curve midtpoint
            // https://stackoverflow.com/questions/5634460/quadratic-b%c3%a9zier-curve-calculate-points?rq=1
            var parameter = 0.5;
            var x = ((1 - parameter) * (1 - parameter) * (1 - parameter) *
                points[0].X + 3 * (1 - parameter) * (1 - parameter)
                * parameter *
                points[1].X + 3 * (1 - parameter)
                                            * parameter * parameter *
                                            points[2].X + parameter * parameter * parameter *
                points[3].X) - (MarkerSize / 2);

            var y = ((1 - parameter) * (1 - parameter) * (1 - parameter) *
                points[0].Y + 3 * (1 - parameter) * (1 - parameter)
                * parameter *
                points[1].Y + 3 * (1 - parameter)
                                * parameter * parameter *
                                points[2].Y + parameter * parameter * parameter *
                points[3].Y) - (MarkerSize / 2);

            return new Point(x, y);
        }

        /// <summary>
        /// Returns the 'midpoint' of the multi-segment bezier curve.
        /// </summary>
        /// <returns></returns>
        private Point MultiBezierMidpoint()
        {
            int bezierMiddleSegmentIndex = -1;
            if (ViewModel.ComputedBezierPathGeometry.Figures.Count % 2 == 0)
            {
                bezierMiddleSegmentIndex = (int) (ViewModel.ComputedBezierPathGeometry.Figures.Count / 2 - 1);
            }
            else
            {
                bezierMiddleSegmentIndex = (int)(ViewModel.ComputedBezierPathGeometry.Figures.Count / 2);
            }

            var segmentToCalculateMidpointOn = ViewModel.BezierControlPoints[bezierMiddleSegmentIndex];

            return ConnectorBezierMidpoint(segmentToCalculateMidpointOn);
        }

        /// <summary>
        /// Places watch node at the midpoint of the connector
        /// </summary>
        internal void PlaceWatchNode(IEnumerable<Point> connectorPinLocations)
        {
            NodeModel startNode = ViewModel.Model.Start.Owner;
            NodeModel endNode = ViewModel.Model.End.Owner;
            this.Dispatcher.Invoke(() =>
            {
                var watchNode = new Watch();
                var nodeX = MidPoint.X - (watchNode.Width / 2);
                var nodeY = MidPoint.Y - (watchNode.Height / 2);
                DynamoModel.ExecuteCommand(new DynamoModel.CreateNodeCommand(watchNode, nodeX, nodeY, false, false));
                WireNewNode(DynamoModel, startNode, endNode, watchNode, connectorPinLocations);
            });
        }

        /// <summary>
        /// Rewires nodes and newly placed watch node between them.
        /// </summary>
        /// <param name="dynamoModel"></param>
        /// <param name="startNode"></param>
        /// <param name="endNode"></param>
        /// <param name="watchNodeModel"></param>
        private void WireNewNode(DynamoModel dynamoModel, NodeModel startNode, NodeModel endNode, NodeModel watchNodeModel,
            IEnumerable<Point> connectorPinLocations)
        {
            (List<int> startIndex, List<int> endIndex) = GetPortIndex(startNode, endNode);

            // Connect startNode and watch node
            foreach (var idx in startIndex)
            {
                dynamoModel.ExecuteCommand(new DynamoModel.MakeConnectionCommand(startNode.GUID, idx, PortType.Output, DynamoModel.MakeConnectionCommand.Mode.Begin));
                dynamoModel.ExecuteCommand(new DynamoModel.MakeConnectionCommand(watchNodeModel.GUID, 0, PortType.Input, DynamoModel.MakeConnectionCommand.Mode.End));
            }

            // Connect watch node and endNode
            foreach (var idx in endIndex)
            {
                dynamoModel.ExecuteCommand(new DynamoModel.MakeConnectionCommand(watchNodeModel.GUID, 0, PortType.Output, DynamoModel.MakeConnectionCommand.Mode.Begin));
                dynamoModel.ExecuteCommand(new DynamoModel.MakeConnectionCommand(endNode.GUID, idx, PortType.Input, DynamoModel.MakeConnectionCommand.Mode.End));
            }

            PlacePinsOnWires(startNode, watchNodeModel, connectorPinLocations);
        }

        private void PlacePinsOnWires(NodeModel startNode, NodeModel watchNodeModel, IEnumerable<Point> connectorPinLocations)
        {
            ///Collect ports & connectors of newly connected nodes
            ///so that old pins (that need to remain) can be transferred over correctly.
            PortModel startNodePort = startNode.OutPorts[0];
            PortModel watchNodePort = watchNodeModel.OutPorts[0];
            Graph.Connectors.ConnectorModel[] connectors = new Graph.Connectors.ConnectorModel[2];
            connectors[0] = startNodePort.Connectors[0];
            connectors[1] = watchNodePort.Connectors[0];
            /// Place each pin where required on the newly connected connectors.
            foreach (var connectorPinLocation in connectorPinLocations)
            {
                int wireIndex = ConnectorSegmentIndex(MidPoint, connectorPinLocation);
                ViewModel.PinConnectorPlacementFromWatchNode(connectors, wireIndex, connectorPinLocation);
            }
        }

        private static (List<int> StartIndex, List<int> EndIndex) GetPortIndex(NodeModel startNode, NodeModel endNode)
        {
            var connectors = startNode.AllConnectors;
            var filter = connectors.Where(c => c.End.Owner.GUID == endNode.GUID);

            var startIndex = filter
                .Select(c => c.Start.Index)
                .Distinct()
                .ToList();

            var endIndex = filter
                .Select(c => c.End.Index)
                .Distinct()
                .ToList();

            return (startIndex, endIndex);
        }


        /// <summary>
        /// Can only be zero or one: either the connector between the start node and the newly placed watch node,
        /// or the connector between the watch node and the end node.
        /// </summary>
        /// <returns></returns>
        private int ConnectorSegmentIndex(Point midPoint, Point connectorLocation)
        {
            return midPoint.X > connectorLocation.X ? 0 : 1;
        }
    }
}
