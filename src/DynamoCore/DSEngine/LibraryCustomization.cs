using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Dynamo.DSEngine
{
    public class LibraryCustomizationServices
    {
        private static Dictionary<string, bool> _triedLookups = new Dictionary<string, bool>();
        private static Dictionary<string, LibraryCustomization> _cachedCustomizations = new Dictionary<string, LibraryCustomization>();

        public static LibraryCustomization GetForAssembly(string assemblyPath)
        {
            if (_triedLookups.ContainsKey(assemblyPath))
            {
                return _triedLookups[assemblyPath] ? _cachedCustomizations[assemblyPath] : null;
            }

            var customizationPath = "";
            if (ResolveCustomizationPath(assemblyPath, ref customizationPath))
            {
                var c = new LibraryCustomization(XDocument.Load(customizationPath));
                _triedLookups.Add(assemblyPath, true);
                _cachedCustomizations.Add(assemblyPath, c);
                return c;
            }
            else
            {
                _triedLookups.Add(assemblyPath, false);
                return null;
            }
            
        }

        public static bool ResolveCustomizationPath(string assemblyLocation, ref string customizationPath)
        {
            LibraryServices.GetInstance().ResolveLibraryPath(ref assemblyLocation);

            if (!File.Exists(assemblyLocation))
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
