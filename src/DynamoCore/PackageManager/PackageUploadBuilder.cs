using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Models;
using Dynamo.Utilities;
using Greg.Requests;
using RestSharp.Serializers;

namespace Dynamo.PackageManager
{
    static class PackageUploadBuilder
    {
        public static PackageUploadRequestBody NewPackageHeader( Package l )
        {
            var engineVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var engineMetadata = "";

            return new PackageUploadRequestBody(l.Name, l.VersionName, l.Description, l.Keywords, l.License, l.Contents, "dynamo",
                                                         engineVersion, engineMetadata, l.Group, l.Dependencies, 
                                                         l.SiteUrl, l.RepositoryUrl, l.ContainsBinaries, l.NodeLibraries.Select(x => x.FullName) ); 
        } 

        public static PackageUpload NewPackage(DynamoModel dynamoModel, Package pkg, List<string> files, PackageUploadHandle uploadHandle)
        {
            var header = NewPackageHeader(pkg);
            var zipPath = DoPackageFileOperationsAndZip(dynamoModel, header, pkg, files, uploadHandle);
            return new PackageUpload(header, zipPath);
        }

        public static PackageVersionUpload NewPackageVersion(DynamoModel dynamoModel, Package pkg, List<string> files, PackageUploadHandle uploadHandle)
        {
            var header = NewPackageHeader(pkg);
            var zipPath = DoPackageFileOperationsAndZip(dynamoModel, header, pkg, files, uploadHandle);
            return new PackageVersionUpload(header, zipPath);
        }

    #region Utility methods

        private static string DoPackageFileOperationsAndZip(DynamoModel dynamoModel, PackageUploadRequestBody header, Package pkg, List<string> files, PackageUploadHandle uploadHandle)
        {
            uploadHandle.UploadState = PackageUploadHandle.State.Copying;

            DirectoryInfo rootDir, dyfDir, binDir, extraDir;
            FormPackageDirectory(dynamoModel.Loader.PackageLoader.RootPackagesDirectory, pkg.Name, out rootDir, out  dyfDir, out binDir, out extraDir); // shouldn't do anything for pkg versions
            pkg.RootDirectory = rootDir.FullName;
            WritePackageHeader(header, rootDir);
            CopyFilesIntoPackageDirectory(files, dyfDir, binDir, extraDir);
            RemoveDyfFiles(files, dyfDir); 
            RemapCustomNodeFilePaths(dynamoModel.CustomNodeManager, files, dyfDir.FullName);

            uploadHandle.UploadState = PackageUploadHandle.State.Compressing;

            string zipPath;
            FileInfo info;

            try
            {
                zipPath = Greg.Utility.FileUtilities.Zip(rootDir.FullName);
                info = new FileInfo(zipPath);
            }
            catch
            {
                // give nicer error
                throw new Exception(Properties.Resources.CouldNotCompressFile);
            }

            if (info.Length > 15 * 1000000) throw new Exception(Properties.Resources.PackageTooLarge);

            return zipPath;
        }

        private static void RemapCustomNodeFilePaths( CustomNodeManager customNodeManager, IEnumerable<string> filePaths, string dyfRoot )
        {
            var defList = filePaths
                .Where(x => x.EndsWith(/*NXLT*/".dyf"))
                .Select(customNodeManager.GuidFromPath)
                .Select(customNodeManager.GetFunctionDefinition)
                .ToList();
                
            defList.ForEach( func =>
                    {
                        var newPath = Path.Combine(dyfRoot, Path.GetFileName(func.WorkspaceModel.FileName));
                        func.WorkspaceModel.FileName = newPath;
                        customNodeManager.SetNodePath(func.FunctionId, newPath);
                    });
        }

        private static void RemoveDyfFiles(IEnumerable<string> filePaths, DirectoryInfo dyfDir)
        {
            filePaths
                .Where(x => x.EndsWith(/*NXLT*/".dyf") && File.Exists(x) && Path.GetDirectoryName(x) != dyfDir.FullName)
                .ToList()
                .ForEach( File.Delete );
        }

