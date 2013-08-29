//Copyright 2013 Ian Keough

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

using Dynamo.Connectors;
using Dynamo.Models;
using MathNet.Numerics.LinearAlgebra.Generic;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using MathNet.Numerics.LinearAlgebra.Double;
using Dynamo.Utilities;

namespace Dynamo.Nodes
{
    [NodeName("Best Fit Line")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Determine the best fit line for a set of points.  This line minimizes the sum of the distances between the line and the point set.")]
    internal class dynBestFitLine : dynNodeModel
    {
        private readonly PortData _axisPort = new PortData(
            "axis", "A normalized vector representing the axis of the best fit line.",
            typeof(Value.Container));

        private readonly PortData _avgPort = new PortData(
            "avg", "The average (mean) of the point list.", typeof(Value.Container));

        public dynBestFitLine()
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
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SURFACE)]
    [NodeDescription("Determine the best fit plane for a set of points.  This line minimizes the sum of the distances between the line and the point set.")]
    internal class dynBestFitPlane : dynNodeModel
    {
        private readonly PortData _normalPort = new PortData(
            "normal", "A normalized vector representing the axis of the best fit line.",
            typeof(Value.Container));

        private readonly PortData _originPort = new PortData(
            "origin", "The average (mean) of the point list.", typeof(Value.Container));

        private readonly PortData _planePort = new PortData(
    "plane", "The plane representing the output.", typeof(Value.Container));

        public dynBestFitPlane()
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

            var ptList = dynBestFitLine.AsGenericList<XYZ>(pts);
            XYZ meanPt;
            List<XYZ> orderedEigenvectors;
            dynBestFitLine.PrincipalComponentsAnalysis(ptList, out meanPt, out orderedEigenvectors );

            var normal = orderedEigenvectors[0].CrossProduct(orderedEigenvectors[1]);

            var plane = dynRevitSettings.Doc.Application.Application.Create.NewPlane(normal, meanPt);

            outPuts[_planePort] = Value.NewContainer(plane);
            outPuts[_normalPort] = Value.NewContainer(normal);
            outPuts[_originPort] = Value.NewContainer(meanPt);
            
        }
    }
}
