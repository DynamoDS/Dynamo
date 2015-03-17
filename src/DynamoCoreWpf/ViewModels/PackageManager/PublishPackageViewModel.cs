using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.PackageManager.UI;
using Dynamo.ViewModels;

using DynamoUtilities;

using Greg.Requests;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using Double = System.Double;
using String = System.String;
using Dynamo.Wpf.Properties;

namespace Dynamo.PackageManager
{
    public delegate void PublishSuccessHandler(PublishPackageViewModel sender);

    /// <summary>
    /// The ViewModel for Package publishing </summary>
    public class PublishPackageViewModel : NotificationObject
    {
        #region Properties/Fields

        private readonly DynamoViewModel dynamoViewModel;

        /// <summary>
        /// A event called when publishing was a success
        /// </summary>
        public event PublishSuccessHandler PublishSuccess;

        /// <summary>
        /// This dialog is in one of two states.  Uploading or the user is filling out the dialog
        /// </summary>
        private bool _uploading = false;
        public bool Uploading
        {
            get { return _uploading; }
            set
            {
                if (_uploading != value)
                {
                    _uploading = value;
                    RaisePropertyChanged("Uploading");
                    BeginInvoke(
                        (Action) (() => (SubmitCommand).RaiseCanExecuteChanged()));
                }
            }

        }

        /// <summary>
        /// A handle for the package upload so the user can know the state of the upload.
        /// </summary>
        private PackageUploadHandle _uploadHandle = null;
        public PackageUploadHandle UploadHandle
        {
            get { return _uploadHandle; }
            set
            {
                if (_uploadHandle != value)
                {
                    _uploadHandle = value;
                    _uploadHandle.PropertyChanged += UploadHandleOnPropertyChanged;
                    RaisePropertyChanged("UploadHandle");
                }
            }
        }

        /// <summary>
        /// IsNewVersion property </summary>
        /// <value>
        /// Specifies whether we're negotiating uploading a new version </value>
        private bool _isNewVersion = false;
        public bool IsNewVersion
        {
            get { return _isNewVersion; }
            set
            {
                if (_isNewVersion != value)
                {
                    _isNewVersion = value;
                    RaisePropertyChanged("IsNewVersion");
                }
            }
        }
        
        /// <summary>
        /// CanEditName property </summary>
        /// <value>
        /// The name of the node to be uploaded </value>
        public bool CanEditName
        {
            get { return !IsNewVersion; }
        }

        /// <summary>
        /// UploadState property </summary>
        /// <value>
        /// The state of the current upload 
        /// </value>
        private PackageUploadHandle.State _uploadState = PackageUploadHandle.State.Ready;
        public PackageUploadHandle.State UploadState
        {
            get { return _uploadState; }
            set
            {
                if (_uploadState != value)
                {
                    _uploadState = value;
                    RaisePropertyChanged("UploadState");
                }
            }
        }

        /// <summary>
        /// Name property </summary>
        /// <value>
        /// The name of the node to be uploaded </value>
        private string _group = "";
        public string Group
        {
            get { return _group; }
            set
            {
                if (_group != value)
                {
                    _group = value;
                    RaisePropertyChanged("Group");
                }
            }
        }

        /// <summary>
        /// Description property </summary>
        /// <value>
        /// The description to be uploaded </value>
        private string _Description = "";
        public string Description
        {
            get { return _Description; }
            set
            {
                if (_Description != value)
                {
                    _Description = value;
                    RaisePropertyChanged("Description");
                    BeginInvoke(
                        (Action)(() => (SubmitCommand).RaiseCanExecuteChanged()));
                }
            }
        }

        /// <summary>
        /// Keywords property </summary>
        /// <value>
        /// A string of space-delimited keywords</value>
        private string _Keywords = "";
        public string Keywords
        {
            get { return _Keywords; }
            set
            {
                if (_Keywords != value)
                {
                    value = value.Replace(',', ' ').ToLower().Trim();
                    var options = RegexOptions.None;
                    var regex = new Regex(@"[ ]{2,}", options);
                    value = regex.Replace(value, @" ");

                    _Keywords = value;
                    RaisePropertyChanged("Keywords");
                    KeywordList = value.Split(' ').Where(x => x.Length > 0).ToList();
                }
            }
        }

