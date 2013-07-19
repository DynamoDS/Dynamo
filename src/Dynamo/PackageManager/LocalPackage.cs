using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Greg.Requests;
using Microsoft.Practices.Prism.Commands;
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

        private bool _typesVisibleInManager;
        public bool TypesVisibleInManager { get { return _typesVisibleInManager; } set { _typesVisibleInManager = value; RaisePropertyChanged("TypesVisibleInManager"); } }

        private string _rootDirectory;
        public string RootDirectory { get { return _rootDirectory; } set { _rootDirectory = value; RaisePropertyChanged("RootDirectory"); } }

        private string _versionName;
        public string VersionName { get { return _versionName; } set { _versionName = value; RaisePropertyChanged("VersionName"); } }

        public DelegateCommand ToggleTypesVisibleInManagerCommand { get; set; }
        public DelegateCommand GetLatestVersionCommand { get; set; }
        public DelegateCommand MakeNewVersionCommand { get; set; }
        public DelegateCommand UninstallCommand { get; set; }
        public DelegateCommand VoteCommand { get; set; }

        public ObservableCollection<Type> LoadedTypes { get; set; }
        public ObservableCollection<CustomNodeInfo> LoadedCustomNodes { get; set; }

        public LocalPackage(string directory, string name, string versionName)
        {
            this.Loaded = false;
            this.RootDirectory = directory;
            this.Name = name;
            this.VersionName = versionName;
            this.LoadedTypes = new ObservableCollection<Type>();
            this.LoadedCustomNodes = new ObservableCollection<CustomNodeInfo>();

            ToggleTypesVisibleInManagerCommand = new DelegateCommand(ToggleTypesVisibleInManager, CanToggleTypesVisibleInManager);
            GetLatestVersionCommand = new DelegateCommand(GetLatestVersion, CanGetLatestVersion);
            MakeNewVersionCommand = new DelegateCommand(MakeNewVersion, CanMakeNewVersion);
            UninstallCommand = new DelegateCommand(Uninstall, CanUninstall);
            VoteCommand = new DelegateCommand(Vote, CanVote);
        }

        public static LocalPackage FromJson(string headerPath)
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

        public void Load()
        {
            try
            {
                GetAssemblies().Select(DynamoLoader.LoadNodesFromAssembly).SelectMany(x => x).ToList().ForEach( x => LoadedTypes.Add(x));
                DynamoLoader.LoadCustomNodes(CustomNodeDirectory).ForEach(x => LoadedCustomNodes.Add(x));

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

        public bool InUse()
        {
            // get all of the function ids from the custom nodes in this package
            var guids = LoadedCustomNodes.Select(x => x.Guid);

            // check if any of the custom nodes is in a workspace
            var customNodeInUse =  dynSettings.Controller.DynamoViewModel.AllNodes.Where(x => x is dynFunction)
                                   .Cast<dynFunction>()
                                   .Any(x => guids.Contains(x.Definition.FunctionId));

            return (LoadedTypes.Any() || customNodeInUse) && Loaded;
        }

        private void Uninstall()
        {

            var res = MessageBox.Show("Are you sure you want to uninstall " + this.Name + "  " + this.VersionName + "?  This will delete the packages root directory.\n\n You can always redownload the package.", "Uninstalling Package", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (res == MessageBoxResult.No) return;

            try
            {
                LoadedCustomNodes.ToList().ForEach(x => dynSettings.CustomNodeLoader.Remove(x.Guid));
                dynSettings.PackageLoader.LocalPackages.Remove(this);
                Directory.Delete(this.RootDirectory, true);
            }
            catch( Exception e )
            {
                MessageBox.Show("Dynamo failed to uninstall the package.  You may need to delete the package's root directory manually.", "Uninstall Failure", MessageBoxButton.OK, MessageBoxImage.Error);
                DynamoLogger.Instance.Log("Exception when attempting to uninstall the package " + this.Name + " from " + this.RootDirectory);
                DynamoLogger.Instance.Log(e.GetType() + ": " + e.Message);
            }
            
        }

        private bool CanUninstall()
        {
            return !InUse();
        }

        private void Vote()
        {
            throw new NotImplementedException();
        }

        private bool CanVote()
        {
            return false;
        }

        private void MakeNewVersion()
        {
            throw new NotImplementedException();
        }

        private bool CanMakeNewVersion()
        {
            return false;
        }

        private void GetLatestVersion()
        {
            throw new NotImplementedException();
        }

        private bool CanGetLatestVersion()
        {
            return false;
        }

        private void ToggleTypesVisibleInManager()
        {
            this.TypesVisibleInManager = !this.TypesVisibleInManager;
        }

        private bool CanToggleTypesVisibleInManager()
        {
            return true;
        }

    }
}
