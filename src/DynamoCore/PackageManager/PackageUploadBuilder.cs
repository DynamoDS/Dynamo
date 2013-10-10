﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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

            return new PackageUploadRequestBody(l.Name, l.VersionName, l.Description, l.Keywords, "MIT", l.Contents, "dynamo",
                                                         engineVersion, engineMetadata, l.Group, l.Dependencies );
        }

        public static PackageUpload NewPackage(Package pkg, List<string> files, PackageUploadHandle uploadHandle)
        {
            var zipPath = DoPackageFileOperationsAndZip(pkg, files, uploadHandle);
            return BuildPackageUpload(pkg.Header, zipPath);
        }

        public static PackageVersionUpload NewPackageVersion(Package pkg, List<string> files, PackageUploadHandle uploadHandle)
        {
            var zipPath = DoPackageFileOperationsAndZip(pkg, files, uploadHandle);
            return BuildPackageVersionUpload(pkg.Header, zipPath);
        }


    #region Utility methods

        private static string DoPackageFileOperationsAndZip(Package pkg, List<string> files, PackageUploadHandle uploadHandle)
        {
            uploadHandle.UploadState = PackageUploadHandle.State.Copying;

            DirectoryInfo rootDir, dyfDir, binDir, extraDir;
            FormPackageDirectory(dynSettings.PackageLoader.RootPackagesDirectory, pkg.Name, out rootDir, out  dyfDir, out binDir, out extraDir); // shouldn't do anything for pkg versions
            pkg.RootDirectory = rootDir.FullName;
            WritePackageHeader(pkg.Header, rootDir);
            CopyFilesIntoPackageDirectory(files, dyfDir, binDir, extraDir);
            RemoveDyfFiles(files, dyfDir); 
            RemapCustomNodeFilePaths(files, dyfDir.FullName);

            uploadHandle.UploadState = PackageUploadHandle.State.Compressing;

            var zipPath = Greg.Utility.FileUtilities.Zip(rootDir.FullName);

            return zipPath;
        }


        private static PackageVersionUpload BuildPackageVersionUpload(PackageUploadRequestBody pkgHeader, string zipPath )
        {
            return new PackageVersionUpload(  pkgHeader.name,
                                                pkgHeader.version,
                                                pkgHeader.description,
                                                pkgHeader.keywords,
                                                pkgHeader.contents,
                                                "dynamo",
                                                pkgHeader.engine_version,
                                                pkgHeader.engine_metadata,
                                                pkgHeader.group,
                                                zipPath,
                                                pkgHeader.dependencies);    
        }

        private static PackageUpload BuildPackageUpload(PackageUploadRequestBody pkgHeader, string zipPath)
        {
            return new PackageUpload(pkgHeader.name,
                                        pkgHeader.version,
                                        pkgHeader.description,
                                        pkgHeader.keywords,
                                        pkgHeader.license,
                                        pkgHeader.contents,
                                        "dynamo",
                                        pkgHeader.engine_version,
                                        pkgHeader.engine_metadata,
                                        pkgHeader.group,
                                        zipPath,
                                        pkgHeader.dependencies);
        }

        private static void RemapCustomNodeFilePaths( IEnumerable<string> filePaths, string dyfRoot )
        {

            var defList= filePaths
                .Where(x => x.EndsWith(".dyf"))
                .Select( path => dynSettings.CustomNodeManager.GuidFromPath(path))
                .Select( guid => dynSettings.CustomNodeManager.GetFunctionDefinition(guid) )
                .ToList();
                
            defList.ForEach( func =>
                    {
                        var newPath = Path.Combine(dyfRoot, Path.GetFileName(func.WorkspaceModel.FileName));
                        func.WorkspaceModel.FileName = newPath;
                        dynSettings.CustomNodeManager.SetNodePath(func.FunctionId, newPath);
                    });
        }

        private static void RemoveDyfFiles(IEnumerable<string> filePaths, DirectoryInfo dyfDir)
        {
            filePaths
                .Where(x => x.EndsWith(".dyf") && File.Exists(x) && Path.GetDirectoryName(x) != dyfDir.FullName)
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
            var dyfPath = Path.Combine(rootPath, "dyf");
            var binPath = Path.Combine(rootPath, "bin");
            var extraPath = Path.Combine(rootPath, "extra");

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
            var headerPath = Path.Combine(rootDir.FullName, "pkg.json");
            if (File.Exists(headerPath)) File.Delete(headerPath);
            File.WriteAllText(headerPath, pkgHeaderStr);
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

                
                if (file.EndsWith("dyf"))
                {
                    destPath = Path.Combine(dyfDir.FullName, Path.GetFileName(file));
                }
                else if (file.EndsWith("dll") || file.EndsWith("exe"))
                {
                    destPath = Path.Combine(binDir.FullName, Path.GetFileName(file));
                }
                else
                {
                    destPath = Path.Combine(extraDir.FullName, Path.GetFileName(file));
                }

                if (destPath == file) continue;
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
