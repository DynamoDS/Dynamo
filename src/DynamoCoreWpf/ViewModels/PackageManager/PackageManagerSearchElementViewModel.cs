using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CoreNodeModels.Properties;
using Dynamo.ViewModels;
using Dynamo.Wpf.Utilities;
using Dynamo.Wpf.ViewModels;
using Greg.Responses;
using Prism.Commands;

namespace Dynamo.PackageManager.ViewModels
{
    public class PackageManagerSearchElementViewModel : BrowserItemViewModel, IEquatable<PackageManagerSearchElementViewModel>
    {
        public ICommand DownloadLatestCommand { get; set; }
        public ICommand UpvoteCommand { get; set; }
        public ICommand VisitSiteCommand { get; set; }
        public ICommand VisitRepositoryCommand { get; set; }
        public ICommand DownloadLatestToCustomPathCommand { get; set; }

        /// <summary>
        /// VM IsDeprecated property
        /// </summary>
        public bool IsDeprecated { get { return this.SearchElementModel.IsDeprecated; } }
        /// <summary>
        /// VM Hosts property
        /// </summary>
        public List<string> Hosts { get { return this.SearchElementModel.Hosts; } }
        /// <summary>
        /// VM LatestVersionCreated property
        /// </summary>
        public string LatestVersionCreated { get { return this.SearchElementModel.LatestVersionCreated; } }
        /// <summary>
        /// VM Downloads property
        /// </summary>
        public int Downloads { get { return this.SearchElementModel.Downloads; } }
        /// <summary>
        /// VM Votes property
        /// </summary>
        public int Votes { get { return this.SearchElementModel.Votes; } }
        /// <summary>
        /// If the element has an upvote from the current user
        /// </summary>
        public bool HasUpvote { get { return this.SearchElementModel.HasUpvote; } }
        /// <summary>
        /// VM Package Version property
        /// </summary>
        public IEnumerable<string> PkgVersion { get { return this.SearchElementModel.PackageVersions; } }
        /// <summary>
        /// VM Maintainers property
        /// </summary>
        public string Maintainers { get { return this.SearchElementModel.Maintainers; } }
        /// <summary>
        /// VM LatestVersion property
        /// </summary>
        public string LatestVersion { get { return this.SearchElementModel.LatestVersion; } }
        /// <summary>
        /// VM Name Property
        /// </summary>
        public string Name { get { return this.SearchElementModel.Name; } }

        public PackageManagerSearchElement SearchElementModel { get; internal set; }


        /// <summary>
        /// The currently selected version of a package
        /// </summary>
        private VersionInformation selectedVersion;

        public bool? IsSelectedVersionCompatible
        {
            get { return isSelectedVersionCompatible; }
            set
            {
                if (isSelectedVersionCompatible != value)
                {
                    isSelectedVersionCompatible = value;
                    RaisePropertyChanged(nameof(IsSelectedVersionCompatible));
                }
            }
        }

        /// <summary>
        /// Keeps track of the currently selected package version in the UI
        /// </summary>
        public VersionInformation SelectedVersion   
        {
            get { return selectedVersion; }
            set
            {
                if (selectedVersion != value)
                {
                    selectedVersion = value;

                    // Update the compatibility info so the icon of the currently selected version is updated
                    IsSelectedVersionCompatible = selectedVersion.IsCompatible;
                    SearchElementModel.SelectedVersion = selectedVersion;
                }
            }
        }

        /// <summary>
        /// Alternative constructor to assist communication between the 
        /// PackageManagerSearchViewModel and the PackageManagerSearchElementViewModel.
        /// </summary>
        /// <param name="element">A PackageManagerSearchElement</param>
        /// <param name="canLogin">A Boolean used for access control to certain internal packages.</param>
        /// <param name="install">Whether a package can be installed.</param>
        /// <param name="isEnabledForInstall">Whether the package is enabled for install in the UI.</param>
        public PackageManagerSearchElementViewModel(PackageManagerSearchElement element, bool canLogin, bool install, bool isEnabledForInstall = true) 
            : base(element)
        {
            this.SearchElementModel = element;
            CanInstall = install;
            IsEnabledForInstall = isEnabledForInstall;

            // Attempts to show the latest compatible version. If no compatible, will return the latest instead.
            //this.SelectedVersion = this.SearchElementModel.LatestVersion;
            this.SelectedVersion = this.SearchElementModel.LatestCompatibleVersion;
            this.VersionInformation = this.SearchElementModel.VersionDetails;
            WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>
                .AddHandler(this.SearchElementModel, nameof(INotifyPropertyChanged.PropertyChanged), OnSearchElementModelPropertyChanged);


            this.ToggleIsExpandedCommand = new DelegateCommand(() => this.SearchElementModel.IsExpanded = !this.SearchElementModel.IsExpanded);

            this.DownloadLatestCommand = new DelegateCommand(
                () => OnRequestDownload(false),
                () => !SearchElementModel.IsDeprecated && CanInstall);
            this.DownloadLatestToCustomPathCommand = new DelegateCommand(() => OnRequestDownload(true));

            this.UpvoteCommand = new DelegateCommand(SearchElementModel.Upvote, () => canLogin);

            this.VisitSiteCommand =
                new DelegateCommand(() => GoToUrl(FormatUrl(SearchElementModel.SiteUrl)), () => !String.IsNullOrEmpty(SearchElementModel.SiteUrl));
            this.VisitRepositoryCommand =
                new DelegateCommand(() => GoToUrl(FormatUrl(SearchElementModel.RepositoryUrl)), () => !String.IsNullOrEmpty(SearchElementModel.RepositoryUrl));
        }

