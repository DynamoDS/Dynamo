using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class LoftForm : MigrationNode
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
                "Form.ByLoftingCurveReferences", 
                "Form.ByLoftingCurveReferences@CurveReference[],bool");

            migratedData.AppendNode(dsRevitNode);
            string dsRevitNodeId = MigrationManager.GetGuidFromXmlElement(dsRevitNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.INPUT);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId newInPort0 = new PortId(dsRevitNodeId, 0, PortType.INPUT);
            PortId newInPort1 = new PortId(dsRevitNodeId, 1, PortType.INPUT);

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, newInPort0);

            // Add default values
            foreach (XmlNode child in oldNode.ChildNodes)
            {
                var newChild = child.Clone() as XmlElement;

                switch (newChild.GetAttribute("index"))
                {
                    case "0":
                        newChild.SetAttribute("index", "1");
                        dsRevitNode.AppendChild(newChild);
                        break;

                    default:
                        break;
                }
            }

            return migratedData;
        }
    }

    public class CreateRevolvedGeometry : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll", "Solid.ByRevolve", "Solid.ByRevolve@Curve[],CoordinateSystem,double,double");
        }
    }

    public class CreateSweptGeometry : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Autodesk.DesignScript.Geometry.Solid.BySweep",
                "Autodesk.DesignScript.Geometry.Solid.BySweep@" + 
                "Autodesk.DesignScript.Geometry.Curve,Autodesk.DesignScript.Geometry.Curve");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.INPUT);
            PortId oldInPort3 = new PortId(newNodeId, 3, PortType.INPUT);

            PortId newInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            PortId newInPort1 = new PortId(newNodeId, 1, PortType.INPUT);

            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector3 = data.FindFirstConnector(oldInPort3);
            data.RemoveFirstConnector(oldInPort1);

            //connector1.RemoveAll();
            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector3, newInPort0);

            return migrationData;
        }
    }

    public class CreateExtrusionGeometry : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "RevitNodes.dll",
                "Solid.ByExtrusion",
                "Solid.ByExtrusion@Curve[],Vector,double");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            //append asVector Node
            XmlElement pointAsVector0 = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1, "ProtoGeometry.dll",
                "Point.AsVector", "Point.AsVector");
            migrationData.AppendNode(pointAsVector0);
            string pointAsVector0Id = MigrationManager.GetGuidFromXmlElement(pointAsVector0);

            PortId pToV0 = new PortId(pointAsVector0Id, 0, PortType.INPUT);
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.INPUT);

            XmlElement connector1 = data.FindFirstConnector(oldInPort1);
            data.ReconnectToPort(connector1, pToV0);
            data.CreateConnector(pointAsVector0, 0, newNode, 1);

            return migrationData;
        }
    }

    public class CreateBlendGeometry : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement dsRevitNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsRevitNode, "RevitNodes.dll",
                "Solid.ByBlend", "Solid.ByBlend@Curve[][]");
            migratedData.AppendNode(dsRevitNode);
            string dsRevitNodeId = MigrationManager.GetGuidFromXmlElement(dsRevitNode);

            XmlElement createListNode = MigrationManager.CreateNode(data.Document,
                oldNode, 0, "DSCoreNodesUI.CreateList", "Create List");
            migratedData.AppendNode(createListNode);
            createListNode.SetAttribute("inputcount", "2");
            string createListNodeId = MigrationManager.GetGuidFromXmlElement(createListNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.INPUT);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId newInPort0 = new PortId(createListNodeId, 0, PortType.INPUT);
            PortId newInPort1 = new PortId(createListNodeId, 1, PortType.INPUT);

            data.ReconnectToPort(connector0, newInPort0);
            data.ReconnectToPort(connector1, newInPort1);
            data.CreateConnector(createListNode, 0, dsRevitNode, 0);

            return migratedData;
        }
    }

    public class CreateSweptBlendGeometry : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", 
                "Autodesk.DesignScript.Geometry.Solid.BySweep", 
                "Autodesk.DesignScript.Geometry.Solid.BySweep@Autodesk.DesignScript.Geometry.PolyCurve,Autodesk.DesignScript.Geometry.PolyCurve,bool");
        }
    }

    public class BooleanOperation : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement dummyNode = MigrationManager.CreateDummyNode(oldNode, 2, 1);
            migrationData.AppendNode(dummyNode);

            return migrationData;
        }
    }

    public class SolidDifference : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll", "Solid.ByBooleanDifference",
                "Solid.ByBooleanDifference@Solid,Solid");
        }
    }

    public class SolidUnion : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll", "Solid.ByBooleanUnion",
                "Solid.ByBooleanUnion@Solid,Solid");
        }
    }

    public class SolidIntersection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll", "Solid.ByBooleanIntersection",
                "Solid.ByBooleanIntersection@Solid,Solid");
        }
    }

    public class ElementSolid : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "Solid.FromElement", "Solid.FromElement@Element");
        }
    }

    public class SolidCylinder : MigrationNode
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
                "Solid.Cylinder", "Solid.Cylinder@Point,double,Vector,double");

            migratedData.AppendNode(dsRevitNode);
            string dsRevitNodeId = MigrationManager.GetGuidFromXmlElement(dsRevitNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.INPUT);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId oldInPort2 = new PortId(oldNodeId, 2, PortType.INPUT);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            PortId oldInPort3 = new PortId(oldNodeId, 3, PortType.INPUT);
            XmlElement connector3 = data.FindFirstConnector(oldInPort3);

            PortId newInPort0 = new PortId(dsRevitNodeId, 0, PortType.INPUT);
            PortId newInPort1 = new PortId(dsRevitNodeId, 1, PortType.INPUT);
            PortId newInPort2 = new PortId(dsRevitNodeId, 2, PortType.INPUT);
            PortId newInPort3 = new PortId(dsRevitNodeId, 3, PortType.INPUT);

            data.ReconnectToPort(connector0, newInPort2);
            data.ReconnectToPort(connector1, newInPort0);
            data.ReconnectToPort(connector2, newInPort1);
            data.ReconnectToPort(connector3, newInPort3);

            // Add default values
            foreach (XmlNode child in oldNode.ChildNodes)
            {
                var newChild = child.Clone() as XmlElement;
                switch (newChild.GetAttribute("index"))
                {
                    case "0":
                        if (connector0 != null) break;
                        XmlElement zAxis0 = MigrationManager.CreateFunctionNode(
                            data.Document, oldNode, 1, "ProtoGeometry.dll",
                            "Vector.ZAxis", "Vector.ZAxis");
                        migratedData.AppendNode(zAxis0);
                        data.CreateConnector(zAxis0, 0, dsRevitNode, 2);
                        break;

                    case "1":
                        if (connector1 != null) break;
                        XmlElement cbn1 = MigrationManager.CreateCodeBlockNodeModelNode(
                            data.Document, oldNode, 0, "Point.ByCoordinates(0,0,0);");
                        migratedData.AppendNode(cbn1);
                        data.CreateConnector(cbn1, 0, dsRevitNode, 0);
                        break;

                    case "2":
                        newChild.SetAttribute("index", "1");
                        dsRevitNode.AppendChild(newChild);
                        break;

                    case "3":
                        dsRevitNode.AppendChild(newChild);
                        break;

                    default:
                        break;
                }
            }

            return migratedData;
        }
    }

    public class SolidSphere : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "RevitNodes.dll",
                "Solid.Sphere", "Solid.Sphere@Point,double");

            migrationData.AppendNode(newNode);

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
                            data.Document, oldNode, 0, "Point.ByCoordinates(0,0,0);");
                        migrationData.AppendNode(cbn0);
                        data.CreateConnector(cbn0, 0, newNode, 0);
                        break;

                    case "1":
                        newNode.AppendChild(newChild);
                        break;

                    default:
                        break;
                }
            }

            return migrationData;
        }
    }

    public class SolidTorus : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "RevitNodes.dll",
                "Solid.Torus", "Solid.Torus@Vector,Point,double,double");

            migrationData.AppendNode(newNode);

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

                        XmlElement zAxis0 = MigrationManager.CreateFunctionNode(
                            data.Document, oldNode, 0, "ProtoGeometry.dll",
                            "Vector.ZAxis", "Vector.ZAxis");
                        migrationData.AppendNode(zAxis0);
                        data.CreateConnector(zAxis0, 0, newNode, 0);
                        break;

                    case "1":
                        PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.INPUT);
                        XmlElement connector1 = data.FindFirstConnector(oldInPort1);
                        if (connector1 != null) break;

                        XmlElement cbn1 = MigrationManager.CreateCodeBlockNodeModelNode(
                            data.Document, oldNode, 1, "Point.ByCoordinates(0,0,1);");
                        migrationData.AppendNode(cbn1);
                        data.CreateConnector(cbn1, 0, newNode, 1);
                        break;

                    case "2": case "3":
                        newNode.AppendChild(newChild);
                        break;

                    default:
                        break;
                }
            }

            return migrationData;
        }
    }

    public class SolidBoxByTwoCorners : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "RevitNodes.dll",
                "Solid.BoxByTwoCorners", "Solid.BoxByTwoCorners@Point,Point");

            migrationData.AppendNode(newNode);

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
                            data.Document, oldNode, 0, "Point.ByCoordinates(-1,-1,-1);");
                        migrationData.AppendNode(cbn0);
                        data.CreateConnector(cbn0, 0, newNode, 0);
                        break;

                    case "1":
                        PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.INPUT);
                        XmlElement connector1 = data.FindFirstConnector(oldInPort1);
                        if (connector1 != null) break;

                        XmlElement cbn1 = MigrationManager.CreateCodeBlockNodeModelNode(
                            data.Document, oldNode, 1, "Point.ByCoordinates(1,1,1);");
                        migrationData.AppendNode(cbn1);
                        data.CreateConnector(cbn1, 0, newNode, 1);
                        break;

                    default:
                        break;
                }
            }

            return migrationData;
        }
    }

    public class SolidBoxByCenterAndDimensions : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "RevitNodes.dll",
                "Solid.BoxByCenterAndDimensions",
                "Solid.BoxByCenterAndDimensions@Point,double,double,double");

            migrationData.AppendNode(newNode);

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

                        XmlElement cbn = MigrationManager.CreateCodeBlockNodeModelNode(
                            data.Document, oldNode, 0, "Point.ByCoordinates(0,0,0);");
                        migrationData.AppendNode(cbn);
                        data.CreateConnector(cbn, 0, newNode, 0);
                        break;

                    case "1": case "2": case "3":
                        newNode.AppendChild(newChild);
                        break;

                    default:
                        break;
                }
            }

            return migrationData;
        }
    }

    public class GeometryObjectsFromRoot : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Geometry.Explode", "Geometry.Explode");
        }
    }

    public class TransformSolid : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Geometry.Transform",
                "Geometry.Transform@CoordinateSystem");
        }
    }

    public class BlendEdges : MigrationNode
    {
    }

    public class ChamferEdges : MigrationNode
    {
    }

    public class ReplaceFacesOfSolid : MigrationNode
    {
        // PB: deprecated
    }

    public class OnesidedEdgesAsCurveLoops : MigrationNode
    {
        // PB: deprecated
    }

    public class PatchSolid : MigrationNode
    {
        // PB: deprecated
    }

    public class SkinCurveLoops : MigrationNode
    {
        // PB: deprecated
    }

    public class VolumeMeasure : MigrationNode
    {
        // PB: deprecated
    }

}
