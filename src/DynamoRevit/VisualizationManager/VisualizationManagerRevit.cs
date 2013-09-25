using System;
using System.Collections.Generic;
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
        public override void Draw()
        {
            foreach (KeyValuePair<string, List<object>>geoms in Visualizations)
            {
                foreach (var obj in geoms.Value)
                {
                    if (obj is Element)
                    {
                        DrawElement(obj);
                    }
                    if (obj is Transform)
                    {
                        DrawTransform(obj);
                    }
                    else if (obj is XYZ)
                    {
                        DrawXyz(obj);
                    }
                    else if (obj is ParticleSystem)
                    {
                        DrawParticleSystem(obj);
                    }
                    else if (obj is TriangleFace)
                    {
                        DrawTriangleFace(obj);
                    }
                    else if (obj is GeometryObject)
                    {
                        //Draw Revit geometry
                        DrawGeometryObject(obj);

                        //If GeometryKeeper is available,
                        //send geometry.
                        //TODO: GeometryKeeper
                    }
                }
            }
            
            base.Draw();
        }

        private void DrawElement(object obj)
        {
            if (obj == null)
                return;

            if (obj is CurveElement)
            {
                DrawCurveElement(obj);
            }
            else if (obj is ReferencePoint)
            {
                DrawReferencePoint(obj);
            }
            else if (obj is Form)
            {
                DrawForm(obj);
            }
            else if (obj is GeometryElement)
            {
                DrawGeometryElement(obj);
            }
            else if (obj is GeometryObject)
            {
                DrawGeometryObject(obj);
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
                        DrawGeometryObject(geom);
                    }
                }
            }
        }

        private void DrawFace(object obj)
        {
            var face = obj as Face;

            if (face == null)
                return;

            Mesh3D[] meshes = RevitMeshToHelixMesh(face.Triangulate(0.2));

            foreach (Mesh3D mesh in meshes)
            {
                Meshes.Add(mesh);
            }
        }

        private void DrawForm(object obj)
        {
            var form = obj as Form;

            if (form == null)
                return;

            DrawGeometryElement(form.get_Geometry(new Options()));
        }

        private void DrawGeometryElement(object obj)
        {
            try
            {
                var gelem = obj as GeometryElement;

                foreach (GeometryObject go in gelem)
                {
                    DrawGeometryObject(go);
                }
            }
            catch (Exception ex)
            {
                DynamoLogger.Instance.Log(ex.Message);
                DynamoLogger.Instance.Log(ex.StackTrace);
            }

        }

        private void DrawGeometryObject(object obj)
        {
            if (obj == null)
                return;

            if (obj is XYZ)
            {
                DrawXyz(obj);
            }
            if (obj is Curve)
            {
                DrawCurve(obj);
            }
            else if (obj is Solid)
            {
                DrawSolid(obj);
            }
            else if (obj is Face)
            {
                DrawFace(obj);
            }
            else
            {
                DrawUndrawable(obj);
            }
        }

        private void DrawUndrawable(object obj)
        {
            //TODO: write a message, throw an exception, draw a question mark
        }

        private void DrawReferencePoint(object obj)
        {
            var point = obj as ReferencePoint;

            if (point == null)
                return;

            Points.Add(new Point3D(point.GetCoordinateSystem().Origin.X,
                point.GetCoordinateSystem().Origin.Y,
                point.GetCoordinateSystem().Origin.Z));
        }

        private void DrawXyz(object obj)
        {
            var point = obj as XYZ;
            if (point == null)
                return;

            Points.Add(new Point3D(point.X, point.Y, point.Z));
        }

        private void DrawCurve(object obj)
        {
            var curve = obj as Curve;

            if (curve == null)
                return;

            IList<XYZ> points = curve.Tessellate();

            for (int i = 0; i < points.Count; ++i)
            {
                XYZ xyz = points[i];

                Lines.Add(new Point3D(xyz.X, xyz.Y, xyz.Z));

                if (i == 0 || i == (points.Count - 1))
                    continue;

                Lines.Add(new Point3D(xyz.X, xyz.Y, xyz.Z));
            }
        }

        private void DrawCurveElement(object obj)
        {
            var elem = obj as CurveElement;

            if (elem == null)
                return;

            DrawCurve(elem.GeometryCurve);
        }

        private void DrawSolid(object obj)
        {
            var solid = obj as Solid;

            if (solid == null)
                return;

            foreach (Face f in solid.Faces)
            {
                DrawFace(f);
            }

            foreach (Edge edge in solid.Edges)
            {
                DrawCurve(edge.AsCurve());
            }
        }

        private void DrawTransform(object obj)
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

            XAxisPoints.Add(origin);
            XAxisPoints.Add(xEnd);

            YAxisPoints.Add(origin);
            YAxisPoints.Add(yEnd);

            ZAxisPoints.Add(origin);
            ZAxisPoints.Add(zEnd);
        }

        private void DrawTriangleFace(object obj)
        {
            var face = obj as TriangleFace;
            if (face == null)
                return;

            var mesh = new Mesh3D();
            mesh.Vertices.Add(face.Vertices[1].ToPoint3D());
            mesh.Vertices.Add(face.Vertices[0].ToPoint3D());
            mesh.Vertices.Add(face.Vertices[2].ToPoint3D());
            mesh.AddFace(new int[3] { mesh.Vertices.Count - 1, mesh.Vertices.Count - 2, mesh.Vertices.Count - 3 });
        }

        private void DrawParticleSystem(object obj)
        {
            var ps = obj as ParticleSystem;
            if (ps == null)
                return;

            for (int i = 0; i < ps.numberOfParticles(); i++)
            {
                Particle p = ps.getParticle(i);
                XYZ pos = p.getPosition();
                if (i < Points.Count)
                {
                    Points[i] = new Point3D(pos.X, pos.Y, pos.Z);
                }
                else
                {
                    var pt = new Point3D(pos.X, pos.Y, pos.Z);
                    Points.Add(pt);
                }
            }

            for (int i = 0; i < ps.numberOfSprings(); i++)
            {
                ParticleSpring spring = ps.getSpring(i);
                XYZ pos1 = spring.getOneEnd().getPosition();
                XYZ pos2 = spring.getTheOtherEnd().getPosition();

                if (i * 2 + 1 < Lines.Count)
                {
                    Lines[i * 2] = new Point3D(pos1.X, pos1.Y, pos1.Z);
                    Lines[i * 2 + 1] = new Point3D(pos2.X, pos2.Y, pos2.Z);
                }
                else
                {
                    var pt1 = new Point3D(pos1.X, pos1.Y, pos1.Z);
                    var pt2 = new Point3D(pos2.X, pos2.Y, pos2.Z);

                    Lines.Add(pt1);
                    Lines.Add(pt2);
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
