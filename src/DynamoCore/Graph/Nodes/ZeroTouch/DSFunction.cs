using Autodesk.DesignScript.Runtime;
using Dynamo.Engine;
using Dynamo.Migration;
using System.Linq;

namespace Dynamo.Graph.Nodes.ZeroTouch
{
    /// <summary>
    ///     DesignScript function node. All functions from DesignScript share the
    ///     same function node but internally have different procedure.
    /// </summary>
    [NodeName("Function Node"), NodeDescription("DSFunctionNodeDescription", typeof(Properties.Resources)),
    IsVisibleInDynamoLibrary(false), IsMetaNode]
    [AlsoKnownAs("Dynamo.Nodes.DSFunction")]
    public class DSFunction : DSFunctionBase
    {
        /// <summary>
        /// The NodeType property provides a name which maps to the 
        /// server type for the node. This property should only be
        /// used for serialization. 
        /// </summary>
        public override string NodeType
        {
            get
            {
                return "FunctionNode";
            }
        }

        public string FunctionSignature
        {
            get
            {
                return Controller.Definition.MangledName;
            }
        }

        /// <summary>
        ///     Indicates whether Node is input or not.
        /// </summary>
        public override bool IsInputNode
        {
            get { return false; }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DSFunction"/> class.
        /// </summary>
        /// <param name="description">Function descritor.</param>
        public DSFunction(FunctionDescriptor functionDescription) 
            : base(new ZeroTouchNodeController<FunctionDescriptor>(functionDescription)) 
        {
        }

        [NodeMigration(version: "2.0.0.0")]
        public static NodeMigrationData MigrateGetImportedObjects(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);
            var node = data.MigratedNodes.ElementAt(0);

            if (node.GetAttribute("function").Equals("Dynamo.Translation.FileLoader.GetImportedObjects"))
            {
                var nodeGuid = node.GetAttribute("guid");
                var inputConnector = data.FindFirstConnector(new PortId(nodeGuid, 0, PortType.Input));
                var outputConnector = data.FindFirstConnector(new PortId(nodeGuid, 0, PortType.Output));

                // Reconnect the input node and the output node
                inputConnector.SetAttribute("end", outputConnector.GetAttribute("end"));
                migrationData.CreateConnector(inputConnector);
            }
            else
            {
                migrationData.AppendNode(node);
            }
            return migrationData;
        }
    }
}
