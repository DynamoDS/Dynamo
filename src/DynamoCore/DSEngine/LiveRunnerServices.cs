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
        private ILiveRunner liveRunner;
        private List<string> loadedLibraries;

        private LiveRunnerServices()
        {
            liveRunner = new ProtoScript.Runners.LiveRunner();
            liveRunner.GraphUpdateReady += OnGraphUpdateReady;
            liveRunner.NodeValueReady += OnNodeValueReady;
            loadedLibraries = new List<string>();

            DSLibraryServices.Instance.LibraryLoaded += new DSLibraryServices.LibraryLoadedEventHandler(OnLoadLibrary);
            liveRunner.ResetVMAndResyncGraph(DSLibraryServices.Instance.PreLoadedLibraries);
            loadedLibraries = new List<string>(DSLibraryServices.Instance.PreLoadedLibraries);
        }
      
        private void OnGraphUpdateReady(object sender, GraphUpdateReadyEventArgs e)
        {
        }

        private void OnNodeValueReady(object sender, NodeValueReadyEventArgs e)
        {
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

        public string GetStringValue(string var)
        {
            RuntimeMirror mirror = liveRunner.InspectNodeValue(var);
            return (mirror == null) ? "null" : mirror.GetStringData();
        }

        public void UpdateGraph(GraphSyncData graphData)
        {
            try
            {
                liveRunner.UpdateGraph(graphData);
            }
            catch (Exception e)
            {
                DynamoLogger.Instance.LogWarning("Update graph failed: " + e.Message, WarningLevel.Severe);
            }
        }

        private void OnLoadLibrary(object sender, DSLibraryServices.LibraryLoadedEventArgs e)
        {
            string newLibraryPath = e.LibraryPath;
            loadedLibraries.Add(newLibraryPath);
            liveRunner.ResetVMAndResyncGraph(loadedLibraries);
        }
    }
}