        /// <summary>
        /// KeywordList property </summary>
        /// <value>
        /// A list of keywords, usually produced by parsing Keywords</value>
        public List<string> KeywordList { get; set; }

        /// <summary>
        /// FullVersion property </summary>
        /// <value>
        /// The major, minor, and build version joined into one string</value>
        public string FullVersion
        {
            get { return MajorVersion + "." + MinorVersion + "." + BuildVersion; }
        }

        /// <summary>
        /// MinorVersion property </summary>
        /// <value>
        /// The second element of the version</value>
        private string _MinorVersion = "";
        public string MinorVersion
        {
            get { return _MinorVersion; }
            set
            {
                if (_MinorVersion != value)
                {
                    int val;
                    if (!Int32.TryParse(value, out val) || value == "") return;
                    if (value.Length != 1) value = value.TrimStart(new char[] { '0' });
                    _MinorVersion = value;
                    RaisePropertyChanged("MinorVersion");
                    BeginInvoke(
                        (Action)(() => (SubmitCommand).RaiseCanExecuteChanged()));
                }
            }
        }

        /// <summary>
        /// BuildVersion property </summary>
        /// <value>
        /// The third element of the version</value>
        private string _BuildVersion = "";
        public string BuildVersion
        {
            get { return _BuildVersion; }
            set
            {
                if (_BuildVersion != value)
                {
                    int val;
                    if (!Int32.TryParse(value, out val) || value == "") return;
                    if (value.Length != 1) value = value.TrimStart(new char[] { '0' });
                    _BuildVersion = value;
                    RaisePropertyChanged("BuildVersion");
                    BeginInvoke(
                        (Action)(() => (SubmitCommand).RaiseCanExecuteChanged()));
                }
            }
        }

        /// <summary>
        /// MajorVersion property </summary>
        /// <value>
        /// The first element of the version</value>
        private string _MajorVersion = "";
        public string MajorVersion
        {
            get { return _MajorVersion; }
            set
            {
                if (_MajorVersion != value)
                {
                    int val;
                    if (!Int32.TryParse(value, out val) || value == "") return;
                    if (value.Length != 1) value = value.TrimStart(new char[] { '0' });
                    _MajorVersion = value;
                    RaisePropertyChanged("MajorVersion");
                    BeginInvoke(
                        (Action)(() => (SubmitCommand).RaiseCanExecuteChanged()));
                }
            }
        }


        /// <summary>
        /// License property </summary>
        /// <value>
        /// The license for the package </value>
        private string _license = "";
        public string License
        {
            get { return _license; }
            set
            {
                if (_license != value)
                {
                    _license = value;
                    RaisePropertyChanged("License");
                }
            }
        }

        /// <summary>
        /// SiteUrl property </summary>
        /// <value>
        /// The website for the package</value>
        private string _siteUrl = "";
        public string SiteUrl
        {
            get { return _siteUrl; }
            set
            {
                if (_siteUrl != value)
                {
                    _siteUrl = value;
                    RaisePropertyChanged("SiteUrl");
                }
            }
        }

        /// <summary>
        /// RepositoryUrl property </summary>
        /// <value>
        /// The repository url for the package</value>
        private string _repositoryUrl = "";
        public string RepositoryUrl
        {
            get { return _repositoryUrl; }
            set
            {
                if (_repositoryUrl != value)
                {
                    _repositoryUrl = value;
                    RaisePropertyChanged("RepositoryUrl");
                }
            }
        }

