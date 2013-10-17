using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Mirror;
using ProtoScript.Runners;

namespace Dynamo.DSEngine
{
    public class LiveRunnerServices
    {
        public static LiveRunnerServices Instance = new LiveRunnerServices();

        private DynamoLogger logger = DynamoLogger.Instance;
        private ILiveRunner liveRunner;

        private LiveRunnerServices()
        {
            liveRunner = new ProtoScript.Runners.LiveRunner();
            liveRunner.DynamoGraphUpdateReady += OnGraphUpdateReady;
            liveRunner.DynamoNodeValueReady += OnNodeValueReady;

            PreLoadGeometryLibrary();
        }
      
        // temporary solution: preload assembilies in liverunner. Waiting for
        // LiveRunner to provide an API to load libraries.
        private void PreLoadGeometryLibrary()
        {
            List<string> libs = new List<string>();
            libs.Add("Math.dll");
            libs.Add("ProtoGeometry.dll");
            liveRunner.ResetVMAndResyncGraph(libs);
        }

        private void OnGraphUpdateReady(object sender, DynamoGraphUpdateReadyEventArgs e)
        {
            /// not implemented yet.
        }

        private void OnNodeValueReady(object sender, DynamoNodeValueReadyEventArgs e)
        {
            /// not implemented yet.
        }

        public ProtoCore.Core  Core
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

        public string GetStringValue(string var)
        {
            RuntimeMirror mirror = liveRunner.InspectNodeValue(var);
            return (mirror == null) ? "null" : mirror.GetStringData();
        }

        public void UpdateGraph(GraphSyncData graphData)
        {
            try
            {
                // async BeginUpdateGraph() hasn't been implemented yet.
                // liveRunner.BeginUpdateGraph(graphData);
                liveRunner.UpdateGraph(graphData);
            }
            catch (Exception e)
            {
                logger.Log("Update graph failed: " + e.Message);
            }
        }
    }
}
