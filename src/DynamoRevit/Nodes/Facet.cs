using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Microsoft.FSharp.Collections;

namespace Dynamo.Nodes
{
    /// <summary>
    /// A triangle facet.   
    /// </summary>
    public class Facet
    {
        public List<XYZ> Points { get; set; }

        protected Facet(){}

        public Facet(XYZ a, XYZ b, XYZ c)
        {
            Points = new List<XYZ> {a, b, c};
        }
    }

    [NodeName("Facet")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SURFACE_CREATE)]
    [NodeDescription("Create a facet from three points.")]
    [NodeSearchTags("facet", "triangle")]
    public class FacetByThreePoints : NodeWithOneOutput
    {
        public FacetByThreePoints()
        {
            InPortData.Add(new PortData("points", "XYZ locations of facet corners", typeof(FScheme.Value.List)));
            OutPortData.Add(new PortData("facet", "The facet.", typeof(FScheme.Value.Container)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var pts = ((FScheme.Value.List) args[0]).Item;

            if (pts.Count() != 3)
            {
                throw new Exception("Input points lists must have three points.");
            }

            var a = (XYZ) ((FScheme.Value.Container) pts[0]).Item;
            var b = (XYZ) ((FScheme.Value.Container) pts[1]).Item;
            var c = (XYZ) ((FScheme.Value.Container) pts[2]).Item;

            return FScheme.Value.NewContainer(new Facet(a,b,c));
        }
    }

    [NodeName("Quad")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SURFACE_CREATE)]
    [NodeDescription("Create a quad from four points.")]
    [NodeSearchTags("quad", "polygon")]
    public class QuadByFourPoints : NodeWithOneOutput
    {
        public QuadByFourPoints()
        {
            InPortData.Add(new PortData("points", "XYZ locations of quad corners", typeof(FScheme.Value.List)));
            OutPortData.Add(new PortData("quad", "The quad.", typeof(FScheme.Value.List)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var pts = ((FScheme.Value.List)args[0]).Item;

            if (pts.Count() != 4)
            {
                throw new Exception("Input points lists must have four points.");
            }

            var a = (XYZ)((FScheme.Value.Container)pts[0]).Item;
            var b = (XYZ)((FScheme.Value.Container)pts[1]).Item;
            var c = (XYZ)((FScheme.Value.Container)pts[2]).Item;
            var d = (XYZ)((FScheme.Value.Container)pts[3]).Item;

            var results = FSharpList<FScheme.Value>.Empty;
            results = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewContainer(new Facet(a, b, c)), results);
            results = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewContainer(new Facet(a, c, d)), results);
            return FScheme.Value.NewList(results);
        }
    }
}
