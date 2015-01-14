using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class Plane : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Plane.ByOriginNormal", "Plane.ByOriginNormal@Point,Vector");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            //append asVector Node
            XmlElement pointAsVector0 = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1, "ProtoGeometry.dll",
                "Point.AsVector", "Point.AsVector");
            migrationData.AppendNode(pointAsVector0);
            string pointAsVector0Id = MigrationManager.GetGuidFromXmlElement(pointAsVector0);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.Input);
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.Input);
            PortId pToV = new PortId(pointAsVector0Id, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);
            data.ReconnectToPort(connector0, pToV);
            data.ReconnectToPort(connector1, oldInPort0);
            data.CreateConnector(pointAsVector0, 0, newNode, 1);

            return migrationData;
        }
    }
    public class XyPlane : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction nodes
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "CoordinateSystem.XYPlane", "CoordinateSystem.XYPlane");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            var csNode = MigrationManager.CreateFunctionNode(data.Document, oldNode, 0, "ProtoGeometry.dll",
                "CoordinateSystem.Identity", "CoordinateSystem.Identity");
            migrationData.AppendNode(csNode);
            string csNodeId = MigrationManager.GetGuidFromXmlElement(csNode);

            // Create connector
            data.CreateConnector(csNode, 0, newNode, 0);

            return migrationData;
        }
    }

    public class XzPlane : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction nodes
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "CoordinateSystem.ZXPlane", "CoordinateSystem.ZXPlane");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            var csNode = MigrationManager.CreateFunctionNode(data.Document, oldNode, 0, "ProtoGeometry.dll",
                "CoordinateSystem.Identity", "CoordinateSystem.Identity");
            migrationData.AppendNode(csNode);
            string csNodeId = MigrationManager.GetGuidFromXmlElement(csNode);

            // Create connector
            data.CreateConnector(csNode, 0, newNode, 0);

            return migrationData;
        }
    }

    public class YzPlane : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction nodes
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "CoordinateSystem.YZPlane", "CoordinateSystem.YZPlane");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            var csNode = MigrationManager.CreateFunctionNode(data.Document, oldNode, 0, "ProtoGeometry.dll",
                "CoordinateSystem.Identity", "CoordinateSystem.Identity");
            migrationData.AppendNode(csNode);
            string csNodeId = MigrationManager.GetGuidFromXmlElement(csNode);

            // Create connector
            data.CreateConnector(csNode, 0, newNode, 0);

            return migrationData;
        }
    }

    public class SketchPlane : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "SketchPlane.ByPlane", "SketchPlane.ByPlane@Plane");
        }
    }

    internal class BestFitPlane : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            if ((data.FindFirstConnector(new PortId(oldNodeId, 1, PortType.Output)) != null) ||
                (data.FindFirstConnector(new PortId(oldNodeId, 2, PortType.Output)) != null))
            {
                // If the second or third output port is utilized, migrate to CBN
                XmlElement codeBlockNode = MigrationManager.CreateCodeBlockNodeFrom(oldNode);
                codeBlockNode.SetAttribute("CodeText",
                    "p = Plane.ByBestFitThroughPoints(XYZs);\n" +
                    "p.Normal.AsPoint();\n" +
                    "p.Origin;");
                codeBlockNode.SetAttribute("nickname", "Best Fit Plane");
                migrationData.AppendNode(codeBlockNode);
            }
            else
            {
                // When only the first output port is utilized, migrate directly
                var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
                MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                    "Plane.ByBestFitThroughPoints", "Plane.ByBestFitThroughPoints");
                migrationData.AppendNode(newNode);
            }

            return migrationData;
        }
    }

    public class PlaneFromReferencePlane : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "ReferencePlane.Plane", "ReferencePlane.Plane");
        }
    }
}
