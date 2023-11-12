using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using Dynamo.Core;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.PackageManager.UI;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Properties;
using Dynamo.Wpf.Utilities;
using DynamoUtilities;
using Greg.Requests;
using PythonNodeModels;
using Double = System.Double;
using String = System.String;
using NotificationObject = Dynamo.Core.NotificationObject;
using Prism.Commands;
using System.Runtime.InteropServices;

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
        /// Default license type for each package, no need to localize
        /// </summary>
        private readonly string defaultLicense = "MIT";

        /// <summary>
        /// reference of DynamoViewModel
        /// </summary>
        public DynamoViewModel DynamoViewModel
        {
            get { return dynamoViewModel; }
        }

        /// <summary>
        /// Package Publish entry, binded to the host multi-selection option
        /// </summary>
        public class HostComboboxEntry
        {
            /// <summary>
            /// Name of the host
            /// </summary>
            public string HostName { get; set; }

            /// <summary>
            /// Boolean indicates if the host entry is selected
            /// </summary>
            public bool IsSelected { get; set; }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="hostName">Name of the host</param>
            public HostComboboxEntry(string hostName)
            {
                HostName = hostName;
                IsSelected = false;
            }
        }

        public PublishPackageView Owner { get; set; }

        /// <summary>
        /// A event called when publishing was a success
        /// </summary>
        public event PublishSuccessHandler PublishSuccess;

        public event EventHandler<PackagePathEventArgs> RequestShowFolderBrowserDialog;
        public virtual void OnRequestShowFileDialog(object sender, PackagePathEventArgs e)
        {
            if (RequestShowFolderBrowserDialog != null)
            {
                RequestShowFolderBrowserDialog(sender, e);
            }
        }

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
                    BeginInvoke(() =>
                    {
                        SubmitCommand.RaiseCanExecuteChanged();
                        PublishLocallyCommand.RaiseCanExecuteChanged();
                    });
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
                    BeginInvoke(() =>
                    {
                        SubmitCommand.RaiseCanExecuteChanged();
                        PublishLocallyCommand.RaiseCanExecuteChanged();
                    });
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
                    BeginInvoke(() =>
                    {
                        SubmitCommand.RaiseCanExecuteChanged();
                        PublishLocallyCommand.RaiseCanExecuteChanged();
                    });
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
                    BeginInvoke(() =>
                    {
                        SubmitCommand.RaiseCanExecuteChanged();
                        PublishLocallyCommand.RaiseCanExecuteChanged();
                    });
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
                    BeginInvoke(() =>
                    {
                        SubmitCommand.RaiseCanExecuteChanged();
                        PublishLocallyCommand.RaiseCanExecuteChanged();
                    });
                }
            }
        }

        private string _license;
        /// <summary>
        /// License property
        /// </summary>
        /// <value> The license for the package </value>
        public string License
        {
            get { return _license; }
            set
            {
                if (_license != value)
                {
                    _license = value;
                    RaisePropertyChanged(nameof(License));
                }
            }
        }

        private string copyrightHolder;
        /// <summary>
        /// The name of the author who holds this package's copyright.
        /// </summary>
        public string CopyrightHolder
        {
            get => copyrightHolder;
            set
            {
                copyrightHolder = value;
                RaisePropertyChanged(nameof(CopyrightHolder));
            }
        }

        private string copyrightYear;
        /// <summary>
        /// The year of this package's copyright.
        /// </summary>
        public string CopyrightYear
        {
            get => copyrightYear;
            set
            {
                copyrightYear = value;
                RaisePropertyChanged(nameof(CopyrightYear));
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
                    BeginInvoke(() =>
                    {
                        SubmitCommand.RaiseCanExecuteChanged();
                        PublishLocallyCommand.RaiseCanExecuteChanged();
                    });
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

        private List<HostComboboxEntry> knownHosts;

        /// <summary>
        /// Know hosts received from package manager.
        /// Data binded to the multi-selection host dependency option.
        /// </summary>
        public List<HostComboboxEntry> KnownHosts
        {
            get { return knownHosts; }
            set
            {
                if (knownHosts != value)
                {
                    knownHosts = value;
                    RaisePropertyChanged(nameof(KnownHosts));
                }
            }
        }

        private List<string> selectedHosts = new List<String>();
        /// <summary>
        /// Current selected hosts as dependencies.
        /// Will be passed for serialization when publishing package.
        /// </summary>
        public List<string> SelectedHosts
        {
            get { return selectedHosts; }
            set
            {
                if (selectedHosts != value && value != null)
                {
                    selectedHosts = value;
                    // The following logic is mainly for publishing from an existing package with
                    // pre-serialized host dependencies. We set the selection state so user do not have 
                    // to replicate the selection again
                    foreach (var host in KnownHosts)
                    {
                        if (selectedHosts.Contains(host.HostName))
                        {
                            host.IsSelected = true;
                            SelectedHostsString += host.HostName + ", ";
                        }
                    }
                    // Format string since it will be displayed
                    SelectedHostsString = SelectedHostsString.Trim().TrimEnd(',');
                    RaisePropertyChanged( nameof(SelectedHosts));
                    RaisePropertyChanged( nameof(SelectedHostsString));
                }
            }
        }

        private string selectedHostsString = string.Empty;
        /// <summary>
        /// Current selected hosts as dependencies string for display
        /// </summary>
        public string SelectedHostsString
        {
            get { return selectedHostsString; }
            set
            {
                if (selectedHostsString != value)
                {
                    selectedHostsString = value;
                    RaisePropertyChanged(nameof(SelectedHostsString));
                }
            }
        }

        /// <summary>
        /// Boolean indicating if the current publishing package is depending on other package
        /// </summary>
        public bool HasDependencies
        {
            get { return Dependencies.Count > 0; }
        }

        /// <summary>
        /// This property seems dup with HasDependencies
        /// TODO: Remove in Dynamo 3.0
        /// </summary>
        public bool HasNoDependencies
        {
            get { return !HasDependencies; }
        }

        private string markdownFilesDirectory;

        /// <summary>
        /// An optional directory, specified by the user, which holds guidance documents
        /// for their nodes or packages in the markdown format.
        /// </summary>
        public string MarkdownFilesDirectory
        {
            get => markdownFilesDirectory;
            set
            {
                markdownFilesDirectory = value;
                RaisePropertyChanged(nameof(MarkdownFilesDirectory));
            }
        }

        /// <summary>
        /// SubmitCommand property </summary>
        /// <value>
        /// A command which, when executed, submits the current package</value>
        public DelegateCommand SubmitCommand { get; private set; }

        /// <summary>
        /// PublishLocallyCommand property </summary>
        /// <value>
        /// A command which, when executed, publish the current package to a local folder</value>
        public DelegateCommand PublishLocallyCommand { get; private set; }

        /// <summary>
        /// A command which, when executed, adds the selected file(s) to the PackageContents.
        /// </summary>
        public DelegateCommand ShowAddFileDialogAndAddCommand { get; private set; }

        /// <summary>
        /// A command which, when executed, recursively adds the selected folders and their
        /// subfolders to the PackageContents 
        /// </summary>
        public DelegateCommand SelectDirectoryAndAddFilesRecursivelyCommand { get; set; }

        /// <summary>
        /// SelectMarkdownDirectoryCommand property. A command which, when executed,
        /// opens the directory selection dialog and prompts the user to specify
        /// a directory for their (optional) markdown files. </summary>
        public DelegateCommand SelectMarkdownDirectoryCommand { get; private set; }

        /// <summary>
        /// A command, fired by the 'Reset' button in the PublishPackageView.
        /// Sets the MarkdownDirectory property to an empty string.
        /// </summary>
        public DelegateCommand ClearMarkdownDirectoryCommand { get; private set; }

        /// <summary>
        /// A command, fired by the trash can icon in the Package Contents Datagrid.
        /// Used to remove an item from the list of package contents.
        /// </summary>
        public Dynamo.UI.Commands.DelegateCommand RemoveItemCommand { get; private set; }

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
        public ObservableCollection<PackageItemRootViewModel> PackageContents { get; set; } = new ObservableCollection<PackageItemRootViewModel>();

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

                RefreshPackageContents();
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
        /// Optional Markdown files list
        /// </summary>
        internal IEnumerable<string> MarkdownFiles;

        /// <summary>
        /// Dependencies property </summary>
        /// <value>
        /// Computed and manually added package dependencies</value>
        public ObservableCollection<PackageDependency> Dependencies { get; set; } = new ObservableCollection<PackageDependency>();

        /// <summary>
        /// A user-facing comma-separated string of this package's dependencies.
        /// </summary>
        public string DependencyNames
        {
            get => dependencyNames;
            set
            {
                dependencyNames = value; 
                RaisePropertyChanged(nameof(DependencyNames));
            }
        }

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
                SelectedHosts = value.host_dependencies as List<string>;
                _dynamoBaseHeader = value;
            }
        }

        private bool isWarningEnabled;
        /// <summary>
        /// This flag will be in true when the package name is invalid
        /// </summary>
        public bool IsWarningEnabled
        {
            get
            {
                return isWarningEnabled;
            }
            set
            {
                isWarningEnabled = value;
                RaisePropertyChanged(nameof(IsWarningEnabled));
            }
        }

        private string currentWarningMessage;
        /// <summary>
        /// This property will hold the warning message that has to be shown in the warning icon next to the TextBox
        /// </summary>
        public string CurrentWarningMessage
        {
            get
            {
                return currentWarningMessage;
            }
            set
            {
                currentWarningMessage = value;
                RaisePropertyChanged(nameof(CurrentWarningMessage));
            }
        }

        private static MetadataLoadContext sharedMetaDataLoadContext = null;
        /// <summary>
        /// A shared MetaDataLoadContext that is used for assembly inspection during package publishing.
        /// This member is shared so the behavior is similar to the ReflectionOnlyLoadContext this is replacing.
        /// TODO - eventually it would be good to move to separate publish load contexts that are cleaned up at the appropriate time(?).
        /// </summary>
        private static MetadataLoadContext SharedPublishLoadContext
        {
            get
            {
                sharedMetaDataLoadContext ??= InitSharedPublishLoadContext();
                return sharedMetaDataLoadContext;
            }
        }


        #endregion

        internal PublishPackageViewModel()
        {
            customNodeDefinitions = new List<CustomNodeDefinition>();
            SubmitCommand = new DelegateCommand(Submit, CanSubmit);
            PublishLocallyCommand = new DelegateCommand(PublishLocally, CanPublishLocally);
            ShowAddFileDialogAndAddCommand = new DelegateCommand(ShowAddFileDialogAndAdd, CanShowAddFileDialogAndAdd);
            SelectDirectoryAndAddFilesRecursivelyCommand = new DelegateCommand(SelectDirectoryAndAddFilesRecursively);
            SelectMarkdownDirectoryCommand = new DelegateCommand(SelectMarkdownDirectory);
            ClearMarkdownDirectoryCommand = new DelegateCommand(ClearMarkdownDirectory);
            RemoveItemCommand = new Dynamo.UI.Commands.DelegateCommand(RemoveItem);
            ToggleMoreCommand = new DelegateCommand(() => MoreExpanded = !MoreExpanded, () => true);
            Dependencies.CollectionChanged += DependenciesOnCollectionChanged;
            Assemblies = new List<PackageAssembly>();
            MarkdownFiles = new List<string>();
            PropertyChanged += ThisPropertyChanged;
            RefreshPackageContents();
            RefreshDependencyNames();
        }

        private void DependenciesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => RefreshDependencyNames();
        
        private void RefreshDependencyNames()
        {
            if (Dependencies.Count < 1)
            {
                DependencyNames = Properties.Resources.NoneString;
                return;
            }
            DependencyNames = string.Join(", ", Dependencies.Select(x => x.name));
        }

        private void RefreshPackageContents()
        {
            PackageContents.Clear();
            
            var itemsToAdd = CustomNodeDefinitions
                .Select(def => new PackageItemRootViewModel(def))
                .Concat(Assemblies.Select((pa) => new PackageItemRootViewModel(pa)))
                .Concat(AdditionalFiles.Select((s) => new PackageItemRootViewModel(new FileInfo(s))))
                .ToList()
                .ToObservableCollection();

            foreach (var item in itemsToAdd) PackageContents.Add(item);
        }

        /// <summary>
        /// Return a list of HostComboboxEntry describing known hosts from PM.
        /// Return an empty list if PM is down.
        /// </summary>
        /// <returns>A list of HostComboboxEntry describing known hosts from PM.</returns>
        private List<HostComboboxEntry> initializeHostSelections()
        {
            var hostSelections = new List<HostComboboxEntry>();
            foreach (var host in dynamoViewModel.PackageManagerClientViewModel.Model.GetKnownHosts())
            {
                hostSelections.Add(new HostComboboxEntry(host));
            }
            return hostSelections;
        }

        private void BeginInvoke(Action action)
        {
            // dynamoViewModel.UIDispatcher can be null in unit tests.
            if (dynamoViewModel != null && dynamoViewModel.UIDispatcher != null)
                dynamoViewModel.UIDispatcher.BeginInvoke(action);
        }

        /// <summary>
        /// The class constructor. </summary>
        public PublishPackageViewModel( DynamoViewModel dynamoViewModel ) : this()
        {
            this.dynamoViewModel = dynamoViewModel;
            KnownHosts = initializeHostSelections();
            isWarningEnabled = false;
        }

        private void ClearAllEntries()
        {
            // this function clears all the entries of the publish package dialog
            this.Name = string.Empty;
            this.RepositoryUrl = string.Empty;
            this.SiteUrl = string.Empty;
            this.License = string.Empty;
            this.Keywords = string.Empty;
            this.Description = string.Empty;
            this.Group = string.Empty;
            this.MajorVersion = "0";
            this.MinorVersion = "0";
            this.BuildVersion = "0";
            this.ErrorString = string.Empty;
            this.Uploading = false;
            this.UploadHandle = null;
            this.IsNewVersion = false;
            this.MoreExpanded = false;
            this.ClearPackageContents();
            this.ClearMarkdownDirectory();
            this.UploadState = PackageUploadHandle.State.Ready;
            this.AdditionalFiles = new ObservableCollection<string>();
            this.Dependencies = new ObservableCollection<PackageDependency>();
            this.Assemblies = new List<PackageAssembly>();
            this.SelectedHosts = new List<String>();
            this.SelectedHostsString = string.Empty;
            this.copyrightHolder = string.Empty;
            this.copyrightYear = string.Empty;
        }

        private void ClearPackageContents()
        {
            //  this method clears the package contents in the publish package dialog

            this.Package = null;
            this.CustomNodeDefinitions = new List<CustomNodeDefinition>();
            RaisePropertyChanged("PackageContents");
        }

        private void ThisPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PackageContents")
            {
                CanSubmit();
               SubmitCommand.RaiseCanExecuteChanged();
               PublishLocallyCommand.RaiseCanExecuteChanged();
            }
        }

        public static PublishPackageViewModel FromLocalPackage(DynamoViewModel dynamoViewModel, Package pkg)
        {
            var defs = new List<CustomNodeDefinition>();

            foreach (var x in pkg.LoadedCustomNodes)
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

            var pkgViewModel = new PublishPackageViewModel(dynamoViewModel)
            {
                Group = pkg.Group,
                Description = pkg.Description,
                Keywords = pkg.Keywords != null ? String.Join(" ", pkg.Keywords) : "",
                CustomNodeDefinitions = defs,
                Name = pkg.Name,
                RepositoryUrl = pkg.RepositoryUrl ?? "",
                SiteUrl = pkg.SiteUrl ?? "",
                Package = pkg,
                License = pkg.License,
                SelectedHosts = pkg.HostDependencies as List<string>,
                CopyrightHolder = pkg.CopyrightHolder,
                CopyrightYear = pkg.CopyrightYear
            };

            // add additional files
            pkg.EnumerateAdditionalFiles();
            foreach (var file in pkg.AdditionalFiles)
            {
                pkgViewModel.AdditionalFiles.Add(file.Model.FullName);
            }

            var nodeLibraryNames = pkg.Header.node_libraries;

            var assembliesLoadedTwice = new List<string>();
            foreach (var file in pkg.EnumerateAssemblyFilesInPackage())
            {
                Assembly assem;
                var result = PackageLoader.TryMetaDataContextLoad(file, SharedPublishLoadContext, out assem);

                switch (result)
                {
                    case AssemblyLoadingState.Success:
                        {
                            var isNodeLibrary = nodeLibraryNames == null || nodeLibraryNames.Contains(assem.FullName);
                            pkgViewModel.Assemblies.Add(new PackageAssembly()
                            {
                                IsNodeLibrary = isNodeLibrary,
                                Assembly = assem
                            });
                            break;
                        }
                    case AssemblyLoadingState.NotManagedAssembly:
                        {
                            // if it's not a .NET assembly, we load it as an additional file
                            pkgViewModel.AdditionalFiles.Add(file);
                            break;
                        }
                    case AssemblyLoadingState.AlreadyLoaded:
                        {
                            assembliesLoadedTwice.Add(file);
                            break;
                        }
                }
            }

            //after dependencies are loaded refresh package contents
            pkgViewModel.RefreshPackageContents();
            pkgViewModel.UpdateDependencies();

            if (assembliesLoadedTwice.Any())
            {
                pkgViewModel.UploadState = PackageUploadHandle.State.Error;
                pkgViewModel.ErrorString = Resources.OneAssemblyWasLoadedSeveralTimesErrorMessage + string.Join("\n", assembliesLoadedTwice);
            }

            if (pkg.VersionName == null) return pkgViewModel;

            var parts = pkg.VersionName.Split('.');
            if (parts.Count() != 3) return pkgViewModel;

            pkgViewModel.MajorVersion = parts[0];
            pkgViewModel.MinorVersion = parts[1];
            pkgViewModel.BuildVersion = parts[2];
            return pkgViewModel;

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

        private enum GetFunctionDefinition
        {
            AllDependentFuncDefs,
            AllFuncDefs
        }
        private List<CustomNodeWorkspaceModel> GetFunctionDefinitionWS(GetFunctionDefinition gfd)
        {
            var workspaces = new List<CustomNodeWorkspaceModel>();
            if (gfd == GetFunctionDefinition.AllDependentFuncDefs)
            {
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
            }
            else if (gfd == GetFunctionDefinition.AllFuncDefs)
            {
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
            }
            return workspaces;
        }

        private IEnumerable<string> GetAllUnqualifiedFiles()
        {
            // all workspaces
            var workspaces = GetFunctionDefinitionWS(GetFunctionDefinition.AllFuncDefs);

            var pmExtension = dynamoViewModel.Model.GetPackageManagerExtension();

            // Get all unqualified files to notify to users
            var files =
               workspaces.Select(f => f.FileName)
                   .Where(
                       p => (
                             pmExtension.PackageLoader.IsUnderPackageControl(p)
                             && (pmExtension.PackageLoader.GetOwnerPackage(p).Name != Name)
                            )
                          );

            return files;

        }

        internal IEnumerable<string> GetAllFiles()
        {
            // all workspaces
            var workspaces = GetFunctionDefinitionWS(GetFunctionDefinition.AllFuncDefs);

            // make sure workspaces are saved
            var unsavedWorkspaceNames =
                workspaces.Where(ws => ws.HasUnsavedChanges || ws.FileName == null).Select(ws => ws.Name).ToList();
            if (unsavedWorkspaceNames.Any())
            {
                throw new Exception(Wpf.Properties.Resources.MessageUnsavedChanges0 +
                                    String.Join(", ", unsavedWorkspaceNames) +
                                    Wpf.Properties.Resources.MessageUnsavedChanges1);
            }

            var pmExtension = dynamoViewModel.Model.GetPackageManagerExtension();
            // omit files currently already under package control
            var files =
                workspaces.Select(f => f.FileName)
                    .Where(
                        p =>
                            (pmExtension.PackageLoader.IsUnderPackageControl(p)
                                && (pmExtension.PackageLoader.GetOwnerPackage(p).Name == Name)
                                || !pmExtension.PackageLoader.IsUnderPackageControl(p)));

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
            var pmExtension = dynamoViewModel.Model.GetPackageManagerExtension();
            var pkgLoader = pmExtension.PackageLoader;

            // all workspaces
            var workspaces = GetFunctionDefinitionWS(GetFunctionDefinition.AllDependentFuncDefs);

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

            workspaces = GetFunctionDefinitionWS(GetFunctionDefinition.AllFuncDefs);

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

        private List<string> GetPythonDependency()
        {
            var pmExtension = dynamoViewModel.Model.GetPackageManagerExtension();
            var pkgLoader = pmExtension.PackageLoader;

            var workspaces = GetFunctionDefinitionWS(GetFunctionDefinition.AllDependentFuncDefs);

            // get python engine from custom nodes and their dependent packages
            var allDepPackagesPythonEngine =
                workspaces
                    .Select(x => x.FileName)
                    .Union(AdditionalFiles)
                    .Where(pkgLoader.IsUnderPackageControl)
                    .Select(pkgLoader.GetOwnerPackage)
                    .Where(x => x != null)
                    .Where(x => (x.Name != Name)).Distinct()
                    .Select(x => x.HostDependencies)
                    .SelectMany(x => x)
                    .Distinct();

            workspaces = GetFunctionDefinitionWS(GetFunctionDefinition.AllFuncDefs);

            // get python engine from custom nodes that include python scripts
            var pythonEngineDirectDep = workspaces
                .SelectMany(x => x.Nodes)
                .Where(x => x is PythonNode)
                .Select(x => ((PythonNode)x).EngineName)
                .Distinct();

            return pythonEngineDirectDep.Union(allDepPackagesPythonEngine).ToList();

        }

        private IEnumerable<Tuple<string, string>> GetAllNodeNameDescriptionPairs()
        {
            var pmExtension = dynamoViewModel.Model.GetPackageManagerExtension();
            var pkgLoader = pmExtension.PackageLoader;

            var workspaces = GetFunctionDefinitionWS(GetFunctionDefinition.AllFuncDefs);

            // collect the name-description pairs for every custom node
            return
                workspaces
                    .Where(
                        p =>
                            (pkgLoader.IsUnderPackageControl(p.FileName) && pkgLoader.GetOwnerPackage(p.FileName).Name == Name)
                                || !pmExtension.PackageLoader.IsUnderPackageControl(p.FileName))
                    .Select(
                        x =>
                            new Tuple<string, string>(
                            x.Name,
                            !String.IsNullOrEmpty(x.Description)
                                ? x.Description
                                : Wpf.Properties.Resources.MessageNoNodeDescription));
        }

        private string _errorString = "";
        private string dependencyNames;

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
            var fDialog = new OpenFileDialog
            {
                Filter = string.Format(Resources.FileDialogCustomNodeDLLXML, "*.dyf;*.dll;*.xml") + "|" +
                         string.Format(Resources.FileDialogAllFiles, "*.*"),
                Title = Resources.AddCustomFileToPackageDialogTitle,
                Multiselect = true
            };

            // if you've got the current space path, add it to shortcuts 
            // so that user is able to easily navigate there
            var currentFileName = dynamoViewModel.Model.CurrentWorkspace.FileName;
            if (!string.IsNullOrEmpty(currentFileName))
            {
                var fi = new FileInfo(currentFileName);
                fDialog.CustomPlaces.Add(fi.DirectoryName);
            }
            
            // add the definitions directory to shortcuts as well
            var pathManager = dynamoViewModel.Model.PathManager;
            if (Directory.Exists(pathManager.DefaultUserDefinitions))
            {
                fDialog.CustomPlaces.Add(pathManager.DefaultUserDefinitions);
            }

            if (fDialog.ShowDialog() != DialogResult.OK) return;

            UploadState = PackageUploadHandle.State.Ready;

            foreach (var file in fDialog.FileNames)
            {
                AddFile(file);
            }
            RefreshPackageContents();
            RaisePropertyChanged(nameof(PackageContents));
            RefreshDependencyNames();
        }

        /// <summary>
        /// Method linked to SelectDirectoryAndAddFilesRecursivelyCommand.
        /// Opens the Select Folder dialog and prompts the user to select a directory.
        /// Recursively adds any files found in the given directory to PackageContents.
        /// </summary>
        private void SelectDirectoryAndAddFilesRecursively()
        {
            PathManager pathManager = DynamoViewModel.Model.PathManager as PathManager;
            PackagePathEventArgs packagePathEventArgs = new PackagePathEventArgs
            {
                Path = pathManager.DefaultPackagesDirectory
            };

            OnRequestShowFileDialog(this, packagePathEventArgs);

            if (packagePathEventArgs.Cancel) return;

            string directoryPath = packagePathEventArgs.Path;

            List<string> filePaths = Directory
                .GetFiles
                (
                    directoryPath,
                    "*",
                    SearchOption.AllDirectories
                ).ToList();

            if (filePaths.Count < 1) return;

            List<string> existingPackageContents = PackageContents
                .Where(x => x.FileInfo != null)
                .Select(x => x.FileInfo.FullName)
                .ToList();

            foreach (var filePath in filePaths)
            {
                if (existingPackageContents.Contains(filePath)) continue;
                AddFile(filePath);
            }

            RefreshPackageContents();
            RaisePropertyChanged(nameof(PackageContents));
            RefreshDependencyNames();
        }

        /// <summary>
        /// Method linked to the SelectMarkdownDirectoryCommand.
        /// Prompts the user to specify a directory containing markdown files, which if successful,
        /// is saved to the MarkdownFilesDirectory property.
        /// </summary>
        private void SelectMarkdownDirectory()
        {
            PathManager pathManager = DynamoViewModel.Model.PathManager as PathManager;
            PackagePathEventArgs packagePathEventArgs = new PackagePathEventArgs
            {
                Path = pathManager.DefaultPackagesDirectory
            };

            OnRequestShowFileDialog(this, packagePathEventArgs);

            if (packagePathEventArgs.Cancel) return;

            string directoryPath = packagePathEventArgs.Path;

            if (!IsDirectoryWritable(directoryPath))
            {
                ErrorString = String.Format(Resources.FolderNotWritableError, directoryPath);
                string errorMessage = ErrorString + Environment.NewLine + Resources.SolutionToFolderNotWritatbleError;
                if (DynamoModel.IsTestMode) return;
                MessageBoxService.Show(errorMessage, Resources.FileNotPublishCaption, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            MarkdownFilesDirectory = directoryPath;

            // Store all files paths from the markdown directory to list without affecting the package content UI
            MarkdownFiles = Directory
                .GetFiles
                (
                    directoryPath,
                    "*",
                    SearchOption.AllDirectories
                ).ToList();
        }

        /// <summary>
        /// Method linked to the ClearMarkdownDirectoryCommand.
        /// Sets the MarkdownFilesDirectory to an empty string.
        /// </summary>
        private void ClearMarkdownDirectory() => MarkdownFilesDirectory = string.Empty;

        /// <summary>
        /// Removes an item from the package contents list.
        /// </summary>
        private void RemoveItem(object parameter)
        {
            if (!(parameter is PackageItemRootViewModel packageItemRootViewModel)) return;

            string fileName = packageItemRootViewModel.FileInfo == null ? packageItemRootViewModel.Name : packageItemRootViewModel.FileInfo.FullName;
            DependencyType fileType = packageItemRootViewModel.DependencyType;

            if (fileName.ToLower().EndsWith(".dll") || fileType.Equals(DependencyType.Assembly))
            {
                Assemblies.Remove(Assemblies
                    .First(x => x.Name == Path.GetFileNameWithoutExtension(fileName)));
            }
            else if (fileType.Equals(DependencyType.CustomNode))
            {
                CustomNodeDefinitions.Remove(CustomNodeDefinitions
                    .First(x => x.DisplayName == fileName));
            }
            else
            {
                AdditionalFiles.Remove(AdditionalFiles
                    .First(x => x == fileName));
            }
                        
            RefreshPackageContents();
            return;
        }

        private bool CanShowAddFileDialogAndAdd()
        {
            return true;
        }

        internal void AddFile(string filename)
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

                // Check the ID to confirm weather the file is contained in any package.
                // If Yes, then update the loadedworkspace node info to this file location
                // This is to solve the issues when user copy a file from a given package to new location
                // and start publishing with the new copied file.
                // Since the loadedworkspace node info location is update, the issue is fixed.

                foreach(var node in dynamoViewModel.Model.CustomNodeManager.LoadedWorkspaces)
                {
                    if(node.CustomNodeId == dynamoViewModel.Model.CustomNodeManager.GuidFromPath(filename))
                    {
                        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
                        node.SetInfo(fileNameWithoutExtension,null,null,filename);
                    }
                }

                if (dynamoViewModel.Model.CustomNodeManager.TryGetFunctionDefinition(nodeInfo.FunctionId, DynamoModel.IsTestMode, out CustomNodeDefinition funcDef)
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
                UploadState = PackageUploadHandle.State.Error;
                ErrorString = String.Format(Resources.MessageFailedToAddFile, filename);
                dynamoViewModel.Model.Logger.Log(e);
            }
        }

        private void AddDllFile(string filename)
        {
            try
            {
                // we're not sure if this is a managed assembly or not
                // we try to load it, if it fails - we add it as an additional file
                var result = PackageLoader.TryLoadFrom(filename, out Assembly assem);
                if (result)
                {
                    var assemName = assem.GetName().Name;

                    // The user has attempted to load an existing dll from another path. This is not allowed 
                    // as the existing assembly cannot be modified while Dynamo is active.
                    if (this.Assemblies.Any(x => assemName == x.Assembly.GetName().Name))
                    {
                        MessageBoxService.Show(string.Format(Resources.PackageDuplicateAssemblyWarning, 
                                        dynamoViewModel.BrandingResourceProvider.ProductName),
                                        Resources.PackageDuplicateAssemblyWarningTitle, 
                                        MessageBoxButton.OK, 
                                        MessageBoxImage.Stop);
                        return; // skip loading assembly
                    }

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
                UploadState = PackageUploadHandle.State.Error;
                ErrorString = String.Format(Resources.MessageFailedToAddFile, filename);
                dynamoViewModel.Model.Logger.Log(e);
            }
        }

        /// <summary>
        /// Delegate used to submit the publish online request</summary>
        private void Submit()
        {
            MessageBoxResult response = DynamoModel.IsTestMode ? MessageBoxResult.OK : MessageBoxService.Show(Owner, Resources.PrePackagePublishMessage, Resources.PrePackagePublishTitle, MessageBoxButton.OKCancel, MessageBoxImage.Information);
            if (response == MessageBoxResult.Cancel)
            {
                return;
            }
            var contentFiles = BuildPackage();
            try
            {
                //if buildPackage() returns no files then the package
                //is empty so we should return
                if (contentFiles == null || contentFiles.Count() < 1)
                {
                    return;
                }
                // begin submission
                var pmExtension = dynamoViewModel.Model.GetPackageManagerExtension();
                var handle = pmExtension.PackageManagerClient.PublishAsync(Package, contentFiles, MarkdownFiles, IsNewVersion);

                // start upload
                Uploading = true;
                UploadHandle = handle;
            }
            catch (Exception e)
            {
                UploadState = PackageUploadHandle.State.Error;
                ErrorString = TranslatePackageManagerError(e.Message);
                dynamoViewModel.Model.Logger.Log(e);
            }
        }

        private static readonly Regex UserIsNotAMaintainerRegex =
            new Regex("^The user sending the new package version, ([^,]+), is not a maintainer of the package (.*)$");
        private static readonly Regex PackageAlreadyExistsRegex =
            new Regex("^A package with the given name and engine already exists\\.$");

        /// <summary>
        /// Inspects an error message to see if it matches a known Package Manager error message. If it does,
        /// it returns the equivalent translated resource, otherwise it returns the same message.
        /// NOTE: This function is internal for testing purposes only.
        /// </summary>
        /// <param name="message">Message to inspect</param>
        /// <returns>Translated message or same as parameter</returns>
        internal static string TranslatePackageManagerError(string message)
        {
            if (UserIsNotAMaintainerRegex.IsMatch(message))
            {
                var match = UserIsNotAMaintainerRegex.Match(message);
                return string.Format(Properties.Resources.PackageManagerUserIsNotAMaintainer, match.Groups[1], match.Groups[2]);
            }
            else if (PackageAlreadyExistsRegex.IsMatch(message))
            {
                return Properties.Resources.PackageManagerPackageAlreadyExists;
            }

            return message;
        }

        /// <summary>
        /// Delegate used to publish the element to a local folder</summary>
        private void PublishLocally()
        {
            var publishPath = GetPublishFolder();
            if (string.IsNullOrEmpty(publishPath))
                return;

            var files = BuildPackage();

            try
            {
                var unqualifiedFiles = GetAllUnqualifiedFiles();

                // if the unqualified files are bigger than 0, error message is triggered.
                // At the same time, as unqualified files existed, 
                // files returned from BuildPackage() is 0.
                // This is caused by the package file is not existed or it has already been in a package.
                // files.Count() is also checking for the exception that was caught in BuildPackage().
                // The scenario can be user trying to publish unsaved workspace.
                if (files == null || files.Count() < 1 || unqualifiedFiles.Count() > 0) 
                {
                    string filesCannotBePublished = null;
                    foreach (var file in unqualifiedFiles)
                    {
                        filesCannotBePublished = filesCannotBePublished + file + "\n";
                    }
                    string FileNotPublishMessage = string.Format(Resources.FileNotPublishMessage, filesCannotBePublished);
                    UploadState = PackageUploadHandle.State.Error;
                    MessageBoxResult response = DynamoModel.IsTestMode ? MessageBoxResult.OK : MessageBoxService.Show(FileNotPublishMessage, Resources.FileNotPublishCaption, MessageBoxButton.OK, MessageBoxImage.Error);

                    if (response == MessageBoxResult.OK)
                    {
                        this.ClearPackageContents();
                        UploadState = PackageUploadHandle.State.Ready;
                        Uploading = false;
                    }
                    return;
                }

                UploadState = PackageUploadHandle.State.Copying;
                Uploading = true;
                // begin publishing to local directory
                var remapper = new CustomNodePathRemapper(DynamoViewModel.Model.CustomNodeManager,
                    DynamoModel.IsTestMode);
                var builder = new PackageDirectoryBuilder(new MutatingFileSystem(), remapper);
                builder.BuildDirectory(Package, publishPath, files, MarkdownFiles);
                UploadState = PackageUploadHandle.State.Uploaded;

                // Once upload is successful, a display message will appear to ask
                // whether user wants to continue uploading another file or not.
                if (UploadState == PackageUploadHandle.State.Uploaded)
                {
                    // For test mode, presume the dialog input to be No and proceed.
                    MessageBoxResult dialogResult = DynamoModel.IsTestMode ? MessageBoxResult.No : MessageBoxService.Show(Resources.PublishPackageMessage, Resources.PublishPackageDialogCaption, MessageBoxButton.YesNo, MessageBoxImage.Information); ;

                    if (dialogResult == MessageBoxResult.Yes)
                    { 
                        this.ClearAllEntries();
                        Uploading = false;
                        UploadState = PackageUploadHandle.State.Ready;
                    }
                    else
                    {
                        Uploading = true;
                        System.Threading.Timer timer = null;
                        timer = new System.Threading.Timer((obj) =>
                        {
                            OnPublishSuccess();
                            timer.Dispose();
                        },
                            null, 1200, System.Threading.Timeout.Infinite);
                    }
                }
            }
            catch (Exception e)
            {
                UploadState = PackageUploadHandle.State.Error;
                ErrorString = e.Message;
                dynamoViewModel.Model.Logger.Log(e);
            }
        }

        // build the package
        private IEnumerable<string> BuildPackage()
        {
            try
            {
                var isNewPackage = Package == null;

                Package = Package ?? new Package("", Name, FullVersion, License);

                Package.VersionName = FullVersion;
                Package.Description = Description;
                Package.Group = Group;
                Package.Keywords = KeywordList;
                Package.License = string.IsNullOrEmpty(License) ? defaultLicense : License;
                Package.SiteUrl = SiteUrl;
                Package.RepositoryUrl = RepositoryUrl;
                Package.CopyrightHolder = string.IsNullOrEmpty(CopyrightHolder) ? dynamoViewModel.Model.AuthenticationManager?.Username : CopyrightHolder;
                Package.CopyrightYear = string.IsNullOrEmpty(CopyrightYear) ? DateTime.Now.Year.ToString() : copyrightYear;

                AppendPackageContents();

                Package.Dependencies.Clear();
                GetAllDependencies().ToList().ForEach(Package.Dependencies.Add);

                Package.HostDependencies = Enumerable.Empty<string>();
                Package.HostDependencies = SelectedHosts;
                foreach (string py in GetPythonDependency()) 
                {
                    if (!Package.HostDependencies.Contains(py)) 
                    {
                        Package.HostDependencies = Package.HostDependencies.Concat(new[] { py });
                    }
                }

                var files = GetAllFiles().ToList();
                var pmExtension = dynamoViewModel.Model.GetPackageManagerExtension();

                if (isNewPackage)
                {
                    pmExtension.PackageLoader.Add(Package);
                }

                Package.AddAssemblies(Assemblies);
                Package.LoadState.SetAsLoaded();
                return files;
            }
            catch (Exception e)
            {
                UploadState = PackageUploadHandle.State.Error;
                ErrorString = e.Message;
                dynamoViewModel.Model.Logger.Log(e);
                Package?.LoadState.SetAsError(ErrorString);
            }

            return new string[] {};
        }

        private bool IsDirectoryWritable(string dirPath, bool throwIfFails = false)
        {
            try
            {
                using (var fs = File.Create(
                    Path.Combine(
                        dirPath,
                        Path.GetRandomFileName()
                    ),
                    1,
                    FileOptions.DeleteOnClose)
                )
                { }
                return true;
            }
            catch
            {
                if (throwIfFails)
                    throw;
                else
                    return false;
            }
        }

        private string GetPublishFolder()
        {
            var pathManager = DynamoViewModel.Model.PathManager as PathManager;
            var setting = DynamoViewModel.PreferenceSettings;

            var args = new PackagePathEventArgs
            {
                Path = pathManager.DefaultPackagesDirectory
            };

            OnRequestShowFileDialog(this, args);

            if (args.Cancel)
                return string.Empty;

            var folder = args.Path;

            if (!IsDirectoryWritable(folder))
            {
                ErrorString = String.Format(Resources.FolderNotWritableError, folder);
                var ErrorMessage = ErrorString + "\n" + Resources.SolutionToFolderNotWritatbleError;
                Dynamo.Wpf.Utilities.MessageBoxService.Show(ErrorMessage, Resources.FileNotPublishCaption, MessageBoxButton.OK, MessageBoxImage.Warning);
                return string.Empty;
            }

            var pkgSubFolder = Path.Combine(folder, PathManager.PackagesDirectoryName);

            var index = pathManager.PackagesDirectories.IndexOf(folder);
            var subFolderIndex = pathManager.PackagesDirectories.IndexOf(pkgSubFolder);

            // This folder is not in the list of package folders.
            // Add it to the list as the default
            if (index == -1 && subFolderIndex == -1)
            {
                setting.CustomPackageFolders.Insert(0, folder);
            }
            else
            {
                // This folder has a package subfolder that is in the list.
                // Make the subfolder the default
                if (subFolderIndex != -1)
                {
                    index = subFolderIndex;
                    folder = pkgSubFolder;
                }

                var temp = setting.CustomPackageFolders[index];
                setting.CustomPackageFolders[index] = setting.CustomPackageFolders[0];
                setting.CustomPackageFolders[0] = temp;

            }

            return folder;
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
            if (dynamoViewModel == null || !dynamoViewModel.Model.AuthenticationManager.HasAuthProvider)
            {
                ErrorString = string.Format(Resources.CannotSubmitPackage,dynamoViewModel.BrandingResourceProvider.ProductName);
                return false;
            }

            return CheckPackageValidity();
        }

        /// <summary>
        /// Delegate used to publish the element locally </summary>
        private bool CanPublishLocally()
        {
            return CheckPackageValidity();
        }

        private bool CheckPackageValidity()
        {
            if (!string.IsNullOrEmpty(Name) && Name.IndexOfAny(PathHelper.SpecialAndInvalidCharacters()) >= 0)
            {
                ErrorString = Resources.PackageNameCannotContainTheseCharacters + " " + new String(PathHelper.SpecialAndInvalidCharacters());
                EnableInvalidNameWarningState(ErrorString);
                return false;
            }
            else
            {
                IsWarningEnabled = false;
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

            if (UploadState != PackageUploadHandle.State.Error) ErrorString = "";

            if (Uploading) return false;

            return true;
        }

        /// <summary>
        /// This method will enable the warning icon next to the Package Name TextBox
        /// </summary>
        /// <param name="warningMessage">Message that will be displayed when the mouse is over the warning</param>
        internal void EnableInvalidNameWarningState(string warningMessage)
        {
            CurrentWarningMessage = warningMessage;
            IsWarningEnabled = true;
        }

        private static MetadataLoadContext InitSharedPublishLoadContext()
        {
            // Retrieve the location of the assembly and the referenced assemblies used by the domain
            var runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");
            // Create PathAssemblyResolver that can resolve assemblies using the created list.
            var resolver = new PathAssemblyResolver(runtimeAssemblies);
             return new MetadataLoadContext(resolver);
        }
    }
}
