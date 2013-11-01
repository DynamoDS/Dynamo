using System;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Revit;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;

namespace Dynamo.Nodes
{

    [NodeName("Nurbs Spline Model Curve")]
    [NodeCategory(BuiltinNodeCategories.REVIT_REFERENCE)]
    [NodeDescription("Node to create a planar nurbs spline model curve.")]
    public class ModelCurveNurbSpline : RevitTransactionNodeWithOneOutput
    {
        public ModelCurveNurbSpline()
        {
            InPortData.Add(new PortData("pts", "The points from which to create the nurbs curve", typeof(FScheme.Value.List)));
            OutPortData.Add(new PortData("cv", "The nurbs spline model curve created by this operation.", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var pts = ((FScheme.Value.List)args[0]).Item.Select(
               x => ((ReferencePoint)((FScheme.Value.Container)x).Item).Position
            ).ToList();

            if (pts.Count <= 1)
            {
                throw new Exception("Not enough reference points to make a curve.");
            }

            var ns = UIDocument.Application.Application.Create.NewNurbSpline(
                    pts, Enumerable.Repeat(1.0, pts.Count).ToList());

            ModelNurbSpline c;

            if (Elements.Any() && dynUtils.TryGetElement(Elements[0], out c))
            {
                ModelCurve.setCurveMethod(c, ns); //c.GeometryCurve = ns;
            }
            else
            {
                Elements.Clear();

                double rawParam = ns.ComputeRawParameter(.5);
                Transform t = ns.ComputeDerivatives(rawParam, false);

                XYZ norm = t.BasisZ;

                if (norm.GetLength() == 0)
                {
                    norm = XYZ.BasisZ;
                }

                Autodesk.Revit.DB.Plane p = new Autodesk.Revit.DB.Plane(norm, t.Origin);
                Autodesk.Revit.DB.SketchPlane sp = this.UIDocument.Document.FamilyCreate.NewSketchPlane(p);
                //sps.Add(sp);

                c = UIDocument.Document.FamilyCreate.NewModelCurve(ns, sp) as ModelNurbSpline;

                Elements.Add(c.Id);
            }

            return FScheme.Value.NewContainer(c);
        }
    }
}
