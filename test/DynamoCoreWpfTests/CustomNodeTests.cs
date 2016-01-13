using System.Linq;
using CoreNodeModels.Input;
using Dynamo.Graph.Nodes;
using NUnit.Framework;
using Dynamo.Tests;
using Dynamo.Selection;
using Dynamo.Models;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    class CustomNodeTests :  DynamoViewModelUnitTest
    {
        [Test]
        public void TestNewNodeFromSelectionCommandState()
        {
            Assert.IsFalse(ViewModel.CurrentSpaceViewModel.NodeFromSelectionCommand.CanExecute(null));

            var node = new DoubleInput();
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(node, false);
            ViewModel.Model.AddToSelection(node);
            Assert.IsTrue(ViewModel.CurrentSpaceViewModel.NodeFromSelectionCommand.CanExecute(null));

            var ws = ViewModel.Model.CustomNodeManager.Collapse(
                DynamoSelection.Instance.Selection.OfType<NodeModel>(),
                ViewModel.Model.CurrentWorkspace,
                true,
                new FunctionNamePromptEventArgs
                {
                    Category = "Testing",
                    Description = "",
                    Name = "__CollapseTest2__",
                    Success = true
                });

            Assert.IsFalse(ViewModel.CurrentSpaceViewModel.NodeFromSelectionCommand.CanExecute(null));
        }
    }
}
