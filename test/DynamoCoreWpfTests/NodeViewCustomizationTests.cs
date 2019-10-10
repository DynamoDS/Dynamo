using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CoreNodeModels.Input;
using CoreNodeModelsWpf.Controls;
using CoreNodeModelsWpf.Nodes;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    public class NodeViewCustomizationTests : DynamoTestUIBase
    {
        // adapted from: http://stackoverflow.com/questions/9336165/correct-method-for-using-the-wpf-dispatcher-in-unit-tests

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

        [Test, Category("DisplayHardwareDependent")]
        public void Watch3DHasViewer()
        {
            var path = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Process) + ";" + Model.PathManager.DynamoCoreDirectory;
            Environment.SetEnvironmentVariable("Path", path, EnvironmentVariableTarget.Process);

            var renderingTier = (System.Windows.Media.RenderCapability.Tier >> 16);
            if (renderingTier < 2)
            {
                Assert.Inconclusive("Hardware rendering is not available. Watch3D is not created.");
            }

            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("6869c998-b819-4686-8849-6f36162c4182"); // NodeViewOf<Watch3D>();
            var watchView = nodeView.ChildrenOfType<Watch3DView>().FirstOrDefault();
            Assert.NotNull(watchView);
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
        public void doubleInputNodeWillNotAcceptRangeSyntax()
        {
            var number = new DoubleInput();
            Model.AddNodeToCurrentWorkspace(number, true);
            DispatcherUtil.DoEvents();
            var nodeView = NodeViewWithGuid(number.GUID.ToString());
            nodeView.inputGrid.ChildrenOfType<DynamoTextBox>().First().Text = "0..10";
            DispatcherUtil.DoEvents();
            Assert.IsTrue(number.Value != "0..10");
            Assert.IsTrue(number.Value == "0");
            Assert.IsTrue(number.IsInErrorState);

        }

        [Test]
        public void doubleInputNodeWillNotAcceptIds()
        {
            var number = new DoubleInput();
            Model.AddNodeToCurrentWorkspace(number, true);
            DispatcherUtil.DoEvents();
            var nodeView = NodeViewWithGuid(number.GUID.ToString());
            nodeView.inputGrid.ChildrenOfType<DynamoTextBox>().First().Text = "start..end";
            DispatcherUtil.DoEvents();
            Assert.IsTrue(number.Value != "start..end");
            Assert.IsTrue(number.Value == "0");
            Assert.IsTrue(number.IsInErrorState);

        }

        //these tests live here as they have a dependency on CoreNodesWpf which
        //is only accessible at test time after it is loaded via Dynamo 
        [Test]
        [Category("UnitTests")]
        public static void validateNumericFailsOnNonNumericInputs()
        {
            var numericalRule = new NumericValidationRule();

            Assert.IsFalse(numericalRule.Validate("0..10", CultureInfo.InvariantCulture).IsValid);
            Assert.IsFalse(numericalRule.Validate("0..10..2", CultureInfo.InvariantCulture).IsValid);
            Assert.IsFalse(numericalRule.Validate("0..10..5", CultureInfo.InvariantCulture).IsValid);
            Assert.IsFalse(numericalRule.Validate("0..10..#5", CultureInfo.InvariantCulture).IsValid);
            Assert.IsFalse(numericalRule.Validate("0.0..10..0.5", CultureInfo.InvariantCulture).IsValid);
            Assert.IsFalse(numericalRule.Validate("start..end", CultureInfo.InvariantCulture).IsValid);
            Assert.IsFalse(numericalRule.Validate("a..b", CultureInfo.InvariantCulture).IsValid);
            Assert.IsFalse(numericalRule.Validate("a..10", CultureInfo.InvariantCulture).IsValid);
            Assert.IsFalse(numericalRule.Validate("a", CultureInfo.InvariantCulture).IsValid);

        }
        [Test]
        [Category("UnitTests")]
        public static void validateNumericPassesOnNumericInputs()
        {
            var numericalRule = new NumericValidationRule();

            Assert.IsTrue(numericalRule.Validate("10", CultureInfo.InvariantCulture).IsValid);
            Assert.IsTrue(numericalRule.Validate("2147483647", CultureInfo.InvariantCulture).IsValid);
            Assert.IsTrue(numericalRule.Validate("9223372036854775807", CultureInfo.InvariantCulture).IsValid);
            Assert.IsTrue(numericalRule.Validate(".00000000001", CultureInfo.InvariantCulture).IsValid);
            Assert.IsTrue(numericalRule.Validate("100,000,000", CultureInfo.InvariantCulture).IsValid);
            Assert.IsTrue(numericalRule.Validate("1,2", CultureInfo.InvariantCulture).IsValid);

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

            var inputPortControl = nodeView.inputPortControl;
            Assert.AreEqual(6, inputPortControl.ChildrenOfType<TextBlock>().Count());
        }

        [Test]
        public void PythonStringNodeHasButtonsAndCorrectNumberOfInputs()
        {
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("895bcbe1-430f-4105-895a-a686c7cff8aa"); // NodeViewOf<PythonStringNode>();

            var eles = nodeView.inputGrid.ChildrenOfType<DynamoNodeButton>();
            Assert.AreEqual(2, eles.Count());

            var inputPortControl = nodeView.inputPortControl;
            Assert.AreEqual(8, inputPortControl.ChildrenOfType<TextBlock>().Count());
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

       [Test, Category("DisplayHardwareDependent")]
        public void WatchContainsExpectedUiElements()
        {
            OpenAndRun(@"UI\WatchUINodes.dyn");

            var nodeView = NodeViewWithGuid("ed970d46-4fe0-4640-b13b-0fe313f94fe4"); // NodeViewOf<Watch>();

            var tree = nodeView.ChildrenOfType<WatchTree>();
            Assert.AreEqual(1, tree.Count());

            var items = tree.First().treeView1.ChildrenOfType<TextBlock>();
            Assert.AreEqual(8, items.Count());
        }

        [Test, Category("DisplayHardwareDependent")]
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

            var inputPortControl = nodeView.inputPortControl;
            Assert.AreEqual(6, inputPortControl.ChildrenOfType<TextBlock>().Count());

            nodeView = NodeViewWithGuid("2f031397-539e-4df4-bfca-d94d0bd02bc1"); // String.Concat node

            eles = nodeView.inputGrid.ChildrenOfType<DynamoNodeButton>();
            Assert.AreEqual(2, eles.Count());

            inputPortControl = nodeView.inputPortControl;
            Assert.AreEqual(4, inputPortControl.ChildrenOfType<TextBlock>().Count());

            nodeView = NodeViewWithGuid("0cb04cce-1b05-47e0-a73f-ee81af4b7f43"); // List.Join node

            eles = nodeView.inputGrid.ChildrenOfType<DynamoNodeButton>();
            Assert.AreEqual(2, eles.Count());

            inputPortControl = nodeView.inputPortControl;
            Assert.AreEqual(4, inputPortControl.ChildrenOfType<TextBlock>().Count());
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
            var guid0 = Guid.Parse("1a245b04-ad9e-4b9c-8301-730afbd4e6fc");
            var guid1 = Guid.Parse("cece298a-22de-4f4a-a323-fdb04af406a4");

            OpenAndRun(@"UI\InvalidValueShouldNotCrashColorRangeNode.dyn");
            var node0 = Model.CurrentWorkspace.Nodes.First(n => n.GUID == guid0);
            var node1 = Model.CurrentWorkspace.Nodes.First(n => n.GUID == guid1);
            node0.OnNodeModified(); // Mark node as dirty to trigger an immediate run.
            node1.OnNodeModified(); // Mark node as dirty to trigger an immediate run.

            Assert.Pass(); // We should reach here safely without exception.
        }

        [Test, Category("DisplayHardwareDependent")]
        public void WatchConnectDisconnectTest()
        {
            WatchIsEmptyWhenLoaded();
            Run();

            var watchGuid = "ed970d46-4fe0-4640-b13b-0fe313f94fe4";
            var cbnGuid = "b4fc5d9a-4c5a-4dba-b7a0-b2e1d8876d33";
            var anotherNodeGuid = "84509ca2-09bc-4294-82a0-3844021c1a65";

            var nodeView = NodeViewWithGuid(watchGuid);

            var tree = nodeView.ChildrenOfType<WatchTree>();
            Assert.AreEqual(1, tree.Count());

            var items = tree.First().treeView1.ChildrenOfType<TextBlock>();
            // watch is computed with cbn and has its value
            Assert.AreEqual(8, items.Count());

            // disconnect watch
            Model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(watchGuid, 0, PortType.Input,
                DynamoModel.MakeConnectionCommand.Mode.Begin));
            Model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(Guid.Empty, -1, PortType.Input,
                DynamoModel.MakeConnectionCommand.Mode.Cancel));
            // value should be empty
            Assert.AreEqual(0, items.Count());

            // connect another node to watch
            Model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(anotherNodeGuid, 0, PortType.Output,
                DynamoModel.MakeConnectionCommand.Mode.Begin));
            Model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(watchGuid, 0, PortType.Input,
                DynamoModel.MakeConnectionCommand.Mode.End));
            // value should be empty
            Assert.AreEqual(0, items.Count());

            // connect back cbn whose value watch node has
            Model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(cbnGuid, 0, PortType.Output,
                DynamoModel.MakeConnectionCommand.Mode.Begin));
            Model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(watchGuid, 0, PortType.Input,
                DynamoModel.MakeConnectionCommand.Mode.End));

            Run();
            DispatcherUtil.DoEvents();
            tree = nodeView.ChildrenOfType<WatchTree>();
            items = tree.First().treeView1.ChildrenOfType<TextBlock>();
            Assert.AreEqual(8, items.Count());
        }

        [Test]
        public void TestEditReadOnlyCustomNodeProperty()
        {
            // Open a read-only custom node
            var pathInTestsDir = @"core\CustomNodes\add_Read_only.dyf";
            var filePath = Path.Combine(GetTestDirectory(ExecutingDirectory), pathInTestsDir);
            FileInfo fInfo = new FileInfo(filePath);
            fInfo.IsReadOnly = true;
            Assert.IsTrue(DynamoUtilities.PathHelper.IsReadOnlyPath(filePath));

            // a file with a read-only custom node definition is opened
            Open(@"core\CustomNodes\TestAdd.dyn");
            var homeWorkspace = Model.CurrentWorkspace as HomeWorkspaceModel;
            Assert.NotNull(Model.CurrentWorkspace);

            var funcNode = homeWorkspace.Nodes.OfType<Function>().First();
            var customNodeView = NodeViewWithGuid("fb872c7c-21af-4074-8011-818874738dc7");
            foreach (var menuItem in customNodeView.MainContextMenu.Items)
            {
                MenuItem item = menuItem as MenuItem;
                if (item != null && item.Header.ToString() == Dynamo.Wpf.Properties.Resources.ContextMenuEditCustomNodeProperty)
                    Assert.IsFalse(item.IsEnabled);
            }
        }
    }

}
