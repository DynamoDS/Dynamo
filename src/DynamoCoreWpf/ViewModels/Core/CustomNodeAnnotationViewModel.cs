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


        private DelegateCommand _collapseBackToCustomNodeInstanceCommand;
        public DelegateCommand CollapseBackToCustomNodeInstanceCommand
        {
            get
            {
                if (_collapseBackToCustomNodeInstanceCommand == null)
                    _collapseBackToCustomNodeInstanceCommand =
                        new DelegateCommand(CollapseBackToCustomNodeInstance, CanCollapseBackToCustomNodeInstance);

                return _collapseBackToCustomNodeInstanceCommand;
            }
        }

        private bool CanCollapseBackToCustomNodeInstance(object obj)
        {
            return annotationModel.IsSelected;
        }

        private void CollapseBackToCustomNodeInstance(object obj)
        {
            if (annotationModel.IsSelected)
            {
                this.WorkspaceViewModel.DynamoViewModel.CollapseBackToCustomNodeInstance(annotationModel);
            }
        }
    }
}
