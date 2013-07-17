using System;
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

        public static PackageUploadRequestBody NewPackageHeader(    string name,
                                                                    string version,
                                                                    string description,
                                                                    IEnumerable<string> keywords,
                                                                    string license,
                                                                    string group,
                                                                    IEnumerable<PackageDependency> deps,
                                                                    IEnumerable<Tuple<string, string>> nodeNameDescriptionPairs)    
        {
            var contents = String.Join(", ",
                                          nodeNameDescriptionPairs.Select((pair) => pair.Item1 + " - " + pair.Item2));
            var engineVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var engineMetadata = "";

            return new PackageUploadRequestBody(name, version, description, keywords, license, contents, "dynamo",
                                                         engineVersion, engineMetadata, group, deps);
        }

        public static PackageUpload NewPackage( PackageUploadRequestBody pkgHeader,
                                                IEnumerable<string> files,
                                                PackageUploadHandle uploadHandle )
        {

            uploadHandle.UploadState = PackageUploadHandle.State.Copying;

            // modify the file system for package
            DirectoryInfo rootDir, dyfDir, binDir, extraDir;
            FormPackageDirectory(dynSettings.PackageLoader.RootPackagesDirectory, pkgHeader.name, out rootDir, out  dyfDir, out binDir, out extraDir);
            WritePackageHeader(pkgHeader, rootDir);
            CopyFilesIntoPackageDirectory(files, dyfDir, binDir, extraDir);
            RemapCustomNodeFilePaths(files, dyfDir.FullName);
            RemoveDyfFiles(files);

            uploadHandle.UploadState = PackageUploadHandle.State.Compressing;

            var zipPath = Greg.Utility.FileUtilities.Zip(rootDir.FullName);
            return BuildPackageUpload(pkgHeader, zipPath);

        }

        public static PackageUpload NewPackage(LocalPackage pkg, PackageUploadHandle packageUploadHandle)
        {
            throw new NotImplementedException();
        }


        public static PackageVersionUpload NewPackageVersion(LocalPackage pkg, PackageUploadHandle uploadHandle)
        {
            throw new NotImplementedException();
        }

        public static PackageVersionUpload NewPackageVersion(PackageUploadRequestBody pkg, IEnumerable<string> files, PackageUploadHandle uploadHandle)
        {
            throw new NotImplementedException();
        }

    #region Utility methods

        private static PackageUpload BuildPackageUpload(PackageUploadRequestBody pkgHeader, string zipPath )
        {
            return new PackageUpload(  pkgHeader.name,
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
            filePaths
                .Where(x => x.EndsWith(".dyf"))
                .Select( path => dynSettings.CustomNodeLoader.GuidFromPath(path))
                .Select( guid => dynSettings.CustomNodeLoader.GetFunctionDefinition(guid) )
                .ToList()
                .ForEach( func =>
                    {
                        var newPath = Path.Combine(dyfRoot, Path.GetFileName(func.Workspace.FilePath));
                        func.Workspace.FilePath = newPath;
                        dynSettings.CustomNodeLoader.SetNodePath(func.FunctionId, newPath);
                    });
        }

        private static void RemoveDyfFiles(IEnumerable<string> filePaths)
        {
            filePaths
                .Where(x => x.EndsWith(".dyf") && File.Exists(x))
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

                if (file.EndsWith("dyf"))
                {
                    File.Copy(file, Path.Combine(dyfDir.FullName, Path.GetFileName(file)));
                }
                else if (file.EndsWith("dll") || file.EndsWith("exe"))
                {
                    File.Copy(file, Path.Combine(binDir.FullName, Path.GetFileName(file)));
                }
                else
                {
                    File.Copy(file, Path.Combine(extraDir.FullName, Path.GetFileName(file)));
                }

            }
        }

#endregion


    }
}