        private static DirectoryInfo TryCreateDirectory(string path)
        {
            return Directory.Exists(path) ? new DirectoryInfo(path) : Directory.CreateDirectory(path);
        }

        private static void FormPackageDirectory(string packageDirectory, string packageName, out DirectoryInfo root, out DirectoryInfo dyfDir, out DirectoryInfo binDir, out DirectoryInfo extraDir )
        {
            // create a directory where the package will be stored
            var rootPath = Path.Combine(packageDirectory, packageName);
            var dyfPath = Path.Combine(rootPath, /*NXLT*/"dyf");
            var binPath = Path.Combine(rootPath, /*NXLT*/"bin");
            var extraPath = Path.Combine(rootPath, /*NXLT*/"extra");

            root = TryCreateDirectory(rootPath);
            dyfDir = TryCreateDirectory(dyfPath);
            binDir = TryCreateDirectory(binPath);
            extraDir = TryCreateDirectory(extraPath);
        }

        private static void WritePackageHeader(PackageUploadRequestBody pkgHeader, DirectoryInfo rootDir)
        {
            // build the package header json, which will be stored with the pkg
            var jsSer = new JsonSerializer();
            var pkgHeaderStr = jsSer.Serialize(pkgHeader);

            // write the pkg header to the root directory of the pkg
            var headerPath = Path.Combine(rootDir.FullName, /*NXLT*/"pkg.json");
            if (File.Exists(headerPath)) File.Delete(headerPath);
            File.WriteAllText(headerPath, pkgHeaderStr);
        }

        private static bool IsXmlDocFile(string path, IEnumerable<string> files)
        {
            if (!path.ToLower().EndsWith(/*NXLT*/".xml")) return false;

            var fn = Path.GetFileNameWithoutExtension(path);

            return
                files.Where(x => x.EndsWith(/*NXLT*/".dll"))
                    .Select(Path.GetFileNameWithoutExtension)
                    .Contains(fn);
        }

        private static bool IsDynamoCustomizationFile(string path, IEnumerable<string> files)
        {
            if (!path.ToLower().EndsWith(/*NXLT*/".xml")) return false;

            var name = Path.GetFileNameWithoutExtension(path);

            if (!name.EndsWith(/*NXLT*/"_DynamoCustomization")) return false;

            name = name.Remove(name.Length - /*NXLT*/"_DynamoCustomization".Length);

            return
                files.Where(x => x.EndsWith(/*NXLT*/".dll"))
                    .Select(Path.GetFileNameWithoutExtension)
                    .Contains(name);
        }

        public static string NormalizePath(string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       .ToUpperInvariant();
        }

        private static void CopyFilesIntoPackageDirectory(IEnumerable<string> files, DirectoryInfo dyfDir,
                                                          DirectoryInfo binDir, DirectoryInfo extraDir)
        {
            // copy the files to their destination
            foreach (var file in files)
            {
                if (file == null) continue;
                if (!File.Exists(file)) continue;
                string destPath;

                if (file.ToLower().EndsWith(/*NXLT*/".dyf"))
                {
                    destPath = Path.Combine(dyfDir.FullName, Path.GetFileName(file));
                }
                else if (file.ToLower().EndsWith(/*NXLT*/".dll") || IsXmlDocFile(file, files) 
                    || IsDynamoCustomizationFile(file, files))
                {
                    destPath = Path.Combine(binDir.FullName, Path.GetFileName(file));
                }
                else
                {
                    destPath = Path.Combine(extraDir.FullName, Path.GetFileName(file));
                }

                if (NormalizePath(destPath) == NormalizePath(file)) continue;
                if (File.Exists(destPath))
                {
                    File.Delete(destPath);
                }

                File.Copy(file, destPath);
            }
        }

#endregion

    }
}
