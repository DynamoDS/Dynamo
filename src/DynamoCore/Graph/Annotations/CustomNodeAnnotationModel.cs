using Dynamo.Graph.Nodes;
using Dynamo.Graph.Notes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Graph.Annotations
{
    public class CustomNodeAnnotationModel: AnnotationModel
    {
        public CustomNodeAnnotationModel(IEnumerable<NodeModel> nodes, IEnumerable<NoteModel> notes):
            base(nodes, notes)
        {
        }
    }
}
