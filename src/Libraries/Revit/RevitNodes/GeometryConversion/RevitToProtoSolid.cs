using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;

using Edge = Autodesk.Revit.DB.Edge;
using Element = Revit.Elements.Element;
using Face = Autodesk.Revit.DB.Face;
using Point = Autodesk.DesignScript.Geometry.Point;
using Solid = Autodesk.DesignScript.Geometry.Solid;
using UV = Autodesk.DesignScript.Geometry.UV;
using RevitUV = Autodesk.Revit.DB.UV;
using Quadrant = System.Int32;

namespace Revit.GeometryConversion
{
    public static class PointInPoly
    {
        /// <summary>
        /// Determine the quadrant of a polygon vertex 
        /// relative to the test point.
        /// </summary>
        public static Quadrant GetQuadrant(RevitUV vertex, RevitUV p)
        {
            return (vertex.U > p.U)
              ? ((vertex.V > p.V) ? 0 : 3)
              : ((vertex.V > p.V) ? 1 : 2);
        }

        /// <summary>
        /// Determine the X intercept of a polygon edge 
        /// with a horizontal line at the Y value of the 
        /// test point.
        /// </summary>
        public static double X_intercept(RevitUV p, RevitUV q, double y)
        {
            Debug.Assert(0 != (p.V - q.V),
              "unexpected horizontal segment");

            return q.U
              - ((q.V - y)
                * ((p.U - q.U) / (p.V - q.V)));
        }

        public static void AdjustDelta(
          ref int delta,
          RevitUV vertex,
          RevitUV next_vertex,
          RevitUV p)
        {
            switch (delta)
            {
                // make quadrant deltas wrap around:
                case 3: delta = -1; break;
                case -3: delta = 1; break;
                // check if went around point cw or ccw:
                case 2:
                case -2:
                    if (X_intercept(vertex, next_vertex, p.V)
                      > p.U)
                    {
                        delta = -delta;
                    }
                    break;
            }
        }

        /// <summary>
        /// Determine whether given 2D point lies within 
        /// the polygon.
        /// 
        /// Written by Jeremy Tammik, Autodesk, 2009-09-23, 
        /// based on code that I wrote back in 1996 in C++, 
        /// which in turn was based on C code from the 
        /// article "An Incremental Angle Point in Polygon 
        /// Test" by Kevin Weiler, Autodesk, in "Graphics 
        /// Gems IV", Academic Press, 1994.
        /// 
        /// Copyright (C) 2009 by Jeremy Tammik. All 
        /// rights reserved.
        /// 
        /// This code may be freely used. Please preserve 
        /// this comment.
        /// </summary>
        public static bool PolygonContains(
          List<RevitUV> polygon,
          RevitUV point)
        {
            // initialize
            Quadrant quad = GetQuadrant(
              polygon[0], point);

            Quadrant angle = 0;

            // loop on all vertices of polygon
            Quadrant next_quad, delta;
            int n = polygon.Count;
            for (int i = 0; i < n; ++i)
            {
                RevitUV vertex = polygon[i];

                RevitUV next_vertex = polygon[ (i + 1 < n) ? i + 1 : 0];

                // calculate quadrant and delta from last quadrant

                next_quad = GetQuadrant(next_vertex, point);
                delta = next_quad - quad;

                AdjustDelta(
                  ref delta, vertex, next_vertex, point);

                // add delta to total angle sum
                angle = angle + delta;

                // increment for next step
                quad = next_quad;
            }

            // complete 360 degrees (angle of + 4 or -4 ) 
            // means inside

            return (angle == +4) || (angle == -4);

            // odd number of windings rule:
            // if (angle & 4) return INSIDE; else return OUTSIDE;
            // non-zero winding rule:
            // if (angle != 0) return INSIDE; else return OUTSIDE;
        }
    }

    public static class PolygonContainment
    {
        public static IEnumerable<Autodesk.Revit.DB.UV> GetVerticesOfEdgeLoopInFace(IEnumerable<Autodesk.Revit.DB.Edge> edgeLoop, Autodesk.Revit.DB.Face face)
        {
            return edgeLoop.SelectMany(x => x.TessellateOnFace(face));
        }

        public static bool IsContainedIn(this List<RevitUV> edgeLoop, List<RevitUV> poly)
        {
            // if any point is inside of the edge loop, it is contained within
            return edgeLoop.First().IsContainedIn(poly);
        }

        public static bool IsContainedIn(this RevitUV edgeVertex, List<RevitUV> poly)
        {
            return PointInPoly.PolygonContains(poly, edgeVertex);
        }

