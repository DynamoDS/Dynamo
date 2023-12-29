using System;
using System.Linq;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Workspaces;
using NUnit.Framework;

namespace Dynamo.Tests
{
    public static class TestExtensions
    {
        public static NodeModel NodeFromWorkspace(this WorkspaceModel vm, string guidString)
        {
            Guid guid;
            Guid.TryParse(guidString, out guid);
            return vm.NodeFromWorkspace(guid);
        }

        public static NodeModel NodeFromWorkspace(this WorkspaceModel ws, Guid guid)
        {
            return ws.Nodes.FirstOrDefault(node => node.GUID == guid);
        }

        public static T NodeFromWorkspace<T>(this WorkspaceModel ws, Guid guid) where T : NodeModel
        {
            var nodeToT = NodeFromWorkspace(ws, guid);
            Assert.NotNull(nodeToT);
            Assert.IsAssignableFrom(typeof(T), nodeToT);
            return nodeToT as T;
        }

        public static T NodeFromWorkspace<T>(this WorkspaceModel ws, string guidString)
            where T : NodeModel
        {
            Guid guid;
            Guid.TryParse(guidString, out guid);
            return ws.NodeFromWorkspace<T>(guid);
        }

        public static T FirstNodeFromWorkspace<T>(this WorkspaceModel model) where T : NodeModel
        {
            return model.Nodes.OfType<T>().FirstOrDefault();
        }

        public static NodeModel GetDSFunctionNodeFromWorkspace(this WorkspaceModel model, string nodeName)
        {
            return model.Nodes.FirstOrDefault(node => node is DSFunction &&
                                                      node.Name == nodeName);
        }
    }
}
