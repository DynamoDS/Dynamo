using CoreNodeModels.Input;
using Dynamo.Selection;
using Dynamo.Tests;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class NodeToCodeTests : DynamoViewModelUnitTest
    {
        [Test]
        public void TestNodeToCodeCommandState()
        {
            DynamoSelection.Instance.ClearSelection();
            Assert.IsFalse(ViewModel.CurrentSpaceViewModel.NodeToCodeCommand.CanExecute(null));

            var node = new DoubleInput();
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(node, false);
            DynamoSelection.Instance.Selection.Add(node);
            Assert.IsTrue(ViewModel.CurrentSpaceViewModel.NodeToCodeCommand.CanExecute(null));

            DynamoSelection.Instance.ClearSelection();
            Assert.IsFalse(ViewModel.CurrentSpaceViewModel.NodeToCodeCommand.CanExecute(null));
        }
    }
}
