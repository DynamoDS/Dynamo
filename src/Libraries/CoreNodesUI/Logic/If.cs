using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Models;
using Dynamo.Nodes;
using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesUI.Logic
{
    [NodeName("If")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_CONDITIONAL)]
    [NodeDescription("Conditional statement")]
    [IsDesignScriptCompatible]
    public class If : NodeModel
    {
        public If()
        {
            InPortData.Add(new PortData("test", "Test block"));
            InPortData.Add(new PortData("true", "True block"));
            InPortData.Add(new PortData("false", "False block"));

            OutPortData.Add(new PortData("result", "Result"));

            RegisterAllPorts();

            //TODO: Default Values
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            return new[]
            {
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    AstFactory.BuildConditionalNode(
                        inputAstNodes[0],
                        inputAstNodes[1],
                        inputAstNodes[2]))
            };
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            System.Xml.XmlElement xmlNode = data.MigratedNodes.ElementAt(0);
            var element = MigrationManager.CloneAndChangeType(xmlNode, "DSCoreNodesUI.Logic.If");

            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(element);
            return migrationData;
        }
    }
}
