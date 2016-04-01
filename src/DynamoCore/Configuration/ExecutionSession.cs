using System;
using System.Collections.Generic;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Session;

namespace Dynamo.Configuration
{
    class ExecutionSession : IExecutionSession
    {
        private IPathManager pathManager;
        private Dictionary<string, object> parameters = new Dictionary<string,object>();
        
        /// <summary>
        /// Constructs a new execution session.
        /// </summary>
        public ExecutionSession(Scheduler.UpdateGraphAsyncTask updateTask, DynamoModel model, string geometryFactoryPath)
        {
            CurrentWorkspacePath = updateTask.TargetedWorkspace.FileName;
            pathManager = model.PathManager;
            parameters[ParameterKeys.GeometryFactory] = geometryFactoryPath;
            parameters[ParameterKeys.MajorVersion] = pathManager.MajorFileVersion;
            parameters[ParameterKeys.MinorVersion] = pathManager.MinorFileVersion;
            parameters[ParameterKeys.NumberFormat] = model.PreferenceSettings.NumberFormat;
        }

        /// <summary>
        /// File path for the current workspace in execution. Could be null or
        /// empty string if workspace is not yet saved.
        /// </summary>
        public string CurrentWorkspacePath { get; private set; }

        /// <summary>
        /// Gets session parameter value for the given parameter name.
        /// </summary>
        /// <param name="parameter">Name of session parameter</param>
        /// <returns>Session parameter value as object</returns>
        public object GetParameterValue(string parameter)
        {
            return parameters[parameter];
        }

        /// <summary>
        /// Gets list of session parameter keys available in the session.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetParameterKeys()
        {
            return parameters.Keys;
        }

        /// <summary>
        /// A helper method to resolve the given file path. The given file path
        /// will be resolved by searching into the current workspace, packages and
        /// definitions folder, core and host application installation folders etc.
        /// </summary>
        /// <param name="filepath">Input file path</param>
        /// <returns>True if the file is found</returns>
        public bool ResolveFilePath(ref string filepath)
        {
            throw new NotImplementedException();
        }
    }
}
