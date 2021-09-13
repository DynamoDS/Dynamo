using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using Dynamo.Graph;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.UI.Commands;
using Dynamo.Utilities;
using Newtonsoft.Json;
using Color = System.Windows.Media.Color;

namespace Dynamo.ViewModels
{
    public class AnnotationViewModel : ViewModelBase
    {
        private AnnotationModel annotationModel;
        private IEnumerable<PortModel> originalInPorts;
        private IEnumerable<PortModel> originalOutPorts;
        private Dictionary<string, RectangleGeometry> GroupIdToCutGeometry = new Dictionary<string, RectangleGeometry>();

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
                if (value)
                {
                    this.ShowGroupContents();
                }
                else
                {
                    this.SetGroupInputPorts();
                    this.SetGroupOutPorts();
                    this.CollapseGroupContents(true);
                    RaisePropertyChanged(nameof(InbetweenNodesCount));
                }
                RaisePropertyChanged(nameof(IsExpanded));
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

        private ObservableCollection<NodeViewModel> inputNodes;
        /// <summary>
        /// Collection of the groups input NodeViewModels.
        /// This is used for displaying node icons when the
        /// group is collapsed.
        /// </summary>
        [JsonIgnore]
        public ObservableCollection<NodeViewModel> InputNodes
        {
            get => inputNodes;
            private set => inputNodes = value;
        }

        private ObservableCollection<NodeViewModel> outputNodes;
        /// <summary>
        /// Collection of the groups output NodeViewModels.
        /// This is used for displaying node icons when the
        /// group is collapsed.
        /// </summary>
        [JsonIgnore]
        public ObservableCollection<NodeViewModel> OutputNodes
        {
            get => outputNodes;
            private set => outputNodes = value;
        }

        /// <summary>
        /// Counter of all nodes in the group that
        /// aren't either an input or output node.
        /// This is used to display the amount of nodes
        /// the are in between the input and output nodes.
        /// </summary>
        public int InbetweenNodesCount
        {
            get => Nodes
                .Except(InputNodes
                    .Select(x => x.NodeModel)
                    .Union(OutputNodes.Select(x => x.NodeModel)))
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
        public GeometryCollection NestedGroupsGeometryCollection
        {
            get => new GeometryCollection(GroupIdToCutGeometry.Values.Select(x => x));
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

        private bool CanAddToGroup(object obj)
        {
            return DynamoSelection.Instance.Selection.Count >= 0;
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
                        this.AnnotationModel.AddToSelectedModels(model, true);
                    }
                }
            }
        }

