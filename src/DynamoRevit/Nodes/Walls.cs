using System;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Revit;
using Dynamo.Utilities;

namespace Dynamo.Nodes
{
    [NodeName("Wall By Curve")]
    [NodeCategory(BuiltinNodeCategories.REVIT_DOCUMENT)]
    [NodeDescription("Get a material from the active Revit document by name.")]
    public class WallByCurve : RevitTransactionNodeWithOneOutput
    {
        public WallByCurve()
        {
            InPortData.Add(new PortData("curve", "A list of curves representing the edges of the floor.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("level", "A level to associate this wall with.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("wall type", "A level to associate this floor with.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("height", "The floor type to use for floor creation.", typeof(FScheme.Value.Number)));

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
    [NodeCategory(BuiltinNodeCategories.REVIT_DOCUMENT)]
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
}
