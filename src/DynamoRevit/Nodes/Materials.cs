using System;
using System.Linq;
using Dynamo.Models;
using Dynamo.Utilities;
using Autodesk.Revit.DB;

namespace Dynamo.Nodes
{
    [NodeName("Get Material by Name")]
    [NodeCategory(BuiltinNodeCategories.REVIT_DOCUMENT)]
    [NodeDescription("Get a material from the active Revit document by name.")]
    public class Material : NodeWithOneOutput
    {
        public Material()
        {
            InPortData.Add(new PortData("name", "Material name.", typeof(FScheme.Value.String)));
            OutPortData.Add(new PortData("id", "The Element Id of the selected material.", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(Microsoft.FSharp.Collections.FSharpList<FScheme.Value> args)
        {
            var matName = ((FScheme.Value.String) args[0]).Item;

            var fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);

            Autodesk.Revit.DB.Material foundMat;

            fec.OfClass(typeof (Autodesk.Revit.DB.Material));
            try
            {
                foundMat = (Autodesk.Revit.DB.Material)fec.ToElements().First(x => x.Name == matName);
            }
            catch (InvalidOperationException)
            {
                throw new Exception("A material with that name could not be found.");
            }

            return FScheme.Value.NewContainer(foundMat.Id);
        }
    }
}
