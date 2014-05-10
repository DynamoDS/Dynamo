using System.IO;
using System.Linq;
using Dynamo.DSEngine;
using Dynamo.Utilities;

namespace Dynamo
{
    public static class STLExport
    {
        public static void ExportToSTL(string path, string modelName)
        {
            var vis = dynSettings.Controller.VisualizationManager;

            var packages = dynSettings.Controller.DynamoModel.Nodes
                .Where(node => node.HasRenderPackages)
                .SelectMany(rp=>rp.RenderPackages)
                .Cast<RenderPackage>()
                .Where(rp=>rp.TriangleVertices.Count % 9 == 0)
                .ToList();

            var n = packages.SelectMany(rp => rp.TriangleNormals).ToList();
            var v = packages.SelectMany(rp => rp.TriangleVertices).ToList();

            using (TextWriter tw = new StreamWriter(path))
            {
                tw.WriteLine(string.Format("solid {0}", modelName));

                int nCount = 0;
                for (int i = 0; i < v.Count(); i += 9)
                {
                    tw.WriteLine(string.Format("\tfacet normal {0} {1} {2}", n[nCount], n[nCount+1], n[nCount+2]));
                    tw.WriteLine("\t\touter loop");
                    tw.WriteLine(string.Format("\t\t\tvertex {0} {1} {2}", v[i], v[i+1], v[i+2]));
                    tw.WriteLine(string.Format("\t\t\tvertex {0} {1} {2}", v[i+3], v[i+4], v[i+5]));
                    tw.WriteLine(string.Format("\t\t\tvertex {0} {1} {2}", v[i+6], v[i+7], v[i+8]));
                    tw.WriteLine("\t\tendloop");
                    tw.WriteLine("\tendfacet");

                    nCount += 3;
                }
                tw.WriteLine(string.Format("endsolid {0}", modelName));
            }
        }
    }
}
