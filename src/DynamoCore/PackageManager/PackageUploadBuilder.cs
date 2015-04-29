using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Utilities;
using Greg.Requests;
using Greg.Responses;
using RestSharp.Serializers;

namespace Dynamo.PackageManager
{
    internal class ICustomNodeManager
    {
        
    }

    internal class PackageUploadBuilder
    {
        private readonly IFileSystem fileSystem;
        private readonly ICompressor compressor;

        public const string PackageEngineName = "dynamo";
        public const long MaximumPackageSize = 100 * 1024 * 1024;

        internal struct UploadParams
        {
            public string RootDirectory;
            public ICustomNodeManager CustomNodeManager;
            public Package Package;
            public IEnumerable<string> Files;
            public PackageUploadHandle Handle;
            public bool IsTestMode;
        }

        public PackageUploadBuilder(IFileSystem fileSystem, ICompressor compressor)
        {
            this.fileSystem = fileSystem;
            this.compressor = compressor;
        }

        #region Core operative methods

        public PackageUpload NewPackage(UploadParams p)
        {
            var requestBody = NewRequestBody(p.Package);
            var zipPath = UpdateFilesAndZip(requestBody, p);

            return new PackageUpload(requestBody, zipPath);
        }

        public PackageVersionUpload NewPackageVersion(UploadParams p)
        {
            var requestBody = NewRequestBody(p.Package);
            var zipPath = UpdateFilesAndZip(requestBody, p);

            return new PackageVersionUpload(requestBody, zipPath);
        }

        #endregion

        #region Utility methods

        public static PackageUploadRequestBody NewRequestBody(Package l)
        {
            var engineVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var engineMetadata = "";

            return new PackageUploadRequestBody(l.Name, l.VersionName, l.Description, l.Keywords, l.License, l.Contents, PackageEngineName,
                                                         engineVersion, engineMetadata, l.Group, l.Dependencies,
                                                         l.SiteUrl, l.RepositoryUrl, l.ContainsBinaries, l.NodeLibraries.Select(x => x.FullName));
        }

        private string UpdateFilesAndZip( PackageUploadRequestBody requestBody, UploadParams p)
        {
            p.Handle.UploadState = PackageUploadHandle.State.Copying;

            IDirectoryInfo rootDir, dyfDir, binDir, extraDir;
            FormPackageDirectory(p.RootDirectory, p.Package.Name, out rootDir, out  dyfDir, out binDir, out extraDir); // shouldn't do anything for pkg versions
            p.Package.RootDirectory = rootDir.FullName;
            WritePackageHeader(requestBody, rootDir);
            CopyFilesIntoPackageDirectory(p.Files, dyfDir, binDir, extraDir);
            RemoveDyfFiles(p.Files, dyfDir);
            RemapCustomNodeFilePaths(p.CustomNodeManager, p.Files, dyfDir.FullName, p.IsTestMode);

            p.Handle.UploadState = PackageUploadHandle.State.Compressing;

            IFileInfo info;

            try
            {
                info = compressor.Zip(rootDir.FullName);
            }
            catch
            {
                // give nicer error
                throw new Exception(Properties.Resources.CouldNotCompressFile);
            }

            if (info.Length > MaximumPackageSize) throw new Exception(Properties.Resources.PackageTooLarge);

            return info.Name;
        }

        private static void RemapCustomNodeFilePaths(CustomNodeManager customNodeManager, IEnumerable<string> filePaths, string dyfRoot, bool isTestMode)
        {
            var defList = filePaths.Where(x => x.EndsWith(".dyf"))
                .Select(customNodeManager.GuidFromPath)
                .Select(
                    id =>
                    {
                        CustomNodeWorkspaceModel def;
                        return
                            new { Success = customNodeManager.TryGetFunctionWorkspace(id, isTestMode, out def), Workspace = def };
                    })
                    .Where(result => result.Success)
                    .Select(result => result.Workspace);

            foreach (var func in defList)
            {
                var newPath = Path.Combine(dyfRoot, Path.GetFileName(func.FileName));
                func.FileName = newPath;
            }
        }

