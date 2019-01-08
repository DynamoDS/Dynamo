using System;
using System.Collections.Generic;
using System.Linq;
using ProtoCore.AssociativeGraph;
using ProtoCore.DSASM;
using ProtoCore.Utils;

namespace ProtoCore
{
    /// <summary>
    /// The RuntimeData is an object that contains properties that is consumed only by the runtime VM
    /// It is instantiated prior to execution and is populated with information gathered from the CompileCore
    /// 
    /// The runtime VM is designed to run independently from the front-end (UI, compiler) 
    /// and the only 2 properties it needs are the RuntimeData and the DSExecutable.
    /// 
    /// The RuntimeData will also contain properties that are populated at runtime and consumed at runtime.
    /// </summary>
    public class RuntimeData
    {

#region COMPILER_GENERATED_READ_ONLY
        // COMPILER_GENERATED_READ_ONLY are properties generated at the compilation phase. 
        // Once generated these properties are consumed by the runtime-VM and are read-only

      
        private Dictionary<Guid, List<CallSite.RawTraceData>> uiNodeToSerializedDataMap = null;
        public IDictionary<string, CallSite> CallsiteCache { get; set; }
        /// <summary>		
        /// Map from a callsite's guid to a graph UI node. 		
        /// </summary>
        public Dictionary<Guid, Guid> CallSiteToNodeMap { get; private set; }
 		 
 #endregion

        public RuntimeData()
        {
            CallsiteCache = new Dictionary<string, CallSite>();
            CallSiteToNodeMap = new Dictionary<Guid, Guid>();
        }

      

        /// <summary>
        /// Retrieves an existing instance of a callsite associated with a UID
        /// It creates a new callsite if non was found
        /// </summary>
        /// <param name="core"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public CallSite GetCallSite(int classScope, string methodName, Executable executable, RuntimeCore runtimeCore)
        {
            Validity.Assert(null != executable.FunctionTable);
            CallSite csInstance = null;
            var graphNode = executable.ExecutingGraphnode;
            var topGraphNode = graphNode;

            // If it is a nested function call, append all callsite ids
            List<string> callsiteIdentifiers = new List<string>();
            foreach (var prop in runtimeCore.InterpreterProps)
            {
                if (prop != null && prop.executingGraphNode != null && graphNode != prop.executingGraphNode)
                {
                    topGraphNode = prop.executingGraphNode;
                    if (!string.IsNullOrEmpty(topGraphNode.CallsiteIdentifier))
                    {
                        callsiteIdentifiers.Add(topGraphNode.CallsiteIdentifier);
                    }
                }
            }
            if (graphNode != null)
            {
                callsiteIdentifiers.Add(graphNode.CallsiteIdentifier);
            }
            var callsiteID = string.Join(";", callsiteIdentifiers.ToArray());

            // TODO Jun: Currently generates a new callsite for imperative and 
            // internally generated functions.
            // Fix the issues that cause the cache to go out of sync when 
            // attempting to cache internal functions. This may require a 
            // secondary callsite cache for internal functions so they dont 
            // clash with the graphNode UID key
            var language = executable.instrStreamList[runtimeCore.RunningBlock].language;
            bool isImperative = language == Language.Imperative;
            bool isInternalFunction = CoreUtils.IsInternalFunction(methodName);

            if (isInternalFunction || isImperative)
            {
                csInstance = new CallSite(classScope,
                                          methodName,
                                          executable.FunctionTable,
                                          runtimeCore.Options.ExecutionMode);
            }
            else if (!CallsiteCache.TryGetValue(callsiteID, out csInstance))
            {
                // Attempt to retrieve a preloaded callsite data (optional).
                var traceData = GetAndRemoveTraceDataForNode(topGraphNode.guid, callsiteID);

                csInstance = new CallSite(classScope,
                                          methodName,
                                          executable.FunctionTable,
                                          runtimeCore.Options.ExecutionMode,
                                          traceData);

                CallsiteCache[callsiteID] = csInstance;
                CallSiteToNodeMap[csInstance.CallSiteID] = topGraphNode.guid;
            }

            if (graphNode != null && !CoreUtils.IsDisposeMethod(methodName))
            {
                csInstance.UpdateCallSite(classScope, methodName);
                if (runtimeCore.Options.IsDeltaExecution)
                {
                    runtimeCore.RuntimeStatus.ClearWarningForExpression(graphNode.exprUID);
                }
            }

            return csInstance;
        }

