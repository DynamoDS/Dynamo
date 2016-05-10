using Dynamo.Graph.Annotations;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Selection;
using Dynamo.UI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.ViewModels
{
    public class CustomNodeAnnotationViewModel : AnnotationViewModel
    {
        private CustomNodeAnnotationModel annotationModel;

        public CustomNodeAnnotationViewModel(WorkspaceViewModel workspaceViewModel, CustomNodeAnnotationModel model)
            : base(workspaceViewModel, model)
        {
            annotationModel = model;
            annotationModel.PropertyChanged += OnPropertyChanged;
        }


        private DelegateCommand _updateCustomNodeDefinitionCommand;
        public DelegateCommand UpdateCustomNodeDefinitionCommand
        {
            get
            {
                if (_updateCustomNodeDefinitionCommand == null)
                    _updateCustomNodeDefinitionCommand =
                        new DelegateCommand(UpdateCustomNodeDefintion, CanUpdateCustomNodeDefinition);

                return _updateCustomNodeDefinitionCommand;
            }
        }

        private bool CanUpdateCustomNodeDefinition(object obj)
        {
            return annotationModel.IsSelected;
        }

        private void UpdateCustomNodeDefintion(object obj)
        {
            if (annotationModel.IsSelected)
            {
                this.WorkspaceViewModel.DynamoViewModel.UpdateCustomNodeDefinition(annotationModel);
            }
        }

        private DelegateCommand _restoreCustomNodeInstanceCommand;
        public DelegateCommand RestoreCustomNodeInstanceCommand
        {
            get
            {
                if (_restoreCustomNodeInstanceCommand == null)
                    _restoreCustomNodeInstanceCommand =
                        new DelegateCommand(RestoreCustomNodeInstance, CanRestoreCustomNodeInstance);

                return _restoreCustomNodeInstanceCommand;
            }
        }

        private DelegateCommand _syncWithCustomNodeDefinitionCommand;
        public DelegateCommand SyncWithCustomNodeDefinitionCommand
        {
            get
            {
                if (_syncWithCustomNodeDefinitionCommand == null)
                    _syncWithCustomNodeDefinitionCommand =
                        new DelegateCommand(SyncWithCustomNodeDefinition, CanRestoreCustomNodeInstance);

                return _syncWithCustomNodeDefinitionCommand;
            }
        }

        private DelegateCommand _editCustomNodeCommand;
        public DelegateCommand EditCustomNodeCommand
        {
            get
            {
                if (_editCustomNodeCommand == null)
                {
                    _editCustomNodeCommand = new DelegateCommand(EditCustomNode, CanEditCustomNode);
                }

                return _editCustomNodeCommand;
            }
        }

        private bool CanRestoreCustomNodeInstance(object obj)
        {
            return annotationModel.IsSelected;
        }

        private void RestoreCustomNodeInstance(object obj)
        {
            if (annotationModel.IsSelected)
            {
                this.WorkspaceViewModel.DynamoViewModel.RestoreCustomNodeInstance(annotationModel);
            }
        }

        private bool CanSyncWithCustomNodeDefinition(object obj)
        {
            return annotationModel.IsSelected;
        }

        private void SyncWithCustomNodeDefinition(object obj)
        {
            if (annotationModel.IsSelected)
            {
                this.WorkspaceViewModel.DynamoViewModel.SyncWithCustomNodeDefinition(annotationModel);
            }
        }

        private bool CanEditCustomNode(object obj)
        {
            return this.annotationModel.FunctionID != Guid.Empty;
        }

        private void EditCustomNode(object obj)
        {
            if (annotationModel.IsSelected)
            {
                WorkspaceViewModel.DynamoViewModel.GoToWorkspace(this.annotationModel.FunctionID);
            }
        }

        private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Background":
                    RegenerateRenderPackages();
                    break;
            }
        }

        private void RegenerateRenderPackages()
        {
            var backGroundPreivew = WorkspaceViewModel.DynamoViewModel.Watch3DViewModels.FirstOrDefault();
            var isInCustomNode = WorkspaceViewModel.DynamoViewModel.Model.CurrentWorkspace is CustomNodeWorkspaceModel;
            if (!isInCustomNode && backGroundPreivew != null)
            {
                var nodes = SelectedModels.OfType<NodeModel>();
                backGroundPreivew.RegeneratePackagesForNode(nodes);
            }
        }
    }
}
