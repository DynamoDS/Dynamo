using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Autodesk.LibG;
using Dynamo.Models;
using Dynamo.Selection;
using HelixToolkit.Wpf;

namespace Dynamo
{
    public class VisualizationManagerASM : VisualizationManager
    {
        public VisualizationManagerASM()
        {
            AlternateDrawingContextAvailable = false;
            DrawToAlternateContext = false;
        }

        public static void DrawLibGGraphicItem(NodeModel node, object geom, RenderDescription rd,  Octree.OctreeSearch.Octree octree)
        {
            var selected = DynamoSelection.Instance.Selection.Contains(node);
            var g = geom as GraphicItem;

            if (g is CoordinateSystem)
            {
                #region draw coordinate systems

                var line_strip_vertices = g.line_strip_vertices_threadsafe();

                for (int i = 0; i < line_strip_vertices.Count; i += 6)
                {
                    var p1 = new Point3D(
                        line_strip_vertices[i],
                        line_strip_vertices[i + 1],
                        line_strip_vertices[i + 2]);

                    var p2 = new Point3D(
                        line_strip_vertices[i + 3],
                        line_strip_vertices[i + 4],
                        line_strip_vertices[i + 5]);

                    if (i < 6)
                    {
                        rd.XAxisPoints.Add(p1);
                        rd.XAxisPoints.Add(p2);
                    }
                    else if (i >= 6 && i < 12)
                    {
                        rd.YAxisPoints.Add(p1);
                        rd.YAxisPoints.Add(p2);
                    }
                    else
                    {
                        rd.ZAxisPoints.Add(p1);
                        rd.ZAxisPoints.Add(p2);
                    }
                }

                #endregion
            }
            else
            {
                #region draw points

                var point_vertices = g.point_vertices_threadsafe();

                for (int i = 0; i < point_vertices.Count; i += 3)
                {
                    if (selected)
                    {
                        rd.SelectedPoints.Add(new Point3D(point_vertices[i],
                                                          point_vertices[i + 1], point_vertices[i + 2]));
                    }
                    else
                    {
                        rd.Points.Add(new Point3D(point_vertices[i],
                                                  point_vertices[i + 1], point_vertices[i + 2]));
                    }
                }

                #endregion

                #region draw lines

                SizeTList num_line_strip_vertices = g.num_line_strip_vertices_threadsafe();
                FloatList line_strip_vertices = g.line_strip_vertices_threadsafe();

                int counter = 0;

                foreach (uint num_verts in num_line_strip_vertices)
                {
                    for (int i = 0; i < num_verts; ++i)
                    {
                        var p = new Point3D(
                            line_strip_vertices[counter],
                            line_strip_vertices[counter + 1],
                            line_strip_vertices[counter + 2]);

                        if (selected)
                        {
                            rd.SelectedLines.Add(p);
                        }
                        else
                        {
                            rd.Lines.Add(p);
                        }

                        counter += 3;

                        if (i == 0 || i == num_verts - 1)
                            continue;

                        if (selected)
                        {
                            rd.SelectedLines.Add(p);
                        }
                        else
                        {
                            rd.Lines.Add(p);
                        }
                    }
                }

                #endregion

                #region draw surface

                //var sw = new Stopwatch();
                //sw.Start();

                var builder = new MeshBuilder();
                var points = new Point3DCollection();
                var tex = new PointCollection();
                var norms = new Vector3DCollection();
                var tris = new List<int>();

                FloatList triangle_vertices = g.triangle_vertices_threadsafe();
                FloatList triangle_normals = g.triangle_normals_threadsafe();
            
                for (int i = 0; i < triangle_vertices.Count; i+=3)
                {
                    var new_point = new Point3D(triangle_vertices[i],
                                                triangle_vertices[i + 1],
                                                triangle_vertices[i + 2]);

                    var normal = new Vector3D(triangle_normals[i],
                                                triangle_normals[i + 1],
                                                triangle_normals[i + 2]);

                    //find a matching point
                    //compare the angle between the normals
                    //to discern a 'break' angle for adjacent faces
                    //int foundIndex = -1;
                    //for (int j = 0; j < points.Count; j++)
                    //{
                    //    var testPt = points[j];
                    //    var testNorm = norms[j];
                    //    var ang = Vector3D.AngleBetween(normal, testNorm);

                    //    if (new_point.X == testPt.X &&
                    //        new_point.Y == testPt.Y &&
                    //        new_point.Z == testPt.Z &&
                    //        ang > 90.0000)
                    //    {
                    //        foundIndex = j;
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
                    tex.Add(new System.Windows.Point(0,0));

                    octree.AddNode(new_point.X, new_point.Y, new_point.Z, node.GUID.ToString());
                }

                //builder.AddTriangles(points, norms, tex);
                builder.Append(points, tris, norms, tex);

                //sw.Stop();
                //Debug.WriteLine(string.Format("{0} elapsed for drawing geometry.", sw.Elapsed));

                //don't add empty meshes
                if (builder.Positions.Count > 0)
                {
                    if (selected)
                    {
                        rd.SelectedMeshes.Add(builder.ToMesh(true));
                    }
                    else
                    {
                        rd.Meshes.Add(builder.ToMesh(true));
                    }
                }

                #endregion
            }
        }
    }
}
