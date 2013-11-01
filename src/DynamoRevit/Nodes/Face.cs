using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Microsoft.FSharp.Collections;

namespace Dynamo.Nodes
{
    [NodeName("Faces Intersecting Line")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_INTERSECT)]
    [NodeDescription("Creates list of faces of the solid intersecting given line.")]
    public class FacesByLine : NodeWithOneOutput
    {
        public FacesByLine()
        {
            InPortData.Add(new PortData("solid", "solid to extract faces from", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("line", "line to extract faces from", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("faces of solid along the line", "extracted list of faces", typeof(object)));

            RegisterAllPorts();

        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            Solid thisSolid = (Solid)((FScheme.Value.Container)args[0]).Item;
            Line selectLine = (Line)((FScheme.Value.Container)args[1]).Item;

            FaceArray faceArr = thisSolid.Faces;
            var thisEnum = faceArr.GetEnumerator();

            SortedList<double, Autodesk.Revit.DB.Face> intersectingFaces = new SortedList<double, Autodesk.Revit.DB.Face>();

            for (; thisEnum.MoveNext(); )
            {
                Autodesk.Revit.DB.Face thisFace = (Autodesk.Revit.DB.Face)thisEnum.Current;
                IntersectionResultArray resultArray = null;

                SetComparisonResult resultIntersect = thisFace.Intersect(selectLine, out resultArray);
                if (resultIntersect != SetComparisonResult.Overlap)
                    continue;
                bool first = true;
                double linePar = -1.0;
                foreach (IntersectionResult ir in resultArray)
                {
                    double irPar = ir.Parameter;
                    if (first == true)
                    {
                        linePar = irPar;
                        first = false;
                    }
                    else if (irPar < linePar)
                        linePar = irPar;
                }
                intersectingFaces.Add(linePar, thisFace);
            }

            var result = FSharpList<FScheme.Value>.Empty;

            var intersectingFacesEnum = intersectingFaces.Reverse().GetEnumerator();
            for (; intersectingFacesEnum.MoveNext(); )
            {
                Autodesk.Revit.DB.Face faceObj = intersectingFacesEnum.Current.Value;
                result = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewContainer(faceObj), result);
            }

            return FScheme.Value.NewList(result);
        }
    }

    [NodeName("Face From Points")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SURFACE_CREATE)]
    [NodeDescription("Creates face on grid of points")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class FaceThroughPoints : NodeWithOneOutput
    {

        public FaceThroughPoints()
        {
            InPortData.Add(new PortData("Points", "Points to create face, list or list of lists", typeof(FScheme.Value.List)));
            InPortData.Add(new PortData("NumberOfRows", "Number of rows in the grid of the face", typeof(object)));
            OutPortData.Add(new PortData("face", "Face", typeof(object)));

            RegisterAllPorts();

        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var listIn = ((FScheme.Value.List)args[0]).Item.Select(
                    x => ((XYZ)((FScheme.Value.Container)x).Item)
                       ).ToList();
            /* consider passing n x m grid of points instead of flat list
            var in1 = ((Value.Container)args[0]).Item;
            List<XYZ> listIn = in1 as List<XYZ>;
            List<List<XYZ>> listOfListsIn = (listIn != null) ? null : (in1 as List<List<XYZ>>);

            if (listIn == null && listOfListsIn == null)
                throw new Exception("no XYZ list or list of XYZ lists in Face Through Points node");

            if (listOfListsIn != null)
            {
                listIn = new List<XYZ>();
                for (int indexL = 0; indexL < listOfListsIn.Count; indexL++)
                {
                    listIn.Concat(listOfListsIn[indexL]);
                }
            }
            */

            int numberOfRows = (int)((FScheme.Value.Number)args[1]).Item;
            if (numberOfRows < 2 || listIn.Count % numberOfRows != 0)
                throw new Exception("number of rows should  match number of points Face Through Points node");

            bool periodicU = false;
            bool periodicV = false;

            Type HermiteFaceType = typeof(Autodesk.Revit.DB.HermiteFace);

            MethodInfo[] hermiteFaceStaticMethods = HermiteFaceType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            System.String nameOfMethodCreate = "Create";
            Autodesk.Revit.DB.Face result = null;

            foreach (MethodInfo m in hermiteFaceStaticMethods)
            {
                if (m.Name == nameOfMethodCreate)
                {
                    object[] argsM = new object[7];
                    argsM[0] = numberOfRows;
                    argsM[1] = listIn;
                    argsM[2] = new List<XYZ>();
                    argsM[3] = new List<XYZ>();
                    argsM[4] = new List<XYZ>();
                    argsM[5] = periodicU;
                    argsM[6] = periodicV;

                    result = (Autodesk.Revit.DB.Face)m.Invoke(null, argsM);

                    break;
                }
            }

            return FScheme.Value.NewContainer(result);
        }
    }
}
