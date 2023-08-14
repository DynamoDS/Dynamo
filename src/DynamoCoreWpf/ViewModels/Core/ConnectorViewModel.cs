using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Utilities;
using Dynamo.UI.Commands;

using Point = System.Windows.Point;
using Dynamo.Selection;
using System.ComponentModel;
using System.Text;
using System.Windows.Threading;
using System.Windows.Shapes;
using System.Windows.Media;
using Dynamo.Graph;
using DynCmd = Dynamo.Models.DynamoModel;
using Dynamo.Models;

namespace Dynamo.ViewModels
{
    public enum PreviewState { Selection, ExecutionPreview, Hover, None }

    public partial class ConnectorViewModel : ViewModelBase
    {

        #region Properties

        private double panelX;
        private double panelY;
        private Point mousePosition;
        private ConnectorAnchorViewModel connectorAnchorViewModel;
        private ConnectorContextMenuViewModel connectorContextMenuViewModel;
        private readonly WorkspaceViewModel workspaceViewModel;
        private PortModel activeStartPort;
        private ConnectorModel model;
        private bool isConnecting = false;
        private bool isCollapsed = false;
        private bool isHidden = false;
        private bool isTemporarilyVisible = false;
        private string connectorDataToolTip;
        private bool canShowConnectorTooltip = true;
        private bool mouseHoverOn;
        private bool connectorAnchorViewModelExists;
        private bool isDataFlowCollection;
        private bool anyPinSelected;
        private double dotTop;
        private double dotLeft;
        private double endDotSize = 6;
        private double zIndex = 3;

        private Point curvePoint1;
        private Point curvePoint2;
        private Point curvePoint3;

        /// <summary>
        /// Required timer for desired delay prior to ' connector anchor' display.
        /// </summary>
        private System.Windows.Threading.DispatcherTimer hoverTimer;

        /// <summary>
        /// Collection of ConnectorPinViewModels associated with this connector.
        /// </summary>
        public ObservableCollection<ConnectorPinViewModel> ConnectorPinViewCollection { get; set; }

        /// <summary>
        /// Used to draw multi-segment bezier curves.
        /// </summary>
        public List<Point[]> BezierControlPoints { get; set; }

        /// <summary>
        /// Property tracks 'X' location from mouse poisition
        /// </summary>
        public double PanelX
        {
            get { return panelX; }
            set
            {
                if (value.Equals(panelX)) return;
                panelX = value;
                MousePosition = new Point(panelX, PanelY);
                RaisePropertyChanged(nameof(PanelX));
                RaisePropertyChanged(nameof(MousePosition));
            }
        }

        /// <summary>
        /// Property tracks 'Y' property of the mouse position
        /// </summary>
        public double PanelY
        {
            get { return panelY; }
            set
            {
                if (value.Equals(panelY)) return;
                panelY = value;
                MousePosition = new Point(PanelX, panelY);
                RaisePropertyChanged(nameof(PanelY));
                RaisePropertyChanged(nameof(MousePosition));
            }
        }

        /// <summary>
        /// Constructed mouse position (point) for children of this viewmodel to consume.
        /// </summary>
        public Point MousePosition
        {
            get
            {
                return mousePosition;
            }
            set
            {
                mousePosition = value;
                RaisePropertyChanged(nameof(MousePosition));
            }
        }

        /// <summary>
        /// This WatchHoverViewModel controls the visibility and behaviour of the WatchHoverIcon
        /// which appears when you hover over this connector.
        /// </summary>
        public ConnectorAnchorViewModel ConnectorAnchorViewModel
        {
            get { return connectorAnchorViewModel; }
            private set { connectorAnchorViewModel = value; RaisePropertyChanged(nameof(ConnectorAnchorViewModel)); }
        }

        /// <summary>
        /// Instantiates the context menu when required.
        /// </summary>
        public ConnectorContextMenuViewModel ConnectorContextMenuViewModel
        {
            get { return connectorContextMenuViewModel; }
            private set { connectorContextMenuViewModel = value; RaisePropertyChanged(nameof(ConnectorContextMenuViewModel)); }
        }
        /// <summary>
        /// Used to point to the active start port corresponding to this connector
        /// </summary>
        public PortModel ActiveStartPort { get { return activeStartPort; } internal set { activeStartPort = value; } }

        /// <summary>
        /// Refers to the connector model associated with this connector view model.
        /// </summary>
        public ConnectorModel ConnectorModel
        {
            get { return model; }
        }

        /// <summary>
        /// Provides us with the status of this connector with regards to whether it is currently connecting.
        /// </summary>
        public bool IsConnecting
        {
            get { return isConnecting; }
            set
            {
                isConnecting = value;
                RaisePropertyChanged(nameof(IsConnecting));
            }
        }

        /// <summary>
        /// Controls connector visibility: on/off. When wire is off, additional styling xaml turns off tooltips.
        /// </summary>
        public override bool IsCollapsed
        {
            get => isCollapsed;
            set
            {
                if (isCollapsed == value)
                {
                    return;
                }

                isCollapsed = value;
                RaisePropertyChanged(nameof(IsCollapsed));
                SetCollapseOfPins(IsCollapsed);
                RaisePropertyChanged(nameof(ZIndex));
            }
        }

        public bool IsHidden
        {
            get => isHidden;
            set
            {
                if (isHidden == value)
                {
                    return;
                }

                isHidden = value;
                RaisePropertyChanged(nameof(IsHidden));
                SetVisibilityOfPins(IsHidden);
                SetPartialVisibilityOfPins(IsHidden);
            }
        }

        /// <summary>
        /// Property which overrides 'isDisplayed==false' condition. When this prop is set to true, wires are set to 
        /// 40% opacity.
        /// </summary>
        public bool IsTemporarilyDisplayed
        {
            get { return isTemporarilyVisible; }
            set
            {
                isTemporarilyVisible = value;
                RaisePropertyChanged(nameof(IsTemporarilyDisplayed));
                SetPartialVisibilityOfPins(isTemporarilyVisible);
                if (connectorAnchorViewModel != null)
                    connectorAnchorViewModel.IsTemporarilyDisplayed = isTemporarilyVisible;
            }
        }

