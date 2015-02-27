using System;
using System.IO;
using System.Linq;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.ViewModels;

using NUnit.Framework;

namespace Dynamo.Tests
{
    internal class CustomNodeWorkspaceOpening : DSEvaluationViewModelTest
    {
        public void OpenTestFile(string folder, string fileName)
        {
            var examplePath = Path.Combine(GetTestDirectory(), folder, fileName);
            ViewModel.OpenCommand.Execute(examplePath);
        }

        [Test]
        public void CanOpenWorkspaceWithMissingCustomNodeThenFixByOpeningNeededCustomNodeWorkspace()
        {
            var model = ViewModel.Model;

            // a file with a missing custom node definition is opened
            OpenTestFile(@"core\CustomNodes", "noro.dyn");

            var homeWorkspace = model.CurrentWorkspace as HomeWorkspaceModel;
            Assert.NotNull(homeWorkspace);

            var funcNode = homeWorkspace.Nodes.OfType<Function>().First();
            Assert.IsTrue( funcNode.Definition.IsProxy );

            // the required custom node is opened
            OpenTestFile(@"core\CustomNodes\files", "ro.dyf");
            Assert.IsFalse( funcNode.Definition.IsProxy );
            
            homeWorkspace.Run();

            model.CurrentWorkspace = homeWorkspace;

            Assert.AreEqual(12.0, GetPreviewValue(funcNode.GUID));
        }

        [Test]
        public void CanOpenCustomNodeWorkspace()
        {
            var model = ViewModel.Model;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\combine", "Sequence2.dyf");
            ViewModel.OpenCommand.Execute(examplePath);

            var nodeWorkspace = model.Workspaces.FirstOrDefault(x => x is CustomNodeWorkspaceModel);
            Assert.IsNotNull(nodeWorkspace);
            Assert.AreEqual( model.CurrentWorkspace.Name, "Sequence2");
        }

        [Test]
        [Category("Failure")]
        public void CustomNodeWorkspaceIsAddedToSearchOnOpening()
        {
            var examplePath = Path.Combine(GetTestDirectory(), @"core\combine", "Sequence2.dyf");
            ViewModel.OpenCommand.Execute(examplePath);
            
            ViewModel.SearchViewModel.SearchAndUpdateResults("Sequence2");
            Assert.AreEqual(1, ViewModel.SearchViewModel.SearchResults.Count);
            Assert.AreEqual("Sequence2", ViewModel.SearchViewModel.SearchResults[0].Model.Name);
        }
    }

}
