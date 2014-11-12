using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SystemTestServices;

using NUnit.Framework;

namespace SamplesLibraryUI
{
    /// <summary>
    /// HelloDynamoSystemTests is a test fixture that contains
    /// system tests for Dynamo. System tests test the entire 
    /// Dynamo system including the UI. They do this by starting
    /// a session of Dynamo, then opening .dyn files, executing them
    /// and comparing the values returned from Dynamo with those
    /// stored on our test class.
    /// </summary>
    [TestFixture]
    public class HelloDynamoSystemTests : SystemTestBase
    {
        [Test]
        public void HelloDynamoTest()
        {
            OpenAndRunDynamoDefinition(@"..\..\HellowWorldSystemTest.dyn");
        }
    }
}