        private void SetCollapseOfPins(bool isCollapsed)
        {
            if (ConnectorPinViewCollection is null) { return; }

            foreach (var pin in ConnectorPinViewCollection)
            {
                pin.IsCollapsed = isCollapsed;
            }
        }
        private void SetVisibilityOfPins(bool isHidden)
        {
            if (ConnectorPinViewCollection is null) { return; }

            foreach (var pin in ConnectorPinViewCollection)
            {
                pin.IsHidden = isHidden;
            }
        }
        private void SetPartialVisibilityOfPins(bool isHidden)
        {
            if (ConnectorPinViewCollection is null) { return; }

            foreach (var pin in ConnectorPinViewCollection)
            {
                pin.IsTemporarilyVisible = isHidden;
            }
        }

        /// <summary>
        /// Contains up-to-date tooltip corresponding to connector you are hovering over.
        /// </summary>
        public string ConnectorDataTooltip
        {
            get
            {
                return connectorDataToolTip;
            }
            set
            {
                connectorDataToolTip = value;
                RaisePropertyChanged(nameof(ConnectorDataTooltip));
            }
        }
        /// <summary>
        /// Flag controlling whether the connector tooltip is visible.
        /// Worth noting that in addition to this flag, connector tooltip
        /// is only visible when the connectors are set to
        /// 'bezier' mode.
        /// </summary>
        public bool CanShowConnectorTooltip
        {
            get
            {
                return canShowConnectorTooltip;
            }
            set
            {
                canShowConnectorTooltip = value;
                RaisePropertyChanged(nameof(CanShowConnectorTooltip));
            }
        }

        /// <summary>
        /// Property to determine whether the data corresponding to this connector holds a collection or a single value.
        /// 'Collection' is defined as 5 or more items in this case.
        /// </summary>
        public bool IsDataFlowCollection
        {
            get
            {
                return isDataFlowCollection;
            }
            set
            {
                isDataFlowCollection = value;
                RaisePropertyChanged(nameof(IsDataFlowCollection));
            }
        }

        /// <summary>
        /// Flags whether or not the user is hovering over the current connector.
        /// </summary>
        public bool MouseHoverOn
        {
            get
            {
                return mouseHoverOn;
            }
            set
            {
                mouseHoverOn = value;
                RaisePropertyChanged(nameof(MouseHoverOn));
            }
        }

        /// <summary>
        /// Flags whether or not the user is hovering over the current connector.
        /// </summary>
        public bool ConnectorAnchorViewModelExists
        {
            get
            {
                return connectorAnchorViewModelExists;
            }
            set
            {
                connectorAnchorViewModelExists = value;
                RaisePropertyChanged(nameof(ConnectorAnchorViewModelExists));
                RaisePropertyChanged(nameof(PreviewState));
            }
        }
        /// <summary>
        /// Binding property for connector canvas
        /// </summary>
        public double Left
        {
            get { return 0; }
        }

        /// <summary>
        /// Binding property for connector canvas
        /// </summary>
        public double Top
        {
            get { return 0; }
        }

        //Changed the connectors ZIndex to 2. Groups have ZIndex of 1.
        // 08/02/2021 - ZIndex to 3 as groups can now have grouped groups
        // and they will have a ZIndex of 2
        public double ZIndex
        {
            get 
            {
                return SetZIndex();
            }

            protected set
            {
                zIndex = value;
                RaisePropertyChanged(nameof(ZIndex));
            }
         
        }

        private int SetZIndex()
        {
            if (isConnecting)
                return (int)zIndex;

            var firstNode = this.Nodevm;
            var lastNode = this.NodeEnd;

            int index = firstNode is null || lastNode is null ? 1 : 3;

            //reduce ZIndex if one of associated nodes is collapsed
            bool oneNodeInCollapsedGroup = OneConnectingNodeInCollapsedGroup(firstNode, lastNode);
            bool bothNodesInCollapsedGroup = ConnectingNodesBothInCollapsedGroup(firstNode, lastNode);
            if (oneNodeInCollapsedGroup && !bothNodesInCollapsedGroup)
            {
                var lowestIndex = new int[] { this.Nodevm.ZIndex, this.NodeEnd.ZIndex }
                .OrderBy(x => x)
                .FirstOrDefault();

                //if ZIndex above that of groups, set to be less than that of groups
                if (index > 2)
                {
                    index = 1;
                }
            }

            return index;
        }
        private bool OneConnectingNodeInCollapsedGroup(NodeViewModel firstNode, NodeViewModel lastNode)
        {
            if (firstNode == null || lastNode == null) return false;
            return firstNode.IsNodeInCollapsedGroup || lastNode.IsNodeInCollapsedGroup;
        }
        private bool ConnectingNodesBothInCollapsedGroup(NodeViewModel firstNode, NodeViewModel lastNode)
        {
            if (firstNode == null || lastNode == null) return false;
            return firstNode.IsNodeInCollapsedGroup && lastNode.IsNodeInCollapsedGroup;
        }

        /// <summary>
        ///     The start point of the path pulled from the port's center
        /// </summary>
        public Point CurvePoint0
        {
            get
            {
                if (model == null)
                    return activeStartPort.Center.AsWindowsType();
                else if (model.Start != null)
                    return model.Start.Center.AsWindowsType();
                else
                    return new Point();
            }
        }

        public Point CurvePoint1
        {
            get
            {
                return curvePoint1;
            }
            set
            {
                curvePoint1 = value;
                RaisePropertyChanged(nameof(CurvePoint1));
            }
        }

        public Point CurvePoint2
        {
            get { return curvePoint2; }
            set
            {
                curvePoint2 = value;
                RaisePropertyChanged(nameof(CurvePoint2));
            }
        }

        public Point CurvePoint3
        {
            get { return curvePoint3; }
            set
            {
                curvePoint3 = value;
                RaisePropertyChanged(nameof(CurvePoint3));
            }
        }

        public double DotTop
        {
            get { return dotTop; }
            set
            {
                dotTop = value;
                RaisePropertyChanged(nameof(DotTop));
            }
        }

        public double DotLeft
        {
            get { return dotLeft; }
            set
            {
                dotLeft = value;
                RaisePropertyChanged(nameof(DotLeft));
            }
        }

