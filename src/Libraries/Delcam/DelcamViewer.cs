using System;
using System.Collections.Generic;

using Autodesk.DesignScript.Geometry;

using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Properties;
using Dynamo.Utilities;


using ProtoCore.AST.AssociativeAST;
using System.Xml;
using System.Globalization;

namespace DynamoDelcam
{
    [NodeCategory(BuiltinNodeCategories.GEOMETRY)]
    [NodeName("Delcam Viewer")]
    [NodeSearchTags("CAM", "Delcam", "Robot", "Mill")]
    [IsDesignScriptCompatible]
    public class DelcamViewer : NodeModel
    {
        public DelcamViewer()
        {
            AssociativeNode geometryNode = new ArrayNode();
            InPortData.Add(new PortData("geometry", "Geometry to mill out", geometryNode));
            OutPortData.Add(new PortData("string", "Outputting something"));

            ShouldDisplayPreviewCore = true;
            RegisterAllPorts();
        }

        public static string CreateDelcamToolPath(IEnumerable<Geometry> geometry)
        {
            return null;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            var geometryListNode = inputAstNodes[0];

            AssociativeNode node = null;

            node = AstFactory.BuildFunctionCall(
                        new Func<IEnumerable<Geometry>, string>(CreateDelcamToolPath),
                        new List<AssociativeNode> { geometryListNode });

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); // Base implementation must be called.

            var helper = new XmlElementHelper(element);
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called.
            var helper = new XmlElementHelper(element);
        }

        #endregion
    }
}

