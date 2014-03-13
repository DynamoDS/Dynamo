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
                data.Document, "ProtoGeometry.dll", "UV.U", "UV.U");
            migratedData.AppendNode(uvU);
            string uvUId = MigrationManager.GetGuidFromXmlElement(uvU);

            XmlElement uvV = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll", "UV.V", "UV.V");
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
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "ReferencePoint.ByPointVectorDistance",
                "ReferencePoint.ByPointVectorDistance@Point,Vector,double");
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
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "ReferencePoint.ByLengthOnCurveReference",
                "ReferencePoint.ByLengthOnCurveReference@CurveReference,double");
        }
    }

    public class DistanceBetweenPoints : MigrationNode
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
}
