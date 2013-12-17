using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Revit;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;

namespace Dynamo.Nodes
{
    [NodeName("Reference Curve Chain")]
    [NodeCategory(BuiltinNodeCategories.REVIT_BAKE)]
    [NodeDescription("Creates continuous chain of reference curves ")]
    public class PlanarRefCurveChain : RevitTransactionNodeWithOneOutput
    {
        public PlanarRefCurveChain()
        {
            InPortData.Add(new PortData("list", "A list of ref curves to make one planar chain", typeof(FScheme.Value.List)));

            OutPortData.Add(new PortData("Chain", "Chain of ref. curves ready to be one profile for nodes like Loft Form", typeof(ModelCurveArray)));

            RegisterAllPorts();
        }
        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var doc = dynRevitSettings.Doc;
            var refCurveList = ((FScheme.Value.List)args[0]).Item.Select(
               x => (((FScheme.Value.Container)x).Item is Autodesk.Revit.DB.ModelCurve ?
                   ((Autodesk.Revit.DB.ModelCurve)((FScheme.Value.Container)x).Item)
                   : (Autodesk.Revit.DB.ModelCurve)(
                                      doc.Document.GetElement(
                                             ((Reference)((FScheme.Value.Container)x).Item).ElementId)
                                                             )
                                 )
                   ).ToList();

            var myModelCurves = new ModelCurveArray();

            //Plane thisPlane = null;
            //Line oneLine = null;

            var refIds = new List<ElementId>();
            var loopStart = new XYZ();
            var otherEnd = new XYZ();
            int index = 0;
            double tolerance = 0.000000001;
            foreach (var refCurve in refCurveList)
            {
                if (index == 0)
                {
                    loopStart = refCurve.GeometryCurve.Evaluate(0.0, true);
                    otherEnd = refCurve.GeometryCurve.Evaluate(1.0, true);
                }
                else //if (index > 0)
                {
                    XYZ startXYZ = refCurve.GeometryCurve.Evaluate(0.0, true);
                    XYZ endXYZ = refCurve.GeometryCurve.Evaluate(1.0, true);
                    if (index == 1)
                    {
                        if (startXYZ.DistanceTo(otherEnd) > tolerance && endXYZ.DistanceTo(otherEnd) > tolerance &&
                            (startXYZ.DistanceTo(loopStart) > tolerance || endXYZ.DistanceTo(loopStart) > tolerance))
                        {
                            XYZ temp = loopStart;
                            loopStart = otherEnd;
                            otherEnd = temp;
                        }
                        if (startXYZ.DistanceTo(otherEnd) > tolerance && endXYZ.DistanceTo(otherEnd) < tolerance)
                            otherEnd = startXYZ;
                        else if (startXYZ.DistanceTo(otherEnd) < tolerance && endXYZ.DistanceTo(otherEnd) > tolerance)
                            otherEnd = endXYZ;
                        else
                            throw new Exception("Gap between curves in chain of reference curves.");
                    }
                }

                refIds.Add(refCurve.Id);
                myModelCurves.Append(refCurve);
                index++;
            }

            List<ElementId> removeIds = new List<ElementId>();
            foreach (ElementId oldId in this.Elements)
            {
                if (!refIds.Contains(oldId))
                {
                    removeIds.Add(oldId);
                }
            }

            foreach (ElementId removeId in removeIds)
            {
                this.Elements.Remove(removeId);
            }
            foreach (ElementId newId in refIds)
            {
                if (!this.Elements.Contains(newId))
                    this.Elements.Add(newId);
            }
            //if (!curveLoop.HasPlane())
            //    throw new Exception(" Planar Ref Curve Chain fails: not planar");
            return FScheme.Value.NewContainer(myModelCurves);
        }
    }

    /*
    [NodeName("Reference Curve")]
    [NodeCategory(BuiltinNodeCategories.REVIT_REFERENCE)]
    [NodeDescription("Creates a reference curve.")]
    [NodeSearchTags("curve", "revit", "ref", "reference", "model")]
    public class ReferenceCurve : RevitTransactionNodeWithOneOutput
    {
        public ReferenceCurve()
        {
            InPortData.Add(new PortData("c", "A Geometric Curve.", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("rc", "Reference Curve", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var c = (Curve)((FScheme.Value.Container)args[0]).Item;
            Autodesk.Revit.DB.ModelCurve mc = null;

            Curve flattenCurve = null;
            Autodesk.Revit.DB.Plane plane = dynRevitUtils.GetPlaneFromCurve(c, false);

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
                    //mc.SketchPlane = sp;
                    if (!needsRemake)
                    {
                        if (!mc.GeometryCurve.IsBound && c.IsBound)
                        {
                            c = c.Clone();
                            c.MakeUnbound();
                        }
                        ModelCurve.setCurveMethod(mc, c);  //mc.GeometryCurve = c;
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
                        mc = this.UIDocument.Document.IsFamilyDocument
                       ? this.UIDocument.Document.FamilyCreate.NewModelCurve(c, sp)
                       : this.UIDocument.Document.Create.NewModelCurve(c, sp);
                    mc.ChangeToReferenceLine();
                    this.Elements[0] = mc.Id;
                    //mc.SketchPlane = sp;
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
                mc.ChangeToReferenceLine();
                //mc.SketchPlane = sp;
                if (mc.SketchPlane.Id != sp.Id)
                {
                    //found better plane
                    this.DeleteElement(sp.Id);
                }
            }

            return FScheme.Value.NewContainer(mc);
        }
    }
     * */
}
