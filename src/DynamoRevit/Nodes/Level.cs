using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Revit;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using RevitServices.Persistence;

namespace Dynamo.Nodes
{
    [NodeName("Level")]
    [NodeCategory(BuiltinNodeCategories.REVIT_DATUMS)]
    [NodeDescription("Creates a level datum")]
    public class Level : RevitTransactionNodeWithOneOutput
    {
        public Level()
        {
            InPortData.Add(new PortData("elevation", "The elevation of the level.", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("name", "The name of the level.", typeof(FScheme.Value.String)));

            OutPortData.Add(new PortData("level", "The level.", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            //Level elements take in one double for the z elevation (height)f
            var h = (double)((FScheme.Value.Number)args[0]).Item;
            var name = ((FScheme.Value.String)args[1]).Item;

            Autodesk.Revit.DB.Level lev;

            if (this.Elements.Any())
            {
                if (dynUtils.TryGetElement(this.Elements[0], out lev))
                {
                    lev.Elevation = h;
                    lev.Name = name;
                }
                else
                {
                    lev = this.UIDocument.Document.IsFamilyDocument
                        ? this.UIDocument.Document.FamilyCreate.NewLevel(h)
                        : this.UIDocument.Document.Create.NewLevel(h);
                    lev.Name = name;
                    this.Elements[0] = lev.Id;
                }
            }
            else
            {
                lev = this.UIDocument.Document.IsFamilyDocument
                    ? this.UIDocument.Document.FamilyCreate.NewLevel(h)
                    : this.UIDocument.Document.Create.NewLevel(h);
                lev.Name = name;
                this.Elements.Add(lev.Id);
            }

            return FScheme.Value.NewContainer(lev);
        }
    }

    [NodeName("Select Level From List")]
    [NodeCategory(BuiltinNodeCategories.REVIT_DATUMS)]
    [NodeDescription("Select a level in the active document")]
    public class SelectLevel : DropDrownBase
    {
        public SelectLevel()
        {
            OutPortData.Add(new PortData("level", "The level.", typeof(FScheme.Value.Container)));

            RegisterAllPorts();

            PopulateItems();
        }

        public override void PopulateItems()
        {
            Items.Clear();

            //find all levels in the project
            var levelColl = new FilteredElementCollector(DocumentManager.GetInstance().CurrentUIDocument.Document);
            levelColl.OfClass(typeof(Autodesk.Revit.DB.Level));

            levelColl.ToElements().ToList().ForEach(x => Items.Add(new DynamoDropDownItem(x.Name, x)));
        }
    }
}
