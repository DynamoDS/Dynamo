using System;
using System.Collections.Generic;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using Dynamo.FSchemeInterop;
using Autodesk.LibG;
using Geometry = Autodesk.LibG.Geometry;
using Point = Autodesk.LibG.Point;
using Vector = Autodesk.LibG.Vector;
using Polygon = Autodesk.LibG.Polygon;

namespace Dynamo.Nodes
{
    public abstract class LibGNode : NodeWithOneOutput
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

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Point 2D")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_PRIMITIVES)]
    [NodeDescription("Create a point in the XY plane.")]
    [NodeSearchable(false)]
    public class Point2DNode : LibGNode
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

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Point")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_PRIMITIVES)]
    [NodeDescription("Create a point in 3D space.")]
    [NodeSearchable(false)]
    public class Point3DNode : LibGNode
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

            return Value.NewContainer(_point);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Point X")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_PRIMITIVES)]
    [NodeDescription("Extract the X value of a Point.")]
    [NodeSearchable(false)]
    public class PointXNode : LibGNode
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

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Point Y")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_PRIMITIVES)]
    [NodeDescription("Extract the Y value of a Point.")]
    [NodeSearchable(false)]
    public class PointYNode : LibGNode
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

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Point Z")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_PRIMITIVES)]
    [NodeDescription("Extract the Z value of a Point.")]
    [NodeSearchable(false)]
    public class PointZNode : LibGNode
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

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Vector X")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_PRIMITIVES)]
    [NodeDescription("Extract the X value of a Vector.")]
    [NodeSearchable(false)]
    public class VectorXNode : LibGNode
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

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Vector Y")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_PRIMITIVES)]
    [NodeDescription("Extract the Y value of a Vector.")]
    [NodeSearchable(false)]
    public class VectorYNode : LibGNode
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

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Vector Z")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_PRIMITIVES)]
    [NodeDescription("Extract the Z value of a Vector.")]
    [NodeSearchable(false)]
    public class VectorZNode : LibGNode
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

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Vector")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_PRIMITIVES)]
    [NodeDescription("Create a Vector representing a direction.")]
    [NodeSearchable(false)]
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

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("CoordinateSystem")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_PRIMITIVES)]
    [NodeDescription("Create a CoordinateSystem.")]
    [NodeSearchable(false)]
    public class CoordinateSystemNode : LibGNode
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

            return Value.NewContainer(_cs);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Line - LibG")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_CURVE)]
    [NodeDescription("Create a Line between two Points.")]
    [NodeSearchable(false)]
    public class LineNode : LibGNode
    {
        public LineNode()
        {
            InPortData.Add(new PortData("Start", "Start Point", typeof(Value.Container)));
            InPortData.Add(new PortData("End", "End Point", typeof(Value.Container)));
            OutPortData.Add(new PortData("Line", "Line", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var sp = (Point)((Value.Container)args[0]).Item;
            var ep = (Point)((Value.Container)args[1]).Item;

            //if the line is zero length
            if (sp.distance_to(ep) < 0.00001)
            {
                return Value.NewContainer(null);
            }

            var line = Line.by_start_point_end_point(sp, ep);

            return Value.NewContainer(line);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Circle - LibG")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_CURVE)]
    [NodeDescription("Create a Circle at a Point with radius")]
    [NodeSearchTags("circle")]
    [NodeSearchable(false)]
    public class CircleNode : LibGNode
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

            return Value.NewContainer(_circle);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Sweep As Surface")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_SURFACE)]
    [NodeDescription("Sweep a Crve along a path producing a Surface")]
    [NodeSearchable(false)]
    public class SweepAsSurfaceNode : LibGNode
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

            return Value.NewContainer(_surface);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Sweep As Solid")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_SOLID)]
    [NodeDescription("Sweep a Crve along a path producing a Solid")]
    [NodeSearchable(false)]
    public class SweepAsSolidNode : LibGNode
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

            return Value.NewContainer(_solid);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Length - LibG")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_CURVE)]
    [NodeDescription("Get the length of a Curve.")]
    [NodeSearchable(false)]
    public class LengthNode : LibGNode
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

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Surface Area - LibG")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_SURFACE)]
    [NodeDescription("Get the area of a Surface.")]
    [NodeSearchable(false)]
    public class AreaNode : LibGNode
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

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Translate")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_MODIFY)]
    [NodeDescription("Translate Object.")]
    [NodeSearchable(false)]
    public class TranslateNode : LibGNode
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
                GraphicItem.persist(graphicItem);
            }

            return Value.NewContainer(_transformableItem);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("BSplineCurve")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_CURVE)]
    [NodeDescription("Create a B-Spline Curve through input points.")]
    [NodeSearchable(false)]
    public class BSplineCurveNode : LibGNode
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

            return Value.NewContainer(_bsplinecurve);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Closed BSplineCurve")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_CURVE)]
    [NodeDescription("Create a B-Spline Curve through input points.")]
    [NodeSearchable(false)]
    public class ClosedBSplineCurveNode : LibGNode
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

            return Value.NewContainer(_bsplinecurve);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Polygon")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_CURVE)]
    [NodeDescription("Create a Polygon from a set of points.")]
    [NodeSearchable(false)]
    public class PolygonNode : LibGNode
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

            return Value.NewContainer(_polygon);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Loft")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_SURFACE)]
    [NodeDescription("Loft Curves to create a surface")]
    [NodeSearchable(false)]
    public class LoftNode : LibGNode
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

            return Value.NewContainer(_surface);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Draw")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_VIEW)]
    [NodeDescription("Draws Geometry created in a Python or external script")]
    [NodeSearchable(false)]
    public class ForceDrawNode : LibGNode
    {
        public ForceDrawNode()
        {
            InPortData.Add(new PortData("Object", "Object to draw.", typeof(Value.Container)));
            OutPortData.Add(new PortData("Object", "Same Object", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = ((Value.Container)args[0]).Item;

            GraphicItem item = input as GraphicItem;

            return args[0];
        }
    }


    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Patch")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_SURFACE)]
    [NodeDescription("Patch Curves to create a Surface")]
    [NodeSearchable(false)]
    public class PatchNode : LibGNode
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

            return Value.NewContainer(_surface);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Extrude Curve")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_SURFACE)]
    [NodeDescription("Extrude a curve in a direction.")]
    [NodeSearchable(false)]
    public class ExtrudeNode : LibGNode
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

            return Value.NewContainer(_surface);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Cuboid")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_SOLID)]
    [NodeDescription("Cuboid is like a cube, or a shoebox.")]
    [NodeSearchable(false)]
    public class CuboidNode : LibGNode
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

            return Value.NewContainer(_solid);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Plane - LibG")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_SURFACE)]
    [NodeDescription("Plane, an infinite 2D expanse in 3D space.")]
    [NodeSearchable(false)]
    public class PlaneNode : LibGNode
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

            return Value.NewContainer(_plane);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Point At Parameter")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_CURVE)]
    [NodeDescription("Point at parameter along Curve.")]
    [NodeSearchable(false)]
    public class PointAtParameterNode : LibGNode
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

            return Value.NewContainer(_point);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Point At Distance")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_CURVE)]
    [NodeDescription("Point at distance along Curve.")]
    [NodeSearchable(false)]
    public class PointAtDistanceNode : LibGNode
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

            return Value.NewContainer(_point);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Normal At Parameter")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_CURVE)]
    [NodeDescription("Normal at Parameter along Curve.")]
    [NodeSearchable(false)]
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

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Point At UV Parameter")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_SURFACE)]
    [NodeDescription("Point at Parameter on Surface.")]
    [NodeSearchable(false)]
    public class PointAtUvParameterNode : LibGNode
    {
        Point _point = null;

        public PointAtUvParameterNode()
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

            return Value.NewContainer(_point);
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Normal At UV Parameter")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_SURFACE)]
    [NodeDescription("Normal at Parameter on Surface.")]
    [NodeSearchable(false)]
    public class NormalAtUvParameterNode : LibGNode
    {
        Vector _vector = null;

        public NormalAtUvParameterNode()
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

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Thicken Surface")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_SOLID)]
    [NodeDescription("Thicken / Extrude a Surface by an amount.")]
    [NodeSearchable(false)]
    public class ThickenSurfaceNode : LibGNode
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

            return Value.NewContainer(_solid);
        }
    }



    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Intersect")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_MODIFY)]
    [NodeDescription("Intersect two pieces of Geometry.")]
    [NodeSearchable(false)]
    public class IntersectNode : LibGNode
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

                return Value.NewList(Utils.ToFSharpList(return_values));
            }
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Trim")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_MODIFY)]
    [NodeDescription("Trim Geometry with a tool Geometry.")]
    [NodeSearchable(false)]
    public class TrimNode : LibGNode
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

            return Value.NewContainer(_result); 
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Import SAT")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_VIEW)]
    [NodeDescription("Import SAT Geometry from file.")]
    [NodeSearchable(false)]
    public class ImportSATNode : LibGNode
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
            System.String file_name = ((Value.String)args[0]).Item;

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
            }

            List<Value> return_values = new List<Value>();

            foreach (DSObject obj in _result)
            {
                return_values.Add(Value.NewContainer(obj));
            }

            return Value.NewList(Utils.ToFSharpList(return_values));
        }
    }

    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.VASARI_2013)]
    [NodeName("Export To SAT")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_EXPERIMENTAL_VIEW)]
    [NodeDescription("Export Geometry to a SAT file.")]
    [NodeSearchable(false)]
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

            System.String file_name = ((Value.String)args[0]).Item;
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