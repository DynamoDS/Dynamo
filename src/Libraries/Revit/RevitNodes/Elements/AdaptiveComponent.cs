using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using DSNodeServices;
using Revit.GeometryConversion;
using Revit.GeometryObjects;
using Revit.GeometryReferences;
using RevitServices.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Point = Autodesk.DesignScript.Geometry.Point;
using Reference = Autodesk.Revit.DB.Reference;

namespace Revit.Elements
{
    /// <summary>
    /// A Revit Adaptive Component
    /// </summary>
    [RegisterForTrace]
    public class AdaptiveComponent : AbstractFamilyInstance
    {
        #region Private constructors

        /// <summary>
        /// Internal constructor for the AdaptiveComponent wrapper
        /// </summary>
        /// <param name="pts">Points to use as reference</param>
        /// <param name="fs">FamilySymbol to place</param>
        private AdaptiveComponent(Point[] pts, FamilySymbol fs)
        {

            // if the family instance is present in trace...
            var oldFam =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.FamilyInstance>(Document);

            // just mutate it...
            if (oldFam != null)
            {
               InternalSetFamilyInstance(oldFam);
                if (fs.InternalFamilySymbol.Id != oldFam.Symbol.Id)
                   InternalSetFamilySymbol(fs);
                InternalSetPositions(pts.ToXyzs());
                return;
            }

            // otherwise create a new family instance...
            TransactionManager.Instance.EnsureInTransaction(Document);

            var fam = AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(Element.Document, fs.InternalFamilySymbol);

            if (fam == null)
                throw new Exception("An adaptive component could not be found or created.");

            InternalSetFamilyInstance(fam);
            InternalSetPositions(pts.ToXyzs());

            TransactionManager.Instance.TransactionTaskDone();

            // remember this value
            ElementBinder.SetElementForTrace(this.InternalElement);
        }

        /// <summary>
        /// Internal constructor for the AdaptiveComponent wrapper
        /// </summary>
        /// <param name="pts">Points to use as reference</param>
        /// <param name="f">Face to use as reference</param>
        /// <param name="fs">FamilySymbol to place</param>
        private AdaptiveComponent(double[][] pts, ElementFaceReference f, FamilySymbol fs)
        {
            // if the family instance is present in trace...
            var oldFam =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.FamilyInstance>(Document);

            // just mutate it...
            if (oldFam != null)
            {
                InternalSetFamilyInstance(oldFam);
                if (fs.InternalFamilySymbol.Id != oldFam.Symbol.Id)
                   InternalSetFamilySymbol(fs);
                InternalSetUvsAndFace(pts.ToUvs(), f.InternalReference );
                return;
            }

            // otherwise create a new family instance...
            TransactionManager.Instance.EnsureInTransaction(Document);

            var fam = AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(Element.Document, fs.InternalFamilySymbol);

            if (fam == null)
                throw new Exception("An adaptive component could not be found or created.");

            InternalSetFamilyInstance(fam);
            InternalSetUvsAndFace(pts.ToUvs(), f.InternalReference );

            TransactionManager.Instance.TransactionTaskDone();

        }

        /// <summary>
        /// Internal constructor for the AdaptiveComponent wrapper
        /// </summary>
        /// <param name="parms">Params on curve to reference</param>
        /// <param name="c">Curve to use as reference</param>
        /// <param name="fs">FamilySymbol to place</param>
        private AdaptiveComponent(double[] parms, Reference c, FamilySymbol fs)
        {
            // if the family instance is present in trace...
            var oldFam =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.FamilyInstance>(Document);

            // just mutate it...
            if (oldFam != null)
            {
                InternalSetFamilyInstance(oldFam);
                if (fs.InternalFamilySymbol.Id != oldFam.Symbol.Id)
                   InternalSetFamilySymbol(fs);
                InternalSetParamsAndCurve(parms, c);
                return;
            }

            // otherwise create a new family instance...
            TransactionManager.Instance.EnsureInTransaction(Document);

            var fam = AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(Element.Document, fs.InternalFamilySymbol);

            if (fam == null)
                throw new Exception("An adaptive component could not be found or created.");

            InternalSetFamilyInstance(fam);
            InternalSetParamsAndCurve(parms, c);

            TransactionManager.Instance.TransactionTaskDone();

        }

        /// <summary>
        /// Internal constructor for existing Elements.
        /// </summary>
        /// <param name="familyInstance"></param>
        private AdaptiveComponent(Autodesk.Revit.DB.FamilyInstance familyInstance)
        {
            InternalSetFamilyInstance(familyInstance);
        }

        #endregion

        #region Internal mutators

       /// <summary>
       /// Set the family symbol for the internal family instance 
       /// </summary>
       /// <param name="famInst"></param>
       /// <param name="fs"></param>
        private void InternalSetFamilySymbol( FamilySymbol fs)
       {
          TransactionManager.Instance.EnsureInTransaction(Document);

          InternalFamilyInstance.Symbol = fs.InternalFamilySymbol;

          TransactionManager.Instance.TransactionTaskDone();

       }

        /// <summary>
        /// Set the positions of the internal family instance from a list of XYZ points
        /// </summary>
        /// <param name="points"></param>
        private void InternalSetPositions( XYZ[] points )
        {
            TransactionManager.Instance.EnsureInTransaction(Document);

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

            TransactionManager.Instance.TransactionTaskDone();
        }

