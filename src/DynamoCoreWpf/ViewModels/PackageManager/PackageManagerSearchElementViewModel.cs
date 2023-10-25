using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels;
using Greg.Responses;
using Prism.Commands;

namespace Dynamo.PackageManager.ViewModels
{
    public class PackageManagerSearchElementViewModel : BrowserItemViewModel, IEquatable<PackageManagerSearchElementViewModel>
    {
        public ICommand DownloadLatestCommand { get; set; }
        public ICommand UpvoteCommand { get; set; }

        [Obsolete("This UI command will no longer decrease package votes and will be removed in Dynamo 3.0")]
        public ICommand DownvoteCommand { get; set; }
        public ICommand VisitSiteCommand { get; set; }
        public ICommand VisitRepositoryCommand { get; set; }
        public ICommand DownloadLatestToCustomPathCommand { get; set; }

        /// <summary>
        /// The PackageManagerSearchElement Model this VM links to
        /// </summary>
        public PackageManagerSearchElement SearchElementModel { get; internal set; }
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


        /// <summary>
        /// The currently selected version of a package
        /// </summary>
        public string SelectedVersion { get; set; }

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

            this.SelectedVersion = this.SearchElementModel.LatestVersion;

            this.ToggleIsExpandedCommand = new DelegateCommand(() => this.SearchElementModel.IsExpanded = !this.SearchElementModel.IsExpanded);

            this.DownloadLatestCommand = new DelegateCommand(
                () => OnRequestDownload(SearchElementModel.Header.versions.First(x => x.version.Equals(SelectedVersion)), false),
                () => !SearchElementModel.IsDeprecated && CanInstall);
            this.DownloadLatestToCustomPathCommand = new DelegateCommand(() => OnRequestDownload(SearchElementModel.Header.versions.First(x => x.version.Equals(SelectedVersion)), true));

            this.UpvoteCommand = new DelegateCommand(SearchElementModel.Upvote, () => canLogin);

            // TODO: Remove the initialization of the UI command in Dynamo 3.0
            this.DownvoteCommand = new DelegateCommand(SearchElementModel.Downvote, () => canLogin);

            this.VisitSiteCommand =
                new DelegateCommand(() => GoToUrl(FormatUrl(SearchElementModel.SiteUrl)), () => !String.IsNullOrEmpty(SearchElementModel.SiteUrl));
            this.VisitRepositoryCommand =
                new DelegateCommand(() => GoToUrl(FormatUrl(SearchElementModel.RepositoryUrl)), () => !String.IsNullOrEmpty(SearchElementModel.RepositoryUrl));
        }

        /// <summary>
        /// PackageManagerSearchElementViewModel Constructor (only used for testing in Dynamo).
        /// </summary>
        /// <param name="element">A PackageManagerSearchElement</param>
        /// <param name="canLogin">A Boolean used for access control to certain internal packages.</param>
        public PackageManagerSearchElementViewModel(PackageManagerSearchElement element, bool canLogin) : this(element, canLogin, true)
        {}

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

        private List<String> CustomPackageFolders;
        
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
