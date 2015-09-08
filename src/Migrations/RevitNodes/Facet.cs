using Dynamo.Models;
using Dynamo.Migration;
using System.Linq;
using System.Xml;

namespace Dynamo.Nodes
{
    public class FacetByThreePoints : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(
                data,
                "ProtoGeometry.dll",
                "Surface.ByPerimeterPoints",
                "Surface.ByPerimeterPoints@Point[]");
        }
    }

    public class QuadByFourPoints : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return FacetByThreePoints.Migrate_0630_to_0700(data);
        }
    }
}
