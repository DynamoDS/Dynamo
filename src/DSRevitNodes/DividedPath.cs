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
    class DividedPath: AbstractGeometry
    {
        #region Properties

        /// <summary>
        /// Internal variable containing the wrapped Revit object
        /// </summary>
        public Autodesk.Revit.DB.DividedPath InternalDividedPath
        {
            get; private set;
        }

        #endregion

        #region Private constructors

        private DividedPath(Curve c, int divs)
        {
            // Note: This constructor always *recreates* the divided path.
            // Mutating the divided path would requireobtaining the referenced 
            // curve from the DividedPath, which does not look to be possible 
            // (without an expensive reverse lookup)

            // let's create the new curve
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            //...just make a divided path and store it.
            InternalDividedPath = Autodesk.Revit.DB.DividedPath.Create(Document,
                new List<Reference>() {c.InternalCurve.Reference});
            InternalDividedPath.FixedNumberOfPoints = divs;

            TransactionManager.GetInstance().TransactionTaskDone();
        }

        #endregion

        #region Static constructors

        static DividedPath ByCurveAndEqualDivisions(Curve c, int divisions)
        {
            return new DividedPath(c, divisions);
        }

        static DividedPath ByCurveAndDivisionsOfLength(Curve c, double length)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
