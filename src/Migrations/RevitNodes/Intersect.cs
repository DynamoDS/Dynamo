using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class CurveFaceIntersection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "Face.Intersect", "Face.Intersect@Curve");
        }
    }

    public class CurveCurveIntersection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll",
                "Geometry.Intersect", "Geometry.Intersect@Geometry");
        }
    }

    public class FaceFaceIntersection : MigrationNode
    {
    }

    public class CurvePlaneIntersection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll",
                "Geometry.Intersect", "Geometry.Intersect@Geometry");
        }
    }
}
