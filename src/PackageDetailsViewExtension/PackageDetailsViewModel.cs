using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Core;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;
using Dynamo.PackageManager;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;

namespace Dynamo.PackageDetails
{
    public class PackageDetailsViewModel : NotificationObject
    {
        #region Private Fields

        /// <summary>
        /// A reference to the ViewExtension.
        /// </summary>
        private PackageDetailsViewExtension PackageDetailsViewExtension { get; set; }
        private readonly PackageManagerClientViewModel packageManagerClientViewModel;
        private List<PackageDetailItem> packageDetailItems;
        private string license;
        private IPreferences Preferences
        {
            get {
                if (packageManagerClientViewModel == null && Models.DynamoModel.IsTestMode) return null;
                return packageManagerClientViewModel.DynamoViewModel.PreferenceSettings;
            }
        }

        private int numberVotes;
        private bool hasVoted;
        #endregion

        #region Public Properties

        /// <summary>
        /// Stores a collection of PackageDetailItems.
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
                license = string.IsNullOrEmpty(value) ? "MIT" : value;
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
        public int NumberVotes
        {
            get => numberVotes;
            set
            {
                if (value != numberVotes) { numberVotes = value; }
                RaisePropertyChanged(nameof(NumberVotes));
            }
        }

        /// <summary>
        /// How many times this package has been downloaded.
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
        /// The site URL of the package whose details are being inspected.
        /// </summary>
        public string PackageSiteURL { get; }

        /// <summary>
        /// The repository URL of the package whose details are being inspected.
        /// </summary>
        public string PackageRepositoryURL { get; }

        /// <summary>
        /// The keywords associated with the package whose details are being inspected.
        /// </summary>
        public string Keywords { get; }

        /// <summary>
        /// The group associated with the package whose details are being inspected.
        /// </summary>
        public string Group { get; }

        /// <summary>
        /// Returns, true if custom package paths are not disabled,
        /// False if custom package paths are disabled.
        /// </summary>
        public bool IsEnabledForInstall { get; private set; }

        /// <summary>
        /// Shows if the current user has voted for this package
        /// </summary>
        public bool HasVoted
        {
            get => hasVoted;
            set
            {
                if (value != hasVoted) { hasVoted = value; }
                RaisePropertyChanged(nameof(HasVoted));
            }
        }

        #endregion

        #region Commands

        public DelegateCommand OpenDependencyDetailsCommand { get; set; }
        public DelegateCommand TryInstallPackageVersionCommand { get; set; }
        public DelegateCommand UpvoteCommand { get; set; }

        /// <summary>
        /// Retrieves a package by name and display its details in the PackageDetailsView.
        /// </summary>
        /// <param name="obj"></param>
        internal void OpenDependencyDetails(object obj)
        {
            if (!(obj is string stringValue)) return;
            
            PackageManagerSearchElement packageManagerSearchElement = GetPackageByName(stringValue);
            
            if (packageManagerSearchElement == null) return;
            PackageDetailsViewExtension.OpenPackageDetails(packageManagerSearchElement);
        }

        /// <summary>
        /// Attempts to retrieve a package by name and install it locally.
        /// </summary>
        /// <param name="obj"></param>
        internal void TryInstallPackageVersion(object obj)
        {
            if (!(obj is string versionName)) return;
            PackageManagerSearchElement packageManagerSearchElement = GetPackageByName(PackageName);
            if (packageManagerSearchElement == null) return;
            var compatible = packageManagerSearchElement.VersionInfos?.First(x => x.Version.Equals(versionName))?.IsCompatible;

            PackageInfo packageInfo = new PackageInfo(PackageName, Version.Parse(versionName));
            
            this.PackageDetailsViewExtension.PackageManagerClientViewModel.DownloadAndInstallPackage(packageInfo);
        }

        /// <summary>
        /// Upvotes a packge
        /// </summary>
        /// <param name="obj"></param>
        private void UpvotePackage(object obj)
        {
            PackageManagerSearchElement packageManagerSearchElement = GetPackageByName(this.PackageName);
            packageManagerSearchElement?.Upvote();

            HasVoted = true;
            NumberVotes = ++NumberVotes;
        }


        private bool CanUpvotePackage(object obj)
        {
            // If any version of the package has been installed locally, 
            // and if the user has not voted for this package before
            // then we allow voting
            return PackageDetailItems.Any(x => !x.CanInstall) && !this.HasVoted;
        }


        /// <summary>
        /// After installing a package version, this method sets the CanInstall flag to false for
        /// that specific version only. A dynamic check would be better, but the package has not been
        /// added to PackageLoader.LocalPackages by the end of the DownloadAndInstallPackage method.
        /// </summary>
        private void RefreshPackageDetailItemInstalledStatus(string versionName)
        {
            foreach (PackageDetailItem packageDetailItem in PackageDetailItems)
            {
                if (packageDetailItem.PackageVersion.version != versionName) continue;
                packageDetailItem.CanInstall = false;
                RaisePropertyChanged(nameof(packageDetailItem.CanInstall));

                // Also update the 'CanExecute' condition for 'UpvoteCommand' which relies on a package being installed
                UpvoteCommand.RaiseCanExecuteChanged();  
            }
        }

