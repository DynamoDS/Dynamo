using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using DSNodeServices;
using DSRevitNodes.References;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace DSRevitNodes.Elements
{   
    /// <summary>
    /// A Revit DividedPath
    /// </summary>
    [RegisterForTrace]
    public class DSDividedPath: AbstractElement
    {
        #region Properties

        /// <summary>
        /// Internal variable containing the wrapped Revit object
        /// </summary>
        internal Autodesk.Revit.DB.DividedPath InternalDividedPath
        {
            get;
            private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        internal override Element InternalElement
        {
            get { return InternalDividedPath; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Construct a DSDividedPath from an existing one.  
        /// </summary>
        /// <param name="divPath"></param>
        private DSDividedPath(Autodesk.Revit.DB.DividedPath divPath)
        {
            InternalSetDividedPath(divPath);
        }

        /// <summary>
        /// Private constructor to build a DividedPath
        /// </summary>
        /// <param name="c">Host curves</param>
        /// <param name="divs">Number of divisions</param>
        private DSDividedPath(DSCurveReference[] c, int divs)
        {
            // PB: This constructor always *recreates* the divided path.
            // Mutating the divided path would require obtaining the referenced 
            // curve from the DividedPath, which does not look to be possible 
            // (without an expensive reverse lookup)

            // make sure all of the curves are element references
            var curveRefs = c.Select(x => x.InternalReference).ToList();

            TransactionManager.GetInstance().EnsureInTransaction(Document);

            // build the divided path
            var divPath = Autodesk.Revit.DB.DividedPath.Create( Document, curveRefs );
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
            InternalElementId = divPath.Id;
            InternalUniqueId = divPath.UniqueId;
        }

        #endregion

        #region Static constructors

        public static DSDividedPath ByCurveAndDivisions(DSCurveReference curve, int divisions)
        {
            if (curve == null)
            {
                throw new ArgumentNullException("curves");
            }

            return new DSDividedPath(new[] { curve }, divisions);
        }

        public static DSDividedPath ByCurvesAndDivisions(DSCurveReference[] curve, int divisions)
        {
            if (curve == null)
            {
                throw new ArgumentNullException("curves");
            }

            if (curve.Any(x => x == null))
            {
                throw new ArgumentNullException(String.Format("curves[{0}]",  Array.FindIndex(curve, x => x == null)) );
            }

            return new DSDividedPath(curve, divisions);
        }

        //public static DSDividedPath ByCurveAndDivisionsOfLength(DSCurve c, double length)
        //{
        //    throw new NotImplementedException();
        //}

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Construct this type from an existing Revit element.
        /// </summary>
        /// <param name="dividedPath"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static DSDividedPath FromExisting(Autodesk.Revit.DB.DividedPath dividedPath, bool isRevitOwned)
        {
            return new DSDividedPath(dividedPath)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

    }
}
