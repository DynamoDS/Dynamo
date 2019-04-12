using System.Collections.Generic;
using System.IO;
using Dynamo;
using Dynamo.Events;
using Dynamo.Session;
using NUnit.Framework;

namespace IntegrationTests
{
    /// <summary>
    /// Tests to ensure that the execution events are being fired at the right time
    /// </summary>
    [TestFixture]
    public class ExecutionEventsObserver : DynamoModelTestBase
    {
        private static bool preSeen = false;
        private static bool midSeen = false;
        private static bool postSeen = false;


        private static void PreSeen(IExecutionSession session)
        {
            Assert.IsNotNull(session);
            var filepath = "ExecutionEvents.dyn";
            Assert.IsTrue(session.ResolveFilePath(ref filepath));
            Assert.IsTrue(Path.IsPathRooted(filepath));

            filepath = @"xyz\DoNotExist.file";
            Assert.IsFalse(session.ResolveFilePath(ref filepath));
            Assert.AreEqual(@"xyz\DoNotExist.file", filepath);
            preSeen = true;
        }

        private static void PostSeen(IExecutionSession session)
        {
            Assert.IsNotNull(session);
            postSeen = true;
        }

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        public override void Setup()
        {
            ExecutionEvents.GraphPreExecution += PreSeen;
            ExecutionEvents.GraphPostExecution += PostSeen;
            base.Setup(); // Setup DynamoModel in this call.
        }

        public override void Cleanup()
        {
            ExecutionEvents.GraphPreExecution -= PreSeen;
            ExecutionEvents.GraphPostExecution -= PostSeen;

            //Reset
            preSeen = false;
            midSeen = false;
            postSeen = false;

            base.Cleanup();
        }

        [Test]
        public void TestPreAndPostExec()
        {
            //Before state
            Assert.IsFalse(preSeen);
            Assert.IsFalse(midSeen);
            Assert.IsFalse(postSeen);

            //Run the graph
            var examplePath = Path.Combine(TestDirectory, @"System\IntegrationTests\dyns", "ExecutionEvents.dyn");
            OpenModel(examplePath);
            RunCurrentModel();

            //After state
            Assert.IsTrue(preSeen);
            Assert.IsTrue(midSeen);
            Assert.IsTrue(postSeen);

        }

        public static int MidExecNotify(int i)
        {
            //During state
            Assert.IsTrue(preSeen);
            Assert.IsFalse(midSeen);
            Assert.IsFalse(postSeen);

            midSeen = true;

            return i + 1;
        }

    }

    /// <summary>
    /// Class to be loaded into Dynamo to run the tests
    /// </summary>
    public class ExecutionEventsRunTarget
    {
        /// <summary>
        /// This will be called from a node in the graph
        /// </summary>
        /// <returns></returns>
        public static int Exec()
        {
            ExecutionEventsObserver.MidExecNotify(0);
            return 42; 
        }
    }
}
