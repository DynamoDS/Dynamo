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
    [NodeName("Curve Face Intersection")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Calculates the intersection of the specified curve with this face.")]
    public class dynCurveFaceIntersection : dynRevitTransactionNode
    {
        public dynCurveFaceIntersection()
        {
            InPortData.Add(new PortData("crv", "The specified curve to intersect with this face.", typeof(Value.Container)));
            InPortData.Add(new PortData("face", "The face from which to calculate the intersection.", typeof(Value.Container)));

            OutPortData.Add(new PortData("result", "The set comparison result.", typeof(Value.String)));
            OutPortData.Add(new PortData("xsects", "A list of intersection information.", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var crv = (Curve)((Value.Container)args[0]).Item;
            var face = (Face)((Value.Container)args[1]).Item;

            IntersectionResultArray xsects = new IntersectionResultArray();
            SetComparisonResult result = face.Intersect(crv, out xsects);

            var xsect_results = FSharpList<Value>.Empty;
            foreach (IntersectionResult ir in xsects)
            {
                var xsect = FSharpList<Value>.Empty;
                xsect = FSharpList<Value>.Cons(Value.NewNumber(ir.EdgeParameter), xsect);
                xsect = FSharpList<Value>.Cons(Value.NewContainer(ir.EdgeObject), xsect);
                xsect = FSharpList<Value>.Cons(Value.NewNumber(ir.Parameter), xsect);
                xsect = FSharpList<Value>.Cons(Value.NewContainer(ir.UVPoint), xsect);
                xsect = FSharpList<Value>.Cons(Value.NewContainer(ir.XYZPoint), xsect);
                xsect_results = FSharpList<Value>.Cons(Value.NewList(xsect), xsect_results);
            }
            var results = FSharpList<Value>.Empty;
            results = FSharpList<Value>.Cons(Value.NewString(result.ToString()), results);
            results = FSharpList<Value>.Cons(Value.NewList(xsect_results), results);

            return Value.NewList(results);
        }
    }

}
