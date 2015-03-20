using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;
using Dynamo.Interfaces;
using Dynamo.UI;
using DynamoUtilities;

namespace Dynamo.DSEngine
{
    public class LibraryCustomizationServices
    {
        private static Dictionary<string, bool> triedPaths = new Dictionary<string, bool>();
        private static Dictionary<string, LibraryCustomization> cache = new Dictionary<string, LibraryCustomization>();

        public static LibraryCustomization GetForAssembly(string assemblyPath, IPathManager pathManager)
        {
            if (triedPaths.ContainsKey(assemblyPath))
            {
                return triedPaths[assemblyPath] ? cache[assemblyPath] : null;
            }

            var customizationPath = "";
            var resourceAssemblyPath = "";

            XDocument xDocument = null;
            Assembly resAssembly = null;

            if (ResolveForAssembly(assemblyPath, pathManager, ref customizationPath))
            {
                xDocument = XDocument.Load(customizationPath);
            }

            if (ResolveResourceAssembly(assemblyPath, pathManager, out resourceAssemblyPath))
            {
                resAssembly = Assembly.LoadFrom(resourceAssemblyPath);
            }

            // We need 'LibraryCustomization' if either one is not 'null'
            if (xDocument != null || (resAssembly != null))
            {
                var c = new LibraryCustomization(resAssembly, xDocument);
                triedPaths.Add(assemblyPath, true);
                cache.Add(assemblyPath, c);
                return c;
            }

            triedPaths.Add(assemblyPath, false);
            return null;

        }

        private static bool ResolveForAssembly(string assemblyLocation,
            IPathManager pathManager, ref string customizationPath)
        {
            try
            {
                if ((pathManager != null) && (!pathManager.ResolveLibraryPath(ref assemblyLocation)))
                {
                    return false;
                }

                var qualifiedPath = Path.GetFullPath(assemblyLocation);
                var fn = Path.GetFileNameWithoutExtension(qualifiedPath);
                var dir = Path.GetDirectoryName(qualifiedPath);

                fn = fn + "_DynamoCustomization.xml";

                customizationPath = Path.Combine(dir, fn);

                return File.Exists(customizationPath);
            }
            catch
            {
                // Just to be sure, that nothing will be crashed.
                customizationPath = "";
                return false;
            }
        }

        private static bool ResolveResourceAssembly(
            string assemblyLocation,
            IPathManager pathManager,
            out string resourceAssemblyPath)
        {
            try
            {
                var fn = Path.GetFileNameWithoutExtension(assemblyLocation);
                // First try side-by-side search for customization dll.
                var dirName = Path.GetDirectoryName(assemblyLocation);
                resourceAssemblyPath = Path.Combine(dirName, fn + Configurations.IconResourcesDLL);

                if (File.Exists(resourceAssemblyPath))
                    return true;
                if (pathManager == null) // Can't resolve path without IPathManager.
                    return false;

                resourceAssemblyPath = fn + Configurations.IconResourcesDLL;
                // Side-by-side customization dll not found, try other resolution paths.
                return pathManager.ResolveLibraryPath(ref resourceAssemblyPath);
            }
            catch
            {
                // Just to be sure, that nothing will be crashed.
                resourceAssemblyPath = "";
                return false;
            }
        }
    }

    public class LibraryCustomization
    {
        private readonly Assembly resourceAssembly;
        private readonly XDocument xmlDocument;

        /// <summary>
        /// Resources assembly. Assembly where icons are saved.
        /// </summary>
        public Assembly ResourceAssembly { get { return resourceAssembly; } }

        internal LibraryCustomization(Assembly resAssembly, XDocument document)
        {
            this.xmlDocument = document;
            this.resourceAssembly = resAssembly;
        }

        public string GetNamespaceCategory(string namespaceName)
        {
            var format = "string(/doc/namespaces/namespace[@name='{0}']/category)";
            object obj = String.Empty;
            if (xmlDocument != null)
                obj = xmlDocument.XPathEvaluate(String.Format(format, namespaceName));
            return obj.ToString().Trim();
        }
    }
}
