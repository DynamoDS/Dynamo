using System;
using System.Collections.Generic;
using System.Xml;
using NUnit.Framework;
using static Dynamo.Models.DynamoModel;

namespace Dynamo.Tests.ModelsTest
{
    [TestFixture]
    class CreateCustomNodeCommandTest
    {
        /// <summary>
        /// This test method will execute the next methods from the CreateCustomNodeCommand class
        ///  public CreateCustomNodeCommand(string nodeId, string name,string category, string description, bool makeCurrent)
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void CreateCustomNodeCommand_Constructor()
        {
            //Arrange
            var guid = Guid.NewGuid();

            //Act
            var command1 = new CreateCustomNodeCommand(guid.ToString(), "nameNode", "categoryNode","descriptionNode",true);

            //Assert
            //Checking that the command was created successully
            Assert.IsNotNull(command1);
            Assert.AreEqual(command1.Name, "nameNode");
            Assert.AreEqual(command1.Category, "categoryNode");
            Assert.AreEqual(command1.Description, "descriptionNode");
        }

        /// <summary>
        /// This test method will execute the next methods from the CreateAnnotationCommand class
        /// public CreateAnnotationCommand(IEnumerable<Guid> annotationId, string annotationText,double x, double y, bool defaultPosition)
        /// protected override void SerializeCore(XmlElement element)
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void CreateAnnotationCommand_Constructor()
        {
            //Arrange
            XmlDocument xmlDocument = new XmlDocument();
            var guid = Guid.NewGuid();
            IEnumerable<Guid> annotationGuid = new[] { guid };

            //Act
            var command1 = new CreateAnnotationCommand(annotationGuid, string.Empty, 100,100,true);
            var serializedCommand = command1.Serialize(xmlDocument);

            //Assert
            Assert.IsNotNull(command1);
            Assert.AreEqual(command1.ModelGuid.ToString(),guid.ToString());
            Assert.IsNotNull(serializedCommand);
        }
    }
}
