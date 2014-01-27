using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using Dynamo.Revit;

namespace Dynamo.Nodes
{
    [NodeName("Spacing Rule Layout")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_DIVIDE)]
    [NodeDescription("The spacing rule layout to be applied to a divided path.")]
    public class DividedPathSpacingRuleLayout : EnumAsConstants
    {
        public DividedPathSpacingRuleLayout():base(typeof(SpacingRuleLayout)){}
    }

    [NodeName("Divided Path")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_DIVIDE)]
    [NodeDescription("Divides curves or edges and makes a DividedPath.")]
    public class DividedPath : RevitTransactionNodeWithOneOutput
    {
        public DividedPath()
        {
            InPortData.Add(new PortData("curves", "A reference or model curve.", typeof(Value.Container)));//TODO make this a ref, but how to handle tracking persistance
            InPortData.Add(new PortData("number or spacing", "A number of divisions or a length to define a spacing.", typeof(Value.Number))); // just divide equally for now, dont worry about spacing and starting point
            InPortData.Add(new PortData("start", "Starting parameter or length along the curve.", typeof(double)));
            InPortData.Add(new PortData("spacing", "A spacing rule layout.", typeof(double)));

            OutPortData.Add(new PortData("divided path ", "the divided path element", typeof(Value.Container)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];
            var xi = ((Value.Number)args[1]).Item;// Number
            var x0 = ((Value.Number)args[2]).Item;// Starting Coord
            var xs = (SpacingRuleLayout)((Value.Container)args[3]).Item; // Spacing

            Autodesk.Revit.DB.DividedPath divPath;
            var refList = new List<Reference>();

            refList.Clear();

            var c = (CurveElement)((Value.Container)input).Item;

            FSharpList<Value> result = FSharpList<Value>.Empty;

            Curve crvRef = c.GeometryCurve;

            refList.Add(crvRef.Reference);

            if (this.Elements.Any())
            {
                if (dynUtils.TryGetElement(this.Elements[0], out divPath))
                {
                    SetSpacing(divPath, xi, xs);
                }
                else
                {
                    divPath = Autodesk.Revit.DB.DividedPath.Create(this.UIDocument.Document, refList);
                    SetSpacing(divPath, xi, xs);
                    this.Elements[0] = divPath.Id;
                }
            }
            else
            {
                divPath = Autodesk.Revit.DB.DividedPath.Create(this.UIDocument.Document, refList);
                SetSpacing(divPath, xi, xs);
                this.Elements.Add(divPath.Id);
            }
            refList.Clear();

            return Value.NewContainer(divPath);
        }

        private static void SetSpacing(Autodesk.Revit.DB.DividedPath divPath, double xi, SpacingRuleLayout xs)
        {
            divPath.SpacingRuleLayout = xs;

            switch (divPath.SpacingRuleLayout)
            {
                case SpacingRuleLayout.FixedDistance:
                    divPath.Distance = xi;
                    break;
                case SpacingRuleLayout.FixedNumber:
                    divPath.FixedNumberOfPoints = (int) xi;
                    break;
                case SpacingRuleLayout.MaximumSpacing:
                    divPath.MaximumDistance = xi;
                    break;
                case SpacingRuleLayout.MinimumSpacing:
                    divPath.MinimumDistance = xi;
                    break;
            }
        }
    }

    [NodeName("XYZs From Divided Path")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_DIVIDE)]
    [NodeDescription("Get the points along a divided path.")]
    public class PointsOnDividedPath : NodeWithOneOutput
    {
        public PointsOnDividedPath()
        {
            InPortData.Add(new PortData("divided path", "A divided path from which to get the points.", typeof(Value.Container)));
            OutPortData.Add(new PortData("xyzs", "The points along the divided path.", typeof(Value.List)));
            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var path = (Autodesk.Revit.DB.DividedPath) ((Value.Container) args[0]).Item;

            var pts = FSharpList<Value>.Empty;

            var geom = path.get_Geometry(dynRevitSettings.GeometryOptions);

            if (geom == null)
            {
                throw new Exception("There is no up to date point information. Try placing a transaction node before this node on the canvas.");
            }

            foreach (var geob in geom)
            {
                Debug.WriteLine(string.Format("Divided path contains a {0}", geob));
                var pt = (Point) geob;
                pts = FSharpList<Value>.Cons(Value.NewContainer(pt.Coord), pts);
            }

            return Value.NewList(pts);
        }
    }

}
