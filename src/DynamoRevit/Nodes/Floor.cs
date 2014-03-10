using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Revit;
using Dynamo.Utilities;
using Value = Dynamo.FScheme.Value;
using RevitServices.Persistence;

namespace Dynamo.Nodes
{
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
            if (DocumentManager.Instance.CurrentUIDocument.Document.IsFamilyDocument)
            {
                throw new Exception("Floors can not be created in family documents.");
            }

            var edges = ((Value.List) args[0]).Item;
            var floorType = (FloorType) ((Value.Container) args[1]).Item;
            var level = (Autodesk.Revit.DB.Level)((Value.Container) args[2]).Item;

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
                    DocumentManager.Instance.CurrentUIDocument.Document.Delete(floor.Id);
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
            edges.ToList().ForEach(x => ca.Append((Curve) ((Value.Container) x).Item));
            var floor = DocumentManager.Instance.CurrentUIDocument.Document.Create.NewFloor(ca, floorType, level, false);
            return floor;
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "Floor.ByOutlineTypeAndLevel", "Floor.ByOutlineTypeAndLevel@Curve[],FloorType,Level");
        }
    }

    [NodeName("Select Floor Type")]
    [NodeCategory(BuiltinNodeCategories.REVIT_DOCUMENT)]
    [NodeDescription("Select a floor type.")]
    public class SelectFloorType : DropDrownBase
    {
        public SelectFloorType()
        {
            OutPortData.Add(new PortData("floor type", "The selected floor type.", typeof(Value.Container)));

            RegisterAllPorts();

            PopulateItems();
        }

        public override void PopulateItems()
        {
            var floorTypesColl = new FilteredElementCollector(DocumentManager.Instance.CurrentUIDocument.Document);
            floorTypesColl.OfClass(typeof (FloorType));

            Items.Clear();

            floorTypesColl.ToElements().ToList().ForEach(x=>Items.Add(new DynamoDropDownItem(x.Name, x)));

            Items = Items.OrderBy(x => x.Name).ToObservableCollection<DynamoDropDownItem>();
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(MigrationManager.CloneAndChangeName(
                data.MigratedNodes.ElementAt(0), "DSRevitNodesUI.FloorType", "Floor Type"));

            return migrationData;
        }
    }
}
