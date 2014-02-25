using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using HelixToolkit.Wpf;
using Curve = Autodesk.Revit.DB.Curve;
using Solid = Autodesk.Revit.DB.Solid;
using Face = Autodesk.Revit.DB.Face;
using Edge = Autodesk.Revit.DB.Edge;
using PointCollection = System.Windows.Media.PointCollection;

namespace Dynamo
{
    class VisualizationManagerRevit : VisualizationManager
    {
        public VisualizationManagerRevit(DynamoController controller):base(controller)
        {
            if (dynSettings.Controller.Context == Context.VASARI_2014)
            {
                AlternateDrawingContextAvailable = true;
                DrawToAlternateContext = false;

                AlternateContextName = "Vasari";
            }
            else
            {
                AlternateDrawingContextAvailable = false;
            }

            //Visualizers.Add(typeof(Element), DrawElement);
            //Visualizers.Add(typeof(Transform), DrawTransform);
            //Visualizers.Add(typeof(XYZ), DrawXyz);
            //Visualizers.Add(typeof(ParticleSystem), DrawParticleSystem);
            //Visualizers.Add(typeof(TriangleFace), DrawTriangleFace);
            //Visualizers.Add(typeof(GeometryObject), DrawGeometryObject);
            //Visualizers.Add(typeof(Autodesk.Revit.DB.CurveLoop), DrawCurveLoop);
            //Visualizers.Add(typeof(Facet), DrawFacet);
        }

        //private void DrawElement(NodeModel node, object obj, string tag, RenderDescription rd, Octree.OctreeSearch.Octree octree)
        //{
        //    if (obj == null)
        //        return;

        //    if (obj is CurveElement)
        //    {
        //        DrawCurveElement(node, obj, tag, rd, octree);
        //    }
        //    else if (obj is ReferencePoint)
        //    {
        //        DrawReferencePoint(node, obj, rd, octree);
        //    }
        //    else if (obj is Form)
        //    {
        //        DrawForm(node, obj, tag, rd, octree);
        //    }
        //    else if (obj is GeometryElement)
        //    {
        //        DrawGeometryElement(node, obj, tag, rd, octree);
        //    }
        //    else if (obj is GeometryObject)
        //    {
        //        DrawGeometryObject(node, obj, tag, rd, octree);
        //    }
        //    else
        //    {
        //        var elem = obj as Element;
        //        if (elem != null)
        //        {
        //            var o = new Options { DetailLevel = ViewDetailLevel.Medium };
        //            GeometryElement geom = elem.get_Geometry(o);

        //            if (geom != null)
        //            {
        //                DrawGeometryObject(node, geom, tag, rd, octree);
        //            }
        //        }
        //    }
        //}

        //private void DrawFace(NodeModel node, object obj, RenderDescription rd, Octree.OctreeSearch.Octree octree)
        //{
        //    var face = obj as Face;

        //    if (face == null)
        //        return;

        //    var mesh = face.Triangulate(0.2);
        //    if (mesh == null)
        //    {
        //        Debug.WriteLine("Mesh could not be computed from face.");
        //        return;
        //    }

        //    if (node.IsSelected)
        //    {
        //        rd.SelectedMeshes.Add(RevitMeshToHelixMesh(mesh, octree, node));
        //    }
        //    else
        //    {
        //        rd.Meshes.Add(RevitMeshToHelixMesh(mesh, octree,node));
        //    }
        //}

        //private void DrawForm(NodeModel node, object obj, string tag, RenderDescription rd, Octree.OctreeSearch.Octree octree)
        //{
        //    var form = obj as Form;

        //    if (form == null)
        //        return;

        //    DrawGeometryElement(node, form.get_Geometry(new Options()), tag, rd, octree);
        //}

        //private void DrawGeometryElement(NodeModel node, object obj, string tag, RenderDescription rd, Octree.OctreeSearch.Octree octree)
        //{
        //    try
        //    {
        //        var gelem = obj as GeometryElement;

        //        foreach (GeometryObject go in gelem)
        //        {
        //            DrawGeometryObject(node, go, tag, rd, octree);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        DynamoLogger.Instance.Log(ex.Message);
        //        DynamoLogger.Instance.Log(ex.StackTrace);
        //    }

        //}

        //private void DrawGeometryObject(NodeModel node, object obj,string tag, RenderDescription rd, Octree.OctreeSearch.Octree octree)
        //{
        //    if (obj == null)
        //        return;

        //    if (obj is XYZ)
        //    {
        //        DrawXyz(node, obj, tag, rd, octree);
        //    }
        //    if (obj is Curve)
        //    {
        //        DrawCurve(node, obj, tag, rd, octree);
        //    }
        //    else if (obj is Solid)
        //    {
        //        DrawSolid(node, obj, tag, rd, octree);
        //    }
        //    else if (obj is Face)
        //    {
        //        DrawFace(node, obj, rd, octree);
        //    }
        //    else
        //    {
        //        DrawUndrawable(node, obj, rd, octree);
        //    }
        //}