        /// <summary>
        /// Uses the PackageManagerClient to attempt to find a package with the same name
        /// as the provided string.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal virtual PackageManagerSearchElement GetPackageByName(string name)
        {
            List<PackageManagerSearchElement> packageManagerSearchElements;

            // Checking if there are any cached values.
            if (packageManagerClientViewModel.CachedPackageList != null &&
                packageManagerClientViewModel.CachedPackageList.Count > 0)
            {
                packageManagerSearchElements = packageManagerClientViewModel.CachedPackageList;
            }
            // If the cache is null or empty, we must call ListAll and wait for the results.
            else
            {
                packageManagerSearchElements = packageManagerClientViewModel.ListAll();
            }

            return packageManagerSearchElements
                .FirstOrDefault(x => x.Name == name);
        }
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
            PackageLoader packageLoader = packageDetailsViewExtension.PackageManagerExtension.PackageLoader;
            packageManagerClientViewModel = packageDetailsViewExtension.PackageManagerClientViewModel;
            IsPackageDeprecated = packageManagerSearchElement.IsDeprecated;
            IsEnabledForInstall = Preferences == null || !(Preferences as IDisablePackageLoadingPreferences).DisableCustomPackageLocations;

            // Reversing the versions, so they appear newest-first.
            PackageDetailItems = packageManagerSearchElement.Header.versions
                .AsEnumerable()
                .Reverse()
                .Select(x => new PackageDetailItem
                (
                    packageManagerSearchElement.VersionInfos,
                    packageManagerSearchElement.Name,
                    x,
                    DetectWhetherCanInstall(packageLoader, x.version, packageManagerSearchElement.Name),
                    IsEnabledForInstall && !IsPackageDeprecated
                )).ToList();

            PackageName = packageManagerSearchElement.Name;
            PackageAuthorName = packageManagerSearchElement.Maintainers;
            PackageDescription = packageManagerSearchElement.Description;
            DatePublished = packageManagerSearchElement.LatestVersionCreated;
            NumberDownloads = packageManagerSearchElement.Downloads;
            NumberVotes = packageManagerSearchElement.Votes;
            PackageDetailsViewExtension = packageDetailsViewExtension;
            License = packageManagerSearchElement.Header.license;
            PackageSiteURL = packageManagerSearchElement.SiteUrl;
            PackageRepositoryURL = packageManagerSearchElement.RepositoryUrl;
            HasVoted = packageManagerSearchElement.HasUpvote;
            Keywords = packageManagerSearchElement.Keywords;
            Group = packageManagerSearchElement.Header.group;

            if (!Models.DynamoModel.IsTestMode)
            {
                packageLoader.PackageAdded += PackageLoaderOnPackageAdded;
            }

            OpenDependencyDetailsCommand = new DelegateCommand(OpenDependencyDetails);
            TryInstallPackageVersionCommand = new DelegateCommand(TryInstallPackageVersion);
            UpvoteCommand = new DelegateCommand(UpvotePackage, CanUpvotePackage);
        }

        private void PackageLoaderOnPackageAdded(Package obj)
        {
            DetectWhetherCanInstall
            (
                PackageDetailsViewExtension.PackageManagerExtension.PackageLoader,
                obj.Header.version,
                obj.Header.name
            );

            RefreshPackageDetailItemInstalledStatus(obj.Header.version);
        }

        /// <summary>
        /// Detects whether the user can install a particular package at a particular version.
        /// Checks whether this is already installed using the PackageLoader.
        /// </summary>
        private bool DetectWhetherCanInstall(PackageLoader packageLoader, string packageVersion, string packageName)
        {
            // In order for CanInstall to be false, both the name and installed package version must match
            // what is found in the PackageLoader.LocalPackages which are designated as 'Loaded'.

            if (packageLoader == null) return false;

            List<Package> sameNamePackages = packageLoader
                .LocalPackages
                .Where(x => x.Name == packageName)
                .ToList();

            if (sameNamePackages.Count < 1) return true;

            return !sameNamePackages
                .Select(x => x.VersionName)
                .Contains(packageVersion);
        }

        /// <summary>
        /// Called when the extension is disposed, unsubscribes from events.
        /// </summary>
        internal void Dispose()
        {
            if (PackageDetailsViewExtension.PackageManagerExtension.PackageLoader == null) return;
            PackageDetailsViewExtension.PackageManagerExtension.PackageLoader.PackageAdded -= PackageLoaderOnPackageAdded;
        }
    }
}
