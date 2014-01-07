using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Collections.ObjectModel;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using System.Windows;
using Dynamo.Core;
using DynCmd = Dynamo.ViewModels.DynamoViewModel;

namespace Dynamo.ViewModels
{
    /// <summary>
    /// Interaction logic for dynControl.xaml
    /// </summary>

    public partial class NodeViewModel : ViewModelBase
    {
        #region delegates
        public delegate void SetToolTipDelegate(string message);
        public delegate void NodeHelpEventHandler(object sender, NodeHelpEventArgs e);
        #endregion

        #region private members

        ObservableCollection<PortViewModel> inPorts = new ObservableCollection<PortViewModel>();
        ObservableCollection<PortViewModel> outPorts = new ObservableCollection<PortViewModel>();
        NodeModel nodeLogic;
        private bool isFullyConnected = false;
        private double zIndex = 3;

        #endregion

        #region public members

        public NodeModel NodeModel { get { return nodeLogic; } private set { nodeLogic = value; } }

        public bool IsFullyConnected
        {
            get { return isFullyConnected; }
            set
            {
                isFullyConnected = value;
                RaisePropertyChanged("IsFullyConnected");
            }
        }

        public LacingStrategy ArgumentLacing
        {
            get { return nodeLogic.ArgumentLacing; }
            set
            {
                nodeLogic.ArgumentLacing = value;
                RaisePropertyChanged("ArgumentLacing");
            }
        }

        public NodeModel NodeLogic
        {
            get { return nodeLogic; }
        }

        public InfoBubbleViewModel ErrorBubble { get; set; }

        public InfoBubbleViewModel PreviewBubble { get; set; }

        public string ToolTipText
        {
            get { return nodeLogic.ToolTipText; }
        }

        public ObservableCollection<PortViewModel> InPorts
        {
            get { return inPorts; }
            set
            {
                inPorts = value;
                RaisePropertyChanged("InPorts");
            }
        }

        public ObservableCollection<PortViewModel> OutPorts
        {
            get { return outPorts; }
            set
            {
                outPorts = value;
                RaisePropertyChanged("OutPorts");
            }
        }

        public bool IsSelected
        {
            get { return nodeLogic.IsSelected; }
        }

        public string NickName
        {
            get { return nodeLogic.NickName; }
            set { nodeLogic.NickName = value; }
        }

        public string OldValue
        {
            get { 
                if (this.nodeLogic.WorkSpace is CustomNodeWorkspaceModel)
                {
                    return "Not available in custom nodes";
                }

#if USE_DSENGINE
                return NodeModel.PrintValue(nodeLogic.VariableToPreview,
                                            0,
                                            Configurations.PreviewMaxListLength,
                                            0,
                                            Configurations.PreviewMaxListDepth,
                                            Configurations.PreviewMaxLength);
#else
                return NodeModel.PrintValue(nodeLogic.OldValue, 0, Configurations.PreviewMaxListLength, 0, 
                    Configurations.PreviewMaxListDepth, Configurations.PreviewMaxLength);

#endif
            }
        }

        public ElementState State
        {
            get { return nodeLogic.State; }
        }

        public string Description
        {
            get { return nodeLogic.Description; }
        }

        public bool IsCustomFunction
        {
            get { return nodeLogic.IsCustomFunction ? true : false; }
        }

        /// <summary>
        /// Element's left position is two-way bound to this value
        /// </summary>
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
        public double Top
        {
            get { return nodeLogic.Y; }
            set
            {
                nodeLogic.Y = value;
                RaisePropertyChanged("Top");
            }
        }

        public double ZIndex
         {
            get { return zIndex; }
            set { zIndex = value; RaisePropertyChanged("ZIndex"); }
         }

        /// <summary>
        /// Input grid's enabled state is now bound to this property
        /// which tracks the node model's InteractionEnabled property
        /// </summary>
        public bool IsInteractionEnabled
        {
            get { return nodeLogic.InteractionEnabled; }
        }

        public bool IsVisible
        {
            get
            {
                return nodeLogic.IsVisible;
            }
            set
            {
                nodeLogic.IsVisible = value;
                RaisePropertyChanged("IsVisible");
            }
        }

        public bool IsUpstreamVisible
        {
            get
            {
                return nodeLogic.IsUpstreamVisible;
            }
            set
            {
                nodeLogic.IsUpstreamVisible = value;
                RaisePropertyChanged("IsUpstreamVisible");
            }
        }

