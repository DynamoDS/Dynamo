using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class DividedSurface : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"RevitNodes.dll",
                /*NXLT*/"DividedSurface.ByFaceAndUVDivisions",
                /*NXLT*/"DividedSurface.ByFaceAndUVDivisions@FaceReference,int,int");
        }
    }
}
