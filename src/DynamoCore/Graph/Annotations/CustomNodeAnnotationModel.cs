using Dynamo.Graph.Nodes;
using Dynamo.Graph.Notes;
using Dynamo.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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
        /// <param name="functionID"></param>
        /// <param name="nodes"></param>
        public CustomNodeAnnotationModel(Guid functionID, IEnumerable<NodeModel> nodes, IEnumerable<NoteModel> notes):
            base(nodes, notes)
        {
            FunctionID = functionID; 
        }
        
        /// <summary>
        /// Function ID of custom node definition
        /// </summary>
        public Guid FunctionID 
        {
            get; private set;
        }

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            XmlElementHelper helper = new XmlElementHelper(element);
            helper.SetAttribute("functionID", this.FunctionID);
            base.SerializeCore(element, context);
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            XmlElementHelper helper = new XmlElementHelper(element);
            base.DeserializeCore(element, context);
        }
    }
}
