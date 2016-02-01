using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
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
            Assert.AreEqual(new System.Windows.Thickness(-15, 5, 5, 5), watchTree.ChildOfType<VirtualizingPanel>().Margin);
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
                nodeView.ChildOfType<ICSharpCode.AvalonEdit.Editing.TextArea>().Focus();
            });
            
            
            DispatcherUtil.DoEvents();

            Assert.IsTrue(nodeView.PreviewControl.IsHidden);
        }
    }
}