        private void RemoveDyfFiles(IEnumerable<string> filePaths, IDirectoryInfo dyfDir)
        {
            filePaths
                .Where(x => x.EndsWith(".dyf") && File.Exists(x) && Path.GetDirectoryName(x) != dyfDir.FullName)
                .ToList()
                .ForEach( fileSystem.DeleteFile );
        }

        private void FormPackageDirectory(string packageDirectory, string packageName, out IDirectoryInfo root, out IDirectoryInfo dyfDir, out IDirectoryInfo binDir, out IDirectoryInfo extraDir)
        {
            var rootPath = Path.Combine(packageDirectory, packageName);
            var dyfPath = Path.Combine(rootPath, "dyf");
            var binPath = Path.Combine(rootPath, "bin");
            var extraPath = Path.Combine(rootPath, "extra");

            root = fileSystem.TryCreateDirectory(rootPath);
            dyfDir = fileSystem.TryCreateDirectory(dyfPath);
            binDir = fileSystem.TryCreateDirectory(binPath);
            extraDir = fileSystem.TryCreateDirectory(extraPath);
        }

        private void WritePackageHeader(PackageUploadRequestBody pkgHeader, IDirectoryInfo rootDir)
        {
            // build the package header json, which will be stored with the pkg
            var jsSer = new JsonSerializer();
            var pkgHeaderStr = jsSer.Serialize(pkgHeader);

            // write the pkg header to the root directory of the pkg
            var headerPath = Path.Combine(rootDir.FullName, "pkg.json");
            if (fileSystem.FileExists(headerPath))
            {
                fileSystem.DeleteFile(headerPath);
            }

            fileSystem.WriteAllText(headerPath, pkgHeaderStr);
        }

        private static bool IsXmlDocFile(string path, IEnumerable<string> files)
        {
            if (!path.ToLower().EndsWith(".xml")) return false;

            var fn = Path.GetFileNameWithoutExtension(path);

            return
                files.Where(x => x.EndsWith(".dll"))
                    .Select(Path.GetFileNameWithoutExtension)
                    .Contains(fn);
        }

        private static bool IsDynamoCustomizationFile(string path, IEnumerable<string> files)
        {
            if (!path.ToLower().EndsWith(".xml")) return false;

            var name = Path.GetFileNameWithoutExtension(path);

            if (!name.EndsWith("_DynamoCustomization")) return false;

            name = name.Remove(name.Length - "_DynamoCustomization".Length);

            return
                files.Where(x => x.EndsWith(".dll"))
                    .Select(Path.GetFileNameWithoutExtension)
                    .Contains(name);
        }

        public static string NormalizePath(string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       .ToUpperInvariant();
        }

        private void CopyFilesIntoPackageDirectory(IEnumerable<string> files, IDirectoryInfo dyfDir,
                                                          IDirectoryInfo binDir, IDirectoryInfo extraDir)
        {
            // copy the files to their destination
            foreach (var file in files)
            {
                if (file == null)
                {
                    continue;
                }

                if (!fileSystem.FileExists(file))
                {
                    continue;
                }
                string destPath;

                if (file.ToLower().EndsWith(".dyf"))
                {
                    destPath = Path.Combine(dyfDir.FullName, Path.GetFileName(file));
                }
                else if (file.ToLower().EndsWith(".dll") || IsXmlDocFile(file, files) 
                    || IsDynamoCustomizationFile(file, files))
                {
                    destPath = Path.Combine(binDir.FullName, Path.GetFileName(file));
                }
                else
                {
                    destPath = Path.Combine(extraDir.FullName, Path.GetFileName(file));
                }

                if (NormalizePath(destPath) == NormalizePath(file))
                {
                    continue;
                }

                if (fileSystem.FileExists(destPath))
                {
                    fileSystem.DeleteFile(destPath);
                }

                fileSystem.CopyFile(file, destPath);
            }
        }

#endregion

    }
}
