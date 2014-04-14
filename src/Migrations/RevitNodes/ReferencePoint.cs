using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class ReferencePointByXyz : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "ReferencePoint.ByPoint", "ReferencePoint.ByPoint@Point");
        }
    }

    public class PointOnEdge : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "ReferencePoint.ByParameterOnCurveReference",
                "ReferencePoint.ByParameterOnCurveReference@CurveReference,double");
        }
    }

    public class PointOnFaceUv : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsReferencePoint = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsReferencePoint, "RevitNodes.dll",
                "ReferencePoint.ByParametersOnFaceReference",
                "ReferencePoint.ByParametersOnFaceReference@FaceReference,double,double");

            migratedData.AppendNode(dsReferencePoint);
            string dsReferencePointId = MigrationManager.GetGuidFromXmlElement(dsReferencePoint);

            XmlElement uvU = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "ProtoGeometry.dll", "UV.U", "UV.U");
            migratedData.AppendNode(uvU);
            string uvUId = MigrationManager.GetGuidFromXmlElement(uvU);

            XmlElement uvV = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1, "ProtoGeometry.dll", "UV.V", "UV.V");
            migratedData.AppendNode(uvV);
            string uvVId = MigrationManager.GetGuidFromXmlElement(uvV);

            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.INPUT);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            XmlElement connector2 = null;
            if (connector1!=null)
            {
                connector2 = MigrationManager.CreateFunctionNodeFrom(connector1);
                data.CreateConnector(connector2);
            }

            PortId newInPort = new PortId(dsReferencePointId, 0, PortType.INPUT);
            data.ReconnectToPort(connector0, newInPort);
            newInPort = new PortId(uvUId, 0, PortType.INPUT);
            data.ReconnectToPort(connector1, newInPort);

            if (connector2 != null)
            {
                newInPort = new PortId(uvVId, 0, PortType.INPUT);
                data.ReconnectToPort(connector2, newInPort);
            }

            data.CreateConnector(uvU, 0, dsReferencePoint, 1);
            data.CreateConnector(uvV, 0, dsReferencePoint, 2);

            return migratedData;           
        }
    }

    public class PointNormalDistance : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "RevitNodes.dll",
                "ReferencePoint.ByPointVectorDistance",
                "ReferencePoint.ByPointVectorDistance@Point,Vector,double");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new nodes
            XmlElement refptAsPoint = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "RevitNodes.dll",
                "ReferencePoint.Point", "ReferencePoint.Point");
            migrationData.AppendNode(refptAsPoint);
            string refptAsPointId = MigrationManager.GetGuidFromXmlElement(refptAsPoint);

            XmlElement pointAsVector = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1, "ProtoGeometry.dll",
                "Point.AsVector", "Point.AsVector");
            migrationData.AppendNode(pointAsVector);
            string pointAsVectorId = MigrationManager.GetGuidFromXmlElement(pointAsVector);

            XmlElement vectorNormalized = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 2, "ProtoGeometry.dll",
                "Vector.Normalized", "Vector.Normalized");
            migrationData.AppendNode(vectorNormalized);
            string vectorNormalizedId = MigrationManager.GetGuidFromXmlElement(vectorNormalized);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.INPUT);
            PortId oldInPort2 = new PortId(newNodeId, 2, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            PortId refptAsPointInPort0 = new PortId(refptAsPointId, 0, PortType.INPUT);
            PortId pointAsVectorInPort0 = new PortId(pointAsVectorId, 0, PortType.INPUT);

            data.ReconnectToPort(connector0, refptAsPointInPort0);
            data.ReconnectToPort(connector1, pointAsVectorInPort0);
            data.CreateConnector(refptAsPoint, 0, newNode, 0);
            data.CreateConnector(pointAsVector, 0, vectorNormalized, 0);
            data.CreateConnector(vectorNormalized, 0, newNode, 1);

            return migrationData;
        }

    }

    public class PlaneFromRefPoint : MigrationNode
    {
    }

    public class PointOnCurveByLength : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            // Create DSFunction node
            XmlElement newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "RevitNodes.dll",
                "ReferencePoint.ByLengthOnCurveReference",
                "ReferencePoint.ByLengthOnCurveReference@CurveReference,double");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new adapter node
            XmlElement codeBlockNode = MigrationManager.CreateCodeBlockNodeModelNode(
                data.Document, oldNode, 0,
                "// Convert ModelCurve into CurveReference\n" +
                "curve.CurveReference;\n\n" +
                "// Convert normalized length into actual length\n" +
                "// and flip the evaluation if 'beginning' is set to false\n" +
                "len * (normalized ? curve.Curve.Length : 1)\n" +
                "* (beginning ? 1 : -1) + (beginning ? 0 : curve.Curve.Length);");
            migrationData.AppendNode(codeBlockNode);
            string cbnId = MigrationManager.GetGuidFromXmlElement(codeBlockNode);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.INPUT);
            PortId oldInPort2 = new PortId(newNodeId, 2, PortType.INPUT);
            PortId oldInPort3 = new PortId(newNodeId, 3, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);
            XmlElement connector3 = data.FindFirstConnector(oldInPort3);
            PortId cbnInPort0 = new PortId(cbnId, 0, PortType.INPUT);
            PortId cbnInPort1 = new PortId(cbnId, 1, PortType.INPUT);
            PortId cbnInPort2 = new PortId(cbnId, 2, PortType.INPUT);
            PortId cbnInPort3 = new PortId(cbnId, 3, PortType.INPUT);

            data.ReconnectToPort(connector0, cbnInPort0);
            data.ReconnectToPort(connector1, cbnInPort1);
            data.ReconnectToPort(connector2, cbnInPort2);
            data.ReconnectToPort(connector3, cbnInPort3);
            data.CreateConnector(codeBlockNode, 0, newNode, 0);
            data.CreateConnector(codeBlockNode, 1, newNode, 1);

            return migrationData;

        }
    }

    public class DistanceBetweenPoints : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement distanceToPoint = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(distanceToPoint, "ProtoGeometry.dll",
                "Geometry.DistanceTo",
                "Geometry.DistanceTo@Autodesk.DesignScript.Geometry.Geometry");
            migratedData.AppendNode(distanceToPoint);
            string distanceToPointId = MigrationManager.GetGuidFromXmlElement(distanceToPoint);

            XmlElement point1 = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "ProtoGeometry.dll", 
                "ReferencePoint.Point", "ReferencePoint.Point");
            migratedData.AppendNode(point1);
            string point1Id = MigrationManager.GetGuidFromXmlElement(point1);

            XmlElement point2 = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1, "ProtoGeometry.dll", 
                "ReferencePoint.Point", "ReferencePoint.Point");
            migratedData.AppendNode(point2);
            string point2Id = MigrationManager.GetGuidFromXmlElement(point2);


            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.INPUT);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId newInPort0 = new PortId(point1Id, 0, PortType.INPUT);
            PortId newInPort1 = new PortId(point2Id, 0, PortType.INPUT);
            data.ReconnectToPort(connector0, newInPort0);
            data.ReconnectToPort(connector1, newInPort1);

            data.CreateConnector(point1, 0, distanceToPoint, 0);
            data.CreateConnector(point2, 0, distanceToPoint, 1);

            return migratedData;  
        }
    }
}
