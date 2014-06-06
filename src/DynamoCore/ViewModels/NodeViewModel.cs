using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using Dynamo.Controls;
using Dynamo.DSEngine;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.UI;
using Dynamo.Utilities;
using System.Windows;
using Dynamo.Core;
using ProtoCore.AST.AssociativeAST;
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
        public delegate void NodeDialogEventHandler(object sender, NodeDialogEventArgs e);
        #endregion

        #region private members

        ObservableCollection<PortViewModel> inPorts = new ObservableCollection<PortViewModel>();
        ObservableCollection<PortViewModel> outPorts = new ObservableCollection<PortViewModel>();
        NodeModel nodeLogic;
        private bool isFullyConnected = false;
        private double zIndex = 3;
        private string astText = string.Empty;

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
            get
            {
                if (nodeLogic.WorkSpace is CustomNodeWorkspaceModel)
                    return "Not available in custom nodes";

                var variableName = nodeLogic.AstIdentifierBase;

                string previewValue = "<null>";
                if (!string.IsNullOrEmpty(variableName))
                {
                    try
                    {
                        var engine = dynSettings.Controller.EngineController;
                        previewValue = engine.GetStringValue(variableName);
                    }
                    catch (Exception ex)
                    {
                        dynSettings.DynamoLogger.Log(ex.Message);
                    }
                }

                return previewValue;
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
            get { return nodeLogic.ShouldDisplayPreview(); }
        }

        public bool ShouldShowGlyphBar
        {
            get { return IsPreviewInsetVisible || ArgumentLacing != LacingStrategy.Disabled; }
        }

        /// <summary>
        /// Enable or disable text labels on nodes.
        /// </summary>
        public bool IsDisplayingLabels
        {
            get { return nodeLogic.DisplayLabels; }
            set
            {
                nodeLogic.DisplayLabels = value;
                RaisePropertyChanged("IsDisplayingLabels");
            }
        }

        public bool CanDisplayLabels
        {
            get
            {
                lock (nodeLogic.RenderPackagesMutex)
                {
                    return nodeLogic.RenderPackages.Any(y => ((RenderPackage)y).IsNotEmpty());
                }
            }
        }

        public string ASTText
        {
            get { return astText; }
            set
            {
                astText = value;
                RaisePropertyChanged("ASTText");
            }
        }

        public bool ShowDebugASTs
        {
            get { return dynSettings.Controller.DebugSettings.ShowDebugASTs; }
            set
            {
                dynSettings.Controller.DebugSettings.ShowDebugASTs = value;
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

        public NodeViewModel(NodeModel logic)
        {
            nodeLogic = logic;

            //respond to collection changed events to sadd
            //and remove port model views
            logic.InPorts.CollectionChanged += inports_collectionChanged;
            logic.OutPorts.CollectionChanged += outports_collectionChanged;

            logic.PropertyChanged += logic_PropertyChanged;

            dynSettings.Controller.DynamoViewModel.Model.PropertyChanged += Model_PropertyChanged;
            dynSettings.Controller.DebugSettings.PropertyChanged += DebugSettings_PropertyChanged;

            ErrorBubble = new InfoBubbleViewModel();

            //Do a one time setup of the initial ports on the node
            //we can not do this automatically because this constructor
            //is called after the node's constructor where the ports
            //are initially registered
            SetupInitialPortViewModels();

            if (IsDebugBuild)
            {
                dynSettings.Controller.EngineController.AstBuilt += EngineController_AstBuilt;
            }
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
        void EngineController_AstBuilt(object sender, AstBuilder.ASTBuiltEventArgs e)
        {
            if (e.Node == nodeLogic)
            {
                var sb = new StringBuilder();
                sb.AppendLine(string.Format("{0} AST:", e.Node.GUID));

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
                InPorts.Add(new PortViewModel(item, nodeLogic));
            }

            foreach (var item in nodeLogic.OutPorts)
            {
                OutPorts.Add(new PortViewModel(item, nodeLogic));
            }
        }

        /// <summary>
        /// Respond to property changes on the model
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
                case "NickName":
                    RaisePropertyChanged("NickName");
                    break;
                case "OldValue":
                    RaisePropertyChanged("OldValue");
                    RaisePropertyChanged("CanDisplayLabels");
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
                case "IsUpstreamVisible":
                    RaisePropertyChanged("IsUpstreamVisible");
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
            }
        }

        private void UpdateBubbleContent()
        {
            if (ErrorBubble == null || dynSettings.Controller == null)
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
                if (!dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.Errors.Contains(ErrorBubble))
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
            var command = new DynamoViewModel.DeleteModelCommand(nodeLogic.GUID);
            dynSettings.Controller.DynamoViewModel.ExecuteCommand(command);
        }

        private void SetLacingType(object param)
        {
            // Record the state of this node before changes.
            DynamoModel dynamo = dynSettings.Controller.DynamoModel;
            dynamo.CurrentWorkspace.RecordModelForModification(nodeLogic);

            LacingStrategy strategy = LacingStrategy.Disabled;
            if (!Enum.TryParse(param.ToString(), out strategy))
                strategy = LacingStrategy.Disabled;

            NodeLogic.ArgumentLacing = strategy;

            RaisePropertyChanged("ArgumentLacing");
            dynSettings.Controller.DynamoViewModel.UndoCommand.RaiseCanExecuteChanged();
            dynSettings.Controller.DynamoViewModel.RedoCommand.RaiseCanExecuteChanged();
        }

        private bool CanSetLacingType(object param)
        {
            // Only allow setting of lacing strategy when it is not disabled.
            return (ArgumentLacing != LacingStrategy.Disabled);
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

        private void inports_collectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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

        private void outports_collectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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

            nodeLogic.IsVisible = !nodeLogic.IsVisible;

            RaisePropertyChanged("IsVisible");
            dynSettings.Controller.DynamoViewModel.UndoCommand.RaiseCanExecuteChanged();
            dynSettings.Controller.DynamoViewModel.RedoCommand.RaiseCanExecuteChanged();
        }

        private void ToggleIsUpstreamVisible(object parameter)
        {
            // Record the state of this node before changes.
            DynamoModel dynamo = dynSettings.Controller.DynamoModel;
            dynamo.CurrentWorkspace.RecordModelForModification(nodeLogic);

            nodeLogic.IsUpstreamVisible = !nodeLogic.IsUpstreamVisible;

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

