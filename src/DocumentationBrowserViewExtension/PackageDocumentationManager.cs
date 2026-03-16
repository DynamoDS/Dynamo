using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Dynamo.Interfaces;
using Dynamo.Logging;
using Dynamo.Utilities;

namespace Dynamo.DocumentationBrowser
{
    /// <summary>
    /// Creates a lookup for node annotation markdown files associated with a package.
    /// Also creates file watchers for the package node annotation paths so new files can be hot reloaded in Dynamo.
    /// </summary>
    public class PackageDocumentationManager : IDisposable
    {
        /// <summary>
        /// Dictionary to lookup node documentation markdown files with the node namespace.
        /// key: node namespace - value: file path to markdown file.
        /// </summary>
        private Dictionary<string, string> nodeDocumentationFileLookup = new Dictionary<string, string>();

        /// <summary>
        /// Dictionary to keep track of each FileSystemWatcher created for the package node annotation directory path
        /// key: directory path - value: FileSystemWatcher
        /// </summary>
        private Dictionary<string, FileSystemWatcher> markdownFileWatchers = new Dictionary<string, FileSystemWatcher>();

        /// <summary>
        /// Map of package name to its node documentation directory path. Used to resolve hash-named doc files in packages.
        /// </summary>
        private Dictionary<string, string> packageDocDirectories = new Dictionary<string, string>();

        private const string VALID_DOC_FILEEXTENSION = "*.md";
        private const string FALLBACK_DOC_DIRECTORY_NAME = "fallback_docs";
        private static PackageDocumentationManager instance;
        //these fields should only be directly set by tests.
        internal DirectoryInfo dynamoCoreFallbackDocPath;
        internal DirectoryInfo hostDynamoFallbackDocPath;

        /// <summary>
        /// PackageDocManager singleton instance.
        /// </summary>
        public static PackageDocumentationManager Instance
        {
            get
            {
                if (instance is null) { instance = new PackageDocumentationManager(); }
                return instance;
            }
        }

        internal Action<ILogMessage> MessageLogged;

        /// <summary>
        /// Uses the path manager to set the fallback_doc path for DynamoCore and any Host running
        /// </summary>
        /// <param name="pathManager"></param>
        internal void AddDynamoPaths(IPathManager pathManager)
        {
            if (pathManager is null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(pathManager.DynamoCoreDirectory))
            {
                var coreDir = new DirectoryInfo(Path.Combine(pathManager.DynamoCoreDirectory, Thread.CurrentThread.CurrentCulture.ToString(), FALLBACK_DOC_DIRECTORY_NAME));
                if (!coreDir.Exists)
                {
                    coreDir = new DirectoryInfo(Path.Combine(pathManager.DynamoCoreDirectory, "en-US", FALLBACK_DOC_DIRECTORY_NAME));
                }
                dynamoCoreFallbackDocPath = coreDir.Exists ? coreDir : null;
            }

            if (!string.IsNullOrEmpty(pathManager.HostApplicationDirectory))
            {
                var hostDir = new DirectoryInfo(Path.Combine(pathManager.HostApplicationDirectory, Thread.CurrentThread.CurrentCulture.ToString(), FALLBACK_DOC_DIRECTORY_NAME));
                if (!hostDir.Exists)
                {
                    hostDir = new DirectoryInfo(Path.Combine(pathManager.HostApplicationDirectory, "en-US", FALLBACK_DOC_DIRECTORY_NAME));
                }
                hostDynamoFallbackDocPath = hostDir.Exists ? hostDir : null;
            }   
        }

        private PackageDocumentationManager() { }

