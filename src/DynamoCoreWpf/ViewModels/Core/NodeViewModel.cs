using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Dynamo.Configuration;
using Dynamo.Engine.CodeGeneration;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.UI;
using Dynamo.Wpf.ViewModels.Core;
using Newtonsoft.Json;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace Dynamo.ViewModels
{
    /// <summary>
    /// Interaction logic for dynControl.xaml
    /// </summary>

    public partial class NodeViewModel : ViewModelBase
    {
        #region delegates
        public delegate void SetToolTipDelegate(string message);
        public delegate void NodeDialogEventHandler(object sender, NodeDialogEventArgs e);
        public delegate void SnapInputEventHandler(PortViewModel portViewModel);
        public delegate void PreviewPinStatusHandler(bool pinned);

        internal delegate void NodeAutoCompletePopupEventHandler(Popup popup);
        internal delegate void PortContextMenuPopupEventHandler(Popup popup);
        #endregion

        #region events
        public event SnapInputEventHandler SnapInputEvent;
        #endregion

        [JsonIgnore]
        public Action OnMouseLeave;

        #region private members

        ObservableCollection<PortViewModel> inPorts = new ObservableCollection<PortViewModel>();
        ObservableCollection<PortViewModel> outPorts = new ObservableCollection<PortViewModel>();
        NodeModel nodeLogic;
        private int zIndex = Configurations.NodeStartZIndex;
        private string astText = string.Empty;
        private bool isexplictFrozen;
        private bool canToggleFrozen = true;
        private bool isRenamed = false;
        private bool isNodeInCollapsedGroup = false;
        private const string WatchNodeName = "Watch";
        #endregion

        #region public members

        /// <summary>
        /// Returns NodeModel ID
        /// </summary>
        [JsonConverter(typeof(IdToGuidConverter))]
        [JsonProperty(Order = 1)]
        public Guid Id
        {
            get { return NodeModel.GUID; }
        }

        [JsonIgnore]
        public readonly DynamoViewModel DynamoViewModel;

        [JsonIgnore]
        public readonly WorkspaceViewModel WorkspaceViewModel;

        [JsonIgnore]
        public readonly Size? PreferredSize;

        private bool previewPinned;
        [JsonIgnore]
        public bool PreviewPinned
        {
            get { return previewPinned; }
            set
            {
                if (previewPinned == value) return;
                previewPinned = value;

                DynamoViewModel.ExecuteCommand(
                    new DynamoModel.UpdateModelValueCommand(
                        System.Guid.Empty, NodeModel.GUID, "PreviewPinned", previewPinned.ToString()));
            }
        }

        [JsonIgnore]
        public NodeModel NodeModel { get { return nodeLogic; } private set { nodeLogic = value; } }

        [JsonIgnore]
        public LacingStrategy ArgumentLacing
        {
            get { return nodeLogic.ArgumentLacing; }
        }

        [JsonIgnore]
        public NodeModel NodeLogic
        {
            get { return nodeLogic; }
        }

        [JsonIgnore]
        public InfoBubbleViewModel ErrorBubble { get; set; }

        [JsonIgnore]
        [Obsolete("This property is deprecated and will be removed in a future version of Dynamo.")]
        public string ToolTipText
        {
            get { return nodeLogic.ToolTipText; }
        }

        [JsonIgnore]
        public ObservableCollection<PortViewModel> InPorts
        {
            get { return inPorts; }
            set
            {
                inPorts = value;
                RaisePropertyChanged("InPorts");
            }
        }

        [JsonIgnore]
        public ObservableCollection<PortViewModel> OutPorts
        {
            get { return outPorts; }
            set
            {
                outPorts = value;
                RaisePropertyChanged("OutPorts");
            }
        }

        [JsonIgnore]
        public bool IsSelected
        {
            get
            {
                return nodeLogic.IsSelected;
            }
        }

        [JsonIgnore]
        public bool IsInput
        {
            get
            {
                return nodeLogic.IsInputNode;
            }
        }

        [JsonProperty(Order = 3)]
        public bool IsSetAsInput
        {
            get
            {
                return nodeLogic.IsSetAsInput;
            }
            set
            {
                if (nodeLogic.IsSetAsInput != value)
                {
                    DynamoViewModel.ExecuteCommand(new DynamoModel.UpdateModelValueCommand(
                        Guid.Empty, NodeModel.GUID, nameof(IsSetAsInput), value.ToString()));

                    RaisePropertyChanged(nameof(IsSetAsInput));
                    Analytics.TrackEvent(Actions.Set, Categories.NodeContextMenuOperations, "AsInput");
                }
            }
        }

        [JsonIgnore]
        public bool IsOutput
        {
            get
            {
                return nodeLogic.IsOutputNode;
            }
        }

        [JsonProperty(Order = 4)]
        public bool IsSetAsOutput
        {
            get
            {
                return nodeLogic.IsSetAsOutput;
            }
            set
            {
                if (nodeLogic.IsSetAsOutput != value)
                {
                    DynamoViewModel.ExecuteCommand(new DynamoModel.UpdateModelValueCommand(
                        Guid.Empty, NodeModel.GUID, nameof(IsSetAsOutput), value.ToString()));

                    RaisePropertyChanged(nameof(IsSetAsOutput));
                    Analytics.TrackEvent(Actions.Set, Categories.NodeContextMenuOperations, "AsOutput");
                }
            }
        }


        /// <summary>
        /// The Name of the nodemodel this view points to
        /// this is the name of the node as it is displayed in the UI.
        /// </summary>
        [JsonProperty(Order = 2)]
        public string Name
        {
            get
            {
                IsRenamed = OriginalName != nodeLogic.Name;
                return nodeLogic.Name;
            }
            set { nodeLogic.Name = value; }
        }

        /// <summary>
        /// The original name of the node. Notice this property will return
        /// current node name if the node is dummy node or unloaded custom node.
        /// </summary>
        [JsonIgnore]
        public string OriginalName
        {
            get { return nodeLogic.GetOriginalName(); }
        }


        /// <summary>
        /// If a node has been renamed. Notice this boolean will be disabled
        /// (always false) if the node is dummy node or unloaded custom node.
        /// </summary>
        [JsonIgnore]
        public bool IsRenamed
        {
            get { return isRenamed; }
            set
            {
                if (isRenamed != value)
                {
                    isRenamed = value;
                    RaisePropertyChanged(nameof(IsRenamed));
                }
            }
        }

        [JsonIgnore]
        public ElementState State
        {
            get { return nodeLogic.State; }
        }
        
        /// <summary>
        /// The total number of info/warnings/errors dismissed by the user on this node.
        /// This is displayed on the node by a little icon beside the Context Menu button.
        /// </summary>
        [JsonIgnore]
        public int NumberOfDismissedAlerts
        {
            get => DismissedAlerts.Count;
        }

        [JsonIgnore]
        public string Description
        {
            get { return nodeLogic.Description; }
        }

        [JsonIgnore]
        public bool IsCustomFunction
        {
            get { return nodeLogic.IsCustomFunction ? true : false; }
        }
        
        /// <summary>
        /// Element's left position is two-way bound to this value
        /// </summary>
        [JsonIgnore]
        public double Left
        {
            get { return nodeLogic.X; }
            set
            {
                nodeLogic.X = value;
                RaisePropertyChanged("Left");
            }
        }

        /// <summary>
        /// Element's top position is two-way bound to this value
        /// </summary>
        [JsonIgnore]
        public double Top
        {
            get { return nodeLogic.Y; }
            set
            {
                nodeLogic.Y = value;
                RaisePropertyChanged("Top");
            }
        }

        /// <summary>
        /// ZIndex is used to order nodes, when some node is clicked.
        /// This selected node should be moved above others.
        /// Start value of zIndex is 3, because 1 is for groups and 2 is for connectors.
        /// Nodes should be always at the top.
        /// 
        /// Static is used because every node should know what is the highest z-index right now.
        /// </summary>
        internal static int StaticZIndex = Configurations.NodeStartZIndex;

        /// <summary>
        /// ZIndex represents the order on the z-plane in which nodes appear.
        /// </summary>
        [JsonIgnore]
        public int ZIndex
        {
            get { return zIndex; }
            set
            {
                zIndex = value;
                RaisePropertyChanged("ZIndex");
                if (ErrorBubble == null) return;
                ErrorBubble.ZIndex = zIndex + 1;
            }
        }

        /// <summary>
        /// Input grid's enabled state is now bound to this property
        /// which tracks the node model's InteractionEnabled property
        /// </summary>
        [JsonIgnore]
        public bool IsInteractionEnabled
        {
            get { return true; }
        }

        [JsonProperty("ShowGeometry",Order = 6)]
        public bool IsVisible
        {
            get
            {
                return nodeLogic.IsVisible;
            }
        }

        /// <summary>
        /// Determines whether or not the semi-transparent overlay is displaying on the node.
        /// This reflects whether the node is in a info/warning/error/frozen state
        /// </summary>
        [JsonIgnore]
        public bool NodeOverlayVisible => IsFrozen;

        /// <summary>
        /// Determines whether the node is showing a bar at its base, indicating that the
        /// node has undismissed info/warning/error messages.
        /// </summary>
        [JsonIgnore]
        public bool NodeWarningBarVisible => (ErrorBubble != null && ErrorBubble.DoesNodeDisplayMessages) || IsVisible == false;

        /// <summary>
        /// The color of the warning bar: blue for info, orange for warnings, red for errors.
        /// </summary>
        [JsonIgnore]
        public SolidColorBrush WarningBarColor
        {
            get => warningBarColor;
            internal set
            {
                if (warningBarColor != value)
                {
                    warningBarColor = value;
                    RaisePropertyChanged(nameof(WarningBarColor));
                }
            }
        }

        /// <summary>
        /// Determines the color of the node's visual overlay, which displays
        /// if the node is in a Frozen, Info, Error or Warning state.
        /// </summary>
        [JsonIgnore]
        public SolidColorBrush NodeOverlayColor
        {
            get => nodeOverlayColor;
            internal set
            {
                if (nodeOverlayColor != value)
                {
                    nodeOverlayColor = value;
                    RaisePropertyChanged(nameof(NodeOverlayColor));
                }
            }
        }

        [JsonIgnore]
        public Visibility PeriodicUpdateVisibility
        {
            get
            {
                return nodeLogic.CanUpdatePeriodically
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        [JsonIgnore]
        public bool EnablePeriodicUpdate
        {
            get { return nodeLogic.CanUpdatePeriodically; }
            set { nodeLogic.CanUpdatePeriodically = value; }
        }

        [JsonIgnore]
        public bool ShowsVisibilityToggles
        {
            get { return true; }
        }

        [JsonIgnore]
        public bool IsPreviewInsetVisible
        {
            get { return WorkspaceViewModel.Model is HomeWorkspaceModel && nodeLogic.ShouldDisplayPreview; }
        }

        [JsonIgnore]
        public bool ShouldShowGlyphBar
        {
            get { return IsPreviewInsetVisible || ArgumentLacing != LacingStrategy.Disabled; }
        }

        /// <summary>
        /// Enable or disable text labels on nodes.
        /// </summary>
        [JsonIgnore]
        public bool IsDisplayingLabels
        {
            get { return nodeLogic.DisplayLabels; }
            set
            {
                if (nodeLogic.DisplayLabels != value)
                {
                    DynamoViewModel.ExecuteCommand(new DynamoModel.UpdateModelValueCommand(
                    Guid.Empty, NodeModel.GUID, nameof(nodeLogic.DisplayLabels), value.ToString()));

                    RaisePropertyChanged(nameof(IsDisplayingLabels));
                    Analytics.TrackEvent(Actions.Show, Categories.NodeContextMenuOperations, "Labels");
                }
            }
        }

        [JsonIgnore]
        public bool CanDisplayLabels
        {
            get
            {
                //lock (nodeLogic.RenderPackagesMutex)
                //{
                //    return nodeLogic.RenderPackages.Any(y => ((RenderPackage)y).IsNotEmpty());
                //}

                return true;
            }
        }

        [JsonIgnore]
        public string ASTText
        {
            get { return astText; }
            set
            {
                astText = value;
                RaisePropertyChanged("ASTText");
            }
        }

        [JsonIgnore]
        public bool ShowDebugASTs
        {
            get { return DynamoViewModel.Model.DebugSettings.ShowDebugASTs; }
            set
            {
                DynamoViewModel.Model.DebugSettings.ShowDebugASTs = value;
            }
        }

        [JsonIgnore]
        public bool WillForceReExecuteOfNode
        {
            get
            {
                return NodeModel.NeedsForceExecution;
            }
        }

        private bool showExectionPreview;
        [JsonIgnore]
        public bool ShowExecutionPreview
        {
            get
            {
                return showExectionPreview;
            }
            set
            {
                showExectionPreview = value;
                RaisePropertyChanged("ShowExecutionPreview");
                RaisePropertyChanged("PreviewState");
            }
        }

        [JsonIgnore]
        public PreviewState PreviewState
        {
            get
            {
                if (ShowExecutionPreview)
                {
                    return PreviewState.ExecutionPreview;
                }

                if (NodeModel.IsSelected)
                {
                    return PreviewState.Selection;
                }

                return PreviewState.None;
            }
        }

        private bool isNodeNewlyAdded;
        private ImageSource imageSource;
        private string imgGlyphOneSource;
        private string imgGlyphTwoSource;
        private string imgGlyphThreeSource;
        private SolidColorBrush warningBarColor;
        private SolidColorBrush nodeOverlayColor;

        private static readonly string warningGlyph = "/DynamoCoreWpf;component/UI/Images/NodeStates/alert-64px.png";
        private static readonly string errorGlyph = "/DynamoCoreWpf;component/UI/Images/NodeStates/error-64px.png";
        private static readonly string infoGlyph = "/DynamoCoreWpf;component/UI/Images/NodeStates/info-64px.png";
        private static readonly string previewGlyph = "/DynamoCoreWpf;component/UI/Images/NodeStates/hidden-64px.png";
        private static readonly string frozenGlyph = "/DynamoCoreWpf;component/UI/Images/NodeStates/frozen-64px.png";

        [JsonIgnore]
        public bool IsNodeAddedRecently
        {
            get
            {
                return isNodeNewlyAdded;
            }
            set
            {
                isNodeNewlyAdded = value;
            }
        }

        /// <summary>
        /// Returns a value indicating whether this model is frozen.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is frozen; otherwise, <c>false</c>.
        /// </value>
        [JsonIgnore]
        public bool IsFrozen
        {
            get
            {
                RaisePropertyChanged("IsFrozenExplicitly");
                RaisePropertyChanged("CanToggleFrozen");
                return NodeModel.IsFrozen;
            }
            set
            {
                NodeModel.IsFrozen = value;
            }
        }

        /// <summary>
        /// A flag indicating whether the node is set to freeze by the user.
        /// </summary>
        /// <value>
        ///  Returns true if the node has been frozen explicitly by the user, otherwise false.
        /// </value>  
        [JsonProperty("Excluded", Order = 5)]
        public bool IsFrozenExplicitly
        {
            get
            {
                //if the node is freeze by the user, then always
                //check the Freeze property     
                if (this.NodeLogic.isFrozenExplicitly)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// A flag indicating whether the underlying NodeModel's IsFrozen property can be toggled.      
        /// </summary>
        /// <value>
        ///  This will return false if this node is not the root of the freeze operation, otherwise it will return 
        ///  true.
        /// </value>
        [JsonIgnore]
        public bool CanToggleFrozen
        {
            get
            {
                return !NodeModel.IsAnyUpstreamFrozen();
            }
        }

        /// <summary>
        ///     Returns or set the X position of the Node.
        /// </summary>
        [JsonProperty(Order = 7)]
        public double X
        {
            get { return NodeModel.X; }
            set
            {
                NodeModel.X = value;
            }
        }

        /// <summary>
        ///     Returns or set the Y position of the Node.
        /// </summary>
        [JsonProperty(Order = 8)]
        public double Y
        {
            get { return NodeModel.Y; }
            set
            {
                NodeModel.Y = value;
            }
        }

        [JsonIgnore]
        public ImageSource ImageSource
        {
            get => imageSource;
            set
            {
                imageSource = value;
                RaisePropertyChanged(nameof(ImageSource));
            }
        }

        [JsonIgnore]
        public string ImgGlyphOneSource
        {
            get => imgGlyphOneSource;
            set
            {
                imgGlyphOneSource = value;
                RaisePropertyChanged(nameof(ImgGlyphOneSource));
            }
        }

        [JsonIgnore]
        public string ImgGlyphTwoSource
        {
            get => imgGlyphTwoSource;
            set
            {
                imgGlyphTwoSource = value;
                RaisePropertyChanged(nameof(ImgGlyphTwoSource));
            }
        }

        [JsonIgnore]
        public string ImgGlyphThreeSource
        {
            get => imgGlyphThreeSource;
            set
            {
                imgGlyphThreeSource = value;
                RaisePropertyChanged(nameof(ImgGlyphThreeSource));
            }
        }

        internal double ActualHeight { get; set; }
        internal double ActualWidth { get; set; }

        /// <summary>
        /// Node description defined by the user.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string UserDescription
        {
            get { return NodeModel.UserDescription; }
            set { NodeModel.UserDescription = value; }
        }

        public override bool IsCollapsed
        {
            get => base.IsCollapsed;
            set
            {
                base.IsCollapsed = value;
                if (ErrorBubble == null) return;
                ErrorBubble.IsCollapsed = value;
                RaisePropertyChanged(nameof(NodeWarningBarVisible));
            }
        }

        /// <summary>
        /// Used as a flag to indicate to associated connectors what ZIndex to be drawn at.
        /// </summary>
        [JsonIgnore]
        public bool IsNodeInCollapsedGroup
        {
            get => isNodeInCollapsedGroup;
            set
            {
                isNodeInCollapsedGroup = value;
                RaisePropertyChanged(nameof(IsNodeInCollapsedGroup));
            }
        }

        /// <summary>
        /// A collection of error/warning/info messages, dismissed via a sub-menu in the node Context Menu.
        /// </summary>
        [JsonIgnore]
        public ObservableCollection<string> DismissedAlerts => nodeLogic.DismissedAlerts;


        internal bool IsWatchNode
        {
            get => OriginalName.Contains(WatchNodeName);
        }
        #endregion

        #region events

        internal event NodeAutoCompletePopupEventHandler RequestAutoCompletePopupPlacementTarget;
        internal event PortContextMenuPopupEventHandler RequestPortContextMenuPopupPlacementTarget;

        internal void OnRequestAutoCompletePopupPlacementTarget(Popup popup)
        {
            RequestAutoCompletePopupPlacementTarget?.Invoke(popup);
        }

        public void OnRequestPortContextMenuPlacementTarget(Popup popup)
        {
            RequestPortContextMenuPopupPlacementTarget?.Invoke(popup);
        }

        public event NodeDialogEventHandler RequestShowNodeHelp;
        public virtual void OnRequestShowNodeHelp(Object sender, NodeDialogEventArgs e)
        {
            if (RequestShowNodeHelp != null)
            {
                RequestShowNodeHelp(this, e);
            }
        }

        public event NodeDialogEventHandler RequestShowNodeRename;
        public virtual void OnRequestShowNodeRename(Object sender, NodeDialogEventArgs e)
        {
            if (RequestShowNodeRename != null)
            {
                RequestShowNodeRename(this, e);
            }
        }

        public event EventHandler RequestsSelection;
        public virtual void OnRequestsSelection(Object sender, EventArgs e)
        {
            if (RequestsSelection != null)
            {
                RequestsSelection(this, e);
            }
        }

        /// <summary>
        /// Event to determine when Node is selected
        /// </summary>
        internal event EventHandler Selected;
        internal void OnSelected(object sender, EventArgs e)
        {
            Selected?.Invoke(this, e);
        }

        /// <summary>
        /// Event to determine when Node is removed
        /// </summary>
        internal event EventHandler Removed;
        internal void OnRemoved(object sender, EventArgs e)
        {
            Removed?.Invoke(this, e);
        }

        #endregion

        #region constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="workspaceViewModel"></param>
        /// <param name="logic"></param>
        public NodeViewModel(WorkspaceViewModel workspaceViewModel, NodeModel logic)
        {
            WorkspaceViewModel = workspaceViewModel;
            DynamoViewModel = workspaceViewModel.DynamoViewModel;

            nodeLogic = logic;
            previewPinned = logic.PreviewPinned;

            //respond to collection changed events to add
            //and remove port model views
            logic.InPorts.CollectionChanged += inports_collectionChanged;
            logic.OutPorts.CollectionChanged += outports_collectionChanged;

            logic.PropertyChanged += logic_PropertyChanged;
            logic.Infos.CollectionChanged += Infos_CollectionChanged;

            DynamoViewModel.Model.PropertyChanged += Model_PropertyChanged;
            DynamoViewModel.Model.DebugSettings.PropertyChanged += DebugSettings_PropertyChanged;

            //Do a one time setup of the initial ports on the node
            //we can not do this automatically because this constructor
            //is called after the node's constructor where the ports
            //are initially registered
            SetupInitialPortViewModels();

            if (IsDebugBuild)
            {
                DynamoViewModel.EngineController.AstBuilt += EngineController_AstBuilt;
            }

            ShowExecutionPreview = workspaceViewModel.DynamoViewModel.ShowRunPreview;
            IsNodeAddedRecently = true;
            DynamoSelection.Instance.Selection.CollectionChanged += SelectionOnCollectionChanged;
            ZIndex = ++StaticZIndex;
            ++NoteViewModel.StaticZIndex;

            if (workspaceViewModel.InCanvasSearchViewModel != null && workspaceViewModel.InCanvasSearchViewModel.TryGetNodeIcon(this, out ImageSource imgSource))
            {
                ImageSource = imgSource;
            }
            logic.NodeMessagesClearing += Logic_NodeMessagesClearing;
            logic.NodeInfoMessagesClearing += Logic_NodeInfoMessagesClearing;

            logic_PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsVisible)));
            UpdateBubbleContent();
        }

        private void Infos_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateBubbleContent();
        }


        /// <summary>
        /// Updates whether the Warning Bar is visible or not and whether the node's
        /// Frozen/Info/Warning/Error overlay is displaying.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateOverlays(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(NodeWarningBarVisible));
            RaisePropertyChanged(nameof(NodeOverlayVisible));
            RaisePropertyChanged(nameof(NodeOverlayColor));
        }
        
        /// <summary>
        /// Clears the existing messages on a node before it executes and re-evalutes its warnings/errors. 
        /// </summary>
        /// <param name="obj"></param>
        private void Logic_NodeMessagesClearing(NodeModel obj)
        {
            // Because errors are evaluated before the graph/node executes, we need to ensure 
            // errors aren't being dismissed when the graph runs.
            // Persistent warnings should also not be dismissed when a graph runs as they can include:
            // 1. Compile-time warnings in CBNs
            // 2. Obsolete nodes with warnings
            // 3. Dummy or unresolved nodes
            if (nodeLogic.State == ElementState.Error || nodeLogic.State == ElementState.PersistentWarning) return;

            // For certain nodes without ErrorBubble in place, handle color overlay specifically
            if (ErrorBubble == null)
            {
                HandleColorOverlayChange();
                return;
            }

            if (DynamoViewModel.UIDispatcher != null)
            {
                DynamoViewModel.UIDispatcher.Invoke(() =>
                {
                    ErrorBubble.NodeMessages.Clear();
                });
            }
            else
            {
                ErrorBubble.NodeMessages.Clear();
            }
        }

        /// <summary>
        /// Clears the existing messages on a node before it executes and re-evalutes its warnings/errors. 
        /// </summary>
        /// <param name="obj"></param>
        private void Logic_NodeInfoMessagesClearing(NodeModel obj)
        {
            if (ErrorBubble == null) return;

            var itemsToRemove = ErrorBubble.NodeMessages.Where(x => x.Style == InfoBubbleViewModel.Style.Info).ToList();

            if (DynamoViewModel.UIDispatcher != null)
            {
                DynamoViewModel.UIDispatcher.Invoke(() =>
                {
                    foreach (var itemToRemove in itemsToRemove)
                    {
                        ErrorBubble.NodeMessages.Remove(itemToRemove);
                    }
                });
            }
            else
            {
                foreach (var itemToRemove in itemsToRemove)
                {
                    ErrorBubble.NodeMessages.Remove(itemToRemove);
                }
            }
        }

        private void DismissedNodeMessages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!(sender is ObservableCollection<InfoBubbleDataPacket> observableCollection)) return;

            DismissedAlerts.Clear();

            foreach (InfoBubbleDataPacket infoBubbleDataPacket in observableCollection)
            {
                DismissedAlerts.Add(infoBubbleDataPacket.Message);
            }
            
            RaisePropertyChanged(nameof(DismissedAlerts));
            RaisePropertyChanged(nameof(NumberOfDismissedAlerts));

            UpdateModelDismissedAlertsCount();
        }

        /// <summary>
        /// Calls an update for the DismissedAlertCount inside the NodeModel to push PropertyChanged fire
        /// </summary>
        private void UpdateModelDismissedAlertsCount()
        {
            if (DismissedAlerts != null)
            {
                nodeLogic.DismissedAlertsCount = DismissedAlerts.Count;
            }
        }

        /// <summary>
        /// Dispose function
        /// </summary>
        public override void Dispose()
        {
            NodeModel.PropertyChanged -= logic_PropertyChanged;
            NodeModel.InPorts.CollectionChanged -= inports_collectionChanged;
            NodeModel.OutPorts.CollectionChanged -= outports_collectionChanged;
            NodeModel.Infos.CollectionChanged -= Infos_CollectionChanged;

            DynamoViewModel.Model.PropertyChanged -= Model_PropertyChanged;
            DynamoViewModel.Model.DebugSettings.PropertyChanged -= DebugSettings_PropertyChanged;
            if (IsDebugBuild)
            {
                DynamoViewModel.EngineController.AstBuilt -= EngineController_AstBuilt;
            }

            foreach (var p in InPorts)
            {
                p.Dispose();
            }

            foreach (var p in OutPorts)
            {
                p.Dispose();
            }

            NodeModel.NodeMessagesClearing -= Logic_NodeMessagesClearing;
            NodeModel.NodeInfoMessagesClearing -= Logic_NodeInfoMessagesClearing;
            
            if (ErrorBubble != null) DisposeErrorBubble();

            DynamoSelection.Instance.Selection.CollectionChanged -= SelectionOnCollectionChanged;
            base.Dispose();
        }

        public NodeViewModel(WorkspaceViewModel workspaceViewModel, NodeModel logic, Size preferredSize)
            : this(workspaceViewModel, logic)
        {
            // preferredSize is set when a node needs to have a fixed size
            PreferredSize = preferredSize;
        }

        private void SelectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CreateGroupCommand.RaiseCanExecuteChanged();
            AddToGroupCommand.RaiseCanExecuteChanged();
            UngroupCommand.RaiseCanExecuteChanged();
            ToggleIsFrozenCommand.RaiseCanExecuteChanged();
            RaisePropertyChanged("IsFrozenExplicitly");
            RaisePropertyChanged("CanToggleFrozen");
        }

        void DebugSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShowDebugASTs")
            {
                RaisePropertyChanged("ShowDebugASTs");
            }
        }

        /// <summary>
        /// Handler for the EngineController's AstBuilt event.
        /// Formats a string of AST for preview on the node.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EngineController_AstBuilt(object sender, CompiledEventArgs e)
        {
            if (e.Node == nodeLogic.GUID)
            {
                var sb = new StringBuilder();
                sb.AppendLine(string.Format("{0} AST:", e.Node));

                foreach (var assocNode in e.AstNodes)
                {
                    var pretty = assocNode.ToString();

                    //shorten the guids
                    var strRegex = @"([0-9a-f-]{32}).*?";
                    var myRegex = new Regex(strRegex, RegexOptions.None);
                    string strTargetString = assocNode.ToString();

                    foreach (Match myMatch in myRegex.Matches(strTargetString))
                    {
                        if (myMatch.Success)
                        {
                            pretty = pretty.Replace(myMatch.Value, "..." + myMatch.Value.Substring(myMatch.Value.Length - 7));
                        }
                    }
                    sb.AppendLine(pretty);
                }

                ASTText = sb.ToString();
            }
        }

        #endregion

        /// <summary>
        /// Do a one setup of the ports 
        /// </summary>
        private void SetupInitialPortViewModels()
        {
            foreach (var item in nodeLogic.InPorts)
            {
                PortViewModel inportViewModel = SubscribeInPortEvents(item);
                InPorts.Add(inportViewModel);
            }

            foreach (var item in nodeLogic.OutPorts)
            {
                PortViewModel outportViewModel = SubscribeOutPortEvents(item);
                OutPorts.Add(outportViewModel);
            }
        }


        /// <summary>
        /// Respond to property changes on the Dynamo model
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentWorkspace":
                    RaisePropertyChanged("NodeVisibility");
                    break;
            }
        }

        /// <summary>
        /// Respond to property changes on the node model.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void logic_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Name":
                    RaisePropertyChanged("Name");
                    break;
                case "X":
                    RaisePropertyChanged("Left");
                    UpdateErrorBubblePosition();
                    break;
                case "Y":
                    RaisePropertyChanged("Top");
                    UpdateErrorBubblePosition();
                    break;
                case "InteractionEnabled":
                    RaisePropertyChanged("IsInteractionEnabled");
                    break;
                case "IsSelected":
                    RaisePropertyChanged("IsSelected");
                    RaisePropertyChanged("PreviewState");
                    break;
                case "State":
                    RaisePropertyChanged("State");
                    HandleColorOverlayChange();
                    RaisePropertyChanged(nameof(NodeWarningBarVisible));
                    break;
                case "ArgumentLacing":
                    RaisePropertyChanged("ArgumentLacing");
                    break;
                case nameof(NodeModel.ToolTipText):
                    UpdateBubbleContent();
                    // TODO Update preview bubble visibility to false
                    break;
                case "IsVisible":
                    RaisePropertyChanged("IsVisible");
                    HandleColorOverlayChange();
                    RaisePropertyChanged(nameof(NodeWarningBarVisible));
                    break;
                case "Width":
                    RaisePropertyChanged("Width");
                    UpdateErrorBubbleWidth();
                    UpdateErrorBubblePosition();
                    break;
                case "Height":
                    RaisePropertyChanged("Height");
                    UpdateErrorBubblePosition();
                    break;
                case nameof(NodeModel.DisplayLabels):
                    RaisePropertyChanged(nameof(IsDisplayingLabels));
                    break;
                case nameof(NodeModel.IsSetAsInput):
                    RaisePropertyChanged(nameof(IsSetAsInput));
                    break;
                case nameof(NodeModel.IsSetAsOutput):
                    RaisePropertyChanged(nameof(IsSetAsOutput));
                    break;
                case "Position":
                    UpdateErrorBubblePosition();
                    break;
                case "ForceReExecuteOfNode":
                    RaisePropertyChanged("WillForceReExecuteOfNode");
                    break;
                case "CanUpdatePeriodically":
                    RaisePropertyChanged("EnablePeriodicUpdate");
                    RaisePropertyChanged("PeriodicUpdateVisibility");
                    break;
                case "IsFrozen":
                    RaiseFrozenPropertyChanged();
                    HandleColorOverlayChange();
                    RaisePropertyChanged(nameof(NodeOverlayVisible));
                    break;
            }
        }

        /// <summary>
        /// Respond to property changes on the error bubble.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ErrorBubble_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ErrorBubble.DoesNodeDisplayMessages):
                    HandleColorOverlayChange();
                    RaisePropertyChanged(nameof(NodeWarningBarVisible));
                    break;
            }
        }

        /// <summary>
        /// A single method that handles all color-related overrides
        /// The following events trigger color update:
        /// Error, Warning, Frozen, PreviewOff, Info 
        /// </summary>
        private void HandleColorOverlayChange()
        {
            WarningBarColor = GetWarningColor();
            NodeOverlayColor = GetBorderColor();
        }

        /// <summary>
        /// Updates the width of the node's Warning/Error bubbles, in case the width of the node changes.
        /// </summary>
        private void UpdateErrorBubbleWidth()
        {
            ErrorBubble.BubbleWidth = NodeModel.Width;
        }

        /// <summary>
        /// Creates a new ErrorBubble and assigns it to the ErrorBubble property.
        /// </summary>
        private void BuildErrorBubble()
        {
            if (ErrorBubble == null) ErrorBubble = new InfoBubbleViewModel(this)
            {
                IsCollapsed = this.IsCollapsed
            };

            ErrorBubble.NodeInfoToDisplay.CollectionChanged += UpdateOverlays;
            ErrorBubble.NodeWarningsToDisplay.CollectionChanged += UpdateOverlays;
            ErrorBubble.NodeErrorsToDisplay.CollectionChanged += UpdateOverlays;
            ErrorBubble.PropertyChanged += ErrorBubble_PropertyChanged;
            
            if (DynamoViewModel.UIDispatcher != null)
            {
                DynamoViewModel.UIDispatcher.Invoke(() =>
                {
                    WorkspaceViewModel.Errors.Add(ErrorBubble);
                });
            }
            
            // The Error bubble sits above the node in ZIndex. Since pinned notes sit above
            // the node as well and the ErrorBubble needs to display on top of these, the
            // ErrorBubble's ZIndex should be the node's ZIndex + 2.
            ErrorBubble.ZIndex = ZIndex + 2;

            // The Node displays a count of dismissed messages, listening to that collection in the node's ErrorBubble
            
            ErrorBubble.DismissedMessages.CollectionChanged += DismissedNodeMessages_CollectionChanged;
        }

        // These colors are duplicated from the DynamoColorsAndBrushesDictionary as it is not assumed that the xaml will be loaded before setting the color
        // SharedDictionaryManager.DynamoColorsAndBrushesDictionary["NodeErrorColor"];
        private static SolidColorBrush errorColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#EB5555"));
        // SharedDictionaryManager.DynamoColorsAndBrushesDictionary["NodeWarningColor"];
        private static SolidColorBrush warningColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FAA21B"));
        private static SolidColorBrush infoColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#6AC0E7"));
        // SharedDictionaryManager.DynamoColorsAndBrushesDictionary["NodePreviewColor"];
        private static SolidColorBrush noPreviewColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#BBBBBB"));

        /// <summary>
        /// Sets the color of the warning bar, which informs the user that the node is in
        /// either an error or a warning state.
        /// </summary>
        /// <param name="style"></param>
        /// <returns></returns>
        internal SolidColorBrush GetWarningColor()
        {
            if (nodeLogic.IsInErrorState)
            {
                return errorColor;
            }

            if (NodeModel.State == ElementState.Warning || NodeModel.State == ElementState.PersistentWarning)
            {
                //Handle the case where the user has dismissed the warning and no warnings are showing.
                if (ErrorBubble != null && !ErrorBubble.DoesNodeDisplayMessages)
                {
                    return noPreviewColor;
                }

                return warningColor;
            }

            if (NodeModel.State == ElementState.Info)
            {
                return infoColor;
            }

            return noPreviewColor;
        }
        /// <summary>
        /// Determines the color of the overlay border based on the
        /// state of the node. Priorities apply in order of appearance in if/else statement.
        /// Applicable if zoom is smaller than 60% (State 2)
        /// TODO: Can be done with switch case statement if we refactor node view states
        /// </summary>
        /// <returns></returns>
        internal SolidColorBrush GetBorderColor()
        {
            SolidColorBrush result = null;

            /*
                Priorities seem to be:
                Error > Warning > Info ; Frozen > Preview > None
                Pass through all possible states in reverse order 
                to assign icon values for each possible scenario
            */

            ResetColorGlyphs();

            if (!this.IsVisible)
            {
                result = (SolidColorBrush)SharedDictionaryManager.DynamoColorsAndBrushesDictionary["NodePreviewColor"];
                ImgGlyphOneSource = previewGlyph; 
            }
            if (this.IsFrozen)
            {
                result = (SolidColorBrush)SharedDictionaryManager.DynamoColorsAndBrushesDictionary["NodeFrozenOverlayColor"]; 
                if (ImgGlyphOneSource == null)
                {
                    ImgGlyphOneSource = frozenGlyph;
                }
                else
                {
                    ImgGlyphOneSource = frozenGlyph;
                    ImgGlyphTwoSource = previewGlyph;
                }
            }
            if (NodeModel.State == ElementState.Info)
            {
                result = (SolidColorBrush)SharedDictionaryManager.DynamoColorsAndBrushesDictionary["NodeInfoColor"];
                if (ImgGlyphTwoSource == null)
                {
                    ImgGlyphTwoSource = infoGlyph;
                }
                else
                {
                    ImgGlyphThreeSource = infoGlyph;
                }
            }

            if (NodeModel.State == ElementState.Warning || NodeModel.State == ElementState.PersistentWarning)
            {
                result = warningColor;
                if (ImgGlyphTwoSource == null)
                {
                    ImgGlyphTwoSource = warningGlyph;
                }
                else
                {
                    ImgGlyphThreeSource = warningGlyph;
                }
            }
            if (NodeModel.State == ElementState.Error)
            {
                result = errorColor;
                if (ImgGlyphTwoSource == null)
                {
                    ImgGlyphTwoSource = errorGlyph;
                }
                else
                {
                    ImgGlyphThreeSource = errorGlyph;
                }
            }

            return result;
        }
        private void ResetColorGlyphs()
        {
            ImgGlyphOneSource = null;
            ImgGlyphTwoSource = null;
            ImgGlyphThreeSource = null;
        }

        /// <summary>
        /// Disposes the ErrorBubble when it's no longer needed.
        /// </summary>
        private void DisposeErrorBubble()
        {
            if (ErrorBubble == null) return;
            ErrorBubble.DismissedMessages.CollectionChanged -= DismissedNodeMessages_CollectionChanged;

            if (DynamoViewModel.UIDispatcher != null)
            {
                DynamoViewModel.UIDispatcher.Invoke(() =>
                {
                    ErrorBubble.NodeMessages.Clear();
                });
            }

            ErrorBubble.NodeInfoToDisplay.CollectionChanged -= UpdateOverlays;
            ErrorBubble.NodeWarningsToDisplay.CollectionChanged -= UpdateOverlays;
            ErrorBubble.NodeErrorsToDisplay.CollectionChanged -= UpdateOverlays;
            ErrorBubble.PropertyChanged -= ErrorBubble_PropertyChanged;

            ErrorBubble.Dispose();
        }

        public void UpdateBubbleContent()
        {
            if (DynamoViewModel == null) return;

            bool hasErrorOrWarning = NodeModel.IsInErrorState || NodeModel.State == ElementState.Warning; 
            bool isNodeStateInfo = NodeModel.State == ElementState.Info;

            // Persistent warnings should continue to be displayed even if nodes are not involved in an execution as they can include:
            // 1. Compile-time warnings in CBNs
            // 2. Obsolete nodes with warnings
            // 3. Dummy or unresolved nodes
            if (NodeModel.State != ElementState.PersistentWarning && !NodeModel.IsInErrorState && !isNodeStateInfo)
            {
                if (!NodeModel.WasInvolvedInExecution || !hasErrorOrWarning) return;
            }

            if (NodeModel.Infos.Count == 0) return;

            if (ErrorBubble == null) BuildErrorBubble();

            if (!WorkspaceViewModel.Errors.Contains(ErrorBubble)) return;

            var topLeft = new Point(NodeModel.X, NodeModel.Y);
            var botRight = new Point(NodeModel.X + NodeModel.Width, NodeModel.Y + NodeModel.Height);

            const InfoBubbleViewModel.Direction connectingDirection = InfoBubbleViewModel.Direction.Bottom;
            var packets = new List<InfoBubbleDataPacket>(NodeModel.Infos.Count);

            InfoBubbleViewModel.Style style = InfoBubbleViewModel.Style.None;
            int styleRank = int.MaxValue;

            foreach (var info in NodeModel.Infos)
            {
                var infoStyle = info.State == ElementState.Error ? InfoBubbleViewModel.Style.Error : InfoBubbleViewModel.Style.Warning;
                infoStyle = info.State == ElementState.Info ? InfoBubbleViewModel.Style.Info : infoStyle;

                // Set the info bubble style based on the heirarchy of node messages style. 1) Error 2) Warning 3) Info.
                if (infoStyle == InfoBubbleViewModel.Style.Info && styleRank > 3)
                {
                    style = InfoBubbleViewModel.Style.Info;
                    styleRank = 3;
                }
                
                if (infoStyle == InfoBubbleViewModel.Style.Warning && styleRank > 2)
                {
                    style = InfoBubbleViewModel.Style.Warning;
                    styleRank = 2;
                }

                if (infoStyle == InfoBubbleViewModel.Style.Error)
                {
                    style = InfoBubbleViewModel.Style.Error;
                    styleRank = 1;
                }

                var data = new InfoBubbleDataPacket(infoStyle, topLeft, botRight, info.Message, connectingDirection);
                packets.Add(data);
            }

            ErrorBubble.InfoBubbleStyle = style;

            // If running Dynamo with UI, use dispatcher, otherwise not
            if (DynamoViewModel.UIDispatcher != null)
            {
                DynamoViewModel.UIDispatcher.Invoke(() =>
                {
                    foreach (var data in packets)
                    {
                        if (!ErrorBubble.NodeMessages.Contains(data))
                        {
                            ErrorBubble.NodeMessages.Add(data);
                        }
                    }
                    HandleColorOverlayChange();
                });
            }
            else
            {
                foreach (var data in packets)
                {
                    if (!ErrorBubble.NodeMessages.Contains(data))
                    {
                        ErrorBubble.NodeMessages.Add(data);
                    }
                }
                HandleColorOverlayChange();
            }
            ErrorBubble.ChangeInfoBubbleStateCommand.Execute(InfoBubbleViewModel.State.Pinned);            
        }

        private void UpdateErrorBubblePosition()
        {
            if (ErrorBubble == null)
                return;
            var data = new InfoBubbleDataPacket
            {
                TopLeft = GetTopLeft(),
                BotRight = GetBotRight()
            };
            ErrorBubble.UpdatePositionCommand.Execute(data);
        }

        private void ShowHelp(object parameter)
        {
            OnRequestShowNodeHelp(this, new NodeDialogEventArgs(NodeModel));
            Analytics.TrackEvent(Actions.ViewDocumentation, Categories.NodeContextMenuOperations, NodeModel.Name);
        }

        private bool CanShowHelp(object parameter)
        {
            return true;
        }

        private void ShowRename(object parameter)
        {
            OnRequestShowNodeRename(this, new NodeDialogEventArgs(NodeModel));
            Analytics.TrackEvent(Actions.Rename, Categories.NodeContextMenuOperations);
        }

        private bool CanShowRename(object parameter)
        {
            return true;
        }

        private bool CanDeleteNode(object parameter)
        {
            return true;
        }

        private void DeleteNodeAndItsConnectors(object parameter)
        {
            var command = new DynamoModel.DeleteModelCommand(nodeLogic.GUID);
            DynamoViewModel.ExecuteCommand(command);
            OnRemoved(this, EventArgs.Empty);
            Analytics.TrackEvent(Actions.Delete, Categories.NodeContextMenuOperations, nodeLogic.Name);
        }

        private void SetLacingType(object param)
        {
            DynamoViewModel.ExecuteCommand(
                new DynamoModel.UpdateModelValueCommand(
                    Guid.Empty, NodeModel.GUID, nameof(ArgumentLacing), param.ToString()));

            DynamoViewModel.RaiseCanExecuteUndoRedo();

            Analytics.TrackEvent(Actions.Set, Categories.NodeContextMenuOperations, "Lacing");
        }

        private bool CanSetLacingType(object param)
        {
            // Only allow setting of lacing strategy when it is not disabled.
            return (ArgumentLacing != LacingStrategy.Disabled);
        }

        private void ViewCustomNodeWorkspace(object parameter)
        {
            var f = (nodeLogic as Function);
            if (f != null)
                DynamoViewModel.FocusCustomNodeWorkspace(f.Definition.FunctionId);
        }

        private bool CanViewCustomNodeWorkspace(object parameter)
        {
            return nodeLogic.IsCustomFunction;
        }

        private void inports_collectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //The visual height of the node is bound to preferred height.
            //PreferredHeight = Math.Max(inPorts.Count * 20 + 10, outPorts.Count * 20 + 10); //spacing for inputs + title space + bottom space

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                //create a new port view model
                foreach (var item in e.NewItems)
                {
                    PortViewModel inportViewModel = SubscribeInPortEvents(item as PortModel);
                    InPorts.Add(inportViewModel);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                //remove the port view model whose model item
                //is the one passed in
                foreach (var item in e.OldItems)
                {
                    PortViewModel portToRemove = UnSubscribePortEvents(InPorts.ToList().First(x => x.PortModel == item)); ;
                    InPorts.Remove(portToRemove);
                    portToRemove.Dispose();
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var p in InPorts)
                {
                    UnSubscribePortEvents(p);
                    p.Dispose();
                }
                InPorts.Clear();
            }
        }

        private void outports_collectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //The visual height of the node is bound to preferred height.
            //PreferredHeight = Math.Max(inPorts.Count * 20 + 10, outPorts.Count * 20 + 10); //spacing for inputs + title space + bottom space

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                //create a new port view model
                foreach (var item in e.NewItems)
                {
                    PortViewModel outportViewModel = SubscribeOutPortEvents(item as PortModel);
                    OutPorts.Add(outportViewModel);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                //remove the port view model whose model item is the
                //one passed in
                foreach (var item in e.OldItems)
                {
                    PortViewModel portToRemove = UnSubscribePortEvents(OutPorts.ToList().First(x => x.PortModel == item));
                    OutPorts.Remove(portToRemove);
                    portToRemove.Dispose();
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var p in OutPorts)
                {
                    UnSubscribePortEvents(p);
                    p.Dispose();
                }
                OutPorts.Clear();
            }
        }


        /// <summary>
        /// Registers the in port events.
        /// </summary>
        /// <param name="item">PortModel.</param>
        /// <returns></returns>
        protected virtual PortViewModel SubscribeInPortEvents(PortModel item)
        {
            InPortViewModel portViewModel = new InPortViewModel(this, item);
            portViewModel.MouseEnter += OnRectangleMouseEnter;
            portViewModel.MouseLeave += OnRectangleMouseLeave;
            portViewModel.MouseLeftButtonDown += OnMouseLeftButtonDown;
            return portViewModel;
        }

        /// <summary>
        /// Registers the out port events.
        /// </summary>
        /// <param name="item">PortModel.</param>
        /// <returns></returns>
        protected virtual PortViewModel SubscribeOutPortEvents(PortModel item)
        {
            OutPortViewModel portViewModel = new OutPortViewModel(this, item);
            portViewModel.MouseEnter += OnRectangleMouseEnter;
            portViewModel.MouseLeave += OnRectangleMouseLeave;
            portViewModel.MouseLeftButtonDown += OnMouseLeftButtonDown;
            return portViewModel;
        }


        /// <summary>
        /// Unsubscribe port events.
        /// </summary>
        /// <param name="item">The PortViewModel.</param>
        /// <returns></returns>
        private PortViewModel UnSubscribePortEvents(PortViewModel item)
        {
            item.MouseEnter -= OnRectangleMouseEnter;
            item.MouseLeave -= OnRectangleMouseLeave;
            item.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            return item;
        }


        /// <summary>
        /// Handles the MouseLeftButtonDown event of the port control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnMouseLeftButtonDown(object sender, EventArgs e)
        {
            PortViewModel portViewModel = sender as PortViewModel;
            portViewModel.EventType = PortEventType.MouseLeftButtonDown;
            if (SnapInputEvent != null)
                SnapInputEvent(portViewModel);
        }

        /// <summary>
        /// Handles the MouseLeave event of the port control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnRectangleMouseLeave(object sender, EventArgs e)
        {
            PortViewModel portViewModel = sender as PortViewModel;
            portViewModel.EventType = PortEventType.MouseLeave;
            if (SnapInputEvent != null)
                SnapInputEvent(portViewModel);
        }

        /// <summary>
        /// Handles the MouseEnter event of the port control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnRectangleMouseEnter(object sender, EventArgs e)
        {
            PortViewModel portViewModel = sender as PortViewModel;
            portViewModel.EventType = PortEventType.MouseEnter;
            if (SnapInputEvent != null)
                SnapInputEvent(portViewModel);
        }


        private void ToggleIsVisible(object parameter)
        {
            // Invert the visibility before setting the value
            var visibility = (!nodeLogic.IsVisible).ToString();
            var command = new DynamoModel.UpdateModelValueCommand(Guid.Empty,
                new[] { nodeLogic.GUID }, "IsVisible", visibility);

            DynamoViewModel.Model.ExecuteCommand(command);
            DynamoViewModel.RaiseCanExecuteUndoRedo();

            Analytics.TrackEvent(Actions.Preview, Categories.NodeContextMenuOperations, visibility);
        }

        private bool CanVisibilityBeToggled(object parameter)
        {
            return true;
        }

        private void SetState(object parameter)
        {
            nodeLogic.State = (ElementState)parameter;
        }

        private bool CanSetState(object parameter)
        {
            if (parameter is ElementState)
                return true;
            return false;
        }

        private void Select(object parameter)
        {
            //this logic has been moved to the view
            //because it depends on Keyboard modifiers.

            //if (!nodeLogic.IsSelected)
            //{
            //    if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
            //    {
            //        DynamoSelection.Instance.ClearSelection();
            //    }

            //    if (!DynamoSelection.Instance.Selection.Contains(nodeLogic))
            //        DynamoSelection.Instance.Selection.Add(nodeLogic);
            //}
            //else
            //{
            //    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            //    {
            //        DynamoSelection.Instance.Selection.Remove(nodeLogic);
            //    }
            //}

            //if the node is not already selected
            //then clear the selection

            OnRequestsSelection(this, EventArgs.Empty);
        }

        private bool CanSelect(object parameter)
        {
            return true;
        }

        private void SetModelSize(object parameter)
        {
            var size = parameter as double[];
            NodeModel.SetSize(size[0], size[1]);
        }

        private bool CanSetModelSize(object parameter)
        {
            var size = parameter as double[];
            if (size == null)
            {
                return false;
            }

            return NodeModel.Width != size[0] || NodeModel.Height != size[1];
        }

        private void GotoWorkspace(object parameters)
        {
            DynamoViewModel.GoToWorkspace((NodeLogic as Function).Definition.FunctionId);
        }

        private bool CanGotoWorkspace(object parameters)
        {
            if (NodeLogic is Function)
            {
                return true;
            }

            return false;
        }

        private void CreateGroup(object parameters)
        {
            DynamoViewModel.AddAnnotationCommand.Execute(null);
            Analytics.TrackEvent(Actions.Create, Categories.NodeContextMenuOperations, "Group");
        }

        private bool CanCreateGroup(object parameters)
        {
            var groups = WorkspaceViewModel.Model.Annotations;
            //Create Group should be disabled when a group is selected
            if (groups.Any(x => x.IsSelected))
            {
                return false;
            }

            //Create Group should be disabled when a node selected is already in a group
            if (!groups.Any(x => x.IsSelected))
            {
                var modelSelected = DynamoSelection.Instance.Selection.OfType<ModelBase>().Where(x => x.IsSelected);
                foreach (var model in modelSelected)
                {
                    if (groups.ContainsModel(model.GUID))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void ToggleIsFrozen(object parameters)
        {
            var node = this.nodeLogic;
            if (node != null)
            {
                var oldFrozen = (!node.isFrozenExplicitly).ToString();
                var command = new DynamoModel.UpdateModelValueCommand(Guid.Empty,
                    new[] { node.GUID }, "IsFrozen", oldFrozen);

                DynamoViewModel.Model.ExecuteCommand(command);
            }
            else if (DynamoSelection.Instance.Selection.Any())
            {
                node = DynamoSelection.Instance.Selection.Cast<NodeModel>().First();
                node.IsFrozen = !node.IsFrozen;
            }

            RaiseFrozenPropertyChanged();

            Analytics.TrackEvent(Actions.Freeze, Categories.NodeContextMenuOperations);
        }

        private void RaiseFrozenPropertyChanged()
        {
            RaisePropertyChanged("IsFrozen");
            RaisePropertyChangedOnDownStreamNodes();
        }

        /// <summary>
        /// When a node is frozen, raise the IsFrozen property changed event on
        /// all its downstream nodes, to ensure UI updates correctly.
        /// </summary>
        private void RaisePropertyChangedOnDownStreamNodes()
        {
            HashSet<NodeModel> nodes = new HashSet<NodeModel>();
            this.nodeLogic.GetDownstreamNodes(this.nodeLogic, nodes);

            foreach (var inode in nodes)
            {
                var current = this.WorkspaceViewModel.Nodes.FirstOrDefault(x => x.NodeLogic == inode);
                if (current != null)
                {
                    current.RaisePropertyChanged("IsFrozen");
                }
            }
        }

        private void UngroupNode(object parameters)
        {
            WorkspaceViewModel.DynamoViewModel.UngroupModelCommand.Execute(null);
            Analytics.TrackEvent(Actions.RemovedFrom, Categories.NodeContextMenuOperations, "Node");
        }

        private bool CanUngroupNode(object parameters)
        {
            var groups = WorkspaceViewModel.Model.Annotations;
            if (!groups.Any(x => x.IsSelected))
            {
                return (groups.ContainsModel(NodeLogic.GUID));
            }
            return false;
        }

        private void AddToGroup(object parameters)
        {
            WorkspaceViewModel.DynamoViewModel.AddModelsToGroupModelCommand.Execute(null);
            Analytics.TrackEvent(Actions.AddedTo, Categories.NodeContextMenuOperations, "Node");
        }

        private bool CanAddToGroup(object parameters)
        {
            var selectedGroups = WorkspaceViewModel.Model.Annotations
                .Where(x => x.IsSelected);

            if (selectedGroups.Any() && 
                !selectedGroups.All(x => !x.IsExpanded)) 
            {
                return !(WorkspaceViewModel.Model.Annotations.ContainsModel(NodeLogic.GUID));
            }
            return false;
        }

        

        private void SelectUpstreamNeighbours(object parameters)
        {
            NodeModel.SelectUpstreamNeighbours();

            var upstreamNodes = NodeModel
                .AllUpstreamNodes(new List<NodeModel>())
                .ToList();

            upstreamNodes.Add(NodeModel);

            SelectRelatedGroupsToNodes(upstreamNodes);
        }
        
        private void SelectDownstreamNeighbours(object parameters)
        {
            NodeModel.SelectDownstreamNeighbours();

            var downstreamNodes = NodeModel
                .AllDownstreamNodes(new List<NodeModel>())
                .ToList();

            downstreamNodes.Add(NodeModel);

            SelectRelatedGroupsToNodes(downstreamNodes);
        }

        private void SelectDownstreamAndUpstreamNeighbours(object parameters)
        {
            NodeModel.SelectUpstreamAndDownstreamNeighbours();
            
            var nodesSelected = NodeModel
                .AllUpstreamNodes(new List<NodeModel>())
                .ToList();

            nodesSelected.AddRange(NodeModel
                .AllDownstreamNodes(new List<NodeModel>())
                .ToList());

            nodesSelected.Add(NodeModel);

            SelectRelatedGroupsToNodes(nodesSelected);

        }


        private void SelectRelatedGroupsToNodes(List<NodeModel> nodes)
        {
            var nodesGUIDS = nodes.Select(n => n.GUID);
            var groups = WorkspaceViewModel.Annotations;
            foreach (var group in groups)
            {
                var groupsNodesGUIDS = group.Nodes.Select(n => n.GUID);

                if (groupsNodesGUIDS.Intersect(nodesGUIDS).Any())
                    group.AddGroupAndGroupedNodesToSelection();
            }
        }
        
        #region Private Helper Methods
        internal Point GetTopLeft()
        {
            return new Point(NodeModel.X, NodeModel.Y);
        }

        internal Point GetBotRight()
        {
            return new Point(NodeModel.X + NodeModel.Width, NodeModel.Y + NodeModel.Height);
        }
        #endregion

    }

    public class NodeDialogEventArgs : EventArgs
    {
        public NodeModel Model { get; set; }
        public bool Handled { get; set; }
        public NodeDialogEventArgs(NodeModel model)
        {
            Model = model;
            Handled = false;
        }
    }
}
