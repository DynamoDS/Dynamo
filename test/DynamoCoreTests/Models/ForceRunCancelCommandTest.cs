using System.IO;
using System.Xml;
using Dynamo.Models;
using Dynamo.Utilities;
using NUnit.Framework;
using static Dynamo.Models.DynamoModel;

namespace Dynamo.Tests.ModelsTest
{
    [TestFixture]
    class ForceRunCancelCommandTest : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will execute the next methods of the ForceRunCancelCommand
        /// public RunCancelCommand(bool showErrors, bool cancelRun)
        /// void ExecuteCore(DynamoModel dynamoModel)
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void ForceRunCancelCommand_DeserializeCoreTest()
        {
            //Arrange
            string wspath = Path.Combine(TestDirectory, @"core\callsite\RebindingSingleDimension.dyn");
            var fileCommand = new DynamoModel.OpenFileCommand(wspath);

            var runCancelCommand = new DynamoModel.ForceRunCancelCommand(false, false);
            CurrentDynamoModel.ExecuteCommand(runCancelCommand);

            XmlDocument xmlDocument = new XmlDocument();
            XmlElement elemTest = xmlDocument.CreateElement("TestCommand");

            //Act
            var helper = new XmlElementHelper(elemTest);
            //DeserializeCore method is looking for the attributes ShowErrors and CancelRun, then we need to set them up before calling the method.
            helper.SetAttribute("ShowErrors", true);
            helper.SetAttribute("CancelRun", true);

            var deserializedCommand = ForceRunCancelCommand.DeserializeCore(elemTest);

            //Assert
            Assert.IsNotNull(deserializedCommand);
        }
    }
}
