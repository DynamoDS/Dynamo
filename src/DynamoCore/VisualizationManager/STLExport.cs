using System.IO;
using System.Linq;
using Dynamo.Utilities;

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
                //write the solid name
                tw.WriteLine(string.Format("solid {0}", modelName));

                foreach (var mesh in meshes)
                {
                    for (int i = 0; i < mesh.TriangleIndices.Count; i += 3)
                    {
                        var a = mesh.Positions[mesh.TriangleIndices[i]];
                        var b = mesh.Positions[mesh.TriangleIndices[i+1]];
                        var c = mesh.Positions[mesh.TriangleIndices[i+2]];

                        tw.WriteLine("\tfacet");
                        tw.WriteLine("\t\touter loop");
                        tw.WriteLine(string.Format("\t\t\t{0} {1} {2}", a.X, a.Y, a.Z));
                        tw.WriteLine(string.Format("\t\t\t{0} {1} {2}", b.X, b.Y, b.Z));
                        tw.WriteLine(string.Format("\t\t\t{0} {1} {2}", c.X, c.Y, c.Z));
                        tw.WriteLine("\t\tend loop");
                        tw.WriteLine("\tendfacet");
                    }
                }

                tw.WriteLine("endsolid");
            }
        }
    }
}
