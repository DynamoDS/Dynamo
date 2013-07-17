using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dynamo.Utilities;
using Greg.Requests;
using RestSharp.Serializers;

namespace Dynamo.PackageManager
{
    static class PackageUploadBuilder
    {
        public static PackageUpload BuildNewPackage(PackageUploadRequestBody pkgHeader,
                                                    IEnumerable<string> files,
                                                    PackageUploadHandle uploadHandle )
        {
            // tell progress
            uploadHandle.UploadState = PackageUploadHandle.State.Copying;

            // create a directory where the package will be stored
            var rootDir = Directory.CreateDirectory(Path.Combine(dynSettings.PackageLoader.RootPackagesDirectory, pkgHeader.name));

            // build the directory substructure
            var binDir = rootDir.CreateSubdirectory("bin");
            var dyfDir = rootDir.CreateSubdirectory("dyf");
            var extraDir = rootDir.CreateSubdirectory("extra");

            // build the package header json, which will be stored with the pkg
            var jsSer = new JsonSerializer();
            var pkgHeaderStr = jsSer.Serialize(pkgHeader);

            // write the pkg header to the root directory of the pkg
            var headerPath = Path.Combine(rootDir.FullName, "pkg.json");
            if (File.Exists(headerPath)) File.Delete(headerPath);
            File.WriteAllText(headerPath, pkgHeaderStr);

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

            // remap all of the custom node paths
            RemapCustomNodeFilePaths(files, dyfDir.FullName);

            // delete all of the original dyf files
            RemoveCopiedDyfFiles(files);

            // tell handle about progress
            uploadHandle.UploadState = PackageUploadHandle.State.Compressing;

            // zip up the folder and get its path 
            var zipPath = Greg.Utility.FileUtilities.Zip(rootDir.FullName);

            var pkgUpload = new PackageUpload(  pkgHeader.name,
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
            return pkgUpload;

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
                        func.Workspace.FilePath = newPath);
                        dynSettings.CustomNodeLoader.SetNodePath(func.FunctionId, newPath);
                    });
        }

        private static void RemoveCopiedDyfFiles(IEnumerable<string> filePaths)
        {
            filePaths
                .Where(x => x.EndsWith(".dyf") && File.Exists(x))
                .ToList()
                .ForEach( File.Delete );
        }

        public static void BuildPackageVersion()
        {
            
        }





    }
}
