using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Revit;
using MathNet.Numerics.LinearAlgebra.Generic;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using MathNet.Numerics.LinearAlgebra.Double;
using Dynamo.Utilities;

namespace Dynamo.Nodes
{
    [NodeName("Best Fit Line")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_FIT)]
    [NodeDescription("Determine the best fit line for a set of points.  This line minimizes the sum of the distances between the line and the point set.")]
    internal class BestFitLine : NodeModel
    {
        private readonly PortData _axisPort = new PortData(
            "axis", "A normalized vector representing the axis of the best fit line.",
            typeof(Value.Container));

        private readonly PortData _avgPort = new PortData(
            "avg", "The average (mean) of the point list.", typeof(Value.Container));

        public BestFitLine()
        {
            InPortData.Add(new PortData("XYZs", "A List of XYZ's.", typeof (Value.List)));
            OutPortData.Add(_axisPort);
            OutPortData.Add(_avgPort);

            ArgumentLacing = LacingStrategy.Longest;
            RegisterAllPorts();
        }

        public static List<T> AsGenericList<T>(FSharpList<Value> list)
        {
            return list.Cast<Value.Container>().Select(x => x.Item).Cast<T>().ToList();
        }

        public static XYZ MeanXYZ( List<XYZ> pts )
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
            var covarMat = (1/((double) pts.Count - 1))*ctrdMat*ctrdMat.Transpose();

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
        
        public override void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            var pts = ((Value.List)args[0]).Item;

            var ptList = AsGenericList<XYZ>(pts);
            XYZ meanPt;
            List<XYZ> orderedEigenvectors;
            PrincipalComponentsAnalysis(ptList, out meanPt, out orderedEigenvectors );

            outPuts[_axisPort] = Value.NewContainer(orderedEigenvectors[0]);
            outPuts[_avgPort] = Value.NewContainer(meanPt);
        }
    }

    [NodeName("Best Fit Plane")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SURFACE_CREATE)]
    [NodeDescription("Determine the best fit plane for a set of points.  This line minimizes the sum of the distances between the line and the point set.")]
    internal class BestFitPlane : NodeModel
    {
        private readonly PortData _normalPort = new PortData(
            "normal", "A normalized vector representing the axis of the best fit line.",
            typeof(Value.Container));

        private readonly PortData _originPort = new PortData(
            "origin", "The average (mean) of the point list.", typeof(Value.Container));

        private readonly PortData _planePort = new PortData(
    "plane", "The plane representing the output.", typeof(Value.Container));

        public BestFitPlane()
        {
            InPortData.Add(new PortData("XYZs", "A List of XYZ's.", typeof(Value.List)));
            OutPortData.Add(_planePort);
            OutPortData.Add(_normalPort);
            OutPortData.Add(_originPort);

            ArgumentLacing = LacingStrategy.Longest;
            RegisterAllPorts();
        }

        public override void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            var pts = ((Value.List)args[0]).Item;

            if (pts.Length < 3)
                throw new Exception("3 or more XYZs are necessary to form the best fit plane.");

            var ptList = BestFitLine.AsGenericList<XYZ>(pts);
            XYZ meanPt;
            List<XYZ> orderedEigenvectors;
            BestFitLine.PrincipalComponentsAnalysis(ptList, out meanPt, out orderedEigenvectors );

            var normal = orderedEigenvectors[0].CrossProduct(orderedEigenvectors[1]);

            var plane = dynRevitSettings.Doc.Application.Application.Create.NewPlane(normal, meanPt);

            // take first 3 pts to form simplified normal 
            var bma = ptList[1] - ptList[0];
            var cma = ptList[2] - ptList[0];

            var simplifiedNorm = bma.CrossProduct(cma);

            // find sign of normal that maximizes the dot product pca normal and simplfied normal 
            var dotProd = simplifiedNorm.DotProduct(normal);

            if (dotProd < 0)
            {
                normal = -1*normal;
            }

            outPuts[_planePort] = Value.NewContainer(plane);
            outPuts[_normalPort] = Value.NewContainer(normal);
            outPuts[_originPort] = Value.NewContainer(meanPt);
            
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
            InPortData.Add(new PortData("points", "Points to Fit Arc Through", typeof(Value.List)));
            OutPortData.Add(new PortData("arc", "Best Fit Arc", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            List<XYZ> xyzList = new List<XYZ>();

            FSharpList<Value> vals = ((Value.List)args[0]).Item;
            var doc = dynRevitSettings.Doc;

            for (int ii = 0; ii < vals.Count(); ii++)
            {
                var item = ((Value.Container)vals[ii]).Item;

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

            return Value.NewContainer(result);
        }
    }

    [NodeName("Approximate By Tangent Arcs")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_FIT)]
    [NodeDescription("Appoximates curve by sequence of tangent arcs.")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class ApproximateByTangentArcs : RevitTransactionNodeWithOneOutput
    {
        public ApproximateByTangentArcs()
        {
            InPortData.Add(new PortData("curve", "Curve to Approximate by Tangent Arcs", typeof(Value.Container)));
            OutPortData.Add(new PortData("arcs", "List of Approximating Arcs", typeof(Value.List)));

            RegisterAllPorts();
        }


        public override Value Evaluate(FSharpList<Value> args)
        {
            Curve thisCurve = (Curve)((Value.Container)args[0]).Item;

            if (thisCurve == null)
            {
                throw new Exception("Not enough reference points to make a curve.");
            }


            Type CurveType = typeof(Autodesk.Revit.DB.Curve);

            MethodInfo[] curveInstanceMethods = CurveType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            System.String nameOfMethodApproximateByTangentArcs = "ApproximateByTangentArcs";
            List<Curve> resultArcs = null;
            var result = FSharpList<Value>.Empty;

            foreach (MethodInfo m in curveInstanceMethods)
            {
                if (m.Name == nameOfMethodApproximateByTangentArcs)
                {
                    object[] argsM = new object[0];

                    resultArcs = (List<Curve>)m.Invoke(thisCurve, argsM);

                    break;
                }
            }
            for (int indexCurve = resultArcs.Count - 1; indexCurve > -1; indexCurve--)
            {
                result = FSharpList<Value>.Cons(Value.NewContainer(resultArcs[indexCurve]), result);
            }

            return Value.NewList(result);
        }
    }

}
