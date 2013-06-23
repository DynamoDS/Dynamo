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
    internal class dynBestFitLine : dynNodeWithMultipleOutputs
    {
        public dynBestFitLine()
        {
            InPortData.Add(new PortData("XYZs", "A List of XYZ's.", typeof (Value.List)));
            OutPortData.Add(new PortData("axis", "A normalized vector representing the axis of the best fit line.", typeof (Value.Container)));
            OutPortData.Add(new PortData("avg", "The average (mean) of the point list.", typeof(Value.Container)));
            
            this.ArgumentLacing = LacingStrategy.Disabled;
            RegisterAllPorts();
        }

        public static List<T> AsGenericList<T>(FSharpList<Value> list)
        {
            var f = new List<T>();
            var l = list.Length;
            for (var i = 0; i < l; i++)
            {
                var ctnr = list.Head;
                list = list.TailOrNull;

                f.Add((T)(ctnr as Value.Container).Item);
            }
            return f;
        }

        public static XYZ MeanXYZ( IEnumerable<XYZ> pts )
        {
            return pts.Aggregate(new XYZ(), (i, p) => i.Add(p)).Divide(pts.Count());
        }

        public static XYZ MakeXYZ(Vector<double> vec)
        {
            return new XYZ(vec[0], vec[1], vec[2]);
        }

        public static void PrincipalComponentsAnalysis( List<XYZ> pts, out XYZ meanXYZ,
                                                            out List<XYZ> orderEigenvectors)
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
        
        public override Value Evaluate(FSharpList<Value> args)
        {
            var pts = (args[0] as Value.List).Item;

            var ptList = AsGenericList<XYZ>(pts);
            XYZ meanPt;
            List<XYZ> orderedEigenvectors;
            PrincipalComponentsAnalysis(ptList, out meanPt, out orderedEigenvectors );

            var results = FSharpList<Value>.Empty;
            results = FSharpList<Value>.Cons(Value.NewContainer(meanPt), results);
            results = FSharpList<Value>.Cons(Value.NewContainer( orderedEigenvectors[0] ) , results);
           
            return Value.NewList(results);
        }

    }

    [NodeName("Best Fit Plane")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SURFACE)]
    [NodeDescription("Determine the best fit plane for a set of points.  This line minimizes the sum of the distances between the line and the point set.")]
    internal class dynBestFitPlane : dynNodeWithMultipleOutputs
    {
        public dynBestFitPlane()
        {
            InPortData.Add(new PortData("XYZs", "A List of XYZ's.", typeof(Value.List)));
            OutPortData.Add(new PortData("normal", "A normalized vector representing the axis of the best fit line.", typeof(Value.Container)));
            OutPortData.Add(new PortData("origin", "The average (mean) of the point list.", typeof(Value.Container)));

            this.ArgumentLacing = LacingStrategy.Disabled;
            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var pts = (args[0] as Value.List).Item;

            if (pts.Length < 3)
                throw new Exception("3 or more XYZs are necessary to form the best fit plane.");

            var ptList = dynBestFitLine.AsGenericList<XYZ>(pts);
            XYZ meanPt;
            List<XYZ> orderedEigenvectors;
            dynBestFitLine.PrincipalComponentsAnalysis(ptList, out meanPt, out orderedEigenvectors );

            var normal = orderedEigenvectors[0].CrossProduct(orderedEigenvectors[1]);

            var results = FSharpList<Value>.Empty;
            results = FSharpList<Value>.Cons(Value.NewContainer( meanPt ) , results);
            results = FSharpList<Value>.Cons(Value.NewContainer(normal), results);

            return Value.NewList(results);
        }


    }

}
