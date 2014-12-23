using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class Plane : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Plane.ByOriginNormal", /*NXLT*/"Plane.ByOriginNormal@Point,Vector");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            //append asVector Node
            XmlElement pointAsVector0 = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Point.AsVector", /*NXLT*/"Point.AsVector");
            migrationData.AppendNode(pointAsVector0);
            string pointAsVector0Id = MigrationManager.GetGuidFromXmlElement(pointAsVector0);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.INPUT);
            PortId pToV = new PortId(pointAsVector0Id, 0, PortType.INPUT);
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction nodes
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"CoordinateSystem.XYPlane", /*NXLT*/"CoordinateSystem.XYPlane");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            var csNode = MigrationManager.CreateFunctionNode(data.Document, oldNode, 0,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"CoordinateSystem.Identity", /*NXLT*/"CoordinateSystem.Identity");
            migrationData.AppendNode(csNode);
            string csNodeId = MigrationManager.GetGuidFromXmlElement(csNode);

            // Create connector
            data.CreateConnector(csNode, 0, newNode, 0);

            return migrationData;
        }
    }

    public class XzPlane : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction nodes
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"CoordinateSystem.ZXPlane", /*NXLT*/"CoordinateSystem.ZXPlane");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            var csNode = MigrationManager.CreateFunctionNode(data.Document, oldNode, 0,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"CoordinateSystem.Identity", /*NXLT*/"CoordinateSystem.Identity");
            migrationData.AppendNode(csNode);
            string csNodeId = MigrationManager.GetGuidFromXmlElement(csNode);

            // Create connector
            data.CreateConnector(csNode, 0, newNode, 0);

            return migrationData;
        }
    }

    public class YzPlane : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction nodes
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"CoordinateSystem.YZPlane", /*NXLT*/"CoordinateSystem.YZPlane");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            var csNode = MigrationManager.CreateFunctionNode(data.Document, oldNode, 0,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"CoordinateSystem.Identity", /*NXLT*/"CoordinateSystem.Identity");
            migrationData.AppendNode(csNode);
            string csNodeId = MigrationManager.GetGuidFromXmlElement(csNode);

            // Create connector
            data.CreateConnector(csNode, 0, newNode, 0);

            return migrationData;
        }
    }

    public class SketchPlane : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"RevitNodes.dll",
                /*NXLT*/"SketchPlane.ByPlane", /*NXLT*/"SketchPlane.ByPlane@Plane");
        }
    }

    internal class BestFitPlane : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            if ((data.FindFirstConnector(new PortId(oldNodeId, 1, PortType.OUTPUT)) != null) ||
                (data.FindFirstConnector(new PortId(oldNodeId, 2, PortType.OUTPUT)) != null))
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
                MigrationManager.SetFunctionSignature(newNode,/*NXLT*/"ProtoGeometry.dll",
                    /*NXLT*/"Plane.ByBestFitThroughPoints", /*NXLT*/"Plane.ByBestFitThroughPoints");
                migrationData.AppendNode(newNode);
            }

            return migrationData;
        }
    }

    public class PlaneFromReferencePlane : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"RevitNodes.dll",
                /*NXLT*/"ReferencePlane.Plane", /*NXLT*/"ReferencePlane.Plane");
        }
    }
}
