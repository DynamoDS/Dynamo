using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class FamilyInstanceParameterSelector : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(
                oldNode, /*NXLT*/"DSRevitNodesUI.FamilyInstanceParameters", "Get Family Parameter");
            migrationData.AppendNode(newNode);
            
            foreach (XmlElement subNode in oldNode.ChildNodes)
                newNode.AppendChild(subNode.Clone());

            return migrationData;
        }
    }

    public class FamilyInstanceCreatorXyz : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsRevitNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsRevitNode,/*NXLT*/"RevitNodes.dll",
                /*NXLT*/"FamilyInstance.ByPoint", /*NXLT*/"FamilyInstance.ByPoint@FamilySymbol,Point");

            migratedData.AppendNode(dsRevitNode);
            string dsRevitNodeId = MigrationManager.GetGuidFromXmlElement(dsRevitNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId newInPort0 = new PortId(dsRevitNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(dsRevitNodeId, 1, PortType.Input);

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, newInPort0);

            return migratedData;
        }
    }

    public class FamilyInstanceCreatorLevel : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsRevitNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
             MigrationManager.SetFunctionSignature(dsRevitNode,/*NXLT*/"RevitNodes.dll",
                 /*NXLT*/"FamilyInstance.ByPointAndLevel",
                 /*NXLT*/"FamilyInstance.ByPointAndLevel@FamilySymbol,Point,Level");

            migratedData.AppendNode(dsRevitNode);
            string dsRevitNodeId = MigrationManager.GetGuidFromXmlElement(dsRevitNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId oldInPort2 = new PortId(oldNodeId, 2, PortType.Input);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            PortId newInPort0 = new PortId(dsRevitNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(dsRevitNodeId, 1, PortType.Input);
            PortId newInPort2 = new PortId(dsRevitNodeId, 2, PortType.Input);

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, newInPort0);
            data.ReconnectToPort(connector2, newInPort2);

            return migratedData;
        }
    }

    public class CurvesFromFamilyInstance : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"RevitNodes.dll",
                /*NXLT*/"Element.Curves", /*NXLT*/"Element.Curves");
        }
    }

    public class FamilyInstanceParameterSetter : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"RevitNodes.dll", /*NXLT*/"Element.SetParameterByName", /*NXLT*/"Element.SetParameterByName@string,object");
        }
    }

    public class FamilyInstanceParameterGetter : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"RevitNodes.dll",
                /*NXLT*/"Element.GetParameterValueByName", /*NXLT*/"Element.GetParameterValueByName@string");
        }
    }

    public class GetFamilyInstanceLocation : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"RevitNodes.dll",
                /*NXLT*/"FamilyInstance.Location", /*NXLT*/"FamilyInstance.Location");
        }
    }

    public class GetParameters : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement codeBlockNode = MigrationManager.CreateCodeBlockNodeFrom(oldNode);

            // Create CBN based on the input/output port count
            string codeText = ""; int num = 1;
            foreach (XmlElement childNode in oldNode.ChildNodes)
            {
                if (childNode.Name == /*NXLT*/"Output")
                {
                    codeText += /*NXLT*/"element.GetParameterValueByName(param" + num++ + /*NXLT*/");";
                    if (num > 1)
                        codeText += /*NXLT*/"\n";
                }
            }

            // To avoid empty CBN
            if (codeText == "")
                codeText = /*NXLT*/"element;";

            codeBlockNode.SetAttribute(/*NXLT*/"CodeText", codeText);
            codeBlockNode.SetAttribute(/*NXLT*/"nickname", "Get Parameters");

            migrationData.AppendNode(codeBlockNode);
            return migrationData;
        }
    }

}


