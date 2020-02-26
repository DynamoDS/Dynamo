using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Events;
using Dynamo.Graph.Nodes;
using ProtoCore.AST.AssociativeAST;
using VMDataBridge;

namespace Dynamo.Engine.Profiling
{
    /// <summary>
    /// This class manages a diagnostic session and the data collected in a session.
    /// </summary>
    class ProfilingSession : IDisposable
    {
        private ProfilingData profilingData;

        /// <summary>
        /// Creeate a diagnostic session for collecting profiling data.
        /// </summary>
        public ProfilingSession()
        {
            RegisterEventHandlers();
            profilingData = new ProfilingData();
        }

        /// <summary>
        /// Ensure that the event handlers are disconnected when the profiling session is terminated.
        /// </summary>
        public void Dispose()
        {
            UnregisterEventHandlers();
        }

        /// <summary>
        /// Return an interface to the profiling data for this profiling session.
        /// </summary>
        public IProfilingExecutionTimeData ProfilingData
        {
            get
            {
                return profilingData;
            }
        }

        private void OnGraphPreExecution(Session.IExecutionSession session)
        {
            profilingData.Reset();
            profilingData.StartTime = DateTime.Now;
        }

        private void OnGraphPostExecution(Session.IExecutionSession session)
        {
            profilingData.EndTime = DateTime.Now;
        }

        internal void RegisterNode(NodeModel node)
        {
            profilingData.RegisterNode(node);
        }

        internal void UnregisterDeletedNodes(IEnumerable<NodeModel> nodes)
        {
            profilingData.UnregisterDeletedNodes(nodes);
        }

        private const string beginTag = "_beginCallback";
        private const string endTag = "_endCallback";
        public const string profilingID = "_dynamo_profiling";

        internal BinaryExpressionNode CreatePreCompilationAstNode(NodeModel node, List<AssociativeNode> inputAstNodes)
        {
            IdentifierNode identifier = AstFactory.BuildIdentifier(node.AstIdentifierBase + beginTag);

            string id = node.GUID.ToString();
            ExprListNode exprListNode = AstFactory.BuildExprList(inputAstNodes);
            AssociativeNode bridgeData = DataBridge.GenerateBridgeDataAst(id+ profilingID, exprListNode);

            return AstFactory.BuildAssignment(identifier, bridgeData);
        }

        internal BinaryExpressionNode CreatePostCompilationAstNode(NodeModel node, List<AssociativeNode> inputAstNodes)
        {
            IdentifierNode identifier = AstFactory.BuildIdentifier(node.AstIdentifierBase + endTag);

            string id = node.GUID.ToString();
            List<AssociativeNode> outPortNodeList = 
                Enumerable.Range(0, node.OutPorts.Count).Select(
                    index => (AssociativeNode)node.GetAstIdentifierForOutputIndex(index)).ToList();
            ExprListNode exprListNode = AstFactory.BuildExprList(outPortNodeList);
            AssociativeNode bridgeData = DataBridge.GenerateBridgeDataAst(id+ profilingID, exprListNode);

            return AstFactory.BuildAssignment(identifier, bridgeData);
        }

        private void RegisterEventHandlers()
        {
            ExecutionEvents.GraphPreExecution += OnGraphPreExecution;
            ExecutionEvents.GraphPostExecution += OnGraphPostExecution;
        }

        private void UnregisterEventHandlers()
        {
            ExecutionEvents.GraphPreExecution -= OnGraphPreExecution;
            ExecutionEvents.GraphPostExecution -= OnGraphPostExecution;
        }
    }
}