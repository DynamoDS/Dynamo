using System.Linq;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class FamilyTypeSelector : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(MigrationManager.CloneAndChangeName(
                data.MigratedNodes.ElementAt(0), "DSRevitNodesUI.FamilyTypes", "Family Types"));

            return migrationData;
        }
    }

    public class FamilyTypeParameterSetter : MigrationNode
    {
    }

    public class FamilyTypeParameterGetter : MigrationNode
    {
    }

    public class GetFamilyInstancesByType : MigrationNode
    {

    }
}
