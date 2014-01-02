using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using RevitServices.Persistence;
using Line = Autodesk.Revit.DB.Line;
using Solid = Autodesk.Revit.DB.Solid;

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

    [NodeName("Surface Derivatives")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SURFACE_QUERY)]
    [NodeDescription("Returns a derivatives of the face (f) at the parameter (uv).")]
    public class ComputeFaceDerivatives : GeometryBase
    {
        public ComputeFaceDerivatives()
        {
            InPortData.Add(new PortData("f", "The face to evaluate(Face)", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("uv", "The parameter to evaluate(UV)", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("t", "Transform describing the face at the parameter(Transform)", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var faceRef = ((FScheme.Value.Container)args[0]).Item as Reference;
            var uv = (UV)((FScheme.Value.Container)args[1]).Item;

            var t = Transform.Identity;

            Autodesk.Revit.DB.Face f = (faceRef == null) ?
                ((Autodesk.Revit.DB.Face)((FScheme.Value.Container)args[0]).Item) :
                (DocumentManager.GetInstance().CurrentUIDocument.Document.GetElement(faceRef.ElementId).GetGeometryObjectFromReference(faceRef) as Autodesk.Revit.DB.Face);

            if (f != null)
            {
                t = f.ComputeDerivatives(uv);
                t.BasisX = t.BasisX.Normalize();
                t.BasisZ = t.BasisZ.Normalize();
                t.BasisY = t.BasisX.CrossProduct(t.BasisZ);
            }

            return FScheme.Value.NewContainer(t);
        }

    }

    [NodeName("Evaluate Surface")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SURFACE_QUERY)]
    [NodeDescription("Evaluate a parameter(UV) on a face to find the XYZ location.")]
    class XyzEvaluate : GeometryBase
    {
        public XyzEvaluate()
        {
            InPortData.Add(new PortData("uv", "The point to evaluate.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("face", "The face to evaluate.", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("XYZ", "The location.", typeof(FScheme.Value.Container)));
            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            Reference faceRef = (args[1] as FScheme.Value.Container).Item as Reference;
            Autodesk.Revit.DB.Face f = (faceRef == null) ?
                ((args[1] as FScheme.Value.Container).Item as Autodesk.Revit.DB.Face) :
                DocumentManager.GetInstance().CurrentUIDocument.Document.GetElement(faceRef).GetGeometryObjectFromReference(faceRef) as Autodesk.Revit.DB.Face;


            XYZ face_point = null;

            if (f != null)
            {
                //each item in the list will be a reference point
                UV param = (UV)(args[0] as FScheme.Value.Container).Item;
                face_point = f.Evaluate(param);
            }

            return FScheme.Value.NewContainer(face_point);
        }
    }

    [NodeName("Surface Normal")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SURFACE_QUERY)]
    [NodeDescription("Evaluate a point on a face to find the normal.")]
    class NormalEvaluate : GeometryBase
    {
        public NormalEvaluate()
        {
            InPortData.Add(new PortData("uv", "The point to evaluate.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("face", "The face to evaluate.", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("XYZ", "The normal.", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var faceRef = (args[1] as FScheme.Value.Container).Item as Reference;
            Autodesk.Revit.DB.Face f = (faceRef == null) ?
                ((args[1] as FScheme.Value.Container).Item as Autodesk.Revit.DB.Face) :
                DocumentManager.GetInstance().CurrentUIDocument.Document.GetElement(faceRef).GetGeometryObjectFromReference(faceRef) as Autodesk.Revit.DB.Face;

            XYZ norm = null;

            if (f != null)
            {
                //each item in the list will be a reference point
                UV uv = (UV)(args[0] as FScheme.Value.Container).Item;
                norm = f.ComputeNormal(uv);
            }

            return FScheme.Value.NewContainer(norm);
        }
    }

    [NodeName("Surface Area")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_MEASURE)]
    [NodeDescription("An element which measures the surface area of a face (f)")]
    public class SurfaceArea : MeasurementBase
    {
        public SurfaceArea()
        {
            InPortData.Add(new PortData("f", "The face whose surface area you wish to calculate (Reference).", typeof(FScheme.Value.Container)));//Ref to a face of a form
            OutPortData.Add(new PortData("a", "The surface area of the face (Number).", typeof(FScheme.Value.Number)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            double area = 0.0;

            object arg0 = ((FScheme.Value.Container)args[0]).Item;

            Autodesk.Revit.DB.Face f;

            Reference faceRef = arg0 as Reference;
            if (faceRef != null)
                f = DocumentManager.GetInstance().CurrentUIDocument.Document.GetElement(faceRef.ElementId).GetGeometryObjectFromReference(faceRef) as Autodesk.Revit.DB.Face;
            else
                f = arg0 as Autodesk.Revit.DB.Face;

            if (f != null)
            {
                area = f.Area;
            }

            //Fin
            return FScheme.Value.NewNumber(area);
        }
    }

    [NodeName("Get Surface Domain")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_MEASURE)]
    [NodeDescription("Measure the domain of a surface in U and V.")]
    public class SurfaceDomain : NodeWithOneOutput
    {
        public SurfaceDomain()
        {
            InPortData.Add(new PortData("f", "The surface whose domain you wish to calculate (Reference).", typeof(FScheme.Value.Container)));//Ref to a face of a form
            OutPortData.Add(new PortData("domain", "The surface's domain.", typeof(FScheme.Value.List)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            BoundingBoxUV bbox = null;

            object arg0 = ((FScheme.Value.Container)args[0]).Item;

            Autodesk.Revit.DB.Face f;

            var faceRef = arg0 as Reference;
            if (faceRef != null)
                f = DocumentManager.GetInstance().CurrentUIDocument.Document.GetElement(faceRef.ElementId).GetGeometryObjectFromReference(faceRef) as Autodesk.Revit.DB.Face;
            else
                f = arg0 as Autodesk.Revit.DB.Face;

            if (f != null)
            {
                bbox = f.GetBoundingBox();
            }

            var min = Vector.ByCoordinates(bbox.Min.U, bbox.Min.V,0);
            var max = Vector.ByCoordinates(bbox.Max.U, bbox.Max.V,0);

            return FScheme.Value.NewContainer(DSCoreNodes.Domain2D.ByMinimumAndMaximum(min, max));
        }
    }
}
