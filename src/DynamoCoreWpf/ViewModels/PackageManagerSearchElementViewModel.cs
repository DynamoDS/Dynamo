using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.Wpf.ViewModels;

using Greg.Responses;

using Microsoft.Practices.Prism.Commands;

namespace Dynamo.PackageManager.ViewModels
{
    public class PackageManagerSearchElementViewModel : BrowserItemViewModel
    {
        public ICommand DownloadLatest { get; set; }
        public ICommand UpvoteCommand { get; set; }
        public ICommand DownvoteCommand { get; set; }

        public PackageManagerSearchElement Model { get; private set; }

        public PackageManagerSearchElementViewModel(PackageManagerSearchElement element) : base(element)
        {
            this.Model = element;

            this.DownloadLatest = new DelegateCommand(() => OnRequestDownload(Model.Header.versions.Last()));
            this.UpvoteCommand = new DelegateCommand(element.Upvote);
            this.DownvoteCommand = new DelegateCommand(element.Downvote);
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
