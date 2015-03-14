using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Dynamo.Interfaces;
using DynamoUtilities;

namespace Dynamo.DSEngine
{
    public static class DocumentationServices
    {
        private static Dictionary<string, bool> _triedPaths = new Dictionary<string, bool>();

        private static Dictionary<string, XDocument> _cached =
            new Dictionary<string, XDocument>(StringComparer.OrdinalIgnoreCase);

        public static void DestroyCachedData()
        {
            if (_triedPaths != null) // Release references for collection.
                _triedPaths.Clear();

            if (_cached != null) // Release references for collection.
                _cached.Clear();
        }

        public static XDocument GetForAssembly(string assemblyPath, IPathManager pathManager)
        {
            if (_triedPaths.ContainsKey(assemblyPath))
            {
                return _triedPaths[assemblyPath] ? _cached[assemblyPath] : null;
            }

            var documentationPath = "";
            if (ResolveForAssembly(assemblyPath, pathManager, ref documentationPath))
            {
                var c = XDocument.Load(documentationPath);
                _triedPaths.Add(assemblyPath, true);
                _cached.Add(assemblyPath, c);
                return c;
            }

            _triedPaths.Add(assemblyPath, false);
            return null;
        }

        private static bool ResolveForAssembly(string assemblyLocation,
            IPathManager pathManager, ref string documentationPath)
        {
            if (pathManager != null)
                pathManager.ResolveLibraryPath(ref assemblyLocation);

            if (!File.Exists(assemblyLocation))
            {
                return false;
            }

            var assemblyPath = Path.GetFullPath(assemblyLocation);

            var baseDir = Path.GetDirectoryName(assemblyPath);
            var xmlFileName = Path.GetFileNameWithoutExtension(assemblyPath) + ".xml";

            var language = System.Threading.Thread.CurrentThread.CurrentUICulture.ToString();
            var localizedResPath = Path.Combine(baseDir, language);
            documentationPath = Path.Combine(localizedResPath, xmlFileName);

            if (File.Exists(documentationPath))
                return true;

            localizedResPath = Path.Combine(baseDir, UI.Configurations.FallbackUiCulture);
            documentationPath = Path.Combine(localizedResPath, xmlFileName);
            if (File.Exists(documentationPath))
                return true;

            documentationPath = Path.Combine(baseDir, xmlFileName);
            return File.Exists(documentationPath);
        }
    }

}
