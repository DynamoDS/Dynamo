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

        private PanelSurface(Point[] vertices, int[][] panels)
        {
            this.vertices = vertices;
            this.panels = panels;
        }

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
