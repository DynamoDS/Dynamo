using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Dynamo.Engine;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.PackageManager;
using Dynamo.Utilities;
using NodeDocumentationMarkdownGenerator.Verbs;

namespace NodeDocumentationMarkdownGenerator.Commands
{
    internal class FromPackageFolderCommand
    {
        private readonly LibraryServices libraryService;

        public FromPackageFolderCommand(LibraryServices libraryService)
        {
            this.libraryService = libraryService;
        }

        internal string HandlePackageDocumentation(FromPackageOptions opts)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var package = PackageFromRoot(opts.InputFolderPath);

            var nodeLibraryFileNames = ScanNodeLibraries(package, opts.HostPaths);
            var customNodeFileNames = ScanCustomNodes(package);

            var fileInfos = new List<MdFileInfo>();
            fileInfos.AddRange(nodeLibraryFileNames);
            fileInfos.AddRange(customNodeFileNames);

            var outdir = package.NodeDocumentaionDirectory;
            if (!Directory.Exists(outdir))
                Directory.CreateDirectory(outdir);

            MarkdownHandler.CreateMdFilesFromFileNames(fileInfos, outdir, opts.Overwrite);

            return $"{fileInfos.Count} documentation files created for {package.Name} package, all documentation files are available in {package.NodeDocumentaionDirectory}\n" + "(•_•)\n" + "( •_•) >⌐■-■\n" + "(⌐■_■)";
        }

        private List<MdFileInfo> ScanNodeLibraries(Package pkg, IEnumerable<string> hostPaths)
        {
            var binDlls = new DirectoryInfo(pkg.BinaryDirectory)
                .EnumerateFiles("*.dll")
                .ToList();

            var nodeLibraryPaths = binDlls
                .Where(x => pkg.Header.node_libraries.Contains(AssemblyName.GetAssemblyName(x.FullName).FullName))
                .Select(x => x.FullName)
                .ToList();

            var addtionalPathsToLoad = binDlls.Select(x => x.FullName).Except(nodeLibraryPaths).ToList();

            var hostDllPaths = hostPaths
                .SelectMany(p => new DirectoryInfo(p)
                    .EnumerateFiles("*.dll", SearchOption.AllDirectories)
                    .Select(d => d.FullName)
                    .ToList());

            addtionalPathsToLoad.AddRange(hostDllPaths);

            return AssemblyHandler.ScanAssemblies(nodeLibraryPaths, addtionalPathsToLoad);
        }

        private List<MdFileInfo> ScanCustomNodes(Package pkg)
        {
            var fileInfos = new List<MdFileInfo>();

            foreach (var path in Directory.EnumerateFiles(pkg.CustomNodeDirectory, "*.dyf"))
            {
                var fileInfo = MarkdownHandler.GetMdFileInfoFromFromCustomNode(path);
                if (fileInfo is null) continue;

                fileInfos.Add(fileInfo);
            }
            return fileInfos;
        }

        private Package PackageFromRoot(string packageFolderPath)
        {
            var log = new LogSourceBase();
            var headerPath = Path.Combine(packageFolderPath, "pkg.json");
            Package pkg = Package.FromJson(headerPath, log.AsLogger());

            return pkg;
        }
        
        //private System.Type[] GetNodeLibraries(Package pkg)
        //{
        //    AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomain_ReflectionOnlyAssemblyResolve;

        //    string[] runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll", SearchOption.AllDirectories);
        //    var dynamoDlls = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).EnumerateFiles("*.dll").Select(x => x.FullName);

        //    var paths = new List<string>(runtimeAssemblies);
        //    paths.AddRange(dynamoDlls);

        //    if (pkg is null)
        //        throw new ArgumentNullException(nameof(pkg));

        //    var list = AppDomain.CurrentDomain.GetAssemblies().OrderByDescending(a => a.FullName).Select(a => a).ToList();
        //    //Assembly.ReflectionOnlyLoad("PresentaitonFramework.dll");

        //    foreach (var loc in paths)
        //    {
        //        try
        //        {
        //            Assembly.ReflectionOnlyLoadFrom(loc);
        //        }
        //        catch (Exception)
        //        {

        //            continue;
        //        }
        //    }

        //    //list.Where(x => x.FullName.Contains("System") || x.FullName.Contains("PresentationFramework")).ToList().ForEach(a => Assembly.ReflectionOnlyLoad(a.FullName));
        //    var reflectionAsses = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies();

        //    var ss = reflectionAsses.Where(x => x.FullName.Contains("DynamoCore")).ToList();
        //    //var asses = pkg.EnumerateAssembliesInBinDirectory().Select(x => Assembly.ReflectionOnlyLoadFrom(x.Assembly.Location));

        //    var normalDomainCore = list.Where(x => x.Location.EndsWith("DynamoCore.dll")).ToList();
        //    var reflectionDomainCore = reflectionAsses.Where(x => x.Location.EndsWith("DynamoCore.dll")).ToList();

        //    var normalDomainCoretype = normalDomainCore.FirstOrDefault().DefinedTypes.Where(x => x.FullName == "Dynamo.Graph.Nodes.NodeModel").ToList();
        //    var reflectionDomainCoretype = reflectionDomainCore.FirstOrDefault().DefinedTypes.Where(x => x.FullName == "Dynamo.Graph.Nodes.NodeModel").ToList();

        //    var asses = new DirectoryInfo(pkg.BinaryDirectory).
        //        EnumerateFiles("*.dll").
        //        Select(x=> Assembly.ReflectionOnlyLoadFrom(x.FullName)).
        //        ToList();

            
        //    foreach (var lib in pkg.Header.node_libraries)
        //    {
        //        var ass = asses.
        //            Where(a => a.FullName == lib).
        //            FirstOrDefault();

        //        var assTypes = ass.GetTypes();

        //        foreach (var t in assTypes)
        //        {
        //            var NodeType = typeof(NodeModel);
        //            var reflectionType = System.Type.ReflectionOnlyGetType(NodeType.AssemblyQualifiedName, false, false);
        //            t.IsSubclassOf(reflectionType);
        //            var baseNode = t.BaseType.BaseType;
        //        }

        //        if (!NodeModelAssemblyLoader.ContainsNodeModelSubType(ass))
        //        {
        //            libraryService.LoadNodeLibrary(ass.Location, false);
        //            continue;
        //        }


        //        System.Type[] typesInAsm = null;

        //        try
        //        {
        //            typesInAsm = ass.GetTypes();
        //        }
        //        catch (ReflectionTypeLoadException ex)
        //        {
        //            typesInAsm = ex.Types;
        //        }

        //        typesInAsm.ToList().ForEach(t => GetTypeLoadData(t));

        //        // Ass.gettypes
        //        // create typeloaddata
        //    }

        //    return null;
        //}
    }
}
