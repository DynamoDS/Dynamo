using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.ViewModels;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    public class NodeViewTests : DynamoTestUIBase
    {
        // adapted from: http://stackoverflow.com/questions/9336165/correct-method-for-using-the-wpf-dispatcher-in-unit-tests

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("FunctionObject.ds");            
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("BuiltIn.ds");
            libraries.Add("DSCPython.dll");
            libraries.Add("FFITarget.dll");
        }

        public override void Open(string path)
        {
            base.Open(path);

            DispatcherUtil.DoEvents();
        }

        public override void Run()
        {
            base.Run();

            DispatcherUtil.DoEvents();
        }

        [Test]
        public void ZIndex_Test_MouseDown()
        {
            // Reset zindex to start value.
            Dynamo.ViewModels.NodeViewModel.StaticZIndex = 3;
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("b8c2a62f-f1ce-4327-8d98-c4e0cc0ebed4");

            // Index of first node == 4.
            Assert.AreEqual(4, nodeView.ViewModel.ZIndex);
            Assert.AreEqual(3 + ViewModel.HomeSpace.Nodes.Count(), Dynamo.ViewModels.NodeViewModel.StaticZIndex);
            nodeView.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = Mouse.PreviewMouseDownEvent,
                Source = this,
            });

            // After click node should have The highest index.
            Assert.AreEqual(nodeView.ViewModel.ZIndex, Dynamo.ViewModels.NodeViewModel.StaticZIndex);
        }

        [Test]
        public void ZIndex_Test_MouseEnter_Leave()
        {
            // Reset zindex to start value.
            Dynamo.ViewModels.NodeViewModel.StaticZIndex = 3;
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("b8c2a62f-f1ce-4327-8d98-c4e0cc0ebed4");

            // Index of first node == 4.
            Assert.AreEqual(4, nodeView.ViewModel.ZIndex);
            Assert.AreEqual(3 + ViewModel.HomeSpace.Nodes.Count(), Dynamo.ViewModels.NodeViewModel.StaticZIndex);

            var dispatcherOperation = View.Dispatcher.BeginInvoke(new Action(
                () => nodeView.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0)
                 {
                     RoutedEvent = Mouse.MouseEnterEvent
                 })));
            DispatcherUtil.DoEvents();

            dispatcherOperation.Completed += (s, args) =>
            {
                // After mouse enter node should have the highest index.
                Assert.AreEqual(nodeView.ViewModel.ZIndex, Dynamo.ViewModels.NodeViewModel.StaticZIndex);
            };

            dispatcherOperation = View.Dispatcher.BeginInvoke(new Action(
                () => nodeView.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0)
                {
                    RoutedEvent = Mouse.MouseLeaveEvent
                })));
            DispatcherUtil.DoEvents();

            dispatcherOperation.Completed += (s, args) =>
            {
                // After mouse leave node should have the old index.
                Assert.AreEqual(nodeView.ViewModel.ZIndex, Dynamo.ViewModels.NodeViewModel.StaticZIndex);
                Assert.AreEqual(4, nodeView.ViewModel.ZIndex);
            };
        }

        [Test]
        public void ZIndex_NodeAsMemberOfGroup()
        {
            // Reset zindex to start value.
            Dynamo.ViewModels.NodeViewModel.StaticZIndex = 3;
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("b8c2a62f-f1ce-4327-8d98-c4e0cc0ebed4");

            //Select the node for group
            DynamoSelection.Instance.Selection.Add(nodeView.ViewModel.NodeModel);

            //Create a Group around that node
            ViewModel.AddAnnotationCommand.Execute(null);
            var annotation = ViewModel.HomeSpaceViewModel.Annotations.First();
            Assert.IsNotNull(annotation);

            // Index of first node == 4.
            Assert.AreEqual(4, nodeView.ViewModel.ZIndex);
            Assert.AreEqual(3 + ViewModel.HomeSpace.Nodes.Count(), Dynamo.ViewModels.NodeViewModel.StaticZIndex);

            var dispatcherOperation = View.Dispatcher.BeginInvoke(new Action(
               () => nodeView.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0)
               {
                   RoutedEvent = Mouse.MouseEnterEvent
               })));
            DispatcherUtil.DoEvents();

            dispatcherOperation.Completed += (s, args) =>
            {
                // After mouse enter node should have The highest index.
                Assert.AreEqual(nodeView.ViewModel.ZIndex, Dynamo.ViewModels.NodeViewModel.StaticZIndex);
                Assert.AreEqual(2, annotation.ZIndex);
            };

            dispatcherOperation = View.Dispatcher.BeginInvoke(new Action(
                () => nodeView.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0)
                {
                    RoutedEvent = Mouse.MouseLeaveEvent
                })));
            DispatcherUtil.DoEvents();

            dispatcherOperation.Completed += (s, args) =>
            {
                // After mouse leave node should have the old index.
                Assert.AreEqual(4, nodeView.ViewModel.ZIndex);
                Assert.AreEqual(2, annotation.ZIndex);
            };
        }

        [Test]
        public void NodesHaveCorrectLocationsIndpendentOfCulture()
        {
            string openPath = @"core\nodeLocationTest.dyn";
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("es-AR");
            Open(openPath);

            Assert.AreEqual(1, this.Model.CurrentWorkspace.Nodes.Count());
            var node = this.Model.CurrentWorkspace.Nodes.First();
            Assert.AreEqual(217.952067513811, node.X);
            Assert.AreEqual(177.041832898393, node.Y);

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("zu-ZA");
            Open(openPath);

            Assert.AreEqual(1, this.Model.CurrentWorkspace.Nodes.Count());
            node = this.Model.CurrentWorkspace.Nodes.First();
            Assert.AreEqual(217.952067513811, node.X);
            Assert.AreEqual(177.041832898393, node.Y);

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ja-JP");
            Open(openPath);

            Assert.AreEqual(1, this.Model.CurrentWorkspace.Nodes.Count());
            node = this.Model.CurrentWorkspace.Nodes.First();
            Assert.AreEqual(217.952067513811, node.X);
            Assert.AreEqual(177.041832898393, node.Y);

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
        }

        [Test]
        public void SettingNodeAsInputOrOutputMarksGraphAsModified()
        {
            Open(@"core\isInput_isOutput\IsInput.dyn");

            // Get the node view for a specific node in the graph
            NodeView nodeView = NodeViewWithGuid("c275ece0-2316-4e27-8d51-915257374c1e");

            // Get a reference to the current workspace
            NodeViewModel nodeViewModel = (nodeView.DataContext as NodeViewModel);
            WorkspaceModel ws = nodeViewModel.DynamoViewModel.CurrentSpace;

            // Verify IsSetAsInput is set to true
            Assert.AreEqual(nodeViewModel.IsSetAsInput, true);
            // Verify IsSetAsOutput is set to false
            Assert.AreEqual(nodeViewModel.IsSetAsOutput, false);
            // Verify the graph is not marked as modified
            Assert.AreEqual(ws.HasUnsavedChanges, false);

            // Set IsSetAsInput to false
            nodeViewModel.IsSetAsInput = false;
            // Verify graph is marked as modified
            Assert.AreEqual(nodeViewModel.IsSetAsInput, false);
            Assert.AreEqual(ws.HasUnsavedChanges, true);

            // Save the graph
            string tempPath = Path.Combine(Path.GetTempPath(), "IsInput.dyn");
            ws.Save(tempPath);

            // Verify graph is no longer marked as modified
            Assert.AreEqual(nodeViewModel.IsSetAsInput, false);
            Assert.AreEqual(ws.HasUnsavedChanges, false);

            // Repeat process for IsSetAsOutput //

            // Verify IsSetAsOutput is set to false
            Assert.AreEqual(nodeViewModel.IsSetAsOutput, false);
            // Set IsSetAsOutput to true
            nodeViewModel.IsSetAsOutput = true;

            // Verify graph is marked as modified
            Assert.AreEqual(nodeViewModel.IsSetAsOutput, true);
            Assert.AreEqual(ws.HasUnsavedChanges, true);

            // Save the graph
            ws.Save(tempPath);

            // Verify graph is no longer marked as modified
            Assert.AreEqual(nodeViewModel.IsSetAsOutput, true);
            Assert.AreEqual(ws.HasUnsavedChanges, false);

            // Delete temp file
            File.Delete(tempPath);
        }

        [Test]
        public void SettingOriginalNodeName()
        {
            Open(@"core\originalNodeNameTests\originalNodeName.dyn");

            // Get the node view for a specific node in the graph
            NodeView nodeView = NodeViewWithGuid("37749b95-916a-4dfe-b69b-b516b2ccafe9");

            // Get a reference to the current workspace
            NodeViewModel nodeViewModel = (nodeView.DataContext as NodeViewModel);

            string expectedOriginalName = "List Create";

            Assert.AreEqual(false, nodeViewModel.IsRenamed);
            Assert.AreEqual(nodeViewModel.OriginalName, expectedOriginalName);

            string newName = "NewName";
            nodeViewModel.Name = newName;
            Assert.AreEqual(nodeViewModel.IsRenamed, true);
            Assert.AreEqual(nodeViewModel.Name, newName);
            Assert.AreEqual(nodeViewModel.OriginalName, expectedOriginalName);
        }

        [Test]
        public void CheckDummyNodeName()
        {
            Open(@"core\originalNodeNameTests\originalNodeName.dyn");

            // Get the node view for the dummy node in the graph
            NodeView nodeView = NodeViewWithGuid("94e8d6a8-5463-483c-9fe6-ace668670fae");
            // Get a reference to the dummy node view model
            NodeViewModel nodeViewModel = (nodeView.DataContext as NodeViewModel);

            Assert.AreEqual(false, nodeViewModel.IsRenamed);
            string expectedOriginalName = "Colors.GetAllColors_V1";
            Assert.AreEqual(nodeViewModel.OriginalName, expectedOriginalName);
        }

        [Test]
        public void CheckNotLoadedCustomNodeOriginalName()
        {
            // Custom node inside is not loaded this case so that renamed tag will be disabled
            Open(@"core\originalNodeNameTests\originalNodeName.dyn");

            // Get the node view for the not loaded custom node in the graph
            NodeView nodeView = NodeViewWithGuid("5795dc19-47c9-4084-a5da-df248e03edc4");

            // Get a reference to the custom node view model
            NodeViewModel nodeViewModel = (nodeView.DataContext as NodeViewModel);

            Assert.AreEqual(false, nodeViewModel.IsRenamed);
            string expectedOriginalName = "RootNode_V1";
            Assert.AreEqual(nodeViewModel.OriginalName, expectedOriginalName);
        }

        [Test]
        public void CheckLoadedCustomNodeOriginalName()
        {
            // Opening the dyf first will make sure the custom node is loaded
            // so that rename tag can function correctly in this case
            Open(@"core\custom_node_dep_test\RootNode.dyf");
            Open(@"core\originalNodeNameTests\originalNodeName.dyn");

            // Get the node view for the not loaded custom node in the graph
            NodeView nodeView = NodeViewWithGuid("5795dc19-47c9-4084-a5da-df248e03edc4");

            // Get a reference to the custom node view model
            NodeViewModel nodeViewModel = (nodeView.DataContext as NodeViewModel);

            Assert.AreEqual(true, nodeViewModel.IsRenamed);
            string expectedNewOriginalName = "RootNode";
            Assert.AreEqual(nodeViewModel.OriginalName, expectedNewOriginalName);
        }

        [Test]
        public void SettingOriginalNodeNameOnCustomNode()
        {
            Open(@"core\originalNodeNameTests\originalNodeNameCustomNode.dyn");

            // Get the node view for a specific node in the graph
            NodeView nodeView = NodeViewWithGuid(Guid.Parse("d0b81747d16f48cfbc9992182783d229").ToString());

            // Get a reference to the current workspace
            NodeViewModel nodeViewModel = (nodeView.DataContext as NodeViewModel);

            string expectedOriginalName = "NestedNode";

            Assert.AreEqual(nodeViewModel.IsRenamed, false);
            Assert.AreEqual(nodeViewModel.OriginalName, expectedOriginalName);

            string newName = "NewName";
            nodeViewModel.Name = newName;
            Assert.AreEqual(nodeViewModel.IsRenamed, true);
            Assert.AreEqual(nodeViewModel.Name, newName);
            Assert.AreEqual(nodeViewModel.OriginalName, expectedOriginalName);
        }

        [Test]
        public void InputPortType_NodeModelNode_AreCorrect()
        {
            Open(@"UI\CoreUINodes.dyn");

            // Get the node view for a specific node in the graph
            NodeView nodeView = NodeViewWithGuid(Guid.Parse("9dedd5c5c8b14fbebaea28194fd38c9a").ToString());

            var inPorts = nodeView.ViewModel.InPorts;
            Assert.AreEqual(1, inPorts.Count());

            var port = inPorts[0].PortModel;
            var type = port.GetInputPortType();
            Assert.AreEqual("System.Drawing.Bitmap", type);
        }

        [Test]
        [Category("RegressionTests")]
        public void GettingNodeNameDoesNotTriggerPropertyChangeCycle()
        {
            //add a node
            var numNode = new CoreNodeModels.Input.DoubleInput();
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(numNode, true);

            //subscribe to all property changes
            var nvm = ViewModel.CurrentSpaceViewModel.Nodes.First();
            nvm.PropertyChanged += NodeNameTest_PropChangedHandler;
            //get the node name.
            var temp = nvm.Name;
            nvm.PropertyChanged -= NodeNameTest_PropChangedHandler;
        }

        private void NodeNameTest_PropChangedHandler(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //get the name,this will sometimes cause another propertyChanged event
             var temp = (sender as NodeViewModel).Name;
        }

        /// <summary>
        /// Check if elements are correctly displayed/collapsed on zoom level change 
        /// Current zoom level is 0.4 (hard-coded in multiple Converters
        /// </summary>
        [Test]
        public void ZoomChangeVisibilityTest()
        {
            // Arrange
            Open(@"UI\ZoomNodeColorStates.dyn");

            NodeView nodeViewWarningWarningFrozenHidden = NodeViewWithGuid(Guid.Parse("f0df914c7d4249ffa34301ebf548e490").ToString());

            // Get a reference to the current workspace
            NodeViewModel nodeViewModel = (nodeViewWarningWarningFrozenHidden.DataContext as NodeViewModel);
            WorkspaceViewModel wvm = nodeViewModel.WorkspaceViewModel as WorkspaceViewModel;

            // Zoom out, less than 0.4
            wvm.Zoom = 0.3;
            Assert.AreEqual(nodeViewWarningWarningFrozenHidden.zoomGlyphsGrid.Visibility, System.Windows.Visibility.Visible);   // The Grid containing the Glyphs
            Assert.AreEqual(nodeViewWarningWarningFrozenHidden.nodeColorOverlayZoomOut.Visibility, System.Windows.Visibility.Visible);  // The Color State Border overlay

            // Zoom in, more than 0.4
            wvm.Zoom = 0.6;
            Assert.AreEqual(nodeViewWarningWarningFrozenHidden.zoomGlyphsGrid.Visibility, System.Windows.Visibility.Collapsed);
            Assert.AreEqual(nodeViewWarningWarningFrozenHidden.nodeColorOverlayZoomOut.Visibility, System.Windows.Visibility.Collapsed);  
        }

        [Test]
        public void ZoomWarningFileFromPathTest()
        {
            // Arrange
            Open(@"UI\DisplayImage.dyn");

            NodeView filePathNode = NodeViewWithGuid(Guid.Parse("5a424eaa78c84cffaef5469c034de703").ToString());
            NodeView fileFromPathNode = NodeViewWithGuid(Guid.Parse("eeeadd2b09294b5fbe3ea2668b99777a").ToString());
            NodeView imageReadFromFileNode = NodeViewWithGuid(Guid.Parse("8d82e3934d0e464cb810ddc7389ab0ae").ToString());

            // Get a reference to the NodeViewModels and the current workspace
            NodeViewModel filePathNodeViewModel = filePathNode.DataContext as NodeViewModel;
            NodeViewModel fileFromPathNodeViewModel = (fileFromPathNode.DataContext as NodeViewModel);
            NodeViewModel imageReadFromFileNodeViewModel = imageReadFromFileNode.DataContext as NodeViewModel;

            WorkspaceViewModel wvm = filePathNodeViewModel.WorkspaceViewModel as WorkspaceViewModel;
                        
            // Zoom out, less than 0.4
            wvm.Zoom = 0.3;

            Assert.AreEqual(fileFromPathNode.nodeColorOverlayZoomOut.Visibility, System.Windows.Visibility.Visible);
            Assert.AreEqual(fileFromPathNode.zoomGlyphsGrid.Visibility, System.Windows.Visibility.Visible);            

            // Fix the image path and re run the engine
            filePathNodeViewModel.NodeModel.UpdateValue(new Dynamo.Graph.UpdateValueParams("Value", ".\\Bricks.PNG"));

            wvm.Zoom = 0.6;

            Assert.AreEqual(fileFromPathNode.nodeColorOverlayZoomOut.Visibility, System.Windows.Visibility.Collapsed);
            Assert.AreEqual(fileFromPathNode.zoomGlyphsGrid.Visibility, System.Windows.Visibility.Collapsed);
        }

        /// <summary>
        /// Tests the GetWarningColor method to ensure that the node's WarningBar displays
        /// the proper colors when a node is displaying Info/Warning/Error messages.
        /// </summary>
        [Test]
        public void WarningColorReflectsElementState()
        {
            // Arrange
            Open(@"UI\NodeWarningBarColorTest.dyn");

            // Get the node view for a specific node in the graph
            NodeView nodeViewNoWarningBar = NodeViewWithGuid(Guid.Parse("0ebe50b82c0946e089d99d5aa82bcf9a").ToString());
            NodeView nodeViewNoWarningNoPreview = NodeViewWithGuid(Guid.Parse("d27869a007c848e59c9b337342c6e238").ToString());
            NodeView nodeViewWarningBarWarning = NodeViewWithGuid(Guid.Parse("6bb495c40b88459f9118f0b447d6ddae").ToString());
            NodeView nodeViewWarningBarError = NodeViewWithGuid(Guid.Parse("90007f8f5665438da11b53ccc2707ac2").ToString());

            SolidColorBrush noPreviewBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BBBBBB"));
            SolidColorBrush warningBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FAA21B"));
            SolidColorBrush errorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EB5555"));

            Assert.AreEqual(nodeViewNoWarningBar.ViewModel.IsVisible, true);
            Assert.AreEqual(nodeViewNoWarningBar.ViewModel.GetWarningColor().ToString(), noPreviewBrush.ToString());
            Assert.AreEqual(nodeViewNoWarningNoPreview.ViewModel.IsVisible, false);
            Assert.AreEqual(nodeViewNoWarningNoPreview.ViewModel.GetWarningColor().ToString(), noPreviewBrush.ToString());
            Assert.AreEqual(nodeViewWarningBarWarning.ViewModel.GetWarningColor().ToString(), warningBrush.ToString());
            Assert.AreEqual(nodeViewWarningBarError.ViewModel.GetWarningColor().ToString(), errorBrush.ToString());

            var guid = System.Guid.Parse("90007f8f5665438da11b53ccc2707ac2");
            Model.ExecuteCommand(new DynamoModel.UpdateModelValueCommand(
                Model.CurrentWorkspace.Guid, guid, "Code", "5.6"));
            
            Assert.AreEqual(nodeViewWarningBarError.ViewModel.IsVisible, false);
            Assert.AreEqual(nodeViewWarningBarError.ViewModel.GetWarningColor().ToString(), noPreviewBrush.ToString());
        }

        /// <summary>
        /// Assert node color border to check if colors correspond
        /// to the correct node state for PreviewOff/Frozen/Info/Warning/Error states
        /// </summary>
        [Test]
        public void ZoomNodeColorReflectElementState()
        {
            // Arrange
            Open(@"UI\ZoomNodeColorStates.dyn");

            // Get node views for all possible no-warning states
            NodeView nodeViewNoWarningBar = NodeViewWithGuid(Guid.Parse("a7fe5cbc2ec44870b6f554c82d299b78").ToString());
            NodeView nodeViewNoWarningBarHidden = NodeViewWithGuid(Guid.Parse("72bd1d5f7773426299ecf493586c21fe").ToString());
            NodeView nodeViewNoWarningBarFrozen = NodeViewWithGuid(Guid.Parse("75de711c7ac644e7b91cf7639273acb7").ToString());
            NodeView nodeViewWarningBarHiddenFrozen = NodeViewWithGuid(Guid.Parse("02c8cd1be4ca481ba8645bc9460c7c65").ToString());

            // Get node views for all possible warning states
            NodeView nodeViewWarningBarWarning = NodeViewWithGuid(Guid.Parse("cace32098baa4ab38b7d270bb3d685eb").ToString());
            NodeView nodeViewWarningBarWarningHidden = NodeViewWithGuid(Guid.Parse("2aaadb7f480c42508e7df1e9e1db4f71").ToString());
            NodeView nodeViewWarningBarWarningFrozenHidden = NodeViewWithGuid(Guid.Parse("f0df914c7d4249ffa34301ebf548e490").ToString());

            // Get node views for all possible error states
            NodeView nodeViewWarningBarError = NodeViewWithGuid(Guid.Parse("ae95e072fd744974925ee33843fede81").ToString());
            NodeView nodeViewWarningBarErrorHidden = NodeViewWithGuid(Guid.Parse("110f8e29d54a44ba9126ae10ae31e753").ToString());
            NodeView nodeViewWarningBarErrorFrozenHidden = NodeViewWithGuid(Guid.Parse("eb8182862daa4b70adb4fbcdadf774fd").ToString());

            SolidColorBrush noPreviewBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BBBBBB"));
            SolidColorBrush frozenBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BCD3EE"));
            //TODO add test case for info once it's released
            SolidColorBrush infoBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6AC0E7"));
            SolidColorBrush warningBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FAA21B"));
            SolidColorBrush errorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EB5555"));


            Assert.AreEqual(nodeViewNoWarningBar.ViewModel.IsVisible, true);    
            Assert.AreEqual(nodeViewNoWarningBar.ViewModel.GetBorderColor(), null);
            Assert.AreEqual(nodeViewNoWarningBarHidden.ViewModel.IsVisible, false);
            Assert.AreEqual(nodeViewNoWarningBarHidden.ViewModel.GetBorderColor().ToString(), noPreviewBrush.ToString());
            Assert.AreEqual(nodeViewNoWarningBarFrozen.ViewModel.IsVisible, true);
            Assert.AreEqual(nodeViewNoWarningBarFrozen.ViewModel.GetBorderColor().ToString(), frozenBrush.ToString());
            Assert.AreEqual(nodeViewWarningBarHiddenFrozen.ViewModel.IsVisible, false);
            Assert.AreEqual(nodeViewWarningBarHiddenFrozen.ViewModel.GetBorderColor().ToString(), frozenBrush.ToString());
            
            Assert.AreEqual(nodeViewWarningBarWarning.ViewModel.IsVisible, true);
            Assert.AreEqual(nodeViewWarningBarWarning.ViewModel.GetBorderColor().ToString(), warningBrush.ToString());
            Assert.AreEqual(nodeViewWarningBarWarningHidden.ViewModel.IsVisible, false);
            Assert.AreEqual(nodeViewWarningBarWarningHidden.ViewModel.GetBorderColor().ToString(), warningBrush.ToString());
            Assert.AreEqual(nodeViewWarningBarWarningFrozenHidden.ViewModel.IsVisible, false);
            Assert.AreEqual(nodeViewWarningBarWarningFrozenHidden.ViewModel.GetBorderColor().ToString(), warningBrush.ToString());

            Assert.AreEqual(nodeViewWarningBarError.ViewModel.IsVisible, true);
            Assert.AreEqual(nodeViewWarningBarError.ViewModel.GetBorderColor().ToString(), errorBrush.ToString());
            Assert.AreEqual(nodeViewWarningBarErrorHidden.ViewModel.IsVisible, false);
            Assert.AreEqual(nodeViewWarningBarErrorHidden.ViewModel.GetBorderColor().ToString(), errorBrush.ToString());
            Assert.AreEqual(nodeViewWarningBarErrorFrozenHidden.ViewModel.IsVisible, false);
            Assert.AreEqual(nodeViewWarningBarErrorFrozenHidden.ViewModel.GetBorderColor().ToString(), errorBrush.ToString());
        }

        /// <summary>
        /// Assert node color border to check if colors correspond
        /// to the correct node state for PreviewOff/Frozen/Info/Warning/Error states
        /// </summary>
        [Test]
        public void ZoomNodeGlyphStatesCheck()
        {
            // Arrange
            Open(@"UI\ZoomNodeColorStates.dyn");

            // Get node views for all possible glyph usages
            NodeView nodeViewNoState = NodeViewWithGuid(Guid.Parse("a7fe5cbc2ec44870b6f554c82d299b78").ToString());
            NodeView nodeViewFrozen = NodeViewWithGuid(Guid.Parse("75de711c7ac644e7b91cf7639273acb7").ToString());
            NodeView nodeViewHiddenFrozen = NodeViewWithGuid(Guid.Parse("02c8cd1be4ca481ba8645bc9460c7c65").ToString());
            NodeView nodeViewWarningWarningFrozenHidden = NodeViewWithGuid(Guid.Parse("f0df914c7d4249ffa34301ebf548e490").ToString());

            // Get a reference to the current workspace per node
            NodeViewModel nodeViewModelNoState = (nodeViewNoState.DataContext as NodeViewModel);
            NodeViewModel nodeViewModelFrozen = (nodeViewFrozen.DataContext as NodeViewModel);
            NodeViewModel nodeViewModelHiddenFrozen = (nodeViewHiddenFrozen.DataContext as NodeViewModel);
            NodeViewModel nodeViewModelWarningWarningFrozenHidden = (nodeViewWarningWarningFrozenHidden.DataContext as NodeViewModel);

            // If no states are present, all glyphs should be null
            Assert.AreEqual(nodeViewModelNoState.ImgGlyphOneSource, null);
            Assert.AreEqual(nodeViewModelNoState.ImgGlyphTwoSource, null);
            Assert.AreEqual(nodeViewModelNoState.ImgGlyphThreeSource, null);
            // Only the first glyph should have a value, and it should be frozen
            Assert.AreEqual(nodeViewModelFrozen.ImgGlyphOneSource.Split('/').Last(), "frozen-64px.png");
            Assert.AreEqual(nodeViewModelFrozen.ImgGlyphTwoSource, null);
            Assert.AreEqual(nodeViewModelFrozen.ImgGlyphThreeSource, null);
            // Only the last glyph should be null, the first two will have values
            Assert.AreEqual(nodeViewModelHiddenFrozen.ImgGlyphOneSource.Split('/').Last(), "frozen-64px.png");
            Assert.AreEqual(nodeViewModelHiddenFrozen.ImgGlyphTwoSource.Split('/').Last(), "hidden-64px.png");
            Assert.AreEqual(nodeViewModelHiddenFrozen.ImgGlyphThreeSource, null);
            // All the glyphs should have values
            Assert.AreEqual(nodeViewModelWarningWarningFrozenHidden.ImgGlyphOneSource.Split('/').Last(), "frozen-64px.png");
            Assert.AreEqual(nodeViewModelWarningWarningFrozenHidden.ImgGlyphTwoSource.Split('/').Last(), "hidden-64px.png");
            Assert.AreEqual(nodeViewModelWarningWarningFrozenHidden.ImgGlyphThreeSource.Split('/').Last(), "alert-64px.png");
        }

        [Test]
        public void TestPortColors_NodeModel()
        {
            Open(@"UI\color_range_ports.dyn");
            
            // color range node
            var nvm = NodeViewWithGuid("423d7eaf-9308-4129-b11f-14c186fa4279");

            var portVMs = nvm.ViewModel.InPorts;
            var outPorts = nvm.ViewModel.OutPorts;

            Assert.AreEqual(3, portVMs.Count);
            Assert.AreEqual(1, outPorts.Count);

            Assert.AreEqual(InPortViewModel.PortValueMarkerRed.Color, (portVMs[0] as InPortViewModel).PortValueMarkerColor.Color);
            Assert.AreEqual(InPortViewModel.PortValueMarkerRed.Color, (portVMs[1] as InPortViewModel).PortValueMarkerColor.Color);
            Assert.AreEqual(InPortViewModel.PortValueMarkerBlue.Color, (portVMs[2] as InPortViewModel).PortValueMarkerColor.Color);

            Assert.False((outPorts[0] as OutPortViewModel).PortDefaultValueMarkerVisible);

            // python node
            nvm = NodeViewWithGuid("0be84b72-d0d9-4deb-8fa8-7af9594ec6bc");
            var portVM = nvm.ViewModel.InPorts;
            var outport = nvm.ViewModel.OutPorts;

            Assert.AreEqual(1, portVM.Count);
            Assert.AreEqual(1, outport.Count);

            Assert.AreEqual(InPortViewModel.PortValueMarkerRed.Color, (portVM[0] as InPortViewModel).PortValueMarkerColor.Color);
            Assert.False((outport[0] as OutPortViewModel).PortDefaultValueMarkerVisible);

            Run();
            DispatcherUtil.DoEvents();

            Assert.AreEqual(InPortViewModel.PortValueMarkerBlue.Color, (portVMs[0] as InPortViewModel).PortValueMarkerColor.Color);
            Assert.AreEqual(InPortViewModel.PortValueMarkerBlue.Color, (portVMs[1] as InPortViewModel).PortValueMarkerColor.Color);
            Assert.AreEqual(InPortViewModel.PortValueMarkerBlue.Color, (portVMs[2] as InPortViewModel).PortValueMarkerColor.Color);
            Assert.False((outPorts[0] as OutPortViewModel).PortDefaultValueMarkerVisible);

            Assert.AreEqual(InPortViewModel.PortValueMarkerBlue.Color, (portVM[0] as InPortViewModel).PortValueMarkerColor.Color);
            Assert.False((outport[0] as OutPortViewModel).PortDefaultValueMarkerVisible);

            nvm = NodeViewWithGuid("e7699f76-1481-431e-a3a5-b13fa9ef3358");

            var dportVMs = nvm.ViewModel.InPorts;
            var doutPorts = nvm.ViewModel.OutPorts;

            Assert.AreEqual(InPortViewModel.PortValueMarkerRed.Color, (dportVMs[0] as InPortViewModel).PortValueMarkerColor.Color);
            Assert.True((doutPorts[0] as OutPortViewModel).PortDefaultValueMarkerVisible);
        }

        [Test]
        public void TestPortDefaultValueMarket_Visibility()
        {
            Open(@"UI\outport_valuemarker_portDefaultValueMarkerVisible.dyn");

            var nodeWithFunction = NodeViewWithGuid("e3269c4b-2bab-43d0-b362-f0a589cbe02d");
            var nodeWithOutFunction = NodeViewWithGuid("43985007-e995-494f-b3e7-7c5d6ba317c3");

            var outPorts_Function = nodeWithFunction.ViewModel.OutPorts;
            var outPorts_WithoutFunction = nodeWithOutFunction.ViewModel.OutPorts;

            OutPortViewModel outPort_With_Function = outPorts_Function[0] as OutPortViewModel;
            OutPortViewModel outPort_Without_Function = outPorts_WithoutFunction[0] as OutPortViewModel;

            Assert.AreEqual(outPort_With_Function.ValueMarkerWidth, outPort_With_Function.ValueMarkerWidthWithFunction);
            Assert.AreEqual(outPort_Without_Function.ValueMarkerWidth, outPort_Without_Function.ValueMarkerWidthWithoutFunction);
        }
    }
}
