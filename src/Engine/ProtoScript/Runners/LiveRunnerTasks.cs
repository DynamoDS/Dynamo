using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GraphToDSCompiler;
using ProtoCore.AssociativeGraph;
using ProtoCore.Utils;

namespace ProtoScript.Runners
{
    public partial class LiveRunner
    {
        private abstract class Task
        {
            protected LiveRunner runner;
            protected Task(LiveRunner runner)
            {
                this.runner = runner;
            }
            public abstract void Execute();
        }

        private class NodeValueRequestTask : Task
        {
            private Guid nodeGuid;
            public NodeValueRequestTask(Guid nodeGuid, LiveRunner runner)
                : base(runner)
            {
                this.nodeGuid = nodeGuid;
            }

            public override void Execute()
            {
                throw new NotImplementedException();
            }
        }

        private class UpdateGraphTask : Task
        {
            private GraphSyncData syncData;
            public UpdateGraphTask(GraphSyncData syncData, LiveRunner runner)
                : base(runner)
            {
                this.syncData = syncData;
            }

            public override void Execute()
            {
                throw new NotImplementedException();
            }
        }

        private class ConvertNodesToCodeTask : Task
        {
            private List<Subtree> subtrees;
            public ConvertNodesToCodeTask(List<Subtree> subtrees, LiveRunner runner)
                : base(runner)
            {
                this.subtrees = subtrees;
            }

            public override void Execute()
            {
                NodesToCodeCompletedEventArgs args = null;
                List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

                if (subtrees != null)
                {
                    foreach (var tree in subtrees)
                    {
                        Validity.Assert(tree.AstNodes != null && tree.AstNodes.Count > 0);
                        astList.AddRange(tree.AstNodes);
                    }
                }

                lock (runner.operationsMutex)
                {
                    try
                    {
                        string code = GraphUtilities.ASTListToCode(astList);
                        args = new NodesToCodeCompletedEventArgs(code, EventStatus.OK, "Node to code task complete.");
                    }
                    catch (Exception exception)
                    {
                        args = new NodesToCodeCompletedEventArgs(string.Empty, EventStatus.Error, exception.Message);
                    }
                }

                // Notify the listener
                if (null != runner.NodesToCodeCompleted)
                {
                    runner.NodesToCodeCompleted(this, args);
                }
            }
        }

        private class PropertyChangedTask : Task
        {
            public PropertyChangedTask(LiveRunner runner, GraphNode graphNode)
                : base(runner)
            {
                objectCreationGraphNode = graphNode;
            }

            public override void Execute()
            {
                throw new NotImplementedException();
            }

            public GraphNode objectCreationGraphNode { get; set; }
        }

        private class UpdateCmdLineInterpreterTask : Task
        {
            private string cmdLineString;
            public UpdateCmdLineInterpreterTask(string code, LiveRunner runner)
                : base(runner)
            {
                this.cmdLineString = code;
            }

            public override void Execute()
            {
                throw new NotImplementedException();
            }
        }
    }

    namespace Obsolete
    {
        public partial class LiveRunner
        {
            private abstract class Task
            {
                protected LiveRunner runner;
                protected Task(LiveRunner runner)
                {
                    this.runner = runner;
                }
                public abstract void Execute();
            }

            private class NodeValueRequestTask : Task
            {
                private uint nodeId;
                public NodeValueRequestTask(uint nodeId, LiveRunner runner)
                    : base(runner)
                {
                    this.nodeId = nodeId;
                }

                public override void Execute()
                {
                    lock (runner.operationsMutex)
                    {
                        if (runner.NodeValueReady != null) // If an event handler is registered.
                        {
                            NodeValueReadyEventArgs retArgs = null;

                            try
                            {
                                // Now that background worker is free, get the value...
                                ProtoCore.Mirror.RuntimeMirror mirror = runner.InternalGetNodeValue(nodeId);
                                if (null != mirror)
                                {
                                    retArgs = new NodeValueReadyEventArgs(mirror, nodeId);

                                    System.Diagnostics.Debug.WriteLine("Node {0} => {1}",
                                        nodeId.ToString("x8"), mirror.GetData().GetStackValue());
                                }
                                else
                                {
                                    retArgs = new NodeValueNotAvailableEventArgs(nodeId);
                                }

                            }
                            catch (Exception e)
                            {
                                retArgs = new NodeValueReadyEventArgs(null, nodeId, EventStatus.Error, e.ToString());
                            }

                            if (null != retArgs)
                            {
                                runner.NodeValueReady(this, retArgs); // Notify all listeners (e.g. UI).
                            }
                        }
                    }
                }
            }

