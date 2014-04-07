using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Dynamo.DSEngine
{
    public static class DocumentationServices
    {
        private static Dictionary<string, bool> _triedPaths = new Dictionary<string, bool>();

        private static Dictionary<string, XDocument> _cached =
            new Dictionary<string, XDocument>(StringComparer.OrdinalIgnoreCase);

        public static XDocument GetForAssembly(string assemblyPath)
        {
            if (_triedPaths.ContainsKey(assemblyPath))
            {
                return _triedPaths[assemblyPath] ? _cached[assemblyPath] : null;
            }

            var documentationPath = "";
            if (ResolveForAssembly(assemblyPath, ref documentationPath))
            {
                var c = XDocument.Load(documentationPath);
                _triedPaths.Add(assemblyPath, true);
                _cached.Add(assemblyPath, c);
                return c;
            }
            else
            {
                _triedPaths.Add(assemblyPath, false);
                return null;
            }
        }

        public static bool ResolveForAssembly(string assemblyLocation, ref string documentationPath)
        {
            Dynamo.Nodes.Utilities.ResolveLibraryPath(ref assemblyLocation);

            if (!File.Exists(assemblyLocation))
            {
                return false;
            }

            var qualifiedPath = Path.GetFullPath(assemblyLocation);
            var fn = Path.GetFileNameWithoutExtension(qualifiedPath);
            var dir = Path.GetDirectoryName(qualifiedPath);

            fn = fn + ".xml";

            documentationPath = Path.Combine(dir, fn);

            return File.Exists(documentationPath);
        }
    }

}
