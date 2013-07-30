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
using Polygon = Autodesk.LibG.Polygon;

namespace Dynamo.Nodes
{
    public abstract class LibGNode : dynNodeWithOneOutput
    {
        public LibGNode()
        {
            DynamoAsm.EnsureStarted();

            ArgumentLacing = LacingStrategy.Longest;
        }

        internal DSObject RestoreProperDSType(DSObject d)
        {
            Geometry g = Geometry.cast(d);

            if (g != null)
                return RestoreProperType(g);

            Vector v = Vector.cast(d);

            if (v != null)
                return v;

            CoordinateSystem cs = CoordinateSystem.cast(d);

            if (cs != null)
                return cs;

            return d;
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

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
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

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
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

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Point X")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Extract the X value of a Point.")]
    public class PointXNode : GraphicItemNode
    {
        public PointXNode()
        {
            InPortData.Add(new PortData("Point", "Input Point", typeof(Value.Container)));
            OutPortData.Add(new PortData("X", "X value", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Point p = (Point)((Value.Container)args[0]).Item;

            double x = p.x();

            return Value.NewNumber(x);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Point Y")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Extract the Y value of a Point.")]
    public class PointYNode : GraphicItemNode
    {
        public PointYNode()
        {
            InPortData.Add(new PortData("Point", "Input Point", typeof(Value.Container)));
            OutPortData.Add(new PortData("Y", "Y value", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Point p = (Point)((Value.Container)args[0]).Item;

            double y = p.y();

            return Value.NewNumber(y);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Point Z")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Extract the Z value of a Point.")]
    public class PointZNode : GraphicItemNode
    {
        public PointZNode()
        {
            InPortData.Add(new PortData("Point", "Input Point", typeof(Value.Container)));
            OutPortData.Add(new PortData("Z", "Z value", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Point p = (Point)((Value.Container)args[0]).Item;

            double z = p.z();

            return Value.NewNumber(z);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Vector X")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Extract the X value of a Vector.")]
    public class VectorXNode : GraphicItemNode
    {
        public VectorXNode()
        {
            InPortData.Add(new PortData("Vector", "Input Vector", typeof(Value.Container)));
            OutPortData.Add(new PortData("X", "X value", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Vector p = (Vector)((Value.Container)args[0]).Item;

            double x = p.x();

            return Value.NewNumber(x);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Vector Y")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Extract the Y value of a Vector.")]
    public class VectorYNode : GraphicItemNode
    {
        public VectorYNode()
        {
            InPortData.Add(new PortData("Vector", "Input Vector", typeof(Value.Container)));
            OutPortData.Add(new PortData("Y", "Y value", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Vector p = (Vector)((Value.Container)args[0]).Item;

            double y = p.y();

            return Value.NewNumber(y);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Vector Z")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Extract the Z value of a Vector.")]
    public class VectorZNode : GraphicItemNode
    {
        public VectorZNode()
        {
            InPortData.Add(new PortData("Vector", "Input Vector", typeof(Value.Container)));
            OutPortData.Add(new PortData("Z", "Z value", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Vector p = (Vector)((Value.Container)args[0]).Item;

            double z = p.z();

            return Value.NewNumber(z);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Vector")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Create a Vector representing a direction.")]
    public class VectorNode : LibGNode
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

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("CoordinateSystem")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Create a CoordinateSystem.")]
    public class CoordinateSystemNode : GraphicItemNode
    {
        private CoordinateSystem _cs = null;

        public CoordinateSystemNode()
        {
            InPortData.Add(new PortData("Origin", "Origin Point", typeof(Value.Container)));
            InPortData.Add(new PortData("X Axis", "X Axis Vector", typeof(Value.Container)));
            InPortData.Add(new PortData("Y Axis", "Y Axis Vector", typeof(Value.Container)));
            OutPortData.Add(new PortData("CoordinateSystem", "CoordinateSystem", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Point origin = (Point)((Value.Container)args[0]).Item;
            Vector x_axis = (Vector)((Value.Container)args[1]).Item;
            Vector y_axis = (Vector)((Value.Container)args[2]).Item;

            _cs = CoordinateSystem.by_origin_vectors(origin, x_axis, y_axis);
            _graphicItems.Add(_cs);

            return Value.NewContainer(_cs);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
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

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Circle")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Create a Circle at a Point with radius")]
    [NodeSearchTags("circle")]
    public class CircleNode : GraphicItemNode
    {
        private Circle _circle = null;

        public CircleNode()
        {
            InPortData.Add(new PortData("Center", "Center Point", typeof(Value.Container)));
            InPortData.Add(new PortData("Radius", "Radius", typeof(Value.Number)));
            InPortData.Add(new PortData("Normal", "Normal Vector", typeof(Value.Container)));
            OutPortData.Add(new PortData("Circle", "Circle", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Point cp = (Point)((Value.Container)args[0]).Item;
            double r = ((Value.Number)args[1]).Item;
            Vector normal = (Vector)((Value.Container)args[2]).Item;

            _circle = Circle.by_center_point_radius_normal(cp, r, normal);

            _graphicItems.Add(_circle);

            return Value.NewContainer(_circle);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Sweep As Surface")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Sweep a Crve along a path producing a Surface")]
    public class SweepAsSurfaceNode : GraphicItemNode
    {
        private Surface _surface = null;

        public SweepAsSurfaceNode()
        {
            InPortData.Add(new PortData("Path", "Path Curve", typeof(Value.Container)));
            InPortData.Add(new PortData("Cross Section", "Cross Section Curve", typeof(Value.Container)));
            OutPortData.Add(new PortData("Surface", "Swept Surface", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Curve pathCurve = (Curve)((Value.Container)args[0]).Item;
            Curve crossSection = (Curve)((Value.Container)args[1]).Item;

            _surface = crossSection.sweep_as_surface(pathCurve);
            GraphicItem.persist(_surface);
            _graphicItems.Add(_surface);

            return Value.NewContainer(_surface);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Sweep As Solid")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Sweep a Crve along a path producing a Solid")]
    public class SweepAsSolidNode : GraphicItemNode
    {
        private Solid _solid = null;

        public SweepAsSolidNode()
        {
            InPortData.Add(new PortData("Path", "Path Curve", typeof(Value.Container)));
            InPortData.Add(new PortData("Cross Section", "Cross Section Curve", typeof(Value.Container)));
            OutPortData.Add(new PortData("Solid", "Swept Solid", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Curve pathCurve = (Curve)((Value.Container)args[0]).Item;
            Curve crossSection = (Curve)((Value.Container)args[1]).Item;

            _solid = crossSection.sweep_as_solid(pathCurve);
            GraphicItem.persist(_solid);
            _graphicItems.Add(_solid);

            return Value.NewContainer(_solid);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Length")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Get the length of a Curve.")]
    public class LengthNode : GraphicItemNode
    {
        public LengthNode()
        {
            InPortData.Add(new PortData("Curve", "Input Curve", typeof(Value.Container)));
            OutPortData.Add(new PortData("Length", "Length, not in a unit", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Curve c = (Curve)((Value.Container)args[0]).Item;

            double l = c.length();

            return Value.NewNumber(l);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Area")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Get the area of a Surface.")]
    public class AreaNode : GraphicItemNode
    {
        public AreaNode()
        {
            InPortData.Add(new PortData("Surface", "Input Surface", typeof(Value.Container)));
            OutPortData.Add(new PortData("Area", "Surface area", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Surface s = (Surface)((Value.Container)args[0]).Item;

            double a = s.area();

            return Value.NewNumber(a);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Translate")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Translate Object.")]
    public class TranslateNode : GraphicItemNode
    {
        TransformableItem _transformableItem;
        public TranslateNode()
        {
            InPortData.Add(new PortData("Object", "Input Object", typeof(Value.Container)));
            InPortData.Add(new PortData("Vector", "Translation Vector", typeof(Value.Container)));
            OutPortData.Add(new PortData("Object", "Translated Object", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            DSObject item = (DSObject)((Value.Container)args[0]).Item;
            Vector v = (Vector)((Value.Container)args[1]).Item;

            DSObject cloned = item.clone();

            _transformableItem = RestoreProperDSType(cloned) as TransformableItem;

            // TODO: throw exception if not transformable item

            _transformableItem.translate(v.x(), v.y(), v.z());

            GraphicItem graphicItem = _transformableItem as GraphicItem;

            if (graphicItem != null)
            {
                _graphicItems.Add(graphicItem);
                GraphicItem.persist(graphicItem);
            }

            return Value.NewContainer(_transformableItem);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
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

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Closed BSplineCurve")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Create a B-Spline Curve through input points.")]
    public class ClosedBSplineCurveNode : GraphicItemNode
    {
        private BSplineCurve _bsplinecurve = null;

        public ClosedBSplineCurveNode()
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

            _bsplinecurve = BSplineCurve.by_points(points, true);
            _graphicItems.Add(_bsplinecurve);

            return Value.NewContainer(_bsplinecurve);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Polygon")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Create a Polygon from a set of points.")]
    public class PolygonNode : GraphicItemNode
    {
        private Polygon _polygon = null;

        public PolygonNode()
        {
            InPortData.Add(new PortData("Points", "Boundary Points", typeof(Value.List)));
            OutPortData.Add(new PortData("Polygon", "Output Polygon", typeof(Value.Container)));

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

            _polygon = Polygon.by_vertices(points);
            _graphicItems.Add(_polygon);

            return Value.NewContainer(_polygon);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Loft")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Loft Curves to create a surface")]
    public class LoftNode : GraphicItemNode
    {
        private Surface _surface = null;

        public LoftNode()
        {
            InPortData.Add(new PortData("Curves", "Cross section Curves", typeof(Value.List)));
            OutPortData.Add(new PortData("Loft", "Lofted Surface", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            CurveList curves = new CurveList();
            var input = (args[0] as Value.List).Item;

            foreach (Value v in input)
            {
                Curve c = ((Value.Container)v).Item as Curve;
                curves.Add(c);
            }

            _surface = Surface.loft_by_cross_sections(curves);
            _graphicItems.Add(_surface);

            return Value.NewContainer(_surface);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Draw")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Draws Geometry created in a Python or external script")]
    public class ForceDrawNode : GraphicItemNode
    {
        public ForceDrawNode()
        {
            InPortData.Add(new PortData("Objects", "List of Objects to draw.", typeof(Value.List)));
            OutPortData.Add(new PortData("Objects", "List of same Objects", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = (args[0] as Value.List).Item;

            foreach (Value v in input)
            {
                GraphicItem item = ((Value.Container)v).Item as GraphicItem;

                if (item == null)
                    continue;

                _graphicItems.Add(item);
            }

            return args[0];
        }
    }


    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Patch")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Patch Curves to create a Surface")]
    public class PatchNode : GraphicItemNode
    {
        private Surface _surface = null;

        public PatchNode()
        {
            InPortData.Add(new PortData("Curves", "Cross section Curves", typeof(Value.List)));
            OutPortData.Add(new PortData("Patch", "Surface Patch", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            CurveList curves = new CurveList();
            var input = (args[0] as Value.List).Item;

            foreach (Value v in input)
            {
                Curve c = ((Value.Container)v).Item as Curve;
                curves.Add(c);
            }

            _surface = Surface.by_patch(curves);
            _graphicItems.Add(_surface);

            return Value.NewContainer(_surface);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
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

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Cuboid")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Cuboid is like a cube, or a shoebox.")]
    public class CuboidNode : GraphicItemNode
    {
        Solid _solid = null;

        public CuboidNode()
        {
            InPortData.Add(new PortData("CoordinateSystem", "Base CoordinateSystem", typeof(Value.Container)));
            InPortData.Add(new PortData("Width", "Cuboid Width", typeof(Value.Number)));
            InPortData.Add(new PortData("Length", "Cuboid Length", typeof(Value.Number)));
            InPortData.Add(new PortData("Height", "Cuboid Height", typeof(Value.Number)));
            OutPortData.Add(new PortData("Cuboid", "Cuboid Solid", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            CoordinateSystem cs = (CoordinateSystem)((Value.Container)args[0]).Item;
            double w = ((Value.Number)args[1]).Item;
            double l = ((Value.Number)args[2]).Item;
            double h = ((Value.Number)args[3]).Item;

            _solid = Cuboid.by_lengths(cs, w, l, h);

            _graphicItems.Add(_solid);

            return Value.NewContainer(_solid);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Plane")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Plane, an infinite 2D expanse in 3D space.")]
    public class PlaneNode : GraphicItemNode
    {
        Plane _plane = null;

        public PlaneNode()
        {
            InPortData.Add(new PortData("Origin", "Origin Point", typeof(Value.Container)));
            InPortData.Add(new PortData("Normal", "Normal Vector to Plane", typeof(Value.Container)));
            OutPortData.Add(new PortData("Plane", "Output Plane", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Point origin = (Point)((Value.Container)args[0]).Item;
            Vector normal = (Vector)((Value.Container)args[1]).Item;

            _plane = Plane.by_origin_normal(origin, normal);

            _graphicItems.Add(_plane);

            return Value.NewContainer(_plane);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Point At Parameter")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Point at parameter along Curve.")]
    public class PointAtParameterNode : GraphicItemNode
    {
        Point _point = null;

        public PointAtParameterNode()
        {
            InPortData.Add(new PortData("Curve", "Curve to evaluate", typeof(Value.Container)));
            InPortData.Add(new PortData("Parameter", "Parameter from 0 to 1", typeof(Value.Number)));
            OutPortData.Add(new PortData("Point", "Point at Parameter", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Curve curve = (Curve)((Value.Container)args[0]).Item;
            double param = ((Value.Number)args[1]).Item;

            _point = curve.point_at_parameter(param);
            GraphicItem.persist(_point);
            _graphicItems.Add(_point);

            return Value.NewContainer(_point);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Point At Distance")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Point at distance along Curve.")]
    public class PointAtDistanceNode : GraphicItemNode
    {
        Point _point = null;

        public PointAtDistanceNode()
        {
            InPortData.Add(new PortData("Curve", "Curve to evaluate", typeof(Value.Container)));
            InPortData.Add(new PortData("Distance", "Distance along curve", typeof(Value.Number)));
            OutPortData.Add(new PortData("Point", "Point at distance", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Curve curve = (Curve)((Value.Container)args[0]).Item;
            double dist = ((Value.Number)args[1]).Item;

            _point = curve.point_at_distance(dist);
            GraphicItem.persist(_point);
            _graphicItems.Add(_point);

            return Value.NewContainer(_point);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Normal At Parameter")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Normal at Parameter along Curve.")]
    public class NormalAtParameterNode : LibGNode
    {
        Vector _vector = null;

        public NormalAtParameterNode()
        {
            InPortData.Add(new PortData("Curve", "Curve to evaluate", typeof(Value.Container)));
            InPortData.Add(new PortData("Parameter", "Parameter from 0 to 1", typeof(Value.Number)));
            OutPortData.Add(new PortData("Vector", "Vector at Parameter", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Curve curve = (Curve)((Value.Container)args[0]).Item;
            double param = ((Value.Number)args[1]).Item;

            _vector = curve.normal_at_parameter(param);

            return Value.NewContainer(_vector);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Point At UV Parameter")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Point at Parameter on Surface.")]
    public class PointAtUVParameterNode : GraphicItemNode
    {
        Point _point = null;

        public PointAtUVParameterNode()
        {
            InPortData.Add(new PortData("Surface", "Surface to evaluate", typeof(Value.Container)));
            InPortData.Add(new PortData("U", "Parameter from 0 to 1", typeof(Value.Number)));
            InPortData.Add(new PortData("V", "Parameter from 0 to 1", typeof(Value.Number)));
            OutPortData.Add(new PortData("Point", "Point at Parameter", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Surface surf = (Surface)((Value.Container)args[0]).Item;
            double u = ((Value.Number)args[1]).Item;
            double v = ((Value.Number)args[2]).Item;

            _point = surf.point_at_parameter(u, v);
            GraphicItem.persist(_point);

            _graphicItems.Add(_point);

            return Value.NewContainer(_point);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Normal At UV Parameter")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Normal at Parameter on Surface.")]
    public class NormalAtUVParameterNode : LibGNode
    {
        Vector _vector = null;

        public NormalAtUVParameterNode()
        {
            InPortData.Add(new PortData("Surface", "Surface to evaluate", typeof(Value.Container)));
            InPortData.Add(new PortData("U", "Parameter from 0 to 1", typeof(Value.Number)));
            InPortData.Add(new PortData("V", "Parameter from 0 to 1", typeof(Value.Number)));
            OutPortData.Add(new PortData("Vector", "Point at Parameter", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Surface surf = (Surface)((Value.Container)args[0]).Item;
            double u = ((Value.Number)args[1]).Item;
            double v = ((Value.Number)args[2]).Item;

            _vector = surf.normal_at_parameter(u, v);

            return Value.NewContainer(_vector);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Thicken Surface")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Thicken / Extrude a Surface by an amount.")]
    public class ThickenSurfaceNode : GraphicItemNode
    {
        Solid _solid = null;

        public ThickenSurfaceNode()
        {
            InPortData.Add(new PortData("Surface", "Surface to thicken", typeof(Value.Container)));
            InPortData.Add(new PortData("Thickness", "Thickness amount", typeof(Value.Number)));
            OutPortData.Add(new PortData("Solid", "Thickened Solid", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Surface surf = (Surface)((Value.Container)args[0]).Item;
            double dist = ((Value.Number)args[1]).Item;

            _solid = surf.thicken(dist);
            GraphicItem.persist(_solid);

            _graphicItems.Add(_solid);

            return Value.NewContainer(_solid);
        }
    }



    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
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

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Trim")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Trim Geometry with a tool Geometry.")]
    public class TrimNode : GraphicItemNode
    {
        Geometry _result = null;

        public TrimNode()
        {
            InPortData.Add(new PortData("Geometry", "Base Geoemtry to trim", typeof(Value.Container)));
            InPortData.Add(new PortData("Tool", "Tool Geometry", typeof(Value.Container)));
            InPortData.Add(new PortData("Pick Point", "This node returns the Geoemtry closest to the pick Point", typeof(Value.Container)));

            OutPortData.Add(new PortData("Geometry", "Outcome", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Geometry base_geometry = (Geometry)((Value.Container)args[0]).Item;
            Geometry tool_geometry = (Geometry)((Value.Container)args[1]).Item;
            Point pick_point = (Point)((Value.Container)args[2]).Item;

            Surface base_surface = base_geometry as Surface;
            Solid base_solid = base_geometry as Solid;

            Geometry result = null;

            if (base_surface != null)
                result = base_surface.trim(tool_geometry, pick_point);

            if (base_solid != null)
                result = base_solid.trim(tool_geometry, pick_point);

            // TODO: implement curve. Trim has odd interface
            _result = RestoreProperType(result);

            GraphicItem.persist(_result);
            _graphicItems.Add(_result);

            return Value.NewContainer(_result); 
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Import SAT")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Import SAT Geometry from file.")]
    public class ImportSATNode : GraphicItemNode
    {
        List<DSObject> _result = new List<DSObject>();

        public ImportSATNode()
        {
            InPortData.Add(new PortData("File Name", "File name of .SAT file", typeof(Value.String)));

            OutPortData.Add(new PortData("Geometry", "Imported Geometry", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            String file_name = ((Value.String)args[0]).Item;

            DSObjectList objects = ASMImporter.import_file(file_name);

            _result.Clear();

            foreach (DSObject obj in objects)
            {
                DSObject restored = RestoreProperDSType(obj);

                _result.Add(restored);

                GraphicItem item = restored as GraphicItem;

                if (item == null)
                    continue;

                GraphicItem.persist(item);
                _graphicItems.Add(item);
            }

            List<Value> return_values = new List<Value>();

            foreach (DSObject obj in _result)
            {
                return_values.Add(Value.NewContainer(obj));
            }

            return Value.NewList(Utils.SequenceToFSharpList(return_values));
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013, Context.VASARI_2014)]
    [NodeName("Export To SAT")]
    [NodeCategory(BuiltinNodeCategories.CORE_GEOMETRY)]
    [NodeDescription("Export Geometry to a SAT file.")]
    public class ExportSATNode : LibGNode
    {
        public ExportSATNode()
        {
            InPortData.Add(new PortData("File Name", "File name of .SAT file", typeof(Value.String)));
            InPortData.Add(new PortData("Geometry", "List of Geometry to export", typeof(Value.List)));

            OutPortData.Add(new PortData("Geometry", "Exported Geometry", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            GeometryList geometry = new GeometryList();

            String file_name = ((Value.String)args[0]).Item;
            var input = (args[1] as Value.List).Item;            

            foreach (Value v in input)
            {
                Geometry g = ((Value.Container)v).Item as Geometry;
                geometry.Add(g);
            }

            ASMExporter.export_geometry(file_name, geometry);

            return args[1];
        }
    }

}