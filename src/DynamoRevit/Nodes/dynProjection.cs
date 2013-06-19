using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

using Autodesk.Revit;
using Autodesk.Revit.DB;

using Microsoft.FSharp.Collections;

using Value = Dynamo.FScheme.Value;
using Dynamo.FSchemeInterop;
using Dynamo.Revit;
using Dynamo.Connectors;
using Dynamo.Utilities;

namespace Dynamo.Nodes
{
    [NodeName("Project Point On Curve")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_INTERSECT)]
    [NodeDescription("Project a point onto a curve.")]
    public class dynProjectPointOnCurve : dynRevitTransactionNode, IDrawable, IClearable
    {
        public dynProjectPointOnCurve()
        {
            InPortData.Add(new PortData("xyz", "The point to be projected.", typeof(Value.Container)));
            InPortData.Add(new PortData("crv", "The curve on which to project the point.", typeof(Value.Container)));

            OutPortData.Add(new PortData("xyz", "The nearest point on the curve.", typeof(Value.Container)));
            OutPortData.Add(new PortData("t", "The unnormalized parameter on the curve.", typeof(Value.Number)));
            OutPortData.Add(new PortData("d", "The distance from the point to the curve .", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var xyz = (XYZ)((Value.Container)args[0]).Item;
            var crv = (Curve)((Value.Container)args[1]).Item;

            IntersectionResult ir = crv.Project(xyz);
            XYZ pt = ir.XYZPoint;
            double t = ir.Parameter;
            double d = ir.Distance;

            var results = FSharpList<Value>.Empty;
            results = FSharpList<Value>.Cons(Value.NewNumber(d), results);
            results = FSharpList<Value>.Cons(Value.NewNumber(t), results);
            results = FSharpList<Value>.Cons(Value.NewContainer(pt), results);

            pts.Add(pt);

            return Value.NewList(results);
        }

        protected List<XYZ> pts = new List<XYZ>();
        public RenderDescription RenderDescription { get; set; }
        public void Draw()
        {
            if (this.RenderDescription == null)
                this.RenderDescription = new RenderDescription();
            else
                this.RenderDescription.ClearAll();

            foreach (XYZ pt in pts)
                this.RenderDescription.points.Add(new Point3D(pt.X, pt.Y, pt.Z));
        }

        public void ClearReferences()
        {
            pts.Clear();
        }
    }

    [NodeName("Project Point On Face")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_INTERSECT)]
    [NodeDescription("Project a point onto a face.")]
    public class dynProjectPointOnFace : dynRevitTransactionNode, IDrawable, IClearable
    {
        public dynProjectPointOnFace()
        {
            InPortData.Add(new PortData("xyz", "The point to be projected.", typeof(Value.Container)));
            InPortData.Add(new PortData("face", "The face on which to project the point.", typeof(Value.Container)));

            OutPortData.Add(new PortData("xyz", "The nearest point to the projected point on the face.", typeof(Value.Container)));
            OutPortData.Add(new PortData("uv", "The UV coordinates of the nearest point on the face..", typeof(Value.Number)));
            OutPortData.Add(new PortData("d", "The distance from the point to the face", typeof(Value.Number)));
            OutPortData.Add(new PortData("edge", "The edge if projected point is near an edge.", typeof(Value.Container)));
            OutPortData.Add(new PortData("edge t", "The parameter of the nearest point on the edge.", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var xyz = (XYZ)((Value.Container)args[0]).Item;
            var face = (Autodesk.Revit.DB.Face)((Value.Container)args[1]).Item;

            IntersectionResult ir = face.Project(xyz);
            XYZ pt = ir.XYZPoint;
            UV uv = ir.UVPoint;
            double d = ir.Distance;
            Edge e = null;
            try
            {
                e = ir.EdgeObject;
            }
            catch { }
            double et = 0;
            try
            {
                et = ir.EdgeParameter;
            }
            catch { }

            var results = FSharpList<Value>.Empty;
            results = FSharpList<Value>.Cons(Value.NewNumber(et), results);
            results = FSharpList<Value>.Cons(Value.NewContainer(e), results);
            results = FSharpList<Value>.Cons(Value.NewNumber(d), results);
            results = FSharpList<Value>.Cons(Value.NewContainer(uv), results);
            results = FSharpList<Value>.Cons(Value.NewContainer(xyz), results);

            pts.Add(pt);

            return Value.NewList(results);
        }

        protected List<XYZ> pts = new List<XYZ>();
        public RenderDescription RenderDescription { get; set; }
        public void Draw()
        {
            if (this.RenderDescription == null)
                this.RenderDescription = new RenderDescription();
            else
                this.RenderDescription.ClearAll();

            foreach (XYZ pt in pts)
                this.RenderDescription.points.Add(new Point3D(pt.X, pt.Y, pt.Z));
        }

        public void ClearReferences()
        {
            pts.Clear();
        }
    }
}
