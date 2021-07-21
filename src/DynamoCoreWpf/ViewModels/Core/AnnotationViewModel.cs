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
        public readonly WorkspaceViewModel WorkspaceViewModel;        
        
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
                var zIndex = 1;
                if (annotationModel.IsNested)
                {
                    return zIndex + 1;
                }

                return zIndex; 
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
            return DynamoSelection.Instance.Selection.Count >= 0 && !this.AnnotationModel.IsNested;
        }

        private void AddGroupToGroup(object obj)
        {
            if (annotationModel.IsSelected)
            {
                var selectedModels = DynamoSelection.Instance.Selection
                    .OfType<AnnotationModel>()
                    .Where(x=>x.GUID != this.AnnotationModel.GUID && !x.IsNested);

                foreach (var model in selectedModels)
                {
                    this.AnnotationModel.AddToSelectedModels(model, false);
                    model.IsNested = true;
                    
                }
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

        public IEnumerable<ViewModelBase> ViewModelBases
        {
            get;set;
        }

        private ObservableCollection<PortViewModel> inPorts;
        
        /// <summary>
        /// 
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
        /// 
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

        private bool isLocked;
        public bool IsLocked
        {
            get => isLocked;
            set
            {
                isLocked = value;
                RaisePropertyChanged(nameof(IsLocked));
            }
        }

        private bool isExpanded;
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                if (isExpanded == value) return;
                
                isExpanded = value;
                if (isExpanded)
                {
                    this.ShowGroupNodes();
                }
                else
                {
                    this.CollapseGroupNodes();
                    this.SetGroupInputPorts();
                    this.SetGroupOutPorts();
                }
                RaisePropertyChanged(nameof(IsExpanded));
            }
        }

        private bool nodeHoveringState;
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

            ViewModelBases = this.WorkspaceViewModel.GetViewModelsInternal(annotationModel.Nodes.Select(x => x.GUID));
            IsExpanded = true;
        }

        internal void SetGroupInputPorts()
        {
            InPorts.Clear();

            var groupPortModels = Nodes.OfType<NodeModel>()
                .SelectMany(x => x.InPorts
                    .Where(p => !p.IsConnected || !p.Connectors.Any(c => Nodes.Contains(c.Start.Owner)))
                );

            var originalPortViewModels = WorkspaceViewModel.Nodes
                .SelectMany(x => x.InPorts)
                .Where(x => groupPortModels.Contains(x.PortModel));

            var newPortViewModels = originalPortViewModels.Select(x => x.CreateProxyPortViewModel());

            if (newPortViewModels == null) return;
            InPorts.AddRange(newPortViewModels);
        }

        internal void SetGroupOutPorts()
        {
            OutPorts.Clear();

            var groupPortModels = Nodes.OfType<NodeModel>()
                .SelectMany(x => x.OutPorts
                    .Where(p => !p.IsConnected || !p.Connectors.Any(c => Nodes.Contains(c.End.Owner)))
                );

            var portViewModels = WorkspaceViewModel.Nodes
                .SelectMany(x => x.OutPorts)
                .Where(x => groupPortModels.Contains(x.PortModel));

            if (portViewModels == null) return;
            OutPorts.AddRange(portViewModels);
        }


        internal void ClearSelection()
        {
            // Group is created already.So just populate it.
            var selectNothing = new DynamoModel.SelectModelCommand(Guid.Empty, System.Windows.Input.ModifierKeys.None.AsDynamoType());
            WorkspaceViewModel.DynamoViewModel.ExecuteCommand(selectNothing);
        }

        internal void CollapseGroupNodes()
        {
            foreach (var viewModel in ViewModelBases)
            {
                if (viewModel is AnnotationViewModel annotationViewModel)
                {
                    annotationViewModel.IsCollapsed = true;
                    annotationViewModel.CollapseGroupNodes();
                }
                viewModel.IsCollapsed = true;
            }
        }

        internal void ShowGroupNodes()
        {
            if (!IsExpanded)
            {
                return;
            }

            foreach (var viewModel in ViewModelBases)
            {
                if (viewModel is AnnotationViewModel annotationViewModel)
                {
                    annotationViewModel.IsCollapsed = false;
                    annotationViewModel.ShowGroupNodes();
                }

                viewModel.IsCollapsed = false;
            }
        }

        private bool CanChangeFontSize(object obj)
        {
            return true;
        }

        private void UpdateFontSize(object parameter)
        {
            if (parameter == null) return;

            WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
                new DynamoModel.UpdateModelValueCommand(
                    Guid.Empty, AnnotationModel.GUID, "FontSize", parameter.ToString()));

            WorkspaceViewModel.DynamoViewModel.RaiseCanExecuteUndoRedo();
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
                    break;
                case "Height":
                    RaisePropertyChanged("Height");
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
                case nameof(AnnotationModel.IsNested):
                    RaisePropertyChanged(nameof(ZIndex));
                    AddToGroupCommand.RaiseCanExecuteChanged();
                    break;
                case nameof(AnnotationModel.Nodes):
                    ViewModelBases = this.WorkspaceViewModel.GetViewModelsInternal(annotationModel.Nodes.Select(x => x.GUID));
                    break;
                case nameof(AnnotationModel.ModelAreaHeight):
                    RaisePropertyChanged(nameof(ModelAreaHeight));
                    break;
            }
        }

        /// <summary>
        /// Selects this group and models within it.
        /// </summary>
        internal void Select()
        {
            var annotationGuid = this.AnnotationModel.GUID;
            this.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
                new DynamoModel.SelectModelCommand(annotationGuid, Keyboard.Modifiers.AsDynamoType()));

            //Select all the models inside the group - This avoids some performance bottleneck 
            //with many nodes selected at the same time - which makes moving the group very slow

            var groupedGroupsNodes = this.AnnotationModel.Nodes.OfType<AnnotationModel>().SelectMany(x => x.Nodes);
            DynamoSelection.Instance.Selection.AddRange(this.AnnotationModel.Nodes.Concat(groupedGroupsNodes));
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
        }
    }
}
