using System.Linq;
using System.Xml;
using Dynamo.Models;
using Dynamo.Migration;

namespace Dynamo.Nodes
{
    public class CurveFaceIntersection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);

            #region Create DSFunction node

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Geometry.Intersect", "Geometry.Intersect@Geometry");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            var oldInPort0 = new PortId(newNodeId, 0, PortType.Input);
            var faceInConnector = data.FindFirstConnector(oldInPort0);
            var oldInPort1 = new PortId(newNodeId, 1, PortType.Input);
            var crvInConnector = data.FindFirstConnector(oldInPort1);

            // probably unnecessary
            data.ReconnectToPort(faceInConnector, oldInPort0);
            data.ReconnectToPort(crvInConnector, oldInPort1);

            #endregion

            // in ports of curve-face intersection

                // 1) crv   -> stays the same
                // 2) face  -> stays the same

            // out ports of curve-face intersection

                // 1) result    -> this will be killed off by the migration
                // 2) xyz       -> this is out port 1 of oldNode
                // 3) uv        -> use Surface.ParameterAtPoint
                // 4) t         -> use Curve.ParameterAtPoint
                // 5) edge      -> killed
                // 6) edge t    -> killed


            // reconnect output port at 1 to 0
            var oldXYZOut = new PortId(newNodeId, 1, PortType.Output);
            var newXyzOut = new PortId(newNodeId, 0, PortType.Output);
            var xyzConnects = data.FindConnectors(oldXYZOut);

            if (xyzConnects != null)
            {
                xyzConnects.ToList().ForEach(x => data.ReconnectToPort(x, newXyzOut));
            }

            // get uv parm
            if (faceInConnector != null)
            {
                var faceInputNodeId = faceInConnector.Attributes["start"].Value;
                var faceInputIndex = int.Parse(faceInConnector.Attributes["start_index"].Value);

                // get the parameter as a vector
                var parmAtPt = MigrationManager.CreateFunctionNode(
                    data.Document, oldNode, 0, "ProtoGeometry.dll",
                    "Surface.UVParameterAtPoint",
                    "Surface.UVParameterAtPoint@Point");
                migrationData.AppendNode(parmAtPt);
                var parmAtPtId = MigrationManager.GetGuidFromXmlElement(parmAtPt);

                // connect output of project to parm at pt
                data.CreateConnectorFromId(faceInputNodeId, faceInputIndex, parmAtPtId, 0);
                data.CreateConnector(newNode, 0, parmAtPt, 1);

                // reconnect remaining output ports to new nodes
                var newUVOut = new PortId(parmAtPtId, 0, PortType.Output);
                var oldUVOut = new PortId(newNodeId, 2, PortType.Output);

                var oldUVConnectors = data.FindConnectors(oldUVOut);
                if (oldUVConnectors != null)
                    oldUVConnectors.ToList().ForEach(x => data.ReconnectToPort(x, newUVOut));
            }

            // get v parm
            if (crvInConnector != null)
            {
                var crvInputNodeId = crvInConnector.Attributes["start"].Value;
                var crvInputIndex = int.Parse(crvInConnector.Attributes["start_index"].Value);

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
                var newTOut = new PortId(parmAtPtId, 0, PortType.Output);
                var oldTOut = new PortId(newNodeId, 3, PortType.Output);

                var oldTConnectors = data.FindConnectors(oldTOut);
                if (oldTConnectors != null)
                    oldTConnectors.ToList().ForEach(x => data.ReconnectToPort(x, newTOut));
            }

            return migrationData;
        }
    }

    public class CurveCurveIntersection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Geometry.Intersect", "Geometry.Intersect@Geometry");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            var oldInPort0 = new PortId(newNodeId, 0, PortType.Input);
            var connector0 = data.FindFirstConnector(oldInPort0);
            var oldInPort1 = new PortId(newNodeId, 1, PortType.Input);
            var connector1 = data.FindFirstConnector(oldInPort1);

            data.ReconnectToPort(connector0, oldInPort0);
            data.ReconnectToPort(connector1, oldInPort1);

            // reconnect output port at 1 to 0
            var oldXYZOut = new PortId(newNodeId, 1, PortType.Output);
            var newXyzOut = new PortId(newNodeId, 0, PortType.Output);
            var xyzConnects = data.FindConnectors(oldXYZOut);

            if (xyzConnects != null)
            {
                xyzConnects.ToList().ForEach(x => data.ReconnectToPort(x, newXyzOut));
            }

            // get u parm
            if (connector0 != null)
            {
                var crvInputNodeId = connector0.Attributes["start"].Value;
                var crvInputIndex = int.Parse(connector0.Attributes["start_index"].Value);

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
                var newTOut = new PortId(parmAtPtId, 0, PortType.Output);
                var oldTOut = new PortId(newNodeId, 2, PortType.Output);

                var oldTConnectors = data.FindConnectors(oldTOut);

                oldTConnectors.ToList().ForEach(x => data.ReconnectToPort(x, newTOut));
            }

            // get v parm
            if (connector1 != null)
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
                var newTOut = new PortId(parmAtPtId, 0, PortType.Output);
                var oldTOut = new PortId(newNodeId, 3, PortType.Output);

                var oldTConnectors = data.FindConnectors(oldTOut);

                oldTConnectors.ToList().ForEach(x => data.ReconnectToPort(x, newTOut));
            }

            return migrationData;
        }
    }

    public class FaceFaceIntersection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll",
                "Geometry.Intersect", "Geometry.Intersect@Geometry");
        }
    }

    public class CurvePlaneIntersection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll",
                "Geometry.Intersect", "Geometry.Intersect@Geometry");
        }
    }
}
