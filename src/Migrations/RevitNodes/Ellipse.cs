using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class Ellipse : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Ellipse.ByOriginRadii",
                "Ellipse.ByOriginRadii@Point,double,double");
        }
    }

    public class EllipticalArc : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "EllipseArc.ByPlaneRadiiStartAngleSweepAngle",
                "EllipseArc.ByPlaneRadiiStartAngleSweepAngle@Plane,double,double,double,double");
        }
    }
}
