using System;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;
using Dynamo.FSchemeInterop;
using Dynamo.Models;
using Dynamo.Revit;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;

namespace Dynamo.Nodes
{
    [NodeName("Nurbs Spline Model Curve")]
    [NodeCategory(BuiltinNodeCategories.REVIT_REFERENCE)]
    [NodeDescription("Node to create a planar nurbs spline model curve.")]
    [NodeSearchTags("curve", "model", "line", "revit", "nurbs")]
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

    [NodeName("Model Curve")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Creates a model curve.")]
    [NodeSearchTags("curve", "model", "line", "revit")]
    public class ModelCurve : RevitTransactionNodeWithOneOutput
    {
        public ModelCurve()
        {
            InPortData.Add(new PortData("c", "A Geometric Curve.", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("mc", "Model Curve", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        static bool hasMethodSetCurve = true;

        static public void setCurveMethod(Autodesk.Revit.DB.ModelCurve mc, Curve c)
        {
            bool foundMethod = false;

            if (hasMethodSetCurve)
            {
                Type CurveElementType = typeof(Autodesk.Revit.DB.CurveElement);
                MethodInfo[] curveElementMethods = CurveElementType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                System.String nameOfMethodSetCurve = "SetGeometryCurveOverridingJoins";

                foreach (MethodInfo m in curveElementMethods)
                {
                    if (m.Name == nameOfMethodSetCurve)
                    {
                        object[] argsM = new object[1];
                        argsM[0] = c;

                        foundMethod = true;
                        m.Invoke(mc, argsM);
                        break;
                    }
                }
            }
            if (!foundMethod)
            {
                hasMethodSetCurve = false;
                mc.GeometryCurve = c;
            }
        }

        static bool hasMethodResetSketchPlane = true;
        //returns unused sketch plane id
        static public ElementId resetSketchPlaneMethod(Autodesk.Revit.DB.ModelCurve mc, Curve c, Autodesk.Revit.DB.Plane flattenedOnPlane, out bool needsSketchPlaneReset)
        {
            //do we need to reset?
            needsSketchPlaneReset = false;
            Autodesk.Revit.DB.Plane newPlane = flattenedOnPlane != null ? flattenedOnPlane : dynRevitUtils.GetPlaneFromCurve(c, false);

            Autodesk.Revit.DB.Plane curPlane = mc.SketchPlane.Plane;

            bool resetPlane = false;

            {
                double llSqCur = curPlane.Normal.DotProduct(curPlane.Normal);
                double llSqNew = newPlane.Normal.DotProduct(newPlane.Normal);
                double dotP = newPlane.Normal.DotProduct(curPlane.Normal);
                double dotSqNormalized = (dotP / llSqCur) * (dotP / llSqNew);
                double angleTol = Math.PI / 1800.0;
                if (dotSqNormalized < 1.0 - angleTol * angleTol)
                    resetPlane = true;
            }
            Autodesk.Revit.DB.SketchPlane sp = null;

            if (!resetPlane)
            {
                double originDiff = curPlane.Normal.DotProduct(curPlane.Origin - newPlane.Origin);
                double tolerance = 0.000001;
                if (originDiff > tolerance || originDiff < -tolerance)
                {
                    sp = dynRevitUtils.GetSketchPlaneFromCurve(c);
                    mc.SketchPlane = dynRevitUtils.GetSketchPlaneFromCurve(c);
                }
                return (sp == null || mc.SketchPlane.Id == sp.Id) ? ElementId.InvalidElementId : sp.Id;
            }

            //do reset if method is available

            bool foundMethod = false;

            if (hasMethodResetSketchPlane)
            {
                Type CurveElementType = typeof(Autodesk.Revit.DB.CurveElement);
                MethodInfo[] curveElementMethods = CurveElementType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                System.String nameOfMethodSetCurve = "ResetSketchPlaneAndCurve";
                System.String nameOfMethodSetCurveAlt = "SetSketchPlaneAndCurve";

                foreach (MethodInfo m in curveElementMethods)
                {
                    if (m.Name == nameOfMethodSetCurve || m.Name == nameOfMethodSetCurveAlt)
                    {
                        object[] argsM = new object[2];
                        sp = dynRevitUtils.GetSketchPlaneFromCurve(c);
                        argsM[0] = sp;
                        argsM[1] = null;

                        foundMethod = true;
                        m.Invoke(mc, argsM);
                        break;
                    }
                }
            }
            if (!foundMethod)
            {
                //sp = dynRevitUtils.GetSketchPlaneFromCurve(c);
                hasMethodResetSketchPlane = false;
                needsSketchPlaneReset = true;
                //expect exception, so try to keep old plane?
                //mc.SketchPlane = sp;
                return ElementId.InvalidElementId;
            }

            if (sp != null && mc.SketchPlane.Id != sp.Id)
                return sp.Id;

            return ElementId.InvalidElementId;
        }


        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var c = (Curve)((FScheme.Value.Container)args[0]).Item;

            Autodesk.Revit.DB.ModelCurve mc = null;
            Autodesk.Revit.DB.Plane plane = dynRevitUtils.GetPlaneFromCurve(c, false);

            Curve flattenCurve = null;

            //instead of changing Revit curve keep it "as is"
            //user might have trouble modifying curve in Revit if it is off the sketch plane

            if (this.Elements.Any())
            {
                bool needsRemake = false;
                if (dynUtils.TryGetElement(this.Elements[0], out mc))
                {
                    ElementId idSpUnused = ModelCurve.resetSketchPlaneMethod(mc, c, plane, out needsRemake);

                    if (idSpUnused != ElementId.InvalidElementId)
                    {
                        this.DeleteElement(idSpUnused);
                    }
                    if (!needsRemake)
                    {
                        if (!mc.GeometryCurve.IsBound && c.IsBound)
                        {
                            c = c.Clone();
                            c.MakeUnbound();
                        }
                        ModelCurve.setCurveMethod(mc, c); // mc.GeometryCurve = c;
                    }
                    else
                        this.DeleteElement(this.Elements[0]);

                }
                else
                    needsRemake = true;
                if (needsRemake)
                {
                    var sp = dynRevitUtils.GetSketchPlaneFromCurve(c);
                    if (dynRevitUtils.GetPlaneFromCurve(c, true) == null)
                    {
                        flattenCurve = dynRevitUtils.Flatten3dCurveOnPlane(c, plane);
                        mc = this.UIDocument.Document.IsFamilyDocument
                             ? this.UIDocument.Document.FamilyCreate.NewModelCurve(flattenCurve, sp)
                                : this.UIDocument.Document.Create.NewModelCurve(flattenCurve, sp);

                        ModelCurve.setCurveMethod(mc, c);
                    }
                    else
                    {
                        mc = this.UIDocument.Document.IsFamilyDocument
                           ? this.UIDocument.Document.FamilyCreate.NewModelCurve(c, sp)
                           : this.UIDocument.Document.Create.NewModelCurve(c, sp);
                    }
                    this.Elements[0] = mc.Id;
                    if (mc.SketchPlane.Id != sp.Id)
                    {
                        //THIS BIZARRE as Revit could use different existing SP, so if Revit had found better plane  this sketch plane has no use
                        this.DeleteElement(sp.Id);
                    }
                    this.Elements[0] = mc.Id;
                }
            }
            else
            {
                var sp = dynRevitUtils.GetSketchPlaneFromCurve(c);

                if (dynRevitUtils.GetPlaneFromCurve(c, true) == null)
                {
                    flattenCurve = dynRevitUtils.Flatten3dCurveOnPlane(c, plane);
                    mc = this.UIDocument.Document.IsFamilyDocument
                         ? this.UIDocument.Document.FamilyCreate.NewModelCurve(flattenCurve, sp)
                            : this.UIDocument.Document.Create.NewModelCurve(flattenCurve, sp);

                    ModelCurve.setCurveMethod(mc, c);
                }
                else
                {
                    mc = this.UIDocument.Document.IsFamilyDocument
                       ? this.UIDocument.Document.FamilyCreate.NewModelCurve(c, sp)
                       : this.UIDocument.Document.Create.NewModelCurve(c, sp);
                }
                this.Elements.Add(mc.Id);
                if (mc.SketchPlane.Id != sp.Id)
                {
                    //found better plane
                    this.DeleteElement(sp.Id);
                }
            }

            return FScheme.Value.NewContainer(mc);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSRevitNodes.dll", 
                "ModelCurve.ByPlanarCurve", "ModelCurve.ByPlanarCurve@Curve");
        }
    }

    [NodeName("Model Curves From Curve Loop")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Creates a model curve.")]
    [NodeSearchTags("curve", "model", "line", "revit")]
    public class ModelCurveFromCurveLoop : RevitTransactionNodeWithOneOutput
    {
        public ModelCurveFromCurveLoop()
        {
            InPortData.Add(new PortData("cl", "A Geometric Planar Curve Loop.", typeof (FScheme.Value.Container)));
            OutPortData.Add(new PortData("mcs", "Model Curves", typeof (FScheme.Value.List)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            Autodesk.Revit.DB.CurveLoop cl = (Autodesk.Revit.DB.CurveLoop)((FScheme.Value.Container)args[0]).Item;

            if (cl == null)
                throw new InvalidOperationException("No curve loop");

            Autodesk.Revit.DB.Plane plane = cl.GetPlane();
            if (plane == null)
                throw new InvalidOperationException("Curve loop is not planar");
          
            var mcs = new System.Collections.Generic.List<Autodesk.Revit.DB.ModelCurve>();

            var listCurves = new System.Collections.Generic.List<Curve> ();
            CurveLoopIterator CLiter = cl.GetCurveLoopIterator();
            for (; CLiter.MoveNext(); )
            {
                listCurves.Add(CLiter.Current.Clone());
            }

            int numCurves = listCurves.Count;
            Autodesk.Revit.DB.SketchPlane sp = null;
            for (int index = 0; index < numCurves; index++)
            {

                //instead of changing Revit curve keep it "as is"
                //user might have trouble modifying curve in Revit if it is off the sketch plane
                Autodesk.Revit.DB.ModelCurve mc = null;
                if (this.Elements.Any() && index < this.Elements.Count)
                {
                    bool needsRemake = false;
                    if (dynUtils.TryGetElement(this.Elements[index], out mc))
                    {
                        ElementId idSpUnused = ModelCurve.resetSketchPlaneMethod(mc, listCurves[index], plane, out needsRemake);

                        if (idSpUnused != ElementId.InvalidElementId && index == numCurves - 1)
                        {
                            this.DeleteElement(idSpUnused);
                        }
                        if (!needsRemake)
                        {
                            if (!mc.GeometryCurve.IsBound && listCurves[index].IsBound)
                            {
                                listCurves[index] = listCurves[index].Clone();
                                listCurves[index].MakeUnbound();
                            }
                            ModelCurve.setCurveMethod(mc, listCurves[index]); // mc.GeometryCurve = c;
                        }
                        else
                            this.DeleteElement(this.Elements[index]);

                    }
                    else
                        needsRemake = true;
                    if (needsRemake)
                    {
                        if (sp == null)
                            sp = this.UIDocument.Document.IsFamilyDocument ?
                                this.UIDocument.Document.FamilyCreate.NewSketchPlane(plane) :
                                this.UIDocument.Document.Create.NewSketchPlane(plane);
                        if (dynRevitUtils.GetPlaneFromCurve(listCurves[index], true) == null)
                        {

                            mc = this.UIDocument.Document.IsFamilyDocument
                                ? this.UIDocument.Document.FamilyCreate.NewModelCurve(listCurves[index], sp)
                                : this.UIDocument.Document.Create.NewModelCurve(listCurves[index], sp);

                            ModelCurve.setCurveMethod(mc, listCurves[index]);
                        }
                        else
                        {
                            mc = this.UIDocument.Document.IsFamilyDocument
                                ? this.UIDocument.Document.FamilyCreate.NewModelCurve(listCurves[index], sp)
                                : this.UIDocument.Document.Create.NewModelCurve(listCurves[index], sp);
                        }
                        if (index < this.Elements.Count)
                           this.Elements[index] = mc.Id;
                        else
                        {
                            this.Elements.Add( mc.Id);
                        }
                        if (mc.SketchPlane.Id != sp.Id && index == numCurves - 1)
                        {
                            //THIS BIZARRE as Revit could use different existing SP, so if Revit had found better plane  this sketch plane has no use
                            this.DeleteElement(sp.Id);
                        }
                    }
                }
                else
                {
                    if (sp == null)
                        sp = this.UIDocument.Document.IsFamilyDocument ?
                                this.UIDocument.Document.FamilyCreate.NewSketchPlane(plane) :
                                this.UIDocument.Document.Create.NewSketchPlane(plane);

                    if (dynRevitUtils.GetPlaneFromCurve(listCurves[index], true) == null)
                    {
                        mc = this.UIDocument.Document.IsFamilyDocument
                            ? this.UIDocument.Document.FamilyCreate.NewModelCurve(listCurves[index], sp)
                            : this.UIDocument.Document.Create.NewModelCurve(listCurves[index], sp);

                        ModelCurve.setCurveMethod(mc, listCurves[index]);
                    }
                    else
                    {
                        mc = this.UIDocument.Document.IsFamilyDocument
                            ? this.UIDocument.Document.FamilyCreate.NewModelCurve(listCurves[index], sp)
                            : this.UIDocument.Document.Create.NewModelCurve(listCurves[index], sp);
                    }
                    this.Elements.Add(mc.Id);
                    if (mc.SketchPlane.Id != sp.Id && index == numCurves - 1) 
                    {
                        //found better plane
                        this.DeleteElement(sp.Id);
                    }
                }
                if (mc != null)
                   mcs.Add(mc);
            }
           FSharpList<FScheme.Value> results = FSharpList<FScheme.Value>.Empty;
               
           foreach (var mc in mcs)
           {
               results = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewContainer(mc), results);
           }
           return FScheme.Value.NewList(Utils.SequenceToFSharpList(results.Reverse()));
        }
    }
}
