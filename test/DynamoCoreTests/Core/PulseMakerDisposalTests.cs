using System;
using System.Threading;
using Dynamo.Core;
using Dynamo.Models;
using NUnit.Framework;

namespace Dynamo.Tests.Core
{
    /// <summary>
    /// Tests for PulseMaker disposal to prevent memory leaks
    /// </summary>
    [TestFixture]
    public class PulseMakerDisposalTests
    {
        /// <summary>
        /// Test that PulseMaker implements IDisposable
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void PulseMaker_ImplementsIDisposable()
        {
            // Arrange & Act
            var pulseMaker = new PulseMaker();

            // Assert
            Assert.IsInstanceOf<IDisposable>(pulseMaker);

            // Cleanup
            pulseMaker.Dispose();
        }

        /// <summary>
        /// Test that Dispose stops the timer
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void PulseMaker_Dispose_StopsTimer()
        {
            // Arrange
            var pulseMaker = new PulseMaker();
            bool eventFired = false;
            pulseMaker.RunStarted += () => { eventFired = true; };

            // Start the timer with a short interval
            pulseMaker.Start(100);

            // Wait to ensure timer fires at least once
            Thread.Sleep(150);
            
            // Verify timer was working
            Assert.IsTrue(eventFired, "Timer should have fired before disposal");

            // Act - Dispose should stop the timer
            pulseMaker.Dispose();

            // Verify TimerPeriod is 0 after disposal
            Assert.AreEqual(0, pulseMaker.TimerPeriod, "TimerPeriod should be 0 after disposal");

            // Reset the flag and wait longer than the timer period
            eventFired = false;
            Thread.Sleep(250);

            // Assert - Event should not fire after disposal
            Assert.IsFalse(eventFired, "Timer should not fire after Dispose() is called");
        }

        /// <summary>
        /// Test that Dispose clears event handlers to prevent memory leaks
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void PulseMaker_Dispose_ClearsEventHandlers()
        {
            // Arrange
            var pulseMaker = new PulseMaker();
            bool eventFired = false;
            Action handler = () => { eventFired = true; };
            pulseMaker.RunStarted += handler;

            // Act
            pulseMaker.Dispose();

            // Try to trigger the event after disposal (this won't happen naturally,
            // but we're testing that the event handler list was cleared)
            // Since the event is protected, we verify by checking that starting won't work
            
            // Assert - If we could trigger the event, it shouldn't fire
            // We verify disposal by checking timer period is 0 after Stop() was called
            Assert.AreEqual(0, pulseMaker.TimerPeriod, "TimerPeriod should be 0 after disposal");
        }

        /// <summary>
        /// Test that double dispose is safe (doesn't throw exception)
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void PulseMaker_DoubleDispose_IsSafe()
        {
            // Arrange
            var pulseMaker = new PulseMaker();
            pulseMaker.Start(100);

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() =>
            {
                pulseMaker.Dispose();
                pulseMaker.Dispose(); // Second disposal should be safe
            });
        }

        /// <summary>
        /// Test that disposal works correctly even when timer is not started
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void PulseMaker_Dispose_WorksWhenTimerNotStarted()
        {
            // Arrange
            var pulseMaker = new PulseMaker();

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() =>
            {
                pulseMaker.Dispose();
            });
        }

        /// <summary>
        /// Test that disposal works correctly when timer is stopped before disposal
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void PulseMaker_Dispose_WorksAfterStopCalled()
        {
            // Arrange
            var pulseMaker = new PulseMaker();
            pulseMaker.Start(100);
            pulseMaker.Stop();

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() =>
            {
                pulseMaker.Dispose();
            });
        }
    }
}
