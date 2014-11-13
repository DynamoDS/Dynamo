using System;
using System.Collections.Generic;
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
    /// 
    /// IMPORTANT! 
    /// In order for these tests to work, your test dll needs to be 
    /// located in the dynamo core directory. Set your build
    /// output path to your Dynamo folder, or add a copy step that
    /// moves the output from this project to the Dynamo core directory.
    /// </summary>
    [TestFixture]
    public class HelloDynamoSystemTests : SystemTestBase
    {
        [Test, RequiresSTA]
        public void HelloDynamoTest()
        {
            // HelloWorldSystemTest.dyn is a test .dyn file which
            // should be copied to the output directory, so it's available
            // for testing. You can also change this path to anywhere you
            // would like to get your test file from, but it has to be
            // a relative path from the dynamo core directory (the working directory).

            OpenAndRunDynamoDefinition(@".\HelloDynamoSystemTest.dyn");
        }
    }
}
