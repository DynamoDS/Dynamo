using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using DSNodeServices;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Curve = Autodesk.DesignScript.Geometry.Curve;

namespace DSRevitNodes
{   
    /// <summary>
    /// A Revit DividedPath
    /// </summary>
    [RegisterForTrace]
    class DSDividedPath: AbstractElement
    {
        #region Properties

        /// <summary>
        /// Internal variable containing the wrapped Revit object
        /// </summary>
        protected Autodesk.Revit.DB.DividedPath InternalDividedPath
        {
            get;
            private set;
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Private constructor to build a DividedPath
        /// </summary>
        /// <param name="c">Host curve</param>
        /// <param name="divs">Number of divisions</param>
        private DSDividedPath(DSCurve c, int divs)
        {
            // PB: This constructor always *recreates* the divided path.
            // Mutating the divided path would require obtaining the referenced 
            // curve from the DividedPath, which does not look to be possible 
            // (without an expensive reverse lookup)

            TransactionManager.GetInstance().EnsureInTransaction(Document);

            // build the divided path
            var divPath = Autodesk.Revit.DB.DividedPath.Create(Document,
                new List<Reference>() {c.InternalCurve.Reference});
            divPath.FixedNumberOfPoints = divs;

            // set internally
            InternalSetDividedPath(divPath);

            TransactionManager.GetInstance().TransactionTaskDone();

            // delete any cached ele and set this new one
            ElementBinder.CleanupAndSetElementForTrace(Document, InternalDividedPath.Id);
        }

        #endregion

        #region Private mutators

        /// <summary>
        /// Set the internal object and update the id's
        /// </summary>
        /// <param name="divPath">The divided path</param>
        private void InternalSetDividedPath(Autodesk.Revit.DB.DividedPath divPath)
        {
            InternalDividedPath = divPath;
            InternalId = divPath.Id;
            InternalUniqueId = divPath.UniqueId;
        }

        #endregion

        #region Static constructors

        static DSDividedPath ByCurveAndEqualDivisions(DSCurve c, int divisions)
        {
            return new DSDividedPath(c, divisions);
        }

        static DSDividedPath ByCurveAndDivisionsOfLength(DSCurve c, double length)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
