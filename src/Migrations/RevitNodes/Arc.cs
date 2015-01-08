using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class ArcStartMiddleEnd : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"ProtoGeometry.dll", /*NXLT*/"Arc.ByThreePoints",
                /*NXLT*/"Arc.ByThreePoints@Point,Point,Point");
        }
    }

    public class ArcCenter : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            // This migration assumes that the first input of the old node is
            // always an XYZ and never a Transform.

            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Arc.ByCenterPointRadiusAngle", /*NXLT*/"Arc.ByCenterPointRadiusAngle@Point,double,double,double,Vector");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new nodes
            XmlElement zAxisNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0,/*NXLT*/"ProtoGeometry.dll", /*NXLT*/"Vector.ZAxis", /*NXLT*/"Vector.ZAxis");
            migrationData.AppendNode(zAxisNode);
            string zAxisNodeId = MigrationManager.GetGuidFromXmlElement(zAxisNode);

            XmlElement toDegreeNodeStart = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1, /*NXLT*/"DSCoreNodes.dll", 
                "Math.RadiansToDegrees", "Math.RadiansToDegrees@double");
            migrationData.AppendNode(toDegreeNodeStart);
            string toDegreeNodeStartId = MigrationManager.GetGuidFromXmlElement(toDegreeNodeStart);

            XmlElement toDegreeNodeEnd = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 2, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"Math.RadiansToDegrees", /*NXLT*/"Math.RadiansToDegrees@double");
            migrationData.AppendNode(toDegreeNodeEnd);
            string toDegreeNodeEndId = MigrationManager.GetGuidFromXmlElement(toDegreeNodeEnd);

            PortId oldInPort2 = new PortId(oldNodeId, 2, PortType.Input);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            PortId oldInPort3 = new PortId(oldNodeId, 3, PortType.Input);
            XmlElement connector3 = data.FindFirstConnector(oldInPort3);

            PortId toDegreeNodeStartPort = new PortId(toDegreeNodeStartId, 0, PortType.Input);
            PortId toDegreeNodeEndPort = new PortId(toDegreeNodeEndId, 0, PortType.Input);

            // Update connectors
            data.ReconnectToPort(connector2, toDegreeNodeStartPort);
            data.ReconnectToPort(connector3, toDegreeNodeEndPort);
            data.CreateConnector(toDegreeNodeStart, 0, newNode, 2);
            data.CreateConnector(toDegreeNodeEnd, 0, newNode, 3);
            data.CreateConnector(zAxisNode, 0, newNode, 4);

            return migrationData;
        }
    }

    public class BestFitArc : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"ProtoGeometry.dll", /*NXLT*/"Arc.ByBestFitThroughPoints", /*NXLT*/"Arc.ByBestFitThroughPoints@Point[]");
        }
    }
}
