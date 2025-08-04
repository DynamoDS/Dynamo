using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Dynamo.Configuration;
using Dynamo.Graph;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Notes;
using Dynamo.Interfaces;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.UI.Commands;
using Dynamo.Utilities;
using DynamoUtilities;
using Newtonsoft.Json;
using Color = System.Windows.Media.Color;

namespace Dynamo.ViewModels
{
    public class AnnotationViewModel : ViewModelBase
    {
        public delegate void SnapInputEventHandler(PortViewModel portViewModel);
        public event SnapInputEventHandler SnapInputEvent;

        private AnnotationModel annotationModel;
        private IEnumerable<PortModel> originalInPorts;
        private IEnumerable<PortModel> originalOutPorts;
        private Dictionary<Guid, Geometry> GroupIdToCutGeometry = new Dictionary<Guid, Geometry>();
        // vertical offset accounts for the port margins
        private const int verticalOffset = 17;
        private const int portVerticalMidPoint = 17;
        private const int portToggleOffset = 30;
        private ObservableCollection<Dynamo.Configuration.StyleItem> groupStyleList;
        private IEnumerable<Configuration.StyleItem> preferencesStyleItemsList;
        private PreferenceSettings preferenceSettings;
        private double heightBeforeToggle;
        private double widthBeforeToggle;

        // Collapsed proxy ports for Code Block Nodes appear visually misaligned - 0.655px
        // taller compared to their actual ports. This is due to the fixed height - 16.345px
        // used inside CBNs for code lines, while proxy ports use 14px height + 3px top margin.
        // To compensate for this visual mismatch and keep connector alignment consistent,
        // we apply this adjusted proxy height.
        private const double CBNProxyPortVisualHeight = 17;
        private const double MinSpacing = 50;
        private const double MinChangeThreshold = 1;

        public readonly WorkspaceViewModel WorkspaceViewModel;

        #region Properties
        [JsonIgnore]
        public AnnotationModel AnnotationModel
        {
            get { return annotationModel; }
            set
            {
                annotationModel = value;
                RaisePropertyChanged("AnnotationModel");
            }
        }

        [JsonIgnore]
        public Double Width
        {
            get { return annotationModel.Width; }
            set
            {
                annotationModel.Width = value;
            }
        }

        [JsonIgnore]
        public Double Height
        {
            get { return annotationModel.Height; }
            set
            {
                annotationModel.Height = value;
            }
        }

        [JsonIgnore]
        public double ModelAreaHeight
        {
            get => annotationModel.ModelAreaHeight;
            set
            {
                annotationModel.ModelAreaHeight = value;
            }
        }

        [JsonIgnore]
        public Double Top
        {
            get { return annotationModel.Y; }
            set
            {
                annotationModel.Y = value;
            }
        }

        [JsonIgnore]
        public Double Left
        {
            get { return annotationModel.X; }
            set { annotationModel.X = value; }
        }

        [JsonIgnore]
        public double ZIndex
        {
            get
            {
                if (BelongsToGroup())
                {
                    return 2;
                }
                return 1;
            }
        }

        [JsonIgnore]
        public String AnnotationText
        {
            get { return annotationModel.AnnotationText; }
            set
            {
                annotationModel.AnnotationText = value;
            }
        }

        /// <summary>
        /// Group description
        /// </summary>
        [JsonIgnore]
        public string AnnotationDescriptionText
        {
            get { return annotationModel.AnnotationDescriptionText; }
            set
            {
                annotationModel.AnnotationDescriptionText = value;
            }
        }

        private Color _background;
        [JsonIgnore]
        public Color Background
        {
            get
            {
                var solidColorBrush =
                    (SolidColorBrush)
                        new BrushConverter().ConvertFromString(annotationModel.Background);
                if (solidColorBrush != null) _background = solidColorBrush.Color;
                return _background;
            }
            set
            {
                annotationModel.Background = value.ToString();
            }
        }

        [JsonIgnore]
        public PreviewState PreviewState
        {
            get
            {
                if (annotationModel.IsSelected)
                {
                    return PreviewState.Selection;
                }

                return PreviewState.None;
            }
        }

        [JsonIgnore]
        public Double FontSize
        {
            get
            {
                return annotationModel.FontSize;
            }
            set
            {
                annotationModel.FontSize = value;
            }
        }

        /// <summary>
        /// Id of the applied GroupStyle
        /// </summary>
        [JsonIgnore]
        public Guid GroupStyleId
        {
            get
            {
                return annotationModel.GroupStyleId;
            }
            set
            {
                annotationModel.GroupStyleId = value;
            }
        }

        [JsonIgnore]
        public IEnumerable<ModelBase> Nodes
        {
            get { return annotationModel.Nodes; }
        }

        private IEnumerable<ViewModelBase> viewModelBases;
        /// <summary>
        /// Collection of ViewModelBases that belongs to
        /// this group.
        /// Same as AnnotationModel.Nodes but with ViewModels
        /// instead of ModelBase.
        /// </summary>
        internal IEnumerable<ViewModelBase> ViewModelBases
        {
            get => viewModelBases;
            set
            {
                viewModelBases = value;
            }

        }

        private ObservableCollection<PortViewModel> inPorts;

        /// <summary>
        /// Collection of all input ports on this group.
        /// All nodes contained in the group which input port
        /// is either not connected or connected to a node outside
        /// of this group will have their input ports
        /// added to this collection.
        /// </summary>
        [JsonIgnore]
        public ObservableCollection<PortViewModel> InPorts
        {
            get => inPorts;
            private set
            {
                inPorts = value;
            }
        }

        private ObservableCollection<PortViewModel> optionalInPorts;
        /// <summary>
        /// Collection of optional input ports.
        /// These are inputs using default values or unconnected.
        /// </summary>
        [JsonIgnore]
        public ObservableCollection<PortViewModel> OptionalInPorts
        {
            get => optionalInPorts;
            private set
            {
                optionalInPorts = value;
            }
        }

        private ObservableCollection<PortViewModel> outPorts;
        /// <summary>
        /// Collection of all output ports on this group.
        /// All nodes contained in the group which output port 
        /// is connected to a node outside of this group will have
        /// their output ports added to this collection.
        /// </summary>
        [JsonIgnore]
        public ObservableCollection<PortViewModel> OutPorts
        {
            get => outPorts;
            private set
            {
                outPorts = value;
            }
        }

        private ObservableCollection<PortViewModel> unconnectedOutPorts;
        /// <summary>
        /// Collection of unconnected output ports in the group.
        /// </summary>
        [JsonIgnore]
        public ObservableCollection<PortViewModel> UnconnectedOutPorts
        {
            get => unconnectedOutPorts;
            private set
            {
                unconnectedOutPorts = value;
            }
        }

        private bool isOptionalInPortsCollapsed;
        /// <summary>
        /// Controls visibility of optional input ports in the group.
        /// </summary>
        [JsonIgnore]
        public bool IsOptionalInPortsCollapsed
        {
            get => isOptionalInPortsCollapsed;
            set
            {
                if (isOptionalInPortsCollapsed == value) return;

                // Record for undo
                var undoRecorder = WorkspaceViewModel.Model.UndoRecorder;
                using (undoRecorder.BeginActionGroup())
                    undoRecorder.RecordModificationForUndo(annotationModel);

                isOptionalInPortsCollapsed = value;
                annotationModel.IsOptionalInPortsCollapsed = value;

                RaisePropertyChanged(nameof(IsOptionalInPortsCollapsed));
                WorkspaceViewModel.HasUnsavedChanges = true;

                HandlePrePortToggleLayout();
            }
        }

        private bool isUnconnectedOutPortsCollapsed;
        /// <summary>
        /// Controls visibility of unconnected output ports in the group.
        /// </summary>
        public bool IsUnconnectedOutPortsCollapsed
        {
            get => isUnconnectedOutPortsCollapsed;
            set
            {
                if (isUnconnectedOutPortsCollapsed == value) return;

                // Record for undo
                var undoRecorder = WorkspaceViewModel.Model.UndoRecorder;
                using (undoRecorder.BeginActionGroup())
                    undoRecorder.RecordModificationForUndo(annotationModel);


                isUnconnectedOutPortsCollapsed = value;
                annotationModel.IsUnconnectedOutPortsCollapsed = value;

                RaisePropertyChanged(nameof(IsUnconnectedOutPortsCollapsed));
                WorkspaceViewModel.HasUnsavedChanges = true;

                HandlePrePortToggleLayout();
            }
        }

