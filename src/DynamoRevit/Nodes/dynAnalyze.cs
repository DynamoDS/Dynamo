using Autodesk.Revit.DB;
using Dynamo.Models;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using Dynamo.Utilities;

namespace Dynamo.Nodes
{
    [NodeName("Evaluate Surface")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_SURFACE)]
    [NodeDescription("Evaluate a parameter(UV) on a face to find the XYZ location.")]
    class XyzEvaluate : GeometryBase
    {
        public XyzEvaluate()
        {
            InPortData.Add(new PortData("uv", "The point to evaluate.", typeof(Value.Container)));
            InPortData.Add(new PortData("face", "The face to evaluate.", typeof(Value.Container)));
            OutPortData.Add(new PortData("XYZ", "The location.", typeof(Value.Container)));
            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Reference faceRef = (args[1] as Value.Container).Item as Reference;
            Autodesk.Revit.DB.Face f = (faceRef == null) ? 
                ((args[1] as Value.Container).Item as Autodesk.Revit.DB.Face) : 
                dynRevitSettings.Doc.Document.GetElement(faceRef).GetGeometryObjectFromReference(faceRef) as Autodesk.Revit.DB.Face;


            XYZ face_point = null;

            if (f != null)
            {
                //each item in the list will be a reference point
                UV param = (UV)(args[0] as Value.Container).Item;
                face_point = f.Evaluate(param);
            }

            return Value.NewContainer(face_point);
        }
    }

    [NodeName("Surface Normal")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_SURFACE)]
    [NodeDescription("Evaluate a point on a face to find the normal.")]
    class NormalEvaluate : GeometryBase
    {
        public NormalEvaluate()
        {
            InPortData.Add(new PortData("uv", "The point to evaluate.", typeof(Value.Container)));
            InPortData.Add(new PortData("face", "The face to evaluate.", typeof(Value.Container)));
            OutPortData.Add(new PortData("XYZ", "The normal.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Reference faceRef = (args[1] as Value.Container).Item as Reference;
            Autodesk.Revit.DB.Face f = (faceRef == null) ?
                ((args[1] as Value.Container).Item as Autodesk.Revit.DB.Face) :
                dynRevitSettings.Doc.Document.GetElement(faceRef).GetGeometryObjectFromReference(faceRef) as Autodesk.Revit.DB.Face;

            XYZ norm = null;

            if (f != null)
            {
                //each item in the list will be a reference point
                UV uv = (UV)(args[0] as Value.Container).Item;
                norm = f.ComputeNormal(uv);
            }

            return Value.NewContainer(norm);
        }
    }

    [NodeName("Surface Derivatives")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_SURFACE)]
    [NodeDescription("Returns a transform describing the face (f) at the parameter (uv).")]
    public class ComputeFaceDerivatives : GeometryBase
    {
        public ComputeFaceDerivatives()
        {
            InPortData.Add(new PortData("f", "The face to evaluate(Face)", typeof(Value.Container)));
            InPortData.Add(new PortData("uv", "The parameter to evaluate(UV)", typeof(Value.Container)));
            OutPortData.Add(new PortData("t", "Transform describing the face at the parameter(Transform)", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var faceRef = ((Value.Container)args[0]).Item as Reference;
            var uv = (UV)((Value.Container)args[1]).Item;

            var t = Transform.Identity;

            Autodesk.Revit.DB.Face f = (faceRef == null) ? 
                ((Autodesk.Revit.DB.Face)((Value.Container)args[0]).Item) : 
                (dynRevitSettings.Doc.Document.GetElement(faceRef.ElementId).GetGeometryObjectFromReference(faceRef) as Autodesk.Revit.DB.Face);

            if (f != null)
            {
                t = f.ComputeDerivatives(uv);
                t.BasisX = t.BasisX.Normalize();
                t.BasisZ = t.BasisZ.Normalize();
                t.BasisY = t.BasisX.CrossProduct(t.BasisZ);
            }

            return Value.NewContainer(t);
        }

    }

    [NodeName("Curve Derivatives")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_CURVE)]
    [NodeDescription("Returns a transform describing the face (f) at the parameter (uv).")]
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

    [NodeName("Frame on Curve")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_CURVE)]
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
                Reference r = (Reference)((Value.Container)args[1]).Item;
                if (r != null)
                {
                    Element refElem = dynRevitSettings.Doc.Document.GetElement(r.ElementId);
                    if (refElem != null)
                    {
                        GeometryObject geob = refElem.GetGeometryObjectFromReference(r);
                        thisEdge = geob as Edge;
                        if (thisEdge == null)
                            thisCurve = geob as Curve;
                    }
                }
            }

            Transform result = (thisCurve != null) ?
                (!XyzOnCurveOrEdge.curveIsReallyUnbound(thisCurve) ? thisCurve.ComputeDerivatives(parameter, true) : thisCurve.ComputeDerivatives(parameter, false))
                :
                (thisEdge == null ? null : thisEdge.ComputeDerivatives(parameter));

            return Value.NewContainer(result);
        }
    }
}
