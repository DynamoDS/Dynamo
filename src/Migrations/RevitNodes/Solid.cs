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
            XmlElement newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Surface.ByLoft",
                "Surface.ByLoft@Autodesk.DesignScript.Geometry.Curve[]");

            migratedData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);


            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            data.RemoveFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);
            PortId newInPort0 = new PortId(newNodeId, 0, PortType.Input);
            data.ReconnectToPort(connector1, newInPort0);

            PortId oldInPort2 = new PortId(oldNodeId, 2, PortType.Input);
            data.RemoveFirstConnector(oldInPort2);

            return migratedData;
        }
    }

    public class CreateRevolvedGeometry : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement dummyNode = MigrationManager.CreateDummyNode(oldNode, 4, 1);
            migrationData.AppendNode(dummyNode);

            return migrationData;
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
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.Input);
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.Input);
            PortId oldInPort3 = new PortId(newNodeId, 3, PortType.Input);

            PortId newInPort0 = new PortId(newNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(newNodeId, 1, PortType.Input);

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
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Curve.ExtrudeAsSolid",
                "Curve.ExtrudeAsSolid@Autodesk.DesignScript.Geometry.Vector,double");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            //append asVector Node
            XmlElement pointAsVector0 = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1, "ProtoGeometry.dll",
                "Point.AsVector", "Point.AsVector");
            migrationData.AppendNode(pointAsVector0);
            string pointAsVector0Id = MigrationManager.GetGuidFromXmlElement(pointAsVector0);

            PortId pToV0 = new PortId(pointAsVector0Id, 0, PortType.Input);
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.Input);

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
            MigrationManager.SetFunctionSignature(dsRevitNode, "ProtoGeometry.dll",
                "Solid.ByLoft", "Solid.ByLoft@Curve[]");
            migratedData.AppendNode(dsRevitNode);
            string dsRevitNodeId = MigrationManager.GetGuidFromXmlElement(dsRevitNode);

            XmlElement createListNode = MigrationManager.CreateNode(data.Document,
                oldNode, 0, "DSCoreNodesUI.CreateList", "Create List");
            migratedData.AppendNode(createListNode);
            createListNode.SetAttribute("inputcount", "2");
            string createListNodeId = MigrationManager.GetGuidFromXmlElement(createListNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId newInPort0 = new PortId(createListNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(createListNodeId, 1, PortType.Input);

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
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var index = oldNode.GetAttribute("index");
            switch (index)
            {
                case "1":
                    return Dynamo.Nodes.SolidIntersection.Migrate_0630_to_0700(data);
                case "2":
                    return Dynamo.Nodes.SolidDifference.Migrate_0630_to_0700(data);
                default:
                    return Dynamo.Nodes.SolidUnion.Migrate_0630_to_0700(data);
            }
        }
    }

    public class SolidDifference : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Solid.Difference",
                "Solid.Difference@Solid,Solid");
        }
    }

    public class SolidUnion : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Solid.Union",
                "Solid.Union@Solid,Solid");
        }
    }

    public class SolidIntersection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Geometry.Intersect",
                "Geometry.Intersect@Geometry,Geometry");
        }
    }

    public class ElementSolid : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "Element.Solids", "Element.Solids");
        }
    }

    public class SolidCylinder : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            var migratedData = new NodeMigrationData(data.Document);
            var oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var codeBlockNode = MigrationManager.CreateCodeBlockNodeFrom(oldNode);
            codeBlockNode.SetAttribute("CodeText",
                "p=Plane.ByOriginNormal(origin,axis.AsVector());\n"+
                "cs=CoordinateSystem.ByPlane(p);\n"+
                "Cylinder.ByRadiusHeight(cs,r,h);");
            migratedData.AppendNode(codeBlockNode);

            //create and reconnect the connecters
            var oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            var oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            var oldInPort2 = new PortId(oldNodeId, 2, PortType.Input);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            var oldInPort3 = new PortId(oldNodeId, 3, PortType.Input);
            XmlElement connector3 = data.FindFirstConnector(oldInPort3);

            var oldOutPort0 = new PortId(oldNodeId, 0, PortType.Output);
            var oldOutConnectors = data.FindConnectors(oldOutPort0);
            
            var newInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            var newInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            var newInPort2 = new PortId(oldNodeId, 2, PortType.Input);
            var newInPort3 = new PortId(oldNodeId, 3, PortType.Input);
            var newOutPort2 = new PortId(oldNodeId, 2, PortType.Output);

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, newInPort0);
            data.ReconnectToPort(connector2, newInPort2);
            data.ReconnectToPort(connector3, newInPort3);

            if (oldOutConnectors.Any())
            {
                foreach (var connector in oldOutConnectors)
                {
                    //connect anything that previously was connected to output port 0
                    //to output port 2
                    data.ReconnectToPort(connector, newOutPort2); 
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
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Sphere.ByCenterPointRadius", "Sphere.ByCenterPointRadius@Point,double");

            migrationData.AppendNode(newNode);

            // Add default values
            foreach (XmlNode child in oldNode.ChildNodes)
            {
                var newChild = child.Clone() as XmlElement;

                switch (newChild.GetAttribute("index"))
                {
                    case "0":
                        PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
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
                        PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
                        XmlElement connector0 = data.FindFirstConnector(oldInPort0);
                        if (connector0 != null) break;

                        XmlElement zAxis0 = MigrationManager.CreateFunctionNode(
                            data.Document, oldNode, 0, "ProtoGeometry.dll",
                            "Vector.ZAxis", "Vector.ZAxis");
                        migrationData.AppendNode(zAxis0);
                        data.CreateConnector(zAxis0, 0, newNode, 0);
                        break;

                    case "1":
                        PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
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
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Cuboid.ByCorners", "Cuboid.ByCorners@Point,Point");

            migrationData.AppendNode(newNode);

            // Add default values
            foreach (XmlNode child in oldNode.ChildNodes)
            {
                var newChild = child.Clone() as XmlElement;

                switch (newChild.GetAttribute("index"))
                {
                    case "0":
                        PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
                        XmlElement connector0 = data.FindFirstConnector(oldInPort0);
                        if (connector0 != null) break;

                        XmlElement cbn0 = MigrationManager.CreateCodeBlockNodeModelNode(
                            data.Document, oldNode, 0, "Point.ByCoordinates(-1,-1,-1);");
                        migrationData.AppendNode(cbn0);
                        data.CreateConnector(cbn0, 0, newNode, 0);
                        break;

                    case "1":
                        PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
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
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll", "Cuboid.ByLengths",
                "Cuboid.ByLengths@Autodesk.DesignScript.Geometry.Point,double,double,double");

            migrationData.AppendNode(newNode);

            // Add default values
            foreach (XmlNode child in oldNode.ChildNodes)
            {
                var newChild = child.Clone() as XmlElement;

                switch (newChild.GetAttribute("index"))
                {
                    case "0":
                        PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
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

    public class OnesidedEdgesAsCurveLoops : MigrationNode
    {
        // PB: deprecated
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

    public class PatchSolid : MigrationNode
    {
        // PB: deprecated
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

    public class SkinCurveLoops : MigrationNode
    {
        // PB: deprecated
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

    public class VolumeMeasure : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll",
                "Solid.Volume", "Solid.Volume");
        }
    }

}
