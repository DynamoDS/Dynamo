using System.Windows;
using Dynamo.Controls;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    /// <summary>
    /// Covers DYN-10845: the main window bounds restored on startup must always land
    /// on a connected display with the title bar and caption buttons reachable, even
    /// when the monitor layout changed since the bounds were saved.
    /// </summary>
    [TestFixture]
    [Category("UnitTests")]
    public class DynamoViewWindowBoundsTests
    {
        // A 1920x1080 display with a 48 DIP taskbar at the bottom.
        private static readonly Rect PrimaryWorkArea = new Rect(0, 0, 1920, 1032);

        [Test]
        public void WhenSavedBoundsFitOnScreenThenTheyAreRestoredUnchanged()
        {
            var saved = new Rect(100, 50, 1024, 768);

            var restored = DynamoView.GetRestoredWindowBounds(saved, new[] { PrimaryWorkArea }, PrimaryWorkArea);

            Assert.AreEqual(saved, restored);
        }

        [Test]
        public void WhenSavedMonitorIsDisconnectedThenWindowIsCenteredOnFallbackWorkArea()
        {
            // Saved on a secondary monitor to the right that is no longer connected.
            var saved = new Rect(2200, 100, 1024, 768);

            var restored = DynamoView.GetRestoredWindowBounds(saved, new[] { PrimaryWorkArea }, PrimaryWorkArea);

            Assert.AreEqual(new Rect(448, 132, 1024, 768), restored);
        }

        [Test]
        public void WhenOverlapIsBelowToleranceThenWindowIsCentered()
        {
            // Only a 50 DIP sliver of the window overlaps the primary work area.
            var saved = new Rect(1870, 100, 1024, 768);

            var restored = DynamoView.GetRestoredWindowBounds(saved, new[] { PrimaryWorkArea }, PrimaryWorkArea);

            Assert.AreEqual(new Rect(448, 132, 1024, 768), restored);
        }

        [Test]
        public void WhenSavedSizeIsEmptyThenWindowIsCentered()
        {
            var saved = new Rect(100, 100, 0, 0);

            var restored = DynamoView.GetRestoredWindowBounds(saved, new[] { PrimaryWorkArea }, PrimaryWorkArea);

            Assert.AreEqual(new Rect(448, 132, 1024, 768), restored);
        }

        [Test]
        public void WhenFallbackWorkAreaIsSmallerThanDefaultSizeThenWindowFillsIt()
        {
            var smallWorkArea = new Rect(0, 0, 800, 600);
            var saved = new Rect(5000, 5000, 1024, 768);

            var restored = DynamoView.GetRestoredWindowBounds(saved, new[] { smallWorkArea }, smallWorkArea);

            Assert.AreEqual(smallWorkArea, restored);
        }

        [Test]
        public void WhenStackedMonitorAboveIsDisconnectedThenTitleBarIsMovedOnScreen()
        {
            // The window straddled a secondary monitor at (0,-1080) and the primary
            // monitor; only the primary monitor is connected now.
            var saved = new Rect(0, -300, 1024, 768);

            var restored = DynamoView.GetRestoredWindowBounds(saved, new[] { PrimaryWorkArea }, PrimaryWorkArea);

            Assert.AreEqual(new Rect(0, 0, 1024, 768), restored);
        }

        [Test]
        public void WhenSavedSizeExceedsConnectedDisplaysThenWindowIsShrunkToFit()
        {
            // Maximized on a 3840x2160 display, then launched on a 1920x1080 laptop.
            var saved = new Rect(-8, -8, 3856, 2176);

            var restored = DynamoView.GetRestoredWindowBounds(saved, new[] { PrimaryWorkArea }, PrimaryWorkArea);

            Assert.AreEqual(PrimaryWorkArea, restored);
        }

        [Test]
        public void WhenWindowIsMaximizedOnMonitorWithNegativeCoordinatesThenItStaysOnThatMonitor()
        {
            var leftWorkArea = new Rect(-1920, 0, 1920, 1032);
            var saved = new Rect(-1928, -8, 1936, 1048);

            var restored = DynamoView.GetRestoredWindowBounds(
                saved, new[] { leftWorkArea, PrimaryWorkArea }, PrimaryWorkArea);

            Assert.AreEqual(new Rect(-1920, 0, 1936, 1032), restored);
        }

        [Test]
        public void WhenWindowSpansSideBySideMonitorsThenBoundsAreRestoredUnchanged()
        {
            var rightWorkArea = new Rect(1920, 0, 1920, 1032);
            var saved = new Rect(1000, 100, 1800, 800);

            var restored = DynamoView.GetRestoredWindowBounds(
                saved, new[] { PrimaryWorkArea, rightWorkArea }, PrimaryWorkArea);

            Assert.AreEqual(saved, restored);
        }
    }
}
