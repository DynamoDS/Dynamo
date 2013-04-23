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
    [NodeName("Transform Identity")]
    [NodeCategory(BuiltinNodeCategories.REVIT_TRANSFORMS)]
    [NodeDescription("Returns the identity transformation.")]
    public class dynTransformIdentity: dynNodeWithOneOutput
    {
        public dynTransformIdentity()
        {
            OutPortData.Add(new PortData("t", "Transform", typeof(object)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewContainer(
               Transform.Identity
            );
        }
    }

    [NodeName("Transform From Origin and Vectors")]
    [NodeCategory(BuiltinNodeCategories.REVIT_TRANSFORMS)]
    [NodeDescription("Returns a transformation with origin (o), up vector (u), and forward (f).")]
    public class dynTransformOriginAndVectors: dynNodeWithOneOutput
    {
        public dynTransformOriginAndVectors()
        {
            InPortData.Add(new PortData("o", "Origin(XYZ)", typeof(object)));
            InPortData.Add(new PortData("u", "Up(XYZ)", typeof(object)));
            InPortData.Add(new PortData("forward", "Up(XYZ)", typeof(object)));
            OutPortData.Add(new PortData("t", "Transform", typeof(object)));

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

    [NodeName("Transform Scale Basis")]
    [NodeCategory(BuiltinNodeCategories.REVIT_TRANSFORMS)]
    [NodeDescription("Returns the identity transformation.")]
    public class dynTransformScaleBasis: dynNodeWithOneOutput
    {
        public dynTransformScaleBasis()
        {
            InPortData.Add(new PortData("t", "TransformToScale(Transform)", typeof(object)));
            InPortData.Add(new PortData("d", "Scale(Number)", typeof(object)));
            OutPortData.Add(new PortData("ts", "Transform scaled.(Transform)", typeof(object)));

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

    [NodeName("Transform Rotation")]
    [NodeCategory(BuiltinNodeCategories.REVIT_TRANSFORMS)]
    [NodeDescription("Returns a transform that rotates by the specified angle about the specified axis and point.")]
    public class dynTransformRotation: dynNodeWithOneOutput
    {
        public dynTransformRotation()
        {
            InPortData.Add(new PortData("or", "Origin(XYZ)", typeof(object)));
            InPortData.Add(new PortData("ax", "Axis(XYZ)", typeof(object)));
            InPortData.Add(new PortData("dn", "Angle(Number)", typeof(object)));
            OutPortData.Add(new PortData("t", "Transform", typeof(object)));

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

    [NodeName("Transform Translation")]
    [NodeCategory(BuiltinNodeCategories.REVIT_TRANSFORMS)]
    [NodeDescription("Returns he transformation that translates by the specified vector.")]
    public class dynTransformTranslation: dynNodeWithOneOutput
    {
        public dynTransformTranslation()
        {
            InPortData.Add(new PortData("v", "Vector(XYZ)", typeof(object)));
            OutPortData.Add(new PortData("t", "Transform", typeof(object)));

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

    [NodeName("Transform Reflection")]
    [NodeCategory(BuiltinNodeCategories.REVIT_TRANSFORMS)]
    [NodeDescription("Returns the transformation that reflects about the specified plane.")]
    public class dynTransformReflection: dynNodeWithOneOutput
    {
        public dynTransformReflection()
        {
            InPortData.Add(new PortData("pl", "Plane(Plane)", typeof(object)));
            OutPortData.Add(new PortData("t", "Transform", typeof(object)));

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

    [NodeName("Transform Point")]
    [NodeCategory(BuiltinNodeCategories.REVIT_TRANSFORMS)]
    [NodeDescription("Transform a point with a transform.")]
    public class dynTransformPoint: dynNodeWithOneOutput
    {
        public dynTransformPoint()
        {
            InPortData.Add(new PortData("t", "Transform(Plane)", typeof(object)));
            InPortData.Add(new PortData("p1", "The point(XYZ)", typeof(object)));
            OutPortData.Add(new PortData("p2", "The transformed point.(XYZ)", typeof(object)));

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

    [NodeName("Face Compute Derivatives")]
    [NodeCategory(BuiltinNodeCategories.REVIT_TRANSFORMS)]
    [NodeDescription("Returns a transform describing the face (f) at the parameter (uv).")]
    public class dynFaceComputerDerivative: dynNodeWithOneOutput
    {
        public dynFaceComputerDerivative()
        {
            InPortData.Add(new PortData("f", "The face to evaluate(Face)", typeof(object)));
            InPortData.Add(new PortData("uv", "The parameter to evaluate(UV)", typeof(object)));
            OutPortData.Add(new PortData("t", "Transform describing the face at the parameter(Transform)", typeof(object)));
        
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
