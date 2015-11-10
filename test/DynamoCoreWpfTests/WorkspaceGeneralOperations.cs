using System.Linq;
using NUnit.Framework;
using Dynamo.Selection;
using System.IO;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Workspaces;

namespace Dynamo.Tests
{
    public class WorkspaceGeneralOperations : DynamoViewModelUnitTest
    {
        [Test]
        public void SwitchingToCustomWorkspaceWithSelectionShouldNotAllowGeometricOperations()
        {
            var model = ViewModel.Model; // The current DynamoModel instance.

            // Step 0: Create a new node in Home workspace.
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
            model.CurrentWorkspace.AddAndRegisterNode(addNode, false);
            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count(), 1);

            // Step 1: Select the newly node, geometry operation should be enabled.
            DynamoSelection.Instance.Selection.Add(addNode);
            Assert.AreEqual(true, ViewModel.CurrentSpaceViewModel.HasSelection);
            Assert.AreEqual(true, ViewModel.CurrentSpaceViewModel.IsGeometryOperationEnabled);

            // Step 2: Open a Custom workspace.
            var customNodePath = Path.Combine(TestDirectory, @"core\CustomNodes\NoInput.dyf");
            ViewModel.OpenCommand.Execute(customNodePath);
            var customWorkspace = model.Workspaces.FirstOrDefault(x => x is CustomNodeWorkspaceModel);
            Assert.IsNotNull(customWorkspace);

            // Step 3: Switch over from home workspace to custom workspace.
            Assert.IsTrue(ViewModel.CurrentSpaceViewModel.Model is HomeWorkspaceModel);
            ViewModel.CurrentWorkspaceIndex = 1;
            Assert.IsTrue(ViewModel.CurrentSpaceViewModel.Model is CustomNodeWorkspaceModel);

            // Step 4: Verify that the geometry operations are 
            // disabled despite the fact that there is still selection.
            Assert.AreEqual(true, ViewModel.CurrentSpaceViewModel.HasSelection);
            Assert.AreEqual(false, ViewModel.CurrentSpaceViewModel.IsGeometryOperationEnabled);
        }
    }
}
