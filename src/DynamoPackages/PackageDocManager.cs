using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.PackageManager
{
    public class PackageDocManager
    {
        private Dictionary<string, string> nodeDocumentationDict = new Dictionary<string, string>();
        private Dictionary<string, FileSystemWatcher> watchers = new Dictionary<string, FileSystemWatcher>();

        private static PackageDocManager instance;

        public static PackageDocManager Instance
        {
            get
            {
                if (instance is null) { instance = new PackageDocManager(); }
                return instance;
            }
        }

        private PackageDocManager() { }

        public string GetAnnotationDoc(string nodeNamespace)
        {
            string output;
            nodeDocumentationDict.TryGetValue(nodeNamespace, out output);
            return output;
        }

        public FileSystemWatcher GetFileSystemWatcher(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            FileSystemWatcher watcher;
            watchers.TryGetValue(directory, out watcher);
            return watcher;
        }

        public void AddPackageDocumentation(Package package)
        {
            if (!package.NodeAnnotationPaths.Any())
                return;

            foreach (var path in package.NodeAnnotationPaths)
            {
                var directoryInfo = new DirectoryInfo(path);
                MonitorDirectory(directoryInfo);
                var files = directoryInfo.GetFiles("*.md", SearchOption.AllDirectories);
                AddDocumentationToDictionary(files);
            }
        }

        private void MonitorDirectory(DirectoryInfo directoryInfo)
        {
            if (watchers.ContainsKey(directoryInfo.FullName))
                return;
            
            var watcher = new FileSystemWatcher(directoryInfo.FullName, "*.md") { EnableRaisingEvents = true };
            watcher.Renamed += OnFileRenamed;
            watcher.Deleted += OnFileDeleted;
            watcher.Created += OnFileCreated;
            watchers.Add(watcher.Path, watcher);
        }

        private void AddDocumentationToDictionary(FileInfo[] files)
        {
            foreach (var file in files)
            {
                nodeDocumentationDict.Add(Path.GetFileNameWithoutExtension(file.Name), file.FullName);
            }
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            nodeDocumentationDict.Add(e.Name, e.FullPath);
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            if (nodeDocumentationDict.ContainsKey(e.Name))
                nodeDocumentationDict.Remove(e.Name);
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            if (nodeDocumentationDict.ContainsKey(e.OldName))
                nodeDocumentationDict.Remove(e.OldName);

            if (!nodeDocumentationDict.ContainsKey(e.Name))
                nodeDocumentationDict.Add(e.Name, e.FullPath);
        }
    }
}
