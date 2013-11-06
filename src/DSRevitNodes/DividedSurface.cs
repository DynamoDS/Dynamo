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
    class DividedSurface : AbstractGeometry
    {
        #region Properties

        /// <summary>
        /// Internal variable containing the wrapped Revit object
        /// </summary>
        public Autodesk.Revit.DB.DividedSurface InternalDividedSurface
        {
            get; private set;
        }

        #endregion

        #region Private constructors

        private DividedSurface(Face face, int uDivs, int vDivs)
        {
            // if the family instance is present in trace...
            var oldEle =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.DividedSurface>(Document);

            // just mutate it...
            if (oldEle != null)
            {
                InternalDividedSurface = oldEle;
                InternalSetDivisions(uDivs, vDivs);
                return;
            }

            // otherwise create a new family instance...
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            InternalDividedSurface = Document.FamilyCreate.NewDividedSurface(face.InternalFace.Reference);

            if (InternalDividedSurface == null)
                throw new Exception("Revit unexpectedly failed to create the DividedSurface.");

            this.InternalID = InternalDividedSurface.Id;
            InternalSetDivisions(uDivs, vDivs);

            TransactionManager.GetInstance().TransactionTaskDone();
        }

        #endregion

        #region Private mutators

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

        static DividedSurface ByFaceUVDivisions(Face f, int uDivs, int vDivs)
        {
            return new DividedSurface(f, uDivs, vDivs);
        }

        #endregion

    }
}

