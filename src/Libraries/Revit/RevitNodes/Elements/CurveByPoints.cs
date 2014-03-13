using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using DSNodeServices;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Revit.Elements
{
    /// <summary>
    /// A Revit Curve By Points
    /// </summary>
    [RegisterForTrace]
    public class CurveByPoints : CurveElement
    {
        #region private mutators

        private void InternalSetReferencePoints(ReferencePointArray pts)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);
            ((Autodesk.Revit.DB.CurveByPoints) InternalCurveElement).SetPoints(pts);
            TransactionManager.Instance.TransactionTaskDone();
        }

        #endregion

        #region private constructors

        /// <summary>
        /// Construct a model curve from the document.  The result is Dynamo owned
        /// </summary>
        /// <param name="curve"></param>
        private CurveByPoints(Autodesk.Revit.DB.CurveByPoints curveByPoints)
        {
            InternalSetCurveElement(curveByPoints);
        }

        private CurveByPoints(IEnumerable<Autodesk.Revit.DB.ReferencePoint> refPoints)
        {
            //Add all of the elements in the sequence to a ReferencePointArray.
            var refPtArr = new ReferencePointArray();
            foreach (var refPt in refPoints)
            {
                refPtArr.Append(refPt);
            }

            //Phase 1 - Check to see if the object exists and should be rebound
            var cbp =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.CurveByPoints>(Document);

            if (cbp != null)
            {
                InternalSetCurveElement(cbp);
                InternalSetReferencePoints(refPtArr);
                return;
            }

            TransactionManager.Instance.EnsureInTransaction(Document);

            cbp = DocumentManager.Instance.CurrentDBDocument.FamilyCreate.NewCurveByPoints(refPtArr);

            cbp.IsReferenceLine = false;

            InternalSetCurveElement(cbp);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(this.InternalElement);
        }

        #endregion

        #region public constructors

        /// <summary>
        /// Construct a Revit ModelCurve element from a Curve
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static CurveByPoints ByReferencePoints(IEnumerable<ReferencePoint> points)
        {
            if (points.Count() < 2)
            {
                throw new Exception("Cannot create Curve By Points with less than two points.");
            }

            return new CurveByPoints(points.Select(x=>x.InternalReferencePoint));
        }

        /// <summary>
        /// Construct a Revit ModelCurve element from an existing element.  The result is Dynamo owned.
        /// </summary>
        /// <param name="modelCurve"></param>
        /// <returns></returns>
        internal static CurveByPoints FromExisting(Autodesk.Revit.DB.CurveByPoints curveByPoints, bool isRevitOwned)
        {
            return new CurveByPoints(curveByPoints)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion
    
        public override string ToString()
        {
            return "CurveByPoints";
        }
    }
}
