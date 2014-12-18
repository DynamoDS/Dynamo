using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using DynamoUtilities;
using NUnit.Framework;
using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.Tests
{
    internal class CoreTests : DynamoViewModelUnitTest
    {
        // OpenCommand
        [Test]
        public void CanOpenGoodFile()
        {
            string openPath = Path.Combine(GetTestDirectory(), @"core\multiplicationAndAdd\multiplicationAndAdd.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            Assert.AreEqual(5, ViewModel.CurrentSpace.Nodes.Count);
        }

        [Test]
        [Category("UnitTests")]
        public void CanAddANodeByName()
        {
            var model = ViewModel.Model;
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
            model.CurrentWorkspace.AddNode(addNode, false);
            Assert.AreEqual(ViewModel.CurrentSpace.Nodes.Count, 1);
        }

        [Test]
        [Category("UnitTests")]
        public void CanAddANote()
        {
            // Create some test note data
            Guid id = Guid.NewGuid();
            ViewModel.Model.CurrentWorkspace.AddNote(false, 200, 200, "This is a test note", id);
            Assert.AreEqual(ViewModel.CurrentSpace.Notes.Count, 1);
        }

        [Test]
        [Category("UnitTests")]
        public void CanAddToSelectionAndNotThrowExceptionWhenPassedIncorrectType()
        {
            var model = ViewModel.Model;
            
            int numNodes = 100;

            // select all of them one by one
            for (int i = 0; i < numNodes; i++)
            {
                Assert.DoesNotThrow(() => model.AddToSelection(null));
                Assert.DoesNotThrow(() => model.AddToSelection(5));
                Assert.DoesNotThrow(() => model.AddToSelection("noodle"));
                Assert.DoesNotThrow(() => model.AddToSelection(new StringBuilder()));
            }
        }

        [Test]
        [Category("UnitTests")]
        public void CanAddToSelectionCommand()
        {
            var model = ViewModel.Model;

            int numNodes = 100;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
                model.CurrentWorkspace.AddNode(addNode, false);

                Assert.AreEqual(i + 1, ViewModel.CurrentSpace.Nodes.Count);

                model.AddToSelection(addNode);
                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }
        }

        // Log

        [Test]
        [Category("UnitTests")]
        public void CanClearLog()
        {
            Assert.AreNotEqual(0, ViewModel.LogText.Length);

            ViewModel.ClearLog(null);
            Assert.AreEqual(0, ViewModel.LogText.Length);
        }

        // Clearworkspace 

        [Test]
        [Category("UnitTests")]
        public void CanClearWorkspaceWithEmptyWorkspace()
        {
            ViewModel.Model.ClearCurrentWorkspace();
            Assert.AreEqual(0, ViewModel.Model.CurrentWorkspace.Nodes.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void CanClearWorkspaceWithNodes()
        {
            var model = ViewModel.Model;
            Assert.AreEqual(0, ViewModel.Model.CurrentWorkspace.Nodes.Count());

            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
            model.CurrentWorkspace.AddNode(addNode, false);
            model.CurrentWorkspace.AddNode(new DoubleInput(), false);
            model.CurrentWorkspace.AddNode(new DoubleInput(), false);
            Assert.AreEqual(3, ViewModel.Model.CurrentWorkspace.Nodes.Count());

            model.ClearCurrentWorkspace();
            Assert.AreEqual(0, ViewModel.Model.CurrentWorkspace.Nodes.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void CanAdd100NodesToClipboard()
        {
            var model = ViewModel.Model;

            int numNodes = 100;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
                model.CurrentWorkspace.AddNode(addNode, false);
                Assert.AreEqual(i + 1, ViewModel.CurrentSpace.Nodes.Count);

                model.AddToSelection(ViewModel.Model.CurrentWorkspace.Nodes[i]);
                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            model.Copy();
            Assert.AreEqual(numNodes, ViewModel.Model.ClipBoard.Count);
        }

        [Test]
        [Category("UnitTests")]
        public void ValidateConnectionsDoesNotClearError()
        {
            var model = ViewModel.Model;
            model.CurrentWorkspace.AddNode(
                 new CodeBlockNodeModel("30", 100.0, 100.0, model.LibraryServices),
                false);

            Assert.AreEqual(1, model.CurrentWorkspace.Nodes.Count());

            // Make sure we have the number node created in active state.
            var codeBlockNode = model.CurrentWorkspace.Nodes[0] as CodeBlockNodeModel;
            Assert.IsNotNull(codeBlockNode);
            Assert.AreEqual(ElementState.Active, codeBlockNode.State);

            // Entering an invalid value will cause it to be erroneous.
            codeBlockNode.SetCodeContent("--", model.CurrentWorkspace.UndoRecorder); // Invalid numeric value.
            Assert.AreEqual(ElementState.Error, codeBlockNode.State);
            Assert.IsNotEmpty(codeBlockNode.ToolTipText); // Error tooltip text.

            // Ensure the number node is not selected now.
            Assert.AreEqual(false, codeBlockNode.IsSelected);

            // Try to select the node and make sure it is still erroneous.
            model.AddToSelection(codeBlockNode);
            Assert.AreEqual(true, codeBlockNode.IsSelected);
            Assert.AreEqual(ElementState.Error, codeBlockNode.State);
            Assert.IsNotEmpty(codeBlockNode.ToolTipText); // Error tooltip text.

            // Deselect the node and ensure its error state isn't cleared.
            DynamoSelection.Instance.Selection.Remove(codeBlockNode);
            Assert.AreEqual(false, codeBlockNode.IsSelected);
            Assert.AreEqual(ElementState.Error, codeBlockNode.State);
            Assert.IsNotEmpty(codeBlockNode.ToolTipText); // Error tooltip text.

            // Update to valid numeric value, should cause the node to be active.
            codeBlockNode.SetCodeContent("1234;", model.CurrentWorkspace.UndoRecorder);
            Assert.AreEqual(ElementState.Active, codeBlockNode.State);
            Assert.IsEmpty(codeBlockNode.ToolTipText); // Error tooltip is gone.
        }

        [Test]
        [Category("UnitTests")]
        public void CanAdd1NodeToClipboardAndPaste()
        {
            var model = ViewModel.Model;

            int numNodes = 1;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
                model.CurrentWorkspace.AddNode(addNode, false);
                Assert.AreEqual(i + 1, ViewModel.CurrentSpace.Nodes.Count);

                model.AddToSelection(ViewModel.Model.CurrentWorkspace.Nodes[i]);
                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            model.Copy();
            Assert.AreEqual(numNodes, ViewModel.Model.ClipBoard.Count);

            model.Paste();
            Assert.AreEqual(numNodes * 2, ViewModel.CurrentSpace.Nodes.Count);
        }

        [Test]
        [Category("UnitTests")]
        public void CanAdd100NodesToClipboardAndPaste()
        {
            var model = ViewModel.Model;

            int numNodes = 100;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
                model.CurrentWorkspace.AddNode(addNode, false);
                Assert.AreEqual(i + 1, ViewModel.CurrentSpace.Nodes.Count);

                model.AddToSelection(addNode);
                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            model.Copy();
            Assert.AreEqual(numNodes, ViewModel.Model.ClipBoard.Count);

            model.Paste();
            Assert.AreEqual(numNodes * 2, ViewModel.CurrentSpace.Nodes.Count);
        }

        [Test]
        [Category("UnitTests")]
        public void CanAdd100NodesToClipboardAndPaste3Times()
        {
            var model = ViewModel.Model;

            int numNodes = 100;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
                model.CurrentWorkspace.AddNode(addNode, false);
                Assert.AreEqual(i + 1, ViewModel.CurrentSpace.Nodes.Count);

                model.AddToSelection(ViewModel.Model.CurrentWorkspace.Nodes[i]);

                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            model.Copy();

            Assert.AreEqual(numNodes, ViewModel.Model.ClipBoard.Count);

            int numPastes = 3;
            for (int i = 1; i <= numPastes; i++)
            {
                model.Paste();
                Assert.AreEqual(numNodes, ViewModel.Model.ClipBoard.Count);
                Assert.AreEqual(numNodes * (i + 1), ViewModel.CurrentSpace.Nodes.Count);
            }
        }

        [Test]
        [Category("UnitTests")]
        public void CanAddOneNodeToClipboard()
        {
            var model = ViewModel.Model;

            int numNodes = 1;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
                model.CurrentWorkspace.AddNode(addNode, false);

                Assert.AreEqual(i + 1, ViewModel.CurrentSpace.Nodes.Count);

                model.AddToSelection(ViewModel.Model.CurrentWorkspace.Nodes[i]);
                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            model.Copy();
            Assert.AreEqual(numNodes, ViewModel.Model.ClipBoard.Count);
        }

        [Test]
        [Category("UnitTests")]
        public void CanCopyAndPasteDSVarArgFunctionNode()
        {
            var model = ViewModel.Model;
            Assert.AreEqual(0, model.CurrentWorkspace.Nodes.Count);

            const string dsVarArgFunctionName = "DSCore.String.Split@string,string[]";
            var node = new DSVarArgFunction(model.LibraryServices.GetFunctionDescriptor(dsVarArgFunctionName));
            model.CurrentWorkspace.AddNode(node, false);

            // Here we check to see if we do get a DSVarArgFunction node (which
            // is what this test case is written for, other nodes will render the 
            // test case meaningless).
            // 
            Assert.AreEqual(1, model.CurrentWorkspace.Nodes.Count);

            model.AddToSelection(node); // Select the only DSVarArgFunction node.
            model.Copy(); // Copy the only DSVarArgFunction node.

            Assert.DoesNotThrow(() =>
            {
                model.Paste(); // Nope, paste should not crash Dynamo.
            });
        }

        /// <summary>
        ///     Pasting an Input or Output node into the Home Workspace converts them
        ///     to a Code Block node.
        /// </summary>
        [Test]
        public void PasteInputAndOutputNodeInHomeWorkspace()
        {
            const string name = "Custom Node Creation Test";
            const string description = "Description";
            const string category = "Custom Node Category";

            ViewModel.ExecuteCommand(new DynamoModel.CreateCustomNodeCommand(
                    Guid.NewGuid(),
                    name,
                    category,
                    description,
                    true));

            ViewModel.ExecuteCommand(new DynamoModel.CreateNodeCommand(
                new Symbol(),
                0, 0,
                true, true));

            ViewModel.ExecuteCommand(new DynamoModel.CreateNodeCommand(
                new Output(),
                0, 0,
                true, true));

            foreach (var node in ViewModel.Model.CurrentWorkspace.Nodes)
                ViewModel.Model.AddToSelection(node); 

            ViewModel.Model.Copy();
            ViewModel.HomeCommand.Execute(null);
            ViewModel.Model.Paste();

            var homeNodes = ViewModel.HomeSpace.Nodes;

            Assert.AreEqual(2, homeNodes.Count);
            Assert.IsInstanceOf<CodeBlockNodeModel>(homeNodes[0]);
            Assert.IsInstanceOf<CodeBlockNodeModel>(homeNodes[1]);
        }

        [Test]
        public void TestFileDirtyOnLacingChange()
        {
            string openPath = Path.Combine(GetTestDirectory(), @"core\LacingTest.dyn");            
            ViewModel.OpenCommand.Execute(openPath);

            WorkspaceModel workspace = ViewModel.CurrentSpace;            
            Assert.AreEqual(false, workspace.CanUndo);
            Assert.AreEqual(false, workspace.CanRedo);

            //Assert HasUnsavedChanges is false 
            Assert.AreEqual(false, workspace.HasUnsavedChanges);

            Assert.AreEqual(5, workspace.Nodes.Count);          
            
            //Get the first node and assert the lacing strategy
            var node = ViewModel.Model.CurrentWorkspace.Nodes[0];
            Assert.IsNotNull(node);
            Assert.AreEqual(LacingStrategy.Shortest, node.ArgumentLacing);

            var workSpaceViewModel = ViewModel.CurrentSpaceViewModel;
            var nodes = workSpaceViewModel.Nodes;
            var nodeViewModel = nodes.First(x => x.NodeLogic == node);
            
            //change the lacing strategy
            nodeViewModel.SetLacingTypeCommand.Execute("Longest");
            Assert.AreEqual(LacingStrategy.Longest, node.ArgumentLacing);
          
            Assert.AreEqual(true, workspace.HasUnsavedChanges);
        }

        // SaveImage

        //[Test]
        //public void CanGoHomeWhenInDifferentWorkspace()
        //{
        //    // move to different workspace
        //    // go home
        //    // need to create new function via command
        //    //TODO: loadWorkspaceFromFileCommand
        //}

        // SaveAsCommand

        [Test]
        public void CanSaveAsEmptyFile()
        {
            string fn = "ruthlessTurtles.dyn";
            string path = Path.Combine(TempFolder, fn);
            ViewModel.SaveAsCommand.Execute(path);
            
            var tempFldrInfo = new DirectoryInfo(TempFolder);
            Assert.AreEqual(1, tempFldrInfo.GetFiles().Length);
            Assert.AreEqual(fn, tempFldrInfo.GetFiles()[0].Name);
        }

        [Test]
        public void CanSaveAsFileWithNodesInIt()
        {
            var model = ViewModel.Model;

            int numNodes = 100;

            for (int i = 0; i < numNodes; i++)
            {
                var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
                model.CurrentWorkspace.AddNode(addNode, false);
                Assert.AreEqual(i + 1, ViewModel.CurrentSpace.Nodes.Count);
            }

            string fn = "ruthlessTurtles.dyn";
            string path = Path.Combine(TempFolder, fn);
            ViewModel.SaveAsCommand.Execute(path);

            var tempFldrInfo = new DirectoryInfo(TempFolder);
            Assert.AreEqual(1, tempFldrInfo.GetFiles().Length);
            Assert.AreEqual(fn, tempFldrInfo.GetFiles()[0].Name);
        }

        // SaveCommand

        [Test]
        [Category("UnitTests")]
        public void CannotSaveEmptyWorkspaceIfSaveIsCalledWithoutSettingPath()
        {
            ViewModel.SaveAsCommand.Execute(null);

            Assert.IsNull(ViewModel.CurrentSpace.FileName);
        }

        [Test]
        [Category("UnitTests")]
        public void CannotSavePopulatedWorkspaceIfSaveIsCalledWithoutSettingPath()
        {
            var model = ViewModel.Model;

            int numNodes = 100;

            for (int i = 0; i < numNodes; i++)
            {
                var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
                model.CurrentWorkspace.AddNode(addNode, false);
                Assert.AreEqual(i + 1, ViewModel.CurrentSpace.Nodes.Count);
            }

            ViewModel.SaveCommand.Execute(null);

            Assert.IsNull(ViewModel.CurrentSpace.FileName);
        }



        [Test]
        [Category("UnitTests")]
        public void CanSelectAndNotThrowExceptionWhenPassedIncorrectType()
        {
            int numNodes = 100;

            // select all of them one by one
            for (int i = 0; i < numNodes; i++)
            {
                ViewModel.Model.OnRequestSelect(this, new ModelEventArgs(null));
            }
        }

        [Test]
        [Category("UnitTests")]
        public void CanStayHomeWhenInHomeWorkspace()
        {
            var model = ViewModel.Model;

            for (int i = 0; i < 20; i++)
            {
                model.CurrentWorkspace = ViewModel.HomeSpace;
                Assert.AreEqual(true, ViewModel.ViewingHomespace);
            }
        }

        [Test]
        [Category("UnitTests")]
        public void TestRecordModelsForModificationWithEmptyInput()
        {
            WorkspaceModel workspace = ViewModel.CurrentSpace;
            Assert.AreEqual(false, workspace.CanUndo);

            // Calling the method with a null argument.
            WorkspaceModel.RecordModelsForModification(null, workspace.UndoRecorder);
            Assert.AreEqual(false, workspace.CanUndo);

            // Calling the method with an empty list.
            List<ModelBase> models = new List<ModelBase>();
            WorkspaceModel.RecordModelsForModification(models, workspace.UndoRecorder);
            Assert.AreEqual(false, workspace.CanUndo);

            // Calling the method with a list full of null.
            models.Add(null);
            models.Add(null);
            WorkspaceModel.RecordModelsForModification(models, workspace.UndoRecorder);
            Assert.AreEqual(false, workspace.CanUndo);
        }

        [Test]
        [Category("UnitTests")]
        public void TestRecordCreatedModelsWithEmptyInput()
        {
            WorkspaceModel workspace = ViewModel.CurrentSpace;
            Assert.AreEqual(false, workspace.CanUndo);

            // Calling the method with a null argument.
            workspace.RecordCreatedModels(null);
            Assert.AreEqual(false, workspace.CanUndo);

            // Calling the method with an empty list.
            List<ModelBase> models = new List<ModelBase>();
            workspace.RecordCreatedModels(models);
            Assert.AreEqual(false, workspace.CanUndo);

            // Calling the method with a list full of null.
            models.Add(null);
            models.Add(null);
            workspace.RecordCreatedModels(models);
            Assert.AreEqual(false, workspace.CanUndo);
        }

        [Test]
        [Category("UnitTests")]
        public void TestRecordAndDeleteModelsWithEmptyInput()
        {
            WorkspaceModel workspace = ViewModel.CurrentSpace;
            Assert.AreEqual(false, workspace.CanUndo);

            // Calling the method with a null argument.
            workspace.RecordAndDeleteModels(null);
            Assert.AreEqual(false, workspace.CanUndo);

            // Calling the method with an empty list.
            List<ModelBase> models = new List<ModelBase>();
            workspace.RecordAndDeleteModels(models);
            Assert.AreEqual(false, workspace.CanUndo);

            // Calling the method with a list full of null.
            models.Add(null);
            models.Add(null);
            workspace.RecordAndDeleteModels(models);
            Assert.AreEqual(false, workspace.CanUndo);
        }

        [Test]
        [Category("UnitTests")]
        public void CanSumTwoNumbers()
        {
            var model = ViewModel.Model;
            
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
            model.CurrentWorkspace.AddNode(addNode, false);
            model.CurrentWorkspace.AddNode(new CodeBlockNodeModel("2", 100.0, 100.0, model.LibraryServices), false);
            model.CurrentWorkspace.AddNode(new CodeBlockNodeModel("2", 100.0, 100.0, model.LibraryServices), false);
            model.CurrentWorkspace.AddNode(new Watch { X = 100, Y = 300}, false);

            ConnectorModel.Make(ViewModel.Model.CurrentWorkspace.Nodes[1], ViewModel.Model.CurrentWorkspace.Nodes[0], 0, 0);
            ConnectorModel.Make(ViewModel.Model.CurrentWorkspace.Nodes[2], ViewModel.Model.CurrentWorkspace.Nodes[0], 0, 1);
            ConnectorModel.Make(ViewModel.Model.CurrentWorkspace.Nodes[0], ViewModel.Model.CurrentWorkspace.Nodes[3], 0, 0);

            ViewModel.HomeSpace.Run();

            Thread.Sleep(250);

            Assert.AreEqual(ViewModel.Model.CurrentWorkspace.Nodes[3] is Watch, true);

            var w = (Watch)ViewModel.Model.CurrentWorkspace.Nodes[3];
            Assert.AreEqual(4.0, w.CachedValue);
        }

        [Test]
        public void CanOpenDSVarArgFunctionFile()
        {
            string openPath = Path.Combine(GetTestDirectory(),
                @"core\dsfunction\dsvarargfunction.dyn");

            var dynamoModel = ViewModel.Model;
            ViewModel.OpenCommand.Execute(openPath);
            var workspace = dynamoModel.CurrentWorkspace;
            Assert.AreEqual(1, workspace.Nodes.Count);

            var node = workspace.NodeFromWorkspace<DSVarArgFunction>(
                Guid.Parse("a182d3f8-bb7d-4480-8aa5-eaacd6161415"));

            Assert.IsNotNull(node);
            Assert.IsNotNull(node.Controller.Definition);
            Assert.AreEqual(3, node.InPorts.Count);
        }
        
        [Test]
        [Category("UnitTests")]
        public void SelectionDoesNotChangeWhenAddingAlreadySelectedNode()
        {
            var model = ViewModel.Model;

            int numNodes = 100;

            // create 100 nodes, and select them as you go
            for (int i = 0; i < numNodes; i++)
            {
                var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
                model.CurrentWorkspace.AddNode(addNode, false);
                Assert.AreEqual(i + 1, ViewModel.CurrentSpace.Nodes.Count);

                model.AddToSelection(ViewModel.Model.CurrentWorkspace.Nodes[i]);
                Assert.AreEqual(i + 1, DynamoSelection.Instance.Selection.Count);
            }

            // the number selected stays the same
            for (int i = 0; i < numNodes; i++)
            {
                model.AddToSelection(ViewModel.Model.CurrentWorkspace.Nodes[i]);
                Assert.AreEqual(numNodes, DynamoSelection.Instance.Selection.Count);
            }
        }

        [Test]
        [Category("UnitTests")]
        public void TestDraggedNode()
        {
            var model = ViewModel.Model;
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+")) { X = 16, Y = 32 };
            model.CurrentWorkspace.AddNode(addNode, false);
            NodeModel locatable = ViewModel.Model.CurrentWorkspace.Nodes[0];

            var startPoint = new Point2D(8, 64);
            var dn = new WorkspaceViewModel.DraggedNode(locatable, startPoint);

            // Initial node position.
            Assert.AreEqual(16, locatable.X);
            Assert.AreEqual(32, locatable.Y);

            // Move the mouse cursor to move node.
            dn.Update(new Point2D(-16, 72));
            Assert.AreEqual(-8, locatable.X);
            Assert.AreEqual(40, locatable.Y);
        }

        [Test]
        public void NodesHaveCorrectLocationsIndpendentOfCulture()
        {
            string openPath = Path.Combine(GetTestDirectory(), @"core\nodeLocationTest.dyn");

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("es-AR");
            ViewModel.OpenCommand.Execute(openPath);

            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Nodes.Count);
            var node = ViewModel.Model.CurrentWorkspace.Nodes.First();
            Assert.AreEqual(217.952067513811, node.X);
            Assert.AreEqual(177.041832898393, node.Y);

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("zu-ZA");
            ViewModel.OpenCommand.Execute(openPath);

            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Nodes.Count);
            node = ViewModel.Model.CurrentWorkspace.Nodes.First();
            Assert.AreEqual(217.952067513811, node.X);
            Assert.AreEqual(177.041832898393, node.Y);

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ja-JP");
            ViewModel.OpenCommand.Execute(openPath);

            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Nodes.Count);
            node = ViewModel.Model.CurrentWorkspace.Nodes.First();
            Assert.AreEqual(217.952067513811, node.X);
            Assert.AreEqual(177.041832898393, node.Y);

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
        }

        [Test]
        [Category("UnitTests")]
        public void AngleConverter()
        {
            RadianToDegreesConverter converter = new RadianToDegreesConverter();
            double radians = Convert.ToDouble(converter.ConvertBack("90.0", typeof(string), null, new CultureInfo("en-US")));
            Assert.AreEqual(1.57, radians, 0.01);

            radians = Convert.ToDouble(converter.ConvertBack("180.0", typeof(string), null, new CultureInfo("en-US")));
            Assert.AreEqual(3.14, radians, 0.01);

            radians = Convert.ToDouble(converter.ConvertBack("360.0", typeof(string), null, new CultureInfo("en-US")));
            Assert.AreEqual(6.28, radians, 0.01);

            radians = Convert.ToDouble(converter.ConvertBack("-90.0", typeof(string), null, new CultureInfo("en-US")));
            Assert.AreEqual(-1.57, radians, 0.01);

            double degrees = Convert.ToDouble(converter.Convert("-1.570795", typeof(string), null, new CultureInfo("en-US")));
            Assert.AreEqual(-90.0, degrees, 0.01);

            degrees = Convert.ToDouble(converter.Convert("6.28318", typeof(string), null, new CultureInfo("en-US")));
            Assert.AreEqual(360.0, degrees, 0.01);

            degrees = Convert.ToDouble(converter.Convert("3.14159", typeof(string), null, new CultureInfo("en-US")));
            Assert.AreEqual(180.0, degrees, 0.01);
        }
        [Test]
        [Category("UnitTests")]
        public void AngleConverterGerman()
        {
            RadianToDegreesConverter converter = new RadianToDegreesConverter();
            double radians = Convert.ToDouble(converter.ConvertBack("90,0", typeof(string), null, new CultureInfo("de-DE")));
            Assert.AreEqual(1.57, radians, 0.01);

            radians = Convert.ToDouble(converter.ConvertBack("180,0", typeof(string), null, new CultureInfo("de-DE")));
            Assert.AreEqual(3.14, radians, 0.01);

            radians = Convert.ToDouble(converter.ConvertBack("360,0", typeof(string), null, new CultureInfo("de-DE")));

            Assert.AreEqual(6.28, radians, 0.01);

            radians = Convert.ToDouble(converter.ConvertBack("-90,0", typeof(string), null, new CultureInfo("de-DE")));
            Assert.AreEqual(-1.57, radians, 0.01);

            double degrees = Convert.ToDouble(converter.Convert("-1,570795", typeof(string), null, new CultureInfo("de-DE")));
            Assert.AreEqual(-90.0, degrees, 0.01);

            degrees = Convert.ToDouble(converter.Convert("6,28318", typeof(string), null, new CultureInfo("de-DE")));
            Assert.AreEqual(360.0, degrees, 0.01);

            degrees = Convert.ToDouble(converter.Convert("3,14159", typeof(string), null, new CultureInfo("de-DE")));
            Assert.AreEqual(180.0, degrees, 0.01);
        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_3166()
        {
            // Create the node with given information.
            var vm = ViewModel;
            NodeModel node =
                new DSVarArgFunction(vm.Model.LibraryServices.GetFunctionDescriptor("DSCore.List.Join@var[]..[]"));
            vm.ExecuteCommand(new DynCmd.CreateNodeCommand(node, 0, 0, true, false));
            
            var nodeGuid = node.GUID;

            // The node sound be found, and it should be a DSVarArgFunction.
            var workspace = ViewModel.Model.CurrentWorkspace;
            Assert.IsNotNull(node);

            // Delete the node and ensure it is gone.
            vm.ExecuteCommand(new DynCmd.DeleteModelCommand(nodeGuid));
            node = workspace.NodeFromWorkspace(nodeGuid);
            Assert.IsNull(node);

            // Perform undo operation.
            var undoOperation = DynCmd.UndoRedoCommand.Operation.Undo;
            vm.ExecuteCommand(new DynCmd.UndoRedoCommand(undoOperation));

            // Now that deletion is undone, ensure the node exists.
            node = workspace.NodeFromWorkspace(nodeGuid);
            Assert.IsNotNull(node);
            Assert.IsNotNull(node as DSVarArgFunction);
        }
    }
}
