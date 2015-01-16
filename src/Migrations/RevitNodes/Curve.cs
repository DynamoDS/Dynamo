using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class CurveTransformed : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Geometry.Transform", /*NXLT*/"Geometry.Transform@CoordinateSystem,CoordinateSystem");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new node
            XmlElement identityCoordinateSystem = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"CoordinateSystem.Identity",
                /*NXLT*/"CoordinateSystem.Identity");
            migrationData.AppendNode(identityCoordinateSystem);

            // Update connectors
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.Input);
            PortId newInPort2 = new PortId(newNodeId, 2, PortType.Input);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);
            
            data.ReconnectToPort(connector1, newInPort2);
            data.CreateConnector(identityCoordinateSystem, 0, newNode, 1);

            return migrationData;
        }
    }

    
    public class CurvesThroughPoints : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Line.ByStartPointEndPoint", /*NXLT*/"Line.ByStartPointEndPoint@Point,Point");
            newNode.SetAttribute(/*NXLT*/"lacing", "Shortest");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new nodes
            XmlElement reverse = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"List.Reverse",
                /*NXLT*/"List.Reverse@var[]..[]");
            migrationData.AppendNode(reverse);
            string reverseId = MigrationManager.GetGuidFromXmlElement(reverse);

            XmlElement rest = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1,/*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"List.RestOfItems",
                /*NXLT*/"List.RestOfItems@var[]..[]");
            migrationData.AppendNode(rest);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            PortId reverseInPort0 = new PortId(reverseId, 0, PortType.Input);
            
            data.ReconnectToPort(connector0, reverseInPort0);
            data.CreateConnector(reverse, 0, newNode, 0);
            data.CreateConnector(reverse, 0, rest, 0);
            data.CreateConnector(rest, 0, newNode, 1);
            
            return migrationData;
        }
    }

    public class CurveByPoints : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"RevitNodes.dll",
                /*NXLT*/"CurveByPoints.ByReferencePoints", /*NXLT*/"CurveByPoints.ByReferencePoints@ReferencePoint,bool");
        }
    }

    public class CurveByPointsByLine : MigrationNode
    {
        // Deprecated
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"RevitNodes.dll",
                /*NXLT*/"ModelCurve.ByCurve", /*NXLT*/"ModelCurve.ByCurve@Autodesk.DesignScript.Geometry.Curve");
        }
    }

    public class CurveRef : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"RevitNodes.dll",
                /*NXLT*/"ModelCurve.ReferenceCurveByCurve", /*NXLT*/"ModelCurve.ReferenceCurveByCurve@Curve");
        }
    }

    public class CurveFromModelCurve : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"RevitNodes.dll",
                /*NXLT*/"CurveElement.Curve", /*NXLT*/"CurveElement.Curve");
        }
    }
     
    public class CurveLoop : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"ProtoGeometry.dll", /*NXLT*/"PolyCurve.ByJoinedCurves",
                /*NXLT*/"PolyCurve.ByJoinedCurves@Curve[]");
        }
    }

    public class ThickenCurveLoop : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"PolyCurve.ByThickeningCurve", /*NXLT*/"PolyCurve.ByThickeningCurve@Curve,double,Vector");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new node
            XmlElement pointAsVector = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Point.AsVector", /*NXLT*/"Point.AsVector");
            migrationData.AppendNode(pointAsVector);
            string pointAsVectorId = MigrationManager.GetGuidFromXmlElement(pointAsVector);

            PortId pToV0 = new PortId(pointAsVectorId, 0, PortType.Input);
            PortId oldInPort2 = new PortId(newNodeId, 2, PortType.Input);

            XmlElement connector2 = data.FindFirstConnector(oldInPort2);
            data.ReconnectToPort(connector2, pToV0);
            data.CreateConnector(pointAsVector, 0, newNode, 2);

            return migrationData;
        }
    }

    public class ListCurveLoop : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"ProtoGeometry.dll", /*NXLT*/"PolyCurve.Curves", /*NXLT*/"PolyCurve.Curves");
        }
    }

    public class OffsetCrv : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"ProtoGeometry.dll", /*NXLT*/"Curve.Offset", /*NXLT*/"Curve.Offset@double");
        }
    }

    public class BoundCurve : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create nodes
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Curve.ParameterTrim", /*NXLT*/"Autodesk.DesignScript.Geometry.Curve.ParameterTrim@double,double");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            XmlElement startParam = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Curve.ParameterAtPoint", /*NXLT*/"Curve.ParameterAtPoint@Point");
            migrationData.AppendNode(startParam);
            string startParamId = MigrationManager.GetGuidFromXmlElement(startParam);

            XmlElement endParam = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Curve.ParameterAtPoint", /*NXLT*/"Curve.ParameterAtPoint@Point");
            migrationData.AppendNode(endParam);
            string endParamId = MigrationManager.GetGuidFromXmlElement(endParam);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.Input);
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.Input);
            PortId oldInPort2 = new PortId(newNodeId, 2, PortType.Input);
            PortId startParamInPort = new PortId(startParamId, 1, PortType.Input);
            PortId endParamInPort = new PortId(endParamId, 1, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            data.ReconnectToPort(connector1, startParamInPort);
            data.ReconnectToPort(connector2, endParamInPort);
            data.CreateConnector(startParam, 0, newNode, 1);
            data.CreateConnector(endParam, 0, newNode, 2);
            
            if (connector0 != null)
            {
                string curveInputId = connector0.GetAttribute(/*NXLT*/"start").ToString();
                data.CreateConnectorFromId(curveInputId, 0, startParamId, 0);
                data.CreateConnectorFromId(curveInputId, 0, endParamId, 0);
            }
            
            return migrationData;
        }
    }

    public class ComputeCurveDerivatives : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"ProtoGeometry.dll", /*NXLT*/"Curve.CoordinateSystemAtParameter",
                /*NXLT*/"Curve.CoordinateSystemAtParameter@double");
        }
    }

    public class TangentTransformOnCurveOrEdge : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Curve.CoordinateSystemAtParameter", /*NXLT*/"Curve.CoordinateSystemAtParameter@double");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.Input);
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);
            data.ReconnectToPort(connector0, oldInPort1);
            data.ReconnectToPort(connector1, oldInPort0);

            return migrationData;
        }
    }

    public class ApproximateByTangentArcs : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"ProtoGeometry.dll", /*NXLT*/"Curve.ApproximateWithArcAndLineSegments",
                /*NXLT*/"Curve.ApproximateWithArcAndLineSegments");
        }
    }

    public class CurveDomain : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement codeBlockNode = MigrationManager.CreateCodeBlockNodeFrom(oldNode);
            codeBlockNode.SetAttribute(/*NXLT*/"CodeText", "{0,1};");
            codeBlockNode.SetAttribute(/*NXLT*/"nickname", "Get Curve Domain");

            migrationData.AppendNode(codeBlockNode);
            return migrationData;
        }
    }

    public class CurveLength : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"ProtoGeometry.dll", /*NXLT*/"Curve.Length",
                /*NXLT*/"Curve.Length");
        }
    }
}

