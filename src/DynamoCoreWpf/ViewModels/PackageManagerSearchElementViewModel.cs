using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

using Dynamo.Wpf.ViewModels;

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

        public new PackageManagerSearchElement Model { get; private set; }

        public PackageManagerSearchElementViewModel(PackageManagerSearchElement element) : base(element)
        {
            this.Model = element;

            this.ToggleIsExpandedCommand = new DelegateCommand(() => this.Model.IsExpanded = !this.Model.IsExpanded );
            this.DownloadLatestCommand = new DelegateCommand(() => OnRequestDownload(Model.Header.versions.Last()));
            this.UpvoteCommand = new DelegateCommand((Action)Model.Upvote, Model.CanUpvote);
            this.DownvoteCommand = new DelegateCommand((Action)Model.Downvote, Model.CanDownvote);
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

        public List<Tuple<PackageVersion, DelegateCommand>> Versions
        {
            get
            {
                return
                    Model.Header.versions.Select(
                        x => new Tuple<PackageVersion, DelegateCommand>(x, new DelegateCommand(() => OnRequestDownload(x)))).ToList();
            }
        }

        public delegate void PackageSearchElementDownloadHandler(PackageManagerSearchElement element, PackageVersion version);
        public event PackageSearchElementDownloadHandler RequestDownload;
        public void OnRequestDownload(PackageVersion version)
        {
            if (RequestDownload != null)
            {
                RequestDownload(this.Model, version);
            }
        }
    }
}
