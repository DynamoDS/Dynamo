using System.Collections.Generic;
using System.IO;
using CoreNodeModels.Input;
using Dynamo.Graph.Workspaces;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;
using SystemTestServices;
using DSColor = DSCore.Color;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    class ColorPaletteTests : SystemTestBase
    {
        [Test]
        public void ColorPalette_AddToHomespaceAndRun_NoException()
        {
            var homespace = Model.CurrentWorkspace as HomeWorkspaceModel;
            Assert.NotNull(homespace, "The current workspace is not a HomeWorkspaceModel");
            var colorPalette = new ColorPalette();
            Model.AddNodeToCurrentWorkspace(colorPalette, true);
            homespace.Run();           
            Assert.DoesNotThrow(DispatcherUtil.DoEvents);
            Assert.AreEqual(colorPalette.DsColor, DSColor.ByARGB(255,0,0,0));
            Assert.Pass();

            // Verify same results in JSON

            // Change from default color to red
            homespace.UpdateModelValue(new List<System.Guid>() { colorPalette.GUID }, "DsColor", DSColor.ByARGB(255, 255, 0, 0).ToString());
            homespace.Run();
            Assert.AreEqual(DSColor.ByARGB(255, 255, 0, 0), colorPalette.DsColor);
            // Save in temp location
            var tempPath = Path.GetTempPath() + "ColorPaletteTest_temp.dyn";
            ViewModel.SaveAs(tempPath);
            // Close workspace
            Assert.IsTrue(ViewModel.CloseHomeWorkspaceCommand.CanExecute(null));
            ViewModel.CloseHomeWorkspaceCommand.Execute(null);
            // Open JSON temp file
            OpenDynamoDefinition(tempPath);
            // Run
            RunCurrentModel();
            // Verify xml results against json
            Assert.DoesNotThrow(DispatcherUtil.DoEvents);
            Assert.AreEqual(colorPalette.DsColor, DSColor.ByARGB(255, 255, 0, 0));
            // Delete temp file
            File.Delete(tempPath);
        }

        [Test]
        public void ColorPalette_Undo()
        {
            var homespace = Model.CurrentWorkspace as HomeWorkspaceModel;
            Assert.NotNull(homespace, "The current workspace is not a HomeWorkspaceModel");
            var colorPalette = new ColorPalette();
            Model.AddNodeToCurrentWorkspace(colorPalette, true);
            homespace.Run();
            Assert.DoesNotThrow(DispatcherUtil.DoEvents);
            Assert.AreEqual(DSColor.ByARGB(255, 0, 0, 0),colorPalette.DsColor);
            homespace.UpdateModelValue(new List<System.Guid>() { colorPalette.GUID }, "DsColor", DSColor.ByARGB(255, 255, 255, 255).ToString());
            Assert.AreEqual(DSColor.ByARGB(255, 255, 255, 255), colorPalette.DsColor);
            homespace.Undo();
            Assert.AreEqual(DSColor.ByARGB(255, 0, 0, 0), colorPalette.DsColor);
            homespace.Redo();
            Assert.AreEqual(DSColor.ByARGB(255, 255, 255, 255), colorPalette.DsColor);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void ColorPalette_CreatesCorrectUndoStackForComplexState()
        {
            var homespace = Model.CurrentWorkspace as HomeWorkspaceModel;
            Assert.NotNull(homespace, "The current workspace is not a HomeWorkspaceModel");
            var colorPalette = new ColorPalette();
            Model.AddNodeToCurrentWorkspace(colorPalette, true);
            homespace.Run();
            Assert.DoesNotThrow(DispatcherUtil.DoEvents);
            Assert.AreEqual(DSColor.ByARGB(255, 0, 0, 0), colorPalette.DsColor);
            homespace.UpdateModelValue(new List<System.Guid>() { colorPalette.GUID }, "DsColor", DSColor.ByARGB(255, 255, 0, 0).ToString());
            homespace.UpdateModelValue(new List<System.Guid>() { colorPalette.GUID }, "DsColor", DSColor.ByARGB(255, 0, 0, 255).ToString());
            homespace.UpdateModelValue(new List<System.Guid>() { colorPalette.GUID }, "DsColor", DSColor.ByARGB(255, 255, 0, 0).ToString());

            //now undo a few times and assert the color is red.
            homespace.Undo();
            Assert.AreEqual(DSColor.ByARGB(255, 0, 0, 255), colorPalette.DsColor);
            homespace.Undo();
            Assert.AreEqual(DSColor.ByARGB(255, 255, 0, 0), colorPalette.DsColor);
            Assert.IsTrue(homespace.UndoRecorder.CanUndo);
            homespace.Undo();
            Assert.AreEqual(DSColor.ByARGB(255, 0, 0, 0), colorPalette.DsColor);

        }
    }
}
