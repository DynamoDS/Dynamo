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
        /// Checks if the nodeNamespace has a documentation markdown associated.
        /// </summary>
        /// <param name="nodeNamespace"></param>
        /// <returns></returns>
        public bool ContainsAnnotationDoc(string nodeNamespace)
        {
            return nodeDocumentationFileLookup.ContainsKey(nodeNamespace);
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
                    nodeDocumentationFileLookup.Add(Path.Combine(packageName,Path.GetFileNameWithoutExtension(file.Name)), file.FullName);
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
