﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Mirror;
using ProtoScript.Runners;

namespace Dynamo.DSEngine
{
    internal class LiveRunnerFactory
    {
        internal static ILiveRunner CreateLiveRunner(EngineController controller)
        {
            LiveRunner.Options option = new LiveRunner.Options();
            return new LiveRunner(option);
        }
    }

    public class LiveRunnerServices
    {
        private ILiveRunner liveRunner;
        private EngineController controller;

        public LiveRunnerServices(EngineController controller)
        {
            this.controller = controller;
            liveRunner = LiveRunnerFactory.CreateLiveRunner(controller);

            liveRunner.GraphUpdateReady += GraphUpdateReady;
            liveRunner.NodeValueReady += NodeValueReady;
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
            return liveRunner.InspectNodeValue(var);
        }

        /// <summary>
        /// Update graph with graph sync data.
        /// </summary>
        /// <param name="graphData"></param>
        public void UpdateGraph(GraphSyncData graphData)
        {
            liveRunner.UpdateGraph(graphData);
        }

        /// <summary>
        /// Each time when a new library is imported, LiveRunner need to reload
        /// all libraries and reset VM.
        /// </summary>
        /// <param name="libraries"></param>
        public void ReloadAllLibraries(List<string> libraries)
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
