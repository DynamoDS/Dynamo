using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.PackageManager
{
    /// <summary>
    /// Creates a lookup for node annotation markdown files associated with a package.
    /// Also creates file watchers for the package node annotation paths so new files can be hot reloaded in Dynamo.
    /// </summary>
    public class PackageDocManager
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
        private static PackageDocManager instance;

        /// <summary>
        /// PackageDocManager singleton instance.
        /// </summary>
        public static PackageDocManager Instance
        {
            get
            {
                if (instance is null) { instance = new PackageDocManager(); }
                return instance;
            }
        }

        private PackageDocManager() { }

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
        /// Note this only works for Markdown files and for package that specify a node annotation path in the pkg json.
        /// </summary>
        /// <param name="package"></param>
        internal void AddPackageDocumentation(Package package)
        {
            if (!package.NodeAnnotationPaths.Any())
                return;

            //foreach (var path in package.NodeAnnotationPaths)
            //{
            //    var directoryInfo = new DirectoryInfo(path);
            //    MonitorDirectory(directoryInfo);
            //    var files = directoryInfo.GetFiles(fileExtension, SearchOption.AllDirectories);
            //    TrackDocumentationFiles(files);
            //}

            var directoryInfo = new DirectoryInfo(package.NodeDocumentaionDirectory);
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
    }
}
