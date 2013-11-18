using Dynamo.Models;
using Microsoft.FSharp.Collections;
using Autodesk.Revit.DB;

namespace Dynamo.Nodes
{
    [NodeName("UV")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SURFACE_UV)]
    [NodeDescription("Creates a UV from two double values.")]
    public class Uv : GeometryBase
    {
        public Uv()
        {
            InPortData.Add(new PortData("U", "U", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("V", "V", typeof(FScheme.Value.Number)));
            OutPortData.Add(new PortData("uv", "UV", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            double u, v;
            u = ((FScheme.Value.Number)args[0]).Item;
            v = ((FScheme.Value.Number)args[1]).Item;

            return FScheme.Value.NewContainer(new UV(u, v));
        }
    }

    [NodeName("UV Domain")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SURFACE_UV)]
    [NodeDescription("Create a two dimensional domain specifying the Minimum and Maximum UVs.")]
    public class Domain2D : NodeWithOneOutput
    {
        public Domain2D()
        {
            InPortData.Add(new PortData("min", "The minimum UV of the domain.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("max", "The maximum UV of the domain.", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("domain", "A domain.", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var min = (UV)((FScheme.Value.Container)args[0]).Item;
            var max = (UV)((FScheme.Value.Container)args[1]).Item;

            var vmax = Autodesk.LibG.Vector.by_coordinates(max.U, max.V);
            var vmin = Autodesk.LibG.Vector.by_coordinates(min.U, min.V);

            return FScheme.Value.NewContainer(DSCoreNodes.Domain2D.ByMinimumAndMaximum(vmin, vmax));
        }
    }

    [NodeName("UV Grid")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SURFACE_UV)]
    [NodeDescription("Creates a grid of UVs from a domain.")]
    [NodeSearchTags("points", "array", "collection", "field", "uv")]
    public class UvGrid : NodeWithOneOutput
    {
        public UvGrid()
        {
            InPortData.Add(new PortData("domain", "A two dimensional domain.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("U-count", "Number in the U direction.", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("V-count", "Number in the V direction.", typeof(FScheme.Value.Number)));
            OutPortData.Add(new PortData("UVs", "List of UVs in the grid", typeof(FScheme.Value.List)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var domain = (DSCoreNodes.Domain2D)((FScheme.Value.Container)args[0]).Item;
            double ui = ((FScheme.Value.Number)args[1]).Item;
            double vi = ((FScheme.Value.Number)args[2]).Item;
            double us = domain.USpan / ui;
            double vs = domain.VSpan / vi;

            FSharpList<FScheme.Value> result = FSharpList<FScheme.Value>.Empty;

            for (int i = 0; i <= ui; i++)
            {
                double u = domain.Min.x() + i * us;

                for (int j = 0; j <= vi; j++)
                {
                    double v = domain.Min.y() + j * vs;

                    result = FSharpList<FScheme.Value>.Cons(
                        FScheme.Value.NewContainer(new UV(u, v)),
                        result
                    );
                }
            }

            return FScheme.Value.NewList(
               ListModule.Reverse(result)
            );
        }
    }

    [NodeName("UV Random")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SURFACE_UV)]
    [NodeDescription("Creates a random collection of UVs from a domain.")]
    [NodeSearchTags("field")]
    public class UvRandom : NodeWithOneOutput
    {
        public UvRandom()
        {
            InPortData.Add(new PortData("dom", "A domain.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("U-count", "Number in the U direction.", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("V-count", "Number in the V direction.", typeof(FScheme.Value.Number)));
            OutPortData.Add(new PortData("UVs", "List of UVs in the grid", typeof(FScheme.Value.List)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            double ui, vi;

            var domain = (DSCoreNodes.Domain2D)((FScheme.Value.Container)args[0]).Item;
            ui = ((FScheme.Value.Number)args[1]).Item;
            vi = ((FScheme.Value.Number)args[2]).Item;

            FSharpList<FScheme.Value> result = FSharpList<FScheme.Value>.Empty;

            //UV min = ((Value.Container)domain[0]).Item as UV;
            //UV max = ((Value.Container)domain[1]).Item as UV;

            var min = new UV(domain.Min.x(), domain.Min.y());
            var max = new UV(domain.Max.x(), domain.Max.y());

            var r = new System.Random();
            double uSpan = max.U - min.U;
            double vSpan = max.V - min.V;

            for (int i = 0; i < ui; i++)
            {
                for (int j = 0; j < vi; j++)
                {
                    result = FSharpList<FScheme.Value>.Cons(
                        FScheme.Value.NewContainer(new UV(min.U + r.NextDouble() * uSpan, min.V + r.NextDouble() * vSpan)),
                        result
                    );
                }
            }

            return FScheme.Value.NewList(
               ListModule.Reverse(result)
            );
        }
    }
}
