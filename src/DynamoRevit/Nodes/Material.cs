using System;
using System.Linq;
using Dynamo.Models;
using Dynamo.Utilities;
using Autodesk.Revit.DB;
using RevitServices.Persistence;

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

            var document = DocumentManager.GetInstance().CurrentUIDocument.Document;
            var fec = new FilteredElementCollector(document);

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

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSRevitNodes.dll",
                "Material.ByName", "Material.ByName@string");
        }
    }
}
