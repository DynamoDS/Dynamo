using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>Boundary handling policy when panelizing a surface.</summary>
    public enum PanelSurfaceBoundaryCondition
    {
        /// <summary>Keep panels that cross surface boundaries intact.</summary>
        Keep = 0,
        /// <summary>Remove panels that extend outside surface boundaries.</summary>
        Remove = 1,
        /// <summary>Remove vertices that lie outside surface boundaries.</summary>
        RemoveVertices = 2,
        /// <summary>Trim panels at surface boundaries.</summary>
        Trim = 3
    }

    /// <summary>A surface subdivided into an indexed collection of planar polygonal panels.</summary>
    public class PanelSurface : Geometry
    {
        private readonly Point[] vertices;
        private readonly int[][] panels;

        internal PanelSurface(Point[] vertices, int[][] panels)
        {
            this.vertices = vertices;
            this.panels = panels;
        }

        internal Point[] VerticesArray => vertices;
        internal int[][] PanelsArray => panels;

        /// <summary>Gets the total number of panels in this panelized surface.</summary>
        public int NumPanels => panels.Length;

        /// <summary>Gets the total number of shared vertices across all panels.</summary>
        public int NumVertices => vertices.Length;

        /// <inheritdoc />
        public override BoundingBox BoundingBox => BoundingBox.ByGeometry(vertices);

        /// <summary>Returns the 3D position of vertex <paramref name="vertexIndex"/>.</summary>
        public Point GetPoint(int vertexIndex) => vertices[vertexIndex];

        /// <summary>Returns the vertex index at corner <paramref name="vertexNumber"/> of panel <paramref name="panelIndex"/>.</summary>
        public int GetVertexIndex(int panelIndex, int vertexNumber) => panels[panelIndex][vertexNumber];

        /// <summary>Returns the number of vertices for each specified panel index (or all panels when null/empty).</summary>
        public int[] GetNumPanelVertices(int[] panelIndices = null) =>
            SelectPanelIndices(panelIndices).Select(i => panels[i].Length).ToArray();

        /// <summary>Returns the vertex positions for each specified panel index (or all panels when null/empty).</summary>
        public Point[][] GetPanelPoints(int[] panelIndices = null) =>
            SelectPanelIndices(panelIndices)
                .Select(i => panels[i].Select(v => vertices[v]).ToArray())
                .ToArray();

        /// <summary>Returns a closed <see cref="Polygon"/> for each specified panel index (or all panels when null/empty).</summary>
        public Polygon[] GetPanelPolygon(int[] panelIndices = null) =>
            SelectPanelIndices(panelIndices)
                .Select(i => Polygon.ByPoints(panels[i].Select(v => vertices[v])))
                .ToArray();

        /// <summary>Subdivides a surface into a <paramref name="numU"/> by <paramref name="numV"/> grid of quadrilateral panels.</summary>
        public static PanelSurface ByQuads(
            Surface surface,
            int numU = 10,
            int numV = 10,
            PanelSurfaceBoundaryCondition boundaryCondition = PanelSurfaceBoundaryCondition.Keep)
        {
            ArgumentNullException.ThrowIfNull(surface);
            ValidateCounts(numU, numV);
            _ = boundaryCondition;

            var (pts, strideV) = SampleGrid(surface, numU, numV);
            var panelList = new List<int[]>(numU * numV);
            for (var u = 0; u < numU; u++)
            {
                for (var v = 0; v < numV; v++)
                {
                    var i00 = u * strideV + v;
                    var i10 = (u + 1) * strideV + v;
                    var i11 = (u + 1) * strideV + (v + 1);
                    var i01 = u * strideV + (v + 1);
                    panelList.Add(new[] { i00, i10, i11, i01 });
                }
            }
            return new PanelSurface(pts, panelList.ToArray());
        }

        /// <summary>Subdivides a surface into diagonally split triangular panels.</summary>
        public static PanelSurface ByDiagonallySplitSquares(
            Surface surface,
            int numU = 10,
            int numV = 10,
            bool alternativeDiagonal = false,
            PanelSurfaceBoundaryCondition boundaryCondition = PanelSurfaceBoundaryCondition.Keep)
        {
            ArgumentNullException.ThrowIfNull(surface);
            ValidateCounts(numU, numV);
            _ = boundaryCondition;

            var (pts, strideV) = SampleGrid(surface, numU, numV);
            var panelList = new List<int[]>(numU * numV * 2);
            for (var u = 0; u < numU; u++)
            {
                for (var v = 0; v < numV; v++)
                {
                    var i00 = u * strideV + v;
                    var i10 = (u + 1) * strideV + v;
                    var i11 = (u + 1) * strideV + (v + 1);
                    var i01 = u * strideV + (v + 1);
                    if (!alternativeDiagonal)
                    {
                        panelList.Add(new[] { i00, i10, i11 });
                        panelList.Add(new[] { i00, i11, i01 });
                    }
                    else
                    {
                        panelList.Add(new[] { i00, i10, i01 });
                        panelList.Add(new[] { i10, i11, i01 });
                    }
                }
            }
            return new PanelSurface(pts, panelList.ToArray());
        }

        /// <summary>Subdivides each grid cell of a surface into four triangles meeting at the cell center.</summary>
        public static PanelSurface ByCrossSplitSquares(
            Surface surface,
            int numU = 10,
            int numV = 10,
            PanelSurfaceBoundaryCondition boundaryCondition = PanelSurfaceBoundaryCondition.Keep)
        {
            ArgumentNullException.ThrowIfNull(surface);
            ValidateCounts(numU, numV);
            _ = boundaryCondition;

            var (gridPts, strideV) = SampleGrid(surface, numU, numV);
            var pts = new List<Point>(gridPts);
            var panelList = new List<int[]>(numU * numV * 4);
            for (var u = 0; u < numU; u++)
            {
                for (var v = 0; v < numV; v++)
                {
                    var i00 = u * strideV + v;
                    var i10 = (u + 1) * strideV + v;
                    var i11 = (u + 1) * strideV + (v + 1);
                    var i01 = u * strideV + (v + 1);
                    var ic = pts.Count;
                    pts.Add(surface.PointAtParameter((u + 0.5) / numU, (v + 0.5) / numV));
                    panelList.Add(new[] { i00, i10, ic });
                    panelList.Add(new[] { i10, i11, ic });
                    panelList.Add(new[] { i11, i01, ic });
                    panelList.Add(new[] { i01, i00, ic });
                }
            }
            return new PanelSurface(pts.ToArray(), panelList.ToArray());
        }

        /// <summary>Subdivides a surface into a diamond-shaped panel pattern.</summary>
        public static PanelSurface ByDiamonds(
            Surface surface,
            int numU = 10,
            int numV = 10,
            PanelSurfaceBoundaryCondition boundaryCondition = PanelSurfaceBoundaryCondition.Keep)
        {
            ArgumentNullException.ThrowIfNull(surface);
            ValidateCounts(numU, numV);
            _ = boundaryCondition;

            var pts = new List<Point>();
            var panelList = new List<int[]>(numU * numV);
            for (var u = 0; u < numU; u++)
            {
                var u0 = (double)u / numU;
                var u1 = (double)(u + 1) / numU;
                var um = 0.5 * (u0 + u1);
                for (var v = 0; v < numV; v++)
                {
                    var v0 = (double)v / numV;
                    var v1 = (double)(v + 1) / numV;
                    var vm = 0.5 * (v0 + v1);
                    var b = pts.Count;
                    pts.Add(surface.PointAtParameter(um, v0));
                    pts.Add(surface.PointAtParameter(u1, vm));
                    pts.Add(surface.PointAtParameter(um, v1));
                    pts.Add(surface.PointAtParameter(u0, vm));
                    panelList.Add(new[] { b, b + 1, b + 2, b + 3 });
                }
            }
            return new PanelSurface(pts.ToArray(), panelList.ToArray());
        }

        /// <summary>Subdivides a surface into a hexagonal tiling pattern.</summary>
        public static PanelSurface ByHexagons(
            Surface surface,
            int numU = 10,
            int numV = 10,
            PanelSurfaceBoundaryCondition boundaryCondition = PanelSurfaceBoundaryCondition.Keep)
        {
            ArgumentNullException.ThrowIfNull(surface);
            ValidateCounts(numU, numV);
            _ = boundaryCondition;

            var pts = new List<Point>();
            var panelList = new List<int[]>(numU * numV);
            var du = 1.0 / numU;
            var dv = 1.0 / numV;
            for (var u = 0; u < numU; u++)
            {
                for (var v = 0; v < numV; v++)
                {
                    var uShift = (v % 2 == 1) ? 0.5 * du : 0.0;
                    var cu = Math.Clamp((u + 0.5) * du + uShift * 0.5, 0.0, 1.0);
                    var cv = (v + 0.5) * dv;
                    var ru = 0.45 * du;
                    var rv = 0.45 * dv;
                    var b = pts.Count;
                    for (var k = 0; k < 6; k++)
                    {
                        var ang = k * (Math.PI / 3.0);
                        var pu = Math.Clamp(cu + ru * Math.Cos(ang), 0.0, 1.0);
                        var pv = Math.Clamp(cv + rv * Math.Sin(ang), 0.0, 1.0);
                        pts.Add(surface.PointAtParameter(pu, pv));
                    }
                    panelList.Add(new[] { b, b + 1, b + 2, b + 3, b + 4, b + 5 });
                }
            }
            return new PanelSurface(pts.ToArray(), panelList.ToArray());
        }

        /// <summary>Subdivides a surface into a staggered quad (brick) pattern.</summary>
        public static PanelSurface ByStaggeredQuads(
            Surface surface,
            int numU = 10,
            int numV = 10,
            bool staggerVertically = false,
            double displacementFactor = 0.5,
            PanelSurfaceBoundaryCondition boundaryCondition = PanelSurfaceBoundaryCondition.Keep)
        {
            ArgumentNullException.ThrowIfNull(surface);
            ValidateCounts(numU, numV);
            _ = boundaryCondition;

            var pts = new List<Point>();
            var panelList = new List<int[]>(numU * numV);
            var du = 1.0 / numU;
            var dv = 1.0 / numV;
            for (var u = 0; u < numU; u++)
            {
                for (var v = 0; v < numV; v++)
                {
                    var uOff = (!staggerVertically && (v % 2 == 1)) ? displacementFactor * du : 0.0;
                    var vOff = (staggerVertically && (u % 2 == 1)) ? displacementFactor * dv : 0.0;
                    var u0 = Math.Clamp(u * du + uOff, 0.0, 1.0);
                    var u1 = Math.Clamp((u + 1) * du + uOff, 0.0, 1.0);
                    var v0 = Math.Clamp(v * dv + vOff, 0.0, 1.0);
                    var v1 = Math.Clamp((v + 1) * dv + vOff, 0.0, 1.0);
                    if (u1 - u0 <= 1e-9 || v1 - v0 <= 1e-9)
                        continue;
                    var b = pts.Count;
                    pts.Add(surface.PointAtParameter(u0, v0));
                    pts.Add(surface.PointAtParameter(u1, v0));
                    pts.Add(surface.PointAtParameter(u1, v1));
                    pts.Add(surface.PointAtParameter(u0, v1));
                    panelList.Add(new[] { b, b + 1, b + 2, b + 3 });
                }
            }
            return new PanelSurface(pts.ToArray(), panelList.ToArray());
        }

        /// <summary>Subdivides a surface into sheared parallelogram panels.</summary>
        public static PanelSurface ByParallelograms(
            Surface surface,
            int numU = 10,
            int numV = 10,
            double shearFactor = 0.5,
            bool alignWithUAxis = false,
            PanelSurfaceBoundaryCondition boundaryCondition = PanelSurfaceBoundaryCondition.Keep)
        {
            ArgumentNullException.ThrowIfNull(surface);
            ValidateCounts(numU, numV);
            _ = boundaryCondition;

            var pts = new List<Point>();
            var panelList = new List<int[]>(numU * numV);
            var du = 1.0 / numU;
            var dv = 1.0 / numV;
            for (var u = 0; u < numU; u++)
            {
                for (var v = 0; v < numV; v++)
                {
                    var u0 = u * du;
                    var u1 = (u + 1) * du;
                    var v0 = v * dv;
                    var v1 = (v + 1) * dv;
                    var b = pts.Count;
                    if (!alignWithUAxis)
                    {
                        var su = shearFactor * du;
                        pts.Add(surface.PointAtParameter(u0, v0));
                        pts.Add(surface.PointAtParameter(u1, v0));
                        pts.Add(surface.PointAtParameter(Math.Clamp(u1 + su, 0.0, 1.0), v1));
                        pts.Add(surface.PointAtParameter(Math.Clamp(u0 + su, 0.0, 1.0), v1));
                    }
                    else
                    {
                        var sv = shearFactor * dv;
                        pts.Add(surface.PointAtParameter(u0, v0));
                        pts.Add(surface.PointAtParameter(u1, Math.Clamp(v0 + sv, 0.0, 1.0)));
                        pts.Add(surface.PointAtParameter(u1, Math.Clamp(v1 + sv, 0.0, 1.0)));
                        pts.Add(surface.PointAtParameter(u0, v1));
                    }
                    panelList.Add(new[] { b, b + 1, b + 2, b + 3 });
                }
            }
            return new PanelSurface(pts.ToArray(), panelList.ToArray());
        }

        internal override Geometry MirrorCore(Plane mirrorPlane) =>
            new PanelSurface(
                vertices.Select(v => (Point)v.Mirror(mirrorPlane)).ToArray(),
                panels.Select(p => p.Reverse().ToArray()).ToArray());

        /// <inheritdoc />
        public override Geometry Translate(double xTranslation = 0, double yTranslation = 0, double zTranslation = 0) =>
            new PanelSurface(
                vertices.Select(v => (Point)v.Translate(xTranslation, yTranslation, zTranslation)).ToArray(),
                panels.Select(p => p.ToArray()).ToArray());

        /// <inheritdoc />
        public override Geometry Rotate(Point origin, Vector axis, double degrees) =>
            new PanelSurface(
                vertices.Select(v => (Point)v.Rotate(origin, axis, degrees)).ToArray(),
                panels.Select(p => p.ToArray()).ToArray());

        /// <inheritdoc />
        public override Geometry Scale(Point basePoint, double xamount, double yamount, double zamount) =>
            new PanelSurface(
                vertices.Select(v => (Point)v.Scale(basePoint, xamount, yamount, zamount)).ToArray(),
                panels.Select(p => p.ToArray()).ToArray());

        /// <inheritdoc />
        public override Geometry Transform(CoordinateSystem cs) =>
            new PanelSurface(
                vertices.Select(v => (Point)v.Transform(cs)).ToArray(),
                panels.Select(p => p.ToArray()).ToArray());

        internal override void WritePreview(IRenderPackage package, TessellationParameters parameters)
        {
            foreach (var panel in panels)
            {
                if (panel.Length < 3)
                    continue;
                var p0 = vertices[panel[0]];
                for (var i = 1; i + 1 < panel.Length; i++)
                {
                    var p1 = vertices[panel[i]];
                    var p2 = vertices[panel[i + 1]];
                    var n = Vector.ByTwoPoints(p0, p1).Cross(Vector.ByTwoPoints(p0, p2));
                    var unit = n.Length > 1e-12 ? n.Normalized() : Vector.ZAxis();
                    Emit(package, p0, unit);
                    Emit(package, p1, unit);
                    Emit(package, p2, unit);
                }
            }
        }

        private static void Emit(IRenderPackage package, Point p, Vector n)
        {
            package.AddTriangleVertex(p.X, p.Y, p.Z);
            package.AddTriangleVertexNormal(n.X, n.Y, n.Z);
            package.AddTriangleVertexUV(0, 0);
            package.AddTriangleVertexColor(
                DefaultMeshColorR, DefaultMeshColorG, DefaultMeshColorB, DefaultMeshColorA);
        }

        private IEnumerable<int> SelectPanelIndices(int[] panelIndices)
        {
            if (panelIndices == null || panelIndices.Length == 0)
                return Enumerable.Range(0, panels.Length);
            return panelIndices;
        }

        private static void ValidateCounts(int numU, int numV)
        {
            if (numU < 1)
                throw new ArgumentOutOfRangeException(nameof(numU), "U panel count must be at least 1.");
            if (numV < 1)
                throw new ArgumentOutOfRangeException(nameof(numV), "V panel count must be at least 1.");
        }

        private static (Point[] Points, int StrideV) SampleGrid(Surface surface, int numU, int numV)
        {
            var strideV = numV + 1;
            var pts = new Point[(numU + 1) * strideV];
            for (var u = 0; u <= numU; u++)
            {
                var pu = (double)u / numU;
                for (var v = 0; v <= numV; v++)
                {
                    var pv = (double)v / numV;
                    pts[u * strideV + v] = surface.PointAtParameter(pu, pv);
                }
            }
            return (pts, strideV);
        }
    }
}
