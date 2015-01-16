using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class DraftingView: MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"RevitNodes.dll", /*NXLT*/"DraftingView.ByName", /*NXLT*/"DraftingView.ByName@string");
        }
    }

    public abstract class ViewBase:MigrationNode
    {
    }

    public class IsometricView : ViewBase
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
                /*NXLT*/"AxonometricView.ByEyePointAndTarget",
                /*NXLT*/"AxonometricView.ByEyePointAndTarget@Point,Point,var,string,bool");

            migratedData.AppendNode(dsRevitNode);
            string dsRevitNodeId = MigrationManager.GetGuidFromXmlElement(dsRevitNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId oldInPort2 = new PortId(oldNodeId, 2, PortType.Input);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            PortId oldInPort3 = new PortId(oldNodeId, 3, PortType.Input);
            XmlElement connector3 = data.FindFirstConnector(oldInPort3);

            PortId oldInPort4 = new PortId(oldNodeId, 4, PortType.Input);
            XmlElement connector4 = data.FindFirstConnector(oldInPort4);

            PortId newInPort0 = new PortId(dsRevitNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(dsRevitNodeId, 1, PortType.Input);
            PortId newInPort2 = new PortId(dsRevitNodeId, 2, PortType.Input);
            PortId newInPort3 = new PortId(dsRevitNodeId, 3, PortType.Input);
            PortId newInPort4 = new PortId(dsRevitNodeId, 4, PortType.Input);

            data.ReconnectToPort(connector0, newInPort0);
            data.ReconnectToPort(connector1, newInPort1);
            data.ReconnectToPort(connector2, newInPort3);
            data.ReconnectToPort(connector3, newInPort2);
            data.ReconnectToPort(connector4, newInPort4);

            return migratedData;
        }
    }

    public class PerspectiveView : ViewBase
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
                /*NXLT*/"PerspectiveView.ByEyePointAndTarget",
                /*NXLT*/"PerspectiveView.ByEyePointAndTarget@Point,Point,var,string,bool");

            migratedData.AppendNode(dsRevitNode);
            string dsRevitNodeId = MigrationManager.GetGuidFromXmlElement(dsRevitNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId oldInPort2 = new PortId(oldNodeId, 2, PortType.Input);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            PortId oldInPort3 = new PortId(oldNodeId, 3, PortType.Input);
            XmlElement connector3 = data.FindFirstConnector(oldInPort3);

            PortId oldInPort4 = new PortId(oldNodeId, 4, PortType.Input);
            XmlElement connector4 = data.FindFirstConnector(oldInPort4);

            PortId newInPort0 = new PortId(dsRevitNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(dsRevitNodeId, 1, PortType.Input);
            PortId newInPort2 = new PortId(dsRevitNodeId, 2, PortType.Input);
            PortId newInPort3 = new PortId(dsRevitNodeId, 3, PortType.Input);
            PortId newInPort4 = new PortId(dsRevitNodeId, 4, PortType.Input);

            data.ReconnectToPort(connector0, newInPort0);
            data.ReconnectToPort(connector1, newInPort1);
            data.ReconnectToPort(connector2, newInPort3);
            data.ReconnectToPort(connector3, newInPort2);
            data.ReconnectToPort(connector4, newInPort4);

            return migratedData;
        }
    }

    public class BoundingBoxXyz : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            var migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var newNode = MigrationManager.CreateCustomNodeFrom(oldNode.OwnerDocument, oldNode,
                /*NXLT*/"1e10c22b-18f6-452c-8220-4b1407c80b7b",
                /*NXLT*/"BoundingBoxXYZ",
                "This node represents an upgrade of the 0.6.3 BoundingBoxXYZ node to 0.7.x",
                new List<string>() {"coordinate system", "width", "length", "height"},
                new List<string>() {"bounding box"});

            migratedData.AppendNode(newNode);
            return migratedData;
        }
    }

    public class SectionView : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"RevitNodes.dll",
                /*NXLT*/"SectionView.ByBoundingBox", /*NXLT*/"SectionView.ByBoundingBox@BoundingBox");
        }
    }

    public class ActiveRevitView : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"RevitNodes.dll",
                /*NXLT*/"Document.ActiveView", /*NXLT*/"Document.ActiveView");
        }
    }

    public class SaveImageFromRevitView : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"RevitNodes.dll",
                /*NXLT*/"View.ExportAsImage", /*NXLT*/"View.ExportAsImage@string");
        }
    }

    public class WatchImage : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(MigrationManager.CloneAndChangeName(
                data.MigratedNodes.ElementAt(0), /*NXLT*/"Dynamo.Nodes.WatchImageCore", "Watch Image"));

            return migrationData;
        }
    }

    public class ViewSheet : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"RevitNodes.dll",
                /*NXLT*/"Sheet.ByNameNumberTitleBlockAndViews",
                /*NXLT*/"Sheet.ByNameNumberTitleBlockAndViews@string,string,FamilySymbol,var[]");
        }
    }

    public class OverrideColorInView : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsRevitNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsRevitNode,/*NXLT*/"RevitNodes.dll", /*NXLT*/"Element.OverrideColorInView", /*NXLT*/"Element.OverrideColorInView@Color");

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
}
