using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Dynamo.Engine.CodeGeneration;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;

namespace Dynamo.Scheduler
{
    /// <summary>
    /// A task for compiling the graph to Expressions.
    /// </summary>
    public class ExpressionCompileTask : AsyncTask
    {
        private readonly IEnumerable<NodeModel> nodes;
        private readonly NodeCompilerRegistry<Expression> compilers;
        private Dictionary<Guid, List<Expression>> nodeExpressionMap = new Dictionary<Guid, List<Expression>>();
         
        public ExpressionCompileTask(IScheduler scheduler, WorkspaceModel workspace, NodeCompilerRegistry<Expression> compilers) : base(scheduler)
        {
            this.nodes = workspace.Nodes;
            this.compilers = compilers;
        }

        public override TaskPriority Priority
        {
            get { return TaskPriority.Normal; }
        }

        protected override void HandleTaskExecutionCore()
        {
            // For each node, look for the upstream nodes, and
            // get their output port identifiers.
            var sortedNodes = AstBuilder.TopologicalSort(nodes).ToArray();

            nodeExpressionMap.Clear();
            foreach (var node in sortedNodes)
            {
                nodeExpressionMap.Add(node.GUID, BuildNodeExpressions(node));
            }

            var block = Expression.Block(nodeExpressionMap.Values.Last());
            var result = Expression.Lambda(block).Compile();
            Console.WriteLine(result.DynamicInvoke());
        }

        protected override void HandleTaskCompletionCore()
        {
            
        }

        private List<Expression> BuildNodeExpressions(NodeModel node)
        {
            var inputExpressions = new List<Expression>();
            var results = new List<Expression>();

            for (var index = 0; index < node.InPorts.Count; index++)
            {
                Tuple<int, NodeModel> inputTuple;

                if (node.TryGetInput(index, out inputTuple))
                {
                    var outputIndexOfInput = inputTuple.Item1;
                    var inputModel = inputTuple.Item2;
                    var portExpression = nodeExpressionMap[inputModel.GUID][outputIndexOfInput];

                    inputExpressions.Add(portExpression);
                }
            }

            INodeCompiler<Expression> compiler;
            if (compilers.TryGetCompilerOfType(node.GetType(), out compiler))
            {
                var outputExpressions = compiler.Compile(node, inputExpressions);
                results.AddRange(outputExpressions);
            }

            return results;
        }

    }
}