        /// <summary>
        /// Gets or sets the models IsExpanded property.
        /// When set it will either show all of the groups node
        /// or hide them and create input and output ports on the group.
        /// </summary>
        public bool IsExpanded
        {
            get => annotationModel.IsExpanded;
            set
            {
                // This change is triggered by the user interaction in the View.
                // Before we updating the value in the Model and ViewModel
                // we record the current state in the UndoRedoStack.
                // This ensures that any modifications can be reverted by the user.
                var undoRecorder = WorkspaceViewModel.Model.UndoRecorder;
                using (undoRecorder.BeginActionGroup())
                {
                    undoRecorder.RecordModificationForUndo(annotationModel);
                }

                annotationModel.IsExpanded = value;

                // Methods to collapse or expand the group based on the new value of IsExpanded.
                ManageAnnotationMVExpansionAndCollapse();
                HandlePrePortToggleLayout();
            }
        }

        private void ReportNodesPosition()
        {
            foreach (var node in Nodes.OfType<AnnotationModel>())
            {
                node.ReportPosition();
            }
        }

        private bool nodeHoveringState;
        /// <summary>
        /// This is used to determine if there is
        /// a node hovering over this group.
        /// When this is true the views nodeHoveringStateBorder
        /// is activated.
        /// </summary>
        [JsonIgnore]
        public bool NodeHoveringState
        {
            get => nodeHoveringState;
            set
            {
                if (nodeHoveringState == value)
                {
                    return;
                }

                nodeHoveringState = value;
                RaisePropertyChanged(nameof(NodeHoveringState));
            }
        }

        /// <summary>
        /// Collection of the nested groups in this group.
        /// This is used for displaying nested groups info
        /// when this group is collapsed.
        /// </summary>
        public ICollection<AnnotationViewModel> NestedGroups
        {
            get => nestedGroups;
            set
            {
                nestedGroups = value;
                RaisePropertyChanged(nameof(NestedGroups));
            }
        }

        /// <summary>
        /// Counter of all nodes in the group that
        /// aren't either an input or output node.
        /// This is used to display the amount of nodes
        /// the are in between the input and output nodes.
        /// </summary>
        public int NodeContentCount
        {
            get => Nodes
                .OfType<NodeModel>()
                .Count();
        }

        /// <summary>
        /// Rectangle of the ModelArea
        /// </summary>
        public System.Windows.Rect ModelAreaRect
        {
            get
            {
                return new System.Windows.Rect(0, 0, Width, ModelAreaHeight);
            }
        }

        /// <summary>
        /// Collection of rectangles based on AnnotationModels
        /// that belongs to this group.
        /// This is used to make a cutout in this groups background
        /// where another group is placed so there wont be an overlay.
        /// </summary>
        public SmartObservableCollection<Geometry> NestedGroupsGeometries = new SmartObservableCollection<Geometry>();

        /// <summary>
        /// This property will be used to populate the GroupStyle context menu (the one shown when clicking right over a Group)
        /// </summary>
        [JsonIgnore]
        public ObservableCollection<Configuration.StyleItem> GroupStyleList
        {
            get
            {
                return groupStyleList;
            }
            set
            {
                groupStyleList = value;
                RaisePropertyChanged(nameof(GroupStyleList));
            }
        }
        #endregion

        #region Commands
        private DelegateCommand _changeFontSize;
        [JsonIgnore]
        public DelegateCommand ChangeFontSize
        {
            get
            {
                if (_changeFontSize == null)
                    _changeFontSize =
                        new DelegateCommand(UpdateFontSize, CanChangeFontSize);

                return _changeFontSize;
            }
        }

        private DelegateCommand _addToGroupCommand;
        [JsonIgnore]
        public DelegateCommand AddToGroupCommand
        {
            get
            {
                if (_addToGroupCommand == null)
                    _addToGroupCommand =
                        new DelegateCommand(AddToGroup, CanAddToGroup);

                return _addToGroupCommand;
            }
        }

        private DelegateCommand addGroupToGroupCommand;
        /// <summary>
        /// Adds the selected groups to this group
        /// </summary>
        [JsonIgnore]
        public DelegateCommand AddGroupToGroupCommand
        {
            get
            {
                if (addGroupToGroupCommand == null)
                    addGroupToGroupCommand =
                        new DelegateCommand(AddGroupToGroup, CanAddGroupToGroup);

                return addGroupToGroupCommand;
            }
        }

        private DelegateCommand removeGroupFromGroup;
        private ICollection<AnnotationViewModel> nestedGroups;

        /// <summary>
        /// Command to remove this group from the group it
        /// belongs to.
        /// </summary>
        [JsonIgnore]
        public DelegateCommand RemoveGroupFromGroupCommand
        {
            get
            {
                if (removeGroupFromGroup == null)
                    removeGroupFromGroup =
                        new DelegateCommand(RemoveGroupFromGroup, CanUngroupGroup);

                return removeGroupFromGroup;
            }
        }

        private DelegateCommand dissolveNestedGroup;
        /// <summary>
        /// Command to dissolve hosted groups inside the host group
        /// belongs to.
        /// </summary>
        [JsonIgnore]
        public DelegateCommand DissolveNestedGroupsCommand
        {
            get
            {
                if (dissolveNestedGroup == null)
                    dissolveNestedGroup =
                        new DelegateCommand(DissolveNestedGroups, CanUngroupGroup);

                return dissolveNestedGroup;
            }
        }


        private bool CanAddToGroup(object obj)
        {
            return
                DynamoSelection.Instance.Selection.Count >= 0 &&
                IsExpanded;
        }

        private void AddToGroup(object obj)
        {
            if (annotationModel.IsSelected)
            {
                var selectedModels = DynamoSelection.Instance.Selection.OfType<ModelBase>();
                foreach (var model in selectedModels)
                {
                    if (!(model is AnnotationModel))
                    {
                        this.AnnotationModel.AddToTargetAnnotationModel(model, true);
                    }
                }
                Analytics.TrackEvent(Actions.AddedTo, Categories.GroupOperations, "Node");
            }
        }

        internal bool CanAddGroupToGroup(object obj)
        {
            // First make sure this group is selected
            // and that it does not already belong to
            // another group
            if (!this.AnnotationModel.IsSelected ||
                !this.IsExpanded ||
                BelongsToGroup())
            {
                return false;
            }

            var selectedAnnotationModels = DynamoSelection.Instance.Selection
                    .OfType<AnnotationModel>()
                    .Where(model => model.GUID != this.AnnotationModel.GUID);

            // Then we make sure that there are any other
            // AnnotationModels selected
            if (!selectedAnnotationModels.Any()) return false;

            // Lastly we make sure that non of the selected
            // groups (except this one) already has nested groups
            // and there are at least one of the groups that does
            // not already belong to another group.
            return !selectedAnnotationModels.Any(x => x.HasNestedGroups) &&
                !selectedAnnotationModels.All(x => WorkspaceViewModel.Model.Annotations.ContainsModel(x));

        }

        private void AddGroupToGroup(object obj)
        {
            if (annotationModel.IsSelected)
            {
                var selectedModels = DynamoSelection.Instance.Selection
                    .OfType<AnnotationModel>()
                    .Where(x => x.GUID != this.AnnotationModel.GUID &&
                                !WorkspaceViewModel.Model.Annotations.ContainsModel(x));

                using (NestedGroupsGeometries.DeferCollectionReset())
                {
                    foreach (var model in selectedModels)
                    {
                        WorkspaceViewModel.DynamoViewModel.AddGroupToGroupModelCommand.Execute(this.AnnotationModel.GUID);
                        if (Nodes.Contains(model))
                        {
                            var groupViewModel = ViewModelBases.OfType<AnnotationViewModel>()
                                .Where(x => x.AnnotationModel.GUID == model.GUID)
                                .FirstOrDefault();

                            groupViewModel.AddToGroupCommand.RaiseCanExecuteChanged();
                            groupViewModel.AddGroupToGroupCommand.RaiseCanExecuteChanged();
                            groupViewModel.RemoveGroupFromGroupCommand.RaiseCanExecuteChanged();
                            AddToCutGeometryDictionary(groupViewModel);
                        }
                    }
                }
            }
        }

        private void RemoveGroupFromGroup(object parameters)
        {
            // Clear the selection and only select this group
            // as we only want to remove this group from
            // the group it belongs to.
            DynamoSelection.Instance.ClearSelection();
            var annotationGuid = this.AnnotationModel.GUID;
            this.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
                new DynamoModel.SelectModelCommand(annotationGuid, Keyboard.Modifiers.AsDynamoType()));
            WorkspaceViewModel.DynamoViewModel.UngroupModelCommand.Execute(null);
            RaisePropertyChanged(nameof(ZIndex));
            Analytics.TrackEvent(Actions.RemovedFrom, Categories.GroupOperations, "Group");
        }

