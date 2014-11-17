using System;
using Autodesk.Revit.DB;
using Autodesk.DesignScript.Geometry;
using DSNodeServices;
using Revit.Elements;
using Revit.GeometryConversion;
using Revit.GeometryReferences;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace Revit.Elements
{
    /// <summary>
    /// A Revit Grid Element
    /// </summary>
    [RegisterForTrace]
    public class Grid : Element
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
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalGrid; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Private constructor for wrapping an existing Element
        /// </summary>
        /// <param name="grid"></param>
        private Grid(Autodesk.Revit.DB.Grid grid)
        {
            InternalSetGrid(grid);
        }

        /// <summary>
        /// Private constructor that creates a new Element every time
        /// </summary>
        /// <param name="line"></param>
        private Grid(Autodesk.Revit.DB.Line line)
        {
            // Changing the underlying curve requires destroying the Grid
            TransactionManager.Instance.EnsureInTransaction(Document);

            Autodesk.Revit.DB.Grid g = Document.Create.NewGrid( line );
            InternalSetGrid(g);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.CleanupAndSetElementForTrace(Document, this.InternalElement);
        }

        /// <summary>
        /// Private constructor that creates a new Element every time
        /// </summary>
        /// <param name="arc"></param>
        private Grid(Autodesk.Revit.DB.Arc arc)
        {
            // Changing the underlying curve requires destroying the Grid
            TransactionManager.Instance.EnsureInTransaction(Document);

            Autodesk.Revit.DB.Grid g = Document.Create.NewGrid(arc);
            InternalSetGrid(g);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.CleanupAndSetElementForTrace(Document, this.InternalElement);
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

        /// <summary>
        /// Get the underlying Curve geometry from this Element
        /// </summary>
        public Autodesk.DesignScript.Geometry.Curve Curve
        {
            get
            {
                TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);
                DocumentManager.Regenerate();
                TransactionManager.Instance.TransactionTaskDone();
                return this.InternalGrid.Curve.ToProtoType();
            }
        }

        /// <summary>
        /// Get a Reference to the underlying Curve Geometry of this Element
        /// </summary>
        public ElementCurveReference ElementCurveReference
        {
            get
            {
                return new ElementCurveReference(this.InternalGrid.Curve);
            }
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Create a Revit Grid Element in a Project along a Line.  
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static Grid ByLine(Autodesk.DesignScript.Geometry.Line line)
        {
            if (Document.IsFamilyDocument)
            {
                throw new Exception("A Grid Element can only be created in a Revit Project");
            }

            if (line == null)
            {
                throw new ArgumentNullException("line");
            }

            return new Grid( (Autodesk.Revit.DB.Line) line.ToRevitType());
        }

        /// <summary>
        /// Create a Revit Grid Element in a project between two end points
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static Grid ByStartPointEndPoint(Autodesk.DesignScript.Geometry.Point start, Autodesk.DesignScript.Geometry.Point end)
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

            return new Grid(line);
        }

        /// <summary>
        /// Create a Revit Grid Element in a project along an Arc
        /// </summary>
        /// <param name="arc"></param>
        /// <returns></returns>
        public static Grid ByArc(Autodesk.DesignScript.Geometry.Arc arc)
        {
            if (Document.IsFamilyDocument)
            {
                throw new Exception("A Grid Element can only be created in a Revit Project");
            }

            if (arc == null)
            {
                throw new ArgumentNullException("arc");
            }

            return new Grid( (Autodesk.Revit.DB.Arc) arc.ToRevitType() );
        }

        #endregion

        #region Internal static constructor

        /// <summary>
        /// Wrap an existing Element in the associated DS type
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static Grid FromExisting(Autodesk.Revit.DB.Grid grid, bool isRevitOwned)
        {
            if (grid == null)
            {
                throw new ArgumentNullException("grid");
            }

            return new Grid(grid)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

    }
}
