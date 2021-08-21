﻿using System;
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

namespace Dynamo.ViewModels
{
    public enum PreviewState { Selection, ExecutionPreview, None }

    public partial class ConnectorViewModel : ViewModelBase
    {

        #region Properties

        private double panelX;
        private double panelY;
        private Point mousePosition;
        private ConnectorAnchorViewModel connectorAnchorViewModel;
        private readonly WorkspaceViewModel workspaceViewModel;
        private PortModel activeStartPort;
        private ConnectorModel model;
        private bool isConnecting = false;
        private bool isVisible = true;
        private bool isPartlyVisible = false;
        private string connectorDataToolTip;
        private bool canShowConnectorTooltip = true;
        private bool mouseHoverOn;
        private bool connectorAnchorViewModelExists;
        private bool isDataFlowCollection;
        private bool anyPinSelected;
        private double dotTop;
        private double dotLeft;
        private double endDotSize = 6;

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
        public bool IsVisible
        {
            get { return isVisible; }
            set
            {
                isVisible = value;
                RaisePropertyChanged(nameof(IsVisible));
                SetVisibilityOfPins(IsVisible);
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
                SetPartialVisibilityOfPins(isPartlyVisible);
                if (connectorAnchorViewModel != null)
                    connectorAnchorViewModel.IsPartlyVisible = isPartlyVisible;
            }
        }

