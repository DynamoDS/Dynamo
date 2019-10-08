using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.PackageManager.Interfaces;
using Dynamo.Utilities;
using RestSharp.Serializers;

namespace Dynamo.PackageManager
{
    public interface IPackageDirectoryBuilder
    {
        IDirectoryInfo BuildDirectory(Package packages, string packagesDirectory, IEnumerable<string> files);
    }

    /// <summary>
    ///     This class provides all of the tools to build a Package directory of the correct structure.
    /// </summary>
    internal class PackageDirectoryBuilder : IPackageDirectoryBuilder
    {
        internal const string CustomNodeDirectoryName = "dyf";
        internal const string BinaryDirectoryName = "bin";
        internal const string ExtraDirectoryName = "extra";
        internal const string PackageJsonName = "pkg.json";

        private readonly IFileSystem fileSystem;
        private readonly IPathRemapper pathRemapper;

        /// <summary>
        ///     The class constructor
        /// </summary>
        /// <param name="fileSystem">For moving files around</param>
        /// <param name="pathRemapper">For modifying custom node paths</param>
        internal PackageDirectoryBuilder(IFileSystem fileSystem, IPathRemapper pathRemapper) 
        {
            if (fileSystem == null) throw new ArgumentNullException("fileSystem");
            if (pathRemapper == null) throw new ArgumentNullException("pathRemapper");

            this.fileSystem = fileSystem;
            this.pathRemapper = pathRemapper;
        }

        #region Public Class Operational Methods

        /// <summary>
        ///     Forms a properly formed package directory
        /// </summary>
        /// <param name="package">The package to be formed</param>
        /// <param name="packagesDirectory">The parent directory for the parent directory</param>
        /// <param name="files">The collection of files to be moved</param>
        /// <returns></returns>
        public IDirectoryInfo BuildDirectory(Package package, string packagesDirectory, IEnumerable<string> files)
        {
            IDirectoryInfo rootDir, dyfDir, binDir, extraDir;

            FormPackageDirectory(packagesDirectory, package.Name, out rootDir, out  dyfDir, out binDir, out extraDir); // shouldn't do anything for pkg versions
            package.RootDirectory = rootDir.FullName;

            WritePackageHeader(package, rootDir);
            RemoveUnselectedFiles(files, rootDir);
            CopyFilesIntoPackageDirectory(files, dyfDir, binDir, extraDir);
            RemoveDyfFiles(files, dyfDir);
            RemapCustomNodeFilePaths(files, dyfDir.FullName);

            return rootDir;
        }

        #endregion

        #region Private Utility Methods

        private void RemapCustomNodeFilePaths(IEnumerable<string> filePaths, string dyfRoot)
        {
            foreach (var func in filePaths.Where(x => x.EndsWith(".dyf")))
            {
                pathRemapper.SetPath(func, dyfRoot);
            }
        }

        private void RemoveDyfFiles(IEnumerable<string> filePaths, IDirectoryInfo dyfDir)
        {
            var dyfsToRemove = filePaths
                .Where(x => x.EndsWith(".dyf") && fileSystem.FileExists(x) && Path.GetDirectoryName(x) != dyfDir.FullName);

            foreach (var dyf in dyfsToRemove)
            {
                fileSystem.DeleteFile(dyf);
            }
        }

        private void RemoveUnselectedFiles(IEnumerable<string> filePaths, IDirectoryInfo dir)
        {
            // Remove all files which are not listed in the files list
            filePaths = filePaths.Select(x => x.ToLower());
            foreach (var path in fileSystem.GetFiles(dir.FullName).Select(x => x.ToLower())
                .Where(x => !x.EndsWith("pkg.json") && !filePaths.Contains(x)))
            {
                fileSystem.DeleteFile(path);
            }

            // Remove all backup folders
            var backupFolderName = Configuration.Configurations.BackupFolderName.ToLower();
            foreach (var path in fileSystem.GetDirectories(dir.FullName)
                .Where(x => x.Split(new[] { '/', '\\' }).Select(y => y.ToLower()).Contains(backupFolderName)))
            {
                fileSystem.DeleteDirectory(path);
            }
        }

        private void FormPackageDirectory(string packageDirectory, string packageName, out IDirectoryInfo root, out IDirectoryInfo dyfDir, out IDirectoryInfo binDir, out IDirectoryInfo extraDir)
        {
            var rootPath = Path.Combine(packageDirectory, packageName);
            var dyfPath = Path.Combine(rootPath, CustomNodeDirectoryName);
            var binPath = Path.Combine(rootPath, BinaryDirectoryName);
            var extraPath = Path.Combine(rootPath, ExtraDirectoryName);

            root = fileSystem.TryCreateDirectory(rootPath);
            dyfDir = fileSystem.TryCreateDirectory(dyfPath);
            binDir = fileSystem.TryCreateDirectory(binPath);
            extraDir = fileSystem.TryCreateDirectory(extraPath);
        }

        private void WritePackageHeader(Package package, IDirectoryInfo rootDir)
        {
            var pkgHeader = PackageUploadBuilder.NewRequestBody(package);

            // build the package header json, which will be stored with the pkg
            var jsSer = new JsonSerializer();
            var pkgHeaderStr = jsSer.Serialize(pkgHeader);

            // write the pkg header to the root directory of the pkg
            var headerPath = Path.Combine(rootDir.FullName, PackageJsonName);
            if (fileSystem.FileExists(headerPath))
            {
                fileSystem.DeleteFile(headerPath);
            }

            fileSystem.WriteAllText(headerPath, pkgHeaderStr);
        }


        internal void CopyFilesIntoPackageDirectory(IEnumerable<string> files, IDirectoryInfo dyfDir,
                                                          IDirectoryInfo binDir, IDirectoryInfo extraDir)
        {
            // normalize the paths to ensure correct comparison
            var dyfDirPath = NormalizePath(dyfDir.FullName);
            var binDirPath = NormalizePath(binDir.FullName);
            var extraDirPath = NormalizePath(extraDir.FullName);

            foreach (var file in files.Where(x => x != null))
            {
                // If the file doesn't actually exist, don't copy it
                if (!fileSystem.FileExists(file))
                {
                    continue;
                }

                // determine which folder to put the file in
                string targetFolder = extraDirPath; 

                if (file.EndsWith(".dyf"))
                {
                    targetFolder = dyfDirPath;
                }
                else if (file.EndsWith(".dll") || IsXmlDocFile(file, files) || IsDynamoCustomizationFile(file, files))
                {
                    targetFolder = binDirPath;
                }

                // this also accounts for if the file is in a subdirectory of the target folder
                if (NormalizePath(file).StartsWith(targetFolder))
                {
                    continue;
                }

                var destPath = Path.Combine(targetFolder, Path.GetFileName(file));

                if (fileSystem.FileExists(destPath))
                {
                    fileSystem.DeleteFile(destPath);
                }

                fileSystem.CopyFile(file, destPath);
            }
        }

        #endregion

        #region Public Static Utility Methods 
        
        public static bool IsXmlDocFile(string path, IEnumerable<string> files)
        {
            if (!path.ToLower().EndsWith(".xml")) return false;

            var fn = Path.GetFileNameWithoutExtension(path);

            return
                files.Where(x => x.EndsWith(".dll"))
                    .Select(Path.GetFileNameWithoutExtension)
                    .Contains(fn);
        }

        public static bool IsDynamoCustomizationFile(string path, IEnumerable<string> files)
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

        #endregion

    }
}
