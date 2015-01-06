using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class FreeForm : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"RevitNodes.dll",
                /*NXLT*/"ImportInstance.ByGeometries", /*NXLT*/"ImportInstance.ByGeometries@Autodesk.DesignScript.Geometry.Geometry[]");
        }
    }
}
