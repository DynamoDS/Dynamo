using System;
using System.IO;
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
            // Searches for IronPython engine binary in Dynamo folder
            var dynamoDir = Environment.CurrentDirectory;
            var libraryLoader = sp.LibraryLoader;
            Assembly pythonEvaluatorLib = null;
            try
            {
                pythonEvaluatorLib = Assembly.LoadFrom(Path.Combine(dynamoDir, PythonEvaluatorAssembly + ".dll"));
            }
            catch (Exception)
            {
                // Most likely the IronPython engine is excluded in this case
                return;
            }
            // Import IronPython Engine into VM, so Python node using IronPython engine could evaluate correctly
            libraryLoader.LoadNodeLibrary(pythonEvaluatorLib);
        }

        public void Ready(ReadyParams sp)
        {
            // Do nothing for now
        }

        public void Shutdown()
        {
            // Do nothing for now
        }
    }
}
