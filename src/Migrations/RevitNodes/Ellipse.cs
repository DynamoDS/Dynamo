using Dynamo.Models;
using System.Linq;
using Migrations;
using System.Xml;

namespace Dynamo.Nodes
{
    public class Ellipse : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"ProtoGeometry.dll", /*NXLT*/"Ellipse.ByOriginRadii",
                /*NXLT*/"Ellipse.ByOriginRadii@Point,double,double");
        }
    }

    public class EllipticalArc : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"EllipseArc.ByPlaneRadiiStartAngleSweepAngle",
                /*NXLT*/"EllipseArc.ByPlaneRadiiStartAngleSweepAngle@Plane,double,double,double,double");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new node
            XmlElement zAxis = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Vector.ZAxis",
                /*NXLT*/"Vector.ZAxis");
            migrationData.AppendNode(zAxis);

            XmlElement planeNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1, "", /*NXLT*/"Plane.ByOriginNormal",
                /*NXLT*/"Plane.ByOriginNormal@Point,Vector");
            planeNode.SetAttribute("isVisible", "false");
            migrationData.AppendNode(planeNode);
            string planeNodeId = MigrationManager.GetGuidFromXmlElement(planeNode);

            XmlElement converterNode0 = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 2,/*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"Math.RadiansToDegrees", /*NXLT*/"Math.RadiansToDegrees@double");
            migrationData.AppendNode(converterNode0);
            string converterNode0Id = MigrationManager.GetGuidFromXmlElement(converterNode0);

            XmlElement converterNode1 = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 3,/*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"Math.RadiansToDegrees", /*NXLT*/"Math.RadiansToDegrees@double");
            migrationData.AppendNode(converterNode1);
            string converterNode1Id = MigrationManager.GetGuidFromXmlElement(converterNode1);

            XmlElement minusNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 4, "", "-", "-@,");
            migrationData.AppendNode(minusNode);
            string minusNodeId = MigrationManager.GetGuidFromXmlElement(minusNode);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.Input);
            PortId oldInPort3 = new PortId(newNodeId, 3, PortType.Input);
            PortId oldInPort4 = new PortId(newNodeId, 4, PortType.Input);

            PortId planeNodeInPort = new PortId(planeNodeId, 0, PortType.Input);
            PortId converterInPort = new PortId(converterNode0Id, 0, PortType.Input);
            PortId minusNodeInPort0 = new PortId(minusNodeId, 0, PortType.Input);
            PortId minusNodeInPort1 = new PortId(minusNodeId, 1, PortType.Input);

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
