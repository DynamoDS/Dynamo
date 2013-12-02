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
    [RegisterForTrace]
    public class DSWall : AbstractElement
    {
        #region Internal Properties

        /// <summary>
        /// Internal reference to the Revit Element
        /// </summary>
        internal Autodesk.Revit.DB.Wall InternalWall
        {
            get; private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        internal override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalWall; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Create from an existing Revit Element
        /// </summary>
        /// <param name="wall"></param>
        private DSWall(Autodesk.Revit.DB.Wall wall)
        {
            InternalSetWall(wall);
        }

        /// <summary>
        /// Create a new WallType, deleting the original
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="wallType"></param>
        /// <param name="baseLevel"></param>
        /// <param name="height"></param>
        /// <param name="offset"></param>
        /// <param name="flip"></param>
        /// <param name="isStructural"></param>
        private DSWall(Autodesk.Revit.DB.Curve curve, Autodesk.Revit.DB.WallType wallType, Autodesk.Revit.DB.Level baseLevel, double height, double offset, bool flip, bool isStructural)
        {
            // This creates a new wall and deletes the old one
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            var wall = Wall.Create(Document, curve, wallType.Id, baseLevel.Id, height, offset, flip, isStructural);
            InternalSetWall(wall);

            TransactionManager.GetInstance().TransactionTaskDone();

            // delete the element stored in trace and add this new one
            ElementBinder.CleanupAndSetElementForTrace(Document, this.InternalElementId);
        }

        #endregion

        #region Private mutators

        /// <summary>
        /// Set the internal Element, ElementId, and UniqueId
        /// </summary>
        /// <param name="wall"></param>
        private void InternalSetWall(Autodesk.Revit.DB.Wall wall)
        {
            this.InternalWall = wall;
            this.InternalElementId = wall.Id;
            this.InternalUniqueId = wall.UniqueId;
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Create a Revit Wall from a guiding Curve, height, Level, and WallType
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="height"></param>
        /// <param name="level"></param>
        /// <param name="wallType"></param>
        /// <returns></returns>
        public static DSWall ByCurveAndHeight(Autodesk.DesignScript.Geometry.Curve curve, double height, DSLevel level, DSWallType wallType)
        {
            if (curve == null)
            {
                throw new ArgumentNullException("curve");
            }

            if (level == null)
            {
                throw new ArgumentNullException("level");
            }

            if (wallType == null)
            {
                throw new ArgumentNullException("wallType");
            }

            return new DSWall(curve.ToRevitType(), wallType.InternalWallType, level.InternalLevel, height, 0.0, false, false);
        }

        /// <summary>
        /// Create a Revit Wall from a guiding Curve, start Level, end Level, and WallType
        /// </summary>
        /// <param name="c"></param>
        /// <param name="startLevel"></param>
        /// <param name="endLevel"></param>
        /// <param name="wallType"></param>
        /// <returns></returns>
        public static DSWall ByCurveAndLevels(Autodesk.DesignScript.Geometry.Curve c, DSLevel startLevel, DSLevel endLevel, DSWallType wallType)
        {
            if (endLevel == null)
            {
                throw new ArgumentNullException("endLevel");
            }

            if (startLevel == null)
            {
                throw new ArgumentNullException("startLevel");
            }

            var height = endLevel.Elevation - startLevel.Elevation;

            return DSWall.ByCurveAndHeight(c, height, startLevel, wallType);
        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Create a Revit Wall from an existing reference
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static DSWall FromExisting(Autodesk.Revit.DB.Wall wall, bool isRevitOwned)
        {
            return new DSWall(wall)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion


    }
}

/*

  [NodeName("Wall by Curve")]
    [NodeCategory(BuiltinNodeCategories.REVIT_DOCUMENT)]
    [NodeDescription("WARNING!  Recreated, not modified on change.  Create a wall given a curve, a level, a wall type, and a height.")]
    public class WallByCurve : RevitTransactionNodeWithOneOutput
    {
        public WallByCurve()
        {
            InPortData.Add(new PortData("curve", "A curve.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("level", "A level to associate this wall with.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("wall type", "The wall type to use for the wall.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("height", "The height of the wall.", typeof(FScheme.Value.Number)));

            OutPortData.Add(new PortData("wall", "The wall.", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(Microsoft.FSharp.Collections.FSharpList<FScheme.Value> args)
        {
            //if we're in a family document, don't even try to add a floor
            if (dynRevitSettings.Doc.Document.IsFamilyDocument)
            {
                throw new Exception("Walls can not be created in family documents.");
            }

            var curve = (Curve)((FScheme.Value.Container)args[0]).Item;
            var level = (Autodesk.Revit.DB.Level)((FScheme.Value.Container)args[1]).Item;
            var wallType = (WallType)((FScheme.Value.Container)args[2]).Item;
            var height = ((FScheme.Value.Number)args[3]).Item;

            Wall wall = null;

            if (this.Elements.Any())
            {

                if (dynUtils.TryGetElement(this.Elements[0], out wall))
                {
                    //Delete the existing floor. Revit API does not allow update of floor sketch.
                    dynRevitSettings.Doc.Document.Delete(wall.Id);
                }

                wall = Wall.Create(dynRevitSettings.Doc.Document, curve, wallType.Id, level.Id, height, 0.0, false, false);
                this.Elements[0] = wall.Id;

            }
            else
            {
                wall = Wall.Create(dynRevitSettings.Doc.Document,curve, wallType.Id, level.Id, height, 0.0, false, false);
                Elements.Add(wall.Id);
            }

            return FScheme.Value.NewContainer(wall);
        }
    }

    [NodeName("Select Wall Type")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a wall type.")]
    public class SelectWallType : DropDrownBase
    {
        public SelectWallType()
        {
            OutPortData.Add(new PortData("wall type", "The selected wall type.", typeof(FScheme.Value.Container)));

            RegisterAllPorts();

            PopulateItems();
        }

        public override void PopulateItems()
        {
            var wallTypesColl = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            wallTypesColl.OfClass(typeof(WallType));

            Items.Clear();

            wallTypesColl.ToElements().ToList().ForEach(x => Items.Add(new DynamoDropDownItem(x.Name, x)));
        }
    }
*/