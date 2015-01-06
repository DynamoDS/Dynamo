using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    class TopographyFromPoints:MigrationNode
    {
        [NodeMigration(from:"0.6.3.0")]
        public static NodeMigrationData Migrate(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"RevitNodes.dll", /*NXLT*/"Topography.ByPoints",
                /*NXLT*/"Revit.Elements.Topography.ByPoints@Autodesk.DesignScript.Geometry.Point[]");
        }
    }

    class PointsFromTopography : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0")]
        public static NodeMigrationData Migrate(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"RevitNodes.dll", /*NXLT*/"Topography.Points",
                /*NXLT*/"Revit.Elements.Topography.Points");
        }
    }
}
