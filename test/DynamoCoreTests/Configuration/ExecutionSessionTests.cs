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
        private IEnumerable<string> packagePaths;

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            // Add multiple libraries to better simulate typical Dynamo application usage.
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("DSCPython.dll");
            libraries.Add("FunctionObject.ds");
            base.GetLibrariesToPreload(libraries);
        }
#if NET6_0_OR_GREATER
        [OneTimeSetUp]
#elif NETFRAMEWORK
        [TestFixtureSetUp]
#endif
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

        [Test]
        [Category("UnitTests")]
        public void TestExecutionSessionPackagePaths()
        {
            ExecutionEvents.GraphPreExecution += ExecutionEvents_GraphPreExecution;
            RunModel(@"core\HomogeneousList\HomogeneousInputsValid.dyn");
            Assert.IsNotEmpty(packagePaths, "packgePaths was empty");
            ExecutionEvents.GraphPreExecution -= ExecutionEvents_GraphPreExecution;
        }

        private void ExecutionEvents_GraphPreExecution(Session.IExecutionSession session)
        {
            packagePaths = ExecutionEvents.ActiveSession.GetParameterValue(Session.ParameterKeys.PackagePaths) as IEnumerable<string>;

        }
        
#if NET6_0_OR_GREATER
        [OneTimeTearDown]
#elif NETFRAMEWORK
        [TestFixtureTearDown]
#endif
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
