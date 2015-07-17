using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

using SystemTestServices;

using Dynamo;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Services;
using Dynamo.Tests;
using Dynamo.ViewModels;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    class PresetViewModelTest :  DynamoViewModelUnitTest
    {
        [Test]
        public void TogglePresetOptionVisibility()
        {
            //Create a Node
            var addNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+"));
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            //verify the node was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Nodes.Count());

            //Check the Preset option visibility.
            Assert.AreEqual(false, ViewModel.EnablePresetOptions);

            DynamoSelection.Instance.Selection.Add(addNode);

            var IDS = DynamoSelection.Instance.Selection.OfType<NodeModel>().Select(x => x.GUID).ToList();
            //create the preset from 2 nodes
            ViewModel.Model.CurrentWorkspace.AddPreset(
                "state1",
                "a state with 2 numbers", IDS);
            //assert that the preset has been created
            Assert.AreEqual(ViewModel.Model.CurrentWorkspace.Presets.Count(), 1);
            Assert.AreEqual(ViewModel.Model.CurrentWorkspace.Presets.First().Nodes.Count(), 1);

            //Check the Preset option visibility.
            Assert.AreEqual(true, ViewModel.EnablePresetOptions);

            //Delete the preset
            //delete state
            var state = ViewModel.Model.CurrentWorkspace.Presets.First();
            ViewModel.Model.CurrentWorkspace.RemovePreset(state);

            Assert.AreEqual(ViewModel.Model.CurrentWorkspace.Presets.Count(), 0);

            //Check the Preset option visibility.
            Assert.AreEqual(false, ViewModel.EnablePresetOptions);
        }
    }
}