        public bool ShowsVisibilityToggles
        {
            get
            {
                //if the node is a Function, show the visibility toggles
                //if any of it's internal nodes is drawable.

                //return nodeLogic.OldValue!=null;
                return true;
            }
        }

        public bool IsPreviewInsetVisible
        {
            get
            {
                if(this.PreviewBubble == null)
                    return false;

                return (this.PreviewBubble.InfoBubbleState == InfoBubbleViewModel.State.Minimized);
            }
        }

        /// <summary>
        /// Enable or disable text labels on nodes.
        /// </summary>
        public bool IsDisplayingLabels
        {
            get 
            { return nodeLogic.DisplayLabels; }
            set
            {
                nodeLogic.DisplayLabels = value;
                RaisePropertyChanged("IsDisplayingLabels");
            }
        }

        public bool CanDisplayLabels
        {
            get { return nodeLogic.OldValue != null; }
        }

        #endregion

        #region events
        public event NodeHelpEventHandler RequestShowNodeHelp;
        public virtual void OnRequestShowNodeHelp(Object sender, NodeHelpEventArgs e)
        {
            if (RequestShowNodeHelp != null)
            {
                RequestShowNodeHelp(this, e);
            }
        }

        public event EventHandler RequestShowNodeRename;
        public virtual void OnRequestShowNodeRename(Object sender, EventArgs e)
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

        public NodeViewModel(NodeModel logic)
        {
            nodeLogic = logic;

            //respond to collection changed events to sadd
            //and remove port model views
            logic.InPorts.CollectionChanged += inports_collectionChanged;
            logic.OutPorts.CollectionChanged += outports_collectionChanged;

            logic.PropertyChanged += logic_PropertyChanged;

            dynSettings.Controller.DynamoViewModel.Model.PropertyChanged += Model_PropertyChanged;
            dynSettings.Controller.PropertyChanged += Controller_PropertyChanged;
            
            this.ErrorBubble = new InfoBubbleViewModel();
            this.ErrorBubble.PropertyChanged += ErrorBubble_PropertyChanged;

            // Nodes mentioned in switch cases will not have preview bubble
            switch (nodeLogic.Name)
            {
                case "Number":
                    break;
                case "String":
                    break;
                case "Watch":
                    break;
                case "Watch 3D":
                    break;
                case "Boolean":
                    break;
                default:
                    this.PreviewBubble = new InfoBubbleViewModel();
                    this.PreviewBubble.PropertyChanged += PreviewBubble_PropertyChanged;
                    break;
            }
            //Do a one time setup of the initial ports on the node
            //we can not do this automatically because this constructor
            //is called after the node's constructor where the ports
            //are initially registered
            SetupInitialPortViewModels();

            //dynSettings.Controller.RequestNodeSelect += new NodeEventHandler(Controller_RequestNodeSelect);
        }

        //void Controller_RequestNodeSelect(object sender, EventArgs e)
        //{
        //    ModelBase n = (e as ModelEventArgs).Model;

        //    DynamoSelection.Instance.ClearSelection();
        //    DynamoSelection.Instance.Selection.Add(n);
        //}

        #endregion

        /// <summary>
        /// Do a one setup of the ports 
        /// </summary>
        private void SetupInitialPortViewModels()
        {
            foreach (var item in nodeLogic.InPorts)
            {
                InPorts.Add(new PortViewModel(item as PortModel, nodeLogic));
            }

            foreach (var item in nodeLogic.OutPorts)
            {
                OutPorts.Add(new PortViewModel(item as PortModel, nodeLogic));
            }
        }

