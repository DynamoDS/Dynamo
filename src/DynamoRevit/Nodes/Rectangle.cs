using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using RevitServices.Persistence;

namespace Dynamo.Nodes
{
    [NodeName("Rectangle")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Create a rectangle by specifying the center, width, height, and normal.  Outputs a CurveLoop object directed counter-clockwise from upper right.")]
    [NodeSearchTags("rectangle", "quad")]
    public class Rectangle : GeometryBase
    {
        public Rectangle()
        {
            InPortData.Add(new PortData("transform", "The a transform for the rectangle.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("width", "The width of the rectangle.", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("height", "The height of the rectangle.", typeof(FScheme.Value.Number)));
            OutPortData.Add(new PortData("geometry", "The curve loop representing the rectangle.", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var t = (Transform)((FScheme.Value.Container)args[0]).Item;
            double width = ((FScheme.Value.Number)args[1]).Item;
            double height = ((FScheme.Value.Number)args[2]).Item;

            //ccw from upper right
            var p0 = new XYZ(width / 2, height / 2, 0);
            var p3 = new XYZ(-width / 2, height / 2, 0);
            var p2 = new XYZ(-width / 2, -height / 2, 0);
            var p1 = new XYZ(width / 2, -height / 2, 0);

            p0 = t.OfPoint(p0);
            p1 = t.OfPoint(p1);
            p2 = t.OfPoint(p2);
            p3 = t.OfPoint(p3);

            var l1 = DocumentManager.GetInstance().CurrentUIDocument.Application.Application.Create.NewLineBound(p0, p1);
            var l2 = DocumentManager.GetInstance().CurrentUIDocument.Application.Application.Create.NewLineBound(p1, p2);
            var l3 = DocumentManager.GetInstance().CurrentUIDocument.Application.Application.Create.NewLineBound(p2, p3);
            var l4 = DocumentManager.GetInstance().CurrentUIDocument.Application.Application.Create.NewLineBound(p3, p0);

            var cl = new Autodesk.Revit.DB.CurveLoop();
            cl.Append(l1);
            cl.Append(l2);
            cl.Append(l3);
            cl.Append(l4);

            return FScheme.Value.NewContainer(cl);
        }
    }
}