        //private void DrawUndrawable(NodeModel node, object obj, RenderDescription rd, Octree.OctreeSearch.Octree octree)
        //{
        //    //TODO: write a message, throw an exception, draw a question mark
        //}

        //private void DrawReferencePoint(NodeModel node, object obj, RenderDescription rd, Octree.OctreeSearch.Octree octree)
        //{
        //    var point = obj as ReferencePoint;

        //    if (point == null)
        //        return;

        //    if (node.IsSelected)
        //    {
        //        rd.SelectedPoints.Add(new Point3D(point.GetCoordinateSystem().Origin.X,
        //        point.GetCoordinateSystem().Origin.Y,
        //        point.GetCoordinateSystem().Origin.Z));
        //    }
        //    else
        //    {
        //        rd.Points.Add(new Point3D(point.GetCoordinateSystem().Origin.X,
        //        point.GetCoordinateSystem().Origin.Y,
        //        point.GetCoordinateSystem().Origin.Z));
        //    }
        //}

        //private void DrawXyz(NodeModel node, object obj, string tag, RenderDescription rd, Octree.OctreeSearch.Octree octree)
        //{
        //    var point = obj as XYZ;
        //    if (point == null)
        //        return;

        //    var pt = new Point3D(point.X, point.Y, point.Z);

        //    if(node.IsSelected)
        //        rd.SelectedPoints.Add(pt);
        //    else
        //        rd.Points.Add(pt);

        //    if (node.DisplayLabels)
        //    {
        //        rd.Text.Add(new BillboardTextItem { Text = tag, Position = pt });
        //    }
        //}

        //private void DrawCurve(NodeModel node, object obj, string tag, RenderDescription rd, Octree.OctreeSearch.Octree octree)
        //{
        //    var curve = obj as Curve;

        //    if (curve == null)
        //        return;

        //    IList<XYZ> points = curve.Tessellate();

        //    bool selected = node.IsSelected;

        //    for (int i = 0; i < points.Count-1; ++i)
        //    {
        //        XYZ start = points[i];
        //        XYZ end = points[i+1];

        //        var startPt = new Point3D(start.X, start.Y, start.Z);
        //        var endPt = new Point3D(end.X, end.Y, end.Z);

        //        //draw a label at the start of the curve
        //        if (node.DisplayLabels && i==0)
        //        {
        //            rd.Text.Add(new BillboardTextItem { Text = tag, Position = startPt });
        //        }

        //        if (selected)
        //        {
        //            rd.SelectedLines.Add(startPt);
        //            rd.SelectedLines.Add(endPt);
        //        }
        //        else
        //        {
        //            rd.Lines.Add(startPt);
        //            rd.Lines.Add(endPt);
        //        }
        //    }
        //}

        //private void DrawCurveElement(NodeModel node, object obj, string tag, RenderDescription rd, Octree.OctreeSearch.Octree octree)
        //{
        //    var elem = obj as CurveElement;

        //    if (elem == null)
        //        return;

        //    DrawCurve(node, elem.GeometryCurve, tag, rd, octree);
        //}

        //private void DrawSolid(NodeModel node, object obj, string tag, RenderDescription rd, Octree.OctreeSearch.Octree octree)
        //{
        //    var solid = obj as Solid;

        //    if (solid == null)
        //        return;

        //    foreach (Face f in solid.Faces)
        //    {
        //        DrawFace(node, f, rd, octree);
        //    }

        //    foreach (Edge edge in solid.Edges)
        //    {
        //        DrawCurve(node, edge.AsCurve(), tag, rd, octree);
        //    }
        //}

        //private void DrawTransform(NodeModel node, object obj, string tag, RenderDescription rd, Octree.OctreeSearch.Octree octree)
        //{
        //    var t = obj as Transform;
        //    if (t == null)
        //        return;

        //    var origin = new Point3D(t.Origin.X, t.Origin.Y, t.Origin.Z);
        //    XYZ x1 = t.Origin + t.BasisX.Normalize();
        //    XYZ y1 = t.Origin + t.BasisY.Normalize();
        //    XYZ z1 = t.Origin + t.BasisZ.Normalize();
        //    var xEnd = new Point3D(x1.X, x1.Y, x1.Z);
        //    var yEnd = new Point3D(y1.X, y1.Y, y1.Z);
        //    var zEnd = new Point3D(z1.X, z1.Y, z1.Z);

        //    rd.XAxisPoints.Add(origin);
        //    rd.XAxisPoints.Add(xEnd);

        //    rd.YAxisPoints.Add(origin);
        //    rd.YAxisPoints.Add(yEnd);

