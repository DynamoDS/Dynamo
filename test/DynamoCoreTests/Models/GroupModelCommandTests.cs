using System;
using System.Collections.Generic;
using System.Xml;
using NUnit.Framework;
using static Dynamo.Models.DynamoModel;

namespace Dynamo.Tests.ModelsTest
{
    /// <summary>
    /// This test class contains methods for testing the  UngroupModelCommand and AddModelToGroupCommand clases
    /// </summary>
    [TestFixture]
    class GroupModelCommandTests : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will execute the next constructors from the UngroupModelCommand class
        /// public UngroupModelCommand(string modelGuid)
        /// public UngroupModelCommand(Guid modelGuid)
        /// void SerializeCore(XmlElement element) method
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void UngroupModelCommand_Constructors()
        {
            //Arrange
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            XmlDocument xmlDocument = new XmlDocument();

            //Act
            var command1 = new UngroupModelCommand(guid1.ToString());
            var command2 = new UngroupModelCommand(guid2);
            var serializedCommand = command1.Serialize(xmlDocument);

            //Assert
            //Verify that the guid in the commands created are right
            Assert.IsNotNull(command1);
            Assert.AreEqual(command1.ModelGuid.ToString(), guid1.ToString());
            Assert.IsNotNull(command2);
            Assert.AreEqual(command2.ModelGuid.ToString(), guid2.ToString());
            Assert.IsNotNull(serializedCommand);
        }

        /// <summary>
        /// This test method will execute the next constructors from the AddModelToGroupCommand class
        /// public AddModelToGroupCommand(string modelGuid) 
        /// public AddModelToGroupCommand(Guid modelGuid)
        /// public AddModelToGroupCommand(IEnumerable<Guid> modelGuid)
        /// protected override void ExecuteCore(DynamoModel dynamoModel)
        /// protected override void SerializeCore(XmlElement element)
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void AddModelToGroupCommand_Constructors()
        {
            //Arrange
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var guid3 = Guid.NewGuid();
            IEnumerable<Guid> groupModelGuid = new[] { guid3 };
            XmlDocument xmlDocument = new XmlDocument();
            var xmlElement = xmlDocument.CreateElement("ModelGroupElement");

            //Act
            var command1 = new AddModelToGroupCommand(guid1.ToString());
            var command2 = new AddModelToGroupCommand(guid2);
            var command3 = new AddModelToGroupCommand(groupModelGuid);

            AddModelToGroupCommand.DeserializeCore(xmlElement);
            command1.Serialize(xmlDocument);
            command1.Execute(CurrentDynamoModel);

            //Assert
            //Verify that the guid in the commands created are right
            Assert.IsNotNull(command1);
            Assert.IsNotNull(command2);
            Assert.IsNotNull(command3);
        }
    }
}
