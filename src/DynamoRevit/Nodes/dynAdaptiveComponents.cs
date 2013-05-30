using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Xml;

using Autodesk.Revit.DB;

using Dynamo.Connectors;
using Dynamo.FSchemeInterop;
using Dynamo.Utilities;

using Microsoft.FSharp.Collections;

using Value = Dynamo.FScheme.Value;
using Dynamo.Revit;

namespace Dynamo.Nodes
{
    [NodeName("Adaptive Component by Points")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILYCREATION)]
    [NodeDescription("Given a list of XYZs and a family type, creates an adaptive component at that location.")]
    public class dynAdaptiveComponentByPoints : dynRevitTransactionNodeWithOneOutput
    {
        public dynAdaptiveComponentByPoints()
        {
            InPortData.Add(new PortData("xyzs", "The XYZs that define the locations of your adaptive points.", typeof(Value.Container)));
            InPortData.Add(new PortData("fs", "The family type to create the adaptive component.", typeof(Value.Container)));
            OutPortData.Add(new PortData("ac", "The adaptive component.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            FSharpList<Value> pts = ((Value.List)args[0]).Item;
            FamilySymbol fs = (FamilySymbol)((Value.Container)args[1]).Item;

            FamilyInstance ac = null;

            //if the adapative component already exists, then move the points
            if (Elements.Any())
            {
                //mutate
                Element e;
                //...we attempt to fetch it from the document...
                if (dynUtils.TryGetElement(this.Elements[0],typeof(FamilyInstance), out e))
                {
                    ac = e as FamilyInstance;
                    ac.Symbol = fs;
                }
            }
            else
            {
                //create
                ac = AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(dynRevitSettings.Doc.Document, fs);
                Elements.Add(ac.Id);
            }

            if (ac == null)
                throw new Exception("An adaptive component could not be found or created.");

            IList<ElementId> placePointIds = new List<ElementId>();
            placePointIds = AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(ac);

            if (placePointIds.Count() != pts.Count())
                throw new Exception("The input list of points does not have the same number of values required by the adaptive component.");

            // Set the position of each placement point
            int i = 0;
            foreach (ElementId id in placePointIds)
            {
                ReferencePoint point = dynRevitSettings.Doc.Document.GetElement(id) as ReferencePoint;
                XYZ pt = (XYZ)((Value.Container)pts.ElementAt(i)).Item;
                point.Position = pt;
                i++;
            }

            return Value.NewContainer(ac);
        }

    }
}
