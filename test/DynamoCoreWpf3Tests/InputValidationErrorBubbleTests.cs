using CoreNodeModels.Input;
using CoreNodeModels.Properties;
using CoreNodeModelsWpf.Controls;
using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using DateTimeNode = CoreNodeModels.Input.DateTime;

namespace DynamoCoreWpfTests
{
    public class InputValidationErrorBubbleTests : DynamoTestUIBase
    {
        protected override void GetLibrariesToPreload(System.Collections.Generic.List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        private static DynamoTextBox ValueBox(NodeView nodeView)
        {
            // Number / DateTime: single box. Sliders: ValTb.
            var named = nodeView.ChildrenOfType<DynamoTextBox>()
                .FirstOrDefault(tb => tb.Name == "ValTb" || tb.Name == "DateTimeTb");
            return named ?? nodeView.inputGrid.ChildrenOfType<DynamoTextBox>().First();
        }

        private static int ErrorCount(NodeModel node) => node.NodeInfos.Count(i => i.State == ElementState.Error);

        private NodeView AddAndGetView(NodeModel node)
        {
            Model.AddNodeToCurrentWorkspace(node, true);
            DispatcherUtil.DoEventsLoop(() => View.NodeViewsInFirstWorkspace().Any(nv => nv.ViewModel.NodeLogic.GUID == node.GUID), timeoutSeconds: 5);
            return NodeViewWithGuid(node.GUID.ToString());
        }

        private void CommitText(DynamoTextBox box, string text)
        {
            box.Text = text; // triggers DynamoTextBox.UpdateDataSource(recordForUndo: true)
            DispatcherUtil.DoEvents();
        }

        #region Number (DoubleInput)

        [Test]
        public void WhenNumberGetsInvalidInputThenKeepsValueShowsErrorKeepsTextAndSkipsUndo()
        {
            var node = new DoubleInput();
            var view = AddAndGetView(node);
            var box = ValueBox(view);

            CommitText(box, "1");
            CommitText(box, "2");
            CommitText(box, "K");

            Assert.AreEqual("2", node.Value);
            Assert.AreEqual("K", box.Text);
            Assert.IsTrue(node.IsInErrorState);
            Assert.AreEqual(1, ErrorCount(node));
            Assert.AreEqual(Resources.NumberNodeInputMustBeNumeric,
                node.NodeInfos.Single(i => i.State == ElementState.Error).Message);

            Model.CurrentWorkspace.Undo();
            DispatcherUtil.DoEvents();

            // Invalid "K" was not recorded — undo reverses 1→2, not 2→K.
            Assert.AreEqual("1", node.Value);
            Assert.IsFalse(node.IsInErrorState);
            Assert.AreEqual(0, ErrorCount(node));
            Assert.AreEqual("1.000", box.Text);
        }

        [Test]
        public void WhenNumberGetsValidInputAfterErrorThenClearsBubbleAndUpdatesValue()
        {
            var node = new DoubleInput();
            var view = AddAndGetView(node);
            var box = ValueBox(view);

            CommitText(box, "5");
            CommitText(box, "K");
            Assert.IsTrue(node.IsInErrorState);

            CommitText(box, "7");

            Assert.AreEqual("7", node.Value);
            Assert.AreEqual("7.000", box.Text);
            Assert.IsFalse(node.IsInErrorState);
            Assert.AreEqual(0, ErrorCount(node));
        }

        [Test]
        public void WhenNumberSwapsInvalidInputsThenShowsOnlyLatestError()
         {
            var node = new DoubleInput();
            var view = AddAndGetView(node);
            var box = ValueBox(view);

            CommitText(box, "1");
            CommitText(box, "K");
            CommitText(box, "m");

            Assert.AreEqual("1", node.Value);
            Assert.AreEqual("m", box.Text);
            Assert.AreEqual(1, ErrorCount(node));
            Assert.AreEqual(Resources.NumberNodeInputMustBeNumeric,
                node.NodeInfos.Single(i => i.State == ElementState.Error).Message);
        }

        [Test]
        public void WhenNumberUndoesAfterErrorOnSameValueThenClearsBubble()
        {
            // Repro: Value setter early-returns when restored string equals current;
            // DeserializeCore must clear Infos unconditionally.
            var node = new DoubleInput();
            AddAndGetView(node);

            Model.ExecuteCommand(new DynamoModel.UpdateModelValueCommand(
                Model.CurrentWorkspace.Guid, node.GUID, nameof(DoubleInput.Value), "5"));
            Model.ExecuteCommand(new DynamoModel.UpdateModelValueCommand(
                Model.CurrentWorkspace.Guid, node.GUID, nameof(DoubleInput.Value), "5"));

            var view = NodeViewWithGuid(node.GUID.ToString());
            CommitText(ValueBox(view), "K");
            Assert.IsTrue(node.IsInErrorState);

            Model.CurrentWorkspace.Undo();
            DispatcherUtil.DoEvents();

            Assert.AreEqual("5", node.Value);
            Assert.IsFalse(node.IsInErrorState);
            Assert.AreEqual(0, ErrorCount(node));
        }

        #endregion

        #region Integer Slider 64-bit

        [Test]
        public void WhenIntegerSlider64GetsInvalidInputThenKeepsValueShowsErrorAndSkipsUndo()
        {
            var node = new IntegerSlider64Bit();
            var view = AddAndGetView(node);
            var box = ValueBox(view);

            CommitText(box, "1");
            CommitText(box, "2");
            CommitText(box, "K");

            Assert.AreEqual(2L, node.Value);
            Assert.AreEqual("K", box.Text);
            Assert.IsTrue(node.IsInErrorState);
            Assert.AreEqual(1, ErrorCount(node));

            Model.CurrentWorkspace.Undo();
            DispatcherUtil.DoEvents();

            Assert.AreEqual(1L, node.Value);
            Assert.IsFalse(node.IsInErrorState);
            Assert.AreEqual("1", box.Text);
        }

        [Test]
        public void WhenIntegerSlider64GetsThousandsGroupedInputThenAcceptsValue()
        {
            var node = new IntegerSlider64Bit { Max = 10000 };
            var view = AddAndGetView(node);
            var box = ValueBox(view);

            CommitText(box, "2,500");

            Assert.AreEqual(2500L, node.Value);
            Assert.IsFalse(node.IsInErrorState);
            Assert.AreEqual(0, ErrorCount(node));
        }

        [Test]
        public void WhenIntegerSlider64OverflowsThenShowsRangeErrorNotStacked()
        {
            var node = new IntegerSlider64Bit();
            var view = AddAndGetView(node);
            var box = ValueBox(view);

            CommitText(box, "1");
            CommitText(box, "m");
            Assert.AreEqual(Resources.IntegerSliderInputMustBeInteger,
                node.NodeInfos.Single(i => i.State == ElementState.Error).Message);

            CommitText(box, "9223372036854775808"); // long.MaxValue + 1

            Assert.AreEqual(1L, node.Value);
            Assert.AreEqual(1, ErrorCount(node));
            Assert.AreEqual(Resources.IntegerSliderInfoMessage,
                node.NodeInfos.Single(i => i.State == ElementState.Error).Message);
        }

        #endregion

        #region Double Slider

        [Test]
        public void WhenDoubleSliderGetsInvalidInputThenKeepsValueShowsErrorAndClearsOnValid()
        {
            var node = new CoreNodeModels.Input.DoubleSlider();
            var view = AddAndGetView(node);
            var box = ValueBox(view);

            CommitText(box, "3.5");
            CommitText(box, "abc");

            Assert.AreEqual(3.5, node.Value, 1e-9);
            Assert.AreEqual("abc", box.Text);
            Assert.IsTrue(node.IsInErrorState);

            CommitText(box, "4.25");

            Assert.AreEqual(4.25, node.Value, 1e-9);
            Assert.IsFalse(node.IsInErrorState);
            Assert.AreEqual(0, ErrorCount(node));
        }

        #endregion

        #region DateTime

        [Test]
        public void WhenDateTimeGetsInvalidInputThenKeepsValueShowsErrorAndClearsOnUndo()
        {
            var node = new DateTimeNode
            {
                // DefaultDateFormat carries only minutes, so we need a value that round-trips
                // exactly through serialize/undo. UtcNow would lose seconds and ticks.
                Value = new System.DateTime(2015, 5, 30, 5, 30, 0, System.DateTimeKind.Utc)
            };
            var original = node.Value;
            var view = AddAndGetView(node);
            var box = ValueBox(view);

            var valid = new System.DateTime(2020, 12, 8, 12, 0, 0, System.DateTimeKind.Utc);
            CommitText(box, valid.ToString(PreferenceSettings.DefaultDateFormat, CultureInfo.InvariantCulture));
            Assert.AreEqual(valid, node.Value);

            CommitText(box, "not-a-date");

            Assert.AreEqual(valid, node.Value);
            Assert.AreEqual("not-a-date", box.Text);
            Assert.IsTrue(node.IsInErrorState);
            Assert.AreEqual(Resources.DateTimeNodeInputInvalidFormat,
                node.NodeInfos.Single(i => i.State == ElementState.Error).Message);

            Model.CurrentWorkspace.Undo();
            DispatcherUtil.DoEvents();

            Assert.AreEqual(original, node.Value);
            Assert.IsFalse(node.IsInErrorState);
            Assert.AreEqual(0, ErrorCount(node));
        }

        #endregion

        #region Legacy IntegerSlider (32-bit) binding regression

        [Test]
        public void WhenIntegerSliderCustomizedThenTextBoxesAreBoundAndPopulated()
        {
            var node = new CoreNodeModels.Input.IntegerSlider64Bit { Value = 41, Min = 0, Max = 100, Step = 1 };
            var view = AddAndGetView(node);
            var slider = view.ChildrenOfType<DynamoSlider>().First();

            foreach (var name in new[] { "ValTb", "MinTb", "MaxTb", "StepTb" })
            {
                var tb = (DynamoTextBox)slider.FindName(name);
                Assert.IsNotNull(tb, $"{name} missing");
                Assert.IsNotNull(
                    BindingOperations.GetBindingExpression(tb, TextBox.TextProperty),
                    $"{name} has no Text binding");
            }

            Assert.AreEqual("41", ((DynamoTextBox)slider.FindName("ValTb")).Text);
            Assert.AreEqual("0", ((DynamoTextBox)slider.FindName("MinTb")).Text);
            Assert.AreEqual("100", ((DynamoTextBox)slider.FindName("MaxTb")).Text);
            Assert.AreEqual("1", ((DynamoTextBox)slider.FindName("StepTb")).Text);
        }

        #endregion
    }
}
