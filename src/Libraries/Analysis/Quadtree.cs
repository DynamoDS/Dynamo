using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace Analysis
{
    [IsVisibleInDynamoLibrary(false)]
    public class Quadtree
    {
        internal Node Root { get; set; }

        internal Quadtree(UV min, UV max)
        {
            Root = new Node(min, max);
        }

        internal bool TryFind(UV uv, out Node node)
        {
            return Root.TryFind(uv, out node);
        }
    }

    public class SurfaceQuadtree : Quadtree, IGraphicItem
    {
        private Surface surface;
        private IList<UV> uvs;
 
        private SurfaceQuadtree(Surface surface, IList<UV> uvs)
            : base(UV.ByCoordinates(), UV.ByCoordinates(1, 1))
        {
            this.surface = surface;
            this.uvs = uvs;
        }

        public static SurfaceQuadtree BySurfaceAndUVs(Surface surface, IList<UV> uvs)
        {
            if (surface == null)
            {
                throw new ArgumentNullException("surface");
            }

            if (!uvs.Any())
            {
                throw new Exception("You must specify some UV locations.");
            }

            return new SurfaceQuadtree(surface, uvs);
        }

        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(IRenderPackage package, double tol = -1, int maxGridLines = 512)
        {
            var qt = new Quadtree(UV.ByCoordinates(), UV.ByCoordinates(1, 1));

            foreach (var uv in uvs)
            {
                var pt = surface.PointAtParameter(uv.U, uv.V);
                package.PushPointVertex(pt.X, pt.Y, pt.Z);
                package.PushPointVertexColor(0,0,0,255);

                qt.Root.Insert(uv);
            }

            DrawNode(qt.Root, package);
        }

        private void DrawNode(Node node, IRenderPackage package)
        {
            if (!node.IsLeafNode)
            {
                DrawNode(node.NW, package);
                DrawNode(node.NE, package);
                DrawNode(node.SW, package);
                DrawNode(node.SE, package);
            }
            else
            {
                // Draw the node boundary
                var a = surface.PointAtParameter(node.Bounds.Min.U, node.Bounds.Min.V);
                var b = surface.PointAtParameter(node.Bounds.Min.U, node.Bounds.Max.V);
                var c = surface.PointAtParameter(node.Bounds.Max.U, node.Bounds.Max.V);
                var d = surface.PointAtParameter(node.Bounds.Max.U, node.Bounds.Min.V);

                package.PushLineStripVertex(a.X, a.Y, a.Z);
                package.PushLineStripVertex(b.X, b.Y, b.Z);
                package.PushLineStripVertexCount(2);

                package.PushLineStripVertex(b.X, b.Y, b.Z);
                package.PushLineStripVertex(c.X, c.Y, c.Z);
                package.PushLineStripVertexCount(2);

                package.PushLineStripVertex(c.X, c.Y, c.Z);
                package.PushLineStripVertex(d.X, d.Y, d.Z);
                package.PushLineStripVertexCount(2);

                package.PushLineStripVertex(d.X, d.Y, d.Z);
                package.PushLineStripVertex(a.X, a.Y, a.Z);
                package.PushLineStripVertexCount(2);
            }
        }
    }

    internal class Node
    {
        public Node Parent { get; internal set; }
        public Node NW { get; internal set; }
        public Node NE { get; internal set; }
        public Node SW { get; internal set; }
        public Node SE { get; internal set; }
        public UV Point { get; set; }
        public UVRect Bounds { get; set; }
        public object Item { get; set; }

        public bool IsLeafNode
        {
            get { return NW == null && NE == null && SW == null && SE == null; }
        }

        public Node(UV min, UV max)
        {
            Bounds = new UVRect(min, max);
        }

        public bool Contains(UV uv)
        {
            return Bounds.Contains(uv);
        }

        public void Insert(UV uv)
        {
            if (!Contains(uv)) return;

            if (IsLeafNode)
            {
                // If the node that is being inserted is 
                // the same as one that exists, then return
                // true;

                if (Point == null)
                {
                    Point = uv;
                    return;
                }

                if (uv.IsAlmostEqualTo(Point))
                {
                    return;
                }

                Split();

                NW.Insert(UV.ByCoordinates(Point.U, Point.V));
                NE.Insert(UV.ByCoordinates(Point.U, Point.V));
                SE.Insert(UV.ByCoordinates(Point.U, Point.V));
                SW.Insert(UV.ByCoordinates(Point.U, Point.V));

                Point = null;

                // Insert the new UV into the correct cell
                NW.Insert(uv);
                NE.Insert(uv);
                SW.Insert(uv);
                SE.Insert(uv);
            }
            else
            {
                NW.Insert(uv);
                NE.Insert(uv);
                SW.Insert(uv);
                SE.Insert(uv);
            }
        }

        /// <summary>
        /// Splite a node into four quadrants.
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns></returns>
        private void Split()
        {
            var l0 = UV.ByCoordinates(Bounds.Min.U, Bounds.Min.V);
            var l1 = UV.ByCoordinates(Bounds.Min.U, Bounds.Min.V + Bounds.Height / 2);
            var l2 = UV.ByCoordinates(Bounds.Min.U, Bounds.Max.V);

            var c0 = UV.ByCoordinates(Bounds.Min.U + Bounds.Width / 2, Bounds.Min.V);
            var c1 = UV.ByCoordinates(Bounds.Min.U + Bounds.Width / 2, Bounds.Min.V + Bounds.Height / 2);
            var c2 = UV.ByCoordinates(Bounds.Min.U + Bounds.Width / 2, Bounds.Max.V);

            var r0 = UV.ByCoordinates(Bounds.Max.U, Bounds.Min.V);
            var r1 = UV.ByCoordinates(Bounds.Max.U, Bounds.Min.V + Bounds.Height / 2);
            var r2 = UV.ByCoordinates(Bounds.Max.U, Bounds.Max.V);

            NW = new Node(l1, c2);
            NE = new Node(c1, r2);
            SW = new Node(l0, c1);
            SE = new Node(c0, r1);

            NW.Parent = this;
            NE.Parent = this;
            SW.Parent = this;
            SE.Parent = this;
        }

        public bool TryFind(UV uv, out Node node)
        {
            if (!Contains(uv))
            {
                node = null;
                return false;
            }

            if (IsLeafNode)
            {
                if (Point.IsAlmostEqualTo(uv))
                {
                    node = this;
                    return true;
                }
                else
                {
                    node = null;
                    return false;
                }
            }

            if (NW.TryFind(uv, out node))
            {
                return true;
            }

            if (NE.TryFind(uv, out node))
            {
                return true;
            }

            if (SW.TryFind(uv, out node))
            {
                return true;
            }

            if (SE.TryFind(uv, out node))
            {
                return true;
            }

            node = null;
            return false;
        }
    }

    internal class UVRect
    {
        public UV Min { get; set; }
        public UV Max { get; set; }

        public double Width
        {
            get { return Max.U - Min.U; }
        }

        public double Height
        {
            get { return Max.V - Min.V; }
        }

        public UVRect(UV min, UV max)
        {
            Min = min;
            Max = max;
        }

        public bool Contains(UV uv)
        {
            return uv.U < Max.U && uv.U >= Min.U && 
                uv.V < Max.V && uv.V >= Min.V;
        }

        public bool Intersects(UVRect rect)
        {
            throw new NotImplementedException();
        }
    }

    internal static class UVExtensions
    {
        public static bool IsAlmostEqualTo(this UV a, UV b)
        {
            const double tolerance = 0.00001;
            return System.Math.Abs(a.U - b.U) < tolerance && System.Math.Abs(a.V - b.V) < tolerance;
        }
    }
}
