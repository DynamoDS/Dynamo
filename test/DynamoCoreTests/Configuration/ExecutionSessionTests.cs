using System;
using System.Collections.Generic;
using Dynamo.Events;
using NUnit.Framework;

namespace Dynamo.Tests.Configuration
{
    [TestFixture]
    class ExecutionSessionTests : DynamoModelTestBase
    {
        private TimeSpan lastExecutionDuration = new TimeSpan();

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            // Add multiple libraries to better simulate typical Dynamo application usage.
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("DSIronPython.dll");
            libraries.Add("FunctionObject.ds");
            base.GetLibrariesToPreload(libraries);
        }

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            ExecutionEvents.GraphPostExecution += ExecutionEvents_GraphPostExecution;
        }

        [Test]
        [Category("UnitTests")]
        public void TestExecutionSession()
        {
            RunModel(@"core\HomogeneousList\HomogeneousInputsValid.dyn");
            Assert.IsNotNull(lastExecutionDuration);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            ExecutionEvents.GraphPostExecution -= ExecutionEvents_GraphPostExecution;
        }

        private void ExecutionEvents_GraphPostExecution(Session.IExecutionSession session)
        {
            lastExecutionDuration = (TimeSpan)session.GetParameterValue(Session.ParameterKeys.LastExecutionDuration);

            Assert.IsNotNull(session.GetParameterKeys());

            var filepath = "ExecutionEvents.dyn";
            Assert.IsFalse(session.ResolveFilePath(ref filepath));
        }
    }
}
