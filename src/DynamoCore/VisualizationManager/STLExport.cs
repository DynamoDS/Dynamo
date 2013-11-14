using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Utilities;
using QuantumConcepts.Formats.StereoLithography;
using QuantumConcepts.Common.Extensions;

namespace Dynamo
{
    public static class STLExport
    {
        public static void ExportToSTL(string path, string modelName)
        {
            var vis = dynSettings.Controller.VisualizationManager;

            //get all the meshes
            var meshes =
                vis.Visualizations.SelectMany(x => x.Value.Meshes)
                    .Concat(vis.Visualizations.SelectMany(x => x.Value.SelectedMeshes));

            using (TextWriter tw = new StreamWriter(path))
            {
                tw.WriteLine(string.Format("solid {0}", modelName));

                foreach (var mesh in meshes)
                {
                    for (int i = 0; i < mesh.TriangleIndices.Count; i += 3)
                    {
                        var a = mesh.Positions[mesh.TriangleIndices[i]];
                        var b = mesh.Positions[mesh.TriangleIndices[i+1]];
                        var c = mesh.Positions[mesh.TriangleIndices[i+2]];

                        tw.WriteLine("\tfacet normal 0 0 0");
                        tw.WriteLine("\t\touter loop");
                        tw.WriteLine(string.Format("\t\t\tvertex {0} {1} {2}", a.X, a.Y, a.Z));
                        tw.WriteLine(string.Format("\t\t\tvertex {0} {1} {2}", b.X, b.Y, b.Z));
                        tw.WriteLine(string.Format("\t\t\tvertex {0} {1} {2}", c.X, c.Y, c.Z));
                        tw.WriteLine("\t\tendloop");
                        tw.WriteLine("\tendfacet");
                    }
                }

                tw.WriteLine(string.Format("endsolid {0}", modelName));
            }
        }

        public static void ExportToSTLBinary(string path, string modelName)
        {
            var vis = dynSettings.Controller.VisualizationManager;

            //get all the meshes
            var meshes =
                vis.Visualizations.SelectMany(x => x.Value.Meshes)
                    .Concat(vis.Visualizations.SelectMany(x => x.Value.SelectedMeshes));

            //prepare a facet list
            var facets = new List<Facet>();
            foreach (var mesh in meshes)
            {
                for (int i = 0; i < mesh.TriangleIndices.Count; i += 3)
                {
                    var a = mesh.Positions[mesh.TriangleIndices[i]];
                    var b = mesh.Positions[mesh.TriangleIndices[i+1]];
                    var c = mesh.Positions[mesh.TriangleIndices[i+2]];

                    var verts = new List<Vertex>
                    {
                        new Vertex(Convert.ToDecimal(a.X), Convert.ToDecimal(a.Y), Convert.ToDecimal(a.Z)),
                        new Vertex(Convert.ToDecimal(b.X), Convert.ToDecimal(b.Y), Convert.ToDecimal(b.Z)),
                        new Vertex(Convert.ToDecimal(c.X), Convert.ToDecimal(c.Y), Convert.ToDecimal(c.Z))
                    };

                    facets.Add(new Facet(new Normal(0, 0, 0), verts, 0));
                }
            }

            var stl1 = new STLDocument("Test", facets);

            using (var stream = new FileStream(path,FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    stl1.Write(writer);
                    writer.Flush();
                }
            }
            
        }
    }
}
