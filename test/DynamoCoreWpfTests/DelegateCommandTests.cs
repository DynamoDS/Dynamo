using System;
using System.Threading;
using System.Threading.Tasks;
using Dynamo.UI.Commands;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    /// <summary>
    /// Tests for DelegateCommand threading behavior.
    /// Regression tests for DYN-10409: RaiseCanExecuteChanged called from a background thread
    /// (e.g. the Dynamo scheduler thread) caused SynchronizationLockException in WPF's
    /// CommandManager.FindCommandBinding.
    /// </summary>
    [TestFixture]
    public class DelegateCommandTests : DynamoTestUIBase
    {
        /// <summary>
        /// When RaiseCanExecuteChanged is called from a background thread, the CanExecuteChanged
        /// handler must be invoked on the UI thread, not the calling thread.
        /// Before the fix this test fails: the handler fires on the background thread, which
        /// causes WPF's CommandManager to crash with SynchronizationLockException.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenRaiseCanExecuteChangedCalledFromBackgroundThread_HandlerInvokedOnUIThread()
        {
            var command = new DelegateCommand(_ => { });
            int uiThreadId = Environment.CurrentManagedThreadId;
            int handlerThreadId = -1;

            command.CanExecuteChanged += (s, e) => handlerThreadId = Environment.CurrentManagedThreadId;

            // Fire from a background thread — the pre-fix code calls the handler on this thread,
            // which is a WPF threading violation. The fix marshals via Dispatcher.BeginInvoke.
            Task.Run(() => command.RaiseCanExecuteChanged()).Wait();

            // Pump the dispatcher so any BeginInvoke posted by the fix has a chance to run.
            DispatcherUtil.DoEventsLoop(() => handlerThreadId != -1, timeoutSeconds: 5);

            Assert.AreNotEqual(-1, handlerThreadId, "CanExecuteChanged handler was never invoked");
            Assert.AreEqual(uiThreadId, handlerThreadId,
                "CanExecuteChanged must be invoked on the UI thread when raised from a background thread. " +
                "Calling from a non-UI thread causes SynchronizationLockException in WPF's CommandManager (DYN-10409).");
        }

        /// <summary>
        /// When RaiseCanExecuteChanged is called from the UI thread it should invoke the handler
        /// synchronously on the same thread (no unnecessary async round-trip).
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenRaiseCanExecuteChangedCalledFromUIThread_HandlerInvokedSynchronously()
        {
            var command = new DelegateCommand(_ => { });
            int uiThreadId = Environment.CurrentManagedThreadId;
            int handlerThreadId = -1;

            command.CanExecuteChanged += (s, e) => handlerThreadId = Environment.CurrentManagedThreadId;

            command.RaiseCanExecuteChanged();

            // No dispatcher pump needed — UI-thread calls should be synchronous.
            Assert.AreEqual(uiThreadId, handlerThreadId,
                "CanExecuteChanged should be invoked synchronously when already on the UI thread");
        }

        /// <summary>
        /// Verify that no exception escapes when RaiseCanExecuteChanged is called concurrently
        /// from multiple background threads.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenRaiseCanExecuteChangedCalledConcurrentlyFromBackgroundThreads_NoException()
        {
            var command = new DelegateCommand(_ => { });
            int invocationCount = 0;

            command.CanExecuteChanged += (s, e) => Interlocked.Increment(ref invocationCount);

            const int threadCount = 10;
            var tasks = new Task[threadCount];
            for (int i = 0; i < threadCount; i++)
                tasks[i] = Task.Run(() => command.RaiseCanExecuteChanged());

            Assert.DoesNotThrow(() => Task.WaitAll(tasks),
                "RaiseCanExecuteChanged must not throw when called from multiple background threads");

            DispatcherUtil.DoEventsLoop(() => invocationCount >= threadCount, timeoutSeconds: 5);
            Assert.AreEqual(threadCount, invocationCount,
                "All CanExecuteChanged invocations should be delivered");
        }
    }
}