        /// <summary>
        /// Set the positions of the InternalFamilyInstace from an array of uvs
        /// </summary>
        /// <param name="points"></param>
        private void InternalSetUvsAndFace(Autodesk.Revit.DB.UV[] uvs, Autodesk.Revit.DB.Reference faceReference)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);

            var placePointIds = AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(InternalFamilyInstance);

            if (placePointIds.Count() != uvs.Length)
                throw new Exception("The input list of UVs does not have the same number of values required by the adaptive component.");

            // Set the position of each placement point
            int i = 0;
            foreach (var id in placePointIds)
            {
                var uv = uvs[i];
                var point = Document.GetElement(id) as Autodesk.Revit.DB.ReferencePoint;
                var peref = Document.Application.Create.NewPointOnFace(faceReference, uv);
                point.SetPointElementReference(peref);
                i++;
            }

            TransactionManager.Instance.TransactionTaskDone();
        }

        /// <summary>
        /// Set the positions of the InternalFamilyInstace from an array of parameters and curve
        /// </summary>
        /// <param name="points"></param>
        private void InternalSetParamsAndCurve(double[] parms, Autodesk.Revit.DB.Reference c)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);

            var placePointIds = AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(InternalFamilyInstance);

            if (placePointIds.Count() != parms.Length)
                throw new Exception("The input list of parameters does not have the same number of values required by the adaptive component.");

            // Set the position of each placement point
            int i = 0;
            foreach (ElementId id in placePointIds)
            {
                var t = parms[i];
                var point = Document.GetElement(id) as Autodesk.Revit.DB.ReferencePoint;
                var ploc = new PointLocationOnCurve(PointOnCurveMeasurementType.NormalizedCurveParameter, t,
                                                    PointOnCurveMeasureFrom.Beginning);
                var peref = Document.Application.Create.NewPointOnEdge(c, ploc);
                point.SetPointElementReference(peref);
                i++;
            }

            TransactionManager.Instance.TransactionTaskDone();
        }

        #endregion

        #region Public properties

        public FamilySymbol Symbol
        {
            get
            {
                return FamilySymbol.FromExisting(this.InternalFamilyInstance.Symbol, true);
            }
        }

        public List<Point> Locations
        {
            get
            {
                TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);
                DocumentManager.Regenerate();
                var pts = new List<Point>();
                var ids = AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(InternalFamilyInstance);
                TransactionManager.Instance.TransactionTaskDone();
                foreach (var id in ids)
                {
                    var p = DocumentManager.Instance.CurrentDBDocument.GetElement(id) as Autodesk.Revit.DB.ReferencePoint;
                    pts.Add(p.Position.ToPoint());
                }
                return pts;
            }
        }

        #endregion

        #region Static constructors

        /// <summary>
        /// Create an AdaptiveComponent from a list of points.
        /// </summary>
        /// <param name="points">The points to reference in the AdaptiveComponent</param>
        /// <param name="familySymbol">The family symbol to use to build the AdaptiveComponent</param>
        /// <returns></returns>
        public static AdaptiveComponent ByPoints( Point[] points, FamilySymbol familySymbol )
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            if (familySymbol == null)
            {
                throw new ArgumentNullException("familySymbol");
            }

            return new AdaptiveComponent(points, familySymbol);
        }

        /// <summary>
        /// Create an adaptive component by uv points on a face.
        /// </summary>
        /// <param name="uvs">An array of UV pairs</param>
        /// <param name="face">The surface on which to place the AdaptiveComponent</param>
        /// <param name="face">The face on which to place the AdaptiveComponent</param>
        /// <returns></returns>
        public static AdaptiveComponent ByParametersOnFace(double[][] uvs, object face, FamilySymbol familySymbol)
        {
            if (uvs == null)
            {
                throw new ArgumentNullException("uvs");
            }

            if (face == null)
            {
                throw new ArgumentNullException("face");
            }

            if (familySymbol == null)
            {
                throw new ArgumentNullException("familySymbol");
            }

            return new AdaptiveComponent(uvs, ElementFaceReference.TryGetFaceReference(face), familySymbol);
        }

        /// <summary>
        /// Create an adaptive component referencing the parameters on a Curve reference
        /// </summary>
        /// <param name="parameters">The parameters on the curve</param>
        /// <param name="curve">The curve to reference</param>
        /// <param name="familySymbol">The family symbol to construct</param>
        /// <returns></returns>
        public static AdaptiveComponent ByParametersOnCurveReference(double[] parameters, object curve, FamilySymbol familySymbol)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            if (curve == null)
            {
                throw new ArgumentNullException("curve");
            }

            if (familySymbol == null)
            {
                throw new ArgumentNullException("familySymbol");
            }

            return new AdaptiveComponent(parameters, ElementCurveReference.TryGetCurveReference(curve).InternalReference, familySymbol);
        }

        #endregion

        #region Internal static constructor

        /// <summary>
        /// Construct from an existing instance of an AdaptiveComponent. 
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static AdaptiveComponent FromExisting(Autodesk.Revit.DB.FamilyInstance familyInstance, bool isRevitOwned)
        {
            if (familyInstance == null)
            {
                throw new ArgumentNullException("familyInstance");
            }

            // Not all family instances are adaptive components
            if (!AdaptiveComponentInstanceUtils.HasAdaptiveFamilySymbol(familyInstance))
            {
                throw new Exception("The FamilyInstance is not an adaptive component");
            }

            return new AdaptiveComponent(familyInstance)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

        
    }
}