        /// <summary>
        /// Respond to property changes on the model
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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
        void logic_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "NickName":
                    RaisePropertyChanged("NickName");
                    break;
                case "OldValue":
                    RaisePropertyChanged("OldValue");
                    UpdatePreviewBubbleContent();
                    RaisePropertyChanged("CanDisplayLabels");
                    break;
                case "IsUpdated":
                    UpdatePreviewBubbleContent();
                    break;
                case "X":
                    RaisePropertyChanged("Left");
                    UpdateErrorBubblePosition();
                    UpdatePreviewBubblePosition();
                    break;
                case "Y":
                    RaisePropertyChanged("Top");
                    UpdateErrorBubblePosition();
                    UpdatePreviewBubblePosition();
                    break;
                case "InteractionEnabled":
                    RaisePropertyChanged("IsInteractionEnabled");
                    break;
                case "IsSelected":
                    RaisePropertyChanged("IsSelected");
                    UpdateZIndex();
                    break;
                case "State":
                    RaisePropertyChanged("State");
                    break;
                case "ArgumentLacing":
                    RaisePropertyChanged("ArgumentLacing");
                    break;
                case "ToolTipText":
                    UpdateErrorBubbleContent();
                    // TODO Update preview bubble visibility to false
                    break;
                case "IsVisible":
                    RaisePropertyChanged("IsVisible");
                    break;
                case "IsUpstreamVisible":
                    RaisePropertyChanged("IsUpstreamVisible");
                    break;
                case "Width":
                    RaisePropertyChanged("Width");
                    UpdateErrorBubblePosition();
                    UpdatePreviewBubblePosition();
                    break;
                case "Height":
                    RaisePropertyChanged("Height");
                    UpdateErrorBubblePosition();
                    UpdatePreviewBubblePosition();
                    break;
                case "DisplayLabels":
                    RaisePropertyChanged("IsDisplayingLables");
                    break;
                case "Position":
                    UpdateErrorBubblePosition();
                    UpdatePreviewBubblePosition();
                    break;
            }
        }

        private void Controller_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsShowPreviewByDefault":
                    HandleDefaultShowPreviewChanged();
                    break;
            }
        }

        private void ErrorBubble_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // TODO set preview to be no visible

            //switch (e.PropertyName)
            //{
            //    case "IsShowPreviewByDefault":
            //        RaisePropertyChanged("IsPreviewInsetVisible");
            //        break;
            //}
        }

        private void PreviewBubble_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "InfoBubbleState":
                    RaisePropertyChanged("IsPreviewInsetVisible");
                    break;
            }
        }

        private void HandleDefaultShowPreviewChanged()
        {
            if (this.PreviewBubble == null)
                return;

            UpdatePreviewBubbleContent();
            if (dynSettings.Controller.IsShowPreviewByDefault)
            {
                this.PreviewBubble.ChangeInfoBubbleStateCommand.Execute(InfoBubbleViewModel.State.Pinned);
            }
            else
            {
                this.PreviewBubble.ChangeInfoBubbleStateCommand.Execute(InfoBubbleViewModel.State.Minimized);
            }
        }

        private void UpdateZIndex()
        {
            if (this.IsSelected == true)
            {
                this.ZIndex = 4;

                if (this.PreviewBubble != null)
                    this.PreviewBubble.ZIndex = 4;
            }
            else
            {
                this.ZIndex = 3;

                if (this.PreviewBubble != null)
                    this.PreviewBubble.ZIndex = 3;
            }
        }

        private void UpdateErrorBubbleContent()
        {
            if (this.ErrorBubble == null || dynSettings.Controller == null)
                return;
            if (string.IsNullOrEmpty(NodeModel.ToolTipText))
            {
                if (NodeModel.State != ElementState.Error)
                {
                    ErrorBubble.ChangeInfoBubbleStateCommand.Execute(InfoBubbleViewModel.State.Minimized);
                }
            }
            else
            {
                if (!dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.Errors.Contains(this.ErrorBubble))
                    return;

                Point topLeft = new Point(NodeModel.X, NodeModel.Y);
                Point botRight = new Point(NodeModel.X + NodeModel.Width, NodeModel.Y + NodeModel.Height);
                InfoBubbleViewModel.Style style = InfoBubbleViewModel.Style.ErrorCondensed;
                // NOTE!: If tooltip is not cached here, it will be cleared once the dispatcher is invoked below
                string content = NodeModel.ToolTipText;
                InfoBubbleViewModel.Direction connectingDirection = InfoBubbleViewModel.Direction.Bottom;
                InfoBubbleDataPacket data = new InfoBubbleDataPacket(style, topLeft, botRight, content, connectingDirection);

                this.ErrorBubble.UpdateContentCommand.Execute(data);
                this.ErrorBubble.ChangeInfoBubbleStateCommand.Execute(InfoBubbleViewModel.State.Pinned);
            }
        }

        private void UpdateErrorBubblePosition()
        {
            if (this.ErrorBubble == null)
                return;
            InfoBubbleDataPacket data = new InfoBubbleDataPacket();
            data.TopLeft = GetTopLeft();
            data.BotRight = GetBotRight();
            this.ErrorBubble.UpdatePositionCommand.Execute(data);
        }

        private void UpdatePreviewBubbleContent()
        {
            if (this.PreviewBubble == null || this.NodeModel is Watch || dynSettings.Controller == null)
                return;

            //create data packet to send to preview bubble
            InfoBubbleViewModel.Style style = InfoBubbleViewModel.Style.PreviewCondensed;
            string content = this.OldValue;
            InfoBubbleViewModel.Direction connectingDirection = InfoBubbleViewModel.Direction.Top;
            InfoBubbleDataPacket data = new InfoBubbleDataPacket(style, GetTopLeft(), GetBotRight(), content, connectingDirection);

            var vm = dynSettings.Controller.DynamoViewModel;
            if (vm.CurrentSpaceViewModel.Previews.Contains(this.PreviewBubble))
                this.PreviewBubble.UpdateContentCommand.Execute(data);
        }

        private void UpdatePreviewBubblePosition()
        {
            if (this.PreviewBubble == null || this.NodeModel is Watch)
                return;
            InfoBubbleDataPacket data = new InfoBubbleDataPacket();
            data.TopLeft = GetTopLeft();
            data.BotRight = GetBotRight();
            this.PreviewBubble.UpdatePositionCommand.Execute(data);
        }

        private void ShowHelp(object parameter)
        {
            //var helpDialog = new NodeHelpPrompt(this.NodeModel);
            //helpDialog.Show();

            OnRequestShowNodeHelp(this, new NodeHelpEventArgs(NodeModel));
        }

        private bool CanShowHelp(object parameter)
        {
            return true;
        }

        private void ShowRename(object parameter)
        {
            OnRequestShowNodeRename(this, EventArgs.Empty);
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
            var command = new DynCmd.DeleteModelCommand(nodeLogic.GUID);
            dynSettings.Controller.DynamoViewModel.ExecuteCommand(command);
        }

        private void SetLacingType(object param)
        {
            // Record the state of this node before changes.
            DynamoModel dynamo = dynSettings.Controller.DynamoModel;
            dynamo.CurrentWorkspace.RecordModelForModification(nodeLogic);

            LacingStrategy strategy = LacingStrategy.Disabled;
            if (!System.Enum.TryParse(param.ToString(), out strategy))
                strategy = LacingStrategy.Disabled;

            NodeLogic.ArgumentLacing = strategy;

            RaisePropertyChanged("ArgumentLacing");
            dynSettings.Controller.DynamoViewModel.UndoCommand.RaiseCanExecuteChanged();
            dynSettings.Controller.DynamoViewModel.RedoCommand.RaiseCanExecuteChanged();
        }

        private bool CanSetLacingType(object param)
        {
            // Only allow setting of lacing strategy when it is not disabled.
            return (this.ArgumentLacing != LacingStrategy.Disabled);
        }

        private void ViewCustomNodeWorkspace(object parameter)
        {
            var f = (nodeLogic as Function);
            if(f!= null)
                dynSettings.Controller.DynamoViewModel.FocusCustomNodeWorkspace(f.Definition);
        }

        private bool CanViewCustomNodeWorkspace(object parameter)
        {
            return nodeLogic.IsCustomFunction;
        }

        //private void SetLayout(object parameters)
        //{
        //    var dict = parameters as Dictionary<string,
        //    double>;
        //    nodeLogic.X = dict["X"];
        //    nodeLogic.Y = dict["Y"];
        //    nodeLogic.Height = dict["Height"];
        //    nodeLogic.Width = dict["Width"];
        //}

        //private bool CanSetLayout(object parameters)
        //{
        //    var dict = parameters as Dictionary<string,
        //    double>;
        //    if (dict == null)
        //        return false;
        //    return true;
        //}

        private void inports_collectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //The visual height of the node is bound to preferred height.
            //PreferredHeight = Math.Max(inPorts.Count * 20 + 10, outPorts.Count * 20 + 10); //spacing for inputs + title space + bottom space

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                //create a new port view model
                foreach (var item in e.NewItems)
                {
                    InPorts.Add(new PortViewModel(item as PortModel, nodeLogic));
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                //remove the port view model whose model item
                //is the one passed in
                foreach (var item in e.OldItems)
                {
                    InPorts.Remove(InPorts.ToList().First(x => x.PortModel == item));
                }
            }
        }

        private void outports_collectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //The visual height of the node is bound to preferred height.
            //PreferredHeight = Math.Max(inPorts.Count * 20 + 10, outPorts.Count * 20 + 10); //spacing for inputs + title space + bottom space

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                //create a new port view model
                foreach (var item in e.NewItems)
                {
                    OutPorts.Add(new PortViewModel(item as PortModel, nodeLogic));
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                //remove the port view model whose model item is the
                //one passed in
                foreach (var item in e.OldItems)
                {
                    OutPorts.Remove(OutPorts.ToList().First(x => x.PortModel == item));
                }
            }
        }

        private void ToggleIsVisible(object parameter)
        {
            // Record the state of this node before changes.
            DynamoModel dynamo = dynSettings.Controller.DynamoModel;
            dynamo.CurrentWorkspace.RecordModelForModification(nodeLogic);

            this.nodeLogic.IsVisible = !this.nodeLogic.IsVisible;

            RaisePropertyChanged("IsVisible");
            dynSettings.Controller.DynamoViewModel.UndoCommand.RaiseCanExecuteChanged();
            dynSettings.Controller.DynamoViewModel.RedoCommand.RaiseCanExecuteChanged();
        }

        private void ToggleIsUpstreamVisible(object parameter)
        {
            // Record the state of this node before changes.
            DynamoModel dynamo = dynSettings.Controller.DynamoModel;
            dynamo.CurrentWorkspace.RecordModelForModification(nodeLogic);

            this.nodeLogic.IsUpstreamVisible = !this.nodeLogic.IsUpstreamVisible;

            RaisePropertyChanged("IsUpstreamVisible");
            dynSettings.Controller.DynamoViewModel.UndoCommand.RaiseCanExecuteChanged();
            dynSettings.Controller.DynamoViewModel.RedoCommand.RaiseCanExecuteChanged();
        }

        private bool CanVisibilityBeToggled(object parameter)
        {
            return true;
        }

        private bool CanUpstreamVisibilityBeToggled(object parameter)
        {
            return true;
        }

        private void ValidateConnections(object parameter)
        {
            nodeLogic.ValidateConnections();
        }

        private bool CanValidateConnections(object parameter)
        {
            return true;
        }

        private void SetupCustomUIElements(object nodeUI)
        {
            nodeLogic.InitializeUI(nodeUI);
        }

        private bool CanSetupCustomUIElements(object NodeUI)
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

        private void ShowTooltip(object parameter)
        {
            dynSettings.Controller.DynamoViewModel.ShowInfoBubble(parameter);
        }

        private bool CanShowTooltip(object parameter)
        {
            return true;
        }

        private void HideTooltip(object parameter)
        {
            if (dynSettings.Controller != null)
                dynSettings.Controller.DynamoViewModel.HideInfoBubble(parameter);
        }

        private bool CanHideTooltip(object parameter)
        {
            return true;
        }

        private void FadeOutTooltip(object parameter)
        {
            if (dynSettings.Controller != null)
                dynSettings.Controller.DynamoViewModel.FadeOutInfoBubble(parameter);
        }

        private bool CanFadeOutTooltip(object parameter)
        {
            return true;
        }

        private bool CanCollapseTooltip(object parameter)
        {
            return true;
        }

        private void ShowPreview(object parameter)
        {
            if (this.PreviewBubble == null)
                return;

            UpdatePreviewBubbleContent();
            this.PreviewBubble.ZIndex = 5;
            this.PreviewBubble.OnRequestAction(
                new InfoBubbleEventArgs(InfoBubbleEventArgs.Request.Show));
        }

        private bool CanShowPreview(object parameter)
        {
            return true;
        }

        private void HidePreview(object parameter)
        {
            //this.PreviewBubble.FadeOutCommand.Execute(null);
        }

        private bool CanHidePreview(object parameter)
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
            dynSettings.Controller.DynamoViewModel.GoToWorkspace((NodeLogic as Function).Definition.FunctionId);
        }

        private bool CanGotoWorkspace(object parameters)
        {
            if (NodeLogic is Function)
            {
                return true;
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

    public class NodeHelpEventArgs : EventArgs
    {
        public NodeModel Model { get; set; }
        public NodeHelpEventArgs(NodeModel model)
        {
            Model = model;
        }
    }
}

