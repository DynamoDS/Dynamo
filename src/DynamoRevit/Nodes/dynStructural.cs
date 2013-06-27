using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Dynamo.Connectors;
using Dynamo.Revit;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
    [NodeName("Select Structural Framing Type")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILYCREATION)]
    [NodeDescription("Create structural framing.")]
    public class dynStructuralFramingSelector: dynDropDrownBase
    {
        public dynStructuralFramingSelector()
        {
            OutPortData.Add(new PortData("type", "The selected structural framing type.", typeof(Value.Container)));

            RegisterAllPorts();

            PopulateItems();
        }

        public override void PopulateItems()
        {
            base.PopulateItems();

            Items.Clear();

            //find all the structural framing family types in the project
            var collector = new FilteredElementCollector(dynRevitSettings.Doc.Document);

            var catFilter = new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming);
            collector.OfClass(typeof(FamilySymbol)).WherePasses(catFilter);

            foreach (var e in collector.ToElements())
                Items.Add(new DynamoDropDownItem(e.Name, e));
        }

    }

    [NodeName("Structural Framing")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILYCREATION)]
    [NodeDescription("Create structural framing.")]
    public class dynStructuralFraming : dynRevitTransactionNodeWithOneOutput
    {
        public dynStructuralFraming()
        {
            InPortData.Add(new PortData("type", "The framing type.", typeof(Value.Container)));
            InPortData.Add(new PortData("curves", "The curve(s) to be used as center lines for your framing elements.", typeof(Value.Container)));
            InPortData.Add(new PortData("gamma", "An angle which represents the desired cross section rotation of the elements.", typeof(Value.Container)));
            OutPortData.Add(new PortData("framing", "The structural framing instance(s) created by this operation.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var symbol = (FamilySymbol)((Value.Container)args[0]).Item;
            var curve = (Curve) ((Value.Container) args[1]).Item;
            var gamma = ((Value.Number) args[2]).Item;

            FamilyInstance instance = null;
            if (this.Elements.Any())
            {
                Element e;
                if (dynUtils.TryGetElement(this.Elements[0], typeof(FamilyInstance), out e))
                {
                    instance = e as FamilyInstance;

                    //update the curve
                    var locCurve = instance.Location as LocationCurve;
                    locCurve.Curve = curve;

                    //TODO:update the gamma

                }
                else
                {
                    instance = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(curve, symbol,
                                                                                  dynRevitSettings.DefaultLevel,
                                                                                  StructuralType.Beam);
                    this.Elements[0] = instance.Id;
                }
            }
            else
            {
                instance = dynRevitSettings.Doc.Document.Create.NewFamilyInstance(curve, symbol,
                                                                                  dynRevitSettings.DefaultLevel,
                                                                                  StructuralType.Beam);
                this.Elements.Add(instance.Id);
            }

            return Value.NewContainer(instance);
        }
        
    }
}
