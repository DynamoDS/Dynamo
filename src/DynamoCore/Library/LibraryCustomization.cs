using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

using DynamoUtilities;

namespace Dynamo.DSEngine
{
    public class LibraryCustomizationServices
    {
        private static Dictionary<string, bool> triedPaths = new Dictionary<string, bool>();
        private static Dictionary<string, LibraryCustomization> cache = new Dictionary<string, LibraryCustomization>();

        public static LibraryCustomization GetForAssembly(string assemblyPath)
        {
            if (triedPaths.ContainsKey(assemblyPath))
            {
                return triedPaths[assemblyPath] ? cache[assemblyPath] : null;
            }

            var customizationPath = "";
            if (ResolveForAssembly(assemblyPath, ref customizationPath))
            {
                var c = new LibraryCustomization(XDocument.Load(customizationPath));
                triedPaths.Add(assemblyPath, true);
                cache.Add(assemblyPath, c);
                return c;
            }

            triedPaths.Add(assemblyPath, false);
            return null;
            
        }

        public static bool ResolveForAssembly(string assemblyLocation, ref string customizationPath)
        {
            if (!DynamoPathManager.Instance.ResolveLibraryPath(ref assemblyLocation))
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
    }

    public class LibraryCustomization
    {
        private XDocument XmlDocument;

        internal LibraryCustomization(XDocument document)
        {
            this.XmlDocument = document;
        }

        public string GetNamespaceCategory(string namespaceName)
        {
            return XmlDocument.XPathEvaluate(
                String.Format("string(/doc/namespaces/namespace[@name='{0}']/category)", namespaceName)
                ).ToString().Trim();

            //var nodes = (IEnumerable<Object>)XmlDocument.XPathEvaluate(
            //    String.Format("/doc/namespaces/namespace", namespaceName)
            //    );

            //foreach (var node in nodes)
            //{
            //}

            //return nodes.ToString().Trim();

        }
    }
}
