using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
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
using Prism.Commands;
using PythonNodeModels;
using Double = System.Double;
using NotificationObject = Dynamo.Core.NotificationObject;
using String = System.String;

namespace Dynamo.PackageManager
{
    public delegate void PublishSuccessHandler(PublishPackageViewModel sender);


    /// <summary>
    /// Keyword tag displaying under the keyword input text box
    /// </summary>
    public class KeywordTag : NotificationObject
    {
        /// <summary>
        /// Name of the host
        /// </summary>
        public string Name { get; set; }

        private bool _onChecked;
        /// <summary>
        /// Triggers the remove action
        /// </summary>
        public bool OnChecked
        {
            get { return _onChecked; }
            set
            {
                _onChecked = value;

                RaisePropertyChanged(nameof(OnChecked));
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param Name="name">Keyword name</param>
        public KeywordTag(string name)
        {
            Name = name;
        }
    }

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
        public class HostComboboxEntry : NotificationObject
        {
            /// <summary>
            /// Name of the host
            /// </summary>
            public string HostName { get; set; }

            /// <summary>
            /// Boolean indicates if the host entry is selected
            /// </summary>
            private bool _isSelected;
            public bool IsSelected
            {
                get { return _isSelected; }
                set
                {
                    _isSelected = value;
                    RaisePropertyChanged(nameof(IsSelected));
                }
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="hostName">Name of the host</param>
            public HostComboboxEntry(string hostName)
            {
                HostName = hostName;
                IsSelected = false;
            }

            /// <summary>
            /// Reset the state of the `IsSelected` property without raising update
            /// </summary>
            internal void ResetState()
            {
                this._isSelected = false;
            }
        }

        public Window Owner { get; set; }

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
                    // Can we try commenting out the can execute?
                    // The way async works here, when an error is returned from the response
                    // The Uploadling flag is set back to 'false' before the CanExecute code has reached it,
                    // as a result the error message is overriden.
                    //BeginInvoke(() =>
                    //{
                    //    SubmitCommand.RaiseCanExecuteChanged();   
                    //    PublishLocallyCommand.RaiseCanExecuteChanged();
                    //});
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
                if (value != null && _uploadHandle != value)
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
                    RaisePropertyChanged("CanEditName");
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
        /// UploadType property </summary>
        /// <value>
        /// The type of the upload - local or online    
        /// </value>
        private PackageUploadHandle.UploadType _uploadType = PackageUploadHandle.UploadType.Local;
        public PackageUploadHandle.UploadType UploadType
        {
            get { return _uploadType; }
            set
            {
                if (_uploadType != value)
                {
                    _uploadType = value;
                    RaisePropertyChanged("UploadType");
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
                    RaisePropertyChanged(nameof(HasChanges));
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
                    RaisePropertyChanged(nameof(HasChanges));
                    BeginInvoke(() =>
                    {
                        SubmitCommand.RaiseCanExecuteChanged();
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
                    value = value.Replace(',', ' ').ToLower();
                    var options = RegexOptions.None;
                    var regex = new Regex(@"[ ]{2,}", options);
                    value = regex.Replace(value, @" ");

                    _Keywords = value;
                    RaisePropertyChanged("Keywords");
                    RaisePropertyChanged(nameof(HasChanges));
                    KeywordList = value.Split(' ').Where(x => x.Length > 0).ToList();
                }
            }
        }

        private ObservableCollection<KeywordTag> keywordsCollection = new ObservableCollection<KeywordTag>();

        /// <summary>
        /// A collection of dynamic non-hosted filters
        /// such as New, Updated, Deprecated, Has/HasNoDependencies
        /// </summary>
        public ObservableCollection<KeywordTag> KeywordsCollection
        {
            get { return keywordsCollection; }
            set
            {
                keywordsCollection = value;
                RaisePropertyChanged(nameof(KeywordsCollection));
            }
        }

        /// <summary>
        /// KeywordList property </summary>
        /// <value>
        /// A list of keywords, usually produced by parsing Keywords</value>
        public List<string> KeywordList { get; set; } = new List<string>();

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
        private string _MinorVersion = "0";
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
                    RaisePropertyChanged(nameof(HasChanges));
                    BeginInvoke(() =>
                    {
                        SubmitCommand.RaiseCanExecuteChanged();
                    });
                }
            }
        }

        /// <summary>
        /// BuildVersion property </summary>
        /// <value>
        /// The third element of the version</value>
        private string _BuildVersion = "0";
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
                    RaisePropertyChanged(nameof(HasChanges));
                    BeginInvoke(() =>
                    {
                        SubmitCommand.RaiseCanExecuteChanged();
                    });
                }
            }
        }

        /// <summary>
        /// MajorVersion property </summary>
        /// <value>
        /// The first element of the version</value>
        private string _MajorVersion = "0";
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
                    RaisePropertyChanged(nameof(HasChanges));
                    BeginInvoke(() =>
                    {
                        SubmitCommand.RaiseCanExecuteChanged();
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
                    RaisePropertyChanged(nameof(HasChanges));
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
                RaisePropertyChanged(nameof(HasChanges));
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
                RaisePropertyChanged(nameof(HasChanges));
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
                    RaisePropertyChanged(nameof(HasChanges));
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
                    RaisePropertyChanged(nameof(HasChanges));
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
                    });

                    RaisePropertyChanged(nameof(HasChanges));
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
                    RaisePropertyChanged(nameof(SelectedHosts));
                    RaisePropertyChanged(nameof(SelectedHostsString));
                    RaisePropertyChanged(nameof(HasChanges));
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
                    RaisePropertyChanged(nameof(HasChanges));
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


        private string publishDirectory;

        /// <summary>
        /// A public property to surface the folder to publish the package locally
        /// </summary>
        public string PublishDirectory
        {
            get => publishDirectory;
            set
            {
                publishDirectory = value;
                RaisePropertyChanged(nameof(PublishDirectory));
            }
        }

        private ICollection<PackageCompatibility> compatibilityMatrix;
        public ICollection<PackageCompatibility> CompatibilityMatrix
        {
            get => compatibilityMatrix;
            set
            {
                compatibilityMatrix = value;
                RaisePropertyChanged(nameof(CompatibilityMatrix));
                BeginInvoke(() =>
                {
                    SubmitCommand.RaiseCanExecuteChanged();
                });
            }
        }

        private string releaseNotesUrl;
        public string ReleaseNotesUrl
        {
            get => releaseNotesUrl;
            set
            {
                releaseNotesUrl = value;
                RaisePropertyChanged(nameof(ReleaseNotesUrl));
            }
        }

        /// <summary>
        /// SubmitCommand property </summary>
        /// <value>
        /// A command which, when executed, submits the current package</value>
        public DelegateCommand SubmitCommand { get; private set; }

        /// <summary>
        /// CancelCommand property </summary>
        /// <value>
        /// A command which will clear the user interface and all underlaying data</value>
        public DelegateCommand CancelCommand { get; private set; }

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
        /// Sets the keywords tags based on the current KeywordList items
        /// </summary>
        public DelegateCommand SetKeywordsCommand { get; private set; }

        /// <summary>
        /// The package used for this submission
        /// </summary>
        public Package Package { get; set; }

        /// <summary>
        /// PackageContents property 
        /// </summary>
        public ObservableCollection<PackageItemRootViewModel> PackageContents { get; set; } = new ObservableCollection<PackageItemRootViewModel>();
        public ObservableCollection<PackageItemRootViewModel> PreviewPackageContents { get; set; } = new ObservableCollection<PackageItemRootViewModel>();

        private ObservableCollection<PackageItemRootViewModel> _rootContents;
        /// <summary>
        /// A dedicated container for the files located under the current selected folder
        /// </summary>
        public ObservableCollection<PackageItemRootViewModel> RootContents
        {
            get { return _rootContents; }
            set
            {
                if (_rootContents != value)
                {
                    _rootContents = value;
                    RaisePropertyChanged(nameof(RootContents));
                }
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

                RefreshPackageContents();
                UpdateDependencies();
            }
        }

        private Dictionary<string, string> CustomDyfFilepaths { get; set; } = new Dictionary<string, string>();

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
                    RaisePropertyChanged(nameof(HasChanges));
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
                RaisePropertyChanged(nameof(HasChanges));
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

        private bool _retainFolderStructureOverride;
        /// <summary>
        /// Controls if the automatic folder structure should be used, or retain existing one
        /// </summary>
        public bool RetainFolderStructureOverride
        {
            get
            {
                return _retainFolderStructureOverride;
            }
            set
            {
                if (_retainFolderStructureOverride != value)
                {
                    _retainFolderStructureOverride = value;
                    RaisePropertyChanged(nameof(RetainFolderStructureOverride));
                    PreviewPackageBuild();
                }
            }
        }
        /// <summary>
        /// The root directory of the package
        /// </summary>
        private IEnumerable<string> CurrentPackageRootDirectories { get; set; }
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
                try
                {
                    sharedMetaDataLoadContext = sharedMetaDataLoadContext == null || sharedMetaDataLoadContext.CoreAssembly == null ? PackageLoader.InitSharedPublishLoadContext() : sharedMetaDataLoadContext;
                }
                catch (ObjectDisposedException)
                {
                    // This can happen if the shared context has been disposed before.
                    // In this case, we create a new one.
                    sharedMetaDataLoadContext = PackageLoader.InitSharedPublishLoadContext();
                }
                return sharedMetaDataLoadContext;
            }
        }

        private string _rootFolder;
        /// <summary>
        /// The publish folder for the current package
        /// </summary>
        public string RootFolder
        {
            get { return _rootFolder; }
            set
            {
                _rootFolder = value;
                RaisePropertyChanged(nameof(RootFolder));
                RaisePropertyChanged(nameof(HasChanges));
            }
        }

        /// <summary>
        /// Indicates if the user has made any changes to the current publish package form
        /// </summary>
        public bool HasChanges
        {
            get { return AnyUserChanges(); }
        }

        /// <summary>
        /// Indicates if this view model is created during a FromLocalPackage routine
        /// </summary>
        private bool IsPublishFromLocalPackage = false;

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
            CancelCommand = new DelegateCommand(Cancel);
            RemoveItemCommand = new Dynamo.UI.Commands.DelegateCommand(RemoveItem);
            ToggleMoreCommand = new DelegateCommand(() => MoreExpanded = !MoreExpanded, () => true);
            SetKeywordsCommand = new DelegateCommand(SetKeywords, CanSetKeywords);
            Dependencies.CollectionChanged += DependenciesOnCollectionChanged;
            Assemblies = new List<PackageAssembly>();
            MarkdownFiles = new List<string>();
            RootContents = new ObservableCollection<PackageItemRootViewModel>();
            PropertyChanged += ThisPropertyChanged;
            RefreshPackageContents();
            RefreshDependencyNames();
        }

        private void DependenciesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => RefreshDependencyNames();

        private void RefreshDependencyNames()
        {
            if (Dependencies.Count < 1)
            {
                var textInfo = CultureInfo.CurrentUICulture.TextInfo;
                DependencyNames = textInfo.ToTitleCase(Properties.Resources.NoneString);
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
                .Concat(CustomDyfFilepaths.Select((s) => new PackageItemRootViewModel((string)s.Key, (string)s.Value)))
                .ToList()
                .ToObservableCollection();

            var items = new Dictionary<string, PackageItemRootViewModel>();

            if (!String.IsNullOrEmpty(RootFolder))
            {
                var root = new PackageItemRootViewModel(RootFolder);
                items[RootFolder] = root;
                RootFolder = String.Empty;
            }

            foreach (var item in itemsToAdd)
            {
                if (String.IsNullOrEmpty(item.DirectoryName)) continue;
                if (!items.ContainsKey(item.DirectoryName))
                {
                    // Custom nodes don't have folders, we have introduced CustomNodePreview item instead
                    if (item.DependencyType.Equals(DependencyType.CustomNode)) continue;
                    if (items.Values.Any(x => IsDuplicateFile(x, item))) continue;
                    var root = new PackageItemRootViewModel(item.DirectoryName);

                    root.ChildItems.Add(item);
                    items[item.DirectoryName] = root;
                }
                else
                {
                    items[item.DirectoryName].ChildItems.Add(item);
                }
            }

            var updatedItems = BindParentToChild(items);

            updatedItems.AddRange(itemsToAdd.Where(pa => pa.DependencyType.Equals(DependencyType.CustomNode)));

            foreach (var item in updatedItems) PackageContents.Add(item);

            PreviewPackageBuild();
        }

        private bool IsDuplicateFile(PackageItemRootViewModel item1, PackageItemRootViewModel item2)
        {
            // We know that item2 is a file
            switch (item1.DependencyType)
            {
                case DependencyType.Folder:
                    return item1.ChildItems.Any(x => IsDuplicateFile(x, item2));
                case DependencyType.File:
                case DependencyType.Assembly:
                case DependencyType.CustomNodePreview:
                    return item1.FilePath.Equals(item2.FilePath);
                case DependencyType.CustomNode:
                default:
                    return false;
            }
        }

        /// <summary>
        /// Attempts to recreate the file/folder content structure 
        /// </summary>
        /// <param name="items">A dictionary of the content items</param>
        /// <returns></returns>
        internal List<PackageItemRootViewModel> BindParentToChild(Dictionary<string, PackageItemRootViewModel> items)
        {
            foreach (var parent in items)
            {
                foreach (var child in items)
                {
                    if (parent.Value.Equals(child.Value)) continue;
                    if (IsSubPathOfDeep(parent.Value, child.Value))
                    {
                        if (child.Value.isChild) continue; // if this was picked up already, don't add it again
                        parent.Value.AddChildRecursively(child.Value);
                        child.Value.isChild = true;
                    }
                }
            }

            // Only add the folder items, they contain the files
            var updatedItems = OrganizePackageRootItems(items);
            return updatedItems;
        }

        /// <summary>
        /// Organizes package items into root items based on common paths and hierarchical structure.
        /// This includes determining root items, establishing parent-child relationships, and collecting all child items.
        /// </summary>
        /// <param name="items">A dictionary of package item keys and their corresponding PackageItemRootViewModel objects.</param>
        /// <returns>A list of PackageItemRootViewModel items representing the organized root items and their child items.</returns>
        private List<PackageItemRootViewModel> OrganizePackageRootItems(Dictionary<string, PackageItemRootViewModel> items)
        {
            var rootItems = items.Values.Where(x => !x.isChild).ToList();
            if (!rootItems.Any()) return rootItems;

            var roots = new List<PackageItemRootViewModel>();

            var commonPaths = GetCommonPaths(items.Keys.ToArray());
            if (commonPaths == null) return null;

            CurrentPackageRootDirectories = commonPaths;

            // Add a new root item for each common path found
            commonPaths.ForEach(p => roots.Add(new PackageItemRootViewModel(p)));

            // Check each root item and create any missing connections
            foreach (var item in rootItems)
            {
                bool itemAssigned = false;
                var itemDir = new DirectoryInfo(item.DirectoryName);

                foreach (var root in roots)
                {
                    var rootDir = new DirectoryInfo(root.DirectoryName);

                    if (itemDir.FullName.StartsWith(rootDir.FullName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (itemDir.Parent.FullName.Equals(rootDir.FullName))
                        {
                            root.ChildItems.Add(item);
                        }
                        else
                        {
                            root.AddChildRecursively(item);
                        }
                        itemAssigned = true;
                        break;
                    }
                }

                // If the item does not belong to any existing root, create a new root for it
                if (!itemAssigned)
                {
                    var newRoot = new PackageItemRootViewModel(item.DirectoryName);
                    newRoot.ChildItems.Add(item);
                    roots.Add(newRoot);
                }
            }

            // Collect all child items from all roots
            var allChildItems = roots.SelectMany(r => r.ChildItems).ToList();

            return allChildItems;
        }

        /// <summary>
        /// Test if path2 is subpath of path1
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        /// <returns></returns>
        internal bool IsSubPathOfDeep(PackageItemRootViewModel path1, PackageItemRootViewModel path2)
        {
            var di1 = new DirectoryInfo(path1.DirectoryName);
            var di2 = new DirectoryInfo(path2.DirectoryName);

            while (di2.Parent != null)
            {
                if (di2.Parent.FullName == di1.FullName)
                {
                    return true;
                }
                else
                {
                    if (di2.Parent.FullName.Length < di1.FullName.Length) return false;
                    di2 = di2.Parent;
                }
            }

            return false;
        }

        /// <summary>
        /// Utility method to get the common file path, this may fail for files with the same partial name.
        /// </summary>
        /// <param name="paths">A collection of file paths</param>
        /// <returns></returns>
        internal List<string> GetCommonPaths(string[] paths)
        {
            if (paths == null || paths.Length == 0)
                return new List<string>();

            // Group paths by their root (drive letter)
            var groupedPaths = paths.GroupBy(p => Path.GetPathRoot(p)).ToList();
            List<string> commonPaths = new List<string>();

            foreach (var group in groupedPaths)
            {
                var pathArray = group.ToArray();
                if (pathArray.Length == 1)
                {
                    commonPaths.Add(Path.GetDirectoryName(pathArray[0]));
                    continue;
                }

                var k = pathArray[0].Length;
                for (var i = 1; i < pathArray.Length; i++)
                {
                    k = Math.Min(k, pathArray[i].Length);
                    for (var j = 0; j < k; j++)
                    {
                        if (pathArray[i][j] != pathArray[0][j])
                        {
                            k = j;
                            break;
                        }
                    }
                }

                var commonPrefix = pathArray[0].Substring(0, k);
                var commonDir = Path.GetDirectoryName(commonPrefix);

                if (string.IsNullOrEmpty(commonDir))
                {
                    // Special case for the root directory
                    commonDir = Path.GetPathRoot(commonPrefix);
                }

                if (!string.IsNullOrEmpty(commonDir))
                {
                    commonPaths.Add(commonDir);
                }
            }

            return commonPaths.Distinct().ToList();
        }

        /// <summary>
        /// Return a list of HostComboboxEntry describing known hosts from PM.
        /// Return an empty list if PM is down.
        /// </summary>
        /// <returns>A list of HostComboboxEntry describing known hosts from PM.</returns>
        private List<HostComboboxEntry> initializeHostSelections()
        {
            var hostSelections = new List<HostComboboxEntry>();
            try
            {
                foreach (var host in dynamoViewModel.PackageManagerClientViewModel.Model?.GetKnownHosts())
                {
                    hostSelections.Add(new HostComboboxEntry(host));
                }
            }
            catch (Exception ex)
            {
                dynamoViewModel.Model.Logger.Log("Could not fetch hosts: " + ex.Message);
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
        public PublishPackageViewModel(DynamoViewModel dynamoViewModel) : this()
        {
            this.dynamoViewModel = dynamoViewModel;
            KnownHosts = initializeHostSelections();
            isWarningEnabled = false;
        }

        private void ClearAllEntries()
        {
            if (DynamoModel.IsTestMode) return;

            try
            {
                this.KnownHosts.ForEach(host => host.ResetState());
                this.KnownHosts.ForEach(host => host.IsSelected = false);
            }
            catch { Exception ex; }

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
            // Clearing the UploadHandle when using Submit currently throws - when testing? - check trheading
            try
            {
                if (this._uploadHandle != null)
                {
                    this._uploadHandle.PropertyChanged -= UploadHandleOnPropertyChanged;
                    this.UploadHandle = null;
                }
            }
            catch { Exception ex; }
            this.IsNewVersion = false;
            this.MoreExpanded = false;
            this.UploadState = PackageUploadHandle.State.Ready;
            this.AdditionalFiles = new ObservableCollection<string>();
            this.Dependencies = new ObservableCollection<PackageDependency>();
            this.Assemblies = new List<PackageAssembly>();
            this.SelectedHostsString = string.Empty;
            this.SelectedHosts = new List<String>();
            this.copyrightHolder = string.Empty;
            this.copyrightYear = string.Empty;
            this.RootFolder = string.Empty;
            this.ClearMarkdownDirectory();
            this.ClearPackageContents();
            this.KeywordsCollection?.Clear();
            this.PublishDirectory = string.Empty;
            this.CompatibilityMatrix?.Clear();
            this.ReleaseNotesUrl = string.Empty;
        }

        /// <summary>
        /// Decides if any user changes have been made in the current packge publish session
        /// </summary>
        /// <returns>true if any changes have been made, otherwise false</returns>
        internal bool AnyUserChanges()
        {
            if (!String.IsNullOrEmpty(this.Name)) return true;
            if (!String.IsNullOrEmpty(this.RepositoryUrl)) return true;
            if (!String.IsNullOrEmpty(this.SiteUrl)) return true;
            if (!String.IsNullOrEmpty(this.License)) return true;
            if (!String.IsNullOrEmpty(this.Keywords)) return true;
            if (!String.IsNullOrEmpty(this.Description)) return true;
            if (!String.IsNullOrEmpty(this.Group)) return true;
            if (!String.IsNullOrEmpty(this.MajorVersion) && !(this.MajorVersion.Equals("0"))) return true;
            if (!String.IsNullOrEmpty(this.MinorVersion) && !(this.MinorVersion.Equals("0"))) return true;
            if (!String.IsNullOrEmpty(this.BuildVersion) && !(this.BuildVersion.Equals("0"))) return true;
            if (this.AdditionalFiles.Any()) return true;
            if (this.Dependencies.Any()) return true;
            if (this.Assemblies.Any()) return true;
            if (this.SelectedHosts.Any()) return true;
            if (!String.IsNullOrEmpty(this.SelectedHostsString)) return true;
            if (!String.IsNullOrEmpty(this.copyrightHolder)) return true;
            if (!String.IsNullOrEmpty(this.copyrightYear)) return true;
            if (!String.IsNullOrEmpty(this.RootFolder)) return true;
            if (this.CompatibilityMatrix != null && this.CompatibilityMatrix.Any()) return true;
            if (!String.IsNullOrEmpty(this.ReleaseNotesUrl)) return true;

            return false;
        }

        private void ClearPackageContents()
        {
            //  this method clears the package contents in the publish package dialog
            if (this.Package != null) this.Package = null;

            // Make changes to your ObservableCollection or other UI-bound collection here.
            if (this.PackageContents.Any())
            {
                this.PackageContents.Clear();
                RaisePropertyChanged(nameof(PackageContents));
            }
            if (this.PreviewPackageContents.Any())
            {
                this.PreviewPackageContents.Clear();
                RaisePropertyChanged(nameof(PreviewPackageContents));
            }
            if (this.RootContents.Any())
            {
                this.RootContents.Clear();
                RaisePropertyChanged(nameof(RootContents));
            }
            if (this.CustomDyfFilepaths.Any())
            {
                this.CustomDyfFilepaths.Clear();
                RaisePropertyChanged(nameof(CustomDyfFilepaths));
            }

            this.CustomNodeDefinitions = new List<CustomNodeDefinition>();
        }

        private void ThisPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PackageContents")
            {
                CanSubmit();
                SubmitCommand.RaiseCanExecuteChanged();
            }
        }

        private bool CanSetKeywords()
        {
            return KeywordList.Count() > 0;
        }

        private void SetKeywords()
        {
            KeywordsCollection = KeywordList.Select(x => new KeywordTag(x)).ToObservableCollection();
            foreach (var keyword in KeywordsCollection)
            {
                keyword.PropertyChanged += Keyword_PropertyChanged;
            }
        }

        private void Keyword_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is KeywordTag keyword)) return;
            if (e.PropertyName == nameof(KeywordTag.OnChecked))
            {
                keyword.PropertyChanged -= Keyword_PropertyChanged;
                KeywordsCollection.Remove(keyword);
            }
        }

        [Obsolete("This property is deprecated and will be removed in a future version of Dynamo")]
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
                var result = PackageLoader.TryMetaDataContextLoad(string.Empty, file, SharedPublishLoadContext, out assem);

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

        /// <summary>
        /// The method is used to create a PublishPackageViewModel from a Package object.
        /// If retainFolderStructure is set to true, the folder structure of the package will be retained. Else, the default folder structure will be imposed.
        /// </summary>
        /// <param name="dynamoViewModel"></param>
        /// <param name="pkg">The package to be loaded</param>
        /// <param name="retainFolderStructure">If true, the folder structure of the package will be retained as set by the user</param>
        /// <returns></returns>
        internal static PublishPackageViewModel FromLocalPackage(DynamoViewModel dynamoViewModel, Package pkg, bool retainFolderStructure)
        {
            var defs = new List<CustomNodeDefinition>();
            var defPreviews = new Dictionary<string, string>();

            foreach (var x in pkg.LoadedCustomNodes)
            {
                CustomNodeDefinition def;
                if (dynamoViewModel.Model.CustomNodeManager.TryGetFunctionDefinition(
                    x.FunctionId,
                    DynamoModel.IsTestMode,
                    out def))
                {
                    defs.Add(def);

                    // Check if the dictionary already contains the key
                    if (defPreviews.ContainsKey(x.Name))
                    {
                        defPreviews[$"{x.Category}.{x.Name}"] = x.Path;
                    }
                    else
                    {
                        defPreviews[x.Name] = x.Path;
                    }
                }
            }

            var pkgViewModel = new PublishPackageViewModel(dynamoViewModel)
            {
                Group = pkg.Group,
                Description = pkg.Description,
                Keywords = pkg.Keywords != null ? String.Join(" ", pkg.Keywords) : string.Empty,
                CustomNodeDefinitions = defs,
                CustomDyfFilepaths = defPreviews,
                Name = pkg.Name,
                RepositoryUrl = pkg.RepositoryUrl ?? string.Empty,
                SiteUrl = pkg.SiteUrl ?? string.Empty,
                Package = pkg,
                License = pkg.License,
                SelectedHosts = pkg.HostDependencies as List<string>,
                CopyrightHolder = pkg.CopyrightHolder,
                CopyrightYear = pkg.CopyrightYear,
                IsPublishFromLocalPackage = true,
                CurrentPackageRootDirectories = new List<string> { pkg.RootDirectory },
                //default retain folder structure to true when publishing a new version from local.
                RetainFolderStructureOverride = retainFolderStructure
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
                var result = PackageLoader.TryMetaDataContextLoad(pkg.RootDirectory, file, SharedPublishLoadContext, out assem);

                switch (result)
                {
                    case AssemblyLoadingState.Success:
                        {
                            pkgViewModel.Assemblies.Add(GetPackageAssembly(nodeLibraryNames, assem));
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
                            // When retaining the folder structure, we bypass this check as users are in full control of the folder structure.
                            if (pkgViewModel.RetainFolderStructureOverride)
                            {
                                if (assem == null)
                                {
                                    pkgViewModel.AdditionalFiles.Add(file);
                                }
                                else
                                {
                                    if (!pkgViewModel.Assemblies.Any(x => x.Assembly == assem))
                                    {
                                        pkgViewModel.Assemblies.Add(GetPackageAssembly(nodeLibraryNames, assem));
                                    }
                                    else
                                    {
                                        pkgViewModel.AdditionalFiles.Add(file);
                                    }
                                }
                            }
                            else
                            {
                                assembliesLoadedTwice.Add(file);
                            }
                            break;
                        }
                }
            }

            //after dependencies are loaded refresh package contents
            pkgViewModel.RefreshPackageContents();
            pkgViewModel.UpdateDependencies();

            if (!pkgViewModel.RetainFolderStructureOverride && assembliesLoadedTwice.Any())
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

        /// <summary>
        /// Gets a Package Assembly object, if the assembly exist in the node libraries list, the IsNodeLibrary flag will be set to true.
        /// </summary>
        /// <param name="nodeLibraries">List of existing node libraries</param>
        /// <param name="assem">Assembly file</param>
        /// <returns>Package Assembly</returns>
        internal static PackageAssembly GetPackageAssembly(IEnumerable<string> nodeLibraries, Assembly assem)
        {
            var isNodeLibrary = nodeLibraries == null || nodeLibraries.Contains(assem.FullName);
            return new PackageAssembly()
            {
                IsNodeLibrary = isNodeLibrary,
                Assembly = assem
            };
        }

        public void OnPublishSuccess()
        {
            if (PublishSuccess != null)
                PublishSuccess(this);
        }

        private void UploadHandleOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(PackageUploadHandle.UploadState))
            {
                UploadState = ((PackageUploadHandle)sender).UploadState;

                if (((PackageUploadHandle)sender).UploadState == PackageUploadHandle.State.Uploaded)
                {
                    BeginInvoke(() =>
                    {
                        OnPublishSuccess();
                        ClearAllEntries();
                    });
                }

            }
            else if (propertyChangedEventArgs.PropertyName == nameof(PackageUploadHandle.ErrorString))
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
                if (!DynamoModel.IsTestMode) MessageBoxService.Show(System.Windows.Application.Current?.MainWindow, Resources.MessageUnsavedChanges0, Resources.UnsavedChangesMessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                throw new Exception(Resources.MessageUnsavedChanges0 +
                                    String.Join(", ", unsavedWorkspaceNames) +
                                    Resources.MessageUnsavedChanges1);
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
            // if we retain the folder structure, we don't want to lose assemblies in sub-folders
            // otherwise we need to delete duplicate assemblies which will end up in the same `dll` folder
            files = RetainFolderStructureOverride && !IsPublishFromLocalPackage ?
                files.Union(Assemblies.Select(x => x.LocalFilePath)) :
                files.Union(Assemblies.Select(x => x.Assembly.Location));

            return files;
        }

        private void UpdateDependencies()
        {
            Dependencies.Clear();
            GetAllDependencies()?.ToList().ForEach(Dependencies.Add);
        }

        private IEnumerable<PackageDependency> GetAllDependencies()
        {
            var pmExtension = dynamoViewModel.Model.GetPackageManagerExtension();
            if (pmExtension == null) return null;
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

            AddAllFilesAfterSelection(fDialog.FileNames.ToList());
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

            if (!IsDirectoryWritable(directoryPath))
            {
                ErrorString = String.Format(Resources.FolderNotWritableError, directoryPath);
                var ErrorMessage = ErrorString + "\n" + Resources.SolutionToFolderNotWritatbleError;
                Dynamo.Wpf.Utilities.MessageBoxService.Show(Owner, ErrorMessage, Resources.FileNotPublishCaption, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            List<string> filePaths = Directory
                .GetFiles
                (
                    directoryPath,
                    "*",
                    SearchOption.AllDirectories
                ).ToList();

            if (filePaths.Count < 1) return;

            AddAllFilesAfterSelection(filePaths);
        }

        /// <summary>
        /// Combines adding files from single file prompt and files in folders prompt
        /// </summary>
        /// <param name="filePaths"></param>
        internal void AddAllFilesAfterSelection(List<string> filePaths, string rootFolder = null)
        {
            this.RootFolder = rootFolder ?? string.Empty;

            UploadState = PackageUploadHandle.State.Ready;

            List<string> existingPackageContents = PackageItemRootViewModel.GetFiles(PackageContents.ToList())
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
                MessageBoxService.Show(Owner, errorMessage, Resources.FileNotPublishCaption, MessageBoxButton.OK, MessageBoxImage.Error);
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

            RaisePropertyChanged(nameof(MarkdownFiles));
        }

        /// <summary>
        /// Method linked to the ClearMarkdownDirectoryCommand.
        /// Sets the MarkdownFilesDirectory to an empty string.
        /// Also, cleans up the markdown files.
        /// </summary>
        private void ClearMarkdownDirectory()
        {
            MarkdownFilesDirectory = string.Empty;
            this.MarkdownFiles = new List<string>();
        }

        /// <summary>
        /// Removes an item from the package contents list.
        /// </summary>
        private void RemoveItem(object parameter)
        {
            if (!(parameter is PackageItemRootViewModel packageItemRootViewModel)) return;

            RemoveItemRecursively(packageItemRootViewModel);
            RefreshPackageContents();
            RaisePropertyChanged(nameof(PackageContents));
            RaisePropertyChanged(nameof(PreviewPackageContents));

            return;
        }

        /// <summary>
        /// The Cancel command to clear all package data and user interface
        /// </summary>
        private void Cancel()
        {
            this.ClearAllEntries();
        }

        private void RemoveItemRecursively(PackageItemRootViewModel packageItemRootViewModel)
        {
            DependencyType fileType = packageItemRootViewModel.DependencyType;

            if (fileType == DependencyType.Folder)
            {
                var nestedFiles = PackageItemRootViewModel.GetFiles(packageItemRootViewModel)
                                                          .Where(x => !x.DependencyType.Equals(DependencyType.Folder))
                                                          .ToList();

                foreach (var file in nestedFiles)
                {
                    RemoveItemRecursively(file);
                }
            }
            else
            {
                RemoveSingleItem(packageItemRootViewModel, fileType);
            }
        }

        internal void RemoveSingleItem(PackageItemRootViewModel vm, DependencyType fileType)
        {
            var fileName = vm.DisplayName;

            if (fileType.Equals(DependencyType.Assembly))
            {
                Assemblies.Remove(Assemblies
                    .FirstOrDefault(x => x.Name == fileName));
            }
            else if (fileName.ToLower().EndsWith(".dll"))
            {
                fileName = vm.FilePath;
                AdditionalFiles.Remove(AdditionalFiles
                    .FirstOrDefault(x => x == fileName));
            }
            else if (fileType.Equals(DependencyType.CustomNode) || fileType.Equals(DependencyType.CustomNodePreview))
            {
                fileName = Path.GetFileNameWithoutExtension(fileName);

                // We allow multiple .dyf files with identical node names to be loaded at once
                // We use the node Namespace as a prefix ([Namespace].[Node Name]) to allow for that
                string[] nameVariations = {
                    fileName,
                    fileName.Replace(".", ""), // Edge case where the actual Display Name as the '.' removed
                    fileName.Contains('.') ? fileName.Split('.')[1] : fileName // Edge case for the .dyf files that were added first
                };

                foreach (var variation in nameVariations)
                {
                    var customNode = CustomNodeDefinitions.FirstOrDefault(x => x.DisplayName == variation);
                    if (customNode != null)
                    {
                        CustomNodeDefinitions.Remove(customNode);
                        break; // Exit loop once found and removed
                    }
                }

                // Find and remove the corresponding key in CustomDyfFilepaths
                var keyToRemove = CustomDyfFilepaths.Keys
                                    .FirstOrDefault(k => Path.GetFileNameWithoutExtension(k) == fileName);

                if (keyToRemove != null) CustomDyfFilepaths.Remove(keyToRemove);
            }
            else
            {
                fileName = vm.FilePath;
                AdditionalFiles.Remove(AdditionalFiles
                    .FirstOrDefault(x => x == fileName));
            }
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

                foreach (var node in dynamoViewModel.Model.CustomNodeManager.LoadedWorkspaces)
                {
                    if (node.CustomNodeId == dynamoViewModel.Model.CustomNodeManager.GuidFromPath(filename))
                    {
                        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
                        node.SetInfo(fileNameWithoutExtension, null, null, filename);
                    }
                }

                if (dynamoViewModel.Model.CustomNodeManager.TryGetFunctionDefinition(nodeInfo.FunctionId, DynamoModel.IsTestMode, out CustomNodeDefinition funcDef)
                    && CustomNodeDefinitions.All(x => x.FunctionId != funcDef.FunctionId))
                {
                    CustomNodeDefinitions.Add(funcDef);
                    CustomDyfFilepaths.TryAdd(Path.GetFileName(filename), filename);
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

        // A boolean flag used to trigger only once the message prompt
        // When the user has attempted to load an existing dll from another path
        private bool duplicateAssemblyWarningTriggered = false;

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
                    if (!this.RetainFolderStructureOverride && this.Assemblies.Any(x => assemName == x.Assembly.GetName().Name) && !duplicateAssemblyWarningTriggered)
                    {
                        MessageBoxService.Show(Owner, string.Format(Resources.PackageDuplicateAssemblyWarning,
                                        dynamoViewModel.BrandingResourceProvider.ProductName),
                                        Resources.PackageDuplicateAssemblyWarningTitle,
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Stop);
                        duplicateAssemblyWarningTriggered = true;
                        return;
                    }

                    Assemblies.Add(new PackageAssembly()
                    {
                        Assembly = assem,
                        LocalFilePath = filename
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
            if (!dynamoViewModel.IsIDSDKInitialized(true, Owner)) return;
            MessageBoxResult response = DynamoModel.IsTestMode ? MessageBoxResult.OK : MessageBoxService.Show(Owner, Resources.PrePackagePublishMessage, Resources.PrePackagePublishTitle, MessageBoxButton.OKCancel, MessageBoxImage.Information);
            if (response == MessageBoxResult.Cancel)
            {
                return;
            }

            UploadType = PackageUploadHandle.UploadType.Submit;

            var contentFiles = BuildPackage();

            //if buildPackage() returns no files then the package
            //is empty so we should return
            if (!contentFiles.Any()) return;

            //do not create the updatedFiles used for retain folder route unless needed
            IEnumerable<IEnumerable<string>> updatedFiles = null;
            if (RetainFolderStructureOverride)
            {
                updatedFiles = UpdateFilesForRetainFolderStructure(contentFiles);
                if (!updatedFiles.Any()) return;
            }

            try
            {
                // begin submission
                var pmExtension = dynamoViewModel.Model.GetPackageManagerExtension();
                var handle = pmExtension.PackageManagerClient.PublishAsync(Package, RetainFolderStructureOverride ? updatedFiles : contentFiles, MarkdownFiles, IsNewVersion, CurrentPackageRootDirectories, RetainFolderStructureOverride);

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
        /// Delegate used to publish the element to a local folder
        /// </summary>
        private void PublishLocally()
        {
            var publishPath = GetPublishFolder();
            if (string.IsNullOrEmpty(publishPath))
                return;

            UploadType = PackageUploadHandle.UploadType.Local;

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
                    MessageBoxResult response = DynamoModel.IsTestMode ? MessageBoxResult.OK : MessageBoxService.Show(Owner, FileNotPublishMessage, Resources.FileNotPublishCaption, MessageBoxButton.OK, MessageBoxImage.Error);

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

                if (RetainFolderStructureOverride)
                {
                    var updatedFiles = UpdateFilesForRetainFolderStructure(files);

                    // begin publishing to local directory retaining the folder structure
                    var remapper = new CustomNodePathRemapper(DynamoViewModel.Model.CustomNodeManager,
                        DynamoModel.IsTestMode);
                    var builder = new PackageDirectoryBuilder(new MutatingFileSystem(), remapper);
                    builder.BuildRetainDirectory(Package, publishPath, CurrentPackageRootDirectories, updatedFiles, MarkdownFiles);
                    UploadState = PackageUploadHandle.State.Uploaded;
                }
                else
                {
                    // begin publishing to local directory
                    var remapper = new CustomNodePathRemapper(DynamoViewModel.Model.CustomNodeManager,
                        DynamoModel.IsTestMode);
                    var builder = new PackageDirectoryBuilder(new MutatingFileSystem(), remapper);
                    builder.BuildDirectory(Package, publishPath, files, MarkdownFiles);
                    UploadState = PackageUploadHandle.State.Uploaded;
                }

                if (UploadState == PackageUploadHandle.State.Uploaded)
                {
                    OnPublishSuccess();
                    ClearAllEntries();
                }
            }
            catch (Exception e)
            {
                UploadState = PackageUploadHandle.State.Error;
                ErrorString = e.Message;
                dynamoViewModel.Model.Logger.Log(e);
            }
        }

        /// <summary>
        /// Allocate files in lists by folder
        /// When we are calling this method, we have chosen the 'retain folder structure' path
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        internal IEnumerable<IEnumerable<string>> UpdateFilesForRetainFolderStructure(IEnumerable<string> files)
        {
            if (!files.Any() || !PreviewPackageContents.Any())
            {
                return Enumerable.Empty<IEnumerable<string>>();
            }

            if (PreviewPackageContents.Count() > 1)
            {
                // we cannot have more than 1 root folder at this stage
                return Enumerable.Empty<IEnumerable<string>>();
            }

            var updatedFileStructure = new List<IEnumerable<string>>();
            var packageFolderItem = PreviewPackageContents.First();

            foreach (var root in packageFolderItem.ChildItems)
            {
                var updatedFolder = new List<string>();
                if (root.DependencyType.Equals(DependencyType.Folder))
                {
                    var folderContents = PackageItemRootViewModel.GetFiles(root);
                    foreach (var item in folderContents)
                    {
                        if (item.DependencyType.Equals(DependencyType.Folder) || item.DependencyType.Equals(DependencyType.CustomNode)) continue;
                        if (files.Contains(item.FilePath))
                        {
                            updatedFolder.Add(item.FilePath);
                        }
                    }
                }
                else if (root.DependencyType.Equals(DependencyType.CustomNode))
                {
                    continue;
                }
                else
                {
                    updatedFolder.Add(root.FilePath);
                }

                updatedFileStructure.Add(updatedFolder);
            }

            return updatedFileStructure;
        }

        // build the package
        internal IEnumerable<string> BuildPackage()
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
                Package.Header.compatibility_matrix = CompatibilityMatrix;  // New - CompatibilityMatrix, Dynamo 3.5
                Package.Header.release_notes_url = ReleaseNotesUrl; // New - ReleaseNotesUrl, Dynamo 3.5

                AppendPackageContents();

                Package.Dependencies.Clear();
                GetAllDependencies()?.ToList().ForEach(Package.Dependencies.Add);

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
                //clean shared load context when publishing package
                PackageLoader.CleanSharedPublishLoadContext(SharedPublishLoadContext);
                return files;
            }
            catch (Exception e)
            {
                UploadState = PackageUploadHandle.State.Error;
                ErrorString = e.Message;
                dynamoViewModel.Model.Logger.Log(e);
                Package?.LoadState.SetAsError(ErrorString);
            }

            return new string[] { };
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
                Dynamo.Wpf.Utilities.MessageBoxService.Show(Owner, ErrorMessage, Resources.FileNotPublishCaption, MessageBoxButton.OK, MessageBoxImage.Warning);
                return string.Empty;
            }

            PublishDirectory = folder;

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

            RootFolder = folder;
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
                ErrorString = string.Format(Resources.CannotSubmitPackage, dynamoViewModel.BrandingResourceProvider.ProductName);
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
            if (ErrorString.StartsWith(Resources.OneAssemblyWasLoadedSeveralTimesErrorMessage)) return false;

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

            if (Name.Length <= 0 && !PackageContents.Any())
            {
                ErrorString = Resources.PackageManagerProvidePackageNameAndFiles;
                return false;
            }
            else if (Name.Length <= 0 && Double.Parse(BuildVersion) + Double.Parse(MinorVersion) + Double.Parse(MajorVersion) <= 0)
            {
                ErrorString = Resources.PackageManagerProvidePackageNameAndVersion;
                return false;
            }
            else if (!PackageContents.Any() && Double.Parse(BuildVersion) + Double.Parse(MinorVersion) + Double.Parse(MajorVersion) <= 0)
            {
                ErrorString = Resources.PackageManagerProvideVersionAndFiles;
                return false;
            }
            else if (Name.Length <= 0)
            {
                ErrorString = Resources.PackageManagerProvidePackageName;
                return false;
            }
            else if (Double.Parse(BuildVersion) + Double.Parse(MinorVersion) + Double.Parse(MajorVersion) <= 0)
            {
                ErrorString = Resources.PackageManagerProvideVersion;
                return false;
            }
            else if (!PackageContents.Any())
            {
                ErrorString = Resources.PackageManagerProvideFiles;
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

            if (!PackageContents.Any())
            {
                ErrorString = Resources.PackageNeedAtLeastOneFile;
                return false;
            }

            if (CompatibilityMatrix == null || !CompatibilityMatrix.Any())
            {
                ErrorString = Resources.PackageCompatibilityMatrixMissing;
                return false;
            }

            if (UploadState != PackageUploadHandle.State.Error) ErrorString = "";

            if (Uploading) return false;

            this.ErrorString = Resources.PackageManagerNoValidationErrors;
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

        private void PreviewPackageBuild()
        {
            if (PreviewPackageContents == null) PreviewPackageContents = new ObservableCollection<PackageItemRootViewModel>();
            else PreviewPackageContents.Clear();

            if (PackageContents?.Count == 0) return;

            var publishPath = !String.IsNullOrEmpty(RootFolder) ? RootFolder : new FileInfo("Publish Path").FullName;
            if (string.IsNullOrEmpty(publishPath))
                return;

            var files = GetAllFiles().ToList();
            if (!RetainFolderStructureOverride)
            {
                //Look for duplicate filenames to alert user
                var duplicateFiles = files.GroupBy(x => Path.GetFileName(x))
                    .Where(x => x.Count() > 1)
                    .ToList();
                if (duplicateFiles.Count > 0)
                {
                    if (!DynamoModel.IsTestMode)
                    {
                        var DialogOptions = new Dictionary<Dynamo.UI.Prompts.DynamoMessageBox.DialogFlags, bool>() { { Dynamo.UI.Prompts.DynamoMessageBox.DialogFlags.Scrollable, true } };
                        MessageBoxService.Show(System.Windows.Application.Current?.MainWindow, string.Format(Resources.DuplicateFilesInPublishWarningMessage.Replace("\\n", Environment.NewLine), duplicateFiles.Count, string.Join("\n", duplicateFiles.Select(x => x.Key).ToList())), Resources.DuplicateFilesInPublishWarningTitle, DialogOptions, MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }

            // Removes duplicate file names, retaining  only the first encounter file path for each unique file name
            files = files.GroupBy(file => Path.GetFileName(file), StringComparer.OrdinalIgnoreCase)
                         .Select(group => group.First())
                         .ToList();
            try
            {
                // Generate the Package Name, either based on the user 'Description', or the root path name, if no 'Description' yet
                var packageName = !string.IsNullOrEmpty(Name) ? Name : Path.GetFileName(publishPath);
                var rootItemPreview = RetainFolderStructureOverride ?
                    GetExistingRootItemViewModel(publishPath, packageName) :
                    GetPreBuildRootItemViewModel(publishPath, packageName, files);

                PreviewPackageContents.Add(rootItemPreview);

                RaisePropertyChanged(nameof(PreviewPackageContents));
            }
            catch (Exception e)
            {
                UploadState = PackageUploadHandle.State.Error;
                ErrorString = e.Message;
                dynamoViewModel.Model.Logger.Log(e);
            }
        }

        internal PackageItemRootViewModel GetExistingRootItemViewModel(string publishPath, string packageName)
        {
            if (!PackageContents.Any()) return null;
            if (PackageContents.Count(x => x.DependencyType.Equals(DependencyType.Folder)) == 1)
            {
                // If there is only one root item, this root item becomes the new folder
                var item = PackageContents.First(x => x.DependencyType.Equals(DependencyType.Folder));

                item = new PackageItemRootViewModel(Path.Combine(publishPath, packageName));
                item.AddChildren(PackageContents.First().ChildItems.ToList());

                return item;
            }

            // It means we have more than 1 root item, in which case we need to combine them
            var rootItem = new PackageItemRootViewModel(Path.Combine(publishPath, packageName));
            foreach (var item in PackageContents)
            {
                // Skip 'bare' custom nodes, they will be represented by their CustomNodePreview counterparts
                if (item.DependencyType.Equals(DependencyType.CustomNode)) { continue; }

                item.isChild = true;
                rootItem.AddChildren(item);
            }
            return rootItem;
        }

        internal PackageItemRootViewModel GetPreBuildRootItemViewModel(string publishPath, string packageName, List<string> files)
        {
            PackageDirectoryBuilder.PreBuildDirectory(packageName, publishPath,
                    out string rootDir, out string dyfDir, out string binDir, out string extraDir, out string docDir);

            var rootItemPreview = new PackageItemRootViewModel(rootDir);
            var dyfItemPreview = new PackageItemRootViewModel(dyfDir) { isChild = true };
            var binItemPreview = new PackageItemRootViewModel(binDir) { isChild = true };
            var extraItemPreview = new PackageItemRootViewModel(extraDir) { isChild = true };
            var docItemPreview = new PackageItemRootViewModel(docDir) { isChild = true };

            var pkg = new PackageItemRootViewModel(new FileInfo(Path.Combine(rootDir, "pkg.json")));
            rootItemPreview.AddChildRecursively(pkg);

            foreach (var file in files)
            {
                if (!File.Exists(file)) continue;
                var fileName = Path.GetFileName(file);

                if (Path.GetDirectoryName(file).EndsWith(PackageDirectoryBuilder.DocumentationDirectoryName))
                {
                    var doc = new PackageItemRootViewModel(new FileInfo(Path.Combine(docDir, fileName)));
                    docItemPreview.AddChildRecursively(doc);
                }
                else if (file.ToLower().EndsWith(".dyf"))
                {
                    var dyfPreview = new PackageItemRootViewModel(fileName, Path.Combine(dyfDir, fileName));
                    dyfItemPreview.AddChildRecursively(dyfPreview);
                }
                else if (file.ToLower().EndsWith(".dll") || PackageDirectoryBuilder.IsXmlDocFile(file, files) || PackageDirectoryBuilder.IsDynamoCustomizationFile(file, files))
                {
                    // Assemblies carry the information if they are NodeLibrary or not
                    // TODO: Propose - check if x.LocalFilePath.Equals(file) instead
                    // There are cases where the Assembly name is different than the actual file name on disc. The filepath will be a way to match those
                    if (Assemblies.Any(x => x.Name.Equals(Path.GetFileNameWithoutExtension(fileName))))
                    {
                        var packageContents = PackageItemRootViewModel.GetFiles(PackageContents.ToList());
                        var dll = packageContents.First(x => x.DependencyType.Equals(DependencyType.Assembly) && x.DisplayName.Equals(Path.GetFileNameWithoutExtension(fileName)));
                        if (dll != null)
                            binItemPreview.AddChildren(dll);
                    }
                    else
                    {
                        var dll = new PackageItemRootViewModel(new FileInfo(Path.Combine(binDir, fileName)));
                        binItemPreview.AddChildRecursively(dll);
                    }
                }
                else
                {
                    var extra = new PackageItemRootViewModel(new FileInfo(Path.Combine(extraDir, fileName)));
                    extraItemPreview.AddChildRecursively(extra);
                }
            }

            foreach (var docFile in MarkdownFiles)
            {
                var fileName = Path.GetFileName(docFile);
                var doc = new PackageItemRootViewModel(new FileInfo(Path.Combine(docDir, fileName)));
                docItemPreview.AddChildRecursively(doc);
            }

            rootItemPreview.AddChildRecursively(dyfItemPreview);
            rootItemPreview.AddChildRecursively(binItemPreview);
            rootItemPreview.AddChildRecursively(extraItemPreview);
            rootItemPreview.AddChildRecursively(docItemPreview);

            return rootItemPreview;
        }
    }
}
