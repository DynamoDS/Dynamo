using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dynamo.Utilities;
using Greg.Requests;
using Microsoft.Practices.Prism.ViewModel;
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

        private string _rootDirectory;
        public string RootDirectory { get { return _rootDirectory; } set { _rootDirectory = value; RaisePropertyChanged("RootDirectory"); } }

        private string _versionName;
        public string VersionName { get { return _versionName; } set { _versionName = value; RaisePropertyChanged("VersionName"); } }

        public List<Type> LoadedTypes { get; set; }
        public List<CustomNodeInfo> LoadedCustomNodes { get; set; }

        public LocalPackage(string directory, string name, string versionName)
        {
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
            }
            catch (Exception e)
            {
               
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
            var des = new RestSharp.Deserializers.JsonDeserializer();

            var pkgHeader = File.ReadAllText(headerPath);
            var res = new RestResponse();
            res.Content = pkgHeader;

            var body = des.Deserialize<PackageUploadRequestBody>(res);

            return new LocalPackage(Path.GetDirectoryName(headerPath), body.name, body.version);

        }
    }
}
