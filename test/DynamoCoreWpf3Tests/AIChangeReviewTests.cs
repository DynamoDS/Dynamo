using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using CoreNodeModels.Input;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.UI.Controls;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;
using ProtoCore.Namespace;

namespace DynamoCoreWpfTests
{
    /// <summary>
    /// Tests for the AI change review affordances (DYN-11007): the AI Assistant badge's tooltip, the
    /// dotted outline on a changed input, and the input's popover with its before and after values,
    /// "Undo this change" and "Next change".
    /// </summary>
    public class AIChangeReviewTests : DynamoTestUIBase
    {
        private const string TurnA = "turn-a";
        private const string TurnB = "turn-b";

        private WorkspaceModel Workspace => ViewModel.Model.CurrentWorkspace;

        private DoubleInput AddNumberNode()
        {
            var node = new DoubleInput();
            Workspace.AddAndRegisterNode(node, false);
            DispatcherUtil.DoEvents();
            return node;
        }

        /// <summary>A Code Block with two input ports, "a" and "b".</summary>
        private CodeBlockNodeModel AddTwoInputNode()
        {
            var node = new CodeBlockNodeModel("a + b;", 0, 0, ViewModel.Model.LibraryServices, new ElementResolver());
            Workspace.AddAndRegisterNode(node, false);
            DispatcherUtil.DoEvents();
            return node;
        }

        private NodeView ViewOf(NodeModel node)
        {
            DispatcherUtil.DoEvents();
            return NodeViewWithGuid(node.GUID.ToString());
        }

        private InPorts PortViewOf(NodeModel node, string portName)
        {
            return ViewOf(node).ChildrenOfType<InPorts>()
                .Single(p => ((InPortViewModel)p.DataContext).PortModel.Name == portName);
        }

        private static InPortViewModel PortViewModelOf(InPorts portView)
        {
            return (InPortViewModel)portView.DataContext;
        }

        /// <summary>Opens a changed input's popover, as keyboard focus or hover does.</summary>
        private static AIChangePopover OpenPopover(InPorts portView)
        {
            portView.ShowAIChangePopover();
            DispatcherUtil.DoEvents();
            return portView.AIChangePopover;
        }

        private static void Click(Button button)
        {
            button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            DispatcherUtil.DoEvents();
        }

        /// <summary>
        /// Runs a turn as DynamoMCP does: one undo group holding its edits, inside a BeginAIEdit
        /// scope, with the changes recorded before the group closes.
        /// </summary>
        private void AsAITurn(Action edit)
        {
            using (Workspace.BeginUndoActionGroup())
            using (Workspace.BeginAIEdit())
            {
                edit();
            }

            DispatcherUtil.DoEvents();
        }

        private void SetValue(NodeModel node, string value)
        {
            ViewModel.Model.ExecuteCommand(new DynamoModel.UpdateModelValueCommand(node.GUID, "Value", value));
        }

        #region Badge tooltip

        [Test]
        [Category("UnitTests")]
        public void PlacedNodeBadgeToolTipSaysPlacedByAutodeskAssistant()
        {
            var node = AddNumberNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);

            Assert.AreEqual("Placed by Autodesk Assistant", ViewOf(node).AIBadgeToolTipText);
        }

        [Test]
        [Category("UnitTests")]
        public void ModifiedNodeBadgeToolTipSaysEditedByAutodeskAssistant()
        {
            var node = AddNumberNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");

            Assert.AreEqual("Edited by Autodesk Assistant", ViewOf(node).AIBadgeToolTipText);
        }

