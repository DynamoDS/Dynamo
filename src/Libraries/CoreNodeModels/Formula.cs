using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.Engine;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Migration;
using Newtonsoft.Json;
using ProtoCore;
using ProtoCore.AST.AssociativeAST;

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
            try
            {
                var formulaConverter = new MigrateFormulaToDS();
                convertedCode = formulaConverter.ConvertFormulaToDS(formula);
            }
            catch (BuildHaltException)
            {
                newNode.Attributes["CodeText"].Value = formula;
                //(node as CodeBlockNodeModel).FormulaMigrationWarning(Resources.FormulaDSConversionFailure);
                conversionFailed = true;
            }
            if (!conversionFailed)
            {
                newNode.Attributes["CodeText"].Value = convertedCode;
                //(node as CodeBlockNodeModel).FormulaMigrationWarning(Resources.FormulaMigrated);
            }
            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }
}
