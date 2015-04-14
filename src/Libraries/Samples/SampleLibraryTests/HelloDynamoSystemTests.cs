using SystemTestServices;

using Autodesk.DesignScript.Runtime;

using Dynamo.Nodes;
using Dynamo.Tests;

using NUnit.Framework;

namespace SampleLibraryTests
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
    /// System tests have dependencies on Dynamo core dlls. In 
    /// order for these tests to work, your test dll needs to be 
    /// located in the Dynamo core directory. Set your build
    /// output path to your Dynamo core directory, or add a copy step that
    /// moves the output from this project to the Dynamo core directory.
    /// </summary>
    [TestFixture]
    [IsVisibleInDynamoLibrary(false)]
    public class HelloDynamoSystemTests : SystemTestBase
    {
        // The RequiresSTA attribute is required by
        // NUNit to run tests that use the UI.
        [Test, RequiresSTA]
        public void HelloDynamoTest()
        {
            // HelloWorldSystemTest.dyn is a test .dyn file which
            // should be copied to the output directory, so it's available
            // for testing. You can also change this path to anywhere you
            // would like to get your test file from, but it has to be
            // a relative path from the dynamo core directory (the working directory).

            OpenAndRunDynamoDefinition(@".\HelloDynamoSystemTest.dyn");

            // Ensure that the graph opened without any "dummy nodes".
            // Dummy nodes would appear if your graph had a node that
            // could not be found in the library.

            AssertNoDummyNodes();

            // Get the first node of a certain type from the workspace.
            // DSFunction nodes are a type of node which wrap built-in functions
            // like the '+' function, which is what we're looking for.

            var addNode = Model.CurrentWorkspace.FirstNodeFromWorkspace<DSFunction>();
            Assert.NotNull(addNode);

            // Ensure that the value of that node after evaluation is
            // the value that we are looking for.
            Assert.AreEqual(addNode.GetValue(0, Model.EngineController).Data, 42);
        }
    }
}
