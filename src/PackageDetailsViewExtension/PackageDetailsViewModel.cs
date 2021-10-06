using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Core;
using Dynamo.PackageManager.ViewModels;

namespace Dynamo.PackageDetails
{
    public class PackageDetailsViewModel : NotificationObject
    {
        private List<PackageDetailItem> packageDetailItems;
        private string license;

        /// <summary>
        /// Stores a  collection of the PackageDetailItems.
        /// </summary>
        public List<PackageDetailItem> PackageDetailItems
        {
            get => packageDetailItems;
            set
            {
                packageDetailItems = value;
                RaisePropertyChanged(nameof(PackageDetailItems));
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

        /// <summary>
        /// The name of the package whose details are being inspected.
        /// </summary>
        public string PackageName { get; }

        /// <summary>
        /// The author of the package whose details are being inspected.
        /// </summary>
        public string PackageAuthorName { get; }

        /// <summary>
        /// A longform description of the package whose details are being inspected.
        /// </summary>
        public string PackageDescription { get; }

        /// <summary>
        /// How many votes this package has received.
        /// </summary>
        public int NumberVotes { get; }

        /// <summary>
        /// How many votes times this package has been downloaded.
        /// </summary>
        public int NumberDownloads { get; }

        /// <summary>
        /// The date on which this package was published.
        /// </summary>
        public string DatePublished { get; }
        
        /// <summary>
        /// Whether this package has been deprecated.
        /// </summary>
        public bool IsPackageDeprecated { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="packageDetailsViewExtension"></param>
        /// <param name="packageManagerSearchElementViewModel"></param>
        public PackageDetailsViewModel
        (
            PackageDetailsViewExtension packageDetailsViewExtension,
            PackageManagerSearchElementViewModel packageManagerSearchElementViewModel
        )
        {
            this.PackageDetailItems = packageManagerSearchElementViewModel.Model.Header.versions
                .AsEnumerable()
                .Reverse()
                .Select(x => new PackageDetailItem(packageDetailsViewExtension, x))
                .ToList();

            this.PackageName = packageManagerSearchElementViewModel.Model.Name;
            this.PackageAuthorName = packageManagerSearchElementViewModel.Model.Maintainers;
            this.PackageDescription = packageManagerSearchElementViewModel.Model.Description;
            this.DatePublished = packageManagerSearchElementViewModel.Model.LatestVersionCreated;
            this.NumberDownloads = packageManagerSearchElementViewModel.Model.Downloads;
            this.NumberVotes = packageManagerSearchElementViewModel.Model.Votes;
            this.IsPackageDeprecated = packageManagerSearchElementViewModel.Model.IsDeprecated;

            License = packageManagerSearchElementViewModel.Model.Header.license;
        }
    }
}
