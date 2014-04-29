using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class VoronoiOnFace : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(
                data,
                "Tessellation.dll",
                "Voronoi.ByParametersOnSurface",
                "Voronoi.ByParametersOnSurface@IEnumerable<UV>,Surface");
        }
    }

    public class DelaunayOnFace : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(
                data,
                "Tessellation.dll",
                "Delaunay.ByParametersOnSurface",
                "Delaunay.ByParametersOnSurface@IEnumerable<UV>,Surface");
        }
    }

    public class ConvexHull3D : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(
                data,
                "Tessellation.dll",
                "ConvexHull.ByPoints",
                "ConvexHull.ByPoints@IEnumerable<Point>");
        }
    }

    public class Delaunay3D : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(
                data,
                "Tessellation.dll",
                "Delaunay.ByPoints",
                "Delaunay.ByPoints@IEnumerable<Point>");
        }
    }
}
