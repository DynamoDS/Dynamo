using System;
using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class ProjectPointOnCurve : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Geometry.ClosestPointTo", "Geometry.ClosestPointTo@Geometry");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            var oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            var connector0 = data.FindFirstConnector(oldInPort0);
            var oldInPort1 = new PortId(newNodeId, 1, PortType.INPUT);
            var connector1 = data.FindFirstConnector(oldInPort1);

            data.ReconnectToPort(connector0, oldInPort0);
            data.ReconnectToPort(connector1, oldInPort1);

            var oldDOut = new PortId(newNodeId, 2, PortType.OUTPUT);
            var oldTOut = new PortId(newNodeId, 1, PortType.OUTPUT);

            if ((connector0 != null) && (data.FindConnectors(oldDOut) != null))
            {
                // Get the original output ports connected to input
                var ptInputNodeId = connector0.Attributes["start"].Value;
                var ptInputIndex = int.Parse(connector0.Attributes["start_index"].Value);

                // make distance to node
                var distTo = MigrationManager.CreateFunctionNode(
                    data.Document, oldNode, 1, "ProtoGeometry.dll",
                    "Geometry.DistanceTo",
                    "Geometry.DistanceTo@Geometry");
                migrationData.AppendNode(distTo);
                var distToId = MigrationManager.GetGuidFromXmlElement(distTo);

                data.CreateConnector(newNode, 0, distTo, 0);
                data.CreateConnectorFromId(ptInputNodeId, ptInputIndex, distToId, 1);

                var newDOut = new PortId(distToId, 0, PortType.OUTPUT);
                var oldDConnectors = data.FindConnectors(oldDOut);

                if (oldDConnectors != null)
                    oldDConnectors.ToList().ForEach(x => data.ReconnectToPort(x, newDOut));
            }

            if ((connector1 != null) && (data.FindConnectors(oldTOut) != null))
            {
                var crvInputNodeId = connector1.Attributes["start"].Value;
                var crvInputIndex = int.Parse(connector1.Attributes["start_index"].Value);

                // make parm at point node 
                var parmAtPt = MigrationManager.CreateFunctionNode(
                    data.Document, oldNode, 0, "ProtoGeometry.dll",
                    "Curve.ParameterAtPoint",
                    "Curve.ParameterAtPoint@Point");
                migrationData.AppendNode(parmAtPt);
                var parmAtPtId = MigrationManager.GetGuidFromXmlElement(parmAtPt);

                // connect output of project to parm at pt
                data.CreateConnectorFromId(crvInputNodeId, crvInputIndex, parmAtPtId, 0);
                data.CreateConnector(newNode, 0, parmAtPt, 1);

                // reconnect remaining output ports to new nodes
                var newTOut = new PortId(parmAtPtId, 0, PortType.OUTPUT);
                var oldTConnectors = data.FindConnectors(oldTOut);

                if (oldTConnectors != null)
                    oldTConnectors.ToList().ForEach(x => data.ReconnectToPort(x, newTOut));
            }

            return migrationData;
        }
    }

    public class ProjectPointOnFace : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            #region Create new DSFunction node - Geometry.GetClosestPoint@Geometry

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Geometry.GetClosestPoint", "Geometry.GetClosestPoint@Geometry");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            var oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            var ptInConnector = data.FindFirstConnector(oldInPort0);
            var oldInPort1 = new PortId(newNodeId, 1, PortType.INPUT);
            var faceInConnector = data.FindFirstConnector(oldInPort1);

            data.ReconnectToPort(ptInConnector, oldInPort1);
            data.ReconnectToPort(faceInConnector, oldInPort0);

            #endregion

            #region Reconnect the old UV out port

            // if necessary, get the face UV
            var oldUVOut = new PortId(newNodeId, 1, PortType.OUTPUT);
            var oldUVConnectors = data.FindConnectors(oldUVOut);

            if (oldUVConnectors != null && oldUVConnectors.Any())
            {
                // make parm at point node 
                var parmAtPt = MigrationManager.CreateFunctionNode(
                    data.Document, oldNode, 0, "ProtoGeometry.dll",
                    "Surface.UVParameterAtPoint",
                    "Surface.UVParameterAtPoint@Point");
                migrationData.AppendNode(parmAtPt);
                var parmAtPtId = MigrationManager.GetGuidFromXmlElement(parmAtPt);

                var crvInputNodeId = faceInConnector.Attributes["start"].Value;
                var crvInputIndex = int.Parse(faceInConnector.Attributes["start_index"].Value);

                // connect output of project to parm at pt
                data.CreateConnectorFromId(crvInputNodeId, crvInputIndex, parmAtPtId, 0);
                data.CreateConnector(newNode, 0, parmAtPt, 1);

                // reconnect remaining output ports to new nodes
                var newTOut = new PortId(parmAtPtId, 0, PortType.OUTPUT);
                oldUVConnectors.ToList().ForEach(x => data.ReconnectToPort(x, newTOut));
            }
            #endregion

            #region Reconnect the old distance out port

            var oldDOut = new PortId(newNodeId, 2, PortType.OUTPUT);
            var oldDConnectors = data.FindConnectors(oldDOut);

            // If necessary, get the distance to the projected point
            if (oldDConnectors != null && oldDConnectors.Any())
            {
                // Get the original output ports connected to input
                var ptInputNodeId = ptInConnector.Attributes["start"].Value;
                var ptInputIndex = int.Parse(ptInConnector.Attributes["start_index"].Value);

                // make distance to node
                var distTo = MigrationManager.CreateFunctionNode(
                    data.Document, oldNode, 0, "ProtoGeometry.dll",
                    "Geometry.DistanceTo",
                    "Geometry.DistanceTo@Geometry");
                migrationData.AppendNode(distTo);
                var distToId = MigrationManager.GetGuidFromXmlElement(distTo);

                data.CreateConnector(newNode, 0, distTo, 0);
                data.CreateConnectorFromId(ptInputNodeId, ptInputIndex, distToId, 1);

                var newDOut = new PortId(distToId, 0, PortType.OUTPUT);
                oldDConnectors.ToList().ForEach(x => data.ReconnectToPort(x, newDOut));
            }

            #endregion

            return migrationData;
        }


    }
}