        public double EndDotSize
        {
            get { return endDotSize; }
            set
            {
                endDotSize = value;
                RaisePropertyChanged(nameof(EndDotSize));
            }
        }

        public NodeViewModel Nodevm
        {
            get
            {
                return workspaceViewModel.Nodes?.FirstOrDefault(x => x.NodeLogic.GUID == model.Start.Owner.GUID);
            }
        }

        public NodeViewModel NodeEnd
        {
            get
            {
                return workspaceViewModel.Nodes?.FirstOrDefault(x => x.NodeLogic.GUID == model.End.Owner.GUID);
            }
        }

        private PreviewState previewState = PreviewState.None;
        public PreviewState PreviewState
        {
            get
            {
                if (model == null)
                {
                    return PreviewState.None;
                }

                if (Nodevm.ShowExecutionPreview || NodeEnd.ShowExecutionPreview)
                {
                    return PreviewState.ExecutionPreview;
                }

                if (model.Start.Owner.IsSelected ||
                    model.End.Owner.IsSelected || AnyPinSelected)
                {
                    return PreviewState.Selection;
                }

                if(this.ConnectorAnchorViewModelExists)
                {
                    return PreviewState.Hover;
                }

                if(previewState != null)
                {
                    return previewState;
                }

                return PreviewState.None;
            }
            set
            {
                previewState = value;
                RaisePropertyChanged(nameof(PreviewState));
            }
        }

        /// <summary>
        /// Toggle used to turn Connector PreviewState to the correct state when a pin is selected.
        /// Modelled after connector preview behaviour when a node is selected.
        /// </summary>
        public bool AnyPinSelected
        {
            get
            {
                return anyPinSelected;
            }
            set
            {
                anyPinSelected = value;
                RaisePropertyChanged(nameof(AnyPinSelected));
            }
        }
        public bool IsFrozen
        {
            get { return model == null ? activeStartPort.Owner.IsFrozen : Nodevm.IsFrozen; }
        }
        public Path ComputedBezierPath { get; set; }
        private PathGeometry _computedPathGeometry;
        public PathGeometry ComputedBezierPathGeometry
        {
            get
            {
                return _computedPathGeometry;
            }
            set
            {
                _computedPathGeometry = value;
                RaisePropertyChanged(nameof(ComputedBezierPathGeometry));
            }
        }

        #endregion

        /// <summary>
        /// Updates 'ConnectorDataTooltip' to reflect data of wire being hovered over.
        /// </summary>
        private void UpdateConnectorDataToolTip()
        {
            bool isCollectionofFiveorMore = false;

            //if model is null or enginecontroller is disposed, return
            if (model is null ||
                model.Start is null ||
                model.Start.Owner is null||
                workspaceViewModel.DynamoViewModel.EngineController.IsDisposed == true)
            { return; }

            //if it is possible to get the last value of the model.Start.Owner
            try
            {
                var portValue = model.Start.Owner.GetValue(model.Start.Index, workspaceViewModel.DynamoViewModel.EngineController);
                if (portValue is null)
                {
                    ConnectorDataTooltip = string.Empty;
                    return;
                }

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"{model.Start.Owner.Name} -> {model.End.Owner.Name}");

                var isCollection = portValue.IsCollection;
                if (isCollection)
                {
                    if (isCollection && portValue.GetElements().Count() > 5)
                    {
                        // only sets 'is a collection' to true if the collection meets a size of 5
                        isCollectionofFiveorMore = true;
                        for (int i = 0; i < 5; i++)
                        {
                            stringBuilder.AppendLine(portValue.GetElements().ElementAt(i).StringData);
                        }
                        stringBuilder.AppendLine("...");
                        stringBuilder.AppendLine(portValue.GetElements().Last().StringData);
                        ConnectorDataTooltip = stringBuilder.ToString();
                    }
                    else
                    {
                        for (int i = 0; i < portValue.GetElements().Count(); i++)
                        {
                            stringBuilder.AppendLine(portValue.GetElements().ElementAt(i).StringData);
                        }
                        ConnectorDataTooltip = stringBuilder.ToString();
                    }
                }
                else
                {
                    stringBuilder.AppendLine(portValue.StringData);
                    ConnectorDataTooltip = stringBuilder.ToString();
                }
                isDataFlowCollection = isCollectionofFiveorMore;
            }
            catch (Exception ex)//the odd case of model.Start.Owner value not being available. 
            {
                _ = ex.Message;
            }
        }

        #region Commands

        /// <summary>
        /// Delegate command used to dispose the existing connector and thus
        /// its connectivity to nodes.
        /// </summary>
        public DelegateCommand BreakConnectionCommand { get; set; }
        /// <summary>
        /// Delegate command used to set the visibility of the connector to 'transparent'.
        /// </summary>
        public DelegateCommand ShowhideConnectorCommand { get; set; }
        /// <summary>
        /// Delegate command us to select the nodes connected to this connector.
        /// </summary>
        public DelegateCommand SelectConnectedCommand { get; set; }
        /// <summary>
        /// Delegate command to run when the mouse is hovering over this connector.
        /// </summary>
        public DelegateCommand MouseHoverCommand { get; set; }
        /// <summary>
        /// Delegate command to run when the mouse just ended hovering over this connector.
        /// </summary>
        public DelegateCommand MouseUnhoverCommand { get; set; }
        /// <summary>
        /// Delegate command to run when 'Pin Wire' item is clicked on this connector ContextMenu.
        /// </summary>
        public DelegateCommand PinConnectorCommand { get; set; }
        /// <summary>
        /// Delegate command to trigger a construction of a ContextMenu.
        /// </summary>
        public DelegateCommand InstantiateContextMenuCommand { get; set; }
        /// <summary>
        /// Delegate command to focus the view on the start node
        /// </summary>
        public DelegateCommand GoToStartNodeCommand { get; set; }
        /// <summary>
        /// Delegate command to focus the view on the end node
        /// </summary>
        public DelegateCommand GoToEndNodeCommand { get; set; }

