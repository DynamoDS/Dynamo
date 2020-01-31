using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        private static readonly string identifierPattern = @"([a-zA-Z-_@0-9]+)";
        private static readonly string indexPattern = @"([-+]?\d+)";
        private static readonly string callsiteIDPattern =
            identifierPattern +
            Constants.kInClassDecl + indexPattern +
            Constants.kSingleUnderscore + Constants.kInFunctionScope + indexPattern +
            Constants.kSingleUnderscore + Constants.kInstance + indexPattern +
            identifierPattern;
        private static readonly string joinPattern = ';' + callsiteIDPattern;
        private static readonly string fullCallsiteID = callsiteIDPattern + string.Format("({0})*", joinPattern);

        /// <summary>
        /// Map from callsite id to callsite.
        /// </summary>
        public IDictionary<string, CallSite> CallsiteCache { get; set; }
        /// <summary>		
        /// Map from a callsite's guid to a graph UI node. 		
        /// </summary>
        public Dictionary<Guid, Guid> CallSiteToNodeMap { get; }
        /// <summary>		
        /// Map from a graph UI node to callsite identifiers. 		
        /// </summary>
        internal Dictionary<Guid, List<CallSite>> NodeToCallsiteObjectMap { get; }
        
#endregion

        public RuntimeData()
        {
            CallsiteCache = new Dictionary<string, CallSite>();
            CallSiteToNodeMap = new Dictionary<Guid, Guid>();
            NodeToCallsiteObjectMap = new Dictionary<Guid, List<CallSite>>();
        }


        /// <summary>
        /// Retrieves an existing instance of a callsite associated with a UID
        /// It creates a new callsite if non was found
        /// </summary>
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
                List<CallSite> callsites;
                if (NodeToCallsiteObjectMap.TryGetValue(topGraphNode.guid, out callsites))
                {
                    callsites.Add(csInstance);
                }
                else
                {
                    NodeToCallsiteObjectMap[topGraphNode.guid] = new List<CallSite>() { csInstance };
                }
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

        /// <summary>
        /// This API is used by host integrations such as for Revit and C3D.
        /// It is used to gets the trace data list for all nodes binding to elements in the host.
        /// </summary>
        /// <param name="nodeGuids"></param>
        /// <param name="executable"></param>
        /// <returns></returns>
        public Dictionary<Guid, List<CallSite>> GetCallsitesForNodes(
            IEnumerable<Guid> nodeGuids, Executable executable)
        {
            if (nodeGuids == null)
                throw new ArgumentNullException("nodeGuids");

            var nodeMap = new Dictionary<Guid, List<CallSite>>();

            if (!nodeGuids.Any()) // Nothing to persist now.
                return nodeMap;

            List<CallSite> callsites;
            foreach (Guid nodeGuid in nodeGuids)
            {
                if (NodeToCallsiteObjectMap.TryGetValue(nodeGuid, out callsites))
                {
                    nodeMap[nodeGuid] = callsites;
                }
                else
                {
                    nodeMap[nodeGuid] = new List<CallSite>();
                }
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
        /// Note that this call only pops off a single callsite trace data 
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
                    if (DoCallSiteIDsMatch(callsiteID, callsiteDataList[i].ID))
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

            return callsiteTraceData;
        }

        private static bool DoCallSiteIDsMatch(string compilerGenerated, string deserialized)
        {
            if (compilerGenerated == deserialized) return true;

            var matches1 = Regex.Match(compilerGenerated, fullCallsiteID);
            var matches2 = Regex.Match(deserialized, fullCallsiteID);

            if (matches1.Groups.Count != matches2.Groups.Count) return false;

            // If both group counts match, they should number 12 in all.
            // We should ignore checking for the 1st, 7th, and 10th group specifically
            // as per the Regex pattern (for fullCallsiteID) since that group includes the function scope
            // that can vary for custom nodes or DS functions that make nested calls to
            // host element creation methods.
            //Groups
            //0: full string
            //1: function id
            //2: global class index
            //3: global function
            //4: function call id
            //5: outer node instance guid
            //6: name,global class index, funcscope,instance,guid,
            //7: name,
            //8: global class index,
            //9: function scope,
            //10: instance,
            //11: node instance guid
            for (int i = 0; i < matches1.Groups.Count; i++)
            {
                if (i == 0 || i == 6 || i == 9) continue;

                if (matches1.Groups[i].Value != matches2.Groups[i].Value) return false;
            }

            return true;
        }

        #endregion // Trace Data Serialization Methods/Members

    }
}
