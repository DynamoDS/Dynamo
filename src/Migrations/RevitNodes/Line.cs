using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

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
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Line.ByStartPointDirectionLength",
                "Line.ByStartPointDirectionLength@Point,Vector,double");
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
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.INPUT);
            PortId newInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            data.ReconnectToPort(connector1, newInPort0);
            
            if (connector1 != null)
            {
                // Create new node only when the old node is connected to a normal vector
                XmlElement translateNode = MigrationManager.CreateFunctionNode(
                    data.Document, "ProtoGeometry.dll", "Geometry.Translate",
                    "Geometry.Translate@Autodesk.DesignScript.Geometry.Vector");
                migrationData.AppendNode(translateNode);
                string translateNodeId = MigrationManager.GetGuidFromXmlElement(translateNode);

                // Update connectors
                PortId newInPortTranslate1 = new PortId(translateNodeId, 1, PortType.INPUT);
                
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
    }

}
