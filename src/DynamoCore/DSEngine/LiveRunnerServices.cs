using System;
using System.Collections.Generic;
using Dynamo.Interfaces;

using ProtoCore.Mirror;
using ProtoScript.Runners;

namespace Dynamo.DSEngine
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

    public class LiveRunnerServices : LogSourceBase, IDisposable
    {
        private readonly ILiveRunner liveRunner;
        
        public LiveRunnerServices(EngineController controller, string geometryFactoryFileName)
        {
            liveRunner = LiveRunnerFactory.CreateLiveRunner(controller, geometryFactoryFileName);
        }

        public void Dispose()
        {
            var disposable = liveRunner as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }

        // To be superceeded by runtime core
        public ProtoCore.Core Core
        {
            get
            {
                return liveRunner.Core;
            }
        }

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
        /// <param name="var"></param>
        /// <param name="verboseLogging"></param>
        /// <returns></returns>
        public RuntimeMirror GetMirror(string var, bool verboseLogging)
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
        public void UpdateGraph(GraphSyncData graphData, bool verboseLogging)
        {
            if (verboseLogging)
                Log("LRS.UpdateGraph: " + graphData);

            liveRunner.UpdateGraph(graphData);
        }

        /// <summary>
        /// Preview graph with graph sync data.
        /// </summary>
        /// <param name="graphData"></param>
        public List<Guid> PreviewGraph(GraphSyncData graphData, bool verboseLogging)
        {
            if (verboseLogging)
               Log("LRS.PreviewGraph: " + graphData);

            return liveRunner.PreviewGraph(graphData);
        }

        /// <summary>
        /// Return runtime warnings for this run.
        /// </summary>
        /// <returns></returns>
        public IDictionary<Guid, List<ProtoCore.Runtime.WarningEntry>> GetRuntimeWarnings()
        {
            return liveRunner.GetRuntimeWarnings();
        }

        /// <summary>
        /// Return build warnings for this run.
        /// </summary>
        /// <returns></returns>
        public IDictionary<Guid, List<ProtoCore.BuildData.WarningEntry>> GetBuildWarnings()
        {
            return liveRunner.GetBuildWarnings();
        }

        /// <summary>
        /// Each time when a new library is imported, LiveRunner need to reload
        /// all libraries and reset VM.
        /// </summary>
        /// <param name="libraries"></param>
        public void ReloadAllLibraries(IEnumerable<string> libraries)
        { 
            liveRunner.ResetVMAndResyncGraph(libraries);
        }

        /// <summary>
        /// GraphUpdateReady event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GraphUpdateReady(object sender, GraphUpdateReadyEventArgs e)
        {
            if (EventStatus.OK == e.ResultStatus)
            {
            }
        }

        /// <summary>
        /// NodeValueReady event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NodeValueReady(object sender, NodeValueReadyEventArgs e)
        {
            if (EventStatus.OK == e.ResultStatus)
            {
            }
        }
    }
}
