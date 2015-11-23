using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Visualization;
using Dynamo.Wpf.ViewModels.Watch3D;

namespace Dynamo.Manipulation
{
    /// <summary>
    /// Interface to define visuals of a manipulator, called Gizmo.
    /// </summary>
    public interface IGizmo
    {
        /// <summary>
        /// Unique name of the Gizmo
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Base position of the Gizmo
        /// </summary>
        Point Origin { get; }

        /// <summary>
        /// Performs hit test based on view projection ray and returns the
        /// object which is hit. The hit object could be an axis, plane or
        /// rotational arc etc.
        /// </summary>
        /// <param name="source">Source of the view projection ray.</param>
        /// <param name="direction">View projection direction.</param>
        /// <param name="hitObject">The object hit by the input ray.</param>
        /// <returns>True when an object is hit, else false.</returns>
        bool HitTest(Point source, Vector direction, out object hitObject);

        /// <summary>
        /// Computes new offset based on the hit point.
        /// </summary>
        /// <param name="newPosition">New position of mouse click.</param>
        /// <param name="viewDirection">view projection direction</param>
        /// <returns>Offset vector of hit point wrt it's origin.</returns>
        Vector GetOffset(Point newPosition, Vector viewDirection);

        /// <summary>
        /// Gets render package for all the drawables of this Gizmo.
        /// </summary>
        /// <param name="factory">Render package factory</param>
        /// <returns>List of render packages.</returns>
        IEnumerable<IRenderPackage> GetDrawables(IRenderPackageFactory factory);
    }

    /// <summary>
    /// Translation Gizmo, that handles translation.
    /// </summary>
    class TranslationGizmo : IGizmo
    {
        /// <summary>
        /// Describes an axis with it's name, direction and color.
        /// </summary>
        struct AxisDescriptor
        {
            public Vector Axis; //Axis direction
            public string Name; //Name of the axis
            public Color Color; //Color for axis
        }

        /// <summary>
        /// Defines type of planes.
        /// </summary>
        enum Planes
        {
            xyPlane,
            yzPlane,
            zxPlane,
        }

        #region Private members

        /// <summary>
        /// Origin position.
        /// </summary>
        private Point position;

        /// <summary>
        /// List of axis available for manipulation
        /// </summary>
        private List<AxisDescriptor> axes = new List<AxisDescriptor>();

        /// <summary>
        /// List of planes available for manipulation
        /// </summary>
        private List<Plane> planes = new List<Plane>();

        /// <summary>
        /// Scale to draw the gizmo
        /// </summary>
        private double scale = 1.0;

        /// <summary>
        /// Hit Data
        /// </summary>
        private Vector hitAxis = null;
        private Plane hitPlane = null;

        /// <summary>
        /// Name of the gizmo.
        /// </summary>
        private string name = "_gizmo";

        #endregion

        #region public methods and constructors

        /// <summary>
        /// Constructs a linear gizmo, can be moved in one direction only.
        /// </summary>
        /// <param name="origin">Position of the gizmo.</param>
        /// <param name="axis1">Axis of freedom</param>
        /// <param name="size">Visual size of the Gizmo</param>
        public TranslationGizmo(Point origin, Vector axis1, double size)
        {
            UpdateGeometry(origin, axis1, null, null, size);
        }

        /// <summary>
        /// Constructs planar gizmo, can be manipulated in two directions.
        /// </summary>
        /// <param name="origin">Position of the gizmo</param>
        /// <param name="axis1">First axis of freedom</param>
        /// <param name="axis2">Second axis of freedom</param>
        /// <param name="size">Visual size of the Gizmo</param>
        public TranslationGizmo(Point origin, Vector axis1, Vector axis2, double size)
        {
            UpdateGeometry(origin, axis1, axis2, null, size);
        }

        /// <summary>
        /// Construcs a 3D gizmo, can be manipulated in all three directions.
        /// </summary>
        /// <param name="origin">Position of the gizmo</param>
        /// <param name="axis1">First axis of freedom</param>
        /// <param name="axis2">Second axis of freedom</param>
        /// <param name="axis3">Third axis of freedom</param>
        /// <param name="size">Visual size of the Gizmo</param>
        public TranslationGizmo(Point origin, Vector axis1, Vector axis2, Vector axis3, double size)
        {
            UpdateGeometry(origin, axis1, axis2, axis3, size);
        }

