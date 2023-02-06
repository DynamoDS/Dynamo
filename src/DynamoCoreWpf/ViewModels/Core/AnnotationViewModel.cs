using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using Dynamo.Configuration;
using Dynamo.Graph;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Nodes;
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
        private AnnotationModel annotationModel;
        private IEnumerable<PortModel> originalInPorts;
        private IEnumerable<PortModel> originalOutPorts;
        private Dictionary<Guid, int> GroupIdToCutGeometryIndex = new Dictionary<Guid, int>();
        // vertical offset accounts for the port margins
        private const int verticalOffset = 20;
        private const int portVerticalMidPoint = 17;
        private ObservableCollection<Dynamo.Configuration.StyleItem> groupStyleList;
        private IEnumerable<Configuration.StyleItem> preferencesStyleItemsList;
        private PreferenceSettings preferenceSettings;

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
                annotationModel.IsExpanded = value;
                InPorts.Clear();
                OutPorts.Clear();
                if (value)
                {
                    this.ShowGroupContents();       
                }
                else
                {
                    this.SetGroupInputPorts();
                    this.SetGroupOutPorts();
                    this.CollapseGroupContents(true);
                    RaisePropertyChanged(nameof(NodeContentCount));
                }
                WorkspaceViewModel.HasUnsavedChanges = true;
                AddGroupToGroupCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged(nameof(IsExpanded));
                RedrawConnectors();
                ReportNodesPosition();
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
        /// This property getter returns an empty GeometryCollection
        /// </summary>
        [Obsolete("This property will be removed in Dynamo 3.0 - please use NestedGroupsGeometries instead.")]
        public GeometryCollection NestedGroupsGeometryCollection
        {
            get => new GeometryCollection();
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

        private bool CanAddGroupToGroup(object obj)
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

            foreach (var annotation in hostedAnnotations){
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

        private bool CanUngroupGroup(object parameters)
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
        #endregion

        public AnnotationViewModel(WorkspaceViewModel workspaceViewModel, AnnotationModel model)
        {
            annotationModel = model;

            this.WorkspaceViewModel = workspaceViewModel;
            this.preferenceSettings = WorkspaceViewModel.DynamoViewModel.PreferenceSettings;
            model.PropertyChanged += model_PropertyChanged;
            model.RemovedFromGroup += OnModelRemovedFromGroup;
            model.AddedToGroup += OnModelAddedToGroup;
            ToggleIsVisibleGroupCommand = new DelegateCommand(ToggleIsVisibleGroup, CanToggleIsVisibleGroup);

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

            ViewModelBases = this.WorkspaceViewModel.GetViewModelsInternal(annotationModel.Nodes.Select(x => x.GUID));

            // Add all grouped AnnotaionModels to the CutGeometryDictionary.
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
}


        /// <summary>
        /// Creates input ports for the group based on its Nodes.
        /// Input ports that either is connected to a Node outside of the
        /// group, or has a port that is not connected will be used for the group.
        /// This function appends to the inputs
        /// </summary>
        private void SetGroupInputPorts()
        {
            List<PortViewModel> newPortViewModels;

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
            newPortViewModels = CreateProxyInPorts(originalInPorts);

            if (newPortViewModels == null) return;
            InPorts.AddRange(newPortViewModels);
            return;
        }

        /// <summary>
        /// Creates output ports for the group based on its Nodes.
        /// Output ports that are not connected will be used for the group.
        /// This function appends to the outports
        /// </summary>
        private void SetGroupOutPorts()
        {
            List<PortViewModel> newPortViewModels;

            // we need to store the original ports here
            // as we need thoese later for when we
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
            newPortViewModels = CreateProxyOutPorts(originalOutPorts);

            if (newPortViewModels == null) return;
            OutPorts.AddRange(newPortViewModels);
            return;
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
                        return new Point2D(Left + Width, y - 8);
                    }
                    return new Point2D(Left + Width, y);
            }
            return new Point2D();
        }

        private List<PortViewModel> CreateProxyInPorts(IEnumerable<PortModel> groupPortModels)
        {
            var originalPortViewModels = WorkspaceViewModel.Nodes
                .SelectMany(x => x.InPorts)
                .Where(x => groupPortModels.Contains(x.PortModel))
                .ToList();

            var newPortViewModels = new List<PortViewModel>();
            double verticalPosition = 0;
            foreach (var groupPort in groupPortModels)
            {
                var originalPort = originalPortViewModels.FirstOrDefault(x => x.PortModel.GUID == groupPort.GUID);
                if (originalPort != null)
                {
                    var portViewModel = originalPort.CreateProxyPortViewModel(groupPort);
                    newPortViewModels.Add(portViewModel);
                    // calculate new position for the proxy outports
                    groupPort.Center = CalculatePortPosition(groupPort, verticalPosition);
                    verticalPosition += originalPort.Height;
                }
            }
            return newPortViewModels;
        }

        private List<PortViewModel> CreateProxyOutPorts(IEnumerable<PortModel> groupPortModels)
        {
            var originalPortViewModels = WorkspaceViewModel.Nodes
                .SelectMany(x => x.OutPorts)
                .Where(x => groupPortModels.Contains(x.PortModel))
                .ToList();

            var newPortViewModels = new List<PortViewModel>();
            double verticalPosition = 0;
            foreach (var group in groupPortModels)
            {
                var originalPort = originalPortViewModels.FirstOrDefault(x => x.PortModel.GUID == group.GUID);
                if (originalPort != null)
                {
                    var portViewModel = originalPort.CreateProxyPortViewModel(group);
                    newPortViewModels.Add(portViewModel);
                    // calculate new position for the proxy outports
                    group.Center = CalculatePortPosition(group, verticalPosition);
                    verticalPosition += originalPort.Height;
                }
            }

            return newPortViewModels;
        }

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
            for (int i = 0; i < outPorts.Count(); i++)
            {
                var model = outPorts[i]?.PortModel;
                if (model != null && model.IsProxyPort)
                {
                    // calculate new position for the proxy outports.
                    model.Center = CalculatePortPosition(model, verticalPosition);
                    verticalPosition += model.Height;
                }
            }
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

        private void RedrawConnectors()
        {
            var allNodes = this.Nodes
                .OfType<AnnotationModel>()
                .SelectMany(x => x.Nodes.OfType<NodeModel>())
                .Concat(this.Nodes.OfType<NodeModel>());

            foreach (var connector in allNodes.SelectMany(x=>x.AllConnectors))
            {
                var connectorViewModel = WorkspaceViewModel
                    .Connectors
                    .Where(x => connector.GUID == x.ConnectorModel.GUID)
                    .FirstOrDefault();

                connectorViewModel.Redraw();
                connector.Start.Owner.ReportPosition();
            }
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
                    if(annotationViewModel.Nodes.Any() && !annotationViewModel.IsCollapsed)
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
            using (NestedGroupsGeometries.DeferCollectionReset())
            {
                ViewModelBases
                    .OfType<AnnotationViewModel>()
                    .ToList()
                    .ForEach(x => UpdateGroupCutGeometry(x));
            }
        }

        private void HandleNodesCollectionChanges()
        {
            var allGroupedGroups = Nodes.OfType<AnnotationModel>();
            var removedFromGroup = GroupIdToCutGeometryIndex.Keys
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
                    .Except(GroupIdToCutGeometryIndex.Keys.ToList());

                foreach (var key in addedToGroup)
                {
                    var groupViewModel = ViewModelBases.OfType<AnnotationViewModel>()
                        .Where(x => x.AnnotationModel.GUID == key)
                        .FirstOrDefault();

                    AddToCutGeometryDictionary(groupViewModel);
                }
            }

            WorkspaceViewModel.HasUnsavedChanges = true;
        }

        private void RemoveKeyFromCutGeometryDictionary(Guid groupGuid)
        {
            if (GroupIdToCutGeometryIndex is null ||
                !GroupIdToCutGeometryIndex.ContainsKey(groupGuid))
            {
                return;
            }

            NestedGroupsGeometries.RemoveAt(GroupIdToCutGeometryIndex[groupGuid]);
            GroupIdToCutGeometryIndex.Remove(groupGuid);

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
            if (GroupIdToCutGeometryIndex.ContainsKey(key)) return;

            int nextPos = NestedGroupsGeometries.Count;
            NestedGroupsGeometries.Insert(nextPos, CreateRectangleGeometry(annotationViewModel));
            GroupIdToCutGeometryIndex[key] = nextPos;

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
            if (GroupIdToCutGeometryIndex == null ||
                !GroupIdToCutGeometryIndex.ContainsKey(key))
            {
                return;
            }
            var index = GroupIdToCutGeometryIndex[key];
            if (index >= 0 &&
                index < NestedGroupsGeometries.Count)
            {
                NestedGroupsGeometries[index] = CreateRectangleGeometry(annotationViewModel);
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
