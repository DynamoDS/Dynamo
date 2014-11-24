using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
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
        public dynNodeView NodeViewOf<T>() where T : NodeModel
        {
            var nodeViews = View.NodeViewsInFirstWorkspace().OfNodeModelType<T>();

            Assert.AreEqual(1, nodeViews.Count(), "Expected a single NodeView of provided type in the workspace!");

            //var models = nodeViews0.Select(x => x.ViewModel.NodeModel);
            //var nodeViews = nodeViews0;

            return nodeViews.First();
        }

        [Test]
        public void Watch3DHasViewer()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewOf<Watch3D>();
            var watchView = nodeView.ChildrenOfType<Watch3DView>().First();
            Assert.AreEqual(0, watchView.Points.Count);
        }

        [Test]
        public void StringInputHasTextboxAndCorrectValue()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewOf<StringInput>();
            var element = nodeView.ChildrenOfType<TextBox>().First();
            Assert.AreEqual("\"ok\"", element.Text);
        }

        [Test]
        public void BoolSelectorHasRadioButtonsAndProperlySetValues()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewOf<BoolSelector>();

            var elements = nodeView.ChildrenOfType<RadioButton>();

            Assert.AreEqual(2, elements.Count());
            Assert.AreEqual(false, elements.First().IsChecked);
            Assert.AreEqual(true, elements.Skip(1).First().IsChecked);
        }

        [Test]
        public void DoubleInputHasTextBoxAndCorrectValue()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewOf<DoubleInput>();

            var element = nodeView.ChildrenOfType<DynamoTextBox>().First();
            Assert.AreEqual("12.000", element.Text);
        }

        [Test]
        public void DoubleSliderHasSliderAndCorrectValues()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewOf<DoubleSlider>();

            var element = nodeView.ChildrenOfType<DynamoSlider>().First();
            Assert.AreEqual(0, element.Value, 1e-6);
        }

        [Test]
        public void IntegerSliderHasSliderAndCorrectValues()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewOf<IntegerSlider>();

            var element = nodeView.ChildrenOfType<DynamoSlider>().First();
            Assert.AreEqual(41, element.Value, 1e-6);
        }

        [Test]
        public void FilenameHasButton()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewOf<Filename>();

            var eles = nodeView.ChildrenOfType<DynamoNodeButton>();
            Assert.AreEqual(1, eles.Count());
        }

        [Test]
        public void DirectoryHasButton()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewOf<DSCore.File.Directory>();

            var eles = nodeView.ChildrenOfType<DynamoNodeButton>();
            Assert.AreEqual(1, eles.Count());
        }

        [Test]
        public void LengthFromStringHasTextBoxWithCorrectValue()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewOf<LengthFromString>();

            var ele = nodeView.ChildrenOfType<DynamoTextBox>().First();
            Assert.AreEqual("0.000m", ele.Text);
        }


        [Test]  
        public void VolumeFromStringHasTextBoxWithCorrectValue()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewOf<VolumeFromString>();

            var ele = nodeView.ChildrenOfType<DynamoTextBox>().First();
            Assert.AreEqual("0.000m³", ele.Text);
        }

        [Test]
        public void AreaFromStringHasTextBoxWithCorrectValue()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewOf<AreaFromString>();

            var ele = nodeView.ChildrenOfType<DynamoTextBox>().First();
            Assert.AreEqual("0.000m²", ele.Text);
        }

        [Test]
        public void PythonNodeHasButtonsAndCorrectNumberOfInputs()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewOf<DoubleInput>();

            var eles = nodeView.inputGrid.ChildrenOfType<DynamoNodeButton>();
            Assert.AreEqual(4, eles.Count());

            var inPortGrid = nodeView.inPortGrid;
            Assert.AreEqual(3, inPortGrid.ChildrenOfType<TextBlock>().Count());
        }

        [Test]
        public void PythonStringNodeHasButtonsAndCorrectNumberOfInputs()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewOf<PythonStringNode>();

            var eles = nodeView.inputGrid.ChildrenOfType<DynamoNodeButton>();
            Assert.AreEqual(4, eles.Count());

            var inPortGrid = nodeView.inPortGrid;
            Assert.AreEqual(4, inPortGrid.ChildrenOfType<TextBlock>().Count());
        }

        [Test]
        public void WatchIsEmptyWhenLoaded()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewOf<Dynamo.Nodes.Watch>();

            var eles = nodeView.ChildrenOfType<WatchTree>();
            Assert.AreEqual(1, eles.Count());

            Assert.AreEqual(0, eles.First().Children().Count());
        }

        // TODO(Peter) Watch tests
        // TODO(Peter) CustomNode tests
        // TODO(Peter) WatchImage tests
    }
}
