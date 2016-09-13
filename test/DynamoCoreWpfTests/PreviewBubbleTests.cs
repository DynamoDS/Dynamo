using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CoreNodeModels;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;
using Dynamo.Utilities;

namespace DynamoCoreWpfTests
{
    class PreviewBubbleTests : DynamoTestUIBase
    {
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
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void PreviewBubbleVisible_OpenFile()
        {
            Open(@"core\PinnedNodeWorkspace.dyn");
            var nodeView = NodeViewWithGuid("c86f23ca-2a32-49b5-97c9-d82504801483");
            nodeView.PreviewControl.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

            DispatcherUtil.DoEvents();

            Assert.IsTrue(nodeView.PreviewControl.IsExpanded);
        }

        [Test]
        public void PreviewBubbleVisible_MouseMoveOverNode()
        {
            Open(@"core\DetailedPreviewMargin_Test.dyn");
            var nodeView = NodeViewWithGuid("7828a9dd-88e6-49f4-9ed3-72e355f89bcc");
            nodeView.PreviewControl.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

            RaiseMouseEnterOnNode(nodeView);

            Assert.IsTrue(nodeView.PreviewControl.IsCondensed);
        }

        [Test]
        public void PreviewBubbleVisible_MouseMoveOverNode_InCustomWorkspace()
        {
            Open(@"core\custom_node_saving\Constant2.dyf");
            var nodeView = NodeViewWithGuid("9ce91e89-c087-49cd-9fd9-540cca086475");
            nodeView.PreviewControl.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

            RaiseMouseEnterOnNode(nodeView);

            Assert.IsTrue(nodeView.PreviewControl.IsHidden);
        }

        [Test]
        public void PreviewBubbleVisible_MouseMoveOutOfNode()
        {
            Open(@"core\DetailedPreviewMargin_Test.dyn");
            var nodeView = NodeViewWithGuid("7828a9dd-88e6-49f4-9ed3-72e355f89bcc");
            nodeView.PreviewControl.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
            
            RaiseMouseEnterOnNode(nodeView);

            Assert.IsTrue(nodeView.PreviewControl.IsCondensed);

            RaiseMouseLeaveNode(nodeView);

            Assert.IsTrue(nodeView.PreviewControl.IsHidden);
        }

        [Test]
        public void PreviewBubbleHiiden_OnFrozenNode()
        {
            Open(@"core\DetailedPreviewMargin_Test.dyn");
            var nodeView = NodeViewWithGuid("7828a9dd-88e6-49f4-9ed3-72e355f89bcc");
            nodeView.ViewModel.IsFrozen = true;
            nodeView.PreviewControl.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

            RaiseMouseEnterOnNode(nodeView);

            Assert.IsTrue(nodeView.PreviewControl.IsHidden);
        }

        [Test]
        public void PreviewBubble_ListMargin()
        {
            OpenAndRun(@"core\DetailedPreviewMargin_Test.dyn");

            var nodeView = NodeViewWithGuid("81c94fd0-35a0-4680-8535-00aff41192d3");
            nodeView.PreviewControl.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

            // Fire transition on dynamo main ui thread.
            View.Dispatcher.Invoke(() =>
            {
                nodeView.PreviewControl.BindToDataSource();
                nodeView.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Condensed);
                nodeView.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Expanded);
            });

