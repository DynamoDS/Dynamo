using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.Wpf.Controls;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    public class NodeViewCustomizationTests : DynamoTestUIBase
    {
        // adapted from: http://stackoverflow.com/questions/9336165/correct-method-for-using-the-wpf-dispatcher-in-unit-tests

        public NodeView NodeViewOf<T>() where T : NodeModel
        {
            var nodeViews = View.NodeViewsInFirstWorkspace();
            var nodeViewsOfType = nodeViews.OfNodeModelType<T>();
            Assert.AreEqual(1, nodeViewsOfType.Count(), "Expected a single NodeView of provided type in the workspace!");

            return nodeViewsOfType.First();
        }

        public NodeView NodeViewWithGuid(string guid)
        {
            var nodeViews = View.NodeViewsInFirstWorkspace();
            var nodeViewsOfType = nodeViews.Where(x => x.ViewModel.NodeLogic.GUID.ToString() == guid);
            Assert.AreEqual(1, nodeViewsOfType.Count(), "Expected a single NodeView with guid: " + guid);

            return nodeViewsOfType.First();
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

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("ProtoGeometry.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void Watch3DHasViewer()
        {
            var renderingTier = (System.Windows.Media.RenderCapability.Tier >> 16);
            if (renderingTier < 2)
            {
                Assert.Inconclusive("Hardware rendering is not available. Watch3D is not created.");
            }

            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("6869c998-b819-4686-8849-6f36162c4182"); // NodeViewOf<Watch3D>();
            var watchView = nodeView.ChildrenOfType<Watch3DView>().First();
            Assert.Null(watchView.Points);
        }

        [Test]
        public void StringInputHasTextboxAndCorrectValue()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("3d436f17-cc3d-4b84-afd9-fc71ff538b3b"); // NodeViewOf<StringInput>();
            var element = nodeView.ChildrenOfType<TextBox>().First();
            Assert.AreEqual("\"ok\"", element.Text);
        }

        [Test]
        public void BoolSelectorHasRadioButtonsAndProperlySetValues()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("5792b7bc-a30c-4bda-bf55-a5888a73a08b"); // NodeViewOf<BoolSelector>();

            var elements = nodeView.ChildrenOfType<RadioButton>();

            Assert.AreEqual(2, elements.Count());
            Assert.AreEqual(false, elements.First().IsChecked);
            Assert.AreEqual(true, elements.Skip(1).First().IsChecked);
        }

        [Test]
        public void DoubleInputHasTextBoxAndCorrectValue()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("85b95961-5ac1-482f-b520-82d5dc115e97"); // NodeViewOf<DoubleInput>();

            var element = nodeView.ChildrenOfType<DynamoTextBox>().First();
            Assert.AreEqual("12.000", element.Text);
        }

        [Test]
        public void DoubleSliderHasSliderAndCorrectValues()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("8820644a-ba01-4118-8f04-26ebf58c11cc"); // NodeViewOf<DoubleSlider>();

            var element = nodeView.ChildrenOfType<DynamoSlider>().First();
            Assert.AreEqual(1.0, element.slider.Value, 1e-6);
        }

        [Test]
        public void IntegerSliderHasSliderAndCorrectValues()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("d0fa1feb-ec0e-4cee-a86a-34077f5a870a"); // NodeViewOf<IntegerSlider>();

            var element = nodeView.ChildrenOfType<DynamoSlider>().First();
            Assert.AreEqual(41, element.slider.Value, 1e-6);
        }

        [Test]
        public void FilenameHasButton()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("32013fd9-f925-4510-b449-3e0b3d8a5887"); // NodeViewOf<Filename>();

            var eles = nodeView.ChildrenOfType<DynamoNodeButton>();
            Assert.AreEqual(1, eles.Count());
        }

        [Test]
        public void DirectoryHasButton()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("4e50a79d-783f-4c54-9961-c0a9ee3214e1");
                // NodeViewOf<DSCore.File.Directory>();

            var eles = nodeView.ChildrenOfType<DynamoNodeButton>();
            Assert.AreEqual(1, eles.Count());
        }

        [Test]
        public void LengthFromStringHasTextBoxWithCorrectValue()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("41a95cc4-1224-4390-be2a-09968143db7c"); // NodeViewOf<LengthFromString>();

            var ele = nodeView.ChildrenOfType<DynamoTextBox>().First();

            // When LengthFromString became Number From Feet and Inches, we locked
            // its LengthUnit to FractionalFoot. Unlike the AreaFromString and VolumeFromString
            // nodes, this will always visualize as fractional feet and inches.
            Assert.AreEqual("0' 0\"", ele.Text);
        }


        [Test]
        public void VolumeFromStringHasTextBoxWithCorrectValue()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("0b0a7eda-f487-4582-90bc-d75f1871627c"); // NodeViewOf<VolumeFromString>();

            var ele = nodeView.ChildrenOfType<DynamoTextBox>().First();
            Assert.AreEqual("0.000m³", ele.Text);
        }

        [Test]
        public void AreaFromStringHasTextBoxWithCorrectValue()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("739f111c-886c-4313-8517-5cfdf4a2e559"); // NodeViewOf<AreaFromString>();

            var ele = nodeView.ChildrenOfType<DynamoTextBox>().First();
            Assert.AreEqual("0.000m²", ele.Text);
        }

        [Test]
        public void PythonNodeHasButtonsAndCorrectNumberOfInputs()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("23aaaf18-2c93-4b78-85f9-6f348a932e75"); // NodeViewOf<PythonNode>();

            var eles = nodeView.inputGrid.ChildrenOfType<DynamoNodeButton>();
            Assert.AreEqual(2, eles.Count());

            var inPortGrid = nodeView.inPortGrid;
            Assert.AreEqual(3, inPortGrid.ChildrenOfType<TextBlock>().Count());
        }

        [Test]
        public void PythonStringNodeHasButtonsAndCorrectNumberOfInputs()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("895bcbe1-430f-4105-895a-a686c7cff8aa"); // NodeViewOf<PythonStringNode>();

            var eles = nodeView.inputGrid.ChildrenOfType<DynamoNodeButton>();
            Assert.AreEqual(2, eles.Count());

            var inPortGrid = nodeView.inPortGrid;
            Assert.AreEqual(4, inPortGrid.ChildrenOfType<TextBlock>().Count());
        }

        [Test]
        public void WatchIsEmptyWhenLoaded()
        {
            Open(@"UI\WatchUINodes.dyn");

            var nodeView = NodeViewWithGuid("ed970d46-4fe0-4640-b13b-0fe313f94fe4"); // NodeViewOf<Watch>();

            var tree = nodeView.ChildrenOfType<WatchTree>();
            Assert.AreEqual(1, tree.Count());

            var items = tree.First().treeView1.ChildrenOfType<TextBlock>();
            Assert.AreEqual(0, items.Count());
        }

        [Test]
        public void WatchContainsExpectedUiElements()
        {
            OpenAndRun(@"UI\WatchUINodes.dyn");

            var nodeView = NodeViewWithGuid("ed970d46-4fe0-4640-b13b-0fe313f94fe4"); // NodeViewOf<Watch>();

            var tree = nodeView.ChildrenOfType<WatchTree>();
            Assert.AreEqual(1, tree.Count());

            var items = tree.First().treeView1.ChildrenOfType<TextBlock>();
            Assert.AreEqual(8, items.Count());
        }

        [Test]
        public void WatchImageCoreContainsImage()
        {
            OpenAndRun(@"UI\WatchUINodes.dyn");

            var nodeView = NodeViewWithGuid("cf3ed4fb-f0a2-4dfe-89c1-11e8bbcfe80d");
                // NodeViewOf<Dynamo.Nodes.WatchImageCore>();

            var imgs = nodeView.ChildrenOfType<Image>();

            Assert.AreEqual(1, imgs.Count());

            var img = imgs.First();

            Assert.Greater(img.ActualWidth, 10);
            Assert.Greater(img.ActualHeight, 10);
        }

        [Test, Category("Failure")]
        public void Watch3DContainsExpectedGeometry()
        {
            OpenAndRun(@"UI\WatchUINodes.dyn");

            var nodeView = NodeViewWithGuid("6edc4c28-15ef-4d60-af6d-6ed829871973");
                // NodeViewOf<Dynamo.Nodes.Watch3D>();

            var watch3ds = nodeView.ChildrenOfType<Watch3DView>();

            Assert.AreEqual(1, watch3ds.Count());

            var watch3DView = watch3ds.First();

            Assert.AreEqual(1, watch3DView.Points.Positions.Count);
        }

        [Test]
        public void CustomNodeIsCustomized()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("b97925f7-86d8-44fc-8e57-5768593d6b4e");
                // NodeViewOf<Dynamo.Nodes.Function>();

            Assert.True(nodeView.customNodeBorder0.Visibility == Visibility.Visible);
        }

        [Test]
        public void DSVarArgFunctionNodeHasButtons()
        {
            Open(@"UI\VariableInputNodes.dyn");

            var nodeView = NodeViewWithGuid("0abcef04-75e7-4264-b387-602aad74e34d"); // String.Join node

            var eles = nodeView.inputGrid.ChildrenOfType<DynamoNodeButton>();
            Assert.AreEqual(2, eles.Count());

            var inPortGrid = nodeView.inPortGrid;
            Assert.AreEqual(3, inPortGrid.ChildrenOfType<TextBlock>().Count());

            nodeView = NodeViewWithGuid("2f031397-539e-4df4-bfca-d94d0bd02bc1"); // String.Concat node

            eles = nodeView.inputGrid.ChildrenOfType<DynamoNodeButton>();
            Assert.AreEqual(2, eles.Count());

            inPortGrid = nodeView.inPortGrid;
            Assert.AreEqual(2, inPortGrid.ChildrenOfType<TextBlock>().Count());

            nodeView = NodeViewWithGuid("0cb04cce-1b05-47e0-a73f-ee81af4b7f43"); // List.Join node

            eles = nodeView.inputGrid.ChildrenOfType<DynamoNodeButton>();
            Assert.AreEqual(2, eles.Count());

            inPortGrid = nodeView.inPortGrid;
            Assert.AreEqual(2, inPortGrid.ChildrenOfType<TextBlock>().Count());
        }

        [Test]
        public void TestColorRangeNodeOnMultipleFilesWithoutClosing()
        {
            Open(@"UI\UIColorRange.dyn");

            var nodeView = NodeViewWithGuid("72a66222-5a7d-4dce-b695-5e18b5a93bc3");
            var image = nodeView.inputGrid.ChildrenOfType<Image>().FirstOrDefault();
            Assert.IsNotNull(image);
            Assert.IsNotNull(image.Source);

            //Open another or same ColorRange file
            Open(@"UI\UIColorRange2.dyn");

            nodeView = NodeViewWithGuid("19b8c58f-e518-41e3-979b-811a029c1ddf");
            image = nodeView.inputGrid.ChildrenOfType<Image>().FirstOrDefault();
            Assert.IsNotNull(image);
            Assert.IsNotNull(image.Source);
        }

        [Test]
        public void InvalidInputShouldNotCrashColorRangeNode()
        {
            Open(@"UI\ColorRangeInvalidInputCrash.dyn");

            // Update the code block node from "Color.FromRGBA" to "5.6", making 
            // it a double value type. After the fix this should not have caused 
            // any crash in ColorRange node.
            //
            var guid = System.Guid.Parse("c1d3a92a-e4d4-47a8-8533-bf19e63e0bf9");
            Model.ExecuteCommand(new DynamoModel.UpdateModelValueCommand(
                Model.CurrentWorkspace.Guid, guid, "Code", "5.6"));
        }

        [Test]
        public void ArrayExprShouldNotCrashColorRangeNode()
        {
            var guid = System.Guid.Parse("c90f5c20-8c63-4708-bd1a-289647bae471");

            OpenAndRun(@"UI\ArrayExprShouldNotCrashColorRangeNode.dyn");
            var nodes = Model.CurrentWorkspace.Nodes.Where(n => n.GUID == guid);
            var node = nodes.ElementAt(0) as CodeBlockNodeModel;
            node.OnNodeModified(); // Mark node as dirty to tigger an immediate run.

            Assert.Pass(); // We should reach here safely without exception.
        }

        [Test]
        public void InvalidValueShouldNotCrashColorRangeNode()
        {
            var guid0 = System.Guid.Parse("1a245b04-ad9e-4b9c-8301-730afbd4e6fc");
            var guid1 = System.Guid.Parse("cece298a-22de-4f4a-a323-fdb04af406a4");

            OpenAndRun(@"UI\InvalidValueShouldNotCrashColorRangeNode.dyn");
            var nodes0 = Model.CurrentWorkspace.Nodes.Where(n => n.GUID == guid0);
            var nodes1 = Model.CurrentWorkspace.Nodes.Where(n => n.GUID == guid0);
            var node0 = nodes0.ElementAt(0) as CodeBlockNodeModel;
            var node1 = nodes0.ElementAt(0) as CodeBlockNodeModel;
            node0.OnNodeModified(); // Mark node as dirty to tigger an immediate run.
            node1.OnNodeModified(); // Mark node as dirty to tigger an immediate run.

            Assert.Pass(); // We should reach here safely without exception.
        }
    }
}
