﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Dynamo.Core;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using CoreNodeModels;
using Dynamo.UI.Commands;
using System;

namespace Dynamo.ViewModels
{
    public class ConnectorAnchorViewModel: NotificationObject
    {
        #region Properties 
        private Point currentPosition;
        private bool isHalftone;
        private bool isPartlyVisible = false;
        private bool isDataFlowCollection;
        private bool canDisplayIcons = false;
        private bool canShowTooltip = true;

        private bool watchIconPreviewOn = false;
        private bool pinIconPreviewOn = false;
        private string dataTooltipText;

        private ConnectorViewModel ViewModel { get; set; }
        private DynamoModel DynamoModel { get; set; }
        private Dispatcher Dispatcher { get; set; }

        /// <summary>
        /// The size of the Watch Icon (x & y dimensions).
        /// </summary>
        public double MarkerSize { get; set; } = 30;
        public double AnchorSize { get; set; } = 15;

        /// <summary>
        /// Midpoint of the connector bezier curve.
        /// </summary>
        public Point CurrentPosition
        {
            get
            {
                return currentPosition;
            }
            set
            {
                currentPosition = value;
                RaisePropertyChanged(nameof(CurrentPosition));
            }
        }

        /// <summary>
        /// Follows the 'isHalftone' visibility of the connector.
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

        /// <summary>
        /// Property which overrides 'isVisible==false' condition. When this prop is set to true, wires are set to 
        /// 40% opacity.
        /// </summary>
        public bool IsPartlyVisible
        {
            get { return isPartlyVisible; }
            set
            {
                isPartlyVisible = value;
                RaisePropertyChanged(nameof(IsPartlyVisible));
            }
        }

        /// <summary>
        /// Is the mouse over one of pin icon? Flag to switch color(binding) of pin button.
        /// </summary>
        public bool PinIconPreviewOn
        {
            get
            {
                return pinIconPreviewOn;
            }
            set
            {
                pinIconPreviewOn = value;
                RaisePropertyChanged(nameof(PinIconPreviewOn));
            }
        }
        ///Is the mouse over one of watch icon? Flag to switch color(binding) of watch button.
        public bool WatchIconPreviewOn
        {
            get
            {
                return watchIconPreviewOn;
            }
            set
            {
                watchIconPreviewOn = value;
                RaisePropertyChanged(nameof(WatchIconPreviewOn));
            }
        }

        internal void SwitchWatchPreviewOn()
        {
            WatchIconPreviewOn = true;
        }

        internal void SwitchWatchPreviewOff()
        {
            WatchIconPreviewOn = false;
        }

        public bool IsDataFlowCollection
        {
            get { return isDataFlowCollection; }
            set
            {
                isDataFlowCollection = value;
                RaisePropertyChanged(nameof(IsDataFlowCollection));
            }
        }

        internal void SwitchPinPreviewOn()
        {
            PinIconPreviewOn = true;
        }

        internal void SwitchPinPreviewOff()
        {
            PinIconPreviewOn = false;
        }

        /// <summary>
        /// This flag's final destination, (dynamomodel -> dynamoviewmodel -> connectorviewmodel-> connectoranchorviewmodel)
        /// where it tells the view whether or not it can
        /// show the tooltip.
        /// </summary>
        public bool CanShowTooltip
        {
            get
            {
                return canShowTooltip;
            }
            set
            {
                canShowTooltip = value;
                RaisePropertyChanged(nameof(CanShowTooltip));
            }
        }

        /// <summary>
        /// This property acts as the main flag that signals whether or not watch icon/pin icon/tooltip
        /// should be visibile/ hidden.
        /// </summary>
        public bool CanDisplayIcons
        {
            get { return canDisplayIcons; }
            set
            {
                canDisplayIcons = value;
                RaisePropertyChanged(nameof(CanDisplayIcons));
            }
        }
        /// <summary>
        /// This property holds the string representation of the data 
        /// being passed through this connector. It gets relayed from the 
        /// ConnectorViewModel.
        /// </summary>
        public string DataToolTipText
        {
            get
            {
                return dataTooltipText;
            }
            set
            {
                dataTooltipText = value;
                RaisePropertyChanged(nameof(DataToolTipText));
            }
        }
        #endregion

