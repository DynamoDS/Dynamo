using Dynamo.Graph.Annotations;
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
            :base(workspaceViewModel, model)
        {
            annotationModel = model;
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
    }
}
