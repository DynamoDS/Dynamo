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
    class DSDividedPath: AbstractGeometry
    {
        #region Properties

        /// <summary>
        /// Internal variable containing the wrapped Revit object
        /// </summary>
        public Autodesk.Revit.DB.DividedPath InternalDividedPath
        {
            get;
            private set;
        }

        #endregion

        #region Private constructors

        private DSDividedPath(Curve c, int divs)
        {
            // Note: This constructor always *recreates* the divided path.
            // Mutating the divided path would requireobtaining the referenced 
            // curve from the DividedPath, which does not look to be possible 
            // (without an expensive reverse lookup)

            // let's create the new curve
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            var divPath = Autodesk.Revit.DB.DividedPath.Create(Document,
                new List<Reference>() {c.InternalCurve.Reference});
            divPath.FixedNumberOfPoints = divs;
            InternalSetDividedPath(divPath);

            TransactionManager.GetInstance().TransactionTaskDone();

            ElementBinder.CleanupAndSetElementForTrace(Document, InternalDividedPath.Id);
        }

        #endregion

        private void InternalSetDividedPath(Autodesk.Revit.DB.DividedPath divPath)
        {
            InternalDividedPath = divPath;
            InternalID = divPath.Id;
            InternalUniqueId = divPath.UniqueId;
        }

        #region Static constructors

        static DSDividedPath ByCurveAndEqualDivisions(Curve c, int divisions)
        {
            return new DSDividedPath(c, divisions);
        }

        static DSDividedPath ByCurveAndDivisionsOfLength(Curve c, double length)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
