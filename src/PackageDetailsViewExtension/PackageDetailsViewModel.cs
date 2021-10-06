using System.Collections.Generic;
using System.Linq;
using Dynamo.Core;
using Dynamo.PackageManager;
using Dynamo.PackageManager.ViewModels;
using Greg.Responses;

namespace Dynamo.PackageDetails
{
    public class PackageDetailsViewModel : NotificationObject
    {
        public PackageManagerSearchElementViewModel PackageManagerSearchElementViewModel { get; }

        private List<PackageVersion> packageVersions;
        private string license;

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

        /// <summary>
        /// This package's license type (e.g. MIT)
        /// </summary>
        public string License
        {
            get => license;
            set
            {
                license = value ?? "MIT";
                RaisePropertyChanged(nameof(License));
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

            License = PackageManagerSearchElementViewModel.Model.Header.license;
        }
    }
}
