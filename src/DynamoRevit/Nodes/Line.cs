using System;
using System.Reflection;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Revit;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;

namespace Dynamo.Nodes
{
    [NodeName("Line by Endpoints")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Creates a geometric line.")]
    [NodeSearchTags("curve", "two point", "line")]
    public class LineBound : GeometryBase
    {
        public LineBound()
        {
            InPortData.Add(new PortData("start", "Start XYZ", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("end", "End XYZ", typeof(FScheme.Value.Container)));
            //InPortData.Add(new PortData("bound?", "Boolean: Is this line bounded?", typeof(bool)));

            OutPortData.Add(new PortData("line", "Line", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var ptA = ((FScheme.Value.Container)args[0]).Item;
            var ptB = ((FScheme.Value.Container)args[1]).Item;

            Line line = null;

            if (ptA is XYZ)
            {

                line = dynRevitSettings.Doc.Application.Application.Create.NewLineBound(
                  (XYZ)ptA, (XYZ)ptB
                  );


            }
            else if (ptA is ReferencePoint)
            {
                line = dynRevitSettings.Doc.Application.Application.Create.NewLineBound(
                  (XYZ)((ReferencePoint)ptA).Position, (XYZ)((ReferencePoint)ptB).Position
               );

            }

            return FScheme.Value.NewContainer(line);
        }
    }

    [NodeName("Line by Origin and Direction")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Creates a line in the direction of an XYZ normal.")]
    public class LineVectorfromXyz : NodeWithOneOutput
    {
        public LineVectorfromXyz()
        {
            InPortData.Add(new PortData("normal", "Normal Point (XYZ)", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("origin", "Origin Point (XYZ)", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("curve", "Curve", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var ptA = (XYZ)((FScheme.Value.Container)args[0]).Item;
            var ptB = (XYZ)((FScheme.Value.Container)args[1]).Item;

            // CurveElement c = MakeLine(this.UIDocument.Document, ptA, ptB);
            CurveElement c = MakeLineCBP(dynRevitSettings.Doc.Document, ptA, ptB);

            return FScheme.Value.NewContainer(c);
        }


        public Autodesk.Revit.DB.ModelCurve MakeLine(Document doc, XYZ ptA, XYZ ptB)
        {
            Autodesk.Revit.ApplicationServices.Application app = doc.Application;
            // Create plane by the points
            Line line = app.Create.NewLine(ptA, ptB, true);
            XYZ norm = ptA.CrossProduct(ptB);
            double length = norm.GetLength();
            if (length == 0) norm = XYZ.BasisZ;
            Autodesk.Revit.DB.Plane plane = app.Create.NewPlane(norm, ptB);
            Autodesk.Revit.DB.SketchPlane skplane = doc.FamilyCreate.NewSketchPlane(plane);
            // Create line here
            Autodesk.Revit.DB.ModelCurve modelcurve = doc.FamilyCreate.NewModelCurve(line, skplane);
            return modelcurve;
        }

        public Autodesk.Revit.DB.CurveByPoints MakeLineCBP(Document doc, XYZ ptA, XYZ ptB)
        {
            ReferencePoint sunRP = doc.FamilyCreate.NewReferencePoint(ptA);
            ReferencePoint originRP = doc.FamilyCreate.NewReferencePoint(ptB);
            ReferencePointArray sunRPArray = new ReferencePointArray();
            sunRPArray.Append(sunRP);
            sunRPArray.Append(originRP);
            Autodesk.Revit.DB.CurveByPoints sunPath = doc.FamilyCreate.NewCurveByPoints(sunRPArray);
            return sunPath;
        }
    }

    [NodeName("Bisector Line")]
    [NodeCategory(BuiltinNodeCategories.REVIT_REFERENCE)]
    [NodeDescription("Creates bisector of two lines")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class Bisector : RevitTransactionNodeWithOneOutput
    {
        public Bisector()
        {
            InPortData.Add(new PortData("line1", "First Line", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("line2", "Second Line", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("bisector", "Bisector Line", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }
        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            Line line1 = (Line)((FScheme.Value.Container)args[0]).Item;
            Line line2 = (Line)((FScheme.Value.Container)args[1]).Item;

            Type LineType = typeof(Autodesk.Revit.DB.Line);

            MethodInfo[] lineInstanceMethods = LineType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            System.String nameOfMethodCreateBisector = "CreateBisector";
            Line result = null;

            foreach (MethodInfo m in lineInstanceMethods)
            {
                if (m.Name == nameOfMethodCreateBisector)
                {
                    object[] argsM = new object[1];
                    argsM[0] = line2;

                    result = (Line)m.Invoke(line1, argsM);

                    break;
                }
            }

            return FScheme.Value.NewContainer(result);
        }
    }
}
