using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class LoftForm : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Surface.ByLoft",
                /*NXLT*/"Surface.ByLoft@Autodesk.DesignScript.Geometry.Curve[]");

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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Autodesk.DesignScript.Geometry.Solid.BySweep",
                /*NXLT*/"Autodesk.DesignScript.Geometry.Solid.BySweep@" +
                /*NXLT*/"Autodesk.DesignScript.Geometry.Curve,Autodesk.DesignScript.Geometry.Curve");
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Curve.ExtrudeAsSolid",
                /*NXLT*/"Curve.ExtrudeAsSolid@Autodesk.DesignScript.Geometry.Vector,double");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            //append asVector Node
            XmlElement pointAsVector0 = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Point.AsVector", /*NXLT*/"Point.AsVector");
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement dsRevitNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsRevitNode,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Solid.ByLoft", /*NXLT*/"Solid.ByLoft@Curve[]");
            migratedData.AppendNode(dsRevitNode);
            string dsRevitNodeId = MigrationManager.GetGuidFromXmlElement(dsRevitNode);

            XmlElement createListNode = MigrationManager.CreateNode(data.Document,
                oldNode, 0, /*NXLT*/"DSCoreNodesUI.CreateList", "Create List");
            migratedData.AppendNode(createListNode);
            createListNode.SetAttribute(/*NXLT*/"inputcount", "2");
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Autodesk.DesignScript.Geometry.Solid.BySweep",
                /*NXLT*/"Autodesk.DesignScript.Geometry.Solid.BySweep@Autodesk.DesignScript.Geometry.PolyCurve,Autodesk.DesignScript.Geometry.PolyCurve,bool");
        }
    }

    public class BooleanOperation : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var index = oldNode.GetAttribute(/*NXLT*/"index");
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"ProtoGeometry.dll", /*NXLT*/"Solid.Difference",
                /*NXLT*/"Solid.Difference@Solid,Solid");
        }
    }

    public class SolidUnion : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"ProtoGeometry.dll", /*NXLT*/"Solid.Union",
                /*NXLT*/"Solid.Union@Solid,Solid");
        }
    }

    public class SolidIntersection : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"ProtoGeometry.dll", /*NXLT*/"Geometry.Intersect",
                /*NXLT*/"Geometry.Intersect@Geometry,Geometry");
        }
    }

    public class ElementSolid : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"RevitNodes.dll",
                /*NXLT*/"Element.Solids", /*NXLT*/"Element.Solids");
        }
    }

    public class SolidCylinder : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            var migratedData = new NodeMigrationData(data.Document);
            var oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var codeBlockNode = MigrationManager.CreateCodeBlockNodeFrom(oldNode);
            codeBlockNode.SetAttribute(/*NXLT*/"CodeText",
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {

            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Sphere.ByCenterPointRadius", /*NXLT*/"Sphere.ByCenterPointRadius@Point,double");

            migrationData.AppendNode(newNode);

            // Add default values
            foreach (XmlNode child in oldNode.ChildNodes)
            {
                var newChild = child.Clone() as XmlElement;

                switch (newChild.GetAttribute(/*NXLT*/"index"))
                {
                    case /*NXLT*/"0":
                        PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
                        XmlElement connector0 = data.FindFirstConnector(oldInPort0);
                        if (connector0 != null) break;

                        XmlElement cbn0 = MigrationManager.CreateCodeBlockNodeModelNode(
                            data.Document, oldNode, 0, /*NXLT*/"Point.ByCoordinates(0,0,0);");
                        migrationData.AppendNode(cbn0);
                        data.CreateConnector(cbn0, 0, newNode, 0);
                        break;

                    case /*NXLT*/"1":
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode,/*NXLT*/"RevitNodes.dll",
                /*NXLT*/"Solid.Torus", /*NXLT*/"Solid.Torus@Vector,Point,double,double");

            migrationData.AppendNode(newNode);

            // Add default values
            foreach (XmlNode child in oldNode.ChildNodes)
            {
                var newChild = child.Clone() as XmlElement;

                switch (newChild.GetAttribute(/*NXLT*/"index"))
                {
                    case /*NXLT*/"0":
                        PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
                        XmlElement connector0 = data.FindFirstConnector(oldInPort0);
                        if (connector0 != null) break;

                        XmlElement zAxis0 = MigrationManager.CreateFunctionNode(
                            data.Document, oldNode, 0,/*NXLT*/"ProtoGeometry.dll",
                            /*NXLT*/"Vector.ZAxis", /*NXLT*/"Vector.ZAxis");
                        migrationData.AppendNode(zAxis0);
                        data.CreateConnector(zAxis0, 0, newNode, 0);
                        break;

                    case /*NXLT*/"1":
                        PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
                        XmlElement connector1 = data.FindFirstConnector(oldInPort1);
                        if (connector1 != null) break;

                        XmlElement cbn1 = MigrationManager.CreateCodeBlockNodeModelNode(
                            data.Document, oldNode, 1, /*NXLT*/"Point.ByCoordinates(0,0,1);");
                        migrationData.AppendNode(cbn1);
                        data.CreateConnector(cbn1, 0, newNode, 1);
                        break;

                    case /*NXLT*/"2":
                    case /*NXLT*/"3":
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Cuboid.ByCorners", /*NXLT*/"Cuboid.ByCorners@Point,Point");

            migrationData.AppendNode(newNode);

            // Add default values
            foreach (XmlNode child in oldNode.ChildNodes)
            {
                var newChild = child.Clone() as XmlElement;

                switch (newChild.GetAttribute(/*NXLT*/"index"))
                {
                    case /*NXLT*/"0":
                        PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
                        XmlElement connector0 = data.FindFirstConnector(oldInPort0);
                        if (connector0 != null) break;

                        XmlElement cbn0 = MigrationManager.CreateCodeBlockNodeModelNode(
                            data.Document, oldNode, 0, /*NXLT*/"Point.ByCoordinates(-1,-1,-1);");
                        migrationData.AppendNode(cbn0);
                        data.CreateConnector(cbn0, 0, newNode, 0);
                        break;

                    case /*NXLT*/"1":
                        PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
                        XmlElement connector1 = data.FindFirstConnector(oldInPort1);
                        if (connector1 != null) break;

                        XmlElement cbn1 = MigrationManager.CreateCodeBlockNodeModelNode(
                            data.Document, oldNode, 1, /*NXLT*/"Point.ByCoordinates(1,1,1);");
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode,/*NXLT*/"ProtoGeometry.dll", /*NXLT*/"Cuboid.ByLengths",
                /*NXLT*/"Cuboid.ByLengths@Autodesk.DesignScript.Geometry.Point,double,double,double");

            migrationData.AppendNode(newNode);

            // Add default values
            foreach (XmlNode child in oldNode.ChildNodes)
            {
                var newChild = child.Clone() as XmlElement;

                switch (newChild.GetAttribute(/*NXLT*/"index"))
                {
                    case /*NXLT*/"0":
                        PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
                        XmlElement connector0 = data.FindFirstConnector(oldInPort0);
                        if (connector0 != null) break;

                        XmlElement cbn = MigrationManager.CreateCodeBlockNodeModelNode(
                            data.Document, oldNode, 0, /*NXLT*/"Point.ByCoordinates(0,0,0);");
                        migrationData.AppendNode(cbn);
                        data.CreateConnector(cbn, 0, newNode, 0);
                        break;

                    case /*NXLT*/"1":
                    case /*NXLT*/"2":
                    case /*NXLT*/"3":
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"ProtoGeometry.dll", /*NXLT*/"Geometry.Explode", /*NXLT*/"Geometry.Explode");
        }
    }

    public class TransformSolid : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"ProtoGeometry.dll", /*NXLT*/"Geometry.Transform",
                /*NXLT*/"Geometry.Transform@CoordinateSystem");
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Solid.Volume", /*NXLT*/"Solid.Volume");
        }
    }

}
