using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    class RayBounce : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "RayBounce.ByOriginDirection",
                "Revit.References.RayBounce.ByOriginDirection@Autodesk.DesignScript.Geometry.Point,Autodesk.DesignScript.Geometry.Vector,int,Revit.Elements.Views.View3D");
        }
    }
}
