using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Dynamo.Core;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using CoreNodeModels;
using Dynamo.UI.Commands;
using System;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Workspaces;
using Dynamo.Graph;
using Dynamo.Logging;

namespace Dynamo.ViewModels
{
    public class ConnectorAnchorViewModel: ViewModelBase
    {
        #region Properties 
        private WorkspaceViewModel WorkspaceViewModel { get; }
        private Point currentPosition;
        private bool isHalftone;
        private bool isTemporarilyVisible = false;
        private bool isDataFlowCollection;
        private bool canDisplayIcons = false;
        private bool canShowTooltip = true;

        private bool watchIconPreviewOn = false;
        private bool pinIconPreviewOn = false;
        private string dataTooltipText;

        private double? cachedZoom = null;
        private Dictionary<double, double> cachedSizes = new Dictionary<double, double>();

        private ConnectorViewModel ViewModel { get; set; }
        private DynamoModel DynamoModel { get; set; }
        private DynamoViewModel DynamoViewModel { get; set; }
        private Dispatcher Dispatcher { get; set; }

        /// <summary>
        /// The size of the Watch and Pin icons (x &amp; y dimensions).
        /// </summary>
        public double MarkerSize { get; set; } = 30;
        /// <summary>
        /// The scaled size of the Watch and Pin icons (x &amp; y dimensions).
        /// </summary>
        public double ScaledMarkerSize => GetCachedScaledSize(MarkerSize);
        /// <summary>
        /// The size of the anchor button (x and y dimensions).
        /// </summary>
        public double AnchorSize { get; set; } = 15;
        /// <summary>
        /// The scaled size of the anchor button (x and y dimensions).
        /// </summary>
        public double ScaledAnchorSize => GetCachedScaledSize(AnchorSize);
        /// <summary>
        /// The offset for the anchor button, calculated as half the size.
        /// </summary>
        public double AnchorOffset => ScaledAnchorSize / -2;
        /// <summary>
        /// The vertical offset for the Watch and Pin buttons.
        /// </summary>
        public double MarkerOffsetY => GetCachedScaledSize(-37);
        /// <summary>
        /// The horizontal offset for the Pin button.
        /// </summary>
        public double MarkerOffsetX => GetCachedScaledSize(-30);

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
        /// Property which overrides 'IsCollapsed' condition. When this prop is set to true, wires are set to 
        /// 40% opacity.
        /// </summary>
        public bool IsTemporarilyDisplayed
        {
            get { return isTemporarilyVisible; }
            set
            {
                isTemporarilyVisible = value;
                RaisePropertyChanged(nameof(IsTemporarilyDisplayed));
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
        /// should be visible/ hidden.
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
           //Collect previous pin locations
            var pinLocations = ViewModel.CollectPinLocations();
            List<ModelBase> AllDeletedModels = new List<ModelBase>();
            ViewModel.DiscardAllConnectorPinModels(AllDeletedModels);
            List<ModelBase> AllCreatedModels = PlaceWatchNode(ViewModel.ConnectorModel, pinLocations, AllDeletedModels);

            // Log analytics of creation of watch node
            Logging.Analytics.TrackEvent(
                Actions.Create,
                Categories.ConnectorOperations,
                "Watch Node");
            RecordUndoModels(DynamoModel.CurrentWorkspace, AllCreatedModels, AllDeletedModels);
        }

        private void RecordUndoModels(WorkspaceModel workspace, List<ModelBase> undoItems, List<ModelBase> deletedItems)
        {
                var userActionDictionary = new Dictionary<ModelBase, UndoRedoRecorder.UserAction>();
                //Add models that were newly created
                foreach (var undoItem in undoItems)
                {
                    userActionDictionary.Add(undoItem, UndoRedoRecorder.UserAction.Creation);
                }
                //Add models that were newly deleted
                foreach (var deletedItem in deletedItems)
                {
                    userActionDictionary.Add(deletedItem, UndoRedoRecorder.UserAction.Deletion);
                }
                WorkspaceModel.RecordModelsForUndo(userActionDictionary, workspace.UndoRecorder);
        }

        /// <summary>
        /// Places pin at the location of mouse (over a connector)
        /// </summary>
        /// <param name="parameters"></param>
        private void PinConnectorCommandExecute(object parameters)
        {
            ViewModel.PinConnectorCommand.Execute(null);
            Logging.Analytics.TrackEvent(
                    Actions.Pin,
                    Categories.ConnectorOperations);
        }

        private void InitCommands()
        {
            PlaceWatchNodeCommand = new DelegateCommand(PlaceWatchNodeCommandExecute, x=> true);
            PinConnectorCommand = new DelegateCommand(PinConnectorCommandExecute, x => true);
        }
        #endregion

        internal void RequestDisposeViewModel()
        {
            OnRequestDispose(this, EventArgs.Empty);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectorViewModel"></param>
        /// <param name="dynamoViewModel"></param>
        /// <param name="tooltipText"></param>
        public ConnectorAnchorViewModel(ConnectorViewModel connectorViewModel,
           DynamoViewModel dynamoViewModel,
           string tooltipText)
        {
            ViewModel = connectorViewModel;
            DynamoViewModel = dynamoViewModel;
            DynamoModel = DynamoViewModel.Model;
            WorkspaceViewModel = dynamoViewModel.CurrentSpaceViewModel;
            DataToolTipText = tooltipText;
            InitCommands();

            Dispatcher = Dispatcher.CurrentDispatcher;

            IsHalftone = false;
            connectorViewModel.PropertyChanged += OnConnectorViewModelPropertyChanged;
        }

        /// <summary>
        /// Dispose this.
        /// </summary>
        public override void Dispose()
        {
            ViewModel.PropertyChanged -= OnConnectorViewModelPropertyChanged;
            base.Dispose();
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
        internal List<ModelBase> PlaceWatchNode(ConnectorModel connector, IEnumerable<Point> connectorPinLocations, List<ModelBase> allDeletedModels)
        {
            var createdModels = new List<ModelBase>();

            NodeModel startNode = ViewModel.ConnectorModel.Start.Owner;
            NodeModel endNode = ViewModel.ConnectorModel.End.Owner;
            this.Dispatcher.Invoke(() =>
            {
                var watchNode = new Watch();
                var nodeX = CurrentPosition.X - (watchNode.Width / 2);
                var nodeY = CurrentPosition.Y - (watchNode.Height / 2);
                DynamoModel.ExecuteCommand(new DynamoModel.CreateNodeCommand(watchNode, nodeX, nodeY, false, false));
                createdModels.Add(watchNode);
                WireNewNode(DynamoModel, startNode, endNode, watchNode, connector, connectorPinLocations, createdModels, allDeletedModels);
            });

            return createdModels;
        }

        /// <summary>
        /// Rewires nodes and newly placed watch node between them.
        /// </summary>
        /// <param name="dynamoModel"></param>
        /// <param name="startNode"></param>
        /// <param name="endNode"></param>
        /// <param name="watchNodeModel"></param>
        /// <param name="connector"></param>
        /// <param name="connectorPinLocations"></param>
        /// <param name="allCreatedModels"></param>
        /// <param name="allDeletedModels"></param>
        private void WireNewNode(
            DynamoModel dynamoModel, 
            NodeModel startNode,
            NodeModel endNode, 
            NodeModel watchNodeModel,
            ConnectorModel connector,
            IEnumerable<Point> connectorPinLocations,
            List<ModelBase> allCreatedModels,
            List<ModelBase> allDeletedModels)
        {
            (int startIndex, int endIndex) = GetPortIndex(connector);

            // Connect startNode and watch node
            dynamoModel.ExecuteCommand(new DynamoModel.MakeConnectionCommand(startNode.GUID, startIndex, PortType.Output, DynamoModel.MakeConnectionCommand.Mode.Begin));
            dynamoModel.ExecuteCommand(new DynamoModel.MakeConnectionCommand(watchNodeModel.GUID, 0, PortType.Input, DynamoModel.MakeConnectionCommand.Mode.End));

            // Connect watch node and endNode
            dynamoModel.ExecuteCommand(new DynamoModel.MakeConnectionCommand(watchNodeModel.GUID, 0, PortType.Output, DynamoModel.MakeConnectionCommand.Mode.Begin));
            dynamoModel.ExecuteCommand(new DynamoModel.MakeConnectionCommand(endNode.GUID, endIndex, PortType.Input, DynamoModel.MakeConnectionCommand.Mode.End));

            PlacePinsOnWires(startNode, watchNodeModel, connector, connectorPinLocations, allCreatedModels, allDeletedModels);
        }

        private void PlacePinsOnWires(NodeModel startNode, 
            NodeModel watchNodeModel, 
            ConnectorModel connector, 
            IEnumerable<Point> connectorPinLocations,
            List<ModelBase> allCreatedModels,
            List<ModelBase> allDeletedModels)
        {
            // Collect ports & connectors of newly connected nodes
            // so that old pins (that need to remain) can be transferred over correctly.
            PortModel startNodePort = startNode.OutPorts.FirstOrDefault(p => p.GUID == connector.Start.GUID);
            PortModel watchNodePort = watchNodeModel.OutPorts[0];
            Graph.Connectors.ConnectorModel[] connectors = new Graph.Connectors.ConnectorModel[2];

            connectors[0] = startNodePort.Connectors.FirstOrDefault(c => c.End.Owner.GUID == watchNodeModel.GUID && c.GUID != connector.GUID);
            connectors[1] = watchNodePort.Connectors.FirstOrDefault(c => c.Start.Owner.GUID == watchNodeModel.GUID && c.GUID != connector.GUID);

            if (connectors.Any(c => c is null))
            {
                return;
            }

            allCreatedModels.AddRange(connectors);
            allDeletedModels.Add(connector);

            if (connectorPinLocations.Count()<1)
            {
                return;
            }

            // Place each pin where required on the newly connected connectors.
            foreach (var connectorPinLocation in connectorPinLocations)
            {
                int wireIndex = ConnectorSegmentIndex(CurrentPosition, connectorPinLocation);
                ViewModel.PinConnectorPlacementFromWatchNode(connectors, wireIndex, connectorPinLocation, allCreatedModels);
            }
        }

        private (int StartIndex, int EndIndex) GetPortIndex(ConnectorModel connector)
        {
            return (connector.Start.Index, connector.End.Index);
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

        /// <summary>
        /// Calculates a scaled size based on the zoom level, applying a non-linear decay.
        /// Ensures the size decreases monotonically and approaches a minimum size as zoom decreases.
        /// </summary>
        private double CalculateScaledSize(double defaultSize)
        {
            // Proportion for the minimum size relative to defaultSize
            const double resizingFactor = 0.5;
            // Controls the rate of size decay for lower zoom levels
            const double curveSteepness = 1.5;

            double minSize = defaultSize * resizingFactor;

            if (WorkspaceViewModel.Zoom >= 1)
            {
                return defaultSize;
            }

            // Apply non-linear decay formula
            double zoomFactor = Math.Pow(WorkspaceViewModel.Zoom, curveSteepness);
            return (minSize + (defaultSize - minSize) * zoomFactor) / WorkspaceViewModel.Zoom;
        }

        /// <summary>
        /// Retrieves a scaled size from the cache or calculates it if the Zoom level has changed or the size is not cached.
        /// </summary>
        private double GetCachedScaledSize(double defaultSize)
        {
            if (cachedZoom != WorkspaceViewModel.Zoom)
            {
                cachedSizes.Clear();
                cachedZoom = WorkspaceViewModel.Zoom;
            }

            if (!cachedSizes.TryGetValue(defaultSize, out var scaledSize))
            {
                scaledSize = CalculateScaledSize(defaultSize);
                cachedSizes[defaultSize] = scaledSize;
            }

            return scaledSize;
        }
    }
}
