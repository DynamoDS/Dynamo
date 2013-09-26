using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Autodesk.Revit.DB;
using Dynamo.Nodes;
using HelixToolkit.Wpf;
using Curve = Autodesk.Revit.DB.Curve;
using Solid = Autodesk.Revit.DB.Solid;
using Face = Autodesk.Revit.DB.Face;
using Edge = Autodesk.Revit.DB.Edge;

namespace Dynamo
{
    class VisualizationManagerRevit : VisualizationManager
    {
        public override void UpdateVisualizations()
        {
            //only update those nodes which have been flagged for update
            //they are flagged when their eval_internal is hit

            var toUpdate = Visualizations.Values.ToList().Where(x => x.RequiresUpdate == true);

            foreach (var n in toUpdate)
            {
                var rd = n.Description;

                //clear the render description
                rd.Clear();

                foreach (var obj in n.Geometry)
                {
                    if (obj is Element)
                    {
                        DrawElement(obj, rd);
                    }
                    if (obj is Transform)
                    {
                        DrawTransform(obj, rd);
                    }
                    else if (obj is XYZ)
                    {
                        DrawXyz(obj, rd);
                    }
                    else if (obj is ParticleSystem)
                    {
                        DrawParticleSystem(obj, rd);
                    }
                    else if (obj is TriangleFace)
                    {
                        DrawTriangleFace(obj, rd);
                    }
                    else if (obj is GeometryObject)
                    {
                        //Draw Revit geometry
                        DrawGeometryObject(obj, rd);

                        //If GeometryKeeper is available,
                        //send geometry.
                        //TODO: GeometryKeeper
                    }
                }
            }

            OnVisualizationUpdateComplete(this, EventArgs.Empty);
        }

