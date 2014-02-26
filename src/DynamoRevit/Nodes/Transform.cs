using Autodesk.Revit.DB;
using Dynamo.Models;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using System.Xml;
using System.Linq;

namespace Dynamo.Nodes
{
    [NodeName("Identity Transform")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_TRANSFORM_CREATE)]
    [NodeDescription("Returns the identity transformation.")]
    public class TransformIdentity: GeometryBase
    {
        public TransformIdentity()
        {
            OutPortData.Add(new PortData("t", "Transform", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Transform t = Transform.Identity;

            return Value.NewContainer(t);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "CoordinateSystem.Identity",
                "CoordinateSystem.Identity");
        }
    }

    [NodeName("Transform from Origin and Vectors")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_TRANSFORM_CREATE)]
    [NodeDescription("Returns a transformation with origin (o), up vector (u), and forward (f).")]
    [NodeSearchTags("move", "copy")]
    public class TransformOriginAndVectors : GeometryBase
    {
        public TransformOriginAndVectors()
        {
            InPortData.Add(new PortData("o", "Origin(XYZ)", typeof(Value.Container)));
            InPortData.Add(new PortData("u", "Up(XYZ)", typeof(Value.Container)));
            InPortData.Add(new PortData("forward", "Forward(XYZ)", typeof(Value.Container)));
            OutPortData.Add(new PortData("t", "Transform", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var origin = (XYZ)((Value.Container)args[0]).Item;
            var up = (XYZ)((Value.Container)args[1]).Item;
            var forward = (XYZ)((Value.Container)args[2]).Item;

            Transform t = Transform.Identity;
            t.Origin = origin;
            t.BasisZ = up.Normalize();
            t.BasisY = forward.Normalize();
            t.BasisX = forward.CrossProduct(up).Normalize();

            return Value.NewContainer(
               t
            );
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement coordinateNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(coordinateNode, "ProtoGeometry.dll",
                "CoordinateSystem.ByOriginVectors",
                "CoordinateSystem.ByOriginVectors@Autodesk.DesignScript.Geometry.Point," + 
                "Autodesk.DesignScript.Geometry.Vector,Autodesk.DesignScript.Geometry.Vector," + 
                "Autodesk.DesignScript.Geometry.Vector");

            migratedData.AppendNode(coordinateNode);
            string coordinateNodeId = MigrationManager.GetGuidFromXmlElement(coordinateNode);

            XmlElement asVectorNode0 = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll", "Point.AsVector", "Point.AsVector");
            migratedData.AppendNode(asVectorNode0);
            string asVectorNode0Id = MigrationManager.GetGuidFromXmlElement(asVectorNode0);

            XmlElement asVectorNode1 = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll", "Point.AsVector", "Point.AsVector");
            migratedData.AppendNode(asVectorNode1);
            string asVectorNode1Id = MigrationManager.GetGuidFromXmlElement(asVectorNode1);

            XmlElement vectorCrossNode = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll", "Vector.Cross", "Vector.Cross@Vector");
            migratedData.AppendNode(vectorCrossNode);
            string vectorCrossNodeId = MigrationManager.GetGuidFromXmlElement(vectorCrossNode);

            XmlElement vectorReverseNode = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll", "Vector.Reverse", "Vector.Reverse");
            migratedData.AppendNode(vectorReverseNode);
            string vectorReverseNodeId = MigrationManager.GetGuidFromXmlElement(vectorReverseNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.INPUT);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId oldInPort2 = new PortId(oldNodeId, 2, PortType.INPUT);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            PortId newInPort0 = new PortId(coordinateNodeId, 0, PortType.INPUT);
            PortId newInPort1 = new PortId(coordinateNodeId, 1, PortType.INPUT);
            PortId newInPort2 = new PortId(coordinateNodeId, 2, PortType.INPUT);
            PortId newInPort3 = new PortId(coordinateNodeId, 3, PortType.INPUT);

            PortId newInPort4 = new PortId(asVectorNode0Id, 0, PortType.INPUT);
            PortId newInPort5 = new PortId(asVectorNode1Id, 0, PortType.INPUT);

            data.ReconnectToPort(connector0, newInPort0);
            data.ReconnectToPort(connector1, newInPort4);
            data.ReconnectToPort(connector2, newInPort5);

            data.CreateConnector(asVectorNode0, 0, vectorCrossNode, 0);
            data.CreateConnector(asVectorNode1, 0, vectorCrossNode, 1);
            data.CreateConnector(vectorCrossNode, 0, vectorReverseNode, 0);
            data.CreateConnector(vectorReverseNode, 0, coordinateNode, 1);
            data.CreateConnector(asVectorNode1, 0, coordinateNode, 2);
            data.CreateConnector(asVectorNode0, 0, coordinateNode, 3);

            return migratedData;
        }
    }

    [NodeName("Transform From-To")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_TRANSFORM_CREATE)]
    [NodeDescription("Returns a transformation from origin, up, forward vectors to another triple of vectors. Normalizes vectors if needed.")]
    [NodeSearchTags("move", "copy")]
    public class TransFromTo : GeometryBase
    {
        public TransFromTo()
        {
            InPortData.Add(new PortData("o-from", "Origin(XYZ)", typeof(Value.Container)));
            InPortData.Add(new PortData("u-from", "Up(XYZ)", typeof(Value.Container)));
            InPortData.Add(new PortData("f-from", "Up(XYZ)", typeof(Value.Container)));
            InPortData.Add(new PortData("o-to", "Origin(XYZ)", typeof(Value.Container)));
            InPortData.Add(new PortData("u-to", "Up(XYZ)", typeof(Value.Container)));
            InPortData.Add(new PortData("f-to", "Up(XYZ)", typeof(Value.Container)));
            OutPortData.Add(new PortData("t", "Transform", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var originF = (XYZ)((Value.Container)args[0]).Item;
            var upF = (XYZ)((Value.Container)args[1]).Item;
            var forwardF = (XYZ)((Value.Container)args[2]).Item;

            var originT = (XYZ)((Value.Container)args[3]).Item;
            var upT = (XYZ)((Value.Container)args[4]).Item;
            var forwardT = (XYZ)((Value.Container)args[5]).Item;

            Transform tF = Transform.Identity;
            tF.Origin = originF;
            tF.BasisZ = upF.Normalize();
            tF.BasisY = forwardF.Normalize();
            tF.BasisX = forwardF.CrossProduct(upF).Normalize();
            tF.BasisY = tF.BasisZ.CrossProduct(tF.BasisX).Normalize();

            Transform tT = Transform.Identity;
            tT.Origin = originT;
            tT.BasisZ = upT.Normalize();
            tT.BasisY = forwardT.Normalize();
            tT.BasisX = forwardT.CrossProduct(upT).Normalize();
            tT.BasisY = tT.BasisZ.CrossProduct(tT.BasisX).Normalize();

            Transform t = tT.Multiply(tF.Inverse);

            return Value.NewContainer(
               t
            );
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "CoordinateSystem.PostMultiplyBy",
                "CoordinateSystem.PostMultiplyBy@CoordinateSystem");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new nodes
            XmlElement vectorCrossFrom = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll",
                "Vector.Cross", "Vector.Cross@Vector");
            migrationData.AppendNode(vectorCrossFrom);
            string vectorCrossFromId = MigrationManager.GetGuidFromXmlElement(vectorCrossFrom);

            XmlElement vectorCrossTo = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll",
                "Vector.Cross", "Vector.Cross@Vector");
            migrationData.AppendNode(vectorCrossTo);
            string vectorCrossToId = MigrationManager.GetGuidFromXmlElement(vectorCrossTo);

            XmlElement csFrom = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll", "CoordinateSystem.ByOriginVectors",
                "CoordinateSystem.ByOriginVectors@Autodesk.DesignScript.Geometry.Point,"
                + "Autodesk.DesignScript.Geometry.Vector,Autodesk.DesignScript.Geometry"
                + ".Vector,Autodesk.DesignScript.Geometry.Vector");
            string csFromId = MigrationManager.GetGuidFromXmlElement(csFrom);
            migrationData.AppendNode(csFrom);

            XmlElement csTo = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll", "CoordinateSystem.ByOriginVectors",
                "CoordinateSystem.ByOriginVectors@Autodesk.DesignScript.Geometry.Point,"
                + "Autodesk.DesignScript.Geometry.Vector,Autodesk.DesignScript.Geometry"
                + ".Vector,Autodesk.DesignScript.Geometry.Vector");
            string csToId = MigrationManager.GetGuidFromXmlElement(csTo);
            migrationData.AppendNode(csTo);

            XmlElement csInverse = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll",
                "CoordinateSystem.Inverse", "CoordinateSystem.Inverse");
            migrationData.AppendNode(csInverse);

            // Update connectors
            PortId csFrom0 = new PortId(csFromId, 0, PortType.INPUT);
            PortId csFrom1 = new PortId(csFromId, 1, PortType.INPUT);
            PortId csFrom2 = new PortId(csFromId, 2, PortType.INPUT);
            PortId csFrom3 = new PortId(csFromId, 3, PortType.INPUT);
            PortId csTo0 = new PortId(csToId, 0, PortType.INPUT);
            PortId csTo1 = new PortId(csToId, 1, PortType.INPUT);
            PortId csTo2 = new PortId(csToId, 2, PortType.INPUT);
            PortId csTo3 = new PortId(csToId, 3, PortType.INPUT);

            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.INPUT);
            PortId oldInPort2 = new PortId(newNodeId, 2, PortType.INPUT);
            PortId oldInPort3 = new PortId(newNodeId, 3, PortType.INPUT);
            PortId oldInPort4 = new PortId(newNodeId, 4, PortType.INPUT);
            PortId oldInPort5 = new PortId(newNodeId, 5, PortType.INPUT);

            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);
            XmlElement connector3 = data.FindFirstConnector(oldInPort3);
            XmlElement connector4 = data.FindFirstConnector(oldInPort4);
            XmlElement connector5 = data.FindFirstConnector(oldInPort5);

            if (connector1 != null)
            {
                string uFromId = connector1.GetAttribute("start").ToString();
                data.CreateConnectorFromId(uFromId, 0, vectorCrossFromId, 1);
            }

            if (connector2 != null)
            {
                string fFromId = connector2.GetAttribute("start").ToString();
                data.CreateConnectorFromId(fFromId, 0, vectorCrossFromId, 0);
            }

            if (connector4 != null)
            {
                string uToId = connector4.GetAttribute("start").ToString();
                data.CreateConnectorFromId(uToId, 0, vectorCrossToId, 1);
            }

            if (connector5 != null)
            {
                string fToId = connector5.GetAttribute("start").ToString();
                data.CreateConnectorFromId(fToId, 0, vectorCrossToId, 0);
            }

            data.ReconnectToPort(connector0, csFrom0);
            data.ReconnectToPort(connector1, csFrom3);
            data.ReconnectToPort(connector2, csFrom2);
            data.ReconnectToPort(connector3, csTo0);
            data.ReconnectToPort(connector4, csTo3);
            data.ReconnectToPort(connector5, csTo2);

            data.CreateConnector(vectorCrossFrom, 0, csFrom, 1);
            data.CreateConnector(vectorCrossTo, 0, csTo, 1);
            data.CreateConnector(csFrom, 0, csInverse, 0);
            data.CreateConnector(csInverse, 0, newNode, 1);
            data.CreateConnector(csTo, 0, newNode, 0);

            return migrationData;
        }
    }


    [NodeName("Scale Transform")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_TRANSFORM_CREATE)]
    [NodeDescription("Returns the scale transformation.")]
    public class TransformScaleBasis : GeometryBase
    {
        public TransformScaleBasis()
        {
            InPortData.Add(new PortData("t", "TransformToScale(Transform)", typeof(Value.Container)));
            InPortData.Add(new PortData("d", "Scale(Number)", typeof(Value.Number)));
            OutPortData.Add(new PortData("ts", "Transform scaled.(Transform)", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var transform = (Transform)((Value.Container)args[0]).Item;
            var scale = ((Value.Number)args[1]).Item;

            Transform t = transform.ScaleBasis(scale);

            return Value.NewContainer(t);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "CoordinateSystem.Scale",
                "CoordinateSystem.Scale@double");
        }
    }

    [NodeName("Rotate Transform")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_TRANSFORM_CREATE)]
    [NodeDescription("Returns a transform that rotates by the specified angle about the specified axis and point.")]
    public class TransformRotation : GeometryBase
    {
        public TransformRotation()
        {
            InPortData.Add(new PortData("or", "Origin(XYZ)", typeof(Value.Container)));
            InPortData.Add(new PortData("ax", "Axis(XYZ)", typeof(Value.Container)));
            InPortData.Add(new PortData("dn", "Angle(Number)", typeof(Value.Number)));
            OutPortData.Add(new PortData("t", "Transform", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var origin = (XYZ)((Value.Container)args[0]).Item;
            var axis = (XYZ)((Value.Container)args[1]).Item;
            var angle = ((Value.Number)args[2]).Item;

            Transform t = Transform.get_Rotation(origin, axis, angle);

            return Value.NewContainer(t);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "CoordinateSystem.Rotate", "CoordinateSystem.Rotate@Point,Vector,double");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new node
            XmlElement identityCoordinateSystem = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll",
                "CoordinateSystem.Identity",
                "CoordinateSystem.Identity");
            migrationData.AppendNode(identityCoordinateSystem);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.INPUT);
            PortId oldInPort2 = new PortId(newNodeId, 2, PortType.INPUT);

            PortId newInPort1 = new PortId(newNodeId, 1, PortType.INPUT);
            PortId newInPort2 = new PortId(newNodeId, 2, PortType.INPUT);
            PortId newInPort3 = new PortId(newNodeId, 3, PortType.INPUT);

            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            data.ReconnectToPort(connector0, newInPort1);
            data.ReconnectToPort(connector1, newInPort2);
            data.ReconnectToPort(connector2, newInPort3);

            data.CreateConnector(identityCoordinateSystem, 0, newNode, 0);

            return migrationData;
        }
    }

    [NodeName("Translate Transform")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_TRANSFORM_CREATE)]
    [NodeDescription("Returns he transformation that translates by the specified vector.")]
    [NodeSearchTags("copy", "move")]
    public class TransformTranslation : GeometryBase
    {
        public TransformTranslation()
        {
            InPortData.Add(new PortData("v", "Vector(XYZ)", typeof(Value.Container)));
            OutPortData.Add(new PortData("t", "Transform", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var vector = (XYZ)((Value.Container)args[0]).Item;

            Transform t = Transform.get_Translation(vector);

            return Value.NewContainer(t);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "CoordinateSystem.Translate",
                "CoordinateSystem.Translate@Autodesk.DesignScript.Geometry.Vector");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new node
            XmlElement identityCoordinateSystem = MigrationManager.CreateFunctionNode(
                data.Document, "ProtoGeometry.dll",
                "CoordinateSystem.Identity",
                "CoordinateSystem.Identity");
            migrationData.AppendNode(identityCoordinateSystem);

            // Update connectors
            PortId oldInPort0 = new PortId(newNodeId, 0, PortType.INPUT);
            PortId newInPort1 = new PortId(newNodeId, 1, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            data.ReconnectToPort(connector0, newInPort1);
            data.CreateConnector(identityCoordinateSystem, 0, newNode, 0);

            return migrationData;
        }
    }

    [NodeName("Reflect Transform")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_TRANSFORM_CREATE)]
    [NodeDescription("Returns the transformation that reflects about the specified plane.")]
    [NodeSearchTags("mirror", "symmetric")]
    public class TransformReflection : GeometryBase
    {
        public TransformReflection()
        {
            InPortData.Add(new PortData("pl", "Plane(Plane)", typeof(Value.Container)));
            OutPortData.Add(new PortData("t", "Transform", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var plane = (Autodesk.Revit.DB.Plane)((Value.Container)args[0]).Item;

            Transform t = Transform.get_Reflection(plane);

            return Value.NewContainer(t);
        }
    }

    [NodeName("Transform XYZ")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_TRANSFORM_APPLY)]
    [NodeDescription("Transform a point with a transform.")]
    [NodeSearchTags("move", "copy")]
    public class TransformPoint : GeometryBase
    {
        public TransformPoint()
        {
            InPortData.Add(new PortData("t", "Transform(Plane)", typeof(Value.Container)));
            InPortData.Add(new PortData("p1", "The point(XYZ)", typeof(Value.Container)));
            OutPortData.Add(new PortData("p2", "The transformed point.(XYZ)", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var t = (Transform)((Value.Container)args[0]).Item;
            var pt = (XYZ)((Value.Container)args[1]).Item;

            XYZ tpt = GetPointTransformed(pt, t);

            return Value.NewContainer(tpt);
        }

        public static XYZ GetPointTransformed(XYZ point, Transform transform)
        {
            double x = point.X;
            double y = point.Y;
            double z = point.Z;

            //transform basis of the old coordinate system in the new coordinate // system
            XYZ b0 = transform.get_Basis(0);
            XYZ b1 = transform.get_Basis(1);
            XYZ b2 = transform.get_Basis(2);
            XYZ origin = transform.Origin;

            //transform the origin of the old coordinate system in the new 
            //coordinate system
            double xTemp = x * b0.X + y * b1.X + z * b2.X + origin.X;
            double yTemp = x * b0.Y + y * b1.Y + z * b2.Y + origin.Y;
            double zTemp = x * b0.Z + y * b1.Z + z * b2.Z + origin.Z;

            return new XYZ(xTemp, yTemp, zTemp);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Geometry.Translate",
                "Geometry.Translate@Autodesk.DesignScript.Geometry.Vector");
        }
    }

    [NodeName("Multiply Transform")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_TRANSFORM_MODIFY)]
    [NodeDescription("Multiply two transforms.")]
    public class Multiplytransform : GeometryBase
    {
        public Multiplytransform()
        {
            InPortData.Add(new PortData("t1", "The first transform", typeof(Value.Container)));
            InPortData.Add(new PortData("t2", "The second transform", typeof(Value.Container)));
            OutPortData.Add(new PortData("transform", "The transform which is the result of multiplication.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var t1 = (Transform)((Value.Container)args[0]).Item;
            var t2 = (Transform)((Value.Container)args[1]).Item;

            Transform t = t1.Multiply(t2);

            return Value.NewContainer(t);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "CoordinateSystem.PostMultiplyBy",
                "CoordinateSystem.PostMultiplyBy@CoordinateSystem");
        }
    }

    [NodeName("Transform to Curve Point")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_TRANSFORM_MODIFY)]
    [NodeDescription("Returns a transformation of XY plane to plane at point on curve perpendicular to curve tangent direction.")]
    [NodeSearchTags("move", "copy")]
    public class TransToCurve : GeometryBase
    {
        public TransToCurve()
        {
            InPortData.Add(new PortData("c", "Curve(Curve)", typeof(Value.Container)));
            InPortData.Add(new PortData("p", "Raw Parameter(Number)", typeof(Value.Container)));
            OutPortData.Add(new PortData("t", "Transform to point on Curve", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {

            Curve crv = (Curve)((Value.Container)args[0]).Item;
            double parameter = ((Value.Number)args[1]).Item;


            Transform tCurve = crv.ComputeDerivatives(parameter, false);
             

            Transform tF = Transform.Identity;
            tF.Origin = tCurve.Origin;
            tF.BasisZ = tCurve.BasisX.Normalize();

            tF.BasisX = XYZ.BasisZ.CrossProduct(tF.BasisZ);
            if (tF.BasisX.IsZeroLength())
               tF.BasisX = XYZ.BasisX;
            tF.BasisY = tF.BasisZ.CrossProduct(tF.BasisX);

            return Value.NewContainer(
               tF
            );
        }
    }

    [NodeName("Inverse Transform")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_TRANSFORM_MODIFY)]
    [NodeDescription("Returns the inverse transformation.")]
    public class InverseTransform : GeometryBase
    {
        public InverseTransform()
        {
            InPortData.Add(new PortData("t", "TransformToInverse(Transform)", typeof(Value.Container)));
            OutPortData.Add(new PortData("ts", "Inversed Transform. (Transform)", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {

            var transform = (Transform)((Value.Container)args[0]).Item;

            Transform t = transform.Inverse;

            return Value.NewContainer(t);

        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "CoordinateSystem.Inverse", "CoordinateSystem.Inverse");
        }
    }

    [NodeName("Transform Basis X")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_TRANSFORM_MODIFY)]
    [NodeDescription("Returns the x basis vector of the transform.")]
    public class BasisX : GeometryBase
    {
        public BasisX()
        {
            InPortData.Add(new PortData("transform", "Transform.", typeof(Value.Container)));
            OutPortData.Add(new PortData("xyz", "Basis X.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var transform = (Transform)((Value.Container)args[0]).Item;
            return Value.NewContainer(transform.BasisX);
        }
    }

    [NodeName("Transform Basis Y")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_TRANSFORM_MODIFY)]
    [NodeDescription("Returns the y basis vector of the transform.")]
    public class BasisY : GeometryBase
    {
        public BasisY()
        {
            InPortData.Add(new PortData("transform", "Transform.", typeof(Value.Container)));
            OutPortData.Add(new PortData("xyz", "Basis Y.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var transform = (Transform)((Value.Container)args[0]).Item;
            return Value.NewContainer(transform.BasisY);
        }
    }

    [NodeName("Transform Basis Z")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_TRANSFORM_MODIFY)]
    [NodeDescription("Returns the z basis vector of the transform.")]
    public class BasisZ : GeometryBase
    {
        public BasisZ()
        {
            InPortData.Add(new PortData("transform", "Transform.", typeof(Value.Container)));
            OutPortData.Add(new PortData("xyz", "Basis Z.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var transform = (Transform)((Value.Container)args[0]).Item;
            return Value.NewContainer(transform.BasisZ);
        }
    }

    [NodeName("Transform Origin")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_TRANSFORM_MODIFY)]
    [NodeDescription("Returns the z basis vector of the transform.")]
    public class Origin : GeometryBase
    {
        public Origin()
        {
            InPortData.Add(new PortData("transform", "Transform.", typeof(Value.Container)));
            OutPortData.Add(new PortData("xyz", "Origin.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var transform = (Transform)((Value.Container)args[0]).Item;
            return Value.NewContainer(transform.Origin);
        }
    }
}
