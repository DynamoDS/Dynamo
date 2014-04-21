using System.Linq;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class FloorByOutlineLevelAndOffset : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "Floor.ByOutlineTypeAndLevel", "Floor.ByOutlineTypeAndLevel@Curve[],FloorType,Level");
        }
    }

    public class SelectFloorType : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(MigrationManager.CloneAndChangeName(
                data.MigratedNodes.ElementAt(0), "DSRevitNodesUI.FloorTypes", "Floor Types"));

            return migrationData;
        }
    }
}
