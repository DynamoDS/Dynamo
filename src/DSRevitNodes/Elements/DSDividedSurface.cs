using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using DSNodeServices;
using DSRevitNodes.References;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Face = Autodesk.DesignScript.Geometry.Face;

namespace DSRevitNodes.Elements
{
    /// <summary>
    /// A Revit DividedSurface
    /// </summary>
    [RegisterForTrace]
    public class DSDividedSurface : AbstractElement
    {
        #region Private Properties

        /// <summary>
        /// Internal variable containing the wrapped Revit object
        /// </summary>
        internal Autodesk.Revit.DB.DividedSurface InternalDividedSurface
        {
            get; private set;
        }

        #endregion

        #region Private constructors

        private DSDividedSurface(DSFaceReference dsFace, int uDivs, int vDivs, double rotation)
        {
            // if the family instance is present in trace...
            var oldEle =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.DividedSurface>(Document);

            // just mutate it...
            if (oldEle != null)
            {
                InternalSetDividedSurface(oldEle);
                InternalSetDivisions(uDivs, vDivs);
                InternalSetRotation(rotation);
                return;
            }

            // otherwise create a new family instance...
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            var divSurf = Document.FamilyCreate.NewDividedSurface(dsFace.InternalReference);

            InternalSetDividedSurface(divSurf);
            InternalSetDivisions(uDivs, vDivs);
            InternalSetRotation(rotation);

            TransactionManager.GetInstance().TransactionTaskDone();

            // remember this new value
            ElementBinder.SetElementForTrace(this.InternalElementId);
        }

        #endregion

        #region Public properties

        public int UDivisions
        {
            get
            {
                return InternalDividedSurface.NumberOfUGridlines + 1;
            }
        }

        public int VDivisions
        {
            get
            {
                return InternalDividedSurface.NumberOfVGridlines + 1;
            }
        }

        public double Rotation
        {
            get
            {
                return InternalDividedSurface.AllGridRotation;
            }
        }

        #endregion

        #region Private mutators

        private void InternalSetDividedSurface(Autodesk.Revit.DB.DividedSurface divSurf)
        {
            this.InternalDividedSurface = divSurf;
            this.InternalElementId = divSurf.Id;
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

        private void InternalSetRotation(double rotation)
        {
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            InternalDividedSurface.AllGridRotation = rotation;

            TransactionManager.GetInstance().TransactionTaskDone();
        }

        #endregion

        #region Static constructors

        public static DSDividedSurface ByFaceUVDivisions(DSFaceReference f, int uDivs, int vDivs)
        {
            return new DSDividedSurface(f, uDivs, vDivs, 0.0);
        }

        public static DSDividedSurface ByFaceUVDivisionsRotation(DSFaceReference f, int uDivs, int vDivs, double rotation)
        {
            return new DSDividedSurface(f, uDivs, vDivs, rotation);
        }

        #endregion

    }
}

