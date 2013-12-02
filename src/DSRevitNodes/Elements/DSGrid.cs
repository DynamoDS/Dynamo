using System;
using Autodesk.Revit.DB;
using Autodesk.DesignScript.Geometry;
using DSNodeServices;
using DSRevitNodes.Elements;
using DSRevitNodes.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace DSRevitNodes
{
    /// <summary>
    /// A Revit Grid Element
    /// </summary>
    [RegisterForTrace]
    public class DSGrid : AbstractElement
    {
        #region Internal properties

        /// <summary>
        /// Internal reference to Element
        /// </summary>
        internal Autodesk.Revit.DB.Grid InternalGrid
        {
            get; private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        internal override Element InternalElement
        {
            get { return InternalGrid; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Private constructor for wrapping an existing Element
        /// </summary>
        /// <param name="grid"></param>
        private DSGrid(Autodesk.Revit.DB.Grid grid)
        {
            InternalSetGrid(grid);
        }

        /// <summary>
        /// Private constructor that creates a new Element every time
        /// </summary>
        /// <param name="line"></param>
        private DSGrid(Autodesk.Revit.DB.Line line)
        {
            // Changing the underlying curve requires destroying the Grid
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            Autodesk.Revit.DB.Grid g = Document.Create.NewGrid( line );
            InternalSetGrid(g);

            TransactionManager.GetInstance().TransactionTaskDone();

            ElementBinder.CleanupAndSetElementForTrace(Document, this.InternalElementId);
        }

        /// <summary>
        /// Private constructor that creates a new Element every time
        /// </summary>
        /// <param name="arc"></param>
        private DSGrid(Autodesk.Revit.DB.Arc arc)
        {
            // Changing the underlying curve requires destroying the Grid
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            Autodesk.Revit.DB.Grid g = Document.Create.NewGrid(arc);
            InternalSetGrid(g);

            TransactionManager.GetInstance().TransactionTaskDone();

            ElementBinder.CleanupAndSetElementForTrace(Document, this.InternalElementId);
        }

        #endregion

        #region Private mutators

        /// <summary>
        /// Set the internal Element, ElementId, and UniqueId
        /// </summary>
        /// <param name="grid"></param>
        private void InternalSetGrid(Autodesk.Revit.DB.Grid grid)
        {
            this.InternalGrid = grid;
            this.InternalElementId = grid.Id;
            this.InternalUniqueId = grid.UniqueId;
        }

        #endregion

        #region Public properties

        public Autodesk.DesignScript.Geometry.Curve Curve
        {
            get
            {
                return this.InternalGrid.Curve.ToProtoType();
            }
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Create a Revit Grid Element in a Project along a Line.  
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static DSGrid ByLine(Autodesk.DesignScript.Geometry.Line line)
        {
            if (Document.IsFamilyDocument)
            {
                throw new Exception("A Grid Element can only be created in a Revit Project");
            }

            if (line == null)
            {
                throw new ArgumentNullException("line");
            }

            return new DSGrid( (Autodesk.Revit.DB.Line) line.ToRevitType());
        }

        /// <summary>
        /// Create a Revit Grid Element in a project between two end points
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static DSGrid ByStartAndEndPoint(Autodesk.DesignScript.Geometry.Point start, Autodesk.DesignScript.Geometry.Point end)
        {
            if (Document.IsFamilyDocument)
            {
                throw new Exception("A Grid Element can only be created in a Revit Project");
            }

            if (start == null)
            {
                throw new ArgumentNullException("start");
            }

            if (end == null)
            {
                throw new ArgumentNullException("end");
            }

            var line = Autodesk.Revit.DB.Line.CreateBound(start.ToXyz(), end.ToXyz());

            return new DSGrid(line);
        }

        /// <summary>
        /// Create a Revit Grid Element in a project along an Arc
        /// </summary>
        /// <param name="arc"></param>
        /// <returns></returns>
        public static DSGrid ByArc(Autodesk.DesignScript.Geometry.Arc arc)
        {
            if (Document.IsFamilyDocument)
            {
                throw new Exception("A Grid Element can only be created in a Revit Project");
            }

            if (arc == null)
            {
                throw new ArgumentNullException("arc");
            }

            return new DSGrid( (Autodesk.Revit.DB.Arc) arc.ToRevitType() );
        }

        #endregion

        #region Internal static constructor

        /// <summary>
        /// Wrap an existing Element in the associated DS type
        /// </summary>
        /// <param name="grid"></param>
        /// <returns></returns>
        internal static DSGrid FromExisting(Autodesk.Revit.DB.Grid grid, bool isRevitOwned)
        {
            if (grid == null)
            {
                throw new ArgumentNullException("grid");
            }

            return new DSGrid(grid)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

    }
}
