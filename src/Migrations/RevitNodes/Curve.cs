using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class CurveTransformed : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Geometry.Transform", "Geometry.Transform@CoordinateSystem,CoordinateSystem");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new node
            XmlElement identityCoordinateSystem = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "ProtoGeometry.dll",
                "CoordinateSystem.Identity",
                "CoordinateSystem.Identity");
            migrationData.AppendNode(identityCoordinateSystem);

            // Update connectors
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.INPUT);
            PortId newInPort2 = new PortId(newNodeId, 2, PortType.INPUT);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);
            
            data.ReconnectToPort(connector1, newInPort2);
            data.CreateConnector(identityCoordinateSystem, 0, newNode, 1);

            return migrationData;
        }
    }

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
                "Geometry.GetClosestPoint", "Geometry.GetClosestPoint@Geometry");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            var oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            var connector0 = data.FindFirstConnector(oldInPort0);
            var oldInPort1 = new PortId(newNodeId, 1, PortType.INPUT);
            var connector1 = data.FindFirstConnector(oldInPort1);

            data.ReconnectToPort(connector0, oldInPort0);
            data.ReconnectToPort(connector1, oldInPort1);

            if (connector0 != null)
            {
                // Get the original output ports connected to input
                var ptInputNodeId = connector0.Attributes["start"].Value;
                var ptInputIndex = int.Parse(connector0.Attributes["start_index"].Value);

                // make distance to node
                var distTo = MigrationManager.CreateFunctionNode(
                    data.Document, oldNode, 0, "ProtoGeometry.dll",
                    "Geometry.DistanceTo",
                    "Geometry.DistanceTo@Geometry");
                migrationData.AppendNode(distTo);
                var distToId = MigrationManager.GetGuidFromXmlElement(distTo);

                data.CreateConnector(newNode, 0, distTo, 0);
                data.CreateConnectorFromId(ptInputNodeId, ptInputIndex, distToId, 1);

                var oldDOut = new PortId(newNodeId, 2, PortType.OUTPUT);
                var newDOut = new PortId(distToId, 0, PortType.OUTPUT);
                var oldDConnectors = data.FindConnectors(oldDOut);
                oldDConnectors.ToList().ForEach(x => data.ReconnectToPort(x, newDOut));
            }

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
                var oldTOut = new PortId(newNodeId, 1, PortType.OUTPUT);

                var oldTConnectors = data.FindConnectors(oldTOut);

                oldTConnectors.ToList().ForEach(x => data.ReconnectToPort(x, newTOut));
            }

            return migrationData;
        }
    }

    public class CurvesThroughPoints : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll",
                "PolyCurve.ByPoints", "PolyCurve.ByPoints@Point[],bool");
        }
    }

    public class CurveByPoints : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "CurveByPoints.ByReferencePoints", "CurveByPoints.ByReferencePoints@ReferencePoint,bool");
        }
    }

    public class CurveByPointsByLine : MigrationNode
    {
        // Deprecated
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

    public class CurveRef : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "ModelCurve.ReferenceCurveByCurve", "ModelCurve.ReferenceCurveByCurve@Curve");
        }
    }

    public class CurveFromModelCurve : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "CurveElement.Curve", "CurveElement.Curve");
        }
    }
     
    public class CurveLoop : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "PolyCurve.ByJoinedCurves",
                "PolyCurve.ByJoinedCurves@Curve[]");
        }
    }

    public class ThickenCurveLoop : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "PolyCurve.OffsetPlanarPolyCurve", 
                "PolyCurve.OffsetPlanarPolyCurve@double,bool,bool");
        }
    }

    public class ListCurveLoop : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "PolyCurve.Curves", "PolyCurve.Curves");
        }
    }

    public class OffsetCrv : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Curve.Offset", "Curve.Offset@double");
        }
    }

    public class BoundCurve : MigrationNode
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

    public class ComputeCurveDerivatives : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Curve.CoordinateSystemAtParameter",
                "Curve.CoordinateSystemAtParameter@double");
        }
    }

    public class TangentTransformOnCurveOrEdge : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Curve.CoordinateSystemAtParameter", "Curve.CoordinateSystemAtParameter@double");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);
            data.ReconnectToPort(connector0, oldInPort1);
            data.ReconnectToPort(connector1, oldInPort0);

            return migrationData;
        }
    }

    public class ApproximateByTangentArcs : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Curve.ApproximateWithArcAndLineSegments",
                "Curve.ApproximateWithArcAndLineSegments");
        }
    }

    public class CurveDomain : MigrationNode
    {
    }

    public class CurveLength : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Curve.Length",
                "Curve.Length");
        }
    }
}

