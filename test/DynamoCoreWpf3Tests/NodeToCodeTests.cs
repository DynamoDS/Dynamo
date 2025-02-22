using CoreNodeModels;
using CoreNodeModels.Input;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Selection;
using Dynamo.Tests;
using NUnit.Framework;
using ProtoCore.Namespace;
using DynCmd = Dynamo.Models.DynamoModel;

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

        [Test]
        public void TestNodeToCodeCannotRun()
        {
            DynamoSelection.Instance.ClearSelection();
            Assert.IsFalse(ViewModel.CurrentSpaceViewModel.NodeToCodeCommand.CanExecute(null));

            var model = ViewModel.Model;
            //create some numbers
            var numberNode1 = new DoubleInput();
            numberNode1.Value = "1";
            numberNode1.Error("Tesst");

            var cbn = new CodeBlockNodeModel("xx;", 100, 100, model.LibraryServices, model.CurrentWorkspace.ElementResolver);
            model.ExecuteCommand(new DynCmd.CreateNodeCommand(cbn, 0, 0, true, false));
            model.ExecuteCommand(new DynCmd.CreateNodeCommand(numberNode1, 0, 0, true, false));

            numberNode1.ConnectOutput(0, 0, cbn);

            cbn.SetCodeContent("yy; !", model.CurrentWorkspace.ElementResolver);

            DynamoSelection.Instance.Selection.Add(cbn);

            Assert.IsFalse(ViewModel.CurrentSpaceViewModel.NodeToCodeCommand.CanExecute(null));

            Assert.DoesNotThrow(() => {
                ViewModel.CurrentSpaceViewModel.NodeToCodeCommand.Execute(null);
            });



        }
    }
}