        //    rd.ZAxisPoints.Add(origin);
        //    rd.ZAxisPoints.Add(zEnd);
        //}

        //private void DrawTriangleFace(NodeModel node, object obj,string tag, RenderDescription rd, Octree.OctreeSearch.Octree octree)
        //{
        //    var face = obj as TriangleFace;
        //    if (face == null)
        //        return;

        //    var builder = new MeshBuilder();

        //    builder.Positions.Add(face.Vertices[1].ToPoint3D());
        //    builder.Positions.Add(face.Vertices[0].ToPoint3D());
        //    builder.Positions.Add(face.Vertices[2].ToPoint3D());
        //    builder.TriangleIndices.Add(builder.Positions.Count-1);
        //    builder.TriangleIndices.Add(builder.Positions.Count - 2);
        //    builder.TriangleIndices.Add(builder.Positions.Count-3);
        //    builder.TextureCoordinates.Add(new System.Windows.Point(0, 0));
        //    builder.TextureCoordinates.Add(new System.Windows.Point(0, 0));
        //    builder.TextureCoordinates.Add(new System.Windows.Point(0, 0));

        //    if (node.IsSelected)
        //    {
        //        rd.SelectedMeshes.Add(builder.ToMesh(true));
        //    }
        //    else
        //    {
        //        rd.Meshes.Add(builder.ToMesh(true));
        //    }
        //}

        //private void DrawParticleSystem(NodeModel node, object obj,string tag, RenderDescription rd, Octree.OctreeSearch.Octree octree)
        //{
        //    var ps = obj as ParticleSystem;
        //    if (ps == null)
        //        return;

        //    for (int i = 0; i < ps.numberOfParticles(); i++)
        //    {
        //        Particle p = ps.getParticle(i);
        //        XYZ pos = p.getPosition();
        //        if (i < rd.Points.Count)
        //        {
        //            rd.Points[i] = new Point3D(pos.X, pos.Y, pos.Z);
        //        }
        //        else
        //        {
        //            var pt = new Point3D(pos.X, pos.Y, pos.Z);
        //            rd.Points.Add(pt);
        //        }
        //    }

        //    for (int i = 0; i < ps.numberOfSprings(); i++)
        //    {
        //        ParticleSpring spring = ps.getSpring(i);
        //        XYZ pos1 = spring.getOneEnd().getPosition();
        //        XYZ pos2 = spring.getTheOtherEnd().getPosition();

        //        if (i * 2 + 1 < rd.Lines.Count)
        //        {
        //            rd.Lines[i * 2] = new Point3D(pos1.X, pos1.Y, pos1.Z);
        //            rd.Lines[i * 2 + 1] = new Point3D(pos2.X, pos2.Y, pos2.Z);
        //        }
        //        else
        //        {
        //            var pt1 = new Point3D(pos1.X, pos1.Y, pos1.Z);
        //            var pt2 = new Point3D(pos2.X, pos2.Y, pos2.Z);

        //            rd.Lines.Add(pt1);
        //            rd.Lines.Add(pt2);
        //        }
        //    }
        //}

        //private void DrawCurveLoop(NodeModel node, object obj,string tag, RenderDescription rd, Octree.OctreeSearch.Octree octree)
        //{
        //    var cl = obj as Autodesk.Revit.DB.CurveLoop;
        //    if (cl == null)
        //        return;

        //    foreach (var crv in cl)
        //    {
        //        DrawCurve(node, crv, tag, rd, octree);
        //    }
        //}

        //private void DrawFacet(NodeModel node, object obj, string tag, RenderDescription rd,
        //    Octree.OctreeSearch.Octree octree)
        //{
        //    var facet = obj as Facet;
        //    if (facet == null)
        //        return;

        //    var builder = new MeshBuilder();
        //    var points = new Point3DCollection();
        //    var tex = new PointCollection();
        //    var norms = new Vector3DCollection();
        //    var tris = new List<int>();

        //    var a = facet.Points[0];
        //    var b = facet.Points[1];
        //    var c = facet.Points[2];

        //    var side1 = (b - a).Normalize();
        //    var side2 = (c - a).Normalize();
        //    var norm = side1.CrossProduct(side2);

        //    int count = 0;
        //    foreach (var pt in facet.Points)
        //    {
        //        points.Add(new Point3D(pt.X,pt.Y,pt.Z));
        //        tex.Add(new System.Windows.Point(0,0));
        //        tris.Add(count);
        //        norms.Add(new Vector3D(norm.X,norm.Y,norm.Z));
        //        count++;
        //    }

        //    builder.Append(points, tris, norms, tex);

        //    if (node.IsSelected)
        //    {
        //        rd.SelectedMeshes.Add(builder.ToMesh(true));
        //    }
        //    else
        //    {
        //        rd.Meshes.Add(builder.ToMesh(true));
        //    }

