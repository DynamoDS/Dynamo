using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using Dynamo.Tests;

using DynamoServices;

using NUnit.Framework;

using ProtoScript.Runners;

using ProtoTestFx.TD;

namespace IntegrationTests
{
    /// <summary>
    /// Tests to ensure that the execution events are being fired at the right time
    /// </summary>
    [TestFixture]
    public class ExecutionEventsObserver : DSEvaluationViewModelUnitTest
    {
        private static bool preSeen = false;
        private static bool midSeen = false;
        private static bool postSeen = false;


        private static void PreSeen()
        {
            preSeen = true;
        }

        private static void PostSeen()
        {
            postSeen = true;
        }


        [SetUp]
        public static void Setup()
        {
            ExecutionEvents.GraphPreExecution += PreSeen;
            ExecutionEvents.GraphPostExecution += PostSeen;

        }

        [TearDown]
        public static void Cleanup()
        {
            ExecutionEvents.GraphPreExecution -= PreSeen;
            ExecutionEvents.GraphPostExecution -= PostSeen;

            //Reset
            preSeen = false;
            midSeen = false;
            postSeen = false;
        }

        [Test]
        public void TestPreAndPostExec()
        {
            //Before state
            Assert.IsFalse(preSeen);
            Assert.IsFalse(midSeen);
            Assert.IsFalse(postSeen);

            //Run the graph
            var model = ViewModel.Model;
            var examplePath = Path.Combine(TestDirectory, @"System\IntegrationTests\dyns", "ExecutionEvents.dyn");
            ViewModel.OpenCommand.Execute(examplePath);
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