            private class PropertyChangedTask : Task
            {
                public PropertyChangedTask(LiveRunner runner, GraphNode graphNode)
                    : base(runner)
                {
                    objectCreationGraphNode = graphNode;
                }

                public override void Execute()
                {
                    if (!objectCreationGraphNode.propertyChanged)
                    {
                        return;
                    }
                    objectCreationGraphNode.propertyChanged = false;
                    UpdateNodeRef updateNode = objectCreationGraphNode.updateNodeRefList[0];

                    GraphUpdateReadyEventArgs retArgs = null;
                    lock (runner.operationsMutex)
                    {
                        try
                        {
                            // @keyu: graph nodes may have been recreated caused of
                            // some update on the UI so that we have to find out
                            // new graph code that create this ffi object.
                            var graph = runner.runnerCore.DSExecutable.instrStreamList[0].dependencyGraph;
                            var graphnodes = graph.GetGraphNodesAtScope(this.objectCreationGraphNode.classIndex, this.objectCreationGraphNode.procIndex);
                            foreach (var graphnode in graphnodes)
                            {
                                if ((graphnode == objectCreationGraphNode) ||
                                    (graphnode.updateNodeRefList.Count == 1 &&
                                     updateNode.IsEqual(graphnode.updateNodeRefList[0])))
                                {
                                    graphnode.propertyChanged = true;
                                    break;
                                }
                            }

                            runner.ResetVMForExecution();
                            runner.Execute();

                            var modfiedGuidList = runner.GetModifiedGuidList();
                            runner.ResetModifiedSymbols();
                            var syncDataReturn = runner.CreateSynchronizeDataForGuidList(modfiedGuidList);
                            retArgs = new GraphUpdateReadyEventArgs(syncDataReturn);
                        }
                        catch (Exception e)
                        {
                            retArgs = new GraphUpdateReadyEventArgs(new SynchronizeData(),
                                                                    EventStatus.Error,
                                                                    e.Message);
                        }
                    }

                    if (runner.GraphUpdateReady != null)
                    {
                        runner.GraphUpdateReady(this, retArgs);
                    }
                }

                public GraphNode objectCreationGraphNode { get; set; }
            }

            private class UpdateGraphTask : Task
            {
                private SynchronizeData syncData;
                public UpdateGraphTask(SynchronizeData syncData, LiveRunner runner)
                    : base(runner)
                {
                    this.syncData = syncData;

                }

                public override void Execute()
                {
                    GraphUpdateReadyEventArgs retArgs;

                    lock (runner.operationsMutex)
                    {
                        try
                        {
                            string code = null;
                            runner.SynchronizeInternal(syncData, out code);

                            var modfiedGuidList = runner.GetModifiedGuidList();
                            runner.ResetModifiedSymbols();
                            var syncDataReturn = runner.CreateSynchronizeDataForGuidList(modfiedGuidList);

                            retArgs = null;

                            ReportErrors(code, syncDataReturn, modfiedGuidList, ref retArgs);

                            //ReportBuildErrorsAndWarnings(code, syncDataReturn, modfiedGuidList, ref retArgs);
                            //ReportRuntimeWarnings(code, syncDataReturn, modfiedGuidList, ref retArgs);

                            if (retArgs == null)
                                retArgs = new GraphUpdateReadyEventArgs(syncDataReturn);
                        }
                        // Any exceptions that are caught here are most likely from the graph compiler
                        catch (Exception e)
                        {
                            retArgs = new GraphUpdateReadyEventArgs(syncData, EventStatus.Error, e.Message);
                        }
                    }

                    if (runner.GraphUpdateReady != null)
                    {
                        runner.GraphUpdateReady(this, retArgs); // Notify all listeners (e.g. UI).
                    }
                }

