using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            liveRunner.DynamoGraphUpdateReady += new DynamoGraphUpdateReadyEventHandler(OnGraphUpdateReady);
            liveRunner.DynamoNodeValueReady += new DynamoNodeValueReadyEventHandler(OnNodeValueReady);
        }
        
        private void OnGraphUpdateReady(object sender, DynamoGraphUpdateReadyEventArgs e)
        {
            /// not implemented yet.
        }

        private void OnNodeValueReady(object sender, DynamoNodeValueReadyEventArgs e)
        {
            /// not implemented yet.
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
