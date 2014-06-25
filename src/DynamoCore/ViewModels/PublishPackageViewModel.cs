using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Dynamo.Nodes;
using Dynamo.PackageManager.UI;
using Dynamo.Utilities;

using DynamoUtilities;

using Greg.Requests;
using Greg.Responses;
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

        #region Properties

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
                    dynSettings.Controller.UIDispatcher.BeginInvoke(
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
        /// Client property 
        /// </summary>
        /// <value>
        /// The PackageManagerClient object for performing OAuth calls</value>
        public PackageManagerClient Client { get; internal set; }

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
        /// Dependencies property </summary>
        /// <value>
        /// The set of dependencies  </value>
        private List<PackageItemRootViewModel> _packageContents = null;
        public List<PackageItemRootViewModel> PackageContents
        {
            get
            {
                _packageContents = FunctionDefinitions.Select((def) => new PackageItemRootViewModel(def))
                                                      .ToList();
                return _packageContents;
            }
        }

        /// <summary>
        /// AdditionalFiles property </summary>
        /// <value>
        /// Tells whether the publish UI is visible</value>
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
                    dynSettings.Controller.UIDispatcher.BeginInvoke(
                        (Action)(() => (this.SubmitCommand).RaiseCanExecuteChanged()));
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
                    dynSettings.Controller.UIDispatcher.BeginInvoke(
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
                    if (value.Length != 1) value = value.TrimStart(new char[] {'0'});
                    this._MinorVersion = value;
                    this.RaisePropertyChanged("MinorVersion");
                    dynSettings.Controller.UIDispatcher.BeginInvoke(
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
                    if (value.Length != 1) value = value.TrimStart(new char[] {'0'});
                    this._BuildVersion = value;
                    this.RaisePropertyChanged("BuildVersion");
                    dynSettings.Controller.UIDispatcher.BeginInvoke(
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
                    if (value.Length != 1) value = value.TrimStart(new char[] {'0'});
                    this._MajorVersion = value;
                    this.RaisePropertyChanged("MajorVersion");
                    dynSettings.Controller.UIDispatcher.BeginInvoke(
                        (Action)(() => (this.SubmitCommand).RaiseCanExecuteChanged()));
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
        /// The package used for this submission
        /// </summary>
        public Package Package { get; set; }

        /// <summary>
        /// CustomNodeDefinition property </summary>
        /// <value>
        /// The FuncDefinition for the current package to be uploaded</value>
        private List<CustomNodeDefinition> _FunctionDefinitions;
        public List<CustomNodeDefinition> FunctionDefinitions
        {
            get { return _FunctionDefinitions; }
            set
            {
                _FunctionDefinitions = value;
                this.Name = FunctionDefinitions[0].WorkspaceModel.Name;
                this.UpdateDependencies();
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

        /// <summary>
        /// The class constructor. </summary>
        /// <param name="client"> Reference to to the PackageManagerClient object for the app </param>
        public PublishPackageViewModel(PackageManagerClient client)
        {
            Client = client;
            this.SubmitCommand = new DelegateCommand(this.Submit, this.CanSubmit);
            this.ShowAddFileDialogAndAddCommand = new DelegateCommand(this.ShowAddFileDialogAndAdd, this.CanShowAddFileDialogAndAdd);
            this.Dependencies = new ObservableCollection<PackageDependency>();
        }

        public static PublishPackageViewModel FromLocalPackage(Package l)
        {

            var vm = new PublishPackageViewModel(dynSettings.PackageManagerClient)
                {
                    Group = l.Group,
                    Description = l.Description,
                    Keywords = l.Keywords != null ? String.Join(" ", l.Keywords ) : ""
                };

            vm.FunctionDefinitions =
                l.LoadedCustomNodes.Select(x => dynSettings.CustomNodeManager.GetFunctionDefinition(x.Guid)).ToList();

            if (l.VersionName != null)
            {
                var parts = l.VersionName.Split('.');
                if (parts.Count() == 3)
                {
                    vm.MajorVersion = parts[0];
                    vm.MinorVersion = parts[1];
                    vm.BuildVersion = parts[2];
                }
            }

            vm.Name = l.Name;
            vm.Package = l;

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
                this.ErrorString = ((PackageUploadHandle) sender).ErrorString;
                this.Uploading = false;
            } 
        }

        private IEnumerable<CustomNodeDefinition> AllDependentFuncDefs()
        {
            return
                FunctionDefinitions.Select(x => x.Dependencies)
                                   .SelectMany(x => x)
                                   .Where(x => !FunctionDefinitions.Contains(x))
                                   .Distinct();
        }

        private IEnumerable<CustomNodeDefinition> AllFuncDefs()
        {
            return AllDependentFuncDefs().Union(FunctionDefinitions).Distinct();
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
                throw new Exception("The following workspaces have not been saved " +
                                    String.Join(", ", unsavedWorkspaceNames) + ". Please save them and try again.");
            }

            // omit files currently already under package control
            var files =
                allFuncs.Select(f => f.WorkspaceModel.FileName)
                        .Where(p =>
                                (dynSettings.PackageLoader.IsUnderPackageControl(p) &&
                                dynSettings.PackageLoader.GetOwnerPackage(p).Name == this.Name) || !dynSettings.PackageLoader.IsUnderPackageControl(p));

            // union with additional files
            files = files.Union(this.AdditionalFiles);

            return files;
        }

        private void UpdateDependencies(){
            this.Dependencies.Clear();
            this.GetAllDependencies().ToList().ForEach(this.Dependencies.Add);
        }

        private IEnumerable<PackageDependency> GetAllDependencies()
        {
            // get all of dependencies from custom nodes and additional files
            var allFilePackages =
                AllDependentFuncDefs()
                    .Select(x => x.WorkspaceModel.FileName)
                    .Union( AdditionalFiles )
                    .Where(dynSettings.PackageLoader.IsUnderPackageControl)
                    .Select(dynSettings.PackageLoader.GetOwnerPackage)
                    .Where(x => x != null)
                    .Where(x => (x.Name != this.Name))
                    .Distinct()
                    .Select(x => new PackageDependency(x.Name, x.VersionName));

            // get all of the dependencies from types
            var allTypePackages = AllFuncDefs()
                .Select(x => x.WorkspaceModel.Nodes)
                .SelectMany(x => x)
                .Select(x => x.GetType())
                .Where(dynSettings.PackageLoader.IsUnderPackageControl)
                .Select(dynSettings.PackageLoader.GetOwnerPackage)
                .Where(x => x != null)
                .Where(x => (x.Name != this.Name) )
                .Distinct()
                .Select(x => new PackageDependency(x.Name, x.VersionName));

            return allFilePackages.Union(allTypePackages);

        }

        private IEnumerable<Tuple<string, string>> GetAllNodeNameDescriptionPairs()
        {
            // TODO: include descriptions for all compiled nodes

            // collect the name-description pairs for every custom node
            return
                AllFuncDefs()
                    .Where(p =>
                                (dynSettings.PackageLoader.IsUnderPackageControl(p) &&
                                dynSettings.PackageLoader.GetOwnerPackage(p).Name == this.Name) || !dynSettings.PackageLoader.IsUnderPackageControl(p))
                        .Select(x => new Tuple<string, string>(x.WorkspaceModel.Name, !String.IsNullOrEmpty(x.WorkspaceModel.Description) ? x.WorkspaceModel.Description : "No description provided" ));
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

        private void ShowAddFileDialogAndAdd()
        {
            // show file open dialog
            FileDialog fDialog = null;

            if (fDialog == null)
            {
                fDialog = new OpenFileDialog()
                {
                    Filter = "Dynamo Custom Node Definitions (*.dyf)|*.dyf",
                    Title = "Add Custom Node To Package..."
                };
            }

            // if you've got the current space path, use it as the inital dir
            if (!string.IsNullOrEmpty(dynSettings.Controller.DynamoViewModel.Model.CurrentWorkspace.FileName))
            {
                var fi = new FileInfo(dynSettings.Controller.DynamoViewModel.Model.CurrentWorkspace.FileName);
                fDialog.InitialDirectory = fi.DirectoryName;
            }
            else // use the definitions directory
            {
                if (Directory.Exists(DynamoPathManager.Instance.UserDefinitions))
                {
                    fDialog.InitialDirectory = DynamoPathManager.Instance.UserDefinitions;
                }
            }

            if (fDialog.ShowDialog() == DialogResult.OK)
            {
                
                var nodeInfo = dynSettings.Controller.CustomNodeManager.AddFileToPath(fDialog.FileName);
                if (nodeInfo != null)
                {
                    // add the new packages folder to path
                    dynSettings.Controller.CustomNodeManager.AddDirectoryToSearchPath(Path.GetDirectoryName(fDialog.FileName));
                    dynSettings.Controller.CustomNodeManager.UpdateSearchPath();
                    
                    var funcDef = dynSettings.Controller.CustomNodeManager.GetFunctionDefinition(nodeInfo.Guid);

                    if (funcDef != null && this.FunctionDefinitions.All(x => x.FunctionId != funcDef.FunctionId))
                    {
                        this.FunctionDefinitions.Add(funcDef);
                        this.GetAllDependencies();
                        this.RaisePropertyChanged("PackageContents");
                    }
                }
            }
        }

        private bool CanShowAddFileDialogAndAdd()
        {
            return true;
        }

        /// <summary>
        /// Delegate used to submit the element</summary>
        private void Submit()
        {

            try
            {
                var newpkg = Package == null;

                Package = Package ?? new Package("", this.Name, this.FullVersion);

                Package.VersionName = FullVersion;
                Package.Description = Description;
                Package.Group = Group;
                Package.Keywords = KeywordList;

                var files = GetAllFiles().ToList();

                Package.Contents = String.Join(", ", GetAllNodeNameDescriptionPairs().Select((pair) => pair.Item1 + " - " + pair.Item2));

                Package.Dependencies.Clear();
                GetAllDependencies().ToList().ForEach(Package.Dependencies.Add);

                if (newpkg) dynSettings.PackageLoader.LocalPackages.Add(Package);

                var handle = Client.Publish(Package, files, IsNewVersion);

                if (handle == null)
                    throw new Exception("Failed to authenticate.  Are you logged in?");

                this.Uploading = true;
                this.UploadHandle = handle;

            }
            catch (Exception e)
            {
                ErrorString = e.Message;
                dynSettings.DynamoLogger.Log(e);
            }

        }

        /// <summary>
        /// Delegate used to submit the element </summary>
        private bool CanSubmit()
        {

            if (Description.Length <= 10)
            {
                this.ErrorString = "Description must be longer than 10 characters.";
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

            if ( Double.Parse( this.BuildVersion) + Double.Parse( this.MinorVersion) + Double.Parse( this.MajorVersion ) <= 0 )
            {
                this.ErrorString = "At least one of your version values must be greater than 0.";
                return false;
            }

            if ( this.UploadState != PackageUploadHandle.State.Error ) this.ErrorString = "";

            if (this.Uploading) return false;

            return true;
        }

    }

}
