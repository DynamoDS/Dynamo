﻿using System;
using System.Linq;
using Dynamo.Selection;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;
using System.Windows.Input;

namespace DynamoCoreWpfTests
{
    public class NodeViewTests : DynamoTestUIBase
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

        [Test]
        public void ZIndex_Test_MouseDown()
        {
            // Reset zindex to start value.
            Dynamo.ViewModels.NodeViewModel.StaticZIndex = 3;
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("b8c2a62f-f1ce-4327-8d98-c4e0cc0ebed4");

            // Index of first node == 4.
            Assert.AreEqual(4, nodeView.ViewModel.ZIndex);
            Assert.AreEqual(3 + ViewModel.HomeSpace.Nodes.Count(), Dynamo.ViewModels.NodeViewModel.StaticZIndex);
            nodeView.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = Mouse.PreviewMouseDownEvent,
                Source = this,
            });

            // After click node should have The highest index.
            Assert.AreEqual(nodeView.ViewModel.ZIndex, Dynamo.ViewModels.NodeViewModel.StaticZIndex);
        }

        [Test]
        public void ZIndex_Test_MouseEnter_Leave()
        {
            // Reset zindex to start value.
            Dynamo.ViewModels.NodeViewModel.StaticZIndex = 3;
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("b8c2a62f-f1ce-4327-8d98-c4e0cc0ebed4");

            // Index of first node == 4.
            Assert.AreEqual(4, nodeView.ViewModel.ZIndex);
            Assert.AreEqual(3 + ViewModel.HomeSpace.Nodes.Count(), Dynamo.ViewModels.NodeViewModel.StaticZIndex);

            var dispatcherOperation = View.Dispatcher.BeginInvoke(new Action(
                () => nodeView.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0)
                 {
                     RoutedEvent = Mouse.MouseEnterEvent
                 })));
            DispatcherUtil.DoEvents();

            dispatcherOperation.Completed += (s, args) =>
            {
                // After mouse enter node should have the highest index.
                Assert.AreEqual(nodeView.ViewModel.ZIndex, Dynamo.ViewModels.NodeViewModel.StaticZIndex);
            };

            dispatcherOperation = View.Dispatcher.BeginInvoke(new Action(
                () => nodeView.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0)
                {
                    RoutedEvent = Mouse.MouseLeaveEvent
                })));
            DispatcherUtil.DoEvents();

            dispatcherOperation.Completed += (s, args) =>
            {
                // After mouse leave node should have the old index.
                Assert.AreEqual(nodeView.ViewModel.ZIndex, Dynamo.ViewModels.NodeViewModel.StaticZIndex);
                Assert.AreEqual(4, nodeView.ViewModel.ZIndex);
            };
        }

        [Test]
        public void ZIndex_NodeAsMemberOfGroup()
        {
            // Reset zindex to start value.
            Dynamo.ViewModels.NodeViewModel.StaticZIndex = 3;
            Open(@"UI\CoreUINodes.dyn");

            var nodeView = NodeViewWithGuid("b8c2a62f-f1ce-4327-8d98-c4e0cc0ebed4");

            //Select the node for group
            DynamoSelection.Instance.Selection.Add(nodeView.ViewModel.NodeModel);

            //Create a Group around that node
            ViewModel.AddAnnotationCommand.Execute(null);
            var annotation = ViewModel.HomeSpaceViewModel.Annotations.First();
            Assert.IsNotNull(annotation);

            // Index of first node == 4.
            Assert.AreEqual(4, nodeView.ViewModel.ZIndex);
            Assert.AreEqual(3 + ViewModel.HomeSpace.Nodes.Count(), Dynamo.ViewModels.NodeViewModel.StaticZIndex);

            var dispatcherOperation = View.Dispatcher.BeginInvoke(new Action(
               () => nodeView.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0)
               {
                   RoutedEvent = Mouse.MouseEnterEvent
               })));
            DispatcherUtil.DoEvents();

            dispatcherOperation.Completed += (s, args) =>
            {
                // After mouse enter node should have The highest index.
                Assert.AreEqual(nodeView.ViewModel.ZIndex, Dynamo.ViewModels.NodeViewModel.StaticZIndex);
                Assert.AreEqual(2, annotation.ZIndex);
            };

            dispatcherOperation = View.Dispatcher.BeginInvoke(new Action(
                () => nodeView.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0)
                {
                    RoutedEvent = Mouse.MouseLeaveEvent
                })));
            DispatcherUtil.DoEvents();

            dispatcherOperation.Completed += (s, args) =>
            {
                // After mouse leave node should have the old index.
                Assert.AreEqual(4, nodeView.ViewModel.ZIndex);
                Assert.AreEqual(2, annotation.ZIndex);
            };
        }
    }
}
