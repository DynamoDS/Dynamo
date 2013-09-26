using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media.Media3D;
using Autodesk.LibG;
using HelixToolkit.Wpf;

namespace Dynamo
{
    class VisualizationManagerASM : VisualizationManager
    {
        public override void UpdateVisualizations()
        {
            //only update those nodes which have been flagged for update
            //they are flagged when their eval_internal is hit

            var toUpdate = Visualizations.Values.ToList().Where(x => x.RequiresUpdate == true);

            Debug.WriteLine(string.Format("{0} visualizations to update", toUpdate.Count()));
            Debug.WriteLine(string.Format("Updating visualizations on thread {0}.", System.Threading.Thread.CurrentThread.ManagedThreadId));

            foreach (var n in toUpdate)
            {
                var rd = n.Description;
                rd.Clear();

                foreach (var geom in n.Geometry)
                {
                    var g = geom as GraphicItem;
                    if (g == null)
                        continue;

                    FloatList point_vertices = g.point_vertices_threadsafe();

                    for (int i = 0; i < point_vertices.Count; i += 3)
                    {
                        rd.Points.Add(new Point3D(point_vertices[i],
                                               point_vertices[i + 1], point_vertices[i + 2]));
                    }

                    SizeTList num_line_strip_vertices = g.num_line_strip_vertices_threadsafe();
                    FloatList line_strip_vertices = g.line_strip_vertices_threadsafe();

                    int counter = 0;

                    foreach (uint num_verts in num_line_strip_vertices)
                    {
                        for (int i = 0; i < num_verts; ++i)
                        {
                            Point3D p = new Point3D(
                                line_strip_vertices[counter],
                                line_strip_vertices[counter + 1],
                                line_strip_vertices[counter + 2]);

                            rd.Lines.Add(p);

                            counter += 3;

                            if (i == 0 || i == num_verts - 1)
                                continue;

                            rd.Lines.Add(p);
                        }
                    }

                    FloatList triangle_vertices = g.triangle_vertices_threadsafe();

                    List<int> indices_front = new List<int>();
                    List<int> indices_back = new List<int>();
                    List<Point3D> vertices = new List<Point3D>();

                    for (int i = 0; i < triangle_vertices.Count/9; ++i)
                    {
                        for (int k = 0; k < 3; ++k)
                        {

                            int index = i*9 + k*3;

                            Point3D new_point = new Point3D(triangle_vertices[index],
                                                            triangle_vertices[index + 1],
                                                            triangle_vertices[index + 2]);

                            bool new_point_exists = false;
                            for (int l = 0; l < vertices.Count; ++l)
                            {
                                Point3D p = vertices[l];
                                if ((p.X == new_point.X) && (p.Y == new_point.Y) && (p.Z == new_point.Z))
                                {
                                    indices_front.Add(l);
                                    new_point_exists = true;
                                    break;
                                }
                            }

                            if (new_point_exists)
                                continue;

                            indices_front.Add(vertices.Count);
                            vertices.Add(new_point);
                        }

                        int a = indices_front[indices_front.Count - 3];
                        int b = indices_front[indices_front.Count - 2];
                        int c = indices_front[indices_front.Count - 1];

                        indices_back.Add(c);
                        indices_back.Add(b);
                        indices_back.Add(a);
                    }

                    rd.Meshes.Add(new Mesh3D(vertices, indices_front));
                    rd.Meshes.Add(new Mesh3D(vertices, indices_back));
                }
            }

            
            OnVisualizationUpdateComplete(this, EventArgs.Empty);
        }
    }
}
