using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Core;
using Dynamo.PackageManager;
using Dynamo.PackageManager.ViewModels;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;

namespace Dynamo.PackageDetails
{
    public class PackageDetailsViewModel : NotificationObject, IDisposable
    {
        #region Private Fields

        private List<PackageDetailItem> packageDetailItems;
        private string license;

        #endregion

        #region Public Properties

        public PackageLoader PackagerLoader { get; set; }
        public PackageManagerClientViewModel PackageManagerClientViewModel { get; set; }

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

        #endregion

        #region Commands

        public DelegateCommand OpenDependencyDetailsCommand { get; set; }
        public void OpenDependencyDetails(object obj)
        {
            if (!(obj is string stringValue)) return;

            List<PackageManagerSearchElement> packageManagerSearchElements =
                this.PackageManagerClientViewModel.ListAll();

            PackageManagerSearchElement packageManagerSearchElement = packageManagerSearchElements
                .FirstOrDefault(x => x.Name == stringValue);

            if (packageManagerSearchElement == null) return;


        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="packageLoader"></param>
        /// <param name="packageManagerClientViewModel"></param>
        /// <param name="packageManagerSearchElementViewModel"></param>
        public PackageDetailsViewModel
        (
            PackageLoader packageLoader,
            PackageManagerClientViewModel packageManagerClientViewModel,
            PackageManagerSearchElementViewModel packageManagerSearchElementViewModel
        )
        {
            this.PackagerLoader = packageLoader;
            this.PackageManagerClientViewModel = packageManagerClientViewModel;

            this.PackageDetailItems = packageManagerSearchElementViewModel.Model.Header.versions
                .AsEnumerable()
                .Reverse()
                .Select(x => new PackageDetailItem(packageLoader, x))
                .ToList();

            this.PackageName = packageManagerSearchElementViewModel.Model.Name;
            this.PackageAuthorName = packageManagerSearchElementViewModel.Model.Maintainers;
            this.PackageDescription = packageManagerSearchElementViewModel.Model.Description;
            this.DatePublished = packageManagerSearchElementViewModel.Model.LatestVersionCreated;
            this.NumberDownloads = packageManagerSearchElementViewModel.Model.Downloads;
            this.NumberVotes = packageManagerSearchElementViewModel.Model.Votes;
            this.IsPackageDeprecated = packageManagerSearchElementViewModel.Model.IsDeprecated;

            this.OpenDependencyDetailsCommand = new DelegateCommand(OpenDependencyDetails);

            License = packageManagerSearchElementViewModel.Model.Header.license;
        }
        
        public void Dispose()
        {
        }
    }
}
