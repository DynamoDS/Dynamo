using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Dynamo.Models;
using Dynamo.Revit;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
    [NodeName("Topography From Points")]
    [NodeSearchTags("topography","points")]
    class TopographyFromPoints:RevitTransactionNodeWithOneOutput
    {
        public TopographyFromPoints()
        {
            InPortData.Add(new PortData("points", "A list of points from which to create a topography.", typeof(FScheme.Value.List)));
            OutPortData.Add(new PortData("topography", "The topography surface.", typeof(FScheme.Value.Container)));
            RegisterAllPorts();
            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if(!args[0].IsList)
                throw new Exception("The input is not a list of points.");

            var newPts = ((Value.List)args[0]).Item.Select(x => (XYZ)((Value.Container)x).Item).ToList();

            if(!newPts.Any())
                throw new Exception("There are no points in the list.");

            TopographySurface topo = null;

            if (Elements.Any())
            {
                if (dynUtils.TryGetElement(Elements[0], out topo))
                {
                    //In Revit 2014, a transaction edit scope is required to edit a topo surface
                    //we can not currently handle this as it generates a transaction group
                    //so just delete the existing surface and remake.
                    DeleteElement(Elements[0]);

                    topo = CreateTopographySurface(newPts);
                    Elements[0] = topo.Id;
                }
            }
            else
            {
                topo = CreateTopographySurface(newPts);
                Elements.Add(topo.Id);
            }

            if(topo == null)
                throw new ArgumentException("A topography surface could not be created.");

            return Value.NewContainer(topo);
        }

        private TopographySurface CreateTopographySurface(List<XYZ> points)
        {
            return TopographySurface.Create(dynRevitSettings.Doc.Document, points);
        }

    }
}
