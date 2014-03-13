using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using DSNodeServices;
using Revit.References;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Revit.Elements
{   
    /// <summary>
    /// A Revit DividedPath
    /// </summary>
    [RegisterForTrace]
    public class DividedPath: AbstractElement
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
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalDividedPath; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Construct a DSDividedPath from an existing one.  
        /// </summary>
        /// <param name="divPath"></param>
        private DividedPath(Autodesk.Revit.DB.DividedPath divPath)
        {
            InternalSetDividedPath(divPath);
        }

        /// <summary>
        /// Private constructor to build a DividedPath
        /// </summary>
        /// <param name="c">Host curves</param>
        /// <param name="divs">Number of divisions</param>
        private DividedPath(CurveReference[] c, int divs)
        {
            // PB: This constructor always *recreates* the divided path.
            // Mutating the divided path would require obtaining the referenced 
            // curve from the DividedPath, which does not look to be possible 
            // (without an expensive reverse lookup)

            // make sure all of the curves are element references
            var curveRefs = c.Select(x => x.InternalReference).ToList();

            TransactionManager.Instance.EnsureInTransaction(Document);

            // build the divided path
            var divPath = Autodesk.Revit.DB.DividedPath.Create( Document, curveRefs );
            divPath.FixedNumberOfPoints = divs;

            // set internally
            InternalSetDividedPath(divPath);

            TransactionManager.Instance.TransactionTaskDone();

            // delete any cached ele and set this new one
            ElementBinder.CleanupAndSetElementForTrace(Document, this.InternalElement);
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

        public static DividedPath ByCurveAndDivisions(CurveReference curve, int divisions)
        {
            if (curve == null)
            {
                throw new ArgumentNullException("curve");
            }

            if (divisions < 2)
            {
                throw new Exception("The number of divisions must be greater than 2!");
            }

            return new DividedPath(new[] { curve }, divisions);
        }

        public static DividedPath ByCurvesAndDivisions(CurveReference[] curves, int divisions)
        {
            if (curves == null)
            {
                throw new ArgumentNullException("curves");
            }

            if (divisions < 2)
            {
                throw new Exception("The number of divisions must be greater than 2!");
            }

            if (curves.Any(x => x == null))
            {
                throw new ArgumentNullException(String.Format("curves[{0}]",  Array.FindIndex(curves, x => x == null)) );
            }

            return new DividedPath(curves, divisions);
        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Construct this type from an existing Revit element.
        /// </summary>
        /// <param name="dividedPath"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static DividedPath FromExisting(Autodesk.Revit.DB.DividedPath dividedPath, bool isRevitOwned)
        {
            return new DividedPath(dividedPath)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

    }
}
