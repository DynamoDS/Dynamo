using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class TransformIdentity: MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "CoordinateSystem.Identity",
                "CoordinateSystem.Identity");
        }
    }

    public class TransformOriginAndVectors : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement coordinateNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(coordinateNode, "ProtoGeometry.dll",
                "CoordinateSystem.ByOriginVectors",
                "CoordinateSystem.ByOriginVectors@Autodesk.DesignScript.Geometry.Point," + 
                "Autodesk.DesignScript.Geometry.Vector,Autodesk.DesignScript.Geometry.Vector," + 
                "Autodesk.DesignScript.Geometry.Vector");

            migratedData.AppendNode(coordinateNode);
            string coordinateNodeId = MigrationManager.GetGuidFromXmlElement(coordinateNode);

            XmlElement asVectorNode0 = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll", "Point.AsVector", "Point.AsVector");
            migratedData.AppendNode(asVectorNode0);
            string asVectorNode0Id = MigrationManager.GetGuidFromXmlElement(asVectorNode0);

            XmlElement asVectorNode1 = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll", "Point.AsVector", "Point.AsVector");
            migratedData.AppendNode(asVectorNode1);
            string asVectorNode1Id = MigrationManager.GetGuidFromXmlElement(asVectorNode1);

            XmlElement vectorCrossNode = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll", "Vector.Cross", "Vector.Cross@Vector");
            migratedData.AppendNode(vectorCrossNode);
            string vectorCrossNodeId = MigrationManager.GetGuidFromXmlElement(vectorCrossNode);

            XmlElement vectorReverseNode = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll", "Vector.Reverse", "Vector.Reverse");
            migratedData.AppendNode(vectorReverseNode);
            string vectorReverseNodeId = MigrationManager.GetGuidFromXmlElement(vectorReverseNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.INPUT);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId oldInPort2 = new PortId(oldNodeId, 2, PortType.INPUT);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            PortId newInPort0 = new PortId(coordinateNodeId, 0, PortType.INPUT);
            PortId newInPort1 = new PortId(coordinateNodeId, 1, PortType.INPUT);
            PortId newInPort2 = new PortId(coordinateNodeId, 2, PortType.INPUT);
            PortId newInPort3 = new PortId(coordinateNodeId, 3, PortType.INPUT);

            PortId newInPort4 = new PortId(asVectorNode0Id, 0, PortType.INPUT);
            PortId newInPort5 = new PortId(asVectorNode1Id, 0, PortType.INPUT);

            data.ReconnectToPort(connector0, newInPort0);
            data.ReconnectToPort(connector1, newInPort4);
            data.ReconnectToPort(connector2, newInPort5);

            data.CreateConnector(asVectorNode0, 0, vectorCrossNode, 0);
            data.CreateConnector(asVectorNode1, 0, vectorCrossNode, 1);
            data.CreateConnector(vectorCrossNode, 0, vectorReverseNode, 0);
            data.CreateConnector(vectorReverseNode, 0, coordinateNode, 1);
            data.CreateConnector(asVectorNode1, 0, coordinateNode, 2);
            data.CreateConnector(asVectorNode0, 0, coordinateNode, 3);

            return migratedData;
        }
    }

    public class TransFromTo : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "CoordinateSystem.PostMultiplyBy",
                "CoordinateSystem.PostMultiplyBy@CoordinateSystem");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new nodes
            XmlElement vectorCrossFrom = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll",
                "Vector.Cross", "Vector.Cross@Vector");
            migrationData.AppendNode(vectorCrossFrom);
            string vectorCrossFromId = MigrationManager.GetGuidFromXmlElement(vectorCrossFrom);

            XmlElement vectorCrossTo = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll",
                "Vector.Cross", "Vector.Cross@Vector");
            migrationData.AppendNode(vectorCrossTo);
            string vectorCrossToId = MigrationManager.GetGuidFromXmlElement(vectorCrossTo);

            XmlElement csFrom = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll", "CoordinateSystem.ByOriginVectors",
                "CoordinateSystem.ByOriginVectors@Autodesk.DesignScript.Geometry.Point,"
                + "Autodesk.DesignScript.Geometry.Vector,Autodesk.DesignScript.Geometry"
                + ".Vector,Autodesk.DesignScript.Geometry.Vector");
            string csFromId = MigrationManager.GetGuidFromXmlElement(csFrom);
            migrationData.AppendNode(csFrom);

            XmlElement csTo = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll", "CoordinateSystem.ByOriginVectors",
                "CoordinateSystem.ByOriginVectors@Autodesk.DesignScript.Geometry.Point,"
                + "Autodesk.DesignScript.Geometry.Vector,Autodesk.DesignScript.Geometry"
                + ".Vector,Autodesk.DesignScript.Geometry.Vector");
            string csToId = MigrationManager.GetGuidFromXmlElement(csTo);
            migrationData.AppendNode(csTo);

            XmlElement csInverse = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll",
                "CoordinateSystem.Inverse", "CoordinateSystem.Inverse");
            migrationData.AppendNode(csInverse);

            // Update connectors
            PortId csFrom0 = new PortId(csFromId, 0, PortType.INPUT);
            PortId csFrom1 = new PortId(csFromId, 1, PortType.INPUT);
            PortId csFrom2 = new PortId(csFromId, 2, PortType.INPUT);
            PortId csFrom3 = new PortId(csFromId, 3, PortType.INPUT);
            PortId csTo0 = new PortId(csToId, 0, PortType.INPUT);
            PortId csTo1 = new PortId(csToId, 1, PortType.INPUT);
            PortId csTo2 = new PortId(csToId, 2, PortType.INPUT);
            PortId csTo3 = new PortId(csToId, 3, PortType.INPUT);

            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.INPUT);
            PortId oldInPort2 = new PortId(newNodeId, 2, PortType.INPUT);
            PortId oldInPort3 = new PortId(newNodeId, 3, PortType.INPUT);
            PortId oldInPort4 = new PortId(newNodeId, 4, PortType.INPUT);
            PortId oldInPort5 = new PortId(newNodeId, 5, PortType.INPUT);

            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);
            XmlElement connector3 = data.FindFirstConnector(oldInPort3);
            XmlElement connector4 = data.FindFirstConnector(oldInPort4);
            XmlElement connector5 = data.FindFirstConnector(oldInPort5);

            if (connector1 != null)
            {
                string uFromId = connector1.GetAttribute("start").ToString();
                data.CreateConnectorFromId(uFromId, 0, vectorCrossFromId, 1);
            }

            if (connector2 != null)
            {
                string fFromId = connector2.GetAttribute("start").ToString();
                data.CreateConnectorFromId(fFromId, 0, vectorCrossFromId, 0);
            }

            if (connector4 != null)
            {
                string uToId = connector4.GetAttribute("start").ToString();
                data.CreateConnectorFromId(uToId, 0, vectorCrossToId, 1);
            }

            if (connector5 != null)
            {
                string fToId = connector5.GetAttribute("start").ToString();
                data.CreateConnectorFromId(fToId, 0, vectorCrossToId, 0);
            }

            data.ReconnectToPort(connector0, csFrom0);
            data.ReconnectToPort(connector1, csFrom3);
            data.ReconnectToPort(connector2, csFrom2);
            data.ReconnectToPort(connector3, csTo0);
            data.ReconnectToPort(connector4, csTo3);
            data.ReconnectToPort(connector5, csTo2);

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
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "CoordinateSystem.Scale",
                "CoordinateSystem.Scale@double");
        }
    }

    public class TransformRotation : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "CoordinateSystem.Rotate", "CoordinateSystem.Rotate@Point,Vector,double");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new node
            XmlElement identityCoordinateSystem = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll",
                "CoordinateSystem.Identity",
                "CoordinateSystem.Identity");
            migrationData.AppendNode(identityCoordinateSystem);

            XmlElement converterNode = MigrationManager.CreateFunctionNode(
                data.Document, "DSCoreNodes.dll",
                "Math.RadiansToDegrees", "Math.RadiansToDegrees@double");
            migrationData.AppendNode(converterNode);
            string converterNodeId = MigrationManager.GetGuidFromXmlElement(converterNode);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.INPUT);
            PortId oldInPort2 = new PortId(newNodeId, 2, PortType.INPUT);

            PortId newInPort1 = new PortId(newNodeId, 1, PortType.INPUT);
            PortId newInPort2 = new PortId(newNodeId, 2, PortType.INPUT);

            PortId converterInPort = new PortId(converterNodeId, 0, PortType.INPUT);

            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, newInPort2);
            data.ReconnectToPort(connector2, converterInPort);

            data.CreateConnector(converterNode, 0, newNode, 3);
            data.CreateConnector(identityCoordinateSystem, 0, newNode, 0);

            return migrationData;
        }
    }

    public class TransformTranslation : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "CoordinateSystem.Translate",
                "CoordinateSystem.Translate@Autodesk.DesignScript.Geometry.Vector");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new node
            XmlElement identityCoordinateSystem = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll",
                "CoordinateSystem.Identity",
                "CoordinateSystem.Identity");
            migrationData.AppendNode(identityCoordinateSystem);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            PortId newInPort1 = new PortId(newNodeId, 1, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            data.ReconnectToPort(connector0, newInPort1);
            data.CreateConnector(identityCoordinateSystem, 0, newNode, 0);

            return migrationData;
        }
    }

    public class TransformReflection : MigrationNode
    {
    }

    public class TransformPoint : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Geometry.Translate",
                "Geometry.Translate@Autodesk.DesignScript.Geometry.Vector");
        }
    }

    public class Multiplytransform : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "CoordinateSystem.PostMultiplyBy",
                "CoordinateSystem.PostMultiplyBy@CoordinateSystem");
        }
    }

    public class TransToCurve : MigrationNode
    {
    }

    public class InverseTransform : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "CoordinateSystem.Inverse", "CoordinateSystem.Inverse");
        }
    }

    public class BasisX : MigrationNode
    {
    }

    public class BasisY : MigrationNode
    {
    }

    public class BasisZ : MigrationNode
    {
    }

    public class Origin : MigrationNode
    {
    }
}
