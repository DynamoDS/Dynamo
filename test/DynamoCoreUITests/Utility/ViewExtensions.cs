using System.Collections.Generic;
using System.Linq;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.Views;

namespace DynamoCoreUITests.Utility
{
    public static class ViewExtensions
    {
        public static IEnumerable<dynNodeView> OfNodeModelType<T>(this IEnumerable<dynNodeView> nodeViews) where T : NodeModel
        {
            return nodeViews.Where(x => x.ViewModel.NodeModel as T != null);
        }

        public static IEnumerable<dynNodeView> ChildNodeViews(this dynWorkspaceView nodeViews)
        {
            return nodeViews.ChildrenOfType<dynNodeView>();
        }

        public static IEnumerable<dynNodeView> NodeViewsInFirstWorkspace(this DynamoView dynamoView)
        {
            return dynamoView.WorkspaceTabs.ChildrenOfType<dynWorkspaceView>().First().ChildNodeViews();
        }
    }
}