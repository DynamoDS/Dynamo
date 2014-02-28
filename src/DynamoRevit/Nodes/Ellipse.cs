using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using RevitServices.Persistence;

namespace Dynamo.Nodes
{
    [NodeName("Ellipse")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Creates a geometric ellipse.")]
    public class Ellipse : GeometryBase
    {
        public Ellipse()
        {
            InPortData.Add(new PortData("center", "Center XYZ or Coordinate System", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("radX", "Major Radius", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("radY", "Minor Radius", typeof(FScheme.Value.Number)));
            OutPortData.Add(new PortData("ell", "Ellipse", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        const double RevitPI = 3.14159265358979;

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var ptA = ((FScheme.Value.Container)args[0]).Item;
            var radX = (double)((FScheme.Value.Number)args[1]).Item;
            var radY = (double)((FScheme.Value.Number)args[2]).Item;

            Autodesk.Revit.DB.Ellipse ell = null;

            if (ptA is XYZ)
            {
                ell = DocumentManager.GetInstance().CurrentUIDocument.Application.Application.Create.NewEllipse(
                    //ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * Math.PI
                  (XYZ)ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * RevitPI
               );

            }
            else if (ptA is ReferencePoint)
            {
                ell = DocumentManager.GetInstance().CurrentUIDocument.Application.Application.Create.NewEllipse(
                    //ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * Math.PI
               (XYZ)((ReferencePoint)ptA).Position, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * RevitPI
                );
            }
            else if (ptA is Transform)
            {
                Transform trf = ptA as Transform;
                XYZ center = trf.Origin;
                ell = DocumentManager.GetInstance().CurrentUIDocument.Application.Application.Create.NewEllipse(
                    //ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * Math.PI
                     center, radX, radY, trf.BasisX, trf.BasisY, 0, 2 * RevitPI
                  );
            }

            return FScheme.Value.NewContainer(ell);
        }
    }

    [NodeName("Ellipse Arc")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Creates a geometric elliptical arc. Start and End Values may be between 0 and 2*PI in Radians")]
    public class EllipticalArc : GeometryBase
    {
        public EllipticalArc()
        {
            InPortData.Add(new PortData("center", "Center XYZ or Coordinate System", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("radX", "Major Radius", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("radY", "Minor Radius", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("start", "Start Param", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("end", "End Param", typeof(FScheme.Value.Number)));
            OutPortData.Add(new PortData("ell", "Ellipse", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var ptA = ((FScheme.Value.Container)args[0]).Item;
            var radX = (double)((FScheme.Value.Number)args[1]).Item;
            var radY = (double)((FScheme.Value.Number)args[2]).Item;
            var start = (double)((FScheme.Value.Number)args[3]).Item;
            var end = (double)((FScheme.Value.Number)args[4]).Item;

            Autodesk.Revit.DB.Ellipse ell = null;

            if (ptA is XYZ)
            {
                ell = DocumentManager.GetInstance().CurrentUIDocument.Application.Application.Create.NewEllipse(
                    //ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * Math.PI
                  (XYZ)ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, start, end
               );

            }
            else if (ptA is ReferencePoint)
            {
                ell = DocumentManager.GetInstance().CurrentUIDocument.Application.Application.Create.NewEllipse(
                    //ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * Math.PI
               (XYZ)((ReferencePoint)ptA).Position, radX, radY, XYZ.BasisX, XYZ.BasisY, start, end
                );
            }
            else if (ptA is Transform)
            {
                Transform trf = ptA as Transform;
                XYZ center = trf.Origin;
                ell = DocumentManager.GetInstance().CurrentUIDocument.Application.Application.Create.NewEllipse(
                    //ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * Math.PI
                     center, radX, radY, trf.BasisX, trf.BasisY, start, end
                  );
            }

            return FScheme.Value.NewContainer(ell);
        }
    }
}
