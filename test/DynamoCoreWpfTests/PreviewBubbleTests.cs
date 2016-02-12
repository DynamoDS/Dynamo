using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        public void PreviewBubbleVisible_MouseMoveOverNode()
        {
            Open(@"core\DetailedPreviewMargin_Test.dyn");
            var nodeView = NodeViewWithGuid("7828a9dd-88e6-49f4-9ed3-72e355f89bcc");
            nodeView.PreviewControl.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

            View.Dispatcher.Invoke(() =>
            {
                nodeView.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = Mouse.MouseEnterEvent });
            });
            DispatcherUtil.DoEvents();

            Assert.IsTrue(nodeView.PreviewControl.IsCondensed);
        }

        [Test]
        public void PreviewBubbleVisible_MouseMoveOutOfNode()
        {
            Open(@"core\DetailedPreviewMargin_Test.dyn");
            var nodeView = NodeViewWithGuid("7828a9dd-88e6-49f4-9ed3-72e355f89bcc");
            nodeView.PreviewControl.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));


            // Raise mouse enter event.
            View.Dispatcher.Invoke(() =>
            {
                nodeView.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = Mouse.MouseEnterEvent });
            });
            DispatcherUtil.DoEvents();

            Assert.IsTrue(nodeView.PreviewControl.IsCondensed);

            // Raise mouse leave event.
            View.Dispatcher.Invoke(() =>
            {
                nodeView.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = Mouse.MouseLeaveEvent });
            });
            DispatcherUtil.DoEvents();

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
                nodeView.PreviewControl.BindToDataSource(nodeView.ViewModel.NodeModel.CachedValue);
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
                nodeView.PreviewControl.BindToDataSource(nodeView.ViewModel.NodeModel.CachedValue);
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
                nodeView.PreviewControl.BindToDataSource(nodeView.ViewModel.NodeModel.CachedValue);
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
                nodeView.PreviewControl.BindToDataSource(nodeView.ViewModel.NodeModel.CachedValue);
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

            // Raise mouse enter event.
            View.Dispatcher.Invoke(() =>
            {
                nodeView.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = Mouse.MouseEnterEvent });
            });
            DispatcherUtil.DoEvents();

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
            Assert.IsTrue(ElementIsInContainer(nodeView.PreviewControl.HiddenDummy, nodeView.PreviewControl));

            View.Dispatcher.Invoke(() =>
            {
                nodeView.PreviewControl.BindToDataSource(nodeView.ViewModel.NodeModel.CachedValue);
                nodeView.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Condensed);
            });

            DispatcherUtil.DoEvents();

            // preview is condensed
            Assert.IsTrue(ElementIsInContainer(nodeView.PreviewControl.HiddenDummy, nodeView.PreviewControl));

            View.Dispatcher.Invoke(() =>
            {
                nodeView.PreviewControl.TransitionToState(Dynamo.UI.Controls.PreviewControl.State.Expanded);
            });

            DispatcherUtil.DoEvents();

            // preview is expanded
            Assert.IsTrue(ElementIsInContainer(nodeView.PreviewControl.HiddenDummy, nodeView));
        }        

        private bool ElementIsInContainer(FrameworkElement element, FrameworkElement container)
        {
            var relativePosition = element.TranslatePoint(new Point(), container);
            
            return (relativePosition.X == 0) && (element.ActualWidth <= container.ActualWidth);
        }
    }
}
