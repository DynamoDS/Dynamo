using System;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using RevitServices.Persistence;

namespace Dynamo.Nodes
{
    [NodeName("Nurbs Spline")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Node to create a planar nurbs spline curve.")]
    public class GeometryCurveNurbSpline : GeometryBase
    {
        public GeometryCurveNurbSpline()
        {
            InPortData.Add(new PortData("xyzs", "The xyzs from which to create the nurbs curve", typeof(FScheme.Value.List)));
            OutPortData.Add(new PortData("cv", "The nurbs spline curve created by this operation.", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var pts = ((FScheme.Value.List)args[0]).Item.Select(
               x => ((XYZ)((FScheme.Value.Container)x).Item)).ToList();

            if (pts.Count <= 1)
            {
                throw new Exception("Not enough reference points to make a curve.");
            }

            var ns = DocumentManager.GetInstance().CurrentUIDocument.Application.Application.Create.NewNurbSpline(
                    pts, Enumerable.Repeat(1.0, pts.Count).ToList());

            return FScheme.Value.NewContainer(ns);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "NurbsCurve.ByPoints", "NurbsCurve.ByPoints@Point[]");
        }
    }
}
