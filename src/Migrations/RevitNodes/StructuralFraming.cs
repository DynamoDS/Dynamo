using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class StructuralFramingSelector: MigrationNode
    {
    }

    public class StructuralFraming : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsRevitNode = MigrationManager.CreateFunctionNode(
                data.Document, "RevitNodes.dll",
                "StructuralFraming.ByCurveLevelUpVectorAndType",
                "StructuralFraming.ByCurveLevelUpVectorAndType@Curve,Level,Vector,StructuralType,FamilySymbol");

            migratedData.AppendNode(dsRevitNode);
            string dsRevitNodeId = MigrationManager.GetGuidFromXmlElement(dsRevitNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.INPUT);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId oldInPort2 = new PortId(oldNodeId, 2, PortType.INPUT);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            PortId newInPort0 = new PortId(dsRevitNodeId, 0, PortType.INPUT);
            //PortId newInPort1 = new PortId(dsRevitNodeId, 1, PortType.INPUT);
            PortId newInPort2 = new PortId(dsRevitNodeId, 2, PortType.INPUT);
            PortId newInPort3 = new PortId(dsRevitNodeId, 3, PortType.INPUT);
            //PortId newInPort4 = new PortId(dsRevitNodeId, 4, PortType.INPUT);

            data.ReconnectToPort(connector0, newInPort3);
            data.ReconnectToPort(connector1, newInPort0);
            data.ReconnectToPort(connector2, newInPort2);

            return migratedData;
        }
    }
}
