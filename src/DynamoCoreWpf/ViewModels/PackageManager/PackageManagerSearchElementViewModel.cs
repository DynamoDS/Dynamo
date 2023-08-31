using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels;
using Greg.Responses;
#if NETFRAMEWORK
using Microsoft.Practices.Prism.Commands;
#else
using Prism.Commands;
#endif

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

        public new PackageManagerSearchElement Model { get; internal set; }

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
            this.Model = element;
            CanInstall = install;
            IsEnabledForInstall = isEnabledForInstall;

            this.ToggleIsExpandedCommand = new DelegateCommand(() => this.Model.IsExpanded = !this.Model.IsExpanded);

            this.DownloadLatestCommand = new DelegateCommand(
                () => OnRequestDownload(Model.Header.versions.First(x => x.version.Equals(Model.SelectedVersion)), false),
                () => !Model.IsDeprecated && CanInstall);
            this.DownloadLatestToCustomPathCommand = new DelegateCommand(() => OnRequestDownload(Model.Header.versions.First(x => x.version.Equals(Model.SelectedVersion)), true));

            this.UpvoteCommand = new DelegateCommand(Model.Upvote, () => canLogin);

            // TODO: Remove the initialization of the UI command in Dynamo 3.0
            this.DownvoteCommand = new DelegateCommand(Model.Downvote, () => canLogin);

            this.VisitSiteCommand =
                new DelegateCommand(() => GoToUrl(FormatUrl(Model.SiteUrl)), () => !String.IsNullOrEmpty(Model.SiteUrl));
            this.VisitRepositoryCommand =
                new DelegateCommand(() => GoToUrl(FormatUrl(Model.RepositoryUrl)), () => !String.IsNullOrEmpty(Model.RepositoryUrl));
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
                    Model.Header.versions.Select(
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
                RequestDownload(this.Model, version, downloadPath);
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
            return this.Model.Id == other.Model.Id;
        }

        /// <summary>
        /// Overridden Getter for HashCode 
        /// </summary>
        /// <returns>HashCode of package</returns>
        public override int GetHashCode()
        {
            return Model.Id.GetHashCode();
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
