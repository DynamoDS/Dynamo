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
            liveRunner.DynamoGraphUpdateReady += new DynamoGraphUpdateReadyEventHandler(OnGraphUpdateReady);
            liveRunner.DynamoNodeValueReady += new DynamoNodeValueReadyEventHandler(OnNodeValueReady);

            PreLoadGeometryLibrary();
        }
      
        // temporary solution: preload assembilies in liverunner. Waiting for
        // LiveRunner to provide an API to load libraries.
        private void PreLoadGeometryLibrary()
        {
            ImportNode import1 = new ImportNode { ModuleName = "Math.dll" };
            ImportNode import2 = new ImportNode { ModuleName = "ProtoGeometry.dll" };

            Subtree tree = new Subtree();
            tree.GUID = System.Guid.NewGuid();
            tree.AstNodes = new List<AssociativeNode> {import1, import2};
            GraphSyncData synData = new GraphSyncData(null, new List<Subtree> { tree }, null);
            liveRunner.UpdateGraph(synData);
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
