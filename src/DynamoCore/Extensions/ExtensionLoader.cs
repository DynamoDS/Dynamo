using Dynamo.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Dynamo.Extensions
{
    public class ExtensionLoader: IExtensionLoader
    {
        private IExtension Load(ExtensionDefinition extension)
        {
            try
            {
                var assembly = Assembly.Load(extension.AssemblyName);
                var result = assembly.CreateInstance(extension.TypeName) as IExtension;
                return result;
            }
            catch
            {
                return null;
            }
        }

        public IEnumerable<IExtension> Load(string extensionsPath)
        {
            var result = new List<IExtension>();

            if (Directory.Exists(extensionsPath))
            {
                var files = Directory.GetFiles(extensionsPath, "*_ExtensionDefinition.xml");
                foreach (var file in files)
                {
                    var document = new XmlDocument();
                    document.Load(file);

                    var topNode = document.GetElementsByTagName("ExtensionDefinition");

                    if (topNode.Count == 0)
                    {
                        continue;
                    }

                    var definition = new ExtensionDefinition();
                    foreach (XmlNode item in topNode[0].ChildNodes)
                    {
                        if (item.Name == "AssemblyName")
                        {
                            definition.AssemblyName = item.InnerText;
                        }
                        else if (item.Name == "TypeName")
                        {
                            definition.TypeName = item.InnerText;
                        }
                    }

                    var extension = Load(definition);
                    if (extension != null)
                    {
                        result.Add(extension);
                    }
                }
            }

            return result;
        }
    }
}
