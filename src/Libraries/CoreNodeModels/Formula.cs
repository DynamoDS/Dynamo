using System.Linq;
using Dynamo.Engine;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.NodeLoaders;
using Dynamo.Migration;
using ProtoCore;

namespace CoreNodeModels
{
    [NodeName("Formula")]
    [NodeDescription("FormulaDescription", typeof(Properties.Resources))]
    [NodeCategory(BuiltinNodeCategories.CORE_SCRIPTING)]
    [IsDesignScriptCompatible]
    [OutPortTypes("Function")]
    [AlsoKnownAs("DSCoreNodesUI.Formula")]
    public class Formula : NodeModel
    {
        [NodeMigration(version: "3.0.0.0")]
        public static NodeMigrationData MigrateToCodeBlockNode(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);
            var node = data.MigratedNodes.ElementAt(0);

            var child = node.FirstChild;
            if (child == null)
            {
                return migrationData;
            }
            var formula = child.InnerText;
            string convertedCode = string.Empty;
            bool conversionFailed = false;

            var newNode = MigrationManager.CreateCodeBlockNodeFrom(node);
            newNode.SetAttribute("guid", node.Attributes["guid"].Value);
            try
            {
                var formulaConverter = new MigrateFormulaToDS();
                convertedCode = formulaConverter.ConvertFormulaToDS(formula);
            }
            catch (BuildHaltException)
            {
                newNode.Attributes["CodeText"].Value = formula;

                conversionFailed = true;
            }
            if (!conversionFailed)
            {
                newNode.Attributes["CodeText"].Value = convertedCode;
            }
            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }
}