        private bool CanAddGroupToGroup(object obj)
        {
            // First make sure this group is selected
            // and that it does not already belong to
            // another group
            if (!this.AnnotationModel.IsSelected ||
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

                foreach (var model in selectedModels)
                {
                    WorkspaceViewModel.DynamoViewModel.AddGroupToGroupModelCommand.Execute(this.AnnotationModel.GUID);
                    if (Nodes.Contains(model))
                    {
                        var groupViewModel = ViewModelBases.OfType<AnnotationViewModel>()
                            .Where(x => x.AnnotationModel.GUID == model.GUID)
                            .FirstOrDefault();
                        groupViewModel.RaisePropertyChanged(nameof(ZIndex));
                        groupViewModel.AddToGroupCommand.RaiseCanExecuteChanged();
                        groupViewModel.AddGroupToGroupCommand.RaiseCanExecuteChanged();
                        groupViewModel.RemoveGroupFromGroupCommand.RaiseCanExecuteChanged();
                        AddToCutGeometryDictionary(groupViewModel);
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
        }

        private bool CanUngroupGroup(object parameters)
        {
            return BelongsToGroup();
        }

        private bool CanChangeFontSize(object obj)
        {
            return true;
        }
        #endregion

        public AnnotationViewModel(WorkspaceViewModel workspaceViewModel, AnnotationModel model)
        {
            annotationModel = model;           
            this.WorkspaceViewModel = workspaceViewModel;
            model.PropertyChanged += model_PropertyChanged;
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
            InputNodes = new ObservableCollection<NodeViewModel>();
            OutputNodes = new ObservableCollection<NodeViewModel>();

            InPorts.CollectionChanged += InPorts_CollectionChanged;
            OutPorts.CollectionChanged += OutPorts_CollectionChanged;

            ViewModelBases = this.WorkspaceViewModel.GetViewModelsInternal(annotationModel.Nodes.Select(x => x.GUID));

            // Add all grouped AnnotaionModels to the CutGeometryDictionary.
            ViewModelBases.OfType<AnnotationViewModel>()
                .ToList()
                .ForEach(x => AddToCutGeometryDictionary(x));

            if (!IsExpanded)
            {
                SetGroupInputPorts();
                SetGroupOutPorts();
                CollapseGroupContents(true);
            }
        }

        /// <summary>
        /// Creates input ports for the group based on its Nodes.
        /// Input ports that either is connected to a Node outside of the
        /// group, or has a port that is not connected will be used for the group.
        /// </summary>
        internal void SetGroupInputPorts()
        {
            InPorts.Clear();
            List<PortViewModel> newPortViewModels;

            if (!AnnotationModel.HasNestedGroups)
            {
                // we need to store the original ports here
                // as we need thoese later for when we
                // need to collapse the groups content
                originalInPorts = GetGroupInPorts();

                // Create proxies of the ports so we can
                // visually add them to the group but they
                // should still reference their NodeModel
                // owner
                newPortViewModels = CreateProxyPorts(originalInPorts);

                if (newPortViewModels == null) return;
                InPorts.AddRange(newPortViewModels);
                return;
            }

            // We need to get all NodeModels for the nested groups 
            // here, as we will have to show any ports belonging to a
            // node that are either unconnected or connected to outside
            // of the owner group.
            var ownerGroupNodes = Nodes.OfType<AnnotationModel>()
                .SelectMany(x=>x.Nodes.OfType<NodeModel>())
                .Concat(Nodes.OfType<NodeModel>());

            // Find the needed input ports of all the nested groups
            var groupedGroupsInPorts = new List<PortModel>();
            foreach (var group in ViewModelBases.OfType<AnnotationViewModel>())
            {
                groupedGroupsInPorts.AddRange(group.GetGroupInPorts(ownerGroupNodes));
            }

            originalInPorts = GetGroupInPorts().Concat(groupedGroupsInPorts);

            newPortViewModels = CreateProxyPorts(originalInPorts);

            if (newPortViewModels == null) return;
            InPorts.AddRange(newPortViewModels);
        }

        /// <summary>
        /// Creates output ports for the group based on its Nodes.
        /// Output ports that are not connected will be used for the group.
        /// </summary>
        internal void SetGroupOutPorts()
        {
            OutPorts.Clear();
            List<PortViewModel> newPortViewModels;

            if (!AnnotationModel.HasNestedGroups)
            {
                // we need to store the original ports here
                // as we need thoese later for when we
                // need to collapse the groups content
                originalOutPorts = GetGroupOutPorts();

                // Create proxies of the ports so we can
                // visually add them to the group but they
                // should still reference their NodeModel
                // owner
                newPortViewModels = CreateProxyPorts(originalOutPorts);

                if (newPortViewModels == null) return;
                OutPorts.AddRange(newPortViewModels);
                return;
            }

            // We need to get all NodeModels for the nested groups 
            // here, as we will have to show any ports belonging to a
            // node that are either unconnected or connected to outside
            // of the owner group.
            var ownerGroupNodes = Nodes.OfType<AnnotationModel>()
                .SelectMany(x => x.Nodes.OfType<NodeModel>())
                .Concat(Nodes.OfType<NodeModel>());

            var groupedGroupsOutPorts = new List<PortModel>();
            foreach (var group in ViewModelBases.OfType<AnnotationViewModel>())
            {
                groupedGroupsOutPorts.AddRange(group.GetGroupOutPorts(ownerGroupNodes));
            }

            originalOutPorts = GetGroupOutPorts().Concat(groupedGroupsOutPorts);

            newPortViewModels = CreateProxyPorts(originalOutPorts);

            if (newPortViewModels == null) return;
            OutPorts.AddRange(newPortViewModels);
        }

        internal IEnumerable<PortModel> GetGroupInPorts(IEnumerable<NodeModel> ownerNodes = null)
        {
            // If this group does not contain any AnnotationModels
            // we want to get all ports that are connected to something
            // outside of the group
            if (ownerNodes != null)
            {
                return Nodes.OfType<NodeModel>()
                    .SelectMany(x => x.InPorts
                        .Where(p => !p.IsConnected || !p.Connectors.Any(c => ownerNodes.Contains(c.Start.Owner)))
                    );
            }

            // If this group does contain any AnnotationModels
            // we only want to get the ports where the owner does
            // not belong to a group.
            return Nodes.OfType<NodeModel>()
                .SelectMany(x => x.InPorts
                    .Where(p => !p.IsConnected || !p.Connectors.Any(c => Nodes.Contains(c.Start.Owner)))
                );
        }

        internal IEnumerable<PortModel> GetGroupOutPorts(IEnumerable<NodeModel> ownerNodes = null)
        {
            // If this group does not contain any AnnotationModels
            // we want to get all ports that are connected to something
            // outside of the group
            if (ownerNodes != null)
            {
                return Nodes.OfType<NodeModel>()
                    .SelectMany(x => x.OutPorts
                        .Where(p => !p.IsConnected || !p.Connectors.Any(c => ownerNodes.Contains(c.End.Owner)))
                    );
            }

            // If this group does contain any AnnotationModels
            // we only want to get the ports where the owner does
            // not belong to a group.
            return Nodes.OfType<NodeModel>()
                .SelectMany(x => x.OutPorts
                    .Where(p => !p.IsConnected || !p.Connectors.Any(c => Nodes.Contains(c.End.Owner)))
                );
        }

        private List<PortViewModel> CreateProxyPorts(IEnumerable<PortModel> groupPortModels)
        {
            var originalPortViewModels = WorkspaceViewModel.Nodes
                .SelectMany(x => x.InPorts.Concat(x.OutPorts))
                .Where(x => groupPortModels.Contains(x.PortModel))
                .ToList();

            var newPortViewModels = new List<PortViewModel>();
            for (int i = 0; i < groupPortModels.Count(); i++)
            {
                var model = groupPortModels.ElementAt(i);
                newPortViewModels.Add(originalPortViewModels[i].CreateProxyPortViewModel(model));
            }

            return newPortViewModels;
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
            }

            if (!collapseConnectors) return;

            CollapseConnectors();
        }

        private void CollapseConnectors()
        {
            if (originalInPorts is null)
            {
                return;
            }

            var excludedPorts = originalInPorts.Concat(originalOutPorts);

            var allNodes = this.Nodes
                .OfType<AnnotationModel>()
                .SelectMany(x => x.Nodes.OfType<NodeModel>())
                .Concat(this.Nodes.OfType<NodeModel>());

            var connectorsToHide = allNodes
                .SelectMany(x => x.InPorts.Concat(x.OutPorts))
                .Except(excludedPorts)
                .SelectMany(x => x.Connectors)
                .Distinct();

            foreach (var connector in connectorsToHide)
            {
                var connectorViewModel = WorkspaceViewModel
                    .Connectors
                    .Where(x => connector.GUID == x.ConnectorModel.GUID)
                    .FirstOrDefault();

                connectorViewModel.IsCollapsed = true;
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
                    // If there is a group in this group
                    // we expand that and all of its content.
                    annotationViewModel.IsCollapsed = false;
                    annotationViewModel.ShowGroupContents();
                }

                viewModel.IsCollapsed = false;
            }

            foreach (var nodeModel in Nodes.OfType<NodeModel>())
            {
                var connectorGuids = nodeModel.AllConnectors
                    .Select(x => x.GUID);

                var connectorViewModels = WorkspaceViewModel.Connectors
                    .Where(x => connectorGuids.Contains(x.ConnectorModel.GUID))
                    .ToList();

                connectorViewModels.ForEach(x => x.IsCollapsed = false);
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
                    break;
                case nameof(AnnotationModel.Position):
                    RaisePropertyChanged(nameof(ModelAreaRect));
                    RaisePropertyChanged(nameof(AnnotationModel.Position));
                    break;

            }
        }

        private void UpdateAllGroupedGroups()
        {
            ViewModelBases
                .OfType<AnnotationViewModel>()
                .ToList()
                .ForEach(x => UpdateGroupCutGeometry(x));
        }

        private void HandleNodesCollectionChanges()
        {
            var allGroupedGroups = Nodes.OfType<AnnotationModel>();
            var removedFromGroup = GroupIdToCutGeometry.Keys
                .ToList()
                .Except(allGroupedGroups.Select(x => x.GUID.ToString()));
            foreach (var key in removedFromGroup)
            {
                RemoveKeyFromCutGeometryDictionary(key);
            }

            var addedToGroup = allGroupedGroups
                .Select(x => x.GUID.ToString())
                .Except(GroupIdToCutGeometry.Keys.ToList());

            foreach (var key in addedToGroup)
            {
                var groupViewModel = ViewModelBases.OfType<AnnotationViewModel>()
                    .Where(x => x.AnnotationModel.GUID.ToString() == key)
                    .FirstOrDefault();

                AddToCutGeometryDictionary(groupViewModel);
            }
        }

        private void RemoveKeyFromCutGeometryDictionary(string groupGuid)
        {
            if (GroupIdToCutGeometry is null)
            {
                return;
            }

            GroupIdToCutGeometry.Remove(groupGuid);
            RaisePropertyChanged(nameof(NestedGroupsGeometryCollection));

            var groupViewModel = this.WorkspaceViewModel.Annotations
                .Where(x => x.AnnotationModel.GUID.ToString() == groupGuid)
                .FirstOrDefault();

            if (groupViewModel is null) return;
            groupViewModel.PropertyChanged -= GroupViewModel_PropertyChanged;
        }

        private void AddToCutGeometryDictionary(AnnotationViewModel annotationViewModel)
        {
            var key = annotationViewModel.AnnotationModel.GUID.ToString();
            if (GroupIdToCutGeometry.ContainsKey(key)) return;

            GroupIdToCutGeometry[key] = CreateRectangleGeometry(annotationViewModel);
            annotationViewModel.PropertyChanged += GroupViewModel_PropertyChanged;
            RaisePropertyChanged(nameof(NestedGroupsGeometryCollection));
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

        private void OutPorts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (PortViewModel item in e.NewItems)
                    {
                        if (OutputNodes.Contains(item.NodeViewModel)) continue;
                        OutputNodes.Add(item.NodeViewModel);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (PortViewModel item in e.OldItems)
                    {
                        if (!OutputNodes.Contains(item.NodeViewModel)) continue;
                        OutputNodes.Remove(item.NodeViewModel);
                    }
                    break;
                default:
                    break;
            }
        }

        private void InPorts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (PortViewModel item in e.NewItems)
                    {
                        if (InputNodes.Contains(item.NodeViewModel)) continue;
                        InputNodes.Add(item.NodeViewModel);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (PortViewModel item in e.OldItems)
                    {
                        if (!InputNodes.Contains(item.NodeViewModel)) continue;
                        InputNodes.Remove(item.NodeViewModel);
                    }
                    break;
                default:
                    break;
            }
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
            var key = annotationViewModel.AnnotationModel.GUID.ToString();
            var updatedGeometry = CreateRectangleGeometry(annotationViewModel);
            GroupIdToCutGeometry[key] = updatedGeometry;
            RaisePropertyChanged(nameof(NestedGroupsGeometryCollection));
        }

        private bool BelongsToGroup()
        {
            return WorkspaceViewModel.Model.Annotations.ContainsModel(this.annotationModel);
        }

        public override void Dispose()
        {
            InPorts.CollectionChanged -= InPorts_CollectionChanged;
            OutPorts.CollectionChanged -= OutPorts_CollectionChanged;
            annotationModel.PropertyChanged -= model_PropertyChanged;
            DynamoSelection.Instance.Selection.CollectionChanged -= SelectionOnCollectionChanged;
            base.Dispose();
        }
    }
}
