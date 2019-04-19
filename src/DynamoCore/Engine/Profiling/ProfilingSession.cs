using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Core;
using Dynamo.Events;
using Dynamo.Graph.Nodes;
using ProtoCore.AST.AssociativeAST;
using VMDataBridge;

namespace Dynamo.Engine.Profiling
{
    /// <summary>
    /// This class manages a diagnostic session and the data collected in a session.
    /// </summary>
    class ProfilingSession : NotificationObject, IDisposable
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
        public IProfilingData ProfilingData
        {
            get
            {
                return profilingData;
            }
        }

        private void OnGraphPreExecution(Session.IExecutionSession session)
        {
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

        internal BinaryExpressionNode CreatePreCompilationAstNode(NodeModel node, List<AssociativeNode> inputAstNodes)
        {
            IdentifierNode identifier = AstFactory.BuildIdentifier(node.AstIdentifierBase + "_beginCallback");

            string id = node.GUID.ToString();
            ExprListNode exprListNode = AstFactory.BuildExprList(inputAstNodes);
            AssociativeNode bridgeData = DataBridge.GenerateBridgeDataAst(id, exprListNode);

            return AstFactory.BuildAssignment(identifier, bridgeData);
        }

        internal BinaryExpressionNode CreatePostCompilationAstNode(NodeModel node, List<AssociativeNode> inputAstNodes)
        {
            IdentifierNode identifier = AstFactory.BuildIdentifier(node.AstIdentifierBase + "_endCallback");

            string id = node.GUID.ToString();
            List<AssociativeNode> outPortNodeList = 
                Enumerable.Range(0, node.OutPorts.Count).Select(
                    index => (AssociativeNode)node.GetAstIdentifierForOutputIndex(index)).ToList();
            ExprListNode exprListNode = AstFactory.BuildExprList(outPortNodeList);
            AssociativeNode bridgeData = DataBridge.GenerateBridgeDataAst(id, exprListNode);

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