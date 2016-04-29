using Dynamo.Graph.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.ViewModels
{
    public class CustomNodeAnnotationViewModel : AnnotationViewModel
    {
        public CustomNodeAnnotationViewModel(WorkspaceViewModel workspaceViewModel, AnnotationModel model)
            :base(workspaceViewModel, model)
        {
        }
    }
}
