using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using DSNodeServices;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Face = Autodesk.DesignScript.Geometry.Face;

namespace DSRevitNodes
{
    /// <summary>
    /// A Revit DividedSurface
    /// </summary>
    [RegisterForTrace]
    class DSDividedSurface : AbstractGeometry
    {
        #region Private Properties

        /// <summary>
        /// Internal variable containing the wrapped Revit object
        /// </summary>
        public Autodesk.Revit.DB.DividedSurface InternalDividedSurface
        {
            get; private set;
        }

        #endregion

        #region Private constructors

        private DSDividedSurface(DSFace dsFace, int uDivs, int vDivs)
        {
            // if the family instance is present in trace...
            var oldEle =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.DividedSurface>(Document);

            // just mutate it...
            if (oldEle != null)
            {
                InternalSetDividedSurface(oldEle);
                InternalSetDivisions(uDivs, vDivs);
                return;
            }

            // otherwise create a new family instance...
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            var divSurf = Document.FamilyCreate.NewDividedSurface(dsFace.InternalFace.Reference);

            InternalSetDividedSurface(divSurf);
            InternalSetDivisions(uDivs, vDivs);

            TransactionManager.GetInstance().TransactionTaskDone();

            // remember this new value
            ElementBinder.SetElementForTrace(this.InternalID);
        }

        #endregion

        #region Private mutators

        private void InternalSetDividedSurface(Autodesk.Revit.DB.DividedSurface divSurf)
        {
            this.InternalDividedSurface = divSurf;
            this.InternalID = divSurf.Id;
            this.InternalUniqueId = divSurf.UniqueId;
        }

        private void InternalSetDivisions(int uDivs, int vDivs)
        {
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            if (InternalDividedSurface.USpacingRule.Number != uDivs)
                InternalDividedSurface.USpacingRule.Number = uDivs;
            if (InternalDividedSurface.VSpacingRule.Number != vDivs)
                InternalDividedSurface.VSpacingRule.Number = vDivs;

            TransactionManager.GetInstance().TransactionTaskDone();
        }

        #endregion

        #region Static constructors

        static DSDividedSurface ByFaceUVDivisions(DSFace f, int uDivs, int vDivs)
        {
            return new DSDividedSurface(f, uDivs, vDivs);
        }

        #endregion

    }
}

