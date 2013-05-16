using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Dynamo.Connectors;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using Dynamo.FSchemeInterop;

namespace Dynamo.Nodes
{
    [NodeName("Identity Transf")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_TRANSFORM)]
    [NodeDescription("Returns the identity transformation.")]
    public class dynTransformIdentity: dynNodeWithOneOutput
    {
        public dynTransformIdentity()
        {
            OutPortData.Add(new PortData("t", "Transform", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewContainer(
               Transform.Identity
            );
        }
    }

    [NodeName("Transf From Origin and Vecs")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_TRANSFORM)]
    [NodeDescription("Returns a transformation with origin (o), up vector (u), and forward (f).")]
    public class dynTransformOriginAndVectors: dynNodeWithOneOutput
    {
        public dynTransformOriginAndVectors()
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
            t.BasisZ = up;
            t.BasisY = forward;
            t.BasisX = forward.CrossProduct(up);
            
            return Value.NewContainer(
               t
            );
        }
    }

    [NodeName("Scale Transf")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_TRANSFORM)]
    [NodeDescription("Returns the identity transformation.")]
    public class dynTransformScaleBasis: dynNodeWithOneOutput
    {
        public dynTransformScaleBasis()
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

            return Value.NewContainer(
               transform.ScaleBasis(scale)
            );
        }
    }

    [NodeName("Rotate Transf")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_TRANSFORM)]
    [NodeDescription("Returns a transform that rotates by the specified angle about the specified axis and point.")]
    public class dynTransformRotation: dynNodeWithOneOutput
    {
        public dynTransformRotation()
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

            return Value.NewContainer(
               Transform.get_Rotation(origin, axis, angle)
            );
        }
    }

    [NodeName("Translate Transf")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_TRANSFORM)]
    [NodeDescription("Returns he transformation that translates by the specified vector.")]
    public class dynTransformTranslation: dynNodeWithOneOutput
    {
        public dynTransformTranslation()
        {
            InPortData.Add(new PortData("v", "Vector(XYZ)", typeof(Value.Container)));
            OutPortData.Add(new PortData("t", "Transform", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var vector = (XYZ)((Value.Container)args[0]).Item;

            return Value.NewContainer(
               Transform.get_Translation(vector)
            );
        }
    }

    [NodeName("Reflect Transf")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_TRANSFORM)]
    [NodeDescription("Returns the transformation that reflects about the specified plane.")]
    public class dynTransformReflection: dynNodeWithOneOutput
    {
        public dynTransformReflection()
        {
            InPortData.Add(new PortData("pl", "Plane(Plane)", typeof(Value.Container)));
            OutPortData.Add(new PortData("t", "Transform", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var plane = (Plane)((Value.Container)args[0]).Item;

            return Value.NewContainer(
               Transform.get_Reflection(plane)
            );
        }
    }

    [NodeName("Transf Point")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_TRANSFORM)]
    [NodeDescription("Transform a point with a transform.")]
    public class dynTransformPoint: dynNodeWithOneOutput
    {
        public dynTransformPoint()
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

            return Value.NewContainer(
               TransformPoint(pt, t)
            );
        }

        private XYZ TransformPoint(XYZ point, Transform transform)
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

    [NodeName("Multiply Transf")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_TRANSFORM)]
    [NodeDescription("Multiply two transforms.")]
    public class Multiplytransform : dynNodeWithOneOutput
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

            return Value.NewContainer(
               t1.Multiply(t2));
        }

    }

    [NodeName("Compute Face Derivs")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_SURFACE)]
    [NodeDescription("Returns a transform describing the face (f) at the parameter (uv).")]
    public class dynComputeFaceDerivatives: dynNodeWithOneOutput
    {
        public dynComputeFaceDerivatives()
        {
            InPortData.Add(new PortData("f", "The face to evaluate(Face)", typeof(Value.Container)));
            InPortData.Add(new PortData("uv", "The parameter to evaluate(UV)", typeof(Value.Container)));
            OutPortData.Add(new PortData("t", "Transform describing the face at the parameter(Transform)", typeof(Value.Container)));
        
            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var faceRef = (Reference)((Value.Container)args[0]).Item;
            var uv = (UV)((Value.Container)args[1]).Item;

            Transform t = Transform.Identity;

            Face f = dynRevitSettings.Doc.Document.GetElement(faceRef.ElementId).GetGeometryObjectFromReference(faceRef) as Face;
            if (f != null)
            {
                t = f.ComputeDerivatives(uv);
                t.BasisX = t.BasisX.Normalize();
                t.BasisZ = t.BasisZ.Normalize();
                t.BasisY = t.BasisX.CrossProduct(t.BasisZ);
            }
            return Value.NewContainer(
               t
            );
        }

    }
}
