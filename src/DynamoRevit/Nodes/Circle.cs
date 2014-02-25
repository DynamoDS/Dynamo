using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using RevitServices.Persistence;

namespace Dynamo.Nodes
{
    [NodeName("Circle")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Creates a geometric circle.")]
    public class Circle : GeometryBase
    {
        public Circle()
        {
            InPortData.Add(new PortData("center", "Start XYZ", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("radius", "Radius", typeof(FScheme.Value.Number)));
            OutPortData.Add(new PortData("circle", "Circle CurveLoop", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public const double RevitPI = 3.14159265358979;

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var ptA = ((FScheme.Value.Container)args[0]).Item;
            var radius = (double)((FScheme.Value.Number)args[1]).Item;

            Curve circle = null;

            if (ptA is XYZ)
            {
                //Curve circle = this.UIDocument.Application.Application.Create.NewArc(ptA, radius, 0, 2 * Math.PI, XYZ.BasisX, XYZ.BasisY);
                //circle = dynRevitSettings.Doc.Application.Application.Create.NewArc((XYZ)ptA, radius, 0, 2 * RevitPI, XYZ.BasisX, XYZ.BasisY);
                circle = DocumentManager.GetInstance().CurrentUIDocument.Application.Application.Create.NewArc((XYZ)ptA, radius, 0, 2 * RevitPI, XYZ.BasisX, XYZ.BasisY);

            }
            else if (ptA is ReferencePoint)
            {
                //Curve circle = this.UIDocument.Application.Application.Create.NewArc(ptA, radius, 0, 2 * Math.PI, XYZ.BasisX, XYZ.BasisY);
                //circle = dynRevitSettings.Doc.Application.Application.Create.NewArc((XYZ)((ReferencePoint)ptA).Position, radius, 0, 2 * RevitPI, XYZ.BasisX, XYZ.BasisY);\
                circle = DocumentManager.GetInstance().CurrentUIDocument.Application.Application.Create.NewArc((XYZ)ptA, radius, 0, 2 * RevitPI, XYZ.BasisX, XYZ.BasisY);
            }

            return FScheme.Value.NewContainer(circle);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Circle.ByCenterPointRadius",
                "Circle.ByCenterPointRadius@Point,double");
        }
    }
}
