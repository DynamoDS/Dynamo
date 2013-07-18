using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dynamo.Utilities;
using Greg.Requests;
using Microsoft.Practices.Prism.ViewModel;
using Newtonsoft.Json;
using RestSharp;

namespace Dynamo.PackageManager
{
    public class LocalPackage : NotificationObject
    {
        public string Name { get; set; }

        public string CustomNodeDirectory
        {
            get { return Path.Combine(this.RootDirectory, "dyf"); }
        }

        public string BinaryDirectory
        {
            get { return Path.Combine(this.RootDirectory, "bin"); }
        }

        public bool Loaded { get; set; }

        private string _rootDirectory;
        public string RootDirectory { get { return _rootDirectory; } set { _rootDirectory = value; RaisePropertyChanged("RootDirectory"); } }

        private string _versionName;
        public string VersionName { get { return _versionName; } set { _versionName = value; RaisePropertyChanged("VersionName"); } }

        public List<Type> LoadedTypes { get; set; }
        public List<CustomNodeInfo> LoadedCustomNodes { get; set; }

        public LocalPackage(string directory, string name, string versionName)
        {
            this.Loaded = false;
            this.RootDirectory = directory;
            this.Name = name;
            this.VersionName = versionName;
            this.LoadedTypes = new List<Type>();
            this.LoadedCustomNodes = new List<CustomNodeInfo>();
        }

        public void Load()
        {
            try
            {
                LoadedTypes =
                    GetAssemblies().Select(DynamoLoader.LoadNodesFromAssembly).SelectMany(x => x).ToList();
                    
                LoadedCustomNodes = DynamoLoader.LoadCustomNodes(CustomNodeDirectory).ToList();

                Loaded = true;
            }
            catch (Exception e)
            {
                DynamoLogger.Instance.Log("Exception when attempting to load package " + this.Name + " from " + this.RootDirectory);
                DynamoLogger.Instance.Log(e.GetType() + ": " + e.Message);
            }

        }

        public List<Assembly> GetAssemblies()
        {
            if (!Directory.Exists(BinaryDirectory)) 
                return new List<Assembly>();

            return
                (new DirectoryInfo(BinaryDirectory))
                    .EnumerateFiles("*.dll")
                    .Select((fileInfo) => Assembly.LoadFrom(fileInfo.FullName)).ToList();
        }

        public bool ContainsFile(string path)
        {
            return Directory.EnumerateFiles(RootDirectory, "*", SearchOption.AllDirectories).Any(s => s == path);
        }

        public void Uninstall()
        {

            throw new NotImplementedException();

            // remove this package completely - we can dynamically remove custom nodes, but NOT loaded types
            // you'll need to make the user aware of this and cache the folder to remove a next startup

        }

        internal static LocalPackage FromJson(string headerPath)
        {

            try
            {
                var pkgHeader = File.ReadAllText(headerPath);
                var body = JsonConvert.DeserializeObject<PackageUploadRequestBody>(pkgHeader);

                if (body.name == null || body.version == null)
                {
                    throw new Exception("The header is missing a name or version field.");
                }

                return new LocalPackage(Path.GetDirectoryName(headerPath), body.name, body.version);
            }
            catch (Exception e)
            {
                DynamoLogger.Instance.Log("Failed to form package from json header.");
                DynamoLogger.Instance.Log(e.GetType() + ": " + e.Message);
                return null;
            }

        }

    }
}
