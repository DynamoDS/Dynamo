using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class VoronoiOnFace : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, /*NXLT*/"Tessellation.dll",
                /*NXLT*/"Voronoi.ByParametersOnSurface",
                /*NXLT*/"Voronoi.ByParametersOnSurface@IEnumerable<UV>,Surface");
            newNode.SetAttribute("lacing", "Shortest");
            migrationData.AppendNode(newNode);

            return migrationData;
        }
    }

    public class DelaunayOnFace : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, /*NXLT*/"Tessellation.dll",
                /*NXLT*/"Delaunay.ByParametersOnSurface",
                /*NXLT*/"Delaunay.ByParametersOnSurface@IEnumerable<UV>,Surface");
            newNode.SetAttribute("lacing", "Shortest");
            migrationData.AppendNode(newNode);

            return migrationData;
        }
    }

    public class ConvexHull3D : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, /*NXLT*/"Tessellation.dll",
                /*NXLT*/"ConvexHull.ByPoints",
                /*NXLT*/"ConvexHull.ByPoints@IEnumerable<Point>");
            newNode.SetAttribute("lacing", "Shortest");
            migrationData.AppendNode(newNode);

            return migrationData;
        }
    }

    public class Delaunay3D : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, /*NXLT*/"Tessellation.dll",
                /*NXLT*/"Delaunay.ByPoints",
                /*NXLT*/"Delaunay.ByPoints@IEnumerable<Point>");
            newNode.SetAttribute("lacing", "Shortest");
            migrationData.AppendNode(newNode);

            return migrationData;
        }
    }
}
