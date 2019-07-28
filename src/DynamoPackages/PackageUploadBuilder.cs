using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dynamo.PackageManager.Interfaces;
using Greg.Requests;

namespace Dynamo.PackageManager
{
    public interface IPackageUploadBuilder
    {
        PackageUpload NewPackageUpload(Package package, string packagesDirectory, IEnumerable<string> files,
            PackageUploadHandle handle);

        PackageVersionUpload NewPackageVersionUpload(Package package, string packagesDirectory,
            IEnumerable<string> files, PackageUploadHandle handle);
    }

    internal class PackageUploadBuilder : IPackageUploadBuilder
    {
        private readonly IPackageDirectoryBuilder builder;
        private readonly IFileCompressor fileCompressor;
        private static string engineVersion;

        internal static void SetEngineVersion(Version version)
        {
            if (version != null)
            {
                engineVersion = version.ToString();
            }
        }

        internal const long MaximumPackageSize = 1000 * 1024 * 1024; // 1 GB

        internal PackageUploadBuilder(IPackageDirectoryBuilder builder, IFileCompressor fileCompressor)
        {
            if (builder == null) throw new ArgumentNullException("builder");
            if (fileCompressor == null) throw new ArgumentNullException("fileCompressor");

            this.builder = builder;
            this.fileCompressor = fileCompressor;
        }
         
        #region Public Operational Class Methods

        public static PackageUploadRequestBody NewRequestBody(Package package)
        {
            if (package == null) throw new ArgumentNullException("package");

            var version = engineVersion ?? Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var engineMetadata = "";

            return new PackageUploadRequestBody(package.Name, package.VersionName, package.Description, package.Keywords, package.License, package.Contents, PackageManagerClient.PackageEngineName,
                                                         version, engineMetadata, package.Group, package.Dependencies,
                                                         package.SiteUrl, package.RepositoryUrl, package.ContainsBinaries, 
                                                         package.NodeLibraries.Select(x => x.FullName), package.HostDependencies);
        }


        public PackageUpload NewPackageUpload(Package package, string packagesDirectory, IEnumerable<string> files, PackageUploadHandle handle)
        {
            if (package == null) throw new ArgumentNullException("package");
            if (packagesDirectory == null) throw new ArgumentNullException("packagesDirectory");
            if (files == null) throw new ArgumentNullException("files");
            if (handle == null) throw new ArgumentNullException("handle");

            return new PackageUpload(NewRequestBody(package),
                BuildAndZip(package, packagesDirectory, files, handle).Name);
        }

        public PackageVersionUpload NewPackageVersionUpload(Package package, string packagesDirectory, IEnumerable<string> files, PackageUploadHandle handle)
        {
            if (package == null) throw new ArgumentNullException("package");
            if (packagesDirectory == null) throw new ArgumentNullException("packagesDirectory");
            if (files == null) throw new ArgumentNullException("files");
            if (handle == null) throw new ArgumentNullException("handle");

            return new PackageVersionUpload(NewRequestBody(package), BuildAndZip(package, packagesDirectory, files, handle).Name);
        }

        #endregion

        #region Private Class Methods

        private IFileInfo BuildAndZip(Package package, string packagesDirectory, IEnumerable<string> files, PackageUploadHandle handle)
        {
            handle.UploadState = PackageUploadHandle.State.Copying;

            var dir = builder.BuildDirectory(package, packagesDirectory, files);

            handle.UploadState = PackageUploadHandle.State.Compressing;

            return Zip(dir);
        }

        private IFileInfo Zip(IDirectoryInfo directory)
        {
            IFileInfo info;

            try
            {
                info = fileCompressor.Zip(directory);
            }
            catch
            {
                throw new Exception(Properties.Resources.CouldNotCompressFile);
            }

            // the file is stored in a tempt directory, we allow it to get cleaned up by the user later
            if (info.Length > MaximumPackageSize) throw new Exception(Properties.Resources.PackageTooLarge);

            return info;
        }

        #endregion
    }


}
