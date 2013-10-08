using Autodesk.Revit.DB;
using Dynamo.Models;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
    [NodeName("Identity Transform")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_TRANSFORM)]
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
    }

    [NodeName("Transf From Origin and Vecs")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_TRANSFORM)]
    [NodeDescription("Returns a transformation with origin (o), up vector (u), and forward (f).")]
    public class TransformOriginAndVectors : GeometryBase
    {
        public TransformOriginAndVectors()
        {
            InPortData.Add(new PortData("o", "Origin(XYZ)", typeof(Value.Container)));
            InPortData.Add(new PortData("u", "Up(XYZ)", typeof(Value.Container)));
            InPortData.Add(new PortData("forward", "Up(XYZ)", typeof(Value.Container)));
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
    }

    [NodeName("Scale Transform")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_TRANSFORM)]
    [NodeDescription("Returns the identity transformation.")]
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
    }

    [NodeName("Rotate Transform")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_TRANSFORM)]
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
    }

    [NodeName("Translate Transform")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_TRANSFORM)]
    [NodeDescription("Returns he transformation that translates by the specified vector.")]
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
    }

    [NodeName("Reflect Transform")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_TRANSFORM)]
    [NodeDescription("Returns the transformation that reflects about the specified plane.")]
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

    [NodeName("Transform Point")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_TRANSFORM)]
    [NodeDescription("Transform a point with a transform.")]
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

        private XYZ GetPointTransformed(XYZ point, Transform transform)
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

    }

    [NodeName("Multiply Transform")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_TRANSFORM)]
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

    }

}
