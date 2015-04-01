using System.Collections.Generic;
using System.Linq;
using Dynamo.Models;
using Dynamo.Nodes;
using System;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.DSEngine
{
    public class Nodes2CodeParam
    {
        public IEnumerable<IEnumerable<NodeModel>> NodeGraphs { get; private set; }
        public IEnumerable<IEnumerable<AssociativeNode>> AstGraphs { get; private set; }

        public Nodes2CodeParam(
            IEnumerable<IEnumerable<NodeModel>> nodeGraphs,
            IEnumerable<IEnumerable<AssociativeNode>> astGraphs)
        {
            NodeGraphs = nodeGraphs;
            AstGraphs = astGraphs;
        }
    }

    public class Nodes2CodeUtils
    {
        private static IEnumerable<IEnumerable<NodeModel>> GetNodeGraphs(
            AstBuilder astBuilder,
            IEnumerable<NodeModel> nodes)
        {
            var topScopedNodes = ScopedNodeModel.GetNodesInTopScope(nodes);
            var sortedNodes = AstBuilder.TopologicalSort(topScopedNodes).ToList();
            sortedNodes.Add(new Function(CustomNodeDefinition.MakeProxy(System.Guid.NewGuid(), ""), "", "", ""));

            var subGraphs = new List<List<NodeModel>>();
            var subGraph = new List<NodeModel>();
            foreach (var node in sortedNodes)
            {
               if (node is Function)
               {
                   if (subGraph.Any())
                   {
                       subGraphs.Add(subGraph);
                       subGraph = new List<NodeModel>();
                   }
               }
               subGraph.Add(node);
            }

            return subGraphs;
        }

        public static Nodes2CodeParam Node2Code(AstBuilder astBuilder, IEnumerable<NodeModel> nodes, bool verboseLogging)
        {
            var astGraphs = new List<IEnumerable<AssociativeNode>>();
            var nodeGraphs = GetNodeGraphs(astBuilder, nodes);

            foreach (var graph in nodeGraphs)
            {
                var astNodes = astBuilder.CompileToAstNodes(graph, false, verboseLogging, false);
                astGraphs.Add(astNodes);
            }

            return new Nodes2CodeParam(nodeGraphs, astGraphs);
        }
    }
}
