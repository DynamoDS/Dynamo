using System;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Revit;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;

namespace Dynamo.Nodes
{
    [NodeName("Divided Surface")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SURFACE_DIVIDE)]
    [NodeDescription("An element which divides surfaces into patterns and faces")]
    public class DividedSurface : RevitTransactionNodeWithOneOutput
    {
        public DividedSurface()
        {
            InPortData.Add(new PortData("face", "The face to divide.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("U", "Spacing on face in U direction.", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("V", "Spacing on face in U direction", typeof(FScheme.Value.Number)));

            OutPortData.Add(new PortData("ds ", "the divided surface element", typeof(FScheme.Value.Container)));

            RegisterAllPorts();

        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var input = args[0];
            var uDiv = ((FScheme.Value.Number)args[1]).Item;
            var vDiv = ((FScheme.Value.Number)args[2]).Item;

            if (uDiv < 0 || vDiv < 0)
                throw new Exception("Can not create subdivided surface with a negative number of U or V divisions.");

            Autodesk.Revit.DB.DividedSurface divSurf;

            var face = (Face)((FScheme.Value.Container)input).Item;

            FSharpList<FScheme.Value> result = FSharpList<FScheme.Value>.Empty;

            //If we've made any elements previously...
            if (this.Elements.Any())
            {
                //...try to get the first one...
                if (!dynUtils.TryGetElement(this.Elements[0], out divSurf))
                {
                    //...otherwise, just make a new one and replace it in the list.
                    divSurf = this.UIDocument.Document.FamilyCreate.NewDividedSurface(face.Reference);
                    this.Elements[0] = divSurf.Id;
                }
            }
            //...otherwise...
            else
            {
                //...just make a divided curve and store it.
                divSurf = this.UIDocument.Document.FamilyCreate.NewDividedSurface(face.Reference);
                this.Elements.Add(divSurf.Id);
            }

            //update the spacing rule
            if (divSurf != null)
            {
                if (divSurf.USpacingRule.Number != (int)uDiv)
                    divSurf.USpacingRule.Number = (int)uDiv;
                if (divSurf.VSpacingRule.Number != (int)vDiv)
                    divSurf.VSpacingRule.Number = (int)vDiv;
            }

            //Fin
            return FScheme.Value.NewContainer(divSurf);
        }
    }
}
