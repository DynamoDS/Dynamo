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
            Assert.IsInstanceOf<ToolTip>(badge.ToolTip);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenBadgeToolTipShowsThenItIsWhiteTextOnDarkGreyBelowTheNode()
        {
            var node = AddNumberNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            var toolTip = (ToolTip)ViewOf(node).AIBadge.ToolTip;

            // Figma 8:304
            var box = (Border)toolTip.Template.LoadContent();
            Assert.AreEqual(Color.FromRgb(0x33, 0x33, 0x33), ((SolidColorBrush)box.Background).Color);
            Assert.AreEqual(Colors.White, ((SolidColorBrush)((TextBlock)toolTip.Content).Foreground).Color);
            Assert.AreEqual(PlacementMode.Bottom, toolTip.Placement);
            Assert.AreSame(ViewOf(node).nodeBorder, toolTip.PlacementTarget, "Lined up under the node, not the badge.");
        }

        [Test]
        [Category("UnitTests")]
        public void WhenAIBadgeIsPressedThenItsNodeIsSelectedAndDragged()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            var badgeImage = ViewOf(node).AIBadge.ChildrenOfType<Image>().Single();
            var workspaceViewModel = ViewModel.CurrentSpaceViewModel;

            try
            {
                // Most of the badge is outside the node's bounds, where a press on the canvas
                // would clear the selection and start a selection box
                badgeImage.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                {
                    RoutedEvent = Mouse.MouseDownEvent
                });
                DispatcherUtil.DoEvents();

                Assert.IsTrue(node.IsSelected, "Pressing the badge selects its node and keeps it selected.");
                Assert.IsTrue(workspaceViewModel.IsDragging, "Pressing the badge starts dragging its node.");
                Assert.IsFalse(workspaceViewModel.IsSelecting, "Pressing the badge does not start a selection box.");
            }
            finally
            {
                workspaceViewModel.CancelActiveState();
                Mouse.Capture(null);
            }
        }

        #endregion

        #region Keyboard review

        /// <summary>
        /// Presses a key on whatever has keyboard focus, as WPF delivers it: PreviewKeyDown, then
        /// KeyDown unless the preview was handled.
        /// </summary>
        /// <returns>Whether the key press was handled.</returns>
        private static bool PressKey(Key key)
        {
            var target = (UIElement)Keyboard.FocusedElement;
            var source = PresentationSource.FromVisual(target);

            var preview = new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key) { RoutedEvent = Keyboard.PreviewKeyDownEvent };
            target.RaiseEvent(preview);
            var handled = preview.Handled;

            if (!handled)
            {
                var down = new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key) { RoutedEvent = Keyboard.KeyDownEvent };
                target.RaiseEvent(down);
                handled = down.Handled;
            }

            DispatcherUtil.DoEvents();
            return handled;
        }

        /// <summary>Selects a node and gives the canvas keyboard focus, as clicking the node does.</summary>
        private void SelectWithCanvasFocus(NodeModel node)
        {
            Dynamo.Selection.DynamoSelection.Instance.ClearSelection();
            Dynamo.Selection.DynamoSelection.Instance.Selection.Add(node);
            Keyboard.Focus(View);
            DispatcherUtil.DoEvents();
        }

        [Test]
        [Category("UnitTests")]
        public void WhenAINodeIsSelectedThenEnterMovesFocusToItsBadge()
        {
            var node = AddTwoInputNode();
            AsAITurn(() => Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "b", null, "B.out"));
            SelectWithCanvasFocus(node);

            Assert.IsTrue(PressKey(Key.Enter));

            Assert.IsTrue(ViewOf(node).AIBadge.IsKeyboardFocused);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenSelectedNodeHasNoAIChangeThenEnterLeavesFocusOnTheCanvas()
        {
            var node = AddTwoInputNode();
            SelectWithCanvasFocus(node);

            Assert.IsFalse(PressKey(Key.Enter));

            Assert.AreSame(View, Keyboard.FocusedElement);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenBadgeHasFocusThenTabMovesThroughChangedInputsAndWrapsAround()
        {
            var node = AddTwoInputNode();
            AsAITurn(() =>
            {
                Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "a", null, "A.out");
                Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "b", null, "B.out");
            });
            var badge = ViewOf(node).AIBadge;
            var portA = PortViewOf(node, "a");
            var portB = PortViewOf(node, "b");
            SelectWithCanvasFocus(node);
            PressKey(Key.Enter);

            Assert.IsTrue(PressKey(Key.Tab), "Tab is taken by the review, not by select neighbours.");
            Assert.IsTrue(portA.IsKeyboardFocused);
            Assert.IsTrue(portA.AIChangePopover.IsOpen, "Focusing a changed input opens its popover.");

            PressKey(Key.Tab);
            Assert.IsTrue(portB.IsKeyboardFocused);

            PressKey(Key.Tab);
            Assert.IsTrue(badge.IsKeyboardFocused, "Tab wraps back to the badge.");
        }

        [Test]
        [Category("UnitTests")]
        public void WhenReviewingThenEscapeReturnsFocusToTheCanvas()
        {
            var node = AddTwoInputNode();
            AsAITurn(() => Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "a", null, "A.out"));
            var port = PortViewOf(node, "a");
            SelectWithCanvasFocus(node);
            PressKey(Key.Enter);
            PressKey(Key.Tab);
            Assert.IsTrue(port.IsKeyboardFocused);

            Assert.IsTrue(PressKey(Key.Escape));

            Assert.IsFalse(port.IsKeyboardFocusWithin);
            Assert.IsFalse(ViewOf(node).AIBadge.IsKeyboardFocused);
            Assert.IsTrue(View.IsKeyboardFocusWithin, "Focus is back on the canvas, so canvas shortcuts work again.");
        }

        [Test]
        [Category("UnitTests")]
        public void WhenNotReviewingThenTabStillSelectsNeighbours()
        {
            var number = AddNumberNode();
            var node = AddTwoInputNode();
            ViewModel.Model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(number.GUID, 0, PortType.Output, DynamoModel.MakeConnectionCommand.Mode.Begin));
            ViewModel.Model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(node.GUID, 0, PortType.Input, DynamoModel.MakeConnectionCommand.Mode.End));
            AsAITurn(() => Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "a", null, "Number.out"));
            SelectWithCanvasFocus(node);

            PressKey(Key.Tab);

            Assert.IsTrue(number.IsSelected, "Tab on the canvas keeps selecting neighbours.");
        }

        #endregion

        #region Changed input outline

        [Test]
        [Category("UnitTests")]
        public void WhenInputIsChangedThenItIsOutlinedInPurpleWithACornerDot()
        {
            var node = AddTwoInputNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "b", "A.out", "B.out");
            DispatcherUtil.DoEvents();

            var outline = PortViewOf(node, "b").AIChangeOutline;
            Assert.IsNotNull(outline);
            Assert.AreEqual(Visibility.Visible, outline.Visibility);
            Assert.IsFalse(outline.IsHitTestVisible, "The outline never takes the port's clicks.");

            // Figma 8:304: a solid 2px outline, and a purple dot with a white ring on its corner
            var purple = Color.FromRgb(0xA7, 0x6E, 0xF5);
            var path = outline.ChildrenOfType<Path>().Single();
            Assert.AreEqual(purple, ((SolidColorBrush)path.Stroke).Color);
            Assert.AreEqual(2, path.StrokeThickness);
            Assert.That(path.StrokeDashArray == null || path.StrokeDashArray.Count == 0, "The outline is solid.");

            var dot = outline.ChildrenOfType<Ellipse>().Single();
            Assert.AreEqual(purple, ((SolidColorBrush)dot.Fill).Color);
            Assert.AreEqual(Colors.White, ((SolidColorBrush)dot.Stroke).Color);
            Assert.AreEqual(HorizontalAlignment.Right, dot.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Top, dot.VerticalAlignment);

            Assert.IsNull(PortViewOf(node, "a").AIChangeOutline, "Inputs the turn did not change are not outlined.");
        }

        /// <summary>
        /// Renders a port view as it appears on screen and counts the pixels tinted by the outline's
        /// purple, so the test fails if the outline is built but never drawn.
        /// </summary>
        private static int CountOutlinePixels(InPorts portView)
        {
            var width = (int)Math.Ceiling(portView.ActualWidth);
            var height = (int)Math.Ceiling(portView.ActualHeight);
            var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(portView);

            var pixels = new byte[width * height * 4];
            bitmap.CopyPixels(pixels, width * 4, 0);

            var count = 0;
            for (var i = 0; i < pixels.Length; i += 4)
            {
                int b = pixels[i], g = pixels[i + 1], r = pixels[i + 2];
                // AIPurple600 (#A76EF5) and its blends: blue and red well above green
                if (b - g > 40 && r - g > 15) count++;
            }

            return count;
        }

        [Test]
        [Category("UnitTests")]
        public void WhenInputIsChangedThenItsOutlineIsDrawn()
        {
            var node = AddTwoInputNode();

            AsAITurn(() => Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA));
            AsAITurn(() => Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnB, "a", "A.out", "B.out"));

            var changed = CountOutlinePixels(PortViewOf(node, "a"));
            var unchanged = CountOutlinePixels(PortViewOf(node, "b"));
            TestContext.Progress.WriteLine($"Outline pixels: changed={changed}, unchanged={unchanged}");

            Assert.AreEqual(0, unchanged, "An input the turn did not change has no purple.");
            Assert.Greater(changed, 40, "The changed input's outline is drawn.");
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
            Assert.AreEqual("Edited by Autodesk Assistant", popover.TitleText.Text);
            Assert.AreEqual("Input b", popover.InputNameText.Text);
            Assert.AreEqual("A.out", popover.BeforeText.Text);
            Assert.AreEqual(TextDecorations.Strikethrough, popover.BeforeText.TextDecorations, "The old value is struck through.");
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
            Assert.IsNull(popover.BeforeText.TextDecorations, "\"None\" replaced nothing, so it is not struck through.");
            Assert.AreEqual("A.out", popover.AfterText.Text);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenPopoverOpensThenItUsesTheLightFigmaStyle()
        {
            var node = AddTwoInputNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "b", "A.out", "B.out");
            DispatcherUtil.DoEvents();

            var popover = OpenPopover(PortViewOf(node, "b"));

            // Colours sampled from Figma 8:304
            Color ColorOf(Brush brush) => ((SolidColorBrush)brush).Color;
            Assert.AreEqual(Colors.White, ColorOf(popover.Content.Background));
            Assert.AreEqual(Color.FromRgb(0xCC, 0xCC, 0xCC), ColorOf(popover.Content.BorderBrush));
            Assert.AreEqual(Color.FromRgb(0x1A, 0x1A, 0x1A), ColorOf(popover.TitleText.Foreground));
            Assert.AreEqual(Color.FromRgb(0x66, 0x66, 0x66), ColorOf(popover.InputNameText.Foreground));
            Assert.AreEqual(Color.FromRgb(0x66, 0x66, 0x66), ColorOf(popover.BeforeText.Foreground));
            Assert.AreEqual(Color.FromRgb(0x1A, 0x1A, 0x1A), ColorOf(popover.AfterText.Foreground));
            Assert.AreEqual(Color.FromRgb(0x74, 0x47, 0xB8), ColorOf(popover.UndoButton.Foreground));
            Assert.AreEqual(Color.FromRgb(0x4D, 0x4D, 0x4D), ColorOf(popover.NextButton.Foreground));
            Assert.AreEqual(14, popover.TitleText.FontSize);
            StringAssert.Contains("Bold", popover.AfterText.FontFamily.Source, "The new value is bold.");
            StringAssert.Contains("Bold", popover.UndoButton.FontFamily.Source);
            StringAssert.Contains("Regular", popover.NextButton.FontFamily.Source);
        }

        /// <summary>The horizontal space between the right edge of Undo and the left edge of Next.</summary>
        private static double GapBetweenActions(AIChangePopover popover)
        {
            popover.UndoButton.UpdateLayout();
            var undoRight = popover.UndoButton.TranslatePoint(new Point(popover.UndoButton.ActualWidth, 0), popover.NextButton).X;
            return -undoRight;
        }

        [Test]
        [Category("UnitTests")]
        public void WhenPopoverOpensThenUndoAndNextAreSpacedApart()
        {
            var node = AddTwoInputNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "a", null, "A.out");
            DispatcherUtil.DoEvents();

            var popover = OpenPopover(PortViewOf(node, "a"));

            Assert.GreaterOrEqual(GapBetweenActions(popover), AIChangePopover.ActionsGap);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenActionLabelsAreLongThenUndoAndNextStillDoNotTouch()
        {
            var node = AddTwoInputNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "a", null, "A.out");
            DispatcherUtil.DoEvents();
            var popover = OpenPopover(PortViewOf(node, "a"));

            // Longer than any current translation, and wider than the popover's 340 px minimum
            popover.UndoButton.Content = "Diese Änderung des Autodesk Assistant rückgängig machen";
            popover.NextButton.Content = "Zur nächsten Änderung wechseln ›";
            DispatcherUtil.DoEvents();

            Assert.GreaterOrEqual(GapBetweenActions(popover), AIChangePopover.ActionsGap);
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

        /// <summary>Unloads and reloads a port view, as removing it from the visual tree and adding it back does.</summary>
        private static void Reload(InPorts portView)
        {
            portView.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
            DispatcherUtil.DoEvents();
            portView.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
            DispatcherUtil.DoEvents();
        }

        [Test]
        [Category("UnitTests")]
        public void WhenPortViewIsReloadedThenKeyboardFocusStillOpensItsPopover()
        {
            var node = AddTwoInputNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "a", null, "A.out");
            DispatcherUtil.DoEvents();
            var port = PortViewOf(node, "a");

            Reload(port);
            Keyboard.Focus(port);
            DispatcherUtil.DoEvents();

            Assert.IsTrue(port.IsKeyboardFocused);
            Assert.IsNotNull(port.AIChangePopover);
            Assert.IsTrue(port.AIChangePopover.IsOpen);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenPortViewIsUnloadedThenItsPopoverClosesAndLaterChangesShowOnReload()
        {
            var node = AddTwoInputNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "a", null, "A.out");
            DispatcherUtil.DoEvents();
            var port = PortViewOf(node, "a");
            var popover = OpenPopover(port);

            port.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
            DispatcherUtil.DoEvents();
            Assert.IsFalse(popover.IsOpen, "Unloading closes the popover.");

            Workspace.ClearAIChanges();
            DispatcherUtil.DoEvents();
            port.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
            DispatcherUtil.DoEvents();

            Assert.IsFalse(port.Focusable, "A change cleared while unloaded is picked up on reload.");
            Assert.AreEqual(Visibility.Collapsed, port.AIChangeOutline.Visibility);

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnB, "a", null, "B.out");
            DispatcherUtil.DoEvents();

            Assert.IsTrue(port.Focusable, "A reloaded port still follows new changes.");
            Assert.AreEqual(Visibility.Visible, port.AIChangeOutline.Visibility);
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
