using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

using DSCore.Properties;

namespace DSCore
{
    [IsVisibleInDynamoLibrary(false)]
    public class Quadtree
    {
        public Node Root { get; set; }

        private Quadtree(IEnumerable<UV> uvs)
        {
            Root = new Node(UV.ByCoordinates(), UV.ByCoordinates(1, 1));
            uvs.ToList().ForEach(uv => Root.Insert(uv));
        }

        /// <summary>
        /// Construct a Quadtree encompassing the (0,0)->(1,1) domain.
        /// </summary>
        /// <param name="uvs">A set of UVs in the (0,0)->(1,1) domain.</param>
        /// <returns>A Quadtree object.</returns>
        public static Quadtree ByUVs(IEnumerable<UV> uvs)
        {
            if (uvs == null)
            {
                throw new ArgumentNullException(
                    "uvs",
                    Resources.QuadtreeConstructionNullUVSetMessage);
            }

            if (!uvs.Any())
            {
                throw new ArgumentException(
                    Resources.QuadtreeConstructionEmptyUVSetMessage);
            }

            return new Quadtree(uvs);
        }

        /// <summary>
        /// Find all quadtree points (UVs) in the quadtree within a radius of the given UV location.
        /// </summary>
        /// <param name="center">The UV at the center of the search area.</param>
        /// <param name="radius">The radius of the search area.</param>
        /// <returns>A list of UVs.</returns>
        public List<UV> FindPointsWithinRadius(UV center, double radius)
        {
            if (center == null)
            {
                throw new ArgumentNullException(
                    "center",
                    Resources.FindPointsWithinRadiusNullPointMessage);
            }

            if (radius <= 0.0)
            {
                throw new ArgumentException(
                    "radius",
                    Resources.FindPointsWithinRadiusSearchRadiusMessage);
            }

            return Root.FindNodesWithinRadius(center, radius)
                .Where(n => n.Point != null)
                .Select(n => n.Point)
                .ToList();
        }

        public List<UV> FindPointsInRectangle(UVRect rectangle)
        {
            return
                Root.FindNodesIntersectingRectangle(rectangle)
                    .Where(n => n.Point != null)
                    .Select(n => n.Point)
                    .ToList();
        }

    }

    [IsVisibleInDynamoLibrary(false)]
    public class Node
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
            if (!Contains(uv))
            {
                return;
            }

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

                // Move the existing point into a new cell
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

            if (NW.Contains(uv))
            {
                if (NW.TryFind(uv, out node))
                {
                    return true;
                }
            }

            else if (NE.Contains(uv))
            {
                if (NE.TryFind(uv, out node))
                {
                    return true;
                }
            }


            else if (SW.Contains(uv))
            {
                if (SW.TryFind(uv, out node))
                {
                    return true;
                }
            }

            else if (SE.Contains(uv))
            {
                if (SE.TryFind(uv, out node))
                {
                    return true;
                } 
            }

