using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Dynamo.Connectors;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Expression = Dynamo.FScheme.Expression;
using Dynamo.FSchemeInterop;

namespace Dynamo.Elements
{
    [ElementName("Transform Identity")]
    [ElementCategory(BuiltinElementCategories.REVIT_TRANSFORMS)]
    [ElementDescription("Returns the identity transformation.")]
    [RequiresTransaction(false)]
    public class dynTransformIdentity : dynNode
    {
        public dynTransformIdentity()
        {
            OutPortData = new PortData("t", "Transform", typeof(object));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            return Expression.NewContainer(
               Transform.Identity
            );
        }
    }

    [ElementName("Transform Scale Basis")]
    [ElementCategory(BuiltinElementCategories.REVIT_TRANSFORMS)]
    [ElementDescription("Returns the identity transformation.")]
    [RequiresTransaction(false)]
    public class dynTransformScaleBasis : dynNode
    {
        public dynTransformScaleBasis()
        {
            InPortData.Add(new PortData("t", "TransformToScale(Transform)", typeof(object)));
            InPortData.Add(new PortData("d", "Scale(Number)", typeof(object)));
            OutPortData = new PortData("ts", "Transform scaled.(Transform)", typeof(object));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var transform = (Transform)((Expression.Container)args[0]).Item;
            var scale = ((Expression.Number)args[1]).Item;

            return Expression.NewContainer(
               transform.ScaleBasis(scale)
            );
        }
    }

    [ElementName("Transform Rotation")]
    [ElementCategory(BuiltinElementCategories.REVIT_TRANSFORMS)]
    [ElementDescription("Returns a transform that rotates by the specified angle about the specified axis and point.")]
    [RequiresTransaction(false)]
    public class dynTransformRotation : dynNode
    {
        public dynTransformRotation()
        {
            InPortData.Add(new PortData("or", "Origin(XYZ)", typeof(object)));
            InPortData.Add(new PortData("ax", "Axis(XYZ)", typeof(object)));
            InPortData.Add(new PortData("dn", "Angle(Number)", typeof(object)));
            OutPortData = new PortData("t", "Transform", typeof(object));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var origin = (XYZ)((Expression.Container)args[0]).Item;
            var axis = (XYZ)((Expression.Container)args[1]).Item;
            var angle = ((Expression.Number)args[2]).Item;

            return Expression.NewContainer(
               Transform.get_Rotation(origin, axis, angle)
            );
        }
    }

    [ElementName("Transform Translation")]
    [ElementCategory(BuiltinElementCategories.REVIT_TRANSFORMS)]
    [ElementDescription("Returns he transformation that translates by the specified vector.")]
    [RequiresTransaction(false)]
    public class dynTransformTranslation : dynNode
    {
        public dynTransformTranslation()
        {
            InPortData.Add(new PortData("v", "Vector(XYZ)", typeof(object)));
            OutPortData = new PortData("t", "Transform", typeof(object));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var vector = (XYZ)((Expression.Container)args[0]).Item;

            return Expression.NewContainer(
               Transform.get_Translation(vector)
            );
        }
    }

    [ElementName("Transform Reflection")]
    [ElementCategory(BuiltinElementCategories.REVIT_TRANSFORMS)]
    [ElementDescription("Returns the transformation that reflects about the specified plane.")]
    [RequiresTransaction(false)]
    public class dynTransformReflection : dynNode
    {
        public dynTransformReflection()
        {
            InPortData.Add(new PortData("pl", "Plane(Plane)", typeof(object)));
            OutPortData = new PortData("t", "Transform", typeof(object));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var plane = (Plane)((Expression.Container)args[0]).Item;

            return Expression.NewContainer(
               Transform.get_Reflection(plane)
            );
        }
    }

    [ElementName("Transform Point")]
    [ElementCategory(BuiltinElementCategories.REVIT_TRANSFORMS)]
    [ElementDescription("Transform a point with a transform.")]
    [RequiresTransaction(false)]
    public class dynTransformPoint : dynNode
    {
        public dynTransformPoint()
        {
            InPortData.Add(new PortData("t", "Transform(Plane)", typeof(object)));
            InPortData.Add(new PortData("p1", "The point(XYZ)", typeof(object)));
            OutPortData = new PortData("p2", "The transformed point.(XYZ)", typeof(object));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var t = (Transform)((Expression.Container)args[0]).Item;
            var pt = (XYZ)((Expression.Container)args[1]).Item;

            return Expression.NewContainer(
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


}
