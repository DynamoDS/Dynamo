using System;
using System.Collections.Generic;

using Dynamo.Models;

using ProtoCore.Mirror;
using ProtoScript.Runners;

namespace Dynamo.DSEngine
{
    internal class LiveRunnerFactory
    {
        internal static ILiveRunner CreateLiveRunner(EngineController controller, string geometryFactoryFileName)
        {
            LiveRunner.Configuration configuration = new LiveRunner.Configuration();
            configuration.PassThroughConfiguration.Add(Autodesk.DesignScript.Interfaces.ConfigurationKeys.GeometryFactory, geometryFactoryFileName);
            return new LiveRunner(configuration);
        }
    }

    public class LiveRunnerServices : IDisposable
    {
        private ILiveRunner liveRunner;
        private readonly DynamoModel dynamoModel;

        public LiveRunnerServices(DynamoModel dynamoModel, EngineController controller, string geometryFactoryFileName)
        {
            this.dynamoModel = dynamoModel;
            liveRunner = LiveRunnerFactory.CreateLiveRunner(controller, geometryFactoryFileName);
        }

        public void Dispose()
        {
            if (liveRunner is IDisposable)
                (liveRunner as IDisposable).Dispose();
        }

        public ProtoCore.Core Core
        {
            get
            {
                return liveRunner.Core;
            }
        }

        public RuntimeMirror GetMirror(string var)
        {

            var mirror = liveRunner.InspectNodeValue(var);

            if (dynamoModel.DebugSettings.VerboseLogging)
                dynamoModel.Logger.Log(/*NXLT*/"LRS.GetMirror var: " + var + " " + (mirror != null ? mirror.GetStringData() : "null"));

            return mirror;

        }

        /// <summary>
        /// Update graph with graph sync data.
        /// </summary>
        /// <param name="graphData"></param>
        public void UpdateGraph(GraphSyncData graphData)
        {
            if (dynamoModel.DebugSettings.VerboseLogging)
                dynamoModel.Logger.Log(/*NXLT*/"LRS.UpdateGraph: " + graphData);

            liveRunner.UpdateGraph(graphData);
        }

        /// <summary>
        /// Return runtime warnings for this run.
        /// </summary>
        /// <returns></returns>
        public IDictionary<Guid, List<ProtoCore.RuntimeData.WarningEntry>> GetRuntimeWarnings()
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
