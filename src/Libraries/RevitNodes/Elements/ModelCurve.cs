using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;

using Revit.GeometryConversion;

using RevitServices.Persistence;
using RevitServices.Transactions;
using Curve = Autodesk.Revit.DB.Curve;
using Line = Autodesk.Revit.DB.Line;
using Plane = Autodesk.Revit.DB.Plane;

namespace Revit.Elements
{
    /// <summary>
    /// A Revit ModelCurve
    /// </summary>
    [DSNodeServices.RegisterForTrace]
    public class ModelCurve : CurveElement
    {
        #region Private constructors

        /// <summary>
        /// Construct a model curve from the document.  The result is Dynamo owned
        /// </summary>
        /// <param name="curve"></param>
        private ModelCurve(Autodesk.Revit.DB.ModelCurve curve)
        {
            InternalSetCurveElement(curve);
        }

        // PB: This implementation borrows the somewhat risky notions from the original Dynamo
        // implementation.  In short, it has the ability to infer a sketch plane,
        // which might also mean deleting the original one.

        /// <summary>
        /// Internal constructor for ModelCurve
        /// </summary>
        /// <param name="crv"></param>
        /// <param name="makeReferenceCurve"></param>
        private ModelCurve(Autodesk.Revit.DB.Curve crv, bool makeReferenceCurve)
        {
            //Phase 1 - Check to see if the object exists and should be rebound
            var mc =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.ModelCurve>(Document);

            //There was a modelcurve, try and set sketch plane
            // if you can't, rebuild 
            if (mc != null)
            {
                InternalSetCurveElement(mc);
                if (!InternalSetSketchPlaneFromCurve(crv))
                {
                    InternalSetCurve(crv);
                    return;
                }
            }

            ElementId oldId = (mc != null) ? mc.Id : ElementId.InvalidElementId;
            string oldUniqueId = (mc != null) ? mc.UniqueId : string.Empty;

            TransactionManager.Instance.EnsureInTransaction(Document);

            // (sic erat scriptum)
            var sp = GetSketchPlaneFromCurve(crv);
            var plane = sp.GetPlane();

            if (CurveUtils.GetPlaneFromCurve(crv, true) == null)
            {
                var flattenCurve = Flatten3dCurveOnPlane(crv, plane);
                mc = Document.IsFamilyDocument
                    ? Document.FamilyCreate.NewModelCurve(flattenCurve, sp)
                    : Document.Create.NewModelCurve(flattenCurve, sp);

                setCurveMethod(mc, crv);
            }
            else
            {
                mc = Document.IsFamilyDocument
                    ? Document.FamilyCreate.NewModelCurve(crv, sp)
                    : Document.Create.NewModelCurve(crv, sp);
            }

            if (mc.SketchPlane.Id != sp.Id)
            {
                //THIS BIZARRE as Revit could use different existing SP, so if Revit had found better plane  this sketch plane has no use
                DocumentManager.Instance.DeleteElement(new ElementUUID(sp.UniqueId));
            }

            InternalSetCurveElement(mc);
            if (oldId != mc.Id && oldId != ElementId.InvalidElementId)
                DocumentManager.Instance.DeleteElement(new ElementUUID(oldUniqueId));
            if (makeReferenceCurve)
                mc.ChangeToReferenceLine();

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(this.InternalElement);

        }

        #endregion

        #region Private mutators

        /// <summary>
        /// Set the curve internally.  Returns false if this method failed to set the curve
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool InternalSetSketchPlaneFromCurve(Autodesk.Revit.DB.Curve c)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);

            // Infer the sketch plane
            Autodesk.Revit.DB.Plane plane = CurveUtils.GetPlaneFromCurve(c, false);

            // attempt to change the sketch plane
            bool needsRemake = false;
            string idSpUnused = resetSketchPlaneMethod(this.InternalCurveElement, c, plane, out needsRemake);

            // if we got a valid id, delete the old sketch plane
            if (idSpUnused != String.Empty)
            {
                DocumentManager.Instance.DeleteElement(new ElementUUID(idSpUnused));
            }

            TransactionManager.Instance.TransactionTaskDone();

