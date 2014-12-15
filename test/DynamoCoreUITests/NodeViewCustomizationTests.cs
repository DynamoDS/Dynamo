using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DSCore.File;
using DSCoreNodesUI;
using DSIronPythonNode;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using DynamoCoreUITests.Utility;
using NUnit.Framework;
using Dynamo.Utilities;
using UnitsUI;

namespace DynamoCoreUITests
{
    public class NodeViewCustomizationTests : DynamoTestUIBase
    {
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

        [Test]
        public void Watch3DHasViewer()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("6869c998-b819-4686-8849-6f36162c4182"); // NodeViewOf<Watch3D>();
            var watchView = nodeView.ChildrenOfType<Watch3DView>().First();
            Assert.AreEqual(0, watchView.Points.Count);
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
            Assert.AreEqual(1.0, element.Value, 1e-6);
        }

        [Test]
        public void IntegerSliderHasSliderAndCorrectValues()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("d0fa1feb-ec0e-4cee-a86a-34077f5a870a"); // NodeViewOf<IntegerSlider>();

            var element = nodeView.ChildrenOfType<DynamoSlider>().First();
            Assert.AreEqual(41, element.Value, 1e-6);
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

            var nodeView = NodeViewWithGuid("4e50a79d-783f-4c54-9961-c0a9ee3214e1"); // NodeViewOf<DSCore.File.Directory>();

            var eles = nodeView.ChildrenOfType<DynamoNodeButton>();
            Assert.AreEqual(1, eles.Count());
        }

        [Test]
        public void LengthFromStringHasTextBoxWithCorrectValue()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("41a95cc4-1224-4390-be2a-09968143db7c"); // NodeViewOf<LengthFromString>();

            var ele = nodeView.ChildrenOfType<DynamoTextBox>().First();
            Assert.AreEqual("0.000m", ele.Text);
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

            Action assert = () =>
            {
                var items = tree.First().treeView1.ChildrenOfType<TextBlock>();
                Assert.AreEqual(8, items.Count());
            };

            AssertWhenDispatcherDone(assert);
        }

        [Test]
        public void WatchImageCoreContainsImage()
        {
            OpenAndRun(@"UI\WatchUINodes.dyn");

            var nodeView = NodeViewWithGuid("cf3ed4fb-f0a2-4dfe-89c1-11e8bbcfe80d"); // NodeViewOf<Dynamo.Nodes.WatchImageCore>();

            var imgs = nodeView.ChildrenOfType<Image>();

            Assert.AreEqual(1, imgs.Count());

            var img = imgs.First();

            Action assert = () =>
            {
                Assert.Greater(img.ActualWidth, 10);
                Assert.Greater(img.ActualHeight, 10);
            };

            AssertWhenDispatcherDone(assert);
        }

        [Test]
        public void Watch3DContainsExpectedGeometry()
        {
            OpenAndRun(@"UI\WatchUINodes.dyn");

            var nodeView = NodeViewWithGuid("6edc4c28-15ef-4d60-af6d-6ed829871973"); // NodeViewOf<Dynamo.Nodes.Watch3D>();

            var watch3ds = nodeView.ChildrenOfType<Watch3DView>();

            Assert.AreEqual(1, watch3ds.Count());

            var watch3DView = watch3ds.First();

            Action assert = () => Assert.AreEqual(1, watch3DView.Points.Count);
            AssertWhenDispatcherDone(assert);
            
        }

        [Test]
        public void CustomNodeIsCustomized()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("b97925f7-86d8-44fc-8e57-5768593d6b4e"); // NodeViewOf<Dynamo.Nodes.Function>();

            Assert.True( nodeView.customNodeBorder0.Visibility == Visibility.Visible);
        }
    }
}
