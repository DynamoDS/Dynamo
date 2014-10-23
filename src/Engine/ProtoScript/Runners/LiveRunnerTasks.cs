using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                        string code = new ProtoCore.CodeGenDS(astList).GenerateCode();
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

}