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
            return MigrateToDsFunction(data, "RevitNodes.dll", "Solid.ByExtrusion", "Solid.ByExtrusion@Curve[],Vector,double");
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
                "DSCoreNodesUI.CreateList", "Create List");
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
                "Solid.FromElement", "Solid.FromElement@AbstractElement");
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

            return migratedData;
        }
    }

    public class SolidSphere : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll", "Solid.Sphere", "Solid.Sphere@Point,double");
        }
    }

    public class SolidTorus : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll", "Solid.Torus", "Solid.Torus@Vector,Point,double,double");
        }
    }

    public class SolidBoxByTwoCorners : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll", "Solid.BoxByTwoCorners",
                "Solid.BoxByTwoCorners@Point,Point");
        }
    }

    public class SolidBoxByCenterAndDimensions : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll", "Solid.BoxByCenterAndDimensions",
                "Solid.BoxByCenterAndDimensions@Point,double,double,double");
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

    public class ReplaceFacesOfSolid : MigrationNode
    {
    }

    public class BlendEdges : MigrationNode
    {
    }

    public class ChamferEdges : MigrationNode
    {
    }

    public class OnesidedEdgesAsCurveLoops : MigrationNode
    {
    }

    public class PatchSolid : MigrationNode
    {
    }

    public class SkinCurveLoops : MigrationNode
    {
    }

    public class VolumeMeasure : MigrationNode
    {
    }

}
