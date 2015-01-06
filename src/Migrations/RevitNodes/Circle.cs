using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class Circle : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"ProtoGeometry.dll", "Circle.ByCenterPointRadius",
                "Circle.ByCenterPointRadius@Point,double");
        }
    }
}
