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
        /// <param name="c"></param>
        private ModelCurve(Autodesk.Revit.DB.Curve c, bool makeReferenceCurve)
        {
            //Phase 1 - Check to see if the object exists and should be rebound
            var mc =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.ModelCurve>(Document);

            //There was a modelcurve, try and set sketch plane
            // if you can't, rebuild 
            if (mc != null)
            {
                InternalSetCurveElement(mc);
                if (!InternalSetSketchPlaneFromCurve(c))
                {
                    InternalSetCurve(c);
                    return;
                }
            }

            ElementId oldId = (mc != null) ? mc.Id : ElementId.InvalidElementId;

            TransactionManager.Instance.EnsureInTransaction(Document);

            // (sic erat scriptum)
            var sp = GetSketchPlaneFromCurve(c);
            var plane = sp.GetPlane();

            if (GetPlaneFromCurve(c, true) == null)
            {
                var flattenCurve = Flatten3dCurveOnPlane(c, plane);
                mc = Document.IsFamilyDocument
                    ? Document.FamilyCreate.NewModelCurve(flattenCurve, sp)
                    : Document.Create.NewModelCurve(flattenCurve, sp);

                setCurveMethod(mc, c);
            }
            else
            {
                mc = Document.IsFamilyDocument
                    ? Document.FamilyCreate.NewModelCurve(c, sp)
                    : Document.Create.NewModelCurve(c, sp);
            }

            if (mc.SketchPlane.Id != sp.Id)
            {
                //THIS BIZARRE as Revit could use different existing SP, so if Revit had found better plane  this sketch plane has no use
                DocumentManager.Instance.DeleteElement(sp.Id);
            }

            InternalSetCurveElement(mc);
            if (oldId != mc.Id && oldId != ElementId.InvalidElementId)
                DocumentManager.Instance.DeleteElement(oldId);
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
            Autodesk.Revit.DB.Plane plane = GetPlaneFromCurve(c, false);

            // attempt to change the sketch plane
            bool needsRemake = false;
            ElementId idSpUnused = resetSketchPlaneMethod(this.InternalCurveElement, c, plane, out needsRemake);

            // if we got a valid id, delete the old sketch plane
            if (idSpUnused != ElementId.InvalidElementId)
            {
                DocumentManager.Instance.DeleteElement(idSpUnused);
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

        // <summary>
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

        private static ElementId resetSketchPlaneMethod(Autodesk.Revit.DB.CurveElement mc, Curve c, Autodesk.Revit.DB.Plane flattenedOnPlane, out bool needsSketchPlaneReset)
        {
            //do we need to reset?
            needsSketchPlaneReset = false;
            Autodesk.Revit.DB.Plane newPlane = flattenedOnPlane != null ? flattenedOnPlane : GetPlaneFromCurve(c, false);

            Autodesk.Revit.DB.Plane curPlane = mc.SketchPlane.Plane;

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
                return ElementId.InvalidElementId;
            }

            if (sp != null && mc.SketchPlane.Id != sp.Id)
                return sp.Id;

            return ElementId.InvalidElementId;
        }

        private static Plane GetPlaneFromCurve(Curve c, bool planarOnly)
        {
            //cases to handle
            //straight line - normal will be inconclusive

            //find the plane of the curve and generate a sketch plane
            double period = c.IsBound ? 0.0 : (c.IsCyclic ? c.Period : 1.0);

            var p0 = c.IsBound ? c.Evaluate(0.0, true) : c.Evaluate(0.0, false);
            var p1 = c.IsBound ? c.Evaluate(0.5, true) : c.Evaluate(0.25 * period, false);
            var p2 = c.IsBound ? c.Evaluate(1.0, true) : c.Evaluate(0.5 * period, false);

            if (c is Line)
            {
                var v1 = p1 - p0;
                var v2 = p2 - p0;
                XYZ norm = null;

                //keep old plane computations
                if (System.Math.Abs(p0.Z - p2.Z) < 0.0001)
                {
                    norm = XYZ.BasisZ;
                }
                else
                {
                    var p3 = new XYZ(p2.X, p2.Y, p0.Z);
                    var v3 = p3 - p0;
                    norm = v1.CrossProduct(v3);
                    if (norm.IsZeroLength())
                    {
                        norm = v2.CrossProduct(XYZ.BasisY);
                    }
                    norm = norm.Normalize();
                }

                return new Plane(norm, p0);

            }

            Autodesk.Revit.DB.CurveLoop cLoop = new Autodesk.Revit.DB.CurveLoop();
            cLoop.Append(c.Clone());
            if (cLoop.HasPlane())
            {
                return cLoop.GetPlane();
            }
            if (planarOnly)
                return null;

            IList<XYZ> points = c.Tessellate();
            List<XYZ> xyzs = new List<XYZ>();
            for (int iPoint = 0; iPoint < points.Count; iPoint++)
                xyzs.Add(points[iPoint]);

            var bestFitPlane =
                Autodesk.DesignScript.Geometry.Plane.ByBestFitThroughPoints(xyzs.ToPoints(false));

            return bestFitPlane.ToPlane(false);
        }

        private static Autodesk.Revit.DB.SketchPlane GetSketchPlaneFromCurve(Curve c)
        {
            Plane plane = GetPlaneFromCurve(c, false);
            return Autodesk.Revit.DB.SketchPlane.Create(Document, plane);
        }

        private static Curve Flatten3dCurveOnPlane(Curve c, Plane plane)
        {
            if (c is Autodesk.Revit.DB.HermiteSpline)
            {
                var hs = c as Autodesk.Revit.DB.HermiteSpline;
                plane = GetPlaneFromCurve(c, false);
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
