﻿using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class FacesByLine : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            /*
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "PolySurface.LocateSurfacesByLine", "PolySurface.LocateSurfacesByLine@Line");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new nodes
            XmlElement polySurface = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "ProtoGeometry.dll",
                "PolySurface.BySolid", "PolySurface.BySolid@Geometry.Solid");
            migrationData.AppendNode(polySurface);
            string polySurfaceId = MigrationManager.GetGuidFromXmlElement(polySurface);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            PortId polySurfaceInPort0 = new PortId(polySurfaceId, 0, PortType.INPUT);

            data.ReconnectToPort(connector0, polySurfaceInPort0);
            data.CreateConnector(polySurface, 0, newNode, 0);

            return migrationData;*/

            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement dummyNode = MigrationManager.CreateDummyNode(oldNode, 2, 1);
            migrationData.AppendNode(dummyNode);

            return migrationData;
        }
    }

    public class FaceThroughPoints : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement dummyNode = MigrationManager.CreateDummyNode(oldNode, 2, 1);
            migrationData.AppendNode(dummyNode);

            return migrationData;
        }
    }

    public class ComputeFaceDerivatives : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);
            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Surface.CoordinateSystemAtParameter", "Surface.CoordinateSystemAtParameter@double,double");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Update connectors
            PortId facePort = new PortId(newNodeId, 0, PortType.Input);
            PortId uvPort = new PortId(newNodeId, 1, PortType.Input);
            PortId newInPort0 = new PortId(newNodeId, 0, PortType.Input);

            XmlElement uvPortConnector = data.FindFirstConnector(uvPort);
            XmlElement facePortConnector = data.FindFirstConnector(facePort);

            data.ReconnectToPort(facePortConnector, newInPort0);

            if (uvPortConnector != null)
            {
                // Create new nodes only when the old node is connected to a UV node
                XmlElement nodeU = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "ProtoGeometry.dll", "UV.U", "UV.U");
                migrationData.AppendNode(nodeU);
                string nodeUId = MigrationManager.GetGuidFromXmlElement(nodeU);

                XmlElement nodeV = MigrationManager.CreateFunctionNode(
                    data.Document, oldNode, 1, "ProtoGeometry.dll", "UV.V", "UV.V");
                migrationData.AppendNode(nodeV);
                string nodeVId = MigrationManager.GetGuidFromXmlElement(nodeV);

                // Update connectors
                PortId newInPortNodeU = new PortId(nodeUId, 0, PortType.Input);

                string nodeUVId = uvPortConnector.GetAttribute("start").ToString();
                data.ReconnectToPort(uvPortConnector, newInPortNodeU);
                data.CreateConnector(nodeU, 0, newNode, 1);
                data.CreateConnector(nodeV, 0, newNode, 2);
                data.CreateConnectorFromId(nodeUVId, 0, nodeVId, 0);
            }

            return migrationData;
        }

    }

    class XyzEvaluate : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Surface.PointAtParameter", "Surface.PointAtParameter@double,double");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.Input);
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.Input);
            PortId newInPort0 = new PortId(newNodeId, 0, PortType.Input);
            
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            data.ReconnectToPort(connector1, newInPort0);

            if (connector0 != null)
            {
                // Create new nodes only when the old node is connected to a UV node
                XmlElement nodeU = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "ProtoGeometry.dll", "UV.U", "UV.U");
                migrationData.AppendNode(nodeU);
                string nodeUId = MigrationManager.GetGuidFromXmlElement(nodeU);

                XmlElement nodeV = MigrationManager.CreateFunctionNode(
                    data.Document, oldNode, 1, "ProtoGeometry.dll", "UV.V", "UV.V");
                migrationData.AppendNode(nodeV);
                string nodeVId = MigrationManager.GetGuidFromXmlElement(nodeV);

                // Update connectors
                PortId newInPortNodeU = new PortId(nodeUId, 0, PortType.Input);

                string nodeUVId = connector0.GetAttribute("start").ToString();
                data.ReconnectToPort(connector0, newInPortNodeU);
                data.CreateConnector(nodeU, 0, newNode, 1);
                data.CreateConnector(nodeV, 0, newNode, 2);
                data.CreateConnectorFromId(nodeUVId, 0, nodeVId, 0);
            }
            
            return migrationData;
        }
    }

    class NormalEvaluate : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Surface.NormalAtParameter", "Surface.NormalAtParameter@double,double");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.Input);
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.Input);
            PortId newInPort0 = new PortId(newNodeId, 0, PortType.Input);

            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            data.ReconnectToPort(connector1, newInPort0);

            if (connector0 != null)
            {
                // Create new nodes only when the old node is connected to a UV node
                XmlElement nodeU = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "ProtoGeometry.dll", "UV.U", "UV.U");
                migrationData.AppendNode(nodeU);
                string nodeUId = MigrationManager.GetGuidFromXmlElement(nodeU);

                XmlElement nodeV = MigrationManager.CreateFunctionNode(
                    data.Document, oldNode, 1, "ProtoGeometry.dll", "UV.V", "UV.V");
                migrationData.AppendNode(nodeV);
                string nodeVId = MigrationManager.GetGuidFromXmlElement(nodeV);

                // Update connectors
                PortId newInPortNodeU = new PortId(nodeUId, 0, PortType.Input);

                string nodeUVId = connector0.GetAttribute("start").ToString();
                data.ReconnectToPort(connector0, newInPortNodeU);
                data.CreateConnector(nodeU, 0, newNode, 1);
                data.CreateConnector(nodeV, 0, newNode, 2);
                data.CreateConnectorFromId(nodeUVId, 0, nodeVId, 0);
            }

            return migrationData;
        }
    }

    public class SurfaceArea : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Surface.Area", "Surface.Area");
        }
    }

    public class SurfaceDomain : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement codeBlockNode = MigrationManager.CreateCodeBlockNodeFrom(oldNode);
            codeBlockNode.SetAttribute("CodeText", "{{0,0},{1,1}};");
            codeBlockNode.SetAttribute("nickname", "Get Surface Domain");

            migrationData.AppendNode(codeBlockNode);
            return migrationData;
        }
    }
}
