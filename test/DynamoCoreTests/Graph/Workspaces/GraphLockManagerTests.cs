using System;
using System.IO;
using Dynamo.Graph.Workspaces.Locking;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class GraphLockManagerTests : DynamoModelTestBase
    {
        [Test]
        [Category("UnitTests")]
        public void WhenNoLockExistsThenAcquireCreatesOwnedLockAndReleaseDeletesIt()
        {
            //Arrange - creates a graph in the test temp folder
            var graphPath = CreateGraphFile("unlocked.dyn");
            var lockPath = GraphLockFile.GetLockFilePath(graphPath);

            using (var manager = CreateManager())
            {
                //Act
                var result = manager.AcquireLock(graphPath, true);

                //Assert that when the graph is opened a lock file is created
                Assert.AreEqual(GraphLockOutcome.Opened, result.Conflict);
                Assert.IsTrue(result.ShouldOpen);
                Assert.AreEqual(Path.GetFullPath(graphPath), result.GraphPath);
                Assert.IsTrue(File.Exists(lockPath));

                var ownedLock = ReadLockInfo(lockPath);
                Assert.AreEqual(Path.GetFullPath(graphPath), ownedLock.GraphPath);

                //Act - close the graph
                manager.CompleteOpen(graphPath, true);
                manager.Release(graphPath);
            }

            //Assert that the lock file is deleted
            Assert.IsFalse(File.Exists(lockPath));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenLiveLockExistsAndUserCancelsThenAcquireReturnsCancelled()
        {
            //Arrange - create a live lock owned by another Dynamo instance so opening the graph
            // must go through the user prompt instead of silently taking the lock
            var graphPath = CreateGraphFile("locked.dyn");
            var lockPath = GraphLockFile.GetLockFilePath(graphPath);
            var existingLock = CreateForeignLockInfo(graphPath, DateTime.UtcNow);
            Assert.IsTrue(GraphLockFile.TryCreateNewLockFile(lockPath, existingLock));

            // Simulate the user clicking Cancel in the graph-lock prompt
            var prompt = new TestGraphLockUserPrompt(GraphLockUserResponse.Cancel());

            using (var manager = CreateManager(prompt))
            {
                //Act
                var result = manager.AcquireLock(graphPath, true);

                //Assert that the graph will not open
                Assert.AreEqual(GraphLockOutcome.Cancelled, result.Conflict);
                Assert.IsFalse(result.ShouldOpen);
                Assert.AreEqual(existingLock.SessionId, result.ExistingLock.SessionId);
                Assert.AreEqual(1, prompt.CallCount);
                Assert.AreEqual(Path.GetFullPath(graphPath), prompt.GraphPath);
                Assert.AreEqual(existingLock.SessionId, prompt.ExistingLock.SessionId);

                manager.CompleteOpen(graphPath, false);
            }

            //Assert that the original lock still belongs to the other Dynamo instance
            Assert.AreEqual(existingLock.SessionId, ReadLockInfo(lockPath).SessionId);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenLiveLockExistsAndUserSavesCopyThenAcquireReturnsCopyPath()
        {
            //Arrange - write a live lock for the source graph so the Save As branch of the
            // conflict prompt is the only path that can open the graph
            var graphPath = CreateGraphFile("locked-copy-source.dyn", "source graph");
            var copyPath = Path.Combine(TempFolder, "locked-copy-target.dyn");
            var sourceLockPath = GraphLockFile.GetLockFilePath(graphPath);
            var copyLockPath = GraphLockFile.GetLockFilePath(copyPath);
            var existingLock = CreateForeignLockInfo(graphPath, DateTime.UtcNow);
            Assert.IsTrue(GraphLockFile.TryCreateNewLockFile(sourceLockPath, existingLock));

            // Simulate the user clicking Save As and choosing a copy path.
            var prompt = new TestGraphLockUserPrompt(GraphLockUserResponse.SaveAs(copyPath));

            using (var manager = CreateManager(prompt))
            {
                //Act
                var result = manager.AcquireLock(graphPath, true);

                //Assert
                Assert.AreEqual(GraphLockOutcome.Opened, result.Conflict);
                Assert.IsTrue(result.ShouldOpen);
                Assert.AreEqual(Path.GetFullPath(copyPath), result.GraphPath);
                Assert.AreEqual(1, prompt.CallCount);
                Assert.IsTrue(File.Exists(copyPath));
                Assert.AreEqual(File.ReadAllText(graphPath), File.ReadAllText(copyPath));
                Assert.AreEqual(existingLock.SessionId, ReadLockInfo(sourceLockPath).SessionId);

                // Assert that the copy gets its own lock
                var copyLock = ReadLockInfo(copyLockPath);
                Assert.AreEqual(Path.GetFullPath(copyPath), copyLock.GraphPath);
                Assert.That(copyLock.SessionId, Is.Not.EqualTo(existingLock.SessionId));

                //Act
                manager.CompleteOpen(copyPath, true);
                manager.Release(copyPath);
            }

            //Assert
            Assert.IsTrue(File.Exists(sourceLockPath));
            Assert.IsFalse(File.Exists(copyLockPath));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenExistingLockIsStaleThenAcquireReplacesLockOwner()
        {
            //Arrange - create an expired lock so the manager should take ownership without
            // asking the user to cancel or save a copy.
            var graphPath = CreateGraphFile("stale-lock.dyn");
            var lockPath = GraphLockFile.GetLockFilePath(graphPath);
            var staleLock = CreateForeignLockInfo(graphPath, DateTime.UtcNow.AddSeconds(-10));
            Assert.IsTrue(GraphLockFile.TryCreateNewLockFile(lockPath, staleLock));

            using (var manager = CreateManager(heartbeatMilliseconds: 1000))
            {
                //Act
                var result = manager.AcquireLock(graphPath, true);

                //Assert that Dynamo can open the graph
                Assert.AreEqual(GraphLockOutcome.Opened, result.Conflict);
                Assert.IsTrue(result.ShouldOpen);
                Assert.AreEqual(Path.GetFullPath(graphPath), result.GraphPath);

                var acquiredLock = ReadLockInfo(lockPath);
                Assert.AreEqual(Path.GetFullPath(graphPath), acquiredLock.GraphPath);
                Assert.That(acquiredLock.SessionId, Is.Not.EqualTo(staleLock.SessionId));
                Assert.Greater(acquiredLock.LastHeartbeatUtc, staleLock.LastHeartbeatUtc);

                //Act
                manager.CompleteOpen(graphPath, true);
                manager.Release(graphPath);
            }

            //Assert that the lock is deleted
            Assert.IsFalse(File.Exists(lockPath));
        }

        private GraphLockManager CreateManager(IGraphLockUserPrompt prompt = null, int heartbeatMilliseconds = GraphLockManager.DefaultHeartbeatMilliseconds)
        {
            return new GraphLockManager(CurrentDynamoModel, prompt, heartbeatMilliseconds, forceEnable: true);
        }

        private string CreateGraphFile(string fileName, string contents = "graph")
        {
            var graphPath = Path.Combine(TempFolder, fileName);
            File.WriteAllText(graphPath, contents);
            return graphPath;
        }

        // Creates lock metadata that cannot be mistaken for a lock from this test process
        private static GraphLockInfo CreateForeignLockInfo(string graphPath, DateTime lastHeartbeatUtc)
        {
            return new GraphLockInfo
            {
                SessionId = Guid.NewGuid(),
                GraphPath = Path.GetFullPath(graphPath),
                MachineName = "RemoteMachine-" + Guid.NewGuid(),
                ProcessId = 1234,
                ProcessStartUtc = DateTime.UtcNow.AddHours(-1),
                LastHeartbeatUtc = lastHeartbeatUtc
            };
        }

        private static GraphLockInfo ReadLockInfo(string lockPath)
        {
            Assert.IsTrue(GraphLockFile.TryRead(lockPath, out var lockInfo));
            return lockInfo;
        }

        private sealed class TestGraphLockUserPrompt : IGraphLockUserPrompt
        {
            private readonly GraphLockUserResponse response;

            internal TestGraphLockUserPrompt(GraphLockUserResponse response)
            {
                this.response = response;
            }

            internal int CallCount { get; private set; }

            internal string GraphPath { get; private set; }

            internal GraphLockInfo ExistingLock { get; private set; }

            public GraphLockUserResponse AskUser(string graphPath, GraphLockInfo existingLock)
            {
                CallCount++;
                GraphPath = graphPath;
                ExistingLock = existingLock;
                return response;
            }
        }
    }
}
