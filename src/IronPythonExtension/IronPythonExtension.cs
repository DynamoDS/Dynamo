﻿using System;
using System.IO;
using System.Reflection;
using Dynamo.Extensions;

namespace IronPythonExtension
{
    /// <summary>
    /// This extension does nothing but loading DSIronPython to make IronPython engine 
    /// available as one alternative Python evaluation option
    /// </summary>
    public class IronPythonExtension : IExtension
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

        /// <summary>
        /// Action to be invoked when Dynamo begins to start up. 
        /// </summary>
        /// <param name="sp"></param>
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
            if (pythonEvaluatorLib != null)
            {
                libraryLoader.LoadNodeLibrary(pythonEvaluatorLib);
            }
        }

        /// <summary>
        /// Action to be invoked when the Dynamo has started up and is ready
        /// for user interaction. 
        /// </summary>
        /// <param name="sp"></param>
        public void Ready(ReadyParams sp)
        {
            // Do nothing for now
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
