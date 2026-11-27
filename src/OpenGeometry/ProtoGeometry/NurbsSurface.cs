using System;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A non-uniform rational B-spline surface backed by SMLib.</summary>
    public class NurbsSurface : Surface
    {
        internal NurbsSurface(NativeGeometry.SolidHandle handle) : base(handle)
        {
        }

        /// <summary>Gets the polynomial degree in the U parameter direction.</summary>
        public int DegreeU => NativeGeometry.NurbsSurfaceInfo(Handle).UDegree;

        /// <summary>Gets the polynomial degree in the V parameter direction.</summary>
        public int DegreeV => NativeGeometry.NurbsSurfaceInfo(Handle).VDegree;

        /// <summary>Gets the number of control points in the U direction.</summary>
        public int NumControlPointsU => checked((int)NativeGeometry.NurbsSurfaceInfo(Handle).UCount);

        /// <summary>Gets the number of control points in the V direction.</summary>
        public int NumControlPointsV => checked((int)NativeGeometry.NurbsSurfaceInfo(Handle).VCount);

        /// <summary>Gets a value indicating whether the surface is periodic in the U direction.</summary>
        public bool IsPeriodicInU => NativeGeometry.NurbsSurfaceInfo(Handle).IsPeriodicU;

        /// <summary>Gets a value indicating whether the surface is periodic in the V direction.</summary>
        public bool IsPeriodicInV => NativeGeometry.NurbsSurfaceInfo(Handle).IsPeriodicV;

        /// <summary>Gets a value indicating whether the surface uses non-uniform rational weights.</summary>
        public bool IsRational => NativeGeometry.NurbsSurfaceInfo(Handle).IsRational;

        /// <summary>Returns the control point grid indexed as [u][v].</summary>
        public Point[][] ControlPoints()
        {
            var info = NativeGeometry.NurbsSurfaceInfo(Handle);
            var (flat, _) = NativeGeometry.NurbsSurfaceControlPoints(Handle);
            var uCount = checked((int)info.UCount);
            var vCount = checked((int)info.VCount);
            var grid = new Point[uCount][];
            for (var u = 0; u < uCount; u++)
            {
                grid[u] = new Point[vCount];
                for (var v = 0; v < vCount; v++)
                    grid[u][v] = new Point(flat[u * vCount + v]);
            }
            return grid;
        }

        /// <summary>Returns the control point weight grid indexed as [u][v].</summary>
        public double[][] Weights()
        {
            var info = NativeGeometry.NurbsSurfaceInfo(Handle);
            var (_, flat) = NativeGeometry.NurbsSurfaceControlPoints(Handle);
            var uCount = checked((int)info.UCount);
            var vCount = checked((int)info.VCount);
            var grid = new double[uCount][];
            for (var u = 0; u < uCount; u++)
            {
                grid[u] = new double[vCount];
                for (var v = 0; v < vCount; v++)
                    grid[u][v] = flat[u * vCount + v];
            }
            return grid;
        }

        /// <summary>Returns the knot vector in the U parameter direction.</summary>
        public double[] UKnots() => NativeGeometry.NurbsSurfaceKnots(Handle, vDirection: false);

        /// <summary>Returns the knot vector in the V parameter direction.</summary>
        public double[] VKnots() => NativeGeometry.NurbsSurfaceKnots(Handle, vDirection: true);

        /// <summary>Creates a NURBS surface interpolating a regular 2D grid of points.</summary>
        /// <param name="points">Grid of points indexed as [u][v].</param>
        /// <param name="uDegree">Polynomial degree in U (clamped to uCount - 1).</param>
        /// <param name="vDegree">Polynomial degree in V (clamped to vCount - 1).</param>
        /// <returns>The interpolated NURBS surface.</returns>
        public static NurbsSurface ByPoints(Point[][] points, int uDegree = 3, int vDegree = 3)
        {
            var (flat, uCount, vCount) = FlattenGrid(points, nameof(points));
            var degU = ClampDegree(uDegree, uCount, nameof(uDegree));
            var degV = ClampDegree(vDegree, vCount, nameof(vDegree));
            return new NurbsSurface(NativeGeometry.NurbsSurfaceByPoints(flat, (uint)uCount, (uint)vCount, degU, degV));
        }

        /// <summary>Creates a NURBS surface defined by a 2D grid of control points.</summary>
        /// <param name="controlVertices">Grid of control points indexed as [u][v].</param>
        /// <param name="uDegree">Polynomial degree in U (clamped to uCount - 1).</param>
        /// <param name="vDegree">Polynomial degree in V (clamped to vCount - 1).</param>
        /// <returns>The constructed NURBS surface.</returns>
        public static NurbsSurface ByControlPoints(Point[][] controlVertices, int uDegree = 3, int vDegree = 3)
        {
            var (flat, uCount, vCount) = FlattenGrid(controlVertices, nameof(controlVertices));
            var degU = ClampDegree(uDegree, uCount, nameof(uDegree));
            var degV = ClampDegree(vDegree, vCount, nameof(vDegree));
            return new NurbsSurface(NativeGeometry.NurbsSurfaceByControlPoints(flat, (uint)uCount, (uint)vCount, degU, degV));
        }

        private static (NativePoint[] Flat, int UCount, int VCount) FlattenGrid(Point[][] grid, string paramName)
        {
            ArgumentNullException.ThrowIfNull(grid);
            if (grid.Length < 2)
                throw new ArgumentException("A NURBS surface requires at least 2 rows of points.", paramName);
            var uCount = grid.Length;
            var vCount = grid[0]?.Length ?? 0;
            if (vCount < 2)
                throw new ArgumentException("A NURBS surface requires at least 2 columns of points.", paramName);

            var flat = new NativePoint[uCount * vCount];
            for (var u = 0; u < uCount; u++)
            {
                var row = grid[u];
                if (row == null || row.Length != vCount)
                    throw new ArgumentException("All rows in the NURBS surface point grid must have the same length.", paramName);
                for (var v = 0; v < vCount; v++)
                {
                    if (row[v] == null)
                        throw new ArgumentException("Point grid cannot contain null entries.", paramName);
                    flat[u * vCount + v] = row[v].Position;
                }
            }
            return (flat, uCount, vCount);
        }

        private static int ClampDegree(int degree, int count, string paramName)
        {
            if (degree < 1)
                throw new ArgumentOutOfRangeException(paramName, "Surface degree must be at least 1.");
            return Math.Min(degree, count - 1);
        }
    }
}
