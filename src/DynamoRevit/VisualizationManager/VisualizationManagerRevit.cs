using System;
using System.Collections.Generic;
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
        public VisualizationManagerRevit()
        {
            if (dynSettings.Controller.Context == Context.VASARI_2014)
            {
                AlternateDrawingContextAvailable = true;
                DrawToAlternateContext = true;

                AlternateContextName = "Vasari";
            }
            else
            {
                AlternateDrawingContextAvailable = false;
            }

            Visualizers.Add(typeof(Element), DrawElement);
            Visualizers.Add(typeof(Transform), DrawTransform);
            Visualizers.Add(typeof(XYZ), DrawXyz);
            Visualizers.Add(typeof(ParticleSystem), DrawParticleSystem);
            Visualizers.Add(typeof(TriangleFace), DrawTriangleFace);
            Visualizers.Add(typeof(GeometryObject), DrawGeometryObject);
            Visualizers.Add(typeof(Autodesk.Revit.DB.CurveLoop), DrawCurveLoop);
        }

        //public override void UpdateVisualizations()
        //{
        //    //only update those nodes which have been flagged for update
        //    var toUpdate = Visualizations.Values.ToList().Where(x => x.RequiresUpdate == true);

        //    var selIds =
        //        DynamoSelection.Instance.Selection.Where(x => x is NodeModel)
        //                       .Select(x => ((NodeModel)x).GUID.ToString());

        //    var selected = Visualizations.Where(x => selIds.Contains(x.Key)).Select(x => x.Value);

        //    var sw = new Stopwatch();
        //    sw.Start();

        //    foreach (var n in toUpdate)
        //    {
        //        var rd = n.Description;

        //        //clear the render description
        //        rd.Clear();

        //        foreach (var obj in n.Geometry)
        //        {
        //            if (obj is Element)
        //            {
        //                DrawElement(obj, rd);
        //            }
        //            if (obj is Transform)
        //            {
        //                DrawTransform(obj, rd);
        //            }
        //            else if (obj is XYZ)
        //            {
        //                DrawXyz(obj, rd);
        //            }
        //            else if (obj is ParticleSystem)
        //            {
        //                DrawParticleSystem(obj, rd);
        //            }
        //            else if (obj is TriangleFace)
        //            {
        //                DrawTriangleFace(obj, rd);
        //            }
        //            else if (obj is GeometryObject)
        //            {
        //                //Draw Revit geometry
        //                DrawGeometryObject(obj, rd);
        //            }
        //            else if (obj is GraphicItem)
        //            {
        //                //support drawing ASM visuals
        //                //VisualizationManagerASM.DrawLibGGraphicItem((GraphicItem)obj, rd);
        //            }
        //        }

        //        //set this flag to avoid processing again
        //        //if not necessary
        //        n.RequiresUpdate = false;
        //    }

        //    sw.Stop();

        //    //generate an aggregated render description to send to the UI
        //    var aggRd = AggregateRenderDescriptions();

        //    LogVisualizationUpdateData(aggRd, sw.Elapsed.ToString());

        //    OnVisualizationUpdateComplete(this, new VisualizationEventArgs(AggregateRenderDescriptions()));
        //}

        private void DrawElement(NodeModel node, object obj, RenderDescription rd)
        {
            if (obj == null)
                return;

            if (obj is CurveElement)
            {
                DrawCurveElement(node, obj, rd);
            }
            else if (obj is ReferencePoint)
            {
                DrawReferencePoint(node, obj, rd);
            }
            else if (obj is Form)
            {
                DrawForm(node, obj, rd);
            }
            else if (obj is GeometryElement)
            {
                DrawGeometryElement(node, obj, rd);
            }
            else if (obj is GeometryObject)
            {
                DrawGeometryObject(node, obj, rd);
            }
            else
            {
                var elem = obj as Element;
                if (elem != null)
                {
                    var o = new Options { DetailLevel = ViewDetailLevel.Medium };
                    GeometryElement geom = elem.get_Geometry(o);

                    if (geom != null)
                    {
                        DrawGeometryObject(node, geom, rd);
                    }
                }
            }
        }

        private void DrawFace(NodeModel node, object obj, RenderDescription rd)
        {
            var face = obj as Face;

            if (face == null)
                return;

            if (node.IsSelected)
            {
                rd.SelectedMeshes.Add(RevitMeshToHelixMesh(face.Triangulate(0.1)));
            }
            else
            {
                rd.Meshes.Add(RevitMeshToHelixMesh(face.Triangulate(0.1)));
            }
        }

        private void DrawForm(NodeModel node, object obj, RenderDescription rd)
        {
            var form = obj as Form;

            if (form == null)
                return;

            DrawGeometryElement(node, form.get_Geometry(new Options()), rd);
        }

        private void DrawGeometryElement(NodeModel node, object obj, RenderDescription rd)
        {
            try
            {
                var gelem = obj as GeometryElement;

                foreach (GeometryObject go in gelem)
                {
                    DrawGeometryObject(node, go, rd);
                }
            }
            catch (Exception ex)
            {
                DynamoLogger.Instance.Log(ex.Message);
                DynamoLogger.Instance.Log(ex.StackTrace);
            }

        }

        private void DrawGeometryObject(NodeModel node, object obj, RenderDescription rd)
        {
            if (obj == null)
                return;

            if (obj is XYZ)
            {
                DrawXyz(node, obj, rd);
            }
            if (obj is Curve)
            {
                DrawCurve(node, obj, rd);
            }
            else if (obj is Solid)
            {
                DrawSolid(node, obj, rd);
            }
            else if (obj is Face)
            {
                DrawFace(node, obj, rd);
            }
            else
            {
                DrawUndrawable(node, obj, rd);
            }
        }

        private void DrawUndrawable(NodeModel node, object obj, RenderDescription rd)
        {
            //TODO: write a message, throw an exception, draw a question mark
        }

        private void DrawReferencePoint(NodeModel node, object obj, RenderDescription rd)
        {
            var point = obj as ReferencePoint;

            if (point == null)
                return;

            if (node.IsSelected)
            {
                rd.SelectedPoints.Add(new Point3D(point.GetCoordinateSystem().Origin.X,
                point.GetCoordinateSystem().Origin.Y,
                point.GetCoordinateSystem().Origin.Z));
            }
            else
            {
                rd.Points.Add(new Point3D(point.GetCoordinateSystem().Origin.X,
                point.GetCoordinateSystem().Origin.Y,
                point.GetCoordinateSystem().Origin.Z));
            }
        }

        private void DrawXyz(NodeModel node, object obj, RenderDescription rd)
        {
            var point = obj as XYZ;
            if (point == null)
                return;

            if(node.IsSelected)
                rd.SelectedPoints.Add(new Point3D(point.X, point.Y, point.Z));
            else
                rd.Points.Add(new Point3D(point.X, point.Y, point.Z));
        }

        private void DrawCurve(NodeModel node, object obj, RenderDescription rd)
        {
            var curve = obj as Curve;

            if (curve == null)
                return;

            IList<XYZ> points = curve.Tessellate();

            bool selected = node.IsSelected;

            for (int i = 0; i < points.Count; ++i)
            {
                XYZ xyz = points[i];

                rd.Lines.Add(new Point3D(xyz.X, xyz.Y, xyz.Z));

                if (i == 0 || i == (points.Count - 1))
                    continue;

                if (selected)
                {
                    rd.SelectedLines.Add(new Point3D(xyz.X, xyz.Y, xyz.Z));
                }
                else
                {
                    rd.Lines.Add(new Point3D(xyz.X, xyz.Y, xyz.Z));
                }
            }
        }

        private void DrawCurveElement(NodeModel node, object obj, RenderDescription rd)
        {
            var elem = obj as CurveElement;

            if (elem == null)
                return;

            DrawCurve(node, elem.GeometryCurve, rd);
        }

        private void DrawSolid(NodeModel node, object obj, RenderDescription rd)
        {
            var solid = obj as Solid;

            if (solid == null)
                return;

            foreach (Face f in solid.Faces)
            {
                DrawFace(node, f, rd);
            }

            foreach (Edge edge in solid.Edges)
            {
                DrawCurve(node, edge.AsCurve(), rd);
            }
        }

        private void DrawTransform(NodeModel node, object obj, RenderDescription rd)
        {
            var t = obj as Transform;
            if (t == null)
                return;

            var origin = new Point3D(t.Origin.X, t.Origin.Y, t.Origin.Z);
            XYZ x1 = t.Origin + t.BasisX.Multiply(3);
            XYZ y1 = t.Origin + t.BasisY.Multiply(3);
            XYZ z1 = t.Origin + t.BasisZ.Multiply(3);
            var xEnd = new Point3D(x1.X, x1.Y, x1.Z);
            var yEnd = new Point3D(y1.X, y1.Y, y1.Z);
            var zEnd = new Point3D(z1.X, z1.Y, z1.Z);

            rd.XAxisPoints.Add(origin);
            rd.XAxisPoints.Add(xEnd);

            rd.YAxisPoints.Add(origin);
            rd.YAxisPoints.Add(yEnd);

            rd.ZAxisPoints.Add(origin);
            rd.ZAxisPoints.Add(zEnd);
        }

        private void DrawTriangleFace(NodeModel node, object obj, RenderDescription rd)
        {
            var face = obj as TriangleFace;
            if (face == null)
                return;

            var builder = new MeshBuilder();

            builder.Positions.Add(face.Vertices[1].ToPoint3D());
            builder.Positions.Add(face.Vertices[0].ToPoint3D());
            builder.Positions.Add(face.Vertices[2].ToPoint3D());
            builder.TriangleIndices.Add(builder.Positions.Count-1);
            builder.TriangleIndices.Add(builder.Positions.Count - 2);
            builder.TriangleIndices.Add(builder.Positions.Count-3);
            builder.TextureCoordinates.Add(new System.Windows.Point(0, 0));
            builder.TextureCoordinates.Add(new System.Windows.Point(0, 0));
            builder.TextureCoordinates.Add(new System.Windows.Point(0, 0));

            if (node.IsSelected)
            {
                rd.SelectedMeshes.Add(builder.ToMesh(true));
            }
            else
            {
                rd.Meshes.Add(builder.ToMesh(true));
            }
        }

        private void DrawParticleSystem(NodeModel node, object obj, RenderDescription rd)
        {
            var ps = obj as ParticleSystem;
            if (ps == null)
                return;

            for (int i = 0; i < ps.numberOfParticles(); i++)
            {
                Particle p = ps.getParticle(i);
                XYZ pos = p.getPosition();
                if (i < rd.Points.Count)
                {
                    rd.Points[i] = new Point3D(pos.X, pos.Y, pos.Z);
                }
                else
                {
                    var pt = new Point3D(pos.X, pos.Y, pos.Z);
                    rd.Points.Add(pt);
                }
            }

            for (int i = 0; i < ps.numberOfSprings(); i++)
            {
                ParticleSpring spring = ps.getSpring(i);
                XYZ pos1 = spring.getOneEnd().getPosition();
                XYZ pos2 = spring.getTheOtherEnd().getPosition();

                if (i * 2 + 1 < rd.Lines.Count)
                {
                    rd.Lines[i * 2] = new Point3D(pos1.X, pos1.Y, pos1.Z);
                    rd.Lines[i * 2 + 1] = new Point3D(pos2.X, pos2.Y, pos2.Z);
                }
                else
                {
                    var pt1 = new Point3D(pos1.X, pos1.Y, pos1.Z);
                    var pt2 = new Point3D(pos2.X, pos2.Y, pos2.Z);

                    rd.Lines.Add(pt1);
                    rd.Lines.Add(pt2);
                }
            }
        }

        private void DrawCurveLoop(NodeModel node, object obj, RenderDescription rd)
        {
            var cl = obj as Autodesk.Revit.DB.CurveLoop;
            if (cl == null)
                return;

            foreach (var crv in cl)
            {
                DrawCurve(node, crv, rd);
            }
        }

        private static MeshGeometry3D RevitMeshToHelixMesh(Mesh rmesh)
        {
            var builder = new MeshBuilder();
            var points = new Point3DCollection();
            var tex = new PointCollection();
            var norms = new Vector3DCollection();
            var tris = new List<int>();

            for (int i = 0; i < rmesh.NumTriangles; ++i)
            {
                var tri = rmesh.get_Triangle(i);

                //calculate the face normal by
                //getting the cross product of two edges
                var a = tri.get_Vertex(0);
                var b = tri.get_Vertex(1);
                var c = tri.get_Vertex(2);
                var e1 = b - a;
                var e2 = c - a;
                var normXYZ = e1.CrossProduct(e2).Normalize();
                var normal = new Vector3D(normXYZ.X, normXYZ.Y, normXYZ.Z);

                for (int j = 0; j < 3; j++)
                {
                    var new_point = RevitPointToWindowsPoint(tri.get_Vertex(j));

                    //find a matching point
                    //compare the angle between the normals
                    //to discern a 'break' angle for adjacent faces
                    //int foundIndex = -1;
                    //for (int k = 0; k < points.Count; k++)
                    //{
                    //    var testPt = points[k];
                    //    var testNorm = norms[k];
                    //    var ang = Vector3D.AngleBetween(normal, testNorm);

                    //    if (new_point.X == testPt.X &&
                    //        new_point.Y == testPt.Y &&
                    //        new_point.Z == testPt.Z &&
                    //        ang > 90.0000)
                    //    {
                    //        foundIndex = k;
                    //        //average the merged normals
                    //        norms[k] = (testNorm + normal) / 2;
                    //        break;
                    //    }
                    //}

                    //if (foundIndex != -1)
                    //{
                    //    tris.Add(foundIndex);
                    //    continue;
                    //}

                    tris.Add(points.Count);
                    points.Add(new_point);
                    norms.Add(normal);
                    tex.Add(new System.Windows.Point(0, 0));
                }

                builder.Append(points, tris, norms, tex);
            }

            return builder.ToMesh(true);
        }

        private static Point3D RevitPointToWindowsPoint(XYZ xyz)
        {
            return new Point3D(xyz.X, xyz.Y, xyz.Z);
        }
    
    }
}
