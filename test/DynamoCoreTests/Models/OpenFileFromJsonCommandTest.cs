using System.IO;
using System.Xml;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Utilities;
using NUnit.Framework;
using static Dynamo.Models.DynamoModel;

namespace Dynamo.Tests.ModelsTest
{
    [TestFixture]
    class OpenFileFromJsonCommandTest : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will execute the command OpenFileFromJsonCommand
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void RunOpenFileFromJsonCommandTest()
        {
            //Arrange
            string samplepath = Path.Combine(TestDirectory, @"core\callsite\RebindingSingleDimension.dyn");
            string fileContents = File.ReadAllText(samplepath);
            string jsonGraphId = WorkspaceModel.ComputeGraphIdFromJson(fileContents);
            var openFromJsonCommand = new OpenFileFromJsonCommand(fileContents, true);

            //Act
            CurrentDynamoModel.ExecuteCommand(openFromJsonCommand);

            XmlDocument xmlDocument = new XmlDocument();
            XmlElement elemTest = xmlDocument.CreateElement("XmlFileContents");
            var helper = new XmlElementHelper(elemTest);

            helper.SetAttribute("XmlFileContents", fileContents);

            var deserializedCommand = OpenFileFromJsonCommand.DeserializeCore(elemTest);


            //Assert
            Assert.IsTrue(CurrentDynamoModel.CurrentWorkspace.FromJsonGraphId == jsonGraphId);
            Assert.IsEmpty(CurrentDynamoModel.CurrentWorkspace.FileName);
            Assert.IsTrue(CurrentDynamoModel.CurrentWorkspace.HasUnsavedChanges);
            Assert.IsNotNull(deserializedCommand);
        }
    }
}