        /// <summary>
        /// When mouse hovers over connector, if the data coming through the connector is collection of 5 or more,
        /// a 'watch' icon appears at the midpoint of the connector, enabling the user to place a watch node
        /// at that location by clicking on it.
        /// </summary>
        /// <param name="parameter"></param>
        private void MouseHoverCommandExecute(object parameter)
        {
            MouseHoverOn = true;
            
            if (hoverTimer != null)
                ForceTimerOff(hoverTimer);

            if (ConnectorAnchorViewModel == null && hoverTimer == null)
            {
                StartTimer(hoverTimer, new TimeSpan(0, 0, 1));
            }
        }

        /// <summary>
        /// Timer gets triggered when the user 'unhovers' from the connector. This allows enough time for the user
        /// to click on the 'watch' icon.
        /// </summary>
        /// <param name="parameter"></param>
        private void MouseUnhoverCommandExecute(object parameter)
        {
            MouseHoverOn = false;
        }
        /// <summary>
        /// Called from outside during unit tests and thus 'internal' as opposed to 'private'.
        /// </summary>
        internal void FlipOnConnectorAnchor()
        {
            ConnectorAnchorViewModel = new ConnectorAnchorViewModel(this, workspaceViewModel.DynamoViewModel, ConnectorDataTooltip)
            {
                CanShowTooltip = CanShowConnectorTooltip,
                CurrentPosition = MousePosition,
                IsHalftone = IsHidden,
                IsDataFlowCollection = IsDataFlowCollection
            };
            ConnectorAnchorViewModel.RequestDispose += DisposeAnchor;
        }

        private void DisposeAnchor(object arg1, EventArgs arg2)
        {
            ConnectorAnchorViewModel.RequestDispose -= DisposeAnchor;
            ConnectorAnchorViewModel.Dispose();
            ConnectorAnchorViewModel = null;
        }

        internal void CreateContextMenu()
        {
            ConnectorContextMenuViewModel = new ConnectorContextMenuViewModel(this)
            {
                CurrentPosition = MousePosition,
                IsCollapsed = this.IsHidden
            };
            //Updates PreviewState of connector.
            PreviewState = PreviewState.Selection;
            ConnectorContextMenuViewModel.RequestDispose += DisposeContextMenu;
        }

        private void DisposeContextMenu(object arg1, EventArgs arg2)
        {
            PreviewState = PreviewState.None;
            ConnectorContextMenuViewModel.RequestDispose -= DisposeContextMenu;
            ConnectorContextMenuViewModel.Dispose();
            ConnectorContextMenuViewModel = null;
        }

        /// <summary>
        /// If hover == true, then after the timer is up something will appear.
        /// IF set to false, the object in question will disappear.
        /// </summary>
        /// <param name="timer"></param>
        /// <param name="timeSpan"></param>
        private void StartTimer(DispatcherTimer timer, TimeSpan timeSpan)
        {
            timer = new DispatcherTimer
            {
                Interval = timeSpan
            };
            timer.Start();
            timer.Tick += TimerDoneShow;
        }

        /// <summary>
        /// Handles showing ConnectorAnchor when timer is stopped.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerDoneShow(object sender, EventArgs e)
        {
            var timer = sender as DispatcherTimer;
            if (timer is null) { return; }
            timer.Tick -= TimerDoneShow;

            if (MouseHoverOn == false) return;
            FlipOnConnectorAnchor();

            timer.Stop();
            timer = null;
        }

        /// <summary>
        /// Stops timer, sets ConnectorAnchorViewModel to null.
        /// </summary>
        /// <param name="timer"></param>
        private void ForceTimerOff(DispatcherTimer timer)
        {
            timer.Stop();
            timer = null;
            ConnectorAnchorViewModel = null;
            RaisePropertyChanged(nameof(ConnectorAnchorViewModel));
        }

        /// <summary>
        /// Breaks connections between node models it is connected to.
        /// </summary>
        /// <param name="parameter"></param>
        private void BreakConnectionCommandExecute(object parameter)
        {
            // The deletion (and accompanying undo/redo actions) get relayed to the WorkspaceModel.
            workspaceViewModel.Model.ClearConnector(ConnectorModel);
            workspaceViewModel.Model.HasUnsavedChanges = true;
            workspaceViewModel.DynamoViewModel.RaiseCanExecuteUndoRedo();
        }
        /// <summary>
        /// Toggles wire viz on/off. This can be overwritten when a node is selected in hidden mode.
        /// </summary>
        /// <param name="parameter"></param>
        internal void HideConnectorCommandExecute(object parameter)
        {
            // Use the inverse of the current visibility state,
            // unless the command is coming from the port, in
            // which case use that parameter it is specifying.
            bool usedFlag = parameter != null?
                Convert.ToBoolean(parameter):
                !ConnectorModel.IsHidden;
                
            workspaceViewModel.DynamoViewModel.ExecuteCommand(
                   new DynCmd.UpdateModelValueCommand(System.Guid.Empty, ConnectorModel.GUID,
                   nameof(ConnectorModel.IsHidden), usedFlag.ToString()));

            workspaceViewModel.DynamoViewModel.RaiseCanExecuteUndoRedo();

            bool adjacentNodeSelected = model.Start.Owner.IsSelected || model.End.Owner.IsSelected;
            if (adjacentNodeSelected && ConnectorModel.IsHidden)
            {
                IsTemporarilyDisplayed = true;
            }
            else
            {
                IsTemporarilyDisplayed = false;
            }
            workspaceViewModel.Model.HasUnsavedChanges = true;
        }
        /// <summary>
        /// Selects nodes connected to this wire.
        /// </summary>
        /// <param name="parameter"></param>
        private void SelectConnectedCommandExecute(object parameter)
        {
            var leftSideNode = model.Start.Owner;
            var rightSideNode = model.End.Owner;

            DynamoSelection.Instance.Selection.Add(leftSideNode);
            DynamoSelection.Instance.Selection.Add(rightSideNode);
        }
        /// <summary>
        /// Places pin at the location of mouse (over a connector)
        /// </summary>
        /// <param name="parameters"></param>
        private void PinConnectorCommandExecute(object parameters)
        {
            MousePosition = new Point(PanelX - ConnectorPinModel.StaticWidth, PanelY - ConnectorPinModel.StaticWidth);
            if (ConnectorAnchorViewModel != null) ConnectorAnchorViewModel.CurrentPosition = MousePosition;
            if (MousePosition == new Point(0, 0)) return;
            var connectorPinModel = new ConnectorPinModel(MousePosition.X, MousePosition.Y, Guid.NewGuid(), model.GUID);
            ConnectorModel.AddPin(connectorPinModel);
            workspaceViewModel.Model.RecordCreatedModel(connectorPinModel);
            workspaceViewModel.Model.HasUnsavedChanges = true;
        }

