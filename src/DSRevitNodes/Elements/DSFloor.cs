using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using DSRevitNodes.Elements;
using DSRevitNodes.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Curve = Autodesk.DesignScript.Geometry.Curve;

namespace DSRevitNodes.Elements
{
    /// <summary>
    /// A Revit Floor
    /// </summary>
    public class DSFloor : AbstractElement
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
        internal override Element InternalElement
        {
            get { return InternalFloor; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Private constructor
        /// </summary>
        private DSFloor(Autodesk.Revit.DB.Floor floor)
        {
            InternalSetFloor(floor);
        }
      
        /// <summary>
        /// Private constructor
        /// </summary>
        private DSFloor(Autodesk.Revit.DB.CurveArray curveArray, Autodesk.Revit.DB.FloorType floorType, Autodesk.Revit.DB.Level level)
        {
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            // we assume the floor is not structural here, this may be a bad assumption
            var floor = Document.Create.NewFloor(curveArray, floorType, level, false);

            InternalSetFloor( floor );

            TransactionManager.GetInstance().TransactionTaskDone();

            ElementBinder.CleanupAndSetElementForTrace(Document, this.InternalElementId);
        }

        #endregion

        #region Private mutators


        /// <summary>
        /// Set the InternalFloor property and the associated element id and unique id
        /// </summary>
        /// <param name="floor"></param>
        private void InternalSetFloor(Autodesk.Revit.DB.Floor floor)
        {
            this.InternalFloor = floor;
            this.InternalElementId = floor.Id;
            this.InternalUniqueId = floor.UniqueId;
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Create a Revit Floor given it's curve outline and Level
        /// </summary>
        /// <param name="outline"></param>
        /// <param name="level"></param>
        /// <returns>The floor</returns>
        public static DSFloor ByOutline( Autodesk.DesignScript.Geometry.Curve[] outline, DSFloorType floorType, DSLevel level)
        {
            if (outline == null)
            {
                throw new ArgumentNullException("outline");
            }

            if ( level == null )
            {
                throw new ArgumentNullException("level");
            }

            if (outline.Count() < 3)
            {
                throw new Exception("Outline must have at least 3 edges to enclose an area.");
            }

            var ca = new CurveArray();
            outline.ToList().ForEach(x => ca.Append(x.ToRevitType())); 

            return new DSFloor(ca, floorType.InternalFloorType, level.InternalLevel );
        }
        #endregion

        #region Internal static constructors

        /// <summary>
        /// Create a Floor from a user selected Element.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static DSFloor FromExisting(Autodesk.Revit.DB.Floor floor, bool isRevitOwned)
        {
            return new DSFloor(floor)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

    }
}
