using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Revit;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using RevitServices.Persistence;

namespace Dynamo.Nodes
{
    [NodeName("Arc by Start, Middle, End")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Creates a geometric arc given start, middle and end points in XYZ.")]
    [NodeSearchTags("arc", "circle", "start", "middle", "end", "3 point", "three")]
    public class ArcStartMiddleEnd : GeometryBase
    {
        public ArcStartMiddleEnd()
        {
            InPortData.Add(new PortData("start", "Start XYZ", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("mid", "XYZ on Curve", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("end", "End XYZ", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("arc", "Arc", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {

            Arc a = null;

            var ptA = ((FScheme.Value.Container)args[0]).Item;//start
            var ptB = ((FScheme.Value.Container)args[1]).Item;//middle
            var ptC = ((FScheme.Value.Container)args[2]).Item;//end

            if (ptA is XYZ)
            {

                a = DocumentManager.GetInstance().CurrentUIDocument.Application.Application.Create.NewArc(
                   (XYZ)ptA, (XYZ)ptC, (XYZ)ptB //start, end, middle 
                );


            }
            else if (ptA is ReferencePoint)
            {
                a = DocumentManager.GetInstance().CurrentUIDocument.Application.Application.Create.NewArc(
                   (XYZ)((ReferencePoint)ptA).Position, (XYZ)((ReferencePoint)ptB).Position, (XYZ)((ReferencePoint)ptC).Position //start, end, middle 
                );

            }

            return FScheme.Value.NewContainer(a);
        }
    }

    [NodeName("Arc by Center, Normal, Radius")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Creates a geometric arc given a center point and two end parameters. Start and End Values may be between 0 and 2*PI in Radians")]
    [NodeSearchTags("arc", "circle", "center", "radius")]
    public class ArcCenter : GeometryBase
    {
        public ArcCenter()
        {
            InPortData.Add(new PortData("center", "center XYZ or Coordinate System", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("radius", "Radius", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("start", "Start Param", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("end", "End Param", typeof(FScheme.Value.Number)));
            OutPortData.Add(new PortData("arc", "Arc", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var ptA = ((FScheme.Value.Container)args[0]).Item;
            var radius = (double)((FScheme.Value.Number)args[1]).Item;
            var start = (double)((FScheme.Value.Number)args[2]).Item;
            var end = (double)((FScheme.Value.Number)args[3]).Item;

            Arc a = null;

            if (ptA is XYZ)
            {
                a = DocumentManager.GetInstance().CurrentUIDocument.Application.Application.Create.NewArc(
                   (XYZ)ptA, radius, start, end, XYZ.BasisX, XYZ.BasisY
                );
            }
            else if (ptA is ReferencePoint)
            {
                a = DocumentManager.GetInstance().CurrentUIDocument.Application.Application.Create.NewArc(
                   (XYZ)((ReferencePoint)ptA).Position, radius, start, end, XYZ.BasisX, XYZ.BasisY
                );
            }
            else if (ptA is Transform)
            {
                Transform trf = ptA as Transform;
                XYZ center = trf.Origin;
                a = DocumentManager.GetInstance().CurrentUIDocument.Application.Application.Create.NewArc(
                             center, radius, start, end, trf.BasisX, trf.BasisY
                );
            }

            return FScheme.Value.NewContainer(a);
        }
    }

    [NodeName("Best Fit Arc")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_FIT)]
    [NodeDescription("Creates best fit arc through points")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class BestFitArc : RevitTransactionNodeWithOneOutput
    {
        public BestFitArc()
        {
            InPortData.Add(new PortData("points", "Points to Fit Arc Through", typeof(FScheme.Value.List)));
            OutPortData.Add(new PortData("arc", "Best Fit Arc", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            List<XYZ> xyzList = new List<XYZ>();

            FSharpList<FScheme.Value> vals = ((FScheme.Value.List)args[0]).Item;
            var doc = DocumentManager.GetInstance().CurrentUIDocument;

            for (int ii = 0; ii < vals.Count(); ii++)
            {
                var item = ((FScheme.Value.Container)vals[ii]).Item;

                if (item is ReferencePoint)
                {
                    ReferencePoint refPoint = (ReferencePoint)item;
                    XYZ thisXYZ = refPoint.GetCoordinateSystem().Origin;
                    xyzList.Add(thisXYZ);
                }
                else if (item is XYZ)
                {
                    XYZ thisXYZ = (XYZ)item;
                    xyzList.Add(thisXYZ);
                }
            }

            if (xyzList.Count <= 1)
            {
                throw new Exception("Not enough reference points to make a curve.");
            }


            Type ArcType = typeof(Autodesk.Revit.DB.Arc);

            MethodInfo[] arcStaticMethods = ArcType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            System.String nameOfMethodCreateByFit = "CreateByFit";
            Arc result = null;

            foreach (MethodInfo m in arcStaticMethods)
            {
                if (m.Name == nameOfMethodCreateByFit)
                {
                    object[] argsM = new object[1];
                    argsM[0] = xyzList;

                    result = (Arc)m.Invoke(null, argsM);

                    break;
                }
            }

            return FScheme.Value.NewContainer(result);
        }
    }
}
