using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Core;
using Dynamo.Graph.Workspaces;
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
        public DelegateCommand TryInstallPackageVersionCommand { get; set; }

        public void OpenDependencyDetails(object obj)
        {
            if (!(obj is string stringValue)) return;

            PackageManagerSearchElement packageManagerSearchElement = GetPackageByName(stringValue);
            
            if (packageManagerSearchElement == null) return;

            PackageDetailsViewExtension.OpenPackageDetailsCommand.Execute(packageManagerSearchElement);

        }

        public void TryInstallPackageVersion(object obj)
        {
            if (!(obj is string versionName)) return;
            PackageManagerSearchElement packageManagerSearchElement = GetPackageByName(PackageName);
            if (packageManagerSearchElement == null) return;

            PackageInfo packageInfo = new PackageInfo(PackageName, Version.Parse(versionName));
            
            this.PackageDetailsViewExtension.packageManagerClientViewModel.DownloadAndInstallPackage(packageInfo);
        }

        /// <summary>
        /// Uses the PackageManagerClient to attempt to find a package with the same name
        /// as the provided string.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public PackageManagerSearchElement GetPackageByName(string name)
        {
            List<PackageManagerSearchElement> packageManagerSearchElements;

            // Checking if there are any cached values.
            if (PackageManagerClientViewModel.CachedPackageList != null &&
                PackageManagerClientViewModel.CachedPackageList.Count > 0)
            {
                packageManagerSearchElements = PackageManagerClientViewModel.CachedPackageList;
            }
            // If the cache is null or empty, we must call ListAll and await the results.
            else
            {
                packageManagerSearchElements = PackageManagerClientViewModel.ListAll();
            }

            return packageManagerSearchElements
                .FirstOrDefault(x => x.Name == name);
        }

        public PackageDetailsViewExtension PackageDetailsViewExtension { get; set; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="packageDetailsViewExtension"></param>
        /// <param name="packageManagerSearchElement"></param>
        public PackageDetailsViewModel
        (
            PackageDetailsViewExtension packageDetailsViewExtension,
            PackageManagerSearchElement packageManagerSearchElement
        )
        {
            PackagerLoader = packageDetailsViewExtension.PackageManagerExtension.PackageLoader;
            PackageManagerClientViewModel = packageDetailsViewExtension.packageManagerClientViewModel;

            PackageDetailItems = packageManagerSearchElement.Header.versions
                .AsEnumerable()
                .Reverse()
                .Select(x => new PackageDetailItem(packageDetailsViewExtension.PackageManagerExtension.PackageLoader, x))
                .ToList();

            PackageName = packageManagerSearchElement.Name;
            PackageAuthorName = packageManagerSearchElement.Maintainers;
            PackageDescription = packageManagerSearchElement.Description;
            DatePublished = packageManagerSearchElement.LatestVersionCreated;
            NumberDownloads = packageManagerSearchElement.Downloads;
            NumberVotes = packageManagerSearchElement.Votes;
            IsPackageDeprecated = packageManagerSearchElement.IsDeprecated;
            PackageDetailsViewExtension = packageDetailsViewExtension;
            License = packageManagerSearchElement.Header.license;

            OpenDependencyDetailsCommand = new DelegateCommand(OpenDependencyDetails);
            TryInstallPackageVersionCommand = new DelegateCommand(TryInstallPackageVersion);

            
        }
        
        public void Dispose()
        {
        }
    }
}
