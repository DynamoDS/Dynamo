using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Utilities;
using Dynamo.UI.Commands;
using Newtonsoft.Json;

using Point = System.Windows.Point;
using Dynamo.Selection;
using Dynamo.Engine;
using System.ComponentModel;
using System.Text;
using Dynamo.ViewModels;
using System.Windows.Threading;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dynamo.Graph;
using Dynamo.Nodes;

namespace Dynamo.ViewModels
{
    public enum PreviewState { Selection, ExecutionPreview, None }

    public partial class ConnectorViewModel : ViewModelBase
    {

        #region Properties

        /// <summary>
        /// Collection of ConnectorPinViewModels associated with this connector.
        /// </summary>
        public ObservableCollection<ConnectorPinViewModel> ConnectorPinViewCollection { get; set; }

        public List<Point[]> BezierControlPoints { get; set; }

        private double _panelX;
        private double _panelY;

        public double PanelX
        {
            get { return _panelX; }
            set
            {
                if (value.Equals(_panelX)) return;
                _panelX = value;
                RaisePropertyChanged(nameof(PanelX));
            }
        }

        public double PanelY
        {
            get { return _panelY; }
            set
            {
                if (value.Equals(_panelY)) return;
                _panelY = value;
                RaisePropertyChanged(nameof(PanelY));
            }
        }

        private Point _mousePosition;
        Point MousePosition
        {
            get
            {
                return _mousePosition;
            }
            set
            {
                _mousePosition = value;
                RaisePropertyChanged(nameof(MousePosition));
            }
        }

        /// <summary>
        /// Required timer for 'watch placement' button desirable behaviour.
        /// </summary>
        private System.Windows.Threading.DispatcherTimer timer;

        private WatchHoverIconViewModel watchHoverViewModel;
        /// <summary>
        /// This WatchHoverViewModel controls the visibility and behaviour of the WatchHoverIcon
        /// which appears when you hover over this connector.
        /// </summary>
        public WatchHoverIconViewModel WatchHoverViewModel
        {
            get { return watchHoverViewModel; }
            private set { watchHoverViewModel = value; RaisePropertyChanged(nameof(WatchHoverViewModel)); }
        }

        private readonly WorkspaceViewModel workspaceViewModel;
        private PortModel activeStartPort;
        public PortModel ActiveStartPort { get { return activeStartPort; } internal set { activeStartPort = value; } }

        private ConnectorModel model;

        /// <summary>
        /// Refers to the connector model associated with this connector view model.
        /// </summary>
        public ConnectorModel ConnectorModel
        {
            get { return model; }
        }

        private bool isConnecting = false;

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

        private bool isVisible = true;
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

        private bool isPartlyVisible = false;
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
            }
        }


        private string connectorDataToolTip;
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

        private bool isDataFlowCollection;
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

        private bool mouseHoverOn;
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
                RaisePropertyChanged(nameof(CurvePoint1));
            }
        }

        private Point _curvePoint2;
        public Point CurvePoint2
        {
            get { return _curvePoint2; }
            set
            {
                _curvePoint2 = value;
                RaisePropertyChanged(nameof(CurvePoint2));
            }
        }

        private Point _curvePoint3;
        public Point CurvePoint3
        {
            get { return _curvePoint3; }
            set
            {
                _curvePoint3 = value;
                RaisePropertyChanged(nameof(CurvePoint3));
            }
        }

        private double dotTop;
        public double DotTop
        {
            get { return dotTop; }
            set
            {
                dotTop = value;
                RaisePropertyChanged(nameof(DotTop));
            }
        }

        private double dotLeft;
        public double DotLeft
        {
            get { return dotLeft; }
            set
            {
                dotLeft = value;
                RaisePropertyChanged(nameof(DotLeft));
            }
        }

        private double endDotSize = 6;
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
                //if (workspaceViewModel.DynamoViewModel.ConnectorType == ConnectorType.BEZIER &&
                //    workspaceViewModel.DynamoViewModel.IsShowingConnectors)
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

        private bool anyPinSelected;
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
            catch ///the odd case of model.Start.Owner value not being available. 
            {
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
            var pX = PanelX;
            var pY = PanelY;
            if (WatchHoverViewModel == null && isDataFlowCollection && timer == null)
            {
                MouseHoverOn = true;
                WatchHoverViewModel = new WatchHoverIconViewModel(this, workspaceViewModel.DynamoViewModel.Model);
                WatchHoverViewModel.IsHalftone = !IsVisible;
                RaisePropertyChanged(nameof(WatchHoverIconViewModel));
            }

        }
        /// <summary>
        /// Timer gets triggered when the user 'unhovers' from the connector. This allows enough time for the user
        /// to click on the 'watch' icon.
        /// </summary>
        /// <param name="parameter"></param>
        private void MouseUnhoverCommandExecute(object parameter)
        {
            if (WatchHoverViewModel != null && timer == null)
            {
                timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 1);
                timer.Start();
                timer.Tick += TimerDone;
            }
        }

        /// <summary>
        /// 'Timer off' event handler associated with unhover command.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerDone(object sender, EventArgs e)
        {
            timer.Stop();
            timer = null;
            WatchHoverViewModel = null;
            RaisePropertyChanged(nameof(WatchHoverIconViewModel));
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
            MousePosition = new Point(PanelX, PanelY);
            if (MousePosition == new Point(0, 0)) return;
            var connectorPinModel = new ConnectorPinModel(PanelX, PanelY, Guid.NewGuid(), model.GUID);
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
            return !IsConnecting;
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

            connectorModel.PropertyChanged += ConnectorModelPropertyChanged;
            connectorModel.Start.Owner.PropertyChanged += StartOwner_PropertyChanged;
            connectorModel.End.Owner.PropertyChanged += EndOwner_PropertyChanged;

            workspaceViewModel.DynamoViewModel.PropertyChanged += DynamoViewModel_PropertyChanged;
            Nodevm.PropertyChanged += nodeViewModel_PropertyChanged;
            Redraw();
            InitializeCommands();

            UpdateConnectorDataToolTip();
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
            var matchingConnectorPinViewModel = this.workspaceViewModel.Pins.FirstOrDefault(x => x.Model == connectorPin);
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
            model.PropertyChanged -= ConnectorModelPropertyChanged;
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
                    RaisePropertyChanged(nameof(BezVisibility));
                    RaisePropertyChanged(nameof(PlineVisibility));
                    var dynModel = sender as DynamoViewModel;
                    IsVisible = dynModel.IsShowingConnectors;
                    break;
                default: break;
            }
        }

        void ConnectorModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DynamoViewModel.Model.CurrentWorkspace):
                    RaisePropertyChanged(nameof(BezVisibility));
                    RaisePropertyChanged(nameof(PlineVisibility));
                    var dynModel = sender as DynamoViewModel;
                    IsVisible = dynModel.IsShowingConnectors;
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
        /// Removes all connectorPinViewModels. This occurs during 'dispose'
        /// as well as during the 'PlaceWatchNode' operation, where previous pins
        /// are discarded and new ones are created.
        /// </summary>
        internal void DiscardAllConnectorPinModels()
        {
            foreach (var pin in ConnectorPinViewCollection)
            {
                pin.Model.Dispose();
                pin.Dispose();
            }
            ConnectorPinViewCollection.Clear();
            workspaceViewModel.Pins.Clear();
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
                    points[count] = new Point(wirePin.Left, wirePin.Top);
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
