using System.IO;
using Dynamo.Models;
using NUnit.Framework;

namespace Dynamo.Tests.Engine
{
    [TestFixture]
    class LiveRunnerServicesTest : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will execute the RuntimeMirror GetMirror(string var, bool verboseLogging) method.
        /// </summary>

        [Test]
        [Category("UnitTests")]
        public void LiveRunnerServicesGetMirrorTest()
        {
            //Arrange
            var openPath = Path.Combine(TestDirectory, @"core\DetailedPreviewMargin_Test.dyn");
            RunModel(openPath);
            //With this flag set to true we reach the section for logging.
            CurrentDynamoModel.EngineController.VerboseLogging = true;

            //Act            
            //This the id of a CoreNodeModels.Input.StringInput node inside the .dyn file
            var node = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("10d3d247-5378-4186-8f2d-286db0cc61a5");
            var runtimeMirror = CurrentDynamoModel.EngineController.GetMirror(node.AstIdentifierBase);

            //Assert
            //Verify that the runtimeMirror was created correctly
            Assert.IsNotNull(runtimeMirror);

        }

        /// <summary>
        /// This test method will execute the UpdateGraph method from the LiveRunnerServices class
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void LiveRunnerServicesUpdateGraph()
        {
            //Arrange
            var openPath = Path.Combine(TestDirectory, @"core\DetailedPreviewMargin_Test.dyn");

            RunModel(openPath);
            //With this flag set to true we reach the section for logging.
            CurrentDynamoModel.EngineController.VerboseLogging = true;

            //We cannot set the VerboseLogging flag before the Model is created then in order to execute the UpdateGraph()
            //we need to call the OpenFileCommand again with the VerboseLogging already set to true
            var openPath2 = Path.Combine(TestDirectory, @"core\Angle.dyn");

            //Act
            //Internally this will execute the UpdateGraph() method
            var commandFile = new DynamoModel.OpenFileCommand(openPath2);
            CurrentDynamoModel.ExecuteCommand(commandFile);


            //Assert
            //Verify that the command was created successfully
            Assert.IsNotNull(commandFile);

        }
        
    }
}
