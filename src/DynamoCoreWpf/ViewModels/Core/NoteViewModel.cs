using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Dynamo.Configuration;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Notes;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.Wpf.ViewModels.Core;
using Newtonsoft.Json;
using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.ViewModels
{
    public partial class NoteViewModel : ViewModelBase
    {
        private int DISTANCE_TO_PINNED_NODE = 16;
        private int DISTANCE_TO_PINNED_NODE_WITH_WARNING = 64;

        #region Events

        public event EventHandler RequestsSelection;
        public virtual void OnRequestsSelection(Object sender, EventArgs e)
        {
            if (RequestsSelection != null)
            {
                RequestsSelection(this, e);
            }
        }

        #endregion

        #region Properties

        private NoteModel _model;

        [JsonIgnore]
        public readonly WorkspaceViewModel WorkspaceViewModel;
        private int zIndex = Configurations.NodeStartZIndex; // initialize the start Z-Index of a note to the same as that of a node
        internal static int StaticZIndex = Configurations.NodeStartZIndex;

        [JsonIgnore]
        public NoteModel Model
        {
            get { return _model; }
            set
            {
                _model = value;
                RaisePropertyChanged("Model");
            }
        }

        /// <summary>
        /// Element's left position is two-way bound to this value
        /// </summary>
        [JsonIgnore]
        public double Left
        {
            get { return _model.X; }
            set
            {
                _model.X = value;
                RaisePropertyChanged("Left");
            }
        }

        /// <summary>
        /// Element's top position is two-way bound to this value
        /// </summary>
        [JsonIgnore]
        public double Top
        {
            get { return _model.Y; }
            set
            {
                _model.Y = value;
                RaisePropertyChanged("Top");
            }
        }

        /// <summary>
        /// ZIndex represents the order on the z-plane in which the notes and other objects appear. 
        /// </summary>
        [JsonIgnore]
        public int ZIndex
        {

            get { return zIndex; }
            set { zIndex = value; RaisePropertyChanged("ZIndex"); }
        }

        [JsonIgnore]
        public string Text
        {
            get { return _model.Text; }
            set { _model.Text = value; }
        }

        [JsonIgnore]
        public bool IsSelected
        {
            get { return _model.IsSelected; }
        }

        private bool isOnEditMode;

        /// <summary>
        /// Property determines if note is being edited, 
        /// is set to true when double clicking the note
        /// is set to false when note's textbox looses focus
        /// </summary>
        [JsonIgnore]
        public bool IsOnEditMode
        {
            get { return isOnEditMode; }
            set { isOnEditMode = value; RaisePropertyChanged(nameof(IsOnEditMode)); }
        }

        /// <summary>
        /// NodeViewModel which this Note is pinned to
        /// When using the pin to node command  
        /// note and node become entangled so that 
        /// if you select and move one the other one 
        /// moves as well.
        /// </summary>
        public NodeViewModel PinnedNode
        {
            get
            {
                if (Model.PinnedNode == null)
                {
                    return null;
                }

                return WorkspaceViewModel.Nodes
                    .Where(x => x.Id == Model.PinnedNode.GUID)
                    .FirstOrDefault();
            }
        }

        #endregion

        public NoteViewModel(WorkspaceViewModel workspaceViewModel, NoteModel model)
        {
            this.WorkspaceViewModel = workspaceViewModel;
            _model = model;
            model.PropertyChanged += note_PropertyChanged;
            model.UndoRequest += note_PinUnpinToNode;
            DynamoSelection.Instance.Selection.CollectionChanged += SelectionOnCollectionChanged;
            ZIndex = ++StaticZIndex; // places the note on top of all nodes/notes

            if (Model.PinnedNode != null)
            {
                SubscribeToPinnedNode();
            }
            IsOnEditMode = false;
        }

        public override void Dispose()
        {
            if (Model.PinnedNode != null)
            {
                UnsuscribeFromPinnedNode();
            }
            _model.PropertyChanged -= note_PropertyChanged;
            _model.UndoRequest -= note_PinUnpinToNode;
            DynamoSelection.Instance.Selection.CollectionChanged -= SelectionOnCollectionChanged;
        }

        private void note_PinUnpinToNode(ModelBase obj)
        {
            if (Model.UndoRedoAction.Equals(NoteModel.UndoAction.Unpin))
            {
                UnpinFromNode(obj);
                return;
            }
            if (Model.UndoRedoAction.Equals(NoteModel.UndoAction.Pin))
            {
                NodeModel node = WorkspaceViewModel.Model.Nodes
                    .Where(x => x.GUID.Equals(Model.PinnedNodeGuid))
                    .FirstOrDefault();

                if (node == null) return;

                // In case the user has selected a different Node before Undo
                // We run the risk of pinning to the wrong Node
                // Therefore clear selection before running
                DynamoSelection.Instance.ClearSelection();
                DynamoSelection.Instance.Selection.Add(node);
                PinToNode(obj);
                return;
            }
        }


        private void SelectionOnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CreateGroupCommand.RaiseCanExecuteChanged();
            AddToGroupCommand.RaiseCanExecuteChanged();
            UngroupCommand.RaiseCanExecuteChanged();
            PinToNodeCommand.RaiseCanExecuteChanged();
        }

        private void Select(object parameter)
        {
            OnRequestsSelection(this, EventArgs.Empty);
        }

        public void UpdateSizeFromView(double w, double h)
        {
            this._model.SetSize(w, h);
            MoveNoteAbovePinnedNode();
        }

        private bool CanSelect(object parameter)
        {
            if (!DynamoSelection.Instance.Selection.Contains(_model))
            {
                return true;
            }
            return false;
        }

        //respond to changes on the model's properties
        void note_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "X":
                    RaisePropertyChanged("Left");
                    break;
                case "Y":
                    RaisePropertyChanged("Top");
                    break;
                case "Text":
                    RaisePropertyChanged("Text");
                    break;
                case "IsSelected":
                    RaisePropertyChanged("IsSelected");
                    break;
                case nameof(NoteModel.PinnedNode):
                    RaisePropertyChanged(nameof(this.PinnedNode));
                    PinToNodeCommand.RaiseCanExecuteChanged();
                    UnpinFromNodeCommand.RaiseCanExecuteChanged();
                    break;

            }
        }

        private void CreateGroup(object parameters)
        {
            WorkspaceViewModel.DynamoViewModel.AddAnnotationCommand.Execute(null);
        }

        private bool CanCreateGroup(object parameters)
        {
            var groups = WorkspaceViewModel.Model.Annotations;
            //Create Group should be disabled when a group is selected
            if (groups != null && groups.Any(x => x.IsSelected))
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

        private void UngroupNote(object parameters)
        {
            WorkspaceViewModel.DynamoViewModel.UngroupModelCommand.Execute(null);
        }

        private bool CanUngroupNote(object parameters)
        {
            var groups = WorkspaceViewModel.Model.Annotations;
            if (!groups.Any(x => x.IsSelected))
            {
                return (groups.ContainsModel(Model.GUID));
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
            if (groups.Any(x => x.IsSelected) &&
                !groups.All(x => !x.IsExpanded))
            {
                return !(groups.ContainsModel(Model.GUID));
            }
            return false;
        }

        private void PinToNode(object parameters)
        {

            var nodeToPin = DynamoSelection.Instance.Selection
                .OfType<NodeModel>()
                .FirstOrDefault();

            if (nodeToPin == null)
            {
                return;
            }

            WorkspaceModel.RecordModelForModification(Model, WorkspaceViewModel.Model.UndoRecorder);

            var nodeGroup = WorkspaceViewModel.Annotations
                .FirstOrDefault(x => x.AnnotationModel.ContainsModel(nodeToPin));

            if (nodeGroup != null)
            {
                nodeGroup.AnnotationModel.AddToTargetAnnotationModel(this.Model);
            }

            Model.PinnedNode = nodeToPin;

            MoveNoteAbovePinnedNode();
            SubscribeToPinnedNode();

            WorkspaceViewModel.HasUnsavedChanges = true;
        }

        private bool CanPinToNode(object parameters)
        {
            // Go over all elements selected and get the first NodeModel,
            // If there is no nodeModel or there is already a node pinned
            // Note cannot be pinned to any node

            var nodeSelection = DynamoSelection.Instance.Selection
                    .OfType<NodeModel>();

            var noteSelection = DynamoSelection.Instance.Selection
                    .OfType<NoteModel>();

            if (nodeSelection == null || noteSelection == null ||
                nodeSelection.Count() != 1 || noteSelection.Count() != 1)
                return false;

            var nodeToPin = nodeSelection.FirstOrDefault();

            var nodeAlreadyPinned = WorkspaceViewModel.Notes
                .Where(n => n.PinnedNode != null)
                .Any(n => n.PinnedNode.NodeModel.GUID == nodeToPin.GUID);

            if (nodeToPin == null || Model.PinnedNode != null || nodeAlreadyPinned)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// This method will be executed for validate if the "Unpin from node" option should be shown or not in the context menu (when clicking right over the note)
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private bool CanUnpinFromNode(object parameters)
        {
            if (PinnedNode != null)
                return true;

            return false;
        }

        private void UnpinFromNode(object parameters)
        {
            UnsuscribeFromPinnedNode();

            Model.PinnedNode = null;
            WorkspaceViewModel.HasUnsavedChanges = true;
        }

        private void SubscribeToPinnedNode()
        {
            // Subscribe to PinnedNode (model and viewmodel) Property_Changed
            // so that note moves above and behind node every time
            // NodeModel.State, NodeModel.Position, or NodeViewModel.ZIndex change
            Model.PinnedNode.PropertyChanged += PinnedNodeModel_PropertyChanged;
            PinnedNode.PropertyChanged += PinnedNodeViewModel_PropertyChanged;

            // Subscribe to PinnedNode.Selected (fires before node is selected)
            // so that this note is added to the selection
            PinnedNode.Selected += PinnedNodeViewModel_OnPinnedNodeSelected;

            //Subscribed to PinnedNode.Removed event of Node to remove pinned note when node is removed.
            PinnedNode.Removed += PinnedNodeViewModel_OnPinnedNodeRemoved;

            Analytics.TrackEvent(
                Actions.Pin,
                Categories.NoteOperations, Model.PinnedNode.Name);
        }

        private void UnsuscribeFromPinnedNode()
        {
            Model.PinnedNode.PropertyChanged -= PinnedNodeModel_PropertyChanged;
            if (PinnedNode != null)
            {
                PinnedNode.PropertyChanged -= PinnedNodeViewModel_PropertyChanged;
                PinnedNode.Selected -= PinnedNodeViewModel_OnPinnedNodeSelected;
                PinnedNode.Removed -= PinnedNodeViewModel_OnPinnedNodeRemoved;

                Analytics.TrackEvent(
                    Actions.Unpin,
                    Categories.NoteOperations, Model.PinnedNode.Name);
            }
        }

        private void PinnedNodeViewModel_OnPinnedNodeRemoved(object sender, EventArgs e)
        {
            WorkspaceViewModel.DynamoViewModel.DeleteCommand.Execute(null);
        }

        private void PinnedNodeModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NodeModel.State)
                || e.PropertyName == nameof(NodeModel.Position))
            {
                MoveNoteAbovePinnedNode();
            }
        }

        private void PinnedNodeViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ZIndex))
            {
                MoveNoteAbovePinnedNode();
            }
        }

        private void PinnedNodeViewModel_OnPinnedNodeSelected(object sender, EventArgs e)
        {

            if (!(sender is NodeViewModel node)
                || Model.PinnedNode == null
                || node.Id != Model.PinnedNode.GUID)
            {
                return;
            }

            DynamoSelection.Instance.Selection.AddUnique(Model);
        }

        private void MoveNoteAbovePinnedNode()
        {
            if (PinnedNode == null) return;

            var distanceToNode = DISTANCE_TO_PINNED_NODE;
            if ((Model.PinnedNode.State == ElementState.Error ||
                Model.PinnedNode.State == ElementState.Warning) && Model.PinnedNode.DismissedAlerts.Count == 0)
            {
                distanceToNode = DISTANCE_TO_PINNED_NODE_WITH_WARNING;
            }
            Model.CenterX = Model.PinnedNode.CenterX;
            Model.CenterY = Model.PinnedNode.CenterY - (Model.PinnedNode.Height * 0.5) - (Model.Height * 0.5) - distanceToNode;
            Model.ReportPosition();

            ZIndex = Convert.ToInt32(PinnedNode.ZIndex);
        }
    }
}
