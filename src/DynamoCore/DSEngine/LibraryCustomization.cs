using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Dynamo.DSEngine
{
    public class LibraryCustomization
    {
        private XDocument XmlDocument;

        private LibraryCustomization(XDocument document)
        {
            this.XmlDocument = document;
        }

        public static LibraryCustomization LoadFromXml(string customizationPath)
        {
            if (!File.Exists(customizationPath))
            {
                throw new ArgumentException("A customization file does not exists at: " + customizationPath);
            }

            return new LibraryCustomization(XDocument.Load(customizationPath));
        }

        public static bool ResolveCustomizationFile(string assemblyLocation, ref string customizationPath)
        {
            var qualifiedPath = Path.GetFullPath(assemblyLocation);
            var fn = Path.GetFileNameWithoutExtension(qualifiedPath);
            var dir = Path.GetDirectoryName(qualifiedPath);

            fn = fn + "_DynamoCustomization.xml";

            customizationPath = Path.Combine(dir, fn);

            return File.Exists(customizationPath);
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