        private void SetVisibilityOfPins(bool visibility)
        {
            if (ConnectorPinViewCollection is null) { return; }

            foreach (var pin in ConnectorPinViewCollection)
            {
                var visibilityModified = visibility && BezVisibility ? true : false;
                //set visible or hidden based on connector
                pin.IsVisible = visibilityModified;
            }
        }
        private void SetPartialVisibilityOfPins(bool partialVisibility)
        {
            if (ConnectorPinViewCollection is null) { return; }

            foreach (var pin in ConnectorPinViewCollection)
            {
                var partialVisibilityModified = partialVisibility && BezVisibility ? true : false;
                //set 'partlyVisible' based on connector (when selected while connector is hidden)
                pin.IsPartlyVisible = partialVisibilityModified;
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
        public double ZIndex
        {
            get { return 2; }
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

        /// <summary>
        /// Returns visible if the connectors is in the current space and the 
        /// model's current connector type is BEZIER
        /// </summary>
        public bool BezVisibility
        {
            get
            {
                if (workspaceViewModel.DynamoViewModel.ConnectorType == ConnectorType.BEZIER)
                    return true;
                return false;
            }
            set
            {
                RaisePropertyChanged(nameof(BezVisibility));
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
                if (workspaceViewModel.DynamoViewModel.ConnectorType == ConnectorType.POLYLINE)
                    return true;
                return false;
            }
            set
            {
                RaisePropertyChanged(nameof(PlineVisibility));
            }
        }

        public NodeViewModel Nodevm
        {
            get
            {
                return workspaceViewModel.Nodes.FirstOrDefault(x => x.NodeLogic.GUID == model.Start.Owner.GUID);
            }
        }

        public PreviewState PreviewState
        {
            get
            {
                if (model == null)
                {
                    return PreviewState.None;
                }

                if (Nodevm.ShowExecutionPreview)
                {
                    return PreviewState.ExecutionPreview;
                }

                if (model.Start.Owner.IsSelected ||
                    model.End.Owner.IsSelected || AnyPinSelected)
                {
                    return PreviewState.Selection;
                }

                return PreviewState.None;
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

            ///if model is null or enginecontroller is disposed, return
            if (model is null ||
                model.Start is null ||
                model.Start.Owner is null||
                workspaceViewModel.DynamoViewModel.EngineController.IsDisposed == true)
            { return; }

            ///if it is possible to get the last value of the model.Start.Owner
            try
            {
                var portValue = model.Start.Owner.GetValue(model.Start.Index, workspaceViewModel.DynamoViewModel.EngineController);
                if (portValue is null)
                {
                    ConnectorDataTooltip = "N/A";
                    return;
                }

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"{model.Start.Owner.Name} -> {model.End.Owner.Name}");

                var isCollection = portValue.IsCollection;
                if (isCollection)
                {
                    if (isCollection && portValue.GetElements().Count() > 5)
                    {
                        ///only sets 'is a collection' to true if the collection meets a size of 5
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
            catch (Exception ex)///the odd case of model.Start.Owner value not being available. 
            {
                string m = ex.Message;
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
        public DelegateCommand HideConnectorCommand { get; set; }
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
            ConnectorAnchorViewModel = new ConnectorAnchorViewModel(this, workspaceViewModel.DynamoViewModel.Model, ConnectorDataTooltip);
            ConnectorAnchorViewModel.CanShowTooltip = CanShowConnectorTooltip;
            ConnectorAnchorViewModel.CurrentPosition = MousePosition;
            ConnectorAnchorViewModel.IsHalftone = !IsVisible;
            ConnectorAnchorViewModel.IsDataFlowCollection = IsDataFlowCollection;
            ConnectorAnchorViewModel.RequestDispose += DisposeAnchor;
        }

        private void DisposeAnchor(object arg1, EventArgs arg2)
        {
            ConnectorAnchorViewModel.RequestDispose -= DisposeAnchor;
            ConnectorAnchorViewModel = null;
        }

        /// <summary>
        /// If hover == true, then after the timer is up something will appear.
        /// IF set to false, the object in question will disappear.
        /// </summary>
        /// <param name="timer"></param>
        /// <param name="timeSpan"></param>
        /// <param name="hover"></param>
        private void StartTimer(DispatcherTimer timer, TimeSpan timeSpan)
        {
            timer = new DispatcherTimer();
            timer.Interval = timeSpan;
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
            this.Dispose();
            ConnectorModel.Delete();
        }
        /// <summary>
        /// Toggles wire viz on/off. This can be overwritten when a node is selected in hidden mode.
        /// </summary>
        /// <param name="parameter"></param>
        private void HideConnectorCommandExecute(object parameter)
        {
            IsVisible = !IsVisible;
            bool adjacentNodeSelected = model.Start.Owner.IsSelected || model.End.Owner.IsSelected;
            if (adjacentNodeSelected && isVisible == false)
            {
                IsPartlyVisible = true;
            }
            else
            {
                IsPartlyVisible = false;
            }
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
            ConnectorAnchorViewModel.CurrentPosition = MousePosition;
            if (MousePosition == new Point(0, 0)) return;
            var connectorPinModel = new ConnectorPinModel(MousePosition.X, MousePosition.Y, Guid.NewGuid(), model.GUID);
            ConnectorModel.AddPin(connectorPinModel);
            workspaceViewModel.Model.RecordCreatedModel(connectorPinModel);
        }

        /// <summary>
        /// Helper function ssed for placing (re-placing) connector
        /// pins when a WatchNode is placed in the center of a connector.
        /// </summary>
        /// <param name="point"></param>
        public void PinConnectorPlacementFromWatchNode(ConnectorModel[] connectors, int connectorWireIndex, Point point)
        {
            var connectorPinModel = new ConnectorPinModel(point.X, point.Y, Guid.NewGuid(), model.GUID);
            connectors[connectorWireIndex].AddPin(connectorPinModel);
            workspaceViewModel.Model.RecordCreatedModel(connectorPinModel);
        }

        private void HandlerRedrawRequest(object sender, EventArgs e)
        {
            Redraw();
        }

        private bool CanRunMouseHover(object parameter)
        {
            return !IsConnecting && BezVisibility;
        }
        private bool CanRunMouseUnhover(object parameter)
        {
            return MouseHoverOn;
        }

        private void InitializeCommands()
        {
            BreakConnectionCommand = new DelegateCommand(BreakConnectionCommandExecute, x => true);
            HideConnectorCommand = new DelegateCommand(HideConnectorCommandExecute, x => true);
            SelectConnectedCommand = new DelegateCommand(SelectConnectedCommandExecute, x => true);
            MouseHoverCommand = new DelegateCommand(MouseHoverCommandExecute, CanRunMouseHover);
            MouseUnhoverCommand = new DelegateCommand(MouseUnhoverCommandExecute, CanRunMouseUnhover);
            PinConnectorCommand = new DelegateCommand(PinConnectorCommandExecute, x => true);
        }

        #endregion

        /// <summary>
        /// Construct a view and start drawing.
        /// </summary>
        /// <param name="port"></param>
        public ConnectorViewModel(WorkspaceViewModel workspace, PortModel port)
        {
            this.workspaceViewModel = workspace;
            ConnectorPinViewCollection = new ObservableCollection<ConnectorPinViewModel>();
            ConnectorPinViewCollection.CollectionChanged += HandleCollectionChanged;

            IsVisible = workspaceViewModel.DynamoViewModel.IsShowingConnectors;
            IsConnecting = true;
            MouseHoverOn = false;
            activeStartPort = port;

            Redraw(port.Center);

            InitializeCommands();
            this.PropertyChanged += connectorViewModelPropertyChanged;
        }

        private void connectorViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
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
        /// <param name="connectorModel"></param>
        public ConnectorViewModel(WorkspaceViewModel workspace, ConnectorModel connectorModel)
        {
            this.workspaceViewModel = workspace;
            model = connectorModel;
            connectorModel.ConnectorPinModels.CollectionChanged += ConnectorPinModelCollectionChanged;

            ConnectorPinViewCollection = new ObservableCollection<ConnectorPinViewModel>();
            ConnectorPinViewCollection.CollectionChanged += HandleCollectionChanged;

            IsVisible = workspaceViewModel.DynamoViewModel.IsShowingConnectors;
            MouseHoverOn = false;

            if (connectorModel.ConnectorPinModels != null)
            {
                foreach (var p in connectorModel.ConnectorPinModels)
                {
                    AddConnectorPinViewModel(p);
                }
            }

            connectorModel.Start.Owner.PropertyChanged += StartOwner_PropertyChanged;
            connectorModel.End.Owner.PropertyChanged += EndOwner_PropertyChanged;

            workspaceViewModel.DynamoViewModel.PropertyChanged += DynamoViewModel_PropertyChanged;
            Nodevm.PropertyChanged += nodeViewModel_PropertyChanged;
            Redraw();
            InitializeCommands();

            UpdateConnectorDataToolTip();
            this.PropertyChanged += connectorViewModelPropertyChanged;
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
            var pinViewModel = new ConnectorPinViewModel(this.workspaceViewModel, pinModel);
            pinViewModel.IsVisible = IsVisible;
            pinViewModel.IsPartlyVisible = isPartlyVisible;
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
                case nameof(ConnectorPinModel.IsSelected):
                    var vm = sender as ConnectorPinViewModel;
                    AnyPinSelected = vm.IsSelected;
                    RaisePropertyChanged(nameof(PreviewState));
                    break;
                default:
                    break;
            }
        }

        private void HandleRequestSelected(object sender, EventArgs e)
        {
            ConnectorPinViewModel pinViewModel = sender as ConnectorPinViewModel;
            IsPartlyVisible = pinViewModel.IsSelected && IsVisible == false ? true : false;
        }
        /// <summary>
        /// Handles ConnectorPin 'Unpin' command.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleConnectorPinViewModelRemove(object sender, EventArgs e)
        {
            var viewModelSender = sender as ConnectorPinViewModel;
            if (viewModelSender is null) return;

            workspaceViewModel.Model.RecordAndDeleteModels(
                new List<ModelBase>() { viewModelSender.Model });
            ConnectorModel.ConnectorPinModels.Remove(viewModelSender.Model);
        }

        private void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Redraw();
        }
        public virtual void Dispose()
        {
            model.Start.Owner.PropertyChanged -= StartOwner_PropertyChanged;
            model.End.Owner.PropertyChanged -= EndOwner_PropertyChanged;
            model.ConnectorPinModels.CollectionChanged -= ConnectorPinModelCollectionChanged;

            workspaceViewModel.DynamoViewModel.Model.PreferenceSettings.PropertyChanged -= DynamoViewModel_PropertyChanged;
            Nodevm.PropertyChanged -= nodeViewModel_PropertyChanged;

            foreach (var pin in ConnectorPinViewCollection.ToList())
            {
                pin.RequestRedraw -= HandlerRedrawRequest;
                pin.RequestSelect -= HandleRequestSelected;
            }

            DiscardAllConnectorPinModels();
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
                    IsPartlyVisible = model.Start.Owner.IsSelected && IsVisible == false ? true : false;
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
                    IsPartlyVisible = model.End.Owner.IsSelected && IsVisible == false ? true : false;
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
                case nameof(ConnectorType):
                    if (workspaceViewModel.DynamoViewModel.ConnectorType == ConnectorType.BEZIER)
                    {
                        BezVisibility = true;
                        SetVisibilityOfPins(IsVisible);
                        PlineVisibility = false;
                    }
                    else
                    {
                        BezVisibility = false;
                        SetVisibilityOfPins(IsVisible);
                        PlineVisibility = true;
                    }

                    Redraw();
                    break;
                case nameof(DynamoViewModel.IsShowingConnectors):
                    var dynModel = sender as DynamoViewModel;
                    IsVisible = dynModel.IsShowingConnectors;
                    bool adjacentNodeSelected = model.Start.Owner.IsSelected || model.End.Owner.IsSelected;
                    if (adjacentNodeSelected && isVisible == false)
                    {
                        IsPartlyVisible = true;
                    }
                    else
                    {
                        IsPartlyVisible = false;
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
        /// Removes all connectorPinViewModels/ connectorPinModels. This occurs during 'dispose'
        /// operation as well as during the 'PlaceWatchNode', where all previous pins corresponding 
        /// to a connector are cleareed.
        /// </summary>
        internal void DiscardAllConnectorPinModels()
        {
            foreach (var pin in ConnectorPinViewCollection)
            {
                workspaceViewModel.Pins.Remove(pin);
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
                p2 = (Point)parameter;
            }
            else if (parameter is Point2D)
            {
                p2 = ((Point2D)parameter).AsWindowsType();
            }

            CurvePoint3 = p2;

            var offset = 0.0;
            double distance = 0;
            if (this.BezVisibility == true)
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
            if (this.BezVisibility == true)
            {
                distance = Math.Sqrt(Math.Pow(endPt.X - startPt.X, 2) + Math.Pow(endPt.Y - startPt.Y, 2));
                offset = .45 * distance;
            }
            else
            {
                distance = endPt.X - startPt.X;
                offset = distance / 2;
            }

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
                if (this.BezVisibility == true)
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

                dotTop = CurvePoint3.Y - EndDotSize / 2;
                dotLeft = CurvePoint3.X - EndDotSize / 2;

                ///Add chain of points including start/end
                Point[] points = new Point[ConnectorPinViewCollection.Count];
                int count = 0;
                foreach (var wirePin in ConnectorPinViewCollection)
                {
                    points[count] = new Point(wirePin.Left+ConnectorPinModel.StaticWidth - (wirePin.HalfWidth * 0.3), wirePin.Top+ ConnectorPinModel.StaticWidth - (wirePin.HalfWidth * 0.3));
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
