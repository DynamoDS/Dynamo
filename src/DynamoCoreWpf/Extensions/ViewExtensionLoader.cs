using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                if (viewExtension.RequiresSignedEntryPoint)
                {
                    CheckExtensionCertificates(viewExtension);
                }

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

            foreach (var pathToVerifyCert in DirectoriesToVerifyCertificates)
            {
                if (extensionPath.Contains(pathToVerifyCert))
                {
                    definition.RequiresSignedEntryPoint = true;
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

        /// <summary>
        /// A list of root directories which require extensions to have a signed entry point
        /// File path locations from package definition xml's are validated against this collection 
        /// </summary>
        internal List<string> DirectoriesToVerifyCertificates = new List<string>();

        /// <summary>
        /// Checks if the AssemblyPath defined in the view extension definition is a valid dll with valid certificate
        /// </summary>
        /// <param name="extension">The view extension to verify</param>
        /// <returns></returns>
        private static bool CheckExtensionCertificates(ViewExtensionDefinition viewExtension)
        {
            //Verify the node library exists in the package bin directory
            if (!File.Exists(viewExtension.AssemblyPath))
            {
                throw new Exception(String.Format(
                        "A view extension called {0} found at {1} is missing dlls which are defined in the view extension definition.  Ignoring it.",
                        viewExtension.TypeName, viewExtension.AssemblyPath));
            }

            //Verify that you can load the node library assembly into a Reflection only context
            Assembly asm;
            try
            {
                asm = Assembly.ReflectionOnlyLoadFrom(viewExtension.AssemblyPath);
            }
            catch
            {
                throw new Exception(String.Format(
                    "A view extension called {0} found at {1} has a dll which could not be loaded.  Ignoring it.",
                    viewExtension.TypeName, viewExtension.AssemblyPath));
            }

            //Verify the node library has a verified signed certificate
            var cert = asm.Modules.FirstOrDefault()?.GetSignerCertificate();
            if (cert != null)
            {
                var cert2 = new System.Security.Cryptography.X509Certificates.X509Certificate2(cert);
                if (cert2.Verify())
                {
                    return true;
                }
            }

            throw new Exception(String.Format(
                "A view extension called {0} found at {1} did not have a signed certificate.  Ignoring it.",
                viewExtension.TypeName, viewExtension.AssemblyPath));
        }
    }
}
