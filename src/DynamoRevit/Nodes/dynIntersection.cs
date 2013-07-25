using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

using Autodesk.Revit;
using Autodesk.Revit.DB;

using Microsoft.FSharp.Collections;

using Value = Dynamo.FScheme.Value;
using Dynamo.FSchemeInterop;
using Dynamo.Revit;
using Dynamo.Connectors;
using Dynamo.Utilities;

namespace Dynamo.Nodes
{
    [NodeName("Curve Face Intersection")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_INTERSECT)]
    [NodeDescription("Calculates the intersection of the specified curve with this face.")]
    public class dynCurveFaceIntersection : dynRevitTransactionNode
    {
        public dynCurveFaceIntersection()
        {
            InPortData.Add(new PortData("crv", "The specified curve to intersect with this face.", typeof(Value.Container)));
            InPortData.Add(new PortData("face", "The face from which to calculate the intersection.", typeof(Value.Container)));

            OutPortData.Add(new PortData("result", "The set comparison result.", typeof(Value.String)));
            OutPortData.Add(new PortData("xsects", "A list of intersection information. {XYZ point, UV point, curve parameter, edge object, edge parameter}", typeof(Value.List)));

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
            }

            IntersectionResultArray xsects = new IntersectionResultArray();
            SetComparisonResult result = face.Intersect(crv, out xsects);

            var xsect_results = FSharpList<Value>.Empty;
            var results = FSharpList<Value>.Empty;
            if (xsects != null)
            {
                foreach (IntersectionResult ir in xsects)
                {
                    var xsect = FSharpList<Value>.Empty;
                    try
                    {
                        xsect = FSharpList<Value>.Cons(Value.NewNumber(ir.EdgeParameter), xsect);
                    }
                    catch
                    {
                        xsect = FSharpList<Value>.Cons(Value.NewNumber(0), xsect);
                    }
                    xsect = FSharpList<Value>.Cons(Value.NewContainer(ir.EdgeObject), xsect);
                    xsect = FSharpList<Value>.Cons(Value.NewNumber(ir.Parameter), xsect);
                    if (thisPlane != null)
                    {
                        UV planeUV = new UV(thisPlane.XVec.DotProduct(ir.XYZPoint - thisPlane.Origin),  
                                             thisPlane.YVec.DotProduct(ir.XYZPoint - thisPlane.Origin));
                        xsect = FSharpList<Value>.Cons(Value.NewContainer(planeUV), xsect);
                    }
                    else
                       xsect = FSharpList<Value>.Cons(Value.NewContainer(ir.UVPoint), xsect);

                    xsect = FSharpList<Value>.Cons(Value.NewContainer(ir.XYZPoint), xsect);
                    xsect_results = FSharpList<Value>.Cons(Value.NewList(xsect), xsect_results);
                }
            }
            results = FSharpList<Value>.Cons(Value.NewList(xsect_results), results);
            results = FSharpList<Value>.Cons(Value.NewString(result.ToString()), results);
            return Value.NewList(results);
        }
    }

    [NodeName("Curve Curve Intersection")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_INTERSECT)]
    [NodeDescription("Calculates the intersection of the specified curve with this face.")]
    public class dynCurveCurveIntersection : dynRevitTransactionNode, IDrawable, IClearable
    {
        public dynCurveCurveIntersection()
        {
            InPortData.Add(new PortData("crv1", "The curve with which to intersect.", typeof(Value.Container)));
            InPortData.Add(new PortData("crv2", "The intersecting curve.", typeof(Value.Container)));

            OutPortData.Add(new PortData("result", "The set comparison result.", typeof(Value.String)));
            OutPortData.Add(new PortData("xsects", "A list of intersection information. {XYZ point, curve 1 parameter, curve 2 parameter}", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var crv1 = (Curve)((Value.Container)args[0]).Item;
            var crv2 = (Curve)((Value.Container)args[1]).Item;

            IntersectionResultArray xsects = new IntersectionResultArray();
            SetComparisonResult result = crv1.Intersect(crv2, out xsects);
            var results = FSharpList<Value>.Empty;

            var xsect_results = FSharpList<Value>.Empty;
            if (xsects != null)
            {
                foreach (IntersectionResult ir in xsects)
                {
                    var xsect = FSharpList<Value>.Empty;
                    xsect = FSharpList<Value>.Cons(Value.NewNumber(ir.UVPoint.U), xsect);
                    xsect = FSharpList<Value>.Cons(Value.NewNumber(ir.UVPoint.V), xsect);
                    xsect = FSharpList<Value>.Cons(Value.NewContainer(ir.XYZPoint), xsect);
                    xsect_results = FSharpList<Value>.Cons(Value.NewList(xsect), xsect_results);

                    pts.Add(ir.XYZPoint);
                }
                
            }
            results = FSharpList<Value>.Cons(Value.NewList(xsect_results), results);
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

        public void ClearReferences()
        {
            pts.Clear();
        }
        #endregion
    }
}
