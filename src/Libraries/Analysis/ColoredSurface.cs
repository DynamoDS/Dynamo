using System;
using System.Diagnostics;
using System.Linq;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

using DSCore;

using Point = Autodesk.DesignScript.Geometry.Point;

namespace Analysis
{
    public class ColoredSurface : IGraphicItem
    {
        private Surface surface;
        private Color[] colors;
        private UV[] uvs;

        private ColoredSurface(Surface surface, 
            DSCore.Color[] colors, UV[] uvs)
        {
            this.surface = surface;
            this.colors = colors;
            this.uvs = uvs;
        }

        public static ColoredSurface ByColorsAndUVs(Surface surface, DSCore.Color[] colors, UV[] uvs)
        {
            if (surface == null)
            {
                throw new ArgumentNullException("surface");
            }

            if (colors == null)
            {
                throw new ArgumentNullException("colors");
            }

            if (!colors.Any())
            {
                throw new ArgumentException("There are no colors specified.");
            }

            if (uvs == null)
            {
                throw new ArgumentNullException("uvs");
            }

            if (!uvs.Any())
            {
                throw new ArgumentException("There are no UVs specified.");
            }

            if (uvs.Count() != colors.Count())
            {
                throw new Exception("The number of colors and the number of locations specified must be equal.");
            }

            return new ColoredSurface(surface, colors, uvs);
        }

        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(IRenderPackage package, double tol = -1, int maxGridLines = 512)
        {
            var sw = new Stopwatch();

            // Use ASM's tesselation routine to tesselate
            // the surface. Tesselate with a high degree 
            // of precision to ensure that UVs can be matched 
            // to vertices.
            surface.Tessellate(package);

            DebugTime(sw, "Ellapsed for tessellation.");

            int colorCount = 0;

            // Build a quadtree
            var qt = new Quadtree(UV.ByCoordinates(), UV.ByCoordinates(1, 1));
            for (int i = 0; i < uvs.Count(); i++)
            {
                qt.Root.Insert(uvs[i]);
            }
            DebugTime(sw, "Ellapsed for quadtree construction.");

            // Store the colors
            for (int i = 0; i < uvs.Count(); i++)
            {
                Node node;
                if (qt.Root.TryFind(uvs[i], out node))
                {
                    node.Item = colors[i];
                }
            }
            DebugTime(sw, "Ellapsed for storing colors.");

            var avgColor = Color.ByARGB(255, 255, 255, 255);

            for (int i = 0; i < package.TriangleVertices.Count; i += 3)
            {
                var vx = package.TriangleVertices[i];
                var vy = package.TriangleVertices[i + 1];
                var vz = package.TriangleVertices[i + 2];

                // Get the triangle vertex
                var v = Point.ByCoordinates(vx, vy, vz);
                var uv = surface.UVParameterAtPoint(v);

                // Create the weighted color by finding
                // all nodes within 2 levels of the node
                Node node = qt.Root.FindNodeWhichContains(uv);
                if (node != null)
                {
                    var nodes = node.FindAllNodesUpLevel(1);
                    var weightedColors =
                        nodes.Where(n => n.Point != null)
                            .Where(n => n.Item != null)
                            .Select(n => new Color.WeightedColor2D((Color)n.Item, uv.Area(n.Point))).ToList();
                    avgColor = Color.Blerp(weightedColors);
                }

                //var nodes = new List<Node>();
                //double radius = 0.05;
                //while (nodes.Count() < 3)
                //{
                    //nodes = qt.Root.FindNodesWithinRadius(uv, radius);
                //    radius += 0.01;
                //}
                //DebugTime(sw, "For finding nodes in radius.");

                //var weightedColors =
                //    nodes.Where(n => n.Point != null)
                //        .Where(n => n.Item != null)
                //        .Select(n => new Color.WeightedColor2D((Color)n.Item, uv.Area(n.Point))).ToList();
                //avgColor = Color.Blerp(weightedColors);
                //DebugTime(sw, "For interpolating colors.");

                package.TriangleVertexColors[colorCount] = avgColor.Red;
                package.TriangleVertexColors[colorCount + 1] = avgColor.Green;
                package.TriangleVertexColors[colorCount + 2] = avgColor.Blue;
                package.TriangleVertexColors[colorCount + 3] = avgColor.Alpha;

                colorCount += 4;
            }

            DebugTime(sw, "Ellapsed for setting colors on mesh.");
            sw.Stop();
        }

        private void DebugTime(Stopwatch sw, string message)
        {
            sw.Stop();
            Debug.WriteLine("{0}:{1}",sw.Elapsed, message);
            sw.Reset();
            sw.Start();
        }
    }
}
