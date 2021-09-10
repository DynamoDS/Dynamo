using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels;
using Greg.Responses;
using Microsoft.Practices.Prism.Commands;

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
        /// A function used to determine whether the user has installed this package already.
        /// Matching occurs via package name.
        /// </summary>
        private Func<string,bool> isPackageInstalled;

        public new PackageManagerSearchElement Model { get; internal set; }


        public PackageManagerSearchElementViewModel(PackageManagerSearchElement element, bool canLogin) : base(element)
        {
            this.Model = element;

            this.ToggleIsExpandedCommand = new DelegateCommand(() => this.Model.IsExpanded = !this.Model.IsExpanded );

            this.DownloadLatestCommand = new DelegateCommand(
                () => OnRequestDownload(Model.Header.versions.Last(), false),
                () => !Model.IsDeprecated && !IsInstalled);
            this.DownloadLatestToCustomPathCommand = new DelegateCommand(() => OnRequestDownload(Model.Header.versions.Last(), true));

            this.UpvoteCommand = new DelegateCommand(Model.Upvote, () => canLogin);

            // TODO: Remove the initialization of the UI command in Dynamo 3.0
            this.DownvoteCommand = new DelegateCommand(Model.Downvote, () => canLogin);

            this.VisitSiteCommand =
                new DelegateCommand(() => GoToUrl(FormatUrl(Model.SiteUrl)), () => !String.IsNullOrEmpty(Model.SiteUrl));
            this.VisitRepositoryCommand =
                new DelegateCommand(() => GoToUrl(FormatUrl(Model.RepositoryUrl)), () => !String.IsNullOrEmpty(Model.RepositoryUrl));

        }

        /// <summary>
        /// Alternative constructor which takes a Function object in order to
        /// assist communication between the PackageManagerSearchViewModel
        /// and the PackageManagerSearchElementViewModel.
        /// </summary>
        /// <param name="element">A PackageManagerSearchElement</param>
        /// <param name="canLogin">A Boolean used for access control to certain internal packages.</param>
        /// <param name="isPackageInstalledFunction">A function that we pass to this constructor which takes a string and returns a Boolean.</param>
        public PackageManagerSearchElementViewModel(PackageManagerSearchElement element, bool canLogin, Func<string,bool> isPackageInstalledFunction) : this(element, canLogin)
        {
            this.isPackageInstalled = isPackageInstalledFunction;
            this.IsInstalled = isPackageInstalled(this.Model.Name);
        }

        private bool isInstalled;

        /// <summary>
        /// A Boolean flag reporting whether or not the user already has this SearchElement's package installed.
        /// </summary>
        public bool IsInstalled
        {
            get => isInstalled;
            set
            {
                isInstalled = value;
                RaisePropertyChanged(nameof(IsInstalled));
            }
        }

        /// <summary>
        /// Checks whether the user has already installed a package with the given name.
        /// </summary>
        /// <returns>A Boolean indicating whether the user already has a package installed with the given name.</returns>
        internal bool IsPackageInstalled()
        {
            IsInstalled = isPackageInstalled(this.Model.Name);
            return IsInstalled;
        }

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
                var sInfo = new ProcessStartInfo("explorer.exe", new Uri(url).AbsoluteUri);
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
        public delegate bool CheckIfPackageIsInstalledHandler(string packageName);
        public event CheckIfPackageIsInstalledHandler CheckIfPackageInstalled;

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

            IsPackageInstalled();
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
