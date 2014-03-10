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
            var fn = Path.GetFileName(qualifiedPath);
            var dir = Path.GetDirectoryName(qualifiedPath);

            fn = "DynamoCustomization_" + fn.Replace(".dll", ".xml").Replace(".ds", ".xml");

            customizationPath = Path.Combine(dir, fn);

            return File.Exists(customizationPath);
        }

        public string GetNamespaceCategory(string namespaceName)
        {
            return XmlDocument.XPathEvaluate(
                String.Format("string(/doc/namespaces/namespace[@name='{0}']/category)", namespaceName)
                ).ToString().Trim();
        }
    }
}
