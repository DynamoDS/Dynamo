using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class Rectangle : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"ProtoGeometry.dll", /*NXLT*/"Rectangle.ByWidthHeight",
                /*NXLT*/"Rectangle.ByWidthHeight@Autodesk.DesignScript.Geometry.CoordinateSystem,double,double");
        }
    }
}