            return !needsRemake;
        }

        #endregion

        #region Public constructor

        /// <summary>
        /// Construct a Revit ModelCurve element from a Curve
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static ModelCurve ByCurve(Autodesk.DesignScript.Geometry.Curve curve)
        {
            if (curve == null)
            {
                throw new ArgumentNullException("curve");
            }

            return new ModelCurve(ExtractLegalRevitCurve(curve), false);
        }

        /// <summary>
        /// Construct a Revit ModelCurve element from a Curve
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static ModelCurve ReferenceCurveByCurve(Autodesk.DesignScript.Geometry.Curve curve)
        {
            if (curve == null)
            {
                throw new ArgumentNullException("curve");
            }

            if (!Document.IsFamilyDocument)
                throw new Exception("Revit can only create a ReferenceCurve in a family document!");

           return new ModelCurve(ExtractLegalRevitCurve(curve), true);
        }

        #endregion

        #region Private static constructors
        /// <summary>
        /// Construct a Revit ModelCurve element from an existing element.  The result is Dynamo owned.
        /// </summary>
        /// <param name="modelCurve"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static ModelCurve FromExisting(Autodesk.Revit.DB.ModelCurve modelCurve, bool isRevitOwned)
        {
            return new ModelCurve(modelCurve)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

        #region Helper methods

        private static Autodesk.Revit.DB.Curve ExtractLegalRevitCurve(Autodesk.DesignScript.Geometry.Curve curve)
        {
            // PB:  PolyCurves may have discontinuities that prevent them from being turned into legal Revit ModelCurves.
            // Moreover, a single ModelCurve may not be a composite or multiple curves.

            // One could add a static method to ModelCurve that is an overload of ByCurve - ModelCurve[] ByCurve(PolyCurve).
            // But, this adds an unnecessary high level of complexity to the Element rebinding code.  Hence, we will go the
            // more straightforward route of just providing an informative error message to the user.
            if (curve is PolyCurve)
            {
                throw new Exception(
                    "Revit does not support turning PolyCurves into ModelCurves.  "
                        + "Try exploding your PolyCurve into multiple Curves.");
            }

            return curve.ToRevitType();
        }

        private static bool hasMethodResetSketchPlane = true;

        private static string resetSketchPlaneMethod(Autodesk.Revit.DB.CurveElement mc, Curve c, Autodesk.Revit.DB.Plane flattenedOnPlane, out bool needsSketchPlaneReset)
        {
            //do we need to reset?
            needsSketchPlaneReset = false;
            Autodesk.Revit.DB.Plane newPlane = flattenedOnPlane != null ? flattenedOnPlane : CurveUtils.GetPlaneFromCurve(c, false);

            Autodesk.Revit.DB.Plane curPlane = mc.SketchPlane.GetPlane();

            bool resetPlane = false;

            {
                double llSqCur = curPlane.Normal.DotProduct(curPlane.Normal);
                double llSqNew = newPlane.Normal.DotProduct(newPlane.Normal);
                double dotP = newPlane.Normal.DotProduct(curPlane.Normal);
                double dotSqNormalized = (dotP / llSqCur) * (dotP / llSqNew);
                double angleTol = System.Math.PI / 1800.0;
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
                    sp = GetSketchPlaneFromCurve(c);
                    mc.SketchPlane = GetSketchPlaneFromCurve(c);
                }
                return (sp == null || mc.SketchPlane.Id == sp.Id) ? "" : sp.UniqueId;
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
                        sp = GetSketchPlaneFromCurve(c);
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
                return "";
            }

            if (sp != null && mc.SketchPlane.Id != sp.Id)
                return sp.UniqueId;

            return "";
        }

        private static Autodesk.Revit.DB.SketchPlane GetSketchPlaneFromCurve(Curve c)
        {
            Plane plane = CurveUtils.GetPlaneFromCurve(c, false);
            return Autodesk.Revit.DB.SketchPlane.Create(Document, plane);
        }

        private static Curve Flatten3dCurveOnPlane(Curve c, Plane plane)
        {
            if (c is Autodesk.Revit.DB.HermiteSpline)
            {
                var hs = c as Autodesk.Revit.DB.HermiteSpline;
                plane = CurveUtils.GetPlaneFromCurve(c, false);
                var projPoints = new List<XYZ>();
                foreach (var pt in hs.ControlPoints)
                {
                    var proj = pt - (pt - plane.Origin).DotProduct(plane.Normal) * plane.Normal;
                    projPoints.Add(proj);
                }

                return Autodesk.Revit.DB.HermiteSpline.Create(projPoints, false);
            }

            if (c is Autodesk.Revit.DB.NurbSpline)
            {
                var ns = c as Autodesk.Revit.DB.NurbSpline;
                if (plane == null)
                {
                    var bestFitPlane = Autodesk.DesignScript.Geometry.Plane.ByBestFitThroughPoints(
                        ns.CtrlPoints.ToList().ToPoints(false));

                    plane = bestFitPlane.ToPlane(false);
                }

                var projPoints = new List<XYZ>();
                foreach (var pt in ns.CtrlPoints)
                {
                    var proj = pt - (pt - plane.Origin).DotProduct(plane.Normal) * plane.Normal;
                    projPoints.Add(proj);
                }

                return Autodesk.Revit.DB.NurbSpline.Create(projPoints, ns.Weights.Cast<double>().ToList(), ns.Knots.Cast<double>().ToList(), ns.Degree, ns.isClosed, ns.isRational);
            }

            return c;
        }

        #endregion

        public override string ToString()
        {
            return "ModelCurve";
        }
    }
}