        /// <summary>
        /// Retrieves the markdown node documentation file associated with the input node namespace
        /// </summary>
        /// <param name="nodeNamespace">Namespace of the node to lookup documentation for</param>
        /// <returns></returns>
        public string GetAnnotationDoc(string nodeNamespace, string packageName)
        {
            if(nodeDocumentationFileLookup
                .TryGetValue(Path.Combine(packageName, nodeNamespace), 
                out string output))
            {
                return output;
            }

            var shortName = Hash.GetHashFilenameFromString(nodeNamespace);
            FileInfo matchingDoc = null;

            // Try hash-named file in package doc directory (packages may use hash filenames for long paths)
            if (!string.IsNullOrEmpty(packageName) && packageDocDirectories.TryGetValue(packageName, out var pkgDocPath))
            {
                var pkgDir = new DirectoryInfo(pkgDocPath);
                if (pkgDir.Exists)
                {
                    matchingDoc = pkgDir.GetFiles($"{shortName}.md", SearchOption.AllDirectories).FirstOrDefault();
                    if (matchingDoc != null)
                    {
                        // Cache so future lookups and file watcher logic resolve correctly
                        nodeDocumentationFileLookup[Path.Combine(packageName, nodeNamespace)] = matchingDoc.FullName;
                        return matchingDoc.FullName;
                    }
                }
            }

            if (hostDynamoFallbackDocPath != null)
            {
                matchingDoc = hostDynamoFallbackDocPath.GetFiles($"{shortName}.md").FirstOrDefault() ??
                              hostDynamoFallbackDocPath.GetFiles($"{nodeNamespace}.md").FirstOrDefault();
                if (matchingDoc != null)
                {
                    return matchingDoc.FullName;
                }
            }

            if (dynamoCoreFallbackDocPath != null)
            {
                matchingDoc = dynamoCoreFallbackDocPath.GetFiles($"{shortName}.md").FirstOrDefault() ??
                              dynamoCoreFallbackDocPath.GetFiles($"{nodeNamespace}.md").FirstOrDefault();
            }

            return matchingDoc is null ? string.Empty : matchingDoc.FullName;
        }

        /// <summary>
        /// Checks if the node has a documentation markdown associated (by direct lookup or hash-named file in package/fallback).
        /// </summary>
        /// <param name="nodeNamespace">Namespace (e.g. minimum qualified name) of the node.</param>
        /// <param name="packageName">Package name if the node is from a package; otherwise empty.</param>
        /// <returns>True if documentation can be resolved for this node.</returns>
        public bool ContainsAnnotationDoc(string nodeNamespace, string packageName)
        {
            return !string.IsNullOrEmpty(GetAnnotationDoc(nodeNamespace, packageName));
        }

        /// <summary>
        /// Add package node annotation markdown files to the node annotation lookup.
        /// Note this only works for Markdown files.
        /// </summary>
        /// <param name="package"></param>
        internal void AddPackageDocumentation(string packageDocumentationPath, string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageDocumentationPath))
                return;

            var directoryInfo = new DirectoryInfo(packageDocumentationPath);
            if (!directoryInfo.Exists)
                return;

            MonitorDirectory(directoryInfo);
            packageDocDirectories[packageName] = directoryInfo.FullName;
            var files = directoryInfo.GetFiles(VALID_DOC_FILEEXTENSION, SearchOption.AllDirectories);
            TrackDocumentationFiles(files, packageName);
        }

        private void MonitorDirectory(DirectoryInfo directoryInfo)
        {
            if (markdownFileWatchers.ContainsKey(directoryInfo.FullName))
                return;
            
            var watcher = new FileSystemWatcher(directoryInfo.FullName, VALID_DOC_FILEEXTENSION) { EnableRaisingEvents = true };
            watcher.Renamed += OnFileRenamed;
            watcher.Deleted += OnFileDeleted;
            watcher.Created += OnFileCreated;
            markdownFileWatchers.Add(watcher.Path, watcher);
        }

        private void TrackDocumentationFiles(FileInfo[] files, string packageName)
        {
            foreach (var file in files)
            {
                try
                {
                    var key = Path.Combine(packageName, Path.GetFileNameWithoutExtension(file.Name));
                    nodeDocumentationFileLookup[key] = file.FullName;
                }
                catch (Exception e)
                {
                    LogWarning(e.Message, WarningLevel.Error);
                }
            }
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            nodeDocumentationFileLookup.Add(Path.GetFileNameWithoutExtension(e.Name), e.FullPath);
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            var fileName = Path.GetFileNameWithoutExtension(e.Name);
            if (nodeDocumentationFileLookup.ContainsKey(fileName))
            {
                nodeDocumentationFileLookup.Remove(fileName);
            }
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            var oldFileName = Path.GetFileNameWithoutExtension(e.OldName);
            if (nodeDocumentationFileLookup.ContainsKey(oldFileName))
            {
                nodeDocumentationFileLookup.Remove(oldFileName);
            }

            var newFileName = Path.GetFileNameWithoutExtension(e.Name);
            if (!nodeDocumentationFileLookup.ContainsKey(newFileName))
            {
                nodeDocumentationFileLookup.Add(newFileName, e.FullPath);
            }
        }

        private void LogWarning(string msg, WarningLevel level) => this.MessageLogged?.Invoke(LogMessage.Warning(msg, level));

        public void Dispose()
        {
            foreach (var watcher in markdownFileWatchers.Values)
            {
                watcher.Renamed -= OnFileRenamed;
                watcher.Deleted -= OnFileDeleted;
                watcher.Created -= OnFileCreated;
            }
        }
    }
}
