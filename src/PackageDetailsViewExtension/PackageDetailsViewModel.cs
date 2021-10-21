﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Core;
using Dynamo.Graph.Workspaces;
using Dynamo.PackageManager;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;

namespace Dynamo.PackageDetails
{
    public class PackageDetailsViewModel : NotificationObject
    {
        #region Private Fields

        private readonly PackageManagerClientViewModel packageManagerClientViewModel;
        private List<PackageDetailItem> packageDetailItems;
        private string license;

        #endregion

        #region Public Properties
        
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
        /// A reference to the ViewExtension.
        /// </summary>
        private PackageDetailsViewExtension PackageDetailsViewExtension { get; set; }

        #endregion

        #region Commands

        public DelegateCommand OpenDependencyDetailsCommand { get; set; }
        public DelegateCommand TryInstallPackageVersionCommand { get; set; }

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

            PackageInfo packageInfo = new PackageInfo(PackageName, Version.Parse(versionName));
            
            this.PackageDetailsViewExtension.PackageManagerClientViewModel.DownloadAndInstallPackage(packageInfo);
            RefreshPackageDetailItemInstalledStatus(versionName);
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
            }
        }

        /// <summary>
        /// Uses the PackageManagerClient to attempt to find a package with the same name
        /// as the provided string.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal PackageManagerSearchElement GetPackageByName(string name)
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

            // Reversing the versions, so they appear newest-first.
            PackageDetailItems = packageManagerSearchElement.Header.versions
                .AsEnumerable()
                .Reverse()
                .Select(x => new PackageDetailItem
                (
                    packageManagerSearchElement.Name,
                    x,
                    DetectWhetherCanInstall(packageLoader, x.version)
                )).ToList();

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

        /// <summary>
        /// Detects whether the user can install a particular package at a particular version.
        /// Checks whether this is already installed using the PackageLoader.
        /// </summary>
        private bool DetectWhetherCanInstall(PackageLoader packageLoader, string packageVersion)
        {
            // In order for CanInstall to be false, both the name and installed package version must match
            // what is found in the PackageLoader.LocalPackages which are designated as 'Loaded'.

            if (packageLoader == null) return false;

            List<Package> sameNamePackages = packageLoader
                .LocalPackages
                .Where(x => x.Name == PackageName)
                .Where(x => x.LoadState.State == PackageLoadState.StateTypes.Loaded)
                .ToList();

            if (sameNamePackages.Count < 1) return true;

            return !sameNamePackages
                .Select(x => x.VersionName)
                .Contains(packageVersion);
        }
    }
}
