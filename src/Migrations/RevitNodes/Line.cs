using System.Linq;
using System.Xml;
using Dynamo.Models;
using Dynamo.Migration;

namespace Dynamo.Nodes
{
    public class LineBound : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Line.ByStartPointEndPoint",
                "Line.ByStartPointEndPoint@Point,Point");
        }
    }

    public class LineByStartPtDirLength : MigrationNode
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
                "Line.ByStartPointDirectionLength",
                "Line.ByStartPointDirectionLength@Point,Vector,double");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            //append asVector Node
            XmlElement pointAsVector0 = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1, "ProtoGeometry.dll",
                "Point.AsVector", "Point.AsVector");
            migrationData.AppendNode(pointAsVector0);
            string pointAsVector0Id = MigrationManager.GetGuidFromXmlElement(pointAsVector0);

            PortId pToV0 = new PortId(pointAsVector0Id, 0, PortType.Input);
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.Input);

            XmlElement connector1 = data.FindFirstConnector(oldInPort1);
            data.ReconnectToPort(connector1, pToV0);
            data.CreateConnector(pointAsVector0, 0, newNode, 1); 

            return migrationData;
        }
    }

    public class LineVectorfromXyz : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Line.ByStartPointEndPoint", "Line.ByStartPointEndPoint@Point,Point");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.Input);
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.Input);
            PortId newInPort0 = new PortId(newNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            data.ReconnectToPort(connector1, newInPort0);
            
            if (connector1 != null)
            {
                // Create new node only when the old node is connected to a normal vector
                XmlElement translateNode = MigrationManager.CreateFunctionNode(
                    data.Document, oldNode, 0, "ProtoGeometry.dll", "Geometry.Translate",
                    "Geometry.Translate@Autodesk.DesignScript.Geometry.Vector");
                migrationData.AppendNode(translateNode);
                string translateNodeId = MigrationManager.GetGuidFromXmlElement(translateNode);

                // Update connectors
                PortId newInPortTranslate1 = new PortId(translateNodeId, 1, PortType.Input);
                
                string nodeOriginId = connector1.GetAttribute("start").ToString();
                data.CreateConnector(translateNode, 0, newNode, 1);
                data.CreateConnectorFromId(nodeOriginId, 0, translateNodeId, 0);
                data.ReconnectToPort(connector0, newInPortTranslate1);
            }
            
            return migrationData;
        }
    }

    public class Bisector : MigrationNode
    {
    }

    internal class BestFitLine : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);
            
            if (data.FindFirstConnector(new PortId(oldNodeId, 1, PortType.Output)) != null)
            {
                // If the second output port is utilized, migrate to CBN
                XmlElement codeBlockNode = MigrationManager.CreateCodeBlockNodeFrom(oldNode);
                codeBlockNode.SetAttribute("CodeText",
                    "Line.ByBestFitThroughPoints(XYZs)\n" +
                    ".Direction.Normalized().AsPoint();\n" +
                    "Point.ByCoordinates(Math.Average(XYZs.X),\n" +
                    "Math.Average(XYZs.Y), Math.Average(XYZs.Z));");
                codeBlockNode.SetAttribute("nickname", "Best Fit Line");
                migrationData.AppendNode(codeBlockNode);
            }
            else
            {
                // When only the first output port is utilized, migrate to a chain of nodes

                var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
                MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                    "Vector.AsPoint", "Vector.AsPoint");

                var lineNode = MigrationManager.CreateFunctionNode(
                    data.Document, oldNode, 0, "ProtoGeometry.dll",
                    "Line.ByBestFitThroughPoints",
                    "Line.ByBestFitThroughPoints@Point[]");
                string lineNodeId = MigrationManager.GetGuidFromXmlElement(lineNode);
                
                var directionNode = MigrationManager.CreateFunctionNode(
                    data.Document, oldNode, 1, "ProtoGeometry.dll",
                    "Line.Direction", "Line.Direction");

                var normalizedNode = MigrationManager.CreateFunctionNode(
                    data.Document, oldNode, 2, "ProtoGeometry.dll",
                    "Vector.Normalized", "Vector.Normalized");

                migrationData.AppendNode(newNode);
                migrationData.AppendNode(lineNode);
                migrationData.AppendNode(directionNode);
                migrationData.AppendNode(normalizedNode);

                // Update connectors
                PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
                PortId lineInPort0 = new PortId(lineNodeId, 0, PortType.Input);
                XmlElement connector0 = data.FindFirstConnector(oldInPort0);

                data.ReconnectToPort(connector0, lineInPort0);
                data.CreateConnector(lineNode, 0, directionNode, 0);
                data.CreateConnector(directionNode, 0, normalizedNode, 0);
                data.CreateConnector(normalizedNode, 0, newNode, 0);
            }

            return migrationData;
        }
    }

}
