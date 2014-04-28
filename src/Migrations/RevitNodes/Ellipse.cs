using Dynamo.Models;
using System.Linq;
using Migrations;
using System.Xml;

namespace Dynamo.Nodes
{
    public class Ellipse : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Ellipse.ByOriginRadii",
                "Ellipse.ByOriginRadii@Point,double,double");
        }
    }

    public class EllipticalArc : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "EllipseArc.ByPlaneRadiiStartAngleSweepAngle", 
                "EllipseArc.ByPlaneRadiiStartAngleSweepAngle@Plane,double,double,double,double");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new node
            XmlElement zAxis = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "ProtoGeometry.dll",
                "Vector.ZAxis",
                "Vector.ZAxis");
            migrationData.AppendNode(zAxis);

            XmlElement planeNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1, "", "Plane.ByOriginNormal", 
                "Plane.ByOriginNormal@Point,Vector");
            planeNode.SetAttribute("isVisible", "false");
            migrationData.AppendNode(planeNode);
            string planeNodeId = MigrationManager.GetGuidFromXmlElement(planeNode);

            XmlElement converterNode0 = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 2, "DSCoreNodes.dll",
                "Math.RadiansToDegrees", "Math.RadiansToDegrees@double");
            migrationData.AppendNode(converterNode0);
            string converterNode0Id = MigrationManager.GetGuidFromXmlElement(converterNode0);

            XmlElement converterNode1 = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 3, "DSCoreNodes.dll",
                "Math.RadiansToDegrees", "Math.RadiansToDegrees@double");
            migrationData.AppendNode(converterNode1);
            string converterNode1Id = MigrationManager.GetGuidFromXmlElement(converterNode1);

            XmlElement minusNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 4, "", "-", "-@,");
            migrationData.AppendNode(minusNode);
            string minusNodeId = MigrationManager.GetGuidFromXmlElement(minusNode);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            PortId oldInPort3 = new PortId(newNodeId, 3, PortType.INPUT);
            PortId oldInPort4 = new PortId(newNodeId, 4, PortType.INPUT);

            PortId planeNodeInPort = new PortId(planeNodeId, 0, PortType.INPUT);
            PortId converterInPort = new PortId(converterNode0Id, 0, PortType.INPUT);
            PortId minusNodeInPort0 = new PortId(minusNodeId, 0, PortType.INPUT);
            PortId minusNodeInPort1 = new PortId(minusNodeId, 1, PortType.INPUT);

            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector3 = data.FindFirstConnector(oldInPort3);
            XmlElement connector4 = data.FindFirstConnector(oldInPort4);

            data.ReconnectToPort(connector0, planeNodeInPort);
            data.ReconnectToPort(connector3, converterInPort);
            data.ReconnectToPort(connector4, minusNodeInPort0);

            if (connector3 != null)
            {
                XmlElement connector5 = MigrationManager.CreateFunctionNodeFrom(connector3);
                data.CreateConnector(connector5);
                data.ReconnectToPort(connector5, minusNodeInPort1);
            }

            data.CreateConnector(minusNode, 0, converterNode1, 0);
            data.CreateConnector(converterNode0, 0, newNode, 3);
            data.CreateConnector(converterNode1, 0, newNode, 4);
            data.CreateConnector(zAxis, 0, planeNode, 1);
            data.CreateConnector(planeNode, 0, newNode, 0);

            return migrationData;
        }
    }
}
