using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using DSNodeServices;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Curve = Autodesk.DesignScript.Geometry.Curve;
using Face = Autodesk.DesignScript.Geometry.Face;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace DSRevitNodes
{
    /// <summary>
    /// A Revit Adaptive Component
    /// </summary>
    [RegisterForTrace]
    public class DSAdaptiveComponent : AbstractGeometry
    {
        #region Properties

        /// <summary>
        /// Internal variable containing the wrapped Revit object
        /// </summary>
        public Autodesk.Revit.DB.FamilyInstance InternalFamilyInstance
        {
            get; private set;
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Internal constructor for the AdaptiveComponent wrapper
        /// </summary>
        /// <param name="pts">Points to use as reference</param>
        /// <param name="fs">FamilySymbol to place</param>
        private DSAdaptiveComponent(Point[] pts, FamilySymbol fs)
        {

            // if the family instance is present in trace...
            var oldFam =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.FamilyInstance>(Document);

            // just mutate it...
            if (oldFam != null)
            {
                InternalSetFamilyInstance(oldFam);
                InternalSetPositions(pts.ToXyzs());
                return;
            }

            // otherwise create a new family instance...
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            var fam = AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(AbstractGeometry.Document, fs.InternalFamilySymbol);

            if (fam == null)
                throw new Exception("An adaptive component could not be found or created.");

            InternalSetFamilyInstance(fam);
            InternalSetPositions(pts.ToXyzs());

            TransactionManager.GetInstance().TransactionTaskDone();

            // remember this value
            ElementBinder.SetElementForTrace(this.InternalID);
        }

        /// <summary>
        /// Internal constructor for the AdaptiveComponent wrapper
        /// </summary>
        /// <param name="pts">Points to use as reference</param>
        /// <param name="f">Face to use as reference</param>
        /// <param name="fs">FamilySymbol to place</param>
        private DSAdaptiveComponent(double[][] pts, DSFace f, FamilySymbol fs)
        {
            // if the family instance is present in trace...
            var oldFam =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.FamilyInstance>(Document);

            // just mutate it...
            if (oldFam != null)
            {
                InternalSetFamilyInstance(oldFam);
                InternalSetUvsAndFace(pts.ToUvs(), f.InternalFace );
                return;
            }

            // otherwise create a new family instance...
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            var fam = AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(AbstractGeometry.Document, fs.InternalFamilySymbol);

            if (fam == null)
                throw new Exception("An adaptive component could not be found or created.");

            InternalSetFamilyInstance(fam);
            InternalSetUvsAndFace(pts.ToUvs(), f.InternalFace);

            TransactionManager.GetInstance().TransactionTaskDone();

        }

        /// <summary>
        /// Internal constructor for the AdaptiveComponent wrapper
        /// </summary>
        /// <param name="parms">Params on curve to reference</param>
        /// <param name="c">Curve to use as reference</param>
        /// <param name="fs">FamilySymbol to place</param>
        private DSAdaptiveComponent(double[] parms, Curve c, FamilySymbol fs)
        {
            // if the family instance is present in trace...
            var oldFam =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.FamilyInstance>(Document);

            // just mutate it...
            if (oldFam != null)
            {
                InternalSetFamilyInstance(oldFam);
                InternalSetParamsAndCurve(parms, c.InternalCurve);
                return;
            }

            // otherwise create a new family instance...
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            var fam = AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(AbstractGeometry.Document, fs.InternalFamilySymbol);

            if (fam == null)
                throw new Exception("An adaptive component could not be found or created.");

            InternalSetFamilyInstance(fam);
            InternalSetParamsAndCurve(parms, c.InternalCurve);

            TransactionManager.GetInstance().TransactionTaskDone();

        }

        #endregion

        #region Internal mutators

        /// <summary>
        /// Set the internal object and update the id's
        /// </summary>
        /// <param name="ele">The new adaptive component</param>
        private void InternalSetFamilyInstance(Autodesk.Revit.DB.FamilyInstance ele)
        {
            InternalFamilyInstance = ele;
            InternalID = ele.Id;
            InternalUniqueId = ele.UniqueId;
        }

        /// <summary>
        /// Set the positions of the internal family instance from a list of XYZ points
        /// </summary>
        /// <param name="points"></param>
        private void InternalSetPositions( XYZ[] points )
        {
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            IList<ElementId> placePointIds = AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(InternalFamilyInstance);

            if (placePointIds.Count() != points.Count())
                throw new Exception("The input list of points does not have the same number of values required by the adaptive component.");

            // Set the position of each placement point
            int i = 0;
            foreach (var id in placePointIds)
            {
                var point = (Autodesk.Revit.DB.ReferencePoint)Document.GetElement(id);
                point.Position = points[i];
                i++;
            }

            TransactionManager.GetInstance().TransactionTaskDone();
        }

        /// <summary>
        /// Set the positions of the InternalFamilyInstace from an array of uvs
        /// </summary>
        /// <param name="points"></param>
        private void InternalSetUvsAndFace( UV[] uvs, Autodesk.Revit.DB.Face f)
        {
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            var placePointIds = AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(InternalFamilyInstance);

            if (placePointIds.Count() != uvs.Length)
                throw new Exception("The input list of UVs does not have the same number of values required by the adaptive component.");

            // Set the position of each placement point
            int i = 0;
            foreach (var id in placePointIds)
            {
                var uv = uvs[i];
                var point = Document.GetElement(id) as Autodesk.Revit.DB.ReferencePoint;
                var peref = Document.Application.Create.NewPointOnFace(f.Reference, uv);
                point.SetPointElementReference(peref);
                i++;
            }

            TransactionManager.GetInstance().TransactionTaskDone();
        }

        /// <summary>
        /// Set the positions of the InternalFamilyInstace from an array of parameters and curve
        /// </summary>
        /// <param name="points"></param>
        private void InternalSetParamsAndCurve(double[] parms, Autodesk.Revit.DB.Curve c)
        {
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            var placePointIds = AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(InternalFamilyInstance);

            if (placePointIds.Count() != parms.Length)
                throw new Exception("The input list of parameters does not have the same number of values required by the adaptive component.");

            // Set the position of each placement point
            int i = 0;
            foreach (ElementId id in placePointIds)
            {
                var t = parms[i];
                var point = Document.GetElement(id) as Autodesk.Revit.DB.ReferencePoint;
                var ploc = new PointLocationOnCurve(PointOnCurveMeasurementType.NonNormalizedCurveParameter, t,
                                                    PointOnCurveMeasureFrom.Beginning);
                var peref = Document.Application.Create.NewPointOnEdge(c.Reference, ploc);
                point.SetPointElementReference(peref);
                i++;
            }

            TransactionManager.GetInstance().TransactionTaskDone();
        }


        #endregion

        #region Static constructors

        /// <summary>
        /// Create an AdaptiveComponent from a list of points.
        /// </summary>
        /// <param name="pts">The points to reference in the AdaptiveComponent</param>
        /// <param name="fs">The family symbol to use to build the AdaptiveComponent</param>
        /// <returns></returns>
        static DSAdaptiveComponent ByPoints( Point[] pts, FamilySymbol fs )
        {
            return new DSAdaptiveComponent(pts, fs);
        }

        /// <summary>
        /// Create an adaptive component by uv points on a face.
        /// </summary>
        /// <param name="uvs">An array of UV pairs</param>
        /// <param name="f">The face on which to place the AdaptiveComponent</param>
        /// <param name="f">The face on which to place the AdaptiveComponent</param>
        /// <returns></returns>
        static DSAdaptiveComponent ByPointsOnFace(double[][] uvs, DSFace f, FamilySymbol fs)
        {
            return new DSAdaptiveComponent(uvs, f, fs);
        }

        /// <summary>
        /// Create an adaptive component referencing the parameters on a ReferenceCurve
        /// </summary>
        /// <param name="parms">The parameters on the curve</param>
        /// <param name="curve">The curve to reference</param>
        /// <param name="fs">The family symbol to construct</param>
        /// <returns></returns>
        static DSAdaptiveComponent ByPointsOnCurve(double[] parms, Curve curve, FamilySymbol fs)
        {
            return new DSAdaptiveComponent(parms,  curve, fs);
        }

        #endregion

    }
}
