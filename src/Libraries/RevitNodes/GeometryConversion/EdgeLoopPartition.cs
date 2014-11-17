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
    internal static class EdgeLoopPartition
    {
        public static List<List<List<Edge>>> ByEdgeLoopsAndFace(Face face, List<List<Edge>> edgeLoops)
        {
            if (edgeLoops.Count == 1)
                return new List<List<List<Edge>>> ()
                {
                    edgeLoops
                };

            var tesselatedEdgeLoops = new List<Tuple<List<Edge>, List<UV>>>();

            // tesselate all of the edge loops, joining them with their edge loop parent
            foreach (var edgeLoop in edgeLoops)
            {
                var verts = TesselateEdgeLoopOnFace(edgeLoop, face).ToList();
                tesselatedEdgeLoops.Add(new Tuple<List<Edge>, List<UV>>(edgeLoop.ToList(), verts));
            }

            var outerEdgeLoops = new List<Tuple<List<Edge>, List<UV>>>();
            var innerEdgeLoops = new List<Tuple<List<Edge>, List<UV>>>();

            // collect the outer loops, which are not contained in any other loop
            for (var i = 0; i < tesselatedEdgeLoops.Count; i++)
            {
                var edgeLoop = tesselatedEdgeLoops[i];

                var isOuter = tesselatedEdgeLoops.Where((_, j) => i != j)
                    .All(bound => !edgeLoop.Item2.IsContainedIn(bound.Item2));

                if (isOuter)
                {
                    outerEdgeLoops.Add(edgeLoop);
                }
                else
                {
                    innerEdgeLoops.Add(edgeLoop);
                }
            }

            var partitionedLoops = new List<List<List<Edge>>>();

            // for each outer loop, collect the loops that are inside of it
            // forming the partitions of the original edge loop set
            foreach (var outerLoop in outerEdgeLoops)
            {
                var comp = new List<List<Edge>> { outerLoop.Item1 };
                var mask = new List<bool>();

                foreach (var innerLoop in innerEdgeLoops)
                {
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

                // remove innerEdge loops that have already been added to a partition
                innerEdgeLoops = innerEdgeLoops.Where((_, j) => mask[j]).ToList();

                // add the new partition
                partitionedLoops.Add(comp);
            }

            return partitionedLoops;
        }

        public static List<List<Autodesk.Revit.DB.Edge>> GetAllEdgeLoopsFromRevitFace(Face face)
        {
            return face.EdgeLoops.Cast<EdgeArray>()
                .Select(x => x.Cast<Autodesk.Revit.DB.Edge>().ToList())
                .ToList();
        }

        private static IEnumerable<Autodesk.Revit.DB.UV> TesselateEdgeLoopOnFace(IEnumerable<Autodesk.Revit.DB.Edge> edgeLoop, Autodesk.Revit.DB.Face face)
        {
            return edgeLoop.SelectMany(x => x.TessellateOnFace(face).TakeAllButLast());
        }

        private static bool IsContainedIn(this IEnumerable<UV> edgeLoop, List<UV> poly)
        {
            return edgeLoop.All(x => x.IsContainedIn(poly));
        }

        private static bool IsContainedIn(this UV edgeVertex, List<UV> poly)
        {
            return PolygonContainment.PolygonContains(poly, edgeVertex);
        }
    }

}
