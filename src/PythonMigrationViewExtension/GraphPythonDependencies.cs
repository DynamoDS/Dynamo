using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Wpf.Extensions;
using PythonNodeModels;

namespace Dynamo.PythonMigration
{
    internal class GraphPythonDependencies
    {
        private readonly List<PythonNodeBase> graphPythonNodes;

        internal GraphPythonDependencies(IWorkspaceModel workspace)
        {
            if (workspace is null)
                throw new ArgumentNullException(nameof(workspace));

            graphPythonNodes = workspace.Nodes
                .Where(n => n.GetType().IsSubclassOf(typeof(PythonNodeBase)))
                .Select(n => n as PythonNodeBase)
                .ToList();
        }

        internal bool ContainsPythonDependencies()
        {
            return graphPythonNodes.Any();
        }

        internal IEnumerable<PythonNodeBase> GetPythonNodes()
        {
            return graphPythonNodes.ToList();
        }

        internal bool ContainsIronPythonDependencies()
        {
            return graphPythonNodes.Any(n => IsIronPythonNode(n));
        }

        internal bool ContainsCPythonDependencies()
        {
            return graphPythonNodes.Any(n => IsCPythonNode(n));
        }

        internal bool IsIronPythonNode(NodeModel obj)
        {
            if (!(obj is PythonNodeBase pythonNode))
                return false;

            if (!graphPythonNodes.Contains(pythonNode))
                graphPythonNodes.Add(pythonNode);

            return pythonNode.Engine == PythonEngineVersion.IronPython2;
        }

        internal bool IsCPythonNode(NodeModel obj)
        {
            if (!(obj is PythonNodeBase pythonNode))
                return false;

            if (!graphPythonNodes.Contains(pythonNode))
                graphPythonNodes.Add(pythonNode);

            return pythonNode.Engine == PythonEngineVersion.CPython3;
        }

        internal void RemovePythonNode(NodeModel obj)
        {
            if (!(obj is PythonNodeBase pythonNode))
                return;

            if (!graphPythonNodes.Contains(pythonNode))
                return;

            graphPythonNodes.Remove(pythonNode);
        }
    }
}
