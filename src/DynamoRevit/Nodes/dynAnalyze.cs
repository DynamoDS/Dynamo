using Autodesk.Revit.DB;
using Dynamo.Models;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using Dynamo.Utilities;

namespace Dynamo.Nodes
{
    [NodeName("Curve Derivatives")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_QUERY)]
    [NodeDescription("Returns a transform describing the curve at the parameter.")]
    public class ComputeCurveDerivatives : GeometryBase
    {
        public ComputeCurveDerivatives()
        {
            InPortData.Add(new PortData("crv", "The curve to evaluate", typeof(Value.Container)));
            InPortData.Add(new PortData("p", "The parameter to evaluate", typeof(Value.Container)));
            OutPortData.Add(new PortData("t", "Transform describing the curve at the parameter(Transform)", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Curve crv = (Curve)((Value.Container)args[0]).Item;
            double p = (double)((Value.Number)args[1]).Item;

            Transform t = Transform.Identity;

            if (crv != null)
            {
                t = crv.ComputeDerivatives(p,true);
                t.BasisX = t.BasisX.Normalize();
                t.BasisZ = t.BasisZ.Normalize();
                t.BasisY = t.BasisX.CrossProduct(t.BasisZ);
            }

            return Value.NewContainer(t);
        }

    }

    [NodeName("Transform on Curve")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_QUERY)]
    [NodeDescription("Evaluates tangent vector of curve or edge at parameter.")]
    public class TangentTransformOnCurveOrEdge : GeometryBase
    {
        public TangentTransformOnCurveOrEdge()
        {
            InPortData.Add(new PortData("parameter", "The normalized parameter to evaluate at within 0..1 range except for closed curve", typeof(Value.Number)));
            InPortData.Add(new PortData("curve or edge", "The geometry curve or edge to evaluate.", typeof(Value.Container)));
            OutPortData.Add(new PortData("transform", "tangent transform at parameter.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var parameter = ((Value.Number)args[0]).Item;

            var thisCurve = ((Value.Container)args[1]).Item as Curve;
            var thisEdge = (thisCurve != null) ? null : (((Value.Container)args[1]).Item as Edge);

            if (thisCurve == null && thisEdge == null && ((Value.Container)args[1]).Item is Reference)
            {
                var r = (Reference)((Value.Container)args[1]).Item;
                if (r != null)
                {
                    var refElem = dynRevitSettings.Doc.Document.GetElement(r.ElementId);
                    if (refElem != null)
                    {
                        GeometryObject geob = refElem.GetGeometryObjectFromReference(r);
                        thisEdge = geob as Edge;
                        if (thisEdge == null)
                            thisCurve = geob as Curve;
                    }
                }
            }

            var result = (thisCurve != null) ?
                (!XyzOnCurveOrEdge.curveIsReallyUnbound(thisCurve) ? thisCurve.ComputeDerivatives(parameter, true) : thisCurve.ComputeDerivatives(parameter, false))
                :
                (thisEdge == null ? null : thisEdge.ComputeDerivatives(parameter));

            
             

            return Value.NewContainer(result);
        }
    }
}