        private void OnSearchElementModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SearchElementModel.LatestCompatibleVersion))
            {
                this.SelectedVersion = this.SearchElementModel.LatestCompatibleVersion;
            }
            if (e.PropertyName == nameof(SearchElementModel.VersionDetails))
            {
                this.VersionInformation = this.SearchElementModel.VersionDetails;
            }
        }


        /// <summary>
        /// PackageManagerSearchElementViewModel Constructor (only used for testing in Dynamo).
        /// </summary>
        /// <param name="element">A PackageManagerSearchElement</param>
        /// <param name="canLogin">A Boolean used for access control to certain internal packages.</param>
        public PackageManagerSearchElementViewModel(PackageManagerSearchElement element, bool canLogin) : this(element, canLogin, true)
        {}

        /// <summary>
        /// A property showing if the currently logged-in user owns the package
        /// </summary>
        private bool isOwner = false;
        public bool IsOnwer
        {
            get
            {
                return isOwner;
            }

            internal set
            {   
                isOwner = value;
                RaisePropertyChanged(nameof(IsOnwer));
            }
        }

        private bool canInstall;
        /// <summary>
        /// A Boolean flag reporting whether or not the user can install this SearchElement's package.
        /// </summary>
        public bool CanInstall
        {
            get
            {
                return canInstall;
            }

            internal set
            {
                canInstall = value;
                RaisePropertyChanged(nameof(CanInstall));
            }
        }

        /// <summary>
        /// True if package is enabled for download if custom package paths are not disabled,
        /// False if custom package paths are disabled.
        /// </summary>
        public bool IsEnabledForInstall { get; private set; }

        public event EventHandler<PackagePathEventArgs> RequestShowFileDialog;
        public virtual void OnRequestShowFileDialog(object sender, PackagePathEventArgs e)
        {
            if (RequestShowFileDialog != null)
            {
                RequestShowFileDialog(sender, e);
            }
        }

        private static string FormatUrl(string url)
        {
            var lurl = url.ToLower();

            // if not preceded by http(s), process start will fail
            if (!lurl.StartsWith(@"http://") && !lurl.StartsWith(@"https://"))
            {
                return @"http://" + url;
            }

            return url;
        }

        private static void GoToUrl(string url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                var sInfo = new ProcessStartInfo("explorer.exe", new Uri(url).AbsoluteUri) { UseShellExecute = true };
                Process.Start(sInfo);
            }
        }

        public List<Tuple<PackageVersion, DelegateCommand<object>>> Versions
        {
            get
            {
                return
                    SearchElementModel.Header.versions.Select(
                        x => new Tuple<PackageVersion, DelegateCommand<object>>(
                            x, new DelegateCommand<object>((p) => OnRequestDownload(x, p.Equals("true")))
                        )).Reverse().ToList();
            }
        }

        private List<VersionInformation> versionInformation;

        public List<VersionInformation> VersionInformation
        {
            get { return versionInformation; }
            set
            {
                if (value != versionInformation)
                {
                    versionInformation = value;
                    RaisePropertyChanged(nameof(VersionInformation));
                }
            }
        }


        private List<String> CustomPackageFolders;
        private bool? isSelectedVersionCompatible;

        public delegate void PackageSearchElementDownloadHandler(
            PackageManagerSearchElement element, PackageVersion version, string downloadPath = null);
        public event PackageSearchElementDownloadHandler RequestDownload;
        
        public void OnRequestDownload(PackageVersion version, bool downloadToCustomPath)
        {
            string downloadPath = String.Empty;

            if (downloadToCustomPath)
            {
                downloadPath = GetDownloadPath();

                if (String.IsNullOrEmpty(downloadPath))
                    return;
            }

            if (RequestDownload != null)
                RequestDownload(this.SearchElementModel, version, downloadPath);
        }

        private void OnRequestDownload(bool downloadToCustomPath)
        {
            var version = this.SearchElementModel.Header.versions.First(x => x.version.Equals(SelectedVersion.Version));
            var compatible = SelectedVersion.IsCompatible;

            if (compatible == null || compatible == false)
            {
                var msg = Resources.PackageManagerIncompatibleVersionDownloadMsg;
                var result = MessageBoxService.Show(msg,
                    Resources.PackageManagerIncompatibleVersionDownloadTitle,
                    MessageBoxButton.OKCancel, MessageBoxImage.Warning);

                if (result != MessageBoxResult.OK)
                {
                    return;
                }
            }

            string downloadPath = String.Empty;

            if (downloadToCustomPath)
            {
                downloadPath = GetDownloadPath();

                if (String.IsNullOrEmpty(downloadPath))
                    return;
            }

            if (RequestDownload != null)
                RequestDownload(this.SearchElementModel, version, downloadPath);
        }

        /// <summary>
        /// Comparator of two PackageManagerSearchElementViewModel based on package Id.
        /// Override for package results union.
        /// </summary>
        /// <param name="other">The PackageManagerSearchElementViewModel to compare</param>
        /// <returns></returns>
        public bool Equals(PackageManagerSearchElementViewModel other)
        {
            if (other == null) return false;
            return this.SearchElementModel.Id == other.SearchElementModel.Id;
        }

        /// <summary>
        /// Overridden Getter for HashCode 
        /// </summary>
        /// <returns>HashCode of package</returns>
        public override int GetHashCode()
        {
            return SearchElementModel.Id.GetHashCode();
        }

        private string GetDownloadPath()
        {
            var args = new PackagePathEventArgs();

            ShowFileDialog(args);

            if (args.Cancel)
                return string.Empty;

            return args.Path;
        }

        private void ShowFileDialog(PackagePathEventArgs e)
        {
            OnRequestShowFileDialog(this, e);
        }
    }
}
