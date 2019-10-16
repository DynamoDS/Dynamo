using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using Dynamo.Configuration;
using Dynamo.Engine.CodeGeneration;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.Wpf.ViewModels.Core;
using Newtonsoft.Json;

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
        #endregion

        #region public members

        /// <summary>
        /// Returns NodeModel ID
        /// </summary>
        [JsonConverter(typeof(IdToGuidConverter))]
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
                    nodeLogic.IsSetAsInput = value;
                    RaisePropertyChanged("IsSetAsInput");
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
                    nodeLogic.IsSetAsOutput = value;
                    RaisePropertyChanged("IsSetAsOutput");
                }
            }
        }
        /// <summary>
        /// The Name of the nodemodel this view points to
        /// this is the name of the node as it is displayed in the UI.
        /// </summary>
        public string Name
        {
            get { return nodeLogic.Name; }
            set { nodeLogic.Name = value; }
        }

        [JsonIgnore]
        public ElementState State
        {
            get { return nodeLogic.State; }
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

        [JsonProperty("ShowGeometry")]
        public bool IsVisible
        {
            get
            {
                return nodeLogic.IsVisible;
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
                nodeLogic.DisplayLabels = value;
                RaisePropertyChanged("IsDisplayingLabels");
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
        [JsonProperty("Excluded")]
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
        public double Y
        {
            get { return NodeModel.Y; }
            set
            {
                NodeModel.Y = value;
            }
        }

        #endregion

        #region events
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

        #endregion

        #region constructors

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

            DynamoViewModel.Model.PropertyChanged += Model_PropertyChanged;
            DynamoViewModel.Model.DebugSettings.PropertyChanged += DebugSettings_PropertyChanged;

            ErrorBubble = new InfoBubbleViewModel(DynamoViewModel);
            UpdateBubbleContent();

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
        }

        /// <summary>
        /// Dispose function
        /// </summary>
        public override void Dispose()
        {
            this.NodeModel.PropertyChanged -= logic_PropertyChanged;

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
            ErrorBubble.Dispose();
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
                PortViewModel inportViewModel = SubscribePortEvents(item);
                InPorts.Add(inportViewModel);
            }

            foreach (var item in nodeLogic.OutPorts)
            {
                PortViewModel outportViewModel = SubscribePortEvents(item);
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
                    break;
                case "ArgumentLacing":
                    RaisePropertyChanged("ArgumentLacing");
                    break;
                case "ToolTipText":
                    UpdateBubbleContent();
                    // TODO Update preview bubble visibility to false
                    break;
                case "IsVisible":
                    RaisePropertyChanged("IsVisible");
                    break;
                case "Width":
                    RaisePropertyChanged("Width");
                    UpdateErrorBubblePosition();
                    break;
                case "Height":
                    RaisePropertyChanged("Height");
                    UpdateErrorBubblePosition();
                    break;
                case "DisplayLabels":
                    RaisePropertyChanged("IsDisplayingLables");
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
                    break;
            }
        }

        public void UpdateBubbleContent()
        {
            if (ErrorBubble == null || DynamoViewModel == null)
                return;
            if (string.IsNullOrEmpty(NodeModel.ToolTipText))
            {
                if (NodeModel.State != ElementState.Error && NodeModel.State != ElementState.Warning)
                {
                    ErrorBubble.ChangeInfoBubbleStateCommand.Execute(InfoBubbleViewModel.State.Minimized);
                }
            }
            else
            {
                if (!WorkspaceViewModel.Errors.Contains(ErrorBubble))
                    return;

                var topLeft = new Point(NodeModel.X, NodeModel.Y);
                var botRight = new Point(NodeModel.X + NodeModel.Width, NodeModel.Y + NodeModel.Height);
                InfoBubbleViewModel.Style style = NodeModel.State == ElementState.Error
                    ? InfoBubbleViewModel.Style.ErrorCondensed
                    : InfoBubbleViewModel.Style.WarningCondensed;
                // NOTE!: If tooltip is not cached here, it will be cleared once the dispatcher is invoked below
                string content = NodeModel.ToolTipText;
                const InfoBubbleViewModel.Direction connectingDirection = InfoBubbleViewModel.Direction.Bottom;
                var data = new InfoBubbleDataPacket(style, topLeft, botRight, content, connectingDirection);

                ErrorBubble.UpdateContentCommand.Execute(data);
                ErrorBubble.ChangeInfoBubbleStateCommand.Execute(InfoBubbleViewModel.State.Pinned);
            }
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
            //var helpDialog = new NodeHelpPrompt(this.NodeModel);
            //helpDialog.Show();

            OnRequestShowNodeHelp(this, new NodeDialogEventArgs(NodeModel));
        }

        private bool CanShowHelp(object parameter)
        {
            return true;
        }

        private void ShowRename(object parameter)
        {
            OnRequestShowNodeRename(this, new NodeDialogEventArgs(NodeModel));
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
        }

        private void SetLacingType(object param)
        {
            DynamoViewModel.ExecuteCommand(
                new DynamoModel.UpdateModelValueCommand(
                    Guid.Empty, NodeModel.GUID, "ArgumentLacing", param.ToString()));

            DynamoViewModel.RaiseCanExecuteUndoRedo();
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
                    PortViewModel inportViewModel = SubscribePortEvents(item as PortModel);
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
                    PortViewModel outportViewModel = SubscribePortEvents(item as PortModel);
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
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var p in OutPorts)
                {
                    UnSubscribePortEvents(p);
                }
                OutPorts.Clear();
            }
        }


        /// <summary>
        /// Registers the port events.
        /// </summary>
        /// <param name="item">PortModel.</param>
        /// <returns></returns>
        private PortViewModel SubscribePortEvents(PortModel item)
        {
            PortViewModel portViewModel = new PortViewModel(this, item);
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
        }

        private bool CanToggleIsFrozen(object parameters)
        {
            return DynamoSelection.Instance.Selection.Count() == 1;
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
        }

        private bool CanAddToGroup(object parameters)
        {
            var groups = WorkspaceViewModel.Model.Annotations;
            if (groups.Any(x => x.IsSelected))
            {
                return !(groups.ContainsModel(NodeLogic.GUID));
            }
            return false;
        }


        #region Private Helper Methods
        private Point GetTopLeft()
        {
            return new Point(NodeModel.X, NodeModel.Y);
        }

        private Point GetBotRight()
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
