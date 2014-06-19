using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DSRevitNodesTests
{
    public static class AssemblyResolver
    {
        private static bool resolverSetup;
        public static void Setup()
        {
            if (resolverSetup) return;
            AppDomain.CurrentDomain.AssemblyResolve += ResolveProtoGeometry;
            resolverSetup = true;
        }

        private static Assembly  ResolveProtoGeometry(object sender, ResolveEventArgs args)
        {
            if (!args.Name.Contains("ProtoGeometry")) return null;

            // ProtoGeometry is in the parent directory - build that path
            // We do not use the DynamoUtilities resolver here to avoid introducting
            // unnecessary dependencies.
            var assemPath = Assembly.GetExecutingAssembly().Location;
            var assemDir = new DirectoryInfo( Path.GetDirectoryName(assemPath) );
            var parentAssemDir = assemDir.Parent.FullName;
            var protoGeomPath = Path.Combine(parentAssemDir, "ProtoGeometry.dll");

            return File.Exists(protoGeomPath) ? Assembly.LoadFrom(protoGeomPath) : null;
        }
    }
    
}
 