        /// <summary>
        /// Construcs a 3D gizmo, can be manipulated in all three directions.
        /// </summary>
        /// <param name="origin">Position of the gizmo</param>
        /// <param name="axis1">First axis of freedom</param>
        /// <param name="axis2">Second axis of freedom</param>
        /// <param name="axis3">Third axis of freedom</param>
        /// <param name="size">Visual size of the Gizmo</param>
        public void UpdateGeometry(Point origin, Vector axis1, Vector axis2, Vector axis3, double size)
        {
            if (axis1 == null)
                throw new ArgumentNullException("axis1");

            //Reset the dataset, but don't reset the cached hitAxis or hitPlane.
            //hitAxis and hitPlane are used to compute the offset for a move.
            axes.Clear();
            planes.Clear();
            
            scale = size;
            position = origin;
            var col = Convert.ToByte(255);
            
            axes.Add(new AxisDescriptor() { Axis = axis1, Color = Color.FromRgb(col, 0, 0), Name = "_xAxis" });
            if (axis2 != null)
            {
                axes.Add(new AxisDescriptor() { Axis = axis2, Color = Color.FromRgb(0, col, 0), Name = "_yAxis" });
                planes.Add(Plane.ByOriginXAxisYAxis(position, axis1, axis2));
            }
            if (axis3 != null)
            {
                axes.Add(new AxisDescriptor() { Axis = axis3, Color = Color.FromRgb(0, 0, col), Name = "_zAxis" });
                if (axis2 != null)
                    planes.Add(Plane.ByOriginXAxisYAxis(position, axis2, axis3));
                planes.Add(Plane.ByOriginXAxisYAxis(position, axis3, axis1));
            }

            if (axes.Count == 1 && hitAxis != null)
                hitAxis = axes.First().Axis;
        }

        #endregion