            DispatcherUtil.DoEvents();
            var watchTree = nodeView.PreviewControl.ChildOfType<WatchTree>();
            Assert.NotNull(watchTree);
            Assert.AreEqual(new System.Windows.Thickness(5, 5, 5, 5), watchTree.ChildOfType<VirtualizingPanel>().Margin);
        }

        [Test]
        public void PreviewBubble_NumberMargin()
        {
            OpenAndRun(@"core\DetailedPreviewMargin_Test.dyn");

            var nodeView = NodeViewWithGuid("7828a9dd-88e6-49f4-9ed3-72e355f89bcc");
            nodeView.PreviewControl.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

            // Fire transition on dynamo main ui thread.
            View.Dispatcher.Invoke(() =>
            {
                nodeView.PreviewControl.BindToDataSource();
                nodeView.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Condensed);
                nodeView.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Expanded);
            });

            DispatcherUtil.DoEvents();
            var watchTree = nodeView.PreviewControl.ChildOfType<WatchTree>();
            Assert.NotNull(watchTree);
            Assert.AreEqual(new System.Windows.Thickness(-10, 5, 5, 5), watchTree.ChildOfType<VirtualizingPanel>().Margin);
        }

        [Test]
        public void PreviewBubble_CloseWhenFocusInCodeBlock()
        {
            OpenAndRun(@"core\DetailedPreviewMargin_Test.dyn");

            var nodeView = NodeViewWithGuid("1382aaf9-9432-4cf0-86ae-c586d311767e");
            nodeView.PreviewControl.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

            View.Dispatcher.Invoke(() =>
            {
                nodeView.PreviewControl.BindToDataSource();
                nodeView.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Condensed);
            });
            DispatcherUtil.DoEvents();
            nodeView.ChildOfType<ICSharpCode.AvalonEdit.Editing.TextArea>().Focus();

            Assert.IsTrue(nodeView.PreviewControl.IsHidden);
        }

        [Test]
        public void PreviewBubble_NoCrashWithCodeBlock()
        {
            var code = new CodeBlockNodeModel(this.Model.LibraryServices);

            this.Model.AddNodeToCurrentWorkspace(code, true);
            this.Run();
            var nodeView = NodeViewWithGuid(code.GUID.ToString());
            
            Assert.IsNotNull(nodeView);
            View.Dispatcher.Invoke(() =>
            {
                nodeView.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0) 
                { RoutedEvent = Mouse.MouseEnterEvent });
                nodeView.PreviewControl.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0)
                { RoutedEvent = Mouse.MouseEnterEvent });
            });
            DispatcherUtil.DoEvents();

            Assert.IsTrue(nodeView.PreviewControl.IsHidden);
        }

        [Test]
        public void PreviewBubble_CodeBlockIsNotInFocus()
        {
            var code = new CodeBlockNodeModel(this.Model.LibraryServices);

            Model.AddNodeToCurrentWorkspace(code, true);
            Run();

            // Click on DragCanvas.
            View.ChildOfType<DragCanvas>().RaiseEvent(
                new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = Mouse.MouseDownEvent
            });
            var nodeView = NodeViewWithGuid(code.GUID.ToString());
            Assert.IsNotNull(nodeView);

            // Move mouse on code node.
            var dispatcherOperation = View.Dispatcher.BeginInvoke(new Action(
            () =>
            {
                nodeView.RaiseEvent(
                    new MouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = Mouse.MouseEnterEvent });
            }));
            DispatcherUtil.DoEvents();

            dispatcherOperation.Completed += (s, args) =>
                Assert.IsTrue(nodeView.PreviewControl.IsCondensed);

            // Move mouse on code node preview.
            dispatcherOperation = View.Dispatcher.BeginInvoke(new Action(
            () =>
            {
                nodeView.PreviewControl.RaiseEvent(
                    new MouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = Mouse.MouseEnterEvent });
            }));
            DispatcherUtil.DoEvents();

            dispatcherOperation.Completed += (s, args) =>
            Assert.IsTrue(nodeView.PreviewControl.IsExpanded);
        }

        [Test]
        public void PreviewBubble_IsExpandedChangedInWatchTree()
        {
            OpenAndRun(@"core\DetailedPreviewMargin_Test.dyn");

            var nodeView = NodeViewWithGuid("81c94fd0-35a0-4680-8535-00aff41192d3");
            nodeView.PreviewControl.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

            // Fire transition on dynamo main ui thread.
            View.Dispatcher.Invoke(() =>
            {
                nodeView.PreviewControl.BindToDataSource();
                nodeView.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Condensed);
                nodeView.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Expanded);
            });

            DispatcherUtil.DoEvents();
            var watchTree = nodeView.PreviewControl.ChildOfType<WatchTree>();
            Assert.NotNull(watchTree);

            // Save old values.
            var oldHeight = nodeView.PreviewControl.ActualHeight;
            var oldWidth = nodeView.PreviewControl.ActualWidth;

            var dispatcherOperation = View.Dispatcher.BeginInvoke(new Action(
                () =>
                {
                    // Collapse root tree view item.
                    watchTree.ChildOfType<TreeViewItem>().IsExpanded = false;
                }));
            DispatcherUtil.DoEvents();

            // Wait until operation is completed.
            dispatcherOperation.Completed += (s, args) =>
            {
                // Width of preview bubble should stay the same, but height should be smaller.
                Assert.AreEqual(oldWidth, nodeView.PreviewControl.ActualWidth);
                Assert.Greater(oldHeight, nodeView.PreviewControl.ActualHeight);
            };
        }

        #region Watch PreviewBubble

        [Test]
        public void Watch_PreviewAllowanceDisabled()
        {
            OpenAndRun(@"core\WatchPreviewBubble.dyn");

            var nodeView = NodeViewWithGuid("456e57f3-d06f-4a53-9771-27188ee9cb40");

            RaiseMouseEnterOnNode(nodeView);

            Assert.IsTrue(nodeView.PreviewControl.IsHidden);
        }

        #endregion

        [Test]
        public void PreviewBubble_HiddenDummyVerticalBoundaries()
        {
            Open(@"core\DetailedPreviewMargin_Test.dyn");

            var nodeView = NodeViewWithGuid("1382aaf9-9432-4cf0-86ae-c586d311767e");
            nodeView.PreviewControl.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
                        
            // preview is hidden
            Assert.IsTrue(ElementIsInContainer(nodeView.PreviewControl.HiddenDummy, nodeView.PreviewControl, 0));

            View.Dispatcher.Invoke(() =>
            {
                nodeView.PreviewControl.BindToDataSource();
                nodeView.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Condensed);
            });

            DispatcherUtil.DoEvents();

            // preview is condensed
            Assert.IsTrue(ElementIsInContainer(nodeView.PreviewControl.HiddenDummy, nodeView.PreviewControl, 0));

            View.Dispatcher.Invoke(() =>
            {
                nodeView.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Expanded);
            });

            DispatcherUtil.DoEvents();

            // preview is expanded, and its size will be slighly larger than the size of node view.
            // See PR: https://github.com/DynamoDS/Dynamo/pull/6799
            Assert.IsTrue(ElementIsInContainer(nodeView.PreviewControl.HiddenDummy, nodeView, 10));
        }

        [Test]
        public void PreviewBubble_ToggleShowPreviewBubbles()
        {
            Open(@"core\DetailedPreviewMargin_Test.dyn");
            var nodeView = NodeViewWithGuid("7828a9dd-88e6-49f4-9ed3-72e355f89bcc");
            Assert.IsTrue(ViewModel.ShowPreviewBubbles, "Preview bubbles are turned off");

            nodeView.PreviewControl.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

            RaiseMouseEnterOnNode(nodeView);
            Assert.IsTrue(nodeView.PreviewControl.IsCondensed, "Compact preview bubble is not shown");

            RaiseMouseLeaveNode(nodeView);
            Assert.IsTrue(nodeView.PreviewControl.IsHidden, "Preview bubble is not hidden");

            // turn off preview bubbles
            ViewModel.ShowPreviewBubbles = false;
            Assert.IsFalse(ViewModel.ShowPreviewBubbles, "Preview bubbles have not been turned off");

            RaiseMouseEnterOnNode(nodeView);

            Assert.IsTrue(nodeView.PreviewControl.IsHidden, "Preview bubble is not hidden");
        }

        [Test]
        public void PreviewBubble_ShowExpandedPreviewOnPinIconHover()
        {
            Open(@"core\DetailedPreviewMargin_Test.dyn");
            var nodeView = NodeViewWithGuid("7828a9dd-88e6-49f4-9ed3-72e355f89bcc");

            var previewBubble = nodeView.PreviewControl;
            previewBubble.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
            previewBubble.bubbleTools.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

            // open preview bubble
            RaiseMouseEnterOnNode(nodeView);
            Assert.IsTrue(previewBubble.IsCondensed, "Compact preview bubble should be shown");
            Assert.AreEqual(Visibility.Collapsed, previewBubble.bubbleTools.Visibility, "Pin icon should not be shown");

            // hover preview bubble to see pin icon
            RaiseMouseEnterOnNode(previewBubble);
            Assert.AreEqual(Visibility.Visible, previewBubble.bubbleTools.Visibility, "Pin icon should be shown");

            // expand preview bubble
            RaiseMouseEnterOnNode(previewBubble.bubbleTools);
            Assert.IsTrue(previewBubble.IsExpanded, "Expanded preview bubble should be shown");
        }

        [Test]
        public void PreviewBubble_ShownForColorRange()
        {
            var colorRange = new ColorRange();
            Model.AddNodeToCurrentWorkspace(colorRange, true);
            DispatcherUtil.DoEvents();
            var nodeView = NodeViewWithGuid(colorRange.GUID.ToString());
            nodeView.PreviewControl.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

            // open preview bubble
            RaiseMouseEnterOnNode(nodeView);
            Assert.IsFalse(nodeView.PreviewControl.IsHidden, "Preview bubble for color range should be shown");
        }

        private bool ElementIsInContainer(FrameworkElement element, FrameworkElement container, int offset)
        {
            var relativePosition = element.TranslatePoint(new Point(), container);
            relativePosition.X += offset;
            
            return (relativePosition.X == 0) && (element.ActualWidth <= container.ActualWidth);
        }

        private void RaiseMouseEnterOnNode(IInputElement nv)
        {
            View.Dispatcher.Invoke(() =>
            {
                nv.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = Mouse.MouseEnterEvent });
            });

            DispatcherUtil.DoEvents();
        }

        private void RaiseMouseLeaveNode(IInputElement nv)
        {
            View.Dispatcher.Invoke(() =>
            {
                nv.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = Mouse.MouseLeaveEvent });
            });

            DispatcherUtil.DoEvents();
        }
    }
}
