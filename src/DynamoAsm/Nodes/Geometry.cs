using System;
using System.Windows;
using System.Collections.Generic;
using Microsoft.FSharp.Collections;
using System.Windows.Media;
using System.Windows.Media.Media3D;

using HelixToolkit.Wpf;

using Value = Dynamo.FScheme.Value;
using Dynamo.FSchemeInterop;
using Dynamo.Connectors;
using Dynamo.Utilities;
using Dynamo.Controls;

using Autodesk.LibG;
using Geometry = Autodesk.LibG.Geometry;
using Point = Autodesk.LibG.Point;
using Vector = Autodesk.LibG.Vector;

namespace Dynamo.Nodes
{
    public abstract class LibGNode : dynNodeWithOneOutput
    {
        public LibGNode()
        {
            DynamoAsm.EnsureStarted();

            ArgumentLacing = LacingStrategy.Longest;
        }

        internal Geometry RestoreProperType(Geometry g)
        {
            Point p = Point.cast(g);

            if (p != null)
                return p;

            Surface surf = Surface.cast(g);

            if (surf != null)
                return surf;

            Curve c = Curve.cast(g);

            if (c != null)
                return c;

            Solid solid = Solid.cast(g);

            if (solid != null)
                return solid;

            return g;
        }
    }

    public abstract class GraphicItemNode : LibGNode, IDrawable, IClearable
    {
        protected List<GraphicItem> _graphicItems = new List<GraphicItem>();
        public RenderDescription RenderDescription { get; set; }

