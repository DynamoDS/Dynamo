using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Annotations;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Utilities;
using Greg.Requests;
using RestSharp.Serializers;

namespace Dynamo.PackageManager
{
    internal class TrueFileSystem : IFileSystem
    {
        public void CopyFile([NotNull] string filePath, [NotNull] string destinationPath)
        {
            File.Copy(filePath, destinationPath);
        }

        public void DeleteFile([NotNull] string filePath)
        {
            File.Delete(filePath);
        }

        public  IDirectoryInfo TryCreateDirectory(string path)
        {
            return this.DirectoryExists(path)
                ? new TrueDirectoryInfo(new System.IO.DirectoryInfo(path))
                : new TrueDirectoryInfo(Directory.CreateDirectory(path));
        }

        public bool DirectoryExists([NotNull] string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }

        public bool FileExists([NotNull] string filePath)
        {
            return File.Exists(filePath);
        }

        public void WriteAllText([NotNull] string filePath, [NotNull] string content)
        {
            File.WriteAllText(filePath, content);
        }
    }

    internal class TrueDirectoryInfo : IDirectoryInfo
    {
        private readonly System.IO.DirectoryInfo dirInfo;

        public TrueDirectoryInfo(System.IO.DirectoryInfo dirInfo)
        {
            this.dirInfo = dirInfo;
        }

        public string FullName
        {
            get { return dirInfo.FullName; }
        }
    }

    internal class TrueFileInfo : IFileInfo
    {
        private readonly System.IO.FileInfo fileInfo;

        public TrueFileInfo(string path)
        {
            this.fileInfo = new FileInfo(path);
        }

        public long Length
        {
            get { return fileInfo.Length; }
        }
    }

    internal class PackageUploadBuilder
    {
        private readonly IFileSystem fileSystem;
        private readonly IDataCompressor dataCompressor;

        public PackageUploadBuilder(IFileSystem fileSystem, IDataCompressor dataCompressor)
        {
            this.fileSystem = fileSystem;
            this.dataCompressor = dataCompressor;
        }

        public static PackageUploadRequestBody NewPackageHeader( Package l )
        {
            var engineVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var engineMetadata = "";

            return new PackageUploadRequestBody(l.Name, l.VersionName, l.Description, l.Keywords, l.License, l.Contents, "dynamo",
                                                         engineVersion, engineMetadata, l.Group, l.Dependencies, 
                                                         l.SiteUrl, l.RepositoryUrl, l.ContainsBinaries, l.NodeLibraries.Select(x => x.FullName) ); 
        } 

        public PackageUpload NewPackage(string rootPkgDir, CustomNodeManager customNodeManager, Package pkg, List<string> files, PackageUploadHandle uploadHandle, bool isTestMode)
        {
            var header = NewPackageHeader(pkg);
            var zipPath = DoPackageFileOperationsAndZip(rootPkgDir, customNodeManager, header, pkg, files, uploadHandle, isTestMode);
            return new PackageUpload(header, zipPath);
        }

        public PackageVersionUpload NewPackageVersion(string rootPkgDir, CustomNodeManager customNodeManager, Package pkg, List<string> files, PackageUploadHandle uploadHandle, bool isTestMode)
        {
            var header = NewPackageHeader(pkg);
            var zipPath = DoPackageFileOperationsAndZip(rootPkgDir, customNodeManager, header, pkg, files, uploadHandle, isTestMode);
            return new PackageVersionUpload(header, zipPath);
        }

    #region Utility methods

        private string DoPackageFileOperationsAndZip(string rootPkgDir, CustomNodeManager customNodeManager, 
            PackageUploadRequestBody header, Package pkg, List<string> files, PackageUploadHandle uploadHandle, bool isTestMode)
        {
            uploadHandle.UploadState = PackageUploadHandle.State.Copying;

            IDirectoryInfo rootDir, dyfDir, binDir, extraDir;
            FormPackageDirectory(rootPkgDir, pkg.Name, out rootDir, out  dyfDir, out binDir, out extraDir); // shouldn't do anything for pkg versions
            pkg.RootDirectory = rootDir.FullName;
            WritePackageHeader(header, rootDir);
            CopyFilesIntoPackageDirectory(files, dyfDir, binDir, extraDir);
            RemoveDyfFiles(files, dyfDir); 
            RemapCustomNodeFilePaths(customNodeManager, files, dyfDir.FullName, isTestMode);

            uploadHandle.UploadState = PackageUploadHandle.State.Compressing;

            string zipPath;
            IFileInfo info;

            try
            {
                zipPath = dataCompressor.Zip(rootDir.FullName);
                info = new TrueFileInfo(zipPath);
            }
            catch
            {
                // give nicer error
                throw new Exception(Properties.Resources.CouldNotCompressFile);
            }

            if (info.Length > 100 * 1024 * 1024) throw new Exception(Properties.Resources.PackageTooLarge);

            return zipPath;
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
                    }).Where(result => result.Success).Select(result => result.Workspace);

            foreach (var func in defList)
            {
                var newPath = Path.Combine(dyfRoot, Path.GetFileName(func.FileName));
                func.FileName = newPath;
            }
        }

        private static void RemoveDyfFiles(IEnumerable<string> filePaths, IDirectoryInfo dyfDir)
        {
            filePaths
                .Where(x => x.EndsWith(".dyf") && File.Exists(x) && Path.GetDirectoryName(x) != dyfDir.FullName)
                .ToList()
                .ForEach( File.Delete );
        }

        private void FormPackageDirectory(string packageDirectory, string packageName, out IDirectoryInfo root, out IDirectoryInfo dyfDir, out IDirectoryInfo binDir, out IDirectoryInfo extraDir)
        {
            // create a directory where the package will be stored
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
