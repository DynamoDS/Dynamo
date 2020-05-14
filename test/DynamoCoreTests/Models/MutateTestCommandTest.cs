using System.IO;
using System.Xml;
using Dynamo.Models;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests.ModelsTest
{
    [TestFixture]
    class MutateTestCommandTest : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will execute the next methods from the MutateTestCommand class:
        /// void ExecuteCore(DynamoModel dynamoModel)
        /// void SerializeCore(XmlElement element)
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void MutateTestCommand_DeserializeCoreTest()
        {
            //Arrange
            var command = new DynamoModel.MutateTestCommand();
            XmlDocument xmlDocument = new XmlDocument();

            //Act
            CurrentDynamoModel.ExecuteCommand(command);           
            XmlElement elemTest = xmlDocument.CreateElement("TestCommand");
            var xmlElement = command.Serialize(xmlDocument);

            //Assert
            Assert.IsNotNull(xmlElement);
        }
    }
}