        /// <summary>
        /// Instantiates this connector's ContextMenu.
        /// </summary>
        /// <param name="parameters"></param>
        private void InstantiateContextMenuCommandExecute(object parameters)
        {
            if (ConnectorContextMenuViewModel != null)
            {
                ConnectorContextMenuViewModel = null;
                PreviewState = PreviewState.None;
                return;
            }

            CreateContextMenu();
        }

        private void GoToStartNodeCommandExecute(object parameters)
        {
            var startNodeID = ConnectorModel.Start.Owner.GUID;

            //Select
            var command = new DynCmd.SelectModelCommand(startNodeID, ModifierKeys.None);
            workspaceViewModel.DynamoViewModel.ExecuteCommand(command);

            //Focus the node
            workspaceViewModel.DynamoViewModel.CurrentSpaceViewModel.FocusNodeCommand.Execute(startNodeID.ToString());
        }

        private void GoToEndNodeCommandExecute(object parameters)
        {
            var endNodeID = ConnectorModel.End.Owner.GUID;

            //Select
            var command = new DynCmd.SelectModelCommand(endNodeID, ModifierKeys.None);
            workspaceViewModel.DynamoViewModel.ExecuteCommand(command);

            //Focus the node
            workspaceViewModel.DynamoViewModel.CurrentSpaceViewModel.FocusNodeCommand.Execute(endNodeID.ToString());
        }

        /// <summary>
        /// Helper function ssed for placing (re-placing) connector
        /// pins when a WatchNode is placed in the center of a connector.
        /// </summary>
        /// <param name="connectors"></param>
        /// <param name="connectorWireIndex"></param>
        /// <param name="point"></param>
        /// <param name="createdModels"></param>
        public void PinConnectorPlacementFromWatchNode(ConnectorModel[] connectors, int connectorWireIndex, Point point, List<ModelBase> createdModels)
        {
            var selectedConnector = connectors[connectorWireIndex];

            var connectorPinModel = new ConnectorPinModel(point.X, point.Y, Guid.NewGuid(), selectedConnector.GUID);
            selectedConnector.AddPin(connectorPinModel);
            createdModels.Add(connectorPinModel);
        }

        private void HandlerRedrawRequest(object sender, EventArgs e)
        {
            Redraw();
        }

        private bool CanRunMouseHover(object parameter)
        {
            return !IsConnecting;
        }
        private bool CanRunMouseUnhover(object parameter)
        {
            return MouseHoverOn;
        }

        private bool CanInstantiateContextMenu(object parameter)
        {
            return !IsConnecting && !ConnectorPinViewCollection.Any(p => p.IsHoveredOver);
        }

        private void InitializeCommands()
        {
            BreakConnectionCommand = new DelegateCommand(BreakConnectionCommandExecute, x => true);
            ShowhideConnectorCommand = new DelegateCommand(HideConnectorCommandExecute, x => true);
            SelectConnectedCommand = new DelegateCommand(SelectConnectedCommandExecute, x => true);
            MouseHoverCommand = new DelegateCommand(MouseHoverCommandExecute, CanRunMouseHover);
            MouseUnhoverCommand = new DelegateCommand(MouseUnhoverCommandExecute, CanRunMouseUnhover);
            PinConnectorCommand = new DelegateCommand(PinConnectorCommandExecute, x => true);
            InstantiateContextMenuCommand = new DelegateCommand(InstantiateContextMenuCommandExecute, CanInstantiateContextMenu);
            GoToStartNodeCommand = new DelegateCommand(GoToStartNodeCommandExecute, x => true);
            GoToEndNodeCommand = new DelegateCommand(GoToEndNodeCommandExecute, x => true);
        }

        #endregion

        /// <summary>
        /// Construct a view and start drawing.
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="port"></param>
        public ConnectorViewModel(WorkspaceViewModel workspace, PortModel port)
        {
            this.workspaceViewModel = workspace;
            ConnectorPinViewCollection = new ObservableCollection<ConnectorPinViewModel>();
            ConnectorPinViewCollection.CollectionChanged += HandleCollectionChanged;

            IsHidden = !workspaceViewModel.DynamoViewModel.IsShowingConnectors;
            IsConnecting = true;
            MouseHoverOn = false;
            activeStartPort = port;
            ZIndex = SetZIndex();

            Redraw(port.Center);

            InitializeCommands();
            this.PropertyChanged += ConnectorViewModelPropertyChanged;
        }

