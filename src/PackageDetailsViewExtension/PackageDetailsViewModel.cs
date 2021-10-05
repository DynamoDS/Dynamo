using System.Collections.Generic;
using System.Linq;
using Dynamo.Core;
using Dynamo.PackageManager;
using Dynamo.PackageManager.ViewModels;
using Dynamo.ViewModels;
using Greg.Responses;

namespace Dynamo.PackageDetails
{
    public class PackageDetailsViewModel : NotificationObject
    {
        public PackageManagerSearchElementViewModel PackageManagerSearchElementViewModel { get; }

        private List<PackageVersion> packageVersions;

        /// <summary>
        /// Stores a reversed collection of the PackageVersions, since the DataGrid refuses to sort its collection in XAML.
        /// </summary>
        public List<PackageVersion> PackageVersions
        {
            get => packageVersions;
            set
            {
                packageVersions = value;
                RaisePropertyChanged(nameof(PackageVersions));
            }
        }

        public PackageLoader PackageLoader { get; set; }

        /// <summary>
        /// The installed versions of this package, if any.
        /// </summary>
        public List<string> InstalledPackageVersions { get; set; } = new List<string>();
    
        public PackageDetailsViewModel(PackageManagerSearchElementViewModel packageManagerSearchElementViewModel)
        {
            PackageManagerSearchElementViewModel = packageManagerSearchElementViewModel;
            PackageVersions = PackageManagerSearchElementViewModel.Model.Header.versions
                .AsEnumerable()
                .Reverse()
                .ToList();

            
            
        }
    }
}
