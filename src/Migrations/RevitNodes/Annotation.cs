using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class ModelText : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            #region Migration Setup Steps

            NodeMigrationData migratedData = new NodeMigrationData(data.Document);

            // Legacy "ModelText" node takes in the following 6 inputs:
            // 
            //      0 - text (string)
            //      1 - position (XYZ)
            //      2 - normal (XYZ)
            //      3 - up (XYZ)
            //      4 - depth (double)
            //      5 - text type name (string)
            // 
            // The new "ModelText.ByTextSketchPlaneAndPosition" node takes in
            // the following inputs:
            // 
            //      0 - text (string)
            //      1 - sketchPlane (SketchPlane)
            //      2 - xCoordinateInPlane (double)
            //      3 - yCoordinateInPlane (double)
            //      4 - textDepth (double)
            //      5 - modelTextType (ModelTextType)
            // 
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            #endregion

            #region Create New Nodes...

            XmlElement dsModelText = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsModelText, "RevitNodes.dll", 
                "ModelText.ByTextSketchPlaneAndPosition", 
                "ModelText.ByTextSketchPlaneAndPosition@" + 
                "string,SketchPlane,double,double,double,ModelTextType");

            migratedData.AppendNode(dsModelText);
            string dsModelTextId = MigrationManager.GetGuidFromXmlElement(dsModelText);

            // Create a "Plane.ByOriginNormal" that takes a "Point" (origin) and 
            // a "Vector" (normal). This new node will convert both the "position" 
            // and "normal" to a "Plane".
            XmlElement plane = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "ProtoGeometry.dll", "Plane.ByOriginNormal",
                "Plane.ByOriginNormal@Point,Vector");

            migratedData.AppendNode(plane);
            string planeId = MigrationManager.GetGuidFromXmlElement(plane);

            // Create a "SketchPlane.ByPlane" node which converts a "Plane" 
            // into a "SketchPlane".
            XmlElement dsSketchPlane = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1, "RevitNodes.dll",
                "SketchPlane.ByPlane", "SketchPlane.ByPlane@Plane");

            migratedData.AppendNode(dsSketchPlane);
            string dsSketchPlaneId = MigrationManager.GetGuidFromXmlElement(dsSketchPlane);

            // Create a "ModelTextType.ByName" node that converts a "string"
            // into "ModelTextType" node.
            XmlElement dsModelTextType = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 2, "RevitNodes.dll",
                "ModelTextType.ByName", "ModelTextType.ByName@string");

            migratedData.AppendNode(dsModelTextType);
            string dsModelTextTypeId = MigrationManager.GetGuidFromXmlElement(dsModelTextType);

            //append asVector Node
            XmlElement pointAsVector0 = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 3, "ProtoGeometry.dll",
                "Point.AsVector", "Point.AsVector");
            migratedData.AppendNode(pointAsVector0);
            string pointAsVector0Id = MigrationManager.GetGuidFromXmlElement(pointAsVector0);

            //append number Node
            XmlElement numberNode = MigrationManager.CreateCodeBlockNodeModelNode(
                data.Document, oldNode, 4, "0;");
            migratedData.AppendNode(numberNode);

            #endregion

            #region Move Connectors Onto the New Nodes

            // Move connector for "text" over to the new node.
            PortId oldInPort = new PortId(oldNodeId, 0, PortType.Input);
            PortId newInPort = new PortId(dsModelTextId, 0, PortType.Input);
            XmlElement connector = data.FindFirstConnector(oldInPort);
            data.ReconnectToPort(connector, newInPort);

            // Move connector for "position" over to "Plane" node.
            oldInPort = new PortId(oldNodeId, 1, PortType.Input);
            newInPort = new PortId(planeId, 0, PortType.Input);
            connector = data.FindFirstConnector(oldInPort);
            data.ReconnectToPort(connector, newInPort);

            // Move connector for "normal" over to "Plane" node.
            oldInPort = new PortId(oldNodeId, 2, PortType.Input);
            newInPort = new PortId(pointAsVector0Id, 0, PortType.Input);
            connector = data.FindFirstConnector(oldInPort);
            data.ReconnectToPort(connector, newInPort);
            data.CreateConnector(pointAsVector0, 0, plane, 1);

            // Connect from "Plane" to "SketchPlane".
            data.CreateConnector(plane, 0, dsSketchPlane, 0);

            // Connect from "SketchPlane" to the new node.
            data.CreateConnector(dsSketchPlane, 0, dsModelText, 1);

            oldInPort = new PortId(oldNodeId, 3, PortType.Input);
            data.RemoveFirstConnector(oldInPort);

            // Move connector for "depth" over to the new node.
            oldInPort = new PortId(oldNodeId, 4, PortType.Input);
            newInPort = new PortId(dsModelTextId, 4, PortType.Input);
            connector = data.FindFirstConnector(oldInPort);
            data.ReconnectToPort(connector, newInPort);

            // Move connector for "text type name" over to "ModelTextType" node.
            oldInPort = new PortId(oldNodeId, 5, PortType.Input);
            newInPort = new PortId(dsModelTextTypeId, 0, PortType.Input);
            connector = data.FindFirstConnector(oldInPort);
            data.ReconnectToPort(connector, newInPort);

            // Connect from "ModelTextType" to the new node.
            data.CreateConnector(dsModelTextType, 0, dsModelText, 5);

            data.CreateConnector(numberNode, 0, dsModelText, 2);
            data.CreateConnector(numberNode, 0, dsModelText, 3);

            #endregion

            return migratedData;
        }
    }
}