        public static IEnumerable<IEnumerable<IEnumerable<Edge>>> PartitionedEdgeLoops(this Face face)
        {
            // children are contained within only one edge

            // parents are contained within zero edges
            var edgeLoops = GetEdgeLoopsFromRevitFace(face).Select(x => x.ToList());

            var tesselatedEdgeLoops = new List<Tuple<List<Edge>, List<RevitUV>>>();

            // tesselate all of the edge loops, joining them with their edge loop parent
            foreach (var edgeLoop in edgeLoops)
            {
                var verts = GetVerticesOfEdgeLoopInFace(edgeLoop, face).ToList();
                tesselatedEdgeLoops.Add( new Tuple<List<Edge>, List<RevitUV>>(edgeLoop, verts));
            }

            var outerEdgeLoops = new List<Tuple<List<Edge>, List<RevitUV>>>();
            var innerEdgeLoops = new List<Tuple<List<Edge>, List<RevitUV>>>();

            // outerLoops are not contained in any other loop
            for (var i = 0; i < tesselatedEdgeLoops.Count; i++)
            {
                var el = tesselatedEdgeLoops[i];

                var isOuter = tesselatedEdgeLoops.Where((t, j) => i != j)
                    .All(bound => !el.Item2.IsContainedIn(bound.Item2));

                if (isOuter)
                {
                    outerEdgeLoops.Add(el);
                }
                else
                {
                    innerEdgeLoops.Add(el);
                }
            }

            // for each outer loop, collect its children
            var partitionedLoops = new List<List<List<Edge>>>();

            for (var i = 0; i < outerEdgeLoops.Count; i++)
            {
                var outerLoop = outerEdgeLoops[i];

                var comp = new List<List<Edge>> { outerLoop.Item1 };

                var mask = new List<bool>();

                for (var j = 0; j < innerEdgeLoops.Count; j++)
                {
                    var innerLoop = innerEdgeLoops[i];
                    if (innerLoop.Item2.IsContainedIn(outerLoop.Item2))
                    {
                        mask.Add(false);
                        comp.Add(innerLoop.Item1);
                    }
                    else
                    {
                        mask.Add(true);
                    }
                }

                outerEdgeLoops = outerEdgeLoops.Where((t, j) => mask[j]).ToList();
                partitionedLoops.Add( comp );
            }

            return partitionedLoops;
        }

        public static IEnumerable<IEnumerable<Autodesk.Revit.DB.Edge>> GetEdgeLoopsFromRevitFace(Face face)
        {
            return face.EdgeLoops.Cast<EdgeArray>()
                .Select(x => x.Cast<Autodesk.Revit.DB.Edge>());
        }

    }

    public static class SolidDebugging
    {
        public static IEnumerable<Autodesk.Revit.DB.Solid> GetRevitSolids(Element ele)
        {
            return ele.InternalGeometry().OfType<Autodesk.Revit.DB.Solid>().ToArray();
        }

        public static IEnumerable<Surface> GetSurfaces(Autodesk.Revit.DB.Solid geom)
        {
            return geom.Faces.Cast<Face>().Select(x => x.ToProtoType(false));
        }

        public static Surface GetSurfaceFromRevitFace(Face geom)
        {
            return geom.ToProtoType(false);
        }

        public static IEnumerable<Face> GetRevitFaces(Autodesk.Revit.DB.Solid geom)
        {
            return geom.Faces.Cast<Face>();
        }

        public static IEnumerable<IEnumerable<Autodesk.Revit.DB.Edge>> GetEdgeLoopsFromRevitFace(Face face)
        {
            return face.EdgeLoops.Cast<EdgeArray>()
                .Select(x => x.Cast<Autodesk.Revit.DB.Edge>());
        }

        public static IEnumerable<PolyCurve> GetEdgeLoopsFromRevitFaceAsPolyCurves(Face geom)
        {
            return RevitToProtoFace.EdgeLoopsAsPolyCurves(geom);
        }

        public static Surface GetUntrimmedSurfaceFromRevitFace(Face geom, 
            IEnumerable<PolyCurve> edgeLoops )
        {
            var dyFace = (dynamic)geom;
            return (Surface) SurfaceExtractor.ExtractSurface(dyFace, edgeLoops);
        }


        public static IEnumerable<Point> GetVerticesOfEdgeAsPoints()
        {

        }
    }

    [IsVisibleInDynamoLibrary(false)]
    [SupressImportIntoVM]
    public static class RevitToProtoSolid
    {
        public static Autodesk.DesignScript.Geometry.Solid ToProtoType(this Autodesk.Revit.DB.Solid solid, 
            bool performHostUnitConversion = true)
        {
            var srfs = solid.Faces.Cast<Autodesk.Revit.DB.Face>().Select(x => x.ToProtoType(false));
            var converted = Solid.ByJoinedSurfaces( srfs );

            return performHostUnitConversion ? converted.InDynamoUnits() : converted;
        }
    }
}