        #region private methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        private object HitTest(Point source, Vector direction)
        {
            double tolerance = 0.15; //Hit tolerance
            
            using (var ray = GetRayGeometry(source, direction))
            {
                //First hit test for position
                if (ray.DistanceTo(position) < tolerance)
                {
                    if (planes.Any())
                        return planes.First();//Xy or first available plane is hit
                    
                    return axes.First().Axis; //xAxis or first axis is hit
                }

                foreach (var plane in planes)
                {
                    var pt = plane.Intersect(ray).FirstOrDefault() as Point;
                    if (pt != null)
                    {
                        var vec = Vector.ByTwoPoints(position, pt);
                        var dot1 = plane.XAxis.Dot(vec);
                        var dot2 = plane.YAxis.Dot(vec);
                        if( dot1 > 0 && dot2 > 0 && dot1 < scale/2 && dot2 < scale/2)
                            return plane; //specific plane is hit
                    }
                }

                foreach (var a in axes)
                {
                    using (var line = Line.ByStartPointDirectionLength(position, a.Axis, scale))
                    {
                        if (line.DistanceTo(ray) < tolerance)
                            return a.Axis; //specific axis is hit.
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        private Line GetRayGeometry(Point source, Vector direction)
        {
            double size = position.DistanceTo(source) * 100;
            return Line.ByStartPointDirectionLength(source, direction, size);
        }

        #endregion

        #region interface methods

        /// <summary>
        /// Name of the Gizmo
        /// </summary>
        public string Name 
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Origin of the Gizmo
        /// </summary>
        public Point Origin
        {
            get { return position; }
            set { position = value; }
        }

        /// <summary>
        /// Performs hit test on Gizmo to find out hit object. The returned 
        /// hitObject could be either axis vector or a plane.
        /// </summary>
        /// <param name="source">Mouse click source for hit test</param>
        /// <param name="direction">View projection direction</param>
        /// <param name="hitObject">object hit</param>
        /// <returns>True if Gizmo was hit successfully</returns>
        public bool HitTest(Point source, Vector direction, out object hitObject)
        {
            hitAxis = null;
            hitPlane = null; //reset hit objects
            hitObject = HitTest(source, direction);
            hitAxis = hitObject as Vector;
            if (hitAxis == null)
                hitPlane = hitObject as Plane;

            return hitObject != null;
        }

        /// <summary>
        /// Computes move vector, based on new position of mouse and view direction.
        /// </summary>
        /// <param name="newPosition">New mouse position</param>
        /// <param name="viewDirection">view direction</param>
        /// <returns>Offset vector wrt Origin</returns>
        public Vector GetOffset(Point newPosition, Vector viewDirection)
        {
            Point hitPoint = position;
            using (var ray = GetRayGeometry(newPosition, viewDirection))
            {
                if (hitPlane != null)
                    hitPoint = hitPlane.Intersect(ray).FirstOrDefault() as Point;
                else if (hitAxis != null)
                {
                    var axis = hitAxis.Cross(viewDirection);
                    var plane = Plane.ByOriginXAxisYAxis(position, hitAxis, axis);
                    hitPoint = plane.Intersect(ray).FirstOrDefault() as Point;
                    if (null != hitPoint)
                    {
                        var projection = hitAxis.Dot(Vector.ByTwoPoints(position, hitPoint));
                        hitPoint = position.Add(hitAxis.Normalized().Scale(projection));
                    }
                    else
                    {
                        using (var axisLine = RayExtensions.ToOriginCenteredLine(position, hitAxis))
                            hitPoint = axisLine.ClosestPointTo(ray);
                    }
                }
            }
            if (hitPoint == null)
                return Vector.ByCoordinates(0, 0, 0);

            return Vector.ByTwoPoints(position, hitPoint);
        }

        /// <summary>
        /// Gets drawables to render this Gizmo
        /// </summary>
        /// <param name="factory">Render package factory</param>
        /// <returns>List of render package</returns>
        public IEnumerable<IRenderPackage> GetDrawables(IRenderPackageFactory factory)
        {
            List<IRenderPackage> drawables = new List<IRenderPackage>();
            foreach (var axis in axes)
            {
                IRenderPackage package = factory.CreateRenderPackage();
                DrawAxis(ref package, axis);
                drawables.Add(package);
            }

            var p = Planes.xyPlane;
            foreach (var plane in planes)
            {
                IRenderPackage package = factory.CreateRenderPackage();
                DrawPlane(ref package, plane, p++);
                drawables.Add(package);
            }

            if(null != hitAxis)
            {
                IRenderPackage package = factory.CreateRenderPackage();
                DrawAxisLine(ref package, hitAxis, "_xAxisLine");
                drawables.Add(package);
            }
            if(null != hitPlane)
            {
                IRenderPackage package = factory.CreateRenderPackage();
                DrawAxisLine(ref package, hitPlane.XAxis, "_xAxisLine");
                drawables.Add(package);
                
                package = factory.CreateRenderPackage();
                DrawAxisLine(ref package, hitPlane.YAxis, "_yAxisLine");
                drawables.Add(package);
            }
            return drawables;
        }

        #endregion

        #region Helper methods to draw the gizmo

        /// <summary>
        /// Draws axis line
        /// </summary>
        /// <param name="package"></param>
        /// <param name="axis"></param>
        /// <param name="name"></param>
        private void DrawAxisLine(ref IRenderPackage package, Vector axis, string name)
        {
            package.Description = RenderDescriptions.AxisLine + Name + name;
            using (var line = RayExtensions.ToOriginCenteredLine(Origin, axis))
            {
                package.AddLineStripVertexCount(2);
                package.AddLineStripVertexColor(100, 100, 100, 255);
                package.AddLineStripVertex(line.StartPoint.X, line.StartPoint.Y, line.StartPoint.Z);
                package.AddLineStripVertexColor(100, 100, 100, 255);
                package.AddLineStripVertex(line.EndPoint.X, line.EndPoint.Y, line.EndPoint.Z);
            }
        }

        /// <summary>
        /// Draws plane at half the scale.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="plane"></param>
        /// <param name="name"></param>
        private void DrawPlane(ref IRenderPackage package, Plane plane, Planes name)
        {
            package.Description = RenderDescriptions.ManipulatorPlane + Name + name.ToString();
            var p1 = Origin.Add(plane.XAxis.Scale(scale/2));
            var p2 = p1.Add(plane.YAxis.Scale(scale/2));
            var p3 = Origin.Add(plane.YAxis.Scale(scale/2));
            
            package.AddLineStripVertexCount(3);
            package.AddLineStripVertexColor(0, 0, 255, 255);
            package.AddLineStripVertex(p1.X, p1.Y, p1.Z);

            package.AddLineStripVertexColor(0, 0, 255, 255);
            package.AddLineStripVertex(p2.X, p2.Y, p2.Z);

            package.AddLineStripVertexColor(0, 0, 255, 255);
            package.AddLineStripVertex(p3.X, p3.Y, p3.Z);
        }

        /// <summary>
        /// Draws axis as 3D arrow based on scale factor.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="axis"></param>
        private void DrawAxis(ref IRenderPackage package, AxisDescriptor axis)
        {
            package.Description = RenderDescriptions.ManipulatorAxis + Name + axis.Name;

            using (var axisEnd = Origin.Add(axis.Axis.Scale(scale)))
            {
                var color = axis.Color;
                package.AddLineStripVertexCount(2);
                package.AddLineStripVertexColor(color.R, color.G, color.B, color.A);
                package.AddLineStripVertex(Origin.X, Origin.Y, Origin.Z);
                package.AddLineStripVertexColor(color.R, color.G, color.B, color.A);
                package.AddLineStripVertex(axisEnd.X, axisEnd.Y, axisEnd.Z);
            }
        }

        #endregion

    }
}
