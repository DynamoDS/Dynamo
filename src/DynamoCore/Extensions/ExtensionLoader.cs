﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using Dynamo.Logging;
using DynamoUtilities;

namespace Dynamo.Extensions
{
    /// <summary>
    /// Provides functionality for loading Dynamo's extensions.
    /// This class loads formatted XMLs which contain information about
    /// *Extension.dll and type name of IExtension inheritor.
    /// 
    /// Example:
    /// <ExtensionDefinition>
    ///   <AssemblyPath>..\ExtensionName.dll</AssemblyPath>
    ///   <TypeName>Dynamo.ExtensionName.ExtensionTypeName</TypeName>
    /// </ExtensionDefinition>
    /// </summary>
    public class ExtensionLoader: IExtensionLoader, ILogSource
    {
        private IExtension Load(ExtensionDefinition extension)
        {
            try
            {
                if (extension.RequiresSignedEntryPoint)
                {
                    CertificateVerification.CheckAssemblyForValidCertificate(extension.AssemblyPath);
                }

                var assembly = Assembly.LoadFrom(extension.AssemblyPath);
                var result = assembly.CreateInstance(extension.TypeName) as IExtension;
                ExtensionLoading?.Invoke(result);
                return result;
            }
            catch(Exception ex)
            {
                var name = extension.TypeName == null ? "null" : extension.TypeName;
                Log("Could not create an instance of " + name);
                Log(ex.Message);
                Log(ex.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// Loads <see cref="IExtension"/> from assembly.
        /// </summary>
        /// <param name="extensionPath">Assembly full path</param>
        /// <returns>Loaded <see cref="IExtension"/></returns>
        public IExtension Load(string extensionPath)
        {
            var document = new XmlDocument();
            document.Load(extensionPath);

            var topNode = document.GetElementsByTagName("ExtensionDefinition");

            if (topNode.Count == 0)
            {
                Log("Malformed " + extensionPath + " file");
                return null;
            }

            var definition = new ExtensionDefinition();
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

            //Check if the extension definition file was located in a directory which requires certificate validation.
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

        /// <summary>
        /// Loads a collection of <see cref="IExtension"/> from given folder
        /// </summary>
        /// <param name="extensionsPath">Assemblies location folder</param>
        /// <returns>Loaded collection of <see cref="IExtension"/></returns>
        public IEnumerable<IExtension> LoadDirectory(string extensionsPath)
        {
            var result = new List<IExtension>();

            if (Directory.Exists(extensionsPath))
            {
                var files = Directory.GetFiles(extensionsPath, "*_ExtensionDefinition.xml");
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

        /// <summary>
        /// This event is used for logging messages.
        /// </summary>
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
        public event Action<IExtension> ExtensionLoading;

        /// <summary>
        /// A list of root directories which require extensions to have a signed entry point
        /// File path locations from package definition xml's are validated against this collection 
        /// </summary>
        internal List<string> DirectoriesToVerifyCertificates = new List<string>();

    }
}
