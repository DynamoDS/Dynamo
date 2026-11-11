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
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Views;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    /// <summary>
    /// Tests for the AI-touched halo (DYN-10999): the soft purple glow drawn behind nodes the
    /// AI Assistant placed, modified or moved, its breathing glow and fade-out (DYN-11004), its clock
    /// (DYN-11005: starts on screen, hover holds it, Esc ends it, a new touch restarts it), the
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

        private WorkspaceView WorkspaceViewOf()
        {
            return View.WorkspaceTabs.ChildrenOfType<WorkspaceView>().First();
        }

        /// <summary>
        /// Runs the on-screen check now instead of waiting for the view to settle, so every waiting
        /// glow whose node is on screen starts. Returns the nodes whose glow started.
        /// </summary>
        private IReadOnlyList<NodeViewModel> ShowGlows()
        {
            DispatcherUtil.DoEvents();
            var started = WorkspaceViewOf().CheckAIHaloVisibility();
            DispatcherUtil.DoEvents();
            return started;
        }

        private const double OffScreenX = 100000;

        /// <summary>
        /// A node far outside the window, so the view's own on-screen check never starts its glow.
        /// Tests bring it "on screen" by passing a matching area to <see cref="StartGlowsIn"/>.
        /// </summary>
        private NodeModel AddNodeOffScreen(double x = OffScreenX, double y = 0)
        {
            var node = AddNumberNode();
            node.X = x;
            node.Y = y;
            DispatcherUtil.DoEvents();
            return node;
        }

        /// <summary>
        /// The workspace's on-screen check, as if <paramref name="visibleBounds"/> were the part of
        /// the canvas on screen.
        /// </summary>
        private IReadOnlyList<NodeViewModel> StartGlowsIn(Rect visibleBounds)
        {
            var started = ViewModel.CurrentSpaceViewModel.StartAIHaloGlowsInView(visibleBounds);
            DispatcherUtil.DoEvents();
            return started;
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
            ShowGlows();

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
            ShowGlows();

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
            ShowGlows();

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
            ShowGlows();

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
            ShowGlows();
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
            ShowGlows();
            var halo = ViewOf(node).AIHalo;
            var badge = ViewOf(node).AIBadge;

            // Behind every other part of the node.
            var haloZIndex = Panel.GetZIndex(halo);
            foreach (var child in nodeView.grid.Children.OfType<UIElement>().Where(c => c != halo))
            {
                Assert.Less(haloZIndex, Panel.GetZIndex(child));
            }

            // Extends past the node edge and never takes mouse clicks.
            Assert.AreEqual(new System.Windows.Thickness(-8), halo.Margin);
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
            ShowGlows();

            var halo = (Grid)ViewOf(node).AIHalo;
            var glow = halo.Children.OfType<Border>().Single();

            Assert.IsTrue(halo.HasAnimatedProperties, "The halo's opacity is animated.");
            Assert.IsFalse(glow.HasAnimatedProperties, "The blurred glow itself never changes.");
            Assert.IsFalse(glow.Effect.HasAnimatedProperties, "No blur changes.");
            Assert.AreEqual(new System.Windows.Thickness(-8), halo.Margin, "No size changes.");
        }

        [Test]
        [Category("UnitTests")]
        public void HaloIsGoneAfterTheFadeOutButTheBadgeStays()
        {
            var now = TimeSpan.FromMinutes(1);
            AIHaloGlow.Now = () => now;
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            ShowGlows();
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
            ShowGlows();
            Assert.IsNotNull(ViewOf(node).AIHaloClock);

            // 6 s in, the glow is in its steady hold. A new view for the same node, for example
            // after switching tabs, must join it there rather than fade in from zero.
            now += TimeSpan.FromSeconds(6);
            var rebuilt = new NodeView { DataContext = ViewModelOf(node) };
            rebuilt.UpdateAIHalo();

            var clock = rebuilt.AIHaloClock;
            Assert.IsNotNull(clock);
            WaitUntilPlayingFrom(clock, TimeSpan.FromSeconds(6));
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
            ShowGlows();
            var clock = ViewOf(node).AIHaloClock;
            Assert.IsNotNull(clock);
            WaitUntil(() => clock.CurrentTime != null, TimeSpan.FromSeconds(3));

            clock.Controller.SkipToFill();
            WaitUntil(() => nodeView.AIHaloClock == null, TimeSpan.FromSeconds(3));

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
            ShowGlows();
            Assert.AreEqual(Brushes.Transparent, nodeView.nodeBorder.BorderBrush);

            now += AIHaloGlow.FadeOutEnd;
            nodeView.UpdateAIHalo();

            Assert.AreSame(borderBrushBefore, nodeView.nodeBorder.BorderBrush);
        }

        #endregion

        #region Glow clock: starts on screen (DYN-11005)

        [Test]
        [Category("UnitTests")]
        public void GlowWaitsUntilTheNodeIsOnScreen()
        {
            var now = TimeSpan.FromMinutes(1);
            AIHaloGlow.Now = () => now;
            var node = AddNodeOffScreen();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            ShowGlows();

            var glow = ViewModelOf(node).AIHaloGlow;
            Assert.IsTrue(glow.IsWaiting, "Off screen, the clock has not started.");
            Assert.IsNull(ViewOf(node).AIHaloClock);
            Assert.IsTrue(ViewOf(node).AIHalo == null || ViewOf(node).AIHalo.Visibility == Visibility.Collapsed);
            Assert.AreEqual(Visibility.Visible, ViewOf(node).AIBadge.Visibility, "The badge shows straight away.");

            // Long after the change, the node comes on screen
            now += TimeSpan.FromMinutes(5);
            var started = StartGlowsIn(AroundNode(node));

            CollectionAssert.AreEqual(new[] { ViewModelOf(node) }, started);
            Assert.IsTrue(glow.IsGlowing);
            Assert.AreEqual(TimeSpan.Zero, glow.Elapsed, "The clock starts now, not at the change.");
            Assert.AreEqual(Visibility.Visible, ViewOf(node).AIHalo.Visibility);
        }

        [Test]
        [Category("UnitTests")]
        public void GlowStartsOnlyWhenAtLeastHalfOfTheNodeIsOnScreen()
        {
            var node = AddNodeOffScreen();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            Assert.Greater(node.Width, 0, "The node has been measured.");

            // 40% of the node's width in view
            var fortyPercent = new Rect(node.X + node.Width * 0.6, node.Y - 100, 1000, node.Height + 200);
            Assert.IsEmpty(StartGlowsIn(fortyPercent));
            Assert.IsTrue(ViewModelOf(node).AIHaloGlow.IsWaiting);

            // 60% in view
            var sixtyPercent = new Rect(node.X + node.Width * 0.4, node.Y - 100, 1000, node.Height + 200);
            Assert.AreEqual(1, StartGlowsIn(sixtyPercent).Count);
            Assert.IsTrue(ViewModelOf(node).AIHaloGlow.IsGlowing);
        }

        [Test]
        [Category("UnitTests")]
        public void NodeInACollapsedGroupWaitsUntilItIsShown()
        {
            var node = AddNodeOffScreen();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            var nodeViewModel = ViewModelOf(node);

            nodeViewModel.IsCollapsed = true;
            Assert.IsEmpty(StartGlowsIn(AroundNode(node)));
            Assert.IsTrue(nodeViewModel.AIHaloGlow.IsWaiting);

            nodeViewModel.IsCollapsed = false;
            Assert.AreEqual(1, StartGlowsIn(AroundNode(node)).Count);
        }

        [Test]
        [Category("UnitTests")]
        public void ViewStartsTheGlowOnItsOwnOnceItSettles()
        {
            var node = AddNumberNode();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);

            Assert.IsTrue(WaitUntil(() => ViewModelOf(node).AIHaloGlow.IsStarted, TimeSpan.FromSeconds(3)),
                "A node already on screen starts once the view has been still for a moment.");
            Assert.AreEqual(Visibility.Visible, ViewOf(node).AIHalo.Visibility);
        }

        [Test]
        [Category("UnitTests")]
        public void PanningANodeOnScreenStartsItsGlow()
        {
            var node = AddNodeOffScreen();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            ShowGlows();
            Assert.IsTrue(ViewModelOf(node).AIHaloGlow.IsWaiting);

            // Pan so the node sits near the top-left corner of the view
            WorkspaceViewOf().zoomBorder.SetTranslateTransformOrigin(new(200 - node.X, 200 - node.Y));

            Assert.IsTrue(WaitUntil(() => ViewModelOf(node).AIHaloGlow.IsStarted, TimeSpan.FromSeconds(3)));
        }

        [Test]
        [Category("UnitTests")]
        public void DraggingANodeOnScreenStartsItsGlow()
        {
            var node = AddNodeOffScreen();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            ShowGlows();
            Assert.IsTrue(ViewModelOf(node).AIHaloGlow.IsWaiting);

            // The node moves into view; the view itself does not pan or zoom
            node.X = 50;
            node.Y = 50;

            Assert.IsTrue(WaitUntil(() => ViewModelOf(node).AIHaloGlow?.IsStarted == true, TimeSpan.FromSeconds(3)),
                "Moving the node into view starts its glow.");
        }

        [Test]
        [Category("UnitTests")]
        public void GlowWaitsBehindTheHomePageAndStartsOnReturn()
        {
            var node = AddNumberNode();
            ViewModel.ShowStartPage = true;
            try
            {
                Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
                ShowGlows();
                Assert.IsTrue(ViewModelOf(node).AIHaloGlow.IsWaiting, "The Home page covers the node.");
            }
            finally
            {
                ViewModel.ShowStartPage = false;
            }

            Assert.IsTrue(WaitUntil(() => ViewModelOf(node).AIHaloGlow.IsStarted, TimeSpan.FromSeconds(3)),
                "Back on the graph, the glow starts.");
        }

        [Test]
        [Category("UnitTests")]
        public void NodesThatComeOnScreenTogetherAreStaggered()
        {
            var nodes = Enumerable.Range(0, 3).Select(i => AddNodeOffScreen(OffScreenX + i * 300)).ToList();
            foreach (var node in nodes)
            {
                Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            }

            var started = StartGlowsIn(new Rect(OffScreenX - 100, -100, 2000, 1000));

            CollectionAssert.AreEqual(nodes.Select(ViewModelOf), started);
            CollectionAssert.AreEqual(new[] { 0.0, 100, 200 }, DelaysOf(nodes), "100 ms apart.");
        }

        [Test]
        [Category("UnitTests")]
        public void StaggerFollowsTheOrderTheNodesWereTouched()
        {
            var nodes = Enumerable.Range(0, 3).Select(i => AddNodeOffScreen(OffScreenX + i * 300)).ToList();

            // Touched right to left
            foreach (var node in Enumerable.Reverse(nodes))
            {
                Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            }
            StartGlowsIn(new Rect(OffScreenX - 100, -100, 2000, 1000));

            CollectionAssert.AreEqual(new[] { 200.0, 100, 0 }, DelaysOf(nodes));
        }

        [Test]
        [Category("UnitTests")]
        public void NodeThatComesOnScreenAloneStartsWithoutDelay()
        {
            var first = AddNodeOffScreen(OffScreenX);
            var second = AddNodeOffScreen(OffScreenX + 5000);
            Workspace.RecordAINodeChange(first.GUID, AIChangeKind.Placed, TurnA);
            Workspace.RecordAINodeChange(second.GUID, AIChangeKind.Placed, TurnA);

            StartGlowsIn(AroundNode(first));
            Assert.IsTrue(ViewModelOf(second).AIHaloGlow.IsWaiting);
            StartGlowsIn(AroundNode(second));

            CollectionAssert.AreEqual(new[] { 0.0, 0.0 }, DelaysOf(new[] { first, second }));
        }

        [Test]
        [Category("UnitTests")]
        public void NodesChangedSeveralTimesTakeOneStaggerSlotEach()
        {
            var nodes = Enumerable.Range(0, 5).Select(i => AddNodeOffScreen(OffScreenX + i * 300)).ToList();

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
            StartGlowsIn(new Rect(OffScreenX - 100, -100, 3000, 1000));

            CollectionAssert.AreEqual(new[] { 0.0, 100, 200, 300, 400 }, DelaysOf(nodes));
        }

        [Test]
        [Category("UnitTests")]
        public void NodeChangedAgainBeforeItIsSeenKeepsWaitingAtTheNewStrength()
        {
            var node = AddNodeOffScreen();

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Moved, TurnA, null, "0, 0", "100, 50");
            var glow = ViewModelOf(node).AIHaloGlow;
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");

            Assert.AreSame(glow, ViewModelOf(node).AIHaloGlow);
            Assert.IsTrue(glow.IsWaiting);
            Assert.AreEqual(NodeViewModel.AIHaloFullOpacity, glow.Strength);
            Assert.IsTrue(glow.Breathes);
        }

        #endregion

        #region Glow clock: hover, Esc, restart (DYN-11005)

        [Test]
        [Category("UnitTests")]
        public void HoverHoldsTheGlowAndLeavingLetsItRunOn()
        {
            var now = TimeSpan.FromMinutes(1);
            AIHaloGlow.Now = () => now;
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            ShowGlows();
            var nodeViewModel = ViewModelOf(node);
            var glow = nodeViewModel.AIHaloGlow;
            var clock = ViewOf(node).AIHaloClock;

            now += TimeSpan.FromSeconds(2);
            nodeViewModel.SetAIHaloHovered(true);
            DispatcherUtil.DoEvents();
            now += TimeSpan.FromSeconds(10);

            Assert.IsTrue(glow.IsHeld);
            Assert.AreEqual(TimeSpan.FromSeconds(2), glow.Elapsed, "Time while hovered does not count.");
            Assert.IsTrue(WaitUntil(() => clock.IsPaused, TimeSpan.FromSeconds(3)), "The halo stops at its current value.");
            Assert.AreSame(clock, ViewOf(node).AIHaloClock);

            nodeViewModel.SetAIHaloHovered(false);
            DispatcherUtil.DoEvents();
            now += TimeSpan.FromSeconds(1);

            Assert.IsFalse(glow.IsHeld);
            Assert.AreEqual(TimeSpan.FromSeconds(3), glow.Elapsed, "Runs on from where it was held.");
            Assert.IsTrue(WaitUntil(() => !clock.IsPaused, TimeSpan.FromSeconds(3)));
            Assert.AreSame(clock, ViewOf(node).AIHaloClock, "Same animation, not a restart.");
        }

        [Test]
        [Category("UnitTests")]
        public void NodeThatComesOnScreenUnderTheMouseStartsHeld()
        {
            var now = TimeSpan.FromMinutes(1);
            AIHaloGlow.Now = () => now;
            var node = AddNodeOffScreen();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            var nodeViewModel = ViewModelOf(node);

            nodeViewModel.SetAIHaloHovered(true);
            StartGlowsIn(AroundNode(node));
            now += TimeSpan.FromSeconds(5);

            var glow = nodeViewModel.AIHaloGlow;
            Assert.IsTrue(glow.IsStarted);
            Assert.AreEqual(TimeSpan.Zero, glow.Elapsed);

            nodeViewModel.SetAIHaloHovered(false);
            now += TimeSpan.FromSeconds(1);
            Assert.AreEqual(TimeSpan.FromSeconds(1), glow.Elapsed);
        }

        [Test]
        [Category("UnitTests")]
        public void EscEndsTheGlowOfTheHoveredNodeOnlyAndKeepsTheBadge()
        {
            var hovered = AddNumberNode();
            var other = AddNumberNode();
            Workspace.RecordAINodeChange(hovered.GUID, AIChangeKind.Placed, TurnA);
            Workspace.RecordAINodeChange(other.GUID, AIChangeKind.Placed, TurnA);
            ShowGlows();
            var unsavedBefore = Workspace.HasUnsavedChanges;

            ViewModelOf(hovered).SetAIHaloHovered(true);
            Assert.IsTrue(ViewModel.CurrentSpaceViewModel.DismissHoveredAIHaloGlow());
            DispatcherUtil.DoEvents();

            var glow = ViewModelOf(hovered).AIHaloGlow;
            Assert.IsTrue(glow.IsDismissed);
            Assert.IsTrue(glow.IsFinished);
            Assert.AreEqual(Visibility.Collapsed, ViewOf(hovered).AIHalo.Visibility);
            Assert.IsNull(ViewOf(hovered).AIHaloClock);
            Assert.AreEqual(Visibility.Visible, ViewOf(hovered).AIBadge.Visibility, "Esc keeps the badge.");
            Assert.IsTrue(hovered.IsRecentlyModifiedByAI, "The change record is kept.");

            Assert.IsTrue(ViewModelOf(other).AIHaloGlow.IsGlowing, "Other nodes keep glowing.");
            Assert.AreEqual(Visibility.Visible, ViewOf(other).AIHalo.Visibility);
            Assert.AreEqual(unsavedBefore, Workspace.HasUnsavedChanges, "Dismissing never marks the workspace as changed.");
        }

        [Test]
        [Category("UnitTests")]
        public void EscDoesNothingWithoutAHoveredGlowingNode()
        {
            var now = TimeSpan.FromMinutes(1);
            AIHaloGlow.Now = () => now;
            var glowing = AddNumberNode();
            var waiting = AddNodeOffScreen();
            Workspace.RecordAINodeChange(glowing.GUID, AIChangeKind.Placed, TurnA);
            Workspace.RecordAINodeChange(waiting.GUID, AIChangeKind.Placed, TurnA);
            ShowGlows();

            // Nothing hovered
            Assert.IsFalse(ViewModel.CurrentSpaceViewModel.DismissHoveredAIHaloGlow());

            // Hovering a node whose glow is still waiting
            ViewModelOf(waiting).SetAIHaloHovered(true);
            Assert.IsFalse(ViewModel.CurrentSpaceViewModel.DismissHoveredAIHaloGlow());
            ViewModelOf(waiting).SetAIHaloHovered(false);

            // Hovering a node whose glow has faded out: only the badge is left
            now += AIHaloGlow.FadeOutEnd + TimeSpan.FromSeconds(1);
            ViewModelOf(glowing).SetAIHaloHovered(true);
            Assert.IsFalse(ViewModel.CurrentSpaceViewModel.DismissHoveredAIHaloGlow());
            Assert.IsFalse(ViewModelOf(glowing).AIHaloGlow.IsDismissed);
        }

        [Test]
        [Category("UnitTests")]
        public void DismissedGlowStaysGoneInARebuiltView()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            ShowGlows();
            ViewModelOf(node).SetAIHaloHovered(true);
            ViewModel.CurrentSpaceViewModel.DismissHoveredAIHaloGlow();

            var rebuilt = new NodeView { DataContext = ViewModelOf(node) };
            rebuilt.UpdateAIHalo();

            Assert.IsNull(rebuilt.AIHaloClock);
            Assert.IsNull(rebuilt.AIHalo, "No halo is created for a dismissed glow.");
        }

        [Test]
        [Category("UnitTests")]
        public void NodeTouchedAgainWhileGlowingRestartsItsGlow()
        {
            var now = TimeSpan.FromMinutes(1);
            AIHaloGlow.Now = () => now;
            var node = AddNumberNode();
            AsAIEdit(() => Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA));
            ShowGlows();
            var glow = ViewModelOf(node).AIHaloGlow;
            var clock = ViewOf(node).AIHaloClock;
            var version = glow.Version;

            // A later AI edit, 8 s into the glow
            now += TimeSpan.FromSeconds(8);
            AsAIEdit(() => Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2"));

            // The current glow keeps playing until the on-screen check, which finds the node on screen
            Assert.IsTrue(glow.IsRestartPending);
            Assert.AreEqual(version, glow.Version);
            ShowGlows();

            Assert.AreSame(glow, ViewModelOf(node).AIHaloGlow);
            Assert.IsFalse(glow.IsRestartPending);
            Assert.AreNotEqual(version, glow.Version, "The timeline starts again.");
            Assert.AreEqual(TimeSpan.Zero, glow.Elapsed, "The timer restarts from the new touch.");
            Assert.AreEqual(TimeSpan.Zero, glow.Delay, "No stagger wait on a restart.");
            Assert.AreNotSame(clock, ViewOf(node).AIHaloClock, "The halo plays the restarted glow.");
            Assert.AreEqual(Visibility.Visible, ViewOf(node).AIHalo.Visibility);
        }

        [Test]
        [Category("UnitTests")]
        public void NodeMovedThenModifiedWhileGlowingRestartsAtFullStrength()
        {
            var now = TimeSpan.FromMinutes(1);
            AIHaloGlow.Now = () => now;
            var node = AddNumberNode();
            AsAIEdit(() => Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Moved, TurnA, null, "0, 0", "100, 50"));
            ShowGlows();

            now += TimeSpan.FromSeconds(8);
            AsAIEdit(() => Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2"));
            ShowGlows();

            var glow = ViewModelOf(node).AIHaloGlow;
            Assert.AreEqual(NodeViewModel.AIHaloFullOpacity, glow.Strength);
            Assert.IsTrue(glow.Breathes);
            Assert.AreEqual(TimeSpan.Zero, glow.Elapsed);
        }

        [Test]
        [Category("UnitTests")]
        public void NodeTouchedAgainAfterItLeftTheScreenWaitsUntilItIsBack()
        {
            var now = TimeSpan.FromMinutes(1);
            AIHaloGlow.Now = () => now;
            var node = AddNodeOffScreen();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            StartGlowsIn(AroundNode(node));
            var glow = ViewModelOf(node).AIHaloGlow;
            Assert.IsTrue(glow.IsGlowing);

            // The view moves away, then a second change arrives 5 s into the glow
            now += TimeSpan.FromSeconds(5);
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");
            StartGlowsIn(new Rect(0, 0, 1000, 1000));

            Assert.IsTrue(glow.IsWaiting, "Off screen, the second change waits to be seen.");
            Assert.IsFalse(glow.IsRestartPending);
            Assert.AreEqual(Visibility.Collapsed, ViewOf(node).AIHalo.Visibility);
            Assert.IsNull(ViewOf(node).AIHaloClock);

            // A minute later the view comes back
            now += TimeSpan.FromMinutes(1);
            CollectionAssert.AreEqual(new[] { ViewModelOf(node) }, StartGlowsIn(AroundNode(node)));

            Assert.IsTrue(glow.IsGlowing, "The glow is still there to be seen.");
            Assert.AreEqual(TimeSpan.Zero, glow.Elapsed);
            Assert.AreEqual(Visibility.Visible, ViewOf(node).AIHalo.Visibility);
        }

        [Test]
        [Category("UnitTests")]
        public void SecondChangeIsNotLostWhenTheFirstGlowEndsBeforeTheCheck()
        {
            var now = TimeSpan.FromMinutes(1);
            AIHaloGlow.Now = () => now;
            var node = AddNodeOffScreen();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            StartGlowsIn(AroundNode(node));
            var glow = ViewModelOf(node).AIHaloGlow;

            // Touched again just before the fade-out ends; the first timeline then runs out
            now += AIHaloGlow.FadeOutEnd - TimeSpan.FromMilliseconds(50);
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");
            now += TimeSpan.FromSeconds(1);

            Assert.IsFalse(glow.IsFinished, "The second change has not been seen yet.");
            StartGlowsIn(AroundNode(node));
            Assert.IsTrue(glow.IsGlowing);
            Assert.AreEqual(TimeSpan.Zero, glow.Elapsed);
        }

        [Test]
        [Category("UnitTests")]
        public void ViewRebuiltDuringARestartedFadeInKeepsItsStartingStrength()
        {
            var now = TimeSpan.FromMinutes(1);
            AIHaloGlow.Now = () => now;
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            ShowGlows();
            var nodeView = ViewOf(node);

            // 8 s in, the halo holds at 70%
            now += TimeSpan.FromSeconds(8);
            nodeView.UpdateAIHalo();
            WaitUntilPlayingFrom(nodeView.AIHaloClock, TimeSpan.FromSeconds(8));
            Assert.AreEqual(AIHaloGlow.HoldLevel, nodeView.AIHalo.Opacity, 0.01);

            // Touched again: the glow restarts from 70%
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");
            ShowGlows();
            var glow = ViewModelOf(node).AIHaloGlow;
            Assert.AreEqual(AIHaloGlow.HoldLevel, glow.StartOpacityFor(glow.Version, 0), 0.01);

            // A view rebuilt during the fade-in starts from the same strength, not from zero
            var rebuilt = new NodeView { DataContext = ViewModelOf(node) };
            rebuilt.UpdateAIHalo();
            WaitUntil(() => rebuilt.AIHaloClock?.CurrentTime != null, TimeSpan.FromSeconds(3));

            Assert.GreaterOrEqual(rebuilt.AIHalo.Opacity, AIHaloGlow.HoldLevel - 0.01, "Not dimmed by the rebuild.");
        }

        [Test]
        [Category("UnitTests")]
        public void NodeTouchedAgainAfterItsGlowFadedGlowsAgain()
        {
            var now = TimeSpan.FromMinutes(1);
            AIHaloGlow.Now = () => now;
            var node = AddNumberNode();
            AsAIEdit(() => Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA));
            ShowGlows();
            var first = ViewModelOf(node).AIHaloGlow;

            // Touched again after the fade-out, for example late in a long turn
            now += AIHaloGlow.FadeOutEnd + TimeSpan.FromSeconds(45);
            AsAIEdit(() => Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2"));
            ShowGlows();

            var second = ViewModelOf(node).AIHaloGlow;
            Assert.AreNotSame(first, second);
            Assert.IsTrue(second.IsGlowing);
            Assert.AreEqual(TimeSpan.Zero, second.Elapsed);
            Assert.AreEqual(Visibility.Visible, ViewOf(node).AIHalo.Visibility);
        }

        [Test]
        [Category("UnitTests")]
        public void NodeTouchedAgainAfterEscGlowsAgain()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            ShowGlows();
            var nodeViewModel = ViewModelOf(node);
            nodeViewModel.SetAIHaloHovered(true);
            nodeViewModel.DismissAIHaloGlow();
            nodeViewModel.SetAIHaloHovered(false);

            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Modified, TurnA, "Value", "1", "2");
            ShowGlows();

            Assert.IsTrue(nodeViewModel.AIHaloGlow.IsGlowing, "A new change gets a new glow.");
            Assert.AreEqual(Visibility.Visible, ViewOf(node).AIHalo.Visibility);
        }

        #endregion

        /// <summary>
        /// A clock starts, and applies a seek, on WPF's next animation tick. Pumps the UI thread
        /// until then, for at most 3 s (shorter than the seek, so time passing alone cannot pass).
        /// </summary>
        private static void WaitUntilPlayingFrom(AnimationClock clock, TimeSpan time)
        {
            WaitUntil(() => clock.CurrentTime != null && clock.CurrentTime >= time, TimeSpan.FromSeconds(3));
        }

        /// <summary>
        /// Pumps the UI thread until the condition holds or the time runs out. Returns the condition.
        /// </summary>
        private static bool WaitUntil(Func<bool> condition, TimeSpan timeout)
        {
            var giveUpAt = System.DateTime.UtcNow + timeout;
            while (!condition() && System.DateTime.UtcNow < giveUpAt)
            {
                DispatcherUtil.DoEvents();
                Thread.Sleep(15);
            }
            return condition();
        }

        private static Rect AroundNode(NodeModel node)
        {
            return new Rect(node.X - 50, node.Y - 50, Math.Max(node.Width, 1) + 100, Math.Max(node.Height, 1) + 100);
        }

        private void AsAIEdit(Action changes)
        {
            using (Workspace.BeginAIEdit())
            {
                changes();
            }
        }

        private List<double> DelaysOf(IEnumerable<NodeModel> nodes)
        {
            return nodes.Select(n => ViewModelOf(n).AIHaloGlow.Delay.TotalMilliseconds).ToList();
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
            ShowGlows();

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
            ShowGlows();

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
            ShowGlows();

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
            Assert.AreEqual(new System.Windows.Thickness(0, -13.5, -13.5, 0), badge.Margin);

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

        [Test]
        [Category("UnitTests")]
        public void HaloMovesOutsideAFrozenBorderAndReturns()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            ShowGlows();
            Assert.AreEqual(new System.Windows.Thickness(-8), ViewOf(node).AIHalo.Margin);

            // The assistant froze it, so the change record stays. A person's freeze would clear it.
            AsAIEdit(() => node.IsFrozen = true);
            DispatcherUtil.DoEvents();

            Assert.IsNotNull(node.AIChange);
            Assert.AreEqual(new System.Windows.Thickness(-16), ViewOf(node).AIHalo.Margin);
            Assert.AreEqual(new CornerRadius(24, 24, 0, 0), HaloGlow(node).CornerRadius);

            AsAIEdit(() => node.IsFrozen = false);
            DispatcherUtil.DoEvents();

            Assert.AreEqual(new System.Windows.Thickness(-8), ViewOf(node).AIHalo.Margin);
        }

        [Test]
        [Category("UnitTests")]
        public void HaloMovesOutsideAPreviewOffBorder()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            ShowGlows();
            Assert.AreEqual(new System.Windows.Thickness(-8), ViewOf(node).AIHalo.Margin);

            AsAIEdit(() => node.UpdateValue(new UpdateValueParams("IsVisible", "false")));
            DispatcherUtil.DoEvents();

            Assert.IsNotNull(node.AIChange);
            Assert.IsFalse(node.IsVisible);
            Assert.AreEqual(new System.Windows.Thickness(-16), ViewOf(node).AIHalo.Margin);
            Assert.AreEqual(NodeViewModel.AIHaloFullOpacity, ViewModelOf(node).AIHaloOpacity,
                "Preview off draws a border. It does not dim the glow.");
        }

        [Test]
        [Category("UnitTests")]
        public void AlertCapsTheHaloAtSixtyPercentUntilItClears()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            Assert.AreEqual(NodeViewModel.AIHaloFullOpacity, ViewModelOf(node).AIHaloOpacity);

            node.Warning("Index was outside the bounds of the array.", true);
            DispatcherUtil.DoEvents();

            Assert.AreEqual(NodeViewModel.AIHaloAlertOpacity, ViewModelOf(node).AIHaloOpacity);

            node.ClearErrorsAndWarnings();
            DispatcherUtil.DoEvents();

            Assert.AreEqual(NodeViewModel.AIHaloFullOpacity, ViewModelOf(node).AIHaloOpacity);
        }

        [Test]
        [Category("UnitTests")]
        public void MovedNodeWithAnAlertStaysAtTheMovedOpacity()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Moved, TurnA, null, "0, 0", "100, 50");

            node.Warning("Index was outside the bounds of the array.", true);
            DispatcherUtil.DoEvents();

            Assert.AreEqual(NodeViewModel.AIHaloMovedOpacity, ViewModelOf(node).AIHaloOpacity,
                "40% is already under the 60% alert cap.");
        }

        private Border HaloGlow(NodeModel node)
        {
            return ((Grid)ViewOf(node).AIHalo).Children.OfType<Border>().Single();
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
        public void WarningAddedAfterTheAIChangeTakesPriority()
        {
            var node = AddNumberNode();
            Freeze(node);
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            CollectionAssert.AreEqual(new[] { FrozenIcon, AIIcon, null }, ZoomedOutIconsOf(node));

            // A warning is a state change, not a user edit, so the AI change record is kept
            AddWarning(node);

            Assert.IsNotNull(node.AIChange);
            CollectionAssert.AreEqual(new[] { FrozenIcon, WarningIcon, null }, ZoomedOutIconsOf(node),
                "The warning pushes the AI icon out.");
        }

        [Test]
        [Category("UnitTests")]
        public void FreezingAnAINodeRemovesItsAIIcon()
        {
            var node = AddNumberNode();
            Workspace.RecordAINodeChange(node.GUID, AIChangeKind.Placed, TurnA);
            CollectionAssert.AreEqual(new[] { AIIcon, null, null }, ZoomedOutIconsOf(node));

            // Freezing is a user edit: the user takes over the node and its AI change record is cleared
            Freeze(node);

            Assert.IsNull(node.AIChange);
            CollectionAssert.AreEqual(new[] { FrozenIcon, null, null }, ZoomedOutIconsOf(node));
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
            ShowGlows();
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
            ShowGlows();
            var nodeView = ViewOf(node);

            Assert.AreEqual(Visibility.Visible, nodeView.AIHalo.Visibility);
            Assert.IsNotNull(nodeView.AIHaloClock, "The glow plays as usual.");
        }

        #endregion
    }

    /// <summary>
    /// The halo glow's timeline (DYN-11004, Figma node 20:338) and its clock (DYN-11005, Figma node
    /// 7:989), checked without a Dynamo window.
    /// </summary>
    [TestFixture]
    public class AIHaloGlowTimelineTests
    {
        private const double Tolerance = 1e-6;
        private static readonly TimeSpan Start = TimeSpan.FromMinutes(5);

        private TimeSpan now;

        [SetUp]
        public void FixTheClock()
        {
            now = Start;
            AIHaloGlow.Now = () => now;
        }

        [TearDown]
        public void RestoreTheClock()
        {
            AIHaloGlow.Now = AIHaloGlow.DefaultClock;
        }

        /// <summary>A glow whose node has just come on screen.</summary>
        private static AIHaloGlow NewGlow(double strength = 1.0, bool breathes = true, int place = 0)
        {
            var glow = AIHaloGlow.Create(strength, breathes);
            glow.Begin(AIHaloGlow.StaggerDelay(place));
            return glow;
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

        #region Timeline

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
        public void StaggerIsOneHundredMillisecondsPerNodeUpToOneSecond()
        {
            var delays = Enumerable.Range(0, 13)
                .Select(place => AIHaloGlow.StaggerDelay(place).TotalMilliseconds)
                .ToList();

            CollectionAssert.AreEqual(
                new[] { 0.0, 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 1000, 1000 },
                delays);
        }

        [Test]
        [Category("UnitTests")]
        public void DelayedGlowWaitsThenRunsTheSameTimeline()
        {
            var third = NewGlow(place: 2);
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

            Assert.AreEqual(0.7, ValueAt(animation, 0.0), Tolerance, "No dip to zero when a running glow restarts");
            Assert.AreEqual(1.0, ValueAt(animation, 0.6), Tolerance);
        }

        [Test]
        [Category("UnitTests")]
        public void GlowIsFinishedAtTheEndOfTheFadeOut()
        {
            var glow = NewGlow();

            now = Start + TimeSpan.FromSeconds(14.8);
            Assert.IsFalse(glow.IsFinished);
            Assert.IsTrue(glow.IsGlowing);

            now = Start + TimeSpan.FromSeconds(14.9);
            Assert.IsTrue(glow.IsFinished);
            Assert.IsFalse(glow.IsGlowing);
        }

        #endregion

        #region Clock (DYN-11005)

        [Test]
        [Category("UnitTests")]
        public void NewGlowWaitsAndItsClockDoesNotRun()
        {
            var glow = AIHaloGlow.Create(1.0, true);

            now += TimeSpan.FromMinutes(10);

            Assert.IsTrue(glow.IsWaiting);
            Assert.IsFalse(glow.IsStarted);
            Assert.IsFalse(glow.IsGlowing);
            Assert.IsFalse(glow.IsFinished, "A glow never seen has not faded out.");
            Assert.AreEqual(TimeSpan.Zero, glow.Elapsed);
        }

        [Test]
        [Category("UnitTests")]
        public void ClockStartsWhenTheNodeComesOnScreen()
        {
            var glow = AIHaloGlow.Create(1.0, true);
            now += TimeSpan.FromMinutes(10);

            Assert.IsTrue(glow.Begin(TimeSpan.Zero));
            now += TimeSpan.FromSeconds(3);

            Assert.IsTrue(glow.IsGlowing);
            Assert.AreEqual(TimeSpan.FromSeconds(3), glow.Elapsed);
            Assert.IsFalse(glow.Begin(TimeSpan.Zero), "Only the first time on screen starts the clock.");
            Assert.AreEqual(TimeSpan.FromSeconds(3), glow.Elapsed);
        }

        [Test]
        [Category("UnitTests")]
        public void GlowsAreOrderedByTouch()
        {
            var first = AIHaloGlow.Create(1.0, true);
            var second = AIHaloGlow.Create(1.0, true);

            Assert.Less(first.TouchOrder, second.TouchOrder);
        }

        [Test]
        [Category("UnitTests")]
        public void HeldGlowDoesNotCountTime()
        {
            var glow = NewGlow();
            now += TimeSpan.FromSeconds(2);

            Assert.IsTrue(glow.Hold());
            now += TimeSpan.FromSeconds(30);
            Assert.AreEqual(TimeSpan.FromSeconds(2), glow.Elapsed);
            Assert.IsTrue(glow.IsGlowing, "A held glow does not fade out.");

            Assert.IsTrue(glow.Release());
            now += TimeSpan.FromSeconds(1);
            Assert.AreEqual(TimeSpan.FromSeconds(3), glow.Elapsed);
        }

        [Test]
        [Category("UnitTests")]
        public void HoldingDuringTheStaggerWaitHoldsTheWaitToo()
        {
            var glow = NewGlow(place: 5);
            now += TimeSpan.FromMilliseconds(200);

            glow.Hold();
            now += TimeSpan.FromSeconds(10);

            Assert.AreEqual(TimeSpan.FromMilliseconds(200), glow.Elapsed, "Still inside its 500 ms wait.");
        }

        [Test]
        [Category("UnitTests")]
        public void GlowThatBeginsHeldRunsOnceReleased()
        {
            var glow = AIHaloGlow.Create(1.0, true);
            glow.Hold();

            glow.Begin(TimeSpan.Zero);
            now += TimeSpan.FromSeconds(5);
            Assert.AreEqual(TimeSpan.Zero, glow.Elapsed);

            glow.Release();
            now += TimeSpan.FromSeconds(1);
            Assert.AreEqual(TimeSpan.FromSeconds(1), glow.Elapsed);
        }

        [Test]
        [Category("UnitTests")]
        public void RetouchMarksARunningGlowForRestartAndKeepsItPlaying()
        {
            var glow = NewGlow();
            var version = glow.Version;
            now += TimeSpan.FromSeconds(8);

            glow.Retouch(1.0, true);

            Assert.IsTrue(glow.IsRestartPending);
            Assert.AreEqual(version, glow.Version, "Nothing changes on screen until the check.");
            Assert.AreEqual(TimeSpan.FromSeconds(8), glow.Elapsed);
            Assert.IsTrue(glow.IsGlowing);
        }

        [Test]
        [Category("UnitTests")]
        public void RestartStartsAgainWithoutDelay()
        {
            var glow = NewGlow(NodeViewModel.AIHaloMovedOpacity, breathes: false, place: 3);
            var version = glow.Version;
            now += TimeSpan.FromSeconds(8);

            glow.Retouch(1.0, true);
            Assert.IsTrue(glow.Restart());

            Assert.IsFalse(glow.IsRestartPending);
            Assert.AreEqual(TimeSpan.Zero, glow.Elapsed);
            Assert.AreEqual(TimeSpan.Zero, glow.Delay);
            Assert.AreNotEqual(version, glow.Version);
            Assert.AreEqual(1.0, glow.Strength);
            Assert.IsTrue(glow.Breathes);

            now += TimeSpan.FromSeconds(1);
            Assert.AreEqual(TimeSpan.FromSeconds(1), glow.Elapsed);
        }

        [Test]
        [Category("UnitTests")]
        public void RetouchWhileHeldRestartsHeld()
        {
            var glow = NewGlow();
            now += TimeSpan.FromSeconds(8);
            glow.Hold();

            glow.Retouch(1.0, true);
            glow.Restart();
            now += TimeSpan.FromSeconds(5);

            Assert.AreEqual(TimeSpan.Zero, glow.Elapsed, "Stays at the start until the mouse leaves.");
        }

        [Test]
        [Category("UnitTests")]
        public void WaitAgainSendsTheGlowBackToWaiting()
        {
            var glow = NewGlow(place: 2);
            var version = glow.Version;
            now += TimeSpan.FromSeconds(5);
            glow.Retouch(1.0, true);

            Assert.IsTrue(glow.WaitAgain());

            Assert.IsTrue(glow.IsWaiting);
            Assert.IsFalse(glow.IsStarted);
            Assert.IsFalse(glow.IsRestartPending);
            Assert.AreNotEqual(version, glow.Version);
            now += TimeSpan.FromMinutes(1);
            Assert.AreEqual(TimeSpan.Zero, glow.Elapsed, "The clock does not run while waiting.");

            // Back on screen, it starts like a new glow, with its place in the stagger
            Assert.IsTrue(glow.Begin(AIHaloGlow.StaggerDelay(1)));
            Assert.AreEqual(TimeSpan.FromMilliseconds(100), glow.Delay);
            Assert.IsTrue(glow.IsGlowing);
        }

        [Test]
        [Category("UnitTests")]
        public void RestartAndWaitAgainNeedATouchFirst()
        {
            var glow = NewGlow();

            Assert.IsFalse(glow.Restart());
            Assert.IsFalse(glow.WaitAgain());
            Assert.IsTrue(glow.IsStarted);
        }

        [Test]
        [Category("UnitTests")]
        public void PendingRestartIsNotFinishedWhenTheOldTimelineEnds()
        {
            var glow = NewGlow();
            now += TimeSpan.FromSeconds(14);
            glow.Retouch(1.0, true);

            now += TimeSpan.FromSeconds(5);

            Assert.IsFalse(glow.IsFinished, "The newer change still has to be seen.");
            Assert.IsFalse(glow.IsGlowing, "The old timeline has ended, so no halo meanwhile.");
            Assert.IsTrue(glow.Restart());
            Assert.IsTrue(glow.IsGlowing);
        }

        [Test]
        [Category("UnitTests")]
        public void DismissClearsAPendingRestart()
        {
            var glow = NewGlow();
            now += TimeSpan.FromSeconds(3);
            glow.Retouch(1.0, true);

            Assert.IsTrue(glow.Dismiss());

            Assert.IsFalse(glow.IsRestartPending);
            Assert.IsFalse(glow.Restart());
            Assert.IsFalse(glow.WaitAgain());
            Assert.IsTrue(glow.IsFinished);
        }

        [Test]
        [Category("UnitTests")]
        public void StartOpacityIsKeptForEachVersion()
        {
            var glow = NewGlow();
            Assert.AreEqual(0.0, glow.StartOpacityFor(glow.Version, 0), Tolerance, "A new halo starts from zero");

            now += TimeSpan.FromSeconds(8);
            glow.Retouch(1.0, true);
            glow.Restart();
            Assert.AreEqual(0.7, glow.StartOpacityFor(glow.Version, 0.7), Tolerance, "The first view keeps its current opacity");
            Assert.AreEqual(0.7, glow.StartOpacityFor(glow.Version, 0), Tolerance, "A rebuilt view gets the same value");

            glow.Retouch(1.0, true);
            glow.WaitAgain();
            Assert.AreEqual(0.0, glow.StartOpacityFor(glow.Version, 0), Tolerance, "A new version starts afresh");
        }

        [Test]
        [Category("UnitTests")]
        public void RetouchWhileWaitingKeepsWaitingAtTheNewStrength()
        {
            var glow = AIHaloGlow.Create(NodeViewModel.AIHaloMovedOpacity, false);
            var version = glow.Version;

            glow.Retouch(1.0, true);

            Assert.IsTrue(glow.IsWaiting);
            Assert.AreEqual(version, glow.Version);
            Assert.AreEqual(1.0, glow.Strength);
            Assert.IsTrue(glow.Breathes);
        }

        [Test]
        [Category("UnitTests")]
        public void HoldAndReleaseDoNotChangeTheVersion()
        {
            var glow = NewGlow();
            var version = glow.Version;

            glow.Hold();
            glow.Release();

            Assert.AreEqual(version, glow.Version);
        }

        [Test]
        [Category("UnitTests")]
        public void DismissEndsAGlowingGlowOnly()
        {
            var waiting = AIHaloGlow.Create(1.0, true);
            Assert.IsFalse(waiting.Dismiss(), "Nothing to end before the node is seen.");

            var glowing = NewGlow();
            now += TimeSpan.FromSeconds(3);
            Assert.IsTrue(glowing.Dismiss());
            Assert.IsTrue(glowing.IsDismissed);
            Assert.IsTrue(glowing.IsFinished);
            Assert.IsFalse(glowing.IsGlowing);
            Assert.IsFalse(glowing.Dismiss(), "Already dismissed.");

            var fadedOut = NewGlow();
            now += AIHaloGlow.FadeOutEnd;
            Assert.IsFalse(fadedOut.Dismiss(), "Only the badge is left.");
        }

        [Test]
        [Category("UnitTests")]
        public void DismissedGlowIgnoresRetouchAndRelease()
        {
            var glow = NewGlow();
            glow.Hold();
            glow.Dismiss();

            glow.Retouch(1.0, true);
            glow.Release();
            now += TimeSpan.FromSeconds(1);

            Assert.IsTrue(glow.IsFinished);
            Assert.IsFalse(glow.Begin(TimeSpan.Zero));
        }

        #endregion
    }
}
