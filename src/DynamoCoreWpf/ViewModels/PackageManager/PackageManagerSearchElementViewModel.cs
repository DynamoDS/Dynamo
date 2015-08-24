using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

using Dynamo.Wpf.ViewModels;
using Dynamo.ViewModels;

using Greg.Responses;

using Microsoft.Practices.Prism.Commands;

namespace Dynamo.PackageManager.ViewModels
{
    public class PackageManagerSearchElementViewModel : BrowserItemViewModel
    {
        public ICommand DownloadLatestCommand { get; set; }
        public ICommand UpvoteCommand { get; set; }
        public ICommand DownvoteCommand { get; set; }
        public ICommand VisitSiteCommand { get; set; }
        public ICommand VisitRepositoryCommand { get; set; }
        public DelegateCommand DownloadLatestToCustomPathCommand { get; set; }

        public PackageManagerSearchViewModel PackageManagerSearchViewModel { get; private set; }
        public new PackageManagerSearchElement Model { get; internal set; }

        public PackageManagerSearchElementViewModel(
            PackageManagerSearchViewModel searchViewModel, PackageManagerSearchElement element, bool canLogin) : base(element)
        {
            this.PackageManagerSearchViewModel = searchViewModel;
            this.Model = element;

            this.ToggleIsExpandedCommand = new DelegateCommand(() => this.Model.IsExpanded = !this.Model.IsExpanded );

            this.DownloadLatestCommand = new DelegateCommand(() => OnRequestDownload(Model.Header.versions.Last(), false));
            this.DownloadLatestToCustomPathCommand = new DelegateCommand(() => OnRequestDownload(Model.Header.versions.Last(), true));

            this.UpvoteCommand = new DelegateCommand(Model.Upvote, () => canLogin);
            this.DownvoteCommand = new DelegateCommand(Model.Downvote, () => canLogin);

            this.VisitSiteCommand =
                new DelegateCommand(() => GoToUrl(FormatUrl(Model.SiteUrl)), () => !String.IsNullOrEmpty(Model.SiteUrl));
            this.VisitRepositoryCommand =
                new DelegateCommand(() => GoToUrl(FormatUrl(Model.RepositoryUrl)), () => !String.IsNullOrEmpty(Model.RepositoryUrl));
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

        public void OnRequestDownload(PackageVersion version, bool downloadToCustomPath)
        {
            string downloadPath = null;

            if (downloadToCustomPath)
            {
                downloadPath = GetDownloadPath();

                if (downloadPath == null)
                    return;
            }

            if (RequestDownload != null)
                RequestDownload(this.Model, version, downloadPath);

            DownloadLatestToCustomPathCommand.RaiseCanExecuteChanged();
        }

        private string GetDownloadPath()
        {
            List<String> CustomPackageFolders = this.PackageManagerSearchViewModel.PackageManagerClientViewModel
                .DynamoViewModel.Model.PreferenceSettings.CustomPackageFolders;

            var args = new PackagePathEventArgs();

            ShowFileDialog(args);

            if (!CustomPackageFolders.Contains(args.Path))
                CustomPackageFolders.Insert(CustomPackageFolders.Count, args.Path);

            if (args.Cancel)
                return null;

            return args.Path;
        }

        private void ShowFileDialog(PackagePathEventArgs e)
        {
            PackageManagerSearchViewModel.OnRequestShowFileDialog(this, e);
        }
    }
}