        /// <summary>
        /// Name property </summary>
        /// <value>
        /// The name of the node to be uploaded </value>
        private string _name = "";
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    RaisePropertyChanged("Name");
                    BeginInvoke(
                        (Action)(() => (SubmitCommand).RaiseCanExecuteChanged()));
                }
            }
        }

        private bool _moreExpanded;
        public bool MoreExpanded
        {
            get { return _moreExpanded; }
            set
            {
                if (_moreExpanded != value)
                {
                    _moreExpanded = value;
                    RaisePropertyChanged("MoreExpanded");
                }
            }
        }

        public bool HasDependencies
        {
            get { return Dependencies.Count > 0; }
        }

        public bool HasNoDependencies
        {
            get { return !HasDependencies; }
        }

        /// <summary>
        /// SubmitCommand property </summary>
        /// <value>
        /// A command which, when executed, submits the current package</value>
        public DelegateCommand SubmitCommand { get; private set; }

        /// <summary>
        /// SubmitCommand property </summary>
        /// <value>
        /// A command which, when executed, submits the current package</value>
        public DelegateCommand ShowAddFileDialogAndAddCommand { get; private set; }

        /// <summary>
        /// ToggleMoreCommand property </summary>
        /// <value>
        /// A command which, when executed, submits the current package</value>
        public DelegateCommand ToggleMoreCommand { get; private set; }

        /// <summary>
        /// The package used for this submission
        /// </summary>
        public Package Package { get; set; }

        /// <summary>
        /// PackageContents property 
        /// </summary>
        private List<PackageItemRootViewModel> _packageContents = null;
        public IEnumerable<PackageItemRootViewModel> PackageContents
        {
            get
            {
                _packageContents = CustomNodeDefinitions.Select(
                    (def) => new PackageItemRootViewModel(def))
                    .Concat(Assemblies.Select((pa) => new PackageItemRootViewModel(pa)))
                    .Concat(AdditionalFiles.Select((s) => new PackageItemRootViewModel(new FileInfo(s))))
                    .ToList();
                return _packageContents;
            }
        }

        /// <summary>
        /// CustomNodeDefinitions property 
        /// </summary>
        private List<CustomNodeDefinition> customNodeDefinitions;
        public List<CustomNodeDefinition> CustomNodeDefinitions
        {
            get { return customNodeDefinitions; }
            set
            {
                customNodeDefinitions = value;

                if (customNodeDefinitions.Count > 0 && Name == null)
                {
                    Name = CustomNodeDefinitions[0].DisplayName;
                }
                
                UpdateDependencies();
            }
        }

        public List<PackageAssembly> Assemblies { get; set; }

        /// <summary>
        /// AdditionalFiles property 
        /// </summary>
        private ObservableCollection<string> _additionalFiles = new ObservableCollection<string>();
        public ObservableCollection<string> AdditionalFiles
        {
            get { return _additionalFiles; }
            set
            {
                if (_additionalFiles != value)
                {
                    _additionalFiles = value;
                    RaisePropertyChanged("AdditionalFiles");
                }
            }
        }

        /// <summary>
        /// Dependencies property </summary>
        /// <value>
        /// Computed and manually added package dependencies</value>
        public ObservableCollection<PackageDependency> Dependencies { get; set; }

        /// <summary>
        /// BaseVersionHeader property </summary>
        /// <value>
        /// The PackageHeader if we're uploading a new verson, setting this updates
        /// almost all of the fields in this object</value>
        /// 
        private PackageUploadRequestBody _dynamoBaseHeader;
        public PackageUploadRequestBody BaseVersionHeader
        {
            get { return _dynamoBaseHeader; }
            set
            {
                IsNewVersion = true;
                Description = value.description;
                string[] versionSplit = value.version.Split('.');
                MajorVersion = versionSplit[0];
                MinorVersion = versionSplit[1];
                BuildVersion = versionSplit[2];
                Name = value.name;
                Keywords = String.Join(" ", value.keywords);
                _dynamoBaseHeader = value;
            }
        }

        #endregion

        internal PublishPackageViewModel()
        {
            customNodeDefinitions = new List<CustomNodeDefinition>();
            SubmitCommand = new DelegateCommand(Submit, CanSubmit);
            ShowAddFileDialogAndAddCommand = new DelegateCommand(ShowAddFileDialogAndAdd, CanShowAddFileDialogAndAdd);
            ToggleMoreCommand = new DelegateCommand(() => MoreExpanded = !MoreExpanded, () => true);
            Dependencies = new ObservableCollection<PackageDependency>();
            Assemblies = new List<PackageAssembly>();
            PropertyChanged += ThisPropertyChanged;
        }

        private void BeginInvoke(Action action)
        {
            if (dynamoViewModel != null)
                dynamoViewModel.UIDispatcher.BeginInvoke(action);
        }

        /// <summary>
        /// The class constructor. </summary>
        public PublishPackageViewModel( DynamoViewModel dynamoViewModel ) : this()
        {
            this.dynamoViewModel = dynamoViewModel;
        }

        private void ThisPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PackageContents") CanSubmit();
        }

        public static PublishPackageViewModel FromLocalPackage(DynamoViewModel dynamoViewModel, Package l)
        {
            var defs = new List<CustomNodeDefinition>();

            foreach (var x in l.LoadedCustomNodes)
            {
                CustomNodeDefinition def;
                if (dynamoViewModel.Model.CustomNodeManager.TryGetFunctionDefinition(
                    x.FunctionId,
                    DynamoModel.IsTestMode,
                    out def))
                {
                    defs.Add(def);
                }
            }

            var vm = new PublishPackageViewModel(dynamoViewModel)
            {
                Group = l.Group,
                Description = l.Description,
                Keywords = l.Keywords != null ? String.Join(" ", l.Keywords) : "",
                CustomNodeDefinitions = defs,
                Name = l.Name,
                RepositoryUrl = l.RepositoryUrl ?? "",
                SiteUrl = l.SiteUrl ?? "",
                Package = l,
                License = l.License
            };

            // add additional files
            l.EnumerateAdditionalFiles();
            foreach (var file in l.AdditionalFiles)
            {
                vm.AdditionalFiles.Add(file.Model.FullName);
            }

            var nodeLibraryNames = l.Header.node_libraries;

            // load assemblies into reflection only context
            foreach (var file in l.EnumerateAssemblyFilesInBinDirectory())
            {
                Assembly assem;
                var result = PackageLoader.TryReflectionOnlyLoadFrom(file, out assem);
                if (result)
                {
                    var isNodeLibrary = nodeLibraryNames == null || nodeLibraryNames.Contains(assem.FullName);
                    vm.Assemblies.Add(new  PackageAssembly()
                    {
                        IsNodeLibrary = isNodeLibrary,
                        Assembly = assem
                    });
                }
                else
                {
                    // if it's not a .NET assembly, we load it as an additional file
                    vm.AdditionalFiles.Add(file);
                }
            }

            if (l.VersionName == null) return vm;

            var parts = l.VersionName.Split('.');
            if (parts.Count() != 3) return vm;

            vm.MajorVersion = parts[0];
            vm.MinorVersion = parts[1];
            vm.BuildVersion = parts[2];
            return vm;

        }

        public void OnPublishSuccess()
        {
            if (PublishSuccess != null)
                PublishSuccess(this);
        }

        private void UploadHandleOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "UploadState")
            {
                UploadState = ((PackageUploadHandle)sender).UploadState;

                if (((PackageUploadHandle)sender).UploadState == PackageUploadHandle.State.Uploaded)
                {
                    OnPublishSuccess();
                }

            }
            else if (propertyChangedEventArgs.PropertyName == "ErrorString")
            {
                UploadState = PackageUploadHandle.State.Error;
                ErrorString = ((PackageUploadHandle)sender).ErrorString;
                Uploading = false;
            }
        }

        private IEnumerable<CustomNodeDefinition> AllDependentFuncDefs()
        {
            return
                CustomNodeDefinitions.Select(x => x.Dependencies)
                                   .SelectMany(x => x)
                                   .Where(x => !CustomNodeDefinitions.Contains(x))
                                   .Distinct();
        }

        private IEnumerable<CustomNodeDefinition> AllFuncDefs()
        {
            return AllDependentFuncDefs().Union(CustomNodeDefinitions).Distinct();
        }

        private IEnumerable<string> GetAllFiles()
        {
            // get all function defs
            var allFuncs = AllFuncDefs().ToList();

            // all workspaces
            var workspaces = new List<CustomNodeWorkspaceModel>();
            foreach (var def in allFuncs)
            {
                CustomNodeWorkspaceModel ws;
                if (dynamoViewModel.Model.CustomNodeManager.TryGetFunctionWorkspace(
                    def.FunctionId,
                    DynamoModel.IsTestMode,
                    out ws))
                {
                    workspaces.Add(ws);
                }
            }
            
            // make sure workspaces are saved
            var unsavedWorkspaceNames =
                workspaces.Where(ws => ws.HasUnsavedChanges || ws.FileName == null).Select(ws => ws.Name).ToList();
            if (unsavedWorkspaceNames.Any())
            {
                throw new Exception(Wpf.Properties.Resources.MessageUnsavedChanges0 +
                                    String.Join(", ", unsavedWorkspaceNames) +
                                    Wpf.Properties.Resources.MessageUnsavedChanges1);
            }

            // omit files currently already under package control
            var files =
                workspaces.Select(f => f.FileName)
                    .Where(
                        p =>
                            (dynamoViewModel.Model.PackageLoader.IsUnderPackageControl(p)
                                && (dynamoViewModel.Model.PackageLoader.GetOwnerPackage(p).Name == Name)
                                || !dynamoViewModel.Model.PackageLoader.IsUnderPackageControl(p)));

            // union with additional files
            files = files.Union(AdditionalFiles);
            files = files.Union(Assemblies.Select(x => x.Assembly.Location));

            return files;
        }

        private void UpdateDependencies()
        {
            Dependencies.Clear();
            GetAllDependencies().ToList().ForEach(Dependencies.Add);
        }

        private IEnumerable<PackageDependency> GetAllDependencies()
        {
            var pkgLoader = dynamoViewModel.Model.PackageLoader;

            // all workspaces
            var workspaces = new List<CustomNodeWorkspaceModel>();
            foreach (var def in AllDependentFuncDefs())
            {
                CustomNodeWorkspaceModel ws;
                if (dynamoViewModel.Model.CustomNodeManager.TryGetFunctionWorkspace(
                    def.FunctionId,
                    DynamoModel.IsTestMode,
                    out ws))
                {
                    workspaces.Add(ws);
                }
            }

            // get all of dependencies from custom nodes and additional files
            var allFilePackages =
                workspaces
                    .Select(x => x.FileName)
                    .Union(AdditionalFiles)
                    .Where(pkgLoader.IsUnderPackageControl)
                    .Select(pkgLoader.GetOwnerPackage)
                    .Where(x => x != null)
                    .Where(x => (x.Name != Name))
                    .Distinct()
                    .Select(x => new PackageDependency(x.Name, x.VersionName));

            workspaces = new List<CustomNodeWorkspaceModel>();
            foreach (var def in AllFuncDefs())
            {
                CustomNodeWorkspaceModel ws;
                if (dynamoViewModel.Model.CustomNodeManager.TryGetFunctionWorkspace(
                    def.FunctionId,
                    DynamoModel.IsTestMode,
                    out ws))
                {
                    workspaces.Add(ws);
                }
            }

            // get all of the dependencies from types
            var allTypePackages = workspaces
                .SelectMany(x => x.Nodes)
                .Select(x => x.GetType())
                .Where(pkgLoader.IsUnderPackageControl)
                .Select(pkgLoader.GetOwnerPackage)
                .Where(x => x != null)
                .Where(x => (x.Name != Name))
                .Distinct()
                .Select(x => new PackageDependency(x.Name, x.VersionName));

            var dsFunctionPackages = workspaces
                .SelectMany(x => x.Nodes)
                .OfType<DSFunctionBase>()
                .Select(x => x.Controller.Definition.Assembly)
                .Where(pkgLoader.IsUnderPackageControl)
                .Select(pkgLoader.GetOwnerPackage)
                .Where(x => x != null)
                .Where(x => (x.Name != Name))
                .Distinct()
                .Select(x => new PackageDependency(x.Name, x.VersionName));

            return allFilePackages.Union(allTypePackages).Union(dsFunctionPackages);

        }

        private IEnumerable<Tuple<string, string>> GetAllNodeNameDescriptionPairs()
        {
            var pkgLoader = dynamoViewModel.Model.PackageLoader;

            var workspaces = new List<CustomNodeWorkspaceModel>();
            foreach (var def in AllFuncDefs())
            {
                CustomNodeWorkspaceModel ws;
                if (dynamoViewModel.Model.CustomNodeManager.TryGetFunctionWorkspace(
                    def.FunctionId,
                    DynamoModel.IsTestMode,
                    out ws))
                {
                    workspaces.Add(ws);
                }
            }

            // collect the name-description pairs for every custom node
            return
                workspaces
                    .Where(
                        p =>
                            (pkgLoader.IsUnderPackageControl(p.FileName) && pkgLoader.GetOwnerPackage(p.FileName).Name == Name)
                                || !dynamoViewModel.Model.PackageLoader.IsUnderPackageControl(p.FileName))
                    .Select(
                        x =>
                            new Tuple<string, string>(
                            x.Name,
                            !String.IsNullOrEmpty(x.Description)
                                ? x.Description
                                : Wpf.Properties.Resources.MessageNoNodeDescription));
        }

        private string _errorString = "";
        public string ErrorString
        {
            get { return _errorString; }
            set
            {
                _errorString = value;
                RaisePropertyChanged("ErrorString");
            }
        }

        private void ShowAddFileDialogAndAdd()
        {
            // show file open dialog
            FileDialog fDialog = null;

            if (fDialog == null)
            {
                fDialog = new OpenFileDialog()
                {
                    Filter = string.Format(Resources.FileDialogCustomNodeDLLXML, "*.dyf;*.dll;*.xml"),
                    Title = Resources.AddCustomFileToPackageDialogTitle
                };
            }

            // if you've got the current space path, use it as the inital dir
            if (!string.IsNullOrEmpty(dynamoViewModel.Model.CurrentWorkspace.FileName))
            {
                var fi = new FileInfo(dynamoViewModel.Model.CurrentWorkspace.FileName);
                fDialog.InitialDirectory = fi.DirectoryName;
            }
            else // use the definitions directory
            {
                var pathManager = dynamoViewModel.Model.PathManager;
                if (Directory.Exists(pathManager.UserDefinitions))
                {
                    fDialog.InitialDirectory = pathManager.UserDefinitions;
                }
            }

            if (fDialog.ShowDialog() == DialogResult.OK)
            {
                AddFile(fDialog.FileName);
            }
        }

        private bool CanShowAddFileDialogAndAdd()
        {
            return true;
        }

        private void AddFile(string filename)
        {
            if (!File.Exists(filename)) return;

            if (filename.ToLower().EndsWith(".dll"))
            {
                AddDllFile(filename);
                return;
            }

            if (filename.ToLower().EndsWith(".dyf"))
            {
                AddCustomNodeFile(filename);
                return;
            }

            AddAdditionalFile(filename);
        }

        private void AddCustomNodeFile(string filename)
        {
            CustomNodeInfo nodeInfo;
            if (dynamoViewModel.Model.CustomNodeManager.AddUninitializedCustomNode(filename, DynamoModel.IsTestMode, out nodeInfo))
            {
                // add the new packages folder to path
                dynamoViewModel.Model.CustomNodeManager.AddUninitializedCustomNodesInPath(Path.GetDirectoryName(filename), DynamoModel.IsTestMode);

                CustomNodeDefinition funcDef;
                if (dynamoViewModel.Model.CustomNodeManager.TryGetFunctionDefinition(nodeInfo.FunctionId, DynamoModel.IsTestMode, out funcDef)
                    && CustomNodeDefinitions.All(x => x.FunctionId != funcDef.FunctionId))
                {
                    CustomNodeDefinitions.Add(funcDef);
                    RaisePropertyChanged("PackageContents");
                }
            }
        }

        private void AddAdditionalFile(string filename)
        {
            try
            {
                AdditionalFiles.Add(filename);
                RaisePropertyChanged("PackageContents");
            }
            catch (Exception e)
            {
                dynamoViewModel.Model.Logger.Log(e);
            }
        }

        private void AddDllFile(string filename)
        {
            try
            {
                Assembly assem;

                // we're not sure if this is a managed assembly or not
                // we try to load it, if it fails - we add it as an additional file
                var result = PackageLoader.TryLoadFrom(filename, out assem);
                if (result)
                {
                    Assemblies.Add(new PackageAssembly()
                    {
                        Assembly = assem,
                        IsNodeLibrary = true // assume is node library when first added
                    });
                    RaisePropertyChanged("PackageContents");
                }
                else
                {
                    AddAdditionalFile(filename);
                }               
            }
            catch (Exception e)
            {
                dynamoViewModel.Model.Logger.Log(e);
            }
        }

        /// <summary>
        /// Delegate used to submit the element</summary>
        private void Submit()
        {
            try
            {
                // build the package
                var isNewPackage = Package == null;

                Package = Package ?? new Package("", Name, FullVersion, License);

                Package.VersionName = FullVersion;
                Package.Description = Description;
                Package.Group = Group;
                Package.Keywords = KeywordList;
                Package.License = License;
                Package.SiteUrl = SiteUrl;
                Package.RepositoryUrl = RepositoryUrl;

                AppendPackageContents();

                Package.Dependencies.Clear();
                GetAllDependencies().ToList().ForEach(Package.Dependencies.Add);

                var files = GetAllFiles().ToList();

                if (isNewPackage)
                    dynamoViewModel.Model.PackageLoader.LocalPackages.Add(Package);

                Package.AddAssemblies(Assemblies);

                // begin submission
                var handle = dynamoViewModel.Model.PackageManagerClient.Publish(Package, files, IsNewVersion, DynamoModel.IsTestMode);

                // start upload
                Uploading = true;
                UploadHandle = handle;

            }
            catch (Exception e)
            {
                ErrorString = e.Message;
                dynamoViewModel.Model.Logger.Log(e);
            }
        }

        private void AppendPackageContents()
        {
            Package.Contents = String.Join(", ", GetAllNodeNameDescriptionPairs().Select((pair) => pair.Item1 + " - " + pair.Item2));
        }

        /// <summary>
        /// Delegate used to submit the element </summary>
        private bool CanSubmit()
        {
            // Typically, this code should never be seen as the publish package dialogs should not 
            // be active when there is no authenticator
            if (dynamoViewModel == null || !dynamoViewModel.Model.PackageManagerClient.HasAuthProvider)
            {
                ErrorString = Resources.CannotSubmitPackage;
                return false;
            }

            if (Name.Contains(@"\") || Name.Contains(@"/") || Name.Contains(@"*"))
            {
                ErrorString = Resources.PackageNameCannotContainTheseCharacters;
                return false;
            }

            if (Name.Length < 3)
            {
                ErrorString = Resources.NameNeedMoreCharacters;
                return false;
            }

            if (Description.Length <= 10)
            {
                ErrorString = Resources.DescriptionNeedMoreCharacters;
                return false;
            }

            if (MajorVersion.Length <= 0)
            {
                ErrorString = Resources.MajorVersionNonNegative;
                return false;
            }

            if (MinorVersion.Length <= 0)
            {
                ErrorString = Resources.MinorVersionNonNegative;
                return false;
            }

            if (BuildVersion.Length <= 0)
            {
                ErrorString = Resources.BuildVersionNonNegative;
                return false;
            }

            if (Double.Parse(BuildVersion) + Double.Parse(MinorVersion) + Double.Parse(MajorVersion) <= 0)
            {
                ErrorString = Resources.VersionValueGreaterThan0;
                return false;
            }

            if (!PackageContents.Any())
            {
                ErrorString = Resources.PackageNeedAtLeastOneFile;
                return false;
            }

            if ( UploadState != PackageUploadHandle.State.Error ) ErrorString = "";

            if (Uploading) return false;

            return true;
        }

    }

}
