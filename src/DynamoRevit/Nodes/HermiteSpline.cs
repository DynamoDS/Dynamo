using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.FSchemeInterop;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using RevitServices.Persistence;

namespace Dynamo.Nodes
{
    [NodeName("Hermite Spline")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Creates a geometric hermite spline.")]
    [NodeSearchTags("curve through points", "interpolate", "spline")]
    public class HermiteSpline : GeometryBase
    {
        Autodesk.Revit.DB.HermiteSpline hs;

        public HermiteSpline()
        {
            InPortData.Add(new PortData("xyzs", "List of pts.(List XYZ)", typeof(FScheme.Value.List)));
            OutPortData.Add(new PortData("spline", "Spline", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var pts = ((FScheme.Value.List)args[0]).Item;

            hs = null;

            var containers = Utils.SequenceToFSharpList(pts);

            var ctrlPts = new List<XYZ>();
            foreach (FScheme.Value e in containers)
            {
                if (e.IsContainer)
                {
                    XYZ pt = (XYZ)((FScheme.Value.Container)(e)).Item;
                    ctrlPts.Add(pt);
                }
            }
            if (pts.Count() > 0)
            {
                var document = DocumentManager.Instance.CurrentUIDocument.Document;
                hs = document.Application.Create.NewHermiteSpline(ctrlPts, false);
            }

            return FScheme.Value.NewContainer(hs);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "NurbsCurve.ByPoints", "NurbsCurve.ByPoints@Point[]");
        }
    }
}
