using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Dynamo.Configuration;
using Dynamo.Interfaces;

namespace Dynamo.Engine
{
    internal static class DocumentationServices
    {
        private static Dictionary<string, bool> _triedPaths = new Dictionary<string, bool>();

        private static Dictionary<string, XmlReader> _cached =
            new Dictionary<string, XmlReader>(StringComparer.OrdinalIgnoreCase);

        public static void DestroyCachedData()
        {
            if (_triedPaths != null) // Release references for collection.
                _triedPaths.Clear();

            if (_cached != null) // Release references for collection.
                _cached.Clear();
        }

        public static XmlReader GetForAssembly(string assemblyPath, IPathManager pathManager)
        {
            if (_triedPaths.ContainsKey(assemblyPath))
            {
                return _triedPaths[assemblyPath] ? _cached[assemblyPath] : null;
            }

            var documentationPath = "";
            if (ResolveForAssembly(assemblyPath, pathManager, ref documentationPath))
            {
                var c = XmlReader.Create(documentationPath);
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
            var assemblyName = Path.GetFileNameWithoutExtension(assemblyLocation);
            if (pathManager != null)
            {
                pathManager.ResolveLibraryPath(ref assemblyLocation);
            }

            string baseDir = String.Empty;
            if (String.IsNullOrEmpty(assemblyLocation) || !File.Exists(assemblyLocation))
            {
                // Some nodes do not have a corresponding assembly, but their documentation 
                // xml file resides alongside DynamoCore.dll. If the assembly could not be 
                // located, fall back onto using DynamoCoreDirectory.
                baseDir = pathManager.DynamoCoreDirectory;
            }
            else
            {
                // Found the assembly location, search for documentation alongside it.
                baseDir = Path.GetDirectoryName(Path.GetFullPath(assemblyLocation));
            }

            var language = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            //try with the system culture
            var localizedDocPath = Path.Combine(baseDir, language);

            //try with the fallback culture
            var localizedFallbackDockPath = Path.Combine(baseDir, Configurations.FallbackUiCulture);

            var searchPaths = new List<string>() { localizedDocPath, localizedFallbackDockPath, baseDir };
            var extension = ".xml";

            documentationPath = FindFileInPaths(assemblyName, extension, searchPaths.ToArray());
            if(documentationPath != string.Empty)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// searchs for a file with an extension in a list of paths, returns the first match
        /// where the extension is case insensitive. If no file is found, returns an empty string.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="extension"></param>
        /// <param name="searchPaths"></param>
        /// <returns></returns>
        internal static string FindFileInPaths(string filename, string extension, string[] searchPaths)
        {
            foreach (var path in searchPaths)
            {
                if (Directory.Exists(path))
                {
                    var files = Directory.GetFiles(path);
                    //matches occur where filename and extension are the same when both are lowercased
                    var matches = files.ToList().Where(x => String.CompareOrdinal(Path.GetFileName(x).ToLower(), (filename + extension).ToLower()) == 0);
                    if (matches.Count() > 1)
                    {
                        Console.WriteLine(string.Format("While searching for {0}{1} in {2}, {3} matches were found, the first will be loaded",
                            filename, extension, path, matches.Count().ToString()));
                    }
                    if (matches.Count() > 0)
                    {
                        //found a match, return the first one
                        return matches.First();
                    }

                }
            }
            return string.Empty;
        }

    }
}
