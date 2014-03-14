using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using Dynamo.Revit;

namespace Dynamo.Nodes
{
    [NodeName("Project Point On Curve")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_QUERY)]
    [NodeDescription("Project a point onto a curve.")]
    public class ProjectPointOnCurve : RevitTransactionNode
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

            outPuts[_xyzPort] = Value.NewContainer(pt);
            outPuts[_tPort] = Value.NewNumber(t);
            outPuts[_dPort] = Value.NewNumber(d);
        }
    }

    [NodeName("Project Point On Face or Plane")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SURFACE_QUERY)]
    [NodeDescription("Project a point onto a face or plane.")]
    public class ProjectPointOnFace : RevitTransactionNode
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

        private readonly PortData _resultPort = new PortData(
           "result", "The projection result.", typeof(Value.String));

        public ProjectPointOnFace()
        {
            InPortData.Add(new PortData("xyz", "The point to be projected.", typeof(Value.Container)));
            InPortData.Add(new PortData("face/plane", "The face or plane on which to project the point.", typeof(Value.Container)));

            OutPortData.Add(_xyzPort);
            OutPortData.Add(_uvPort);
            OutPortData.Add(_dPort);
            OutPortData.Add(_edgePort);
            OutPortData.Add(_edgeTPort);
            OutPortData.Add(_resultPort);

            RegisterAllPorts();
        }

        public override void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            var xyz = (XYZ)((Value.Container)args[0]).Item;
            var inputArg = ((Value.Container)args[1]).Item;

            XYZ pt;
            UV uv;
            double d;
            Edge e = null;
            double et = 0.0;
  
            var face = inputArg is Face ? (Face)inputArg : null;
            if (face == null && !(inputArg is Autodesk.Revit.DB.Plane))
                throw new Exception(" Project Point On Face needs Face or Plane as argument no. 1");
            if (face == null)
            {
                var pln = (Autodesk.Revit.DB.Plane) inputArg;
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
                bool projectedOnFace = true;
                if (ir == null)
                {
                    outPuts[_resultPort] = Value.NewString("Projected Outside Face");
                    double edgeDist = 1.0e12;
                    XYZ edgePt = xyz;

                    projectedOnFace = false;

                    EdgeArrayArray edges = face.EdgeLoops;
                    for (int iLoop = 0; iLoop < edges.Size; iLoop++)
                    {
                        for (int iEdge = 0; iEdge < edges.get_Item(iLoop).Size; iEdge++)
                        {
                            Edge thisEdge = edges.get_Item(iLoop).get_Item(iEdge);
                            Curve thisAsCurve = thisEdge.AsCurve();
                            IntersectionResult irCurve = thisAsCurve.Project(xyz);
                            if (irCurve != null && irCurve.Distance < edgeDist - 1.0e-12)
                            {
                                edgeDist = irCurve.Distance;
                                e = thisEdge;
                                edgePt = irCurve.XYZPoint;
                            }
                            else if (irCurve == null)
                            {
                                XYZ vertex0 = thisEdge.Evaluate(0.0);
                                if (vertex0.DistanceTo(xyz) < edgeDist - 1.0e-12)
                                {
                                    edgeDist = vertex0.DistanceTo(xyz);
                                    e = thisEdge;
                                    edgePt = vertex0;
                                }
                                XYZ vertex1 = thisEdge.Evaluate(1.0);
                                if (vertex1.DistanceTo(xyz) < edgeDist - 1.0e-12)
                                {
                                    edgeDist = vertex1.DistanceTo(xyz);
                                    e = thisEdge;
                                    edgePt = vertex1;
                                }
                            }
                        }
                    }
                    if (1.01*edgeDist < 1.0e12)
                    {
                        ir = face.Project(edgePt);
                        d = edgePt.DistanceTo(xyz);
                        pt = edgePt;
                    }
                    else
                        throw new Exception(" Could not find closest point on Face to return as projection.");
                }
                else
                {
                    outPuts[_resultPort] = Value.NewString("Projected Into Face");
                    d = ir.Distance;
                    pt = ir.XYZPoint;
                }

                uv = projectedOnFace ? ir.UVPoint : null;
 
                if (projectedOnFace)
                {
                    try
                    {
                        e = ir.EdgeObject;
                        et = ir.EdgeParameter;
                    }
                    catch
                    {
                        e = null;
                        et = 0.0;
                    }
                }
            }
   

            outPuts[_xyzPort] = Value.NewContainer(pt);
            outPuts[_uvPort] = Value.NewContainer(uv);
            outPuts[_dPort] = Value.NewNumber(d);
            outPuts[_edgePort] = Value.NewContainer(e);
            outPuts[_edgeTPort] = Value.NewNumber(et);
        }
    }
}
