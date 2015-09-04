using System.Linq;
using System.Xml;
using Dynamo.Models;
using Dynamo.Migration;

namespace Dynamo.Nodes
{
    public class VoronoiOnFace : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "Tessellation.dll",
                "Voronoi.ByParametersOnSurface",
                "Voronoi.ByParametersOnSurface@IEnumerable<UV>,Surface");
            newNode.SetAttribute("lacing", "Shortest");
            migrationData.AppendNode(newNode);

            return migrationData;
        }
    }

    public class DelaunayOnFace : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "Tessellation.dll",
                "Delaunay.ByParametersOnSurface",
                "Delaunay.ByParametersOnSurface@IEnumerable<UV>,Surface");
            newNode.SetAttribute("lacing", "Shortest");
            migrationData.AppendNode(newNode);

            return migrationData;
        }
    }

    public class ConvexHull3D : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "Tessellation.dll",
                "ConvexHull.ByPoints",
                "ConvexHull.ByPoints@IEnumerable<Point>");
            newNode.SetAttribute("lacing", "Shortest");
            migrationData.AppendNode(newNode);

            return migrationData;
        }
    }

    public class Delaunay3D : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "Tessellation.dll",
                "Delaunay.ByPoints",
                "Delaunay.ByPoints@IEnumerable<Point>");
            newNode.SetAttribute("lacing", "Shortest");
            migrationData.AppendNode(newNode);

            return migrationData;
        }
    }
}