        private void DissolveNestedGroups(object parameters)
        {
            // For this command to work, this needs to be a host group
            if (!this.AnnotationModel.HasNestedGroups) return;

            var hostedAnnotations = this.Nodes.OfType<AnnotationModel>();
            var nodes = GetAllHostedNodes(hostedAnnotations);
            DynamoSelection.Instance.ClearSelection();

            foreach (var annotation in hostedAnnotations)
            {
                var annotationGuid = annotation.GUID;
                this.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
                    new DynamoModel.SelectModelCommand(annotationGuid, Keyboard.Modifiers.AsDynamoType()));
                WorkspaceViewModel.DynamoViewModel.UngroupAnnotationCommand.Execute(null);
            }

            foreach (var node in nodes)
            {
                this.AnnotationModel.AddToTargetAnnotationModel(node);
            }

            RaisePropertyChanged(nameof(ZIndex));
            Analytics.TrackEvent(Actions.RemovedFrom, Categories.GroupOperations, "Group");
        }

        private List<ModelBase> GetAllHostedNodes(IEnumerable<AnnotationModel> hostedAnnotations)
        {
            List<ModelBase> result = new List<ModelBase>();

            foreach (var annotation in hostedAnnotations)
            {
                result.AddRange(annotation.Nodes);
            }

            return result;
        }

        internal bool CanUngroupGroup(object parameters)
        {
            return BelongsToGroup();
        }

        private bool CanChangeFontSize(object obj)
        {
            return true;
        }

        /// <summary>
        /// Command to toggle this group's node preview visibility.
        /// </summary>
        [JsonIgnore]
        public DelegateCommand ToggleIsVisibleGroupCommand { get; private set; }

        /// <summary>
        /// Command to toggle this group's frozen state.
        /// When executed, it will freeze or unfreeze all nodes within the group.
        /// </summary>
        [JsonIgnore]
        public DelegateCommand ToggleIsFrozenGroupCommand { get; private set; }
        #endregion

        public AnnotationViewModel(WorkspaceViewModel workspaceViewModel, AnnotationModel model)
        {
            annotationModel = model;

            this.WorkspaceViewModel = workspaceViewModel;
            this.preferenceSettings = WorkspaceViewModel.DynamoViewModel.PreferenceSettings;
            preferenceSettings.PropertyChanged += OnPreferenceChanged;

            isOptionalInPortsCollapsed = annotationModel.HasToggledOptionalInPorts
                ? annotationModel.IsOptionalInPortsCollapsed
                : preferenceSettings.OptionalInPortsCollapsed;

            isUnconnectedOutPortsCollapsed = annotationModel.HasToggledUnconnectedOutPorts
                ? annotationModel.IsUnconnectedOutPortsCollapsed
                : preferenceSettings.UnconnectedOutPortsCollapsed;

            model.PropertyChanged += model_PropertyChanged;
            model.RemovedFromGroup += OnModelRemovedFromGroup;
            model.AddedToGroup += OnModelAddedToGroup;
            ToggleIsVisibleGroupCommand = new DelegateCommand(ToggleIsVisibleGroup, CanToggleIsVisibleGroup);
            ToggleIsFrozenGroupCommand = new DelegateCommand(ToggleIsFrozenGroup, CanToggleIsFrozenGroup);

            DynamoSelection.Instance.Selection.CollectionChanged += SelectionOnCollectionChanged;

            //https://jira.autodesk.com/browse/QNTM-3770
            //Notes and Groups are serialized as annotations. Do not unselect the node selection during
            //Notes serialization
            if (model.Nodes.Count() > 0)
            {
                // Group is created already.So just populate it.
                var selectNothing = new DynamoModel.SelectModelCommand(Guid.Empty, System.Windows.Input.ModifierKeys.None.AsDynamoType());
                WorkspaceViewModel.DynamoViewModel.ExecuteCommand(selectNothing);
            }

            InPorts = new ObservableCollection<PortViewModel>();
            OutPorts = new ObservableCollection<PortViewModel>();
            OptionalInPorts = new ObservableCollection<PortViewModel>();
            UnconnectedOutPorts = new ObservableCollection<PortViewModel>();

            ViewModelBases = this.WorkspaceViewModel.GetViewModelsInternal(annotationModel.Nodes.Select(x => x.GUID));

            // Add all grouped AnnotationModels to the CutGeometryDictionary.
            // And raise ZIndex changed to make sure nested groups have
            // a higher zIndex than the parent.
            using (NestedGroupsGeometries.DeferCollectionReset())
            {
                foreach (var annotationViewModel in viewModelBases.OfType<AnnotationViewModel>())
                {
                    annotationViewModel.RaisePropertyChanged(nameof(ZIndex));
                    AddToCutGeometryDictionary(annotationViewModel);
                }
            }

            if (!IsExpanded)
            {
                SetGroupInputPorts();
                SetGroupOutPorts();
                CollapseGroupContents(true);
            }
            groupStyleList = new ObservableCollection<Configuration.StyleItem>();
            //This will add the GroupStyles created in Preferences panel to the Group Style Context menu.
            LoadGroupStylesFromPreferences(preferenceSettings.GroupStyleItemsList);

            // Passes the CollapseToMinSize from PreferenceSettings to the model
            if (preferenceSettings.CollapseToMinSize)
            {
                annotationModel.IsCollapsedToMinSize = true;
            }
        }

        private void OnPreferenceChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IPreferences.OptionalInPortsCollapsed):
                    if (!annotationModel.HasToggledOptionalInPorts)
                    {
                        IsOptionalInPortsCollapsed = preferenceSettings.OptionalInPortsCollapsed;
                    }
                    break;
                case nameof(IPreferences.UnconnectedOutPortsCollapsed):
                    if (!annotationModel.HasToggledUnconnectedOutPorts)
                    {
                        IsUnconnectedOutPortsCollapsed = preferenceSettings.UnconnectedOutPortsCollapsed;
                    }
                    break;
                case nameof(IPreferences.CollapseToMinSize):
                    annotationModel.IsCollapsedToMinSize = preferenceSettings.CollapseToMinSize;
                    // Update the boundary only if the group is collapsed
                    if (!IsExpanded)
                    {
                        annotationModel.UpdateBoundaryFromSelection();
                    }
                    break;
            }
        }

        /// <summary>
        /// Creates input ports for the group based on its Nodes.
        /// Input ports that either is connected to a Node outside of the
        /// group, or has a port that is not connected will be used for the group.
        /// This function appends to the inputs
        /// </summary>
        private void SetGroupInputPorts()
        {
            List<PortViewModel> mainPortViewModels;
            List<PortViewModel> optionalPortViewModels;

            // we need to store the original ports here
            // as we need those later for when we
            // need to collapse the groups content
            if (this.AnnotationModel.HasNestedGroups)
            {
                var ownerNodes = Nodes
                    .OfType<AnnotationModel>()
                    .SelectMany(x => x.Nodes.OfType<NodeModel>())
                    .Concat(Nodes.OfType<NodeModel>());

                originalInPorts = GetGroupInPorts(ownerNodes);
            }
            else
            {
                originalInPorts = GetGroupInPorts();
            }

            // Create proxies of the ports so we can
            // visually add them to the group but they
            // should still reference their NodeModel
            // owner
            var newPortViewModels = CreateProxyInPorts(originalInPorts);
            mainPortViewModels = newPortViewModels.Main;
            optionalPortViewModels = newPortViewModels.Optional;

            if (mainPortViewModels != null)
                InPorts.AddRange(mainPortViewModels);

            if (optionalPortViewModels != null)
                OptionalInPorts.AddRange(optionalPortViewModels);
        }

        /// <summary>
        /// Creates output ports for the group based on its Nodes.
        /// Output ports that are not connected will be used for the group.
        /// This function appends to the outports
        /// </summary>
        private void SetGroupOutPorts()
        {
            List<PortViewModel> mainPortViewModels;
            List<PortViewModel> unconnectedPortViewModels;

            // we need to store the original ports here
            // as we need those later for when we
            // need to collapse the groups content
            if (this.AnnotationModel.HasNestedGroups)
            {
                var ownerNodes = Nodes
                    .OfType<AnnotationModel>()
                    .SelectMany(x => x.Nodes.OfType<NodeModel>())
                    .Concat(Nodes.OfType<NodeModel>());

                originalOutPorts = GetGroupOutPorts(ownerNodes);
            }
            else
            {
                originalOutPorts = GetGroupOutPorts();
            }

            // Create proxies of the ports so we can
            // visually add them to the group but they
            // should still reference their NodeModel
            // owner
            var newPortViewModels = CreateProxyOutPorts(originalOutPorts);
            mainPortViewModels = newPortViewModels.Main;
            unconnectedPortViewModels = newPortViewModels.Unconnected;

            if (mainPortViewModels != null)
                OutPorts.AddRange(mainPortViewModels);

            if (unconnectedPortViewModels != null)
                UnconnectedOutPorts.AddRange(unconnectedPortViewModels);
        }

        internal IEnumerable<PortModel> GetGroupInPorts(IEnumerable<NodeModel> ownerNodes = null)
        {
            // If this group does not contain any AnnotationModels
            // we want to get all ports that are connected to something
            // outside of the group
            if (ownerNodes != null)
            {
                return ownerNodes.SelectMany(x => x.InPorts
                        .Where(p => !p.IsConnected ||
                                    !p.Connectors.Any(c => ownerNodes.Contains(c.Start.Owner)) ||
                                    // If the port is connected to any of the groups outports
                                    // we need to return it as well
                                    p.Connectors.Any(c => outPorts.Select(m => m.PortModel).Contains(c.Start)))
                        );
            }

            // If this group does contain any AnnotationModels
            // we only want to get the ports where the owner does
            // not belong to a group.
            return Nodes.OfType<NodeModel>()
                .SelectMany(x => x.InPorts
                    .Where(p => !p.IsConnected ||
                                !p.Connectors.Any(c => Nodes.Contains(c.Start.Owner)) ||
                                // If the port is connected to any of the groups outports
                                // we need to return it as well
                                p.Connectors.Any(c => outPorts.Select(m => m.PortModel).Contains(c.Start)))
                );
        }

        internal IEnumerable<PortModel> GetGroupOutPorts(IEnumerable<NodeModel> ownerNodes = null)
        {
            // If this group does not contain any AnnotationModels
            // we want to get all ports that are connected to something
            // outside of the group
            if (ownerNodes != null)
            {
                return ownerNodes
                    .SelectMany(x => x.OutPorts
                        .Where(p => !p.IsConnected ||
                                    !p.Connectors.All(c => ownerNodes.Contains(c.End.Owner)))
                    );
            }

            // If this group does contain any AnnotationModels
            // we only want to get the ports where the owner does
            // not belong to a group.
            return Nodes.OfType<NodeModel>()
                .SelectMany(x => x.OutPorts
                    .Where(p => !p.IsConnected ||
                                !p.Connectors.All(c => Nodes.Contains(c.End.Owner)))
                );
        }

        private Point2D CalculatePortPosition(PortModel portModel, double verticalPosition)
        {
            double groupHeaderHeight = Height - ModelAreaRect.Height;
            double y = Top + groupHeaderHeight + verticalPosition + verticalOffset + portVerticalMidPoint;
            switch (portModel.PortType)
            {
                case PortType.Input:
                    return new Point2D(Left, y);
                case PortType.Output:
                    if (portModel.Owner is CodeBlockNodeModel)
                    {
                        // Special case because code block outputs are smaller than regular outputs.
                        return new Point2D(Left + Width, y - 7);
                    }
                    return new Point2D(Left + Width, y);
            }
            return new Point2D();
        }

        private (List<PortViewModel> Main, List<PortViewModel> Optional) CreateProxyInPorts(IEnumerable<PortModel> groupPortModels)
        {
            var originalPortViewModels = WorkspaceViewModel.Nodes
                .SelectMany(x => x.InPorts)
                .Where(x => groupPortModels.Contains(x.PortModel))
                .ToList();

            var mainPortViewModels = new List<PortViewModel>();
            var optionalPortViewModels = new List<PortViewModel>();

            double verticalPosition = 0;

            foreach (var groupPort in groupPortModels)
            {
                // Track proxy connection changes while group is collapsed
                groupPort.PropertyChanged += OnPortConnectionChanged;

                var originalPort = originalPortViewModels.FirstOrDefault(x => x.PortModel.GUID == groupPort.GUID);
                if (originalPort != null)
                {
                    var portViewModel = originalPort.CreateProxyPortViewModel(groupPort);

                    if (!originalPort.UsingDefaultValue || groupPort.Connectors.Any())
                    {
                        mainPortViewModels.Add(portViewModel);

                        // Calculate new position for the proxy inports
                        groupPort.Center = CalculatePortPosition(groupPort, verticalPosition);
                        verticalPosition += originalPort.Height;
                    }
                    else
                    {
                        // Defer position setting for optional (unconnected) ports
                        optionalPortViewModels.Add(portViewModel);
                    }
                }
            }
            // Leave space for toggle button
            verticalPosition += portToggleOffset;

            // Position optional input ports
            foreach (var portViewModel in optionalPortViewModels)
            {
                var groupPort = portViewModel.PortModel;
                groupPort.Center = CalculatePortPosition(groupPort, verticalPosition);
                verticalPosition += groupPort.Height;
            }

            return (mainPortViewModels, optionalPortViewModels);
        }

        private (List<PortViewModel> Main, List<PortViewModel> Unconnected) CreateProxyOutPorts(IEnumerable<PortModel> groupPortModels)
        {
            var originalPortViewModels = WorkspaceViewModel.Nodes
                .SelectMany(x => x.OutPorts)
                .Where(x => groupPortModels.Contains(x.PortModel))
                .ToList();

            var mainPortViewModels = new List<PortViewModel>();
            var unconnectedPortViewModels = new List<PortViewModel>();

            double verticalPosition = 0;

            foreach (var groupPort in groupPortModels)
            {
                // Track proxy connection changes while group is collapsed
                groupPort.PropertyChanged += OnPortConnectionChanged;

                var originalPort = originalPortViewModels.FirstOrDefault(x => x.PortModel.GUID == groupPort.GUID);
                if (originalPort != null)
                {
                    var portViewModel = originalPort.CreateProxyPortViewModel(groupPort);

                    if (originalPort.IsConnected)
                    {
                        mainPortViewModels.Add(portViewModel);

                        // Calculate new position for the proxy inports
                        groupPort.Center = CalculatePortPosition(groupPort, verticalPosition);
                        verticalPosition += originalPort.Height;
                    }
                    else
                    {
                        // Defer position setting for unconnected ports
                        unconnectedPortViewModels.Add(portViewModel);
                    }
                }
            }
            // Leave space for toggle button
            verticalPosition += portToggleOffset;

            // Position unconnected output ports
            foreach (var portViewModel in unconnectedPortViewModels)
            {
                var groupPort = portViewModel.PortModel;
                groupPort.Center = CalculatePortPosition(groupPort, verticalPosition);
                verticalPosition += groupPort.Height;
            }

            return (mainPortViewModels, unconnectedPortViewModels);
        }

        private void OnPortConnectionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(PortModel.IsConnected)) return;
            if (sender is not PortModel port) return;

            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                var proxyPortVM = FindPortViewModel(port);
                if (proxyPortVM == null) return;

                bool updatedInputs = false;
                bool updatedOutputs = false;

                if (port.PortType == PortType.Input)
                {
                    // Connected input ports should be in the InPorts collection.
                    if (port.Connectors.Any() && OptionalInPorts.Contains(proxyPortVM))
                    {
                        OptionalInPorts.Remove(proxyPortVM);
                        InPorts.Add(proxyPortVM);
                        updatedInputs = true;
                    }
                    // Disconnected optional ports using default value go to OptionalInPorts.
                    else if (!port.Connectors.Any() && port.UsingDefaultValue)
                    {
                        InPorts.Remove(proxyPortVM);
                        OptionalInPorts.Add(proxyPortVM);
                        updatedInputs = true;
                    }
                }

                if (port.PortType == PortType.Output)
                {
                    if (port.IsConnected && UnconnectedOutPorts.Contains(proxyPortVM))
                    {
                        UnconnectedOutPorts.Remove(proxyPortVM);
                        OutPorts.Add(proxyPortVM);
                        updatedOutputs = true;
                    }
                    else if (!port.IsConnected && !UnconnectedOutPorts.Contains(proxyPortVM))
                    {
                        OutPorts.Remove(proxyPortVM);
                        UnconnectedOutPorts.Add(proxyPortVM);
                        updatedOutputs = true;
                    }
                }

                if (updatedInputs)
                {
                    UpdateProxyPortsPosition();
                    RaisePropertyChanged(nameof(InPorts));
                    RaisePropertyChanged(nameof(OptionalInPorts));
                }
                if (updatedOutputs)
                {

                    UpdateProxyPortsPosition();
                    RaisePropertyChanged(nameof(OutPorts));
                    RaisePropertyChanged(nameof(UnconnectedOutPorts));
                }
            });
        }

        private PortViewModel FindPortViewModel(PortModel model)
        {
            return OutPorts.Concat(UnconnectedOutPorts)
                           .Concat(InPorts)
                           .Concat(OptionalInPorts)
                           .FirstOrDefault(pvm => pvm.PortModel == model);
        }

        private void UnsubscribeFromProxyPortEvents()
        {
            if (originalInPorts != null)
            {
                foreach (var port in originalInPorts)
                    port.PropertyChanged -= OnPortConnectionChanged;
            }

            if (originalOutPorts != null)
            {
                foreach (var port in originalOutPorts)
                    port.PropertyChanged -= OnPortConnectionChanged;
            }
        }

        #region Proxy Port Snapping Events

        private void AttachProxyPortEventHandlers(PortViewModel portViewModel)
        {
            portViewModel.MouseEnter += ProxyPort_MouseEnter;
            portViewModel.MouseLeave += ProxyPort_MouseLeave;
            portViewModel.MouseLeftButtonDown += ProxyPort_MouseLeftButtonDown;
        }

        private void DetachProxyPortEventHandlers()
        {
            foreach (var portViewModel in InPorts)
            {
                portViewModel.MouseEnter -= ProxyPort_MouseEnter;
                portViewModel.MouseLeave -= ProxyPort_MouseLeave;
                portViewModel.MouseLeftButtonDown -= ProxyPort_MouseLeftButtonDown;
            }

            foreach (var portViewModel in OutPorts)
            {
                portViewModel.MouseEnter -= ProxyPort_MouseEnter;
                portViewModel.MouseLeave -= ProxyPort_MouseLeave;
                portViewModel.MouseLeftButtonDown -= ProxyPort_MouseLeftButtonDown;
            }
        }

        private void ProxyPort_MouseEnter(object sender, EventArgs e)
        {
            if (sender is PortViewModel portViewModel)
            {
                portViewModel.EventType = PortEventType.MouseEnter;
                SnapInputEvent?.Invoke(portViewModel);
            }
        }

        private void ProxyPort_MouseLeave(object sender, EventArgs e)
        {
            if (sender is PortViewModel portViewModel)
            {
                portViewModel.EventType = PortEventType.MouseLeave;
                SnapInputEvent?.Invoke(portViewModel);
            }
        }

        private void ProxyPort_MouseLeftButtonDown(object sender, EventArgs e)
        {
            if (sender is PortViewModel portViewModel)
            {
                portViewModel.EventType = PortEventType.MouseLeftButtonDown;
                SnapInputEvent?.Invoke(portViewModel);
            }
        }

        #endregion

        internal void UpdateProxyPortsPosition()
        {
            var parent = WorkspaceViewModel.Annotations
                .FirstOrDefault(x => x.AnnotationModel.ContainsModel(AnnotationModel));

            if (parent != null && !parent.IsExpanded) return;

            double verticalPosition = 0;

            for (int i = 0; i < inPorts.Count(); i++)
            {
                var model = inPorts[i]?.PortModel;
                if (model != null && model.IsProxyPort)
                {
                    // calculate new position for the proxy inports.
                    model.Center = CalculatePortPosition(model, verticalPosition);
                    verticalPosition += model.Height;
                }
            }

            verticalPosition = 0;
            // Update all input ports
            verticalPosition = PositionPorts(inPorts, verticalPosition);
            verticalPosition += portToggleOffset;
            verticalPosition = PositionPorts(optionalInPorts, verticalPosition);

            // Reset vertical position for output ports
            verticalPosition = 0;

            verticalPosition = PositionPorts(outPorts, verticalPosition);
            verticalPosition += portToggleOffset;
            PositionPorts(unconnectedOutPorts, verticalPosition);
        }

        private double PositionPorts(IEnumerable<PortViewModel> portViewModels, double startY)
        {
            double y = startY;

            foreach (var portVM in portViewModels)
            {
                var model = portVM?.PortModel;
                if (model?.IsProxyPort == true)
                {
                    model.Center = CalculatePortPosition(model, y);

                    bool isCondensedCBN = model.Owner is CodeBlockNodeModel &&
                              !InPorts.Contains(portVM) &&
                              !OptionalInPorts.Contains(portVM);

                    double height = isCondensedCBN ?
                        CBNProxyPortVisualHeight :
                        model.Height;

                    y += height;
                }
            }

            return y;
        }

        internal void ClearSelection()
        {
            // Group is created already.So just populate it.
            var selectNothing = new DynamoModel.SelectModelCommand(Guid.Empty, System.Windows.Input.ModifierKeys.None.AsDynamoType());
            WorkspaceViewModel.DynamoViewModel.ExecuteCommand(selectNothing);
        }

        /// <summary>
        /// Collapse all nodes in this group
        /// by settings its IsCollapsed property
        /// to true.
        /// Only the Parent group should handle collapsing
        /// the connector that needs it.
        /// </summary>
        /// <param name="collapseConnectors"></param>
        private void CollapseGroupContents(bool collapseConnectors)
        {
            foreach (var viewModel in ViewModelBases)
            {
                if (viewModel is AnnotationViewModel annotationViewModel)
                {
                    // If there is a group in this group
                    // we collapse that and all of its content.
                    annotationViewModel.IsCollapsed = true;
                    annotationViewModel.CollapseGroupContents(false);
                }

                viewModel.IsCollapsed = true;
                if (viewModel is NodeViewModel nodeViewModel)
                {
                    nodeViewModel.IsNodeInCollapsedGroup = true;
                }
            }

            if (!collapseConnectors) return;

            CollapseConnectors();

            Analytics.TrackEvent(Actions.Collapsed, Categories.GroupOperations);
        }

        private void CollapseConnectors()
        {
            if (originalInPorts is null)
            {
                return;
            }

            var allNodes = this.Nodes
                .OfType<AnnotationModel>()
                .SelectMany(x => x.Nodes.OfType<NodeModel>())
                .Concat(this.Nodes.OfType<NodeModel>());

            var inportsToHide = allNodes
                .SelectMany(x => x.InPorts)
                .Except(originalInPorts)
                .SelectMany(x => x.Connectors)
                .Distinct();

            var outportsToHide = allNodes
                .SelectMany(x => x.OutPorts)
                .SelectMany(x => x.Connectors)
                .Distinct()
                .Where(x => Nodes.Contains(x.End.Owner));

            var connectorsToHide = inportsToHide.Concat(outportsToHide);

            foreach (var connector in connectorsToHide)
            {
                var connectorViewModel = WorkspaceViewModel
                    .Connectors
                    .Where(x => connector.GUID == x.ConnectorModel.GUID)
                    .FirstOrDefault();

                connectorViewModel.IsCollapsed = true;
            }
        }

        private void RedrawConnectors(bool isCollapsedCheck = false)
        {
            if (isCollapsedCheck && IsExpanded)
                return;

            // Suppress boundary updates while redrawing connectors to avoid
            // redundant UpdateBoundaryFromSelection calls caused by connector repositioning
            annotationModel.SuppressBoundaryUpdate = true;

            var allNodes = this.Nodes
                .OfType<AnnotationModel>()
                .SelectMany(x => x.Nodes.OfType<NodeModel>())
                .Concat(this.Nodes.OfType<NodeModel>());

            foreach (var connector in allNodes.SelectMany(x => x.AllConnectors))
            {
                var connectorViewModel = WorkspaceViewModel
                    .Connectors
                    .Where(x => connector.GUID == x.ConnectorModel.GUID)
                    .FirstOrDefault();

                connectorViewModel.Redraw();
                connector.Start.Owner.ReportPosition();
            }

            // Re-enable boundary updates once internal redraw is complete
            annotationModel.SuppressBoundaryUpdate = false;
        }

        /// <summary>
        /// Shows all content of the group by setting
        /// its IsCollapsed property to false.
        /// </summary>
        internal void ShowGroupContents()
        {
            if (!IsExpanded)
            {
                return;
            }

            foreach (var viewModel in ViewModelBases)
            {
                if (viewModel is AnnotationViewModel annotationViewModel)
                {
                    // Update connectors and ports if the nested group is not collapsed
                    if (annotationViewModel.Nodes.Any() && !annotationViewModel.IsCollapsed)
                    {
                        UpdateConnectorsAndPortsOnShowContents(annotationViewModel.Nodes);
                    }
                    // If there is a group in this group
                    // we expand that and all of its content.
                    annotationViewModel.IsCollapsed = false;
                    annotationViewModel.ShowGroupContents();
                }

                viewModel.IsCollapsed = false;

                if (viewModel is NodeViewModel nodeViewModel)
                {
                    nodeViewModel.IsNodeInCollapsedGroup = false;
                }
            }

            UpdateConnectorsAndPortsOnShowContents(Nodes);
            UpdateProxyPortsPosition();

            Analytics.TrackEvent(Actions.Expanded, Categories.GroupOperations);
        }

        private void UpdateConnectorsAndPortsOnShowContents(IEnumerable<ModelBase> nodes)
        {
            foreach (var nodeModel in nodes.OfType<NodeModel>())
            {
                var connectorGuids = nodeModel.AllConnectors
                    .Select(x => x.GUID);

                var connectorViewModels = WorkspaceViewModel.Connectors
                    .Where(x => connectorGuids.Contains(x.ConnectorModel.GUID))
                    .ToList();

                connectorViewModels.ForEach(x => x.IsCollapsed = false);

                // Set IsProxyPort back to false when the group is expanded.
                nodeModel.InPorts.ToList().ForEach(x => x.IsProxyPort = false);
                nodeModel.OutPorts.ToList().ForEach(x => x.IsProxyPort = false);
            }
        }


        /// <summary>
        /// Manages the expansion or collapse of the annotation group in the view model.
        /// </summary>
        private void ManageAnnotationMVExpansionAndCollapse()
        {
            if (InPorts.Any() || OutPorts.Any() || OptionalInPorts.Any() || UnconnectedOutPorts.Any())
            {
                DetachProxyPortEventHandlers();
                InPorts.Clear();
                OutPorts.Clear();
                OptionalInPorts.Clear();
                UnconnectedOutPorts.Clear();
            }

            if (annotationModel.IsExpanded)
            {
                UnsubscribeFromProxyPortEvents();
                this.ShowGroupContents();
            }
            else
            {
                this.SetGroupInputPorts();
                this.SetGroupOutPorts();
                this.CollapseGroupContents(true);
                UpdateProxyPortsPosition();
                RaisePropertyChanged(nameof(NodeContentCount));
            }
            WorkspaceViewModel.HasUnsavedChanges = true;
            AddGroupToGroupCommand.RaiseCanExecuteChanged();
            RaisePropertyChanged(nameof(IsExpanded));
            RedrawConnectors();
            ReportNodesPosition();
        }

        /// <summary>
        /// Adjusts layout by moving nearby elements to prevent overlap when the group expands.
        /// </summary>
        internal void UpdateLayoutForGroupExpansion()
        {
            var model = annotationModel;

            double deltaY = ModelAreaHeight - heightBeforeToggle;
            double deltaX = Width - widthBeforeToggle;

            // Log the current size
            heightBeforeToggle = ModelAreaHeight;
            widthBeforeToggle = Width;

            // Skip layout update if changes are negligible
            if (deltaX < MinChangeThreshold && deltaY < MinChangeThreshold)
                return;

            var alreadyMoved = new HashSet<ModelBase>();
            var undoRecorder = WorkspaceViewModel.Model.UndoRecorder;

            using (undoRecorder.BeginActionGroup())
            {
                if (deltaY > MinChangeThreshold)
                    ApplySpacing(model, isHorizontal: false, alreadyMoved);

                if (deltaX > MinChangeThreshold)
                    ApplySpacing(model, isHorizontal: true, alreadyMoved);
            }
        }

        /// <summary>
        /// Applies spacing to reposition nearby models when a group expands, avoiding overlaps and updating boundaries.
        /// </summary>
        private void ApplySpacing(AnnotationModel expandingGroup, bool isHorizontal, HashSet<ModelBase> alreadyMoved)
        {
            var offsets = GetAffectedModels(expandingGroup, isHorizontal, alreadyMoved);
            if (offsets.Count == 0) return;

            // Ensure changes to all affected models are tracked for Undo
            WorkspaceViewModel.Model.RecordModelsForModification(offsets.Keys.ToList());

            foreach (var (model, offset) in offsets)
            {
                // Skip moving groups directly, just update pinned notes to ensure boundaries update
                if (model is AnnotationModel) continue;
                
                if (isHorizontal)
                    model.X += offset;
                else
                    model.Y += offset;

                model.ReportPosition();
                alreadyMoved.Add(model);
            }

            // To ensure group boundaries are updated correctly
            foreach (var note in WorkspaceViewModel.Model.Annotations
                .SelectMany(group => group.Nodes.OfType<NoteModel>())
                .Where(note => note.PinnedNode != null))
            {
                note.ReportPosition();
            }
        }

        /// <summary>
        /// Calculates all models affected by a group's expansion and the offset needed to reposition them.
        /// </summary>
        private Dictionary<ModelBase, double> GetAffectedModels(
            AnnotationModel expandingGroup,
            bool isHorizontal,
            HashSet<ModelBase> skip)
        {
            // Track already processed items from prior horizontal/vertical pass
            var visited = new HashSet<ModelBase>(skip);

            // Ensure expanding group and all its content (including nested groups) are ignored
            if (!visited.Any())
            {
                visited.Add(expandingGroup);

                foreach (var node in expandingGroup.Nodes)
                {
                    visited.Add(node);

                    if (node is AnnotationModel nestedGroup)
                    {
                        foreach (var nestedNode in nestedGroup.Nodes)
                            visited.Add(nestedNode);
                    }
                }
            }

            var toProcess = new List<ModelBase>();
            var directlyAffected = new List<ModelBase>();
            var otherGroups = WorkspaceViewModel.Model.Annotations.Where(g => !visited.Contains(g));
            var allGroupedItems = WorkspaceViewModel.Model.Annotations.SelectMany(g => g.Nodes);
            double smallestSpacing = double.MaxValue;

            var expandedBounds = GetExpandingGroupBounds(expandingGroup);
            visited.Add(expandingGroup);
            foreach (var node in expandingGroup.Nodes) visited.Add(node);

            // Pick direction-specific helpers
            Func<Rect2D, Rect2D, bool> overlaps = isHorizontal ? IsRightAndVerticallyOverlapping : IsBelowAndHorizontallyOverlapping;
            Func<Rect2D, Rect2D, double> getSpacing = isHorizontal ? (a, b) => b.Left - a.Right : (a, b) => b.Top - a.Bottom;

            // --- Step 1: Find directly affected items ---
            // Groups
            foreach (var group in otherGroups)
            {
                if (overlaps(expandedBounds, group.Rect))
                {
                    var spacing = getSpacing(expandedBounds, group.Rect);
                    if (spacing < MinSpacing)
                    {
                        smallestSpacing = Math.Min(smallestSpacing, spacing);
                        directlyAffected.Add(group);
                        foreach (var node in group.Nodes)
                            visited.Add(node);
                    }
                }
            }

            // Free-standing items
            var freeItems = WorkspaceViewModel.Model.Notes.Cast<ModelBase>()
                .Concat(WorkspaceViewModel.Model.Nodes.Cast<ModelBase>())
                .Where(item => !allGroupedItems.Contains(item))
                .ToList();

            foreach (var item in freeItems)
            {
                if (overlaps(expandedBounds, item.Rect))
                {
                    var spacing = getSpacing(expandedBounds, item.Rect);
                    if (spacing < MinSpacing)
                    {
                        smallestSpacing = Math.Min(smallestSpacing, spacing);
                        directlyAffected.Add(item is NoteModel { PinnedNode: NodeModel pinned } ? pinned : item);
                    }
                }
            }

            // --- Step 2: Initialize movement ---
            var moveBy = MinSpacing - smallestSpacing;
            if (moveBy <= 0) return new();

            var allToMove = new Dictionary<ModelBase, double>();
            foreach (var model in directlyAffected)
            {
                toProcess.Add(model);
                allToMove[model] = moveBy;

                if (model is AnnotationModel group)
                {
                    foreach (var node in group.Nodes)
                    {
                        if (node is not NoteModel { PinnedNode: not null })
                            allToMove[node] = moveBy;
                    }
                }
            }

            // --- Step 3: Recursively propagate movement downstream ---
            for (int i = 0; i < toProcess.Count; i++)
            {
                var current = toProcess[i];
                var offset = allToMove.GetValueOrDefault(current, moveBy);
                var currentBounds = current.Rect;

                // Simulate the moved position
                currentBounds = isHorizontal
                    ? new Rect2D(currentBounds.X, currentBounds.Y, currentBounds.Width + offset, currentBounds.Height)
                    : new Rect2D(currentBounds.X, currentBounds.Y, currentBounds.Width, currentBounds.Height + offset);

                // Groups
                foreach (var group in otherGroups)
                {
                    if (!overlaps(currentBounds, group.Rect)) continue;

                    var requiredOffset = MinSpacing - getSpacing(currentBounds, group.Rect);
                    if (requiredOffset <= 0) continue;

                    allToMove[group] = Math.Max(requiredOffset, allToMove.GetValueOrDefault(group, 0));
                    toProcess.Add(group);

                    foreach (var node in group.Nodes)
                    {
                        if (node is NoteModel note && note.PinnedNode != null) continue;

                        allToMove[node] = Math.Max(requiredOffset, allToMove.GetValueOrDefault(node, 0));
                        visited.Add(node);
                    }
                }

                // Free-standing items
                foreach (var item in freeItems)
                {
                    if (!overlaps(currentBounds, item.Rect)) continue;

                    var requiredOffset = MinSpacing - getSpacing(currentBounds, item.Rect);
                    if (requiredOffset <= 0) continue;

                    if (item is NoteModel note && note.PinnedNode is NodeModel pinned)
                    {
                        if (!visited.Contains(pinned))
                        {
                            allToMove[pinned] = Math.Max(requiredOffset, allToMove.GetValueOrDefault(pinned, 0));
                            toProcess.Add(pinned);
                        }
                    }
                    else
                    {
                        allToMove[item] = Math.Max(requiredOffset, allToMove.GetValueOrDefault(item, 0));
                        toProcess.Add(item);
                    }
                }
            }

            AddExternalConnectorPinsToMove(allToMove);
            return allToMove;
        }

        /// <summary>
        /// Adds external connector pins to the movement list if they connect nodes from different groups.
        /// </summary>
        private void AddExternalConnectorPinsToMove(Dictionary<ModelBase, double> allToMove)
        {
            // Get all moved nodes
            var movedNodes = allToMove.Keys.OfType<NodeModel>().ToList();

            // Collect only moved groups
            var movedGroups = allToMove.Keys.OfType<AnnotationModel>();

            // Map each node to its group
            var nodeToGroup = new Dictionary<NodeModel, AnnotationModel>();

            foreach (var group in movedGroups)
            {
                foreach (var node in group.Nodes.OfType<NodeModel>())
                {
                    nodeToGroup[node] = group;
                }
            }

            foreach (var node in movedNodes)
            {
                if (!allToMove.TryGetValue(node, out var nodeOffset)) continue;

                foreach (var connector in node.AllConnectors)
                {
                    var startNode = connector.Start.Owner;
                    var endNode = connector.End.Owner;

                    bool sameGroup =
                        nodeToGroup.TryGetValue(startNode, out var groupA) &&
                        nodeToGroup.TryGetValue(endNode, out var groupB) &&
                        groupA == groupB;

                    if (sameGroup) continue;

                    // Add each pin with the largest offset from its connected node (if multiple nodes affect it)
                    foreach (var pin in connector.ConnectorPinModels)
                    {
                        if (!allToMove.TryGetValue(pin, out var existingOffset))
                        {
                            allToMove[pin] = nodeOffset;
                        }
                        else
                        {
                            allToMove[pin] = Math.Max(existingOffset, nodeOffset);
                        }
                    }
                }
            }
        }

        private static Rect2D GetExpandingGroupBounds(AnnotationModel group)
        {
            var width = group.Width;
            var height = group.ModelAreaHeight + group.TextBlockHeight;           

            return new Rect2D(group.X, group.Y, width, height);
        }

        private bool IsBelowAndHorizontallyOverlapping(Rect2D thisGroup, Rect2D other)
        {
            return other.Top > thisGroup.Top &&
                other.Left < thisGroup.Right &&
                other.Right > thisGroup.Left;
        }

        private bool IsRightAndVerticallyOverlapping(Rect2D thisGroup, Rect2D other)
        {
            return other.Left > thisGroup.Left &&
                other.Top < thisGroup.Bottom &&
                other.Bottom > thisGroup.Top;
        }

        private void HandlePrePortToggleLayout()
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                UpdateLayoutForGroupExpansion();
            }),
            DispatcherPriority.ApplicationIdle);
        }

        private void UpdateFontSize(object parameter)
        {
            if (parameter == null) return;

            WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
                new DynamoModel.UpdateModelValueCommand(
                    Guid.Empty, AnnotationModel.GUID, "FontSize", parameter.ToString()));

            WorkspaceViewModel.DynamoViewModel.RaiseCanExecuteUndoRedo();
        }

        /// <summary>
        /// This method will be called by the ChangeGroupStyleCommand when a GroupStyle is selected from the ContextMenu
        /// </summary>
        /// <param name="itemEntryParameter">GroupStyle item selected</param>
        internal void UpdateGroupStyle(GroupStyleItem itemEntryParameter)
        {
            if (itemEntryParameter == null) return;

            Background = (Color)ColorConverter.ConvertFromString("#" + itemEntryParameter.HexColorString);
            FontSize = (double)itemEntryParameter.FontSize;
            GroupStyleId = itemEntryParameter.GroupStyleId;

            WorkspaceViewModel.HasUnsavedChanges = true;
        }

        /// <summary>
        /// This method loads the group styles defined by the user and stored in the xml file
        /// </summary>
        /// <param name="styleItemsList"></param>
        /// <returns></returns>
        private void LoadGroupStylesFromPreferences(IEnumerable<Configuration.StyleItem> styleItemsList)
        {
            preferencesStyleItemsList = styleItemsList;

            var defaultGroupStylesList = styleItemsList.Where(style => style.IsDefault == true);
            var customGroupStylesList = styleItemsList.Where(style => style.IsDefault == false);

            //Adds to the list the Default Group Styles created by Dynamo
            groupStyleList.AddRange(defaultGroupStylesList);

            //Adds the separator between the Default Group Styles and the Custom Group Styles
            groupStyleList.Add(new GroupStyleSeparator());

            //Adds to the list the Custom Group Styles created by the user
            groupStyleList.AddRange(customGroupStylesList);
        }

        /// <summary>
        /// This method will be executed when the MenuIte.SubmenuOpened event is executed
        /// The purpose is adding to the GroupStyles ContextMenu the Styles added in the Preferences panel.
        /// </summary>
        internal void ReloadGroupStyles()
        {
            if (preferencesStyleItemsList == null) return;
            groupStyleList.Clear();

            LoadGroupStylesFromPreferences(preferencesStyleItemsList);
        }

        /// <summary>
        /// Selects this group and models within it.
        /// </summary>
        internal void SelectAll()
        {
            var annotationGuid = this.AnnotationModel.GUID;
            this.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
                new DynamoModel.SelectModelCommand(annotationGuid, Keyboard.Modifiers.AsDynamoType()));

            //Select all the models inside the group - This avoids some performance bottleneck 
            //with many nodes selected at the same time - which makes moving the group very slow

            var groupedGroupsNodes = this.AnnotationModel.Nodes.OfType<AnnotationModel>().SelectMany(x => x.Nodes);
            DynamoSelection.Instance.Selection.AddRange(this.AnnotationModel.Nodes.Concat(groupedGroupsNodes));
        }

        internal void SelectGroupOnly()
        {
            var annotationGuid = this.AnnotationModel.GUID;
            DynamoSelection.Instance.Selection.Add(AnnotationModel);
            this.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
                new DynamoModel.SelectModelCommand(annotationGuid, Keyboard.Modifiers.AsDynamoType()));
        }

        internal void AddGroupAndGroupedNodesToSelection()
        {
            var guids = this.AnnotationModel.Nodes.Select(n => n.GUID).ToList();
            guids.Add(this.AnnotationModel.GUID);

            this.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
                new DynamoModel.SelectModelCommand(guids, Keyboard.Modifiers.AsDynamoType()));

        }

        private void SelectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            AddToGroupCommand.RaiseCanExecuteChanged();
            AddGroupToGroupCommand.RaiseCanExecuteChanged();
            RemoveGroupFromGroupCommand.RaiseCanExecuteChanged();
        }

        private void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "X":
                    RaisePropertyChanged("Left");
                    break;
                case "Y":
                    RaisePropertyChanged("Top");
                    break;
                case "Width":
                    RaisePropertyChanged("Width");
                    RaisePropertyChanged(nameof(ModelAreaRect));
                    UpdateAllGroupedGroups();
                    break;
                case "Height":
                    RaisePropertyChanged("Height");
                    UpdateAllGroupedGroups();
                    break;
                case nameof(AnnotationDescriptionText):
                    RaisePropertyChanged(nameof(AnnotationDescriptionText));
                    break;
                case "AnnotationText":
                    RaisePropertyChanged("AnnotationText");
                    break;
                case "Background":
                    RaisePropertyChanged("Background");
                    break;
                case "IsSelected":
                    RaisePropertyChanged("PreviewState");
                    break;
                case "FontSize":
                    RaisePropertyChanged("FontSize");
                    break;
                case "SelectedModels":
                    this.AnnotationModel.UpdateBoundaryFromSelection();
                    break;
                case nameof(AnnotationModel.Nodes):
                    ViewModelBases = this.WorkspaceViewModel.GetViewModelsInternal(annotationModel.Nodes.Select(x => x.GUID));
                    HandleNodesCollectionChanges();
                    break;
                case nameof(AnnotationModel.ModelAreaHeight):
                    RaisePropertyChanged(nameof(ModelAreaHeight));
                    RaisePropertyChanged(nameof(ModelAreaRect));
                    RaisePropertyChanged(nameof(Width));
                    break;
                case nameof(AnnotationModel.Position):
                    RaisePropertyChanged(nameof(ModelAreaRect));
                    RaisePropertyChanged(nameof(AnnotationModel.Position));
                    UpdateProxyPortsPosition();
                    break;
                case nameof(IsExpanded):
                    ManageAnnotationMVExpansionAndCollapse();
                    break;
                case nameof(AnnotationModel.IsOptionalInPortsCollapsed):
                    isOptionalInPortsCollapsed = annotationModel.IsOptionalInPortsCollapsed;
                    RaisePropertyChanged(nameof(IsOptionalInPortsCollapsed));
                    break;
                case nameof(AnnotationModel.IsUnconnectedOutPortsCollapsed):
                    IsUnconnectedOutPortsCollapsed = annotationModel.IsUnconnectedOutPortsCollapsed;
                    RaisePropertyChanged(nameof(IsUnconnectedOutPortsCollapsed));
                    break;
            }
        }

        private void OnModelRemovedFromGroup(object sender, EventArgs e)
        {
            Analytics.TrackEvent(Actions.RemovedFrom, Categories.GroupOperations, "Node");
            RaisePropertyChanged(nameof(ZIndex));
        }

        private void OnModelAddedToGroup(object sender, EventArgs e)
        {
            Analytics.TrackEvent(Actions.AddedTo, Categories.GroupOperations, "Group");
            RaisePropertyChanged(nameof(ZIndex));
        }

        private void UpdateAllGroupedGroups()
        {
            try
            {
                using (NestedGroupsGeometries.DeferCollectionReset())
                {
                    if (ViewModelBases != null)
                    {
                        ViewModelBases
                            .OfType<AnnotationViewModel>()
                            .ToList()?
                            .ForEach(x => UpdateGroupCutGeometry(x));
                    }
                }
            }
            catch (Exception ex)
            {
                WorkspaceViewModel.DynamoViewModel.Model.Logger.Log("Error updating all grouped groups");
                WorkspaceViewModel.DynamoViewModel.Model.Logger.Log(ex);
                WorkspaceViewModel.DynamoViewModel.Model.Logger.Log(ex.StackTrace);
            }
        }

        private void HandleNodesCollectionChanges()
        {
            var allGroupedGroups = Nodes.OfType<AnnotationModel>();
            var removedFromGroup = GroupIdToCutGeometry.Keys
                .ToList()
                .Except(allGroupedGroups.Select(x => x.GUID));

            using (NestedGroupsGeometries.DeferCollectionReset())
            {
                foreach (var key in removedFromGroup)
                {
                    RemoveKeyFromCutGeometryDictionary(key);
                }

                var addedToGroup = allGroupedGroups
                    .Select(x => x.GUID)
                    .Except(GroupIdToCutGeometry.Keys.ToList());

                foreach (var key in addedToGroup)
                {
                    var groupViewModel = ViewModelBases.OfType<AnnotationViewModel>()
                        .Where(x => x.AnnotationModel.GUID == key)
                        .FirstOrDefault();

                    AddToCutGeometryDictionary(groupViewModel);
                }
            }

            WorkspaceViewModel.HasUnsavedChanges = true;
            this.AnnotationModel.UpdateGroupFrozenStatus();
        }

        private void RemoveKeyFromCutGeometryDictionary(Guid groupGuid)
        {
            if (GroupIdToCutGeometry is null ||
                !GroupIdToCutGeometry.ContainsKey(groupGuid))
            {
                return;
            }

            NestedGroupsGeometries.Remove(GroupIdToCutGeometry[groupGuid]);
            GroupIdToCutGeometry.Remove(groupGuid);

            var groupViewModel = this.WorkspaceViewModel.Annotations
                .Where(x => x.AnnotationModel.GUID == groupGuid)
                .FirstOrDefault();

            if (groupViewModel != null)
            {
                groupViewModel.PropertyChanged -= GroupViewModel_PropertyChanged;
            }
        }

        private void AddToCutGeometryDictionary(AnnotationViewModel annotationViewModel)
        {
            var key = annotationViewModel.AnnotationModel.GUID;
            if (GroupIdToCutGeometry.ContainsKey(key)) return;

            int nextPos = NestedGroupsGeometries.Count;
            var geo = CreateRectangleGeometry(annotationViewModel);
            NestedGroupsGeometries.Insert(nextPos, geo);
            GroupIdToCutGeometry[key] = geo;

            annotationViewModel.PropertyChanged += GroupViewModel_PropertyChanged;
        }

        private RectangleGeometry CreateRectangleGeometry(AnnotationViewModel annotationViewModel)
        {
            return new RectangleGeometry(
                new System.Windows.Rect(
                    new System.Windows.Point(
                        Math.Abs(annotationViewModel.Left - this.Left), (annotationViewModel.Top + annotationViewModel.AnnotationModel.TextBlockHeight) - (this.Top + this.AnnotationModel.TextBlockHeight)),
                    new System.Windows.Size(
                        annotationViewModel.Width, annotationViewModel.ModelAreaHeight)
                    )
                );
        }

        private void GroupViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(AnnotationModel.Position):
                case nameof(Width):
                case nameof(Height):
                    UpdateGroupCutGeometry(sender as AnnotationViewModel);
                    break;
                default:
                    break;
            }
        }

        private void UpdateGroupCutGeometry(AnnotationViewModel annotationViewModel)
        {
            var key = annotationViewModel.AnnotationModel.GUID;
            if (GroupIdToCutGeometry == null ||
                !GroupIdToCutGeometry.ContainsKey(key))
            {
                return;
            }
            var geo = GroupIdToCutGeometry[key];
            if (geo != null)
            {
                int index = NestedGroupsGeometries.IndexOf(geo);
                if (index >= 0 && index < NestedGroupsGeometries.Count)
                {
                    NestedGroupsGeometries[index] = CreateRectangleGeometry(annotationViewModel);
                }
            }
        }

        private bool BelongsToGroup()
        {
            return WorkspaceViewModel.Model.Annotations.ContainsModel(this.annotationModel);
        }

        internal void ToggleIsVisibleGroup(object parameters)
        {
            DynamoSelection.Instance.ClearSelection();
            var nodesInGroup = this.AnnotationModel.Nodes.Select(n => n.GUID).ToList();

            var command = new DynamoModel.UpdateModelValueCommand(Guid.Empty,
            nodesInGroup, nameof(this.AnnotationModel.IsVisible), (!this.AnnotationModel.IsVisible).ToString());

            this.AnnotationModel.IsVisible = !this.AnnotationModel.IsVisible;
            WorkspaceViewModel.DynamoViewModel.Model.ExecuteCommand(command);
            WorkspaceViewModel.DynamoViewModel.RaiseCanExecuteUndoRedo();
            WorkspaceViewModel.HasUnsavedChanges = true;

            Analytics.TrackEvent(Actions.Preview, Categories.GroupOperations, this.AnnotationModel.IsVisible.ToString());
        }

        internal bool CanToggleIsVisibleGroup(object parameters)
        {
            return true;
        }

        internal void ToggleIsFrozenGroup(object parameters)
        {
            DynamoSelection.Instance.ClearSelection();
            bool newFrozenState = !this.AnnotationModel.IsFrozen;

            // Collect all nodes inside this group
            var nodesInGroup = this.AnnotationModel.Nodes.
                OfType<NodeModel>()
                .Select(n => n.GUID)
                .ToList();

            // Collect all nodes inside the nested groups
            var nestedGroups = this.AnnotationModel.Nodes.OfType<AnnotationModel>();
            foreach (var nestedGroup in nestedGroups)
            {
                nestedGroup.IsFrozen = newFrozenState;
                nodesInGroup.AddRange(nestedGroup.Nodes.OfType<NodeModel>().Select(n => n.GUID));
            }

            var command = new DynamoModel.UpdateModelValueCommand(
                Guid.Empty,
                nodesInGroup,
                nameof(this.AnnotationModel.IsFrozen),
                newFrozenState.ToString());

            this.AnnotationModel.IsFrozen = newFrozenState;

            WorkspaceViewModel.DynamoViewModel.Model.ExecuteCommand(command);
            WorkspaceViewModel.DynamoViewModel.RaiseCanExecuteUndoRedo();
            WorkspaceViewModel.HasUnsavedChanges = true;

            Analytics.TrackEvent(Actions.Freeze, Categories.GroupOperations, newFrozenState.ToString());
        }

        internal bool CanToggleIsFrozenGroup(object parameters)
        {
            return true;
        }

        public override void Dispose()
        {
            annotationModel.PropertyChanged -= model_PropertyChanged;
            annotationModel.RemovedFromGroup -= OnModelRemovedFromGroup;
            annotationModel.AddedToGroup -= OnModelAddedToGroup;

            DynamoSelection.Instance.Selection.CollectionChanged -= SelectionOnCollectionChanged;
            base.Dispose();
        }
    }
}
