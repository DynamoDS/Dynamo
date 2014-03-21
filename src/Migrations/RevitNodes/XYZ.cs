using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class Xyz : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Point.ByCoordinates",
                "Point.ByCoordinates@double,double,double");
        }
    }

    public class XyzFromPolar : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Point.ByCylindricalCoordinates", "Point.ByCylindricalCoordinates@CoordinateSystem,double,double,double");
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

            PortId newInPort3 = new PortId(newNodeId, 3, PortType.INPUT);
            PortId converterInPort = new PortId(converterNodeId, 0, PortType.INPUT);

            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            data.ReconnectToPort(connector0, newInPort3);
            data.ReconnectToPort(connector1, converterInPort);
            data.CreateConnector(converterNode, 0, newNode, 1);
            data.CreateConnector(identityCoordinateSystem, 0, newNode, 0);

            return migrationData;
        }
    }

    public class XyzToPolar : MigrationNode
    {
    }

    public class XyzFromSpherical : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Point.BySphericalCoordinates", "Point.BySphericalCoordinates@CoordinateSystem,double,double,double");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new node
            XmlElement identityCoordinateSystem = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll",
                "CoordinateSystem.Identity",
                "CoordinateSystem.Identity");
            migrationData.AppendNode(identityCoordinateSystem);

            XmlElement converterPhiNode = MigrationManager.CreateFunctionNode(
                data.Document, "DSCoreNodes.dll",
                "Math.RadiansToDegrees", "Math.RadiansToDegrees@double");
            migrationData.AppendNode(converterPhiNode);
            string converterPhiNodeId = MigrationManager.GetGuidFromXmlElement(converterPhiNode);

            XmlElement converterThetaNode = MigrationManager.CreateFunctionNode(
                data.Document, "DSCoreNodes.dll",
                "Math.RadiansToDegrees", "Math.RadiansToDegrees@double");
            migrationData.AppendNode(converterThetaNode);
            string converterThetaNodeId = MigrationManager.GetGuidFromXmlElement(converterThetaNode);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.INPUT);
            PortId oldInPort2 = new PortId(newNodeId, 2, PortType.INPUT);
            PortId newInPort3 = new PortId(newNodeId, 3, PortType.INPUT);

            PortId converterThetaInPort = new PortId(converterThetaNodeId, 0, PortType.INPUT);
            PortId converterPhiInPort = new PortId(converterPhiNodeId, 0, PortType.INPUT);
            
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            data.ReconnectToPort(connector0, newInPort3);
            data.ReconnectToPort(connector1, converterThetaInPort);
            data.ReconnectToPort(connector2, converterPhiInPort);
            
            data.CreateConnector(identityCoordinateSystem, 0, newNode, 0);
            data.CreateConnector(converterPhiNode, 0, newNode, 1);
            data.CreateConnector(converterThetaNode, 0, newNode, 2);

            return migrationData;
        }
    }

    public class XyzToSpherical : MigrationNode
    {
    }

    public class XyzFromListOfNumbers : MigrationNode
    {
    }

    public class XyzFromReferencePoint : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll", "ReferencePoint.Point", "ReferencePoint.Point");
        }
    }

    public class XyzComponents : MigrationNode
    {
    }

    public class XyzGetX : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Point.X", "Point.X");
        }
    }

    public class XyzGetY : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Point.Y", "Point.Y");
        }
    }

    public class XyzGetZ : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Point.Z", "Point.Z");
        }
    }

    public class XyzDistance : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Geometry.DistanceTo", "Geometry.DistanceTo@Geometry");
        }
    }

    public class XyzLength : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Vector.Length", "Vector.Length");
        }
    }

    public class XyzNormalize : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Vector.Normalized", "Vector.Normalized");
        }
    }

    public class XyzIsZeroLength : MigrationNode
    {
    }

    public class XyzBasisX : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Vector.XAxis", "Vector.XAxis");
        }
    }

    public class XyzBasisY : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Vector.YAxis", "Vector.YAxis");
        }
    }

    public class XyzBasisZ : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Vector.ZAxis", "Vector.ZAxis");
        }
    }

    public class XyzScale : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Vector.Scale", "Vector.Scale@double");
        }
    }

    public class XyzScaleOffset : MigrationNode
    {
    }

    public class XyzAdd : MigrationNode
    {
    }

    public class XyzSubtract : MigrationNode
    {
    }

    public class XyzAverage : MigrationNode
    {
    }

    public class XyzNegate : MigrationNode
    {
    }

    public class XyzCrossProduct : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Vector.Cross", "Vector.Cross@Vector");
        }
    }

    public class XyzDotProduct : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Vector.Dot", "Vector.Dot@Vector");
        }
    }

    public class XyzStartEndVector : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll", 
                "Vector.Normalized", "Vector.Normalized");

            migratedData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            XmlElement vectorNode = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll", 
                "Vector.ByTwoPoints", "Vector.ByTwoPoints@Point,Point");
            migratedData.AppendNode(vectorNode);
            string vectorNodeId = MigrationManager.GetGuidFromXmlElement(vectorNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.INPUT);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId newInPort0 = new PortId(vectorNodeId, 0, PortType.INPUT);
            PortId newInPort1 = new PortId(vectorNodeId, 1, PortType.INPUT);
            PortId newInPort2 = new PortId(newNodeId, 0, PortType.INPUT);

            data.ReconnectToPort(connector0, newInPort0);
            data.ReconnectToPort(connector1, newInPort1);
            data.CreateConnector(vectorNode, 0, newNode,0);

            return migratedData;
        }
    }

    public class ReferencePtGrid : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement dummyNode = MigrationManager.CreateDummyNode(oldNode, 9, 1);
            migrationData.AppendNode(dummyNode);

            return migrationData;
        }
    }

    public class XyzArrayAlongCurve : MigrationNode
    {
    }

    public class EqualDistXyzAlongCurve : MigrationNode
    {
    }

    public class XyzOnCurveOrEdge : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Curve.PointAtParameter", "Curve.PointAtParameter@double");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);
            data.ReconnectToPort(connector0, oldInPort1);
            data.ReconnectToPort(connector1, oldInPort0);

            return migrationData;
        }
    }

    public class XyzByDistanceOffsetFromOrigin : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement translateNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(translateNode, "ProtoGeometry.dll",
                "Geometry.Translate", "Geometry.Translate@Autodesk.DesignScript.Geometry.Vector,double");

            migratedData.AppendNode(translateNode);
            string translateNodeId = MigrationManager.GetGuidFromXmlElement(translateNode);

            XmlElement distanceToNode = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll", "Geometry.DistanceTo", "Geometry.DistanceTo@Geometry");
            migratedData.AppendNode(distanceToNode);
            string distanceToNodeId = MigrationManager.GetGuidFromXmlElement(distanceToNode);

            XmlElement multiplyNode = MigrationManager.CreateFunctionNode(
                data.Document, "", "*", "*@,");
            migratedData.AppendNode(multiplyNode);
            string multiplyNodeId = MigrationManager.GetGuidFromXmlElement(multiplyNode);

            XmlElement asVectorNode = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll", "Point.AsVector", "Point.AsVector");
            migratedData.AppendNode(asVectorNode);
            string asVectorNodeId = MigrationManager.GetGuidFromXmlElement(asVectorNode);


            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            XmlElement connector3 = null;
            if (connector0 != null)
            {
                connector3 = MigrationManager.CreateFunctionNodeFrom(connector0);
                data.CreateConnector(connector3);
            }

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.INPUT);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            XmlElement connector4 = null;
            if (connector1 != null)
            {
                connector4 = MigrationManager.CreateFunctionNodeFrom(connector1);
                data.CreateConnector(connector4);
            }

            PortId oldInPort2 = new PortId(oldNodeId, 2, PortType.INPUT);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            PortId newInPort0 = new PortId(translateNodeId, 0, PortType.INPUT);
            PortId newInPort1 = new PortId(translateNodeId, 1, PortType.INPUT);
            PortId newInPort2 = new PortId(translateNodeId, 2, PortType.INPUT);
            PortId newInPort3 = new PortId(distanceToNodeId, 0, PortType.INPUT);
            PortId newInPort4 = new PortId(distanceToNodeId, 1, PortType.INPUT);
            PortId newInPort5 = new PortId(multiplyNodeId, 1, PortType.INPUT);
            PortId newInPort6 = new PortId(asVectorNodeId, 0, PortType.INPUT);

            data.ReconnectToPort(connector0, newInPort0);
            data.ReconnectToPort(connector1, newInPort6);
            data.ReconnectToPort(connector2, newInPort5);
            data.ReconnectToPort(connector3, newInPort3);
            data.ReconnectToPort(connector4, newInPort4);

            data.CreateConnector(distanceToNode, 0, multiplyNode, 0);
            data.CreateConnector(asVectorNode, 0, translateNode, 1);
            data.CreateConnector(multiplyNode, 0, translateNode, 2);

            return migratedData;
        }
    }
}
