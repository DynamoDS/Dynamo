using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;

using Revit.GeometryConversion;
using Revit.GeometryReferences;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Revit.Elements
{   
    /// <summary>
    /// A Revit DividedPath
    /// </summary>
    [DSNodeServices.RegisterForTrace]
    public class DividedPath: Element
    {
        #region Private fields
        
        private static Options geometryOptions = new Options
        {
            ComputeReferences = true,
            DetailLevel = ViewDetailLevel.Medium,
            IncludeNonVisibleObjects = false
        };

        #endregion

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

        /// <summary>
        /// All points along the DividedPath.
        /// </summary>
        public IEnumerable<Autodesk.DesignScript.Geometry.Point> Points
        {
            get
            {
                DocumentManager.Regenerate();
                return Geometry().Cast<Autodesk.DesignScript.Geometry.Point>();
            }
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
        private DividedPath(ElementCurveReference[] c, int divs)
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

        #region Hidden public static constructors

        // These constructors are hidden, but allow this constructor to be more tolerant of
        // incorrect types without breaking replication guides

        [IsVisibleInDynamoLibrary(false)]
        public static DividedPath ByCurveAndDivisions(ElementCurveReference element, int divisions)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            if (divisions < 2)
            {
                throw new Exception("The number of divisions must be greater than 2!");
            }

            return new DividedPath(new[] { ElementCurveReference.TryGetCurveReference(element) }, divisions);
        }

        [IsVisibleInDynamoLibrary(false)]
        public static DividedPath ByCurvesAndDivisions(ElementCurveReference[] curveReferences, int divisions)
        {
            if (curveReferences == null)
            {
                throw new ArgumentNullException("curveReferences");
            }

            if (divisions < 2)
            {
                throw new Exception("The number of divisions must be greater than 2!");
            }

            if (curveReferences.Any(x => x == null))
            {
                throw new ArgumentNullException(String.Format("curves[{0}]",  Array.FindIndex(curveReferences, x => x == null)) );
            }

            return new DividedPath(curveReferences.Select(x => ElementCurveReference.TryGetCurveReference(x)).ToArray(), divisions);
        }

        [IsVisibleInDynamoLibrary(false)]
        public static DividedPath ByCurveAndDivisions(Revit.Elements.Element element, int divisions)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            if (divisions < 2)
            {
                throw new Exception("The number of divisions must be greater than 2!");
            }

            return new DividedPath(new[] { ElementCurveReference.TryGetCurveReference(element) }, divisions);
        }

        [IsVisibleInDynamoLibrary(false)]
        public static DividedPath ByCurvesAndDivisions(Revit.Elements.Element[] elements, int divisions)
        {
            if (elements == null)
            {
                throw new ArgumentNullException("elements");
            }

            if (divisions < 2)
            {
                throw new Exception("The number of divisions must be greater than 2!");
            }

            if (elements.Any(x => x == null))
            {
                throw new ArgumentNullException(String.Format("curves[{0}]", Array.FindIndex(elements, x => x == null)));
            }

            return new DividedPath(elements.Select(x => ElementCurveReference.TryGetCurveReference(x)).ToArray(), divisions);
        }

        #endregion

        #region Static constructors

        public static DividedPath ByCurveAndDivisions(Autodesk.DesignScript.Geometry.Curve curve, int divisions)
        {
            if (curve == null)
            {
                throw new ArgumentNullException("curve");
            }

            if (divisions < 2)
            {
                throw new Exception("The number of divisions must be greater than 2!");
            }

            return new DividedPath(new[] { ElementCurveReference.TryGetCurveReference(curve) }, divisions);
        }

        public static DividedPath ByCurvesAndDivisions(Autodesk.DesignScript.Geometry.Curve[] curve, int divisions)
        {
            if (curve == null)
            {
                throw new ArgumentNullException("curve");
            }

            if (divisions < 2)
            {
                throw new Exception("The number of divisions must be greater than 2!");
            }

            if (curve.Any(x => x == null))
            {
                throw new ArgumentNullException(String.Format("curves[{0}]", Array.FindIndex(curve, x => x == null)));
            }

            return new DividedPath(curve.Select(x => ElementCurveReference.TryGetCurveReference(x)).ToArray(), divisions);
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
