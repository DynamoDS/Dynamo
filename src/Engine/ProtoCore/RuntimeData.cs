using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using ProtoCore.AssociativeGraph;
using ProtoCore.AssociativeEngine;
using ProtoCore.AST;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.BuildData;
using ProtoCore.CodeModel;
using ProtoCore.DebugServices;
using ProtoCore.DSASM;
using ProtoCore.Lang;
using ProtoCore.Lang.Replication;
using ProtoCore.Runtime;
using ProtoCore.Utils;
using ProtoFFI;

using StackFrame = ProtoCore.DSASM.StackFrame;

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

      
        private Dictionary<Guid, List<string>> uiNodeToSerializedDataMap = null;
      



 #endregion

        public RuntimeData()
        {
            
        }

      

        /// <summary>
        /// Retrieves an existing instance of a callsite associated with a UID
        /// It creates a new callsite if non was found
        /// </summary>
        /// <param name="core"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public CallSite GetCallSite(GraphNode graphNode,
                                    int classScope,
                                    string methodName,
                                    Executable executable,
                                    int runningBlock,
                                    Options options,
                                    RuntimeStatus runtimeStatus
             )
        {
            Validity.Assert(null != executable.FunctionTable);
            CallSite csInstance = null;

            // TODO Jun: Currently generates a new callsite for imperative and 
            // internally generated functions.
            // Fix the issues that cause the cache to go out of sync when 
            // attempting to cache internal functions. This may require a 
            // secondary callsite cache for internal functions so they dont 
            // clash with the graphNode UID key
            var language = executable.instrStreamList[runningBlock].language;
            bool isImperative = language == Language.kImperative;
            bool isInternalFunction = CoreUtils.IsInternalFunction(methodName);

            if (isInternalFunction || isImperative)
            {
                csInstance = new CallSite(classScope,
                                          methodName,
                                          executable.FunctionTable,
                                          options.ExecutionMode);
            }
            else if (!executable.CallsiteCache.TryGetValue(graphNode.CallsiteIdentifier, out csInstance))
            {
                // Attempt to retrieve a preloaded callsite data (optional).
                var traceData = GetAndRemoveTraceDataForNode(graphNode.guid);

                csInstance = new CallSite(classScope,
                                          methodName,
                                          executable.FunctionTable,
                                          options.ExecutionMode,
                                          traceData);

                executable.CallsiteCache[graphNode.CallsiteIdentifier] = csInstance;
                executable.CallSiteToNodeMap[csInstance.CallSiteID] = graphNode.guid;
                executable.ASTToCallSiteMap[graphNode.AstID] = csInstance;

            }

            if (graphNode != null && !CoreUtils.IsDisposeMethod(methodName))
            {
                csInstance.UpdateCallSite(classScope, methodName);
                if (options.IsDeltaExecution)
                {
                    runtimeStatus.ClearWarningForExpression(graphNode.exprUID);
                }
            }

            return csInstance;
        }


        #region Trace Data Serialization Methods/Members

        /// <summary>
        /// Call this method to obtain serialized trace data for a list of nodes.
        /// </summary>
        /// <param name="nodeGuids">A list of System.Guid of nodes whose 
        /// serialized trace data is to be retrieved. This parameter cannot be 
        /// null.</param>
        /// <returns>Returns a dictionary that maps each node Guid to its 
        /// corresponding list of serialized callsite trace data.</returns>
        /// 
        public IDictionary<Guid, List<string>>
            GetTraceDataForNodes(IEnumerable<Guid> nodeGuids, Executable executable)
        {
            if (nodeGuids == null)
                throw new ArgumentNullException("nodeGuids");

            var nodeDataPairs = new Dictionary<Guid, List<string>>();

            if (!nodeGuids.Any()) // Nothing to persist now.
                return nodeDataPairs;

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
                    return nodeDataPairs;
            }

            foreach (Guid nodeGuid in nodeGuids)
            {
                // Get a list of GraphNode objects that correspond to this node.
                var graphNodeIds = graphNodes.
                    Where(gn => gn.guid == nodeGuid).
                    Select(gn => gn.CallsiteIdentifier);

                if (!graphNodeIds.Any())
                    continue;

                // Get all callsites that match the graph node ids.
                var matchingCallSites = (from cs in executable.CallsiteCache
                                         from gn in graphNodeIds
                                         where cs.Key == gn
                                         select cs.Value);

                // Append each callsite element under node element.
                var serializedCallsites =
                    matchingCallSites.Select(callSite => callSite.GetTraceDataToSave())
                        .Where(traceDataToSave => !String.IsNullOrEmpty(traceDataToSave))
                        .ToList();

                // No point adding serialized callsite data if it's empty.
                if (serializedCallsites.Count > 0)
                    nodeDataPairs.Add(nodeGuid, serializedCallsites);
            }

            return nodeDataPairs;
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
                var matchingGraphNodes = graphNodes.
                    Where(gn => gn.guid == nodeGuid);

                if (!matchingGraphNodes.Any())
                    continue;

                // Get all callsites that match the graph node ids.
                var matchingCallSites = (from cs in executable.CallsiteCache
                                         from gn in matchingGraphNodes
                                         where string.Equals(cs.Key, gn.CallsiteIdentifier)
                                         select cs.Value);

                // Append each callsite element under node element.
                nodeMap[nodeGuid] = matchingCallSites.ToList();
            }

            return nodeMap;
        }

        /// <summary>
        /// Call this method to set the list of serialized trace data, 
        /// possibly loaded from an external storage.
        /// </summary>
        /// <param name="nodeDataPairs">A Dictionary that matches a node Guid 
        /// to its corresponding list of serialized callsite trace data.</param>
        /// 
        public void SetTraceDataForNodes(
            IEnumerable<KeyValuePair<Guid, List<string>>> nodeDataPairs)
        {
            if (nodeDataPairs == null || (nodeDataPairs.Count() <= 0))
                return; // There is no preloaded trace data.

            if (uiNodeToSerializedDataMap == null)
                uiNodeToSerializedDataMap = new Dictionary<Guid, List<string>>();

            foreach (var nodeData in nodeDataPairs)
                uiNodeToSerializedDataMap.Add(nodeData.Key, nodeData.Value);
        }

        /// <summary>
        /// Call this method to remove the trace data list for a given UI node. 
        /// This is required for the scenario where a code block node content is 
        /// modified before its corresponding callsite objects are reconstructed
        /// (i.e. before any execution takes place, and after a file-load). 
        /// Modifications on UI nodes will always result in trace data being 
        /// reconstructed again.
        /// </summary>
        /// <param name="nodeGuid">The System.Guid of the node for which trace 
        /// data is to be destroyed.</param>
        /// 
        public void DestroyLoadedTraceDataForNode(Guid nodeGuid)
        {
            // There is preloaded trace data from external file.
            if (uiNodeToSerializedDataMap != null)
            {
                if (uiNodeToSerializedDataMap.Count > 0)
                    uiNodeToSerializedDataMap.Remove(nodeGuid);
            }
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
        private string GetAndRemoveTraceDataForNode(Guid nodeGuid)
        {
            if (uiNodeToSerializedDataMap == null)
                return null; // There is no preloaded trace data.
            if (uiNodeToSerializedDataMap.Count <= 0)
                return null; // There is no preloaded trace data.

            // Get the node element for the given node.
            List<string> callsiteDataList = null;
            if (!uiNodeToSerializedDataMap.TryGetValue(nodeGuid, out callsiteDataList))
                return null;

            // There exists a node element matching the UI node's GUID, get its 
            // first child callsite element, remove it from the child node list,
            // and return it to the caller.
            // 
            string callsiteTraceData = null;
            if (callsiteDataList != null && (callsiteDataList.Count > 0))
            {
                callsiteTraceData = callsiteDataList[0];
                callsiteDataList.RemoveAt(0);
            }

            // On removal of the last callsite trace data, the node entry
            // itself will be removed from the uiNodeToSerializedDataMap.
            if (callsiteDataList != null && (callsiteDataList.Count <= 0))
                uiNodeToSerializedDataMap.Remove(nodeGuid);

            return callsiteTraceData;
        }

        #endregion // Trace Data Serialization Methods/Members

    }
}