        private void ConnectorViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case nameof(ConnectorAnchorViewModel):
                    ConnectorAnchorViewModelExists = ConnectorAnchorViewModel is null ? false : true;
                    break;
                default: break;
            }
        }

        /// <summary>
        /// Construct a view and respond to property changes on the model. 
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="connectorModel"></param>
        public ConnectorViewModel(WorkspaceViewModel workspace, ConnectorModel connectorModel)
        {
            this.workspaceViewModel = workspace;
            model = connectorModel;
            IsHidden = model.IsHidden;
            MouseHoverOn = false;
            ZIndex = SetZIndex();

            model.PropertyChanged += HandleConnectorPropertyChanged;
            model.ConnectorPinModels.CollectionChanged += ConnectorPinModelCollectionChanged;

            ConnectorPinViewCollection = new ObservableCollection<ConnectorPinViewModel>();
            ConnectorPinViewCollection.CollectionChanged += HandleCollectionChanged;


            if (connectorModel.ConnectorPinModels != null)
            {
                foreach (var p in connectorModel.ConnectorPinModels)
                {
                    AddConnectorPinViewModel(p);
                }
            }

            connectorModel.Start.PropertyChanged += StartPortModel_PropertyChanged;
            connectorModel.End.PropertyChanged += EndPortModel_PropertyChanged;

            connectorModel.Start.Owner.PropertyChanged += StartOwner_PropertyChanged;
            connectorModel.End.Owner.PropertyChanged += EndOwner_PropertyChanged;

            workspaceViewModel.DynamoViewModel.PropertyChanged += DynamoViewModel_PropertyChanged;
            if (Nodevm != null)
            {
                Nodevm.PropertyChanged += nodeViewModel_PropertyChanged;
            }

            if (NodeEnd != null)
            {
                NodeEnd.PropertyChanged += nodeEndViewModel_PropertyChanged;
            }
            
            Redraw();
            InitializeCommands();

            UpdateConnectorDataToolTip();
            this.PropertyChanged += ConnectorViewModelPropertyChanged;
        }

        private void HandleConnectorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ConnectorModel.IsHidden):
                    ConnectorModel connector = sender as ConnectorModel;
                    if (connector is null)
                    {
                        return;
                    }
                    IsHidden = connector.IsHidden;
                    break;
                default:
                    break;
            }
        }
        private void ConnectorPinModelCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ConnectorPinModel newItem in e.NewItems)
                    {
                        AddConnectorPinViewModel(newItem);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (ConnectorPinModel oldItem in e.OldItems)
                    {
                        RemoveConnectorPinModelViewModel(oldItem);
                    }
                    break;
                default: break;
            }
        }
        /// <summary>
        /// Removes connectorPinViewModel, given a model
        /// </summary>
        /// <param name="connectorPin"></param>
        private void RemoveConnectorPinModelViewModel(ConnectorPinModel connectorPin)
        {
            var matchingConnectorPinViewModel = this.workspaceViewModel.Pins.FirstOrDefault(x => x.Model.GUID == connectorPin.GUID);
            if (matchingConnectorPinViewModel is null) return;
            RemoveConnectorPinModelViewModel(matchingConnectorPinViewModel);
        }

        /// <summary>
        /// Removes connectorPinViewModel from collections it belongs to.
        /// </summary>
        /// <param name="connectorPinViewModel"></param>
        private void RemoveConnectorPinModelViewModel(ConnectorPinViewModel connectorPinViewModel)
        {
            connectorPinViewModel.PropertyChanged -= PinViewModelPropertyChanged;
            connectorPinViewModel.RequestSelect -= HandleRequestSelected;
            connectorPinViewModel.RequestRedraw -= HandlerRedrawRequest;
            connectorPinViewModel.RequestRemove -= HandleConnectorPinViewModelRemove;
            workspaceViewModel.Pins.Remove(connectorPinViewModel);
            ConnectorPinViewCollection.Remove(connectorPinViewModel);

            if (ConnectorPinViewCollection.Count == 0)
                BezierControlPoints = null;

            connectorPinViewModel.Dispose();
        }

        /// <summary>
        /// View model adding method only- given a model
        /// </summary>
        /// <param name="pinModel"></param>
        private void AddConnectorPinViewModel(ConnectorPinModel pinModel)
        {
            var pinViewModel = new ConnectorPinViewModel(this.workspaceViewModel, pinModel)
            {
                IsHidden = this.IsHidden,
                IsTemporarilyVisible = isTemporarilyVisible
            };
            pinViewModel.PropertyChanged += PinViewModelPropertyChanged;

            pinViewModel.RequestSelect += HandleRequestSelected;
            pinViewModel.RequestRedraw += HandlerRedrawRequest;
            pinViewModel.RequestRemove += HandleConnectorPinViewModelRemove;

            workspaceViewModel.Pins.Add(pinViewModel);
            ConnectorPinViewCollection.Add(pinViewModel);
        }

        /// <summary>
        /// Checking to see if any connector pin is selected, if so
        /// global 'AnyPinSelected' is set to true and wire Preview State is set to 'Selected'
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PinViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ConnectorPinViewModel.IsSelected):
                    var vm = sender as ConnectorPinViewModel;
                    AnyPinSelected = vm.IsSelected;
                    break;
                default:
                    break;
            }
        }

        private void HandleRequestSelected(object sender, EventArgs e)
        {
            ConnectorPinViewModel pinViewModel = sender as ConnectorPinViewModel;
            IsTemporarilyDisplayed = pinViewModel.IsSelected && IsHidden;
        }
        /// <summary>
        /// Handles ConnectorPin 'Unpin' command.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleConnectorPinViewModelRemove(object sender, EventArgs e)
        {
            if (!(sender is ConnectorPinViewModel viewModelSender)) return;

            workspaceViewModel.Model.RecordAndDeleteModels(
                new List<ModelBase>() { viewModelSender.Model });
            ConnectorModel.ConnectorPinModels.Remove(viewModelSender.Model);
        }

        private void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Redraw();
        }

        /// <summary>
        /// Dispose function
        /// </summary>
        public override void Dispose()
        {
            model.PropertyChanged -= HandleConnectorPropertyChanged;

            model.Start.PropertyChanged -= StartPortModel_PropertyChanged;
            model.End.PropertyChanged -= EndPortModel_PropertyChanged;

            model.Start.Owner.PropertyChanged -= StartOwner_PropertyChanged;
            model.End.Owner.PropertyChanged -= EndOwner_PropertyChanged;
            model.ConnectorPinModels.CollectionChanged -= ConnectorPinModelCollectionChanged;

            workspaceViewModel.DynamoViewModel.PropertyChanged -= DynamoViewModel_PropertyChanged;
            workspaceViewModel.DynamoViewModel.Model.PreferenceSettings.PropertyChanged -= DynamoViewModel_PropertyChanged;
            if (Nodevm != null)
            {
                Nodevm.PropertyChanged -= nodeViewModel_PropertyChanged;
            }
            if (NodeEnd != null)
            {
                NodeEnd.PropertyChanged -= nodeEndViewModel_PropertyChanged;
            }
            ConnectorPinViewCollection.CollectionChanged -= HandleCollectionChanged;

            foreach (var pin in ConnectorPinViewCollection.ToList())
            {
                pin.RequestRedraw -= HandlerRedrawRequest;
                pin.RequestSelect -= HandleRequestSelected;
            }

            this.PropertyChanged -= ConnectorViewModelPropertyChanged;
            DiscardAllConnectorPinModels();

            if(ConnectorContextMenuViewModel != null)
            {
                ConnectorContextMenuViewModel.Dispose();
            }
            if(ConnectorAnchorViewModel != null)
            {
                ConnectorAnchorViewModel.Dispose();
            }
            base.Dispose();
        }

        private void nodeViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(NodeViewModel.ShowExecutionPreview):
                    RaisePropertyChanged(nameof(PreviewState));
                    break;
                case nameof(NodeViewModel.IsFrozen):
                    RaisePropertyChanged(nameof(IsFrozen));
                    break;
                case nameof(NodeViewModel.IsNodeInCollapsedGroup):
                    RaisePropertyChanged(nameof(ZIndex));
                    break;
                default: break;
            }
        }

        private void nodeEndViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(NodeViewModel.IsNodeInCollapsedGroup):
                    RaisePropertyChanged(nameof(ZIndex));
                    break;
                default: break;
            }
        }

        void StartPortModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(PortModel.Center):
                    RaisePropertyChanged(nameof(CurvePoint0));
                    Redraw();
                    break;
            }
        }

        void EndPortModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(PortModel.Center):
                    RaisePropertyChanged(nameof(CurvePoint3));
                    Redraw();
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
                case nameof(NodeModel.IsSelected):
                    RaisePropertyChanged(nameof(PreviewState));
                    IsTemporarilyDisplayed = model.Start.Owner.IsSelected 
                        && IsHidden ? true : false;
                    break;
                case nameof(NodeModel.Position):
                    RaisePropertyChanged(nameof(CurvePoint0));
                    Redraw();
                    break;
                case nameof(NodeModel.Width):
                    RaisePropertyChanged(nameof(CurvePoint0));
                    Redraw();
                    break;
                case nameof(NodeViewModel.ShowExecutionPreview):
                    RaisePropertyChanged(nameof(PreviewState));
                    break;
                case nameof(NodeModel.CachedValue):
                    UpdateConnectorDataToolTip();
                    break;
                default:
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
                case nameof(NodeModel.IsSelected):
                    RaisePropertyChanged(nameof(PreviewState));
                    IsTemporarilyDisplayed = model.End.Owner.IsSelected 
                        && IsHidden ? true : false;
                    break;
                case nameof(NodeModel.Position):
                    RaisePropertyChanged(nameof(CurvePoint0));
                    Redraw();
                    break;
                case nameof(NodeModel.Width):
                    RaisePropertyChanged(nameof(CurvePoint0));
                    Redraw();
                    break;
                case nameof(NodeViewModel.ShowExecutionPreview):
                    RaisePropertyChanged(nameof(PreviewState));
                    break;
            }
        }

        void DynamoViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DynamoViewModel.IsShowingConnectors):
                    var dynModel = sender as DynamoViewModel;
                    IsHidden = !dynModel.IsShowingConnectors;
                    bool adjacentNodeSelected = model.Start.Owner.IsSelected || model.End.Owner.IsSelected;
                    if (adjacentNodeSelected && ConnectorModel.IsHidden)
                    {
                        IsTemporarilyDisplayed = true;
                    }
                    else
                    {
                        IsTemporarilyDisplayed = false;
                    }
                    break;
                case nameof(DynamoViewModel.IsShowingConnectorTooltip):
                    dynModel = sender as DynamoViewModel;
                    CanShowConnectorTooltip = dynModel.IsShowingConnectorTooltip;
                    break;
                default: break;
            }
        }

        private void HandlePinModelChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (ConnectorPinModel oldPin in e.OldItems)
            {
                var matchingPinViewModel = ConnectorPinViewCollection.FirstOrDefault(pin => pin.ConnectorGuid == oldPin.ConnectorId);
                oldPin.Dispose();

                workspaceViewModel.Pins.Remove(matchingPinViewModel);
                ConnectorPinViewCollection.Remove(matchingPinViewModel);

                if (ConnectorPinViewCollection.Count == 0)
                    BezierControlPoints = null;

                matchingPinViewModel.Dispose();
            }
        }


        /// <summary>
        ///  Removes all connectorPinViewModels/ connectorPinModels. This occurs during 'dispose'
        /// operation as well as during the 'PlaceWatchNode', where all previous pins corresponding 
        /// to a connector are cleareed.
        /// </summary>
        /// <param name="allDeletedModels"> This argument is used when placing a WatchNode from ConnectorAnchorViewModel. A reference
        /// to all previous pins is required for undo/redo recorder.</param>
        internal void DiscardAllConnectorPinModels(List<ModelBase> allDeletedModels = null)
        {
            foreach (var pin in ConnectorPinViewCollection)
            {
                workspaceViewModel.Pins.Remove(pin);
                ConnectorModel.RemovePin(pin.Model);

                if(allDeletedModels != null)
                {
                    allDeletedModels.Add(pin.Model);
                }
                pin.Model.Dispose();
                pin.Dispose();
            }

            ConnectorPinViewCollection.Clear();
        }

        /// <summary>
        /// Collects pin locations of a connector. These are needed to reconstruct
        /// pins when new connectors are constructed. Specifically when a Watch node is 
        /// placed on a connector, thereby creating new connectors.
        /// </summary>
        /// <returns></returns>
        internal List<Point> CollectPinLocations()
        {
            List<Point> points = new List<Point>();
            foreach (var connectorPin in ConnectorPinViewCollection)
            {
                points.Add(new Point(connectorPin.Left, connectorPin.Top));
            }

            return points;
        }

        #region ConnectorRedraw

        /// <summary>
        ///     Recalculate the path points using the internal model.
        /// </summary>
        public void Redraw()
        {
            if (this.ConnectorModel.End != null && ConnectorPinViewCollection.Count > 0)
            {
                RedrawBezierManyPoints();
            }
            else if (this.ConnectorModel.End != null)
            {
                this.Redraw(this.ConnectorModel.End.Center);
            }

            RaisePropertyChanged(nameof(ZIndex));
        }

        /// <summary>
        /// Recalculate the connector's points given the end point
        /// </summary>
        /// <param name="parameter">The position of the end point</param>
        public void Redraw(object parameter)
        {
            var p2 = new Point();

            if (parameter is Point)
            {
                p2 = (Point)parameter;
            }
            else if (parameter is Point2D)
            {
                p2 = ((Point2D)parameter).AsWindowsType();
            }

            CurvePoint3 = p2;

            var offset = 0.0;
            double distance = 0;

            distance = Math.Sqrt(Math.Pow(CurvePoint3.X - CurvePoint0.X, 2) + Math.Pow(CurvePoint3.Y - CurvePoint0.Y, 2));
            offset = .45 * distance;

            CurvePoint1 = new Point(CurvePoint0.X + offset, CurvePoint0.Y);
            CurvePoint2 = new Point(p2.X - offset, p2.Y);

            //if connector is dragged from an input port
            if (ActiveStartPort != null && ActiveStartPort.PortType == PortType.Input)
            {
                CurvePoint1 = new Point(CurvePoint0.X - offset, CurvePoint1.Y); ;
                CurvePoint2 = new Point(p2.X + offset, p2.Y);
            }

            dotTop = CurvePoint3.Y - EndDotSize / 2;
            dotLeft = CurvePoint3.X - EndDotSize / 2;

            //Update all the bindings at once.
            //http://stackoverflow.com/questions/4651466/good-way-to-refresh-databinding-on-all-properties-of-a-viewmodel-when-model-chan
            //RaisePropertyChanged(string.Empty);


            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = CurvePoint0;

            BezierSegment segment = new BezierSegment(CurvePoint1, CurvePoint2, CurvePoint3, true);
            var segmentCollection = new PathSegmentCollection(1);
            segmentCollection.Add(segment);
            pathFigure.Segments = segmentCollection;
            PathFigureCollection pathFigureCollection = new PathFigureCollection();
            pathFigureCollection.Add(pathFigure);

            ComputedBezierPathGeometry = new PathGeometry();
            ComputedBezierPathGeometry.Figures = pathFigureCollection;
            ComputedBezierPath = new Path();
            ComputedBezierPath.Data = ComputedBezierPathGeometry;
        }

        private PathFigure DrawSegmentBetweenPointPairs(Point startPt, Point endPt, ref List<Point[]> controlPointList)
        {
            var offset = 0.0;
            double distance = 0;

            distance = Math.Sqrt(Math.Pow(endPt.X - startPt.X, 2) + Math.Pow(endPt.Y - startPt.Y, 2));
            offset = .45 * distance;

            var pt1 = new Point(startPt.X + offset, startPt.Y);
            var pt2 = new Point(endPt.X - offset, endPt.Y);


            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = startPt;

            BezierSegment segment = new BezierSegment(pt1, pt2, endPt, true);
            var segmentCollection = new PathSegmentCollection(1);
            segmentCollection.Add(segment);
            pathFigure.Segments = segmentCollection;

            controlPointList.Add(new Point[] { startPt, pt1, pt2, endPt });

            return pathFigure;
        }

        private void RedrawBezierManyPoints()
        {
            var parameter = this.ConnectorModel.End.Center;
            var param = parameter as object;

            var controlPoints = new List<Point[]>();
            try
            {
                var p2 = new Point();

                if (parameter is Point)
                {
                    p2 = (Point)param;
                }
                else if (parameter is Point2D)
                {
                    p2 = ((Point2D)param).AsWindowsType();
                }

                CurvePoint3 = p2;

                var offset = 0.0;
                double distance = 0;

                distance = Math.Sqrt(Math.Pow(CurvePoint3.X - CurvePoint0.X, 2) + Math.Pow(CurvePoint3.Y - CurvePoint0.Y, 2));
                offset = .45 * distance;

                CurvePoint1 = new Point(CurvePoint0.X + offset, CurvePoint0.Y);
                CurvePoint2 = new Point(p2.X - offset, p2.Y);

                //if connector is dragged from an input port
                if (ActiveStartPort != null && ActiveStartPort.PortType == PortType.Input)
                {
                    CurvePoint1 = new Point(CurvePoint0.X - offset, CurvePoint1.Y); ;
                    CurvePoint2 = new Point(p2.X + offset, p2.Y);
                }

                dotTop = CurvePoint3.Y - EndDotSize / 2;
                dotLeft = CurvePoint3.X - EndDotSize / 2;

                // Add chain of points including start/end
                Point[] points = new Point[ConnectorPinViewCollection.Count];
                int count = 0;
                foreach (var wirePin in ConnectorPinViewCollection)
                {
                    points[count] = new Point(wirePin.Left+ConnectorPinModel.StaticWidth - (ConnectorPinViewModel.OneThirdWidth * 0.5), wirePin.Top+ ConnectorPinModel.StaticWidth - (ConnectorPinViewModel.OneThirdWidth * 0.5));
                    count++;
                }

                var orderedPoints = points.OrderBy(p => p.X).ToList();

                orderedPoints.Insert(0, CurvePoint0);
                orderedPoints.Insert(orderedPoints.Count, CurvePoint3);

                Point[,] pointPairs = BreakIntoPointPairs(orderedPoints);

                PathFigureCollection pathFigureCollection = new PathFigureCollection();

                for (int i = 0; i < pointPairs.GetLength(0); i++)
                {
                    //each segment starts here
                    var segmentList = new List<Point>();

                    for (int j = 0; j < pointPairs.GetLength(1); j++)
                    {
                        segmentList.Add(pointPairs[i, j]);
                    }

                    var pathFigure = DrawSegmentBetweenPointPairs(segmentList[0], segmentList[1], ref controlPoints);
                    pathFigureCollection.Add(pathFigure);
                }

                BezierControlPoints = new List<Point[]>();
                BezierControlPoints = controlPoints;

                ComputedBezierPathGeometry = new PathGeometry();
                ComputedBezierPathGeometry.Figures = pathFigureCollection;
                ComputedBezierPath = new Path();
                ComputedBezierPath.Data = ComputedBezierPathGeometry;
            }
            catch (Exception ex)
            {
                string mess = ex.Message;
            }
        }

        /// <summary>
        /// Point pairs from a chain of sorted points.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private Point[,] BreakIntoPointPairs(List<Point> points)
        {
            Point[,] outPointPairs = new Point[points.Count - 1, 2];

            for (int i = 0; i < points.Count - 1; i++)
                for (int j = 0; j < 2; j++)
                    outPointPairs[i, j] = points[i + j];
            return outPointPairs;
        }

        private bool CanRedraw(object parameter)
        {
            return true;
        }

        #endregion
    }
}
