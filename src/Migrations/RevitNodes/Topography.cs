using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    class TopographyFromPoints:MigrationNode
    {
        [NodeMigration(from:"0.6.3.0")]
        public static NodeMigrationData Migrate(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll", "Topography.ByPoints",
                "Revit.Elements.Topography.ByPoints@Autodesk.DesignScript.Geometry.Point[]");
        }
    }

    class PointsFromTopography : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0")]
        public static NodeMigrationData Migrate(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll", "Topography.Points",
                "Revit.Elements.Topography.Points");
        }
    }
}
