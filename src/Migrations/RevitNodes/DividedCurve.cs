using System.Linq;
using System.Xml;
using Dynamo.Models;
using Dynamo.Migration;

namespace Dynamo.Nodes
{

    public class DividedPathSpacingRuleLayout : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(
                MigrationManager.CloneAndChangeName(
                    data.MigratedNodes.ElementAt(0),
                    "DSRevitNodesUI.SpacingRuleLayouts",
                    "Spacing Rule Layout"));
            return migrationData;
        }
    }

    public class DividedPath : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "DividedPath.ByCurveAndDivisions", 
                "DividedPath.ByCurveAndDivisions@CurveReference,int");
        }
    }

    public class PointsOnDividedPath : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(
                data,
                "RevitNodes.dll",
                "DividedPath.Points",
                "DividedPath.Points@DividedPath");
        }
    }

}
