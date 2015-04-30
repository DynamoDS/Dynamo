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
    /// <summary>
    ///     This class provides all of the tools to build a Package directory of the correct structure.
    /// </summary>
    internal class PackageDirectoryBuilder
    {
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

        #region Utility methods

        /// <summary>
        ///     Forms a properly formed package directory
        /// </summary>
        /// <param name="package">The package to be formed</param>
        /// <param name="targetDirectory">The directory where the package should end up.  It is
        ///     assumed that this directory is empty.  If it doesn't exist,
        ///     it will be created.
        /// </param>
        /// <param name="files">The collection of files to be moved</param>
        /// <returns></returns>
        internal IDirectoryInfo BuildDirectory(Package package, string packagesDirectory, IEnumerable<string> files)
        {
            IDirectoryInfo rootDir, dyfDir, binDir, extraDir;

            FormPackageDirectory(packagesDirectory, package.Name, out rootDir, out  dyfDir, out binDir, out extraDir); // shouldn't do anything for pkg versions
            package.RootDirectory = rootDir.FullName;

            WritePackageHeader(package, rootDir);
            CopyFilesIntoPackageDirectory(files, dyfDir, binDir, extraDir);
            RemoveDyfFiles(files, dyfDir);
            RemapCustomNodeFilePaths(files, dyfDir.FullName);

            return rootDir;
        }

        internal void RemapCustomNodeFilePaths(IEnumerable<string> filePaths, string dyfRoot)
        {
            foreach (var func in filePaths.Where(x => x.EndsWith(".dyf")))
            {
                pathRemapper.SetPath(func, dyfRoot);
            }
        }

        internal void RemoveDyfFiles(IEnumerable<string> filePaths, IDirectoryInfo dyfDir)
        {
            var dyfsToRemove = filePaths
                .Where(x => x.EndsWith(".dyf") && fileSystem.FileExists(x) && Path.GetDirectoryName(x) != dyfDir.FullName);

            foreach (var dyf in dyfsToRemove)
            {
                fileSystem.DeleteFile(dyf);
            }
        }

        internal void FormPackageDirectory(string packageDirectory, string packageName, out IDirectoryInfo root, out IDirectoryInfo dyfDir, out IDirectoryInfo binDir, out IDirectoryInfo extraDir)
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

        internal void WritePackageHeader(Package package, IDirectoryInfo rootDir)
        {
            var pkgHeader = PackageUploadBuilder.NewRequestBody(package);

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

        internal static bool IsXmlDocFile(string path, IEnumerable<string> files)
        {
            if (!path.ToLower().EndsWith(".xml")) return false;

            var fn = Path.GetFileNameWithoutExtension(path);

            return
                files.Where(x => x.EndsWith(".dll"))
                    .Select(Path.GetFileNameWithoutExtension)
                    .Contains(fn);
        }

        internal static bool IsDynamoCustomizationFile(string path, IEnumerable<string> files)
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

        internal void CopyFilesIntoPackageDirectory(IEnumerable<string> files, IDirectoryInfo dyfDir,
                                                          IDirectoryInfo binDir, IDirectoryInfo extraDir)
        {
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