                private void ReportErrors(string code, SynchronizeData syncDataReturn, Dictionary<uint, string> modifiedGuidList, ref GraphUpdateReadyEventArgs retArgs)
                {
                    Dictionary<ulong, ProtoCore.Core.ErrorEntry> errorMap = runner.runnerCore.LocationErrorMap;

                    if (errorMap.Count == 0)
                        return;

                    retArgs = new GraphUpdateReadyEventArgs(syncDataReturn);
                    foreach (var kvp in errorMap)
                    {
                        ProtoCore.Core.ErrorEntry err = kvp.Value;
                        string msg = err.Message;
                        int lineNo = err.Line;

                        // If error is a Build error
                        if (err.BuildId != ProtoCore.BuildData.WarningID.kDefault)
                        {
                            // Error comes from imported DS file
                            if (!string.IsNullOrEmpty(err.FileName))
                            {
                                msg += " At line " + err.Line + ", column " + err.Col + ", in " + Path.GetFileName(err.FileName);
                                if (err.Type == ProtoCore.Core.ErrorType.Error)
                                {
                                    retArgs.Errors.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = 0 });
                                }
                                else if (err.Type == ProtoCore.Core.ErrorType.Warning)
                                {
                                    retArgs.Warnings.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = 0 });
                                }
                                continue;
                            }

                        }

                        string varName = GetVarNameFromCode(lineNo, code);

                        // Errors
                        if (err.Type == ProtoCore.Core.ErrorType.Error)
                        {
                            // TODO: How can the lineNo be invalid ?
                            if (lineNo == ProtoCore.DSASM.Constants.kInvalidIndex || varName == null)
                            {
                                retArgs.Errors.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = 0 });
                                continue;
                            }

