using Dynamo.Graph.Nodes;
using Dynamo.Graph.Notes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Graph.Annotations
{
    /// <summary>
    /// Annotation for expanded custom node.
    /// </summary>
    public class CustomNodeAnnotationModel: AnnotationModel
    {
        /// <summary>
        /// Construct CustomNodeAnnotationModel with custom node definition and nodes
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="nodes"></param>
        public CustomNodeAnnotationModel(CustomNodeDefinition definition, IEnumerable<NodeModel> nodes):
            base(nodes, Enumerable.Empty<NoteModel>())
        {
            Definition = definition;
        }
        
        /// <summary>
        /// Custom node definition
        /// </summary>
        public CustomNodeDefinition Definition
        {
            get;set;
        }
    }
}
