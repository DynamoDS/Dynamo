using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Controls;
using Dynamo.Extensions;
using Dynamo.Wpf.Extensions;

namespace PythonExtension
{
    public class PythonViewExtension : IViewExtension
    {
        private const string PythonNodeModelAssembly = "PythonNodeModels";

        private const string PythonNodeModelAssemblyWpf = "PythonNodeModelsWpf";

        private const string PythonEvaluatorAssembly = "DSIronPython";

        public void Dispose()
        {
        }

        public string UniqueId => "D7B449D7-4D54-47EF-B742-30C7BEDFBE92";

        public string Name => "PythonExtension";

        public void Startup(ViewStartupParams viewLoadedParams)
        {
        }

        public void Loaded(ViewLoadedParams viewLoadedParams)
        {
            // Searches for python node libraries in package paths
            
            var packagePaths = viewLoadedParams.ViewStartupParams.Preferences.CustomPackageFolders;
            string pythonFile = string.Empty;
            foreach (var path in packagePaths)
            {
                //var pkgDir = Path.GetDirectoryName(path);
                pythonFile = Directory.EnumerateFiles(path, PythonNodeModelAssembly + ".dll",
                    SearchOption.AllDirectories).FirstOrDefault();

                if (!string.IsNullOrEmpty(pythonFile))
                    break;
            }
            if (string.IsNullOrEmpty(pythonFile))
                return;

            var pythonDir = Path.GetDirectoryName(pythonFile);
            var libraryLoader = viewLoadedParams.ViewStartupParams.LibraryLoader;

            // Import Python Engine into VM
            var pythonEvaluatorLib = Assembly.LoadFrom(Path.Combine(pythonDir, PythonEvaluatorAssembly + ".dll"));
            libraryLoader.LoadNodeLibrary(pythonEvaluatorLib);

            // Load python node model assembly
            var pythonNodeModelLib = Assembly.LoadFrom(Path.Combine(pythonDir, PythonNodeModelAssembly + ".dll"));
            libraryLoader.LoadNodeLibrary(pythonNodeModelLib);

            var pythonNodeModelWpfLib = Assembly.LoadFrom(Path.Combine(pythonDir, PythonNodeModelAssemblyWpf + ".dll"));
            
            var dynamoView = viewLoadedParams.DynamoWindow as DynamoView;
            dynamoView.LoadNodeViewCustomizations(pythonNodeModelWpfLib);
        }

        public void Shutdown()
        {
        }
    }
}
