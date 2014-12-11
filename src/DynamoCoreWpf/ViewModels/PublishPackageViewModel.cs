using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using Dynamo.Nodes;
using Dynamo.PackageManager.UI;
using Dynamo.ViewModels;

using DynamoUtilities;

using Greg.Requests;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using Double = System.Double;
using String = System.String;

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
                if (this._uploading != value)
                {
                    this._uploading = value;
                    this.RaisePropertyChanged("Uploading");
                    this.BeginInvoke(
                        (Action) (() => (this.SubmitCommand).RaiseCanExecuteChanged()));
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
                if (this._uploadHandle != value)
                {
                    this._uploadHandle = value;
                    this._uploadHandle.PropertyChanged += UploadHandleOnPropertyChanged;
                    this.RaisePropertyChanged("UploadHandle");
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
                if (this._isNewVersion != value)
                {
                    this._isNewVersion = value;
                    this.RaisePropertyChanged("IsNewVersion");
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
                if (this._uploadState != value)
                {
                    this._uploadState = value;
                    this.RaisePropertyChanged("UploadState");
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
                if (this._group != value)
                {
                    this._group = value;
                    this.RaisePropertyChanged("Group");
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
                if (this._Description != value)
                {
                    this._Description = value;
                    this.RaisePropertyChanged("Description");
                    this.BeginInvoke(
                        (Action)(() => (this.SubmitCommand).RaiseCanExecuteChanged()));
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
                if (this._Keywords != value)
                {
                    value = value.Replace(',', ' ').ToLower().Trim();
                    var options = RegexOptions.None;
                    var regex = new Regex(@"[ ]{2,}", options);
                    value = regex.Replace(value, @" ");

                    this._Keywords = value;
                    this.RaisePropertyChanged("Keywords");
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
            get { return this.MajorVersion + "." + this.MinorVersion + "." + this.BuildVersion; }
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
                if (this._MinorVersion != value)
                {
                    int val;
                    if (!Int32.TryParse(value, out val) || value == "") return;
                    if (value.Length != 1) value = value.TrimStart(new char[] { '0' });
                    this._MinorVersion = value;
                    this.RaisePropertyChanged("MinorVersion");
                    this.BeginInvoke(
                        (Action)(() => (this.SubmitCommand).RaiseCanExecuteChanged()));
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
                if (this._BuildVersion != value)
                {
                    int val;
                    if (!Int32.TryParse(value, out val) || value == "") return;
                    if (value.Length != 1) value = value.TrimStart(new char[] { '0' });
                    this._BuildVersion = value;
                    this.RaisePropertyChanged("BuildVersion");
                    this.BeginInvoke(
                        (Action)(() => (this.SubmitCommand).RaiseCanExecuteChanged()));
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
                if (this._MajorVersion != value)
                {
                    int val;
                    if (!Int32.TryParse(value, out val) || value == "") return;
                    if (value.Length != 1) value = value.TrimStart(new char[] { '0' });
                    this._MajorVersion = value;
                    this.RaisePropertyChanged("MajorVersion");
                    this.BeginInvoke(
                        (Action)(() => (this.SubmitCommand).RaiseCanExecuteChanged()));
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
                if (this._license != value)
                {
                    this._license = value;
                    this.RaisePropertyChanged("License");
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
                if (this._siteUrl != value)
                {
                    this._siteUrl = value;
                    this.RaisePropertyChanged("SiteUrl");
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
                if (this._repositoryUrl != value)
                {
                    this._repositoryUrl = value;
                    this.RaisePropertyChanged("RepositoryUrl");
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
                if (this._name != value)
                {
                    this._name = value;
                    this.RaisePropertyChanged("Name");
                    this.BeginInvoke(
                        (Action)(() => (this.SubmitCommand).RaiseCanExecuteChanged()));
                }
            }
        }

        private bool _moreExpanded;
        public bool MoreExpanded
        {
            get { return _moreExpanded; }
            set
            {
                if (this._moreExpanded != value)
                {
                    this._moreExpanded = value;
                    this.RaisePropertyChanged("MoreExpanded");
                }
            }
        }

        public bool HasDependencies
        {
            get { return this.Dependencies.Count > 0; }
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
                this.UpdateDependencies();
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
                if (this._additionalFiles != value)
                {
                    this._additionalFiles = value;
                    this.RaisePropertyChanged("AdditionalFiles");
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
                this.IsNewVersion = true;
                this.Description = value.description;
                string[] versionSplit = value.version.Split('.');
                this.MajorVersion = versionSplit[0];
                this.MinorVersion = versionSplit[1];
                this.BuildVersion = versionSplit[2];
                this.Name = value.name;
                this.Keywords = String.Join(" ", value.keywords);
                this._dynamoBaseHeader = value;
            }
        }

        #endregion

        internal PublishPackageViewModel()
        {
            this.customNodeDefinitions = new List<CustomNodeDefinition>();
            this.SubmitCommand = new DelegateCommand(this.Submit, this.CanSubmit);
            this.ShowAddFileDialogAndAddCommand = new DelegateCommand(this.ShowAddFileDialogAndAdd, this.CanShowAddFileDialogAndAdd);
            this.ToggleMoreCommand = new DelegateCommand(() => this.MoreExpanded = !this.MoreExpanded, () => true);
            this.Dependencies = new ObservableCollection<PackageDependency>();
            this.Assemblies = new List<PackageAssembly>();
            this.PropertyChanged += this.ThisPropertyChanged;
        }

        private void BeginInvoke(Action action)
        {
            if (this.dynamoViewModel != null)
                this.dynamoViewModel.UIDispatcher.BeginInvoke(action);
        }

        /// <summary>
        /// The class constructor. </summary>
        public PublishPackageViewModel( DynamoViewModel dynamoViewModel ) : this()
        {
            this.dynamoViewModel = dynamoViewModel;
        }

        private void ThisPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PackageContents")
            {
                this.CanSubmit();
                this.dynamoViewModel.UIDispatcher.BeginInvoke(
                        (Action)(() => (this.SubmitCommand).RaiseCanExecuteChanged()));
            }
        }

        public static PublishPackageViewModel FromLocalPackage(DynamoViewModel dynamoViewModel, Package l)
        {
            var vm = new PublishPackageViewModel(dynamoViewModel)
            {
                Group = l.Group,
                Description = l.Description,
                Keywords = l.Keywords != null ? String.Join(" ", l.Keywords) : "",
                CustomNodeDefinitions =
                    l.LoadedCustomNodes.Select(
                        x => dynamoViewModel.Model.CustomNodeManager.GetFunctionDefinition(x.Guid))
                        .ToList(),
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
            if (this.PublishSuccess != null)
                this.PublishSuccess(this);
        }

        private void UploadHandleOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "UploadState")
            {
                this.UploadState = ((PackageUploadHandle)sender).UploadState;

                if (((PackageUploadHandle)sender).UploadState == PackageUploadHandle.State.Uploaded)
                {
                    this.OnPublishSuccess();
                }

            }
            else if (propertyChangedEventArgs.PropertyName == "ErrorString")
            {
                this.UploadState = PackageUploadHandle.State.Error;
                this.ErrorString = ((PackageUploadHandle)sender).ErrorString;
                this.Uploading = false;
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
            var workspaces = allFuncs.Select(def => def.WorkspaceModel).ToList();

            // make sure workspaces are saved
            var unsavedWorkspaceNames =
                workspaces.Where(ws => ws.HasUnsavedChanges || ws.FileName == null).Select(ws => ws.Name).ToList();
            if (unsavedWorkspaceNames.Any())
            {
                throw new Exception("The following workspaces have not been saved: " +
                                    String.Join(", ", unsavedWorkspaceNames) + ". Please save them and try again.");
            }

            // omit files currently already under package control
            var files =
                allFuncs.Select(f => f.WorkspaceModel.FileName)
                        .Where(p =>
                                (this.dynamoViewModel.Model.Loader.PackageLoader.IsUnderPackageControl(p) &&
                                (this.dynamoViewModel.Model.Loader.PackageLoader.GetOwnerPackage(p).Name == this.Name) ||
                                !this.dynamoViewModel.Model.Loader.PackageLoader.IsUnderPackageControl(p)));

            // union with additional files
            files = files.Union(this.AdditionalFiles);
            files = files.Union(this.Assemblies.Select(x => x.Assembly.Location));

            return files;
        }

        private void UpdateDependencies()
        {
            this.Dependencies.Clear();
            this.GetAllDependencies().ToList().ForEach(this.Dependencies.Add);
        }

        private IEnumerable<PackageDependency> GetAllDependencies()
        {
            var pkgLoader = this.dynamoViewModel.Model.Loader.PackageLoader;

            // get all of dependencies from custom nodes and additional files
            var allFilePackages =
                AllDependentFuncDefs()
                    .Select(x => x.WorkspaceModel.FileName)
                    .Union(AdditionalFiles)
                    .Where(pkgLoader.IsUnderPackageControl)
                    .Select(pkgLoader.GetOwnerPackage)
                    .Where(x => x != null)
                    .Where(x => (x.Name != this.Name))
                    .Distinct()
                    .Select(x => new PackageDependency(x.Name, x.VersionName));

            // get all of the dependencies from types
            var allTypePackages = AllFuncDefs()
                .Select(x => x.WorkspaceModel.Nodes)
                .SelectMany(x => x)
                .Select(x => x.GetType())
                .Where(pkgLoader.IsUnderPackageControl)
                .Select(pkgLoader.GetOwnerPackage)
                .Where(x => x != null)
                .Where(x => (x.Name != this.Name))
                .Distinct()
                .Select(x => new PackageDependency(x.Name, x.VersionName));

            var dsFunctionPackages = AllFuncDefs()
                .Select(x => x.WorkspaceModel.Nodes)
                .SelectMany(x => x)
                .OfType<DSFunctionBase>()
                .Select(x => x.Controller.Definition.Assembly)
                .Where(pkgLoader.IsUnderPackageControl)
                .Select(pkgLoader.GetOwnerPackage)
                .Where(x => x != null)
                .Where(x => (x.Name != this.Name))
                .Distinct()
                .Select(x => new PackageDependency(x.Name, x.VersionName));

            return allFilePackages.Union(allTypePackages).Union(dsFunctionPackages);

        }

        private IEnumerable<Tuple<string, string>> GetAllNodeNameDescriptionPairs()
        {
            var pkgLoader = this.dynamoViewModel.Model.Loader.PackageLoader;

            // collect the name-description pairs for every custom node
            return
                AllFuncDefs()
                    .Where(p =>
                                (pkgLoader.IsUnderPackageControl(p) &&
                                pkgLoader.GetOwnerPackage(p).Name == this.Name) || !dynamoViewModel.Model.Loader.PackageLoader.IsUnderPackageControl(p))
                        .Select(x => new Tuple<string, string>(x.WorkspaceModel.Name, !String.IsNullOrEmpty(x.WorkspaceModel.Description) ? x.WorkspaceModel.Description : "No description provided"));
        }

        private string _errorString = "";
        public string ErrorString
        {
            get { return _errorString; }
            set
            {
                this._errorString = value;
                this.RaisePropertyChanged("ErrorString");
            }
        }

        private bool openFileDialogHasBeenShown = false;

        private void ShowAddFileDialogAndAdd()
        {
            // show file open dialog
            var fDialog = new OpenFileDialog()
            {
                Title = "Add File to Package...",
                RestoreDirectory = true,
                Multiselect = true,
                CheckFileExists = true
            };

            // if we've shown the dialog, don't use this logic to determine the start directory
            if (!openFileDialogHasBeenShown)
            {
                // if you've got the current space path, use it as the inital dir
                if (!string.IsNullOrEmpty(this.dynamoViewModel.Model.CurrentWorkspace.FileName))
                {
                    var fi = new FileInfo(this.dynamoViewModel.Model.CurrentWorkspace.FileName);
                    fDialog.InitialDirectory = fi.DirectoryName;
                }
                else // use the definitions directory
                {
                    if (Directory.Exists(DynamoPathManager.Instance.UserDefinitions))
                    {
                        fDialog.InitialDirectory = DynamoPathManager.Instance.UserDefinitions;
                    }
                }
            }

            if (fDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (var fn in fDialog.FileNames)
                {
                    this.AddFile(fn);
                }
            }

            openFileDialogHasBeenShown = true;
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
                this.AddDllFile(filename);
                return;
            }

            if (filename.ToLower().EndsWith(".dyf"))
            {
                this.AddCustomNodeFile(filename);
                return;
            }

            this.AddAdditionalFile(filename);
        }

        private void AddCustomNodeFile(string filename)
        {
            var nodeInfo = this.dynamoViewModel.Model.CustomNodeManager.AddFileToPath(filename);
            if (nodeInfo != null)
            {
                // add the new packages folder to path
                this.dynamoViewModel.Model.CustomNodeManager.AddDirectoryToSearchPath(Path.GetDirectoryName(filename));
                this.dynamoViewModel.Model.CustomNodeManager.UpdateSearchPath();

                var funcDef = this.dynamoViewModel.Model.CustomNodeManager.GetFunctionDefinition(nodeInfo.Guid);

                if (funcDef != null && this.CustomNodeDefinitions.All(x => x.FunctionId != funcDef.FunctionId))
                {
                    this.CustomNodeDefinitions.Add(funcDef);
                    this.RaisePropertyChanged("PackageContents");
                }
            }
        }

        private void AddAdditionalFile(string filename)
        {
            try
            {
                this.AdditionalFiles.Add(filename);
                this.RaisePropertyChanged("PackageContents");
            }
            catch (Exception e)
            {
                this.dynamoViewModel.Model.Logger.Log(e);
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
                    this.Assemblies.Add(new PackageAssembly()
                    {
                        Assembly = assem,
                        IsNodeLibrary = true // assume is node library when first added
                    });
                    this.RaisePropertyChanged("PackageContents");
                }
                else
                {
                    AddAdditionalFile(filename);
                }               
            }
            catch (Exception e)
            {
                this.dynamoViewModel.Model.Logger.Log(e);
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

                Package = Package ?? new Package("", this.Name, this.FullVersion, this.License);

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
                {
                    this.dynamoViewModel.Model.Loader.PackageLoader.LocalPackages.Add(Package);
                }

                Package.AddAssemblies(this.Assemblies);

                // begin submission
                var handle = this.dynamoViewModel.Model.PackageManagerClient.Publish(Package, files, IsNewVersion);

                // start upload
                this.Uploading = true;
                this.UploadHandle = handle;

            }
            catch (Exception e)
            {
                ErrorString = e.Message;
                this.dynamoViewModel.Model.Logger.Log(e);
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
            if (this.dynamoViewModel == null || !this.dynamoViewModel.Model.PackageManagerClient.HasAuthenticator)
            {
                this.ErrorString = "You can't submit a package in this version of Dynamo.  You'll need a host application, like Revit, to submit a package.";
                return false;
            }

            if (this.Name.Contains(@"\") || this.Name.Contains(@"/") || this.Name.Contains(@"*"))
            {
                this.ErrorString = @"The name of the package cannot contain /,\, or *.";
                return false;
            }

            if (this.Name.Length < 3)
            {
                this.ErrorString = "Name must be at least 3 characters.";
                return false;
            }

            if (Description.Length <= 10)
            {
                this.ErrorString = "Description must be longer than 10 characters.";
                return false;
            }

            if (this.MajorVersion.Length <= 0)
            {
                this.ErrorString = "You must provide a Major version as a non-negative integer.";
                return false;
            }

            if (this.MinorVersion.Length <= 0)
            {
                this.ErrorString = "You must provide a Minor version as a non-negative integer.";
                return false;
            }

            if (this.BuildVersion.Length <= 0)
            {
                this.ErrorString = "You must provide a Build version as a non-negative integer.";
                return false;
            }

            if (Double.Parse(this.BuildVersion) + Double.Parse(this.MinorVersion) + Double.Parse(this.MajorVersion) <= 0)
            {
                this.ErrorString = "At least one of your version values must be greater than 0.";
                return false;
            }

            if (!this.PackageContents.Any())
            {
                this.ErrorString = "Your package must contain at least one file.";
                return false;
            }

            if ( this.UploadState != PackageUploadHandle.State.Error ) this.ErrorString = "";

            if (this.Uploading) return false;

            return true;
        }

    }

}
