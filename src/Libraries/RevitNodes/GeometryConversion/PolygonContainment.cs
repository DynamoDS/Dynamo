using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;

namespace Revit.GeometryConversion
{
    [SupressImportIntoVM]
    public static class PolygonContainment
    {
        /// <summary>
        /// Determine the quadrant of a polygon vertex 
        /// relative to the test point.
        /// </summary>
        public static Int32 GetQuadrant(UV vertex, UV p)
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
        public static double GetXIntercept(UV p, UV q, double y)
        {
            Debug.Assert(0 != (p.V - q.V),
              "unexpected horizontal segment");

            return q.U
              - ((q.V - y)
                * ((p.U - q.U) / (p.V - q.V)));
        }

        public static void AdjustDelta(
          ref int delta,
          UV vertex,
          UV next_vertex,
          UV p)
        {
            switch (delta)
            {
                // make quadrant deltas wrap around:
                case 3: delta = -1; break;
                case -3: delta = 1; break;
                // check if went around point cw or ccw:
                case 2:
                case -2:
                    if (GetXIntercept(vertex, next_vertex, p.V)
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
        public static bool PolygonContains(List<UV> polygon, UV point)
        {
            // initialize
            Int32 quad = GetQuadrant(
              polygon[0], point);

            Int32 angle = 0;

            // loop on all vertices of polygon
            Int32 next_quad, delta;
            int n = polygon.Count;
            for (int i = 0; i < n; ++i)
            {
                UV vertex = polygon[i];

                UV next_vertex = polygon[(i + 1 < n) ? i + 1 : 0];

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

}
