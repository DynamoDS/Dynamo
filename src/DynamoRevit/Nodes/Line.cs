using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Revit;
using Dynamo.Utilities;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;
using Microsoft.FSharp.Collections;
using RevitServices.Persistence;

namespace Dynamo.Nodes
{
    [NodeName("Line by Endpoints")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Creates a geometric line.")]
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

                line = DocumentManager.GetInstance().CurrentUIDocument.Application.Application.Create.NewLineBound(
                  (XYZ)ptA, (XYZ)ptB
                  );


            }
            else if (ptA is ReferencePoint)
            {
                line = DocumentManager.GetInstance().CurrentUIDocument.Application.Application.Create.NewLineBound(
                  (XYZ)((ReferencePoint)ptA).Position, (XYZ)((ReferencePoint)ptB).Position
               );

            }

            return FScheme.Value.NewContainer(line);
        }
    }

    [NodeName("Line By Start Point Direction Length")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_CREATE)]
    [NodeDescription("Creates a geometric line from a start point, a direction, and a length.")]
    public class LineByStartPtDirLength : GeometryBase
    {
        public LineByStartPtDirLength()
        {
            InPortData.Add(new PortData("start", "The origin of the line.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("direction", "The direction vector.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("length", "The length of the line.", typeof(FScheme.Value.Container)));

            OutPortData.Add(new PortData("line", "Line", typeof(FScheme.Value.Container)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var ptA = (XYZ)((FScheme.Value.Container)args[0]).Item;
            var vec = (XYZ)((FScheme.Value.Container)args[1]).Item;
            var len = ((FScheme.Value.Number)args[2]).Item;

            if (len == 0)
            {
                throw new Exception("Cannot create a line with zero length.");
            }

            var ptB = ptA + vec.Multiply(len);

            if (ptA.IsAlmostEqualTo(ptB))
            {
                throw new Exception("The start point and end point are extremely close together. The line will be too short.");
            }

            var line = DocumentManager.GetInstance().CurrentUIDocument.Application.Application.Create.NewLineBound(ptA, ptB);

            return FScheme.Value.NewContainer(line);
        }
    }

    [NodeName("Line by Origin Direction")]
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
            CurveElement c = MakeLineCBP(DocumentManager.GetInstance().CurrentUIDocument.Document, ptA, ptB);

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

    [NodeName("Best Fit Line")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_FIT)]
    [NodeDescription("Determine the best fit line for a set of points.  This line minimizes the sum of the distances between the line and the point set.")]
    internal class BestFitLine : NodeModel
    {
        private readonly PortData _axisPort = new PortData(
            "axis", "A normalized vector representing the axis of the best fit line.",
            typeof(FScheme.Value.Container));

        private readonly PortData _avgPort = new PortData(
            "avg", "The average (mean) of the point list.", typeof(FScheme.Value.Container));

        public BestFitLine()
        {
            InPortData.Add(new PortData("XYZs", "A List of XYZ's.", typeof(FScheme.Value.List)));
            OutPortData.Add(_axisPort);
            OutPortData.Add(_avgPort);

            ArgumentLacing = LacingStrategy.Longest;
            RegisterAllPorts();
        }

        public static List<T> AsGenericList<T>(FSharpList<FScheme.Value> list)
        {
            return list.Cast<FScheme.Value.Container>().Select(x => x.Item).Cast<T>().ToList();
        }

        public static XYZ MeanXYZ(List<XYZ> pts)
        {
            return pts.Aggregate(new XYZ(), (i, p) => i.Add(p)).Divide(pts.Count);
        }

        public static XYZ MakeXYZ(Vector<double> vec)
        {
            return new XYZ(vec[0], vec[1], vec[2]);
        }

        public static void PrincipalComponentsAnalysis(List<XYZ> pts, out XYZ meanXYZ, out List<XYZ> orderEigenvectors)
        {
            var meanPt = MeanXYZ(pts);
            meanXYZ = meanPt;

            var l = pts.Count();
            var ctrdMat = DenseMatrix.Create(3, l, (r, c) => pts[c][r] - meanPt[r]);
            var covarMat = (1 / ((double)pts.Count - 1)) * ctrdMat * ctrdMat.Transpose();

            var eigen = covarMat.Evd();

            var valPairs = new List<Tuple<double, Vector<double>>>
                {
                    new Tuple<double, Vector<double>>(eigen.EigenValues()[0].Real, eigen.EigenVectors().Column(0)),
                    new Tuple<double, Vector<double>>(eigen.EigenValues()[1].Real, eigen.EigenVectors().Column(1)),
                    new Tuple<double, Vector<double>>(eigen.EigenValues()[2].Real, eigen.EigenVectors().Column(2))
                };

            var sortEigVecs = valPairs.OrderByDescending((x) => x.Item1).ToList();

            orderEigenvectors = new List<XYZ>
                {
                    MakeXYZ( sortEigVecs[0].Item2 ),
                    MakeXYZ( sortEigVecs[1].Item2 ),
                    MakeXYZ( sortEigVecs[2].Item2 )
                };
        }

        public override void Evaluate(FSharpList<FScheme.Value> args, Dictionary<PortData, FScheme.Value> outPuts)
        {
            var pts = ((FScheme.Value.List)args[0]).Item;

            var ptList = AsGenericList<XYZ>(pts);
            XYZ meanPt;
            List<XYZ> orderedEigenvectors;
            PrincipalComponentsAnalysis(ptList, out meanPt, out orderedEigenvectors);

            outPuts[_axisPort] = FScheme.Value.NewContainer(orderedEigenvectors[0]);
            outPuts[_avgPort] = FScheme.Value.NewContainer(meanPt);
        }
    }

}
