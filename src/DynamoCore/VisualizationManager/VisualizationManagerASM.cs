using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media.Media3D;
using Autodesk.LibG;
using Dynamo.Models;
using Dynamo.Selection;
using HelixToolkit.Wpf;

namespace Dynamo
{
    public class VisualizationManagerASM : VisualizationManager
    {
        public override void UpdateVisualizations()
        {
            //only update those nodes which have been flagged for update
            var toUpdate = Visualizations.Values.ToList().Where(x => x.RequiresUpdate == true);

            Debug.WriteLine(string.Format("{0} visualizations to update", toUpdate.Count()));
            Debug.WriteLine(string.Format("Updating visualizations on thread {0}.", System.Threading.Thread.CurrentThread.ManagedThreadId));

            var selIds =
                DynamoSelection.Instance.Selection.Where(x => x is NodeModel)
                               .Select(x => ((NodeModel) x).GUID.ToString());

            var selected = Visualizations.Where(x => selIds.Contains(x.Key)).Select(x => x.Value);

            foreach (var n in toUpdate)
            {
                var rd = n.Description;
                rd.Clear();

                foreach (var geom in n.Geometry)
                {
                    var g = geom as GraphicItem;
                    if (g == null)
                        continue;
                    
                    if (g is CoordinateSystem)
                    {
                        var line_strip_vertices = g.line_strip_vertices_threadsafe();

                        for (int i = 0; i < line_strip_vertices.Count; i+=6)
                        {
                            var p1 = new Point3D(
                                line_strip_vertices[i],
                                line_strip_vertices[i + 1],
                                line_strip_vertices[i + 2]);

                            var p2 = new Point3D(
                                line_strip_vertices[i+3],
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
                    }
                    else
                    {
                        #region draw points

                        var point_vertices = g.point_vertices_threadsafe();

                        var selArray = selected as Visualization[] ?? selected.ToArray();

                        for (int i = 0; i < point_vertices.Count; i += 3)
                        {
                            if (selArray.Contains(n))
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

                                if (selArray.Contains(n))
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

                                if (selArray.Contains(n))
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

                        var builder = new MeshBuilder();

                        FloatList triangle_vertices = g.triangle_vertices_threadsafe();
                        FloatList triangle_normals = g.triangle_normals_threadsafe();

                        for (int i = 0; i < triangle_vertices.Count / 9; ++i)
                        {
                            for (int k = 0; k < 3; ++k)
                            {

                                int index = i * 9 + k * 3;

                                var new_point = new Point3D(triangle_vertices[index],
                                                                triangle_vertices[index + 1],
                                                                triangle_vertices[index + 2]);

                                var normal = new Vector3D(triangle_normals[index],
                                                                triangle_normals[index + 1],
                                                                triangle_normals[index + 2]);

                                bool new_point_exists = false;
                                for (int l = 0; l < builder.Positions.Count; ++l)
                                {
                                    Point3D p = builder.Positions[l];
                                    if ((p.X == new_point.X) && (p.Y == new_point.Y) && (p.Z == new_point.Z))
                                    {
                                        //indices_front.Add(l);
                                        builder.TriangleIndices.Add(l);
                                        new_point_exists = true;
                                        break;
                                    }
                                }

                                if (new_point_exists)
                                    continue;

                                builder.TriangleIndices.Add(builder.Positions.Count);
                                builder.Normals.Add(normal);
                                builder.Positions.Add(new_point);
                                builder.TextureCoordinates.Add(new System.Windows.Point(0, 0));
                            }
                        }

                        //don't add empty meshes
                        if (builder.Positions.Count > 0)
                        {
                            if (selArray.Contains(n))
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

                    //set this flag to avoid processing again
                    //if not necessary
                    n.RequiresUpdate = false;
                }
            }
            
            OnVisualizationUpdateComplete(this, EventArgs.Empty);
        }
    }
}
