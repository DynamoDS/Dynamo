using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private const string fileExtension = "*.md";
        private static PackageDocumentationManager instance;

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

        private PackageDocumentationManager() { }

        /// <summary>
        /// Retrieves the markdown node documentation file associated with the input node namespace
        /// </summary>
        /// <param name="nodeNamespace">Namespace of the node to lookup documentation for</param>
        /// <returns></returns>
        public string GetAnnotationDoc(string nodeNamespace)
        {
            nodeDocumentationFileLookup.TryGetValue(nodeNamespace, out string output);
            return output;
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
        internal void AddPackageDocumentation(string packageDocumentationPath)
        {
            if (string.IsNullOrWhiteSpace(packageDocumentationPath))
                return;

            var directoryInfo = new DirectoryInfo(packageDocumentationPath);
            if (!directoryInfo.Exists)
                return;

            MonitorDirectory(directoryInfo);
            var files = directoryInfo.GetFiles(fileExtension, SearchOption.AllDirectories);
            TrackDocumentationFiles(files);
        }

        private void MonitorDirectory(DirectoryInfo directoryInfo)
        {
            if (markdownFileWatchers.ContainsKey(directoryInfo.FullName))
                return;
            
            var watcher = new FileSystemWatcher(directoryInfo.FullName, fileExtension) { EnableRaisingEvents = true };
            watcher.Renamed += OnFileRenamed;
            watcher.Deleted += OnFileDeleted;
            watcher.Created += OnFileCreated;
            markdownFileWatchers.Add(watcher.Path, watcher);
        }

        private void TrackDocumentationFiles(FileInfo[] files)
        {
            foreach (var file in files)
            {
                nodeDocumentationFileLookup.Add(Path.GetFileNameWithoutExtension(file.Name), file.FullName);
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
                nodeDocumentationFileLookup.Remove(fileName);
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            var oldFileName = Path.GetFileNameWithoutExtension(e.OldName);
            if (nodeDocumentationFileLookup.ContainsKey(oldFileName))
                nodeDocumentationFileLookup.Remove(oldFileName);

            var newFileName = Path.GetFileNameWithoutExtension(e.Name);
            if (!nodeDocumentationFileLookup.ContainsKey(newFileName))
                nodeDocumentationFileLookup.Add(newFileName, e.FullPath);
        }

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
