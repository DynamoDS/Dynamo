using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using Dynamo.Logging;

namespace Dynamo.Wpf.Extensions
{
    public class ViewExtensionLoader : IViewExtensionLoader, ILogSource
    {
        private IViewExtension Load(ViewExtensionDefinition viewExtension)
        {
            try
            {
                var assembly = Assembly.LoadFrom(viewExtension.AssemblyPath);
                var result = assembly.CreateInstance(viewExtension.TypeName) as IViewExtension;
                ExtensionLoading?.Invoke(result);
                return result;
            }
            catch
            {
                var name = viewExtension.TypeName == null ? "null" : viewExtension.TypeName;
                Log("Could not create an instance of " + name);
                return null;
            }
        }

        public IViewExtension Load(string extensionPath)
        {
            var document = new XmlDocument();
            document.Load(extensionPath);

            var topNode = document.GetElementsByTagName("ViewExtensionDefinition");

            if (topNode.Count == 0)
            {
                Log("Malformed " + extensionPath + " file");
                return null;
            }

            var definition = new ViewExtensionDefinition();
            var path = Path.GetDirectoryName(extensionPath);
            foreach (XmlNode item in topNode[0].ChildNodes)
            {
                if (item.Name == "AssemblyPath")
                {
                    path = Path.Combine(path, item.InnerText);
                    definition.AssemblyPath = path;
                }
                else if (item.Name == "TypeName")
                {
                    definition.TypeName = item.InnerText;
                }
            }

            var extension = Load(definition);
            return extension;
        }

        public IEnumerable<IViewExtension> LoadDirectory(string extensionsPath)
        {
            var result = new List<IViewExtension>();

            if (Directory.Exists(extensionsPath))
            {
                var files = Directory.GetFiles(extensionsPath, "*_ViewExtensionDefinition.xml");
                foreach (var file in files)
                {
                    var extension = Load(file);
                    if (extension != null)
                    {
                        result.Add(extension);
                    }
                }
            }

            return result;
        }

        public event Action<ILogMessage> MessageLogged;

        private void Log(ILogMessage obj)
        {
            if (MessageLogged != null)
            {
                MessageLogged(obj);
            }
        }

        private void Log(string msg)
        {
            Log(LogMessage.Info(msg));
        }

        /// <summary>
        /// An event that is raised when an extension starts loading.
        /// </summary>
        public event Action<IViewExtension> ExtensionLoading;
    }
}
