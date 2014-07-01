using System;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Curve = Autodesk.DesignScript.Geometry.Curve;

namespace Revit.Elements
{
    /// <summary>
    /// A Revit Floor
    /// </summary>
    public class Floor : Element
    {
        #region Internal properties

        /// <summary>
        /// An internal handle on the Revit floor
        /// </summary>
        internal Autodesk.Revit.DB.Floor InternalFloor
        {
            get; private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalFloor; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Private constructor
        /// </summary>
        private Floor(Autodesk.Revit.DB.Floor floor)
        {
            InternalSetFloor(floor);
        }
      
        /// <summary>
        /// Private constructor
        /// </summary>
        private Floor(CurveArray curveArray, Autodesk.Revit.DB.FloorType floorType, Autodesk.Revit.DB.Level level)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);

            // we assume the floor is not structural here, this may be a bad assumption
            var floor = Document.Create.NewFloor(curveArray, floorType, level, false);

            InternalSetFloor( floor );

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.CleanupAndSetElementForTrace(Document, InternalFloor);
        }

        #endregion

        #region Private mutators


        /// <summary>
        /// Set the InternalFloor property and the associated element id and unique id
        /// </summary>
        /// <param name="floor"></param>
        private void InternalSetFloor(Autodesk.Revit.DB.Floor floor)
        {
            InternalFloor = floor;
            InternalElementId = floor.Id;
            InternalUniqueId = floor.UniqueId;
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Create a Revit Floor given it's curve outline and Level
        /// </summary>
        /// <param name="outlineCurves"></param>
        /// <param name="floorType"></param>
        /// <param name="level"></param>
        /// <returns>The floor</returns>
        public static Floor ByOutlineTypeAndLevel(Curve[] outlineCurves, FloorType floorType, Level level)
        {
            if (outlineCurves == null)
            {
                throw new ArgumentNullException("outlineCurves");
            }

            return ByOutlineTypeAndLevel(PolyCurve.ByJoinedCurves(outlineCurves), floorType, level);
        }

        /// <summary>
        /// Create a Revit Floor given it's curve outline and Level
        /// </summary>
        /// <param name="outline"></param>
        /// <param name="floorType"></param>
        /// <param name="level"></param>
        /// <returns>The floor</returns>
        public static Floor ByOutlineTypeAndLevel(PolyCurve outline, FloorType floorType, Level level)
        {
            if (outline == null)
            {
                throw new ArgumentNullException("outline");
            }

            if (floorType == null)
            {
                throw new ArgumentNullException("floorType");
            }

            if ( level == null )
            {
                throw new ArgumentNullException("level");
            }

            if (!outline.IsClosed)
            {
                throw new ArgumentException("The input PolyCurve is not closed");
            }

            var ca = new CurveArray();
            outline.Curves().ForEach(x => ca.Append(x.ToRevitType())); 

            return new Floor(ca, floorType.InternalFloorType, level.InternalLevel );
        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Create a Floor from a user selected Element.
        /// </summary>
        /// <param name="floor"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static Floor FromExisting(Autodesk.Revit.DB.Floor floor, bool isRevitOwned)
        {
            return new Floor(floor)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

    }
}
