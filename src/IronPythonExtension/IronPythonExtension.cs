using System;
using System.IO;
using System.Reflection;
using Dynamo.Extensions;
using Dynamo.Logging;
using Dynamo.PythonServices;

namespace IronPythonExtension
{
    /// <summary>
    /// This extension does nothing but loading DSIronPython to make IronPython engine 
    /// available as one alternative Python evaluation option
    /// </summary>
    public class IronPythonExtension : IExtension, ILogSource
    {
        private const string PythonEvaluatorAssembly = "DSIronPython";

        /// <summary>
        /// Dispose function
        /// </summary>
        public void Dispose()
        {
            // Do nothing for now
        }

        /// <summary>
        /// Extension unique GUID
        /// </summary>
        public string UniqueId => "D7B449D7-4D54-47EF-B742-30C7BEDFBE92";

        /// <summary>
        /// Extension name
        /// </summary>
        public string Name => "IronPythonExtension";

        #region ILogSource

        public event Action<ILogMessage> MessageLogged;
        internal void OnMessageLogged(ILogMessage msg)
        {
            if (this.MessageLogged != null)
            {
                MessageLogged?.Invoke(msg);
            }
        }
        #endregion

        /// <summary>
        /// Action to be invoked when Dynamo begins to start up. 
        /// </summary>
        /// <param name="sp"></param>
        public void Startup(StartupParams sp)
        {
            // Do nothing for now
        }

        /// <summary>
        /// Action to be invoked when the Dynamo has started up and is ready
        /// for user interaction. 
        /// </summary>
        /// <param name="sp"></param>
        public void Ready(ReadyParams sp)
        {
            // Searches for DSIronPython engine binary in same folder with extension itself
            var targetDir = Path.GetDirectoryName(Assembly.GetAssembly(typeof(IronPythonExtension)).Location);
            var libraryLoader = sp.StartupParams.LibraryLoader;
            Assembly pythonEvaluatorLib = null;
            try
            {
                pythonEvaluatorLib = Assembly.LoadFrom(Path.Combine(targetDir, PythonEvaluatorAssembly + ".dll"));
            }
            catch (Exception ex)
            {
                // Most likely the IronPython engine is excluded in this case
                // but logging the exception message in case for diagnose
                OnMessageLogged(LogMessage.Info(ex.Message));
                return;
            }
            // Import IronPython Engine into VM, so Python node using IronPython engine could evaluate correctly
            if (pythonEvaluatorLib != null)
            {
                libraryLoader.LoadNodeLibrary(pythonEvaluatorLib);
            }
        }

        /// <summary>
        /// Action to be invoked when shutdown has begun.
        /// </summary>
        public void Shutdown()
        {
            // Do nothing for now
        }
    }
}
