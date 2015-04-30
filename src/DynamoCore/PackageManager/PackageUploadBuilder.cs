using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Greg.Requests;

namespace Dynamo.PackageManager
{
    internal class PackageUploadBuilder
    {
        private readonly PackageDirectoryBuilder builder;
        private readonly IFileCompressor fileCompressor;

        internal const string PackageEngineName = "dynamo";
        internal const long MaximumPackageSize = 100 * 1024 * 1024;

        internal PackageUploadBuilder(PackageDirectoryBuilder builder, IFileCompressor fileCompressor)
        {
            if (builder == null) throw new ArgumentNullException("builder");
            if (fileCompressor == null) throw new ArgumentNullException("fileCompressor");

            this.builder = builder;
            this.fileCompressor = fileCompressor;
        }
         
        #region Public Operational Class Methods

        internal static PackageUploadRequestBody NewRequestBody(Package l)
        {
            var engineVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var engineMetadata = "";

            return new PackageUploadRequestBody(l.Name, l.VersionName, l.Description, l.Keywords, l.License, l.Contents, PackageEngineName,
                                                         engineVersion, engineMetadata, l.Group, l.Dependencies,
                                                         l.SiteUrl, l.RepositoryUrl, l.ContainsBinaries, l.NodeLibraries.Select(x => x.FullName));
        }


        internal PackageUpload NewPackageUpload(Package package, string packagesDirectory, IEnumerable<string> files, PackageUploadHandle handle)
        {
            handle.UploadState = PackageUploadHandle.State.Copying;

            var dir = builder.BuildDirectory(package, packagesDirectory, files);

            handle.UploadState = PackageUploadHandle.State.Compressing;

            var zipFile = Zip(dir);

            return new PackageUpload(NewRequestBody(package), zipFile.Name);
        }

        internal PackageVersionUpload NewPackageVersionUpload(Package package, string packagesDirectory, IEnumerable<string> files, PackageUploadHandle handle)
        {
            handle.UploadState = PackageUploadHandle.State.Copying;

            var dir = builder.BuildDirectory(package, packagesDirectory, files);

            handle.UploadState = PackageUploadHandle.State.Compressing;

            var zipFile = Zip(dir);

            return new PackageVersionUpload(NewRequestBody(package), zipFile.Name);
        }

        #endregion

        #region Private Class Methods

        private IFileInfo Zip(IDirectoryInfo directory)
        {
            IFileInfo info;

            try
            {
                info = fileCompressor.Zip(directory.FullName);
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
