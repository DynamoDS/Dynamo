﻿using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class Uv : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"ProtoGeometry.dll", /*NXLT*/"UV.ByCoordinates",
                /*NXLT*/"UV.ByCoordinates@double,double");
        }
    }

    public class Domain : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement codeBlockNode = MigrationManager.CreateCodeBlockNodeFrom(oldNode);
            codeBlockNode.SetAttribute(/*NXLT*/"CodeText", "{min,max};");
            codeBlockNode.SetAttribute(/*NXLT*/"nickname", "Domain");

            migrationData.AppendNode(codeBlockNode);
            return migrationData;
        }
    }
    
    public class Domain2D : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement codeBlockNode = MigrationManager.CreateCodeBlockNodeFrom(oldNode);
            codeBlockNode.SetAttribute(/*NXLT*/"CodeText", /*NXLT*/"{{min.U,min.V},{max.U,max.V}};");
            codeBlockNode.SetAttribute(/*NXLT*/"nickname", "UV Domain");

            migrationData.AppendNode(codeBlockNode);
            return migrationData;
        }
    }

    public class UvGrid : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement codeBlockNode = MigrationManager.CreateCodeBlockNodeModelNode(
                data.Document, oldNode, 0,
                /*NXLT*/"domain[0][0]..domain[1][0]..#ucount+1;\n" +
                /*NXLT*/"domain[0][1]..domain[1][1]..#vcount+1;");
            migrationData.AppendNode(codeBlockNode);
            string codeBlockNodeId = MigrationManager.GetGuidFromXmlElement(codeBlockNode);

            XmlElement uvNode = MigrationManager.CreateFunctionNode(data.Document, oldNode, 1,
                /*NXLT*/"ProtoGeometry.dll", /*NXLT*/"UV.ByCoordinates", /*NXLT*/"UV.ByCoordinates@double,double");
            uvNode.SetAttribute("lacing", "CrossProduct");
            migrationData.AppendNode(uvNode);

            XmlElement flattenNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(flattenNode, /*NXLT*/"",
                /*NXLT*/"Flatten", /*NXLT*/"Flatten");
            flattenNode.SetAttribute(/*NXLT*/"lacing", "Shortest");
            migrationData.AppendNode(flattenNode);

            // Update connectors
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            PortId oldInPort2 = new PortId(oldNodeId, 2, PortType.Input);
            PortId newInPort0 = new PortId(codeBlockNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(codeBlockNodeId, 1, PortType.Input);
            PortId newInPort2 = new PortId(codeBlockNodeId, 2, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            data.ReconnectToPort(connector0, newInPort0);
            data.ReconnectToPort(connector1, newInPort1);
            data.ReconnectToPort(connector2, newInPort2);

            data.CreateConnector(codeBlockNode, 0, uvNode, 0);
            data.CreateConnector(codeBlockNode, 1, uvNode, 1);
            data.CreateConnector(uvNode, 0, flattenNode, 0);

            return migrationData;
        }
    }

    public class UvRandom : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement codeBlockNode = MigrationManager.CreateCodeBlockNodeModelNode(
                data.Document, oldNode, 0,
                /*NXLT*/"dom[0][0]+Math.RandomList\n" +
                /*NXLT*/"(ucount*vcount)\n" +
                /*NXLT*/"*(dom[1][0]-dom[0][0]);\n" +
                /*NXLT*/"dom[0][1]+Math.RandomList\n" +
                /*NXLT*/"(ucount*vcount)\n" +
                /*NXLT*/"*(dom[1][1]-dom[0][1]);");
            codeBlockNode.SetAttribute(/*NXLT*/"nickname", "Random UV");
            migrationData.AppendNode(codeBlockNode);
            string codeBlockNodeId = MigrationManager.GetGuidFromXmlElement(codeBlockNode);

            XmlElement uvNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(uvNode,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"UV.ByCoordinates", /*NXLT*/"UV.ByCoordinates@double,double");
            uvNode.SetAttribute(/*NXLT*/"lacing", "Longest");
            migrationData.AppendNode(uvNode);

            // Update connectors
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            PortId oldInPort2 = new PortId(oldNodeId, 2, PortType.Input);
            PortId newInPort0 = new PortId(codeBlockNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(codeBlockNodeId, 1, PortType.Input);
            PortId newInPort2 = new PortId(codeBlockNodeId, 2, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            data.ReconnectToPort(connector0, newInPort0);
            data.ReconnectToPort(connector1, newInPort1);
            data.ReconnectToPort(connector2, newInPort2);

            data.CreateConnector(codeBlockNode, 0, uvNode, 0);
            data.CreateConnector(codeBlockNode, 1, uvNode, 1);

            return migrationData;
        }
    }
}
