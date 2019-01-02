using System;
using System.Collections.Generic;
using Dynamo.Logging;
using ProtoCore.Mirror;
using ProtoScript.Runners;

namespace Dynamo.Engine
{
    internal class LiveRunnerFactory
    {
        internal static ILiveRunner CreateLiveRunner(EngineController controller, string geometryFactoryFileName)
        {
            var configuration = new LiveRunner.Configuration();
            configuration.PassThroughConfiguration.Add(Autodesk.DesignScript.Interfaces.ConfigurationKeys.GeometryFactory, geometryFactoryFileName);
            return new LiveRunner(configuration);
        }
    }

    /// <summary>
    /// This is a helper class that can get mirror data from live runner, update graph etc.
    /// </summary>
    public class LiveRunnerServices : LogSourceBase, IDisposable
    {
        private readonly ILiveRunner liveRunner;
        
        /// <summary>
        /// Creates LiveRunnerServices.
        /// </summary>
        /// <param name="controller">Engine controller</param>
        /// <param name="geometryFactoryFileName">Path to LibG</param>
        public LiveRunnerServices(EngineController controller, string geometryFactoryFileName)
        {
            liveRunner = LiveRunnerFactory.CreateLiveRunner(controller, geometryFactoryFileName);
        }

        /// <summary>
        /// Dispose LiveRunner object.
        /// </summary>
        public void Dispose()
        {
            var disposable = liveRunner as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }

        // To be superseded by runtime core
        public ProtoCore.Core Core
        {
            get
            {
                return liveRunner.Core;
            }
        }

        /// <summary>
        /// RuntimeCore of liveRunner.
        /// RuntimeCore is an object that is instantiated once across the lifecycle of the runtime.
        /// This is the entry point of the runtime VM and its input is a DS Executable format. 
        /// </summary>
        public ProtoCore.RuntimeCore RuntimeCore
        {
            get
            {
                return liveRunner.RuntimeCore;
            }
        }


        /// <summary>
        /// TPDP
        /// </summary>
        /// <param name="var">AST node id</param>
        /// <param name="verboseLogging">if set to true this enables verbose logging</param>
        /// <returns>RuntimeMirror</returns>
        internal RuntimeMirror GetMirror(string var, bool verboseLogging)
        {

            var mirror = liveRunner.InspectNodeValue(var);

            if (verboseLogging)
                Log("LRS.GetMirror var: " + var + " " + (mirror != null ? mirror.GetStringData() : "null"));

            return mirror;

        }

        /// <summary>
        /// Update graph with graph sync data.
        /// </summary>
        /// <param name="graphData"></param>
        /// <param name="verboseLogging"></param>
        internal void UpdateGraph(GraphSyncData graphData, bool verboseLogging)
        {
            if (verboseLogging)
                Log("LRS.UpdateGraph: " + graphData);

            liveRunner.UpdateGraph(graphData);
        }

        /// <summary>
        /// Preview graph with graph sync data.
        /// </summary>
        /// <param name="graphData"></param>
        /// <param name="verboseLogging"></param>
        internal List<Guid> PreviewGraph(GraphSyncData graphData, bool verboseLogging)
        {
            if (verboseLogging)
               Log("LRS.PreviewGraph: " + graphData);

            return liveRunner.PreviewGraph(graphData);
        }

        /// <summary>
        /// Returns runtime warnings for this run.
        /// </summary>
        /// <returns></returns>
        internal IDictionary<Guid, List<ProtoCore.Runtime.WarningEntry>> GetRuntimeWarnings()
        {
            return liveRunner.GetRuntimeWarnings();
        }

        /// <summary>
        /// Returns build warnings for this run.
        /// </summary>
        /// <returns></returns>
        internal IDictionary<Guid, List<ProtoCore.BuildData.WarningEntry>> GetBuildWarnings()
        {
            return liveRunner.GetBuildWarnings();
        }

        /// <summary>
        /// Returns GUIDs of exectued ASTs in this run.
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        internal IEnumerable<Guid> GetExecutedAstGuids(Guid sessionID)
        {
            return liveRunner.GetExecutedAstGuids(sessionID);
        }

        /// <summary>
        /// Remove recorded GUIDs of executed ASTs for the specified session.
        /// </summary>
        /// <param name="sessionID"></param>
        internal void RemoveRecordedAstGuidsForSession(Guid sessionID)
        {
            liveRunner.RemoveRecordedAstGuidsForSession(sessionID);
        }

        /// <summary>
        /// Each time when a new library is imported, LiveRunner need to reload
        /// all libraries and reset VM.
        /// </summary>
        /// <param name="libraries"></param>
        internal void ReloadAllLibraries(IEnumerable<string> libraries)
        { 
            liveRunner.ResetVMAndResyncGraph(libraries);
        }
    }
}