                            foreach (var pair in runner.graphCompiler.mapModifiedName)
                            {
                                string name = pair.Key;
                                if (name.Equals(varName))
                                {
                                    uint guid = pair.Value;
                                    retArgs.Errors.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = runner.graphCompiler.GetRealUID(guid) });
                                    break;
                                }

                            }
                        }
                        else if (err.Type == ProtoCore.Core.ErrorType.Warning) // Warnings
                        {
                            // TODO: How can the lineNo be invalid ?
                            if (lineNo == ProtoCore.DSASM.Constants.kInvalidIndex || varName == null)
                            {
                                retArgs.Warnings.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = 0 });
                                continue;
                            }

                            foreach (var pair in modifiedGuidList)
                            {
                                // Get the uid recognized by the graphIDE                            
                                string name = pair.Value;
                                if (name.Equals(varName))
                                {
                                    uint guid = pair.Key;
                                    retArgs.Warnings.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = runner.graphCompiler.GetRealUID(guid) });
                                    break;
                                }
                            }
                            if (retArgs.Warnings.Count == 0)
                            {
                                foreach (var pair in runner.graphCompiler.mapModifiedName)
                                {
                                    string name = pair.Key;
                                    if (name.Equals(varName))
                                    {
                                        uint guid = pair.Value;
                                        retArgs.Warnings.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = runner.graphCompiler.GetRealUID(guid) });
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                private void ReportBuildErrorsAndWarnings(string code, SynchronizeData syncDataReturn, Dictionary<uint, string> modifiedGuidList, ref GraphUpdateReadyEventArgs retArgs)
                {
                    //GraphUpdateReadyEventArgs retArgs = null;
                    if (runner.runnerCore.BuildStatus.ErrorCount > 0)
                    {
                        retArgs = new GraphUpdateReadyEventArgs(syncDataReturn);

                        foreach (var err in runner.runnerCore.BuildStatus.Errors)
                        {
                            string msg = err.Message;
                            int lineNo = err.Line;

                            // TODO: How can the lineNo be invalid ?
                            if (lineNo == ProtoCore.DSASM.Constants.kInvalidIndex)
                            {
                                retArgs.Errors.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = 0 });
                                continue;
                            }

                            string varName = GetVarNameFromCode(lineNo, code);

                            foreach (var ssnode in syncData.AddedNodes)
                            {
                                if (ssnode.Content.Contains(varName))
                                {
                                    uint id = ssnode.Id;

                                    retArgs.Errors.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = id });
                                    break;
                                }
                            }
                            if (retArgs.Errors.Count == 0)
                            {
                                foreach (var ssnode in syncData.ModifiedNodes)
                                {
                                    if (ssnode.Content.Contains(varName))
                                    {
                                        uint id = ssnode.Id;

                                        retArgs.Errors.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = id });
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (runner.runnerCore.BuildStatus.WarningCount > 0)
                    {
                        if (retArgs == null)
                            retArgs = new GraphUpdateReadyEventArgs(syncDataReturn);

                        foreach (var warning in runner.runnerCore.BuildStatus.Warnings)
                        {
                            string msg = warning.msg;
                            int lineNo = warning.line;

                            // TODO: How can the lineNo be invalid ?
                            if (lineNo == ProtoCore.DSASM.Constants.kInvalidIndex)
                            {
                                retArgs.Warnings.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = 0 });
                                continue;
                            }

                            string varName = GetVarNameFromCode(lineNo, code);

                            // This array should be empty for Build errors

                            /*foreach (var ssnode in syncDataReturn.ModifiedNodes)
                            {
                                if(ssnode.Content.Contains(varName))
                                {
                                    uint id = ssnode.Id;

                                    retArgs.Warnings.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = id });
                                    break;
                                }
                            }*/
                            foreach (var kvp in modifiedGuidList)
                            {
                                // Get the uid recognized by the graphIDE
                                uint guid = kvp.Key;
                                string name = kvp.Value;
                                if (name.Equals(varName))
                                {
                                    retArgs.Warnings.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = runner.graphCompiler.GetRealUID(guid) });
                                    break;
                                }
                            }

                            if (retArgs.Warnings.Count == 0)
                            {
                                LogWarningsFromInputNodes(retArgs, varName, msg);
                            }

                        }
                    }

                }

                void LogWarningsFromInputNodes(GraphUpdateReadyEventArgs retArgs, string varName, string msg)
                {

                    foreach (var ssnode in syncData.AddedNodes)
                    {
                        if (ssnode.Content.Contains(varName))
                        {
                            uint id = ssnode.Id;

                            retArgs.Warnings.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = id });
                            break;
                        }
                    }

                    if (retArgs.Warnings.Count == 0)
                    {
                        foreach (var ssnode in syncData.ModifiedNodes)
                        {
                            if (ssnode.Content.Contains(varName))
                            {
                                uint id = ssnode.Id;

                                retArgs.Warnings.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = id });
                                break;
                            }
                        }
                    }
                }

                private void ReportRuntimeWarnings(string code, SynchronizeData syncDataReturn, Dictionary<uint, string> modifiedGuidList, ref GraphUpdateReadyEventArgs retArgs)
                {
                    //GraphUpdateReadyEventArgs retArgs = null;

                    if (runner.runnerCore.RuntimeStatus.Warnings.Count > 0)
                    {
                        if (retArgs == null)
                            retArgs = new GraphUpdateReadyEventArgs(syncDataReturn);

                        foreach (var err in runner.runnerCore.RuntimeStatus.Warnings)
                        {
                            string msg = err.message;
                            int lineNo = err.Line;

                            // TODO: How can the lineNo be invalid ?
                            if (lineNo == ProtoCore.DSASM.Constants.kInvalidIndex)
                            {
                                retArgs.Warnings.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = 0 });
                                continue;
                            }

                            string varName = GetVarNameFromCode(lineNo, code);

                            foreach (var kvp in modifiedGuidList)
                            {
                                // Get the uid recognized by the graphIDE
                                uint guid = kvp.Key;
                                string name = kvp.Value;
                                if (name.Equals(varName))
                                {
                                    retArgs.Warnings.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = runner.graphCompiler.GetRealUID(guid) });
                                    break;
                                }
                            }

                            if (retArgs.Warnings.Count == 0)
                            {
                                LogWarningsFromInputNodes(retArgs, varName, msg);
                            }
                        }
                    }

                }

                private string GetVarNameFromCode(int lineNo, string code)
                {
                    string varName = null;

                    // Search the code using the input line no.
                    /*string[] lines = code.Split('\n');
                    string stmt = "";
                    for (int i = lineNo - 1; i < lines.Length; ++i)
                    {
                        stmt += lines[i];
                    }

                    List<ProtoCore.AST.Node> nodes = GraphUtilities.ParseCodeBlock(stmt);
                 
                    // The first node must be a binary expressions
                    ProtoCore.AST.AssociativeAST.BinaryExpressionNode ben = nodes[0] as ProtoCore.AST.AssociativeAST.BinaryExpressionNode;*/

                    // Search for the binary expression in the input code which contains the warning
                    ProtoCore.AST.AssociativeAST.BinaryExpressionNode ben = null;
                    Validity.Assert(runner.runnerCore.AstNodeList != null);
                    foreach (var node in runner.runnerCore.AstNodeList)
                    {
                        if (node is ProtoCore.AST.AssociativeAST.BinaryExpressionNode)
                        {
                            if (lineNo >= node.line && lineNo <= node.endLine)
                            {
                                ben = node as ProtoCore.AST.AssociativeAST.BinaryExpressionNode;
                                break;
                            }
                        }
                    }

                    if (ben != null)
                    {
                        ProtoCore.AST.AssociativeAST.IdentifierNode lhs = ben.LeftNode as ProtoCore.AST.AssociativeAST.IdentifierNode;
                        //Validity.Assert(lhs != null);
                        if (lhs != null)
                        {
                            varName = lhs.Name;
                        }
                        else // lhs could be an IdentifierListNode in a CodeBlock
                        {
                            ProtoCore.AST.AssociativeAST.IdentifierListNode lhsIdent = ben.LeftNode as ProtoCore.AST.AssociativeAST.IdentifierListNode;
                            Validity.Assert(lhsIdent != null);

                            // Extract line of code corresponding to this Ast node and get its LHS string
                            string identstmt = ProtoCore.Utils.ParserUtils.ExtractStatementFromCode(code, lhsIdent);
                            int equalIndex = identstmt.IndexOf('=');
                            varName = ProtoCore.Utils.ParserUtils.GetLHSatAssignment(identstmt, equalIndex)[0];
                        }

                    }

                    return varName;
                }
            }


            private class ConvertNodesToCodeTask : Task
            {
                private List<SnapshotNode> snapshotNodes;
                public ConvertNodesToCodeTask(List<SnapshotNode> snapshotNodes, LiveRunner runner)
                    : base(runner)
                {
                    if (null == snapshotNodes || (snapshotNodes.Count <= 0))
                        throw new ArgumentException("snapshotNodes", "Invalid SnapshotNode list (35CA7759D0C9)");

                    this.snapshotNodes = snapshotNodes;
                }

                public override void Execute()
                {
                    NodesToCodeCompletedEventArgs args = null;

                    // Gather a list of node IDs to be sent back.
                    List<uint> inputNodeIds = new List<uint>();
                    foreach (SnapshotNode node in snapshotNodes)
                        inputNodeIds.Add(node.Id);

                    lock (runner.operationsMutex)
                    {
                        try
                        {
                            // Do the thing you do...
                            List<SnapshotNode> outputNodes = GraphToDSCompiler.GraphUtilities.NodeToCodeBlocks(snapshotNodes, runner.graphCompiler);
                            args = new NodesToCodeCompletedEventArgs(inputNodeIds,
                                outputNodes, EventStatus.OK, "Yay, it works!");
                        }
                        catch (Exception exception)
                        {
                            args = new NodesToCodeCompletedEventArgs(inputNodeIds,
                                null, EventStatus.Error, exception.Message);
                        }
                    }

                    // Notify the listener (e.g. UI) of the completion.
                    if (null != runner.NodesToCodeCompleted)
                        runner.NodesToCodeCompleted(this, args);
                }
            }
        }
    }
}