        private void DrawElement(object obj, RenderDescription rd)
        {
            if (obj == null)
                return;

            if (obj is CurveElement)
            {
                DrawCurveElement(obj, rd);
            }
            else if (obj is ReferencePoint)
            {
                DrawReferencePoint(obj, rd);
            }
            else if (obj is Form)
            {
                DrawForm(obj, rd);
            }
            else if (obj is GeometryElement)
            {
                DrawGeometryElement(obj, rd);
            }
            else if (obj is GeometryObject)
            {
                DrawGeometryObject(obj, rd);
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
                        DrawGeometryObject(geom, rd);
                    }
                }
            }
        }

        private void DrawFace(object obj, RenderDescription rd)
        {
            var face = obj as Face;

            if (face == null)
                return;

            Mesh3D[] meshes = RevitMeshToHelixMesh(face.Triangulate(0.2));

            foreach (Mesh3D mesh in meshes)
            {
                rd.Meshes.Add(mesh);
            }
        }

        private void DrawForm(object obj, RenderDescription rd)
        {
            var form = obj as Form;

            if (form == null)
                return;

            DrawGeometryElement(form.get_Geometry(new Options()), rd);
        }

        private void DrawGeometryElement(object obj, RenderDescription rd)
        {
            try
            {
                var gelem = obj as GeometryElement;

                foreach (GeometryObject go in gelem)
                {
                    DrawGeometryObject(go, rd);
                }
            }
            catch (Exception ex)
            {
                DynamoLogger.Instance.Log(ex.Message);
                DynamoLogger.Instance.Log(ex.StackTrace);
            }

        }

        private void DrawGeometryObject(object obj, RenderDescription rd)
        {
            if (obj == null)
                return;

            if (obj is XYZ)
            {
                DrawXyz(obj, rd);
            }
            if (obj is Curve)
            {
                DrawCurve(obj, rd);
            }
            else if (obj is Solid)
            {
                DrawSolid(obj, rd);
            }
            else if (obj is Face)
            {
                DrawFace(obj, rd);
            }
            else
            {
                DrawUndrawable(obj, rd);
            }
        }

        private void DrawUndrawable(object obj, RenderDescription rd)
        {
            //TODO: write a message, throw an exception, draw a question mark
        }

        private void DrawReferencePoint(object obj, RenderDescription rd)
        {
            var point = obj as ReferencePoint;

            if (point == null)
                return;

            rd.Points.Add(new Point3D(point.GetCoordinateSystem().Origin.X,
                point.GetCoordinateSystem().Origin.Y,
                point.GetCoordinateSystem().Origin.Z));
        }

        private void DrawXyz(object obj, RenderDescription rd)
        {
            var point = obj as XYZ;
            if (point == null)
                return;

            rd.Points.Add(new Point3D(point.X, point.Y, point.Z));
        }

        private void DrawCurve(object obj, RenderDescription rd)
        {
            var curve = obj as Curve;

            if (curve == null)
                return;

            IList<XYZ> points = curve.Tessellate();

            for (int i = 0; i < points.Count; ++i)
            {
                XYZ xyz = points[i];

                rd.Lines.Add(new Point3D(xyz.X, xyz.Y, xyz.Z));

                if (i == 0 || i == (points.Count - 1))
                    continue;

                rd.Lines.Add(new Point3D(xyz.X, xyz.Y, xyz.Z));
            }
        }

        private void DrawCurveElement(object obj, RenderDescription rd)
        {
            var elem = obj as CurveElement;

            if (elem == null)
                return;

            DrawCurve(elem.GeometryCurve, rd);
        }

        private void DrawSolid(object obj, RenderDescription rd)
        {
            var solid = obj as Solid;

            if (solid == null)
                return;

            foreach (Face f in solid.Faces)
            {
                DrawFace(f, rd);
            }

            foreach (Edge edge in solid.Edges)
            {
                DrawCurve(edge.AsCurve(), rd);
            }
        }

        private void DrawTransform(object obj, RenderDescription rd)
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

        private void DrawTriangleFace(object obj, RenderDescription rd)
        {
            var face = obj as TriangleFace;
            if (face == null)
                return;

            var mesh = new Mesh3D();
            mesh.Vertices.Add(face.Vertices[1].ToPoint3D());
            mesh.Vertices.Add(face.Vertices[0].ToPoint3D());
            mesh.Vertices.Add(face.Vertices[2].ToPoint3D());
            mesh.AddFace(new int[3] { mesh.Vertices.Count - 1, mesh.Vertices.Count - 2, mesh.Vertices.Count - 3 });
            rd.Meshes.Add(mesh);
        }

        private void DrawParticleSystem(object obj, RenderDescription rd)
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

        private static Mesh3D[] RevitMeshToHelixMesh(Mesh rmesh)
        {
            var indicesFront = new List<int>();
            var indicesBack = new List<int>();
            var vertices = new List<Point3D>();

            for (int i = 0; i < rmesh.NumTriangles; ++i)
            {
                MeshTriangle tri = rmesh.get_Triangle(i);

                for (int k = 0; k < 3; ++k)
                {
                    Point3D newPoint = RevitPointToWindowsPoint(tri.get_Vertex(k));

                    bool newPointExists = false;
                    for (int l = 0; l < vertices.Count; ++l)
                    {
                        Point3D p = vertices[l];
                        if ((p.X == newPoint.X) && (p.Y == newPoint.Y) && (p.Z == newPoint.Z))
                        {
                            indicesFront.Add(l);
                            newPointExists = true;
                            break;
                        }
                    }

                    if (newPointExists)
                        continue;

                    indicesFront.Add(vertices.Count);
                    vertices.Add(newPoint);
                }

                int a = indicesFront[indicesFront.Count - 3];
                int b = indicesFront[indicesFront.Count - 2];
                int c = indicesFront[indicesFront.Count - 1];

                indicesBack.Add(c);
                indicesBack.Add(b);
                indicesBack.Add(a);
            }

            var meshes = new List<Mesh3D>
            {
                new Mesh3D(vertices, indicesFront),
                new Mesh3D(vertices, indicesBack)
            };

            return meshes.ToArray();
        }

        private static Point3D RevitPointToWindowsPoint(XYZ xyz)
        {
            return new Point3D(xyz.X, xyz.Y, xyz.Z);
        }
    }
}
