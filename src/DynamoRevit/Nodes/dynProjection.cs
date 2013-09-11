using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

using Autodesk.Revit;
using Autodesk.Revit.DB;
using Dynamo.Models;
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
    public class ProjectPointOnCurve : RevitTransactionNode, IDrawable, IClearable
    {
        private readonly PortData _xyzPort = new PortData(
            "xyz", "The nearest point on the curve.", typeof(Value.Container));

        private readonly PortData _tPort = new PortData(
            "t", "The unnormalized parameter on the curve.", typeof(Value.Number));

        private readonly PortData _dPort = new PortData(
            "d", "The distance from the point to the curve .", typeof(Value.Number));

        public ProjectPointOnCurve()
        {
            InPortData.Add(new PortData("xyz", "The point to be projected.", typeof(Value.Container)));
            InPortData.Add(new PortData("crv", "The curve on which to project the point.", typeof(Value.Container)));

            OutPortData.Add(_xyzPort);
            OutPortData.Add(_tPort);
            OutPortData.Add(_dPort);

            RegisterAllPorts();
        }

        public override void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            var xyz = (XYZ)((Value.Container)args[0]).Item;
            var crv = (Curve)((Value.Container)args[1]).Item;

            IntersectionResult ir = crv.Project(xyz);
            XYZ pt = ir.XYZPoint;
            double t = ir.Parameter;
            double d = ir.Distance;

            pts.Add(pt);

            outPuts[_xyzPort] = Value.NewContainer(pt);
            outPuts[_tPort] = Value.NewNumber(t);
            outPuts[_dPort] = Value.NewNumber(d);
        }

        protected List<XYZ> pts = new List<XYZ>();

        new public void Draw()
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

    [NodeName("Project Point On Face/Plane")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_INTERSECT)]
    [NodeDescription("Project a point onto a face or plane.")]
    public class ProjectPointOnFace : RevitTransactionNode, IDrawable, IClearable
    {
        private readonly PortData _xyzPort = new PortData(
            "xyz", "The nearest point to the projected point on the face.", typeof(Value.Container));

        private readonly PortData _uvPort = new PortData(
            "uv", "The UV coordinates of the nearest point on the face.", typeof(Value.Number));

        private readonly PortData _dPort = new PortData(
            "d", "The distance from the point to the face", typeof(Value.Number));

        private readonly PortData _edgePort = new PortData(
            "edge", "The edge if projected point is near an edge.", typeof(Value.Container));

        private readonly PortData _edgeTPort = new PortData(
            "edge t", "The parameter of the nearest point on the edge.", typeof(Value.Number));

        public ProjectPointOnFace()
        {
            InPortData.Add(new PortData("xyz", "The point to be projected.", typeof(Value.Container)));
            InPortData.Add(new PortData("face/plane", "The face or plane on which to project the point.", typeof(Value.Container)));

            OutPortData.Add(_xyzPort);
            OutPortData.Add(_uvPort);
            OutPortData.Add(_dPort);
            OutPortData.Add(_edgePort);
            OutPortData.Add(_edgeTPort);

            RegisterAllPorts();
        }

        public override void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            var xyz = (XYZ)((Value.Container)args[0]).Item;
            var inputArg = ((Value.Container)args[1]).Item;

            XYZ pt;
            UV uv;
            double d;
            Edge e;
            double et;
  
            var face = inputArg is Face ? (Face)inputArg : null;
            if (face == null && !(inputArg is Autodesk.Revit.DB.Plane))
                throw new Exception(" Project Point On Face needs Face or Plane as argument no. 1");
            if (face == null)
            {
                var pln = (Autodesk.Revit.DB.Plane)inputArg;
                uv = new UV(
                    pln.XVec.DotProduct(xyz - pln.Origin), pln.YVec.DotProduct(xyz - pln.Origin));
                pt = pln.Origin + uv[0]*pln.XVec + uv[1]*pln.YVec;
                d = xyz.DistanceTo(pt);
                e = null;
                et = 0.0;
            }
            else
            {
                IntersectionResult ir = face.Project(xyz);

                pt = ir.XYZPoint;
                uv = ir.UVPoint;
                d = ir.Distance;
                e = null;
                et = 0;

                try
                {
                    e = ir.EdgeObject;
                }
                catch { }

                try
                {
                    et = ir.EdgeParameter;
                }
                catch { }
            }

            pts.Add(pt);

            outPuts[_xyzPort] = Value.NewContainer(pt);
            outPuts[_uvPort] = Value.NewContainer(uv);
            outPuts[_dPort] = Value.NewNumber(d);
            outPuts[_edgePort] = Value.NewContainer(e);
            outPuts[_edgeTPort] = Value.NewNumber(et);
        }

        protected List<XYZ> pts = new List<XYZ>();

        new public void Draw()
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
