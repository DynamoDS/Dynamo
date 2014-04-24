using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class Xyz : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Point.ByCoordinates", "Point.ByCoordinates@double,double,double");
            migrationData.AppendNode(newNode);

            // Add default values
            foreach (XmlNode child in oldNode.ChildNodes)
                newNode.AppendChild(child.Clone());

            return migrationData;
        }
    }

    public class XyzFromPolar : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Point.ByCylindricalCoordinates", "Point.ByCylindricalCoordinates@CoordinateSystem,double,double,double");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new node
            XmlElement identityCoordinateSystem = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "ProtoGeometry.dll",
                "CoordinateSystem.Identity",
                "CoordinateSystem.Identity");
            migrationData.AppendNode(identityCoordinateSystem);

            XmlElement converterNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1, "DSCoreNodes.dll",
                "Math.RadiansToDegrees", "Math.RadiansToDegrees@double");
            migrationData.AppendNode(converterNode);
            string converterNodeId = MigrationManager.GetGuidFromXmlElement(converterNode);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.INPUT);

            PortId newInPort3 = new PortId(newNodeId, 3, PortType.INPUT);
            PortId converterInPort = new PortId(converterNodeId, 0, PortType.INPUT);

            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            data.ReconnectToPort(connector0, newInPort3);
            data.ReconnectToPort(connector1, converterInPort);
            data.CreateConnector(converterNode, 0, newNode, 1);
            data.CreateConnector(identityCoordinateSystem, 0, newNode, 0);

            // Add default values
            foreach (XmlNode child in oldNode.ChildNodes)
                newNode.AppendChild(child.Clone());

            return migrationData;
        }
    }

    public class XyzToPolar : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);
            
            XmlElement codeBlockNode = MigrationManager.CreateCodeBlockNodeFrom(oldNode);
            codeBlockNode.SetAttribute("CodeText",
                "radius = Math.Sqrt(p.X*p.X + p.Y*p.Y);\n" +
                "rotation = Math.DegreesToRadians\n" +
                "(Math.Atan(p.Y/p.X));\n" +
                "offset = p.Z;");

            codeBlockNode.SetAttribute("nickname", "XYZ to Polar Coordinates");

            // Add default values
            foreach (XmlNode child in oldNode.ChildNodes)
            {
                var newChild = child.Clone() as XmlElement;

                switch (newChild.GetAttribute("index"))
                {
                    case "0":
                        PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.INPUT);
                        XmlElement connector0 = data.FindFirstConnector(oldInPort0);
                        if (connector0 != null) break;

                        XmlElement cbn0 = MigrationManager.CreateCodeBlockNodeModelNode(
                            data.Document, oldNode, 0, "Point.ByCoordinates(1,0,0);");
                        migrationData.AppendNode(cbn0);
                        data.CreateConnector(cbn0, 0, codeBlockNode, 0);
                        break;

                    default:
                        break;
                }
            }

            migrationData.AppendNode(codeBlockNode);
            return migrationData;
        }
    }

    public class XyzFromSpherical : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Point.BySphericalCoordinates", "Point.BySphericalCoordinates@CoordinateSystem,double,double,double");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new node
            XmlElement identityCoordinateSystem = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "ProtoGeometry.dll",
                "CoordinateSystem.Identity",
                "CoordinateSystem.Identity");
            migrationData.AppendNode(identityCoordinateSystem);

            XmlElement converterPhiNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1, "DSCoreNodes.dll",
                "Math.RadiansToDegrees", "Math.RadiansToDegrees@double");
            migrationData.AppendNode(converterPhiNode);
            string converterPhiNodeId = MigrationManager.GetGuidFromXmlElement(converterPhiNode);

            XmlElement converterThetaNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 2, "DSCoreNodes.dll",
                "Math.RadiansToDegrees", "Math.RadiansToDegrees@double");
            migrationData.AppendNode(converterThetaNode);
            string converterThetaNodeId = MigrationManager.GetGuidFromXmlElement(converterThetaNode);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.INPUT);
            PortId oldInPort2 = new PortId(newNodeId, 2, PortType.INPUT);
            PortId newInPort3 = new PortId(newNodeId, 3, PortType.INPUT);

            PortId converterThetaInPort = new PortId(converterThetaNodeId, 0, PortType.INPUT);
            PortId converterPhiInPort = new PortId(converterPhiNodeId, 0, PortType.INPUT);
            
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            data.ReconnectToPort(connector0, newInPort3);
            data.ReconnectToPort(connector1, converterThetaInPort);
            data.ReconnectToPort(connector2, converterPhiInPort);
            
            data.CreateConnector(identityCoordinateSystem, 0, newNode, 0);
            data.CreateConnector(converterPhiNode, 0, newNode, 1);
            data.CreateConnector(converterThetaNode, 0, newNode, 2);
            
            // Add default values
            foreach (XmlNode child in oldNode.ChildNodes)
            {
                var newChild = child.Clone() as XmlElement;
                switch (newChild.GetAttribute("index"))
                {
                    case "0":
                        newChild.SetAttribute("index", "3");
                        break;
                    case "1":
                        newChild.SetAttribute("index", "2");
                        break;
                    case "2":
                        newChild.SetAttribute("index", "1");
                        break;
                }
                newNode.AppendChild(newChild);
            }

            return migrationData;
        }
    }

    public class XyzToSpherical : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement codeBlockNode = MigrationManager.CreateCodeBlockNodeFrom(oldNode);

            codeBlockNode.SetAttribute("CodeText",
                "radius = Math.Sqrt(p.X*p.X + p.Y*p.Y + p.Z*p.Z);\n" +
                "xyrotation = (p.X == 0 && p.Y == 0) ? 0 : (p.X == 0) ?\n" +
                "((p.Y > 0) ? Math.PI/2 : Math.PI*3/2)\n" +
                ": Math.DegreesToRadians(Math.Atan(p.Y/p.X));\n" +
                "zrotation = (p.X == 0 && p.Y == 0) ? ((p.Z > 0) ? 0 : Math.PI) :\n" +
                "Math.DegreesToRadians(Math.Acos(p.Z / radius));");

            codeBlockNode.SetAttribute("nickname", "XYZ to Spherical Coordinates");

            migrationData.AppendNode(codeBlockNode);
            return migrationData;
        }
    }

    public class XyzFromListOfNumbers : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement codeBlockNode = MigrationManager.CreateCodeBlockNodeFrom(oldNode);

            codeBlockNode.SetAttribute("CodeText",
                "Point.ByCoordinates(\n" +
                "List.Sublists(list,0,3),\n" +
                "List.Sublists(list,1,3),\n" +
                "List.Sublists(list,2,3));");

            codeBlockNode.SetAttribute("nickname", "XYZ from List of Numbers");

            migrationData.AppendNode(codeBlockNode);
            return migrationData;
        }
    }

    public class XyzFromReferencePoint : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll", "ReferencePoint.Point", "ReferencePoint.Point");
        }
    }

    public class XyzComponents : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement codeBlockNode = MigrationManager.CreateCodeBlockNodeFrom(oldNode);

            codeBlockNode.SetAttribute("CodeText", "p.X;\np.Y;\np.Z;");

            codeBlockNode.SetAttribute("nickname", "XYZ Components");

            migrationData.AppendNode(codeBlockNode);
            return migrationData;
        }
    }

    public class XyzGetX : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Point.X", "Point.X");
        }
    }

    public class XyzGetY : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Point.Y", "Point.Y");
        }
    }

    public class XyzGetZ : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Point.Z", "Point.Z");
        }
    }

    public class XyzDistance : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Geometry.DistanceTo", "Geometry.DistanceTo@Geometry");
        }
    }

    public class XyzLength : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Vector.Length", "Vector.Length");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new node
            XmlElement pointAsVector = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "ProtoGeometry.dll",
                "Point.AsVector", "Point.AsVector");
            migrationData.AppendNode(pointAsVector);
            string pointAsVectorId = MigrationManager.GetGuidFromXmlElement(pointAsVector);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            
            PortId pointAsVectorInPort0 = new PortId(pointAsVectorId, 0, PortType.INPUT);
            
            data.ReconnectToPort(connector0, pointAsVectorInPort0);
            data.CreateConnector(pointAsVector, 0, newNode, 0);
            
            return migrationData;
        }
    }

    public class XyzNormalize : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create nodes
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Vector.AsPoint", "Vector.AsPoint");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            XmlElement pointAsVector = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "ProtoGeometry.dll",
                "Point.AsVector", "Point.AsVector");
            migrationData.AppendNode(pointAsVector);
            string pointAsVectorId = MigrationManager.GetGuidFromXmlElement(pointAsVector);

            XmlElement vectorNormalized = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1, "ProtoGeometry.dll",
                "Vector.Normalized", "Vector.Normalized");
            migrationData.AppendNode(vectorNormalized);
            string vectorNormalizedId = MigrationManager.GetGuidFromXmlElement(vectorNormalized);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId pointAsVectorInPort0 = new PortId(pointAsVectorId, 0, PortType.INPUT);
            PortId vectorNormalizedInPort0 = new PortId(vectorNormalizedId, 0, PortType.INPUT);

            data.ReconnectToPort(connector0, pointAsVectorInPort0);
            data.CreateConnector(pointAsVector, 0, vectorNormalized, 0);
            data.CreateConnector(vectorNormalized, 0, newNode, 0);

            return migrationData;
        }
    }

    public class XyzIsZeroLength : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "",
                "Equals", "Equals@var,var");

            migratedData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            XmlElement asVectorNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "ProtoGeometry.dll", "Point.AsVector", "Point.AsVector");
            migratedData.AppendNode(asVectorNode);
            string asVectorNodeId = MigrationManager.GetGuidFromXmlElement(asVectorNode);

            XmlElement lengthNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1, "ProtoGeometry.dll", "Vector.Length", "Vector.Length");
            migratedData.AppendNode(lengthNode);

            XmlElement numberNode = MigrationManager.CreateCodeBlockNodeModelNode(
                data.Document, oldNode, 2, "0;");
            migratedData.AppendNode(numberNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId newInPort0 = new PortId(asVectorNodeId, 0, PortType.INPUT);

            data.ReconnectToPort(connector0, newInPort0);
            data.CreateConnector(asVectorNode, 0, lengthNode, 0);
            data.CreateConnector(lengthNode, 0, newNode, 0);
            data.CreateConnector(numberNode, 0, newNode, 1);
            
            return migratedData;
        }
    }

    public class XyzBasisX : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create nodes
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Vector.AsPoint", "Vector.AsPoint");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            XmlElement pointAsVector = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "ProtoGeometry.dll",
                "Vector.XAxis", "Vector.XAxis");
            migrationData.AppendNode(pointAsVector);
            string pointAsVectorId = MigrationManager.GetGuidFromXmlElement(pointAsVector);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId pointAsVectorInPort0 = new PortId(pointAsVectorId, 0, PortType.INPUT);

            data.ReconnectToPort(connector0, pointAsVectorInPort0);
            data.CreateConnector(pointAsVector, 0, newNode, 0);

            return migrationData;
        }
    }

    public class XyzBasisY : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create nodes
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Vector.AsPoint", "Vector.AsPoint");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            XmlElement pointAsVector = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "ProtoGeometry.dll",
                "Vector.YAxis", "Vector.YAxis");
            migrationData.AppendNode(pointAsVector);
            string pointAsVectorId = MigrationManager.GetGuidFromXmlElement(pointAsVector);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId pointAsVectorInPort0 = new PortId(pointAsVectorId, 0, PortType.INPUT);

            data.ReconnectToPort(connector0, pointAsVectorInPort0);
            data.CreateConnector(pointAsVector, 0, newNode, 0);

            return migrationData;
        }
    }

    public class XyzBasisZ : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create nodes
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Vector.AsPoint", "Vector.AsPoint");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            XmlElement pointAsVector = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "ProtoGeometry.dll",
                "Vector.ZAxis", "Vector.ZAxis");
            migrationData.AppendNode(pointAsVector);
            string pointAsVectorId = MigrationManager.GetGuidFromXmlElement(pointAsVector);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId pointAsVectorInPort0 = new PortId(pointAsVectorId, 0, PortType.INPUT);

            data.ReconnectToPort(connector0, pointAsVectorInPort0);
            data.CreateConnector(pointAsVector, 0, newNode, 0);

            return migrationData;
        }
    }

    public class XyzScale : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Geometry.Scale", "Geometry.Scale@double");
        }
    }

    public class XyzScaleOffset : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var pointAdd = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(pointAdd, "ProtoGeometry.dll",
                "Point.Add", "Point.Add@Vector");
            migrationData.AppendNode(pointAdd);
            string pointAddId = MigrationManager.GetGuidFromXmlElement(pointAdd);

            // Create new nodes
            XmlElement vectorDiff = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "ProtoGeometry.dll",
                "Vector.ByTwoPoints", "Vector.ByTwoPoints@Point,Point");
            migrationData.AppendNode(vectorDiff);
            string vectorDiffId = MigrationManager.GetGuidFromXmlElement(vectorDiff);

            XmlElement geometryScale = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1, "ProtoGeometry.dll",
                "Geometry.Scale", "Geometry.Scale@double");
            migrationData.AppendNode(geometryScale);
            string geometryScaleId = MigrationManager.GetGuidFromXmlElement(geometryScale);

            // Update connectors
            PortId oldInPort0 = new PortId(pointAddId, 0, PortType.INPUT);
            PortId oldInPort1 = new PortId(pointAddId, 1, PortType.INPUT);
            PortId oldInPort2 = new PortId(pointAddId, 2, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            PortId geometryScaleInPort1 = new PortId(geometryScaleId, 1, PortType.INPUT);
            PortId vectorDiffInPort0 = new PortId(vectorDiffId, 0, PortType.INPUT);
            PortId vectorDiffInPort1 = new PortId(vectorDiffId, 1, PortType.INPUT);

            data.ReconnectToPort(connector0, vectorDiffInPort1);
            data.ReconnectToPort(connector1, geometryScaleInPort1);
            data.ReconnectToPort(connector2, vectorDiffInPort0);
            data.CreateConnector(vectorDiff, 0, geometryScale, 0);
            data.CreateConnector(geometryScale, 0, pointAdd, 1);

            if (connector2 != null)
            {
                string baseInputId = connector2.GetAttribute("start").ToString();
                data.CreateConnectorFromId(baseInputId, 0, pointAddId, 0);
            }

            return migrationData;
        }
    }

    public class XyzAdd : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Point.Add", "Point.Add@Vector");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new node
            XmlElement pointAsVector = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "ProtoGeometry.dll",
                "Point.AsVector", "Point.AsVector");
            migrationData.AppendNode(pointAsVector);
            string pointAsVectorId = MigrationManager.GetGuidFromXmlElement(pointAsVector);

            // Update connectors
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.INPUT);
            PortId pointAsVectorInPort0 = new PortId(pointAsVectorId, 0, PortType.INPUT);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);
            
            data.ReconnectToPort(connector1, pointAsVectorInPort0);
            data.CreateConnector(pointAsVector, 0, newNode, 1);

            return migrationData;
        }
    }

    public class XyzSubtract : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Point.Subtract", "Point.Subtract@Vector");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new node
            XmlElement pointAsVector = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "ProtoGeometry.dll",
                "Point.AsVector", "Point.AsVector");
            migrationData.AppendNode(pointAsVector);
            string pointAsVectorId = MigrationManager.GetGuidFromXmlElement(pointAsVector);

            // Update connectors
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.INPUT);
            PortId pointAsVectorInPort0 = new PortId(pointAsVectorId, 0, PortType.INPUT);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            data.ReconnectToPort(connector1, pointAsVectorInPort0);
            data.CreateConnector(pointAsVector, 0, newNode, 1);

            return migrationData;
        }
    }

    public class XyzAverage : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);
            
            XmlElement newPointNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newPointNode, "ProtoGeometry.dll",
                "Point.ByCoordinates", "Point.ByCoordinates@double,double,double");
            migrationData.AppendNode(newPointNode);
            string newPointNodeId = MigrationManager.GetGuidFromXmlElement(newPointNode);

            XmlElement getXNode = MigrationManager.CreateFunctionNode(data.Document, oldNode, 0,
                "ProtoGeometry.dll", "Point.X", "Point.X");
            XmlElement getYNode = MigrationManager.CreateFunctionNode(data.Document, oldNode, 1,
                "ProtoGeometry.dll", "Point.Y", "Point.Y");
            XmlElement getZNode = MigrationManager.CreateFunctionNode(data.Document, oldNode, 2,
                "ProtoGeometry.dll", "Point.Z", "Point.Z");
            migrationData.AppendNode(getXNode);
            migrationData.AppendNode(getYNode);
            migrationData.AppendNode(getZNode);

            XmlElement xAverageNode = MigrationManager.CreateFunctionNode(data.Document, oldNode, 3,
                "DSCoreNodes.dll", "Average", "Average@Double[]");
            XmlElement yAverageNode = MigrationManager.CreateFunctionNode(data.Document, oldNode, 4,
                "DSCoreNodes.dll", "Average", "Average@Double[]");
            XmlElement zAverageNode = MigrationManager.CreateFunctionNode(data.Document, oldNode, 5,
                "DSCoreNodes.dll", "Average", "Average@Double[]");
            migrationData.AppendNode(xAverageNode);
            migrationData.AppendNode(yAverageNode);
            migrationData.AppendNode(zAverageNode);

            string getXNodeId = MigrationManager.GetGuidFromXmlElement(getXNode);
            string getYNodeId = MigrationManager.GetGuidFromXmlElement(getYNode);
            string getZNodeId = MigrationManager.GetGuidFromXmlElement(getZNode);
            string xAverageNodeId = MigrationManager.GetGuidFromXmlElement(xAverageNode);
            string yAverageNodeId = MigrationManager.GetGuidFromXmlElement(yAverageNode);
            string zAverageNodeId = MigrationManager.GetGuidFromXmlElement(zAverageNode);

            data.CreateConnector(getXNode, 0, xAverageNode, 0);
            data.CreateConnector(getYNode, 0, yAverageNode, 0);
            data.CreateConnector(getZNode, 0, zAverageNode, 0);

            data.CreateConnector(xAverageNode, 0, newPointNode, 0);
            data.CreateConnector(yAverageNode, 0, newPointNode, 1);
            data.CreateConnector(zAverageNode, 0, newPointNode, 2);

            PortId oldOutPort = new PortId(oldNodeId, 0, PortType.OUTPUT);
            PortId newOutPort = new PortId(newPointNodeId, 0, PortType.OUTPUT);
            var connectors = data.FindConnectors(oldOutPort);
            if (null != connectors)
            {
                foreach (var connector in connectors)
                {
                    data.ReconnectToPort(connector, newOutPort);
                }
            }

            PortId oldInPort = new PortId(oldNodeId, 0, PortType.INPUT);
            PortId newInPort0 = new PortId(getXNodeId, 0, PortType.INPUT);
            PortId newInPort1 = new PortId(getYNodeId, 0, PortType.INPUT);
            PortId newInPort2 = new PortId(getZNodeId, 0, PortType.INPUT);
            var connector0 = data.FindFirstConnector(oldInPort);
            if (null != connector0)
            {
                data.ReconnectToPort(connector0, newInPort0);
                var connector1 = MigrationManager.CreateFunctionNodeFrom(connector0);
                data.CreateConnector(connector1);
                var connector2 = MigrationManager.CreateFunctionNodeFrom(connector0);
                data.CreateConnector(connector2);
                data.ReconnectToPort(connector1, newInPort1);
                data.ReconnectToPort(connector2, newInPort2);
            }

            return migrationData;
        }
    }

    public class XyzNegate : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var geometryScale = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(geometryScale, "ProtoGeometry.dll",
                "Geometry.Scale", "Geometry.Scale@double");
            migrationData.AppendNode(geometryScale);
            
            // Create new node
            XmlElement minusOne = MigrationManager.CreateCodeBlockNodeModelNode(
                data.Document, oldNode, 0, "-1");
            migrationData.AppendNode(minusOne);

            // Update connectors
            data.CreateConnector(minusOne, 0, geometryScale, 1);

            return migrationData;
        }
    }

    public class XyzCrossProduct : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create nodes
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Vector.AsPoint", "Vector.AsPoint");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            XmlElement pointAsVector0 = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "ProtoGeometry.dll",
                "Point.AsVector", "Point.AsVector");
            migrationData.AppendNode(pointAsVector0);
            string pointAsVector0Id = MigrationManager.GetGuidFromXmlElement(pointAsVector0);

            XmlElement pointAsVector1 = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "ProtoGeometry.dll",
                "Point.AsVector", "Point.AsVector");
            migrationData.AppendNode(pointAsVector1);
            string pointAsVector1Id = MigrationManager.GetGuidFromXmlElement(pointAsVector1);

            XmlElement vectorCross = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1, "ProtoGeometry.dll",
                "Vector.Cross", "Vector.Cross@Vector");
            migrationData.AppendNode(vectorCross);
            string vectorCrossId = MigrationManager.GetGuidFromXmlElement(vectorCross);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId pointAsVector0InPort0 = new PortId(pointAsVector0Id, 0, PortType.INPUT);
            PortId pointAsVector1InPort0 = new PortId(pointAsVector1Id, 0, PortType.INPUT);
            PortId vectorCrossInPort0 = new PortId(vectorCrossId, 0, PortType.INPUT);

            data.ReconnectToPort(connector0, pointAsVector0InPort0);
            data.ReconnectToPort(connector1, pointAsVector1InPort0);
            data.CreateConnector(pointAsVector0, 0, vectorCross, 0);
            data.CreateConnector(pointAsVector1, 0, vectorCross, 1);
            data.CreateConnector(vectorCross, 0, newNode, 0);

            return migrationData;
        }
    }

    public class XyzDotProduct : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create nodes
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Vector.AsPoint", "Vector.AsPoint");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            XmlElement pointAsVector0 = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "ProtoGeometry.dll",
                "Point.AsVector", "Point.AsVector");
            migrationData.AppendNode(pointAsVector0);
            string pointAsVector0Id = MigrationManager.GetGuidFromXmlElement(pointAsVector0);

            XmlElement pointAsVector1 = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "ProtoGeometry.dll",
                "Point.AsVector", "Point.AsVector");
            migrationData.AppendNode(pointAsVector1);
            string pointAsVector1Id = MigrationManager.GetGuidFromXmlElement(pointAsVector1);

            XmlElement vectorDot = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1, "ProtoGeometry.dll",
                "Vector.Dot", "Vector.Dot@Vector");
            migrationData.AppendNode(vectorDot);
            string vectorDotId = MigrationManager.GetGuidFromXmlElement(vectorDot);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId pointAsVector0InPort0 = new PortId(pointAsVector0Id, 0, PortType.INPUT);
            PortId pointAsVector1InPort0 = new PortId(pointAsVector1Id, 0, PortType.INPUT);
            PortId vectorDotInPort0 = new PortId(vectorDotId, 0, PortType.INPUT);

            data.ReconnectToPort(connector0, pointAsVector0InPort0);
            data.ReconnectToPort(connector1, pointAsVector1InPort0);
            data.CreateConnector(pointAsVector0, 0, vectorDot, 0);
            data.CreateConnector(pointAsVector1, 0, vectorDot, 1);
            data.CreateConnector(vectorDot, 0, newNode, 0);

            return migrationData;
        }
    }

    public class XyzStartEndVector : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            // Create nodes
            XmlElement newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll", 
                "Vector.AsPoint", "Vector.AsPoint");
            migrationData.AppendNode(newNode);
            
            XmlElement normalized = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "ProtoGeometry.dll",
                "Vector.Normalized", "Vector.Normalized@Vector");
            migrationData.AppendNode(normalized);
            
            XmlElement vectorNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1, "ProtoGeometry.dll",
                "Vector.ByTwoPoints", "Vector.ByTwoPoints@Point,Point");
            migrationData.AppendNode(vectorNode);
            string vectorNodeId = MigrationManager.GetGuidFromXmlElement(vectorNode);

            // Update connectors
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.INPUT);
            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId newInPort0 = new PortId(vectorNodeId, 0, PortType.INPUT);
            PortId newInPort1 = new PortId(vectorNodeId, 1, PortType.INPUT);
            
            data.ReconnectToPort(connector0, newInPort0);
            data.ReconnectToPort(connector1, newInPort1);
            data.CreateConnector(vectorNode, 0, normalized, 0);
            data.CreateConnector(normalized, 0, newNode, 0);

            return migrationData;
        }
    }

    public class ReferencePtGrid : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement codeBlockNode = MigrationManager.CreateCodeBlockNodeFrom(oldNode);
            string codeBlockNodeId = MigrationManager.GetGuidFromXmlElement(codeBlockNode);

            codeBlockNode.SetAttribute("CodeText",
                "Flatten(Point.ByCoordinates(\n" +
                "(xstart..#xcount..xspace)<1>,\n" +
                "(ystart..#ycount..yspace)<2>,\n" +
                "(zstart..#zcount..zspace)<3>));");

            codeBlockNode.SetAttribute("nickname", "XYZ Grid");

            migrationData.AppendNode(codeBlockNode);

            // Update connectors
            PortId inPort0 = new PortId(codeBlockNodeId, 0, PortType.INPUT);
            PortId inPort1 = new PortId(codeBlockNodeId, 1, PortType.INPUT);
            PortId inPort2 = new PortId(codeBlockNodeId, 2, PortType.INPUT);
            PortId inPort3 = new PortId(codeBlockNodeId, 3, PortType.INPUT);
            PortId inPort4 = new PortId(codeBlockNodeId, 4, PortType.INPUT);
            PortId inPort5 = new PortId(codeBlockNodeId, 5, PortType.INPUT);
            PortId inPort6 = new PortId(codeBlockNodeId, 6, PortType.INPUT);
            PortId inPort7 = new PortId(codeBlockNodeId, 7, PortType.INPUT);
            PortId inPort8 = new PortId(codeBlockNodeId, 8, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(inPort0);
            XmlElement connector1 = data.FindFirstConnector(inPort1);
            XmlElement connector2 = data.FindFirstConnector(inPort2);
            XmlElement connector3 = data.FindFirstConnector(inPort3);
            XmlElement connector4 = data.FindFirstConnector(inPort4);
            XmlElement connector5 = data.FindFirstConnector(inPort5);
            XmlElement connector6 = data.FindFirstConnector(inPort6);
            XmlElement connector7 = data.FindFirstConnector(inPort7);
            XmlElement connector8 = data.FindFirstConnector(inPort8);

            data.ReconnectToPort(connector0, inPort1);
            data.ReconnectToPort(connector1, inPort4);
            data.ReconnectToPort(connector2, inPort7);
            data.ReconnectToPort(connector3, inPort0);
            data.ReconnectToPort(connector4, inPort3);
            data.ReconnectToPort(connector5, inPort6);
            data.ReconnectToPort(connector6, inPort2);
            data.ReconnectToPort(connector7, inPort5);
            data.ReconnectToPort(connector8, inPort8);

            return migrationData;
        }
    }

    public class XyzArrayAlongCurve : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement codeBlockNode = MigrationManager.CreateCodeBlockNodeFrom(oldNode);
            string codeBlockNodeId = MigrationManager.GetGuidFromXmlElement(codeBlockNode);

            codeBlockNode.SetAttribute("CodeText",
                "List.AddItemToFront(curve.PointAtParameter(0),\n" +
                "curve.DivideByLength(count - 2).PointAtParameter(1));");

            codeBlockNode.SetAttribute("nickname", "XYZ Array On Curve");

            migrationData.AppendNode(codeBlockNode);

            return migrationData;
        }
    }

    public class EqualDistXyzAlongCurve : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement codeBlockNode = MigrationManager.CreateCodeBlockNodeFrom(oldNode);
            string codeBlockNodeId = MigrationManager.GetGuidFromXmlElement(codeBlockNode);

            codeBlockNode.SetAttribute("CodeText",
                "List.AddItemToFront(curve.PointAtParameter(0),\n" +
                "curve.DivideByDistance(count - 2).PointAtParameter(1));");

            codeBlockNode.SetAttribute("nickname", "Equal Distanced XYZs On Curve");

            migrationData.AppendNode(codeBlockNode);

            return migrationData;
        }
    }

    public class XyzOnCurveOrEdge : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Curve.PointAtParameter", "Curve.PointAtParameter@double");
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

    public class XyzByDistanceOffsetFromOrigin : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement translateNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(translateNode, "ProtoGeometry.dll",
                "Geometry.Translate", "Geometry.Translate@Autodesk.DesignScript.Geometry.Vector,double");

            migratedData.AppendNode(translateNode);
            string translateNodeId = MigrationManager.GetGuidFromXmlElement(translateNode);

            XmlElement distanceToNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "ProtoGeometry.dll",
                "Geometry.DistanceTo", "Geometry.DistanceTo@Geometry");
            migratedData.AppendNode(distanceToNode);
            string distanceToNodeId = MigrationManager.GetGuidFromXmlElement(distanceToNode);

            XmlElement multiplyNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1, "", "*", "*@,");
            migratedData.AppendNode(multiplyNode);
            string multiplyNodeId = MigrationManager.GetGuidFromXmlElement(multiplyNode);

            XmlElement asVectorNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 2, "ProtoGeometry.dll", "Point.AsVector", "Point.AsVector");
            migratedData.AppendNode(asVectorNode);
            string asVectorNodeId = MigrationManager.GetGuidFromXmlElement(asVectorNode);


            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            XmlElement connector3 = null;
            if (connector0 != null)
            {
                connector3 = MigrationManager.CreateFunctionNodeFrom(connector0);
                data.CreateConnector(connector3);
            }

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.INPUT);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            XmlElement connector4 = null;
            if (connector1 != null)
            {
                connector4 = MigrationManager.CreateFunctionNodeFrom(connector1);
                data.CreateConnector(connector4);
            }

            PortId oldInPort2 = new PortId(oldNodeId, 2, PortType.INPUT);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            PortId newInPort0 = new PortId(translateNodeId, 0, PortType.INPUT);
            PortId newInPort1 = new PortId(translateNodeId, 1, PortType.INPUT);
            PortId newInPort2 = new PortId(translateNodeId, 2, PortType.INPUT);
            PortId newInPort3 = new PortId(distanceToNodeId, 0, PortType.INPUT);
            PortId newInPort4 = new PortId(distanceToNodeId, 1, PortType.INPUT);
            PortId newInPort5 = new PortId(multiplyNodeId, 1, PortType.INPUT);
            PortId newInPort6 = new PortId(asVectorNodeId, 0, PortType.INPUT);

            data.ReconnectToPort(connector0, newInPort0);
            data.ReconnectToPort(connector1, newInPort6);
            data.ReconnectToPort(connector2, newInPort5);
            data.ReconnectToPort(connector3, newInPort3);
            data.ReconnectToPort(connector4, newInPort4);

            data.CreateConnector(distanceToNode, 0, multiplyNode, 0);
            data.CreateConnector(asVectorNode, 0, translateNode, 1);
            data.CreateConnector(multiplyNode, 0, translateNode, 2);

            return migratedData;
        }
    }
}
