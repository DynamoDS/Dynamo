using System;
using System.Collections.Generic;
using Dynamo.Events;
using NUnit.Framework;

namespace Dynamo.Tests
{
    /// <summary>
    /// Tests to validate VM method resolution performance optimization for input lists of homogeneous types
    /// This performance optimization occurs in Callsite.cs in ExecWithRISlowPath()
    /// Note these test requires DynamoModelTestBase as the optimization is less apparent 
    /// when running the VM in isolation. 
    /// </summary>
    [TestFixture]
    class HomogeneousListTests : DynamoModelTestBase
    {
        private TimeSpan lastExecutionDuration = new TimeSpan();

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
        public void TestMethodResolutionforHomogeneousListInputs()
        {

            RunModel(@"core\HomogeneousList\HomogeneousInputsValid.dyn");

            AssertPreviewValue("e3aabcc0-3d94-425b-af4e-a3171baaa78a",
                new object[]
                {
                    new object[] { 1, 3, 4 },
                    new object[] { 2, 4, 5 },
                    new object[] { 3, 5, 6 }
                });
        }

        [Test]
        public void TestMethodResolutionforHeterogeneousListInputs()
        {

            RunModel(@"core\HomogeneousList\HeterogeneousInputsValid.dyn");

            AssertPreviewValue("e3aabcc0-3d94-425b-af4e-a3171baaa78a",
                new object[]
                {
                    new object[] { 1.5, 3, 4 },
                    new object[] { 2.5, 4, 5 },
                    new object[] { 3.5, 5, 6 }
                });
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
        }
    }
}
