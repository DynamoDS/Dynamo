using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Media3D;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Microsoft.FSharp.Collections;

using Value = Dynamo.FScheme.Value;
using Dynamo.Revit;
using Dynamo.Connectors;
using Dynamo.Utilities;

namespace Dynamo.Nodes
{
    [NodeName("Curve Face Intersection")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_INTERSECT)]
    [NodeDescription("Calculates the intersection of a curve and a face.")]
    public class CurveFaceIntersection : RevitTransactionNode
    {
        private readonly PortData _resultPort = new PortData(
            "result", "The set comparison result.", typeof(Value.String));

        private readonly PortData _xyzPort = new PortData(
            "xyz", "The evaluated intersection point(s).", typeof(Value.List));

        private readonly PortData _uvPort = new PortData(
            "uv", "The UV parameter(s) on the face.", typeof(Value.List));

        private readonly PortData _tPort = new PortData(
            "t", "The raw intersection parameter(s) on the curve. ", typeof(Value.List));

        private readonly PortData _edgePort = new PortData(
            "edge", "The edge if the intersection happens to be near an edge of the face.",
            typeof(Value.List));

        private readonly PortData _edgeTPort = new PortData(
            "edge t", "The parameter of the nearest point(s) on the edge.", typeof(Value.List));

        public CurveFaceIntersection()
        {
            InPortData.Add(new PortData("crv", "The specified curve to intersect with this face.", typeof(Value.Container)));
            InPortData.Add(new PortData("face", "The face from which to calculate the intersection.", typeof(Value.Container)));

            OutPortData.Add(_resultPort);
            //OutPortData.Add(new PortData("xsects", "A list of intersection information. {XYZ point, UV point, curve parameter, edge object, edge parameter}", typeof(Value.List)));
            
            //from the API
            // XYZPoint is the evaluated intersection point. 
            // UVPoint is the intersection parameters on the face. 
            // Parameter is the raw intersection parameter on the curve. 
            // EdgeObject is the edge if the intersection happens to be near an edge of the face. 
            // EdgeParameter is the parameter of the nearest point on the edge.

            //set the outputs
            //XYZ point
            OutPortData.Add(_xyzPort);

            //uv point
            OutPortData.Add(_uvPort);

            //parameter
            OutPortData.Add(_tPort);

            //edge object
            OutPortData.Add(_edgePort);

            //edge parameter
            OutPortData.Add(_edgeTPort);

            RegisterAllPorts();
        }

        public override void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            var crv = (Curve)((Value.Container)args[0]).Item;
            Face face = null;
            Autodesk.Revit.DB.Plane thisPlane = null;

            var geo = ((Value.Container)args[1]).Item;

            if (geo is Face)
                face = geo as Face;
            else if (geo is Autodesk.Revit.DB.Plane)
            {
                #region plane processing

                thisPlane = geo as Autodesk.Revit.DB.Plane;
                // tesselate curve and find uv envelope in projection to the plane
                IList<XYZ> tessCurve = crv.Tessellate();
                var curvePointEnum = tessCurve.GetEnumerator();
                var corner1 = new XYZ();
                var corner2 = new XYZ();
                bool cornersSet = false;
                for (; curvePointEnum.MoveNext(); )
                {
                    if (!cornersSet)
                    {
                        corner1 = curvePointEnum.Current;
                        corner2 = curvePointEnum.Current;
                        cornersSet = true;
                    }
                    else
                    {
                        for (int coord = 0; coord < 3; coord++)
                        {
                           if (corner1[coord] > curvePointEnum.Current[coord])
                              corner1 = new XYZ(coord == 0 ? curvePointEnum.Current[coord] : corner1[coord],
                                              coord == 1 ? curvePointEnum.Current[coord] : corner1[coord],
                                              coord == 2 ? curvePointEnum.Current[coord] : corner1[coord]);
                            if (corner2[coord] < curvePointEnum.Current[coord])
                               corner2 = new XYZ(coord == 0 ? curvePointEnum.Current[coord] : corner2[coord],
                                              coord == 1 ? curvePointEnum.Current[coord] : corner2[coord],
                                              coord == 2 ? curvePointEnum.Current[coord] : corner2[coord]);
                        }
                    }
                }
               
                double dist1 = thisPlane.Origin.DistanceTo(corner1);
                double dist2 = thisPlane.Origin.DistanceTo(corner2);
                double sizeRect = 2.0 * (dist1 + dist2) + 100.0;
 
                var cLoop = new Autodesk.Revit.DB.CurveLoop();
                for (int index = 0; index < 4; index++)
                {
                    double coord0 = (index == 0 || index == 3) ? -sizeRect : sizeRect;
                    double coord1 = (index < 2) ? -sizeRect : sizeRect;
                    XYZ pnt0 =  thisPlane.Origin + coord0 * thisPlane.XVec + coord1 * thisPlane.YVec;

                    double coord3 = (index < 2) ? sizeRect : -sizeRect;
                    double coord4 = (index == 0 || index == 3) ? -sizeRect : sizeRect;
                    XYZ pnt1 = thisPlane.Origin + coord3 * thisPlane.XVec + coord4 * thisPlane.YVec;
                    Line cLine = dynRevitSettings.Revit.Application.Create.NewLineBound(pnt0, pnt1);
                    cLoop.Append(cLine);
                }
                var listCLoops = new List<Autodesk.Revit.DB.CurveLoop> { cLoop };

                Solid tempSolid = GeometryCreationUtilities.CreateExtrusionGeometry(listCLoops, thisPlane.Normal, 100.0);

                //find right face

                FaceArray facesOfExtrusion = tempSolid.Faces;
                for (int indexFace = 0; indexFace < facesOfExtrusion.Size; indexFace++)
                {
                    Face faceAtIndex = facesOfExtrusion.get_Item(indexFace);
                    if (faceAtIndex is PlanarFace)
                    {
                        var pFace = faceAtIndex as PlanarFace;
                        if (Math.Abs(thisPlane.Normal.DotProduct(pFace.Normal)) < 0.99)
                            continue;
                        if (Math.Abs(thisPlane.Normal.DotProduct(thisPlane.Origin - pFace.Origin)) > 0.1)
                            continue;
                        face = faceAtIndex;
                        break;
                    }
                }
                if (face == null)
                    throw new Exception("Curve Face Intersection could not process supplied Plane.");

                #endregion
            }

            IntersectionResultArray xsects;
            var result = face.Intersect(crv, out xsects);

            //var xsect_results = FSharpList<Value>.Empty;
            var xsect_xyzs = FSharpList<Value>.Empty;
            var xsect_face_uvs = FSharpList<Value>.Empty;
            var xsect_params = FSharpList<Value>.Empty;
            var xsect_edges = FSharpList<Value>.Empty;
            var xsect_edge_params = FSharpList<Value>.Empty;
            
            var results = FSharpList<Value>.Empty;
            if (xsects != null)
            {
                foreach (IntersectionResult ir in xsects)
                {
                    var xsect = FSharpList<Value>.Empty;
                    try
                    {
                        xsect_edge_params = FSharpList<Value>.Cons(Value.NewNumber(ir.EdgeParameter), xsect_edge_params);
                    }
                    catch
                    {
                        xsect_edge_params = FSharpList<Value>.Cons(Value.NewNumber(0), xsect_edge_params);
                    }
                    xsect_edges = FSharpList<Value>.Cons(Value.NewContainer(ir.EdgeObject), xsect_edges);
                    xsect_params = FSharpList<Value>.Cons(Value.NewNumber(ir.Parameter), xsect_params);

                    if (thisPlane != null)
                    {
                        UV planeUV = new UV(thisPlane.XVec.DotProduct(ir.XYZPoint - thisPlane.Origin),  
                                             thisPlane.YVec.DotProduct(ir.XYZPoint - thisPlane.Origin));
                        xsect_face_uvs = FSharpList<Value>.Cons(Value.NewContainer(planeUV), xsect_face_uvs);
                    }
                    else
                        xsect_face_uvs = FSharpList<Value>.Cons(Value.NewContainer(ir.UVPoint), xsect_face_uvs);

                    xsect_xyzs = FSharpList<Value>.Cons(Value.NewContainer(ir.XYZPoint), xsect_xyzs);

                    //xsect_results = FSharpList<Value>.Cons(Value.NewList(xsect), xsect_results);
                }
            }

            outPuts[_edgeTPort] = Value.NewList(xsect_edge_params);
            outPuts[_edgePort] = Value.NewList(xsect_edges);
            outPuts[_tPort] = Value.NewList(xsect_params);
            outPuts[_uvPort] = Value.NewList(xsect_face_uvs);
            outPuts[_xyzPort] = Value.NewList(xsect_xyzs);
            outPuts[_resultPort] = Value.NewString(result.ToString());
        }
    }

    [NodeName("Curve Curve Intersection")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_INTERSECT)]
    [NodeDescription("Calculates the intersection of two curves.")]
    public class CurveCurveIntersection : RevitTransactionNode
    {
        private readonly PortData _resultPort = new PortData(
            "result", "The set comparison result.", typeof(Value.String));

        private readonly PortData _xyzPort = new PortData(
            "xyz", "The evaluated intersection point(s).", typeof(Value.List));

        private readonly PortData _uPort = new PortData(
            "u", "The unnormalized U parameter(s) on this curve.", typeof(Value.List));

        private readonly PortData _vPort = new PortData(
            "v", "The unnormalized V parameter(s) on this curve.", typeof(Value.List));

        public CurveCurveIntersection()
        {
            InPortData.Add(new PortData("crv1", "The curve with which to intersect.", typeof(Value.Container)));
            InPortData.Add(new PortData("crv2", "The intersecting curve.", typeof(Value.Container)));

            OutPortData.Add(_resultPort);
            //OutPortData.Add(new PortData("xsects", "A list of intersection information. {XYZ point, curve 1 parameter, curve 2 parameter}", typeof(Value.List)));

            // from the API
            // XYZPoint is the evaluated intersection point
            // UVPoint.U is the unnormalized parameter on this curve (use ComputeNormalizedParameter to compute the normalized value).
            // UVPoint.V is the unnormalized parameter on the specified curve (use ComputeNormalizedParameter to compute the normalized value).
            OutPortData.Add(_xyzPort);
            OutPortData.Add(_uPort);
            OutPortData.Add(_vPort);
            

            RegisterAllPorts();
        }

        public override void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            var crv1 = (Curve)((Value.Container)args[0]).Item;
            var crv2 = (Curve)((Value.Container)args[1]).Item;

            IntersectionResultArray xsects;
            SetComparisonResult result = crv1.Intersect(crv2, out xsects);

            var xyz = FSharpList<Value>.Empty;
            var u = FSharpList<Value>.Empty;
            var v = FSharpList<Value>.Empty;

            if (xsects != null)
            {
                foreach (IntersectionResult ir in xsects)
                {
                    xyz = FSharpList<Value>.Cons(Value.NewContainer(ir.XYZPoint), xyz);
                    u = FSharpList<Value>.Cons(Value.NewNumber(ir.UVPoint.U), u);
                    v = FSharpList<Value>.Cons(Value.NewNumber(ir.UVPoint.V), v);

                }
                
            }

            outPuts[_vPort] = Value.NewList(v);
            outPuts[_uPort] = Value.NewList(u);
            outPuts[_xyzPort] = Value.NewList(xyz);
            outPuts[_resultPort] = Value.NewString(result.ToString());
        }
    }

    [NodeName("Face Face Intersection")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_INTERSECT)]
    [NodeDescription("Calculates the intersection of two faces.")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    public class FaceFaceIntersection : RevitTransactionNode
    {
        private readonly PortData _resultPort = new PortData(
            "result", "The intersection result.", typeof(Value.String));

        private readonly PortData _curvePort = new PortData(
            "curve", "A single Curve representing the intersection.", typeof(Value.Container));

        public FaceFaceIntersection()
        {
            InPortData.Add(new PortData("face1", "The first face to intersect.", typeof(Value.Container)));
            InPortData.Add(new PortData("face2", "The face to intersect with face1.", typeof(Value.Container)));

            OutPortData.Add(_resultPort);
            OutPortData.Add(_curvePort);

            RegisterAllPorts();
        }

        public override void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            var face1 = (Face)((Value.Container)args[0]).Item;
            var face2 = (Face)((Value.Container)args[1]).Item;

            Type faceType = typeof(Face);
            MethodInfo[] faceMethods = faceType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            const string nameOfMethodIntersect = "Intersect";

            bool set = false;
            foreach (MethodInfo mi in faceMethods)
            {
                //find a method that matches the name
                if (mi.Name == nameOfMethodIntersect)
                {
                    //find the method which matches the signature
                    ParameterInfo[] pi = mi.GetParameters();
                    if (pi.Length == 2 && 
                        pi[0].ParameterType == typeof(Face) && 
                        pi[1].ParameterType == typeof(Curve).MakeByRefType())
                    {
                        var methodArgs = new object[2];
                        methodArgs[0] = face2;
                        methodArgs[1] = null;

                        //var result = face1.Intersect(face2, out resultCurve);
                        var result = mi.Invoke(face1, methodArgs);
                  
                        set = true;

                        outPuts[_resultPort] = Value.NewString(result.ToString());
                        outPuts[_curvePort] = Value.NewContainer(methodArgs[1]);
                    }
                }
            }

            if (!set)
                throw new Exception("No suitable method found to perform intersection");
        }
    }

    //[NodeName("Face Level Intersection")]
    //[NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_INTERSECT)]
    //[NodeDescription("Calculates the intersection of a face and a level.")]
    //[DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    //public class dynFaceLevelIntersection : dynRevitTransactionNode
    //{
    //    public dynFaceLevelIntersection()
    //    {
    //        InPortData.Add(new PortData("face", "The first face to intersect.", typeof(Value.Container)));
    //        InPortData.Add(new PortData("level", "The level to intersect with the face.", typeof(Value.Container)));

    //        OutPortData.Add(new PortData("result", "The intersection result.", typeof(Value.String)));
    //        OutPortData.Add(new PortData("curve", "A single Curve representing the intersection.", typeof(Value.Container)));

    //        RegisterAllPorts();
    //    }

    //    public override Value Evaluate(FSharpList<Value> args)
    //    {
    //        var face1 = (Face)((Value.Container)args[0]).Item;
    //        var level = (Level)((Value.Container)args[1]).Item;

    //        Type faceType = typeof(Autodesk.Revit.DB.Face);
    //        MethodInfo[] faceMethods = faceType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
    //        string nameOfMethodIntersect = "Intersect";

    //        Face face2 = null;

    //        var geometryOptions = new Options();
    //        geometryOptions.ComputeReferences = true;
    //        geometryOptions.DetailLevel = ViewDetailLevel.Medium;
    //        geometryOptions.IncludeNonVisibleObjects = true;
    //        var geomElem = level.get_Geometry(geometryOptions);

    //        foreach (Autodesk.Revit.DB.GeometryObject geomObj in geomElem)
    //        {
    //            face2 = geomObj as Face;
    //        }
    //        if(face2 == null)
    //            throw new Exception("A surface could not be derived from the level.");

    //        Curve resultCurve = null;
    //        var results = FSharpList<Value>.Empty;

    //        foreach (MethodInfo mi in faceMethods)
    //        {
    //            //find a method that matches the name
    //            if (mi.Name == nameOfMethodIntersect)
    //            {
    //                //find the method which matches the signature
    //                ParameterInfo[] pi = mi.GetParameters();
    //                if (pi.Length == 2 &&
    //                    pi[0].ParameterType == typeof(Face) &&
    //                    pi[1].ParameterType == typeof(Curve).MakeByRefType())
    //                {
    //                    object[] methodArgs = new object[2];
    //                    methodArgs[0] = face2;
    //                    methodArgs[1] = resultCurve;

    //                    //var result = face1.Intersect(face2, out resultCurve);
    //                    var result = mi.Invoke(face1, methodArgs);
    //                    if (methodArgs[1] != null)
    //                        curves.Add((Curve)methodArgs[1]);

    //                    results = FSharpList<Value>.Cons(Value.NewContainer(methodArgs[1]), results);
    //                    results = FSharpList<Value>.Cons(Value.NewString(result.ToString()), results);
    //                }
    //            }
    //        }

    //        return Value.NewList(results);
    //    }
    //}

}
