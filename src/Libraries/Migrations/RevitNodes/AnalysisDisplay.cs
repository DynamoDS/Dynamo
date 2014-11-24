using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    class SpatialFieldManager : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement dummyNode = MigrationManager.CreateDummyNode(oldNode, 1, 1);
            migrationData.AppendNode(dummyNode);

            return migrationData;
        }
    }

    class AnalysisResultsDisplayStyleColor : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement dummyNode = MigrationManager.CreateDummyNode(oldNode, 0, 1);
            migrationData.AppendNode(dummyNode);

            return migrationData;
        }
    }

    class SpatialFieldFace : MigrationNode 
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsRevitNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsRevitNode, "RevitNodes.dll",
                "FaceAnalysisDisplay.ByViewFacePointsAndValues",
                "FaceAnalysisDisplay.ByViewFacePointsAndValues@var,FaceReference,double[][],double[]");

            migratedData.AppendNode(dsRevitNode);
            string dsRevitNodeId = MigrationManager.GetGuidFromXmlElement(dsRevitNode);

            XmlElement documentNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "RevitNodes.dll",
                "Document.Current", "Document.Current");
            migratedData.AppendNode(documentNode);

            XmlElement activeViewNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1, "RevitNodes.dll",
                "Document.ActiveView", "Document.ActiveView");
            migratedData.AppendNode(activeViewNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.INPUT);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId oldInPort2 = new PortId(oldNodeId, 2, PortType.INPUT);
            
            PortId oldInPort3 = new PortId(oldNodeId, 3, PortType.INPUT);
            XmlElement connector3 = data.FindFirstConnector(oldInPort3);

            PortId newInPort1 = new PortId(dsRevitNodeId, 1, PortType.INPUT);
            PortId newInPort2 = new PortId(dsRevitNodeId, 2, PortType.INPUT);
            PortId newInPort3 = new PortId(dsRevitNodeId, 3, PortType.INPUT);

            data.ReconnectToPort(connector0, newInPort3);
            data.ReconnectToPort(connector1, newInPort2);
            data.ReconnectToPort(connector3, newInPort1);

            data.RemoveFirstConnector(oldInPort2);
            data.CreateConnector(documentNode, 0, activeViewNode, 0);
            data.CreateConnector(activeViewNode, 0, dsRevitNode, 0);

            return migratedData;
        }
    }

    class SpatialFieldPoints : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsRevitNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsRevitNode, "RevitNodes.dll",
                "PointAnalysisDisplay.ByViewPointsAndValues",
                "PointAnalysisDisplay.ByViewPointsAndValues@var,Point[],double[]");

            XmlElement documentNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "RevitNodes.dll",
                "Document.Current", "Document.Current");
            migratedData.AppendNode(documentNode);

            XmlElement activeViewNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1, "RevitNodes.dll",
                "Document.ActiveView", "Document.ActiveView");
            migratedData.AppendNode(activeViewNode);

            migratedData.AppendNode(dsRevitNode);
            string dsRevitNodeId = MigrationManager.GetGuidFromXmlElement(dsRevitNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.INPUT);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId oldInPort2 = new PortId(oldNodeId, 2, PortType.INPUT);
            
            PortId newInPort1 = new PortId(dsRevitNodeId, 1, PortType.INPUT);
            PortId newInPort2 = new PortId(dsRevitNodeId, 2, PortType.INPUT);

            data.ReconnectToPort(connector0, newInPort2);
            data.ReconnectToPort(connector1, newInPort1);

            data.RemoveFirstConnector(oldInPort2);
            data.CreateConnector(documentNode, 0, activeViewNode, 0);
            data.CreateConnector(activeViewNode, 0, dsRevitNode, 0);

            return migratedData;
        }
    }

    class SpatialFieldVectors : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsRevitNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsRevitNode, "RevitNodes.dll",
                "VectorAnalysisDisplay.ByViewPointsAndVectorValues", 
                "VectorAnalysisDisplay.ByViewPointsAndVectorValues@var,Point[],Vector[]");

            migratedData.AppendNode(dsRevitNode);
            string dsRevitNodeId = MigrationManager.GetGuidFromXmlElement(dsRevitNode);

            XmlElement documentNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "RevitNodes.dll",
                "Document.Current", "Document.Current");
            migratedData.AppendNode(documentNode);

            XmlElement activeViewNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1, "RevitNodes.dll",
                "Document.ActiveView", "Document.ActiveView");
            migratedData.AppendNode(activeViewNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.INPUT);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId oldInPort2 = new PortId(oldNodeId, 2, PortType.INPUT);
            
            PortId newInPort1 = new PortId(dsRevitNodeId, 1, PortType.INPUT);
            PortId newInPort2 = new PortId(dsRevitNodeId, 2, PortType.INPUT);

            data.ReconnectToPort(connector0, newInPort2);
            data.ReconnectToPort(connector1, newInPort1);
            
            data.RemoveFirstConnector(oldInPort2);
            data.CreateConnector(documentNode, 0, activeViewNode, 0);
            data.CreateConnector(activeViewNode, 0, dsRevitNode, 0);

            return migratedData;
        }
    }

    class SpatialFieldCurve : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement dummyNode = MigrationManager.CreateDummyNode(oldNode, 3, 1);
            migrationData.AppendNode(dummyNode);

            return migrationData;
        }
    }
}