        public Dictionary<Guid, List<CallSite>>
        GetCallsitesForNodes(IEnumerable<Guid> nodeGuids, Executable executable)
        {
            if (nodeGuids == null)
                throw new ArgumentNullException("nodeGuids");

            var nodeMap = new Dictionary<Guid, List<CallSite>>();

            if (!nodeGuids.Any()) // Nothing to persist now.
                return nodeMap;

            // Attempt to get the list of graph node if one exists.
            IEnumerable<GraphNode> graphNodes = null;
            {
                if (executable != null)
                {
                    var stream = executable.instrStreamList;
                    if (stream != null && (stream.Length > 0))
                    {
                        var graph = stream[0].dependencyGraph;
                        if (graph != null)
                            graphNodes = graph.GraphList;
                    }
                }


                if (graphNodes == null) // No execution has taken place.
                    return nodeMap;
            }

            foreach (Guid nodeGuid in nodeGuids)
            {
                // Get a list of GraphNode objects that correspond to this node.
                var matchingGraphNodes = graphNodes.Where(gn => gn.guid == nodeGuid);

                if (!matchingGraphNodes.Any())
                    continue;

                // Get all callsites that match the graph node ids.
                var matchingCallSites = (from cs in CallsiteCache
                                         from gn in matchingGraphNodes
                                         where string.Equals(cs.Key, gn.CallsiteIdentifier)
                                         select cs.Value);

                // Append each callsite element under node element.
                nodeMap[nodeGuid] = matchingCallSites.ToList();
            }

            return nodeMap;
        }

        #region Trace Data Serialization Methods/Members

        private IEnumerable<GraphNode> GetGraphNodeData(Executable executable)
        {
            // No executable, nothing to return
            if (executable == null)
              return null;

            // No stream data to query
            var stream = executable.instrStreamList;
            if (stream == null || stream.Length == 0)
              return null;

            // Get the list of graph node if one exists
            var graph = stream[0].dependencyGraph;
            if (graph == null)
              return null;

            return graph.GraphList;
        }

        /// <summary>
        /// Call this method to obtain serialized trace data for a list of nodes.
        /// </summary>
        /// <param name="nodeGuids">A list of System.Guid of nodes whose 
        /// serialized trace data is to be retrieved. This parameter cannot be 
        /// null.</param>
        /// <param name="executable">A container of callsite data for the nodes in the graph.</param>
        /// <returns>Returns a dictionary that maps each node Guid to its 
        /// corresponding list of serialized callsite trace data.</returns>
        public IDictionary<Guid, List<CallSite.RawTraceData>>
                GetTraceDataForNodes(IEnumerable<Guid> nodeGuids, Executable executable)
        {
            if (nodeGuids == null)
                throw new ArgumentNullException("nodeGuids");

            var nodeDataPairs = new Dictionary<Guid, List<CallSite.RawTraceData>>();

            // Nothing to persist now
            if (!nodeGuids.Any())
                return nodeDataPairs;

            IEnumerable<GraphNode> graphNodeData = GetGraphNodeData(executable);
            if (graphNodeData == null)
              return nodeDataPairs;

            Dictionary<Guid, List<string>> callsiteMap = new Dictionary<Guid, List<string>>();
            foreach (GraphNode graphNode in graphNodeData)
            {
                Guid guid = graphNode.guid;

                List<string> callsSiteData;
                if (!callsiteMap.TryGetValue(guid, out callsSiteData))
                {
                    callsSiteData = new List<string>();
                    callsiteMap[guid] = callsSiteData;
                }

                callsSiteData.Add(graphNode.CallsiteIdentifier);
            }

            foreach (Guid nodeGuid in nodeGuids)
            {
                List<string> graphNodeIds;
                if (!callsiteMap.TryGetValue(nodeGuid, out graphNodeIds))
                  continue;

                // Get all callsites that match the graph node ids.
                // 
                // Note we assume the graph node is the top-level graph node here.
                // The callsite id is the concatenation of all graphnodes' callsite
                // identifier along the nested function call. 
                var matchingCallSites = (from cs in CallsiteCache
                                         from gn in graphNodeIds
                                         where !string.IsNullOrEmpty(gn) && cs.Key.StartsWith(gn)
                                         select new { cs.Key, cs.Value });

                // Append each callsite element under node element.
                var serializedCallsites = new List<CallSite.RawTraceData>();
                foreach (var site in matchingCallSites)
                {
                    var traceData = site.Value.GetTraceDataToSave();
                    if (!string.IsNullOrEmpty(traceData))
                    {
                        serializedCallsites.Add(new CallSite.RawTraceData(site.Key, traceData));
                    }
                }

                // No point adding serialized callsite data if it's empty.
                if (serializedCallsites.Any())
                {
                    nodeDataPairs.Add(nodeGuid, serializedCallsites);
                }
            }

            return nodeDataPairs;
        }