        /// <summary>
        /// Raises a 'redraw' event for this ConnectorPinViewModel
        /// </summary>
        public event EventHandler RequestDispose;
        public virtual void OnRequestDispose(Object sender, EventArgs e)
        {
            RequestDispose(this, e);
        }

        #region Commands

        /// <summary>
        /// Command which places a watch node in the center of the connector.
        /// </summary>
        public DelegateCommand PlaceWatchNodeCommand { get; set; }
        /// <summary>
        /// Delegate command to run when 'Pin Wire' item is clicked on this connector ContextMenu.
        /// </summary>
        public DelegateCommand PinConnectorCommand { get; set; }

        private void PlaceWatchNodeCommandExecute(object param)
        {
            var pinLocations = ViewModel.CollectPinLocations();
            ViewModel.DiscardAllConnectorPinModels();
            PlaceWatchNode(pinLocations);
        }

        /// <summary>
        /// Places pin at the location of mouse (over a connector)
        /// </summary>
        /// <param name="parameters"></param>
        private void PinConnectorCommandExecute(object parameters)
        {
            ViewModel.PinConnectorCommand.Execute(null);
        }

        private void InitCommands()
        {
            PlaceWatchNodeCommand = new DelegateCommand(PlaceWatchNodeCommandExecute, x=> true);
            PinConnectorCommand = new DelegateCommand(PinConnectorCommandExecute, x => true);
        }
        #endregion

        internal void DisposeViewModel()
        {
            OnRequestDispose(this, EventArgs.Empty);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectorViewModel"></param>
        /// <param name="dynamoModel"></param>
        /// <param name="tooltipText"></param>
        public ConnectorAnchorViewModel(ConnectorViewModel connectorViewModel, DynamoModel dynamoModel, string tooltipText)
        {
            ViewModel = connectorViewModel;
            DynamoModel = dynamoModel;
            DataToolTipText = tooltipText;
            InitCommands();

            Dispatcher = Dispatcher.CurrentDispatcher;

            IsHalftone = false;
            connectorViewModel.PropertyChanged += OnConnectorViewModelPropertyChanged;
        }

        /// <summary>
        /// Dispose function
        /// </summary>
        public void Dispose()
        {
            ViewModel.PropertyChanged -= OnConnectorViewModelPropertyChanged;
        }

        private void OnConnectorViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!(e.PropertyName.Contains("CurvePoint") |
                  e.PropertyName == nameof(ConnectorViewModel.ConnectorPinViewCollection)))
                return;
        }

        /// <summary>
        /// Places watch node at the midpoint of the connector
        /// </summary>
        internal void PlaceWatchNode(IEnumerable<Point> connectorPinLocations)
        {
            NodeModel startNode = ViewModel.ConnectorModel.Start.Owner;
            NodeModel endNode = ViewModel.ConnectorModel.End.Owner;
            this.Dispatcher.Invoke(() =>
            {
                var watchNode = new Watch();
                var nodeX = CurrentPosition.X - (watchNode.Width / 2);
                var nodeY = CurrentPosition.Y - (watchNode.Height / 2);
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
            // Collect ports & connectors of newly connected nodes
            // so that old pins (that need to remain) can be transferred over correctly.
            PortModel startNodePort = startNode.OutPorts[0];
            PortModel watchNodePort = watchNodeModel.OutPorts[0];
            Graph.Connectors.ConnectorModel[] connectors = new Graph.Connectors.ConnectorModel[2];
            connectors[0] = startNodePort.Connectors[0];
            connectors[1] = watchNodePort.Connectors[0];
            // Place each pin where required on the newly connected connectors.
            foreach (var connectorPinLocation in connectorPinLocations)
            {
                int wireIndex = ConnectorSegmentIndex(CurrentPosition, connectorPinLocation);
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
