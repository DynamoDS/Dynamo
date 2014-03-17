using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class Level : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll", "Level.ByElevationAndName",
                "Level.ByElevationAndName@double,name");
        }
    }

    public class SelectLevel : MigrationNode
    {
    }
}