        /// <summary>
        /// Call this method to set the list of serialized trace data, 
        /// possibly loaded from an external storage.
        /// </summary>
        /// <param name="nodeDataPairs">A Dictionary that matches a node Guid 
        /// to its corresponding list of serialized callsite trace data.</param>
        /// 
        public void SetTraceDataForNodes(
            IEnumerable<KeyValuePair<Guid, List<CallSite.RawTraceData>>> nodeDataPairs)
        {
            if (nodeDataPairs == null || (nodeDataPairs.Count() <= 0))
                return; // There is no preloaded trace data.

            if (uiNodeToSerializedDataMap == null)
                uiNodeToSerializedDataMap = new Dictionary<Guid, List<CallSite.RawTraceData>>();

            foreach (var nodeData in nodeDataPairs)
                uiNodeToSerializedDataMap.Add(nodeData.Key, nodeData.Value);
        }

        /// <summary>
        /// Call this method to pop the top-most serialized callsite trace data.
        /// Note that this call only pops off a signle callsite trace data 
        /// belonging to a given UI node denoted by the given node guid.
        /// </summary>
        /// <param name="nodeGuid">The Guid of a given UI node whose top-most 
        /// callsite trace data is to be retrieved and removed.</param>
        /// <returns>Returns the serialized callsite trace data in Base64 encoded
        /// string for the given UI node.</returns>
        /// 
        private string GetAndRemoveTraceDataForNode(Guid nodeGuid, string callsiteID)
        {
            if (uiNodeToSerializedDataMap == null)
                return null; // There is no preloaded trace data.
            if (uiNodeToSerializedDataMap.Count <= 0)
                return null; // There is no preloaded trace data.

            // Get the node element for the given node.
            List<CallSite.RawTraceData> callsiteDataList = null;
            if (!uiNodeToSerializedDataMap.TryGetValue(nodeGuid, out callsiteDataList))
            {
                return null;
            }

            // There exists a node element matching the UI node's GUID, get its 
            // first child callsite element, remove it from the child node list,
            // and return it to the caller.
            // 
            string callsiteTraceData = null;
            if (callsiteDataList != null)
            {
                for (int i = 0; i < callsiteDataList.Count; i++)
                {
                    if (callsiteDataList[i].ID == callsiteID)
                    {
                        callsiteTraceData = callsiteDataList[i].Data;
                        callsiteDataList.RemoveAt(i);
                        // Remove the trace data
                        if (callsiteDataList.Any())
                        {
                            uiNodeToSerializedDataMap[nodeGuid] = callsiteDataList;
                        }
                        else
                        {
                            uiNodeToSerializedDataMap.Remove(nodeGuid);
                        }
                        break;
                    }
                }
            }

            // For backword compatibility: old dyn file doesn't have CallSiteID
            // attribute, so the call site id will be empty string.
            if (callsiteTraceData == null && !string.IsNullOrEmpty(callsiteID))
            {
                return GetAndRemoveTraceDataForNode(nodeGuid, string.Empty);
            }
            else
            {
                return callsiteTraceData;
            }
        }

        #endregion // Trace Data Serialization Methods/Members

    }
}