            node = null;
            return false;
        }

        public List<Node> GetAllNodes()
        {
            var nodes = new List<Node>();

            if (IsLeafNode)
            {
                nodes.Add(this);
                return nodes;
            }

            nodes.AddRange(NW.GetAllNodes());
            nodes.AddRange(NE.GetAllNodes());
            nodes.AddRange(SW.GetAllNodes());
            nodes.AddRange(SE.GetAllNodes());

            return nodes;
        }

        public List<Node> FindAllNodesUpLevel(int count)
        {
            var i = 0;
            Node level = this;

            while (i < count)
            {
                if (level.Parent != null)
                {
                    level = level.Parent;
                }
                else
                {
                    break;
                }
                i++;
            }

            return level.GetAllNodes();
        }

        public Node FindNodeWhichContains(UV point)
        {
            Node n = null;

            if (IsLeafNode)
            {
                if (Contains(point))
                {
                    return this;
                }
            }
            else
            {
                if (NW.Contains(point))
                {
                    n = NW.FindNodeWhichContains(point);
                }
                else if (NE.Contains(point))
                {
                    n = NE.FindNodeWhichContains(point);
                }
                else if (SW.Contains(point))
                {
                    n = SW.FindNodeWhichContains(point);
                }
                else if (SE.Contains(point))
                {
                    n = SE.FindNodeWhichContains(point);
                }
                
            }

            return n;
        }

        public List<Node> FindNodesWithinRadius(UV location, double radius)
        {
            var nodes = new List<Node>();
            var circle = Circle.ByCenterPointRadius(
                Autodesk.DesignScript.Geometry.Point.ByCoordinates(location.U, location.V),
                radius);

            if (!Intersects(circle)) return nodes;

            if (IsLeafNode)
            {
                nodes.Add(this);
                return nodes;
            }

            nodes.AddRange(NW.FindNodesWithinRadius(location, radius));
            nodes.AddRange(NE.FindNodesWithinRadius(location, radius));
            nodes.AddRange(SW.FindNodesWithinRadius(location, radius));
            nodes.AddRange(SE.FindNodesWithinRadius(location, radius));

            return nodes;
        }

        public List<Node> FindNodesIntersectingRectangle(UVRect rectangle)
        {
            var nodes = new List<Node>();
            if (!Intersects(rectangle)) return nodes;

            if (IsLeafNode)
            {
                nodes.Add(this);
                return nodes;
            }

            nodes.AddRange(NW.FindNodesIntersectingRectangle(rectangle));
            nodes.AddRange(NE.FindNodesIntersectingRectangle(rectangle));
            nodes.AddRange(SW.FindNodesIntersectingRectangle(rectangle));
            nodes.AddRange(SE.FindNodesIntersectingRectangle(rectangle));

            return nodes;
        }

        private bool Intersects(Circle circle)
        {
            //http://stackoverflow.com/questions/401847/circle-rectangle-collision-detection-intersection

            var circleDistX = System.Math.Abs(circle.CenterPoint.X - Bounds.CenterPoint.U);
            var circleDistY = System.Math.Abs(circle.CenterPoint.Y - Bounds.CenterPoint.V);

            if (circleDistX > (Bounds.Width / 2 + circle.Radius)) { return false; }
            if (circleDistY > (Bounds.Height / 2 + circle.Radius)) { return false; }

            if (circleDistX <= (Bounds.Width / 2)) { return true; }
            if (circleDistY <= (Bounds.Height / 2)) { return true; }

            var cornerDistance_sq = System.Math.Pow(circleDistX - Bounds.Width / 2, 2) +
                                 System.Math.Pow(circleDistY - Bounds.Height / 2, 2);

            return (cornerDistance_sq <= System.Math.Pow(circle.Radius, 2));
        }

        private bool Intersects(UVRect rect)
        {
            return Bounds.Intersects(rect);
        }
    }

    /// <summary>
    /// Helper class used to define a Rectangle described
    /// by a minimum and a maximum UV.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class UVRect
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

        public UV CenterPoint
        {
            get { return UV.ByCoordinates(Min.U + Width/2, Min.V + Height/2); }
        }

        public UVRect(UV min, UV max)
        {
            Min = min;
            Max = max;
        }

        public bool Contains(UV uv)
        {
            return uv.U <= Max.U && uv.U >= Min.U && 
                uv.V <= Max.V && uv.V >= Min.V;
        }

        public bool Intersects(UVRect rect)
        {
            return this.Min.U < rect.Max.U && this.Max.U > rect.Min.U && this.Min.V < rect.Max.V
                && this.Max.V > rect.Min.V;
        }
    }

    /// <summary>
    /// Extensions methods for UVs.
    /// </summary>
    internal static class UVExtensions
    {
        public static bool IsAlmostEqualTo(this UV a, UV b)
        {
            const double tolerance = 0.00001;
            return System.Math.Abs(a.U - b.U) < tolerance && System.Math.Abs(a.V - b.V) < tolerance;
        }

        public static double Area(this UV min, UV max)
        {
            var u = System.Math.Sqrt(System.Math.Pow(max.U - min.U, 2));
            var v = System.Math.Sqrt(System.Math.Pow(max.V - min.V, 2));
            return u * v;
        }
    }
}
