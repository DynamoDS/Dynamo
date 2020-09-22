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
        /// <param name="nodeNamespace">Namespace of the node to lookup documentaion for</param>
        /// <returns></returns>
        public string GetAnnotationDoc(string nodeNamespace)
        {
            string output;
            nodeDocumentationFileLookup.TryGetValue(nodeNamespace, out output);
            return output;
        }

        /// <summary>
        /// Retrieves the FileWatcher for the specified directory file path.
        /// </summary>
        /// <param name="filePath">File path to lookup associated file watcher.</param>
        /// <returns></returns>
        public FileSystemWatcher GetMarkdownFileWatcher(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            FileSystemWatcher watcher;
            markdownFileWatchers.TryGetValue(directory, out watcher);
            return watcher;
        }

        /// <summary>
        /// Add package node annotation markdown files to the node annotation lookup.
        /// Note this only works for Markdown files and for package that specify a node annotation path in the pkg json.
        /// </summary>
        /// <param name="package"></param>
        public void AddPackageDocumentation(Package package)
        {
            if (!package.NodeAnnotationPaths.Any())
                return;

            foreach (var path in package.NodeAnnotationPaths)
            {
                var directoryInfo = new DirectoryInfo(path);
                MonitorDirectory(directoryInfo);
                var files = directoryInfo.GetFiles(fileExtension, SearchOption.AllDirectories);
                AddDocumentationToDictionary(files);
            }
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

        private void AddDocumentationToDictionary(FileInfo[] files)
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
