using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class CurveFaceIntersection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "Face.Intersect", "Face.Intersect@Curve");
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

            var oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            var connector0 = data.FindFirstConnector(oldInPort0);
            var oldInPort1 = new PortId(newNodeId, 1, PortType.INPUT);
            var connector1 = data.FindFirstConnector(oldInPort1);

            data.ReconnectToPort(connector0, oldInPort0);
            data.ReconnectToPort(connector1, oldInPort1);

            // reconnect output port at 1 to 0
            var oldXYZOut = new PortId(newNodeId, 1, PortType.OUTPUT);
            var newXyzOut = new PortId(newNodeId, 0, PortType.OUTPUT);
            var xyzConnects = data.FindConnectors(oldXYZOut);

            xyzConnects.ToList().ForEach(x => data.ReconnectToPort(x, newXyzOut));

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
                var newTOut = new PortId(parmAtPtId, 0, PortType.OUTPUT);
                var oldTOut = new PortId(newNodeId, 2, PortType.OUTPUT);

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
                var newTOut = new PortId(parmAtPtId, 0, PortType.OUTPUT);
                var oldTOut = new PortId(newNodeId, 3, PortType.OUTPUT);

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
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "Face.Intersect", "Face.Intersect@Face");
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