        [Test]
        [Category("UnitTests")]
        public void BadgeToolTipFollowsTheLatestChange()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnB);
            DispatcherUtil.DoEvents();

            Assert.AreEqual("Placed by Autodesk Assistant", ViewOf(node).AIBadgeToolTipText);
        }

        [Test]
        [Category("UnitTests")]
        public void BadgeToolTipOpensOnHoverAndKeyboardFocus()
        {
            var node = AddNumberNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            var badge = ViewOf(node).AIBadge;

            Assert.IsTrue(badge.IsHitTestVisible, "Takes the pointer, so hovering shows the tooltip.");
            Assert.IsTrue(badge.Focusable, "Takes keyboard focus, so focusing shows the tooltip.");
            Assert.IsTrue(ToolTipService.GetShowsToolTipOnKeyboardFocus(badge));
            Assert.IsInstanceOf<DynamoToolTip>(badge.ToolTip);
        }

        #endregion

        #region Changed input outline

        [Test]
        [Category("UnitTests")]
        public void ChangedInputIsOutlinedInPurpleDots()
        {
            var node = AddTwoInputNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "b", "A.out", "B.out");
            DispatcherUtil.DoEvents();

            var outline = PortViewOf(node, "b").AIChangeOutline;
            Assert.IsNotNull(outline);
            Assert.AreEqual(Visibility.Visible, outline.Visibility);
            Assert.IsFalse(outline.IsHitTestVisible, "The outline never takes the port's clicks.");

            var path = outline.ChildrenOfType<Path>().Single();
            Assert.AreEqual(Color.FromRgb(0xA7, 0x6E, 0xF5), ((SolidColorBrush)path.Stroke).Color);
            Assert.AreEqual(PenLineCap.Round, path.StrokeDashCap, "Round caps on zero-length dashes draw dots.");
            CollectionAssert.AreEqual(new[] { 0.0, 2.0 }, path.StrokeDashArray);

            Assert.IsNull(PortViewOf(node, "a").AIChangeOutline, "Inputs the turn did not change are not outlined.");
        }

        [Test]
        [Category("UnitTests")]
        public void ChangedInputIsFocusableOnlyWhileItHasAChange()
        {
            var node = AddTwoInputNode();
            var port = PortViewOf(node, "a");
            Assert.IsFalse(port.Focusable);

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "a", null, "A.out");
            DispatcherUtil.DoEvents();
            Assert.IsTrue(port.Focusable);

            Workspace.ClearAIChanges();
            DispatcherUtil.DoEvents();
            Assert.IsFalse(port.Focusable);
            Assert.AreEqual(Visibility.Collapsed, port.AIChangeOutline.Visibility);
        }

        [Test]
        [Category("UnitTests")]
        public void SettingChangeOutlinesNoInput()
        {
            var node = AddTwoInputNode();

            // A change inside the node's code has no port to show it on (D6)
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Code", "a + b;", "a * b;");
            DispatcherUtil.DoEvents();

            Assert.IsNull(PortViewOf(node, "a").AIChangeOutline);
            Assert.IsNull(PortViewOf(node, "b").AIChangeOutline);
        }

        #endregion

        #region Popover

        [Test]
        [Category("UnitTests")]
        public void PopoverShowsTheInputNameWithBeforeAndAfterValues()
        {
            var node = AddTwoInputNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "b", "A.out", "B.out");
            DispatcherUtil.DoEvents();

            var popover = OpenPopover(PortViewOf(node, "b"));

            Assert.IsTrue(popover.IsOpen);
            Assert.AreEqual("b", popover.InputNameText.Text);
            Assert.AreEqual("A.out", popover.BeforeText.Text);
            Assert.AreEqual("B.out", popover.AfterText.Text);
            Assert.AreEqual("Undo this change", popover.UndoButton.Content);
            Assert.AreEqual("Next change ›", popover.NextButton.Content);
        }

        [Test]
        [Category("UnitTests")]
        public void PopoverShowsNoneForAMissingValue()
        {
            var node = AddTwoInputNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "a", null, "A.out");
            DispatcherUtil.DoEvents();

            var popover = OpenPopover(PortViewOf(node, "a"));

            Assert.AreEqual("None", popover.BeforeText.Text);
            Assert.AreEqual("A.out", popover.AfterText.Text);
        }

        [Test]
        [Category("UnitTests")]
        public void KeyboardFocusOnChangedInputOpensItsPopover()
        {
            var node = AddTwoInputNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "a", null, "A.out");
            DispatcherUtil.DoEvents();
            var port = PortViewOf(node, "a");

            Keyboard.Focus(port);
            DispatcherUtil.DoEvents();

            Assert.IsTrue(port.IsKeyboardFocused);
            Assert.IsNotNull(port.AIChangePopover);
            Assert.IsTrue(port.AIChangePopover.IsOpen);
        }

        [Test]
        [Category("UnitTests")]
        public void PopoverClosesWhenTheChangeIsCleared()
        {
            var node = AddTwoInputNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "a", null, "A.out");
            DispatcherUtil.DoEvents();
            var popover = OpenPopover(PortViewOf(node, "a"));

            Workspace.ClearAIChanges();
            DispatcherUtil.DoEvents();

            Assert.IsFalse(popover.IsOpen);
        }

        [Test]
        [Category("UnitTests")]
        public void UndoThisChangeUndoesTheTurn()
        {
            var number = AddNumberNode();
            var node = AddTwoInputNode();
            SetValue(number, "1");

            AsAITurn(() =>
            {
                SetValue(number, "2");
                Workspace.RecordAINodeChange(number.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");
                Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "a", null, "Number.out");
            });

            var popover = OpenPopover(PortViewOf(node, "a"));
            Assert.IsTrue(popover.UndoButton.IsEnabled);

            Click(popover.UndoButton);

            Assert.AreEqual("1", number.Value, "The whole turn is undone.");
            Assert.IsFalse(popover.IsOpen);
        }

        [Test]
        [Category("UnitTests")]
        public void UndoThisChangeIsDisabledOnceALaterEditIsOnTheUndoStack()
        {
            var number = AddNumberNode();
            var other = AddNumberNode();
            var node = AddTwoInputNode();

            AsAITurn(() =>
            {
                SetValue(number, "2");
                Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "a", null, "Number.out");
            });
            SetValue(other, "5");

            var popover = OpenPopover(PortViewOf(node, "a"));

            Assert.IsFalse(popover.UndoButton.IsEnabled);
            Assert.IsTrue(ToolTipService.GetShowOnDisabled(popover.UndoButton), "The reason shows while disabled.");
        }

        [Test]
        [Category("UnitTests")]
        public void OpenPopoverDisablesUndoThisChangeWhenALaterEditIsRecorded()
        {
            var number = AddNumberNode();
            var other = AddNumberNode();
            var node = AddTwoInputNode();

            AsAITurn(() =>
            {
                SetValue(number, "2");
                Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "a", null, "Number.out");
            });

            var popover = OpenPopover(PortViewOf(node, "a"));
            Assert.IsTrue(popover.UndoButton.IsEnabled);

            // An edit lands on the undo stack while the popover stays open
            SetValue(other, "5");
            DispatcherUtil.DoEvents();

            Assert.IsTrue(popover.IsOpen);
            Assert.IsFalse(popover.UndoButton.IsEnabled, "The button follows the undo stack without reopening.");
            Assert.AreEqual("Only the latest change can be undone from here", popover.UndoButton.ToolTip);
        }

        [Test]
        [Category("UnitTests")]
        public void NextChangeMovesThroughTheTurnsChangedInputsAndWrapsAround()
        {
            var first = AddTwoInputNode();
            var second = AddTwoInputNode();
            Workspace.RecordAINodeChange(first.GUID, AIChangeKind.Modified, TurnA, "a", null, "A.out");
            Workspace.RecordAINodeChange(first.GUID, AIChangeKind.Modified, TurnA, "b", null, "B.out");
            Workspace.RecordAINodeChange(second.GUID, AIChangeKind.Modified, TurnA, "b", null, "C.out");
            DispatcherUtil.DoEvents();

            var firstA = PortViewModelOf(PortViewOf(first, "a"));
            var firstB = PortViewModelOf(PortViewOf(first, "b"));
            var secondB = PortViewModelOf(PortViewOf(second, "b"));
            var firstNode = firstA.NodeViewModel;

            Assert.AreSame(firstB, firstNode.GetNextAIChangedInput(firstA), "Same node first.");
            Assert.AreSame(secondB, firstNode.GetNextAIChangedInput(firstB), "Then the turn's next node.");
            Assert.AreSame(firstA, secondB.NodeViewModel.GetNextAIChangedInput(secondB), "Then back to the start.");
        }

        [Test]
        [Category("UnitTests")]
        public void NextChangeOpensTheNextInputsPopoverAndSelectsItsNode()
        {
            var first = AddTwoInputNode();
            var second = AddTwoInputNode();
            Workspace.RecordAINodeChange(first.GUID, AIChangeKind.Modified, TurnA, "a", null, "A.out");
            Workspace.RecordAINodeChange(second.GUID, AIChangeKind.Modified, TurnA, "b", null, "B.out");
            DispatcherUtil.DoEvents();
            var popover = OpenPopover(PortViewOf(first, "a"));
            Assert.IsTrue(popover.NextButton.IsEnabled);

            Click(popover.NextButton);

            var next = PortViewOf(second, "b");
            Assert.IsFalse(popover.IsOpen);
            Assert.IsNotNull(next.AIChangePopover);
            Assert.IsTrue(next.AIChangePopover.IsOpen);
            Assert.IsTrue(second.IsSelected);
        }

        [Test]
        [Category("UnitTests")]
        public void NextChangeIsDisabledForATurnsOnlyChangedInput()
        {
            var node = AddTwoInputNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "a", null, "A.out");
            DispatcherUtil.DoEvents();

            var popover = OpenPopover(PortViewOf(node, "a"));

            Assert.IsFalse(popover.NextButton.IsEnabled);
        }

        [Test]
        [Category("UnitTests")]
        public void NextChangeSkipsNodesALaterTurnTookOver()
        {
            var first = AddTwoInputNode();
            var second = AddTwoInputNode();
            Workspace.RecordAINodeChange(first.GUID, AIChangeKind.Modified, TurnA, "a", null, "A.out");
            Workspace.RecordAINodeChange(second.GUID, AIChangeKind.Modified, TurnA, "a", null, "B.out");
            Workspace.RecordAINodeChange(second.GUID, AIChangeKind.Modified, TurnB, "b", null, "C.out");
            DispatcherUtil.DoEvents();

            var firstA = PortViewModelOf(PortViewOf(first, "a"));

            Assert.IsNull(firstA.NodeViewModel.GetNextAIChangedInput(firstA), "The second node now shows turn B.");
        }

        #endregion
    }
}
