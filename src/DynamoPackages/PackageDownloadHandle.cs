using System;
using System.IO;
using Dynamo.Core;
using Dynamo.Models;

using Greg.Responses;

namespace Dynamo.PackageManager
{
    /// <summary>
    /// View model for the installation of a package
    /// </summary>
    public class PackageDownloadHandle : NotificationObject
    {
        /// <summary>
        /// Possible states for a package installation
        /// </summary>
        public enum State
        {
            Uninitialized, Downloading, Downloaded, Installing, Installed, Error
        }

        private string _errorString = "";
        /// <summary>
        /// Error message that resulted from an unsuccessful installation
        /// </summary>
        public string ErrorString { get { return _errorString; } set { _errorString = value; RaisePropertyChanged("ErrorString"); } }

        private State _downloadState = State.Uninitialized;
        /// <summary>
        /// State of the installation of the package
        /// </summary>
        public State DownloadState
        {
            get { return _downloadState; }
            set
            {
                _downloadState = value;
                RaisePropertyChanged("DownloadState");
            }
        }

        /// <summary>
        /// Name of the package
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Identifier of the package
        /// </summary>
        public string Id { get; set; }

        private string _downloadPath;
        /// <summary>
        /// Path where the package is downloaded to
        /// </summary>
        public string DownloadPath { get { return _downloadPath; } set { _downloadPath = value; RaisePropertyChanged("DownloadPath"); } }

        private string _versionName;
        /// <summary>
        /// Version of the package
        /// </summary>
        public string VersionName { get { return _versionName; } set { _versionName = value; RaisePropertyChanged("VersionName"); } }

        /// <summary>
        /// Creates an empty view model for a package installation 
        /// </summary>
        public PackageDownloadHandle()
        {
            this.DownloadPath = string.Empty;
        }

        /// <summary>
        /// Transitions the installation to error with an error message
        /// </summary>
        /// <param name="errorString">Error message</param>
        public void Error(string errorString)
        {
            this.DownloadState = State.Error;
            this.ErrorString = errorString;
        }

        /// <summary>
        /// Transition the installation to downloaded with a path to the file
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        public void Done(string filePath)
        {
            this.DownloadState = State.Downloaded;
            this.DownloadPath = filePath;
        }

        private static string BuildInstallDirectoryString(string packagesDirectory, string name)
        {
            // <user>/appdata/roaming/packages/package_name
            return packagesDirectory + @"\" + name.Replace("/", "_").Replace(@"\", "_");
        }

        /// <summary>
        /// Unzips the downloaded package to a staging directory and parses <c>pkg.json</c>.
        /// The caller must delete <paramref name="stagingDirectory"/> with
        /// <see cref="DiscardStagingDirectory"/> when installation is aborted or after
        /// <see cref="CompleteInstallation"/> (which leaves the staging folder in place — discard afterward).
        /// </summary>
        public bool TryPrepareInstallation(DynamoModel dynamoModel, out Package pkg, out string stagingDirectory)
        {
            pkg = null;
            stagingDirectory = null;
            this.DownloadState = State.Installing;

            var unzipPath = Greg.Utility.FileUtilities.UnZip(DownloadPath);
            if (!Directory.Exists(unzipPath))
            {
                throw new Exception(Properties.Resources.PackageEmpty);
            }

            stagingDirectory = unzipPath;
            pkg = Package.FromDirectory(unzipPath, dynamoModel.Logger);
            return pkg != null;
        }

        /// <summary>
        /// Copies a staged package into the Dynamo packages directory tree and sets <paramref name="pkg"/>.RootDirectory.
        /// </summary>
        /// <param name="pkg">Package metadata (initially rooted at the staging folder).</param>
        /// <param name="stagingDirectory">Path returned from <see cref="TryPrepareInstallation"/>.</param>
        /// <param name="packagesRootDirectory">Root packages folder (e.g. default or custom package path).</param>
        public void CompleteInstallation(Package pkg, string stagingDirectory, string packagesRootDirectory)
        {
            if (pkg == null)
            {
                throw new ArgumentNullException(nameof(pkg));
            }
            if (string.IsNullOrEmpty(stagingDirectory))
            {
                throw new ArgumentException("Staging directory is required.", nameof(stagingDirectory));
            }

            var installedPath = BuildInstallDirectoryString(packagesRootDirectory, pkg.Name);
            Directory.CreateDirectory(installedPath);

            foreach (string dirPath in Directory.GetDirectories(stagingDirectory, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(stagingDirectory, installedPath));
            }

            foreach (string newPath in Directory.GetFiles(stagingDirectory, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(stagingDirectory, installedPath));
            }

            pkg.RootDirectory = installedPath;
        }

        /// <summary>
        /// Deletes a staging directory created by <see cref="TryPrepareInstallation"/>.
        /// </summary>
        public static void DiscardStagingDirectory(string stagingDirectory)
        {
            if (string.IsNullOrEmpty(stagingDirectory) || !Directory.Exists(stagingDirectory))
            {
                return;
            }

            try
            {
                Directory.Delete(stagingDirectory, true);
            }
            catch
            {
                // Best-effort cleanup; avoid blocking package manager UX on locked files.
            }
        }

        /// <summary>
        /// Extracts and parses the metadata of a downloaded package
        /// </summary>
        /// <param name="dynamoModel">Dynamo model</param>
        /// <param name="installDirectory">If specified, overrides Dynamo's default base folder for packages</param>
        /// <param name="pkg">Metatda parsed from the package</param>
        /// <returns>Whether the operation succeeded or not</returns>
        public bool Extract(DynamoModel dynamoModel, string installDirectory, out Package pkg)
        {
            string stagingDirectory = null;
            try
            {
                if (!TryPrepareInstallation(dynamoModel, out pkg, out stagingDirectory))
                {
                    return false;
                }

                if (String.IsNullOrEmpty(installDirectory))
                {
                    installDirectory = dynamoModel.PathManager.DefaultPackagesDirectory;
                }

                CompleteInstallation(pkg, stagingDirectory, installDirectory);
                return true;
            }
            finally
            {
                DiscardStagingDirectory(stagingDirectory);
            }
        }

        // cancel, install, redownload

    }

}
