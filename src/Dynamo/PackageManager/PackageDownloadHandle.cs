using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dynamo.Utilities;
using Greg.Requests;
using Greg.Responses;
using Microsoft.Practices.Prism.ViewModel;
using RestSharp;

namespace Dynamo.PackageManager
{
    public class PackageDownloadHandle : NotificationObject
    {
        public enum State
        {
            Uninitialized, Downloading, Downloaded, Installing, Installed, Error
        }

        private string _errorString = "";
        public string ErrorString { get { return _errorString; } set { _errorString = value; RaisePropertyChanged("ErrorString"); } }

        private State _downloadState = State.Uninitialized;

        public State DownloadState
        {
            get { return _downloadState; }
            set
            {
                _downloadState = value;
                RaisePropertyChanged("DownloadState");
            }
        }

        public Greg.Responses.PackageHeader Header { get; private set; }
        public string Name { get { return Header.name; } }

        private string _downloadPath;
        public string DownloadPath { get { return _downloadPath; } set { _downloadPath = value; RaisePropertyChanged("DownloadPath"); } }

        private string _versionName;
        public string VersionName { get { return _versionName; } set { _versionName = value; RaisePropertyChanged("VersionName"); } }

        private PackageDownloadHandle()
        {
            
        }

        public PackageDownloadHandle(Greg.Responses.PackageHeader header, string version)
        {
            this.Header = header;
            this.DownloadPath = "";
        }

        public void Start()
        {
            dynSettings.Controller.PackageManagerClient.DownloadAndInstall(this);
        }

        public void Error(string errorString)
        {
            this.DownloadState = State.Error;
            this.ErrorString = errorString;
        }
        
        public void Done( string filePath )
        {
            this.DownloadState = State.Downloaded;
            this.DownloadPath = filePath;
        }

        private string BuildInstallDirectoryString()
        {
            // assembly_path/dynamo_packages/package_name

            Assembly dynamoAssembly = Assembly.GetExecutingAssembly();
            string location = Path.GetDirectoryName(dynamoAssembly.Location);
            return Path.Combine(location, "dynamo_packages", this.Name);

        }

        public bool Extract( out DynamoInstalledPackage pkg )
        {
            if (this.DownloadState != State.Downloaded)
            {
                throw new Exception("The package cannot be extracted unless it is downloaded. ");
            }

            this.DownloadState = State.Installing;

            // unzip, place files
            var unzipPath = Greg.Utility.FileUtilities.UnZip(DownloadPath);
            if (!Directory.Exists(unzipPath))
            {
                throw new Exception("The package was found to be empty and was not installed.");
            }
            
            var installedPath = BuildInstallDirectoryString();
            Directory.CreateDirectory(installedPath);

            // Now create all of the directories
            foreach (string dirPath in Directory.GetDirectories(unzipPath, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(unzipPath, installedPath));

            // Copy all the files
            foreach (string newPath in Directory.GetFiles(unzipPath, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(unzipPath, installedPath));

            // provide handle to installed package 
            pkg = new DynamoInstalledPackage(installedPath, Header.name, VersionName);

            return true;
        }

        // cancel, install, redownload

    }

    public class DynamoInstalledPackage : NotificationObject
    {
        public string Name { get; set; }

        public string CustomNodeDirectory
        {
            get { return Path.Combine(this.Directory, "dyf"); }
        }

        public string BinaryDirectory
        {
            get { return Path.Combine(this.Directory, "bin"); }
        }

        private string _directory;
        public string Directory { get { return _directory; } set { _directory = value; RaisePropertyChanged("Directory"); } }

        private string _versionName;
        public string VersionName { get { return _versionName; } set { _versionName = value; RaisePropertyChanged("VersionName"); } }

        public List<Type> LoadedTypes { get; set; }
        public List<CustomNodeInfo> LoadedCustomNodes { get; set; } 

        public DynamoInstalledPackage(string directory, string name, string versionName )
        {
            this.Directory = directory;
            this.Name = name;
            this.VersionName = versionName;
            this.LoadedTypes = new List<Type>();
            this.LoadedCustomNodes = new List<CustomNodeInfo>();
        }

        public void RegisterWithHost()
        {
            dynSettings.Bench.Dispatcher.BeginInvoke((Action)(() =>
            {
                LoadedTypes = 
                    GetAssemblies().Select(DynamoLoader.LoadNodesFromAssembly).SelectMany(x => x).ToList();

                LoadedCustomNodes = DynamoLoader.LoadCustomNodes(CustomNodeDirectory).ToList();

            }));
        }

        public List<Assembly> GetAssemblies()
        {
            return
                (new DirectoryInfo(BinaryDirectory))
                    .EnumerateFiles("*.dll")
                    .Select((fileInfo) => Assembly.LoadFrom(fileInfo.FullName)).ToList();
        }
       
        // location of all files
        public void Uninstall()
        {
            throw new NotImplementedException();
            // remove this package completely
        }

        internal static DynamoInstalledPackage FromJson(string headerPath)
        {
            var des = new RestSharp.Deserializers.JsonDeserializer();

            var pkgHeader = File.ReadAllText(headerPath);
            var res = new RestResponse();
            res.Content = pkgHeader;

            var body = des.Deserialize<PackageUploadRequestBody>(res);

            return new DynamoInstalledPackage(Path.GetDirectoryName(headerPath), body.name, body.version);

        }
    }



}
