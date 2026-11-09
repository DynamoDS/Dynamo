using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using CoreNodeModels.Input;
using Dynamo.Controls;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.ViewModels;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    /// <summary>
    /// Tests for the AI-touched halo (DYN-10999): the soft purple glow drawn behind nodes the
    /// AI Assistant placed, modified or moved, its breathing glow and fade-out (DYN-11004), and the
    /// AI Assistant badge (DYN-11000) drawn on nodes it placed or modified, and the zoomed-out
    /// treatment (DYN-11010) that shows the badge as a zoom-state icon.
    /// </summary>
    public class AITouchedHaloTests : DynamoTestUIBase
    {
        private const string TurnA = "turn-a";

        [TearDown]
        public void RestoreTheGlowClock()
        {
            AIHaloGlow.Now = AIHaloGlow.DefaultClock;
        }

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
            var glow = ViewModelOf(node).AIHaloGlow;
            Assert.AreEqual(NodeViewModel.AIHaloFullOpacity, glow.Strength);
            Assert.IsTrue(glow.Breathes);
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
            var glow = ViewModelOf(node).AIHaloGlow;
            Assert.AreEqual(NodeViewModel.AIHaloFullOpacity, glow.Strength);
            Assert.IsTrue(glow.Breathes);
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
            var glow = ViewModelOf(node).AIHaloGlow;
            Assert.AreEqual(NodeViewModel.AIHaloMovedOpacity, glow.Strength);
            Assert.IsFalse(glow.Breathes, "Moved nodes fade in, hold and fade out, without breathing.");
        }

        [Test]
        [Category("UnitTests")]
        public void MovedNodeThatIsThenModifiedShowsFullHalo()
        {
            var node = AddNumberNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Moved, TurnA, null, "0, 0", "100, 50");
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");

            var glow = ViewModelOf(node).AIHaloGlow;
            Assert.AreEqual(NodeViewModel.AIHaloFullOpacity, glow.Strength);
            Assert.IsTrue(glow.Breathes);
            Assert.AreEqual(Visibility.Visible, ViewOf(node).AIHalo.Visibility);
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
            Assert.IsNull(ViewModelOf(node).AIHaloGlow);
            Assert.AreEqual(Visibility.Collapsed, ViewOf(node).AIHalo.Visibility);
            Assert.IsFalse(ViewOf(node).AIHalo.HasAnimatedProperties, "The glow animation is stopped.");
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
            var badge = ViewOf(node).AIBadge;

            // Behind every other part of the node.
            var haloZIndex = Panel.GetZIndex(halo);
            foreach (var child in nodeView.grid.Children.OfType<UIElement>().Where(c => c != halo))
            {
                Assert.Less(haloZIndex, Panel.GetZIndex(child));
            }

            // Extends past the node edge and never takes mouse clicks.
            Assert.AreEqual(new Thickness(-8), halo.Margin);
            Assert.IsFalse(halo.IsHitTestVisible);
            Assert.IsFalse(badge.IsHitTestVisible);

            // The halo and the badge are the only new parts, and the node's fill is unchanged.
            // Part opacities are not compared: some parts fade in over 0.2 s, so their value
            // depends on test timing.
            var partsAfter = nodeView.grid.Children.OfType<UIElement>()
                .Where(c => c != halo && c != badge).ToList();
            CollectionAssert.AreEqual(partsBefore, partsAfter);
            Assert.AreSame(bodyFill, NodePart<Rectangle>(nodeView, "nodeBackground").Fill);
            Assert.AreSame(headerFill, NodePart<Border>(nodeView, "nameBackground").Background);
            Assert.AreEqual(1.0, nodeView.grid.Opacity);
        }

        #region Lifetime and motion (DYN-11004)

        [Test]
        [Category("UnitTests")]
        public void OnlyTheHaloOpacityIsAnimated()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);

            var halo = (Grid)ViewOf(node).AIHalo;
            var glow = halo.Children.OfType<Border>().Single();

            Assert.IsTrue(halo.HasAnimatedProperties, "The halo's opacity is animated.");
            Assert.IsFalse(glow.HasAnimatedProperties, "The blurred glow itself never changes.");
            Assert.IsFalse(glow.Effect.HasAnimatedProperties, "No blur changes.");
            Assert.AreEqual(new Thickness(-8), halo.Margin, "No size changes.");
        }

        [Test]
        [Category("UnitTests")]
        public void HaloIsGoneAfterTheFadeOutButTheBadgeStays()
        {
            var now = TimeSpan.FromMinutes(1);
            AIHaloGlow.Now = () => now;
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            var nodeView = ViewOf(node);
            Assert.AreEqual(Visibility.Visible, nodeView.AIHalo.Visibility);

            // Just before the end of the fade-out the halo is still on
            now += AIHaloGlow.FadeOutEnd - TimeSpan.FromMilliseconds(100);
            nodeView.UpdateAIHalo();
            Assert.AreEqual(Visibility.Visible, nodeView.AIHalo.Visibility);

            // After it, only the badge remains, even if the view checks again (for example when rebuilt)
            now += TimeSpan.FromMilliseconds(100);
            nodeView.UpdateAIHalo();
            Assert.IsTrue(ViewModelOf(node).AIHaloGlow.IsFinished);
            Assert.AreEqual(Visibility.Collapsed, nodeView.AIHalo.Visibility);
            Assert.IsFalse(nodeView.AIHalo.HasAnimatedProperties);
            Assert.AreEqual(Visibility.Visible, nodeView.AIBadge.Visibility, "The badge stays until the highlight is cleared.");
            Assert.IsTrue(node.IsRecentlyModifiedByAI, "The change record is kept.");
        }

        [Test]
        [Category("UnitTests")]
        public void ViewRebuiltMidGlowPicksUpWhereTheGlowIs()
        {
            var now = TimeSpan.FromMinutes(1);
            AIHaloGlow.Now = () => now;
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            Assert.IsNotNull(ViewOf(node).AIHaloClock);

            // 6 s in, the glow is in its steady hold. A new view for the same node, for example
            // after switching tabs, must join it there rather than fade in from zero.
            now += TimeSpan.FromSeconds(6);
            var rebuilt = new NodeView { DataContext = ViewModelOf(node) };
            rebuilt.UpdateAIHalo();

            var clock = rebuilt.AIHaloClock;
            Assert.IsNotNull(clock);
            PumpUntil(() => clock.CurrentTime >= TimeSpan.FromSeconds(6));
            Assert.GreaterOrEqual(clock.CurrentTime.Value, TimeSpan.FromSeconds(6), "Seeks to the glow's current time.");
            Assert.Less(clock.CurrentTime.Value, AIHaloGlow.HoldEnd);
            Assert.AreEqual(AIHaloGlow.HoldLevel, rebuilt.AIHalo.Opacity, 1e-6, "Shows the hold level straight away.");
        }

        [Test]
        [Category("UnitTests")]
        public void HaloHidesItselfWhenItsAnimationEnds()
        {
            // The glow's clock stays put, so only the animation finishing can hide the halo
            var now = TimeSpan.FromMinutes(1);
            AIHaloGlow.Now = () => now;
            var node = AddNumberNode();
            var nodeView = ViewOf(node);
            var borderBrushBefore = nodeView.nodeBorder.BorderBrush;
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            var clock = ViewOf(node).AIHaloClock;
            Assert.IsNotNull(clock);
            PumpUntil(() => clock.CurrentTime != null);

            clock.Controller.SkipToFill();
            PumpUntil(() => nodeView.AIHaloClock == null);

            Assert.IsFalse(ViewModelOf(node).AIHaloGlow.IsFinished, "Hidden by the animation, not by the view model.");
            Assert.IsNull(nodeView.AIHaloClock);
            Assert.AreEqual(Visibility.Collapsed, nodeView.AIHalo.Visibility);
            Assert.IsFalse(nodeView.AIHalo.HasAnimatedProperties);
            Assert.AreSame(borderBrushBefore, nodeView.nodeBorder.BorderBrush);
            Assert.AreEqual(Visibility.Visible, nodeView.AIBadge.Visibility, "The badge stays until the highlight is cleared.");
        }

        [Test]
        [Category("UnitTests")]
        public void NodeBorderComesBackAfterTheFadeOut()
        {
            var now = TimeSpan.FromMinutes(1);
            AIHaloGlow.Now = () => now;
            var node = AddNumberNode();
            var nodeView = ViewOf(node);
            var borderBrushBefore = nodeView.nodeBorder.BorderBrush;

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            DispatcherUtil.DoEvents();
            Assert.AreEqual(Brushes.Transparent, nodeView.nodeBorder.BorderBrush);

            now += AIHaloGlow.FadeOutEnd;
            nodeView.UpdateAIHalo();

            Assert.AreSame(borderBrushBefore, nodeView.nodeBorder.BorderBrush);
        }

        [Test]
        [Category("UnitTests")]
        public void NodeTouchedAgainWhileGlowingKeepsItsGlow()
        {
            var now = TimeSpan.FromMinutes(1);
            AIHaloGlow.Now = () => now;
            var node = AddNumberNode();
            AsAIEdit(() => Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA));
            var glow = ViewModelOf(node).AIHaloGlow;
            var clock = ViewOf(node).AIHaloClock;

            // A later AI edit, 8 s into the glow
            now += TimeSpan.FromSeconds(8);
            AsAIEdit(() => Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2"));

            Assert.AreSame(glow, ViewModelOf(node).AIHaloGlow, "The glow is not restarted.");
            Assert.AreSame(clock, ViewOf(node).AIHaloClock);
        }

        [Test]
        [Category("UnitTests")]
        public void NodeMovedThenModifiedWhileGlowingGetsTheFullGlow()
        {
            var now = TimeSpan.FromMinutes(1);
            AIHaloGlow.Now = () => now;
            var node = AddNumberNode();
            AsAIEdit(() => Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Moved, TurnA, null, "0, 0", "100, 50"));
            var moved = ViewModelOf(node).AIHaloGlow;

            now += TimeSpan.FromSeconds(8);
            AsAIEdit(() => Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2"));

            var glow = ViewModelOf(node).AIHaloGlow;
            Assert.AreEqual(NodeViewModel.AIHaloFullOpacity, glow.Strength);
            Assert.IsTrue(glow.Breathes);
            Assert.AreEqual(moved.StartedAt, glow.StartedAt, "Stays on the same timeline; no restart.");
        }

        [Test]
        [Category("UnitTests")]
        public void NodeTouchedAgainAfterItsGlowFadedGlowsAgain()
        {
            var now = TimeSpan.FromMinutes(1);
            AIHaloGlow.Now = () => now;
            var node = AddNumberNode();
            AsAIEdit(() => Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA));
            var first = ViewModelOf(node).AIHaloGlow;

            // Touched again after the fade-out, for example late in a long turn
            now += AIHaloGlow.FadeOutEnd + TimeSpan.FromSeconds(45);
            AsAIEdit(() => Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2"));

            var second = ViewModelOf(node).AIHaloGlow;
            Assert.AreNotSame(first, second);
            Assert.AreEqual(now, second.StartedAt, "The timer starts again from the new touch.");
            Assert.IsFalse(second.IsFinished);
            Assert.AreEqual(Visibility.Visible, ViewOf(node).AIHalo.Visibility);
        }

        [Test]
        [Category("UnitTests")]
        public void NodeChangedAgainInTheSameAIEditKeepsItsGlow()
        {
            var node = AddNumberNode();

            AsAIEdit(() =>
            {
                // Placed, then given a value
                Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
                var glow = ViewModelOf(node).AIHaloGlow;
                var clock = ViewOf(node).AIHaloClock;
                Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");

                Assert.AreSame(glow, ViewModelOf(node).AIHaloGlow, "The running glow is not restarted.");
                Assert.IsNotNull(clock);
                Assert.AreSame(clock, ViewOf(node).AIHaloClock, "The halo keeps playing the same animation.");
            });
        }

        [Test]
        [Category("UnitTests")]
        public void NodesChangedSeveralTimesInOneAIEditTakeOneStaggerSlotEach()
        {
            var nodes = Enumerable.Range(0, 5).Select(_ => AddNumberNode()).ToList();

            AsAIEdit(() =>
            {
                // Every node placed, then every node given a value
                foreach (var node in nodes)
                {
                    Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
                }
                foreach (var node in nodes)
                {
                    Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");
                }
            });

            CollectionAssert.AreEqual(new[] { 0.0, 100, 200, 300, 400 }, DelaysOf(nodes));
        }

        [Test]
        [Category("UnitTests")]
        public void NodeMovedThenModifiedInOneAIEditGetsTheFullGlowInItsSlot()
        {
            var first = AddNumberNode();
            var node = AddNumberNode();
            AIHaloGlow moved = null;

            AsAIEdit(() =>
            {
                Workspace.RecordAINodeChange(first.GUID, AIChangeKind.Placed, TurnA);
                Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Moved, TurnA, null, "0, 0", "100, 50");
                moved = ViewModelOf(node).AIHaloGlow;
                Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");
            });

            var glow = ViewModelOf(node).AIHaloGlow;
            Assert.AreEqual(NodeViewModel.AIHaloFullOpacity, glow.Strength);
            Assert.IsTrue(glow.Breathes);
            Assert.AreEqual(moved.Delay, glow.Delay, "Keeps its place in the stagger.");
            Assert.AreEqual(moved.StartedAt, glow.StartedAt);
            Assert.AreEqual(TimeSpan.FromMilliseconds(100), glow.Delay);
        }

        [Test]
        [Category("UnitTests")]
        public void NodesTouchedInTheSameAIEditAreStaggered()
        {
            var nodes = Enumerable.Range(0, 3).Select(_ => AddNumberNode()).ToList();

            PlaceInOneAIEdit(nodes);

            CollectionAssert.AreEqual(new[] { 0.0, 100, 200 }, DelaysOf(nodes), "100 ms apart, in the order touched.");
        }

        [Test]
        [Category("UnitTests")]
        public void AIEditsBackToBackAreStaggeredSeparately()
        {
            var firstEdit = Enumerable.Range(0, 3).Select(_ => AddNumberNode()).ToList();
            var secondEdit = Enumerable.Range(0, 2).Select(_ => AddNumberNode()).ToList();

            // No UI work and no time in between
            PlaceInOneAIEdit(firstEdit);
            PlaceInOneAIEdit(secondEdit);

            CollectionAssert.AreEqual(new[] { 0.0, 100, 200 }, DelaysOf(firstEdit));
            CollectionAssert.AreEqual(new[] { 0.0, 100 }, DelaysOf(secondEdit), "A new AI edit starts a new stagger.");
        }

        [Test]
        [Category("UnitTests")]
        public void PauseInsideOneAIEditKeepsOneStagger()
        {
            var now = TimeSpan.FromMinutes(1);
            AIHaloGlow.Now = () => now;
            var nodes = Enumerable.Range(0, 3).Select(_ => AddNumberNode()).ToList();

            AsAIEdit(() =>
            {
                Workspace.RecordAINodeChange(nodes[0].GUID, AIChangeKind.Placed, TurnA);

                // For example a dialog: the UI thread runs other work and time passes
                DispatcherUtil.DoEvents();
                now += TimeSpan.FromSeconds(5);

                Workspace.RecordAINodeChange(nodes[1].GUID, AIChangeKind.Placed, TurnA);
                Workspace.RecordAINodeChange(nodes[2].GUID, AIChangeKind.Placed, TurnA);
            });

            CollectionAssert.AreEqual(new[] { 0.0, 100, 200 }, DelaysOf(nodes));
        }

        [Test]
        [Category("UnitTests")]
        public void NestedAIEditsAreOneStagger()
        {
            var nodes = Enumerable.Range(0, 3).Select(_ => AddNumberNode()).ToList();

            using (Workspace.BeginAIEdit())
            {
                Workspace.RecordAINodeChange(nodes[0].GUID, AIChangeKind.Placed, TurnA);
                using (Workspace.BeginAIEdit())
                {
                    Workspace.RecordAINodeChange(nodes[1].GUID, AIChangeKind.Placed, TurnA);
                }
                Workspace.RecordAINodeChange(nodes[2].GUID, AIChangeKind.Placed, TurnA);
            }

            CollectionAssert.AreEqual(new[] { 0.0, 100, 200 }, DelaysOf(nodes));
        }

        [Test]
        [Category("UnitTests")]
        public void NodeTouchedOutsideAnAIEditStartsWithoutDelay()
        {
            var nodes = Enumerable.Range(0, 2).Select(_ => AddNumberNode()).ToList();

            foreach (var node in nodes)
            {
                Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            }

            CollectionAssert.AreEqual(new[] { 0.0, 0.0 }, DelaysOf(nodes));
        }

        /// <summary>
        /// WPF starts, seeks and completes animation clocks on its next animation tick. Pumps the UI
        /// thread until <paramref name="done"/> is true, for at most 3 s (shorter than any wait the
        /// tests check for, so time passing alone cannot make them pass).
        /// </summary>
        private static void PumpUntil(Func<bool> done)
        {
            var giveUpAt = System.DateTime.UtcNow + TimeSpan.FromSeconds(3);
            while (!done() && System.DateTime.UtcNow < giveUpAt)
            {
                DispatcherUtil.DoEvents();
                Thread.Sleep(15);
            }
        }

        private void AsAIEdit(Action changes)
        {
            using (Workspace.BeginAIEdit())
            {
                changes();
            }
        }

        private void PlaceInOneAIEdit(IEnumerable<NodeModel> nodes)
        {
            AsAIEdit(() =>
            {
                foreach (var node in nodes)
                {
                    Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
                }
            });
        }

        private List<double> DelaysOf(IEnumerable<NodeModel> nodes)
        {
            return nodes.Select(n => ViewModelOf(n).AIHaloGlow.Delay.TotalMilliseconds).ToList();
        }

        #endregion

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

        [Test]
        [Category("UnitTests")]
        public void UntouchedNodeHasNoBadge()
        {
            var node = AddNumberNode();

            Assert.IsFalse(ViewModelOf(node).ShowAIBadge);
            Assert.IsNull(ViewOf(node).AIBadge, "The badge is only created for nodes that need one.");
        }

        [Test]
        [Category("UnitTests")]
        public void PlacedNodeShowsBadge()
        {
            var node = AddNumberNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);

            Assert.IsTrue(ViewModelOf(node).ShowAIBadge);
            var badge = ViewOf(node).AIBadge;
            Assert.IsNotNull(badge);
            Assert.AreEqual(Visibility.Visible, badge.Visibility);
        }

        [Test]
        [Category("UnitTests")]
        public void ModifiedNodeShowsBadge()
        {
            var node = AddNumberNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");

            Assert.IsTrue(ViewModelOf(node).ShowAIBadge);
            var badge = ViewOf(node).AIBadge;
            Assert.IsNotNull(badge);
            Assert.AreEqual(Visibility.Visible, badge.Visibility);
        }

        [Test]
        [Category("UnitTests")]
        public void MovedOnlyNodeHasNoBadge()
        {
            var node = AddNumberNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Moved, TurnA, null, "0, 0", "100, 50");

            Assert.IsFalse(ViewModelOf(node).ShowAIBadge);
            Assert.IsNull(ViewOf(node).AIBadge, "Moved nodes are the quietest: halo only, no badge.");
            Assert.IsNotNull(ViewOf(node).AIHalo, "The 40% halo is still shown.");
        }

        [Test]
        [Category("UnitTests")]
        public void MovedNodeThatIsThenModifiedShowsBadge()
        {
            var node = AddNumberNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Moved, TurnA, null, "0, 0", "100, 50");
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");

            Assert.AreEqual(Visibility.Visible, ViewOf(node).AIBadge.Visibility);
        }

        [Test]
        [Category("UnitTests")]
        public void BadgeHidesWhenTheChangeRecordIsCleared()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            Assert.AreEqual(Visibility.Visible, ViewOf(node).AIBadge.Visibility);

            Workspace.ClearAIChanges();

            Assert.IsFalse(ViewModelOf(node).ShowAIBadge);
            Assert.AreEqual(Visibility.Collapsed, ViewOf(node).AIBadge.Visibility);
        }

        [Test]
        [Category("UnitTests")]
        public void BadgeChangeIsAnnounced()
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

            CollectionAssert.Contains(raised, nameof(NodeViewModel.ShowAIBadge));
        }

        [Test]
        [Category("UnitTests")]
        public void BadgeIsCentredOnTheTopRightCornerAndInFrontOfTheNode()
        {
            var node = AddNumberNode();
            var nodeView = ViewOf(node);

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            var badge = (Grid)ViewOf(node).AIBadge;

            // 24px disc plus a 1.5px white ring on each side.
            Assert.AreEqual(27, NodeView.AIBadgeSize);
            Assert.AreEqual(NodeView.AIBadgeSize, badge.Width);
            Assert.AreEqual(NodeView.AIBadgeSize, badge.Height);
            Assert.IsNotNull(badge.Children.OfType<Image>().Single().Source);

            // Top-right corner of the node, pulled out by half its size so its centre sits on the corner.
            Assert.AreEqual(HorizontalAlignment.Right, badge.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Top, badge.VerticalAlignment);
            Assert.AreEqual(Grid.GetRow(nodeView.nodeBorder), Grid.GetRow(badge), "Same top edge as the node.");
            Assert.AreEqual(new Thickness(0, -13.5, -13.5, 0), badge.Margin);

            // In front of every other part of the node, including the zoomed-out overlay, and
            // never takes mouse clicks.
            var badgeZIndex = Panel.GetZIndex(badge);
            foreach (var child in nodeView.grid.Children.OfType<UIElement>().Where(c => c != badge))
            {
                Assert.Greater(badgeZIndex, Panel.GetZIndex(child));
            }
            Assert.IsFalse(badge.IsHitTestVisible);
        }

        [Test]
        [Category("UnitTests")]
        public void BadgeIsNotShownWhenZoomedOut()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            var badgeImage = ((Grid)ViewOf(node).AIBadge).Children.OfType<Image>().Single();
            var workspaceViewModel = ViewModel.CurrentSpaceViewModel;

            // Zoomed out, below the 0.4 threshold where nodes switch to their zoomed-out look
            workspaceViewModel.Zoom = 0.3;
            DispatcherUtil.DoEvents();
            Assert.AreEqual(Visibility.Collapsed, badgeImage.Visibility);

            // Zoomed back in
            workspaceViewModel.Zoom = 0.6;
            DispatcherUtil.DoEvents();
            Assert.AreEqual(Visibility.Visible, badgeImage.Visibility);
        }

        [Test]
        [Category("UnitTests")]
        public void BadgeAddedWhileZoomedOutIsNotShown()
        {
            ViewModel.CurrentSpaceViewModel.Zoom = 0.3;
            var node = AddNumberNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            DispatcherUtil.DoEvents();

            var badgeImage = ((Grid)ViewOf(node).AIBadge).Children.OfType<Image>().Single();
            Assert.AreEqual(Visibility.Collapsed, badgeImage.Visibility);
        }

        #region Zoomed-out treatment (DYN-11010)

        private const string AIIcon = "ai-badge-96px.png";
        private const string FrozenIcon = "frozen-64px.png";
        private const string PreviewOffIcon = "hidden-64px.png";
        private const string WarningIcon = "alert-64px.png";

        /// <summary>
        /// The node's zoomed-out icons by file name: the two bottom-row places (left, right),
        /// then the place above them. Null where a place is empty.
        /// </summary>
        private string[] ZoomedOutIconsOf(NodeModel node)
        {
            DispatcherUtil.DoEvents();
            var nodeViewModel = ViewModelOf(node);
            return new[]
            {
                nodeViewModel.ImgGlyphOneSource?.Split('/').Last(),
                nodeViewModel.ImgGlyphTwoSource?.Split('/').Last(),
                nodeViewModel.ImgGlyphThreeSource?.Split('/').Last()
            };
        }

        private static void Freeze(NodeModel node)
        {
            node.UpdateValue(new UpdateValueParams(nameof(NodeModel.IsFrozen), "true"));
        }

        private static void TurnPreviewOff(NodeModel node)
        {
            node.UpdateValue(new UpdateValueParams(nameof(NodeModel.IsVisible), "false"));
        }

        private static void AddWarning(NodeModel node)
        {
            // Persistent, so a graph run does not clear it during the test
            node.Warning("A warning", true);
        }

        [Test]
        [Category("UnitTests")]
        public void UntouchedNodeHasNoZoomedOutAIIcon()
        {
            var node = AddNumberNode();

            CollectionAssert.AreEqual(new string[] { null, null, null }, ZoomedOutIconsOf(node));
        }

        [Test]
        [Category("UnitTests")]
        public void PlacedNodeShowsTheAIIconWhenZoomedOut()
        {
            var node = AddNumberNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);

            CollectionAssert.AreEqual(new[] { AIIcon, null, null }, ZoomedOutIconsOf(node));
        }

        [Test]
        [Category("UnitTests")]
        public void ModifiedNodeShowsTheAIIconWhenZoomedOut()
        {
            var node = AddNumberNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");

            CollectionAssert.AreEqual(new[] { AIIcon, null, null }, ZoomedOutIconsOf(node));
        }

        [Test]
        [Category("UnitTests")]
        public void MovedOnlyNodeHasNoZoomedOutAIIcon()
        {
            var node = AddNumberNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Moved, TurnA, null, "0, 0", "100, 50");

            CollectionAssert.AreEqual(new string[] { null, null, null }, ZoomedOutIconsOf(node),
                "Like the corner badge, moved nodes get the halo only.");
        }

        [Test]
        [Category("UnitTests")]
        public void AIIconIsShownOnTheNodeOnlyWhenZoomedOut()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            var nodeView = ViewOf(node);
            var badgeImage = ((Grid)nodeView.AIBadge).Children.OfType<Image>().Single();
            var aiIconImage = (Image)LogicalTreeHelper.FindLogicalNode(nodeView.zoomGlyphsGrid, "ZoomStateImgTwo");
            var workspaceViewModel = ViewModel.CurrentSpaceViewModel;

            // Zoomed out: the corner badge is dropped and the icon is shown on the node
            workspaceViewModel.Zoom = 0.3;
            DispatcherUtil.DoEvents();
            Assert.AreEqual(Visibility.Collapsed, badgeImage.Visibility);
            Assert.AreEqual(Visibility.Visible, nodeView.zoomGlyphsGrid.Visibility);
            Assert.AreEqual(Visibility.Visible, aiIconImage.Visibility);
            Assert.IsNotNull(aiIconImage.Source);

            // Zoomed back in: the corner badge is back and the zoomed-out icons are hidden
            workspaceViewModel.Zoom = 0.6;
            DispatcherUtil.DoEvents();
            Assert.AreEqual(Visibility.Visible, badgeImage.Visibility);
            Assert.AreEqual(Visibility.Collapsed, nodeView.zoomGlyphsGrid.Visibility);
        }

        [Test]
        [Category("UnitTests")]
        public void AIIconSitsNextToTheFrozenIcon()
        {
            var node = AddNumberNode();
            Freeze(node);

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);

            CollectionAssert.AreEqual(new[] { FrozenIcon, AIIcon, null }, ZoomedOutIconsOf(node));
        }

        [Test]
        [Category("UnitTests")]
        public void AIIconSitsNextToThePreviewOffIcon()
        {
            var node = AddNumberNode();
            TurnPreviewOff(node);

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);

            CollectionAssert.AreEqual(new[] { PreviewOffIcon, AIIcon, null }, ZoomedOutIconsOf(node));
        }

        [Test]
        [Category("UnitTests")]
        public void AIIconTakesTheFreeBottomPlaceAndLeavesTheWarningWhereItIs()
        {
            var node = AddNumberNode();
            AddWarning(node);

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);

            CollectionAssert.AreEqual(new[] { AIIcon, WarningIcon, null }, ZoomedOutIconsOf(node),
                "The warning keeps its place; the AI icon never goes above the bottom row.");
        }

        [Test]
        [Category("UnitTests")]
        public void AIIconIsLeftOutWhenFrozenAndPreviewOffFillTheBottomRow()
        {
            var node = AddNumberNode();
            Freeze(node);
            TurnPreviewOff(node);

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);

            CollectionAssert.AreEqual(new[] { FrozenIcon, PreviewOffIcon, null }, ZoomedOutIconsOf(node));
        }

        [Test]
        [Category("UnitTests")]
        public void AIIconIsLeftOutWhenAStateIconAndAWarningFillTheBottomRow()
        {
            var node = AddNumberNode();
            Freeze(node);
            AddWarning(node);

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);

            CollectionAssert.AreEqual(new[] { FrozenIcon, WarningIcon, null }, ZoomedOutIconsOf(node),
                "The free place above the bottom row is not used for the AI icon.");
        }

        [Test]
        [Category("UnitTests")]
        public void AIIconIsLeftOutWhenAllPlacesAreTaken()
        {
            var node = AddNumberNode();
            Freeze(node);
            TurnPreviewOff(node);
            AddWarning(node);

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);

            CollectionAssert.AreEqual(new[] { FrozenIcon, PreviewOffIcon, WarningIcon }, ZoomedOutIconsOf(node));
        }

        [Test]
        [Category("UnitTests")]
        public void StateIconsAddedAfterTheAIChangeTakePriority()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            CollectionAssert.AreEqual(new[] { AIIcon, null, null }, ZoomedOutIconsOf(node));

            Freeze(node);
            CollectionAssert.AreEqual(new[] { FrozenIcon, AIIcon, null }, ZoomedOutIconsOf(node));

            TurnPreviewOff(node);
            CollectionAssert.AreEqual(new[] { FrozenIcon, PreviewOffIcon, null }, ZoomedOutIconsOf(node));
        }

        [Test]
        [Category("UnitTests")]
        public void AIIconGoesWhenTheChangeRecordIsCleared()
        {
            var node = AddNumberNode();
            Freeze(node);
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            CollectionAssert.AreEqual(new[] { FrozenIcon, AIIcon, null }, ZoomedOutIconsOf(node));

            Workspace.ClearAIChanges();

            CollectionAssert.AreEqual(new[] { FrozenIcon, null, null }, ZoomedOutIconsOf(node),
                "Only the AI icon goes; the frozen icon stays.");
        }

        [Test]
        [Category("UnitTests")]
        public void AIIconStaysAfterTheHaloFadesOut()
        {
            var now = TimeSpan.FromMinutes(1);
            AIHaloGlow.Now = () => now;
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            var nodeView = ViewOf(node);

            now += AIHaloGlow.FadeOutEnd;
            nodeView.UpdateAIHalo();

            Assert.AreEqual(Visibility.Collapsed, nodeView.AIHalo.Visibility, "The halo still fades out on time.");
            CollectionAssert.AreEqual(new[] { AIIcon, null, null }, ZoomedOutIconsOf(node),
                "Like the corner badge, the icon stays until the change record is cleared.");
        }

        [Test]
        [Category("UnitTests")]
        public void AINodeIsNotTintedWhenZoomedOut()
        {
            var node = AddNumberNode();
            var overlayBefore = ViewModelOf(node).NodeOverlayColor;

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            DispatcherUtil.DoEvents();

            Assert.AreSame(overlayBefore, ViewModelOf(node).NodeOverlayColor, "The halo is the AI colour signal.");
        }

        [Test]
        [Category("UnitTests")]
        public void HaloStillShowsWhenZoomedOut()
        {
            ViewModel.CurrentSpaceViewModel.Zoom = 0.3;
            var node = AddNumberNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            var nodeView = ViewOf(node);

            Assert.AreEqual(Visibility.Visible, nodeView.AIHalo.Visibility);
            Assert.IsNotNull(nodeView.AIHaloClock, "The glow plays as usual.");
        }

        #endregion
    }

    /// <summary>
    /// The halo glow's timeline (DYN-11004, Figma node 20:338), checked without a Dynamo window.
    /// </summary>
    [TestFixture]
    public class AIHaloGlowTimelineTests
    {
        private const double Tolerance = 1e-6;
        private static readonly TimeSpan Start = TimeSpan.FromMinutes(5);

        private static int lastTestAIEdit;

        [SetUp]
        public void FixTheClock()
        {
            AIHaloGlow.Now = () => Start;
        }

        [TearDown]
        public void RestoreTheClock()
        {
            AIHaloGlow.Now = AIHaloGlow.DefaultClock;
        }

        /// <summary>
        /// A new AI edit number. Negative, so it never matches one a workspace hands out.
        /// </summary>
        private static int NewAIEdit() => --lastTestAIEdit;

        private static AIHaloGlow NewGlow(double strength = 1.0, bool breathes = true)
        {
            return AIHaloGlow.Start(NewAIEdit(), strength, breathes);
        }

        /// <summary>The animation's value at a time, worked out from its key frames.</summary>
        private static double ValueAt(DoubleAnimationUsingKeyFrames animation, double seconds)
        {
            var time = TimeSpan.FromSeconds(seconds);
            var previousValue = 0.0;
            var previousTime = TimeSpan.Zero;
            foreach (DoubleKeyFrame frame in animation.KeyFrames)
            {
                var frameTime = frame.KeyTime.TimeSpan;
                if (time <= frameTime)
                {
                    if (frameTime == previousTime) return frame.Value;
                    var progress = (time - previousTime).TotalMilliseconds / (frameTime - previousTime).TotalMilliseconds;
                    return frame.InterpolateValue(previousValue, progress);
                }
                previousValue = frame.Value;
                previousTime = frameTime;
            }
            return previousValue;
        }

        [Test]
        [Category("UnitTests")]
        public void BreathingGlowFollowsTheTimeline()
        {
            var animation = NewGlow().CreateAnimation();

            Assert.AreEqual(14.9, animation.Duration.TimeSpan.TotalSeconds, Tolerance);
            Assert.AreEqual(0.00, ValueAt(animation, 0.0), Tolerance, "Starts invisible");
            Assert.AreEqual(1.00, ValueAt(animation, 0.6), Tolerance, "Fade-in complete");
            Assert.AreEqual(0.65, ValueAt(animation, 1.6), Tolerance, "First breath trough");
            Assert.AreEqual(1.00, ValueAt(animation, 2.6), Tolerance, "First breath peak");
            Assert.AreEqual(0.65, ValueAt(animation, 3.6), Tolerance, "Second breath trough");
            Assert.AreEqual(1.00, ValueAt(animation, 4.6), Tolerance, "Second breath peak");
            Assert.AreEqual(0.70, ValueAt(animation, 5.4), Tolerance, "Settled");
            Assert.AreEqual(0.70, ValueAt(animation, 9.0), Tolerance, "Steady hold");
            Assert.AreEqual(0.70, ValueAt(animation, 13.4), Tolerance, "End of hold");
            Assert.AreEqual(0.00, ValueAt(animation, 14.9), Tolerance, "Faded out");
        }

        [Test]
        [Category("UnitTests")]
        public void BreathsNeverDropBelowSixtyFivePercent()
        {
            var animation = NewGlow().CreateAnimation();

            for (var ms = 600; ms <= 4600; ms += 10)
            {
                var value = ValueAt(animation, ms / 1000.0);
                Assert.GreaterOrEqual(value, 0.65 - Tolerance, $"at {ms} ms");
                Assert.LessOrEqual(value, 1.0 + Tolerance, $"at {ms} ms");
            }
        }

        [Test]
        [Category("UnitTests")]
        public void FadeInDeceleratesAndBreathsEaseInAndOut()
        {
            var animation = NewGlow().CreateAnimation();

            // Ease-out cubic-bezier(0, 0, 0.2, 1): well past half way at half the time
            Assert.Greater(ValueAt(animation, 0.3), 0.75);

            // Ease-in-out cubic-bezier(0.4, 0, 0.6, 1): slow at both ends of a half breath, half way in the middle
            Assert.AreEqual(0.825, ValueAt(animation, 1.1), 0.01);
            Assert.Greater(ValueAt(animation, 0.7), 0.98);
            Assert.Less(ValueAt(animation, 1.5), 0.67);
        }

        [Test]
        [Category("UnitTests")]
        public void MovedNodeGlowFadesInHoldsAndFadesOutWithoutBreathing()
        {
            var animation = NewGlow(NodeViewModel.AIHaloMovedOpacity, breathes: false).CreateAnimation();

            Assert.AreEqual(14.9, animation.Duration.TimeSpan.TotalSeconds, Tolerance);
            Assert.AreEqual(0.0, ValueAt(animation, 0.0), Tolerance);
            for (var ms = 600; ms <= 13400; ms += 100)
            {
                Assert.AreEqual(0.4, ValueAt(animation, ms / 1000.0), Tolerance, $"steady 40% at {ms} ms");
            }
            Assert.AreEqual(0.0, ValueAt(animation, 14.9), Tolerance);
        }

        [Test]
        [Category("UnitTests")]
        public void StrongerGlowKeepsItsStartAndItsSlot()
        {
            var edit = NewAIEdit();
            AIHaloGlow.Start(edit, 1.0, true);
            var glow = AIHaloGlow.Start(edit, NodeViewModel.AIHaloMovedOpacity, false);

            var upgraded = glow.WithStrength(1.0, true);

            Assert.AreEqual(1.0, upgraded.Strength);
            Assert.IsTrue(upgraded.Breathes);
            Assert.AreEqual(glow.StartedAt, upgraded.StartedAt);
            Assert.AreEqual(TimeSpan.FromMilliseconds(100), upgraded.Delay);
        }

        [Test]
        [Category("UnitTests")]
        public void GlowOutsideAnAIEditStartsAloneWithoutDelay()
        {
            var first = AIHaloGlow.Start(0, 1.0, true);
            var second = AIHaloGlow.Start(0, 1.0, true);

            Assert.AreEqual(TimeSpan.Zero, first.Delay);
            Assert.AreEqual(TimeSpan.Zero, second.Delay);
        }

        [Test]
        [Category("UnitTests")]
        public void StaggerIsOneHundredMillisecondsPerNodeUpToOneSecond()
        {
            var edit = NewAIEdit();
            var delays = Enumerable.Range(0, 13)
                .Select(_ => AIHaloGlow.Start(edit, 1.0, true).Delay.TotalMilliseconds)
                .ToList();

            CollectionAssert.AreEqual(
                new[] { 0.0, 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 1000, 1000 },
                delays);
        }

        [Test]
        [Category("UnitTests")]
        public void DelayedGlowWaitsThenRunsTheSameTimeline()
        {
            var edit = NewAIEdit();
            AIHaloGlow.Start(edit, 1.0, true);
            AIHaloGlow.Start(edit, 1.0, true);
            var third = AIHaloGlow.Start(edit, 1.0, true);
            var animation = third.CreateAnimation();

            Assert.AreEqual(0.2, third.Delay.TotalSeconds, Tolerance);
            Assert.AreEqual(15.1, animation.Duration.TimeSpan.TotalSeconds, Tolerance);
            Assert.AreEqual(0.0, ValueAt(animation, 0.1), Tolerance, "Waits its turn");
            Assert.AreEqual(0.0, ValueAt(animation, 0.2), Tolerance);
            Assert.AreEqual(1.0, ValueAt(animation, 0.8), Tolerance, "Fade-in complete 0.6 s after its start");
            Assert.AreEqual(0.0, ValueAt(animation, 15.1), Tolerance);
        }

        [Test]
        [Category("UnitTests")]
        public void RestartedGlowStartsFromTheCurrentOpacity()
        {
            var animation = NewGlow().CreateAnimation(startOpacity: 0.7);

            Assert.AreEqual(0.7, ValueAt(animation, 0.0), Tolerance, "No dip to zero when a running glow is replaced");
            Assert.AreEqual(1.0, ValueAt(animation, 0.6), Tolerance);
        }

        [Test]
        [Category("UnitTests")]
        public void GlowIsFinishedAtTheEndOfTheFadeOut()
        {
            var now = Start;
            AIHaloGlow.Now = () => now;
            var glow = NewGlow();

            now = Start + TimeSpan.FromSeconds(14.8);
            Assert.IsFalse(glow.IsFinished);

            now = Start + TimeSpan.FromSeconds(14.9);
            Assert.IsTrue(glow.IsFinished);
        }
    }
}
