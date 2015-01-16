﻿using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class TransformIdentity: MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"ProtoGeometry.dll", /*NXLT*/"CoordinateSystem.Identity",
                /*NXLT*/"CoordinateSystem.Identity");
        }
    }

    public class TransformOriginAndVectors : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            var migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var newNode = MigrationManager.CreateCustomNodeFrom(oldNode.OwnerDocument, oldNode,
                /*NXLT*/"9a89dcea-3f52-48bc-ae41-b5a3ed1fd1bb",
                "TransformOriginVectors",
                "This node represents an upgrade of the 0.6.3 TransformOriginVectors node to 0.7.x",
                new List<string>() {"origin", "up", "forward"},
                new List<string>() {"CoordinateSystem"});

            migratedData.AppendNode(newNode);
            return migratedData;
        }
    }

    public class TransFromTo : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"CoordinateSystem.PostMultiplyBy",
                /*NXLT*/"CoordinateSystem.PostMultiplyBy@CoordinateSystem");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new nodes
            XmlElement vectorCrossFrom = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Vector.Cross", /*NXLT*/"Vector.Cross@Vector");
            migrationData.AppendNode(vectorCrossFrom);
            string vectorCrossFromId = MigrationManager.GetGuidFromXmlElement(vectorCrossFrom);

            XmlElement vectorCrossTo = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Vector.Cross", /*NXLT*/"Vector.Cross@Vector");
            migrationData.AppendNode(vectorCrossTo);
            string vectorCrossToId = MigrationManager.GetGuidFromXmlElement(vectorCrossTo);

            XmlElement csFrom = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 2,/*NXLT*/"ProtoGeometry.dll", /*NXLT*/"CoordinateSystem.ByOriginVectors",
                /*NXLT*/"CoordinateSystem.ByOriginVectors@Autodesk.DesignScript.Geometry.Point,"
                + /*NXLT*/"Autodesk.DesignScript.Geometry.Vector,Autodesk.DesignScript.Geometry"
                + /*NXLT*/".Vector,Autodesk.DesignScript.Geometry.Vector");
            string csFromId = MigrationManager.GetGuidFromXmlElement(csFrom);
            migrationData.AppendNode(csFrom);

            XmlElement csTo = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 3,/*NXLT*/"ProtoGeometry.dll", /*NXLT*/"CoordinateSystem.ByOriginVectors",
                /*NXLT*/"CoordinateSystem.ByOriginVectors@Autodesk.DesignScript.Geometry.Point,"
                + /*NXLT*/"Autodesk.DesignScript.Geometry.Vector,Autodesk.DesignScript.Geometry"
                + /*NXLT*/".Vector,Autodesk.DesignScript.Geometry.Vector");
            string csToId = MigrationManager.GetGuidFromXmlElement(csTo);
            migrationData.AppendNode(csTo);

            XmlElement csInverse = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 4,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"CoordinateSystem.Inverse", /*NXLT*/"CoordinateSystem.Inverse");
            migrationData.AppendNode(csInverse);

            //append asVector Node
            XmlElement pointAsVector0 = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 5,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Point.AsVector", /*NXLT*/"Point.AsVector");
            migrationData.AppendNode(pointAsVector0);
            string pointAsVector0Id = MigrationManager.GetGuidFromXmlElement(pointAsVector0);

            XmlElement pointAsVector1 = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 6,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Point.AsVector", /*NXLT*/"Point.AsVector");
            migrationData.AppendNode(pointAsVector1);
            string pointAsVector1Id = MigrationManager.GetGuidFromXmlElement(pointAsVector1);

            XmlElement pointAsVector2 = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 7,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Point.AsVector", /*NXLT*/"Point.AsVector");
            migrationData.AppendNode(pointAsVector2);
            string pointAsVector2Id = MigrationManager.GetGuidFromXmlElement(pointAsVector2);

            XmlElement pointAsVector3 = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 8,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Point.AsVector", /*NXLT*/"Point.AsVector");
            migrationData.AppendNode(pointAsVector3);
            string pointAsVector3Id = MigrationManager.GetGuidFromXmlElement(pointAsVector3);

            PortId pToV0 = new PortId(pointAsVector0Id, 0, PortType.Input);
            PortId pToV1 = new PortId(pointAsVector1Id, 0, PortType.Input);
            PortId pToV2 = new PortId(pointAsVector2Id, 0, PortType.Input);
            PortId pToV3 = new PortId(pointAsVector3Id, 0, PortType.Input);

            // Update connectors
            PortId csFrom0 = new PortId(csFromId, 0, PortType.Input);
            PortId csFrom1 = new PortId(csFromId, 1, PortType.Input);
            PortId csFrom2 = new PortId(csFromId, 2, PortType.Input);
            PortId csFrom3 = new PortId(csFromId, 3, PortType.Input);
            PortId csTo0 = new PortId(csToId, 0, PortType.Input);
            PortId csTo1 = new PortId(csToId, 1, PortType.Input);
            PortId csTo2 = new PortId(csToId, 2, PortType.Input);
            PortId csTo3 = new PortId(csToId, 3, PortType.Input);

            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.Input);
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.Input);
            PortId oldInPort2 = new PortId(newNodeId, 2, PortType.Input);
            PortId oldInPort3 = new PortId(newNodeId, 3, PortType.Input);
            PortId oldInPort4 = new PortId(newNodeId, 4, PortType.Input);
            PortId oldInPort5 = new PortId(newNodeId, 5, PortType.Input);

            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);
            XmlElement connector3 = data.FindFirstConnector(oldInPort3);
            XmlElement connector4 = data.FindFirstConnector(oldInPort4);
            XmlElement connector5 = data.FindFirstConnector(oldInPort5);

            data.ReconnectToPort(connector0, csFrom0);
            data.ReconnectToPort(connector1, pToV0);
            data.ReconnectToPort(connector2, pToV1);
            data.ReconnectToPort(connector3, csTo0);
            data.ReconnectToPort(connector4, pToV2);
            data.ReconnectToPort(connector5, pToV3);

            data.CreateConnector(pointAsVector0, 0, csFrom, 3); 
            data.CreateConnector(pointAsVector1, 0, csFrom, 2);
            data.CreateConnector(pointAsVector2, 0, csTo, 3);
            data.CreateConnector(pointAsVector3, 0, csTo, 2);

            data.CreateConnector(pointAsVector0, 0, vectorCrossFrom, 1);
            data.CreateConnector(pointAsVector1, 0, vectorCrossFrom, 0);
            data.CreateConnector(pointAsVector2, 0, vectorCrossTo, 1);
            data.CreateConnector(pointAsVector3, 0, vectorCrossTo, 0);

            data.CreateConnector(vectorCrossFrom, 0, csFrom, 1);
            data.CreateConnector(vectorCrossTo, 0, csTo, 1);
            data.CreateConnector(csFrom, 0, csInverse, 0);
            data.CreateConnector(csInverse, 0, newNode, 1);
            data.CreateConnector(csTo, 0, newNode, 0);

            return migrationData;
        }
    }

    public class TransformScaleBasis : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"ProtoGeometry.dll", /*NXLT*/"CoordinateSystem.Scale",
                /*NXLT*/"CoordinateSystem.Scale@double");
        }
    }

    public class TransformRotation : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"CoordinateSystem.Rotate", /*NXLT*/"CoordinateSystem.Rotate@Point,Vector,double");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new node
            XmlElement identityCoordinateSystem = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"CoordinateSystem.Identity",
                /*NXLT*/"CoordinateSystem.Identity");
            migrationData.AppendNode(identityCoordinateSystem);

            XmlElement converterNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1,/*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"Math.RadiansToDegrees", /*NXLT*/"Math.RadiansToDegrees@double");
            migrationData.AppendNode(converterNode);
            string converterNodeId = MigrationManager.GetGuidFromXmlElement(converterNode);

            //append asVector Node
            XmlElement pointAsVector0 = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Point.AsVector", /*NXLT*/"Point.AsVector");
            migrationData.AppendNode(pointAsVector0);
            string pointAsVector0Id = MigrationManager.GetGuidFromXmlElement(pointAsVector0);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.Input);
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.Input);
            PortId oldInPort2 = new PortId(newNodeId, 2, PortType.Input);

            PortId newInPort1 = new PortId(newNodeId, 1, PortType.Input);
            PortId newInPort2 = new PortId(pointAsVector0Id, 0, PortType.Input);

            PortId converterInPort = new PortId(converterNodeId, 0, PortType.Input);

            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, newInPort2);
            data.ReconnectToPort(connector2, converterInPort);

            data.CreateConnector(converterNode, 0, newNode, 3);
            data.CreateConnector(identityCoordinateSystem, 0, newNode, 0);
            data.CreateConnector(pointAsVector0, 0, newNode, 2);

            return migrationData;
        }
    }

    public class TransformTranslation : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"CoordinateSystem.Translate",
                /*NXLT*/"CoordinateSystem.Translate@Autodesk.DesignScript.Geometry.Vector");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new node
            XmlElement identityCoordinateSystem = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"CoordinateSystem.Identity",
                /*NXLT*/"CoordinateSystem.Identity");
            migrationData.AppendNode(identityCoordinateSystem);

            //append asVector Node
            XmlElement pointAsVector0 = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Point.AsVector", /*NXLT*/"Point.AsVector");
            migrationData.AppendNode(pointAsVector0);
            string pointAsVector0Id = MigrationManager.GetGuidFromXmlElement(pointAsVector0);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(newNodeId, 1, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            PortId pToV0 = new PortId(pointAsVector0Id, 0, PortType.Input);

            data.ReconnectToPort(connector0, pToV0);
            data.CreateConnector(pointAsVector0, 0, newNode, 1); 
            data.CreateConnector(identityCoordinateSystem, 0, newNode, 0);

            return migrationData;
        }
    }

    public class TransformReflection : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"CoordinateSystem.Mirror",
                /*NXLT*/"CoordinateSystem.Mirror@Autodesk.DesignScript.Geometry.Plane");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new node
            XmlElement identityCoordinateSystem = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"CoordinateSystem.Identity",
                /*NXLT*/"CoordinateSystem.Identity");
            migrationData.AppendNode(identityCoordinateSystem);

            // Update connectors
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId newInPort1 = new PortId(newNodeId, 1, PortType.Input);

            data.ReconnectToPort(connector0, newInPort1);
            data.CreateConnector(identityCoordinateSystem, 0, newNode, 0);

            return migrationData;
        }
    }

    public class TransformPoint : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Geometry.Transform",
                /*NXLT*/"Geometry.Transform@Autodesk.DesignScript.Geometry.CoordinateSystem");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Update connectors
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId newInPort0 = new PortId(newNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(newNodeId, 1, PortType.Input);

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, newInPort0);

            return migrationData;
        }
    }

    public class Multiplytransform : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"ProtoGeometry.dll", /*NXLT*/"CoordinateSystem.PostMultiplyBy",
                /*NXLT*/"CoordinateSystem.PostMultiplyBy@CoordinateSystem");
        }
    }

    public class TransToCurve : MigrationNode
    {
    }

    public class InverseTransform : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"ProtoGeometry.dll", /*NXLT*/"CoordinateSystem.Inverse", /*NXLT*/"CoordinateSystem.Inverse");
        }
    }

    public class BasisX : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Vector.AsPoint", /*NXLT*/"Vector.AsPoint");
            migrationData.AppendNode(newNode);

            XmlElement axisNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"CoordinateSystem.XAxis", /*NXLT*/"CoordinateSystem.XAxis");
            migrationData.AppendNode(axisNode);
            string axisNodeId = MigrationManager.GetGuidFromXmlElement(axisNode);

            // Update connectors
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId newInPort0 = new PortId(axisNodeId, 0, PortType.Input);

            data.ReconnectToPort(connector0, newInPort0);
            data.CreateConnector(axisNode, 0, newNode, 0);

            return migrationData;
        }
    }

    public class BasisY : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Vector.AsPoint", /*NXLT*/"Vector.AsPoint");
            migrationData.AppendNode(newNode);

            XmlElement axisNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"CoordinateSystem.YAxis", /*NXLT*/"CoordinateSystem.YAxis");
            migrationData.AppendNode(axisNode);
            string axisNodeId = MigrationManager.GetGuidFromXmlElement(axisNode);

            // Update connectors
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId newInPort0 = new PortId(axisNodeId, 0, PortType.Input);

            data.ReconnectToPort(connector0, newInPort0);
            data.CreateConnector(axisNode, 0, newNode, 0);

            return migrationData;
        }
    }

    public class BasisZ : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Vector.AsPoint", /*NXLT*/"Vector.AsPoint");
            migrationData.AppendNode(newNode);

            XmlElement axisNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"CoordinateSystem.ZAxis", /*NXLT*/"CoordinateSystem.ZAxis");
            migrationData.AppendNode(axisNode);
            string axisNodeId = MigrationManager.GetGuidFromXmlElement(axisNode);

            // Update connectors
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId newInPort0 = new PortId(axisNodeId, 0, PortType.Input);

            data.ReconnectToPort(connector0, newInPort0);
            data.CreateConnector(axisNode, 0, newNode, 0);

            return migrationData;
        }
    }

    public class Origin : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"ProtoGeometry.dll", /*NXLT*/"CoordinateSystem.Origin", /*NXLT*/"CoordinateSystem.Origin");
        }
    }
}