        public void Draw()
        {
            if (DynamoAsm.HasShutdown)
                return;

            if (this.RenderDescription == null)
                this.RenderDescription = new RenderDescription();
            else
                this.RenderDescription.ClearAll();

            foreach (GraphicItem g in _graphicItems)
            {
                FloatList point_vertices = g.point_vertices_threadsafe();

                for (int i = 0; i < point_vertices.Count; i += 3)
                {
                    this.RenderDescription.points.Add(new Point3D(point_vertices[i],
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

                        this.RenderDescription.lines.Add(p);

                        counter += 3;

                        if (i == 0 || i == num_verts - 1)
                            continue;

                        this.RenderDescription.lines.Add(p);
                    }
                }

                FloatList triangle_vertices = g.triangle_vertices_threadsafe();

                List<int> indices_front = new List<int>();
                List<int> indices_back = new List<int>();
                List<Point3D> vertices = new List<Point3D>();

                for (int i = 0; i < triangle_vertices.Count / 9; ++i)
                {
                    for (int k = 0; k < 3; ++k)
                    {

                        int index = i * 9 + k * 3;

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

                this.RenderDescription.meshes.Add(new Mesh3D(vertices, indices_front));
                this.RenderDescription.meshes.Add(new Mesh3D(vertices, indices_back));
            }
        }

        public void ClearReferences()
        {
            foreach (GraphicItem g in _graphicItems)
            {
                GraphicItem.unpersist(g);
            }

            _graphicItems.Clear();
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Point 2D")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Create a point in the XY plane.")]
    public class Point2DNode : GraphicItemNode
    {
        private Point _point = null;

        public Point2DNode()
        {
            InPortData.Add(new PortData("X", "X", typeof(Value.Number)));
            InPortData.Add(new PortData("Y", "Y", typeof(Value.Number)));
            OutPortData.Add(new PortData("Point", "Point", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            double x, y;
            x = ((Value.Number)args[0]).Item;
            y = ((Value.Number)args[1]).Item;

            if (_point == null)
            {
                _point = Point.by_coordinates(x, y);
            }
            else
            {
                _point.set_x(x);
                _point.set_y(y);
            }

            return Value.NewContainer(_point);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Point")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Create a point in 3D space.")]
    public class Point3DNode : GraphicItemNode
    {
        private Point _point = null;

        public Point3DNode()
        {
            InPortData.Add(new PortData("X", "X", typeof(Value.Number)));
            InPortData.Add(new PortData("Y", "Y", typeof(Value.Number)));
            InPortData.Add(new PortData("Z", "Z", typeof(Value.Number)));
            OutPortData.Add(new PortData("Point", "Point", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            double x, y, z;
            x = ((Value.Number)args[0]).Item;
            y = ((Value.Number)args[1]).Item;
            z = ((Value.Number)args[2]).Item;

            _point = Point.by_coordinates(x, y, z);

            _graphicItems.Add(_point);

            return Value.NewContainer(_point);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Vector")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Create a Vector representing a direction.")]
    public class VectorNode : dynNodeWithOneOutput
    {
        private Vector _vector = null;

        public VectorNode()
        {
            InPortData.Add(new PortData("X", "X", typeof(Value.Number)));
            InPortData.Add(new PortData("Y", "Y", typeof(Value.Number)));
            InPortData.Add(new PortData("Z", "Z", typeof(Value.Number)));
            OutPortData.Add(new PortData("Vector", "Vector", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            double x, y, z;
            x = ((Value.Number)args[0]).Item;
            y = ((Value.Number)args[1]).Item;
            z = ((Value.Number)args[2]).Item;

            if (_vector == null)
            {
                _vector = Vector.by_coordinates(x, y, z);
            }
            else
            {
                _vector.set_x(x);
                _vector.set_y(y);
                _vector.set_z(z);
            }

            return Value.NewContainer(_vector);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Line")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Create a Line between two Points.")]
    public class LineNode : GraphicItemNode
    {
        private Line _line = null;

        public LineNode()
        {
            InPortData.Add(new PortData("Start", "Start Point", typeof(Value.Container)));
            InPortData.Add(new PortData("End", "End Point", typeof(Value.Container)));
            OutPortData.Add(new PortData("Line", "Line", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Point sp = (Point)((Value.Container)args[0]).Item;
            Point ep = (Point)((Value.Container)args[1]).Item;

            _line = Line.by_start_point_end_point(sp, ep);
            _graphicItems.Add(_line);

            return Value.NewContainer(_line);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("BSplineCurve")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Create a B-Spline Curve through input points.")]
    public class BSplineCurveNode : GraphicItemNode
    {
        private BSplineCurve _bsplinecurve = null;

        public BSplineCurveNode()
        {
            InPortData.Add(new PortData("Points", "Points to interpolate", typeof(Value.List)));
            OutPortData.Add(new PortData("BSplineCurve", "Output curve", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            PointList points = new PointList();
            var input = (args[0] as Value.List).Item;

            foreach (Value v in input)
            {
                Point p = ((Value.Container)v).Item as Point;
                points.Add(p);
            }          

            _bsplinecurve = BSplineCurve.by_points(points);
            _graphicItems.Add(_bsplinecurve);

            return Value.NewContainer(_bsplinecurve);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Extrude Curve")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Extrude a curve in a direction.")]
    public class ExtrudeNode : GraphicItemNode
    {
        Surface _surface = null;

        public ExtrudeNode()
        {
            InPortData.Add(new PortData("Curve", "Curve to extrude", typeof(Value.Container)));
            InPortData.Add(new PortData("Direction", "Direction Vector", typeof(Value.Container)));
            InPortData.Add(new PortData("Distance", "Distance amount", typeof(Value.Number)));
            OutPortData.Add(new PortData("Surface", "Extruded Surface", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Curve curve = (Curve)((Value.Container)args[0]).Item;
            Vector dir = (Vector)((Value.Container)args[1]).Item;
            double dist = ((Value.Number)args[2]).Item;

            _surface = curve.extrude(dir, dist);
            GraphicItem.persist(_surface);

            _graphicItems.Add(_surface);

            return Value.NewContainer(_surface);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Intersect")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Intersect two pieces of Geometry.")]
    public class IntersectNode : GraphicItemNode
    {
        List<Geometry> _result = new List<Geometry>();

        public IntersectNode()
        {
            InPortData.Add(new PortData("Geometry", "First Geometry object to intersect", typeof(Value.Container)));
            InPortData.Add(new PortData("Geometry", "Second Geometry object to intersect", typeof(Value.Container)));

            OutPortData.Add(new PortData("Geometry", "Outcome", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Geometry geom1 = (Geometry)((Value.Container)args[0]).Item;
            Geometry geom2 = (Geometry)((Value.Container)args[1]).Item;

            GeometryList result = geom1.intersect(geom2);

            foreach (Geometry g in _result)
            {
                GraphicItem.unpersist(g);
            }

            _result.Clear();

            foreach (Geometry g in result)
            {
                Geometry restored = RestoreProperType(g);

                GraphicItem.persist(restored);
                _result.Add(restored);
                _graphicItems.Add(restored);
            }

            if (_result.Count == 1)
                return Value.NewContainer(_result[0]);
            else
            {
                List<Value> return_values = new List<Value>();

                foreach (Geometry g in _result)
                {
                    return_values.Add(Value.NewContainer(g));
                }

                return Value.NewList(Utils.SequenceToFSharpList(return_values));
            }
        }
    }
}