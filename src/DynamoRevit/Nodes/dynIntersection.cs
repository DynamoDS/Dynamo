using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Media3D;
using Autodesk.Revit.DB;

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
    public class dynCurveFaceIntersection : dynRevitTransactionNode, IDrawable, IClearable
    {
        public dynCurveFaceIntersection()
        {
            InPortData.Add(new PortData("crv", "The specified curve to intersect with this face.", typeof(Value.Container)));
            InPortData.Add(new PortData("face", "The face from which to calculate the intersection.", typeof(Value.Container)));

            OutPortData.Add(new PortData("result", "The set comparison result.", typeof(Value.String)));
            //OutPortData.Add(new PortData("xsects", "A list of intersection information. {XYZ point, UV point, curve parameter, edge object, edge parameter}", typeof(Value.List)));
            
            //from the API
            // XYZPoint is the evaluated intersection point. 
            // UVPoint is the intersection parameters on the face. 
            // Parameter is the raw intersection parameter on the curve. 
            // EdgeObject is the edge if the intersection happens to be near an edge of the face. 
            // EdgeParameter is the parameter of the nearest point on the edge.

            //set the outputs
            //XYZ point
            OutPortData.Add(new PortData("xyz", "The evaluated intersection point(s).", typeof(Value.List)));

            //uv point
            OutPortData.Add(new PortData("uv", "The UV parameter(s) on the face.", typeof(Value.List)));

            //parameter
            OutPortData.Add(new PortData("t", "The raw intersection parameter(s) on the curve. ", typeof(Value.List)));

            //edge object
            OutPortData.Add(new PortData("edge", "The edge if the intersection happens to be near an edge of the face.", typeof(Value.List)));

            //edge parameter
            OutPortData.Add(new PortData("edge t", "The parameter of the nearest point(s) on the edge.", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var crv = (Curve)((Value.Container)args[0]).Item;
            Face face = null;
            Solid tempSolid = null;
            Plane thisPlane = null;

            if (((Value.Container)args[1]).Item is Face)
                face = (Autodesk.Revit.DB.Face)((Value.Container)args[1]).Item;
            else if (((Value.Container)args[1]).Item is Plane)
            {
                #region plane processing

                thisPlane = ((Value.Container)args[1]).Item as Plane;
                // tesselate curve and find uv envelope in projection to the plane
                IList<XYZ> tessCurve = crv.Tessellate();
                var curvePointEnum = tessCurve.GetEnumerator();
                XYZ corner1 = new XYZ();
                XYZ corner2 = new XYZ();
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
 

                CurveLoop cLoop = new CurveLoop();
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
                List<CurveLoop> listCLoops = new List<CurveLoop> ();
                listCLoops.Add(cLoop);

                tempSolid = GeometryCreationUtilities.CreateExtrusionGeometry(listCLoops, thisPlane.Normal, 100.0);

                //find right face

                FaceArray facesOfExtrusion = tempSolid.Faces;
                for (int indexFace = 0; indexFace < facesOfExtrusion.Size; indexFace++)
                {
                    Face faceAtIndex = facesOfExtrusion.get_Item(indexFace);
                    if (faceAtIndex is PlanarFace)
                    {
                        PlanarFace pFace = faceAtIndex as PlanarFace;
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

            var xsects = new IntersectionResultArray();
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
                    pts.Add(ir.XYZPoint);

                    //xsect_results = FSharpList<Value>.Cons(Value.NewList(xsect), xsect_results);
                }
            }

            //results = FSharpList<Value>.Cons(Value.NewList(xsect_results), results);
            results = FSharpList<Value>.Cons(Value.NewList(xsect_edge_params), results);
            results = FSharpList<Value>.Cons(Value.NewList(xsect_edges), results);
            results = FSharpList<Value>.Cons(Value.NewList(xsect_params), results);
            results = FSharpList<Value>.Cons(Value.NewList(xsect_face_uvs), results);
            results = FSharpList<Value>.Cons(Value.NewList(xsect_xyzs), results);
            results = FSharpList<Value>.Cons(Value.NewString(result.ToString()), results);

            return Value.NewList(results);
        }

        #region IDrawable Interface
        protected List<XYZ> pts = new List<XYZ>();
        public RenderDescription RenderDescription { get; set; }
        public void Draw()
        {
            if (this.RenderDescription == null)
                this.RenderDescription = new RenderDescription();
            else
                this.RenderDescription.ClearAll();

            foreach (XYZ pt in pts)
                this.RenderDescription.points.Add(new Point3D(pt.X, pt.Y, pt.Z));
        }
        #endregion

        #region IClearable Interface
        public void ClearReferences()
        {
            pts.Clear();
        }
        #endregion
    }

    [NodeName("Curve Curve Intersection")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_INTERSECT)]
    [NodeDescription("Calculates the intersection of two curves.")]
    public class dynCurveCurveIntersection : dynRevitTransactionNode, IDrawable, IClearable
    {
        public dynCurveCurveIntersection()
        {
            InPortData.Add(new PortData("crv1", "The curve with which to intersect.", typeof(Value.Container)));
            InPortData.Add(new PortData("crv2", "The intersecting curve.", typeof(Value.Container)));

            OutPortData.Add(new PortData("result", "The set comparison result.", typeof(Value.String)));
            //OutPortData.Add(new PortData("xsects", "A list of intersection information. {XYZ point, curve 1 parameter, curve 2 parameter}", typeof(Value.List)));

            // from the API
            // XYZPoint is the evaluated intersection point
            // UVPoint.U is the unnormalized parameter on this curve (use ComputeNormalizedParameter to compute the normalized value).
            // UVPoint.V is the unnormalized parameter on the specified curve (use ComputeNormalizedParameter to compute the normalized value).
            OutPortData.Add(new PortData("xyz", "The evaluated intersection point(s).", typeof(Value.List)));
            OutPortData.Add(new PortData("u", "The unnormalized U parameter(s) on this curve.", typeof(Value.List)));
            OutPortData.Add(new PortData("v", "The unnormalized V parameter(s) on this curve.", typeof(Value.List)));
            

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var crv1 = (Curve)((Value.Container)args[0]).Item;
            var crv2 = (Curve)((Value.Container)args[1]).Item;

            IntersectionResultArray xsects = new IntersectionResultArray();
            SetComparisonResult result = crv1.Intersect(crv2, out xsects);
            
            var results = FSharpList<Value>.Empty;
            var xyz = FSharpList<Value>.Empty;
            var u = FSharpList<Value>.Empty;
            var v = FSharpList<Value>.Empty;
            

            //var xsect_results = FSharpList<Value>.Empty;
            if (xsects != null)
            {
                foreach (IntersectionResult ir in xsects)
                {
                    //var xsect = FSharpList<Value>.Empty;
                    xyz = FSharpList<Value>.Cons(Value.NewContainer(ir.XYZPoint), xyz);
                    u = FSharpList<Value>.Cons(Value.NewNumber(ir.UVPoint.U), u);
                    v = FSharpList<Value>.Cons(Value.NewNumber(ir.UVPoint.V), v);
                    
                    //xsect_results = FSharpList<Value>.Cons(Value.NewList(xsect), xsect_results);

                    pts.Add(ir.XYZPoint);
                }
                
            }
            //results = FSharpList<Value>.Cons(Value.NewList(xsect_results), results);

            results = FSharpList<Value>.Cons(Value.NewList(v), results);
            results = FSharpList<Value>.Cons(Value.NewList(u), results);
            results = FSharpList<Value>.Cons(Value.NewList(xyz), results);
            results = FSharpList<Value>.Cons(Value.NewString(result.ToString()), results);

            return Value.NewList(results);
        }

        #region IDrawable Interface
        protected List<XYZ> pts = new List<XYZ>();
        public RenderDescription RenderDescription { get; set; }
        public void Draw()
        {
            if (this.RenderDescription == null)
                this.RenderDescription = new RenderDescription();
            else
                this.RenderDescription.ClearAll();

            foreach (XYZ pt in pts)
                this.RenderDescription.points.Add(new Point3D(pt.X, pt.Y, pt.Z));
        }
        #endregion

        #region IClearable Interface
        public void ClearReferences()
        {
            pts.Clear();
        }
        #endregion
    }

    [NodeName("Face Face Intersection")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_INTERSECT)]
    [NodeDescription("Calculates the intersection of two faces.")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    public class dynFaceFaceIntersection : dynRevitTransactionNode, IDrawable, IClearable
    {
        public dynFaceFaceIntersection()
        {
            InPortData.Add(new PortData("face1", "The first face to intersect.", typeof(Value.Container)));
            InPortData.Add(new PortData("face2", "The face to intersect with face1.", typeof(Value.Container)));

            OutPortData.Add(new PortData("result", "The intersection result.", typeof(Value.String)));
            OutPortData.Add(new PortData("curve", "A single Curve representing the intersection.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var face1 = (Face)((Value.Container)args[0]).Item;
            var face2 = (Face)((Value.Container)args[1]).Item;

            Type faceType = typeof(Autodesk.Revit.DB.Face);
            MethodInfo[] faceMethods = faceType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            string nameOfMethodIntersect = "Intersect";

            Curve resultCurve = null;
            var results = FSharpList<Value>.Empty;

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
                        object[] methodArgs = new object[2];
                        methodArgs[0] = face2;
                        methodArgs[1] = resultCurve;

                        //var result = face1.Intersect(face2, out resultCurve);
                        var result = (FaceIntersectionFaceResult)mi.Invoke(face1, methodArgs);
                        if (methodArgs[1] != null)
                            curves.Add((Curve)methodArgs[1]);

                        results = FSharpList<Value>.Cons(Value.NewContainer(methodArgs[1]), results);
                        results = FSharpList<Value>.Cons(Value.NewString(result.ToString()), results);
                    }
                }
            }
            
            return Value.NewList(results);
        }

        #region IDrawable Interface
        protected List<Curve> curves = new List<Curve>();
        public RenderDescription RenderDescription { get; set; }
        public void Draw()
        {
            if (this.RenderDescription == null)
                this.RenderDescription = new RenderDescription();
            else
                this.RenderDescription.ClearAll();

            foreach (Curve crv in curves)
            {
                //convert the tessellated curve to a render description
                IList<XYZ> curvePts = crv.Tessellate();
                for (int i = 1; i < curvePts.Count; i++)
                {
                    var a = curvePts[i - 1];
                    var b = curvePts[i];

                    RenderDescription.lines.Add(new Point3D(a.X, a.Y, a.Z));
                    RenderDescription.lines.Add(new Point3D(b.X, b.Y, b.Z));
                }
            }
        }

        public void ClearReferences()
        {
            curves.Clear();
        }
        #endregion
    }
}