        //    if (node.DisplayLabels)
        //    {
        //        var cp = (a + b + c)/3;
        //        rd.Text.Add(new BillboardTextItem { Text = tag, Position = new Point3D(cp.X,cp.Y,cp.Z)});
        //    }
        //}

        ///// <summary>
        ///// Convert a Revit mesh to a Helix mesh for visualization.
        ///// In order to merge mesh vertices, this method uses a dictionary with a string key formed as x:y:z of the point.
        ///// This assumes that where vertices are the "same" in the Revit mesh, they will have the same coordinates. This
        ///// is NOT a safe strategy to use in other mesh-processing contexts where vertices might have small discrepancies.
        ///// </summary>
        ///// <param name="rmesh"></param>
        ///// <param name="octree"></param>
        ///// <param name="node"></param>
        ///// <returns></returns>
        //private static MeshGeometry3D RevitMeshToHelixMesh(Mesh rmesh, Octree.OctreeSearch.Octree octree, NodeModel node)
        //{
        //    var builder = new MeshBuilder();
        //    var points = new Point3DCollection();
        //    var tex = new PointCollection();
        //    var norms = new Vector3DCollection();
        //    var tris = new List<int>();

        //    //A dictionary which will contain a point, a normal, and an index
        //    //keyed on the location of the point as a hash
        //    var pointDict = new Dictionary<string, PointData>();
        //    for (int i = 0; i < rmesh.NumTriangles; ++i)
        //    {
        //        var tri = rmesh.get_Triangle(i);
        //        //calculate the face normal by
        //        //getting the cross product of two edges
        //        var a = tri.get_Vertex(0);
        //        var b = tri.get_Vertex(1);
        //        var c = tri.get_Vertex(2);
        //        var e1 = b - a;
        //        var e2 = c - a;
        //        var normXYZ = e1.CrossProduct(e2).Normalize();
        //        var normal = new Vector3D(normXYZ.X, normXYZ.Y, normXYZ.Z);

        //        for (int j = 0; j < 3; j++)
        //        {
        //            var pt = RevitPointToWindowsPoint(tri.get_Vertex(j));
        //            var key = pt.X + ":" + pt.Y + ":" + pt.Z;
        //            if (!pointDict.ContainsKey(key))
        //            {
        //                //if the dictionary doesn't contain the key
        //                var pd = new PointData(pt.X,pt.Y,pt.Z);
        //                pd.Normals.Add(normal);
        //                pd.Index = pointDict.Count;
        //                pointDict.Add(key, pd);
        //                tris.Add(pd.Index);
        //            }
        //            else
        //            {
        //                //add an index to our tris array
        //                //add a normal to our internal collection
        //                //for post processing
        //                var data = pointDict[key];
        //                tris.Add(data.Index);
        //                data.Normals.Add(normal);
        //            }
        //        }
        //    }

        //    var lst = pointDict.ToList();
        //    lst.ForEach(x => points.Add(x.Value.Position));
        //    lst.ForEach(x=>octree.AddNode(x.Value.Position.X, x.Value.Position.Y, x.Value.Position.Z, node.GUID.ToString()));
        //    lst.ForEach(x=>tex.Add(x.Value.Tex));

        //    //merge the normals
        //    foreach (var pd in lst)
        //    {
        //        var avg = new Vector3D();
        //        var nList = pd.Value.Normals;
        //        foreach (var n in nList)
        //        {
        //            avg.X += n.X;
        //            avg.Y += n.Y;
        //            avg.Z += n.Z;
        //        }
        //        avg.X = avg.X / nList.Count;
        //        avg.Y = avg.Y / nList.Count;
        //        avg.Z = avg.Z / nList.Count;
        //        norms.Add(avg);
        //    }

        //    builder.Append(points, tris, norms, tex);
        //    Debug.WriteLine(string.Format("Mesh had {0} faces coming in and {1} faces going out.", rmesh.NumTriangles, builder.TriangleIndices.Count / 3));

        //    return builder.ToMesh(true);
        //}

        ///// <summary>
        ///// A class for storing data about a point for mesh processing.
        ///// </summary>
        //protected class PointData
        //{
        //    internal List<Vector3D> Normals { get; set; }
        //    internal int Index { get; set; }
        //    internal Point3D Position { get; set; }
        //    internal System.Windows.Point Tex { get; set; }
        //    internal PointData(double x, double y, double z)
        //    {
        //        Position = new Point3D(x,y,z);
        //        Tex = new System.Windows.Point(0, 0);
        //        Normals = new List<Vector3D>();
        //    }
        //}

        //private static Point3D RevitPointToWindowsPoint(XYZ xyz)
        //{
        //    return new Point3D(xyz.X, xyz.Y, xyz.Z);
        //}
    
    }
}
