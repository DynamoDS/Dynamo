using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Greg.Responses;

using Microsoft.Practices.Prism.Commands;

namespace Dynamo.PackageManager.ViewModels
{
    public class PackageManagerSearchElementViewModel
    {
        public ICommand DownloadLatest { get; set; }
        public ICommand UpvoteCommand { get; set; }
        public ICommand DownvoteCommand { get; set; }

        public PackageManagerSearchElement Model { get; private set; }

        public PackageManagerSearchElementViewModel(PackageManagerSearchElement element)
        {
            this.Model = element;

            this.DownloadLatest = new DelegateCommand((Action)element.Execute);
            this.UpvoteCommand = new DelegateCommand((Action)element.Upvote);
            this.DownvoteCommand = new DelegateCommand((Action)element.Downvote);
        }

        public List<Tuple<PackageVersion, DelegateCommand>> Versions
        {
            get
            {
                return
                    Model.Header.versions.Select(
                        x => new Tuple<PackageVersion, DelegateCommand>(x, new DelegateCommand(() =>
                        {
                            this.Model.VersionNumberToDownload = x;
                            this.Model.Execute();
                        }, () => true))).ToList();
            }

        }

    }



}
