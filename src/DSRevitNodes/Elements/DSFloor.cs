using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using DSRevitNodes.Elements;
using DSRevitNodes.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Curve = Autodesk.DesignScript.Geometry.Curve;

namespace DSRevitNodes
{
    /// <summary>
    /// A Revit Floor
    /// </summary>
    public class DSFloor : AbstractElement
    {
        internal Autodesk.Revit.DB.Floor InternalFloor
        {
            get; private set;
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

        public static DSFloor ByOutline( Autodesk.DesignScript.Geometry.Curve[] outline, DSLevel level)
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

            return new DSFloor(ca, level.InternalLevel );
        }
    }
}
/*

[NodeName("Floor By Outline")]
[NodeCategory(BuiltinNodeCategories.REVIT_DOCUMENT)]
[NodeDescription("WARNING!  Recreated, not modified on change.  Create a floor given a list of curves, a floor type and a level.")]
public class FloorByOutlineLevelAndOffset : RevitTransactionNodeWithOneOutput
{
    public FloorByOutlineLevelAndOffset()
    {
        InPortData.Add(new PortData("curves", "A list of curves representing the edges of the floor.", typeof(Value.List)));
        InPortData.Add(new PortData("floor type", "The floor type to use for floor creation.", typeof(Value.Container)));
        InPortData.Add(new PortData("level", "A level to associate this floor with.", typeof(Value.Container)));

        OutPortData.Add(new PortData("floor", "The floor.", typeof(Value.Container)));

        RegisterAllPorts();

    }

    public override FScheme.Value Evaluate(Microsoft.FSharp.Collections.FSharpList<FScheme.Value> args)
    {
        //if we're in a family document, don't even try to add a floor
        if (dynRevitSettings.Doc.Document.IsFamilyDocument)
        {
            throw new Exception("Floors can not be created in family documents.");
        }

        var edges = ((Value.List)args[0]).Item;
        var floorType = (FloorType)((Value.Container)args[1]).Item;
        var level = (Autodesk.Revit.DB.Level)((Value.Container)args[2]).Item;

        Autodesk.Revit.DB.Floor floor = null;

        //convert the edges to a curveArray
        if (edges.Count() < 3)
        {
            throw new Exception("The edge list provided does not have an adequate number of edges to create a floor.");
        }

        if (this.Elements.Any())
        {
            if (dynUtils.TryGetElement(this.Elements[0], out floor))
            {
                //Delete the existing floor. Revit API does not allow update of floor sketch.
                dynRevitSettings.Doc.Document.Delete(floor.Id);
            }

            floor = CreateFloor(edges, floorType, level);
            this.Elements[0] = floor.Id;
        }
        else
        {
            floor = CreateFloor(edges, floorType, level);
            Elements.Add(floor.Id);
        }

        return Value.NewContainer(floor);
    }

    private static Autodesk.Revit.DB.Floor CreateFloor(IEnumerable<Value> edges, FloorType floorType, Autodesk.Revit.DB.Level level)
    {
        var ca = new CurveArray();
        edges.ToList().ForEach(x => ca.Append((Curve)((Value.Container)x).Item));
        var floor = dynRevitSettings.Doc.Document.Create.NewFloor(ca, floorType, level, false);
        return floor;
    }
}
*/