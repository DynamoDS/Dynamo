using System.Collections.Generic;
using System.Linq;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.Views;

namespace DynamoCoreWpfTests.Utility
{
    public static class ViewExtensions
    {
        public static IEnumerable<NodeView> OfNodeModelType<T>(this IEnumerable<NodeView> nodeViews) where T : NodeModel
        {
            return nodeViews.Where(x => x.ViewModel.NodeModel is T);
        }

        public static IEnumerable<NodeView> ChildNodeViews(this WorkspaceView nodeViews)
        {
            return nodeViews.ChildrenOfType<NodeView>();
        }

        public static IEnumerable<NodeView> NodeViewsInFirstWorkspace(this DynamoView dynamoView)
        {
            return dynamoView.WorkspaceTabs.ChildrenOfType<WorkspaceView>().First().ChildNodeViews();
        }
    }
}