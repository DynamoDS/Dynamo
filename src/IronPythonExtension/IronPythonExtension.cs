using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Extensions;

namespace IronPythonExtension
{
    public class IronPythonExtension : IExtension
    {
        private const string PythonEvaluatorAssembly = "DSIronPython";

        public void Dispose()
        {
        }

        public string UniqueId => "D7B449D7-4D54-47EF-B742-30C7BEDFBE92";

        public string Name => "IronPythonExtension";

        public void Startup(StartupParams sp)
        {
            // Searches for python node libraries in package paths
            var packagePaths = sp.Preferences.CustomPackageFolders;
            string pythonFile = string.Empty;
            foreach (var path in packagePaths)
            {
                //var pkgDir = Path.GetDirectoryName(path);
                pythonFile = Directory.EnumerateFiles(path, "" + ".dll",
                    SearchOption.AllDirectories).FirstOrDefault();

                if (!string.IsNullOrEmpty(pythonFile))
                    break;
            }
            if (string.IsNullOrEmpty(pythonFile))
                return;

            var pythonDir = Path.GetDirectoryName(pythonFile);
            var libraryLoader = sp.LibraryLoader;

            // Import Python Engine into VM
            var pythonEvaluatorLib = Assembly.LoadFrom(Path.Combine(pythonDir, PythonEvaluatorAssembly + ".dll"));
            libraryLoader.LoadNodeLibrary(pythonEvaluatorLib);
        }

        public void Ready(ReadyParams sp)
        {
            throw new System.NotImplementedException();
        }

        public void Shutdown()
        {
            throw new System.NotImplementedException();
        }
    }
}
