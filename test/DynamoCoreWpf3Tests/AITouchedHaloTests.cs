using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using CoreNodeModels.Input;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.ViewModels;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    /// <summary>
    /// Tests for the AI-touched halo (DYN-10999): the soft purple glow drawn behind nodes the
    /// AI Assistant placed, modified or moved.
    /// </summary>
    public class AITouchedHaloTests : DynamoTestUIBase
    {
        private const string TurnA = "turn-a";

        private WorkspaceModel Workspace => ViewModel.Model.CurrentWorkspace;

        private NodeModel AddNumberNode()
        {
            var node = new DoubleInput();
            Workspace.AddAndRegisterNode(node, false);
            DispatcherUtil.DoEvents();
            return node;
        }

        private NodeViewModel ViewModelOf(NodeModel node)
        {
            return ViewModel.CurrentSpaceViewModel.Nodes.Single(n => n.NodeModel == node);
        }

        private NodeView ViewOf(NodeModel node)
        {
            DispatcherUtil.DoEvents();
            return NodeViewWithGuid(node.GUID.ToString());
        }

        [Test]
        [Category("UnitTests")]
        public void UntouchedNodeHasNoHalo()
        {
            var node = AddNumberNode();

            Assert.AreEqual(0, ViewModelOf(node).AIHaloOpacity);
            Assert.IsNull(ViewOf(node).AIHalo, "The halo is only created for nodes that need one.");
        }

        [Test]
        [Category("UnitTests")]
        public void PlacedNodeShowsFullHalo()
        {
            var node = AddNumberNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);

            var halo = ViewOf(node).AIHalo;
            Assert.IsNotNull(halo);
            Assert.AreEqual(Visibility.Visible, halo.Visibility);
            Assert.AreEqual(NodeViewModel.AIHaloFullOpacity, halo.Opacity);
        }

        [Test]
        [Category("UnitTests")]
        public void ModifiedNodeShowsFullHalo()
        {
            var node = AddNumberNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");

            var halo = ViewOf(node).AIHalo;
            Assert.IsNotNull(halo);
            Assert.AreEqual(Visibility.Visible, halo.Visibility);
            Assert.AreEqual(NodeViewModel.AIHaloFullOpacity, halo.Opacity);
        }

        [Test]
        [Category("UnitTests")]
        public void MovedOnlyNodeShowsHaloAtFortyPercent()
        {
            var node = AddNumberNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Moved, TurnA, null, "0, 0", "100, 50");

            Assert.AreEqual(0.4, NodeViewModel.AIHaloMovedOpacity);
            var halo = ViewOf(node).AIHalo;
            Assert.IsNotNull(halo);
            Assert.AreEqual(Visibility.Visible, halo.Visibility);
            Assert.AreEqual(NodeViewModel.AIHaloMovedOpacity, halo.Opacity);
        }

        [Test]
        [Category("UnitTests")]
        public void MovedNodeThatIsThenModifiedShowsFullHalo()
        {
            var node = AddNumberNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Moved, TurnA, null, "0, 0", "100, 50");
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");

            Assert.AreEqual(NodeViewModel.AIHaloFullOpacity, ViewOf(node).AIHalo.Opacity);
        }

        [Test]
        [Category("UnitTests")]
        public void HaloHidesWhenTheChangeRecordIsCleared()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            Assert.AreEqual(Visibility.Visible, ViewOf(node).AIHalo.Visibility);

            Workspace.ClearAIChanges();

            Assert.AreEqual(0, ViewModelOf(node).AIHaloOpacity);
            Assert.AreEqual(Visibility.Collapsed, ViewOf(node).AIHalo.Visibility);
        }

        [Test]
        [Category("UnitTests")]
        public void HaloOpacityChangeIsAnnounced()
        {
            var node = AddNumberNode();
            var raised = new List<string>();
            System.ComponentModel.PropertyChangedEventHandler handler = (s, e) => raised.Add(e.PropertyName);
            var nodeViewModel = ViewModelOf(node);
            nodeViewModel.PropertyChanged += handler;
            try
            {
                Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            }
            finally
            {
                nodeViewModel.PropertyChanged -= handler;
            }

            CollectionAssert.Contains(raised, nameof(NodeViewModel.AIHaloOpacity));
        }

        [Test]
        [Category("UnitTests")]
        public void HaloSitsBehindTheNodeAndLeavesItUnchanged()
        {
            var node = AddNumberNode();
            var nodeView = ViewOf(node);
            var partsBefore = nodeView.grid.Children.OfType<UIElement>().ToList();
            var bodyFill = NodePart<Rectangle>(nodeView, "nodeBackground").Fill;
            var headerFill = NodePart<Border>(nodeView, "nameBackground").Background;

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            var halo = ViewOf(node).AIHalo;

            // Behind every other part of the node.
            var haloZIndex = Panel.GetZIndex(halo);
            foreach (var child in nodeView.grid.Children.OfType<UIElement>().Where(c => c != halo))
            {
                Assert.Less(haloZIndex, Panel.GetZIndex(child));
            }

            // Extends past the node edge and never takes mouse clicks.
            Assert.AreEqual(new Thickness(-8), halo.Margin);
            Assert.IsFalse(halo.IsHitTestVisible);

            // The halo is the only new part and the node's fill is unchanged. Part opacities are not
            // compared: some parts fade in over 0.2 s, so their value depends on test timing.
            var partsAfter = nodeView.grid.Children.OfType<UIElement>().Where(c => c != halo).ToList();
            CollectionAssert.AreEqual(partsBefore, partsAfter);
            Assert.AreSame(bodyFill, NodePart<Rectangle>(nodeView, "nodeBackground").Fill);
            Assert.AreSame(headerFill, NodePart<Border>(nodeView, "nameBackground").Background);
            Assert.AreEqual(1.0, nodeView.grid.Opacity);
        }

        private static T NodePart<T>(NodeView nodeView, string name) where T : FrameworkElement
        {
            return nodeView.grid.Children.OfType<T>().Single(e => e.Name == name);
        }

        [Test]
        [Category("UnitTests")]
        public void NodeBorderIsHiddenWhileHaloShowsAndRestoredAfter()
        {
            var node = AddNumberNode();
            var nodeView = ViewOf(node);
            var borderBrushBefore = nodeView.nodeBorder.BorderBrush;
            var borderThicknessBefore = nodeView.nodeBorder.BorderThickness;

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            DispatcherUtil.DoEvents();

            Assert.AreEqual(Brushes.Transparent, nodeView.nodeBorder.BorderBrush,
                "No pale ring between the node and the halo.");
            Assert.AreEqual(borderThicknessBefore, nodeView.nodeBorder.BorderThickness,
                "The node keeps its size.");

            Workspace.ClearAIChanges();
            DispatcherUtil.DoEvents();

            Assert.AreSame(borderBrushBefore, nodeView.nodeBorder.BorderBrush);
        }

        [Test]
        [Category("UnitTests")]
        public void HaloUsesTheAIPurpleColor()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);

            var glow = ((Grid)ViewOf(node).AIHalo).Children.OfType<Border>().Single();

            Assert.AreEqual((Color)ColorConverter.ConvertFromString("#A76EF5"), ((SolidColorBrush)glow.Background).Color);
            Assert.IsNotNull(glow.Effect, "The glow is blurred.");
            Assert.IsInstanceOf<BitmapCache>(ViewOf(node).AIHalo.CacheMode, "The blurred glow is cached as a bitmap.");
        }
    }
}
