using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Autodesk.DesignScript.Geometry;
using Dynamo.Wpf.ViewModels.Watch3D;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using OrthographicCamera = HelixToolkit.Wpf.SharpDX.OrthographicCamera;
using PerspectiveCamera = HelixToolkit.Wpf.SharpDX.PerspectiveCamera;
using Point = Autodesk.DesignScript.Geometry.Point;
using ProjectionCamera = HelixToolkit.Wpf.SharpDX.ProjectionCamera;

namespace Dynamo.Controls
{
    internal static class CameraExtensions
    {
        private static Matrix WorldToModelMatrix()
        {
            return new Matrix(
                1, 0, 0, 0,
                0, 0, 1, 0,
                0, -1, 0, 0,
                0, 0, 0, 1
                );
        }

        internal static Ray Point2DToRay(this Viewport3DX viewport, Vector2 point2D)
        {
            var camera = viewport.Camera as ProjectionCamera;
            if (camera != null)
            {
                var p = new Vector3(point2D.X, point2D.Y, 1);

                var vp = viewport.GetScreenViewProjectionMatrix();
                var vpi = Matrix.Invert(vp);

                var test = 1f / ((p.X * vpi.M14) + (p.Y * vpi.M24) + (p.Z * vpi.M34) + vpi.M44);
                if (double.IsInfinity(test))
                {
                    vpi.M44 = vpi.M44 + 0.000001f;
                }

                var worldToModelMatrix = WorldToModelMatrix();
                
                p.Z = 0;
                var zn = Vector3.TransformCoordinate(p, vpi);
                zn = Vector3.TransformCoordinate(zn, worldToModelMatrix);

                p.Z = 1;
                var zf = Vector3.TransformCoordinate(p, vpi);
                zf = Vector3.TransformCoordinate(zf, worldToModelMatrix);
                
                Vector3 r = zf - zn;

                r.Normalize();

                if (camera is PerspectiveCamera)
                {
                    var pos = Vector3.TransformCoordinate(camera.Position.ToVector3(), worldToModelMatrix);
                    return new Ray(pos, r);
                }
                else if (camera is OrthographicCamera)
                {
                    return new Ray(zn, r);
                }
            }
            throw new HelixToolkitException("Unproject camera error.");
        }

        internal static Ray3D Point2DToRay3D(this Viewport3DX viewport, System.Windows.Point point2d)
        {
            var r = viewport.Point2DToRay(point2d.ToVector2());
            return new Ray3D(r.Position.ToPoint3D(), r.Direction.ToVector3D());
        }
    }

    internal static class PointExtensions
    {
        public static Point ToPoint(this Point3D point)
        {
            return Point.ByCoordinates(point.X, point.Y, point.Z);
        }

        public static Vector ToVector(this Vector3D vec)
        {
            return Vector.ByCoordinates(vec.X, vec.Y, vec.Z);
        }
    }

    public class Ray3 : IRay
    {
        private readonly Point origin;
        private readonly Vector direction;

        public Ray3(Point origin, Vector direction)
        {
            this.origin = origin;
            this.direction = direction;
        }

        internal Ray3(Point3D origin, Vector3D direction)
        {
            this.origin = origin.ToPoint();
            this.direction = direction.ToVector();
        }

        public Point Origin
        {
            get { return origin; }
        }

        public Vector Direction
        {
            get { return direction; }
        }
    }
}
